# Prayer Design Audit — Graveyard Keeper 1.407

Status: final cross-roster design audit, 2026-09-16. Stock mechanics are verified separately in `PRAYER_MECHANICS.md`. `PRAYER_REBALANCE_OPTIONS.md` is the current concrete Rebalanced roster specification.

No Balance/Rework mechanic is an accepted runtime result until the required implementation/runtime acceptance exists.

## Audit question

The final pass rechecked every prayer after the late design changes:

- specialists must materially reward specialization rather than merely match Combo;
- Bronze must already be credible;
- Silver/Gold must justify premium writing inputs;
- a weekly prayer may be very strong when its niche is narrow;
- duration is valid premium power only when the player can use that extra window;
- natural progression obsolescence is acceptable;
- do not nerf healthy behavior for visual/numerical symmetry;
- requirements must follow the prayer's whole cost proposition rather than one universal q ladder.

The audit specifically asked whether the new Faith/Donations specialist philosophy invalidates any earlier prayer decisions.

## Repository/baseline finding

Current stable PrayerClarity is **1.0.1**, accepted source `7cf6d9287d2aa7cfa8c0529be98f62a3d87360ce`, including the accepted native prayer-item tooltip surface.

The current `research/rework-audit-2026-09-16` branch diverged from `main` before that 1.0.1 work. Therefore future gameplay implementation must start from current stable `main` and carry the accepted research specification forward. Do not build Rebalanced production by treating the research branch's older Clarity source as the runtime base.

No open PR currently carries Rebalanced production work.

## Final audit matrix

| Prayer/family | Audit verdict | Reason |
| --- | --- | --- |
| Ordinary | **No change** | Starter baseline; intentional replacement target. |
| Faith | **Accepted rework** | True resource specialist: +250/+350/+450% Faith, q25/40/70, clean success contribution. |
| Donations | **Accepted rework** | True money specialist: +250/+350/+450%, q25/40/70, +1/+2/+3 silver floor protects early-game role. |
| Combo | **No balance change** | Generalist remains familiar and broader; Hard Book cost but q15/30/60 is easier than specialists. |
| Repentance | **Accepted rework** | 50/75/100% daily confession probability creates a visible reliability ladder and strong Gold week. |
| Shoots & Roots | **Accepted fix + rework** | Stock scope mismatch repaired; -20/-30/-40% gives quality progression in a narrow farming niche. |
| Repose | **Accepted rework** | Reliability ladder improves premium quality without skipping story corpse tiers. |
| Combat | **Accepted structural/numeric rework** | Offense+defense+regen belong to one narrow weekly combat-preparation fantasy. Strong power is intentional. |
| Imagination | **Accepted rework** | Preserve healthy +0.7 core; premium tiers pay through duration and 3 Silver/3 Gold Stories. |
| Excellence | **Accepted rework candidate** | +0.2/+0.5/+1.0 makes expensive upper tiers matter in a narrow quality-craft scope. |
| Prosperity | **No change** | Existing 1/2/3 Blessings already provide a meaningful quality ladder and natural obsolescence. |
| BSS Soul's Repose | **Reopened by audit** | New ordinary Faith specialist would dominate stock BSS Faith output through ordinary Soul Gratitude states. |
| Soul Contentment | **Accepted rework** | +20% all tiers; 36/72/108 min is the quality axis, avoiding magnitude×duration over-scaling. |
| Thorough Cleansing | **No magnitude increase** | x2 is already strong; 36/72/108 min is sufficient premium scaling. |

## Cross-family finding 1 — Faith / Donations / Combo is now coherent

The accepted family has two distinct costs and two distinct benefits.

### Combo

- Hard Book +7 Faith;
- q15/30/60;
- +50/+100/+150% both Faith and donations;
- stock fixed Faith/money outputs.

It is expensive to manufacture but flexible and easier to guarantee.

### Specialists

- Chapter +5 Faith;
- q25/40/70;
- +250/+350/+450% in the chosen resource;
- Faith removes prayer-owned off-theme/fixed outputs;
- Donations keeps only its thematic +1/+2/+3 silver floor besides the percentage.

The q grammar is intentionally simple: specialists are Combo +10 Church Quality at every tier.

The long-horizon design check is also intentional. Over two successful weeks, rotating Faith specialist then Donations specialist should outperform two repeated Combo sermons on each planned target resource. Otherwise there is no strategic reward for maintaining two prayers and planning around them.

## Cross-family finding 2 — BSS Soul's Repose must be reconsidered

This is the one material prayer decision invalidated by the new specialist philosophy.

Verified stock bases are:

- ordinary Faith-family event: `0.2 * CQ * EloquenceFactor`;
- BSS Soul's Repose event: `0.1 * (CQ + GP) * EloquenceFactor`.

Stock BSS Soul's Repose only adds +50/+100/+150% Faith. Once ordinary Faith rises to +250/+350/+450%, stock BSS Soul's Repose is no longer a convincing Faith specialist through ordinary Soul Gratitude states.

### Recommended alignment

Give BSS Soul's Repose the same pure-specialist contract as ordinary Faith:

