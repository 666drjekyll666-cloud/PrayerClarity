# Rebalanced Native-Seam Architecture Audit

Status: **accepted architecture record for Rebalanced 0.2.3**. Historical candidate sections are retained to show how the final seams were selected.

Target: Graveyard Keeper 1.407, Assembly-CSharp MVID `6f50b8e7-156b-49ac-bbe8-7505894b2364`.

Accepted runtime baseline entering this audit was Rebalanced 0.2.2. The audit concluded in accepted/public Rebalanced 0.2.3:
- accepted ref `accepted/rebalanced-0.2.3`;
- tag/release `rebalanced-v0.2.3`;
- exact accepted runtime source `ab1eb67cbf2465a912c392120395011503b720c3`;
- accepted/released DLL SHA-256 `03a4a8b43c8a5ffef8ec62eac58370d3f6bced347bc2fb4f49a2ef81a23cb1ca`.

## Why this audit exists

Rebalanced 0.2.0 exposed a process failure in Shoots & Roots. The implementation used Graveyard Keeper's own SmartExpression parser, but replaced the stock crop formula in order to move the prayer contribution from WGO scope to player scope. Static expression validation and unrelated synthetic-buff/UI runtime passes did not execute the real crop-growth calculation. In the live growth path, Expressive threw `InvalidCastException`; `SmartExpression.EvaluateFloat` fell back to 1 second and crops completed almost immediately.

This distinguishes two ideas that must not be conflated:

- **host technology is used** — e.g. Harmony, SmartExpression, FlowCanvas;
- **host behavior remains authoritative** — the game's original formula/lifecycle/rounding/stacking path still owns the calculation and the mod only supplies a changed input.

The second is the architecture target.

The permanent global rule now lives in `DevRules/ENGINEERING_RULES.md` under **Host-native-first modification gate**. PrayerClarity's local `AGENTS.md` adds game-specific examples.

## Mechanism hierarchy for this audit

Prefer, when equivalent semantics are possible:

1. project verified values into stock data already consumed by the native path;
2. use an explicit host extension point;
3. project the narrow parameter/operand read by the native calculation;
4. intercept a narrow semantic result only if input projection is not practical/correct;
5. replace a host-owned formula/expression/graph/algorithm only after steps 1-4 are shown insufficient.

A native parser executing a mod-authored replacement formula is step 5, not step 1.

## Current mechanism inventory

| Area | Current Rebalanced mechanism | Native ownership assessment | Audit action |
| --- | --- | --- | --- |
| Faith / Donations / Combo / Soul's Repose sermon coefficients | one-time `CraftDefinition` projection of `needs_quality`, `k_faith`, `k_money`, stock outputs | **Good** — stock sermon path consumes changed stock fields | retain; normal-sermon payout spot-check remains useful |
| Imagination premium Stories | modify prayer's stock success output list | **Good** — stock prayer-success output path remains authoritative | retain; one real successful sermon reward check |
| Protection retirement / Combat alias | Tech/CraftDefinition projection | **Good** | retain |
| Tier capture | successful `StartPrayAnimation` postfix stores tier/scalars in player params | **Defensible custom state** — qualities share one native buff ID, so tier magnitude needs a carrier | search for a native quality carrier before future expansion; no immediate rewrite |
| Shoots & Roots 0.2.2 | temporary WGO `totem_effect["buff_plant"]` projection; stock `craft_time` left unchanged; 95% aggregate cap | **Good after repair** — game owns formula and tick | accepted; no further architecture change |
| Repentance | replace `church_budka_roll.execute_expressions[0]` SmartExpression | **Concern / step 5** — replaces stock reset expression | highest-priority seam research; seek parameter/result projection |
| Combat regeneration | populate stock `buff_sword.se_tick`, set `tick_period=1` | **Architecturally valid extension point** — `PlayerBuff.CustomUpdate` natively executes `se_tick` | focused runtime numeric test; do not classify as Roots-style rewrite |
| Combat damage | postfix `WorldGameObject.GetDamage` adds tier extra | **Narrow and simple, but after native sum** — stock GetDamage already consumes `add_damage`; a native-input projection may be more future-proof but must beat the current low complexity | compare current postfix with scoped temporary `add_damage` projection around GetDamage |
| Combat armor | `DecHP` context + ThreadStatic state + global `GetParam("add_armor")` postfix | **Likely over-complex / hot-hook cost** — stock DecHP already consumes `add_armor` | prefer researching temporary player `totem_effect["add_armor"]` projection scoped to DecHP prefix/finalizer |
| Excellence | postfix `CraftDefinition.GetBuffValue("buff_star")` changes active buff quality contribution | **Narrow semantic seam / acceptable** | compare with safe BuffDefinition `craft_q` projection; keep current if shared-definition mutation is riskier |
| Repose | identify ordinary Donkey callback, then narrow arguments before native `GenerateBody` | **Complex but justified** — custom reliability design cannot be expressed by one existing scalar; native body generator remains authoritative | retain unless a simpler verified body-range seam appears; terminal runtime check when available |
| Soul Contentment | find live `soul_portal` FlowCanvas graph once and change isolated coefficient 0.1 -> 0.2 | **Provisionally acceptable, brittle boundary** — host graph still owns multiply/round/cap and there is no recurring scan; a global GetParam overlay may actually have broader/hotter cost | compare one-time structural constant projection with any equally narrow parameter seam; do not rewrite merely to reduce line count |
| Thorough Cleansing | stock `increase_sin_shard_drop=1` x2 path retained | **Good** | retain |

