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

                // Technology may decompose one concrete item reward even when the same
                // prayer also has a timed/special effect (for example Rebalanced
                // Imagination). Unexpected multi-item rewards still fall back to the
                // complete special-text path rather than being partially rendered.
                if (found != null) return null;
                found = new RewardDetails(id, value);
            }

            return found;
        }
    }
}
