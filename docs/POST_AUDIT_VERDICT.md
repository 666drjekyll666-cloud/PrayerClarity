# PrayerClarity Post-Audit Verdict

Date: 2026-09-19

Scope: post-audit lifecycle/evidence closure after the cross-project Graveyard Keeper engineering audit.

This document is a **closure record**, not a new redesign proposal. It records which previously questioned mechanisms were re-checked against the current accepted source, pinned Graveyard Keeper 1.407 behavior, accepted runtime evidence, save/load lifecycle, and the current DevRules host-native-first contract.

## Baselines

### PrayerClarity: Vanilla

- accepted/released version: **1.0.31**
- accepted ref: `accepted/vanilla-1.0.31`
- exact source: `30b036f16dc6a7964f7ef72e2e3ececa5951c812`
- DLL SHA-256: `140a2b3bc21eaa0b9e95f344a8a9ab5f47d5b9aa37572a0b159d905c79351ba3`
- release: `v1.0.31`

### PrayerClarity: Rebalanced

- accepted/released version: **0.2.10**
- accepted ref: `accepted/rebalanced-0.2.10`
- exact runtime source: `30b036f16dc6a7964f7ef72e2e3ececa5951c812`
- DLL SHA-256: `1f7131bda66554bb18396bed28a5997a531c2a0b9a44df1a61071d2c7e9b78e1`
- release: `rebalanced-v0.2.10`
- stable promotion merge: `1d4e196f51f067b8d0127aafa8505cfc4ee88d40`

The architecture verdict was established on 0.2.3. Rebalanced 0.2.4 subsequently changed only the Repentance/Repose tier durations through the already accepted once-per-load `CraftDefinition.dur_parameter` projection. Rebalanced 0.2.10 retains those gameplay/save-lifecycle seams; its q60 Gold Repose change is another static `CraftDefinition.needs_quality` projection, while the shared Clarity refinements are event-driven UI/crafting-description hooks with no new persistent state, polling, or gameplay lifecycle owner. Therefore the **A — architecture/save-lifecycle clean** verdict carries forward unchanged.

The Vanilla files compiled by `PrayerClarity.csproj` that appear in the broader history range were rechecked by blob identity where relevant; the accepted shared runtime blobs remain identical on current `main`.

## Final status

- **Vanilla: A — no architecture action.**
- **Rebalanced: A — architecture/save-lifecycle clean; no production change required.**

The previous Rebalanced **B — runtime evidence gap only** status is closed. The remaining save/load question was resolved by direct static serialization/lifecycle evidence plus existing accepted runtime load-order evidence; a new manual save/reload test would not close a concrete remaining unknown.

## Architecture map

| Feature | Native owner | PrayerClarity seam | Persistent mod state? | Runtime frequency | Verdict |
| --- | --- | --- | --- | --- | --- |
| Static Rebalanced definitions | GameBalance CraftDefinition/BuffDefinition consumers | validated/idempotent once-per-load projection | shared runtime definitions, not save-owned mod state | once per save load | keep |
| Tier capture | player Item/GameRes save data | successful StartPrayAnimation postfix writes tier/scalars | yes, native player params | successful sermon only | keep |
| Repentance | stock confession FlowCanvas/RNG | effective `confession_probability` accessor projection while `buff_sins` is live | tier token only | confession probability read | keep |
| Combat damage | `WorldGameObject.GetDamage` | scoped nonserialized `totem_effect["add_damage"]` delta | tier token/scalar only | player damage calculation while buff live | keep |
| Combat armor | `HPActionComponent.DecHP` | scoped nonserialized `totem_effect["add_armor"]` +4 | tier token only | player incoming damage while buff live | keep |
| Combat regeneration | `PlayerBuff.CustomUpdate` / `BuffDefinition.se_tick` | native tick expression reads persisted regen scalar | regen scalar in player params | native buff tick while buff live | keep |
| Shoots & Roots | stock plant `craft_time` SmartExpression | scoped WGO `totem_effect["buff_plant"]` input | tier/reduction player params | verified plant DoAction while stock buff active | keep |
| Repose | ordinary Donkey Flow_DropBody -> native `GenerateBody` | narrow pending event context / tier_min narrowing | tier token; pending mode is transient only | ordinary Donkey body generation | keep |
| Excellence | native multiquality `GetBuffValue` consumer | exact `buff_star` effective value override | tier token only | linked-buff quality calculation | keep |
| Soul Contentment | live `soul_portal` FlowCanvas graph | exact-topology, idempotent 0.1 -> 0.2 coefficient projection | no tier state | attached-script lifecycle only | keep |

