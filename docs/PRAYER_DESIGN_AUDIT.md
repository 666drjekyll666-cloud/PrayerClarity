# Prayer Design Audit — Graveyard Keeper 1.407

Status: design audit, 2026-09-14. No balance changes are accepted or implemented.

This document evaluates prayer roles **after** the stock 1.407 mechanics were recovered. It deliberately separates three questions:

1. Is the stock mechanic functioning as wired?
2. Is the player able to understand it?
3. Even when functioning and understandable, does the prayer have a healthy gameplay role relative to spending the weekly sermon opportunity on another prayer?

`docs/PRAYER_MECHANICS.md` remains the source of truth for stock mechanics. This file records design judgements and candidates only.

## Product layers

PrayerClarity/Prayer Overhaul should keep three semantic layers separate even if they eventually ship in one DLL:

### Clarity

Information only. Explain verified stock/current-mod behavior without changing outcomes.

### Vanilla Fixes

Repair a proven stock defect only when the intended behavior is sufficiently recoverable from direct game evidence. Do not use this label for a value invented by us.

### Balance / Rework

Intentional design changes to functioning mechanics, or a replacement design where the game reveals the intended role but not the intended magnitude. These are subjective changes and require their own justification and runtime acceptance.

A future configuration can expose profiles such as `Vanilla + Clarity`, `Fixed Vanilla`, and `Rebalanced`, while sharing one mechanics/presentation model.

## Permanent non-goal: do not nerf failed-sermon base donations

Stock 1.407 passes `0.5` to the failed-sermon donation participation path, but the helper's integer `Random.Range(0,1)` implementation causes every visitor to participate. Consequently a failed sermon still pays the full **base** donation pool while prayer-specific bonuses are lost.

**Project decision:** preserve this player-favourable stock outcome. Do not repair the apparent 50% participation path in PrayerClarity/Prayer Overhaul. The clarity layer should describe the behavior that the selected profile actually provides; the fix/rebalance layers should not reduce this base payout.

This is an explicit product policy, not a claim about original developer intent.

## Design principles

- Broken is not the same as weak.
- Niche is not the same as bad.
- A progression prayer may legitimately become obsolete after it has served its progression role.
- Do not make every prayer equally good at every stage; preserve distinct reasons to choose them.
- Avoid blanket buffs merely to make the table symmetrical.
- Prefer fixing disconnected vanilla intent before evaluating whether that repaired effect is too weak.
- Prefer buffs/role improvements over nerfs unless a functioning prayer demonstrably damages choice or progression.
- The weekly sermon opportunity is the relevant opportunity cost: a passive effect must justify giving up the general-purpose sermon that week.
- Community posts indicate player demand and perceived value; they do not define mechanics or automatically establish balance consensus.

## Prayer-by-prayer audit

