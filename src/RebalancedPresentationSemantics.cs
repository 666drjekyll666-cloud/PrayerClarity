using System;
using System.Globalization;

namespace PrayerClarity
{
    internal static class RebalancedPresentationSemantics
    {
        internal static void Install()
        {
            PrayerEditionSemantics.Install(TryBuildTierEffect, TryBuildActiveEffect);
        }

        internal static bool TryBuildTierEffect(string craftId, string buffId, out string text, out string semanticKey)
        {
            text = null;
            semanticKey = null;

            RebalancedPrayerRule rule;
            int tier;
            if (!RebalancedRuleSet.TryParseCraftId(craftId, out rule, out tier)) return false;
            return TryBuildRuleEffect(rule, tier, out text, out semanticKey);
        }

        internal static bool TryBuildActiveEffect(string buffId, out string text)
        {
            text = null;
            string semanticKey;
            RebalancedPrayerRule rule;
            int tier;

            switch (buffId)
            {
                case "buff_plant":
                    tier = RebalancedTierState.GetCapturedTier(RebalancedTierState.PlantTierParam);
                    return tier > 0 && RebalancedRuleSet.TryGet("b_plant", out rule) && TryBuildRuleEffect(rule, tier, out text, out semanticKey);
                case "buff_sins":
                    tier = RebalancedTierState.GetCapturedTier(RebalancedTierState.ConfessionTierParam);
                    return tier > 0 && RebalancedRuleSet.TryGet("b_sins", out rule) && TryBuildRuleEffect(rule, tier, out text, out semanticKey);
                case "buff_skull":
                    tier = RebalancedTierState.GetCapturedTier(RebalancedTierState.ReposeTierParam);
                    return tier > 0 && RebalancedRuleSet.TryGet("b_skull", out rule) && TryBuildRuleEffect(rule, tier, out text, out semanticKey);
                case "buff_sword":
                    tier = RebalancedTierState.GetCapturedTier(RebalancedTierState.CombatTierParam);
                    return tier > 0 && RebalancedRuleSet.TryGet("b_sword", out rule) && TryBuildRuleEffect(rule, tier, out text, out semanticKey);
                case "buff_star":
                    tier = RebalancedTierState.GetCapturedTier(RebalancedTierState.ExcellenceTierParam);
                    return tier > 0 && RebalancedRuleSet.TryGet("b_star", out rule) && TryBuildRuleEffect(rule, tier, out text, out semanticKey);
                case "buff_gp_increase":
                    return RebalancedRuleSet.TryGet("b_grat_points_incr", out rule) && TryBuildRuleEffect(rule, 1, out text, out semanticKey);
                default:
                    return false;
            }
        }

        private static bool TryBuildRuleEffect(RebalancedPrayerRule rule, int tier, out string text, out string semanticKey)
        {
            text = null;
            semanticKey = null;
            if (rule == null || tier < 1 || tier > 3) return false;

            if (rule.GrowthReduction != null)
            {
                float value = rule.TierValue(rule.GrowthReduction, tier);
                text = Localization.F("rebalanced.active.plant", value * 100f);
                semanticKey = "rebalanced:growth=" + Rv(value);
                return true;
            }

            if (rule.ConfessionProbability != null)
            {
                float value = rule.TierValue(rule.ConfessionProbability, tier);
                text = Localization.F("rebalanced.active.sins", value * 100f);
                semanticKey = "rebalanced:confession=" + Rv(value);
                return true;
            }

            if (rule.ReposeModes != null)
            {
                ReposeQualityMode mode = rule.TierValue(rule.ReposeModes, tier, ReposeQualityMode.Stock);
                switch (mode)
                {
                    case ReposeQualityMode.Stock:
                        text = Localization.F("rebalanced.active.repose.bronze");
                        semanticKey = "rebalanced:repose=stock";
                        return true;
                    case ReposeQualityMode.HalfwayToBest:
                        text = Localization.F("rebalanced.active.repose.silver");
                        semanticKey = "rebalanced:repose=halfway";
                        return true;
                    case ReposeQualityMode.Best:
                        text = Localization.F("rebalanced.active.repose.gold");
                        semanticKey = "rebalanced:repose=best";
                        return true;
                }
            }

            if (rule.CombatDamage != null && rule.CombatArmor != null && rule.CombatRegenPerSecond != null)
            {
                float damage = rule.TierValue(rule.CombatDamage, tier);
                float armor = rule.TierValue(rule.CombatArmor, tier);
                float regen = rule.TierValue(rule.CombatRegenPerSecond, tier);
                text = Localization.F("rebalanced.active.combat", damage, armor, regen);
                semanticKey = "rebalanced:combat=" + Rv(damage) + "," + Rv(armor) + "," + Rv(regen);
                return true;
            }

            if (rule.CraftQualityBonus != null)
            {
                float value = rule.TierValue(rule.CraftQualityBonus, tier);
                if (string.Equals(rule.PrayerId, "b_pen", StringComparison.Ordinal))
                    text = Localization.F("active.pen", value);
                else if (string.Equals(rule.PrayerId, "b_star", StringComparison.Ordinal))
                    text = Localization.F("active.star", value);
                else
                    return false;
                semanticKey = "rebalanced:craft_q=" + Rv(value);
                return true;
            }

            if (rule.SoulGratitudeBonusRate != null)
            {
                float value = rule.TierValue(rule.SoulGratitudeBonusRate, tier);
                text = Localization.F("rebalanced.active.gratitude", value * 100f);
                semanticKey = "rebalanced:gratitude=" + Rv(value);
                return true;
            }

            if (rule.SinShardMultiplier != null)
            {
                float value = rule.TierValue(rule.SinShardMultiplier, tier);
                if (Math.Abs(value - 2f) < 0.0001f)
                    text = Localization.F("active.sin_shard");
                else
                    return false;
                semanticKey = "rebalanced:sin_shard=" + Rv(value);
                return true;
            }

            return false;
        }

        private static string Rv(float value)
        {
            return value.ToString("R", CultureInfo.InvariantCulture);
        }
    }
}
