# Rebalanced Test Console 0.1.8 — prayer item gallery

Research-only harness target:
- PrayerClarity: Rebalanced 0.2.25 source `3f5d4feef3b50de048ab3eec1d2e9a9388182340`;
- Graveyard Keeper 1.407.

## Added gallery actions

The existing F1 Rebalanced Test Console now includes:
- `Give Bronze prayers`;
- `Give Silver prayers`;
- `Give Gold prayers`;
- `Give ALL prayer items`;
- `Remove prayer items spawned by this console`.

Discovery is data-driven:
- enumerate `GameBalance.me.items_data`;
- keep real `ItemDefinition.ItemType.Preach` definitions;
- require a real linked `pray:*` craft;
- keep only quality 1/2/3 definitions;
- therefore installed DLC prayer items are included automatically when present.

The harness uses the native player inventory `Item.AddItem(Item,bool)` seam. Existing copies are not duplicated. If inventory capacity prevents an add, the action reports partial success instead of mutating another storage surface.

## Save safety

Spawned prayer items are **save-persistent** if the player saves while they remain in inventory.

The console tracks only items that it actually added during the current session and exposes an explicit cleanup action. Do not move/use spawned prayer items before cleanup if you want exact restoration.

This gallery is setup automation only. Tooltip acceptance remains human visual evidence; the harness does not assert that presentation is correct.