## Tier-state lifecycle — proven invariant

Main invariant:

> A PrayerClarity tier token may persist in the save, but it has no gameplay effect without the corresponding live prayer buff.

### Capture

`RebalancedTierState` captures a tier only after a **successful** `PlayerComponent.StartPrayAnimation` call.

It stores:
- plant tier + configured reduction;
- confession tier;
- Repose tier;
- Combat tier + regeneration scalar;
- Excellence tier.

A later successful sermon of the same prayer family overwrites that family's token/scalar. A different prayer family writes its own independent token and does not need to erase unrelated stale tokens.

### Native serialization

Direct Graveyard Keeper 1.407 evidence establishes the complete persistence path:

1. `WorldGameObject.SetParam` writes player parameters into the player's `Item._params` / `GameRes`.
2. `GameRes` is serializable and stores arbitrary parameter names and values in serialized `_res_type` / `_res_v` lists.
3. `GameSave.PrepareForSave()` assigns the complete `MainGame.me.player.data` Item to serialized `GameSave._inventory`.
4. On load, `GameSave.GetSavedPlayerInventory()` returns that Item.
5. `PlayerComponent.SpawnPlayer(..., inventory)` restores that saved inventory/player data to the newly spawned player.

Therefore PrayerClarity's custom tier/reduction/regen keys are not dependent on a custom serializer and survive through the same native path as other player parameters.

### Active buff serialization

`GameSave.buffs` is the native serialized `List<PlayerBuff>`; each `PlayerBuff` carries its buff ID/end time/tick state.

A loaded timed prayer therefore restores:
- the native live buff in `GameSave.buffs`;
- the corresponding PrayerClarity tier/scalar in player `GameRes`.

No mod-owned parallel persistence is required.

### Load ordering

Accepted 0.2.3 runtime evidence shows:

`PrepareScene -> ClearCraftsListOnGameStart -> PrayerClarity static projection applied -> StartPlayingGame`

Thus the projected Rebalanced definitions, including Combat's accepted `buff_sword.se_tick`, are installed before ordinary resumed gameplay/buff ticking.

## Stale-token isolation by consumer

### Repentance

The accessor override returns stock behavior unless:
- the requested parameter is exactly `confession_probability`;
- live `buff_sins` exists;
- captured tier is valid.

A stale confession tier cannot act alone.

### Combat damage and armor

`TryGetActiveCombatTier` requires:
- the WGO is the player;
- live `buff_sword` exists;
- tier is valid.

The scoped `add_damage` / `add_armor` projections are restored by finalizers.

A stale Combat tier cannot affect damage or armor.

### Combat regeneration

The regeneration expression is executed by native `PlayerBuff.CustomUpdate` only for a live `buff_sword`.

The scalar can remain persisted after expiry without creating an independent timer or update path.

### Shoots & Roots

The plant bridge first requires the live stock player `buff_plant` contribution before using the persisted reduction scalar.

A stale plant tier/reduction token cannot activate growth reduction by itself.

### Repose

Both arming and consuming the pending Repose mode require live `buff_skull`.

The transient pending mode is cleared around the body-generation call and is not save state.

### Excellence

Stock `CraftDefinition.GetBuffValue("buff_star")` returns zero when `buff_star` is not active. The Rebalanced postfix only replaces a positive stock result.

A stale Excellence tier cannot create a craft-quality bonus without the native buff.

