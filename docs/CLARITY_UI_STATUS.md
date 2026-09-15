# PrayerClarity — Clarity UI status

Status: research/product record after runtime test of PrayerClarity 0.1.14 on Graveyard Keeper 1.407, 2026-09-15.

This file tracks only the information-only **Clarity** layer. It does not accept or implement Vanilla Fixes or Balance/Rework mechanics.

## Current pulpit result

Runtime evidence for 0.1.14 confirms the current pulpit forecast remains usable at 2560x1440 and the calibrated dependency-note font is 10.

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

The pulpit is not the whole PrayerClarity product. Two additional surfaces are now required before Clarity can be considered complete.

### 1. Character -> Temporary effects / active prayer buffs

This is **required**, not an optional later enhancement.

Existing direct audit already established that vanilla active prayer-buff presentation binds the `PlayerBuff`, icon and remaining timer, but does not expose the quantitative prayer mechanic itself.

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

## Next work order

1. Treat 0.1.14 as the end of the inline-item-icon experiment; no 0.1.15 icon-chasing build.
2. Perform a static/read-only audit of the Character/Temporary Effects and technology-description UI seams, reusing existing 0.1.6 presentation evidence first.
3. Record exact targets and the minimal text model for each surface.
4. Implement both Clarity surfaces in one coherent dev candidate together with removal of the dead inline-item-icon path.
5. Runtime-test the relevant Character/Temporary Effects and technology screens.
6. Only after those surfaces are accepted, perform the remaining second-resolution/localization smoke tests and decide which pulpit tuning controls, if any, belong in the released product.

No hosted CI is required for this documentation/research step.