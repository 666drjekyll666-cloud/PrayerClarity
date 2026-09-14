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

A prayer can therefore have a healthy **role** but still need a **quality-progression** rework.

## Starter-prayer boundary

The free starter `b_empty` Ordinary Prayer remains outside the current Faith-specialist rework.

The proposed `+100/+200/+300%` specialist curve applies to the separately crafted `b_faith` Prayer for Faith. Do not accidentally accelerate the opening game by modifying `b_empty` under the same design label.

## Generalist vs specialist principle

The quantitative audit established the key structural issue:

- Faith prayer target `k_faith` = `.5 / 1 / 1.5`;
- Combo target `k_faith` = `.5 / 1 / 1.5`;
- Donations target `k_money` = `.5 / 1 / 1.5`;
- Combo target `k_money` = `.5 / 1 / 1.5`;
- Donations and Combo are unlocked by the **same** `Price of faith` technology.

The specialists therefore are not actually stronger specialists at equal quality. Their remaining advantages are production/progression gates:

- Chapter +5 Faith versus Combo Hard Book +7 Faith;
- q10/20/50 versus Combo q15/30/60.

Those are meaningful early-game advantages, but once book production and church quality are routine, the generalist keeps equal target-resource power while also supplying the second resource.

**Design consequence:** a specialist premium is the leading systemic rework direction. Prefer testing stronger Faith/Donation specialist curves while keeping Combo useful and familiar before considering direct Combo nerfs.

Current leading non-accepted curve in `PRAYER_REBALANCE_OPTIONS.md` is `+100/+200/+300%` target bonus for Faith/Donations while Combo stays stock.

## Church/graveyard requirements as design levers

Higher church-quality requirements are a natural balancing lever because sermon success already uses church quality and the UI already exposes that relationship.

Thematic requirements/scalers can also be useful where the relationship is obvious. Donations is the strongest candidate because graveyard quality already creates the money baseline.

Current preferred architecture is conservative:

- **church quality** remains the universal sermon-success/delivery gate;
- first exploit the fact that Graveyard Quality already scales the larger Donations specialist bonus naturally;
- add a visible Graveyard Quality threshold only if runtime testing shows an additional progression gate is useful;
- avoid making the same graveyard stat both a continuous reward multiplier and an opaque second success multiplier;
- do not add thematic requirements merely for symmetry.

## Prayer-by-prayer design matrix

