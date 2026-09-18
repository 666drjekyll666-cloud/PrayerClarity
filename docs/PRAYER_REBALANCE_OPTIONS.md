# PrayerClarity: Rebalanced — roster specification

Status: **canonical accepted Rebalanced ruleset**. The roster below is implemented and runtime-accepted in PrayerClarity: Rebalanced 0.1.5. Stock Graveyard Keeper 1.407 remains canonical in `PRAYER_MECHANICS.md`; values here are deliberate Balance/Rework design unless explicitly identified as a Vanilla Fix and must not be described as recovered vanilla intent.

Current shared accepted runtime/source: `3b7cea7986138f57d7ace6998b9cc6bca952af1e`. Frozen refs are `accepted/vanilla-1.0.24` and `accepted/rebalanced-0.1.5`; public releases are `v1.0.24` and `rebalanced-v0.1.5`. Exact binary hashes and runtime gates are recorded in `TEST_BUILD_LOG.md`.

`PRAYER_DESIGN_AUDIT.md` records the role/cross-roster rationale. `PRAYER_POWER_BUDGET.md` remains the quantitative stock cost/progression input; any older candidate numbers there are historical analysis, not the current roster.

## Product and evidence rules

- Public editions are **PrayerClarity: Vanilla** and **PrayerClarity: Rebalanced**. They are sibling alternatives, not a base mod plus an add-on.
- PrayerClarity: Vanilla remains mechanically stock. Do not silently put Vanilla Fixes or rebalancing into that edition.
- PrayerClarity: Rebalanced contains the complete Clarity experience plus the coherent ruleset below.
- Keep **Clarity**, **Vanilla Fixes**, and **Balance/Rework** distinct in code and documentation even when a Rebalanced prayer uses both a proven repair and intentional tuning.
- No per-prayer balance sliders are planned.
- Preserve the pulpit reward-reveal boundary: explain dependencies and prayer-owned effects, but do not expose the exact final current Faith/donation payout before the sermon.
- Preserve full **base** donations on failed sermons. Prayer-specific success bonuses and special outputs may still disappear according to verified mechanics.
- Base sermon Faith/donations remain present according to the prayer event. Specialist cleanup changes prayer-owned success contributions, not common base sermon rewards.
- Every prayer must justify unlock, crafting/quality cost, success gate and weekly sermon opportunity. Bronze must already be credible; Silver/Gold must visibly buy magnitude, reliability, useful duration, output or certainty.
- Do not normalize quality requirements globally for symmetry.
- Gameplay and all four Clarity surfaces must consume one effective semantic model.

## Locked roster

| Family | PrayerClarity: Rebalanced target |
| --- | --- |
| Ordinary `b_empty` | **Stock.** Starter baseline. |
| Faith `b_faith` | **+200 / +300 / +400% Faith**, q **25 / 40 / 70**. Keep common base donations. Remove prayer-owned fixed Faith, fixed money and off-theme money percentage. |
| Donations `b_money` | **+200 / +300 / +400% donations**, q **25 / 40 / 70**. Keep common base Faith. Remove prayer-owned Faith bonus; retain thematic **+1 / +2 / +3 silver** fixed floor. |
| Combo `b_faith_money` | **Stock:** +50/+100/+150% Faith and donations, q **15/30/60**, stock fixed Faith/money outputs. |
| Repentance `b_sins` | Daily confession probability **50 / 75 / 100%**, duration **18/36/54 min**, q **10/20/40**. |
| Shoots & Roots `b_plant` | Repair stock scope and scale growth time **-20 / -30 / -40%**, duration **36/72/108 min**, q **10/20/30**. The scope repair is Vanilla Fix; quality scaling is Balance/Rework. |
| Repose `b_skull` | Bronze stock-style expanded-pool roll; Silver halfway from current stock best-tier probability to certainty; Gold guaranteed best prayer-eligible tier. Ceiling remains normal progression max +1. Duration **18/36/54 min**, q **20/40/50**. |
| Combat (`b_sword`; `b_shield` legacy alias) | Merge accepted. Damage **+5/+10/+15**, armor **+4**, regeneration **1/2/4 HP/sec**, duration **36/72/108 min**, q **10/20/40**. |
| Imagination `b_pen` | **+0.7 craft quality** at every tier; duration **18/36/54 min**; Silver successful sermon gives **3 Silver Stories**, Gold gives **3 Gold Stories**; q **10/40/60**. |
| Excellence `b_star` | **+0.2/+0.5/+1.0** linked-craft quality, duration **18/36/54 min**, q **10/40/60**. |
| Prosperity `b_village` | **Stock:** 1/2/3 Commercial Blessings and existing sermon outputs, q **10/20/30**. Natural progression obsolescence is accepted. |
| BSS Soul's Repose `b_souls` | **+200 / +300 / +400% Faith**, q **25/40/70**. Preserve Soul-Gratitude base formula and recipe; remove prayer-owned fixed Faith/money and off-theme donation percentage. |
| Soul Contentment `b_grat_points_incr` | **+20% Soul Gratitude** at every tier; duration **36/72/108 min**, q **10/20/30**. Duration is the premium-tier axis. |
| Thorough Cleansing `b_sin_shard` | **x2 Sin Shards** at every tier; duration **36/72/108 min**, q **10/20/30**. Duration is the premium-tier axis. |

