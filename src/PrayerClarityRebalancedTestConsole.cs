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
        public const string PluginName = "PrayerClarity: Rebalanced Test Console";
        public const string PluginVersion = "0.1.0";

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

        private readonly List<TimedEffect> _effects = new List<TimedEffect>();
        private Rect _windowRect = new Rect(24f, 24f, 620f, 560f);
        private bool _visible;
        private string _status = "F1 opens/closes this console. Synthetic buffs use the game's native BuffsLogics path.";

        private void Awake()
        {
            _log = Logger;
            ResolveBuffApi();

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
                tier =>
                {
                    SetPlayerParam("prayerclarity_rebalanced_confession_tier", tier);
                    SetPlayerParam(
                        "prayerclarity_rebalanced_confession_bonus",
                        Tier(tier, 0.35f, 0.60f, 0.85f));
                }));

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
                        "prayerclarity_rebalanced_combat_extra_damage",
                        Math.Max(0f, Tier(tier, 5f, 10f, 15f) - 5f));
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

            _effects.Add(new TimedEffect("Soul Contentment (BSS)", "buff_gp_increase", 36f));
            _effects.Add(new TimedEffect("Thorough Cleansing (BSS)", "buff_sin_shard", 36f));

            Logger.LogInfo(
                PluginName + " " + PluginVersion +
                " loaded. Press F1 for the research-only timed-effect console. " +
                "This tool mutates the current save's live buff list through the native BuffsLogics API; clear synthetic buffs before saving a test state you want to keep.");
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
                "PrayerClarity: Rebalanced Test Console 0.1.0");
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

            GUILayout.Space(8f);
            GUILayout.Label("Status: " + _status);

            GUI.DragWindow(new Rect(0f, 0f, 10000f, 24f));
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

        private void Activate(TimedEffect effect, int tier)
        {
            try
            {
                if (effect == null || tier < 1 || tier > 3) return;

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
