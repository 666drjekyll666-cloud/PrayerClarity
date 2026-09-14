# Test / Research Build Log

This file records handed executable artifacts once PrayerClarity research reaches a point where the user's installed Graveyard Keeper 1.407 runtime must provide evidence.

## PrayerClarity 0.1.0 — ready for pulpit UX runtime test

- Type: first production-architecture prototype; **Clarity only**, with no intended prayer-mechanics or balance changes.
- Purpose: validate the minimal pulpit decision-time surface before implementing broader tooltip/HUD layers or any Vanilla Fix / Balance-Rework behavior.
- Source branch: `dev/clarity-pulpit-v0.1`.
- Frozen candidate ref: `candidate/0.1.0`.
- Exact source SHA: `6321da4ca868998c166a115d77d2d634e372ffde`.
- GitHub Actions run: `34860993438`.
- Workflow result: success on `ubuntu-latest`; restore, `net472` build, all 11 embedded-locale markers, artifact staging and upload passed.
- Workflow artifact ID: `10353769580` (`PrayerClarity-0.1.0-ci-6321da4ca868998c166a115d77d2d634e372ffde`).
- Handoff filename: `PrayerClarity-0.1.0-ci.dll`.
- Handoff DLL SHA-256: `dc47501e5a9d444309954f4b2a58c230f2bc1c8a725c1163eb43944576b6c949`.
- Supported target identity: Graveyard Keeper 1.407 Assembly-CSharp MVID `6f50b8e7-156b-49ac-bbe8-7505894b2364`; other MVIDs fail closed before patching.
- Localization contract: English, French, German, Simplified Chinese, Spanish (Spain), Portuguese (Brazil), Korean, Japanese, Russian, Italian and Polish are embedded in the DLL; current game language is resolved on the relevant UI redraw with English fallback.
- Runtime architecture: one postfix on `PrayCraftGUI.RedrawTextValues(float,float)`; no polling, no Unity-wide scans and no call to `PrayLogics.CalculatePray` for preview. Base Faith/money are read through the game's own `SmartExpression.EvaluateFloat`; known special effects are read from verified game definitions/semantics.
- Fail-safe behavior: a forecast error is logged once and vanilla pulpit UI remains usable.
- Requested user test: install this DLL as the only PrayerClarity production prototype, open the church pulpit and switch among several available prayers without needing to execute a sermon. Check whether the added success/failure outcome line and optional effect/reward line are readable, useful and correctly localized; specifically note clipping/overlap, unexpected values, duplicated text, or a prayer whose explanation is misleading. A screenshot of the pulpit with several representative prayers is sufficient initial evidence.
- Status: **candidate built and frozen; pending user runtime UX evidence**. Do not merge runtime behavior to `main` yet.

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
