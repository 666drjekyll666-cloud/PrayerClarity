# PrayerClarity 0.1.19 candidate scope

Status: pre-runtime candidate scope, 2026-09-16.

0.1.19 remains Clarity-only. It does not intentionally change prayer mechanics, success probability, sermon rewards, buff magnitude, save data or balance.

## Native quality stars in Technology prayer details

Read-only probe 0.1.9 verified that Graveyard Keeper 1.407 already exposes inline quality-star symbols in the native `small_font`:

- `(s1)` -> `item_star_1_font`;
- `(s2)` -> `item_star_2_font`;
- `(s3)` -> `item_star_3_font`.

The existing Technology prayer details are rendered as `BubbleWidgetTextData` and already rely on the same native inline-symbol system for stock tokens such as `(cross)`.

0.1.19 therefore replaces the translated quality words `Bronze / Silver / Gold` in PrayerClarity's Technology summary rows with the corresponding native quality-star tokens. No custom image assets, extra icon GameObjects, per-frame work, or new localization strings are introduced.

## Requested runtime check

Open a Technology tooltip that shows all three prayer qualities and confirm:

1. the three rows begin with native quality stars rather than raw `(s1)`, `(s2)`, `(s3)` text;
2. the stars look like the game's own Bronze/Silver/Gold quality marks and are visually distinguishable;
3. baseline and spacing are clean;
4. no new wrapping, clipping, or tooltip overlap appears.

If those checks pass, this quality-label change is ready for acceptance.
