# Implementation Seam Probe 0.1.0 — handoff manifest

Status: **research-only read-only runtime evidence build**. Not production PrayerClarity behavior.

Purpose: close the two remaining narrow implementation-seam questions before Rebalanced production work:

1. **Combat:** identify the exact outgoing player-attack damage calculation/consumer so Silver/Gold Combat can add only their tier delta above stock `buff_sword` damage without globally patching generic parameter access.
2. **Repose:** identify the executable `Flow_DropBody` node/closure ownership and node-ID surface needed to target only ordinary donkey corpse-delivery nodes, not story/intro body generation.

The probe performs reflection and IL inspection only after a save has loaded. It does not require combat, a sermon, a donkey delivery, or any other gameplay action.

Safety contract:

- no Harmony patches;
- no graph execution;
- no `GameBalance` mutation;
- no player/world/save mutation;
- no per-frame work after the single audit run;
- output is a text report only.

## Pre-probe facts already established

Repose body RNG itself is already known: `GameSave.GenerateBody(tier_min, tier_max, ...)` filters `GameBalance.bodies_data` to the inclusive tier range and chooses one `BodyDefinition` through stock `ExtentionTools.RandomElement` before generating the corpse item.

The serialized `npc_donkey` graph already establishes that ordinary corpse delivery has two `Flow_DropBody` branches (node IDs `210` and `232`) fed by the same dynamic body range:

- `body_min + add_body_min -> Tier min`;
- `body_max + add_body_max -> Tier max`.

A separate story/intro `Flow_DropBody` node (`157`) uses fixed tier values. Therefore a global `GameSave.GenerateBody` patch is explicitly rejected as too broad. The preferred seam is the two ordinary-delivery nodes or an equivalently narrow execution context.

The accepted Repose formula remains:

- Bronze: stock range/stock RNG;
- Silver: 50% force `tier_min = tier_max`, otherwise stock range, giving exactly `0.5 + 0.5 * P_stock(best)`;
- Gold: always force `tier_min = tier_max`;
- selection inside the resulting tier remains stock random and the progression ceiling is never exceeded.

Combat regeneration is not part of this probe: the native `PlayerBuff` tick mechanism is already sufficient for the accepted 1/2/4 HP/sec design, and stock `buff_sword` supplies the Bronze +5 damage floor. The unresolved damage question is only where to add Silver/Gold's extra +5/+10 outgoing damage safely.

## Build identity

- Frozen ref: `candidate/implementation-seam-probe-0.1.0`.
- Exact source SHA: `5444e7679259f11a392a2971fd6ddf6072872fc8`.
- GitHub Actions run: `35223084322`.
- Job: `105207788716`.
- Workflow artifact: `PrayerClarity-ImplementationSeamProbe-0.1.0`.
- Artifact ID: `10498165221`.
- Artifact ZIP SHA-256: `64e36a19ce181ddf0d63fdf06cd348fd37fb8ff9e06510c5644a6fb2e0619617`.
- Handoff filename: `PrayerClarity.ImplementationSeamProbe.0.1.0.dll`.
- Handoff DLL SHA-256: `bf67ffd9105baa6850eb0e8e145467b01fbff324fd3ddf265b6587f16ab5e407`.

## Expected runtime evidence

Loading any save should create:

`BepInEx/PrayerClarity-implementation-seam-probe-0.1.0.txt`

The report contains:

- complete declared `BaseCharacterAttack` method IL and relevant attack/damage callers;
- matching `BaseCharacterComponent` attack/damage methods;
- `Flow_DropBody` and nested closure field/method IL;
- relevant Node/FlowNode base schemas;
- loaded `npc_donkey` node identity/owner metadata when exposed by the runtime object graph.

Once that report is accepted, this probe should be removed and not rerun unless the relevant game/runtime assumptions change.
