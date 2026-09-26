# Pulpit Geometry Probe 0.1.1

## Research-method checkpoint

**Exact question:** after Rebalanced 0.2.35 finishes its four bounded settled-layout passes, what NGUI anchor/component state owns the craft-button geometry, and why does the final transform correction not persist?

**Why this existing probe is extended:** the 0.2.35 Japanese F8 dump proves the fourth pass does not leave different final geometry from 0.2.34, but probe 0.1.0 did not expose button anchors or root components. Source inspection proves PrayerClarity writes `craft button.transform.localPosition`; it does not prove whether NGUI anchors subsequently rewrite that transform.

**Behavior:** still read-only. No Harmony patches, no UI/game/save mutation. F8 now logs:
- settled Effect and window geometry;
- the union of all active visible craft-button widgets using the same geometry rule as 0.2.35;
- every craft-button widget's NGUI anchor state and anchor targets/offsets;
- all components on the craft-button root.

## Minimum runtime test

1. Use exact Rebalanced 0.2.35 and Neutral Test Console 0.1.17 if useful.
2. Replace probe 0.1.0 with **probe 0.1.1**.
3. Open Japanese Soul's Repose, wait until fully rendered, press **F8 once**.
4. Return `LogOutput.log`.

No Russian/Korean repeat, sermon execution, or Faith test is required for this research step.
