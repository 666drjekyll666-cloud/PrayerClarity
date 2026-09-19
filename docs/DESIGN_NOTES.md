# PrayerClarity — Design Notes

Status: product/architecture source of truth, reconciled 2026-09-19 with the accepted stable sibling releases PrayerClarity: Vanilla 1.0.25 and PrayerClarity: Rebalanced 0.2.4.

Detailed evidence and history live in specialized documents rather than being duplicated here:

- `PRAYER_MECHANICS.md` — stock Graveyard Keeper 1.407 mechanics;
- `PLAYER_UX_RESEARCH.md` — player/presentation evidence;
- `PRAYER_DESIGN_AUDIT.md` — current prayer-by-prayer role/design verdicts;
- `PRAYER_POWER_BUDGET.md` — quantitative stock unlock/craft/quality/opportunity-cost analysis;
- `PRAYER_REBALANCE_OPTIONS.md` — locked Rebalanced design specification;
- `PULPIT_REVEAL_UX.md` — accepted pre-sermon reward-reveal boundary;
- `TECHNOLOGY_TOOLTIP_UX.md` and current candidate/runtime docs — detailed Technology UX evolution;
- `CLARITY_UI_STATUS.md` and `RUNTIME_1.0.20_RESULT.md` — accepted Clarity/runtime evidence.

Do not use historical candidate scope files or older balance examples as the current Rebalanced roster when they conflict with `PRAYER_REBALANCE_OPTIONS.md`.

## Product family

Accepted public naming:

1. **PrayerClarity: Vanilla** — complete Clarity presentation over stock Graveyard Keeper 1.407 prayer/sermon mechanics and balance.
2. **PrayerClarity: Rebalanced** — sibling edition containing the same Clarity experience plus the intentional locked Rebalanced ruleset.

They are peer alternatives in one PrayerClarity family, not base mod + upgrade/add-on. A player should install one edition or the other.

`PrayerClarity` remains the repository/codebase family name. Public edition subtitles do not force repository, namespace, plugin GUID or DLL naming decisions.

Internal evidence/design layers remain distinct:

- **Clarity** — information/presentation only;
- **Vanilla Fixes** — evidence-backed repair where intended stock mechanic/magnitude is recoverable;
- **Balance / Rework** — deliberate new design/tuning.

**Vanilla Fixes is not an accepted third public edition.** PrayerClarity: Vanilla explicitly promises stock gameplay behavior, so fixes must not be silently folded into it. PrayerClarity: Rebalanced may incorporate a proven Vanilla Fix where the locked Rebalanced design requires the repaired mechanic, while documentation must still distinguish repair from invented tuning.

## Accepted baseline

Current accepted stable runtime identities are edition-specific:

- **PrayerClarity: Vanilla 1.0.25** — accepted ref `accepted/vanilla-1.0.25`, exact source `ebe069b4ad202ae786af9c63ded0ffb00502cff7`, release `v1.0.25`, DLL SHA-256 `72b2237607734d8b50d666cef56e18d458b88c6a456211d496e2410ac646e3a9`.
- **PrayerClarity: Rebalanced 0.2.4** — accepted ref `accepted/rebalanced-0.2.4`, exact runtime source `4d5d3021c0b9eef16d09402c5f25851ff7a66981`, release `rebalanced-v0.2.4`, DLL SHA-256 `ecba7297a80bde91a044d4d7c7e4348fd32805e960e3fa467be8d1465cfa52e2`.

Both stable releases were published from their exact accepted CI artifacts without rebuilding. Later `main` documentation/repository-hygiene commits do not redefine those frozen runtime identities.

## Permanent gameplay/product policies

- Preserve full **base donations on failed sermons**. This is an explicit PrayerClarity product decision. Prayer-specific bonuses/special success outputs may still be lost according to verified mechanics.
- Keep stock 1.407 behavior documented independently of Rebalanced behavior.
- Do not present intentional Rebalanced values as restored developer intent.
- Use temptation parity: unlock/craft/quality/success/weekly opportunity cost all matter.
- Bronze must already be credible. Silver/Gold should buy meaningful magnitude, reliability, duration, output or certainty.
- Do not rebalance for visual/numerical symmetry.
- Prefer strengthening alternatives over nerfing familiar healthy rewards unless evidence justifies a nerf.
- Natural obsolescence is acceptable for progression prayers that have completed their role.

