# Prayer Rebalance Options — modeled candidate roster

Status: design hypothesis, 2026-09-14. **Nothing here is accepted runtime behavior yet unless explicitly marked as an accepted product/design direction.** Stock 1.407 remains canonical in `PRAYER_MECHANICS.md`; full-cost context is in `PRAYER_POWER_BUDGET.md`.

## Design objective

Use **temptation parity**, not numerical parity. A prayer should justify its technology/craft/quality investment and the weekly sermon slot in the stage or niche where it belongs. Bronze should already be credible; silver/gold should create a meaningful quality chase. Gold may deliberately create reliability or a very strong niche window. Prefer making alternatives attractive over nerfing familiar player rewards.

The free starter `b_empty` Ordinary Prayer is **not** part of the Faith-specialist buff. Keep it stock.

## Current candidate roster

| Family | Leading Rebalanced candidate |
| --- | --- |
| Ordinary `b_empty` | **stock** |
| Faith `b_faith` | **+100 / +200 / +300% Faith** (`k_faith=1/2/3`), stock small money side-bonus |
| Donations `b_money` | **+100 / +200 / +300% donations** (`k_money=1/2/3`), stock small Faith side-bonus |
| Combo `b_faith_money` | **stock initially** (`+50/+100/+150%` both) |
| Repentance `b_sins` | **30 / 50 / 70% confession chance**, stock 18/36/54 min |
| Shoots & Roots `b_plant` | Fixed Vanilla: repaired **-20%** all tiers; Rebalanced candidate **-20 / -30 / -40% growth time** |
| Repose `b_skull` | Bronze stock random; Silver halfway between current vanilla best-tier chance and 100%; Gold 100% best prayer-eligible tier; never exceed story max +1 |
| Combat (`b_sword` + legacy `b_shield`) | save-safe merge; **+5/+8/+12 damage, +4 armor, regen 1 HP every 3/2/1.5 sec**, 36/72/108 min |
| Imagination `b_pen` | **+0.5 / +0.7 / +1.0** writing-quality input, 18/36/54 min |
| Excellence `b_star` | **+0.2 / +0.5 / +1.0** linked-craft quality input, 18/36/54 min |
| Prosperity `b_village` | stock 1/2/3 Blessings |
| BSS Soul's Repose `b_souls` | stock mechanics; dynamic Faith forecast |
| Soul Contentment | stock initially |
| Thorough Cleansing | stock x2 Sin Shards |

These values are a **first integrated test roster**, not accepted balance.

## Faith / Donations / Combo

Stock specialists have the same target coefficients as Combo at equal quality (`.5/1/1.5`). Their advantages are only cheaper Chapter +5 Faith crafting and lower church-quality requirements versus Combo's Hard Book +7 Faith and q15/30/60. Donations and Combo are unlocked together by `Price of faith`.

Leading candidate keeps Combo familiar and turns specialists into actual maxima:

- Faith target `k_faith = 1 / 2 / 3`;
- Donations target `k_money = 1 / 2 / 3`;
- keep the small stock secondary coefficient `.2`;
- Combo stays `.5 / 1 / 1.5` for both resources.

Representative no-perk states used for design comparison:

| State | Stock Combo | Faith specialist | Donations specialist |
| --- | --- | --- | --- |
| CQ20 / GQ50, bronze | 7 Faith / 3.25s | **9 / 2.8s** | **6 / 4.0s** |
| CQ40 / GQ100, silver | 18 / 8.0s | **26 / 5.6s** | **12 / 11.0s** |
| CQ80 / GQ200, gold | 43 / 18.0s | **67 / 10.2s** | **22 / 27.0s** |

This preserves Combo as generalist while making specialist quality worth pursuing.

Do **not** add a second Graveyard Quality gate initially. Base donations already scale with GQ, so stronger `k_money` naturally amplifies the value of a developed cemetery. Keep church quality as universal sermon-success gate; reserve an explicit GQ threshold only if runtime balance needs it.

## Repentance

