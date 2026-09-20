# Changelog

## PrayerClarity: Vanilla 1.0.32 candidate

- Refines Technology prayer hierarchy without changing stock Graveyard Keeper 1.407 mechanics.
- Vanilla Technology uses the condition-style header **On success:** / **При успехе:** instead of **Bonuses on success**, making the heading apply naturally to the whole success-only block.
- When a stock prayer has both a shared Faith/donation rider and a shared named effect, those two lines are kept together without a blank gap; tier blocks remain separated below.
- Prayer-item tooltips keep their existing **Bonuses on success** heading and accepted spacing.
- Rebalanced presentation is behaviorally unchanged: its Technology provider keeps the existing Rebalanced heading/grouping.

## PrayerClarity: Rebalanced 0.2.13

- Accepted stable release preserving the 0.2.12 Technology repair and all 0.2.11 specialist/resource-balance changes.
- Soul Contentment now shows its +20% gain with the native Soul Gratitude icon instead of the ambiguous generic word “gratitude”.
- BSS Soul's Repose replaces the redundant shared Technology sentence with a direct scaling hint: more Soul Gratitude means a larger Faith bonus.
- No prayer mechanics, requirements, payouts, durations, stacking, HUD behavior, or shared Vanilla presentation change.

## PrayerClarity: Rebalanced 0.2.12 candidate

- Preserves all 0.2.11 specialist-purity and resource-balance changes.
- Fixes the Technology tooltip regression exposed when a specialist has no generic Faith/money success contribution: PrayerClarity now replaces the stock requirement/lore row directly and inserts **Base result** / **Bonuses on success** before the crafting-location footer even when vanilla omits its `preach_params_2` block.
- Removes the stale stock **+10%** Soul Contentment Technology sentence in Rebalanced, where the effective prayer is **+20%**; the current effect is shown only from Rebalanced mechanics data.
- Prayer-item tooltips, pulpit/HUD, Temporary Effects, balance values, durations and specialist mechanics are unchanged from 0.2.11.

## PrayerClarity: Rebalanced 0.2.11 candidate

- Purifies specialist prayers: non-resource utility prayers no longer carry unrelated prayer-owned Faith/donation percentage riders.
- Removes unrelated fixed Faith/money success outputs from those specialist prayers while preserving their named effects and physical specialist rewards.
- Retunes Prayer for Donations to flat success rewards of **+5 / +15 / +30 silver** at Bronze/Silver/Gold.
- Retunes Combo Prayer donations to **+100 / +200 / +300%** while keeping Combo Faith at **+100 / +150 / +200%**.
- Keeps Prayer for Faith, Prayer for Donations, Combo Prayer, BSS Prayer for Soul's Repose, and Casual Prayer as explicit resource/starter exceptions.
- No changes to accepted specialist effect magnitudes, durations, stacking rules, Repose corpse-quality behavior, Roots cap, or shared Vanilla presentation.

## PrayerClarity: Vanilla 1.0.31 / Rebalanced 0.2.10

- Accepted shared release: replaces the visually ineffective blank-separator spacing attempt from 1.0.30 / 0.2.9 with a controlled leading text line on PrayerClarity-owned **Base result** and **Bonuses on success** title rows in prayer-item tooltips only.
- Keeps Technology comparison tooltips unchanged and vertically compact.
- Preserves the accepted 1.0.30 / 0.2.9 Repose q20/q40/q60 requirement, Prayer for Excellence stock lore fallback, compact Base-result grammar, and all previously accepted mechanics/presentation behavior.
- No prayer mechanics, balance values, durations, stacking rules, payouts, or HUD behavior change in this candidate.

## Shared candidate — Vanilla 1.0.30 / Rebalanced 0.2.9

