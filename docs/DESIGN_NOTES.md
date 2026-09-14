# PrayerClarity — Design Notes

Status: product/design stage, 2026-09-15. Stock mechanics, presentation audit, community cross-check, quantitative power budget and the revised first non-production Rebalanced roster are sufficiently modeled. No gameplay implementation or numerical rebalance is accepted yet.

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

### Runtime layout evidence and current 0.1.9 real-window hypothesis

The information model is accepted; final layout is still being calibrated.

- **0.1.3:** fixed-column/multi-widget layout was rejected in runtime because its geometry drifted outside the pulpit.
- **0.1.4:** replacing that hierarchy with one stock `l_total_values` label plus `ResizeHeight` was also rejected. It overlapped the selected prayer/button and accumulated a downward offset on repeated redraws.
- **0.1.5:** fixed measured regions removed cumulative drift and repeated prayer switching stayed geometrically stable, but the composition remained crowded and the stock frame was too short.
- **0.1.6:** live Configuration Manager tuning was useful and applied without rebuilds, but its frame-extension implementation was rejected. Changing height could explode width and decorative pieces moved inconsistently.
- **0.1.7:** the second frame-resize attempt was rejected. It deterministically resized child artwork rather than the real window boundary. The user nevertheless found a workable internal composition by moving the prayer selector to the upper-right.
- **0.1.8:** effect wrapping and the information hierarchy improved, but runtime confirmed that the fixed stock frame is still too restrictive to finalize the intended layout. The user explicitly requested true window sizing before final placement is accepted. Prosperity still exposed an internal localization ID, Soul's Repose remained blank, and the expected Sin Shard cue did not appear; these are presentation/resolver defects, not evidence that the underlying mechanics are unknown.

The original narrow layout probe established stock 1.407 geometry and inline symbols:

- actual `UI Root/Pray GUI/window`: `UIWidget`, `274x241`, center pivot;
- anchored `window/container`: `UIWidget`, `274x199`;
- `l_total_values`: local `(-3,49)`, `242x68`, center pivot, 16 px font, `spacingY=-3`, `ShrinkContent`;
- prayer button: window-local `y=-85`;
- controller tips: window-local `y=-136`;
- `(faith)` — Faith;
- `(cross)` — church-quality cross;
- `(wskull)` — green-wreath graveyard-quality skull;
- `(skull)` — ordinary white skull;
- `(gld)`, `(slv)`, `(brz)` — money denominations;
- `(gratitude_points)` — Soul Gratitude;
- `(up)` — native small green up arrow.

The 0.1.4 probe observed Y progress `49 -> 35 -> 21 -> 7 -> -7 -> -21 -> -35` after the stock center-pivot label had been converted to 170/208 px `ResizeHeight`. 0.1.5 eliminated that mutable stock-label geometry path, and later repeated-switch tests confirmed the cumulative drift no longer occurs.

A dedicated **Pulpit Frame Slice Probe 0.1.0** then closed the real-window question:

- `window/back`: `UI2DSprite`, already `Sliced`, border `30/30/30/30`, `288x240`, TopLeft pivot;
- `window/decore_back`: `UI2DSprite`, already `Sliced`, border `15/15/15/15`, `270x202`, Bottom pivot;
- `window/header`: `UI2DSprite`, already `Sliced`, border `65/5/65/5`, `264x21`, Top pivot;
- `window/header/pixel line`: simple horizontal line, `264x2`;
- `window/decore` (`pulpit_bench_back`): `UI2DSprite` **Simple**, `244x186`; this decoration must not be stretched when the frame grows;
- the container's four anchors target the real `window`, so the native hierarchy already contains the semantic seam required for a proper resize.

This proves that the next experiment should resize the actual `window`/`container` and the already-sliced frame pieces. It should **not** introduce a custom replacement texture, stretch `pulpit_bench_back`, or iterate another child-art-only workaround.

**Latest user calibration at 2560x1440 from the 0.1.8 runtime test:**

- Context X/Y/font: `8 / 72 / 14`;
- Result heading X/Y/font: initial v4 split `8 / 20 / 14`;
- Result rows X/Y/font: initial v4 split `16 / 4 / 14`;
- Effect X/Y/font/icon: `-122 / -30 / 12 / 10`;
- Note X/Y/font: `-6 / -87 / 9`;
- Prayer selector X/Y: `70 / 45`;
- Prayer button X/Y: `0 / -120`.

**0.1.9 presentation/geometry hypothesis:**