Verified stock baseline: 15% confession probability; successful confession yields about 1 Faith + 1 Story; prayer duration is 18/36/54 min; `buff_sins` has no consumer in 1.407.

Leading rework: **30 / 50 / 70%**. With two confessionals, rough continuous expected additional successful confessions over the full tier windows versus ordinary 15% are about **+0.72 / +3.36 / +7.92**. Actual rolls remain discrete/random and timing-dependent.

## Shoots and Roots

### Fixed Vanilla profile

Repair the proven parameter-scope bug only. Preserve the dormant stock coefficient: **-20% growth time** at every quality; quality continues to scale duration 36/72/108 min.

### Rebalanced profile

The user's proposed quality curve is now the leading candidate:

- bronze **-20% growth time**;
- silver **-30%**;
- gold **-40%**;
- duration remains 36/72/108 min.

This corresponds to approximate cycle-throughput increases of +25%, +42.9% and +66.7% while active. Because the prayer is narrow and costs the weekly sermon slot, this is a reasonable first temptation-parity candidate rather than an obviously excessive buff.

## Combat Prayer — save-safe soft merge

### Accepted migration direction

Do not delete or rewrite saved prayer items and do not add a third permanent prayer ID merely to merge combat roles.

- `b_sword` is the canonical Combat Prayer in Rebalanced.
- Existing `b_shield` items stay intact and act as same-quality **legacy aliases** to the effective Combat Prayer.
- Retire/hide the redundant new-player Protection recipe only after lifecycle inspection proves that safe.
- No save ID migration. Removing the mod/profile restores vanilla interpretation.

### Leading combat package

Keep q10/20/40 and durations 36/72/108 initially.

| Quality | Damage | Armor | Regeneration |
| --- | ---: | ---: | ---: |
| Bronze | +5 | +4 | **1 HP / 3 sec** |
| Silver | +8 | +4 | **1 HP / 2 sec** |
| Gold | +12 | +4 | **1 HP / 1.5 sec** |

Rationale:

- bronze includes at least the full stock offense and defense of the two prayers being merged;
- armor stays +4 because incoming armor is flat subtraction and larger values can nullify low-damage enemies;
- damage is the safer magnitude-scaling axis;
- vanilla long-heal potion itself heals 1 HP every 1.5 seconds, so gold reaches an already-existing healing cadence rather than inventing a faster one;
- the weekly sermon, deep unlock, expensive Book and combat-only niche justify a much stronger sustained-healing fantasy than the rejected 1 HP/min proposal.

The approximate raw healing ceilings are 20/30/40 HP per minute before the HP cap. This may make low-pressure combat extremely forgiving when combined with +4 armor; that is **intentional enough to test**, not sufficient reason to pre-nerf it. The aggressive fallback if gold still feels insufficient is 1 HP/sec; the conservative fallback is to lengthen only the gold tick to 2 sec.

## Repose — quality as reliability

Direct runtime shows corpse generation randomly selects from all `BodyDefinition`s in the allowed tier range. Stock `body_max+1` therefore opens a better tier but does not guarantee the Donkey chooses it. In representative adjacent-tier pools, the stock chance of the newly opened best tier is roughly 80% on the first transition and ~55% on later transitions.

Do not use `+1/+2/+3 max tier`: corpse tiers encode a short story progression and that would skip several stages.

Instead keep the ceiling at **normal story max +1**, capped at the final normal corpse tier:

- **Bronze:** stock selection from the expanded pool.
- **Silver:** exactly halfway between the current stock probability of the best prayer-eligible tier and certainty.
- **Gold:** 100% best prayer-eligible tier.

Implementation rule for silver:

`P_silver(best) = 0.5 + 0.5 * P_vanilla(best)`.

Equivalent implementation: on each delivery, 50% force the best prayer-eligible tier; otherwise perform the ordinary vanilla selection. Examples:

- vanilla 80% -> silver **90%** -> gold 100%;
- vanilla 55.6% -> silver **77.8%** -> gold 100%;
- vanilla 54.5% -> silver **77.3%** -> gold 100%.

