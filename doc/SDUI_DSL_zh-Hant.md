# FSkynet SDUI DSL (JSON 語意) 說明文件

本文檔詳細說明了 `PulseTrade.Comm.Spa` 中 Server-Driven UI (SDUI) 所使用的 JSON DSL 結構、元件型別，以及前後端的通訊與互動機制。

## 1. 核心概念與結構

`fskynet-sdui` 是一種宣告式的 JSON DSL，其結構借鏡了 Python Dash 的理念。後端 Actor 在回傳結果時，會帶有一組 Payload，其基本結構如下：

```json
{
  "schema": "fskynet-sdui",
  "data": {
    "orderData": [ ... ],
    "historyData": [ ... ]
  },
  "sdui": "[ ... JSON String of Components ... ]"
}
```

- **`schema`**：必須固定為 `"fskynet-sdui"`，前端依據此標記決定將 Payload 交由 SDUI 渲染引擎處理。
- **`data`**：存放由後端 fCell 轉換而來的原始資料，以 key-value 形式儲存，供 SDUI 元件綁定 (透過 `dataRef`)。
- **`sdui`**：一個包含元件陣列的 JSON 字串 (或直接為 JSON 陣列)，描述了整個動態畫布 (Canvas) 的佈局與互動。

---

## 2. 動作與互動定義 (Actions)

SDUI 將業務邏輯保留在後端，前端的互動行為透過宣告式的 `Action` 定義。目前支援的 Action 分為兩種：

### A. Remote Action (遠端動作)
將使用者在前端互動的狀態，送回給後端的 Actor 處理。
- **`SendCommand`**：
  - `action`: `"SendCommand"`
  - `command`: 要執行的指令名稱字串。
  - `includeStateOf`: 字串陣列，指定要一起打包送回後端的元件狀態 (如 `"grid1.selectedRow"`, `"input1.value"`)。

### B. Local Action (本地動作)
前端純粹的 UI 狀態改變，不消耗 WebSocket 頻寬與後端運算資源。
- **`LocalSort`**：針對表格資料的本地排序。
  - `action`: `"LocalSort"`
  - `targetGridId`: 目標 DataGrid 的 ID。
  - `column`: 要排序的欄位名稱。
  - `direction`: 排序方向 (e.g. `"Asc"`, `"Desc"`)。
- **`LocalAggregate`**：針對表格資料的本地聚合運算。
  - `action`: `"LocalAggregate"`
  - `targetGridId`: 目標 DataGrid 的 ID。
  - `column`: 作為聚合依據的欄位名稱。
  - `functionType`: 聚合函數 (e.g. `"Sum"`, `"Average"`)。
- **`OpenCanvas`**：開啟另一個動態畫布。
  - `action`: `"OpenCanvas"`
  - `moduleId`: 目標模組 ID。
  - `targetId`: 目標頁面或資源 ID。

---

## 3. 核心與進階元件清單 (Components)

每一個 SDUI 元件在 JSON 中都是一個包含 `"type"` 屬性的物件。

### 核心元件 (Core Components)

1. **Heading (標題)**
   - `type`: `"Heading"`
   - `text`: 標題顯示文字。
   - 範例：`{"type": "Heading", "text": "即時交易深度"}`

2. **Button (按鈕)**
   - `type`: `"Button"`
   - `id`: 元件唯一識別碼。
   - `text`: 按鈕文字。
   - `onClick`: 按下按鈕時執行的 `Action`。
   - 範例：`{"type": "Button", "id": "btn1", "text": "市價買入", "onClick": {"action": "SendCommand", "command": "buy", "includeStateOf": []}}`

3. **Dropdown (下拉選單)**
   - `type`: `"Dropdown"`
   - `id`: 元件唯一識別碼。
   - `options`: 選項字串陣列。
   - `onSelect`: 選擇時觸發的 `Action`。