| Prayer family | Current role / evidence | Design diagnosis | Preliminary verdict |
| --- | --- | --- | --- |
| `b_empty` Ordinary | starter sermon; low-complexity baseline | intentionally superseded by crafted prayers | **keep; clarity only** |
| `b_faith` Faith | specialist Faith prayer; easier chapter-based craft and lower church-quality requirement than equivalent Combo; same Faith scaling at equivalent tier | healthy progression/specialist role even if Combo later becomes convenient | **keep; clarity only** |
| `b_money` Donations | specialist donation prayer; easier chapter-based craft and lower q than Combo | can become economically obsolete, but has a legitimate earlier progression role | **keep initially; clarity only** |
| `b_faith_money` Combo | generalist anchor; strong default because it combines both proportional bonuses, but costs a book and has higher q requirements | strong does not by itself mean overpowered; useful comparison baseline for all niche sermons | **keep; no nerf planned** |
| `b_plant` Shoots and Roots | intended-looking -20% growth-time formula exists, but stock prayer writes `buff_plant` to player while formula reads growing WGO | **proven stock wiring defect** | **Vanilla Fix candidate: reconnect existing -20% effect; reassess only after repair** |
| `b_sins` Repentance | prayer creates timed `buff_sins`; no consumer found. Base confessional logic sets `confession_probability=0.15`; no prayer linkage found so far | role is recoverable (increase confessional use), magnitude is not yet recoverable | **repair role desirable; magnitude remains design-gated. Do not call an invented multiplier a Vanilla Fix** |
| `b_skull` Repose | +1 maximum Donkey corpse tier, 18/36/54 min; becomes useless after final corpse tier | clear progression accelerator with a natural expiry point | **keep; clarity should disclose current relevance/obsolescence where practical** |
| `b_sword` Retribution | +5 damage, 36/72/108 min; same magnitude as Sword Master and common +damage food/potion buff, but far longer duration; early/steel/damask swords have 10/15/25 damage | magnitude is substantial; perceived weakness is mainly the limited need for a week-long combat choice in a finite/easy dungeon | **keep initially; clarity first, no automatic buff** |
| `b_shield` Protection | +4 armor, 36/72/108 min; same +4 as armor food/potion buff and close to best lamellar armor's 5 armor | magnitude is substantial; same opportunity-cost/context issue as Retribution | **keep initially; clarity first, no automatic buff** |
| `b_pen` Imagination | +0.7 writing-quality input, 18/36/54 min; enables concentrated writing production | strong, distinct production-window niche; recent player evidence shows it can be extremely valuable | **keep; clarity only** |
| `b_star` Excellence | +0.2 linked-craft quality input, 18/36/54 min; narrow set of quality crafts | narrow but meaningful late-game/quality-crafting role | **keep; clarity first; enumerate affected crafts for player wording if needed** |
| `b_village` Prosperity | produces Commercial Blessings, permanently advancing merchant tiers | strong progression utility that naturally loses value after vendors are advanced | **keep; no nerf planned** |
| `b_souls` Soul's Repose (BSS Faith prayer) | Faith baseline adds current Soul Gratitude; can exceed Combo substantially at high GP | strong state-dependent alternative, not a dominated prayer | **keep; dynamic forecast is the main UX fix** |
| `b_grat_points_incr` Soul Contentment | +10% Soul Gratitude, exact rounding verified, 36/72/108 min | clear BSS progression/throughput niche; value depends on active soul workflow | **keep initially; clarity/duration only** |
| `b_sin_shard` Thorough Cleansing | doubles Sin Shards, 36/72/108 min | highly valued by players doing BSS corpse/soul progression; strong but purpose-specific | **keep; no nerf planned** |

## Community signal: there is no single accepted rebalance recipe

Repeated discussion supports three different conclusions at once:

1. **Combo is a common default.** Players frequently describe it as their long-term general-purpose sermon and some explicitly wish more prayers had compelling uses.
2. **Several alternatives are already excellent in their niche.** Prosperity accelerates merchant tiers; Imagination can power a concentrated writing cycle; Thorough Cleansing dramatically cuts Sin-Shard grind; BSS Soul's Repose can outperform Combo for Faith; Repose is useful before the final corpse tier.
3. **Crafting/progression costs matter.** Faith/Donation prayers use chapters and lower church-quality thresholds, while Combo uses a book and higher thresholds. Therefore apparent numerical domination at equal tier does not mean the specialist prayers are pointless throughout progression.

This is evidence for a **role audit**, not evidence for globally buffing every non-Combo prayer.

Representative sources:

- 2025 `What's the best prayer to use?`: Combo is a common default, but replies defend Prosperity, BSS prayers, and other goal-dependent choices: https://www.reddit.com/r/GraveyardKeeper/comments/1ij5rxj/
- 2025 `Prayers?`: players describe Prosperity/Imagination/BSS niches while also saying several buffs feel weak or hit-or-miss: https://www.reddit.com/r/GraveyardKeeper/comments/1mfb9z0/
- 2025 Faith/Donation vs Combo discussion: specialist prayers require chapters and lower church quality; Combo requires a book/q60 at gold: https://www.reddit.com/r/GraveyardKeeper/comments/1k3y63m/
- 2026 Imagination production example: one silver Imagination prayer enables a concentrated multi-day writing run with very high returns: https://www.reddit.com/r/GraveyardKeeper/comments/1ul3qc4/
- 2026 BSS Soul's Repose formula discussion: high Soul Gratitude can make it substantially better for Faith than ordinary prayers: https://www.reddit.com/r/GraveyardKeeper/comments/1rsjmat/
- 2026 Repose discussion: current players describe it as an early/mid-game +1 corpse-tier tool that naturally expires at the final tier: https://www.reddit.com/r/GraveyardKeeper/comments/1ty6zka/
- long-running Repentance testing / combat-prayer value discussion: https://steamcommunity.com/app/599140/discussions/0/1637542851358404514/
- official Lazy Bear reply on intended Shoots/Roots role: https://steamcommunity.com/app/599140/discussions/0/3190243624323744636/

