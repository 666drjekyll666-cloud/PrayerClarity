# PrayerClarity: Rebalanced — fairness / power-cost audit

Status: **research reopened by direct player feedback, 2026-09-18**. No production values are changed by this document. PrayerClarity: Rebalanced 0.1.5 remains the accepted stable runtime baseline until a new candidate is explicitly designed, implemented, tested and accepted.

## Trigger

Direct player/runtime feedback after using the accepted Rebalanced roster:

- the redesigned prayers feel substantially more useful, exciting and powerful than their stock versions;
- this is partly the intended result: the old roster contained weak, opaque, disconnected or dominated choices;
- the new concern is not "make them weaker again", but avoid the product reading as an arbitrary overpowered mod;
- especially Gold quality should feel **earned, interesting and fair**, not simply like free vertical power.

This feedback reopens the **fairness/power-budget question**, not the already-verified mechanics or implementation seams.

## Product goal

Priority order:

1. **Interesting / fun**
2. Powerful and satisfying progression
3. Fair / reasonable relationship between power and its cost
4. Preserve meaningful prayer choice and subsystem progression
5. Avoid grind or maintenance chores whose only purpose is to "pay a tax"

Balance is therefore not numerical equality. A narrow or late prayer may be intentionally very strong. The failure condition is a prayer that becomes an obvious dominant answer, trivializes the subsystem it touches, or receives a major vertical upgrade without a commensurate progression/opportunity gate.

## Current accepted roster — power delta pressure

The accepted rules remain canonical in `PRAYER_REBALANCE_OPTIONS.md`.

### Highest pressure

**Faith / Donations / BSS Soul's Repose**

- Stock Gold proportional bonus: +150%, i.e. 2.5x the relevant base before fixed outputs.
- Rebalanced Gold proportional bonus: +400%, i.e. 5x the relevant base.
- The Rebalanced proportional result is therefore roughly **2x the stock proportional total** at Gold before fixed-output differences.
- Rebalanced requirements already rose to q 25/40/70.
- These are reusable resource engines with no natural hard cap, so their power increase deserves the strongest access gate scrutiny.

**Combat**

- Stock Retribution: +5 damage.
- Stock Protection: +4 armor as a separate prayer.
- Rebalanced Gold Combat: +15 damage, +4 armor and 4 HP/s regeneration in one package.
- q remains 10/20/40.
- The package is intentionally narrow and deeply unlocked, but the power increase is large while the sermon-success gate did not increase. This is a major fairness-audit target.

**Excellence**

- Stock magnitude: +0.2 linked-craft quality at all tiers.
- Rebalanced: +0.2 / +0.5 / +1.0.
- Gold magnitude is **5x stock** while q remains 10/40/60.
- Natural craft-quality ceilings limit abuse, but Gold is a major vertical upgrade and should be evaluated as an aspirational tier.

### Medium pressure / strong but bounded

**Shoots & Roots**

- Stock 1.407 special path is disconnected; intended-looking term is -20%.
- Rebalanced repairs the scope and scales to -20/-30/-40%.
- q remains 10/20/30.
- Gold is 2x the recoverable intended magnitude, but the effect is farming-specific and time-limited. Raising Gold too far into late game could destroy the prayer's progression role.

**Repentance**

- Stock special effect has no detected consumer.
- Rebalanced becomes 50/75/100% daily confession probability.
- q remains 10/20/40.
- Relative uplift is necessarily huge because stock is effectively inert, but the effect is interaction-heavy, short-window and confessional-specific.

**Soul Contentment**

- Stock +10% Soul Gratitude -> Rebalanced +20%.
- q remains 10/20/30.
- Magnitude doubles, but the prayer is DLC-gated, workflow-specific and duration-limited.

### Lower pressure / naturally bounded or mostly stock

- Repose: same progression ceiling; premium quality primarily buys reliability.
- Imagination: core +0.7 remains stock; premium tiers add thematic Story returns.
- Thorough Cleansing: x2 magnitude remains stock; quality buys duration.
- Prosperity: stock.
- Combo: stock.
- Ordinary: stock.

## Church Quality as a balancing resource

### Direct project fact

Sermon success probability is:

`RoundToInt(current Church Quality / prayer requirement * 100)`, clamped to 0..100.

Therefore `q` is a **100%-success threshold, not a hard lock**. Raising q:

- preserves the ability to attempt the prayer early;
- converts early use into risk/reward;
- lets candles/incense act as optional accelerants;
- eventually becomes free of recurring preparation once permanent Church Quality catches up.

This makes q unusually attractive as a balance lever: it is native, visible, understandable and already integrated with the prayer decision.

### External/current church progression evidence

External current references strongly support the following working anchors:

- Cathedral unlock progression requires Church Quality 50.
- A normal late-game passive Cathedral layout retaining useful confessionals is commonly reported at **94** Church Quality.
- An absolute passive maximum of **96** is reported by replacing the two Confessional II placements (+7 each) with higher-passive Stone Church Shrines (+8 each).
- The wiki currently lists an extreme temporary maximum of 261 using a candelabra-heavy layout and 60 top candles.
- Community evidence also distinguishes a passive layout from an "active" consumable-heavy layout.

Sources:

- https://graveyardkeeper.fandom.com/wiki/Church
- https://steamcommunity.com/app/599140/discussions/5/3196992771951465239/
- https://steamcommunity.com/app/599140/discussions/0/1736589519998595071/
- https://steamcommunity.com/app/599140/discussions/0/4351113819081639981/
- https://www.reddit.com/r/GraveyardKeeper/comments/1ld5xfo/max_quality_church/

**Evidence classification:** useful strong external progression evidence, but the exact passive 94/96 ceiling has not yet been established by the project's own direct 1.407 GameBalance/runtime inspection. Before a production balance change that depends critically on the exact ceiling, verify the relevant 1.407 furniture/slot data directly. A candidate capped at q <= 90 is not sensitive to the 94-vs-96 distinction.

### Requirement bands against the passive Cathedral

Using 94 as the practical passive anchor and 96 as the absolute passive anchor:

| q | Share of practical 94 | Permanent headroom to 94 | Design reading |
| ---: | ---: | ---: | --- |
| 30 | 32% | 64 | early / low gate |
| 40 | 43% | 54 | early-mid gate |
| 50 | 53% | 44 | Cathedral-upgrade threshold region |
| 60 | 64% | 34 | vanilla top-end Gold requirement |
| 70 | 74% | 24 | current Rebalanced premium |
| 80 | 85% | 14 | mature Cathedral |
| 90 | 96% | 4 | near-max passive Cathedral / aspirational Gold |
| 100 | 106% | -6 | recurring temporary boost required for certainty in a practical passive layout |

The current stock roster peaks at **q60**. Rebalanced currently peaks at **q70**.

### What higher q actually feels like

Approximate success chance before the automatic-success clamp:

| Current CQ | q70 | q80 | q90 | q100 |
| ---: | ---: | ---: | ---: | ---: |
| 50 | 71% | 63% | 56% | 50% |
| 60 | 86% | 75% | 67% | 60% |
| 70 | 100% | 88% | 78% | 70% |
| 80 | 100% | 100% | 89% | 80% |
| 90 | 100% | 100% | 100% | 90% |
| 94 | 100% | 100% | 100% | 94% |

This is why q80-q90 is a promising design space: it can make Gold feel earned and make temporary church boosts useful earlier **without** turning candles/incense into permanent weekly chores in a completed Cathedral.

q100+ is qualitatively different: it deliberately creates a recurring preparation requirement for certainty. That may be appropriate for a special "ritual" design, but it conflicts with the current fun-first direction if applied broadly.

## Why Church Quality is preferable to several other "price" levers

### 1. Prayer recipe cost

Existing recipe cost matters for acquisition and quality production, especially Chapter vs Hard Book, but the prayer is reusable. More Faith/materials therefore create a **one-time sunk cost**, not a continuing strategic price.

Useful for progression timing; weak for balancing long-term weekly power.

### 2. Technology cost / later unlock

Also a one-time progression gate. Moving prayers deeper can destroy stage-specific roles (especially Roots or Repentance) and would be more invasive than adjusting the existing sermon-success system.

Use only if the prayer is conceptually unlocked at the wrong stage, not merely because its Gold tier is strong.

### 3. New recurring consumable cost

This would be a strong ongoing price, but risks becoming friction/grind and would invent a new payment system.

Prefer the game's existing candle/incense -> Church Quality relationship if a temporary preparation cost is desired.

### 4. Magnitude nerf

Always available, but it attacks the very improvement the Rebalanced edition was designed to deliver. Use only when an effect trivializes its subsystem or makes another prayer irrelevant even after fair access costs.

### 5. Duration

A strong native lever for timed prayers. Keeping a dramatic magnitude but shortening the usable window can preserve "power fantasy" while demanding planning.

However, current quality progression deliberately uses duration as a premium axis for several prayers. Avoid flattening quality progression merely to solve an unrelated fairness concern.

### 6. Scope / cap / reliability

Often better than raw nerfs:

- Repose already uses a progression ceiling and converts premium quality into reliability.
- Excellence has natural finite craft-quality ceilings.
- Niche prayers can remain very strong if their useful scope stays narrow.
- A strong specialist is acceptable when it is clearly not the universal answer.

### 7. Weekly opportunity cost

Every sermon already displaces every other sermon for that week. This is a real recurring price and should receive more design credit than a one-time crafting cost.

The key test is therefore not "is Gold numerically large?" but "does this prayer become the obvious weekly choice even when the player has another concrete goal?"

## Design literature cross-check

The direction above is consistent with recurring professional design principles:

- Sid Meier, **Interesting Decisions** (GDC 2012): gameplay quality depends on meaningful decisions, risk/reward, useful information and removal of choices that are not interesting.
  - https://www.gdcvault.com/play/1015756/
- Mark Rosewater, **Why So Many Restrictions?**: restrictions are a core design tool; limiting when/how an effect can be used lets designers safely make effects larger and shape the desired play pattern.
  - https://magic.wizards.com/en/news/making-magic/why-so-many-restrictions
- Game Developer, **The 3 Key Factors of Balance in Game Design**: power, utility and cost must be evaluated together; stronger effects can justify higher costs, but over-costing creates another false choice.
  - https://www.gamedeveloper.com/design/the-3-key-factors-of-balance-in-game-design
- Game Developer, **Balancing Multiplayer Games — Opportunity, Power and Relativity**: opportunity cost can be time, delayed advantage or strategic position; expensive/powerful options can be healthy when their cost produces a meaningful decision.
  - https://www.gamedeveloper.com/design/balancing-multiplayer-games-opportunity-power-and-relativity
- Jesse Schell, **The Art of Game Design / Lens of Meaningful Choice**: explicitly asks whether a design has dominant strategies and whether its choices are meaningful.

These are design frameworks, not mathematical proof of one correct q table.

## Candidate balance philosophies

### A — Conservative requirement correction

Preserve every effect magnitude. Raise only Gold requirements on the most vertically amplified prayers.

Intent:

- minimal disturbance;
- keep Bronze/Silver progression roles;
- make Gold feel more earned;
- no permanent consumable chore.

Typical target band: q70-q80.

### B — Aspirational Gold

Treat the strongest Gold prayers as near-complete-Cathedral rewards.

Intent:

- Gold remains spectacular;
- early Gold use is possible but risky;
- candles/incense can bridge the gap;
- permanent late-game certainty remains available.

Typical target band: q80-q90.

This best matches the current user feedback.

### C — Mixed-lever fairness pass

Use q only where it is thematically/progression appropriate, and use natural bounds elsewhere:

- economic specialists -> higher q;
- Combat -> q plus narrow duration/scope review;
- Excellence -> q plus natural quality cap;
- Roots/Repentance -> preserve earlier q so their stage role is not lost;
- Repose -> rely on ceiling/reliability design;
- BSS duration specialists -> rely on DLC gate + weekly opportunity + duration.