### Soul Contentment

No tier token is used.

## Static-definition projection

The current projection remains appropriate:

- reset boundary is native `CraftComponent.ClearCraftsListOnGameStart`;
- projection is guarded once per load;
- failure disables further projection for that load rather than repeatedly mutating;
- output additions remove PrayerClarity-owned prior rows before re-adding, avoiding duplication;
- expression/definition paths validate expected structure or accepted already-projected state;
- the exact Graveyard Keeper 1.407 Assembly-CSharp MVID is verified before Rebalanced installs;
- runtime evidence confirms the projection happens before ordinary gameplay resumes.

There is no evidence that shared-definition cloning would improve correctness. Rebalanced intentionally changes shared balance definitions for the supported game build.

## Previously questioned mechanisms that remain closed

Do **not** reopen these merely because the patch name looks broad or because a theoretically more native mechanism might exist:

- Repentance semantic `confession_probability` interception;
- Combat scoped native modifiers;
- Roots stock-formula ownership and WGO input bridge;
- Excellence `GetBuffValue("buff_star")` seam;
- Soul Contentment exact live-graph coefficient projection;
- persistent tier tokens.

Reopen only when current source changes, a concrete runtime conflict appears, the supported game binary changes, or new direct evidence invalidates an existing assumption.

## Repose remaining evidence

The ordinary Repose gameplay path is not blocked.

The only known unresolved Repose item is the **terminal Donkey-progression endpoint wording/presentation** on a save that has reached the final ordinary corpse tier. This is presentation-only and non-blocking. Do not require the user to progress a save solely to close it.

## Vanilla verdict

Vanilla 1.0.31 remains a Clarity/UI-only accepted release. No gameplay architecture action is indicated.

The same terminal Repose endpoint presentation case remains optional/non-blocking.

## Production instrumentation

The production Rebalanced project does not compile the research Test Console and no research diagnostic hotkeys or `FEATURE_DIAGNOSTIC` probes are part of the released DLL.

One stale informational startup sentence was recorded in the 0.2.4 source:

`Runtime behavior is development-only until accepted.`

That historical sentence had no gameplay/lifecycle effect and was not a reason for a standalone release. Later accepted releases supersede that source state.

## Repository hygiene

Immutable accepted/candidate evidence refs should remain.

The repository still contains multiple historical mutable `dev/*`, `fix/*`, and `research/*` branches. This is branch hygiene, not a production/runtime problem. Cleanup should be handled separately and conservatively: delete only branches whose useful evidence is already preserved in accepted/candidate refs or canonical documentation.

No hosted CI is required for this documentation-only post-audit closure.

## Final decision

At the time of this post-audit closure, no architecture/lifecycle production bug was discovered and no architecture change was required.

### 2026-09-19 Rebalanced 0.2.4 addendum

A later balance decision changed Repentance and Repose duration from 18/36/54 to **30/42/54 minutes**. The accepted implementation reused the existing static prayer projection and did not alter the architecture audited here. Runtime presentation showed the expected 2.7/3.7/4.8 in-game-day values under the user's Longer Days +50% setup, and the exact accepted CI binary was published as `rebalanced-v0.2.4` without rebuilding.

### 2026-09-20 Rebalanced 0.2.10 / Vanilla 1.0.31 addendum

The accepted shared source `30b036f16dc6a7964f7ef72e2e3ececa5951c812` carries the closed gameplay/save-lifecycle architecture forward. Rebalanced ordinary Repose Gold now uses q60 through the existing static definition projection. Shared presentation additions (compact Base-result wording, item-tooltip hierarchy/spacing, Excellence stock-lore fallback, and active-effect/Technology text refinements) run only on the relevant UI/crafting-description paths and add no save-owned state or per-frame polling. The exact accepted binaries were published as `v1.0.31` and `rebalanced-v0.2.10` without rebuilding.

Future PrayerClarity architecture audits should start from this verdict rather than re-litigating the closed mechanisms above without new evidence.
