# Player UX Research — Prayer/Sermon System

Status: initial evidence map, 2026-09-14.

This document records **player-experience evidence**, not authoritative game mechanics. Mechanics are verified separately in `PRAYER_MECHANICS.md`.

## Method

Sources are used to answer questions such as:

- what players cannot infer from the game;
- what wording they misread;
- which prayer comparisons repeatedly require outside arithmetic;
- which dependencies (church, graveyard, Soul Gratitude, prayer quality) are not obvious at decision time;
- which parts of the system have prompted calculators, tables, guides, or mods.

A comment or thread is a **community signal**, not proof of a mechanic. Repetition across time strengthens evidence of a durable UX problem but is not called a consensus without stronger sampling.

## Strong current signals

| Date | Source | Player-experience signal | Classification / caveat |
| --- | --- | --- | --- |
| 2026-06-09 | Reddit — `Is Prayer for Faith in bronze quality worth using?` | New player reads `Faith (x2)` as “double base Faith” and `Faith (x1) (+50%)` as 150% of base, therefore initially concludes the upgraded Faith prayer may be worse than Casual Prayer. The post explicitly asks for the math behind the displayed line. | **community signal**; very current and directly about presentation semantics |
| 2026-03-13 | Reddit — `The Effect of "Prayer for Soul's Repose" is Secretly Capped?` | Player observes a result that does not match their inferred formula, suspects a hidden cap, then updates the post after community members derive a different formula from testing. | **community signal**; current and especially relevant because direct 1.407 runtime data independently confirms the Souls base-Faith formula depends on both church quality and Soul Gratitude |
| 2026-01 | Steam — `What does graveyard quality do (aside from finishing bishop quests)` | Player explicitly asks where the game explains what graveyard rating does and initially believes Faith/money both come from church quality. Replies say this information may not be shown and explain graveyard -> sermon donations, church -> Faith. | **community signal**; current, directly tests discoverability of a major dependency |
| 2026-04 | Steam — `Why is my friend getting twice the donations with worse stats?` | Player compares two games and cannot explain different donations from visible church/graveyard stats. Explanation requires separating total donations, visitors, graveyard rating, prayer success, and perks. | **community signal**; current, demonstrates difficulty interpreting the visible donation animation/results |

Source URLs:

- https://www.reddit.com/r/GraveyardKeeper/comments/1u0z66v/is_prayer_for_faith_in_bronze_quality_worth_using/
- https://www.reddit.com/r/GraveyardKeeper/comments/1rsjmat/the_effect_of_prayer_for_souls_repose_is_secretly/
- https://steamcommunity.com/app/599140/discussions/0/780944762959117296/
- https://steamcommunity.com/app/599140/discussions/4/801218828360078237/

## Repeated historical/supporting signals

These are useful for showing persistence, but older reports are not automatically assumed to describe the exact 1.407 UI.

| Date | Source | Signal | Caveat |
| --- | --- | --- | --- |
| 2021-03 | Reddit — `Please fix prayers descriptions` | Poster specifically argues that `x2 Faith` is interpreted as multiplication although observed behavior is additive; says prayer descriptions make comparisons difficult. | strong historical wording signal; current 2026 thread shows the same interpretation still occurs |
| 2024-05 | Reddit — `Gold prayer for Faith vs Silver Combo Prayer?` | Player asks which prayer is better; one reply links an external sermon calculator spreadsheet to answer quantitatively. | supports “comparison requires external arithmetic” signal |
| 2021-10 | Reddit — `FINALLY got my first sermon...` | Player expected visible Combo Prayer stats to imply a larger benefit; reply links a sermon-gain calculator. | older, but directly about expectation versus result |
| 2023-12 | Reddit — `Beginner's guide to prayers?` | New player says they are trying to figure out how prayers work and cannot choose the next prayer from in-game understanding alone. | general discoverability signal, not evidence of one specific UI defect |
| 2025-02 | Reddit — `What's the best prayer to use?` | Player says passive-effect prayers seem unreliable because they did not notice a difference from Repose/Repentance. | suggests weak effect feedback; exact cause still unknown |
| 2023-12 | Steam — `How good Pray for soul's repose?` | Player asks how much Soul Gratitude changes Faith and how to compare it with ordinary prayer percentages. | supports Soul Gratitude predictability problem |
| 2018-09 | Steam — `What do the different sermons do?` | Player says sermon details do not explain buff effects well and hovering the buff provides no useful description. | historical only; must revalidate current 1.407 buff UI before treating as current UX finding |

Source URLs:

- https://www.reddit.com/r/GraveyardKeeper/comments/ma1hcy/
- https://www.reddit.com/r/GraveyardKeeper/comments/1d3svge/
- https://www.reddit.com/r/GraveyardKeeper/comments/qe9hhr/
- https://www.reddit.com/r/GraveyardKeeper/comments/18laao6/
- https://www.reddit.com/r/GraveyardKeeper/comments/1ij5rxj/
- https://steamcommunity.com/app/599140/discussions/0/4034726433730288491/
- https://steamcommunity.com/app/599140/discussions/0/1733212454825720761/

