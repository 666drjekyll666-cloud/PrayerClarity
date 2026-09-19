# Secondary Clarity Surface Evidence

Status: direct Graveyard Keeper 1.407 runtime evidence, accepted for implementation targeting on 2026-09-15.

Evidence source for UI seams: read-only `PrayerClarity Secondary Surface Probe 0.1.7`, source commit `6ab56ca30161f03147a980ba3c79573e02b19431`, executed against `Assembly-CSharp` MVID `6f50b8e7-156b-49ac-bbe8-7505894b2364`.

The probe used reflection / IL / loaded UI hierarchy inspection only. It did not install Harmony patches or intentionally mutate save, player or world state.

## Character -> Temporary Effects

`InventoryGUI.RedrawBuffsAndPerks()` rebuilds the visible active-buff list from `MainGame.me.save.buffs`. For each non-hidden `PlayerBuff` it clones `perk_buff_item_prefab` and calls:

`PerkBuffItemGUI.Draw(PlayerBuff)`

That method is the exact presentation boundary for one active buff. It:

- stores the `PlayerBuff` in `linked_buff`;
- writes `buff.definition.GetLocalizedName()` to `txt_header`;
- writes `buff.definition.GetDescriptionIfExists()` to `txt_descr`;
- resolves `buff.definition.GetIconName()` into `icon_buff`;
- calls `RecalculateDescriptionHeight()`;
- repositions its child `SimpleUITable`;
- calls `Redraw()`.

`PerkBuffItemGUI.Update()` only calls `Redraw()`, and `Redraw()` updates the remaining-time text. Therefore PrayerClarity does not need an independent polling loop. A narrow postfix on `Draw(PlayerBuff)` can replace only the prayer-buff description while leaving the vanilla update lifecycle intact.

The verified prefab hierarchy contains the existing description label directly under `text container`, so no broad hierarchy scan is required.

Runtime testing of 0.1.15 confirmed that the quantitative prayer-buff descriptions are readable in the real Character -> Temporary Effects surface for representative prayer buffs. This presentation is accepted as the current Clarity direction.

Current ownership/synchronization contract:

- the game keeps the active buff header and icon; PrayerClarity does not replace them;
- the header comes from `BuffDefinition.GetLocalizedName()` and the icon from `BuffDefinition.GetIconName()`, so the visible icon is technically the buff/effect icon rather than an item-icon copy;
- PrayerClarity replaces only `txt_descr` for verified prayer buffs, using the current active-effect semantic projection rather than copying Technology tooltip prose;
- shared/Vanilla active effects read the live `BuffDefinition` values where applicable and format them through `active.*` localization keys;
- Rebalanced installs its own active-effect resolver, which reads the captured active prayer tier and the current `RebalancedRuleSet` for tier-dependent repaired/reworked effects. This keeps active-effect numbers aligned with the same effective rules used by the edition rather than maintaining a second numeric table;
- Technology-specific explanatory strings (`tech.*` / `rebalanced.tech.*`) are intentionally not mirrored verbatim into Temporary Effects. The status list uses a shorter context-specific sentence, although several concrete effect-value strings are deliberately shared between surfaces.

A 2026-09-18 source audit found one stale exception to that contract: long-duration promotion for `buff_plant` and `buff_sins` had been suppressed for the original Vanilla broken/unverified state and the suppression also carried into Rebalanced after those mechanics were repaired. Candidate Rebalanced 0.1.5 makes that suppression edition-aware: Vanilla behavior remains unchanged, while a concrete Rebalanced active-effect resolver makes the remaining duration meaningful and therefore visible in the description.

## Prayer-buff timer evidence

A second read-only probe, `PrayerClarity AuditProbe 0.1.8`, source commit `1389a1f56d928f0c1776a0939add1c73faaafd8f`, closed the timer-state question against the same verified game MVID.

`PlayerBuff` stores:

- `string buff_id`;
- `float end_time`;
- `float _tick_time`.

`PlayerBuff.GetTimerText()` computes:

`remaining = end_time - MainGame.game_time`

and only for formatting multiplies that normalized interval by the stock `450` seconds/day constant before converting it to hours/minutes/seconds.

`BuffsLogics.AddBuff(string, float?)` performs the inverse conversion when constructing or refreshing a buff: the selected duration is divided by `450`, multiplied by `60`, and added to `MainGame.game_time` as `PlayerBuff.end_time`.

Therefore:

`remaining in-game days = PlayerBuff.end_time - MainGame.game_time`

No parsing of the formatted timer string is required.

