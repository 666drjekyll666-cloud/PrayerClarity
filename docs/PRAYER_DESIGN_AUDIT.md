# Prayer Design Audit — Graveyard Keeper 1.407

Status: **current prayer-by-prayer design source of truth**, reconciled 2026-09-19 with PrayerClarity: Vanilla 1.0.25, stable PrayerClarity: Rebalanced 0.2.3, and the user-accepted Rebalanced 0.2.4 duration candidate.

Stock mechanics remain canonical in `PRAYER_MECHANICS.md`. Exact current Rebalanced values are canonical in `PRAYER_REBALANCE_OPTIONS.md`. Historical alternatives and earlier coefficient experiments are retained in `PRAYER_POWER_BUDGET.md` and Git history as analysis only.

## Current accepted baselines

- **PrayerClarity: Vanilla 1.0.25** — `accepted/vanilla-1.0.25`, exact source `ebe069b4ad202ae786af9c63ded0ffb00502cff7`, release `v1.0.25`.
- **PrayerClarity: Rebalanced 0.2.3** — `accepted/rebalanced-0.2.3`, exact runtime source `ab1eb67cbf2465a912c392120395011503b720c3`, release `rebalanced-v0.2.3`.

The Rebalanced roster values were established in 0.2.0. Releases 0.2.2 and 0.2.3 changed runtime safety/ownership, not the nominal balance roster, except for the accepted Roots aggregate safety cap.

## Audit rules

- specialization must create a real reason to spend the weekly sermon on the specialist;
- Bronze must already be credible;
- Silver/Gold must justify their higher writing/quality burden;
- a narrow prayer may be deliberately strong in its niche;
- duration is real power only when the underlying activity can exploit the window;
- natural progression obsolescence is acceptable;
- healthy stock behavior should not be nerfed for symmetry;
- Church Quality requirements follow the whole proposition, not a universal ladder;
- failed sermons retain the full vanilla base donation pool by permanent project policy;
- base Faith/donations are distinct from prayer-owned success bonuses;
- gameplay and every UI surface must consume the same effective semantic/rule source.

## Current design matrix

| Prayer/family | Current verdict | Accepted design |
| --- | --- | --- |
| Ordinary | **No change** | Stock starter baseline, q10. |
| Faith | **Accepted specialist rework** | q20/40/60; flat success-only **+5/+10/+20 Faith**; no prayer-owned donation bonus. |
| Donations | **Accepted specialist rework** | q20/40/60; flat success-only **+5/+10/+15 silver**; no prayer-owned Faith bonus. |
| Combo | **Accepted generalist rework** | q40/60/80; success-only **+100/+150/+200% Faith and donations**; no prayer-owned flat Faith/money. |
| Prosperity | **No balance change** | Stock q10/20/30 and 1/2/3 Commercial Blessings. |
| Shoots & Roots | **Accepted repair + scaling** | q10/30/50; nominal growth time **-20/-30/-40%**, 36/72/108 min; current runtime also enforces the accepted 95% combined reduction safety cap. |
| Repentance | **Accepted rework; 0.2.4 duration candidate accepted** | q20/40/60; daily confession probability **50/75/100%**; candidate duration **30/42/54 min** (stable 0.2.3: 18/36/54). |
| Repose | **Accepted reliability rework; 0.2.4 duration candidate accepted** | q20/40/50; Bronze stock-style pool, Silver moves halfway toward certainty, Gold guarantees the best actually existing eligible ordinary corpse tier; candidate duration **30/42/54 min** (stable 0.2.3: 18/36/54). |
| Combat | **Accepted structural/numeric rework** | q20/40/60; damage **+5/+10/+15**, armor **+4**, regen **1/2/4 HP/s**; 36/72/108 min. |
| Imagination | **Accepted premium-output rework** | q20/40/60; writing quality **+0.7** all tiers; successful Silver -> 3 Silver Stories, Gold -> 3 Gold Stories; 18/36/54 min. |
| Excellence | **Accepted magnitude rework** | q20/60/90; linked-craft quality **+0.2/+0.5/+1.0**; 18/36/54 min. |
| BSS Soul's Repose | **Accepted state-scaling Faith specialist** | q30/60/120; **+50/+100/+150% Faith** on the verified Souls base; current Soul Gratitude remains an input. |
| Soul Contentment | **Accepted rework** | q20/40/60; **+20% Soul Gratitude** all tiers; 36/72/108 min. |
| Thorough Cleansing | **No magnitude increase** | q30/60/90; **x2 Sin Shards** all tiers; 36/72/108 min. |

## Faith / Donations / Combo family

