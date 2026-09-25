# Marker OFI event-time selection hypothesis

## Context

- Repo: `C:/Users/Administrator/test_gemini/PulseTrade.Comm.Spa.Dynamic`
- Branch: `20260915_033.ptcs_group_support`
- Baseline commit: `77fa1cd`
- Baseline status: clean
- Evidence: true SPAA entry marker has exact event time 22:01 inside a 60K candle; marker exists, but `entries.First.HoverAsync()` leaves row OFI count at zero for 180 seconds.

## Hypothesis

1. Marker SVG has no interaction seam that publishes its resolved placement slot/event time. Hover is therefore handled by the enclosing chart pointer path, which derives a row-axis slot from X and may snap to the 60K axis timestamp rather than the marker's 22:01 event time.
2. OFI lookup currently matches only `TaMarkerPlacement.SlotIndex`; a marker inside a coarse candle can be visually placed at the candle slot while shared cursor identity is reconstructed from a different axis time. The owner BrowserDemo only tests axis-aligned marker movement, so this divergence escaped.

## Experiment

- Add a coarse-row marker whose `EventTimeUtc` is not equal to its candle axis timestamp and reproduce hover count zero.
- Add an explicit marker pointer interaction carrying accepted placement slot plus marker event time into the existing cursor DOM update/callback path.
- Verify marker hover selects OFI directly, while ordinary chart hover retains current nearest-axis behavior and no chart remount/provider action occurs.

## Result

- Hypothesis 1 confirmed: explicit glyph hover selection fixes the consumer shape without changing ordinary plot hover.
- `TaMarkerCursorItem` now carries the original marker event time; both glyph and OFI item expose `data-marker-event-time` for deterministic consumer evidence.
- The `:30` marker inside a minute slot is intentionally not axis-aligned. Direct glyph hover produces OFI count 1 and the same original event-time attribute.
- Exact-package unit, package, browser interaction and performance gates passed. True SPAA re-acceptance remains the external release gate.
