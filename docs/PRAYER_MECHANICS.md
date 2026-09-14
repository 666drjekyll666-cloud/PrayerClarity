# Prayer Mechanics — Graveyard Keeper 1.407

Status: initial evidence map, updated 2026-09-14.

This document records verified mechanics before any PrayerClarity production implementation. Player-facing names are intentionally not treated as interchangeable with internal IDs until localization or direct UI evidence establishes the mapping.

## Evidence basis

Primary evidence comes from read-only diagnostics captured from the user's installed Graveyard Keeper 1.407 runtime and Assembly-CSharp IL. The inspected runtime uses Assembly-CSharp 11.0.0.0. Raw proprietary game data is not committed; only minimal derived facts are retained here.

Community pages, wiki pages, guides, calculators, and public screenshots are discovery/presentation evidence. They are not used as authority for internal formulas when direct 1.407 runtime or IL evidence is available.

## Internal model

A sermon/prayer interaction spans several distinct layers:

- a prayer item, e.g. `b_faith:1`;
- a `CraftDefinition` with `craft_type = PrayCraft`, normally used at `church_pulpit`;
- the craft's `linked_sub_id`, selecting a `PrayEventDefinition` such as `default_1` or `pray_for_souls_1`;
- the `PrayEventDefinition`, supplying base `people`, `faith`, and `money` expressions;
- prayer-craft fields such as `needs_quality`, `k_faith`, `k_money`, fixed outputs, optional `buff`, and `dur_parameter`;
- downstream code that applies rewards, drops, and buffs.

Therefore `prayer item`, `PrayCraft`, `PrayEventDefinition`, `sermon`, and localized prayer names are not synonyms.

## Core calculation

### Base event values

`PrayLogics.CalculatePray` evaluates the linked event as:

- `people = max(0, RoundToInt(event.people))`
- `faith = max(0, RoundToInt(event.faith))`
- `money = max(0, event.money)`

Base `people` and `faith` are integer-rounded here. Base `money` remains floating-point.

### Success probability

For a prayer with non-zero `needs_quality`:

`success_percent = RoundToInt(current_church_quality / needs_quality * 100)`

The result is clamped to `0..100`. If `needs_quality == 0`, success is 100%.

The success draw is:

- automatically successful at 100%; otherwise
- `Random.Range(0, 100) < success_percent`.

This is a direct linear relationship between current church quality and the prayer's quality requirement.

### Prayer-specific bonuses

On success, `CalculatePray` processes the prayer craft's output list and coefficients:

- fixed output `faith` -> add its `value` to `faith_bonus`;
- fixed output `money` -> add `value / 100` to `money_bonus`;
- `k_faith != 0` -> add `RoundToInt(base_faith * k_faith)` to `faith_bonus`;
- `k_money != 0` -> add `Round(base_money * k_money * 100) / 100` to `money_bonus`;
- any other output remains in the sermon-drop list.

On a failed success check, prayer-specific `faith_bonus` and `money_bonus` are set to zero.

**Open:** the final consumer of `PrayResult` still needs a targeted static read before documenting complete failed-sermon payout semantics. What is proved now is the calculation structure above; do not infer more from the existence of the base fields alone.

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

Direct consequences:

- ordinary event-base Faith is driven by **church quality**;
- event-base money is driven by **graveyard quality**;
- Eloquence modifies event-base Faith;
- Cardinal modifies the ordinary event-base money coefficient where the event uses `0.03 + 0.01*C`;
- the Souls event family adds current `gratitude_points` to church quality before calculating base Faith.

Without Eloquence, Souls base Faith simplifies to `(CQ + GP) / 10` before integer rounding.

## Verified prayer-craft families

The table uses internal family IDs. It does not assert the current localized display name unless separately mapped. `q` = `needs_quality`; `kF` = `k_faith`; `kM` = `k_money`.

