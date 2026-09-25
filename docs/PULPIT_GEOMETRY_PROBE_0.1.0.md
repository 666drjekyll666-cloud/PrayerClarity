# Pulpit Geometry Probe 0.1.0

## Research-method checkpoint

**Exact question:** after Rebalanced 0.2.33 has fully rendered a long localized pulpit Effect, what live geometry does NGUI expose for the Effect label, every craft-button UIWidget, the anchored container and the root window, and why did the 0.2.33 content-driven growth condition not fire?

**Existing-path review:** the runtime screenshot already proves the visual failure; exact source inspection proves the intended 0.2.33 measurement path; the returned log proves 0.2.33 and neutral Test Console 0.1.16 were loaded without a PrayerClarity forecast exception. None of those establishes whether the same-redraw Effect `worldCorners` were stale, the selected craft-button widget was not the visible button bounds, or another live geometry value differed from the implementation assumption.

**Why a probe:** one post-render read-only geometry snapshot is narrower and more reliable than another speculative production candidate. The neutral Test Console remains unchanged.

## Behavior

- hard-depends on PrayerClarity: Rebalanced;
- installs no Harmony patches;
- changes no UI, mechanics, save state or calendar state;
- while the pulpit is visibly open on the failing prayer, press **F8 once**;
- logs the root window/container dimensions and bounds, Effect raw/processed text plus label dimensions/bounds, every craft-button UIWidget, the exact widget production would select, and a replay of the 0.2.33 overlap/deficit calculation using the settled post-render geometry.

## Minimum runtime test

1. Use exact Rebalanced 0.2.33.
2. Neutral Test Console 0.1.16 may remain installed.
3. Add this probe DLL.
4. Open the failing Russian Repose pulpit screen and wait until it is visibly rendered.
5. Press F8 once.
6. Return `LogOutput.log`.

No sermon execution, save, Japanese/English retest, or other prayer sweep is required for this research step.
