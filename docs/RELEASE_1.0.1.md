# PrayerClarity 1.0.1 — accepted release evidence

Status: **accepted / stable / published**.

## Identity

- Version: `1.0.1`
- Exact runtime/source SHA: `7cf6d9287d2aa7cfa8c0529be98f62a3d87360ce`
- Frozen candidate ref: `candidate/1.0.1`
- Frozen accepted ref: `accepted/clarity-1.0.1`
- Candidate build run: `35072962204`
- Candidate workflow artifact ID: `10437097204`
- Candidate artifact: `PrayerClarity-1.0.1-ci-7cf6d9287d2aa7cfa8c0529be98f62a3d87360ce`
- Artifact ZIP digest: `sha256:dadf99d291e272a24b6dc14ec9489835da6de88165d3290eb38125d42c5a22a0`
- Candidate DLL: `PrayerClarity-1.0.1-ci.dll`
- DLL SHA-256: `476380ef08a1cced70cb47968f1263a04c03e0e1f2c81d7fd6a3ef057c24c5f2`
- Supported Assembly-CSharp MVID: `6f50b8e7-156b-49ac-bbe8-7505894b2364`

## Build gate

Run `35072962204` completed successfully on `ubuntu-latest` from the exact candidate source above. The produced artifact remained immutable through runtime acceptance and stable publication.

The candidate artifact was independently re-downloaded before promotion and its raw DLL SHA-256 was re-verified as `476380ef08a1cced70cb47968f1263a04c03e0e1f2c81d7fd6a3ef057c24c5f2`.

No rebuild was used for stable publication.

## Final runtime acceptance

On 2026-09-16 the user tested the exact 1.0.1 candidate in Graveyard Keeper 1.407 and explicitly approved it for stable/main promotion and public distribution.

The supplied runtime evidence confirms:

- `PrayerClarity 1.0.1` loaded successfully;
- the game was Graveyard Keeper 1.407;
- `PrayerClarity` exposed no Configuration Manager entries;
- a real ordinary-prayer inventory tooltip used the new PrayerClarity item-tooltip presentation;
- the test-only item-cell bridge then drew synthetic prayer items into the native `PrayCraftGUI` item cell without inventory/save mutation, allowing normal hover/tooltips for otherwise unavailable prayers;
- Combo Gold rendered the full quality ladder with native stars, requirements, and grouped Faith/donation success modifiers;
- Shoots & Roots Silver rendered the quality ladder plus the current special-effect/problem text correctly;
- the runtime log records repeated native item-cell draws for synthetic prayer IDs and no PrayerClarity runtime error in the tested sequence.

This closes the remaining Clarity presentation surface that was absent from 1.0.0.

## Stable publication

The exact accepted DLL was promoted without rebuilding by GitHub Actions run `35076380012`.

- GitHub Release: `v1.0.1`
- Release target: `7cf6d9287d2aa7cfa8c0529be98f62a3d87360ce`
- Stable asset name: `PrayerClarity.dll`
- Stable asset SHA-256: `476380ef08a1cced70cb47968f1263a04c03e0e1f2c81d7fd6a3ef057c24c5f2`
- Release asset bytes match the runtime-tested candidate exactly; only the filename changed to the canonical installed filename.

## Product scope

PrayerClarity 1.0.1 is **Clarity-only**. It changes presentation, not prayer balance or gameplay mechanics.

Accepted Clarity surfaces are now:

- pulpit;
- Technology prayer previews;
- native prayer-item tooltips;
- Character -> Temporary Effects.

Future Vanilla Fixes and Balance/Rework work remain separate from this accepted baseline.
