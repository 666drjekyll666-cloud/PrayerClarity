# Prayer Mechanics — Graveyard Keeper 1.407

Status: research evidence map, updated 2026-09-14 after read-only probes 0.1.0–0.1.3.

This document records verified mechanics before any PrayerClarity production implementation. `prayer item`, `PrayCraft`, `PrayEventDefinition`, localized prayer name, buff, and sermon flow remain distinct concepts unless direct evidence maps them.

## Evidence basis

Primary evidence comes from the user's installed Graveyard Keeper 1.407 runtime:

- `Assembly-CSharp, Version=11.0.0.0`;
- module MVID `6f50b8e7-156b-49ac-bbe8-7505894b2364`;
- Assembly-CSharp IL;
- runtime `GameBalance` definitions;
- current Russian localization;
- serialized/loaded FlowCanvas graphs;
- narrow read-only reflection probes 0.1.0–0.1.3.

The probes contain no Harmony patches, do not execute FlowCanvas graphs, and do not intentionally mutate game/save state. Proprietary assemblies, full decompilations, and raw game assets are not committed.

Community pages and wikis are UX/discovery evidence, not authority for mechanics when direct 1.407 evidence exists.

## Internal model

A sermon spans several layers:

1. prayer item, e.g. `b_faith:1`;
2. `CraftDefinition` with `craft_type = PrayCraft`;
3. craft `linked_sub_id` -> `PrayEventDefinition`;
4. event expressions for base `people`, `faith`, `money`;
5. prayer fields such as `needs_quality`, `k_faith`, `k_money`, fixed outputs, optional `buff`, `dur_parameter`;
6. `FlowCanvas/pray` distribution/animation graph;
7. downstream visitor, item-drop, and buff consumers.

Do not collapse these layers into one notion of “prayer”.

## Core prayer calculation

### Base values

`PrayLogics.CalculatePray` evaluates:

- `people = max(0, RoundToInt(event.people))`
- `faith = max(0, RoundToInt(event.faith))`
- `money = max(0, event.money)`

Base people/Faith are integer-rounded here. Base money remains floating-point.

### Success probability

For `needs_quality != 0`:

`success_percent = RoundToInt(current_church_quality / needs_quality * 100)`

Then clamp to `0..100`.

- 100% succeeds automatically;
- otherwise success is `Random.Range(0,100) < success_percent`.

For `needs_quality == 0`, success is 100%.

### Success-only prayer bonuses

On success:

- fixed `faith` output -> `faith_bonus += value`;
- fixed `money` output -> `money_bonus += value / 100`;
- `k_faith` -> `faith_bonus += RoundToInt(base_faith * k_faith)`;
- `k_money` -> `money_bonus += Round(base_money * k_money * 100) / 100`;
- non-Faith/non-money outputs remain in `_sermon_drops`.

On failure:

- `faith_bonus = 0`;
- `money_bonus = 0`.

Base event values remain intact.

`Flow_CalculatePrayEvent` exposes downstream:

- `people`;
- base `faith`;
- `faith_bonus` separately;
- `money = base_money + money_bonus`;
- `success`.

## Final sermon payout wiring

Probe 0.1.2 captured the stock `FlowCanvas/pray` graph.

### Faith

Base Faith is sent to `Flow_SpreadFaithIncome` before the success/failure reaction stage. A second `Flow_SpreadFaithIncome` later receives `faith_bonus`.

Therefore:

- successful sermon: `Faith delivered = base_faith + faith_bonus`;
- failed sermon: `Faith delivered = base_faith`.

Failure does **not** remove base Faith.

### Money

The graph passes combined money to `Flow_SpreadMoneyIncome`. Its `chance` input is:

- success -> `1.0`;
- failure -> `0.5`.

However, `SpreadMoneyIncome` selects visitors using the integer overload `UnityEngine.Random.Range(0,1)`. That overload can only return `0`, so both `0 < 1.0` and `0 < 0.5` are always true. In stock 1.407 all visitors are selected in both branches.

Therefore the actual result is:

- successful sermon: `money delivered = base_money + money_bonus`;
- failed sermon: `money delivered = base_money`.

The graph appears structured as though failure should reduce donation participation, but the current helper implementation does not do so. Record this as an implementation mismatch, not as asserted developer intent.

### Distribution details

`SpreadFaithIncome(prayers, faith)` assigns each Faith unit independently to a random valid visitor by incrementing that visitor's `_faith`.

`SpreadMoneyIncome(prayers, money, success_percent)`:

