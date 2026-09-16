# PrayerClarity — Rework research probe handoff

Status: read-only research artifacts awaiting one runtime evidence capture. These are not production mechanics candidates.

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

## One-pass user capture

Both DLLs may be installed together beside stable PrayerClarity 1.0.0. Load any save and remain in-game for roughly ten seconds after the game is ready. No sermon, confession, crafting, crop growth, manual saving, UI interaction, or controlled gameplay is required.

Return the two generated text files. Remove the research probe DLLs after capture.

The older SmartExpression Roots lifecycle probe is deliberately deferred: it is an implementation-seam question, not a blocker for the current rework roster/design pass.
