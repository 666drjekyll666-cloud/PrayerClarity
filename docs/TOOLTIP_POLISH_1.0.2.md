# PrayerClarity 1.0.2 — tooltip polish design

Status: **user-approved Clarity UI design; implementation candidate in progress**.

This document records the accepted presentation rules for the post-1.0.1 prayer tooltip polish. It changes presentation only. It does not change prayer mechanics, sermon rewards, success probability, balance, save data, or the accepted pulpit reveal boundary.

## UX findings

Runtime screenshots of the accepted 1.0.1 baseline exposed three separate problems:

1. **Prayer item tooltips show the whole Bronze/Silver/Gold family instead of the concrete hovered item.**
   A Silver prayer item currently describes Bronze, Silver, and Gold. That duplicates the role of Technology and makes item inspection unnecessarily noisy.

2. **Technology tooltips repeat invariant information once per quality tier.**
   Requirements, success modifiers, effects, and durations are rendered as three complete mini-descriptions even when only one property changes. Invariant effects can therefore appear three times.

3. **The resulting text wall can exceed the visible screen area.**
   This already occurs at 2560x1440 for a long Technology tooltip, so 1920x1080 must be treated as an explicit runtime stress case.

## Accepted surface roles

### Technology

Technology is a **comparison/planning surface**.

It shows the complete quality progression, but the renderer is property-first rather than tier-card-first:

`requirement -> success-only contribution -> effect -> duration`

The player should see what is shared once and only see Bronze/Silver/Gold rows for properties that actually differ.

### Prayer item

The prayer-item tooltip is an **inspection surface for the concrete hovered item**.

It shows only that item's linked prayer craft and current quality tier. It must not resolve `root:1/root:2/root:3` siblings merely to reproduce the Technology comparison.

## Property-first collapsing rules

Comparison happens on semantic values before player-facing formatting/localization.

- If an atomic property is identical for every quality tier, render it once without tier repetition.
- If an atomic property differs for at least one tier, render the complete existing tier progression.
- Compound properties are compared by component. Example: if Faith rate is `+25%` for all tiers but fixed Faith is `+1/+2/+3`, show `+25%` once and show only the fixed value as the tier progression.
- Do not use partial shorthand such as `Bronze/Silver +25%, Gold +50%`; either a value is common or the full tier progression is shown.
- Zero and absence are not silently conflated when a varying progression makes zero materially informative.
- Special-effect equality must be based on a semantic fingerprint/raw values, not equality of localized display strings.
- Duration is an independent property from effect magnitude/text and may collapse independently.

This grammar is intended to survive Clarity, future Vanilla Fixes, and future Balance/Rework without changing the comparison language.

## Native symbols

Use the game's verified inline symbols rather than repeating obvious nouns where the symbol is unambiguous:

- `(s1)`, `(s2)`, `(s3)` — Bronze/Silver/Gold quality;
- `(cross)` — Church Quality requirement;
- `(faith)` — Faith;
- `(slv)` — money context.

The requirement progression therefore uses the form:

`(s1) 10 (cross)   (s2) 20 (cross)   (s3) 30 (cross)`

For donation modifiers, retain a localized `Donations` noun next to the money symbol because money denomination and the semantic concept of a donation modifier are not strictly identical.

## Visual hierarchy

- Preserve vanilla title/lore/crafting context.
- Keep the `Prayer details`/`Параметры молитвы` header as the section boundary.
- Render the PrayerClarity mechanics body left-aligned.
- Use consistent blank space between semantic sections and compact spacing inside a section.
- Do not add decorative panels, custom assets, or extra icon systems for this polish.

## Adaptive progression layout

Use a conservative adaptive rule for the first runtime candidate:

- short scalar progressions such as Church Quality requirement and duration may be horizontal;
- a progression containing one varying scalar component may be horizontal;
- compound progressions with multiple varying components are vertical;
- long effect text is always vertical when quality-specific.

Do not introduce font measurement/reflection machinery before runtime evidence shows it is necessary. The first candidate should determine how much overflow is solved by information compaction alone.

## Overflow policy

Do **not** patch tooltip screen placement in the first 1.0.2 candidate.

First remove redundant content and verify at:

- 2560x1440;
- 1920x1080;
- Russian;
- preferably German or another long-string locale.

Only if a normalized tooltip still leaves the screen should PrayerClarity investigate the narrow native placement/clamping seam as a separate problem.

## Architecture

Keep one semantic source of truth:

- `PrayerForecast.TierDetails` owns raw requirement, Faith/donation contribution, and structured special-effect semantics;
- special effect core meaning and duration remain separate semantic properties;
- `TooltipDetailsRenderer` owns Technology comparative rendering and current-item rendering;
- the pulpit and Temporary Effects accepted presentation remain unchanged by this work.

No polling, broad Unity scan, mirrored runtime state, or mechanics mutation is justified.
