# Technology Prayer Carousel Prototype 0.1.0

Status: research-only, not a production candidate.

Target:
- Graveyard Keeper 1.407.
- PrayerClarity: Rebalanced candidate 0.2.21.
- Exact base source: `1d8eb667d4ddbc37efd24cbd84606f1a43e19606`.

## Question

Can the single Better Save Soul Technology node remain the only real technology while gamepad users inspect its three existing prayer unlocks one at a time, without changing save/progression state or stealing normal tree navigation?

## Static 1.407 evidence

Exact 1.407 decompilation was inspected from `Kupie/GYK_DECOMP`, commit `6abf79199d92482af1c7573870dd9a20ec2270b9`; `LazyConsts.VERSION` is 1.407.

Relevant native path:
- `TechTreeGUIItem.Draw` draws the child `TechTreeGUIUnlockItem` objects.
- Mouse mode initializes each child Tooltip from its own `TechUnlock.GetTooltip`.
- Gamepad mode passes `init_tooltip=false` to those children and `TechTreeGUIUnlockItem.Draw` disables the child collider.
- `TechTreeGUIItem.InitGamepadTooltip` instead appends every visible `TechUnlock.GetTooltip` into the parent Technology tooltip.
- The parent `TechTreeGUIItem`, not the child unlocks, receives the native `GamepadNavigationItem` focus callbacks.
- `BaseGUI.OnPressedLeft/Right` already owns normal tree navigation through `GamepadNavigationController.Navigate`.
- `BaseGUI` also exposes `OnPressedOption1/Option2`; in 1.407 they map to gamepad X/Y, and `TechTreeGUI` does not override them.

Conclusion: there is no native used child-focus path to reuse directly. The narrow alternative is a parent-owned, transient three-state carousel over the existing `TechUnlock` objects.

## Prototype behavior

Only when the focused Technology node's three visible unlocks resolve through PrayerClarity's current `ResolvePrayerCrafts` path to exactly:
- `b_souls`
- `b_grat_points_incr`
- `b_sin_shard`

the prototype:
- keeps native gamepad focus on the original Technology node;
- replaces the combined parent tooltip with the selected existing `TechUnlock.GetTooltip` result;
- uses X for previous prayer and Y for next prayer;
- leaves D-pad left/right/up/down navigation untouched;
- dims the two non-selected existing unlock icons while the node is focused;
- restores icon colours when focus leaves the node;
- writes no tech, save, cost, dependency, unlock, or reveal state.

The prototype deliberately calls the normal `TechUnlock.GetTooltip` path, so PrayerClarity's existing Technology presentation patch still owns the selected prayer's mechanics rows.

## Runtime question

This prototype needs one perceptual/controller test only:
1. focus the Better Save Soul prayer Technology node;
2. verify one prayer tooltip is visible;
3. press X/Y through all three prayers and confirm the highlighted icon and tooltip move together;
4. use D-pad left/right to leave the node and confirm ordinary Technology-tree navigation still works.

No BSS mechanics need to be retested.
