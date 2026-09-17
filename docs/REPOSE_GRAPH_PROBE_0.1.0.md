# Repose Graph Probe 0.1.0 — handoff manifest

Status: **research-only load-time reflection audit**. Not production PrayerClarity behavior.

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

## Acceptance criterion

The Repose caller-isolation gate can close if the report exposes a stable runtime fingerprint covering both normal corpse-delivery `Flow_DropBody` branches while excluding the fixed story/intro node.

Prefer semantic/UID/owner predicates over the superseded static numeric IDs. A global `GameSave.GenerateBody` patch remains rejected.