This avoids the false symmetry of giving every Gold prayer the same late-game requirement.

## First-pass pressure recommendation

**Research recommendation, not accepted values:**

- Faith / Donations / BSS Soul's Repose: strongest candidates for **Gold q80-q90**.
- Excellence: candidate for **Gold q80**.
- Combat: candidate for **Gold q60-q70**; its deep unlock and narrow activity already pay part of the cost.
- Roots: at most a modest Gold increase, roughly **q40-q50**, to preserve farming-stage relevance.
- Repentance: at most **q50-q60**; the short, interaction-heavy effect is already self-limiting.
- Repose: q50 may already be fair; q60 is the highest obvious correction before the finite progression window becomes too late.
- Imagination: likely keep q60 unless the Gold Story return proves self-sustaining enough to dominate writing progression.
- Soul Contentment: likely leave low/moderate because of DLC/workflow scope; test before raising.
- Thorough Cleansing / Prosperity / Combo / Ordinary: no current fairness reason to change.

## Narrow next research gate

Do **not** change production code yet.

Before choosing exact values:

1. verify permanent Church Quality furniture/slot ceiling directly for 1.407 if we intend to anchor requirements near q90+;
2. model success probabilities at representative CQ 30/50/60/70/80/90 for candidate q ladders;
3. for each prayer classify whether the concern is:
   - subsystem trivialization,
   - dominant weekly choice,
   - progression timing,
   - or merely a satisfying visible power increase;
4. prefer a small set of targeted q changes over global normalization;
5. only if q cannot create a fair/interesting decision without making the prayer late or tedious, reopen magnitude/duration/scope.

No hosted CI is justified for this research/documentation stage.


## Combo benchmark — two-week specialist rotation

The community/meta concern is directly corroborated by external player guidance:

- one long-running guide describes Combo as the default best prayer and says players will likely use it for the rest of the game once available;
- a Steam discussion reaches the same structural conclusion: once Book quality and Church Quality stop being barriers, there is little reason to use the separate Faith/Donations prayers;
- Reddit advice likewise commonly recommends Combo every service.

Community references:

- https://www.lostnoob.com/graveyard-keeper/prayer/
- https://steamcommunity.com/app/599140/discussions/0/1734336452596992977/
- https://www.reddit.com/r/GraveyardKeeper/comments/116k5jo

This is **community/meta evidence**, not mechanics authority. The direct project evidence already proves why the pressure exists: stock Combo has the same per-resource percentage coefficients as the stock specialists at equal quality, while adding the other resource at the same time.

### Correct comparison horizon

A one-sermon comparison exaggerates the apparent size of the Rebalanced specialist premium.

The fair strategic benchmark is:

- Week 1: Faith specialist;
- Week 2: Donations specialist;

versus:

- Week 1: Combo;
- Week 2: Combo.

Let:

- `F` = ordinary base Faith for one sermon;
- `D` = ordinary base donations for one sermon;
- `pC` = Combo success probability;
- `pS` = specialist success probability.

For proportional output only, when all sermons are guaranteed:

| Quality | Combo coefficient per resource | Specialist coefficient in target week | Two Combo weeks | Faith+Donations rotation | Rotation premium before flat outputs |
| --- | ---: | ---: | ---: | ---: | ---: |
| Bronze | +50% | +200% | 3.0 base units | 4.0 base units | +33% |
| Silver | +100% | +300% | 4.0 base units | 5.0 base units | +25% |
| Gold | +150% | +400% | 5.0 base units | 6.0 base units | +20% |

This is the central interpretation:

> Rebalanced Gold's visible `+400%` is not a 2x two-week advantage over Gold Combo. Because the specialist only boosts one resource in one of the two weeks, the long-horizon proportional advantage is **6 base units versus 5**, i.e. about **+20%** before flat-output effects.

The specialist premium therefore becomes **less relatively dominant** as quality rises: +33% Bronze -> +25% Silver -> +20% Gold.

### Flat outputs further protect Combo

Stock Combo also carries success-only flat Faith and money outputs. The accepted Rebalanced Faith specialist removes its flat prayer-owned outputs; Donations retains only its thematic fixed money floor.

Using the stock tier pattern of +1/+2/+3 flat Faith and +1/+2/+3 silver as the working cross-check, guaranteed two-week totals become:

| Quality | Two Combo — Faith | Rotation — Faith | Rotation wins when | Two Combo — donations | Rotation — donations | Rotation wins when |
| --- | ---: | ---: | --- | ---: | ---: | --- |
| Bronze | `3F + 2` | `4F` | `F > 2` | `3D + 2` | `4D + 1` | `D > 1` |
| Silver | `4F + 4` | `5F` | `F > 4` | `4D + 4` | `5D + 2` | `D > 2` |
| Gold | `5F + 6` | `6F` | `F > 6` | `5D + 6` | `6D + 3` | `D > 3` |

The exact flat-output values should remain subordinate to direct balance/runtime evidence if this benchmark later drives production. The structural conclusion does not depend on them: flat outputs make Combo **better at low base values**, which is desirable progression behavior.

### Current accepted q already creates a progression crossover

Representative expected-value model:

- no Eloquence;
- no Cardinal;
- current accepted requirements;
- stock success formula including rounding;
- flat-output pattern above.

For the Faith side of the two-week benchmark:

| Quality | Combo q | Specialist q | Approx. CQ where rotation first exceeds two Combo sermons |
| --- | ---: | ---: | ---: |
| Bronze | 15 | 25 | ~20 |
| Silver | 30 | 40 | ~35 |
| Gold | 60 | 70 | ~62 |

This is a healthy shape:

- at the Combo guarantee threshold, Combo generally remains the safer/better aggregate choice;
- as Church Quality approaches the harder specialist threshold, the rotation catches up;
- once the specialist threshold is comfortably met, deliberate specialization wins by a moderate amount.

Eloquence increases base Faith while Combo's flat Faith remains fixed, so it moves the Faith-specialist crossover somewhat earlier. Cardinal similarly increases the donation base and makes Donations specialization more attractive.

### Donation side depends on Graveyard Quality as well as Church Quality

Without Cardinal, `D = 0.03 * GQ`.

At the current accepted Gold requirements:

- at CQ 60, Gold Combo is guaranteed but Gold specialists are ~86% success; Donations rotation only overtakes two Combo sermons at roughly **GQ 260**;
- at CQ 65, the crossover is roughly **GQ 149**;
- at CQ 70, both are guaranteed and the crossover falls to roughly **GQ 101**.

This is especially useful design behavior because it creates a genuine state-dependent choice instead of a universal replacement.

## Gold requirement sensitivity — Combo-meta constraint

The earlier fairness pass considered q80-q90 for the strongest Gold specialists. The two-week Combo benchmark changes the recommendation.

For Gold Faith, using the same representative assumptions:

| Specialist Gold q | Approx. CQ where Faith->Donations rotation first beats two Gold Combo sermons |
| ---: | ---: |
| 70 (current) | ~62 |
| 75 | ~65 |
| 80 | ~69 |
| 85 | ~73 |
| 90 | ~77 |

Donation specialization is stricter because of Combo's second flat money payout. Approximate Graveyard Quality needed for the rotation to beat two Gold Combo sermons:

| Specialist Gold q | At CQ 70 | At CQ 75 | At CQ 80 | At CQ 90 |
| ---: | ---: | ---: | ---: | ---: |
| 70 | ~101 | ~101 | ~101 | ~101 |
| 75 | ~149 | ~101 | ~101 | ~101 |
| 80 | ~216 | ~140 | ~101 | ~101 |
| 85 | ~422 | ~216 | ~140 | ~101 |
| 90 | ~1017 | ~366 | ~199 | ~101 |

These values assume no Cardinal; Cardinal lowers the required Graveyard Quality.

### Updated implication

q90 is no longer the default leading idea for Faith/Donations.

It makes Gold feel highly earned, but it also preserves the old Combo dominance for too much of the Cathedral progression. That conflicts with the product goal that players should **notice that Combo is no longer the automatic answer**.

The current strongest candidate band is therefore **q80-q85**, with **q80** the clean leading value:

- Gold Combo remains easier and fully reliable from CQ 60;
- around CQ 70, specialist rotation becomes roughly competitive rather than clearly superior;
- by CQ 80, the player has earned a clear but modest specialist advantage;
- a mature passive Cathedral still guarantees the specialist without recurring consumable chores.

This is a better strategic story than q90 if dethroning Combo's universal-meta status is a first-class goal.

## Power / Cost map — first concrete pass

This table is a research map, not an accepted rebalance.

| Prayer / family | Power increase vs stock | Existing meaningful costs / bounds | Dominant-choice risk | Best balance lever | Current direction |
| --- | --- | --- | --- | --- | --- |
| **Combo** | none | Hard Book +7 Faith; q15/30/60 | **stock meta anchor** | none | **Do not nerf.** Preserve as convenient generalist and reference point. |
| **Faith** | very high targeted resource multiplier | Chapter +5 Faith; higher q; loses off-theme/fixed prayer bonuses; one weekly slot | high if too cheap, but two-week premium is only moderate | Church Quality | Keep Bronze/Silver. Gold **q80 leading**, q85 alternate. |
| **Donations** | very high targeted resource multiplier | Chapter +5 Faith; higher q; only one fixed-money floor over two-week rotation; depends on GQ; one weekly slot | medium-high and strongly state-dependent | Church Quality + natural GQ scaling | Keep Bronze/Silver. Gold **q80 leading**, because q90 preserves Combo too long. |
| **BSS Soul's Repose** | same specialist multiplier plus Soul-Gratitude-dependent base | DLC progression; Chapter +5 Faith +2 Sin Shards; GP state dependence; one weekly slot | medium-high at high GP | Church Quality + existing GP state | Likely align Gold near **q80**, not q90, unless direct output modeling shows runaway Faith. |
| **Combat** | major: merged offense+defense+regen | deep smithing route; Hard Book +7 Faith; combat-only use; weekly slot; no resource-engine scaling | medium | modest q increase, scope already narrow | Gold likely **q60-ish**, maybe 70; do not price it like an uncapped resource engine. |
| **Excellence** | Gold magnitude 5x stock (+0.2 -> +1.0) | late routes; Hard Book +7 Faith; finite quality ceilings; useful only around specific crafts | medium | q plus natural cap | Gold **q70-80** candidate; likely does not need magnitude nerf first. |
| **Shoots & Roots** | repairs inert stock path; Gold doubles recoverable -20% term to -40% | farming-only; finite duration; Chapter prayer; opportunity cost | low-medium | stage relevance; possibly small q rise | **Do not late-game-gate it.** q30 may already be defensible; q40 max obvious test. |
| **Repentance** | inert stock special -> 50/75/100% daily confession chance | requires confessionals and repeated interaction; short effect; Chapter; weekly slot | low-medium | interaction burden / modest q | q40 may already be fair; q50 is a conservative test, not an automatic need. |
| **Repose** | reliability increase, not ceiling increase | Hard Book; finite corpse progression window; natural obsolescence; ceiling stays max+1 | low | existing ceiling/reliability | **Keep q50** unless runtime play shows certainty arrives too cheaply. |
| **Imagination** | core +0.7 unchanged; premium adds Story return | Hard Book; q60; writing-only window; Story reward feeds but does not replace full book chain | low-medium | existing q / reward audit | **Keep q60 provisionally.** Check whether 3 Gold Stories create a self-sustaining loop before changing. |
| **Soul Contentment** | +10% -> +20% | DLC; Soul workflow; Chapter+Faith+Sin Shards; duration is premium axis | low-medium | existing scope/duration | Probably keep current q unless actual Soul throughput proves excessive. |
| **Thorough Cleansing** | magnitude remains stock x2; quality buys duration | DLC; Soul-healing workflow; weekly slot | low | existing scope/duration | **No balance change indicated.** |
| **Prosperity** | stock | progression-limited Merchant use; naturally becomes obsolete | low | natural obsolescence | **No change.** |
| **Ordinary** | stock | starter baseline | none | none | **No change.** |