| Prayer family | Stock role / cost context | Design diagnosis | Current design direction |
| --- | --- | --- | --- |
| `b_empty` Ordinary | free starter sermon | intentionally temporary baseline; changing it would directly accelerate opening Faith income | **keep stock; clarity only** |
| `b_faith` Faith | Chapter +5 Faith; lower q than Combo; same Faith coefficient as Combo at equal tier | healthy early specialist, but loses output specialization once Combo's book/q gates are solved | **Rework candidate: specialist premium; current lead +100/+200/+300%** |
| `b_money` Donations | Chapter +5 Faith; same unlock as Combo; lower q; same money coefficient as Combo | clearest specialist/generalist compression because it is not even earlier-unlocked | **Rework candidate: specialist premium; let Graveyard Quality scale value naturally first** |
| `b_faith_money` Combo | Hard Book +7 Faith; q15/30/60; same target coefficients as both specialists | powerful universal default once production gate is routine | **keep stock initially; solve choice compression by specialist premium before nerfing** |
| `b_plant` Shoots and Roots | Chapter +5 Faith; special farming prayer | proven wiring defect disconnects existing `-20%` growth formula | **Vanilla Fix: reconnect stock -20%; runtime-test before extra buff** |
| `b_sins` Repentance | Chapter +5 Faith; q10/20/40; 18/36/54 min; base confession chance 15% | timed buff has no consumer; intended role known but magnitude absent | **Balance/Rework: current lead 30/50/70% confession chance** |
| `b_skull` Repose | Hard Book +7 Faith; +1 Donkey max corpse tier, 18/36/54 min | role is valid, but all qualities share same +1 and only duration scales; raw +1/+2/+3 risks skipping several short story corpse tiers | **Rework candidate: quality should improve reliability/quality floor while staying at most one story tier ahead** |
| `b_sword` Retribution | deep Smithing route; Hard Book +7 Faith; +5 damage, 36/72/108 min | one half of an expensive combat-preparation proposition | **preferred Rebalanced architecture: canonical combined Combat Prayer ID** |
| `b_shield` Protection | same deep unlock; separate Hard Book +7 Faith; +4 armor, 36/72/108 min | second half of the same expensive combat-preparation proposition | **preferred soft merge: hide for new Rebalanced crafting; preserve existing items as same-quality legacy aliases** |
| `b_pen` Imagination | Hard Book +7 Faith; +0.7 writing input, 18/36/54 min | bronze effect is genuinely powerful; duration-only silver/gold can be weak because writing can be stockpiled and batched | **Rework candidate: retain bronze +0.7, model magnitude scaling (lead +0.7/+1.0/+1.3)** |
| `b_star` Excellence | Hard Book +7 Faith; +0.2 linked-craft input, 18/36/54 min; affects a narrow quality-crafting set | useful niche exists, but fixed magnitude plus duration-only scaling creates same quality problem | **Reopen quality scaling; model after Imagination** |
| `b_village` Prosperity | Chapter +5 Faith; 1/2/3 Blessings of Commerce by quality | permanent vendor-progression utility and direct 1x/2x/3x special-output scaling | **keep stock; strong quality-progression reference** |
| `b_souls` Soul's Repose (BSS) | shared BSS unlock; state-dependent Faith baseline includes Soul Gratitude; quality also scales Faith bonus | can beat Combo/Faith substantially at high Gratitude and higher quality multiplies that stronger baseline | **keep stock; dynamic current-value preview is main improvement** |
| `b_grat_points_incr` Soul Contentment | +10% Gratitude, 36/72/108 min | clear workflow-specific throughput niche; long duration can cross sermon weeks | **keep first; clarity/quality duration** |
| `b_sin_shard` Thorough Cleansing | x2 shards, 36/72/108 min | deliberately powerful specialist effect; clear temptation-parity benchmark | **keep; strong reference case** |

## Repentance — final classification and current design pressure

The base confessional system uses a 15% roll. Successful confessional interactions observed in runtime logs pay 1 Faith plus a Story. The prayer creates `buff_sins`, but no surviving 1.407 consumer or intended multiplier exists.

Therefore a working Repentance is definitively **Balance / Rework**.

A flat `15% -> 30%` is useful as a bronze benchmark because it doubles the stock chance, but the power-budget analysis shows why it may be too modest as the whole prayer: finite 18/36/54-minute windows plus a full weekly sermon cost make quality-scaled chance a more promising design.

Current leading hypothesis is **30% / 50% / 70%**, not accepted. It keeps uncertainty while making silver/gold materially better.

## Combat prayers — merged product direction with save compatibility

Retribution and Protection are two separate prayers in stock, but the preferred Rebalanced product direction is now **one strong combat-preparation prayer**, not two expensive one-stat sermons.

The mod can merge them without destructive save migration:

- keep one existing ID (candidate `b_sword`) as canonical Combat Prayer;
- combine offense + defense + a bounded regeneration component;
- stop offering the second recipe as a new distinct Rebalanced choice;
- existing `b_shield` inventory items remain unchanged in the save and are interpreted as same-quality legacy aliases of Combat Prayer while Rebalanced is active;
- do not rewrite saved item IDs;
- uninstall/profile-off therefore restores vanilla identity naturally.

This preserves existing player investment while allowing the roster to behave as if the game had originally shipped one coherent combat sermon.

Exact damage/armor/regeneration scaling remains open. Armor is flat subtraction and stock potion regeneration is too strong to copy unchanged over multi-day prayer durations, so the combined package must be modeled as a whole rather than built by stacking stock short-buff numbers.

