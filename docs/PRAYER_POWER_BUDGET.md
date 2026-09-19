# Prayer Power Budget — Graveyard Keeper 1.407

Status: **historical quantitative design-analysis input**, 2026-09-14, retained as rationale for the accepted Rebalanced roster. Stock mechanics remain canonical in `PRAYER_MECHANICS.md`; current accepted Rebalanced values are canonical only in `PRAYER_REBALANCE_OPTIONS.md`. Candidate coefficient examples below are historical and must not override the current stable Rebalanced 0.2.10 ruleset.

The technology and recipe values below were recovered from direct 1.407 `GameBalance`/craft-registry runtime dumps rather than inferred from the wiki. Where a technology is hidden/quest-gated, its internal `price` field is not automatically treated as a player-paid cost.

## Power-budget model

A prayer has several different costs. They must not be collapsed into one fake number:

1. **marginal unlock cost** — the technology that actually exposes the prayer;
2. **route/depth cost** — prerequisite technologies showing how late/deep the prayer becomes available;
3. **shared production infrastructure** — writing/book technologies and stations used by many systems, not uniquely purchased for one prayer;
4. **prayer recipe cost** — Chapter/Book, Faith and DLC materials consumed by the item itself;
5. **quality cost** — producing a sufficiently good Chapter/Book to obtain bronze/silver/gold;
6. **success gate** — current sermon requirement and church-quality chance;
7. **weekly opportunity cost** — choosing this prayer displaces every other sermon for that Pride day/week.

The design target is **temptation parity**: the whole proposition should feel worth buying and worth spending the sermon slot on at the stage where its role matters.

## Direct 1.407 matrix

`R/G/B` = red/green/blue tech points. `q` = church quality required for guaranteed success.

| Prayer | Direct unlock context | Prayer recipe | q B/S/G | Stock special role | Power-budget reading |
| --- | --- | --- | --- | --- | --- |
| Ordinary | starter/quest baseline | none | — | baseline sermon | intended to be replaced |
| Faith | hidden `Faith` unlock; effectively church-opening progression | **Chapter + 5 Faith** | 10/20/50 | Faith specialist | low production/q barrier; however its Faith coefficient is **identical to Combo** at equal tier |
| Prosperity | `Faithbuisness`: **10R + 5B**, parent Faith | **Chapter + 5 Faith** | 10/20/30 | 1/2/3 Commercial Blessings + fixed outputs | strong early progression tool; naturally exhausts itself after merchant tiers |
| Donations | `Price of faith`: **20B**, parent Faithbuisness; visible Theology route from Faith = **10R + 25B** | **Chapter + 5 Faith** | 10/20/50 | donation specialist | same donation coefficient as Combo at equal tier; specialization is only cheaper recipe/lower q once unlocked |
| Combo | **same `Price of faith` unlock as Donations** | **Hard Book + 7 Faith** | 15/30/60 | Faith + donations generalist | early shared tech unlock; higher production/q gate is its main tax; after that gate is solved it dominates equal-tier specialists in breadth |
| Repentance | `Сomfort of faith` 10R+5B -> `Power of faith` 10R+30B; visible route = **20R + 35B** | **Chapter + 5 Faith** | 10/20/40 | intended extra confessions | stock special effect absent; rework must justify a real tech/craft/week investment |
| Shoots and Roots | `Improvement` 10R -> `Gardening` **50G + 10B**; visible route = **10R + 50G + 10B** | **Chapter + 5 Faith** | 10/20/30 | intended -20% growth time | proven Vanilla Fix first; only then judge whether -20% is sufficiently tempting |
| Repose | `Embalm 1` **30G + 10B** -> `Embalming` **50G + 10B** (plus quest/start gates); visible paid route = **80G + 20B** | **Hard Book + 7 Faith** | 20/40/50 | Donkey max corpse tier +1, 18/36/54 min | costly book prayer with finite progression window; needs clear value while that window is open |
| Retribution | Smithing route to `Martial skills`: Primitive Forging 10R -> Advanced Forging 40R+5G -> Weapons 55R+25G -> Martial Skills **10R+75B**; visible route from Iron = **115R + 30G + 75B** | **Hard Book + 7 Faith** | 10/20/40 | +5 damage, 36/72/108 min | very deep availability + expensive item + weekly slot for one stat line; structural rework candidate |
| Protection | **same Martial Skills unlock** as Retribution | **Hard Book + 7 Faith** | 10/20/40 | +4 armor, 36/72/108 min | second separately crafted/used combat prayer with same full opportunity cost; structural rework candidate |
| Imagination | Writing 5R+5B -> Inventing Stories 25B -> Writing Tricks **10R+40B**; visible prayer route from Research = **15R + 70B** | **Hard Book + 7 Faith** | 10/40/60 | +0.7 writing-quality input, 18/36/54 min | expensive but demonstrably creates a powerful planned writing window; healthy reference case |
| Excellence | accessible from **two late routes**: `Engineer` (smithing) or `Working trick` (building) | **Hard Book + 7 Faith** | 10/40/60 | +0.2 linked-craft quality, 18/36/54 min | opportunistic late infrastructure prayer; role value depends on affected crafts, not one unique path |
| BSS Soul's Repose | `soul_sins_1` -> `soul_sins_2` **10R+15G+10 Gratitude** -> `soul_church_additions` **25R+50G**; cumulative paid = **35R+65G+10 Gratitude** | **Chapter + 5 Faith + 2 Sin Shards** | 15/30/60 | Soul Gratitude boosts Faith baseline | state-dependent specialist can exceed ordinary/Combo Faith strongly; healthy niche candidate |
| BSS Soul Contentment | same shared BSS unlock | **Chapter + 4 Faith + 2 Sin Shards** | 10/20/30 | +10% Soul Gratitude, 36/72/108 min | narrow workflow effect; high-quality duration can span later sermon weeks |
| BSS Thorough Cleansing | same shared BSS unlock | **Chapter + 4 Faith + 1 Sin Shard** | 10/20/30 | x2 Sin Shards, 36/72/108 min | deliberately powerful niche; useful benchmark for temptation parity |

