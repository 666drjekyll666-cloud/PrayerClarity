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
        public const string PluginName = "PrayerClarity: Rebalanced Neutral Test Console";
        public const string PluginVersion = "0.1.18";

        private static readonly BindingFlags AnyStatic =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        private static readonly BindingFlags AnyInstance =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        private static ManualLogSource _log;

        private Rect _windowRect = new Rect(24f, 24f, 620f, 780f);
        private bool _visible;
        private string _status =
            "F2 opens/closes this neutral setup console. It installs no Harmony patches or research probes.";

        private readonly Dictionary<string, int> _spawnedPrayerItems =
            new Dictionary<string, int>(StringComparer.Ordinal);

        private sealed class TimedPrayerBuff
        {
            internal string Name;
            internal string PrayerFamily;

            internal TimedPrayerBuff(string name, string prayerFamily)
            {
                Name = name;
                PrayerFamily = prayerFamily;
            }
        }

        private static readonly TimedPrayerBuff[] TimedPrayerBuffs =
        {
            new TimedPrayerBuff("Shoots & Roots", "b_plant"),
            new TimedPrayerBuff("Repentance", "b_sins"),
            new TimedPrayerBuff("Repose", "b_skull"),
            new TimedPrayerBuff("Combat", "b_sword"),
            new TimedPrayerBuff("Imagination", "b_pen"),
            new TimedPrayerBuff("Excellence", "b_star"),
            new TimedPrayerBuff("Soul Contentment (BSS)", "b_grat_points_incr"),
            new TimedPrayerBuff("Thorough Cleansing (BSS)", "b_sin_shard")
        };

        private readonly HashSet<string> _syntheticBuffIds =
            new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<string, float> _originalSyntheticParams =
            new Dictionary<string, float>(StringComparer.Ordinal);

        private static Action<string, float?> _addBuff;
        private static Action<string> _removeBuff;
        private static MethodInfo _findBuffById;

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
            Logger.LogInfo(
                PluginName + " " + PluginVersion +
                " loaded. Press F2 for neutral test-state setup/access helpers. " +
                "No Harmony patches, mechanic simulations, presentation rewrites, or diagnostic probes are installed. " +
                "Prayer-gallery items, synthetic prayer buffs/tier tokens, and temporary inventory expansion can persist in save state until cleaned up/restored.");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
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
                "PrayerClarity: Neutral Test Console " + PluginVersion);
        }

        private void DrawWindow(int id)
        {
            GUILayout.Label("Neutral setup/access utility. F2 toggles. Escape closes.");
            GUILayout.Label("No Harmony patches or research probes are installed by this DLL.");

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

            GUILayout.Space(10f);
            GUILayout.Label("Synthetic timed prayer buffs (native BuffsLogics.AddBuff):");
            GUILayout.Label("Blocks activation if a real copy of the same buff is already active. Cleanup restores captured tier params.");

            foreach (TimedPrayerBuff effect in TimedPrayerBuffs)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(effect.Name, GUILayout.Width(220f));
                if (GUILayout.Button("Bronze", GUILayout.Width(72f)))
                    ActivateSyntheticPrayerBuff(effect, 1);
                if (GUILayout.Button("Silver", GUILayout.Width(72f)))
                    ActivateSyntheticPrayerBuff(effect, 2);
                if (GUILayout.Button("Gold", GUILayout.Width(72f)))
                    ActivateSyntheticPrayerBuff(effect, 3);
                if (GUILayout.Button("Remove", GUILayout.Width(72f)))
                    RemoveSyntheticPrayerBuff(effect);
                GUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Remove ALL synthetic prayer buffs"))
                RemoveAllSyntheticPrayerBuffs();

            GUILayout.Label("Synthetic buffs/tier tokens may persist if you save. Use cleanup before saving a permanent playthrough state.");

            GUILayout.Space(10f);
            GUILayout.Label("Temporary player inventory capacity (SAVE-PERSISTENT until restored):");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Expand player inventory ×5"))
                ExpandPlayerInventory();
            if (GUILayout.Button("Restore original inventory size"))
                RestorePlayerInventorySize();
            GUILayout.EndHorizontal();

            GUILayout.Space(10f);
            GUILayout.Label("Native UI access:");

            if (GUILayout.Button("Open pulpit sermon UI now (any day / repeat this week)"))
                OpenPulpitSermonNow();

            GUILayout.Label("Opening is nonpersistent. Pressing Pray runs a REAL sermon and changes normal save/game state.");

            GUILayout.Space(10f);
            GUILayout.Label("Status: " + _status);

            GUI.DragWindow(new Rect(0f, 0f, 10000f, 24f));
        }

        private sealed class PrayerItemDefinition
                {
                    internal string Id;
                    internal int Quality;
                }

        private static void ResolveBuffApi()
        {
            Type buffsLogics = FindType("BuffsLogics");
            if (buffsLogics == null)
                throw new MissingMemberException("BuffsLogics");

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

            if (add == null)
                throw new MissingMethodException("BuffsLogics.AddBuff(string, Nullable<float>)");
            if (remove == null)
                throw new MissingMethodException("BuffsLogics.RemoveBuff(string)");
            if (_findBuffById == null)
                throw new MissingMethodException("BuffsLogics.FindBuffByID(string)");

            _addBuff = (Action<string, float?>)Delegate.CreateDelegate(typeof(Action<string, float?>), add);
            _removeBuff = (Action<string>)Delegate.CreateDelegate(typeof(Action<string>), remove);
        }

        private void ActivateSyntheticPrayerBuff(TimedPrayerBuff effect, int tier)
        {
            try
            {
                if (effect == null || tier < 1 || tier > 3)
                    return;

                object craft = ResolvePrayerCraft(effect.PrayerFamily, tier);
                if (craft == null)
                {
                    _status = effect.Name + " " + TierName(tier) + " prayer craft is unavailable in this game/DLC state.";
                    return;
                }

                string buffId = Convert.ToString(Get(craft, "buff"));
                if (string.IsNullOrEmpty(buffId))
                {
                    _status = effect.Name + " has no timed buff on this tier.";
                    return;
                }

                bool live = IsBuffActive(buffId);
                bool synthetic = _syntheticBuffIds.Contains(buffId);
                if (live && !synthetic)
                {
                    _status = effect.Name + " already has a real active buff. Synthetic activation was blocked to preserve it.";
                    return;
                }

                if (live)
                    _removeBuff(buffId);

                ApplySyntheticTierParams(effect.PrayerFamily, tier);

                float duration = Convert.ToSingle(Get(craft, "dur_parameter") ?? 0f);
                _addBuff(buffId, duration > 0f ? (float?)duration : null);

                if (!IsBuffActive(buffId))
                {
                    RestoreSyntheticTierParams(effect.PrayerFamily);
                    _status = effect.Name + " synthetic buff could not be activated.";
                    return;
                }

                _syntheticBuffIds.Add(buffId);
                _status =
                    effect.Name + " " + TierName(tier) +
                    " synthetic buff activated through native BuffsLogics.AddBuff" +
                    (duration > 0f ? " (" + duration.ToString("0.#") + " min)." : ".");
                _log?.LogInfo(
                    "SYNTHETIC_PRAYER_BUFF_ACTIVATED family=" + effect.PrayerFamily +
                    " tier=" + tier +
                    " buff=" + buffId +
                    " duration=" + duration.ToString("0.###"));
            }
            catch (Exception ex)
            {
                _status = "Synthetic buff activation failed for " + effect?.Name + ": " + ex.GetType().Name;
                _log?.LogError("SYNTHETIC_PRAYER_BUFF_ACTIVATION_FAILED " + ex);
            }
        }

        private void RemoveSyntheticPrayerBuff(TimedPrayerBuff effect)
        {
            if (effect == null) return;

            try
            {
                object craft = ResolvePrayerCraft(effect.PrayerFamily, 1);
                string buffId = craft == null ? null : Convert.ToString(Get(craft, "buff"));
                if (string.IsNullOrEmpty(buffId) || !_syntheticBuffIds.Contains(buffId))
                {
                    _status = effect.Name + " has no synthetic buff tracked by this console.";
                    return;
                }

                if (IsBuffActive(buffId))
                    _removeBuff(buffId);

                _syntheticBuffIds.Remove(buffId);
                RestoreSyntheticTierParams(effect.PrayerFamily);
                _status = effect.Name + " synthetic buff removed and captured tier params restored.";
                _log?.LogInfo("SYNTHETIC_PRAYER_BUFF_REMOVED family=" + effect.PrayerFamily + " buff=" + buffId);
            }
            catch (Exception ex)
            {
                _status = "Synthetic buff removal failed for " + effect.Name + ": " + ex.GetType().Name;
                _log?.LogError("SYNTHETIC_PRAYER_BUFF_REMOVAL_FAILED " + ex);
            }
        }

        private void RemoveAllSyntheticPrayerBuffs()
        {
            try
            {
                int removed = 0;
                foreach (string buffId in _syntheticBuffIds.ToArray())
                {
                    if (IsBuffActive(buffId))
                        _removeBuff(buffId);
                    _syntheticBuffIds.Remove(buffId);
                    removed++;
                }

                foreach (KeyValuePair<string, float> pair in _originalSyntheticParams.ToArray())
                    SetPlayerParam(pair.Key, pair.Value);
                _originalSyntheticParams.Clear();

                _status = "Removed " + removed + " synthetic prayer buff(s) and restored captured tier params.";
                _log?.LogInfo("SYNTHETIC_PRAYER_BUFF_CLEANUP removed=" + removed);
            }
            catch (Exception ex)
            {
                _status = "Synthetic buff cleanup failed: " + ex.GetType().Name;
                _log?.LogError("SYNTHETIC_PRAYER_BUFF_CLEANUP_FAILED " + ex);
            }
        }

        private static bool IsBuffActive(string buffId)
        {
            return !string.IsNullOrEmpty(buffId) &&
                   _findBuffById != null &&
                   _findBuffById.Invoke(null, new object[] { buffId }) != null;
        }

        private object ResolvePrayerCraft(string family, int tier)
        {
            Type gameBalanceType = FindType("GameBalance");
            object gameBalance = GetStatic(gameBalanceType, "me");
            System.Collections.IEnumerable items =
                Get(gameBalance, "items_data") as System.Collections.IEnumerable;
            if (items == null)
                throw new InvalidOperationException("GameBalance.me.items_data unavailable.");

            foreach (object definition in items)
            {
                if (definition == null) continue;
                if (!string.Equals(Convert.ToString(Get(definition, "type")), "Preach", StringComparison.Ordinal))
                    continue;

                int quality = (int)Math.Round(Convert.ToSingle(Get(definition, "quality") ?? 0f));
                if (quality != tier) continue;

                object linkedCraft = Get(definition, "linked_craft");
                string craftId = GetId(linkedCraft);
                if (string.Equals(GetPrayerFamily(craftId), family, StringComparison.Ordinal))
                    return linkedCraft;
            }

            return null;
        }

        private void ApplySyntheticTierParams(string family, int tier)
        {
            switch (family)
            {
                case "b_plant":
                    CaptureAndSetPlayerParam("prayerclarity_rebalanced_plant_tier", tier);
                    CaptureAndSetPlayerParam(
                        "prayerclarity_rebalanced_plant_reduction",
                        tier == 1 ? 0.20f : tier == 2 ? 0.30f : 0.40f);
                    break;
                case "b_sins":
                    CaptureAndSetPlayerParam("prayerclarity_rebalanced_confession_tier", tier);
                    break;
                case "b_skull":
                    CaptureAndSetPlayerParam("prayerclarity_rebalanced_repose_tier", tier);
                    break;
                case "b_sword":
                    CaptureAndSetPlayerParam("prayerclarity_rebalanced_combat_tier", tier);
                    CaptureAndSetPlayerParam(
                        "prayerclarity_rebalanced_combat_regen",
                        tier == 1 ? 1f : tier == 2 ? 2f : 4f);
                    break;
                case "b_star":
                    CaptureAndSetPlayerParam("prayerclarity_rebalanced_excellence_tier", tier);
                    break;
                case "b_sin_shard":
                    CaptureAndSetPlayerParam("prayerclarity_rebalanced_sin_shard_tier", tier);
                    break;
            }
        }

        private void RestoreSyntheticTierParams(string family)
        {
            foreach (string param in SyntheticParamsForFamily(family))
            {
                float original;
                if (!_originalSyntheticParams.TryGetValue(param, out original))
                    continue;
                SetPlayerParam(param, original);
                _originalSyntheticParams.Remove(param);
            }
        }

        private static IEnumerable<string> SyntheticParamsForFamily(string family)
        {
            switch (family)
            {
                case "b_plant":
                    yield return "prayerclarity_rebalanced_plant_tier";
                    yield return "prayerclarity_rebalanced_plant_reduction";
                    yield break;
                case "b_sins":
                    yield return "prayerclarity_rebalanced_confession_tier";
                    yield break;
                case "b_skull":
                    yield return "prayerclarity_rebalanced_repose_tier";
                    yield break;
                case "b_sword":
                    yield return "prayerclarity_rebalanced_combat_tier";
                    yield return "prayerclarity_rebalanced_combat_regen";
                    yield break;
                case "b_star":
                    yield return "prayerclarity_rebalanced_excellence_tier";
                    yield break;
                case "b_sin_shard":
                    yield return "prayerclarity_rebalanced_sin_shard_tier";
                    yield break;
            }
        }

        private void CaptureAndSetPlayerParam(string name, float value)
        {
            if (!_originalSyntheticParams.ContainsKey(name))
                _originalSyntheticParams[name] = GetPlayerParam(GetPlayer(), name, 0f);
            SetPlayerParam(name, value);
        }

        private static void SetPlayerParam(string name, float value)
        {
            object player = GetPlayer();
            if (player == null)
                throw new InvalidOperationException("MainGame.me.player unavailable.");

            MethodInfo setParam = player.GetType().GetMethod(
                "SetParam",
                AnyInstance,
                null,
                new[] { typeof(string), typeof(float) },
                null);
            if (setParam == null)
                throw new MissingMethodException("WorldGameObject.SetParam(string,float)");

            setParam.Invoke(player, new object[] { name, value });
        }

        private static string TierName(int tier)
        {
            return tier == 1 ? "Bronze" : tier == 2 ? "Silver" : "Gold";
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
