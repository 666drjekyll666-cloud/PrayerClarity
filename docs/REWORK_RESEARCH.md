# PrayerClarity — Rework research

Status: research/design synthesis, updated 2026-09-16 after user design review and Repentance lifecycle probe 0.1.1. No Balance/Rework mechanic here is accepted runtime behavior until implemented and runtime-tested where required.

## Baseline

- Target: Graveyard Keeper 1.407.
- Stable presentation baseline: PrayerClarity 1.0.0, accepted runtime/source `493d2168489af80b5c1305f7ff1435e2ee1dd0d7`.
- Stock mechanics remain canonical in `PRAYER_MECHANICS.md`.
- Rebalanced values are deliberate design, never restored-vanilla claims.
- Failed-sermon full base donations remain preserved in every future profile.
- Product direction: one coherent opinionated **Rebalanced** profile, not per-prayer sliders.
- The isolated Fixed Vanilla profile is not a product priority; the proven Shoots & Roots repair belongs inside Rebalanced while stock 1.407 remains documented separately.

## Global Rebalanced design rules

### Temptation parity

A prayer costs technology, writing/crafting resources, prayer-quality effort, Church Quality/success requirements and one weekly sermon slot. Every prayer should create a believable point where that complete proposition is worth choosing.

This is not numerical parity. A narrow prayer may be deliberately very strong inside its niche.

### A weekly prayer should feel powerful

Prayer use is much more expensive than drinking an ordinary consumable. A narrow prayer should therefore create an obvious, satisfying gameplay effect rather than a small efficiency coupon.

Combat is the clearest reference: dungeon play is only one activity among many, so spending the weekly sermon on a combat-only prayer should produce a genuine power-fantasy window.

### Bronze is a floor; Silver/Gold are progression

Do not weaken a healthy Bronze solely to manufacture tier symmetry.

Silver and especially Gold require materially more difficult writing inputs. A player comparing prayer qualities in PrayerClarity should immediately understand why paying that extra production cost is worthwhile.

Premium quality can scale through:

- magnitude;
- reliability;
- useful duration;
- tier-scaled physical/resource output;
- a thematic secondary reward.

Duration-only quality is valid only when the extra duration materially increases useful activity/value.

### Specialists beat the generalist at their specialty

Combo may remain good at both Faith and donations, but equal-quality Faith/Donations specialists should have the highest peak in their own resource. Prefer strengthening specialists over nerfing Combo.

### Natural obsolescence is valid

Prosperity and Repose may be progression-window prayers. They need not dominate endgame if they are strong when they matter.

### Balance against substitutes

Compare prayers against the systems players actually use instead: potions, fertilizer/zombies, writing perks, crates/tavern income, corpse progression, BSS loops, etc.

### One coherent fantasy per prayer

If two prayers split one narrow fantasy into two weak weekly choices, merge/repackage before merely inflating both numbers.

### Prefer clean player-facing arithmetic

Percentage-first Faith/Donations presentation is preferable to a cluttered mixture of percentage, flat and off-theme rewards, but cleanup must not accidentally nerf the intended progression window.

### Stronger ceiling may justify stronger Church Quality requirements

A higher specialist ceiling may require a higher guaranteed-success gate. Requirements should fit normal church/cathedral progression, not theoretical extreme candle stacking.

### Mechanics and Clarity share one semantic model

All effective Rebalanced values/rewards/requirements must feed the same model used by pulpit, Technology and active-effect UI.

## Community/design synthesis

Repeated player signals remain useful as design evidence, not mechanics proof:

- routine prayer choice is compressed around Faith/Combo;
- Combat, Repentance and Shoots & Roots have weak propositions relative to the weekly slot;
- Imagination is a healthy writing bootstrap and should not have its Bronze strength reduced for symmetry;
- Prosperity is a healthy progression prayer with natural obsolescence;
- Thorough Cleansing is already a successful narrow specialist;
- Soul Contentment is state-dependent rather than automatically weak.

## Accepted user design decisions — 2026-09-16

### Combat merge — accepted design direction

Retribution and Protection should become one effective **Combat Prayer** in Rebalanced.

Migration contract:

- `b_sword` is the canonical effective Combat Prayer identity;
- existing `b_shield` items remain intact and act as same-quality legacy aliases;
- no save-ID rewrite/deletion;
- retire/hide duplicate new-player Protection crafting only after its lifecycle seam is verified;
- removing the mod/profile restores vanilla interpretation.

### Combat first runtime target — accepted design target

Keep the current q and duration ladder initially unless later evidence requires adjustment:

| Quality | Damage | Armor | Regeneration | Duration |
| --- | ---: | ---: | ---: | ---: |
| Bronze | +5 | +4 | **1 HP/sec** | 36 min |
| Silver | +10 | +4 | **2 HP/sec** | 72 min |
| Gold | +15 | +4 | **4 HP/sec** | 108 min |

Current q remains 10/20/40 for the first implementation candidate.

Design intent:

- hits remain meaningful;
- dodging remains required;
- after clearing a room, a short corridor restores a large, satisfying chunk of HP;
- potion dependence falls strongly while the blessing is active;
- Gold should feel exceptional enough to justify crafting/using a combat-only weekly prayer once or twice for dungeon progression.

Armor remains deliberately conservative because stock armor is flat subtraction and can trivialize weak hits faster than regeneration does.

### Visual power fantasy — accepted research direction

A subtle native visual effect is desirable if it can be implemented cheaply and event-driven. Combat is the strongest candidate for a persistent holy aura or weapon-like glow, but other prayers may also receive restrained thematic polish if a reusable game-native FX seam exists.

Do **not** justify per-frame scans, broad polling, custom heavy VFX systems or brittle asset surgery for this polish.

