# PrayerClarity 1.0.7 — final comparative tooltip polish

Status: **Clarity-only runtime candidate preparation**.

Baseline: accepted/published `PrayerClarity 1.0.1`. Immediate predecessor `candidate/1.0.6` was runtime-tested and superseded, not accepted.

## Runtime evidence from 1.0.6

User in-game review at 2560x1440 confirmed:

1. Prayer-item current-tier-only tooltips remain visually accepted and require no change.
2. Technology `max_width = 360` remains visually acceptable.
3. Property-first grouping, horizontal requirement progression, structured Prosperity reward output, Effect, and horizontal Duration direction are broadly accepted.
4. Comparative success blocks are still visually noisy when the resource icon duplicates the immediately adjacent resource name.
5. `Reward bonus on success` / equivalent wording is longer and more semantic than needed; a shorter `Bonuses on success` heading better describes the section without implying an exact final payout forecast.
6. Stock NGUI wrapping can still split a tier marker from its value, e.g. a Gold quality symbol at the end of one line with its `+%/+flat` value on the next. This is a presentation defect.

## Accepted 1.0.7 design

### Comparative success heading

The comparative-only heading is shortened to the localized equivalent of:

`Bonuses on success:`

Russian: `Бонусы при успехе:`

The prayer-item tooltip keeps its existing `Added on success` wording.

### Resource headings

Comparative Technology rows use the resource name only:

```text
Faith:
...

Donations:
...
```

The duplicate `(faith)` / `(slv)` icon beside the same noun is removed. Item tooltip icon grammar remains unchanged.

### Atomic tier segments

Every compact comparative tier segment is constructed as one unbreakable text unit. Internal spaces use U+00A0 NO-BREAK SPACE; ordinary spaces exist only **between** tier segments.

Example logical row:

```text
(s1): +10% +1 (s2): +10% +2 (s3): +10% +3
```

Internally, spaces inside each `(sN): value` segment are non-breaking. NGUI may therefore wrap only between complete Bronze/Silver/Gold segments, yielding native adaptive `3`, `2+1`, or `1+1+1` layout without a new post-build label hook or per-frame work.

The same atomic-segment rule is used for:

- varying Requirement progression;
- varying Faith progression;
- varying Donations progression;
- varying Duration progression;
- varying structured reward Quantity progression.

Runtime acceptance must verify that Graveyard Keeper's font renders U+00A0 with normal visible spacing. If it does not, this mechanism is rejected rather than hidden behind further formatting hacks.

### Why no runtime text-measurement hook

NGUI exposes printed-size/wrapping machinery, but accurate measurement depends on the concrete `UILabel` state. PrayerClarity currently builds `BubbleWidgetTextData` before that label exists. Adding a post-build label-discovery / measurement lifecycle solely for these short rows would widen reflection and lifecycle complexity for little value.

The accepted candidate strategy therefore lets stock NGUI perform the adaptive wrap while constraining legal break positions in the generated text.

## Width and unchanged surfaces

- Technology PrayerClarity mechanics-body `max_width` remains `360`.
- No global tooltip positioning changes.
- Prayer-item tooltip grammar and width remain unchanged.
- Pulpit and Temporary Effects remain unchanged.
- No prayer mechanics or balance changes.
- Percentage modifiers remain percentages; no selective `+100% -> ×2` rewrite.

## Localization

`tech.success_reward_bonus` is shortened in all 11 supported interface locales:

`en`, `fr`, `de`, `zh_cn`, `es`, `pt_br`, `ko`, `ja`, `ru`, `it`, `pl`.

No other player-facing semantics are changed.

## Requested runtime check

At 2560x1440 inspect at minimum:

1. Repose / Donations / Combo: no quality symbol may detach from its value; verify NBSP renders as ordinary visual spacing.
2. Imagination: Duration should remain compact and tier segments must stay intact.
3. Prosperity: structured reward Quantity row must remain compact and intact.
4. One prayer-item tooltip: confirm no presentation regression.
5. With `Gamepad Tooltip Position Fix` enabled, confirm controller combined Technology tooltip remains readable; PrayerClarity itself does not reposition it.

If 2560x1440 is clean, perform a 1920x1080 smoke test before stable acceptance.

Stable promotion still requires explicit user runtime acceptance. Any handed 1.0.7 DLL must be tied to a frozen `candidate/1.0.7` source ref.