## Updated balance principle

The resource-specialist family should satisfy all of these simultaneously:

1. **Combo remains the easiest broad answer.**
2. At lower Church Quality, Combo's reliability and flat outputs can make it objectively better.
3. A specialist is clearly best when the player urgently wants its one resource.
4. Over a planned two-week Faith+Donations rotation, specialists should eventually beat two Combo sermons, but only **modestly**, not by an overwhelming margin.
5. That crossover should occur during meaningful Cathedral progression, not only after the church is effectively complete.
6. The Gold specialist should feel earned, but not so late that the player never gets to enjoy the alternative before resource scarcity has already disappeared.

This argues strongly for treating **the two-week Combo benchmark as the acceptance test for all future Faith/Donations q changes**.

## Narrow next gate after this map

Before production code:

1. directly verify the 1.407 passive Church Quality ceiling only if the final candidate depends on q near the ceiling;
2. test q80 versus q85 for Gold Faith/Donations/BSS Soul's Repose using representative CQ/GQ/GP states;
3. separately model Combat and Excellence because their power is bounded differently and should not inherit the resource-specialist q by symmetry;
4. do not reopen effect magnitude unless these native cost levers fail to produce interesting decisions.

No hosted CI is justified for this research pass.


## Readability constraint — the player must not solve the balance model

New product constraint from direct player feedback:

> A good prayer choice may be validated by formulas internally, but the player should not need formulas, expected-value calculations or a two-week spreadsheet to understand the intended choice.

The target decision grammar should be legible from normal game information:

- **need Faith -> Faith specialist;**
- **need donations -> Donations specialist;**
- **need both -> Combo;**
- Church Quality / displayed success chance tells the player whether the ambitious option is currently reliable.

The internal two-week benchmark remains useful as a balance test, but it must not become required player knowledge.

### Consequence for tuning

Avoid balance that depends on narrow hidden crossovers such as "specialists are better only above CQ X and GQ Y" unless the UI makes the relevant state obvious.

Prefer **robust role ordering**:

1. a Faith specialist should plainly provide the strongest Faith result on a successful sermon;
2. a Donations specialist should plainly provide the strongest donation result;
3. Combo should plainly provide the strongest broad/balanced proposition when both resources matter;
4. perks should reinforce these roles rather than flip them;
5. q / success chance should create a visible progression/reliability tradeoff, not an invisible expected-value puzzle.

Exact math should prove that these intuitive rules remain healthy across representative states.

## Sermon-income perks — direct 1.407 mechanics

Project mechanics evidence resolves both relevant perks.

### Eloquence

Ordinary sermon base Faith:

`base_faith = CQ * 0.2 * (1 + 0.3 * E)`

Souls sermon base Faith:

`base_faith = (CQ + GP) * 0.1 * (1 + 0.3 * E)`

where `E = p_eloquence` (0/1).

Therefore Eloquence is exactly **+30% to the Faith base before sermon percentage bonuses**, subject to the normal integer rounding path.

Prayer percentage Faith bonus is later calculated from that already-modified base. Consequently Eloquence also increases the absolute gain from Faith/Combo/Souls percentage modifiers. Prayer-owned flat Faith outputs are not multiplied by Eloquence.

Example ignoring rounding:

- CQ 50 without Eloquence -> ordinary base Faith 10;
- with Eloquence -> base Faith 13.

Eloquence is a Game of Crone progression perk tied to a particular quest outcome, so core balance must not assume every player owns it.

### Cardinal

Ordinary/Souls base donation pool:

`base_money = GQ * (0.03 + 0.01 * C)`

where `C = p_cardinal` (0/1).

Therefore Cardinal changes the base coefficient from **3% of Graveyard Quality to 4%**, i.e. a **+33.33% relative increase** to base donations.

Prayer percentage donation bonuses are calculated from that larger base, so Cardinal also increases their absolute value. Prayer-owned flat money outputs are not multiplied.

Example:

- GQ 100 without Cardinal -> base donation pool 3 silver;
- with Cardinal -> 4 silver.

Cardinal is unlocked by the same **Price of Faith** technology that unlocks Prayer for Donations and Combo. Therefore it is especially relevant to the specialist-vs-Combo comparison: once that branch is unlocked, the proportional donation game is naturally more important while fixed +silver outputs become relatively less dominant.

### Balance implication

Both perks scale the **base** used by percentage prayers. They do not create a special hidden preference for Combo.

If specialist percentage > Combo percentage, Eloquence/Cardinal preserve that ordering and increase the absolute reward for specialization. This is good for readable design: late progression makes the explicit percentage choice matter more instead of introducing a new opaque rule.

## Crafting grammar — Chapter prayers versus Book prayers

### Accepted project-level cost distinction

Current direct project evidence already establishes the recipe-class split:

- Faith / Donations / Roots / Repentance / Prosperity: **Chapter + 5 Faith**;
- Combo / Repose / Retribution / Protection / Imagination / Excellence: **Book + 7 Faith**.

A Chapter itself is produced from **3 Notes**.

A Book adds another production layer: **Chapter + cover -> Book**.

This means the game already communicates an implicit class hierarchy:

- **Chapter prayer:** cheaper/easier entry;
- **Book prayer:** more advanced, more expensive, harder to quality-upgrade.

### Cover chain

Current external wiki data:

- Softcover: 2 Pigskin Paper;
- Bronze Hard Cover: Softcover + Tanning Agent + Faith;
- higher Hard Cover quality requires additional advanced materials;
- Gold Hard Cover can require 2 Gold Jewelry Details;
- a Book can be made with either a Softcover or a Hard Cover;
- better cover quality increases the resulting Book quality.

A Hard Cover is **not directly required by Combo**. Combo requires a Book. Hard Covers are one route to a better-quality Book.

The quality of a Book is driven by Chapter quality + cover quality and Book-specific quality modifiers such as Desk II/Jeweler. Current sources agree that Writer/Playwright are not Book-quality bonuses; Jeweler is the dedicated +0.7 Book-quality perk.

External references:

- https://graveyardkeeper.fandom.com/wiki/Book
- https://graveyardkeeper.fandom.com/wiki/Chapter
- https://graveyardkeeper.fandom.com/wiki/Softcover
- https://graveyardkeeper.fandom.com/wiki/Hard_cover
- https://graveyardkeeper.fandom.com/wiki/Perks

### Gold Combo does not mean "must consume a Gold Book"

Writing quality is probabilistic rather than a strict ingredient-tier lock. Current community/wiki documentation describes the general writing model as:

`result tier = craft difficulty + ingredient quality + applicable quality bonuses`

with the fractional part acting as the chance to upgrade to the next tier.

Current sources also report that most sermon crafts can receive writing-quality bonuses (Desk II / Writer / Playwright / Inspiration), while Book crafting has its own Chapter/cover/Jeweler path. Therefore a Gold Combo prayer is not conceptually equivalent to "recipe requires one Gold Book"; a sufficiently good lower-tier Book plus writing modifiers can potentially roll upward.

There is a documentation conflict between current wiki subpages over the exact sermon-quality modifiers/complexities. The project has **not yet directly audited the 1.407 prayer-crafting quality formula**. Do not make a production recipe/quality redesign depend on exact external probabilities until that path is verified directly.

### Perceived-balance finding

Even without exact probabilities, the material grammar is unambiguous:

`Chapter -> specialist`

versus

`Chapter -> Book (cover layer) -> Combo`.

Vanilla therefore gives the player an intuitive explanation for Combo's breadth/power: it is a **Book-class prayer**.

The accepted Rebalanced roster partially reverses this grammar:

- the Chapter specialists are much stronger in their target resource;
- they are cheaper to manufacture;
- but they compensate through higher Church Quality requirements.

This can be mathematically fair while still feeling **counterintuitive**, because the material cost and visible item hierarchy say "Book is premium" while the output can say "Chapter is better".

That perceived inconsistency is a real UX/balance finding, not merely an economic calculation issue.

## New design hypothesis — early specialists, premium late-game Combo

A promising alternate architecture is to restore a coherent visible progression grammar:

### Specialists

- remain **Chapter-class** prayers;
- available earlier;
- lower crafting burden;
- lower Church Quality requirements;
- genuinely strong in one resource;
- remain the obvious answer when the player has one urgent goal.

### Combo

- remains or becomes an explicitly **premium Book-class** prayer;
- materially harder/costlier to produce at high quality;
- substantially higher Church Quality requirement;
- rewards that investment with broad Faith+donation power;
- remains the natural answer when both resources matter;
- does **not** need to beat a specialist at that specialist's one resource.

This is a cleaner player mental model:

`cheap + focused + earlier`

versus

`expensive + broad + later`.

The cost of Combo is then legible without formulas: the player sees the Book, cover chain, extra Faith and higher church requirement before ever comparing expected values.

### Important distinction: signaling cost vs recurring balance cost

Recipe/material cost and Church Quality should not be treated as interchangeable.

- **Recipe/material cost** is mostly a one-time cost. It is weak at controlling infinite long-run sermon output, but extremely useful for **progression and signaling**: "this is an advanced prayer".
- **Church Quality requirement** controls reliability every week until the church matures. It is a much stronger long-run progression lever.

A coherent design may deliberately use both:

- expensive Book recipe says **premium class**;
- high q says **you are not yet ready to use this premium class reliably**;
- strong broad output says **the investment has a visible payoff**.

### Do not raise Combo q without increasing its proposition

A stock-output Combo with a dramatically higher q and higher crafting cost would simply become unattractive.

If Combo is moved into a stronger late-game/premium role, its reward proposition must be re-audited as well. The design target is not "tax Combo until specialists win"; it is:

> specialists are the best focused tools; Combo is the expensive high-progression broad tool.

Possible value-space to model next, not accepted:

- leave specialist target multipliers strong;
- raise Combo's Faith+donation multipliers enough that its **combined** output clearly justifies Book-class cost;
- keep each specialist's target output above Combo's corresponding single-resource output;
- use higher Combo q to make the broad convenience/power a later progression reward.

This could make the choice categorical rather than mathematical:

- "I need Faith" -> specialist;
- "I need money" -> specialist;
- "I need both and my church/book infrastructure is mature" -> Combo.

### q100 warning

A conceptual Combo ladder such as 60/80/100 is useful for thinking, but q100 exceeds the currently observed ~94-96 passive Cathedral ceiling. Gold would therefore require temporary Church Quality boosts for 100% reliability.

That may be an intentional "ultimate ritual" design, but it would also create recurring candle/incense preparation. Under the current fun-first policy, prefer testing ladders that keep fully developed passive certainty possible (for example candidates ending at q80-q90) before choosing q100.

## Updated next gate

The fairness audit now has two competing architectures to model:

1. **Current architecture:** Combo remains stock/easier-q; specialists pay higher q for stronger focus.
2. **Premium-Combo architecture:** specialists are earlier/cheaper focused tools; Combo becomes a later, costlier, higher-q broad tool with a correspondingly stronger combined proposition.

The second architecture has a major UX advantage: it matches the game's existing Chapter-vs-Book crafting language and makes the intended decision understandable without expected-value math.

Before production work:

1. directly verify 1.407 sermon-quality crafting probabilities/bonus owners if recipe quality becomes part of the new balance;
2. model a few explicit Combo multiplier/q ladders against the locked specialist values;
3. require the following invariant across representative states:
   - specialist wins its own resource;
   - Combo wins or strongly competes on combined/balanced value;
   - player can infer that relationship from the tooltip/recipe without calculations;
