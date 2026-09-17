# PrayerClarity 1.0.15 candidate scope

Status: runtime candidate following the successful 1.0.14 Technology tooltip polish test.

Runtime evidence from 1.0.14 confirmed:

- content-driven Technology tooltip width remains functional;
- title widow/orphan protection produces natural wraps such as `Молитва / об упокоении` and `Prayer / for repentance`;
- `Длительность эффекта` / localized `Effect duration` wording is clearer;
- very long shared effect text (Repose) wraps cleanly and no longer dictates excessive tooltip width;
- Russian and English Technology surfaces remain readable.

Remaining UX finding: medium-long effect descriptions can still become the sole width-driver. Representative English runtime examples are the Repentance effect (47 characters) and Shoots & Roots effect (55 characters), while shorter effects around 41–42 characters remain acceptably compact.

1.0.15 changes only the Technology effect-wrap heuristic:

- lower the long-effect threshold from 72 to 45 characters;
- lower the minimum segment length from 24 to 18 characters so medium-long effects can wrap;
- choose the break slightly after the midpoint and penalize boundaries next to very short connector/particle words, avoiding examples such as `special / effect` and `does / not` without rewriting localized text;
- continue applying the rule only to effect prose, never tier thresholds, reward rows, crafting location, or flavor text;
- preserve the 1.0.13/1.0.14 content-driven NGUI width mechanism unchanged;
- no mechanics changes.

Expected English examples:

- `The prayer's special effect / appears not to work`;
- `Known issue: this prayer does not / speed up plant growth`;
- Repose remains a two-line effect with a natural near-midpoint break.

Runtime gate: visually inspect Repose, Repentance and Shoots & Roots in at least one Latin locale; verify Repose remains stable, the two medium-long effects wrap more compactly, and short effects are not unnecessarily split.
