# Graveyard Keeper 1.407 — Game Needs Matrix for prayer design

Status: **research synthesis; no production balance change**.

Purpose: identify the player problems that can plausibly justify spending the weekly sermon slot, and check whether each PrayerClarity: Rebalanced prayer becomes available while its target problem is still alive.

This is deliberately broader than a late-game audit, but it is **not** an attempt to catalogue every system in Graveyard Keeper. The matrix covers needs that can plausibly matter to prayer design: progression resources, throughput, timing, quality, corpse/soul workflow, and completionist goals.

## Stage model

Graveyard Keeper is nonlinear, so these are overlapping design stages rather than exact day ranges.

1. **Pre-church / opening** — graveyard cleanup, basic tools/materials, food/energy, first money.
2. **Early church** — sermons, Study Table, Faith/Science/blue-point bottleneck, first writing and vendor progression.
3. **Developing keeper / midgame** — alchemy, better writing, farming, dungeon/combat, embalming, first zombies and automation, Merchant/Trade Office progression.
4. **Advanced infrastructure** — large church/cathedral, mature writing, high-quality crafting, Trade Office/tavern/zombie economy, advanced corpse work.
5. **DLC/endgame** — Better Save Soul, high Church Quality, quest completion, mature automation.
6. **Completionist/postgame** — perfect corpses/graveyard, high zombie quality, optimization and convenience after normal progression resources lose value.

Legend:
- **+++** acute/core bottleneck;
- **++** meaningful recurring need;
- **+** situational/project-specific;
- **0** usually solved or weak;
- **C** completionist long-tail.

## Needs matrix

| Player need | Pre-church | Early church | Midgame | Advanced | DLC/endgame | Completionist | Natural game-side resolution |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| **Money** | +++ | +++ | +++ | ++ | + / 0 | 0 | burial certificates / vendor selling -> silver, wine, crates -> Trade Office / tavern abundance |
| **Faith** | 0 | +++ | +++ | ++ | + | + / C | sermons/confessionals; Study costs eventually end, but zombies and high-end marble/BSS crafts can still consume Faith |
| **Science / blue tech points** | 0 | +++ | +++ | + | 0 | 0 | Study Table + Faith/Science, then craft-generated points and finite tech tree completion |
| **Energy / food** | +++ | +++ | ++ | + | 0 / + | 0 | better food, wine, speed/automation; eventually abundance rather than scarcity |
| **Basic materials / hauling** | +++ | +++ | ++ | + | + | + | quarry + zombies + porter chains; late pain shifts from scarcity to logistics/micro |
| **Vendor tier access** | + | ++ | ++ | 0 | 0 | 0 | normal trade progression or Blessings of Commerce; finite problem |
| **Manual farming throughput / crop growth** | ++ | ++ | +++ | ++ | + | + | fertilizers, better seeds, zombie farms/vineyards; some seed/manual maintenance remains |
| **Writing quality / Stories / books** | 0 | ++ | +++ | ++ | + | + / C | perks + Desk II + Inspiration; batch writing can remain valuable for prayers/books/points |
| **Corpse quality / useful incoming bodies** | ++ | ++ | +++ | ++ | + | C | story corpse-tier progression, embalming, Cultist/BSS organ upgrading; terminal natural tier eventually removes stock Repose value |
| **Perfect-body / perfect-graveyard throughput** | 0 | 0 | + | ++ | +++ | C+++ | embalming + BSS organ enhancement; Sin Shards become the long-tail resource |
| **Combat power** | + | + | +++ | + | 0 / + DLC | 0 | dungeon is finite and does not repopulate; outside it combat is sparse |
| **Confessional output (Faith/Stories)** | 0 | + | ++ | ++ | + | 0 / + | confessionals; useful while Faith/Stories matter, weaker after those stockpiles lose value |
| **Soul Gratitude / BSS tech progression** | 0 | 0 | 0 | 0 | +++ -> ++ | + / 0 | healing/portal; later primarily Remote Craft Control and a few BSS interactions |
| **Sin Shards** | 0 | 0 | 0 | 0 | +++ | C+++ | soul healing; very large optional sink in organ perfection |
| **Soul handling / decay pressure** | 0 | 0 | 0 | 0 | ++ / +++ | + | Soul Containers extend the window, but corpse refrigeration does not preserve the soul |
| **Calendar waiting / time advancement** | + | ++ | ++ | ++ | +++ | ++ | meditation and planning; late game often shifts from resource scarcity to waiting for NPC/week cycles |
| **High-end craft quality** | 0 | + | ++ | +++ | ++ | C | perks/workstations + targeted quality buffs; useful in planned production batches |
| **Information / recipe / logistics friction** | ++ | +++ | +++ | ++ | ++ | ++ | mostly UX rather than balance; external lookup and memory remain common player work |