1. chooses selected visitors using the test above;
2. gives each selected visitor `floor(money*100/selected_count)/100`;
3. distributes leftover cents randomly until the requested total is reached.

Visible per-person coin drops are therefore distribution of a total pool, not the formula that creates the pool.

## Verified PrayEventDefinition catalogue

Notation:

- `CQ = GetZoneQ("church")`
- `GQ = GetZoneQ("graveyard")`
- `E = Ppar("p_eloquence")`
- `C = Ppar("p_cardinal")`
- `GP = Ppar("gratitude_points")`

| Event ID | Faith | Money | People |
| --- | --- | --- | --- |
| `default` | `CQ*0.4*(1+0.3E)` | `GQ*0.06` | `CQ*0.5` |
| `default_0` | `CQ*0.2*(1+0.3E)` | `GQ*(0.03+0.01C)` | `CQ*1` |
| `default_1` | same | same | `CQ*0.5` |
| `default_2` | same | same | `CQ*0.34` |
| `default_3` | same | same | `CQ*0.34` |
| `pray_for_souls_1` | `(CQ+GP)*0.1*(1+0.3E)` | `GQ*(0.03+0.01C)` | `CQ*0.5` |
| `pray_for_souls_2` | same | same | `CQ*0.34` |
| `pray_for_souls_3` | same | same | `CQ*0.34` |

Direct consequences:

- ordinary base Faith depends on church quality;
- base donations depend on graveyard quality;
- Eloquence changes base Faith;
- Cardinal changes the ordinary donation coefficient;
- Souls prayer uses current Soul Gratitude in its Faith baseline.

Without Eloquence, Souls base Faith is `(CQ + GP)/10` before integer rounding.

## Verified player prayer families

`q` = `needs_quality`; `kF` = `k_faith`; `kM` = `k_money`.

| Internal family | Current RU name | Event(s) | q tiers | kF tiers | kM tiers | Special data |
| --- | --- | --- | --- | --- | --- | --- |
| `b_empty` | Обычная молитва | `default_0` | 10 | 0 | 0 | fixed Faith on base craft; suffixed/placeholder reachability still open |
| `b_faith` | Молитва веры | `default_1/2/3` | 10/20/50 | .5/1/1.5 | .2/.2/.2 | fixed Faith + money |
| `b_money` | Молитва о пожертвованиях | `default_1/2/3` | 10/20/50 | .2/.2/.2 | .5/1/1.5 | fixed Faith + money |
| `b_faith_money` | Комбо-молитва | `default_1/2/3` | 15/30/60 | .5/1/1.5 | .5/1/1.5 | fixed Faith + money |
| `b_plant` | Молитва о корнях и побегах | `default_1/2/3` | 10/20/30 | .25/.25/.25 | .25/.25/.25 | `buff_plant`; 36/72/108 min |
| `b_sins` | Молитва о покаянии | `default_1/2/3` | 10/20/40 | .25/.5/.75 | .1/.1/.1 | `buff_sins`; 18/36/54 min; no consumer found in final audit |
| `b_skull` | Молитва об упокоении | `default_1/2/3` | 20/40/50 | .1/.1/.1 | .25/.5/.75 | `buff_skull`; 18/36/54 min |
| `b_sword` | Молитва о возмездии | `default_1/2/3` | 10/20/40 | .25/.25/.25 | .25/.25/.25 | `buff_sword`; 36/72/108 min |
| `b_shield` | Защитная молитва | `default_1/2/3` | 10/20/40 | .25/.25/.25 | .25/.25/.25 | `buff_shield`; 36/72/108 min |
| `b_pen` | Молитва воображения | `default_1/2/3` | 10/40/60 | .25/.5/.75 | .1/.1/.1 | `buff_pen`; 18/36/54 min |
| `b_star` | Молитва о совершенстве | `default_1/2/3` | 10/40/60 | .25/.5/.75 | .1/.1/.1 | `buff_star`; 18/36/54 min |
| `b_village` | Молитва о процветании | `default_1/2/3` | 10/20/30 | .25/.25/.25 | .25/.25/.25 | Faith 1/2/3; money 1/2/3 silver; `blessing_commerce` x1/x2/x3 |
| `b_souls` | Молитва за упокой душ | `pray_for_souls_1/2/3` | 15/30/60 | .5/1/1.5 | .25/.25/.25 | Souls-specific baseline |
| `b_grat_points_incr` | Молитва о довольстве душ | `default_1/2/3` | 10/20/30 | .25/.25/.25 | .25/.25/.25 | `buff_gp_increase`; 36/72/108 min |
| `b_sin_shard` | Молитва за тщательное очищение душ | `default_1/2/3` | 10/20/30 | .25/.25/.25 | .25/.25/.25 | `buff_sin_shard`; 36/72/108 min |

