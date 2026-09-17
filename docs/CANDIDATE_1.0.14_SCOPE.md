# PrayerClarity 1.0.14 candidate scope

Status: runtime candidate; not accepted until user in-game verification.

## Runtime evidence entering this candidate

PrayerClarity 1.0.13 was loaded in Graveyard Keeper 1.407 and the user verified that the `UILabel.overflowWidth` fix finally made Technology prayer tooltips expand with their content. The tier-first structure remained readable and coherent tier rows no longer suffered the previous narrow-prefab wrapping failure.

The same runtime screenshots exposed three narrower polish issues:

1. Native prayer titles can wrap awkwardly before their final noun, e.g. Russian `Молитва об` / `упокоении`.
2. A single unusually long effect sentence can become the width driver and leave excessive empty horizontal space elsewhere in the tooltip; Repose is the clearest observed case.
3. The tier label `Длительность` is semantically ambiguous because it can visually read as the duration of the immediately preceding Faith/donation bonus rather than the prayer's temporary effect.

These are presentation findings only. No prayer mechanics change is authorized by this candidate.

## Candidate changes

- Preserve the working 1.0.13 native NGUI `overflowWidth` seam unchanged.
- Apply a narrow title widow-control rule on the verified native Technology prayer title row: only a short final connector (up to four characters) is joined to the final word with a non-breaking space. Ordinary titles are not made fully atomic.
- For Technology special-effect prose only, insert one semantic line break when a localized effect string is longer than 72 characters and a reasonable whitespace split exists near its midpoint. This keeps pathological one-line effects from dictating the width of the entire tooltip while leaving normal effect strings unchanged.
- Rename the mod-owned Technology duration label from generic `Duration` to `Effect duration` (localized equivalents in all 11 supported locales).
- No changes to pulpit behavior, item-tooltip behavior, Temporary Effects behavior, prayer mechanics, payout calculations, success calculations, duration calculations, or viewport-clamp logic.

## Narrow runtime gate

Required user verification:

1. Repose Technology tooltip: title should no longer orphan the final word; the long Donkey effect should wrap into a balanced multi-line block; overall tooltip width should become visibly more compact without returning to the old 150-unit wrapping failure.
2. One timed prayer such as Imagination or another verified duration-bearing prayer: tier rows should say the localized equivalent of `Effect duration`, making the duration/effect relationship explicit.
3. One compact prayer such as Combo: it should remain compact and should not receive an unnecessary long-effect line break.

Do not accept or merge solely from CI success. Runtime presentation verification is required.
