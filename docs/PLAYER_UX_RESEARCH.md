# Player UX Research — Prayer/Sermon System

Status: evidence map, updated 2026-09-14 after direct 1.407 presentation/mechanics probes 0.1.0–0.1.5 and external wiki/community cross-checking.

This document records **player-experience evidence and UX findings**. Authoritative mechanics live in `PRAYER_MECHANICS.md`; design options live in `DESIGN_NOTES.md`.

## Method

Community sources identify:

- what players cannot infer from the game;
- wording players misread;
- prayer comparisons that require outside arithmetic;
- hidden/unclear dependencies on church, graveyard, Soul Gratitude, prayer quality, perks, or buff duration;
- effects that require wiki tables, calculators, reload testing, or forum explanations.

A thread/comment is a **community signal**, not proof of mechanics. Direct 1.407 runtime/UI evidence decides whether the signal still applies. External disagreement is a re-check trigger, not authority over current runtime evidence.

## Strong current community signals

| Date | Source | Signal | Classification |
| --- | --- | --- | --- |
| 2026-06-09 | Reddit — `Is Prayer for Faith in bronze quality worth using?` | Player reads `Faith (x2)` as double base Faith and `Faith (x1) (+50%)` as 150% of base. | **community signal**, recent, notation-specific |
| 2026-03-13 | Reddit — `The Effect of "Prayer for Soul's Repose" is Secretly Capped?` | Player's inferred Souls formula does not match observed output until other users derive the hidden relation. | **community signal**; runtime independently confirms Soul Gratitude enters the Faith baseline |
| 2026-01-15 | Steam — `What does graveyard quality do` | Player believes Faith/money are both driven by church quality and asks where graveyard rating matters. | **community signal**, discoverability |
| 2026-04-12 | Steam — donation comparison discussion | Player tries to reconcile per-person donation animations with church/graveyard stats. | **community signal**; visible coins can teach the wrong mental model |
| 2026-04-20 | Reddit — prayer quality/buff strength question | Player asks whether a better prayer strengthens the buff or only extends it. | **community signal**, quality scaling |

Primary current-source URLs:

- https://www.reddit.com/r/GraveyardKeeper/comments/1u0z66v/is_prayer_for_faith_in_bronze_quality_worth_using/
- https://www.reddit.com/r/GraveyardKeeper/comments/1rsjmat/the_effect_of_prayer_for_souls_repose_is_secretly/
- https://steamcommunity.com/app/599140/discussions/0/780944762959117296/
- https://steamcommunity.com/app/599140/discussions/4/801218828360078237/
- https://www.reddit.com/r/GraveyardKeeper/comments/1sqeane/does_the_higher_tier_inspirational_books_give_a/

## Historical/supporting signals

Repeated older discussions show the same classes of confusion:

- 2021 Reddit: `x2 Faith` read as multiplication although the fixed component is additive;
- 2024 Reddit: Gold Faith vs Silver Combo comparison answered with an external calculator;
- 2021 Reddit: first-sermon mismatch answered with a gain calculator;
- 2023 Reddit: beginner asks which prayer to craft/use;
- 2025 Reddit: passive prayers feel unreliable because the player cannot observe the effect clearly;
- 2023 Steam: Soul's Repose comparison requires outside explanation;
- 2018 Steam: players explicitly say sermon effects are poorly explained.

Supporting URLs:

- https://www.reddit.com/r/GraveyardKeeper/comments/ma1hcy/
- https://www.reddit.com/r/GraveyardKeeper/comments/1d3svge/
- https://www.reddit.com/r/GraveyardKeeper/comments/qe9hhr/
- https://www.reddit.com/r/GraveyardKeeper/comments/18laao6/
- https://www.reddit.com/r/GraveyardKeeper/comments/1ij5rxj/
- https://steamcommunity.com/app/599140/discussions/0/4034726433730288491/
- https://steamcommunity.com/app/599140/discussions/0/1733212454825720761/

## Direct current-1.407 presentation audit

### Selection panel answers success, not outcome

`PrayCraftGUI.RedrawTextValues` shows:

- current church quality;
- selected sermon requirement;
- success chance.

Current RU strings:

- `Качество церкви: %1`;
- `Проповедь требует: %1`;
- `Шанс успеха: %1`;
- below threshold: `Попытка молитвы`;
- at/above threshold: `Молиться`.

