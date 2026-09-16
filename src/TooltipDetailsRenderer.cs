using System;
using System.Collections.Generic;
using System.Globalization;

namespace PrayerClarity
{
    internal static class TooltipDetailsRenderer
    {
        private const float Epsilon = 0.0001f;

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
                values.Add(TierPrefix(tier, true) +
                           tier.Requirement.ToString(CultureInfo.InvariantCulture) +
                           " (cross)");
            }

            return Localization.F("tech.requires_header") + ":\n" +
                   string.Join("   ", values.ToArray());
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

            List<string> lines = new List<string>
            {
                Localization.F("tech.success_bonus") + ":"
            };

            if (anyFaith)
                lines.Add(BuildResourceBlock(
                    tiers,
                    "(faith)",
                    t => t.FaithBonusRate,
                    t => t.FixedFaithBonus,
                    comparative));

            if (anyMoney)
                lines.Add(BuildResourceBlock(
                    tiers,
                    "(slv) " + Localization.F("tech.donations"),
                    t => t.MoneyBonusRate,
                    t => t.FixedMoneyBonus,
                    comparative));

            return string.Join("\n", lines.ToArray());
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

        private static string BuildResourceBlock(
            List<PrayerForecast.TierDetails> tiers,
            string label,
            Func<PrayerForecast.TierDetails, float> rate,
            Func<PrayerForecast.TierDetails, float> fixedValue,
            bool comparative)
        {
            if (!comparative || tiers.Count == 1)
            {
                PrayerForecast.TierDetails tier = tiers[0];
                string value = FormatCombined(rate(tier), fixedValue(tier), false);
                return label + (string.IsNullOrEmpty(value) ? string.Empty : " " + value);
            }

            bool rateSame = AllEqual(tiers, rate);
            bool fixedSame = AllEqual(tiers, fixedValue);

            List<string> common = new List<string>();
            float firstRate = rate(tiers[0]);
            float firstFixed = fixedValue(tiers[0]);

            if (rateSame && Math.Abs(firstRate) >= Epsilon)
                common.Add(FormatPercent(firstRate, false));
            if (fixedSame && Math.Abs(firstFixed) >= Epsilon)
                common.Add(FormatSignedNumber(firstFixed, false));

            List<string> lines = new List<string>();
            lines.Add(label + (common.Count == 0 ? string.Empty : " " + string.Join(" ", common.ToArray())));

            if (!rateSame || !fixedSame)
            {
                bool horizontal = (rateSame ? 0 : 1) + (fixedSame ? 0 : 1) == 1;
                List<string> values = new List<string>();

                foreach (PrayerForecast.TierDetails tier in tiers)
                {
                    List<string> parts = new List<string>();
                    if (!rateSame) parts.Add(FormatPercent(rate(tier), true));
                    if (!fixedSame) parts.Add(FormatSignedNumber(fixedValue(tier), true));
                    values.Add(TierPrefix(tier, true) + string.Join(" ", parts.ToArray()));
                }

                lines.Add(horizontal
                    ? string.Join("   ", values.ToArray())
                    : string.Join("\n", values.ToArray()));
            }

            return string.Join("\n", lines.ToArray());
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
                values.Add(TierPrefix(tier, true) + value);
            }

            return header + ":\n" + string.Join("   ", values.ToArray());
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
