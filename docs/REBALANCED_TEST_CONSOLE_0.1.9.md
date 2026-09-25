# Rebalanced Test Console 0.1.9 — filtered prayer gallery and temporary inventory expansion

Research-only harness target:
- PrayerClarity: Rebalanced 0.2.25 source `3f5d4feef3b50de048ab3eec1d2e9a9388182340`;
- Graveyard Keeper 1.407.

## Prayer gallery correction

0.1.8 intentionally used a broad structural filter (`ItemType.Preach` + linked `pray:*`) and runtime evidence showed that this also admits eight internal/unclassified families that are present in `GameBalance` but are not part of the verified player-facing prayer catalogue.

0.1.9 keeps data-driven item/tier discovery but gates the linked craft family against the project's verified multi-quality player catalogue:

- `b_faith`
- `b_money`
- `b_faith_money`
- `b_plant`
- `b_sins`
- `b_skull`
- `b_sword`
- `b_shield`
- `b_pen`
- `b_star`
- `b_village`
- `b_souls`
- `b_grat_points_incr`
- `b_sin_shard`

The broad internal/unclassified rows `b_ghost`, `b_energy`, `b_random`, `b_techpoint_blue`, `b_techpoint_green`, `b_techpoint_red`, `b_circle`, and `b_cross` are deliberately excluded.

## Temporary player inventory capacity

Added two explicit test-only actions:

- `Expand player inventory ×5`
- `Restore original inventory size`

The implementation uses the host-owned player item container and its native `SetInventorySize(int)` seam. The original capacity is captured once per console session and the expanded target is exactly five times that captured value.

The ordinary inventory GUI already renders its inventory widget inside `UIScrollView`, so increasing capacity does not require a replacement inventory UI.

Restore safety:
- restore is blocked while the console still tracks spawned prayer items;
- restore is also blocked if the inventory still contains more entries than the original capacity;
- the user should remove/move test items first.

## Save safety

Both prayer items spawned by the gallery and the expanded inventory capacity are **save-persistent** if the user saves while they are active.

For an exact cleanup:
1. `Remove prayer items spawned by this console`;
2. `Restore original inventory size`;
3. only then save a permanent playthrough state.

This remains a research/setup harness. Tooltip correctness is still accepted by direct visual observation.