## Shared writing-production burden

Prayer recipe labels understate the real early-game burden.

A Chapter is itself produced from **3 multi-quality Notes**. A Hard Book consumes a multi-quality Chapter plus a multi-quality cover. The book/cover chain adds its own writing technology, materials and quality bottlenecks. The `Books` technology itself costs **50R + 10B** and is shared infrastructure rather than a cost unique to Combo/combat/etc.

Therefore do not charge the entire writing tree to each book-sermon, but do preserve the meaningful distinction:

- **chapter-tier prayer**: materially easier to enter and quality-upgrade;
- **book-tier prayer**: a higher production/quality gate, especially before writing infrastructure is mature.

This is a legitimate part of vanilla balance. The problem begins when that one-time production gate stops mattering while the generalist benefit remains permanently superior in breadth.

## Major structural finding — specialists are not actually stronger specialists

At equal quality:

- Faith prayer `k_faith` = **0.5 / 1.0 / 1.5**;
- Combo `k_faith` = **0.5 / 1.0 / 1.5**.

And:

- Donations `k_money` = **0.5 / 1.0 / 1.5**;
- Combo `k_money` = **0.5 / 1.0 / 1.5**.

The same-tier specialist therefore does **not** beat Combo at its own specialty. Combo simply adds the other strong specialty at the same time. Faith/Donations retain two meaningful progression advantages:

- Chapter +5 Faith instead of Hard Book +7 Faith;
- lower q requirements (10/20/50 vs Combo 15/30/60).

Those advantages matter early. Once books and church quality are routine, they cease to create an output tradeoff.

**Design diagnosis:** this is the clearest systemic rework pressure in the base roster. A healthy specialist/generalist relationship should normally make the specialist best at the thing it specializes in.

### Historical preferred rework direction

Do not nerf Combo first. Test a **specialist premium**:

- Combo remains the strong convenient generalist;
- Faith becomes the best Faith-producing ordinary sermon;
- Donations becomes the best money-producing ordinary sermon;
- silver/gold specialist progression should increasingly reward committing to the specialty.

Illustrative coefficient families such as `.75 / 1.5 / 2.25` were **historical design hypotheses only**. They were superseded by later accepted roster revisions; the current stable specialist/generalist grammar is documented only in `PRAYER_REBALANCE_OPTIONS.md` for Rebalanced 0.2.10.

## Combo gate — candidate levers

Combo already pays a real generalist tax through Book +7 Faith and q15/30/60. Candidate changes should preserve that logic rather than simply punish the player.

Order of preference:

1. strengthen specialist outputs;
2. make specialist quality progression diverge more strongly from Combo;
3. only if choice compression remains, consider a higher Combo church requirement;
4. avoid moving the technology or inflating recipe cost unless simpler levers fail.

The goal is a real choice: **good at both** versus **best at one**.

## Thematic requirements — useful, but avoid double scaling

The user's design idea that requirements can follow the prayer's theme is strong. Donations is the clearest candidate because graveyard quality already creates the donation baseline.

However, making graveyard quality both the continuous reward input **and** the only success probability can create an unnecessarily strong rich-get-richer loop.

Candidate architectures, from least invasive to most:

1. **Church still controls sermon success; graveyard unlocks/scales the specialist premium.** Example: the Donations tooltip can expose a Graveyard Quality target for full specialist bonus while success remains church-based.
2. **Hybrid requirement:** both church and graveyard thresholds matter, but only as clear gates, not multiplicative reward scaling.
3. **Graveyard-only success gate for Donations:** thematically clean but breaks the universal sermon-success grammar; reserve unless the first two are unsatisfying.

Current preference: preserve **church = ability to deliver a sermon** and use **thematic state = how strongly the specialist effect pays off**. This is easier to learn and less likely to create opaque double taxation.

The same principle may later apply selectively to other prayers (Soul Gratitude already does this naturally). Do not add thematic gates where they exist only for symmetry.

## Repentance power budget

Direct stock evidence:

- ordinary confession probability = **15% per confessional roll**;
- Repentance's intended role is more confessions;
- no surviving stock consumer/multiplier exists;
- recipe = Chapter +5 Faith;
- visible Theology route from Faith = 20R+35B;
- q = 10/20/40;
- duration = 18/36/54 min;
- it consumes the week's sermon choice.

A flat `15% -> 30%` is a sensible **first benchmark** because it cleanly doubles the base chance, but it is not automatically a sufficient prayer reward. With two confessionals, expected events per daily roll rise from 0.30 to 0.60; the absolute gain is still only 0.30 expected confessions per day before considering the finite buff window.

Therefore test Repentance as a quality-scaled throughput prayer rather than assuming one flat doubled value for every tier.

Candidate families, not accepted:

- **conservative:** 30% / 45% / 60% (2x / 3x / 4x the base chance);
- **aggressive:** 30% / 50% / 70%;
- **duration-led:** smaller chance growth but substantially longer windows.

The final choice should model the actual Faith/value produced by Confessional I/II, number of usable confessionals and how many daily rolls fit the effect window. Bronze must already feel worthwhile; gold should feel like a meaningful religious-event week rather than merely “the timer lasts longer.”

## Combat prayer budget

Retribution and Protection are not one prayer. They are two separate book-sermons unlocked together, each requiring its own Book +7 Faith and its own weekly sermon use.

That makes the relevant comparison holistic:

- very deep smithing route before availability;
- expensive book quality;
- one weekly slot per stat;
- limited sustained-combat demand;
- consumables and cautious play substitute for much of the need.

Their raw +5 damage/+4 armor are not tiny, but the **offer** can still be unattractive.

Rework options to model next:

1. combine meaningful offense + defense into one coherent combat-preparation prayer and repurpose the other;
2. keep two prayers but make each a strong thematic package rather than a single stat;
3. let quality increase magnitude/secondary behavior rather than only duration;
4. increase requirements if stronger packages need a progression gate.

Do not choose concrete combat numbers until verified stock player/enemy parameters show which existing effects can form stable packages.

## Quality and weekly boundaries

Using the vanilla time base, one game day is about 7.5 real minutes and the weekly sermon interval is roughly 45 minutes.

This makes duration scaling strategically meaningful:

- 18/36/54 min ≈ 2.4 / 4.8 / 7.2 game days; only gold cleanly spans a full weekly interval;
- 36/72/108 min ≈ 4.8 / 9.6 / 14.4 days; silver/gold can remain active across one or more later sermon opportunities.

That can be legitimate quality power. It should be explicitly surfaced by Clarity. But where the useful activity lasts only briefly, extra duration alone is not enough reason to pursue gold.

## Current rework priority after quantitative audit

### Tier 1 — clear structural work

1. **Shoots and Roots:** restore stock -20% path as Vanilla Fix; then evaluate real attractiveness.
2. **Faith / Donations / Combo family:** establish specialist premium so specialist prayers remain meaningfully best at their target after the Book/q gate is solved.
3. **Repentance:** design a new confession-throughput rule; 30% is a baseline benchmark, not a final answer.
4. **Retribution / Protection:** redesign the proposition, not merely the raw stat, because two deep book-prayers each spend a full week for one combat line.

### Tier 2 — clarity first, then reassess

- Repose;
- Excellence;
- Soul Contentment.

These may be fine once effect, quality progression and relevance window are visible.

### Tier 3 — strong/healthy reference cases

- Prosperity;
- Imagination;
- high-Gratitude BSS Soul's Repose;
- Thorough Cleansing.

These show that a niche prayer can be highly desirable without becoming the universal meta.

## Design gate result

This historical gate was completed during the early Rebalanced line. The roster was revised further and the current stable result is PrayerClarity: Rebalanced 0.2.10. This file remains useful for the underlying unlock/craft/quality/opportunity-cost evidence, but it is **not** the source of truth for current Rebalanced numbers.

Any future rebalance should reuse this full-cost framework and create a new explicit proposal rather than resurrecting the superseded candidate values above.