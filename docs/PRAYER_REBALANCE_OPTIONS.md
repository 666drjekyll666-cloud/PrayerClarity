# Prayer Rebalance Options — design hypotheses

Status: candidate roster, 2026-09-14. **Nothing in this file is accepted gameplay behavior.** It translates the verified stock mechanics and quantitative power budget into concrete options for comparison before any production patch is written.

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

This violates the intended specialist/generalist temptation structure more clearly than any raw claim that Combo is simply “too strong.”

### Option FDC-A — modest specialist premium

Keep Combo completely stock. Change the target coefficient of Faith/Donations to:

- bronze `.75`
- silver `1.5`
- gold `2.25`

Ignoring the fixed +1/+2/+3 output for comparison, target-resource total becomes approximately:

- specialist `1.75x / 2.5x / 3.25x` base;
- Combo `1.5x / 2x / 2.5x` base.

So the specialist is roughly 17% / 25% / 30% ahead in its target before fixed-output effects.

**Pros:** conservative; preserves Combo; early chapter/q advantages remain meaningful.

**Cons:** gold specialist may still not feel dramatic enough for the explicit “I want this instead of Combo” goal.

### Option FDC-B — strong specialist premium

Target coefficient:

- `1.0 / 2.0 / 3.0`

Target-resource total becomes `2x / 3x / 4x` base versus Combo `1.5x / 2x / 2.5x`, approximately 33% / 50% / 60% ahead before fixed components.

**Pros:** unmistakable specialization; every quality tier is attractive; gold becomes a real strategic target.

**Cons:** could make ordinary Faith/Donations too efficient if tech/quality progression is already trivial by the time gold is available.

### Option FDC-C — quality-diverging specialist

Target coefficient:

- `.75 / 1.75 / 3.0`

This keeps bronze only modestly better than Combo but makes quality investment increasingly meaningful: target-resource total `1.75x / 2.75x / 4x` base versus Combo `1.5x / 2x / 2.5x`.

**Pros:** best match for “bronze is good; silver/gold should be tempting”; preserves Combo as the obvious convenient generalist while rewarding deliberate specialist investment.

**Cons:** less mathematically regular than a single fixed premium ratio.

### Current preferred hypothesis

**FDC-C** is the strongest design direction to test first.

Do not remove the specialists' small secondary coefficient (`Faith` currently has `k_money=.2`, Donations `k_faith=.2`) in the first test; their identity can remain “strongly specialized” rather than “literally zero secondary benefit.”

### Thematic Donations requirement

Do not immediately replace church-based sermon success with graveyard quality.

Preferred experiment:

- church quality continues to determine success;
- the enhanced **specialist premium** is associated with a visible Graveyard Quality target/threshold;
- below that target the player still has a functioning prayer, but the specialist ceiling is not fully realized.

Exact GQ thresholds are intentionally unset until representative graveyard progression values are modeled. Avoid continuous GQ-on-GQ multiplication because base donations already scale with graveyard quality.

If this extra rule adds more complexity than interesting choice, drop it; the specialist coefficient alone may solve the problem.

---

## 2. Prayer for Repentance

### Verified stock baseline

- ordinary confession chance: 15% per confessional roll;
- successful confession observed to yield **1 Faith + 1 Story**;
- prayer special effect has no working consumer in stock 1.407;
- recipe: Chapter +5 Faith;
- q: 10/20/40;
- duration: 18/36/54 min;
- role “more confessions” is recoverable, exact vanilla magnitude is not.

Therefore every working implementation below is **Balance / Rework**.

For rough comparison, one game day is about 7.5 minutes and the prayer windows correspond to about 2.4 / 4.8 / 7.2 days. The estimates below assume two confessionals and treat the daily roll continuously for expected-value comparison; actual discrete rolls depend on sermon timing.

### Option R-A — clean multiples of base

Chance while active:

- bronze 30%
- silver 45%
- gold 60%

Compared with stock 15%, rough expected **additional** confession events over the full tier-specific window with two confessionals:

- bronze `+0.72`
- silver `+2.88`
- gold `+6.48`

Each extra event also means roughly +1 Faith +1 Story.

**Pros:** 2x/3x/4x base chance is internally elegant; quality changes both power and practical return.

**Cons:** bronze special effect remains modest; gold is good rather than spectacular.

### Option R-B — stronger player-facing progression

Chance:

- `30% / 50% / 70%`

Approximate extra events across two confessionals:

- `+0.72 / +3.36 / +7.92`

**Pros:** round player-facing values; silver/gold become visibly more compelling; still leaves failure/randomness.

**Cons:** numbers are our design rather than derived multiples of vanilla.

### Option R-C — event-week fantasy

Chance:

- `30% / 60% / 100%`

Approximate extra events:

- `+0.72 / +4.32 / +12.24`

