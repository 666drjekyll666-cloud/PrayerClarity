# PrayerClarity 1.0.2 — tooltip polish candidate scope

Status: **Clarity-only runtime candidate preparation**.

Baseline: accepted/published `PrayerClarity 1.0.1`, exact runtime source `7cf6d9287d2aa7cfa8c0529be98f62a3d87360ce`.

Canonical design for this candidate: `docs/TOOLTIP_POLISH_1.0.2.md`.

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

Stable promotion requires explicit runtime acceptance. `candidate/1.0.2`, once created, is immutable evidence and must not replace or move `candidate/1.0.1` or `accepted/clarity-1.0.1`.
