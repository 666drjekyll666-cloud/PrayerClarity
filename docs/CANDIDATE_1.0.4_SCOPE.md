# PrayerClarity 1.0.4 — compact success rows and local width candidate

Status: **Clarity-only runtime candidate preparation**.

Baseline: accepted/published `PrayerClarity 1.0.1`. Immediate predecessor `candidate/1.0.3` was runtime-tested and superseded, not accepted.

## Runtime feedback carried forward

1. Prayer-item current-tier-only tooltips remain accepted; do not change their width or grammar.
2. Technology comparative success rows in 1.0.3 became too tall when each quality rendered on its own line.
3. The separate `(faith)` line in Imagination was visually orphaned from the tier progression.
4. Long comparative prayer tooltips can still leave the screen vertically even at 2560x1440.
5. Requirement rows already read well horizontally; varying duration rows read well vertically.

## Accepted 1.0.4 refinement

### Success block

- Keep the heading `Added on success:` / localized equivalent.
- Faith and Donations become named resource sub-blocks.
- If the complete contribution is identical for every tier, show it once.
- If any component differs, each tier shows the complete contribution, including shared components, so the progression is self-contained.
- Differing tier values render horizontally on one line with native quality and resource symbols.
- Faith and Donations blocks are separated by one blank line.
- Item tooltips retain the 1.0.3 single-tier grammar.

Conceptual example:

```text
Added on success:
Faith:
(s1) +25% +1 (faith)   (s2) +25% +2 (faith)   (s3) +25% +3 (faith)

Donations:
(s1) +25% (slv)   (s2) +50% (slv)   (s3) +75% (slv)
```

The Faith label uses the verified vanilla localization ID `faith`; PrayerClarity does not add a duplicate translation table entry.

### Width

Direct runtime metadata evidence identifies the fourth argument of `BubbleWidgetTextData(string, TextStyle, Alignment, int)` as `max_width`. Vanilla Technology prayer rows pass `-1`.

For this candidate only the PrayerClarity-generated **Technology mechanics body** uses a fixed `max_width = 300`.

- no global Tooltip resize;
- no item-tooltip width change;
- no per-frame measurement;
- no dynamic screen scan;
- no positioning/clamping patch yet.

`300` is a runtime-calibration value. It is deliberately fixed and cheap; acceptance depends on real 2560x1440 and later 1920x1080 behavior.

### Other properties

- Requirement remains horizontal.
- Varying Duration remains vertical, one complete tier per line.
- Effect/reward/crafting-location grammar remains as in 1.0.3.

## Non-scope

- no prayer mechanics changes;
- no Vanilla Fixes;
- no Balance/Rework;
- no pulpit changes;
- no Temporary Effects changes;
- no item-tooltip redesign;
- no global tooltip sizing behavior.

## Requested runtime check

At 2560x1440 inspect at minimum:

1. Faith/Donations/Combo Technology prayers: success progression should remain on one line per resource and Faith/Donations should be visually separated.
2. Imagination: no orphan `(faith)` line; the success progression should read as one resource block.
3. Repose or another long comparative prayer: confirm the reduced height and wider bubble remain visually natural.
4. Prosperity: confirm the property-first reward block still fits and the wider mechanics body does not create awkward wrapping elsewhere.
5. One prayer item tooltip: confirm item width/grammar is unchanged.

If 2560x1440 is clean, perform a 1920x1080 smoke test before considering stable acceptance.
