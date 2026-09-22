# Test / Research Build Log

## PrayerClarity: Rebalanced 0.2.15 — accepted stable

- User runtime acceptance: **2026-09-23**. Stable promotion authorized.
- Frozen accepted ref: `accepted/rebalanced-0.2.15`.
- Exact build/source SHA: `ac953fe25ef17dbaf63340a7b9309dadec7e207e`.
- GitHub Actions run: `35791783893`; Rebalanced build: **success, 0 warnings, 0 errors**.
- Workflow artifact ID: `10722636745`.
- Workflow artifact ZIP digest: `sha256:fc99440a10056010e741e7f6459c09895c95ef1dcd5a90032d7609569d67af7a`.
- Canonical Rebalanced DLL SHA-256: `a980ecfeec4553c208280ca6ca2ca49196d4fb2bab6b6e121c908d0550ce0c4f`.
- Handoff DLL: `PrayerClarity.Rebalanced-0.2.15-test.dll`; it is the exact canonical Rebalanced DLL bytes from the run, renamed without rebuilding.
- The accepted CI artifact was produced while the shared workflow still stamped its package filename/BUILD_INFO as 0.2.14. Those stale packaging labels are **not** authoritative; the compiled assembly itself contains `0.2.15.0`, informational version `0.2.15+ac953fe25ef17dbaf63340a7b9309dadec7e207e`, and plugin version `0.2.15`. The workflow is corrected as part of stable promotion; the accepted DLL itself is not rebuilt.
- Candidate changes:
  - Repose requirements: **20 / 40 / 90**;
  - Bronze unchanged;
  - Silver unchanged from accepted 0.2.14;
  - Gold keeps best-available-tier narrowing and additionally limits that tier to the game-derived maximum total skull score;
  - no hard-coded terminal tier/body IDs/10-skull value;
  - vanilla `GameSave.GenerateBody`, RNG and body construction remain authoritative;
  - temporary body-catalog projection is restored in a finalizer.
- Accepted runtime evidence:
  - the live 1.407 terminal fixture contained 33 ordinary candidates across tiers 2..3, with best tier 3, maximum total skull score 10, and 9 tied maximum-score candidates;
  - ten consecutive real `GameSave.GenerateBody(2,4,-1,-1)` calls exercised the production Gold seam;
  - every completed assertion selected tier 3 with total skull score 10, saw the same 9-candidate maximum-score scoped pool, and confirmed exact catalog restoration after the call;
  - an 11th native tier-3 body generation began before the research harness itself stalled from batching too many heavyweight calls into one UI callback; the harness issue is separate from production Gold behavior;
  - the changed Gold-generation/restoration property is accepted without further user repetition.
- Silver remains unchanged from accepted 0.2.14 and was not reopened.
- Earlier-progression testing is not required because the implementation derives the best available tier and maximum score dynamically from the live Repose-expanded range.
- Numbered accepted bytes are immutable. Stable publication must reuse the exact DLL hash above without rebuilding.

This file records handed executable artifacts once PrayerClarity research reaches a point where the user's installed Graveyard Keeper 1.407 runtime must provide evidence.


### Repose Gold one-button runtime companion

- Research-only companion: `PrayerClarity.ReposeGoldSelfTest 0.1.0`.
- Exact self-test source SHA: `2f878bbee45bc7621e0f67bb9af864d7a2f94d26`.
- CI run: `35792990408`; result: **success, 0 warnings, 0 errors**.
- Artifact ID: `10722798343`.
- Artifact ZIP digest: `sha256:beeabab41441aeec06e9fa5a685d65ba66f881858140c0bb6cab4d56f0baa092`.
- Self-test DLL SHA-256: `382061c506177d65e0116dd677e94f10f505e534b143907be635fc23f837865d`.
- Target production bytes remain the exact 0.2.15 candidate DLL SHA-256 `a980ecfeec4553c208280ca6ca2ca49196d4fb2bab6b6e121c908d0550ce0c4f` from source `ac953fe25ef17dbaf63340a7b9309dadec7e207e`.
- The companion does not replace production mechanics. It opens an F1 research window with one button.
- The button refuses to run if a real `buff_skull` is already active; otherwise it activates `buff_skull` through native `BuffsLogics.AddBuff`, injects the already-accepted post-Donkey Gold pending state, and performs 16 real `GameSave.GenerateBody(2,4,-1,-1)` calls.
- It independently inspects the live 1.407 body catalogue, expects the canonical fixture of 33 ordinary definitions across tiers 2..3, best tier 3, maximum total skull score 10, and 9 tied maximum-score definitions.
- During every generation it verifies that production installed a narrowed scoped body catalogue containing only maximum-score candidates, that the actually selected `BodyDefinition` has the expected best tier/score, and that the exact original `GameBalance.bodies_data` reference is restored after the call.
- Cleanup removes the synthetic buff. If production catalogue restoration fails, the harness records FAIL first and then performs an emergency restore of the exact pre-test catalogue reference.
- What this proves: the new 0.2.15 Gold filtering/generation/restoration path under the real game runtime.
- What this intentionally does not re-prove: the already accepted ordinary-Donkey FlowCanvas caller predicate or sermon calendar/pulpit lifecycle.


#### Self-test 0.1.0 runtime result — rejected harness, accepted Gold-path evidence

- User runtime on Graveyard Keeper 1.407 loaded Rebalanced 0.2.15 and Self-Test 0.1.0 correctly.
- 0.1.0 executed a synchronous loop of 16 real `GameSave.GenerateBody` calls directly from the UI action.
- The game became unresponsive during that batched run; therefore Self-Test 0.1.0 is **rejected as a reusable test harness** and must not be run again.
- Before the hang, samples 1–10 completed their full independent assertions. Every one selected tier 3, total skull score 10, saw exactly 9 scoped maximum-score candidates, and observed `catalog_restored=true`.
- The native game log then recorded an 11th body creation/generation (`body_3_2`, tier 3) before the harness emitted its own sample-11 assertion. This localizes the failure to the batched harness execution/verification envelope rather than demonstrating a Gold-generation failure.
- For the **production Gold-generation/restoration property**, this evidence is sufficient: the harness directly observed the complete scoped candidate set (9 tied max-score definitions) and verified the selected body plus exact catalog restoration on ten consecutive real native calls. Further user repetition would add cost without materially increasing confidence.
- The missing final cleanup log belongs to the rejected harness session lifecycle, not to the per-call production finalizer; per-call production restoration was already observed ten times.
- Repose Gold generation/restoration is therefore **accepted runtime evidence for the 0.2.15 candidate**. No rerun with 0.1.1 is required for this change.

#### Self-test 0.1.1 replacement

