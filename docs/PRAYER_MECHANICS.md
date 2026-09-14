# Prayer Mechanics — Graveyard Keeper 1.407

Status: initial evidence map, 2026-09-14.

This document records verified mechanics before any PrayerClarity production implementation. Player-facing names are intentionally not treated as interchangeable with internal IDs until localization or direct UI evidence establishes the mapping.

## Evidence basis

Primary evidence for this first pass comes from read-only diagnostics previously captured from the user's installed Graveyard Keeper 1.407 runtime and Assembly-CSharp IL. The inspected runtime uses Assembly-CSharp 11.0.0.0. Those raw game-derived diagnostics are not committed to this repository; only minimal derived facts are recorded here.

Community pages, wiki pages, guides, and calculators are discovery/UX sources. They are not used as authority for internal formulas when direct runtime or IL evidence is available.

## Internal model

A player prayer/sermon interaction is represented by several distinct layers:

- a prayer item (for example an item ID such as `b_faith:1`);
- a `CraftDefinition` with `craft_type = PrayCraft`, generally selected at `church_pulpit`;
- the craft's `linked_sub_id`, which can select a `PrayEventDefinition` such as `default_1` or `pray_for_souls_1`;
- the `PrayEventDefinition`, which supplies base `people`, `faith`, and `money` expressions;
- prayer-craft parameters such as `needs_quality`, `k_faith`, `k_money`, fixed craft outputs, optional `buff`, and `dur_parameter`;
- downstream application code that awards the calculated result and applies any buff/drop behavior.

Therefore `prayer item`, `PrayCraft`, `PrayEventDefinition`, `sermon`, and localized prayer names are not synonyms.

## Core calculation

### Base event values

`PrayLogics.CalculatePray` evaluates the linked event and normalizes the three base values as follows:

- `people = max(0, RoundToInt(event.people))`
- `faith = max(0, RoundToInt(event.faith))`
- `money = max(0, event.money)`

`people` and base `faith` therefore use integer rounding at this stage. Base `money` remains floating-point.

### Success probability

For a prayer craft with non-zero `needs_quality`:

`success_percent = RoundToInt(current_church_quality / needs_quality * 100)`

The result is clamped to `0..100`.

If `needs_quality == 0`, success is treated as 100%.

The success draw is:

- automatically successful at 100%; otherwise
- `Random.Range(0, 100) < success_percent`.

This establishes a direct linear church-quality threshold for sermon success. It does **not** yet establish exactly how the current 1.407 UI presents that percentage; presentation is a separate audit.

### Prayer-specific bonuses

On success, `CalculatePray` processes fixed `CraftDefinition.output` entries and prayer multipliers:

- an output `faith` item contributes its value to `faith_bonus`;
- an output `money` item contributes `value / 100` to `money_bonus`;
- if `k_faith != 0`, add `RoundToInt(base_faith * k_faith)` to `faith_bonus`;
- if `k_money != 0`, add `Round(base_money * k_money * 100) / 100` to `money_bonus`;
- non-faith/non-money outputs remain sermon drops.

On failed success-check, the prayer-specific `faith_bonus` and `money_bonus` are zeroed.

**Open:** the downstream payout/application path still needs a targeted read before stating the complete failure payout semantics. The safe current statement is that the calculation result retains separately calculated event-base values while prayer-specific bonuses are zeroed on failure.

## Verified `PrayEventDefinition` catalogue

Notation:

- `CQ = GetZoneQ("church")`
- `GQ = GetZoneQ("graveyard")`
- `E = Ppar("p_eloquence")`
- `C = Ppar("p_cardinal")`
- `GP = Ppar("gratitude_points")`

| Event ID | Faith expression | Money expression | People expression |
| --- | --- | --- | --- |
| `default` | `CQ * 0.4 * (1 + 0.3*E)` | `GQ * 0.06` | `CQ * 0.5` |
| `default_0` | `CQ * 0.2 * (1 + 0.3*E)` | `GQ * (0.03 + 0.01*C)` | `CQ * 1` |
| `default_1` | `CQ * 0.2 * (1 + 0.3*E)` | `GQ * (0.03 + 0.01*C)` | `CQ * 0.5` |
| `default_2` | `CQ * 0.2 * (1 + 0.3*E)` | `GQ * (0.03 + 0.01*C)` | `CQ * 0.34` |
| `default_3` | `CQ * 0.2 * (1 + 0.3*E)` | `GQ * (0.03 + 0.01*C)` | `CQ * 0.34` |
| `pray_for_souls_1` | `(CQ + GP) * 0.1 * (1 + 0.3*E)` | `GQ * (0.03 + 0.01*C)` | `CQ * 0.5` |
| `pray_for_souls_2` | `(CQ + GP) * 0.1 * (1 + 0.3*E)` | `GQ * (0.03 + 0.01*C)` | `CQ * 0.34` |
| `pray_for_souls_3` | `(CQ + GP) * 0.1 * (1 + 0.3*E)` | `GQ * (0.03 + 0.01*C)` | `CQ * 0.34` |