## Evidence notes by need

### Money

Recent 2026 player reports still describe money as an acute early/mid bottleneck. Basic selling such as coal, stone, firewood, food, burial certificates or metal can produce meaningful day-scale income before industrial systems exist.

The strongest fixed progression spike is the **12 gold Aristocrat status** near the end of the main progression. After Trade Office and/or Stranger Sins tavern automation are mature, money commonly flips from bottleneck to surplus. Trade Office supports up to 10 crate pallets; current crate prices are about 15s for goods and 16.5s for gold produce.

Implication: money has a strong **phase window**, not an evergreen endgame role.

Sources:
- https://www.reddit.com/r/GraveyardKeeper/comments/1sjg3aq/
- https://www.reddit.com/r/GraveyardKeeper/comments/1sz2tu7/
- https://graveyardkeeper.fandom.com/wiki/Trade_Office
- https://graveyardkeeper.fandom.com/wiki/The_Graveyard

### Faith / research

Faith is one of the clearest early/midgame bottlenecks because most Study Table research consumes it, and early blue-point acquisition depends on Study. Faith also funds sermons, Zombies (10 Faith), advanced stone/marble work, and some BSS crafts.

The tech/research sink is finite. Once research and core infrastructure are done, Faith demand becomes playstyle-dependent: a completionist building many zombies or high-end marble decorations can still consume hundreds, while a normal story-completion player can accumulate large unused reserves.

Implication: Faith is a healthy early/mid resource target, but **“more Faith” alone is not automatically an endgame reward**.

Sources:
- https://graveyardkeeper.fandom.com/wiki/Faith
- https://graveyardkeeper.fandom.com/wiki/Study_table
- https://www.reddit.com/r/GraveyardKeeper/comments/1sia0bl/
- https://www.reddit.com/r/GraveyardKeeper/comments/1s5834w/
- https://steamcommunity.com/app/599140/discussions/0/3808409963279695415/

### Farming / automation

Manual farming is repeatedly described as laborious before zombie automation. Zombie farms/vineyards and abundant money reduce the importance of raw growth speed later, although gold-seed maintenance and certain manual crop work can remain.

Implication: a farming prayer needs to be attractive **before automation solves most of its problem**; it does not need to remain a universal endgame choice.

Sources:
- https://www.reddit.com/r/GraveyardKeeper/comments/1sv366d/
- https://www.reddit.com/r/GraveyardKeeper/comments/1fh7vwy/
- https://www.reddit.com/r/GraveyardKeeper/comments/1t61pe2/

### Writing

Writing quality has a genuine midgame production bottleneck: higher prayers/books and story conversion benefit from Writer/Playwright, better desks and Prayer for Imagination.

A 2026 player report demonstrates that a single Imagination window can support a large planned writing batch, producing books and hundreds of tech points. This is a good example of a prayer solving a **temporary production project**, not an evergreen resource shortage.

Sources:
- https://www.reddit.com/r/GraveyardKeeper/comments/1ul3qc4/
- https://www.reddit.com/r/GraveyardKeeper/comments/18sp9hd
- https://www.reddit.com/r/GraveyardKeeper/comments/1faiobu/

### Combat

The dungeon is the main sustained combat phase and is finite; it does not repopulate. Outside it, only small numbers of bats/slimes and limited DLC encounters remain.

Implication: combat prayers naturally have a finite role window. Their quality should be judged by whether they are tempting **during dungeon progression**, not by postgame usefulness.

Sources:
- https://www.reddit.com/r/GraveyardKeeper/comments/1g53nja
- https://www.reddit.com/r/GraveyardKeeper/comments/1sqplt2/
- https://www.reddit.com/r/GraveyardKeeper/comments/10q3iky/

### Corpse quality / Repose

Stock corpse quality rises with progression. Community understanding correctly identifies a terminal point where stock Repose no longer unlocks a higher natural corpse tier.