Faith and Donations are flat specialists: Faith gives +5/+10/+20 Faith, Donations gives +5/+10/+15 silver, and both guarantee at q20/q40/q60.

Combo is a percentage generalist: +100/+150/+200% to both Faith and donations, guaranteed at q40/q60/q80.

This creates the intended progression:

`early focused specialists -> mature specialists / early Combo -> late premium Combo`

The specialists retain early value because their prayer-owned reward does not depend on a large church/graveyard base. Combo becomes increasingly attractive as the underlying sermon economy matures, but pays the higher Church Quality gate and Hard Book recipe.

Do not restore the obsolete +200/+300/+400% specialist ladder or q25/40/70 requirements from the early audit. Those values are historical only.

## BSS Soul's Repose

Verified base Faith is:

`Base Faith = (Church Quality + current Soul Gratitude) * 0.1 * EloquenceFactor`

Current Soul Gratitude means the amount held at sermon time, not lifetime healed souls and not capacity.

The accepted q30/60/120 ladder gates a +50/+100/+150% prayer-owned Faith bonus. Gold q120 is intentionally aspirational late-game power. This prayer remains state-dependent rather than being normalized to the ordinary Faith specialist.

## Repentance

The accepted design repairs an otherwise disconnected stock prayer by reusing the game's daily confession scheduler and two-confessional loop.

Current probability ladder is 50% / 75% / 100%. Stable 0.2.3 duration is 18/36/54 minutes. For 0.2.4, the accepted candidate is **30/42/54 minutes**.

Because the effect acts only on once-per-game-day confession rolls, duration must be judged by how many future daily rolls actually occur while the buff is live, not just by the probability itself. The accepted 0.2.4 revision raises Bronze enough to cover about four stock-day opportunities, moves Silver to a distinct intermediate window, and leaves Gold unchanged.

## Imagination

The healthy stock core remains +0.7 writing quality at all tiers.

Premium tiers add a bounded success reward:

- Bronze: no extra Story;
- Silver: 3 Silver Stories on sermon success;
- Gold: 3 Gold Stories on sermon success.

This is a weekly premium output layered onto a writing-planning buff, not intended to replace the broader Story economy. Any future change should compare the three-Story grant against confessionals, player/zombie Story crafting, Journalist/dialogue sources, Better Save Soul Story crafting where available, and finite NPC exchanges.

## No global q normalization

Different prayers pay different unlock, recipe, opportunity-cost and activity-window costs. Their q ladders are deliberately non-uniform. Uniform q would be visual symmetry, not better balance.

## Role-collision check

The current roster keeps distinct jobs:

- Faith / Donations / Combo: focused flat specialists versus scaling generalist;
- Repentance: confession throughput and delayed church-side Faith/Story generation;
- Roots: farming/growth acceleration;
- Repose: corpse-progression reliability;
- Combat: one coherent combat-preparation package;
- Imagination: writing-quality planning plus small premium Story injection;
- Excellence: broader linked-craft quality;
- Prosperity: permanent merchant progression that can naturally become obsolete;
- Soul Contentment / Thorough Cleansing: narrow Better Save Soul workflow accelerators.

A prayer should be reopened when new evidence shows that this role is not actually tempting, materially overlaps another system, or creates a power-budget problem.

## Current architecture consequence

Current stable 0.2.3 keeps Graveyard Keeper authoritative where practical:

- Roots leaves stock growth formulas intact and projects only the native input, with the accepted 95% aggregate cap;
- Repentance leaves the stock daily reset/RNG/loop intact and projects only the effective `confession_probability` read while the native buff is live;
- Combat damage/armor use scoped native `add_damage` / `add_armor` inputs; regeneration uses the native buff tick extension point;
- Repose narrows only the verified ordinary Donkey generation context and still calls stock `GenerateBody`;
- Excellence uses the exact stock `GetBuffValue("buff_star")` semantic seam;
- Soul Contentment retains its accepted narrow live-graph coefficient projection.

`REBALANCED_NATIVE_SEAM_AUDIT.md` and `POST_AUDIT_VERDICT.md` are authoritative for implementation/lifecycle closure.

## Current status

The stable balance/architecture baseline is **Rebalanced 0.2.3**. There is no blanket rebalance or architecture task pending.

Known non-blocking evidence gaps remain:

- terminal Repose endpoint wording/presentation has not been observed on the user's terminal-progression save state;
- the physical 3 Silver / 3 Gold Story drop from a real successful premium Imagination sermon has not yet been visually observed, although it uses the verified native sermon-drop path.

Future changes should start from a concrete gameplay/UX finding and create a new explicit proposal rather than reviving superseded historical values.
