# PrayerClarity: Rebalanced — implementation target audit

Status: **research / architecture evidence**, updated 2026-09-17 against PrayerClarity: Vanilla 1.0.20. This document identifies safe implementation seams; it does not make Rebalanced runtime behavior accepted.

Target game evidence remains Graveyard Keeper 1.407, `Assembly-CSharp` MVID `6f50b8e7-156b-49ac-bbe8-7505894b2364`.

Accepted presentation/runtime base: PrayerClarity: Vanilla **1.0.20**, source `c7ac91c1cea6c498fb406323725768b605d8139f`.

Locked gameplay intent is in `PRAYER_REBALANCE_OPTIONS.md`. Stock behavior remains canonical in `PRAYER_MECHANICS.md`.

## Primary architecture finding — one rule set, native engine where possible

PrayerClarity: Vanilla 1.0.20 already has one shared presentation path centered on `PrayerForecast`:

- Pulpit calls `PrayerForecast.Build(...)`;
- Technology and prayer-item tooltips call `PrayerForecast.BuildTierDetails(...)`;
- Temporary Effects calls `PrayerForecast.BuildActiveBuffText(...)`.

`BuildTierDetails` currently reads the live stock prayer `CraftDefinition` directly: `needs_quality`, `k_faith`, `k_money`, outputs, `buff`, `dur_parameter`, and linked event. `BuildSpecial` then reads relevant `BuffDefinition` data. This is correct for PrayerClarity: Vanilla, but several locked Rebalanced effects cannot be represented safely by stock fields alone.

The Rebalanced edition therefore needs **one declarative effective-rule source**, not separate gameplay and tooltip tables:

`stock prayer definition + locked Rebalanced override -> EffectivePrayerDefinition`

The same resolved rule object supplies:

1. presentation semantics for Pulpit, Technology, item tooltip and Temporary Effects;
2. safe per-save projections into native definitions where the stock engine already owns the behavior;
3. narrow consumer hooks only for effects the stock data model cannot express safely.

Do **not** create a second manually maintained table of Rebalanced numbers in UI code. Do **not** replace the native prayer engine with a parallel calculator.

## Per-save static projection lifecycle

### Runtime load boundary is now evidenced

Independent accepted runtime evidence from another Graveyard Keeper 1.407 production mod establishes a usable definition-mutation window on the same target build:

`PrepareScene -> ClearCraftsListOnGameStart -> first CraftComponent.FillCraftsList() -> StartPlayingGame`

That mod snapshots and mutates live `GameBalance.me.craft_data` during the first `FillCraftsList()` of each loaded save, then resets its one-shot guard on return to Main Menu. The mutation is observed on repeated existing-save/new-save loads and requires no polling.

For PrayerClarity: Rebalanced the preferred lifecycle is therefore:

- patch the first `CraftComponent.FillCraftsList()` per loaded save;
- resolve only the exact prayer/consumer definitions that PrayerClarity owns;
- apply absolute/idempotent values from the locked rule set once;
- perform no recurring scan or reapplication until another save is loaded.

This is better than waiting in `Update()`, scene polling, or maintaining a separate copied balance database.

### Remaining identity proof

One binary fact remains before treating live `CraftDefinition` projection as closed production architecture: prove that `GameBalance.GetData/GetDataOrNull(id)` returns the same live `CraftDefinition` instance stored in `craft_data`, rather than a cloned/cached copy.

A read-only **Lookup Identity Probe 0.1.0** exists solely for this question:

- source/ref: `candidate/lookup-identity-probe-0.1.0` at `b752e624baf826c5bb2ed6b6454f4f380bb4cc32`;
- CI run `35218014297`, artifact `10494968504`;
- no Harmony, no balance mutation, no save writes;
- compares `ReferenceEquals(craft_data_row, GetDataOrNull(id))` for every `pray:*` definition and also reports output-list identity.

Until that runtime result is returned, the lifecycle seam is **strongly supported but not fully closed**.

## Safe native/static prayer fields

Once object identity is confirmed, the least-mechanism implementation for stock-owned sermon fields is to project the locked rule into the live `CraftDefinition` at the per-save load boundary.

Candidate static projections from the single effective rule set:

- Faith: q25/40/70, `k_faith=2.5/3.5/4.5`, zero off-theme `k_money`, remove prayer-owned fixed Faith/money outputs;
- Donations: q25/40/70, `k_money=2.5/3.5/4.5`, zero prayer-owned Faith bonus/fixed Faith, retain +1/+2/+3 silver fixed money;
- BSS Soul's Repose: q25/40/70, `k_faith=2.5/3.5/4.5`, remove prayer-owned fixed/off-theme money contributions while preserving the Souls event formula and recipe;
- Imagination Silver/Gold: add the locked 3 Silver / 3 Gold Story success reward through the normal prayer output path after output-list mutation is verified against the live lookup object;
- ordinary duration/q values that remain stock require no projection.

This lets vanilla `PrayLogics.CalculatePray` remain authoritative for the behaviors it already implements and lets the accepted Clarity surfaces read the same live/effective definitions.

## Hard no-go — quality-dependent `BuffDefinition.res`

Direct inspection of `BuffsLogics.AddBuff` / `RemoveBuff` established an unsafe asymmetry:

- AddBuff applies the **current** `BuffDefinition.res` to player parameters;
- saved `PlayerBuff` does not retain the resource delta that was applied;
- RemoveBuff later subtracts the **then-current** definition resource.

Changing shared resources such as `buff_sword.res` from +5 to +15 by prayer tier can therefore leave permanent parameter drift after save/load, edition removal or definition changes.

**Do not tier-scale passive prayer resources by rewriting `BuffDefinition.res`.**

Non-resource `BuffDefinition` behavior such as `tick_period` / `se_tick` can be considered separately because it does not create an AddBuff/RemoveBuff resource delta.

## Active prayer quality persistence and live-state gating

### Vanilla `PlayerBuff` loses source prayer quality

The inspected `PlayerBuff` state contains buff ID, end time, tick state and definition lookup, but not the source prayer `CraftDefinition`/item quality. Bronze/Silver/Gold therefore cannot be reconstructed reliably from an active buff after save/load.

### Capture seam

The source craft still exists during successful prayer application:

- `PrayCraftGUI.DoPrayForBuff(...)` -> `PlayerComponent.StartPrayAnimation(pray_craft, success)`;
- `StartPrayAnimation` retains `_pray_craft` / `_pray_buff` / success state;
- `PlayerComponent.CreatePrayBuffFlyingObject` still has the selected craft and duration before the actual player buff is created.

This remains the narrow tier-capture boundary.

### Persisted token store

Direct evidence shows player `Item._params` is a serialised `GameRes` that accepts arbitrary string keys and is persisted with player inventory. Namespaced per-effect float parameters remain the preferred tier store.

A crucial simplification is now explicit: **the custom tier token does not need to be cleared on buff expiry if every custom mechanic is gated by the corresponding vanilla live buff/state.** A stale token without the vanilla active marker is inert.

Examples:

- `buff_plant` itself adds/removes player `buff_plant=1`; the custom plant tier token supplies only reduction magnitude;
- `buff_sins` itself adds/removes player `buff_sins=1`; the custom confession token supplies only the bonus above stock 15%;
- Combat/Excellence/Repose read their token only while the corresponding stock buff/effect is active.

This avoids save-cleanup logic and leaves mod removal structurally safe. PrayerClarity: Vanilla ignores Rebalanced tokens entirely.

## Presentation integration in 1.0.20

The current 1.0.20 UI surfaces remain valid. Rebalanced changes the semantic model below them, not their layout ownership.

- **Pulpit:** `PrayCraftGUI.RedrawTextValues(...)` remains the accepted redraw seam. If q is projected before the pulpit opens, vanilla success chance remains authoritative.
- **Technology:** keep the existing generic Bronze/Silver/Gold renderer; feed it effective tier data.
- **Prayer item tooltip:** keep the existing current-tier-only behavior.
- **Temporary Effects:** resolve quality-sensitive values from effective rule + persisted tier, not from tier-mutated `BuffDefinition.res`.

The accepted exact-reward reveal boundary remains unchanged.

## Prayer-specific implementation targets

### Shoots & Roots (`b_plant` / `buff_plant`) — lifecycle blocker substantially closed

Verified stock bug: activation writes player `buff_plant=1`, while affected growth `CraftDefinition.craft_time` expressions read `WGOpar("buff_plant")` on the growing/workbench WGO. Direct audits found the relevant consumers in `craft_time`; examples include ordinary crops, vineyard/refugee planting and world growth/respawn expressions.

The previous question "how do we recompile an already-loaded SmartExpression?" no longer needs an internal compiler mutation. `SmartExpression.ParseExpression(string)` exists, and the correct low-risk path is to replace the entire affected `craft_time` field with a newly parsed expression at the same per-save projection boundary.

Implementation shape:

- preserve each vanilla expression and its other terms exactly;
- replace only the prayer term so it reads player state instead of WGO state;
- multiply by the persisted Rebalanced magnitude while gated by stock `Ppar("buff_plant")`;
- target magnitude .20/.30/.40 by prayer tier;
- never replace plant growth with an external timer or broad WGO scan.

Example semantic term:

`- Ppar("buff_plant") * Ppar("prayerclarity_plant_reduction")`

The exact transformed source must be generated from the known affected expressions rather than by a blind global text replacement.

### Repentance (`b_sins` / `buff_sins`) — native daily scheduler can stay intact

Verified stock lifecycle:

- `LogicDefinition church_budka_roll` executes `SetPpar("confession_probability", 0.15)`;
- then it runs stock script `church_budka_roll`;
- cadence is one in-game day (`period_time=1`, fixed `start_time=2`);
- each existing confessional independently removes its prior availability state, rolls, compares against the player probability, and adds `confession_available` on success.

Locked Rebalanced probability is 50/75/100%.

Preferred target is therefore **not** the RNG and not a once-at-sermon assignment. Replace only the daily reset SmartExpression at the per-save definition projection boundary so stock scheduling and stock confessional graphs remain untouched.

Semantic form:

`SetPpar("confession_probability", 0.15 + Ppar("buff_sins") * Ppar("prayerclarity_confession_bonus"))`

Tier bonus values are .35/.60/.85. With no active prayer, stock `buff_sins=0` and the exact stock 15% baseline remains.

Before production, parse the exact expression through the native SmartExpression parser and fail closed if unavailable; do not guess syntax at runtime.

### Repose (`b_skull` / `buff_skull`) — RNG algorithm closed, caller isolation remains

Direct IL closes the actual corpse RNG:

`GameSave.GenerateBody(tier_min, tier_max, ...)` builds the list of all `BodyDefinition`s whose tier is inside the supplied range, then chooses one through the stock `RandomElement` path before generating the corpse item.

This means the locked reliability ladder can be represented exactly without rewriting corpse generation:

- Bronze: pass stock range untouched;
- Silver: 50% force `tier_min = tier_max`, otherwise pass the stock range;
- Gold: force `tier_min = tier_max`.

Silver then equals exactly `0.5 + 0.5 * P_stock(best)`, while the game still randomly selects among vanilla definitions within the chosen best tier.

Donkey graph evidence already shows normal delivery derives its tier range from `body_min/body_max` plus progression offsets.

**Remaining Repose blocker:** isolate the exact normal-donkey/`Flow_DropBody` call context. Do not globally modify every `GameSave.GenerateBody` call merely because Repose is active; story/special body generation must remain unaffected.

### Combat (`b_sword`; `b_shield` legacy alias) — native buff tick is the regeneration seam

Locked package remains +5/+10/+15 damage, +4 armor and 1/2/4 HP/sec.

Stock resources remain untouched:

- `buff_sword.res` stays +5 damage;
- `buff_shield.res` stays +4 armor.

Native `PlayerBuff.CustomUpdate(deltaTime)` already:

- respects stopped game time;
- accumulates native buff tick time;
- reads `BuffDefinition.tick_period`;
- executes `BuffDefinition.se_tick` when due;
- observes hp/energy/money changes through the game's existing effect path.

Vanilla itself uses the same mechanism for long-heal potion regeneration (`AddPpar("hp",1)` every 1.5 s).

Therefore Combat regeneration does **not** need PrayerClarity polling, a coroutine, or a new scheduler. The low-mechanism target is to give canonical `buff_sword` a Rebalanced-session tick behavior while leaving its resource delta stock:

- `tick_period = 1s`;
- `se_tick = AddPpar("hp", Ppar("prayerclarity_combat_regen"))`;
- token values 1/2/4.

Do not give `buff_shield` a second regen tick; legacy/canonical Combat must converge on one sword+shield effective lifecycle so regeneration cannot double-stack.

**Remaining Combat blocker:** find the narrow outgoing player-damage consumer for Silver/Gold's additional +5/+10 damage beyond stock +5. Do not patch generic `GetParam`/`GameRes.Get` globally.

### Combat alias / Protection retirement

Direct balance evidence now identifies the stock duplicate craft and technology ownership:

- `b_shield` / `b_shield_2` are visible desk/desk_2 recipes using Hard Book +7 Faith;
- technology `Martial skills` exposes `b_sword`, `b_shield`, `@b_sword_2`, `@b_shield_2` together.

Rebalanced should preserve all existing `b_shield:*` item IDs as legacy Combat aliases. Do not migrate or delete saved items.