PrayerClarity 0.2.15+ deliberately extends Repose's role by making Gold guarantee the maximum visible skull quality inside the best available ordinary tier. This turns it from a purely progression-window prayer into a possible **corpse-quality/completionist specialist**.

Sources:
- direct PrayerClarity 1.407 evidence in `PRAYER_MECHANICS.md`;
- https://www.reddit.com/r/GraveyardKeeper/comments/1ty6zka/
- https://www.reddit.com/r/GraveyardKeeper/comments/o2j8py/

### BSS: Soul Gratitude and Sin Shards

Soul Gratitude is required during BSS progression and later powers Remote Craft Control. Its value can fall once the BSS tree/workflow is mature.

Sin Shards differ: they have a very large legitimate completionist sink because organ enhancement can consume many shards per perfected corpse. Current players pursuing 26-white-skull bodies explicitly report needing thousands and recommend Prayer for Soul's Thorough Cleansing.

Implication: **Sin Shards are a much healthier late/postgame resource target than raw Soul Gratitude, money, or generic Faith.**

Sources:
- https://graveyardkeeper.fandom.com/wiki/Soul_Gratitude
- https://graveyardkeeper.fandom.com/wiki/Sin_Shard
- https://graveyardkeeper.fandom.com/wiki/Souls_room
- https://www.reddit.com/r/GraveyardKeeper/comments/1tbe3cl/
- https://www.reddit.com/r/GraveyardKeeper/comments/1sllyim/

### Time / friction

Late game increasingly becomes a management/waiting problem rather than a raw-material problem. Automation creates abundance, while NPC weekdays, Merchant/service cycles, DLC progression, transport and residual manual tasks remain.

This is not a single proven universal bottleneck, but recent community discussion repeatedly describes both sides of the late-game state: automation removes scarcity while leaving tedium/waiting, or creates a feeling that there is little meaningful work left.

Implication: **workflow convenience and bounded time/throughput effects are legitimate endgame reward space**, often more relevant than printing another stockpile currency.

Sources:
- https://www.reddit.com/r/GraveyardKeeper/comments/1t61pe2/
- https://www.reddit.com/r/GraveyardKeeper/comments/1tqeijw/
- https://www.reddit.com/r/GraveyardKeeper/comments/1t00xki/

## Prayer-role overlay against player needs

Current accepted Rebalanced 0.2.16 roster is the mechanical baseline.

| Prayer | Intended/accepted role | Need window | Current fit | Research verdict |
| --- | --- | --- | --- | --- |
| **Casual** | starter sermon | first church services | alive briefly | **Healthy natural starter; obsolescence is expected.** |
| **Faith** | direct Faith specialist | early church -> research/zombie buildup | matches strongest Faith scarcity | **Healthy progression specialist. Late obsolescence is acceptable.** |
| **Donations** | direct money specialist | early/mid money shortage -> Aristocrat buildup | target need is real, but weekly +5/+15/+30s competes with easy day-scale selling and later crates/tavern | **Reopen. Role is valid, current payout/window fit is questionable. Do not judge by late-game money abundance alone.** |
| **Combo** | convenient Faith + money generalist | midgame when both still matter | broad convenience is valuable; later money and sometimes Faith lose scarcity | **Healthy bridge/generalist; does not need to be evergreen.** |
| **Prosperity** | vendor-tier accelerator | early/mid vendor progression | directly solves a finite friction and then becomes obsolete | **Excellent progression-tool pattern. Natural obsolescence is healthy.** |
| **Shoots & Roots** | farming throughput | manual/partly automated agriculture | strongest before zombie farming and abundant seed/money economy | **Likely healthy phase specialist if magnitude is tempting during that window. Do not force endgame relevance.** |
| **Repentance** | more confessional events -> Faith/Stories | church midgame while Faith/Stories matter | solves a real but relatively soft/indirect need; loses value as both outputs stockpile | **Phase-limited specialist. Keep under observation; no need for evergreen value.** |
| **Repose** | better/reliable corpse quality | corpse progression -> completionist bodies | Rebalanced Gold now preserves a late quality role | **Strong fit after 0.2.15; unusual success because the rework moves it from finite progression into legitimate completionist value.** |
| **Combat** | dungeon/combat preparation | dungeon progression | target need disappears after finite combat content | **Valid finite-role prayer; evaluate against dungeon window only.** |
| **Imagination** | planned high-quality writing batch | writing/prayer/book production | direct evidence of powerful use during live writing bottleneck | **One of the healthiest role fits.** |
| **Excellence** | planned high-quality linked crafting | advanced project crafting | narrow but intentionally strong when the player has a specific premium craft batch | **Healthy project specialist. q95 Gold fits capstone/project preparation.** |
| **Soul's Repose** | high-end Faith specialist scaling with Soul Gratitude | BSS/endgame | arrives when generic Faith often has declining marginal value; at ordinary SG it can also lose to Combo despite q120 | **Major role/value mismatch. Reopen concept, not just coefficient.** |
| **Soul Contentment** | faster Soul Gratitude gain | BSS progression / RCC buildup | solves a real BSS progression need but can naturally become obsolete | **Probably healthy progression/DLC specialist; natural obsolescence acceptable.** |
| **Thorough Cleansing** | double Sin Shards | BSS -> perfect bodies/graveyard | target resource has a very large long-tail sink | **Excellent late/completionist fit. One of the strongest examples of a true endgame prayer.** |

