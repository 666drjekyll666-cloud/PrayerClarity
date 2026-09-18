using System;
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
    public sealed class PrayerClarityRebalancedTestConsole : BaseUnityPlugin
    {
        public const string PluginGuid = "nikich.graveyardkeeper.prayerclarity.rebalanced.testconsole";
        public const string RebalancedPluginGuid = "nikich.graveyardkeeper.prayerclarity.rebalanced";
        public const string PluginName = "PrayerClarity: Rebalanced Test Console";
        public const string PluginVersion = "0.1.3";

        private sealed class TimedEffect
        {
            internal readonly string Name;
            internal readonly string BuffId;
            internal readonly float BaseDurationMinutes;
            internal readonly Action<int> ProjectTier;

            internal TimedEffect(string name, string buffId, float baseDurationMinutes, Action<int> projectTier = null)
            {
                Name = name;
                BuffId = buffId;
                BaseDurationMinutes = baseDurationMinutes;
                ProjectTier = projectTier;
            }
        }

        private static readonly BindingFlags AnyStatic =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        private static readonly BindingFlags AnyInstance =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        private static ManualLogSource _log;
        private static Action<string, float?> _addBuff;
        private static Action<string> _removeBuff;
        private static MethodInfo _findBuffById;
        private static bool _simulateBoostII;
        private static int _regenDiagnosticsRemaining;
        private static readonly HashSet<string> RootsDiagnosticLoggedCrafts = new HashSet<string>(StringComparer.Ordinal);

        private readonly List<TimedEffect> _effects = new List<TimedEffect>();
        private Rect _windowRect = new Rect(24f, 24f, 620f, 560f);
        private bool _visible;
        private string _status = "F1 opens/closes this console. Synthetic buffs use the game's native BuffsLogics path.";

        private void Awake()
        {
            _log = Logger;
            ResolveBuffApi();
            PatchRootsDiagnostic();
            PatchBoostIISimulation();
            PatchCombatRegenDiagnostic();

            _effects.Add(new TimedEffect(
                "Shoots & Roots",
                "buff_plant",
                36f,
                tier =>
                {
                    SetPlayerParam("prayerclarity_rebalanced_plant_tier", tier);
                    SetPlayerParam(
                        "prayerclarity_rebalanced_plant_reduction",
                        Tier(tier, 0.20f, 0.30f, 0.40f));
                }));

            _effects.Add(new TimedEffect(
                "Repentance",
                "buff_sins",
                18f,
                tier => SetPlayerParam("prayerclarity_rebalanced_confession_tier", tier)));

            _effects.Add(new TimedEffect(
                "Repose",
                "buff_skull",
                18f,
                tier => SetPlayerParam("prayerclarity_rebalanced_repose_tier", tier)));

            _effects.Add(new TimedEffect(
                "Combat",
                "buff_sword",
                36f,
                tier =>
                {
                    SetPlayerParam("prayerclarity_rebalanced_combat_tier", tier);
                    SetPlayerParam(
                        "prayerclarity_rebalanced_combat_regen",
                        Tier(tier, 1f, 2f, 4f));
                }));

            _effects.Add(new TimedEffect("Imagination", "buff_pen", 18f));

            _effects.Add(new TimedEffect(
                "Excellence",
                "buff_star",
                18f,
                tier => SetPlayerParam("prayerclarity_rebalanced_excellence_tier", tier)));

            _effects.Add(new TimedEffect("Soul Contentment (BSS)", "buff_gp_increase", 36f));
            _effects.Add(new TimedEffect("Thorough Cleansing (BSS)", "buff_sin_shard", 36f));

            Logger.LogInfo(
                PluginName + " " + PluginVersion +
                " loaded. Press F1 for the research-only timed-effect console. " +
                "This tool mutates the current save's live buff list through the native BuffsLogics API; clear synthetic buffs before saving a test state you want to keep.");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
                _visible = !_visible;

            if (_visible && Input.GetKeyDown(KeyCode.Escape))
                _visible = false;
        }

        private void OnGUI()
        {
            if (!_visible) return;
            _windowRect = GUILayout.Window(
                736214,
                _windowRect,
                DrawWindow,
                "PrayerClarity: Rebalanced Test Console 0.1.3");
        }

        private void DrawWindow(int id)
        {
            GUILayout.Label("Research-only. F1 toggles. Escape closes.");
            GUILayout.Label("Activate uses the real BuffsLogics.AddBuff path; Remove uses BuffsLogics.RemoveBuff.");
            GUILayout.Label("Do not save a permanent playthrough state while a synthetic test buff is active.");

            GUILayout.Space(8f);

            foreach (TimedEffect effect in _effects)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(
                    effect.Name + (IsActive(effect.BuffId) ? "  [ACTIVE]" : ""),
                    GUILayout.Width(225f));

                if (GUILayout.Button("Bronze", GUILayout.Width(72f)))
                    Activate(effect, 1);
                if (GUILayout.Button("Silver", GUILayout.Width(72f)))
                    Activate(effect, 2);
                if (GUILayout.Button("Gold", GUILayout.Width(72f)))
                    Activate(effect, 3);
                if (GUILayout.Button("Remove", GUILayout.Width(72f)))
                    Remove(effect);

                GUILayout.EndHorizontal();
            }

            GUILayout.Space(10f);

            if (GUILayout.Button("Remove all synthetic prayer buffs"))
                RemoveAll();

            GUILayout.Space(8f);
            GUILayout.Label("Native-seam probes (0.2.3):");

            if (GUILayout.Button("Probe Repentance probability"))
                ProbeRepentanceProbability();

            if (GUILayout.Button("Probe Combat outgoing damage"))
                ProbeCombatDamage();

            if (GUILayout.Button("Probe Combat armor (controlled 20 damage; HP restored)"))
                ProbeCombatArmor();

            if (GUILayout.Button("Prepare Combat regen probe (set HP to max - 20)"))
                PrepareCombatRegenProbe();

            GUILayout.Space(8f);

            if (GUILayout.Button(
                _simulateBoostII
                    ? "Boost II growth simulation: ON"
                    : "Boost II growth simulation: OFF"))
            {
                _simulateBoostII = !_simulateBoostII;
                RootsDiagnosticLoggedCrafts.Clear();
                _status = _simulateBoostII
                    ? "Boost II simulation enabled. Relevant growth expressions read grow_time=3 without changing save data."
                    : "Boost II simulation disabled.";
                _log?.LogInfo(_status);
            }

            GUILayout.Label("Simulation affects only active plant crafts whose expression already uses both grow_time and buff_plant.");

            GUILayout.Space(8f);
            GUILayout.Label("Status: " + _status);

            GUI.DragWindow(new Rect(0f, 0f, 10000f, 24f));
        }

        private void ProbeRepentanceProbability()
        {
            object player = GetPlayer();
            if (player == null)
            {
                _status = "Repentance probe failed: player unavailable.";
                return;
            }

            try
            {
                Type getterType = FindType("FlowCanvas.Nodes.Flow_GetPlayerParam");
                MethodInfo invoke = getterType?.GetMethod(
                    "Invoke",
                    AnyInstance,
                    null,
                    new[] { typeof(string) },
                    null);
                if (invoke == null)
                    throw new MissingMethodException("Flow_GetPlayerParam.Invoke(string)");

                float original = GetPlayerParam(player, "confession_probability", 0f);
                float result;
                try
                {
                    SetPlayerParam("confession_probability", 0.15f);
                    object getter = Activator.CreateInstance(getterType);
                    result = Convert.ToSingle(invoke.Invoke(getter, new object[] { "confession_probability" }));
                }
                finally
                {
                    SetPlayerParam("confession_probability", original);
                }

                int tier = (int)Math.Round(
                    GetPlayerParam(player, "prayerclarity_rebalanced_confession_tier", 0f));
                bool active = IsActive("buff_sins");

                _status = "Repentance probe: active=" + active +
                          ", tier=" + tier +
                          ", effective probability=" + result.ToString("0.###") + ".";
                _log?.LogInfo(
                    "REPENTANCE_DIAGNOSTIC active=" + active +
                    " tier=" + tier +
                    " stock_stored_for_probe=0.15" +
                    " effective_probability=" + result.ToString("0.###"));
            }
            catch (Exception ex)
            {
                _status = "Repentance probe failed: " + ex.GetType().Name;
                _log?.LogError("PrayerClarity Rebalanced Test Console Repentance probe failed. " + ex);
            }
        }

        private void ProbeCombatDamage()
        {
            object player = GetPlayer();
            if (player == null)
            {
                _status = "Combat damage probe failed: player unavailable.";
                return;
            }

            try
            {
                if (!IsActive("buff_sword"))
                {
                    _status = "Combat damage probe requires an active synthetic Combat buff.";
                    return;
                }

                MethodInfo getEquippedWeapon = player.GetType().GetMethod(
                    "GetEquippedWeapon",
                    AnyInstance,
                    null,
                    Type.EmptyTypes,
                    null);
                if (getEquippedWeapon == null || getEquippedWeapon.Invoke(player, null) == null)
                {
                    _status = "Combat damage probe requires an equipped weapon (native GetDamage otherwise uses its 10-damage fallback).";
                    return;
                }

                Type damageType = FindType("ObjectDefinition+DamageType");
                MethodInfo getDamage = player.GetType().GetMethod(
                    "GetDamage",
                    AnyInstance,
                    null,
                    new[] { damageType },
                    null);
                if (damageType == null || getDamage == null)
                    throw new MissingMethodException("WorldGameObject.GetDamage(DamageType)");

                float originalTier = GetPlayerParam(
                    player,
                    "prayerclarity_rebalanced_combat_tier",
                    0f);
                object defaultDamageType = Enum.ToObject(damageType, 0);

                float bronzeBaseline;
                float actual;
                try
                {
                    SetPlayerParam("prayerclarity_rebalanced_combat_tier", 1f);
                    bronzeBaseline = Convert.ToSingle(
                        getDamage.Invoke(player, new[] { defaultDamageType }));

                    SetPlayerParam("prayerclarity_rebalanced_combat_tier", originalTier);
                    actual = Convert.ToSingle(
                        getDamage.Invoke(player, new[] { defaultDamageType }));
                }
                finally
                {
                    SetPlayerParam("prayerclarity_rebalanced_combat_tier", originalTier);
                }

                float delta = actual - bronzeBaseline;
                _status = "Combat damage probe: Bronze/native=" +
                          bronzeBaseline.ToString("0.###") +
                          ", current=" + actual.ToString("0.###") +
                          ", tier delta=" + delta.ToString("0.###") + ".";
                _log?.LogInfo(
                    "COMBAT_DAMAGE_DIAGNOSTIC tier=" + originalTier.ToString("0") +
                    " bronze_native_result=" + bronzeBaseline.ToString("0.###") +
                    " current_result=" + actual.ToString("0.###") +
                    " tier_delta=" + delta.ToString("0.###") +
                    " stored_add_damage=" + GetPlayerParam(player, "add_damage", 0f).ToString("0.###"));
            }
            catch (Exception ex)
            {
                _status = "Combat damage probe failed: " + ex.GetType().Name;
                _log?.LogError("PrayerClarity Rebalanced Test Console Combat damage probe failed. " + ex);
            }
        }

        private void ProbeCombatArmor()
        {
            object player = GetPlayer();
            if (player == null)
            {
                _status = "Combat armor probe failed: player unavailable.";
                return;
            }

            try
            {
                if (!IsActive("buff_sword"))
                {
                    _status = "Combat armor probe requires an active synthetic Combat buff.";
                    return;
                }

                object components = Get(player, "components");
                object hpComponent = Get(components, "hp");
                MethodInfo decHp = hpComponent?.GetType().GetMethod(
                    "DecHP",
                    AnyInstance,
                    null,
                    new[] { typeof(float) },
                    null);
                if (decHp == null)
                    throw new MissingMethodException("HPActionComponent.DecHP(float)");

                float originalTier = GetPlayerParam(
                    player,
                    "prayerclarity_rebalanced_combat_tier",
                    0f);
                float originalHp = GetPlayerParam(player, "hp", 0f);

                float stockLoss;
                float combatLoss;
                try
                {
                    SetPlayerParam("prayerclarity_rebalanced_combat_tier", 0f);
                    SetPlayerParam("hp", originalHp);
                    decHp.Invoke(hpComponent, new object[] { 20f });
                    stockLoss = originalHp - GetPlayerParam(player, "hp", originalHp);

                    SetPlayerParam("hp", originalHp);
                    SetPlayerParam("prayerclarity_rebalanced_combat_tier", originalTier);
                    decHp.Invoke(hpComponent, new object[] { 20f });
                    combatLoss = originalHp - GetPlayerParam(player, "hp", originalHp);
                }
                finally
                {
                    SetPlayerParam("hp", originalHp);
                    SetPlayerParam("prayerclarity_rebalanced_combat_tier", originalTier);
                }

                float prevented = stockLoss - combatLoss;
                _status = "Combat armor probe: stock loss=" +
                          stockLoss.ToString("0.###") +
                          ", Combat loss=" + combatLoss.ToString("0.###") +
                          ", prevented=" + prevented.ToString("0.###") + ".";
                _log?.LogInfo(
                    "COMBAT_ARMOR_DIAGNOSTIC tier=" + originalTier.ToString("0") +
                    " input_damage=20" +
                    " stock_loss=" + stockLoss.ToString("0.###") +
                    " combat_loss=" + combatLoss.ToString("0.###") +
                    " prevented_by_rebalanced_armor=" + prevented.ToString("0.###"));
            }
            catch (Exception ex)
            {
                _status = "Combat armor probe failed: " + ex.GetType().Name;
                _log?.LogError("PrayerClarity Rebalanced Test Console Combat armor probe failed. " + ex);
            }
        }

        private void PrepareCombatRegenProbe()
        {
            object player = GetPlayer();
            if (player == null)
            {
                _status = "Combat regen probe failed: player unavailable.";
                return;
            }

            try
            {
                if (!IsActive("buff_sword"))
                {
                    _status = "Combat regen probe requires an active synthetic Combat buff.";
                    return;
                }

                Type mainGame = FindType("MainGame");
                object me = GetStatic(mainGame, "me");
                object save = Get(me, "save");
                float maxHp = Convert.ToSingle(Get(save, "max_hp"));
                float target = Math.Max(1f, maxHp - 20f);

                _regenDiagnosticsRemaining = 3;
                SetPlayerParam("hp", target);

                int tier = (int)Math.Round(
                    GetPlayerParam(player, "prayerclarity_rebalanced_combat_tier", 0f));
                _status = "Combat regen probe armed at HP " +
                          target.ToString("0.###") + "/" + maxHp.ToString("0.###") +
                          "; waiting for native se_tick.";
                _log?.LogInfo(
                    "COMBAT_REGEN_PROBE_ARMED tier=" + tier +
                    " hp=" + target.ToString("0.###") +
                    " max_hp=" + maxHp.ToString("0.###"));
            }
            catch (Exception ex)
            {
                _status = "Combat regen probe failed: " + ex.GetType().Name;
                _log?.LogError("PrayerClarity Rebalanced Test Console Combat regen probe failed. " + ex);
            }
        }

        private static void ResolveBuffApi()
        {
            Type buffsLogics = FindType("BuffsLogics");
            if (buffsLogics == null) throw new MissingMemberException("BuffsLogics");

            MethodInfo add = buffsLogics.GetMethods(AnyStatic)
                .FirstOrDefault(m =>
                {
                    if (m.Name != "AddBuff") return false;
                    ParameterInfo[] p = m.GetParameters();
                    return p.Length == 2 &&
                           p[0].ParameterType == typeof(string) &&
                           p[1].ParameterType == typeof(float?);
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

            if (add == null) throw new MissingMethodException("BuffsLogics.AddBuff(string, Nullable<float>)");
            if (remove == null) throw new MissingMethodException("BuffsLogics.RemoveBuff(string)");
            if (_findBuffById == null) throw new MissingMethodException("BuffsLogics.FindBuffByID(string)");

            _addBuff = (Action<string, float?>)Delegate.CreateDelegate(typeof(Action<string, float?>), add);
            _removeBuff = (Action<string>)Delegate.CreateDelegate(typeof(Action<string>), remove);
        }

        private static void PatchCombatRegenDiagnostic()
        {
            Type worldGameObject = FindType("WorldGameObject");
            if (worldGameObject == null)
                throw new MissingMemberException("WorldGameObject");

            MethodInfo setParam = worldGameObject.GetMethod(
                "SetParam",
                AnyInstance,
                null,
                new[] { typeof(string), typeof(float) },
                null);
            if (setParam == null)
                throw new MissingMethodException("WorldGameObject.SetParam(string,float)");

            Type harmonyType = FindType("HarmonyLib.Harmony");
            Type harmonyMethodType = FindType("HarmonyLib.HarmonyMethod");
            if (harmonyType == null || harmonyMethodType == null)
                throw new InvalidOperationException("Harmony unavailable.");

            object harmony = Activator.CreateInstance(
                harmonyType,
                new object[] { "nikich.graveyardkeeper.prayerclarity.rebalanced.testconsole.regendiag" });

            MethodInfo prefixMethod = typeof(PrayerClarityRebalancedTestConsole).GetMethod(
                nameof(HpSetParamPrefix),
                BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo postfixMethod = typeof(PrayerClarityRebalancedTestConsole).GetMethod(
                nameof(HpSetParamPostfix),
                BindingFlags.NonPublic | BindingFlags.Static);

            object prefix = CreateHarmonyMethod(harmonyMethodType, prefixMethod);
            object postfix = CreateHarmonyMethod(harmonyMethodType, postfixMethod);

            MethodInfo patch = harmonyType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(m =>
                    m.Name == "Patch" &&
                    m.GetParameters().Length >= 5 &&
                    typeof(MethodBase).IsAssignableFrom(m.GetParameters()[0].ParameterType));
            if (patch == null) throw new MissingMethodException("Harmony.Patch");

            object[] args = new object[patch.GetParameters().Length];
            args[0] = setParam;
            args[1] = prefix;
            args[2] = postfix;
            args[3] = null;
            args[4] = null;
            patch.Invoke(harmony, args);
        }

        private static void HpSetParamPrefix(object __instance, string param_name, ref float __state)
        {
            __state = float.NaN;
            if (_regenDiagnosticsRemaining <= 0 ||
                !string.Equals(param_name, "hp", StringComparison.Ordinal) ||
                !IsActive("buff_sword") ||
                !IsPlayer(__instance))
                return;

            __state = GetPlayerParam(__instance, "hp", 0f);
        }

        private static void HpSetParamPostfix(object __instance, string param_name, float value, float __state)
        {
            if (float.IsNaN(__state) ||
                _regenDiagnosticsRemaining <= 0 ||
                !string.Equals(param_name, "hp", StringComparison.Ordinal))
                return;

            float after = GetPlayerParam(__instance, "hp", value);
            float delta = after - __state;
            if (delta <= 0.0001f) return;

            int tier = (int)Math.Round(
                GetPlayerParam(__instance, "prayerclarity_rebalanced_combat_tier", 0f));
            _regenDiagnosticsRemaining--;

            _log?.LogInfo(
                "COMBAT_REGEN_DIAGNOSTIC tier=" + tier +
                " before=" + __state.ToString("0.###") +
                " after=" + after.ToString("0.###") +
                " delta=" + delta.ToString("0.###") +
                " samples_remaining=" + _regenDiagnosticsRemaining);
        }

        private static bool IsPlayer(object instance)
        {
            object value = Get(instance, "is_player");
            return value != null && Convert.ToBoolean(value);
        }

        private static void PatchRootsDiagnostic()
        {
            Type craftComponent = FindType("CraftComponent");
            Type worldGameObject = FindType("WorldGameObject");
            if (craftComponent == null || worldGameObject == null)
                throw new MissingMemberException("Roots diagnostic runtime types are unavailable.");

            MethodInfo doAction = craftComponent.GetMethod(
                "DoAction",
                AnyInstance,
                null,
                new[] { worldGameObject, typeof(float), typeof(bool) },
                null);
            if (doAction == null)
                throw new MissingMethodException("CraftComponent.DoAction(WorldGameObject,float,bool)");

            Type harmonyType = FindType("HarmonyLib.Harmony");
            Type harmonyMethodType = FindType("HarmonyLib.HarmonyMethod");
            if (harmonyType == null || harmonyMethodType == null)
                throw new InvalidOperationException("Harmony unavailable.");

            object harmony = Activator.CreateInstance(
                harmonyType,
                new object[] { "nikich.graveyardkeeper.prayerclarity.rebalanced.testconsole.rootsdiag" });

            MethodInfo postfixMethod = typeof(PrayerClarityRebalancedTestConsole).GetMethod(
                nameof(RootsDoActionPostfix),
                BindingFlags.NonPublic | BindingFlags.Static);
            object postfix = CreateHarmonyMethod(harmonyMethodType, postfixMethod);

            MethodInfo patch = harmonyType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(m =>
                    m.Name == "Patch" &&
                    m.GetParameters().Length >= 5 &&
                    typeof(MethodBase).IsAssignableFrom(m.GetParameters()[0].ParameterType));
            if (patch == null) throw new MissingMethodException("Harmony.Patch");

            object[] args = new object[patch.GetParameters().Length];
            args[0] = doAction;
            args[1] = null;
            args[2] = postfix;
            args[3] = null;
            args[4] = null;
            patch.Invoke(harmony, args);
        }

        private static void PatchBoostIISimulation()
        {
            Type worldGameObject = FindType("WorldGameObject");
            if (worldGameObject == null)
                throw new MissingMemberException("WorldGameObject");

            MethodInfo getParam = worldGameObject.GetMethod(
                "GetParam",
                AnyInstance,
                null,
                new[] { typeof(string), typeof(float) },
                null);
            if (getParam == null)
                throw new MissingMethodException("WorldGameObject.GetParam(string,float)");

            Type harmonyType = FindType("HarmonyLib.Harmony");
            Type harmonyMethodType = FindType("HarmonyLib.HarmonyMethod");
            if (harmonyType == null || harmonyMethodType == null)
                throw new InvalidOperationException("Harmony unavailable.");

            object harmony = Activator.CreateInstance(
                harmonyType,
                new object[] { "nikich.graveyardkeeper.prayerclarity.rebalanced.testconsole.boost2sim" });

            MethodInfo postfixMethod = typeof(PrayerClarityRebalancedTestConsole).GetMethod(
                nameof(GrowTimeGetParamPostfix),
                BindingFlags.NonPublic | BindingFlags.Static);
            object postfix = CreateHarmonyMethod(harmonyMethodType, postfixMethod);

            MethodInfo patch = harmonyType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(m =>
                    m.Name == "Patch" &&
                    m.GetParameters().Length >= 5 &&
                    typeof(MethodBase).IsAssignableFrom(m.GetParameters()[0].ParameterType));
            if (patch == null) throw new MissingMethodException("Harmony.Patch");

            object[] args = new object[patch.GetParameters().Length];
            args[0] = getParam;
            args[1] = null;
            args[2] = postfix;
            args[3] = null;
            args[4] = null;
            patch.Invoke(harmony, args);
        }

        private static void GrowTimeGetParamPostfix(object __instance, string param_name, ref float __result)
        {
            if (!_simulateBoostII ||
                !string.Equals(param_name, "grow_time", StringComparison.Ordinal) ||
                !IsActive("buff_plant") ||
                __instance == null)
                return;

            try
            {
                object components = Get(__instance, "components");
                object craftComponent = Get(components, "craft");
                object craft = Get(craftComponent, "current_craft");
                object craftTime = Get(craft, "craft_time");
                if (craftTime == null) return;

                MethodInfo rawMethod = craftTime.GetType().GetMethod(
                    "GetRawExpressionString",
                    AnyInstance,
                    null,
                    Type.EmptyTypes,
                    null);
                string raw = rawMethod == null ? null : rawMethod.Invoke(craftTime, null) as string;
                if (string.IsNullOrEmpty(raw) ||
                    raw.IndexOf("WGOpar(\"grow_time\")", StringComparison.Ordinal) < 0 ||
                    raw.IndexOf("WGOpar(\"buff_plant\")", StringComparison.Ordinal) < 0)
                    return;

                __result = Math.Max(__result, 3f);
            }
            catch (Exception ex)
            {
                _log?.LogError("PrayerClarity Rebalanced Test Console Boost II simulation failed closed. " + ex);
            }
        }

        private static object CreateHarmonyMethod(Type harmonyMethodType, MethodInfo method)
        {
            ConstructorInfo ctor = harmonyMethodType.GetConstructor(new[] { typeof(MethodInfo) });
            if (ctor != null) return ctor.Invoke(new object[] { method });

            object instance = Activator.CreateInstance(harmonyMethodType);
            FieldInfo methodField = harmonyMethodType.GetField(
                "method",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (methodField == null) throw new MissingMemberException("HarmonyMethod.method");
            methodField.SetValue(instance, method);
            return instance;
        }

        private static void RootsDoActionPostfix(object __instance)
        {
            try
            {
                if (!IsActive("buff_plant")) return;

                object craft = Get(__instance, "current_craft");
                string craftId = GetId(craft);
                if (string.IsNullOrEmpty(craftId) || RootsDiagnosticLoggedCrafts.Contains(craftId)) return;

                object craftTime = Get(craft, "craft_time");
                if (craftTime == null) return;

                MethodInfo rawMethod = craftTime.GetType().GetMethod(
                    "GetRawExpressionString",
                    AnyInstance,
                    null,
                    Type.EmptyTypes,
                    null);
                string raw = rawMethod == null ? null : rawMethod.Invoke(craftTime, null) as string;
                if (string.IsNullOrEmpty(raw) ||
                    raw.IndexOf("WGOpar(\"buff_plant\")", StringComparison.Ordinal) < 0)
                    return;

                object wgo = Get(__instance, "wgo");
                object player = GetPlayer();
                if (wgo == null || player == null) return;

                float reduction = GetPlayerParam(player, "prayerclarity_rebalanced_plant_reduction", 0f);
                int tier = (int)Math.Round(GetPlayerParam(player, "prayerclarity_rebalanced_plant_tier", 0f));
                if (reduction <= 0.0001f || tier < 1 || tier > 3) return;

                MethodInfo wgoGetParam = wgo.GetType().GetMethod(
                    "GetParam",
                    AnyInstance,
                    null,
                    new[] { typeof(string), typeof(float) },
                    null);
                if (wgoGetParam == null) return;

                float effectiveBuffPlant = Convert.ToSingle(
                    wgoGetParam.Invoke(wgo, new object[] { "buff_plant", 0f }));
                float growTime = Convert.ToSingle(
                    wgoGetParam.Invoke(wgo, new object[] { "grow_time", 0f }));

                MethodInfo evaluateFloat = craftTime.GetType().GetMethod(
                    "EvaluateFloat",
                    AnyInstance,
                    null,
                    new[] { wgo.GetType(), player.GetType() },
                    null);
                if (evaluateFloat == null)
                {
                    Type wgoType = FindType("WorldGameObject");
                    evaluateFloat = craftTime.GetType().GetMethod(
                        "EvaluateFloat",
                        AnyInstance,
                        null,
                        new[] { wgoType, wgoType },
                        null);
                }
                if (evaluateFloat == null) return;

                float withRoots = Convert.ToSingle(
                    evaluateFloat.Invoke(craftTime, new[] { wgo, player }));

                object runtimeEffect = Get(wgo, "totem_effect");
                if (runtimeEffect == null) return;

                MethodInfo resGet = runtimeEffect.GetType().GetMethod(
                    "Get",
                    AnyInstance,
                    null,
                    new[] { typeof(string), typeof(float) },
                    null);
                MethodInfo resSet = runtimeEffect.GetType().GetMethod(
                    "Set",
                    AnyInstance,
                    null,
                    new[] { typeof(string), typeof(float) },
                    null);
                if (resGet == null || resSet == null) return;

                float currentRuntimeValue = Convert.ToSingle(
                    resGet.Invoke(runtimeEffect, new object[] { "buff_plant", 0f }));

                float withoutRoots;
                try
                {
                    // Controlled research baseline: remove the current runtime-only
                    // buff_plant contribution, evaluate the same native expression,
                    // then restore it immediately.
                    resSet.Invoke(
                        runtimeEffect,
                        new object[] { "buff_plant", 0f });
                    withoutRoots = Convert.ToSingle(
                        evaluateFloat.Invoke(craftTime, new[] { wgo, player }));
                }
                finally
                {
                    resSet.Invoke(
                        runtimeEffect,
                        new object[] { "buff_plant", currentRuntimeValue });
                }

                RootsDiagnosticLoggedCrafts.Add(craftId);
                float saved = withoutRoots - withRoots;
                float savedPercent = withoutRoots > 0.0001f ? saved / withoutRoots * 100f : 0f;

                _log?.LogInfo(
                    "ROOTS_DIAGNOSTIC craft=" + craftId +
                    " tier=" + tier +
                    " configured_reduction=" + reduction.ToString("0.###") +
                    " effective_WGO_buff_plant=" + effectiveBuffPlant.ToString("0.###") +
                    " grow_time=" + growTime.ToString("0.###") +
                    " boost_ii_sim=" + _simulateBoostII +
                    " craft_time_without_roots=" + withoutRoots.ToString("0.###") +
                    " craft_time_with_roots=" + withRoots.ToString("0.###") +
                    " saved=" + saved.ToString("0.###") +
                    " (" + savedPercent.ToString("0.#") + "% of this fertilizer-adjusted baseline)." +
                    " raw=\"" + raw + "\"");
            }
            catch (Exception ex)
            {
                _log?.LogError("PrayerClarity Rebalanced Test Console Roots diagnostic failed. " + ex);
            }
        }

        private static object GetPlayer()
        {
            Type mainGame = FindType("MainGame");
            object me = GetStatic(mainGame, "me");
            return Get(me, "player");
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
            return getParam == null
                ? fallback
                : Convert.ToSingle(getParam.Invoke(player, new object[] { name, fallback }));
        }

        private static string GetId(object obj)
        {
            if (obj == null) return null;
            object id = Get(obj, "id") ?? Get(obj, "obj_id");
            return id == null ? null : Convert.ToString(id);
        }

        private void Activate(TimedEffect effect, int tier)
        {
            try
            {
                if (effect == null || tier < 1 || tier > 3) return;

                if (string.Equals(effect.BuffId, "buff_plant", StringComparison.Ordinal))
                    RootsDiagnosticLoggedCrafts.Clear();

                if (IsActive(effect.BuffId))
                    _removeBuff(effect.BuffId);

                effect.ProjectTier?.Invoke(tier);

                float duration = effect.BaseDurationMinutes * tier;
                _addBuff(effect.BuffId, duration);

                if (IsActive(effect.BuffId))
                {
                    _status = effect.Name + " " + TierName(tier) +
                              " activated through native BuffsLogics.AddBuff (" +
                              duration.ToString("0.#") + " min override).";
                    _log?.LogInfo(_status);
                }
                else
                {
                    _status = effect.Name + " could not be activated. The buff definition may be unavailable in this game/DLC state.";
                    _log?.LogWarning(_status);
                }
            }
            catch (Exception ex)
            {
                _status = "Activation failed for " + effect?.Name + ": " + ex.GetType().Name;
                _log?.LogError("PrayerClarity Rebalanced Test Console activation failed. " + ex);
            }
        }

        private void Remove(TimedEffect effect)
        {
            if (effect == null) return;
            try
            {
                bool wasActive = IsActive(effect.BuffId);
                if (wasActive)
                    _removeBuff(effect.BuffId);

                _status = wasActive
                    ? effect.Name + " removed through native BuffsLogics.RemoveBuff."
                    : effect.Name + " was not active.";
                _log?.LogInfo(_status);
            }
            catch (Exception ex)
            {
                _status = "Removal failed for " + effect.Name + ": " + ex.GetType().Name;
                _log?.LogError("PrayerClarity Rebalanced Test Console removal failed. " + ex);
            }
        }

        private void RemoveAll()
        {
            int removed = 0;
            foreach (TimedEffect effect in _effects)
            {
                try
                {
                    if (!IsActive(effect.BuffId)) continue;
                    _removeBuff(effect.BuffId);
                    removed++;
                }
                catch (Exception ex)
                {
                    _log?.LogError("PrayerClarity Rebalanced Test Console failed to remove " + effect.BuffId + ". " + ex);
                }
            }

            _status = "Removed " + removed + " active prayer buff(s).";
            _log?.LogInfo(_status);
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

        private static float Tier(int tier, float bronze, float silver, float gold)
        {
            return tier == 1 ? bronze : tier == 2 ? silver : gold;
        }

        private static string TierName(int tier)
        {
            return tier == 1 ? "Bronze" : tier == 2 ? "Silver" : "Gold";
        }

        private static void SetPlayerParam(string name, float value)
        {
            Type mainGame = FindType("MainGame");
            object me = GetStatic(mainGame, "me");
            object player = Get(me, "player");
            if (player == null) throw new InvalidOperationException("MainGame.me.player unavailable.");

            MethodInfo setParam = player.GetType().GetMethod(
                "SetParam",
                AnyInstance,
                null,
                new[] { typeof(string), typeof(float) },
                null);
            if (setParam == null) throw new MissingMethodException("WorldGameObject.SetParam(string,float)");

            setParam.Invoke(player, new object[] { name, value });
        }

        private static object Get(object instance, string name)
        {
            if (instance == null || string.IsNullOrEmpty(name)) return null;

            for (Type type = instance.GetType(); type != null; type = type.BaseType)
            {
                FieldInfo field = type.GetField(
                    name,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (field != null) return field.GetValue(instance);

                PropertyInfo property = type.GetProperty(
                    name,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (property != null && property.GetIndexParameters().Length == 0)
                    return property.GetValue(instance, null);
            }

            return null;
        }

        private static object GetStatic(Type type, string name)
        {
            if (type == null || string.IsNullOrEmpty(name)) return null;

            for (Type current = type; current != null; current = current.BaseType)
            {
                FieldInfo field = current.GetField(
                    name,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);
                if (field != null) return field.GetValue(null);

                PropertyInfo property = current.GetProperty(
                    name,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);
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

                Type match = types.FirstOrDefault(t =>
                    t != null &&
                    (t.FullName == fullOrShortName || t.Name == fullOrShortName));
                if (match != null) return match;
            }

            return null;
        }
    }
}
