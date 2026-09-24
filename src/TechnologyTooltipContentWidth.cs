using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx.Logging;
using UnityEngine;

namespace PrayerClarity
{
    internal static class TechnologyTooltipContentWidth
    {
        internal const int OwnedMaxWidth = 900;
        private const int MinimumAnchorWidth = 150;
        private const int WideLayoutMaxWidth = 520;
        private const string NoBreakSpace = "\u00A0";
        private const int SoftProseWordThreshold = 5;

        private sealed class WideLayoutMarker { }

        private static readonly ConditionalWeakTable<object, WideLayoutMarker> WideLayoutData =
            new ConditionalWeakTable<object, WideLayoutMarker>();

        private static ManualLogSource _log;
        private static bool _errorLogged;

        internal static void PreferWideLayout(object data)
        {
            if (data == null) return;
            WideLayoutData.Remove(data);
            WideLayoutData.Add(data, new WideLayoutMarker());
        }

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

                WideLayoutMarker marker;
                if (WideLayoutData.TryGetValue(__0, out marker))
                {
                    // This is the same native NGUI seam runtime-verified in the
                    // 1.0.13 width work: ResizeFreely + overflowWidth lets content
                    // expand naturally, and WidgetsBubbleGUI.UpdateSize() then grows
                    // the enclosing bubble/parchment from the child widget size.
                    //
                    // The 0.2.19 experiment mistakenly treated a larger overflowWidth
                    // as a minimum width. For the combined BSS tooltip we instead use a
                    // real finite expansion ceiling and let its retained long prose
                    // drive the natural width up to that ceiling.
                    R.Set(label, "text", fullText);
                    R.Set(label, "overflowWidth", Math.Min(maxWidth, WideLayoutMaxWidth));
                    R.Get(label, "processedText");
                    return;
                }

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

            // A fully atomic mechanics row replaces every ordinary space with NBSP.
            // A prose sentence may still contain one smaller atomic cluster (for
            // example ↑ + white skull + red skull); normal spaces around that cluster
            // remain valid wrapping opportunities and the sentence must stay soft.
            bool hasNoBreakSpace = rawLine.IndexOf(NoBreakSpace) >= 0;
            bool hasOrdinarySpace = rawLine.IndexOf(' ') >= 0;
            if (hasNoBreakSpace && !hasOrdinarySpace) return false;

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
