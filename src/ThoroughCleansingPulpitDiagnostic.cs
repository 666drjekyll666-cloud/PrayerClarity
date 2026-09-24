using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;

namespace PrayerClarityResearch
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(RebalancedPluginGuid, BepInDependency.DependencyFlags.HardDependency)]
    public sealed class ThoroughCleansingPulpitDiagnostic : BaseUnityPlugin
    {
        public const string PluginGuid = "nikich.graveyardkeeper.prayerclarity.thoroughcleansingpulpitdiagnostic";
        public const string RebalancedPluginGuid = "nikich.graveyardkeeper.prayerclarity.rebalanced";
        public const string PluginName = "PrayerClarity: Thorough Cleansing Pulpit Diagnostic";
        public const string PluginVersion = "0.1.0";

        private static readonly BindingFlags Inst =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly BindingFlags Stat =
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private static ManualLogSource _log;
        private static Type _pulpitPolishType;
        private static MethodInfo _tryBuildTierEffect;
        private static MethodInfo _buildTierDetails;
        private static string _lastDiagnostic;

        private void Awake()
        {
            _log = Logger;
            try
            {
                Install();
                Logger.LogInfo(
                    PluginName + " " + PluginVersion +
                    " loaded. Research-only, read-only diagnostic; no save or gameplay state is changed.");
            }
            catch (Exception ex)
            {
                Logger.LogError("PC_THOROUGH_DIAGNOSTIC_INSTALL_FAILED " + ex);
            }
        }

        private static void Install()
        {
            _pulpitPolishType = FindType("PrayerClarity.PulpitPolish");
            Type semanticsType = FindType("PrayerClarity.PrayerEditionSemantics");
            Type forecastType = FindType("PrayerClarity.PrayerForecast");

            if (_pulpitPolishType == null) throw new MissingMemberException("PrayerClarity.PulpitPolish");
            if (semanticsType == null) throw new MissingMemberException("PrayerClarity.PrayerEditionSemantics");
            if (forecastType == null) throw new MissingMemberException("PrayerClarity.PrayerForecast");

            MethodInfo finalWriter = _pulpitPolishType.GetMethod("PolishEffectRow", Stat);
            if (finalWriter == null) throw new MissingMethodException("PulpitPolish.PolishEffectRow()");

            _tryBuildTierEffect = semanticsType.GetMethod(
                "TryBuildTierEffect",
                Stat,
                null,
                new[]
                {
                    typeof(string),
                    typeof(string),
                    typeof(string).MakeByRefType(),
                    typeof(string).MakeByRefType()
                },
                null);
            if (_tryBuildTierEffect == null)
                throw new MissingMethodException("PrayerEditionSemantics.TryBuildTierEffect(...)");

            _buildTierDetails = forecastType.GetMethod(
                "BuildTierDetails",
                Stat,
                null,
                new[] { typeof(object) },
                null);
            if (_buildTierDetails == null)
                throw new MissingMethodException("PrayerForecast.BuildTierDetails(object)");

            PatchPostfix(finalWriter, nameof(PulpitPolishPostfix));
        }

        private static void PulpitPolishPostfix()
        {
            try
            {
                object gui = GetStatic(_pulpitPolishType, "_gui");
                object craft = Get(gui, "pray_craft");
                string craftId = GetId(craft);
                if (string.IsNullOrEmpty(craftId) ||
                    !craftId.StartsWith("pray:b_sin_shard:", StringComparison.Ordinal))
                    return;

                string buffId = Convert.ToString(Get(craft, "buff")) ?? string.Empty;
                object[] semanticArgs = { craftId, buffId, null, null };
                bool providerMatched = Convert.ToBoolean(_tryBuildTierEffect.Invoke(null, semanticArgs));
                string providerText = semanticArgs[2] as string;
                string providerKey = semanticArgs[3] as string;

                object tier = _buildTierDetails.Invoke(null, new[] { craft });
                int tierIndex = ToInt(Get(tier, "QualityTier"));
                string tierCore = Convert.ToString(Get(tier, "SpecialCoreText")) ?? string.Empty;
                string tierSpecial = Convert.ToString(Get(tier, "SpecialText")) ?? string.Empty;

                object result = GetStatic(_pulpitPolishType, "_forecast");
                string resultSpecial = Convert.ToString(Get(result, "SpecialText")) ?? string.Empty;

                object effectLabel = GetStatic(_pulpitPolishType, "_effectLabel");
                string finalText = Convert.ToString(Get(effectLabel, "text")) ?? string.Empty;

                string line =
                    "PC_THOROUGH_PULPIT_DIAGNOSTIC" +
                    " craft=" + Safe(craftId) +
                    " buff=" + Safe(buffId) +
                    " needs_quality=" + Safe(Convert.ToString(Get(craft, "needs_quality"))) +
                    " duration=" + Safe(Convert.ToString(Get(craft, "dur_parameter"))) +
                    " provider_matched=" + providerMatched +
                    " provider_text=" + Safe(providerText) +
                    " provider_key=" + Safe(providerKey) +
                    " tier=" + tierIndex +
                    " tier_core=" + Safe(tierCore) +
                    " tier_special=" + Safe(tierSpecial) +
                    " result_special=" + Safe(resultSpecial) +
                    " final_effect=" + Safe(finalText);

                if (string.Equals(line, _lastDiagnostic, StringComparison.Ordinal)) return;
                _lastDiagnostic = line;
                _log?.LogInfo(line);
            }
            catch (Exception ex)
            {
                _log?.LogError("PC_THOROUGH_PULPIT_DIAGNOSTIC_FAILED " + ex);
            }
        }

        private static void PatchPostfix(MethodInfo target, string postfixName)
        {
            Type harmonyType = FindType("HarmonyLib.Harmony");
            Type harmonyMethodType = FindType("HarmonyLib.HarmonyMethod");
            if (harmonyType == null || harmonyMethodType == null)
                throw new InvalidOperationException("Harmony is unavailable.");

            MethodInfo postfixMethod = typeof(ThoroughCleansingPulpitDiagnostic).GetMethod(postfixName, Stat);
            if (postfixMethod == null)
                throw new MissingMethodException(typeof(ThoroughCleansingPulpitDiagnostic).FullName, postfixName);

            object harmonyMethod = Activator.CreateInstance(harmonyMethodType, new object[] { postfixMethod });
            object harmony = Activator.CreateInstance(harmonyType, new object[] { PluginGuid });

            MethodInfo patch = null;
            foreach (MethodInfo method in harmonyType.GetMethods(Inst))
            {
                if (method.Name != "Patch") continue;
                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length >= 5 && typeof(MethodBase).IsAssignableFrom(parameters[0].ParameterType))
                {
                    patch = method;
                    break;
                }
            }
            if (patch == null) throw new MissingMethodException("Harmony.Patch");

            object[] args = new object[patch.GetParameters().Length];
            args[0] = target;
            args[1] = null;
            args[2] = harmonyMethod;
            args[3] = null;
            args[4] = null;
            patch.Invoke(harmony, args);
        }

        private static Type FindType(string fullOrSimpleName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type exact = assembly.GetType(fullOrSimpleName, false);
                if (exact != null) return exact;

                Type[] types;
                try { types = assembly.GetTypes(); }
                catch (ReflectionTypeLoadException ex) { types = ex.Types; }

                if (types == null) continue;
                foreach (Type type in types)
                {
                    if (type != null &&
                        (string.Equals(type.FullName, fullOrSimpleName, StringComparison.Ordinal) ||
                         string.Equals(type.Name, fullOrSimpleName, StringComparison.Ordinal)))
                        return type;
                }
            }
            return null;
        }

        private static object GetStatic(Type type, string name)
        {
            if (type == null) return null;
            for (Type current = type; current != null; current = current.BaseType)
            {
                FieldInfo field = current.GetField(name, Stat);
                if (field != null) return field.GetValue(null);
                PropertyInfo property = current.GetProperty(name, Stat);
                if (property != null && property.CanRead) return property.GetValue(null, null);
            }
            return null;
        }

        private static object Get(object instance, string name)
        {
            if (instance == null) return null;
            for (Type current = instance.GetType(); current != null; current = current.BaseType)
            {
                FieldInfo field = current.GetField(name, Inst);
                if (field != null) return field.GetValue(instance);
                PropertyInfo property = current.GetProperty(name, Inst);
                if (property != null && property.CanRead) return property.GetValue(instance, null);
            }
            return null;
        }

        private static string GetId(object instance)
        {
            return Convert.ToString(Get(instance, "id"));
        }

        private static int ToInt(object value)
        {
            return value == null ? 0 : Convert.ToInt32(value);
        }

        private static string Safe(string value)
        {
            if (value == null) return "<null>";
            return "\"" + value
                .Replace("\\", "\\\\")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\"", "\\\"") + "\"";
        }
    }
}
