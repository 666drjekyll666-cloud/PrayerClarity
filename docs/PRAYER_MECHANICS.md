# Prayer Mechanics — Graveyard Keeper 1.407

Status: evidence map, updated 2026-09-14 after targeted read-only probes 0.1.0–0.1.2.

This document records verified mechanics before any PrayerClarity production implementation. `prayer item`, `PrayCraft`, `PrayEventDefinition`, localized prayer names, and the sermon flow are kept distinct unless direct evidence maps them.

## Evidence basis

Primary evidence comes from read-only diagnostics captured from the user's installed Graveyard Keeper 1.407 runtime, including Assembly-CSharp IL, runtime balance definitions, current localization, and serialized FlowCanvas graphs. The inspected runtime uses `Assembly-CSharp, Version=11.0.0.0` with module MVID `6f50b8e7-156b-49ac-bbe8-7505894b2364`.

The probes are diagnostic only: no Harmony patches, graph execution, or game-state mutation. Raw proprietary assemblies/assets/graphs are not committed; only minimal derived facts are retained here.

Community pages, wikis, guides, calculators, and screenshots are discovery or UX evidence, not authority for internal formulas when direct 1.407 evidence is available.

## Internal model

A sermon/prayer interaction spans distinct layers:

- a prayer item, e.g. `b_faith:1`;
- a `CraftDefinition` with `craft_type = PrayCraft`, normally used at `church_pulpit`;
- the craft's `linked_sub_id`, selecting a `PrayEventDefinition` such as `default_1` or `pray_for_souls_1`;
- the `PrayEventDefinition`, supplying base `people`, `faith`, and `money` expressions;
- prayer-craft fields such as `needs_quality`, `k_faith`, `k_money`, fixed outputs, optional `buff`, and `dur_parameter`;
- the `pray` FlowCanvas graph, which distributes Faith/money and drives prayer animation;
- downstream visitor, item-drop, and buff consumers.

These layers must not be treated as synonyms.

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

### Prayer-specific bonuses

On success, `CalculatePray` processes the prayer craft's output list and coefficients:

- fixed output `faith` -> add its `value` to `faith_bonus`;
- fixed output `money` -> add `value / 100` to `money_bonus`;
- `k_faith != 0` -> add `RoundToInt(base_faith * k_faith)` to `faith_bonus`;
- `k_money != 0` -> add `Round(base_money * k_money * 100) / 100` to `money_bonus`;
- any other output remains in `_sermon_drops`.

On a failed success check:

- `faith_bonus = 0`;
- `money_bonus = 0`.

The base event values remain intact.

### Downstream `PrayResult` shape

`Flow_CalculatePrayEvent` exposes:

- `people = PrayResult.people`;
- `faith = PrayResult.faith`;
- `faith_bonus = PrayResult.faith_bonus`;
- `money = PrayResult.money + PrayResult.money_bonus`;
- `success = PrayResult.success`.

So the sermon graph receives money already combined, while base Faith and bonus Faith remain separate.

## Final sermon payout wiring

Probe 0.1.2 captured the stock `FlowCanvas/pray` graph directly.

### Faith

The graph sends base `faith` from `Flow_CalculatePrayEvent` to `Flow_SpreadFaithIncome` **before** the prayer-buff/success-reaction stage. Therefore base Faith is distributed on both successful and failed sermons.

A second `Flow_SpreadFaithIncome` receives `faith_bonus` later in the graph. Both the success and failure reaction branches reach that node, but `CalculatePray` has already set `faith_bonus = 0` on failure.

Therefore:

- success: `Faith delivered = base_faith + faith_bonus`;
- failure: `Faith delivered = base_faith`.

### Money

The graph sends the combined money output to `Flow_SpreadMoneyIncome`. A `Flow_ConditionFloatValue` supplies its `chance` input:

- success -> `1.0`;
- failure -> `0.5`.

However, `PrayLogics.SpreadMoneyIncome` tests each visitor with the **integer** overload `UnityEngine.Random.Range(0, 1)`, then compares the resulting integer (converted to float) with `success_percent`.

Because integer `Random.Range(0, 1)` can only return `0`, both `0 < 1.0` and `0 < 0.5` are always true. In stock 1.407 every visitor is selected in both branches. The helper then distributes the requested money among the selected visitors while preserving the requested total to cent precision.

Therefore the actual stock-1.407 result is:

- success: `money delivered = base_money + money_bonus`;
- failure: `money delivered = base_money`.

The graph structure suggests the `0.5` failure value was intended to affect donation participation, but the current helper implementation does not produce that effect. Treat this as a direct implementation mismatch, not as an inferred design intention.

### Visitor-distribution details

`PrayLogics.SpreadFaithIncome(prayers, faith)`:

1. removes null visitor references;
2. repeats exactly `faith` times;
3. chooses a random visitor for each unit;
4. adds `1` to that visitor's `_faith`.

`PrayLogics.SpreadMoneyIncome(prayers, money, success_percent)`:

1. builds the selected visitor list using the test described above;
2. computes equal share `floor(money * 100 / selected_count) / 100`;
3. adds that share to every selected visitor's `_money`;
4. assigns leftover cents one at a time to random selected visitors until the requested total is reached.

The visible per-person coin animation therefore represents distribution of a total pool; it is not itself the source formula for total donations.

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

- ordinary base Faith is driven by **church quality**;
- base donations are driven by **graveyard quality**;
- Eloquence modifies base Faith;
- Cardinal modifies the ordinary base-donation coefficient where the event uses `0.03 + 0.01*C`;
- the Souls family adds current Soul Gratitude to church quality before calculating base Faith.

Without Eloquence, Souls base Faith is `(CQ + GP) / 10` before integer rounding.

## Verified prayer-craft families

`q` = `needs_quality`; `kF` = `k_faith`; `kM` = `k_money`.

| Internal family | Current RU name | Linked event(s) | q, tiers 1/2/3 | kF, tiers | kM, tiers | Additional direct data |
| --- | --- | --- | --- | --- | --- | --- |
| `b_empty` | Обычная молитва | `default_0` for `pray:b_empty` | 10 | 0 | 0 | fixed Faith output on non-suffixed craft; placeholder/suffixed reachability still open |
| `b_faith` | Молитва веры | `default_1/2/3` | 10 / 20 / 50 | .5 / 1 / 1.5 | .2 / .2 / .2 | fixed Faith + money outputs |
| `b_money` | Молитва о пожертвованиях | `default_1/2/3` | 10 / 20 / 50 | .2 / .2 / .2 | .5 / 1 / 1.5 | fixed Faith + money outputs |
| `b_faith_money` | Комбо-молитва | `default_1/2/3` | 15 / 30 / 60 | .5 / 1 / 1.5 | .5 / 1 / 1.5 | fixed Faith + money outputs |
| `b_plant` | Молитва о корнях и побегах | `default_1/2/3` | 10 / 20 / 30 | .25 / .25 / .25 | .25 / .25 / .25 | `buff_plant`; duration 36 / 72 / 108 |
| `b_sins` | Молитва о покаянии | `default_1/2/3` | 10 / 20 / 40 | .25 / .5 / .75 | .1 / .1 / .1 | `buff_sins`; duration 18 / 36 / 54; consumer open |
| `b_skull` | Молитва об упокоении | `default_1/2/3` | 20 / 40 / 50 | .1 / .1 / .1 | .25 / .5 / .75 | `buff_skull`; duration 18 / 36 / 54 |
| `b_sword` | Молитва о возмездии | `default_1/2/3` | 10 / 20 / 40 | .25 / .25 / .25 | .25 / .25 / .25 | `buff_sword`; duration 36 / 72 / 108 |
| `b_shield` | Защитная молитва | `default_1/2/3` | 10 / 20 / 40 | .25 / .25 / .25 | .25 / .25 / .25 | `buff_shield`; duration 36 / 72 / 108 |
| `b_pen` | Молитва воображения | `default_1/2/3` | 10 / 40 / 60 | .25 / .5 / .75 | .1 / .1 / .1 | `buff_pen`; duration 18 / 36 / 54 |
| `b_star` | Молитва о совершенстве | `default_1/2/3` | 10 / 40 / 60 | .25 / .5 / .75 | .1 / .1 / .1 | `buff_star`; duration 18 / 36 / 54 |
| `b_village` | Молитва о процветании | `default_1/2/3` | 10 / 20 / 30 | .25 / .25 / .25 | .25 / .25 / .25 | Faith 1/2/3; money 1/2/3 silver; `blessing_commerce` x1/x2/x3 |
| `b_souls` | Молитва за упокой душ | `pray_for_souls_1/2/3` | 15 / 30 / 60 | .5 / 1 / 1.5 | .25 / .25 / .25 | Souls-specific baseline |
| `b_grat_points_incr` | Молитва о довольстве душ | `default_1/2/3` | 10 / 20 / 30 | .25 / .25 / .25 | .25 / .25 / .25 | `buff_gp_increase`; duration 36 / 72 / 108 |
| `b_sin_shard` | Молитва за тщательное очищение душ | `default_1/2/3` | 10 / 20 / 30 | .25 / .25 / .25 | .25 / .25 / .25 | `buff_sin_shard`; duration 36 / 72 / 108 |

