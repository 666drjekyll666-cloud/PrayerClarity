# Prayer Design Audit — Graveyard Keeper 1.407

Status: design audit, 2026-09-14. Stock mechanics are verified separately; quantitative power-budget and candidate roster work now follow this audit. No balance/rework numbers are implemented or accepted yet.

This document evaluates prayer design after the stock 1.407 mechanics were recovered. `docs/PRAYER_MECHANICS.md` remains canonical for what vanilla actually does. Quantitative full-cost comparison is in `docs/PRAYER_POWER_BUDGET.md`; concrete non-accepted roster candidates are in `docs/PRAYER_REBALANCE_OPTIONS.md`.

## Product layers

The project keeps three semantic layers separate even if they eventually ship in one DLL:

- **Clarity** — describe the effective mechanics truthfully without changing them.
- **Vanilla Fixes** — repair a proven stock defect only when the intended behavior and magnitude are recoverable from evidence.
- **Balance / Rework** — intentional design changes to functioning mechanics, or replacement mechanics where vanilla reveals the role but not the missing magnitude.

Possible user profiles remain `Vanilla + Clarity`, `Fixed Vanilla`, and `Rebalanced`.

## Permanent product decision — do not nerf failed-sermon base donations

Stock 1.407 passes a nominal `0.5` visitor-selection chance to failed-sermon donations, but the helper uses integer `Random.Range(0,1)` and therefore still selects every visitor. The player keeps the full **base** donation pool on failure while prayer-specific bonuses disappear.

**Accepted policy:** preserve this player-favourable outcome in every project profile. Do not repair the apparent 50% path in a way that reduces player rewards.

This is a product decision, not a claim about original developer intent.

## Core balance principle — every prayer should be tempting

A prayer is not merely a buff value. The player pays several costs before receiving its benefit:

1. **technology investment** — the relevant technology and prerequisites must be unlocked;
2. **craft investment** — prayer recipes consume scarce writing components and Faith, especially early/mid-game;
3. **quality investment** — silver/gold prayers require better writing materials and/or books;
4. **church-quality / success gate** — higher-quality prayers need a stronger church to be reliable;
5. **weekly opportunity cost** — one sermon choice displaces every other prayer for that week.

Therefore the design target is **temptation parity, not numerical parity**:

> when the player unlocks or considers crafting a prayer, it should present a compelling reason to want it at the stage where its role is relevant.

A niche prayer may legitimately be more powerful in its niche than a universal prayer. A progression prayer may legitimately become obsolete after serving its progression role. What is undesirable is a prayer that is expensive to unlock/craft and still produces no convincing moment where the player wants to choose it.

## Quality progression principle

Bronze should already be a credible prayer, not merely a prerequisite for the version the player actually wants.

Silver/gold should provide a meaningful reason for further investment. That can be through:

- stronger magnitude;
- longer duration;
- more physical outputs;
- better reward coefficients;
- crossing weekly boundaries so the buff remains active when the next sermon becomes available;
- or a combination of these.

Do **not** require every prayer quality tier to scale the same way. Existing 36/72/108-minute effects are a useful pattern: higher quality can reduce the effective weekly opportunity cost because a long buff survives into later sermon weeks.

However, if extra duration does not materially improve the prayer's real use case, duration-only quality progression may still be insufficient.

## Generalist vs specialist principle

The quantitative audit has now established the key structural issue more strongly than the earlier qualitative analysis:

- Faith prayer target `k_faith` = `.5 / 1 / 1.5`;
- Combo target `k_faith` = `.5 / 1 / 1.5`;
- Donations target `k_money` = `.5 / 1 / 1.5`;
- Combo target `k_money` = `.5 / 1 / 1.5`;
- Donations and Combo are unlocked by the **same** `Price of faith` technology.

The specialists therefore are not actually stronger specialists at equal quality. Their remaining advantages are production/progression gates:

- Chapter +5 Faith versus Combo Hard Book +7 Faith;
- q10/20/50 versus Combo q15/30/60.

Those are meaningful early-game advantages, but once book production and church quality are routine, the generalist keeps equal target-resource power while also supplying the second resource.