Immediate consequences that are directly supported by the expressions:

- ordinary event-base Faith is driven by **church quality**;
- event-base money is driven by **graveyard quality**;
- Eloquence modifies event-base Faith;
- Cardinal modifies the normal event-base money coefficient where the event uses the `0.03 + 0.01*C` form;
- the Souls event family adds current `gratitude_points` to church quality before calculating base Faith.

Without Eloquence, the Souls base-Faith expression simplifies to `(CQ + GP) / 10`.

## Verified prayer-craft families

The table below uses **internal family IDs**, not asserted player-facing English/Russian names. `q` is `needs_quality`; `kF` is `k_faith`; `kM` is `k_money`. Values are listed as quality tier 1 / 2 / 3 where the family has three tiers.

| Internal family | Linked event(s) | q | kF | kM | Additional direct data | Status |
| --- | --- | ---: | ---: | ---: | --- | --- |
| `b_empty` | `default_0` for `pray:b_empty` | 10 | 0 | 0 | fixed Faith output exists on the non-suffixed craft | core craft verified; suffixed placeholder variants require reachability audit |
| `b_faith` | `default_1/2/3` | 10 / 20 / 50 | 0.5 / 1 / 1.5 | 0.2 / 0.2 / 0.2 | fixed Faith + money outputs | verified |
| `b_money` | `default_1/2/3` | 10 / 20 / 50 | 0.2 / 0.2 / 0.2 | 0.5 / 1 / 1.5 | fixed Faith + money outputs | verified |
| `b_faith_money` | `default_1/2/3` | 15 / 30 / 60 | 0.5 / 1 / 1.5 | 0.5 / 1 / 1.5 | fixed Faith + money outputs | verified |
| `b_plant` | `default_1/2/3` | 10 / 20 / 30 | 0.25 / 0.25 / 0.25 | 0.25 / 0.25 / 0.25 | `buff_plant`; `dur_parameter` 36 / 72 / 108 | verified craft; effect scope still being enumerated |
| `b_sins` | `default_1/2/3` | 10 / 20 / 40 | 0.25 / 0.5 / 0.75 | 0.1 / 0.1 / 0.1 | `buff_sins`; duration parameter 18 / 36 / 54 | craft verified; consumer of `buff_sins` still open |
| `b_skull` | `default_1/2/3` | 20 / 40 / 50 | 0.1 / 0.1 / 0.1 | 0.25 / 0.5 / 0.75 | `buff_skull`; duration parameter 18 / 36 / 54 | verified craft and direct buff resource, exact player-facing mapping pending |
| `b_sword` | `default_1/2/3` | 10 / 20 / 40 | 0.25 / 0.25 / 0.25 | 0.25 / 0.25 / 0.25 | `buff_sword`; duration parameter 36 / 72 / 108 | verified |
| `b_shield` | `default_1/2/3` | 10 / 20 / 40 | 0.25 / 0.25 / 0.25 | 0.25 / 0.25 / 0.25 | `buff_shield`; duration parameter 36 / 72 / 108 | verified |
| `b_pen` | `default_1/2/3` | 10 / 40 / 60 | 0.25 / 0.5 / 0.75 | 0.1 / 0.1 / 0.1 | `buff_pen`; duration parameter 18 / 36 / 54 | craft verified; `craft_q` consumer still open |
| `b_star` | `default_1/2/3` | tiered; tier 3 = 60 | 0.25 / 0.5 / 0.75 | 0.1 / 0.1 / 0.1 | `buff_star`; duration parameter 18 / 36 / 54 | partial quality-threshold table pending exact re-read |
| `b_village` | `default_1/2/3` | 10 / 20 / 30 | 0.25 / 0.25 / 0.25 | 0.25 / 0.25 / 0.25 | item outputs present | output semantics still open |
| `b_souls` | `pray_for_souls_1/2/3` | 15 / 30 / 60 | 0.5 / 1 / 1.5 | 0.25 / 0.25 / 0.25 | Souls-specific event formulas | verified internal family; localization mapping pending |
| `b_grat_points_incr` | `default_1/2/3` | 10 / 20 / 30 | 0.25 / 0.25 / 0.25 | 0.25 / 0.25 / 0.25 | `buff_gp_increase`; duration parameter 36 / 72 / 108 | craft and flag verified; consumer magnitude still open |
| `b_sin_shard` | `default_1/2/3` | 10 / 20 / 30 | 0.25 / 0.25 / 0.25 | 0.25 / 0.25 / 0.25 | `buff_sin_shard`; duration parameter 36 / 72 / 108 | craft and flag verified; consumer magnitude still open |

