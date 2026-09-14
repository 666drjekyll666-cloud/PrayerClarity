using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;

namespace PrayerClarityPulpitLayoutProbe
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("nikich.graveyardkeeper.prayerclarity", BepInDependency.DependencyFlags.HardDependency)]
    public sealed class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "nikich.graveyardkeeper.prayerclarity.pulpitlayoutprobe";
        private const string PluginName = "PrayerClarity Pulpit Layout Probe";
        private const string PluginVersion = "0.1.0";
        private static readonly Guid SupportedMvid = new Guid("6f50b8e7-156b-49ac-bbe8-7505894b2364");
        private static readonly BindingFlags Inst = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly BindingFlags Stat = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private static Assembly _game;
        private static ManualLogSource _log;
        private static string _path;
        private static int _redraw;
        private static bool _fullDumpDone;

        private void Awake()
        {
            _log = Logger;
            try
            {
                _game = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "Assembly-CSharp");
                if (_game == null) throw new InvalidOperationException("Assembly-CSharp unavailable.");
                if (_game.ManifestModule.ModuleVersionId != SupportedMvid)
                    throw new InvalidOperationException("Unsupported game MVID: " + _game.ManifestModule.ModuleVersionId);

                _path = Path.Combine(Paths.BepInExRootPath, "PrayerClarity-pulpit-layout-0.1.0.txt");
                File.WriteAllText(_path,
                    "PrayerClarity pulpit layout probe 0.1.0\r\n" +
                    "Read-only UI geometry/font-symbol diagnostic.\r\n" +
                    "Game MVID: " + SupportedMvid + "\r\n" +
                    "UTC: " + DateTime.UtcNow.ToString("O") + "\r\n\r\n",
                    new UTF8Encoding(false));

                Type prayGui = GameType("PrayCraftGUI");
                Type worldGameObject = GameType("WorldGameObject");
                Patch(Method(prayGui, "Open", new[] { worldGameObject }),
                    StaticMethod(nameof(OpenPrefix)), StaticMethod(nameof(OpenPostfix)));
                Patch(Method(prayGui, "RedrawTextValues", new[] { typeof(float), typeof(float) }),
                    StaticMethod(nameof(RedrawPrefix)), StaticMethod(nameof(RedrawPostfix)));

                Logger.LogInfo("PrayerClarity pulpit layout probe loaded. Open the pulpit and switch several Test Harness prayers. Output: " + _path);
            }
            catch (Exception ex)
            {
                Logger.LogError("PrayerClarity pulpit layout probe disabled: " + ex);
            }
        }

        private static void OpenPrefix(object __instance)
        {
            Snapshot(__instance, "OPEN_PREFIX", !_fullDumpDone);
            _fullDumpDone = true;
        }

        private static void OpenPostfix(object __instance)
        {
            Snapshot(__instance, "OPEN_POSTFIX", false);
        }

        private static void RedrawPrefix(object __instance, float needs_q, float chance)
        {
            _redraw++;
            LabelSnapshot(__instance, "REDRAW_" + _redraw + "_PREFIX needs_q=" + needs_q + " chance=" + chance);
        }

        private static void RedrawPostfix(object __instance, float needs_q, float chance)
        {
            LabelSnapshot(__instance, "REDRAW_" + _redraw + "_POSTFIX needs_q=" + needs_q + " chance=" + chance);
        }

        private static void Snapshot(object gui, string stage, bool full)
        {
            StringBuilder sb = new StringBuilder(8192);
            sb.AppendLine("============================================================");
            sb.AppendLine(stage);
            sb.AppendLine("============================================================");
            AppendLabel(sb, gui);

            if (full)
            {
                sb.AppendLine();
                sb.AppendLine("=== PRAY GUI UI FIELDS ===");
                AppendUiFields(sb, gui);

                GameObject guiGo = GoOf(gui);
                if (guiGo != null)
                {
                    sb.AppendLine();
                    sb.AppendLine("=== PRAY GUI HIERARCHY (depth <= 6) ===");
                    AppendHierarchy(sb, guiGo.transform, 0, 6);
                }

                object label = Get(gui, "l_total_values");
                GameObject labelGo = GoOf(label);
                if (labelGo != null && labelGo.transform.parent != null)
                {
                    sb.AppendLine();
                    sb.AppendLine("=== TOTAL VALUES PARENT SUBTREE (depth <= 4) ===");
                    AppendHierarchy(sb, labelGo.transform.parent, 0, 4);
                }

                sb.AppendLine();
                sb.AppendLine("=== BITMAP FONT SYMBOLS ===");
                AppendSymbols(sb, label);
            }

            sb.AppendLine();
            Write(sb.ToString());
        }

        private static void LabelSnapshot(object gui, string stage)
        {
            StringBuilder sb = new StringBuilder(1400);
            sb.AppendLine("--- " + stage + " ---");
            AppendLabel(sb, gui);
            Write(sb.ToString());
        }

        private static void AppendLabel(StringBuilder sb, object gui)
        {
            object label = Get(gui, "l_total_values");
            sb.AppendLine("l_total_values=" + Describe(label));
            if (label == null) return;

            string[] names = { "width", "height", "pivot", "alignment", "overflowMethod", "fontSize", "depth", "spacingX", "spacingY" };
            foreach (string name in names) sb.AppendLine("  " + name + "=" + S(Get(label, name)));
            sb.AppendLine("  leftAnchor=" + Anchor(Get(label, "leftAnchor")));
            sb.AppendLine("  rightAnchor=" + Anchor(Get(label, "rightAnchor")));
            sb.AppendLine("  topAnchor=" + Anchor(Get(label, "topAnchor")));
            sb.AppendLine("  bottomAnchor=" + Anchor(Get(label, "bottomAnchor")));
            string text = Convert.ToString(Get(label, "text"));
            sb.AppendLine("  text=\"" + (text ?? "").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\"", "\\\"") + "\"");
        }

        private static void AppendUiFields(StringBuilder sb, object gui)
        {
            if (gui == null) return;
            for (Type type = gui.GetType(); type != null; type = type.BaseType)
            {
                foreach (FieldInfo field in type.GetFields(Inst | BindingFlags.DeclaredOnly))
                {
                    object value;
                    try { value = field.GetValue(gui); } catch { continue; }
                    if (GoOf(value) == null) continue;
                    sb.AppendLine(type.Name + "." + field.Name + " -> " + Describe(value));
                }
            }
        }

        private static void AppendHierarchy(StringBuilder sb, Transform t, int depth, int maxDepth)
        {
            if (t == null || depth > maxDepth) return;
            GameObject go = t.gameObject;
            string indent = new string(' ', depth * 2);
            sb.Append(indent).Append(go.name)
                .Append(" | path=").Append(GoPath(go))
                .Append(" | local=").Append(V(t.localPosition))
                .Append(" | scale=").Append(V(t.localScale))
                .Append(" | active=").Append(go.activeInHierarchy)
                .Append(" | layer=").Append(go.layer);

            Component[] components;
            try { components = go.GetComponents<Component>(); } catch { components = new Component[0]; }
            if (components.Length > 0)
            {
                sb.Append(" | components=");
                bool first = true;
                foreach (Component c in components)
                {
                    if (c == null) continue;
                    if (!first) sb.Append(',');
                    first = false;
                    sb.Append(c.GetType().Name);
                    object w = Get(c, "width"), h = Get(c, "height"), p = Get(c, "pivot");
                    if (w != null || h != null || p != null)
                        sb.Append('[').Append(S(w)).Append('x').Append(S(h)).Append(" pivot=").Append(S(p)).Append(']');
                }
            }
            sb.AppendLine();

            if (depth == maxDepth) return;
            for (int i = 0; i < t.childCount; i++) AppendHierarchy(sb, t.GetChild(i), depth + 1, maxDepth);
        }

        private static void AppendSymbols(StringBuilder sb, object label)
        {
            object font = Get(label, "bitmapFont");
            sb.AppendLine("bitmapFont=" + S(font));
            IEnumerable symbols = Get(font, "symbols") as IEnumerable;
            if (symbols == null) { sb.AppendLine("symbols unavailable"); return; }

            int i = 0;
            foreach (object symbol in symbols)
            {
                if (i >= 256) { sb.AppendLine("truncated at 256 symbols"); break; }
                sb.Append("SYMBOL[").Append(i).Append("] sequence=\"")
                    .Append(Convert.ToString(Get(symbol, "sequence"))).Append("\" spriteName=\"")
                    .Append(Convert.ToString(Get(symbol, "spriteName"))).Append("\" length=")
                    .Append(S(Get(symbol, "length"))).AppendLine();
                i++;
            }
            sb.AppendLine("symbol_count_dumped=" + i);
        }

        private static string Describe(object value)
        {
            if (value == null) return "<null>";
            GameObject go = GoOf(value);
            if (go == null) return value.GetType().FullName;
            return value.GetType().Name + " @ " + GoPath(go) +
                   " local=" + V(go.transform.localPosition) +
                   " width=" + S(Get(value, "width")) +
                   " height=" + S(Get(value, "height")) +
                   " pivot=" + S(Get(value, "pivot")) +
                   " depth=" + S(Get(value, "depth"));
        }

        private static string Anchor(object anchor)
        {
            if (anchor == null) return "<null>";
            GameObject target = GoOf(Get(anchor, "target"));
            return "target=" + (target == null ? S(Get(anchor, "target")) : GoPath(target)) +
                   ", relative=" + S(Get(anchor, "relative")) +
                   ", absolute=" + S(Get(anchor, "absolute"));
        }

        private static GameObject GoOf(object value)
        {
            if (value == null) return null;
            GameObject go = value as GameObject;
            if (go != null) return go;
            Component c = value as Component;
            if (c != null) return c.gameObject;
            return Get(value, "gameObject") as GameObject;
        }

        private static string GoPath(GameObject go)
        {
            if (go == null) return "<null>";
            string path = go.name;
            Transform p = go.transform.parent;
            int guard = 0;
            while (p != null && guard++ < 32) { path = p.gameObject.name + "/" + path; p = p.parent; }
            return path;
        }

        private static string V(Vector3 v) { return "(" + v.x.ToString("0.###") + "," + v.y.ToString("0.###") + "," + v.z.ToString("0.###") + ")"; }

        private static string S(object value)
        {
            if (value == null) return "<null>";
            UnityEngine.Object uo = value as UnityEngine.Object;
            return uo == null ? value.ToString() : uo.GetType().Name + "(" + uo.name + ")";
        }

        private static object Get(object obj, string name)
        {
            if (obj == null) return null;
            try
            {
                for (Type type = obj.GetType(); type != null; type = type.BaseType)
                {
                    FieldInfo f = type.GetField(name, Inst);
                    if (f != null) return f.GetValue(obj);
                    PropertyInfo p = type.GetProperty(name, Inst);
                    if (p != null && p.CanRead) return p.GetValue(obj, null);
                }
            }
            catch { }
            return null;
        }

        private static Type GameType(string name)
        {
            Type exact = _game.GetType(name, false);
            if (exact != null) return exact;
            try { return _game.GetTypes().FirstOrDefault(t => t != null && t.Name == name); }
            catch (ReflectionTypeLoadException ex) { return ex.Types.FirstOrDefault(t => t != null && t.Name == name); }
        }

        private static Type AnyType(string name)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    Type type = assembly.GetType(name, false) ?? assembly.GetTypes().FirstOrDefault(t => t != null && t.Name == name);
                    if (type != null) return type;
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Type type = ex.Types.FirstOrDefault(t => t != null && t.Name == name);
                    if (type != null) return type;
                }
                catch { }
            }
            return null;
        }

        private static MethodInfo Method(Type type, string name, Type[] signature)
        {
            return type == null || signature.Any(t => t == null) ? null : type.GetMethod(name, Inst, null, signature, null);
        }

        private static MethodInfo StaticMethod(string name)
        {
            return typeof(Plugin).GetMethod(name, Stat);
        }

        private static void Patch(MethodInfo target, MethodInfo prefix, MethodInfo postfix)
        {
            if (target == null) throw new MissingMethodException("Harmony target missing.");
            Type harmonyType = AnyType("HarmonyLib.Harmony");
            Type harmonyMethodType = AnyType("HarmonyLib.HarmonyMethod");
            if (harmonyType == null || harmonyMethodType == null) throw new InvalidOperationException("Harmony unavailable.");
            object harmony = Activator.CreateInstance(harmonyType, new object[] { PluginGuid });
            object pre = prefix == null ? null : Activator.CreateInstance(harmonyMethodType, new object[] { prefix });
            object post = postfix == null ? null : Activator.CreateInstance(harmonyMethodType, new object[] { postfix });
            MethodInfo patch = harmonyType.GetMethods(Inst).FirstOrDefault(m => m.Name == "Patch" && m.GetParameters().Length >= 5);
            if (patch == null) throw new MissingMethodException("Harmony.Patch");
            object[] args = new object[patch.GetParameters().Length];
            args[0] = target; args[1] = pre; args[2] = post; args[3] = null; args[4] = null;
            patch.Invoke(harmony, args);
        }

        private static void Write(string text)
        {
            try { File.AppendAllText(_path, text, new UTF8Encoding(false)); }
            catch (Exception ex) { _log?.LogWarning("Pulpit layout probe write failed: " + ex.Message); }
        }
    }
}
