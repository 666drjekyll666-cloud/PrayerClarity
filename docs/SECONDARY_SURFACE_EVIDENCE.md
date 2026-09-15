# Secondary Clarity Surface Evidence

Status: direct Graveyard Keeper 1.407 runtime evidence, accepted for implementation targeting on 2026-09-15.

Evidence source: read-only `PrayerClarity Secondary Surface Probe 0.1.7`, source commit `6ab56ca30161f03147a980ba3c79573e02b19431`, executed against `Assembly-CSharp` MVID `6f50b8e7-156b-49ac-bbe8-7505894b2364`.

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

`PerkBuffItemGUI.Update()` only calls `Redraw()`, and `Redraw()` updates the remaining-time text. Therefore PrayerClarity does not need a per-frame description patch. A narrow postfix on `Draw(PlayerBuff)` can replace only the prayer-buff description while leaving the vanilla timer/update lifecycle intact.

The verified prefab hierarchy contains the existing description label directly under `text container`, so no broad hierarchy scan is required.

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

Therefore PrayerClarity can append a compact prayer-quality breakdown in a postfix on `TechUnlock.GetTooltip(Tooltip)`, using the same path for mouse and gamepad tooltips instead of maintaining a second technology UI implementation.

## Production consequence

The narrow production seams are now:

- pulpit: existing `PrayCraftGUI.RedrawTextValues` postfix;
- active effects: `PerkBuffItemGUI.Draw(PlayerBuff)` postfix;
- technology: `TechUnlock.GetTooltip(Tooltip)` postfix.

All are relevant UI lifecycle boundaries. No per-frame polling, global Unity scan, repeated reflection enumeration or duplicate subscription is required.

The 0.1.15 Clarity candidate uses these seams only for presentation. Verified prayer mechanics remain unchanged.