## Direct host evidence used by this audit

Public decomp reference used for call-shape inspection: `Kupie/GYK_DECOMP` commit `6abf79199d92482af1c7573870dd9a20ec2270b9`. Project/runtime evidence remains authoritative for the user's installed 1.407 build.

### Buff lifecycle and extension points

`BuffDefinition` exposes:
- `GameRes res`;
- `length`;
- `tick_period`;
- `se_start`;
- `se_finish`;
- `se_tick`;
- `craft_q`.

`BuffsLogics.AddBuff` adds `BuffDefinition.res` to the player and executes `se_start`; removal subtracts the same `res` and executes `se_finish`.

`PlayerBuff.CustomUpdate` executes `BuffDefinition.se_tick` when `tick_period` elapses. Therefore adding Combat regeneration through an otherwise empty `se_tick` is use of a stock buff extension seam, not replacement of an existing combat-regeneration algorithm.

### Native combat inputs

`WorldGameObject.GetDamage(DamageType)` for the player returns weapon calculated damage plus:

`GetParam("add_damage", 0f)`.

`HPActionComponent.DecHP(float)` subtracts equipped armor and then:

`GetParam("add_armor", 0f)`.

These facts make `add_damage` and `add_armor` the preferred first research targets for tier-aware Combat input projection.

### Native multi-quality input

`CraftDefinition.GetBuffValue(buff_id)` returns zero when the buff is inactive, otherwise `BuffDefinition.craft_q`. `GetMultiqualityResult` sums those linked buff values.

The existing Excellence postfix is therefore already attached to the exact semantic accessor that owns the active-buff contribution. Directly mutating shared `BuffDefinition.craft_q` may be simpler in lines but can be worse in ownership/lifecycle; evaluate before changing.

### Repentance logic

Stock Prayer of Repentance produces `buff_sins=1`, but the project's direct 1.407 audit found no stock gameplay consumer for `buff_sins`.

The daily logic `church_budka_roll` owns an execute expression that resets:

`SetPpar("confession_probability", 0.15)`.

`LogicData.Execute()` natively evaluates `execute_expressions` before running scripts/events. Current Rebalanced replaces that entire reset expression with one that also multiplies live prayer state and a custom tier scalar.

The accepted design (50/75/100% confession probability) remains valid, but the current mechanism must not be considered final until the downstream read/roll of `confession_probability` is traced. Preferred outcome: leave the stock 0.15 reset intact and adjust the value through the narrowest verified parameter seam around its consumer.

### Soul Contentment

The project directly established the stock soul-portal formula:

`GP_awarded = RoundToInt(GP_base * (1 + 0.1 * increase_gp_gain))`.

