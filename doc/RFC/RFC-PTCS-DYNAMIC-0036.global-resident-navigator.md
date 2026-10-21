# RFC-PTCS-DYNAMIC-0036 Global Resident Navigator

- ID: RFC-PTCS-DYNAMIC-0036
- Status: Accepted / owner package and browser validation passed; formal consumer validation pending
- Date: 2026-10-21
- Owners: PTCS.Dynamic Renderer / Aster; Research host consumer / Daedalus
- Related: RFC-PTCS-DYNAMIC-0032, RFC-PTCS-DYNAMIC-0033, RFC-PTCS-DYNAMIC-0034

## Background

The existing navigator describes the observations currently resident in the client and moves the visible chart window inside that resident interval. A Research document may have a much larger durable series, for example 2018 through 2026, while the client intentionally keeps only 1,500, 3,000, or 4,500 observations resident. Reusing the existing navigator for both domains makes a global relocation look like an inner viewport move and couples resident capacity to the 4,000-detail validation path.

## Goals

1. Add an optional whole-series overview above the existing resident navigator.
2. Let one pointer release request one atomic relocation of the resident interval.
3. Preserve the inner viewport width and resident-relative offset when the resident interval moves.
4. Keep the existing projection, actions, renderer, and wire behavior unchanged when the optional overview is absent.

## Non-goals

- No Research provider, query, storage, or derived-TA implementation in the owner package.
- No second coverage authority, polling loop, or per-pointer-move remote action.
- No change to the existing inner navigator, its visible-range action, or its 4,000-detail policy.
- No hard-coded 4,500 limit in generic contracts; the consumer advertises the current resident width.

## Decision

### D1. Additive whole-series projection

`TaLoadedCoverageProjection` gains `GlobalOverview: TaGlobalCoverageOverview option`. The overview carries the bounded whole-series axis and the resident interval within it:

```fsharp
type TaGlobalCoverageOverview =
    { TotalObservationCount: int64
      AxisRef: string
      Anchors: TaOverviewAnchor array
      ResidentStartObservationOrdinal: int64
      ResidentObservationCount: int }
```

The existing outer `CoverageIdentity`, `CoverageRevision`, and `QueryGeneration` remain the only authority and settlement identity. `GlobalOverview=None` preserves the 0.2.2 wire and renderer behavior.

### D2. Separate resident relocation action

The action is independent of `VisibleRangeChanged`:

```fsharp
ResidentCoverageChanged of
    CanvasInstanceId *
    baseRowId: string *
    relocation: TaCoverageWindowIntent
```

`BaseRowId` identifies the current document/base series and remains an action field. `TaCoverageWindowIntent` remains a pure ordinal relocation intent. The new path validates positive count, matching advertised resident count, and containment inside the whole-series total. It does not apply `VisibleRangeChange.MaximumBasePoints`, `MaximumVisibleRangeBasePoints`, or `MaximumActiveDetailBars`.

### D3. Local draft and settlement

Global drag changes only a renderer-local draft. Pointer release emits at most one `ResidentCoverageChanged`. Matching coverage revision and query generation commit the projection; rejected, conflicted, or superseded actions clear or replace only their own global draft. No timer, polling loop, or action is emitted during pointer movement.

### D4. Preserve the inner viewport by relative offset

When the resident interval moves, the inner viewport keeps its width and its offset relative to the resident start. For resident width 3,000, inner offset 1,500, and viewport width 700, relocation to `newResidentStart` produces `newResidentStart + 1,500` with width 700. Near a boundary, clamp the offset while preserving width; shrink width only when the complete series itself is shorter. The old absolute ordinal is not retained.

### D5. The visible selection owns the next gesture

While a remote coverage action awaits its response or matching projection, the renderer may show a newer optimistic selection than the authoritative projection. A new pointerdown therefore takes its index baseline from the rendered or published draft and its ratio baseline from the navigator's currently rendered `data-selection-left-ratio` / `data-selection-right-ratio`. It must not clear that draft before choosing the baseline. Internal ratio overrides are fallback state only; CSS/SVG bounding-box conversion is not authority because borders and view-box scaling can introduce visible edge drift.

## Impact

- Contracts and wire codecs gain optional/additive fields and one action discriminator.
- Renderer gains a second, visually separate navigator and an independent draft/pending state machine.
- Interactive.Client transports the new action without Research-specific vocabulary.
- Research host owns whole-series anchors, durable relocation authority, resident materialization, and derived-series shared-axis updates.

## Current owner state (2026-10-22)

- Contracts and renderer source now contain the additive action allowlist, independent move-only global strip, local draft/pending settlement, and a BrowserDemo 10,000-total/1,500-resident fixture. Contracts Expecto passes 51/51 and Renderer model/source-contract Expecto passes 71/71.
- The first attempted `0.1.37 -> 0.2.9 -> 0.2.9` artifacts are retired: `GeneratePackageOnBuild=true` caused direct `dotnet pack` to reuse an older Release DLL, so those packages do not contain this UI.
- Owner candidates are Contracts `0.1.38`, Renderer `0.2.10`, and Interactive.Client `0.2.10`. All three passed fresh local-first package publication; Renderer and Interactive.Client local package SHA-256 values are `ECCD46DADDEEAD7AAE3F9BAD488552D84923E658FD4139FC88DA6387FDB6DCE1` and `84BB347B698CDC9BC15791CC842A78C29BBE2804CCA7F2378E5B4EAE72F460AA`. The package-consuming BrowserDemo build passes, and its Expecto Playwright gate passes the existing resident gesture plus the independent global relocation/drop-only action. Daedalus formal 18884 consumer validation remains pending.
- Existing `0.2.8` remains the valid chained-gesture release and does not contain the global strip.

## Acceptance

1. Existing 0.2.2 tests remain unchanged and pass with `GlobalOverview=None`.
2. Projection codec round-trips both absent and present global overview; old payloads decode to `None`.
3. Action codec round-trips `ResidentCoverageChanged` with `BaseRowId` outside the relocation intent.
4. A 4,500-observation resident relocation is accepted by the generic path and is not clamped to 4,000.
5. Drag emits no remote action; one drop emits exactly one action.
6. Settlement is revision/generation-safe and stale results cannot clear a newer draft.
7. Inner viewport width and resident-relative offset are preserved across relocation, with bounded edge clamping.
8. Daedalus formal consumer proves whole-series navigation, 2018/2026 relocation, derived-TA shared axis, and existing inner navigator behavior.
9. Pointerdown freezes the rendered selection ratios and semantic left/right/body ownership. An authoritative projection arriving before pointerup may rebuild the shell but cannot move the active preview, its fixed edge, or body width; pointerup still emits exactly one intent.
10. If a second gesture starts while the first gesture's remote response is still pending, pointerdown keeps the exact rendered edge as its baseline. The first response may arrive while the second gesture is held without moving its fixed edge, changing its body width, clearing its semantic mode, or suppressing its one later intent.
