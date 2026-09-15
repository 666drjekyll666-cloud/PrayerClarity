# PrayerClarity — Clarity UI status

Status: research/product record after runtime testing of PrayerClarity 0.1.14 on Graveyard Keeper 1.407, 2026-09-15.

This file tracks only the information-only **Clarity** layer. It does not accept or implement Vanilla Fixes or Balance/Rework mechanics.

## Current pulpit result

Runtime evidence for 0.1.14 confirms the current pulpit forecast remains usable at 2560x1440 and the calibrated dependency-note font is 10.

A second-resolution smoke at **1920x1080** also passed. The user switched at least Russian, German and Japanese in the Test Harness and the forecast remained contained inside the resized pulpit; wrapping and general geometry remained usable. 2560x1440 language switching had already remained usable as well.

The current game/font presentation can look somewhat thin in some Latin/Cyrillic text at 1080p, while Japanese remains comparatively strong. This is not currently classified as a layout blocker. Do not introduce a custom font stack merely to improve weight before proving coverage/fallback behaviour for Cyrillic, CJK and the rest of the supported 11-language set.

Accepted/retained presentation direction:

- keep the `Guaranteed` + `Success bonus (N%)` result model;
- keep special effects in a separate Effect row;
- keep verified leading prayer-buff icons where the game exposes a reliable `BuffDefinition.GetIconName()` path;
- keep player-facing localized text as the safe fallback for resource/item nouns.

### Prosperity and Thorough Cleansing inline item icons

Two successive approaches did not produce a visible inline item/resource icon in the user's 1.407 runtime:

1. direct guessed/derived sprite IDs;
2. asking a constructed game `Item` for `GetIcon()` and passing the result through `EasySpritesCollection`.

The player-facing text remained correct and readable in both cases.

**Decision:** stop spending implementation complexity on inline icons for Prosperity and Thorough Cleansing. Text is accepted as the Clarity presentation for those nouns unless a genuinely native, low-cost seam is discovered incidentally later.

The next production cleanup should remove the now-unnecessary custom inline-item-icon resolution/widget path rather than keeping recurring fallback work for a visual that is not delivered. Working leading buff icons remain separate and should not be removed.

Do not create another numbered build solely for this cleanup; combine it with the next coherent Clarity candidate.

## Required remaining Clarity surfaces

The pulpit is not the whole PrayerClarity product. Two additional surfaces are required before Clarity can be considered complete.

### 1. Character -> Temporary effects / active prayer buffs

This is **required**, not an optional later enhancement.

Existing direct audit established that vanilla active prayer-buff presentation binds the `PlayerBuff`, icon and remaining timer, but does not expose the quantitative prayer mechanic itself.

Target player question while the buff is active:

> What exactly is this temporary effect doing right now?

The Clarity presentation should reuse the same semantic effect descriptions already established for the pulpit, adapted for an active-effect context. It should normally communicate:

- the concrete effect magnitude/relationship;
- remaining duration using the game's existing timer rather than duplicating timer logic;
- relevant conditions or scope when needed;
- accurate stock status for broken/disconnected effects rather than inventing a mechanic.

Do not copy pulpit-only reward rows into the buff surface. This surface should explain the active buff itself.

Before production code, re-audit the exact Character/Temporary Effects presentation seam (`BuffsGUI` / `BuffIcon` / related tooltip/details path) and choose the narrowest native place for added text.

### 2. Technology tree / prayer unlock descriptions

Prayer-related technology presentation must also become clear enough that a player can understand what a prayer is for before spending technology points/unlocking it.

Target player questions at unlock time:

> What does this prayer actually do?
>
> What changes when I later obtain a higher-quality version?

This is primarily a **static role / effect / quality-progression** surface, not a current-state forecast. Dynamic current church/graveyard/Soul Gratitude values belong at the pulpit.

Before production code, audit the exact `TechTreeGUI` / technology tooltip or unlock-dialog path used for prayer unlocks in 1.407. Do not guess localization keys, tech-to-prayer mappings or UI targets.

## Shared semantic-model rule

Pulpit, technology descriptions and active-buff descriptions must not become three independent copies of prayer mechanics.

Use one PrayerClarity-owned semantic model and render context-appropriate subsets:

- **Technology:** role + effect + how quality changes it;
- **Pulpit:** current guaranteed/success rewards + current special effect/duration/dependencies;
- **Active buff:** what the currently active effect does + existing remaining-time presentation.

