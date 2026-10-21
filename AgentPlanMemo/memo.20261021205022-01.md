# Navigator P0 priority insertion

- Original plan: implement accepted RFC-PTCS-DYNAMIC-0036 whole-series resident navigator contract and owner package.
- Completed: RFC created; additive Contracts/types/codec/action validation implemented; Contracts Expecto 50/50; nothing published.
- Inserted P0: 0.2.2 resident navigator gesture ownership and active-rerender draft corruption reported by Daedalus.
- New order: reproduce owner source -> lock gesture ownership and draft authority -> owner tests/BrowserDemo -> 0.2.3 candidate -> Daedalus formal gate -> resume RFC-0036 Renderer/UI.
- Dirty scope: Contracts/LoadedCoverage/EditorAction/RendererModel/tests/RFC/log/version refs for unfinished RFC-0036; preserve as WIP.
- Owner boundary: Aster owns owner package; Daedalus owns Research host and formal Playwright trace/gate.
- Risk: do not mix the new whole-series gesture with the existing resident navigator P0.
- Resume condition: P0 source and consumer gates pass and 0.2.3 package authority is accepted.

## 2026-10-21 update

- P0 owner source and browser gates now pass: Renderer 68/68 and focused Expecto Playwright 1/1.
- Source commit/push and 0.2.3 package authority are the remaining owner gates; Daedalus formal acceptance follows publication.
- RFC-0036 UI remains paused until that consumer gate completes. Its additive Contracts/pure-model WIP is preserved in the same exact-path source set.
