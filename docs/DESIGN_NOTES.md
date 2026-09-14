# PrayerClarity — Design Notes

Status: product/design hypothesis stage, 2026-09-14. Mechanics discovery is sufficiently closed; production implementation is not accepted yet.

## Problem statement

Stock Graveyard Keeper 1.407 answers **“will this sermon succeed?”** reasonably well, but poorly answers **“what will this prayer actually do for me now?”**.

The player can see church quality, sermon requirement, and success chance. They generally cannot see current Faith/donation outcomes, many passive prayers omit magnitude/duration, quality can change duration without changing strength, and two stock prayers contain special-effect anomalies that vanilla does not disclose.

Research has also established that the product question is broader than presentation alone: some stock prayer effects are disconnected/broken, while community discussions repeatedly question the usefulness and opportunity cost of several otherwise-working prayers. The next decision is therefore the scope of the product, not yet the exact layout of one tooltip.

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

- Normal technology-tree unlock presentation ultimately uses `TechUnlock.GetTooltip`, which builds craft/sermon information dynamically.
- Item/prayer description uses `ItemDefinition.GetItemDescription`, starting from the localized `<item_id>_d` string and adding sermon-specific information.
- `TechUnlockDialogGUI.OpenAsItemsList` can reuse `ItemDefinition.GetItemDescription` for actual item entries, so some presentation is shared, but this is not the same as saying every technology tooltip and inventory tooltip are one surface.
- The pulpit selection (`PrayCraftGUI`) is a third decision surface with live church quality / requirement / success chance.
- After success, timed prayer effects enter the ordinary `BuffsGUI` / `PlayerBuff` system used by other temporary buffs. The active-buff hover text itself has not yet been audited closely enough to assign it an information role.

**Design consequence:** build one internal prayer-information model, then render the appropriate subset through adapters for each surface. Do not independently hard-code divergent descriptions in several UI patches.

Proposed information roles:

- **Technology tree:** what capability the prayer unlocks, its broad niche, and how prayer quality changes it.
- **Prayer item:** exact static properties of this quality tier (effect, duration, requirement, reward modifiers where useful).
- **Pulpit selection:** dynamic current-state answer — success/failure outcomes, Faith, donations, special output/effect.
- **Active buff:** current active effect + remaining duration; avoid duplicating information vanilla already communicates well.

## Information target

For the selected prayer the player should be able to obtain, without formula algebra:

1. expected successful Faith/donation result now;
2. failure result when chance <100%;
3. special output/effect;
4. duration;
5. what changes with prayer quality;
6. only the current dependencies needed to understand the displayed result;
7. a truthful indication when a stock special effect is disconnected/inert rather than pretending the intended effect works.

## Product scope — three semantic layers

The strongest current architecture is **one project / one mod package with independently controlled semantic layers**, rather than three unrelated implementations.

### Layer 1 — Clarity

Informational only. It must be able to describe stock 1.407 without changing outcomes.

Examples:

- unambiguous Faith/donation wording;
- exact passive magnitude and duration;
- current-state pulpit forecast;
- success vs failure result;
- church/graveyard/Soul Gratitude dependency hints;
- quality comparison.

This layer is the stable baseline and should remain usable even if every gameplay-changing option is disabled.

### Layer 2 — Vanilla fixes

Narrow behavior changes only where evidence supports a broken/disconnected stock implementation and a recoverable vanilla intent.

Strong candidate:

- **Shoots and Roots:** stock data already contains the intended-looking `-20%` growth-time term and Lazy Bear publicly stated that the prayer should reduce garden-crop growth time and affect zombie beds. The current failure is a proven parameter-owner mismatch. A repair can therefore preserve the existing 20% magnitude instead of inventing a new number.

Not yet equivalent:

- **Repentance:** intended semantic target is increased confessional use, but no stock consumer or intended magnitude has been recovered. Restoring this requires one more focused design/mechanics investigation of the confessional roll path / dormant data. Choosing a new probability without that evidence is a rebalance decision, not merely a bug fix.

Any fix must be documented separately from stock mechanics so PrayerClarity never rewrites history about what vanilla 1.407 actually does.

### Layer 3 — Balance tuning / rework

Subjective changes to mechanics that already function but may be dominated, overly narrow, progression-obsolete, or insufficiently rewarding relative to using the weekly sermon slot for Combo/Faith.

This layer should not be conflated with bug fixing. Initial versions should keep it independently switchable; exact defaults depend on the final product identity.

