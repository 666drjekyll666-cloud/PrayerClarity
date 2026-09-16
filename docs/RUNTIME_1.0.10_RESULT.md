# PrayerClarity 1.0.10 — runtime Technology UX result

Status: **superseded / not accepted**, 2026-09-17.

Frozen candidate: `candidate/1.0.10` at source SHA `f21674f0ddcc7e06a7d0d1b587faa57e2e5c0b1d`.

## Runtime evidence

The user tested the tier-first Technology tooltip candidate in Graveyard Keeper 1.407 and supplied screenshots of Shoots & Roots and Prosperity.

The new semantic grouping is visibly active, but the intended width behavior is not: coherent tier lines still wrap at approximately the previous narrow Technology width. Examples include the localized guaranteed-success sentence and tier reward lines splitting over multiple physical lines.

This directly rejects the 1.0.10 adaptive-width implementation as sufficient.

## Engineering conclusion

Historical runtime evidence already established that `BubbleWidgetTextData(string, TextStyle, Alignment, int)` argument 4 is `max_width`, and that increasing the PrayerClarity Technology mechanics-body cap from `300` to `360` changed wrapping in-game.

1.0.10 attempted to derive a larger cap early from the `Tooltip` component's root / `UIRoot` geometry. Runtime behavior shows that this path did not produce the intended wider body in the tested Technology lifecycle.

Do not add more early UI-root discovery or broad hierarchy scanning merely to preserve the dynamic calculation.

Next candidate should use the least-sufficient mechanism:

- keep width control local to the PrayerClarity Technology mechanics `BubbleWidgetTextData` only;
- use a generous fixed **maximum**, not a forced width;
- let NGUI/content determine the actual width up to that cap;
- retain the already accepted PrayerClarity-owned final viewport clamp;
- keep prayer-item tooltip, pulpit, Temporary Effects and mechanics unchanged.

The width correction must use a new numbered candidate because 1.0.10 was already handed out and is immutable.