Stock `buff_gp_increase` supplies `increase_gp_gain=1`, producing +10%.

Current Rebalanced changes the live graph's 0.1 coefficient to 0.2. For the accepted +20% design, a more host-native shape should be investigated first: keep the graph at 0.1 and make the existing input resolve to 2 only while the native buff is active. This would preserve stock rounding and graph ownership automatically.

## What went wrong in the earlier process

The failure was not absence of an engineering principle. DevRules already contained the least-sufficient-mechanism rule before Rebalanced expression projection was implemented.

The missing enforcement was:

1. **No mechanism-ranking gate.** We did not require an explicit answer to “can the host keep owning this formula if we change one input?”
2. **Native parser was mistaken for native ownership.** Because SmartExpression was a game facility, replacing a whole expression looked less invasive than it really was.
3. **Acceptance evidence did not execute the changed semantic path.** Synthetic buff activation and UI checks proved buff lifecycle/presentation, not crop `craft_time` evaluation.
4. **No stacking-edge gate for replaced formulas.** Fertilizer + Roots was only examined after the defect was discovered.

The corrective rule is therefore both architectural and evidentiary:
- preserve the host-owned calculation where possible;
- if a formula/graph must be replaced, execute that exact live path and a material interaction edge before acceptance.

## Prioritized research plan

### P0 — Repentance

**Native seam identified; production change not yet implemented.**

Previously captured read-only runtime evidence (Repentance audit/lifecycle probes) established:
- the once-per-game-day scheduler resets player `confession_probability` to stock `0.15` before the roll;
- the `church_budka_roll` graph loops over two confessionals;
- each iteration removes the previous `confession_available` interaction;
- the graph uses `Flow_RandomFloat` over 0..1 and a `Flow_GetPlayerParam` node with `param="confession_probability"` for the probability comparison.

Pinned game-code inspection now closes the accessor itself:

`FlowCanvas.Nodes.Flow_GetPlayerParam.Invoke(string param) => MainGame.me.player.GetParam(param, 0f)`.

Preferred candidate mechanism:
- restore the stock `SetPpar("confession_probability", 0.15)` expression unchanged;
- patch only `Flow_GetPlayerParam.Invoke(string)`;
- require exact `param == "confession_probability"`;
- require the node owner graph to be exactly `church_budka_roll`;
- require live native `buff_sins`;
- for a valid captured Rebalanced tier, return accepted effective probability 0.50 / 0.75 / 1.00;
- otherwise return the untouched stock result.

This leaves daily scheduling, reset, two-confessional loop, random-number generation, interaction removal/addition and reward flow entirely game-owned. It also removes the remaining host-formula replacement from Repentance.

Runtime acceptance for a future candidate must exercise the real daily graph (or an exact research-only forced execution of that same graph), verify the effective probability at Bronze/Silver/Gold, and confirm stock 0.15 when the prayer is inactive.

### P1 — Soul Contentment

Goal: determine whether the current one-time graph-constant projection is already the least sufficient mechanism.

Current strengths:
- runs only when the relevant attached script is initialized;
- structurally requires the `soul_portal` graph and the exact `increase_gp_gain -> × coefficient -> +1` chain;
- changes one isolated constant only;
- leaves stock multiplication, `RoundToInt`, zone cap and graph execution authoritative;
- no per-frame/global parameter hook remains afterward.

Alternative to compare, not assume superior:
- leave coefficient at 0.1 and make effective `increase_gp_gain` contribute one extra unit only while the native buff is active.

Reject the alternative if it requires a broader/hotter global `GetParam` patch or creates worse lifecycle/stacking ownership than the current one-time graph projection.

### P1 — Combat damage / armor

Trace all meaningful consumers of `add_damage` and `add_armor`.

Desired result:
- compare current narrow damage postfix against a temporary `totem_effect["add_damage"]` projection scoped to `GetDamage`; choose the simpler and safer mechanism, not the more cosmetically native one;
- replace the armor ThreadStatic + global `GetParam` hook if a DecHP-scoped temporary `totem_effect["add_armor"]` projection is verified equivalent;
- let native `GetDamage` / `DecHP` own their arithmetic;
- do not introduce a global parameter hook merely to reduce source lines.

