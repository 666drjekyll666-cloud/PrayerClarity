# PrayerClarity: Rebalanced — strong candidate specification

Status: **strong pre-production design candidate, accepted for implementation planning on 2026-09-18**.

This document is the single concise source of truth for the **next Rebalanced candidate**. It does **not** describe the current stable Rebalanced 0.1.5 release and must not be treated as already implemented or runtime-accepted.

Current stable behavior remains documented in `PRAYER_REBALANCE_OPTIONS.md`.

Historical alternatives, calculations and rejected variants remain in `REBALANCE_FAIRNESS_AUDIT.md`.

## Product intent

The candidate keeps the accepted PrayerClarity product philosophy:

- make each prayer a tempting purchase and weekly choice at the stage where it belongs;
- prefer clear roles over numerical sameness;
- allow progression prayers to become obsolete naturally;
- let narrow prayers be deliberately strong in their niche;
- use existing game progression costs before inventing new currencies or maintenance chores;
- keep the player-facing choice understandable without expected-value math;
- preserve the sermon animation as the reveal moment for exact Faith/donation payout;
- keep all runtime work narrow, event-driven/on-demand and tied to verified game state.

The ordinary economic progression should read naturally as:

`early focused specialists -> mature specialists / early Combo -> late premium Combo`

Better Save Soul adds an optional second endgame axis rather than replacing that progression.

## Vanilla balance basis

All balance decisions use **stock Graveyard Keeper 1.407 time and progression**.

PrayerClarity: Rebalanced does **not** rebalance prayer durations.

Canonical vanilla duration conversion:

- one day = 450 seconds = 7.5 real minutes;
- 18 / 36 / 54 min = 2.4 / 4.8 / 7.2 vanilla days;
- 36 / 72 / 108 min = 4.8 / 9.6 / 14.4 vanilla days.

External day-length mods may change observed runtime day counts in the user's installation. They are compatibility/test context only and are not balance targets.

## Strong candidate roster

| Family | Proposed 100% CQ | Proposed Rebalanced behavior |
| --- | ---: | --- |
| Ordinary `b_empty` | **10** | Stock starter baseline. |
| Faith `b_faith` | **20 / 40 / 60** | Success bonus is flat only: **+5 / +10 / +20 Faith**. No prayer-owned donation bonus. |
| Donations `b_money` | **20 / 40 / 60** | Success bonus is flat only: **+5 / +10 / +15 silver**. No prayer-owned Faith bonus. |
| Combo `b_faith_money` | **40 / 60 / 80** | Success bonus is percentage only: **+100 / +150 / +200% Faith and donations**. No prayer-owned flat Faith/money. |
| Prosperity `b_village` | **10 / 20 / 30** | Keep stock 1 / 2 / 3 Commercial Blessings and stock sermon outputs. |
| Shoots & Roots `b_plant` | **10 / 30 / 50** | Repair stock scope; growth time **-20 / -30 / -40%**. Duration remains **36 / 72 / 108 min**. |
| Repentance `b_sins` | **20 / 40 / 60** | Daily confession probability **50 / 75 / 100%**. Duration remains **18 / 36 / 54 min**. |
| Repose `b_skull` | **20 / 40 / 50** | Bronze stock-style expanded pool; Silver halfway from stock best-tier probability to certainty; Gold guarantees the best eligible corpse tier. Duration remains **18 / 36 / 54 min**. |
| Combat `b_sword` (`b_shield` legacy alias) | **20 / 40 / 60** | Damage **+5 / +10 / +15**, armor **+4**, regeneration **1 / 2 / 4 HP/s**. Duration remains **36 / 72 / 108 min**. |
| Imagination `b_pen` | **20 / 40 / 60** | Writing quality **+0.7** at all tiers; successful Silver gives **3 Silver Stories**, Gold gives **3 Gold Stories**. Duration remains **18 / 36 / 54 min**. |
| Excellence `b_star` | **20 / 60 / 90** | Linked-craft quality **+0.2 / +0.5 / +1.0**. Duration remains **18 / 36 / 54 min**. |
| BSS Soul's Repose `b_souls` | **30 / 60 / 120** | **+50 / +100 / +150% Faith** on the verified Souls base. Preserve current Soul Gratitude scaling; remove prayer-owned fixed/off-theme outputs. |
| BSS Soul Contentment `b_grat_points_incr` | **20 / 40 / 60** | **+20% Soul Gratitude** at all tiers. Duration remains **36 / 72 / 108 min**. |
| BSS Thorough Cleansing `b_sin_shard` | **30 / 60 / 90** | **x2 Sin Shards** at all tiers. Duration remains **36 / 72 / 108 min**. |

