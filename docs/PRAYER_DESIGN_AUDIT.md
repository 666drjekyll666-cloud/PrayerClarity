# Prayer Design Audit — Graveyard Keeper 1.407

Status: **final cross-roster design audit reconciled with stable PrayerClarity: Vanilla 1.0.24 / Rebalanced 0.1.5**. Stock mechanics remain canonical in `PRAYER_MECHANICS.md`; the accepted Rebalanced gameplay ruleset is in `PRAYER_REBALANCE_OPTIONS.md`.

The current Rebalanced roster is an accepted runtime result. Historical alternatives in this document are analysis only when they differ from `PRAYER_REBALANCE_OPTIONS.md`.

## Baseline reconciliation

The current stable sibling releases share accepted runtime/source `3b7cea7986138f57d7ace6998b9cc6bca952af1e`: Vanilla 1.0.24 (`accepted/vanilla-1.0.24`, `v1.0.24`) and Rebalanced 0.1.5 (`accepted/rebalanced-0.1.5`, `rebalanced-v0.1.5`). The late 2026-09-16 design audit remains the rationale for the accepted roster, but its earlier candidate numbers are superseded where the final accepted ruleset differs.

Accepted public family naming is now:

1. **PrayerClarity: Vanilla** — complete Clarity UX, stock prayer mechanics/balance.
2. **PrayerClarity: Rebalanced** — sibling edition containing the same Clarity UX plus the intentional Rebalanced ruleset.

These are peer editions, not base+addon. `Vanilla Fixes` remains an internal evidence/design category and must not be silently shipped inside PrayerClarity: Vanilla.

## Audit rules

The final roster was checked against these principles:

- specialization must materially reward specialization rather than merely match Combo;
- Bronze must already be credible;
- Silver/Gold must justify premium writing inputs;
- a narrow weekly prayer may be very strong in its niche;
- duration is valid premium power only when the player can exploit the extra window;
- natural progression obsolescence is acceptable;
- healthy stock behavior should not be nerfed for visual or numerical symmetry;
- q requirements follow the whole cost proposition, not a universal ladder;
- failed sermons retain the full vanilla base donation pool;
- base Faith/donations are distinct from prayer-owned success bonuses;
- every Rebalanced UI surface must describe the same effective mechanics that gameplay executes.

## Final audit matrix

| Prayer/family | Audit verdict | Reason |
| --- | --- | --- |
| Ordinary | **No change** | Starter baseline and intentional replacement target. |
| Faith | **Accepted Rebalanced rework** | True Faith specialist: +200/+300/+400%, q25/40/70. |
| Donations | **Accepted Rebalanced rework** | True money specialist: +200/+300/+400%, q25/40/70; +1/+2/+3 silver protects the early role. |
| Combo | **No balance change** | Generalist remains broader, more expensive to craft and easier to guarantee. |
| Repentance | **Accepted Rebalanced rework** | 50/75/100% daily confession probability creates a meaningful reliability/throughput ladder. |
| Shoots & Roots | **Accepted fix + Rebalanced scaling** | Proven scope mismatch repaired; -20/-30/-40% creates quality progression in a narrow farming niche. |
| Repose | **Accepted Rebalanced rework** | Premium quality buys corpse-tier reliability without skipping story tiers. |
| Combat | **Accepted structural/numeric rework** | Offense, defense and regeneration form one coherent narrow weekly combat-preparation package. |
| Imagination | **Accepted Rebalanced rework** | Preserve healthy +0.7 core; premium value comes from duration plus 3 Silver/3 Gold Stories. |
| Excellence | **Accepted Rebalanced rework** | +0.2/+0.5/+1.0 makes premium qualities matter in a narrower craft scope. |
| Prosperity | **No change** | 1/2/3 Blessings already provides strong quality progression and healthy eventual obsolescence. |
| BSS Soul's Repose | **Accepted Rebalanced rework** | Align with Faith-specialist grammar while preserving a Soul-Gratitude-dependent niche. |
| Soul Contentment | **Accepted Rebalanced rework** | +20% all tiers; duration is the premium axis. |
| Thorough Cleansing | **No magnitude increase** | x2 is already strong; duration is sufficient premium scaling. |

## Cross-family finding — Faith / Donations / Combo

Combo keeps stock q **15/30/60**, +50/+100/+150% Faith and donations, stock fixed outputs and the Hard Book +7 Faith production gate.

Faith and Donations use q **25/40/70** and +200/+300/+400% in the chosen resource. The specialists are Chapter +5 Faith prayers but require exactly +10 more Church Quality than Combo at every tier. This creates a clean trade:

- Combo: broader, costlier item, easier success gate;
- specialist: narrower, cheaper item, harder gate, substantially stronger target output.

