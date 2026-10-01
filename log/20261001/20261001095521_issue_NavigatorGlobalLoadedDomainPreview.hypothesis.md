# Issue hypothesis: navigator global loaded-domain preview

## Symptom

True SPAA renders the selection against 900 loaded observations while retaining 200 active detail observations. Dragging the selection left does not visibly preview within 250ms. Release requests Earlier with an active-page-sized delta.

## Core hypothesis

`Renderer.startNavigatorDrag` mixes two coordinate systems in one gesture:

1. hit testing uses `navigatorRatios`, which maps the active window into the global loaded domain;
2. pointer delta, `previewWindowBounds`, and `navigatorBoundaryDirection` use `referenceLength()`, which is only the active detail length.

Therefore the pointer-to-observation scale and draft clamp disagree with the rendered selection. The global draft cannot move across loaded coverage locally.

## Experiment

1. Add a pure model test for global loaded count 900, active detail `700+200`, visible global `852+48`, and a pointer-derived global delta.
2. Add a BrowserDemo fixture/gate that performs pointermove without pointerup and asserts the preview label and selection ratio change while callback/render counts do not.
3. Refactor the drag session to carry explicit global domain/window and active-detail mapping. Release maps an entirely contained global draft to a local commit; other drafts use the existing typed coverage intent.

## Falsification

- If the existing intent can only express adjacent fixed pages and cannot carry the drafted ordinal/count, a Renderer-only correction is insufficient.
- If BrowserDemo passes but true SPAA still fails with matching DOM geometry and package identity, inspect consumer iframe/pointer delivery rather than adding another renderer mapping.

## Estimated change size

Approximately 45-80 non-type-declaration lines in model/renderer plus focused tests and browser verifier assertions. Exceeding 160 implementation lines requires revisiting the design before continuing.

## Experiment result

- Hypothesis confirmed: one drag mixed global selection ratios with active-detail delta/clamp.
- The correction also exposed stale serialized coverage actions in the latest-wins queue. The queue was reduced to global target data and now rebuilds intent authority at dispatch.
- The implementation diff crossed the 160-added-line review threshold because it includes mechanical queue-shape replacement, two pure coordinate helpers, diagnostics and regression coverage. Keeping the old serialized action would be smaller but would violate the existing latest-runtime rebase contract. No new public abstraction or wire type was introduced, so the two-helper plus target-only queue design remains the smaller correct boundary.
- The first final-candidate browser run reproduced a `283.08ms` preview against the `<=250ms` gate. Pointermove still performed synchronous diagnostic attribute writes on both navigator and chart stack for every Playwright step, outside the coalesced draft rAF. These writes are now coalesced into the same local rAF; release semantics and callback behavior are unchanged.