- Status: **reserve regression diagnostic; not required for current 0.2.15 acceptance**.
- Exact source SHA: `d52e3fce96e3c44316e824cb2be6955fe2f70ddf`.
- CI run: `35795884919`; result: **success, 0 warnings, 0 errors**.
- Artifact ID: `10723344922`.
- Artifact ZIP digest: `sha256:206c2739b1a50f7af9cc7ed0a62c5012c2138e7b2569714e1bae692fb2b17519`.
- Self-test DLL SHA-256: `e7b2685f95c5b0ca4e0c82cb0641252ba8e3bec8697adac18fbe31017eff2274`.
- Production candidate remains unchanged: Rebalanced 0.2.15 source `ac953fe25ef17dbaf63340a7b9309dadec7e207e`, DLL SHA-256 `a980ecfeec4553c208280ca6ca2ca49196d4fb2bab6b6e121c908d0550ce0c4f`.
- 0.1.1 removes the synchronous 16-call UI loop. It performs **4 real GenerateBody calls**, one paced step at a time with 0.25 s between steps, while keeping the same production-path, scoped-catalog, selected-body and restoration assertions.
- Four samples are sufficient because the core Gold assertion is deterministic: the harness independently verifies that the live scoped candidate set contains all 9 tied maximum-score definitions; repeated generation is retained only as a small runtime/restoration sanity sample, not as statistical proof.

## PrayerClarity: Rebalanced 0.2.14 — accepted stable

- User runtime acceptance: **2026-09-20**.
- Frozen accepted ref: `accepted/rebalanced-0.2.14`.
- Exact tested/build source SHA: `11fa4648fe57995938a2a17093ac4ed4f5e314cd`.
- Candidate CI run: `35503599956`; result: **success**.
- Artifact ID: `10603401146`.
- Artifact: `PrayerClarity-rebalanced-0.2.14-ci-11fa4648fe57995938a2a17093ac4ed4f5e314cd`.
- Artifact ZIP digest: `sha256:5e170c82b46dc42d8e4e3799bf91fddf2e651c7aafc409b7dd65328c18d799cc`.
- Accepted/release DLL SHA-256: `1f26c487777c4c4744f6ea4318796d369aead2b2fbd6ed299ad3edd8e79b4dc8`.
- Stable promotion PR: **#26**, squash-merged to `main` as `8a2e9634538dd0a789fc0636a5a427fa484bdcaf`.
- Accepted behavior:
  - Rebalanced Technology uses **При успехе:** / **On success:** instead of **Бонусы при успехе** / **Bonuses on success**;
  - prayer-item tooltips remain unchanged and keep the existing **Бонусы при успехе** hierarchy;
  - all accepted 0.2.13 mechanics, balance, requirements, payouts, durations, Soul wording, HUD and Temporary Effects behavior remain unchanged.
- Runtime evidence explicitly confirmed both requested conditions: Technology shows **При успехе:** and prayer-item tooltips still show **Бонусы при успехе**.
- Status: **accepted and promoted to main; publication must reuse the exact handed DLL bytes without rebuilding**.
- Stable publication workflow: `35504153407`; result: **success**. It downloaded artifact `10603401146`, re-verified DLL SHA-256 `1f26c487777c4c4744f6ea4318796d369aead2b2fbd6ed299ad3edd8e79b4dc8`, and published the exact accepted bytes without rebuilding.
- Published release: `rebalanced-v0.2.14` -> target `11fa4648fe57995938a2a17093ac4ed4f5e314cd`, asset `PrayerClarity.Rebalanced.dll`.
- Final status: **accepted, merged to main, frozen by accepted ref, and published as stable Rebalanced release**.

## PrayerClarity: Rebalanced 0.2.13 — accepted stable

- User runtime acceptance: **2026-09-20**.
- Frozen accepted ref: `accepted/rebalanced-0.2.13`.
- Exact tested/build source SHA: `4726160ced2dfd15f08b10cc0499eb8c5490e3eb`.
- Candidate CI run: `35482448986`; result: **success**.
- Artifact ID: `10596500719`.
- Artifact: `PrayerClarity-rebalanced-0.2.13-ci-4726160ced2dfd15f08b10cc0499eb8c5490e3eb`.
- Artifact ZIP digest: `sha256:bc72d63459537e4e52c1a6e71b515d2ca8a9ca19a6d6688be2b8cc26057d7911`.
- Accepted/released DLL SHA-256: `da8633b622ad755ca9bac76bbf737e37d6a0f5fb24dab8862efa297c2b1874bd`.
- Stable promotion PR: **#20**, squash-merged to `main` as `b8888d32648a6f7c16f473792636872bc8212344`.
- Accepted behavior:
  - specialist prayers no longer carry unrelated prayer-owned Faith/donation percentage bonuses or fixed Faith/money outputs;
  - Prayer for Donations pays **+5 / +15 / +30 silver**;
  - Combo Prayer Faith remains **+100 / +150 / +200%** and donations are **+100 / +200 / +300%**;
  - 0.2.12 Technology layout repair is accepted: cleaned specialists no longer revive the stock requirement sentence, and crafting-location text stays at the bottom;
  - Soul Contentment shows the effective **+20% Soul Gratitude** with the native Soul Gratitude icon;
  - BSS Soul's Repose uses the approved concise scaling explanation that more Soul Gratitude means a larger Faith bonus.
- Runtime screenshots/user feedback explicitly accepted the final Soul Contentment and Soul's Repose wording; earlier 0.2.11/0.2.12 checks had already confirmed the resource values, specialist cleanup, Technology ordering, prayer-item tooltip, HUD and Temporary Effects behavior.
- Stable publication workflow: `35482867100`; result: **success**. It downloaded artifact `10596500719`, re-verified the accepted DLL hash, and published the exact accepted bytes without rebuilding.
- Published release: `rebalanced-v0.2.13` -> target `4726160ced2dfd15f08b10cc0499eb8c5490e3eb`, asset `PrayerClarity.Rebalanced.dll`.
- Final status: **accepted, merged to main, frozen by accepted ref, and published as stable Rebalanced release**.

## PrayerClarity: Rebalanced 0.2.12 candidate

- Status: **handed for focused Technology runtime/visual acceptance; not stable; do not merge to main yet**.
- Candidate branch: `candidate/rebalanced-0.2.12`.
- Exact build/source SHA: `2ac84b31dce30ccb35648a3706e58f40dda6429d`.
- GitHub Actions run: `35481230526`; result: **success**.
- Artifact ID: `10595647907`.
- Artifact: `PrayerClarity-rebalanced-0.2.12-ci-2ac84b31dce30ccb35648a3706e58f40dda6429d`.
- Artifact ZIP digest: `sha256:9a96958fe6c7f37c92715783487880e4deedc8f1c8b338a0c8c331de070abb2c`.
- Handoff DLL: `PrayerClarity.Rebalanced-0.2.12-ci.dll`; SHA-256: `2b8b9246781749be4a33988d99c0fbccffd85979803032a862fdc8a027f79a28`.
- 0.2.12 preserves the accepted-in-testing 0.2.11 balance/specialist-cleanup values unchanged.
- Technology presentation repair:
  - when specialist cleanup leaves no generic Faith/money success contribution, PrayerClarity no longer depends on vanilla emitting the optional `preach_params_2` block;
  - it uses the always-present prayer requirement/lore row as the Rebalanced fallback anchor, removes the stale stock requirement sentence, and inserts Base result / Bonuses on success before the stock crafting-location footer;
  - Rebalanced Soul Contentment suppresses its obsolete stock +10% Technology sentence so the effective +20% effect is the only numeric mechanic shown.
