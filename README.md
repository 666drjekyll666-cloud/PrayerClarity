# PrayerClarity

PrayerClarity is a BepInEx mod for **Graveyard Keeper 1.407** that makes prayers easier to understand before you use them, without changing prayer balance or sermon mechanics.

**Current stable version: 1.0.20**

[Download the latest GitHub release](https://github.com/666drjekyll666-cloud/PrayerClarity/releases/latest)

## What it changes

- **Pulpit:** explains the guaranteed reward sources, exact success chance, prayer-owned success bonuses, special effects, and effect duration while preserving the sermon itself as the reveal moment for the final Faith/donation payout.
- **Technology tree:** shows shared prayer properties once, then compact Bronze/Silver/Gold tier snapshots with the quality needed for 100% success and the values that actually change by tier. Long descriptive effects wrap inside a content-driven width instead of making the whole tooltip unnecessarily wide.
- **Prayer item tooltips:** show the mechanics of the concrete prayer quality you are holding rather than repeating the full three-tier comparison.
- **Character -> Temporary Effects:** shows the actual quantitative effect of active prayer buffs and expresses long remaining durations in in-game days.
- **Localization:** PrayerClarity-owned text is included for all 11 interface languages supported by Graveyard Keeper: English, French, German, Simplified Chinese, Spanish, Brazilian Portuguese, Korean, Japanese, Russian, Italian, and Polish.

## What it does not change

PrayerClarity 1.0.20 is **Clarity-only**. It does not rebalance prayers, change sermon rewards, repair broken vanilla prayer effects, or otherwise alter prayer gameplay mechanics.

The repository also contains research for future Vanilla Fixes and Balance/Rework work. Those are separate from the stable Clarity release.

## Installation

### Nexus archive

1. Install **BepInEx 5** for Graveyard Keeper.
2. Extract the PrayerClarity archive into the Graveyard Keeper game folder.
3. The installed DLL should end up at `BepInEx/plugins/PrayerClarity/PrayerClarity.dll`.

### Raw DLL / GitHub Release

1. Install **BepInEx 5** for Graveyard Keeper.
2. Download `PrayerClarity.dll` from the latest GitHub Release.
3. Put it in `Graveyard Keeper/BepInEx/plugins/PrayerClarity/`.
4. When updating, replace the existing DLL. Do not keep multiple PrayerClarity versions side by side.

PrayerClarity requires no user-facing configuration.

## Compatibility

- Graveyard Keeper **1.407**
- BepInEx 5

PrayerClarity verifies the supported `Assembly-CSharp` build before installing its patches. On an unverified game binary it disables itself instead of patching unknown code.
