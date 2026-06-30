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

Current package verifier：`dotnet run --project .\tests\PulseTrade.Comm.Spa.Dynamic.Tests.fsproj -c Release --no-restore -- --summary --no-spinner`，Expecto 15/15 pass。

覆蓋：

- `SduiDocument` 可表達 Canvas 與 FormInput surface；
- shared node/action/binding JSON codec round-trip；
- Canvas-only node 與 FormInput node 不需要不同 schema root；
- invalid schema/version/duplicate id controlled failure。

## 6. RFC-PTCS-DYNAMIC-0005 ActorsPage Renderer Gates

### DYN-T-526 ActorsPage renderer registration/classifier

Verifier：`tests/PulseTrade.Comm.Spa.Dynamic.Tests` 內 `DYN-T-526`。

Current first-slice 判讀：

- payload 含 `ActorTopologyPage` 時，`ActorDynamicTab.IsActorsPagePayload` 回 true；
- Dynamic entrypoint 會呼叫 page renderer registration；
- full WebSharper bundle build 必須用 `DYN-VFY-001` short path 先過。

限制：目前 classifier 使用單一 `IndexOf("ActorTopologyPage")`，因 `String.Contains` 與多段 predicate 會讓 WebSharper 10.1.5.674 `wsfsc.exe` crash。嚴格 `schema/surface/documentType` parser 是 DYN-T-528 之後的 gate。

### DYN-T-527 Generic Canvas isolation

Verifier：`tests/PulseTrade.Comm.Spa.Dynamic.Tests` 內 `DYN-T-527`。

Canvas message payload 不含 `ActorTopologyPage` 時，ActorsPage classifier 回 false。Generic Canvas renderer 仍由 `DynamicRenderer.TryRender` 處理一般 `schema=fskynet-sdui` message reply。

### DYN-T-528 ActorsPage node grouping and tree toggle gate

2026-06-28 current source-host evidence:

- PTCS source host + Dynamic source Release bundle passes a Playwright MCP gate on `/actors`;
- Dynamic page renderer is registered and the same renderer path is also protected through the transitional `MessageRenderers` fallback;
- Dynamic page host is present, fallback tree/table rows are absent, blocks are ordered PTCS Host -> GW Host -> RN Host, and full `akka.tcp://...` addresses are visible;
- `0.1.3-beta24` restores hierarchy presentation inside each host block: virtual `/user` and `/system` ancestors remain visible, status dots and connector lines are present, and virtual ancestors do not create a synthetic Unknown block;
- `0.1.3-beta27` with PTCS `0.2.5-beta40` verifies the same hierarchy and also asserts PTCS core does not append fallback `actor-node` / `actor-card` DOM after Dynamic accepts `/actors`;
- boxed tree toggle is functional: click changes `- / aria-expanded=true` to `+ / aria-expanded=false` and re-expand restores the child rows;
- evidence is `G:\PulseTrade.fs\log\20260628\20260628220000.actors-page-toggle-check.json`;
- screenshot evidence is `G:\PulseTrade.fs\log\20260628\20260628220000.actors-page-toggle-fixed.png`.
- reusable F# Playwright verifier `G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\Scripts\verify.actorsPageDynamic.playwright.fsx` now repeats the accepted path with locator-only assertions for Dynamic ownership, fallback absence, core-card absence, full addresses, report controls, status dots, connectors, depth rows, no synthetic Unknown block, and visible row collapse/expand; beta40/Dynamic beta27 screenshot evidence is `G:\PulseTrade.fs\log\20260629\20260629011000.actorspage-beta40-dyn27-mcp.png`.
- `0.1.3-beta29` adds browser-local report schedule start/stop verification to the same F# Playwright verifier; package bundle verification now rejects the beta28 stale schedule-disabled marker.

### DYN-T-529..532 Remaining ActorsPage gates

尚未完成：

- strict ActorsPage DSL parser / codec；
- server-side persisted report schedule / restart / failover visual-state gates beyond the current grouped/toggle/hierarchy/schedule-browser slice；
- Dynamic absent / unsupported renderer fallback is covered by PTCS `G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\Scripts\verify.actorsActorTree.playwright.fsx -- --with-unsupported-client-extension`; Playwright MCP evidence: `G:\PulseTrade.fs\log\20260629\20260629001159.actors-unsupported-fallback-playwright-mcp.png`。

目前已落地的 package coverage：

