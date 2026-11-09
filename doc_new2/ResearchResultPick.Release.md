# Research 00013 Renderer 發布紀錄

Renderer 0.3.53 修正衍生 scalar 的自動 Y 軸，以及成交定位／相鄰可視範圍更新的 native ordinal 保留。原生 DMI 仍固定 0～100，明示的 Y 軸限制仍優先。來源及回歸為同一隔離 worktree 的 Renderer、RendererModel 與 TAResearchRendererTests。

Interactive.Client 0.3.50 → 0.3.51 僅同步必要的 Renderer 0.3.53 相依及重建 bundle；Client 程式碼未修改。Contracts 維持 0.1.44、FSharp.Core 維持使用者指定的 10.1.402。

2026-11-08 發布前 exact package consumer 驗證：Renderer 343/343、Client 59/59 通過。命令與結果在 `log/20261108/release-renderer-0353-tests.log`、`release-client-0351-tests.log`；同輪測試與升版 evidence 已複製納管於 Symbolics repo 的 `log/20261108/dynamic-release-00013/`。版本與相依由既有 `bumpVersion.fsx --apply` 管理；尚未存在的新 Client 套件先以 producer-only scope 建置本機產物，再同步直接 consumer，沒有以舊版本覆寫公開套件。

Research 的 native UI 驗收範圍與排除項見 Symbolics repo 的 `doc_new2/ResearchResultPick.Acceptance.md`。完整歷史回測仍依人類決定待驗；這次 package tests 不代替該驗收。正式 NuGet 發布結果與 source checkpoint 待驗證後追加。

## 發布完成（2026-11-08）

Source checkpoint `0b4c1b31fc9aedfaaaab422e1bd498fdeb0660b5` 已推送。Renderer 0.3.53 與 Interactive.Client 0.3.51 已由 opt-in Release Pack hook 發布並同步 SDK library-packs，來源與本機 feed 的 SHA-256 相同。NuGet 回下載的每個原始 entry 均相符，新增的 repository signature 也已通過 `dotnet nuget verify --all`；因簽章而不同的 archive hash 分別留存於 Symbolics repo `log/20261108/00013-nuget-download-verification.json`。

兩個 package 內的 DLL 與 18883 的 Research 0.1.85 實際載入產物相同。該服務已重新驗證真 CSV 的14列與48根可視範圍；18884正式0.1.61仍隔離保留。

## 2026-11-10 Interactive.Client 0.3.53 source checkpoint（PARTIAL）

0.3.51→0.3.53；0.3.52 為未發布的中間候選。FramePump 將非視覺資料分批工作交給 MessageChannel，維持 8ms budget、取消/generation fence、驗證與 atomic commit。已解析的 JSON snapshot 直接交給既有 WebSharper Json.Decode，不再逐批 stringify/parse。沒有修改 Renderer 或 Contracts，FSharp.Core 保持10.1.402。新版 exact local package consumer 59/59，Research完整WebSharper建置0 errors。

18883 的 Research 0.1.86 候選042637已實測原30K GLOBAL最左端→2020、1500與48根保留及延後5秒無回跳；服務與served bundle hash一致。原30K全流程仍8.565秒（decode pump2.199秒），不宣告GLOBAL延遲完全解決。本輪00005策略完成不搶選擇及00013原生930K alias Tagging真資料證據，集中於Symbolics的doc_new2/ResearchResultPick.Acceptance.md與log/20261110。完整歷史回測依使用者決議待驗。發布結果後續追加。

2026-11-10 發布完成：Interactive.Client 0.3.53 source f617672 已push；opt-in Release Pack完成NuGet push與SDK library-packs同hash複製。公開回下載所有原始entry hash一致、repository signature PASS。原00002 GLOBAL仍約8.6秒；此發布不代表全backlog完成。
