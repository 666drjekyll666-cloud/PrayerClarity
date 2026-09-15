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
            string guaranteed = "  " + Localization.F("forecast.guaranteed") + ": " +
                                DependencyMap(forecast.UsesSoulGratitude);
            string success = "  " + Localization.F("forecast.success_bonus", forecast.ChancePercent) + ": " +
                             FormatPrayerContribution(
                                 forecast.FaithBonusRate,
                                 forecast.FixedFaithBonus,
                                 forecast.MoneyBonusRate,
                                 forecast.FixedMoneyBonus);
            return guaranteed + "\n" + success;
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

            return parts.Count == 0 ? "—" : string.Join("; ", parts.ToArray());
        }

        private static string FormatResourceContribution(string icon, float rate, float fixedValue)
        {
            if (Math.Abs(rate) < 0.0001f && Math.Abs(fixedValue) < 0.0001f) return null;

            string value = icon;
            if (Math.Abs(rate) >= 0.0001f)
                value += " " + Localization.F("format.of_base", FormatPercent(rate));
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
    }
}