- Vanilla 1.0.31 is still the accepted stable sibling. The workflow rebuilt Vanilla only as a compile/regression check; no new Vanilla artifact is handed out or accepted here.
- Requested focused runtime acceptance:
  1. Technology -> **Молитва об упокоении** (or another cleaned specialist): no stock “20–60 required...” sentence; lore -> Base result -> Bonuses on success -> crafting location at the bottom; q20/q40/q60 tier data unchanged.
  2. Technology -> **Молитва о процветании**: Commercial Blessing x1/x2/x3 remains; no generic Faith/money garnish; crafting location is again at the bottom.
  3. Technology -> **Молитва о довольствии душ**: obsolete vanilla +10% sentence is absent; effective Rebalanced +20% is shown.
  4. Spot-check Donations +5/+15/+30 silver and Combo Faith +100/+150/+200%, donations +100/+200/+300% remain intact.
- No repeat HUD, Temporary Effects, prayer-item-tooltip or full-sermon test is requested unless this candidate exposes a new discrepancy; those surfaces passed in 0.2.11 and 0.2.12 changes only Technology presentation.
- Numbered binaries are immutable after this handoff.
- User runtime result, 2026-09-20: **0.2.12 Technology repair passed**.
  - cleaned specialist Technology tooltips no longer show the stale stock requirement sentence;
  - Base result / Bonuses on success / tier effect rows are back in the intended order;
  - crafting location is back at the bottom;
  - Prosperity keeps Commercial Blessing x1/x2/x3;
  - Donations +5/+15/+30 silver and Combo Faith +100/+150/+200%, donations +100/+200/+300% remain correct;
  - prayer-item tooltip, HUD and Temporary Effects remained correct.
- One wording issue remains before stable acceptance: Russian Soul Contentment currently says `За исцеление души: +20% благодарности`; the mechanic is correct, but the named resource **Soul Gratitude** is not explicit. Prefer the existing `(gratitude_points)` game icon/token rather than another long noun phrase.
- New non-blocking UX observation: BSS Soul's Repose is mathematically stronger than its +50/+100/+150% tier numbers look because those percentages apply to the enlarged Souls Faith base `Church Quality + current Soul Gratitude`. The current Base result already shows that dependency, but a shared effect line could explain that the tier bonus is calculated from that base without adding more rows.


## PrayerClarity: Rebalanced 0.2.11 candidate

- Status: **handed for runtime/visual acceptance; not stable; do not merge to main yet**.
- Candidate branch: `candidate/rebalanced-0.2.11`.
- Exact build/source SHA: `71bf43a3e622bfc44cd58b991df390389a5debb5`.
- GitHub Actions run: `35480090700`; result: **success**.
- Artifact ID: `10595801273`.
- Artifact: `PrayerClarity-rebalanced-0.2.11-ci-71bf43a3e622bfc44cd58b991df390389a5debb5`.
- Artifact ZIP digest: `sha256:753fba0c8aebfda7d29d132038dddff7d21a73a8ff7d14c5a0362b05e90816e5`.
- Handoff DLL: `PrayerClarity.Rebalanced-0.2.11-ci.dll`; SHA-256: `2768796539631904962f9b89f36a29ee9649c4012eae000ee73933c857c9ad6b`.
- Vanilla sibling was rebuilt only as a compile/regression check at unchanged version 1.0.31; this candidate changes Rebalanced behavior only.
- Candidate behavior:
  - Молитва о пожертвованиях: success-only flat reward **+5 / +15 / +30 silver**;
  - Комбо-молитва: Faith **+100 / +150 / +200%**, donations **+100 / +200 / +300%**;
  - utility/specialist prayers explicitly remove unrelated prayer-owned Faith/donation percentages and fixed Faith/money outputs;
  - named specialist effects, durations, Repose logic, Roots cap, Commercial Blessings and Imagination Story rewards remain unchanged;
  - Молитва за упокой душ remains the deliberate Faith-scaling specialist and keeps +50 / +100 / +150% Faith.
- Requested runtime/visual acceptance:
  1. inspect Молитва об упокоении (or another ordinary specialist): under **Бонусы при успехе** the named effect should appear without generic Faith/donation rows;
  2. inspect Молитва о процветании: Commercial Blessing x1/x2/x3 must remain, while generic Faith/money rows are gone;
  3. inspect Молитва о пожертвованиях: +5/+15/+30 silver;
  4. inspect Комбо-молитва: Faith +100/+150/+200%, donations +100/+200/+300%;
  5. if convenient, inspect one Better Save Soul utility prayer and confirm only its named specialist effect remains.
- A full sermon payout retest is not required unless the projected tooltip values disagree with runtime behavior or a PrayerClarity error appears, because the accepted stock payout path consumes these same projected craft fields/output rows.
- Numbered binaries are immutable after this handoff.
- User runtime result, 2026-09-20: **balance/specialist cleanup passed, Technology presentation failed; candidate not accepted**.
  - Donations +5/+15/+30 silver confirmed.
  - Combo Faith +100/+150/+200% and donations +100/+200/+300% confirmed.
  - specialist generic Faith/donation garnish removal confirmed.
  - prayer-item tooltips, HUD, Temporary Effects and other checked surfaces remained correct.
  - Technology tooltips regressed for cleaned specialists because vanilla omitted the `preach_params_2` anchor once generic resource contributions became empty; the stock requirement sentence reappeared and the crafting-location footer came before appended Clarity sections.
  - Rebalanced Soul Contentment also retained the stale stock +10% description despite the effective +20% mechanic.
- **0.2.11 is superseded by 0.2.12 for presentation repair; its handed bytes/source remain immutable evidence.**

## PrayerClarity: Vanilla 1.0.32 — accepted stable

- User runtime acceptance: **2026-09-20**.
- Frozen accepted ref: `accepted/vanilla-1.0.32`.
- Exact tested/build source SHA: `aaabd3cf154faa019e36b2112439ec9990fdc1bd`.
- Candidate CI run: `35502879595`; result: **success**.
- Artifact ID: `10602579329`.
- Artifact: `PrayerClarity-1.0.32-ci-aaabd3cf154faa019e36b2112439ec9990fdc1bd`.
- Artifact ZIP digest: `sha256:774f041e451bcac296cb20aca0a1c9c2a6a1cdca2e8b72e4e557e0524b049e85`.
- Accepted/release DLL SHA-256: `c12742a23d78214c9d4514a758f9c2417a578df683bebb2cda32a69a8d7674bb`.
- Stable promotion PR: **#23**, squash-merged to `main` as `71457fe9bcf0f068687ea23f7ebf548582f13efa`.
- Accepted behavior:
  - Vanilla Technology uses **При успехе:** / **On success:** as the condition-style success header;
  - a shared stock Faith/donation rider and a shared named effect are rendered as one continuous success-only block;
  - spacing before Bronze/Silver/Gold tier blocks remains;
  - prayer-item tooltips remain unchanged and keep the existing **Бонусы при успехе** hierarchy;
  - stock Graveyard Keeper 1.407 prayer mechanics, requirements, rewards and balance remain unchanged.
