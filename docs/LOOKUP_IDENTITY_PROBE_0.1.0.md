# Lookup Identity Probe 0.1.0 — handoff manifest and accepted result

Status: **completed research-only runtime evidence build**. Not production code and not PrayerClarity: Rebalanced gameplay behavior.

Purpose: answer one remaining static-projection question on Graveyard Keeper 1.407: whether each live `pray:*` `CraftDefinition` stored in `GameBalance.me.craft_data` is the exact same object returned by `GameBalance.GetDataOrNull<CraftDefinition>(id)`, and whether the `output` list is the same referenced list.

Safety contract:

- read-only reflection;
- no Harmony patches;
- no `GameBalance` mutation;
- no player/world/save mutation;
- no sermon execution required;
- no polling after the one probe run.

Build identity:

- Frozen ref: `candidate/lookup-identity-probe-0.1.0`.
- Exact source SHA: `b752e624baf826c5bb2ed6b6454f4f380bb4cc32`.
- GitHub Actions run: `35218014297`.
- Job: `105191018307`.
- Workflow artifact ID: `10494968504` (`PrayerClarity-LookupIdentityProbe-0.1.0`).
- Downloaded artifact ZIP SHA-256: `4958fb2d541a54a401c8d5dbb73bd1221ce7b8be577e49b8d8000843580cd8ca`.
- Handoff filename: `PrayerClarity.LookupIdentityProbe.0.1.0.dll`.
- Handoff DLL SHA-256: `bc549b15f0d9ef41e543707e37882ef9534f8b218dae871dd60cb2a5293ca9b1`.

## Accepted runtime result — 2026-09-17

User runtime evidence was produced on the supported Graveyard Keeper 1.407 `Assembly-CSharp` MVID `6f50b8e7-156b-49ac-bbe8-7505894b2364`.

Observed summary:

- `prayerRows=72`;
- `lookupNull=0`;
- `sameCraft=72`, `differentCraft=0`;
- `sameOutput=72`, `differentOutput=0`;
- every reported prayer ID had `duplicateId=false`;
- probe result: `PASS=true`.

**Accepted fact:** for every discovered `pray:*` row, `GameBalance.GetDataOrNull<CraftDefinition>(id)` returned the exact same live `CraftDefinition` instance stored in `GameBalance.me.craft_data`, and its `output` field referenced the exact same list object.

Architecture consequence:

- per-save absolute/idempotent mutation of the live prayer `CraftDefinition` is visible through the same lookup path used by PrayerClarity presentation code and by stock consumers;
- no second prayer-definition cache needs to be rebuilt for those fields;
- output-list projection likewise needs no separate list/cache synchronization;
- the previously open live-definition identity gate for the one-shot `CraftComponent.FillCraftsList()` projection architecture is closed.

The probe has answered its only question and should not remain installed or be repeated unless a future game build changes the relevant lifecycle/identity assumptions.
