# Test / Research Build Log

This file records handed executable artifacts once PrayerClarity research reaches a point where the user's installed Graveyard Keeper 1.407 runtime must provide evidence.

## Stable sibling releases — Vanilla 1.0.24 / Rebalanced 0.1.5

- User acceptance: 2026-09-18. The tested shared candidate was explicitly approved for promotion to the stable line for both sibling editions.
- Exact accepted runtime/source SHA: `3b7cea7986138f57d7ace6998b9cc6bca952af1e`.
- Frozen accepted refs: `accepted/vanilla-1.0.24` and `accepted/rebalanced-0.1.5`.
- Stable promotion PR: #1, merged to `main` as `a1510a89a31f687b0fa77d8ab8e5221e3fe34e4b`.
- Publication workflow run: `35290795881`; result: **success**.
- Publication reused the exact accepted CI artifact from run `35286685613` and verified both hashes before release; neither DLL was rebuilt.
- **PrayerClarity: Vanilla 1.0.24** — tag `v1.0.24`, asset `PrayerClarity.dll`, SHA-256 `ab53816f120ee9318944459a976a74bfe48125fabf872b6f9666920c779b4879`.
- **PrayerClarity: Rebalanced 0.1.5** — tag `rebalanced-v0.1.5`, asset `PrayerClarity.Rebalanced.dll`, SHA-256 `aa34c5fc62aa7ad02d32099264389554e2df05c212374ec5be7701fa95c428cf`.
- Runtime acceptance included the shared Technology/presentation polish and the final Temporary Effects gate. Rebalanced Silver semantics matched the accepted rule set; the Vanilla sibling retained stock mechanics/presentation policy where intended.
- Status: **accepted, merged to main, and published as two stable sibling releases**.

## PrayerClarity Test Harness Rebalanced Compatibility 0.1.2

- Type: research/test-only compatibility adapter; not production PrayerClarity behavior.
- Purpose: replace the failed 0.1.1 probe with a hook on the already runtime-verified Harness seam `BuffsLogics.AddBuff(string, Nullable<float>)`. Before the synthetic PlayerBuff is added, the adapter infers Bronze/Silver/Gold from the Harness duration override and writes the same Rebalanced tier/effect tokens that the normal successful-sermon path would capture.
- Source branch: `research/test-harness-rebalanced-compat`.
- Candidate ref: `candidate/test-harness-rebalanced-compat-0.1.2`.
- Exact source SHA: `437e8020208a91818d6ca8aef4dc9f44c2a3fd22`.
- GitHub Actions run: `35289408160`.
- Workflow result: success on `ubuntu-latest`; `net472` Release build and artifact upload passed.
- Workflow artifact ID: `10525896671` (`PrayerClarity-TestHarness-RebalancedCompat-0.1.2-ci-437e8020208a91818d6ca8aef4dc9f44c2a3fd22`).
- Artifact ZIP digest: `sha256:1de2867e41cfd2008be8c06920cb2189b9a4a59d047601159ed418045ac642fd`.
- Handoff filename: `PrayerClarity.TestHarness.RebalancedCompat-0.1.2-ci.dll`; SHA-256: `4b1e13c70e23ae0b02c8a3ef080518f7f054abf6ee7d2112b2deb6c90694788b`.
- Test protocol: replace compatibility 0.1.1 with 0.1.2; keep Rebalanced 0.1.5 and the existing Harness/bridges; use Silver; activate each canonical timed prayer once and skip retired Protection/b_shield. The log should contain `Synthetic Rebalanced tier projected at BuffsLogics.AddBuff` for Roots, Repentance, Repose, Combat and Excellence before judging Temporary Effects text.
- Expected Silver Temporary Effects: Repentance 75%; Roots -30% growth time; Repose Silver reliability wording; Combat +10 damage / +4 armor / 2 HP/s; Imagination +0.7; Excellence +0.5; Soul Contentment +20%; Thorough Cleansing x2.
- Runtime result, 2026-09-18: **passed**. Rebalanced 0.1.5 loaded with compatibility 0.1.2 and the legacy Harness/bridges. The log records successful tier projection immediately before native AddBuff for Silver Roots (`buff_plant`, 72), Repentance (`buff_sins`, 36), Repose (`buff_skull`, 36), Combat (`buff_sword`, 72) and Excellence (`buff_star`, 36). The resulting Temporary Effects UI shows the expected Silver semantics: Roots -30%, Repentance 75%, Repose's half-guaranteed-best wording, Combat +10/+4/2 HP/s, Imagination +0.7, Excellence +0.5, Soul Contentment +20% and Thorough Cleansing x2.
- The current Rebalanced log contains no `InvalidCastException` or `SmartExpression` failure. The earlier Roots exception is therefore **not reproduced** under the correctly tier-projected test path and is no longer treated as a confirmed current blocker. Reopen only if it appears in a normal-sermon/runtime path.
- Status: **test adapter verified; Temporary Effects Silver presentation gate passed; research-only artifact remains non-production**.

## PrayerClarity Test Harness Rebalanced Compatibility 0.1.1