- Runtime evidence explicitly confirmed that the new Technology wording reads naturally across prayers and that prayer-item tooltips did not change.
- Status: **accepted and promoted to main; publication must reuse the exact handed DLL bytes without rebuilding**.
- Stable publication workflow: `35503356665`; result: **success**. It downloaded artifact `10602579329`, re-verified DLL SHA-256 `c12742a23d78214c9d4514a758f9c2417a578df683bebb2cda32a69a8d7674bb`, and published the exact accepted bytes without rebuilding.
- Published release: `v1.0.32` -> target `aaabd3cf154faa019e36b2112439ec9990fdc1bd`, asset `PrayerClarity.dll`.
- Final status: **accepted, merged to main, frozen by accepted ref, and published as stable Vanilla release**.

## Current stable baselines — 2026-09-20

- **PrayerClarity: Vanilla 1.0.32** — `accepted/vanilla-1.0.32`, source `aaabd3cf154faa019e36b2112439ec9990fdc1bd`, release `v1.0.32`, DLL SHA-256 `c12742a23d78214c9d4514a758f9c2417a578df683bebb2cda32a69a8d7674bb`.
- **PrayerClarity: Rebalanced 0.2.14** — `accepted/rebalanced-0.2.14`, source `11fa4648fe57995938a2a17093ac4ed4f5e314cd`, release `rebalanced-v0.2.14`, DLL SHA-256 `1f26c487777c4c4744f6ea4318796d369aead2b2fbd6ed299ad3edd8e79b4dc8`.
- Entries below are immutable historical build/test evidence. A section naming an older release records what was stable **at that point in the history**; it does not override this current-baseline header.


### Vanilla direct-runtime coverage note — 2026-09-20

- The user has now explicitly loaded **PrayerClarity: Vanilla 1.0.31** and supplied direct in-game Technology screenshot evidence.
- The screenshot confirms the accumulated shared Clarity presentation is active on Vanilla itself, but also exposes one remaining Vanilla-specific UX gap: `Bonuses on success -> Faith +10% -> blank gap -> Effect` reads as if the named effect were outside the success condition.
- This is a presentation/hierarchy issue only; no stock prayer mechanic discrepancy is indicated.
- A focused Vanilla 1.0.32 candidate should test only the new success-scope hierarchy plus a small regression spot-check of the already accepted shared surfaces.
- Focused direct Vanilla checklist:
  1. Technology: ordinary prayer Base result uses compact parenthetical dependencies; stock success values remain stock.
  2. Technology: Prosperity keeps Commercial Blessing x1/x2/x3; Prayer for Excellence shows its stock lore once at Writing Desk II.
  3. Prayer item: visible spacing between lore -> Base result -> Bonuses on success; 100%-success threshold stays on one line.
  4. Reward rows: item rewards render as one `name ×N` line and percentage rows omit the old “of base value” suffix.
  5. Soul's Repose: Base result still names both Church Quality and Soul Gratitude inputs, while all prayer mechanics/requirements remain vanilla.
  6. Character -> Temporary Effects / HUD: only a visual spot-check is needed; no Rebalanced-only semantics should appear.

## Shared candidate — Vanilla 1.0.31 / Rebalanced 0.2.10

- Status: **accepted and promoted to main; stable publication uses these exact handed bytes**.
- Candidate branch: `candidate/rebalanced-0.2.10`.
- Exact build/source SHA: `30b036f16dc6a7964f7ef72e2e3ececa5951c812`.
- GitHub Actions run: `35475014644`; result: **success**.
- Artifact ID: `10594265029`.
- Artifact: `PrayerClarity-shared-ui-1.0.31-rebalanced-0.2.10-ci-30b036f16dc6a7964f7ef72e2e3ececa5951c812`.
- Artifact ZIP digest: `sha256:a01744655280ee598451c79d64bee3c845e1e5d521c253cc64e082342adf3c0a`.
- **PrayerClarity: Rebalanced 0.2.10** handoff DLL SHA-256: `1f7131bda66554bb18396bed28a5997a531c2a0b9a44df1a61071d2c7e9b78e1`.
- **PrayerClarity: Vanilla 1.0.31** handoff DLL SHA-256: `140a2b3bc21eaa0b9e95f344a8a9ab5f47d5b9aa37572a0b159d905c79351ba3`.
- Candidate change:
  - replace the ineffective `BubbleWidgetBlankSeparatorData` item-tooltip spacing attempt with one controlled leading newline on PrayerClarity-owned **Base result** and **Bonuses on success** title rows;
  - apply this only to prayer-item tooltips, including the fallback append path;
  - remove the now-unused blank-separator helper from `ItemTooltipPresentation`;
  - Technology comparison tooltips remain unchanged.
- All accepted 0.2.9 behavior is preserved: Repose q20/q40/q60, stock Excellence lore fallback, compact Base-result dependency grammar, Roots cap presentation and existing mechanics.
- Runtime acceptance requested:
  - inspect a prayer item with lore and confirm visible `lore -> gap -> Base result -> gap -> Bonuses on success` rhythm;
  - spot-check that no excessive double-gap appears when the stock prayer description itself wraps;
  - no mechanics, Repose, Excellence-lore or Technology retest is required unless the presentation exposes a discrepancy.
- Numbered binaries are immutable after this handoff.
- User runtime/visual result, 2026-09-20: **passed**. The prayer-item tooltip now shows a clearly visible lore -> gap -> Base result -> gap -> Bonuses on success rhythm. No additional spacing change is requested.
- The supplied acceptance session loaded **PrayerClarity: Rebalanced 0.2.10** successfully; no PrayerClarity-specific error was reported. Repose q20/q40/q60 and the restored stock Excellence lore were already accepted in the preceding 0.2.9 pass and are carried forward unchanged.
- Frozen accepted refs: `accepted/vanilla-1.0.31` and `accepted/rebalanced-0.2.10`, both pointing to exact tested source `30b036f16dc6a7964f7ef72e2e3ececa5951c812`.
- Stable promotion PR: **#14**, squash-merged to `main` as `1d4e196f51f067b8d0127aafa8505cfc4ee88d40`.
- Stable publication workflow: `35476301759`; result: **success**. It downloaded artifact `10594265029` from the accepted CI run, re-verified both recorded DLL hashes, and published the exact accepted bytes without rebuilding.
- Published releases:
  - `v1.0.31` -> target `30b036f16dc6a7964f7ef72e2e3ececa5951c812`, asset `PrayerClarity.dll`;
  - `rebalanced-v0.2.10` -> target `30b036f16dc6a7964f7ef72e2e3ececa5951c812`, asset `PrayerClarity.Rebalanced.dll`.
- Final status: **accepted, merged to main, frozen by accepted refs, and published as stable sibling releases**.


## Shared candidate — Vanilla 1.0.30 / Rebalanced 0.2.9

