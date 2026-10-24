# Main plot exact-pan and bilateral Y-axis hypothesis

## Symptom

- Small horizontal drags on the TA main plot can cross a local resident boundary and land one full visible page away, presenting the 1500-bar window at the left of a 4500-bar resident domain.
- TA rows draw horizontal grid lines without matching left/right Y coordinate labels.

## Expected invariants

1. A drag that crosses `N` K bars moves the global visible start by exactly `N`, after domain clamping, while preserving visible count.
2. Main-plot drag never invokes navigator resize/move semantics.
3. Each row's two Y axes show identical tick values derived from the exact low/high geometry used by its traces and horizontal grid.

## Earliest divergence and hypothesis

- `mainPlotPanDelta` produces the correct delta.
- At a boundary, `panWindow` forwards that delta, but `dispatchAdjacentCoverageNow` calls page-oriented `tryAdjacentCoverageIntent`; that helper computes `currentStart +/- count` and ignores the drag delta. This is the earliest common cause of the full-page jump.
- The missing Y labels are structural: the row SVG emits five grid lines but no axis-label DOM at all.

## Experiment

- Add a model helper dedicated to main-plot boundary pan intent that computes `targetStart = clamp(currentGlobalStart + delta)` and keeps `ObservationCount = visible Count`; retain the existing page-oriented helper for navigator boundary gestures.
- Add five reactive tick overlays on both sides of every TA chart, driven by the existing prepared-geometry low/high state.
- First make focused tests fail for exact delta/count and bilateral DOM contracts, then implement and run the Renderer package-backed suite.

## Estimated implementation size

- Fewer than 80 non-type-declaration source lines across `RendererModel.fs` and `Renderer.fs`, plus focused tests and version/document updates.

## Scope correction from real 18884 evidence

- The original hypothesis explained the first full-page target error, but Daedilus's real run exposed a second divergence after fetch: the accepted coverage branch sets local `StartIndex = 0` even when the provider expands the resident interval around the requested window.
- The source fix must therefore preserve the exact requested active-axis ordinal twice: in the outgoing intent and when reanchoring the returned active interval.
- The no-1K run also disproved the existing lifecycle/domain assumption. `Document.IsSome` is not sufficient authority to replace the actual GLOBAL bootstrap, and `GlobalOverview.AxisRef` cannot share ordinal arithmetic with `ActiveDetail.BaseAxisRef` when they differ.
- This adds a typed lifecycle gate and an explicit active-to-global boundary mapping contract. The mapping shape is under owner/consumer coordination; no cross-axis implementation is accepted until the provider can produce it and validation/tests lock its authority.

## Coordination result

- Daedilus subsequently proved the Research backend can keep and translate the independent 1K GLOBAL domain under the existing Dynamic wire contract. The active-to-global contract extension is therefore not part of 0.2.31.
- The retained owner change is the lifecycle invariant: an actual GLOBAL bootstrap remains mounted until the authoritative loaded-coverage projection resolves a validated `GlobalOverview`.
