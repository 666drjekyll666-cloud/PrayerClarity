using System;
using System.Reflection;
using BepInEx.Logging;

namespace PrayerClarity
{
    internal static class TechnologyTooltipContentWidth
    {
        // Finite width ceiling and ownership marker for PrayerClarity's Technology
        // mechanics body. Runtime probes on Graveyard Keeper 1.407 showed vanilla
        // rows at max_width = -1 and the owned row at max_width = 900.
        internal const int OwnedMaxWidth = 900;

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

                // Runtime probe 0.1.2 proved the owned UILabel is ResizeFreely but
                // retains overflowWidth=150, while data.max_width=900. For NGUI
                // ResizeFreely, overflowWidth is the native wrap/expansion ceiling;
                // mutating UILabel.width directly is therefore the wrong seam.
                // Set only the native overflow ceiling and force UILabel to process
                // the already-assigned text now, before the enclosing bubble asks
                // the child widget for its size.
                R.Set(label, "overflowWidth", maxWidth);
                R.Get(label, "processedText");
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
    }
}