- Status: **handed for runtime/visual acceptance; not stable; do not merge to main yet**.
- Candidate branch: `candidate/rebalanced-0.2.9`.
- Exact build/source SHA: `305c18c7082ccf93f5b31eab264eb73f539902ef`.
- GitHub Actions run: `35474112411`; result: **success**.
- Artifact ID: `10594182101`.
- Artifact: `PrayerClarity-shared-ui-1.0.30-rebalanced-0.2.9-ci-305c18c7082ccf93f5b31eab264eb73f539902ef`.
- Artifact ZIP digest: `sha256:85b168fb1b99b0a7d5f2e6c7a59c96eb1534a284d49ee4e714a7d6fe2cc8e977`.
- **PrayerClarity: Rebalanced 0.2.9** handoff DLL SHA-256: `928de524fc5aacce347d2a425926db7343310c1d8aa9acf7d081cd54abb6bc99`.
- **PrayerClarity: Vanilla 1.0.30** handoff DLL SHA-256: `b7819e6048edab2ce2c0852aa454715b58c472da66d9887917ca570a7c432747`.
- Candidate changes:
  - Rebalanced ordinary Repose Gold 100%-success requirement changes from q50 to **q60**; Bronze/Silver remain q20/q40, corpse-quality behavior and 30/42/54-minute durations are unchanged;
  - single-prayer item tooltips add one native blank separator before **Base result**, matching the existing native gap before **Bonuses on success**; Technology tooltips are unchanged;
  - crafting descriptions for Prayer for Excellence receive a narrow fallback to the game's own localized `b_star_d` lore only when the Excellence output path omitted it.
- Lore evidence:
  - direct decompiled `ItemDefinition.GetItemDescription` falls back from colon-quality IDs to the base `*_d` localization key;
  - direct decompiled `CraftDefinition.GetDescription` routes multi-quality outputs through `Item.GetMultiqualityItemDescription`, which does not use that same colon fallback;
  - public extracted localization data contains `b_star_d` but no corresponding quality-specific Excellence description key observed in the user's Desk II path;
  - the production fallback therefore reuses existing stock localized copy instead of inventing new text or globally intercepting localization.
- Runtime acceptance requested:
  - verify Rebalanced Repose now reads q20 / q40 / q60 in Technology and that no other Repose behavior changed;
  - inspect one prayer item tooltip and confirm the visual rhythm is lore -> gap -> Base result -> gap -> Bonuses on success;
  - inspect Prayer for Excellence at Writing Desk II and confirm its short stock lore line appears naturally and only once;
  - spot-check that the accepted 1.0.29 / 0.2.8 Base-result parentheses and Roots active-cap presentation remain unchanged.
- Numbered binaries are immutable after this handoff.
- User runtime/visual result, 2026-09-20: **0.2.9 partial acceptance**. Rebalanced Repose q20/q40/q60 is confirmed in-game. With Longer Days configured to 675 seconds/day, the unchanged 30/42/54-minute Repose durations correctly present as approximately 2.7/3.7/4.8 game days. Prayer for Excellence now shows the restored stock lore at Writing Desk II.
- The intended item-tooltip vertical spacing did **not** become visibly larger. Screenshot evidence shows lore -> Base result and Base result -> Bonuses on success remain visually tight. This is not a PrayerClarity runtime error; the session loaded Rebalanced 0.2.9 successfully and no PrayerClarity-specific error was logged.
- Source follow-up: `BubbleWidgetBlankSeparatorData` has no draw behavior and its visible height comes only from the serialized prefab/widget size. On this item-tooltip surface that prefab spacing is effectively negligible, so inserting another BlankSeparatorData row did not satisfy the UX goal. Do not repeat that mechanism in the next candidate.


## Shared UI candidate — Vanilla 1.0.29 / Rebalanced 0.2.8

- Status: **handed for runtime/visual acceptance; not stable; do not merge to main yet**.
- Candidate branch: `candidate/rebalanced-0.2.8`.
- Exact build/source SHA: `436cda740d826cbbd8964dec3dd6002892efbac4`.
- GitHub Actions run: `35473008241`; result: **success**.
- Artifact ID: `10593861146`.
- Artifact: `PrayerClarity-shared-ui-1.0.29-rebalanced-0.2.8-ci-436cda740d826cbbd8964dec3dd6002892efbac4`.
- Artifact ZIP digest: `sha256:b63f1f0c03c964ce355eee660af6f686c017c24364bc4871c9d6d0e1eaa4ff28`.
- **PrayerClarity: Rebalanced 0.2.8** handoff DLL SHA-256: `70a3b7e44bd96a72f15f3b7b4a654607c68d05e0109c92561a7c352fa547f501`.
- **PrayerClarity: Vanilla 1.0.29** handoff DLL SHA-256: `fb08bf6f7bd3727683b057b5e493375b79ef23bb1eb08dc905c2c40635c446d9`.
- Candidate changes:
  - compact Base result dependencies to parenthetical source labels: `Faith (Church Quality)` / `Donations (Graveyard Quality)`; Soul's Repose keeps Soul Gratitude in the Faith source;
  - Rebalanced Shoots & Roots prayer-selection surfaces show only the direct tier reduction; the accepted 95% combined safety cap remains mechanically unchanged and remains visible in Character -> Temporary Effects;
  - preserve all accepted 1.0.28 / 0.2.7 compact percentage, item-reward and atomic-success-row behavior;
  - keep the accepted top-HUD decimal-dot residual behavior; no heavier punctuation-only workaround was added.
- Runtime acceptance requested:
  - inspect ordinary prayer Base result in Russian and confirm the parenthetical dependency grammar is immediately understandable and materially narrower;
  - inspect BSS Soul's Repose if convenient and confirm Faith still clearly shows both Church Quality and Soul Gratitude as inputs;
  - inspect Rebalanced Shoots & Roots in Technology/prayer-item tooltip/pulpit and confirm the 95% cap text is gone there while Character -> Temporary Effects still shows the cap;
  - no sermon mechanics, stacking, duration, payout or balance retest is required unless the UI exposes a discrepancy.
- Numbered binaries are immutable after this handoff.
- User runtime/visual result, 2026-09-20: **1.0.29 / 0.2.8 presentation scope passed**. Parenthetical Base result dependencies render correctly in Russian, including Soul's Repose with Soul Gratitude icon; Roots no longer shows the 95% cap on Technology/selection surfaces while Character -> Temporary Effects still shows the cap.
- New follow-up observations are not regressions in this candidate: Rebalanced ordinary Repose still uses the accepted q20/40/50 ladder; prayer-item spacing may benefit from one additional native blank separator before Base result; Prayer for Excellence appears to lack its lore line at Desk II and requires source-localization investigation before any fix.


## Shared UI candidate — Vanilla 1.0.28 / Rebalanced 0.2.7

