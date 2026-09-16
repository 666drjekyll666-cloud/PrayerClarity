# PrayerClarity — Vanilla Fixes specification

Status: research specification. No Vanilla Fix mechanics are implemented or accepted yet.

Baseline:

- target game: Graveyard Keeper 1.407;
- accepted Clarity runtime/source: `493d2168489af80b5c1305f7ff1435e2ee1dd0d7` (`accepted/clarity-1.0.0`);
- current stable UI/presentation remains PrayerClarity 1.0.0;
- stock mechanics remain canonical in `PRAYER_MECHANICS.md`;
- Balance/Rework hypotheses remain separate in `PRAYER_REBALANCE_OPTIONS.md`.

A change belongs here only when stock behavior is directly shown to be broken/disconnected and the intended stock behavior **including its relevant magnitude/algorithm** can be recovered without inventing design.

## Current classification

| Prayer / case | Stock 1.407 fact | Diagnosis | Layer |
| --- | --- | --- | --- |
| Shoots & Roots (`b_plant` / `buff_plant`) | Prayer applies `buff_plant=1` to player data; affected growth expressions contain an existing `-0.2*WGOpar("buff_plant")` term, but `WGOpar` reads the growing/workbench WGO; no stock propagation path from player to those WGOs was found | Concrete parameter-owner wiring mismatch; both intended scope relationship and dormant `-0.2` term are recoverable | **Vanilla Fix candidate** |
| Repentance (`b_sins` / `buff_sins`) | Prayer applies a timed `buff_sins`; no gameplay consumer was found across inspected code, loaded FlowCanvas graphs, or GameBalance data | Special role is disconnected, but no surviving 1.407 algorithm/magnitude says how the buff should alter confessions | **Balance / Rework only** |
| Failed-sermon base donations | Stock graph supplies a nominal `0.5` participation chance on failure, but the helper uses integer `Random.Range(0,1)`, so all visitors still participate and the full base donation pool is paid | Implementation mismatch is real, but project policy explicitly preserves the player-favourable stock result | **Intentionally not fixed** |
| Faith / Donations / Combo | Verified mechanics work; specialists become strategically compressed by Combo once recipe/church gates cease to matter | Design/balance issue, not broken wiring | **Balance / Rework** |
| Repose | Verified `body_max +1` reaches live Donkey corpse generation; quality mostly changes duration | Working but potentially under-compelling/reliability-limited | **Balance / Rework** |
| Retribution / Protection | Verified +5 damage / +4 armor effects work | Working but structurally unattractive as two deep book prayers / two weekly slots | **Balance / Rework** |
| Imagination / Excellence | Verified `craft_q` consumers work | Working; proposed quality scaling changes magnitude | **Balance / Rework** |
| Prosperity / BSS Soul's Repose / Soul Contentment / Thorough Cleansing | Principal special mechanics are directly connected and quantified | No proven Vanilla Fix requirement | **Stock unless a later Rework is explicitly accepted** |

## VF-01 — Shoots & Roots scope repair

### Stock 1.407 fact

Affected growth-time definitions already contain the prayer term, for example:

`1440 * (1 - 0.2*WGOpar("grow_time") - 0.2*WGOpar("buff_plant"))`

and:

`1080 * (1 - 0.2*WGOpar("buff_plant"))`.

Direct runtime evidence established:

- `WGOpar(name)` reads `SmartExpression._wgo.GetParam(name, 0)`;
- `CraftComponent` evaluates `craft_time` with the growing/workbench WGO as the expression WGO and the player as the character;
- successful prayer-buff application places `buff_plant=1` on player data;
- no stock path was found that copies that player parameter to affected growing/workbench WGOs.

Therefore the normal prayer path cannot satisfy the existing `WGOpar("buff_plant")` read.

### Proven problem

The prayer has a real timed buff and the game has a real growth-time term for that buff, but they use different parameter owners. The special effect is effectively inert in stock 1.407 under the inspected path.

### Classification

**Vanilla Fix.** No new gameplay magnitude is needed: the existing stock coefficient is `-0.2` and the existing prayer durations remain `36 / 72 / 108` in-game minutes by prayer quality.

### Exact Fixed Vanilla behavior

Repair **only the parameter ownership of the existing prayer term** in affected stock growth expressions.

Semantically, the stock `buff_plant` read must come from player prayer state (`Ppar("buff_plant")` or an exactly equivalent native player-scope evaluation) instead of the growing/workbench WGO.

Preserve everything else in each stock expression:

- the existing `-0.2` coefficient;
- any independent `grow_time` term;
- existing base times;
- existing rounding/evaluation behavior;
- the stock 36/72/108-minute prayer durations;
- stock sermon success/failure behavior and resource rewards.

Do **not** reinterpret the repair as a generic multiplicative `current time * 0.8` rule. Where another stock term such as `grow_time` is present, keep the original additive expression structure exactly and change only the owner used for `buff_plant`.

