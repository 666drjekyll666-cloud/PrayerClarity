# PrayerClarity — Rework research

Status: research/design synthesis, updated 2026-09-16 after the rework-focused community pass, user design review, and runtime probes 0.1.0 for Repentance and quality-prayer scope. No Balance/Rework mechanic in this document is accepted runtime behavior unless explicitly marked as a product/design rule.

## Baseline

- Target: Graveyard Keeper 1.407.
- Stable presentation baseline: PrayerClarity 1.0.0, accepted runtime/source `493d2168489af80b5c1305f7ff1435e2ee1dd0d7`.
- Stock mechanics remain canonical in `PRAYER_MECHANICS.md`.
- Rebalanced values remain hypotheses until narrowed and runtime/user-tested as required.
- Failed-sermon full base donations remain intentionally preserved in every future profile.
- Product direction: one coherent opinionated **Rebalanced** profile, not per-prayer sliders/configuration.
- The isolated Fixed Vanilla profile is no longer a design priority. The proven Shoots & Roots scope repair should be incorporated into Rebalanced while stock 1.407 remains independently documented.

## Rework-focused community pass — 2026-09-16

The evidence below is `community signal`, not mechanics proof.

Repeated signals:

1. Too few prayers compete for the weekly slot. Faith/Combo dominate routine sermons; Imagination, Prosperity, Repose and BSS Thorough Cleansing are situational; Combat, Repentance and Shoots & Roots are repeatedly described as not worth the weekly opportunity.
2. Combo compresses the specialist choice at equal quality. Faith/Donations do not beat Combo at their own target output when quality is equal. Their real stock advantages are easier Chapter crafting and lower church-quality requirements.
3. Donation income naturally loses strategic value when crates/tavern/other income systems mature. This supports a strong progression specialist rather than forced permanent endgame parity.
4. Imagination is a healthy early/mid-game writing bootstrap. This is evidence against reducing the stock Bronze `craft_q=+0.7` merely to manufacture a symmetric tier curve.
5. Prosperity is a healthy progression prayer: 1/2/3 permanent merchant-tier Blessings produce a clear quality ladder and natural obsolescence.
6. Repose has a meaningful but finite progression window. Reliability scaling can improve that existing fantasy without inventing permanent endgame relevance.
7. Combat prayers have proposition failure more than simple stat opacity: two deep Book prayers each consume a full sermon for one narrow activity, while cheap potions compete directly with them.
8. Repentance needs more than nominal functionality if confession throughput/reward remains too small to justify the week.
9. Thorough Cleansing is already a successful specialist: players change sermon rotation to obtain x2 Sin Shards. Do not inflate it merely for symmetry.
10. Soul Contentment is state-dependent rather than demonstrably weak. Its quality ladder still needs a specific useful-duration/value audit under the new quality rule below.

## Accepted design rules for Rebalanced

### 1. Temptation parity, not numerical parity

Each prayer must create a believable moment where spending the weekly sermon on it is attractive. A narrow prayer may be much stronger than a generalist inside its niche.

### 2. A weekly prayer should feel powerful

The prayer item, its quality-production burden, and the once-per-week sermon slot are materially more expensive than ordinary consumables. The payoff should therefore be perceptible and satisfying, not a tiny efficiency increase that could be replaced by one cheap potion or routine action.

For narrow prayers this explicitly permits very strong niche windows. Combat is the clearest example: if the player spends the week's sermon on a dungeon-focused prayer that may only be used once or twice in a playthrough, the result should feel like a real combat blessing rather than a minor stat coupon.

### 3. Generalist breadth must cost specialist peak

Combo may remain good at both Faith and donations, but an equal-quality specialist should be the best ordinary sermon for its target resource. Prefer strengthening specialists over nerfing Combo.

### 4. Healthy Bronze behavior is a floor

Do not make a currently healthy Bronze prayer worse merely to manufacture a quality ladder. Stock Bronze Imagination `+0.7` is the current reference case.

### 5. Silver and Gold must visibly justify their production cost

Silver and especially Gold prayer items require materially more difficult writing inputs and resources. A player comparing tiers in the PrayerClarity UI should have an obvious reason to want the upgrade.

Quality value may come from:

- stronger magnitude;
- better reliability;
- a useful longer window;
- a tier-scaled physical/output reward;
- another thematic secondary benefit.

A duration-only ladder is acceptable **only when the extra duration materially increases useful expected activity/value**. If the relevant activity is normally completed in a short burst, duration alone does not justify premium tiers.

### 6. Natural obsolescence is allowed