### P2 — Combat regeneration

Architecture is provisionally acceptable because `se_tick` is a stock extension seam.

Focused runtime gate:
- damage player below max HP;
- activate Bronze/Silver/Gold Combat;
- observe +1/+2/+4 HP per tick at 1-second period;
- verify no expression/runtime errors;
- verify behavior at/near max HP and after buff removal.

### P2 — Excellence

Verify one representative linked multi-quality craft under Silver/Gold.

Only replace the current `GetBuffValue` seam if a proposed `BuffDefinition.craft_q` projection has clearly lower lifecycle/shared-state risk.

### P3 — Repose / normal sermon outputs

When suitable save state exists:
- terminal Donkey progression Silver/Gold;
- normal successful Faith / Donations / Combo payout spot-check;
- one real Imagination premium Story reward.

These are lower architectural risk than P0/P1.

## Production rule during this audit

Do not change accepted Rebalanced 0.2.2 production mechanics merely because an alternative looks cleaner.

For each proposed simplification:

`trace native owner -> prove narrower seam -> compare failure/stacking/lifecycle -> implement on dev branch -> focused runtime test -> explicit acceptance -> new numbered version`.

0.2.2 remains immutable.


## Additional static audit findings

### Existing documentation confirmed the process gap

Historical `docs/IMPLEMENTATION_TARGET_AUDIT.md` explicitly marked both Roots and Repentance expression replacement as "closed target" and justified Roots with the native `SmartExpression.ParseExpression(string)` path. That document is now corrected on `main` for Roots and marks the Repentance mechanism as reopened.

This is direct evidence that the earlier mistake was a design-review classification error: native parser execution was treated as equivalent to native formula ownership.

### Combat consumer breadth

Direct code search against the pinned 1.407 decomp found:
- `add_damage` consumed in `WorldGameObject.GetDamage`;
- `add_armor` consumed in `HPActionComponent.DecHP`.

No second game-code consumer was found for either literal in the inspected assembly. This strengthens, but does not by itself runtime-accept, a future exact-player/exact-param overlay as a lower-blast-radius replacement for the current result postfix / ThreadStatic armor context.

### Current patch inventory sanity check

The current production `Rebalanced*.cs` set contains no hidden additional whole-formula replacement beyond `RebalancedExpressionProjection`. Roots uses SmartExpression only to validate the stock expression shape; it does not parse a replacement. The remaining parser-authored production expressions are:
- Repentance daily probability replacement;
- Combat regeneration `se_tick` expression.

Their architectural statuses differ: Repentance replaces host-owned logic and remains P0; Combat regen populates a native empty buff extension field and remains a runtime-verification item rather than an automatic rewrite target.


### Native-first does not mean "always patch the lowest-level parameter getter"

A broad low-level hook can be less desirable than a narrow semantic hook even if the broad hook changes an earlier input.

For example:
- `WorldGameObject.GetParam` is a hot, generic primitive used throughout the game;
- `WorldGameObject.GetDamage` and `HPActionComponent.DecHP` are narrow semantic operations;
- the current Soul Contentment graph mutation is event-driven and one-time.

Therefore mechanism ranking must account for blast radius, recurring cost, lifecycle and failure isolation, not only whether the modification occurs before or after the host formula. The goal is host ownership with the **least sufficient overall mechanism**.


## Combat follow-up evidence

### Regeneration

Pinned `SmartExpression` inspection shows `AddPpar("hp", value)`:
- reads current player HP through `GetParam`;
- adds the evaluated value;
- clamps HP to `save.max_hp`;
- writes through the game's `SetParam`.

Combined with the verified `PlayerBuff.CustomUpdate -> BuffDefinition.se_tick` lifecycle, this means the current Combat regeneration design delegates tick scheduling and max-HP clamping to native code. No architectural rewrite is recommended before a focused runtime numeric check.

### Damage / armor scoped-input candidate

`WorldGameObject.GetParam(name, 0)` returns serialized/data value plus nonserialized `totem_effect` value.

