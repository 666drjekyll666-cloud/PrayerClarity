# Rebalanced Runtime Risk Audit

Status: research / runtime-audit source of truth for the current Rebalanced 0.2.1 candidate family.

Target: Graveyard Keeper 1.407, Assembly-CSharp MVID `6f50b8e7-156b-49ac-bbe8-7505894b2364`.

This document classifies where PrayerClarity: Rebalanced changes executable game behavior, how invasive each mechanism is, what has been runtime-proved, and what still warrants focused verification. It is not a balance-spec replacement; concrete intended values remain in `PRAYER_REBALANCE_OPTIONS.md`.

## Summary

| Area | Mechanism | Current risk | Runtime status | Primary symptom if wrong |
|---|---|---:|---|---|
| Faith / Donations / Combo / Soul's Repose reward coefficients | one-time CraftDefinition data projection | low-medium | shared presentation accepted; exact normal-sermon payout is not the strongest-tested path | wrong Faith/money amount after a successful sermon |
| Imagination premium Stories | one-time CraftDefinition output projection | low-medium | presentation / synthetic buff path accepted; real successful sermon Story drop not yet directly observed | missing/wrong Story quality/count |
| Requirements / retired Protection | one-time CraftDefinition / TechDefinition projection | low | extensively visible in UI/runtime | wrong Church Quality gate, duplicate/visible Protection recipe |
| Tier capture | postfix on successful prayer lifecycle, persisted player params | medium | synthetic harness and normal prayer paths exercised | tier-dependent effect uses wrong quality after a successful sermon |
| Shoots & Roots | narrow `CraftComponent.DoAction` WGO-scope bridge using temporary nonserialized `totem_effect` | **high until max-stack edge closes** | Bronze/Silver, fertilizer interaction, removal verified | near-instant growth or wrong fertilizer stacking |
| Repentance | SmartExpression replacement on `church_budka_roll` | **high** | implementation-shape validation exists; needs focused live confession roll verification | no confession, wrong probability, SmartExpression/Expressive error |
| Combat regeneration | SmartExpression `se_tick` replacement on `buff_sword` | medium-high | synthetic active buff path exercised; focused HP tick verification still useful | no regen / wrong regen / expression errors |
| Combat damage | narrow `WorldGameObject.GetDamage` postfix | medium | active buff path exercised, but focused numeric damage check is preferable | damage remains +5 or double-applies |
| Combat armor | scoped `HPActionComponent.DecHP` + ThreadStatic `GetParam("add_armor")` extension | medium | architecture is narrow/fail-closed; focused incoming-hit check useful | armor not applied or leaks outside damage calculation |
| Excellence | narrow `CraftDefinition.GetBuffValue("buff_star")` postfix | low-medium | active buff/presentation path exercised | linked craft quality remains stock +0.2 or wrong tier |
| Repose | Donkey delivery callback + one following `GenerateBody` range narrowing | medium-high | static endpoint research strong; terminal/runtime body-delivery cases remain partly deferred | wrong corpse-tier distribution / effect applied to non-Donkey bodies |
| Soul Contentment | one-time live `soul_portal` FlowCanvas graph coefficient projection | medium-high | graph shape directly validated; focused real portal gain check still useful | stays +10%, becomes wrong coefficient, or fails closed |
| Thorough Cleansing | stock x2 buff retained; no new custom runtime multiplier path | low | stock mechanism reused | mainly presentation mismatch rather than new runtime failure |

## Persistent tier state

Rebalanced stores quality/effect tokens on the player after a **successful** sermon. These values can outlive the temporary buff, but all tier-sensitive runtime consumers require the corresponding live native buff before using the stored token. A stale token by itself is therefore inert.

The research Test Console also writes these tokens so synthetic buffs reproduce the production consumer state. Removing a synthetic buff does not need to erase the token to simulate production expiry; live-buff gating is the real lifecycle boundary.

## Shoots & Roots — verified behavior and newly identified edge

### Runtime-proved

