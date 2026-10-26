# Resident navigator drop dispatch latency investigation

## Pre-work plan

- 背景：Daedilus真18884 formal 4500 overview body drag在mouseup後顯示`Preview 1499-2998 · release to render`，約6.34秒後才出request4；5秒內workspace action未dispatch。
- 已證實事實：evidence `G:/coldfar_py/coldfar-symbolics/temp/agent.daedalus/formal-overview-edge-drag/middle-frame-trace.json`共365 frames，其中317 frames保持pending preview；pointer drag最後位置約1793048971272，`commit-local`於1793048971303，request4於1793048977613。request3於1793048969931、response3於1793048973390，故drop發生時另有consumer request在途，但其response後仍延遲約4.22秒。
- 預期不變量：pointerup應同步確定final draft並在同一drop action path prompt dispatch唯一typed viewport action；既有FIFO、generation/revision gate、optimistic preview與pending response不得丟失或重複，且local resident move不應等待低頻reconciliation/poll。
- Source of truth：Renderer navigator pointerup/drag reducer與action dispatch seam、owner tests；consumer timeline只作E2E觀測，不作shell command或consumer修改授權。
- 假設 H1：body drop只更新browser-local committed window，依賴低頻schedule/reconciliation稍後觀察差異才dispatch，因此落在約5秒週期。
- 假設 H2：drop已產生action但被現有in-flight action gate延後；response到達後未立即drain pending action，而等下一timer/poll。
- 最小區分證據：定位`commit-local`／pointerup action path、pending action ownership與timer interval；以可控callback/clock固定drop時立即dispatch一次，以及in-flight settlement後立即drain的contract。
- 預估最小實作：若H1成立約5–20行把final drop送既有typed action seam；若H2成立約10–30行settlement drain；另加10–35行參數化 regression。超過2倍先在`## Experiment`記原因。
- 步驟：定位pointerup→commit→dispatch call chain → 先補失敗測試 → 最小修正 → focused/full owner tests與Release build → bump exact packages/direct tests → commit/push後final package → Daedilus真drag與完整22-scenario重驗。
- 範圍：僅Dynamic Renderer、direct package/test與文件；不改consumer repo、不以縮短poll或重啟作修正。
- 停止條件：drop action在owner可觀測同一callback turn產生且唯一，既有resize/move/overscroll tests綠，真18884於5秒內不再停Preview；完整22-scenario未綠前不宣告全案完成。

## Operations

- 2026-10-27 05:11 +08:00：建立hypothesis；尚未修改程式碼。
- 2026-10-27 05:13 +08:00：定位到drop `commit-local`會由`setWindowFromPrepared`建立typed `VisibleRangeChanged`，但前一action在途時只寫`queuedViewportIntent`；action settlement清除`PendingActionId`後只以`requestAnimationFrame`安排flush。consumer response3後主執行緒仍忙於處理/paint約4.22秒，RAF queue遂把已ready的drop dispatch拖到request4。最早偏離點是settlement callback沒有同步嘗試drain，非poll interval或drop未產生action。
- 2026-10-27 05:14 +08:00：新增DYN-T-712以可控callbacks要求順序`settled -> flush-now -> schedule-fallback`；舊0.2.46 package build如預期RED，`flushViewportIntentAfterActionSettled` FS0039。
- 2026-10-27 05:16 +08:00：進一步核對consumer request3是既有poll/host request而非必然為Renderer action；因此root contract收斂為所有queue unblock authority（action settlement與`Poll/PendingActionId` View）都須先同步drain，再保留RAF作projection尚未ready的fallback。修正共用`flushViewportIntentNowAndAfterFrame`，不縮poll、不移除FIFO/generation/revision gates。
- 2026-10-27 05:18 +08:00：Renderer/Interactive.Client與direct tests同步0.2.47。provisional Renderer Release 39 warnings/0 errors，DYN-T-712 1/1、fresh exact-package full103/103 PASS；candidate SHA-256=`E809754124E02B7B8CD55A0A460EA9246E2F140AC93D83F764B21100329AB0B6`。
- 2026-10-27 05:20 +08:00：Interactive.Client以更新後Renderer[0.2.47]重建，Release 1 warning/0 errors；provisional SHA-256=`5100C5FC21781E528B3E617E1BC4BBBE37F2F07551B3364147F2E872F8CB117F`，fresh exact-package Interactive tests20/20 PASS。兩包均為pre-commit candidate。
- 2026-10-27 05:20 +08:00：同版candidate重建期間兩次wsfsc `-532462766`皆由exact task cache的parentless compiler services持有各自project `websharper.log`：Renderer PID34548；Interactive PIDs39500/19792/25484。逐一驗executable位於本repo `temp/agent.asteer`、command line指向exact log且parent不存在後才停止；未改ACL、未清cache、未碰Daedilus process，重跑皆PASS。
- 2026-10-27 05:26 +08:00：WBS/TEST補齊0.2.47與DYN-T-711/712的owner/consumer gates。changed-path敏感掃描High=0/Medium=0；11個owned paths均strict UTF-8、NUL/U+FFFD=0且BOM配置不變；`git diff --check` PASS。`check.fsx` PASS=23/WARN=3/FAIL=0，WARN僅為舊main log缺顯式假設、external dirty與既有RFC traceability缺口。
- 2026-10-27 05:26 +08:00：兩次bounded inspection命令因Windows `rg` wildcard exit2及PowerShell foreach直接串pipe parser error失敗；改用`rg -g`與先收集`$rows`後重跑。兩次都只讀、未改source/package。
- 2026-10-27 05:31 +08:00：source/docs/tests commit `75ff3380d96ab6e5bdfed7a6d4d95fca66b8b9b2` 已push。final immutable Renderer 0.2.47 SHA-256=`F95716A95B40ED8CA4AD4E27725C1C32C09A0E9573C0294CF7580F159329334C`、Interactive.Client 0.2.47 SHA-256=`CFDF4FDE5A6C46A66F9B759A4B6AEC985C6E81CB2EDF8D3EFEF0C4268750F611`；兩包RepositoryCommit exact、Interactive exact Renderer[0.2.47]，SDK10.0.402 library-packs/task feed hash parity。fresh final exact-package Renderer103/103、Interactive20/20、package verifier PASS；全新package-only BrowserDemo exact0.2.47 Release僅WS9002 1 warning/0 errors，bundle7檔/1,454,056 bytes。
- 2026-10-27 05:32 +08:00：fresh restore首先因literal NuGet URL被CLI誤當project-relative path、後又因僅local sources缺HtmlAgilityPack失敗；改用現有user NuGet.Config＋SDK library-packs 8.3 absolute additional source後通過，未新增／改寫 repo `nuget.config`。
