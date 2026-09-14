# Prayer Rebalance Options — design hypotheses

Status: modeled candidate roster, 2026-09-14. **Nothing in this file is accepted gameplay behavior unless explicitly marked as an accepted product/design direction.** It translates verified stock mechanics and the quantitative power budget into concrete options before any production patch is written.

Canonical stock behavior remains in `PRAYER_MECHANICS.md`. Cost/progression evidence lives in `PRAYER_POWER_BUDGET.md`.

## Design objective

Use **temptation parity**, not numerical parity.

A prayer should be attractive enough to justify its unlock, craft/quality cost and weekly sermon slot in the stage/niche where it belongs. A narrow prayer may therefore be substantially stronger in its niche than Combo. Bronze should already be credible; silver/gold should create a visible reason to invest further.

Prefer buffs/new reasons to choose alternatives over nerfing familiar player rewards.

### Starter prayer boundary

The free starter `b_empty` / Ordinary Prayer is **not** part of the specialist-premium proposal below. Do not accidentally turn the opening sermon into the `+100%` Faith specialist.

The current rework target is the separately crafted `b_faith` Prayer for Faith. Keep Ordinary Prayer stock unless a later dedicated early-game analysis establishes a reason to change it.

---

## 1. Faith / Donations / Combo

### Stock problem

At equal quality:

- Faith and Combo have identical target Faith coefficients: `.5 / 1 / 1.5`;
- Donations and Combo have identical target money coefficients: `.5 / 1 / 1.5`;
- Donations and Combo are unlocked by the same `Price of faith` technology;
- specialists are cheaper (Chapter +5 Faith vs Book +7) and have lower q (10/20/50 vs 15/30/60), but after those gates are solved Combo is just as effective in either target resource while also supplying the other one.

This violates the intended specialist/generalist temptation structure more clearly than the simpler claim that Combo is merely “too strong.”

### Candidate FDC-A — modest specialist premium

Target coefficient for Faith/Donations:

- `.75 / 1.5 / 2.25`

Approximate target-resource total before fixed-output effects: `1.75x / 2.5x / 3.25x` base versus Combo `1.5x / 2x / 2.5x`.

**Assessment after modeling:** too conservative. Rounding can make the bronze premium disappear in realistic low-Faith states, and gold still does not create the strongest quality chase.

### Candidate FDC-B — clean strong specialist premium

Target coefficient:

- **`1.0 / 2.0 / 3.0`**

Approximate target-resource total before fixed-output effects: **`2x / 3x / 4x` base** versus Combo `1.5x / 2x / 2.5x`.

This is now the **leading design hypothesis** because it is both strong and legible: +100% / +200% / +300% target bonus.

Keep Combo stock initially. Keep the specialists' small secondary coefficient (`Faith k_money=.2`, `Donations k_faith=.2`) in the first candidate so they remain strongly specialized rather than artificially zeroing the secondary reward.

### Candidate FDC-C — irregular quality-diverging premium

Target coefficient:

- `.75 / 1.75 / 3.0`

Earlier this looked attractive because the relative gap grows aggressively with quality. Representative-output modeling changed that judgement: it is harder to communicate, and the bronze tier is unnecessarily sensitive to rounding.

**Assessment:** retain as fallback only.

### Representative-state model

These are **design test states**, not claims about the typical player's exact ratings. No Eloquence/Cardinal is assumed so the comparison isolates the prayer relationship.

- early bronze: Church 20 / Graveyard 50;
- mid silver: Church 40 / Graveyard 100;
- late gold: Church 80 / Graveyard 200.

Ordinary base values at those states are approximately:

- Faith: `4 / 8 / 16`;
- donations: `1.5s / 3s / 6s`.

Fixed +1/+2/+3 Faith and +1/+2/+3 silver are included below.

#### Stock Combo vs leading FDC-B

| State | Stock Combo | Faith specialist FDC-B | Donations specialist FDC-B |
| --- | --- | --- | --- |
| Early bronze | **7 Faith / 3.25s** | **9 Faith / 2.8s** | **6 Faith / 4.0s** |
| Mid silver | **18 Faith / 8.0s** | **26 Faith / 5.6s** | **12 Faith / 11.0s** |
| Late gold | **43 Faith / 18.0s** | **67 Faith / 10.2s** | **22 Faith / 27.0s** |

This produces the desired shape:

- Combo remains the obvious **generalist**;
- Faith gives up meaningful money to become the clear Faith maximum;
- Donations gives up meaningful Faith to become the clear money maximum;
- quality increasingly rewards deliberate specialization;
- no existing Combo reward is reduced.

**Current preferred first candidate: FDC-B.**

### Thematic Donations / Graveyard Quality lever

The user's thematic requirement idea remains useful, but the representative model shows a reason **not to add a second gate immediately**.

Base donations already scale with Graveyard Quality. Increasing `k_money` means the specialist premium itself automatically grows with Graveyard Quality. In other words, the reworked Donations prayer already rewards a better cemetery more strongly without adding a new rule.

Preferred order now:

1. first test FDC-B with ordinary church-based sermon success and existing GQ-based donation baseline;
2. if Donations is still too easy/unbounded, add a **visible Graveyard Quality threshold** for the specialist premium;
3. consider a hybrid church + graveyard gate only if needed;
4. avoid graveyard-only sermon success unless simpler designs fail.

This preserves the intuitive grammar: **church = can I deliver the sermon; graveyard = how much money is there to amplify?**

---

## 2. Prayer for Repentance

### Verified stock baseline

- ordinary confession chance: 15% per confessional roll;
- successful confession observed to yield **1 Faith + 1 Story** (Story quality can vary; its exact distribution is not modeled here);
- prayer special effect has no working consumer in stock 1.407;
- recipe: Chapter +5 Faith;
- q: 10/20/40;
- duration: 18/36/54 min;
- role “more confessions” is recoverable, exact vanilla magnitude is not.

Therefore every working implementation below is **Balance / Rework**.

One game day is about 7.5 real minutes, so the buff windows are roughly 2.4 / 4.8 / 7.2 daily-roll intervals. Expected values below are continuous planning approximations; actual results are discrete/random and depend on sermon timing.

### Candidate R-A — clean multiples of base

Chance:

- `30% / 45% / 60%`

Additional expected confessions **per confessional** over the full corresponding buff window compared with vanilla 15%:

- `+0.36 / +1.44 / +3.24`

With two simultaneously usable confessionals that would scale to roughly `+0.72 / +2.88 / +6.48`.

**Assessment:** elegant 2x/3x/4x base chance, but silver/gold may still undersell the prayer fantasy.

### Candidate R-B — stronger quality progression

Chance:

- **`30% / 50% / 70%`**

Additional expected confessions per confessional:

- `+0.36 / +1.68 / +3.96`

For two confessionals, approximately:

- `+0.72 / +3.36 / +7.92`

Each additional successful event contributes roughly +1 Faith plus one Story, so gold can become a deliberate Faith/writing-material production week rather than a barely observable probability change.

**Current preferred first candidate: R-B.**

The user's original `15% -> 30%` intuition works well here as **bronze**, not necessarily as the entire prayer ceiling.

### Candidate R-C — event-week fantasy

Chance:

- `30% / 60% / 100%`

Additional expected events per confessional:

- `+0.36 / +2.16 / +6.12`

For two confessionals: `+0.72 / +4.32 / +12.24`.

**Pros:** gold feels truly divine and creates a deterministic “confession week.”

**Cons:** potentially excessive Story/Faith generation; 100% deletes uncertainty; reserve if R-B tests too weak.

---

## 3. Shoots and Roots

### First step is not a rebalance

Repair the proven stock scope mismatch so the existing `-20%` growth-time term actually sees the prayer state.

Keep initially:

- q 10/20/30;
- effect magnitude -20%;
- duration 36/72/108 min;
- existing Faith/money prayer coefficients.

Classification: **Vanilla Fix**.

The repaired effect applies broadly enough that its real power cannot be judged from the old broken experience. Runtime-test the repaired -20% first; only then decide whether temptation parity requires more.

---

## 4. Retribution / Protection — merge direction and save-safe migration

### Verified stock budget problem

Both are separate prayers:

- shared deep `Martial skills` unlock after a substantial Smithing route;
- each uses Hard Book +7 Faith;
- each separately spends the weekly sermon slot;
- Retribution gives +5 damage;
- Protection gives +4 armor;
- duration 36/72/108 min.

The user preference is now clear: if redesigning the game from scratch, these two one-stat sermons would be better represented by **one genuinely strong combat-preparation prayer**, potentially including regeneration.

The main mod-specific concern is migration: existing saves can already contain either or both prayer items.

### Preferred architecture C-C2 — soft merge with legacy alias