**Design consequence:** a specialist premium is now the leading systemic rework direction. Prefer testing stronger Faith/Donation specialist curves while keeping Combo useful and familiar before considering direct Combo nerfs.

Candidate curves live in `PRAYER_REBALANCE_OPTIONS.md`; no values are accepted yet.

## Church/graveyard requirements as design levers

Higher church-quality requirements are a natural balancing lever because sermon success already uses church quality and the UI already exposes that relationship.

Thematic requirements/scalers can also be useful where the relationship is obvious. Donations is the strongest candidate because graveyard quality already creates the money baseline.

Current preferred architecture is conservative:

- **church quality** remains the universal sermon-success/delivery gate;
- **graveyard quality** may gate/scale the Donations **specialist premium** or provide a visible thematic threshold;
- avoid making the same graveyard stat both a continuous reward multiplier and an opaque second success multiplier;
- do not add thematic requirements merely for symmetry.

A graveyard-only sermon success rule remains an option, not the preferred first experiment.

## Prayer-by-prayer design matrix

| Prayer family | Stock role / cost context | Design diagnosis | Current design direction |
| --- | --- | --- | --- |
| `b_empty` Ordinary | starter sermon | intentionally temporary baseline | **keep; clarity only** |
| `b_faith` Faith | Chapter +5 Faith; lower q than Combo; same Faith coefficient as Combo at equal tier | healthy early specialist, but loses output specialization once Combo's book/q gates are solved | **Rework candidate: specialist premium; keep cheaper craft/lower q identity** |
| `b_money` Donations | Chapter +5 Faith; same unlock as Combo; lower q; same money coefficient as Combo | clearest specialist/generalist compression because it is not even earlier-unlocked | **Rework candidate: specialist premium; optionally thematic Graveyard Quality condition/scaler** |
| `b_faith_money` Combo | Hard Book +7 Faith; q15/30/60; same target coefficients as both specialists | powerful universal default once production gate is routine | **keep as strong generalist initially; solve choice compression by specialist premium before nerfing** |
| `b_plant` Shoots and Roots | Chapter +5 Faith; special farming prayer | proven wiring defect disconnects existing `-20%` growth formula | **Vanilla Fix: reconnect stock -20%; runtime-test before extra buff** |
| `b_sins` Repentance | Chapter +5 Faith; q10/20/40; 18/36/54 min; base confession chance 15% | timed buff has no consumer; intended role known but magnitude absent | **Balance/Rework: quality-scaled confession chance; current candidate family includes 30/50/70%** |
| `b_skull` Repose | Hard Book +7 Faith; +1 Donkey max corpse tier, 18/36/54 min | clear progression accelerator, naturally expires at final corpse tier | **keep role; clarity first; only tune if usable window is too narrow for investment** |
| `b_sword` Retribution | deep Smithing route; Hard Book +7 Faith; +5 damage, 36/72/108 min | full investment/week cost is high for a one-dimensional combat offer, even though +5 itself is substantial | **Rework candidate: broader offensive package preferred over blind scalar** |
| `b_shield` Protection | same deep unlock; separate Hard Book +7 Faith; +4 armor, 36/72/108 min | second one-dimensional combat sermon paying the same full weekly cost | **Rework candidate: broader defensive package; preserve recognizable identity if possible** |
| `b_pen` Imagination | Hard Book +7 Faith; +0.7 writing input, 18/36/54 min | powerful concentrated production window; demonstrated player value | **keep; clarity only initially** |
| `b_star` Excellence | Hard Book +7 Faith; can be unlocked from two late production routes; +0.2 linked-craft input | narrow but potentially useful quality-crafting tool | **keep; clarity first; affected-craft usefulness decides later tuning** |
| `b_village` Prosperity | Chapter +5 Faith; permanent vendor-progression outputs | very strong progression utility, naturally exhausts itself | **keep; strong reference case** |
| `b_souls` Soul's Repose (BSS) | shared BSS unlock; Chapter +5 Faith +2 Sin Shards; state-dependent Faith | can beat ordinary/Combo Faith substantially at high Gratitude | **keep; dynamic forecast is main improvement** |
| `b_grat_points_incr` Soul Contentment | shared BSS unlock; Chapter +4 Faith +2 Sin Shards; +10% Gratitude | clear workflow-specific throughput niche | **keep first; clarity/quality duration** |
| `b_sin_shard` Thorough Cleansing | shared BSS unlock; Chapter +4 Faith +1 Sin Shard; x2 shards | deliberately powerful specialist effect; clear temptation-parity benchmark | **keep; strong reference case** |

