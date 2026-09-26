using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using BepInEx;
using UnityEngine;

namespace PrayerClarityResearch
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(RebalancedPluginGuid, BepInDependency.DependencyFlags.HardDependency)]
    public sealed class PulpitGeometryProbe : BaseUnityPlugin
    {
        public const string PluginGuid = "nikich.graveyardkeeper.prayerclarity.research.pulpitgeometry";
        public const string RebalancedPluginGuid = "nikich.graveyardkeeper.prayerclarity.rebalanced";
        public const string PluginName = "PrayerClarity Pulpit Geometry Probe";
        public const string PluginVersion = "0.1.1";

        private static readonly BindingFlags Any =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        private void Awake()
        {
            Logger.LogInfo(
                PluginName + " " + PluginVersion +
                " loaded. Read-only research DLL. Open the pulpit on the failing prayer, wait for it to render, then press F8 once.");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F8))
                Dump();
        }

        private void Dump()
        {
            try
            {
                GameObject windowGo = GameObject.Find("UI Root/Pray GUI/window");
                if (windowGo == null)
                {
                    Logger.LogWarning("PULPIT_GEOMETRY status=NO_WINDOW");
                    return;
                }

                Transform window = windowGo.transform;
                Transform container = window.Find("container");
                Transform root = container == null ? null : container.Find("PrayerClarity.PulpitForecast");
                Transform effect = root == null ? null : root.Find("PrayerClarity.Effect");
                Transform button = window.Find("craft button");

                Type widgetType = FindType("UIWidget");
                Type labelType = FindType("UILabel");

                Logger.LogInfo(
                    "PULPIT_GEOMETRY_BEGIN frame=" + Time.frameCount +
                    " window=" + PathOf(window) +
                    " container=" + PathOf(container) +
                    " effect=" + PathOf(effect) +
                    " button=" + PathOf(button));

                DumpTransform("WINDOW", window);
                DumpWidget("WINDOW_WIDGET", GetComponent(window, widgetType), window);
                DumpTransform("CONTAINER", container);
                DumpWidget("CONTAINER_WIDGET", GetComponent(container, widgetType), window);
                DumpTransform("FORECAST_ROOT", root);
                DumpTransform("EFFECT_TRANSFORM", effect);

                object effectLabel = GetComponent(effect, labelType);
                if (effectLabel != null)
                {
                    object rawText = Get(effectLabel, "text");
                    object processedText = Get(effectLabel, "processedText");
                    Logger.LogInfo(
                        "PULPIT_EFFECT_LABEL" +
                        " active=" + Active(effect) +
                        " width=" + F(Get(effectLabel, "width")) +
                        " height=" + F(Get(effectLabel, "height")) +
                        " pivot=" + F(Get(effectLabel, "pivot")) +
                        " overflow=" + F(Get(effectLabel, "overflowMethod")) +
                        " printedSize=" + F(Get(effectLabel, "printedSize")) +
                        " localSize=" + F(Get(effectLabel, "localSize")) +
                        " raw=" + Q(rawText) +
                        " processed=" + Q(processedText));
                    DumpWidget("EFFECT_WIDGET", effectLabel, window);
                }
                else
                {
                    Logger.LogWarning("PULPIT_EFFECT_LABEL status=MISSING");
                }

                DumpTransform("BUTTON_ROOT", button);
                DumpComponents("BUTTON_ROOT_COMPONENTS", button);

                object productionButtonWidget = null;
                Component[] buttonWidgets = Array.Empty<Component>();
                if (button != null && widgetType != null)
                {
                    productionButtonWidget = button.gameObject.GetComponent(widgetType);
                    buttonWidgets = button.gameObject.GetComponentsInChildren(widgetType, true);
                    if (productionButtonWidget == null && buttonWidgets.Length > 0)
                        productionButtonWidget = buttonWidgets[0];
                }

                Logger.LogInfo(
                    "PULPIT_BUTTON_WIDGETS count=" + buttonWidgets.Length +
                    " productionCandidate=" + WidgetIdentity(productionButtonWidget));

                for (int i = 0; i < buttonWidgets.Length; i++)
                {
                    Component component = buttonWidgets[i];
                    DumpWidget(
                        "BUTTON_WIDGET index=" + i +
                        " directRootWidget=" + ReferenceEquals(component, productionButtonWidget),
                        component,
                        window);
                    DumpAnchorState("BUTTON_WIDGET_ANCHOR index=" + i, component);
                }

                if (effectLabel != null && productionButtonWidget != null)
                {
                    float effectBottom;
                    float effectTop;
                    float buttonBottom;
                    float buttonTop;
                    float windowBottom;
                    float windowTop;

                    bool effectOk = TryBounds(effectLabel, window, out effectBottom, out effectTop);
                    bool buttonOk = TryVisibleButtonBounds(buttonWidgets, window, out buttonBottom, out buttonTop);
                    object windowWidget = GetComponent(window, widgetType);
                    bool windowOk = TryBounds(windowWidget, window, out windowBottom, out windowTop);

                    if (effectOk && buttonOk)
                    {
                        const float clearance = 8f;
                        float downward = Math.Max(0f, buttonTop - (effectBottom - clearance));
                        Logger.LogInfo(
                            "PULPIT_GEOMETRY_CURRENT_ALGORITHM" +
                            " effectBottom=" + Num(effectBottom) +
                            " effectTop=" + Num(effectTop) +
                            " buttonBottom=" + Num(buttonBottom) +
                            " buttonTop=" + Num(buttonTop) +
                            " requiredDownward=" + Num(downward));

                        if (windowOk)
                        {
                            float predictedBottom = buttonBottom - downward;
                            const float bottomMargin = 10f;
                            float deficit = (windowBottom + bottomMargin) - predictedBottom;
                            Logger.LogInfo(
                                "PULPIT_GEOMETRY_WINDOW_TEST" +
                                " windowBottom=" + Num(windowBottom) +
                                " windowTop=" + Num(windowTop) +
                                " predictedButtonBottom=" + Num(predictedBottom) +
                                " bottomDeficit=" + Num(deficit) +
                                " wouldGrow=" + (deficit > 0f));
                        }
                    }
                    else
                    {
                        Logger.LogWarning(
                            "PULPIT_GEOMETRY_CURRENT_ALGORITHM status=BOUNDS_FAILED" +
                            " effectOk=" + effectOk +
                            " buttonOk=" + buttonOk +
                            " windowOk=" + windowOk);
                    }
                }

                Logger.LogInfo("PULPIT_GEOMETRY_END frame=" + Time.frameCount);
            }
            catch (Exception ex)
            {
                Logger.LogError("PULPIT_GEOMETRY_EXCEPTION " + ex);
            }
        }

        private void DumpTransform(string tag, Transform transform)
        {
            if (transform == null)
            {
                Logger.LogWarning("PULPIT_" + tag + " status=MISSING");
                return;
            }

            Logger.LogInfo(
                "PULPIT_" + tag +
                " path=" + PathOf(transform) +
                " active=" + transform.gameObject.activeInHierarchy +
                " localPos=" + Vec(transform.localPosition) +
                " worldPos=" + Vec(transform.position) +
                " localScale=" + Vec(transform.localScale));
        }

        private void DumpWidget(string tag, object widget, Transform frame)
        {
            if (widget == null)
            {
                Logger.LogWarning("PULPIT_" + tag + " status=MISSING");
                return;
            }

            GameObject go = Get(widget, "gameObject") as GameObject;
            float min;
            float max;
            bool bounds = TryBounds(widget, frame, out min, out max);

            Logger.LogInfo(
                "PULPIT_" + tag +
                " id=" + WidgetIdentity(widget) +
                " active=" + (go != null && go.activeInHierarchy) +
                " width=" + F(Get(widget, "width")) +
                " height=" + F(Get(widget, "height")) +
                " pivot=" + F(Get(widget, "pivot")) +
                " localSize=" + F(Get(widget, "localSize")) +
                " boundsOk=" + bounds +
                (bounds ? " minY=" + Num(min) + " maxY=" + Num(max) : string.Empty));
        }

        private void DumpComponents(string tag, Transform transform)
        {
            if (transform == null)
            {
                Logger.LogWarning("PULPIT_" + tag + " status=MISSING");
                return;
            }

            Component[] components = transform.gameObject.GetComponents<Component>();
            List<string> names = new List<string>();
            foreach (Component component in components)
            {
                if (component == null) continue;
                names.Add(component.GetType().FullName ?? component.GetType().Name);
            }

            Logger.LogInfo(
                "PULPIT_" + tag +
                " count=" + names.Count +
                " types=" + Q(string.Join(",", names.ToArray())));
        }

        private void DumpAnchorState(string tag, object widget)
        {
            if (widget == null)
            {
                Logger.LogWarning("PULPIT_" + tag + " status=MISSING");
                return;
            }

            Logger.LogInfo(
                "PULPIT_" + tag +
                " id=" + WidgetIdentity(widget) +
                " isAnchored=" + F(Get(widget, "isAnchored")) +
                " isAnchoredHorizontal=" + F(Get(widget, "isAnchoredHorizontally")) +
                " isAnchoredVertical=" + F(Get(widget, "isAnchoredVertically")) +
                " updateAnchors=" + F(Get(widget, "updateAnchors")) +
                " anchorUpdate=" + F(Get(widget, "anchorUpdate")));

            DumpAnchorPoint(tag + " side=left", Get(widget, "leftAnchor"));
            DumpAnchorPoint(tag + " side=right", Get(widget, "rightAnchor"));
            DumpAnchorPoint(tag + " side=bottom", Get(widget, "bottomAnchor"));
            DumpAnchorPoint(tag + " side=top", Get(widget, "topAnchor"));
        }

        private void DumpAnchorPoint(string tag, object anchor)
        {
            if (anchor == null)
            {
                Logger.LogInfo("PULPIT_" + tag + " anchor=<null>");
                return;
            }

            object target = Get(anchor, "target");
            string targetText = "<null>";
            Transform targetTransform = target as Transform;
            Component targetComponent = target as Component;
            if (targetTransform != null)
                targetText = PathOf(targetTransform);
            else if (targetComponent != null)
                targetText = PathOf(targetComponent.transform) + ":" + targetComponent.GetType().Name;
            else if (target != null)
                targetText = target.GetType().FullName + ":" + target;

            Logger.LogInfo(
                "PULPIT_" + tag +
                " target=" + Q(targetText) +
                " relative=" + F(Get(anchor, "relative")) +
                " absolute=" + F(Get(anchor, "absolute")));
        }

        private static bool TryVisibleButtonBounds(Component[] widgets, Transform frame, out float minY, out float maxY)
        {
            minY = 0f;
            maxY = 0f;
            if (widgets == null || frame == null) return false;

            bool found = false;
            float unionMin = float.PositiveInfinity;
            float unionMax = float.NegativeInfinity;

            foreach (Component widget in widgets)
            {
                if (widget == null || !widget.gameObject.activeInHierarchy) continue;

                float childMin;
                float childMax;
                if (!TryBounds(widget, frame, out childMin, out childMax)) continue;

                if (childMin < unionMin) unionMin = childMin;
                if (childMax > unionMax) unionMax = childMax;
                found = true;
            }

            if (!found) return false;
            minY = unionMin;
            maxY = unionMax;
            return true;
        }

        private static object GetComponent(Transform transform, Type type)
        {
            return transform == null || type == null ? null : transform.gameObject.GetComponent(type);
        }

        private static bool TryBounds(object widget, Transform frame, out float minY, out float maxY)
        {
            minY = 0f;
            maxY = 0f;
            if (widget == null || frame == null) return false;

            Vector3[] corners = Get(widget, "worldCorners") as Vector3[];
            if (corners == null || corners.Length == 0) return false;

            minY = float.PositiveInfinity;
            maxY = float.NegativeInfinity;
            foreach (Vector3 corner in corners)
            {
                float y = frame.InverseTransformPoint(corner).y;
                if (y < minY) minY = y;
                if (y > maxY) maxY = y;
            }

            return !float.IsInfinity(minY) && !float.IsInfinity(maxY);
        }

        private static string WidgetIdentity(object widget)
        {
            if (widget == null) return "<null>";
            GameObject go = Get(widget, "gameObject") as GameObject;
            Component component = widget as Component;
            return (component == null ? widget.GetType().Name : component.GetType().Name) +
                   ":" + (go == null ? "<no-go>" : PathOf(go.transform));
        }

        private static object Get(object instance, string name)
        {
            if (instance == null) return null;
            Type type = instance.GetType();

            PropertyInfo property = type.GetProperty(name, Any);
            if (property != null)
            {
                try { return property.GetValue(instance, null); }
                catch { }
            }

            FieldInfo field = type.GetField(name, Any);
            if (field != null)
            {
                try { return field.GetValue(instance); }
                catch { }
            }

            return null;
        }

        private static Type FindType(string shortName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    Type type = assembly.GetType(shortName, false);
                    if (type != null) return type;

                    foreach (Type candidate in assembly.GetTypes())
                    {
                        if (candidate != null && string.Equals(candidate.Name, shortName, StringComparison.Ordinal))
                            return candidate;
                    }
                }
                catch { }
            }
            return null;
        }

        private static bool Active(Transform transform)
        {
            return transform != null && transform.gameObject.activeInHierarchy;
        }

        private static string PathOf(Transform transform)
        {
            if (transform == null) return "<null>";
            List<string> parts = new List<string>();
            Transform p = transform;
            while (p != null)
            {
                parts.Add(p.name);
                p = p.parent;
            }
            parts.Reverse();
            return string.Join("/", parts.ToArray());
        }

        private static string F(object value)
        {
            return value == null ? "<null>" : value.ToString();
        }

        private static string Q(object value)
        {
            string s = value == null ? "<null>" : value.ToString();
            return "\"" + s.Replace("\\", "\\\\").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\"", "\\\"") + "\"";
        }

        private static string Num(float value)
        {
            return value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
        }

        private static string Vec(Vector3 value)
        {
            return "(" + Num(value.x) + "," + Num(value.y) + "," + Num(value.z) + ")";
        }
    }
}
