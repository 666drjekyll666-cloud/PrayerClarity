# PrayerClarity 1.0.19 candidate scope

## Purpose

Post-release Clarity polish only: add one visual line of separation between the final prayer tier block and the native crafting-location footer (`Crafted at:` / localized equivalent) in Technology prayer tooltips.

## Runtime change

- Reuse the existing verified `TooltipTextPolish.NormalizeFollowingCraftingRow` seam.
- Normalize the native crafting-location row to begin with exactly one leading line break.
- Strip an existing leading line break before rebuilding the row so repeated tooltip construction is idempotent and cannot accumulate blank lines.
- Keep the existing heading/value normalization (`Crafted at:` on one line, locations on the next).

## Explicitly unchanged

- prayer mechanics and balance;
- pulpit presentation;
- prayer item tooltips;
- Temporary Effects;
- Technology content-width / viewport policy;
- tier grouping, wording, values and reward layout;
- localization strings.

## Runtime gate

Open at least one multi-tier prayer in Technology and confirm there is one clear blank-line gap before the localized crafting-location footer, with no duplicated blank lines after switching away and back. A second prayer is a useful spot check. No sermon execution is required.

If accepted, promote as the stable 1.0.19 hotfix and publish the exact tested bytes; do not replace 1.0.18 assets in place.
