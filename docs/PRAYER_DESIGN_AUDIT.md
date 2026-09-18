# Prayer Design Audit — Graveyard Keeper 1.407

Status: **final cross-roster design audit, rebased to PrayerClarity: Vanilla 1.0.20 on 2026-09-17**. Stock mechanics remain canonical in `PRAYER_MECHANICS.md`; the locked gameplay target is in `PRAYER_REBALANCE_OPTIONS.md`.

No Balance/Rework mechanic is an accepted runtime result until the relevant implementation and user/runtime evidence exists.

## Baseline reconciliation

The accepted presentation/runtime baseline is **PrayerClarity: Vanilla 1.0.20**, exact accepted source `c7ac91c1cea6c498fb406323725768b605d8139f`, accepted ref `accepted/clarity-1.0.20`, public release `v1.0.20`.

The current research line starts from the later `main` documentation/naming state. The commits after the accepted runtime source changed documentation/repository hygiene only; production Clarity source did not change. Therefore the late 2026-09-16 balance audit can be carried forward without reopening mechanics or power-budget conclusions.

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
| Faith | **Locked rework** | True Faith specialist: +250/+350/+450%, q25/40/70. |
| Donations | **Locked rework** | True money specialist: +250/+350/+450%, q25/40/70; +1/+2/+3 silver protects the early role. |
| Combo | **No balance change** | Generalist remains broader, more expensive to craft and easier to guarantee. |
| Repentance | **Locked rework** | 50/75/100% daily confession probability creates a meaningful reliability/throughput ladder. |
| Shoots & Roots | **Locked fix + rework** | Proven scope mismatch repaired; -20/-30/-40% creates quality progression in a narrow farming niche. |
| Repose | **Locked rework** | Premium quality buys corpse-tier reliability without skipping story tiers. |
| Combat | **Locked structural/numeric rework** | Offense, defense and regeneration form one coherent narrow weekly combat-preparation package. |
| Imagination | **Locked rework** | Preserve healthy +0.7 core; premium value comes from duration plus 3 Silver/3 Gold Stories. |
| Excellence | **Locked first runtime target** | +0.2/+0.5/+1.0 makes premium qualities matter in a narrower craft scope. |
| Prosperity | **No change** | 1/2/3 Blessings already provides strong quality progression and healthy eventual obsolescence. |
| BSS Soul's Repose | **Locked rework** | Align with Faith-specialist grammar while preserving a Soul-Gratitude-dependent niche. |
| Soul Contentment | **Locked rework** | +20% all tiers; duration is the premium axis. |
| Thorough Cleansing | **No magnitude increase** | x2 is already strong; duration is sufficient premium scaling. |

## Cross-family finding — Faith / Donations / Combo

Combo keeps stock q **15/30/60**, +50/+100/+150% Faith and donations, stock fixed outputs and the Hard Book +7 Faith production gate.

Faith and Donations use q **25/40/70** and +250/+350/+450% in the chosen resource. The specialists are Chapter +5 Faith prayers but require exactly +10 more Church Quality than Combo at every tier. This creates a clean trade:

- Combo: broader, costlier item, easier success gate;
- specialist: narrower, cheaper item, harder gate, substantially stronger target output.

The two-week sanity check remains intentional: rotating Faith then Donations should outperform repeating Combo on the separately planned target resources. Otherwise there is no strategic reward for maintaining and planning around two specialist prayers.

Specialization only cleans the **prayer-owned success contribution**. Faith still receives ordinary base donations; Donations still receives ordinary base Faith.

## Cross-family finding — BSS Soul's Repose

Verified stock bases are:

- ordinary Faith-family base: `0.2 * CQ * EloquenceFactor`;
- BSS Soul's Repose base: `0.1 * (CQ + GP) * EloquenceFactor`.

If Soul's Repose stayed stock while ordinary Faith moved to +250/+350/+450%, ordinary Faith would dominate the DLC prayer through ordinary Soul Gratitude states. The accepted correction gives Soul's Repose the same +250/+350/+450% ladder and q25/40/70 while preserving its Soul-Gratitude base and 2 Sin Shard cost.

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

## Implementation-risk findings

These do not reopen the roster but must be proven before runtime acceptance.

### Shared effective semantic model

PrayerClarity: Vanilla 1.0.20 already derives Pulpit, Technology, prayer-item and Temporary Effects presentation from shared semantic data. Rebalanced cannot patch gameplay independently and leave those readers on stock `GameBalance` values.

The next engineering task is therefore to define one effective prayer-definition seam that supplies both gameplay and presentation. The Vanilla edition must resolve stock semantics; the Rebalanced edition must resolve the locked effective semantics. No second manually maintained UI-only table of prayer values.

### Combat legacy alias

`b_shield` remains save-safe as a same-quality alias of canonical `b_sword`. Because Combat duration can span later sermon weeks, the two IDs must resolve to one effective buff/refresh lifecycle and must never stack two Combat packages.

### Roots lifecycle

The verified stock bug is a parameter-owner mismatch. Implementation must preserve the native additive growth expression and attach the effective prayer term at the verified evaluation scope instead of inventing an external growth timer.

### Repose RNG

Reliability scaling must hook the verified corpse-selection lifecycle and preserve all stock progression ceilings. Silver must remain mathematically halfway from the current stock best-tier probability to certainty.

### Repentance scheduler

The stock logic resets confession probability to 15% before the daily roll. Rebalanced must inject the effective tier probability at the daily roll seam rather than writing a one-time player parameter that stock overwrites.

### Protection retirement

Do not delete or rewrite saved `b_shield` IDs. Hide/retire only the redundant future crafting/unlock path after its exact lifecycle is verified.

### Edition packaging

PrayerClarity: Vanilla and PrayerClarity: Rebalanced are separate player-facing editions, but should share source/presentation infrastructure rather than drift into forks. Exact plugin GUID, DLL filename and mutual-exclusion mechanism remain implementation evidence questions.

### VFX

Two runtime native-FX auditions failed to expose a cheap clean aura/weapon effect. VFX are outside the first Rebalanced production scope and are not an implementation gate.

## Roster-lock result

The **balance roster remains closed at the design level after reconciliation with PrayerClarity: Vanilla 1.0.20 and the new edition naming**. The naming change introduces no mechanical contradiction and does not reopen any prayer-choice decision.

The old 1.0.1 waiting condition is obsolete: the required stable Clarity baseline now exists. The next work is implementation-target discovery, not another balance round.

## Next engineering step

On the 1.0.20-based research line:

1. audit how to insert one effective prayer-definition/model layer between stock data and both mechanics/presentation;
2. close the narrow Roots, Repose, Repentance, Combat/alias, Protection-crafting and edition-packaging seams;
3. only after those assumptions are evidenced, create a build-bearing `dev/*` branch and implement one coherent candidate;
4. ask the user only for runtime tests that prove changed gameplay behavior and presentation consistency.

No hosted CI is required for this documentation/rebase audit.