## Shared semantic architecture

The core architectural requirement for both editions is **one effective prayer semantic model**.

The model must be side-effect free for presentation and must describe, where relevant:

- prayer/craft/event identity;
- quality tier;
- success requirement and probability inputs;
- Faith and donation percentage modifiers;
- fixed prayer-owned success outputs;
- special effect magnitude/probability/reliability;
- duration;
- discrete item rewards;
- relevant dependencies such as Church Quality, Graveyard Quality or Soul Gratitude.

The four accepted player-facing surfaces consume context-appropriate projections of that same model:

1. Pulpit;
2. Technology tooltip;
3. prayer item tooltip;
4. Character -> Temporary Effects.

PrayerClarity: Vanilla resolves effective semantics to stock 1.407 values. PrayerClarity: Rebalanced resolves them to the locked ruleset. Do not maintain a second UI-only table of Rebalanced numbers beside separate gameplay patches.

The mechanics layer may require narrow hooks for behavior that is not representable merely by changing stock data. Those hooks must still consult the same effective definitions/quality semantics so gameplay and UI cannot drift.

## Shared prayer information-design contract

This presentation contract applies to **both PrayerClarity: Vanilla and PrayerClarity: Rebalanced**. Edition-specific mechanics and values differ; the information grammar should not fork unless evidence requires it.

Primary rule:

> Describe the in-world effect once in natural player-facing language; show only the quality-dependent delta inside each Bronze/Silver/Gold block.

Practical rules:

- write from the player's/game-world perspective first, then expose the exact number that controls the mechanic;
- prefer concrete game actions and objects (`the donkey brings a corpse`, `a confession can occur`, `plants take less time to grow`) over implementation language (`roll`, `scheduler`, `range narrowing`) unless the technical term is itself player-facing;
- shared/invariant behavior appears once above the quality tiers;
- each tier contains only materially changing information plus its 100%-success Church Quality requirement;
- short label/value mechanics are atomic clauses and should not wrap between the label and its value (`Craft quality: +0.5`, `Confession chance: 75%`, `Plant growth time: -30%`);
- preserve semantic hierarchy with spacing before adding more prose: shared effect -> tier snapshots -> crafting source;
- use native icons where they reduce explanation cost, but do not let an icon become the sole carrier of essential information;
- use color as a secondary cue, never as the only source of meaning;
- special named entities keep the same accent everywhere they recur in one tooltip (for example `Commercial Blessing` in the shared explanation and tier rewards);
- Bronze/Silver/Gold color may emphasize a **key tier value** when that value itself is a direct quality ladder; color only the value, not the entire sentence;
- do not color durations merely because they vary; duration is normally secondary and should remain a compact readable line;
- avoid decorative color proliferation: structural/entity accents and quality-linked value accents must have a defined semantic role;
- prefer exact but natural wording over technically exhaustive prose. Do not add fallback/else-case explanations when they do not materially improve the player's decision;
- when a mechanic cannot be truthfully expressed as one fixed probability, do not invent a percentage merely to make the tier ladder visually symmetric.

The intended scan path is:

`what happens -> what this quality changes -> how long / what extra reward -> where it is crafted`

This contract governs presentation structure, not mechanics. PrayerClarity: Vanilla must continue to describe stock 1.407 behavior honestly; PrayerClarity: Rebalanced may describe repaired/reworked behavior from its effective ruleset.

## Pulpit presentation contract

The accepted pulpit model is:

`base/guaranteed dependency -> success-only prayer contribution -> special effect`

Before the sermon, explain what drives the reward and what the selected prayer changes. Preserve the sermon animation as the reveal moment for the exact final Faith/donation totals.

