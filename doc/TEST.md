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

PTCS `WBS-051B/C` seam 已有 first implementation；browser/runtime gate 目前由 PTCS repo 的 F# Playwright verifier 執行。2026-06-26 regression expansion 已通過 append renderer throw fallback、invalid blank renderer submit isolation、duplicate target key idempotency、built-in add-key fallback、built-in `fcell-chat` textarea regression 與 desktop/mobile geometry。

預計 verifier：

```powershell
dotnet run --project .\tests\PulseTrade.Comm.Spa.Dynamic.Tests.fsproj -c Release --no-build -- --summary
dotnet fsi --exec G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\Scripts\verify.dynamicArguFormDurableProxy.playwright.fsx
```

覆蓋：

- Dynamic append input renderer replaces textarea only for matching `actor-dynamic` key；
- renderer missing/throw fallback textarea；
- blank renderer submission shows controlled validation and does not reach RN DurableProxy；
- Add Key dialog first-slice proof returned `actorAddress :: duTypeName :: unionCaseNames` with no delimiter-joined union-case segment；RFC-0003 revised canonical path is now `actorAddress :: duTypeOrTemplateKey :: canonicalArgString`；
- reload/readback returns the PTCS ordered append-page key list while preserving actor address、template key and canonical arg string as separate key segments；
- duplicate Dynamic target key submit keeps one projected key/card；
- desktop/mobile geometry has no overlap or hidden submit button；
- built-in PTCS append pages and existing `fskynet-sdui` message rendering do not regress；
- built-in `fcell-chat` textarea page can still append and read back from its stream when no Dynamic renderer owns the page。

### DYN-T-406 / PTC3-T-067 Cross-project RN proxy E2E

Browser/runtime verifier 已通過 user-facing UI E2E contract：`--pcsl-root` + run-scoped fresh PCSL root、PTCS + Dynamic extension、DurableProxy actor、legacy echo actor、Playwright 建立 `actor-dynamic` page、Dynamic add-key target、variable-length Dynamic key tail、canonical PTCS key readback、text/number/enum/tuple/bool/list form input、raw Argu preview/send、RN DurableProxy fCell2 string forwarding、legacy echo reply 與 `ActorArguTargetReply` full target-key readback。Production-strength gate 仍需等待：

- PTCS `WBS-051D/E`；
- PTC RN `PTC3-063` / `PTC3-066` controller-region restart redelivery and provider completion gaps；
- PTC RN Host `PTC3-065` service-window operational policy for the selected deployment proof。

預計資料流：

```text
Dynamic form submit
  -> PTCS append / actor-argu path
  -> ActorArguTargetCommand.RawArgu
  -> RN DurableProxy
  -> fCell2 string
  -> legacy actor/service reply
  -> ActorArguTargetReply
  -> PTCS fresh history/result readback
```

此 gate 不得以 fake/mock/internal-only proof 當 final acceptance。

## 5. RFC-PTCS-DYNAMIC-0003 Unified SDUI / Form DSL Gates

### DYN-T-501 DSL document model

Verifier：`tests/PulseTrade.Comm.Spa.Dynamic.Tests`。

Current package verifier：`dotnet run --project .\tests\PulseTrade.Comm.Spa.Dynamic.Tests.fsproj --no-restore`，Expecto 9/9 pass。

覆蓋：

- `SduiDocument` 可表達 Canvas 與 FormInput surface；
- shared node/action/binding JSON codec round-trip；
- Canvas-only node 與 FormInput node 不需要不同 schema root；
- invalid schema/version/duplicate id controlled failure。

目前已落地的 package coverage：

- `SduiFormDocument.fromArguFormSchema` 產生 `schema=fskynet-sdui`、`surface=FormInput`、stable `documentId`；
- PFCF_AKKA_CMD fixture 反射 `SimpleAction`、`BBA`、`GenByColMeta`；
- `GenByColMeta` tuple item kinds 驗證為 `bool-value`, `bool-value`, `text`, `enum`。

### DYN-T-502 Argu-to-FormDsl adapter

Verifier：`tests/PulseTrade.Comm.Spa.Dynamic.Tests`。

Current package verifier：`dotnet run --project .\tests\PulseTrade.Comm.Spa.Dynamic.Tests.fsproj --no-restore`，Expecto 9/9 pass。

覆蓋：

- host-registered `IArgParserTemplate` 轉成 Form DSL document；
- 每個 requested union case 轉成 visible section；
- string/number/bool/enum/tuple/list/nested ParseResults supported or controlled unsupported；
- no `SampleArgu` or PTCS.Host-specific DU in package source；
- unknown DU type / union case 不做 unrestricted browser reflection。

目前已落地的 package coverage：

- server `SubmitArguFormCodec.buildRawArgu` 與 frontend `ClientRawArguCodec.buildRawArguFromValues` 產生一致 raw arg string；
- PFCF_AKKA_CMD covered cases：`SimpleAction`、`Entrust`、`PFCFGTC`、`BBA`、`Cooperative`、`ParentChilds`、`FractionalQuote`、`GenByColMeta`、`TableName`；
- expected raw arg strings include `--simpleaction "rebuild all"`、`--pfcfgtc gf --pfcfgtc goi`、`--bba F001 B001 M123`、`--genbycolmeta true false dbo fsrecord`、`--tablename Orders --tablename "Positions Today"`。

### DYN-T-503 Dynamic target resolver first-slice regression

Verifier：`tests/PulseTrade.Comm.Spa.Dynamic.Tests`。

Current package verifier：`dotnet run --project .\tests\PulseTrade.Comm.Spa.Dynamic.Tests.fsproj --no-restore`，Expecto 9/9 pass。

