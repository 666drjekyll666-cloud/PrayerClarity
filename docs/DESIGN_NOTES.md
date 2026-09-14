# PrayerClarity — Design Notes

Status: product/design stage, 2026-09-14. Stock mechanics, presentation/confessional audit, and the quantitative power-budget pass are sufficiently closed. No production implementation or rebalance numbers are accepted yet.

Detailed prayer-by-prayer judgements live in `docs/PRAYER_DESIGN_AUDIT.md`. Quantitative unlock/craft/opportunity-cost analysis lives in `docs/PRAYER_POWER_BUDGET.md`. Stock behavior remains canonical in `docs/PRAYER_MECHANICS.md`.

## Product problem

Stock Graveyard Keeper 1.407 answers **“will this sermon succeed?”** reasonably well, but poorly answers **“what will this prayer actually do for me now?”**.

The broader design problem is now established quantitatively: a prayer costs technology investment, writing/crafting resources, prayer-quality effort, a success gate and—most importantly—the week's sermon opportunity. A prayer should therefore be a desirable strategic purchase/choice, not merely contain a non-zero buff.

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

## Quantitative balance findings

The direct 1.407 power-budget audit changes several earlier qualitative judgements.

### Specialists vs Combo is the strongest systemic issue

Faith and Combo have the same Faith coefficient at equal quality: `.5 / 1 / 1.5`.

Donations and Combo have the same donation coefficient at equal quality: `.5 / 1 / 1.5`.

Donations and Combo are also unlocked together by `Price of faith`. Specialists therefore do not become *better specialists*; they only remain cheaper to craft (Chapter +5 Faith vs Hard Book +7 Faith) and easier to guarantee (q10/20/50 vs Combo q15/30/60).

Those are meaningful early gates, but once books and church quality are routine, Combo gains breadth without giving up target-resource effectiveness.

**Preferred direction:** test a specialist premium before nerfing Combo. Combo should remain a strong generalist; Faith should be best at Faith and Donations best at money.

No coefficient is accepted yet. Candidate families must be simulated at representative progression values before selection.

### Combat prayers pay a very high full budget

Retribution and Protection are two separate Hard-Book prayers unlocked only after a deep Smithing route. Each costs its own Book +7 Faith and its own weekly sermon slot.

Their raw +5 damage/+4 armor are not tiny, but the full proposition is weak enough to justify redesign work: deep unlock, expensive quality, limited sustained-combat demand, and one-dimensional effects that cautious play/consumables can partially replace.

Treat this as a **package-design problem**, not merely “increase +5 to +10.”

### Strong niche prayers are the benchmark

Prosperity, Imagination, high-Gratitude BSS Soul's Repose and Thorough Cleansing demonstrate the desired pattern: a prayer can be narrow or progression-limited and still be exciting because its payoff is large in the relevant window.

## Broken prayer policy

### Shoots and Roots

Stock data contains the exact dormant `-20%` growth-time term and the prayer supplies `buff_plant=1`, but the parameter is written to the player while the growth expressions read the growing/workbench WGO. No propagation path exists in the inspected stock path.

This is the cleanest **Vanilla Fix** candidate: reconnect the existing effect using the existing 20% coefficient, then judge its balance after it actually works.

### Repentance

Stock role: more confessions. Base confessional probability is 15%.

The prayer creates `buff_sins`, but repeated code/data/FlowCanvas audits found no consumer. Probe 0.1.6 found the periodic logic entry that invokes `church_budka_roll`, but no surviving code/FlowCanvas reference specifies how `buff_sins` should alter `confession_probability`.

**Final classification:** the role is recoverable, the magnitude/algorithm is not. A working Repentance must therefore be an explicit **Balance / Rework** design, not presented as Vanilla Fix.

`15% -> 30%` is now recorded only as the first benchmark: it cleanly doubles the stock chance but may be too modest for a dedicated prayer, finite buff and weekly slot. Candidate quality-scaled families such as `30/45/60%` and `30/50/70%` should be evaluated against actual confessional throughput/rewards before acceptance.

## Requirements as balancing levers

### Church quality

Church quality remains the natural universal **sermon delivery/success** gate. Stronger reworked prayers may justifiably require a stronger church.

### Thematic state requirements

A prayer may also have a thematic requirement where the relationship is obvious and useful rather than decorative.

Prayer for Donations is the strongest candidate because **graveyard quality already creates the donation baseline**. However, directly replacing church-based success with graveyard-based success risks breaking the game's universal sermon grammar and double-scaling the same stat.

Preferred design order:

1. keep church quality as the success gate;
2. use graveyard quality as a threshold/scaler for the **specialist premium** of Donations;
3. consider a clear hybrid church + graveyard gate if needed;
4. reserve graveyard-only sermon success for a later option if the simpler designs fail.

This gives the player an intuitive rule: **the church determines whether you can deliver the sermon; the relevant system determines how much a specialist prayer can exploit its niche.**

Do not add thematic gates to every prayer merely for symmetry.

## Quality progression

Bronze should already feel worthwhile. Silver/gold should create meaningful additional value through some combination of magnitude, duration, output, thresholds and reduced effective weekly opportunity cost.

Duration-only scaling can be meaningful when it crosses weekly boundaries. For 36/72/108-minute buffs, silver/gold can remain active into later sermon weeks, letting the player choose a different sermon while the old buff persists.

Use this deliberately. Where the useful activity is short, additional duration alone is not sufficient reason to pursue higher quality.

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

The quantitative power-budget pass is complete enough to stop gathering broad mechanics/cost data. No additional game probe or user runtime test is currently justified.

Next produce a **candidate rebalanced roster** for the Tier-1 design problems:

1. Faith / Donations / Combo specialist-generalist relationship;
2. Repentance confession-throughput design;
3. Retribution / Protection combat-package structure;
4. Shoots and Roots after the known -20% Vanilla Fix.

For each family, compare 2–3 coherent bronze/silver/gold options at representative progression states and select a preferred design hypothesis. Only after that should the first integrated runtime prototype be implemented.