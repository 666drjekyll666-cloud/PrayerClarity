# PrayerClarity 1.0.12 candidate scope

Status: **runtime candidate; not accepted until user in-game verification**.

## Why 1.0.12 exists

PrayerClarity 1.0.10 and 1.0.11 implemented the accepted tier-first Technology tooltip information architecture, but runtime showed that the tooltip body stayed at the old narrow width even when the PrayerClarity mechanics `BubbleWidgetTextData.max_width` was raised to `900`.

Two read-only runtime probes closed the ownership question on Graveyard Keeper 1.407 (Assembly-CSharp MVID `6f50b8e7-156b-49ac-bbe8-7505894b2364`):

- the live PrayerClarity mechanics data reaches the game with `max_width=900`;
- the corresponding live `UILabel` is nevertheless created at `width=148`, `lineWidth=148` and wraps at that width;
- `BubbleWidgetText.Draw(BubbleWidgetTextData)` sets alignment/style/font/text but never reads `BubbleWidgetTextData.max_width`;
- `WidgetsBubbleGUI.UpdateSize()` later sizes the outer bubble from the already-created child widget sizes, so the outer bubble is downstream, not the primary constraint.

Therefore the rejected 1.0.10/1.0.11 assumption was that `BubbleWidgetTextData.max_width` is consumed natively by the game.

## Narrow implementation

1.0.12 adds `TechnologyTooltipContentWidth` and patches only `BubbleWidgetText.Draw(BubbleWidgetTextData)` with a postfix.

The postfix is scoped to the PrayerClarity Technology mechanics body by the existing owned `max_width=900` marker. It does not touch ordinary vanilla tooltip rows (`max_width=-1` in the captured runtime evidence).

For the owned row only it:

1. temporarily gives the live `UILabel` the finite 900-unit measurement ceiling;
2. reads NGUI `printedSize` for the already-localized/encoded text;
3. chooses the natural longest-line width (with two units of measurement padding), never smaller than the original prefab width and never larger than 900;
4. applies that final width and recalculates the label height from the final `printedSize`;
5. leaves the normal `WidgetsBubbleGUI.UpdateSize()` / table lifecycle to size and position the enclosing bubble.

There is no hierarchy scan, polling, mirrored UI state, or prayer-mechanics change. The existing PrayerClarity viewport clamp remains responsible for final on-screen safety.

## Intended UX behavior

The accepted tier-first grammar remains unchanged. The only intended visible change from 1.0.11 is geometry:

- coherent tier rows should remain on one line when their natural width fits below the finite ceiling;
- the Technology tooltip should become only as wide as its longest relevant line, not a fixed 900 units;
- shorter tooltip content should not be forced to 900 units;
- semantic spacing/text/icons remain the 1.0.11 design.

## Required runtime check

Use the same cases that exposed the defect:

- Prosperity (preferred, because its tier reward row is wide);
- Shoots & Roots or Repose as a second structurally different prayer.

Check:

- `★ Для 100% успеха требуется …` stays together where space permits;
- tier reward/modifier rows no longer wrap merely because of the old 148-unit prefab width;
- bubble width follows content rather than jumping to the full 900-unit ceiling;
- no clipping or off-screen regression;
- no new PrayerClarity width-sizing error appears in `LogOutput.log`.

Do not repeat the full accepted 1.0.9 Clarity regression suite unless this narrow change exposes a broader problem.
