# PrayerClarity: Rebalanced 0.2.15 candidate scope

Status: **source candidate; runtime acceptance pending**.

## Accepted design direction

Repose keeps the same three-step progression:

- Bronze: stock-style Repose expanded corpse pool.
- Silver: unchanged 0.2.14 behavior — half stock-style expanded pool, half best actually available ordinary tier.
- Gold: best actually available ordinary tier, then the maximum total skull count inside that tier.

The Gold rule is dynamic. It does not hard-code tier 3, body IDs, or 10 skulls. At terminal Graveyard Keeper 1.407 progression it resolves to the known 10-skull tier-3 bodies; earlier it resolves to the best body in whatever tier the stock Repose-expanded range actually reaches.

Repose requirements are **20 / 40 / 90**. Durations remain **30 / 42 / 54 minutes**.

## Proven host mechanics used by the candidate

`GameSave.GenerateBody` builds its own local candidate list from `GameBalance.bodies_data` by the supplied inclusive tier range, then calls the game's own random selector and `BodyDefinition.GenerateBodyItem()`.

Ordinary tier 3 contains bodies with 7, 8, 9 and 10 total skulls, so the accepted 0.2.14 rule "best tier" does not by itself satisfy the player-facing promise "best body guaranteed".

The candidate derives corpse score from each `BodyDefinition.parts_ids`. Each part resolves to its live `ItemDefinition`; total visible skull score is derived from `q_minus + q_plus`. Conditional item replacement is resolved with the same player-flag condition used by the game's item replacement path.

## Candidate runtime seam

The verified ordinary-Donkey callback predicate is unchanged.

For Gold only:

1. Consume the already Repose-expanded tier range supplied by the game.
2. Resolve the highest actually existing ordinary tier in that range.
3. Keep the accepted best-tier narrowing.
4. Derive the maximum skull score among ordinary body definitions at that tier.
5. During this one synchronous `GenerateBody` invocation, temporarily expose a same-type `bodies_data` catalog whose active candidate range contains only tied maximum-score bodies from the chosen tier.
6. Let stock `GenerateBody` keep ownership of candidate enumeration, RNG, body creation, soul handling and downstream behavior.
7. Restore the exact original catalog reference in a Harmony finalizer.

There is no per-frame work and no permanent balance-data mutation.

## Failure behavior

If maximum-score derivation or projection fails, the temporary catalog is restored immediately and the already accepted 0.2.14 best-tier behavior remains active for that delivery. Runtime errors are logged once.

If another owner unexpectedly replaces `bodies_data` during the scoped call, PrayerClarity does not overwrite that newer reference during cleanup.

## Acceptance target

Late-game Gold must produce only 10-total-skull ordinary Donkey bodies for the full effect while preserving variety among tied best definitions.

Silver must retain its accepted mixed behavior.

Earlier-progression testing is optional if a suitable save is available because the implementation derives the available tier from the live game range instead of assuming terminal progression.
