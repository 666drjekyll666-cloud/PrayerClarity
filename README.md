# PrayerClarity

PrayerClarity is a pair of alternative BepInEx mods for **Graveyard Keeper 1.407**. Both make prayer mechanics clearer; choose the edition that matches how you want the prayers themselves to behave.

## Editions

### PrayerClarity: Vanilla — 1.0.24

*Understand what your prayers actually do — without changing how they work.*

Keeps Graveyard Keeper's stock prayer mechanics and balance intact while improving prayer descriptions, quality comparisons, success requirements, pulpit information, item tooltips, and Character -> Temporary Effects.

[Download PrayerClarity: Vanilla 1.0.24](https://github.com/666drjekyll666-cloud/PrayerClarity/releases/tag/v1.0.24)

### PrayerClarity: Rebalanced — 0.1.5

Uses the same Clarity presentation layer, but intentionally rebalances and repairs the prayer roster so different prayers and qualities create more meaningful choices.

[Download PrayerClarity: Rebalanced 0.1.5](https://github.com/666drjekyll666-cloud/PrayerClarity/releases/tag/rebalanced-v0.1.5)

**Install one edition, not both.**

## Shared clarity features

- **Pulpit:** explains guaranteed reward sources, exact success chance, prayer-owned success bonuses, special effects, and effect duration while preserving the sermon itself as the reveal moment for the final Faith/donation payout.
- **Technology tree:** shows shared prayer properties once, then compact Bronze/Silver/Gold tier snapshots with the quality needed for 100% success and the values that actually change by tier.
- **Prayer item tooltips:** show the mechanics of the concrete prayer quality you are holding rather than repeating the full three-tier comparison.
- **Character -> Temporary Effects:** shows the actual quantitative effect of active prayer buffs and expresses long remaining durations in in-game days.
- **Localization:** PrayerClarity-owned text is included for all 11 interface languages supported by Graveyard Keeper: English, French, German, Simplified Chinese, Spanish, Brazilian Portuguese, Korean, Japanese, Russian, Italian, and Polish.

## Installation

1. Install **BepInEx 5** for Graveyard Keeper.
2. Download the DLL for the edition you want:
   - Vanilla: `PrayerClarity.dll`
   - Rebalanced: `PrayerClarity.Rebalanced.dll`
3. Put the DLL in `Graveyard Keeper/BepInEx/plugins/PrayerClarity/`.
4. When updating, replace the existing DLL. Do not keep old PrayerClarity versions beside the new one.

Neither edition requires user-facing configuration.

## Compatibility

- Graveyard Keeper **1.407**
- BepInEx 5

PrayerClarity verifies the supported `Assembly-CSharp` build before installing its patches. On an unverified game binary it disables itself instead of patching unknown code.

## License

PrayerClarity is released under the [MIT License](LICENSE).
