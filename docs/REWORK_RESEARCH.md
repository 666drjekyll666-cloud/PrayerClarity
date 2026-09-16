# PrayerClarity — Rework research

Status: research/design synthesis, 2026-09-16. No Balance/Rework mechanic in this document is accepted runtime behavior.

## Baseline

- Target: Graveyard Keeper 1.407.
- Stable presentation baseline: PrayerClarity 1.0.0, accepted runtime/source `493d2168489af80b5c1305f7ff1435e2ee1dd0d7`.
- Stock mechanics remain canonical in `PRAYER_MECHANICS.md`.
- Existing numerical candidates remain hypotheses in `PRAYER_REBALANCE_OPTIONS.md` unless revised/accepted explicitly.
- Failed-sermon full base donations remain intentionally preserved in every future profile.

## Rework-focused community pass — 2026-09-16

This pass revisited Reddit, Steam discussions and the current Nexus mod landscape with the mechanics audit already known, so searches focused on choice structure rather than only mechanic discoverability.

The evidence is still `community signal`, not mechanics proof.

### Repeated signals

1. **Too few prayers compete for the weekly slot.** Across discussions from 2018 through 2026, players repeatedly converge on Faith/Combo for routine sermons, with Imagination, Prosperity, Repose and BSS Thorough Cleansing used situationally. Combat prayers, Repentance and Shoots & Roots are repeatedly described as not worth the weekly opportunity.
2. **Combo compresses the specialist choice at equal quality.** Players have noticed since 2018 that Faith/Donations do not beat Combo at their own target output when quality is equal. More recent discussions clarify that specialists still matter because Chapter quality and church requirements are easier; this is a progression gate, not a target-output advantage.
3. **Donation income naturally loses strategic value.** Multiple discussions say sermon money becomes less important once crates/tavern/other income systems mature. This supports treating Donations as a progression specialist rather than forcing permanent late-game parity.
4. **Imagination is a healthy early/mid-game niche.** Players specifically use it to bootstrap better writing and better prayers before late writing perks. This is positive evidence against reducing its stock Bronze magnitude merely to create a symmetric tier curve.
5. **Prosperity is a healthy progression prayer.** Its permanent merchant-tier output creates a clear early use case and then naturally exhausts itself. Natural obsolescence is acceptable when the prayer is compelling during its window.
6. **Repose is understood as a progression-window prayer but stock reliability/late-game value are weak.** Recent 2026 discussion correctly describes the prayer as useful before final corpse progression and ineffective once the normal maximum tier is reached. Reliability scaling remains a plausible rework because it strengthens the existing fantasy without skipping story tiers.
7. **Combat prayers suffer from proposition failure more than raw-stat opacity.** Players repeatedly point out that the dungeon/combat demand is limited and consumables already solve survival. Two separate deep book prayers, each consuming the weekly sermon, are structurally unattractive. A merge/package rework is better motivated than simple +damage/+armor inflation.
8. **Repentance needs more than a nominal repair if confessions remain low-value.** Historical testing cannot detect the stock buff, but even discussions assuming it worked describe confession Faith/Story as too minor to justify a sermon. A chance-only rework must be modeled against actual roll cadence/output before acceptance.
9. **Thorough Cleansing is already a successful specialist.** Players actively alternate it with Combo and call it a preferred prayer because x2 Sin Shards materially changes a scarce BSS workflow. 2025–2026 players still use it as the primary shard accelerator. There is no current evidence requiring x3/x4 scaling.
10. **Soul Contentment is state-dependent rather than obviously weak.** Community discussion notes that +10% Gratitude is pointless at the cap if Gratitude is not being spent, but useful in automation/spending workflows. This is niche/state dependence, not sufficient evidence for a magnitude buff.
11. **BSS Soul's Repose can become extremely strong in unusual high-Gratitude states.** Community examples show very large Faith outputs when Soul Gratitude capacity is inflated. That may involve broader BSS storage behavior/exploits and is not a reason by itself to nerf the prayer inside PrayerClarity.

Representative sources are recorded in chat research and should be folded into `PLAYER_UX_RESEARCH.md` when the next roster is accepted.

## Design rules for the Rebalanced profile

### 1. Temptation parity, not numerical parity

Each prayer must create a believable moment where spending the weekly sermon on it is attractive. A narrow prayer may be much stronger than a generalist inside its niche.

### 2. Generalist breadth must cost specialist peak

Combo may remain good at both Faith and donations, but an equal-quality specialist should be the best ordinary sermon for its target resource. Prefer strengthening specialists over nerfing Combo.

### 3. Healthy Bronze behavior is a floor

Do not make a currently healthy Bronze prayer worse merely to manufacture a quality ladder. Quality progression can add magnitude, reliability, output or useful duration, but should not destroy an established early-game use case without a stronger reason.

### 4. Weekly opportunity cost requires visible consequence

A sermon-only buff that produces a barely measurable change is poorly matched to a once-per-week choice. Rework magnitude should be judged against competing weekly prayers, not only against zero.