- Type: research/test-only compatibility adapter; not production PrayerClarity behavior.
- Purpose: retain the 0.1.0 Vanilla-GUID compatibility alias and additionally mirror Rebalanced's successful-prayer tier/effect-token capture immediately before the legacy Harness synthetically activates a timed buff. This makes Temporary Effects tests representative of Silver/Gold Rebalanced semantics without running the real sermon/reward path.
- Source branch: `research/test-harness-rebalanced-compat`.
- Candidate ref: `candidate/test-harness-rebalanced-compat-0.1.1`.
- Exact source SHA: `0652b8f9feb18eb6dd72c77756bda6f23b32e3d6`.
- GitHub Actions run: `35288758209`.
- Workflow result: success on `ubuntu-latest`; `net472` Release build and artifact upload passed.
- Workflow artifact ID: `10525277772` (`PrayerClarity-TestHarness-RebalancedCompat-0.1.1-ci-0652b8f9feb18eb6dd72c77756bda6f23b32e3d6`).
- Artifact ZIP digest: `sha256:c48907ef8e721a4c450fdef8c148bf481efdcb499e46c6efea68b3545c5a5d08`.
- Handoff filename: `PrayerClarity.TestHarness.RebalancedCompat-0.1.1-ci.dll`; SHA-256: `f5ee41d053a49411754cbc4dabb645fdb9d59c480711d8e7fed24dc4501642b6`.
- Test protocol: replace compatibility 0.1.0 with 0.1.1, keep Rebalanced 0.1.5 and the existing Harness/bridges, use Silver, activate each timed prayer once, and **skip retired Protection/b_shield** when validating canonical Combat because both Combat and Protection resolve to the same `buff_sword` and double activation extends its timer. Do not save the game with synthetic buffs active.
- Expected Silver Temporary Effects: Repentance 75%; Roots -30% growth time; Repose Silver reliability wording; Combat +10 damage / +4 armor / 2 HP/s; Imagination +0.7; Excellence +0.5; Soul Contentment +20%; Thorough Cleansing x2. Durations should continue to reflect the active Longer Days day length.
- Runtime result, 2026-09-18: **failed before installing the adapter patch**. Startup throws `MissingMethodException: PrayCraftGUI.DoPrayForBuff()` because 0.1.1 incorrectly assumed a zero-argument overload. Test Harness 0.1.2 and its bridges still load afterward, so the screenshot necessarily remains stock/fallback for tier-dependent Rebalanced effects.
- Status: **superseded by compatibility 0.1.2; do not use 0.1.1**.

## PrayerClarity Test Harness Rebalanced Compatibility 0.1.0

- Type: research/test-only compatibility shim; no prayer mechanics, save state, UI or Harmony patches.
- Purpose: allow the existing legacy PrayerClarity Test Harness to load while testing PrayerClarity: Rebalanced. The legacy Harness hard-depends on the Vanilla plugin GUID `nikich.graveyardkeeper.prayerclarity`; the shim exposes only that dependency identity and itself hard-depends on `nikich.graveyardkeeper.prayerclarity.rebalanced`, forcing Rebalanced to load first.
- Source branch: `research/test-harness-rebalanced-compat`.
- Candidate ref: `candidate/test-harness-rebalanced-compat-0.1.0-build2`.
- Exact source SHA: `c251c45687c5e2f45bff9b703719ad7478c5f888`.
- GitHub Actions run: `35287963473`.
- Workflow result: success on `ubuntu-latest`; `net472` Release build and artifact upload passed.
- Workflow artifact ID: `10524862386` (`PrayerClarity-TestHarness-RebalancedCompat-0.1.0-ci-c251c45687c5e2f45bff9b703719ad7478c5f888`).
- Artifact ZIP digest: `sha256:93e6d50dcc4d7519fd4060d18c24dd6e0eccf459cf425df06ad141a34cb8cde4`.
- Handoff filename: `PrayerClarity.TestHarness.RebalancedCompat-0.1.0-ci.dll`; SHA-256: `721b63377b365f7d483a1ac93c4b90fc6e7188154180910acfc88ada3190f23a`.
- Runtime gate: install only alongside PrayerClarity: Rebalanced and the existing Test Harness/bridge DLLs. Confirm the Harness and bridges now load and appear in Configuration Manager. If the Harness has a compile-time assembly reference to `PrayerClarity.dll` rather than only the BepInEx GUID dependency, this shim will not be sufficient; the next runtime log will prove that distinction.
- Runtime result, 2026-09-18: **dependency compatibility proved**. Rebalanced 0.1.5, compatibility 0.1.0, Test Harness 0.1.2 and all three Harness bridge plugins loaded together. The next test exposed that the legacy Buff Bridge bypasses Rebalanced tier capture, so 0.1.0 is superseded by 0.1.1 for tier-dependent Temporary Effects verification.
- Status: **dependency question answered; superseded by compatibility 0.1.1 for active-effect testing**.

## Shared polish candidate — PrayerClarity: Vanilla 1.0.24 / Rebalanced 0.1.5