4. keep the stable Rebalanced 0.1.5 untouched until one architecture is explicitly selected.

No hosted CI is justified for this research pass.


## Emerging premium-Combo coefficient candidate — exact two-week proportional parity

A particularly clean candidate falls directly out of the locked specialist ladder.

Keep specialists:

- Bronze: +200% target resource;
- Silver: +300%;
- Gold: +400%.

Candidate Combo:

- Bronze: **+100% Faith and +100% donations**;
- Silver: **+150% / +150%**;
- Gold: **+200% / +200%**.

Ignoring flat outputs and treating `F` / `D` as their own independent base units:

| Quality | One specialist week: target / other | One Combo week: Faith / donations | Two-week specialist rotation per resource | Two Combo weeks per resource |
| --- | --- | --- | ---: | ---: |
| Bronze | 3x / 1x | 2x / 2x | 4x | 4x |
| Silver | 4x / 1x | 2.5x / 2.5x | 5x | 5x |
| Gold | 5x / 1x | 3x / 3x | 6x | 6x |

Therefore the **proportional two-week output is exactly equal at every tier** before flat outputs.

This gives an unusually readable strategic structure:

- specialist = same broad two-week power budget redistributed aggressively into one resource;
- Combo = same proportional budget distributed evenly across both resources;
- stock flat Combo outputs then give the premium Book prayer a modest total-value/convenience edge;
- specialists remain cheaper and can use lower q;
- Combo can justify higher Book-class crafting/progression requirements without needing to beat specialists at their own resource.

This is not an accepted value set yet. Base Faith and donations have different economic utility, flat outputs matter, and q/reliability must still be modeled. But as an **internal balance invariant** it is substantially cleaner than relying on narrow CQ/GQ crossover arithmetic.

Player-facing interpretation requires no two-week calculation:

- Faith specialist visibly has the largest Faith percentage;
- Donations specialist visibly has the largest donation percentage;
- Combo visibly gives two substantial percentages at once.

This candidate should be the first premium-Combo multiplier ladder modeled in the next quantitative pass.


## Community signal — church progression visibility and Combo crafting hierarchy

Current community research adds two relevant signals.

### Players do question why church progression should continue

A 2026 Steam discussion explicitly asks what Graveyard Quality does because the player cannot identify the mechanic from the game itself. In the same discussion, a near-100% player says there is little incentive to improve the graveyard beyond ~200 or the church interior beyond ~60.

A 2024 Reddit thread is explicitly titled "Should I keep improving the church? Does it make sermons more effective?" Replies explain that higher Church Quality still increases Faith even after q60 guarantees all stock prayers, but this is community knowledge rather than a strong in-game progression signal.

Other older Steam threads likewise explain that higher Church Quality continues increasing sermon Faith, often with approximate "5 CQ -> 1 Faith" guidance.

Classification: **community signal + UX finding support**, not mechanics proof. Direct project evidence already proves the underlying base-Faith formula.

Implication: higher Rebalanced q thresholds can create a new **visible progression reason** to improve the church beyond the stock q60 ceiling. This is stronger player-facing motivation than the vanilla hidden/poorly explained base-output scaling.

### Players already read Combo as a premium Book prayer

Community comparisons repeatedly describe:

- Faith/Donations as easier to craft because they need a Chapter;
- Combo as harder because it needs a Book;
- Gold Combo as requiring substantially more writing infrastructure / a high-quality Book;
- stock Combo as equal to the corresponding specialist percentages while adding the second resource.

One 2023 Reddit thread asks almost exactly "why make Prayer for Faith instead of Combo?" because Combo is objectively better at the same quality; replies point to the higher Church Quality threshold and Book production as the reason Faith is the easier entry.

A 2025 thread similarly says Gold Combo and Gold Donations produce the same donation percentage, but Combo also gives Faith; the explicit downside identified is q60 vs q50 and the difficulty of producing a sufficiently good Book.

Classification: **community signal matching direct mechanics evidence**.

Implication: the game/community already has a learned grammar of **Chapter specialist = easier entry; Book Combo = premium/generalist**. A redesign that strengthens this grammar should feel more natural than one that makes Chapter specialists the most progression-demanding prayers.

Community references:

- https://steamcommunity.com/app/599140/discussions/0/780944762959117296/
- https://www.reddit.com/r/GraveyardKeeper/comments/1cwli34
- https://www.reddit.com/r/GraveyardKeeper/comments/100eswp
- https://www.reddit.com/r/GraveyardKeeper/comments/1k3y63m

## Resource-prayer family — staged handoff candidate

The three prayers should be balanced as **one competitive family**:

- Prayer for Faith;
- Prayer for Donations;
- Combo Prayer.

The new design objective is a visible progression handoff rather than permanent equal-meta coexistence.

### Player-facing role grammar

Early / mid game:

- Faith specialist = strongest simple Faith injection;
- Donations specialist = strongest simple money injection;
- both are Chapter-class and comparatively easy to produce.

Late game:

- Combo = premium Book-class generalist;
- no flat outputs;
- strong percentage scaling on both resources;
- higher Church Quality requirements;
- eventually becomes the natural universal late-game sermon.

Natural specialist obsolescence is acceptable here because it is a deliberate progression arc, not accidental domination.

### Candidate A — clean flat-to-percent handoff

**Research hypothesis only; not accepted.**

Specialist requirements:

- Bronze q20
- Silver q40
- Gold q60

Combo requirements:

- Bronze q40
- Silver q60
- Gold q80

This creates the requested one-tier progression relationship:

- specialist Silver and Combo Bronze become guaranteed together at CQ40;
- specialist Gold and Combo Silver become guaranteed together at CQ60;
- Combo Gold becomes the final premium step at CQ80.

#### Prayer for Faith

Shared percentage across all qualities:

- **+50% Faith**

Tier flat Faith:

- Bronze **+5 Faith**
- Silver **+10 Faith**
- Gold **+15 Faith**

No prayer-owned donation percentage or flat donation reward. Normal base donations remain.

#### Prayer for Donations

Shared percentage across all qualities:

- **+50% donations**

Tier flat money:

- Bronze **+2 silver**
- Silver **+4 silver**
- Gold **+6 silver**

No prayer-owned Faith percentage or flat Faith reward. Normal base Faith remains.

#### Combo Prayer

No prayer-owned flat Faith or money.

Percentage bonuses:

- Bronze **+100% Faith / +100% donations**
- Silver **+150% / +150%**
- Gold **+200% / +200%**

Recipe class remains Book +7 Faith unless later evidence shows an additional material tax is required.

### Why these numbers form a coherent handoff

Let `F` be base Faith and `D` be base donations.

Faith specialist versus same-quality Combo crosses over when:

- Bronze: `1.5F + 5 = 2F` -> `F = 10`;
- Silver: `1.5F + 10 = 2.5F` -> `F = 10`;
- Gold: `1.5F + 15 = 3F` -> `F = 10`.

Donation specialist similarly crosses at:

- Bronze: `1.5D + 2 = 2D` -> `D = 4 silver`;
- Silver: `1.5D + 4 = 2.5D` -> `D = 4 silver`;
- Gold: `1.5D + 6 = 3D` -> `D = 4 silver`.

Thus the tier ladder changes **access and total power** without moving the underlying economic handoff point.

Those base thresholds map naturally onto progression:

- Faith base 10 = CQ50 without Eloquence, or roughly CQ39 with Eloquence;
- donation base 4 silver = GQ100 with Cardinal, or roughly GQ133 without Cardinal.

The player should never see these equations. They are internal proof that the visible design has a stable progression boundary.

### Representative stage snapshots

These are internal sanity checks, not intended tooltip content.

#### CQ20 / GQ50, no income perks

Base Faith = 4; base donations = 1.5 silver.

Guaranteed Bronze specialist:

- Faith Bronze: **11 Faith** on success;
- Donations Bronze: **4.25 silver** on success.

Bronze Combo q40 is only about 50% reliable and has no flat floor beyond the normal sermon base.

Interpretation: specialists are clearly the early tools.

#### CQ40 / GQ100, Cardinal active

Base Faith = 8 without Eloquence; donations = 4 silver.

At this point:

- Specialist Silver is guaranteed;
- Combo Bronze is guaranteed.

Outputs on success:

- Faith Silver: **22 Faith**;
- Combo Bronze: **16 Faith + 8 silver**;
- Donations Silver: **10 silver**.

Interpretation: specialists remain clearly better at their target; Combo buys breadth.

#### CQ60 / GQ150, Eloquence + Cardinal active

Base Faith = 15.6; donations = 6 silver.

At this point:

- Specialist Gold is guaranteed;
- Combo Silver is guaranteed.

Outputs on success:

- Faith Gold: **38.4 Faith**;
- Combo Silver: **39 Faith + 15 silver**;
- Donations Gold: **15 silver**.

Interpretation: this is the intended handoff point. Premium Combo Silver roughly catches Gold specialists even in their target while also producing the other resource.

#### CQ80 / GQ200, Eloquence + Cardinal active

Base Faith = 20.8; donations = 8 silver.

Gold Combo is guaranteed:

- Gold Faith specialist: **46.2 Faith**;
- Gold Donations specialist: **18 silver**;
- Gold Combo: **62.4 Faith + 24 silver**.

Interpretation: late-game Combo is now unambiguously the premium universal sermon.

### Why Candidate A is attractive

1. **No spreadsheet required for the player.**
   - Flat number visually says "strong now".
   - Percentage visually says "scales with my developed church/graveyard".
   - q ladder visibly says when each prayer belongs.

2. **Crafting grammar and power grammar agree.**
   - Chapter specialists are earlier and cheaper.
   - Book Combo is later and stronger.

3. **Church progression gains visible milestones beyond stock q60.**
   - CQ40: Combo Bronze / specialist Silver.
   - CQ60: Combo Silver / specialist Gold.
   - CQ80: Combo Gold.

4. **The meta changes over time instead of being permanently replaced.**
   - early specialist meta;
   - transitional choice;
   - late Combo meta.

5. **Natural obsolescence is deliberate.**
   The specialists eventually becoming inefficient is acceptable because they have already served a clear progression role.

### Main risks to test

- +5/+10/+15 Faith may accelerate early Faith progression too aggressively.
- +2/+4/+6 silver may matter very differently depending on when Merchant/Tavern income comes online.
- Eloquence moves the Faith crossover earlier; Cardinal moves the donation crossover earlier.
- q20/40/60 vs q40/60/80 is elegant, but exact prayer acquisition timing must be checked so Combo does not become available far earlier than its intended reliable-use stage.
- removing all Combo flat outputs changes a familiar stock presentation; this is acceptable only if the stronger percentage identity reads more clearly.
- Gold Combo at q80 must still feel worth producing given the Book quality chain.

## Current architecture comparison

Three coherent family architectures are now on the table:

### Architecture 1 — current Rebalanced 0.1.5

- specialists: very high percentages;
- specialists: harder q than Combo;
- Combo: stock;
- intended outcome: permanent focused-vs-generalist choice.

Strength: preserves Combo exactly.
Weakness: crafting hierarchy and q hierarchy point opposite directions; crossover logic is mathematically fair but not obvious.

### Architecture 2 — proportional parity premium Combo

- specialists: +200/+300/+400 target;
- Combo: +100/+150/+200 both;
- potentially higher Combo q.

Strength: specialists remain best at their own resource forever; very clean proportional power budget.
Weakness: cheaper Chapter specialists remain permanent top-end tools, so Book-class progression still does not create a full handoff.

### Architecture 3 — staged flat-to-percent handoff (Candidate A)

