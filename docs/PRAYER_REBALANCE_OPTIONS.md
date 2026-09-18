# PrayerClarity: Rebalanced — stable roster specification

Status: **canonical accepted Rebalanced ruleset for PrayerClarity: Rebalanced 0.2.0**.

Stock Graveyard Keeper 1.407 mechanics remain documented independently in `PRAYER_MECHANICS.md`. Values below are intentional Balance/Rework design unless explicitly identified as a verified repair.

Stable runtime identity:

- Rebalanced version: **0.2.0**
- frozen accepted ref: `accepted/rebalanced-0.2.0`
- exact accepted source SHA: `26048581c3fe6e0d8ef4ae930a0c29474f68bbcf`
- canonical DLL: `PrayerClarity.Rebalanced.dll`
- accepted DLL SHA-256: `84cf07be553e137d4663d24267ab18257facfbe44c0a833d87579876742b6de1`

The sibling Vanilla release remains mechanically stock and is documented separately.

## Product rules

- **PrayerClarity: Vanilla** and **PrayerClarity: Rebalanced** are alternative sibling editions. Install one, not both.
- Rebalanced uses the full shared Clarity presentation layer plus the complete ruleset below.
- Base sermon Faith/donations remain present according to the underlying prayer event.
- Failed sermons retain full base donations by permanent project policy; prayer-owned success bonuses/special outputs still depend on sermon success.
- Do not show the fully resolved current Faith/donation payout before the sermon. The pulpit explains inputs and prayer-owned contributions while preserving the sermon animation as the reward reveal.
- Mechanics and all presentation surfaces consume the same effective semantic/rule source.
- All durations below are **stock Graveyard Keeper 1.407 durations**. External day-length mods are not balance targets.

## Accepted roster

| Family | 100% Church Quality | PrayerClarity: Rebalanced 0.2.0 behavior |
| --- | ---: | --- |
| Ordinary `b_empty` | **10** | Stock starter baseline. |
| Faith `b_faith` | **20 / 40 / 60** | Success bonus is flat only: **+5 / +10 / +20 Faith**. No prayer-owned donation bonus. |
| Donations `b_money` | **20 / 40 / 60** | Success bonus is flat only: **+5 / +10 / +15 silver**. No prayer-owned Faith bonus. |
| Combo `b_faith_money` | **40 / 60 / 80** | Success bonus is percentage only: **+100 / +150 / +200% Faith and donations**. No prayer-owned flat Faith/money. |
| Prosperity `b_village` | **10 / 20 / 30** | Keep stock 1 / 2 / 3 Commercial Blessings and stock sermon outputs. |
| Shoots & Roots `b_plant` | **10 / 30 / 50** | Repair stock scope; growth time **-20 / -30 / -40%**. Duration **36 / 72 / 108 min**. |
| Repentance `b_sins` | **20 / 40 / 60** | Daily confession probability **50 / 75 / 100%**. Duration **18 / 36 / 54 min**. |
| Repose `b_skull` | **20 / 40 / 50** | Bronze stock-style expanded pool; Silver halfway from stock best-tier probability to certainty; Gold guarantees the best **actually existing eligible ordinary corpse tier**. Duration **18 / 36 / 54 min**. |
| Combat `b_sword` (`b_shield` legacy alias) | **20 / 40 / 60** | Damage **+5 / +10 / +15**, armor **+4**, regeneration **1 / 2 / 4 HP/s**. Duration **36 / 72 / 108 min**. |
| Imagination `b_pen` | **20 / 40 / 60** | Writing quality **+0.7** at all tiers; successful Silver gives **3 Silver Stories**, successful Gold gives **3 Gold Stories**. Duration **18 / 36 / 54 min**. |
| Excellence `b_star` | **20 / 60 / 90** | Linked-craft quality **+0.2 / +0.5 / +1.0**. Duration **18 / 36 / 54 min**. |
| BSS Soul's Repose `b_souls` | **30 / 60 / 120** | **+50 / +100 / +150% Faith** on the verified Souls base. Preserve current Soul Gratitude scaling; remove prayer-owned fixed/off-theme outputs. |
| BSS Soul Contentment `b_grat_points_incr` | **20 / 40 / 60** | **+20% Soul Gratitude** at all tiers. Duration **36 / 72 / 108 min**. |
| BSS Thorough Cleansing `b_sin_shard` | **30 / 60 / 90** | **x2 Sin Shards** at all tiers. Duration **36 / 72 / 108 min**. |

## Resource-family grammar

### Faith

The specialist is intentionally flat-only:

- Bronze: **+5 Faith**
- Silver: **+10 Faith**
- Gold: **+20 Faith**

The common sermon base still scales from Church Quality and Eloquence. The flat prayer-owned reward does not.

