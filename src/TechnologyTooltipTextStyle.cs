using System;
using System.Text;

namespace PrayerClarity
{
    internal static class TechnologyTooltipTextStyle
    {
        private const string NoBreakSpace = "\u00A0";

        // One warm accent is deliberately reused for structural labels, named rewards
        // and the few values that deserve immediate attention. Prayer quality already
        // has native Bronze/Silver/Gold star glyphs, so text color should not duplicate
        // that tier identity with a second, weaker visual code.
        private const string KeyAccent = "FF7A70";

        internal static string StructuralLabel(string text)
        {
            return Color(text, KeyAccent);
        }

        internal static string RewardName(string itemId, string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            if (string.Equals(itemId, "blessing_commerce", StringComparison.Ordinal))
                return Color(text, KeyAccent);

            if (itemId != null && itemId.StartsWith("story:", StringComparison.Ordinal))
                return Color(text, KeyAccent);

            return text;
        }

        internal static string QualityValue(int qualityTier, string text)
        {
            return qualityTier >= 1 && qualityTier <= 3 ? Color(text, KeyAccent) : text;
        }

        internal static string QualityValueAfterColon(int qualityTier, string text)
        {
            return qualityTier >= 1 && qualityTier <= 3 ? KeyValueAfterColon(text) : text;
        }

        internal static string GoldValueAfterColon(string text)
        {
            return KeyValueAfterColon(text);
        }

        internal static string KeyValueAfterColon(string text)
        {
            return ValueAfterColon(text, KeyAccent);
        }

        internal static string AtomicValueAfterColon(string text)
        {
            return WrapFriendlyValueAfterColon(text, null);
        }

        internal static string AccentValueAfterColon(string text)
        {
            return WrapFriendlyValueAfterColon(text, KeyAccent);
        }

        internal static string CorpseQualityCue()
        {
            // The cluster is one semantic symbol. NGUI may move the whole group to the
            // next visual line, but must never split arrow / white skull / red skull.
            return "(up)" + NoBreakSpace + "(skull)" + NoBreakSpace + "(rskull)";
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

        internal static string AccentLoreEntity(string text, string itemId, string localizedName)
        {
            if (string.IsNullOrEmpty(text)) return text;

            // Vanilla lore can use a grammatical form that differs from the inventory
            // item name (Russian is one example), so first try the exact localized item
            // name and then fall back to the quoted entity phrase in this known lore.
            string normalized = text;
            string[,] quotePairs =
            {
                { "«", "»" },
                { "‹", "›" },
                { "“", "”" },
                { "„", "“" },
                { "\"", "\"" },
                { "'", "'" },
                { "「", "」" },
                { "『", "』" },
                { "《", "》" },
                { "〈", "〉" },
                { "<", ">" }
            };

            if (!string.IsNullOrEmpty(localizedName))
            {
                for (int i = 0; i < quotePairs.GetLength(0); i++)
                {
                    string quoted = quotePairs[i, 0] + localizedName + quotePairs[i, 1];
                    normalized = normalized.Replace(quoted, localizedName);
                }

                string exact = AccentEntityOccurrences(normalized, itemId, localizedName);
                if (!string.Equals(exact, normalized, StringComparison.Ordinal)) return exact;
            }

            for (int i = 0; i < quotePairs.GetLength(0); i++)
            {
                string open = quotePairs[i, 0];
                string close = quotePairs[i, 1];
                int openAt = normalized.IndexOf(open, StringComparison.Ordinal);
                if (openAt < 0) continue;
                int contentAt = openAt + open.Length;
                int closeAt = normalized.IndexOf(close, contentAt, StringComparison.Ordinal);
                if (closeAt <= contentAt) continue;

                string entity = normalized.Substring(contentAt, closeAt - contentAt);
                return normalized.Substring(0, openAt) +
                       RewardName(itemId, entity) +
                       normalized.Substring(closeAt + close.Length);
            }

            return normalized;
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

        private static string ValueAfterColon(string text, string color)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(color)) return text;
            int colon = Math.Max(text.LastIndexOf(':'), text.LastIndexOf('：'));
            if (colon < 0 || colon + 1 >= text.Length) return text;

            string label = text.Substring(0, colon + 1).TrimEnd();
            string value = text.Substring(colon + 1).Trim();
            if (value.Length == 0) return text;

            return Atomic(label + " " + Color(value, color));
        }

        private static string WrapFriendlyValueAfterColon(string text, string color)
        {
            if (string.IsNullOrEmpty(text)) return text;
            int colon = Math.Max(text.LastIndexOf(':'), text.LastIndexOf('：'));
            if (colon < 0 || colon + 1 >= text.Length) return text;

            string label = text.Substring(0, colon + 1).TrimEnd();
            string value = text.Substring(colon + 1).Trim();
            if (value.Length == 0) return text;

            string atomicValue = Atomic(value);
            if (!string.IsNullOrEmpty(color))
                atomicValue = Color(atomicValue, color);

            // Keep localized prose wrappable while treating the decision value itself
            // (for example "30 [Soul Gratitude]" or "×4") as one visual unit.
            return label + " " + atomicValue;
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