## Faith / Donations / Combo family

### Common-base rule

Specialization does not remove the ordinary sermon base. Base Faith still comes from the event/Church Quality path and base donations still come from Graveyard Quality. On failure, those common base rewards remain while success-only prayer bonuses disappear.

### Combo

Keep stock:

- q **15 / 30 / 60**;
- `k_faith = .5 / 1 / 1.5`;
- `k_money = .5 / 1 / 1.5`;
- stock fixed Faith/money outputs;
- Hard Book +7 Faith production gate.

Combo remains the convenient generalist: more expensive to manufacture, broader in output and easier to guarantee than the specialists.

### Faith specialist

- q **25 / 40 / 70**;
- `k_faith = 2 / 3 / 4` = **+200/+300/+400%**;
- no prayer-owned `k_money`;
- remove prayer-owned fixed Faith and fixed money;
- retain normal base donations.

The percentage is the specialist proposition and scales with church development. Including the unchanged base reward, the proportional Faith total is **3x / 4x / 5x base** on success versus Combo's **1.5x / 2x / 2.5x** at the same quality: exactly twice Combo's proportional Faith total before fixed outputs.

### Donations specialist

- q **25 / 40 / 70**;
- `k_money = 2 / 3 / 4` = **+200/+300/+400%**;
- no prayer-owned Faith percentage/fixed Faith;
- retain normal base Faith;
- retain **+1/+2/+3 silver** fixed money.

The fixed money is an early-game floor, not a symmetry requirement. Before that fixed floor, the successful proportional donation total is likewise **3x / 4x / 5x base**, exactly twice Combo's corresponding **1.5x / 2x / 2.5x** proportional total.

### Requirement grammar

- Combo q: **15 -> 30 -> 60**;
- Specialist q: **25 -> 40 -> 70**.

The specialist row is Combo +10 Church Quality at every tier and keeps the same internal steps (+15, then +30). Over two successful weeks, rotating Faith specialist + Donations specialist is intentionally stronger on planned target resources than simply repeating Combo; Combo pays for convenience and breadth.

## BSS Soul's Repose — accepted specialist alignment

Treat BSS Soul's Repose as the **Soul-Gratitude-dependent Faith specialist**:

- **+200/+300/+400% Faith**;
- q **25/40/70**;
- preserve base Faith `0.1 * (Church Quality + Soul Gratitude) * Eloquence factor`;
- preserve Chapter +5 Faith +2 Sin Shards recipe;
- remove prayer-owned fixed Faith, fixed money and off-theme donation percentage.

Soul Gratitude is part of the prayer event's **base Faith before the success-only percentage is applied**. There is no separate `+X% from Soul Gratitude` success term. The player-facing grammar should therefore explain that base Faith depends on Church Quality and Soul Gratitude, then show the tier's **+200/+300/+400% Faith** modifier.

Ordinary Faith base is `0.2 * CQ`. Because the two specialists use the same multiplier and q ladder, ignoring integer rounding:

- `GP < CQ` -> ordinary Faith wins;
- `GP = CQ` -> equal Faith output;
- `GP > CQ` -> Soul's Repose wins.

The Sin Shard cost therefore buys access to a state-dependent alternative rather than an automatically superior DLC prayer.

## Repentance

Accepted daily-roll rework:

- Bronze **50%**;
- Silver **75%**;
- Gold **100%**;
- duration **18/36/54 min**.

The stock scheduler rolls once per in-game day for each existing confessional. Gold intentionally guarantees those daily rolls while active. This remains interaction-heavy throughput rather than passive Faith generation.

## Shoots & Roots

- Bronze **-20% growth time**;
- Silver **-30%**;
- Gold **-40%**;
- duration **36/72/108 min**.

The stock `-20%` term is recoverable, but stock writes the prayer state to the player while growth expressions read the growing/workbench WGO. Rebalanced must repair that verified scope mismatch while preserving the game's additive growth expression; do not replace plant growth with an external timer system.

