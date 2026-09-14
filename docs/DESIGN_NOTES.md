# PrayerClarity — Design Notes

Status: product/design stage, 2026-09-14. Stock mechanics, presentation audit, community cross-check, quantitative power budget and the revised first non-production Rebalanced roster are sufficiently modeled. No gameplay implementation or numerical rebalance is accepted yet.

Canonical documents:

- `PRAYER_MECHANICS.md` — stock 1.407 truth;
- `PLAYER_UX_RESEARCH.md` — player/presentation evidence;
- `PRAYER_DESIGN_AUDIT.md` — role/design verdicts;
- `PRAYER_POWER_BUDGET.md` — full investment/opportunity-cost model;
- `PRAYER_REBALANCE_OPTIONS.md` — concrete current candidate roster.

## Product architecture

One codebase may expose three semantic layers:

1. **Clarity** — information only;
2. **Vanilla Fixes** — evidence-backed repair where stock intent/magnitude are recoverable;
3. **Balance / Rework** — explicit new design/tuning.

Potential profiles remain `Vanilla + Clarity`, `Fixed Vanilla`, and `Rebalanced`. Stock behavior remains separately documented.

## Permanent policies

- Do not nerf failed-sermon base donations. Preserve full base donations on failure in every profile; only prayer-specific bonuses/special success outputs disappear.
- Use **temptation parity**: a prayer must justify unlock/craft/quality cost plus the weekly sermon opportunity.
- Bronze must already be credible; silver/gold may deliberately buy magnitude, reliability, output, duration or certainty.
- Strong gold behavior is acceptable when bounded by a narrow role and finite game caps.

## Presentation architecture

Technology unlock, prayer item, pulpit choice and active buff HUD are separate vanilla paths. Active timed buffs communicate essentially icon + remaining time, not quantitative meaning.

Build one mod-owned semantic model and render context-appropriate subsets:

- technology tree: role, why unlock it, quality progression;
- prayer item: exact static tier properties;
- pulpit: current guaranteed Faith/donations, success-only additions, special output/effect, duration and relevant probabilities;
- active buff: optional later quantitative hover.

Preview calculations must be side-effect free; never call `PrayLogics.CalculatePray` merely to render information.

Where quality bonuses naturally saturate at a cap, Clarity should say so. Example: if a writing craft is already guaranteed at maximum quality, a stronger Imagination prayer should not pretend to improve it further.

### Accepted pulpit information model

Runtime testing through 0.1.2 established that the useful information model is:

1. keep vanilla `Church quality`;
2. keep vanilla `Sermon requires`;
3. suppress the detached vanilla `Success chance` row once a Clarity forecast is available;
4. show a **guaranteed** row containing the base Faith and donations received regardless of success;
5. show a **success bonus (N%)** row containing only the additional Faith/donations supplied on sermon success;
6. show special prayer effects separately from the two resource rows.

Do not add a separate failure row: failure is already represented by the guaranteed row. This teaches the important stock rule that sermon failure is not zero reward while keeping the result model compact.

### Runtime layout evidence and current 0.1.8 fixed-window hypothesis

The information model is accepted; layout is still being calibrated.

- **0.1.3:** fixed-column/multi-widget layout was rejected in runtime because its geometry drifted outside the pulpit.
- **0.1.4:** replacing that hierarchy with one stock `l_total_values` label plus `ResizeHeight` was also rejected. It overlapped the selected prayer/button and accumulated a downward offset on repeated redraws.
- **0.1.5:** fixed measured regions removed cumulative drift and repeated prayer switching stayed geometrically stable, but the composition remained crowded and the stock frame was too short.
- **0.1.6:** live Configuration Manager tuning was useful and applied without rebuilds, but its frame-extension implementation was rejected. Changing height could explode width and decorative pieces moved inconsistently.
- **0.1.7:** the second frame-resize attempt is also rejected. Forcing NGUI aspect ratio to `Free` and using `UIWidget.SetDimensions` made the child sprites deterministic, but runtime showed the important distinction: those child widgets are artwork inside the pulpit, not the real window boundary. Width/height tuning therefore stretches the inner picture instead of enlarging usable pulpit space. Do not iterate a third sprite-stretch variant.

