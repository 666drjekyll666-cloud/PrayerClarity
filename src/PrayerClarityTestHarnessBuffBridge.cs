using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;

namespace PrayerClarityTestHarness
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("nikich.graveyardkeeper.prayerclarity.testharness", BepInDependency.DependencyFlags.HardDependency)]
    public sealed class PrayerClarityTestHarnessBuffBridgePlugin : BaseUnityPlugin
    {
        private const string PluginGuid = "nikich.graveyardkeeper.prayerclarity.testharness.buffbridge";
        private const string PluginName = "PrayerClarity Test Harness Buff Bridge";
        private const string PluginVersion = "0.1.4";

        private static readonly BindingFlags Inst = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly BindingFlags Stat = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private static ManualLogSource _log;
        private static MethodInfo _addBuff;

        private void Awake()
        {
            _log = Logger;
            try
            {
                Type buffsLogics = AnyType("BuffsLogics");
                _addBuff = buffsLogics == null ? null : buffsLogics.GetMethod(
                    "AddBuff",
                    Stat,
                    null,
                    new[] { typeof(string), typeof(float?) },
                    null);
                if (_addBuff == null)
                    throw new MissingMethodException("Verified BuffsLogics.AddBuff(string, Nullable<float>) not found.");

                MethodInfo target = typeof(PrayerClarityTestHarnessPlugin).GetMethod("ActivateSelectedBuff", Stat);
                MethodInfo prefix = typeof(PrayerClarityTestHarnessBuffBridgePlugin).GetMethod(nameof(ActivateSelectedBuffPrefix), Stat);
                Patch(target, prefix);

                Logger.LogInfo("PrayerClarity Test Harness Buff Bridge 0.1.4 loaded. Synthetic timed buffs use verified BuffsLogics.AddBuff(string, Nullable<float>) directly; sermon animation, rewards, item drops and prayer consumption remain bypassed.");
            }
            catch (Exception ex)
            {
                Logger.LogError("PrayerClarity Test Harness Buff Bridge initialization failed: " + ex);
            }
        }

        private static bool ActivateSelectedBuffPrefix(object gui, string buffId)
        {
            if (string.IsNullOrEmpty(buffId))
                throw new ArgumentException("Synthetic buff id is empty.", nameof(buffId));
            if (_addBuff == null)
                throw new MissingMethodException("BuffsLogics.AddBuff bridge is unavailable.");

            object craft = Get(gui, "pray_craft");
            if (craft == null)
                throw new InvalidOperationException("Synthetic pray_craft is unavailable.");

            object rawDuration = Get(craft, "dur_parameter");
            float duration = rawDuration == null ? 0f : Convert.ToSingle(rawDuration, CultureInfo.InvariantCulture);
            float? length = duration > 0f ? duration : (float?)null;

            _addBuff.Invoke(null, new object[] { buffId, length });
            _log?.LogInfo("Synthetic native PlayerBuff added directly: " + buffId +
                          "; duration_override=" + (length.HasValue ? length.Value.ToString("0.###", CultureInfo.InvariantCulture) : "<definition default>") +
                          ". Normal sermon path remains bypassed.");

            return false;
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

        private static object Get(object obj, string name)
        {
            if (obj == null) return null;
            for (Type type = obj.GetType(); type != null; type = type.BaseType)
            {
                FieldInfo field = type.GetField(name, Inst);
                if (field != null) return field.GetValue(obj);
                PropertyInfo property = type.GetProperty(name, Inst);
                if (property != null && property.CanRead) return property.GetValue(obj, null);
            }
            return null;
        }

        private static void Patch(MethodInfo target, MethodInfo prefix)
        {
            if (target == null || prefix == null) throw new MissingMethodException("Harness buff bridge Harmony target/prefix missing.");
            Type harmonyType = AnyType("HarmonyLib.Harmony");
            Type harmonyMethodType = AnyType("HarmonyLib.HarmonyMethod");
            if (harmonyType == null || harmonyMethodType == null) throw new InvalidOperationException("Harmony unavailable.");

            object harmony = Activator.CreateInstance(harmonyType, new object[] { PluginGuid });
            object harmonyPrefix = Activator.CreateInstance(harmonyMethodType, new object[] { prefix });
            MethodInfo patch = harmonyType.GetMethods(Inst)
                .FirstOrDefault(m => m.Name == "Patch" && m.GetParameters().Length >= 5 && typeof(MethodBase).IsAssignableFrom(m.GetParameters()[0].ParameterType));
            if (patch == null) throw new MissingMethodException("Harmony.Patch");

            object[] args = new object[patch.GetParameters().Length];
            args[0] = target;
            args[1] = harmonyPrefix;
            args[2] = null;
            args[3] = null;
            args[4] = null;
            patch.Invoke(harmony, args);
        }
    }
}
