using System;
using System.Collections.Generic;

namespace PrayerClarity
{
    internal enum ReposeQualityMode
    {
        Stock,
        HalfwayToBest,
        Best
    }

    internal sealed class RebalancedPrayerRule
    {
        internal readonly string PrayerId;
        internal readonly bool OptionalDlc;
        internal readonly float[] Requirements;
        internal readonly float[] FaithBonusRates;
        internal readonly float[] MoneyBonusRates;
        internal readonly bool RemoveFixedFaith;
        internal readonly bool RemoveFixedMoney;
        internal readonly float[] GrowthReduction;
        internal readonly float[] ConfessionProbability;
        internal readonly ReposeQualityMode[] ReposeModes;
        internal readonly float[] CombatDamage;
        internal readonly float[] CombatArmor;
        internal readonly float[] CombatRegenPerSecond;
        internal readonly float[] CraftQualityBonus;
        internal readonly string SuccessRewardBaseItemId;
        internal readonly int[] SuccessRewardQualityTiers;
        internal readonly int[] SuccessRewardCounts;
        internal readonly float[] SoulGratitudeBonusRate;
        internal readonly float[] SinShardMultiplier;

        internal RebalancedPrayerRule(
            string prayerId,
            bool optionalDlc = false,
            float[] requirements = null,
            float[] faithBonusRates = null,
            float[] moneyBonusRates = null,
            bool removeFixedFaith = false,
            bool removeFixedMoney = false,
            float[] growthReduction = null,
            float[] confessionProbability = null,
            ReposeQualityMode[] reposeModes = null,
            float[] combatDamage = null,
            float[] combatArmor = null,
            float[] combatRegenPerSecond = null,
            float[] craftQualityBonus = null,
            string successRewardBaseItemId = null,
            int[] successRewardQualityTiers = null,
            int[] successRewardCounts = null,
            float[] soulGratitudeBonusRate = null,
            float[] sinShardMultiplier = null)
        {
            PrayerId = prayerId;
            OptionalDlc = optionalDlc;
            Requirements = requirements;
            FaithBonusRates = faithBonusRates;
            MoneyBonusRates = moneyBonusRates;
            RemoveFixedFaith = removeFixedFaith;
            RemoveFixedMoney = removeFixedMoney;
            GrowthReduction = growthReduction;
            ConfessionProbability = confessionProbability;
            ReposeModes = reposeModes;
            CombatDamage = combatDamage;
            CombatArmor = combatArmor;
            CombatRegenPerSecond = combatRegenPerSecond;
            CraftQualityBonus = craftQualityBonus;
            SuccessRewardBaseItemId = successRewardBaseItemId;
            SuccessRewardQualityTiers = successRewardQualityTiers;
            SuccessRewardCounts = successRewardCounts;
            SoulGratitudeBonusRate = soulGratitudeBonusRate;
            SinShardMultiplier = sinShardMultiplier;
        }

        internal float TierValue(float[] values, int qualityTier, float fallback = 0f)
        {
            if (values == null || qualityTier < 1 || qualityTier > values.Length) return fallback;
            return values[qualityTier - 1];
        }

        internal int TierValue(int[] values, int qualityTier, int fallback = 0)
        {
            if (values == null || qualityTier < 1 || qualityTier > values.Length) return fallback;
            return values[qualityTier - 1];
        }

        internal ReposeQualityMode TierValue(ReposeQualityMode[] values, int qualityTier, ReposeQualityMode fallback)
        {
            if (values == null || qualityTier < 1 || qualityTier > values.Length) return fallback;
            return values[qualityTier - 1];
        }
    }

