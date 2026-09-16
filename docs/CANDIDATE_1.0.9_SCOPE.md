# PrayerClarity 1.0.9 — final Technology tooltip edge breathing room

Status: **Clarity-only runtime candidate preparation**.

Baseline: runtime-tested `candidate/1.0.8` source `4ce32e11f782eef10ade0c8569d206ba7bca6df3`.

## Runtime result from 1.0.8

User runtime screenshots on 2026-09-17 established that the viewport clamp itself works across the intended scenarios:

- 1920x1080 mouse: a long PrayerClarity Technology tooltip is translated upward and all content remains visible;
- 1920x1080 controller without Gamepad Tooltip Position Fix: the long combined tooltip is translated upward and remains readable;
- 2048x1152 controller without the companion: placement remains usable;
- 2048x1152 controller with Gamepad Tooltip Position Fix: the companion's left-side placement remains intact and PrayerClarity does not disturb it unnecessarily.

The remaining defect is cosmetic: on clamped lower-edge cases the visible parchment/frame can still visually touch the screen edge even though the measured `WidgetsBubbleGUI.widget` is clamped with a 16-screen-pixel logical margin.

The screenshots therefore add direct presentation evidence that the visible frame extends beyond the measured widget bounds by approximately the same 16-screen-pixel amount at the lower edge. This is not a content or mechanics issue.

## 1.0.9 change

Increase `TechnologyTooltipViewportClamp.SafeMarginPixels` from `16` to `24`.

This is intentionally only an additional 8 screen pixels of logical breathing room:

- enough to create a small visible lower gap in the runtime-tested 1080p cases;
- conservative for the very tall combined controller tooltip, whose visible top edge still has only a few tens of pixels of headroom;
- small enough that existing 1440p / 1152p placements should not visibly drift unless they are genuinely near an edge;
- companion placement with an already-large safe margin remains unchanged because the clamp performs no write when no correction is required.

Do not increase to 32 px without new evidence: the tallest combined tooltip is close enough to the 1080p viewport height that a larger symmetric logical margin would unnecessarily reduce fit headroom and could trigger the oversize-axis fallback for localized variants.

## Unchanged behavior

- The 1.0.8 ownership/weak-marker isolation remains unchanged.
- `Screen.safeArea`, current `UIRoot.manualHeight`, and current screen dimensions remain the source of viewport geometry.
- The postfix remains ordered after `nikich.gyk.movegamepadtooltips`.
- Prayer content, prayer-item tooltips, pulpit, Temporary Effects, mechanics, balance and localization strings are unchanged.
- Ordinary non-prayer Technology tooltips remain unmarked and untouched.

## Requested runtime acceptance

A focused visual retest is sufficient:

1. 1920x1080 mouse, one long prayer tooltip: confirm a small visible lower gap now exists.
2. 1920x1080 controller without companion, the long combined tooltip: confirm both top and bottom frame remain visible.
3. Controller with Gamepad Tooltip Position Fix: confirm its left-side placement remains unchanged unless an edge actually requires correction.

If those pass and `LogOutput.log` has no PrayerClarity viewport-safety error, 1.0.9 can replace 1.0.8 as the accepted viewport-polish candidate.
