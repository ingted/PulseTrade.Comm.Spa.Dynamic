# Hypothesis: post-settlement navigator intent is lost

## Proven facts

- 0.2.57 emits `Purpose=NavigatorWindow` for the first cross-axis navigator move and the consumer settles it correctly.
- The next long body drag starts from local 1201/count 79, previews local 0/count 79, but action `ui:4` contains no `CoverageIntent`.
- `Renderer.setNavigatorWindowFromPrepared` is the canonical navigator event-range action seam.
- That seam currently calls `RendererModel.tryCrossAxisNavigatorEventRangeIntent`, which returns `None` when the latest accepted projection's global and active axes match.
- `setWindowFromPreparedRange` forwards that `None` unchanged into `VisibleRangeChanged`.

## Expected invariant

Gesture origin, not transient axis equality or equal counts, selects `TaCoverageWindowPurpose`. A navigator-origin event-range action must carry `NavigatorWindow`; a main-plot-origin action must carry `MainPlotPan`. Event-time bounds remain authoritative and no cross-axis ordinal is invented.

## Earliest divergence / hypothesis

The earliest divergence is the cross-axis predicate inside `tryCrossAxisNavigatorEventRangeIntent`. It treats axis equality after an accepted materialization as permission to erase the origin discriminator. The host then cannot preserve fixed-resident navigator semantics and interprets the event range through the wrong settlement path.

## Minimal differentiating experiment

Add a focused test for a same-axis accepted projection and a navigator target window. RED requires the canonical navigator helper to return a v2 intent with:

- `Purpose=NavigatorWindow`
- `RangeAuthority=ExplicitBounds`
- no `StartObservationOrdinal`
- no `Direction`
- exact requested `ObservationCount`
- next query generation and current coverage revision

The existing cross-axis helper/test remains as compatibility evidence; Renderer switches only its navigator-origin seam to the unconditional typed helper. Main-plot helpers and resident ordinal paths are unchanged.

## Estimated patch

Approximately 8-15 non-type-declaration source/test lines plus version and traceability updates; well below the 2x escalation threshold.
