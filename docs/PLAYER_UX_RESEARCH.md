# Player UX Research — Prayer/Sermon System

Status: evidence map, updated 2026-09-14 after direct 1.407 presentation probes 0.1.0 and 0.1.1.

This document records **player-experience evidence and UX findings**, not authoritative mechanics formulas. Mechanics are verified separately in `PRAYER_MECHANICS.md`.

## Method

Sources are used to identify:

- what players cannot infer from the game;
- wording players repeatedly misread;
- prayer comparisons that require outside arithmetic;
- dependencies on church, graveyard, Soul Gratitude, prayer quality, or perks that are not obvious at decision time;
- prayer effects that require wiki tables, calculators, save/reload testing, or forum explanations.

A comment or thread is a **community signal**, not proof of a mechanic. Repetition strengthens evidence of a durable UX problem but is not called a consensus without stronger sampling.

Direct runtime/UI inspection is used to decide whether a community complaint still applies to current Graveyard Keeper 1.407.

## Strong current community signals

| Date | Source | Player-experience signal | Classification / caveat |
| --- | --- | --- | --- |
| 2026-06-09 | Reddit — `Is Prayer for Faith in bronze quality worth using?` | New player reads `Faith (x2)` as “double base Faith” and `Faith (x1) (+50%)` as 150% of base, initially concluding the upgraded Faith prayer may be worse than Casual Prayer. The post asks for the actual math behind the displayed line. | **community signal**; recent and directly about notation semantics |
| 2026-03-13 | Reddit — `The Effect of "Prayer for Soul's Repose" is Secretly Capped?` | Player observes a result that does not match their inferred formula, suspects a hidden cap, then updates after other players derive a different formula from testing. | **community signal**; recent; direct 1.407 runtime independently confirms the Souls baseline uses both church quality and Soul Gratitude |
| 2026-01-15 | Steam — `What does graveyard quality do (aside from finishing bishop quests)` | Player explicitly asks where the game explains graveyard rating and initially believes sermon Faith/money both come from church quality. | **community signal**; recent, direct discoverability complaint |
| 2026-04-12 | Steam — `Why is my friend getting twice the donations with worse stats?` | Player compares two games and cannot reconcile per-person donation animations with church/graveyard stats. Explanation requires separating total donations, visitor count, graveyard rating, prayer success, and perks. | **community signal**; recent; visible per-person coins can teach the wrong mental model |
| 2026-04-20 | Reddit — `Does the higher tier inspirational books give a stronger buff?` | Player explicitly asks whether a higher-quality inspiration prayer makes the writing buff stronger. Replies clarify that quality affects duration rather than buff magnitude. | **community signal**; recent and directly about effect-strength versus duration scaling |

Source URLs:

- https://www.reddit.com/r/GraveyardKeeper/comments/1u0z66v/is_prayer_for_faith_in_bronze_quality_worth_using/
- https://www.reddit.com/r/GraveyardKeeper/comments/1rsjmat/the_effect_of_prayer_for_souls_repose_is_secretly/
- https://steamcommunity.com/app/599140/discussions/0/780944762959117296/
- https://steamcommunity.com/app/599140/discussions/4/801218828360078237/
- https://www.reddit.com/r/GraveyardKeeper/comments/1sqeane/does_the_higher_tier_inspirational_books_give_a/

## Repeated historical/supporting signals

These help establish persistence but are not automatically treated as current-1.407 presentation evidence.

