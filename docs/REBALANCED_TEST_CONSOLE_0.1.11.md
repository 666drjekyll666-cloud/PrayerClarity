# Rebalanced Test Console 0.1.11

Research-only follow-up for the PrayerClarity 0.2.31 presentation regression.

## Question

The live Combo Prayer item tooltip shows two quality glyphs. Production 0.2.31 attempted to avoid a duplicate by inspecting the returned title string, but the runtime result proves that assumption did not identify the first glyph's owner.

## Probe

- The old prayer-title quality-glyph mutation probe now starts **OFF**.
- A read-only `ItemDefinition.GetTooltipData(Item,bool)` postfix is explicitly ordered before PrayerClarity production's item-tooltip postfix.
- On the first tooltip build for each `b_faith_money*` item definition it logs the item ID, verified item-definition quality, probe state, and raw host-returned title string before PrayerClarity production modifies it.
- Log record prefix: `PRAYER_ITEM_NATIVE_TITLE`.

## Required runtime action

With PrayerClarity: Rebalanced 0.2.31 installed, replace Test Console 0.1.10 with 0.1.11. Open the prayer-item gallery and hover one Combo Prayer that currently exhibits the duplicate star. Return the log. No sermon execution or mechanics retest is required.

## Acceptance use

This probe decides whether the first visible quality glyph belongs to the host-returned title string or a later/separate UI layer. Production title normalization must not be changed until that owner is established.
