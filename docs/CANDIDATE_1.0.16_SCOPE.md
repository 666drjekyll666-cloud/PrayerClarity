# PrayerClarity 1.0.16 candidate scope

Status: design-locked development scope; runtime candidate only after build/handoff gate.

## Evidence leading to 1.0.16

Runtime tests established that PrayerClarity can safely control the Technology mechanics UILabel through its native NGUI `overflowWidth` seam. The 1.0.13/1.0.14 content-driven width implementation works, while 1.0.15 demonstrated the limit of character-count based effect wrapping: medium prose improved, but Repose became taller without a corresponding readability benefit.

The user also confirmed a future `PrayerClarity: Rebalance` profile/mod will reuse this Clarity presentation layer while changing prayer effects and bonuses. Width policy therefore must generalize to new localized effect text without prayer-specific exception lists or per-string tuning.

## 1.0.16 layout policy

Technology mechanics uses a **pixel-measured anchor width**.

- Atomic decision rows are hard width owners. In practice these are primarily the per-tier `100% success requires ...` rows, plus other compact mechanics rows that are intentionally kept intact.
- Descriptive/special-effect prose is a soft row and must not widen the tooltip beyond the atomic anchor. It wraps using the game's native UILabel/NGUI word wrapping.
- Structured item rewards are separate from flat Faith/money additions so a long localized reward name can wrap independently instead of widening the whole tier row.
- The anchor is measured from the live UILabel in rendered pixels, not from character count. This avoids assumptions about Russian/Latin/CJK glyph widths and remains usable for future Rebalance text.
- The existing viewport clamp remains the final safety layer.
- No per-frame measurement, polling or broad scans: measurement happens only when the owned `BubbleWidgetText` is drawn/refreshed.

## Implementation boundary

- Remove the 1.0.15 `>45 characters` effect-wrap heuristic and its manual newline insertion.
- In the existing `BubbleWidgetText.Draw` postfix for PrayerClarity's owned Technology row, temporarily use the verified wide measurement ceiling, measure explicit source lines through the live UILabel `printedSize`, derive an anchor from hard rows, then set `overflowWidth` to that anchor and let NGUI produce the final word wrapping.
- Treat effect prose and standalone `×` reward lines as soft rows for anchor selection.
- Fall back safely to the existing wide ceiling if measurement cannot be established.
- Keep the change Technology-only and Clarity-only; no prayer mechanics/balance changes.

## Runtime gate

Inspect at least:

- Repose: should return to a compact natural height; effect wraps only if the atomic anchor actually requires it.
- Repentance and Shoots & Roots: medium-long effect prose should remain compact rather than becoming width drivers.
- Prosperity: long `Blessing of commerce` reward text must not force an excessively wide tooltip; flat Faith/money outputs remain semantically separate.
- Combo: dense numeric tier rows remain intact.
- Russian and English are the primary visual checks; a CJK spot check is useful if convenient.
- Ordinary non-prayer Technology tooltips remain untouched.
