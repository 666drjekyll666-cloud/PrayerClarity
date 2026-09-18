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
