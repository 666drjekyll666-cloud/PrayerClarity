# PrayerClarity — Design Notes

Status: product/design stage, 2026-09-14. Stock mechanics, presentation/confessional audit, and the quantitative power-budget pass are sufficiently closed. No production implementation or numerical rebalance is accepted yet.

Detailed prayer-by-prayer judgements live in `docs/PRAYER_DESIGN_AUDIT.md`. Quantitative unlock/craft/opportunity-cost analysis lives in `docs/PRAYER_POWER_BUDGET.md`. Concrete candidate curves live in `docs/PRAYER_REBALANCE_OPTIONS.md`. Stock behavior remains canonical in `docs/PRAYER_MECHANICS.md`.

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

### Starter Ordinary Prayer stays stock

The specialist-premium proposal applies to the separately crafted `b_faith` Prayer for Faith, **not** the free `b_empty` Ordinary Prayer supplied at the start.

Do not accidentally accelerate the opening game by turning the starter sermon into the new `+100%` Faith specialist. Any future starter-sermon change requires its own early-game justification.

### Specialists vs Combo is the strongest systemic issue

Faith and Combo have the same Faith coefficient at equal quality: `.5 / 1 / 1.5`.

Donations and Combo have the same donation coefficient at equal quality: `.5 / 1 / 1.5`.

Donations and Combo are also unlocked together by `Price of faith`. Specialists therefore do not become *better specialists*; they only remain cheaper to craft (Chapter +5 Faith vs Hard Book +7 Faith) and easier to guarantee (q10/20/50 vs Combo q15/30/60).

Those are meaningful early gates, but once books and church quality are routine, Combo gains breadth without giving up target-resource effectiveness.

**Preferred direction:** test a specialist premium before nerfing Combo. Current leading curve in `PRAYER_REBALANCE_OPTIONS.md` is `+100/+200/+300%` target output for Faith/Donations while Combo remains stock.

### Combat prayers are now a merge-design problem

Retribution and Protection are two separate Hard-Book prayers unlocked only after a deep Smithing route. Each costs its own Book +7 Faith and its own weekly sermon slot.

The user preference is to replace the two one-stat offerings with **one genuinely strong combat-preparation prayer**, including offense, defense and regeneration if the package remains bounded.

The mod-specific migration problem can be solved without deleting saved items:

- keep one existing ID as canonical Combat Prayer;
- hide the second recipe/unlock for new Rebalanced play;
- keep already-crafted copies of the second ID as same-quality **legacy aliases** of Combat Prayer;
- do not rewrite inventory/save item IDs;
- if the mod is removed, vanilla receives the original IDs again.

This soft-merge/legacy-alias strategy is the preferred compatibility direction. Exact combat numbers are still open.

### Quality progression needs a second audit, not just prayer-role classification

Earlier design notes treated several prayers as healthy because their **role** was good. That was incomplete: bronze/silver/gold can still be poorly differentiated even when the prayer has a useful niche.

Current split:

- **Prosperity:** stock quality already outputs 1/2/3 permanent Blessings of Commerce; this is meaningful scaling.
- **BSS Soul's Repose:** quality multiplies a Soul-Gratitude-enhanced Faith baseline; this is already strong value scaling and mainly needs dynamic Clarity.
- **Imagination:** role is strong, but all qualities provide the same `+0.7` special magnitude; duration-only quality is suspect because writing can be batched. Rework candidate.
- **Repose:** role is legitimate, but all qualities provide the same `+1 max corpse tier`; duration-only quality is suspect. Rework candidate.
- **Excellence:** fixed `+0.2` special magnitude across tiers creates the same quality question and is reopened for modeling.

## Broken prayer policy

### Shoots and Roots

Stock data contains the exact dormant `-20%` growth-time term and the prayer supplies `buff_plant=1`, but the parameter is written to the player while the growth expressions read the growing/workbench WGO. No propagation path exists in the inspected stock path.

This is the cleanest **Vanilla Fix** candidate: reconnect the existing effect using the existing 20% coefficient, then judge its balance after it actually works.

### Repentance

Stock role: more confessions. Base confessional probability is 15%.

The prayer creates `buff_sins`, but repeated code/data/FlowCanvas audits found no consumer. Probe 0.1.6 found the periodic logic entry that invokes `church_budka_roll`, but no surviving code/FlowCanvas reference specifies how `buff_sins` should alter `confession_probability`.

**Final classification:** the role is recoverable, the magnitude/algorithm is not. A working Repentance must therefore be an explicit **Balance / Rework** design, not presented as Vanilla Fix.

