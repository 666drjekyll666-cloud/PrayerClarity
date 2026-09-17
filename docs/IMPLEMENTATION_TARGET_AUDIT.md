# PrayerClarity: Rebalanced — implementation target audit

Status: **research / architecture evidence**, reconciled with PrayerClarity: Vanilla 1.0.20 on 2026-09-17. This document identifies safe implementation seams; it does not make Rebalanced runtime behavior accepted.

Target game evidence remains Graveyard Keeper 1.407, `Assembly-CSharp` MVID `6f50b8e7-156b-49ac-bbe8-7505894b2364`.

Accepted presentation/runtime base: PrayerClarity: Vanilla **1.0.20**, source `c7ac91c1cea6c498fb406323725768b605d8139f`.

Locked gameplay intent is in `PRAYER_REBALANCE_OPTIONS.md`. Stock behavior remains canonical in `PRAYER_MECHANICS.md`.

## Primary architecture finding — one rule set, two mechanical projections

PrayerClarity: Vanilla 1.0.20 already has one shared presentation path centered on `PrayerForecast`:

- Pulpit calls `PrayerForecast.Build(...)`;
- Technology and prayer-item tooltips call `PrayerForecast.BuildTierDetails(...)`;
- Temporary Effects calls `PrayerForecast.BuildActiveBuffText(...)`.

`BuildTierDetails` currently reads the live stock prayer `CraftDefinition` directly: `needs_quality`, `k_faith`, `k_money`, outputs, `buff`, `dur_parameter`, and linked event. `BuildSpecial` then reads relevant `BuffDefinition` data. This is correct for PrayerClarity: Vanilla, but it cannot remain a raw-stock reader in PrayerClarity: Rebalanced because several locked effects are not representable by stock fields alone.

The Rebalanced edition therefore needs **one declarative effective-rule source**, not separate gameplay and tooltip tables.

Recommended shape:

`stock prayer definition + locked Rebalanced override -> EffectivePrayerDefinition`

The same resolved rule object supplies:

1. presentation semantics for Pulpit, Technology, item tooltip and Temporary Effects;
2. safe static projections into native prayer definitions where the stock sermon engine already owns the behavior;
3. narrow custom consumer hooks for effects the stock data model cannot express safely.

Do **not** create a second manually maintained table of Rebalanced numbers in UI code. Do **not** replace the whole native prayer engine with a parallel calculator.

## Static-definition projection — preferred where stock already owns the mechanic

`PrayLogics.CalculatePray` already consumes prayer-owned `needs_quality`, `k_faith`, `k_money` and prayer outputs. The accepted Clarity model reads those same fields. Therefore the least-mechanism candidate for fields whose semantics are already native is to project the locked rule into the corresponding runtime `CraftDefinition` once after balance data is ready.

Potential static projections derived from the single rule set:

- Faith: q25/40/70, `k_faith=2.5/3.5/4.5`, zero off-theme `k_money`, remove prayer-owned fixed Faith/money outputs;
- Donations: q25/40/70, `k_money=2.5/3.5/4.5`, zero prayer-owned Faith bonus/fixed Faith, retain +1/+2/+3 silver fixed money;
- BSS Soul's Repose: q25/40/70, `k_faith=2.5/3.5/4.5`, remove prayer-owned fixed/off-theme money contributions while preserving its Souls event formula and recipe;
- Imagination Silver/Gold: add the locked 3 Silver / 3 Gold Story sermon reward if direct output-list mutation is proven to use the normal success-only `_sermon_drops` path.

If this projection lifecycle is proven safe, stock sermon success calculation and current UI fields naturally consume the same projected values. It also avoids a broad Harmony rewrite of `CalculatePray`.

### Remaining proof before static mutation

Do not yet treat runtime `CraftDefinition` mutation as accepted production behavior. Verify narrowly:

- exact point at which GameBalance prayer definitions are fully loaded and stable;
- whether the relevant definitions are long-lived shared objects for the session;
- whether changing scalar fields and prayer output rows at that point has any reload/reinitialization path that would overwrite them;
- exact mutable output collection shape required for removing/adding Faith/money/Story rows.

Prefer one initialization/lifecycle hook. Do not poll or repeatedly reapply definitions.

## Hard no-go — global quality mutation of `BuffDefinition.res`

Direct inspection of `BuffsLogics.AddBuff` / `RemoveBuff` established an unsafe asymmetry:

- AddBuff applies the **current** `BuffDefinition.res` to player parameters;
- saved `PlayerBuff` does not retain the resource delta that was applied;
- RemoveBuff later subtracts the **then-current** definition resource.

Changing shared resources such as `buff_sword.res` by prayer tier can therefore leave permanent parameter drift after save/load, edition removal or definition changes.

**Do not tier-scale passive prayer resources by rewriting `BuffDefinition.res`.**

Keep stock buff definitions/resources intact where possible and implement quality-specific behavior at the actual consumer or through another symmetric mechanism.

