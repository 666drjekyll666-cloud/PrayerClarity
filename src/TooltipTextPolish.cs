using System;
using System.Collections;
using System.Collections.Generic;

namespace PrayerClarity
{
    internal static class TooltipTextPolish
    {
        private const string NoBreakSpace = "\u00A0";

        internal static void NormalizePrayerTitleRow(IList list, int endExclusive, Type bubbleTextType)
        {
            if (list == null || bubbleTextType == null) return;

            int end = Math.Min(list.Count, Math.Max(0, endExclusive));
            for (int i = 0; i < end; i++)
            {
                object row = list[i];
                if (row == null || !bubbleTextType.IsInstanceOfType(row)) continue;

                string text = R.Get(row, "text") as string;
                if (string.IsNullOrWhiteSpace(text)) continue;

                // The first non-empty text row in the verified prayer Technology
                // tooltip is the native title. Keep its final lexical pair together
                // so short connectors such as "об / of / de" do not become orphans.
                string normalized = ProtectFinalWordPair(text);
                if (!string.Equals(text, normalized, StringComparison.Ordinal))
                    R.Set(row, "text", normalized);
                return;
            }
        }

        internal static void NormalizeFollowingCraftingRow(IList list, int startIndex, Type bubbleTextType)
        {
            if (list == null || bubbleTextType == null) return;

            int end = Math.Min(list.Count, startIndex + 4);
            for (int i = Math.Max(0, startIndex); i < end; i++)
            {
                object row = list[i];
                if (row == null || !bubbleTextType.IsInstanceOfType(row)) continue;

                string text = R.Get(row, "text") as string;
                string normalized = NormalizeCraftingLocationText(text);
                if (string.Equals(text, normalized, StringComparison.Ordinal)) continue;

                R.Set(row, "text", normalized);
                return;
            }
        }

        private static string ProtectFinalWordPair(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;

            string normalized = text.Replace("\r\n", "\n");
            if (normalized.IndexOf('\n') >= 0) return text;

            int lastSpace = normalized.LastIndexOf(' ');
            if (lastSpace <= 0 || lastSpace + 1 >= normalized.Length) return text;

            return normalized.Substring(0, lastSpace) +
                   NoBreakSpace +
                   normalized.Substring(lastSpace + 1);
        }

        private static string NormalizeCraftingLocationText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;

            string normalized = text.Replace("\r\n", "\n");
            int asciiColon = normalized.IndexOf(':');
            int fullColon = normalized.IndexOf('：');
            int colon;
            if (asciiColon < 0) colon = fullColon;
            else if (fullColon < 0) colon = asciiColon;
            else colon = Math.Min(asciiColon, fullColon);

            // The runtime screenshots establish that the first short heading-like row
            // following prayer mechanics is the native crafting-location row. Keep this
            // normalization narrow rather than rewriting arbitrary tooltip prose.
            if (colon <= 0 || colon > 64 || colon + 1 >= normalized.Length) return text;

            string heading = normalized.Substring(0, colon + 1).TrimEnd();
            string value = normalized.Substring(colon + 1).Replace("\n", " ").Trim();
            if (value.Length == 0) return text;

            string[] rawParts = value.Split(',');
            List<string> parts = new List<string>();
            foreach (string raw in rawParts)
            {
                string part = raw.Trim();
                if (part.Length > 0) parts.Add(part);
            }
            if (parts.Count == 0) return text;

            return heading + "\n" + string.Join(", ", parts.ToArray());
        }
    }
}
