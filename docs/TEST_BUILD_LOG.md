# Test / Research Build Log

This file records handed executable artifacts once PrayerClarity research reaches a point where the user's installed Graveyard Keeper 1.407 runtime must provide evidence.

## PrayerClarity 0.1.3 — aligned pulpit presentation candidate

- Type: Clarity-only presentation candidate; no intended prayer-mechanics or balance changes.
- Purpose: validate the 0.1.2 information model with a less dense, fixed-column pulpit layout and stable font sizing.
- Source branch: `dev/clarity-pulpit-v0.1`.
- Frozen candidate ref: `candidate/0.1.3`.
- Exact build source SHA: `dbbb6d26b2b87ee46819984e6ae50c60b44328e0`.
- GitHub Actions run: `34877867872`.
- Workflow result: success on `ubuntu-latest`; restore, `net472` build, all 11 embedded-locale markers, artifact staging and upload passed.
- Workflow artifact ID: `10360339790` (`PrayerClarity-0.1.3-ci-dbbb6d26b2b87ee46819984e6ae50c60b44328e0`).
- Handoff filename: `PrayerClarity-0.1.3-ci.dll`.
- Handoff DLL SHA-256: `e24b93a6e70c69f6c440da08d7f2bdf91c9d6bafc2a979cd6307d0ae7092066c`.
- Presentation change: replace the single forecast UILabel with separate labels for vanilla context, row labels, Faith, money and the special effect. Repeated resources use fixed columns; special-effect text uses height growth rather than shrinking the whole forecast.
- Icon policy in this candidate: preserve native `(faith)` and `Trading.FormatMoney` rendering; for special buffs/rewards, resolve the verified native icon name lazily and cache the resulting sprite for the session, with text-only fallback if no useful sprite resolves.
- Requested runtime test: keep Test Harness 0.1.0, replace only the PrayerClarity production prototype DLL, then inspect Ordinary, Combo/Donations, Retribution and Thorough Cleansing (plus Repose if convenient). Verify fixed resource alignment, readable unchanged font size, separation between context/results, absence of overlap with prayer slot/buttons, and whether native special-effect icons resolve cleanly.
- Status: **ready for runtime UX verification; not accepted**.

## PrayerClarity 0.1.2 — runtime result: information model retained, single-label layout rejected

- Type: Clarity-only presentation candidate; no intended prayer-mechanics or balance changes.
- Purpose: test the `guaranteed + success bonus` information model in the live pulpit after the 0.1.1 forecast seam was proved.
- Frozen candidate ref: `candidate/0.1.2`.
- Exact source SHA: `12da4d081aaf0dac11c8b2e528daf441af3e12ac`.
- GitHub Actions run: `34867338643`.
- Artifact ID: `10356689583`.
- Handoff DLL SHA-256: `bf02d1035664c798967ebcf06a376af7f8e877e3f11d1673fc92e0de5c12c167`.
- Runtime evidence, 2026-09-14: the user exercised ordinary, Combo, Donations, Retribution and Thorough Cleansing examples through the real pulpit/Test Harness. Forecast values rendered and updated correctly.
- UX result: the semantic split into guaranteed base output and success-only additions is useful and should be retained. The single-label layout is **not accepted**: resource positions move between rows, the context and forecast read as one dense block, and long special-effect text causes NGUI `ShrinkContent` to reduce the font size of the entire label.
- Next narrow candidate: separate context from results, align repeated resources into fixed columns, render the special effect in its own row, preserve normal font size, and test native cached icons where verified.
- Status: **superseded for presentation; information model retained**.

## PrayerClarity 0.1.1 — runtime result: forecast path verified, presentation not accepted

