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

## PrayerClarity Test Harness 0.1.0 — pulpit UX test helper

- Type: research/test-only helper; **not production mod code and not a release candidate**.
- Purpose: let the user preview all verified player prayer families and Bronze/Silver/Gold variants through the real pulpit UI without owning those prayers on the current save.
- Source branch: `research/pulpit-test-harness`
- Frozen source ref: `candidate/test-harness-0.1.0`
- Exact build source SHA: `8f35f0ed767a3e9360940672f9023463c822f8cc`
- GitHub Actions run: `34862724352`
- GitHub artifact id: `10356081669`
- Build result: success on `ubuntu-latest`; `net472`, AnyCPU.
- Handoff filename: `PrayerClarity.TestHarness.0.1.0.dll`
- Handoff DLL SHA-256: `e4f4054fe2105698525b1fa87bee89f12442621f9b46b284b607ddc4ab1d4c1a`
- Runtime contract: requires the PrayerClarity 0.1.0 candidate; creates only in-memory synthetic prayer `Item` instances and passes them through the stock `PrayCraftGUI.OnResourcePickerClosed(Item)` selection seam. It does not grant prayer items, unlock technologies, or intentionally write player/save/world state.
- Safety gate: while a synthetic preview is selected, `PrayCraftGUI.OnPrayButtonPressed` is blocked so the synthetic item cannot be used to perform a sermon.
- Control surface: BepInEx Configuration Manager `PrayerClarity Test Harness` -> `Preview` -> `Prayer` / `Quality`. No `Update()` polling loop is used; preview refresh is driven by pulpit open and configuration change events.
- Required user action: keep PrayerClarity 0.1.0 installed, install this helper beside it, open the pulpit, select prayer/quality from Configuration Manager, inspect the resulting Clarity UI, and return screenshots. Remove the helper after UX testing.
- Status: **ready for user UX test**.
