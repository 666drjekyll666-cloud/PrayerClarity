# PrayerClarity implementation target audit

Status: research evidence and implementation constraints for Graveyard Keeper 1.407. This document does not promote any balance candidate to accepted runtime behaviour and does not authorize production implementation by itself.

Runtime target used by the accepted research probes:

- `Assembly-CSharp` 11.0.0.0
- MVID `6f50b8e7-156b-49ac-bbe8-7505894b2364`

The product-layer distinction from `AGENTS.md` remains mandatory: Clarity is information-only, Vanilla Fixes restore directly evidenced broken vanilla behaviour, and Balance/Rework changes mechanics intentionally.

## Cross-cutting lifecycle constraints

### Do not tier-scale permanent buff resources by mutating `BuffDefinition.res`

Direct inspection of `BuffsLogics.AddBuff` and `BuffsLogics.RemoveBuff` establishes an unsafe asymmetry for this project:

- on first add, the game looks up the current `BuffDefinition` and applies `definition.res` to player parameters;
- the saved `PlayerBuff` records the buff ID and expiration state, not the resource delta that was applied;
- on removal, the game looks up the then-current `BuffDefinition` again and subtracts its current `res`.

Therefore changing a shared definition such as `buff_sword.res` from stock `add_damage +5` to a quality-dependent value can leave permanent parameter drift after save/load, configuration changes, or mod removal. A Gold application followed by stock removal is the canonical failure mode.

**No-go:** PrayerClarity must not implement quality scaling of passive prayer resources by globally rewriting `BuffDefinition.res`.

Preferred invariant: where possible, retain stock buff IDs and their stock permanent resources, and layer quality-dependent behaviour at the actual consumer or through another symmetric, explicitly persisted mechanism.

### Active `PlayerBuff` state cannot recover source prayer quality

The inspected `PlayerBuff` state contains the buff ID, end time, tick state and a definition lookup. It does not retain the source `CraftDefinition`, prayer book quality, or original duration parameter.

This means Bronze/Silver/Gold cannot be reconstructed reliably from an active buff after save/load solely from vanilla buff state.

PrayerClarity therefore needs its own tiny semantic tier token for each quality-sensitive active prayer. That token must survive save/load but must not replace or rewrite the vanilla buff itself.

### Capture tier while the prayer source craft still exists

The successful sermon handoff provides a narrow capture seam before the information is lost:

- `PrayCraftGUI.DoPrayForBuff(...)` calls `PlayerComponent.StartPrayAnimation(pray_craft, success)`;
- `StartPrayAnimation` retains `_pray_craft`, `_pray_buff`, and the success state;
- `PlayerComponent.CreatePrayBuffFlyingObject` still has `_pray_craft.dur_parameter` and the chosen buff before creating the actual player buff.

This is the correct source for recording prayer identity/tier rather than attempting to infer quality later from active-buff duration.

### Native player params are the preferred persisted tier store

The initial sidecar idea is no longer necessary. Static and accepted runtime evidence closes the persistence chain inside the game's own player state:

- the live player `Item` stores ordinary parameters in `Item._params`, whose runtime type is `GameRes`;
- `GameRes` is generic string-to-float storage using parallel `_res_type` and `_res_v` lists;
- `GameRes.Set(string,float)` and `GameRes.Add(string,float)` accept non-special arbitrary string keys: if a key is absent it is appended to those lists rather than rejected by a whitelist;
- `GameSave.PrepareForSave()` assigns the whole `MainGame.player.data` `Item` to `GameSave._inventory`;
- `GameSave.GetSavedPlayerInventory()` returns that saved `Item` on load;
- `GameSave.ToBinary()` serializes the `GameSave`, including `_inventory`, through `SmartSerializer.Serialize`.

Therefore a namespaced float player parameter can carry PrayerClarity's active tier with the same save ownership as the vanilla player data. Candidate keys should be explicit per buff, for example `prayerclarity_buff_pen_tier`, rather than one shared global tier that could be ambiguous if effects overlap.

Runtime reads must still be gated by the corresponding active vanilla buff. A stale tier token without `buff_pen`, `buff_star`, `buff_sword`, etc. is inert. This makes stale tokens harmless after natural expiry or mod removal and avoids invasive save-cleanup logic.

