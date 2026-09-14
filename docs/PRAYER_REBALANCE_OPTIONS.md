# Prayer Rebalance Options — design hypotheses

Status: modeled candidate roster, 2026-09-14. **Nothing in this file is accepted gameplay behavior.** It translates verified stock mechanics and the quantitative power budget into concrete options before any production patch is written.

Canonical stock behavior remains in `PRAYER_MECHANICS.md`. Cost/progression evidence lives in `PRAYER_POWER_BUDGET.md`.

## Design objective

Use **temptation parity**, not numerical parity.

A prayer should be attractive enough to justify its unlock, craft/quality cost and weekly sermon slot in the stage/niche where it belongs. A narrow prayer may therefore be substantially stronger in its niche than Combo. Bronze should already be credible; silver/gold should create a visible reason to invest further.

Prefer buffs/new reasons to choose alternatives over nerfing familiar player rewards.

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

## 4. Retribution / Protection

### Verified stock budget problem

Both are separate prayers:

- shared deep `Martial skills` unlock after a substantial Smithing route;
- each uses Hard Book +7 Faith;
- each separately spends the weekly sermon slot;
- Retribution gives +5 damage;
- Protection gives +4 armor;
- duration 36/72/108 min.

Combat formula evidence also makes their raw effects concrete:

- weapon `damage` and player `add_damage` are additive sources in the player attack model;
- incoming player damage subtracts equipped armor and then player `add_armor`, clamping at zero;
- stock already contains short +5 damage, +15 berserk damage, +4 armor, 1-HP periodic healing and movement-speed buff primitives.

So the engine offers several stable primitives, but not every one is appropriate for a multi-day sermon buff.

### Candidate C-A — quality-scaled single-stat prayers

Keep identities unchanged and make magnitude scale with quality as well as duration.

**Pros:** smallest mechanics/compatibility change.

**Cons:** still asks the player to buy/use a whole deep book-sermon for one stat; large armor values can trivialize low-damage enemies because armor is flat subtraction.

### Candidate C-B — two thematic packages

Keep two recognizable prayers but broaden their role:

- **Retribution:** clearly offensive package;
- **Protection:** clearly defensive/survival package.

This remains the preferred architecture, but the static combat audit adds constraints:

- `add_damage` is safe/understood and can remain the offensive core;
- `add_armor` is safe/understood but must scale cautiously because it directly subtracts damage;
- the existing 1-HP periodic-heal primitive is dangerous over 36/72/108 minutes and should **not** simply be copied into Protection;
- the existing `speed_buff=1.5` would create a powerful general traversal benefit, not just combat identity, so it should **not** be casually attached to Retribution.

Therefore a “package” should not become a bag of unrelated potion buffs. Additional secondary effects need a real combat-role justification and bounded impact.

### Candidate C-C — merge combat preparation, repurpose one prayer

One sermon grants both offense and defense; the other prayer receives a distinct new role.

**Pros:** directly addresses the user's concern that two separate expensive weeks are currently needed for the two halves of combat preparation.

**Cons:** requires inventing a second role and is the least vanilla-like option.

### Current narrowing

**C-B remains preferred conceptually, but it is not ready for numerical specification.** The narrow static audit rules out naïvely adding long regeneration or global speed. Before choosing secondary effects, inspect only the existing combat event/attack hooks needed to identify a bounded effect such as on-hit, kill-related, durability/energy, or another genuinely combat-local primitive.

If no clean bounded secondary mechanism exists, prefer a transparent quality-scaled C-A over implementing a complicated custom combat subsystem.

---

## 5. Requirements / progression policy

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

## 6. What remains unchanged unless evidence changes

A Rebalanced profile is allowed to leave many prayers numerically stock:

- Ordinary — starter baseline;
- Prosperity — strong merchant progression tool;
- Repose — finite +1 corpse-tier progression tool;
- Imagination — strong writing-production window;
- Excellence — clarity first, then judge affected-craft usefulness;
- BSS Soul's Repose — strong state-dependent Faith alternative;
- Soul Contentment — clarity first;
- Thorough Cleansing — strong x2 specialist benchmark.

---

## Current candidate roster status

The first three families are now narrowed enough for an eventual integrated candidate:

- **Faith / Donations / Combo:** lead = **FDC-B** (`specialist target k = 1 / 2 / 3`, Combo stock);
- **Repentance:** lead = **R-B** (`30% / 50% / 70%` confession chance, stock tier durations);
- **Shoots and Roots:** lead = stock-intent **-20% Vanilla Fix**, no extra balance buff yet;
- **Retribution / Protection:** architecture lead = **C-B**, numerical/package details still intentionally open.

None is accepted gameplay yet.

## Next gate

Before production implementation:

1. perform one narrow combat-hook audit to determine whether C-B has a simple bounded secondary-effect path; if not, fall back to C-A;
2. assemble a single complete non-production `Rebalanced` specification using FDC-B, R-B, repaired Roots, the chosen combat design, and stock values for healthy prayers;
3. verify technology/item/pulpit wording can all be generated from the same semantic model;
4. only then create a `dev/*` branch and first integrated runtime candidate.

No user in-game test is required until that candidate exists.