# Research 00013 Renderer 發布紀錄

Renderer 0.3.53 修正衍生 scalar 的自動 Y 軸，以及成交定位／相鄰可視範圍更新的 native ordinal 保留。原生 DMI 仍固定 0～100，明示的 Y 軸限制仍優先。來源及回歸為同一隔離 worktree 的 Renderer、RendererModel 與 TAResearchRendererTests。

Interactive.Client 0.3.50 → 0.3.51 僅同步必要的 Renderer 0.3.53 相依及重建 bundle；Client 程式碼未修改。Contracts 維持 0.1.44、FSharp.Core 維持使用者指定的 10.1.402。

2026-11-08 發布前 exact package consumer 驗證：Renderer 343/343、Client 59/59 通過。命令與結果在 `log/20261108/release-renderer-0353-tests.log`、`release-client-0351-tests.log`；同輪測試與升版 evidence 已複製納管於 Symbolics repo 的 `log/20261108/dynamic-release-00013/`。版本與相依由既有 `bumpVersion.fsx --apply` 管理；尚未存在的新 Client 套件先以 producer-only scope 建置本機產物，再同步直接 consumer，沒有以舊版本覆寫公開套件。

Research 的 native UI 驗收範圍與排除項見 Symbolics repo 的 `doc_new2/ResearchResultPick.Acceptance.md`。完整歷史回測仍依人類決定待驗；這次 package tests 不代替該驗收。正式 NuGet 發布結果與 source checkpoint 待驗證後追加。
