using System;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;

namespace PrayerClarityResearch
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(RebalancedPluginGuid, BepInDependency.DependencyFlags.HardDependency)]
    public sealed class PrayerClarityTestHarnessRebalancedCompat : BaseUnityPlugin
    {
        public const string PluginGuid = "nikich.graveyardkeeper.prayerclarity";
        public const string RebalancedPluginGuid = "nikich.graveyardkeeper.prayerclarity.rebalanced";
        public const string PluginName = "PrayerClarity Test Harness Rebalanced Compatibility";
        public const string PluginVersion = "0.1.2";

        private const string HarmonyId = "nikich.graveyardkeeper.prayerclarity.testharness.rebalancedcompat";
        private static ManualLogSource _log;

        private void Awake()
        {
            _log = Logger;
            PatchNativeBuffAdd();
            Logger.LogInfo("PrayerClarity Test Harness compatibility active for PrayerClarity: Rebalanced. Legacy Vanilla GUID alias is provided and synthetic timed-buff AddBuff calls project the selected Rebalanced tier from their verified duration override.");
        }

        private static void PatchNativeBuffAdd()
        {
            Type buffsLogics = FindType("BuffsLogics");
            if (buffsLogics == null) throw new MissingMemberException("BuffsLogics");

            Type nullableFloat = typeof(float?);
            MethodInfo target = buffsLogics.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
                .FirstOrDefault(m =>
                {
                    if (m.Name != "AddBuff") return false;
                    ParameterInfo[] p = m.GetParameters();
                    return p.Length == 2 &&
                           p[0].ParameterType == typeof(string) &&
                           p[1].ParameterType == nullableFloat;
                });
            if (target == null) throw new MissingMethodException("BuffsLogics.AddBuff(string, Nullable<float>)");

            Type harmonyType = FindType("HarmonyLib.Harmony");
            Type harmonyMethodType = FindType("HarmonyLib.HarmonyMethod");
            if (harmonyType == null || harmonyMethodType == null)
                throw new InvalidOperationException("Harmony unavailable.");

            object harmony = Activator.CreateInstance(harmonyType, new object[] { HarmonyId });
            MethodInfo prefixMethod = typeof(PrayerClarityTestHarnessRebalancedCompat).GetMethod(
                nameof(AddBuffPrefix),
                BindingFlags.NonPublic | BindingFlags.Static);
            object prefix = CreateHarmonyMethod(harmonyMethodType, prefixMethod);

            MethodInfo patch = harmonyType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "Patch" &&
                                     m.GetParameters().Length >= 5 &&
                                     typeof(MethodBase).IsAssignableFrom(m.GetParameters()[0].ParameterType));
            if (patch == null) throw new MissingMethodException("Harmony.Patch");

            object[] args = new object[patch.GetParameters().Length];
            args[0] = target;
            args[1] = prefix;
            args[2] = null;
            args[3] = null;
            args[4] = null;
            patch.Invoke(harmony, args);
        }

        private static object CreateHarmonyMethod(Type harmonyMethodType, MethodInfo method)
        {
            ConstructorInfo ctor = harmonyMethodType.GetConstructor(new[] { typeof(MethodInfo) });
            if (ctor != null) return ctor.Invoke(new object[] { method });

            object instance = Activator.CreateInstance(harmonyMethodType);
            FieldInfo methodField = harmonyMethodType.GetField("method", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (methodField == null) throw new MissingMemberException("HarmonyMethod.method");
            methodField.SetValue(instance, method);
            return instance;
        }

        private static void AddBuffPrefix(string __0, float? __1)
        {
            try
            {
                string buffId = __0;
                if (string.IsNullOrEmpty(buffId) || !__1.HasValue) return;

                int tier = InferTier(buffId, __1.Value);
                if (tier == 0) return;

                switch (buffId)
                {
                    case "buff_plant":
                        SetPlayerParam("prayerclarity_rebalanced_plant_tier", tier);
                        SetPlayerParam("prayerclarity_rebalanced_plant_reduction", Tier(tier, 0.20f, 0.30f, 0.40f));
                        break;
                    case "buff_sins":
                        SetPlayerParam("prayerclarity_rebalanced_confession_tier", tier);
                        SetPlayerParam("prayerclarity_rebalanced_confession_bonus", Tier(tier, 0.35f, 0.60f, 0.85f));
                        break;
                    case "buff_skull":
                        SetPlayerParam("prayerclarity_rebalanced_repose_tier", tier);
                        break;
                    case "buff_sword":
                    case "buff_shield":
                        SetPlayerParam("prayerclarity_rebalanced_combat_tier", tier);
                        SetPlayerParam("prayerclarity_rebalanced_combat_extra_damage", Math.Max(0f, Tier(tier, 5f, 10f, 15f) - 5f));
                        SetPlayerParam("prayerclarity_rebalanced_combat_regen", Tier(tier, 1f, 2f, 4f));
                        break;
                    case "buff_star":
                        SetPlayerParam("prayerclarity_rebalanced_excellence_tier", tier);
                        break;
                    default:
                        return;
                }

                _log?.LogInfo("Synthetic Rebalanced tier projected at BuffsLogics.AddBuff: " + buffId +
                              " tier=" + tier + " duration_override=" + __1.Value.ToString("0.###") + ".");
            }
            catch (Exception ex)
            {
                _log?.LogError("PrayerClarity Test Harness Rebalanced tier projection failed; synthetic buff activation may fall back to stock/Vanilla presentation. " + ex);
            }
        }

        private static int InferTier(string buffId, float durationMinutes)
        {
            float baseDuration;
            switch (buffId)
            {
                case "buff_plant":
                case "buff_sword":
                case "buff_shield":
                    baseDuration = 36f;
                    break;
                case "buff_sins":
                case "buff_skull":
                case "buff_star":
                    baseDuration = 18f;
                    break;
                default:
                    return 0;
            }

            int tier = (int)Math.Round(durationMinutes / baseDuration);
            if (tier < 1 || tier > 3) return 0;

            float expected = baseDuration * tier;
            return Math.Abs(durationMinutes - expected) <= 0.05f ? tier : 0;
        }

        private static float Tier(int tier, float bronze, float silver, float gold)
        {
            return tier == 1 ? bronze : tier == 2 ? silver : gold;
        }

        private static void SetPlayerParam(string name, float value)
        {
            Type mainGame = FindType("MainGame");
            object me = GetStatic(mainGame, "me");
            object player = Get(me, "player");
            if (player == null) throw new InvalidOperationException("MainGame.me.player unavailable.");

            MethodInfo setParam = player.GetType().GetMethod(
                "SetParam",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new[] { typeof(string), typeof(float) },
                null);
            if (setParam == null) throw new MissingMethodException("WorldGameObject.SetParam(string,float)");
            setParam.Invoke(player, new object[] { name, value });
        }

        private static object Get(object instance, string name)
        {
            if (instance == null || string.IsNullOrEmpty(name)) return null;
            Type type = instance.GetType();
            while (type != null)
            {
                FieldInfo field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (field != null) return field.GetValue(instance);

                PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (property != null && property.GetIndexParameters().Length == 0) return property.GetValue(instance, null);
                type = type.BaseType;
            }
            return null;
        }

        private static object GetStatic(Type type, string name)
        {
            if (type == null || string.IsNullOrEmpty(name)) return null;
            while (type != null)
            {
                FieldInfo field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);
                if (field != null) return field.GetValue(null);

                PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);
                if (property != null && property.GetIndexParameters().Length == 0) return property.GetValue(null, null);
                type = type.BaseType;
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
                try { types = assembly.GetTypes(); }
                catch (ReflectionTypeLoadException ex) { types = ex.Types; }
                catch { continue; }

                Type match = types.FirstOrDefault(t => t != null && (t.FullName == fullOrShortName || t.Name == fullOrShortName));
                if (match != null) return match;
            }
            return null;
        }
    }
}
