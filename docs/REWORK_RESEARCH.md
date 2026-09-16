# PrayerClarity — Rework research

Status: research/design synthesis, updated 2026-09-16 after user design review, Repentance lifecycle evidence, native visual-FX audit, and two visual auditions. No Balance/Rework mechanic here is accepted runtime behavior until implemented and runtime-tested where required.

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

### Visual power fantasy — accepted scope decision

Visual polish is **optional and out of the first Rebalanced production scope**.

Two runtime auditions showed that the obvious game-native ParticleSystem reuse paths do not produce a clean prayer aura or holy-weapon effect without further custom tuning. The gameplay rework must not wait for, depend on, or carry extra runtime complexity for decorative VFX.

If a genuinely cheap, stable, event-driven native effect seam is discovered incidentally later, visual polish may be revisited as a separate enhancement. Do not spend further research/runtime-test cycles on VFX for the current roster.

### Repentance ladder — accepted design target

Rebalanced Repentance uses the daily-confession probability ladder:

- Bronze: **50%**;
- Silver: **75%**;
- Gold: **100%**.

Keep current durations 18/36/54 min initially.

Gold intentionally means that every daily confession roll succeeds for each existing confessional while the buff is active. This makes premium quality a reliability progression rather than merely a longer timer.

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

Direct runtime data establishes:

`LogicDefinition church_budka_roll`:

- `start_time = 2`;
- `period_time = 1`;
- executes `SetPpar("confession_probability", 0.15)`;
- then executes script `church_budka_roll`.

Confessional I (`church_budka_1`) reward on `confession_available`:

- always **1 Faith**;
- **70%** Story I (`story:1`);
- **30%** Story II (`story:2`).

Confessional II (`church_budka_2`) reward:

- always **2 Faith**;
- **70%** Story II (`story:2`);
- **30%** Story III (`story:3`).

### Probe 0.1.0 visual/scheduler follow-up — scheduler semantics closed

`LogicData.GetNextExecutionTime()` schedules directly in `MainGame.game_time` using `start_time + n * period_time`; `CheckPeriod()` executes when that game-time boundary is reached.

Cross-checking the full stock `logics_data` table closes the unit semantics:

- ordinary daily NPC spawn/despawn and Donkey logic use `period_time = 1`;
- weekly Bishop/key-character logic uses `period_time = 6`;
- Graveyard Keeper's week is six in-game days.

**Fact:** `church_budka_roll period_time=1` is a **once-per-in-game-day** roll at a fixed daily phase. Each existing confessional is rerolled independently every day, and an unconsumed previous `confession_available` is removed before that day's new roll.

The stock prayer-duration values 18/36/54 minutes correspond to approximately **2.4 / 4.8 / 7.2 stock game days** on the vanilla 450-second day timebase. Exact discrete roll count can differ by one depending on sermon timing relative to the daily roll, so balance should use expected ranges rather than promise an exact number of confessions.

### Repentance quantitative implication

With both confessionals built, the accepted 50/75/100 ladder gives expected confession opportunities of roughly:

| Quality | Chance | Active window | Expected confession opportunities |
| --- | ---: | ---: | ---: |
| Bronze | 50% | ~2.4d | ~2.4 |
| Silver | 75% | ~4.8d | ~7.2 |
| Gold | 100% | ~7.2d | ~14.4 |

With only Confessional I built, halve those expected counts.

For both confessionals, expected Faith from consumed confessions is `3 * chance * active_days`, before valuing Stories. Thus a Gold 100% window is roughly **21.6 Faith plus ~14.4 Stories** if the player has both confessionals and actually checks/uses them every day. This is powerful, but it also demands daily church interaction and consumes a full weekly sermon slot.

Implementation seam note: stock `church_budka_roll` writes `confession_probability=0.15` immediately before the roll, so Rebalanced must apply the effective tier probability at the roll boundary rather than relying on a one-time persistent player-param write when the buff starts.

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

## Native visual FX audit — 2026-09-16

