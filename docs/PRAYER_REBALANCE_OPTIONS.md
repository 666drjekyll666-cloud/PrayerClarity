# Prayer Rebalance Options — modeled candidate roster

Status: design hypothesis, updated 2026-09-16. **Nothing here is accepted runtime behavior unless explicitly identified as a product/design rule.** Stock 1.407 remains canonical in `PRAYER_MECHANICS.md`; current rationale/evidence is in `REWORK_RESEARCH.md` and `PRAYER_POWER_BUDGET.md`.

## Product/design rules now treated as direction

- Ship one coherent opinionated **Rebalanced** profile rather than per-prayer tuning sliders.
- Rebalanced may contain the proven Shoots & Roots repair; do not maintain an artificial standalone Fixed Vanilla product merely for one fix.
- A prayer must justify its technology/crafting burden **and** the weekly sermon slot with a clearly felt payoff.
- Bronze should already be credible.
- Silver/Gold must visibly justify their much harder quality-production cost through magnitude, reliability, useful duration, thematic output, or another clear benefit.
- Do not nerf a healthy Bronze merely to create a symmetric tier ladder.
- Duration-only quality is acceptable only when the extra duration materially increases useful expected value.
- Preserve full base donations on failed sermons.

## Current candidate roster

| Family | Current Rebalanced direction |
| --- | --- |
| Ordinary `b_empty` | stock |
| Faith `b_faith` | **+100 / +200 / +300% Faith** as first percent model; evaluate removal of flat/off-theme rewards; stronger q ladder under study |
| Donations `b_money` | **+100 / +200 / +300% donations** as first percent model; evaluate removal of flat/off-theme rewards; stronger q ladder under study |
| Combo `b_faith_money` | stock percentage core **+50/+100/+150% both** initially; generic flat rewards may be removed as part of family cleanup |
| Repentance `b_sins` | quality-scaled confession throughput; old **30/50/70%** remains benchmark only until cadence/reward path is closed |
| Shoots & Roots `b_plant` | repair scope bug + **-20 / -30 / -40% growth time** |
| Repose `b_skull` | Bronze stock selection; Silver halfway from stock best-tier probability to certainty; Gold guarantees best prayer-eligible tier; never exceed progression ceiling |
| Combat (`b_sword` + possible `b_shield` legacy alias) | strong combined combat blessing recommended; former 3/2/1.5-sec regen candidate superseded as too cautious; model ~**1/2/3 HP per second** plus tier-scaled damage and conservative armor |
| Imagination `b_pen` | **keep `craft_q=+0.7` at least at Bronze; current preferred exploration keeps +0.7 all tiers and adds thematic Silver/Gold writing rewards** |
| Excellence `b_star` | **+0.2 / +0.5 / +1.0** remains leading magnitude curve after linked-craft scope verification |
| Prosperity `b_village` | stock 1/2/3 Blessings |
| BSS Soul's Repose `b_souls` | stock initially; already has meaningful quality-scaled Faith coefficient |
| Soul Contentment | magnitude stock initially; explicitly re-audit whether 36/72/108 duration alone justifies premium tiers |
| Thorough Cleansing | keep stock x2 magnitude initially; explicitly re-audit whether 36/72/108 duration alone justifies premium tiers |

## Faith / Donations / Combo — clean percentage-first family

### Terminology

`k_faith` / `k_money` are proportional success modifiers:

- `.5` = +50% of the relevant base;
- `1` = +100%;
- `2` = +200%;
- `3` = +300%.

They are **not** the stock fixed Faith/money output items.

Stock specialists and Combo use the same target-resource percentages at equal quality (`.5/1/1.5`), which is the structural problem: Combo gets both resources without sacrificing specialist peak after its Book/q gates are solved.

### Current first model

- Faith: `k_faith = 1 / 2 / 3`.
- Donations: `k_money = 1 / 2 / 3`.
- Combo: `.5 / 1 / 1.5` in both.

This gives a clean relationship:

- specialist = best at one thing;
- Combo = broad generalist at lower peak.

### Cleanup option now preferred for modeling

Stock also mixes in small fixed rewards and off-theme side percentages. This makes both the mechanics and PrayerClarity presentation harder to read.

Model a cleaner family where:

- Faith has only its Faith percentage premium;
- Donations has only its donation percentage premium;
- Combo has both percentage premiums;
- the generic fixed Faith/money outputs and specialist off-theme `.2` side coefficients are removed.

This is **not accepted yet**. Compare representative total payouts before choosing it.

### Church-quality requirements

If the specialist Gold ceiling rises to +300%, increasing guaranteed-success requirements is legitimate. The old q10/20/50 specialists need not be frozen merely because they are vanilla.

Candidate shapes to model include, for example:

- `10 / 30 / 60`;
- `10 / 30 / 70`;
- `15 / 35 / 70`.

Do not tune against a theoretical maximum-candle church. The requirement should remain reachable in normal cathedral progression and should complement—not erase—the substantial cost of producing Silver/Gold prayer items.

Combo q15/30/60 is a useful existing reference rather than an immutable target.

## Shoots & Roots

Rebalanced should directly include the verified wiring repair and quality curve:

- Bronze: **-20% growth time**;
- Silver: **-30%**;
- Gold: **-40%**;
- durations remain 36/72/108 min unless later playtesting shows an issue.

Preserve the stock expression structure rather than replacing it with an external generic timer multiplier; existing independent terms such as `grow_time` must continue to compose correctly.

## Repentance

Runtime probe 0.1.0 proves `church_budka_roll` independently rolls each of two confessionals against player `confession_probability`.

The old `30 / 50 / 70%` candidate is therefore mechanically expressible, but **not yet quantitatively justified** because the probe did not close:

- invocation cadence;
- exact Confessional I/II reward path;
- the narrow production hook.

Do not keep the old expected-confession totals as authoritative; they assumed a cadence that is not yet directly proven.

Design target: Gold should feel like a genuine high-activity religious/confession period, not merely make an almost invisible event slightly more frequent.

## Combat Prayer — power-fantasy target

### Structural recommendation

The preferred architecture remains a save-safe soft merge:

- `b_sword` becomes the canonical effective Combat Prayer in Rebalanced;
- existing `b_shield` prayer items remain valid and can act as same-quality legacy aliases;
- do not rewrite/delete saved IDs;
- hide/retire duplicate new-player Protection crafting only after lifecycle inspection proves it safe;
- profile/mod removal restores vanilla meaning.

The user has confirmed the required **power level/feel**, but the actual merge still needs explicit confirmation.

### Superseded candidate

The former `+5/+8/+12 damage, +4 armor, 1 HP every 3/2/1.5 sec` package is no longer the leading balance target. The regeneration is too close to cheap consumable convenience for the prayer's full cost and narrow dungeon use.

### New first modeling point

Use a deliberately stronger starting point:

| Quality | Damage | Armor | Regeneration |
| --- | ---: | ---: | ---: |
| Bronze | +5 | +4 | **~1 HP/sec** |
| Silver | +10 | +4 | **~2 HP/sec** |
| Gold | +15 | +4 | **~3 HP/sec** |

These exact values are **not accepted**. They encode the intended test target:

- ordinary enemy hits still matter;
- player must still dodge;
- after clearing a room, a short corridor should restore a noticeably useful chunk of health;
- potion dependence should fall sharply while the weekly blessing is active;
- Gold should feel exceptional enough that a player can reasonably craft/use it once or twice specifically for dungeon progression.

Keep armor conservative because stock armor is flat subtraction and high armor can trivialize low-damage hits much faster than regeneration does.

A holy aura / weapon-glow visual would support the fantasy if a cheap native/event-driven visual seam exists. Treat this as polish research, not a reason for polling or custom heavy effects.

## Repose — premium reliability

Do not scale by `+1/+2/+3 body_max`; that can skip short corpse-progression tiers.