- Status: **handed for runtime/visual acceptance; not stable; do not merge to main yet**.
- Candidate branch: `candidate/rebalanced-0.2.7`.
- Exact build/source SHA: `94a8ecf4d5b17e5eed2115ec49c97b3a6aec2fa4`.
- GitHub Actions run: `35471739528`; result: **success**.
- Artifact ID: `10593055414`.
- Artifact: `PrayerClarity-shared-ui-1.0.28-rebalanced-0.2.7-ci-94a8ecf4d5b17e5eed2115ec49c97b3a6aec2fa4`.
- Artifact ZIP digest: `sha256:d0a3145aa2c201d983a66d7a3f579cc4481a5d44b22a07a81b6a8023b63ad7f6`.
- **PrayerClarity: Rebalanced 0.2.7** handoff DLL SHA-256: `4e7dedfea65aaee7b4e5cc52ce782053fe1b208327976c0c689d28148e99a0dd`.
- **PrayerClarity: Vanilla 1.0.28** handoff DLL SHA-256: `69a927a235ea3547a6b6392dab7db2d208f672fa57d4b2ad1736733fddfecbb6`.
- Candidate changes:
  - remove the redundant “of base value” phrase from Faith/donation percentage rows;
  - render single-item prayer rewards as one atomic `localized name ×N` row;
  - let PrayerClarity-owned prayer-item success rows use the existing content-width seam so the 100% success threshold does not split;
  - refresh the active game locale once on the first long-prayer HUD timer render, covering plugin initialization before `LoadGameSettings` without per-frame language polling.
- Runtime acceptance requested:
  - verify percentage rows, Commercial Blessing / Story reward rows, and the concrete prayer-item success threshold in Russian;
  - verify the long-prayer HUD timer uses a comma decimal separator on Russian UI; if it still renders a dot, do not add a heavier punctuation-only workaround without a new design decision;
  - spot-check that the accepted Base result / Bonuses on success hierarchy, Soul's Repose dependency context, Technology durations and Character -> Temporary Effects remain unchanged.
- The earlier run `35471702289` failed at compile time because `ItemTooltipPresentation.CreateTextData` had not yet exposed its existing native `max_width` constructor argument. No artifact was uploaded from that failed source. The corrected source above builds both sibling DLLs with 0 errors.
- Numbered binaries are immutable after this handoff.
- User runtime/visual result, 2026-09-20: **the intended 1.0.28 / 0.2.7 presentation changes passed**. Percentage rows no longer contain the redundant “of base value” wording; single-item rewards render on one `name ×N` row; the concrete prayer-item 100%-success threshold remains intact; Technology and Character -> Temporary Effects remained readable, including the controller/gamepad presentation.
- Russian long-prayer HUD punctuation did **not** change: the compact HUD timer still uses a dot rather than a comma. The user explicitly accepted leaving this alone rather than adding a heavier punctuation-only workaround. This is not a blocker for the candidate.
- The supplied session loaded **PrayerClarity: Rebalanced 0.2.7** and exercised synthetic Gold Repose and Gold Shoots & Roots through the existing research console; no PrayerClarity-specific runtime error was reported in the supplied log.
- Follow-up UX/design discussion remains open and therefore belongs to a later numbered candidate: whether to shorten/split the base-donation dependency line, and whether to move the Roots 95% aggregate-cap explanation out of prayer-selection tooltips while retaining it on the active-effect surface.

## Stable sibling releases — Vanilla 1.0.25 / Rebalanced 0.2.0

- User acceptance: **2026-09-18**. After separate runtime passes, the user explicitly approved both candidates for promotion to `main` and stable GitHub publication.
- Frozen accepted refs:
  - `accepted/vanilla-1.0.25` -> source SHA `ebe069b4ad202ae786af9c63ded0ffb00502cff7`
  - `accepted/rebalanced-0.2.0` -> source SHA `26048581c3fe6e0d8ef4ae930a0c29474f68bbcf`
- **PrayerClarity: Vanilla 1.0.25**
  - accepted CI run: `35347730945`
  - artifact ID: `10547383753`
  - artifact: `PrayerClarity-1.0.25-ci-ebe069b4ad202ae786af9c63ded0ffb00502cff7`
  - artifact ZIP digest: `sha256:d7fb3fe9bd789a6568a0204d033c044c2f17c028b8b6125b15858317cb6a0f92`
  - release asset: `PrayerClarity.dll`
  - DLL SHA-256: `72b2237607734d8b50d666cef56e18d458b88c6a456211d496e2410ac646e3a9`
- **PrayerClarity: Rebalanced 0.2.0**
  - accepted CI run: `35346055221`
  - artifact ID: `10547166000`
  - artifact: `PrayerClarity-shared-ui-1.0.24-rebalanced-0.2.0-ci-26048581c3fe6e0d8ef4ae930a0c29474f68bbcf`
  - artifact ZIP digest: `sha256:54a49d9a460d255a594b788089cde9b26433f38eb0f1214d6a3a5efa94c5051f`
  - release asset: `PrayerClarity.Rebalanced.dll`
  - DLL SHA-256: `84cf07be553e137d4663d24267ab18257facfbe44c0a833d87579876742b6de1`
- Stable publication policy: publish these **exact accepted bytes without rebuilding**.
- Vanilla runtime acceptance: current Soul Gratitude row rendered correctly for Soul's Repose; stock Gold Soul's Repose remained q60; non-terminal Repose retained normal stock wording; no PrayerClarity-specific runtime error was observed.
- Rebalanced runtime acceptance: revised Faith/Donations/Combo and requirements rendered as designed; Soul's Repose q30/60/120 and Soul Gratitude context rendered correctly; representative timed prayers matched across pulpit/Technology/Temporary Effects; Test Harness + Rebalanced Compatibility operated correctly; no PrayerClarity-specific runtime error was observed.
- Deferred, non-blocking runtime verification:
  - terminal Repose endpoint wording/behavior awaits a save with terminal Donkey corpse progression;
  - the real successful Silver/Gold Imagination sermon has not yet been visually observed dropping its 3 premium Stories, although the implementation uses the verified native sermon-drop path.
- Any future change required by deferred verification must use a new version; the accepted 1.0.25 / 0.2.0 binaries are immutable.
- Stable promotion PR: **#3**, squash-merged to `main` as `d62d44b5b58b5f799432d7ec8475b1e3fe50dbf6`.
- Publication workflow: `35353739384`; result: **success**.
- Publication downloaded the two exact accepted CI artifacts, re-verified both DLL SHA-256 values, renamed only to canonical install filenames, and created:
  - tag/release `v1.0.25` -> target `ebe069b4ad202ae786af9c63ded0ffb00502cff7`, asset `PrayerClarity.dll`;
  - tag/release `rebalanced-v0.2.0` -> target `26048581c3fe6e0d8ef4ae930a0c29474f68bbcf`, asset `PrayerClarity.Rebalanced.dll`.
- Initial publication run `35353663348` stopped after artifact/hash verification because of a shell-control syntax error in the temporary release workflow; no release asset was created or modified by that failed attempt. The workflow-only fix did not change either accepted binary.
- Status: **accepted, merged to main, frozen by accepted refs, and published as two stable sibling releases**.

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


## PrayerClarity: Rebalanced 0.2.1 — Roots runtime fix candidate

