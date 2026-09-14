# Prayer Design Audit — Graveyard Keeper 1.407

Status: design audit, 2026-09-14. Stock mechanics are verified separately; no balance/rework numbers are implemented or accepted yet.

This document evaluates prayer design after the stock 1.407 mechanics were recovered. `docs/PRAYER_MECHANICS.md` remains canonical for what vanilla actually does.

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

The Combo Prayer is the structural reference point because, at equal quality, it combines the principal Faith and donation percentage bonuses rather than choosing one specialization.

Current progression partially taxes that convenience:

- Combo is a **book-sermon** rather than a chapter-sermon;
- its church-quality requirements are higher than Faith/Donations at the same tier;
- high-quality books are materially harder to produce than chapters.

That is real balancing and should be preserved in the model.

But the tech unlock itself is relatively early in Theology (`Business of Faith -> Price of Faith`), and once the craft/quality gates are solved the Combo Prayer becomes an unusually convenient universal default. Recent community discussion repeatedly reflects that outcome.

**Design consequence:** Combo is now a legitimate rework candidate, not because it must be nerfed, but because a universal hybrid should not automatically erase the desire for specialists/niche prayers once its production gate is crossed.

Possible levers to evaluate later, without choosing numbers yet:

- make dedicated Faith/Donation prayers stronger in their specialist output while leaving Combo unchanged;
- apply a generalist tax so Combo gives good-but-not-best Faith and donations simultaneously;
- raise Combo church-quality requirements further;
- move/raise its technology gate only if compatibility and progression evidence justify the larger intervention;
- strengthen niche prayers enough that choosing them over Combo is a deliberate, exciting trade rather than a self-imposed handicap.

Prefer **adding attractive reasons to choose alternatives** over reducing existing player rewards unless a nerf is clearly required for healthy choice.

## Church/graveyard requirements as design levers

Higher church-quality requirements are a natural balancing lever because sermon success already uses church quality and the UI already exposes that relationship.

Do not turn graveyard quality into a generic second success gate merely because another number is available. Graveyard quality should affect a prayer only where the relationship is thematically/mechanically intelligible and can be clearly shown to the player. Existing donation scaling is a good example.

If a reworked prayer becomes materially stronger, increasing its church requirement can be preferable to simply inflating rewards without a corresponding progression gate.

## Prayer-by-prayer design matrix

| Prayer family | Stock role / cost context | Design diagnosis | Current design direction |
| --- | --- | --- | --- |
| `b_empty` Ordinary | starter sermon | intentionally temporary baseline | **keep; clarity only** |
| `b_faith` Faith | chapter-sermon; best early specialist Faith path; lower q than Combo | healthy specialist/progression role, but should remain tempting after Combo appears | **keep stock for first prototype; evaluate specialist advantage in full rebalance** |
| `b_money` Donations | chapter-sermon; specialist money path; lower q than Combo | legitimate early role, later money often loses value | **keep stock first; evaluate specialist advantage, not automatic buff** |
| `b_faith_money` Combo | book-sermon; early Theology unlock; equal-tier hybrid of Faith+donation percentage bonuses; q 15/30/60 | unusually convenient once production gate is solved; strong choice-compression signal | **Rework candidate: preserve useful generalist role but test whether specialists/niches need stronger comparative reasons** |
| `b_plant` Shoots and Roots | chapter-sermon; special farming prayer | proven wiring defect disconnects existing `-20%` growth formula | **Vanilla Fix: reconnect stock -20%; then reassess whether the repaired niche is sufficiently tempting** |
| `b_sins` Repentance | chapter-sermon; intended confessional prayer | timed buff exists but has no consumer; intended magnitude is absent from 1.407 | **Rework: retain “more confessions” role, choose a new explicit rule rather than pretending it is recovered vanilla** |
| `b_skull` Repose | book-sermon; +1 Donkey maximum corpse tier, 18/36/54 min | clear progression accelerator, naturally expires at final corpse tier | **keep role; clarity first; only tune if the usable window is shown to be too narrow for its investment** |
| `b_sword` Retribution | separate book-sermon; +5 damage, 36/72/108 min | stat magnitude is real, but player pays a full unlock/craft/week for an effect often replaceable by cautious play/consumables | **Rework candidate: current one-dimensional combat proposition is not sufficiently compelling merely because +5 is numerically large** |
| `b_shield` Protection | separate book-sermon; +4 armor, 36/72/108 min | same structural problem as Retribution; it is a second expensive one-dimensional combat prayer | **Rework candidate; consider broader/consolidated combat roles rather than a blind +armor buff** |
| `b_pen` Imagination | book-sermon; fixed +0.7 writing-quality input, 18/36/54 min | powerful concentrated production-window niche; demonstrated player value | **keep; clarity only initially** |
| `b_star` Excellence | book-sermon; fixed +0.2 linked-craft quality input, 18/36/54 min | narrow but meaningful quality-crafting role | **keep; clarity first; enumerate affected crafts before judging value** |
| `b_village` Prosperity | chapter-sermon; permanent vendor-progression items | very strong progression utility, naturally exhausts itself | **keep; no nerf planned** |
| `b_souls` Soul's Repose (BSS) | book-style high-value Faith alternative driven by current Soul Gratitude | can beat ordinary/Combo Faith substantially at high GP | **keep; dynamic forecast is the main improvement** |
| `b_grat_points_incr` Soul Contentment | +10% Soul Gratitude, 36/72/108 min | clear workflow-specific throughput niche | **keep first; quality/duration must be visible** |
| `b_sin_shard` Thorough Cleansing | x2 Sin Shards, 36/72/108 min | deliberately powerful specialist effect; players value it because the niche reward is large | **keep; useful reference for how strong a narrow prayer can be** |

