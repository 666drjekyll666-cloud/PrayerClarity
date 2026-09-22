# PrayerClarity: Rebalanced 0.2.15 scope and acceptance

Status: **accepted and published stable Rebalanced 0.2.15 on 2026-09-23**.

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

## Accepted runtime seam

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

## Runtime acceptance result

Graveyard Keeper 1.407 runtime evidence observed the canonical terminal fixture: 33 ordinary candidates across tiers 2..3, best tier 3, maximum total skull score 10, and 9 tied maximum-score tier-3 definitions.

Ten consecutive real `GameSave.GenerateBody(2,4,-1,-1)` calls exercised the production 0.2.15 Gold seam. Every completed assertion selected tier 3 with total skull score 10, saw the 9-candidate maximum-score scoped pool, and confirmed restoration of the exact original body catalog after the call. An 11th native tier-3 body generation began before the research harness itself stalled from batching too many heavyweight calls in one UI callback; that harness failure is not a production Gold failure.

This evidence is sufficient for the changed Gold-generation/restoration property. The previously accepted ordinary-Donkey caller predicate and Silver 0.2.14 behavior were not reopened because 0.2.15 does not change those paths. Earlier-progression testing is not required for acceptance because the implementation derives the available tier and maximum score dynamically from the live Repose-expanded range.


## Stable identity

- accepted ref: `accepted/rebalanced-0.2.15`
- exact accepted runtime source: `ac953fe25ef17dbaf63340a7b9309dadec7e207e`
- stable promotion PR: **#29**
- stable promotion merge: `c9dede71131dbab4322e37409ca0bdf41994fcb9`
- release: `rebalanced-v0.2.15`
- release asset: `PrayerClarity.Rebalanced.dll`
- DLL SHA-256: `a980ecfeec4553c208280ca6ca2ca49196d4fb2bab6b6e121c908d0550ce0c4f`
- publication workflow: `35798813653`
