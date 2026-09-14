# Prayer Design Audit — Graveyard Keeper 1.407

Status: design audit, 2026-09-14. Stock mechanics are verified separately. The quantitative power-budget and first integrated non-production Rebalanced roster are now modeled; gameplay numbers remain unaccepted until runtime/user testing.

Sources of truth:

- `PRAYER_MECHANICS.md` — stock 1.407 mechanics;
- `PLAYER_UX_RESEARCH.md` — player/presentation evidence;
- `PRAYER_POWER_BUDGET.md` — unlock/craft/quality/weekly-slot cost;
- `PRAYER_REBALANCE_OPTIONS.md` — current concrete candidate values.

## Product layers

- **Clarity** — information only.
- **Vanilla Fixes** — evidence-backed repair where stock intent/magnitude are recoverable.
- **Balance / Rework** — intentional new design/tuning, never presented as recovered vanilla behavior.

Possible profiles remain `Vanilla + Clarity`, `Fixed Vanilla`, and `Rebalanced`.

## Permanent policy — failed-sermon donations

Preserve the stock player-favourable outcome: failed sermons still pay the full **base** donation pool while prayer-specific bonuses/special success outputs disappear. Do not repair the apparent 50% participation path in a way that reduces player rewards.

## Core balance rule — temptation parity

A prayer costs technology investment, writing/crafting resources, quality effort, sermon-success requirements and one weekly sermon opportunity. Every prayer should therefore offer a compelling reason to acquire/use it in the stage or niche where it belongs.

This is not numerical parity. A narrow specialist may be much stronger in its niche. A progression prayer may become obsolete after doing its job. Bronze should already be credible; silver/gold should materially improve magnitude, reliability, output or useful duration. **Reliability itself is valid premium power** when the underlying activity is probabilistic.

## Current prayer-by-prayer verdicts

| Prayer | Stock diagnosis | Current design verdict |
| --- | --- | --- |
| Ordinary `b_empty` | free starter | **keep stock**; excluded from Faith specialist buff |
| Faith `b_faith` | no better at Faith than equal-quality Combo once gates are solved | **Rework candidate:** target `k_faith 1/2/3` |
| Donations `b_money` | same target money coefficient as Combo and same tech unlock | **Rework candidate:** target `k_money 1/2/3`; no extra GQ gate initially |
| Combo `b_faith_money` | strong universal default | **keep stock initially**; solve compression by specialist premium |
| Shoots & Roots `b_plant` | proven stock wiring mismatch | **Fixed Vanilla:** reconnect -20%; **Rebalanced:** -20/-30/-40% by prayer quality |
| Repentance `b_sins` | role known, special effect disconnected, intended magnitude absent | **Balance/Rework:** 30/50/70% confession chance candidate |
| Repose `b_skull` | useful role, but +1 max only expands a random pool and quality mostly adds time | **Rework:** bronze vanilla; silver midpoint from current vanilla best-tier chance to certainty; gold 100%; never more than story max +1 |
| Retribution + Protection | two deep book-sermons for two halves of combat preparation | **Rework:** save-safe soft merge into Combat Prayer; legacy alias; strong regen is part of the fantasy |
| Imagination `b_pen` | stock +0.7 at every tier; early power high, duration-only quality can be redundant | **Rework:** +0.5/+0.7/+1.0; expose saturation/current predicted output |
| Excellence `b_star` | very narrow +0.2 at every tier; premium quality lacks payoff | **Rework:** +0.2/+0.5/+1.0; gold may intentionally make reachable high-quality crafts deterministic |
| Prosperity `b_village` | output already scales 1/2/3 permanent Blessings | **keep stock** |
| BSS Soul's Repose `b_souls` | state-dependent Faith specialist with real quality scaling | **keep stock**; dynamic current-result forecast |
| Soul Contentment | clear +10% GP workflow buff | **keep stock initially**, clarity/duration |
| Thorough Cleansing | x2 Sin Shards | **keep stock**; strong-specialist benchmark |

## Specialist/generalist finding

At equal quality Faith and Combo share `.5/1/1.5` Faith coefficients, while Donations and Combo share `.5/1/1.5` money coefficients. Donations and Combo also unlock from the same `Price of faith` technology.

Specialists only retain cheaper Chapter +5 Faith crafting and lower church-quality requirements. Once those gates are routine, Combo gains breadth without sacrificing target-resource power.

Leading solution: leave Combo familiar and make Faith/Donations actual maxima at `k=1/2/3`.

## Shoots and Roots

The stock bug is a Vanilla Fix: prayer state is written to the player while growth expressions read the growing/workbench WGO. The dormant coefficient is -20%.

For Rebalanced, quality scaling **-20/-30/-40% growth time** is now preferred. This is a deliberate balance layer, not recovered vanilla. The corresponding cycle-rate increases are about +25/+42.9/+66.7%, which is appropriately strong for a narrow weekly farming sermon.

