# PrayerClarity 1.0.18 — accepted stable release

Accepted on 2026-09-17 after user runtime verification in Graveyard Keeper 1.407.

## Exact accepted source

- Candidate ref: `candidate/1.0.18`
- Accepted ref: `accepted/clarity-1.0.18`
- Runtime/source SHA: `2e7d2fbac412eb74ea43bcc60bb1fc35bc79b15e`
- CI run: `35203942368`
- CI artifact ID: `10489550250`
- CI artifact ZIP digest: `sha256:3c763659bb4e86d2c3a2bd08485c205f23e9ee25d690995065114159eb94d6ce`

## Release bytes

- Canonical `PrayerClarity.dll` SHA-256: `4a3acce006713602783a7a26de48134c07b96d7e0fbe43a846397c5e822e06ee`
- Runtime handoff DLL (`PrayerClarity-1.0.18-ci.dll`) SHA-256: `4a3acce006713602783a7a26de48134c07b96d7e0fbe43a846397c5e822e06ee`
- Nexus-ready ZIP SHA-256: `d30638f8ea9e61a8ac0d0df834acf9fba4b0d79c1f33885ea7e55d71da3cce1f`
- The canonical DLL, runtime handoff DLL, and DLL inside the Nexus package were staged from the same build output; no post-test rebuild is permitted under version 1.0.18.

## Runtime acceptance evidence

The user confirmed all PrayerClarity surfaces visually correct before the final maintenance gate. The final 1.0.18 log then confirmed:

- `PrayerClarity 1.0.18` loaded successfully;
- Inventory and Technology UI paths were exercised;
- real prayer entries were visited in Technology;
- the prior mass ordinary-item `Craft for sermon not found` flood was absent;
- no PrayerClarity initialization, forecast, Technology-width, viewport, secondary-surface, or prayer-item fallback/error message was present.

The installed research Test Harness plugins were not part of the 1.0.18 release artifact.

## Scope

1.0.18 is Clarity-only. It changes prayer information presentation and maintenance behavior only; it does not rebalance prayers, repair vanilla prayer mechanics, or alter sermon payouts.

Future Vanilla Fixes and PrayerClarity: Rebalance work remain separate from this accepted stable source.