- Adds native blank spacing before **Base result** in single-prayer item tooltips; Technology comparison tooltips remain vertically compact.
- Restores the stock Prayer for Excellence lore on crafting surfaces when Graveyard Keeper's multi-quality output path omits it, reusing the game's own localized `b_star_d` text rather than adding new copy.
- Rebalanced Prayer for Repose changes only the Gold guaranteed-success Church Quality requirement from **50 to 60**; Bronze/Silver remain 20/40 and the accepted corpse-quality behavior plus 30/42/54-minute durations are unchanged.
- Preserves all runtime-accepted 1.0.29 / 0.2.8 Clarity presentation, including compact Base result parentheses and the Roots 95% cap only on the active Temporary Effects surface.

## Shared candidate — Vanilla 1.0.29 / Rebalanced 0.2.8

- Keeps the runtime-accepted 1.0.28 / 0.2.7 tooltip structure and compact reward rows.
- Shortens Base result dependency wording to parenthetical source labels, e.g. `Faith (Church Quality)` and `Donations (Graveyard Quality)`; Soul's Repose keeps Soul Gratitude in the Faith source.
- Rebalanced Shoots & Roots prayer-selection surfaces now show only the direct tier effect (-20/-30/-40% growth time); the existing 95% combined safety cap remains mechanically unchanged and stays visible on the active Temporary Effects surface.
- Leaves the accepted HUD decimal punctuation behavior unchanged; no heavier punctuation-only workaround is added.
- No prayer balance values, stacking rules, duration rules, or runtime mechanics are changed.

## Shared candidate — Vanilla 1.0.28 / Rebalanced 0.2.7

- Simplifies Faith/donation percentage rows to the resource plus signed percentage, removing the redundant “of base value” suffix.
- Renders single-item prayer rewards as one atomic `localized name ×N` row instead of a separate Quantity line.
- Reuses the existing PrayerClarity-owned content-width mechanism for prayer-item success rows so the 100% success threshold stays visually intact.
- Refreshes the game locale once on the first long-prayer HUD timer render, covering BepInEx initialization before Graveyard Keeper loads GameSettings without adding per-frame language polling.
- Preserves all prayer mechanics and the accepted Base result / Bonuses on success structure; runtime acceptance is still required.

## PrayerClarity: Rebalanced 0.2.4

- Increased Repentance duration from 18 / 36 / 54 to **30 / 42 / 54 minutes** while keeping the accepted 50 / 75 / 100% daily confession probabilities.
- Increased Repose duration from 18 / 36 / 54 to **30 / 42 / 54 minutes** while keeping the accepted Bronze/Silver/Gold corpse-quality behavior unchanged.
- The change targets discrete roughly once-per-day confession and Donkey-delivery opportunities: Bronze now has a credible multi-day window, Silver keeps a distinct intermediate duration step, and Gold remains unchanged.
- No new runtime hooks or polling were added; duration is projected through the existing prayer craft data at the established load-time projection seam.

## PrayerClarity: Rebalanced 0.2.3

- Preserved the accepted Rebalanced prayer balance while closing the remaining runtime architecture audit.
- Shoots & Roots keeps the stock growth expression authoritative and uses the accepted 95% combined growth-time reduction cap.
- Repentance now leaves the stock daily 15% reset and confession RNG flow intact, projecting only the effective 50 / 75 / 100% probability through the native FlowCanvas player-param accessor while the prayer is active.
- Combat damage now follows the native `add_damage` path: Bronze keeps stock +5, Silver/Gold add only the scoped +5 / +10 tier delta during native damage calculation.
- Combat armor now follows native `add_armor` handling through a scoped nonserialized +4 projection during `DecHP`, removing the previous ThreadStatic/global `GetParam` interception.
- Combat regeneration remains on the game's native buff `se_tick` extension point and was runtime-verified at +1 / +2 / +4 HP per second.
- Runtime verification confirmed Repentance 0.50 / 0.75 / 1.00, Combat damage deltas 0 / +5 / +10, exactly +4 armor, and tiered regeneration with no PrayerClarity runtime error.
- Research Test Console diagnostics remain development-only and are not shipped in the release.

## PrayerClarity: Vanilla 1.0.25

- Added the current Soul Gratitude value to the pulpit context for Soul's Repose.
- Added contextual Repose endpoint wording when stock +1 corpse-tier maximum no longer opens a higher ordinary body tier.
- Kept all prayer mechanics, requirements, rewards, formulas and balance stock Graveyard Keeper 1.407.
- Preserved synchronized Clarity presentation and all 11 supported interface languages.

