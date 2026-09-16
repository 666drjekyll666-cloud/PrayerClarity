# PrayerClarity 1.0.9 — final Technology tooltip edge breathing room

Status: **accepted Clarity mainline baseline**.

Accepted runtime/source SHA: `4b86b972baeef19aa656a9e414891405006cf67f`.
Frozen candidate ref: `candidate/1.0.9`.
Frozen accepted ref: `accepted/clarity-1.0.9`.
GitHub Actions run: `35155851294`.
Artifact ID: `10470818616` (`PrayerClarity-1.0.9-ci-4b86b972baeef19aa656a9e414891405006cf67f`).
Artifact ZIP digest: `sha256:dea2af19f544f5078c7b1af091c71346d5c6664056aff45d0290dd4a5b5d363d`.
DLL SHA-256: `0c874740643d8522857ee0a48a3f12fb2bdcc0612a61685e0db4fe7ad5d20788`.

## Runtime result from 1.0.8

User runtime screenshots on 2026-09-17 established that the viewport clamp itself works across the intended scenarios:

- 1920x1080 mouse: a long PrayerClarity Technology tooltip is translated upward and all content remains visible;
- 1920x1080 controller without Gamepad Tooltip Position Fix: the long combined tooltip is translated upward and remains readable;
- 2048x1152 controller without the companion: placement remains usable;
- 2048x1152 controller with Gamepad Tooltip Position Fix: the companion's left-side placement remains intact and PrayerClarity does not disturb it unnecessarily.

The remaining defect was cosmetic: on clamped lower-edge cases the visible parchment/frame could visually touch the screen edge even though the measured `WidgetsBubbleGUI.widget` was clamped with a 16-screen-pixel logical margin.

The screenshots therefore added direct presentation evidence that the visible frame extends beyond the measured widget bounds by approximately the same 16-screen-pixel amount at the lower edge. This was not a content or mechanics issue.

## 1.0.9 change

`TechnologyTooltipViewportClamp.SafeMarginPixels` was increased from `16` to `24`.

This is intentionally only an additional 8 screen pixels of logical breathing room:

- enough to create a small visible lower gap in the runtime-tested 1080p cases;
- conservative for the very tall combined controller tooltip, whose visible top edge has limited headroom;
- small enough that existing 1440p / 1152p placements do not visibly drift unless genuinely near an edge;
- companion placement with an already-large safe margin remains unchanged because the clamp performs no write when no correction is required.

Do not increase to 32 px without new evidence: the tallest combined tooltip is close enough to the 1080p viewport height that a larger symmetric logical margin would unnecessarily reduce fit headroom and could trigger the oversize-axis fallback for localized variants.

## Accepted runtime result

On 2026-09-17 the user runtime-tested the exact 1.0.9 candidate and explicitly reported that all tested cases are good and that the viewport issue can be closed.

Accepted visual coverage:

1. 1920x1080 mouse: long prayer Technology tooltip fits with acceptable lower-edge breathing room.
2. 1920x1080 controller without companion: long combined tooltip remains fully visible and usable.
3. Controller with Gamepad Tooltip Position Fix: the companion's left-side placement remains correct and PrayerClarity does not interfere unnecessarily.
4. Higher-resolution/default use remains visually correct.
5. Prayer item tooltips remain unaffected by this viewport change.

The acceptance is visual/runtime evidence from the supplied screenshots and explicit user confirmation. No separate 1.0.9 runtime log was supplied with this final visual acceptance, so this document does not claim additional log-derived evidence beyond the successful candidate CI/build evidence.

## Unchanged behavior

- The 1.0.8 ownership/weak-marker isolation remains unchanged.
- `Screen.safeArea`, current `UIRoot.manualHeight`, and current screen dimensions remain the source of viewport geometry.
- The postfix remains ordered after `nikich.gyk.movegamepadtooltips`.
- Prayer content, prayer-item tooltips, pulpit, Temporary Effects, mechanics, balance and localization strings are unchanged.
- Ordinary non-prayer Technology tooltips remain unmarked and untouched.

## Promotion

`candidate/1.0.9` and `accepted/clarity-1.0.9` both point to the exact accepted runtime/source SHA above.

`main` was fast-forwarded to that same SHA without rebuilding. Any subsequent `main` commits that only record acceptance/release documentation do not change the accepted runtime bytes.

The latest public GitHub Release remains `v1.0.1` until a separate publication decision is made.