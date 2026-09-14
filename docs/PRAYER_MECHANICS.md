# Prayer Mechanics — Graveyard Keeper 1.407

Status: evidence map, updated 2026-09-14 after targeted read-only probe 0.1.0.

This document records verified mechanics before any PrayerClarity production implementation. Player-facing names are intentionally not treated as interchangeable with internal IDs until localization or direct UI evidence establishes the mapping.

## Evidence basis

Primary evidence comes from read-only diagnostics captured from the user's installed Graveyard Keeper 1.407 runtime and Assembly-CSharp IL. The inspected runtime uses Assembly-CSharp 11.0.0.0. Targeted probe 0.1.0 identified module MVID `6f50b8e7-156b-49ac-bbe8-7505894b2364`. Raw proprietary game data is not committed; only minimal derived facts are retained here.

Community pages, wiki pages, guides, calculators, and public screenshots are discovery/presentation evidence. They are not used as authority for internal formulas when direct 1.407 runtime or IL evidence is available.

## Internal model

A sermon/prayer interaction spans several distinct layers:

- a prayer item, e.g. `b_faith:1`;
- a `CraftDefinition` with `craft_type = PrayCraft`, normally used at `church_pulpit`;
- the craft's `linked_sub_id`, selecting a `PrayEventDefinition` such as `default_1` or `pray_for_souls_1`;
- the `PrayEventDefinition`, supplying base `people`, `faith`, and `money` expressions;
- prayer-craft fields such as `needs_quality`, `k_faith`, `k_money`, fixed outputs, optional `buff`, and `dur_parameter`;
- downstream FlowCanvas nodes, visitor-distribution functions, item-drop logic, animation code, and buff logic.

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

### Downstream PrayResult shape

The direct 1.407 FlowCanvas callback for `Flow_CalculatePrayEvent` maps the result as follows:

- flow `people` = `PrayResult.people`;
- flow `faith` = `PrayResult.faith`;
- flow `faith_bonus` = `PrayResult.faith_bonus`;
- flow `money` = `PrayResult.money + PrayResult.money_bonus`;
- flow `success` = `PrayResult.success`.

Therefore the game's downstream sermon graph receives **total money already combined**, while base Faith and bonus Faith remain separate values at this boundary.

**Still open:** the exact FlowCanvas wiring that finally calls visitor Faith/money distribution must be traced before documenting complete failed-sermon payout semantics or claiming the final Faith total path.

## Visitor distribution and physical drops

### Faith distribution primitive

`PrayLogics.SpreadFaithIncome(prayers, faith)`:

1. removes null visitor references from the supplied list;
2. repeats exactly `faith` times;
3. each iteration chooses one random visitor from the list;
4. increments that visitor's `_faith` parameter by 1.

Thus the input is an integer count of Faith units, each assigned individually to a random attending visitor.

### Money distribution primitive

`PrayLogics.SpreadMoneyIncome(prayers, money, success_percent)` builds a selected visitor list, then:

- calculates an equal per-selected-visitor share rounded down to whole cents: `floor(money * 100 / selected_count) / 100`;
- gives that amount through each selected visitor's `_money` parameter;
- distributes remaining cents one at a time to random selected visitors until the accumulated amount reaches the requested `money`.

The caller and exact meaning/range of the method's `success_percent` argument are still open. Do not infer payout probability semantics from this helper in isolation.

### Non-money/non-Faith prayer outputs

`PrayLogics.DropPrayItems()` takes the remaining `_sermon_drops` list and creates every unit as an individual world drop at `faith_drop_point`, staggered by 0.2 seconds.

This proves that prayer outputs other than the special `faith` and `money` rows can be concrete physical sermon rewards rather than merely metadata.

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
| `b_star` | `default_1/2/3` | **10 / 40 / 60** | .25 / .5 / .75 | .1 / .1 / .1 | `buff_star`; `dur_parameter` **18 / 36 / 54** | `craft_q` value verified; consumer open |
| `b_village` | `default_1/2/3` | 10 / 20 / 30 | .25 / .25 / .25 | .25 / .25 / .25 | fixed Faith 1/2/3; money 1/2/3 silver; `blessing_commerce` x1/x2/x3 | outputs verified; downstream use of blessing item still open |
| `b_souls` | `pray_for_souls_1/2/3` | 15 / 30 / 60 | .5 / 1 / 1.5 | .25 / .25 / .25 | Souls-specific event formulas | verified player content; current localized name pending direct 1.407 localization |
| `b_grat_points_incr` | `default_1/2/3` | 10 / 20 / 30 | .25 / .25 / .25 | .25 / .25 / .25 | `buff_gp_increase`; `dur_parameter` 36 / 72 / 108 | verified player content; gratitude-gain consumer open |
| `b_sin_shard` | `default_1/2/3` | 10 / 20 / 30 | .25 / .25 / .25 | .25 / .25 / .25 | `buff_sin_shard`; `dur_parameter` 36 / 72 / 108 | verified player content; effect magnitude traced |

