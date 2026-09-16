# Prayer Rebalance Options — audited roster

Status: design specification, roster-locked 2026-09-16. Stock Graveyard Keeper 1.407 remains canonical in `PRAYER_MECHANICS.md`. Values below are deliberate Rebalanced design, not recovered vanilla behavior. Runtime-sensitive behavior is not an accepted result until implemented and tested where required.

`PRAYER_DESIGN_AUDIT.md` records the role/cross-roster rationale. `REWORK_RESEARCH.md` preserves the research history that led here.

## Product rules

- One coherent opinionated **Rebalanced** ruleset; no per-prayer balance sliders.
- Preserve the accepted Clarity information model and exact-reward reveal boundary.
- Rebalanced includes the proven Shoots & Roots scope repair.
- Preserve full **base** donations on failed sermons.
- Base sermon Faith/donations remain present for every prayer according to its event; specialization changes prayer-owned success bonuses, not the common base sermon rewards.
- Every prayer must justify technology/crafting/quality cost plus the weekly sermon opportunity.
- Bronze must already be credible. Silver/Gold must visibly justify premium writing cost through magnitude, reliability, useful duration, output, certainty or a thematic reward.
- A narrow weekly prayer may be deliberately very strong in its niche.
- Do not normalize q requirements globally for symmetry. Compare the prayer's full proposition: unlock depth, Chapter vs Hard Book, consumables, state requirements, role breadth and weekly opportunity cost.
- Mechanics and all Clarity surfaces must use one effective semantic model.

## Audited roster

| Family | Rebalanced target |
| --- | --- |
| Ordinary `b_empty` | **Stock.** Starter baseline. |
| Faith `b_faith` | **+250 / +350 / +450% Faith**, q **25 / 40 / 70**. Keep common base donations. Remove prayer-owned fixed Faith, fixed money and off-theme money percentage. |
| Donations `b_money` | **+250 / +350 / +450% donations**, q **25 / 40 / 70**. Keep common base Faith. Remove prayer-owned Faith bonus; retain thematic **+1 / +2 / +3 silver** floor. |
| Combo `b_faith_money` | **Stock:** +50/+100/+150% Faith and donations, q **15/30/60**, stock fixed Faith/money outputs. |
| Repentance `b_sins` | Daily confession probability **50 / 75 / 100%**, duration **18/36/54 min**, q **10/20/40**. |
| Shoots & Roots `b_plant` | Repair stock scope; growth time **-20 / -30 / -40%**, duration **36/72/108 min**, q **10/20/30**. |
| Repose `b_skull` | Bronze stock-style expanded-pool roll; Silver halfway from current stock best-tier probability to certainty; Gold guaranteed best prayer-eligible tier. Ceiling remains normal progression max +1. Duration **18/36/54 min**, q **20/40/50**. |
| Combat (`b_sword`; `b_shield` legacy alias) | Merge accepted. Damage **+5/+10/+15**, armor **+4**, regeneration **1/2/4 HP/sec**, duration **36/72/108 min**, q **10/20/40**. |
| Imagination `b_pen` | **+0.7 craft quality** at every tier; duration **18/36/54 min**; Silver successful sermon gives **3 Silver Stories**, Gold gives **3 Gold Stories**; q **10/40/60**. |
| Excellence `b_star` | **+0.2/+0.5/+1.0** linked-craft quality, duration **18/36/54 min**, q **10/40/60**. |
| Prosperity `b_village` | **Stock:** 1/2/3 Commercial Blessings and existing sermon outputs, q **10/20/30**. Natural progression obsolescence is accepted. |
| BSS Soul's Repose `b_souls` | **+250 / +350 / +450% Faith**, q **25/40/70**. Preserve Soul-Gratitude base formula and recipe; remove prayer-owned fixed/off-theme Faith/money extras. |
| Soul Contentment `b_grat_points_incr` | **+20% Soul Gratitude** at every tier; duration **36/72/108 min**, q **10/20/30**. Duration is the premium-tier axis. |
| Thorough Cleansing `b_sin_shard` | **x2 Sin Shards** at every tier; duration **36/72/108 min**, q **10/20/30**. Duration is the premium-tier axis; do not add x3/x4 without new evidence. |

## Faith / Donations / Combo family — accepted structure

### Common-base rule