For **new crafting**, the preferred direction is to retire the duplicate Protection recipe from the technology/crafting presentation while keeping its definitions resolvable for old items. Before production, verify `TechDefinition.GetUnlocksList` caching and the desk recipe-list source so the removal happens before those lists are materialized and does not require post-UI cleanup.

### Imagination (`b_pen` / `buff_pen`)

Locked `craft_q` is +0.7 at all qualities, identical to stock special magnitude. No tier mutation of `buff_pen.craft_q` is required.

Only Silver/Gold Story rewards change. Project those success rewards through the prayer craft output list once live definition identity/output ownership is confirmed.

### Excellence (`b_star` / `buff_star`)

Locked special magnitude is +0.2/+0.5/+1.0. Because `BuffDefinition.craft_q` is shared and active `PlayerBuff` loses source quality, do not mutate that field by tier.

Verified quality seam: `CraftDefinition.GetMultiqualityResult` calls `CraftDefinition.GetBuffValue(buff_id)`.

Preferred target remains a narrow override/postfix restricted to `buff_star`, active Rebalanced edition and a valid persisted tier token. The return value comes from the same effective rule object used by UI.

### Soul Contentment (`b_grat_points_incr`) — consumer graph isolated

Stock `buff_gp_increase` adds player `increase_gp_gain=1`. Direct graph evidence shows the relevant consumer in loaded `soul_portal` only: the graph reads `increase_gp_gain`, multiplies it into the prayer bonus branch, adds 1, and applies that multiplier to the Soul Gratitude result before rounding.

Locked Rebalanced magnitude is +20% at all qualities, so arithmetic itself needs no tier token.

**Do not** change `buff_gp_increase.res` from 1 to 2; the same AddBuff/RemoveBuff resource-drift risk applies. The remaining implementation question is the narrowest safe way to change that consumer coefficient from the stock +10% path to +20% — preferably at the soul-portal consumer rather than through a global parameter hook.

### Thorough Cleansing (`b_sin_shard`)

Locked magnitude remains stock x2; quality only extends stock duration. No main-mechanic patch is required.

## Edition composition

The public editions share presentation/model source but are not a hidden runtime profile toggle.

Preferred boundary:

- common Clarity/presentation source;
- PrayerClarity: Vanilla installs stock semantic readers and accepted Clarity UI patches only;
- PrayerClarity: Rebalanced additionally installs the effective rule provider, per-save static projection and narrow custom mechanics hooks.

Do not duplicate the project into drifting source forks.

Exact sibling BepInEx GUID/DLL filename and mutual-exclusion guard remain packaging decisions to verify before handoff. A user installs one edition, not both.

## What is now closed or substantially narrowed

Closed as architecture/design blockers:

- final roster numbers/roles;
- PrayerClarity: Vanilla 1.0.20 source baseline;
- edition naming;
- shared UI/model ownership;
- persisted player-param tier-token strategy;
- stale-token safety through vanilla live-buff gating;
- no-go on quality-dependent `BuffDefinition.res` mutation;
- per-save definition projection lifecycle candidate;
- Roots replacement strategy via whole parsed `craft_time` expressions;
- Repentance target at the stock daily probability-reset expression;
- Repose's exact RNG mathematics;
- Combat's regeneration scheduler via native `PlayerBuff` ticks;
- stock technology/craft ownership of the duplicate Protection recipe;
- Soul Contentment consumer graph family.

## Remaining narrow evidence gates before `dev/*`

1. **Lookup identity:** run the frozen read-only 0.1.0 probe and confirm live prayer `craft_data` rows are the same objects returned by GameBalance lookup.
2. **Static output/list ownership:** after identity result, verify sermon output-list projection uses the same live objects and does not require cache rebuild.
3. **Repose isolation:** identify a donor-specific `GenerateBody` call context so story/special bodies are untouched.
4. **Combat damage:** identify the narrow outgoing player-damage consumer for the Silver/Gold delta.
5. **Protection retirement:** verify tech unlock-list caching and actual desk recipe-list ownership before removing only new `b_shield` crafting visibility.
6. **Soul Contentment:** pin the exact coefficient/consumer mutation seam inside the isolated Soul Portal bonus path.
7. **Packaging:** exact sibling-edition GUID/DLL/mutual-exclusion composition.

These are technical evidence gaps, not user design questions. Prefer existing static evidence; combine unresolved IL questions into one read-only audit rather than burdening the user with separate probes.

No production implementation is authorized until the remaining integration seams are sufficiently closed. Hosted CI is justified only for a narrow executable probe/build that proves one of those properties.
