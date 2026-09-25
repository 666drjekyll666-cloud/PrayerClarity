# Rebalanced Test Console 0.1.15

Research-only amount+inline-icon wrap repair probe for PrayerClarity-owned prayer-item mechanics rows.

## Established evidence

- Exact Rebalanced 0.2.30 + Test Console 0.1.14 recorded the Soul's Repose raw row as `... 1 (faith) ... 90 (gratitude_points)`.
- Final NGUI `processedText` changed those clusters to `1\\n(faith)` and `90\\n(gratitude_points)` at the fixed 200-unit PrayerClarity item-mechanics width.
- Therefore the orphaning occurs in final NGUI wrapping after PrayerClarity has supplied the text and width.

## Research-method checkpoint

- Question: can one common post-wrap normalization keep an amount and its immediately following inline symbol together without rewriting wording or widening the tooltip?
- Existing evidence/source inspection identifies the wrap owner but cannot establish visual safety of the correction.
- A runtime mutation probe is justified because the property is visual and the candidate normalization can be applied after the exact final wrap state is known.

## Probe

- Use exact Rebalanced 0.2.30.
- Starts ON and can be toggled from F1.
- Does not change localization wording, prayer mechanics, item-tooltip width, title, or other surfaces.
- After the normal PrayerClarity content-width pass, if final `processedText` contains a line boundary where the previous line ends in an integer amount and the next line starts with an inline `(symbol)` token, the probe inserts a hard newline before that amount in the original raw row and reprocesses the label.
- Example intended normalization: `... даёт 1\\n(faith)` -> `... даёт\\n1 (faith)`.
- Existing final-wrap trace remains enabled.
- Repair log prefix: `PRAYER_ITEM_WRAP_REPAIR_PROBE`.

## Required runtime action

Install Rebalanced 0.2.30 and Test Console 0.1.15. Open the prayer-item gallery and hover Gold Soul's Repose. Confirm visually that neither `(faith)` nor `(gratitude_points)` is orphaned from its number and that the wording/overall tooltip remains readable. Return the log.

## Acceptance use

If the visual result is accepted and the repaired `processedText` keeps both clusters intact, the production gate for the common PrayerClarity-owned prayer-item amount+icon wrapping rule can become READY. Production should keep the blast radius limited to PrayerClarity-owned prayer-item mechanics rows.