## Strong conclusions

### 1. Not every prayer needs late-game relevance

The matrix supports the existing project rule. Prosperity is the clearest example: it solves vendor tiers extremely well and then has completed its job.

The correct target is:

> a convincing moment where the player wants the prayer while its problem is alive.

Not:

> every prayer must remain competitive forever.

### 2. Resource prayers should be strongest before abundance systems erase scarcity

Faith and money are genuine early/mid bottlenecks. Zombie automation, completed research, Trade Office and tavern systems later turn many basic resources into abundance.

Therefore a resource prayer can be excellent even if it eventually becomes obsolete — but it must be **strong enough during its intended window to beat the weekly opportunity cost**.

This specifically reopens Donations.

### 3. Project/throughput prayers age better than generic currency prayers

Imagination, Excellence, Repose and Thorough Cleansing remain interesting because they improve a task the player has deliberately chosen to do:
- write a batch of high-quality items;
- craft premium outputs;
- obtain/prep better corpses;
- perfect bodies with large shard demand.

These effects scale with player intent rather than only with currency scarcity.

### 4. True late-game reward space is mostly workflow + completionism

The strongest persistent late-game needs identified here are:
- perfect-body / perfect-graveyard throughput;
- Sin Shards;
- corpse/soul handling;
- high-end project quality;
- calendar/waiting/logistics friction;
- optional large zombie/graveyard projects.

Money, tech points and often Faith are increasingly weak endgame targets.

## Priority design consequences

### A. Soul's Repose — highest priority reopen

Do not assume the fix is a larger Faith multiplier.

The matrix says the deeper problem is that **q120 arrives in a stage where generic Faith may no longer be the player's main need**.

Candidate concepts should therefore be tested against at least two targets:
1. preserve its identity as the strongest Faith specialist for players who still need Faith;
2. add or substitute an effect that addresses an actual BSS/endgame need without colliding with Soul Contentment or Thorough Cleansing.

Promising design space:
- soul-decay / soul-handling relief;
- healed-soul / portal throughput reward;
- bounded BSS workflow convenience;
- another thematic completionist benefit.

No production design is accepted yet.

### B. Donations — second priority reopen

The need itself is real, especially before the 12-gold Aristocrat gate. The issue is **value density per weekly sermon**, not that money is universally useless.

Current +5/+15/+30 silver must be compared with:
- simple vendor selling on a day-scale;
- silver/wine progression;
- crate income once Trade Office comes online;
- the competing sermon slot.

Possible outcome: make Donations substantially stronger in its intended early/mid window and explicitly accept its later obsolescence, rather than trying to make flat silver competitive with endgame business automation.

### C. No immediate blanket rework for the rest

Current evidence does **not** justify reopening every prayer:
- Prosperity's finite role is healthy;
- Combat's finite role is inherent to the game;
- Imagination and Thorough Cleansing have particularly good need-role alignment;
- Repose's accepted rework now has a credible completionist role;
- Roots, Repentance and Soul Contentment are phase-limited by design and should only be reopened if runtime/player evidence shows they fail inside their actual target windows.

## Next useful research

The matrix narrows the next work rather than expanding it.

