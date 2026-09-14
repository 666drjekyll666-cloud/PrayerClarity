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
- pulpit: current success/failure Faith, donations, special output/effect, duration and relevant probabilities;
- active buff: optional later quantitative hover.

Preview calculations must be side-effect free; never call `PrayLogics.CalculatePray` merely to render information.

Where quality bonuses naturally saturate at a cap, Clarity should say so. Example: if a writing craft is already guaranteed at maximum quality, a stronger Imagination prayer should not pretend to improve it further.

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
- **Soul Contentment:** stock initially.
- **Thorough Cleansing:** stock x2 Sin Shards.

Full modeling and fallback options are in `PRAYER_REBALANCE_OPTIONS.md`.

## Important revisions from the prior roster

### Shoots & Roots

The -20% stock coefficient remains the evidence-backed Vanilla Fix. Rebalanced may legitimately scale the actual growth reduction by prayer quality to 20/30/40%, giving increasingly strong throughput rather than duration-only quality.

### Repose

Do not use a fixed 90% silver value globally. Vanilla top-tier probability itself changes with the current corpse-tier pool. Silver should be **mathematically halfway between whatever vanilla currently gives and 100%**:

`P_silver = (P_vanilla + 100%)/2`.

Gold is certainty. This preserves a meaningful gold upgrade at every progression state.

### Imagination / Excellence

Direct multiquality evidence confirms linked perk stars and prayer `craft_q` contributions are additive and outputs have finite quality tiers. Therefore late-game saturation is real rather than hypothetical.

Imagination lead changes to **0.5/0.7/1.0**: stock +0.7 becomes silver rather than bronze, while gold adds a full quality-score point.

Excellence lead changes to **0.2/0.5/1.0**. Because it affects a narrower craft set, a gold prayer that makes a reachable high-quality result deterministic is considered a desirable payoff, not an automatic balance failure.

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

The next implementation slice is intentionally **Clarity-first**: it must not change prayer mechanics. Its purpose is to validate whether the pulpit can communicate verified prayer outcomes clearly enough at the actual decision point.

Before writing that slice, the required cross-cutting contracts are now fixed:

- one pure semantic/forecast model;
- side-effect-free preview calculations;
- pulpit redraw as the first UI boundary;
- full 11-language localization with English fallback;
- no hard-coded player-facing prose in patch code;
- no polling or broad UI scans.

Next:

1. open a `dev/*` branch from the current research baseline;
2. scaffold the smallest production plugin and localization layer;
3. implement the shared semantic/forecast model only for mechanics already directly evidenced;
4. render a concise localized Clarity block on the pulpit selection surface;
5. keep Combat, Roots, Repose, Repentance and other not-yet-closed rework hooks out of this first Clarity-only prototype;
6. run hosted CI only when the coherent candidate reaches a compile/handoff boundary;
7. build one numbered candidate when it is ready for an actual in-game acceptance test.

No user runtime action is currently required.