- `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta29` rollout completed the package/public 81 hierarchy ownership plus browser-local report schedule gate with PTCS `0.2.5-beta40` and public release `live81-ptcs-beta40-dynamic-beta29-report-schedule-202606290205`；
- `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta30` rollout completed the offline cleanup/display slice with PTCS `0.2.5-beta41` and public release `live81-ptcs-beta41-dynamic-beta30-offline-poc2-202606290748`; `poc.full.nuget.2.fsx -- --no-wait` verifies the beta41/beta30 NuGet POC path with Actor Argu Add target key, no `+ Page` Actor Dynamic page creation, and non-empty `/actors/api/snapshot` from the POC2 `nuget2-echo` actor registry projection. Playwright evidence for local POC2 `/actors` is `G:\PulseTrade.fs\log\20260629\poc2-actors-page-fixed-deep-snapshot.md` and `G:\PulseTrade.fs\log\20260629\poc2-actors-page-fixed-deep.png`.
- `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta33` pairs with PTCS `0.2.5-beta43` and `FAkka.WebSocket 1.569.101.301-win6`. `poc.full.nuget.2.fsx -- --no-wait` now additionally asserts the `POC2 FormInput target` alias survives the server-side ActorArgu send/probe path, covering the regression where a later blank key intent replay overwrote alias display with the long target key. The same run completed without `WebSocket disconnected` / `ConnectionAborted` console noise; beta33 also adds ActorsPage browser console groups `[PTCS.Dynamic ActorTree DSL] RENDER/RELOAD` for raw/parsed tree DSL inspection.
- `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta38` pairs with PTCS `0.2.5-beta48`. `poc.full.nuget.journal.fsx -- --no-wait` verifies the ActorRegistry PingPong stop/reload path: the stopped actor is projected as `terminated`, default active ActorTree DSL filters it (`pingPongFiltered=true`), and `includeOffline` keeps it only for diagnostics.
- `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta47` pairs with PTCS `0.2.5-beta57`. `poc.full.nuget.journal.ACL.fsx -- --no-wait --local-port 18082 --github-port 18081 --cluster-port 18787 --pcsl-root .\.pcsl\verify.acl.beta47` verifies the ACL/Login dual-auth path on non-conflict ports: 81-style GitHub OAuth host, 82-style PTCS.Login host, DamnWZ/AssTerry pages, Echo/PingPong target keys, local login, ACL snapshot, Dynamic bundle, durable ActorArgu echo, PingPong stop filtering, fixed actor name reuse, PTCS beta53 Login session-store package compatibility, PTCS beta54 ACL audit compatibility, PTCS beta55 WebSocket principal revalidation compatibility, PTCS beta56 WebSocket proxy cleanup compatibility, and PTCS beta57 HTTP ACL canonical resource compatibility. Public 81 health/alignment is `live81-ptcs-beta57-dynamic-beta47-acl-http-matrix-202607010238`; Playwright MCP public `/actors` evidence is `G:\PulseTrade.fs\log\20260630\public81-ptcs-beta57-actors-snapshot.md`, `G:\PulseTrade.fs\log\20260630\public81-ptcs-beta57-actors.png`, and `G:\PulseTrade.fs\log\20260630\public81-ptcs-beta57-actors-console.txt`. Full authenticated visual proof still needs a fresh browser gate.
- `poc.full.nuget.journal.fsx` manual FSI mode must use `ensureEchoActorRegistered()`, `stopEchoActor()`, or `recreateEchoActor()` for the stable `nuget-journal-echo` actor. Re-running `ActorOfRegistered(..., actorName)` while the actor is live is expected to fail because Akka actor names are unique under `/user`; after stop and path release, `recreateEchoActor()` verifies the same fixed actor name can be reused.
- cross-repo PTC package verifier checks ActorsPage/toggle/status-dot/connector/report/schedule bundle markers and rejects the stale beta28 schedule-disabled text；
- latest public 81 Playwright MCP proof is `G:\PulseTrade.fs\log\20260629\public81-actors-beta41-dyn30.png`；snapshot is `G:\PulseTrade.fs\log\20260629\public81-actors-beta41-dyn30-snapshot.md`；DOM summary is `G:\PulseTrade.fs\log\20260629\public81-actors-beta41-dyn30-dom.json`；
- `SduiFormDocument.fromArguFormSchema` 產生 `schema=fskynet-sdui`、`surface=FormInput`、stable `documentId`；
- PFCF_AKKA_CMD fixture 反射 `SimpleAction`、`BBA`、`GenByColMeta`；
- `GenByColMeta` tuple item kinds 驗證為 `bool-value`, `bool-value`, `text`, `enum`。

### DYN-T-517 Canvas Tree renderer for ActorTreeDocument

Verifier：package focused test plus PTCS browser E2E after PTCS `WBS-054` exists。

Required package coverage：

- `SduiDocument Surface=Canvas` can contain a `Tree` node with `dataRef`, node/parent/label/status field names, `connector=orthogonal`, and `toggle=boxed-plus-minus`；
- DSL decode rejects arbitrary script/action payloads and unknown connector/toggle values with controlled errors；
- node data can represent `id`, `parentId`, `label`, `kind`, `status`, `fullPath`, and optional columns without requiring Dynamic to know Actor Registry storage；
- renderer keeps straight connector geometry and boxed plus/minus toggles, with bounded layout and no text overlap。

