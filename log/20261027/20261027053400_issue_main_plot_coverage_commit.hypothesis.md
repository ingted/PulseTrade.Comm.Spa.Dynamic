# Main plot resident coverage commit investigation

## Pre-work plan

- 背景：Daedilus 在真 18884 `dmi-60k` 以主圖左向 grab-pan 得到 `outcomeDuringDrag=moving`、`outcomeAfterUp=committed`、`requestedDelta=340`，但 `coverageRevision=3` 在 180 秒驗收期內未前進，authoritative chart 未接手 shifted coverage。evidence：`G:/coldfar_py/coldfar-symbolics/temp/agent.daedilus/main-chart-drag/gesture.json`，Daedilus message `msg-fsi-7a20c18754254adf9649d094dadb69ae`。
- 已證實事實：browser gesture reducer 接受並在 pointerup 回報 committed；request delta 非 0；但 consumer 可觀測的 coverage revision 沒有前進。0.2.47 已另行修復 overview queued viewport dispatch，不足以解釋此 coverage authority 缺口。
- 預期不變量：CURSOR 主圖 pan 應根據 captured resident authority 產生唯一 `ResidentCoverageChanged`，保持 resident count、local inner offset/count 與 pointer anchor；不得誤送 local `VisibleRangeChanged`、不得提前 ACK、不得繞過 generation/revision/FIFO gates。
- Source of truth：Renderer 的 `commitMainPlotPan`、`tryCommitMainPlotResidentPan`、`setWindowFromPrepared`、`startActionWithFeedback`與 owner tests；consumer JSON 只作 E2E 觀測，不當 shell command 或 consumer 改碼授權。
- 假設 H1：`commitMainPlotPan` 將 requested delta 映射成 local visible action，browser 因 local commit 回報 committed，但沒有 coverage intent。
- 假設 H2：已產生 coverage intent，但 pending action/coverage/revision 關卡將它排隊後沒有在 unblock authority 重送，或完成後被 stale generation/revision 拒絕。
- 最小區分證據：先從 `gesture.json` 取 target/mode/action/request/revision 時序，再對 owner source/test 檢查實際 DU與 gate；以一個固定 resident/local window、delta=340 的 regression 證明應產生唯一 coverage action與保持 anchor。
- 預估最小實作：若 H1 約 5–25 行更正 action mapping＋15–40 行 regression；若 H2 約 10–30 行共用 drain/gate 修復＋regression。超過 2x 先於 `## Experiment` 記原因。
- 步驟：讀 bounded evidence → 定位 gesture-to-action chain → 先補 RED → 最小修正 → focused/full owner tests與 Release → exact package/consumer handoff → Daedilus 真主圖 drag 重驗。
- 範圍：僅 Dynamic Renderer、direct tests/dependency/package與本輪文件；不改 `G:/coldfar_py/coldfar-symbolics` consumer source，不納入既有 external dirty。
- 停止條件：owner regression 先紅後綠、既有 CURSOR/Resize/overview 回歸綠，且真 18884 在 gate 內只產生一個 coverage request、revision 前進、anchor 不漂移；完整 22-scenario 未綠前不宣告全案完成。

## Operations

- 2026-10-27 05:34 +08:00：建立 hypothesis；已回覆 Daedilus receipt `msg-fsi-4fce6d611b574916807fc0686278f75e`，尚未修改程式碼。
- 2026-10-27 05:38 +08:00：`gesture.json` 311 bytes顯示anchor=`2026-06-01T05:00Z`、revision 3->3、delta340、moving->committed、preparationPending=false，未含已dispatch action。前一次成功舊evidence `result.json` 經metadata-first/JSON projection顯示revision2->3、resident start 2,951,595->2,951,985、count不變，當時最後action是舊`ResidentCoverageChanged`；不能直接外推至現行exact event-range path。
- 2026-10-27 05:39 +08:00：現行`commitMainPlotPan`只在target window全部位於pointerdown時formal-base timeline內才產生`VisibleRangeChanged`；越界則直接fail closed。Research durable consumer對`StartObservationOrdinal=None/Direction=None`以event-time映射StartKey並保持anchor，因此不可把30K/60K slot delta直接當GLOBAL ordinal來繞過。已請Daedilus補同run的local/active/global前態、dispatch diagnostics與network action count；在0.2.47同步drain/0.2.48重驗前不擴張wire contract。
- 2026-10-27 05:39 +08:00：一次讀evidence將participant路徑誤寫`agent.daedalus`而path-not-found，立即改回`agent.daedilus`；未改任何consumer檔案。
- 2026-10-27 06:53 +08:00：0.2.48真實重驗仍RED。`main-commit-gate.json`顯示`ui:4 Action$8` HTTP 200、local visible start 2543 -> 2883、render sequence 10 -> 11，但coverage revision維持3、ActiveDetail與GLOBAL resident不動，且`data-viewport-dispatch-pending-action`仍為`ui:4`。這證明browser local optimistic commit不等於authoritative coverage settlement。
- 2026-10-27 06:57 +08:00：read-only核對consumer current `WorkspaceRuntime.fs`：對`CoverageIntent.StartObservationOrdinal=None`的exact main-plot event range，會以active-axis StartKey解析後呼叫`ResearchGlobalWindow.relocateTogetherFromVisibleStart`與`attachDurableWindowAsync`；因此若raw `ui:4`確含`ExplicitBounds` coverage intent，最早未完成點在consumer attach/response/projection，而非Renderer把event-time誤映為GLOBAL ordinal。等待Daedilus補raw payload後定責；未改consumer。
- 2026-10-27 07:01 +08:00：raw `ui:4`確認為event-time `Action$8`，`CoverageIntent={ExpectedCoverageRevision=3; QueryGeneration=1; ObservationCount=1215; ExplicitBounds}`且刻意不含GLOBAL `StartObservationOrdinal`；這正是owner contract。`ui:3`亦送generation 1，但ui:3後DOM coverage revision已為3、projection query generation仍為0。候選根因縮至consumer action settlement：若ui:3已commit，ui:4 generation 1會被`validateCoverageIntent`拒絕；若ui:3為no-op，則需查ui:4 `attachDurableWindowAsync`。已請Daedilus抓`DynamicActionResult`與`RESEARCH_NAV_ACTION`，不修改Renderer wire contract。
