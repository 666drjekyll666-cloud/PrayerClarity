# PrayerClarity 1.0.2 — tooltip polish candidate scope

Status: **runtime-tested / superseded by 1.0.3; not accepted**.

Baseline: accepted/published `PrayerClarity 1.0.1`, exact runtime source `7cf6d9287d2aa7cfa8c0529be98f62a3d87360ce`.

Canonical design for this candidate: `docs/TOOLTIP_POLISH_1.0.2.md`.

## Frozen build identity

- Frozen candidate ref: `candidate/1.0.2`.
- Exact build source SHA: `6bf5fec32feac3567c5bfe2f450c942cc8d5be19`.
- GitHub Actions run: `35084090426`.
- Workflow artifact ID: `10441760708` (`PrayerClarity-1.0.2-ci-6bf5fec32feac3567c5bfe2f450c942cc8d5be19`).
- Artifact ZIP digest: `sha256:a4487ce684c9556ae6380409267f9f49ef6b2c0f228abb31ab3c459a56238f8d`.
- Handoff DLL SHA-256: `734a0855abd1c808357b247740ca47afc85e1857a7cd4ab88d0be46eb88cbc6d`.

## Runtime result — 2026-09-16

The property-first direction and current-tier-only item tooltip were accepted as the correct design direction, but the candidate itself was not accepted.

Confirmed good:

- item tooltip shows only the hovered prayer tier;
- Technology requirements read naturally as one compact horizontal quality progression;
- shared effects collapse correctly in cases such as Shoots & Roots;
- overall left-aligned property-first hierarchy is substantially clearer than 1.0.1.

Remaining defects observed in the supplied 2560x1440 screenshots:

- a shared Faith percentage followed by `★ +1 / ★★ +2 / ★★★ +3` is semantically ambiguous because the tier rows lack the Faith icon;
- the same ambiguity exists for tier-dependent donation values;
- success properties mix horizontal and vertical progressions, weakening the visual language;
- varying duration can wrap the unit away from the Gold value;
- the native crafting-location row remains `Heading: value1,value2` instead of the accepted heading/newline grammar and lacks spaces after commas;
- Prosperity repeats the full Blessing of Commerce name/description for each tier because reward identity/description and reward quantity are not yet separate semantic properties;
- Prosperity still overflows below the visible screen at 2560x1440, so a 1080p test is deferred until content compaction is corrected.

`candidate/1.0.2` remains immutable evidence and must not be moved or rebuilt.
