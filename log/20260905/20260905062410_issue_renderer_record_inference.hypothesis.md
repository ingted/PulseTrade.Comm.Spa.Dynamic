# Renderer test record inference regression

- Start: 2026-09-05 06:24:10 +08:00
- Repo: `C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic`
- Branch: `20260715_030.win.TACanvas_cross.bar.cursor.first.done`
- Symptom: Renderer test project fails with `FS0656` at `TAResearchRendererTests.fs` lines 197, 245 and 401 after the DYN-TA-017D editor catalog changes.
- Hypothesis: New overlapping record labels make three untyped test fixtures infer fields from inconsistent records. Explicitly constraining each fixture to its intended public contract type should restore compilation without changing runtime behavior.
- Affected scope: `tests/PulseTrade.Comm.Spa.Dynamic.Renderer.Tests/TAResearchRendererTests.fs` only unless evidence identifies a production contract defect.
- Verification: Renderer suite must pass; then run PTCS and Ptcs.Client suites to confirm the shared wire remains compatible.
- Repo state: Existing 26-file DYN-TA-017D implementation is user/session work and will not be reverted.

## Experiment

- Evidence: Renderer tests consume immutable `Renderer alpha33 -> Contracts alpha12`; the dirty source introduces `TaWorkspaceDocument.EditorSchemas`, so the compiler combines old package record labels with the new test field and reports `FS0656`. Production Renderer source would have the same package/source mismatch.
- Decision: Do not annotate around, remove tests, overwrite an existing package version, or introduce ProjectReference. Advance the package chain to Contracts alpha13, Renderer alpha34, Interactive.Client alpha5, Dynamic.Ptcs win53 and Ptcs.Client win71, then align exact test/demo references.
- Estimated fix: zero non-type F# logic lines; approximately 15 XML version/reference edits plus documentation/evidence updates.
- Scope correction: Full WebSharper pack exposed missing JavaScript metadata on three new contract modules and `FS3221` showed the conditional toolbar list discarded existing controls. The actual fix therefore exceeds the initial XML-only estimate: three module attributes plus a behavior-preserving toolbar list composition rewrite are required.
- Version correction: Local Contracts alpha13 was packed before the JavaScript metadata defect surfaced and is superseded without publication. The corrected immutable candidate is alpha14; Renderer alpha34 and the remaining downstream candidates were not successfully packed yet and remain unchanged.
- Browser-call-graph correction: Marking the three direct modules exposed their dependency on `RuntimeValidation` and unsupported `StringComparison`/`IndexOfAny` overloads. The shared validator is now explicitly browser-compiled and uses equivalent WebSharper-supported string operations.
- PTCS adapter correction: Package-first build exposed overlapping transient/browser row labels and two unqualified calls crossing from `TaResearchBrowserWire` to `TaResearchTransientWire`. Added explicit transient return types and qualified the shared value/map codec calls; wire shape and runtime behavior are unchanged.
- Final result: the original record-inference symptom was one layer of a package-first integration gap. The corrected immutable graph is Contracts alpha15, Renderer alpha37, Dynamic.Ptcs win54, Ptcs.Client win73 and Interactive.Client alpha8；focused suites pass 15/15, 22/22, 11/11 and 13/13, and the F# Playwright workflow passes twice consecutively.
- Additional root cause: navigator drag preview used `Doc.EmbedView` to replace the entire SVG on each mousemove. The release event could therefore be delayed or lose the original target. The renderer now preserves the SVG and updates selection/handle attributes dynamically；the verifier uses a bounded wait instead of a fixed 100ms assumption.