Do **not** quality-scale the magnitude in Fixed Vanilla. `-20/-30/-40%` belongs only to the future Rebalanced layer.

### Implementation constraint still open

The current implementation audit has not yet proved the safest lifecycle for replacing already-loaded `SmartExpression` source / compiled state without a broad balance reload.

Before production code, direct 1.407 inspection must establish one narrow, fail-safe mechanism, preferably in this order:

1. replace an affected `craft_time` `SmartExpression` with a newly constructed native expression using the corrected player-scope token, if the native type exposes a safe construction path;
2. otherwise use a verified native setter/recompile path on the existing expression;
3. only if neither exists, evaluate a narrower Harmony seam that redirects this specific `buff_plant` lookup without changing unrelated `WGOpar` semantics.

Do not mirror `buff_plant` into world objects, scan growing objects, poll per frame, or maintain a second growth model.

### Runtime acceptance condition

A future Fixed Vanilla candidate is accepted only if all of the following are demonstrated in Graveyard Keeper 1.407:

- with Shoots & Roots inactive, representative affected growth timing remains stock-identical;
- after a successful Shoots & Roots sermon, representative affected growth definitions consume the existing `-0.2` prayer term while the buff is active;
- at least one expression containing an independent stock modifier (such as `grow_time`) preserves that modifier and its original additive interaction;
- expiration/removal of the prayer buff restores stock timing without persistent WGO/player parameter drift;
- save/load during the active effect does not leave permanent growth modification after the buff ends;
- the accepted Clarity surfaces describe the **effective Fixed Vanilla behavior** rather than the old stock-broken state;
- disabling/removing PrayerClarity returns the save/world to valid stock behavior;
- no per-frame polling, broad Unity scan, or recurring world-object synchronization is introduced.

The user should not be asked to perform this test until a numbered candidate exists with exact source SHA, frozen ref, clean build evidence and artifact hash.

## Deliberately deferred from Vanilla Fixes

### Repentance

The evidence is enough to call the stock special effect disconnected, but not enough to recover a vanilla magnitude. Stock confessional chance is independently known, yet there is no surviving 1.407 consumer that says whether `buff_sins` was intended to add, multiply, replace, guarantee, or otherwise transform that chance.

Any working rule such as `30/50/70%` is therefore a **new Balance/Rework design** and must never be described as Fixed Vanilla.

### Failed-sermon donations

The integer-random mismatch remains documented as a stock implementation anomaly, but PrayerClarity intentionally preserves full base donations on sermon failure in every future profile. This is a permanent product decision unless explicitly reopened by the user.

## Next research gate

VF-01 is mechanically specified. The only blocker before a narrow production implementation is the exact `SmartExpression` replacement/recompile lifecycle on the verified 1.407 assembly.

Resolve that lifecycle with direct assembly/type inspection first. If the current accepted evidence cannot expose it, use one read-only probe whose sole purpose is to dump the relevant `SmartExpression` constructors/fields/methods and the state of representative affected `craft_time` instances. Do not build a mechanics candidate until this gate is closed.

## SmartExpression lifecycle probe 0.1.8 — ready for runtime evidence

- Type: research-only, read-only inspection probe; it does not contain a Vanilla Fix or any Balance/Rework mechanics.
- Research branch: `research/vanilla-fixes-roots`.
- Exact build source SHA: `ff2267e4c87e28c2691e9406f8d4ade318bd1716`.
- Frozen candidate ref: `candidate/smartexpression-probe-0.1.8`.
- GitHub Actions run: `35038610148`.
- Workflow result: successful `net472` build on `ubuntu-latest`, 0 warnings and 0 errors.
- Artifact ID: `10423839476` (`PrayerClarity-SmartExpressionProbe-0.1.8`).
- Artifact ZIP digest: `sha256:9ced7e06bceb22162e2f831adeee725d85a19dd24650b62645f53ea1a289dbda`.
- Handoff DLL: `PrayerClarity.SmartExpressionProbe.0.1.8.dll`.
- DLL SHA-256: `875ec7cf8e56152b71ac60b77c9ecb0026c267715abd750ba7860a9f7e80f8cd`.
- Expected evidence file: `BepInEx/PrayerClarity-smartexpression-0.1.8.txt`.
- Probe question only: enumerate the real 1.407 `SmartExpression` construction/reinitialization contract and loaded `buff_plant` expression state so the project can choose the least invasive safe repair seam.
- Required user action: install this probe DLL alongside the stable PrayerClarity 1.0.0, launch Graveyard Keeper far enough for a save to load and remain in-game for several seconds, then return the generated text file. No sermon, crop planting, save modification, UI calibration, or controlled gameplay sequence is required.
- Status: **ready for one runtime evidence capture; no mechanics acceptance test is requested yet**.
