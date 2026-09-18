# PrayerClarity: Rebalanced 0.1.0 — implementation plan

Status: **development plan**, created after research/architecture gates were closed on 2026-09-17.

Baseline:

- PrayerClarity: Vanilla accepted runtime/source: `c7ac91c1cea6c498fb406323725768b605d8139f` (1.0.20)
- Rebalanced research handoff: `f66121541c423045e05960cf8a3b0d4b1b32832a`

The first development line must preserve all accepted Vanilla Clarity surfaces and add mechanics through the shared effective-rule architecture documented in `IMPLEMENTATION_TARGET_AUDIT.md`.

Implementation order:

1. sibling Rebalanced plugin identity / assembly and mutual-exclusion metadata;
2. common Clarity bootstrap reuse without changing accepted UI semantics;
3. `EffectivePrayerDefinition` / rule source for the locked roster;
4. first-per-save idempotent projection of stock-owned prayer fields/output lists;
5. successful-prayer tier capture and persisted namespaced player tokens;
6. Roots / Repentance / Repose / Combat / Excellence / Soul Contentment narrow mechanics seams;
7. Protection recipe retirement while preserving legacy IDs/items;
8. Rebalanced-aware Clarity semantics from the same rule source;
9. focused automated/static checks, then targeted runtime probes/candidate build;
10. integrated runtime acceptance before any public Rebalanced release.

Do not add per-prayer user sliders or a hidden Vanilla/Rebalanced runtime profile toggle. PrayerClarity: Vanilla and PrayerClarity: Rebalanced are sibling products; the user installs one edition.