This satisfies the design requirement that silver sit genuinely between bronze and gold instead of being almost-gold in every progression state.

## Imagination — global quality/cap check

Stock Imagination adds `craft_q=+0.7` at every prayer quality. Writing recipes consume the same additive quality-score system as linked writing perks; current direct data includes Writer +0.3, Playwright/Good Writer +0.5 and Industriousness +0.2. The prayer buff is added to the same `value_perks` bucket used by those perks.

The output system has finite quality tiers, so extra score naturally saturates once the best available output is already guaranteed. This means late-game redundancy is real and acceptable: a Clarity preview should explicitly tell the player when a stronger Imagination tier no longer improves the selected writing craft.

Leading curve adopts the user's proposal:

- bronze **+0.5**;
- silver **+0.7** (stock magnitude moves here);
- gold **+1.0**;
- 18/36/54 min unchanged.

Why this is preferable to the previous +0.7/+0.9/+1.1 candidate:

- it avoids accelerating early writing as much as stock bronze currently can;
- silver preserves vanilla special-effect strength while lasting longer;
- gold adds a full quality-score point, which is a clear premium tier and can turn a near-next-tier roll into deterministic next quality where the recipe is not already capped;
- if perks/input quality already guarantee gold output, the extra prayer magnitude is correctly worthless rather than creating a hidden fourth quality tier.

The future UI should show the **actual predicted output probabilities/current cap**, not merely `+1.0`, whenever feasible.

## Excellence — reliability is the point

Stock Excellence adds `craft_q=+0.2` at every quality to explicitly linked multiquality crafts. Relevant permanent perk stars are already substantial in several craft families (for example Woodworker +0.5, Mason +0.5, Engineer +0.3, Jeweler +0.7, Industriousness +0.2), and the prayer contribution is additive with them.

Because Excellence is narrower than Imagination and consumes the same weekly sermon opportunity, making gold capable of turning a reachable high-quality craft into a deterministic result is an acceptable **feature**, not automatically an imbalance.

Leading curve:

- bronze **+0.2** (stock magnitude);
- silver **+0.5**;
- gold **+1.0**;
- 18/36/54 min unchanged.

A +1.0 score does **not** mean every craft universally becomes gold. It means the prayer supplies a full quality-score point to crafts that explicitly link `buff_star`; input quality, recipe difficulty, linked perks and the finite output tiers still determine whether that crosses a threshold. If the selected craft is already capped/guaranteed, further score is wasted. Clarity should expose that rather than hiding it.

## Stock reference prayers

- **Prosperity:** keep stock; special output already scales 1/2/3 permanent Commercial Blessings.
- **BSS Soul's Repose:** keep stock; Soul Gratitude raises the Faith baseline and quality scales its Faith bonus. Dynamic current-result forecast is the main improvement.
- **Soul Contentment:** stock initially; explain +10% and duration.
- **Thorough Cleansing:** stock x2 Sin Shards; benchmark for a strong narrow specialist.

## Candidate status and next gate

The first integrated non-production Rebalanced specification is now coherent enough for implementation-target discovery:

- Ordinary stock;
- Faith/Donations +100/+200/+300% target resource;
- Combo stock;
- Repentance 30/50/70%;
- Shoots & Roots: Fixed Vanilla -20%; Rebalanced -20/-30/-40%;
- Combat soft merge: +5/+8/+12 damage, +4 armor, regen 3/2/1.5 sec;
- Repose: vanilla / midpoint-to-certainty / 100% best eligible tier;
- Imagination +0.5/+0.7/+1.0;
- Excellence +0.2/+0.5/+1.0;
- Prosperity/BSS reference prayers stock initially.

These remain **design hypotheses pending runtime/user acceptance**. Next inspect only exact UI/lifecycle/Harmony targets needed for the shared Clarity semantic model and these narrow gameplay changes, then open a `dev/*` branch and build the first integrated candidate. No further broad mechanics research is justified before that implementation work.