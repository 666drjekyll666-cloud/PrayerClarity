# BSS / Donations Rebalance — Implementation Feasibility Research

Status: **technical feasibility established; implemented on `dev/rebalanced-0.2.17`; runtime acceptance pending**.

Target: Graveyard Keeper 1.407.

This document evaluates whether the currently preferred next Rebalanced candidate can be implemented through narrow, host-native-first seams:

- Donations: q20/q40/q60, +20/+50/+100 silver;
- Soul's Repose: q30/q60/q90, successful 1:1 Soul Gratitude -> Faith conversion capped at 30/60/90;
- Soul Contentment: preserve +50% Soul Gratitude gain and prevent soul-condition decay while active;
- Thorough Cleansing: q30/q60/q120, x2/x3/x4 Sin Shards;
- pulpit: show the live Soul's Repose transaction under the existing success block.

The preferred wording/design direction is recorded in `GAME_NEEDS_MATRIX.md`.

## Executive conclusion

**No implementation blocker was found.**

All requested behaviors can be expressed with narrow existing game-owned paths. The only materially new gameplay seam is Soul Contentment's soul-preservation rule; even there, both soul forms converge on the same native durability method, so no world scan or per-frame custom loop is needed.

A future production candidate should still receive focused runtime acceptance for the three runtime-sensitive mechanics:
1. Soul's Repose exact SG deduction + native Faith bonus;
2. Soul Contentment no-decay coverage for both in-corpse and extracted souls;
3. Thorough Cleansing exact x2/x3/x4 output.

Donations is a value-only change on an already accepted path.

## 1. Donations — straightforward existing path

### Direct mechanism

Rebalanced already stores Donations as fixed money output in the canonical ruleset and projects that output into the live `CraftDefinition`. Stock `PrayLogics.CalculatePray` owns sermon success and extracts the fixed `money` output into the success-only money bonus.

Candidate values:
- Bronze q20: +20 silver -> 2000 cents;
- Silver q40: +50 silver -> 5000 cents;
- Gold q60: +100 silver -> 10000 cents.

### Presentation

Use the native money formatter so Gold naturally renders as the game's normal 1-gold representation rather than a hard-coded Russian/English string.

Under an existing success heading, no redundant “Additional donations” label is needed.

### Risk / evidence gate

**Low risk.** This is the same accepted fixed-money path used by current +5/+15/+30 silver.

A fresh standalone in-game test is not required solely for this change; exact source diff + compile/package validation is sufficient unless it is bundled into a larger runtime candidate that is being tested anyway.

## 2. Thorough Cleansing — native scalar already supports x2/x3/x4

### Direct 1.407 evidence

`SoulHealingWidget.OnStartHealButtonPressed()` computes Sin Shards as:

```
base = sins_count
result = base + player.GetParamInt("increase_sin_shard_drop") * base
```

Therefore:
- effective input 1 -> x2;
- effective input 2 -> x3;
- effective input 3 -> x4.

Stock `buff_sin_shard` already supplies the first +1.

### Preferred implementation seam

Capture the successful Thorough Cleansing quality using the existing `RebalancedTierState` pattern.

Patch only the native soul-healing action and temporarily project the **additional** tier delta into the player's nonserialized runtime aggregate before the game reads `increase_sin_shard_drop`:

- Bronze: extra 0; stock buff remains effective 1 -> x2;
- Silver: extra +1 -> effective 2 -> x3;
- Gold: extra +2 -> effective 3 -> x4.

Restore the exact previous runtime value in a Harmony finalizer.

This mirrors the accepted scoped-runtime-input pattern already used by Rebalanced Roots:
- no replacement shard formula;
- no inventory scanning;
- no permanent mutation of `BuffDefinition.res`;
- native `sins_count`, healing result, shard creation and crafting remain authoritative.

### Why not mutate the shared BuffDefinition

Changing `buff_sin_shard.res` dynamically would create shared-definition/lifecycle ownership problems: `BuffsLogics.AddBuff` adds the definition's resource when the buff begins and `RemoveBuff` later subtracts whatever the shared definition contains then. A temporary tier mutation could therefore make add/remove asymmetric.

Scoped input projection around the exact healing operation avoids that risk.

### Runtime gate

One focused test can cover the property:
- heal comparable souls under Bronze/Silver/Gold;
- verify exact x2/x3/x4 shard count;
- verify stock behavior when the prayer is inactive;
- verify no residual tier delta after the healing call.

No broad gameplay test is required.

## 3. Soul Contentment — one common decay owner covers both soul forms

### Direct 1.407 evidence

Soul condition is ordinary `Item.durability`.

Both relevant forms have explicit item types:
- `ItemType.SoulBodyPart` — the soul still embedded inside a corpse;
- `ItemType.Soul` — extracted/healed soul items.

