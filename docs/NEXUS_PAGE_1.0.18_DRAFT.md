# Nexus page draft — PrayerClarity 1.0.18

## Short description

Makes Graveyard Keeper's prayers easier to understand at the pulpit, in Technology, on prayer items, and in Temporary Effects. Clarity-only: no balance or sermon-mechanics changes.

## Description

PrayerClarity explains what each prayer actually does before you commit to using it.

It improves prayer information in four places:

- the pulpit, with clear guaranteed sources, success chance, prayer-owned success bonuses, special effects, and duration while keeping the exact final sermon payout as part of the sermon reveal;
- Technology tooltips, with compact shared details and Bronze/Silver/Gold tier snapshots showing the quality needed for 100% success and the values that change by tier;
- prayer item tooltips, with the mechanics of the concrete quality you are holding;
- Character -> Temporary Effects, with quantitative prayer-buff descriptions and long durations shown in in-game days.

Technology tooltips use content-driven sizing: important numeric decision rows stay readable while long descriptive effect text wraps instead of forcing every tooltip to become excessively wide.

PrayerClarity-owned text is localized for all 11 interface languages supported by Graveyard Keeper.

PrayerClarity 1.0.18 does not rebalance prayers or alter sermon rewards/mechanics.

## Requirements

- Graveyard Keeper 1.407
- BepInEx 5

## Installation

Extract the archive into the Graveyard Keeper game folder. The DLL should end up at:

`BepInEx/plugins/PrayerClarity/PrayerClarity.dll`

When updating, replace the existing PrayerClarity DLL and do not keep multiple versions side by side.
