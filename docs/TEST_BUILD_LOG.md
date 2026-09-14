# Test / Research Build Log

This file records handed executable artifacts once PrayerClarity research reaches a point where the user's installed Graveyard Keeper 1.407 runtime must provide evidence.

## PrayerClarity 0.1.8 — fixed-window wrapping candidate

- Type: Clarity-only presentation candidate; no intended prayer-mechanics or balance changes.
- Purpose: retain the workable fixed-window composition found by the user in 0.1.7, remove the disproved frame-resize controls, add localization-safe effect wrapping, and repair several misleading/blank special-effect rows.
- Frozen candidate ref: `candidate/0.1.8`.
- Exact build source SHA: `46f5d9954d710a160308e393f2cb1bf91e28e82a`.
- GitHub Actions run: `34899841772`.
- Workflow result: success on `ubuntu-latest`; restore, `net472` build, all 11 embedded-locale markers, artifact staging and upload passed.
- Workflow artifact ID: `10369988505` (`PrayerClarity-0.1.8-ci-46f5d9954d710a160308e393f2cb1bf91e28e82a`).
- Handoff filename: `PrayerClarity-0.1.8-ci.dll`.
- Handoff DLL SHA-256: `8b78d5bf6b3bd395b0b8399e016f6e49b894e08ad6e509f50c4f4da2d5eb9656`.
- Layout baseline: the 0.1.7 user calibration from the second supplied screenshot is now the default starting point at 2560x1440: context `8/72/12`, result `8/20/13`, effect `-122/-35/10` with icon size `10`, note `-6/-87/9`, prayer selector `90/55`, prayer button `0/-120`.
- Removed controls: `Window extra width` and `Window extra height` are gone. Runtime proved they only resize child artwork and do not enlarge the actual pulpit window.
- Config isolation: layout controls now live under `Prototype pulpit layout tuning v3`, so rejected v2 width/height values cannot carry into this candidate.
- Effect wrapping: the mod-owned effect label now uses a fixed width with top-left `ResizeHeight`. Only the new effect label expands downward; the stock center-pivot `l_total_values` widget that caused 0.1.4 cumulative drift is not resized.
- Repose copy: stock Clarity now describes the verified `body_max +1` in terms of the Donkey being able to bring a body one quality tier higher and uses the native inline white-skull symbol. It intentionally does **not** use bronze/silver/gold `possible/likely/guaranteed` language because that belongs to the future Rebalanced mechanics, not stock 1.407.
- Prosperity: Commercial Blessing output now appends the game's own localized description explaining that it can be sold to a merchant to raise that merchant's level.
- BSS Soul's Repose: detection now keys off the verified `pray_for_souls_*` event family, so the vanilla localized Soul Gratitude/Faith explanation should no longer be blank.
- Soul Contentment: copy order is now `Effect: +10% (gratitude_points) ...`; the generic leading buff icon is suppressed to avoid duplicate/ambiguous icon grammar.
- Thorough Cleansing: uses explicit `i_sin_shard`, the Sin Shard art referenced by the stock Sin Shard body-part craft rows. The `sin_shard` ItemDefinition itself has blank icon fields, which explains why the prior generic item-icon fallback failed.
- Balance separation: the user-proposed Soul Contentment `+20/+40/+60%` and Thorough Cleansing `x2/x3/x4` curves are recorded as Rebalanced design hypotheses only. This candidate still reports and preserves stock `+10%` and `x2` behavior.
- Requested user test: replace 0.1.7 with 0.1.8 and first leave the new v3 layout values untouched. Confirm that the pulpit opens close to the accepted second-screenshot composition and that Window width/height controls are absent. Then test Shoots & Roots and Repentance for automatic effect-line wrapping; Repose for clearer stock wording/white-skull cue; Prosperity for the merchant-level explanation; BSS Soul's Repose for a nonblank Effect row; Soul Contentment for `+10%` before the Soul Gratitude icon; and Thorough Cleansing for actual Sin Shard art. A quick Faith/Donations/Combo switch should confirm specialist-arrow semantics remain intact. No sermon execution is required.
- Status: **ready for runtime UX verification; not accepted**.

## PrayerClarity 0.1.7 — runtime result: fixed-window layout useful, frame resize rejected

