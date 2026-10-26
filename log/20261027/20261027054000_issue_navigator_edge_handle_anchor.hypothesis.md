# Resident navigator edge handle anchor investigation

## Pre-work plan

- 背景：Daedilus 真 18884 將 loaded 4500 的 resident navigator 右 handle 拖至 80-bar 目標後，實際 commit 為 100 bars。evidence：`G:/coldfar_py/coldfar-symbolics/temp/agent.daedilus/resident-width-and-edges/resized-80.json`與 action/timeline；message `msg-fsi-31c7d05ce8904203ba1ddb7f09b27234`。
- 已證實事實：0.2.45與0.2.47 bundle的 `defaultOptions.MinimumVisibleBars=12`，非 100 hard clamp；`resized-80` selection ratio寬度為 100/4500。navigator handle 寬 8px；selection貼右界時 handle x 被 clamp 成 `width-handleWidth`，故 hit box中心比 logical right boundary左4px；現行 delta 卻從 raw pointerdown center 起算。
- 預期不變量：semantic ResizeLeft/Right 的拖曳錨點是rendered selection logical boundary，不是為了可點而clamp後的8px hit-box中心；Move仍保留實際 grab point。左/右邊界、middle、minimum count、resident/global authority、drop-only action均不變。
- Source of truth：Renderer `startNavigatorDrag`的 rendered ratios/semantic drag/pointer delta 與 `RendererModel.previewWindowBounds`；consumer Playwright 只作 E2E 觀測。
- 假設 H1（已有證據）：edge handle的clamped hit geometry中心被當成resize logical edge，4px偏差在4500 bars的窄track被放大為數十bars。
- 最小區分證據：以4500/478px、selection `[3000,4500)`、right hit center=`right-4px`、target count80 固定反例；old raw anchor 不得為80，logical-boundary anchor 必須為80；左edge鏡像同驗。
- 預估最小實作：5–15行 pure anchor helper＋3–8行 wiring＋20–40行左右參數化 regression。
- 步驟：先補 DYN-T-713 RED → 最小 helper/wiring → focused/full owner tests → Release/exact package → Daedilus 原 80-bar pointer path 重驗。
- 範圍：僅 Dynamic Renderer/direct tests/package與本輪文件；不改 consumer source。
- 停止條件：owner右edge 80與左edge鏡像 exact，現有navigator/main plot/overview回歸綠，且Daedilus真 4500 重驗進入約80 bars並可繼續 left-edge overscroll。

## Operations

- 2026-10-27 05:40 +08:00：建立 hypothesis；回覆 receipt/root-cause 候選 `msg-fsi-90295fc09a494815b13c7701dfffaa60`，尚未修改程式碼。
- 2026-10-27 05:47 +08:00：新增DYN-T-713固定4500/478px、right-edge `[3000,4500)` 與left-edge mirror；0.2.47 package如預期FS0039 RED，`navigatorDragAnchorClientX`不存在。
- 2026-10-27 05:49 +08:00：最小修正新增pure `navigatorDragAnchorClientX`：ResizeLeft/Right使用rendered ratios計算logical boundary，Move/其他保留raw grab point；`startNavigatorDrag` delta改以此anchor起算。Renderer/Interactive.Client與direct tests升0.2.48。
- 2026-10-27 05:53 +08:00：DYN-T-713 1/1、Renderer full104/104 PASS；Interactive.Client candidate Release 1 warning/0 errors、fresh exact-package20/20 PASS。pre-commit candidate Renderer SHA-256=`1501B9B3C359D681FB4922707CC9C6915DF06473BC984344861C8897880AC360`、Interactive SHA-256=`3035BDACC9C54AFFB66EB2837F4A4B9FAE622FEEE311B9833A116594F44E58EE`。
- 2026-10-27 05:53 +08:00：Renderer首次pack逢exact task-cache parentless `wsfscservice.exe` PID37396鎖Renderer `websharper.log`而`-532462766`；驗executable/command/parent後僅停此orphan。編譯又先暴露helper的string match未窮與int/float型別錯，修後重建綠。一次長guard首次yield時未保留session id，造成緊接著的status guard衝突；停止重複啟動、等原build釋放capture後確認package已產生，後續長命令改保留/unified poll session。
- 2026-10-27 06:02 +08:00：核對大批文件patch；guard在合併讀取23,234 bytes時正確阻擋，改以欄位投影確認WBS/TEST/DevLog與三份hypothesis。發現resident log單一`U+FFFD`傳輸污染並以exact line修正；13個owned paths strict UTF-8、無NUL，fsproj既有BOM狀態保持，`git diff --check`無finding。
- 2026-10-27 06:07 +08:00：COMM durable drain收到Daedilus較早的0.2.46提醒；以exact `replyToMessageId`回覆`msg-fsi-e1d514ba62d640d4852e48920c45d905`，明確維持三項未關閉、要求0.2.48同輪重驗。敏感掃描讀到9個支援副檔名檔案，High=0、Medium=0；`.fsproj`不在scanner文字副檔名集合，另已逐檔strict UTF-8檢查。
