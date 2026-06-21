# Work Breakdown Structure (WBS) - PulseTrade.Comm.Spa.Dynamic

## 階段 0: 環境與專案建置 (Setup & Init)
- [x] **WBS-001**: 建立資料夾結構 (`src`, `tests`, `doc`) 與規劃文件 (`REQ`, `SA`, `SD`, `UPSTREAM_RFC`)。
- [ ] **WBS-002**: 建立測試專案 `tests/PulseTrade.Comm.Spa.Dynamic.Tests.fsproj` (包含 Expecto 框架設定)。
- [ ] **WBS-003**: 建立核心擴充專案 `src/PulseTrade.Comm.Spa.Dynamic.fsproj`，並加入 NuGet 參考 (`PulseTrade.Comm.Spa, 0.2.4-beta7`、`WebSharper`)。
- [ ] **WBS-004**: 完成 `TEST.md` 單元測試計畫撰寫。

## 階段 1: 測試驅動 (TDD Phase)
- [ ] **WBS-101**: 撰寫後端擴充的單元測試，模擬掛載 `CommHub.useDynamicSdui()` 驗證是否順利註冊 Actor。
- [ ] **WBS-102**: 撰寫 AST Parser (`FCell2Interop`) 的單元測試，驗證 F# DSL 轉 JSON 的輸出。
- [ ] **WBS-103**: (若技術可行) 撰寫 WebSharper Renderer 的 Mock 註冊測試。

## 階段 2: 核心模組抽離實作 (Implementation Phase)
- [ ] **WBS-201**: 實作 `Server/FCell2Interop.fs` (由原 POC 遷移並改寫)。
- [ ] **WBS-202**: 實作 `Server/Actors.fs` (將 `ShowcaseDemoActor` 抽出)。
- [ ] **WBS-203**: 實作 `Server/Extension.fs` (建立 `CommHub.useDynamicSdui()` 掛載方法)。
- [ ] **WBS-204**: 實作 `Client/DynamicRenderer.fs` (WebSharper Renderer，實作解析 DOM 與 Canvas 彈出邏輯)。

## 階段 3: 驗證與發佈 (Verification & Release)
- [ ] **WBS-301**: 執行所有 Expecto 測試，確保覆蓋率與預期相符。
- [ ] **WBS-302**: 建立 `dotnet pack` 發佈設定。
