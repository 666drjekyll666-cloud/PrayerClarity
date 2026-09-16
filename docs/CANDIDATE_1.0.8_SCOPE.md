# PrayerClarity 1.0.8 — Technology tooltip viewport safety

Status: **Clarity-only runtime candidate handed for verification**.

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

## Frozen candidate / build evidence

- Frozen ref: `candidate/1.0.8`.
- Exact source SHA: `4ce32e11f782eef10ade0c8569d206ba7bca6df3`.
- GitHub Actions run: `35106747551` — success.
- Build result: `0 Warning(s)`, `0 Error(s)`.
- Embedded locale gate passed for all 11 supported locales: `en`, `fr`, `de`, `zh_cn`, `es`, `pt_br`, `ko`, `ja`, `ru`, `it`, `pl`.
- Workflow artifact ID: `10450444006`.
- Artifact name: `PrayerClarity-1.0.8-ci-4ce32e11f782eef10ade0c8569d206ba7bca6df3`.
- Artifact ZIP SHA-256: `4d7dbadab1705b4599f57d8919d395331ce85897c9beed208de612389acc58be`.
- DLL filename in artifact: `PrayerClarity-1.0.8-ci.dll`.
- DLL SHA-256: `200d6a8b40324d558a0ec4d7c8fe6ea6478082a50933a9d0665916abdffbc888`.
- Handoff verification: the artifact was downloaded after CI, extracted, and the DLL SHA-256 was independently rechecked against `BUILD_INFO.txt` and CI output. A convenience copy named `PrayerClarity.dll` has identical bytes/hash; no rebuild was performed.

## Requested runtime acceptance

Test the exact candidate DLL on Graveyard Keeper 1.407:

1. **2560x1440, mouse:** inspect a long prayer Technology tooltip (Repose / Prosperity). It should remain in its accepted location unless it actually crosses a viewport edge.
2. **1920x1080, mouse:** inspect Repose / Prosperity / Excellence. The bubble frame and text should remain inside the viewport with a small lower safety margin.
3. **1920x1080, controller, companion disabled:** inspect a long/combined prayer Technology tooltip. It should remain on-screen.
4. **1920x1080, controller, Gamepad Tooltip Position Fix 1.3.0 enabled:** its preferred lower-left placement should remain recognizable, but PrayerClarity must move it inward only when an edge would otherwise be crossed.
5. **One ordinary non-prayer Technology tooltip:** confirm its placement is unchanged.
6. Check `BepInEx/LogOutput.log`: no PrayerClarity viewport-safety error should appear.

Stable promotion still requires explicit user runtime acceptance. The frozen `candidate/1.0.8` ref remains at the exact runtime source SHA above; this post-freeze documentation update lives only on the dev workstream and does not alter candidate bytes.
