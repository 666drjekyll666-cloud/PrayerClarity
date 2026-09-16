using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx.Logging;
using UnityEngine;

namespace PrayerClarity
{
    internal static class TechnologyTooltipViewportClamp
    {
        private const float SafeMarginPixels = 16f;
        private const string GamepadTooltipPositionFixHarmonyId = "nikich.gyk.movegamepadtooltips";

        private sealed class Marker
        {
            internal static readonly Marker Instance = new Marker();
            private Marker() { }
        }

        private sealed class BubbleState
        {
            internal readonly Component Root;
            internal BubbleState(Component root) { Root = root; }
        }

        private static readonly ConditionalWeakTable<object, Marker> OwnedTooltips = new ConditionalWeakTable<object, Marker>();
        private static readonly ConditionalWeakTable<object, BubbleState> OwnedBubbles = new ConditionalWeakTable<object, BubbleState>();

        private static ManualLogSource _log;
        private static bool _installed;
        private static bool _runtimeFailed;
        private static Type _uiRootType;
        private static Func<object, object> _linkedTooltipGetter;
        private static Func<object, object> _widgetGetter;
        private static Func<object, int> _widgetWidthGetter;
        private static Func<object, int> _widgetHeightGetter;
        private static Func<object, int> _manualHeightGetter;

        internal static void Install(string harmonyId, ManualLogSource log)
        {
            _log = log;

            Type tooltipType = R.GameType("Tooltip");
            Type bubbleType = R.GameType("WidgetsBubbleGUI");
            _uiRootType = R.AnyType("UIRoot");
            if (tooltipType == null || bubbleType == null || _uiRootType == null)
                throw new MissingMemberException("Tooltip / WidgetsBubbleGUI / UIRoot type is unavailable.");

            MemberInfo linkedTooltip = RequireMember(tooltipType, "linked_tooltip");
            MemberInfo widget = RequireMember(bubbleType, "widget");
            Type widgetType = MemberType(widget);
            MemberInfo width = RequireMember(widgetType, "width");
            MemberInfo height = RequireMember(widgetType, "height");
            MemberInfo manualHeight = RequireMember(_uiRootType, "manualHeight");

            _linkedTooltipGetter = CompileGetter<object>(tooltipType, linkedTooltip);
            _widgetGetter = CompileGetter<object>(bubbleType, widget);
            _widgetWidthGetter = CompileGetter<int>(widgetType, width);
            _widgetHeightGetter = CompileGetter<int>(widgetType, height);
            _manualHeightGetter = CompileGetter<int>(_uiRootType, manualHeight);

            MethodInfo tooltipShow = R.Method(tooltipType, "Show", false, new[] { typeof(bool) });
            MethodInfo tooltipClear = R.Method(tooltipType, "ClearData", false, 0);
            MethodInfo bubbleUpdate = R.Method(bubbleType, "Update", false, 0);
            if (tooltipShow == null) throw new MissingMethodException("Tooltip.Show(bool)");
            if (tooltipClear == null) throw new MissingMethodException("Tooltip.ClearData()");
            if (bubbleUpdate == null) throw new MissingMethodException("WidgetsBubbleGUI.Update()");

            R.Patch(harmonyId + ".techviewport.show", typeof(TechnologyTooltipViewportClamp), tooltipShow, nameof(TooltipShowPostfix));
            R.Patch(harmonyId + ".techviewport.clear", typeof(TechnologyTooltipViewportClamp), tooltipClear, nameof(TooltipClearPostfix));
            R.Patch(
                harmonyId + ".techviewport.update",
                typeof(TechnologyTooltipViewportClamp),
                bubbleUpdate,
                nameof(BubbleUpdatePostfix),
                new[] { GamepadTooltipPositionFixHarmonyId });

            _installed = true;
        }

        internal static void MarkTechnologyTooltip(object tooltip)
        {
            if (!_installed || tooltip == null) return;
            OwnedTooltips.Remove(tooltip);
            OwnedTooltips.Add(tooltip, Marker.Instance);
        }

        private static void TooltipClearPostfix(object __instance)
        {
            if (__instance != null) OwnedTooltips.Remove(__instance);
        }

