# PrayerClarity Project Contract

PrayerClarity inherits the global development contract from `666drjekyll666-cloud/DevRules`.

Before substantive work, read:

- `DevRules/ENGINEERING_RULES.md`
- `DevRules/CI_POLICY.md`
- `DevRules/GIT_WORKFLOW.md`
- `DevRules/PROJECT_BOOTSTRAP.md`
- this file
- the current project docs and relevant branch/commit/PR evidence

## Project identity

Target: Graveyard Keeper 1.407.

Purpose: establish how the prayer/sermon system actually works, make its effects understandable at the point of choice, repair narrowly proven broken/disconnected behavior where vanilla intent is recoverable, and explicitly redesign/tune prayers where the verified system still lacks a healthy choice structure.

The product may remain one BepInEx mod/codebase, but it must keep three semantic layers distinct:

1. **Clarity** — information only; no mechanics changes.
2. **Vanilla Fixes** — evidence-backed repairs where the intended mechanic/magnitude is sufficiently recoverable.
3. **Balance / Rework** — intentional new design or tuning. Never present these changes as recovered vanilla mechanics.

Stock Graveyard Keeper 1.407 behavior must remain documented independently of modded behavior.

Do not assume `prayer`, `sermon`, localized names, item IDs, `PrayCraft`, or `PrayEventDefinition` are interchangeable. Record mappings only when evidence supports them.

## Research / design order

Use this sequence unless new evidence justifies a narrower detour:

`catalogue -> verify mechanics -> audit presentation -> research player experience -> identify UX gap -> design/role audit -> quantitative power-budget audit -> candidate roster/spec -> narrow prototype -> runtime test -> accept`

Do not write broad production behavior before the relevant mechanic, UI surface, design rule, candidate specification and acceptance condition are established.

## Evidence labels

Use these statuses explicitly where useful:

- **fact** — supported by accepted project data, direct assembly/resource/localization inspection, runtime measurement, or another direct source;
- **hypothesis** — plausible explanation not yet proved;
- **community signal** — a player report/question useful for UX/design research but not proof of mechanics;
- **UX finding** — supported mismatch between information the player needs and information the game supplies;
- **design hypothesis** — possible presentation/fix/rebalance rule not yet accepted;
- **accepted result** — runtime-sensitive behavior or design accepted after the required user test.

Evidence priority for mechanics:

1. accepted project data;
2. direct assemblies/resources/localization;
3. targeted calling-code inspection;
4. narrow runtime probe;
5. user in-game test when only the installed game can resolve uncertainty.

Wiki/guides/community sources are useful for terminology, discovery, cross-checking and player-experience/design research. They are not a substitute for direct mechanics evidence when game data/code can answer the question.

## Prayer mechanics research

For each player-relevant prayer establish where possible:

- exact player-facing name and internal item/craft/event IDs;
- use conditions and success/failure rules;
- input parameters/formulas;
- fixed vs random components;
- prayer-quality and church/graveyard/other dependencies;
- rounding, thresholds, caps and probabilities;
- Faith, money, item drops and temporary effects;
- relevant getters/methods/data definitions;
- text shown before and after use;
- discrepancies between presentation and actual behavior.

Do not guess IDs, localization keys, formulas, Harmony targets, lifecycle or supposed developer intent.

## Stock-vs-mod boundary

For every gameplay change record separately:

- what stock 1.407 actually does;
- what evidence indicates broken/disconnected behavior vs weak/niche behavior;
- what the mod changes;
- whether the change is Vanilla Fix or Balance/Rework;
- what runtime/user evidence is required before acceptance.

A broken mechanic may be a Vanilla Fix only when intended behavior is sufficiently recoverable. If role is known but magnitude/algorithm must be invented, the change is Balance/Rework.

### Permanent project policy: failed-sermon donations

Stock 1.407 gives the full base donation pool even on sermon failure because the apparent 50% visitor-participation path is defeated by integer `Random.Range(0,1)` behavior.

**Do not change this.** Preserve full base donations on failed sermons in all profiles. Prayer-specific bonuses/special success outputs may still be lost according to verified mechanics. This is an explicit product decision, not a claim about original developer intent.

## UX research

Keep separate:

1. What does the game actually do?
2. What do players think it does?
3. What can a player infer from the UI without external sources?

Do not call a handful of comments a consensus. Preserve date/version context. State UX gaps concretely. Do not assume the answer is a long tooltip.

## Balance / role research

### Core principle: temptation parity

A prayer is a strategic investment, not just a numeric buff. Its real cost includes:

- technology unlock/prerequisite investment;
- crafting materials and Faith;
- effort to obtain prayer quality;
- church-quality/success requirement;
- the opportunity cost of spending the weekly sermon on it instead of another prayer.

**Design target:** every prayer should be a tempting purchase and a tempting weekly choice at the stage/niche where it belongs.

