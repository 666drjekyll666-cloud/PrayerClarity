# PrayerClarity 1.0.11 — Technology tooltip width correction

Status: **runtime candidate preparation; not accepted**.

Baseline: accepted mainline remains PrayerClarity 1.0.9. Immediate predecessor `candidate/1.0.10` was runtime-tested and superseded because the intended adaptive Technology width did not materialize in-game.

## Narrow problem

The 1.0.10 tier-first semantic structure rendered, but coherent tier rows still wrapped at approximately the previous narrow Technology width. This defeats the design goal of buying horizontal space to preserve the tier snapshot structure.

Historical runtime evidence already established that `BubbleWidgetTextData.max_width` controls this wrapping and that `300 -> 360` changed the live tooltip. Therefore the next correction should remain at that verified local seam rather than introducing another UI lifecycle hook.

## Candidate width rule

Only the PrayerClarity-generated **Technology mechanics body** uses:

`max_width = 900`

This is a **maximum**, not a forced width. NGUI/content should choose the actual bubble width from the longest rendered line up to this ceiling. A short prayer therefore remains compact; a long tier row is allowed to widen instead of wrapping merely because of the old 360 cap.

Why 900:

- large enough that the Russian tier requirement/reward rows and longer Latin-localized variants are not expected to hit the cap under normal content;
- still comfortably below the logical horizontal space of the project's 1920x1080 / 2560x1440 acceptance targets and typical 16:9 UIRoot geometry;
- preserves a finite safety ceiling instead of using an unbounded `-1`;
- removes the brittle 1.0.10 attempt to discover UI-root/safe-area geometry before the final bubble exists.

The already accepted PrayerClarity-owned viewport clamp remains unchanged and continues to reposition only marked prayer Technology bubbles if necessary.

## Unchanged behavior

- tier-first grammar from 1.0.10;
- `(s1)/(s2)/(s3)` as tier headings;
- localized `100% success requires` sentence;
- shared proportional modifiers above tiers;
- flat additions separated from percentages;
- Commercial Blessing purpose paragraph omitted from Technology body;
- prayer-item tooltip, pulpit and Temporary Effects unchanged;
- no mechanics, Vanilla Fixes or Balance/Rework changes.

## Runtime gate

Primary check: Prosperity and Shoots & Roots in Russian at the normal resolution.

Expected result:

1. `(sN) Для 100% успеха требуется N (cross)` stays on one physical line when viewport space permits;
2. Prosperity flat Faith/money/Blessing row stays on one physical line when viewport space permits;
3. tooltip becomes only as wide as needed by content, not a fixed 900-wide panel;
4. tooltip remains inside the viewport;
5. tier grouping/readability from 1.0.10 is preserved.

If these two screenshots are clean, spot-check Faith/Donations or Combo. No sermon execution is required.
