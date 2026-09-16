# PrayerClarity 1.0.6 — comparative Technology grammar refinement

Status: **Clarity-only runtime candidate preparation**.

Baseline: accepted/published `PrayerClarity 1.0.1`. Immediate predecessor `candidate/1.0.5` was runtime-tested and superseded, not accepted.

## Runtime evidence from 1.0.5

User in-game review at 2560x1440 confirmed:

1. Prayer-item current-tier-only tooltips remain visually accepted and require no change.
2. Technology `max_width = 360` is visually acceptable and provides enough room for the intended compact comparative layout.
3. Mouse Technology tooltips now generally match the intended property-first design.
4. A remaining ambiguity exists in comparative success rows: a quality star followed by `+% +flat` can read like the star itself is being modified rather than the star identifying the prayer tier.
5. Repeating the resource icon after every tier value spends width without adding meaning.
6. A long row can still wrap awkwardly, including a case where the Gold quality marker became visually separated from its value.
7. Vertical Duration rows are clear but spend unnecessary height now that the Technology body is wider.
8. Controller combined Technology tooltips remain a separate placement concern. `Gamepad Tooltip Position Fix` stays a recommended companion rather than embedded code or a hard dependency.

## Accepted 1.0.6 grammar

### Success-section heading

Comparative Technology tooltips use a new localized heading equivalent to:

`Reward bonus on success:`

Russian: `Бонус к награде при успехе:`

This key is **comparative-only**. The prayer-item tooltip keeps the existing `Added on success` wording because that surface was already visually accepted.

### Resource header

The resource icon and name move to the resource heading and appear once:

```text
(faith) Faith:
...

(slv) Donations:
...
```

This removes repeated resource icons from Bronze/Silver/Gold values and frees horizontal space.

### Quality marker grammar

A quality symbol is an explicit tier label, not an operand. Comparative values use:

```text
(s1): +25% +1  (s2): +25% +2  (s3): +25% +3
```

The colon is intentional: it makes the symbol read as `Bronze:` / `Silver:` / `Gold:` and removes the visual interpretation that the bonus is applied to the star itself.

### Duration

When duration differs by quality, it becomes one horizontal progression row:

```text
Duration:
(s1): 1.6 days  (s2): 3.2 days  (s3): 4.8 days
```

Common duration remains a single scalar value.

### Percentage semantics

Percentage modifiers remain percentages. PrayerClarity does not selectively rewrite `+100%` as `×2`, because additive-percent notation and multiplier notation are different grammars and mixing them across tiers would reduce semantic consistency. The Clarity layer reports the verified modifier directly.

## Width and unchanged surfaces

- Technology PrayerClarity mechanics-body `max_width` remains `360`.
- Requirement progression remains horizontal and unchanged.
- Effect/reward/crafting-location grammar remains unchanged.
- Prayer-item tooltip grammar and width remain unchanged.
- Pulpit and Temporary Effects remain unchanged.
- No prayer mechanics or balance changes.
- No global tooltip positioning changes.

## Localization

The comparative-only success heading is added to all 11 supported interface locales:

`en`, `fr`, `de`, `zh_cn`, `es`, `pt_br`, `ko`, `ja`, `ru`, `it`, `pl`.

## Requested runtime check

At 2560x1440 inspect at minimum:

1. Repose / Donations / Combo: Faith and Donations tiers should read as explicit `quality: value` pairs without an orphaned Gold star.
2. Imagination or another duration prayer: all three duration values should read naturally on one row.
3. Prosperity: reward quantity/effect block should remain unchanged and readable.
4. One prayer-item tooltip: confirm no presentation regression.
5. With `Gamepad Tooltip Position Fix` enabled, confirm controller Technology tooltip remains readable; PrayerClarity itself does not reposition it.

If 2560x1440 is clean, perform a 1920x1080 smoke test before stable acceptance.

Stable promotion still requires explicit user runtime acceptance. Any handed 1.0.6 DLL must be tied to a frozen `candidate/1.0.6` source ref.
