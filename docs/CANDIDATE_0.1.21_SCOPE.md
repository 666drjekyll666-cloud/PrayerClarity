# PrayerClarity 0.1.21 — accepted Clarity baseline

Status: **accepted runtime result**.

## Identity

- Version: `0.1.21`
- Candidate ref: `candidate/0.1.21`
- Accepted frozen ref: `accepted/clarity-0.1.21`
- Exact runtime source SHA: `ccfa329813233ea02482808a38bcb483ff844310`
- GitHub Actions run: `35031015516`
- Artifact ID: `10420999780`
- Candidate DLL: `PrayerClarity-0.1.21-ci.dll`
- DLL SHA-256: `bd77adbf36658146132a81452f14753e972e75e893acf05df50b7c04ae3cad06`
- Supported game: Graveyard Keeper `1.407`
- Supported Assembly-CSharp MVID: `6f50b8e7-156b-49ac-bbe8-7505894b2364`

## Accepted scope

This is the final accepted **Clarity-only vanilla UI baseline** before Vanilla Fixes / Balance-Rework work.

It changes presentation only. It does **not** alter prayer rewards, success calculations, temporary-effect mechanics, sermon execution, or any balance value.

Accepted surfaces:

- pulpit pre-sermon information hierarchy;
- Technology prayer details and native bronze/silver/gold quality symbols;
- Character -> Temporary Effects quantitative descriptions;
- in-game-day duration presentation for prayer buffs;
- all 11 supported game UI languages.

The accepted pulpit lower block is conceptually:

`Guaranteed -> On success (N%) -> Effect`

with resource-source lines under Guaranteed and resource-grouped modifiers under On success. The redundant general `Result` heading is intentionally removed.

## Runtime acceptance

The user reported the 0.1.21 Technology tooltip and pulpit day-duration presentation as correct, then tested the final pulpit copy and stated that everything was in order and that this should be fixed as the final UI variant.

This approval authorizes promotion of the exact accepted source to the stable line. No additional UI runtime test is required before proceeding to later mechanics layers.
