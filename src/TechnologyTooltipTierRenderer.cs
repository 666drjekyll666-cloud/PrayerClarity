using System;
using System.Collections.Generic;
using System.Globalization;

namespace PrayerClarity
{
    internal static class TechnologyTooltipTierRenderer
    {
        private const float Epsilon = 0.0001f;
        private const string NoBreakSpace = "\u00A0";
        private const string InlineSeparator = " · ";
        private const int LongEffectWrapThreshold = 45;
        private const int MinimumEffectSegmentLength = 18;

        internal static string Build(List<PrayerForecast.TierDetails> tiers)
        {
            if (tiers == null || tiers.Count == 0) return null;

            tiers.Sort(CompareTiers);
            if (tiers.Count == 1)
                return TooltipDetailsRenderer.BuildSingle(tiers[0]);

            List<string> sections = new List<string>();

            AddSection(sections, BuildSuccessIntro(tiers));
            AddSection(sections, BuildSharedEffect(tiers));
            AddSection(sections, BuildSharedDuration(tiers));

            foreach (PrayerForecast.TierDetails tier in tiers)
                AddSection(sections, BuildTierBlock(tier, tiers));

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

        private static string BuildSuccessIntro(List<PrayerForecast.TierDetails> tiers)
        {
            bool anyFaith = AnyContribution(tiers, t => t.FaithBonusRate, t => t.FixedFaithBonus);
            bool anyMoney = AnyContribution(tiers, t => t.MoneyBonusRate, t => t.FixedMoneyBonus);
            if (!anyFaith && !anyMoney) return null;

            List<string> lines = new List<string>
            {
                Localization.F("tech.success_reward_bonus") + ":"
            };

            List<string> sharedRates = new List<string>();
            if (anyFaith && IsSharedNonZero(tiers, t => t.FaithBonusRate))
                sharedRates.Add(R.VanillaLocalize("faith") + " " + FormatPercent(tiers[0].FaithBonusRate));
            if (anyMoney && IsSharedNonZero(tiers, t => t.MoneyBonusRate))
                sharedRates.Add(Localization.F("tech.donations") + " " + FormatPercent(tiers[0].MoneyBonusRate));
            if (sharedRates.Count > 0)
                lines.Add(string.Join(InlineSeparator, sharedRates.ToArray()));

            List<string> sharedFlat = new List<string>();
            if (anyFaith && IsSharedNonZero(tiers, t => t.FixedFaithBonus))
                sharedFlat.Add(FormatSignedNumber(tiers[0].FixedFaithBonus) + " (faith)");
            if (anyMoney && IsSharedNonZero(tiers, t => t.FixedMoneyBonus))
                sharedFlat.Add(FormatSignedNumber(tiers[0].FixedMoneyBonus) + " (slv)");
            if (sharedFlat.Count > 0)
                lines.Add(string.Join(InlineSeparator, sharedFlat.ToArray()));

            return string.Join("\n", lines.ToArray());
        }

        private static string BuildSharedEffect(List<PrayerForecast.TierDetails> tiers)
        {
            TooltipSemanticModel.RewardDetails commonReward;
            if (TryGetCommonReward(tiers, out commonReward))
            {
                return Localization.F("forecast.effect_header") + ":\n" +
                       R.VanillaLocalize(commonReward.Id) + " ×" +
                       commonReward.Count.ToString(CultureInfo.InvariantCulture);
            }

            if (!IsSpecialShared(tiers)) return null;
            string core = tiers[0].SpecialCoreText;
            if (string.IsNullOrEmpty(core)) return null;

            return Localization.F("forecast.effect_header") + ":\n" + WrapLongEffect(core);
        }

        private static string BuildSharedDuration(List<PrayerForecast.TierDetails> tiers)
        {
            if (!IsDurationShared(tiers)) return null;
            return Localization.F("tech.duration") + ": " +
                   Localization.F("active.timer_days", tiers[0].SpecialDurationDays);
        }

        private static string BuildTierBlock(
            PrayerForecast.TierDetails tier,
            List<PrayerForecast.TierDetails> allTiers)
        {
            if (tier == null) return null;

            List<string> lines = new List<string>();

            string quality = QualityLabel(tier.QualityTier);
            if (tier.Requirement > 0)
            {
                string threshold = Localization.F("tech.success_threshold", tier.Requirement);
                string first = string.IsNullOrEmpty(quality)
                    ? threshold
                    : quality + NoBreakSpace + threshold;
                lines.Add(MakeAtomic(first));
            }
            else if (!string.IsNullOrEmpty(quality))
            {
                lines.Add(quality);
            }

            string rates = BuildTierRateLine(tier, allTiers);
            if (!string.IsNullOrEmpty(rates)) lines.Add(rates);

            string fixedAndReward = BuildTierFixedAndRewardLine(tier, allTiers);
            if (!string.IsNullOrEmpty(fixedAndReward)) lines.Add(fixedAndReward);

            if (!IsSpecialShared(allTiers))
            {
                TooltipSemanticModel.RewardDetails reward = TooltipSemanticModel.ResolveSingleReward(tier);
                if (reward == null && !string.IsNullOrEmpty(tier.SpecialCoreText))
                    lines.Add(Localization.F("forecast.effect_header") + ": " + WrapLongEffect(tier.SpecialCoreText));
            }

            if (!IsDurationShared(allTiers) && tier.HasSpecialDuration)
            {
                lines.Add(
                    Localization.F("tech.duration") + ": " +
                    Localization.F("active.timer_days", tier.SpecialDurationDays));
            }

            return lines.Count == 0 ? null : string.Join("\n", lines.ToArray());
        }

        private static string BuildTierRateLine(
            PrayerForecast.TierDetails tier,
            List<PrayerForecast.TierDetails> allTiers)
        {
            List<string> parts = new List<string>();

            if (Math.Abs(tier.FaithBonusRate) >= Epsilon &&
                !IsSharedNonZero(allTiers, t => t.FaithBonusRate))
                parts.Add(R.VanillaLocalize("faith") + " " + FormatPercent(tier.FaithBonusRate));

            if (Math.Abs(tier.MoneyBonusRate) >= Epsilon &&
                !IsSharedNonZero(allTiers, t => t.MoneyBonusRate))
                parts.Add(Localization.F("tech.donations") + " " + FormatPercent(tier.MoneyBonusRate));

            return parts.Count == 0 ? null : string.Join(InlineSeparator, parts.ToArray());
        }

        private static string BuildTierFixedAndRewardLine(
            PrayerForecast.TierDetails tier,
            List<PrayerForecast.TierDetails> allTiers)
        {
            List<string> parts = new List<string>();

            if (Math.Abs(tier.FixedFaithBonus) >= Epsilon &&
                !IsSharedNonZero(allTiers, t => t.FixedFaithBonus))
                parts.Add(FormatSignedNumber(tier.FixedFaithBonus) + " (faith)");

            if (Math.Abs(tier.FixedMoneyBonus) >= Epsilon &&
                !IsSharedNonZero(allTiers, t => t.FixedMoneyBonus))
                parts.Add(FormatSignedNumber(tier.FixedMoneyBonus) + " (slv)");

            TooltipSemanticModel.RewardDetails commonReward;
            bool rewardIsShared = TryGetCommonReward(allTiers, out commonReward);
            TooltipSemanticModel.RewardDetails reward = TooltipSemanticModel.ResolveSingleReward(tier);
            if (!rewardIsShared && reward != null)
            {
                parts.Add(
                    R.VanillaLocalize(reward.Id) + " ×" +
                    reward.Count.ToString(CultureInfo.InvariantCulture));
            }

            return parts.Count == 0 ? null : string.Join(InlineSeparator, parts.ToArray());
        }

        private static string WrapLongEffect(string text)
        {
            if (string.IsNullOrEmpty(text) ||
                text.Length <= LongEffectWrapThreshold ||
                text.IndexOf('\n') >= 0)
                return text;

            int min = MinimumEffectSegmentLength;
            int max = text.Length - MinimumEffectSegmentLength;
            if (max <= min) return text;

            int target = text.Length / 2;
            int split = -1;
            int bestDistance = int.MaxValue;

            for (int i = min; i <= max; i++)
            {
                if (!char.IsWhiteSpace(text[i])) continue;
                int distance = Math.Abs(i - target);
                if (distance >= bestDistance) continue;
                bestDistance = distance;
                split = i;
            }

            if (split < 0) return text;
            return text.Substring(0, split).TrimEnd() + "\n" + text.Substring(split + 1).TrimStart();
        }

        private static bool AnyContribution(
            List<PrayerForecast.TierDetails> tiers,
            Func<PrayerForecast.TierDetails, float> rate,
            Func<PrayerForecast.TierDetails, float> fixedValue)
        {
            foreach (PrayerForecast.TierDetails tier in tiers)
            {
                if (tier == null) continue;
                if (Math.Abs(rate(tier)) >= Epsilon || Math.Abs(fixedValue(tier)) >= Epsilon)
                    return true;
            }
            return false;
        }

        private static bool IsSharedNonZero(
            List<PrayerForecast.TierDetails> tiers,
            Func<PrayerForecast.TierDetails, float> selector)
        {
            if (tiers == null || tiers.Count == 0) return false;
            float first = selector(tiers[0]);
            if (Math.Abs(first) < Epsilon) return false;

            for (int i = 1; i < tiers.Count; i++)
            {
                if (Math.Abs(selector(tiers[i]) - first) >= Epsilon)
                    return false;
            }
            return true;
        }

        private static bool IsSpecialShared(List<PrayerForecast.TierDetails> tiers)
        {
            if (tiers == null || tiers.Count == 0) return false;
            string firstKey = tiers[0].SpecialSemanticKey;
            string firstText = tiers[0].SpecialCoreText;
            if (string.IsNullOrEmpty(firstKey) || string.IsNullOrEmpty(firstText)) return false;

            for (int i = 1; i < tiers.Count; i++)
            {
                if (!string.Equals(firstKey, tiers[i].SpecialSemanticKey, StringComparison.Ordinal) ||
                    !string.Equals(firstText, tiers[i].SpecialCoreText, StringComparison.Ordinal))
                    return false;
            }
            return true;
        }

        private static bool IsDurationShared(List<PrayerForecast.TierDetails> tiers)
        {
            if (tiers == null || tiers.Count == 0 || !tiers[0].HasSpecialDuration) return false;
            float first = tiers[0].SpecialDurationDays;

            for (int i = 1; i < tiers.Count; i++)
            {
                if (!tiers[i].HasSpecialDuration ||
                    Math.Abs(tiers[i].SpecialDurationDays - first) >= Epsilon)
                    return false;
            }
            return true;
        }

        private static bool TryGetCommonReward(
            List<PrayerForecast.TierDetails> tiers,
            out TooltipSemanticModel.RewardDetails common)
        {
            common = null;
            if (tiers == null || tiers.Count == 0) return false;

            TooltipSemanticModel.RewardDetails first = TooltipSemanticModel.ResolveSingleReward(tiers[0]);
            if (first == null) return false;

            for (int i = 1; i < tiers.Count; i++)
            {
                TooltipSemanticModel.RewardDetails next = TooltipSemanticModel.ResolveSingleReward(tiers[i]);
                if (next == null ||
                    !string.Equals(first.Id, next.Id, StringComparison.Ordinal) ||
                    first.Count != next.Count)
                    return false;
            }

            common = first;
            return true;
        }

        private static string FormatPercent(float rate)
        {
            float percent = rate * 100f;
            if (Math.Abs(percent) < Epsilon) return "0%";
            string sign = percent > 0f ? "+" : "−";
            return sign + Math.Abs(percent).ToString("0.##", CultureInfo.InvariantCulture) + "%";
        }

        private static string FormatSignedNumber(float value)
        {
            if (Math.Abs(value) < Epsilon) return "0";
            string sign = value > 0f ? "+" : "−";
            return sign + Math.Abs(value).ToString("0.##", CultureInfo.InvariantCulture);
        }

        private static string MakeAtomic(string text)
        {
            return string.IsNullOrEmpty(text) ? text : text.Replace(" ", NoBreakSpace);
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