- specialists: low shared percentage + strong tier flat output;
- Combo: no flats + strong dual percentages;
- specialist q20/40/60;
- Combo q40/60/80.

Strength: clearest progression language and strongest alignment between crafting, q and power.
Weakness: largest mechanical departure from stock/current Rebalanced; requires careful economy modeling.

Candidate A is currently the most promising architecture for the newly stated product goal, but it is **not accepted** until the early/mid/late economy is modeled more deeply and the 1.407 sermon-quality crafting path is directly verified.


## Candidate A quantitative sanity check — power creep and early economy

This section tests the staged handoff candidate against stock 1.407 rather than against the previous Rebalanced roster.

### Stock flat-output correction

Current external sermon tables and historical runtime-aligned community calculations agree with the project semantic model: Faith, Donations and Combo all carry the same stock tier flat outputs:

- Bronze: **+1 Faith, +1 silver**;
- Silver: **+2 Faith, +2 silver**;
- Gold: **+3 Faith, +3 silver**.

The family difference is therefore primarily percentage allocation and q:

- Faith: +50/+100/+150% Faith, +20% donations, q10/20/50;
- Donations: +50/+100/+150% donations, +20% Faith, q10/20/50;
- Combo: +50/+100/+150% both, q15/30/60.

References:
- https://graveyardkeeper.fandom.com/wiki/Sermon
- https://graveyardkeeper.fandom.com/wiki/Prayer_for_faith
- https://graveyardkeeper.fandom.com/wiki/Prayer_for_donations
- https://graveyardkeeper.fandom.com/wiki/Combo_prayer

### Faith specialist — Candidate A versus stock

Candidate A:
- q20/40/60;
- +50% Faith all tiers;
- flat +5/+10/+15 Faith;
- no prayer-owned money bonus.

At the candidate guarantee thresholds and without Eloquence:

| Tier | CQ | Base Faith | Stock same-tier Faith prayer | Candidate A Faith | Delta |
| --- | ---: | ---: | ---: | ---: | ---: |
| Bronze | 20 | 4 | 7 | 11 | **+57%** |
| Silver | 40 | 8 | 18 | 22 | **+22%** |
| Gold | 60 | 12 | 33 | 33 | **0%** |

With Eloquence at the same CQ values, the candidate deltas shrink further; at Gold/CQ60 the candidate is about **8.6% below** stock Gold Faith because percentage scaling matters more.

Interpretation:

- Candidate A is intentionally a **front-loaded Faith progression buff**, not a late-game Faith power creep.
- Bronze is the only genuinely aggressive point.
- The Bronze result has a useful recoup property: at CQ20, Casual gives 6 Faith (base 4 + flat 2) while Candidate A Bronze gives 11. The +5 incremental Faith exactly recovers the specialist's 5-Faith crafting cost after one successful use, ignoring writing-material opportunity cost.
- This directly repairs a long-standing player complaint that stock Copper Faith barely improves on Casual while costing 5 Faith and a Chapter.
- Silver is a moderate uplift.
- Gold converges to stock and then scales worse than stock once Eloquence/base Faith rises.

External historical community example for stock Copper Faith at CQ~20: 7 Faith versus 6 from Casual, described as scarcely worth the crafting cost:
- https://steamcommunity.com/app/599140/discussions/0/1742231705662858611/

Current judgment: **+5/+10/+15 is strong but not obviously excessive**. If runtime feel later says Bronze accelerates research too hard, the first conservative alternate should be +4/+8/+12 rather than changing the percentage core.

### Donations specialist — Candidate A versus stock

Candidate A:
- q20/40/60;
- +50% donations all tiers;
- flat +2/+4/+6 silver;
- no prayer-owned Faith bonus.

Representative comparison:

Without Cardinal:

| GQ | Base donations | Bronze candidate vs stock | Silver candidate vs stock | Gold candidate vs stock |
| ---: | ---: | ---: | ---: | ---: |
| 50 | 1.5s | 4.25s vs 3.25s | 6.25s vs 5.0s | 8.25s vs 6.75s |
| 100 | 3.0s | 6.5s vs 5.5s | 8.5s vs 8.0s | 10.5s vs 10.5s |
| 150 | 4.5s | 8.75s vs 7.75s | 10.75s vs 11.0s | 12.75s vs 14.25s |
| 200 | 6.0s | 11.0s vs 10.0s | 13.0s vs 14.0s | 15.0s vs 18.0s |

With Cardinal, base donations are 4% of GQ instead of 3%, so the percentage-heavy stock Gold overtakes Candidate A even earlier.

Interpretation:

- Bronze Candidate A is always +1 silver above stock Bronze Donations, before considering removed off-theme Faith.
- Silver crosses stock around a 4-silver base.
- Gold crosses stock around a 3-silver base; above that, Candidate A is **weaker than stock Gold Donations** in its target resource.
- This is exactly the intended staged handoff: early flat strength, deliberately poor late scaling.

### Are +2/+4/+6 silver too small?

Not in the early game; they become small later, which is the point.

Useful economy anchors:

- one Burial Certificate sells for **1s50c** at a fixed price;
- at GQ50 without Cardinal, Candidate Bronze Donations pays about **4s25c total**, versus 1s50c base donations; the prayer's incremental money above the base is about **2s75c**, roughly the cash value of 1.8 burial certificates;
- at GQ100 with Cardinal, Candidate Silver pays **10s total**;
- Trade Office silver crates sell for about **10s75c**, gold crates about **16s50c**, and goods crates about **15s** each in current data;
- late tavern and Trade Office systems can produce much larger weekly income.

References:
- https://graveyardkeeper.fandom.com/wiki/Burial_certificate
- https://graveyardkeeper.fandom.com/wiki/Trade_Office
- https://graveyardkeeper.fandom.com/wiki/Talking_Skull
- https://steamcommunity.com/app/599140/discussions/0/5789982181537748511/

Therefore +2/+4/+6 silver is **meaningful early, intentionally modest late**. It does not look like a game-breaking cash injection.

### Combo — Candidate A versus stock

Candidate A Combo:
- q40/60/80;
- +100/+150/+200% Faith and donations;
- no flat Faith or money.

At representative no-perk stages:

- Bronze at CQ40: base Faith 8 -> Candidate 16 Faith vs stock Bronze 13 Faith (**~+23%**);
- Silver at CQ60: base Faith 12 -> Candidate 30 Faith vs stock Silver 26 Faith (**~+15%**);
- Gold at CQ80: base Faith 16 -> Candidate 48 Faith vs stock Gold 43 Faith (**~+12%**).

Representative donation bases of 4/6/8 silver give:

- Bronze: 8s vs stock 7s (**~+14%**);
- Silver: 15s vs stock 14s (**~+7%**);
- Gold: 24s vs stock 23s (**~+4%**).

The output increase is therefore modest at the stage where the higher q is intended to make the tier reliable.

Candidate A is not "everything is stronger":

- specialists are strongly better early;
- specialists flatten and can become weaker than stock late;
- Combo is moderately stronger late, but pays a much higher q ladder and Book-class production burden;
- off-theme specialist rewards are removed.

Current conclusion: **no evidence of severe whole-family power creep**. The design mostly reallocates power across progression stages.

## Full perceived-value / power-cost equation

For this family, perceived and actual prayer value should be audited across all of the following.

### Access and acquisition
1. technology unlock itself;
2. prerequisite/route depth;
3. whether the prayer is Chapter-class or Book-class;
4. raw recipe materials;
5. upfront Faith cost (5 vs 7);
6. quality-production difficulty/probability;
7. prerequisite quality infrastructure/perks (Desk II, Writer, Playwright, Jeweler, Inspiration, covers, Jewelry Details);
8. **recoup time**: how many successful uses are required before the initial Faith/material investment has paid back relative to the alternative;
9. reusability: vanilla prayer items are not consumed by the normal weekly sermon, so crafting cost is amortized over future weeks rather than paid repeatedly.

### Weekly use cost
10. Church Quality requirement;
11. actual success probability below the guarantee threshold;
12. optional candles/incense needed to bridge that threshold;
13. failure downside: base Faith/donations survive, prayer-owned success bonuses do not;
14. one-sermon-per-week opportunity cost;
15. stage timing: whether the prayer becomes reliable while its resource role is still relevant.

### Reward
16. base Faith from Church Quality;
17. base donations from Graveyard Quality;
18. flat Faith;
19. flat money;
20. percentage Faith;
21. percentage money;
22. off-theme rewards / breadth;
23. Eloquence/Cardinal scaling;
24. interaction with other buffs/perks where applicable.

### Economic context
25. current scarcity/marginal utility of Faith;
26. current scarcity/marginal utility of money;
27. substitute Faith sources (especially confessionals);
28. substitute money sources (burial certificates, farming/trading, Trade Office, tavern/DLC systems);
29. natural obsolescence of the resource need itself;
30. caps/ceilings or lack thereof.

### Player comprehension / perceived fairness
31. whether recipe cost visually matches power class;
32. whether q visibly matches progression stage;
33. whether the tooltip makes role comparison obvious without arithmetic;
34. whether a stronger option is situational or simply dominant;
35. whether the reward feels satisfying enough to justify the weekly ritual.

No major missing category is currently apparent. The most important additions were **amortization/reusability, recoup time, quality-production variance, substitute income sources, and perceived/cognitive legibility**.

## Crafting-quality path — evidence state

Accepted/direct project evidence already establishes the structural chain:

- Story -> Note;
- 3 Notes -> Chapter;
- Chapter -> Chapter-class prayer;
- Chapter + cover -> Book;
- Book -> Book-class prayer such as Combo.

Current external documentation consistently reports:

- Notes, Chapters and most sermons use a writing-quality system influenced by craft difficulty, ingredient quality, Writer (+0.3), Playwright (+0.5), Inspiration (+0.7), and Desk II;
- Book quality is a separate quality step driven by Chapter quality + cover quality, Desk II and Jeweler; Writer/Playwright do not apply to the Book craft itself;
- better Hard Covers raise Book quality; Gold Hard Cover can consume Gold Jewelry Details;
- Combo then goes through the sermon-quality step after the Book step.

References:
- https://graveyardkeeper.fandom.com/wiki/Sermon
- https://graveyardkeeper.fandom.com/wiki/Notes
- https://graveyardkeeper.fandom.com/wiki/Chapter
- https://graveyardkeeper.fandom.com/wiki/Book
- https://graveyardkeeper.fandom.com/wiki/Hard_cover
- https://graveyardkeeper.fandom.com/wiki/Perks

This proves the **extra quality bottleneck** for Book prayers, but not yet the exact 1.407 numerical probability formula.

### Narrow 1.407 runtime evidence gate

A dedicated read-only CraftingQualityProbe 0.1.0 has been created on this research line.

Purpose:
- dump the exact 1.407 CraftDefinition quality-related method signatures/IL;
- dump relevant Notes/Chapter/Book/Faith/Donations/Combo craft rows;
- resolve exact quality modifiers/probability arithmetic without mutating game state.

Build:
- source SHA: 97090a19251f972ce42559ce0c6931874bb61a85;
- frozen ref: candidate/crafting-quality-probe-0.1.0;
- Actions run: 35328131561;
- result: success;
- artifact: PrayerClarity-CraftingQualityProbe-0.1.0;
- artifact ZIP digest: sha256:75def9bf4f1d49bb222041e012ebbeed59497eff5d2b3d42abb71e3d94dbfe4c;
- DLL SHA-256: a78a54a33673af9a4fd6e12b71621745e9eed89d2aad03f0f6188a6bf3d06737.

Runtime contract:
- no Harmony;
- no balance mutation;
- no save mutation;
- automatically runs once after a loaded game reaches stable runtime state;
- writes BepInEx/PrayerClarity-crafting-quality-0.1.0.txt.