| Internal family | Linked event(s) | q, tiers 1/2/3 | kF, tiers | kM, tiers | Additional direct data | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `b_empty` | `default_0` for `pray:b_empty` | 10 | 0 | 0 | fixed Faith output on the non-suffixed craft | core craft verified; placeholder/suffixed reachability still needs classification |
| `b_faith` | `default_1/2/3` | 10 / 20 / 50 | .5 / 1 / 1.5 | .2 / .2 / .2 | fixed Faith + money outputs | verified |
| `b_money` | `default_1/2/3` | 10 / 20 / 50 | .2 / .2 / .2 | .5 / 1 / 1.5 | fixed Faith + money outputs | verified |
| `b_faith_money` | `default_1/2/3` | 15 / 30 / 60 | .5 / 1 / 1.5 | .5 / 1 / 1.5 | fixed Faith + money outputs | verified |
| `b_plant` | `default_1/2/3` | 10 / 20 / 30 | .25 / .25 / .25 | .25 / .25 / .25 | `buff_plant`; `dur_parameter` 36 / 72 / 108 | craft verified; effect scope partly traced |
| `b_sins` | `default_1/2/3` | 10 / 20 / 40 | .25 / .5 / .75 | .1 / .1 / .1 | `buff_sins`; `dur_parameter` 18 / 36 / 54 | consumer still open |
| `b_skull` | `default_1/2/3` | 20 / 40 / 50 | .1 / .1 / .1 | .25 / .5 / .75 | `buff_skull`; `dur_parameter` 18 / 36 / 54 | verified direct buff resource; localized mapping pending |
| `b_sword` | `default_1/2/3` | 10 / 20 / 40 | .25 / .25 / .25 | .25 / .25 / .25 | `buff_sword`; `dur_parameter` 36 / 72 / 108 | verified |
| `b_shield` | `default_1/2/3` | 10 / 20 / 40 | .25 / .25 / .25 | .25 / .25 / .25 | `buff_shield`; `dur_parameter` 36 / 72 / 108 | verified |
| `b_pen` | `default_1/2/3` | 10 / 40 / 60 | .25 / .5 / .75 | .1 / .1 / .1 | `buff_pen`; `dur_parameter` 18 / 36 / 54 | `craft_q` consumer open |
| `b_star` | `default_1/2/3` | **10 / 40 / 60** | .25 / .5 / .75 | .1 / .1 / .1 | `buff_star`; `dur_parameter` **18 / 36 / 54** | verified |
| `b_village` | `default_1/2/3` | 10 / 20 / 30 | .25 / .25 / .25 | .25 / .25 / .25 | item outputs present | output semantics open |
| `b_souls` | `pray_for_souls_1/2/3` | 15 / 30 / 60 | .5 / 1 / 1.5 | .25 / .25 / .25 | Souls-specific event formulas | verified player content; current localized name pending direct 1.407 localization |
| `b_grat_points_incr` | `default_1/2/3` | 10 / 20 / 30 | .25 / .25 / .25 | .25 / .25 / .25 | `buff_gp_increase`; `dur_parameter` 36 / 72 / 108 | verified player content; gratitude-gain consumer open |
| `b_sin_shard` | `default_1/2/3` | 10 / 20 / 30 | .25 / .25 / .25 | .25 / .25 / .25 | `buff_sin_shard`; `dur_parameter` 36 / 72 / 108 | verified player content; effect magnitude traced |

The Better Save Soul technology definition `soul_church_additions` directly unlocks `b_souls`, `b_grat_points_incr`, and `b_sin_shard`. This proves these three families are reachable player content, rather than merely orphan rows in the runtime registry.

### Other `PrayCraft` rows are not yet accepted as player-visible prayers

Runtime data also contains families such as `b_ghost`, `b_energy`, `b_random`, `b_techpoint_blue`, `b_techpoint_green`, `b_techpoint_red`, `b_circle`, and `b_cross`.

Some have zero quality requirement, zero Faith/money coefficients, generic/empty linked event data, or other signs of legacy/development/placeholder use. They remain **unclassified internal data** until item definitions, unlock paths, localization, and UI reachability prove player use.

## Verified buff mechanics

