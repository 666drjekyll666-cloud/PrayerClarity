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

Purpose: establish how the prayer/sermon system actually works, audit what the game communicates before use, research where players are confused, and only then decide whether a small BepInEx UX mod is justified.

Do not assume that the terms `prayer`, `sermon`, localized names, item IDs, `PrayCraft`, or `PrayEventDefinition` are interchangeable. Record mappings only when evidence supports them.

## Research order

Use this sequence unless new evidence justifies a narrower detour:

`catalogue -> verify mechanics -> audit presentation -> research player experience -> identify UX gap -> design options -> narrow prototype -> runtime test -> accept`

Production implementation is out of scope until the initial mechanics and UX research are strong enough to identify a concrete problem.

## Evidence labels

Use these statuses explicitly where useful:

- **fact** — supported by accepted project data, direct assembly/resource/localization inspection, runtime measurement, or another direct source;
- **hypothesis** — plausible explanation not yet proved;
- **community signal** — a player report/question useful for UX research but not proof of mechanics;
- **UX finding** — a supported mismatch between information the player needs and information the game supplies;
- **design hypothesis** — a possible fix not yet accepted;
- **accepted result** — runtime-sensitive behavior or design accepted after the required user test.

Evidence priority for mechanics:

1. accepted project data;
2. direct assemblies/resources/localization;
3. targeted calling-code inspection;
4. narrow runtime probe;
5. user in-game test when only the installed game can resolve the uncertainty.

Wiki/guides/community sources are useful for terminology, discovery, and player-experience research. They are not a substitute for direct mechanics evidence when the game data/code can answer the question.

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

Do not guess IDs, localization keys, formulas, Harmony targets, or lifecycle.

## UX research

Keep three questions separate:

1. What does the game actually do?
2. What do players think it does?
3. What can a player infer from the UI without external sources?

Do not call a handful of comments a consensus. Preserve date/version context. Prefer repeated or recent examples, but keep older reports when they directly expose a durable presentation problem that still needs a 1.407 audit.

State UX gaps concretely, for example: `Before choosing a prayer, the player cannot determine X even though X materially changes Y.`

Do not assume the answer is a long tooltip. Potential solutions may include clearer existing text, dynamic values, a forecast, a compact breakdown, a contextual hint, or a small panel.

## Player-facing clarity target

The intended end state is a **white-box player experience**, not a developer-facing formula viewer.

Internally, research must recover the complete mechanics needed to explain every player-relevant prayer: inputs, conditions, dependencies, branching, arithmetic, randomness, rounding, duration, and outputs. No relevant part of the effect should remain an unexplained black box merely because the current game UI omits it.

The eventual UI should translate that complete model into player-facing information without requiring the player to read implementation formulas or coefficient algebra. For each prayer, the player should be able to answer, in ordinary game terms:

- what this prayer does;
- what current values or conditions affect it;
- what outcome/effect to expect now, including uncertainty where real;
- what changing prayer quality or other relevant inputs changes;
- how it differs from another prayer when making a choice.

Prefer concrete current-state values, short dependency explanations, ranges/chances where appropriate, and visual hierarchy over raw formulas. Preserve completeness, but do not turn the interface into a wiki or expose implementation jargon when a simpler faithful explanation exists.

Exact presentation is deliberately undecided until the mechanics and presentation audits are complete.

## Runtime and performance constraints

Any future production mod should be event-driven and cheap:

- compute only when the relevant UI opens or refreshes;
- prefer existing game getters/state;
- avoid per-frame polling, broad Unity scans, repeated heavy reflection/enumeration, duplicate subscriptions, and persistent verbose logging.

A diagnostic probe must answer one narrow question and be removable.

## Repository policy

Long-lived findings belong in repository docs, primarily:

- `docs/PRAYER_MECHANICS.md`
- `docs/PLAYER_UX_RESEARCH.md`
- `docs/DESIGN_NOTES.md` only when design work becomes justified
- `docs/TEST_BUILD_LOG.md` only when distributable/testable builds exist

Do not commit game DLLs, full decompiled source, proprietary assets, or bulk localization/resources. Store only minimal derived facts, IDs, signatures, formulas, hashes, and conclusions.

`main` is stable. Use `research/*` for evidence gathering and `dev/*` for runtime implementation. Unaccepted runtime behavior stays off `main`.

Do not create hosted CI or build infrastructure for routine research/docs/bookkeeping. Use hosted CI only when a concrete executable property requires it, and preserve the required clean build/handoff gate for any binary given to the user.