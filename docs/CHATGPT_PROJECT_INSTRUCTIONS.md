# ChatGPT Project Instructions — PrayerClarity

We are developing **PrayerClarity** for **Graveyard Keeper 1.407**.

Repository: `NikichMods/PrayerClarity`  
Shared host/runtime research: `NikichMods/GraveyardKeeperResearch`

Purpose: establish how prayers/sermons actually work, make their effects understandable at the point of choice, preserve a vanilla-faithful edition, and maintain a separately explicit Rebalanced edition where intentional gameplay changes are accepted.

## Mandatory startup / recovery

Before substantive technical work:

1. inspect the current PrayerClarity repository state, including relevant branches, commits, PRs, build/test evidence, and current docs;
2. read the canonical global contract in `NikichMods/DevRules`:
   - `ENGINEERING_RULES.md`;
   - `CI_POLICY.md`;
   - `GIT_WORKFLOW.md`;
   - `PROJECT_BOOTSTRAP.md`;
   - `RUNTIME_TEST_HARNESS.md` when runtime evidence is relevant;
3. read the current PrayerClarity `AGENTS.md`;
4. read the project docs relevant to the current task;
5. before fresh Graveyard Keeper host/UI/runtime research, check `NikichMods/GraveyardKeeperResearch/docs/RESEARCH_INDEX.md` and linked shared facts.

Repository state and accepted runtime evidence outrank chat memory, old handoff messages, and mutable state that may once have appeared in Project Instructions.

## Project method

Use the project evidence chain:

`catalogue -> verify mechanics -> audit presentation -> research player experience -> identify UX gap -> define acceptance envelope -> compare viable solution families -> choose the least-complex adequate path -> READY/BLOCKED evidence gate -> narrow prototype -> runtime test -> accept`

Keep the user/player outcome separate from the first implementation idea. When materially different approaches could satisfy the same goal, apply the DevRules solution-space checkpoint before substantial implementation or fresh research. Re-open that choice after a failed candidate or when continuing the chosen path would require materially deeper host/UI/runtime research. Generality is not a requirement by itself.

Keep evidence states distinct: **fact**, **hypothesis**, **community signal**, **UX finding**, **design hypothesis**, and **accepted result**.

Do not treat `prayer`, `sermon`, localized names, or internal identifiers as interchangeable without evidence.

Do not guess game IDs, formulas, localization keys, Harmony targets, lifecycle, UI ownership, or final writers when they can be established from accepted project/shared research or direct inspection.

Apply the DevRules per-change evidence gate before production changes.

Before the first production-source mutation for each materially independent behavior change, make a concise gate checkpoint reviewable in chat or the repository: observable property, canonical owner, final writer/consumer/commit point where applicable, blast radius, preserved invariants, acceptance evidence, and gate state **READY** or **BLOCKED**.

There is no small/obvious/presentation-only/follow-up exception. **BLOCKED means research/probe only; do not edit production behavior under that gate.** A new runtime/user-visible regression opens a gate for that exact property; prior evidence may be reused only when it proves the relevant owner/final-writer path.

Treat the reported defect/request as the default scope. Adjacent wording, mechanics, layout, data semantics, lifecycle, and other nearby behavior are preserved unless the proved path requires changing them or the user separately accepts the additional change.

Do not optimize for fewer in-game test cycles or candidate versions by bypassing or combining unresolved gates. Do not bundle independent unresolved hypotheses into one production candidate.

For numbered PrayerClarity production candidates, follow the repository's `docs/CANDIDATE_GATE_TEMPLATE.json` / candidate-CI contract. Research-only Test Console/probe builds remain separate.

## Research and knowledge

Project-specific prayer mechanics, UX decisions, balance, candidate/release state, and acceptance evidence stay canonical in PrayerClarity docs.

Reusable Graveyard Keeper host/runtime facts belong in the shared research repository after acceptance.

Project Instructions are only the persistent bootstrap layer. Do not store changing candidate versions, SHAs, open bugs, temporary hypotheses, or current implementation plans here.

## User-operation boundary

Use GitHub, CI, source inspection, research tools, and disposable probes directly when available.

Do not ask the user to perform Git operations, find source, copy C#, decompile the game, retrieve CI artifacts, or repeat mechanical diagnostics that tools can handle.

Ask the user only for product/design decisions and for installed-game runtime/perceptual evidence that genuinely requires their environment.

Prefer a narrow automated research harness when it reduces repetitive, timing-sensitive, RNG, arithmetic, transcription, or fragile manual testing.

Do not automate merely to eliminate a cheap user action. Before creating new research code, state the exact question, whether accepted evidence/direct inspection/an existing exact artifact/a short direct runtime action can answer it, and why the probe is simpler or more reliable if those paths are insufficient. Prefer fewer assumptions and moving parts over fewer in-game clicks, DLL swaps, restarts, or test cycles.

## New chats

No special first-message handoff is required inside this ChatGPT Project.

When a new chat starts, recover current state from GitHub and accepted evidence before substantive implementation. Do not rely on the previous chat being available.

## Iteration report

After a substantial iteration, report briefly:
- what was unknown;
- what is now proved or changed;
- what UX/community findings materially changed;
- what narrow question remains;
- whether an in-game test is required, and exactly which one.

Do not repeat already accepted runtime tests without a concrete reason.
