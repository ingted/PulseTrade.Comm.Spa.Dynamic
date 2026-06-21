# RFC: Support for Dynamic Extensions in PulseTrade.Comm.Spa (v3)

**Status**: Proposed (Refined for Dash-like SDUI & Canvas Lifecycle)  
**Author**: Gemini CLI  
**Target**: `PulseTrade.Comm.Spa` Development Team  

---

## 1. 背景與動機 (Background & Motivation)

為了支援 **PulseTrade.Comm.Spa.Dynamic (FSkynet)**，我們需要 `PulseTrade.Comm.Spa` 提供更精確的 UI 擴展點。

目前的設計將 SDUI (Server-Driven UI) 的作用範圍限縮於一個 **「Canvas (畫布)」** 概念。
- **對話框摘要 (Summary Card)**：在 Append-only 的聊天 Session 中，擴展模組會插入一個輕量級的「摘要卡片」。
- **動態畫布 (Canvas)**：這是一個由 SPA 負責建構與清理的獨立區域。當使用者點擊 `argu-actor` 類型的 Tab 或對話框內的摘要時，SPA 會向後端請求資料，後端將同時返回 **fCell 資料** 以及用來呈現該資料的 **FSkynet SDUI 定義**。SPA 隨後利用這些定義建構出具備高度互動性（如排序、聚合、命令觸發）的畫布。

借鏡 Python Dash 框架的理念，Canvas 內的組件（Grid, Chart, Button）將由後端宣告，而其觸發的事件將由 SPA 的前端分發器 (Action Dispatcher) 處理。

---

## 2. 建議調整項目 (Proposed Changes)

### A. SPA 前端 Canvas 生命週期管理 (WebSharper 層級)
SPA 專案需要實作一個專門管理 Canvas 狀態的管理器，確保記憶體不外洩。

#### 建議實作 (`Client.fs` 示意):
```fsharp
type CanvasManager() =
    // 當使用者點擊展開時呼叫
    member this.MountCanvas(containerId: string, sdui: SduiDefinition, data: fCell2<string>) =
        // 1. 呼叫 WebSharper Renderer 根據 SDUI 與 fCell 建立 Doc
        // 2. 綁定 Action Dispatcher
        // 3. 將 Doc 掛載至 DOM
        
    // 當使用者關閉 Canvas 時呼叫
    member this.UnmountCanvas(containerId: string) =
        // 1. 移除 DOM 節點
        // 2. 清除相關的 Rx 訂閱與記憶體資源
```

### B. 通訊 Payload 擴充以支援 SDUI (Akka/Domain 層級)
在 `argu-actor` 發送訊息並接收回覆時，Payload 需要同時攜帶 fCell 資料與 SDUI 定義。

#### 建議修改點 (`ActorFabric.fs` 或 Domain):
```fsharp
// 擴充 WebSocket 的回覆結構
type CommSpaCanvasReply = {
    RequestId: string
    Status: string
    DataValue: string // 序列化後的 fCell2 JSON
    SduiDefinition: string // FSkynet 的 UI 宣告 JSON
}
```

### C. 建立前端 Action Dispatcher (類似 Dash Callback)
為了支援類似 Dash 的宣告式動作，SPA 的 WebSharper 端需要一個統一的事件處理中心。當使用者點擊按鈕或操作 Grid 時，Dispatcher 會根據 SDUI 宣告決定是**本地執行**還是**發送給後端**。

```fsharp
type ActionDispatcher(wsContext, canvasState) =
    member this.Dispatch(action: SduiAction) =
        match action with
        | LocalAction(LocalSort(gridId, colName)) ->
            // 本地處理：直接排序 fCell 資料並更新 View
            canvasState.SortGrid(gridId, colName)
        | RemoteAction(SendCommand(cmd, stateRefs)) ->
            // 後端互動：蒐集畫布上特定元件的數值，透過 WebSocket 送回 Actor
            let payload = canvasState.CollectValues(stateRefs)
            wsContext.Send({ Type = "dynamic:action"; Command = cmd; Payload = payload })
```

### D. ServerOptions 與路由分發 (Suave 層級)
除了 WebPart，`ServerOptions` 應允許註冊上述渲染器的後端對應邏輯與靜態資源。

---

## 3. 模組載入流程 (Module Loading Flow)

```fsharp
type ICommSpaExtension =
    abstract member Initialize : hub:CommHub * fabric:CommSpaActorFabric -> unit

// 外部 DLL 實作
type AnalyticsExtension() =
    interface ICommSpaExtension with
        member _.Initialize(hub, fabric) =
            // 註冊一個處理 "analytics-dashboard" 的動態 Actor
            fabric.RegisterModuleHandler("analytics", Props.Create<AnalyticsActor>())
```

---

## 4. 預期效益

1.  **SPA 權責明確**: SPA 專案專注於「基礎建設」，包含 Canvas 的開關、記憶體清理、基礎的 DOM 繪製，以及事件的分發。
2.  **動態模組強大**: Analytics 或 e2eQuotation 的複雜邏輯可透過 FSkynet SDUI 下發，後端完全掌控 UI 佈局與可用動作。
3.  **效能最佳化**: 將 Sorting 與 Aggregation 等行為歸類為 `LocalAction`，在 WebSharper 端直接操作 fCell 記憶體，無需往返 WebSocket。