- Type: narrow shared presentation correction plus one Rebalanced Temporary Effects consistency repair; no prayer balance values, success formulas or gameplay mechanics changed in this candidate.
- Development branch: `dev/shared-tooltip-polish-1.0.24-0.1.5`.
- Frozen candidate ref: `candidate/rebalanced-0.1.5-runtime`; one exact source state intentionally produces both sibling editions.
- Exact build source SHA: `3b7cea7986138f57d7ace6998b9cc6bca952af1e`.
- GitHub Actions run: `35286685613`.
- Workflow result: success on `ubuntu-latest`; all 11 base and Rebalanced localization JSON files parsed, Rebalanced `net472` Release build passed, Vanilla sibling `net472` Release build passed, required embedded locale markers were present, and both candidate DLL pairs were staged and uploaded.
- Workflow artifact ID: `10524397397` (`PrayerClarity-shared-ui-1.0.24-rebalanced-0.1.5-ci-3b7cea7986138f57d7ace6998b9cc6bca952af1e`).
- Artifact ZIP digest: `sha256:b09478ceb3009ff5c7ebb603b9e5de7dcb0404270eeb71c859263230ca72c5d4`.
- Vanilla handoff filename: `PrayerClarity-1.0.24-ci.dll`; canonical filename: `PrayerClarity.dll`; SHA-256: `ab53816f120ee9318944459a976a74bfe48125fabf872b6f9666920c779b4879`.
- Rebalanced handoff filename: `PrayerClarity.Rebalanced-0.1.5-ci.dll`; canonical filename: `PrayerClarity.Rebalanced.dll`; SHA-256: `aa34c5fc62aa7ad02d32099264389554e2df05c212374ec5be7701fa95c428cf`.
- Technology fix: a reward is hoisted into the shared Effect block only when both reward identity **and quantity** are invariant across tiers. Prosperity therefore no longer shows the orphan shared `Effect: Commercial Blessing`; its `x1/x2/x3` Commercial Blessing outputs remain tier-local.
- Temporary Effects audit/fix: the vanilla game still owns the buff title and icon. PrayerClarity replaces only the active-buff description and strategic timer presentation. Rebalanced Roots/Repentance now promote their meaningful remaining duration into that description when their repaired edition-specific active semantics are available; Vanilla keeps the stock broken/unverified presentation policy.
- Requested user test: first inspect Prosperity in Technology in either edition and confirm the shared `Effect: Commercial Blessing` line is gone while Bronze/Silver/Gold still show `x1/x2/x3`. In Rebalanced, activate Shoots & Roots or Repentance and open Character -> Temporary Effects; confirm the concrete effect text is present together with the remaining-duration text when at least one in-game day remains, while the stock title/icon still look normal. No full sermon/mechanics regression pass is required.
- Runtime result, 2026-09-18: the user confirmed Prayer for Prosperity now renders correctly in Technology: the meaningless shared `Effect: Commercial Blessing` line is gone and the Bronze/Silver/Gold `x1/x2/x3` Commercial Blessing quantities remain intact. This Technology fix is accepted.
- Temporary Effects verification is currently blocked by the installed legacy Test Harness. Runtime log shows `PrayerClarity Test Harness 0.1.2` is rejected because its hard dependency is `nikich.graveyardkeeper.prayerclarity` (Vanilla GUID), while the active sibling is `nikich.graveyardkeeper.prayerclarity.rebalanced`; all Harness bridge plugins then skip because the Harness did not load.
- The same runtime log exposed an independent Rebalanced Roots issue before the requested Temporary Effects test: projected plant `SmartExpression` evaluation repeatedly throws `InvalidCastException` on `Ppar("buff_plant")*Ppar("prayerclarity_rebalanced_plant_reduction")`. Treat this as a new mechanics/runtime defect requiring diagnosis before Roots is accepted; do not attribute it to the Test Harness because the Harness never loaded.
- Follow-up runtime evidence, 2026-09-18: compatibility shim 0.1.0 worked. BepInEx loaded Rebalanced 0.1.5, the compatibility alias, Test Harness 0.1.2, Buff Bridge 0.1.4, Button Bridge 0.1.3 and Item Cell Bridge 0.1.5 in one session. The Harness was configured to Silver and synthetic Silver timed buffs were activated across the roster.
- Temporary Effects screenshot/result: tier-invariant effects render correctly (Imagination +0.7, Soul Contentment +20%, Thorough Cleansing x2). Tier-dependent effects fall back to stock/Vanilla presentation: Repentance and Roots show the known-issue text; Repose shows stock +1-tier wording; Combat shows only stock +5 damage; Excellence shows stock +0.2 instead of Silver +0.5. This does **not** yet prove a production Rebalanced presentation defect.
- Root cause of that mismatch is the test path: the Harness Buff Bridge directly calls native `BuffsLogics.AddBuff` and explicitly bypasses the normal sermon path. Rebalanced quality capture is owned by `PlayerComponent.StartPrayAnimation(CraftDefinition,bool)`, so the synthetic path never writes the persisted tier/effect tokens consumed by Rebalanced Temporary Effects. The test harness therefore needs a Rebalanced tier-capture adapter before this surface can be judged.
- Combat timer note: the session activated both canonical Combat `b_sword:2` and retired Protection alias `b_shield:2`; Rebalanced maps both to `buff_sword`, so the same buff was added twice and the screenshot's 12.8-day duration is a Harness artifact rather than the canonical Silver Combat duration.
- Final Temporary Effects runtime result, 2026-09-18: compatibility 0.1.2 correctly projected Silver tier state before synthetic AddBuff. The Rebalanced screenshot matches the expected semantics across every canonical timed prayer: Roots -30%, Repentance 75%, Repose Silver reliability text, Combat +10 damage / +4 armor / 2 HP/s, Imagination +0.7, Excellence +0.5, Soul Contentment +20%, Thorough Cleansing x2. The user explicitly reported the surface as working correctly.
- Vanilla sibling spot-check, 2026-09-18: PrayerClarity 1.0.24 loaded instead of the compatibility alias (BepInEx correctly skipped the lower-version duplicate GUID shim). Vanilla Temporary Effects retained stock/known-issue semantics as intended; the reported long Rage timer came from accidental repeated synthetic activation and is not a PrayerClarity defect.
- The earlier Rebalanced Roots `InvalidCastException`/SmartExpression concern was not reproduced in the correctly tier-projected session. It is downgraded from a confirmed blocker to a non-reproduced prior anomaly; reopen only on normal-sermon evidence.
- Status: **Prosperity Technology correction accepted; Temporary Effects presentation accepted for both sibling editions within the tested scope. No remaining blocker from this polish pass.**