A narrow read-only layout probe established stock 1.407 geometry:

- actual `UI Root/Pray GUI/window` is itself a `UIWidget` around `274x241`;
- `l_total_values`: local `(-3,49)`, `242x68`, center pivot, 16 px font, `spacingY=-3`, `ShrinkContent`;
- selected prayer cell: the middle region below the stock text block;
- prayer button: window-local `y=-85`;
- controller tips: window-local `y=-136`;
- the visible `back`, `decore_back`, `decore` and related pieces are separate child sprites. Resizing them changes artwork geometry, not the window's usable layout contract.

The same stock bitmap font exposes the symbols needed by the presentation:

- `(faith)` — Faith;
- `(cross)` — church-quality cross;
- `(wskull)` — green-wreath graveyard-quality skull;
- `(skull)` — ordinary white skull;
- `(gld)`, `(slv)`, `(brz)` — money denominations;
- `(gratitude_points)` — Soul Gratitude;
- `(up)` — native small green up arrow.

The 0.1.4 probe observed Y progress `49 -> 35 -> 21 -> 7 -> -7 -> -21 -> -35` after the stock center-pivot label had been converted to 170/208 px `ResizeHeight`. 0.1.5 eliminated that mutable geometry path, and later repeated-switch tests confirmed the cumulative drift no longer occurs.

**Accepted 0.1.7 calibration baseline at 2560x1440:** the user found a workable composition inside the unchanged stock window by moving the prayer selector to the upper-right and using the remaining area for forecast text. Freeze these as the next candidate defaults, but do not call them cross-resolution accepted yet:

- Context X/Y/font: `8 / 72 / 12`;
- Result X/Y/font: `8 / 20 / 13`;
- Effect X/Y/font/icon: `-122 / -35 / 10 / 10`;
- Note X/Y/font: `-6 / -87 / 9`;
- Prayer selector X/Y: `90 / 55`;
- Prayer button X/Y: `0 / -120`.

**0.1.8 presentation hypothesis:** stop resizing the frame and harden only the successful internal layout:

- remove `Window extra width/height` controls and all frame/decor mutation code;
- use a fresh `Prototype pulpit layout tuning v3` section so rejected v2 values do not override the new baseline;
- keep live controls only for context/result/effect/note, prayer selector and prayer button;
- make the mod-owned effect label `TopLeft + ResizeHeight` with fixed width. This gives localization-safe line wrapping while avoiding the rejected 0.1.4 failure mode: only the new effect label resizes downward; the stock center-pivot context label is not resized;
- keep `(up)` only on the actual specialist resource: Faith before Faith, Donations before money, no arrow for Ordinary/Combo/non-resource specialists;
- improve stock Repose wording without pretending that quality already changes reliability. In Clarity-only stock behavior, all qualities still expose the same `body_max +1`; quality-specific possible/likely/guaranteed wording belongs only to the future Rebalanced implementation;
- use the proven inline `(skull)` symbol for Repose rather than a generic buff icon;
- Prosperity should explain the stock physical output using vanilla terminology: `Blessing of commerce` can be sold to a merchant to raise that merchant's level;
- identify BSS Soul's Repose by its verified `pray_for_souls_*` event family, not a guessed craft-name spelling, so its vanilla localized effect text actually renders;
- Soul Contentment grammar should read `Effect: +10% (gratitude_points) ...`, not icon -> `Effect` -> second icon;
- direct 1.407 balance data proves `i_sin_shard` is the Sin Shard art used by Sin Shard body-part crafting rows, while the `sin_shard` ItemDefinition itself has blank icon fields. Thorough Cleansing should therefore use `i_sin_shard` explicitly.