1. **Donations window audit** — quantify realistic early/mid income alternatives at the exact stage Bronze/Silver/Gold Donations become available, including ordinary vendor wallet recovery.
2. **Soul's Repose concept audit** — compare 3-5 candidate endgame/BSS roles against the matrix and against role collision with Soul Contentment / Thorough Cleansing.
3. Only then decide whether a Rebalanced 0.2.17 balance candidate is justified.



## 2026-09-23 BSS-family and Donations design directions

Status: **design hypotheses; no production change**.

### BSS unlock timing

Direct project evidence: `b_souls`, `b_grat_points_incr`, and `b_sin_shard` are all unlocked by `soul_church_additions`. The visible paid route through `soul_sins_2 -> soul_church_additions` is approximately **35R + 65G + 10 Soul Gratitude** before prayer crafting cost.

Therefore these are not opening-game prayers, but their Bronze tiers still appear as soon as the shared BSS prayer technology is reached. Raising every Thorough Cleansing tier to q90+ would create a poor unlock/readiness mismatch.

### Thorough Cleansing: accepted design direction

User accepted the following direction on 2026-09-23:
- Bronze: preserve the recognizable stock/Rebalanced core at **x2 Sin Shards**;
- Silver: **x3**;
- Gold: **x4**;
- requirements: **q30 / q60 / q120**;
- preserve existing durations initially: **36 / 72 / 108 min**.

Why this fits the Rebalanced philosophy:
- no new resource or role is invented;
- Bronze preserves the familiar BSS behavior instead of making the prayer unusable when first unlocked;
- higher prayer quality finally improves magnitude instead of duration only;
- q120 moves the aspirational “miracle” gate from the weak Faith-only Soul's Repose proposition to the BSS prayer whose resource has a legitimate long-tail need;
- x4 remains a throughput accelerator, not an automatic perfect-corpse generator: a fully healed soul that would naturally yield 4-7 shards would yield 16-28, while perfect-body workflows commonly consume roughly 30-40+ shards per corpse.

Do not jump directly to x3/x4/x5 with q90/q120/q150 unless evidence shows x2/x3/x4 at q30/q60/q120 is still insufficient. q150 in particular risks turning the prayer into candle/incense maintenance rather than an attractive miracle.

### Soul Contentment: progression tool, not endgame pillar

Current role: **+20% Soul Gratitude** at all tiers, with quality increasing duration.

Community signals are mixed:
- some players actively use Remote Craft Control and therefore continue spending Gratitude;
- others explicitly say they do not use remote crafting or ask what accumulated Gratitude is for.

This supports treating Soul Contentment as a **BSS progression / remote-crafting specialist** that may naturally become obsolete.

If reopened, prefer same-role magnitude progression rather than a new mechanic. Candidate shape to research:
- q20 / q40 / q60 unchanged;
- Bronze +20%;
- Silver/Gold gain larger Gratitude multipliers while keeping the same core effect;
- exact values not yet selected.

Do not balance PrayerClarity around external mods that create new Gratitude sinks.

### Soul's Repose: remove false capstone status before inventing a new role

The q120 Faith-only capstone is not supported by the Game Needs Matrix.

Preferred conservative direction if PrayerClarity should remain a rebalance rather than an overhaul:
- keep the prayer's identity as **Faith strengthened by Soul Gratitude**;
- remove the q120 aspirational status (candidate Gold gate around q90-q95);
- investigate replacing the Souls base `(CQ + SG) * 0.1` with the ordinary Faith base plus a positive Soul-Gratitude contribution, so the prayer does not first halve the normal church contribution before adding Soul value;
- retain the existing +50/+100/+150% tier concept unless the new base still leaves the specialist dominated.

This keeps the same output, same lore and same decision category. Adding Sin-Shard multipliers, Soul-Gratitude payouts or unrelated BSS effects should be considered only if this conservative repair still fails; those changes move toward an overhaul and risk colliding with Soul Contentment / Thorough Cleansing.

### Donations: accepted progression-specialist direction

Current +5/+15/+30 silver competes for the weekly sermon slot during the exact phase when Faith is a severe bottleneck.

Recent player evidence shows ordinary early money routes can yield about 10-20 silver in a day-scale selling session (coal/stone/firewood) before vendor liquidity/deflation slows the loop.

Relevant fixed money gates include:
- Building Permission: 20 silver;
- Trade License: 50 silver;
- Aristocrat status later: 12 gold.

