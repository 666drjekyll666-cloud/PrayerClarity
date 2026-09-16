# PrayerClarity 1.0.5 — comparative-row width calibration

Status: **Clarity-only runtime candidate preparation**.

Baseline: accepted/published `PrayerClarity 1.0.1`. Immediate predecessor `candidate/1.0.4` was runtime-tested and superseded, not accepted.

## Runtime evidence from 1.0.4

User in-game review at 2560x1440 confirmed:

1. Prayer-item current-tier-only tooltips remain visually accepted.
2. Property-first Technology prayer tooltips are substantially cleaner and shorter than the pre-polish layout.
3. The fixed `max_width = 300` mechanics body is still too narrow for some horizontal success progressions: a tier segment such as Silver Faith can split across lines.
4. The three-space separator between tier segments can leave a misleading visual indent when a later tier wraps.
5. The wider/compact approach itself is preferred over reverting to one tier per line.
6. Graveyard Keeper gamepad Technology tooltips may concatenate multiple unlock descriptions into one very tall controller bubble and can leave the screen.
7. The separate stable `Gamepad Tooltip Position Fix 1.3.0` resolves that controller-placement problem by anchoring controller tooltips in the lower-left and letting long bubbles grow upward.

## Accepted 1.0.5 refinement

### Technology prayer mechanics width

Only the PrayerClarity-generated **Technology mechanics body** changes from:

`max_width = 300`

to:

`max_width = 360`

This remains a local `BubbleWidgetTextData.max_width` value. It does not resize the global Tooltip system and does not affect prayer-item tooltips.

### Comparative success spacing

The Faith and Donations progression grammar is unchanged:

```text
Faith:
(s1) +25% +1 (faith) (s2) +25% +2 (faith) (s3) +25% +3 (faith)

Donations:
(s1) +25% (slv) (s2) +50% (slv) (s3) +75% (slv)
```

The separator between complete tier segments changes from three ordinary spaces to one ordinary space. This:

- saves horizontal width;
- removes the visible leftover indent caused by wrapping inside the old three-space separator;
- preserves the native quality/resource symbols as the visual boundaries between tiers.

No non-breaking-space or custom font behavior is introduced in this candidate. Acceptance depends on real runtime layout at the wider fixed width.

## Gamepad companion decision

PrayerClarity does **not** embed or require `Gamepad Tooltip Position Fix`.

Reasoning:

- the controller-position fix is a generic UI concern, not prayer-mechanics clarity;
- it is already a separate stable public mod with its own accepted behavior and lifecycle;
- embedding it would duplicate maintenance and introduce a per-frame positioning patch into PrayerClarity;
- making it a hard dependency would burden mouse/keyboard users who do not need it.

Publication direction: document **Gamepad Tooltip Position Fix 1.3.0 as a recommended companion for controller users**, especially because vanilla can combine multiple Technology descriptions into one long gamepad tooltip.

## Unchanged surfaces

- prayer-item tooltip grammar and width;
- pulpit Clarity;
- Temporary Effects;
- requirement row grammar;
- duration row grammar;
- effect/reward/crafting-location grammar;
- prayer mechanics and balance;
- global tooltip positioning.

## Requested runtime check

At 2560x1440 inspect at minimum:

1. Faith, Donations, Combo and Prosperity Technology tooltips: each Bronze/Silver/Gold success segment should remain visually intact and the Gold segment should not begin with a false indent.
2. Confirm `360` does not make the mouse tooltip feel excessively wide or collide awkwardly with screen edges.
3. Confirm one prayer-item tooltip is unchanged.
4. With `Gamepad Tooltip Position Fix 1.3.0` enabled, confirm the combined controller Technology tooltip remains fully readable in the lower-left.

If the 2560x1440 result is clean, perform a 1920x1080 smoke test before stable acceptance.

Stable promotion still requires explicit user runtime acceptance. Any handed 1.0.5 DLL must be tied to a frozen `candidate/1.0.5` source ref.
