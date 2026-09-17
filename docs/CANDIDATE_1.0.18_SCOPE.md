# PrayerClarity 1.0.18 — release gate

## Purpose

1.0.18 is the final Clarity-only release gate for Graveyard Keeper 1.407 before promotion to `main`, GitHub Release, and Nexus packaging.

The visual/UX behavior is inherited from the user-verified 1.0.16/1.0.17 line. This candidate does not redesign the accepted tooltip or pulpit presentation.

## Runtime change from 1.0.17

The remaining `Craft for sermon not found` diagnostic flood was traced to a second non-prayer `linked_craft` lookup in the Technology tooltip discovery path.

Both relevant paths now reject ordinary item IDs through the verified prayer-family allowlist before reading `ItemDefinition.linked_craft`:

- prayer item tooltip discovery;
- Technology output/multiquality prayer discovery.

This is a maintenance/performance/diagnostic cleanup only. Prayer mechanics and displayed values are unchanged.

## Release hygiene

Before freezing this candidate:

- plugin and project version metadata are set to 1.0.18;
- the production build excludes completed diagnostic probes;
- completed probe workflow/project/source files are removed from the stable-bound branch while their durable findings remain in documentation and Git history;
- the public README describes the current tier-first/content-driven Technology presentation and stable installation paths;
- CI stages a versioned handoff DLL, the byte-identical canonical `PrayerClarity.dll`, and a Nexus-ready archive containing `BepInEx/plugins/PrayerClarity/PrayerClarity.dll`;
- all 11 embedded localization resources remain part of the build gate.

## Required runtime gate

A full visual regression pass is not required because the presentation code is unchanged from the already verified line.

Required user check:

1. start a fresh game session with only the new PrayerClarity build replacing the prior PrayerClarity DLL;
2. open Inventory and hover at least one ordinary non-prayer item;
3. open Technology and inspect ordinary technologies plus at least one real prayer technology;
4. confirm the prayer tooltip still renders correctly;
5. provide the resulting `BepInEx/LogOutput.log`.

Acceptance condition: PrayerClarity 1.0.18 loads without PrayerClarity exceptions, real prayer tooltips still render, and the previous mass `Craft for sermon not found` / `pray:<ordinary item id>` flood is absent from the exercised Inventory/Technology scenario.

## Promotion after acceptance

If the runtime gate passes and the user accepts 1.0.18 as the real release, promote the exact accepted source/binary without rebuilding different bytes under the same version:

- move stable `main` to the accepted source state;
- create/update the frozen accepted ref for 1.0.18;
- publish tag/release `v1.0.18` with the exact canonical DLL bytes from the accepted CI artifact;
- use the exact CI-produced Nexus ZIP for Nexus distribution;
- record hashes and acceptance in the durable release/test documentation.
