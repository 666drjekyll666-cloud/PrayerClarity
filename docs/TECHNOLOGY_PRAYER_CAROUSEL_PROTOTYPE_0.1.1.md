# Technology Prayer Carousel Prototype 0.1.1

Status: research-only, not a production candidate.

Target:
- Graveyard Keeper 1.407.
- PrayerClarity: Rebalanced candidate 0.2.21.
- Exact production base source: `1d8eb667d4ddbc37efd24cbd84606f1a43e19606`.
- Previous research prototype: 0.1.0 / `5c6db9c3cbcaa86cb728d93858497c197060545b`.

## Accepted evidence from 0.1.0

User runtime test confirmed:
- the single BSS Technology node can show exactly one of its three existing prayer `TechUnlock` tooltips;
- switching the selected prayer works correctly;
- dimming the two non-selected unlock icons makes the current prayer immediately legible;
- the remaining UX defect is discoverability: X/Y introduces controls the player has no reason to know.

This closes the underlying tooltip-selection and visual-highlight hypotheses.

## 0.1.1 design hypothesis

Make the three prayers behave like transient horizontal sub-slots inside the one real Technology node:

`left Technology <- prayer 1 <-> prayer 2 <-> prayer 3 -> right Technology`

Rules:
- Right on prayer 1 -> prayer 2.
- Right on prayer 2 -> prayer 3.
- Right on prayer 3 -> vanilla right navigation.
- Left on prayer 3 -> prayer 2.
- Left on prayer 2 -> prayer 1.
- Left on prayer 1 -> vanilla left navigation.
- Entering BSS from the left selects prayer 1.
- Entering BSS from the right selects prayer 3.
- Up/down and all other Technology navigation remain vanilla.
- X/Y are no longer used by the prototype.

## Mechanism

The prototype patches the existing semantic `BaseGUI.OnPressedLeft/Right` handlers, but acts only when:
- the active GUI is `TechTreeGUI`; and
- the currently focused item is the verified three-prayer BSS node.

For internal prayer moves it consumes the semantic action and rebuilds the same parent Tooltip through the selected existing `TechUnlock.GetTooltip` path.

At the first/last prayer boundary it does not replace navigation: it returns control to the original `BaseGUI.OnPressedLeft/Right -> GamepadNavigationController.Navigate` path.

A same-frame direction marker is used only to choose the boundary prayer when native navigation enters the BSS node:
- moving right into BSS -> prayer 1;
- moving left into BSS -> prayer 3.

No tech, save, unlock, cost, dependency, reveal, or prayer-mechanics state is written.

## Runtime acceptance test

One controller/perceptual test is sufficient:

1. Approach the BSS Technology node from a technology on its left; it should enter on prayer 1.
2. Press Right: prayer 1 -> 2 -> 3 -> neighbouring technology to the right.
3. Move Left back into BSS; it should enter on prayer 3.
4. Press Left: prayer 3 -> 2 -> 1 -> neighbouring technology to the left.
5. Confirm Up/Down still behave normally.

No BSS mechanic or tooltip-content retest is required beyond confirming the selected tooltip follows the highlighted icon.