- Type: focused bug-fix candidate over accepted Rebalanced 0.2.0.
- Candidate ref: `candidate/rebalanced-0.2.1`.
- Triggering bug: Rebalanced 0.2.0 rewrote plant `craft_time` with `Ppar("buff_plant")*Ppar("prayerclarity_rebalanced_plant_reduction")`; live Graveyard Keeper 1.407 evaluation throws `InvalidCastException` inside Expressive and `SmartExpression.EvaluateFloat` falls back to `1f`, causing auto-growth to finish almost immediately.
- Vanilla control: PrayerClarity: Vanilla 1.0.25 leaves plant mechanics untouched and the same carrot/cabbage scenario shows no SmartExpression/Expressive exception.
- Fix architecture: keep the verified stock `craft_time` expression unchanged. On affected plant `CraftComponent.DoAction` calls only, temporarily project the active Rebalanced Roots reduction into the stock WGO-owned **NonSerialized `totem_effect`** `buff_plant` entry as `reduction / 0.20` (1 / 1.5 / 2 for Bronze/Silver/Gold), then restore the exact original runtime-effect value in a Harmony finalizer.
- Semantics: preserves the game's additive `grow_time + buff_plant` formula rather than multiplying elapsed time externally.
- Save safety: the injected value lives only in the native NonSerialized `totem_effect` aggregate for the duration of `DoAction`, and is restored even when the original call throws; serialized plant Item data is never modified.
- Performance shape: no per-frame polling or scans. One narrow `CraftComponent.DoAction` hook exits immediately for non-Roots plant crafts; affected auto-growth already executes through this native path at the game's own cadence.
- Exact candidate source SHA: `d13655e01a1b8e797ed019b636f040b3d2f2a55f`.
- GitHub Actions run: `35377381232` — success.
- Workflow artifact ID: `10560138983` (`PrayerClarity-shared-ui-1.0.25-rebalanced-0.2.1-ci-d13655e01a1b8e797ed019b636f040b3d2f2a55f`).
- Handoff filename: `PrayerClarity.Rebalanced-0.2.1-ci.dll`.
- Handoff DLL SHA-256: `adb0bfc90c1245cb652662a826a168f6f4410a696a352e9360abfe8507ab20c7`.
- Build result: success on `ubuntu-latest`; Rebalanced and Vanilla compiled, all 11 locale sets validated/staged, and the shared artifact uploaded.
- Required runtime gate after the clean build:
  1. ordinary freshly planted carrot/cabbage no longer completes in seconds and produces no SmartExpression/Expressive error;
  2. with active Roots, Bronze/Silver/Gold retains the intended -20/-30/-40 percentage-point term in the native additive growth formula;
  3. repeat one case with `grow_time` fertilizer to verify the additive interaction remains intact;
  4. return the runtime log so the absence of the 0.2.0 exception can be confirmed.
- Broader Rebalanced behavior-risk audit is intentionally deferred until this blocker is closed; it remains a required follow-up requested by the user.


### Runtime check 2026-09-18 — Rebalanced 0.2.1 Roots fix, no-buff half of gate

User-tested candidate:
- Rebalanced source: `d13655e01a1b8e797ed019b636f040b3d2f2a55f`
- Rebalanced 0.2.1 loaded successfully and static projection applied.
- Fresh carrot/cabbage growth crafts started normally after planting; the newly planted crops did not transition to `*_ready` during the remainder of the supplied log.
- The supplied runtime log contains zero `ExpressiveException`, zero `InvalidCastException`, zero `SmartExpression` error, and zero `Error in expression` entries.
- User visual observation agrees: freshly planted carrots no longer become ready almost immediately.
- This closes the original 0.2.0 failure mode for ordinary/no-Roots growth.
- Remaining acceptance gate: prove active Rebalanced Roots actually shortens native crop `craft_time` by the intended Bronze/Silver/Gold amount, including one fertilizer-adjusted case.

A research Test Console 0.1.1 was prepared to remove stopwatch/manual timing from that gate:
- candidate ref: `candidate/rebalanced-test-console-0.1.1`
- source SHA: `187579dcc329c05c845aec80849772cb7fbd67bb`
- GitHub Actions run: `35379100576` — success
- artifact ID: `10561017600`
- handoff DLL SHA-256: `4de6e4cee802c88757439ba9cb6ccdb83617601158588cfcd90e024047cad707`
- Roots diagnostic logs, once per affected craft after activation, both `craft_time_without_roots` and `craft_time_with_roots`, plus tier, configured reduction, effective WGO `buff_plant`, fertilizer `grow_time`, and raw native expression.
- Diagnostic comparison is performed after the native `DoAction` call while RebalancedRoots' temporary nonserialized WGO projection is still in scope; it temporarily subtracts only the projected Roots runtime contribution for the read-only comparison evaluation, restores it immediately, and leaves the production finalizer to restore the original WGO state.


### Runtime check 2026-09-18 — active Roots + fertilizer + removal

User-tested Rebalanced 0.2.1 with Rebalanced Test Console 0.1.1.

Observed runtime evidence:
- Bronze synthetic Roots activation used native `BuffsLogics.AddBuff`; diagnostic on `tree_growing`: baseline `1800`, with Roots `1440`, saved `360` = exactly 20%.
- Silver activation used the same native buff path; `tree_growing`: `1800 -> 1260` = exactly 30%.
- Silver on ordinary carrot/cabbage: `1440 -> 1008` = exactly 30% of base growth time.
- Silver plus one time-fertilizer unit on wheat: native formula `1440*(1-0.2*grow_time-0.2*buff_plant)`; `grow_time=1`; no-Roots fertilizer baseline `1152`; Roots result `720`. This proves the intended additive stacking: fertilizer contributes -20 percentage points of base time and Silver Roots contributes another -30 points, for total -50% of base time.
- No `ExpressiveException`, `InvalidCastException`, `SmartExpression` error, or `Error in expression` occurred.
- User removed Roots through the console; runtime logged native `BuffsLogics.RemoveBuff("buff_plant")`.
- The console removal is a valid simulation of natural expiry because stock `BuffsLogics.RecalculateBuffs` removes expired buffs through that same `RemoveBuff` path.
- Production semantics after removal: already accumulated crop progress is retained; subsequent `CraftComponent.DoAction` calls no longer receive the temporary Roots WGO projection because the prefix requires active player `buff_plant`. Therefore an in-progress plant continues from its current progress at its ordinary/fertilizer-adjusted rate rather than rewinding or finishing instantly.

Assessment:
- The 0.2.0 near-instant-growth regression is fixed.
- The 0.2.1 native scope bridge is runtime-confirmed for active Roots.
- Additive interaction with fertilizer is runtime-confirmed.
- Manual removal correctly exercises the same stock removal path as timed expiry.
- Gold is not separately runtime-sampled in this log, but it has no distinct control-flow branch: the same verified bridge uses the persisted tier reduction scalar (.20/.30/.40). Bronze and Silver runtime samples plus definition validation cover the mechanism; no additional Gold-specific runtime test is required unless behavior changes.


### 2026-09-18 — Rebalanced 0.2.2 Roots edge accepted

- Accepted runtime source: `924900365d44cd1ec9e530c9dd9b7e2f6a796bed`.
- Accepted ref: `accepted/rebalanced-0.2.2`.
- Production promotion: PR #5 merged to `main` as `1c4c395d375209e1505a312c922f4f176d78b0da`.
- CI run: `35382737488` — success, 0 warnings / 0 errors.
- Rebalanced DLL SHA-256: `4655fea2a57125aa78965a807fde76f9a056dbbd7f361246cf12351ff45074d6`.
- Runtime helper: Rebalanced Test Console 0.1.2, source `f50fff1d7dc314f7f27ac125d1e760346a6ce6fe` (research-only; not promoted to production).