Item descriptions append `%1 необходимо, чтобы гарантировать успех проповеди.`

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
| `b_plant` | Молитва о корнях и побегах | flavour only; no farming effect/duration and no indication that stock wiring is disconnected |
| `b_sins` | Молитва о покаянии | flavour only; no gameplay effect/duration and no indication that no consumer is detectable |
| `b_skull` | Молитва об упокоении | flavour/joke; no `+1 maximum Donkey corpse tier`, no duration |
| `b_sword` | Молитва о возмездии | no `+5 damage`, no duration |
| `b_shield` | Защитная молитва | no `+4 armor`, no duration |
| `b_pen` | Молитва воображения | no exact `craft_q=0.7` effect/duration |
| `b_star` | Молитва о совершенстве | no exact `craft_q=0.2` effect/duration |
| `b_village` | Молитва о процветании | explains Commercial Blessing purpose, not quantity/current reward |
| `b_souls` | Молитва за упокой душ | says more Soul Gratitude -> more Faith, no current result |
| `b_grat_points_incr` | Молитва о довольстве душ | states `+10%` Soul Gratitude, not duration/quality scaling |
| `b_sin_shard` | Молитва за тщательное очищение душ | states `x2` Sin Shards, not duration/quality scaling |

## Accepted / strongly supported UX findings

### A — `Faith (xN)` communicates the wrong arithmetic

**Actual mechanics:** fixed Faith output is flat additive. A separate `k_faith` creates the proportional bonus.

**Player interpretation:** both 2021 and 2026 examples read `x1/x2` as multiplication.

> Before choosing between prayers, the player can read `Faith (xN)` as a multiplier of base Faith even though that component is a flat additive reward, which can reverse the apparent ranking of prayer options.

**Status:** confirmed/strong.

### B — selection UI gives success chance but no current-state reward forecast

**Actual mechanics:** final rewards combine current church quality, graveyard quality, fixed/proportional prayer bonuses, perks, and for Souls prayer Soul Gratitude.

**Vanilla:** selection UI does not turn those inputs into current Faith/donation totals.

> At prayer-selection time, the player can see whether the sermon is likely to succeed but cannot see the resulting Faith/donation output for the current state, forcing outside arithmetic or experimentation for quantitative comparison.

**Status:** confirmed.

### C — church and graveyard quality drive different outputs, but the decision UI foregrounds only church quality

**Actual mechanics:** ordinary base Faith uses church quality; donations use graveyard quality; success also uses church quality.

**Vanilla:** selection shows church quality/requirement/chance but not current graveyard rating or donation baseline.

> The UI encourages a single “church quality -> sermon result” model even though Faith and donations have different primary inputs.

**Status:** strong current UX finding, supported by 2026 Steam questions.

### D — passive prayers hide magnitude and quality scaling

Verified examples:

- `b_sword`: +5 damage;
- `b_shield`: +4 armor;
- `b_skull`: +1 to the maximum corpse tier used by live Donkey corpse generation;
- `b_pen`: fixed `craft_q=0.7` quality input;
- `b_star`: fixed `craft_q=0.2` quality input.

Prayer `dur_parameter` overrides default buff duration. Common tier sets are:

- 18 / 36 / 54 minutes;
- 36 / 72 / 108 minutes.

Thus higher prayer quality can mean **same strength, longer duration**. Prayer for Repose is a direct example: magnitude remains +1 maximum tier while duration becomes 18/36/54 minutes.

> Before using or upgrading many passive prayers, the player cannot determine what the buff quantitatively does, how long it lasts, or whether prayer quality changes strength, duration, or both.

**Status:** confirmed/strong.

### E — Souls prayer discloses the dependency but not the current outcome

`b_souls` uses `church quality + Soul Gratitude` in its Faith baseline. Current description explicitly says more Gratitude gives more Faith.

> The dependency is disclosed, but the player still cannot see what the current Church Quality + Soul Gratitude state will actually produce.

**Status:** confirmed refined finding. Do not claim Soul Gratitude is completely hidden.

### F — Prayer of Repentance creates a timed buff with no detected gameplay consumer

Direct mechanics audit:

- all three `b_sins` tiers attach `buff_sins`;
- the buff exists and has duration;
- no game-code literal consumer was found;
- 180 loaded FlowCanvas graphs contained no `buff_sins` reference;
- GameBalance references were only the buff definition and the three prayer crafts.

External wiki/community reports also repeatedly describe the prayer as broken/no apparent effect.