覆蓋：

- `[ actorAddress; formDslId ]` resolves as direct DSL target；
- `[ actorAddress; duTypeName; case1; case2 ]` resolves as Argu adapter target；
- first item is always actor address；
- no canonical `1:duType:` / `2:unionCases:` prefix；
- unknown second segment returns renderer validation error。

目前已落地的 package coverage：

- `DynamicTargetKey.tryResolve` resolves `[ actorAddress; formDslId ]` to `DirectDslTarget`；
- `DynamicTargetKey.tryResolve` resolves `[ actorAddress; duTypeName; SimpleAction; BBA; GenByColMeta ]` to `ArguTemplateTarget` and preserves union-case tail order；
- unknown discriminator、unknown union case、direct DSL target with union-case tail all fail with controlled errors。

此 test 是 first-slice regression。RFC-PTCS-DYNAMIC-0003 的新 canonical DU/template target 已改為 `[ actorAddress; duTypeOrTemplateKey; canonicalArgString ]`，由 DYN-T-507..511 接手。

### DYN-T-504 Backend-linked option provider

Verifier：focused F# test plus browser E2E after PTCS `WBS-053E`。

覆蓋：

- `QueryOptions(providerId, dependsOn)` updates dependent select options；
- unregistered provider rejected；
- arbitrary URL/header/script is not accepted；
- provider diagnostics do not include secrets。

### DYN-T-505 Actor key bound to direct DSL target

Verifier：F# Playwright script in PTCS repo after PTCS `WBS-053` seam and Dynamic v2 renderer are ready。

Required path：

```text
actor key [ actorAddress; formDslId ]
  -> Dynamic resolves direct Form DSL
  -> FormInput renderer submits ValueText
  -> PTCS existing append / actor-argu path
  -> target actor / proxy receives command
  -> PTCS history readback
```

### DYN-T-506 Actor key bound to DU/template + canonical arg string

Verifier：F# Playwright script in PTCS repo with PTCS.Host demo DU。

Required path：

```text
actor key [ actorAddress; duTypeOrTemplateKey; canonicalArgString ]
  -> PTCS.Dynamic backend parses canonicalArgString with registered Argu parser
  -> Argu-to-FormDsl adapter
  -> parsed root cases and supported subcommands visible simultaneously
  -> composite raw Argu submit
  -> RN DurableProxy / legacy echo
  -> ActorArguTargetReply
  -> PTCS full target-key history readback
```

PTCS.Host demo source is `C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic\doc\example DU.txt` decoded as Big5/cp950。Missing external types/enums must be stubbed or excluded with controlled unsupported-case diagnostics in PTCS.Host, not in PTCS.Dynamic package。

### DYN-T-507 Arg-string target resolver

Verifier：`tests/PulseTrade.Comm.Spa.Dynamic.Tests` plus backend resolver verifier。

覆蓋：

- `[ actorAddress; formDslId ]` remains direct DSL target；
- `[ actorAddress; duTypeOrTemplateKey; canonicalArgString ]` resolves to parser-backed Dynamic target；
- no `hub.useDynamicSdui(...)` / no Dynamic resolver means PTCS core can still use only `actorAddress` as built-in actor key；
- unknown template key、missing canonical arg string、parse failure all return controlled error。

### DYN-T-508 Alias binding

Verifier：`tests/PulseTrade.Comm.Spa.Dynamic.Tests`。

覆蓋：

- case alias appears in `SduiDocument` section title；
- field alias appears in input label；
- option alias appears in select display label；
- raw Argu command still uses canonical Argu names and values；
- alias metadata does not come from browser target key。

### DYN-T-509 Parser-backed Form DSL defaults

Verifier：`tests/PulseTrade.Comm.Spa.Dynamic.Tests`。

覆蓋：

- canonical arg string is parsed by the registered `IArgParserTemplate`；
- parsed root cases determine rendered sections；
- field default values come from parse result / token scan；
- unsupported fields/subcommands produce controlled unsupported diagnostics。

### DYN-T-510 ParseResults / subcommand raw command builder

Verifier：`tests/PulseTrade.Comm.Spa.Dynamic.Tests`。

Required expected command：

```text
--pfcfedx trivial --pfcfgtcconf OIInf TAIFEX FillSquareCombine OrderByTXDT CathayBKTaifexFill --to 90000 --parentchilds 2 5 --bba F008 000 9910357 --decimalquote 6 0 --round 6 4 2 datarange --referencedatemode ModeAccountingDate --between 20251104 20251104 --calibrate2curdayiflargerthancurday
```

覆蓋：

- `ParseResults<PFCF_AKKA_CMD_DATA_RANGE>` is discovered as tail subcommand；
- root args precede `datarange`；
- `datarange` precedes `--referencedatemode` / `--between` / `--calibrate2curdayiflargerthancurday`；
- command rebuild is deterministic and matches expected raw string exactly。

### DYN-T-511 Browser E2E for backend-resolved FormInput DSL

Verifier：F# Playwright script in PTCS repo after PTCS.Host references updated package。

Required path：

```text
create actor-dynamic page
  -> add target key [ actorAddress; duTypeOrTemplateKey; canonicalArgString ]
  -> Dynamic backend resolves FormInput DSL
  -> browser renders alias labels and parsed default values
  -> submit form
  -> ActorArguTargetCommand.RawArgu equals DYN-T-510 expected command
  -> RN DurableProxy / echo path returns ActorArguTargetReply
```

UI gate must inspect visible labels/controls and final submitted raw string. It must not pass by only testing server codec.