The same runtime had Longer Days active. `TimeOfDay.FromTimeKToSeconds(1f)` returned `675`, confirming the effective day length. Longer Days patches the 450-second conversions in `BuffsLogics.AddBuff` and `PlayerBuff.GetTimerText`; the normalized `end_time - game_time` state itself remains the correct count of in-game days. PrayerClarity therefore does not need a direct Longer Days dependency or config lookup.

Live sample evidence for `buff_plant` showed the same fixed `end_time` while the vanilla formatted timer changed from `1:11:59` to `1:11:57` over about two real seconds.

Accepted presentation hypothesis for the next candidate:

- for verified prayer buffs with at least one in-game day remaining, display approximately `N.N` in-game days;
- below one remaining in-game day, preserve the vanilla precise timer;
- use the existing timer surface rather than adding a second timer row.

## Technology prayer presentation

`TechTreeGUIUnlockItem.Draw(TechUnlockData, TechUnlock)` populates the visible unlock card. When tooltip data is enabled, it clears the component `Tooltip` and calls:

`TechUnlock.GetTooltip(Tooltip)`

`TechTreeGUIItem.InitGamepadTooltip(List<TechUnlock>)` uses the same `TechUnlock.GetTooltip(Tooltip)` path for controller presentation, adding separators between unlocks.

For craft unlocks, stock `TechUnlock.GetTooltip` already follows the multi-quality output to linked crafts and inspects prayer-relevant fields including:

- `needs_quality`;
- `k_money`;
- `k_faith`;
- fixed Faith/money outputs.

The native tooltip mutation API is explicit and sufficient:

- `Tooltip.AddData(BubbleWidgetData)`;
- `Tooltip.SetData(...)`;
- `Tooltip.ClearData()`;
- `BubbleWidgetTextData(string, TextStyle, Alignment, int)`;
- blank/separator widget data types.

The stock prayer mechanics block is also separable in the existing tooltip data: `preach_params_2` is emitted as its own header text item followed by the generated prayer-mechanics body. The broad church-quality requirement is the first line of the immediately preceding description block. This permits PrayerClarity to replace the duplicated mechanics block while preserving the prayer name, flavor text and crafting-location information. Unknown tooltip shapes must fall back to appending the Clarity block rather than deleting stock text.

## Production consequence

The narrow production seams are now:

- pulpit: existing `PrayCraftGUI.RedrawTextValues` postfix;
- active-effect description: `PerkBuffItemGUI.Draw(PlayerBuff)` postfix;
- active prayer-buff timer: existing `PlayerBuff.GetTimerText()` formatting seam, using verified `end_time - MainGame.game_time` state;
- technology: `TechUnlock.GetTooltip(Tooltip)` postfix.

All are relevant native UI/data lifecycle boundaries. No independent per-frame polling, global Unity scan, repeated reflection enumeration or duplicate subscription is required.

The Clarity candidate must leave verified prayer mechanics unchanged.

## 2026-09-19 follow-up — tooltip dependency context and top-HUD timer

Status: **static research complete; one narrow 1.407 HUD prefab probe pending. No production change is accepted by this section.**

### Item prayer requirement wording

Current production ownership is already narrow: `ItemTooltipPresentation` patches only `ItemDefinition.GetTooltipData(Item, bool)` for recognized prayer items, and `TooltipDetailsRenderer.BuildSingle` owns the concrete item's requirement row.

Current single-item grammar is structurally:

`Requires: <prayer-quality glyph> <Church Quality amount> (cross)`.

Therefore changing this to a localized contextual template such as “For <quality> quality, requires: <amount> (cross)” is presentation-only. No mechanics lookup, item mapping or new runtime hook is needed.

Technology is deliberately different: its accepted tier-comparison grammar already says “For 100% success, requires <amount> (cross)”. Earlier usability evidence showed that a bare “Requires: 10” was ambiguous. Do not weaken the Technology threshold wording merely for visual symmetry with the concrete item tooltip.

### Base Faith / donation dependency note

The semantic source already exists. `PrayerForecast.TierDetails.UsesSoulGratitude` and `PresentationText.DependencyMap` distinguish:
- ordinary prayers: Faith from Church Quality; donations from Graveyard Quality;
- BSS Soul's Repose: Faith from Church Quality **and current Soul Gratitude**; donations from Graveyard Quality.

Both Technology and item tooltip already consume the same `TierDetails` model. Therefore the safest implementation is one shared semantic dependency builder with surface-specific localized wording. The requested tooltip wording can be two intentional semantic lines:
- Base Faith — from Church Quality;
- Base donations — from Graveyard Quality.

