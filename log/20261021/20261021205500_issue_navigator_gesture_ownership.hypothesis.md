# Navigator gesture ownership and rerender hypothesis

## Phenomenon

Daedalus reproduced in formal 0.2.2 with 4,500 resident observations and an inner viewport near 1,000: left/right/body gestures sometimes change ownership or geometry before pointerup, especially after a pause during drag.

## Proven facts

- `startNavigatorDrag` captures one `drag` value in the pointerdown closure.
- SVG handle and body hit rectangles use `pointer-events:none`; ownership is inferred only from geometry on the root.
- move/up/cancel listeners are attached to the current SVG after `SetPointerCapture`; the SVG lives under reactive `Doc.EmbedView` and may be replaced while the gesture is active.
- `draftWindow` is reactive, while `renderedNavigatorDraft` is overwritten by an `afterRender` receipt.

## Hypothesis

The earliest shared cause is missing semantic gesture ownership plus listener lifetime tied to a replaceable DOM node. Reactive replacement can detach the captured element/listeners while authoritative state rerenders the selection, producing apparent mode changes, width changes, or snap-back.

## Experiment

1. Give left/right/body hit rectangles explicit pointer-enabled mode metadata.
2. Resolve and freeze that mode once at pointerdown; geometric resolution remains fallback only.
3. Bind active move/up/cancel to `document`, so SVG replacement cannot orphan the gesture.
4. Preserve the closure-owned committed window and draft until pointerup/cancel; keep exactly-once finish guard.
5. Add owner tests/source gates for semantic targets, document listener lifetime, fixed-width body move, and isolated left/right bounds.

Estimated changed non-type lines: under 45. If the patch exceeds 90, stop and re-evaluate instead of widening the renderer.

## Experiment result

- Daedalus trace confirmed semantic ownership and replaceable-SVG listener lifetime as P0 defects.
- A second owner defect was exposed by the live browser oracle: committed cross-axis selection used timestamp slots while a global resident draft used ordinal ratios. `overviewSelectionRatiosForGlobalWindow` now maps resident drafts through the same canonical timestamp authority.
- The original body target maintained a separate derived geometry View that remained width zero after reactive data became ready. The body target now reuses selection geometry and precedes edge handles in SVG order; handles therefore retain edge priority.
- Renderer Expecto passed 68/68. Expecto Playwright passed 1/1 with reactive refresh during every gesture, exact left/right/body geometry invariants, Preview 431, wire ordinal 430, count 200, actionCount 1, and zero console errors.
- The implementation exceeded the initial 45-line estimate because the browser oracle found the two additional coordinate/hit-geometry causes. Scope stayed inside the existing navigator model, DOM binding, fixture, and focused verifier; no new wire action or renderer abstraction was added.