### Other `PrayCraft` rows are not yet accepted as player-visible prayers

Runtime data also contains families such as `b_ghost`, `b_energy`, `b_random`, `b_techpoint_blue`, `b_techpoint_green`, `b_techpoint_red`, `b_circle`, and `b_cross`.

Some have zero quality requirement, zero Faith/money coefficients, generic/empty linked event data, or other signs that they may be legacy, development, placeholder, or indirectly used rows. They remain **unclassified internal data** until item definitions, unlock paths, localization, and UI reachability prove that the player can actually select/use them as prayers.

## Verified buff data

All of these are runtime `BuffDefinition` facts. A buff flag is not automatically the same thing as a complete gameplay effect; its consumers are audited separately.

| Buff ID | Direct resource/effect data | Base length expression | What is already proved |
| --- | --- | --- | --- |
| `buff_plant` | `buff_plant = 1` | `36 * (1 + 0.3*buff_longtimer)` | growing-craft expressions exist that subtract `0.2 * WGOpar("buff_plant")`, proving a 20% time term on those inspected crafts |
| `buff_sins` | `buff_sins = 1` | `36 * (1 + 0.3*buff_longtimer)` | flag and duration verified; downstream mechanic still open |
| `buff_skull` | `body_max = 1` | `36 * (1 + 0.3*buff_longtimer)` | direct +1 `body_max` resource while active |
| `buff_sword` | `add_damage = 5` | `36 * (1 + 0.3*buff_longtimer)` | direct +5 damage resource while active |
| `buff_shield` | `add_armor = 4` | `36 * (1 + 0.3*buff_longtimer)` | direct +4 armor resource while active |
| `buff_pen` | `craft_q = 0.7`, no direct resource | `36 * (1 + 0.3*buff_longtimer)` | quality parameter exists; exact consuming code still open |
| `buff_star` | `craft_q = 0.2`, no direct resource | `36 * (1 + 0.3*buff_longtimer)` | quality parameter exists; exact consuming code still open |
| `buff_sin_shard` | `increase_sin_shard_drop = 1` | `36 * (1 + 0.3*buff_longtimer)` | flag and duration verified; exact drop arithmetic still open |
| `buff_gp_increase` | `increase_gp_gain = 1` | `36 * (1 + 0.3*buff_longtimer)` | flag and duration verified; exact gratitude-gain arithmetic still open |

A separate parameter, `dur_parameter`, varies by prayer quality in the craft rows. How it combines with the `BuffDefinition.length` expression and any prayer-specific application logic still needs a targeted calling-code read before documenting final real-time durations.

## Presentation evidence already located

`PrayCraftGUI` compares current church quality with the selected craft's `needs_quality`, caps the ratio at 1, and forwards both `needs_quality` and the ratio to `RedrawTextValues(...)`.

The action button switches localization key based on that ratio:

- below requirement: `btn_try_pray`
- at/above requirement: `btn_pray`

This proves that the game distinguishes a risky attempt from a guaranteed prayer in the selection UI. It does **not** yet prove whether the exact numerical success percentage is visible, because the body of `RedrawTextValues` and the relevant localization/resources still need direct inspection.

## Open questions for the next static pass

1. Map every accepted internal prayer family to exact current English/Russian localization names and descriptions.
2. Inspect `PrayCraftGUI.RedrawTextValues` and related UI/localization fields to record exactly what the player sees before selection.
3. Trace the final `PrayResult` payout/application path, especially failure semantics.
4. Trace consumers of `buff_sins`, `buff_pen.craft_q`, `buff_star.craft_q`, `increase_gp_gain`, and `increase_sin_shard_drop`.
5. Enumerate and classify `b_village` outputs.
6. Resolve exact tier-1/tier-2 quality thresholds for `b_star` from direct rows rather than secondary material.
7. Determine which extra `PrayCraft` families are reachable player content versus legacy/internal rows.
8. Audit any rounding/caps after `CalculatePray`, including Faith/money display rounding.
9. Determine whether `dur_parameter` is the effective sermon-buff duration input and how it interacts with `BuffDefinition.length`.

No new runtime probe or user in-game test is justified yet: all of these questions should be exhausted through existing 1.407 evidence and targeted static inspection first.