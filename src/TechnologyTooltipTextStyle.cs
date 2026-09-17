using System;
using System.Text;

namespace PrayerClarity
{
    internal static class TechnologyTooltipTextStyle
    {
        private const string NoBreakSpace = "\u00A0";
        private const string StructuralAccent = "C7654E";
        private const string BronzeAccent = "B77A45";
        private const string SilverAccent = "91B6CA";
        private const string GoldAccent = "D9B83E";

        internal static string StructuralLabel(string text)
        {
            return Color(text, StructuralAccent);
        }

        internal static string RewardName(string itemId, string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            if (string.Equals(itemId, "blessing_commerce", StringComparison.Ordinal))
                return Color(text, StructuralAccent);

            if (itemId != null && itemId.StartsWith("story:", StringComparison.Ordinal))
            {
                if (itemId.EndsWith(":2", StringComparison.Ordinal)) return Color(text, SilverAccent);
                if (itemId.EndsWith(":3", StringComparison.Ordinal)) return Color(text, GoldAccent);
            }

            return text;
        }

        internal static string QualityValue(int qualityTier, string text)
        {
            string color;
            switch (qualityTier)
            {
                case 1: color = BronzeAccent; break;
                case 2: color = SilverAccent; break;
                case 3: color = GoldAccent; break;
                default: return text;
            }
            return Color(text, color);
        }

        internal static string QualityValueAfterColon(int qualityTier, string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            int colon = Math.Max(text.LastIndexOf(':'), text.LastIndexOf('：'));
            if (colon < 0 || colon + 1 >= text.Length) return text;

            string label = text.Substring(0, colon + 1).TrimEnd();
            string value = text.Substring(colon + 1).Trim();
            if (value.Length == 0) return text;

            return Atomic(label + " " + QualityValue(qualityTier, value));
        }

        internal static string Atomic(string text)
        {
            return string.IsNullOrEmpty(text) ? text : text.Replace(" ", NoBreakSpace);
        }

        internal static string AccentEntityOccurrences(string text, string itemId, string localizedName)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(localizedName)) return text;
            return text.Replace(localizedName, RewardName(itemId, localizedName));
        }

        internal static string StripColorEncoding(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            StringBuilder result = new StringBuilder(text.Length);
            for (int i = 0; i < text.Length;)
            {
                if (text[i] == '[')
                {
                    int close = text.IndexOf(']', i + 1);
                    if (close > i)
                    {
                        string tag = text.Substring(i + 1, close - i - 1);
                        if (tag == "-" || IsHexColor(tag))
                        {
                            i = close + 1;
                            continue;
                        }
                    }
                }

                result.Append(text[i]);
                i++;
            }

            return result.ToString().Replace(NoBreakSpace, " ");
        }

        private static bool IsHexColor(string value)
        {
            if (value == null || value.Length != 6) return false;
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                bool hex = (c >= '0' && c <= '9') ||
                           (c >= 'A' && c <= 'F') ||
                           (c >= 'a' && c <= 'f');
                if (!hex) return false;
            }
            return true;
        }

        private static string Color(string text, string hex)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return "[" + hex + "]" + text + "[-]";
        }
    }
}