**Accepted architecture constraint:** do not create an external sidecar or depend on save-slot filenames for prayer tiers unless later evidence disproves native player-param persistence on the target build.

A separate save/load persistence probe is not currently justified; the relevant serialization and storage path is already directly evidenced.

## Clarity presentation target

### Pulpit decision surface

`PrayCraftGUI.RedrawTextValues(float needs_q, float chance)` is the strongest first target for decision-time clarity. Vanilla uses it to show church quality, prayer requirement and sermon success chance.

Candidate implementation:

- Harmony postfix on `PrayCraftGUI.RedrawTextValues`;
- read the currently selected `pray_craft` plus current church/graveyard/player state;
- ask one pure PrayerClarity semantic/forecast model for the concise effect summary;
- update or append presentation only when the pulpit selection is redrawn.

**No-go:** do not call `PrayLogics.CalculatePray` merely to preview a result. That method participates in live sermon calculation and RNG/global drop state; preview code must be side-effect-free.

The same semantic model should ultimately feed technology/item/pulpit/active-buff surfaces rather than duplicating prayer formulas in UI patches.

### Active-buff HUD

Probe 0.1.6 established that vanilla `BuffIcon.Draw` binds the `PlayerBuff`, sprite and timer; there is no existing quantitative prayer-effect tooltip to reveal. Active-buff hover/details are therefore a separate optional Clarity surface, not a prerequisite for the first decision-time prototype.

## Prayer-specific implementation targets

### Imagination (`b_pen` / `buff_pen`)

Verified calculation seam: `CraftDefinition.GetMultiqualityResult` collects quality contributions, including `CraftDefinition.GetBuffValue(string buff_id)`. `GetBuffValue` returns the linked active buff's `BuffDefinition.craft_q`.

Candidate Balance/Rework target:

- postfix `CraftDefinition.GetBuffValue(string buff_id)`;
- alter only the result for `buff_pen` when the Rebalanced profile is enabled and the active tier token is present;
- candidate values remain `+0.5 / +0.7 / +1.0` for Bronze/Silver/Gold.

This avoids mutating shared definitions and naturally falls back to stock behaviour when the mod is absent. The previously open persistence gate is now closed by the native player-param tier token.

### Excellence (`b_star` / `buff_star`)

Use the same `CraftDefinition.GetBuffValue` seam, restricted to `buff_star`.

Current Balance/Rework candidate remains `+0.2 / +0.5 / +1.0`.

The tier can use the same native player-param persistence pattern as Imagination.

### Combat (`b_sword`, legacy `b_shield`)

Stock verified resources:

- `buff_sword`: `add_damage +5`;
- `buff_shield`: `add_armor +4`.

The current rework candidate is `+5/+8/+12` damage, `+4` armor, and regeneration of `1 HP` every `3/2/1.5` seconds.

Save-safe direction:

- keep the stock sword and shield buff IDs/resources as the base mechanical state;
- canonical Combat can apply both stock buffs for the same prayer duration;
- legacy shield prayer can remain a save-safe alias instead of migrating saved buff IDs;
- capture the Combat tier in a namespaced player parameter at sermon application;
- add only the tier-specific extra damage (`+0/+3/+7`) at the actual damage consumer;
- add regeneration through a separate symmetric/tick mechanism gated by the active combat buff and persisted tier token.

This direction preserves a valid stock fallback if PrayerClarity is removed. The remaining Combat blockers are the exact damage-consumption seam and the cheapest correct regeneration scheduling seam, not persistence.

The stock Long Heal Potion is a useful mechanical reference: it heals `1 HP` per `1.5 s` tick, confirming that the Gold candidate intentionally reaches vanilla potion-grade regeneration speed rather than inventing a new scale.

### Shoots & Roots (`b_plant` / `buff_plant`)

The vanilla scope bug is directly evidenced in affected growth crafts: prayer activation writes player parameter `buff_plant`, while growth expressions read `WGOpar("buff_plant")` from the work object. The intended coefficient is `-20%` growth time.