## Existing mod landscape — initial check

A current Nexus mod, `Pray the Day Away`, was updated on 2026-05-19 and changes sermon frequency/consumption/pacing. It does not present itself as a prayer-effect clarity/forecast mod. This is useful evidence that sermon mechanics are an active mod surface, but it does **not** prove that no existing clarity mod exists.

Source:

- https://www.nexusmods.com/graveyardkeeper/mods/72

A broader mod inventory should be completed before claiming novelty.

## Provisional synthesis against verified mechanics

These are candidate gaps, not final design requirements.

### Candidate A — additive fixed reward is visually easy to read as a multiplier

**Actual mechanics:** `CalculatePray` treats fixed `faith` output as an additive Faith bonus, while `k_faith` separately adds a percentage-like bonus derived from base Faith.

**Player evidence:** both a 2021 report and a new 2026 report interpret `x1`/`x2` notation as multiplication. The 2026 player specifically reads `Faith (x1) (+50%)` as 150% of base and initially compares it incorrectly with `Faith (x2)`.

**Potential UX gap, pending direct 1.407 localization/UI audit:**

> Before choosing between prayers, a player can interpret the fixed Faith quantity as a multiplier even though the game adds it as a flat amount, producing the wrong ranking of prayer outcomes.

This is currently the strongest candidate because the player interpretation is recent and the underlying additive mechanic is directly verified.

### Candidate B — church rating and graveyard rating feed different sermon outputs

**Actual mechanics:** ordinary event-base Faith uses church quality; event-base money uses graveyard quality. Church quality also participates in prayer success probability.

**Player evidence:** January and April 2026 Steam threads show players trying to infer donations from church/graveyard stats and asking where the relationship is explained.

**Potential UX gap, pending direct presentation audit:**

> Before selecting or upgrading a prayer, a player may not be able to tell that church rating primarily feeds Faith/success while graveyard rating feeds the base donation pool, so visible church improvements can be wrongly expected to raise sermon money directly.

### Candidate C — Soul's Repose cannot be predicted from the visible percentage alone

**Actual mechanics:** the Souls event family calculates base Faith from `(church quality + gratitude_points) * 0.1 * (1 + 0.3*Eloquence)` before prayer-specific bonus terms.

**Player evidence:** 2023 and 2026 discussions ask how Soul Gratitude affects the result; the 2026 player suspects a hidden cap before community members derive a formula.

**Potential UX gap, pending exact current description audit:**

> Before using the Souls prayer, the player cannot reliably predict its Faith baseline from the ordinary prayer percentage notation if the UI does not expose how current Soul Gratitude changes that baseline.

### Candidate D — comparing Faith / Donations / Combo / Souls often requires external arithmetic

**Actual mechanics:** the prayer result is composed from event-base values, fixed output, success probability, and proportional `k_faith` / `k_money` bonuses. Different prayers use different quality thresholds and event families.

**Player evidence:** comparison questions recur across 2021–2026, and multiple answers resort to external spreadsheets/calculators or manual save/reload testing.

**Potential UX gap:**

> At prayer-selection time, the UI may expose ingredients/percentages without exposing the actual expected Faith/money result for the player's current church/graveyard state, forcing arithmetic outside the game to make a quantitative comparison.

This is not yet a final finding because PrayerClarity still needs to record exactly what 1.407 currently previews.

### Candidate E — passive/buff prayer effects may lack sufficient explanation or feedback

Historical and 2025 reports say players do not understand or notice some passive sermon effects. Direct runtime data proves several prayers apply specific buff flags/resources, but the present-day tooltip and buff UI have not yet been audited.

Do not promote this to a current UX finding until the 1.407 descriptions, buff hover UI, and effect consumers are inspected.

## What must be audited before design work

1. Exact English/Russian current localization for every accepted prayer item.
2. Exact prayer-selection panel content and how it formats fixed quantities, percentages, church requirement, and risk.
3. Whether current Faith, donation, or success predictions exist anywhere before confirmation.
4. Current buff descriptions / hover behavior for passive prayer effects.
5. Result-screen wording: whether it distinguishes base versus prayer bonus clearly enough to teach the model after use.
6. Whether Souls prayers surface current Soul Gratitude's quantitative role.
7. Broader Nexus/mod inventory for existing prayer-information solutions.

## Design status

No design has been accepted. In particular, this research does not yet justify assuming that “make the tooltip longer” is the right fix.

The likely decision will depend on the presentation audit:

- if the problem is mostly notation, a localization-sized clarification may be sufficient;
- if the hidden problem is current-state arithmetic, a compact dynamic prediction/breakdown may be better;
- if passive effects are the main gap, a contextual effect line or buff description may be enough.

No production DLL is needed at this stage.