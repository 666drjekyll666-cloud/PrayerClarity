# PrayerClarity 1.0.20 candidate scope

## Purpose

Localization-robustness hotfix for PrayerClarity Technology prayer tooltips.

Runtime testing of 1.0.19 found that Polish `Prayer for Repose` retained the vanilla church-quality requirement prefix before the prayer lore. The existing code removed that stock prefix by parsing localized prose around `(cross)` and sentence punctuation. Polish `preach_params` ends at `%1` without a sentence terminator, so that heuristic is not language-safe.

## Evidence

A read-only localization audit probe on Graveyard Keeper 1.407 checked all 11 supported game languages (`en`, `fr`, `de`, `zh-cn`, `es`, `pt-br`, `ko`, `ja`, `ru`, `it`, `pl`).

- Base prayer lore keys resolved for all 15 verified player-facing prayer families in every language: `15/15` per language.
- Tier-specific `_2_d` lore keys resolved for only `10/15`, so they are not a valid universal contract.
- Polish `preach_params` is `Aby nabożeństwo się powiodło, wymagane jest: %1`, confirming why punctuation-based stripping failed.
- The probe restored the original game language successfully after the audit.

Therefore the base lore key (`b_*_d`) is the verified language-independent seam for preserving native prayer flavor text.

## Runtime change

- Resolve the Technology prayer craft list once per tooltip build.
- Derive one common base lore key from the verified `pray:<family>:<tier>` craft IDs.
- Resolve that lore through the game's current localization resource.
- When replacing the verified vanilla prayer mechanics block, replace the immediately preceding requirement+lore row with the clean base lore only when that row contains `(cross)`.
- Remove punctuation/sentence-terminator parsing entirely.
- If the verified shape or lore key cannot be resolved, leave the vanilla row untouched.

## Explicitly unchanged

- prayer mechanics and balance;
- pulpit presentation;
- prayer item tooltips;
- Temporary Effects;
- Technology tier grouping, values, width/viewport policy, spacing and crafting footer;
- PrayerClarity-owned localization wording.

## Runtime gate

1. Polish: open Prayer for Repose in Technology and confirm the stray `(cross) 20–50` requirement prefix is gone while the Polish vanilla lore remains.
2. Russian or English: open the same prayer and one other multi-tier prayer as regression checks; lore, PrayerClarity mechanics block, spacing and crafting footer must remain unchanged.
3. No sermon execution is required.

## Runtime result — 2026-09-17

**Passed / accepted.** The user reported the exact 1.0.20 candidate as working correctly. Additional Japanese, Simplified Chinese, Korean and German screenshots showed the intended Technology structure without a systemic localization defect. The German post-icon word `erforderlich` is ordinary grammar (`required`), not a duplicated church-quality label; analogous post-icon wording exists in Japanese and Korean. The runtime log contained no PrayerClarity warnings or errors.

Accepted runtime source: `c7ac91c1cea6c498fb406323725768b605d8139f` (`accepted/clarity-1.0.20`). See `docs/RUNTIME_1.0.20_RESULT.md` for exact artifact identity and evidence.

Acceptance does not automatically publish a GitHub or Nexus release.