        private static void TooltipShowPostfix(object __instance)
        {
            if (_runtimeFailed || __instance == null) return;

            Marker marker;
            if (!OwnedTooltips.TryGetValue(__instance, out marker)) return;

            try
            {
                object bubble = _linkedTooltipGetter(__instance);
                Component component = bubble as Component;
                if (component == null) return;

                Transform rootTransform = component.transform.root;
                Component root = rootTransform == null ? null : rootTransform.GetComponent(_uiRootType);
                if (root == null) return;

                OwnedBubbles.Remove(bubble);
                OwnedBubbles.Add(bubble, new BubbleState(root));
            }
            catch (Exception ex)
            {
                DisableAfterRuntimeFailure("linking Technology tooltip bubble", ex);
            }
        }

        private static void BubbleUpdatePostfix(object __instance)
        {
            if (_runtimeFailed || __instance == null) return;

            BubbleState state;
            if (!OwnedBubbles.TryGetValue(__instance, out state)) return;

            try
            {
                Component bubble = __instance as Component;
                if (bubble == null || state.Root == null) return;

                object widget = _widgetGetter(__instance);
                if (widget == null) return;

                int width = _widgetWidthGetter(widget);
                int height = _widgetHeightGetter(widget);
                int manualHeight = _manualHeightGetter(state.Root);
                int screenWidth = Screen.width;
                int screenHeight = Screen.height;
                if (width <= 0 || height <= 0 || manualHeight <= 0 || screenWidth <= 0 || screenHeight <= 0) return;

                float scale = manualHeight / (float)screenHeight;
                Rect safe = Screen.safeArea;
                float halfScreenWidth = screenWidth * 0.5f;
                float halfScreenHeight = screenHeight * 0.5f;

                float left = (safe.xMin - halfScreenWidth) * scale;
                float right = (safe.xMax - halfScreenWidth) * scale;
                float bottom = (safe.yMin - halfScreenHeight) * scale;
                float top = (safe.yMax - halfScreenHeight) * scale;
                float margin = SafeMarginPixels * scale;

                Vector3 position = bubble.transform.localPosition;
                float x = ClampAxis(position.x, left, right, width, margin);
                float y = ClampAxis(position.y, bottom, top, height, margin);

                if (Math.Abs(x - position.x) <= 0.01f && Math.Abs(y - position.y) <= 0.01f) return;
                bubble.transform.localPosition = new Vector3(x, y, position.z);
            }
            catch (Exception ex)
            {
                DisableAfterRuntimeFailure("clamping Technology tooltip bubble", ex);
            }
        }

        private static float ClampAxis(float value, float low, float high, float size, float margin)
        {
            float min = low + margin + size * 0.5f;
            float max = high - margin - size * 0.5f;
            if (min > max) return value;
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private static void DisableAfterRuntimeFailure(string operation, Exception ex)
        {
            if (_runtimeFailed) return;
            _runtimeFailed = true;
            _log?.LogError("PrayerClarity Technology tooltip viewport safety disabled after failure while " + operation + ". Vanilla placement remains available. " + ex);
        }

        private static MemberInfo RequireMember(Type type, string name)
        {
            FieldInfo field = type.GetField(name, R.Inst);
            if (field != null) return field;
            PropertyInfo property = type.GetProperty(name, R.Inst);
            if (property != null && property.CanRead) return property;
            throw new MissingMemberException(type.FullName, name);
        }

        private static Type MemberType(MemberInfo member)
        {
            FieldInfo field = member as FieldInfo;
            if (field != null) return field.FieldType;
            PropertyInfo property = member as PropertyInfo;
            if (property != null) return property.PropertyType;
            throw new NotSupportedException("Unsupported member type: " + member.MemberType);
        }

        private static Func<object, T> CompileGetter<T>(Type declaringType, MemberInfo member)
        {
            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            Expression typed = Expression.Convert(instance, declaringType);
            Expression access;

            FieldInfo field = member as FieldInfo;
            if (field != null)
                access = Expression.Field(typed, field);
            else
            {
                PropertyInfo property = member as PropertyInfo;
                if (property == null) throw new NotSupportedException("Unsupported member type: " + member.MemberType);
                access = Expression.Property(typed, property);
            }

            return Expression.Lambda<Func<object, T>>(Expression.Convert(access, typeof(T)), instance).Compile();
        }
    }
}
