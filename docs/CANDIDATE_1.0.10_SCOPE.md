# PrayerClarity 1.0.10 — Technology tooltip tier-first UX candidate

Status: **runtime UX candidate scope; not accepted**.

Accepted stable mainline baseline remains PrayerClarity 1.0.9 at runtime/source SHA `4b86b972baeef19aa656a9e414891405006cf67f`.

This candidate changes only the **Technology prayer tooltip presentation**. It does not change prayer mechanics, pulpit behavior, prayer-item tooltips, Temporary Effects, the accepted Technology ownership/viewport-clamp architecture, failed-sermon donation policy, Vanilla Fixes, or Balance/Rework values.

## UX problem being tested

The accepted 1.0.9 property-first renderer is mechanically accurate, but a direct usability review showed that a reader can still fail to understand compact expressions such as `+25% +1` even while looking at the Faith heading. The same review found `Требуется: 10` ambiguous and the overall hierarchy visually dense.

The design hypothesis is recorded in `docs/TECHNOLOGY_TOOLTIP_UX.md`.

## Candidate presentation

Technology comparison now uses:

`shared/invariant mechanics -> Bronze snapshot -> Silver snapshot -> Gold snapshot`

Rules:

- native `(s1)/(s2)/(s3)` quality glyph is the tier heading; no redundant Bronze/Silver/Gold word;
- each tier begins with the localized equivalent of `(s1) Для 100% успеха требуется 10 (cross)`;
- proportional modifiers and flat success-only additions are never concatenated into an unexplained `% + flat` arithmetic expression;
- shared proportional modifiers appear once above the tier snapshots;
- tier-dependent proportional modifiers live inside the relevant tier;
- fixed Faith/money outputs and a pure physical reward are grouped in the relevant tier;
- invariant special effects appear once; tier-dependent duration stays with each tier;
- the Commercial Blessing purpose paragraph is omitted from the Technology comparison body while reward identity/count remain visible;
- blank-line rhythm separates shared mechanics and tier snapshots.

## Width policy

The old local `max_width = 360` remains only as a fail-safe fallback.

When the active `UIRoot` geometry is available, the Technology body receives a content-driven maximum derived from the current safe-area width (`72%` of logical safe width). The native bubble should therefore use only the width its content actually needs, while having substantially more room before wrapping coherent tier rows.

The already accepted PrayerClarity-owned viewport clamp remains the final safety layer and still runs only for marked prayer Technology bubbles.

## Localization

The new guaranteed-success sentence is supplied in all 11 supported locales:

`en`, `fr`, `de`, `zh_cn`, `es`, `pt_br`, `ko`, `ja`, `ru`, `it`, `pl`.

## Build evidence

- Development branch: `dev/technology-tooltip-ux-1.0.10`.
- Frozen candidate ref: `candidate/1.0.10`.
- Exact build/source SHA: `f21674f0ddcc7e06a7d0d1b587faa57e2e5c0b1d`.
- GitHub Actions run: `35160674466`.
- Workflow result: **success** on `ubuntu-latest`; restore, `net472` Release build, all 11 embedded-locale markers, artifact staging and upload passed.
- Artifact ID: `10473296768` (`PrayerClarity-1.0.10-ci-f21674f0ddcc7e06a7d0d1b587faa57e2e5c0b1d`).
- Artifact ZIP digest: `sha256:31dc000d9e1d70ba00d6bc93c7d56115eb27a11c97d725b0e693e07c5a7bbbab`.
- Handoff filename: `PrayerClarity-1.0.10-ci.dll`.
- Handoff DLL SHA-256: `10c04409124914529c962017a7829eecd4fc1e1c2ae376f0230aaa06446c2ff7`.

The frozen candidate ref remains on the exact source used for the build; later documentation-only commits on the development branch do not change candidate identity.

## Runtime acceptance test

Primary visual target: **Prayer for Prosperity** in Technology.

Check that:

1. shared Faith/Donations percentages appear once;
2. each quality glyph, guaranteed-success requirement and its tier outputs read as one coherent snapshot;
3. `+25% +1` ambiguity is gone;
4. Commercial Blessing quantity remains clear without the item-purpose paragraph;
5. normal rows do not wrap merely because of the former 360-width cap;
6. tooltip remains inside the viewport.

Then sample:

- Faith or Donations — one varying proportional modifier plus invariant modifier;
- Combo — both proportional modifiers vary by tier;
- Shoots & Roots — invariant stock-effect warning plus tier-dependent duration.

At least Russian should be checked at the user's normal resolution. If convenient, one long Latin locale (German is preferred) and one CJK locale (Japanese is preferred) should be spot-checked for structural wrapping. Ordinary non-prayer Technology tooltips should remain vanilla/unmodified.

No sermon execution is required.

## Acceptance boundary

Do not promote this candidate to `main` merely because it builds. The tier-first hierarchy, adaptive width and resulting visual rhythm require explicit runtime acceptance.
