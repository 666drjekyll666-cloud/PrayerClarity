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

        internal static TooltipPresentationSections BuildSections(List<PrayerForecast.TierDetails> tiers)
        {
            if (tiers == null || tiers.Count == 0) return null;

            tiers.Sort(CompareTiers);
            if (tiers.Count == 1)
                return TooltipDetailsRenderer.BuildSingleSections(tiers[0]);

            List<string> successSections = new List<string>();
            AddSection(successSections, BuildExplicitThresholds(tiers));
            AddSection(successSections, BuildExplicitResourceBlock(
                tiers,
                R.VanillaLocalize("faith"),
                "(faith)",
                t => t.FaithBonusRate,
                t => t.FixedFaithBonus));
            AddSection(successSections, BuildExplicitResourceBlock(
                tiers,
                Localization.F("tech.donations"),
                "(slv)",
                t => t.MoneyBonusRate,
                t => t.FixedMoneyBonus));
            AddSection(successSections, BuildExplicitEffect(tiers));
            AddSection(successSections, BuildExplicitDuration(tiers));

            return new TooltipPresentationSections
            {
                BaseResult = PresentationText.DependencyMap(AllUseSoulGratitude(tiers)),
                SuccessBonuses = successSections.Count == 0
                    ? null
                    : string.Join("\n\n", successSections.ToArray())
            };
        }

        private static string BuildExplicitThresholds(List<PrayerForecast.TierDetails> tiers)
        {
            List<string> lines = new List<string>();
            foreach (PrayerForecast.TierDetails tier in tiers)
            {
                if (tier == null || tier.Requirement <= 0) continue;
                string quality = QualityLabel(tier.QualityTier);
                string threshold = Localization.F("tech.success_threshold", tier.Requirement);
                lines.Add(string.IsNullOrEmpty(quality)
                    ? threshold
                    : quality + NoBreakSpace + threshold);
            }

            return lines.Count == 0 ? null : string.Join("\n", lines.ToArray());
        }

        private static string BuildExplicitResourceBlock(
            List<PrayerForecast.TierDetails> tiers,
            string label,
            string icon,
            Func<PrayerForecast.TierDetails, float> rate,
            Func<PrayerForecast.TierDetails, float> fixedValue)
        {
            bool anyRate = AnyNonZero(tiers, rate);
            bool anyFixed = AnyNonZero(tiers, fixedValue);
            if (!anyRate && !anyFixed) return null;

            List<string> lines = new List<string> { label + ":" };

            if (anyRate)
            {
                if (AllEqual(tiers, rate))
                {
                    lines.Add(
                        icon + NoBreakSpace +
                        Localization.F("tech.percent_of_base", FormatPercent(rate(tiers[0]))));
                }
                else
                {
                    foreach (PrayerForecast.TierDetails tier in tiers)
                    {
                        lines.Add(
                            QualityLabel(tier.QualityTier) + NoBreakSpace +
                            icon + NoBreakSpace +
                            Localization.F("tech.percent_of_base", FormatPercent(rate(tier))));
                    }
                }
            }

            if (anyFixed)
            {
                if (AllEqual(tiers, fixedValue))
                {
                    lines.Add(icon + NoBreakSpace + FormatSignedNumber(fixedValue(tiers[0])));
                }
                else
                {
                    foreach (PrayerForecast.TierDetails tier in tiers)
                    {
                        lines.Add(
                            QualityLabel(tier.QualityTier) + NoBreakSpace +
                            icon + NoBreakSpace +
                            FormatSignedNumber(fixedValue(tier)));
                    }
                }
            }

            return string.Join("\n", lines.ToArray());
        }

        private static string BuildExplicitEffect(List<PrayerForecast.TierDetails> tiers)
        {
            List<string> bodies = new List<string>();

            string edition = BuildEditionEffectBody(tiers);
            if (!string.IsNullOrEmpty(edition))
                bodies.Add(edition);
            else
            {
                string vanilla = BuildVanillaSpecialBody(tiers);
                if (!string.IsNullOrEmpty(vanilla)) bodies.Add(vanilla);
            }

            string reward = BuildRewardBody(tiers);
            if (!string.IsNullOrEmpty(reward)) bodies.Add(reward);

            if (bodies.Count == 0) return null;
            return TechnologyTooltipTextStyle.StructuralLabel(Localization.F("forecast.effect_header")) +
                   ":\n" + string.Join("\n\n", bodies.ToArray());
        }

        private static string BuildEditionEffectBody(List<PrayerForecast.TierDetails> tiers)
        {
            List<string> shared = new List<string>();
            List<string> tierTexts = new List<string>();

            foreach (PrayerForecast.TierDetails tier in tiers)
            {
                string sharedText;
                string tierText;
                if (tier == null ||
                    !PrayerEditionSemantics.TryBuildTechnologyEffect(tier.CraftId, out sharedText, out tierText))
                    return null;

                shared.Add(sharedText ?? string.Empty);
                tierTexts.Add(tierText ?? string.Empty);
            }

            bool sharedSame = true;
            for (int i = 1; i < shared.Count; i++)
            {
                if (!string.Equals(shared[0], shared[i], StringComparison.Ordinal))
                {
                    sharedSame = false;
                    break;
                }
            }

            List<string> lines = new List<string>();
            if (sharedSame && !string.IsNullOrEmpty(shared[0]))
                lines.Add(shared[0]);

            bool anyTierText = false;
            foreach (string value in tierTexts)
                if (!string.IsNullOrEmpty(value)) { anyTierText = true; break; }

            if (anyTierText)
            {
                bool tierSame = true;
                for (int i = 1; i < tierTexts.Count; i++)
                {
                    if (!string.Equals(tierTexts[0], tierTexts[i], StringComparison.Ordinal))
                    {
                        tierSame = false;
                        break;
                    }
                }

                if (tierSame && !string.IsNullOrEmpty(tierTexts[0]))
                {
                    lines.Add(tierTexts[0]);
                }
                else
                {
                    for (int i = 0; i < tiers.Count; i++)
                    {
                        if (string.IsNullOrEmpty(tierTexts[i])) continue;
                        lines.Add(
                            QualityLabel(tiers[i].QualityTier) + NoBreakSpace +
                            FormatEditionTierEffect(tiers[i], tierTexts[i]));
                    }
                }
            }

            if (!sharedSame)
            {
                for (int i = 0; i < tiers.Count; i++)
                {
                    string combined = shared[i];
                    if (!string.IsNullOrEmpty(tierTexts[i]))
                        combined = string.IsNullOrEmpty(combined)
                            ? tierTexts[i]
                            : combined + " " + tierTexts[i];
                    if (string.IsNullOrEmpty(combined)) continue;
                    lines.Add(QualityLabel(tiers[i].QualityTier) + NoBreakSpace + combined);
                }
            }

            return lines.Count == 0 ? null : string.Join("\n", lines.ToArray());
        }

        private static string BuildVanillaSpecialBody(List<PrayerForecast.TierDetails> tiers)
        {
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

            if (IsSpecialShared(tiers))
                return tiers[0].SpecialCoreText;

            List<string> lines = new List<string>();
            foreach (PrayerForecast.TierDetails tier in tiers)
            {
                if (tier == null || string.IsNullOrEmpty(tier.SpecialCoreText)) continue;
                lines.Add(
                    QualityLabel(tier.QualityTier) + NoBreakSpace +
                    tier.SpecialCoreText);
            }

            return lines.Count == 0 ? null : string.Join("\n", lines.ToArray());
        }

        private static string BuildRewardBody(List<PrayerForecast.TierDetails> tiers)
        {
            List<TooltipSemanticModel.RewardDetails> rewards = new List<TooltipSemanticModel.RewardDetails>();
            string rewardId = null;
            bool any = false;

            foreach (PrayerForecast.TierDetails tier in tiers)
            {
                TooltipSemanticModel.RewardDetails reward = TooltipSemanticModel.ResolveSingleReward(tier);
                rewards.Add(reward);
                if (reward == null) continue;
                any = true;
                if (rewardId == null) rewardId = reward.Id;
                else if (!string.Equals(rewardId, reward.Id, StringComparison.Ordinal)) return null;
            }

            if (!any || string.IsNullOrEmpty(rewardId)) return null;

            List<string> lines = new List<string>
            {
                TechnologyTooltipTextStyle.RewardName(rewardId, R.VanillaLocalize(rewardId))
            };

            bool allPresent = true;
            int? commonCount = null;
            bool sameCount = true;
            foreach (TooltipSemanticModel.RewardDetails reward in rewards)
            {
                if (reward == null)
                {
                    allPresent = false;
                    sameCount = false;
                    continue;
                }

                if (!commonCount.HasValue) commonCount = reward.Count;
                else if (commonCount.Value != reward.Count) sameCount = false;
            }

            if (allPresent && sameCount && commonCount.HasValue)
            {
                lines.Add(
                    Localization.F("tech.quantity") + ": ×" +
                    commonCount.Value.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                lines.Add(Localization.F("tech.quantity") + ":");
                for (int i = 0; i < tiers.Count; i++)
                {
                    string count = rewards[i] == null
                        ? "—"
                        : "×" + rewards[i].Count.ToString(CultureInfo.InvariantCulture);
                    lines.Add(
                        QualityLabel(tiers[i].QualityTier) + NoBreakSpace +
                        count);
                }
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

        private static string BuildExplicitDuration(List<PrayerForecast.TierDetails> tiers)
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
            bool shared = tiers[0].HasSpecialDuration;
            float first = tiers[0].SpecialDurationDays;
            for (int i = 1; i < tiers.Count && shared; i++)
            {
                if (!tiers[i].HasSpecialDuration ||
                    Math.Abs(tiers[i].SpecialDurationDays - first) >= Epsilon)
                    shared = false;
            }

            if (shared)
                return header + ": " + Localization.F("active.timer_days", first);

            List<string> lines = new List<string> { header + ":" };
            foreach (PrayerForecast.TierDetails tier in tiers)
            {
                string value = tier.HasSpecialDuration
                    ? Localization.F("active.timer_days", tier.SpecialDurationDays)
                    : "—";
                lines.Add(
                    QualityLabel(tier.QualityTier) + NoBreakSpace +
                    value.Replace(" ", NoBreakSpace));
            }

            return string.Join("\n", lines.ToArray());
        }

        private static bool AnyNonZero(
            List<PrayerForecast.TierDetails> tiers,
            Func<PrayerForecast.TierDetails, float> selector)
        {
            foreach (PrayerForecast.TierDetails tier in tiers)
                if (tier != null && Math.Abs(selector(tier)) >= Epsilon) return true;
            return false;
        }

        private static bool AllEqual(
            List<PrayerForecast.TierDetails> tiers,
            Func<PrayerForecast.TierDetails, float> selector)
        {
            if (tiers == null || tiers.Count == 0) return true;
            float first = selector(tiers[0]);
            for (int i = 1; i < tiers.Count; i++)
                if (Math.Abs(selector(tiers[i]) - first) >= Epsilon) return false;
            return true;
        }

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
            string editionShared;
            if (TryGetCommonEditionTechnologyEffect(tiers, out editionShared))
                return BuildEffectSection(editionShared);

            if (AllUseSoulGratitude(tiers))
                return Localization.F("tech.souls_base_faith");

            TooltipSemanticModel.RewardDetails commonReward;
            if (TryGetCommonReward(tiers, out commonReward))
            {
                string rewardName = R.VanillaLocalize(commonReward.Id);
                List<string> lines = new List<string>
                {
                    TechnologyTooltipTextStyle.StructuralLabel(Localization.F("forecast.effect_header")) + ":",
                    TechnologyTooltipTextStyle.RewardName(commonReward.Id, rewardName)
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

            return lines.Count == 0 ? null : string.Join("\n", lines.ToArray());
        }

        private static string FormatEditionTierEffect(PrayerForecast.TierDetails tier, string text)
        {
            string family = PrayerFamily(tier.CraftId);
            if (string.Equals(family, "b_plant", StringComparison.Ordinal) ||
                string.Equals(family, "b_sins", StringComparison.Ordinal) ||
                string.Equals(family, "b_sword", StringComparison.Ordinal))
                return TechnologyTooltipTextStyle.Atomic(text);

            if (string.Equals(family, "b_star", StringComparison.Ordinal))
                return TechnologyTooltipTextStyle.QualityValueAfterColon(tier.QualityTier, text);

            return text;
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

        private static void AddTierFixedAndRewardLines(
            List<string> lines,
            PrayerForecast.TierDetails tier,
            List<PrayerForecast.TierDetails> allTiers)
        {
            List<string> fixedParts = new List<string>();

            if (Math.Abs(tier.FixedFaithBonus) >= Epsilon &&
                !IsSharedNonZero(allTiers, t => t.FixedFaithBonus))
                fixedParts.Add(FormatSignedNumber(tier.FixedFaithBonus) + " (faith)");

            if (Math.Abs(tier.FixedMoneyBonus) >= Epsilon &&
                !IsSharedNonZero(allTiers, t => t.FixedMoneyBonus))
                fixedParts.Add(FormatSignedNumber(tier.FixedMoneyBonus) + " (slv)");

            if (fixedParts.Count > 0)
                lines.Add(string.Join(InlineSeparator, fixedParts.ToArray()));

            TooltipSemanticModel.RewardDetails commonReward;
            bool rewardIsShared = TryGetCommonReward(allTiers, out commonReward);
            TooltipSemanticModel.RewardDetails reward = TooltipSemanticModel.ResolveSingleReward(tier);
            if (!rewardIsShared && reward != null)
            {
                string rewardName = TechnologyTooltipTextStyle.RewardName(
                    reward.Id,
                    R.VanillaLocalize(reward.Id));
                lines.Add(TechnologyTooltipTextStyle.Atomic(
                    rewardName + " ×" + reward.Count.ToString(CultureInfo.InvariantCulture)));
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