## Active prayer quality persistence

### Vanilla `PlayerBuff` loses source prayer quality

The inspected `PlayerBuff` state contains buff ID, end time, tick state and definition lookup, but not the source `CraftDefinition`/prayer item quality. Bronze/Silver/Gold therefore cannot be reliably reconstructed from an active buff after save/load.

### Capture seam

The source craft still exists during successful prayer application:

- `PrayCraftGUI.DoPrayForBuff(...)` -> `PlayerComponent.StartPrayAnimation(pray_craft, success)`;
- `StartPrayAnimation` retains `_pray_craft` / `_pray_buff` / success state;
- `PlayerComponent.CreatePrayBuffFlyingObject` still has the selected craft and duration before the actual player buff is created.

This is the narrow tier-capture boundary.

### Persisted token store

Direct static evidence shows player `Item._params` is a serialised `GameRes` that accepts arbitrary string keys and is persisted inside the player's saved inventory. Therefore namespaced per-effect float parameters remain the preferred tier store, for example:

- `prayerclarity_buff_sins_tier`;
- `prayerclarity_buff_plant_tier`;
- `prayerclarity_buff_skull_tier`;
- `prayerclarity_combat_tier`;
- `prayerclarity_buff_star_tier`.

A single global tier token is insufficient because long prayer effects can overlap across sermon weeks.

Runtime reads must always be gated by the corresponding active vanilla buff/effective effect. A stale token alone is inert. PrayerClarity: Vanilla must ignore Rebalanced tokens entirely.

No external save sidecar is justified by current evidence.

## Presentation integration in 1.0.20

The current 1.0.20 surfaces are already well placed; the model below them must change, not their layout ownership.

### Pulpit

`PrayCraftGUI.RedrawTextValues(float needs_q, float chance)` remains the accepted redraw seam. `PrayerForecast.Build` should resolve an `EffectivePrayerDefinition` for the selected craft and render it through the existing reward-reveal grammar.

If Rebalanced q is projected into the live craft before the pulpit opens, vanilla's own success-chance argument can remain authoritative. If q is not projected, success chance must be derived from the same effective rule instead; do not display stock `chance` beside a Rebalanced requirement.

### Technology

`TechUnlock.GetTooltip(Tooltip)` already resolves the sibling prayer crafts and sends their `TierDetails` into the accepted content-driven Bronze/Silver/Gold renderer. Keep the renderer generic. Rebalanced should change semantic data, not add prayer-specific layout branches.

### Prayer item tooltip

`ItemDefinition.GetTooltipData(Item,bool)` resolves one concrete linked prayer craft and calls the same tier-detail builder. Keep the current current-tier-only product behavior.

### Temporary Effects

`PerkBuffItemGUI.Draw(PlayerBuff)` currently describes stock `BuffDefinition` state. Rebalanced quality-sensitive active effects must resolve the effective rule using `buff_id` plus the persisted tier token instead of reading tier-varying values from globally mutated buff definitions.

The existing remaining-duration behavior remains valid and should continue to use native `PlayerBuff` timing.

## Prayer-specific target classification

### Native/static sermon-definition changes

These should prefer the static projection path once its lifecycle is proven:

- Faith q/rates/output cleanup;
- Donations q/rates/output cleanup/floor;
- BSS Soul's Repose q/rates/output cleanup;
- Imagination premium Story reward, if output-list projection is proven safe.

Combo, Ordinary, Prosperity and Thorough Cleansing remain stock mechanically and require no gameplay patch for their locked main effect.

### Shoots & Roots (`b_plant` / `buff_plant`)

Verified stock bug: prayer writes `buff_plant` on the player; growth expressions read `WGOpar("buff_plant")` on the growing/workbench WGO. Stock intended-looking coefficient is -20%.

Rebalanced needs -20/-30/-40% by tier while preserving the native additive growth expression.

`SmartExpression` already supports player `Ppar` and work-object `WGOpar`; a custom growth timer is unnecessary.

**Open implementation gate:** prove the safe source/recompile lifecycle for affected already-loaded SmartExpressions. The quality-sensitive expression should consult the active player tier token/effective rule, not mutate every growing WGO and not use polling.

### Repentance (`b_sins` / `buff_sins`)

Later runtime/static evidence closed the earlier unknown:

- `church_budka_roll` runs once per in-game day;
- it resets `confession_probability` to stock 0.15 before the roll;
- each existing confessional rolls independently;
- Confessional I returns 1 Faith plus Story I/II distribution;
- Confessional II returns 2 Faith plus Story II/III distribution.

Locked Rebalanced chance is 50/75/100% by prayer quality.

**Implementation rule:** do not merely set `confession_probability` once when the buff starts because stock overwrites it on the next daily roll. Patch the narrow daily roll calculation/assignment seam so the active `buff_sins` + persisted tier resolves the effective chance immediately before the roll.

The exact method/script hook is still an implementation-target detail to pin before code.

