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
