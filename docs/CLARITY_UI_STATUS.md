# PrayerClarity — Clarity UI status

Status: **accepted stable Clarity release** for Graveyard Keeper 1.407.

Accepted build: **PrayerClarity 1.0.1**  
Accepted runtime/source SHA: `7cf6d9287d2aa7cfa8c0529be98f62a3d87360ce`  
Frozen candidate ref: `candidate/1.0.1`  
Frozen accepted ref: `accepted/clarity-1.0.1`  
Build run: `35072962204`  
Artifact ID: `10437097204` (`PrayerClarity-1.0.1-ci-7cf6d9287d2aa7cfa8c0529be98f62a3d87360ce`)  
Artifact ZIP digest: `sha256:dadf99d291e272a24b6dc14ec9489835da6de88165d3290eb38125d42c5a22a0`  
DLL SHA-256: `476380ef08a1cced70cb47968f1263a04c03e0e1f2c81d7fd6a3ef057c24c5f2`

The user runtime-tested the 1.0.1 release candidate on 2026-09-16 and explicitly accepted it for stable promotion and public distribution. The supplied runtime log confirms Graveyard Keeper 1.407 loaded `PrayerClarity 1.0.1` with **no config entries**. It also confirms the test-only item-cell bridge drew synthetic prayer items into the native `PrayCraftGUI` item cell without inventory/save mutation, allowing the production prayer-item tooltip path to be exercised. No PrayerClarity runtime error was reported in the tested sequence.

Runtime screenshots confirmed both a multi-resource Combo prayer and Shoots & Roots render correctly as native item tooltips, including quality stars, requirements, success modifiers, and special-effect text. This closes the remaining Clarity presentation surface that was absent from 1.0.0.

This accepted release is information-only: it contains **no Vanilla Fixes and no Balance/Rework mechanics**.

## Accepted pulpit presentation

The pulpit keeps the stock sermon reveal moment for the exact final payout. PrayerClarity explains the dependency chain and prayer-owned contribution instead of showing the fully resolved current Faith/donation totals in advance.

Accepted lower-block hierarchy:

1. `Guaranteed` / localized equivalent;
2. resource-source lines, icon-first (`Faith — from Church Quality`, `Donations — from Graveyard Quality`);
3. `On success (N%)` / localized equivalent;
4. prayer-owned success modifiers, grouped by resource;
5. separate `Effect` row for intrinsic/special prayer behavior.

The redundant general `Result` heading is intentionally absent. The upper context block remains the stock-derived current context: Church Quality, sermon requirement, and Graveyard Quality.

Temporary prayer durations shown before use are expressed in **in-game days**, using the effective game day length rather than a hard-coded vanilla constant. This follows compatible day-length changes such as Longer Days without a direct mod dependency.

### Release polish retained in 1.0.1

The accepted presentation contains no prototype/developer-facing residue:

- the accepted pulpit geometry is fixed in code; prototype layout sliders/configuration entries are absent;
- Shoots & Roots and Repentance use concise player-facing wording for their verified broken/unresolved stock effects rather than developer/research language;
- inactive/unverified effects do not advertise a misleading duration as though the missing mechanic were functioning;
- Repose uses a clearer Donkey/body-quality explanation;
- Imagination/Excellence use concise bonus wording;
- Thorough Cleansing and other timed prayer effects use the common in-game-day presentation path;
- stale/duplicate localization strings from earlier prototypes are absent while all 11 supported languages remain embedded.

## Accepted Technology presentation

Prayer-related technology tooltips are enriched at the native `TechUnlock.GetTooltip(Tooltip)` seam.

Accepted behavior:

- preserve vanilla title/lore/crafting information;
- remove duplicated broad prayer-mechanics text where PrayerClarity supplies the clearer structured block;
- show bronze/silver/gold quality rows with the game's native inline quality symbols `(s1)`, `(s2)`, `(s3)`;
- show quality requirement, success-only resource modifiers, and special effect/duration where applicable;
- use the same shared semantic model as the pulpit so the two surfaces do not disagree.

The native quality symbols were directly verified in the game's `small_font` and runtime-tested successfully.

## Accepted prayer-item tooltip presentation

Version 1.0.1 extends the same shared prayer-detail model to native item tooltips through `ItemDefinition.GetTooltipData(Item, bool)`.

Accepted behavior:

- preserve the native prayer item title, flavor text, and crafting-location text;
- replace the broad vanilla prayer-parameter block with the same structured quality rows used by Technology;
- show each available quality tier with its native quality star, sermon requirement, success-only Faith/donation contribution, and special effect where applicable;
- ordinary prayer remains a single non-quality case rather than inventing bronze/silver/gold variants;
- resolve only the linked three-item quality family when it exists; no inventory scanning or mirrored prayer catalogue is required at tooltip time.

The final runtime acceptance exercised the real native item-cell hover lifecycle. Combo Gold displayed all three quality rows and grouped Faith/donation modifiers correctly; Shoots & Roots Silver displayed all quality rows plus the current known-problem effect text correctly.

## Accepted Character -> Temporary Effects presentation

Prayer buffs shown in Character -> Temporary Effects receive a concrete description of their current stock effect.

For long prayer buffs, remaining time is shown in **in-game days** in the normal description text. The small HUD/row timer is hidden while at least one full in-game day remains; inside the final day the stock precise timer returns.

The implementation derives remaining days from the native `PlayerBuff` state (`end_time - MainGame.game_time`) rather than parsing the formatted timer string. It does not add polling or a second timer state.

## Localization

PrayerClarity-owned player-facing strings ship in all 11 supported interface languages:

`en`, `fr`, `de`, `zh_cn`, `es`, `pt_br`, `ko`, `ja`, `ru`, `it`, `pl`.

The accepted runtime work included 1920x1080 and 2560x1440 presentation checks plus language switching across representative Latin/Cyrillic/CJK locales. The final 1.0.1 item-tooltip acceptance used the Russian interface at 2560x1440 and reported no remaining Clarity layout blocker.

## Architecture / performance boundary

The accepted Clarity layer remains event/UI-refresh driven:

- pulpit calculation/rendering occurs at the verified pulpit redraw seam;
- Technology text is produced only when the native tooltip is built;
- prayer-item details are produced only when the native item tooltip is built;
- active-effect text is bound through the native active-buff row path;
- existing game timer/redraw lifecycle is reused;
- no broad Unity scans, background polling, persistent high-volume logging, or independent mirrored prayer state is introduced.

## Stable publication

The exact accepted 1.0.1 DLL was promoted without rebuilding by GitHub Actions run `35076380012`.

- GitHub Release: `v1.0.1`
- Release target: `7cf6d9287d2aa7cfa8c0529be98f62a3d87360ce`
- Stable asset: `PrayerClarity.dll`
- Stable asset SHA-256: `476380ef08a1cced70cb47968f1263a04c03e0e1f2c81d7fd6a3ef057c24c5f2`

The stable asset bytes match the runtime-tested candidate exactly; only the installed filename differs from the CI handoff filename.

## Scope boundary

This accepted release describes **stock 1.407 behavior clearly**. It is not evidence that every stock prayer mechanic is healthy.

Future work remains separated into:

- **Vanilla Fixes** — only evidence-backed repairs where intended vanilla behavior/magnitude is recoverable;
- **Balance / Rework** — explicit intentional tuning or redesigned behavior.

Both later layers must feed their changed values through the accepted shared UI model rather than creating separate presentation logic.
