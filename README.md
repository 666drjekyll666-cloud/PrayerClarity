# PrayerClarity

PrayerClarity is a BepInEx mod for **Graveyard Keeper 1.407** that makes prayer/sermon information clearer without changing prayer balance or gameplay mechanics.

Current stable Clarity baseline: **0.1.21**.

## What it changes

- **Pulpit:** explains guaranteed reward sources, success-only prayer modifiers, success chance, and special prayer effects without revealing the exact final sermon payout in advance.
- **Technology tree:** shows prayer-quality requirements and the prayer-specific modifiers/effects for bronze, silver, and gold versions using the game's native quality-star symbols.
- **Character -> Temporary Effects:** shows the actual quantitative effect of active prayer buffs and expresses long durations in in-game days.
- **Localization:** PrayerClarity-owned UI text is included for all 11 interface languages supported by the game.

## What it does not change

Version 0.1.21 is **Clarity-only**. It contains no prayer balance changes, no Vanilla Fixes, and no intentional changes to sermon rewards or prayer mechanics.

The repository also contains the evidence and design research for later Vanilla Fixes and Balance/Rework work. Those layers remain separate from the accepted 0.1.21 Clarity baseline.

## Compatibility

- Graveyard Keeper **1.407**
- BepInEx 5

PrayerClarity currently guards against an unverified `Assembly-CSharp` build and disables itself instead of patching an unsupported game binary.
