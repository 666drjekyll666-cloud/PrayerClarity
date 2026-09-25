using System;
using System.Collections.Generic;
using System.Globalization;

namespace PrayerClarity
{
    // Shared player-facing formula wording for pulpit and Technology. The semantic
    // calculator keeps exact resolved values separately; this class only renders the
    // accepted reveal model and prayer-owned contribution.
    internal static class PresentationText
    {
        internal static string BuildPulpitResultRows(PrayerForecast.Result forecast)
        {
            if (forecast == null) return string.Empty;

            string dependencies = IndentMultiline(DependencyMap(forecast.UsesSoulGratitude), "    ");
            string contribution = string.Empty;
            bool hasContribution = false;

            if (forecast.SoulGratitudeFaithCap > 0)
            {
                string conversion = forecast.SoulGratitudeConversion > 0
                    ? Localization.F("rebalanced.pulpit.souls_conversion",
                        forecast.SoulGratitudeConversion, forecast.SoulGratitudeConversion)
                    : Localization.F("rebalanced.pulpit.souls_conversion_empty");
                contribution = "\n    " + conversion;
                hasContribution = true;
            }
            else if (Math.Abs(forecast.FaithBonusRate) >= 0.0001f ||
                     forecast.FixedFaithBonus != 0 ||
                     Math.Abs(forecast.MoneyBonusRate) >= 0.0001f ||
                     Math.Abs(forecast.FixedMoneyBonus) >= 0.0001f)
            {
                contribution = FormatPrayerContribution(
                    forecast.FaithBonusRate, forecast.FixedFaithBonus,
                    forecast.MoneyBonusRate, forecast.FixedMoneyBonus);
                hasContribution = true;
            }

            string onSuccess = "  " + Localization.F("tech.on_success_header");
            if (hasContribution) onSuccess += contribution;

            return "  " + Localization.F("forecast.guaranteed") + ":\n" +
                   dependencies + "\n" +
                   "  " + Localization.F("forecast.current_success_chance", forecast.ChancePercent) + "\n" +
                   onSuccess;
        }

        internal static string DependencyMap(bool usesSoulGratitude)
        {
            return Localization.F(usesSoulGratitude
                ? "forecast.dependency_note_souls"
                : "forecast.dependency_note");
        }

        internal static string FormatPrayerContribution(PrayerForecast.TierDetails tier)
        {
            if (tier == null) return "—";
            return FormatPrayerContribution(
                tier.FaithBonusRate,
                tier.FixedFaithBonus,
                tier.MoneyBonusRate,
                tier.FixedMoneyBonus);
        }

        internal static string FormatPrayerContribution(
            float faithRate,
            int fixedFaith,
            float moneyRate,
            float fixedMoney)
        {
            List<string> parts = new List<string>();

            string faith = FormatResourceContribution("(faith)", faithRate, fixedFaith);
            if (!string.IsNullOrEmpty(faith)) parts.Add(faith);

            string money = FormatResourceContribution("(slv)", moneyRate, fixedMoney);
            if (!string.IsNullOrEmpty(money)) parts.Add(money);

            // Success contribution is deliberately a dedicated indented row on every
            // surface. This keeps the resource icons with their percentage/flat values
            // and prevents narrow Technology tooltips from wrapping immediately after
            // the icon. A spaced vertical bar separates the two independent resources.
            return parts.Count == 0
                ? "—"
                : "\n    " + string.Join(" | ", parts.ToArray());
        }

        private static string FormatResourceContribution(string icon, float rate, float fixedValue)
        {
            if (Math.Abs(rate) < 0.0001f && Math.Abs(fixedValue) < 0.0001f) return null;

            string value = icon;
            if (Math.Abs(rate) >= 0.0001f)
                value += " " + FormatPercent(rate);
            if (Math.Abs(fixedValue) >= 0.0001f)
                value += " " + FormatSignedNumber(fixedValue);
            return value;
        }

        private static string FormatPercent(float rate)
        {
            float percent = rate * 100f;
            string sign = percent > 0.0001f ? "+" : percent < -0.0001f ? "−" : string.Empty;
            return sign + Math.Abs(percent).ToString("0.##", CultureInfo.InvariantCulture) + "%";
        }

        private static string FormatSignedNumber(float value)
        {
            if (Math.Abs(value) < 0.0001f) return "0";
            string sign = value > 0f ? "+" : "−";
            return sign + Math.Abs(value).ToString("0.##", CultureInfo.InvariantCulture);
        }

        private static string IndentMultiline(string text, string indent)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            return indent + text.Replace("\r\n", "\n").Replace("\n", "\n" + indent);
        }
    }
}