## Repose

Premium quality buys reliability rather than progression skipping:

- Bronze: stock-style selection from the expanded max+1 pool;
- Silver: `P(best) = 0.5 + 0.5 * P_stock(best)`;
- Gold: 100% best prayer-eligible tier.

Never exceed the story/progression ceiling that stock `body_max+1` can open.

## Combat

| Quality | Damage | Armor | Regeneration | Duration | q |
| --- | ---: | ---: | ---: | ---: | ---: |
| Bronze | +5 | +4 | 1 HP/sec | 36 min | 10 |
| Silver | +10 | +4 | 2 HP/sec | 72 min | 20 |
| Gold | +15 | +4 | 4 HP/sec | 108 min | 40 |

The strong package is intentional for a deep-unlock, Hard Book, combat-only weekly choice. Do not raise q merely to make the table look symmetric with other prayers.

`b_sword` is canonical. Existing `b_shield` items remain save-safe same-quality aliases. Both IDs must resolve to one effective Combat buff lifecycle/refresh behavior; they must not stack. Retire/hide the redundant Protection recipe only after the exact crafting/unlock lifecycle seam is verified.

Decorative VFX are outside the first Rebalanced scope after two native-FX auditions failed to produce a cheap clean result.

## Imagination

Accepted:

- `craft_q = +0.7` at all tiers;
- duration **18/36/54 min**;
- Bronze: no extra Story reward;
- Silver: **3 Silver Stories** on successful sermon;
- Gold: **3 Gold Stories** on successful sermon;
- q **10/40/60**.

Do not nerf the healthy Bronze core or inflate `craft_q` merely to manufacture a numeric tier ladder. Premium tiers improve useful duration and return thematic premium writing material without bypassing Notes -> Chapter -> cover/Hard Book production.

## Excellence

Accepted first runtime target:

- `craft_q = +0.2 / +0.5 / +1.0`;
- duration **18/36/54 min**;
- q **10/40/60**.

Finite craft-quality tiers are the natural cap. Gold may intentionally make an otherwise reachable premium result deterministic.

## BSS duration specialists

### Soul Contentment

- **+20% Soul Gratitude gain** at all qualities;
- duration **36/72/108 min**.

Do not also scale magnitude by quality. Duration is the premium axis and avoids magnitude×duration double scaling.

### Thorough Cleansing

- **x2 Sin Shards** at all qualities;
- duration **36/72/108 min**.

The x2 effect is already a strong scarce-resource specialist. Silver/Gold buy a longer processing window; do not add x3/x4 without new evidence.

## No global cleanup of secondary sermon bonuses

The pure-resource specialists are cleaned because off-theme success bonuses directly blur their comparison. Do **not** generalize that cleanup to Roots, Repentance, Repose, Combat, Imagination, Excellence, Contentment or Cleansing merely for aesthetic symmetry. Their small stock sermon-resource contributions remain an opportunity-cost floor unless a specific model demonstrates a problem.

## Edition architecture

The intended user-facing structure is two separate Nexus offerings backed by one shared source/design system:

1. **PrayerClarity: Vanilla 1.0.24** — accepted Clarity UX over stock prayer mechanics, canonical DLL `PrayerClarity.dll`, plugin GUID `nikich.graveyardkeeper.prayerclarity`.
2. **PrayerClarity: Rebalanced 0.1.5** — the same Clarity UX plus the complete accepted ruleset above, canonical DLL `PrayerClarity.Rebalanced.dll`, plugin GUID `nikich.graveyardkeeper.prayerclarity.rebalanced`.

A user installs one edition or the other. Both are built from shared source/presentation infrastructure rather than copy-pasted forks. Vanilla declares Rebalanced as incompatible so the sibling alternatives are not intended to run together.

## Accepted implementation state

The implementation-target/effective-model audit was completed and the integrated ruleset was accepted in Rebalanced 0.1.5.

The stable implementation uses one shared rules/semantic source across gameplay and the four Clarity surfaces; repairs Roots at the verified expression scope; applies Repentance probability at the native daily-roll logic; narrows Repose corpse selection at the verified generation seam; captures tier-dependent Combat state with one effective `buff_sword` lifecycle; preserves `b_shield` as a legacy alias while retiring its future crafting path; and ships Vanilla/Rebalanced as separate plugin identities and DLLs.

There is no open implementation gate attached to this roster. Any future value or algorithm change is a new Balance/Rework proposal and must go through the normal evidence, candidate, runtime-test and acceptance process.
