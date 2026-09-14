# Prayer Rebalance Options — modeled candidate roster

Status: design hypothesis, 2026-09-14. **Nothing here is accepted runtime behavior yet unless explicitly marked as an accepted product/design direction.** Stock 1.407 remains canonical in `PRAYER_MECHANICS.md`; full-cost context is in `PRAYER_POWER_BUDGET.md`.

## Design objective

Use **temptation parity**, not numerical parity. A prayer should justify its technology/craft/quality investment and the weekly sermon slot in the stage or niche where it belongs. Bronze should already be credible; silver/gold should create a meaningful quality chase. Prefer making alternatives attractive over nerfing familiar player rewards.

The free starter `b_empty` Ordinary Prayer is **not** part of the Faith-specialist buff. Keep it stock.

## Candidate roster summary

| Family | Stock issue / role | Leading Rebalanced candidate |
| --- | --- | --- |
| Ordinary `b_empty` | free starter baseline | **stock** |
| Faith `b_faith` | same target Faith coefficient as Combo | **+100 / +200 / +300% Faith** (`k_faith=1/2/3`), keep small stock money side-bonus |
| Donations `b_money` | same target money coefficient as Combo; same tech unlock | **+100 / +200 / +300% donations** (`k_money=1/2/3`), keep small stock Faith side-bonus |
| Combo `b_faith_money` | universal generalist | **stock initially** (`+50/+100/+150%` both) |
| Repentance `b_sins` | special effect disconnected; role known, magnitude lost | **30 / 50 / 70% confession chance**, stock 18/36/54 min |
| Shoots & Roots `b_plant` | proven stock scope mismatch | **Vanilla Fix:** reconnect existing `-20%` growth-time effect; no extra buff initially |
| Repose `b_skull` | +1 max corpse tier at every quality; higher tiers mostly add time | **quality = reliability, not +2/+3 story-tier skipping**; see below |
| Retribution / Protection `b_sword` + `b_shield` | two deep expensive one-stat combat sermons | **soft-merge into one Combat Prayer**, with `b_shield` as save-safe legacy alias; see below |
| Imagination `b_pen` | strong +0.7 at every tier; duration-only quality scaling can be redundant | **+0.7 / +0.9 / +1.1 writing-quality input**, keep 18/36/54 min |
| Excellence `b_star` | narrow +0.2 at every tier; duration-only quality scaling | **+0.2 / +0.3 / +0.4 linked-craft quality input**, keep 18/36/54 min |
| Prosperity `b_village` | permanent merchant progression | **stock:** 1/2/3 Blessings already scales strongly |
| BSS Soul's Repose `b_souls` | state-dependent Faith specialist | **stock mechanics**, dynamic forecast is the main improvement |
| Soul Contentment `b_grat_points_incr` | +10% Soul Gratitude | **stock initially**, clarity/duration |
| Thorough Cleansing `b_sin_shard` | x2 Sin Shards | **stock**, benchmark for a strong narrow specialist |

## Faith / Donations / Combo

Stock specialists have the same target coefficients as Combo at equal quality (`.5/1/1.5`). Their advantages are only cheaper Chapter +5 Faith crafting and lower church-quality requirements versus Combo's Hard Book +7 Faith and q15/30/60. Donations and Combo are even unlocked together by `Price of faith`.

Leading candidate keeps Combo familiar and turns specialists into actual maxima:

- Faith target `k_faith = 1 / 2 / 3`;
- Donations target `k_money = 1 / 2 / 3`;
- keep the small stock secondary coefficient `.2` in the first candidate;
- Combo stays `.5 / 1 / 1.5` for both resources.

Representative no-perk states used for design comparison:

| State | Stock Combo | Faith specialist | Donations specialist |
| --- | --- | --- | --- |
| CQ20 / GQ50, bronze | 7 Faith / 3.25s | **9 / 2.8s** | **6 / 4.0s** |
| CQ40 / GQ100, silver | 18 / 8.0s | **26 / 5.6s** | **12 / 11.0s** |
| CQ80 / GQ200, gold | 43 / 18.0s | **67 / 10.2s** | **22 / 27.0s** |

This creates a legible choice: Combo is the generalist; Faith and Donations are clearly best at their own resource.

### Graveyard Quality

Do **not** add a second GQ gate initially. Base donations already scale with Graveyard Quality, so a larger `k_money` automatically makes a better cemetery more valuable to the specialist prayer. Keep church quality as the universal sermon-success gate. A visible GQ threshold remains a reserve lever only if runtime balance needs it.

## Repentance

Verified stock baseline: 15% confession probability; successful confession yields about 1 Faith + 1 Story; prayer duration is 18/36/54 min; `buff_sins` has no consumer in 1.407.

Leading rework:

- bronze **30%**;
- silver **50%**;
- gold **70%**.

With two confessionals, rough continuous expected additional successful confessions over the full tier window versus ordinary 15% are about **+0.72 / +3.36 / +7.92**. Actual rolls remain discrete/random and timing-dependent.

This keeps the user's initial `15 -> 30%` intuition as a credible bronze while making silver/gold genuinely desirable.

## Shoots and Roots

First candidate is intentionally not a rebalance. Repair the proven stock wiring mismatch so the existing `-20%` growth-time term receives the prayer state. Keep q10/20/30, stock reward coefficients and 36/72/108-minute duration. Reassess only after the repaired effect has been experienced in runtime.

## Combat Prayer — save-safe soft merge

### Accepted product/design direction

Do not delete or rewrite saved prayer items and do not introduce a third permanent prayer ID merely to merge combat roles.

