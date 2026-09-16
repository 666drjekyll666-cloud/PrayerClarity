# PrayerClarity 1.0.2 — tooltip polish candidate scope

Status: **clean-built runtime candidate awaiting user in-game review**.

Baseline: accepted/published `PrayerClarity 1.0.1`, exact runtime source `7cf6d9287d2aa7cfa8c0529be98f62a3d87360ce`.

Canonical design for this candidate: `docs/TOOLTIP_POLISH_1.0.2.md`.

## Frozen build identity

- Frozen candidate ref: `candidate/1.0.2`.
- Exact build source SHA: `6bf5fec32feac3567c5bfe2f450c942cc8d5be19`.
- GitHub Actions run: `35084090426`.
- Workflow result: success on `ubuntu-latest`; restore, `net472` Release build, all 11 embedded-locale checks, artifact staging and upload passed with `0 warnings / 0 errors`.
- Workflow artifact ID: `10441760708` (`PrayerClarity-1.0.2-ci-6bf5fec32feac3567c5bfe2f450c942cc8d5be19`).
- Artifact ZIP digest: `sha256:a4487ce684c9556ae6380409267f9f49ef6b2c0f228abb31ab3c459a56238f8d`.
- Handoff DLL: `PrayerClarity-1.0.2-ci.dll`.
- Handoff DLL SHA-256: `734a0855abd1c808357b247740ca47afc85e1857a7cd4ab88d0be46eb88cbc6d`.
- Supported Assembly-CSharp MVID: `6f50b8e7-156b-49ac-bbe8-7505894b2364`.
- The downloaded artifact DLL was independently SHA-256 rechecked before handoff and matched the workflow-produced hash.

## Product change

1. Technology prayer tooltips switch from three repeated tier mini-descriptions to a property-first comparison:
   - shared values render once;
   - differing values render as the full quality progression;
   - effect and duration can collapse independently;
   - the mechanics body is left-aligned.

2. Prayer-item tooltips become current-item-only:
   - hovering Bronze shows Bronze;
   - hovering Silver shows Silver;
   - hovering Gold shows Gold;
   - no sibling-quality lookup is performed for item inspection.

3. Native `(s1)/(s2)/(s3)`, `(cross)`, `(faith)`, and money symbols remain the visual vocabulary.

4. New structural labels required by the compact grammar are localized in all 11 supported game locales.

## Explicit non-scope

- no prayer mechanics changes;
- no sermon reward changes;
- no success-probability changes;
- no Vanilla Fixes;
- no Balance/Rework;
- no pulpit redesign;
- no Temporary Effects redesign;
- no tooltip screen-position/clamping patch in this first candidate.

## Implementation boundary

- retain `TechUnlock.GetTooltip(Tooltip)` as the Technology seam;
- retain `ItemDefinition.GetTooltipData(Item, bool)` as the item seam;
- retain `PrayerForecast.BuildTierDetails` as the semantic source;
- compare raw semantic values before localization;
- preserve the existing safe fallback: unexpected tooltip shapes retain vanilla data and append Clarity rather than deleting unknown rows;
- no per-frame work or broad UI scans.

## Requested runtime test

At minimum:

1. **Technology / Shoots & Roots**
   - the known inactive effect should appear once when identical across tiers;
   - requirements and any varying resource components remain clear.

2. **Technology / Imagination or Excellence**
   - common effect magnitude should appear once;
   - tier-dependent duration should appear as a separate compact progression.

3. **Technology / Combo or Donations**
   - Faith/donation components should collapse independently where values are shared;
   - the comparison must remain easy to scan.

4. **Prayer item tooltip**
   - hover a Silver prayer and confirm only Silver mechanics are shown;
   - repeat for another quality if convenient.

5. **Geometry**
   - inspect at the normal 2560x1440 setup;
   - run a 1920x1080 smoke if practical;
   - report any remaining clipping/off-screen placement separately from content readability.

6. **Regression**
   - pulpit and Character -> Temporary Effects should behave exactly as in accepted 1.0.1;
   - no PrayerClarity runtime errors should appear.

Stable promotion requires explicit runtime acceptance. `candidate/1.0.2` is immutable evidence and must not replace or move `candidate/1.0.1` or `accepted/clarity-1.0.1`.
