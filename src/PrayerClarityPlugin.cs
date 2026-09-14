using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;

namespace PrayerClarity
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class PrayerClarityPlugin : BaseUnityPlugin
    {
        internal const string PluginGuid = "nikich.graveyardkeeper.prayerclarity";
        internal const string PluginName = "PrayerClarity";
        internal const string PluginVersion = "0.1.1";
        private static readonly Guid SupportedGameMvid = new Guid("6f50b8e7-156b-49ac-bbe8-7505894b2364");
        private static ManualLogSource _log;
        private static bool _runtimeErrorLogged;

        private void Awake()
        {
            _log = Logger;
            try
            {
                if (!R.BindGameAssembly())
                {
                    Logger.LogError("Assembly-CSharp is unavailable; PrayerClarity is disabled.");
                    return;
                }

                Guid actualMvid = R.GameAssembly.ManifestModule.ModuleVersionId;
                if (actualMvid != SupportedGameMvid)
                {
                    Logger.LogWarning("Unsupported Graveyard Keeper build (Assembly-CSharp MVID " + actualMvid + "). PrayerClarity is disabled rather than patching an unverified build.");
                    return;
                }

                Localization.Initialize(Assembly.GetExecutingAssembly(), Logger);

                Type prayGui = R.GameType("PrayCraftGUI");
                MethodInfo redraw = R.Method(prayGui, "RedrawTextValues", false, new[] { typeof(float), typeof(float) });
                R.Patch(PluginGuid, typeof(PrayerClarityPlugin), redraw, nameof(RedrawTextValuesPostfix));

                Logger.LogInfo(PluginName + " " + PluginVersion + " loaded. Clarity-only pulpit forecast; no prayer mechanics are changed.");
            }
            catch (Exception ex)
            {
                Logger.LogError("PrayerClarity initialization failed: " + ex);
            }
        }

        private static void RedrawTextValuesPostfix(object __instance)
        {
            try
            {
                string forecast = PrayerForecast.Build(__instance);
                if (string.IsNullOrEmpty(forecast)) return;

                object label = R.Get(__instance, "l_total_values");
                if (label == null) return;
                string vanilla = R.Get(label, "text") as string ?? string.Empty;
                R.Set(label, "text", vanilla + "\n" + forecast);
            }
            catch (Exception ex)
            {
                if (_runtimeErrorLogged) return;
                _runtimeErrorLogged = true;
                _log?.LogError("PrayerClarity forecast failed; vanilla pulpit UI remains available. " + ex);
            }
        }
    }
}