For Soul's Repose, only the first line gains Soul Gratitude. This should be inserted immediately after the Prayer details heading on Technology and item surfaces. The semantic newline is intentional; normal label wrapping should still remain enabled within each line for long locales.

### Top HUD timer ownership

Project runtime evidence had already proved that `PlayerBuff.end_time - MainGame.game_time` is the correct remaining **in-game-day** interval and remains correct with Longer Days because the normalized day state is preserved.

Current PrayerClarity patches `PlayerBuff.GetTimerText()` and intentionally returns an empty string for prayer buffs while >=1 day remains. That decision came from the rejected 0.1.16 presentation: the stock compact timer font displayed localized day suffixes unreliably, leaving bare numbers. Character -> Temporary Effects therefore puts the strategic N.N-day value into its normal description label and restores vanilla precise timer text inside the final day.

Independent public decompilation of Graveyard Keeper corroborates the UI ownership:
- `BuffsGUI.Redraw()` creates a `BuffIcon` for each visible PlayerBuff;
- `BuffIcon.Draw(PlayerBuff)` binds the PlayerBuff and icon;
- `BuffsGUI.Update()` calls `BuffIcon.Redraw()`;
- `BuffIcon.Redraw()` reads `linked_buff.GetTimerText()` into `txt_timer`.

Consequently the missing top-HUD timer is explained by PrayerClarity's existing global `GetTimerText` suppression. A new timer state or independent polling loop is unnecessary.

The remaining exact 1.407 question is the live `BuffIcon.txt_timer` font/geometry and the safest localized-label reuse strategy. A read-only probe was prepared on `research/hud-prayer-timer-probe`:
- source SHA `672f0d5c1816447d9cbdc41635ecd6ed91e1db1b`;
- CI run `35465475560`;
- artifact `10591325714`;
- DLL SHA-256 `f05c820802519b9033ba77e343e2374cb4a8a8221626e96293fbeca2fe18a8a2`;
- build: 0 warnings / 0 errors.

The probe has no Harmony patch and performs no game-state/UI mutation. It records exact 1.407 `BuffIcon`, `BuffsGUI`, `PlayerBuff.GetTimerText` signatures plus live HUD timer-label/font/geometry and Character-label comparison into `BepInEx/PrayerClarity-hud-timer-0.1.0.txt`.

### Repose terminal wording

Russian player feedback from one participant indicates that “Ещё более качественные тела недоступны.” can imply an unnecessary prior comparison. The requested Russian wording is “Более качественные тела недоступны.” This is localization-only; Repose mechanics and endpoint detection remain unchanged. Treat this as a one-player UX signal, not community consensus.

### 2026-09-19 HUD probe runtime result

User-provided runtime output from Graveyard Keeper 1.407 matched supported MVID `6f50b8e7-156b-49ac-bbe8-7505894b2364` and confirmed the expected live owner:
- `BuffsGUI` owns `UI Root/HUD: Buffs/Buffs bar`, its grid and `buff_icon_prefab`;
- HUD `BuffIcon.txt_timer` is a 30x16, 16 px, `ShrinkContent` label using static bitmap `micro_font`;
- Character -> Temporary Effects `txt_timer` also uses the same 16 px `micro_font`;
- Character description uses a separate `tiny_font`, and its heading uses `small_font_bold`.

This closes the font/geometry uncertainty. The previous 0.1.16 failure to render localized day suffixes in the compact timer font is structurally consistent with the live prefab. The minimal design hypothesis is therefore:
- retain existing `PlayerBuff.GetTimerText()` suppression for >=1 day so Character does not duplicate the strategic duration already appended to its normal description;
- patch the already-native `BuffIcon.Redraw()` HUD lifecycle narrowly for recognized prayer timed buffs;
- for >=1 day, overwrite only the HUD `txt_timer` with a locale-formatted one-decimal **number only** (e.g. `3.2`);
- below one day, do nothing and preserve vanilla precise timer text;
- do not change the HUD font, label dimensions, grid, or introduce a new Update/polling owner.

The host already calls `BuffIcon.Redraw()` every frame from `BuffsGUI.Update()`; a narrow postfix would extend that existing native timer refresh rather than add another polling loop. It should update text only when the formatted value actually changes.

### 2026-09-19 wording / hierarchy follow-up

User preference:
- concrete prayer-item requirement should reuse the Technology grammar rather than invent a second phrase: quality glyph + localized “For 100% success requires N (cross)”;
- Technology and item tooltip should expose base payout dependencies near the top:
  - Base Faith — from Church Quality;
  - Base donations — from Graveyard Quality;
  - Soul's Repose adds Soul Gratitude to the Faith dependency;