The Configuration Manager controls remain **temporary calibration instrumentation**, not automatically a final user setting surface. Once the fixed-window composition and wrapping are accepted, perform at least one second-resolution check before deciding whether any layout setting should remain public.

Presentation should remain icon-first where the game already has an unambiguous resource/stat icon. An icon may replace an obvious noun such as Faith, money, damage, armor, Sin Shards or Soul Gratitude; it should not replace explanatory relationships or turn a mechanic into a pictogram puzzle.

## Localization architecture

PrayerClarity must ship all mod-owned player-facing text for the full 11-language interface set supported by Graveyard Keeper: English, French, German, Simplified Chinese, Spanish (Spain), Portuguese (Brazil), Korean, Japanese, Russian, Italian and Polish.

Implementation direction:

- read the active game language from the game's own language state (`GameSettings._cur_lng` is the established current seam in the existing GK mod ecosystem);
- normalize casing and `-`/`_` separators so variants such as `pt-BR`/`pt_BR` and `zh-CN`/`zh_CN` resolve to one locale;
- use compact mod-owned key/value localization resources with English fallback;
- keep dynamic values out of translated source strings except as explicit placeholders;
- reuse vanilla localized prayer names, resources and game terminology where practical rather than maintaining duplicate translations of game-owned text;
- refresh on an existing language/UI lifecycle boundary or lazily during relevant UI redraw, not through polling;
- missing locale/key must fail visibly and safely to English rather than showing a blank or corrupting UI.

The first Clarity prototype is localization-complete only when every new visible phrase used by that prototype exists in all 11 language resources. Translation quality can be refined later without changing mechanics, but unsupported-language placeholders are not an acceptable release state.

### Imagination / Excellence terminology

The direct mechanics are closed: `buff_pen` contributes `craft_q=0.7` to writing-linked multiquality recipes and `buff_star` contributes `craft_q=0.2` to explicitly linked crafts in stock 1.407. The same additive quality-score bucket is also used by perks such as Writer, Playwright/Good Writer, Jeweler and Industriousness.

The current `Writing quality` / `Affected craft quality` Clarity copy is therefore mechanically defensible but still **provisional UX wording**. A community/wiki cross-check describes those perks simply as improving `Quality`, but that is not a substitute for direct recovery of the current Russian/game localization. Before final release wording, prefer the exact in-game perk terminology if direct localization evidence is recovered. Do not delay the 0.1.8 layout test for this copy refinement.

## Revised leading Rebalanced roster

These remain design hypotheses pending runtime acceptance:

- **Ordinary:** stock; free starter is not buffed.
- **Faith:** +100/+200/+300% target Faith.
- **Donations:** +100/+200/+300% target donations.
- **Combo:** stock +50/+100/+150% both.
- **Repentance:** 30/50/70% confession chance.
- **Shoots & Roots:** Fixed Vanilla repairs stock -20%; Rebalanced scales **-20/-30/-40% growth time**.
- **Combat:** save-safe Retribution/Protection soft merge; +5/+8/+12 damage, +4 armor, regen **1 HP every 3/2/1.5 sec**, duration 36/72/108 min.
- **Repose:** bronze stock random selection from max+1 pool; silver exactly halfway from current vanilla best-tier chance to certainty; gold 100% best prayer-eligible tier; ceiling remains story max+1.
- **Imagination:** +0.5/+0.7/+1.0 writing-quality input, 18/36/54 min.
- **Excellence:** +0.2/+0.5/+1.0 linked-craft quality input, 18/36/54 min.
- **Prosperity:** stock 1/2/3 Blessings.
- **BSS Soul's Repose:** stock mechanics, dynamic Faith forecast.
- **Soul Contentment:** stock initially; new **+20/+40/+60% Soul Gratitude** curve is a user-proposed design hypothesis pending BSS economy modeling.
- **Thorough Cleansing:** stock x2 initially; new **x2/x3/x4 Sin Shards** curve is a user-proposed design hypothesis pending BSS economy modeling.

