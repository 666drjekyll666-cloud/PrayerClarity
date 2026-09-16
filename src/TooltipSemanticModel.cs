using System;
using System.Collections;

namespace PrayerClarity
{
    internal static class TooltipSemanticModel
    {
        internal sealed class RewardDetails
        {
            internal readonly string Id;
            internal readonly int Count;

            internal RewardDetails(string id, int count)
            {
                Id = id;
                Count = count;
            }
        }

        internal static RewardDetails ResolveSingleReward(PrayerForecast.TierDetails tier)
        {
            if (tier == null || string.IsNullOrEmpty(tier.CraftId)) return null;

            // Only decompose a pure reward special. Mixed event/buff/reward specials keep
            // the generic verified SpecialCoreText path rather than hiding information.
            string key = tier.SpecialSemanticKey;
            if (string.IsNullOrEmpty(key) ||
                !key.StartsWith("rewards:", StringComparison.Ordinal) ||
                key.IndexOf('|') >= 0)
                return null;

            object craft = R.BalanceData(tier.CraftId, "CraftDefinition", true);
            IEnumerable output = craft == null ? null : R.Get(craft, "output") as IEnumerable;
            if (output == null) return null;

            RewardDetails found = null;
            foreach (object item in output)
            {
                if (item == null) continue;
                string id = R.Id(item) ?? string.Empty;
                int value = R.Int(R.Get(item, "value"));
                if (value <= 0 ||
                    string.Equals(id, "faith", StringComparison.Ordinal) ||
                    string.Equals(id, "money", StringComparison.Ordinal))
                    continue;

                // Property-first reward rendering is deliberately narrow: one concrete
                // non-currency output. Unexpected multi-reward specials fall back to the
                // existing complete SpecialCoreText instead of being partially rendered.
                if (found != null) return null;
                found = new RewardDetails(id, value);
            }

            return found;
        }
    }
}