## Broken-prayer repair boundary

### Shoots and Roots

This is the cleanest Vanilla Fix candidate because the current game contains both the intended semantic direction and the exact dormant magnitude:

- growth craft-time expressions contain `-0.2*WGOpar("buff_plant")`;
- the prayer's buff is `buff_plant=1`;
- the stock scope wiring prevents that value from reaching the formulas;
- Lazy Bear separately described the prayer as reducing garden growth time and affecting zombie beds.

Candidate repair principle: make the prayer feed the existing `-20%` path; do not choose a new speed bonus yet.

### Repentance

Current evidence gives:

- base confessional logic sets player `confession_probability` to `0.15` before `church_budka_roll`;
- the prayer creates `buff_sins` for 18/36/54 min;
- no current `buff_sins` consumer was found in code literals, 180 loaded graphs, or balance references beyond the buff/prayer definitions.

The intended role is plausibly “more confessions”, supported by game/community text. But the intended **magnitude/algorithm** is still absent. A change such as `15% -> 30%` would therefore be a new balance design unless the remaining confessional audit recovers a dormant coefficient or branch.

Do not hide this distinction behind the label `bug fix`.

## Combat-prayer comparison — no balance change justified yet

Static stock comparison changes the initial suspicion that Retribution/Protection might simply be numerically weak.

### Retribution

- Prayer: `+5 damage` for 36/72/108 min.
- Sword Master perk: `+5 damage` permanently after unlock.
- common damage food: `+5 damage` for 2 min.
- ordinary damage potion: `+5 damage` for 5 min.
- berserk damage buff: `+15 damage` for 5 min, paired with its poison tradeoff.
- representative sword damage: 10 (`sword_1`), 15 (`sword_steel`), 25 (`sword_damask_gem`).

Thus the prayer is roughly +50% over a 10-damage sword, +33% over 15 and +20% over 25 before other additive bonuses. The prayer's distinctive asset is not peak burst but **very long duration**.

### Protection

- Prayer: `+4 armor` for 36/72/108 min.
- armor food: `+4 armor` for 2 min.
- armor potion: `+4 armor` for 5 min.
- Big Guy perk: `+2 armor` (and +2 damage).
- lamellar armor values found in current item data: 2 and 5.

So `+4 armor` is also a large stat increment. Again, the unresolved design question is whether the game contains enough sustained combat to justify spending the weekly sermon opportunity on that long window, not whether `+4` is trivially small.

**Current verdict:** do not tune either combat prayer yet. First expose their exact magnitude/duration clearly. Revisit only if player testing or stronger evidence shows that the *role* remains unattractive after the UI stops hiding what the prayer actually provides.

## UI implications

The overhaul should use one internal prayer presentation model but render different information according to context:

- **technology tree:** role / why unlock it / how quality changes the concept;
- **prayer item tooltip:** exact properties of this quality tier;
- **pulpit selection:** current-state Faith/donation forecast, success/failure result, special effect and duration;
- **active buff HUD:** rely on vanilla icon/timer/hover where adequate rather than duplicating the same explanation.

Technology, item, pulpit and active-buff surfaces are separate game code paths; implementation should not assume one localization replacement automatically fixes every surface.

## Next gate

Before production balance code:

1. close the exact active-buff hover presentation so we know what vanilla already communicates after use;
2. close `church_budka_roll` / `confession_probability` as far as current runtime data allows;
3. decide whether Repentance has a recoverable Vanilla Fix or needs an explicitly designed Rebalance value;
4. only then select the first narrow runtime implementation slice.

Current evidence does **not** justify a general rebalance pass. The likely first implementation slice remains **Clarity + the proven Shoots/Roots wiring repair**, unless the final confessional audit exposes an equally evidence-backed Repentance repair.