- +250/+350/+450% Faith;
- q25/40/70;
- preserve the Soul-Gratitude-dependent base formula;
- preserve the Chapter +5 Faith +2 Sin Shards recipe;
- remove prayer-owned fixed Faith/money and off-theme donation percentage.

This creates a stable state-dependent comparison independent of tier and Eloquence:

- GP < CQ -> ordinary Faith wins;
- GP = CQ -> equal Faith output before integer rounding;
- GP > CQ -> Soul's Repose wins.

That is a clearer niche than either permanent dominance or permanent inferiority. The 2 Sin Shards are then paid to exploit a sufficiently developed Soul Gratitude state.

This alignment is a **design recommendation pending user confirmation**, not yet an accepted target.

## Cross-family finding 3 — no global q normalization

The new q grammar should not be projected onto every prayer.

Keeping existing q values elsewhere remains coherent because each proposition is different:

- Combat: deep smithing route + Hard Book +7 Faith + narrow dungeon use already imposes a strong tax; higher q would double-tax the prayer.
- Roots/Repentance: Chapter prayers serving narrow workflows; low q lets them become useful in the progression window where those systems matter.
- Repose: Hard Book plus a finite corpse-progression window.
- Imagination/Excellence: Hard Book and upper q60 already form a premium gate.
- Contentment/Cleansing: late DLC unlock plus Soul-resource recipe costs and state-dependent workflow.

Uniform q would be aesthetic symmetry, not better balance.

## Cross-family finding 4 — do not globally strip secondary Faith/money bonuses

The clean resource-specialist design does **not** imply removing every stock Faith/money contribution from every special prayer.

For Faith/Donations (and recommended BSS Soul's Repose), off-theme success bonuses blur the exact comparison the prayer exists to make.

For Roots, Repentance, Repose, Combat, Imagination, Excellence and the BSS workflow prayers, small existing sermon-resource contributions are an opportunity-cost floor beside the special effect. No evidence shows that they cause choice compression, and deleting them would be a broad nerf for stylistic purity.

Therefore leave those secondary stock sermon contributions alone unless a specific runtime model later demonstrates a problem.

## Cross-family finding 5 — no harmful role collisions found elsewhere

### Repentance vs Faith / Imagination

Gold Repentance can return substantial Faith and Stories, but it requires daily church interaction and gives a mixed Story distribution. It does not replace immediate Faith specialization or Imagination's planned writing-quality window/premium Story reward.

### Roots vs farming substitutes

Magnitude and duration both scale, but the prayer remains constrained to farming and competes with fertilizer, zombies and waiting. The deliberately strong Gold tier is consistent with weekly niche specialization.

### Repose

Gold certainty is strong but bounded by a finite story/progression ceiling. Natural obsolescence remains healthy.

### Combat

The strong Gold package is consistent with the explicit goal that a combat-only weekly choice should feel exceptional. The audit does not justify raising q or reducing regeneration before runtime testing.

### Imagination vs Excellence

Their verified craft scopes are different enough to remain complementary rather than redundant. Imagination's premium reward does not justify increasing its +0.7 core; Excellence still needs magnitude progression.

### Prosperity

No new philosophy undermines its stock 1/2/3 permanent Blessing progression. Its eventual obsolescence is intentional.

### Contentment / Thorough Cleansing

The accepted constant-magnitude + duration structure is internally coherent. Increasing both magnitude and duration would over-reward premium tiers without an identified need.

## Implementation-risk findings from the audit

These do not change the roster but must be closed before acceptance.

### Combat legacy alias must not stack

`b_shield` remains save-safe as a legacy alias of the merged Combat Prayer. Because Combat duration can cross future sermon weeks, production must not allow old `b_sword` and `b_shield` items to create two simultaneously stacking Combat packages. Both identities must resolve to one effective buff lifecycle/refresh rule.

### Semantic model must be profile-aware

Stable 1.0.1 already renders pulpit, Technology, item tooltip and Temporary Effects from Clarity semantics. Rebalanced mechanics and those four presentation surfaces must read the same effective values so no UI surface reports stock numbers while gameplay uses rebalanced numbers.

### VFX are not a gate

Two runtime visual auditions failed to expose a cheap clean native effect. Decorative aura/weapon polish is explicitly outside the first Rebalanced production scope.

## Remaining decisions before roster lock

1. User decision: accept/reject the BSS Soul's Repose alignment above.
2. Product decision before production handoff: decide whether Rebalanced is an opt-in profile/ruleset beside stable Vanilla+Clarity or becomes the default behavior. No per-prayer sliders are planned.

Everything else in the roster is sufficiently closed for implementation-target work.

## Next engineering step after decision closure

After the two product/design questions above are closed:

1. create a new `dev/*` branch from current stable `main` 1.0.1;
2. carry forward the audited roster specification, not old research runtime source;
3. close only the remaining implementation seams (Roots expression lifecycle, Repose RNG, Repentance daily roll, Combat tier/regen/non-stacking alias, Protection recipe retirement);
4. implement one shared effective prayer model used by mechanics and Clarity surfaces;
5. build one coherent candidate and request only the in-game tests that can prove the changed runtime behavior.

No hosted CI is required for this audit/documentation pass.