Better Save Soul technology `soul_church_additions` directly unlocks `b_souls`, `b_grat_points_incr`, and `b_sin_shard`.

### Prayer for Prosperity physical output

`b_village` outputs:

- tier 1: Faith x1, 1 silver, `blessing_commerce` x1;
- tier 2: Faith x2, 2 silver, `blessing_commerce` x2;
- tier 3: Faith x3, 3 silver, `blessing_commerce` x3.

Faith/money become bonus fields on success. `blessing_commerce` remains in `_sermon_drops` and is handled as a physical sermon drop.

Current RU item name/description identifies it as `Благословение коммерции`, sold to a merchant to raise that merchant's level. Exact merchant-side implementation remains open.

### Unclassified PrayCraft rows

Runtime data also contains `b_ghost`, `b_energy`, `b_random`, `b_techpoint_blue`, `b_techpoint_green`, `b_techpoint_red`, `b_circle`, `b_cross`.

These remain internal/unclassified until unlock/item/UI reachability proves player use.

## Buff mechanics

| Buff ID | Direct data | Verified meaning/status |
| --- | --- | --- |
| `buff_plant` | `buff_plant=1` | inspected growing crafts contain `-0.2*WGOpar("buff_plant")` craft-time term; full affected set not yet enumerated |
| `buff_sins` | `buff_sins=1` | buff is created by Prayer of Repentance, but final reference audit found no gameplay consumer |
| `buff_skull` | `body_max=1` | +1 `body_max` while active |
| `buff_sword` | `add_damage=5` | +5 damage |
| `buff_shield` | `add_armor=4` | +4 armor |
| `buff_pen` | `craft_q=0.7` | +0.7 input to linked-buff multi-quality calculation |
| `buff_star` | `craft_q=0.2` | +0.2 input to linked-buff multi-quality calculation |
| `buff_sin_shard` | `increase_sin_shard_drop=1` | doubles Sin Shards from soul healing |
| `buff_gp_increase` | `increase_gp_gain=1` | read by `soul_portal`; current item text states +10%; exact graph connection/rounding remains open |

### `buff_sins` final reference audit

Probe 0.1.3 searched three independent surfaces in the running 1.407 installation:

1. string-literal users across loaded assemblies;
2. serialized loaded FlowCanvas graphs;
3. runtime GameBalance fields/SmartExpressions/nested data.

Results:

- no game-code method contains a `buff_sins` literal consumer;
- 180 loaded FlowCanvas graphs contained zero `buff_sins` hits;
- GameBalance hits were only:
  - the `buff_sins` definition itself;
  - `pray:b_sins:1` -> `buff_sins`;
  - `pray:b_sins:2` -> `buff_sins`;
  - `pray:b_sins:3` -> `buff_sins`.

**Fact:** the prayer produces/attaches a timed `buff_sins`, but no consumer was found in the inspected runtime code, loaded graphs, or balance data.

**Strong hypothesis:** the special gameplay effect of Prayer of Repentance is inert/unimplemented in stock 1.407. Keep this wording as a hypothesis rather than claiming mathematical impossibility of an unknown dynamic indirection.

## Multi-quality buffs

`CraftDefinition.GetBuffValue(buff_id)` returns 0 when the buff is inactive and otherwise returns `BuffDefinition.craft_q`.

`CraftDefinition.GetMultiqualityResult(...)` consumes linked buff IDs through this accessor. Therefore `buff_pen=0.7` and `buff_star=0.2` are real additive quality inputs for crafts whose definitions explicitly list those buffs.

Do not generalize the affected craft set until `linked_buffs` are enumerated.

## Sin Shards

Soul-healing output starts from `sins_count` and adds:

`increase_sin_shard_drop * sins_count`

With `buff_sin_shard=1`:

`Sin Shards = 2 * sins_count`.

## Soul Gratitude

`SoulsHelper.CalculatePointsAfterSoulRelease(healed_soul)` computes:

- `d = healed_soul.durability`;
- `s = healed_soul.GetParam("sins_count",0)`;
- if `d > 0.9`, replace `d` with `1`;
- return `5*d + 5*s`.

Thus:

`GP_base = 5 * effective_durability + 5 * sins_count`.

