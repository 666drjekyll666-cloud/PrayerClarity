using System;
using System.Collections.Generic;
using System.Globalization;

namespace PrayerClarity
{
    internal static class TechnologyTooltipTierRenderer
    {
        private const float Epsilon = 0.0001f;
        private const string NoBreakSpace = "\u00A0";

        internal static TooltipPresentationSections BuildSections(List<PrayerForecast.TierDetails> tiers)
        {
            if (tiers == null || tiers.Count == 0) return null;

            List<List<PrayerForecast.TierDetails>> groups;
            if (PrayerEditionSemantics.HasTechnologyProvider &&
                TryGroupMultiplePrayerFamilies(tiers, out groups))
            {
                List<string> familyBlocks = new List<string>();
                foreach (List<PrayerForecast.TierDetails> group in groups)
                {
                    TooltipPresentationSections family = BuildSingleFamilySections(group);
                    if (family == null || string.IsNullOrEmpty(family.SuccessBonuses)) continue;

                    string familyName = PrayerFamilyDisplayName(group[0]);
                    string compactBody = CompactVerticalSections(family.SuccessBonuses);
                    string block = string.IsNullOrEmpty(familyName)
                        ? compactBody
                        : TechnologyTooltipTextStyle.StructuralLabel(familyName) + "\n" + compactBody;
                    AddSection(familyBlocks, block);
                }

                return new TooltipPresentationSections
                {
                    // Rebalanced multi-prayer technologies can contain different prayer
                    // families. Base Faith/donations still use the ordinary dependency
                    // map; family-specific inputs (for example SG conversion) belong in
                    // the family's success/effect block instead of pretending to alter
                    // the shared base result.
                    BaseResult = PresentationText.DependencyMap(false),
                    SuccessBonuses = familyBlocks.Count == 0
                        ? null
                        : string.Join("\n\n", familyBlocks.ToArray())
                };
            }

            return BuildSingleFamilySections(tiers);
        }

        internal static string Build(List<PrayerForecast.TierDetails> tiers)
        {
            if (tiers == null || tiers.Count == 0) return null;

            List<List<PrayerForecast.TierDetails>> groups;
            if (PrayerEditionSemantics.HasTechnologyProvider &&
                TryGroupMultiplePrayerFamilies(tiers, out groups))
            {
                List<string> familyBlocks = new List<string>();
                foreach (List<PrayerForecast.TierDetails> group in groups)
                {
                    string familyBody = BuildSingleFamily(group, true);
                    if (string.IsNullOrEmpty(familyBody)) continue;

                    string familyName = PrayerFamilyDisplayName(group[0]);
                    string block = string.IsNullOrEmpty(familyName)
                        ? familyBody
                        : TechnologyTooltipTextStyle.StructuralLabel(familyName) + "\n" + familyBody;
                    AddSection(familyBlocks, block);
                }

                return familyBlocks.Count == 0 ? null : string.Join("\n\n", familyBlocks.ToArray());
            }

            return BuildSingleFamily(tiers, false);
        }

        private static TooltipPresentationSections BuildSingleFamilySections(List<PrayerForecast.TierDetails> tiers)
        {
            if (tiers == null || tiers.Count == 0) return null;

            tiers.Sort(CompareTiers);
            if (tiers.Count == 1)
                return TooltipDetailsRenderer.BuildSingleSections(tiers[0]);

            return new TooltipPresentationSections
            {
                BaseResult = PresentationText.DependencyMap(AllUseSoulGratitude(tiers)),
                SuccessBonuses = BuildSingleFamily(tiers, false)
            };
        }

        private static string BuildSingleFamily(List<PrayerForecast.TierDetails> tiers, bool compactTiers)
        {
            if (tiers == null || tiers.Count == 0) return null;

            tiers.Sort(CompareTiers);
            if (tiers.Count == 1)
                return TooltipDetailsRenderer.BuildSingle(tiers[0]);

            List<string> success = new List<string>();

            string successIntro = BuildSuccessIntro(tiers);
            string sharedEffect = BuildSharedEffect(tiers);

            if (!PrayerEditionSemantics.HasTechnologyProvider &&
                !string.IsNullOrEmpty(successIntro) &&
                !string.IsNullOrEmpty(sharedEffect))
            {
                // Vanilla keeps stock mixed success rewards. Present the shared resource
                // rider and the prayer's named effect as one success group so the effect
                // is not visually read as unconditional.
                AddSection(success, successIntro + "\n" + sharedEffect);
            }
            else
            {
                AddSection(success, successIntro);
                AddSection(success, sharedEffect);
            }

            AddSection(success, BuildSharedDuration(tiers));

            foreach (PrayerForecast.TierDetails tier in tiers)
                AddSection(success, BuildTierBlock(tier, tiers, compactTiers));

            return success.Count == 0 ? null : string.Join("\n\n", success.ToArray());
        }