The Better Save Soul technology `soul_church_additions` directly unlocks `b_souls`, `b_grat_points_incr`, and `b_sin_shard`.

### `b_village` direct outputs

- tier 1: `faith x1`, `money x100` (= 1 silver as prayer output), `blessing_commerce x1`;
- tier 2: `faith x2`, `money x200`, `blessing_commerce x2`;
- tier 3: `faith x3`, `money x300`, `blessing_commerce x3`.

Faith/money rows become prayer bonus fields on success. `blessing_commerce` is not special-cased by `CalculatePray`, so it remains in `_sermon_drops`.

`blessing_commerce` is current-RU `Благословение коммерции`; its description says it can be sold to a merchant to raise that merchant's level. Its exact downstream merchant transaction/effect path is still open.

### Other `PrayCraft` rows

Runtime data also contains `b_ghost`, `b_energy`, `b_random`, `b_techpoint_blue`, `b_techpoint_green`, `b_techpoint_red`, `b_circle`, and `b_cross`.

These remain **unclassified internal data** until item definitions, unlock paths, localization, and UI reachability prove player use.

## Verified buff mechanics

| Buff ID | Direct data | Verified gameplay meaning |
| --- | --- | --- |
| `buff_plant` | `buff_plant = 1` | inspected growing crafts include `-0.2 * WGOpar("buff_plant")` in craft-time logic; full scope still needs enumeration |
| `buff_sins` | `buff_sins = 1` | buff exists and receives prayer duration; gameplay consumer still unresolved |
| `buff_skull` | `body_max = 1` | +1 `body_max` while active |
| `buff_sword` | `add_damage = 5` | +5 damage while active |
| `buff_shield` | `add_armor = 4` | +4 armor while active |
| `buff_pen` | `craft_q = 0.7` | contributes +0.7 through linked-buff multi-quality calculation |
| `buff_star` | `craft_q = 0.2` | contributes +0.2 through linked-buff multi-quality calculation |
| `buff_sin_shard` | `increase_sin_shard_drop = 1` | doubles base Sin Shard output during soul healing |
| `buff_gp_increase` | `increase_gp_gain = 1` | consumed by the `soul_portal` Gratitude path; exact multiplier wiring still needs full graph capture |

### Multi-quality buffs

`CraftDefinition.GetBuffValue(buff_id)` returns 0 if the named buff is inactive; otherwise it returns that `BuffDefinition.craft_q`.

`CraftDefinition.GetMultiqualityResult(...)` consumes linked buff IDs through this accessor. Therefore `buff_pen = 0.7` and `buff_star = 0.2` are real additive inputs to the game's multi-quality calculation for crafts whose definitions list those buffs.

The exact set of affected crafts is a catalogue question and should not be generalized beyond directly enumerated `linked_buffs`.

### Sin Shards

The soul-healing consumer starts from the healed soul's `sins_count` and adds:

`increase_sin_shard_drop * sins_count`

With `buff_sin_shard = 1`:

`Sin Shards awarded = 2 * sins_count`

### Base Soul Gratitude on release

`SoulsHelper.CalculatePointsAfterSoulRelease(healed_soul)` computes:

1. `d = healed_soul.durability`;
2. `s = healed_soul.GetParam("sins_count", 0)`;
3. if `d > 0.9`, set `d = 1`;
4. return `5*d + 5*s`.

So the direct pre-buff Gratitude amount is:

`GP_base = 5 * effective_durability + 5 * sins_count`

where `effective_durability = 1` for durability above 0.9.

The loaded `soul_portal` graph then:
- calculates this GP value;
- reads player parameter `increase_gp_gain`;
- passes values through multiply/add/multiply nodes;
- rounds with `Mathf.RoundToInt`;
- adds the result to `gratitude_points`;
- also compares current Gratitude with Souls-zone quality for a cap/limit path.

