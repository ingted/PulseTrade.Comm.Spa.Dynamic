# RFC-PTCS-DYNAMIC-0012 TA editor poll/reset stability

- Status：Accepted
- Date：2026-07-15
- Related：`doc/RFC/RFC-PTCS-DYNAMIC-0007.realtime-ta-canvas-runtime.md`、`doc/RFC/RFC-PTCS-DYNAMIC-0010.ta-overview-row-editor-reset-copy.md`、`doc/TAResearch/WBS.DYN-TA-016.md`

## 背景

TA client目前把整個workspace直接組合在`RuntimeState.View`下。週期poll只要改變`Poll`或`DataRevision`，WebSharper就會替換editor DOM，導致已展開的native select失焦/收合。另一方面，client不論Document是否宣告live capability都固定排程5秒poll。既有Reset驗證只刪除一列，沒有覆蓋連續刪除DMI與MACD後完整還原。

## 目標

1. Document shell只在Document identity/revision改變時重建；poll/data變動只更新status與chart subtree。
2. 只有`AllowedActions`含`poll-delta`的Document才排週期poll；static Document保持zero-poll。
3. Add Row select跨至少兩個live poll仍保持focus與draft。
4. Reset Canvas一次恢復mount-time原始ordered rows/query，不是undo最後一步。
5. remote structural action in-flight時，Reset/Add/Remove/Apply均不可再送出競爭命令。

## 非目標

- 不改5秒live cadence。
- 不把editor draft寫入Document、IndexedDB或server。
- 不以停用live update掩蓋DOM重建問題。

## 情境

1. Static SDUI Document只呈現既有資料，不宣告`poll-delta`，open/reactivate後不得產生timer action。
2. Live TA Document每五秒更新data/status，但使用者正在操作的Add Row、query draft與native select DOM保持原狀。
3. 使用者連續移除多列、加入暫存列後按Reset，一次回到mount-time原始DSL。

## 方案取捨

- 採document-shell cache與nested runtime views，保留live chart；不採停用poll或把draft送回server。
- 採Document capability控制poll，不新增renderer-specific provider猜測。
- chart cache納入transport sequence，接受reconnect same-revision full frame；代價是每個新frame sequence會重新評估bounded chart subtree。

## 決策

1. lifecycle state新增`PollEnabled`，由accepted Document的`AllowedActions`推導；`StateAccepted`同時帶入data revision與capability。
2. renderer使用document-shell cache；status/poll/data/chart以nested reactive view更新。editor Vars與DOM identity不依賴data/poll revision。
3. 所有remote buttons使用live `Poll`衍生的disabled view，callback亦重新讀取目前state後才submit。
4. Reset仍由Host companion回authoritative initial Document；Dynamic收到新Document後清local hidden/cursor/window draft。

## 影響範圍

- `PulseTrade.Comm.Spa.Dynamic.Renderer`：document shell、remote action disabled state與chart cache key。
- `PulseTrade.Comm.Spa.Dynamic.Ptcs.Client`：poll capability與lifecycle state。
- package tests及root formal F# Playwright verifier；不改Contracts wire schema與PTCS core protocol。

## 驗收

1. static fixture在兩個poll interval內沒有`poll-delta` action。
2. live fixture開啟row-kind select後跨兩個poll，focus仍在同一control，draft不變，chart data可更新。
3. 連續Remove DMI與兩列MACD後Reset，ordered rows精確恢復price、dmi、macd-short、macd-long與17 traces。
4. F# Playwright無page/console error；正式82 exact package部署通過。

## 關聯文件

- `doc/TAResearch/REQ.md`
- `doc/TAResearch/SA.md`
- `doc/TAResearch/SD.md`
- `doc/TAResearch/Test.md`
- `doc/TAResearch/WBS.DYN-TA-016.md`
- `G:\PulseTrade.fs\Libs\PulseTrade.Comm\doc\RFC\RFC-PTC-PTCSH-0006.TAEditorPollResetStability.md`
