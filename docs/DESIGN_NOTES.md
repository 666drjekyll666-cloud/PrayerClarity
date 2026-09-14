# PrayerClarity — Design Notes

Status: design hypothesis stage, 2026-09-14. No production implementation accepted.

This document starts only after the mechanics and presentation audits established concrete information gaps. It does not select a final layout yet.

## Problem statement

The stock 1.407 prayer-selection UI is good at answering **“will this sermon succeed?”** but poor at answering **“what will this prayer actually do for me now?”**.

The player can see church quality, sermon requirement, and success chance. They generally cannot see a current-state Faith/donation forecast, many passive prayers do not state quantitative effects or duration, and quality upgrades can change duration without changing buff strength.

The design target is therefore not “more tooltip text”. It is a compact white-box explanation at the decision point.

## Actual mechanics -> vanilla information -> likely interpretation

| Area | Actual mechanics | Vanilla before use | Player-understanding risk | UX status |
| --- | --- | --- | --- | --- |
| Success | linear church-quality/requirement chance | church quality, requirement, % chance shown | low | already clear; do not duplicate heavily |
| Faith notation | fixed Faith output is flat additive; `k_faith` is separate proportional bonus | `xN`-style quantity + percentage wording | `xN` can be read as multiplication | confirmed UX finding |
| Donations | base donation pool uses graveyard quality; prayer bonus is separate | selection panel foregrounds church quality and chance; no current donation forecast | player may attribute money to church quality or per-person coin animations | confirmed UX finding |
| Souls prayer | Faith baseline uses `church quality + Soul Gratitude` | description says more Gratitude gives more Faith, but no current result | dependency known, magnitude unpredictable | confirmed UX finding |
| Passive magnitude | examples: +5 damage, +4 armor, +0.7/+0.2 quality inputs; `buff_skull` exposes `body_max=1` but exact live corpse consumer remains to verify | several descriptions are flavour-only | player cannot tell what buff actually does | confirmed UX finding; Repose wording still gated on re-check |
| Passive duration | prayer `dur_parameter` overrides default buff duration; common tiers 18/36/54 or 36/72/108 minutes | prayer descriptions omit duration | higher quality can be mistaken for stronger effect rather than longer effect | confirmed UX finding |
| Post-sermon totals | base and bonus are separate internally and on result report | report shows separate base/bonus rows | player must mentally combine components | supporting UX issue |
| Prayer of Repentance | timed `buff_sins` is attached; no consumer found in code, loaded graphs, or balance data; external wiki/player evidence also reports no apparent effect | flavour description only | player can assume a meaningful special effect where none is detectable | mechanics defect / clarity risk; wording must be cautious |

## Information the player should be able to obtain before committing

For the currently selected prayer, the UI should be able to answer:

1. **What is the expected successful result now?**
   - Faith total or breakdown;
   - donation total or breakdown;
   - physical special output, if any.
2. **What happens if the sermon fails?**
   - base Faith remains;
   - base donations remain in stock 1.407;
   - prayer-specific bonuses/special success outputs are lost.
3. **What special effect does this prayer give?**
   - player-facing effect, not internal parameter name.
4. **How long does it last?**
5. **What changes with prayer quality?**
   - strength, duration, reward coefficient, physical quantity, or some combination.
6. **Which current state drives the result?**
   - church quality;
   - graveyard quality;
   - Soul Gratitude where relevant;
   - relevant perks/state only when they materially change the displayed result.

The UI should show outputs/dependencies, not raw algebra.

## Design option A — rewrite existing descriptions only

Example concept:

- replace ambiguous `xN` wording;
- add `+5 damage for 36 min` to Retribution;
- add `+4 armor for 36 min` to Protection;
- add duration to Souls DLC buffs.

### Advantages

- smallest technical surface;
- cheap runtime cost;
- visually native;
- solves passive-effect ambiguity.

### Limitations

- cannot solve current-state Faith/donation comparison;
- cannot explain current Soul Gratitude result;
- static text becomes awkward for values driven by current state/perks;
- risks turning descriptions into dense mini-wiki entries.

**Assessment:** useful baseline, insufficient for the full white-box goal.

## Design option B — compact dynamic breakdown in the existing prayer-selection context

Concept: keep vanilla success information and add a small dynamic block for the selected prayer.

Illustrative information hierarchy, not accepted wording/layout:

- `Faith on success: 18`
  - `Base 11 · Prayer +7`
- `Donations on success: 42s`
  - `Based on graveyard quality 140`
- `On failure: 11 Faith · 28s`
- `Effect: +5 damage`
- `Duration: 36 min`
- optional short dependency hint: `Faith uses church quality`; `Donations use graveyard quality`.

For a Souls prayer:

- `Faith on success: 24`
- `Uses church quality 42 + Soul Gratitude 80`

For an upgrade comparison where useful:

- `Same +5 damage · duration 36 -> 72 min`

### Advantages

- answers the decision question directly;
- avoids formula algebra;
- naturally supports current-state values;
- makes church/graveyard/Soul Gratitude dependencies visible only when relevant;
- can clarify failure semantics without a separate calculator screen.

### Risks