Rebalanced 0.2.1 removed the unsafe full SmartExpression rewrite used in 0.2.0.

User runtime evidence on 2026-09-18 proves:
- ordinary growth no longer collapses to ~1 second;
- no `ExpressiveException` / `InvalidCastException` occurs in the tested crop path;
- Bronze: `tree_growing 1800 -> 1440` = -20%;
- Silver: `tree_growing 1800 -> 1260` = -30%;
- Silver carrot/cabbage: `1440 -> 1008` = -30%;
- Silver plus one native `grow_time` unit: fertilizer-only baseline `1152`, Roots result `720`; the native additive formula is preserved;
- native `RemoveBuff("buff_plant")` removal works and subsequent calls cease receiving the Roots projection.

### Maximum-stack edge — blocker before stable promotion

Verified stock crop formula shape:

`base * (1 - 0.2*WGOpar("grow_time") - 0.2*WGOpar("buff_plant"))`

Rebalanced maps Roots to effective WGO prayer units:
- Bronze .20 -> 1.0 unit;
- Silver .30 -> 1.5 units;
- Gold .40 -> 2.0 units.

External current farming references describe:
- Peat: 20% growth-time reduction;
- Boost I: 40%;
- Boost II: 60%.

Those external values imply native `grow_time` levels 1 / 2 / 3 if the verified 0.2 coefficient is the sole speed encoding. Runtime already directly observed `grow_time=1` for one fertilizer state.

If Boost II is directly confirmed as `grow_time=3`, Gold Roots produces:

`1 - 0.2*3 - 0.2*2 = 0`.

Direct game-code inspection shows `CraftComponent.DoAction` treats evaluated craft time <= 0.001 as immediate completion by setting progress to 1. Therefore this combination would recreate an instant-growth edge even though the 0.2.0 expression exception is fixed.

**Status:** strong hypothesis / release blocker, not yet promoted to fact for the exact Boost-II runtime parameter until one focused 1.407 test confirms the live `grow_time` value.

Required test using the already-built Test Console 0.1.1:
1. apply Boost fertilizer II to one empty crop bed;
2. activate Gold Shoots & Roots;
3. plant any ordinary crop;
4. return the log.

The existing `ROOTS_DIAGNOSTIC` line will directly report `grow_time`, baseline and Roots craft time. No stopwatch is required.

Do not promote 0.2.1 to stable while this question remains open. If confirmed, 0.2.1 bytes stay immutable and any production correction becomes 0.2.2.

## Trees and non-crop plant growth

The verified stock `buff_plant` consumers are broader than ordinary farm crops. Rebalanced validates and bridges only known definitions whose stock expression already contains the prayer term. The verified set includes ordinary crops, vineyard/refugee crop growth, berry-bush growth/respawn, apple-tree stages, ordinary tree growth/spawn, flowers, hiccup grass and mushroom spawning.

Runtime evidence directly captured `tree_growing` under Bronze Roots: `1800 -> 1440`. Therefore at least that real tree-growth process is definitely accelerated. Mature already-grown trees are not globally time-scaled; only an active growth/respawn craft that already owns the stock `buff_plant` term is affected.

## SmartExpression-specific audit priority

The 0.2.0 Roots regression proves that a syntactically valid replacement expression can still fail at runtime inside Expressive. Two Rebalanced expression projections remain:

1. Repentance reset expression:
   `SetPpar("confession_probability", 0.15 + Ppar("buff_sins") * Ppar("prayerclarity_rebalanced_confession_bonus"))`
2. Combat tick:
   `AddPpar("hp", Ppar("prayerclarity_rebalanced_combat_regen"))`

Neither should be assumed safe merely because parsing succeeds. They are the highest-priority next runtime checks in the broader audit.

## Next focused audit order

1. close Roots Boost-II + Gold edge;
2. Repentance real daily confession roll at Bronze/Silver/Gold, inspecting log for expression errors;
3. Combat: one outgoing hit, one incoming hit, several seconds of HP regeneration;
4. Excellence: one representative linked craft at Silver/Gold;
5. Soul Contentment: one real soul-portal gratitude gain;
6. Repose: ordinary Donkey delivery at Silver/Gold when convenient, terminal endpoint when a suitable save exists;
7. normal successful sermon payout spot-check for Faith / Donations / Combo and one real Imagination premium Story reward.