Probe 0.1.2 observed the loaded `soul_portal` graph reading `increase_gp_gain`, passing values through multiply/add/multiply nodes, `Mathf.RoundToInt`, and then adding to `gratitude_points`. Current Russian prayer text states `+10%` Gratitude gain.

Probe 0.1.3 could not reacquire `soul_portal` through `CustomFlowScript.GetGraph("soul_portal")` (`Graph=null`), so the exact connection order and rounding semantics of that +10% remain unproved. This is now a narrow non-blocking mechanics detail, not a reason for another broad probe.

## Prayer buff application and duration

The stock path is closed:

1. `PlayerComponent.StartPrayAnimation(pray_craft, success)` stores the craft, resolved buff, and success state.
2. `PlayerComponent.CreatePrayBuffFlyingObject(pos)` passes non-zero `dur_parameter` as duration override.
3. `FlyingObject.CreateBuffFlyingObject(...)` stores the override.
4. `FlyingObject.ReallyGiveBuff(...)` calls `BuffsLogics.AddBuff(buff.id, override)`.
5. `AddBuff` uses the override instead of the BuffDefinition default when supplied.
6. `PlayerBuff.GetTimerText()` uses the same 450-second game-time basis.

Prayer durations therefore map directly to displayed game-timer minutes:

- `18/36/54` -> 18/36/54 minutes;
- `36/72/108` -> 36 min / 1 h 12 min / 1 h 48 min.

Higher prayer quality can therefore increase duration while the underlying buff magnitude remains unchanged.

### Success animation / physical-drop path

Probe 0.1.3 adds:

- `BaseCharacterComponent.StartPrayAnimation(success)` writes Animator bool `success`;
- `ChurchPulpit.DoBuffSuccessAnimation()` directly calls both:
  - `PlayerComponent.CreatePrayBuffFlyingObject(...)`;
  - `PrayLogics.DropPrayItems()`.

No C# caller exists for `DoBuffSuccessAnimation`, which is consistent with a Unity AnimationEvent callback. The exact animation-asset edge was not inspected.

**Fact:** buff creation and physical sermon drops are grouped in the method explicitly named `DoBuffSuccessAnimation`, while the Animator receives the sermon success flag.

**Strongly supported, not asset-level proven:** buffs and `_sermon_drops` are success-animation-only outputs.

## Current Russian presentation

Selection UI strings/values:

- `Качество церкви: %1`
- `Проповедь требует: %1`
- `Шанс успеха: %1`
- below threshold: `Попытка молитвы`
- at/above threshold: `Молиться`

Prayer item description also appends `%1 необходимо, чтобы гарантировать успех проповеди.`

`PrayCraftGUI.RedrawTextValues` shows church quality, requirement, and success chance only. It does not forecast current Faith/donation output or quantify passive effects/duration.

Post-sermon report shows base Faith/money plus separate success-only bonus rows; it does not show one combined Faith total or donation total.

Several passive prayer descriptions are qualitative only:

- `b_plant`: no farming magnitude/duration;
- `b_sins`: no mechanical effect/duration;
- `b_skull`: no `body_max` effect/duration;
- `b_sword`: no `+5 damage`/duration;
- `b_shield`: no `+4 armor`/duration;
- `b_pen`: no exact writing-quality effect/duration;
- `b_star`: no exact craft-quality effect/duration.

Better Save Soul descriptions are clearer:

- `b_souls`: says Soul Gratitude increases Faith;
- `b_grat_points_incr`: states `+10%` Gratitude gain;
- `b_sin_shard`: states `x2` Sin Shards.

## Remaining narrow questions

These no longer block UX design:

1. exact `soul_portal` +10% connection order/rounding;
2. exact craft scopes for `buff_pen`, `buff_star`, `buff_plant` where wording needs it;
3. merchant-side implementation of `blessing_commerce`;
4. reachability classification for extra PrayCraft rows;
5. buff-hover text if post-use presentation becomes relevant;
6. English localization only if bilingual support becomes a requirement;
7. Animator asset edge for success callback only if implementation needs that lifecycle target.

## Research-stage conclusion

The core sermon calculation, payout behavior, quality requirements, principal buff durations, major buff magnitudes, Souls baseline, Sin Shard effect, and current selection/report presentation are sufficiently verified to move from mechanics discovery to **UX design options**.

Do not start production implementation yet. Next step is an explicit actual-mechanics -> vanilla-information -> player-interpretation matrix and a narrow UI design hypothesis.