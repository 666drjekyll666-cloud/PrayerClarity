using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;

namespace PrayerClarityResearch
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(RebalancedPluginGuid, BepInDependency.DependencyFlags.HardDependency)]
    public sealed class PrayerClarityTechnologyPrayerCarouselPrototype : BaseUnityPlugin
    {
        public const string PluginGuid = "nikich.graveyardkeeper.prayerclarity.techprayercarouselprototype";
        public const string RebalancedPluginGuid = "nikich.graveyardkeeper.prayerclarity.rebalanced";
        public const string PluginName = "PrayerClarity: Technology Prayer Carousel Prototype";
        public const string PluginVersion = "0.1.0";

        private static readonly BindingFlags Inst =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly BindingFlags Stat =
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly HashSet<string> TargetFamilies =
            new HashSet<string>(StringComparer.Ordinal)
            {
                "b_souls",
                "b_grat_points_incr",
                "b_sin_shard"
            };

        private sealed class State
        {
            internal object Item;
            internal object Tooltip;
            internal readonly List<object> Unlocks = new List<object>();
            internal readonly List<string> Families = new List<string>();
            internal readonly List<object> Children = new List<object>();
            internal readonly Dictionary<object, Color> OriginalSpriteColors =
                new Dictionary<object, Color>(ReferenceComparer.Instance);
            internal int SelectedIndex;
        }

        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            internal static readonly ReferenceComparer Instance = new ReferenceComparer();
            public new bool Equals(object x, object y) { return ReferenceEquals(x, y); }
            public int GetHashCode(object obj) { return RuntimeHelpers.GetHashCode(obj); }
        }

        private static ManualLogSource _log;
        private static Type _techTreeGuiType;
        private static Type _tooltipType;
        private static MethodInfo _resolvePrayerCrafts;
        private static MethodInfo _tooltipClearData;
        private static MethodInfo _tooltipsRedraw;
        private static readonly Dictionary<object, State> States =
            new Dictionary<object, State>(ReferenceComparer.Instance);
        private static readonly HashSet<string> LoggedTargets =
            new HashSet<string>(StringComparer.Ordinal);
        private static State _active;
        private static bool _runtimeErrorLogged;

        private void Awake()
        {
            _log = Logger;
            Logger.LogInfo(
                PluginName + " " + PluginVersion +
                " loaded. Research-only UI prototype; it does not write save/progression state.");
        }

        private void Start()
        {
            try
            {
                Install();
            }
            catch (Exception ex)
            {
                Logger.LogError("PC_CAROUSEL_INSTALL_FAILED " + ex);
            }
        }

        private static void Install()
        {
            Type techTreeItemType = FindType("TechTreeGUIItem");
            _techTreeGuiType = FindType("TechTreeGUI");
            Type baseGuiType = FindType("BaseGUI");
            _tooltipType = FindType("Tooltip");
            Type tooltipsManagerType = FindType("TooltipsManager");
            Type presentationType = FindType("PrayerClarity.SecondarySurfacePresentation");

            if (techTreeItemType == null) throw new MissingMemberException("TechTreeGUIItem");
            if (_techTreeGuiType == null) throw new MissingMemberException("TechTreeGUI");
            if (baseGuiType == null) throw new MissingMemberException("BaseGUI");
            if (_tooltipType == null) throw new MissingMemberException("Tooltip");
            if (tooltipsManagerType == null) throw new MissingMemberException("TooltipsManager");
            if (presentationType == null)
                throw new MissingMemberException("PrayerClarity.SecondarySurfacePresentation");

            MethodInfo initGamepadTooltip = techTreeItemType.GetMethods(Inst)
                .FirstOrDefault(m =>
                    m.Name == "InitGamepadTooltip" &&
                    m.GetParameters().Length == 1);
            MethodInfo onGamepadOver = techTreeItemType.GetMethod(
                "OnGamepadOver", Inst, null, Type.EmptyTypes, null);
            MethodInfo onGamepadOut = techTreeItemType.GetMethod(
                "OnGamepadOut", Inst, null, Type.EmptyTypes, null);
            MethodInfo option1 = baseGuiType.GetMethod(
                "OnPressedOption1", Inst, null, Type.EmptyTypes, null);
            MethodInfo option2 = baseGuiType.GetMethod(
                "OnPressedOption2", Inst, null, Type.EmptyTypes, null);

            _resolvePrayerCrafts = presentationType.GetMethod(
                "ResolvePrayerCrafts", Stat, null, new[] { typeof(object) }, null);
            _tooltipClearData = _tooltipType.GetMethod(
                "ClearData", Inst, null, Type.EmptyTypes, null);
            _tooltipsRedraw = tooltipsManagerType.GetMethod(
                "Redraw", Stat, null, Type.EmptyTypes, null);

            if (initGamepadTooltip == null)
                throw new MissingMethodException("TechTreeGUIItem.InitGamepadTooltip");
            if (onGamepadOver == null)
                throw new MissingMethodException("TechTreeGUIItem.OnGamepadOver");
            if (onGamepadOut == null)
                throw new MissingMethodException("TechTreeGUIItem.OnGamepadOut");
            if (option1 == null)
                throw new MissingMethodException("BaseGUI.OnPressedOption1");
            if (option2 == null)
                throw new MissingMethodException("BaseGUI.OnPressedOption2");
            if (_resolvePrayerCrafts == null)
                throw new MissingMethodException("SecondarySurfacePresentation.ResolvePrayerCrafts(object)");
            if (_tooltipClearData == null)
                throw new MissingMethodException("Tooltip.ClearData()");
            if (_tooltipsRedraw == null)
                throw new MissingMethodException("TooltipsManager.Redraw()");

            Patch(initGamepadTooltip, null, nameof(InitGamepadTooltipPostfix));
            Patch(onGamepadOver, null, nameof(OnGamepadOverPostfix));
            Patch(onGamepadOut, null, nameof(OnGamepadOutPostfix));
            Patch(option1, nameof(Option1Prefix), null);
            Patch(option2, nameof(Option2Prefix), null);

            _log?.LogInfo(
                "PC_CAROUSEL_READY focus the Better Save Soul prayer Technology node; X=previous prayer, Y=next prayer; D-pad remains native tree navigation.");
        }

        private static void InitGamepadTooltipPostfix(object __instance, object __0)
        {
            try
            {
                State oldState;
                if (States.TryGetValue(__instance, out oldState))
                {
                    RestoreHighlight(oldState);
                    States.Remove(__instance);
                    if (ReferenceEquals(_active, oldState)) _active = null;
                }

                State state = TryBuildState(__instance, __0);
                if (state == null) return;

                States[__instance] = state;
                RebuildSelectedTooltip(state, false);

                string techId = Convert.ToString(Get(__instance, "tech_id")) ?? "<unknown>";
                if (LoggedTargets.Add(techId))
                {
                    _log?.LogInfo(
                        "PC_CAROUSEL_TARGET tech_id=" + techId +
                        " families=" + string.Join(",", state.Families.ToArray()) +
                        " unlock_ids=" + string.Join(",", state.Unlocks.Select(GetId).ToArray()));
                }
            }
            catch (Exception ex)
            {
                LogRuntimeFailure("PC_CAROUSEL_INIT_FAILED ", ex);
            }
        }

        private static void OnGamepadOverPostfix(object __instance)
        {
            try
            {
                State state;
                if (!States.TryGetValue(__instance, out state)) return;

                _active = state;
                ApplyHighlight(state);
                LogSelection(state, "focus");
            }
            catch (Exception ex)
            {
                LogRuntimeFailure("PC_CAROUSEL_FOCUS_FAILED ", ex);
            }
        }

        private static void OnGamepadOutPostfix(object __instance)
        {
            try
            {
                State state;
                if (!States.TryGetValue(__instance, out state)) return;

                RestoreHighlight(state);
                if (ReferenceEquals(_active, state)) _active = null;
            }
            catch (Exception ex)
            {
                LogRuntimeFailure("PC_CAROUSEL_UNFOCUS_FAILED ", ex);
            }
        }

        private static bool Option1Prefix(object __instance, ref bool __result)
        {
            return HandleOption(__instance, -1, ref __result);
        }

        private static bool Option2Prefix(object __instance, ref bool __result)
        {
            return HandleOption(__instance, 1, ref __result);
        }

        private static bool HandleOption(object gui, int delta, ref bool result)
        {
            try
            {
                if (gui == null || _techTreeGuiType == null || !_techTreeGuiType.IsInstanceOfType(gui))
                    return true;

                State state = _active;
                if (state == null || state.Unlocks.Count == 0)
                    return true;

                Component item = state.Item as Component;
                if (item == null || !item.gameObject.activeInHierarchy)
                {
                    RestoreHighlight(state);
                    _active = null;
                    return true;
                }

                int count = state.Unlocks.Count;
                state.SelectedIndex = (state.SelectedIndex + delta + count) % count;
                RebuildSelectedTooltip(state, true);
                ApplyHighlight(state);
                LogSelection(state, delta < 0 ? "previous" : "next");

                result = true;
                return false;
            }
            catch (Exception ex)
            {
                LogRuntimeFailure("PC_CAROUSEL_INPUT_FAILED ", ex);
                return true;
            }
        }

        private static State TryBuildState(object item, object visibleUnlocks)
        {
            List<object> unlocks = ToList(visibleUnlocks as IEnumerable);
            if (unlocks.Count != 3) return null;

            var families = new List<string>();
            foreach (object unlock in unlocks)
            {
                string family = ResolveSingleFamily(unlock);
                if (string.IsNullOrEmpty(family) || !TargetFamilies.Contains(family))
                    return null;
                families.Add(family);
            }

            if (new HashSet<string>(families, StringComparer.Ordinal).Count != 3)
                return null;

            Component component = item as Component;
            if (component == null) return null;
            object tooltip = component.gameObject.GetComponent(_tooltipType);
            if (tooltip == null) return null;

            var state = new State
            {
                Item = item,
                Tooltip = tooltip,
                SelectedIndex = 0
            };
            state.Unlocks.AddRange(unlocks);
            state.Families.AddRange(families);

            IEnumerable childEnumerable = Get(item, "_unlocks") as IEnumerable;
            if (childEnumerable != null)
            {
                foreach (object child in childEnumerable)
                {
                    if (child != null) state.Children.Add(child);
                }
            }

            return state;
        }

        private static string ResolveSingleFamily(object techUnlock)
        {
            IEnumerable crafts = _resolvePrayerCrafts.Invoke(null, new[] { techUnlock }) as IEnumerable;
            if (crafts == null) return null;

            string family = null;
            int count = 0;
            foreach (object craft in crafts)
            {
                string craftFamily = PrayerFamilyFromCraftId(GetId(craft));
                if (string.IsNullOrEmpty(craftFamily)) continue;
                count++;

                if (family == null) family = craftFamily;
                else if (!string.Equals(family, craftFamily, StringComparison.Ordinal))
                    return null;
            }

            return count == 0 ? null : family;
        }

        private static string PrayerFamilyFromCraftId(string craftId)
        {
            if (string.IsNullOrEmpty(craftId) ||
                !craftId.StartsWith("pray:", StringComparison.Ordinal))
                return null;

            int lastColon = craftId.LastIndexOf(':');
            if (lastColon <= 5) return null;
            return craftId.Substring(5, lastColon - 5);
        }

        private static void RebuildSelectedTooltip(State state, bool redraw)
        {
            if (state == null ||
                state.Tooltip == null ||
                state.SelectedIndex < 0 ||
                state.SelectedIndex >= state.Unlocks.Count)
                return;

            object unlock = state.Unlocks[state.SelectedIndex];
            MethodInfo getTooltip = unlock.GetType().GetMethod(
                "GetTooltip", Inst, null, new[] { _tooltipType }, null);
            if (getTooltip == null)
                throw new MissingMethodException(unlock.GetType().FullName, "GetTooltip(Tooltip)");

            _tooltipClearData.Invoke(state.Tooltip, null);
            getTooltip.Invoke(unlock, new[] { state.Tooltip });

            if (redraw)
                _tooltipsRedraw.Invoke(null, null);
        }

        private static void ApplyHighlight(State state)
        {
            if (state == null) return;

            for (int i = 0; i < state.Children.Count && i < state.Unlocks.Count; i++)
            {
                object sprite = Get(state.Children[i], "spr");
                if (sprite == null) continue;

                Color original;
                if (!state.OriginalSpriteColors.TryGetValue(sprite, out original))
                {
                    object value = Get(sprite, "color");
                    if (!(value is Color)) continue;
                    original = (Color)value;
                    state.OriginalSpriteColors[sprite] = original;
                }

                float alpha = i == state.SelectedIndex ? original.a : original.a * 0.35f;
                Set(sprite, "color", new Color(original.r, original.g, original.b, alpha));
            }
        }

        private static void RestoreHighlight(State state)
        {
            if (state == null || state.OriginalSpriteColors.Count == 0) return;

            foreach (KeyValuePair<object, Color> pair in state.OriginalSpriteColors)
            {
                if (pair.Key != null)
                    Set(pair.Key, "color", pair.Value);
            }
            state.OriginalSpriteColors.Clear();
        }

        private static void LogSelection(State state, string reason)
        {
            if (state == null ||
                state.SelectedIndex < 0 ||
                state.SelectedIndex >= state.Unlocks.Count)
                return;

            _log?.LogInfo(
                "PC_CAROUSEL_SELECT reason=" + reason +
                " index=" + (state.SelectedIndex + 1) + "/" + state.Unlocks.Count +
                " family=" + state.Families[state.SelectedIndex] +
                " unlock_id=" + GetId(state.Unlocks[state.SelectedIndex]));
        }

        private static void Patch(MethodInfo target, string prefixName, string postfixName)
        {
            Type harmonyType = FindType("HarmonyLib.Harmony");
            Type harmonyMethodType = FindType("HarmonyLib.HarmonyMethod");
            if (harmonyType == null || harmonyMethodType == null)
                throw new InvalidOperationException("Harmony is unavailable.");

            object harmony = Activator.CreateInstance(harmonyType, new object[] { PluginGuid });
            object prefix = CreateHarmonyMethod(harmonyMethodType, prefixName);
            object postfix = CreateHarmonyMethod(harmonyMethodType, postfixName);

            MethodInfo patch = harmonyType.GetMethods(Inst)
                .FirstOrDefault(m =>
                    m.Name == "Patch" &&
                    m.GetParameters().Length >= 5 &&
                    typeof(MethodBase).IsAssignableFrom(m.GetParameters()[0].ParameterType));
            if (patch == null) throw new MissingMethodException("Harmony.Patch");

            object[] args = new object[patch.GetParameters().Length];
            args[0] = target;
            args[1] = prefix;
            args[2] = postfix;
            args[3] = null;
            args[4] = null;
            patch.Invoke(harmony, args);
        }

        private static object CreateHarmonyMethod(Type harmonyMethodType, string methodName)
        {
            if (string.IsNullOrEmpty(methodName)) return null;

            MethodInfo method = typeof(PrayerClarityTechnologyPrayerCarouselPrototype)
                .GetMethod(methodName, Stat);
            if (method == null)
                throw new MissingMethodException(
                    typeof(PrayerClarityTechnologyPrayerCarouselPrototype).FullName,
                    methodName);

            ConstructorInfo ctor = harmonyMethodType.GetConstructor(new[] { typeof(MethodInfo) });
            if (ctor != null) return ctor.Invoke(new object[] { method });

            object value = Activator.CreateInstance(harmonyMethodType);
            if (!Set(value, "method", method))
                throw new MissingMemberException("HarmonyMethod.method");
            return value;
        }

        private static Type FindType(string name)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    Type exact = assembly.GetType(name, false);
                    if (exact != null) return exact;

                    Type match = assembly.GetTypes()
                        .FirstOrDefault(t => t != null && t.Name == name);
                    if (match != null) return match;
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Type match = ex.Types
                        .FirstOrDefault(t => t != null && t.Name == name);
                    if (match != null) return match;
                }
                catch
                {
                }
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

        private static bool Set(object instance, string name, object value)
        {
            if (instance == null || string.IsNullOrEmpty(name)) return false;
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

        private static string GetId(object value)
        {
            return Convert.ToString(Get(value, "id")) ?? string.Empty;
        }

        private static List<object> ToList(IEnumerable values)
        {
            var result = new List<object>();
            if (values == null) return result;
            foreach (object value in values)
                if (value != null) result.Add(value);
            return result;
        }

        private static void LogRuntimeFailure(string prefix, Exception ex)
        {
            if (_runtimeErrorLogged) return;
            _runtimeErrorLogged = true;

            Exception inner = ex is TargetInvocationException && ex.InnerException != null
                ? ex.InnerException
                : ex;
            _log?.LogError(prefix + inner);
        }
    }
}
