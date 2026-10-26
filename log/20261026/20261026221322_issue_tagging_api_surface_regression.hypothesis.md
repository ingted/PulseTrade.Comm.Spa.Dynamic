# Tagging.Client 0.1.10 API surface regression

## Context

- Baseline HEAD: `196df51dec32e357f5affbcbe3806b8fdb5fb8cc`
- Branch: `20260915_033.ptcs_group_support`
- Consumer RED: Research pinned to Contracts `0.1.40`, Tagging.Client `0.1.10`, Renderer/Interactive `0.2.43`; Release compile reports 23 errors.
- External dirty paths remain out of scope: `lib-packs.txt`, `tests/PulseTrade.Comm.Spa.Dynamic.Ptcs.Tests/Program.fs`, `.pcsl/`, `temp/`.

## Proven facts and invariant

- Daedilus measured 0.1.9 DLL as 245760 bytes / 82 types and 0.1.10 as 149504 bytes / 55 types.
- Missing public contracts include TaggingSearchPlugin/Handle/Binding/Query/Page/Field/Matcher, TaggingEditorRequest.SuggestTags, TaggingEditorReply.Suggested, and Binding.Changed/InitialDraft.
- Current owner tests 7/7 did not exercise the Research consumer API surface.
- Invariant: a newer immutable Tagging.Client package must preserve the active 0.1.9 public API while adding the current UI behavior; consumer must not downgrade or accept a package-only owner test as compatibility proof.

## Hypothesis and minimum discriminating evidence

1. Primary: the current Tagging.Client project excludes a still-authoritative `TaggingSearchPlugin.fs` or equivalent source that was included when 0.1.9 was built. Compare current project Compile items, repository history, and the 0.1.9 assembly/source provenance.
2. Secondary: the API lived in a different source/project snapshot not present at current HEAD. If Git history cannot recover it, inspect 0.1.9 metadata/decompilation only to reconstruct the exact public contract, then locate canonical source before implementation.

## Planned experiment

1. Inventory only the Tagging.Client project Compile items, nearby source files, relevant Git history, and exact 0.1.9/0.1.10 package metadata.
2. Establish the earliest source divergence and recover the canonical public API without reverting the new UI work.
3. Add focused compatibility tests that reproduce representative Research compile failures before implementation.
4. Implement the smallest coherent merge, bump only Tagging.Client to immutable `0.1.11`, and run source tests, canonical WebSharper Release, exact-package compatibility build/tests, hash and RepositoryCommit parity.

## Scope and stop condition

- Owned: Tagging.Client source/project, its focused tests, directly affected current docs/log.
- Not owned: Research consumer source, existing external dirty paths, other three package source versions.
- Stop only when 0.1.11 preserves the 0.1.9 active API, retains the new UI behavior, is committed/pushed, and Daedilus receives immutable package evidence; otherwise report the exact blocker.

## Experiment

- The 0.1.9 nupkg identifies `RepositoryCommit=32db8d6865b2b8dc8c25465842a34559c86f0b32`; that object exists in `G:/PulseTrade.fs`, and its Tagging editor/search source is byte-identical to the current canonical source there.
- Restoring those two canonical files plus the search Compile item, then layering the UI behavior, makes the expanded focused suite compile and pass 9/9, including the previously missing public symbols.
- The first canonical WebSharper Rebuild failed with the known `wsfsc.exe` CLR crash. Before retrying, discriminate an orphan compiler-service/log lock from source failure by checking exact service process command lines, parent liveness, and project log ownership.
- The exact owner was orphan PID 11392 with dead parent 11884 and the Tagging project log in its command line. Stopping only that owner restored Rebuild 0 warnings/0 errors; the Research-repo orphan was not touched.
- Metadata comparison against the official 0.1.9 DLL finds zero missing public type names (31 baseline, 34 candidate) and zero missing public member names (456 baseline, 480 candidate). The added public types are the three `TaggingEditorRowSeed` symbols needed by the UI regression seam.
- Focused source compatibility and behavior tests pass 9/9. The canonical version plan and apply both report 0.1.10 -> 0.1.11, 264 scanned projects, zero dependencies/stale references, one changed project, and one mapped test.
