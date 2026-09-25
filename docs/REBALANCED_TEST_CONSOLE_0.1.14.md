# Rebalanced Test Console 0.1.14

Read-only final-wrap trace for prayer-item resource clusters.

## Research-method checkpoint

- Question: can PrayerClarity detect the final NGUI wrap that separates a numeric amount from its immediately following inline resource icon, so the behavior can be fixed once for PrayerClarity-owned prayer-item mechanics rows rather than with prayer-specific hard line breaks?
- Existing path: use exact Rebalanced 0.2.30, whose accepted wording already reproduces the orphaned Soul Gratitude icon without reconstructing that baseline inside the console.
- Why a probe is still needed: source inspection establishes the owner and width path but does not expose the runtime `UILabel.processedText` after NGUI wrapping. A read-only trace of that final state is narrower and more reliable than guessing another separator/whitespace character.

## Probe

- Use exact Rebalanced 0.2.30.
- The console does not rewrite Soul's Repose wording or wrapping.
- A read-only postfix runs after PrayerClarity's `BubbleWidgetText.Draw` content-width postfix.
- It logs only 200-unit prayer-item rows containing `(faith)` or `(gratitude_points)`.
- Log prefix: `PRAYER_ITEM_WRAP_TRACE`.
- Fields include raw text, final `processedText`, label width, overflow mode and printed size.

## Required runtime action

Install Rebalanced 0.2.30 and Test Console 0.1.14. Open the prayer-item gallery and hover Gold Soul's Repose once. Return the log. One screenshot is optional because the orphaned icon symptom is already accepted runtime evidence.

## Acceptance use

If `processedText` exposes the final line break around the resource token, the production gate can target the common PrayerClarity-owned prayer-item wrapping seam and preserve amount+icon clusters universally within that surface. If not, the next research step must inspect the lower NGUI token/print path rather than introduce prayer-specific wording changes.