## PrayerClarity: Rebalanced 0.2.0

- Reworked the main resource family into clearer roles:
  - Faith: flat-only +5 / +10 / +20 Faith, q20 / 40 / 60.
  - Donations: flat-only +5 / +10 / +15 silver, q20 / 40 / 60.
  - Combo: percentage-only +100 / +150 / +200% Faith and donations, q40 / 60 / 80.
- Revised Church Quality ladders across Repentance, Roots, Combat, Imagination, Excellence and Better Save Soul prayers.
- Set BSS Soul's Repose to q30 / 60 / 120 with +50 / +100 / +150% Faith on the verified Soul Gratitude base.
- Added current Soul Gratitude to the Soul's Repose pulpit context.
- Fixed Rebalanced Repose to narrow toward the highest actually existing eligible corpse tier instead of blindly using the raw numeric maximum.
- Added contextual Repose endpoint wording for states where stock/Bronze Repose cannot open a higher ordinary corpse tier.
- Kept Imagination's +0.7 writing-quality effect and its 3 Silver / 3 Gold Story success rewards through the stock sermon-drop pipeline.
- Preserved stock prayer durations and all 11 supported interface languages.

## PrayerClarity: Vanilla 1.0.24

- Refined Technology prayer tooltips around a shared-effect -> tier-specific-value hierarchy.
- Improved semantic emphasis, wrapping, atomic rows, and content-driven width for clearer Bronze/Silver/Gold comparison.
- Removed the redundant shared Prosperity effect line while preserving the tier-specific Commercial Blessing x1/x2/x3 rewards.
- Kept prayer-item, pulpit, and Temporary Effects wording synchronized with the same mechanics semantics.
- Preserved stock Graveyard Keeper 1.407 prayer mechanics and balance.

## PrayerClarity: Rebalanced 0.1.5

- First accepted Rebalanced sibling release.
- Includes the full PrayerClarity presentation layer plus intentional prayer rebalance/rework.
- Uses the accepted specialist Faith/Donations/Soul's Repose progression of +200% / +300% / +400%.
- Rebalances Repentance, Shoots & Roots, Repose, Combat, Imagination, Excellence, Soul Contentment, and Thorough Cleansing with tier-aware effects and durations.
- Synchronizes Technology, prayer-item, pulpit, and Temporary Effects presentation with the effective Rebalanced mechanics.
- Keeps full localization coverage for all 11 supported interface languages.


## 1.0.20

- Added a clear vertical gap before the crafting-location footer in prayer Technology tooltips.
- Replaced punctuation-sensitive parsing of vanilla prayer requirement text with direct localized vanilla lore lookup.
- Fixed the Polish Prayer for Repose Technology tooltip retaining the stock `20–50` requirement text before its description.
- Preserved the existing Clarity-only behavior: no prayer balance or gameplay mechanics changes.

## 1.0.18

- Reworked prayer Technology tooltips into compact shared-details + Bronze/Silver/Gold tier snapshots.
- Added content-driven tooltip width and viewport safety so important tier rows stay readable without long effect text making every tooltip excessively wide.
- Clarified prayer-item tooltips for the concrete quality being inspected.
- Improved active prayer-effect descriptions and long-duration display.
- Refined pulpit information hierarchy while preserving the final sermon payout as an in-sermon reveal.
- Improved wording, spacing, wrapping, and localization across all 11 supported interface languages.
- Removed redundant non-prayer `linked_craft` lookups from prayer-item and Technology discovery paths.
- No prayer balance or gameplay mechanics changes.

## 1.0.1

- Extended Clarity presentation to prayer item tooltips.
- Preserved the existing Clarity-only pulpit, Technology, and Temporary Effects behavior.

## 1.0.0

- First stable Clarity release.
- Added prayer information improvements at the pulpit, in Technology tooltips, and in Character -> Temporary Effects.
- Added all 11 supported interface languages.
