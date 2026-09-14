# PrayerClarity — Design Notes

Status: product/design hypothesis stage, 2026-09-14. Mechanics discovery is sufficiently closed; production implementation is not accepted yet.

Detailed prayer-by-prayer role and balance judgements now live in `docs/PRAYER_DESIGN_AUDIT.md`. Stock behavior remains canonical in `docs/PRAYER_MECHANICS.md`.

## Problem statement

Stock Graveyard Keeper 1.407 answers **“will this sermon succeed?”** reasonably well, but poorly answers **“what will this prayer actually do for me now?”**.

The player can see church quality, sermon requirement, and success chance. They generally cannot see current Faith/donation outcomes, many passive prayers omit magnitude/duration, quality can change duration without changing strength, and two stock prayers contain special-effect anomalies that vanilla does not disclose.

Research also established that the product question is broader than presentation alone: some stock prayer effects are disconnected/broken, while community discussions repeatedly question the usefulness and opportunity cost of several otherwise-working prayers. The product must therefore distinguish clarity, repair and rebalance rather than silently mixing them.

## Actual mechanics -> vanilla information -> player risk

| Area | Actual mechanics | Vanilla before use | UX status |
| --- | --- | --- | --- |
| Success | church quality / requirement -> visible chance | quality, requirement, % chance shown | already clear; do not duplicate heavily |
| Faith notation | fixed Faith output is additive; proportional bonus is separate | `xN` can look multiplicative | confirmed UX finding |
| Donations | baseline uses graveyard quality | selection foregrounds church quality; no current donation forecast | confirmed UX finding |
| Souls prayer | baseline uses church quality + Soul Gratitude | dependency mentioned, current result absent | confirmed UX finding |
| Passive magnitude | +5 damage, +4 armor, +0.7/+0.2 quality inputs, +1 max Donkey corpse tier, etc. | several descriptions are flavour-only | confirmed UX finding |
| Passive duration | common tiers 18/36/54 or 36/72/108 min; quality often changes duration only | duration omitted | confirmed UX finding |
| Failure | base Faith/money remain; prayer bonuses/special success outputs are lost | chance shown, consequence not explained | confirmed supporting issue |
| Repentance | timed `buff_sins` exists; no gameplay consumer found | flavour-only | stock mechanics anomaly + clarity risk |
| Shoots/Roots | `-20%` growth formula exists, but prayer writes `buff_plant` to player while formula reads growing WGO | farming flavour only | confirmed stock wiring mismatch + clarity risk |

## Presentation surfaces are related but not one universal tooltip

The game does not route every prayer description through one content builder.

- Technology-tree unlock presentation uses its own `TechUnlock`/technology UI path and builds sermon/craft information dynamically.
- Item/prayer description uses `ItemDefinition.GetItemDescription`, starting from localized item description data and adding sermon-specific information.
- The pulpit selection (`PrayCraftGUI`) is a separate live decision surface.
- After success, timed prayer effects enter the ordinary `BuffsGUI` / `PlayerBuff` system used by other temporary buffs.

**Design consequence:** build one internal prayer-information model, then render the appropriate subset through adapters for each surface. Do not independently hard-code divergent descriptions in several UI patches.

Proposed roles:

- **Technology tree:** what capability the prayer unlocks, broad niche, and quality progression.
- **Prayer item:** exact static properties of this quality tier.
- **Pulpit selection:** current-state success/failure output, Faith, donations, special output/effect.
- **Active buff:** rely on vanilla icon/timer/hover where adequate; exact hover content is the final presentation audit item.

## Product scope — three semantic layers

The accepted architectural direction is **one project / one mod package with independently controlled semantic layers**.

### Layer 1 — Clarity

Informational only. It must be able to describe stock 1.407 without changing outcomes.

Examples: unambiguous Faith/donation wording, passive magnitude/duration, current-state pulpit forecast, success-vs-failure result, dependency hints and quality comparison.

### Layer 2 — Vanilla Fixes

Narrow behavior changes only where evidence supports a broken/disconnected stock implementation and a sufficiently recoverable vanilla intent.

Current strongest candidate:

- **Shoots and Roots:** reconnect the existing prayer state to the existing `-20%` growth-time path. The coefficient is already stock data and Lazy Bear publicly described the intended reduced-growth-time role.

Repentance is not yet equally clean: the intended semantic target is increased confessional use, but the intended magnitude/algorithm has not been recovered. If the final confessional audit does not expose one, a working Repentance rule belongs to Balance/Rework rather than being mislabeled as a vanilla fix.

### Layer 3 — Balance / Rework

Intentional changes to functioning mechanics, or a new implementation where the game reveals the role but not the missing magnitude. These changes are subjective and require explicit design acceptance.

If the public product remains named **PrayerClarity**, balance tuning should default off. If deliberately repositioned as a broader **Prayer Overhaul**, a fixed/balanced profile may become the intended default while retaining a vanilla-clarity profile.

Potential profiles:

- `Vanilla + Clarity`
- `Fixed Vanilla`
- `Rebalanced`