- Type: Clarity-only presentation/calibration candidate; no intended prayer-mechanics or balance changes.
- Frozen candidate ref: `candidate/0.1.7`.
- Exact build source SHA: `4df3d204866afc39ee0b848c14bb724101a29761`.
- GitHub Actions run: `34895350258`.
- Workflow artifact ID: `10368661089` (`PrayerClarity-0.1.7-ci-4df3d204866afc39ee0b848c14bb724101a29761`).
- Handoff filename: `PrayerClarity-0.1.7-ci.dll`.
- Handoff DLL SHA-256: `3742468ee5fef4190ad631933e4c0da220657e14aab46b3246fd9799ea99d4f2`.
- Runtime evidence, 2026-09-15: the independent Window extra width/height controls still do **not** enlarge the actual pulpit window. They deterministically stretch/move the child background/decor artwork, proving that the targeted child widgets are not the real usable window boundary. The frame-resize approach is rejected rather than iterated again.
- Accepted UX direction: moving the prayer selector into the upper-right creates enough usable space inside the unchanged stock pulpit to fit the forecast. The second supplied screenshot produced a workable calibration baseline: context `8/72/12`, result `8/20/13`, effect `-122/-35/10`, effect icon `10`, note `-6/-87/9`, selector `90/55`, prayer button `0/-120`.
- Accepted arrow semantics: Faith highlights Faith; Donations highlights money; Combo and non-resource specialists show no green up arrow. The user reported Faith/Donations/Combo as reading correctly.
- New presentation failure: Shoots & Roots effect text can run beyond the available horizontal region. This is a localization problem, not a Russian-only string problem, so the next candidate must wrap the effect label automatically rather than insert language-specific hard line breaks.
- Repose finding: the stock text `corpses can be one tier better` is too abstract. A Donkey/body-quality formulation is clearer. However, stock 1.407 exposes the same `body_max +1` special magnitude at every prayer quality, so bronze/silver/gold reliability language must wait for the future Rebalanced implementation.
- BSS Soul's Repose remained blank in the Effect row. Root cause in 0.1.7 code: the special-text path matched a guessed prayer-craft prefix instead of the already-verified `pray_for_souls_*` event family.
- Soul Contentment still read ambiguously because a leading effect icon plus an inline Soul Gratitude icon visually duplicated the noun. The desired grammar is `+10% [Soul Gratitude] from soul healing`.
- Thorough Cleansing still did not show the expected Sin Shard art. Direct balance evidence resolves the icon path: stock Sin Shard body-part crafting rows use `i_sin_shard`, while the `sin_shard` ItemDefinition has blank icon fields.
- Status: **superseded by 0.1.8; fixed-window composition retained, frame resizing rejected**.

## PrayerClarity 0.1.6 — runtime result: live tuning useful, frame/arrow design rejected

- Type: Clarity-only presentation/calibration candidate; no intended prayer-mechanics or balance changes.
- Frozen candidate ref: `candidate/0.1.6`.
- Exact build source SHA: `adf752e1a04c93172bdedc54b913d4b926413c0d`.
- GitHub Actions run: `34891988740`.
- Workflow artifact ID: `10367390730` (`PrayerClarity-0.1.6-ci-adf752e1a04c93172bdedc54b913d4b926413c0d`).
- Handoff filename: `PrayerClarity-0.1.6-ci.dll`.
- Handoff DLL SHA-256: `cd741f905510db6dc968cdf87a0c2ca6f74e507bc9deb29b3bd801b2685e0a8d`.
- Runtime evidence, 2026-09-14: PrayerClarity 0.1.6 loaded successfully in Graveyard Keeper 1.407 at 2560x1440 alongside Test Harness 0.1.0 and the user's normal mod set. The supplied log contains no reported PrayerClarity forecast exception while the harness switched repeatedly across prayer families/qualities.
- Accepted evidence: live Configuration Manager changes apply while the pulpit is open, so runtime calibration is a better iteration tool than rebuild-per-coordinate guessing. The earlier cumulative prayer-switch drift did not return.
- Rejected frame behavior: `Window extra height` was not coherent. Increasing it could make the pulpit explode horizontally, and decorative/frame parts appeared to jump or move independently. This invalidates the 0.1.6 frame-resize implementation, not the general idea of live calibration.
- Rejected arrow grammar: one `(up)` before the whole success-bonus group reads as if it modifies the first resource. This is misleading on Donations, where Faith is printed first but money is the specialization. Ordinary and non-resource-specialist prayers also do not benefit from an up-arrow cue.
- Additional UX findings: Effect X needed a much wider negative range; the prayer selector and prayer button also need live X/Y controls; Soul's Repose had no special-effect explanation; Soul Contentment's `+10%` lacked an obvious noun; Thorough Cleansing did not present the intended Sin Shard icon.
- Context/dependency direction remains useful: removing `Контекст проповеди` reduced clutter, and icon-first source relationships remain preferable.
- Status: **superseded by 0.1.7; not accepted for stable**.

## PrayerClarity 0.1.5 — runtime result: stable geometry, layout rejected