## Repentance evidence

### Probe 0.1.0 — per-roll mechanism

`church_budka_roll`:

- handles the two confessionals independently;
- removes previous `confession_available`;
- rolls a random float in `[0,1]` for each existing confessional;
- compares it with player `confession_probability`;
- adds `confession_available` on success.

Stock player parameter is `confession_probability = 0.15`.

### Probe 0.1.1 — lifecycle data and exact confession rewards

Direct runtime data now establishes:

`LogicDefinition church_budka_roll`:

- `start_time = 2`;
- `period_time = 1`;
- executes `SetPpar("confession_probability", 0.15)`;
- then executes script `church_budka_roll`.

The unit/meaning of `start_time` and `period_time` is still an evidence gap until the scheduler consumer code is inspected. Do not yet label this “once per day” or “twice per day” as fact.

Confessional I (`church_budka_1`) reward on `confession_available`:

- always **1 Faith**;
- **70%** Story I (`story:1`);
- **30%** Story II (`story:2`).

Confessional II (`church_budka_2`) reward:

- always **2 Faith**;
- **70%** Story II (`story:2`);
- **30%** Story III (`story:3`).

This materially raises the value of Repentance compared with treating every confession as “1 Faith + one generic Story”. Final 30/50/70 or another chance ladder must be modeled only after scheduler cadence is proved.

A combined visual/scheduler probe is now intended to close the scheduler-unit evidence without another dedicated Repentance-only probe.

## Imagination and Excellence scope

### Imagination

`buff_pen`, stock `craft_q=+0.7`, is linked to the writing/prayer-production chain: Stories, Notes, Chapters, Soul-writing equivalents and many/all player-facing prayer crafts.

Stock Bronze +0.7 is healthy and should not be nerfed.

Preferred direction:

- keep `craft_q=+0.7` at Bronze/Silver/Gold;
- keep 18/36/54 min duration unless testing finds a problem;
- use thematic Silver/Gold writing rewards to create the premium-quality ladder.

Current favored reward candidate:

- Silver: **3 Silver Stories** on successful sermon;
- Gold: **3 Gold Stories** on successful sermon.

This is not literally a guaranteed 100% refund of the prayer's exact cost: premium Prayer production goes through multi-quality Story -> Note -> Chapter -> cover/Hard Book rolls and perks. But it intentionally feels like the first successful premium sermon repays a major premium writing component and then becomes net-positive on later uses.

The user considers that economy acceptable/promising. Keep the 3/3 reward as the leading candidate unless modeling exposes a problem.

### Excellence

`buff_star` affects a narrower, different set of premium crafts, including hard books, high-tier chisels, carved wood and marble quality work.

Leading magnitude curve remains:

- Bronze +0.2;
- Silver +0.5;
- Gold +1.0;
- duration 18/36/54 min.

Gold being capable of making a reachable premium quality deterministic is an intended payoff, not automatically an imbalance.

## Faith / Donations / Combo current direction

Percentage core approved for continued modeling:

- Faith: `k_faith = 1 / 2 / 3` => +100/+200/+300% Faith;
- Donations: `k_money = 1 / 2 / 3` => +100/+200/+300% donations;
- Combo remains +50/+100/+150% both initially.

The q ladder **10 / 30 / 70** for strengthened specialists is approved as the leading model, with Combo remaining a useful q15/30/60 reference.

Still open: whether to remove every stock fixed/off-theme side reward. Faith cleanup looks especially clean; Donations needs low-GQ modeling because its fixed money floor is proportionally important early and removing it may accidentally nerf the prayer's intended progression window.

## Other prayer directions

- Shoots & Roots: include scope repair and -20/-30/-40% growth time.
- Repose: Bronze stock-style roll; Silver halfway from stock best-tier probability to certainty; Gold guaranteed best prayer-eligible tier; never exceed progression ceiling.
- Prosperity: keep stock 1/2/3 Blessings.
- BSS Soul's Repose: keep stock initially; it already has real state/quality scaling.
- Soul Contentment: re-audit whether 36/72/108 useful duration alone justifies quality cost before increasing magnitude.
- Thorough Cleansing: keep stock x2 magnitude initially; separately verify whether quality-duration alone adequately rewards Silver/Gold.

## Visual FX research gate

Open-source Graveyard Keeper BepInEx projects confirm ordinary Unity object manipulation/instantiation and access to Unity particle-system modules are technically available, but no trustworthy existing public implementation of a player prayer aura has been found yet.

Therefore the preferred evidence path is direct inspection of the installed 1.407 runtime:

1. inspect exact `LogicDefinition` scheduler consumers;
2. inspect `CreatePrayBuffFlyingObject` / buff/flying-object seams;
3. inventory player hierarchy/anchors and loaded native ParticleSystem/effect prefabs;
4. prefer reusing a native game effect over creating a custom VFX stack;
5. only prototype visuals if lifecycle can be tied to buff add/remove or another event-driven boundary.

## Material open decisions / evidence gates

1. **Repentance:** prove scheduler units/cadence, then choose final confession-probability ladder.
2. **Faith/Donations cleanup:** decide exact fixed/off-theme reward removal after representative payout modeling.
3. **Imagination:** 3 Silver / 3 Gold Stories is the leading accepted candidate; model once against actual quality-production economics before roster lock.
4. **BSS quality:** quantify whether duration-only Silver/Gold value is genuinely useful.
5. **Visual polish:** determine whether a cheap native FX seam exists and which prayers deserve persistent/subtle versus one-shot feedback.

Implementation-only gates after roster lock remain Roots SmartExpression lifecycle, Repose corpse RNG seam, Combat damage/regen/visual lifecycle seams, and safe Protection recipe retirement.
