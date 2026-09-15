# PrayerClarity 0.1.18 candidate scope

Status: pre-runtime candidate scope, 2026-09-15.

0.1.18 remains Clarity-only. It does not intentionally change prayer mechanics, success probability, sermon rewards, buff magnitude, save data or balance.

## Runtime evidence carried forward from 0.1.17

User runtime testing accepted the main 0.1.17 presentation direction:

- pulpit hierarchy `Always -> Success adds -> Effect` is materially clearer than the rejected 0.1.16 formula-style presentation;
- Combo no longer shrinks relative to other prayers;
- Temporary Effects day counts render correctly in the normal description label, e.g. `3.2 days` / localized equivalent;
- for prayer buffs with at least one in-game day remaining, the compact HUD timer may remain hidden; the user explicitly accepted this behavior and does **not** want the ordinary HUD timer restored;
- below one remaining in-game day, the existing code still falls back to vanilla precise timer text;
- Technology prayer mechanics are no longer duplicated by a second vanilla mechanics block.

## 0.1.18 changes

### Pulpit calibration

Adopt the user's accepted live-tuned values as new defaults:

- Result header Y: `7`;
- Result header font size: `15`;
- Result rows Y: `-8`.

All other currently accepted 0.1.17 tuning defaults remain unchanged. This calibration keeps the expanded result hierarchy clear of the Effect row / test button in the tested composition.

Existing user config values are still respected; changing defaults does not overwrite a previously saved BepInEx configuration automatically.

### Technology prayer description cleanup

Direct IL evidence for `TechUnlock.GetTooltip` shows that stock 1.407 builds the broad `(cross) X-Y required` prayer requirement and then concatenates the localized prayer item description. Depending on localization/string shape, that requirement may be newline-delimited or may touch the lore sentence with no whitespace.

PrayerClarity already replaces the broad stock requirement with per-quality requirement rows. 0.1.18 therefore makes the cleanup robust for both stock shapes:

- if the leading requirement is its own line, remove that line;
- if it is concatenated directly to lore, detect the leading `(cross)` semantic marker and remove only through the first sentence terminator;
- support common Latin/Cyrillic and CJK sentence terminators;
- if the expected verified shape is absent, leave the stock text untouched rather than deleting unknown content.

This fixes the visible `...проповеди.Жалостливые...` join and removes the now-duplicated broad requirement for the same prayer tooltip family.

### HUD / Temporary Effects

No change from 0.1.17. Do not restore the compact HUD timer for long prayer buffs. Strategic duration remains in the Character -> Temporary Effects prose while at least one in-game day remains; the vanilla precise timer remains the final-day fallback.

## Deferred / non-blocking

- Native Bronze/Silver/Gold quality icons remain deferred until a verified low-cost inline icon seam exists.
- Gamepad technology tooltips aggregate multiple unlocks by vanilla design. No new relocation/scroll behavior is added here; the existing `Tooltip` owns its normal scroll/panel behavior. A compatibility/layout issue should be addressed only if reproduced without the user's separate tooltip-position mod.

## Requested runtime check

1. With pulpit tuning reset to defaults (or on a clean config), inspect a long-effect prayer such as Shoots & Roots and confirm Effect no longer collides with the button.
2. Inspect Faith/Donations/Combo Technology prayer tooltips and confirm the broad `X-Y required` sentence is gone while the lore sentence begins cleanly with normal spacing.
3. Confirm the accepted Temporary Effects/HUD behavior remains unchanged.
4. Report any clipping, lost lore/crafting text, raw localization key, or PrayerClarity exception.
