using System;
using System.Reflection;
using BepInEx.Logging;
using UnityEngine;

namespace PrayerClarity
{
    internal static class TechnologyTooltipContentWidth
    {
        // Ownership marker for PrayerClarity's Technology mechanics body. This stays
        // deliberately wider than any expected final body so the live UILabel can be
        // measured before we replace its native overflow ceiling with the atomic-row
        // anchor derived below.
        internal const int OwnedMaxWidth = 900;
        private const int MinimumAnchorWidth = 150;

        private static ManualLogSource _log;
        private static bool _errorLogged;

        internal static void Install(string harmonyId, ManualLogSource log)
        {
            _log = log;

            Type bubbleWidgetText = R.GameType("BubbleWidgetText");
            Type bubbleWidgetTextData = R.GameType("BubbleWidgetTextData");
            MethodInfo draw = R.Method(
                bubbleWidgetText,
                "Draw",
                false,
                new[] { bubbleWidgetTextData });

            if (draw == null)
                throw new MissingMethodException("BubbleWidgetText.Draw(BubbleWidgetTextData)");

            R.Patch(
                harmonyId + ".technologycontentwidth",
                typeof(TechnologyTooltipContentWidth),
                draw,
                nameof(BubbleWidgetTextDrawPostfix));
        }

        private static void BubbleWidgetTextDrawPostfix(object __instance, object __0)
        {
            try
            {
                if (__instance == null || __0 == null) return;

                int maxWidth = R.Int(R.Get(__0, "max_width"));
                if (maxWidth != OwnedMaxWidth) return;

                object label = R.Get(__instance, "_label") ?? R.Get(__instance, "ui_widget");
                if (label == null) return;

                string fullText = R.Get(label, "text") as string;
                if (string.IsNullOrEmpty(fullText)) return;

                int anchorWidth = MeasureAtomicAnchor(label, fullText, maxWidth);
                if (anchorWidth <= 0) anchorWidth = maxWidth;

                R.Set(label, "text", fullText);
                R.Set(label, "overflowWidth", anchorWidth);
                // Force NGUI to rebuild processedText immediately so the enclosing
                // WidgetsBubbleGUI reads the final wrapped dimensions in the same draw.
                R.Get(label, "processedText");
            }
            catch (Exception ex)
            {
                if (_errorLogged) return;
                _errorLogged = true;
                _log?.LogError(
                    "PrayerClarity Technology tooltip anchor-width sizing failed; " +
                    "the tooltip remains available at the wide fallback ceiling. " + ex);
            }
        }

        private static int MeasureAtomicAnchor(object label, string fullText, int measurementCeiling)
        {
            string effectHeader = Localization.F("forecast.effect_header") + ":";
            string[] lines = fullText.Replace("\r\n", "\n").Split('\n');

            float widest = 0f;
            bool previousWasEffectHeader = false;

            R.Set(label, "overflowWidth", measurementCeiling);

            foreach (string raw in lines)
            {
                string line = raw == null ? string.Empty : raw.Trim();
                if (line.Length == 0)
                {
                    previousWasEffectHeader = false;
                    continue;
                }

                bool isEffectHeader = string.Equals(line, effectHeader, StringComparison.Ordinal);
                bool soft = previousWasEffectHeader ||
                            isEffectHeader ||
                            line.StartsWith(effectHeader + " ", StringComparison.Ordinal) ||
                            line.IndexOf(" ×", StringComparison.Ordinal) >= 0 ||
                            IsSectionHeading(line);

                previousWasEffectHeader = isEffectHeader;
                if (soft) continue;

                R.Set(label, "text", line);
                R.Get(label, "processedText");
                object printed = R.Get(label, "printedSize");
                if (!(printed is Vector2)) continue;

                float width = ((Vector2)printed).x;
                if (width > widest) widest = width;
            }

            if (widest <= 0f) return measurementCeiling;
            int measured = Mathf.CeilToInt(widest) + 2;
            return Mathf.Clamp(measured, MinimumAnchorWidth, measurementCeiling);
        }

        private static bool IsSectionHeading(string line)
        {
            // Headings organize the body but should not become width owners merely
            // because one localization spells the heading with a long phrase.
            return line.EndsWith(":", StringComparison.Ordinal) &&
                   line.IndexOf("100%", StringComparison.Ordinal) < 0;
        }
    }
}