| Date | Source | Signal | Caveat |
| --- | --- | --- | --- |
| 2021-03 | Reddit — `Please fix prayers descriptions` | Poster says `x2 Faith` reads like multiplication although observed behavior is additive; says prayer descriptions make comparisons difficult. | strong historical wording signal; similar interpretation still appears in 2026 |
| 2024-05 | Reddit — `Gold prayer for Faith vs Silver Combo Prayer?` | Quantitative comparison question; answer links an external sermon calculator spreadsheet. | supports “comparison requires outside arithmetic” signal |
| 2021-10 | Reddit — first-sermon expectation mismatch | Player expects visible Combo Prayer stats to imply a larger benefit; reply links a sermon-gain calculator. | historical comparison signal |
| 2023-12 | Reddit — `Beginner's guide to prayers?` | New player says they are trying to figure out how prayers work and cannot confidently choose the next prayer. | broad discoverability signal |
| 2025-02 | Reddit — `What's the best prayer to use?` | Player says some passive-effect prayers seem unreliable because they did not notice a difference. | possible feedback/description problem; exact cause unproved |
| 2023-12 | Steam — `How good Pray for soul's repose?` | Player asks how Soul Gratitude changes Faith and how to compare the Souls prayer with ordinary prayer percentages. | supports Souls predictability issue |
| 2018-09 | Steam — `What do the different sermons do?` | Player complains buff effects are not explained well and buff hover does not answer the question. | historical only; current pre-use item text has now been revalidated independently |

Source URLs:

- https://www.reddit.com/r/GraveyardKeeper/comments/ma1hcy/
- https://www.reddit.com/r/GraveyardKeeper/comments/1d3svge/
- https://www.reddit.com/r/GraveyardKeeper/comments/qe9hhr/
- https://www.reddit.com/r/GraveyardKeeper/comments/18laao6/
- https://www.reddit.com/r/GraveyardKeeper/comments/1ij5rxj/
- https://steamcommunity.com/app/599140/discussions/0/4034726433730288491/
- https://steamcommunity.com/app/599140/discussions/0/1733212454825720761/

## Direct current-1.407 presentation audit

### The selection panel is about success, not outcome

Current `PrayCraftGUI.RedrawTextValues` draws exactly:

- current church quality;
- selected sermon requirement;
- success chance.

Current Russian localization is:

- `Качество церкви: %1`;
- `Проповедь требует: %1`;
- `Шанс успеха: %1`;
- below the threshold, button `Попытка молитвы`;
- at/above the threshold, button `Молиться`.

The prayer item description also appends `%1 необходимо, чтобы гарантировать успех проповеди.`

**Finding:** hidden success probability is not a PrayerClarity problem. Stock 1.407 exposes the relevant requirement and chance directly.

The inspected selection panel does **not** calculate or display the current expected Faith total, donation total, passive-effect magnitude, passive duration, or Souls-prayer Faith result.

### The post-sermon report splits base and bonus values

Current `PrayReportGUI.Open` displays:

- `Пришло посетителей`;
- `Веры получено` = base `PrayResult.faith`;
- `Пожертвования` = base `PrayResult.money`;
- `Результат`;
- success chance;
- on success, separate `Бонус веры` and `Бонус денег` rows when non-zero.

It does not show one combined Faith total or one combined donation total. The player must understand that the bonus rows are additional components rather than replacements.

### Current Russian prayer descriptions are often qualitative/flavour text

Direct 1.407 localization resolves the accepted player-visible families:

| Internal family | Current RU name | Current description's mechanical information |
| --- | --- | --- |
| `b_empty` | Обычная молитва | no quantitative effect |
| `b_faith` | Молитва веры | says more Faith, no current-state amount |
| `b_money` | Молитва о пожертвованиях | says additional donations, no current-state amount |
| `b_faith_money` | Комбо-молитва | says additional donations and Faith, no current-state amount |
| `b_plant` | Молитва о корнях и побегах | flavour only; does not state the farming effect or duration |
| `b_sins` | Молитва о покаянии | flavour only; does not state the gameplay effect or duration |
| `b_skull` | Молитва об упокоении | flavour/advertising joke; does not state the gameplay effect or duration |
| `b_sword` | Молитва о возмездии | says it fills the player with rage; no `+5 damage`, no duration |
| `b_shield` | Защитная молитва | says it makes the player stronger; no `+4 armor`, no duration |
| `b_pen` | Молитва воображения | “Эти слова придадут тебе вдохновение!”; no exact writing-quality effect or duration |
| `b_star` | Молитва о совершенстве | qualitative hard-work wording; no exact craft-quality effect or duration |
| `b_village` | Молитва о процветании | does explain that it gives Commercial Blessing and that the blessing can improve a trader; no quantity/current reward forecast |
| `b_souls` | Молитва за упокой душ | explicitly says more Soul Gratitude gives more Faith, but not how much Faith the current state will produce |
| `b_grat_points_incr` | Молитва о довольстве душ | explicitly says Soul Gratitude gain `+10%`; duration/quality scaling not stated |
| `b_sin_shard` | Молитва за тщательное очищение душ | explicitly says successful soul healing yields `x2` Sin Shards; duration/quality scaling not stated |

