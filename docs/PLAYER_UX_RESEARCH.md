# Player UX Research — Prayer/Sermon System

Status: initial evidence map, updated 2026-09-14.

This document records **player-experience evidence**, not authoritative game mechanics. Mechanics are verified separately in `PRAYER_MECHANICS.md`.

## Method

Sources are used to identify:

- what players cannot infer from the game;
- wording players repeatedly misread;
- prayer comparisons that require outside arithmetic;
- dependencies on church, graveyard, Soul Gratitude, prayer quality, or perks that are not obvious at decision time;
- prayer effects that require wiki tables, calculators, save/reload testing, or forum explanations.

A comment or thread is a **community signal**, not proof of a mechanic. Repetition strengthens evidence of a durable UX problem but is not called a consensus without stronger sampling.

## Strong current signals

| Date | Source | Player-experience signal | Classification / caveat |
| --- | --- | --- | --- |
| 2026-06-09 | Reddit — `Is Prayer for Faith in bronze quality worth using?` | New player reads `Faith (x2)` as “double base Faith” and `Faith (x1) (+50%)` as 150% of base, initially concluding the upgraded Faith prayer may be worse than Casual Prayer. The post asks for the actual math behind the displayed line. | **community signal**; recent and directly about notation semantics |
| 2026-03-13 | Reddit — `The Effect of "Prayer for Soul's Repose" is Secretly Capped?` | Player observes a result that does not match their inferred formula, suspects a hidden cap, then updates after other players derive a different formula from testing. | **community signal**; recent; direct 1.407 runtime independently confirms the Souls baseline uses both church quality and Soul Gratitude |
| 2026-01-15 | Steam — `What does graveyard quality do (aside from finishing bishop quests)` | Player explicitly asks where the game explains graveyard rating and initially believes sermon Faith/money both come from church quality. Replies say the relationship may not be shown in-game and explain graveyard -> donations, church -> Faith. | **community signal**; recent, direct discoverability complaint |
| 2026-04-12 | Steam — `Why is my friend getting twice the donations with worse stats?` | Player compares two games and cannot reconcile per-person donation animations with church/graveyard stats. Explanation requires separating total donations, visitor count, graveyard rating, prayer success, and perks. | **community signal**; recent; highlights that visible per-person coins can teach the wrong mental model |
| 2026-04-20 | Reddit — `Does the higher tier inspirational books give a stronger buff?` | Player explicitly asks whether a higher-quality inspiration prayer makes the writing buff stronger. Replies disagree/clarify that quality affects duration rather than buff magnitude. | **community signal**; recent and directly about whether prayer quality changes effect strength versus duration |

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
| 2018-09 | Steam — `What do the different sermons do?` | Player complains buff effects are not explained well and buff hover does not answer the question. | historical only; current buff UI must be revalidated |

Source URLs:

- https://www.reddit.com/r/GraveyardKeeper/comments/ma1hcy/
- https://www.reddit.com/r/GraveyardKeeper/comments/1d3svge/
- https://www.reddit.com/r/GraveyardKeeper/comments/qe9hhr/
- https://www.reddit.com/r/GraveyardKeeper/comments/18laao6/
- https://www.reddit.com/r/GraveyardKeeper/comments/1ij5rxj/
- https://steamcommunity.com/app/599140/discussions/0/4034726433730288491/
- https://steamcommunity.com/app/599140/discussions/0/1733212454825720761/

## Presentation audit changes the hypothesis

Current 1.407 static evidence shows that the prayer-selection flow computes `current_church_quality / needs_quality`, passes the ratio to `PrayCraftGUI.RedrawTextValues`, and changes the action button from a risky `try pray` state to guaranteed `pray` at the threshold. The prayer item description also includes its required church-quality value.

A public capture of the same Preaching UI family visibly shows `Church quality`, `Sermon needs`, and `Success chance`.