### 5. Natural obsolescence is allowed

Prosperity and potentially Repose can be progression tools. They do not need permanent endgame dominance if they are strong and available during a meaningful progression window.

### 6. Duration-only quality is valid only when duration is useful

Longer duration is real power when it spans meaningful activities or future sermon weeks. If the relevant activity is normally completed in a short burst, higher prayer quality should preferably improve magnitude/reliability instead of only extending dead time.

### 7. Balance against substitutes

Evaluate prayers against the systems players actually substitute for them: books/perks, fertilizer/zombie farms, potions, crates/tavern, corpse progression, BSS shard loops, etc. Raw percentage increases without substitute context are insufficient.

### 8. Do not rebalance healthy specialists for symmetry

Prosperity, stock Imagination and Thorough Cleansing are current reference cases. Preserve them unless direct modeling establishes a real problem.

### 9. Prefer one coherent fantasy per prayer

If two prayers split one small fantasy into two weak weekly choices, merge/repackage before inflating numbers. Combat is the clearest example.

### 10. Avoid hidden double taxation

Do not add new thematic gates merely because they look elegant when the same state already scales the output. Church quality remains the default sermon-success grammar unless a specific rework needs otherwise.

## Revised prayer-by-prayer direction

| Prayer | Current direction after rework-focused pass |
| --- | --- |
| Ordinary | Keep stock. Starter baseline. |
| Faith | Rework specialist peak. Existing `k_faith=1/2/3` remains a strong first model. |
| Donations | Rework specialist peak, but accept natural later obsolescence as other money systems mature. Existing `k_money=1/2/3` remains a first model; do not add extra GQ gate initially. |
| Combo | Keep stock initially. Familiar generalist benchmark. |
| Shoots & Roots | Include the stock scope repair inside the integrated Rebalanced profile; first balance curve remains -20/-30/-40% growth time. Reassess only after real working effect is testable. |
| Repentance | Rework required. `30/50/70%` confession chance remains only a benchmark until exact stock roll cadence/output is captured and modeled. Do not assume chance-only is enough. |
| Repose | Reliability rework remains promising: Bronze stock-style roll, Silver halfway to certainty, Gold guaranteed best prayer-eligible tier, ceiling still bounded by progression. |
| Retribution + Protection | Structural rework remains justified. Prefer one Combat Prayer package/legacy alias over two weak weekly prayers. Current regen numbers require combat-scale modeling before acceptance. |
| Imagination | **Revision:** keep stock `+0.7` Bronze floor. Do not use prior `+0.5/+0.7/+1.0` candidate. Decide later whether Silver/Gold need magnitude scaling after exact linked-writing probability modeling; stock is acceptable initial behavior. |
| Excellence | Rework pressure remains. `+0.2/+0.5/+1.0` is still a candidate, but exact linked-craft scope must be enumerated before acceptance. |
| Prosperity | Keep stock. Healthy progression specialist. |
| BSS Soul's Repose | Keep stock initially. Strong state-dependent specialist; do not balance around community storage exploits without separate evidence. |
| Soul Contentment | Keep stock initially. State-dependent niche; no current evidence justifies +20/+40/+60. |
| Thorough Cleansing | **Keep stock x2.** Do not promote x2/x3/x4 without new evidence; current community behavior marks it as a successful specialist. |

## Data gates before a coherent first Rebalanced candidate

### Gate A — Repentance

Need the exact `church_budka_roll` graph/roll cadence, outputs, and narrow patch seam. This decides whether a 30/50/70 chance ladder is meaningful or whether the prayer needs a broader confession package.

### Gate B — Excellence / Imagination scope

Need exact `linked_buffs` craft lists and enough multi-quality context to model how +0.2/+0.5/+1.0 (or any Imagination premium) changes actual output probabilities and where it saturates.

### Gate C — Combat scale

Before accepting regeneration/damage numbers, compare candidate values to real player HP, representative weapon damage, enemy incoming damage, potion healing and combat duration. The current +5/+8/+12 damage, +4 armor, 3/2/1.5 sec regeneration package remains a hypothesis, not a target.

### Gate D — Repose implementation seam

The design can proceed with existing probability evidence, but implementation still needs the exact corpse-tier RNG selection seam before a runtime candidate.

### Gate E — Roots implementation seam

The integrated Rebalanced profile will still need the already-prepared SmartExpression lifecycle evidence before implementing the repaired/scaled growth term. This is an implementation gate, not a balance-design blocker.

## Recommended staging

1. Close Repentance Gate A first because it can change the prayer's entire role, not just a number.
2. Close Excellence/Imagination Gate B before deciding whether Imagination should change at all and before accepting Excellence magnitudes.
3. Model Combat Gate C before building the merged combat package.
4. Lock the full rebalanced roster in docs.
5. Only then close remaining implementation seams (Roots/Repose/Combat) and build one integrated `dev/*` candidate.

Do not build isolated production fixes solely to prove numbers that can be resolved by research probes/static evidence first.
