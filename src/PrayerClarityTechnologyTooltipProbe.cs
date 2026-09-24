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
    public sealed class PrayerClarityTechnologyTooltipProbe : BaseUnityPlugin
    {
        public const string PluginGuid = "nikich.graveyardkeeper.prayerclarity.tooltipprobe";
        public const string RebalancedPluginGuid = "nikich.graveyardkeeper.prayerclarity.rebalanced";
        public const string PluginName = "PrayerClarity: Technology Tooltip Probe";
        public const string PluginVersion = "0.1.0";

        private const int OwnedMaxWidth = 900;
        private const float SafeMarginPixels = 24f;

        private static readonly BindingFlags Inst =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly BindingFlags Stat =
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private static ManualLogSource _log;
        private static Type _uiRootType;
        private static object _targetBubble;
        private static int _targetUpdateIndex;
        private static int _rowSequence;
        private static bool _installed;

        private void Awake()
        {
            _log = Logger;
            Logger.LogInfo(
                PluginName + " " + PluginVersion +
                " loaded. Research-only/read-only: it records Technology tooltip row geometry, final bubble geometry and Harmony patch ownership; it does not change tooltip values or save data.");
        }

        private void Start()
        {
            try
            {
                InstallProbe();
            }
            catch (Exception ex)
            {
                Logger.LogError("Technology tooltip probe installation failed. " + ex);
            }
        }

        private static void InstallProbe()
        {
            if (_installed) return;

            Type bubbleWidgetText = FindType("BubbleWidgetText");
            Type bubbleWidgetTextData = FindType("BubbleWidgetTextData");
            Type tooltip = FindType("Tooltip");
            Type bubble = FindType("WidgetsBubbleGUI");
            _uiRootType = FindType("UIRoot");

            MethodInfo draw = bubbleWidgetText?.GetMethod(
                "Draw",
                Inst,
                null,
                new[] { bubbleWidgetTextData },
                null);
            MethodInfo show = tooltip?.GetMethod(
                "Show",
                Inst,
                null,
                new[] { typeof(bool) },
                null);
            MethodInfo update = bubble?.GetMethod(
                "Update",
                Inst,
                null,
                Type.EmptyTypes,
                null);

            if (draw == null) throw new MissingMethodException("BubbleWidgetText.Draw(BubbleWidgetTextData)");
            if (show == null) throw new MissingMethodException("Tooltip.Show(bool)");
            if (update == null) throw new MissingMethodException("WidgetsBubbleGUI.Update()");
            if (_uiRootType == null) throw new MissingMemberException("UIRoot");

            PatchPostfixAfterExisting(draw, nameof(BubbleWidgetTextDrawPostfix));
            PatchPostfixAfterExisting(show, nameof(TooltipShowPostfix));
            PatchPostfixAfterExisting(update, nameof(WidgetsBubbleUpdatePostfix));

            DumpPatchOwners("BubbleWidgetText.Draw", draw);
            DumpPatchOwners("Tooltip.Show", show);
            DumpPatchOwners("WidgetsBubbleGUI.Update", update);

            _installed = true;
            _log?.LogInfo(
                "TT_PROBE_READY Open Technology -> Spiritualism and highlight the combined Better Save Soul prayer node once with the gamepad. Then close the game and return LogOutput.log.");
        }

        private static void BubbleWidgetTextDrawPostfix(object __instance, object __0)
        {
            try
            {
                if (__instance == null || __0 == null) return;

                int maxWidth = ToInt(Get(__0, "max_width"));
                if (maxWidth != OwnedMaxWidth) return;

                object label = Get(__instance, "_label") ?? Get(__instance, "ui_widget");
                if (label == null)
                {
                    _log?.LogWarning("TT_PROBE_ROW max_width=900 but live label is unavailable.");
                    return;
                }

                string text = Convert.ToString(Get(label, "text")) ?? string.Empty;
                string processed = Convert.ToString(Get(label, "processedText")) ?? string.Empty;
                object printed = Get(label, "printedSize");

                int sequence = ++_rowSequence;
                _log?.LogInfo(
                    "TT_PROBE_ROW seq=" + sequence +
                    " data_type=" + __0.GetType().FullName +
                    " text=\\\"" + Preview(text) + "\\\"" +
                    " label_type=" + label.GetType().FullName +
                    " overflow_method=" + SafeValue(Get(label, "overflowMethod")) +
                    " width=" + SafeValue(Get(label, "width")) +
                    " height=" + SafeValue(Get(label, "height")) +
                    " line_width=" + SafeValue(Get(label, "lineWidth")) +
                    " overflow_width=" + SafeValue(Get(label, "overflowWidth")) +
                    " printed_size=" + VectorValue(printed) +
                    " processed_lines=" + CountLines(processed) +
                    " processed=\\\"" + Preview(processed) + "\\\"" +
                    " hierarchy=\\\"" + ComponentPath(label as Component) + "\\\"");
            }
            catch (Exception ex)
            {
                _log?.LogError("TT_PROBE_ROW failed. " + ex);
            }
        }

        private static void TooltipShowPostfix(object __instance, bool __0)
        {
            if (!__0 || __instance == null) return;

            try
            {
                object data = Get(__instance, "data");
                IEnumerable rows = Get(data, "data_list") as IEnumerable;
                if (rows == null) return;

                var rowDescriptions = new List<string>();
                int total = 0;
                int wide = 0;
                foreach (object row in rows)
                {
                    if (row == null) continue;
                    int maxWidth = ToInt(Get(row, "max_width"));
                    string rowText = Convert.ToString(Get(row, "text")) ?? string.Empty;
                    if (maxWidth == OwnedMaxWidth) wide++;
                    rowDescriptions.Add(
                        total + ":" + row.GetType().Name +
                        ":max=" + maxWidth +
                        ":text=\\\"" + Preview(rowText) + "\\\"");
                    total++;
                }

                if (wide == 0) return;

                object linked = Get(__instance, "linked_tooltip");
                _targetBubble = linked;
                _targetUpdateIndex = 0;

                _log?.LogInfo(
                    "TT_PROBE_SHOW total_rows=" + total +
                    " owned_width_rows=" + wide +
                    " linked_type=" + (linked == null ? "<null>" : linked.GetType().FullName) +
                    " linked_hierarchy=\\\"" + ComponentPath(linked as Component) + "\\\"");

                foreach (string description in rowDescriptions)
                    _log?.LogInfo("TT_PROBE_DATA_ROW " + description);

                // Dump again at first real display, after all BepInEx Awake/Start patching
                // has had a chance to finish. This is the authoritative owner snapshot.
                Type bubbleWidgetText = FindType("BubbleWidgetText");
                Type bubbleWidgetTextData = FindType("BubbleWidgetTextData");
                Type bubbleType = FindType("WidgetsBubbleGUI");
                Type tooltipType = FindType("Tooltip");

                MethodInfo draw = bubbleWidgetText?.GetMethod(
                    "Draw", Inst, null, new[] { bubbleWidgetTextData }, null);
                MethodInfo show = tooltipType?.GetMethod(
                    "Show", Inst, null, new[] { typeof(bool) }, null);
                MethodInfo update = bubbleType?.GetMethod(
                    "Update", Inst, null, Type.EmptyTypes, null);

                DumpPatchOwners("BubbleWidgetText.Draw@show", draw);
                DumpPatchOwners("Tooltip.Show@show", show);
                DumpPatchOwners("WidgetsBubbleGUI.Update@show", update);
            }
            catch (Exception ex)
            {
                _log?.LogError("TT_PROBE_SHOW failed. " + ex);
            }
        }

        private static void WidgetsBubbleUpdatePostfix(object __instance)
        {
            if (__instance == null || _targetBubble == null || !ReferenceEquals(__instance, _targetBubble))
                return;

            try
            {
                if (_targetUpdateIndex >= 8) return;
                _targetUpdateIndex++;

                object widget = Get(__instance, "widget");
                Component bubble = __instance as Component;
                Component root = null;
                if (bubble != null && _uiRootType != null)
                {
                    Transform rootTransform = bubble.transform.root;
                    if (rootTransform != null)
                        root = rootTransform.GetComponent(_uiRootType);
                }

                int width = ToInt(Get(widget, "width"));
                int height = ToInt(Get(widget, "height"));
                int manualHeight = ToInt(Get(root, "manualHeight"));
                int screenWidth = Screen.width;
                int screenHeight = Screen.height;
                Rect safe = Screen.safeArea;

                float scale = screenHeight > 0 && manualHeight > 0
                    ? manualHeight / (float)screenHeight
                    : 0f;
                float margin = SafeMarginPixels * scale;
                float availableWidth = safe.width * scale - 2f * margin;
                float availableHeight = safe.height * scale - 2f * margin;
                Vector3 position = bubble == null ? Vector3.zero : bubble.transform.localPosition;

                _log?.LogInfo(
                    "TT_PROBE_BUBBLE update=" + _targetUpdateIndex +
                    " widget_width=" + width +
                    " widget_height=" + height +
                    " local_pos=(" + F(position.x) + "," + F(position.y) + "," + F(position.z) + ")" +
                    " root_manual_height=" + manualHeight +
                    " screen=" + screenWidth + "x" + screenHeight +
                    " safe_px=(" + F(safe.x) + "," + F(safe.y) + "," + F(safe.width) + "," + F(safe.height) + ")" +
                    " ui_scale=" + F(scale) +
                    " available_ui=(" + F(availableWidth) + "," + F(availableHeight) + ")" +
                    " oversize_x=" + (width > availableWidth) +
                    " oversize_y=" + (height > availableHeight) +
                    " hierarchy=\\\"" + ComponentPath(bubble) + "\\\"");
            }
            catch (Exception ex)
            {
                _log?.LogError("TT_PROBE_BUBBLE failed. " + ex);
                _targetBubble = null;
            }
        }

        private static void PatchPostfixAfterExisting(MethodInfo target, string postfixName)
        {
            Type harmonyType = FindType("HarmonyLib.Harmony");
            Type harmonyMethodType = FindType("HarmonyLib.HarmonyMethod");
            if (harmonyType == null || harmonyMethodType == null)
                throw new InvalidOperationException("Harmony is unavailable.");

            MethodInfo postfix = typeof(PrayerClarityTechnologyTooltipProbe).GetMethod(postfixName, Stat);
            if (postfix == null)
                throw new MissingMethodException(typeof(PrayerClarityTechnologyTooltipProbe).FullName, postfixName);

            string[] owners = GetPatchOwners(target)
                .Where(x => !string.IsNullOrEmpty(x) && !string.Equals(x, PluginGuid, StringComparison.Ordinal))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            object harmonyMethod = CreateHarmonyMethod(harmonyMethodType, postfix);
            SetMemberIfPresent(harmonyMethod, "after", owners);

            object harmony = Activator.CreateInstance(harmonyType, new object[] { PluginGuid });
            MethodInfo patch = harmonyType.GetMethods(Inst)
                .FirstOrDefault(m =>
                    m.Name == "Patch" &&
                    m.GetParameters().Length >= 5 &&
                    typeof(MethodBase).IsAssignableFrom(m.GetParameters()[0].ParameterType));
            if (patch == null) throw new MissingMethodException("Harmony.Patch");

            object[] args = new object[patch.GetParameters().Length];
            args[0] = target;
            args[1] = null;
            args[2] = harmonyMethod;
            args[3] = null;
            args[4] = null;
            patch.Invoke(harmony, args);
        }

        private static object CreateHarmonyMethod(Type harmonyMethodType, MethodInfo method)
        {
            ConstructorInfo ctor = harmonyMethodType.GetConstructor(new[] { typeof(MethodInfo) });
            if (ctor != null) return ctor.Invoke(new object[] { method });

            object value = Activator.CreateInstance(harmonyMethodType);
            if (!SetMemberIfPresent(value, "method", method))
                throw new MissingMemberException("HarmonyMethod.method");
            return value;
        }

        private static string[] GetPatchOwners(MethodInfo target)
        {
            object info = GetPatchInfo(target);
            if (info == null) return new string[0];

            var owners = new List<string>();
            foreach (string member in new[] { "Prefixes", "Postfixes", "Transpilers", "Finalizers" })
            {
                IEnumerable patches = Get(info, member) as IEnumerable;
                if (patches == null) continue;
                foreach (object patch in patches)
                {
                    string owner = Convert.ToString(Get(patch, "owner"));
                    if (!string.IsNullOrEmpty(owner)) owners.Add(owner);
                }
            }
            return owners.ToArray();
        }

        private static void DumpPatchOwners(string label, MethodInfo target)
        {
            if (target == null)
            {
                _log?.LogWarning("TT_PROBE_PATCH " + label + " target=<null>");
                return;
            }

            try
            {
                object info = GetPatchInfo(target);
                if (info == null)
                {
                    _log?.LogInfo("TT_PROBE_PATCH " + label + " no_patches");
                    return;
                }

                foreach (string member in new[] { "Prefixes", "Postfixes", "Transpilers", "Finalizers" })
                {
                    IEnumerable patches = Get(info, member) as IEnumerable;
                    if (patches == null) continue;

                    int index = 0;
                    foreach (object patch in patches)
                    {
                        _log?.LogInfo(
                            "TT_PROBE_PATCH " + label +
                            " kind=" + member +
                            " index=" + index++ +
                            " owner=" + SafeValue(Get(patch, "owner")) +
                            " priority=" + SafeValue(Get(patch, "priority")) +
                            " before=" + SequenceValue(Get(patch, "before") as IEnumerable) +
                            " after=" + SequenceValue(Get(patch, "after") as IEnumerable) +
                            " patch_method=" + PatchMethodValue(Get(patch, "PatchMethod") ?? Get(patch, "patch")));
                    }
                }
            }
            catch (Exception ex)
            {
                _log?.LogError("TT_PROBE_PATCH " + label + " failed. " + ex);
            }
        }

        private static object GetPatchInfo(MethodInfo target)
        {
            if (target == null) return null;
            Type harmonyType = FindType("HarmonyLib.Harmony");
            MethodInfo getPatchInfo = harmonyType?.GetMethods(Stat)
                .FirstOrDefault(m =>
                {
                    if (m.Name != "GetPatchInfo") return false;
                    ParameterInfo[] p = m.GetParameters();
                    return p.Length == 1 && typeof(MethodBase).IsAssignableFrom(p[0].ParameterType);
                });
            return getPatchInfo?.Invoke(null, new object[] { target });
        }

        private static Type FindType(string name)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    Type exact = assembly.GetType(name, false);
                    if (exact != null) return exact;
                    Type match = assembly.GetTypes().FirstOrDefault(t => t != null && t.Name == name);
                    if (match != null) return match;
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Type match = ex.Types.FirstOrDefault(t => t != null && t.Name == name);
                    if (match != null) return match;
                }
                catch { }
            }
            return null;
        }

        private static object Get(object instance, string name)
        {
            if (instance == null || string.IsNullOrEmpty(name)) return null;
            for (Type type = instance.GetType(); type != null; type = type.BaseType)
            {
                FieldInfo field = type.GetField(name, Inst);
                if (field != null) return field.GetValue(instance);

                PropertyInfo property = type.GetProperty(name, Inst);
                if (property != null && property.CanRead)
                {
                    try { return property.GetValue(instance, null); }
                    catch { return null; }
                }
            }
            return null;
        }

        private static bool SetMemberIfPresent(object instance, string name, object value)
        {
            if (instance == null) return false;
            for (Type type = instance.GetType(); type != null; type = type.BaseType)
            {
                FieldInfo field = type.GetField(name, Inst);
                if (field != null)
                {
                    field.SetValue(instance, value);
                    return true;
                }

                PropertyInfo property = type.GetProperty(name, Inst);
                if (property != null && property.CanWrite)
                {
                    property.SetValue(instance, value, null);
                    return true;
                }
            }
            return false;
        }

        private static int ToInt(object value)
        {
            if (value == null) return 0;
            try { return Convert.ToInt32(value); }
            catch { return 0; }
        }

        private static string Preview(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            string escaped = value
                .Replace("\\", "\\\\")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\"", "\\\"");
            const int limit = 320;
            return escaped.Length <= limit ? escaped : escaped.Substring(0, limit) + "...";
        }

        private static int CountLines(string value)
        {
            if (string.IsNullOrEmpty(value)) return 0;
            int count = 1;
            for (int i = 0; i < value.Length; i++)
                if (value[i] == '\n') count++;
            return count;
        }

        private static string SafeValue(object value)
        {
            return value == null ? "<null>" : Convert.ToString(value);
        }

        private static string VectorValue(object value)
        {
            if (value is Vector2)
            {
                Vector2 v = (Vector2)value;
                return "(" + F(v.x) + "," + F(v.y) + ")";
            }
            return SafeValue(value);
        }

        private static string F(float value)
        {
            return value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
        }

        private static string ComponentPath(Component component)
        {
            if (component == null) return "<none>";
            var names = new List<string>();
            Transform current = component.transform;
            while (current != null)
            {
                names.Add(current.name);
                current = current.parent;
            }
            names.Reverse();
            return string.Join("/", names.ToArray());
        }

        private static string SequenceValue(IEnumerable values)
        {
            if (values == null) return "[]";
            var list = new List<string>();
            foreach (object value in values)
                list.Add(Convert.ToString(value));
            return "[" + string.Join(",", list.ToArray()) + "]";
        }

        private static string PatchMethodValue(object value)
        {
            MethodInfo method = value as MethodInfo;
            if (method != null)
                return (method.DeclaringType == null ? "<null>" : method.DeclaringType.FullName) + "." + method.Name;
            return SafeValue(value);
        }
    }
}