The Better Save Soul technology definition `soul_church_additions` directly unlocks `b_souls`, `b_grat_points_incr`, and `b_sin_shard`. This proves these three families are reachable player content, rather than merely orphan rows in the runtime registry.

### `b_village` direct outputs

Direct runtime craft data gives:

- tier 1: fixed `faith x1`, `money x100` (= 1 silver before the special conversion), `blessing_commerce x1`;
- tier 2: fixed `faith x2`, `money x200` (= 2 silver), `blessing_commerce x2`;
- tier 3: fixed `faith x3`, `money x300` (= 3 silver), `blessing_commerce x3`.

`CalculatePray` removes the Faith/money rows into the corresponding bonus fields on success. `blessing_commerce` is not special-cased, so it remains in `_sermon_drops` and is eligible for physical dropping by `DropPrayItems()`.

The `blessing_commerce` item definition itself is non-usable (`can_be_used = false`), stack size 5, product type `blessing`. Runtime object indexing associates it with the pulpit object group. Its later insertion/use mechanics are still open.

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
| `buff_pen` | `craft_q = 0.7` | same | quality parameter verified; generic accessor traced, consumer open |
| `buff_star` | `craft_q = 0.2` | same | quality parameter verified; generic accessor traced, consumer open |
| `buff_sin_shard` | `increase_sin_shard_drop = 1` | same | **direct consumer traced: doubles base Sin Shard output during soul healing** |
| `buff_gp_increase` | `increase_gp_gain = 1` | same | flag verified; exact gratitude-gain arithmetic open |

### Generic craft-quality buff accessor

`CraftDefinition.GetBuffValue(buff_id)` returns:

- `0` when the named buff is not currently active;
- otherwise that buff definition's `craft_q` value.

Thus `buff_pen = 0.7` and `buff_star = 0.2` are intentionally exposed through a generic craft-quality accessor. The callers that give those two values their concrete gameplay meaning still need direct tracing.

### `buff_sin_shard` exact effect

The soul-healing consumer starts from the healed soul's base `sins_count`, then adds:

`increase_sin_shard_drop * sins_count`

`buff_sin_shard` sets `increase_sin_shard_drop = 1`, therefore while the buff is active:

`Sin Shards awarded = sins_count + sins_count = 2 * sins_count`

This is a direct 1.407 code result, not a wiki inference.

### Prayer-buff application path

Direct 1.407 IL now proves these steps:

1. `PlayerComponent.StartPrayAnimation(pray_craft, success)` resolves `pray_craft.buff` to a `BuffDefinition` and stores both the craft and success state.
2. `PlayerComponent.CreatePrayBuffFlyingObject(pos)` passes that buff to `FlyingObject.CreateBuffFlyingObject`.
3. If `pray_craft.dur_parameter` is non-zero, it is passed as an explicit nullable duration; otherwise no override is supplied.
4. `BuffsLogics.AddBuff(buff_id, length)` uses the explicit `length` when provided, otherwise evaluates `BuffDefinition.length`.
5. `AddBuff` converts the chosen length by `length / 450 * 60` into its internal game-time delta and sets/extends the saved `PlayerBuff.end_time` according to overlay mode.
6. `PlayerBuff.GetTimerText()` converts the remaining internal delta back through the 450-second basis before formatting `H:MM:SS` or `M:SS`.

**One handoff remains open:** probe 0.1.0 did not include the body that takes the created buff flying object to `BuffsLogics.AddBuff`. Until that final call edge is captured, treat `dur_parameter = 18/36/54/...` as a strongly supported duration input, not yet as a fully closed end-to-end duration proof.

