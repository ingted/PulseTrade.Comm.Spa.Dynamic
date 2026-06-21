# System Analysis (SA) - PulseTrade.Comm.Spa.Dynamic

## 1. 系統架構概念 (Architectural Concept)
本系統作為 `PulseTrade.Comm.Spa` 的 NuGet 擴充套件，採用 **外掛架構 (Plugin Architecture)**：
- 宿主 (Host) 為 `PulseTrade.Comm.Spa` (PTCS) 0.2.4-beta7 核心。
- 擴充模組 (Extension) 提供 FSkynet SDUI 特定的渲染元件與 `Actor Dynamic` 等型別。

## 2. 抽取方法論 (Extraction Methodology)
為了將原本位於 `G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa` 中的 `fskynet-sdui` 內容抽取並封裝，我們採用以下方法論：
1. **反向依賴注入 (Dependency Inversion)**：
   原本 PTCS 核心需要認識 `"schema": "fskynet-sdui"`。抽取後，核心退化為一個廣播者，僅提供 `IMessageRenderer` 的註冊介面。Extension 包將負責實作此介面並主動「掛載 (Inject)」到核心註冊表中。
2. **領域層級隔離 (Domain Segregation)**：
   Server 端的 F# AST (`FAkka.FCell2`) 轉換邏輯 (`FCell2Interop.fs`) 與特製的 Actor (`ShowcaseDemoActor`) 將從 PTCS 中移除，並以獨立的 `.fs` 模組放入 `PulseTrade.Comm.Spa.Dynamic` 中。透過 `CommHub` 的擴充方法 (`Extension Methods`) 將其路由註冊回去。
3. **前端元件解耦 (Client Component Decoupling)**：
   WebSharper 中的 `createSduiSummaryCard` 等 DOM 構建邏輯，將連同其相依的 CSS/狀態管理，全部複製到 Dynamic 專案的 `DynamicRenderer.fs` 中，並透過 `[<JavaScript>]` 屬性確保其在瀏覽器端編譯與執行。

## 3. 技術風險與潛在阻礙 (Technical Risks & Potential Obstacles)
雖然目前在 PTCS 內直接實作的 SDUI 運作良好，但抽取到 Extension 專案會面臨以下技術挑戰：
1. **WebSharper 跨組件掛載 (Cross-Assembly WebSharper Interop)**：
   WebSharper 對於跨組件 (Cross-Project) 的 DOM 操作與全域變數註冊可能會有初始化順序 (Initialization Order) 的問題。若 `DynamicRenderer.Start()` 在核心 `Client.fs` 完成載入前就被呼叫，可能會導致 NullReference 或找不到註冊表。
   *對策*：需利用 WebSharper 的 `[<SPAEntryPoint>]` 或是確定的生命週期 Hooks 來確保宿主準備好後再掛載。
2. **內部 API 存取限制 (Internal Access Restrictions)**：
   原本在 PTCS 內部實作時，SDUI 的 UI 元件可以存取 PTCS 內部 (internal) 的狀態或 Helper Functions。抽離為外部 NuGet 依賴後，只能存取 `public` API。若有必須用到的 internal API，將面臨無法存取的困境。
   *對策*：若發現依賴了 internal API，必須透過發佈 `UPSTREAM_RFC` 要求 PTCS 將該 API 升級為 public，或開放對應的 Getter/Setter。
3. **Actor 註冊與狀態還原 (Actor Registry & Replay)**：
   包含 `Actor Dynamic` 等特殊 Actor 在進行系統重啟與 Akka Journal Replay 時，如果系統只認識基礎的 Actor 型別，可能會導致 Dynamic Actor 反序列化失敗或無法重新啟動。
   *對策*：在 Server 掛載 `useDynamicSdui()` 時，必須明確包含 Actor 的 Factory Method 註冊，確保 Replay 引擎知道如何實例化這些擴充 Actor。

## 4. 模組分析 (Module Breakdown)
### 4.1 Server-Side Extension Module
- **FCell2 Interop**：負責將 F# DSL (`fCell2`) 轉換為 JSON SDUI Payload。
- **Dynamic Actors**：負責動態派發任務的 Actor，包含支援 `Actor Dynamic` Tab Page 的主體。
- **Mount API**：提供 `CommHub.useDynamicSdui()`。

### 4.2 Client-Side Extension Module (WebSharper)
- **SDUI Renderer**：處理 JSON Payload 渲染為 Canvas/Grid 等。
- **Dynamic Tab Page**：實作 `Actor Dynamic` 對應的頁面掛載。
