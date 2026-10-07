# RF20 Composer browser integration

Observed 202610071912 +08. Core b3713ef public0.2.60, Dynamic0657adc public0.1.39, formal remains59.

## Hypotheses
1. Core CSS .append-work fixed grid auto rows + overflow:hidden allows user-sized Form300 beyond the remaining narrow viewport. At820x900 Send top962.8. Experiment on test browser: flexible work column, shrink composer and bounded sidebar, scroll fallback. Oracle Send bottom<=viewport, key/value editable, native resize still works. Estimate CSS12lines.
2. Dynamic AppendInputContextDto omits existing Core valueText/setValue, and resolver always uses target default canonicalArgString. User edits render local preview but disappear when mode reconstruction occurs. Reuse existing three-key resolve-target read request with current draft, update existing setValue callback on real edits; reject invalid draft without replacing it. No new protocol/backend/parser or target key mutation. Estimate30functional lines. Validate pure key/draft policy plus real browser bidirectional mode change, quote/Unicode/repeatedfields, invalid draft preserves text, actual echo.

## Scope
Core CSS/Composer tests; Dynamic src/Client/ArguFormRenderer.fs and relevant tests. Do not alter Baster src/PulseTrade.Comm.Spa.Dynamic.Renderer/Renderer.fs WIP. Package61 and exact consumers, sameexistingoutput reuse. Do not mark current RF20 browser full PASS; original failures retained.
