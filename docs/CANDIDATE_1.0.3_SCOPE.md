# PrayerClarity 1.0.3 — tooltip structure refinement candidate

Status: **Clarity-only development candidate preparation**.

Baseline: accepted/published `PrayerClarity 1.0.1`. Immediate predecessor `candidate/1.0.2` was runtime-tested and superseded, not accepted.

## Accepted design refinements

1. **Success resources use one visual language.**
   - If a contribution is identical across tiers, show it once.
   - Any tier-dependent contribution renders vertically, one tier per line.
   - Every tier-dependent Faith value carries `(faith)` on that line.
   - Every tier-dependent donation value carries `(slv)` on that line.
   - Requirement remains the deliberate compact horizontal exception.

2. **Duration is atomic per tier.**
   - Shared duration renders once.
   - Varying duration renders one complete tier per line so NGUI cannot detach the quality symbol or localized unit from the value.

3. **Crafting-location grammar is normalized only on prayer tooltips.**
   - `Heading: value1,value2` becomes `Heading:\nvalue1, value2`.
   - The existing vanilla localized heading and workstation names are preserved.

4. **Pure single-item prayer rewards are property-first.**
   - Reward identity is shown once.
   - Quantity is a separate semantic property and becomes the tier progression.
   - The vanilla localized reward description is shown once where verified (Blessing of Commerce).
   - Mixed/multi-reward states fall back to the existing complete special text rather than hiding data.

## Implementation constraints

- Clarity only; no prayer mechanics/balance changes.
- Pulpit and Temporary Effects grammar remain untouched.
- `TechUnlock.GetTooltip(Tooltip)` and `ItemDefinition.GetTooltipData(Item,bool)` remain the native seams.
- Reward comparison uses raw `CraftDefinition.output` IDs/counts before localization and only at tooltip render time; no polling/cache/mirrored catalogue.
- Unexpected structures fail back to complete existing text.
- No tooltip screen-position/clamping patch yet; first determine the residual overflow after semantic compaction.

## Requested runtime check

At 2560x1440 inspect:

- Repose / Donations / Combo: tier-dependent success values must be visually self-identifying by resource icon and use consistent vertical rows;
- Imagination/Excellence: each duration tier must remain intact on one line;
- Shoots & Roots: common broken-effect notice still appears once;
- Prosperity: Blessing of Commerce name and description appear once, quantity carries the tier progression, and the tooltip no longer runs off-screen if content compaction is sufficient;
- one prayer item tooltip: current-tier-only behavior remains intact;
- crafting locations: heading on its own line and comma-space between multiple stations;
- pulpit and Temporary Effects: no regression.

If 2560x1440 is clean, only then perform the 1920x1080 overflow smoke test.
