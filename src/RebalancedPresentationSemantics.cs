using System;
using System.Globalization;

namespace PrayerClarity
{
    internal static class RebalancedPresentationSemantics
    {
        internal static void Install()
        {
            PrayerEditionSemantics.Install(
                TryBuildTierEffect,
                TryBuildActiveEffect,
                TryBuildTechnologyEffect,
                TryGetSoulConversion,
                TryGetEffectivePrayEvent);
        }

        private static bool TryGetEffectivePrayEvent(
            string craftId,
            string currentEventId,
            out string effectiveEventId)
        {
            effectiveEventId = currentEventId;
            string mapped;
            if (!RebalancedRuleSet.TryGetEffectivePrayEventId(craftId, out mapped))
                return false;

            effectiveEventId = mapped;
            return true;
        }

        private static bool TryGetSoulConversion(string craftId, out int cap, out int conversion)
        {
            cap = 0;
            conversion = 0;

            RebalancedPrayerRule rule;
            int tier;
            if (!RebalancedRuleSet.TryParseCraftId(craftId, out rule, out tier)) return false;
            if (!string.Equals(rule.PrayerId, "b_souls", StringComparison.Ordinal) ||
                rule.SoulGratitudeFaithCaps == null)
                return false;

            cap = Math.Max(0, rule.TierValue(rule.SoulGratitudeFaithCaps, tier, 0));
            conversion = RebalancedSoulsRepose.GetConversionAmountForCap(cap);
            return cap > 0;
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
                    if (tier <= 0 || !RebalancedRuleSet.TryGet("b_plant", out rule)) return false;
                    text = Localization.F(
                        "rebalanced.active.plant",
                        rule.TierValue(rule.GrowthReduction, tier) * 100f,
                        RebalancedRoots.MaxCombinedGrowthReduction * 100f);
                    return true;
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
                case "buff_sin_shard":
                    tier = RebalancedTierState.GetCapturedTier(RebalancedTierState.SinShardTierParam);
                    return tier > 0 && RebalancedRuleSet.TryGet("b_sin_shard", out rule) && TryBuildRuleEffect(rule, tier, out text, out semanticKey);
                default:
                    return false;
            }
        }

        internal static bool TryBuildTechnologyEffect(string craftId, out string sharedText, out string tierText)
        {
            sharedText = null;
            tierText = null;

            RebalancedPrayerRule rule;
            int tier;
            if (!RebalancedRuleSet.TryParseCraftId(craftId, out rule, out tier)) return false;

            if (rule.GrowthReduction != null)
            {
                sharedText = Localization.F("rebalanced.tech.plant_intro");
                tierText = Localization.F(
                    "rebalanced.tech.plant_tier",
                    rule.TierValue(rule.GrowthReduction, tier) * 100f);
                return true;
            }

            if (rule.ConfessionProbability != null)
            {
                sharedText = Localization.F("rebalanced.tech.sins_intro");
                tierText = Localization.F("rebalanced.active.sins", rule.TierValue(rule.ConfessionProbability, tier) * 100f);
                return true;
            }

            if (rule.ReposeModes != null)
            {
                // The in-world concept is identical in both editions; only the
                // tier-specific reliability is rebalanced. Reuse the shared base
                // localization so sibling editions cannot drift in wording.
                sharedText = Localization.F("active.skull");
                ReposeQualityMode mode = rule.TierValue(rule.ReposeModes, tier, ReposeQualityMode.Stock);
                switch (mode)
                {
                    case ReposeQualityMode.Stock:
                        tierText = Localization.F("rebalanced.active.repose.bronze");
                        return true;
                    case ReposeQualityMode.HalfwayToBest:
                        tierText = Localization.F("rebalanced.active.repose.silver");
                        return true;
                    case ReposeQualityMode.Best:
                        tierText = Localization.F("rebalanced.active.repose.gold");
                        return true;
                }
            }

            if (string.Equals(rule.PrayerId, "b_souls", StringComparison.Ordinal) &&
                rule.SoulGratitudeFaithCaps != null)
            {
                int cap = rule.TierValue(rule.SoulGratitudeFaithCaps, tier, 0);
                sharedText = Localization.F("rebalanced.tech.souls_intro");
                tierText = Localization.F("rebalanced.tech.souls_tier", cap);
                return true;
            }

            if (rule.CombatDamage != null && rule.CombatArmor != null && rule.CombatRegenPerSecond != null)
            {
                sharedText = Localization.F("rebalanced.tech.combat_intro");
                tierText = Localization.F(
                    "rebalanced.active.combat",
                    rule.TierValue(rule.CombatDamage, tier),
                    rule.TierValue(rule.CombatArmor, tier),
                    rule.TierValue(rule.CombatRegenPerSecond, tier));
                return true;
            }

            if (rule.CraftQualityBonus != null)
            {
                float value = rule.TierValue(rule.CraftQualityBonus, tier);
                if (string.Equals(rule.PrayerId, "b_pen", StringComparison.Ordinal))
                {
                    sharedText = Localization.F("tech.effect.imagination_intro") + "\n" +
                                 TechnologyTooltipTextStyle.GoldValueAfterColon(Localization.F("active.pen", value));
                    return true;
                }

                if (string.Equals(rule.PrayerId, "b_star", StringComparison.Ordinal))
                {
                    sharedText = Localization.F("tech.effect.excellence_intro");
                    tierText = Localization.F("active.star", value);
                    return true;
                }
            }

            if (rule.SoulGratitudeBonusRate != null)
            {
                sharedText = Localization.F(
                    "rebalanced.tech.gratitude_intro",
                    rule.TierValue(rule.SoulGratitudeBonusRate, tier) * 100f);
                return true;
            }

            if (rule.SinShardMultiplier != null)
            {
                float value = rule.TierValue(rule.SinShardMultiplier, tier);
                sharedText = Localization.F("rebalanced.tech.sin_shard_intro");
                tierText = Localization.F("rebalanced.active.sin_shard", value);
                return true;
            }

            return false;
        }

        private static bool TryBuildRuleEffect(RebalancedPrayerRule rule, int tier, out string text, out string semanticKey)
        {
            text = null;
            semanticKey = null;
            if (rule == null || tier < 1 || tier > 3) return false;

            if (rule.GrowthReduction != null)
            {
                float value = rule.TierValue(rule.GrowthReduction, tier);
                text = Localization.F(
                    "rebalanced.tech.plant_tier",
                    value * 100f);
                semanticKey = "rebalanced:growth=" + Rv(value) +
                              ";cap=" + Rv(RebalancedRoots.MaxCombinedGrowthReduction);
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
                        break;
                    case ReposeQualityMode.HalfwayToBest:
                        text = Localization.F("rebalanced.active.repose.silver");
                        semanticKey = "rebalanced:repose=halfway";
                        break;
                    case ReposeQualityMode.Best:
                        text = Localization.F("rebalanced.active.repose.gold");
                        semanticKey = "rebalanced:repose=best";
                        break;
                    default:
                        return false;
                }

                text += " " + TechnologyTooltipTextStyle.CorpseQualityCue();
                return true;
            }

            if (rule.SoulGratitudeFaithCaps != null &&
                string.Equals(rule.PrayerId, "b_souls", StringComparison.Ordinal))
            {
                int cap = rule.TierValue(rule.SoulGratitudeFaithCaps, tier, 0);
                text = Localization.F("rebalanced.tech.souls_tier", cap);
                semanticKey = "rebalanced:souls_conversion_cap=" + cap.ToString(CultureInfo.InvariantCulture);
                return true;
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
                text = Localization.F("rebalanced.active.sin_shard", value);
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