        private static bool TryGroupMultiplePrayerFamilies(
            List<PrayerForecast.TierDetails> tiers,
            out List<List<PrayerForecast.TierDetails>> groups)
        {
            groups = new List<List<PrayerForecast.TierDetails>>();
            if (tiers == null || tiers.Count == 0) return false;

            Dictionary<string, List<PrayerForecast.TierDetails>> byFamily =
                new Dictionary<string, List<PrayerForecast.TierDetails>>(StringComparer.Ordinal);
            List<string> order = new List<string>();

            foreach (PrayerForecast.TierDetails tier in tiers)
            {
                if (tier == null) continue;
                string family = PrayerFamily(tier.CraftId);
                if (string.IsNullOrEmpty(family)) family = tier.CraftId ?? string.Empty;

                List<PrayerForecast.TierDetails> group;
                if (!byFamily.TryGetValue(family, out group))
                {
                    group = new List<PrayerForecast.TierDetails>();
                    byFamily.Add(family, group);
                    order.Add(family);
                }
                group.Add(tier);
            }

            if (order.Count <= 1) return false;
            foreach (string family in order)
                groups.Add(byFamily[family]);
            return true;
        }

        private static string PrayerFamilyDisplayName(PrayerForecast.TierDetails tier)
        {
            if (tier == null) return null;
            string family = PrayerFamily(tier.CraftId);
            if (string.IsNullOrEmpty(family)) return null;

            string localized = R.VanillaLocalize(family);
            return string.IsNullOrEmpty(localized) || string.Equals(localized, family, StringComparison.Ordinal)
                ? null
                : localized;
        }

        private static string CompactVerticalSections(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            string compact = text.Replace("\r\n", "\n");
            while (compact.IndexOf("\n\n", StringComparison.Ordinal) >= 0)
                compact = compact.Replace("\n\n", "\n");
            return compact;
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

            List<string> lines = new List<string>();

            AddSharedResourceLines(
                lines,
                tiers,
                R.VanillaLocalize("faith"),
                "(faith)",
                t => t.FaithBonusRate,
                t => t.FixedFaithBonus);

            AddSharedResourceLines(
                lines,
                tiers,
                Localization.F("tech.donations"),
                "(slv)",
                t => t.MoneyBonusRate,
                t => t.FixedMoneyBonus);

            return lines.Count == 0 ? null : string.Join("\n", lines.ToArray());
        }

        private static void AddSharedResourceLines(
            List<string> lines,
            List<PrayerForecast.TierDetails> tiers,
            string label,
            string icon,
            Func<PrayerForecast.TierDetails, float> rate,
            Func<PrayerForecast.TierDetails, float> fixedValue)
        {
            bool sharedRate = IsSharedNonZero(tiers, rate);
            bool sharedFixed = IsSharedNonZero(tiers, fixedValue);
            if (!sharedRate && !sharedFixed) return;

            if (sharedRate)
            {
                lines.Add(
                    icon + " " + label + ": " +
                    FormatPercent(rate(tiers[0])));
            }

            if (sharedFixed)
            {
                lines.Add(
                    sharedRate
                        ? icon + " " + FormatSignedNumber(fixedValue(tiers[0]))
                        : icon + " " + label + ": " + FormatSignedNumber(fixedValue(tiers[0])));
            }
        }

