# PrayerClarity 0.1.16 candidate scope

Status: pre-runtime candidate scope, 2026-09-15.

0.1.16 is Clarity-only. It does not intentionally change prayer mechanics, success probability, sermon rewards, buff magnitude, save data or balance.

## Runtime scope

### Pulpit

- keep exact success chance but preserve the sermon as the reveal moment for final Faith/donation totals;
- label the success row as an explicit success bonus;
- Guaranteed names the base reward dependencies directly instead of using the rough `icon from icon · icon from icon` wording;
- prayer-owned percentage modifiers are rendered as percentages of the base reward, with fixed additions in the same resource expression;
- no green `up` marker is used merely to highlight a success contribution; semantic effect arrows remain available inside actual effect descriptions;
- the redundant dependency footer remains hidden.

### Technology

- keep stock prayer title, flavor/crafting description and normal technology presentation;
- replace the duplicated stock broad church-requirement/effect block at its native `preach_params_2` tooltip seam when the verified layout is present;
- show one base-reward dependency line plus Bronze/Silver/Gold rows with requirement, success bonus formula and exact intrinsic effect/duration;
- if the stock tooltip shape is unexpected, fail safely by appending the Clarity block rather than deleting unknown text;
- native quality-star icons remain deferred because no verified low-cost inline-quality-icon seam has been established. Text quality labels are the safe candidate presentation.

### Temporary Effects

- keep the accepted quantitative active prayer-buff descriptions from 0.1.15;
- for verified prayer buffs with at least one in-game day remaining, render the existing timer as approximately `N.N` in-game days;
- below one in-game day, retain the vanilla precise timer;
- derive remaining days from the verified canonical state `PlayerBuff.end_time - MainGame.game_time`; do not parse timer text or depend directly on Longer Days.

## Requested runtime acceptance check

1. Pulpit: inspect Donations/Combo (plus any specialist prayer if convenient). Confirm `Success bonus (N%)`, clear base dependencies, no green arrow on success resources, and no exact final payout.
2. Technology: inspect a prayer unlock such as Faith/Combo. Confirm the old broad requirement/effect mechanics are no longer duplicated, flavor/crafting information remains, and the new base/tier formulas are understandable.
3. Character -> Temporary Effects: activate a timed prayer with the Test Harness. With at least one in-game day remaining, confirm the timer reads approximately `N.N` days while the quantitative effect text remains correct.
4. Report clipping, duplicated stock mechanics, raw localization IDs, or incorrect prayer/effect mapping.
