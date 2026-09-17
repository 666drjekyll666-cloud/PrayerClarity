# PrayerClarity 1.0.17 candidate scope

Status: maintenance-only candidate scope; build and narrow runtime regression required.

## Evidence leading to 1.0.17

PrayerClarity 1.0.16 passed its visual Technology width/layout runtime gate. Repose, Prosperity, Combo and long Russian/English examples rendered compactly and remained inside the viewport. The gamepad screenshot showed centered subsection separators inside the stock composite controller tooltip (`Prayer details`, crafting rows, and the vanilla `Perk: Cardinal` separator) while body text remained left-aligned. This is treated as stock composite-tooltip hierarchy, not a PrayerClarity defect; 1.0.17 does not override it.

The same 1.0.16 runtime log exposed a separate maintenance defect: opening Inventory/Technology causes repeated stock diagnostics such as `No data for object [CraftDefinition] with id = "pray:<ordinary item id>"` and `Craft for sermon not found: <ordinary item id>`.

Static inspection identifies the PrayerClarity contribution: the global `ItemDefinition.GetTooltipData(Item,bool)` postfix called `ResolvePrayerCraft` for every item, and `ResolvePrayerCraft` read `linked_craft` before determining whether the item belonged to a prayer family. The game's `linked_craft` getter performs the `pray:<item id>` lookup, so ordinary items caused redundant failed prayer-craft queries.

## 1.0.17 change

Before touching `linked_craft`, `ItemTooltipPresentation` now reads the inexpensive item `id`, reduces a quality-suffixed id to its family (`b_faith:1` -> `b_faith`), and checks it against the directly verified PrayCraft-family catalogue.

The allowlist contains the verified player prayer families plus the runtime-observed unclassified PrayCraft rows:

`b_empty`, `b_faith`, `b_money`, `b_faith_money`, `b_plant`, `b_sins`, `b_skull`, `b_sword`, `b_shield`, `b_pen`, `b_star`, `b_village`, `b_souls`, `b_grat_points_incr`, `b_sin_shard`, `b_ghost`, `b_energy`, `b_random`, `b_techpoint_blue`, `b_techpoint_green`, `b_techpoint_red`, `b_circle`, `b_cross`.

Only matching families are allowed to read `linked_craft`; the existing `pray:` craft-id verification remains in place after that lookup.

## Boundary

- no prayer mechanics changes;
- no Technology, pulpit, Temporary Effects or prayer-item text/layout changes;
- no gamepad alignment override;
- no polling, scans or repeated subscriptions;
- ordinary items now take only the cheap item-id rejection path through the PrayerClarity item-tooltip postfix.

## Runtime gate

A narrow regression is sufficient:

1. open Inventory and Technology pages containing many ordinary items/recipes and confirm the prior `Craft for sermon not found` / `pray:<ordinary id>` burst is absent from the new session log;
2. hover at least one real prayer item and confirm its PrayerClarity item tooltip still appears correctly;
3. no need to repeat the accepted 1.0.16 Technology visual matrix unless an unrelated visual regression is observed.
