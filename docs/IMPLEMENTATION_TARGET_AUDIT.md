# PrayerClarity: Rebalanced — implementation target audit

Status: **research / architecture evidence complete enough to enter `dev/*`**, updated 2026-09-17 against PrayerClarity: Vanilla 1.0.20. This document identifies the accepted implementation seams. It does **not** make Rebalanced runtime behavior accepted; each implemented mechanic still requires executable/runtime verification before release.

Target game: Graveyard Keeper 1.407, `Assembly-CSharp` MVID `6f50b8e7-156b-49ac-bbe8-7505894b2364`.

Accepted presentation/runtime base: PrayerClarity: Vanilla **1.0.20**, source `c7ac91c1cea6c498fb406323725768b605d8139f`.

Locked gameplay intent is in `PRAYER_REBALANCE_OPTIONS.md`. Stock behavior remains canonical in `PRAYER_MECHANICS.md`.

## Primary architecture — one effective rule source, native engine where possible

PrayerClarity: Vanilla 1.0.20 already has one shared presentation path centered on `PrayerForecast`:

- Pulpit -> `PrayerForecast.Build(...)`;
- Technology and prayer-item tooltips -> `PrayerForecast.BuildTierDetails(...)`;
- Temporary Effects -> `PrayerForecast.BuildActiveBuffText(...)`.

PrayerClarity: Rebalanced must add one declarative effective-rule source:

`stock prayer definition + locked Rebalanced override -> EffectivePrayerDefinition`

The same resolved rules supply:

1. all Clarity presentation surfaces;
2. safe projections into live native definitions where stock behavior is already correct;
3. narrow consumer hooks only where the stock data model cannot represent the locked behavior safely.

Do not maintain a second UI-only table of Rebalanced numbers. Do not replace the native sermon engine with a parallel calculator.

## Per-save projection lifecycle — closed

Independent accepted GK 1.407 runtime evidence establishes a clean mutation window:

`PrepareScene -> ClearCraftsListOnGameStart -> first CraftComponent.FillCraftsList() -> StartPlayingGame`

Use the first `CraftComponent.FillCraftsList()` of each loaded save as the one-shot projection boundary; reset the one-shot guard on return to Main Menu. Apply absolute/idempotent values only. No polling or recurring scans.

### Live object identity — accepted runtime evidence

Lookup Identity Probe 0.1.0 confirmed for all 72 live `pray:*` rows:

- `lookupNull=0`;
- `ReferenceEquals(craft_data row, GetDataOrNull(id)) == true` for every row;
- the `output` list reference is also identical;
- `PASS=true`.

Therefore live `CraftDefinition` mutation is visible through normal GameBalance lookups and output-list projection needs no cache rebuild.

## Safe native/static prayer projections

Project from the effective-rule source into live prayer `CraftDefinition`s when stock `PrayLogics.CalculatePray` already owns the desired behavior:

- Faith: q25/40/70, `k_faith=2.5/3.5/4.5`, remove prayer-owned off-theme/fixed money and fixed Faith outputs;
- Donations: q25/40/70, `k_money=2.5/3.5/4.5`, remove prayer-owned Faith contribution, retain +1/+2/+3 silver fixed money;
- BSS Soul's Repose: q25/40/70, `k_faith=2.5/3.5/4.5`, remove prayer-owned fixed/off-theme money while preserving the Souls event formula and recipe;
- Imagination Silver/Gold: add 3 Silver / 3 Gold Stories through the normal successful-prayer output list;
- stock durations/q values that remain unchanged need no projection.

Failed-sermon base donations remain untouched by permanent product policy.

## Hard no-go — tier-dependent `BuffDefinition.res`

`BuffsLogics.AddBuff` applies the current `BuffDefinition.res`, while `RemoveBuff` later subtracts the then-current definition value; saved `PlayerBuff` does not retain the original resource delta.

Therefore changing a shared `BuffDefinition.res` by prayer quality can create permanent stat drift after save/load or edition changes.

**Never implement tier scaling by mutating shared `BuffDefinition.res`.**

Non-resource buff behavior such as `tick_period` / `se_tick` can be projected separately because it does not create that AddBuff/RemoveBuff asymmetry.

## Active prayer quality persistence

Vanilla `PlayerBuff` does not persist source prayer quality. Capture Bronze/Silver/Gold while the successful prayer craft is still available around:

`PrayCraftGUI.DoPrayForBuff -> PlayerComponent.StartPrayAnimation -> CreatePrayBuffFlyingObject`

Store namespaced tier/effect values in the serialised player `Item._params` / `GameRes` already proven to accept arbitrary keys.

Tokens do not need expiry cleanup if every custom mechanic is additionally gated by its corresponding live vanilla buff/state. A stale token without the vanilla active marker is inert and PrayerClarity: Vanilla ignores all Rebalanced tokens.

