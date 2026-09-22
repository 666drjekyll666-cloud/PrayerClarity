using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;

namespace PrayerClarityResearch
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(RebalancedPluginGuid, BepInDependency.DependencyFlags.HardDependency)]
    public sealed class ReposeGoldSelfTestPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "nikich.graveyardkeeper.prayerclarity.reposegoldselftest";
        public const string RebalancedPluginGuid = "nikich.graveyardkeeper.prayerclarity.rebalanced";
        public const string PluginName = "PrayerClarity Repose Gold Self-Test";
        public const string PluginVersion = "0.1.1";

        private const int FixtureTierMin = 2;
        private const int FixtureTierMax = 4;
        private const int Samples = 4;

        private static readonly BindingFlags AnyStatic =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        private static readonly BindingFlags AnyInstance =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        private static ManualLogSource _log;
        private static Action<string, float?> _addBuff;
        private static Action<string> _removeBuff;
        private static MethodInfo _findBuffById;
        private static MethodInfo _generateBody;
        private static FieldInfo _pendingModeField;

        private static bool _captureGeneration;
        private static object _lastBodyDefinition;
        private static object _catalogSeenDuringGeneration;

        private Rect _windowRect = new Rect(24f, 24f, 670f, 260f);
        private bool _visible;
        private string _status =
            "F1 opens this window. The button runs the full Gold Repose generation self-test.";

        private sealed class CatalogStats
        {
            internal int Count;
            internal int BestTier = int.MinValue;
            internal int MinScore = int.MaxValue;
            internal int MaxScore = int.MinValue;
            internal int MaxScoreCount;
        }

        private sealed class TestSession
        {
            internal object Player;
            internal object Save;
            internal object Balance;
            internal object OriginalCatalog;
            internal CatalogStats Fixture;
            internal bool CanonicalFixture;
            internal float BodyMinBefore;
            internal float BodyMaxBefore;
            internal float AddMinBefore;
            internal float AddMaxBefore;
            internal readonly Dictionary<string, int> SelectedIds =
                new Dictionary<string, int>(StringComparer.Ordinal);
            internal int CompletedSamples;
            internal float NextSampleAt;
        }

        private TestSession _session;

        private void Awake()
        {
            _log = Logger;
            ResolveBuffApi();
            ResolveProductionSeams();
            PatchBodyDefinitionCapture();

            Logger.LogInfo(
                PluginName + " " + PluginVersion +
                " loaded. Press F1 and run the one-button test. " +
                "This is a research-only companion for PrayerClarity: Rebalanced 0.2.15.");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
                _visible = !_visible;

            if (_visible && Input.GetKeyDown(KeyCode.Escape))
                _visible = false;

            TestSession session = _session;
            if (session == null || Time.realtimeSinceStartup < session.NextSampleAt)
                return;

            RunNextSample(session);
        }

        private void OnDestroy()
        {
            TestSession session = _session;
            if (session == null) return;

            _log?.LogWarning(
                "REPOSE_GOLD_SELFTEST interrupted by plugin destruction; running cleanup.");
            CleanupSession(session, "INTERRUPTED");
            _session = null;
        }

        private void OnGUI()
        {
            if (!_visible) return;

            _windowRect = GUILayout.Window(
                736219,
                _windowRect,
                DrawWindow,
                "PrayerClarity: Repose Gold Self-Test 0.1.1");
        }

        private void DrawWindow(int id)
        {
            GUILayout.Label("Research-only. F1 toggles. Escape closes.");
            GUILayout.Label(
                "One click: native Repose buff -> production Gold seam -> real GameSave.GenerateBody -> verify -> cleanup.");
            GUILayout.Label(
                "The test does NOT wait for Sunday or spawn a physical Donkey/corpse in the world.");

            GUILayout.Space(10f);

            if (_session == null)
            {
                if (GUILayout.Button("Run Gold Repose self-test (4 paced GenerateBody calls)", GUILayout.Height(34f)))
                    StartGoldSelfTest();
            }
            else
            {
                GUILayout.Label(
                    "Running: " + _session.CompletedSamples + "/" + Samples +
                    " completed. One native generation is executed per paced step.");
            }

            GUILayout.Space(10f);
            GUILayout.Label("Status: " + _status);

            GUI.DragWindow(new Rect(0f, 0f, 10000f, 24f));
        }

        private void StartGoldSelfTest()
        {
            if (_session != null)
            {
                _status = "REFUSED: a self-test is already running.";
                return;
            }

            if (IsActive("buff_skull"))
            {
                _status =
                    "REFUSED: buff_skull is already active. Let/remove the real Repose effect first so the self-test cannot overwrite it.";
                _log?.LogWarning("REPOSE_GOLD_SELFTEST_REFUSED reason=preexisting_buff_skull");
                return;
            }

            object player = GetPlayer();
            object save = GetSave();
            object balance = GetStatic(FindType("GameBalance"), "me");
            object originalCatalog = Get(balance, "bodies_data");

            if (player == null || save == null || balance == null || originalCatalog == null)
            {
                _status = "FAIL: live player/save/GameBalance state is unavailable.";
                _log?.LogError("REPOSE_GOLD_SELFTEST_FAIL reason=live_state_unavailable");
                return;
            }

            try
            {
                CatalogStats fixture = AnalyzeCatalog(
                    originalCatalog,
                    FixtureTierMin,
                    FixtureTierMax);

                if (fixture.Count <= 0 || fixture.BestTier == int.MinValue)
                    throw new InvalidOperationException(
                        "Control range contains no ordinary body definitions.");

                TestSession session = new TestSession
                {
                    Player = player,
                    Save = save,
                    Balance = balance,
                    OriginalCatalog = originalCatalog,
                    Fixture = fixture,
                    CanonicalFixture =
                        fixture.Count == 33 &&
                        fixture.BestTier == 3 &&
                        fixture.MaxScore == 10 &&
                        fixture.MaxScoreCount == 9,
                    BodyMinBefore = GetPlayerParam(player, "body_min", 0f),
                    BodyMaxBefore = GetPlayerParam(player, "body_max", 0f),
                    AddMinBefore = GetPlayerParam(player, "add_body_min", 0f),
                    AddMaxBefore = GetPlayerParam(player, "add_body_max", 0f),
                    CompletedSamples = 0,
                    NextSampleAt = Time.realtimeSinceStartup + 0.25f
                };

                _addBuff("buff_skull", 54f);
                if (!IsActive("buff_skull"))
                    throw new InvalidOperationException(
                        "Native BuffsLogics.AddBuff did not activate buff_skull.");

                _session = session;
                _status =
                    "RUNNING: 0/" + Samples +
                    ". Test is paced across frames; do not press the button again.";

                _log?.LogInfo(
                    "REPOSE_GOLD_SELFTEST_BEGIN" +
                    " fixture_range=" + FixtureTierMin + ".." + FixtureTierMax +
                    " ordinary_candidates=" + fixture.Count +
                    " best_tier=" + fixture.BestTier +
                    " min_score=" + fixture.MinScore +
                    " max_score=" + fixture.MaxScore +
                    " max_score_candidates=" + fixture.MaxScoreCount +
                    " canonical_1407_fixture=" + session.CanonicalFixture +
                    " samples=" + Samples +
                    " pacing=one_sample_per_step interval_seconds=0.25");
            }
            catch (Exception ex)
            {
                try
                {
                    if (IsActive("buff_skull"))
                        _removeBuff("buff_skull");
                }
                catch (Exception cleanupEx)
                {
                    _log?.LogError(
                        "REPOSE_GOLD_SELFTEST_CLEANUP buff removal failed after start failure. " +
                        cleanupEx);
                }

                _status = "FAIL: " + ex.GetType().Name + " - " + ex.Message;
                _log?.LogError("REPOSE_GOLD_SELFTEST_FAIL " + ex);
            }
        }

        private void RunNextSample(TestSession session)
        {
            if (!ReferenceEquals(_session, session))
                return;

            int sampleIndex = session.CompletedSamples + 1;

            try
            {
                ArmProductionGoldPendingMode();

                _lastBodyDefinition = null;
                _catalogSeenDuringGeneration = null;
                _captureGeneration = true;

                object generated;
                try
                {
                    generated = _generateBody.Invoke(
                        session.Save,
                        new object[] { FixtureTierMin, FixtureTierMax, -1, -1 });
                }
                finally
                {
                    _captureGeneration = false;
                }

                if (generated == null)
                    throw new InvalidOperationException(
                        "GameSave.GenerateBody returned null at sample " + sampleIndex + ".");

                if (_lastBodyDefinition == null)
                    throw new InvalidOperationException(
                        "BodyDefinition.GenerateBodyItem was not observed at sample " +
                        sampleIndex + ".");

                if (_catalogSeenDuringGeneration == null)
                    throw new InvalidOperationException(
                        "Could not capture bodies_data during native generation at sample " +
                        sampleIndex + ".");

                CatalogStats projected = AnalyzeCatalog(
                    _catalogSeenDuringGeneration,
                    session.Fixture.BestTier,
                    FixtureTierMax);

                if (ReferenceEquals(
                    _catalogSeenDuringGeneration,
                    session.OriginalCatalog))
                {
                    throw new InvalidOperationException(
                        "Gold did not install a scoped body catalog at sample " +
                        sampleIndex + ".");
                }

                if (projected.Count != session.Fixture.MaxScoreCount ||
                    projected.BestTier != session.Fixture.BestTier ||
                    projected.MinScore != session.Fixture.MaxScore ||
                    projected.MaxScore != session.Fixture.MaxScore)
                {
                    throw new InvalidOperationException(
                        "Scoped Gold catalog mismatch at sample " + sampleIndex +
                        ": count=" + projected.Count +
                        ", tier=" + projected.BestTier +
                        ", scores=" + projected.MinScore + ".." + projected.MaxScore +
                        "; expected count=" + session.Fixture.MaxScoreCount +
                        ", tier=" + session.Fixture.BestTier +
                        ", score=" + session.Fixture.MaxScore + ".");
                }

                int selectedTier = ToInt(Get(_lastBodyDefinition, "tier"));
                string linkedItemId = Convert.ToString(
                    Get(_lastBodyDefinition, "linked_item_id"));
                int selectedScore = GetBodySkullScore(_lastBodyDefinition);
                string selectedId = GetId(_lastBodyDefinition) ?? "<unknown>";

                if (!string.Equals(linkedItemId, "body", StringComparison.Ordinal) ||
                    selectedTier != session.Fixture.BestTier ||
                    selectedScore != session.Fixture.MaxScore)
                {
                    throw new InvalidOperationException(
                        "Selected body mismatch at sample " + sampleIndex +
                        ": id=" + selectedId +
                        ", linked_item_id=" + linkedItemId +
                        ", tier=" + selectedTier +
                        ", score=" + selectedScore +
                        "; expected tier=" + session.Fixture.BestTier +
                        ", score=" + session.Fixture.MaxScore + ".");
                }

                object currentCatalog = Get(session.Balance, "bodies_data");
                if (!ReferenceEquals(currentCatalog, session.OriginalCatalog))
                {
                    throw new InvalidOperationException(
                        "GameBalance.bodies_data was not restored after sample " +
                        sampleIndex + ".");
                }

                int seen;
                session.SelectedIds.TryGetValue(selectedId, out seen);
                session.SelectedIds[selectedId] = seen + 1;
                session.CompletedSamples++;

                _status =
                    "RUNNING: " + session.CompletedSamples + "/" + Samples +
                    " passed; last=" + selectedId +
                    ", tier=" + selectedTier +
                    ", skull score=" + selectedScore + ".";

                _log?.LogInfo(
                    "REPOSE_GOLD_SELFTEST_SAMPLE" +
                    " index=" + sampleIndex +
                    " body=" + selectedId +
                    " tier=" + selectedTier +
                    " skull_score=" + selectedScore +
                    " scoped_candidates=" + projected.Count +
                    " catalog_restored=true");

                if (session.CompletedSamples >= Samples)
                {
                    FinishSession(session);
                    return;
                }

                session.NextSampleAt = Time.realtimeSinceStartup + 0.25f;
            }
            catch (TargetInvocationException ex)
            {
                Exception actual = ex.InnerException ?? ex;
                FailSession(session, actual);
            }
            catch (Exception ex)
            {
                FailSession(session, ex);
            }
        }

        private void FinishSession(TestSession session)
        {
            string distribution = string.Join(
                ",",
                session.SelectedIds
                    .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                    .Select(pair => pair.Key + "x" + pair.Value)
                    .ToArray());

            string verdict =
                session.CanonicalFixture ? "PASS" : "PASS_NONCANONICAL_DATA";

            _status =
                verdict +
                ": " + Samples + "/" + Samples +
                " generated bodies used tier " + session.Fixture.BestTier +
                " and max score " + session.Fixture.MaxScore +
                "; scoped catalog restored every time.";

            _log?.LogInfo(
                "REPOSE_GOLD_SELFTEST_" + verdict +
                " samples=" + Samples +
                " best_tier=" + session.Fixture.BestTier +
                " max_score=" + session.Fixture.MaxScore +
                " expected_candidates=" + session.Fixture.MaxScoreCount +
                " distinct_selected=" + session.SelectedIds.Count +
                " distribution=" + distribution);

            CleanupSession(session, verdict);
            _session = null;
        }

        private void FailSession(TestSession session, Exception ex)
        {
            _status = "FAIL: " + ex.GetType().Name + " - " + ex.Message;
            _log?.LogError("REPOSE_GOLD_SELFTEST_FAIL " + ex);
            CleanupSession(session, "FAIL");
            _session = null;
        }

        private void CleanupSession(TestSession session, string reason)
        {
            _captureGeneration = false;

            try
            {
                // Start precondition guarantees that there was no pre-existing
                // buff_skull, so any live one here belongs to this harness.
                if (IsActive("buff_skull"))
                    _removeBuff("buff_skull");
            }
            catch (Exception ex)
            {
                _log?.LogError(
                    "REPOSE_GOLD_SELFTEST_CLEANUP buff removal failed. " + ex);
            }

            try
            {
                object currentCatalog = Get(session.Balance, "bodies_data");
                if (!ReferenceEquals(currentCatalog, session.OriginalCatalog))
                {
                    _status += " CLEANUP FAIL: bodies_data reference changed.";
                    _log?.LogError(
                        "REPOSE_GOLD_SELFTEST_CLEANUP_FAIL bodies_data reference was not restored by production.");

                    Set(
                        session.Balance,
                        "bodies_data",
                        session.OriginalCatalog);
                    _log?.LogWarning(
                        "REPOSE_GOLD_SELFTEST_EMERGENCY_RESTORE bodies_data original reference restored by harness.");
                }

                float bodyMinAfter =
                    GetPlayerParam(session.Player, "body_min", 0f);
                float bodyMaxAfter =
                    GetPlayerParam(session.Player, "body_max", 0f);
                float addMinAfter =
                    GetPlayerParam(session.Player, "add_body_min", 0f);
                float addMaxAfter =
                    GetPlayerParam(session.Player, "add_body_max", 0f);

                bool tierInputsRestored =
                    Nearly(session.BodyMinBefore, bodyMinAfter) &&
                    Nearly(session.BodyMaxBefore, bodyMaxAfter) &&
                    Nearly(session.AddMinBefore, addMinAfter) &&
                    Nearly(session.AddMaxBefore, addMaxAfter);

                _log?.LogInfo(
                    "REPOSE_GOLD_SELFTEST_CLEANUP" +
                    " reason=" + reason +
                    " buff_active_after=" + IsActive("buff_skull") +
                    " tier_inputs_restored=" + tierInputsRestored +
                    " body_min=" + bodyMinAfter.ToString("0.###") +
                    " body_max=" + bodyMaxAfter.ToString("0.###") +
                    " add_body_min=" + addMinAfter.ToString("0.###") +
                    " add_body_max=" + addMaxAfter.ToString("0.###"));

                if (!tierInputsRestored)
                    _status += " CLEANUP WARNING: corpse tier player params changed.";
            }
            catch (Exception ex)
            {
                _log?.LogError(
                    "REPOSE_GOLD_SELFTEST_CLEANUP verification failed. " + ex);
            }
        }

        private static CatalogStats AnalyzeCatalog(object catalogObject, int tierMin, int tierMax)
        {
            IEnumerable catalog = catalogObject as IEnumerable;
            if (catalog == null)
                throw new InvalidOperationException("bodies_data is not enumerable.");

            CatalogStats stats = new CatalogStats();

            foreach (object body in catalog)
            {
                if (body == null) continue;

                string linkedItemId = Convert.ToString(Get(body, "linked_item_id"));
                if (!string.Equals(linkedItemId, "body", StringComparison.Ordinal))
                    continue;

                int tier = ToInt(Get(body, "tier"));
                if (tier < tierMin || tier > tierMax)
                    continue;

                int score = GetBodySkullScore(body);
                stats.Count++;
                if (tier > stats.BestTier) stats.BestTier = tier;
                if (score < stats.MinScore) stats.MinScore = score;

                if (score > stats.MaxScore)
                {
                    stats.MaxScore = score;
                    stats.MaxScoreCount = 1;
                }
                else if (score == stats.MaxScore)
                {
                    stats.MaxScoreCount++;
                }
            }

            return stats;
        }

        private static int GetBodySkullScore(object bodyDefinition)
        {
            IEnumerable parts = Get(bodyDefinition, "parts_ids") as IEnumerable;
            if (parts == null)
                throw new InvalidOperationException("BodyDefinition.parts_ids unavailable.");

            int score = 0;
            foreach (object raw in parts)
            {
                string partId = raw as string ?? Convert.ToString(raw);
                if (string.IsNullOrEmpty(partId))
                    throw new InvalidOperationException("BodyDefinition contains an empty part id.");

                object definition = GetBalanceData(partId, "ItemDefinition");
                if (definition == null)
                    throw new InvalidOperationException("Missing ItemDefinition: " + partId);

                definition = ResolveEffectiveItemDefinition(definition);
                score += ToInt(Get(definition, "q_minus"));
                score += ToInt(Get(definition, "q_plus"));
            }

            return score;
        }

        private static object ResolveEffectiveItemDefinition(object definition)
        {
            object replacement = Get(definition, "item_replace");
            if (replacement == null) return definition;

            string playerFlag = Convert.ToString(Get(replacement, "player_flag"));
            string replacementId = Convert.ToString(Get(replacement, "replace_id"));
            if (string.IsNullOrEmpty(playerFlag) || string.IsNullOrEmpty(replacementId))
                return definition;

            object player = GetPlayer();
            if (GetPlayerParam(player, playerFlag, 0f) <= 0f)
                return definition;

            object replacementDefinition = GetBalanceData(replacementId, "ItemDefinition");
            if (replacementDefinition == null)
                throw new InvalidOperationException(
                    "Missing replacement ItemDefinition: " + replacementId);

            return replacementDefinition;
        }

        private static object GetBalanceData(string id, string expectedTypeName)
        {
            object balance = GetStatic(FindType("GameBalance"), "me");
            Type expectedType = FindType(expectedTypeName);
            if (balance == null || expectedType == null)
                return null;

            MethodInfo[] candidates = balance.GetType().GetMethods(AnyInstance)
                .Where(method => method.Name == "GetDataOrNull")
                .Where(method =>
                {
                    ParameterInfo[] parameters = method.GetParameters();
                    return parameters.Length == 1 &&
                           parameters[0].ParameterType == typeof(string);
                })
                .ToArray();

            MethodInfo closed = candidates.FirstOrDefault(method =>
                !method.ContainsGenericParameters &&
                expectedType.IsAssignableFrom(method.ReturnType));
            if (closed != null)
                return closed.Invoke(balance, new object[] { id });

            MethodInfo generic = candidates.FirstOrDefault(method =>
                method.IsGenericMethodDefinition &&
                method.GetGenericArguments().Length == 1);
            if (generic == null)
                throw new MissingMethodException(
                    "GameBalanceBase.GetDataOrNull<" + expectedTypeName + ">(string)");

            return generic.MakeGenericMethod(expectedType)
                .Invoke(balance, new object[] { id });
        }

        private static void ResolveProductionSeams()
        {
            Type gameSave = FindType("GameSave");
            _generateBody = gameSave?.GetMethod(
                "GenerateBody",
                AnyInstance,
                null,
                new[] { typeof(int), typeof(int), typeof(int), typeof(int) },
                null);
            if (_generateBody == null)
                throw new MissingMethodException("GameSave.GenerateBody(int,int,int,int)");

            Type repose = FindType("PrayerClarity.RebalancedRepose");
            _pendingModeField = repose?.GetField(
                "_pendingMode",
                BindingFlags.NonPublic | BindingFlags.Static);
            if (_pendingModeField == null || !_pendingModeField.FieldType.IsEnum)
                throw new MissingMemberException("PrayerClarity.RebalancedRepose._pendingMode");
        }

        private static void ArmProductionGoldPendingMode()
        {
            object best = Enum.Parse(_pendingModeField.FieldType, "Best", false);
            _pendingModeField.SetValue(null, best);
        }

        private static void ResolveBuffApi()
        {
            Type buffsLogics = FindType("BuffsLogics");
            if (buffsLogics == null)
                throw new MissingMemberException("BuffsLogics");

            MethodInfo add = buffsLogics.GetMethods(AnyStatic)
                .FirstOrDefault(method =>
                {
                    if (method.Name != "AddBuff") return false;
                    ParameterInfo[] parameters = method.GetParameters();
                    return parameters.Length == 2 &&
                           parameters[0].ParameterType == typeof(string) &&
                           parameters[1].ParameterType == typeof(float?);
                });
            MethodInfo remove = buffsLogics.GetMethod(
                "RemoveBuff",
                AnyStatic,
                null,
                new[] { typeof(string) },
                null);
            _findBuffById = buffsLogics.GetMethod(
                "FindBuffByID",
                AnyStatic,
                null,
                new[] { typeof(string) },
                null);

            if (add == null)
                throw new MissingMethodException("BuffsLogics.AddBuff(string, Nullable<float>)");
            if (remove == null)
                throw new MissingMethodException("BuffsLogics.RemoveBuff(string)");
            if (_findBuffById == null)
                throw new MissingMethodException("BuffsLogics.FindBuffByID(string)");

            _addBuff = (Action<string, float?>)Delegate.CreateDelegate(
                typeof(Action<string, float?>),
                add);
            _removeBuff = (Action<string>)Delegate.CreateDelegate(
                typeof(Action<string>),
                remove);
        }

        private static void PatchBodyDefinitionCapture()
        {
            Type bodyDefinition = FindType("BodyDefinition");
            MethodInfo generate = bodyDefinition?.GetMethod(
                "GenerateBodyItem",
                AnyInstance,
                null,
                Type.EmptyTypes,
                null);
            if (generate == null)
                throw new MissingMethodException("BodyDefinition.GenerateBodyItem()");

            Type harmonyType = FindType("HarmonyLib.Harmony");
            Type harmonyMethodType = FindType("HarmonyLib.HarmonyMethod");
            if (harmonyType == null || harmonyMethodType == null)
                throw new InvalidOperationException("Harmony unavailable.");

            object harmony = Activator.CreateInstance(
                harmonyType,
                new object[] { PluginGuid + ".capture" });

            MethodInfo prefixMethod = typeof(ReposeGoldSelfTestPlugin).GetMethod(
                nameof(GenerateBodyItemPrefix),
                BindingFlags.NonPublic | BindingFlags.Static);
            object prefix = CreateHarmonyMethod(harmonyMethodType, prefixMethod);

            MethodInfo patch = harmonyType.GetMethods(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(method =>
                    method.Name == "Patch" &&
                    method.GetParameters().Length >= 5 &&
                    typeof(MethodBase).IsAssignableFrom(
                        method.GetParameters()[0].ParameterType));
            if (patch == null)
                throw new MissingMethodException("Harmony.Patch");

            object[] args = new object[patch.GetParameters().Length];
            args[0] = generate;
            args[1] = prefix;
            args[2] = null;
            args[3] = null;
            args[4] = null;
            patch.Invoke(harmony, args);
        }

        private static void GenerateBodyItemPrefix(object __instance)
        {
            if (!_captureGeneration) return;

            _lastBodyDefinition = __instance;
            object balance = GetStatic(FindType("GameBalance"), "me");
            _catalogSeenDuringGeneration = Get(balance, "bodies_data");
        }

        private static object CreateHarmonyMethod(Type harmonyMethodType, MethodInfo method)
        {
            ConstructorInfo ctor = harmonyMethodType.GetConstructor(
                new[] { typeof(MethodInfo) });
            if (ctor != null)
                return ctor.Invoke(new object[] { method });

            object instance = Activator.CreateInstance(harmonyMethodType);
            FieldInfo methodField = harmonyMethodType.GetField(
                "method",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (methodField == null)
                throw new MissingMemberException("HarmonyMethod.method");

            methodField.SetValue(instance, method);
            return instance;
        }

        private static bool IsActive(string buffId)
        {
            try
            {
                return _findBuffById != null &&
                       _findBuffById.Invoke(null, new object[] { buffId }) != null;
            }
            catch
            {
                return false;
            }
        }

        private static object GetPlayer()
        {
            object mainGame = GetStatic(FindType("MainGame"), "me");
            return Get(mainGame, "player");
        }

        private static object GetSave()
        {
            object mainGame = GetStatic(FindType("MainGame"), "me");
            return Get(mainGame, "save");
        }

        private static float GetPlayerParam(object player, string name, float fallback)
        {
            if (player == null) return fallback;

            MethodInfo getParam = player.GetType().GetMethod(
                "GetParam",
                AnyInstance,
                null,
                new[] { typeof(string), typeof(float) },
                null);
            if (getParam == null)
                return fallback;

            return Convert.ToSingle(
                getParam.Invoke(player, new object[] { name, fallback }));
        }

        private static string GetId(object obj)
        {
            object value = Get(obj, "id") ?? Get(obj, "obj_id");
            return value == null ? null : Convert.ToString(value);
        }

        private static int ToInt(object value)
        {
            if (value == null) return 0;
            return Convert.ToInt32(value);
        }

        private static bool Nearly(float a, float b)
        {
            return Math.Abs(a - b) <= 0.0001f;
        }

        private static object Get(object instance, string name)
        {
            if (instance == null || string.IsNullOrEmpty(name)) return null;

            for (Type type = instance.GetType(); type != null; type = type.BaseType)
            {
                FieldInfo field = type.GetField(
                    name,
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Instance |
                    BindingFlags.DeclaredOnly);
                if (field != null)
                    return field.GetValue(instance);

                PropertyInfo property = type.GetProperty(
                    name,
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Instance |
                    BindingFlags.DeclaredOnly);
                if (property != null && property.GetIndexParameters().Length == 0)
                    return property.GetValue(instance, null);
            }

            return null;
        }

        private static void Set(object instance, string name, object value)
        {
            if (instance == null || string.IsNullOrEmpty(name))
                throw new ArgumentNullException("instance/name");

            for (Type type = instance.GetType(); type != null; type = type.BaseType)
            {
                FieldInfo field = type.GetField(
                    name,
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Instance |
                    BindingFlags.DeclaredOnly);
                if (field != null)
                {
                    field.SetValue(instance, value);
                    return;
                }

                PropertyInfo property = type.GetProperty(
                    name,
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Instance |
                    BindingFlags.DeclaredOnly);
                if (property != null &&
                    property.GetIndexParameters().Length == 0 &&
                    property.CanWrite)
                {
                    property.SetValue(instance, value, null);
                    return;
                }
            }

            throw new MissingMemberException(instance.GetType().FullName + "." + name);
        }

        private static object GetStatic(Type type, string name)
        {
            if (type == null || string.IsNullOrEmpty(name)) return null;

            for (Type current = type; current != null; current = current.BaseType)
            {
                FieldInfo field = current.GetField(
                    name,
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Static |
                    BindingFlags.DeclaredOnly);
                if (field != null)
                    return field.GetValue(null);

                PropertyInfo property = current.GetProperty(
                    name,
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Static |
                    BindingFlags.DeclaredOnly);
                if (property != null && property.GetIndexParameters().Length == 0)
                    return property.GetValue(null, null);
            }

            return null;
        }

        private static Type FindType(string fullOrShortName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type direct = assembly.GetType(fullOrShortName, false);
                if (direct != null) return direct;

                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types;
                }
                catch
                {
                    continue;
                }

                Type match = types.FirstOrDefault(type =>
                    type != null &&
                    (type.FullName == fullOrShortName ||
                     type.Name == fullOrShortName));
                if (match != null) return match;
            }

            return null;
        }
    }
}