- Type: Clarity-only presentation candidate; no intended prayer-mechanics or balance changes.
- Purpose: replace the rejected 0.1.4 single expanding label with a layout derived from measured stock pulpit geometry, while retaining the accepted `guaranteed + success bonus + special effect` information model.
- Frozen candidate ref: `candidate/0.1.5`.
- Exact build source SHA: `91cc03b7e1902814d651565ffd591e81338ae4f9`.
- GitHub Actions run: `34888807911`.
- Workflow result: success on `ubuntu-latest`; restore, `net472` build, all 11 embedded-locale markers, artifact staging and upload passed.
- Workflow artifact ID: `10366245690` (`PrayerClarity-0.1.5-ci-91cc03b7e1902814d651565ffd591e81338ae4f9`).
- Handoff filename: `PrayerClarity-0.1.5-ci.dll`.
- Handoff DLL SHA-256: `5f5684dac333683bbf6a547ee4f4d97b5df81b0d25faf4dad7555d3b668b58fb`.
- Geometry evidence: the stock `l_total_values` label is 242x68 at local `(-3,49)`, center pivot, 16 px font, `spacingY=-3`, `ShrinkContent`; the selected prayer item cell occupies the middle of the container and the craft button is at window-local `y=-85`.
- Layout change: context remained in the measured upper text region; a native-font cloned result label occupied the measured region below the prayer item cell; the craft button moved to `y=-100`; a smaller dependency note occupied the gap below that button and above the stock controller tips. No `ResizeHeight` was used for the main stock label.
- Runtime evidence, 2026-09-14: repeated Test Harness switching across Faith, Combo, Souls, Retribution, Donations, Repose, Thorough Cleansing, Gratitude, Roots, Imagination and other synthetic selections no longer produced the cumulative downward drift seen in 0.1.4. The supplied runtime log shows PrayerClarity 0.1.5 loaded and the switches completed without a reported PrayerClarity forecast failure.
- UX result: **the drift fix is accepted as evidence, but the layout is rejected**. The fixed blocks still overlap/crowd the stock prayer slot/button area, the frame remains too short for the desired information hierarchy, and `Контекст проповеди` adds little value.
- Additional UX findings from the runtime test: the dependency note should replace church/graveyard source nouns with their native icons; Thorough Cleansing needs a Sin Shard visual cue; and repeated rebuilds are an inefficient way to calibrate several local NGUI positions.
- Status: **superseded by 0.1.6 calibration candidate; not accepted for stable**.

## PrayerClarity 0.1.4 — runtime result: single expanding label rejected

- Type: Clarity-only presentation candidate; no intended prayer-mechanics or balance changes.
- Purpose: replace the rejected 0.1.3 fixed-column/multi-widget layout with a single native-label vertical information block.
- Frozen candidate ref: `candidate/0.1.4`.
- Exact build source SHA: `cacdb1a544294c3e3601d2ee9022a137573963c7`.
- GitHub Actions run: `34883623571`.
- Workflow result: success on `ubuntu-latest`; restore, `net472` build, all 11 embedded-locale markers, artifact staging and upload passed.
- Workflow artifact ID: `10363439062` (`PrayerClarity-0.1.4-ci-cacdb1a544294c3e3601d2ee9022a137573963c7`).
- Handoff filename: `PrayerClarity-0.1.4-ci.dll`.
- Handoff DLL SHA-256: `bb0d6dfe80de07303c24756d0e3b21c0d9b522e517de8f7750a48556cf94eb07`.
- Runtime UX evidence, 2026-09-14: the semantic content was useful, but the block overlapped the selected prayer slot/button and did not provide the intended paragraph/indent hierarchy.
- Runtime geometry probe 0.1.0 established the exact stock geometry and symbol table. During repeated Test Harness selection changes, the 0.1.4 label changed from the stock 68 px center-pivot label to 170/208 px `ResizeHeight`; its local Y then progressed `49 -> 35 -> 21 -> 7 -> -7 -> -21 -> -35` across redraws. This confirms the reported cumulative downward drift is produced by the 0.1.4 presentation path rather than by prayer mechanics.
- The same probe confirmed native inline symbols `(wskull)` (`icon_skull_wreath_green`), `(faith)` (`icon_faith_ol`), `(gld)/(slv)/(brz)` and `(gratitude_points)` (`techpoint_drop_smile`).
- UX result: **rejected**. The information model remains accepted; the single `ResizeHeight` label geometry does not.
- Status: **superseded by 0.1.5**.

## PrayerClarity 0.1.3 — runtime result: fixed-column layout rejected

- Type: Clarity-only presentation candidate; no intended prayer-mechanics or balance changes.
- Purpose: validate the 0.1.2 information model with a less dense, fixed-column pulpit layout and stable font sizing.
- Frozen candidate ref: `candidate/0.1.3`.
- Exact build source SHA: `dbbb6d26b2b87ee46819984e6ae50c60b44328e0`.
- GitHub Actions run: `34877867872`.
- Workflow artifact ID: `10360339790`.
- Handoff DLL SHA-256: `e24b93a6e70c69f6c440da08d7f2bdf91c9d6bafc2a979cd6307d0ae7092066c`.
- Runtime evidence, 2026-09-14: PrayerClarity 0.1.3 and Test Harness 0.1.0 loaded successfully; synthetic prayer selection continued to work and no PrayerClarity forecast exception was observed. The rendered multi-label layout visibly drifted outside the pulpit window.
- UX result: the fixed-column/multi-widget approach is **rejected**. It introduced brittle geometry without improving clarity enough to justify the extra UI hierarchy. The underlying `guaranteed + success bonus + special effect` information model remains accepted.
- Next narrow candidate: reuse the existing vanilla pulpit UILabel instead of creating a parallel UI hierarchy; present a spacious vertical `context -> result -> effect -> dependency note` block, add live graveyard quality, and use `ResizeHeight` rather than shrinking text.
- Status: **superseded for presentation; information model retained**.

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