## Presentation integration

Keep the accepted 1.0.20 layouts and reveal boundary.

- Pulpit remains dependency/prayer-owned contribution, never exact final Faith/money payout before sermon.
- Technology keeps generic shared-information + Bronze/Silver/Gold snapshot rendering.
- Item tooltip remains current-tier only.
- Temporary Effects resolves effective values from the shared rule source + persisted tier when stock definitions cannot express them safely.

No prayer-specific manual layout branch should be required for reasonable new values/effects.

## Prayer-specific implementation targets

### Shoots & Roots (`b_plant` / `buff_plant`) — closed target

Stock bug: activation writes player `buff_plant=1`, while affected growth `CraftDefinition.craft_time` expressions read `WGOpar("buff_plant")` on the growing/workbench WGO.

Use native `SmartExpression.ParseExpression(string)` to replace the whole affected `craft_time` expression at the per-save projection boundary, preserving every vanilla term except the broken prayer term.

Semantic replacement:

`- Ppar("buff_plant") * Ppar("prayerclarity_plant_reduction")`

Persisted reduction values: `.20/.30/.40`. Do not use external timers or broad WGO scans.

### Repentance (`b_sins` / `buff_sins`) — closed target

Keep the stock once-per-day `church_budka_roll` scheduler and confessional RNG graph.

Replace only the daily probability reset expression so stock 15% remains without the prayer and the active prayer yields 50/75/100%:

`SetPpar("confession_probability", 0.15 + Ppar("buff_sins") * Ppar("prayerclarity_confession_bonus"))`

Persisted bonus values: `.35/.60/.85`.

Parse through the native SmartExpression parser and fail closed if the expected expression/lifecycle is unavailable.

### Repose (`b_skull` / `buff_skull`) — closed target

`GameSave.GenerateBody(tier_min, tier_max, ...)` chooses a stock random `BodyDefinition` inside the supplied inclusive tier range.

Locked ladder can therefore preserve the stock generator:

- Bronze: pass stock range unchanged;
- Silver: 50% force `tier_min=tier_max`, otherwise stock range;
- Gold: always force `tier_min=tier_max`.

This yields Silver exactly `0.5 + 0.5 * P_stock(best)` while preserving stock random choice inside the selected tier.

#### Caller isolation — accepted runtime evidence

A live `npc_donkey` graph contains exactly three `Flow_DropBody` nodes.

- one story/intro node has no dynamic `Tier min` / `Tier max` connections and is fed from a talk chain;
- exactly two ordinary-delivery nodes have `Tier min` and `Tier max` connected through `IntegerAdd` nodes fed by player parameters; they also receive the dynamic soul-tier inputs;
- an actual ordinary donkey delivery independently executed a `Flow_DropBody` owned by graph `npc_donkey`, controller/WGO `[wgo] donkey`, `obj_id=donkey`.

Runtime node numeric IDs/UIDs are not stable enough to be production identity; the execution probe and load-only graph probe produced different identifiers.

**Production predicate:** operate only on a `Flow_DropBody` whose owner graph is `npc_donkey` / donkey WGO and whose `Tier min` and `Tier max` inputs have the ordinary-delivery dynamic player-param connection fingerprint. Never globally patch `GameSave.GenerateBody` based only on Repose being active.

### Combat (`b_sword`; `b_shield` legacy alias) — closed target

Locked package: +5/+10/+15 damage, +4 armor, 1/2/4 HP/sec.

Keep stock `buff_sword.res=+5 damage` and `buff_shield.res=+4 armor`.

#### Regeneration

Use native `PlayerBuff.CustomUpdate` tick behavior, not PrayerClarity polling/coroutines:

- `tick_period=1s`;
- `se_tick=AddPpar("hp", Ppar("prayerclarity_combat_regen"))`;
- persisted regen values 1/2/4.

Only canonical Combat gets the regen tick; legacy Protection must converge on the same effective lifecycle and must not create a second tick.

#### Damage

Implementation Seam Probe 0.1.0 closed the outgoing damage seam: `WorldGameObject.GetDamage(DamageType)` has a player-only path that reads weapon calculated damage and then adds player parameter `add_damage`.

Use that native extension seam. Rebalanced should maintain an effective active `add_damage` of +5/+10/+15 while Combat is active. Do not patch attack animations, hit colliders, generic `damage`, generic `GetParam`, or every hit.

### Combat alias / Protection retirement — closed target

Preserve all `b_shield:*` item/definition IDs for existing saves and old inventory items. Do not migrate or delete them.

For **new crafting**:

1. remove `b_shield` and `@b_shield_2` from technology `Martial skills.crafts`;
2. if `TechDefinition._unlocks_list` is already materialised, remove only the corresponding cached `TechUnlock` rows as well;
3. set recipe definitions `b_shield` and `b_shield_2` to `hidden=true`.

