# PrayerClarity — Clarity UI status

Status: **accepted stable Clarity release** for Graveyard Keeper 1.407.

Accepted build: **PrayerClarity 1.0.0**  
Accepted runtime/source SHA: `493d2168489af80b5c1305f7ff1435e2ee1dd0d7`  
Frozen candidate ref: `candidate/1.0.0`  
Frozen accepted ref: `accepted/clarity-1.0.0`  
Build run: `35035167380`  
Artifact ID: `10423032814` (`PrayerClarity-1.0.0-ci-493d2168489af80b5c1305f7ff1435e2ee1dd0d7`)  
DLL SHA-256: `dcf0338af520fe70989e6866bc6a724cbb3976337d7c9489c2ff9886e1e98580`

The user runtime-tested the 1.0.0 release candidate on 2026-09-16 and explicitly accepted it as the final Clarity version. The supplied runtime log confirms Graveyard Keeper 1.407 loaded `PrayerClarity 1.0.0`, reports `PrayerClarity` with **no config entries**, and records representative synthetic prayer selection/buff activation during the final smoke test. No PrayerClarity runtime error was reported during the tested pulpit, Temporary Effects, and Technology-tree interaction sequence.

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

### 1.0.0 release polish

The final accepted polish pass removes prototype/developer residue from the player-facing build:

- the accepted pulpit geometry is fixed in code; prototype layout sliders/configuration entries were removed;
- Shoots & Roots and Repentance use concise player-facing wording for their verified broken/unresolved stock effects rather than developer/research language;
- inactive/unverified effects do not advertise a misleading duration as though the missing mechanic were functioning;
- Repose uses a clearer Donkey/body-quality explanation;
- Imagination/Excellence use concise bonus wording;
- Thorough Cleansing and other timed prayer effects use the common in-game-day presentation path;
- stale/duplicate localization strings from earlier prototypes were removed while all 11 supported languages remain embedded.

## Accepted Technology presentation

Prayer-related technology tooltips are enriched at the native `TechUnlock.GetTooltip(Tooltip)` seam.

Accepted behavior:

- preserve vanilla title/lore/crafting information;
- remove duplicated broad prayer-mechanics text where PrayerClarity supplies the clearer structured block;
- show bronze/silver/gold quality rows with the game's native inline quality symbols `(s1)`, `(s2)`, `(s3)`;
- show quality requirement, success-only resource modifiers, and special effect/duration where applicable;
- use the same shared semantic model as the pulpit so the two surfaces do not disagree.

The native quality symbols were directly verified in the game's `small_font` and runtime-tested successfully.

## Accepted Character -> Temporary Effects presentation

Prayer buffs shown in Character -> Temporary Effects receive a concrete description of their current stock effect.

For long prayer buffs, remaining time is shown in **in-game days** in the normal description text. The small HUD/row timer is hidden while at least one full in-game day remains; inside the final day the stock precise timer returns.

The implementation derives remaining days from the native `PlayerBuff` state (`end_time - MainGame.game_time`) rather than parsing the formatted timer string. It does not add polling or a second timer state.

## Localization

PrayerClarity-owned player-facing strings ship in all 11 supported interface languages:

`en`, `fr`, `de`, `zh_cn`, `es`, `pt_br`, `ko`, `ja`, `ru`, `it`, `pl`.

The accepted runtime work included 1920x1080 and 2560x1440 presentation checks plus language switching across representative Latin/Cyrillic/CJK locales. The 1.0.0 final smoke test used the Russian interface at 2560x1440 and reported no remaining Clarity layout blocker.

## Architecture / performance boundary

The accepted Clarity layer remains event/UI-refresh driven:

- pulpit calculation/rendering occurs at the verified pulpit redraw seam;
- Technology text is produced only when the native tooltip is built;
- active-effect text is bound through the native active-buff row path;
- existing game timer/redraw lifecycle is reused;
- no broad Unity scans, background polling, persistent high-volume logging, or independent mirrored prayer state is introduced.

## Scope boundary

This accepted release describes **stock 1.407 behavior clearly**. It is not evidence that every stock prayer mechanic is healthy.

Future work remains separated into:

- **Vanilla Fixes** — only evidence-backed repairs where intended vanilla behavior/magnitude is recoverable;
- **Balance / Rework** — explicit intentional tuning or redesigned behavior.

Both later layers must feed their changed values through the accepted shared UI model rather than creating separate presentation logic.