One DLL is preferred unless the rebalance layer later becomes large enough to create compatibility or maintenance reasons for a companion mod.

## Permanent product decision — preserve failed-sermon base donations

The payout audit found an implementation mismatch: on failure the graph supplies `0.5` participation to `SpreadMoneyIncome`, but its integer `Random.Range(0,1)` call causes every visitor to participate. Stock 1.407 therefore distributes the full **base** donation pool on failure, while prayer-specific money bonuses disappear.

**Accepted product policy:** do not “fix” this in a way that reduces player rewards. Preserve the full base donation pool for failed sermons in every PrayerClarity/Prayer Overhaul profile. This is a deliberate compatibility/design decision and must not be described as proven developer intent.

The clarity UI should simply tell the truth: on failure the player keeps base Faith and base donations but loses prayer-specific bonuses/special success outputs.

## Balance philosophy

The community evidence does **not** support a blanket “buff every non-Combo prayer” pass.

Use these classifications before changing a working prayer:

- **broken/disconnected**
- **misleading/opaque**
- **healthy niche**
- **progression tool**
- **dominated/redundant**
- **underpowered opportunity cost**

Niche or progression-limited behavior is not automatically a defect. Repose is a useful example: +1 maximum Donkey corpse tier is meaningful before the final tier and naturally becomes obsolete afterwards. Prosperity similarly has a strong merchant-progression role and then exhausts itself. Imagination can be extremely strong when the player deliberately batches writing work.

Current prayer-by-prayer preliminary verdicts are recorded in `docs/PRAYER_DESIGN_AUDIT.md`. The only functioning stock prayers presently promoted to **rebalance candidates** rather than “keep/clarify” are Retribution and Protection; no new values have been chosen.

## Preferred UI — compact dynamic breakdown

The preferred UI hypothesis remains a compact block in the existing prayer-selection context, backed by the shared prayer-information model.

Illustrative hierarchy only:

- `Faith on success: 18`
  - `Base 11 · Prayer +7`
- `Donations on success: 42s`
- `On failure: 11 Faith · 28s`
- `Effect: +5 damage`
- `Duration: 36 min`

For quality comparison where relevant:

- `Same +5 damage · duration 36 -> 72 min`

For Repose:

- `Effect: Donkey maximum corpse tier +1`
- `Duration: 18 / 36 / 54 min by prayer quality`

For Soul Contentment:

- `Effect: +10% Soul Gratitude`
- an exact future next-soul forecast can reproduce `RoundToInt(GP_base * 1.1)`.

The panel must describe **effective configured mechanics**. If a fix or rebalance option changes an effect, tech/item/pulpit text must all derive from the same model and change with it.

## Side-effect-free calculation architecture

Do **not** call `PrayLogics.CalculatePray` to render a preview because it performs success/random/result bookkeeping and manages sermon result/drop state.

Preferred production direction:

- read selected `CraftDefinition`;
- evaluate linked `PrayEventDefinition` expressions through stock expression evaluation;
- reproduce only verified deterministic rounding/bonus composition;
- read existing church/graveyard/Soul Gratitude/perk state through normal getters;
- compute only on relevant UI open/selection/state change;
- no per-frame polling;
- no broad reflection scans in production;
- route every presentation surface through the same mod-owned semantic model.

Exact lifecycle/Harmony targets remain implementation-stage evidence work and must not be guessed.

## Broken-prayer policy

### Shoots and Roots

Stock facts:

- growth definitions contain `-0.2*WGOpar("buff_plant")`;
- prayer application writes `buff_plant=1` to player data;
- `WGOpar` reads the bound growing/workbench WGO;
- CraftComponent evaluates growth craft time with that WGO;
- no propagation path was found.

This supports an evidence-backed repair using the existing 20% coefficient rather than inventing a new one.

### Repentance

Stock facts:

- prayer attaches timed `buff_sins`;
- no gameplay consumer was found across code literals, 180 loaded FlowCanvas graphs or balance references beyond its definition/crafts;
- `church_budka_roll` separately sets `confession_probability=0.15` in stock balance data;
- community testing repeatedly fails to detect a prayer effect.

The final narrow confessional audit must determine whether any dormant/current path specifies how `buff_sins` should modify that 15%. If not, the role can be retained but the magnitude must be openly designed as new balance.

## Remaining research gate

Only two small questions still affect product/UI design:

1. **Active buff hover:** exactly what stock `BuffsGUI/BuffIcon` displays besides icon/timer, so PrayerClarity does not duplicate after-use information.
2. **Repentance/confessional roll:** exact `church_budka_roll` implementation and every current reference to `confession_probability`, to see whether a recoverable multiplier/branch survived.

A final read-only presentation probe 0.1.6 is dedicated only to those questions. No broader mechanics probe is planned.

After that result, choose the first implementation slice. Current likely order:

1. shared Clarity model + compact pulpit presentation prototype;
2. evidence-backed Shoots/Roots fix behind the Vanilla Fixes layer;
3. Repentance repair only after classifying it correctly as vanilla recovery or explicit new balance;
4. quantify and decide Retribution/Protection tuning separately.
