using System;
using System.Collections;
using System.Collections.Generic;

namespace PrayerClarity
{
    internal static class TooltipTextPolish
    {
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
