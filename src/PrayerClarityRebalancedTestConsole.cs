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
    public sealed class PrayerClarityRebalancedTestConsole : BaseUnityPlugin
    {
        public const string PluginGuid = "nikich.graveyardkeeper.prayerclarity.rebalanced.testconsole";
        public const string RebalancedPluginGuid = "nikich.graveyardkeeper.prayerclarity.rebalanced";
        public const string PluginName = "PrayerClarity: Rebalanced Test Console";
        public const string PluginVersion = "0.1.15";

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
        private Rect _windowRect = new Rect(24f, 24f, 620f, 680f);
        private bool _visible;
        private static bool _qualityTitleProbeEnabled = false;
        private static bool _resourceWrapRepairProbeEnabled = true;
        private static readonly HashSet<string> RawPrayerTitleLogged =
            new HashSet<string>(StringComparer.Ordinal);
        private static readonly HashSet<string> PrayerItemWrapTraceLogged =
            new HashSet<string>(StringComparer.Ordinal);
        private string _status = "F1 opens/closes this console. Synthetic buffs use the game's native BuffsLogics path.";
        private readonly Dictionary<string, int> _spawnedPrayerItems =
            new Dictionary<string, int>(StringComparer.Ordinal);

        private static readonly HashSet<string> PlayerFacingPrayerFamilies =
            new HashSet<string>(StringComparer.Ordinal)
            {
                "b_faith",
                "b_money",
                "b_faith_money",
                "b_plant",
                "b_sins",
                "b_skull",
                "b_sword",
                "b_shield",
                "b_pen",
                "b_star",
                "b_village",
                "b_souls",
                "b_grat_points_incr",
                "b_sin_shard"
            };

        private int _originalPlayerInventorySize = -1;

        private void Awake()
        {
            _log = Logger;
            ResolveBuffApi();
            PatchPrayerItemTitleQualityProbe();
            PatchPrayerItemWrapTrace();
            PatchRootsDiagnostic();
            PatchBoostIISimulation();
            PatchCombatRegenDiagnostic();
            PatchThoroughCleansingDiagnostic();

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

            _effects.Add(new TimedEffect("Soul Contentment (BSS)", "buff_gp_increase", 45f));
            _effects.Add(new TimedEffect(
                "Thorough Cleansing (BSS)",
                "buff_sin_shard",
                36f,
                tier => SetPlayerParam("prayerclarity_rebalanced_sin_shard_tier", tier)));

            Logger.LogInfo(
                PluginName + " " + PluginVersion +
                " loaded. Press F1 for the research-only timed-effect console. " +
                "This tool mutates the current save's live buff list through the native BuffsLogics API and can add prayer items to player inventory; clear synthetic buffs/items before saving a test state you want to keep.");
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
                "PrayerClarity: Rebalanced Test Console " + PluginVersion);
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

            GUILayout.Space(10f);
            GUILayout.Label("Prayer item gallery (SAVE-PERSISTENT until cleanup):");
            GUILayout.Label("Adds only verified player-facing prayer families. Existing copies are not duplicated.");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Give Bronze prayers"))
                GivePrayerItems(1);
            if (GUILayout.Button("Give Silver prayers"))
                GivePrayerItems(2);
            if (GUILayout.Button("Give Gold prayers"))
                GivePrayerItems(3);
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Give ALL prayer items (needs enough inventory space)"))
                GivePrayerItems(0);

            if (GUILayout.Button("Remove prayer items spawned by this console"))
                RemoveSpawnedPrayerItems();

            GUILayout.Space(8f);
            GUILayout.Label("Temporary player inventory capacity (SAVE-PERSISTENT until restored):");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Expand player inventory ×5"))
                ExpandPlayerInventory();
            if (GUILayout.Button("Restore original inventory size"))
                RestorePlayerInventorySize();
            GUILayout.EndHorizontal();

            GUILayout.Space(8f);
            GUILayout.Label("Presentation / access test helpers:");

            if (GUILayout.Button(
                _qualityTitleProbeEnabled
                    ? "Prayer title quality glyph probe: ON"
                    : "Prayer title quality glyph probe: OFF"))
            {
                _qualityTitleProbeEnabled = !_qualityTitleProbeEnabled;
                _status = _qualityTitleProbeEnabled
                    ? "Prayer-title quality glyph probe enabled. Hover a Bronze/Silver/Gold prayer item and check the title."
                    : "Prayer-title quality glyph probe disabled.";
            }

            if (GUILayout.Button(
                _resourceWrapRepairProbeEnabled
                    ? "Prayer item amount+icon wrap repair probe: ON"
                    : "Prayer item amount+icon wrap repair probe: OFF"))
            {
                _resourceWrapRepairProbeEnabled = !_resourceWrapRepairProbeEnabled;
                _status = _resourceWrapRepairProbeEnabled
                    ? "Amount+icon wrap repair probe enabled. Hover Soul's Repose and inspect the resource clusters."
                    : "Amount+icon wrap repair probe disabled.";
            }

            if (GUILayout.Button("Open pulpit sermon UI now (any day / repeat this week)"))
                OpenPulpitSermonNow();

            GUILayout.Label("Opening is nonpersistent. Pressing Pray runs a REAL sermon and changes normal save/game state.");

            GUILayout.Space(8f);
            GUILayout.Label("Native-seam probes:");

            if (GUILayout.Button("Capture live pulpit Soul's Repose identity (no sermon)"))
                CaptureLivePulpitSoulReposeIdentity();

            if (GUILayout.Button("Probe 0.2.17 Soul's Repose conversion (no sermon spent)"))
                ProbeSoulsReposeConversion();

            if (GUILayout.Button("Probe 0.2.18 Soul Contentment decay (temporary items only)"))
                ProbeSoulContentmentPreservation();

            GUILayout.Label("Thorough Cleansing: existing diagnostic retained; 0.2.17 Gold x4 is already accepted evidence unless that seam changes.");

            GUILayout.Space(6f);

            GUILayout.Label("Earlier accepted seam probes:");

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

        private void OpenPulpitSermonNow()
        {
            try
            {
                Type worldMapType = FindType("WorldMap");
                Type worldGameObjectType = FindGameType("WorldGameObject");
                if (worldMapType == null || worldGameObjectType == null)
                    throw new MissingMemberException("WorldMap/WorldGameObject");

                MethodInfo findPulpit = worldMapType.GetMethod(
                    "GetWorldGameObjectByCustomTag",
                    AnyStatic,
                    null,
                    new[] { typeof(string), typeof(bool) },
                    null);
                if (findPulpit == null)
                    throw new MissingMethodException("WorldMap.GetWorldGameObjectByCustomTag(string,bool)");

                object pulpit = findPulpit.Invoke(null, new object[] { "church_pulpit", false });
                if (pulpit == null)
                    throw new InvalidOperationException("church_pulpit WorldGameObject is unavailable.");

                Type guiElementsType = FindType("GUIElements");
                object guiElements = GetStatic(guiElementsType, "me");
                if (guiElements == null)
                    throw new InvalidOperationException("GUIElements.me unavailable.");

                MethodInfo openCraftGui = guiElements.GetType().GetMethod(
                    "OpenCraftGUI",
                    AnyInstance,
                    null,
                    new[] { worldGameObjectType },
                    null);
                if (openCraftGui == null)
                    throw new MissingMethodException("GUIElements.OpenCraftGUI(WorldGameObject)");

                object player = GetPlayer();
                Type mainGameType = FindType("MainGame");
                object mainGame = GetStatic(mainGameType, "me");
                object save = Get(mainGame, "save");
                int day = Convert.ToInt32(Get(save, "day") ?? -1);
                float prayedThisWeek = GetPlayerParam(player, "prayed_this_week", 0f);

                openCraftGui.Invoke(guiElements, new[] { pulpit });

                _status =
                    "Native pulpit sermon UI opened without changing the calendar. " +
                    "If you press Pray, it is a real sermon with normal rewards/effects and save-state changes.";
                _log?.LogInfo(
                    "PULPIT_ANY_DAY_OPENED day=" + day +
                    " prayed_this_week=" + prayedThisWeek.ToString("0.###") +
                    " calendar_mutated=false native_gui=true");
            }
            catch (Exception ex)
            {
                _status = "Any-day pulpit open failed: " + ex.GetType().Name;
                _log?.LogError("PULPIT_ANY_DAY_OPEN_FAILED " + ex);
            }
        }

        private static void PatchPrayerItemWrapTrace()
        {
            Type bubbleWidgetTextType = FindGameType("BubbleWidgetText");
            Type bubbleWidgetTextDataType = FindGameType("BubbleWidgetTextData");
            if (bubbleWidgetTextType == null || bubbleWidgetTextDataType == null)
                throw new MissingMemberException("BubbleWidgetText/BubbleWidgetTextData");

            MethodInfo draw = bubbleWidgetTextType.GetMethod(
                "Draw",
                AnyInstance,
                null,
                new[] { bubbleWidgetTextDataType },
                null);
            if (draw == null)
                throw new MissingMethodException("BubbleWidgetText.Draw(BubbleWidgetTextData)");

            Type harmonyType = FindType("HarmonyLib.Harmony");
            Type harmonyMethodType = FindType("HarmonyLib.HarmonyMethod");
            if (harmonyType == null || harmonyMethodType == null)
                throw new InvalidOperationException("Harmony unavailable.");

            object harmony = Activator.CreateInstance(
                harmonyType,
                new object[] { "nikich.graveyardkeeper.prayerclarity.rebalanced.testconsole.prayeritemwraptrace" });

            MethodInfo postfixMethod = typeof(PrayerClarityRebalancedTestConsole).GetMethod(
                nameof(PrayerItemWrapTracePostfix),
                BindingFlags.NonPublic | BindingFlags.Static);
            object postfix = CreateHarmonyMethod(harmonyMethodType, postfixMethod);
            SetHarmonyAfter(
                harmonyMethodType,
                postfix,
                "nikich.graveyardkeeper.prayerclarity.rebalanced.technologycontentwidth");

            MethodInfo patch = harmonyType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(m =>
                    m.Name == "Patch" &&
                    m.GetParameters().Length >= 5 &&
                    typeof(MethodBase).IsAssignableFrom(m.GetParameters()[0].ParameterType));
            if (patch == null) throw new MissingMethodException("Harmony.Patch");

            object[] args = new object[patch.GetParameters().Length];
            args[0] = draw;
            args[1] = null;
            args[2] = postfix;
            args[3] = null;
            args[4] = null;
            patch.Invoke(harmony, args);
        }

        private static void PrayerItemWrapTracePostfix(object __instance)
        {
            if (__instance == null) return;

            try
            {
                object label = Get(__instance, "_label") ?? Get(__instance, "ui_widget");
                if (label == null) return;

                string raw = Get(label, "text") as string;
                if (string.IsNullOrEmpty(raw) ||
                    (raw.IndexOf("(faith)", StringComparison.Ordinal) < 0 &&
                     raw.IndexOf("(gratitude_points)", StringComparison.Ordinal) < 0))
                    return;

                int width = Convert.ToInt32(Get(label, "width") ?? 0);
                if (width != 200) return;

                string processed = Get(label, "processedText") as string ?? string.Empty;
                object printedObj = Get(label, "printedSize");
                Vector2 printed = printedObj is Vector2 ? (Vector2)printedObj : Vector2.zero;
                string overflow = Convert.ToString(Get(label, "overflowMethod"));

                string signature = width + "|" + raw + "|" + processed;
                if (PrayerItemWrapTraceLogged.Add(signature))
                {
                    _log?.LogInfo(
                        "PRAYER_ITEM_WRAP_TRACE" +
                        " width=" + width +
                        " overflow=" + overflow +
                        " printed=" + printed.x.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) +
                        "x" + printed.y.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) +
                        " raw=\"" + EscapeLogText(raw) + "\"" +
                        " processed=\"" + EscapeLogText(processed) + "\"");
                }

                if (!_resourceWrapRepairProbeEnabled) return;

                string repaired = BuildAmountIconWrapRepair(raw, processed);
                if (string.Equals(repaired, raw, StringComparison.Ordinal)) return;

                Set(label, "text", repaired);
                string repairedProcessed = Get(label, "processedText") as string ?? string.Empty;
                object repairedPrintedObj = Get(label, "printedSize");
                Vector2 repairedPrinted = repairedPrintedObj is Vector2
                    ? (Vector2)repairedPrintedObj
                    : Vector2.zero;

                _log?.LogInfo(
                    "PRAYER_ITEM_WRAP_REPAIR_PROBE" +
                    " before=\"" + EscapeLogText(processed) + "\"" +
                    " raw_after=\"" + EscapeLogText(repaired) + "\"" +
                    " processed_after=\"" + EscapeLogText(repairedProcessed) + "\"" +
                    " printed_after=" +
                    repairedPrinted.x.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) +
                    "x" +
                    repairedPrinted.y.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture));
            }
            catch (Exception ex)
            {
                _log?.LogError("PRAYER_ITEM_WRAP_TRACE_FAILED " + ex);
            }
        }

        private static string BuildAmountIconWrapRepair(string raw, string processed)
        {
            if (string.IsNullOrEmpty(raw) || string.IsNullOrEmpty(processed))
                return raw;

            string normalized = processed.Replace("\r\n", "\n");
            string[] lines = normalized.Split(new[] { '\n' }, StringSplitOptions.None);
            List<int> insertionPoints = new List<int>();

            for (int i = 0; i + 1 < lines.Length; i++)
            {
                string previous = (lines[i] ?? string.Empty).TrimEnd();
                string next = (lines[i + 1] ?? string.Empty).TrimStart();
                if (previous.Length == 0 || next.Length == 0 || next[0] != '(')
                    continue;

                int close = next.IndexOf(')');
                if (close <= 1 || close > 48) continue;
                string symbol = next.Substring(0, close + 1);

                int end = previous.Length - 1;
                while (end >= 0 && char.IsWhiteSpace(previous[end])) end--;
                int start = end;
                while (start >= 0 && char.IsDigit(previous[start])) start--;
                start++;
                if (start > end) continue;

                string amount = previous.Substring(start, end - start + 1);
                int clusterAt = FindAmountSymbolCluster(raw, amount, symbol);
                if (clusterAt <= 0 || raw[clusterAt - 1] == '\n') continue;

                insertionPoints.Add(clusterAt);
            }

            if (insertionPoints.Count == 0) return raw;

            insertionPoints = insertionPoints.Distinct().OrderByDescending(x => x).ToList();
            string result = raw;
            foreach (int index in insertionPoints)
                result = result.Insert(index, "\n");

            return result;
        }

        private static int FindAmountSymbolCluster(string raw, string amount, string symbol)
        {
            string ordinary = amount + " " + symbol;
            int index = raw.IndexOf(ordinary, StringComparison.Ordinal);
            if (index >= 0) return index;

            string noBreak = amount + "\u00A0" + symbol;
            return raw.IndexOf(noBreak, StringComparison.Ordinal);
        }

        private static void PatchPrayerItemTitleQualityProbe()
        {
            Type itemDefinitionType = FindGameType("ItemDefinition");
            Type itemType = FindGameType("Item");
            if (itemDefinitionType == null || itemType == null)
                throw new MissingMemberException("ItemDefinition/Item");

            MethodInfo getTooltipData = itemDefinitionType.GetMethod(
                "GetTooltipData",
                AnyInstance,
                null,
                new[] { itemType, typeof(bool) },
                null);
            if (getTooltipData == null)
                throw new MissingMethodException("ItemDefinition.GetTooltipData(Item,bool)");

            Type harmonyType = FindType("HarmonyLib.Harmony");
            Type harmonyMethodType = FindType("HarmonyLib.HarmonyMethod");
            if (harmonyType == null || harmonyMethodType == null)
                throw new InvalidOperationException("Harmony unavailable.");

            object harmony = Activator.CreateInstance(
                harmonyType,
                new object[] { "nikich.graveyardkeeper.prayerclarity.rebalanced.testconsole.qualitytitleprobe" });

            MethodInfo postfixMethod = typeof(PrayerClarityRebalancedTestConsole).GetMethod(
                nameof(PrayerItemTooltipTitlePostfix),
                BindingFlags.NonPublic | BindingFlags.Static);
            object postfix = CreateHarmonyMethod(harmonyMethodType, postfixMethod);
            SetHarmonyBefore(
                harmonyMethodType,
                postfix,
                "nikich.graveyardkeeper.prayerclarity.rebalanced.itemtooltip");

            MethodInfo patch = harmonyType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(m =>
                    m.Name == "Patch" &&
                    m.GetParameters().Length >= 5 &&
                    typeof(MethodBase).IsAssignableFrom(m.GetParameters()[0].ParameterType));
            if (patch == null) throw new MissingMethodException("Harmony.Patch");

            object[] args = new object[patch.GetParameters().Length];
            args[0] = getTooltipData;
            args[1] = null;
            args[2] = postfix;
            args[3] = null;
            args[4] = null;
            patch.Invoke(harmony, args);
        }

        private static void PrayerItemTooltipTitlePostfix(object __instance, object __result)
        {
            if (__instance == null || __result == null) return;

            try
            {
                string type = Convert.ToString(Get(__instance, "type"));
                if (!string.Equals(type, "Preach", StringComparison.Ordinal)) return;

                int quality = (int)Math.Round(Convert.ToSingle(Get(__instance, "quality") ?? 0f));
                if (quality < 1 || quality > 3) return;

                IList rows = __result as IList;
                if (rows == null || rows.Count == 0 || rows[0] == null) return;

                object titleRow = rows[0];
                string title = Get(titleRow, "text") as string;
                if (string.IsNullOrEmpty(title)) return;

                // Explicit Harmony ordering puts this research postfix before the
                // production item-tooltip postfix, so this is the host-returned title.
                string itemId = GetId(__instance) ?? string.Empty;
                if (itemId.StartsWith("b_faith_money", StringComparison.Ordinal) &&
                    RawPrayerTitleLogged.Add(itemId))
                {
                    _log?.LogInfo(
                        "PRAYER_ITEM_NATIVE_TITLE" +
                        " item_id=" + itemId +
                        " quality=" + quality +
                        " probe_enabled=" + _qualityTitleProbeEnabled +
                        " raw=\"" + EscapeLogText(title) + "\"");
                }

                if (!_qualityTitleProbeEnabled) return;

                string prefix = "(s" + quality + ") ";
                if (title.StartsWith(prefix, StringComparison.Ordinal)) return;

                Set(titleRow, "text", prefix + title);
            }
            catch (Exception ex)
            {
                _log?.LogError("PRAYER_TITLE_QUALITY_GLYPH_PROBE_FAILED " + ex);
            }
        }

        private sealed class PrayerItemDefinition
        {
            internal string Id;
            internal int Quality;
        }

        private void GivePrayerItems(int requestedQuality)
        {
            try
            {
                object player = GetPlayer();
                object data = Get(player, "data");
                if (data == null)
                    throw new InvalidOperationException("MainGame.me.player.data unavailable.");

                Type itemType = FindGameType("Item");
                if (itemType == null)
                    throw new MissingMemberException("Item");

                ConstructorInfo itemCtor = itemType.GetConstructor(
                    AnyInstance,
                    null,
                    new[] { typeof(string), typeof(int) },
                    null);
                MethodInfo addItem = data.GetType().GetMethod(
                    "AddItem",
                    AnyInstance,
                    null,
                    new[] { itemType, typeof(bool) },
                    null);
                MethodInfo getItemsCount = data.GetType().GetMethod(
                    "GetItemsCount",
                    AnyInstance,
                    null,
                    new[] { typeof(string), typeof(bool) },
                    null);

                if (itemCtor == null || addItem == null || getItemsCount == null)
                    throw new MissingMethodException("Player inventory Item/AddItem/GetItemsCount seam.");

                List<PrayerItemDefinition> definitions = FindPrayerItemDefinitions(requestedQuality);
                int added = 0;
                int alreadyPresent = 0;
                int failed = 0;

                foreach (PrayerItemDefinition definition in definitions)
                {
                    int existing = Convert.ToInt32(
                        getItemsCount.Invoke(data, new object[] { definition.Id, true }));
                    if (existing > 0)
                    {
                        alreadyPresent++;
                        continue;
                    }

                    object item = itemCtor.Invoke(new object[] { definition.Id, 1 });
                    bool success = Convert.ToBoolean(
                        addItem.Invoke(data, new object[] { item, true }));

                    if (!success)
                    {
                        failed++;
                        continue;
                    }

                    int count;
                    _spawnedPrayerItems.TryGetValue(definition.Id, out count);
                    _spawnedPrayerItems[definition.Id] = count + 1;
                    added++;
                }

                string tierLabel =
                    requestedQuality == 1 ? "Bronze" :
                    requestedQuality == 2 ? "Silver" :
                    requestedQuality == 3 ? "Gold" : "all qualities";

                _status =
                    "Prayer gallery " + tierLabel +
                    ": added=" + added +
                    ", already present=" + alreadyPresent +
                    ", failed/no space=" + failed +
                    ", discovered=" + definitions.Count + ".";
                _log?.LogInfo(
                    "PRAYER_ITEM_GALLERY" +
                    " quality=" + requestedQuality +
                    " discovered=" + definitions.Count +
                    " added=" + added +
                    " already_present=" + alreadyPresent +
                    " failed=" + failed);
            }
            catch (Exception ex)
            {
                _status = "Prayer item gallery failed: " + ex.GetType().Name;
                _log?.LogError("PRAYER_ITEM_GALLERY_FAILED " + ex);
            }
        }

        private List<PrayerItemDefinition> FindPrayerItemDefinitions(int requestedQuality)
        {
            Type gameBalanceType = FindType("GameBalance");
            object gameBalance = GetStatic(gameBalanceType, "me");
            System.Collections.IEnumerable items =
                Get(gameBalance, "items_data") as System.Collections.IEnumerable;
            if (items == null)
                throw new InvalidOperationException("GameBalance.me.items_data unavailable.");

            List<PrayerItemDefinition> result = new List<PrayerItemDefinition>();
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (object definition in items)
            {
                if (definition == null) continue;

                string type = Convert.ToString(Get(definition, "type"));
                if (!string.Equals(type, "Preach", StringComparison.Ordinal))
                    continue;

                string id = GetId(definition);
                if (string.IsNullOrEmpty(id) || !seen.Add(id))
                    continue;

                int quality = (int)Math.Round(Convert.ToSingle(Get(definition, "quality") ?? 0f));
                if (quality < 1 || quality > 3)
                    continue;
                if (requestedQuality >= 1 && requestedQuality <= 3 && quality != requestedQuality)
                    continue;

                object linkedCraft = Get(definition, "linked_craft");
                string craftId = GetId(linkedCraft);
                if (string.IsNullOrEmpty(craftId) ||
                    !craftId.StartsWith("pray:", StringComparison.Ordinal))
                    continue;

                string family = GetPrayerFamily(craftId);
                if (string.IsNullOrEmpty(family) ||
                    !PlayerFacingPrayerFamilies.Contains(family))
                    continue;

                result.Add(new PrayerItemDefinition
                {
                    Id = id,
                    Quality = quality
                });
            }

            return result
                .OrderBy(x => x.Id, StringComparer.Ordinal)
                .ThenBy(x => x.Quality)
                .ToList();
        }

        private static string GetPrayerFamily(string craftId)
        {
            const string prefix = "pray:";
            if (string.IsNullOrEmpty(craftId) ||
                !craftId.StartsWith(prefix, StringComparison.Ordinal))
                return null;

            string itemId = craftId.Substring(prefix.Length);
            int tierSeparator = itemId.LastIndexOf(':');
            if (tierSeparator <= 0)
                return itemId;

            return itemId.Substring(0, tierSeparator);
        }

        private void ExpandPlayerInventory()
        {
            try
            {
                object player = GetPlayer();
                object data = Get(player, "data");
                if (data == null)
                    throw new InvalidOperationException("MainGame.me.player.data unavailable.");

                int currentSize = Convert.ToInt32(Get(data, "inventory_size") ?? 0);
                if (currentSize <= 0)
                    throw new InvalidOperationException("Player inventory size is unavailable or zero.");

                if (_originalPlayerInventorySize < 0)
                    _originalPlayerInventorySize = currentSize;

                int targetSize = Math.Max(currentSize, _originalPlayerInventorySize * 5);
                MethodInfo setInventorySize = data.GetType().GetMethod(
                    "SetInventorySize",
                    AnyInstance,
                    null,
                    new[] { typeof(int) },
                    null);
                if (setInventorySize == null)
                    throw new MissingMethodException("Item.SetInventorySize(int)");

                bool success = Convert.ToBoolean(
                    setInventorySize.Invoke(data, new object[] { targetSize }));
                if (!success)
                    throw new InvalidOperationException("SetInventorySize returned false.");

                _status =
                    "Player inventory expanded: " +
                    _originalPlayerInventorySize + " -> " + targetSize +
                    ". Restore before saving a permanent playthrough state.";
                _log?.LogInfo(
                    "PLAYER_INVENTORY_TEST_CAPACITY original=" +
                    _originalPlayerInventorySize +
                    " current=" + targetSize);
            }
            catch (Exception ex)
            {
                _status = "Player inventory expansion failed: " + ex.GetType().Name;
                _log?.LogError("PLAYER_INVENTORY_TEST_CAPACITY_FAILED " + ex);
            }
        }

        private void RestorePlayerInventorySize()
        {
            try
            {
                if (_originalPlayerInventorySize < 0)
                {
                    _status = "Player inventory was not expanded by this console in the current session.";
                    return;
                }

                if (_spawnedPrayerItems.Count > 0)
                {
                    _status =
                        "Remove prayer items spawned by this console before restoring inventory size.";
                    return;
                }

                object player = GetPlayer();
                object data = Get(player, "data");
                if (data == null)
                    throw new InvalidOperationException("MainGame.me.player.data unavailable.");

                System.Collections.ICollection inventory =
                    Get(data, "inventory") as System.Collections.ICollection;
                int storedEntries = inventory?.Count ?? 0;
                if (storedEntries > _originalPlayerInventorySize)
                {
                    _status =
                        "Inventory still contains " + storedEntries +
                        " entries, more than original capacity " +
                        _originalPlayerInventorySize +
                        ". Remove/move test items first.";
                    return;
                }

                MethodInfo setInventorySize = data.GetType().GetMethod(
                    "SetInventorySize",
                    AnyInstance,
                    null,
                    new[] { typeof(int) },
                    null);
                if (setInventorySize == null)
                    throw new MissingMethodException("Item.SetInventorySize(int)");

                int restoredSize = _originalPlayerInventorySize;
                bool success = Convert.ToBoolean(
                    setInventorySize.Invoke(data, new object[] { restoredSize }));
                if (!success)
                    throw new InvalidOperationException("SetInventorySize returned false.");

                _originalPlayerInventorySize = -1;
                _status = "Player inventory capacity restored to " + restoredSize + ".";
                _log?.LogInfo(
                    "PLAYER_INVENTORY_TEST_CAPACITY_RESTORED size=" + restoredSize);
            }
            catch (Exception ex)
            {
                _status = "Player inventory restore failed: " + ex.GetType().Name;
                _log?.LogError("PLAYER_INVENTORY_TEST_CAPACITY_RESTORE_FAILED " + ex);
            }
        }

        private void RemoveSpawnedPrayerItems()
        {
            if (_spawnedPrayerItems.Count == 0)
            {
                _status = "No prayer items spawned by this console are tracked.";
                return;
            }

            try
            {
                object player = GetPlayer();
                object data = Get(player, "data");
                if (data == null)
                    throw new InvalidOperationException("MainGame.me.player.data unavailable.");

                Type itemType = FindGameType("Item");
                if (itemType == null)
                    throw new MissingMemberException("Item");

                MethodInfo removeItem = data.GetType().GetMethod(
                    "RemoveItem",
                    AnyInstance,
                    null,
                    new[] { typeof(string), typeof(int), itemType },
                    null);
                if (removeItem == null)
                    throw new MissingMethodException("Player inventory RemoveItem(string,int,Item) seam.");

                int removed = 0;
                List<string> completed = new List<string>();

                foreach (KeyValuePair<string, int> pair in _spawnedPrayerItems)
                {
                    bool success = Convert.ToBoolean(
                        removeItem.Invoke(data, new object[] { pair.Key, pair.Value, null }));
                    if (!success) continue;

                    removed += pair.Value;
                    completed.Add(pair.Key);
                }

                foreach (string id in completed)
                    _spawnedPrayerItems.Remove(id);

                _status =
                    "Removed " + removed + " prayer item(s) spawned by this console. " +
                    (_spawnedPrayerItems.Count == 0
                        ? "Cleanup complete."
                        : _spawnedPrayerItems.Count + " tracked item id(s) could not be removed; do not save this test state yet.");

                _log?.LogInfo(
                    "PRAYER_ITEM_GALLERY_CLEANUP removed=" + removed +
                    " remaining_ids=" + _spawnedPrayerItems.Count);
            }
            catch (Exception ex)
            {
                _status = "Prayer item cleanup failed: " + ex.GetType().Name;
                _log?.LogError("PRAYER_ITEM_GALLERY_CLEANUP_FAILED " + ex);
            }
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


        private void CaptureLivePulpitSoulReposeIdentity()
        {
            try
            {
                Type guiElementsType = FindType("GUIElements");
                object guiElements = GetStatic(guiElementsType, "me");
                object prayGui = Get(guiElements, "pray_craft");
                if (prayGui == null)
                    throw new InvalidOperationException("GUIElements.me.pray_craft unavailable. Open the pulpit and select Gold Soul's Repose first.");

                object liveCraft = Get(prayGui, "pray_craft");
                if (liveCraft == null)
                    throw new InvalidOperationException("PrayCraftGUI.pray_craft is null. Select Gold Soul's Repose first.");

                object canonicalGold = FindBalanceObjectById("craft_data", "pray:b_souls:3");
                object player = GetPlayer();

                string liveId = GetId(liveCraft) ?? "<null>";
                string linkedEvent = Convert.ToString(Get(liveCraft, "linked_sub_id"));
                string outputs = DescribeOutputs(liveCraft);
                string liveMembers = DescribeRelevantMembers(
                    liveCraft,
                    "id", "sub", "quality", "faith", "money", "buff", "name", "item", "craft");
                string guiMembers = DescribeRelevantMembers(
                    prayGui,
                    "craft", "quality", "item", "selected", "cur");

                string tierSummary = "<unavailable>";
                Type forecastType = FindType("PrayerClarity.PrayerForecast");
                MethodInfo buildTier = forecastType == null
                    ? null
                    : forecastType.GetMethods(AnyStatic).FirstOrDefault(m =>
                    {
                        if (m.Name != "BuildTierDetails") return false;
                        ParameterInfo[] p = m.GetParameters();
                        return p.Length == 1;
                    });
                if (buildTier != null)
                {
                    object tier = buildTier.Invoke(null, new[] { liveCraft });
                    tierSummary = tier == null
                        ? "<null>"
                        : "CraftId=" + Convert.ToString(Get(tier, "CraftId")) +
                          ", EventId=" + Convert.ToString(Get(tier, "EventId")) +
                          ", QualityTier=" + Convert.ToString(Get(tier, "QualityTier")) +
                          ", UsesSoulGratitude=" + Convert.ToString(Get(tier, "UsesSoulGratitude")) +
                          ", SoulGratitudeFaithCap=" + Convert.ToString(Get(tier, "SoulGratitudeFaithCap"));
                }

                float gratitude = player == null
                    ? float.NaN
                    : Convert.ToSingle(Get(player, "gratitude_points"));

                bool sameAsCanonicalGold = canonicalGold != null && ReferenceEquals(liveCraft, canonicalGold);
                string canonicalSummary = canonicalGold == null
                    ? "<not found>"
                    : "id=" + (GetId(canonicalGold) ?? "<null>") +
                      ", linked_sub_id=" + Convert.ToString(Get(canonicalGold, "linked_sub_id")) +
                      ", needs_quality=" + Convert.ToString(Get(canonicalGold, "needs_quality")) +
                      ", k_faith=" + Convert.ToString(Get(canonicalGold, "k_faith")) +
                      ", output=" + DescribeOutputs(canonicalGold);

                string diagnostic =
                    "live_type=" + liveCraft.GetType().FullName +
                    ", live_assembly=" + liveCraft.GetType().Assembly.GetName().Name +
                    ", live_id=" + liveId +
                    ", linked_sub_id=" + linkedEvent +
                    ", needs_quality=" + Convert.ToString(Get(liveCraft, "needs_quality")) +
                    ", k_faith=" + Convert.ToString(Get(liveCraft, "k_faith")) +
                    ", k_money=" + Convert.ToString(Get(liveCraft, "k_money")) +
                    ", buff=" + Convert.ToString(Get(liveCraft, "buff")) +
                    ", output=" + outputs +
                    ", same_ref_as_canonical_gold=" + sameAsCanonicalGold +
                    ", gratitude=" + gratitude.ToString("0.###") +
                    " | production_tier=" + tierSummary +
                    " | canonical_gold={" + canonicalSummary + "}" +
                    " | live_members={" + liveMembers + "}" +
                    " | pray_gui_members={" + guiMembers + "}";

                _status = "Live pulpit identity captured. Send LogOutput.log; no sermon was spent.";
                _log?.LogInfo("SOULS_REPOSE_LIVE_PULPIT_IDENTITY " + diagnostic);
            }
            catch (Exception ex)
            {
                _status = "Live pulpit identity capture failed: " + ex.GetType().Name;
                _log?.LogError("PrayerClarity Rebalanced Test Console live pulpit identity capture failed. " + ex);
            }
        }

        private static string DescribeOutputs(object craft)
        {
            System.Collections.IEnumerable output = Get(craft, "output") as System.Collections.IEnumerable;
            if (output == null) return "<null>";

            var parts = new List<string>();
            foreach (object item in output)
            {
                if (item == null)
                {
                    parts.Add("<null>");
                    continue;
                }

                string id = GetId(item) ?? "<null>";
                object value = Get(item, "value");
                object quality = Get(item, "quality");
                parts.Add(
                    id +
                    ":value=" + Convert.ToString(value) +
                    (quality == null ? string.Empty : ":quality=" + Convert.ToString(quality)));
            }

            return parts.Count == 0 ? "<empty>" : string.Join(",", parts.ToArray());
        }

        private static string DescribeRelevantMembers(object instance, params string[] nameFragments)
        {
            if (instance == null) return "<null>";

            var parts = new List<string>();
            var seen = new HashSet<string>(StringComparer.Ordinal);

            for (Type type = instance.GetType(); type != null; type = type.BaseType)
            {
                foreach (FieldInfo field in type.GetFields(AnyInstance | BindingFlags.DeclaredOnly))
                {
                    if (!IsRelevantMemberName(field.Name, nameFragments) || !seen.Add(field.Name)) continue;
                    try { parts.Add(field.Name + "=" + DescribeValue(field.GetValue(instance))); }
                    catch { parts.Add(field.Name + "=<error>"); }
                }

                foreach (PropertyInfo property in type.GetProperties(AnyInstance | BindingFlags.DeclaredOnly))
                {
                    if (property.GetIndexParameters().Length != 0 ||
                        !property.CanRead ||
                        !IsRelevantMemberName(property.Name, nameFragments) ||
                        !seen.Add(property.Name))
                        continue;

                    try { parts.Add(property.Name + "=" + DescribeValue(property.GetValue(instance, null))); }
                    catch { parts.Add(property.Name + "=<error>"); }
                }
            }

            return parts.Count == 0 ? "<none>" : string.Join(", ", parts.ToArray());
        }

        private static bool IsRelevantMemberName(string name, string[] fragments)
        {
            if (string.IsNullOrEmpty(name) || fragments == null) return false;
            foreach (string fragment in fragments)
                if (!string.IsNullOrEmpty(fragment) &&
                    name.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            return false;
        }

        private static string DescribeValue(object value)
        {
            if (value == null) return "<null>";

            Type type = value.GetType();
            if (type.IsPrimitive || type.IsEnum || value is string || value is decimal)
                return Convert.ToString(value);

            string id = GetId(value);
            return type.FullName + (string.IsNullOrEmpty(id) ? string.Empty : "(id=" + id + ")");
        }

        private void ProbeSoulsReposeConversion()
        {
            object player = GetPlayer();
            if (player == null)
            {
                _status = "Soul's Repose probe failed: player unavailable.";
                return;
            }

            try
            {
                object craft = FindBalanceObjectById("craft_data", "pray:b_souls:3");
                if (craft == null) throw new MissingMemberException("CraftDefinition pray:b_souls:3");

                string projectedEventId = Convert.ToString(Get(craft, "linked_sub_id"));
                float requirement = Convert.ToSingle(Get(craft, "needs_quality"));
                float kFaith = Convert.ToSingle(Get(craft, "k_faith"));
                if (!string.Equals(projectedEventId, "default_3", StringComparison.Ordinal) ||
                    Math.Abs(requirement - 90f) > 0.0001f ||
                    Math.Abs(kFaith) > 0.0001f)
                    throw new InvalidOperationException(
                        "Soul's Repose Gold projection is not the expected 0.2.18 shape: event=" +
                        projectedEventId + ", requirement=" + requirement + ", k_faith=" + kFaith + ".");

                // Deliberately feed the old stock Souls event into the real patched
                // CalculatePray call. 0.2.18 must replace this call-local argument from
                // the selected b_souls tier, which is the natural pulpit failure found
                // in 0.2.17. The probe does not manufacture the corrected result.
                const string probeInputEventId = "pray_for_souls_3";

                Type guiElementsType = FindType("GUIElements");
                object guiElements = GetStatic(guiElementsType, "me");
                object prayGui = Get(guiElements, "pray_craft");
                if (prayGui == null) throw new InvalidOperationException("GUIElements.me.pray_craft unavailable.");

                Type prayLogics = FindType("PrayLogics");
                MethodInfo calculate = prayLogics?.GetMethod(
                    "CalculatePray",
                    AnyStatic,
                    null,
                    new[] { typeof(string) },
                    null);
                if (calculate == null) throw new MissingMethodException("PrayLogics.CalculatePray(string)");

                object originalCraft = Get(prayGui, "pray_craft");
                object originalQuality = Get(prayGui, "_cur_quality");
                float originalGratitude = Convert.ToSingle(Get(player, "gratitude_points"));
                object originalLastResult = GetStatic(prayLogics, "last_pray_result");
                object originalDrops = GetStatic(prayLogics, "_sermon_drops");

                var diagnostics = new List<string>();
                var baseFaiths = new List<int>();
                try
                {
                    Set(prayGui, "pray_craft", craft);

                    RunSoulsReposeCase(
                        calculate, prayGui, player, probeInputEventId,
                        73f, 999f, true, 73, 0f, "below_cap", diagnostics, baseFaiths);
                    RunSoulsReposeCase(
                        calculate, prayGui, player, probeInputEventId,
                        136f, 999f, true, 90, 46f, "above_cap", diagnostics, baseFaiths);
                    RunSoulsReposeCase(
                        calculate, prayGui, player, probeInputEventId,
                        0f, 999f, true, 0, 0f, "zero_sg", diagnostics, baseFaiths);
                    RunSoulsReposeCase(
                        calculate, prayGui, player, probeInputEventId,
                        73f, 0f, false, 0, 73f, "failure", diagnostics, baseFaiths);
                }
                finally
                {
                    Set(player, "gratitude_points", originalGratitude);
                    Set(prayGui, "pray_craft", originalCraft);
                    Set(prayGui, "_cur_quality", originalQuality);
                    SetStatic(prayLogics, "last_pray_result", originalLastResult);
                    SetStatic(prayLogics, "_sermon_drops", originalDrops);
                }

                if (baseFaiths.Count != 4 || baseFaiths.Distinct().Count() != 1)
                    throw new InvalidOperationException(
                        "Soul's Repose base Faith still varies with Soul Gratitude: " +
                        string.Join(",", baseFaiths.Select(v => v.ToString()).ToArray()));

                _status = "Soul's Repose live-event probe passed: stock Souls input was normalized, base Faith stayed SG-independent, conversion/spend cases passed.";
                _log?.LogInfo(
                    "SOULS_REPOSE_PROBE PASS input_event=" + probeInputEventId +
                    " effective_base_faith=" + baseFaiths[0] +
                    " | " + string.Join(" | ", diagnostics.ToArray()));
            }
            catch (Exception ex)
            {
                _status = "Soul's Repose probe failed: " + ex.GetType().Name;
                _log?.LogError("PrayerClarity Rebalanced Test Console Soul's Repose probe failed. " + ex);
            }
        }

        private static void RunSoulsReposeCase(
            MethodInfo calculate,
            object prayGui,
            object player,
            string eventId,
            float startGratitude,
            float churchQuality,
            bool expectedSuccess,
            int expectedBonusFaith,
            float expectedRemainingGratitude,
            string label,
            List<string> diagnostics,
            List<int> baseFaiths)
        {
            Set(player, "gratitude_points", startGratitude);
            Set(prayGui, "_cur_quality", churchQuality);

            object result = calculate.Invoke(null, new object[] { eventId });
            bool success = Convert.ToBoolean(Get(result, "success"));
            int faithBonus = Convert.ToInt32(Get(result, "faith_bonus"));
            int baseFaith = Convert.ToInt32(Get(result, "faith"));
            float remaining = Convert.ToSingle(Get(player, "gratitude_points"));

            bool pass =
                success == expectedSuccess &&
                faithBonus == expectedBonusFaith &&
                Math.Abs(remaining - expectedRemainingGratitude) <= 0.0001f;

            string diagnostic =
                label +
                ": success=" + success +
                ", base_faith=" + baseFaith +
                ", faith_bonus=" + faithBonus +
                ", SG=" + startGratitude.ToString("0.###") + "->" + remaining.ToString("0.###") +
                ", expected_bonus=" + expectedBonusFaith +
                ", pass=" + pass;
            diagnostics.Add(diagnostic);
            baseFaiths.Add(baseFaith);
            _log?.LogInfo("SOULS_REPOSE_DIAGNOSTIC " + diagnostic);

            if (!pass)
                throw new InvalidOperationException("Soul's Repose case failed: " + diagnostic);
        }

        private void ProbeSoulContentmentPreservation()
        {
            if (IsActive("buff_gp_increase"))
            {
                _status = "Contentment decay probe requires buff_gp_increase to be inactive first; use Remove on Soul Contentment.";
                return;
            }

            try
            {
                object soulDefinition = FindDecayItemDefinition("Soul");
                object soulBodyPartDefinition = FindDecayItemDefinition("SoulBodyPart");
                object bodyDefinition = FindDecayItemDefinition("Body");
                if (soulDefinition == null || soulBodyPartDefinition == null || bodyDefinition == null)
                    throw new InvalidOperationException("Could not find decaying Soul, SoulBodyPart and Body definitions in GameBalance.items_data.");

                Type itemType = FindGameType("Item");
                ConstructorInfo ctor = itemType?.GetConstructor(
                    AnyInstance,
                    null,
                    new[] { typeof(string), typeof(int) },
                    null);
                MethodInfo updateDurability = itemType?.GetMethods(AnyInstance)
                    .FirstOrDefault(m =>
                    {
                        if (m.Name != "UpdateDurability") return false;
                        ParameterInfo[] p = m.GetParameters();
                        return p.Length == 2 &&
                               p[0].ParameterType == typeof(float) &&
                               p[1].ParameterType == typeof(float);
                    });
                if (ctor == null || updateDurability == null)
                    throw new MissingMethodException(
                        "Assembly-CSharp Item(string,int) / Item.UpdateDurability(float,float)");

                string[] labels = { "Soul", "SoulBodyPart", "Body" };
                object[] definitions = { soulDefinition, soulBodyPartDefinition, bodyDefinition };
                float[] baseline = new float[3];
                float[] protectedValues = new float[3];

                for (int i = 0; i < definitions.Length; i++)
                    baseline[i] = RunTemporaryDurabilityTick(ctor, updateDurability, definitions[i]);

                try
                {
                    _addBuff("buff_gp_increase", 1f);
                    if (!IsActive("buff_gp_increase"))
                        throw new InvalidOperationException("Native buff_gp_increase activation failed.");

                    for (int i = 0; i < definitions.Length; i++)
                        protectedValues[i] = RunTemporaryDurabilityTick(ctor, updateDurability, definitions[i]);
                }
                finally
                {
                    if (IsActive("buff_gp_increase"))
                        _removeBuff("buff_gp_increase");
                }

                bool soulPass = baseline[0] < 0.9999f && protectedValues[0] >= 0.9999f;
                bool bodyPartPass = baseline[1] < 0.9999f && protectedValues[1] >= 0.9999f;
                bool bodyStillDecays =
                    baseline[2] < 0.9999f &&
                    Math.Abs(protectedValues[2] - baseline[2]) <= 0.0001f;

                string diagnostic =
                    labels[0] + "=" + baseline[0].ToString("0.###") + "->" + protectedValues[0].ToString("0.###") +
                    " " + labels[1] + "=" + baseline[1].ToString("0.###") + "->" + protectedValues[1].ToString("0.###") +
                    " " + labels[2] + "=" + baseline[2].ToString("0.###") + "->" + protectedValues[2].ToString("0.###") +
                    " soul_pass=" + soulPass +
                    " bodypart_pass=" + bodyPartPass +
                    " body_still_decays=" + bodyStillDecays;

                _log?.LogInfo("SOUL_CONTENTMENT_DECAY_DIAGNOSTIC " + diagnostic);

                if (!soulPass || !bodyPartPass || !bodyStillDecays)
                    throw new InvalidOperationException("Soul Contentment decay invariant failed: " + diagnostic);

                _status = "Soul Contentment decay probe passed: extracted + in-body souls preserved; ordinary body decay unchanged.";
            }
            catch (Exception ex)
            {
                _status = "Soul Contentment decay probe failed: " + ex.GetType().Name;
                _log?.LogError("PrayerClarity Rebalanced Test Console Soul Contentment decay probe failed. " + ex);
            }
        }

        private static float RunTemporaryDurabilityTick(
            ConstructorInfo ctor,
            MethodInfo updateDurability,
            object definition)
        {
            string id = GetId(definition);
            object item = ctor.Invoke(new object[] { id, 1 });
            Set(item, "durability", 1f);
            updateDurability.Invoke(item, new object[] { 60f, 1f });
            return Convert.ToSingle(Get(item, "durability"));
        }

        private static object FindDecayItemDefinition(string itemTypeName)
        {
            Type gameBalanceType = FindType("GameBalance");
            object gameBalance = GetStatic(gameBalanceType, "me");
            System.Collections.IEnumerable items = Get(gameBalance, "items_data") as System.Collections.IEnumerable;
            if (items == null) return null;

            foreach (object definition in items)
            {
                if (definition == null) continue;
                string type = Convert.ToString(Get(definition, "type"));
                float decrease = Convert.ToSingle(Get(definition, "durability_decrease") ?? 0f);
                if (string.Equals(type, itemTypeName, StringComparison.Ordinal) && decrease > 0.000001f)
                    return definition;
            }
            return null;
        }

        private static object FindBalanceObjectById(string collectionMember, string id)
        {
            Type gameBalanceType = FindType("GameBalance");
            object gameBalance = GetStatic(gameBalanceType, "me");
            System.Collections.IEnumerable values = Get(gameBalance, collectionMember) as System.Collections.IEnumerable;
            if (values == null) return null;

            foreach (object value in values)
                if (string.Equals(GetId(value), id, StringComparison.Ordinal))
                    return value;
            return null;
        }

        private sealed class ThoroughCleansingProbeState
        {
            internal string CraftId;
            internal int BaseShards;
            internal int Tier;
            internal bool Active;
        }

        private static void PatchThoroughCleansingDiagnostic()
        {
            Type widget = FindType("SoulHealingWidget");
            MethodInfo heal = widget?.GetMethod(
                "OnStartHealButtonPressed",
                AnyInstance,
                null,
                Type.EmptyTypes,
                null);
            if (heal == null) throw new MissingMethodException("SoulHealingWidget.OnStartHealButtonPressed()");

            Type harmonyType = FindType("HarmonyLib.Harmony");
            Type harmonyMethodType = FindType("HarmonyLib.HarmonyMethod");
            if (harmonyType == null || harmonyMethodType == null)
                throw new InvalidOperationException("Harmony unavailable.");

            object harmony = Activator.CreateInstance(
                harmonyType,
                new object[] { "nikich.graveyardkeeper.prayerclarity.rebalanced.testconsole.cleansingdiag" });

            MethodInfo prefixMethod = typeof(PrayerClarityRebalancedTestConsole).GetMethod(
                nameof(ThoroughHealingPrefix),
                BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo postfixMethod = typeof(PrayerClarityRebalancedTestConsole).GetMethod(
                nameof(ThoroughHealingPostfix),
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
            args[0] = heal;
            args[1] = prefix;
            args[2] = postfix;
            args[3] = null;
            args[4] = null;
            patch.Invoke(harmony, args);
        }

        private static void ThoroughHealingPrefix(object __instance, ref ThoroughCleansingProbeState __state)
        {
            __state = null;
            try
            {
                object inserted = Get(__instance, "inserted_item");
                object wgo = Get(__instance, "_wgo");
                if (inserted == null || wgo == null) return;

                MethodInfo getParamInt = inserted.GetType().GetMethod(
                    "GetParamInt",
                    AnyInstance,
                    null,
                    new[] { typeof(string) },
                    null);
                if (getParamInt == null) return;

                int sins = Convert.ToInt32(getParamInt.Invoke(inserted, new object[] { "sins_count" }));
                float durability = Convert.ToSingle(Get(inserted, "durability"));
                float healRate = Convert.ToSingle(Get(__instance, "sins_heal_rate"));
                float resultingDurability =
                    durability - (Math.Abs(healRate + 1f) <= 0.0001f ? 0f : 0.5f * (1f - healRate));
                int baseShards = resultingDurability > 0f ? sins : 0;

                object player = GetPlayer();
                int tier = (int)Math.Round(
                    GetPlayerParam(player, "prayerclarity_rebalanced_sin_shard_tier", 0f));

                __state = new ThoroughCleansingProbeState
                {
                    CraftId = GetId(wgo) + ":" + GetId(inserted),
                    BaseShards = baseShards,
                    Tier = tier,
                    Active = IsActive("buff_sin_shard")
                };
            }
            catch (Exception ex)
            {
                _log?.LogError("THOROUGH_CLEANSING_DIAGNOSTIC prefix failed. " + ex);
            }
        }

        private static void ThoroughHealingPostfix(ThoroughCleansingProbeState __state)
        {
            if (__state == null) return;
            try
            {
                object craft = FindBalanceObjectById("craft_data", __state.CraftId);
                System.Collections.IEnumerable output =
                    craft == null ? null : Get(craft, "output") as System.Collections.IEnumerable;

                int shards = -1;
                if (output != null)
                {
                    foreach (object item in output)
                    {
                        if (!string.Equals(GetId(item), "sin_shard", StringComparison.Ordinal)) continue;
                        shards = Convert.ToInt32(Get(item, "value"));
                        break;
                    }
                }

                int multiplier = __state.Tier == 1 ? 2 : __state.Tier == 2 ? 3 : __state.Tier == 3 ? 4 : 1;
                int expected = __state.Active ? __state.BaseShards * multiplier : __state.BaseShards;
                bool pass = shards == expected;

                _log?.LogInfo(
                    "THOROUGH_CLEANSING_DIAGNOSTIC active=" + __state.Active +
                    " tier=" + __state.Tier +
                    " base_shards=" + __state.BaseShards +
                    " expected_multiplier=x" + multiplier +
                    " actual_shards=" + shards +
                    " expected_shards=" + expected +
                    " pass=" + pass);

                if (!pass)
                    _log?.LogError("THOROUGH_CLEANSING_DIAGNOSTIC mismatch.");
            }
            catch (Exception ex)
            {
                _log?.LogError("THOROUGH_CLEANSING_DIAGNOSTIC postfix failed. " + ex);
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

        private static void SetHarmonyBefore(Type harmonyMethodType, object harmonyMethod, params string[] ownerIds)
        {
            SetHarmonyOrder(harmonyMethodType, harmonyMethod, "before", ownerIds);
        }

        private static void SetHarmonyAfter(Type harmonyMethodType, object harmonyMethod, params string[] ownerIds)
        {
            SetHarmonyOrder(harmonyMethodType, harmonyMethod, "after", ownerIds);
        }

        private static void SetHarmonyOrder(
            Type harmonyMethodType,
            object harmonyMethod,
            string memberName,
            params string[] ownerIds)
        {
            if (harmonyMethodType == null || harmonyMethod == null) return;

            FieldInfo field = harmonyMethodType.GetField(
                memberName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(harmonyMethod, ownerIds);
                return;
            }

            PropertyInfo property = harmonyMethodType.GetProperty(
                memberName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (property != null && property.CanWrite)
                property.SetValue(harmonyMethod, ownerIds, null);
        }

        private static string EscapeLogText(string value)
        {
            return (value ?? string.Empty)
                .Replace("\\", "\\\\")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\"", "\\\"");
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


        private static void Set(object instance, string name, object value)
        {
            if (instance == null || string.IsNullOrEmpty(name))
                throw new ArgumentNullException(instance == null ? nameof(instance) : nameof(name));

            for (Type type = instance.GetType(); type != null; type = type.BaseType)
            {
                FieldInfo field = type.GetField(
                    name,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (field != null)
                {
                    field.SetValue(instance, value);
                    return;
                }

                PropertyInfo property = type.GetProperty(
                    name,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (property != null && property.CanWrite && property.GetIndexParameters().Length == 0)
                {
                    property.SetValue(instance, value, null);
                    return;
                }
            }

            throw new MissingMemberException(instance.GetType().FullName, name);
        }

        private static void SetStatic(Type type, string name, object value)
        {
            for (Type current = type; current != null; current = current.BaseType)
            {
                FieldInfo field = current.GetField(
                    name,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);
                if (field != null)
                {
                    field.SetValue(null, value);
                    return;
                }

                PropertyInfo property = current.GetProperty(
                    name,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);
                if (property != null && property.CanWrite && property.GetIndexParameters().Length == 0)
                {
                    property.SetValue(null, value, null);
                    return;
                }
            }

            throw new MissingMemberException(type == null ? "<null>" : type.FullName, name);
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

        private static Type FindGameType(string fullOrShortName)
        {
            Assembly assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => string.Equals(a.GetName().Name, "Assembly-CSharp", StringComparison.Ordinal));
            if (assembly == null) return null;

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

            return types.FirstOrDefault(t =>
                t != null &&
                (t.FullName == fullOrShortName || t.Name == fullOrShortName));
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
