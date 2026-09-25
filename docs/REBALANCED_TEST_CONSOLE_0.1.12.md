# Rebalanced Test Console 0.1.12

Research-only visual wrapping probe for the remaining Soul's Repose prayer-item tooltip defect.

## Established facts

- Production Rebalanced 0.2.30 uses the previously accepted understandable Soul's Repose wording.
- Replacing the ordinary space between the cap number and `(gratitude_points)` with U+00A0 did not prevent the inline icon from being orphaned at runtime.
- `PrayerForecast.BuildSpecial` owns the semantic Soul's Repose sentence; `ItemTooltipPresentation` owns the final prayer-item tooltip rows and the fixed 200 UI-unit mechanics column.
- Wording is a preserved invariant for this probe.

## Probe

- The old Combo Prayer quality-glyph mutation probe remains OFF by default.
- A research-only postfix runs after PrayerClarity production item-tooltip composition for `b_souls*` items.
- When enabled, it changes only the layout separator after `(faith).` from a space to a real newline. All words, values, icons, mechanics and the fixed production width remain unchanged.
- The probe starts ON and can be toggled in F1 as `Soul's Repose sentence-break probe`.
- Log record prefix: `SOULS_REPOSE_WRAP_PROBE`.

## Required runtime action

Use PrayerClarity: Rebalanced 0.2.31 (or the equivalent rejected presentation source currently under test) with Test Console 0.1.12. Open the prayer-item gallery and hover one Soul's Repose item. Confirm whether the old wording is readable and the final Soul Gratitude icon remains with its number rather than occupying a line by itself. No sermon or mechanics test is required.

## Acceptance use

If the sentence break fixes the orphaned icon without degrading the tooltip, the production gate for this exact layout change becomes READY. If not, production remains BLOCKED and the next research step must investigate row width/token wrapping rather than rewrite the player-facing wording.