Do **not** delete or rewrite saved prayer items and do **not** require a third permanent item ID.

Preferred Rebalanced-profile design:

1. choose one existing ID (current candidate: `b_sword`) as the canonical **Combat Prayer**;
2. give it the combined combat package (offense + defense + a bounded regeneration component, exact numbers still open);
3. stop offering the old `b_shield` recipe/unlock as a separate new-player choice in the Rebalanced presentation;
4. preserve every existing `b_shield` item already present in inventories/saves;
5. treat `b_shield` at runtime as a **legacy alias** of the canonical Combat Prayer, mapping bronze/silver/gold to the same-quality combined effect;
6. do not mutate the saved item ID merely to “clean up” the save;
7. if the mod/profile is removed, vanilla sees the original IDs again and the save is not structurally damaged.

This solves the transition problem without making existing crafted prayers disappear or become worthless.

A player who already crafted both simply owns two usable copies/qualities of the same effective combat sermon while Rebalanced is active. One can be sold/destroyed normally if redundant.

**Accepted design direction:** use a compatibility alias/hide strategy rather than destructive item migration if the two combat prayers are merged.

### Combat package constraints

The combined bronze prayer can be substantially stronger than either stock half because it is replacing **two** deep book-sermons and still consumes a weekly slot.

A package containing damage + armor + regeneration is therefore a legitimate candidate, not automatically excessive.

However:

- armor is flat subtraction, so aggressive `+armor` quality scaling can trivialize low-damage enemies;
- copying the stock restoring-potion tick unchanged across 36/72/108 minutes would provide far too much healing;
- regeneration should therefore be a **mod-owned bounded value** or combat/dungeon-local rule rather than blindly reusing the short potion's full tick rate;
- avoid attaching global movement speed merely to make the package longer; that changes traversal rather than combat identity.

Numbers remain intentionally open until the candidate combat package is modeled as a whole.

### Fallback C-A — keep two prayers

If hiding/aliasing an existing prayer proves technically fragile or creates unacceptable compatibility issues, fall back to two quality-scaled thematic prayers. This is no longer the preferred product design, but it remains the low-risk engineering fallback.

---

## 5. Prosperity / Repose / Imagination / Souls Repose — quality-value re-audit

The earlier blanket “keep these healthy prayers stock” judgement was too broad. Their **roles** may be healthy while their bronze/silver/gold progression differs substantially.

### Prosperity — keep stock unless runtime testing contradicts it

Stock quality already changes the permanent output directly:

- bronze: 1 Blessing of Commerce;
- silver: 2;
- gold: 3.

Blessings can be spent across vendors and permanently accelerate access to higher vendor tiers. That gives bronze an immediate progression use and makes gold literally triple the special output.

**Current verdict:** role and quality progression already satisfy temptation parity. The main Clarity work is to explain explicitly that quality changes **how many permanent merchant-progression items** are produced.

Natural obsolescence after the relevant vendors are already developed is acceptable.

### BSS Soul's Repose — keep stock unless dynamic preview exposes a real issue

This prayer's special value is not a flat buff: Soul Gratitude enters the **Faith baseline itself**, after which prayer quality still applies the ordinary `.5 / 1 / 1.5` Faith-bonus progression.

When Soul Gratitude is high relative to Church Quality, the entire baseline can already be far above ordinary sermons, so silver/gold amplify a stronger underlying value rather than merely extending a timer.

**Current verdict:** mechanically healthy and potentially one of the strongest Faith choices. The primary defect is discoverability; the pulpit must show the actual current Faith outcome so the player can see when it beats Combo/Faith.

### Imagination — role healthy, quality progression rework candidate

Stock special magnitude is **+0.7 writing quality at every prayer tier**. Only duration changes 18/36/54 min (plus ordinary sermon reward scaling).

The +0.7 itself is powerful: community examples show it can turn a planned writing batch into a major progression/profit burst. But this creates a quality-design problem: writing can be stockpiled and processed in a short planned session, so bronze's 18-minute window can already cover the activity; silver/gold duration may then have low marginal value.

**Rework candidate I-A:** preserve stock bronze and let prayer quality also improve magnitude:

- bronze `+0.7`;
- silver `+1.0`;
- gold `+1.3`;
- keep 18/36/54 min initially.

Because writing quality uses fractional tier math, +0.3 is roughly a 30-percentage-point shift toward the next quality when no integer boundary is crossed. This gives silver/gold a visible production advantage rather than only more spare time.