### Repose (`b_skull` / `buff_skull`)

Stock `body_max+1` expands the eligible corpse tier pool. Rebalanced changes reliability, not the progression ceiling:

- Bronze stock-style roll;
- Silver `P(best)=0.5+0.5*P_stock(best)`;
- Gold guaranteed best prayer-eligible tier.

Do not rewrite Donkey progression bounds globally.

**Open gate:** identify the exact corpse-tier selection/RNG function after `Flow_DropBody` receives `tier_min/tier_max`, so the implementation can wrap the real stock roll and preserve all non-prayer cases.

### Combat (`b_sword`; `b_shield` legacy alias)

Locked package:

- damage +5/+10/+15;
- armor +4;
- regeneration 1/2/4 HP/sec;
- duration 36/72/108;
- q10/20/40.

`b_sword` is canonical and `b_shield` is a save-safe same-quality alias. They must resolve to one effective Combat lifecycle and never stack.

Do not globally tier-mutate `buff_sword.res` or `buff_shield.res`.

A likely low-mechanism direction remains to preserve stock resource buffs where safe (stock +5 damage, stock +4 armor) and layer only tier-specific extra damage/regeneration through narrow consumers, driven by one `combat_tier` token. Whether canonical Combat should apply both native stock buff IDs or represent one component through a consumer hook must be decided only after checking refresh/removal semantics and edition-removal behavior.

**Open gates:** exact outgoing-damage consumer, cheapest correct regeneration lifecycle, native AddBuff refresh semantics, and alias non-stacking behavior.

### Imagination (`b_pen` / `buff_pen`)

Locked `craft_q` is +0.7 at all qualities — identical to stock special magnitude. Therefore no tier mutation of `buff_pen.craft_q` is required.

Only Silver/Gold Story rewards change. This substantially lowers implementation risk compared with the earlier 0.5/0.7/1.0 candidate.

### Excellence (`b_star` / `buff_star`)

Locked special magnitude is +0.2/+0.5/+1.0. Because `BuffDefinition.craft_q` is shared and the active buff loses source quality, do not mutate that field by tier.

Verified craft-quality seam: `CraftDefinition.GetMultiqualityResult` calls `CraftDefinition.GetBuffValue(buff_id)`.

Preferred target remains a narrow postfix/override on `GetBuffValue` restricted to `buff_star`, active Rebalanced edition and a valid persisted tier token. It should return the value from the same effective rule object used by the UI.

### Soul Contentment (`b_grat_points_incr`)

Locked magnitude is +20% at all qualities; duration remains 36/72/108. Stock buff exposes the boolean-like `increase_gp_gain` path and stock consumer calculates +10% before rounding.

Because the magnitude is constant across qualities, no tier token is needed for the arithmetic itself. Patch the verified Soul Gratitude award consumer narrowly so active Rebalanced `buff_gp_increase` uses 1.2 instead of 1.1. Temporary Effects should render the same +20% rule.

Exact consumer signature should be rechecked from accepted static evidence before production code.

### Thorough Cleansing (`b_sin_shard`)

Locked magnitude stays stock x2; only stock duration quality progression remains. No main-mechanic patch is required.

## Edition composition

The public editions should share presentation/model source but not behave as a hidden runtime profile toggle.

Preferred architectural boundary:

- common Clarity/presentation source;
- PrayerClarity: Vanilla installs only stock semantic readers and accepted Clarity UI patches;
- PrayerClarity: Rebalanced additionally installs the effective rule provider, safe static projection and custom mechanics hooks.

Do not duplicate the entire project into two drifting source forks.

Exact BepInEx GUID, DLL filename, assembly/build composition and mutual-exclusion guard remain evidence/packaging decisions. A user should install one edition, not both.

## What is now closed

The following are no longer design blockers:

- final roster numbers/roles;
- PrayerClarity: Vanilla 1.0.20 baseline;
- public edition naming;
- need for per-prayer tier persistence;
- native player-param persistence strategy;
- shared UI surfaces and their lifecycle seams;
- no-go on global quality-dependent `BuffDefinition.res` mutation.

## Remaining narrow evidence gates before `dev/*`

1. **Static projection lifecycle:** exact one-shot post-GameBalance-load seam; scalar/output mutation survivability.
2. **Roots:** SmartExpression source/recompile lifecycle.
3. **Repose:** exact corpse-tier RNG/selection method.
4. **Repentance:** exact narrow hook around daily probability assignment/roll.
5. **Combat:** damage consumer, regeneration scheduler/lifecycle, AddBuff refresh semantics and alias non-stacking.
6. **Soul Contentment:** reconfirm exact +10% award consumer signature for +20% override.
7. **Packaging:** exact sibling-edition GUID/DLL/mutual-exclusion composition.

These are technical evidence gaps, not user design questions. Resolve them directly from accepted/static data where possible. Use a runtime probe only for a question static evidence cannot close.

No hosted CI is required for this research/documentation audit.