The two-week sanity check remains intentional: rotating Faith then Donations should outperform repeating Combo on the separately planned target resources. Otherwise there is no strategic reward for maintaining and planning around two specialist prayers.

Specialization only cleans the **prayer-owned success contribution**. Faith still receives ordinary base donations; Donations still receives ordinary base Faith.

## Cross-family finding — BSS Soul's Repose

Verified stock bases are:

- ordinary Faith-family base: `0.2 * CQ * EloquenceFactor`;
- BSS Soul's Repose base: `0.1 * (CQ + GP) * EloquenceFactor`.

If Soul's Repose stayed stock while ordinary Faith moved to +200/+300/+400%, ordinary Faith would dominate the DLC prayer through ordinary Soul Gratitude states. The accepted correction gives Soul's Repose the same +200/+300/+400% ladder and q25/40/70 while preserving its Soul-Gratitude base and 2 Sin Shard cost.

This produces a stable state-dependent choice, ignoring integer rounding:

- `GP < CQ` -> ordinary Faith wins;
- `GP = CQ` -> tie;
- `GP > CQ` -> Soul's Repose wins.

## No global q normalization

The specialist q grammar is not a template for every prayer.

- Combat already pays a deep smithing route, Hard Book +7 Faith and a narrow combat-only weekly use; increasing q would double-tax it.
- Roots and Repentance are Chapter prayers whose narrow workflows matter earlier.
- Repose has a Hard Book gate plus a finite corpse-progression window.
- Imagination/Excellence already use Hard Books and q up to 60.
- Contentment/Cleansing arrive through Better Save Soul and use Soul resources/state-dependent workflows.

Uniform q would be aesthetic symmetry rather than better balance.

## No global cleanup of secondary sermon rewards

The clean resource-specialist rule does not imply removing every small Faith/money contribution from every special prayer.

For Faith, Donations and BSS Soul's Repose, off-theme prayer-owned success bonuses blur a direct resource-specialist comparison, so they are deliberately cleaned.

For Roots, Repentance, Repose, Combat, Imagination, Excellence, Contentment and Cleansing, small stock sermon-resource contributions remain an opportunity-cost floor beside the special effect. There is no evidence they compress choices enough to justify a broad nerf.

## Role-collision check

No harmful collision remains after the locked changes:

- **Repentance vs Faith/Imagination:** Repentance requires daily church interaction and produces mixed Stories; it does not replace immediate Faith specialization or Imagination's planned writing window.
- **Roots vs farming substitutes:** strong magnitude+duration remains bounded by the farming niche and competes with fertilizer, zombies and waiting.
- **Repose:** Gold certainty is strong but bounded by the finite corpse progression ceiling.
- **Combat:** exceptional raw power is acceptable because it spends a weekly slot on a narrow activity after a deep unlock.
- **Imagination vs Excellence:** verified craft scopes differ enough to remain complementary; Imagination keeps its healthy +0.7 core while Excellence needs magnitude progression.
- **Prosperity:** stock 1/2/3 permanent Blessings remains coherent and may naturally become obsolete.
- **Contentment / Thorough Cleansing:** constant magnitude plus longer duration avoids unjustified magnitude×duration double scaling.

## Implementation seam findings — resolved in Rebalanced 0.1.5

The implementation audit identified several seams that had to be proven before the roster could become runtime behavior. They are now resolved in the accepted Rebalanced implementation:

- **shared effective semantics:** gameplay and Pulpit/Technology/item-tooltip/Temporary-Effects presentation consume the same Rebalanced rule source rather than independent UI-only values;
- **Combat legacy alias:** saved `b_shield` prayer crafts remain readable and are projected onto the canonical `buff_sword` lifecycle; the duplicate future Protection crafting/unlock path is retired rather than deleting legacy IDs;
- **Roots:** the stock parameter-owner mismatch is repaired by replacing the verified stock prayer term at the affected growth-expression scope, preserving the native additive expression model;
- **Repose:** tier-dependent reliability is applied at the verified corpse-generation lifecycle while respecting the stock progression ceiling;
- **Repentance:** the effective confession probability is injected into the stock daily-roll expression instead of relying on a one-time value that stock resets;
- **edition packaging:** Vanilla and Rebalanced use separate canonical DLLs and plugin GUIDs, with Vanilla declaring the Rebalanced GUID incompatible;
- **VFX:** no acceptable cheap native visual effect was found, so VFX remain outside accepted scope.

## Accepted roster result

The balance roster is closed for the current stable Rebalanced 0.1.5 release. The canonical values are those in `PRAYER_REBALANCE_OPTIONS.md`, including the final specialist ladder of **+200/+300/+400%** rather than earlier candidate ladders.

There is currently no unresolved design/implementation gate in this audit. Reopen a prayer only when new runtime evidence, compatibility evidence, or player UX feedback establishes a concrete problem.