## Shared UI candidate — PrayerClarity: Vanilla 1.0.21 / Rebalanced 0.1.2

- Type: shared Technology information-design/presentation candidate for both editions, plus the accepted Rebalanced specialist-bonus adjustment from **+250/+350/+450%** to **+200/+300/+400%** for Faith, Donations and BSS Soul's Repose. No other Rebalanced mechanic/hook architecture changes.
- Development branch: `dev/shared-tooltip-polish-1.0.21-0.1.2`.
- Frozen candidate ref: `candidate/rebalanced-0.1.2`; one frozen source state intentionally produces both sibling editions.
- Exact build source SHA: `a0bdbc8a66e28a4175376765419ff004d7bae6a7`.
- GitHub Actions run: `35277513366`.
- Workflow result: success on `ubuntu-latest`; all 11 base and Rebalanced localization JSON files parsed, Rebalanced `net472` Release build passed, Vanilla sibling `net472` Release build passed, all required embedded locale markers were present, and both candidate DLL pairs were staged in one run.
- Workflow artifact ID: `10521206721` (`PrayerClarity-shared-ui-1.0.21-rebalanced-0.1.2-ci-a0bdbc8a66e28a4175376765419ff004d7bae6a7`).
- Artifact ZIP digest: `sha256:9a9ac2480d23dd05732da29f539135bfdd7a7b2fa9eccbaf126a25a7ef9d66cc`.
- Vanilla handoff filename: `PrayerClarity-1.0.21-ci.dll`; canonical filename: `PrayerClarity.dll`; SHA-256: `847953001c7fb2a5be008d7d705b2da5971faa38b2b5d33244140a8c9b8be8f6`.
- Rebalanced handoff filename: `PrayerClarity.Rebalanced-0.1.2-ci.dll`; canonical filename: `PrayerClarity.Rebalanced.dll`; SHA-256: `7b08540adb952466e84cd92246e853c07b9a5e26a1e4f22f949439c65c069ddf`.
- Shared presentation scope: Technology follows `shared in-world effect -> terse quality delta`; short mechanics rows are atomic; Effect/special-entity labels gain restrained semantic accents; Commercial Blessing keeps one consistent entity accent; Excellence can color only its key tier value Bronze/Silver/Gold; durations remain plain; new strings ship in all 11 locales. Vanilla and Rebalanced compile the same shared renderer/style/localization infrastructure while edition-specific semantics remain separate.
- Rebalanced wording scope: Roots uses a shared plant-growth explanation plus `growth time -20/-30/-40%`; Repentance uses a shared daily-confessional explanation plus `confession chance 50/75/100%`; Repose uses a shared better-corpse cue with upward/white-skull/red-skull symbols and the accepted natural Bronze/Silver/Gold reliability wording; Imagination lifts its invariant +0.7 writing-quality effect above the tiers and preserves Silver/Gold Story rewards; Excellence presents its +0.2/+0.5/+1.0 values as the key quality ladder; BSS Soul's Repose explicitly explains that base Faith depends on Church Quality and Soul Gratitude before the tier percentage is applied.
- Requested user test: install **one edition at a time**. Primary gate is Rebalanced 0.1.2 in Russian: inspect Repose, Repentance, Shoots & Roots, Imagination, Excellence, Prosperity, Faith, Donations and BSS Soul's Repose where available. Confirm the shared/tier hierarchy reads naturally; skull/up icons render; semantic colors render rather than exposing raw NGUI tags; atomic label/value rows do not split awkwardly; Commercial Blessing and Story accents are coherent; and the tooltip remains viewport-safe. Then spot-check Vanilla 1.0.21 (especially Repose, Imagination/Excellence and Prosperity) to confirm the same shared grammar improves presentation without changing stock mechanics. Existing 0.1.0/0.1.1 runtime evidence already covers the underlying Rebalanced mechanic seams; this pass does not require repeating the full sermon/mechanics smoke unless the UI or values expose a discrepancy.
- Status: **ready for runtime visual/UX verification; not accepted**.