        private static string BuildSharedEffect(List<PrayerForecast.TierDetails> tiers)
        {
            string editionShared;
            if (TryGetCommonEditionTechnologyEffect(tiers, out editionShared))
                return BuildEffectSection(editionShared);

            TooltipSemanticModel.RewardDetails commonReward;
            if (TryGetCommonReward(tiers, out commonReward))
            {
                string rewardName = R.VanillaLocalize(commonReward.Id);
                List<string> lines = new List<string>
                {
                    TechnologyTooltipTextStyle.StructuralLabel(Localization.F("forecast.effect_header")) + ":",
                    TechnologyTooltipTextStyle.RewardName(
                        commonReward.Id,
                        rewardName.Replace(" ", NoBreakSpace)) + NoBreakSpace +
                    "×" + commonReward.Count.ToString(CultureInfo.InvariantCulture)
                };

                return string.Join("\n", lines.ToArray());
            }

            if (!IsSpecialShared(tiers)) return null;
            string core = tiers[0].SpecialCoreText;
            if (string.IsNullOrEmpty(core)) return null;

            string family = PrayerFamily(tiers[0].CraftId);
            if (string.Equals(family, "b_pen", StringComparison.Ordinal))
            {
                return BuildEffectSection(
                    Localization.F("tech.effect.imagination_intro") + "\n" +
                    TechnologyTooltipTextStyle.GoldValueAfterColon(core));
            }

            if (string.Equals(family, "b_star", StringComparison.Ordinal))
            {
                return BuildEffectSection(
                    Localization.F("tech.effect.excellence_intro") + "\n" +
                    TechnologyTooltipTextStyle.Atomic(core));
            }

            return BuildEffectSection(core);
        }

        private static string BuildEffectSection(string body)
        {
            if (string.IsNullOrEmpty(body)) return null;
            return TechnologyTooltipTextStyle.StructuralLabel(Localization.F("forecast.effect_header")) + ":\n" + body;
        }

        private static string BuildSharedDuration(List<PrayerForecast.TierDetails> tiers)
        {
            if (!IsDurationShared(tiers)) return null;
            return Localization.F("tech.duration") + ": " +
                   Localization.F("active.timer_days", tiers[0].SpecialDurationDays);
        }

        private static string BuildTierBlock(
            PrayerForecast.TierDetails tier,
            List<PrayerForecast.TierDetails> allTiers,
            bool compact = false)
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

            AddTierFixedAndRewardLines(lines, tier, allTiers);

            string editionShared;
            string editionTier;
            bool editionHandled = PrayerEditionSemantics.TryBuildTechnologyEffect(
                tier.CraftId,
                out editionShared,
                out editionTier);

            if (editionHandled)
            {
                if (!string.IsNullOrEmpty(editionTier))
                    lines.Add(FormatEditionTierEffect(tier, editionTier));
            }
            else if (!IsSpecialShared(allTiers))
            {
                TooltipSemanticModel.RewardDetails reward = TooltipSemanticModel.ResolveSingleReward(tier);
                if (reward == null && !string.IsNullOrEmpty(tier.SpecialCoreText))
                {
                    lines.Add(
                        TechnologyTooltipTextStyle.StructuralLabel(Localization.F("forecast.effect_header")) +
                        ": " + tier.SpecialCoreText);
                }
            }

            if (!IsDurationShared(allTiers) && tier.HasSpecialDuration)
            {
                lines.Add(
                    Localization.F("tech.duration") + ": " +
                    Localization.F("active.timer_days", tier.SpecialDurationDays));
            }

            return lines.Count == 0 ? null : string.Join(compact ? " · " : "\n", lines.ToArray());
        }

        private static string FormatEditionTierEffect(PrayerForecast.TierDetails tier, string text)
        {
            string family = PrayerFamily(tier.CraftId);
            if (string.Equals(family, "b_plant", StringComparison.Ordinal) ||
                string.Equals(family, "b_sins", StringComparison.Ordinal) ||
                string.Equals(family, "b_sword", StringComparison.Ordinal))
                return TechnologyTooltipTextStyle.Atomic(text);

            if (string.Equals(family, "b_souls", StringComparison.Ordinal))
                return TechnologyTooltipTextStyle.AtomicValueAfterColon(text);

            if (string.Equals(family, "b_sin_shard", StringComparison.Ordinal))
                return TechnologyTooltipTextStyle.AccentValueAfterColon(text);

            if (string.Equals(family, "b_star", StringComparison.Ordinal))
                return TechnologyTooltipTextStyle.QualityValueAfterColon(tier.QualityTier, text);

            return text;
        }

        private static string BuildTierRateLine(
            PrayerForecast.TierDetails tier,
            List<PrayerForecast.TierDetails> allTiers)
        {
            List<string> lines = new List<string>();

            if (Math.Abs(tier.FaithBonusRate) >= Epsilon &&
                !IsSharedNonZero(allTiers, t => t.FaithBonusRate))
            {
                lines.Add(
                    "(faith) " + R.VanillaLocalize("faith") + ": " +
                    FormatPercent(tier.FaithBonusRate));
            }

            if (Math.Abs(tier.MoneyBonusRate) >= Epsilon &&
                !IsSharedNonZero(allTiers, t => t.MoneyBonusRate))
            {
                lines.Add(
                    "(slv) " + Localization.F("tech.donations") + ": " +
                    FormatPercent(tier.MoneyBonusRate));
            }

            return lines.Count == 0 ? null : string.Join("\n", lines.ToArray());
        }