- must fit existing UI without crowding;
- must avoid presenting probabilistic outcomes as guaranteed when chance <100%;
- requires a non-mutating calculation path; production code must not call `CalculatePray` merely to preview because it performs success/random/result-side work.

**Assessment:** strongest current design hypothesis.

## Design option C — separate prayer-detail panel

Concept: dedicated compact panel opened from prayer selection, containing full breakdown/comparison.

### Advantages

- more room for complex effects and quality comparison;
- easiest place to handle exceptional prayers cleanly.

### Limitations

- heavier UI/lifecycle work;
- less immediate than in-context information;
- easy to drift into “wiki inside the game”.

**Assessment:** reserve only if option B cannot fit readable information.

## Recommended prototype direction

Option B remains the preferred prototype direction, but the external cross-check exposed two special-effect paths that should be resolved before a **complete prayer-by-prayer** white-box prototype is treated as mechanically authoritative:

- Prayer for Shoots and Roots: verify that active player `buff_plant` reaches the `WGOpar("buff_plant")` growth-time consumers that exist in current 1.407 balance data;
- Prayer for Repose: trace the current corpse-generation consumer of `buff_skull`/`body_max` rather than relying only on external tier descriptions or save-migration code.

After those narrow checks, prototype option B with these constraints:

- preserve vanilla church-quality / requirement / success-chance lines;
- add only prayer-specific/current-result information that vanilla omits;
- use a compact hierarchy rather than paragraphs;
- show successful result first;
- show failure result only when success chance <100%;
- show effect and duration as one short line each;
- show dependency hints only where they explain a non-obvious number;
- do not show internal IDs, `k_faith`, `k_money`, SmartExpression syntax, or formulas.

A static description cleanup can be included later where it materially improves wording, but it should not be the only solution.

## Calculation architecture hypothesis

A preview must be side-effect free.

Do **not** call `PrayLogics.CalculatePray` just to render UI because it performs success/random/result bookkeeping and manipulates sermon result/drop state.

Preferred production direction:

- read selected `CraftDefinition`;
- evaluate the linked `PrayEventDefinition` expressions through the game's normal expression evaluator;
- reproduce only the deterministic rounding/bonus composition already verified;
- read current church/graveyard/Soul Gratitude/perk state through existing getters;
- calculate only when the prayer selection UI opens or selected prayer/state changes;
- no per-frame polling;
- no broad reflection scans in production;
- cache only stable mappings if needed.

Exact Harmony/lifecycle targets remain a later implementation question and must be inspected rather than guessed.

## Handling Prayer of Repentance

Current evidence is unusual:

- the prayer attaches `buff_sins`;
- the buff exists and has duration;
- no gameplay consumer was found across code literals, 180 loaded FlowCanvas graphs, or balance references beyond the buff definition and the three prayer crafts;
- the official community wiki still labels the effect broken/no apparent effect, and player testing/discussion through 2026 has not established a working confessional modifier;
- a generic 1.301 patch note said `The prayers are fixed`, but does not identify Repentance and is outweighed for current behavior by the direct 1.407 runtime audit.

Do not invent an effect.

Design possibilities, pending acceptance:

1. show only verified sermon rewards/duration and omit a claimed special effect;
2. show a restrained diagnostic-style note such as `Special effect not detected in 1.407`;
3. treat this as a separate vanilla bug finding and keep PrayerClarity informational rather than repairing balance/mechanics.

Preferred principle: **clarity mod first, rebalance/fix mod only by separate decision**.

## Soul Contentment rounding

External wiki evidence confirms the intended/current player-facing magnitude: `+10% Soul Gratitude` while Spiritual Blessing is active. The direct 1.407 graph also reads `increase_gp_gain` and rounds before adding Gratitude, but the precise connection order was not fully captured.

For UI design this is not a blocker if the mod says `+10% Soul Gratitude`. It becomes relevant only if PrayerClarity promises an exact integer forecast for the next released soul; then the production preview must reproduce the game's exact rounding order rather than assume one.

## Prototype acceptance questions

Before any production merge, a prototype should answer:

- Can the player compare Prayer for Faith, Prayer for Donations, and Combo Prayer without external arithmetic?
- Does the panel make graveyard -> donations obvious without overexplaining?
- Does a Souls prayer show the current Soul Gratitude contribution clearly?
- Does a passive prayer state exact effect + duration in one glance?
- Does a higher-quality passive prayer make `same strength, longer duration` immediately understandable where true?
- Is the panel still readable and visually plausible in Graveyard Keeper's UI?
- Does the implementation update only when relevant UI/state changes?

## Current decision

Research strongly supports **option B** as the UX direction, but the broad external cross-check changed the immediate technical order.

Before production/UI implementation, perform only two narrow mechanics re-checks:

1. `buff_plant` end-to-end propagation for Prayer for Shoots and Roots;
2. live `buff_skull` corpse-tier consumer for Prayer for Repose.

The Repentance anomaly is sufficiently characterized for design purposes; Soul Contentment's +10% magnitude is sufficiently characterized unless exact next-soul integer forecasting becomes a requirement. No additional broad prayer probe is justified.