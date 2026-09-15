# PrayerClarity 0.1.15 candidate scope

Status: clean-built runtime candidate awaiting user in-game acceptance, 2026-09-15.

0.1.15 is Clarity-only. It does not intentionally change prayer mechanics, success probability, rewards, buffs, save data or balance.

## Frozen build

- Frozen ref: `candidate/0.1.15`.
- Exact source SHA: `003181f67406e726862838ebec53619916e76bb7`.
- GitHub Actions run: `34975245461`.
- Workflow result: success on `ubuntu-latest`; restore, `net472` build, 11 embedded-locale checks, artifact staging and upload all passed.
- Artifact ID: `10398729202` (`PrayerClarity-0.1.15-ci-003181f67406e726862838ebec53619916e76bb7`).
- Handoff DLL SHA-256: `6f2ea61516ccd54416b4e04b6ffb30ab65fe4e5a4b6b5f28841c4ac9f18c2faa`.
- Supported verified game MVID: `6f50b8e7-156b-49ac-bbe8-7505894b2364`.

## Runtime scope

- pulpit uses the accepted reward-reveal model: base reward dependencies + prayer-owned success modifiers + exact intrinsic effects, without exact pre-sermon Faith/donation totals;
- the redundant lower pulpit dependency note remains hidden;
- Character -> Temporary Effects replaces vanilla prayer-buff flavor/opaque descriptions with verified exact effect meaning while preserving the vanilla timer;
- Technology prayer tooltips append prayer-quality rows with exact Church Quality requirements, prayer-owned Faith/donation modifiers/fixed outputs, and exact verified special effects/durations;
- all new PrayerClarity-owned player-facing copy is present in the same 11 interface locales supported by the project;
- no per-frame polling or broad UI scan is introduced.

## Requested runtime acceptance check

1. At the pulpit, inspect Faith, Donations, Combo, Prosperity and Soul's Repose. Confirm `Guaranteed` explains reward sources, `On success (N%)` shows prayer-owned modifiers/fixed outputs, and no exact final Faith/donation payout is shown before the ceremony. Confirm the old lower dependency note is absent.
2. In Character -> Temporary Effects, inspect several prayer buffs. Confirm the description states the exact verified effect while the vanilla remaining-time timer still updates normally. Useful representatives are Retribution, Protection, Repose, Imagination/Excellence, Roots or Repentance, Soul Contentment and Thorough Cleansing when available.
3. In Technologies, hover prayer-related unlocks. Confirm the appended `Prayer details` section shows Bronze/Silver/Gold rows with Church Quality requirement, prayer-owned resource modifiers/fixed outputs and exact special effect/duration, without a final sermon payout.
4. Report any clipping, duplicate text, missing rows, raw localization IDs or incorrect prayer-to-effect mapping. A screenshot of each of the three surfaces is sufficient for the first acceptance pass.

Runtime acceptance is still required before these presentation changes move to the stable line.
