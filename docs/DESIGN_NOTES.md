# PrayerClarity — Design Notes

Status: product/design stage, 2026-09-14. Stock mechanics and the final presentation/confessional audit are sufficiently closed. No production implementation or rebalance numbers are accepted yet.

Detailed prayer-by-prayer judgements live in `docs/PRAYER_DESIGN_AUDIT.md`. Stock behavior remains canonical in `docs/PRAYER_MECHANICS.md`.

## Product problem

Stock Graveyard Keeper 1.407 answers **“will this sermon succeed?”** reasonably well, but poorly answers **“what will this prayer actually do for me now?”**.

The broader design problem is now also established: a prayer costs technology investment, crafting resources, prayer-quality effort, a church-quality success gate and—most importantly—the week's sermon opportunity. A prayer should therefore be a desirable strategic purchase/choice, not merely contain a non-zero buff.

The design target is **temptation parity**: every prayer should present a compelling reason to want it in the stage/niche where it belongs. This does not mean equal numerical power or permanent end-game relevance.

## Product architecture

One codebase may expose three independently controlled semantic layers:

1. **Clarity** — information only.
2. **Vanilla Fixes** — evidence-backed repairs where stock intent/magnitude are recoverable.
3. **Balance / Rework** — intentional new design/tuning, never mislabeled as recovered vanilla behavior.

Potential profiles:

- `Vanilla + Clarity`
- `Fixed Vanilla`
- `Rebalanced`

Keep stock 1.407 mechanics documented independently from every modded profile.

## Permanent product policy — failed-sermon donations

Stock 1.407 still gives the full base donation pool on sermon failure because the nominal 50% participation path is defeated by the integer `Random.Range(0,1)` implementation.

**Do not change this in any profile.** On failure the player keeps base Faith and base donations but loses prayer-specific bonuses/special success outputs.

This is a deliberate player-favourable project policy, not a claim about original developer intent.

## Presentation surfaces

Prayer information is not built by one universal tooltip.

- Technology-tree presentation uses its own technology/unlock path.
- Prayer item description uses `ItemDefinition.GetItemDescription` and sermon additions.
- Pulpit selection uses `PrayCraftGUI` and live state.
- Timed prayer effects use the standard `BuffsGUI` / `PlayerBuff` system.

Probe 0.1.6 closes the active-buff question: `BuffIcon.Draw` assigns the icon and timer behavior, and `BuffIcon.Redraw` only updates remaining time. No prayer-specific dynamic effect description is wired through `BuffIcon`.

**Consequence:** build one mod-owned prayer-information model and render context-appropriate subsets:

- **technology tree:** role / why unlock it / quality progression;
- **prayer item:** exact static properties of this quality tier;
- **pulpit:** current success/failure Faith, donations, special effect/output and duration;
- **active buff:** optionally improve icon hover later, because vanilla currently communicates essentially icon + remaining time rather than effect magnitude.

Do not rely on after-use HUD as a substitute for decision-point clarity.

## Rebalance philosophy

A prayer's power budget includes:

- technology/prerequisite depth and tech-point cost;
- chapter vs book crafting class and Faith/material cost;
- difficulty of producing bronze/silver/gold inputs;
- church-quality success requirement;
- reward/effect magnitude;
- duration and whether it crosses future sermon weeks;
- the stage where the effect is useful;
- the weekly opportunity cost of not using another sermon.

Bronze should already be credible. Silver/gold should provide meaningful additional value through magnitude, duration, outputs, thresholds or reduced effective weekly opportunity cost.

Niche prayers may be stronger than the universal option inside their niche. Progression prayers may become obsolete naturally after doing their job.

Prefer making alternatives attractive over nerfing a familiar player-favourable result. Nerfs require a stronger justification than “the meta exists.”

## Combo as structural reference

Combo is not automatically “overpowered,” because it pays real costs: it is a book-sermon and has higher church-quality thresholds than Faith/Donations.

