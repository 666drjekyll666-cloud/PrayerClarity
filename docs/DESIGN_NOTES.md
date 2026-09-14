# PrayerClarity — Design Notes

Status: product/design stage, 2026-09-14. Stock mechanics, presentation audit, community cross-check, quantitative power budget and a complete first non-production Rebalanced roster are sufficiently modeled. No gameplay implementation or numerical rebalance is accepted yet.

Canonical documents:

- `PRAYER_MECHANICS.md` — stock 1.407 truth;
- `PLAYER_UX_RESEARCH.md` — player/presentation evidence;
- `PRAYER_DESIGN_AUDIT.md` — role/design verdicts;
- `PRAYER_POWER_BUDGET.md` — full investment/opportunity-cost model;
- `PRAYER_REBALANCE_OPTIONS.md` — concrete current candidate roster.

## Product architecture

One codebase may expose three semantic layers:

1. **Clarity** — information only;
2. **Vanilla Fixes** — evidence-backed repair where stock intent/magnitude are recoverable;
3. **Balance / Rework** — explicit new design/tuning.

Possible profiles remain `Vanilla + Clarity`, `Fixed Vanilla`, and `Rebalanced`. Stock behavior must remain separately documented.

## Permanent policy

Do not nerf failed-sermon base donations. Preserve the stock full base donation pool on failure in every profile; only prayer-specific bonuses/special success outputs disappear.

## Core design target

Use **temptation parity**. A prayer is a strategic purchase whose real budget includes unlock depth, technology points, writing/craft resources, quality effort, success requirements and the weekly sermon slot.

Every prayer should offer a compelling reason to want it in its stage/niche. Bronze should already be credible; silver/gold should materially improve value. A narrow prayer may be much stronger than a universal prayer in its niche.

## Presentation architecture

Prayer information is not built by one universal vanilla tooltip. Technology unlock, prayer item, pulpit choice and active buff HUD are separate paths. Probe 0.1.6 established that active timed buffs effectively communicate icon + remaining time, not quantitative meaning.

Build one mod-owned semantic model and render context-appropriate subsets:

- **technology tree:** role, why unlock it, quality progression;
- **prayer item:** exact static properties of that quality;
- **pulpit:** current success/failure Faith, donations, special output/effect and duration;
- **active buff:** optional later quantitative hover; do not rely on vanilla HUD for explanation.

Preview calculations must be side-effect free; never call `PrayLogics.CalculatePray` just to render information.

## Current leading Rebalanced roster

These are design hypotheses pending implementation/runtime acceptance:

- Ordinary: stock;
- Faith: target Faith bonus +100/+200/+300%;
- Donations: target donations +100/+200/+300%;
- Combo: stock +50/+100/+150% both;
- Repentance: 30/50/70% confession chance;
- Shoots & Roots: repair existing stock -20% growth path;
- Combat: merge Retribution/Protection save-safely; +5/+8/+12 damage, constant +4 armor, 1 HP/min slow regeneration, 36/72/108 min;
- Repose: bronze stock pool expansion; silver 85% best prayer-eligible corpse tier; gold 100%; never exceed normal story max +1;
- Imagination: +0.7/+0.9/+1.1 writing-quality input;
- Excellence: +0.2/+0.3/+0.4 linked-craft quality input;
- Prosperity: stock 1/2/3 Blessings;
- BSS Soul's Repose: stock mechanics, dynamic Faith forecast;
- Soul Contentment: stock initially;
- Thorough Cleansing: stock x2 Sin Shards.

Full rationale and fallback options live in `PRAYER_REBALANCE_OPTIONS.md`.

## Specialist/generalist policy

The current preferred solution to Combo choice compression is **not a Combo nerf**. Faith and Donations should become true specialists while Combo remains a strong familiar generalist.

Church quality stays the universal sermon-success gate. Graveyard Quality already creates donation base value; do not add a second GQ gate until runtime evidence shows the stronger Donations specialist needs one.

## Broken-prayer policy

### Shoots and Roots

Classify as Vanilla Fix: stock contains both the intended -20% growth term and the prayer state; only scope wiring is disconnected.

### Repentance

Classify as Balance/Rework: stock reveals the intended “more confessions” role and the 15% base roll, but no surviving rule specifies how the prayer should change it. Current lead is 30/50/70%.

## Combat migration policy

If the combat merge is implemented:

- use an existing prayer ID as canonical (`b_sword` candidate);
- preserve every existing `b_shield` item in saves as a same-quality legacy alias;
- do not destructively rewrite inventories/saves;
- hide the redundant new-player recipe only after exact lifecycle/unlock behavior is verified;
- profile/mod removal must leave the vanilla save structurally valid.

## Quality progression findings

Duration-only scaling is not automatically healthy. It works when longer duration meaningfully crosses later sermon weeks, but can be redundant when the relevant work can be batched quickly.

This is why Imagination and Excellence now have conservative magnitude scaling candidates, while Repose quality is modeled as better reliability rather than +2/+3 future story tiers.

## Current gate

Broad research is done. The next step is **not another general probe** and not more community browsing by default.

After user review of the modeled roster:

1. inspect exact implementation/lifecycle/Harmony targets for the shared semantic model and each narrow gameplay change;
2. verify save-safe combat alias/hide behavior without destructive migration;
3. open a `dev/*` branch;
4. implement the smallest coherent integrated prototype;
5. build one numbered candidate only when it is ready for an actual in-game acceptance test.

No user runtime action is currently required.