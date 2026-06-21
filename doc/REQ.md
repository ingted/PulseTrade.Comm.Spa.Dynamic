# Requirements (REQ) - PulseTrade.Comm.Spa.Dynamic

## 1. 專案背景 (Background)
`PulseTrade.Comm.Spa` (PTCS) 目前內建了 `fskynet-sdui` (Server-Driven UI) 的邏輯，這使得核心元件過於臃腫且與特定展示層耦合過深。使用者期望能夠將這些動態 UI 與 FSkynet Actor 邏輯抽離為一個獨立的擴充 NuGet 套件 (`PulseTrade.Comm.Spa.Dynamic`)，提供一種可掛載 (Mount / Inject) 的機制，只有在需要的環境中才載入並啟用 SDUI 功能。

## 2. 需求目標 (Goals)
1. **抽離動態元件**：將依賴 `FAkka.FCell2` 與 SDUI 相關的前端及後端邏輯打包進獨立專案中。
2. **開發 NuGet 擴充套件**：建立 `PulseTrade.Comm.Spa.Dynamic`，並參考核心專案 `PulseTrade.Comm.Spa, 0.2.4-beta7` (唯讀參考)。
3. **前端渲染掛載**：支援在 WebSharper 專案中向宿主註冊客製化 Renderer (例如將 JSON Payload 轉換為 Canvas Button)。
4. **後端服務掛載**：支援透過 Extension Method，將 Dynamic 相關的 Actor (`ShowcaseDemoActor`) 掛載到現有的 `CommHub` 或 `ActorSystem` 中。
5. **測試覆蓋率**：所有工項都必須在開工前完成 Expecto 單元測試。

## 3. 約束條件 (Constraints)
- **原始碼唯讀**：上游專案 `G:\PulseTrade2.fs` 不可修改。任何對於上游掛載點的修改需求，均已記錄於 `UPSTREAM_RFC.md` 並交由專責人員處理。
- **開發順序**：遵循 TDD (Test-Driven Development) 流程，測試寫在前面，開發在後面。
- **續航規則 (Endurance Rule)**：切分適當的 WBS，分階段實作與 Commit，避免一次執行過多任務而超出 Token 或執行限制。

## 4. 交付項目 (Deliverables)
1. `PulseTrade.Comm.Spa.Dynamic` 原始碼 (`src` 資料夾，包含前端 WebSharper 擴充與後端 Actor)。
2. `PulseTrade.Comm.Spa.Dynamic.Tests` 測試原始碼 (`tests` 資料夾，包含 Expecto 測試)。
3. 說明文件 (`REQ`, `SA`, `SD`, `WBS`, `TEST`, `UPSTREAM_RFC`)。