However, it unlocks relatively early in Theology and, after the production gate is solved, combines the principal Faith and donation percentage bonuses in one universally convenient choice. Community discussion repeatedly shows it becoming the default.

Therefore Combo is a legitimate **Rework candidate** at the choice-structure level.

Do not pick a nerf yet. Candidate approaches include:

- strengthen dedicated Faith/Donation specialization while leaving Combo unchanged;
- apply a generalist tax so Combo is good at both but best at neither;
- increase Combo's church-quality gate;
- make niche prayers sufficiently powerful that choosing them over Combo is exciting rather than self-handicapping.

## Broken prayer policy

### Shoots and Roots

Stock data contains the exact dormant `-20%` growth-time term and the prayer supplies `buff_plant=1`, but the parameter is written to the player while the growth expressions read the growing/workbench WGO. No propagation path exists in the inspected stock path.

This is the cleanest **Vanilla Fix** candidate: reconnect the existing effect using the existing 20% coefficient, then judge its balance after it actually works.

### Repentance

Stock role: more confessions. Base confessional probability is 15%.

The prayer creates `buff_sins`, but repeated code/data/FlowCanvas audits found no consumer. Probe 0.1.6 found the periodic logic entry that invokes `church_budka_roll`, but no surviving code/FlowCanvas reference specifies how `buff_sins` should alter `confession_probability`.

**Final classification:** the role is recoverable, the magnitude/algorithm is not. A working Repentance must therefore be an explicit **Balance / Rework** design, not presented as Vanilla Fix.

## Combat prayers — structural rework question

Retribution and Protection are two separate book-sermons unlocked together by Martial Skills. Each individually costs the weekly sermon choice.

Their stock magnitudes are not trivial (+5 damage and +4 armor) and their 36/72/108-minute durations are much longer than normal consumable buffs. But raw stat size is not the complete value proposition: sustained combat demand is limited and much of the need can be replaced by cautious play or consumables.

This makes them legitimate **Rework candidates** even without proving their raw values are numerically small.

Future options to compare include consolidating them into a stronger combat package, broadening each into a distinct multi-effect offensive/defensive role, or making quality progression materially deepen the combat package. Do not choose an implementation until available stock parameters and progression impact are inspected.

## Quality progression

Duration-only scaling can be meaningful when it crosses weekly boundaries. For 36/72/108-minute buffs, silver/gold can remain active into later sermon weeks, letting the player choose a different sermon while the old buff persists.

Use this pattern deliberately. Do not automatically add magnitude scaling where duration already creates a strong strategic upgrade; conversely, do not treat duration as sufficient where the real use window makes extra time irrelevant.

## Church/graveyard requirements

Church quality is the natural general sermon-success gate because stock mechanics and UI already support it. Stronger reworked prayers may justify higher church requirements.

Do not use graveyard quality as an arbitrary universal gate. Use it only where the relationship is mechanically/thematically clear and player-facing presentation can explain it.

## Preferred Clarity UI

The preferred pulpit hypothesis remains a compact dynamic breakdown backed by the shared semantic model, for example:

- `Faith on success: 18` (`Base 11 · Prayer +7`)
- `Donations on success: 42s`
- `On failure: 11 Faith · 28s`
- `Effect: +5 damage`
- `Duration: 36 min`

The same model must drive technology/item/pulpit wording for the currently selected profile so a fix/rework never leaves stale vanilla text elsewhere.

Do **not** call `PrayLogics.CalculatePray` merely to preview results; the preview must use a side-effect-free deterministic calculation path.

## Current design gate

Do not start broad production code yet.

Next build a quantitative prayer power-budget/progression matrix covering unlock depth/cost, crafting class, quality difficulty, church requirements, weekly opportunity cost, effect/duration, progression window and community use. Then design candidate changes prayer-by-prayer.

The first runtime prototype should be built only after the Clarity model and first accepted rework/fix specification agree on the effective prayer system.