Default pulpit presentation therefore keeps:

- current relevant context such as Church Quality and Graveyard Quality;
- exact sermon success probability;
- dependency relationships for base Faith/donations and verified special inputs such as Soul Gratitude;
- exact prayer-owned percentage/fixed modifiers;
- exact intrinsic mechanics such as duration, growth reduction, combat stats, regeneration, confession chance, resource multiplier or reward quantity.

It does **not** expose the fully resolved current final Faith/donation payout merely because the semantic model can calculate it.

See `PULPIT_REVEAL_UX.md` for the detailed rationale.

## Technology tooltip contract

Technology is the comparison/planning surface.

Accepted general grammar is:

`shared prayer information -> compact Bronze/Silver/Gold tier snapshots`

- shared/invariant behavior appears once;
- each tier shows the native quality marker, Church Quality required for 100% success, and only values that materially vary by prayer quality;
- percentage and flat contributions remain semantically distinguishable;
- long special effects use content-driven wrapping instead of forcing every tooltip to the width of the longest string;
- Effect duration is semantically separated from Faith/Donations bonus information;
- the `Crafted at` footer remains visually separated from the last tier;
- vanilla lore comes through the verified base `b_*_d` localization seam rather than punctuation-sensitive parsing.

Rebalanced effects should fit this data-driven structure. Do not add bespoke layout code per prayer unless runtime evidence proves the generic semantic structure insufficient.

## Prayer item tooltip contract

A prayer item tooltip describes the **concrete quality currently held**, not the whole Bronze/Silver/Gold comparison.

It should continue to use the same effective semantics as Technology and Pulpit so a Rebalanced item never reports stock mechanics.

## Temporary Effects contract

Active prayer effects show their concrete quantitative meaning and remaining duration. Long remaining durations are expressed in in-game days using the game's effective day length; the precise stock timer returns inside the final day.

Rebalanced timed buffs must feed their actual effective magnitude through this same surface. Do not infer active values from localized strings.

## Localization

All PrayerClarity-owned player-facing strings must support the same 11 interface languages as the accepted Vanilla edition:

`en`, `fr`, `de`, `zh_cn`, `es`, `pt_br`, `ko`, `ja`, `ru`, `it`, `pl`.

Rules:

- follow the game's active language;
- reuse vanilla terminology/localization where practical;
- keep dynamic values separate from translatable prose;
- English is the safe fallback;
- no polling for language changes;
- new Rebalanced player-facing text is not localization-complete if it exists only in English/Russian.

## Runtime/performance architecture

- Keep UI work at existing UI lifecycle/redraw seams.
- Prefer native getters/state over mirrored runtime state.
- No broad Unity scans or background polling in production.
- Cache unavoidable reflection/compatibility bindings.
- Mechanics changes should hook the narrow semantic event that owns the behavior rather than broad `Update()` loops.
- Diagnostic probes remain narrow, removable research artifacts.

## Rebalanced roster status

The design roster is **locked**. `PRAYER_REBALANCE_OPTIONS.md` is the only canonical concrete table of current Rebalanced values.

The old intermediate candidates formerly recorded in this file — including earlier Faith/Donations coefficients, earlier Repentance probabilities, earlier Combat regeneration/damage, earlier Imagination curves and speculative BSS magnitude ladders — are superseded and must not be implemented.

No new balance round is required unless implementation evidence contradicts an assumption that materially affects the locked behavior.

## Current engineering state

The current stable implementation line is Rebalanced 0.2.4. It inherits the accepted 0.2.3 Repentance/Combat native seams and the 0.2.2 Roots safety repair; 0.2.4 changes only Repentance/Repose duration through the existing load-time `CraftDefinition.dur_parameter` projection. `POST_AUDIT_VERDICT.md` remains the architecture/save-lifecycle closure at **A — no architecture action**.

There is currently **no open production implementation gate** recorded by this document. New runtime work should begin only from a concrete mechanics, compatibility, UX, or player-feedback problem. Historical probe/source artifacts are not a reason by themselves to create another numbered candidate.