## Combat prayers — revised diagnosis

Retribution and Protection are **two separate prayers** unlocked together by Martial Skills, not one combined combat sermon. Each uses the expensive book-sermon recipe class (`Book + Faith`) and each separately consumes the weekly sermon choice when used.

Raw magnitude comparisons remain useful:

- Retribution's +5 damage is substantial relative to weapon damage and matches the Sword Master bonus;
- Protection's +4 armor is substantial relative to normal armor values;
- both last far longer than common food/potion equivalents.

But raw stat size is not sufficient to establish good prayer design. The relevant question is whether the player wants to pay the complete prayer cost for that effect.

The current design concern is therefore structural:

- two separate unlockable/craftable items divide offense and defense;
- Graveyard Keeper's sustained combat demand is limited;
- cautious play and consumables can substitute for much of the value;
- each use displaces a full week's Faith/economy/progression sermon.

Potential future design directions to compare rather than immediately implement:

1. **single strong combat package** — one prayer supplies both meaningful offense and defense, with the second prayer repurposed;
2. **distinct packages** — Retribution becomes a genuinely aggressive combat package and Protection a broader survivability package, each with more than one trivial stat line;
3. **quality-scaled specialization** — bronze is already useful, while silver/gold meaningfully deepen the relevant combat role rather than only adding time;
4. **keep long-duration model but increase strategic value** — if an effect lasts across later sermon weeks, the initial weekly opportunity cost becomes easier to justify.

No option is accepted until available game parameters and progression impact are checked.

## Repentance — final stock classification after probe 0.1.6

The base confessional system uses a 15% roll. The prayer creates `buff_sins`, but repeated code/data/FlowCanvas audits found no consumer. Probe 0.1.6 also found no surviving runtime reference that specifies how `buff_sins` should modify `confession_probability`.

Therefore the intended role is recoverable but the intended algorithm/magnitude is **not**.

**Final classification:** a working Repentance implementation belongs to **Balance / Rework**, not Vanilla Fixes.

The new rule should be designed to make the prayer genuinely desirable, with its chapter-sermon cost, quality progression, 15% base confession probability, confessional count, duration, and weekly sermon opportunity all considered together.

## Active-buff UI — final presentation finding

Probe 0.1.6 shows that standard `BuffIcon.Draw` assigns the buff icon and whether the timer is shown; `BuffIcon.Redraw` only updates the remaining-time label. No prayer-specific dynamic tooltip content is wired through `BuffIcon`.

For timed prayer effects, vanilla therefore communicates **icon + remaining time**, not the quantitative meaning of the effect.

This strengthens the Clarity requirement. After-use UI cannot be treated as a substitute for good technology/item/pulpit descriptions. A later UI enhancement may add buff detail, but the decision-point information remains primary.

## Community evidence and design target

Community discussion does not provide one agreed numerical rebalance, but it repeatedly exposes the design target:

- Combo becomes the default for many players once obtainable;
- some niche prayers are excellent precisely because they are powerful in a narrow use case (Imagination, Prosperity, Thorough Cleansing, high-GP BSS Soul's Repose);
- broken or opaque prayers are often treated as useless because players cannot observe their value;
- expensive gold/book prayers create a strong expectation of a meaningful payoff.

Representative sources:

- https://www.reddit.com/r/GraveyardKeeper/comments/1ij5rxj/
- https://www.reddit.com/r/GraveyardKeeper/comments/1mfb9z0/
- https://www.reddit.com/r/GraveyardKeeper/comments/1k3y63m/
- https://www.reddit.com/r/GraveyardKeeper/comments/1ul3qc4/
- https://www.reddit.com/r/GraveyardKeeper/comments/1rsjmat/
- https://www.reddit.com/r/GraveyardKeeper/comments/1ty6zka/
- https://www.reddit.com/r/GraveyardKeeper/comments/1u0z66v/
- https://steamcommunity.com/app/599140/discussions/0/1637542851358404514/

## Next design work

Do not implement arbitrary balance numbers yet.

Next produce a quantitative **power-budget / progression matrix** for each prayer containing at minimum:

- unlock technology and prerequisite depth;
- tech-point cost;
- chapter vs book recipe class and Faith cost;
- bronze/silver/gold production difficulty;
- church-quality requirements;
- Faith/donation opportunity cost relative to the best available alternative at that stage;
- special-effect magnitude and duration;
- whether quality crosses one or more weekly sermon boundaries;
- stage at which the effect becomes available and stage at which it becomes obsolete;
- community evidence of use/non-use.

Then design candidate changes prayer-by-prayer, starting with the structural outliers rather than applying a blanket multiplier.

The first runtime implementation should wait until this rebalance specification and the Clarity presentation model agree on what the effective prayer system is supposed to be.