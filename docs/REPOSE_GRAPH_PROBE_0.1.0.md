# Repose Graph Probe 0.1.0 — handoff manifest

Status: **accepted research-only load-time reflection evidence**. Not production PrayerClarity behavior.

## Question

After the accepted Repose Owner Probe observed an ordinary delivery through runtime `Flow_DropBody` node ID 108 instead of the previously inferred static IDs 210/232, map every live `Flow_DropBody` node in the loaded `npc_donkey` graph and record enough connection context to distinguish the normal dynamic-range branches from the fixed story/intro branch.

## Scope and safety

The probe:

- performs no Harmony patching;
- executes no FlowCanvas nodes or game scripts;
- mutates no balance data, graph data, world objects, tier inputs, RNG, corpse result, or save state;
- once per second after load, looks only for the already-live donkey `FlowScriptController` until it appears;
- once found, performs one reflection audit and stops;
- enumerates the live graph's Node objects, filters `Flow_DropBody`, and logs ID/UID plus input/connection fingerprints.

No donkey delivery, prayer, combat, or other gameplay action is required. Loading an existing save is sufficient.

Output:

`BepInEx/PrayerClarity-repose-graph-probe-0.1.0.txt`

## Accepted runtime result — 2026-09-17

The report matches Graveyard Keeper 1.407 (`Assembly-CSharp` MVID `6f50b8e7-156b-49ac-bbe8-7505894b2364`) and satisfies the load-only/no-mutation contract.

The live `[wgo] donkey` controller owns graph `npc_donkey`, with 290 nodes total and exactly three `Flow_DropBody` nodes.

The three nodes separate cleanly by **connection topology**, not by historic numeric IDs:

- one fixed/story node has default/fixed `Tier min` / `Tier max` values and no dynamic player-param chain;
- two normal-delivery nodes feed `Tier min` and `Tier max` through `IntegerAdd` nodes whose operands are `Flow_GetPlayerParamInt` nodes;
- one of the normal branches additionally resolves `WGO to drop` through `Flow_FindWGO`; this is a branch-shape difference, not a different corpse-tier source.

The numeric runtime IDs in this load are 71 (fixed/story), 97 and 108 (dynamic normal delivery), but **production must not hardcode those IDs or runtime UIDs**. Earlier static evidence and the previous execution logger already demonstrated that these identifiers are not a stable cross-load/source contract.

### Accepted production predicate

Rebalanced Repose may target only an executing `Flow_DropBody` when all of the following hold:

1. owner graph is the live donkey graph (`npc_donkey`) / donkey controller context;
2. both `Tier min` and `Tier max` inputs are dynamically connected through the normal player-parameter tier chain (the `IntegerAdd` + `Flow_GetPlayerParamInt` fingerprint);
3. the fixed/default story/intro branch is therefore excluded by construction.

This closes caller isolation without a broad global-body predicate.

### Implementation shape

Keep stock `GameSave.GenerateBody(...)` and stock body-definition RNG authoritative.

At the targeted normal-delivery callback only:

- Bronze: leave the evaluated tier range unchanged;
- Silver: 50% leave the evaluated stock range unchanged, 50% force `tier_min = tier_max`;
- Gold: force `tier_min = tier_max`;
- never raise `tier_max`, so the progression ceiling is preserved;
- selection among vanilla body definitions inside the resulting tier remains stock RNG.

A practical low-mechanism implementation may use the narrow `Flow_DropBody` callback context to establish a scoped Repose-delivery state around the synchronous `GenerateBody(...)` call, then adjust only that call's `tier_min` argument. If implemented this way, the state must be cleared in postfix/finalizer and must never affect unrelated `GenerateBody` calls.

**Repose caller-isolation gate: CLOSED.** No further donkey/repose research probe is required unless implementation evidence contradicts this topology.

## Build identity

- Frozen ref: `candidate/repose-graph-probe-0.1.0`
- Exact source SHA: `548fea74e1bf96bd11eb1b26dc0a846a7bd41af1`
- GitHub Actions run: `35225417773`
- Job: `105215629017`
- Artifact: `PrayerClarity-ReposeGraphProbe-0.1.0`
- Artifact ID: `10497868477`
- Artifact ZIP SHA-256: `92e505703c947f86de9370aecb8591e3b2eb268ae28533b1597e6db71dd1e119`
- Handoff filename: `PrayerClarity.ReposeGraphProbe.0.1.0.dll`
- Handoff DLL SHA-256: `d98509544b54b4fc123667e12d70e19d1fe26af25ba161f7f3f02608e9f79b76`

## Probe closure

This probe answered the remaining Repose caller-isolation question and should not be rerun.

The Repose Owner Probe 0.1.0 and Repose Graph Probe 0.1.0 can both be removed from the user's `BepInEx/plugins` folder.
