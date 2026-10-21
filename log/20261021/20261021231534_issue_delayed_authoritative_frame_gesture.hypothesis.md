# Issue hypothesis: delayed authoritative response during active navigator gesture

## Symptom

Daedalus formal consumer on exact Contracts 0.1.36 / Renderer 0.2.5 / Interactive.Client 0.2.5 still fails the 12-gesture gate with fixed seed 20261021. During a left-handle gesture, a delayed previous authoritative response moves the fixed right edge by 57.359985px and changes body width by 47.800171px; 16 stress failures include geometry movement during the gesture, settled geometry flashback, and missing authoritative response.

Evidence is owned by Daedalus at `G:\coldfar_py\coldfar-symbolics\temp\agent.daedalus\formal-overview-edge-drag\gesture-ownership.json` and `.png`; the owner repo will read it but not modify it.

## Confirmed facts and invariant

- NuGet package provenance matches owner authority: Renderer DLL SHA-256 `DECB522734C69A2334DD1129CEE241AEA09307F8B800D9A481946E1C3E77B4A7`, Client DLL SHA-256 `43DC5EE32CB849DE60AA4909FAF75E36CC898F0386F9CC12B239D331C2E507B9`.
- Mode classification remains `resize-left` / `resize-right` / `move`; the failure is geometry ownership, not hit-target classification.
- From pointerdown until terminal pointerup/cancel, the rendered selection and fixed edge/body width must remain owned by the gesture snapshot regardless of previous or newer authoritative frames. Exactly one release action may be sent.

## Root-cause candidates

1. **Delayed-response ordering gap:** the owner fixture covers a newer same-identity projection but not a delayed response for the previous action. A response/reducer path may replace or settle selection outside the active-gesture guard, or the replacement chart-stack may observe a transient authoritative value before the override is published.
2. **Browser asset provenance gap:** the package DLL is correct, but the WebSharper content asset consumed by Daedalus may not contain the f40 active-override code. This is independent of DLL hash and must be verified by package asset content/hash.

## Minimal experiments

1. Project only the failing gesture timeline/geometry/error fields from Daedalus JSON; do not load the whole file into context.
2. Compare Renderer source active-gesture state transitions with Daedalus fixture line 607 and delayed-response injection.
3. Inspect official/local Renderer nupkg WebSharper assets for active-override markers and byte parity.
4. Add one owner regression that injects a delayed previous response between pointerdown and pointerup; reproduce RED before changing behavior.

## Change estimate

Expected behavior patch: about 15-35 non-type-declaration lines in Renderer plus one parameterized regression case and fixture wiring. If the implementation exceeds twice that estimate, stop and revise the causal model.

## Experiment 1 - reproduced root cause

- Official/local Renderer DLL contains the embedded WebSharper JavaScript resources and all three f40 markers (`navigatorSelectionRatiosOverride`, `activeNavigatorDragMode`, `data-drag-mode`); package asset omission is rejected.
- Daedalus trace shows the explicit frame-after-move sample itself is stable, but the fixed right edge has already shifted before the first held sample.
- Owner focused Playwright now starts a remote coverage action, holds its 750ms response, and begins the next left-handle gesture against the still-rendered optimistic draft. Existing 0.2.5 code reproduces RED: `baseline=700`, `during=1000` at `chained pointerdown reset the optimistic right edge`.
- Earliest divergence is `startNavigatorDrag`: it derives `committed` from authoritative `uiState`, then unconditionally clears `renderedNavigatorDraft` and `draftWindow`. Loaded-domain coverage actions intentionally keep their optimistic visual authority only in those draft values until the remote response arrives.
- A second confirmed defect explains the `mode=null` trace: replacement `ta-chart-stack` receives `data-drag-mode`, while the replacement `ta-overview-navigator` does not.

The causal model is now singular: chained gestures must begin from the visible draft authority and replacement navigator nodes must inherit the active semantic mode.

## Experiment 2 - visible ratio authority

- Preserving the rendered/published draft fixed the index authority but reusing a stale internal ratio override still changed the next fixed edge.
- Reading the SVG selection client rectangle reduced the error to 2.51px but remained incorrect because the navigator root client width includes its border while the selection is authored in the 0..1000 view box.
- The navigator already publishes data-selection-left-ratio and data-selection-right-ratio from the same reactive view that authors the selection rect and handles. Pointerdown now uses those rendered values first, then falls back to the internal override and committed-window model.
- Candidate 0.2.8 is owner GREEN: Renderer 69/69 and the focused BrowserDemo Playwright gate 1/1. This confirms the causal model locally; formal Research consumer validation is still required.