- `Success bonuses` should be a real native heading at the same visual level as `Prayer details`, not merely an inline TinyDescription label;
- percentage success modifiers must explicitly state their base, so a stock-style `+150% +3` becomes semantically equivalent to `Faith: +150% of base Faith +3`;
- fixed-only and percentage-only cases should omit the absent component rather than expose formula jargon.

Static UI evidence supports the heading change without a custom font or new widget system: `Prayer details` is already emitted as native `BubbleWidgetTextData` style value 3 (`HintTitle`), while the mechanics body is style value 4 (`TinyDescription`). A `Success bonuses` heading can therefore be another native style-3 tooltip row inserted by the existing Technology/item tooltip hooks. A clean implementation should return structured tooltip sections rather than fake bold text inside the body string.

### Shared Clarity candidate acceptance gate — Vanilla 1.0.27 / Rebalanced 0.2.6

The approved presentation design is implemented as a shared candidate in both sibling editions. Runtime acceptance should verify:

- Technology and prayer-item tooltips use native `HintTitle` rows for **Base result** and **Bonuses on success**;
- Base result shows Faith -> Church Quality and Donations -> Graveyard Quality; Soul's Repose retains Soul Gratitude in the Faith dependency;
- the 100% success threshold is inside the success section and each concrete tier keeps its native quality glyph;
- percentage bonuses explicitly say they are percentages of the base value;
- mixed percentage + fixed bonuses are split into separate value lines (variant B), with the resource icon repeated on each component line;
- timed specialist effects keep localized day units in Technology/item tooltip text;
- the normal-world HUD shows a localized one-decimal **number only** while at least one in-game day remains, then returns to the vanilla precise timer below one day;
- Russian terminal Repose wording is `Более качественные тела недоступны.`.

The HUD implementation reuses the game's existing `BuffsGUI.Update() -> BuffIcon.Redraw()` cadence. A narrow `BuffIcon.Redraw` prefix handles only recognized long prayer timers and skips the original timer formatter for that one case; it adds no independent Update/polling owner and writes the label only when the formatted one-decimal value changes.



### 2026-09-20 runtime review — corrected 0.2.5 identity and 1.0.28 / 0.2.7 follow-up

The user's visual/runtime pass was executed with a misversioned corrected Rebalanced `0.2.5` binary from source `2e6276c36af31e405100eba6d431324c10b611df`. Repository comparison proved that the correctly numbered Rebalanced `0.2.6` source `3f5301e32c424f9f70b7ef48eb339d793c778f78` differs from that runtime only in version/workflow/documentation identity; the UI/runtime implementation is the same. The pass is therefore valid evidence for the corrected implementation, while both handed numbers remain immutable historical identities.

Accepted visual findings from that pass:
- native **Base result** and **Bonuses on success** headings are clear and retained;
- base Faith / donations dependency wording, including Soul's Repose Soul Gratitude context, is retained;
- tier blocks and percentage/fixed separation are retained;
- percentage rows should drop the redundant “of base value” phrase and show only the resource plus signed percentage;
- structured single-item rewards should use one compact atomic row, `<localized item name> ×N`, with no separate “Quantity” row;
- the concrete prayer-item 100%-success threshold must remain an atomic mechanics row and may widen only the PrayerClarity-owned text row when needed;
- Technology/item timed-effect text and Character -> Temporary Effects day wording are retained.

The same runtime log also explains the Russian HUD decimal-separator miss without requiring a new polling owner: BepInEx initializes PrayerClarity before Graveyard Keeper executes `LoadGameSettings` / loads the Russian language resource. The existing HUD path cached English formatting early, while later Technology/Character surfaces explicitly refreshed the game language. Rebalanced 0.2.7 / Vanilla 1.0.28 therefore add one late locale refresh on the first long-prayer HUD timer render, then keep the existing native `BuffIcon.Redraw()` cadence with no per-frame language polling.

Acceptance gate for the next shared candidate:
- no “of base value” suffix on Faith/donation percentage rows;
- Commercial Blessing, Good Story, Excellent Story and any equivalent single-item prayer reward render as `name ×N` on one semantic row;
- prayer-item `For 100% success requires N (cross)` remains visually unbroken;
- Russian long-prayer HUD timer uses the locale decimal separator if the one-time late refresh is sufficient; if not, do not add a heavier workaround solely for punctuation;
- all previously accepted 1.0.27 / 0.2.6 structure and mechanics remain unchanged.
