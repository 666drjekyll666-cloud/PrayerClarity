# Implementation Seam Probe 0.1.0 — handoff manifest

Status: **accepted research-only read-only runtime evidence**. Not production PrayerClarity behavior.

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

Combat regeneration is not part of this probe: the native `PlayerBuff` tick mechanism is already sufficient for the accepted 1/2/4 HP/sec design, and stock `buff_sword` supplies the Bronze +5 damage floor.

## Accepted runtime result — 2026-09-17

The returned report matches the target Graveyard Keeper 1.407 assembly MVID and satisfies the read-only contract.

### Combat — damage seam closed

`WorldGameObject.GetDamage(DamageType)` is the exact outgoing player-damage calculation:

- it builds the native `damage` / `damage_<type>` key;
- for `is_player`, it reads the equipped weapon through `Item.GetCalculatedParam(...)`;
- it then reads player parameter `add_damage` with default 0;
- it adds the two values and returns the result.

`CombatComponent.HitOther(...)` sends the attacker combat component to the target `WasHitBy(...)`, then marks the attack successful. Therefore `add_damage` is already the game's narrow player-only outgoing weapon-damage extension seam.

**Accepted implementation consequence:** do not patch attack animation, collider logic, generic `damage`, generic `GetParam`, or every hit. Rebalanced Combat should supply an effective active `add_damage` of +5/+10/+15 while the canonical Combat effect is active. The remaining work is lifecycle/tier state composition, not damage calculation discovery.

### Repose — execution seam substantially closed; one owner predicate remains

`Flow_DropBody.RegisterPorts()` creates dynamic inputs for `Tier min`, `Tier max`, soul tiers and durability. Its compiler-generated execution callback captures the actual `Flow_DropBody` node as `<>4__this`, evaluates all tier inputs immediately before calling `GameSave.GenerateBody(...)`, then drops the returned body through the stock path.

The inherited Node surface exposes both `ID` and `graph`, so the executing node can be identified without a global body-generation patch.

The load-only object scan could not materialize the desired runtime owner:

- `Flow_DropBody` is not a `UnityEngine.Object`, so a Resources scan cannot enumerate its instances;
- `CustomFlowScript.GetGraph("npc_donkey")` returned null in this save/load context.

Therefore the only remaining Repose seam question is very narrow: **during an actual ordinary donkey delivery, what graph/owner identity is visible from the captured node for IDs 210/232?** A logging-only prefix on the existing callback can answer this without changing tier inputs, RNG or the spawned corpse.

Do not broaden this into a global `GenerateBody` hook unless the narrow callback context proves unusable.

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

## Probe closure

Implementation Seam Probe 0.1.0 has answered its load-only questions and should not be rerun.

Combat requires no further seam-discovery probe.

Repose requires at most one final execution-time owner logger during one ordinary donkey delivery; that logger must remain research-only and must not mutate generated body inputs or outputs.