This is the only current in-game evidence request required before exact recipe-quality probabilities are used in Candidate A balance decisions.


## Follow-up — Donations viability, Combo progression, BSS hierarchy, Repentance throughput

### Donations community signal

Current community evidence does not support calling Prayer for Donations universally useless, but it does show two recurring pressures:

1. at equal quality, stock Combo gives the same donation coefficient while also giving the full Faith coefficient, so players repeatedly ask why they would ever craft/use Donations after the Book/q barrier is solved;
2. by the time advanced Merchant-crate / Tavern / other economy loops are established, several players describe sermon money as comparatively unimportant while Faith remains scarce.

Counter-signal exists: some players deliberately used Gold Donations for much of a playthrough and considered the income worthwhile.

Classification: **community signal**, not mechanics proof.

References:
- https://www.reddit.com/r/GraveyardKeeper/comments/1k3y63m
- https://www.reddit.com/r/GraveyardKeeper/comments/tjrku3
- https://www.reddit.com/r/GraveyardKeeper/comments/1d3svge
- https://www.reddit.com/r/GraveyardKeeper/comments/133z7jo
- https://steamcommunity.com/app/599140/discussions/0/1734336452591917037/
- https://steamcommunity.com/app/599140/discussions/0/1736589519992194908/

Design implication: Donations should own a **strong early/mid-game cash window**, not attempt to remain the final late-game money engine.

### Donations flat candidate B — 3 / 6 / 9 silver

Candidate A used +2/+4/+6 silver with +50% donations.

A stronger leading alternative is now:

- Bronze: **+50% + 3 silver**;
- Silver: **+50% + 6 silver**;
- Gold: **+50% + 9 silver**;
- q remains 20/40/60 for the staged-handoff candidate.

This has one useful invariant versus same-tier staged Combo:

- Bronze Combo total = 2.0D;
- Silver Combo total = 2.5D;
- Gold Combo total = 3.0D;
- Donations = 1.5D + 3/6/9.

All three same-tier crossovers occur at exactly D = 6 silver base donations.

That corresponds to approximately:

- GQ 200 without Cardinal;
- GQ 150 with Cardinal.

Below that state, Donations is best at money. Above it, Combo's scaling overtakes it.

Representative early value:

- GQ50, no Cardinal -> D=1.5s;
- Bronze Donations candidate B -> **5.25s total**.

This is meaningfully stronger than stock Bronze (3.25s) without being an economy-breaking amount.

Status: **leading design hypothesis**, not accepted.

### Combo relative-uplift question

Staged Combo currently proposes +100/+150/+200% Faith and donations.

Compared against stock Combo, the percentage uplift in total payout naturally shrinks with tier because the candidate adds a roughly constant +50 percentage points to an already-growing stock multiplier.

Ignoring flats:

- stock totals: 1.5x / 2.0x / 2.5x base;
- staged totals: 2.0x / 2.5x / 3.0x base;
- relative uplift: +33.3% / +25% / +20%.

Representative stock flat outputs reduce these percentages further, producing the previously observed ~23/~15/~12% examples.

This is **not reverse player progression**. The player-facing prayer still rises +100 -> +150 -> +200%, and each tier is stronger than the previous one.

Current design judgment: do not inflate Gold merely to keep "mod versus vanilla percentage uplift" constant. That is an internal comparison metric, not a player-facing progression rule. Reopen only if Gold Combo fails the quality-cost temptation test.

### BSS Soul's Repose must be reopened under staged handoff

The accepted 0.1.5 BSS rule was aligned to the old specialist architecture:

- +200/+300/+400% Faith;
- q25/40/70;
- base Faith = 0.1 * (CQ + Soul Gratitude) * Eloquence factor.

Under staged handoff this creates obvious double scaling: Soul Gratitude already expands the base, then the old specialist multiplier multiplies that expanded base again.

Representative no-Eloquence example:

- CQ100, GP2086 -> base Faith ~= 219;
- stock Gold Soul's Repose (+150% + fixed 3 Faith) ~= **550 Faith**;
- current Rebalanced 0.1.5 Gold (+400%, no flat) ~= **1095 Faith**.

Community runtime reports of ~500-586 Faith with very high Soul Gratitude corroborate that stock BSS already has an enormous endgame ceiling; there is no need to double it again.

References:
- https://www.reddit.com/r/GraveyardKeeper/comments/1rsjmat/
- https://www.reddit.com/r/GraveyardKeeper/comments/1gm1ckn
- https://www.reddit.com/r/GraveyardKeeper/comments/1gzskgh
- https://www.reddit.com/r/GraveyardKeeper/comments/15ys6s3

Leading new direction:

- preserve the **stock +50/+100/+150% Faith ladder** provisionally;
- let Soul Gratitude itself be the special scaling axis;
- remove off-theme outputs if the staged family cleanup still calls for it;
- then choose q based on DLC/Soul-room progression rather than copying ordinary Faith specialist q by symmetry.

With stock BSS percentages against Candidate-A ordinary Faith (+50% +5/+10/+15), no-Eloquence crossover Soul Gratitude at the candidate guarantee CQs is approximately:

- Bronze at CQ20: GP ~53;
- Silver at CQ40: GP ~70;
- Gold at CQ60: GP ~72.

That is a much healthier state-dependent progression story: ordinary Faith wins before serious Soul-room investment; Soul's Repose becomes the strongest Faith engine once the player has actually built up Soul Gratitude.

Status: **design reopening required**; do not preserve 0.1.5 BSS multiplier automatically in the next candidate.

### Repentance — full weekly Faith + Story throughput

Direct accepted runtime evidence:

- scheduler rolls once per in-game day;
- each existing confessional rolls independently;
- Confessional I success reward: **1 Faith** and one Story:
  - 70% Story I;
  - 30% Story II;
- Confessional II success reward: **2 Faith** and one Story:
  - 70% Story II;
  - 30% Story III;
- there are two confessional slots in the church.

Rebalanced accepted probability/duration:

- Bronze 50%, ~2.4 days;
- Silver 75%, ~4.8 days;
- Gold 100%, ~7.2 days.

Continuous expected throughput with both confessionals:

| Tier | Expected confessions | Extra Faith | Expected Stories B/S/G |
| --- | ---: | ---: | --- |
| Bronze | 2.4 | 3.6 | 0.84 / 1.20 / 0.36 |
| Silver | 7.2 | 10.8 | 2.52 / 3.60 / 1.08 |
| Gold | 14.4 | 21.6 | 5.04 / 7.20 / 2.16 |

Discrete scheduler alignment can move the actual count by roughly one daily roll around effect boundaries.

For an exact **six-day sermon week**, Gold 100% always creates 12 confession events across the two church slots, but Faith/Story yield depends on which confessional tier occupies those slots:

| Church setup | Extra Faith | Expected Story mix |
| --- | ---: | --- |
| 2 x Confessional I | **12 Faith** | 8.4 Bronze / 3.6 Silver |
| 1 x Confessional I + 1 x Confessional II | **18 Faith** | 4.2 Bronze / 6.0 Silver / 1.8 Gold |
| 2 x Confessional II | **24 Faith** | 8.4 Silver / 3.6 Gold |

This is **in addition to the Repentance sermon payout itself**.

At CQ40 without Eloquence, Gold Repentance's retained stock sermon contribution is approximately:

- base Faith 8;
- +75% = +6;
- +3 fixed Faith;
- sermon total ~= **17 Faith**.

With two Confessional II and all six days collected, the weekly package is therefore about **41 Faith + 12 Stories**.

This is substantially stronger than treating Repentance as a small utility buff. The power is compensated by:

- two-confessional infrastructure;
- deeper theology progression for Confessional II;
- repeated daily church interaction;
- Story value being indirect rather than liquid Faith;
- weekly opportunity cost.

Nevertheless Repentance must now be included in the Faith-family power audit; it cannot be balanced in isolation only as "confession probability".

## CraftingQualityProbe 0.1.0 result and narrow gap

User runtime log verified the target 1.407 MVID and the core multi-quality algorithm.

CraftDefinition.GetMultiqualityResult directly shows:

1. sum quality-contributing perk values;
2. sum linked-buff craft-quality values;
3. store recipe difficulty;
4. average quality of qualifying multi-quality ingredients;
5. derive Bronze/Silver/Gold probabilities by clamping the resulting scalar into 0..1 / 1..2 / 2..3 bands.

The same log directly shows:

- Notes linked to Writer / Playwright / Industriousness and Inspiration;
- Chapter uses the same writing family;
- Hard Book is a separate quality craft;
- Hard Book uses cover + Chapter;
- Hard Book links Jeweler / Industriousness and Excellence;
- Book difficulty is 0.5 at Desk I and 0.3 at Desk II.

Therefore the extra Book quality bottleneck is directly proven.

However probe 0.1.0 filtered the **pulpit pray:* rows** rather than the actual b_faith / b_money / b_faith_money Desk recipe rows. It therefore did **not** directly close the final prayer-item recipe difficulty/perk stage.

This is a concrete probe-coverage defect, not a user-test failure.

### CraftingQualityProbe 0.1.1

0.1.1 fixes exactly that gap:

- includes actual b_faith, b_money, b_faith_money, b_souls, b_sins craft rows;
- dumps perk-related CraftDefinition methods as well as quality methods;
- dumps MultiqualityCraftResult nested type/IL to close the exact value-result expression.

Build evidence:

- source SHA: 42d0659c242de647b270ec3ca183fc40d42183d7;
- frozen ref: candidate/crafting-quality-probe-0.1.1;
- Actions run: 35330900953;
- result: success;
- artifact ZIP digest: sha256:6bb2b4673a48b2ec56239019b98bbd6cabdad111d56536f5b4b82cc0c20476b8;
- DLL SHA-256: ee24759e29e3a7202370b71aa3acc4d51c03ca05019c44b56991f71121cc969b.

Runtime contract remains read-only: no Harmony, no balance/save mutation.


## Crafting-quality probe 0.1.1 — direct closure

The user's stock 1.407 runtime log from the narrow read-only Crafting Quality Probe 0.1.1 closes the remaining Chapter-vs-Book quality-path question.

Direct facts:

- runtime MVID remains `6f50b8e7-156b-49ac-bbe8-7505894b2364`;
- `MultiqualityCraftResult.value_result = value_items + value_perks - difficulty`;
- `value_items` is the average quality of qualifying multi-quality inputs;
- linked perks and active linked buffs add their quality values before recipe difficulty is subtracted;
- Prayer for Faith final craft: Chapter + 5 Faith, difficulty **0.7**, writing-quality modifiers;
- Prayer for Donations final craft: Chapter + 5 Faith, difficulty **0.7**, writing-quality modifiers;
- Combo Prayer final craft: Book + 7 Faith, difficulty **1.0**, writing-quality modifiers;
- the Book is itself a separate quality gate from cover + Chapter, difficulty **0.5** at Desk / **0.3** at Desk II, with Jeweler / Industriousness / Excellence modifiers;
- BSS Soul's Repose final craft: Chapter + 5 Faith + 2 Sin Shards, difficulty **0**, writing-quality modifiers.

Conclusion: the proposed staged grammar is now directly supported by stock 1.407 crafting structure. Chapter specialists are materially easier to quality-up than Book Combo, and Combo pays both an extra intermediate quality roll and a harder final recipe. This evidence removes the previous need for an additional crafting-quality probe before comparing the staged-handoff architecture.

Do not commit the raw runtime dump; retain only these minimal derived facts.


## Working baseline accepted for further research — staged economic handoff

The user accepts the staged economic-prayer construction as the **working design baseline** for subsequent balancing/research. This is not yet a production acceptance.

Working values:

- Prayer for Faith: q20/40/60, +50% Faith at all tiers, +5/+10/+15 flat Faith, no prayer-owned money bonus.
- Prayer for Donations: q20/40/60, +50% donations at all tiers, +3/+6/+9 silver flat, no prayer-owned Faith bonus.
- Combo Prayer: q40/60/80, +100/+150/+200% Faith and donations, no prayer-owned flat Faith or money.
- BSS Soul's Repose: old +200/+300/+400% Rebalanced scaling is reopened; leading direction is stock-like +50/+100/+150% with Soul Gratitude as its intrinsic scaling axis.
- Repentance: must be evaluated as a weekly Faith + Story engine, not merely a utility buff.

Design intent:

- early specialists are strong, cheap Chapter-class tools;
- midgame exposes specialist-versus-Combo choice;
- late-game Gold Combo becomes the ordinary universal economic sermon;
- specialists are allowed to age out naturally;
- DLC/active-effort prayers can occupy distinct late-game niches without invalidating the core progression.

## Aspirational ritual hypothesis — q~120 "wonder prayer"

A separate design desire is now explicit: the roster should ideally contain at least one **optional super-late-game aspirational prayer** whose certainty is deliberately above a mature passive Cathedral and therefore rewards temporary preparation with candles/incense.

Target feel:

- requirement around q120 (illustrative, not accepted);
- not part of the mandatory Faith/Donations/Combo handoff;
- not the weekly default;
- strong enough that the player intentionally prepares the church for a special service;
- success should feel like an event / "wonder" rather than a maintenance tax;
- normal passive late-game play must remain complete without it.

At practical passive CQ ~94, q120 gives only ~78% success, so certainty requires roughly +26 temporary Church Quality. This is exactly the kind of gap that can give candles/incense an aspirational use without making them compulsory every week.

### Best current homes

**1. Gold Prayer for Excellence — strongest architectural fit**

Why it fits:
- Gold Excellence is already one of the largest vertical upgrades in the Rebalanced roster (+1.0 linked-craft quality vs stock +0.2).
- The effect is bounded by a crafting session and by item-quality ceilings rather than being an uncapped weekly resource engine.
- A player can deliberately prepare a high-CQ service before a planned "masterpiece" crafting session.
- It gives candles/incense a special-event use instead of a weekly upkeep role.
- It does not disturb the newly clean economic-prayer ladder.
- It is thematically coherent: an exceptional church ritual grants an exceptional quality-crafting window.

Potential ladder to investigate, not accepted:
- Bronze q20-30;
- Silver q60;
- Gold q120.

The large Gold jump would be intentional: Gold Excellence is the "miracle" tier rather than merely the next linear upgrade.

**2. Gold BSS Soul's Repose — strongest spectacle/output fit**

Why it fits:
- Soul Gratitude already creates an extraordinary late-game Faith ceiling.
- DLC progression and Soul-room investment make it naturally late.
- q120 would make the enormous Gold payout feel earned rather than free.

Why it is weaker architecturally:
- it is DLC-exclusive;
- its output can already become extreme from Soul Gratitude alone;
- making certainty require consumables on an uncapped Faith engine could feel like an optimization tax rather than a special event.

**3. A new dedicated "Grand" prayer — cleanest fantasy, highest implementation cost**

A separate new prayer could be designed specifically for q120+, but this would require new item/recipe/presentation/localization/possibly-icon work and would expand the mod beyond rebalancing existing content. Do not prefer this while an existing prayer can carry the role.

### Current leading hypothesis

Use **Gold Excellence** as the first q120 candidate.

The intended progression would be:

- core economic system reaches its normal apex at Gold Combo q80;
- ordinary passive Cathedral completion remains satisfying around q90-100;
- Gold Excellence sits above that as an optional aspirational ritual that specifically rewards temporary church preparation.

This preserves the clean economic handoff while still giving the endgame a visible "one more mountain" to climb.


## Whole-roster progression audit — post staged-economic baseline

The staged Faith/Donations/Combo baseline is now treated as the ordinary economic spine. The rest of the roster is re-audited against *when its role matters* and *whether Silver/Gold power has outgrown its current Church Quality gate*.

This is a **candidate q audit**, not production acceptance.

| Prayer | Rebalanced effect | Current q | Progression reading | Leading q direction |
| --- | --- | ---: | --- | ---: |
| Ordinary | stock baseline | 10 | starter only | **10 keep** |
| Faith | +50% +5/10/15 Faith | working 20/40/60 | early/mid specialist, ages out | **20/40/60 working baseline** |
| Donations | +50% +3/6/9s | working 20/40/60 | early/mid cash specialist, ages out | **20/40/60 working baseline** |
| Combo | +100/+150/+200% both, no flats | working 40/60/80 | ordinary late-game economic apex | **40/60/80 working baseline** |
| Prosperity | 1/2/3 Blessings | 10/20/30 | extremely useful early, naturally exhausts vendor-tier role | **10/20/30 keep** |
| Roots | -20/-30/-40%, 36/72/108 min | 10/20/30 | farming-stage utility; Gold lasts ~14.4 game days | **10/30/50 candidate** |
| Repentance | 50/75/100% daily confession, 18/36/54 min | 10/20/40 | deep Theology route; Gold can add ~24 Faith + 12 Stories/week with 2x Confessional II | **20/40/60 leading** |
| Repose | corpse-tier reliability, ceiling unchanged, 18/36/54 | 20/40/50 | stock/Bronze eventually lose the +tier benefit; Rebalanced Silver/Gold may retain a reliability benefit by forcing the best currently eligible tier | **20/40/50 keep; direct final-range check before claiming late-game value in UI** |
| Combat | +5/+10/+15 dmg, +4 armor, 1/2/4 HP/s, 36/72/108 | 10/20/40 | very deep unlock; huge Gold package lasting >2 weeks but narrow combat niche | **20/40/60 leading; 70 upper test** |
| Imagination | +0.7 writing q; 18/36/54; +3 Silver/Gold Stories at S/G | 10/40/60 | powerful writing accelerator; Book prayer; community shows Silver can already create extreme writing sessions | **10/40/60 defensible; test 20/50/70 only if play/economy evidence demands** |
| Excellence | +0.2/+0.5/+1.0 craft q, 18/36/54 | 10/40/60 | late infrastructure prayer; Gold is a major deterministic-quality jump | **20/60/90 leading** |
| BSS Soul's Repose | leading +50/+100/+150% Faith on CQ+GP base | old 25/40/70 | DLC endgame Faith engine; GP creates uncapped-looking late scaling; final prayer craft is relatively easy Chapter-class | **30/60/120 leading miracle ladder** |
| BSS Soul Contentment | +20% GP, 36/72/108 | 10/20/30 | narrow Soul workflow; Gold persists ~14.4 days and amplifies the future Soul's Repose axis | **20/40/60 candidate** |
| BSS Thorough Cleansing | x2 Sin Shards, 36/72/108 | 10/20/30 | extremely strong scarce-resource multiplier; Gold covers >2 sermon weeks | **30/60/90 leading** |

### High-confidence q corrections

#### Repentance -> 20 / 40 / 60

Gold is no longer a vague utility effect. On stock 450-second days, its 54-minute duration spans ~7.2 game days, so with two Confessional II it can cover a full six-day sermon week and add roughly 24 Faith and 12 Stories, on top of its own sermon payout. Under a 1.5x Longer Days setup that preserves the 54-minute wall-clock duration, it spans only ~4.8 game days; practical output is closer to 8-10 confessions (16-20 Faith with two Confessional II) depending on daily-roll alignment.

The prayer also requires player interaction and confessional infrastructure, so it should not be priced like a universal passive resource engine. q60 is a sufficient visible premium; q80+ would likely double-tax the activity cost.

#### Combat -> 20 / 40 / 60

The deep Martial Skills route and Hard Book already pay much of the acquisition cost. However q10/20/40 understates the Rebalanced package: Gold grants +15 damage, +4 armor and 4 HP/s for ~14.4 game days.

q60 makes Gold feel earned without pushing a combat preparation prayer so late that the remaining combat content is gone. q70 is an upper stress-test, not the leading value.

#### Excellence -> 20 / 60 / 90

Gold +1.0 craft quality is a qualitatively different tool from Bronze +0.2 and can turn premium outcomes deterministic. Its 54-minute duration is ~7.2 vanilla game days, but only ~4.8 game days under a 1.5x Longer Days setup that normalizes buff wall-clock duration.

Because its scope is bounded by item-quality ceilings, it does not need q120. q90 makes Gold an aspirational near-max-passive Cathedral reward while preserving the separate q120 miracle slot for BSS Soul's Repose.

#### BSS Thorough Cleansing -> 30 / 60 / 90

x2 Sin Shards is already a major rare-resource multiplier. Gold lasts 108 real-time minutes: ~14.4 game days (>2 sermon intervals) on stock 450-second days, but ~9.6 days (~1.6 sermon intervals) under a 1.5x Longer Days setup that deliberately preserves wall-clock buff duration. Current q30 still looks cheap for stock balance, but the '>2 weeks' rationale must not be generalized to modded day lengths.

Community players describe it as one of the strongest prayers because it can effectively halve the corpse throughput needed for high-end soul/corpse work. A 30/60/90 ladder makes the DLC progression visible without requiring consumables for certainty at a mature passive Cathedral.

### The q120 miracle candidate: BSS Soul's Repose -> 30 / 60 / 120

This is now the strongest architectural hypothesis for the super-late ritual.

Why it fits better than Excellence:

- BSS is optional lateral/endgame content by construction;
- Soul Gratitude creates a second progression axis after the ordinary Church/Graveyard economy is already mature;
- high GP can make the Gold payout spectacular even with stock-like +150% rather than the old Rebalanced +400%;
- current community evidence reports several-hundred-Faith Soul's Repose sermons at high GP;
- the prayer's item recipe is Chapter-class and the 1.407 final craft row has difficulty 0, so its quality-production burden is materially softer than premium Book prayers;
- a q120 sermon gate therefore gives the extraordinary payout a visible cost that matches its fantasy.

Candidate ladder:

- Bronze **q30** — BSS prayer becomes usable without being free;
- Silver **q60** — mature ordinary church;
- Gold **q120** — deliberate miracle service.

At passive CQ94, Gold q120 is about 78% success; at CQ96, 80%. Certainty therefore requires roughly +24-26 temporary CQ.

That is comfortably achievable by the game's existing temporary church system: Candle (4) gives +3 CQ per candle and Incense II gives +4 per burner use. A Cathedral has enough candle capacity that +24-26 is not an exotic theoretical ceiling; it is a deliberate consumable-preparation target.

This creates the desired fantasy:
ordinary prayer progression is complete -> BSS/GP investment creates an extraordinary Faith opportunity -> the player lights candles/incense for a special Gold Soul's Repose service.

### Moderate / uncertain q changes

#### Roots -> 10 / 30 / 50 candidate

Gold is strong because both magnitude and duration scale. On vanilla 450-second days, 108 real-time minutes span ~14.4 game days; external day-length mods can change the number of game days covered without changing the prayer's real-time duration. Current q30 may undersell the vanilla proposition.

But Roots is stage-sensitive. Raising Gold to q60-80 risks making the prayer reliable only after farming acceleration matters less. q50 is the current upper useful target.

#### Soul Contentment -> 20 / 40 / 60 candidate

+20% magnitude is not extreme, but Gold lasts ~14.4 days and indirectly feeds the Soul Gratitude axis that powers Soul's Repose.

q60 creates a useful BSS preparation loop:
Gold Contentment during active soul processing -> build GP over a long window -> later prepare q120 Gold Soul's Repose.

This should be modeled for actual GP throughput before acceptance.

#### Imagination -> provisionally keep 10 / 40 / 60

It is extremely powerful in the right planned writing session. A July 2026 community report describes Silver Imagination enabling a four-day writing burst with large skill-point/profit output.

