# Renderer compound-candle activation emits gap time

## Context

- Baseline HEAD: `80d304cd7b9440834dac008a3ed45197c06f1250`
- Branch: `20260915_033.ptcs_group_support`
- Consumer RED: `Research.TaggingScaleMatrix.playwright.expecto.fsx` clicks the actual rendered compound candle SVG path. Candle and Heikin-Ashi 60K traces emit `TaPointActivation` v2 at `2026-03-24T21:00:00Z`, but the authoritative ES_60K series has adjacent bars at 20:00 and 22:00; 21:00 is the real maintenance gap. SMA60 and DMI60 on the same chart resolve correctly.
- Evidence: `G:/coldfar_py/coldfar-symbolics/temp/agent.daedilus/tagging-scale-matrix/result.txt`.
- External dirty paths remain out of scope: `lib-packs.txt`, `tests/PulseTrade.Comm.Spa.Dynamic.Ptcs.Tests/Program.fs`, `.pcsl/`, `temp/`.

## Invariant and source of truth

- Clicking a rendered trace datapoint must emit that selected trace datapoint's exact authoritative `EventTimeUtc`.
- Raw x-axis interpolation is only a geometric candidate; it must not fabricate a timestamp in a market gap when a concrete rendered point was hit.
- The resolver remains exact and returns 404 for a true gap. It must not be weakened to nearest-point behavior.

## Hypothesis

The compound candle hit path derives activation time from the shared x-scale inverse mapping, while line-based traces retain their bound datapoint identity. At discontinuities, interpolation lands in the missing bucket even though the SVG hit belongs to an adjacent actual candle.

## Planned experiment

1. Locate `TaPointActivation` v2 construction and the compound candle hit-test data path; compare candle/Heikin-Ashi with successful SMA/DMI activation.
2. Add focused 30K/60K regression cases with a deliberate maintenance gap, proving current output selects the gap.
3. Carry the selected rendered datapoint identity through hit resolution and emit its exact `EventTimeUtc`; preserve trace/row/scale identity and all non-gap behavior.
4. Run Renderer focused/full tests, Interactive affected tests, canonical WebSharper Release, exact-package verification, checker/sensitive gates, immutable version bump, commit/push, and consumer handoff.

## Scope and stop condition

- Owned: Renderer hit resolution, directly affected Renderer/Interactive tests and exact dependency/version edges, current docs/log.
- Not owned: Research source, exact resolver behavior, unrelated Contracts null-string path, external dirty paths.
- Stop when 30K/60K gap regressions and existing suites pass, immutable packages are committed/pushed with exact SHA/RepositoryCommit, and Daedilus can rerun the real Playwright gate.

## Experiment 2026-10-26 23:47 +08:00

- Renderer `pointHitCandidates` creates candle candidates with `EventTimeUtc = point.Timestamp`; `tryNearestPointHit` only chooses among existing candidates by rendered x/y distance, and `TaPointActivation.tryCreate[V2]` copies the selected candidate timestamp unchanged. Existing DYN-T-688 explicitly verifies receipt time equals hit time.
- The earliest contradictory authority is consumer-owned `G:/coldfar_py/coldfar-symbolics/PulseTrade.Comm.Spa.Dynamic.FsStl.Spaa.Research/ResearchDurableWindowProjection.fs`: `canonicalEventTime` returns `point.EndUtcExclusive`, and both KBar `barRow` and Heikin-Ashi rows publish that bar-end as chart `EventTimeUtc`.
- The exact resolver in `ResearchTaggingStore.fs` queries `StartUtc <= eventTimeUtc AND EndUtcExclusive > eventTimeUtc`. Therefore a 30K bar-end silently selects the next contiguous bar, while a 60K bar-end at the maintenance boundary selects no point and returns the observed 404. Line/scalar series can remain green because their persisted event-time convention differs.
- This falsifies the initial Renderer hypothesis. Renderer has no source-authoritative stored event time from which it could correct the consumer's projection without scale inference or nearest lookup, both forbidden by the acceptance contract.
- Sent owner-boundary correction and exact source/SQL evidence to Daedilus in `msg-fsi-fa5f914da72046fa98130448770db754`, requesting that Research project the stored authoritative `point.EventTimeUtc` and additionally prove that 30K resolves the clicked PointId rather than the next bar.
- No Renderer/Interactive source change or package bump is justified. This investigation waits for consumer correction or a bounded counterexample where Renderer receives the correct candidate time but emits a different receipt time.