Current leading hypothesis is `30% / 50% / 70%` by bronze/silver/gold. The user's original `15 -> 30%` idea is retained as a strong bronze baseline rather than the final ceiling.

## Requirements as balancing levers

### Church quality

Church quality remains the natural universal **sermon delivery/success** gate. Stronger reworked prayers may justifiably require a stronger church.

### Thematic state requirements

A prayer may also have a thematic requirement where the relationship is obvious and useful rather than decorative.

Prayer for Donations is the strongest candidate because **graveyard quality already creates the donation baseline**. However, directly replacing church-based success with graveyard-based success risks breaking the game's universal sermon grammar and double-scaling the same stat.

Preferred design order:

1. keep church quality as the success gate;
2. first exploit the existing fact that Graveyard Quality naturally scales the larger specialist donation bonus;
3. add a visible Graveyard Quality threshold only if playtesting shows an additional progression gate is needed;
4. consider a hybrid gate only after the simpler design is tested.

This gives the player an intuitive rule: **the church determines whether you can deliver the sermon; the relevant system determines how much a specialist prayer can exploit its niche.**

Do not add thematic gates to every prayer merely for symmetry.

## Quality progression details

Bronze should already feel worthwhile. Silver/gold should create meaningful additional value through some combination of magnitude, duration, output, thresholds and reduced effective weekly opportunity cost.

Duration-only scaling can be meaningful when it crosses weekly boundaries. For 36/72/108-minute buffs, silver/gold can remain active into later sermon weeks, letting the player choose a different sermon while the old buff persists.

But duration-only scaling is weak when the relevant work can simply be stockpiled and completed inside the bronze window.

### Imagination

Stock `+0.7` is genuinely powerful and should remain a credible bronze prayer. Current leading quality-rework hypothesis is:

- bronze `+0.7`;
- silver `+1.0`;
- gold `+1.3`;
- keep 18/36/54 min initially.

This preserves the proven bronze use case while making higher quality improve actual writing probabilities, not just spare time.

### Repose

Do **not** simply use `+1/+2/+3 corpse tiers`. Runtime/story progression has only a few Donkey corpse tiers; +3 can jump several story stages.

Preferred direction is to keep the special reach to at most **one tier above the current story maximum**, but make higher prayer quality improve how reliably the Donkey delivers the boosted tier. Bronze can preserve stock max+1 behavior; gold can aim for reliable best-boosted-tier deliveries while active.

This makes gold strong without sequence-breaking the whole corpse progression.

### Excellence

Reopen quality scaling. A simple `+0.2/+0.4/+0.6` candidate is worth modeling because the effect is narrow and duration-only scaling may not justify silver/gold. No curve is accepted yet.

## Preferred Clarity UI

The preferred pulpit hypothesis remains a compact dynamic breakdown backed by the shared semantic model, for example:

- `Faith on success: 18` (`Base 11 · Prayer +7`)
- `Donations on success: 42s`
- `On failure: 11 Faith · 28s`
- `Effect: +5 damage`
- `Duration: 36 min`

For reworked prayers the same UI must explain effective quality scaling explicitly—for example `Inspiration +0.7` vs `+1.0`, or `Donkey: next-tier corpse chance/reliability`—so the player can see why silver/gold are desirable.

The same model must drive technology/item/pulpit wording for the currently selected profile so a fix/rework never leaves stale vanilla text elsewhere.

Do **not** call `PrayLogics.CalculatePray` merely to preview results; the preview must use a side-effect-free deterministic calculation path.

## Current design gate

The broad mechanics/cost research phase is closed. No additional game probe or user runtime test is currently justified.

Current leading candidate directions are:

1. Faith / Donations specialists `+100/+200/+300%`, Combo stock;
2. Repentance `30/50/70%` confession chance;
3. Shoots and Roots stock-intent `-20%` repair;
4. one combined Combat Prayer with save-safe legacy alias for the removed second recipe, exact package open;
5. Prosperity stock;
6. BSS Soul's Repose stock + dynamic Clarity;
7. Imagination quality magnitude scaling candidate;
8. Repose quality/consistency rework without multi-tier progression skipping;
9. Excellence reopened for quality scaling.

Next narrow work is **candidate-roster completion**, not broad discovery:

- quantify the combat package including bounded regeneration;
- quantify Repose delivery consistency at real story tier ranges;
- model Imagination/Excellence quality probabilities;
- then freeze one complete non-production Rebalanced specification before opening a `dev/*` implementation branch.