Prosperity and Repose can be progression tools. They do not need permanent endgame dominance if they are strong during a meaningful window.

### 7. Balance against real substitutes

Evaluate prayers against books/perks, fertilizer/zombie farms, potions, crates/tavern, corpse progression, BSS resource loops and other systems the player actually substitutes for the prayer.

### 8. Prefer one coherent fantasy per prayer

If two prayers split one small fantasy into two weak weekly choices, merge/repackage before inflating numbers. Combat remains the clearest candidate.

### 9. Preserve simple player-facing arithmetic where possible

For Faith/Donations/Combo, percentage modifiers communicate the prayer's identity more cleanly than mixing flat and proportional bonuses. Removing generic flat side rewards and secondary off-theme modifiers is now a serious design option, provided representative payout modeling confirms the resulting progression.

### 10. Stronger ceiling may justify a stronger church gate

If Rebalanced substantially raises the output ceiling of a specialist prayer, its guaranteed-success Church Quality requirement may also rise. This is a progression gate, not a punishment. Do not balance to a theoretical candle-stacked extreme; choose thresholds that fit normal church/cathedral progression and remain realistically reachable.

### 11. Presentation and mechanics remain one semantic model

Any effective Rebalanced values/rewards/requirements must feed the same PrayerClarity semantic model used by the pulpit, Technology tooltip and active-effect surfaces. Do not create a second set of UI-only balance constants.

## Runtime probe result A — Repentance / confession roll

Probe `PrayerClarity Repentance Research Probe 0.1.0` directly captured `FlowCanvas/church_budka_roll`.

Verified graph behavior:

- the script iterates exactly two confessional object IDs (`church_budka_1` and `church_budka_2`);
- for each existing confessional it removes the prior `confession_available` interaction;
- it generates an independent random float in `[0,1]`;
- it compares that random value against player parameter `confession_probability`;
- the successful branch adds `confession_available` to that confessional.

This closes the *per-roll* mechanism: confession availability is an independent Bernoulli roll per confessional using the player parameter.

Still open:

- what exact game/graph lifecycle invokes `church_budka_roll` and therefore the true cadence;
- the exact reward path for `confession_available` (Confessional I/II differences);
- the narrowest production seam for a Rebalanced Repentance modifier.

Therefore `30/50/70%` remains only a benchmark. A follow-up lifecycle/reward graph scan is justified before choosing final values.

## Runtime probe result B — Imagination and Excellence scope

Probe `PrayerClarity Quality Prayer Scope Probe 0.1.0` establishes that these are not interchangeable generic quality buffs.

### Imagination — `buff_pen`, `craft_q=+0.7`

The linked scope is the writing/prayer-production chain, including:

- Stories;
- Notes;
- Chapters;
- Soul-workbench writing equivalents;
- many/all player-facing prayer crafting recipes that consume Chapter/Book inputs.

This makes Imagination a broad planned **writing/prayer-production window**. The stock Bronze `+0.7` has a real, coherent role and should not be nerfed.

### Excellence — `buff_star`, `craft_q=+0.2`

The linked scope is narrower and materially different. Verified examples include:

- hard-book crafting;
- high-tier chisels;
- carved wood;
- marble quality work.

This supports stronger magnitude progression for Excellence: it is a late specialist for a smaller set of premium multi-quality crafts, not a duplicate of Imagination.

### Consequence for Imagination quality progression

The user's alternative progression is now preferred for design exploration:

- keep `craft_q=+0.7` at Bronze/Silver/Gold;
- let duration remain 18/36/54 min;
- give Silver/Gold a meaningful **thematic success reward** from the writing economy instead of lowering Bronze or blindly inflating `craft_q`.

Stories are a particularly clean candidate because they support the writing loop without bypassing the Chapter/cover/Book production chain. Example counts remain unaccepted; a finished premium Book is a less attractive reward because it skips too much of the very quality-production progression the prayer is meant to support.

## Revised prayer-by-prayer direction

