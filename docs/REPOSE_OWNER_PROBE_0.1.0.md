# Repose Owner Probe 0.1.0 — handoff manifest

Status: **research-only execution logger**. Not production PrayerClarity behavior.

## Question

During an actual ordinary donkey corpse delivery, what executable owner/graph identity is visible from the captured `Flow_DropBody` node used by the normal-delivery branches already identified statically as node IDs 210 and 232?

This is the only remaining Repose caller-isolation question. Body RNG and the accepted Repose formula are already closed.

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

One ordinary donkey corpse delivery is sufficient. The probe should then be removed.

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

## Acceptance criterion

The gate closes if an ordinary donkey delivery logs a stable predicate that separates the normal-delivery node from other `Flow_DropBody` uses, preferably owner/graph identity plus node ID 210 or 232.

If that predicate is available, production Repose must target only that narrow execution context and leave story/intro body generation untouched. A global `GameSave.GenerateBody` patch remains rejected.