If the public product remains named **PrayerClarity**, gameplay-changing balance tuning should default off. If the project is deliberately repositioned as a **Prayer Overhaul**, a fixed/balanced profile may become the intended default, but a vanilla-clarity profile should still exist for verification and user choice.

A practical profile model is:

- `Vanilla + Clarity` — no mechanics changes;
- `Fixed Vanilla` — clarity + evidence-backed repairs;
- `Rebalanced` — clarity + repairs + accepted tuning.

This can be implemented in one DLL with clean feature modules/configuration. Split into a companion rebalance mod only if the tuning layer becomes large enough to create compatibility, maintenance, or user-expectation problems.

## Community-demand interpretation

Current external research supports three different strengths of demand:

### Clarity demand — strong

Questions about what prayers do, how long buffs last, whether quality changes strength or duration, whether graveyard/church quality matters, and which prayer is actually better recur across years and are still present in 2026.

This aligns directly with the verified UI audit and is not dependent on subjective balance preferences.

### Broken-prayer repair demand — strong

Repentance and Shoots/Roots have been repeatedly discussed as broken/no-apparent-effect for years. Current 1.407 runtime inspection independently explains both anomalies rather than relying on old wiki claims.

Repairing a clearly recoverable broken path is therefore well-supported product work, provided the fix does not invent missing mechanics under the label of “vanilla fix”.

### Broad rebalance demand — moderate and fragmented

There is a repeated **choice-compression** signal: Combo is often treated as the universal default, and some players describe the remaining prayers as useless, unreliable, or too situational. A 2025 discussion has multiple users defaulting to Combo while explicitly lamenting that some alternatives have little use.

But the same discussions also identify meaningful niches:

- Prosperity is valuable for vendor progression;
- Imagination can be extremely powerful when batching writing work;
- Excellence has a narrow crafting niche;
- Repose is valuable before the final corpse tier and intentionally/structurally loses value afterwards;
- Better Save Soul prayers can become excellent when the player is actively using their associated DLC systems.

Therefore there is **no evidence of a community consensus on exact replacement numbers or a complete rebalance recipe**. The supported finding is narrower: the weekly-sermon opportunity cost makes weak/narrow prayers hard to justify and causes Combo to dominate default play.

The balance target should therefore be **meaningful reasons to choose a prayer at the stage where its niche is relevant**, not “make every prayer equally powerful forever”.

## Balance-pass decision framework

Before changing a working prayer, classify it as one of:

- **broken/disconnected** — intended path exists but does not function;
- **misleading/opaque** — works, but UI prevents rational evaluation;
- **healthy niche** — situational by design and worthwhile in that situation;
- **progression tool** — useful for a phase, then reasonably obsolete;
- **dominated/redundant** — another reachable prayer provides the same role with no meaningful tradeoff;
- **underpowered opportunity cost** — effect works but rarely justifies consuming the one weekly sermon opportunity.

Only the last two categories are presumptive rebalance candidates. Niche or progression-limited behavior is not automatically a defect.

A prayer-by-prayer balance matrix should evaluate role, unlock timing, church-quality requirement, effect window, weekly opportunity cost, overlap with Combo/Faith, and community/runtime evidence before any number is changed.

## Additional vanilla anomaly to keep separate

The sermon payout audit found another implementation mismatch: on failure, the graph supplies a `0.5` visitor-selection chance to donations, but `SpreadMoneyIncome` calls integer `Random.Range(0,1)`, which always returns zero. Consequently base donations are still distributed to all visitors on failure.

This looks like a bug candidate, but fixing it would **reduce** player rewards and developer intent is inferred from graph shape rather than explicitly documented. It should not be silently included in a generic “fixes” switch without a separate product decision.

## Option A — rewrite descriptions only

Useful for replacing ambiguous `xN` wording and adding lines such as `+5 damage for 36 min`, but insufficient for current-state Faith/donation comparison and Souls calculations.

**Assessment:** useful supplementary cleanup, not the main solution.

## Option B — compact dynamic breakdown in existing prayer selection

Preferred UI hypothesis: preserve vanilla success information and add a small selected-prayer block.

Illustrative hierarchy only:

- `Faith on success: 18`
  - `Base 11 · Prayer +7`
- `Donations on success: 42s`
  - short dependency hint where useful
- `On failure: 11 Faith · 28s`
- `Effect: +5 damage`
- `Duration: 36 min`