Gold turns the effect window into essentially guaranteed daily confessions for every available confessional.

**Pros:** gold feels genuinely divine and clearly worth pursuing; creates a strong writing/Faith production week.

**Cons:** may flood Stories/Faith, especially with two confessionals; 100% removes uncertainty entirely; likely too aggressive without runtime economy testing.

### Current preferred hypothesis

**R-B (`30/50/70%`)** is the best first candidate.

It treats the user's initial 30% intuition as a good bronze value rather than the final ceiling. It is strong enough to create a quality chase without immediately guaranteeing every roll.

Before acceptance, test against actual confessional count, Story quality distribution and normal Faith/writing progression. If R-B is still too weak in practice, R-C is the next direction; do not silently lengthen the duration and call the problem solved.

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

### Why no extra buff yet

The repaired effect applies to a surprisingly broad family of plant/growth crafts, including ordinary farming and other growth systems found in current balance data. Its long silver/gold duration can also span later sermon weeks.

The effect should be runtime-tested *after repair* before deciding that -20% is too small. This is a case where a formerly invisible/broken benefit may already be healthy once it functions and is clearly described.

---

## 4. Retribution / Protection

### Stock budget problem

Both are separate prayers:

- shared deep `Martial skills` unlock after a substantial Smithing path;
- each uses Hard Book +7 Faith;
- each separately spends the weekly sermon slot;
- Retribution gives +5 damage;
- Protection gives +4 armor;
- duration 36/72/108 min.

The numbers are not tiny; the proposition is narrow.

### Option C-A — quality-scaled single-stat prayers

Keep identities unchanged but let quality increase magnitude as well as duration.

Retribution could progress from the stock +5 bronze toward materially higher silver/gold damage; Protection similarly scales armor.

**Pros:** smallest behavioral change; quality becomes exciting.

**Cons:** armor/damage scaling can become extreme quickly; does not solve the fundamental split into two expensive one-stat sermons.

No concrete values should be chosen before the damage/armor formula and enemy context are modeled.

### Option C-B — two full thematic packages

Keep two prayers but broaden each:

- **Retribution:** a clearly offensive package using verified existing combat parameters/effects;
- **Protection:** a clearly defensive/survival package using verified existing combat parameters/effects.

Quality can scale magnitude, package breadth, duration, or a combination.

**Pros:** preserves both items/technology identities; every weekly choice can feel consequential.

**Cons:** requires careful selection of existing stable game parameters; greater compatibility surface.

### Option C-C — one combat-preparation prayer + repurpose the second

Merge offense/defense value into one high-value combat sermon, then give the second prayer a distinct new role.

**Pros:** directly solves “why would I spend two separate weeks/items on two halves of one combat preparation?”

**Cons:** largest thematic redesign; requires deciding what the repurposed prayer becomes; least vanilla-like.

### Current preferred hypothesis

**C-B** should be investigated before C-C. It preserves recognizable vanilla identities while fixing the one-dimensional value proposition.

Do not choose package contents from imagination. First inspect already-supported player parameters/buff mechanics and combat formulas; then select the smallest stable package that produces a real offensive/defensive fantasy.

---

## 5. Requirements / progression policy

Stronger reworked prayers may justify higher requirements, but requirements should communicate theme rather than act as arbitrary taxes.

Current principles:

- **church quality** = universal ability to deliver a sermon successfully;
- **thematic state** = optional requirement/scaler for extracting the full specialist benefit;
- avoid two continuous multipliers from the same state;
- prefer visible thresholds over hidden coefficients;
- do not add a second gate where it does not create a meaningful decision.

Donations/Graveyard Quality is the first candidate for this pattern. Soul Gratitude already demonstrates a natural state-dependent prayer system.

---

## 6. What remains unchanged unless evidence changes

Current healthy/reference prayers should not be modified merely because a Rebalanced profile exists:

- Ordinary: starter baseline;
- Prosperity: strong merchant progression tool;
- Repose: finite +1 corpse-tier progression tool;
- Imagination: strong writing-production window;
- Excellence: clarity first, then judge affected-craft usefulness;
- BSS Soul's Repose: strong state-dependent Faith alternative;
- Soul Contentment: clarity first;
- Thorough Cleansing: strong x2 specialist benchmark.

A rebalanced roster is allowed to leave many prayers numerically stock.

---

## Next gate

Before production implementation:

1. model FDC-A/B/C at representative early/mid/late church + graveyard states and select one specialist curve;
2. model R-A/B/C against confessional throughput and writing/Faith value; current lead is R-B;
3. inspect the stable combat parameter/formula surface and narrow C-B into an actual package;
4. keep Shoots/Roots at the recoverable -20% for its first runtime test;
5. then assemble a single candidate `Rebalanced` roster and verify that every description/forecast can be generated from the same semantic model.

Only then create a production/dev branch and first integrated runtime candidate.