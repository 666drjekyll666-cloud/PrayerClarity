# PrayerClarity 1.0.1 — prayer item tooltip candidate

Status: **runtime candidate; not accepted**.

## Scope

This is a Clarity-only follow-up to the accepted PrayerClarity 1.0.0 baseline. It does not change prayer mechanics, balance, sermon rewards, inventory contents, or save data.

The only product change is a missing presentation surface: hovering an actual prayer item now receives the same structured prayer-details information already shown by PrayerClarity in the Technology tooltip.

## Evidence / implementation boundary

- Graveyard Keeper 1.407 exposes `ItemDefinition.GetTooltipData(Item, bool)` as the native item-tooltip data seam.
- Prayer item definitions expose their linked prayer craft through `linked_craft`.
- Multi-quality prayer item families use the verified `root:1`, `root:2`, `root:3` item naming convention and `ItemDefinition.GetNameWithoutQualitySuffix()` provides the family root.
- The item surface reuses the existing `PrayerForecast.BuildTierDetails` semantic model and `PresentationText.FormatPrayerContribution`; no second mechanics model is introduced.
- It reuses the same localization keys and native `(s1)/(s2)/(s3)` quality glyphs as Technology.
- If the expected stock prayer-mechanics rows exist, they are replaced in place; if an unexpected tooltip shape is encountered, vanilla rows are preserved and the Clarity block is appended instead.
- The new item-tooltip patch has its own fail-safe installation/error boundary, so failure cannot disable the already accepted pulpit, Technology, or Temporary Effects surfaces.

## Intended player-facing parity

For the same prayer family, the `Prayer details` block on an item tooltip should contain the same tier information as Technology:

- Bronze / Silver / Gold native quality stars where applicable;
- Church Quality requirement for each tier;
- prayer-owned success Faith/donation modifiers;
- special effect and strategic duration text.

No current-state pulpit context or exact resolved payout is added to the item tooltip.

## Test fixture

The existing PrayerClarity Test Harness 0.1.4 can construct a synthetic prayer `Item` and hand it to the real pulpit picker without granting the item or technology. Use it first as the non-mutating fixture: select a prayer/quality at the pulpit and hover the selected prayer item cell. If that stock cell exposes its normal item tooltip, no inventory injection is needed.

## Runtime acceptance condition

Compare a representative prayer's Technology tooltip and actual/synthetic prayer-item hover tooltip. The structured prayer details must match semantically and visually, with native quality stars and no duplicated stock mechanic block. Existing 1.0.0 pulpit and Technology behavior must remain intact.
