# PrayerClarity 0.1.15 candidate scope

Status: pre-runtime candidate scope, 2026-09-15.

0.1.15 is Clarity-only. It does not intentionally change prayer mechanics, success probability, rewards, buffs, save data or balance.

Runtime scope:

- pulpit uses the accepted reward-reveal model: base reward dependencies + prayer-owned success modifiers + exact intrinsic effects, without exact pre-sermon Faith/donation totals;
- the redundant lower pulpit dependency note remains hidden;
- Character -> Temporary Effects replaces vanilla prayer-buff flavor/opaque descriptions with verified exact effect meaning while preserving the vanilla timer;
- Technology prayer tooltips append prayer-quality rows with exact Church Quality requirements, prayer-owned Faith/donation modifiers/fixed outputs, and exact verified special effects/durations;
- all new PrayerClarity-owned player-facing copy is present in the same 11 interface locales supported by the project;
- no per-frame polling or broad UI scan is introduced.

Runtime acceptance is still required before any of these presentation changes move to the stable line.
