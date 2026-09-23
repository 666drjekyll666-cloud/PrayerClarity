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

`catalogue -> verify mechanics -> audit presentation -> research player experience -> identify UX gap -> design options -> narrow prototype -> runtime test -> accept`

Keep evidence states distinct: **fact**, **hypothesis**, **community signal**, **UX finding**, **design hypothesis**, and **accepted result**.

Do not treat `prayer`, `sermon`, localized names, or internal identifiers as interchangeable without evidence.

Do not guess game IDs, formulas, localization keys, Harmony targets, lifecycle, UI ownership, or final writers when they can be established from accepted project/shared research or direct inspection.

Apply the DevRules per-change evidence gate before production changes. Do not bundle independent unresolved hypotheses into one candidate.

## Research and knowledge

Project-specific prayer mechanics, UX decisions, balance, candidate/release state, and acceptance evidence stay canonical in PrayerClarity docs.

Reusable Graveyard Keeper host/runtime facts belong in the shared research repository after acceptance.

Project Instructions are only the persistent bootstrap layer. Do not store changing candidate versions, SHAs, open bugs, temporary hypotheses, or current implementation plans here.

## User-operation boundary

Use GitHub, CI, source inspection, research tools, and disposable probes directly when available.

Do not ask the user to perform Git operations, find source, copy C#, decompile the game, retrieve CI artifacts, or repeat mechanical diagnostics that tools can handle.

Ask the user only for product/design decisions and for installed-game runtime/perceptual evidence that genuinely requires their environment.

Prefer a narrow automated research harness when it reduces repetitive, timing-sensitive, RNG, arithmetic, transcription, or fragile manual testing.

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