- `b_sword` becomes the canonical Combat Prayer in the Rebalanced profile.
- Existing `b_shield` items remain intact and act as a same-quality **legacy alias** to the same effective combat package.
- New Rebalanced presentation should stop offering the redundant Protection recipe/unlock when it can be done safely.
- No save ID migration is required. Removing the mod/profile restores vanilla interpretation of both original IDs.

### Leading numeric candidate

Keep stock q10/20/40 and durations 36/72/108 initially.

| Quality | Damage | Armor | Regeneration |
| --- | ---: | ---: | ---: |
| Bronze | +5 | +4 | 1 HP per minute |
| Silver | +8 | +4 | 1 HP per minute |
| Gold | +12 | +4 | 1 HP per minute |

Rationale:

- bronze preserves at least the full stock magnitude of **both** formerly separate prayers, so an existing Retribution/Protection owner is not downgraded by the merge;
- armor stays +4 because incoming damage uses flat subtraction; scaling armor to +8/+12 risks nullifying too many attacks;
- damage is additive and is the safer quality-scaling axis;
- regeneration is deliberately slow attrition recovery, not a copied potion buff.

The player's max HP baseline is 100. Vanilla long-heal potion data heals 1 HP every 1.5 seconds; the candidate prayer's 1 HP/min is **40x slower**. Across uninterrupted full duration it could heal at most 36/72/108 HP, so it supports a long dungeon/combat session without replacing burst healing.

If this package is too strong in runtime, first levers are damage curve and/or higher church-quality requirements, not breaking the alias migration model.

## Repose — make quality improve reliability

Direct runtime confirms body generation chooses randomly from all `BodyDefinition`s inside the allowed tier range. There are only a few story corpse tiers, so `+1/+2/+3 max tier` would skip too much progression.

Stock `+1 max` also does **not** mean the next corpse is guaranteed to be better. In simple one-tier-to-next-tier states, current definition counts imply roughly:

- tier 0 -> {0,1}: next-tier chance `12/(3+12) = 80%`;
- tier 1 -> {1,2}: `15/(12+15) ~= 55.6%`;
- tier 2 -> {2,3}: `18/(15+18) ~= 54.5%`.

That explains why the prayer can feel unreliable even while technically working.

Leading candidate keeps the prayer ceiling at **normal story max + 1**, capped at the game's final normal corpse tier:

- **Bronze:** stock pool expansion (`max +1`), stock random selection;
- **Silver:** **90% chance** to choose a body from the highest prayer-eligible tier; remaining 10% comes from lower eligible tiers;
- **Gold:** **100%** body from the highest prayer-eligible tier;
- keep 18/36/54 min.

At the final story tier, bronze naturally loses its tier-expansion benefit, while silver/gold still provide a consistency benefit by strongly preferring/guaranteeing final-tier deliveries. This extends high-quality relevance without revealing future corpse tiers early.

## Imagination

Stock +0.7 is already powerful, and current player evidence shows even a silver Imagination can drive an extremely productive writing batch. Therefore the earlier `+0.7/+1.0/+1.3` proposal is unnecessarily aggressive.

Leading conservative quality curve:

- bronze **+0.7** (stock magnitude);
- silver **+0.9**;
- gold **+1.1**;
- duration remains 18/36/54 min.

This preserves a strong bronze and gives silver/gold a meaningful +0.2/+0.4 score improvement over bronze without making premium writing quality almost automatic across every input combination.

## Excellence

Stock +0.2 is consumed by a narrower set of multiquality crafts including chisels, marble/wood quality crafts and hard-book-related production. Because several relevant perks already contribute sizeable quality scores, a `+0.2/+0.4/+0.6` curve risks saturating quality too easily.

Leading first candidate:

- bronze **+0.2** (stock magnitude);
- silver **+0.3**;
- gold **+0.4**;
- duration remains 18/36/54 min.

This makes quality matter while remaining deliberately less explosive than Imagination.

## Prayers kept stock in the first Rebalanced candidate

### Prosperity

Keep stock. Special output already scales directly **1 / 2 / 3 Commercial Blessings**, providing permanent merchant progression. Bronze has an immediate use and gold triples the special output. Natural obsolescence after merchant tiers are solved is acceptable.

### BSS Soul's Repose

Keep stock mechanics. Soul Gratitude raises the Faith baseline and quality still applies `.5/1/1.5` Faith-bonus scaling to that stronger baseline. At high Gratitude it can materially beat ordinary/Combo Faith. Main fix is dynamic Clarity: show the current expected Faith so the player can see when it is the best choice.

### Soul Contentment / Thorough Cleansing

Keep initially. Soul Contentment's +10% should be shown clearly with duration; Thorough Cleansing's x2 Sin Shards remains the project's benchmark for a narrow but deliberately powerful specialist.

## Candidate status and next gate

The **non-production Rebalanced roster is now complete enough for review**:

- Ordinary: stock;
- Faith: +100/+200/+300% target Faith;
- Donations: +100/+200/+300% target donations;
- Combo: stock;
- Repentance: 30/50/70%;
- Shoots & Roots: repaired stock -20%;
- Combat: soft merge; +5/+8/+12 damage, +4 armor, 1 HP/min, 36/72/108 min;
- Repose: stock / 90% best eligible tier / 100% best eligible tier;
- Imagination: +0.7/+0.9/+1.1;
- Excellence: +0.2/+0.3/+0.4;
- Prosperity and BSS prayers: stock unless runtime testing establishes a concrete problem.

These are **design hypotheses, not accepted gameplay**. The next engineering gate is no longer broad mechanics research. After design review/acceptance of this roster, inspect only the exact lifecycle/Harmony/UI targets required for one shared Clarity semantic model and the narrow gameplay patches, then open a `dev/*` implementation branch and build the first integrated test candidate.