## Resource-family grammar

### Faith

The specialist uses **flat scaling only**:

- Bronze: +5 Faith;
- Silver: +10 Faith;
- Gold: +20 Faith.

The common sermon base remains unchanged and still scales from Church Quality. Eloquence continues to affect the base but not the flat prayer-owned bonus.

The +20 Gold value is intentional. At CQ60 without Eloquence, it keeps Gold Faith close to stock Gold target output while preserving the flat-only identity.

### Donations

The specialist uses **flat scaling only**:

- Bronze: +5 silver;
- Silver: +10 silver;
- Gold: +15 silver.

The common donation base remains unchanged and still scales from Graveyard Quality/Cardinal.

This makes Donations especially attractive in the early/mid economic window and lets it age out naturally as Graveyard Quality makes percentage scaling stronger.

### Combo

Combo uses **percentage scaling only**:

- Bronze: +100% Faith and donations;
- Silver: +150%;
- Gold: +200%.

No flat prayer-owned Faith or money.

This creates the intended categorical choice:

- need Faith -> Faith specialist;
- need money -> Donations specialist;
- need both / mature late-game scaling -> Combo.

Representative stock-time/progression snapshot without Eloquence and with Cardinal for donations:

| State | Faith specialist | Available Combo Faith | Donations specialist | Available Combo donations |
| --- | ---: | ---: | ---: | ---: |
| CQ20 / GQ50 | Bronze **9** | Combo not guaranteed | Bronze **7s** | Combo not guaranteed |
| CQ40 / GQ100 | Silver **18** | Bronze **16** | Silver **14s** | Bronze **8s** |
| CQ60 / GQ150 | Gold **32** | Silver **30** | Gold **21s** | Silver **15s** |
| CQ80 / GQ200 | Gold **36** | Gold **48** | Gold **23s** | Gold **24s** |

The player should never need this table. It is internal validation that the visible roles form a coherent handoff.

## Better Save Soul endgame structure

### Soul's Repose

Verified stock base:

`Base Faith = (Church Quality + current Soul Gratitude) * 0.1 * EloquenceFactor`

The prayer scales from **current Soul Gratitude held at sermon time**, not lifetime souls healed and not Soul Gratitude capacity.

Candidate quality ladder:

- Bronze q30;
- Silver q60;
- Gold q120.

Gold q120 is the deliberate aspirational ritual above ordinary passive church progression. It rewards temporary preparation with candles/incense without turning every prayer into a recurring consumable tax.

Important nuance: q120 gates the **+150% success bonus**, not the Souls base itself. On sermon failure, the base Faith that already includes Soul Gratitude still remains.

Do not describe q120 as a hard lock on high Faith output.

### Soul Contentment

Keep q20/40/60.

The effect is strong but workflow-limited: it matters only while the player is actively gaining/using Soul Gratitude. The long Gold duration is itself a legitimate premium-tier reward.

Do not raise this prayer further merely for numerical symmetry.

### Thorough Cleansing

Use q30/60/90.

The x2 Sin Shard effect is already strong and Gold lasts 108 vanilla minutes / 14.4 vanilla days. The higher CQ ladder prices a powerful scarce-resource multiplier without requiring the q120 miracle treatment.

## Other prayer progression decisions

### Roots

Use q10/30/50.

The long Gold duration is accepted. Faster growth creates more player activity rather than a passive free resource stream. Do not late-game-gate the prayer merely because 108 minutes is long.

### Combat

Use q20/40/60 and keep the current merged damage/armor/regeneration package.

Combat is a narrow subsystem and not the dominant game economy. Its deep unlock and Book-class crafting are already meaningful costs. Do not overprice it because the paper stats/duration look large.

### Imagination

Normalize to q20/40/60.

The unlock route and Hard Book crafting already place it beyond the opening game, so q20 does not punish early progression and removes the awkward 10/40/60 ladder.

### Excellence

Use q20/60/90.

Gold +1.0 linked-craft quality is a qualitatively strong deterministic-quality tool, but it is bounded by craft-quality ceilings. q90 makes it an aspirational near-max passive effect without competing with Soul's Repose for the unique q120 role.

### Prosperity

Keep q10/20/30 and stock effect/output.