Full modeling and fallback options are in `PRAYER_REBALANCE_OPTIONS.md`.

## Important revisions from the prior roster

### Shoots & Roots

The -20% stock coefficient remains the evidence-backed Vanilla Fix. Rebalanced may legitimately scale the actual growth reduction by prayer quality to 20/30/40%, giving increasingly strong throughput rather than duration-only quality.

### Repose

Do not use a fixed 90% silver value globally. Vanilla top-tier probability itself changes with the current corpse-tier pool. Silver should be **mathematically halfway between whatever vanilla currently gives and 100%**:

`P_silver = (P_vanilla + 100%)/2`.

Gold is certainty. This preserves a meaningful gold upgrade at every progression state.

The future player-facing phrasing may use a simple reliability ladder such as bronze `possible`, silver `very likely`, gold `guaranteed`, but only after that Rebalanced behavior exists. Clarity-only stock text must not advertise those future probabilities.

### Imagination / Excellence

Direct multiquality evidence confirms linked perk stars and prayer `craft_q` contributions are additive and outputs have finite quality tiers. Therefore late-game saturation is real rather than hypothetical.

Imagination lead changes to **0.5/0.7/1.0**: stock +0.7 becomes silver rather than bronze, while gold adds a full quality-score point.

Excellence lead changes to **0.2/0.5/1.0**. Because it affects a narrower craft set, a gold prayer that makes a reachable high-quality result deterministic is considered a desirable payoff, not an automatic balance failure.

These quality-specific values belong to Rebalanced. The current Clarity prototype correctly reports stock `+0.7` Imagination and stock `+0.2` Excellence for every prayer quality.

### Combat regeneration

The rejected 1 HP/min concept did not meet temptation parity. Vanilla long-heal potion heals 1 HP every 1.5 sec. The first Combat Prayer candidate therefore uses **3/2/1.5 sec** ticks across bronze/silver/gold. Gold reaches an existing potion cadence but lasts much longer and is bundled with damage/armor; the resulting power should be judged in runtime rather than pre-nerfed into irrelevance.

## Specialist/generalist policy

Do not nerf Combo first. Faith and Donations become true specialists while Combo stays a familiar generalist.

Church quality remains the universal sermon-success gate. Graveyard Quality already creates the donation base, so do not add a second GQ gate until runtime evidence shows it is needed.

## Broken-prayer policy

### Shoots and Roots

Vanilla Fix = reconnect stock -20% only. Any 30/40% quality scaling exists only in Rebalanced.

### Repentance

Balance/Rework = role known, magnitude lost. Current first candidate is 30/50/70%.

## Combat migration policy

If merge is implemented:

- existing `b_sword` becomes canonical Combat Prayer;
- existing `b_shield` remains in saves as same-quality legacy alias;
- no destructive save migration;
- hide redundant new recipe only after lifecycle verification;
- uninstall/profile change leaves vanilla save structurally valid.

## Current gate

Broad research is done. No additional general probe or community search is justified before implementation-target work.

The current implementation slice remains intentionally **Clarity-first** and does not change prayer mechanics. The forecast seam and `guaranteed + success bonus + special effect` information model are already verified. 0.1.7 proved that the second child-sprite frame resize still does not enlarge the real pulpit window, but it also produced the first useful user-calibrated fixed-window composition. The narrow gate is therefore **0.1.8 fixed-window verification**: start from the accepted 2560x1440 coordinates, remove frame resizing entirely, verify localization-safe effect wrapping, and retest the clarified Repose/Prosperity/BSS effect rows. If that composition is accepted, freeze layout defaults and perform one second-resolution check before expanding Clarity beyond the pulpit.

Cross-cutting contracts remain:

- one pure semantic/forecast model;
- side-effect-free preview calculations;
- pulpit redraw as the first UI boundary;
- full 11-language localization with English fallback;
- no hard-coded player-facing prose in patch code;
- no polling or broad UI scans.
