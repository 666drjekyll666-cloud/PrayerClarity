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
    public sealed class PrayerClarityPulpitLayoutProbePlugin : BaseUnityPlugin
    {
        private const string PluginGuid = "nikich.graveyardkeeper.prayerclarity.pulpitlayoutprobe";
        private const string PluginName = "PrayerClarity Pulpit Layout Probe";
        private const string PluginVersion = "0.1.0";
        private static readonly Guid SupportedGameMvid = new Guid("6f50b8e7-156b-49ac-bbe8-7505894b2364");
        private static readonly BindingFlags Inst = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly BindingFlags Stat = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private static ManualLogSource _log;
        private static Assembly _gameAssembly;
        private static string _outputPath;
        private static int _redrawIndex;
        private static bool _fullDumpWritten;

        private void Awake()
        {
            _log = Logger;
            try
            {
                _gameAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => string.Equals(a.GetName().Name, "Assembly-CSharp", StringComparison.Ordinal));
                if (_gameAssembly == null)
                {
                    Logger.LogError("Assembly-CSharp unavailable; pulpit layout probe disabled.");
                    return;
                }

                Guid actualMvid = _gameAssembly.ManifestModule.ModuleVersionId;
                if (actualMvid != SupportedGameMvid)
                {
                    Logger.LogWarning("Unsupported Graveyard Keeper build; pulpit layout probe disabled. MVID=" + actualMvid);
                    return;
                }

                _outputPath = Path.Combine(Paths.BepInExRootPath, "PrayerClarity-pulpit-layout-0.1.0.txt");
                File.WriteAllText(_outputPath,
                    "PrayerClarity pulpit layout probe 0.1.0\r\n" +
                    "Read-only UI diagnostic: no save/inventory/prayer mechanics mutation.\r\n" +
                    "Game MVID: " + actualMvid + "\r\n" +
                    "UTC: " + DateTime.UtcNow.ToString("O") + "\r\n\r\n",
                    new UTF8Encoding(false));

                Type prayGui = GameType("PrayCraftGUI");
                Patch(Method(prayGui, "Open", new[] { typeof(Action) }),
                    Method(typeof(PrayerClarityPulpitLayoutProbePlugin), nameof(OpenPrefix), true),
                    Method(typeof(PrayerClarityPulpitLayoutProbePlugin), nameof(OpenPostfix), true));
                Patch(Method(prayGui, "RedrawTextValues", new[] { typeof(float), typeof(float) }),
                    Method(typeof(PrayerClarityPulpitLayoutProbePlugin), nameof(RedrawPrefix), true),
                    Method(typeof(PrayerClarityPulpitLayoutProbePlugin), nameof(RedrawPostfix), true));

                Logger.LogInfo("PrayerClarity pulpit layout probe 0.1.0 loaded. Open the pulpit and switch several Test Harness prayers; diagnostic file: " + _outputPath);
            }
            catch (Exception ex)
            {
                Logger.LogError("PrayerClarity pulpit layout probe initialization failed: " + ex);
            }
        }

        private static void OpenPrefix(object __instance)
        {
            DumpSnapshot(__instance, "OPEN_PREFIX", !_fullDumpWritten);
            _fullDumpWritten = true;
        }

        private static void OpenPostfix(object __instance)
        {
            DumpSnapshot(__instance, "OPEN_POSTFIX", false);
        }

        private static void RedrawPrefix(object __instance, float needs_q, float chance)
        {
            _redrawIndex++;
            DumpLabelOnly(__instance, "REDRAW_" + _redrawIndex + "_PREFIX needs_q=" + needs_q + " chance=" + chance);
        }

        private static void RedrawPostfix(object __instance, float needs_q, float chance)
        {
            DumpLabelOnly(__instance, "REDRAW_" + _redrawIndex + "_POSTFIX needs_q=" + needs_q + " chance=" + chance);
        }

        private static void DumpSnapshot(object gui, string stage, bool full)
        {
            try
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

                    GameObject guiGo = GameObjectOf(gui);
                    if (guiGo != null)
                    {
                        sb.AppendLine();
                        sb.AppendLine("=== PRAY GUI LOCAL HIERARCHY (bounded depth 6) ===");
                        AppendHierarchy(sb, guiGo.transform, 0, 6);
                    }

                    object label = SafeGet(gui, "l_total_values");
                    GameObject labelGo = GameObjectOf(label);
                    if (labelGo != null && labelGo.transform.parent != null)
                    {
                        sb.AppendLine();
                        sb.AppendLine("=== TOTAL VALUES PARENT SUBTREE (bounded depth 4) ===");
                        AppendHierarchy(sb, labelGo.transform.parent, 0, 4);
                    }

                    sb.AppendLine();
                    sb.AppendLine("=== BITMAP FONT SYMBOLS ===");
                    AppendFontSymbols(sb, label);
                }

                sb.AppendLine();
                Append(sb.ToString());
            }
            catch (Exception ex)
            {
                Append("PROBE ERROR at " + stage + ": " + ex + "\r\n");
            }
        }

        private static void DumpLabelOnly(object gui, string stage)
        {
            try
            {
                StringBuilder sb = new StringBuilder(1024);
                sb.AppendLine("--- " + stage + " ---");
                AppendLabel(sb, gui);
                Append(sb.ToString());
            }
            catch (Exception ex)
            {
                Append("PROBE ERROR at " + stage + ": " + ex + "\r\n");
            }
        }

        private static void AppendLabel(StringBuilder sb, object gui)
        {
            object label = SafeGet(gui, "l_total_values");
            sb.AppendLine("l_total_values=" + DescribeUiObject(label));
            if (label == null) return;

            string[] properties =
            {
                "width", "height", "pivot", "alignment", "overflowMethod", "fontSize", "depth",
                "spacingX", "spacingY", "useFloatSpacing", "floatSpacingX", "floatSpacingY"
            };
            foreach (string name in properties)
                sb.AppendLine("  " + name + "=" + Scalar(SafeGet(label, name)));

            sb.AppendLine("  leftAnchor=" + DescribeAnchor(SafeGet(label, "leftAnchor")));
            sb.AppendLine("  rightAnchor=" + DescribeAnchor(SafeGet(label, "rightAnchor")));
            sb.AppendLine("  topAnchor=" + DescribeAnchor(SafeGet(label, "topAnchor")));
            sb.AppendLine("  bottomAnchor=" + DescribeAnchor(SafeGet(label, "bottomAnchor")));
            object text = SafeGet(label, "text");
            sb.AppendLine("  text=" + Quote(text == null ? null : text.ToString().Replace("\r", "\\r").Replace("\n", "\\n")));
        }

        private static void AppendUiFields(StringBuilder sb, object gui)
        {
            if (gui == null) return;
            for (Type type = gui.GetType(); type != null; type = type.BaseType)
            {
                foreach (FieldInfo field in type.GetFields(Inst | BindingFlags.DeclaredOnly))
                {
                    object value;
                    try { value = field.GetValue(gui); }
                    catch { continue; }
                    GameObject go = GameObjectOf(value);
                    if (go == null) continue;
                    sb.AppendLine(type.Name + "." + field.Name + " -> " + DescribeUiObject(value));
                }
            }
        }

        private static void AppendHierarchy(StringBuilder sb, Transform transform, int depth, int maxDepth)
        {
            if (transform == null || depth > maxDepth) return;
            GameObject go = transform.gameObject;
            string indent = new string(' ', depth * 2);
            sb.Append(indent).Append(go.name)
                .Append(" | path=").Append(PathOf(go))
                .Append(" | local=").Append(Vec(transform.localPosition))
                .Append(" | scale=").Append(Vec(transform.localScale))
                .Append(" | active=").Append(go.activeInHierarchy)
                .Append(" | layer=").Append(go.layer);

            Component[] components;
            try { components = go.GetComponents<Component>(); }
            catch { components = new Component[0]; }
            if (components.Length > 0)
            {
                sb.Append(" | components=");
                bool first = true;
                foreach (Component component in components)
                {
                    if (component == null) continue;
                    if (!first) sb.Append(',');
                    first = false;
                    sb.Append(component.GetType().Name);
                    object width = SafeGet(component, "width");
                    object height = SafeGet(component, "height");
                    object pivot = SafeGet(component, "pivot");
                    if (width != null || height != null || pivot != null)
                    {
                        sb.Append('[')
                            .Append(Scalar(width)).Append('x').Append(Scalar(height))
                            .Append(" pivot=").Append(Scalar(pivot)).Append(']');
                    }
                }
            }
            sb.AppendLine();

            if (depth == maxDepth) return;
            for (int i = 0; i < transform.childCount; i++)
                AppendHierarchy(sb, transform.GetChild(i), depth + 1, maxDepth);
        }

        private static void AppendFontSymbols(StringBuilder sb, object label)
        {
            if (label == null)
            {
                sb.AppendLine("label unavailable");
                return;
            }

            object font = SafeGet(label, "bitmapFont");
            sb.AppendLine("bitmapFont=" + Scalar(font));
            if (font == null) return;

            object symbolsObject = SafeGet(font, "symbols");
            IEnumerable symbols = symbolsObject as IEnumerable;
            if (symbols == null)
            {
                sb.AppendLine("symbols unavailable");
                return;
            }

            int index = 0;
            foreach (object symbol in symbols)
            {
                if (index >= 256)
                {
                    sb.AppendLine("symbol list truncated at 256 entries");
                    break;
                }
                sb.Append("SYMBOL[").Append(index).Append("]")
                    .Append(" sequence=").Append(Quote(Convert.ToString(SafeGet(symbol, "sequence"))))
                    .Append(" spriteName=").Append(Quote(Convert.ToString(SafeGet(symbol, "spriteName"))))
                    .Append(" length=").Append(Scalar(SafeGet(symbol, "length")))
                    .AppendLine();
                index++;
            }
            sb.AppendLine("symbol_count_dumped=" + index);
        }

        private static string DescribeUiObject(object value)
        {
            if (value == null) return "<null>";
            GameObject go = GameObjectOf(value);
            if (go == null) return value.GetType().FullName;
            Transform t = go.transform;
            return value.GetType().Name + " @ " + PathOf(go) +
                   " local=" + Vec(t.localPosition) +
                   " scale=" + Vec(t.localScale) +
                   " width=" + Scalar(SafeGet(value, "width")) +
                   " height=" + Scalar(SafeGet(value, "height")) +
                   " pivot=" + Scalar(SafeGet(value, "pivot")) +
                   " depth=" + Scalar(SafeGet(value, "depth"));
        }

        private static string DescribeAnchor(object anchor)
        {
            if (anchor == null) return "<null>";
            object target = SafeGet(anchor, "target");
            GameObject targetGo = GameObjectOf(target);
            return "target=" + (targetGo == null ? Scalar(target) : PathOf(targetGo)) +
                   ", relative=" + Scalar(SafeGet(anchor, "relative")) +
                   ", absolute=" + Scalar(SafeGet(anchor, "absolute"));
        }

        private static GameObject GameObjectOf(object value)
        {
            if (value == null) return null;
            GameObject direct = value as GameObject;
            if (direct != null) return direct;
            Component component = value as Component;
            if (component != null) return component.gameObject;
            try { return SafeGet(value, "gameObject") as GameObject; }
            catch { return null; }
        }

        private static string PathOf(GameObject go)
        {
            if (go == null) return "<null>";
            StringBuilder sb = new StringBuilder(go.name);
            Transform parent = go.transform.parent;
            int guard = 0;
            while (parent != null && guard++ < 32)
            {
                sb.Insert(0, parent.gameObject.name + "/");
                parent = parent.parent;
            }
            return sb.ToString();
        }

        private static string Vec(Vector3 value)
        {
            return "(" + value.x.ToString("0.###") + "," + value.y.ToString("0.###") + "," + value.z.ToString("0.###") + ")";
        }

        private static string Scalar(object value)
        {
            if (value == null) return "<null>";
            UnityEngine.Object unityObject = value as UnityEngine.Object;
            if (unityObject != null) return unityObject.GetType().Name + "(" + unityObject.name + ")";
            return value.ToString();
        }

        private static string Quote(string value)
        {
            return value == null ? "<null>" : "\"" + value.Replace("\"", "\\\"") + "\"";
        }

        private static object SafeGet(object obj, string name)
        {
            if (obj == null) return null;
            try
            {
                for (Type type = obj.GetType(); type != null; type = type.BaseType)
                {
                    FieldInfo field = type.GetField(name, Inst);
                    if (field != null) return field.GetValue(obj);
                    PropertyInfo property = type.GetProperty(name, Inst);
                    if (property != null && property.CanRead) return property.GetValue(obj, null);
                }
            }
            catch { }
            return null;
        }

        private static Type GameType(string name)
        {
            if (_gameAssembly == null) return null;
            Type exact = _gameAssembly.GetType(name, false);
            if (exact != null) return exact;
            try { return _gameAssembly.GetTypes().FirstOrDefault(t => t != null && t.Name == name); }
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
            return type == null ? null : type.GetMethod(name, Inst, null, signature, null);
        }

        private static MethodInfo Method(Type type, string name, bool isStatic)
        {
            if (type == null) return null;
            return type.GetMethods(isStatic ? Stat : Inst).FirstOrDefault(m => m.Name == name);
        }

        private static void Patch(MethodInfo target, MethodInfo prefix, MethodInfo postfix)
        {
            if (target == null) throw new MissingMethodException("Harmony target missing");
            Type harmonyType = AnyType("HarmonyLib.Harmony");
            Type harmonyMethodType = AnyType("HarmonyLib.HarmonyMethod");
            if (harmonyType == null || harmonyMethodType == null) throw new InvalidOperationException("Harmony unavailable");

            object harmony = Activator.CreateInstance(harmonyType, new object[] { PluginGuid });
            object harmonyPrefix = prefix == null ? null : Activator.CreateInstance(harmonyMethodType, new object[] { prefix });
            object harmonyPostfix = postfix == null ? null : Activator.CreateInstance(harmonyMethodType, new object[] { postfix });

            MethodInfo patch = harmonyType.GetMethods(Inst)
                .FirstOrDefault(m => m.Name == "Patch" && m.GetParameters().Length >= 5 && typeof(MethodBase).IsAssignableFrom(m.GetParameters()[0].ParameterType));
            if (patch == null) throw new MissingMethodException("Harmony.Patch");

            object[] args = new object[patch.GetParameters().Length];
            args[0] = target;
            args[1] = harmonyPrefix;
            args[2] = harmonyPostfix;
            args[3] = null;
            args[4] = null;
            patch.Invoke(harmony, args);
        }

        private static void Append(string text)
        {
            if (string.IsNullOrEmpty(_outputPath)) return;
            try { File.AppendAllText(_outputPath, text, new UTF8Encoding(false)); }
            catch (Exception ex) { _log?.LogWarning("Could not append pulpit layout diagnostic: " + ex.Message); }
        }
    }
}