It is intentionally valuable early and naturally becomes obsolete when merchant-tier progression is complete. Raising q would move it out of the stage where its role exists.

### Repose

Keep q20/40/50.

Rebalanced reliability is bounded: it changes the distribution toward the best currently eligible tier but does not create a new corpse tier beyond normal progression + the stock prayer ceiling.

## Pulpit UX changes

### Soul Gratitude context metric

When Soul's Repose is selected, the pulpit should show a third current input metric:

`Soul Gratitude: <current gratitude_points>`

Show the **current value only**, not current/capacity, because current GP is the formula input.

The pulpit still does **not** show the exact resolved Faith payout before the sermon.

Implementation direction:

- extend the context-metric model generically;
- render the extra row only for Soul's Repose;
- use content-driven/dynamic context height rather than globally shrinking the font;
- no bespoke permanent panel for one prayer;
- runtime visual acceptance is required because the current context block was tuned for fewer lines.

### Repose endpoint replacement

Do not add endpoint text to Technology.

At the pulpit, when the selected Repose tier can no longer improve the corpse-quality/tier outcome, replace the normal effect sentence rather than appending another explanation.

Current RU normal wording:

`Осёл может привозить более качественные тела.`

RU endpoint target:

`Ещё более качественные тела недоступны.`

English semantic target:

`No higher-quality bodies are available.`

For Rebalanced Silver/Gold, keep the normal reliability wording whenever narrowing the eligible range still changes the distribution.

### Technology / item / Temporary Effects

Do not add explanatory prose just to justify q120 or other balance values.

All surfaces should continue to use the same effective semantic model. Technology remains the comparison/planning surface; prayer-item tooltip shows the concrete held quality; Temporary Effects shows live quantitative buff meaning/duration.

## Reward-reveal boundary

Keep the accepted pulpit contract:

`base dependency -> success-only prayer contribution -> special effect`

Show:

- current Church Quality;
- current Graveyard Quality;
- current Soul Gratitude for Soul's Repose;
- exact success probability;
- exact prayer-owned flat/percentage modifiers;
- exact intrinsic special effects and durations.

Do **not** show the fully resolved current Faith/donation payout before the sermon.

## Implementation architecture constraints

- mechanics and all Clarity surfaces must consume one Rebalanced semantic/rule source;
- fixed Faith/money specialist values must be represented in that rule model, not hard-coded in a display patch;
- reuse stock success-only Faith/money output mechanics rather than inventing a reward system;
- preserve stock base Faith/donations and the permanent project policy that failed sermons retain full base donations;
- use load-time/static projection or existing narrow runtime seams;
- compute Soul Gratitude and Repose contextual presentation only when the pulpit opens/redraws;
- no per-frame polling;
- no broad Unity scans;
- no repeated heavy reflection/enumeration;
- no persistent verbose logging;
- every new player-facing string ships in all 11 supported locales.

## Evidence / acceptance gates

### Blocking before Repose endpoint implementation

Directly close the final Donkey/corpse-range question in stock 1.407:

1. establish the terminal effective `tier_min/tier_max` range;
2. verify what stock body generation does when the prayer's +1 maximum extends beyond the final real corpse tier;
3. verify whether Rebalanced Silver/Gold still improve the distribution at that endpoint;
4. derive the safe on-demand predicate for showing the endpoint replacement.

This should be one narrow evidence task, not a broad new research project.

### Runtime acceptance after implementation

The next candidate build needs focused checks only for changed behavior:

1. Soul's Repose pulpit shows current Soul Gratitude and still fits cleanly;
2. Repose shows normal vs endpoint wording in the correct states;
3. representative Faith / Donations / Combo effects and q thresholds match this document;
4. Gold Soul's Repose correctly shows q120 / its resulting success chance;
5. representative changed q values across the rest of the roster are reflected consistently in Technology/item/pulpit;
6. timed prayer durations remain stock;
7. Vanilla sibling remains mechanically stock; shared presentation changes remain truthful.

Do not rerun already accepted Clarity tests without a concrete regression risk.

## Status boundary

This document is **design-accepted as the strong next candidate**, not runtime-accepted production behavior.

Until implementation, build and user runtime acceptance complete:

- `main` remains stable;
- Rebalanced 0.1.5 remains the current accepted release;
- `PRAYER_REBALANCE_OPTIONS.md` remains canonical for that stable release;
- this document is canonical only for the **next candidate design target**.
