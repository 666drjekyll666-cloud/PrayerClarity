using BepInEx;

namespace PrayerClarityResearch
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(RebalancedPluginGuid, BepInDependency.DependencyFlags.HardDependency)]
    public sealed class PrayerClarityTestHarnessRebalancedCompat : BaseUnityPlugin
    {
        public const string PluginGuid = "nikich.graveyardkeeper.prayerclarity";
        public const string RebalancedPluginGuid = "nikich.graveyardkeeper.prayerclarity.rebalanced";
        public const string PluginName = "PrayerClarity Test Harness Rebalanced Compatibility";
        public const string PluginVersion = "0.1.0";

        private void Awake()
        {
            Logger.LogInfo("PrayerClarity Test Harness compatibility alias active for PrayerClarity: Rebalanced. No gameplay or UI behavior is patched.");
        }
    }
}