Evidence:

- `TechDefinition.GetUnlocksList()` caches directly from `crafts` when `_unlocks_list` is null;
- desk `CraftComponent.FillCraftsList()` independently receives live recipe definitions from `GameBalance.GetCraftsForObject`;
- craft UI/interaction paths already honour `CraftDefinition.hidden`.

This hides duplicate new crafting/tech presentation while keeping the definitions resolvable for legacy items. No post-UI cleanup or save migration is needed.

### Imagination (`b_pen` / `buff_pen`) — closed target

Keep stock `craft_q=+0.7` for every quality. Add only the locked Silver/Gold Story rewards through live prayer output lists.

### Excellence (`b_star` / `buff_star`) — closed target

Locked `craft_q=+0.2/+0.5/+1.0` cannot be represented by mutating shared `BuffDefinition.craft_q` by tier.

Verified consumer seam: `CraftDefinition.GetMultiqualityResult` calls `CraftDefinition.GetBuffValue(buff_id)`.

Use a narrow effective-value override restricted to `buff_star`, Rebalanced edition, active vanilla buff and valid persisted tier token. The returned value comes from the same effective-rule source used by UI.

### Soul Contentment (`b_grat_points_incr`) — closed target

Stock `buff_gp_increase` safely remains `increase_gp_gain=1`; do **not** change its resource delta.

The loaded `soul_portal` graph contains one isolated prayer bonus chain:

`increase_gp_gain -> × 0.1 -> +1 -> multiply Soul Gratitude -> RoundToInt`

The `0.1` is its own graph constant node.

Use event-driven graph lifecycle rather than a global parameter hook: `WorldGameObject.CheckNeededAttachedScript()` loads the attached graph via `CustomFlowScript.GetGraph`, assigns it to the WGO `FlowScriptController`, sets blackboard and starts/resumes behaviour. A narrow postfix can detect `attached_script == "soul_portal"` and idempotently change only this isolated prayer coefficient `0.1 -> 0.2` on the live graph.

No tier token is needed because +20% is identical at all qualities.

### Thorough Cleansing (`b_sin_shard`) — no mechanic patch

Locked magnitude remains stock x2; quality only extends stock duration.

## Edition packaging — closed architecture

PrayerClarity: Vanilla remains the already published sibling:

- assembly/DLL: `PrayerClarity.dll`;
- BepInEx GUID: `nikich.graveyardkeeper.prayerclarity`;
- no balance mechanics.

PrayerClarity: Rebalanced should be a separate plugin artifact:

- assembly/DLL: `PrayerClarity.Rebalanced.dll`;
- BepInEx GUID: `nikich.graveyardkeeper.prayerclarity.rebalanced`;
- plugin name: `PrayerClarity: Rebalanced`;
- compile the same shared Clarity presentation/model source plus Rebalanced-only effective rules/mechanics files;
- do not depend on/install PrayerClarity: Vanilla as a base mod.

BepInEx exposes `BepInIncompatibility`. Rebalanced should declare incompatibility with the existing Vanilla GUID. Chainloader behavior is deterministic and safe for this one-way contract: if both editions are installed, Rebalanced is skipped with an incompatibility error and Vanilla remains active. Do **not** reuse the same GUID and do not create a hidden profile toggle.

User-facing installation contract remains: install **one edition**, not both.

## Research gate status

Closed before `dev/*`:

- roster/design/power-budget decisions;
- Vanilla 1.0.20 baseline and sibling-edition naming;
- shared semantic/UI architecture;
- per-save one-shot projection lifecycle;
- live prayer definition/output identity;
- persisted tier-token strategy and stale-token safety;
- no-go on tier-dependent `BuffDefinition.res`;
- Roots expression replacement;
- Repentance daily reset seam;
- Repose RNG and ordinary-donkey caller isolation;
- Combat regen and outgoing damage seams;
- Protection recipe retirement without deleting legacy IDs;
- Imagination output ownership;
- Excellence quality consumer seam;
- Soul Contentment exact coefficient and graph lifecycle seam;
- Thorough Cleansing no-op mechanics decision;
- sibling DLL/GUID/mutual-exclusion packaging.

There are no remaining material user design questions and no broad mechanics-discovery blocker before implementation.

## Next phase

Create a fresh `dev/*` branch from this research state. Implement in layers:

1. sibling Rebalanced plugin/bootstrap + effective-rule source;
2. per-save static projections and tier capture/persistence;
3. narrow custom mechanics consumers (Roots, Repentance, Repose, Combat, Excellence, Soul Contentment);
4. legacy Protection retirement;
5. Clarity semantic rendering from the same rules;
6. targeted build/runtime verification per mechanic;
7. integrated candidate only after the narrow mechanics tests pass.

Production behavior is not accepted until runtime evidence verifies the implemented candidate.