- expose live `Window extra width` / `Window extra height` controls that modify the real root `window` and anchored `container` from captured vanilla dimensions;
- resize `back`, `decore_back` and `header` using their verified native sliced configuration; do not stretch `pulpit_bench_back`;
- move header/close controls and controller tips with the corresponding new frame edges;
- calculate every live size/position from captured vanilla geometry so repeated slider changes cannot accumulate offsets;
- keep the Configuration Manager controls temporary calibration instrumentation rather than product settings;
- split the `Result` heading into its own label with independent X/Y/font controls; keep Guaranteed and success rows separately tunable;
- shorten `Additional on success (N%)` to a language-appropriate equivalent of `On success (N%)` so resource values retain horizontal space;
- keep `(up)` only on the resource specifically improved by Faith or Donations; Ordinary, Combo and non-resource specialists show no specialist arrow in the resource row;
- use a player-facing Repose sentence based on the Donkey bringing a higher-quality body, with native `(up)` + `(skull)` cues, without advertising future Rebalanced reliability tiers;
- fix game-owned localization reuse at the resolver boundary: direct audit places `GJL` in `Assembly-CSharp-firstpass`, so vanilla localization must resolve across loaded assemblies and cache the actual `GJL.L(string)` method rather than searching Assembly-CSharp only;
- Prosperity should therefore render the vanilla-localized Blessing of Commerce name/description rather than `blessing_commerce`;
- Soul's Repose should render the vanilla `b_souls_d` description rather than an empty Effect row;
- Thorough Cleansing should request verified `i_sin_shard`; sprite lookup is lazy and session-cached after the first successful resolve, with no per-frame search.

Once the real-window composition is accepted, perform at least one second-resolution check before freezing geometry or deciding whether any tuning control should remain public.

Presentation should remain icon-first where the game already has an unambiguous resource/stat icon. An icon may replace an obvious noun such as Faith, money, damage, armor, Sin Shards or Soul Gratitude; it should not replace explanatory relationships or turn a mechanic into a pictogram puzzle.

## Localization architecture

PrayerClarity must ship all mod-owned player-facing text for the full 11-language interface set supported by Graveyard Keeper: English, French, German, Simplified Chinese, Spanish (Spain), Portuguese (Brazil), Korean, Japanese, Russian, Italian and Polish.

Implementation direction:

- read the active game language from the game's own language state (`GameSettings._cur_lng` is the established current seam in the existing GK mod ecosystem);
- normalize casing and `-`/`_` separators so variants such as `pt-BR`/`pt_BR` and `zh-CN`/`zh_CN` resolve to one locale;
- use compact mod-owned key/value localization resources with English fallback;
- keep dynamic values out of translated source strings except as explicit placeholders;
- reuse vanilla localized prayer names, resources and game terminology where practical rather than maintaining duplicate translations of game-owned text;
- direct 1.407 audit places `GJL` in `Assembly-CSharp-firstpass`; resolve and cache its `L(string)` method through the small compatibility boundary rather than assuming it lives in Assembly-CSharp;
- refresh on an existing language/UI lifecycle boundary or lazily during relevant UI redraw, not through polling;
- missing locale/key must fail visibly and safely to English rather than showing a blank or corrupting UI.

The first Clarity prototype is localization-complete only when every new visible phrase used by that prototype exists in all 11 language resources. Translation quality can be refined later without changing mechanics, but unsupported-language placeholders are not an acceptable release state.

### Imagination / Excellence terminology

The direct mechanics are closed: `buff_pen` contributes `craft_q=0.7` to writing-linked multiquality recipes and `buff_star` contributes `craft_q=0.2` to explicitly linked crafts in stock 1.407. The same additive quality-score bucket is also used by perks such as Writer, Playwright/Good Writer, Jeweler and Industriousness.

The current `Writing quality` / `Affected craft quality` Clarity copy is therefore mechanically defensible but still **provisional UX wording**. A community/wiki cross-check describes those perks simply as improving `Quality`, but that is not a substitute for direct recovery of the current Russian/game localization. Before final release wording, prefer the exact in-game perk terminology if direct localization evidence is recovered. Do not delay the pulpit geometry iteration for this copy refinement.

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

The current implementation slice remains intentionally **Clarity-first** and does not change prayer mechanics. The forecast seam and `guaranteed + success bonus + special effect` information model are verified. The frame-slice probe has now closed the window-resize implementation target: stock 1.407 already provides a real root `window`, anchored `container`, and sliced frame pieces that can be resized without inventing replacement artwork.

The narrow gate is therefore **0.1.9 real-window runtime verification**. It must prove that live F1 width/height controls enlarge the actual pulpit frame without stretching `pulpit_bench_back`, exploding the opposite dimension, or accumulating offsets; then the user can calibrate the separate Result heading/rows and retest Repose, Prosperity, Soul's Repose and Thorough Cleansing. If this geometry is accepted, freeze the layout defaults and perform one second-resolution check before expanding Clarity beyond the pulpit.

Cross-cutting contracts remain:

- one pure semantic/forecast model;
- side-effect-free preview calculations;
- pulpit redraw as the first UI boundary;
- full 11-language localization with English fallback;
- no hard-coded player-facing prose in patch code;
- no polling or broad UI scans.
