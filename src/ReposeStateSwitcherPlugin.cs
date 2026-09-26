using System;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;

namespace PrayerClarityResearch
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(RebalancedPluginGuid, BepInDependency.DependencyFlags.HardDependency)]
    public sealed class ReposeStateSwitcherPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "nikich.graveyardkeeper.prayerclarity.repose.stateswitcher";
        public const string RebalancedPluginGuid = "nikich.graveyardkeeper.prayerclarity.rebalanced";
        public const string PluginName = "PrayerClarity Repose State Switcher";
        public const string PluginVersion = "0.1.0";

        private enum OverrideMode
        {
            Live = 0,
            Early = 1,
            Terminal = 2
        }

        private static readonly BindingFlags AnyStatic =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        private static readonly BindingFlags AnyInstance =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        private static ManualLogSource _log;
        private static OverrideMode _mode;
        private static MethodInfo _findBuffById;
        private static object _harmony;

        private Rect _windowRect = new Rect(660f, 24f, 520f, 300f);
        private bool _visible;
        private string _status =
            "Live state: PrayerClarity reads the real corpse-progression parameters.";

        private void Awake()
        {
            _log = Logger;
            InstallPlayerParamPrefix();
            ResolveBuffLookup();

            Logger.LogInfo(
                PluginName + " " + PluginVersion +
                " loaded. Press F3. Research-only: overrides PrayerClarity reads only; no game/save parameters are written.");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F3))
                _visible = !_visible;

            if (_visible && Input.GetKeyDown(KeyCode.Escape))
                _visible = false;
        }

        private void OnGUI()
        {
            if (!_visible) return;

            _windowRect = GUILayout.Window(
                736220,
                _windowRect,
                DrawWindow,
                "PrayerClarity: Repose State Switcher " + PluginVersion);
        }

        private void DrawWindow(int id)
        {
            GUILayout.Label("Research-only visual setup for PrayerClarity: Rebalanced 0.2.40.");
            GUILayout.Label("F3 toggles. Escape closes. No game/save state is written.");

            GUILayout.Space(8f);
            GUILayout.Label("Current override: " + DescribeMode());
            GUILayout.Label("Live buff_skull active: " + (IsReposeBuffActive() ? "yes" : "no"));

            GUILayout.Space(8f);

            if (GUILayout.Button("Can still unlock higher body tier  (early 1..1 -> Repose 1..2)", GUILayout.Height(34f)))
                SetMode(OverrideMode.Early);

            if (GUILayout.Button("Terminal body tiers  (2..3 -> Repose 2..4, no ordinary tier 4)", GUILayout.Height(34f)))
                SetMode(OverrideMode.Terminal);

            if (GUILayout.Button("Restore LIVE save state", GUILayout.Height(30f)))
                SetMode(OverrideMode.Live);

            GUILayout.Space(8f);
            GUILayout.Label("Then use the Neutral Test Console (F2) to open the pulpit and select Bronze Repose.");
            GUILayout.Label("Status: " + _status);

            GUI.DragWindow(new Rect(0f, 0f, 10000f, 24f));
        }

        private void SetMode(OverrideMode mode)
        {
            _mode = mode;

            if (mode == OverrideMode.Early)
            {
                _status =
                    "EARLY override enabled: PrayerClarity sees body_min=1, body_max=1, add_body_min/max=0. " +
                    "Bronze Repose should still show its normal higher-tier effect.";
            }
            else if (mode == OverrideMode.Terminal)
            {
                _status =
                    "TERMINAL override enabled: PrayerClarity sees body_min=2, body_max=3, add_body_min/max=0. " +
                    "Bronze Repose should show the terminal wording plus Silver/Gold explanation.";
            }
            else
            {
                _status =
                    "LIVE state restored: PrayerClarity reads the real corpse-progression parameters again.";
            }

            _log?.LogInfo(
                "REPOSE_STATE_SWITCH mode=" + mode +
                " buff_skull_active=" + IsReposeBuffActive() +
                " save_written=false");
        }

        private static string DescribeMode()
        {
            switch (_mode)
            {
                case OverrideMode.Early:
                    return "EARLY / can still unlock";
                case OverrideMode.Terminal:
                    return "TERMINAL";
                default:
                    return "LIVE save state";
            }
        }

        private static bool PlayerParamPrefix(string param, float fallback, ref float __result)
        {
            OverrideMode mode = _mode;
            if (mode == OverrideMode.Live)
                return true;

            float bodyMin = mode == OverrideMode.Early ? 1f : 2f;
            float bodyMax = mode == OverrideMode.Early ? 1f : 3f;

            switch (param)
            {
                case "body_min":
                    __result = bodyMin;
                    return false;

                case "body_max":
                    // R.PlayerParam observes the live player value. When buff_skull is
                    // already active, stock state includes its +1 modifier and
                    // CorpseTierSemantics subtracts that +1 again before evaluating
                    // the selected prayer. Preserve that relationship in the fixture.
                    __result = bodyMax + (IsReposeBuffActive() ? 1f : 0f);
                    return false;

                case "add_body_min":
                case "add_body_max":
                    __result = 0f;
                    return false;

                default:
                    return true;
            }
        }

        private static void InstallPlayerParamPrefix()
        {
            Type bridge = FindType("PrayerClarity.R");
            MethodInfo target = bridge?.GetMethod(
                "PlayerParam",
                AnyStatic,
                null,
                new[] { typeof(string), typeof(float) },
                null);
            if (target == null)
                throw new MissingMethodException("PrayerClarity.R.PlayerParam(string,float)");

            Type harmonyType = FindType("HarmonyLib.Harmony");
            Type harmonyMethodType = FindType("HarmonyLib.HarmonyMethod");
            if (harmonyType == null || harmonyMethodType == null)
                throw new InvalidOperationException("HarmonyLib runtime types unavailable.");

            _harmony = Activator.CreateInstance(
                harmonyType,
                new object[] { PluginGuid });

            MethodInfo prefixMethod = typeof(ReposeStateSwitcherPlugin).GetMethod(
                nameof(PlayerParamPrefix),
                BindingFlags.NonPublic | BindingFlags.Static);
            object harmonyPrefix = CreateHarmonyMethod(harmonyMethodType, prefixMethod);

            MethodInfo patch = harmonyType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(method => method.Name == "Patch")
                .FirstOrDefault(method =>
                {
                    ParameterInfo[] p = method.GetParameters();
                    return p.Length >= 2 &&
                           typeof(MethodBase).IsAssignableFrom(p[0].ParameterType) &&
                           p[1].ParameterType == harmonyMethodType;
                });
            if (patch == null)
                throw new MissingMethodException("Harmony.Patch");

            ParameterInfo[] parameters = patch.GetParameters();
            object[] args = new object[parameters.Length];
            args[0] = target;
            args[1] = harmonyPrefix;
            for (int i = 2; i < args.Length; i++)
                args[i] = null;

            patch.Invoke(_harmony, args);
        }

        private static object CreateHarmonyMethod(Type harmonyMethodType, MethodInfo method)
        {
            ConstructorInfo ctor = harmonyMethodType.GetConstructor(new[] { typeof(MethodInfo) });
            if (ctor != null)
                return ctor.Invoke(new object[] { method });

            object instance = Activator.CreateInstance(harmonyMethodType);
            FieldInfo field = harmonyMethodType.GetField(
                "method",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
                throw new MissingMemberException("HarmonyMethod.method");

            field.SetValue(instance, method);
            return instance;
        }

        private static void ResolveBuffLookup()
        {
            Type buffsLogics = FindType("BuffsLogics");
            _findBuffById = buffsLogics?.GetMethod(
                "FindBuffByID",
                AnyStatic,
                null,
                new[] { typeof(string) },
                null);
            if (_findBuffById == null)
                throw new MissingMethodException("BuffsLogics.FindBuffByID(string)");
        }

        private static bool IsReposeBuffActive()
        {
            try
            {
                return _findBuffById != null &&
                       _findBuffById.Invoke(null, new object[] { "buff_skull" }) != null;
            }
            catch
            {
                return false;
            }
        }

        private void OnDestroy()
        {
            _mode = OverrideMode.Live;

            try
            {
                if (_harmony == null) return;
                MethodInfo unpatchSelf = _harmony.GetType().GetMethod(
                    "UnpatchSelf",
                    BindingFlags.Public | BindingFlags.Instance);
                unpatchSelf?.Invoke(_harmony, null);
            }
            catch (Exception ex)
            {
                _log?.LogWarning("REPOSE_STATE_SWITCH cleanup failed: " + ex);
            }
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

                Type match = types.FirstOrDefault(type =>
                    type != null &&
                    (type.FullName == fullOrShortName || type.Name == fullOrShortName));
                if (match != null) return match;
            }

            return null;
        }
    }
}
