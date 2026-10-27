# Investigation: returned viewport left overscroll omits Earlier coverage intent

## Symptom

The 0.3.9 real-data formal rerun first settles the 2018 query correctly, then returns to 2026. A left-edge overscroll produces `ui:29` action case 8 with an event-time range and `MaximumBasePoints=1500`, but `CoverageIntent=None`; no Direction=Earlier request follows, so the expected `Loaded=2991` and `Viewing=1..1491` transition never occurs.

## Confirmed facts

- Query settlement is not the failing gate: Daedilus observed `pending=false`, `outcome=selected-applied`, selection/local/applied all 1491, DataRevision 24 -> 25, QueryGeneration stable 19.
- Prior `ui:3..24`, `ui:26`, and `ui:27` action-case-8 requests carry coverage intent with expected coverage revision and query generation.
- `ui:29` alone carries no coverage intent, direction, expected revision, generation, ordinal, observation count, authority, or purpose.
- `ui:29` receives HTTP 200 and exactly one frame, consistent with a prepared-frame event-range request rather than adjacent coverage expansion.

## Confirmed root cause

The returned active detail starts at query-relative ordinal 0. The active-local Earlier boundary path reused `tryAdjacentCoverageIntent`, which clamps the target back to 0. `applyLoadedCoverageIntent` therefore classified the target as fully contained by the current active detail and called the local `setWindow` path, producing a bare current-range action instead of adjacent materialization. Clean pointerdown telemetry proves the projection was present and current, and the previous action was already settled.

The canonical correction keeps the adjacent UTC event range as position authority and adds a direction-bearing, no-ordinal `NavigatorWindow` intent. The fetch cap and visible width remain separate, and a temporarily blocked action uses the existing viewport queue without weakening snapshot-key rejection.

## Minimal distinguishing evidence

1. Map action case 8 and every construction site that can omit `CoverageIntent`.
2. Compare the conditions entering canonical adjacent coverage before and after returned-view query settlement.
3. Reproduce the returned-view sequence in the existing Renderer boundary-pan test seam and assert the emitted intent, direction, generation, and requested range.

## Repository state

- Root: `C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic`
- Branch: `20260915_033.ptcs_group_support`
- Baseline: `2ed7b93ff587ec5859bfd6275e66f4d1aa363a37`
- External dirty paths are excluded from this investigation and any staging.
