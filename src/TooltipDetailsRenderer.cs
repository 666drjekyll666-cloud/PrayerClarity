using System;
using System.Collections.Generic;
using System.Globalization;

namespace PrayerClarity
{
    internal sealed class TooltipPresentationSections
    {
        internal string BaseResult;
        internal string SuccessBonuses;

        internal bool HasContent
        {
            get { return !string.IsNullOrEmpty(BaseResult) || !string.IsNullOrEmpty(SuccessBonuses); }
        }
    }

    internal static class TooltipDetailsRenderer
    {
        private const float Epsilon = 0.0001f;
        private const string NoBreakSpace = "\u00A0";

        internal static TooltipPresentationSections BuildSingleSections(PrayerForecast.TierDetails tier)
        {
            if (tier == null) return null;

            return new TooltipPresentationSections
            {
                BaseResult = PresentationText.DependencyMap(tier.UsesSoulGratitude),
                SuccessBonuses = BuildSingleSuccessBonuses(tier)
            };
        }

        private static string BuildSingleSuccessBonuses(PrayerForecast.TierDetails tier)
        {
            List<string> sections = new List<string>();

            if (tier.Requirement > 0)
            {
                string quality = TierPrefix(tier, false);
                string threshold = Localization.F("tech.success_threshold", tier.Requirement);
                sections.Add(string.IsNullOrEmpty(quality)
                    ? threshold
                    : quality + NoBreakSpace + threshold);
            }

            string faith = BuildSingleExplicitResourceBlock(
                tier,
                R.VanillaLocalize("faith"),
                "(faith)",
                t => t.FaithBonusRate,
                t => t.FixedFaithBonus);
            AddSection(sections, faith);

            string money = BuildSingleExplicitResourceBlock(
                tier,
                Localization.F("tech.donations"),
                "(slv)",
                t => t.MoneyBonusRate,
                t => t.FixedMoneyBonus);
            AddSection(sections, money);

            List<PrayerForecast.TierDetails> tiers = new List<PrayerForecast.TierDetails> { tier };
            AddSection(sections, BuildEffect(tiers, false));
            AddSection(sections, BuildDuration(tiers, false));

            return sections.Count == 0 ? null : string.Join("\n\n", sections.ToArray());
        }

        private static string BuildSingleExplicitResourceBlock(
            PrayerForecast.TierDetails tier,
            string label,
            string icon,
            Func<PrayerForecast.TierDetails, float> rate,
            Func<PrayerForecast.TierDetails, float> fixedValue)
        {
            float rateValue = rate(tier);
            float fixedAmount = fixedValue(tier);
            if (Math.Abs(rateValue) < Epsilon && Math.Abs(fixedAmount) < Epsilon) return null;

            List<string> lines = new List<string> { label + ":" };
            if (Math.Abs(rateValue) >= Epsilon)
            {
                lines.Add(
                    icon + NoBreakSpace +
                    Localization.F("tech.percent_of_base", FormatPercent(rateValue, false)));
            }

            if (Math.Abs(fixedAmount) >= Epsilon)
                lines.Add(icon + NoBreakSpace + FormatSignedNumber(fixedAmount, false));

            return string.Join("\n", lines.ToArray());
        }

        internal static string BuildComparative(List<PrayerForecast.TierDetails> tiers)
        {
            if (tiers == null || tiers.Count == 0) return null;

            tiers.Sort(CompareTiers);
            List<string> sections = new List<string>();

            AddSection(sections, BuildRequirements(tiers, true));
            AddSection(sections, BuildSuccess(tiers, true));
            AddSection(sections, BuildEffect(tiers, true));
            AddSection(sections, BuildDuration(tiers, true));

            return sections.Count == 0 ? null : string.Join("\n\n", sections.ToArray());
        }

        internal static string BuildSingle(PrayerForecast.TierDetails tier)
        {
            if (tier == null) return null;

            List<PrayerForecast.TierDetails> tiers = new List<PrayerForecast.TierDetails> { tier };
            List<string> sections = new List<string>();

            AddSection(sections, BuildRequirements(tiers, false));
            AddSection(sections, BuildSuccess(tiers, false));
            AddSection(sections, BuildEffect(tiers, false));
            AddSection(sections, BuildDuration(tiers, false));

            return sections.Count == 0 ? null : string.Join("\n\n", sections.ToArray());
        }

        private static void AddSection(List<string> sections, string section)
        {
            if (!string.IsNullOrEmpty(section)) sections.Add(section);
        }

