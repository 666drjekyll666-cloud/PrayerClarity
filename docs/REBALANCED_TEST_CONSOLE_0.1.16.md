# Rebalanced Neutral Test Console 0.1.16

This is the long-lived neutral setup/access console for PrayerClarity: Rebalanced.

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

## Migration from 0.1.15

0.1.15 remains immutable historical research evidence. Its quality-title, amount/icon-wrap, Roots, Combat, Repentance, Soul's Repose, Soul Contentment, Thorough Cleansing and other probe/diagnostic machinery is intentionally absent from 0.1.16.