    internal static class RebalancedRuleSet
    {
        private static readonly Dictionary<string, RebalancedPrayerRule> Rules =
            new Dictionary<string, RebalancedPrayerRule>(StringComparer.Ordinal)
            {
                ["b_empty"] = new RebalancedPrayerRule("b_empty"),

                ["b_faith"] = new RebalancedPrayerRule(
                    "b_faith",
                    requirements: F(25f, 40f, 70f),
                    faithBonusRates: F(2.5f, 3.5f, 4.5f),
                    moneyBonusRates: F(0f, 0f, 0f),
                    removeFixedFaith: true,
                    removeFixedMoney: true),

                ["b_money"] = new RebalancedPrayerRule(
                    "b_money",
                    requirements: F(25f, 40f, 70f),
                    faithBonusRates: F(0f, 0f, 0f),
                    moneyBonusRates: F(2.5f, 3.5f, 4.5f),
                    removeFixedFaith: true),

                ["b_faith_money"] = new RebalancedPrayerRule("b_faith_money"),

                ["b_sins"] = new RebalancedPrayerRule(
                    "b_sins",
                    confessionProbability: F(0.50f, 0.75f, 1.00f)),

                ["b_plant"] = new RebalancedPrayerRule(
                    "b_plant",
                    growthReduction: F(0.20f, 0.30f, 0.40f)),

                ["b_skull"] = new RebalancedPrayerRule(
                    "b_skull",
                    reposeModes: new[]
                    {
                        ReposeQualityMode.Stock,
                        ReposeQualityMode.HalfwayToBest,
                        ReposeQualityMode.Best
                    }),

                ["b_sword"] = new RebalancedPrayerRule(
                    "b_sword",
                    combatDamage: F(5f, 10f, 15f),
                    combatArmor: F(4f, 4f, 4f),
                    combatRegenPerSecond: F(1f, 2f, 4f)),

                // Existing Protection items remain same-quality aliases of Combat.
                ["b_shield"] = new RebalancedPrayerRule(
                    "b_shield",
                    combatDamage: F(5f, 10f, 15f),
                    combatArmor: F(4f, 4f, 4f),
                    combatRegenPerSecond: F(1f, 2f, 4f)),

                ["b_pen"] = new RebalancedPrayerRule(
                    "b_pen",
                    craftQualityBonus: F(0.7f, 0.7f, 0.7f),
                    successRewardBaseItemId: "story",
                    successRewardQualityTiers: I(0, 2, 3),
                    successRewardCounts: I(0, 3, 3)),

                ["b_star"] = new RebalancedPrayerRule(
                    "b_star",
                    craftQualityBonus: F(0.2f, 0.5f, 1.0f)),

                ["b_village"] = new RebalancedPrayerRule("b_village"),

                ["b_souls"] = new RebalancedPrayerRule(
                    "b_souls",
                    optionalDlc: true,
                    requirements: F(25f, 40f, 70f),
                    faithBonusRates: F(2.5f, 3.5f, 4.5f),
                    moneyBonusRates: F(0f, 0f, 0f),
                    removeFixedFaith: true,
                    removeFixedMoney: true),

                ["b_grat_points_incr"] = new RebalancedPrayerRule(
                    "b_grat_points_incr",
                    optionalDlc: true,
                    soulGratitudeBonusRate: F(0.20f, 0.20f, 0.20f)),

                ["b_sin_shard"] = new RebalancedPrayerRule(
                    "b_sin_shard",
                    optionalDlc: true,
                    sinShardMultiplier: F(2f, 2f, 2f))
            };

        internal static IEnumerable<RebalancedPrayerRule> All => Rules.Values;

        internal static bool TryGet(string prayerId, out RebalancedPrayerRule rule)
        {
            return Rules.TryGetValue(prayerId ?? string.Empty, out rule);
        }

        internal static bool TryParseCraftId(string craftId, out RebalancedPrayerRule rule, out int qualityTier)
        {
            rule = null;
            qualityTier = 0;
            if (string.IsNullOrEmpty(craftId) || !craftId.StartsWith("pray:", StringComparison.Ordinal)) return false;

            int lastColon = craftId.LastIndexOf(':');
            if (lastColon <= 5 || lastColon + 1 >= craftId.Length) return false;

            string prayerId = craftId.Substring(5, lastColon - 5);
            if (!int.TryParse(craftId.Substring(lastColon + 1), out qualityTier)) return false;
            if (qualityTier < 1 || qualityTier > 3) return false;
            return TryGet(prayerId, out rule);
        }

        internal static string CraftId(string prayerId, int qualityTier)
        {
            return "pray:" + prayerId + ":" + qualityTier;
        }

        private static float[] F(float a, float b, float c) => new[] { a, b, c };
        private static int[] I(int a, int b, int c) => new[] { a, b, c };
    }
}
