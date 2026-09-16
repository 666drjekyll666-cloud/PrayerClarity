# PrayerClarity — Rework research probe handoff

Status: research-artifact ledger. These probes/auditions are temporary evidence tools, not production mechanics candidates.

## Repentance probe 0.1.0

- Purpose: recover the exact loaded `church_budka_roll` graph and code references relevant to confession cadence/outputs and a future Repentance rework seam.
- Exact source SHA: `0d414a5282336107b2cd9c3d5417afcb067528b4`.
- Frozen candidate ref: `candidate/repentance-probe-0.1.0`.
- GitHub Actions run: `35039599120`.
- Build: success on `ubuntu-latest`, `net472`, 0 warnings, 0 errors.
- Artifact ID: `10424467058` (`PrayerClarity-RepentanceProbe-0.1.0`).
- Artifact ZIP digest: `sha256:8dd41c0abfea532a0da55647dd4e5b4c72bc1230659bc91e51b9c7be1ee38e3b`.
- DLL: `PrayerClarity.RepentanceProbe.0.1.0.dll`.
- DLL SHA-256: `5598d604f70d9cb68e42af404391de0bb7633f691aa4cd2c99c2fedccac45d56`.
- Expected evidence: `BepInEx/PrayerClarity-repentance-audit-0.1.0.txt`.
- Contract: read-only graph/IL inspection; no Harmony and no intentional game/save mutation.

## Quality-scope probe 0.1.0

- Purpose: enumerate stock 1.407 GameBalance rows linked to `buff_pen` and `buff_star`, including enough row/quality data to model Imagination and Excellence against their real craft scope and saturation behavior.
- Exact source SHA: `e129aa38d68473181c42fcee329909f9b9ddd167`.
- Frozen candidate ref: `candidate/quality-scope-probe-0.1.0`.
- GitHub Actions run: `35039670229`.
- Build: success on `ubuntu-latest`, `net472`, 0 warnings, 0 errors.
- Artifact ID: `10425090732` (`PrayerClarity-QualityScopeProbe-0.1.0`).
- Artifact ZIP digest: `sha256:b967fe3735e5c6877942cc369c46b0a7208db5530401005cfcbfec1560383138`.
- DLL: `PrayerClarity.QualityScopeProbe.0.1.0.dll`.
- DLL SHA-256: `9f779aaed238fa5d5429c9dd333ba48fa4209baff950078fc88b4623c9972c36`.
- Expected evidence: `BepInEx/PrayerClarity-quality-scope-0.1.0.txt`.
- Contract: read-only GameBalance inspection; no Harmony and no intentional game/save mutation.

## Visual Audition Probe 0.1.1 — 2026-09-16

Purpose: compare three narrowed native-FX directions after Visual Audition 0.1.0 established that the first gold aura was too small, the prayer-track particles detached in world space, and the generic one-shot burst was visually unsuitable.

- Exact source SHA: `a8f60c32695fb42332aa24004625114a942ec576`.
- Frozen candidate ref: `candidate/visual-audition-probe-0.1.1`.
- GitHub Actions run: `35080270944`.
- Build: success on `ubuntu-latest`, `net472`, 0 warnings, 0 errors.
- Artifact ID: `10440370353` (`PrayerClarity-VisualAuditionProbe-0.1.1`).
- Artifact ZIP digest: `sha256:63878ce4333aa6d9a1cd193546b1cf9a81db02ed821fd469ff716c97d3e08747`.
- DLL: `PrayerClarity.VisualAuditionProbe.0.1.1.dll`.
- DLL SHA-256: `6d7e02a457e2415cab1324674525d70184fd15c0c159cbaaa17324947c7f3f91`.
- Contract: temporary runtime visual clones only; no Harmony, no save writes, no production mechanic changes.

Runtime controls:

- F2 — scaled soft-gold player `shard_charge_fx` aura;
- F3 — pulpit `pray_track_fx` forced into local simulation space and attached to Keeper;
- F4 — F2 + F3 together;
- F5 — scaled/recolored native tool-fire ParticleSystem attached to the animated front tool sprite as a holy weapon/tool flame audition;
- F6 — remove audition effects.

User should remove older VisualAudition/VisualScheduler research DLLs before this test and keep stable PrayerClarity 1.0.0 installed.

## Historical one-pass capture note

The Repentance and Quality-scope DLLs were previously installable together beside stable PrayerClarity 1.0.0 for one short capture. Their evidence has since been returned and incorporated into current research.

The older SmartExpression Roots lifecycle probe remains deliberately deferred: it is an implementation-seam question, not a blocker for the current rework roster/design pass.