4. **DataGrid (資料網格)**
   - `type`: `"DataGrid"`
   - `id`: 元件唯一識別碼。
   - `dataRef`: 對應到 `data` 區塊中的 key 名稱 (綁定資料源)。
   - `features`: `{ "AllowSorting": true, "AllowAggregation": true, "Pagination": false }` 等網格特性。

5. **RealtimeChart (即時圖表)**
   - `type`: `"RealtimeChart"`
   - `id`: 元件唯一識別碼。
   - `dataRef`: 綁定的資料源 key。
   - `indicators`: 技術指標清單 (e.g. `["MA", "MACD"]`)。

### 進階元件 (Advanced Components - 基於 tui-chart 概念擴展)

6. **AppLoader (載入器)**
   - `type`: `"AppLoader"`
   - `id`: 元件唯一識別碼。
   - `text`: 載入中提示文字。

7. **AutoComplete (自動完成輸入框)**
   - `type`: `"AutoComplete"`
   - `id`: 元件唯一識別碼。
   - `dataRef`: 提供自動完成建議的資料源 key。
   - `onInput`: 輸入時觸發的 `Action` (可選)。

8. **ColorPicker (顏色選擇器)**
   - `type`: `"ColorPicker"`
   - `id`: 元件唯一識別碼。
   - `defaultColor`: 預設顏色 hex 代碼。
   - `onChange`: 選擇變更時觸發的 `Action`。

9. **ContextMenu (右鍵選單)**
   - `type`: `"ContextMenu"`
   - `id`: 元件唯一識別碼。
   - `targetId`: 綁定右鍵選單的目標元件 ID。
   - `menuItems`: 選單項目陣列，每個項目包含 `text` 與對應的 `Action`。

10. **DatePicker (日期選擇器)**
    - `type`: `"DatePicker"`
    - `id`: 元件唯一識別碼。
    - `format`: 日期格式 (e.g. `"yyyy-MM-dd"`)。
    - `onChange`: 選擇變更時觸發的 `Action`。

11. **Pagination (分頁控制)**
    - `type`: `"Pagination"`
    - `id`: 元件唯一識別碼。
    - `targetGridId`: 受控的 DataGrid ID。
    - `pageSize`: 每頁顯示筆數。

12. **Rolling (滾動播報元件)**
    - `type`: `"Rolling"`
    - `id`: 元件唯一識別碼。
    - `dataRef`: 綁定的資料源 key (通常是一維字串陣列)。
    - `direction`: 滾動方向 (`"up"`, `"down"`, `"left"`, `"right"`)。

13. **SelectBox (進階選擇框)**
    - `type`: `"SelectBox"`
    - `id`: 元件唯一識別碼。
    - `options`: 選項陣列 (支援 key-value pair)。
    - `multiple`: 是否支援多選 (boolean)。
    - `onChange`: 變更時觸發的 `Action`。

14. **TimePicker (時間選擇器)**
    - `type`: `"TimePicker"`
    - `id`: 元件唯一識別碼。
    - `format`: 時間格式 (e.g. `"HH:mm"`)。
    - `onChange`: 變更時觸發的 `Action`。

15. **Tree (樹狀視圖)**
    - `type`: `"Tree"`
    - `id`: 元件唯一識別碼。
    - `dataRef`: 樹狀資料源 key。
    - `onNodeClick`: 點擊節點時觸發的 `Action`。

---

## 4. WebSharper 實作守則

- **純 Native WebSharper**：上述所有元件的渲染與生命週期管理，均使用 F# 搭配 WebSharper 實作，不依賴任何第三方 JavaScript 函式庫或 raw JS snippet。
- **型別安全解析**：SDUI JSON 在 WebSharper 端將使用嚴格定義的 Record/Union 型別進行反序列化 (Deserialization)。
- **技術分析考量**：在實作技術分析圖表 (`RealtimeChart`) 等與資料運算相關的功能時，必須參考既有的 `PulseTrade.MarketData.Analytics.fs` 架構以維持體系一致性。
