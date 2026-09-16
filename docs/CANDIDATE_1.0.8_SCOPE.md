# PrayerClarity 1.0.8 — Technology tooltip viewport safety

Status: **Clarity-only runtime candidate preparation**.

Baseline: exact `candidate/1.0.7` source `972917f2a115e34eca6f1d476f911b6280ab0474`.

## Confirmed problem

Runtime review found that the accepted 1.0.7 Technology content can fit well at 2560x1440 while long prayer Technology tooltips extend below the viewport at 1920x1080. This is a placement defect, not a reason to remove or compress accepted Clarity information.

Viewport probe 0.1.8 then measured the exact final bubble geometry on both resolutions:

- 2560x1440: prayer tooltip projected Y `50..690` and is fully visible;
- 1920x1080: the same `150 x 320` bubble projected Y `-118..522`, proving a 118-pixel bottom overflow.

The probe also proved the active NGUI coordinate mapping from `UIRoot.manualHeight`, screen dimensions and the actual UI-camera projection.

Research evidence and derivation are recorded on `research/viewport-clamp-1.0.8` in `docs/VIEWPORT_CLAMP_RESEARCH.md`.

## Candidate behavior

1. PrayerClarity marks only Technology `Tooltip` instances to which its prayer mechanics block was successfully applied.
2. After `Tooltip.Show(bool)` creates the concrete `TooltipBubbleGUI`, only that exact bubble is weakly marked.
3. A postfix on the proven-late `WidgetsBubbleGUI.Update()` seam checks the marker and immediately returns for every unrelated bubble.
4. For a marked PrayerClarity Technology bubble, its center position is minimally clamped to the current `Screen.safeArea` with a 16-screen-pixel safety margin.
5. Bounds are derived at runtime from `UIRoot.manualHeight` and current screen dimensions; no 1080p/1440p coordinate constants are hardcoded.
6. If a bubble is physically larger than the usable viewport on an axis, PrayerClarity leaves vanilla placement unchanged on that axis rather than selecting an arbitrary hidden edge.

## Compatibility with Gamepad Tooltip Position Fix

`Gamepad Tooltip Position Fix 1.3.0` also patches `WidgetsBubbleGUI.Update()` for controller positioning.

PrayerClarity's viewport postfix is explicitly ordered after Harmony owner:

`nikich.gyk.movegamepadtooltips`

Therefore the companion chooses its preferred controller location first; PrayerClarity then applies only the minimum translation required to keep a PrayerClarity Technology bubble within the viewport.

If the companion is absent, the same safety clamp operates on vanilla placement.

## Runtime cost

The recurring path performs no reflection discovery, hierarchy scan or LINQ.

Bindings are resolved/compiled during plugin initialization. On each `WidgetsBubbleGUI.Update()` call, an unrelated bubble pays only a weak marker lookup and immediate return. Only a marked PrayerClarity Technology bubble reads cached widget/root state, current screen/safe-area values, performs simple arithmetic, and writes `Transform.localPosition` only if a correction is actually required.

## Unchanged behavior

- Prayer content and all 1.0.7 wording/layout remain unchanged.
- Prayer-item tooltips remain unchanged.
- Pulpit remains unchanged.
- Temporary Effects remain unchanged.
- Ordinary non-prayer Technology tooltips are not marked and are not clamped by PrayerClarity.
- No prayer mechanics or balance values change.
- No new localization strings are introduced.
- The separate `Craft for sermon not found: ...` log-noise cleanup is intentionally not coupled to this candidate.

## Requested runtime acceptance

Test the exact candidate DLL on Graveyard Keeper 1.407:

1. **2560x1440, mouse:** inspect a long prayer Technology tooltip (Repose / Prosperity). It should remain in its accepted location unless it actually crosses a viewport edge.
2. **1920x1080, mouse:** inspect Repose / Prosperity / Excellence. The bubble frame and text should remain inside the viewport with a small lower safety margin.
3. **1920x1080, controller, companion disabled:** inspect a long/combined prayer Technology tooltip. It should remain on-screen.
4. **1920x1080, controller, Gamepad Tooltip Position Fix 1.3.0 enabled:** its preferred lower-left placement should remain recognizable, but PrayerClarity must move it inward only when an edge would otherwise be crossed.
5. **One ordinary non-prayer Technology tooltip:** confirm its placement is unchanged.
6. Check `BepInEx/LogOutput.log`: no PrayerClarity viewport-safety error should appear.

Stable promotion still requires explicit user runtime acceptance. A numbered 1.0.8 DLL must correspond to one frozen `candidate/1.0.8` source ref and recorded artifact hash.
