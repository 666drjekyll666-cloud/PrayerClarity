# Player UX Research — Prayer/Sermon System

Status: evidence map, updated 2026-09-14 after direct 1.407 presentation/mechanics probes 0.1.0–0.1.3.

This document records **player-experience evidence and UX findings**. Authoritative mechanics live in `PRAYER_MECHANICS.md`; design options live in `DESIGN_NOTES.md`.

## Method

Community sources are used to identify:

- what players cannot infer from the game;
- wording players misread;
- prayer comparisons that require outside arithmetic;
- hidden/unclear dependencies on church, graveyard, Soul Gratitude, prayer quality, perks, or buff duration;
- effects that require wiki tables, calculators, reload testing, or forum explanations.

A thread/comment is a **community signal**, not proof of mechanics. Direct 1.407 runtime/UI evidence is used to decide whether the signal still applies.

## Strong current community signals

| Date | Source | Signal | Classification |
| --- | --- | --- | --- |
| 2026-06-09 | Reddit — `Is Prayer for Faith in bronze quality worth using?` | Player reads `Faith (x2)` as double base Faith and `Faith (x1) (+50%)` as 150% of base, changing their comparison of prayers. | **community signal**, recent, notation-specific |
| 2026-03-13 | Reddit — `The Effect of "Prayer for Soul's Repose" is Secretly Capped?` | Player's inferred Souls formula does not match observed output; they suspect a hidden cap before others derive a different relation. | **community signal**; direct 1.407 evidence independently confirms Soul Gratitude enters the Faith baseline |
| 2026-01-15 | Steam — `What does graveyard quality do (aside from finishing bishop quests)` | Player believes Faith/money are both driven by church quality and asks where graveyard rating matters. | **community signal**, discoverability |
| 2026-04-12 | Steam — `Why is my friend getting twice the donations with worse stats?` | Player tries to reconcile per-person donation animations with church/graveyard stats. | **community signal**; visible coins can teach the wrong mental model |
| 2026-04-20 | Reddit — `Does the higher tier inspirational books give a stronger buff?` | Player asks whether better prayer quality strengthens the writing buff or only changes duration. | **community signal**, directly about quality scaling |

Source URLs:

- https://www.reddit.com/r/GraveyardKeeper/comments/1u0z66v/is_prayer_for_faith_in_bronze_quality_worth_using/
- https://www.reddit.com/r/GraveyardKeeper/comments/1rsjmat/the_effect_of_prayer_for_souls_repose_is_secretly/
- https://steamcommunity.com/app/599140/discussions/0/780944762959117296/
- https://steamcommunity.com/app/599140/discussions/4/801218828360078237/
- https://www.reddit.com/r/GraveyardKeeper/comments/1sqeane/does_the_higher_tier_inspirational_books_give_a/

## Historical/supporting signals

| Date | Source | Signal | Caveat |
| --- | --- | --- | --- |
| 2021-03 | Reddit — `Please fix prayers descriptions` | `x2 Faith` is read as multiplication although behavior is additive; descriptions make comparisons difficult. | historical, but same interpretation appears again in 2026 |
| 2024-05 | Reddit — `Gold prayer for Faith vs Silver Combo Prayer?` | Answer points to an external sermon calculator spreadsheet. | supports outside-arithmetic problem |
| 2021-10 | Reddit — first-sermon mismatch | Player expects visible Combo Prayer stats to imply a larger benefit; reply uses a gain calculator. | historical |
| 2023-12 | Reddit — `Beginner's guide to prayers?` | New player cannot confidently choose the next prayer. | broad discoverability signal |
| 2025-02 | Reddit — `What's the best prayer to use?` | Passive prayers seem unreliable because player does not notice a difference. | exact cause unproved |
| 2023-12 | Steam — `How good Pray for soul's repose?` | Player asks how Soul Gratitude changes Faith and how to compare Souls with ordinary percentage prayers. | supports quantitative-predictability problem |
| 2018-09 | Steam — `What do the different sermons do?` | Player says buff effects are poorly explained. | historical; current descriptions have now been audited directly |

Source URLs:

- https://www.reddit.com/r/GraveyardKeeper/comments/ma1hcy/
- https://www.reddit.com/r/GraveyardKeeper/comments/1d3svge/
- https://www.reddit.com/r/GraveyardKeeper/comments/qe9hhr/
- https://www.reddit.com/r/GraveyardKeeper/comments/18laao6/
- https://www.reddit.com/r/GraveyardKeeper/comments/1ij5rxj/
- https://steamcommunity.com/app/599140/discussions/0/4034726433730288491/
- https://steamcommunity.com/app/599140/discussions/0/1733212454825720761/

## Direct current-1.407 presentation audit

### Selection panel answers success, not outcome

`PrayCraftGUI.RedrawTextValues` shows exactly:

- current church quality;
- selected sermon requirement;
- success chance.

Current RU strings:

- `Качество церкви: %1`;
- `Проповедь требует: %1`;
- `Шанс успеха: %1`;
- below threshold: `Попытка молитвы`;
- at/above threshold: `Молиться`.

Item descriptions also append `%1 необходимо, чтобы гарантировать успех проповеди.`

**Conclusion:** hidden success probability is **not** a PrayerClarity problem.

The inspected pre-use panel does not provide a current Faith total, donation total, passive magnitude/duration, or Souls Faith result.

### Post-sermon report splits components

`PrayReportGUI.Open` shows:

- visitors;
- base Faith;
- base donations;
- result/status;
- success chance;
- success-only `Бонус веры` and `Бонус денег` rows.

It does not show one combined Faith total or one combined donation total.

### Current Russian prayer descriptions

| Internal family | Current RU name | Mechanical information in description |
| --- | --- | --- |
| `b_empty` | Обычная молитва | no quantitative effect |
| `b_faith` | Молитва веры | says more Faith, no current amount |
| `b_money` | Молитва о пожертвованиях | says extra donations, no current amount |
| `b_faith_money` | Комбо-молитва | says extra Faith + donations, no current amount |
| `b_plant` | Молитва о корнях и побегах | flavour only; no farming effect/duration |
| `b_sins` | Молитва о покаянии | flavour only; no gameplay effect/duration |
| `b_skull` | Молитва об упокоении | flavour/joke; no effect/duration |
| `b_sword` | Молитва о возмездии | rage wording; no `+5 damage`, no duration |
| `b_shield` | Защитная молитва | strength wording; no `+4 armor`, no duration |
| `b_pen` | Молитва воображения | inspiration wording; no exact quality effect/duration |
| `b_star` | Молитва о совершенстве | hard-work wording; no exact quality effect/duration |
| `b_village` | Молитва о процветании | explains Commercial Blessing purpose, not quantity/current reward |
| `b_souls` | Молитва за упокой душ | says more Soul Gratitude -> more Faith, no current result |
| `b_grat_points_incr` | Молитва о довольстве душ | states `+10%` Soul Gratitude, not duration/quality scaling |
| `b_sin_shard` | Молитва за тщательное очищение душ | states `x2` Sin Shards, not duration/quality scaling |

## Accepted / strongly supported UX findings

### A — `Faith (xN)` communicates the wrong arithmetic

**Actual mechanics:** fixed Faith output is flat additive. A separate `k_faith` creates the proportional bonus.

**Player interpretation:** both 2021 and 2026 examples read `x1/x2` as multiplication.

> Before choosing between prayers, the player can read `Faith (xN)` as a multiplier of base Faith even though that component is a flat additive reward, which can reverse the apparent ranking of prayer options.

**Status:** confirmed/strong: direct mechanics + repeated player misinterpretation.

### B — selection UI gives success chance but no current-state reward forecast

**Actual mechanics:** final rewards combine current church quality, graveyard quality, prayer fixed/proportional bonuses, perks, and for Souls prayer Soul Gratitude.

**Vanilla:** selection UI does not turn those inputs into current Faith/donation totals.

> At prayer-selection time, the player can see whether the sermon is likely to succeed but cannot see the resulting Faith/donation output for the current state, forcing outside arithmetic or experimentation for quantitative comparison.

**Status:** confirmed for the principal selection screen; current evidence gives no equivalent pre-use forecast elsewhere.

### C — church and graveyard quality drive different outputs, but the decision UI foregrounds only church quality

**Actual mechanics:** ordinary base Faith uses church quality; donations use graveyard quality; success also uses church quality.

**Vanilla:** selection shows church quality/requirement/chance but not current graveyard rating or donation baseline.

> The UI encourages a single “church quality -> sermon result” model even though Faith and donations have different primary inputs.

**Status:** strong current UX finding, supported by 2026 Steam questions.

