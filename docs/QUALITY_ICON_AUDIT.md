# Native prayer quality icon audit

Status: runtime/static evidence accepted for implementation, 2026-09-16.

## Question

Can Technology prayer summaries replace translated `Bronze / Silver / Gold` words with the game's own quality-star symbols without adding custom art, extra UI objects, polling, or a new localization burden?

## Evidence

Read-only probe `PrayerClarity.AuditProbe 0.1.9` against Graveyard Keeper 1.407 (`Assembly-CSharp` MVID `6f50b8e7-156b-49ac-bbe8-7505894b2364`) found:

- `BaseItemCellGUI` owns a native `UI2DSprite quality_icon` and delegates its rendering to `ItemDefinition.TryDrawQualityOrDisableGameObject(...)`.
- Live/prefab item cells use native quality sprites such as `item_star_1` and `item_star_3`.
- More importantly for text-only Technology tooltips, the game's `small_font` already exposes native inline NGUI symbols:
  - `(s1)` -> `item_star_1_font`;
  - `(s2)` -> `item_star_2_font`;
  - `(s3)` -> `item_star_3_font`.
- The same font also owns already-verified inline symbols PrayerClarity uses elsewhere, including `(cross)` and `(faith)`.
- Technology prayer details are already rendered through `BubbleWidgetTextData`; therefore the lowest-cost seam is to emit the existing inline quality tokens in the existing text block rather than create a separate sprite/widget.

## Decision

Use `(s1)`, `(s2)`, `(s3)` for prayer quality tiers 1/2/3 in Technology summaries.

Do not add custom textures or clone item-cell `quality_icon` objects for this surface. Those approaches are strictly heavier and unnecessary given the verified native inline-symbol seam.

## Runtime acceptance still required

The implementation must still be visually checked in the actual Technology tooltip to confirm:

- the symbols render rather than appearing as raw `(s1)` text;
- baseline/spacing looks native;
- all three qualities are distinguishable and aligned;
- no wrapping regression occurs in the tested tooltip layouts.
