# PulseTrade.Comm.Spa.Dynamic WBS

Progress 是粗略 implementation checkpoint，不代表正式驗收。

| ID | 工項 | 狀態 | Progress | Test ID | 證據 / 備註 |
|---|---|---:|---:|---|---|
| WBS-001 | Harness 與文件鏈初始化 | Done | 100 | - | `doc/` 已有 REQ/SA/SD/WBS/TEST/UPSTREAM_RFC，目錄與專案結構建置完成。 |
| WBS-002 | 單元測試框架與殼層 | Done | 100 | - | `tests/PulseTrade.Comm.Spa.Dynamic.Tests.fsproj` 建立，引入 Expecto，並在 `Program.fs` 中建立 TDD 測試殼層。 |
| WBS-003 | 核心模組與依賴參考 | Done | 100 | - | `src/PulseTrade.Comm.Spa.Dynamic.fsproj` 建立，加入 `PulseTrade.Comm.Spa 0.2.4-beta7` 與 `FAkka.FCell2 10.1.301`。 |
| WBS-101 | 後端擴充點掛載測試 | Done | 100 | TEST-101 | 驗證 `CommHub.useDynamicSdui()` 能正確註冊 Actor 而不拋出例外。(Pending upstream) |
| WBS-102 | FCell AST Parser 轉換測試 | Done | 100 | TEST-102 | 驗證 `FCell2Interop.toJsonString` 能將 `fCell2` AST 正確輸出含有 `fskynet-sdui` 的 JSON。 |
| WBS-103 | 前端 Renderer 註冊與攔截測試 | In progress | 0 | TEST-103 | 驗證 WebSharper 前端 `DynamicSduiRenderer` 能夠攔截包含 `fskynet-sdui` 標籤的字串。(Pending upstream) |
| WBS-104 | "Actor Dynamic" Tab Page 註冊測試 | In progress | 0 | TEST-104 | 驗證前端能成功註冊並呼叫 `actor-dynamic` 頁面類型的 DOM 生成。 |
| WBS-201 | 實作 FCell2Interop (AST轉JSON) | Done | 100 | TEST-102 | 將原 POC 中的 FCell 轉 JSON 邏輯抽出為通用 library function。 |
| WBS-202 | 實作 ShowcaseDemoActor | Done | 100 | - | 將動態 Demo 的 Actor 狀態機與訊息處理從 POC 移出。 |
| WBS-203 | 實作 CommHub 擴充方法 | Done | 100 | TEST-101 | 實作 `.useDynamicSdui()`，將 Actor 與系統綁定。 |
| WBS-204 | 實作 Client/DynamicRenderer | Not started | 0 | TEST-103 | 實作解析 DOM、Canvas 按鈕彈出邏輯，取代原本 PTCS 內的 hardcoded function。 |
| WBS-205 | 實作 Actor Dynamic Tab Page | Not started | 0 | TEST-104 | 根據 SD 設計，實作前端的 Tab 頁面容器與對應的 SDUI 展示元件掛載。 |
| WBS-301 | 測試覆蓋率與 TDD 驗收 | Not started | 0 | ALL | 確保 Expecto 中所有 WBS-100 系列測項由 Failing 轉為 Passing。 |
| WBS-302 | NuGet Pack 與發佈設定 | Not started | 0 | - | 準備 `.nuspec` 或 MSBuild pack 參數，輸出 `0.1.0-alpha1`。 |