The installed 1.407 runtime provides several reusable visual seams, but the obvious candidates did not translate into usable player-facing prayer VFX without extra tuning.

Verified player hierarchy includes:

- `Player(Clone)/content/character/char_hero/fx` — existing character FX sprite anchor;
- `.../char_hero/shard_charge_fx` — existing player-bound looping ParticleSystem, normally present with emission disabled;
- `.../char_hero/tool/fire/fire (1)` — native tool-attached flame ParticleSystem;
- existing player light hierarchy under `Char light` and tool fire.

Verified church pulpit `PrayFX` includes native prayer-themed effects:

- `rays_fx` — one-shot pale rays;
- `sparks_fx` — one-shot white sparks;
- `calcine_fx` — one-shot pale calcine particles;
- `cloud_fx` — short warm/golden cloud;
- `pray_track_fx` and two `pray_track_fx_tst_sub` variants — looping prayer tracks with native sorting components.

The game's `AuraEmitter/AuraReceiver` classes are gameplay radius/parameter systems, **not evidence of a graphical aura system**. Do not use them merely to draw prayer polish.

The native sermon buff path already uses `PlayerComponent.CreatePrayBuffFlyingObject` -> `FlyingObject.CreateBuffFlyingObject`, which creates the buff icon at the pulpit and flies it to the Buffs UI. This is useful for sermon reveal but is not itself a persistent world aura.

### Visual Audition 0.1.0 — user runtime finding

The first native-FX audition was tested by the user and **none of its candidates is accepted as-is**:

- F2 / recolored `shard_charge_fx`: technically follows the player, but is far too small/subtle; visually it mostly lights the belt area.
- F3 / stock `pray_track_fx` parented to the player: visually resembles slow golden hairs/micro-lightning growing upward, but emitted particles remain at their world positions. The player can walk away from them, so it fails as a persistent aura.
- F5 / pulpit one-shot burst: too large, dirty and visually bulky; rejected as general blessing feedback.

### Visual Audition 0.1.1 — user runtime finding

The narrowed follow-up also failed to produce a cheap acceptable result:

- F2 / enlarged `shard_charge_fx`: scaling the effect to make it readable expands the yellow treatment across essentially the entire visible screen. It no longer reads as a local player aura and is unusable.
- F3 / prayer-track with local simulation space: the effect changes scale/behavior dramatically, producing huge screen-spanning curved streaks. It reads like an overlay/screen-space artifact rather than a Keeper-bound aura.
- F5 / recolored tool-fire proxy: does not produce an attractive or convincing holy-weapon effect in actual movement/attack presentation.

The user's screenshot of F2 confirms the failure mode visually: the world is broadly washed in yellow while the Keeper remains a tiny center point, so this is not a matter of minor tuning around an otherwise-correct aura.

### Visual conclusion — accepted scope decision

The visual experiments answered the intended research question: **there is no sufficiently cheap, obvious game-native FX reuse path worth pursuing for the first Rebalanced release**.

Do not continue iterating ParticleSystem scale/simulation-space tricks, create a custom heavy VFX stack, or spend further installed-game test cycles on decorative prayer polish. The gameplay and Clarity work have materially higher value.

This does not prohibit future visuals. If later implementation work exposes a trivial stable sprite/glow/native effect seam with negligible complexity, it may be considered as optional polish in a separate pass. It is not a roster, prototype, or release gate.

## Material open decisions / evidence gates

1. **Faith/Donations cleanup:** decide exact fixed/off-theme reward removal after representative payout modeling.
2. **Imagination:** 3 Silver / 3 Gold Stories is the leading accepted candidate; model once against actual quality-production economics before roster lock.
3. **BSS quality:** quantify whether duration-only Silver/Gold value is genuinely useful.

Implementation-only gates after roster lock remain Roots SmartExpression lifecycle, Repose corpse RNG seam, Combat damage/regen lifecycle seams, Repentance daily-roll seam, and safe Protection recipe retirement.