The accepted Roots repair already proves a scoped temporary `totem_effect` projection pattern with finalizer restoration.

Candidate simplification for future Combat production work:
- `GetDamage` prefix/finalizer: temporarily add only the tier's extra damage beyond stock `buff_sword +5` to player `totem_effect["add_damage"]`;
- `DecHP` prefix/finalizer: temporarily add the accepted +4 armor to player `totem_effect["add_armor"]`;
- let the stock methods read those parameters and perform their own arithmetic;
- restore exact previous runtime-effect values in finalizers.

Potential benefit: remove the global `WorldGameObject.GetParam` Harmony patch and ThreadStatic armor context while preserving the stock damage/armor calculations. This is a design hypothesis until implemented and runtime-compared against accepted behavior.


## 2026-09-18 focused decision closure: Repentance + Combat

### Protection / armor terminology

Rebalanced retires the separate Prayer for Protection crafting/unlock path. Its accepted +4 armor effect is **not removed**: it is folded into the unified Combat prayer.

Accepted Combat semantics remain:
- Bronze: +5 damage, +4 armor, +1 HP/s;
- Silver: +10 damage, +4 armor, +2 HP/s;
- Gold: +15 damage, +4 armor, +4 HP/s.

Legacy `b_shield` prayer crafts are projected to `buff_sword`, so old prayer items resolve to the unified Combat buff rather than maintaining a second runtime buff family.

### Repentance candidate architecture — narrowed

The old whole-expression replacement should be removed.

Preferred production candidate:
- leave stock `church_budka_roll.execute_expressions[0]` exactly `SetPpar("confession_probability", 0.15)`;
- patch `FlowCanvas.Nodes.Flow_GetPlayerParam.Invoke(string)`, not generic `WorldGameObject.GetParam`;
- return stock result unless all conditions hold:
  - `param == "confession_probability"`;
  - live native `buff_sins`;
  - captured Rebalanced Repentance tier is 1..3;
- when all conditions hold, return the accepted ruleset probability 0.50 / 0.75 / 1.00.

Why graph-name ownership is not required for correctness:
- `confession_probability` is itself the semantic parameter being rebalanced;
- the verified confession RNG graph consumes it through this exact FlowCanvas accessor;
- any other FlowCanvas consumer of the same semantic parameter should observe the same effective probability while Repentance is active;
- this avoids a generic hot `WorldGameObject.GetParam` patch and avoids runtime graph scans.

The obsolete persisted `prayerclarity_rebalanced_confession_bonus` scalar is no longer needed by this architecture. Tier state alone is sufficient. Existing stale values in old test saves can remain inert.

### Combat damage candidate architecture — native input projection

Stock `WorldGameObject.GetDamage(DamageType)` adds `GetParam("add_damage")` only through the normal weapon-damage branch. Its no-weapon fallback returns fixed damage without consuming `add_damage`.

Current Rebalanced adds tier extra in a postfix, which can therefore extend behavior beyond the stock `add_damage` semantics.

Preferred candidate:
- prefix/finalizer on `GetDamage`;
- require player + live `buff_sword` + valid captured Combat tier;
- compute only the Rebalanced amount beyond stock buff_sword's native +5:
  - Bronze 0;
  - Silver +5;
  - Gold +10;
- temporarily add that delta to nonserialized `player.totem_effect["add_damage"]`;
- let stock `GetDamage` perform its own weapon/fallback calculation;
- restore the exact prior runtime-effect value in finalizer.

This intentionally means the extra Rebalanced damage follows the same branch semantics as native `add_damage`, including not affecting a path that does not read that parameter.

The persisted `prayerclarity_rebalanced_combat_extra_damage` scalar is not required by this design; the delta can be derived from accepted rules + captured tier.

### Combat armor candidate architecture — scoped input projection, not BuffDefinition.res mutation

A tempting alternative is to append `add_armor=4` directly to `buff_sword.res`. That would allow native AddBuff/RemoveBuff to own the entire lifetime with no per-hit armor hook.