        private static int CompareTiers(PrayerForecast.TierDetails a, PrayerForecast.TierDetails b)
        {
            int aq = a == null || a.QualityTier <= 0 ? int.MaxValue : a.QualityTier;
            int bq = b == null || b.QualityTier <= 0 ? int.MaxValue : b.QualityTier;
            int quality = aq.CompareTo(bq);
            if (quality != 0) return quality;
            return string.CompareOrdinal(a == null ? null : a.CraftId, b == null ? null : b.CraftId);
        }

        private static string BuildRequirements(List<PrayerForecast.TierDetails> tiers, bool comparative)
        {
            bool any = false;
            foreach (PrayerForecast.TierDetails tier in tiers)
            {
                if (tier != null && tier.Requirement > 0)
                {
                    any = true;
                    break;
                }
            }
            if (!any) return null;

            if (!comparative || tiers.Count == 1)
            {
                PrayerForecast.TierDetails tier = tiers[0];
                return Localization.F("tech.requires_header") + ": " +
                       TierPrefix(tier, true) +
                       tier.Requirement.ToString(CultureInfo.InvariantCulture) + " (cross)";
            }

            bool same = true;
            int first = tiers[0].Requirement;
            for (int i = 1; i < tiers.Count; i++)
            {
                if (tiers[i].Requirement != first)
                {
                    same = false;
                    break;
                }
            }

            if (same)
                return Localization.F("tech.requires_header") + ": " +
                       first.ToString(CultureInfo.InvariantCulture) + " (cross)";

            List<string> values = new List<string>();
            foreach (PrayerForecast.TierDetails tier in tiers)
            {
                values.Add(
                    TierPrefix(tier, false) + NoBreakSpace +
                    tier.Requirement.ToString(CultureInfo.InvariantCulture) +
                    NoBreakSpace + "(cross)");
            }

            return Localization.F("tech.requires_header") + ":\n" + JoinAtomicSegments(values);
        }

        private static string BuildSuccess(List<PrayerForecast.TierDetails> tiers, bool comparative)
        {
            bool anyFaith = AnyContribution(
                tiers,
                t => t.FaithBonusRate,
                t => t.FixedFaithBonus);
            bool anyMoney = AnyContribution(
                tiers,
                t => t.MoneyBonusRate,
                t => t.FixedMoneyBonus);

            if (!anyFaith && !anyMoney) return null;

            if (!comparative || tiers.Count == 1)
            {
                List<string> singleLines = new List<string>
                {
                    Localization.F("tech.success_bonus") + ":"
                };

                if (anyFaith)
                    singleLines.Add(BuildSingleResourceLine(
                        tiers[0],
                        "(faith)",
                        t => t.FaithBonusRate,
                        t => t.FixedFaithBonus));

                if (anyMoney)
                    singleLines.Add(BuildSingleResourceLine(
                        tiers[0],
                        "(slv) " + Localization.F("tech.donations"),
                        t => t.MoneyBonusRate,
                        t => t.FixedMoneyBonus));

                return string.Join("\n", singleLines.ToArray());
            }

            List<string> resourceBlocks = new List<string>();
            if (anyFaith)
                resourceBlocks.Add(BuildComparativeResourceBlock(
                    tiers,
                    R.VanillaLocalize("faith"),
                    t => t.FaithBonusRate,
                    t => t.FixedFaithBonus));

            if (anyMoney)
                resourceBlocks.Add(BuildComparativeResourceBlock(
                    tiers,
                    Localization.F("tech.donations"),
                    t => t.MoneyBonusRate,
                    t => t.FixedMoneyBonus));

            return Localization.F("tech.success_reward_bonus") + ":\n" +
                   string.Join("\n\n", resourceBlocks.ToArray());
        }

        private static bool AnyContribution(
            List<PrayerForecast.TierDetails> tiers,
            Func<PrayerForecast.TierDetails, float> rate,
            Func<PrayerForecast.TierDetails, float> fixedValue)
        {
            foreach (PrayerForecast.TierDetails tier in tiers)
            {
                if (Math.Abs(rate(tier)) >= Epsilon || Math.Abs(fixedValue(tier)) >= Epsilon)
                    return true;
            }
            return false;
        }

        private static string BuildSingleResourceLine(
            PrayerForecast.TierDetails tier,
            string label,
            Func<PrayerForecast.TierDetails, float> rate,
            Func<PrayerForecast.TierDetails, float> fixedValue)
        {
            string value = FormatCombined(rate(tier), fixedValue(tier), false);
            return label + (string.IsNullOrEmpty(value) ? string.Empty : " " + value);
        }

