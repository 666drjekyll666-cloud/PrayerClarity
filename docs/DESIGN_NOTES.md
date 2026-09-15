# PrayerClarity — Design Notes

Status: product/design stage, 2026-09-15. Stock mechanics, presentation audit, community cross-check, quantitative power budget and the revised first non-production Rebalanced roster are sufficiently modeled. No gameplay implementation or numerical rebalance is accepted yet.

Canonical documents:

- `PRAYER_MECHANICS.md` — stock 1.407 truth;
- `PLAYER_UX_RESEARCH.md` — player/presentation evidence;
- `PRAYER_DESIGN_AUDIT.md` — role/design verdicts;
- `PRAYER_POWER_BUDGET.md` — full investment/opportunity-cost model;
- `PRAYER_REBALANCE_OPTIONS.md` — concrete current candidate roster;
- `PULPIT_REVEAL_UX.md` — accepted pre-sermon reward-reveal boundary.

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

Technology unlock, prayer item, pulpit choice and active-buff presentation are separate vanilla paths. Active timed buffs communicate essentially icon + remaining time, not quantitative meaning.

Build one mod-owned semantic model and render context-appropriate subsets:

- technology tree: role, why unlock it, quality progression;
- prayer item: exact static tier properties;
- pulpit: reward dependencies, exact success probability, prayer-owned success modifiers/fixed outputs, special output/effect, duration and relevant probabilities — but not the fully resolved current Faith/donation payout;
- active buff: the concrete meaning of the currently active prayer effect plus the game's existing remaining-time presentation.

Preview calculations must be side-effect free; never call `PrayLogics.CalculatePray` merely to render information. The exact side-effect-free calculator may remain available internally for correctness, tests, diagnostics and balance work even when its final payout totals are intentionally hidden from the default pulpit UI.

Where quality bonuses naturally saturate at a cap, Clarity should say so. Example: if a writing craft is already guaranteed at maximum quality, a stronger Imagination prayer should not pretend to improve it further.

### Accepted pulpit information model

Runtime testing through 0.1.14 established the useful semantic split and workable geometry, but the exact-number rendering policy was revised after UX review.

Keep:

1. vanilla `Church quality`;
2. vanilla `Sermon requires`;
3. exact sermon success probability inside the Clarity result presentation;
4. the semantic decomposition `guaranteed/base -> success-only prayer contribution -> special effect`;
5. exact intrinsic prayer mechanics, durations, caps, probabilities and fixed prayer-owned outputs.

Default pre-sermon rendering must now preserve the ceremony as the exact reward-reveal moment:

- **Guaranteed/base** explains the source relationship rather than showing the computed payout: base Faith is driven by Church Quality, base donations by Graveyard Quality, with verified BSS dependencies such as Soul Gratitude made explicit where relevant;
- **On success (N%)** shows the prayer's own modifiers such as Faith `+50%` or Donations `+25%`, plus fixed prayer-owned additions when present, rather than a computed `+2 Faith / +31c` delta;
- **Effect** continues to show exact prayer-owned effects such as duration, damage, armor, growth reduction, regeneration, confession probability, GP/Sin Shard multipliers or Blessing counts;
- do not replace real progression with qualitative `low / medium / high` buckets;
- do not add a separate failure row: failure is already represented by the guaranteed/base relationship.

The verified mechanics/forecast model itself is unchanged by this UX decision. Exact BaseFaith/BaseMoney and success deltas may still be calculated internally; the change is which fields the default renderer exposes.

If dependency relationships are visible directly in the Guaranteed row, the separate lower dependency note is redundant and should be removed unless runtime readability proves a need for it.

Detailed rationale and examples are in `PULPIT_REVEAL_UX.md`.

### Runtime layout evidence and current accepted pulpit geometry

The earlier layout iterations established the final implementation seam and a usable composition:

- **0.1.3:** fixed-column/multi-widget layout was rejected in runtime because its geometry drifted outside the pulpit.
- **0.1.4:** replacing that hierarchy with one stock `l_total_values` label plus `ResizeHeight` was also rejected. It overlapped the selected prayer/button and accumulated a downward offset on repeated redraws.
- **0.1.5:** fixed measured regions removed cumulative drift and repeated prayer switching stayed geometrically stable, but the composition remained crowded and the stock frame was too short.
- **0.1.6:** live Configuration Manager tuning was useful and applied without rebuilds, but its frame-extension implementation was rejected. Changing height could explode width and decorative pieces moved inconsistently.
- **0.1.7:** the second frame-resize attempt was rejected. It deterministically resized child artwork rather than the real window boundary. The user nevertheless found a workable internal composition by moving the prayer selector to the upper-right.
- **0.1.8:** effect wrapping and the information hierarchy improved, but runtime confirmed that the fixed stock frame was still too restrictive.
- **0.1.9–0.1.14:** the real window/container seam, sliced frame pieces and calibrated layout were implemented and iterated. The resulting 0.1.14 layout remained usable at 2560x1440 and passed a second-resolution 1920x1080 smoke with Russian, German and Japanese. Thin stock font rendering at 1080p is noted but is not a layout blocker.

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

A dedicated **Pulpit Frame Slice Probe 0.1.0** closed the real-window question:

- `window/back`: `UI2DSprite`, already `Sliced`, border `30/30/30/30`, `288x240`, TopLeft pivot;
- `window/decore_back`: `UI2DSprite`, already `Sliced`, border `15/15/15/15`, `270x202`, Bottom pivot;
- `window/header`: `UI2DSprite`, already `Sliced`, border `65/5/65/5`, `264x21`, Top pivot;
- `window/header/pixel line`: simple horizontal line, `264x2`;
- `window/decore` (`pulpit_bench_back`): `UI2DSprite` **Simple**, `244x186`; this decoration must not be stretched when the frame grows;
- the container's four anchors target the real `window`, so the native hierarchy already contains the semantic seam required for a proper resize.

Current accepted/calibrated defaults from the 0.1.14 line include real-window extra size `10 / 100`, result/effect/note positioning, Prayer selector Y `40`, and Note font size `10`. Prayer button X/Y remains a non-blocking experimental control rather than a reason to delay the product.

Presentation should remain icon-first where the game already has an unambiguous resource/stat icon. An icon may replace an obvious noun such as Faith, money, damage, armor, Sin Shards or Soul Gratitude; it should not replace explanatory relationships or turn a mechanic into a pictogram puzzle. Prosperity and Thorough Cleansing inline item icons were attempted through two seams and did not resolve in the 1.407 runtime; localized text is the accepted fallback rather than further icon-chasing.

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

The current `Writing quality` / `Affected craft quality` Clarity copy is therefore mechanically defensible but still **provisional UX wording**. A community/wiki cross-check describes those perks simply as improving `Quality`, but that is not a substitute for direct recovery of the current Russian/game localization. Before final release wording, prefer the exact in-game perk terminology if direct localization evidence is recovered.

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
- **BSS Soul's Repose:** stock mechanics, dependency-aware Faith presentation.
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

Broad research is done. The pulpit geometry has passed 2560x1440 and 1920x1080 multi-language smoke testing; further generic pulpit-layout probing is not justified.

The active Clarity work has moved to the two remaining player-facing surfaces:

1. Character -> Temporary Effects / active prayer buffs;
2. prayer-related Technology tooltip/unlock presentation.

Technology already has a promising native tooltip seam through `TechUnlock.GetTooltip(Tooltip)`. Temporary Effects has a separate Inventory/`PerkBuffItemGUI` path rather than the HUD `BuffIcon` path; the narrow read-only secondary-surface probe exists to close the exact binding/tooltip seam before production code.

The next coherent runtime candidate should therefore combine:

- the revised pulpit reward-reveal presentation from `PULPIT_REVEAL_UX.md`;
- removal of the redundant lower pulpit dependency note and dead inline-item-icon path;
- the two secondary Clarity surfaces once their exact UI seams are closed;
- no Vanilla Fixes or Balance/Rework mechanics yet.

Cross-cutting contracts remain:

- one pure semantic/forecast model;
- side-effect-free preview calculations;
- full 11-language localization with English fallback;
- no hard-coded player-facing prose in patch code;
- no polling or broad UI scans;
- unaccepted runtime behavior remains off `main`.