However, this is unsafe across first installation/update when a save already contains an active stock/older `buff_sword`:
- `GameSave.buffs` and player params are serialized;
- game load does not re-run `BuffsLogics.AddBuff` for already active buffs;
- `RemoveBuff` subtracts the **current** `BuffDefinition.res`;
- changing `res` while an old active buff exists can therefore subtract a component that was never added, e.g. leave `add_armor=-4`.

The current user's test save has no active Combat buff, so this edge does not block local testing; it still matters for a public mod that can be first installed over arbitrary vanilla save state.

Preferred candidate:
- prefix/finalizer on `HPActionComponent.DecHP(float)`;
- require player + live `buff_sword` + valid captured Combat tier;
- temporarily add accepted +4 to nonserialized `player.totem_effect["add_armor"]`;
- let stock `DecHP` subtract equipment armor and native `GetParam("add_armor")`;
- restore exact prior runtime value in finalizer.

Benefits over current implementation:
- removes the global Harmony patch on generic `WorldGameObject.GetParam`;
- removes ThreadStatic armor context and bonus state;
- does not mutate serialized player params;
- needs no migration marker or active-buff reconciliation;
- leaves native damage intake arithmetic authoritative.

### Combat regeneration

No architecture change is recommended. The current `buff_sword.se_tick` + `tick_period=1` uses a host-provided buff extension point. Only focused runtime numeric verification remains.

### Resulting production simplification target

If implemented successfully, the next candidate should:
- delete Repentance whole-expression replacement;
- keep SmartExpression authoring only for Combat's native `se_tick` extension;
- remove `ConfessionBonusParam`;
- remove `CombatExtraDamageParam`;
- replace Combat damage result postfix with scoped `add_damage` input projection;
- replace ThreadStatic + generic GetParam armor interception with scoped `add_armor` input projection;
- preserve every accepted 0.2.2 player-facing balance value.


## Rebalanced 0.2.3 native-seam candidate

Status: **accepted and published in Rebalanced 0.2.3**.

Production candidate:
- frozen ref: `candidate/rebalanced-0.2.3`;
- exact source SHA: `ab1eb67cbf2465a912c392120395011503b720c3`;
- CI run: `35388483840` — success;
- artifact ID: `10564359079`;
- artifact ZIP digest: `sha256:097febe34877eb16cb1ac79df56301f24e96deb908c60eb66830d8443bdf4a06`;
- Rebalanced DLL SHA-256: `03a4a8b43c8a5ffef8ec62eac58370d3f6bced347bc2fb4f49a2ef81a23cb1ca`.

Research helper:
- frozen ref: `candidate/rebalanced-test-console-0.1.3`;
- exact source SHA: `5de0f27e97cb38a72fb2b53b93e02131225012e7`;
- CI run: `35388746613` — success;
- artifact ID: `10564044982`;
- artifact ZIP digest: `sha256:69e712507e5194c65f7933f91a9eb463322ae4c4c0fefdf8617f9d8d9942ef55`;
- Test Console DLL SHA-256: `f7624c8696f5dbbf1bf64a959ea370c545c198332bbfe0c1b8eeec4e0775d6fc`.

### 0.2.3 implementation delta

Repentance:
- restores the exact stock daily reset expression `SetPpar("confession_probability", 0.15)`;
- patches only `Flow_GetPlayerParam.Invoke(string)`;
- for exact `confession_probability` reads, live `buff_sins`, and a valid captured tier, supplies 0.50 / 0.75 / 1.00;
- removes the redundant persisted confession-bonus scalar.

Combat damage:
- removes result postfix arithmetic;
- derives the tier delta beyond stock `buff_sword +5` directly from accepted rules;
- temporarily projects Silver +5 / Gold +10 into nonserialized `player.totem_effect["add_damage"]` around native `GetDamage`;
- Bronze adds no extra projection;
- finalizer restores the prior runtime value.

Combat armor:
- removes ThreadStatic state and the global `WorldGameObject.GetParam` patch;
- temporarily projects +4 into nonserialized `player.totem_effect["add_armor"]` around native `HPActionComponent.DecHP`;
- finalizer restores the prior runtime value;
- `BuffDefinition.res` is intentionally left unchanged to avoid first-install/update corruption when a save already contains an active older `buff_sword`.