Required PTCS integration coverage：

- PTCS Actors tab can convert `ActorTreeDocument` to Dynamic Canvas `Tree` when extension is loaded；
- the same `ActorTreeDocument` renders as PTCS fallback table with `parentId` when Dynamic is absent or renderer fails；
- Dynamic does not own PCSL projection, IndexedDB cache, registry truth source, or report write path。

### DYN-T-502 Argu-to-FormDsl adapter

Verifier：`tests/PulseTrade.Comm.Spa.Dynamic.Tests`。

Current package verifier：`dotnet run --project .\tests\PulseTrade.Comm.Spa.Dynamic.Tests.fsproj -c Release --no-restore -- --summary --no-spinner`，Expecto 15/15 pass。

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

Current package verifier：`dotnet run --project .\tests\PulseTrade.Comm.Spa.Dynamic.Tests.fsproj -c Release --no-restore -- --summary --no-spinner`，Expecto 15/15 pass。

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
- parsed root cases determine rendered FormInput section order；
- field default values are projected into `SduiFormNode.DefaultValues` from parse result / token scan；
- list and named tuple defaults are projected without changing canonical raw Argu tokens；
- unsupported template key、missing arg string、parser failure produce controlled errors via DYN-T-507。

Latest evidence：2026-06-26 `dotnet run --project .\tests\PulseTrade.Comm.Spa.Dynamic.Tests.fsproj -c Release --no-restore -- --summary --no-spinner` passed 14/14。Warnings were existing WebSharper `WS9002` and NuGet `NU5123` long path warnings。

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

Latest evidence：2026-06-26 `dotnet run --project .\tests\PulseTrade.Comm.Spa.Dynamic.Tests.fsproj -c Release --no-restore -- --summary --no-spinner` passed 14/14。

### DYN-T-511 Backend resolver endpoint package gate

Verifier：`tests/PulseTrade.Comm.Spa.Dynamic.Tests`。

覆蓋：

- `DynamicArguResolveEndpoint.handle` accepts JSON `{ keys = [ actorAddress; duTypeOrTemplateKey; canonicalArgString ] }`；
- backend uses registered `DynamicArguTemplateRegistration`, not browser reflection, to parse the canonical arg string；
- response includes actor address、template key、canonical arg string and a FormInput DSL document；
- returned document contains alias/default projection for the PFCF data-range command；
- returned document contains all parsed root sections plus the `DataRange` tail subcommand section；
- full-form raw reconstruction preserves list-inline tokens、root tuple defaults、tail tuple defaults and exact `datarange` tail ordering；
- controlled failure is returned as JSON `Ok=false` instead of silent fallback；
- WebSharper client append renderer compiles with the backend-resolved fetch path and document-backed full-form Send path.

Latest evidence：2026-06-27 `dotnet run --project .\tests\PulseTrade.Comm.Spa.Dynamic.Tests.fsproj -c Release --no-restore -- --summary --no-spinner` passed 15/15 for beta12。The package gate verifies exact canonical enum defaults are present in FormInput values, including `DataRange.ReferenceDateMode.value = ModeAccountingDate`；it also verifies a partial canonical arg string `--pfcfedx trivial --pfcfgtcconf OIInf TAIFEX` renders only `PFCFEDX` and `PFCFGTCCONF` instead of the full DU schema。List-valued Argu fields keep parser-projected defaults on the list node, but list item schema is `text` with no enum options so FormInput presents editable repeatable textboxes, not fixed dropdowns。

### DYN-T-512 Browser E2E for backend-resolved FormInput DSL

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

Latest evidence：2026-06-27 `dotnet fsi --exec G:\PulseTrade.fs\Libs\PulseTrade.Comm\scripts\verify-ptcs-host-dynamic-argu-live.fsx -- --port 0 --extension-dir C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic\src\bin\Release\net10.0` passed with:

```text
ptcsHostDynamicArguLive.ok url=http://127.0.0.1:9711 page=http://127.0.0.1:9711/page/dyn-argu-live81 template=PulseTrade.Comm.Spa.Host.Program+DynamicArguDemo+PFCF_AKKA_CMD submit=echo-verified pcslRoot=G:\PulseTrade.fs.Comm.Log\verification\ptcsHostDynamicArguLive\pcsl runPcslRoot=G:\PulseTrade.fs.Comm.Log\verification\ptcsHostDynamicArguLive\pcsl\run-6e598757065b483fa25b17b4805bfe85 serviceLog=G:\PulseTrade.fs.Comm.Log\verification\ptcsHostDynamicArguLive\verify-ptcs-host-dynamic-argu-live.service.log
```