Current Russian prayer text states `(+10%)`, but probe 0.1.2 only captured snippets of this graph, not enough connection data to prove the exact multiplier formula. Do not yet replace graph evidence with the localization claim.

## Prayer buff application and duration

The end-to-end stock path is now closed:

1. `PlayerComponent.StartPrayAnimation(pray_craft, success)` resolves and stores `pray_craft.buff`, the craft, and success state.
2. `PlayerComponent.CreatePrayBuffFlyingObject(pos)` passes `pray_craft.dur_parameter` as an explicit duration when non-zero.
3. `FlyingObject.CreateBuffFlyingObject(...)` stores that override.
4. `FlyingObject.ReallyGiveBuff(buff)` calls `BuffsLogics.AddBuff(buff.id, _override_duration)`.
5. `BuffsLogics.AddBuff` uses the explicit duration when present; otherwise it evaluates `BuffDefinition.length`.
6. The saved `PlayerBuff.end_time` and `PlayerBuff.GetTimerText()` use the same 450-second game-time basis.

For prayer crafts with non-zero `dur_parameter`, that prayer value overrides the buff definition's default length.

In stock 1.407 the prayer tier values correspond to game-timer minutes:

- `18 / 36 / 54` -> 18 / 36 / 54 minutes;
- `36 / 72 / 108` -> 36 min / 1 h 12 min / 1 h 48 min.

Installed time-scaling mods may deliberately change real-time pacing; these are vanilla game-time semantics.

**Still open:** the exact animation callback branch that selects the success animation (which creates the buff flying object and calls `DropPrayItems`) versus the failure animation has not yet been captured as a direct call edge. `ChurchPulpit.DoBuffSuccessAnimation()` itself directly performs both buff-object creation and `DropPrayItems()`.

## Current Russian localization / presentation

Direct runtime localization resolves the accepted families and UI. Important decision-screen strings:

- `Качество церкви: %1`
- `Проповедь требует: %1`
- `Шанс успеха: %1`
- below threshold: `Попытка молитвы`
- at/above threshold: `Молиться`
- item requirement line: `%1 необходимо, чтобы гарантировать успех проповеди.`

`PrayCraftGUI.RedrawTextValues` shows exactly church quality, selected sermon requirement, and success chance. It does not calculate a current Faith/donation forecast.

The post-sermon report uses:
- `Пришло посетителей`
- `Веры получено`
- `Пожертвования`
- `Результат`
- `Бонус веры`
- `Бонус денег`

`PrayReportGUI.Open` displays base Faith and base money separately from success-only bonus rows; it does not show a single combined total.

### Current prayer descriptions

Several base-game passive descriptions are qualitative rather than quantitative:

- `b_plant`: flavour text, no farming magnitude/duration;
- `b_sins`: flavour text, no mechanical effect/duration;
- `b_skull`: flavour/joke, no `body_max` effect/duration;
- `b_sword`: rage wording, no `+5 damage` or duration;
- `b_shield`: strength wording, no `+4 armor` or duration;
- `b_pen`: inspiration wording, no `+0.7 craft_q` or duration;
- `b_star`: hard-work wording, no `+0.2 craft_q` or duration.

The Better Save Soul descriptions are comparatively more explicit:
- `b_souls`: says more Soul Gratitude gives more Faith;
- `b_grat_points_incr`: states `(+10%)` Soul Gratitude gain;
- `b_sin_shard`: states `x2` Sin Shards from successful soul healing.

## Open questions

1. Capture the full `soul_portal` graph to prove the exact `increase_gp_gain` arithmetic and Gratitude cap path.
2. Find the gameplay consumer, if any, of `buff_sins`.
3. Capture the prayer animation success/failure callback edge to prove that buffs and `_sermon_drops` are success-only end to end.
4. Enumerate the exact craft scopes of `buff_pen`, `buff_star`, and `buff_plant` where useful for player-facing wording.
5. Determine the exact downstream merchant effect/use path of `blessing_commerce`.
6. Classify extra `PrayCraft` families as reachable content versus legacy/internal rows.
7. Audit buff-hover text and any remaining pre-use tooltip/resource-picker surfaces.
8. Capture current English localization only if bilingual wording becomes a design requirement.

No production PrayerClarity implementation is justified yet. The remaining unknowns are narrow enough for one more read-only static/runtime probe rather than gameplay experimentation.