## Repose — quality design finding

Direct runtime evidence proves the prayer contributes to the live Donkey `Tier max` path. Current quality changes duration only.

The appealing `+1/+2/+3` idea is **not** rejected because it is “too fun”; it is rejected provisionally because each integer is a whole story corpse tier and the underlying progression only contains a few tiers. A gold +3 can therefore leap several progression stages.

Preferred design goal:

- never access more than the **next** corpse tier beyond current story progression;
- bronze may merely extend the pool upward, as stock does;
- silver should reduce low-tier outcomes / raise the quality floor;
- gold should make boosted next-tier deliveries reliable while active.

This makes quality materially stronger without turning the prayer into a story-sequence skip.

## Imagination / Excellence — quality design finding

Imagination's stock +0.7 is not weak. Current/community evidence shows it can enable extremely productive writing batches and materially improve the chance of moving written products up a quality tier.

The issue is that **all qualities give the same +0.7**. Since writing materials can be accumulated before the sermon, a player can often perform the whole batch during bronze's 18-minute window. Silver/gold duration then does not necessarily create enough extra value.

Current leading Imagination design hypothesis is `+0.7 / +1.0 / +1.3`, retaining 18/36/54 min. This keeps bronze stock-strong while making silver/gold improve the actual quality roll.

Excellence has the same structural quality issue on a narrower craft set: stock +0.2 at every tier. Reopen it after Imagination; a simple `+0.2/+0.4/+0.6` family is a modeling candidate, not accepted behavior.

## Prosperity / BSS Soul's Repose — why they survive the quality audit

### Prosperity

Prayer quality directly yields **1 / 2 / 3 Blessings of Commerce**. These are permanent vendor-progression resources rather than a temporary magnitude hidden behind extra duration. Bronze has immediate early/mid-game value; gold triples the special output and can accelerate several vendors in one sermon.

Natural obsolescence after the useful vendors are maxed is acceptable for a progression prayer.

### BSS Soul's Repose

Soul Gratitude enters the Faith **baseline**, and prayer quality then applies `.5 / 1 / 1.5` Faith bonus scaling on top of that state-dependent baseline. At high Gratitude, gold is therefore not merely “same effect for longer”; it amplifies a much larger current reward and can substantially beat Combo/Faith.

The main defect is that vanilla does not show this current result. A dynamic pulpit preview is the appropriate first fix.

## Active-buff UI — final presentation finding

Probe 0.1.6 shows standard `BuffIcon.Draw` assigns icon/timer behavior and `BuffIcon.Redraw` only updates remaining time. No prayer-specific dynamic effect explanation is wired through the active icon.

For timed prayer effects, vanilla therefore communicates essentially **icon + remaining time**, not quantitative meaning.

This strengthens the Clarity requirement. After-use UI cannot substitute for good technology/item/pulpit information.

## Community evidence and design target

Community discussion does not provide one agreed numerical rebalance, but it repeatedly exposes the target:

- Combo becomes a default once obtainable and production gates are solved;
- some niche prayers are excellent precisely because they are powerful in a narrow use case (Prosperity, Imagination's bronze effect, Thorough Cleansing, high-Gratitude BSS Soul's Repose);
- broken/opaque prayers are treated as useless when players cannot observe value;
- expensive gold/book prayers create an expectation of meaningful payoff.

Representative sources remain documented in `PLAYER_UX_RESEARCH.md` and `PRAYER_POWER_BUDGET.md`.

## Current stage / next design work

Broad mechanics/cost discovery is closed. The next stage is candidate-roster completion:

1. specify/model the merged Combat Prayer package and bounded regeneration;
2. model Repose delivery reliability against real body-tier ranges;
3. model Imagination and Excellence candidate quality curves against real craft-quality probabilities;
4. freeze one complete non-production Rebalanced specification;
5. only then open a `dev/*` implementation branch.

No user in-game test is required at this stage.