Specialization does **not** remove the ordinary sermon base:

- base Faith still comes from the prayer event/Church Quality path;
- base donations still come from Graveyard Quality;
- on failure, the common base rewards remain while success-only prayer bonuses disappear.

The cleanup concerns only the prayer-owned success contribution.

### Combo

Keep stock:

- q **15 / 30 / 60**;
- `k_faith = .5 / 1 / 1.5`;
- `k_money = .5 / 1 / 1.5`;
- stock fixed Faith/money outputs;
- Hard Book + 7 Faith production gate.

Combo remains the convenient generalist: more expensive to manufacture, but easier to guarantee than the stronger specialists and useful when both resources matter.

### Faith specialist

Accepted target:

- q **25 / 40 / 70**;
- `k_faith = 2.5 / 3.5 / 4.5` = **+250/+350/+450%**;
- no prayer-owned `k_money`;
- remove prayer-owned fixed Faith and fixed money;
- retain the normal event's base donations.

The absence of flat Faith is intentional: the percentage is the whole specialist proposition and scales naturally with church development.

### Donations specialist

Accepted target:

- q **25 / 40 / 70**;
- `k_money = 2.5 / 3.5 / 4.5` = **+250/+350/+450%**;
- no prayer-owned Faith percentage/fixed Faith;
- retain the normal event's base Faith;
- retain **+1/+2/+3 silver** fixed money.

The fixed money is an early-game floor: deleting it would make low-Graveyard-Quality Donations weaker exactly in the progression window where church money matters most.

### Requirement grammar

- Combo q: **15 -> 30 -> 60**;
- Specialist q: **25 -> 40 -> 70**.

The specialist row is the Combo row shifted upward by exactly 10 Church Quality at every tier. Both ladders therefore have the same internal steps (+15, then +30).

Two-week sanity check is intentional: rotating Faith specialist + Donations specialist must outperform simply repeating Combo when the player plans around the two separate goals. Combo pays for convenience/breadth; specialists pay back planning and higher success requirements.

## BSS Soul's Repose — accepted specialist alignment

Treat BSS Soul's Repose as the **Soul-Gratitude-dependent Faith specialist**:

- use **+250/+350/+450% Faith**;
- use q **25/40/70**;
- preserve base Faith `0.1 * (Church Quality + Soul Gratitude) * Eloquence factor`;
- preserve Chapter +5 Faith +2 Sin Shards recipe;
- remove prayer-owned fixed Faith, fixed money and off-theme donation percentage.

This creates a stable state-dependent comparison with ordinary Faith. Ignoring integer rounding:

- ordinary Faith base: `0.2 * CQ`;
- Soul's Repose base: `0.1 * (CQ + GP)`.

Because both use the same specialist multiplier and q ladder:

- **GP < CQ:** ordinary Faith wins;
- **GP = CQ:** they tie on Faith output;
- **GP > CQ:** Soul's Repose wins.

The extra 2 Sin Shards are therefore paid to exploit sufficiently developed Soul Gratitude rather than to buy an automatically superior or inferior prayer.

## Repentance

Accepted daily-roll rework:

- Bronze **50%**;
- Silver **75%**;
- Gold **100%**;
- duration 18/36/54 min.

The stock roll occurs once per in-game day for each existing confessional. Gold intentionally guarantees each daily roll while active. With both confessionals, this produces a strong but interaction-heavy confession week rather than passive Faith generation.

## Shoots & Roots

- Bronze **-20% growth time**;
- Silver **-30%**;
- Gold **-40%**;
- duration 36/72/108 min.

Implementation must preserve the game's additive growth expression and repair/scale the prayer term at the verified scope. Do not replace plant growth with an external timer system.

## Repose

Premium quality buys reliability rather than progression skipping:

- Bronze: stock-style selection from the expanded max+1 pool;
- Silver: `P(best) = 0.5 + 0.5 * P_stock(best)`;
- Gold: 100% best prayer-eligible tier.

Never exceed the story/progression ceiling that stock `body_max+1` can open.

## Combat

Accepted package:

| Quality | Damage | Armor | Regeneration | Duration | q |
| --- | ---: | ---: | ---: | ---: | ---: |
| Bronze | +5 | +4 | 1 HP/sec | 36 min | 10 |
| Silver | +10 | +4 | 2 HP/sec | 72 min | 20 |
| Gold | +15 | +4 | 4 HP/sec | 108 min | 40 |