This is not numerical parity. A narrow prayer may be deliberately very strong in its niche. A progression prayer may legitimately become obsolete after completing its role. What is undesirable is paying substantial unlock/craft/week cost for a prayer that never creates a convincing moment where the player wants it.

Bronze should already be credible. Silver/gold should create meaningful extra value through magnitude, duration, outputs, success gates, or crossing future sermon weeks.

Do not rebalance for symmetry. Before changing a functioning prayer classify it as:

- broken/disconnected;
- misleading/opaque;
- healthy niche;
- progression tool;
- dominated/redundant;
- underpowered relative to its full investment/opportunity cost.

Compare unlock timing/cost, chapter vs book recipe class, quality difficulty, alternative systems/consumables/perks, church requirements, duration, stage relevance and weekly opportunity cost.

Prefer making alternatives attractive over reducing familiar player rewards. Nerfs require stronger justification than existence of a meta choice.

`docs/PRAYER_DESIGN_AUDIT.md` is the current source of truth for prayer-by-prayer design judgements.
`docs/PRAYER_POWER_BUDGET.md` is the current source of truth for quantitative unlock/craft/opportunity-cost comparisons.
`docs/PRAYER_REBALANCE_OPTIONS.md` contains candidate rebalanced rules only; nothing there is accepted until explicitly narrowed, implemented and runtime-tested as required.

## Player-facing clarity target

The intended end state is a **white-box player experience**, not a developer-facing formula viewer.

The UI should let the player answer:

- what the prayer does;
- what current values/conditions affect it;
- what outcome/effect to expect now;
- what changing prayer quality or inputs changes;
- how it differs from alternatives;
- when a selected profile repairs or intentionally changes vanilla behavior.

Prefer concrete values, short dependency explanations and visual hierarchy over raw formulas.

Use one mod-owned semantic model for all prayer presentation surfaces so technology text, item tooltip, pulpit forecast, active-buff presentation and effective configured mechanics cannot contradict each other.

## Localization / language support

PrayerClarity is intended to be universal across the languages officially supported by Graveyard Keeper, not an English/Russian-only mod.

Every PrayerClarity-owned player-facing string must ship in the same 11 interface languages as the base game:

- English (`en`);
- French (`fr`);
- German (`de`);
- Simplified Chinese (`zh-cn` / normalized `zh_cn`);
- Spanish — Spain (`es`);
- Portuguese — Brazil (`pt-br` / normalized `pt_br`);
- Korean (`ko`);
- Japanese (`ja`);
- Russian (`ru`);
- Italian (`it`);
- Polish (`pl`).

Localization requirements:

- follow the current in-game language automatically;
- use the game's current language state rather than OS locale or Steam language when the game exposes the active language;
- normalize equivalent language-code separators/casing rather than maintaining duplicate translation logic;
- keep English as the safe fallback for a missing locale or key;
- reuse vanilla localized terminology/names where practical instead of retranslating game-owned terms;
- keep dynamic numbers/formulas separate from translatable sentence templates;
- do not hard-code player-facing prose in Harmony patches or mechanics code;
- a candidate is not localization-complete if any new player-facing string exists only in English/Russian.

Language switching must not introduce per-frame polling. Resolve/reload localization at an existing language/UI lifecycle boundary or lazily when rendering relevant UI.

## Runtime and performance constraints

Production should be event-driven and cheap:

- compute only when relevant UI opens/refreshes;
- prefer existing game getters/state;
- avoid per-frame polling, broad Unity scans, repeated heavy reflection/enumeration, duplicate subscriptions and persistent verbose logging;
- fixes/rebalance should patch the narrow verified behavior rather than repeatedly scanning all objects.

A diagnostic probe must answer one narrow question and be removable.

## Repository policy

Long-lived findings belong primarily in:

- `docs/PRAYER_MECHANICS.md` — stock 1.407 mechanics;
- `docs/PLAYER_UX_RESEARCH.md` — presentation/player evidence;
- `docs/PRAYER_DESIGN_AUDIT.md` — role/balance/fix judgements;
- `docs/PRAYER_POWER_BUDGET.md` — quantitative full-cost/progression analysis;
- `docs/PRAYER_REBALANCE_OPTIONS.md` — candidate rebalanced roster options, explicitly non-accepted until narrowed/tested;
- `docs/DESIGN_NOTES.md` — product/UI architecture and accepted design direction;
- `docs/TEST_BUILD_LOG.md` — only when distributable/testable production candidates exist.

Create additional design-analysis files only when they become durable sources of truth rather than temporary scratch work.

Do not commit game DLLs, full decompiled source, proprietary assets or bulk localization/resources. Store only minimal derived facts, IDs, signatures, formulas, hashes and conclusions.

`main` is stable. Use `research/*` for evidence/design gathering and `dev/*` for runtime implementation. Unaccepted runtime behavior stays off `main`.

Do not create hosted CI for routine research/docs/bookkeeping. Use hosted CI only when a concrete executable property requires it, and preserve the clean build/handoff gate for any binary given to the user.