## PrayerClarity: Rebalanced 0.1.1 — presentation/localization polish candidate

- Type: Rebalanced presentation/localization follow-up; **no prayer-mechanics or balance changes** and no Technology-tooltip layout redesign.
- Development branch: `dev/rebalanced-0.1.1`.
- Frozen candidate ref: `candidate/rebalanced-0.1.1-runtime`.
- Exact build source SHA: `40c5f60062267e887756864e096a0f731efe54f0`.
- GitHub Actions run: `35255682271`.
- Workflow result: success on `ubuntu-latest`; Rebalanced `net472` Release build, sibling Vanilla shared-source build, all 11 base/Rebalanced locale-resource checks, artifact staging and upload passed.
- Workflow artifact ID: `10512445366` (`PrayerClarity-Rebalanced-0.1.1-ci-40c5f60062267e887756864e096a0f731efe54f0`).
- Artifact ZIP digest: `sha256:0c97f4fe018d8fb03ef933f0cf8c327283804e6ee67ef8c3ebe483582dc6aedc`.
- Handoff filename: `PrayerClarity.Rebalanced-0.1.1-ci.dll`.
- Canonical install filename inside the artifact: `PrayerClarity.Rebalanced.dll`.
- Handoff/canonical DLL SHA-256: `93313be354b5fc965c6cbef3a456b37c7ef10371935696f0ba1229452f01d721`.
- Scope: Bronze Repose no longer calls the parameterized stock `active.skull` localization key without its required argument; it uses a dedicated Rebalanced semantic string describing the stock-style expanded body range. Roots, Repentance, Repose, Combat and Soul Contentment presentation remains driven by the same effective-rule semantic layer; new Rebalanced wording was shortened and made more player-facing in all 11 `lang_rebalanced` overlays. Common `lang/*.json`, mechanics rules/hooks and Technology layout were not changed. Version metadata/build packaging was advanced to 0.1.1.
- Requested user test: in Russian, inspect the Technology tooltips for Repose (all three qualities), Shoots & Roots, Repentance and Soul Contentment; optionally spot-check Combat. Confirm Bronze Repose is Russian rather than falling back to English, the new effect lines are shorter/easier to parse, and no new clipping/wrapping or excessive tooltip height appears. No sermon execution or mechanics test is required for this candidate.
- Status: **ready for short runtime visual retest; not accepted**.

## PrayerClarity 1.0.10 — Technology tier-first UX candidate

- Type: Clarity-only Technology presentation candidate; no prayer-mechanics or balance changes.
- Development branch: `dev/technology-tooltip-ux-1.0.10`.
- Frozen candidate ref: `candidate/1.0.10`.
- Exact build source SHA: `f21674f0ddcc7e06a7d0d1b587faa57e2e5c0b1d`.
- GitHub Actions run: `35160674466`.
- Workflow result: success on `ubuntu-latest`; restore, `net472` Release build, all 11 embedded-locale markers, artifact staging and upload passed.
- Workflow artifact ID: `10473296768` (`PrayerClarity-1.0.10-ci-f21674f0ddcc7e06a7d0d1b587faa57e2e5c0b1d`).
- Artifact ZIP digest: `sha256:31dc000d9e1d70ba00d6bc93c7d56115eb27a11c97d725b0e693e07c5a7bbbab`.
- Handoff filename: `PrayerClarity-1.0.10-ci.dll`.
- Handoff DLL SHA-256: `10c04409124914529c962017a7829eecd4fc1e1c2ae376f0230aaa06446c2ff7`.
- Scope: Technology comparison changes from property-first to `shared invariants -> per-tier snapshots`; uses native quality glyphs as tier headings; uses explicit localized `100% success requires` wording; separates proportional and flat success additions; omits the Commercial Blessing purpose paragraph from the comparison body; gives the Technology body an adaptive maximum width derived from 72% of logical safe-area width with the prior 360 value retained as fallback. The accepted 1.0.9 PrayerClarity-owned viewport clamp remains unchanged.
- Requested user test: inspect Prayer for Prosperity first, then Faith or Donations, Combo and Shoots & Roots in Technology. Confirm the tier grouping is immediately readable, the `% + flat` ambiguity is gone, the threshold wording is clear, normal tier rows no longer wrap unnecessarily, and the bubble remains inside the viewport. Russian at the normal resolution is the primary gate; German and Japanese are useful spot checks if convenient. Ordinary non-prayer Technology tooltips should remain vanilla. No sermon execution is required.
- Status: **ready for runtime UX/geometry verification; not accepted**.

## PrayerClarity 0.1.9 — real-window resize / pulpit layout candidate