However:
- it is a Hard Book prayer;
- its +0.7 core does not increase by tier;
- Silver/Gold mostly buy duration plus Story return;
- the prayer itself helps solve the same writing-quality bottleneck that makes premium prayer crafting expensive.

Therefore q10/40/60 remains defensible. Do not raise it merely because the effect is good. A 20/50/70 stress-test is appropriate only if the full writing-economy model shows Silver/Gold trivialize progression too early.

### q values that should not rise

#### Prosperity -> keep 10 / 20 / 30

Community repeatedly identifies it as exceptionally useful early for vendor tiers. Its reward naturally exhausts itself once those tiers are open. Raising q would move the prayer out of the stage in which its role exists.

#### Repose -> keep 20 / 40 / 50

Gold certainty is strong, but the corpse-tier ceiling is unchanged and the effect becomes useless once normal progression reaches the final eligible corpse tier. Recent 2026 community discussion explicitly describes this obsolescence.

A higher q would shorten an already finite usefulness window.

## Emerging full progression grammar

Ordinary base game:

- q10: starter / early utility;
- q20-30: first specialist/niche tools;
- q40: midgame quality step;
- q50-60: mature specialist / strong situational tools;
- q80: Gold Combo, ordinary economic apex;
- q90: exceptional bounded Gold effects such as Excellence / Thorough Cleansing;
- q120: BSS Gold Soul's Repose miracle ritual requiring temporary Church Quality preparation.

This produces multiple visible reasons to keep improving the church beyond vanilla q60 without turning every prayer into the same ladder.

Status: research hypothesis. The staged economic baseline is accepted for further research; the non-economic q changes above still require cross-roster acceptance before implementation.


## Duration-model correction — vanilla baseline vs Longer Days

A material clarification is required for every duration-based balance statement.

### Direct PrayerClarity behavior

`dur_parameter` is the verified prayer-buff duration in real-time minutes.

PrayerClarity production does **not** hard-code a 7.5-minute day when presenting duration. `PrayerForecast.DurationParameterToGameDays` asks the runtime `TimeOfDay.FromTimeKToSeconds(1f)` for the effective seconds-per-day and converts:

`game_days = duration_minutes * 60 / effective_seconds_per_day`.

Therefore the shipped UI already adapts to a day-length mod that patches the game's TimeOfDay conversion.

### Vanilla 1.407 baseline

Vanilla day length = 450 seconds = 7.5 real minutes.

Therefore:

- 18/36/54 real minutes = **2.4 / 4.8 / 7.2 vanilla game days**;
- 36/72/108 real minutes = **4.8 / 9.6 / 14.4 vanilla game days**;
- one six-day sermon interval = **45 real minutes**.

These are valid **vanilla balance** conversions.

### Current Longer Days behavior

Current Longer Days source slows `EnvironmentEngine.Update` according to the selected day multiplier and also patches the stock hard-coded 450-second basis in `BuffsLogics.AddBuff` / `PlayerBuff.GetTimerText` to the configured day length specifically so buffs keep their **normal wall-clock duration** instead of stretching with the longer day.

At 1.5x (675-second / 11.25-minute days):

- 18/36/54 real minutes = **1.6 / 3.2 / 4.8 game days**;
- 36/72/108 real minutes = **3.2 / 6.4 / 9.6 game days**;
- one six-day sermon interval = **67.5 real minutes**.

Consequences for the user's current runtime:

- Gold Repentance (54 min) does **not** span a full sermon week;
- Gold Repose/Imagination/Excellence likewise span ~4.8 modded days;
- Gold Roots/Combat/Contentment/Cleansing (108 min) span ~9.6 modded days, about 1.6 sermon intervals, not >2.

This is a correction to earlier conversational statements that applied vanilla day-count conversion to the user's Longer Days runtime.

### Balance policy

PrayerClarity: Rebalanced should continue to balance primarily against **stock Graveyard Keeper 1.407** rather than compensate for another gameplay mod.

However:

- research prose must label vanilla-day equivalents as vanilla-only;
- player-facing duration should remain dynamically derived from the runtime day length (already implemented);
- runtime feel tests performed with Longer Days must be interpreted with the shorter *game-day coverage* above.

No production duration bug is currently identified in PrayerClarity.

## Repose late-game obsolescence — reopened nuance

Stock Repose is directly proven to add +1 only to Donkey `Tier max`, leaving `Tier min` unchanged. Community consistently reports that once the final corpse-delivery progression is unlocked, the stock +1 ceiling no longer opens a new tier and the prayer becomes useless.

Rebalanced 0.1.5 is structurally different:

- Bronze preserves the stock evaluated range;
- Silver has a 50% chance to force `tier_min = tier_max`;
- Gold always forces `tier_min = tier_max`;
- it never raises the evaluated maximum beyond the stock/prayer ceiling;
- stock RNG still chooses among corpse definitions inside the resulting tier.

Recent community descriptions report that the final normal delivery pool remains a **range** rather than a single tier (commonly described as 2-3). If that external description matches stock 1.407 runtime state, then:

- Bronze still becomes effectively obsolete when +1 can no longer extend the maximum;
- Silver remains useful by sometimes excluding the lower eligible tier;
- Gold remains useful by always excluding it and guaranteeing the best currently eligible tier.

In that case, the Rebalanced reliability redesign **does fix the main vanilla late-game obsolescence for Silver/Gold**.

Evidence status:
- the Rebalanced narrowing algorithm is direct project fact;
- stock +1-max behavior is direct project fact;
- the exact final-game `body_min/body_max` range is currently **community-supported but not yet project-direct runtime evidence**.

Do not add a hard player-facing claim such as "Gold remains useful after final corpse progression" until that final-range state is directly verified.

### UX direction once final range is verified

Vanilla / stock-Bronze can explicitly communicate the natural endpoint, e.g. conceptually:

> Raises the maximum corpse tier while a higher tier is still available.

Rebalanced Silver/Gold should instead communicate reliability:

- Silver: moves halfway from the current stock distribution toward guaranteed best eligible tier;
- Gold: guarantees the best eligible corpse tier.

If the current evaluated range is already a single tier, a dynamic contextual line such as **"No further corpse-tier benefit at current progression"** would be more truthful than a static late-game warning. This can be computed on demand when the prayer UI opens; no per-frame work is required.


## Balance-time policy — vanilla only

User decision: all Rebalanced balance decisions use **stock Graveyard Keeper 1.407 time**.

Longer Days or other day-length mods may affect runtime screenshots/logs from the user's installation and therefore must be accounted for when interpreting observed "days remaining", but they are **not** balance targets.

Canonical vanilla day basis:

- 1 day = 450 seconds = 7.5 real minutes;
- 6-day sermon interval = 45 real minutes;
- 18/36/54-minute prayer durations = 2.4 / 4.8 / 7.2 vanilla days;
- 36/72/108-minute prayer durations = 4.8 / 9.6 / 14.4 vanilla days.

Current Rebalanced 0.1.5 does **not** alter prayer duration values. Rebalanced mechanics change magnitude/behavior while leaving stock prayer `dur_parameter` values intact.

External cross-check:
- official Graveyard Keeper Wiki Effects / sermon pages report the same real-minute prayer duration families;
- official/community day references report a 7.5-minute vanilla day.
Some individual wiki prayer pages retain stale parenthetical "days" labels that imply an older day length; use the verified real-minute values plus the 450-second 1.407 day basis rather than those stale parentheticals.

## Repose pulpit UX policy

Do not add Repose endpoint/obsolescence text to Technology; that surface is already dense and static.

Preferred surface: **pulpit forecast only**, where the player's current corpse-progression state is available and the information affects the immediate sermon decision.

Vanilla target behavior:
- when Repose can still increase the maximum eligible corpse tier, show the normal verified effect;
- when current progression has no higher tier for the stock +1-max mechanic to unlock, add a concise factual contextual line indicating that the corpse-tier effect currently has no further upgrade available;
- do not say "do not use this prayer", because the sermon can still have ordinary Faith/donation value.

Preferred wording direction (not localization-accepted yet):
- "Corpse tier: no higher tier is currently available."
- RU sense: "Уровень трупов: более высокий уровень сейчас недоступен."

Rebalanced:
- Bronze follows the stock endpoint logic;
- Silver/Gold may retain reliability value even after the stock maximum can no longer increase, because they narrow the eligible range toward the best tier;
- do not show the vanilla "no further tier effect" line for Silver/Gold unless direct runtime state proves the evaluated range is already a single tier.

Implementation, if accepted, should be on-demand at pulpit redraw/open using canonical player/donkey progression parameters; no per-frame scans or Technology mutation.


## Cross-roster q decision after BSS/core-progression review

Working design decision for the next implementation candidate (research acceptance; production still unchanged):

| Prayer | Working q |
| --- | ---: |
| Ordinary | 10 |
| Faith | 20 / 40 / 60 |
| Donations | 20 / 40 / 60 |
| Combo | 40 / 60 / 80 |
| Prosperity | 10 / 20 / 30 |
| Shoots & Roots | 10 / 30 / 50 |
| Repentance | 20 / 40 / 60 |
| Repose | 20 / 40 / 50 |
| Combat | 20 / 40 / 60 |
| Imagination | 10 / 40 / 60 |
| Excellence | 20 / 60 / 90 |
| BSS Soul's Repose | 30 / 60 / 120 |
| BSS Soul Contentment | 20 / 40 / 60 |
| BSS Thorough Cleansing | 30 / 60 / 90 |

### Why Soul Contentment stops at 60

Better Save Soul itself can begin soon after the church opens; it is not a postgame-only DLC by access. Soul Contentment is also inherently state-limited: its +20% Gratitude gain matters only while the player is actively healing/releasing souls and has useful Gratitude capacity/consumption. Community reports explicitly note that the prayer becomes pointless when Gratitude is capped and not being spent.

Therefore q30/50/70 was considered and rejected as unnecessary tuning. q20/40/60 is enough to price the doubled Rebalanced magnitude while keeping the prayer inside the stage where its workflow matters.

### Gold Soul's Repose remains the q120 miracle

The high Gold requirement does not imply the whole BSS roster is postgame. Instead the quality ladder deliberately spans progression:

- Bronze q30: usable BSS tool;
- Silver q60: mature normal church;
- Gold q120: aspirational prepared service.

The pulpit already communicates the relevant mechanic ("Faith depends on Church Quality and Soul Gratitude") and displays the unusually high Church Quality requirement/success chance. Do not add editorial copy such as "late-game prayer" or "miracle ritual" merely to justify q120. The mechanics and requirement should explain themselves.

Community evidence supports the role separation: Soul's Repose may be attractive early when Gratitude is high relative to Church Quality, but very high Gratitude can later push it into several-hundred-Faith outputs.

## Repose endpoint pulpit wording — terminology constraint

Current RU stock effect wording owned by PrayerClarity is:

\`Осёл может привозить более качественные тела. (up) (skull) (rskull)\`

When stock/Vanilla Repose cannot raise the maximum eligible corpse tier any further, the pulpit should **replace**, not append to, that effect sentence.

Preferred RU wording:

\`Ещё более качественные тела сейчас недоступны. (up) (skull) (rskull)\`

Rationale:
- reuses the exact player-facing term \`более качественные тела\`;
- states only the current mechanical limitation;
- does not tell the player whether to use the prayer;
- avoids introducing internal terminology such as tier/body_max;
- remains compact in the already-dense pulpit surface.

English semantic target:

\`No higher-quality bodies are currently available.\`

The same contextual replacement may be used for Rebalanced Bronze. Rebalanced Silver/Gold must retain their reliability wording whenever narrowing the currently eligible corpse range still changes the result. Only show the no-further-effect replacement if direct current progression state proves the prayer cannot change the evaluated range at all.

Technology remains unchanged.