> Vanilla flavour text gives the player no way to discover that the prayer's special timed buff has no detectable consumer, so an inert mechanic can look merely subtle or poorly explained.

**Status:** mechanics anomaly + confirmed UX risk. PrayerClarity must not invent/rebalance an effect without a separate product decision.

### G — Prayer for Shoots and Roots contains an effect formula that the prayer buff does not feed

Direct 1.407 evidence now closes the earlier external discrepancy:

- many plant/growth crafts contain `-0.2*WGOpar("buff_plant")` in craft time;
- prayer buff application writes `buff_plant=1` to the **player**;
- `WGOpar(name)` reads `SmartExpression._wgo.GetParam(name,0)`;
- CraftComponent evaluates growth `craft_time` with the **growing/workbench WGO** as `_wgo` and the player only as the character argument;
- no stock propagation path from player `buff_plant` to those growing WGOs was found.

Therefore the apparently intended `20%` growth-speed term is disconnected from the normal prayer-buff path in stock 1.407.

> The game presents a farming-themed prayer but gives no indication that its timed player buff and the growth-time formula use different parameter owners, leaving the special effect effectively inert in the inspected stock path.

**Status:** confirmed wiring mismatch / UX anomaly. This also explains why older wiki/community reports call the prayer broken despite the formula existing in balance data.

Do not silently display “20% faster growth” as though it currently works; doing so would make PrayerClarity less accurate than vanilla.

## Failure semantics are non-obvious

Direct `pray` graph evidence shows:

- failed sermon still delivers base Faith;
- failed sermon still delivers base donations in stock 1.407;
- prayer-specific Faith/money bonuses are removed;
- buff/item outputs are tied to the success-animation path.

Vanilla exposes chance but not this consequence model. A forecast should show success and failure outcomes rather than imply “failure = nothing”.

## DLC effects that are comparatively clear already

Better Save Soul descriptions are stronger than many base-game descriptions:

- Soul Contentment states `+10%`;
- Thorough Cleansing states `x2` Sin Shards;
- Soul's Repose says more Soul Gratitude gives more Faith.

The +10% implementation is now exact:

`GP_awarded = RoundToInt(GP_base * 1.1)` while the prayer buff is active.

Do not rewrite already-clear magnitude text merely for consistency. Useful additions are duration, quality scaling, and current-state output.

## Existing mod landscape — initial check

Current Nexus review found sermon-affecting mods but no obvious prayer-effect explanation/forecast mod:

- `Pray the Day Away` (updated 2026-05-19): frequency/repeatability/consumption/playback changes;
- `Give Me Moar` (updated 2026-06-20): configurable reward multipliers;
- `Not Just - Zombie Enhanced`: at-will sermon feature plus unrelated tooltips.

Sources:

- https://www.nexusmods.com/graveyardkeeper/mods/72
- https://www.nexusmods.com/graveyardkeeper/mods/70
- https://www.nexusmods.com/graveyardkeeper/mods/24

**Status:** useful negative evidence, not proof of novelty.

## External mechanics cross-check — 2026-09-14

Purpose: independent consistency check against the direct 1.407 runtime model.

### Broad agreement

The community wiki agrees with the runtime catalogue on the main requirements/coefficients and passive magnitudes checked here:

- Faith prayer: q 10/20/50, Faith +50/+100/+150%, donations +20%;
- Combo: q 15/30/60, Faith and donations +50/+100/+150%;
- Imagination: fixed +0.7 contribution, duration 18/36/54;
- Excellence: fixed +0.2 contribution, duration 18/36/54;
- Retribution: +5 damage, duration 36/72/108;
- Protection: +4 armor, duration 36/72/108;
- Soul Contentment: +10% Soul Gratitude, duration 36/72/108;
- Thorough Cleansing: x2 Sin Shards, duration 36/72/108.

The March 2026 Soul's Repose discussion independently derives `CQ/5 + (SG-CQ)/10`, algebraically `(CQ+SG)/10`, matching the direct 1.407 Souls baseline without Eloquence.

Sources:

- https://graveyardkeeper.fandom.com/wiki/Sermon
- https://graveyardkeeper.fandom.com/wiki/Prayer_for_faith
- https://graveyardkeeper.fandom.com/wiki/Combo_prayer
- https://graveyardkeeper.fandom.com/ru/wiki/Молитва_воображения
- https://graveyardkeeper.fandom.com/wiki/Prayer_for_excellence
- https://graveyardkeeper.fandom.com/wiki/Prayer_for_retribution
- https://graveyardkeeper.fandom.com/wiki/Prayer_for_protection
- https://graveyardkeeper.fandom.com/wiki/Effects
- https://www.reddit.com/r/GraveyardKeeper/comments/1rsjmat/the_effect_of_prayer_for_souls_repose_is_secretly/

