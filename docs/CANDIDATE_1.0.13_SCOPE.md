# PrayerClarity 1.0.13 candidate scope

Status: **runtime candidate; not accepted until user in-game verification**.

## Why 1.0.13 exists

1.0.10 and 1.0.11 changed `BubbleWidgetTextData.max_width`, but runtime showed that the live Technology mechanics label still wrapped at the old narrow width. 1.0.12 then mutated `UILabel.width` and measured `printedSize`, but runtime again showed no effective widening.

Read-only runtime probe 0.1.2 finally identified the native limiter on Graveyard Keeper 1.407:

- PrayerClarity-owned mechanics data reaches the game with `max_width=900`;
- the corresponding `UILabel` uses `overflowMethod=ResizeFreely`;
- `UILabel.width=148`, `lineWidth=148`;
- critically, `UILabel.overflowWidth=150` and backing `mOverflowWidth=150`;
- `processedText` is already wrapped at that ~150-unit ceiling even though the raw `text` contains the intended unbroken tier rows.

Therefore `UILabel.overflowWidth`, not `BubbleWidgetTextData.max_width` or manual `UILabel.width`, is the native NGUI width ceiling that must be changed.

## Narrow implementation

1.0.13 keeps the same verified `BubbleWidgetText.Draw(BubbleWidgetTextData)` seam and the existing owned marker `max_width == 900`.

For that owned Technology mechanics row only, immediately after vanilla `Draw` has assigned font/style/text, PrayerClarity:

1. sets native `UILabel.overflowWidth = 900`;
2. reads `processedText` once to force native UILabel reprocessing before the enclosing bubble queries child size;
3. does **not** manually calculate or assign final label width or height.

The normal NGUI `ResizeFreely` behavior, `BubbleWidgetText.GetSize()`, `WidgetsBubbleGUI.UpdateSize()`, and the existing viewport clamp remain responsible for final geometry.

No hierarchy scan, per-frame polling, manual text measurement, prayer-mechanics change, or ordinary vanilla tooltip mutation is introduced.

## Intended visible behavior

The tier-first Technology tooltip text remains unchanged from the preceding UX candidates. Only geometry is intended to change:

- coherent tier rows such as `★ Для 100% успеха требуется 20 ✝` should remain on one line when they fit below the finite ceiling;
- the final tooltip should expand only to the natural content width, not become a fixed 900-unit panel;
- long descriptions may still wrap naturally;
- viewport safety remains active.

## Runtime gate

Use Prosperity as the primary case and one structurally different prayer such as Shoots & Roots or Repose.

Check only:

- the tier condition row no longer wraps because of the old ~150-unit ceiling;
- Prosperity reward rows fit naturally where possible;
- the bubble widens with content rather than jumping to a fixed 900-unit width;
- no clipping/off-screen regression;
- no new PrayerClarity content-width error in the log.

Do not repeat the full accepted 1.0.9 regression suite unless this narrow change exposes a broader issue.