For quality comparison where relevant:

- `Same +5 damage · duration 36 -> 72 min`

For Prayer for Repose:

- `Effect: Donkey maximum corpse tier +1`
- `Duration: 18 / 36 / 54 min by prayer quality`

For Soul Contentment:

- `Effect: +10% Soul Gratitude`
- exact forecast may use `RoundToInt(GP_base * 1.1)` if a next-soul preview is ever shown.

### Advantages

- answers the decision question directly;
- supports current-state values without exposing coefficient algebra;
- can reveal church/graveyard/Soul Gratitude dependencies only when relevant;
- can distinguish success and failure outcomes;
- can make `same strength, longer duration` explicit.

### Risks

- must fit existing UI without crowding;
- must not present probabilistic success outcome as guaranteed;
- preview calculation must be side-effect free;
- fixed/rebalanced behavior must be reflected according to the active configuration rather than describing vanilla values after mechanics have changed.

**Assessment:** strongest UI hypothesis, but product-scope/balance decisions now precede the first production prototype.

## Option C — separate detail panel

Gives more space but adds lifecycle/UI complexity and risks becoming an in-game wiki.

**Assessment:** fallback only if option B cannot remain readable.

## Side-effect-free calculation architecture

Do **not** call `PrayLogics.CalculatePray` to render a preview: it performs random success/result bookkeeping and manages sermon result/drop state.

Preferred production direction:

- read selected `CraftDefinition`;
- evaluate linked `PrayEventDefinition` expressions through stock expression evaluation;
- reproduce only verified deterministic rounding/bonus composition;
- read existing church/graveyard/Soul Gratitude/perk state through normal getters;
- compute on prayer-selection open/change, not per frame;
- no broad reflection scans in production;
- cache stable mappings only if needed;
- route preview through the same mod-owned semantic model used by tech/item descriptions so UI surfaces cannot disagree;
- if fixes/rebalance are enabled, the model must describe the **effective configured mechanics**, while research docs retain the stock baseline separately.

Exact Harmony/lifecycle targets remain an implementation-stage inspection task and must not be guessed.

## Broken/disconnected prayer policy

### Prayer of Repentance

Verified stock facts:

- prayer attaches timed `buff_sins`;
- no gameplay consumer was found in code literals, 180 loaded FlowCanvas graphs, or balance references beyond its definition/crafts;
- community/wiki reports independently describe no apparent effect.

Do not invent an intended percentage and call it a vanilla repair. Before implementation, inspect the live confessional roll path and any dormant/current data for a recoverable multiplier/additive value. If none exists, any functional Repentance design belongs to the balance layer and needs an explicit new rule.

### Prayer for Shoots and Roots

Verified stock wiring:

- growth definitions contain `-0.2*WGOpar("buff_plant")`;
- prayer application writes `buff_plant=1` to player data;
- `WGOpar` reads the bound growing/workbench WGO;
- CraftComponent evaluates growth craft time with that WGO as the expression context;
- no propagation path from player param to growing WGO was found.

A fix can plausibly bridge the existing prayer state into the existing growth formula without changing the 20% coefficient. This is the cleanest evidence-backed repair candidate.

## Repose is fully player-translatable

Probe 0.1.5 closes the live Donkey path:

- `Tier min = body_min + add_body_min`;
- `Tier max = body_max + add_body_max`;
- prayer buff contributes `body_max=1`.

Player-facing semantic meaning is:

**Donkey maximum corpse tier +1 while active.**

Prayer quality changes only duration: 18/36/54 minutes. The fact that the effect becomes useless once the player already has access to the final corpse tier is a progression-obsolescence question, not evidence that the mechanic itself is broken.

## Remaining presentation audit

Before finalizing description hierarchy, inspect the active `BuffsGUI` hover/tooltip path for prayer buffs. We already know prayer effects share the standard temporary-buff panel and timer; the missing question is how much mechanical description vanilla exposes when the active icon is hovered/focused. This can prevent redundant PrayerClarity UI.

## Next design gate

Do not start the comprehensive production implementation yet.

Next research/design work should produce a **prayer-by-prayer role/balance matrix** and answer two narrow questions:

1. Repentance: can a credible vanilla-intent magnitude/mechanism be recovered from the confessional implementation, or must a working version be explicitly designed as new balance?
2. Active buff tooltip: what information is already present after use?

Then choose the product profile/default policy and accept individual balance changes before building the first integrated runtime prototype.
