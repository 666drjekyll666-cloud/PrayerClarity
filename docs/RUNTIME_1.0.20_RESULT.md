# PrayerClarity 1.0.20 runtime result

Date: 2026-09-17

## Identity

- Frozen candidate ref: `candidate/1.0.20`
- Accepted runtime ref: `accepted/clarity-1.0.20`
- Exact runtime source SHA: `c7ac91c1cea6c498fb406323725768b605d8139f`
- GitHub Actions run: `35210530177`
- Workflow artifact ID: `10490619751`
- Artifact ZIP digest: `sha256:eb04f58888628caa0ba530e82b6180f0b41246819cbf4b6fac3c9513ac742e40`
- Handoff DLL: `PrayerClarity-1.0.20-ci.dll`
- DLL SHA-256: `fcf96c2c2c71f9dbc7f17646a91a5c44aadc7a210be0f3ef8ea9850411a21ffc`

## Runtime evidence

The user tested the exact 1.0.20 candidate in Graveyard Keeper 1.407 with the normal mod set and reported the Technology prayer tooltip presentation as working correctly.

Screenshots covered Japanese, Simplified Chinese, Korean and German Prayer for Repose Technology tooltips. The previously confirmed Polish localization defect was no longer present in the tested build, and the user considered the presentation correct overall.

Cross-language review found no systemic German anomaly. German `erforderlich` after the church-quality icon means `required` and is part of normal German word order. Equivalent post-icon requirement wording also exists in Japanese (`が必要`) and Korean (`필요`); Simplified Chinese expresses the same relation before the icon. German `Spenden` is the Donations label, corresponding to Japanese `寄付金`, Simplified Chinese `捐款`, and Korean `기부금`.

The supplied runtime log confirms `PrayerClarity 1.0.20` loaded and contains no PrayerClarity warning or error entries.

## Accepted result

**Accepted.** The localization-robustness hotfix replaces punctuation-dependent stripping with the verified base vanilla prayer lore key seam and passes the runtime gate. No prayer mechanics, balance, pulpit behavior, prayer-item tooltip behavior, Temporary Effects behavior, Technology tier grouping, values, spacing, or viewport policy changed.

This acceptance does not by itself publish a new GitHub or Nexus release. The existing public release remains unchanged until a separate release decision is made.
