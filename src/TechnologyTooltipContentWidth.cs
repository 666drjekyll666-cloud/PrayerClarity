using System;
using System.Reflection;
using BepInEx.Logging;
using UnityEngine;

namespace PrayerClarity
{
    internal static class TechnologyTooltipContentWidth
    {
        // This value is both the finite measurement ceiling and the ownership marker
        // for PrayerClarity's Technology mechanics body. Vanilla 1.407 tooltip rows
        // observed by the runtime probes use max_width = -1.
        internal const int OwnedMaxWidth = 900;

        private const float MeasurementPadding = 2f;
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

                int originalWidth = R.Int(R.Get(label, "width"));
                if (originalWidth < 1) originalWidth = 1;

                // BubbleWidgetText.Draw in Graveyard Keeper 1.407 never reads
                // BubbleWidgetTextData.max_width. Give NGUI a wide temporary canvas,
                // let UILabel measure the already-localized/encoded text, then shrink
                // to the natural longest-line width. WidgetsBubbleGUI.UpdateSize()
                // will subsequently size the bubble from this final label geometry.
                R.Set(label, "width", maxWidth);
                Vector2 expandedSize;
                if (!TryReadPrintedSize(label, out expandedSize))
                {
                    R.Set(label, "width", originalWidth);
                    return;
                }

                int desiredWidth = PixelAligned(expandedSize.x + MeasurementPadding);
                desiredWidth = Math.Max(originalWidth, Math.Min(maxWidth, desiredWidth));
                R.Set(label, "width", desiredWidth);

                Vector2 finalSize;
                if (TryReadPrintedSize(label, out finalSize))
                {
                    int desiredHeight = PixelAligned(finalSize.y);
                    if (desiredHeight > 0)
                        R.Set(label, "height", desiredHeight);
                }
            }
            catch (Exception ex)
            {
                if (_errorLogged) return;
                _errorLogged = true;
                _log?.LogError(
                    "PrayerClarity Technology tooltip content-width sizing failed; " +
                    "the tooltip remains available at vanilla/prefab width. " + ex);
            }
        }

        private static bool TryReadPrintedSize(object label, out Vector2 size)
        {
            size = Vector2.zero;
            object value = R.Get(label, "printedSize");
            if (!(value is Vector2)) return false;
            size = (Vector2)value;
            return size.x > 0f && size.y > 0f;
        }

        private static int PixelAligned(float value)
        {
            int pixels = Mathf.CeilToInt(value);
            if ((pixels & 1) != 0) pixels++;
            return Math.Max(2, pixels);
        }
    }
}
