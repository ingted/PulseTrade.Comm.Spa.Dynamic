# Initial global-backed navigator adjacent regression

## 現象

0.3.10 package provenance正確，但原 formal 的第一個初始4500-bars overscroll action將 default query 的 epoch sentinel當成實際 `StartEventTimeUtc`；host依 durable-window contract拒絕，畫面不移動。

## 已證實根因候選

唯一主候選：0.3.10 live boundary branch從「依 projection authority分流」擴成所有 `UsesResidentAuthority=false` snapshot都走 `NavigatorAdjacentCoverage`。這把兩種不可互換的狀態混在一起：

1. initial global-backed/cross-axis：仍有 `GlobalOverview`，GLOBAL ordinal不可用，但4500 local prepared timeline可用；應 clamped local draft後由實際 event times materialize。
2. returned explicit-query active-local：沒有 global overview，active detail ordinal已重設為 query-relative 0；應使用 0.3.10 的 no-ordinal directional explicit range要求相鄰 materialization。

`tryAdjacentCoverageRange`在第一種狀態讀到 default query epoch bound，因而暴露1970；錯誤訊息只是此 authority混用的末端結果。

## 最小驗證

- 加入 pure routing predicate：只有 `GlobalOverview.IsNone` 才允許 adjacent event-range materialization。
- focused test同時固定：returned projection => true；initial cross-axis global-backed projection => false。
- live source test固定 initial global-backed branch呼叫既有 `commitLocalDraft`，不進 adjacent dispatcher。
- 執行 DYN-T-736 與新 regression case，再跑 Renderer/Interactive完整測試、package-only build與 Daedilus real formal。

## 預估修改

- `RendererModel.fs`：約2個非型別宣告行。
- `Renderer.fs`：約12個非型別宣告行，僅重排 boundary routing。
- `TAResearchRendererTests.fs`：約35行測試。
- 版本與 current docs：依 package/release gate同步。

## Experiment

- 修前：0.3.10 package-backed DYN-T-737在authority predicate兩處FS0039，符合測試先行RED。
- 修後：predicate只檢查`GlobalOverview.IsNone`；live no-global branch使用它，global-backed current-snapshot branch呼叫既有`commitLocalDraft`。沒有改query builder、host validator、wire或settlement。
- 結果：DYN-T-736與DYN-T-737各1/1；Renderer127/127、Interactive20/20、package verifier、fresh exact0.3.11 package-only BrowserDemo皆GREEN。真正consumer formal仍由Daedilus驗收。