This rule is especially important before Vanilla Fixes/Rebalanced profiles exist, because every presentation surface must remain consistent with the selected mechanics profile.

## Sequencing decision: finish Clarity surfaces before Balance/Rework implementation

Do **not** implement all Balance/Rework mechanics first and postpone UI integration until afterward.

Preferred order:

1. finish and accept the **UI seams and rendering architecture** for pulpit, Technology and Temporary Effects while stock/Clarity mechanics are the reference state;
2. keep the semantic model profile-aware so the renderer is not hard-wired to stock-only prose/constants;
3. then implement Vanilla Fixes / Balance/Rework mechanics against their separately verified runtime seams;
4. feed the changed values/relationships through the same semantic model rather than building new UI logic;
5. perform a final consistency/localization pass after the selected gameplay profile is implemented.

Reason: this separates two failure domains. UI-seam/layout problems can be diagnosed while mechanics are known and unchanged; later balance-runtime problems can be diagnosed without simultaneously wondering whether Technology/Temporary Effects rendering is wrong. It also prevents the future profiles from creating three separate copies of prayer formulas.

This does **not** mean every sentence written during stock Clarity is permanent. Profile-sensitive wording and quality-progression copy may be revised after Balance/Rework values are accepted. What should become stable now is the UI ownership, lifecycle, renderer contract and shared semantic-model shape.

## Secondary-surface audit status

Static/read-only evidence is now sufficient to narrow the implementation candidates substantially, but one exact Character-screen seam remains unresolved.

Established facts:

- the HUD `BuffIcon` surface is only icon + vanilla timer; it is not the Character/Temporary Effects detail surface;
- the Character screen owns a distinct `InventoryGUI.perk_buff_item_prefab` / `PerkBuffItemGUI` path and `BuffsGUI.Redraw()` explicitly asks `InventoryGUI.RedrawBuffsAndPerks()` to refresh it when Inventory is open;
- Technology nodes already build their native hover data through `TechTreeGUIItem.InitGamepadTooltip(...) -> TechUnlock.GetTooltip(Tooltip)`;
- `TechUnlock.GetTooltip` already contains prayer-specific presentation logic and the generic `Tooltip` API supports `AddData`, so the current lowest-risk Technology candidate is a narrow prayer-only enrichment at that semantic tooltip seam rather than a broad `TechTreeGUI` replacement.

Still unknown before production implementation:

- the exact `PerkBuffItemGUI` draw/bind method and live prefab hierarchy used by Character -> Temporary Effects;
- whether the same native `Tooltip` component is already attached to that row or whether the narrowest safe implementation should add one to the existing row.

A single read-only **secondary-surface probe 0.1.7** was therefore created on `research/clarity-secondary-surfaces`. It dumps only the exact relevant IL/type metadata plus the inactive/live `perk_buff_item_prefab` hierarchy and writes `PrayerClarity-secondary-surfaces-0.1.7.txt`. It does not patch Harmony targets or mutate save/player/world state.

Probe source/build identity:

- source branch: `research/clarity-secondary-surfaces`;
- build source SHA: `e9068634d85c1ab84eabca966f55bbc5101c6d3c`;
- GitHub Actions run: `34967750857`;
- build result: successful;
- handed DLL SHA-256: `10a5351900066d7709a592edebe933ab5543187a66e0817c12dbb4cfa01ada42`.

This is a research probe, not a production/test candidate and must not be promoted to `main` or recorded as accepted gameplay behaviour.

## Next work order

1. Treat 0.1.14 as the end of the inline-item-icon experiment; no icon-chasing build.
2. Treat the 1920x1080 + multi-language pulpit smoke as passed; no additional pulpit-resolution work is required before the secondary-surface audit.
3. Run the narrow 0.1.7 read-only probe once and inspect `PrayerClarity-secondary-surfaces-0.1.7.txt` to close the Character/Temporary Effects seam.
4. Record exact targets and the minimal text model for both secondary surfaces.
5. Implement both Clarity surfaces in one coherent dev candidate together with removal of the dead inline-item-icon path.
6. Runtime-test Character/Temporary Effects and technology screens.
7. After those UI seams are accepted, proceed to Vanilla Fixes / Balance/Rework implementation and drive all changed presentation through the shared semantic model.
8. Perform the final profile-consistency/localization smoke after gameplay changes are accepted.

No further hosted CI is required for research after the 0.1.7 probe build unless new evidence exposes another executable-only question.