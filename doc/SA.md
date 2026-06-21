# System Analysis (SA) - PulseTrade.Comm.Spa.Dynamic

## 1. 系統架構概念 (Architectural Concept)
本系統作為 `PulseTrade.Comm.Spa` 的 NuGet 擴充套件，採用 **外掛架構 (Plugin Architecture)**：
- 宿主 (Host) 為 `PulseTrade.Comm.Spa` (PTCS) 0.2.4-beta7 核心。
- 擴充模組 (Extension) 提供 FSkynet SDUI 特定的渲染元件與 Actor 型別。

## 2. 模組分析 (Module Breakdown)

### 2.1 Server-Side Extension Module
- **FCell2 Interop (Server-AST)**：將 `fCell2Interop.fs` 的邏輯遷移到擴充專案，負責將 F# DSL 轉換為 JSON SDUI Payload。
- **Dynamic Actors**：如 `ShowcaseDemoActor` 或 `DynamicCanvasDemo` 等負責動態派發任務與更新 UI 畫布的 Akka Actors。
- **Mount API**：設計 `CommHub.useDynamicSdui()` 擴充方法，以便在上游初始化時可以一行代碼掛載。

### 2.2 Client-Side Extension Module (WebSharper)
- **SDUI Renderer**：處理 `fskynet-sdui` Schema 的 DOM 產生器 (如 Tree, Rolling, ContextMenu, Autocomplete 等)。
- **Registration Hook**：利用 WebSharper 的 `[<JavaScript>]` 機制，設計在初始化階段向核心的 `IMessageRenderer` 註冊表注入 SDUI Renderer。

## 3. 相依性 (Dependencies)
- `PulseTrade.Comm.Spa` (版本：0.2.4-beta7)
- `FAkka.FCell2` (版本：同步上游)
- `WebSharper` 與 `WebSharper.UI`

## 4. 運作流程 (Data Flow)
1. 前端在瀏覽器載入時，執行 `DynamicClient.Start()`，將 SDUI Renderer 註冊到宿主。
2. 宿主收到包含 `"schema": "fskynet-sdui"` 的推播訊息時，遍歷 Renderer，匹配到 Dynamic Extension 的渲染器。
3. 渲染器產生動態 Canvas (JSON Snippet + 展開按鈕)，顯示在畫面上。
4. 按下展開後，觸發 SDUI 元件的深度渲染 (App Loader, Tree, Grid 等)。
