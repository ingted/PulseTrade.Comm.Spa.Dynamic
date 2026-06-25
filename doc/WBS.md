# PulseTrade.Comm.Spa.Dynamic WBS

Progress 是粗略 implementation checkpoint，不代表正式驗收。

| ID | 工項 | 狀態 | Progress | Test ID | 證據 / 備註 |
|---|---|---:|---:|---|---|
| WBS-001 | Harness 與文件鏈初始化 | Done | 100 | - | `doc/` 已有 REQ/SA/SD/WBS/TEST/UPSTREAM_RFC，目錄與專案結構建置完成。 |
| WBS-002 | 單元測試框架與殼層 | Done | 100 | - | `tests/PulseTrade.Comm.Spa.Dynamic.Tests.fsproj` 建立，引入 Expecto，並在 `Program.fs` 中建立 TDD 測試殼層。 |
| WBS-003 | 核心模組與依賴參考 | Done | 100 | - | `src/PulseTrade.Comm.Spa.Dynamic.fsproj` 建立，加入 `PulseTrade.Comm.Spa 0.2.4-beta7` 與 `FAkka.FCell2 10.1.301`。 |
| WBS-101 | 後端擴充點掛載測試 | Done | 100 | TEST-101 | 驗證 `CommHub.useDynamicSdui()` 能正確註冊 Actor 而不拋出例外。(Pending upstream) |
| WBS-102 | FCell AST Parser 轉換測試 | Done | 100 | TEST-102 | 驗證 `FCell2Interop.toJsonString` 能將 `fCell2` AST 正確輸出含有 `fskynet-sdui` 的 JSON。 |
| WBS-103 | 前端 Renderer 註冊與攔截測試 | Done | 100 | TEST-103 | 驗證 WebSharper 前端 `DynamicSduiRenderer` 能夠攔截包含 `fskynet-sdui` 標籤的字串。(Pending upstream) |
| WBS-104 | "Actor Dynamic" Tab Page 註冊測試 | Done | 100 | TEST-104 | 驗證前端能成功註冊並呼叫 `actor-dynamic` 頁面類型的 DOM 生成。 |
| WBS-201 | 實作 FCell2Interop (AST轉JSON) | Done | 100 | TEST-102 | 將原 POC 中的 FCell 轉 JSON 邏輯抽出為通用 library function。 |
| WBS-202 | 實作 ShowcaseDemoActor | Done | 100 | - | 將動態 Demo 的 Actor 狀態機與訊息處理從 POC 移出。 |
| WBS-203 | 實作 CommHub 擴充方法 | Done | 100 | TEST-101 | 實作 `.useDynamicSdui()`，將 Actor 與系統綁定。 |
| WBS-204 | 實作 Client/DynamicRenderer | Done | 100 | TEST-103 | 實作解析 DOM、Canvas 按鈕彈出邏輯，取代原本 PTCS 內的 hardcoded function。 |
| WBS-205 | 實作 Actor Dynamic Tab Page | Done | 100 | TEST-104 | 根據 SD 設計，實作前端的 Tab 頁面容器與對應的 SDUI 展示元件掛載。 |
| WBS-301 | 測試覆蓋率與 TDD 驗收 | Done | 100 | ALL | 確保 Expecto 中所有 WBS-100 系列測項由 Failing 轉為 Passing。 |
| WBS-302 | NuGet Pack 與發佈設定 | Done | 100 | - | 準備 `.nuspec` 或 MSBuild pack 參數，輸出 `0.1.0-alpha1`。 |
| DYN-WBS-401 | Dynamic Argu Form formal RFC flow | Review | 100 | DYN-T-401 | 新增 `RFC-PTCS-DYNAMIC-0002.dynamic-argu-form-runtime.md`，並同步 REQ/SA/SD/WBS/TEST/Traceability/DevLog；來源草稿保留為 `REQ_Dynamic_Argu_Form.md` / `RFC_Dynamic_Argu_Form.md`。 |
| DYN-WBS-402 | Argu metadata registry / schema generator | Review | 100 | DYN-T-402 | 新增 `Server/ArguForm.fs`：allowlisted sample DU schema、`schema=fskynet-sdui`、`formMode=argu-form`、text/number/enum/tuple/list/bool metadata；Expecto `DYN-T-402` 通過。Remaining：若後續開放 arbitrary DU reflection，需另補安全 allowlist gate。 |
| DYN-WBS-403 | SubmitArguForm state and command-line codec | Review | 100 | DYN-T-403 | 新增 `SubmitArguFormCodec.buildRawArgu` 並以 Expecto `DYN-T-403` 驗證 whitespace/quote escaping、list repetition、bool flag；browser renderer 也以同語意更新 raw preview。 |
| DYN-WBS-404 | Append input renderer integration | Review | 95 | DYN-T-404 | 新增 `Client/ArguFormRenderer.fs`，透過 PTCS `PulseTradeRegisterAppendInputRenderer` 註冊 `dynamic-argu-append-input`；selected Dynamic key 時取代 textarea，並在 E2E 驗證 text/number/enum/tuple/bool/list controls、raw preview、submit、append renderer throw fallback、blank renderer submit controlled validation 與 desktop/mobile geometry。Remaining：後續若開放更多 payload shape，再補專門 invalid-node unit gate。 |
| DYN-WBS-405 | Add-key dialog renderer integration | Review | 96 | DYN-T-405 | Dynamic add-key renderer 已改為回傳 canonical variable-length key `[ actorAddress; "1:duType:<type>"; caseA; caseB; ... ]`，不再把 union cases delimiter-join 成單一 segment；PTCS 端以 `unionCaseNames` string list tail 提供 renderer context。E2E 驗證 reload/readback key list、full key history、built-in add-key fallback，以及 duplicate target key 重送後 projection/card 維持單一項。Remaining：production split-service 後再補跨 process key registry replay。 |
| DYN-WBS-406 | Cross-project RN proxy E2E | Review | 74 | DYN-T-406 / PTC3-T-067 | `G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\Scripts\verify.dynamicArguFormDurableProxy.playwright.fsx` 已完成 browser/runtime E2E：fresh PCSL root + Dynamic form -> variable-length union-case key tail -> PTCS canonical sorted key readback -> raw Argu args -> `ActorArguTargetCommand.RawArgu` -> PTC RN DurableProxy low-cognitive path -> legacy echo actor -> `ActorArguTargetReply` -> PTCS full target-key history readback。Remaining：split-service RN.Host / ProcSupervisor / ShardingDelivery restart-redelivery / production provider proof。 |

## Dynamic Argu Form 相依順序

1. `DYN-WBS-401` 先完成文件與責任邊界。
2. PTCS `WBS-051B/C` 先提供 append input renderer / add-key dialog renderer seam；Dynamic `DYN-WBS-402/403` 可並行開發 local schema/renderer/codec。
3. `DYN-WBS-404/405` 需要 PTCS seam package 或本機 project reference。
4. PTCS `WBS-051D` 驗證 browser form、fallback、geometry、built-in regression。
5. PTC RN `PTC3-063/066/065` 完成 controller-region restart redelivery、provider-bound completion與 service-window proof 後，再做 `DYN-WBS-406` / `PTC3-067` production E2E。