| Prayer | Current direction |
| --- | --- |
| Ordinary | Keep stock. Starter baseline. |
| Faith | Real Faith specialist. Percentage success bonus is the primary axis; `k_faith=1/2/3` (+100/+200/+300%) remains the first model. Reassess/remove flat and off-theme side rewards for clarity. Consider stronger Silver/Gold q if output ceiling warrants it. |
| Donations | Real money specialist. `k_money=1/2/3` remains the first model. Reassess/remove flat and off-theme side rewards. Natural later obsolescence is acceptable. |
| Combo | Keep the familiar +50/+100/+150% both as generalist benchmark initially; generic flat rewards may be removed together with specialist cleanup if modeling supports it. |
| Shoots & Roots | Rebalanced includes the proven scope repair and -20/-30/-40% growth-time curve. |
| Repentance | Rework required; final throughput cannot be chosen until invocation cadence/reward path is closed. |
| Repose | Quality as reliability: Bronze stock-style roll, Silver midpoint to certainty, Gold guaranteed best prayer-eligible tier, still bounded by progression. |
| Retribution + Protection | Strong structural case for one Combat Prayer package with legacy alias. Final merge decision still requires explicit confirmation; if merged, effect should feel dramatically stronger than consumable-level convenience. |
| Imagination | Preserve stock `+0.7` floor/all tiers for now; make Silver/Gold worth crafting via useful duration plus thematic writing rewards rather than a Bronze nerf. |
| Excellence | `+0.2/+0.5/+1.0` remains a strong candidate after scope verification. |
| Prosperity | Keep stock: existing 1/2/3 Blessings already provide meaningful quality progression. |
| BSS Soul's Repose | Keep stock initially: quality already scales Faith modifier and current state can make it very strong. |
| Soul Contentment | Do not automatically buff. Re-audit whether 36/72/108 min actually creates enough extra soul-processing value to justify Silver/Gold. |
| Thorough Cleansing | Keep x2 magnitude initially because it is already a successful specialist; separately verify whether 36/72/108 duration is enough quality value before declaring its tier ladder healthy. |

## Combat design target after user review

The former `1 HP / 3, 2, 1.5 sec` candidate is now considered too conservative as a leading target.

Desired loop:

`fight room -> take meaningful burst damage -> clear room -> short corridor -> noticeably recover before next engagement`.

The player must still dodge and can still be punished by individual hits, but the blessing should strongly reduce potion dependence and create an obvious feeling of supernatural combat readiness.

A new first modeling point is therefore substantially stronger regeneration, for example roughly **1 / 2 / 3 HP per second** across Bronze/Silver/Gold, while keeping armor conservative because armor is flat subtraction. Damage should also scale materially by quality. These numbers are hypotheses, not accepted values; the key accepted rule is the intended feel and opportunity-cost target.

Visual polish such as a holy combat aura or weapon glow is thematically desirable if a narrow native visual seam can be reused cheaply. It is not yet an implementation requirement and must not justify per-frame scans/polling or brittle asset manipulation.

## Faith / Donations terminology

`k_faith` and `k_money` are the **percentage success modifiers**, not the flat rewards:

- `k = 0.5` -> +50% of the relevant base;
- `k = 1` -> +100%;
- `k = 2` -> +200%;
- `k = 3` -> +300%.

Stock prayers also carry separate small fixed outputs. Rebalanced should model whether those fixed outputs and off-theme side percentages can be removed so the UI becomes a clean specialist/generalist comparison instead of a sum of flat + proportional + side-resource bonuses.

## Remaining design decisions / data gates

### Repentance lifecycle/reward gate

Need a follow-up read-only scan for callers/references of `church_budka_roll`, `confession_available` and `confession_probability`, plus the confession reward graph/data. This is now the only mechanics-design gate likely to change Repentance's entire role.

### Faith / Donations economic cleanup

Model the percent-only specialist/generalist family at representative CQ/GQ states and plausible q ladders. Decide whether to remove flat and off-theme success bonuses and choose the actual q B/S/G thresholds.

### Imagination premium rewards

Choose the reward type/count for Silver/Gold. Current preferred direction is multi-quality Stories rather than a finished Book. Exact quantities remain open.

### Combat merge confirmation

The user has confirmed the desired power level/feel and one-profile approach but has not yet explicitly confirmed whether Retribution + Protection should become one effective Combat Prayer. Current design recommendation remains **merge**, with existing `b_shield` items as same-quality legacy aliases and no save-ID rewrite.

### Implementation-only gates after roster lock

- Roots: SmartExpression replacement/reinitialization lifecycle.
- Repose: exact corpse-tier RNG seam.
- Combat: exact damage/regen/visual lifecycle seams.

These should not block design-number work that can be resolved first.

## Recommended staging

1. Close Repentance lifecycle/reward evidence with one narrow follow-up probe.
2. Model the percent-only Faith/Donations/Combo family and church-quality ladders.
3. Choose Imagination Silver/Gold thematic rewards and confirm the Combat merge.
4. Re-audit BSS duration-only tier value under the explicit Silver/Gold rule.
5. Lock the integrated Rebalanced roster in docs.
6. Only then close implementation seams and build one integrated `dev/*` runtime candidate.