## Repentance

Stock confessional probability is 15%; successful confessions pay roughly 1 Faith + 1 Story. No surviving 1.407 consumer specifies how `buff_sins` should alter the roll, so a working prayer is definitively Balance/Rework.

Leading candidate 30/50/70% preserves randomness but makes gold a real production strategy.

## Combat merge

Accepted migration architecture if the merge proceeds:

- canonical Rebalanced Combat Prayer uses existing `b_sword` identity;
- existing `b_shield` items remain unchanged in saves and act as same-quality aliases;
- do not rewrite/delete saved item IDs;
- retire the redundant recipe only if lifecycle inspection proves it safe;
- mod/profile removal returns original IDs to vanilla meaning.

Leading first test package:

- damage **+5/+8/+12**;
- armor **+4** at all tiers because armor is flat subtraction;
- regeneration **1 HP every 3/2/1.5 seconds**;
- duration 36/72/108 min.

The old 1 HP/min concept is rejected as not remotely tempting enough. Vanilla's own long-heal potion uses 1 HP every 1.5 seconds, so gold matching that cadence is a defensible prayer fantasy rather than an unprecedented mechanic. The combined armor + regen may make low-pressure combat extremely forgiving; that is a property to runtime-test, not a reason to pre-emptively make the prayer unattractive.

## Repose — premium reliability instead of story skipping

Direct runtime shows Donkey body generation randomly chooses from every body definition inside the current tier interval. Stock `body_max+1` adds a higher tier to the candidate pool but does not guarantee it. Depending on the current adjacent tiers, the newly opened top-tier chance can be roughly 80% early and ~55% later.

The user's `+1/+2/+3 tier` intuition captures the desire for stronger quality but risks skipping several short story progression tiers. The preferred alternative keeps the ceiling at story max +1 and scales **reliability**:

- bronze: ordinary vanilla selection from expanded pool;
- silver: `P(best) = (P_vanilla(best)+1)/2`, exactly halfway to certainty;
- gold: 100% best prayer-eligible tier.

Equivalent silver implementation: 50% force the best eligible tier, otherwise make the ordinary vanilla roll. This means silver automatically lands in the correct numerical middle for the player's current progression instead of using one arbitrary global 90%.

## Imagination — cap/saturation analysis

The writing multiquality path adds linked perk stars and linked prayer-buff `craft_q` into the same quality-score bucket. Direct data includes Writer +0.3, Good Writer +0.5 and Industriousness +0.2. The output still has finite quality tiers, so score above the top reachable quality is naturally wasted.

This supports the user's quality curve **+0.5/+0.7/+1.0** better than the previous +0.7/+0.9/+1.1 proposal:

- bronze remains useful without front-loading the full stock +0.7 into the earliest version;
- silver preserves stock magnitude;
- gold contributes a full quality-score point and can make the next quality deterministic when the recipe/input/perks are close enough;
- late-game redundancy is acceptable and should be made explicit by Clarity. If a craft is already guaranteed gold, stronger Imagination should be shown as providing no further output benefit.

## Excellence — gold reliability is desirable

Excellence is narrower than Imagination. Direct perk data includes sizeable permanent contributions such as Woodworker +0.5, Mason +0.5, Engineer +0.3, Jeweler +0.7 and Industriousness +0.2. The prayer contribution is additive with those values and only affects crafts explicitly linked to `buff_star`.

The previous +0.2/+0.3/+0.4 proposal was too timid relative to the weekly slot and narrow eligibility. Leading candidate is now **+0.2/+0.5/+1.0**.

Gold intentionally supplies a full quality-score point. This does not guarantee gold for every recipe from any inputs; it makes a reachable next quality deterministic where the remaining gap is <=1 and still saturates at the game's finite output cap. That is an appropriate premium-prayer payoff.

## Healthy stock reference cases

- **Prosperity:** 1/2/3 permanent Blessings already gives direct quality scaling.
- **BSS Soul's Repose:** Soul Gratitude raises Faith baseline and prayer quality further scales Faith bonus; opacity, not raw design, is the main issue.
- **Thorough Cleansing:** x2 Sin Shards is the clearest example of a narrow prayer being intentionally strong enough to justify the weekly slot.

## Current gate

Broad mechanics/community/power-budget research is sufficiently closed. `PRAYER_REBALANCE_OPTIONS.md` now contains the revised first integrated roster.

Next step is **implementation-target discovery only**: inspect exact UI lifecycle/Harmony targets for one shared side-effect-free Clarity model and the narrow gameplay hooks required by these candidate fixes/reworks. Then open a `dev/*` branch and produce an integrated runtime candidate. No additional user in-game test is required before that build exists.