Combat regeneration:
- unchanged architecture: native `buff_sword.se_tick` extension point with 1-second tick period and tier scalar 1 / 2 / 4 HP.

### Focused runtime acceptance

Use Test Console 0.1.3 with Rebalanced 0.2.3.

Repentance:
1. activate Bronze; press `Probe Repentance probability`;
2. repeat Silver and Gold;
3. remove Repentance; press the probe once inactive.
Expected diagnostics: active tiers 1/2/3 -> 0.50/0.75/1.00; inactive controlled stock probe -> 0.15.

Combat:
1. equip a weapon;
2. activate Bronze; press `Probe Combat outgoing damage`;
3. repeat Silver and Gold.
Expected tier delta vs Bronze/native result: 0 / +5 / +10.

Armor:
- with any Combat tier active, press `Probe Combat armor (controlled 20 damage; HP restored)`.
Expected `prevented_by_rebalanced_armor=4` unless ordinary equipped armor saturates the 20-damage probe to zero.

Regeneration:
- activate Bronze/Silver/Gold in turn;
- press `Prepare Combat regen probe (set HP to max - 20)`;
- allow several seconds of unpaused gameplay.
Expected positive HP deltas per native tick: +1 / +2 / +4 respectively.

Return one `LogOutput.log`; no sermon, daily wait, enemy fight, Roots retest, or permanent save mutation is required. Remove synthetic buffs before preserving the test save.


## Rebalanced 0.2.3 runtime verification — 2026-09-18

Status: **runtime verified and explicitly accepted by the user**.

Tested binaries:
- Rebalanced 0.2.3 source `ab1eb67cbf2465a912c392120395011503b720c3`, DLL SHA-256 `03a4a8b43c8a5ffef8ec62eac58370d3f6bced347bc2fb4f49a2ef81a23cb1ca`;
- Test Console 0.1.3 source `5de0f27e97cb38a72fb2b53b93e02131225012e7`, DLL SHA-256 `f7624c8696f5dbbf1bf64a959ea370c545c198332bbfe0c1b8eeec4e0775d6fc`.

User runtime evidence:
- both exact versions loaded together on Graveyard Keeper 1.407;
- Repentance:
  - Bronze -> effective probability 0.50;
  - Silver -> 0.75;
  - Gold -> 1.00;
  - the probe explicitly staged stock stored value 0.15 before each accessor read, demonstrating that the new result comes from the narrow accessor projection rather than an expression rewrite;
- Combat damage:
  - Bronze/native result 10 -> current 10, delta 0;
  - Silver -> 15, delta +5;
  - Gold -> 20, delta +10;
  - stock stored `add_damage` remained 5, so the extra Rebalanced delta is runtime-scoped rather than persisted;
- Combat armor:
  - controlled input 20;
  - stock loss 20;
  - Combat loss 16;
  - exactly 4 damage prevented;
- Combat regeneration:
  - Gold produced repeated +4 HP ticks;
  - Bronze produced repeated +1 HP ticks;
  - Silver activation produced repeated +2 HP effect bubbles in the same run, with the final +1 at the HP cap, consistent with native max-HP clamping;
- no PrayerClarity runtime failure, `Exception`, `ExpressiveException`, `InvalidCastException`, `SmartExpression`, or `Error in expression` occurred in the supplied log;
- synthetic Combat buff was removed at the end.

Minor test-plan deviation:
- the inactive Repentance probe button was not pressed after removing `buff_sins`;
- Silver regen was not armed through the diagnostic button, but native +2 HP bubbles were directly observed after Silver activation.

Assessment:
- no blocker found;
- the missing inactive Repentance probe is not considered a required repeat because production code returns immediately when `buff_sins` is absent, the exact stock reset expression is validated at install, and the active-tier runtime evidence proves the new seam itself;
- user explicitly accepted the candidate with "фиксируем 023";
- accepted ref `accepted/rebalanced-0.2.3` was frozen;
- PR #7 promoted the accepted source to `main`;
- the exact accepted CI binary was published as `rebalanced-v0.2.3` without rebuilding.