Therefore +5 silver once per week is not a credible alternative to an extra +5 Faith for many early players.

Three flat-payout candidate bands:
- **15 / 40 / 80 silver** — conservative;
- **20 / 50 / 100 silver** — strong progression specialist;
- **30 / 60 / 90 silver** — deliberately front-loaded, strongest early alternative.

User accepted **20 / 50 / 100 silver** at q20/q40/q60 on 2026-09-23.

Rationale:
- Bronze roughly replaces one meaningful early selling excursion rather than several days of economy;
- Silver can pay a 50-silver progression purchase and feels materially different from Bronze;
- Gold reaches 1 gold per successful weekly sermon, but remains below mature Trade Office/tavern income and would still require many weeks to pay the 12-gold Aristocrat gate by itself;
- the ladder deliberately becomes obsolete once business automation solves money scarcity.

The user's 30/60/90 proposal remains plausible, but its Bronze tier risks collapsing the first money gates while Gold has a smaller relative quality jump. Quantitative stage-specific testing should compare these two ladders before implementation.


### Soul's Repose: Soul Gratitude -> Faith exchange hypothesis

The user proposed replacing the opaque current Souls-Faith scaling with a direct resource exchange: Soul Gratitude is spent during the sermon and produces Faith.

This direction has several strong system/UX properties:
- preserves the prayer's existing thematic input (Soul Gratitude) and output (Faith);
- removes the hidden two-input `Church Quality + Soul Gratitude` payout arithmetic;
- creates a real sink for current Soul Gratitude instead of merely rewarding hoarding;
- creates an explicit opportunity-cost choice against Remote Craft Control, which already spends Soul Gratitude;
- gives Soul Contentment a stronger ecosystem role because faster Gratitude generation can now replenish a resource the player deliberately spends;
- keeps the prayer inside Rebalanced territory rather than inventing an unrelated BSS effect.

Direct 1.407 Soul Gratitude generation is:
`GP_base = 5 * effective_durability + 5 * sins_count`.
With a fully preserved/healed seven-sin soul this reaches **40 Gratitude**. Current Rebalanced Soul Contentment (+20%) raises that case to **48**.

Preferred concrete candidate:
- ordinary sermon base Faith remains intact and uses the normal church contribution;
- successful Soul's Repose adds a **1:1 conversion bonus**: one Soul Gratitude spent -> one additional Faith;
- conversion is capped by prayer quality to prevent extreme/high-capacity stockpiles from becoming an unbounded Faith exploit;
- Bronze: convert up to **30 SG -> +30 Faith**;
- Silver: up to **60 SG -> +60 Faith**;
- Gold: up to **90 SG -> +90 Faith**;
- candidate Church Quality requirements: **q30 / q60 / q90**;
- consume only the amount actually converted; excess Gratitude remains;
- on sermon failure, **consume no Soul Gratitude** and deliver only the normal base sermon result.

The 30/60/90 cap is intentionally simple: the same tier number can describe both the success gate and the maximum exchange, and the player-facing rule remains “1 Gratitude = 1 Faith”.

At Gold, two perfect souls naturally yield 80 SG and do not fully refill a 90-point exchange. Under the current +20% Soul Contentment effect, two such souls yield 96 SG, which **does** fully refill Gold Soul's Repose. This creates a concrete synergy without changing Soul Contentment first.

UX requirement:
- before sermon use, show the exact amount that will be spent and the exact prayer-owned Faith conversion, e.g. **“Soul Gratitude: 73 -> +73 Faith”**;
- make clear that the conversion is success-only and does not reduce Soul Gratitude capacity;
- never silently consume the player's full stored pool;
- do not use an uncapped “convert all current Gratitude” rule because legitimate/quirky capacity expansion could produce extreme payouts and because wiping an RCC reserve would be hostile UX.

Stage fit:
Better Save Soul is not structurally an endgame-only system. The Spiritualism tree can be entered around early church/morgue progression, and the prayer technology sits only a few nodes into that tree. Therefore this prayer can function as a **BSS-to-core-progression bridge** while Faith is still scarce. If a player postpones BSS until the main game is nearly complete, the prayer may naturally have low value; that is acceptable for a progression specialist and is preferable to forcing an artificial endgame role.

This proposal is still a **design hypothesis**, not accepted production behavior.
