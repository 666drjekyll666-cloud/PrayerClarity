# PrayerClarity

PrayerClarity is a BepInEx mod for **Graveyard Keeper 1.407** that makes prayer/sermon information clearer without changing prayer balance or gameplay mechanics.

Accepted Clarity mainline baseline: **1.0.9**.  
Latest public GitHub Release: **1.0.1**.

[Download the current public release, PrayerClarity 1.0.1](https://github.com/666drjekyll666-cloud/PrayerClarity/releases/tag/v1.0.1)

## What it changes

- **Pulpit:** explains guaranteed reward sources, success-only prayer modifiers, success chance, and special prayer effects without revealing the exact final sermon payout in advance.
- **Technology tree:** uses a compact property-first comparison of Bronze/Silver/Gold prayer progression, collapses invariant information instead of repeating it for every quality tier, keeps quality/value segments intact when wrapping, and keeps PrayerClarity-owned prayer tooltips inside the visible viewport when possible.
- **Prayer item tooltips:** inspect only the concrete hovered prayer item/current quality instead of duplicating the full Bronze/Silver/Gold comparison already available in Technology.
- **Character -> Temporary Effects:** shows the actual quantitative effect of active prayer buffs and expresses long durations in in-game days.
- **Localization:** PrayerClarity-owned UI text is included for all 11 interface languages supported by the game.

## What it does not change

The accepted 1.0.9 mainline baseline is **Clarity-only**. It contains no prayer balance changes, no Vanilla Fixes, and no intentional changes to sermon rewards or prayer mechanics.

The repository also contains evidence and design research for later Vanilla Fixes and Balance/Rework work. Those layers remain separate from the accepted Clarity baseline.

## Installation

For normal users, install the latest published GitHub Release:

1. Install BepInEx 5 for Graveyard Keeper.
2. Download `PrayerClarity.dll` from the current GitHub Release.
3. Put `PrayerClarity.dll` in `Graveyard Keeper/BepInEx/plugins/`.
4. Replace the same file when upgrading; do not keep multiple PrayerClarity versions side by side.

The repository `main` branch may contain a newer accepted runtime baseline than the latest public Release. Mainline acceptance and public publication are deliberately separate steps.

PrayerClarity has no user-facing configuration requirement for its accepted presentation.

## Compatibility

- Graveyard Keeper **1.407**
- BepInEx 5

PrayerClarity guards against an unverified `Assembly-CSharp` build and disables itself instead of patching an unsupported game binary.
