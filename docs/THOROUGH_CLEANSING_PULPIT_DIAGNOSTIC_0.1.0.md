# Thorough Cleansing pulpit diagnostic 0.1.0

Status: research-only, read-only runtime diagnostic.

Question: why can PrayerClarity: Rebalanced 0.2.25 display Gold Thorough Cleansing correctly as x4 in the prayer-item tooltip while the pulpit Effect row displays x2 for the same selected prayer?

Target:
- PrayerClarity: Rebalanced 0.2.25 exact candidate source `3f5d4feef3b50de048ab3eec1d2e9a9388182340`;
- Graveyard Keeper 1.407.

Method:
- hard-depends on the Rebalanced plugin;
- patches only the existing final pulpit Effect writer `PulpitPolish.PolishEffectRow()` with a postfix;
- acts only when the selected craft ID starts with `pray:b_sin_shard:`;
- reads the selected craft/buff/quality/duration, the Rebalanced tier-semantic result, a fresh side-effect-free `PrayerForecast.BuildTierDetails` result, the forecast already held by `PulpitPolish`, and the final label text;
- emits one `PC_THOROUGH_PULPIT_DIAGNOSTIC` line per distinct observed state;
- does not mutate craft data, prayer state, UI text, save data, progression, buffs, or mechanics.

Interpretation:
- if provider/tier/result all report x4 while final text reports x2, the mismatch is downstream/final-writer state;
- if the provider reports false/x2, the issue is upstream semantic resolution;
- if the fresh tier reports x4 but the held pulpit Result reports x2, the selected craft/forecast lifecycle is stale between selection and final rendering.

Required user action:
1. install Rebalanced 0.2.25 plus this diagnostic;
2. open the church pulpit;
3. select the Gold Thorough Cleansing prayer once;
4. return `LogOutput.log`.

No sermon execution or save is required.
