using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx;
using UnityEngine;

// Read-only research probe; not production code.\nnamespace PrayerClarityResearch
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class PrayerClarityHudTimerProbe : BaseUnityPlugin
    {
        public const string PluginGuid = "prayerclarity.auditprobe.hudtimer";
        public const string PluginName = "PrayerClarity HUD Timer Probe";
        public const string PluginVersion = "0.1.0";

        private Assembly _game;
        private float _readyAt = -1f;
        private bool _completed;

        private void Awake()
        {
            Logger.LogInfo("PrayerClarity HUD Timer Probe 0.1.0 loaded: read-only reflection/UI-prefab audit; no Harmony and no game-state mutation.");
        }

        private void Update()
        {
            if (_completed) return;
            if (_game == null)
            {
                _game = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "Assembly-CSharp");
                if (_game == null) return;
            }

            if (!GameStarted(_game)) return;
            if (_readyAt < 0f)
            {
                _readyAt = Time.realtimeSinceStartup;
                return;
            }
            if (Time.realtimeSinceStartup - _readyAt < 5f) return;

            _completed = true;
            try { Run(); }
            catch (Exception ex) { Logger.LogError("PrayerClarity HUD timer audit failed: " + ex); }
        }

        private void Run()
        {
            StringBuilder sb = new StringBuilder(64 * 1024);
            sb.AppendLine("PRAYERCLARITY - HUD PRAYER TIMER AUDIT");
            sb.AppendLine("ProbeVersion=" + PluginVersion);
            sb.AppendLine("GeneratedUtc=" + DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            sb.AppendLine("GameAssembly=" + _game.FullName);
            sb.AppendLine("ModuleVersionId=" + _game.ManifestModule.ModuleVersionId);
            sb.AppendLine("Contract=READ_ONLY_REFLECTION_UI_PREFAB_NO_HARMONY_NO_MUTATION");
            sb.AppendLine("Question=exact BuffIcon/BuffsGUI HUD timer owner, live timer-label font/geometry, and safe native lifecycle seam");
            sb.AppendLine();

            DumpType(sb, "BuffIcon", new[] { "Draw", "Redraw" });
            DumpType(sb, "BuffsGUI", new[] { "Init", "Redraw", "Update" });
            DumpType(sb, "PlayerBuff", new[] { "GetTimerText" });

            object guiElements = GetStatic(FindType("GUIElements"), "me");
            object buffsGui = Get(guiElements, "buffs");
            object inventory = Get(guiElements, "inventory");

            sb.AppendLine("=== LIVE HUD OWNER ===");
            sb.AppendLine("GUIElements=" + Describe(guiElements));
            sb.AppendLine("BuffsGUI=" + Describe(buffsGui));
            DumpObjectGameObject(sb, "BuffsGUI.gameObject", buffsGui);
            DumpObjectGameObject(sb, "BuffsGUI.buffs_hud", Get(buffsGui, "buffs_hud"));
            DumpObjectGameObject(sb, "BuffsGUI.grid", Get(buffsGui, "grid"));
            sb.AppendLine();

            object hudPrefab = Get(buffsGui, "buff_icon_prefab");
            sb.AppendLine("=== HUD BUFF ICON PREFAB ===");
            sb.AppendLine("Prefab=" + Describe(hudPrefab));
            DumpObjectGameObject(sb, "prefab.gameObject", hudPrefab);
            DumpLabel(sb, "HUD txt_timer", Get(hudPrefab, "txt_timer"));
            DumpObjectGameObject(sb, "HUD icon", Get(hudPrefab, "icon"));
            DumpHierarchy(sb, GameObjectOf(hudPrefab), 0, 3);
            sb.AppendLine();

            object itemPrefab = Get(inventory, "perk_buff_item_prefab");
            sb.AppendLine("=== CHARACTER BUFF ITEM PREFAB COMPARISON ===");
            sb.AppendLine("Prefab=" + Describe(itemPrefab));
            DumpLabel(sb, "Character txt_timer", Get(itemPrefab, "txt_timer"));
            DumpLabel(sb, "Character txt_descr", Get(itemPrefab, "txt_descr"));
            DumpLabel(sb, "Character txt_header", Get(itemPrefab, "txt_header"));
            sb.AppendLine();

            string path = Path.Combine(Paths.BepInExRootPath, "PrayerClarity-hud-timer-0.1.0.txt");
            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
            Logger.LogInfo("PrayerClarity HUD timer audit complete: " + path);
        }

        private static bool GameStarted(Assembly game)
        {
            Type mainGame = FindType(game, "MainGame");
            object value = GetStatic(mainGame, "game_started");
            return value is bool && (bool)value;
        }

        private Type FindType(string name) { return FindType(_game, name); }

        private static Type FindType(Assembly game, string name)
        {
            if (game == null) return null;
            try
            {
                return game.GetTypes().FirstOrDefault(
                    t => t != null && (t.Name == name || t.FullName == name));
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.FirstOrDefault(
                    t => t != null && (t.Name == name || t.FullName == name));
            }
        }

        private void DumpType(StringBuilder sb, string name, string[] methods)
        {
            sb.AppendLine("=== TYPE " + name + " ===");
            Type type = FindType(name);
            if (type == null)
            {
                sb.AppendLine("TYPE_MISSING");
                sb.AppendLine();
                return;
            }

            foreach (FieldInfo field in type.GetFields(
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.Static |
                BindingFlags.DeclaredOnly))
            {
                sb.AppendLine("FIELD " + (field.IsStatic ? "static " : "instance ") +
                              Friendly(field.FieldType) + " " + field.Name);
            }

            foreach (MethodInfo method in type.GetMethods(
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.Static |
                BindingFlags.DeclaredOnly)
                .Where(m => methods.Contains(m.Name))
                .OrderBy(m => m.MetadataToken))
            {
                sb.AppendLine("METHOD " + Signature(method));
            }

            sb.AppendLine("=== END TYPE " + name + " ===");
            sb.AppendLine();
        }

        private static void DumpLabel(StringBuilder sb, string labelName, object label)
        {
            sb.AppendLine("--- " + labelName + " ---");
            sb.AppendLine("Object=" + Describe(label));

            GameObject go = GameObjectOf(label);
            if (go != null)
            {
                sb.AppendLine("GO=" + go.name + " path=" + PathOf(go.transform));
                Vector3 p = go.transform.localPosition;
                sb.AppendLine("localPosition=" + F(p.x) + "," + F(p.y) + "," + F(p.z));
                Vector3 s = go.transform.localScale;
                sb.AppendLine("localScale=" + F(s.x) + "," + F(s.y) + "," + F(s.z));
            }

            string[] props =
            {
                "text", "width", "height", "fontSize", "spacingX", "spacingY",
                "pivot", "alignment", "overflowMethod", "supportEncoding",
                "symbolStyle", "depth", "color"
            };

            foreach (string prop in props)
                sb.AppendLine(prop + "=" + Format(SafeGet(label, prop)));

            object bitmapFont = SafeGet(label, "bitmapFont");
            object trueTypeFont = SafeGet(label, "trueTypeFont");
            sb.AppendLine("bitmapFont=" + DescribeUnity(bitmapFont));
            sb.AppendLine("trueTypeFont=" + DescribeUnity(trueTypeFont));

            if (bitmapFont != null)
            {
                sb.AppendLine("bitmapFont.name=" + Format(SafeGet(bitmapFont, "name")));
                sb.AppendLine("bitmapFont.spriteName=" + Format(SafeGet(bitmapFont, "spriteName")));
                sb.AppendLine("bitmapFont.defaultSize=" + Format(SafeGet(bitmapFont, "defaultSize")));
                sb.AppendLine("bitmapFont.dynamicFont=" + DescribeUnity(SafeGet(bitmapFont, "dynamicFont")));
            }

            sb.AppendLine("--- END " + labelName + " ---");
        }

        private static void DumpObjectGameObject(StringBuilder sb, string name, object obj)
        {
            GameObject go = GameObjectOf(obj);
            sb.AppendLine(name + "=" +
                (go == null ? "<null>" : go.name + " path=" + PathOf(go.transform)));
        }

        private static void DumpHierarchy(StringBuilder sb, GameObject go, int depth, int maxDepth)
        {
            if (go == null || depth > maxDepth) return;

            string indent = new string(' ', depth * 2);
            Component[] components;
            try { components = go.GetComponents<Component>(); }
            catch { components = new Component[0]; }

            sb.AppendLine(indent + "GO " + go.name + " active=" + go.activeSelf +
                          " components=" +
                          string.Join(",", components.Where(c => c != null)
                              .Select(c => c.GetType().FullName).ToArray()));

            Transform t = go.transform;
            for (int i = 0; i < t.childCount; i++)
            {
                Transform child = t.GetChild(i);
                if (child != null)
                    DumpHierarchy(sb, child.gameObject, depth + 1, maxDepth);
            }
        }

        private static object Get(object obj, string name)
        {
            if (obj == null) return null;

            for (Type type = obj.GetType(); type != null; type = type.BaseType)
            {
                FieldInfo field = type.GetField(
                    name,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null) return field.GetValue(obj);

                PropertyInfo property = type.GetProperty(
                    name,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (property != null && property.CanRead &&
                    property.GetIndexParameters().Length == 0)
                    return property.GetValue(obj, null);
            }

            return null;
        }

        private static object SafeGet(object obj, string name)
        {
            try { return Get(obj, name); }
            catch { return null; }
        }

        private static object GetStatic(Type type, string name)
        {
            if (type == null) return null;

            for (Type current = type; current != null; current = current.BaseType)
            {
                FieldInfo field = current.GetField(
                    name,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (field != null) return field.GetValue(null);

                PropertyInfo property = current.GetProperty(
                    name,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (property != null && property.CanRead &&
                    property.GetIndexParameters().Length == 0)
                    return property.GetValue(null, null);
            }

            return null;
        }

        private static GameObject GameObjectOf(object obj)
        {
            if (obj == null) return null;

            GameObject go = obj as GameObject;
            if (go != null) return go;

            Component component = obj as Component;
            if (component != null) return component.gameObject;

            return SafeGet(obj, "gameObject") as GameObject;
        }

        private static string PathOf(Transform t)
        {
            if (t == null) return "<null>";
            string path = t.name;
            for (Transform p = t.parent; p != null; p = p.parent)
                path = p.name + "/" + path;
            return path;
        }

        private static string Describe(object obj)
        {
            return obj == null ? "<null>" : obj.GetType().FullName;
        }

        private static string DescribeUnity(object obj)
        {
            if (obj == null) return "<null>";
            UnityEngine.Object unityObject = obj as UnityEngine.Object;
            return unityObject == null
                ? Describe(obj)
                : obj.GetType().FullName + ":" + unityObject.name;
        }

        private static string Format(object value)
        {
            if (value == null) return "<null>";
            IFormattable formattable = value as IFormattable;
            return formattable != null
                ? formattable.ToString(null, CultureInfo.InvariantCulture)
                : value.ToString();
        }

        private static string F(float value)
        {
            return value.ToString("R", CultureInfo.InvariantCulture);
        }

        private static string Friendly(Type type)
        {
            return type == null ? "<null>" : (type.FullName ?? type.Name);
        }

        private static string Signature(MethodInfo method)
        {
            string pars = string.Join(", ",
                method.GetParameters()
                    .Select(p => Friendly(p.ParameterType) + " " + p.Name)
                    .ToArray());
            return Friendly(method.ReturnType) + " " +
                   Friendly(method.DeclaringType) + "." +
                   method.Name + "(" + pars + ")";
        }
    }
}