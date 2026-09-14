# Prayer Design Audit — Graveyard Keeper 1.407

Status: design audit, 2026-09-14. Stock mechanics are verified separately. The quantitative power-budget and the first complete non-production Rebalanced roster are now modeled; no gameplay numbers are accepted until runtime/user testing.

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

A prayer costs technology investment, writing/crafting resources, quality effort, sermon-success requirements and one weekly sermon opportunity. Therefore every prayer should offer a compelling reason to acquire/use it in the stage or niche where it belongs.

This is not numerical parity. A narrow specialist may be much stronger in its niche. A progression prayer may become obsolete after doing its job. Bronze should already be credible; silver/gold should materially improve magnitude, reliability, output or useful duration.

## Prayer-by-prayer verdicts

| Prayer | Stock diagnosis | Current design verdict |
| --- | --- | --- |
| Ordinary `b_empty` | free starter | **keep stock**; explicitly excluded from Faith specialist buff |
| Faith `b_faith` | ceases to be a better Faith specialist once Combo gates are solved | **Rework candidate:** target `k_faith 1/2/3` |
| Donations `b_money` | same target money coefficient as Combo and same tech unlock | **Rework candidate:** target `k_money 1/2/3`; no extra GQ gate initially |
| Combo `b_faith_money` | strong universal default | **keep stock initially**; solve compression by specialist premium rather than nerf |
| Shoots & Roots `b_plant` | proven stock wiring mismatch | **Vanilla Fix:** reconnect existing -20% growth-time path; test before further buff |
| Repentance `b_sins` | role known, special effect disconnected, intended magnitude absent | **Balance/Rework:** leading 30/50/70% confession chance |
| Repose `b_skull` | useful role, unreliable outcome; quality mostly adds time | **Rework candidate:** quality improves top-tier reliability, never more than story max +1 |
| Retribution + Protection | two deep book-sermons for two halves of combat preparation | **Rework:** save-safe soft merge into one Combat Prayer; legacy alias for old Protection items |
| Imagination `b_pen` | strong bronze; duration-only quality progression often redundant | **Rework candidate:** +0.7/+0.9/+1.1 writing-quality input |
| Excellence `b_star` | narrow useful effect; duration-only quality progression | **Rework candidate:** +0.2/+0.3/+0.4 linked-craft quality input |
| Prosperity `b_village` | special output already scales 1/2/3 permanent Blessings | **keep stock**; strong progression reference case |
| BSS Soul's Repose `b_souls` | state-dependent Faith specialist with real quality scaling | **keep stock**; dynamic current-result forecast is key |
| Soul Contentment | clear +10% GP workflow buff | **keep stock initially**, clarity/duration |
| Thorough Cleansing | x2 Sin Shards | **keep stock**; benchmark for strong narrow specialist |

## Specialist vs generalist finding

The strongest systemic stock issue is not simply “Combo is too strong.” At equal quality:

- Faith and Combo have identical target Faith coefficients `.5/1/1.5`;
- Donations and Combo have identical target money coefficients `.5/1/1.5`;
- Donations and Combo unlock from the same `Price of faith` technology.

Specialists only retain cheaper Chapter +5 Faith crafting and lower church-quality requirements. Once those gates become routine, Combo gains breadth without sacrificing target-resource effectiveness.

Leading design direction: leave Combo familiar and make Faith/Donations actual maxima with target `k=1/2/3`.

## Repentance

Stock confessional probability is 15%; successful confessions pay roughly 1 Faith + 1 Story. No surviving 1.407 consumer specifies how `buff_sins` should change the roll, so a working prayer is definitively **Balance/Rework**.

Leading candidate 30/50/70% makes the user's initial 30% intuition a credible bronze and gives silver/gold a real quality chase while preserving randomness.

## Combat merge

Accepted architecture if the merge proceeds:

- canonical Rebalanced Combat Prayer uses existing `b_sword` identity;
- existing `b_shield` items remain unchanged in saves and act as same-quality aliases while Rebalanced is active;
- do not rewrite/delete saved item IDs;
- hide/retire the redundant new-player recipe/unlock only if lifecycle inspection proves it safe;
- uninstall/profile change returns the original IDs to vanilla meaning.

Leading numeric candidate is +5/+8/+12 damage, constant +4 armor and very slow 1 HP/min regeneration for 36/72/108 min. Armor is intentionally not scaled because it is flat subtraction.

## Repose quality problem

Direct runtime shows Donkey corpse generation chooses randomly among every `BodyDefinition` inside the allowed tier interval. Stock `body_max+1` therefore merely adds a higher tier to the pool; it does not guarantee a higher corpse.

With current definition counts, simple next-tier chances are about 80% from tier0 and ~55% from tiers1/2. This explains why a functioning prayer can feel weak.

Leading design keeps progression ceiling at normal story max +1:

- bronze: stock random pool extension;
- silver: 85% top prayer-eligible tier;
- gold: 100% top prayer-eligible tier.

This makes quality meaningful through **reliability**, not by skipping +2/+3 story tiers.

## Imagination / Excellence quality problem

Fresh/current player evidence confirms stock Imagination can already be extremely powerful when writing work is batched. Therefore quality should improve magnitude conservatively, not explode it: current lead +0.7/+0.9/+1.1.

Excellence has a narrower linked-craft set and substantial existing perk scores can already push outcomes near quality boundaries; current lead +0.2/+0.3/+0.4 rather than the earlier +0.2/+0.4/+0.6.

## Healthy stock reference cases

- **Prosperity:** 1/2/3 permanent Blessings already gives direct quality scaling.
- **BSS Soul's Repose:** Soul Gratitude raises the Faith baseline and prayer quality further scales Faith bonus; opacity, not raw design, is the main issue.
- **Thorough Cleansing:** x2 Sin Shards is the clearest example of a narrow prayer being intentionally strong enough to justify the weekly slot.

## Current gate

Broad mechanics/community/power-budget research is sufficiently closed. `PRAYER_REBALANCE_OPTIONS.md` now contains a complete first candidate roster.

Next step after user design review is **implementation-target discovery only**: inspect exact UI lifecycle/Harmony targets for a shared side-effect-free Clarity model and the narrow gameplay hooks required by the candidate fixes/reworks. Then open a `dev/*` branch and produce an integrated runtime candidate. No additional user in-game test is required before that build exists.