`SmartExpression` natively supports both scopes. Its expression environment registers `WGOpar` and `Ppar`; the expression preprocessor maps `$name` to `Ppar("name")` and `@name` to `WGOpar("name")`. A custom evaluator is therefore unnecessary.

**Vanilla Fix candidate:** surgically repair the affected growth expressions to read the corresponding player prayer parameter while retaining the stock `-20%` effect for every quality tier.

**Rebalanced candidate:** `-20/-30/-40%` growth time, with the active tier held in a namespaced player parameter.

Do not conflate these layers. The stock scope repair and quality scaling are separate features.

Before implementation, prove the safe mutation/rebuild lifecycle for the already-loaded `SmartExpression` instances so the corrected source is actually recompiled without broad balance reloads.

### Repose (`b_skull` / `buff_skull`)

The donkey flowgraph chain is sufficiently closed to establish the stock mechanism:

- normal corpse `Tier min` is `body_min + add_body_min`;
- normal corpse `Tier max` is `body_max + add_body_max`;
- those values are passed directly to the relevant `Flow_DropBody` nodes;
- `buff_skull` raises player `body_max` by `+1`, so vanilla Prayer for Repose expands the upper tier bound used by corpse generation.

Current Balance/Rework candidate:

- Bronze: stock roll;
- Silver: halfway from the vanilla best-tier probability to certainty, `P_silver = 0.5 + 0.5 * P_vanilla`;
- Gold: guarantee the best prayer-eligible tier;
- never exceed `story max + 1`.

This should be implemented at the corpse-tier selection/generation seam rather than by rewriting donkey progression parameters globally.

**Open blocker:** inspect the exact corpse-tier selection/RNG implementation used after `Flow_DropBody` receives `tier_min/tier_max`, so Silver and Gold can wrap the actual stock roll instead of approximating it.

### Repentance (`b_sins` / `buff_sins`)

The prayer applies `buff_sins` for `18/36/54` minutes, but the accepted consumer audit did not find a literal consumer in code or the previously loaded graph set. Runtime evidence shows periodic execution of `church_budka_roll`, which is the strongest remaining lead for confessional behaviour.

No balance implementation target is accepted yet.

If Repentance is included in the first rework build, inspect `church_budka_roll` directly before patching anything. Otherwise defer it instead of broadening the first prototype.

### Faith, Donations, Combo, Ordinary, Prosperity and BSS prayers

These do not introduce the same persisted passive-tier problem when their proposed behaviour is calculated at sermon/result time or remains stock. Their exact implementation still must reuse the common semantic model and verified sermon formulas rather than duplicating guessed constants.

The permanent donation policy remains unchanged: PrayerClarity must not 'fix' failed-sermon base donations. In stock 1.407 the downstream integer random range makes that path pay full base donations, and project policy intentionally preserves it in every profile.

## Recommended staging

The cross-cutting active-tier persistence problem is no longer a blocker. The implementation audit now supports a deliberately narrow first `dev/*` prototype without pretending every rework has a closed target.

A low-risk first implementation slice can contain:

1. the shared pure semantic/forecast model and pulpit Clarity surface;
2. sermon-time candidate tuning whose mechanics are already directly evidenced;
3. native player-param tier capture at the successful prayer-application seam;
4. Imagination and Excellence through `CraftDefinition.GetBuffValue`, because both their calculation seam and tier persistence path are now closed.

Combat, Roots, Repose and Repentance should remain out of the first runtime slice until their remaining exact consumer/rebuild/generator seams are closed. This is staging by evidence and implementation risk, not by prayer importance or final product scope.

## Remaining integration-gate questions

The next research work should answer only blockers that materially affect later slices:

- What exact consumer should receive Combat's tier-specific extra damage without mutating `buff_sword.res`, and what existing lifecycle is the cheapest correct home for regeneration ticks?
- What exact corpse-tier selection/RNG algorithm should Repose's Silver/Gold policy wrap?
- How can the affected Roots `SmartExpression` source be replaced and its compiled state safely rebuilt without a broad balance reload?
- If Repentance is in scope for the next slice, what does `church_budka_roll` actually read and write?

Prefer existing accepted static data before any new probe. Use a narrowly scoped probe only when the current evidence cannot answer one of these concrete questions, and do not spend hosted CI on general exploration.