Therefore **hidden success probability is not currently considered a likely PrayerClarity UX gap**. Exact current localization remains to be pinned down, but available evidence points away from spending mod UI on information the base interface already provides.

This is important narrowing: the stronger candidate problems concern **what the reward notation means**, **which current-state values feed the reward**, **whether quality changes magnitude or only duration for passive prayers**, and **whether the player can compare actual outcomes before committing**.

## Existing mod landscape — initial check

Current Nexus search/top-list review found sermon-affecting mods, but no obvious prayer-effect explanation/forecast mod:

- `Pray the Day Away` (updated 2026-05-19) changes sermon frequency, repeatability, consumption/downgrade rules, and playback speed;
- `Give Me Moar` (updated 2026-06-20) adds configurable multipliers for Faith, prayer donations, gratitude, and other resources;
- older `Not Just - Zombie Enhanced` includes an at-will sermon feature and unrelated tooltips, but is not presented as a prayer-effect clarity mod.

Sources:

- https://www.nexusmods.com/graveyardkeeper/mods/72
- https://www.nexusmods.com/graveyardkeeper/mods/70
- https://www.nexusmods.com/graveyardkeeper/mods/24

**Status:** useful negative evidence, not proof of novelty. Search coverage is not exhaustive enough to claim that no clarity/forecast mod exists anywhere.

## Secondary public presentation reference

The current public wiki is useful as a **presentation/reference vocabulary source**, not mechanics authority. It exposes the same ambiguous quantity style for common prayers, e.g. `Faith (x1) (+50%)`, and documents passive effects in prose such as “better chance to write something of good quality.” This supports why players can form multiplier/strength assumptions, but direct 1.407 localization/UI remains the required source for any mod wording decision.

Examples:

- https://graveyardkeeper.fandom.com/wiki/Prayer_for_faith
- https://graveyardkeeper.fandom.com/wiki/Combo_prayer
- https://graveyardkeeper.fandom.com/wiki/Prayer_for_excellence
- https://graveyardkeeper.fandom.com/wiki/Sermon

## Provisional actual mechanics -> UI -> player-understanding synthesis

These are candidate UX gaps, not final design requirements.

### Candidate A — `Faith (xN)` can communicate the wrong arithmetic

**Actual mechanics:** fixed `faith` output is added as a flat `faith_bonus`. The separate `k_faith` field adds a proportional bonus calculated from base Faith.

**What the player appears to receive:** prayer descriptions/effect lines use an `xN`-style quantity alongside percentage bonuses for at least the common prayer families.

**Player interpretation:** both a 2021 report and a 2026 report read `x1/x2` as multiplication. In the recent case, `Faith (x1) (+50%)` is interpreted as 150% of base and compared incorrectly against `Faith (x2)`.

**Provisional UX finding:**

> Before choosing between prayers, the player can read `Faith (xN)` as a multiplier of base Faith even though that component is a flat additive reward, which can reverse the apparent ranking of prayer options.

This is currently the strongest candidate because both sides are supported: direct 1.407 calculation evidence and a recent concrete misinterpretation.

### Candidate B — church and graveyard quality affect different outputs, but players build a single “church quality -> sermon reward” model

**Actual mechanics:** ordinary event-base Faith uses church quality; base money uses graveyard quality. Church quality also determines sermon success probability.

**Current presentation evidence:** church quality and sermon requirement are presented in the prayer-selection flow. We have not yet found equivalent pre-use presentation tying the current graveyard rating to expected donations.

**Player interpretation:** current 2026 Steam threads explicitly ask what graveyard quality does, incorrectly attribute sermon money to church quality, or compare per-person donation animations as if they represented total sermon profitability.

**Provisional UX finding:**

> Before choosing a prayer, the player may see enough church information to reason about success while lacking an equally clear connection between current graveyard quality and the total donation baseline, producing incorrect expectations about money gains.

This still needs the full 1.407 selection/result-screen audit before final acceptance.

### Candidate C — Souls prayer baseline changes with Soul Gratitude in a way ordinary percentage notation does not teach