### Repentance

The wiki still marks Prayer for Repentance `Broken – no apparent effect`; older Steam testing and a 2026 Reddit discussion also fail to establish a working special effect. A broad 1.301 release note says `The prayers are fixed`, but does not specifically prove Repentance functionality.

Direct 1.407 runtime evidence remains stronger: timed `buff_sins` exists, no consumer was found.

Sources:

- https://graveyardkeeper.fandom.com/wiki/Prayer_for_repentance
- https://steamcommunity.com/app/599140/discussions/0/1637542851358404514/
- https://www.reddit.com/r/GraveyardKeeper/comments/1t303jw/what_is_the_chance_per_day_of_getting_faith_from/
- https://store.steampowered.com/news/posts/?appids=599140&enddate=1603904514&feed=steam_community_announcements

The wiki page also has stale/internal duration disagreement; direct 1.407 runtime proves 18/36/54.

### Soul Contentment

External sources consistently describe +10% Soul Gratitude. The exact multiply/round order was absent from external sources, but probe 0.1.4 closed it directly: the +10% multiplier is applied before `Mathf.RoundToInt`.

### Shoots and Roots — discrepancy resolved

The wiki/community history says broken/no apparent effect, while current balance data visibly contains a `-20%` term. Probes 0.1.4–0.1.5 reconcile the contradiction: the formula exists, but it reads `buff_plant` from the growing WGO while the prayer writes the parameter to the player. No propagation route was found.

Sources:

- https://graveyardkeeper.fandom.com/wiki/Prayer_for_shoots_and_roots
- https://steamcommunity.com/app/599140/discussions/0/3190243624323744636/
- https://steamcommunity.com/app/599140/discussions/0/1734336452556299084/

The external “broken” observation is therefore consistent with current 1.407 wiring, while the Lazy Bear statement about intended reduced growth time is consistent with the presence of the dormant `-20%` formula.

### Prayer for Repose — external claim verified directly

External sources describe the effect as raising the corpse tier/range available from Donkey. Probe 0.1.5 captured the live `npc_donkey` graph and proved:

- `Tier min = body_min + add_body_min`;
- `Tier max = body_max + add_body_max`;
- `buff_skull` contributes `body_max=1` to player parameters.

Thus the prayer raises Donkey's **maximum corpse tier by exactly +1** while active, without raising the minimum. Quality changes duration 18/36/54, not magnitude.

Sources:

- https://graveyardkeeper.fandom.com/wiki/Prayer_for_repose
- https://www.reddit.com/r/GraveyardKeeper/comments/1lkyyyx/
- https://www.reddit.com/r/GraveyardKeeper/comments/1ty6zka/does_prayer_for_repose_increase_the_chance_that_a/
- https://steamcommunity.com/app/599140/discussions/0/1637542851358404514/

## White-box information target

Before committing, the player should be able to answer:

1. Will it succeed? — vanilla already does this well.
2. What does the prayer actually do?
3. What changes with prayer quality?
4. Which current inputs matter?
5. What will I get now on success?
6. If chance is below 100%, what do I still get on failure?
7. Is a special effect known to be disconnected/inert in stock 1.407?

The internal implementation may require formulas, but player presentation should prefer current results, concise breakdowns, and dependency hints over coefficient algebra.

## Design transition

The mechanics re-checks that previously blocked a complete prayer-by-prayer white-box prototype are now resolved:

- Soul Contentment rounding/order: closed;
- Prayer for Repose live Donkey consumer: closed;
- Shoots and Roots end-to-end wiring: closed as a stock parameter-scope mismatch;
- Prayer of Repentance: no gameplay consumer found, externally corroborated as apparently inert.

The mechanics evidence was sufficient to build the Clarity layer, and the resulting presentation has since been carried forward and runtime-accepted through PrayerClarity: Vanilla 1.0.25 and the shared presentation in Rebalanced 0.2.4.

There is currently no open UX-research gate in this document. New research should be driven by fresh player feedback or a concrete presentation problem observed in the stable siblings rather than by the historical pre-prototype checklist.