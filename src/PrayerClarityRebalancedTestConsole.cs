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
        public const string PluginVersion = "0.1.16";

        private static readonly BindingFlags AnyStatic =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        private static readonly BindingFlags AnyInstance =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        private static ManualLogSource _log;

        private Rect _windowRect = new Rect(24f, 24f, 620f, 520f);
        private bool _visible;
        private string _status =
            "F1 opens/closes this neutral setup console. It installs no Harmony patches or research probes.";

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
            Logger.LogInfo(
                PluginName + " " + PluginVersion +
                " loaded. Press F1 for neutral test-state setup/access helpers. " +
                "No Harmony patches, mechanic simulations, presentation rewrites, or diagnostic probes are installed. " +
                "Prayer-gallery items and temporary inventory expansion are save-persistent until cleaned up/restored.");
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
                "PrayerClarity: Neutral Test Console " + PluginVersion);
        }

        private void DrawWindow(int id)
        {
            GUILayout.Label("Neutral setup/access utility. F1 toggles. Escape closes.");
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