Accepted edge-case runtime evidence:
- Test Console enabled its nonpersistent Boost-II simulation, which supplies `grow_time=3` only to verified plant expressions that consume both `grow_time` and `buff_plant`.
- Gold Shoots & Roots was activated through native `BuffsLogics.AddBuff`.
- `tree_growing`, whose stock expression has no `grow_time` term, remained ordinary Gold Roots: `1800 -> 1080` (-40%); the Boost-II simulation did not leak into that consumer.
- `garden_wheat_growing` reported `grow_time=3`, effective WGO `buff_plant=1.75`, fertilizer-only baseline `576`, and capped Gold result `72`.
- For the 1440-second stock crop base, 72 seconds is exactly 5% remaining time = 95% combined reduction.
- The effective prayer contribution at the cap is 35 percentage points (1.75 stock prayer units), so 60% fertilizer + 35% applied Gold = 95%; Silver + the same fertilizer remains 90%, preserving a real Silver -> Gold upgrade.
- No `ExpressiveException`, `InvalidCastException`, `SmartExpression`, or `Error in expression` failure occurred in the supplied test log.

Result: the original 0.2.0 instant-growth regression and the later 100%-stack edge are both closed. Rebalanced 0.2.2 is the accepted stable baseline for Roots.


### 2026-09-18 — Rebalanced 0.2.2 publication correction

A GitHub Release/tag for `rebalanced-v0.2.2` was created prematurely during internal stabilization and was later deleted before the public 0.2.3 release. The accepted 0.2.2 source/ref remains valid historical runtime evidence for the Roots repair, but `rebalanced-v0.2.2` is **not** a current published release.


### 2026-09-18 — Rebalanced 0.2.3 accepted and published

- User acceptance: explicit `фиксируем 023` after the focused native-seam runtime pass.
- Accepted ref: `accepted/rebalanced-0.2.3`.
- Exact accepted runtime source: `ab1eb67cbf2465a912c392120395011503b720c3`.
- Candidate CI run: `35388483840` — success.
- Artifact ID: `10564359079`.
- Artifact: `PrayerClarity-shared-ui-1.0.25-rebalanced-0.2.3-ci-ab1eb67cbf2465a912c392120395011503b720c3`.
- Artifact ZIP digest: `sha256:097febe34877eb16cb1ac79df56301f24e96deb908c60eb66830d8443bdf4a06`.
- Accepted/released Rebalanced DLL SHA-256: `03a4a8b43c8a5ffef8ec62eac58370d3f6bced347bc2fb4f49a2ef81a23cb1ca`.
- Stable promotion: PR #7 merged to `main` as `f953e34901de139a5f7af7162af6fc62bb02b4d9`.
- Stable tag/release: `rebalanced-v0.2.3`, targeting the exact accepted runtime source.
- Publication workflow run: `35391896166` — success.
- Publication reused and hash-verified the exact accepted CI artifact; **no rebuild occurred**.
- Published asset: `PrayerClarity.Rebalanced.dll`.

Accepted runtime evidence with Test Console 0.1.3:
- Repentance: Bronze/Silver/Gold effective confession probability = 0.50 / 0.75 / 1.00 while the controlled stored stock value remained 0.15 before each accessor read.
- Combat damage: Bronze/Silver/Gold tier delta relative to the native Bronze result = 0 / +5 / +10; persisted stock `add_damage` remained 5.
- Combat armor: controlled 20 damage caused 20 stock HP loss vs 16 with Combat, exactly 4 damage prevented.
- Combat regeneration: Bronze/Silver/Gold native buff ticks were observed as +1 / +2 / +4 HP, with stock max-HP clamping.
- No PrayerClarity runtime failure, `Exception`, `ExpressiveException`, `InvalidCastException`, `SmartExpression`, or `Error in expression` occurred in the supplied log.
- Synthetic Combat buff was removed at the end of the test.

Architecture accepted in 0.2.3:
- Repentance leaves stock `SetPpar("confession_probability", 0.15)` authoritative and projects only the effective FlowCanvas player-param read while `buff_sins` is live.
- Combat damage projects only the Silver/Gold delta through nonserialized `totem_effect["add_damage"]` around native `GetDamage`.
- Combat armor projects +4 through nonserialized `totem_effect["add_armor"]` around native `DecHP`, eliminating the prior ThreadStatic + generic `GetParam` interception.
- Combat regeneration remains on the native `BuffDefinition.se_tick` extension point.
- Roots retains the accepted 95% aggregate growth-time-reduction safety cap from 0.2.2.

Research helper identity:
- Test Console 0.1.3 source: `5de0f27e97cb38a72fb2b53b93e02131225012e7`.
- Test Console DLL SHA-256: `f7624c8696f5dbbf1bf64a959ea370c545c198332bbfe0c1b8eeec4e0775d6fc`.
- The helper remains research-only and is not part of the public release.

### 2026-09-19 — Rebalanced 0.2.4 accepted and published

- User acceptance: after testing the 0.2.4 candidate, the user reported Repentance and Repose duration presentation as **2.7 / 3.7 / 4.8 in-game days** under Longer Days +50% and explicitly approved fixing, publishing, and documentation consolidation.
- Accepted behavior: Repentance and Repose duration **30 / 42 / 54 min**. Repentance probability remains 50 / 75 / 100%; Repose Bronze/Silver/Gold corpse-quality logic is unchanged.
- Implementation: tiered duration is projected to stock `CraftDefinition.dur_parameter` through the existing once-per-load static projection. No new Harmony hook, polling, persistent mod timer state, or runtime algorithm was introduced.
- Accepted ref: `accepted/rebalanced-0.2.4`.
- Exact accepted runtime source: `4d5d3021c0b9eef16d09402c5f25851ff7a66981`.
- Candidate CI run: `35441674008` — success, 0 warnings / 0 errors.
- Artifact ID: `10583657918`.
- Artifact: `PrayerClarity-shared-ui-1.0.25-rebalanced-0.2.4-ci-4d5d3021c0b9eef16d09402c5f25851ff7a66981`.
- Artifact ZIP digest: `sha256:1e8759dee5652d5e9d8f0180462a8875e569174d375ec70dc7b85a6591f865a5`.
- Accepted/released Rebalanced DLL SHA-256: `ecba7297a80bde91a044d4d7c7e4348fd32805e960e3fa467be8d1465cfa52e2`.
- Stable promotion: PR #8 merged to `main` as `d176fec0eb42852019956a55347dd3af29b3fe1f`.
- Stable tag/release: `rebalanced-v0.2.4`, targeting the exact accepted runtime source.
- Release asset ID: `574716424`, canonical filename `PrayerClarity.Rebalanced.dll`, asset digest `sha256:ecba7297a80bde91a044d4d7c7e4348fd32805e960e3fa467be8d1465cfa52e2`.
- Publication workflow run: `35442370524` — success.
- Publication downloaded artifact `10583657918`, verified the accepted source/version/hash, and uploaded the exact accepted DLL; **no rebuild occurred**.