All normal durability loss converges on:

`Item.UpdateDurability(float delta_time, float parent_modificator)`

The native method:
1. optionally updates child items;
2. evaluates parent/container/zone durability modifiers;
3. subtracts `definition.durability_decrease * modifiers * delta_time`.

`ItemsDurabilityManager` drives this path for world inventories, player inventory, dropped/overhead items, and nested item updates.

### Preferred preservation seam

Patch `Item.UpdateDurability` with a very small prefix:

- if Soul Contentment is not active -> execute stock method;
- if item definition type is neither `Soul` nor `SoulBodyPart` -> execute stock method;
- if Soul Contentment is active and type is one of those two -> skip the durability update for that soul item.

Consequences:
- an in-corpse soul is preserved;
- an extracted soul is preserved;
- the corpse itself still decays normally;
- non-soul tools/items remain completely stock;
- Soul Container I/II/III behavior remains stock when the prayer is inactive;
- while the prayer is active, Container III and the prayer overlap harmlessly at the same “no soul decay” result.

No custom timer loop, container enumeration or Unity object scan is needed. The hook sits on a frequently used native method, but the runtime work is only a type/buff predicate; this is preferable to duplicating the game's durability manager or repeatedly scanning inventories.

### +50% Gratitude gain

The existing `RebalancedSoulContentment` implementation already identifies the exact live `soul_portal` chain:

`increase_gp_gain -> × coefficient -> +1`

and changes only the coefficient while leaving the native multiplication, rounding and zone-cap logic authoritative.

For the new candidate:
- change the projected coefficient from 0.2 to **0.5**;
- keep native `increase_gp_gain=1`;
- no new award formula is needed.

### Duration

Technically, arbitrary float `dur_parameter` values are supported and already pass through the stock prayer-buff duration path.

Accepted duration axis: **1 / 2 / 3 vanilla six-day weeks = 45 / 90 / 135 real-time minutes**. Graveyard Keeper's vanilla day is 7.5 real-time minutes, so one six-day week is 45 minutes.

### Runtime gate

Focused acceptance should prove:
1. soul still inside a corpse does not lose soul durability while Contentment is active;
2. extracted Soul item also does not lose durability;
3. the corpse itself still decays normally;
4. after Contentment expires, soul decay resumes;
5. released/healed soul Gratitude receives +50% with native rounding/cap behavior.

A small dedicated test/probe may be enough; user runtime testing should be requested only if static/probe evidence cannot close these live-state properties.

## 4. Soul's Repose — native sermon payout can own the Faith side

### Problem with the current event

Current `pray_for_souls_1/2/3` uses the special base:

`(Church Quality + Soul Gratitude) * 0.1 * EloquenceFactor`

The new design no longer wants hidden SG scaling in the base. It wants:
- ordinary church-derived sermon Faith;
- a clear success-only 1:1 SG -> Faith transaction.

### Ordinary base without a custom Faith formula

The verified event catalogue shows:
- `default_1/2/3` use ordinary church-derived Faith;
- `pray_for_souls_1/2/3` differ in the Faith expression, while the corresponding people/money structure is otherwise compatible for this role.

Preferred production candidate: project Soul's Repose tiers to the corresponding `default_1/2/3` event IDs rather than authoring a replacement formula.

That returns base Faith ownership to the standard sermon event.

### Native success-only conversion reward

`PrayLogics.CalculatePray()` is the semantic owner of:
- success chance;
- success/failure roll;
- fixed Faith/money prayer outputs;
- percentage bonuses;
- `last_pray_result`.

Direct code search finds the sermon flow calls `PrayLogics.CalculatePray` through the one `Flow_CalculatePrayEvent` node.

Preferred implementation:
1. Prefix `PrayLogics.CalculatePray`.
2. Require selected prayer family exactly `b_souls`.
3. Read current Soul Gratitude and selected tier cap 30/60/90.
4. Compute `conversion = min(current Gratitude, tier cap)`.
5. Temporarily add/project a fixed `faith` output equal to `conversion` on the selected live craft.
6. Let stock `CalculatePray` execute.
7. Stock success logic automatically:
   - includes conversion in `faith_bonus` on success;
   - gives zero conversion bonus on failure;
   - records the correct value in `last_pray_result`.
8. Restore the exact original craft output in a finalizer.
9. Only when the returned result succeeded, subtract exactly `conversion` Soul Gratitude once.

This preserves the stock sermon Faith-spread/report path. The mod does **not** manually spawn Faith after the sermon.

### Resource deduction

The game already exposes `WorldGameObject.gratitude_points` as a clamped property. Remote Craft Control's native `CraftComponent.TrySpendPlayerGratitudePoints` ultimately performs the same subtraction, but also sets craft-specific state.

