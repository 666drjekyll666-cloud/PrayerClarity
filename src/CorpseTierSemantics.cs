using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace PrayerClarity
{
    internal static class CorpseTierSemantics
    {
        private static MethodInfo _findBuffById;

        internal static bool TryGetHighestExistingOrdinaryTier(int tierMin, int tierMax, out int bestTier)
        {
            bestTier = int.MinValue;
            if (tierMax < tierMin) return false;

            IEnumerable bodies = GetBodies();
            if (bodies == null) return false;

            foreach (object body in bodies)
            {
                if (body == null) continue;
                string linkedItemId = R.Get(body, "linked_item_id") as string;
                if (!string.Equals(linkedItemId, "body", StringComparison.Ordinal)) continue;

                int tier = R.Int(R.Get(body, "tier"));
                if (tier < tierMin || tier > tierMax) continue;
                if (tier > bestTier) bestTier = tier;
            }

            return bestTier != int.MinValue;
        }

        internal static int CountExistingOrdinaryTiers(int tierMin, int tierMax)
        {
            if (tierMax < tierMin) return 0;

            IEnumerable bodies = GetBodies();
            if (bodies == null) return 0;

            HashSet<int> tiers = new HashSet<int>();
            foreach (object body in bodies)
            {
                if (body == null) continue;
                string linkedItemId = R.Get(body, "linked_item_id") as string;
                if (!string.Equals(linkedItemId, "body", StringComparison.Ordinal)) continue;

                int tier = R.Int(R.Get(body, "tier"));
                if (tier >= tierMin && tier <= tierMax)
                    tiers.Add(tier);
            }

            return tiers.Count;
        }

        internal static bool TryGetCurrentStockReposeCandidateRange(out int tierMin, out int tierMax)
        {
            tierMin = 0;
            tierMax = 0;

            try
            {
                int bodyMin = ToInt(R.PlayerParam("body_min"));
                int bodyMax = ToInt(R.PlayerParam("body_max"));
                if (HasLiveBuff("buff_skull"))
                    bodyMax -= 1;

                int addMin = ToInt(R.PlayerParam("add_body_min"));
                int addMax = ToInt(R.PlayerParam("add_body_max"));

                tierMin = bodyMin + addMin;
                tierMax = bodyMax + addMax + 1;
                return tierMax >= tierMin;
            }
            catch
            {
                return false;
            }
        }

        internal static bool StockReposeAddsHigherOrdinaryTier()
        {
            int tierMin;
            int buffedTierMax;
            if (!TryGetCurrentStockReposeCandidateRange(out tierMin, out buffedTierMax))
                return true;

            int unbuffedTierMax = buffedTierMax - 1;
            int before = CountExistingOrdinaryTiers(tierMin, unbuffedTierMax);
            int after = CountExistingOrdinaryTiers(tierMin, buffedTierMax);
            return after > before;
        }

        internal static bool BestTierNarrowingChangesDistribution()
        {
            int tierMin;
            int tierMax;
            if (!TryGetCurrentStockReposeCandidateRange(out tierMin, out tierMax))
                return true;
            return CountExistingOrdinaryTiers(tierMin, tierMax) > 1;
        }

        private static IEnumerable GetBodies()
        {
            object balance = R.GetStatic(R.GameType("GameBalance"), "me");
            return balance == null ? null : R.Get(balance, "bodies_data") as IEnumerable;
        }

        private static bool HasLiveBuff(string buffId)
        {
            if (_findBuffById == null)
            {
                Type buffsLogics = R.GameType("BuffsLogics");
                _findBuffById = R.Method(buffsLogics, "FindBuffByID", true, new[] { typeof(string) });
            }

            return _findBuffById != null &&
                   _findBuffById.Invoke(null, new object[] { buffId }) != null;
        }

        private static int ToInt(float value)
        {
            return (int)Math.Round(value);
        }
    }
}