Keep the prayer-eligible ceiling at normal story max +1, capped by final progression, and scale reliability:

- Bronze: ordinary stock selection from the expanded pool.
- Silver: `P(best) = 0.5 + 0.5 * P_vanilla(best)`.
- Gold: 100% best prayer-eligible tier.

Equivalent Silver implementation: 50% force best eligible tier, otherwise perform the normal stock selection.

This gives Silver real premium value and Gold a clear promise without inventing tiers beyond normal progression.

## Imagination — preserve strength, reward premium quality differently

Runtime scope probe confirms `buff_pen` is a broad **writing/prayer-production** buff. It links to Stories, Notes, Chapters, Soul-writing equivalents, and many/all prayer crafting recipes.

Stock `craft_q=+0.7` at Bronze is therefore a healthy meaningful effect. The old `+0.5/+0.7/+1.0` proposal is **superseded** because it nerfs a good Bronze solely for ladder symmetry.

Current preferred design exploration:

- Bronze: `craft_q=+0.7`, 18 min;
- Silver: `craft_q=+0.7`, 36 min **plus a thematic writing reward**;
- Gold: `craft_q=+0.7`, 54 min **plus a larger/premium writing reward**.

Multi-quality Stories are the strongest reward candidate so far:

- they reinforce the same writing fantasy;
- they remain useful inputs to Notes/Chapters;
- they do not skip the complete cover/Book production chain.

Example notions such as several Silver Stories / several Gold Stories are **not accepted quantities**. Determine counts against actual writing recipes and the weekly opportunity cost.

A finished Silver/Gold Hard Book is a weaker design option because it bypasses too much of the premium production chain.

## Excellence — magnitude progression remains appropriate

Runtime scope probe confirms `buff_star` is narrower and distinct from Imagination. Verified linked examples include hard books, high-tier chisels, carved wood and marble work.

Leading curve remains:

- Bronze **+0.2**;
- Silver **+0.5**;
- Gold **+1.0**;
- durations 18/36/54 min.

A Gold `+1.0` quality-score contribution may make reachable high-quality crafts deterministic; that is an intentional premium payoff for a narrow weekly specialist, not automatically overpowered. The finite quality cap still prevents a hidden fourth tier.

## Stock-like reference prayers under the new quality rule

### Prosperity

Keep stock. The special output already scales 1/2/3 permanent Commercial Blessings, so Silver/Gold have obvious value.

### BSS Soul's Repose

Keep stock initially. Its Faith coefficient already scales with prayer quality and Soul Gratitude creates a meaningful state-dependent role.

### Soul Contentment

Do not inflate magnitude merely for symmetry yet. Stock +10% lasts 36/72/108 min, so Silver/Gold may already multiply the number of souls processed under the effect. Quantify actual practical soul cadence before deciding whether quality needs an additional magnitude/reward axis.

### Thorough Cleansing

Keep x2 magnitude initially because community behavior identifies it as a successful specialist. However, x2 at all tiers does **not automatically prove** that Silver/Gold are well-designed. Quantify how many additional soul-processing opportunities the 36/72/108-minute windows realistically cover. Only then decide whether duration alone sufficiently pays for premium prayer quality.

## Open decisions before roster lock

1. Confirm or reject the **Combat soft merge** versus keeping Retribution and Protection as two separately powerful prayers.
2. Decide whether Faith/Donations/Combo should remove stock fixed/off-theme success rewards for a percentage-only clean family.
3. Choose the specialist q ladder after representative payout modeling.
4. Choose Imagination Silver/Gold thematic reward type/count.
5. Close Repentance cadence/reward evidence before locking its chance ladder.
6. Quantify BSS duration-only quality value rather than assuming either stock or large magnitude scaling is correct.

Only after these are narrowed should implementation-only probes/hooks (Roots SmartExpression lifecycle, Repose RNG seam, Combat regen/visual seam) drive a `dev/*` candidate.
