# TA row label 未進 renderer

- 時間：2026-09-09 09:38 +08:00
- Repo：`C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic`
- Branch：`20260715_030.win.TACanvas_cross.bar.cursor.first.done`
- 現象：SPAA 的 typed `TA_CHART` 已送出 `TA_ROW` label，但 browser toolbar 仍顯示多個 `Candlestick`，row card 顯示 binding/trace 名稱。
- 假設：`TaWorkspaceRenderer` 未讀取 `TaRowSpec.Options["label"]`；card 與 toolbar 各自使用 trace label／`rowKindText`，造成同一 row 有兩套名稱。
- 證據：SPAA `WorkspaceRuntime.createDocument` 將 declaration label 寫入 row options；`Renderer.fs` 的 `renderRow` 與 `ta-row-toggles` 均未讀該 option。
- 影響：`PulseTrade.Comm.Spa.Dynamic.Renderer/Renderer.fs`、Renderer focused tests、下游 Interactive.Client bundle/package。
- 驗證：新增 row display label precedence test；Release build/test後由 M15 Playwright 驗證 toolbar與row header。
- 預估非型別宣告修改：20 行以內。

## Experiment

- 實際修改非型別宣告約20行，未超過預估2倍。
- `TaWorkspaceRenderer.rowExplicitLabel/rowDisplayLabel/rowTitle`統一label precedence；card、toggle、edit/remove tooltip與feedback共用display label。
- Renderer exact-package focused suite `26/26`通過；source-identical staging WebSharper build與BrowserDemo build通過。
- Playwright MCP在`http://127.0.0.1:18882/`同時找到toolbar button與row card title `ES 1K + SMA(20)`，console warning/error為0；截圖`.playwright-mcp/ta-row-authored-label-0.1.2.png`。
- 正式發布Renderer `0.1.3`與Interactive.Client `0.1.3`；0.1.2只作本機驗證中間產物，未發布。
