# FSkynet: PulseTrade.Comm.Spa Dynamic UI Framework (v3)

## 1. 核心精神：Dash-Like Canvas SDUI

`PulseTrade.Comm.Spa.Dynamic` (代號 **FSkynet**) 將 **Server-Driven UI (SDUI)** 的能力聚焦於單一 **Canvas (畫布)** 容器，並深度借鏡 Python Dash 框架的理念，將 UI 拆分為 **Layout (宣告式佈局)** 與 **Actions (回調與事件)**。

### A. 對話框摘要與 Canvas 展開
- **對話框摘要 (Summary)**：在核心聊天流中，模組可插入輕量卡片。
- **動態畫布 (Canvas)**：這是一個由 SPA 端負責建構與清理的生命週期容器。
    1. 當使用者點擊 `argu-actor` Tab 或摘要時，SPA 向後端發送請求。
    2. 後端 Actor 返回兩樣東西：**fCell 原始資料** 以及 **FSkynet SDUI 佈局定義**。
    3. SPA 負責將兩者綁定，渲染為高互動性的 WebSharper UI，並在使用者關閉畫布時進行記憶體清理。

---

## 2. 語義模型 (FSkynet DSL)

我們使用 F# DSL 定義 UI 結構，這相當於 Dash 的 `app.layout` 與 `app.callback` 概念。

### A. UI 結構與動作定義 (Layout & Actions)
```fsharp
// 定義動作 (類似 Dash Callback)
type SduiAction = 
    | SendCommand of command: string * includeStateOf: string list // 將畫布上特定元件的狀態回傳給 Actor
    | LocalSort of targetGridId: string * column: string * direction: string // 前端本地處理 fCell 排序
    | LocalAggregate of targetGridId: string * column: string * functionType: string // 前端本地處理 fCell 聚合 (Sum/Average)
    | OpenCanvas of moduleId: string * targetId: string

// 定義元件 (Components)
type CanvasComponent =
    | Heading of text: string
    | Button of id: string * text: string * onClick: SduiAction
    | Dropdown of id: string * options: string list * onSelect: SduiAction
    | DataGrid of id: string * dataRef: string * features: GridFeatures
    | RealtimeChart of id: string * dataRef: string * indicators: string list

and GridFeatures = {
    AllowSorting: bool
    AllowAggregation: bool
    Pagination: bool
}
```

---

## 3. MVC 實作模式：基於 fCell 的狀態同步與事件分發

### Model (fCell State)
後端 Actor 回傳的 `fCell2<string>` 是 Canvas 的唯一資料源 (Source of Truth)。

### View (WebSharper Canvas Builder)
SPA 前端負責接收 SDUI，並將其遞迴渲染為 WebSharper `Doc`。例如：當看到 `DataGrid` 且 `AllowSorting = true` 時，前端會自動加上可點擊的表頭。

### Controller (Action Dispatcher)
借鏡 Dash 的事件機制，SPA 在前端實作一個 **Action Dispatcher**：
- **Local Action** (如 `LocalSort` / `LocalAggregate`)：當使用者點擊表頭排序時，Dispatcher 不發送 WebSocket 請求，而是直接在瀏覽器記憶體中重排 fCell 資料並更新 DOM，確保極致效能。
- **Remote Action** (如 `SendCommand`)：當使用者點擊「送出訂單」按鈕時，Dispatcher 會蒐集 `includeStateOf` 指定的輸入框數值，透過 WebSocket 打回給後端的 `argu-actor`。

---

## 4. 規格範例 (Dash-Style Examples)

### 範例一：即時報價與下單面板 (e2eQuotation Style)
展示如何整合報價表格與動作按鈕。

```fsharp
// 後端建構的 SDUI 定義
let tradingCanvas = [
    Heading("BTC/USDT 即時交易深度與下單")
    
    // 工具列：前端觸發的過濾與排序 (不消耗後端資源)
    Dropdown(id="sort-drp", options=["價格遞增"; "價格遞減"], onSelect=LocalSort("order-book-grid", "Price", "Asc"))
    
    // 資料網格：綁定名為 "orderBookData" 的 fCell 節點
    DataGrid(
        id = "order-book-grid", 
        dataRef = "orderBookData", 
        features = { AllowSorting = true; AllowAggregation = true; Pagination = false }
    )
    
    // 動作按鈕：點擊時，蒐集表單資料送回後端 Actor
    Button(
        id = "btn-buy", 
        text = "市價買入", 
        onClick = SendCommand("place_order", ["order-book-grid.selectedRow"])
    )
]

// 當 argu-actor 被觸發時，回傳資料與佈局
let reply = {
    DataValue = fetchOrderBookAsFCell "BTCUSDT"  // fCell 資料
    SduiDefinition = serializeSdui tradingCanvas // SDUI 宣告
}
```

### 範例二：技術分析匯總 (MarketData.Analytics Style)
展示透過 `LocalAggregate` 在前端直接對大量 fCell 歷史資料進行分群或聚合計算。

```fsharp
let analyticsCanvas = [
    Heading("策略回測績效報表")
    
    // 本地條件聚合：點擊後，前端自動將 fCell 資料依據 "策略類型" 進行 "加總(Sum)" 並重新渲染 Grid
    Button(
        id = "btn-agg-strategy", 
        text = "按策略分群加總", 
        onClick = LocalAggregate("performance-grid", "StrategyName", "Sum")
    )
    
    DataGrid(id = "performance-grid", dataRef = "historyData", features = { ... })
]
```
