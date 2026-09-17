using System;
using System.Reflection;
using BepInEx.Logging;
using UnityEngine;

namespace PrayerClarity
{
    internal static class TechnologyTooltipContentWidth
    {
        internal const int OwnedMaxWidth = 900;
        private const int MinimumAnchorWidth = 150;
        private const string NoBreakSpace = "\u00A0";
        private const int SoftProseWordThreshold = 6;

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

                string semanticLine = TechnologyTooltipTextStyle.StripColorEncoding(line);
                bool isEffectHeader = string.Equals(semanticLine, effectHeader, StringComparison.Ordinal);
                bool soft = previousWasEffectHeader ||
                            isEffectHeader ||
                            semanticLine.StartsWith(effectHeader + " ", StringComparison.Ordinal) ||
                            semanticLine.IndexOf(" ×", StringComparison.Ordinal) >= 0 ||
                            IsSectionHeading(semanticLine) ||
                            IsLongProse(line, semanticLine);

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

        private static bool IsLongProse(string rawLine, string semanticLine)
        {
            if (string.IsNullOrEmpty(semanticLine)) return false;

            // Explicitly atomic mechanics rows contain NBSPs inserted by the renderer.
            // Ordinary human-language prose remains breakable. Once a prose line is
            // long enough to be sentence-like, it must wrap inside the width chosen by
            // the compact mechanics rows instead of stretching the whole bubble.
            if (rawLine.IndexOf(NoBreakSpace) >= 0) return false;

            int words = 1;
            bool inSpace = false;
            for (int i = 0; i < semanticLine.Length; i++)
            {
                bool space = char.IsWhiteSpace(semanticLine[i]);
                if (space && !inSpace) words++;
                inSpace = space;
            }

            return words >= SoftProseWordThreshold;
        }

        private static bool IsSectionHeading(string line)
        {
            return line.EndsWith(":", StringComparison.Ordinal) &&
                   line.IndexOf("100%", StringComparison.Ordinal) < 0;
        }
    }
}
