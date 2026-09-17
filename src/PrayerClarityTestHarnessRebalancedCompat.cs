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
        public const string PluginVersion = "0.1.1";

        private const string HarmonyId = "nikich.graveyardkeeper.prayerclarity.testharness.rebalancedcompat";
        private static ManualLogSource _log;
        private static bool _shieldAliasWarningLogged;

        private void Awake()
        {
            _log = Logger;
            PatchHarnessBuffActivation();
            Logger.LogInfo("PrayerClarity Test Harness compatibility active for PrayerClarity: Rebalanced. Legacy Vanilla GUID alias is provided and synthetic timed-buff activations project the selected Rebalanced tier before the Harness adds the native PlayerBuff.");
        }

        private static void PatchHarnessBuffActivation()
        {
            Type gui = FindType("PrayCraftGUI");
            if (gui == null) throw new MissingMemberException("PrayCraftGUI");

            MethodInfo target = gui.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "DoPrayForBuff" && m.GetParameters().Length == 0);
            if (target == null) throw new MissingMethodException("PrayCraftGUI.DoPrayForBuff()");

            Type harmonyType = FindType("HarmonyLib.Harmony");
            Type harmonyMethodType = FindType("HarmonyLib.HarmonyMethod");
            if (harmonyType == null || harmonyMethodType == null)
                throw new InvalidOperationException("Harmony unavailable.");

            object harmony = Activator.CreateInstance(harmonyType, new object[] { HarmonyId });
            MethodInfo prefixMethod = typeof(PrayerClarityTestHarnessRebalancedCompat).GetMethod(
                nameof(DoPrayForBuffPrefix),
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

        private static void DoPrayForBuffPrefix(object __instance)
        {
            try
            {
                object craft = Get(__instance, "pray_craft") ?? Get(__instance, "_pray_craft");
                string craftId = Get(craft, "id") as string;
                string prayerId;
                int tier;
                if (!TryParsePrayerCraft(craftId, out prayerId, out tier)) return;

                switch (prayerId)
                {
                    case "b_plant":
                        SetPlayerParam("prayerclarity_rebalanced_plant_tier", tier);
                        SetPlayerParam("prayerclarity_rebalanced_plant_reduction", Tier(tier, 0.20f, 0.30f, 0.40f));
                        break;
                    case "b_sins":
                        SetPlayerParam("prayerclarity_rebalanced_confession_tier", tier);
                        SetPlayerParam("prayerclarity_rebalanced_confession_bonus", Tier(tier, 0.35f, 0.60f, 0.85f));
                        break;
                    case "b_skull":
                        SetPlayerParam("prayerclarity_rebalanced_repose_tier", tier);
                        break;
                    case "b_sword":
                    case "b_shield":
                        SetPlayerParam("prayerclarity_rebalanced_combat_tier", tier);
                        SetPlayerParam("prayerclarity_rebalanced_combat_extra_damage", Math.Max(0f, Tier(tier, 5f, 10f, 15f) - 5f));
                        SetPlayerParam("prayerclarity_rebalanced_combat_regen", Tier(tier, 1f, 2f, 4f));
                        if (prayerId == "b_shield" && !_shieldAliasWarningLogged)
                        {
                            _shieldAliasWarningLogged = true;
                            _log?.LogWarning("Synthetic Protection/b_shield is a retired Rebalanced alias for Combat and resolves to the same buff_sword. Activating both b_sword and b_shield in one test can extend the same buff duration twice; skip b_shield when validating the canonical Combat Temporary Effect.");
                        }
                        break;
                    case "b_star":
                        SetPlayerParam("prayerclarity_rebalanced_excellence_tier", tier);
                        break;
                    default:
                        return;
                }

                _log?.LogInfo("Synthetic Rebalanced tier projected before buff activation: " + prayerId + " tier=" + tier + ".");
            }
            catch (Exception ex)
            {
                _log?.LogError("PrayerClarity Test Harness Rebalanced tier projection failed; synthetic buff activation may fall back to stock/Vanilla presentation. " + ex);
            }
        }

        private static bool TryParsePrayerCraft(string craftId, out string prayerId, out int tier)
        {
            prayerId = null;
            tier = 0;
            if (string.IsNullOrEmpty(craftId)) return false;

            string value = craftId.StartsWith("pray:", StringComparison.Ordinal)
                ? craftId.Substring(5)
                : craftId;

            int separator = value.LastIndexOf(':');
            if (separator <= 0 || separator + 1 >= value.Length) return false;
            if (!int.TryParse(value.Substring(separator + 1), out tier) || tier < 1 || tier > 3) return false;

            prayerId = value.Substring(0, separator);
            return prayerId.StartsWith("b_", StringComparison.Ordinal);
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
