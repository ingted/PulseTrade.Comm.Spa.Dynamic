# Renderer first formal paint latency

## Context

- Baseline HEAD: `80d304cd7b9440834dac008a3ed45197c06f1250`
- Branch: `20260915_033.ptcs_group_support`
- Consumer RED on current Renderer/Interactive `0.2.43` and Tagging.Client `0.1.11`: cold first paint 24,681 ms after a 12,328 ms RUN response; warm first paint 15,394 ms after a 6,012 ms RUN response.
- Warm response body is 3,720,012 characters. Client JSON parse 9.4 ms, frame decode 82.1 ms, reduce 95.6 ms, apply 180 ms, runtime `Var` commit 2 ms; the remaining post-response-to-visible gap is approximately 9.4 seconds.
- Formal SLA is first paint <=3 seconds. Consumer evidence: `G:/coldfar_py/coldfar-symbolics/temp/agent.daedalus/run-first-paint/run-first-paint.json`.
- External dirty paths remain out of scope: `lib-packs.txt`, `tests/PulseTrade.Comm.Spa.Dynamic.Ptcs.Tests/Program.fs`, `.pcsl/`, `temp/`.

## Invariant and source of truth

- A 4,500-resident/1,500-visible projection must publish a useful formal first paint within 3 seconds after RUN initiation, without dropping authoritative data or weakening projection commit atomicity.
- Renderer telemetry (`data-renderer-prepare-*`, pending-to-commit, visible-value metrics) is the source of truth for the client-side phase boundary. Server load and transport decode are separately owned and already measured.
- Optimize the earliest repeated or unbounded projection work; do not hide latency by changing the readiness/paint gate.

## Hypotheses

1. Preparation schedules too many fine-grained work items/turns for the resident map, so queue/turn overhead dominates even though decode/reduce are bounded.
2. Visible rendering recomputes or materializes resident-sized trace projections repeatedly before the commit gate, making work proportional to rows x traces x 4,500 rather than the 1,500 visible window.

## Planned experiment

1. Read the preserved telemetry evidence and map each field to the Renderer preparation/scheduler/commit code path.
2. Reproduce the exact phase with existing focused performance tests or a bounded owner fixture; establish one RED threshold and work-count/allocation proxy.
3. Remove the earliest redundant scheduling/projection pass while preserving atomic generation cancellation, 4,500 resident authority, and all 1,500 visible values.
4. Run focused performance/correctness tests, full Renderer and affected Interactive suites, canonical WebSharper Release, immutable version/dependency graph, checker/sensitive gates, commit/push, and consumer handoff.

## Scope and stop condition

- Owned: Renderer preparation/scheduler/commit path, directly affected tests/package references, current docs/log.
- Not owned: Research server load, consumer test source, Research KBar event-time projection, Contracts null-string evidence gap, external dirty paths.
- Stop when the owner benchmark and correctness suites pass, immutable package evidence is committed/pushed, and Daedilus can rerun real first-paint acceptance.

## Experiment 2026-10-27 00:03 +08:00

- Three live runs reproduce 15.6–18.0 s first paint. The final Renderer generation used 100 preparation tasks across 34–37 RAF turns: 1.08–1.23 s wall, 0.28–0.33 s work, and 0.80–0.90 s queue wait. Projection pending-to-commit was 0.35–0.39 s; visible-value work was 0–2 ms.
- The full timeline shows the remaining delay is outside Renderer: latest run had RUN response at 5.96 s, `/api/workspace/actions` response at 11.47 s, action HTTP/body 4.52 s for 30,926,931 characters, action frame wall 3.36 s, then Renderer generation 4 prepare 1.16 s and commit 0.38 s. Absolute <=3 s cannot be reached by Renderer alone; Daedilus owns server/action-frame reductions.
- Renderer still has a measurable owner defect: the 4 ms interactive budget is also used while the screen is blank for full preparation, creating approximately 0.88 s of RAF queue wait. Incremental work needs the 4 ms responsiveness budget; full initial preparation can use a bounded 16 ms budget and still yield every browser turn.

### Pseudocode before change

```text
preparationBudget(isInitial):
  if isInitial then 16ms else 4ms

createPreparationSchedule(generation, budget):
  createBudgetedWorkScheduler(budget, 64, requestAnimationFrame, clock, publishMetrics)

full preparation -> createPreparationSchedule(generation, preparationBudget(true))
incremental preparation -> createPreparationSchedule(generation, preparationBudget(false))

regression:
  schedule 100 atomic 3ms units with fake 16ms RAF waits
  assert same FIFO work/time, initial turns <=17, incremental turns=50
```

- Estimated non-type-declaration change: product 7 lines, regression approximately 35 lines, current docs/log approximately 12 lines. If product code exceeds 14 lines, append the reason before continuing.

## Experiment 2026-10-27 00:05 +08:00

- Implemented the policy seam with 16 ms for initial full preparation and 4 ms for incremental or coverage-replacement preparation. Scheduler implementation, 64-work count bound, ordered atomic work, generation cancellation, and commit gate are unchanged.
- Canonical `bumpVersion.fsx` plan could not be used because it scans ignored task-local and legacy projects as active and demanded unrelated mappings. It made no mutation. The bounded fallback changed only the two package versions, the exact Renderer dependency, and the two direct test dependencies from 0.2.43 to 0.2.44; active `src/` and `tests/` have no remaining 0.2.43 references.
- First canonical pack failed with existing WebSharper `WS9001 Type not found` errors. The restore authority was not the current Contracts nupkg: global cache `Contracts/0.1.40` held a 666,112-byte DLL (`D4D274...`), while canonical SDK library-packs and source package held the 1,281,536-byte DLL (`DEFAB032...`). No compiler service was running. A task-owned fresh `RestorePackagesPath` resolved the canonical DLL and the same pack succeeded without clearing global cache.
- Pre-commit WebSharper-enabled Renderer and Interactive 0.2.44 packages built. Fresh exact-package regression: focused policy 1/1, Renderer 100/100, Interactive 20/20, all with zero ignored/failed/errored. These packages remain candidates until source/docs commit and final RepositoryCommit rebuild.
