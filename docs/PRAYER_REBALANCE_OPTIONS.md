# Prayer Rebalance Options — current candidate roster

Status: design specification, updated 2026-09-16. Stock 1.407 remains canonical in `PRAYER_MECHANICS.md`. Values below are deliberate Rebalanced design; they are **not accepted runtime behavior** until implementation and required in-game testing are complete.

`REWORK_RESEARCH.md` is the rationale/evidence source for the latest decisions.

## Product rules

- One coherent opinionated **Rebalanced** profile; no per-prayer balance sliders.
- Rebalanced includes the proven Shoots & Roots repair.
- Preserve full base donations on failed sermons.
- Every prayer must justify technology/crafting cost plus the weekly sermon slot.
- Bronze must already be credible.
- Silver/Gold must visibly justify premium writing-production cost through magnitude, reliability, useful duration, output or thematic secondary reward.
- Do not nerf a healthy Bronze merely to manufacture tier symmetry.
- A narrow weekly prayer may be very strong in its niche.
- Mechanics and PrayerClarity UI must use the same semantic data/model.

## Current roster

| Family | Current Rebalanced direction |
| --- | --- |
| Ordinary `b_empty` | stock |
| Faith `b_faith` | **+100 / +200 / +300% Faith**; leading q **10/30/70**; flat/off-theme cleanup still under modeling |
| Donations `b_money` | **+100 / +200 / +300% donations**; leading q **10/30/70**; retain/remove early flat floor only after low-GQ modeling |
| Combo `b_faith_money` | percentage core stock **+50/+100/+150% both**, q15/30/60 initially |
| Repentance `b_sins` | quality-scaled confession probability; final ladder pending proven scheduler cadence |
| Shoots & Roots `b_plant` | repaired scope + **-20/-30/-40% growth time** |
| Repose `b_skull` | Bronze stock-style roll; Silver halfway to certainty; Gold guaranteed best prayer-eligible tier; progression ceiling preserved |
| Combat (`b_sword`, `b_shield` legacy alias) | **merge accepted**; +5/+10/+15 damage, +4 armor, **1/2/4 HP/sec**, 36/72/108 min, q10/20/40 initially |
| Imagination `b_pen` | keep **+0.7** craft quality all tiers; 18/36/54 min; leading premium reward **3 Silver Stories / 3 Gold Stories** |
| Excellence `b_star` | **+0.2/+0.5/+1.0**, 18/36/54 min |
| Prosperity `b_village` | stock 1/2/3 Blessings |
| BSS Soul's Repose `b_souls` | stock initially |
| Soul Contentment | stock magnitude initially; audit whether duration alone pays for Silver/Gold |
| Thorough Cleansing | keep stock x2 magnitude initially; audit whether duration alone pays for Silver/Gold |

## Faith / Donations / Combo

`k_faith` and `k_money` are proportional success modifiers:

- `.5` = +50%;
- `1` = +100%;
- `2` = +200%;
- `3` = +300%.

Stock specialists are structurally compressed because their target coefficient is the same as equal-quality Combo. Rebalanced makes the specialist best at its own resource while Combo remains the convenient generalist.

Leading percentage core:

- Faith `k_faith = 1 / 2 / 3`;
- Donations `k_money = 1 / 2 / 3`;
- Combo `.5 / 1 / 1.5` both.

Leading strengthened specialist success gate: **q10/30/70**. This is intentionally above Gold Combo q60 while remaining in realistic cathedral progression.

### Flat/off-theme cleanup remains open

A cleaner family would remove generic fixed outputs and the specialist's off-theme side coefficient. That is attractive for comprehension, but it must not accidentally nerf the early role.

Faith is likely safe to simplify because the stronger percentage naturally replaces much of its early fixed value.

Donations is less trivial: at low Graveyard Quality the fixed money reward is a large fraction of total output. Model representative low/mid/high GQ before deleting the floor.

## Shoots & Roots

Rebalanced:

- Bronze: **-20% growth time**;
- Silver: **-30%**;
- Gold: **-40%**;
- duration 36/72/108 min initially.

Implementation must preserve the existing additive SmartExpression structure and only repair/scale the prayer term; do not replace the game's growth model with an external timer system.

## Repentance

Direct evidence now establishes:

- stock `confession_probability = 0.15`;
- two confessionals are rolled independently;
- `LogicDefinition church_budka_roll` has `start_time=2`, `period_time=1`, but scheduler units still need direct consumer inspection;
- Confessional I reward: **1 Faith + 70% Story I / 30% Story II**;
- Confessional II reward: **2 Faith + 70% Story II / 30% Story III**.

