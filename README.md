# PrayerClarity

PrayerClarity is a BepInEx mod for **Graveyard Keeper 1.407** that makes prayer/sermon information clearer without changing prayer balance or gameplay mechanics.

Current stable Clarity release: **1.0.0**.

[Download PrayerClarity 1.0.0](https://github.com/666drjekyll666-cloud/PrayerClarity/releases/tag/v1.0.0)

## What it changes

- **Pulpit:** explains guaranteed reward sources, success-only prayer modifiers, success chance, and special prayer effects without revealing the exact final sermon payout in advance.
- **Technology tree:** shows prayer-quality requirements and the prayer-specific modifiers/effects for bronze, silver, and gold versions using the game's native quality-star symbols.
- **Character -> Temporary Effects:** shows the actual quantitative effect of active prayer buffs and expresses long durations in in-game days.
- **Localization:** PrayerClarity-owned UI text is included for all 11 interface languages supported by the game.

## What it does not change

Version 1.0.0 is **Clarity-only**. It contains no prayer balance changes, no Vanilla Fixes, and no intentional changes to sermon rewards or prayer mechanics.

The repository also contains evidence and design research for later Vanilla Fixes and Balance/Rework work. Those layers remain separate from the accepted 1.0.0 Clarity release.

## Installation

1. Install BepInEx 5 for Graveyard Keeper.
2. Download `PrayerClarity.dll` from the v1.0.0 GitHub Release.
3. Put `PrayerClarity.dll` in `Graveyard Keeper/BepInEx/plugins/`.
4. Replace the same file when upgrading; do not keep multiple PrayerClarity versions side by side.

PrayerClarity 1.0.0 has no user configuration entries; its accepted presentation is fixed in the release build.

## Compatibility

- Graveyard Keeper **1.407**
- BepInEx 5

PrayerClarity guards against an unverified `Assembly-CSharp` build and disables itself instead of patching an unsupported game binary.