### D — passive prayers hide effect magnitude and quality scaling

Verified examples:

- `b_sword`: +5 damage;
- `b_shield`: +4 armor;
- `b_skull`: +1 `body_max`;
- `b_pen`: fixed `craft_q=0.7` quality input;
- `b_star`: fixed `craft_q=0.2` quality input.

Prayer `dur_parameter` overrides default buff duration. Common tier sets are:

- 18 / 36 / 54 minutes;
- 36 / 72 / 108 minutes.

Thus higher prayer quality can mean **same buff strength, longer duration**.

> Before using or upgrading many passive prayers, the player cannot determine what the buff quantitatively does, how long it lasts, or whether prayer quality changes strength, duration, or both.

**Status:** confirmed/strong. Post-use buff-hover text cannot solve the before-use decision problem by itself.

### E — Souls prayer discloses the dependency but not the current outcome

`b_souls` uses `church quality + Soul Gratitude` in its Faith baseline. Current description explicitly says more Gratitude gives more Faith.

> The dependency is disclosed, but the player still cannot see what the current Church Quality + Soul Gratitude state will actually produce.

**Status:** confirmed refined finding. Do not claim Soul Gratitude is completely hidden.

### F — Prayer of Repentance appears to promise an effect that no runtime consumer uses

Direct mechanics audit:

- all three `b_sins` prayer tiers attach `buff_sins`;
- `buff_sins` exists and has a timed duration;
- final 0.1.3 scan found no game-code literal consumer;
- 180 loaded FlowCanvas graphs contained no `buff_sins` reference;
- GameBalance references were only the buff definition and the three prayer crafts.

**Fact:** no `buff_sins` gameplay consumer was found on the inspected runtime surfaces.

**Strong mechanics hypothesis:** the special effect is inert/unimplemented in stock 1.407.

UX implication:

> Vanilla flavour text gives the player no way to discover that the prayer's special timed buff has no detectable consumer, so external experimentation can be mistaken for an unclear or subtle effect rather than an apparently inert mechanic.

**Status:** mechanics anomaly + UX risk. PrayerClarity must not invent or rebalance an effect without a separate product decision.

## Failure semantics are also non-obvious

Direct `pray` graph evidence shows:

- failed sermon still delivers base Faith;
- failed sermon still delivers base donations in stock 1.407;
- prayer-specific Faith/money bonuses are removed;
- success animation contains buff creation and special item dropping.

Vanilla selection UI exposes chance, but not this consequence model. If PrayerClarity shows forecasts for risky prayers, it should show the success result and the failure result rather than implying “failure = nothing”.

## DLC effects that are comparatively clear already

Better Save Soul descriptions are stronger than many base-game descriptions:

- Soul Gratitude prayer states `+10%`;
- Thorough Cleansing states `x2` Sin Shards;
- Soul's Repose says more Soul Gratitude gives more Faith.

Do not rewrite already-clear magnitude text merely for consistency. Useful missing information is duration, tier scaling, and current-state output.

## Existing mod landscape — initial check

Current Nexus review found sermon-affecting mods but no obvious prayer-effect explanation/forecast mod:

- `Pray the Day Away` (updated 2026-05-19): sermon frequency/repeatability/consumption/playback changes;
- `Give Me Moar` (updated 2026-06-20): configurable reward multipliers;
- `Not Just - Zombie Enhanced`: at-will sermon feature plus unrelated tooltips.

Sources:

- https://www.nexusmods.com/graveyardkeeper/mods/72
- https://www.nexusmods.com/graveyardkeeper/mods/70
- https://www.nexusmods.com/graveyardkeeper/mods/24

**Status:** useful negative evidence, not proof of novelty.

## White-box information target

Before committing, the player should be able to answer:

1. Will it succeed? — vanilla already does this well.
2. What does the prayer actually do?
3. What changes with prayer quality?
4. Which current inputs matter?
5. What will I get now on success?
6. If chance is below 100%, what do I still get on failure?

The internal implementation may require formulas, but player presentation should prefer current results, concise breakdowns, and dependency hints over coefficient algebra.

## Design transition

The research phase has now identified enough concrete gaps to justify design work. `docs/DESIGN_NOTES.md` compares three solution classes and currently favors a **compact dynamic breakdown in the existing prayer-selection context**.

Production code is still premature. The next step is a narrow UI/layout/lifecycle prototype, not another broad mechanics probe.