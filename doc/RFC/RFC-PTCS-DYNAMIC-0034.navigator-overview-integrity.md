# RFC-PTCS-DYNAMIC-0034 Navigator Overview Integrity

- ID: RFC-PTCS-DYNAMIC-0034
- Status: Owner verified / release and consumer acceptance pending
- Date: 2026-10-01
- Owner: PTCS.Dynamic Renderer / Aster
- Related: RFC-PTCS-DYNAMIC-0032, RFC-PTCS-DYNAMIC-0033, RFC-TRADECORE-0030

## Background

The renderer accepts as many as 8,000 loaded-coverage anchors but still reduces the navigator with uniform
point dropping. A short-lived high/low can therefore disappear even though the source carries authoritative OHLC.
The current OC fallback also manufactures high/low from open/close and makes a body-only observation look like
an authored wick. Drag delivery is already local-first, but cursor affordance and loaded-start/end navigation are
not explicit owner contracts.

## Goals

1. Preserve ordered source coverage and candle extrema in a bounded navigator representation.
2. Never invent wick values for OC/scalar observations.
3. Keep pointer movement local and frame-bounded; commit one typed viewport intent on release.
4. Add typed loaded-start and loaded-end navigation without adding a second viewport wire contract.

## Non-goals

- No TradeCore signal/order/fill vocabulary.
- No change to the 8,000-anchor wire limit, 4,000-detail limit, provider backfill, or scenario authority.
- No per-pointer-move remote callback and no DOM node per source anchor.

## Decisions

### D1. Contiguous candle buckets

The renderer maps accepted anchors to ordered candle points, partitions the complete sequence into at most 280
contiguous buckets, and emits first open, last close and summed volume. It emits max high/min low only when every
source point in that bucket has authored high and low. Source start/end ordinals remain explicit test evidence.

### D2. Body-only observations

OC and scalar anchors remain body-only. Their open/close still participate in Y bounds and body polarity, but the
wick path omits them. Mixed buckets are also body-only, because partial wick data is not sufficient authority for
bucket extrema.

### D3. Drag interaction

Left/right handles use `ew-resize`; the selection uses `grab` and active move uses `grabbing`. Pointer movement
updates only the latest local draft through one animation-frame callback. Pointer release/cancel clears capture;
release commits at most one existing `TaCoverageWindowIntent`.

### D4. Loaded edge controls

`Start` and `Latest` derive exact head/tail ordinal windows from current `TaLoadedCoverageProjection` and reuse the
existing visible-range action. The edge choice is a renderer-local typed DU, not a new wire discriminator. Missing
or invalid loaded coverage yields no action.

## Impact and risks

- Renderer, Interactive.Client and Ptcs.Client require exact version bumps because both clients carry renderer
  bundle/package identity.
- Aggregation is O(source count) with bounded output. Tests must prove a hidden one-point spike survives reduction.
- Consumer SPAA remains responsible for true loaded coverage and source OHLC/OC; the renderer does not backfill.

## Acceptance

1. 8,000 ordered candles compact to at most 280 contiguous buckets covering every source index exactly once.
2. A single interior high/low spike remains in the corresponding aggregate bucket.
3. OC/scalar and mixed-authority buckets emit no wick geometry.
4. Browser cursor states are `ew-resize`, `grab`, and `grabbing`; pointerup produces one callback.
5. Start/Latest move to exact loaded head/tail windows through the existing typed action and retain the loaded
   maximum clamp.
6. Full WebSharper/browser tests have no console errors and no formal phase over 100 ms.
7. Exact packages, official signatures/dependencies and Daedalus SPAA consumer gate complete before closure.