For a sermon, use the player Gratitude property directly rather than pretending the sermon is a remote craft.

Safety rules:
- deduction occurs only after the native result is known to be successful;
- no deduction on failure;
- conversion is capped and never exceeds the pre-calculation current Gratitude;
- finalizer restores temporary craft data even if native calculation throws;
- no persistent custom “pending spend” state is needed across frames.

### Pulpit / tooltip feasibility

Existing `PrayerForecast.Build` already knows:
- selected craft and quality tier;
- current sermon success chance;
- current Soul Gratitude;
- current base/bonus resources.

Therefore the pulpit can compute the live transaction only when the pulpit redraws:

`[Soul Gratitude] -73 -> [Faith] +73`

No polling or additional UI lifecycle is required.

Because the new design no longer uses `pray_for_souls_*`, `UsesSoulGratitude` should stop being inferred only from the event-ID prefix and instead derive from the Rebalanced rule/mechanic identity.

Static item/Technology wording can continue through the existing Rebalanced semantic providers.

### Runtime gate

Focused acceptance should cover:
- below-cap conversion (e.g. 73 SG -> -73 / +73);
- above-cap conversion (e.g. 136 SG with Gold -> -90 / +90);
- zero-SG state;
- one failed sermon -> no SG deduction and no conversion Faith bonus;
- ordinary base Faith remains independent of current SG;
- report/visual payout sees the same conversion bonus produced by the native sermon path.

Because failure is probabilistic below 100%, a research harness or controlled low-quality case can be preferable to repeatedly asking the user to retry sermons.

## 5. Presentation architecture

### Donations

Existing success-resource rendering can show the new fixed money values. Use the game's native money formatter.

### Soul's Repose

This should be represented as a **success transaction**, not a timed “Prayer effect”.

Preferred pulpit placement:
`При успехе (N%): [Soul Gratitude] -X -> [Faith] +X`

The existing success heading makes a separate “failure does not spend Gratitude” warning unnecessary.

### Soul Contentment

Presentation needs two parts:
- soul condition does not decay while active;
- released souls give +50% Soul Gratitude.

The existing active-effect / Character Temporary Effects surfaces can display the duration through the current timer infrastructure.

### Thorough Cleansing

Existing tier-specific special-effect rendering can expose x2/x3/x4 and the normal duration.

## 6. Performance / lifecycle assessment

| Change | Runtime shape | Assessment |
| --- | --- | --- |
| Donations | once-per-load craft projection | **Very cheap** |
| Soul's Repose calculation | one scoped prefix/finalizer around one sermon calculation | **Very cheap / event-driven** |
| Soul's Repose pulpit | compute on pulpit redraw only | **Very cheap** |
| Thorough Cleansing | one scoped prefix/finalizer on player soul-heal action | **Very cheap / event-driven** |
| Soul Contentment +50% | one-time live graph coefficient projection | **Existing accepted shape** |
| Soul preservation | lightweight predicate inside native durability update | **Recurring but no scan; narrowest common decay owner found** |

The preservation hook is the only hot-path addition. It should be implemented with the cheapest checks first and must avoid reflection inside each durability call after installation (cache type/member access or use Harmony's typed/reflection-resolved target once).

## 7. Remaining questions before production

No fundamental mechanic question remains.

Accepted design decisions:
1. Soul Contentment duration = **45 / 90 / 135 minutes** (1 / 2 / 3 vanilla weeks);
2. RU preservation wording: **«Состояние душ не ухудшается со временем - ни в теле, ни после извлечения.»**;
3. compact Soul's Repose pulpit transaction is shown under the success row.

Implementation should then proceed on a `dev/*` candidate branch.

## 8. Acceptance plan

Do not ask the user to re-test already-closed mechanics.

For the next runtime candidate, the minimum useful live evidence is:

- **Soul's Repose:** exact one-time SG spend and corresponding native Faith bonus;
- **Soul Contentment:** both soul forms preserved, corpse still decays, decay resumes after expiration;
- **Thorough Cleansing:** exact x2/x3/x4 at Bronze/Silver/Gold.

Where an automated/harness assertion can prove exact arithmetic more cheaply, use it. Human observation is useful primarily for the final pulpit wording/layout and for confirming the soul-decay behavior feels understandable in normal play.

## Verdict

**All currently desired changes are technically feasible on Graveyard Keeper 1.407 with no broad scan, no custom per-frame system and no need to replace the game's sermon or soul-healing algorithms.**

The design remains within PrayerClarity: Rebalanced's intended scope: the prayers keep recognizable thematic identities while their values/roles become worth the weekly sermon cost.