Do not repeat already-closed tests unless a code change touches their mechanism.


## Roots overflow cap

Status: **accepted design; implemented in Rebalanced 0.2.2 candidate, runtime acceptance pending**.

The narrow preferred correction is to preserve the verified native additive relationship but cap the **combined** growth-time reduction at **95% of base time** (minimum remaining growth time 5% of base).

For stock fertilizer reductions 0/20/40/60% and Rebalanced Roots 20/30/40%, the uncapped matrix is:

| Fertilizer | Bronze Roots | Silver Roots | Gold Roots |
| ---: | ---: | ---: | ---: |
| 0% | 20% | 30% | 40% |
| 20% | 40% | 50% | 60% |
| 40% | 60% | 70% | 80% |
| 60% | 80% | 90% | **100%** |

A 95% cap changes only the final dangerous cell: Gold + maximum Boost becomes 95% instead of 100%. All other stock combinations remain numerically unchanged.

Why this is preferred over alternatives:
- do **not** change Roots to multiplicative stacking: that would alter every fertilizer+Roots combination and contradict the already accepted native-additive design;
- do **not** lower Gold Roots globally: the issue exists only when another source has already consumed almost all remaining growth time;
- do **not** clamp to the game's <=0.001 completion threshold: that would merely turn instant growth into practically instant growth;
- a round 95% aggregate cap is easy to explain and keeps at least 5% of base growth time.

Implemented 0.2.2 shape:
- before temporary Roots projection, read the current WGO's existing `grow_time` and `buff_plant` contributions;
- compute how much headroom remains before 95% total reduction;
- inject only `min(configured Roots reduction, available headroom) / 0.20` into the nonserialized `totem_effect["buff_plant"]`;
- restore the original runtime aggregate in the existing finalizer;
- do not mutate serialized crop data or rewrite the native craft expression.

Player-facing Clarity discloses the nominal prayer reduction together with the 95% combined cap across all Rebalanced prayer surfaces.

Because 0.2.1 was already handed out, any implementation change uses a new Rebalanced version; do not replace 0.2.1 bytes.


### 0.2.2 implementation candidate

- Frozen candidate ref: `candidate/rebalanced-0.2.2`
- Exact source SHA: `924900365d44cd1ec9e530c9dd9b7e2f6a796bed`
- CI run: `35382737488` — success, 0 warnings / 0 errors
- Artifact ID: `10562647003`
- Artifact ZIP digest: `sha256:153d6664019d0532dfbb4095dc810e9fc96beb7562f100f4d42cf398a7a1c652`
- Rebalanced DLL SHA-256: `4655fea2a57125aa78965a807fde76f9a056dbbd7f361246cf12351ff45074d6`

Implementation preserves the existing temporary nonserialized `totem_effect["buff_plant"]` bridge. Before injection it reads only modifier terms actually consumed by the verified stock expression, computes remaining headroom to the 95% aggregate cap, injects only the permitted prayer portion, and restores the original runtime value in the existing finalizer.

Presentation now discloses both the nominal prayer reduction and the 95% combined cap in all 11 supported locales.


### Synthetic Boost II acceptance helper

Because the user does not currently own Boost Fertilizer II, Rebalanced Test Console 0.1.2 provides a nonpersistent runtime simulation instead of requiring item acquisition.

- Candidate: `candidate/rebalanced-test-console-0.1.2`
- Source: `f50fff1d7dc314f7f27ac125d1e760346a6ce6fe`
- CI run: `35383083880` — success
- DLL SHA-256: `cd0804631532ea6d0c1d2faa4b086b233591f48b63b9ebb06573d540087d4729`

The simulation only affects the `grow_time` getter while an active plant craft already consumes both stock growth terms and Roots is live. It does not mutate save data.
