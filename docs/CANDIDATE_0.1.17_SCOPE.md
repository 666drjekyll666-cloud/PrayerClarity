# PrayerClarity 0.1.17 candidate scope

Status: clean-built runtime candidate awaiting user in-game acceptance, 2026-09-15.

0.1.17 is Clarity-only. It does not intentionally change prayer mechanics, success probability, sermon rewards, buff magnitude, save data or balance.

## Frozen build

- Frozen ref: `candidate/0.1.17`.
- Exact source SHA: `9fe15128bb3590d988eae60977ded4b8230019b1`.
- GitHub Actions run: `34996594076`.
- Workflow result: success on `ubuntu-latest`; restore, `net472` build, all 11 embedded-locale markers, artifact staging and upload passed.
- Artifact ID: `10408121638` (`PrayerClarity-0.1.17-ci-9fe15128bb3590d988eae60977ded4b8230019b1`).
- Handoff DLL SHA-256: `596ec83429ddf12003eb2ac2e355df8fce3f933033e0cbac3585141b35c78ca5`.
- Supported verified game MVID: `6f50b8e7-156b-49ac-bbe8-7505894b2364`.

## Why 0.1.16 was rejected

Runtime testing confirmed that the 0.1.16 presentation was mechanically correct but visually hard to parse:

- pulpit dependency text mixed duplicated nouns/icons, multiple colons and source relationships into one compact expression;
- `+N% of base +N` introduced an internal term (`base`) that the player had not been taught;
- Combo became visibly smaller because the single result label still used shrink-to-fit behavior for a longer inline formula;
- the Temporary Effects strategic day count rendered as bare numbers because the stock timer font could not reliably display the localized suffix/approximation glyph;
- Technology removed the vanilla duplication successfully, but inherited the same `base reward` / `% of base` wording problem.

## 0.1.17 presentation model

### Pulpit

Use hierarchy instead of one dense formula:

- `Always:`
  - Faith depends on Church Quality;
  - Donations depend on Graveyard Quality;
- `Success (N%) adds:`
  - prayer-owned Faith / donation modifiers and fixed additions;
- `Effect:` exact intrinsic prayer effect.

The exact sermon payout remains hidden until the ceremony.

Faith and donation modifiers use a spaced vertical bar (` | `) only when both resource groups share one compact value line. The previous centered dot is not used because it can read as a multiplication operator in numeric expressions. Dependency relationships use line breaks instead of punctuation separators.

The result label uses vertical growth instead of shrink-to-fit so Combo does not become smaller than other prayers. The effect row is moved down only as far as needed to clear the expanded result block.

### Technology

Keep the successful 0.1.16 removal of duplicated vanilla mechanics, but remove the separate `Base reward` explanation entirely.

Each tier is presented as a small scanable block:

- quality + Church Quality requirement;
- `Added on success:` prayer-owned Faith/donation modifiers;
- `Effect:` when the tier has an intrinsic special effect.

No `% of base` wording is used. Native quality-star icons remain deferred until a verified low-cost inline icon seam exists.

### Temporary Effects

Probe 0.1.8 remains the timer source of truth: `PlayerBuff.end_time - MainGame.game_time` is remaining in-game days.

For prayer buffs with at least one in-game day remaining:

- append the rounded day count to the normal description label, whose font supports ordinary localized prose;
- suppress the stock compact timer text for that period.

Below one in-game day, stop appending the strategic day count and retain the vanilla precise timer.

This avoids parsing timer text and does not depend directly on Longer Days.

## Requested runtime check

1. Pulpit: Donations and Combo. Confirm the new hierarchy is immediately understandable, Combo no longer shrinks, and the ` | ` divider reads as separation rather than arithmetic.
2. Technology: inspect Faith/Combo. Confirm there is no duplicate vanilla mechanic block, no `base reward` wording, and Bronze/Silver/Gold remain easy to scan.
3. Temporary Effects: activate Protection and Soul Contentment/another long prayer through Test Harness 0.1.4. Confirm normal descriptions include a localized day count (for example `+4 armor · 3.2 days`) and the bare-number timer is gone while >=1 day remains.
4. Report overlap/clipping, raw localization IDs, incorrect prayer mapping, or any PrayerClarity exception in the log.
