# PrayerClarity Post-Audit Verdict

Date: 2026-09-19

Scope: post-audit lifecycle/evidence closure after the cross-project Graveyard Keeper engineering audit.

This document is a **closure record**, not a new redesign proposal. It records which previously questioned mechanisms were re-checked against the current accepted source, pinned Graveyard Keeper 1.407 behavior, accepted runtime evidence, save/load lifecycle, and the current DevRules host-native-first contract.

## Baselines

### PrayerClarity: Vanilla

- accepted/released version: **1.0.25**
- accepted ref: `accepted/vanilla-1.0.25`
- exact source: `ebe069b4ad202ae786af9c63ded0ffb00502cff7`
- DLL SHA-256: `72b2237607734d8b50d666cef56e18d458b88c6a456211d496e2410ac646e3a9`
- release: `v1.0.25`

### PrayerClarity: Rebalanced

- accepted/released version: **0.2.3**
- accepted ref: `accepted/rebalanced-0.2.3`
- exact runtime source: `ab1eb67cbf2465a912c392120395011503b720c3`
- DLL SHA-256: `03a4a8b43c8a5ffef8ec62eac58370d3f6bced347bc2fb4f49a2ef81a23cb1ca`
- release: `rebalanced-v0.2.3`
- stable promotion merge: `f953e34901de139a5f7af7162af6fc62bb02b4d9`

No Rebalanced runtime source changed between the accepted 0.2.3 source and the current `main`; later differences are documentation/repository/CI housekeeping only.

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

Vanilla 1.0.25 remains a Clarity/UI-only accepted release. No gameplay architecture action is indicated.

The same terminal Repose endpoint presentation case remains optional/non-blocking.

## Production instrumentation

The production Rebalanced project does not compile the research Test Console and no research diagnostic hotkeys or `FEATURE_DIAGNOSTIC` probes are part of the released DLL.

One stale informational startup sentence in the 0.2.3 source still says:

`Runtime behavior is development-only until accepted.`

That sentence is now factually obsolete, but it has no gameplay/lifecycle effect. Do **not** create a new version solely to change this log line; correct it with the next real Rebalanced release.

## Repository hygiene

Immutable accepted/candidate evidence refs should remain.

The repository still contains multiple historical mutable `dev/*`, `fix/*`, and `research/*` branches. This is branch hygiene, not a production/runtime problem. Cleanup should be handled separately and conservatively: delete only branches whose useful evidence is already preserved in accepted/candidate refs or canonical documentation.

No hosted CI is required for this documentation-only post-audit closure.

## Final decision

No production bug was discovered.

Therefore:
- no production source change;
- no version bump;
- no new build;
- no new release;
- no new runtime test;
- no balance change.

Future PrayerClarity architecture audits should start from this verdict rather than re-litigating the closed mechanisms above without new evidence.