The gate used PTCS.Host loopback, loaded the Dynamic extension DLL, rendered all parsed PFCF data-range sections, submitted the expected full raw Argu string, and verified DurableProxy echo readback. It also covers the user-facing add-target path: editable DU/template key text input with no datalist/select lock-in, no-target cleanup after removing the last key, generic `actor-argu` add-target UI after removing the demo page, re-created `actor-dynamic` add-target UI after removing the generic page, partial raw command rendering only `PFCFEDX/PFCFGTCCONF`, list-valued `PFCFGTCCONF` as editable textbox rows with Add value/Remove-left, Dynamic `Bind target` label, Canonical Argu string visibility after a valid template key, and canonical input preservation so it cannot regress to `"s"`. PTCS core action pool/tab close/`+ Page` and logical page labels/badges (`Actor Dynamic`/`ad`, `Actor Argu`/`aa`) are covered by the same cross-repo verifier against PTCS beta20. Public 81 deployment is tracked by PTC verification after beta12 packaging.

### DYN-T-513 NuGet bundle/live host gate

PTC-side package verifiers:

```powershell
dotnet fsi --exec G:\PulseTrade.fs\Libs\PulseTrade.Comm\scripts\verify-ptcs-dynamic-nuget-bundle.fsx
dotnet fsi --exec G:\PulseTrade.fs\Libs\PulseTrade.Comm\scripts\run-ptcs-dynamic-nuget-live-host.fsx -- --no-wait
```

Coverage:

- direct `#r` load of `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta13` with `PulseTrade.Comm.Spa 0.2.5-beta21`;
- local nupkg contains `lib/net10.0` DLL and bundled WebSharper JS/min/head assets;
- JS marker contract includes `Bind target`, `Add value`, Remove-left list row classes, and no retired `dynamic-argu-key-du-type-list`;
- live host starts an in-process PTCS + Dynamic server, prints Base/Chat/ActorArgu/Dynamic JS URLs, actor address, template key, web/cluster port, default key/argu, and under `--no-wait` verifies health、HTTP actor-argu send、WebSocket `actor-argu` send、state readback echo reply before calling `stopNuGetLiveHost()`。
- NuGet push accepted `PulseTrade.Comm.Spa.Dynamic 0.1.3-beta13`; follow-up flat-container lookup lists beta13, so public restore availability is confirmed at feed index level.

### DYN-T-520 Actor Dynamic / Actor Argu action mode dispatch

Verifier：package tests plus cross-repo PTC Playwright live-host gate.

Coverage:

- Dynamic add-key renderer claims `actor-dynamic-target`, `actor-dynamic-proxy`, and `actor-argu-target`.
- `Actor Argu` target mode requires actor address + DU/template + canonical arg string.
- `Actor Dynamic` target mode can build DU/FormInput target when DU/template is present.
- `Actor Dynamic` direct actor key is not forced into FormInput when DU/template is blank.

### DYN-T-521 Actor Dynamic direct actor-key canvas route

Verifier：cross-repo PTC Playwright gate.

Required path:

```text
Actor Dynamic page
  -> Add actor key [ actorAddress ]
  -> submit JSON DSL from canvas_demo.json
  -> actor echoes/replies same DSL
  -> Dynamic message renderer renders canvas
```

Non-canvas reply must render through PTCS normal message path.

### DYN-T-522 Actor Dynamic DU/FormInput target

Verifier：existing backend-resolved FormInput gate plus mode-aware action entry.

Coverage:

- `Add target key` uses `actor-dynamic-target`.
- key remains `[ actorAddress; duTypeOrTemplateKey; canonicalArgString ]`.
- FormInput renderer resolves through backend parser and submit still produces exact raw Argu.

### DYN-T-523 Actor Dynamic proxy key builder

Verifier：package test and cross-repo PTC browser gate.

Coverage:

- `Add proxy key` visible only for Actor Dynamic.
- renderer accepts proxy actor address and RN actor address.
- submitted key is `[ proxyActorAddress; "proxy-v1"; rnActorAddress; targetKind ]`.
- first segment is proxy actor address so PTCS actor-argu route can still send to proxy.

### DYN-T-524 Actor Argu no canvas / no proxy

Verifier：cross-repo PTC Playwright gate.

Coverage:

- Actor Argu action pool does not show Add proxy key.
- Actor Argu FormInput route works.
- non-canvas Actor Argu reply does not get converted to canvas.

### DYN-T-525 Canvas payload-only render rule

Verifier：package test or browser gate.

Coverage:

- `DynamicRenderer.TryRender` returns `Some` only for payload with `schema = "fskynet-sdui"`.
- page type / key shape alone cannot force canvas rendering.
