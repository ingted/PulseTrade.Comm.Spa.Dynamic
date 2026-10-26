# Issue hypothesis: narrow overview left-edge event-time mapping

## Proven facts

- Daedilus reran immutable Renderer/Interactive.Client 0.2.48 and captured a semantic body move from local `1201/79` with delta `-1202`; in-drag preview reached `1-79` on a `Loaded 4500` navigator.
- The emitted Action$8 used `price-30k` event time `2022-03-11T10:00Z -> 2022-03-15T02:30Z`. The settled first visible canonical event time equals the action start, but the rendered local window became `1122/79`, so the selection jumped back into the middle and could not continue left-edge overscroll.
- `startNavigatorDrag` freezes both the active prepared timeline and `TaLoadedCoverageProjection`. The cross-axis local commit currently calls `setWindow`, whose `visibleEventRangePrepared` always indexes the active prepared timeline; it does not consult the resident overview timeline used by the navigator.
- Evidence authority: `G:/coldfar_py/coldfar-symbolics/temp/agent.daedilus/resident-width-and-edges-0248-event-time/left-edge-80-{gesture,action,event-time}.json` and direct message `msg-fsi-14291c8d4174417ba587787d3627efba`.

## Expected invariant and source of truth

- A cross-axis navigator body drop is an event-time `VisibleRangeChanged`, never a fabricated GLOBAL ordinal.
- Its event-time boundary must come from the same resident overview geometry that produced the preview. When overview and active prepared timelines have exact one-to-one lengths but different event times, preview local `0/count` maps from overview local `0`, not active prepared local `0`.
- Same-axis resident coverage intent, CURSOR main pan, resize width, logical handle anchor, and wire shape remain unchanged.

## Earliest divergence and candidate cause

- The earliest divergence is before transport: local drop dispatch derives Action$8 from `referenceTimelineForDocumentPrepared`, even though the rendered navigator source is `tryOverviewTimeline projection`.
- Candidate root cause: geometry and action range use two different timeline authorities. Distinguishing RED: construct equal-length active and overview timelines with different first timestamps; cross-axis navigator must select overview, while same-axis and invalid/mismatched overview must preserve active fallback.

## Experiment boundary

- Add one pure timeline-authority selector and one focused regression before source wiring.
- Wire only the navigator local-drop path through the frozen projection; keep `setWindow` for all other callers.
- If exact mapping requires lossy ratio interpolation, a new wire field, or consumer changes, stop and return to design.

## Estimated implementation size

- Pure helper and focused test: about 15-30 non-type lines.
- Shared range resolver plus one navigator branch: about 15-35 non-type lines. If total exceeds 70 source lines, stop and revise this hypothesis.