        private static string BuildComparativeResourceBlock(
            List<PrayerForecast.TierDetails> tiers,
            string label,
            Func<PrayerForecast.TierDetails, float> rate,
            Func<PrayerForecast.TierDetails, float> fixedValue)
        {
            bool rateSame = AllEqual(tiers, rate);
            bool fixedSame = AllEqual(tiers, fixedValue);
            float firstRate = rate(tiers[0]);
            float firstFixed = fixedValue(tiers[0]);
            string resourceHeader = label + ":";

            if (rateSame && fixedSame)
            {
                string common = FormatCombined(firstRate, firstFixed, false);
                return resourceHeader +
                       (string.IsNullOrEmpty(common) ? string.Empty : " " + common);
            }

            bool showRate = AnyNonZero(tiers, rate);
            bool showFixed = AnyNonZero(tiers, fixedValue);
            List<string> values = new List<string>();

            foreach (PrayerForecast.TierDetails tier in tiers)
            {
                List<string> parts = new List<string>();
                if (showRate) parts.Add(FormatPercent(rate(tier), true));
                if (showFixed) parts.Add(FormatSignedNumber(fixedValue(tier), true));

                values.Add(
                    TierPrefix(tier, false) + ":" + NoBreakSpace +
                    string.Join(NoBreakSpace, parts.ToArray()));
            }

            return resourceHeader + "\n" + JoinAtomicSegments(values);
        }

        private static bool AnyNonZero(
            List<PrayerForecast.TierDetails> tiers,
            Func<PrayerForecast.TierDetails, float> selector)
        {
            foreach (PrayerForecast.TierDetails tier in tiers)
            {
                if (Math.Abs(selector(tier)) >= Epsilon) return true;
            }
            return false;
        }

        private static bool AllEqual(
            List<PrayerForecast.TierDetails> tiers,
            Func<PrayerForecast.TierDetails, float> selector)
        {
            float first = selector(tiers[0]);
            for (int i = 1; i < tiers.Count; i++)
            {
                if (Math.Abs(selector(tiers[i]) - first) >= Epsilon)
                    return false;
            }
            return true;
        }

        private static string BuildEffect(List<PrayerForecast.TierDetails> tiers, bool comparative)
        {
            string reward = BuildStructuredRewardEffect(tiers, comparative);
            if (!string.IsNullOrEmpty(reward)) return reward;

            bool any = false;
            foreach (PrayerForecast.TierDetails tier in tiers)
            {
                if (tier != null && !string.IsNullOrEmpty(tier.SpecialCoreText))
                {
                    any = true;
                    break;
                }
            }
            if (!any) return null;

            string header = Localization.F("forecast.effect_header") + ":";

            if (!comparative || tiers.Count == 1)
                return header + "\n" + (tiers[0].SpecialCoreText ?? "—");

            bool same = AllSpecialKeysEqual(tiers);
            if (same)
                return header + "\n" + (tiers[0].SpecialCoreText ?? "—");

            List<string> lines = new List<string> { header };
            foreach (PrayerForecast.TierDetails tier in tiers)
                lines.Add(TierPrefix(tier, true) + (string.IsNullOrEmpty(tier.SpecialCoreText) ? "—" : tier.SpecialCoreText));

            return string.Join("\n", lines.ToArray());
        }