## Repentance — final classification and current design pressure

The base confessional system uses a 15% roll. Successful confessional interactions observed in runtime logs pay 1 Faith plus a Story. The prayer creates `buff_sins`, but no surviving 1.407 consumer or intended multiplier exists.

Therefore a working Repentance is definitively **Balance / Rework**.

A flat `15% -> 30%` is useful as a bronze benchmark because it doubles the stock chance, but the power-budget analysis shows why it may be too modest as the whole prayer: finite 18/36/54-minute windows plus a full weekly sermon cost make quality-scaled chance a more promising design.

Current leading hypothesis in `PRAYER_REBALANCE_OPTIONS.md` is **30% / 50% / 70%**, not accepted. It keeps uncertainty while making silver/gold materially better.

## Combat prayers — structural diagnosis

Retribution and Protection are **two separate prayers**, not one combined combat sermon. Both are unlocked at Martial Skills after a deep Smithing route, then each separately consumes Hard Book +7 Faith and a weekly sermon use.

Raw magnitude comparisons remain useful:

- Retribution +5 damage is large relative to several swords and equals common short damage buffs;
- Protection +4 armor is large relative to normal armor and equals the short armor potion/food buff;
- both prayer durations are far longer than consumables.

But raw stat size is not enough to make the proposition attractive. The design issue is that Graveyard Keeper asks for limited sustained combat, cautious play/consumables substitute for much of the need, and each prayer delivers only one stat after a very large progression/investment budget.

Current leading structural hypothesis is to **keep two recognizable prayers but turn each into a full thematic package** (offensive Retribution, defensive Protection) using verified stable game parameters. Combining them into one prayer and repurposing the second remains a larger fallback redesign.

No package contents or numbers are accepted until combat formulas/available parameters are audited narrowly.

## Active-buff UI — final presentation finding

Probe 0.1.6 shows standard `BuffIcon.Draw` assigns icon/timer behavior and `BuffIcon.Redraw` only updates remaining time. No prayer-specific dynamic effect explanation is wired through the active icon.

For timed prayer effects, vanilla therefore communicates essentially **icon + remaining time**, not quantitative meaning.

This strengthens the Clarity requirement. After-use UI cannot substitute for good technology/item/pulpit information.

## Community evidence and design target

Community discussion does not provide one agreed numerical rebalance, but it repeatedly exposes the target:

- Combo becomes a default once obtainable and production gates are solved;
- some niche prayers are excellent precisely because they are powerful in a narrow use case (Imagination, Prosperity, Thorough Cleansing, high-Gratitude BSS Soul's Repose);
- broken/opaque prayers are treated as useless when players cannot observe value;
- expensive gold/book prayers create an expectation of meaningful payoff.

Representative sources remain documented in `PLAYER_UX_RESEARCH.md` and `PRAYER_POWER_BUDGET.md`.

## Current stage / next design work

The quantitative power-budget requested by the project is now complete enough to stop broad data collection. `PRAYER_REBALANCE_OPTIONS.md` contains the first candidate roster families.

Next work is **model and narrow**, not “research everything again”:

1. simulate the Faith/Donations specialist curves versus stock Combo at representative early/mid/late church and graveyard states, then choose one first candidate;
2. compare Repentance chance curves against expected confessional throughput and writing/Faith value, then choose one first candidate;
3. inspect only the combat formula/parameter surface needed to turn the preferred two-package combat hypothesis into concrete candidate effects;
4. keep Shoots/Roots at the recoverable -20% for its first runtime test;
5. assemble one coherent non-production `Rebalanced` specification before opening a `dev/*` implementation branch.

No user in-game test is required at this stage.