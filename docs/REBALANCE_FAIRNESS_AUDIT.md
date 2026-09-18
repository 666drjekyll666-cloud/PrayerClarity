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
