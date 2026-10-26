# Contracts runtime snapshot null-string decode failure

## Context

- Baseline HEAD: `80d304cd7b9440834dac008a3ed45197c06f1250`
- Branch: `20260915_033.ptcs_group_support`
- Consumer RED: Research exact graph uses Contracts `0.1.40`, Tagging.Client `0.1.11`, Renderer/Interactive `0.2.43`; Release compiles with zero errors and host health is green, but the full runtime snapshot backend gate fails at packet 0.
- Exact failure: `RuntimeSnapshotTransportAssembler.decodeFrames`, code `runtime-snapshot-chunk-decode-failed`, message `Failed to deserialize value "null" as type System.String`.
- External dirty paths remain out of scope: `lib-packs.txt`, `tests/PulseTrade.Comm.Spa.Dynamic.Ptcs.Tests/Program.fs`, `.pcsl/`, `temp/`.

## Invariant and source of truth

- Transport encode and decode must agree on nullable wire data produced by the active real snapshot path; a JSON null representing an absent string must not abort the whole bounded chunk unless the contract explicitly forbids absence at that field.
- The earliest serializer/deserializer seam and active wire type are authoritative, not the outer packet error or a consumer workaround.
- No payload dump: identify the failing field/type with a minimal fixture or bounded metadata only.

## Hypotheses

1. The active encoder emits JSON null for an F# `string` field whose decoder uses a serializer policy that rejects null for non-nullable strings. A field-level default/option conversion is missing at the transport boundary.
2. The assembler selects a codec/configuration different from the encoder's canonical serializer, so a value accepted during serialization is rejected only after chunk reassembly.

## Planned experiment

1. Locate `RuntimeSnapshotTransportAssembler`, its frame codec, and every string-bearing wire record used by packet 0; map the exact encode/decode call chain.
2. Add the smallest regression fixture containing JSON null at the active field and prove the current decoder fails before changing code.
3. Fix the earliest shared codec/type boundary, preserving non-null strings and all existing wire fields; add normal/null/restart-or-multi-chunk regression coverage.
4. Bump Contracts immutably, synchronize only required active package consumers, run full Contracts tests plus affected Renderer/Interactive/Tagging source and exact-package gates, then hand off SHA/RepositoryCommit/dependency graph.

## Scope and stop condition

- Owned: Contracts codec/wire types and tests, directly required active package references, current docs/log.
- Not owned: Research consumer source, external dirty paths, unrelated rendering/UI behavior.
- Stop only when the null fixture and existing suite pass, the new immutable package is committed/pushed with exact evidence, and Daedilus can resume the real backend/full gate.

## Experiment 2026-10-26 23:35 +08:00

- Live `http://127.0.0.1:18884/` was exercised through the same RUN UI path with an ignored task-local Playwright probe. It emitted a 2,366-byte Document frame and an approximately 1.98 MB Snapshot frame; recursive `System.Text.Json` inspection found zero JSON literal-null paths in both.
- The live Document and Snapshot both decoded successfully through `BrowserRuntimeCodec.decode` when the probe loaded Contracts `0.1.40`. Repeating against Contracts `0.1.36` also decoded both successfully. This falsifies both initial hypotheses for the currently active host response.
- Daedilus evidence `G:/coldfar_py/coldfar-symbolics/temp/agent.daedilus/research-full-playwright/20261026T225000/03-global-backend/stdout.log` proves the earlier failure, but its stack identifies the canonical `Research.GlobalOverviewBackend.playwright.expecto.fsx`; that file still pins Contracts `0.1.36` and Renderer `0.2.3`, not the reported exact graph `0.1.40`/`0.2.43`.
- Root cause is therefore not yet attributable to Contracts `0.1.40`: the failing wire body was not preserved, the current live body is compatible with both decoder versions, and the consumer package-load evidence conflicts with the handoff description. Changing the codec now would be speculation and could weaken required-string invariants.
- Sent bounded evidence and an exact-reproduction request to Daedilus in `msg-fsi-e3c2c2d766f0435d95add064df06f3e2`. Required next evidence is an exact `0.1.40`/`0.2.43` test copy plus loaded package proof; a repeated failure must retain frame 0 SHA/length/null JSON paths without dumping the payload.
- Source remains unchanged. The prior planned implementation/version-bump path is suspended until one reproducible failing field or serializer mismatch is identified.
