# PrayerClarity — Design Notes

Status: design hypothesis stage, 2026-09-14. Mechanics re-checks are closed; no production implementation accepted yet.

## Problem statement

Stock Graveyard Keeper 1.407 answers **“will this sermon succeed?”** reasonably well, but poorly answers **“what will this prayer actually do for me now?”**.

The player can see church quality, sermon requirement, and success chance. They generally cannot see current Faith/donation outcomes, many passive prayers omit magnitude/duration, quality can change duration without changing strength, and two stock prayers contain special-effect anomalies that vanilla does not disclose.

The design target is a compact white-box explanation at the decision point, not a wiki-style wall of text.

## Actual mechanics -> vanilla information -> player risk

| Area | Actual mechanics | Vanilla before use | UX status |
| --- | --- | --- | --- |
| Success | church quality / requirement -> visible chance | quality, requirement, % chance shown | already clear; do not duplicate heavily |
| Faith notation | fixed Faith output is additive; proportional bonus is separate | `xN` can look multiplicative | confirmed UX finding |
| Donations | baseline uses graveyard quality | selection foregrounds church quality; no current donation forecast | confirmed UX finding |
| Souls prayer | baseline uses church quality + Soul Gratitude | dependency mentioned, current result absent | confirmed UX finding |
| Passive magnitude | +5 damage, +4 armor, +0.7/+0.2 quality inputs, +1 max Donkey corpse tier, etc. | several descriptions are flavour-only | confirmed UX finding |
| Passive duration | common tiers 18/36/54 or 36/72/108 min; quality often changes duration only | duration omitted | confirmed UX finding |
| Failure | base Faith/money remain; prayer bonuses/special success outputs are lost | chance shown, consequence not explained | confirmed supporting issue |
| Repentance | timed `buff_sins` exists; no gameplay consumer found | flavour-only | stock mechanics anomaly + clarity risk |
| Shoots/Roots | `-20%` growth formula exists, but prayer writes `buff_plant` to player while formula reads growing WGO | farming flavour only | confirmed stock wiring mismatch + clarity risk |

## Information target

For the selected prayer the player should be able to obtain, without formula algebra:

1. expected successful Faith/donation result now;
2. failure result when chance <100%;
3. special output/effect;
4. duration;
5. what changes with prayer quality;
6. only the current dependencies needed to understand the displayed result;
7. a truthful indication when a stock special effect is disconnected/inert rather than pretending the intended effect works.

## Option A — rewrite descriptions only

Useful for replacing ambiguous `xN` wording and adding lines such as `+5 damage for 36 min`, but insufficient for current-state Faith/donation comparison and Souls calculations.

**Assessment:** useful supplementary cleanup, not the main solution.

## Option B — compact dynamic breakdown in existing prayer selection

Preferred hypothesis: preserve vanilla success information and add a small selected-prayer block.

Illustrative hierarchy only:

- `Faith on success: 18`
  - `Base 11 · Prayer +7`
- `Donations on success: 42s`
  - short dependency hint where useful
- `On failure: 11 Faith · 28s`
- `Effect: +5 damage`
- `Duration: 36 min`

For quality comparison where relevant:

- `Same +5 damage · duration 36 -> 72 min`

For Prayer for Repose:

- `Effect: Donkey maximum corpse tier +1`
- `Duration: 18 / 36 / 54 min by prayer quality`

For Soul Contentment:

- `Effect: +10% Soul Gratitude`
- exact forecast may use `RoundToInt(GP_base * 1.1)` if a next-soul preview is ever shown.

### Advantages

- answers the decision question directly;
- supports current-state values without exposing coefficient algebra;
- can reveal church/graveyard/Soul Gratitude dependencies only when relevant;
- can distinguish success and failure outcomes;
- can make `same strength, longer duration` explicit.

### Risks

- must fit existing UI without crowding;
- must not present probabilistic success outcome as guaranteed;
- preview calculation must be side-effect free;
- broken/disconnected stock effects need careful wording rather than accidental bug fixing.

**Assessment:** strongest design hypothesis and next prototype target.

## Option C — separate detail panel

Gives more space but adds lifecycle/UI complexity and risks becoming an in-game wiki.

**Assessment:** fallback only if option B cannot remain readable.

## Side-effect-free calculation architecture

Do **not** call `PrayLogics.CalculatePray` to render a preview: it performs random success/result bookkeeping and manages sermon result/drop state.

Preferred production direction:

- read selected `CraftDefinition`;
- evaluate linked `PrayEventDefinition` expressions through stock expression evaluation;
- reproduce only verified deterministic rounding/bonus composition;
- read existing church/graveyard/Soul Gratitude/perk state through normal getters;
- compute on prayer-selection open/change, not per frame;
- no broad reflection scans in production;
- cache stable mappings only if needed.

Exact Harmony/lifecycle targets remain an implementation-stage inspection task and must not be guessed.

## Broken/disconnected prayer policy

PrayerClarity is an information mod first. Do not silently repair stock mechanics.

### Prayer of Repentance

Verified facts:

- prayer attaches timed `buff_sins`;
- no gameplay consumer was found in code literals, 180 loaded FlowCanvas graphs, or balance references beyond its definition/crafts;
- community/wiki reports independently describe no apparent effect.

Preferred design principle: do not invent the intended confessional effect. Candidate wording may be a restrained note such as `Special effect not detected in 1.407`, but exact player-facing wording is still a design hypothesis.

### Prayer for Shoots and Roots

Verified stock wiring:

- growth definitions contain `-0.2*WGOpar("buff_plant")`;
- prayer application writes `buff_plant=1` to player data;
- `WGOpar` reads the bound growing/workbench WGO;
- CraftComponent evaluates growth craft time with that WGO as the expression context;
- no propagation path from player param to growing WGO was found.

Therefore do **not** show `20% faster growth` as a working benefit. If the anomaly is surfaced, wording should distinguish `stock effect disconnected in 1.407` from a balance fix.

## Repose is now fully player-translatable

Probe 0.1.5 closes the live Donkey path:

- `Tier min = body_min + add_body_min`;
- `Tier max = body_max + add_body_max`;
- prayer buff contributes `body_max=1`.

Player-facing semantic meaning is therefore:

**Donkey maximum corpse tier +1 while active.**

Prayer quality changes only duration: 18/36/54 minutes.

## Prototype acceptance questions

A narrow prototype should demonstrate:

- Faith / Donations / Combo can be compared without external arithmetic;
- graveyard -> donations becomes understandable without formula exposition;
- Souls prayer shows current Soul Gratitude contribution clearly;
- passive prayer effect + duration can be understood at a glance;
- quality-only duration changes are obvious;
- risky sermons distinguish success and failure results;
- Repentance and Shoots/Roots are not falsely presented as functioning special effects;
- the UI still looks plausible in Graveyard Keeper;
- runtime work occurs only on relevant UI/state changes.

## Current decision

Mechanics discovery is sufficiently closed for the first narrow prototype. No additional probe or user mechanics test is currently justified.

Proceed with **Option B: compact dynamic breakdown in the existing prayer-selection context**, while keeping exact layout/text provisional until the first runtime UI prototype is inspected.

Production behavior changes to broken prayers remain out of scope unless separately accepted.