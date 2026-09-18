# Repose Owner Probe 0.1.0 — handoff manifest and result

Status: **accepted research-only execution evidence**. Not production PrayerClarity behavior.

## Question

During an actual ordinary donkey corpse delivery, what executable owner/graph identity is visible from the captured `Flow_DropBody` node used by the normal-delivery branches previously identified statically?

Body RNG and the accepted Repose formula were already closed. This probe tested only the caller/owner context.

## Scope and safety

The probe installs one Harmony prefix on the compiler-generated execution callback created by `Flow_DropBody.RegisterPorts()`.

The prefix:

- logs the captured node identity (`ID`, UID/name/tag when available);
- logs its `graph`, `graphAgent`, `cfs` and `wgo` owner context when available;
- does not change callback arguments;
- does not read/replace generated corpse results;
- does not modify tier inputs;
- does not call `GenerateBody` itself;
- does not write game/save state;
- performs work only when a `Flow_DropBody` callback actually executes.

The output file is:

`BepInEx/PrayerClarity-repose-owner-probe-0.1.0.txt`

## Build identity

- Frozen ref: `candidate/repose-owner-probe-0.1.0`
- Exact source SHA: `f06938f0fda1dea2a44505eb78a786927d782ebe`
- GitHub Actions run: `35224279561`
- Job: `105211774668`
- Artifact: `PrayerClarity-ReposeOwnerProbe-0.1.0`
- Artifact ID: `10498567101`
- Artifact ZIP SHA-256: `3f71f609b475c262626e04ffc52f33b88b3e1798cab66a7b1706b82d9e507049`
- Handoff filename: `PrayerClarity.ReposeOwnerProbe.0.1.0.dll`
- Handoff DLL SHA-256: `818d396ebb76ce523d368232adb55a6fd8083227d38c215fcceed1f4d4c6575c`

## Accepted runtime result — 2026-09-17

One ordinary donkey corpse delivery produced one `Flow_DropBody` execution with:

- runtime `node.ID = 108`;
- runtime `node.UID = 9231f192-6e56-4225-b494-b31718fe85c1`;
- `graph = FlowScript` named `npc_donkey`;
- `graphAgent = FlowScriptController` named `[wgo] donkey` at `World/[wgo] donkey`;
- `wgo = WorldGameObject` named `[wgo] donkey` with both `obj_id` and `_obj_id` equal to `donkey`;
- `cfs = null` in this execution context.

This proves that the narrow execution callback exposes a strong ordinary-donkey owner context at runtime. A global `GameSave.GenerateBody` hook remains unnecessary and rejected.

### Important correction to earlier static node numbering

The executed runtime node ID was `108`, not the previously expected static IDs `210/232`.

Therefore the earlier 210/232 values must **not** be used as production runtime node IDs without a direct mapping. They are superseded for runtime targeting by the accepted execution evidence above.

The runtime UID is a stronger identity surface than the earlier inferred numeric IDs, but one delivery observed only one of the normal-delivery branches previously found statically. We must not assume the second branch shares ID 108 or silently leave it unhandled.

## Remaining narrow question

Enumerate the already-loaded `npc_donkey` graph after save load and map every live `Flow_DropBody` node, including UID and upstream connection fingerprint. This can identify both ordinary dynamic-range branches and the fixed story/intro branch without waiting for another donkey delivery.

A separate **Repose Graph Probe 0.1.0** performs exactly that load-only reflection audit. No further execution logger or repeated donkey-delivery test is justified unless that graph enumeration fails.

The Repose caller-isolation gate remains **substantially closed but not yet final** until this sibling mapping is returned.
