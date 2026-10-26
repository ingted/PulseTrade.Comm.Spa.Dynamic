# Issue hypothesis: narrow overview body drag moves only one viewport width

## Proven facts

- Consumer is using immutable `PulseTrade.Comm.Spa.Dynamic.Renderer` / `Interactive.Client` 0.2.48 built from source commit `3649f1ee364fbb88af0567413fc3fd4373575881`.
- The same run resized the overview selection to 79 bars and retained that width.
- A body drag first reached local start 1201; a subsequent large leftward body drag settled at local start 1122, exactly 79 bars earlier, with one accepted action, coverage revision 4 -> 5, and no browser/API error.
- Shared evidence is under `G:\coldfar_py\coldfar-symbolics\temp\agent.daedilus\resident-width-and-edges-0248\`.

## Expected invariant and source of truth

- A navigator body drag preserves selection width and maps the pointer delta over the navigator's logical track to the requested event-time interval.
- The source of truth is the Renderer pointer/drag-mode implementation plus the consumer's captured pointer geometry and accepted `Action$8` event-time interval.
- Handle-resize semantics must not cap or reinterpret a body move; the 0.2.48 logical-edge handle fix remains intact.

## Earliest observed divergence

- Before backend paging or chart commit, the second large leftward body gesture emitted only one-window-width earlier `Action$8`, so the earliest suspected divergence is pointer-to-logical-range calculation or hit/drag ownership in the Renderer.

## Candidate causes and distinguishing evidence

1. Body movement is clamped against the current selection edge or width instead of the full logical track. Inspect the body move calculation and compare emitted ratios with pointer delta.
2. The pointer down is classified as a resize handle because the selection is narrow. Daedilus will provide pointerdown, drag-mode, and delta geometry; compare it with handle hit regions.
3. The consumer gesture only moved by one selection width. Reject this if captured pointer delta spans materially more than the 79-bar selection width.

## Experiment boundary

- Read only the exact navigator pointer handlers and the three bounded evidence files first.
- Add a RED focused test reproducing a narrow selection body move over multiple selection widths before changing source.
- Preserve logical-edge handle anchoring, cross-axis event-time mapping, width, and existing external dirty paths.

## Experiment

- Daedilus captured a semantic `move` hit, local start/count 1201/79, delta -1202, and a correct in-drag `Preview 1-79`; the pointer and selection remained inside the track. Renderer hit ownership and preview math are therefore not the earliest divergence.
- The emitted `Action$8` contains the exact prepared active-axis event-time range for local 1-79 and no `CoverageIntent`. This is the required cross-axis local seam because the overview and active/base axes differ; it is not a GLOBAL resident relocation or adjacent-page coverage intent.
- Consumer settlement changed ActiveDetail 49646 -> 49567 and local visible start 1201 -> 1122, producing the apparent one-window step. The earliest observed divergence is now the consumer's `CoverageIntent=None` event-range settlement (`durableVisibleWindowAtActiveSpan` / attach), outside Renderer owner source.
- No Renderer source change is justified by current evidence. Daedilus owns the consumer correction and real rerun.

## Estimated implementation size

- Expected source change: under 20 non-type lines plus a focused regression test. If the change exceeds 40 source lines, stop and revise the causal model here.