This is direct presentation evidence, not a wiki inference.

## Accepted / strongly supported UX findings

### UX finding A — `Faith (xN)` communicates the wrong arithmetic

**Actual mechanics:** the fixed `faith` output is a flat additive bonus. A separate `k_faith` field adds the proportional bonus from base Faith.

**What the player sees:** prayer quantity notation uses an `xN`-style component alongside percentage bonuses.

**Player interpretation:** both a 2021 report and a June 2026 report read `x1/x2` as multiplication. In the recent case this changes the player's ranking of prayer choices.

> Before choosing between prayers, the player can read `Faith (xN)` as a multiplier of base Faith even though that component is a flat additive reward, which can reverse the apparent comparison between prayers.

**Status:** strong UX finding: direct current mechanics + repeated player misinterpretation.

### UX finding B — the current selection UI gives success information but no current-state reward forecast

**Actual mechanics:** final rewards are assembled from church quality, graveyard quality, prayer-specific fixed and proportional bonuses, perks, and for Souls prayer current Soul Gratitude.

**Current UI:** selection shows church quality, requirement, and success chance. The inspected pre-use panel does not turn the player's current state into a Faith/donation result.

**Player evidence:** quantitative comparison questions recur, including use of external calculators/spreadsheets.

> At the moment of choosing a prayer, the player can see whether it is likely to succeed but cannot see the resulting Faith or donation output for the current church/graveyard/state, so quantitative comparison requires outside arithmetic or experimentation.

**Status:** mechanics and principal selection-panel side are directly verified. Remaining tooltip/resource-picker surfaces should still be checked before declaring that no vanilla pre-use surface anywhere provides equivalent information.

### UX finding C — church and graveyard quality drive different sermon outputs, but the decision UI foregrounds only church quality

**Actual mechanics:** ordinary base Faith uses church quality; base donations use graveyard quality. Church quality also drives sermon success.

**Current UI:** the selection panel explicitly shows church quality, sermon requirement, and chance. It does not show current graveyard rating or a donation baseline.

**Player evidence:** January/April 2026 Steam discussions show players attributing donations to church quality or attempting to infer profitability from attendee coin animations.

> Before choosing a prayer, the player is shown the church-side input needed to reason about success but not the graveyard-side input that determines the donation baseline, encouraging a single “church quality → sermon result” mental model even though Faith and money have different inputs.

**Status:** strong current UX finding.

### UX finding D — passive prayers hide effect magnitude and quality scaling

For several passive prayers, current item descriptions do not state the actual effect at all. Direct mechanics already establish examples such as:

- Prayer for Retribution / `b_sword`: `+5 damage` while active;
- Protection Prayer / `b_shield`: `+4 armor` while active;
- `b_skull`: `+1 body_max` resource while active;
- Prayer for Imagination / `b_pen`: fixed `craft_q = 0.7` used by linked writing-quality crafts;
- Prayer for Excellence / `b_star`: fixed `craft_q = 0.2` used by a broader set of linked quality crafts.

Direct 1.407 buff handoff also shows that the prayer craft's tier-specific `dur_parameter` overrides the buff definition's default duration. The tiers are therefore capable of changing **duration while keeping the underlying buff magnitude fixed**.

For the common duration families:

- `18 / 36 / 54` corresponds to `18 / 36 / 54` displayed game-timer minutes;
- `36 / 72 / 108` corresponds to `36 min / 1 h 12 min / 1 h 48 min`.

The current item descriptions do not state those durations. A current April 2026 player specifically asks whether a higher-tier inspiration prayer strengthens the effect or only changes duration.

> Before using or upgrading many passive prayers, the player cannot determine from the prayer's current description what the buff quantitatively does, how long it lasts, or whether higher prayer quality increases strength, duration, or both.

