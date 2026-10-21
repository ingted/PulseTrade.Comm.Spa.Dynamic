# PTCS Dynamic navigator optimistic viewport

- Started: 2026-10-21 17:26:46 +08:00
- Owner: Aster
- Consumer evidence: agent.daedalus direct messages `msg-fsi-1d17657b880f48da9410d300969b6778`, `msg-fsi-8566dc75bb2d41b2aeda2ca8501408e6`, `msg-fsi-0c4d6954f62d4ce2996ed378769d7eb7`
- Canonical repo: `C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic`
- Branch / baseline: `20260915_033.ptcs_group_support` / `23b4814e4a584264faf8653eab17390defb466c0`

## Pre-work Plan

1. Preserve existing `Renderer.fs` dirty diagnostics exactly; baseline SHA-256 is `cd55fc5a5d401c5be4b84e4cadc8a39b76f551d1c7b1b8d47735d6d3fff3340d` and pre-existing diff is 11 added diagnostic lines.
2. Reproduce the state transition with a focused failing owner test: committed global window `1-1500`, optimistic target `1499-2998`, delayed action response, accepted projection, and rejection rollback.
3. Keep one authority: `pendingBoundaryPan` owns projection correlation; `draftWindow` owns optimistic presentation. Do not add durable state or a parallel viewport authority.
4. Patch queue, accept, reject, and matching projection cleanup; local-only viewport commits remain immediate.
5. Run focused Renderer tests, WebSharper compile/package gates, inspect exact nupkg dependency/provenance, then prepare Renderer `0.1.132` and Interactive.Client `0.1.124`. NuGet push is a separate gate.

## Hypothesis

The formal package already detects navigator boundary direction, but `finish` clears `draftWindow` immediately before dispatching an asynchronous coverage action. The committed viewport therefore renders until the accepted projection arrives. Rejection has no optimistic viewport to roll back, and a queued viewport intent can overwrite `pendingBoundaryPan` before its matching projection commits.

Expected invariant: an asynchronous coverage action retains the latest optimistic global window until its matching projection commits; rejection/error/revision conflict clears it once; a queued newer intent retains its own optimistic window and dispatches only after the prior projection authority settles.

Estimated product-code change: about 25 non-type lines in `Renderer.fs`, plus focused tests and exact package version references.

## Operations

- 2026-10-21 17:26:46 +08:00: Created pre-work hypothesis before product modification. No source, package, service, or NuGet mutation yet.
- 2026-10-21: Implemented the focused optimistic viewport state transition and obtained a local-only Renderer `0.1.132` package plus 65/65 Renderer tests, then produced a local-only Interactive.Client `0.1.124` package. Neither candidate was pushed to NuGet.
- 2026-10-21 Correction: The version plan above was invalid under the repository release rule. The user requires patch counters above 50 to roll the minor version and reset the patch number, without alpha, beta, or win suffixes. The deliverable versions are therefore Renderer `0.2.0` and Interactive.Client `0.2.0`; every exact Renderer dependency is `[0.2.0]`. The incorrect local candidates will be removed after the corrected packages pass the same gates.
- 2026-10-21: Kept `draftWindow` through an asynchronous coverage action, cleared it only after the matching projection or rejection, serialized coverage and local-range intents behind `pendingBoundaryPan`, and cleared stale drafts when document identity or viewport scope changes. Existing 11-line drag diagnostics in `Renderer.fs` were preserved.
- 2026-10-21: Removed the local-only `0.1.132` / `0.1.124` nupkg files. A repeated same-version `dotnet pack` initially returned success while leaving the older DLL and nupkg unchanged. The stale candidate was rejected by comparing source, DLL and packaged-DLL timestamps/hashes. A project `Rebuild` followed by exact nupkg replacement and project `Pack --no-build` produced Renderer `0.2.0` SHA-256 `7FA0ADB1F6B18AA9997D7A32218550CD5D6870B82C92583FF004DA75127EC683`; its packaged DLL equals the rebuilt DLL SHA-256 `E889B8486910ABA6876B327CF05300B16FC67BB18883D8E6943805224362BD90`.
- 2026-10-21: Cleared the isolated same-version package cache, restored Renderer.Tests against exact `PulseTrade.Comm.Spa.Dynamic.Renderer/0.2.0`, rebuilt the executable and ran its actual Expecto entrypoint: 65 passed, 0 ignored, 0 failed, 0 errored.
- 2026-10-21 Blocker: Interactive.Client `0.2.0` rebuild is not complete and no package was published. Ignored generated file `src/PulseTrade.Comm.Spa.Dynamic.Interactive.Client/websharper.log` has an anomalous ACL: no process holds it, but the current identity cannot open, read ACL, take ownership, reset ACL, or delete it. WebSharper `wsfsc.exe` fails before compilation while trying to delete that file. The user was asked to remove this exact file from an elevated PowerShell before the canonical rebuild resumes.
- 2026-10-21 18:35:30 +08:00 Correction: Consumer owner confirmed the current P0 is the resident-local viewport lifecycle. The earlier operation entry describing a broader coverage-boundary patch is superseded: the final source changes only retain the optimistic local draft through the asynchronous visible-range action, keep it when a newer local viewport intent is queued, clear it once on Accepted/Rejected when no newer intent exists, and clear stale drafts on document identity or viewport-scope change. Existing 11-line drag diagnostics remain byte-preserved external WIP; no RN/GW/LINE path changed.
- 2026-10-21 18:35:30 +08:00: Removed 28 recursive generated websharper.log files with Git rm.exe; verified remaining count 0 and no wsfscservice process. Renderer 0.2.0 explicit Rebuild completed with 0 errors. Fresh local-only package SHA-256 is 9495A6485323CA92DEE25F237D5CE2E63D8792A57DFF4776AC5B847AD03A112D; packaged DLL equals rebuilt DLL SHA-256 51F6E8138EBD78C3BE0A1BC9976185F8A4D3309FD7D5E1AB21E01D5F22212620.
- 2026-10-21 18:35:30 +08:00: Restored Renderer.Tests through an isolated NuGet cache with the local Renderer feed, confirmed assets resolve PulseTrade.Comm.Spa.Dynamic.Renderer/0.2.0, rebuilt the executable, and ran its Expecto entrypoint: 65 passed, 0 ignored, 0 failed, 0 errored.
- 2026-10-21 18:35:30 +08:00: Interactive.Client 0.2.0 isolated restore/Rebuild/local pack completed with 0 errors. Assets resolve Renderer 0.2.0, nuspec dependency is exact [0.2.0], package contains DLL/WebSharper JS/manifest, packaged DLL matches rebuilt DLL SHA-256 C9E8FB9402C2B365D41F9FE7C2CA3B9091731156B7240AFF758142F25383696E; local candidate package SHA-256 is 790FB21462C61045E77FE34BC1EAA177CBAD8DD2DE2260BA983D701DC778F3AC. No NuGet push has occurred.