Do not raise q merely because the raw buff is large. Combat already pays a deep smithing unlock, Hard Book +7 Faith and an entire weekly slot for one narrow activity. The strong result is intentional.

`b_shield` is a save-safe legacy alias, not a second stackable Combat Prayer. Production implementation must ensure `b_sword` and legacy `b_shield` resolve to one effective buff lifecycle/refresh behavior so alternating old items cannot stack two Combat packages.

Visual VFX are optional and out of the first Rebalanced production scope after two unsuccessful native-FX auditions.

## Imagination

Accepted:

- +0.7 writing/prayer craft-quality input at all tiers;
- 18/36/54 min;
- Bronze: no extra Story reward;
- Silver: **3 Silver Stories** on successful sermon;
- Gold: **3 Gold Stories** on successful sermon.

Do not increase `craft_q` just to manufacture a numerical tier ladder. The stock +0.7 core is already healthy; premium tiers improve useful duration and refund a thematic premium writing component without bypassing Notes -> Chapter -> cover/Hard Book production.

## Excellence

Accepted first candidate:

- +0.2 / +0.5 / +1.0;
- 18/36/54 min;
- q10/40/60.

Finite craft-quality tiers provide a natural cap. Gold deliberately may make an otherwise reachable premium result deterministic.

## BSS duration specialists

### Soul Contentment

Accepted:

- **+20% Soul Gratitude gain** at all qualities;
- duration **36/72/108 min**.

Do not also scale magnitude by quality. Duration already provides the premium ladder and avoids multiplicative magnitude×duration escalation.

### Thorough Cleansing

Accepted:

- **x2 Sin Shards** at all qualities;
- duration **36/72/108 min**.

The x2 effect is already a successful scarce-resource specialist. Silver/Gold buy a longer processing window; x3/x4 would double-scale an already strong niche without evidence of need.

## No global cleanup of secondary sermon bonuses

The pure-resource specialists are deliberately cleaned because off-theme success bonuses directly blur their comparison.

Do **not** generalize that cleanup to every special-effect prayer merely for aesthetic symmetry. Small stock Faith/money contributions on Roots, Repentance, Repose, Combat, Imagination, Excellence, Contentment or Cleansing are part of the existing weekly opportunity-cost floor unless a specific prayer's final modeling shows a problem. Removing them globally would be a broad nerf without a demonstrated UX/balance need.

## Product split direction

The intended user-facing product structure is **two separate Nexus mods**, backed by one shared source/design system rather than two drifting forks:

1. **PrayerClarity** — Clarity-only for stock/vanilla prayer mechanics. It must not expose an optional balance mode that could silently change mechanics.
2. **PrayerClarity Rebalanced** — the same Clarity experience plus the complete audited Rebalanced ruleset above.

A user should install one product or the other, not both. The exact plugin GUID/file-name/mutual-exclusion mechanism is an implementation question to verify later rather than guess now.

Prefer source sharing/build composition over copy-pasted forks. The Clarity-only artifact should remain mechanically inert by construction; Rebalanced adds the gameplay layer deliberately.

## Implementation timing

Do **not** start Rebalanced production from the old research branch or freeze it to today's 1.0.1 while Clarity is actively being changed in parallel.

Wait until the current Clarity work reaches its next accepted stable baseline. Then:

- create the Rebalanced `dev/*` line from that exact accepted Clarity source state;
- carry forward this roster specification, not old research runtime source;
- preserve every accepted Clarity surface from the chosen baseline;
- build the Rebalanced product as a separate user-facing artifact/package while sharing source/model infrastructure where practical.

## Implementation-only evidence gates after the Clarity baseline freezes

- Roots SmartExpression lifecycle/scope seam;
- Repose corpse RNG seam;
- Combat tier capture, regeneration lifecycle and non-stacking legacy alias behavior;
- Repentance daily-roll tier seam;
- safe retirement/hiding of duplicate Protection crafting;
- shared semantic model so Rebalanced mechanics and all Clarity surfaces report the same effective values;
- exact mutually-exclusive packaging/plugin identity for Clarity-only vs Rebalanced.

No additional hosted CI is required for this documentation/audit lock.