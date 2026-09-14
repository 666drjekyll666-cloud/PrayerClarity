# Prayer Power Budget — first design pass

Status: design-analysis input, 2026-09-14. Stock reward/effect mechanics come from `PRAYER_MECHANICS.md`. Unlock/recipe context below is a current official-wiki cross-check and must be directly re-verified from 1.407 data before any production rebalance changes tech costs/recipes/unlocks.

Purpose: compare prayers by their **full investment and weekly opportunity cost**, not by one displayed buff number.

## Cost dimensions

For design purposes each prayer has at least five costs:

1. technology/prerequisite depth;
2. technology-point investment;
3. craft class/material investment;
4. quality/church-success investment;
5. one weekly sermon opportunity when used.

A niche effect may therefore justify unusually high power if the player pays substantial costs and gives up a universal sermon for it.

## First-pass matrix

| Prayer | Unlock context | Recipe class | Church q B/S/G | Special-role note | First design observation |
| --- | --- | --- | --- | --- | --- |
| Ordinary | starting sermon | none | — | baseline | deliberately temporary |
| Faith | `Faith`, effectively available with church reopening | chapter-tier | 10/20/50 | specialist Faith | low entry cost is part of its role |
| Prosperity | `Business of Faith` (current wiki: 10 Red + 5 Blue) | chapter-tier | 10/20/30 | permanent merchant progression | cheap enough to be a deliberate early progression choice |
| Donations | `Price of Faith` after Business of Faith (20 Blue) | chapter-tier | 10/20/50 | specialist donations | cheaper recipe/lower q than Combo preserve a progression reason |
| Combo | `Price of Faith` together with Donations/Cardinal (20 Blue after Business) | **book-tier** | 15/30/60 | universal Faith + donations | tech unlock is early/cheap relative to several niche prayers; production/q gate is its main tax |
| Repentance | `Power of Faith` after Comfort of Faith (current wiki: 10 Red + 30 Blue; prerequisite Comfort 10 Red + 5 Blue) | chapter-tier | 10/20/40 | intended confessional throughput | stock effect disconnected; new effect must justify investment/week slot |
| Shoots and Roots | `Gardening` after Improvement (50 Green + 10 Blue; Improvement costs 10 Red) | chapter-tier | 10/20/30 | farming throughput | recoverable -20% stock effect; judge power only after repair |
| Repose | `Embalming`, itself behind Embalming Liquids (current wiki: Embalming 50 Green + 10 Blue; Embalming Liquids 30 Green + 10 Blue) | **book-tier** | 20/40/50 | +1 Donkey max corpse tier | meaningful progression tool but has a natural expiry point |
| Retribution | `Martial Skills` after `Weapons`/Advanced Forging (Martial Skills: 10 Red + 75 Blue; Weapons: 55 Red + 25 Green) | **book-tier** | 10/20/40 | +5 damage, 36/72/108 min | relatively deep unlock + expensive prayer + weekly slot for one-dimensional combat effect |
| Protection | same `Martial Skills` unlock as Retribution | **book-tier** | 10/20/40 | +4 armor, 36/72/108 min | same structural issue; separately crafted/used despite paired unlock |
| Imagination | `Writing Tricks` after Writing/Inventing Stories (current wiki row: 10 + 40 tech points) | **book-tier** | 10/40/60 | +0.7 writing quality, 18/36/54 min | expensive but can create a very strong planned production window |
| Excellence | late crafting/smithing/building progression; official wiki currently lists Engineer and/or Tricks of the Trade paths | **book-tier** | 10/40/60 | +0.2 linked craft quality, 18/36/54 min | unlock provenance should be directly rechecked before tuning; niche value depends on affected craft set |
| BSS Soul's Repose | `soul_church_additions` | chapter + Sin Shards | 15/30/60 | Soul Gratitude enters Faith baseline | state-dependent specialist can strongly outperform ordinary Faith choices |
| BSS Soul Contentment | `soul_church_additions` | chapter + Sin Shards | 10/20/30 | +10% Soul Gratitude, 36/72/108 min | narrow workflow prayer; duration can span later sermon weeks |
| BSS Thorough Cleansing | `soul_church_additions` | chapter + Sin Shard | 10/20/30 | x2 Sin Shards, 36/72/108 min | useful reference: a narrow prayer can be deliberately very powerful and still healthy |

## Structural observations

### Combo's convenience arrives early

The Theology branch reaches `Business of Faith -> Price of Faith` quickly: Business unlocks Prosperity, and one additional 20-Blue technology unlocks both Donations and Combo plus Cardinal.

Combo is materially harder to **craft well** than chapter prayers because it uses a Book and has higher church-quality thresholds. That production gate is real and should not be erased from the analysis.

However, once solved, Combo combines both principal economic/faith specializations in one item. This explains why it frequently becomes the default rather than proving that every other prayer is numerically weak.

### Combat prayers pay much more progression cost than their simple output suggests

Retribution and Protection are two separate prayers. They sit behind the Smithing weapon path and `Martial Skills`, while each uses the same expensive book-sermon craft class as Combo.

This matters more than comparing +5 damage to a sword or +4 armor to armor: the player first invests in deeper technology, then crafts a separate prayer, then spends the week's sermon slot for a one-dimensional effect that cautious play/consumables may substitute.

The correct rework question is therefore not merely “should +5 become +10?” but “what combat prayer package would feel worthy of this full investment?”

### Quality can reduce opportunity cost without increasing magnitude

36/72/108-minute buffs are strategically different from 18/36/54-minute buffs. Higher tiers can stay active across later sermon opportunities, allowing a subsequent week to use Combo/Faith/another prayer while the old special effect persists.

This is legitimate quality scaling. The UI must make it obvious; otherwise players cannot value the upgrade.

### Chapter vs book is part of prayer identity

Chapter prayers generally have a lower production barrier and can remain attractive even when an equal-tier Book prayer looks stronger on paper. Recent players explicitly point out that a higher-quality Faith prayer can be preferable to a lower-quality Combo because books are the harder bottleneck.

Do not flatten chapter/book costs unless the whole writing progression is intentionally being redesigned.

## Candidate balancing levers

When a prayer is not tempting enough, evaluate these in order rather than defaulting to a flat buff:

1. clarify hidden strength first;
2. repair disconnected vanilla effect;
3. strengthen the niche effect;
4. improve quality scaling;
5. extend effect across useful time/weekly boundaries;
6. add a thematic secondary effect;
7. adjust church-quality success requirement to support a stronger reward;
8. adjust specialist vs generalist reward coefficients;
9. only then consider tech-tree/recipe changes, which have broader progression/save-compatibility consequences.

Graveyard quality is not a generic success gate. Use it only where the prayer's role has a clear graveyard/economic relationship.

## Data-quality notes

The stock q/effect/duration values in this project are direct 1.407 evidence. The technology-point and recipe-progression context in this first pass is a current external cross-check used for design orientation.

Before implementing any change to unlock position, tech cost, recipe inputs or exact progression gates, recover those fields directly from the installed 1.407 balance/tech data. Do not treat wiki values as sufficient authority for a production patch.

## Next use

Use this matrix together with `PRAYER_DESIGN_AUDIT.md` to draft concrete rework options. Each proposal must state:

- what player choice problem it solves;
- whether it is Clarity, Vanilla Fix or Balance/Rework;
- the full investment it is compensating for;
- why bronze is already attractive;
- why silver/gold are worth pursuing;
- what prevents the result from simply becoming a new universal meta.