A user's installed time-scaling compatibility mods may deliberately alter the real-time conversion. PrayerClarity's mechanics documentation describes the stock 1.407 path unless a compatibility section explicitly says otherwise.

## Presentation evidence

### Prayer item description exposes the required church quality

Current 1.407 `ItemDefinition.GetItemDescription` contains a Preach branch. For sermon items it appends localization key `preach_params`, passing:

`"(cross)" + linked_craft.needs_quality`

Therefore the prayer description directly includes the prayer's required church-quality value in some localized format. Runtime prayer item definitions inspected so far have empty optional `q_hint`, so this requirement line is not coming from the generic quality-hint field.

### Prayer-selection UI directly shows church quality, requirement, and chance

`PrayCraftGUI.RedrawTextValues(needs_q, chance)` directly constructs exactly three lines of decision-state information:

- `pray_gui_church_q` with current church quality, formatted as `(cross){0:0.#}`;
- `pray_gui_sermon_needs` with the selected prayer's requirement, formatted as `(cross){0:0}`;
- `sermon_success_chance` with `RoundToInt(chance * 100) + "%"`.

`OnResourcePickerClosed` computes `chance = current_church_quality / needs_quality`, caps it at 1, and switches the action label to:

- `btn_try_pray` when below the requirement;
- `btn_pray` once the requirement is met.

Therefore **success probability is direct visible information in stock 1.407 and is not PrayerClarity's hidden-information problem**. Exact current Russian/English wording remains a localization task; the values and presentation structure are already direct facts.

### Post-sermon report separates base values from prayer bonuses

`PrayReportGUI.Open(PrayResult)` draws:

- people = `PrayResult.people`;
- Faith = `PrayResult.faith`;
- money = `PrayResult.money`;
- success/failure;
- success chance;
- only on success, a separate Faith bonus row when `faith_bonus > 0`;
- only on success, a separate money bonus row when `money_bonus != 0`.

The report does **not** combine base and bonus into one displayed Faith total or one displayed money total. This is presentation evidence: a player reading the report must understand that the bonus rows are additive to the base rows.

### Pre-use outcome forecast remains unproved

The selected-prayer information drawn by `PrayCraftGUI` is church quality, requirement, and success chance. Nothing in the inspected selection methods provides a current-state numerical forecast for:

- final/base-plus-bonus Faith;
- final/base-plus-bonus donations;
- Souls-prayer Faith after current Soul Gratitude;
- exact passive-buff magnitude/duration in a consistent quantitative form;
- physical special outputs such as `blessing_commerce`.

A complete tooltip/resource-picker audit is still needed before promoting this scoped absence to a final UX finding covering every pre-use surface.

## Secondary localization mapping

A public localization dump provides useful discovery mappings such as `b_faith` -> `Prayer for faith`, `b_money` -> `Prayer for donations`, `b_pen` -> `Prayer for imagination`, and `b_star` -> `Prayer for excellence`. It is not accepted as the canonical current 1.407 localization source, especially for DLC prayers that are absent from that dump.

Probe 0.1.0 attempted direct localization through `GJL` but searched only the Assembly-CSharp type list and did not resolve the type. The follow-up probe should search all loaded assemblies before falling back to secondary localization evidence.

## Open questions for the next narrow pass

1. Map all accepted internal families to exact current 1.407 English/Russian names and descriptions.
2. Trace callers/wiring of `SpreadFaithIncome`, `SpreadMoneyIncome`, and `DropPrayItems` to close final payout semantics, including failed sermons.
3. Trace the final `FlyingObject.CreateBuffFlyingObject -> BuffsLogics.AddBuff` call edge to close exact prayer-buff duration semantics.
4. Trace consumers of `buff_sins`, `buff_pen.craft_q`, `buff_star.craft_q`, and `increase_gp_gain`.
5. Determine the downstream use/effect of physical `blessing_commerce` items.
6. Classify extra `PrayCraft` families as reachable content versus legacy/internal rows.
7. Audit any final rounding/caps after `CalculatePray` and visitor distribution.
8. Audit buff hover text and remaining resource-picker/item-tooltip surfaces for pre-use information.

No production PrayerClarity implementation is justified yet. A single narrow read-only follow-up probe is justified for the call edges and passive-consumer questions that the existing static/runtime evidence does not contain.