- Type: narrow follow-up production-architecture candidate; **Clarity only**, with no intended prayer-mechanics or balance changes.
- Purpose: prove that the 0.1.0 pulpit forecast architecture works once the reflection resolver correctly closes the game's generic `GameBalance.GetData*<T>(string)` seam.
- Source branch: `dev/clarity-pulpit-v0.1`.
- Frozen candidate ref: `candidate/0.1.1`.
- Exact source SHA: `bfa437a729cedebdad8c787fd663edfa5ed51630`.
- GitHub Actions run: `34864837824`.
- Workflow result: success on `ubuntu-latest`; restore, `net472` build, all 11 embedded-locale markers, artifact staging and upload passed.
- Workflow artifact ID: `10355544046` (`PrayerClarity-0.1.1-ci-bfa437a729cedebdad8c787fd663edfa5ed51630`).
- Handoff filename: `PrayerClarity-0.1.1-ci.dll`.
- Handoff DLL SHA-256: `f86dc5710b79c4f7db081f97a634f59ab25831ba964d856e8999d67006228987`.
- Change from 0.1.0: `R.BalanceData` resolves a one-string-parameter balance getter against the expected result type; it uses a compatible closed overload when present, otherwise binds the verified one-generic-argument method definition with `MakeGenericMethod(expectedType)` before invocation. Callers explicitly request `PrayEventDefinition` or `BuffDefinition`.
- Runtime evidence, 2026-09-14: forecast output now renders successfully in the live 1.407 pulpit UI and updates while the Test Harness switches prayer families and qualities. The supplied runtime log contains no recurring `PrayerClarity forecast failed` error for 0.1.1 and confirms synthetic selection across ordinary, Faith, Donations, Repose, Excellence and Roots examples without save/inventory mutation.
- UX result: current append-only presentation is **not accepted**. Vanilla `Church quality / Sermon requires / Success chance` remains useful, but `Success | Failure` on one line is visually poor and separates probability from its corresponding outcome.
- Status: **forecast/runtime seam verified; superseded for UX iteration, not accepted for stable**.

## PrayerClarity 0.1.0 — runtime result: superseded

- Type: first production-architecture prototype; **Clarity only**, with no intended prayer-mechanics or balance changes.
- Purpose: validate the minimal pulpit decision-time surface before implementing broader tooltip/HUD layers or any Vanilla Fix / Balance-Rework behavior.
- Source branch: `dev/clarity-pulpit-v0.1`.
- Frozen candidate ref: `candidate/0.1.0`.
- Exact source SHA: `6321da4ca868998c166a115d77d2d634e372ffde`.
- GitHub Actions run: `34860993438`.
- Workflow result: success on `ubuntu-latest`; `net472`, all 11 embedded-locale markers, artifact staging and upload passed.
- GitHub Actions artifact ID: `10353769580` (`PrayerClarity-0.1.0-ci-6321da4ca868998c166a115d77d2d634e372ffde`).
- Handoff filename: `PrayerClarity-0.1.0-ci.dll`.
- Handoff DLL SHA-256: `dc47501e5a9d444309954f4b2a58c230f2bc1c8a725c1163eb43944576b6c949`.
- Supported target identity: Graveyard Keeper 1.407 Assembly-CSharp MVID `6f50b8e7-156b-49ac-bbe8-7505894b2364`; other MVIDs fail closed before patching.
- Localization contract: English, French, German, Simplified Chinese, Spanish (Spain), Portuguese (Brazil), Korean, Japanese, Russian, Italian and Polish are embedded in the DLL; current game language is resolved on the relevant UI redraw with English fallback.
- Runtime architecture: one postfix on `PrayCraftGUI.RedrawTextValues(float,float)`; no polling, no Unity-wide scans and no call to `PrayLogics.CalculatePray` for preview. Base Faith/money are intended to be read through the game's own `SmartExpression.EvaluateFloat`; known special effects are read from verified game definitions/semantics.
- Fail-safe behavior: a forecast error is logged once and vanilla pulpit UI remains usable.
- Runtime evidence, 2026-09-14: the test harness successfully switched synthetic prayer families/qualities in the live pulpit UI without granting them to the save. The visible `Church quality / Sermon requires / Success chance` block was confirmed to be vanilla UI. PrayerClarity's own forecast did **not** render because `R.BalanceData` selected an open generic `GameBalance.GetData*` overload and late-bound invocation failed with `ContainsGenericParameters=true`. The plugin failed safe and left the vanilla pulpit usable; no prayer-mechanics change was observed.
- Status: **superseded / not accepted**.

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