**Actual mechanics:** Souls events calculate base Faith from `(church quality + gratitude_points) * 0.1 * (1 + 0.3*Eloquence)` before prayer-specific bonuses.

**Player interpretation:** current and historical questions try to derive the effect from observed returns; a March 2026 player initially concludes the effect may be capped because their assumed formula does not match the result.

**Provisional UX finding:**

> Before using the Souls prayer, the player cannot reliably predict its Faith baseline from ordinary prayer percentage notation unless the UI exposes that current Soul Gratitude changes the baseline itself.

Whether current 1.407 explicitly exposes this dependency remains an open presentation question.

### Candidate D — quantitative prayer comparison may require external arithmetic even when individual ingredients are shown

**Actual mechanics:** final prayer-specific reward is composed from event-base values, fixed additive output, proportional coefficients, success state, perks, and for Souls prayers a different Faith baseline.

**Player evidence:** comparison questions recur across years; some answers use external calculators/spreadsheets or manual testing.

**Provisional UX finding:**

> At prayer-selection time, the base game may show requirements and effect ingredients without converting them into the actual Faith/donation outcome for the player's current state, forcing outside arithmetic for a quantitative choice.

This candidate is precisely what the next UI audit must confirm or reject. If the base UI already supplies a current result forecast, this candidate disappears.

### Candidate E — prayer quality can be mistaken for effect-strength scaling when it may change duration instead

Direct runtime data shows several passive prayer buffs have fixed effect parameters while the prayer tiers carry different duration-related values. For example, `buff_pen` exposes one fixed `craft_q` parameter and the prayer family has tier-dependent duration data.

A current April 2026 player explicitly asks whether a higher-tier inspiration prayer makes the writing effect stronger; replies clarify that the practical scaling is duration, not a stronger underlying buff.

**Provisional UX finding:**

> Before investing in a higher-quality passive prayer, the player may not be able to tell whether quality increases the buff's magnitude, its duration, or both, making the value of upgrading ambiguous.

**Status:** mechanics-side duration application is not yet fully traced, and the exact current item/buff text must be audited before accepting this finding.

## Current priority order

1. **Notation semantics (`xN` flat vs multiplicative)** — strongest evidence already available.
2. **Current-state outcome prediction / cross-prayer comparison** — high-value, presentation audit still incomplete.
3. **Church vs graveyard dependency visibility** — strong recent community signal, needs exact UI comparison.
4. **Soul Gratitude dependency visibility** — direct unusual mechanic + recent player confusion.
5. **Passive-buff magnitude vs duration** — now has a recent direct player question; still needs exact UI and duration-path evidence.
6. **Success chance** — deprioritized because the base UI appears to expose it already.

## What must be audited before design work

1. Exact current 1.407 English/Russian prayer names and descriptions.
2. Exact `PrayCraftGUI.RedrawTextValues` output: what the selection panel displays numerically for the selected prayer.
3. Whether any pre-use screen already forecasts final/current Faith or donation totals.
4. Current buff descriptions and buff-hover behavior for passive prayers.
5. Result-screen wording and whether it clearly separates base reward from prayer bonus.
6. Whether Souls prayer UI surfaces current Soul Gratitude's quantitative role.
7. Complete the broader mod inventory enough to support or reject a novelty claim.
8. Trace passive prayer duration application far enough to distinguish magnitude-vs-duration semantics safely.

## Design status

No design is accepted and no production DLL is justified yet.

The evidence now suggests three different-sized solution classes that must remain separate until the presentation audit is complete:

- if the confirmed problem is mainly `xN` notation, a very small wording clarification may be enough;
- if players lack current-state outcome comparison, a compact dynamic forecast/breakdown may be justified;
- if passive effects are the hidden part, a contextual quantitative effect line may solve more than changing the whole prayer panel.

Do not create a long tooltip, calculator panel, or production Harmony patch until the remaining presentation evidence distinguishes these cases.