**Status:** strong UX finding. Exact buff-hover text after use remains worth auditing, but it cannot solve the user's stated **before-use** decision problem by itself.

### UX finding E — Souls prayer tells the dependency but not the current outcome

**Actual mechanics:** `b_souls` uses `(church quality + Soul Gratitude)` as the Faith baseline input before its prayer-specific bonus.

**Current description:** explicitly says `Чем больше (gratitude_points), тем больше веры`.

This means the dependency itself is **not hidden** in current Russian localization. The remaining problem is quantitative predictability.

> Before using Prayer for Soul's Repose, the player is told that Soul Gratitude matters but is not shown what the current Church Quality + Soul Gratitude state will actually produce in Faith.

**Status:** refined UX finding. Do not describe the Soul Gratitude dependency as completely undisclosed.

## DLC effects that are already comparatively clear

The Better Save Soul prayer descriptions are better than many base-game passive descriptions:

- Prayer for Soul Gratitude explicitly states `+10%` Gratitude gain;
- Prayer for Thorough Cleansing explicitly states `x2` Sin Shards from successful soul healing;
- Prayer for Soul's Repose explicitly states that more Soul Gratitude means more Faith.

PrayerClarity should not overwrite already clear magnitude text merely for consistency. The still-hidden useful information is primarily duration, tier scaling, and current-state output where applicable.

## Existing mod landscape — initial check

Current Nexus search/top-list review found sermon-affecting mods, but no obvious prayer-effect explanation/forecast mod:

- `Pray the Day Away` (updated 2026-05-19) changes sermon frequency, repeatability, consumption/downgrade rules, and playback speed;
- `Give Me Moar` (updated 2026-06-20) adds configurable multipliers for Faith, prayer donations, gratitude, and other resources;
- older `Not Just - Zombie Enhanced` includes an at-will sermon feature and unrelated tooltips, but is not presented as a prayer-effect clarity mod.

Sources:

- https://www.nexusmods.com/graveyardkeeper/mods/72
- https://www.nexusmods.com/graveyardkeeper/mods/70
- https://www.nexusmods.com/graveyardkeeper/mods/24

**Status:** useful negative evidence, not proof of novelty.

## Current priority for a white-box UX

The evidence no longer points to a single wording fix. The player needs a compact model that can answer, before committing:

1. **Will it work?** Vanilla already answers this with requirement/chance; do not duplicate unnecessarily.
2. **What does this prayer actually do?** This is poorly exposed for many passive prayers.
3. **What changes with prayer quality?** Often duration versus magnitude is not distinguishable from the description.
4. **What current inputs matter?** Church quality, graveyard quality, Soul Gratitude, relevant perks/state where applicable.
5. **What will I get now?** Faith, donations, physical output, and/or a buff in player-meaningful terms.

The internal implementation may require exact formulas, but the intended player presentation should prefer results and dependencies over formulas. For example, `Current Faith: N — affected by Church Quality and Soul Gratitude` is more consistent with the project goal than showing raw coefficient algebra.

## Remaining presentation questions before design

1. Close exact final payout/failure FlowCanvas wiring so any forecast matches real sermon delivery, not just `CalculatePray` fields.
2. Trace the remaining `buff_sins` gameplay consumer.
3. Close exact `+10% Gratitude` runtime arithmetic, despite the current description already stating the magnitude.
4. Enumerate the practical craft scope of `buff_pen` and `buff_star` far enough to describe them in player terms without listing hundreds of internal craft IDs.
5. Audit current buff-hover text as a secondary/post-use information surface.
6. Classify the remaining internal `PrayCraft` rows as reachable versus legacy/development content.

## Design status

No production design is accepted yet and no production DLL is justified.

However, the evidence now strongly favors a **compact contextual explanation/forecast** over merely rewriting one tooltip. A useful solution likely needs to combine:

- the prayer's actual effect in plain language;
- quality-dependent duration/magnitude behavior;
- relevant current inputs;
- current predicted output where deterministic;
- explicit probability/range wording only where the game is genuinely probabilistic.

This remains a design hypothesis until the final payout wiring and remaining prayer mechanics are closed.
