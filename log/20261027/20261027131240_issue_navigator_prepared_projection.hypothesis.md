# Hypothesis: navigator snapshot reads the wrong half of the committed frame

## Proven facts

- 0.2.58 is deployed and its generated live branch calls `tryNavigatorEventRangeIntent` before `setWindowFromPreparedRange`.
- The formal ui:4 still omits `CoverageIntent`; therefore that branch is not reached or its helper returns `None`.
- Count 79, coverage revision and query generation are valid, so `tryWindowIntent` cannot reject the intent on validation.
- `sendOrQueueVisibleRangeAction` stores and later dispatches the complete `SduiAction`; it does not reconstruct or strip the record.
- The navigator displayed to the user resolves coverage from committed prepared `RawData`.
- `loadedCoverageSnapshot` instead resolves coverage from the paired runtime state's `Data`, then has an explicit `None -> setWindow` bare-action fallback.

## Expected invariant and earliest divergence

The committed frame is an atomic pair. Render and pointerdown must resolve projection and timeline from its prepared half. The earliest divergence is reading `dragState.Data` for the pointerdown projection while reading `dragPreparedData` for its timeline and UI. This creates a mixed authority even without mixing revisions.

## Minimal experiment

Extend the Renderer source-contract gate to require `tryLoadedCoverageResolved document.DefaultView dragPreparedData.RawData` at pointerdown and forbid the corresponding `dragState.Data` expression. Against 0.2.58 this is RED. The implementation is a one-expression authority correction; no fallback to current/latest state and no consumer change.

## Estimated patch

One production expression, focused source assertions, two version pins, and traceability updates; no algorithm or wire change.
