# PrayerClarity 1.0.0 — accepted release evidence

Status: **accepted / stable**.

## Identity

- Version: `1.0.0`
- Exact runtime/source SHA: `493d2168489af80b5c1305f7ff1435e2ee1dd0d7`
- Frozen candidate ref: `candidate/1.0.0`
- Frozen accepted ref: `accepted/clarity-1.0.0`
- GitHub Actions run: `35035167380`
- Workflow artifact ID: `10423032814`
- Artifact: `PrayerClarity-1.0.0-ci-493d2168489af80b5c1305f7ff1435e2ee1dd0d7`
- Artifact ZIP digest: `sha256:70ca68b6be93234b1046a53cb4f76bd914258557eef9b77513be762672a23c1c`
- Candidate DLL: `PrayerClarity-1.0.0-ci.dll`
- DLL SHA-256: `dcf0338af520fe70989e6866bc6a724cbb3976337d7c9489c2ff9886e1e98580`
- Supported Assembly-CSharp MVID: `6f50b8e7-156b-49ac-bbe8-7505894b2364`

The stable GitHub Release must publish these exact DLL bytes under the canonical installed filename `PrayerClarity.dll`; a filename-only rename does not change the hash.

## Build gate

Run `35035167380` completed successfully on `ubuntu-latest`:

- restore succeeded;
- Release `net472` build succeeded;
- compiler reported 0 warnings and 0 errors;
- all 11 embedded locale markers were verified;
- the immutable candidate artifact above was uploaded from the exact accepted source SHA.

No rebuild is required or permitted for the 1.0.0 stable asset while the accepted artifact remains available.

## Final runtime acceptance

On 2026-09-16 the user tested the exact 1.0.0 candidate in Graveyard Keeper 1.407 and explicitly approved it for final/stable promotion.

The supplied runtime evidence confirms:

- `PrayerClarity 1.0.0` loaded successfully;
- the game was Graveyard Keeper 1.407;
- the final test included pulpit prayer switching, representative timed-buff activation, Character -> Temporary Effects, and Technology-tree inspection;
- Configuration Manager / plugin summary reported `PrayerClarity` with **no config entries**, confirming removal of the prototype layout controls from the release build;
- no PrayerClarity runtime error was reported in the tested sequence.

User verdict: all checked presentation and Configuration Manager behavior were correct; 1.0.0 was explicitly accepted as the final version.

## Product scope

PrayerClarity 1.0.0 is **Clarity-only**. It changes presentation, not prayer balance or gameplay mechanics. Future Vanilla Fixes and Balance/Rework work remain separate from this accepted baseline.
