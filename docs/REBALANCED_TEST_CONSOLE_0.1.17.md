# Rebalanced Neutral Test Console 0.1.17

This is the long-lived neutral setup/access console for PrayerClarity: Rebalanced.

## Toggle key

- **F2** opens/closes the console.
- Escape closes it while visible.
- F1 is intentionally not used because the installed BepInEx Configuration Manager commonly owns F1.

## Permanent boundary

The console is deliberately **not** a probe host.

It contains no Harmony patches, no presentation/mechanics rewrites, no simulated values intended to prove a hypothesis, and no diagnostic interception of the production path.

Future behavior-mutating or path-intercepting probes must be separate temporary DLLs on separate research branches. Removing a probe DLL must leave this console available.

## Included neutral utilities

- prayer-item gallery for Bronze, Silver, Gold, or all verified player-facing prayer families;
- cleanup of prayer items spawned by the console;
- temporary player-inventory expansion and restoration;
- opening the native pulpit sermon UI without changing the calendar.

These utilities prepare or access test state; they do not patch the implementation being accepted.

## Save safety

Prayer items added by the gallery and inventory-capacity changes can enter save state. Clean up spawned items and restore inventory size before saving a permanent playthrough state.

Opening the pulpit is nonpersistent by itself, but pressing Pray executes a real sermon with normal game-state changes.

## Migration from 0.1.16

0.1.17 changes only the console toggle from F1 to F2. The neutral-tooling boundary and utility behavior are otherwise unchanged.
