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

Purpose: establish how the prayer/sermon system actually works, make its effects understandable at the point of choice, repair narrowly proven broken/disconnected prayer behavior where vanilla intent is recoverable, and only then consider explicitly separated balance/rework changes where working prayers still lack a healthy role.

The product may remain one BepInEx mod/codebase, but it must keep three semantic layers distinct:

1. **Clarity** — information only; no mechanics changes.
2. **Vanilla Fixes** — evidence-backed repairs of broken/disconnected stock behavior where the intended mechanic/magnitude is sufficiently recoverable.
3. **Balance / Rework** — intentional new design or tuning. Never present these changes as recovered vanilla mechanics.

Stock Graveyard Keeper 1.407 behavior must remain documented independently of modded behavior even after fixes/rebalance exist.

Do not assume that the terms `prayer`, `sermon`, localized names, item IDs, `PrayCraft`, or `PrayEventDefinition` are interchangeable. Record mappings only when evidence supports them.

## Research / design order

Use this sequence unless new evidence justifies a narrower detour:

`catalogue -> verify mechanics -> audit presentation -> research player experience -> identify UX gap -> design/role audit -> narrow prototype -> runtime test -> accept`

Do not write broad production behavior before the relevant mechanic, UI surface, design rule, and acceptance condition are established.

## Evidence labels

Use these statuses explicitly where useful:

- **fact** — supported by accepted project data, direct assembly/resource/localization inspection, runtime measurement, or another direct source;
- **hypothesis** — plausible explanation not yet proved;
- **community signal** — a player report/question useful for UX/design research but not proof of mechanics;
- **UX finding** — a supported mismatch between information the player needs and information the game supplies;
- **design hypothesis** — a possible presentation/fix/rebalance rule not yet accepted;
- **accepted result** — runtime-sensitive behavior or design accepted after the required user test.

Evidence priority for mechanics:

1. accepted project data;
2. direct assemblies/resources/localization;
3. targeted calling-code inspection;
4. narrow runtime probe;
5. user in-game test when only the installed game can resolve the uncertainty.

Wiki/guides/community sources are useful for terminology, discovery, cross-checking and player-experience/design research. They are not a substitute for direct mechanics evidence when the game data/code can answer the question.

## Prayer mechanics research

For each player-relevant prayer, establish where possible:

- exact player-facing name and internal item/craft/event IDs;
- use conditions and success/failure rules;
- input parameters and formulas;
- fixed vs random components;
- prayer-quality and church/graveyard/other dependencies;
- rounding, thresholds, caps, and probabilities;
- Faith, money, item drops, and temporary effects;
- relevant getters/methods/data definitions;
- text shown before use and after use;
- any discrepancy between presentation and actual behavior.

Do not guess IDs, localization keys, formulas, Harmony targets, lifecycle or supposed developer intent.

## Stock-vs-mod boundary

For every gameplay change record separately:

- what stock 1.407 actually does;
- what evidence indicates the behavior is broken/disconnected or merely weak/niche;
- what the mod changes;
- whether that change is classified as Vanilla Fix or Balance/Rework;
- what runtime/user evidence is required before acceptance.

A broken mechanic may be repaired under Vanilla Fixes only when its intended behavior is sufficiently recoverable. If the role is known but the magnitude/algorithm must be invented, the change belongs to Balance/Rework.

### Permanent project policy: failed-sermon donations

Stock 1.407 currently gives the full base donation pool even on sermon failure because the apparent 50% visitor-participation path is defeated by the implementation's integer `Random.Range(0,1)` behavior.

**Do not change this in PrayerClarity/Prayer Overhaul.** Preserve full base donations on failed sermons in all profiles. Prayer-specific bonuses/special success outputs may still be lost according to the verified sermon mechanics. This is an explicit product decision, not a claim about original developer intent.

## UX research

Keep three questions separate:

1. What does the game actually do?
2. What do players think it does?
3. What can a player infer from the UI without external sources?

Do not call a handful of comments a consensus. Preserve date/version context. Prefer repeated or recent examples, but keep older reports when they expose a durable current issue.

State UX gaps concretely, for example: `Before choosing a prayer, the player cannot determine X even though X materially changes Y.`

Do not assume the answer is a long tooltip. Potential solutions may include clearer technology/item text, dynamic current values, a forecast, a compact breakdown, a contextual hint, or a small panel.

## Balance / role research

Do not rebalance for symmetry. Before changing a functioning prayer classify it as one of:

- broken/disconnected;
- misleading/opaque;
- healthy niche;
- progression tool;
- dominated/redundant;
- underpowered relative to the weekly sermon opportunity cost.

Niche behavior or eventual progression obsolescence is not automatically a defect. Compare against crafting cost, unlock timing, quality requirements, alternative consumables/perks/systems, duration, and the cost of giving up another weekly sermon.

Prefer clarity first when hidden strength may explain perceived weakness. Avoid nerfs by default unless strong evidence shows a functioning prayer damages the intended choice structure.

`docs/PRAYER_DESIGN_AUDIT.md` is the current source of truth for prayer-by-prayer design verdicts.

## Player-facing clarity target

The intended end state is a **white-box player experience**, not a developer-facing formula viewer.

Internally, research must recover the complete mechanics needed to explain every player-relevant prayer. The UI should translate that model into game terms so the player can answer:

- what this prayer does;
- what current values or conditions affect it;
- what outcome/effect to expect now, including uncertainty where real;
- what changing prayer quality or other inputs changes;
- how it differs from another prayer when making a choice;
- when a selected profile repairs or intentionally changes vanilla behavior.

Prefer concrete current-state values, short dependency explanations, ranges/chances where appropriate, and visual hierarchy over raw formulas.

Use one mod-owned semantic model for all prayer presentation surfaces so technology text, item tooltip, pulpit forecast and active-profile mechanics cannot contradict each other.

## Runtime and performance constraints

Production should be event-driven and cheap:

- compute only when the relevant UI opens or refreshes;
- prefer existing game getters/state;
- avoid per-frame polling, broad Unity scans, repeated heavy reflection/enumeration, duplicate subscriptions, and persistent verbose logging;
- fixes/rebalance should patch the narrow verified behavior rather than repeatedly scanning all objects.

A diagnostic probe must answer one narrow question and be removable.

## Repository policy

Long-lived findings belong primarily in:

- `docs/PRAYER_MECHANICS.md` — stock 1.407 mechanics;
- `docs/PLAYER_UX_RESEARCH.md` — presentation/player evidence;
- `docs/PRAYER_DESIGN_AUDIT.md` — role/balance/fix judgements;
- `docs/DESIGN_NOTES.md` — product/UI architecture and accepted design direction;
- `docs/TEST_BUILD_LOG.md` — only when distributable/testable production candidates exist.

Do not commit game DLLs, full decompiled source, proprietary assets, or bulk localization/resources. Store only minimal derived facts, IDs, signatures, formulas, hashes and conclusions.

`main` is stable. Use `research/*` for evidence/design gathering and `dev/*` for runtime implementation. Unaccepted runtime behavior stays off `main`.

Do not create hosted CI for routine research/docs/bookkeeping. Use hosted CI only when a concrete executable property requires it, and preserve the clean build/handoff gate for any binary given to the user.
