# Test Plan (TEST) - PulseTrade.Comm.Spa.Dynamic

## 1. 測試專案規劃
測試專案位於 `tests/PulseTrade.Comm.Spa.Dynamic.Tests`。
使用 **Expecto** 框架進行單元測試與整合測試。
此測試計畫依據 WBS 的切分進行實作前定義。

## 2. 測試案例規劃 (Test Cases)

### 2.1 後端擴充點掛載測試 (Server Extension Mount Test)
- **測試目標**：確保 `CommHub.useDynamicSdui()` 能正確將 Dynamic Extension 的環境或 Actor 載入。
- **測試原理**：
  1. 實例化一個模擬的 `CommHub` (利用唯讀的 PTCS 上游套件建立 Dummy Hub)。
  2. 呼叫 `.useDynamicSdui()` 擴充方法。
  3. **判讀標準**：驗證呼叫後不拋出例外，且回傳的 Hub 實例非空。如果系統內建 Actor 查詢機制，則檢查指定的 Actor (`ShowcaseDemoActor` 等) 是否存在。

### 2.2 FCell AST Parser 轉換測試 (AST to JSON Test)
- **測試目標**：驗證抽離出來的 `FCell2Interop.fs` 能將 F# DSL 正確序列化為 JSON Payload。
- **測試原理**：
  1. 建立一個包含 `GridFeatures`, `CanvasComponent` 等巢狀 `fCell2` AST。
  2. 呼叫 `toJsonString()` 或 `toMessagePayload()` 函式。
  3. **判讀標準**：驗證輸出的字串是合法的 JSON，且 `schema` 欄位為 `"ptc.comm.fcell2.chat.v1"` 或預期的 `fskynet-sdui` 字串，並且內容符合預先定義的 Schema 格式。

### 2.3 前端 Renderer 註冊測試 (Client Renderer Hook Test)
- **測試目標**：確保前端的 SDUI Renderer 在被觸發時，能正確識別對應的 JSON Payload。
- **測試原理**：
  由於 F# WebSharper 的 DOM 邏輯在 Server 端 (Node/JS) 執行單元測試較為困難，我們在此測試 `TryRender` 的預判定邏輯。
  1. 直接實例化 `DynamicSduiRenderer` 中的 `TryRender` (去除 UI 操作部分，或模擬 DOM Node 回傳)。
  2. 傳入包含 `"schema": "fskynet-sdui"` 的字串，驗證回傳 `Some node`。
  3. 傳入一般字串 `"hello world"`，驗證回傳 `None`。
  4. **判讀標準**：Renderer 必須只對特定的 SDUI Schema 起作用，不會誤攔截一般對話。

## 3. 開發與測試流程 (TDD Execution)
根據 WBS，在進入 `WBS-200` 系列的開發前，必須先完成此文件內 `2.1` 到 `2.3` 所有的 Expecto 測試撰寫 (測試案例初期應該會是 Failing 的，等待實作後轉為 Passing)。

## 4. RFC-PTCS-DYNAMIC-0002 Dynamic Argu Form Gates

### DYN-T-401 Formal RFC flow

驗證文件：

- `doc/RFC-PTCS-DYNAMIC-0002.dynamic-argu-form-runtime.md`
- `doc/REQ.md`
- `doc/SA.md`
- `doc/SD.md`
- `doc/WBS.md`
- `doc/TEST.md`
- `doc/Traceability.md`
- `doc/DevLog.md`

判讀標準：

- 來源草稿 `REQ_Dynamic_Argu_Form.md` / `RFC_Dynamic_Argu_Form.md` 保留且被 formal RFC 引用；
- formal RFC 明確區分 PTCS.Dynamic、PTCS core、PTC RN / RN.Host 責任；
- WBS 有跨專案相依順序。

### DYN-T-402 Metadata / schema generator

Verifier：`tests/PulseTrade.Comm.Spa.Dynamic.Tests` 內 `DYN-T-402`。

覆蓋：

- allowlisted metadata 轉成 `schema = "fskynet-sdui"`、`formMode = "argu-form"`；
- int/decimal/string/bool/enum/date/time/color field kind mapping；
- invalid DU type / unknown union case controlled failure；
- browser-supplied arbitrary type name 不會 unrestricted reflection；
- generated input ids / `arguParam` stable。

### DYN-T-403 SubmitArguForm codec

Verifier：`tests/PulseTrade.Comm.Spa.Dynamic.Tests` 內 `DYN-T-403`。

覆蓋：

- scoped form state collection；
- whitespace / quote escaping；
- empty optional field omission and required field validation；
- output is raw Argu args string only，不執行 shell command；
- invalid schema / missing `arguParam` controlled failure。

### DYN-T-404 / DYN-T-405 PTCS seam browser gates

PTCS `WBS-051B/C` seam 已有 first implementation；browser/runtime gate 目前由 PTCS repo 的 F# Playwright verifier 執行。2026-06-26 regression expansion 已通過 append renderer throw fallback、built-in add-key fallback、built-in `fcell-chat` textarea regression 與 desktop/mobile geometry。

預計 verifier：

```powershell
dotnet run --project .\tests\PulseTrade.Comm.Spa.Dynamic.Tests.fsproj -c Release --no-build -- --summary
dotnet fsi --exec G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\Scripts\verify.dynamicArguFormDurableProxy.playwright.fsx
```

覆蓋：

- Dynamic append input renderer replaces textarea only for matching `actor-dynamic` key；
- renderer missing/throw fallback textarea；
- Add Key dialog returns `actorAddress :: duTypeName :: unionCaseNames`；
- reload/readback keeps the same key list；
- desktop/mobile geometry has no overlap or hidden submit button；
- built-in PTCS append pages and existing `fskynet-sdui` message rendering do not regress；
- built-in `fcell-chat` textarea page can still append and read back from its stream when no Dynamic renderer owns the page。

### DYN-T-406 / PTC3-T-067 Cross-project RN proxy E2E

First runtime verifier 已通過；production-strength gate 仍需等待：

- PTCS `WBS-051D/E`；
- PTC RN `PTC3-063` / `PTC3-066` controller-region restart redelivery and provider completion gaps；
- PTC RN Host `PTC3-065` service-window operational policy for the selected deployment proof。

預計資料流：

```text
Dynamic form submit
  -> PTCS append / actor-argu path
  -> ActorArguTargetCommand.RawArgu
  -> RN DurableProxy
  -> legacy actor/service reply
  -> ActorArguTargetReply
  -> PTCS fresh history/result readback
```

此 gate 不得以 fake/mock/internal-only proof 當 final acceptance。