        private static string BuildStructuredRewardEffect(List<PrayerForecast.TierDetails> tiers, bool comparative)
        {
            List<TooltipSemanticModel.RewardDetails> rewards = new List<TooltipSemanticModel.RewardDetails>();
            foreach (PrayerForecast.TierDetails tier in tiers)
            {
                TooltipSemanticModel.RewardDetails reward = TooltipSemanticModel.ResolveSingleReward(tier);
                if (reward == null) return null;
                rewards.Add(reward);
            }
            if (rewards.Count == 0) return null;

            string rewardId = rewards[0].Id;
            for (int i = 1; i < rewards.Count; i++)
            {
                if (!string.Equals(rewardId, rewards[i].Id, StringComparison.Ordinal)) return null;
            }

            List<string> lines = new List<string>
            {
                Localization.F("forecast.effect_header") + ":",
                R.VanillaLocalize(rewardId)
            };

            bool sameCount = true;
            int firstCount = rewards[0].Count;
            for (int i = 1; i < rewards.Count; i++)
            {
                if (rewards[i].Count != firstCount)
                {
                    sameCount = false;
                    break;
                }
            }

            if (!comparative || tiers.Count == 1 || sameCount)
            {
                lines.Add(Localization.F("tech.quantity") + ": ×" + firstCount.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                List<string> values = new List<string>();
                for (int i = 0; i < tiers.Count; i++)
                {
                    values.Add(
                        TierPrefix(tiers[i], false) + NoBreakSpace +
                        "×" + rewards[i].Count.ToString(CultureInfo.InvariantCulture));
                }
                lines.Add(Localization.F("tech.quantity") + ":\n" + JoinAtomicSegments(values));
            }

            if (string.Equals(rewardId, "blessing_commerce", StringComparison.Ordinal))
            {
                string description = R.VanillaLocalize("blessing_commerce_d");
                if (!string.IsNullOrEmpty(description) &&
                    !string.Equals(description, "blessing_commerce_d", StringComparison.Ordinal))
                    lines.Add(description);
            }

            return string.Join("\n", lines.ToArray());
        }

        private static bool AllSpecialKeysEqual(List<PrayerForecast.TierDetails> tiers)
        {
            string first = tiers[0].SpecialSemanticKey;
            for (int i = 1; i < tiers.Count; i++)
            {
                if (!string.Equals(first, tiers[i].SpecialSemanticKey, StringComparison.Ordinal))
                    return false;
            }
            return true;
        }

        private static string BuildDuration(List<PrayerForecast.TierDetails> tiers, bool comparative)
        {
            bool any = false;
            foreach (PrayerForecast.TierDetails tier in tiers)
            {
                if (tier != null && tier.HasSpecialDuration)
                {
                    any = true;
                    break;
                }
            }
            if (!any) return null;

            string header = Localization.F("tech.duration");

            if (!comparative || tiers.Count == 1)
            {
                PrayerForecast.TierDetails tier = tiers[0];
                return tier.HasSpecialDuration
                    ? header + ": " + Localization.F("active.timer_days", tier.SpecialDurationDays)
                    : null;
            }

            bool same = tiers[0].HasSpecialDuration;
            float first = tiers[0].SpecialDurationDays;
            for (int i = 1; i < tiers.Count && same; i++)
            {
                if (!tiers[i].HasSpecialDuration ||
                    Math.Abs(tiers[i].SpecialDurationDays - first) >= Epsilon)
                    same = false;
            }

            if (same)
                return header + ": " + Localization.F("active.timer_days", first);

            List<string> values = new List<string>();
            foreach (PrayerForecast.TierDetails tier in tiers)
            {
                string value = tier.HasSpecialDuration
                    ? Localization.F("active.timer_days", tier.SpecialDurationDays)
                    : "—";
                values.Add(
                    TierPrefix(tier, false) + ":" + NoBreakSpace +
                    value.Replace(" ", NoBreakSpace));
            }

            return header + ":\n" + JoinAtomicSegments(values);
        }

        private static string JoinAtomicSegments(List<string> segments)
        {
            return string.Join(" ", segments.ToArray());
        }

        private static string FormatCombined(float rate, float fixedValue, bool includeZeros)
        {
            List<string> parts = new List<string>();
            if (includeZeros || Math.Abs(rate) >= Epsilon)
                parts.Add(FormatPercent(rate, includeZeros));
            if (includeZeros || Math.Abs(fixedValue) >= Epsilon)
                parts.Add(FormatSignedNumber(fixedValue, includeZeros));
            return string.Join(" ", parts.ToArray());
        }

        private static string FormatPercent(float rate, bool includeZero)
        {
            float percent = rate * 100f;
            if (Math.Abs(percent) < Epsilon)
                return includeZero ? "0%" : string.Empty;

            string sign = percent > 0f ? "+" : "−";
            return sign + Math.Abs(percent).ToString("0.##", CultureInfo.InvariantCulture) + "%";
        }

        private static string FormatSignedNumber(float value, bool includeZero)
        {
            if (Math.Abs(value) < Epsilon)
                return includeZero ? "0" : string.Empty;

            string sign = value > 0f ? "+" : "−";
            return sign + Math.Abs(value).ToString("0.##", CultureInfo.InvariantCulture);
        }

        private static string TierPrefix(PrayerForecast.TierDetails tier, bool trailingSpace)
        {
            string quality = tier == null ? null : QualityLabel(tier.QualityTier);
            if (string.IsNullOrEmpty(quality)) return string.Empty;
            return quality + (trailingSpace ? " " : string.Empty);
        }

        private static string QualityLabel(int qualityTier)
        {
            switch (qualityTier)
            {
                case 1: return Localization.F("quality.bronze");
                case 2: return Localization.F("quality.silver");
                case 3: return Localization.F("quality.gold");
                default: return null;
            }
        }
    }
}
