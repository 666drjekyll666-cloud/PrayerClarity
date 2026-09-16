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

Current stable PrayerClarity at the time of this audit is **1.0.1**, accepted source `7cf6d9287d2aa7cfa8c0529be98f62a3d87360ce`, including the accepted native prayer-item tooltip surface.

The `research/rework-audit-2026-09-16` branch diverged from `main` before that work. In addition, Clarity is still being refined in parallel. Therefore Rebalanced production must **not** start from this research branch and must not freeze prematurely to 1.0.1. It should start from the next accepted Clarity baseline after the current Clarity work settles.

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
| Excellence | **Accepted first candidate** | +0.2/+0.5/+1.0 makes expensive upper tiers matter in a narrow quality-craft scope. |
| Prosperity | **No change** | Existing 1/2/3 Blessings already provide a meaningful quality ladder and natural obsolescence. |
| BSS Soul's Repose | **Accepted rework** | Aligns with specialist philosophy while preserving a clean Soul-Gratitude-dependent niche. |
| Soul Contentment | **Accepted rework** | +20% all tiers; 36/72/108 min is the quality axis, avoiding magnitude×duration over-scaling. |
| Thorough Cleansing | **No magnitude increase** | x2 is already strong; 36/72/108 min is sufficient premium scaling. |

## Cross-family finding 1 — Faith / Donations / Combo is coherent

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

## Cross-family finding 2 — BSS Soul's Repose is now aligned

Verified stock bases are:

- ordinary Faith-family event: `0.2 * CQ * EloquenceFactor`;
- BSS Soul's Repose event: `0.1 * (CQ + GP) * EloquenceFactor`.

Stock BSS Soul's Repose only added +50/+100/+150% Faith, so the newly strengthened ordinary Faith specialist would have dominated it through ordinary Soul Gratitude states.

Accepted correction:

- BSS Soul's Repose uses +250/+350/+450% Faith;
- q25/40/70;
- preserve the Soul-Gratitude-dependent base formula;
- preserve Chapter +5 Faith +2 Sin Shards recipe;
- remove prayer-owned fixed Faith/money and off-theme donation percentage.

This creates a stable state-dependent comparison independent of tier and Eloquence:

- GP < CQ -> ordinary Faith wins;
- GP = CQ -> equal Faith output before integer rounding;
- GP > CQ -> Soul's Repose wins.

The 2 Sin Shards therefore buy access to a Faith specialist that becomes preferable only when the player's Soul Gratitude state is actually strong enough to exploit it.

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

For Faith/Donations/BSS Soul's Repose, off-theme success bonuses blur the exact comparison the prayer exists to make.

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

## Product packaging direction

The user-facing product should be split into two clear Nexus offerings rather than one mod with a profile toggle:

1. **PrayerClarity** — Clarity-only, mechanically vanilla. Its strongest promise is that it explains prayer behavior without changing it.
2. **PrayerClarity Rebalanced** — the same Clarity experience plus the complete audited Rebalanced ruleset.

These should be **two products, one shared source/design system**, not two copy-pasted forks. The player installs one or the other.

This split is preferable because:

- the Clarity-only promise stays unambiguous;
- updates cannot silently alter game balance for a Clarity user;
- Nexus descriptions, changelogs and support reports each have one behavioral contract;
- Rebalanced can evolve its mechanics without forcing configuration complexity onto Clarity-only users;
- shared source/model infrastructure still prevents UI/localization logic from drifting.

The exact DLL names, BepInEx GUIDs and mutual-exclusion mechanism remain implementation details to verify later. Clarity-only should remain mechanically inert by construction rather than merely shipping the Rebalanced code behind a disabled setting.

## Implementation-risk findings from the audit

### Combat legacy alias must not stack

`b_shield` remains save-safe as a legacy alias of the merged Combat Prayer. Because Combat duration can cross future sermon weeks, production must not allow old `b_sword` and `b_shield` items to create two simultaneously stacking Combat packages. Both identities must resolve to one effective buff lifecycle/refresh rule.

### Semantic model must remain shared

Whatever the final build layout, Clarity-only and Rebalanced should reuse the same presentation/semantic source where possible. Rebalanced mechanics and all accepted Clarity surfaces must report the same effective values; Clarity-only must continue reporting stock values.

### VFX are not a gate

Two runtime visual auditions failed to expose a cheap clean native effect. Decorative aura/weapon polish is explicitly outside the first Rebalanced production scope.

## Roster-lock status

The **balance roster is now closed at the design level**. No material prayer-choice question remains open.

Runtime-sensitive mechanics are still candidates until implementation and user testing, but the intended behavior/numbers no longer require another design round unless new evidence contradicts them.

## Next engineering step — deliberately deferred

Do not create the Rebalanced development line yet while Clarity is still changing in parallel.

After the current Clarity work reaches its next accepted stable baseline:

1. create a new `dev/*` branch from that exact accepted Clarity source state;
2. carry forward this audited roster specification, not old research runtime source;
3. close only the remaining implementation seams (Roots expression lifecycle, Repose RNG, Repentance daily roll, Combat tier/regen/non-stacking alias, Protection recipe retirement, packaging identity);
4. keep one shared semantic/presentation model while producing separate Clarity-only and Rebalanced user-facing artifacts;
5. build one coherent Rebalanced candidate and request only the in-game tests that prove changed runtime behavior.

No hosted CI is required for this audit/documentation lock.