- Type: Clarity-only presentation/calibration candidate; no intended prayer-mechanics or balance changes.
- Purpose: replace the disproved child-art resizing attempts with live resizing of the verified real Pray GUI `window`/`container`, retain the game's native sliced frame behavior, separate the `Result` heading from result rows, and repair the remaining special-prayer presentation defects found in the 0.1.8 test.
- Development branch: `dev/pulpit-window-resize-0.1.9`.
- Frozen candidate ref: `candidate/0.1.9`.
- Exact build source SHA: `7bc60963e3eb589ed14b7fe011d9c9338aa38dd9`.
- GitHub Actions run: `34908149431`.
- Workflow result: success on `ubuntu-latest`; restore, `net472` build, all 11 embedded-locale markers, artifact staging and upload passed.
- Workflow artifact ID: `10373317657` (`PrayerClarity-0.1.9-ci-7bc60963e3eb589ed14b7fe011d9c9338aa38dd9`).
- Handoff filename: `PrayerClarity-0.1.9-ci.dll`.
- Handoff DLL SHA-256: `3bd34f80d7082ed728732df1e8b9f92f01c567b4f17a427f0383b5735bd2f378`.
- Real-window controls: Configuration Manager section `Prototype pulpit layout tuning v4` exposes `Window extra width` and `Window extra height`. These modify the actual `UI Root/Pray GUI/window` and anchored `container`, not merely decorative child sprites.
- Frame architecture: the verified stock `back` (`Sliced`, border 30), `decore_back` (`Sliced`, border 15) and `header` (`Sliced`, border 65/5/65/5) are resized from captured vanilla dimensions. The `pulpit_bench_back` `Simple` decoration is deliberately not stretched. Header/close controls and controller tips move relative to the enlarged frame.
- Layout calibration: latest 0.1.8 user values are the v4 starting point: context `8/72/14`, Result heading `8/20/14`, result rows `16/4/14`, effect `-122/-30/12` with icon `10`, note `-6/-87/9`, selector `70/45`, prayer button `0/-120`. All remain temporary live tuning controls.
- Result grammar: heading is now an independently positioned/font-sized label; resource rows contain only `Guaranteed` and the shorter `On success (N%)` wording. Specialist arrows remain attached only to the resource actually improved by Faith or Donations.
- Repose wording: stock Clarity uses the player-facing Donkey/body formulation with native `(up)` and `(skull)` symbols; it still does not claim the future Rebalanced bronze/silver/gold reliability ladder.
- Vanilla localization fix: direct audit evidence places `GJL` in `Assembly-CSharp-firstpass`; `R.VanillaLocalize` now resolves `GJL.L(string)` across loaded assemblies once and caches the method. This fixes raw localization IDs such as `blessing_commerce` and the blank Soul's Repose description caused by the old Assembly-CSharp-only resolver.
- Prosperity: uses the vanilla localized `Blessing of commerce` name/description and the stock output count, without the redundant `Reward:` prefix inside an `Effect:` row.
- Soul's Repose: explicitly reuses vanilla `b_souls_d` after the localization resolver fix.
- Thorough Cleansing: still requests verified native sprite `i_sin_shard`; the v4 effect-icon path lazily resolves it and caches only successful sprite lookups, allowing a later UI redraw to recover if the atlas was not ready on the first attempt.
- Localization: all changed PrayerClarity-owned wording is present in the same 11 interface locales as the base game.
- Requested user test: replace 0.1.8 with 0.1.9 and remove the completed frame-slice probe. Keep Test Harness 0.1.0. First adjust only `Window extra width/height` and confirm the *actual frame* grows without stretching the pulpit bench artwork or accumulating drift. Then tune Result heading/rows, selector, prayer button, effect and note as desired. Finally switch Faith -> Donations -> Combo -> Repose -> Prosperity -> Soul's Repose -> Soul Contentment -> Thorough Cleansing and report screenshots plus final F1 values. No sermon execution is required.
- Status: **ready for runtime UX/geometry verification; not accepted**.

## PrayerClarity Pulpit Frame Slice Probe 0.1.0 — completed runtime evidence

- Type: research-only, read-only UI geometry probe; no intended save/player/world mutation.
- Exact source SHA: `74f617ae4ea051e60324b2504611e0b3764e60dc`.
- GitHub Actions run: `34906052752`.
- Workflow artifact ID: `10372548949`.
- Probe DLL SHA-256: `b8c3cc5e82b2e09371f1050100d6834ef41ef834152bea49c0ee4caea64872cd`.
- Runtime evidence, 2026-09-15: the real `UI Root/Pray GUI/window` is a `UIWidget` 274x241; its anchored `container` is 274x199. The visible frame is not one simple bitmap: `back` is an NGUI `UI2DSprite` already configured `Sliced` with 30 px borders; `decore_back` is `Sliced` with 15 px borders; `header` is `Sliced` with 65/5/65/5 borders. `decore` / `pulpit_bench_back` is `Simple` and therefore must not be stretched as the frame grows.
- Consequence: the earlier 0.1.6/0.1.7 failures targeted child artwork rather than the real window boundary. The next resize implementation may legitimately change the root window/container dimensions and use the stock sliced frame contract instead of inventing a custom texture/frame system.
- Status: **question answered; remove probe after capture**.

## PrayerClarity 0.1.8 — runtime result: information improved, fixed-window layout not accepted

