# PrayerClarity 1.0.8 viewport runtime result

Date: 2026-09-17
Status: **functional behavior confirmed; cosmetic edge-margin polish pending**.

Exact tested candidate source: `candidate/1.0.8` -> `4ce32e11f782eef10ade0c8569d206ba7bca6df3`.

User screenshots confirmed:

- 1920x1080 mouse: long prayer Technology tooltip is translated upward and its text/content remain visible;
- 1920x1080 controller without Gamepad Tooltip Position Fix: long combined tooltip is translated upward and remains readable;
- 2048x1152 controller without companion: placement remains usable;
- 2048x1152 controller with Gamepad Tooltip Position Fix: the companion's left-side placement is preserved.

Residual finding: the visible parchment/frame can visually touch the lower screen edge after clamp, despite the logical widget being clamped with a 16-screen-pixel margin. This indicates the visible frame extends beyond the measured `WidgetsBubbleGUI.widget` bounds. The issue is cosmetic, not a content, mechanics or lifecycle failure.

Decision: do not accept 1.0.8 as the final viewport-polish baseline. Carry only a minimal margin adjustment into 1.0.9; preserve the validated ownership isolation, geometry mapping, companion ordering and content unchanged.