The exact curve is a design hypothesis, not accepted behavior.

### Repose — role healthy, stock quality progression is suspect

Stock Repose always supplies `body_max +1`; prayer quality changes duration 18/36/54 min but **not** the corpse-tier magnitude.

The user's proposed `+1/+2/+3` is attractive conceptually, but direct runtime/story evidence makes it risky: each `+1` is a whole Donkey corpse tier and the corpse-tier progression has only a few story steps. A gold `+3` could therefore leap several intended progression tiers rather than merely making the same niche stronger.

Preferred design goal: make higher quality improve **consistency of better corpses without skipping more than the next story tier**.

Candidate RPO-A:

- bronze: stock behavior — extend the pool to the next tier (`max +1`);
- silver: bias/raise the delivery floor as well as preserving access to the next tier, so low-tier results become less common;
- gold: while the buff is active, make the Donkey reliably deliver from the best tier available under the prayer (`current story max +1`) rather than accessing `+2/+3` future story tiers;
- retain 18/36/54 min initially.

This makes gold genuinely desirable (“best boosted corpses reliably”) while preserving the prayer's natural obsolescence at the final corpse tier and avoiding major story/progression skips.

Implementation can use the already verified live `Tier min` / `Tier max` Donkey path; exact probability/floor semantics must be specified before coding.

### Excellence — reopen for the same quality reason

Excellence is narrower than Imagination and stock magnitude is fixed `+0.2` at all qualities, again with only 18/36/54 min duration scaling. External/current player evidence shows real value for specific high-quality marble/chisel/book crafts, but not enough breadth to assume duration-only gold is satisfying.

**Current status:** reopen as a second-tier quality-progression candidate after Imagination. A simple candidate family such as `+0.2 / +0.4 / +0.6` is worth modeling, but no value is selected yet.

---

## 6. Requirements / progression policy

Stronger reworked prayers may justify higher requirements, but requirements should communicate theme rather than act as arbitrary taxes.

Current rules:

- **church quality** = universal ability to deliver a sermon successfully;
- **thematic state** = optional threshold/scaler only where it creates a useful and obvious relationship;
- prefer existing natural scaling before adding a new gate;
- avoid two continuous multipliers from the same state;
- prefer visible thresholds over hidden coefficients;
- do not add a second requirement merely for symmetry.

Donations/Graveyard Quality remains the first thematic candidate, but FDC-B should be tested **without** an added GQ gate first because GQ already naturally scales both the base pool and the stronger specialist bonus.

---

## Current candidate roster status

Current leading hypotheses/directions:

- **Ordinary:** stock; explicitly excluded from Faith-specialist buff;
- **Faith / Donations / Combo:** lead = **FDC-B** (`specialist target k = 1 / 2 / 3`, Combo stock);
- **Repentance:** lead = **R-B** (`30% / 50% / 70%` confession chance, stock tier durations);
- **Shoots and Roots:** stock-intent **-20% Vanilla Fix**, no extra balance buff yet;
- **Combat:** preferred architecture = **C-C2 soft merge** into one Combat Prayer with save-safe legacy alias; exact combat stats/regen open;
- **Prosperity:** stock mechanics/quality scaling currently healthy;
- **Repose:** rework candidate focused on quality/consistency, not `+1/+2/+3` story-tier skipping;
- **Imagination:** magnitude quality-scaling candidate, lead for modeling `+0.7/+1.0/+1.3`;
- **Excellence:** reopen quality scaling after Imagination;
- **BSS Soul's Repose:** stock mechanics currently healthy; dynamic Clarity is the main change;
- **Soul Contentment:** clarity first;
- **Thorough Cleansing:** stock x2 remains the strong-specialist benchmark.

None of the numerical rework curves are accepted gameplay yet.

## Next gate

Before production implementation:

1. specify and model the combined Combat Prayer package, including a bounded regeneration rule;
2. model Repose RPO-A against actual story corpse-tier ranges so gold is powerful without progression skipping;
3. model Imagination I-A (and then Excellence) against real quality-crafting probabilities;
4. assemble one complete non-production `Rebalanced` specification using FDC-B, R-B, repaired Roots, the combat merge, accepted quality reworks, and stock values for healthy prayers;
5. verify technology/item/pulpit wording can all be generated from the same semantic model;
6. only then create a `dev/*` branch and first integrated runtime candidate.

No user in-game test is required until that candidate exists.