        private static void AddTierFixedAndRewardLines(
            List<string> lines,
            PrayerForecast.TierDetails tier,
            List<PrayerForecast.TierDetails> allTiers)
        {
            if (Math.Abs(tier.FixedFaithBonus) >= Epsilon &&
                !IsSharedNonZero(allTiers, t => t.FixedFaithBonus))
            {
                bool hasRate = Math.Abs(tier.FaithBonusRate) >= Epsilon ||
                               IsSharedNonZero(allTiers, t => t.FaithBonusRate);
                lines.Add(
                    hasRate
                        ? "(faith) " + FormatSignedNumber(tier.FixedFaithBonus)
                        : "(faith) " + R.VanillaLocalize("faith") + ": " +
                          FormatSignedNumber(tier.FixedFaithBonus));
            }

            if (Math.Abs(tier.FixedMoneyBonus) >= Epsilon &&
                !IsSharedNonZero(allTiers, t => t.FixedMoneyBonus))
            {
                bool hasRate = Math.Abs(tier.MoneyBonusRate) >= Epsilon ||
                               IsSharedNonZero(allTiers, t => t.MoneyBonusRate);
                lines.Add(
                    !hasRate && tier.Highlight == PrayerForecast.BonusHighlight.Money
                        ? R.FormatSignedMoney(tier.FixedMoneyBonus)
                        : hasRate
                            ? "(slv) " + FormatSignedNumber(tier.FixedMoneyBonus)
                            : "(slv) " + Localization.F("tech.donations") + ": " +
                              FormatSignedNumber(tier.FixedMoneyBonus));
            }

            TooltipSemanticModel.RewardDetails commonReward;
            bool rewardIsShared = TryGetCommonReward(allTiers, out commonReward);
            TooltipSemanticModel.RewardDetails reward = TooltipSemanticModel.ResolveSingleReward(tier);
            if (!rewardIsShared && reward != null)
            {
                string rewardName = TechnologyTooltipTextStyle.RewardName(
                    reward.Id,
                    R.VanillaLocalize(reward.Id).Replace(" ", NoBreakSpace));
                lines.Add(
                    rewardName + NoBreakSpace + "×" +
                    reward.Count.ToString(CultureInfo.InvariantCulture));
            }
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

        private static bool TryGetCommonEditionTechnologyEffect(
            List<PrayerForecast.TierDetails> tiers,
            out string sharedText)
        {
            sharedText = null;
            if (tiers == null || tiers.Count == 0) return false;

            string first = null;
            for (int i = 0; i < tiers.Count; i++)
            {
                PrayerForecast.TierDetails tier = tiers[i];
                string shared;
                string tierText;
                if (tier == null ||
                    !PrayerEditionSemantics.TryBuildTechnologyEffect(tier.CraftId, out shared, out tierText) ||
                    string.IsNullOrEmpty(shared))
                    return false;

                if (i == 0) first = shared;
                else if (!string.Equals(first, shared, StringComparison.Ordinal)) return false;
            }

            sharedText = first;
            return !string.IsNullOrEmpty(sharedText);
        }

        private static bool AllUseSoulGratitude(List<PrayerForecast.TierDetails> tiers)
        {
            if (tiers == null || tiers.Count == 0) return false;
            foreach (PrayerForecast.TierDetails tier in tiers)
                if (tier == null || !tier.UsesSoulGratitude) return false;
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

        private static string PrayerFamily(string craftId)
        {
            if (string.IsNullOrEmpty(craftId) || !craftId.StartsWith("pray:", StringComparison.Ordinal))
                return string.Empty;

            int last = craftId.LastIndexOf(':');
            if (last <= 5) return string.Empty;
            return craftId.Substring(5, last - 5);
        }

        private static string FormatPercent(float rate)
        {
            float percent = rate * 100f;
            if (Math.Abs(percent) < Epsilon) return "0%";
            string sign = percent > 0f ? "+" : "-";
            return sign + Math.Abs(percent).ToString("0.##", CultureInfo.InvariantCulture) + "%";
        }

        private static string FormatSignedNumber(float value)
        {
            if (Math.Abs(value) < Epsilon) return "0";
            string sign = value > 0f ? "+" : "-";
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