The old **30/50/70%** ladder remains a useful benchmark but is not final until cadence is proved and expected weekly output is modeled correctly.

Gold target: a clearly noticeable “confession week”, not a tiny increase in an event the player barely sees.

## Combat Prayer — accepted structural and numeric target

### Save-safe merge

- `b_sword` becomes the canonical Combat Prayer in Rebalanced.
- Existing `b_shield` items remain valid as same-quality legacy aliases.
- No save-ID rewrite/deletion.
- Retire/hide duplicate Protection crafting only after lifecycle inspection proves a safe seam.
- Removing Rebalanced returns the original IDs to vanilla meaning.

### First runtime candidate

| Quality | Damage | Armor | Regeneration | Duration | q |
| --- | ---: | ---: | ---: | ---: | ---: |
| Bronze | **+5** | **+4** | **1 HP/sec** | 36 min | 10 |
| Silver | **+10** | **+4** | **2 HP/sec** | 72 min | 20 |
| Gold | **+15** | **+4** | **4 HP/sec** | 108 min | 40 |

This is deliberately strong. Dungeon/combat is a narrow activity and cheap consumables compete directly with the prayer. The desired loop is:

`fight -> take burst damage -> clear room -> short corridor -> substantial recovery -> next fight`.

Hits must still matter and dodging must remain required. Armor stays at +4 because flat armor scales sharply against low-damage enemies.

A subtle persistent holy aura/weapon-like glow is desirable if the game exposes a cheap native event-driven visual seam. Visual polish must not introduce polling or heavy custom VFX infrastructure.

## Repose

Preserve the prayer-eligible maximum at normal progression max +1 rather than using `+1/+2/+3 body_max` and skipping story tiers.

- Bronze: normal stock selection from expanded pool.
- Silver: `P(best) = 0.5 + 0.5 * P_vanilla(best)`.
- Gold: 100% best prayer-eligible tier.

Equivalent Silver implementation: 50% force best eligible; otherwise perform the vanilla roll.

## Imagination

Stock Bronze `craft_q=+0.7` is a healthy writing/prayer-production bootstrap and must not be reduced just to create a numerical ladder.

Leading design:

- Bronze: **+0.7**, 18 min;
- Silver: **+0.7**, 36 min, plus **3 Silver Stories** on successful sermon;
- Gold: **+0.7**, 54 min, plus **3 Gold Stories** on successful sermon.

The reward reinforces the writing fantasy without bypassing Notes, Chapter, cover and Hard Book production.

The 3/3 reward is not literally a guaranteed exact refund of prayer cost because premium prayer quality is produced through multi-quality transforms and perks. It intentionally makes the first successful premium sermon repay a major premium writing component and subsequent sermons become net-positive.

Keep this as the leading candidate and model its actual economy once before roster lock.

## Excellence

Verified linked scope is narrower than Imagination and includes premium quality crafts such as hard books, chisels, carved wood and marble work.

Leading curve:

- Bronze **+0.2**;
- Silver **+0.5**;
- Gold **+1.0**;
- duration 18/36/54 min.

Gold may intentionally make reachable premium quality deterministic. Finite game quality tiers cap the effect naturally.

## Stock-like reference prayers

### Prosperity

Keep stock. 1/2/3 permanent Commercial Blessings already produce a clear premium-quality ladder.

### BSS Soul's Repose

Keep stock initially. It already has meaningful quality/state scaling.

### Soul Contentment

Do not inflate magnitude automatically. Quantify whether 36/72/108 min changes the number of useful soul-processing actions enough to justify premium prayer quality.

### Thorough Cleansing

Keep x2 magnitude initially because it is already a successful scarce-resource specialist. Separately quantify whether 36/72/108 min is enough premium-tier value.

## Open gates before roster lock

1. Prove Repentance scheduler units/cadence and choose final confession chance ladder.
2. Finish Faith/Donations flat/off-theme payout modeling.
3. Model Imagination 3/3 premium Story reward once against actual quality-production economics.
4. Quantify BSS duration-only quality value.
5. Determine whether native prayer visual FX can be reused cheaply and which prayers benefit without becoming visually noisy.

After roster lock, close implementation-only seams: Roots SmartExpression lifecycle, Repose RNG seam, Combat regen/damage/visual lifecycle and safe Protection recipe retirement.
