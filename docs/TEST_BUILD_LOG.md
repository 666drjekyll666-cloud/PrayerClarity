# Test / Research Build Log

This file records handed executable artifacts once PrayerClarity research reaches a point where the user's installed Graveyard Keeper 1.407 runtime must provide evidence.

## PrayerClarity Audit Probe 0.1.0 — pending runtime evidence

- Type: research-only, read-only diagnostic probe; **not** production mod code.
- Purpose: close static-evidence gaps around the final `PrayResult` consumer, prayer buff application/duration, passive buff consumers, and exact prayer-selection/report presentation.
- Source branch: `research/initial-audit`
- Exact source SHA: `d0d42c5011e4f909d9dddeda876c0bed7e9cfab2`
- GitHub Actions run: `34824714263`
- Build result: success on `ubuntu-latest`; `net472`, AnyCPU.
- Handoff filename: `PrayerClarity.AuditProbe.0.1.0.dll`
- Handoff DLL SHA-256: `3f4a5758541b22b9ccfa695142e27eadf58fdb2489491eb6faca1dbeb1d0bc49`
- Runtime contract: reflection / IL inspection only; no Harmony patches and no intended save/player/world mutation.
- Expected evidence file: `BepInEx/PrayerClarity-audit-0.1.0.txt`.
- Required user action: install the probe DLL in `BepInEx/plugins`, launch Graveyard Keeper 1.407 far enough for BepInEx/game assemblies to load, then return the generated text file. No sermon execution or save mutation is required.
- Status: **pending user runtime capture**.

Do not treat this probe as a release candidate or production architecture. Once its narrow questions are answered, durable derived facts belong in `docs/PRAYER_MECHANICS.md` and the probe should not become a permanent runtime dependency.
