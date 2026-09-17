# Lookup Identity Probe 0.1.0 — handoff manifest

Status: research-only runtime evidence build. Not production code and not PrayerClarity: Rebalanced gameplay behavior.

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

Expected runtime output:

`BepInEx/PrayerClarity-lookup-identity-probe-0.1.0.txt`

The summary reports `prayerRows`, null lookups, reference mismatches, output-list identity, and `PASS=true/false`.

Requested user action:

1. Keep stable PrayerClarity: Vanilla 1.0.20 installed if desired; this probe has a separate plugin GUID.
2. Place the probe DLL in `BepInEx/plugins`.
3. Launch Graveyard Keeper and load any save.
4. No prayer, pulpit or sermon action is required.
5. Return `BepInEx/PrayerClarity-lookup-identity-probe-0.1.0.txt`. If it is not created, return the BepInEx log instead.

Acceptance criterion for this evidence gate: every discovered `pray:*` craft resolves non-null and `ReferenceEquals(craft_data_row, lookup)==true`. Output-list identity is recorded separately to determine whether success-output projection needs any additional cache/list handling.