### Donations

The specialist is intentionally flat-only:

- Bronze: **+5 silver**
- Silver: **+10 silver**
- Gold: **+15 silver**

The common donation base still scales from Graveyard Quality/Cardinal.

### Combo

Combo is intentionally percentage-only:

- Bronze: **+100% Faith and donations**
- Silver: **+150%**
- Gold: **+200%**

No prayer-owned flat Faith or money.

This produces the intended progression:

`early focused specialists -> mature specialists / early Combo -> late premium Combo`

The player-facing choice should remain simple:

- need Faith -> Faith specialist;
- need money -> Donations specialist;
- need both / late scaling -> Combo.

## Better Save Soul endgame structure

### Soul's Repose

Verified base Faith:

`Base Faith = (Church Quality + current Soul Gratitude) * 0.1 * EloquenceFactor`

The prayer scales from **current** Soul Gratitude held at sermon time, not lifetime souls healed and not Soul Gratitude capacity.

Accepted requirement ladder:

- Bronze q30
- Silver q60
- Gold q120

Gold q120 is the deliberate aspirational late-game ritual. It gates the **+150% success bonus**, not the underlying Souls base Faith. A failed sermon still retains the base Faith that already includes current Soul Gratitude.

At the pulpit, Soul's Repose additionally shows the current Soul Gratitude input. Exact final Faith remains hidden until the sermon resolves.

### Soul Contentment

Keep **+20% Soul Gratitude** at all qualities with q20/40/60 and 36/72/108-minute duration. Duration is the premium quality axis.

### Thorough Cleansing

Keep **x2 Sin Shards** at all qualities with q30/60/90 and 36/72/108-minute duration.

## Repose terminal behavior

Direct 1.407 evidence establishes normal terminal Donkey progression as permanent range **2..3**, while ordinary BodyDefinitions exist only through tier 3.

Stock Repose raises raw max by +1, so terminal stock evaluates **2..4**. Because ordinary tier 4 does not exist, stock Repose adds no new corpse quality at that endpoint.

Accepted Rebalanced behavior:

- Bronze leaves the stock-style expanded range unchanged.
- Silver narrows to the highest actually existing eligible tier on its reliability branch.
- Gold always narrows to the highest actually existing eligible tier.
- Do not blindly set `tier_min = tier_max`; resolve the best existing ordinary BodyDefinition first.

This fixes the latent 0.1.5 late-game `4..4` edge case while keeping stock `GameSave.GenerateBody` authoritative.

Presentation rule:

- Vanilla/Bronze may replace the normal Repose effect line with **"No higher-quality bodies are available."** when stock +1 adds no higher ordinary tier.
- Rebalanced Silver/Gold keep their reliability wording while narrowing still changes the distribution.

## Imagination success rewards

Premium Story rewards use the stock prayer output/drop pipeline, not a separate inventory-grant hook:

- Bronze: no extra Story reward;
- Silver successful sermon: `story:2 x3`;
- Gold successful sermon: `story:3 x3`.

The items are projected into the prayer's existing `CraftDefinition.output`; non-Faith/non-money prayer outputs flow through the stock sermon-drop path.

## Runtime / performance shape

- No per-frame polling.
- No broad Unity scans.
- Rule projection is load-time/static.
- Soul Gratitude and Repose contextual presentation are computed only when the relevant pulpit UI redraws.
- Repose corpse-tier narrowing occurs only at the verified corpse-generation seam.
- Stock `GameSave.GenerateBody` remains authoritative.
- New player-facing strings ship in all 11 supported Graveyard Keeper interface languages.

## Acceptance status

PrayerClarity: Rebalanced 0.2.0 was user-tested and accepted for stable promotion on 2026-09-18.

Runtime-accepted in the tested candidate:

- revised Faith / Donations / Combo semantics and CQ requirements;
- Soul's Repose q30/60/120 and current Soul Gratitude pulpit metric;
- representative RU/EN/DE/JA rendering of the new pulpit context;
- synchronized pulpit / Technology / active-effect presentation for representative timed prayers;
- Test Harness + Rebalanced Compatibility operation with the candidate;
- representative stock-duration native buff activation;
- no PrayerClarity-specific exception/error in the supplied runtime log.

Deferred, non-blocking verification:

- terminal Repose wording/behavior still needs a save that has reached terminal Donkey corpse progression;
- physical 3-Story payout from a real successful Silver/Gold Imagination sermon has not yet been observed in the user's runtime, although it uses the already verified native non-Faith/non-money sermon-drop path.

These are recorded in `TEST_BUILD_LOG.md`. Any behavior change discovered later requires a new version; do not silently replace the accepted 0.2.0 bytes.