- Type: Clarity-only presentation candidate; no intended prayer-mechanics or balance changes.
- Frozen candidate ref: `candidate/0.1.8`.
- Exact build source SHA: `46f5d9954d710a160308e393f2cb1bf91e28e82a`.
- GitHub Actions run: `34899841772`.
- Workflow artifact ID: `10369988505` (`PrayerClarity-0.1.8-ci-46f5d9954d710a160308e393f2cb1bf91e28e82a`).
- Handoff filename: `PrayerClarity-0.1.8-ci.dll`.
- Handoff DLL SHA-256: `8b78d5bf6b3bd395b0b8399e016f6e49b894e08ad6e509f50c4f4da2d5eb9656`.
- Runtime result, 2026-09-15: the fixed-window composition is conceptually useful but cannot be finalized inside the stock frame. The user explicitly requested true window resizing before accepting final text placement.
- Latest calibration from the supplied 0.1.8 screenshot: context `8/72/14`, result `8/20/14`, effect `-122/-30/12`, effect icon `10`, note `-6/-87/9`, selector `70/45`, prayer button `0/-120`.
- UX findings: `Result` must be movable/font-sized independently from Guaranteed/Success rows; `Additional on success (100%)` is too long and should be shortened; long special effects need localization-safe wrapping; Repose should use a Donkey + higher-quality-body formulation with native upward/skull cues.
- Remaining presentation defects observed in 0.1.8: Prosperity still exposed raw `blessing_commerce`/redundant `Effect: Reward:` wording; BSS Soul's Repose remained blank; Thorough Cleansing still lacked the expected Sin Shard icon. Soul Contentment ordering was reported as corrected.
- Status: **superseded by 0.1.9; information model retained, fixed-window layout rejected**.

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
- Exact source SHA: `adf752e1a04c93172bdedc54b913d4b926413c0d`.
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
- Workflow result: success on `ubuntu-latest`; `net472`, all 11 embedded-locale markers, artifact staging and upload passed.
- GitHub Actions artifact ID: `10366245690` (`PrayerClarity-0.1.5-ci-91cc03b7e1902814d651565ffd591e81338ae4f9`).
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
- Workflow result: success on `ubuntu-latest`; `net472`, all 11 embedded-locale markers, artifact staging and upload passed.
- GitHub Actions artifact ID: `10363439062` (`PrayerClarity-0.1.4-ci-cacdb1a544294c3e3601d2ee9022a137573963c7`).
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
- Exact source SHA: `dbbb6d26b2b87ee46819984e6ae50c60b44328e0`.
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

## PrayerClarity: Rebalanced 0.2.0 — first executable candidate

- Type: executable candidate for the accepted 2026-09-18 fairness/rework specification.
- Frozen candidate ref: `candidate/rebalanced-0.2.0`.
- Exact build source SHA: `26048581c3fe6e0d8ef4ae930a0c29474f68bbcf`.
- GitHub Actions run: `35346055221`.
- Workflow result: **success** on `ubuntu-latest`; localization JSON validation, restore, Rebalanced build, Vanilla sibling build, locale-marker verification, staging and artifact upload all passed.
- Workflow artifact ID: `10547166000`.
- Artifact: `PrayerClarity-shared-ui-1.0.24-rebalanced-0.2.0-ci-26048581c3fe6e0d8ef4ae930a0c29474f68bbcf`.
- Artifact ZIP digest: `sha256:54a49d9a460d255a594b788089cde9b26433f38eb0f1214d6a3a5efa94c5051f`.
- Handoff filename: `PrayerClarity.Rebalanced-0.2.0-ci.dll`.
- Handoff DLL SHA-256: `84cf07be553e137d4663d24267ab18257facfbe44c0a833d87579876742b6de1`.
- Supported target: Graveyard Keeper 1.407, Assembly-CSharp MVID `6f50b8e7-156b-49ac-bbe8-7505894b2364`.
- Mechanics scope: accepted 0.2.0 resource-family balance, revised CQ ladders, BSS Soul's Repose q30/60/120, fixed specialist outputs through stock success-only prayer output items, and best-existing-tier Repose narrowing.
- Shared presentation scope: current Soul Gratitude pulpit metric for Soul's Repose; contextual Repose endpoint replacement derived from current player/body data; all new strings present in 11 official locales.
- Performance shape: no per-frame work; corpse tier catalogue is inspected only on relevant Donkey delivery or pulpit redraw; stock `GameSave.GenerateBody` remains authoritative.
- Runtime status: **not yet accepted**. Focused in-game acceptance is required; do not promote to `main` yet.


## PrayerClarity: Vanilla 1.0.25 — shared clarity candidate

