using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;

namespace PrayerClarity
{
    internal static class RebalancedStaticProjection
    {
        private static ManualLogSource _log;
        private static bool _projectedForCurrentLoad;
        private static bool _projectionFailed;

        internal static void Install(string harmonyId, ManualLogSource log)
        {
            _log = log;
            Type craftComponent = R.GameType("CraftComponent");
            if (craftComponent == null) throw new MissingMemberException("CraftComponent");

            MethodInfo clear = FindMethod(craftComponent, "ClearCraftsListOnGameStart", 0);
            MethodInfo fill = FindMethod(craftComponent, "FillCraftsList", 0);
            if (clear == null) throw new MissingMethodException("CraftComponent.ClearCraftsListOnGameStart()");
            if (fill == null) throw new MissingMethodException("CraftComponent.FillCraftsList()");

            R.Patch(harmonyId + ".projection.reset", typeof(RebalancedStaticProjection), clear, nameof(ClearCraftsListOnGameStartPostfix));
            R.Patch(harmonyId + ".projection.apply", typeof(RebalancedStaticProjection), fill, nameof(FillCraftsListPostfix));
        }

        private static void ClearCraftsListOnGameStartPostfix()
        {
            _projectedForCurrentLoad = false;
            _projectionFailed = false;
        }

        private static void FillCraftsListPostfix()
        {
            if (_projectedForCurrentLoad || _projectionFailed) return;

            try
            {
                ApplyOnce();
                _projectedForCurrentLoad = true;
                _log?.LogInfo("PrayerClarity: Rebalanced static prayer projection applied for this save load.");
            }
            catch (Exception ex)
            {
                _projectionFailed = true;
                _log?.LogError("PrayerClarity: Rebalanced static prayer projection failed closed for this save load. " + ex);
            }
        }

        private static void ApplyOnce()
        {
            foreach (RebalancedPrayerRule rule in RebalancedRuleSet.All)
            {
                if (!HasStaticProjection(rule)) continue;

                for (int tier = 1; tier <= 3; tier++)
                {
                    string craftId = RebalancedRuleSet.CraftId(rule.PrayerId, tier);
                    object craft = R.BalanceData(craftId, "CraftDefinition", true);
                    if (craft == null)
                    {
                        if (rule.OptionalDlc) continue;
                        throw new MissingMemberException("Missing required prayer craft " + craftId);
                    }

                    ApplyStockOwnedFields(craft, rule, tier);
                }
            }
        }

        private static bool HasStaticProjection(RebalancedPrayerRule rule)
        {
            return rule.Requirements != null ||
                   rule.FaithBonusRates != null ||
                   rule.MoneyBonusRates != null ||
                   rule.RemoveFixedFaith ||
                   rule.RemoveFixedMoney;
        }

        private static void ApplyStockOwnedFields(object craft, RebalancedPrayerRule rule, int tier)
        {
            if (rule.Requirements != null)
                R.Set(craft, "needs_quality", rule.TierValue(rule.Requirements, tier));
            if (rule.FaithBonusRates != null)
                R.Set(craft, "k_faith", rule.TierValue(rule.FaithBonusRates, tier));
            if (rule.MoneyBonusRates != null)
                R.Set(craft, "k_money", rule.TierValue(rule.MoneyBonusRates, tier));

            if (rule.RemoveFixedFaith || rule.RemoveFixedMoney)
                RemovePrayerOwnedFixedOutputs(craft, rule.RemoveFixedFaith, rule.RemoveFixedMoney);
        }

        private static void RemovePrayerOwnedFixedOutputs(object craft, bool removeFaith, bool removeMoney)
        {
            IList output = R.Get(craft, "output") as IList;
            if (output == null) throw new MissingMemberException(R.Id(craft) ?? "CraftDefinition", "output");

            for (int i = output.Count - 1; i >= 0; i--)
            {
                object item = output[i];
                string id = R.Id(item) ?? string.Empty;
                if ((removeFaith && string.Equals(id, "faith", StringComparison.Ordinal)) ||
                    (removeMoney && string.Equals(id, "money", StringComparison.Ordinal)))
                    output.RemoveAt(i);
            }
        }

        private static MethodInfo FindMethod(Type type, string name, int parameterCount)
        {
            if (type == null) return null;
            return type.GetMethods(R.Inst | R.Stat)
                .FirstOrDefault(method => method.Name == name && method.GetParameters().Length == parameterCount);
        }
    }
}