| Buff ID | Direct data | Base length expression | Verified gameplay meaning |
| --- | --- | --- | --- |
| `buff_plant` | `buff_plant = 1` | `36 * (1 + 0.3*buff_longtimer)` | inspected growing crafts include a `-0.2 * WGOpar("buff_plant")` time term; effect scope still needs complete enumeration |
| `buff_sins` | `buff_sins = 1` | same | flag/duration definition verified; consumer open |
| `buff_skull` | `body_max = 1` | same | +1 `body_max` resource while active |
| `buff_sword` | `add_damage = 5` | same | +5 damage resource while active |
| `buff_shield` | `add_armor = 4` | same | +4 armor resource while active |
| `buff_pen` | `craft_q = 0.7` | same | quality parameter verified; consumer open |
| `buff_star` | `craft_q = 0.2` | same | quality parameter verified; consumer open |
| `buff_sin_shard` | `increase_sin_shard_drop = 1` | same | **direct consumer traced: doubles base Sin Shard output during soul healing** |
| `buff_gp_increase` | `increase_gp_gain = 1` | same | flag verified; exact gratitude-gain arithmetic open |

### `buff_sin_shard` exact effect

The soul-healing consumer starts from the healed soul's base `sins_count`, then adds:

`increase_sin_shard_drop * sins_count`

`buff_sin_shard` sets `increase_sin_shard_drop = 1`, therefore while the buff is active:

`Sin Shards awarded = sins_count + sins_count = 2 * sins_count`

This is a direct 1.407 code result, not a wiki inference.

### Buff-duration caution

Prayer crafts also contain tier-dependent `dur_parameter` values. The exact application path combining `dur_parameter` with `BuffDefinition.length` has not yet been traced. Do not publish real-time prayer-buff durations from either field in isolation.

## Presentation evidence

### Prayer item description exposes the required church quality

Current 1.407 `ItemDefinition.GetDescription` contains a `Preach` branch. For sermon items it appends localization key `preach_params`, passing:

`"(cross)" + linked_craft.needs_quality`

Therefore the prayer description directly includes the prayer's required church-quality value in some localized format. Runtime prayer item definitions inspected so far have empty optional `q_hint`, so this requirement line is not coming from the generic quality-hint field.

### Prayer-selection UI knows and presents success state

Current 1.407 `PrayCraftGUI`:

1. computes `ratio = current_church_quality / needs_quality`;
2. caps that ratio at 1;
3. calls `RedrawTextValues(needs_quality, ratio)`;
4. switches the action label to `btn_try_pray` below the requirement and `btn_pray` when the requirement is met.

A public Preaching-screen capture from the same UI family visibly contains `Church quality`, `Sermon needs`, and `Success chance`. Combined with the current 1.407 call path, the working conclusion is that **success probability is not a good candidate for PrayerClarity's hidden-information problem**.

Exact current Russian/English localization strings and the body of `RedrawTextValues` are still worth obtaining before marking every presentation detail as direct 1.407 fact.

### What is not yet proved to be previewed

Nothing found so far proves that the pre-use UI gives the player a current-state numerical forecast for:

- final Faith;
- final total donations;
- Souls-prayer Faith after current Soul Gratitude;
- exact passive-buff magnitude/duration in a consistent quantitative form.

This absence is an audit question, not yet a final UX finding.

## Secondary localization mapping

A public localization dump provides useful discovery mappings such as `b_faith` -> `Prayer for faith`, `b_money` -> `Prayer for donations`, `b_pen` -> `Prayer for imagination`, and `b_star` -> `Prayer for excellence`. It is not accepted as the canonical current 1.407 localization source, especially for DLC prayers that are absent from that dump.

## Open questions for the next static pass

1. Map all accepted internal families to exact current 1.407 English/Russian names and descriptions.
2. Obtain the exact `PrayCraftGUI.RedrawTextValues` body/current localization to close the pre-use UI audit.
3. Trace the final `PrayResult` reward/application consumer, especially failed-sermon payout semantics.
4. Trace consumers of `buff_sins`, `buff_pen.craft_q`, `buff_star.craft_q`, and `increase_gp_gain`.
5. Enumerate and classify `b_village` outputs.
6. Classify extra `PrayCraft` families as reachable content versus legacy/internal rows.
7. Audit any rounding/caps after `CalculatePray`, including final displayed Faith/money rounding.
8. Trace prayer-buff application to determine how `dur_parameter` and `BuffDefinition.length` combine.
9. Audit the current buff-hover/result-screen presentation after the prayer is used.

No production DLL, build infrastructure, hosted CI, or user runtime test is justified yet. The remaining high-value questions should still be exhausted through static evidence first.