- Type: shared Clarity/pulpit follow-up for the Vanilla sibling; **no prayer mechanics or balance changes**.
- Frozen candidate ref: `candidate/vanilla-1.0.25`.
- Exact source SHA: `ebe069b4ad202ae786af9c63ded0ffb00502cff7`.
- GitHub Actions run: `35347730945`.
- Workflow result: **success** on `ubuntu-latest`; 11 localization JSON files validated, restore/build passed, embedded-locale markers passed, candidate/Nexus staging passed.
- Workflow artifact ID: `10547383753`.
- Artifact: `PrayerClarity-1.0.25-ci-ebe069b4ad202ae786af9c63ded0ffb00502cff7`.
- Artifact ZIP digest: `sha256:d7fb3fe9bd789a6568a0204d033c044c2f17c028b8b6125b15858317cb6a0f92`.
- Handoff filename: `PrayerClarity-1.0.25-ci.dll`.
- Handoff DLL SHA-256: `72b2237607734d8b50d666cef56e18d458b88c6a456211d496e2410ac646e3a9`.
- Supported target: Graveyard Keeper 1.407, Assembly-CSharp MVID `6f50b8e7-156b-49ac-bbe8-7505894b2364`.
- Scope:
  - Soul's Repose pulpit context displays current Soul Gratitude as an additional input metric.
  - Repose pulpit can replace the stock effect line with the localized endpoint message when the stock +1 maximum adds no higher ordinary corpse tier.
  - shared corpse-tier catalogue/state lookup is presentation-only in Vanilla.
  - all stock prayer requirements, outputs, formulas, buffs and corpse-generation mechanics remain unchanged.
- Test Harness protocol: use the normal Vanilla Test Harness only. **Do not load PrayerClarity.TestHarness.RebalancedCompat** with Vanilla; that adapter exists only for the Rebalanced sibling and uses the legacy Vanilla GUID alias.
- Runtime acceptance:
  1. select Soul's Repose and confirm the additional Soul Gratitude row fits naturally;
  2. select Repose and confirm the ordinary effect wording remains while a higher ordinary tier is available;
  3. on a save at terminal Donkey progression, confirm the effect line becomes `Ещё более качественные тела недоступны.`;
  4. confirm no stock mechanics/balance values changed.
- Status: **ready for focused runtime verification; not accepted**.


### Runtime check 2026-09-18 — Rebalanced 0.2.0 first pass

User-tested candidate:
- Rebalanced source: `26048581c3fe6e0d8ef4ae930a0c29474f68bbcf`
- DLL SHA-256: `84cf07be553e137d4663d24267ab18257facfbe44c0a833d87579876742b6de1`
- Test Harness 0.1.2 + Rebalanced Compatibility 0.1.2 + existing bridge set.

Accepted observations from this pass:
- Rebalanced 0.2.0 and Test Harness compatibility loaded successfully together.
- Faith / Donations / Combo preview semantics and revised requirements appeared as designed.
- Soul's Repose pulpit displayed the additional current Soul Gratitude metric without requiring manual layout tuning.
- Soul's Repose Bronze and Gold screenshots matched the candidate ladder: q30 Bronze / q120 Gold and +50% / +150% success contribution.
- RU/EN/DE/JA screenshots rendered the added context; JA is visually denser but no blocking clipping/overflow was reported.
- User exercised multiple timed prayer previews/activations and reported matching descriptions across pulpit, Technology and active-effect/character surfaces.
- Harness compatibility successfully projected Rebalanced tier state for real synthetic native buffs (observed Gold Excellence 54 min, Gold Roots 108 min, Gold Combat 108 min).
- No PrayerClarity-specific exception/error was observed in the supplied runtime log.

Not verified in this pass:
- terminal Repose endpoint presentation/behavior: current save has not yet reached terminal corpse progression;
- actual normal-sermon reward payout for the new fixed-only Faith/Donations and percentage-only Combo family: Test Harness synthetic preview bypasses normal sermon rewards by design;
- exhaustive visual acceptance of every locale; multi-language switching was exercised, with screenshots supplied for representative locales;
- exact timed-effect duration audit was not manually repeated because durations are unchanged by the candidate.

Status:
- **presentation/semantic/timed-buff first pass accepted**;
- candidate remains pending the narrow normal-sermon reward check and terminal Repose check (the latter can wait until a suitable save/progression state exists).


### Runtime check 2026-09-18 — Vanilla 1.0.25 shared-Clarity pass

User-tested candidate:
- Vanilla source: `ebe069b4ad202ae786af9c63ded0ffb00502cff7`
- DLL SHA-256: `72b2237607734d8b50d666cef56e18d458b88c6a456211d496e2410ac646e3a9`
- Test Harness 0.1.2 + existing bridge set; Rebalanced Compatibility correctly absent.

Accepted observations:
- PrayerClarity 1.0.25 loaded successfully as the clarity-only sibling.
- Soul's Repose displayed the added current Soul Gratitude row at the pulpit.
- Vanilla Soul's Repose retained stock q60 Gold and stock sermon contributions; the shared UI addition did not alter mechanics.
- Vanilla Repose Gold retained its stock q50/effect presentation on the tested non-terminal save, so the endpoint replacement did not trigger prematurely.
- No PrayerClarity-specific exception/error was observed in the supplied runtime log.

Not verified:
- terminal Repose endpoint wording, because no available save has terminal Donkey corpse progression.

Status:
- **Vanilla 1.0.25 shared-Clarity runtime pass accepted except terminal Repose endpoint**, which can wait for a suitable save.
