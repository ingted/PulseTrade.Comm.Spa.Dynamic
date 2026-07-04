# SDUI Developer Manual

本文件維護 PTCS.Dynamic / SDUI / Canvas / FormInput 的 SDK 與開發知識。後續 DSL renderer、前端 Canvas、FormInput、target key lifecycle、extension loading 與 verifier 經驗都應補進此檔。

## 2026-06-26：FormInput target 移除後殘留

現象：

- 進入已部署的 Dynamic Argu page 時，Dynamic extension 與 FormInput 會正常載入。
- 將 target key 移除到最後一個後，左側已顯示沒有 key，但右側 FormInput 還留在畫面上。
- 畫面有時還會出現類似「Dynamic Form document or Argu schema not found」或「沒有找到 dynamic extension / schema」的小字。
- 移除有擴展的 tab page 後，再新增一般 Actor Argu tab page，UI 退回簡單 `Add actor key`，沒有出現完整 `Add target key` / FormInput 流程。

## 根因

這不是單純 PTCS.Dynamic renderer 的 bug。根因在 PTCS core 的 append-page selected target lifecycle：

1. PTCS core 原本把 `AppendPageDefinition.DefaultKey` 同時當成「新增 key 的預設值」與「目前選取的 live target」。
2. 當使用者刪掉最後一個 target key 後，`selectedKeyJson` 會被清空，但 renderer context 又從 `definition.defaultKey` 推回有效 key。
3. PTCS.Dynamic 只看到 core 傳來的 `selectedKeys/keyParts/duTypeName`，因此它合理地嘗試 render FormInput。
4. 如果該 key 對應的 Dynamic schema / document / backend resolver 狀態不完整，就會顯示 schema-not-found 類診斷；如果狀態完整，則看起來像 target 已刪除但 FormInput 還留著。

第二個問題是 generic Actor Argu page 的 add-key UX：

- Dynamic extension 的 add-key renderer 需要能建立 `[ actorAddress; duTypeOrTemplateKey; canonicalArgString ]` target。
- Host demo page 的 `defaultKey` 內有 actor address，所以早期 UI 可以偷用 defaultKey 第一段當 actor address。
- 一般 Actor Argu page 沒有這種 Host demo defaultKey，因此 add-key renderer 不應要求 defaultKey 必含 actor address，而要提供 actor address input。

## 為什麼不是只改 PTCS.Dynamic

PTCS.Dynamic 是 extension renderer，不擁有以下 core state：

- 哪個 append-page target 目前被選取；
- target key 是否已被使用者移除；
- key-registry live/tail event 是否是舊 event；
- 沒有 target 時是否應該呼叫 append-input renderer；
- `DefaultKey` 是否能視為 live selected key。

因此如果 PTCS core 在 no-target 狀態仍傳入 `DefaultKey`，PTCS.Dynamic 無法可靠判斷這是使用者真的選到 target，還是 core fallback 出來的假 selection。正確邊界是：

- PTCS core：負責 selected target lifecycle、key-registry replay、append renderer 是否被呼叫。
- PTCS.Dynamic：只在收到真實 selected target 或 add-target context 時 render Canvas / FormInput / Dynamic target builder。

## 為什麼後端已 `useDynamicSdui` 仍會失效

`hub.useDynamicSdui(...)` 只表示 extension 後端已註冊：

- client extension manifest；
- WebSharper client bundle；
- Dynamic metadata / Form DSL / Argu template registration；
- backend resolver endpoint，例如 `/client-extensions/dynamic/argu/resolve-target`。

它不代表 PTCS browser core 的 selected target lifecycle 一定正確。也就是說：

- extension loaded：renderer 與 resolver 可用；
- target selected：PTCS core 必須正確指出目前有一個有效 target；
- no target：PTCS core 必須傳達「沒有 target」，而不是 fallback 到 `DefaultKey`。

這三件事分屬不同層。`useDynamicSdui` 解決 extension availability，不解決 append-page target state。

## 修正原則

PTCS core：

- `DefaultKey` 只能用於新增 target 的初始輸入，不可在 no-target 狀態當成 selected target。
- 刪除最後 target 後，append form 的 renderer context 應呈現 `selected-key-source=none`，且不呼叫 Dynamic FormInput renderer。
- client 需記住本輪已 hidden 的 key id，避免 key-registry tail / live replay 把剛移除的舊 key 又加回來。
- 重新 add 同 key 時，才解除本機 hidden marker。

PTCS.Dynamic：

- add-target renderer 必須支援 generic `actor-argu` page。
- actor address 應是明確輸入欄位，不應只從 Host demo `DefaultKey` 取值。
- 對 `[ actorAddress; duTypeOrTemplateKey; canonicalArgString ]` target 走 backend resolver，取得 Form DSL 後 render FormInput。

## Verifier 規則

UI verifier 應用 F# + Playwright native API 驅動：

- `Locator.FillAsync`
- `Locator.SelectOptionAsync`
- `Locator.GetAttributeAsync`
- `Locator.TextContentAsync`
- `Locator.CountAsync`

不要在 verifier 中寫 inline JavaScript 操作 DOM，例如 `document.querySelector(...)`、`dispatchEvent(...)`、`WaitForFunction("() => ...")`。如需等待狀態，使用 F# polling 加 Playwright locator API。

最低 regression gate：

1. 開啟 Dynamic demo page，確認 FormInput 出現。
2. 移除最後一個 target key。
3. 確認 `append-form` 進入 no-target 狀態，FormInput 消失，且沒有 schema-not-found 訊息。
4. 移除 Dynamic demo tab page。
5. 新增 generic `actor-argu` page。
6. 確認 Dynamic add-target UI 出現。
7. 以 actor address、DU/template key、canonical arg string 新增 target。
8. 確認 FormInput 重新出現並能走 submit path。

## 2026-06-26：Arg-string target 必須真的 parse

現象：

- 使用者在 add-target UI 輸入 partial canonical arg string，例如 `--pfcfedx trivial --pfcfgtcconf OIInf TAIFEX`。
- FormInput 仍渲染完整 PFCF 表單，像是 `TO`、`BBA`、`DataRange` 等未出現在 raw command 的 union cases 也全部出現。
- DU/template key 欄位在只有一個 template 時被 render 成不可編輯的文字，無法改成其他 type string。
- 移除 page 後重新新增 target 時，Canonical Argu string 可能被污染成單字 `"s"`，導致 resolver 和 FormInput 都錯。

正確語意：

1. `[ actorAddress; duTypeOrTemplateKey; canonicalArgString ]` 的第三段不是展示用範本字串，而是 Dynamic backend 的 parser input。
2. Dynamic backend 必須用該 template 的 registered Argu parser parse `canonicalArgString`。
3. Form DSL 只應包含 parse result 中出現的 root cases / subcommand cases。
4. 對 `--pfcfedx trivial --pfcfgtcconf OIInf TAIFEX`，FormInput 只應渲染 `PFCFEDX.mode` 與 appendable `PFCFGTCCONF.value`。
5. 未出現在 parse result 的 cases，例如 `TO`、`BBA`、`DataRange`，不得出現在這個 partial target 的 FormInput。

修正原則：

- backend：`DynamicArgStringTarget.scan` / `DynamicFormDsl.filterSchemaByParsedRootCases` 是 canonical path；client 不能自行用 full schema 假裝已解析。
- add-key UI：DU/template key 一律是可編輯 text input + datalist。即使 registry 只有一個 template，也不能 render 成 `<code>` 或不可變 text。
- canonical raw input：移除 key、移除 page、重新新增 page 後，都必須保留使用者輸入的完整字串；不能被 keypress / state replay 污染成 `"s"` 或其他單字。
- verifier：要同時測 full command 和 partial command。partial command 必須 assert rendered case list 精準等於 parsed cases，並確認未解析 cases count 為 0。

本輪 evidence：

- `DYN-T-512` package gate 新增 partial raw resolver case；`PulseTrade.Comm.Spa.Dynamic.Tests` 15/15 passed。
- `G:\PulseTrade.fs\Libs\PulseTrade.Comm\scripts\verify-ptcs-host-dynamic-argu-live.fsx` 新增 Playwright gate：full PFCF data-range command、partial command、editable DU/template key、remove target -> no-target -> add partial target、remove page -> create generic `actor-argu` page -> add partial target，以及 submit 前 canonical input value 必須等於 typed raw string。

## 2026-06-27：Argu list 欄位不是 dropdown

現象：

- `PFCFGTCCONF` / `PFCFGTC` 這類 union case 在 Argu type 上是 `'T list`。
- 早期 renderer 因為 list item type 有 enum/options metadata，把每個 list item render 成 dropdown。
- 這會把 parser-projected default values 誤解成固定選項，使用者無法自由輸入其他值，也無法只把 default 當初始值再增刪項目。

正確語意：

1. Argu `'T list` 是 repeatable value group。
2. Backend parser 投影出來的 values 只是初始值，不是 UI lock-in。
3. Browser FormInput 應 render 為多列 textbox。
4. Add 只新增一列空 textbox。
5. 每一列都要有 Remove，移除後 rebuilt raw Argu 不可包含該值。
6. 如果 list item type 同時有 enum/options metadata，該 metadata 只能輔助未來 autocomplete / validation；在目前 FormInput UX 中不得轉成 select。
7. Backend DSL 也必須反映這個規則：list item node/schema 應是 `Text` / `text` 且 options 為空。不能把 `valueItem kind=Select` 留在 list node 裡，否則未來 renderer 會再次誤判成 dropdown。

Current regression gate：

- `G:\PulseTrade.fs\Libs\PulseTrade.Comm\scripts\verify-ptcs-host-dynamic-argu-live.fsx` 確認 `PFCFGTCCONF` list 容器沒有 `select`。
- 同一 gate 確認預設兩列是 `OIInf`、`TAIFEX` text input；Add 產生空 input；填入 `ManualEntry` 後 preview 追加該 token；Remove 後 preview 移除該 token。
- `DYN-T-511` package gate 與 live 81 resolver check 確認 `PFCFGTCCONF.valueItem kind=text/options=0`，list node default values 仍保留 `OIInf,TAIFEX`。

## 2026-06-27：Compact shell actions 與 Dynamic target binding 邊界

本輪 UI redesign 將 append-page 操作分成兩層：

1. PTCS core owns page lifecycle shell：
   - tab pill 上的 close `x`；
   - topbar 的 compact `+ Page`；
   - sidebar 的 `Actions` details pool，收納 Add / Remove / Reload / Remove page 這類 page/target shell action。
2. PTCS.Dynamic owns Dynamic target binding and FormInput：
   - add-target submit label 使用 `Bind target`；
   - actor address、DU/template key、canonical Argu string 是 Dynamic target builder 的輸入；
   - FormInput 裡的 repeatable list 使用 `Add value` 與 Remove-left textbox rows。

沒有 `hub.useDynamicSdui(...)` 時：

- PTCS core 仍應顯示自己的 page add/remove/reload 與 fallback raw target UI；
- Dynamic FormInput / `Bind target` renderer 不應出現；
- `AppendPageDefinition.DefaultKey` 仍只可作 add-target seed，不可在 no-target 狀態 fallback 成 live selection。

有 `hub.useDynamicSdui(...)` 時：

- PTCS core 只提供 extension registry、safe JSON resolver route、page shell 與 command path；
- Dynamic renderer 透過 backend resolver 將 `[ actorAddress; duTypeOrTemplateKey; canonicalArgString ]` 轉成 FormInput DSL；
- target submit 由 Dynamic renderer 產出 key list 後交回 PTCS core append-page key registry。

Current regression gate：

- `G:\PulseTrade.fs\Libs\PulseTrade.Comm\scripts\verify-ptcs-host-dynamic-argu-live.fsx` 以 F# Playwright native locator API 驗證 PTCS action pool/tab close/`+ Page`、Dynamic `Bind target`、Remove-left list rows 與 exact PFCF echo。
- `G:\PulseTrade.fs\Libs\PulseTrade.Comm\scripts\run-ptcs-dynamic-nuget-live-host.fsx -- --no-wait` 直接 `#r` PTCS beta19 / Dynamic beta11，啟動 live host 並印出 URL、actor address、template key、probe status，供人工測試前快速確認 NuGet bundle 真能啟動。

## 2026-06-28：ActorsPage renderer 與 WebSharper 限制

PTCS `/actors` 的 final seam 不是 generic Canvas message renderer，而是 page-level `ActorsPage` renderer：

- PTCS core 產生 `schema=fskynet-sdui` / `surface=ActorsPage` / `documentType=ActorTopologyPage` payload。
- Dynamic extension 透過 `PulseTradeRegisterPageRenderer` 註冊 `string -> Dom.Node option` renderer。
- renderer 回 `Some node` 時，PTCS 只 mount Dynamic page host；fallback tree/table 不同時出現。
- renderer 缺席、回 `None` 或 throw 時，PTCS 使用 core fallback tree/table。

本輪實作限制：

1. 不要新增 `Client/ActorsPageRenderer.fs` 這類新的 `[<JavaScript>]` compile unit。此 repo + WebSharper 10.1.5.674 下，即使 no-op compile unit 也會讓 `wsfsc.exe` crash。
2. 不要在 `[<JavaScript>]` client code 使用 `String.Contains`。已確認單一 `Contains` 會 crash。
3. 不要使用多段 `IndexOf` chained predicate。已確認多 token predicate 會 crash。
4. first slice 將 renderer 放在既有 `Client/ActorDynamicTab.fs`，classifier 使用單一 `rawContent.IndexOf("ActorTopologyPage") >= 0`。
5. strict `schema/surface/documentType` parser、clean node grouping、role ordering、report actions、restart/failover status 都列入 `DYN-WBS-519` 後續 gate；不能把 first-slice UI 誤當 final renderer。

後續實作補充：

- PTCS server extension bootstrap 與 SPA client bootstrap 都需要建立 `PulseTrade.PageRenderers` / `PulseTradeRegisterPageRenderer`。Dynamic script 可能早於 SPA bundle 載入，只在 client bootstrap 建 registry 會讓 first registration miss 掉。
- WebSharper dynamic call `JS.Window?PulseTradeRegisterPageRenderer("name", 100, renderer)` 會產生不符合預期的 array argument call。這個 interop 點目前必須使用既有 inline bridge 呼叫 `window.PulseTradeRegisterPageRenderer('ptcs-actors-page', 100, renderer)`。
- PTCS `/actors` 目前會先查 `PageRenderers`，再以 `MessageRenderers` 作 transitional compatibility fallback。Dynamic generic `fskynet-sdui` renderer 必須先判斷 `ActorTopologyPage` 並回 ActorsPage DOM，否則會被普通 Canvas summary renderer claim。
- `C:\ptcsdyn-build\bin` 可能保留舊 bundle。Source-host verifier 應優先 `#I` Dynamic source Release output，例如 `C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic\src\bin\Release\net10.0`；若使用短路徑 build 生成 bundle，需同步 DLL/JS 回 source Release output，確保 FSI `#r` 載入同一版。
- 若 served `PulseTrade.Comm.Spa.Dynamic.js` 沒有 `createActorsPageDocument`、`dynamic-actor-node-block` 或 `dynamic-actor-tree-toggle` marker，先停止 stale `wsfscservice.exe`，再走 `DYN-VFY-001` 短路徑 build；不要把 stale bundle 當 renderer 行為判斷。
- ActorsPage source-host fixture 必須投影 distinct `nodeId` / `role`。若 PTCS/GW/RN sample events 共用同一 `nodeId`，PTCS `ActorsSnapshot` 會把所有 actor 合併成單一 node，最後一筆事件的 node address 會覆蓋前面資料，導致 Dynamic 端無法驗證不同 port 的分區。
- Tree toggle 必須是 stateful renderer，不是文字裝飾。驗證至少要檢查 `data-testid="dynamic-actor-tree-toggle"`、`aria-expanded`、`+/-` 文字與 row count 前後變化。
- Dynamic accepted page ownership 需要 PTCS core 配合。2026-06-29 beta40 修正前，即使 Dynamic renderer 回 `Some node`，core snapshot repair 仍可能把 `data-testid="actor-node"` / `actor-card` fallback cards append 到 Dynamic page 底下。這不是 PTCS.Dynamic 單獨可修的 renderer bug；PTCS core 必須在 accepted Dynamic page 時停止更新 fallback `nodes` container。verifier 需同時檢查 `dynamic-actors-page` 存在、fallback rows 為 0、core cards 為 0。
- ActorsPage row metadata 是後續 UI/測試穩定性的契約：virtual parents 使用 `data-node-kind="virtual-path"`，row 需提供 `data-display-address` 與 `data-parent-id`。不要用畫面文字截斷或 CSS hierarchy 當唯一判斷來源。

驗證順序：

1. 先跑 DYN-VFY-001 short-path full WebSharper build。
2. 再跑 DYN-VFY-004 tests，並加 `-p:WebSharperRunCompiler=false`，避免 test build 重跑 long/default path compiler。
3. UI milestone 需由 PTCS/PTC cross-repo Playwright verifier 或 Playwright MCP 驗證 `/actors` 實頁：Dynamic accepted 時 fallback DOM 不 mount，Dynamic absent 時 fallback tree/table 可用。

## 2026-06-27：DU/template key 是自由 full type name input

現象：

- Dynamic add-target UI 曾用 registry keys 產生 datalist/select-like 行為。

## 2026-06-27：Logical page type display 與 hub registry/page/target 邊界

`hub.useDynamicSdui(actorSystem, metadata, templateRegistrations)` 是 hub-level extension registry，不是 tab page instance binding。

- `DynamicArguMetadata` / `templateRegistrations` 是全域 catalog，提供所有 tab page 可用的 DSL document、DU/template key 與 backend resolver registration。
- 單一 tab page 是否使用 Dynamic FormInput 由 page shape/tag 決定，例如 `actor-dynamic` 或帶 `actor-argu` tag 的 page。
- 單一 target 的實際 schema binding 由 target key 決定：`[ actorAddress; duTypeOrTemplateKey; canonicalArgString ]`。
- 沒有 selected target 時，PTCS core 應傳 `selected-key-source=none`，Dynamic renderer 不應用 `DefaultKey` 假裝使用者選到 target。

這表示多個 tab page 共用同一份 registry 不會混亂；真正決定 FormInput 的是每個 target key 第二段 template/DU key 與第三段 canonical arg string。

本輪發現的錯誤 page type 顯示不是 PTCS.Dynamic renderer 根因。PTCS core 使用 physical append-page shape 作 user-facing label，導致 ActorArgu/Dynamic page 顯示 `fcell-chat`。修正屬 PTCS core：

- `actor-dynamic` 顯示 `Actor Dynamic` / `ad`；
- generic `actor-argu` 顯示 `Actor Argu` / `aa`；
- physical `fcell-chat` 只代表 storage/render fallback，不應作為 ActorArgu/Dynamic page 的 operator-facing type；
- `verify-ptcs-host-dynamic-argu-live.fsx` 會在 default page、remove/re-add `actor-argu`、remove/re-add `actor-dynamic` 三條路徑驗證 label/badge 與 Canonical Argu string。
- 在只有一個 host demo template 時，developer 會感覺 type string 被固定，無法自行輸入 full type name 或未來 template key。

正確語意：

1. Target key 第二段是 `duTypeOrTemplateKey`，對 developer 而言必須是自由字串欄位。
2. Host 可以提供 placeholder/default，但不能用 select/datalist 鎖住候選。
3. Resolver 失敗時由 backend 回 controlled error；frontend 不應偷偷 fallback 成 textarea 或把錯誤 key 改成 registry default。

Current regression gate：

- `verify-ptcs-host-dynamic-argu-live.fsx` 使用 Playwright native locator 驗證 `data-testid="dynamic-argu-key-du-type"` 是 input，且頁面不存在 `dynamic-argu-key-du-type-list` datalist 或 select。
- Gate 以手動填入的 DU/template key 新增 target，確保 add-target path 不依賴 Host demo registry auto-select。

## 2026-06-28：Actor Dynamic / Actor Argu action modes

使用者確認最新切分：

- `Actor Argu` 固定 FormInput / Argu command，不支援 canvas，不支援 Add proxy key。
- `Actor Dynamic` 支援 direct actor key、DU/FormInput target key、live proxy key；reply 是 `schema=fskynet-sdui` JSON DSL 才畫 canvas。

PTCS core action shell 會傳 mode-aware shape：

```text
actor-argu-target
actor-dynamic-target
actor-dynamic-proxy
```

Dynamic renderer 規則：

- `actor-argu-target`：必須輸入 actor address、DU/template key、canonical arg string。
- `actor-dynamic-target`：有 DU/template + canonical arg string 時使用 FormInput；無 DU 時應交回 PTCS fallback direct actor-key path。
- `actor-dynamic-proxy`：輸入 PTCS Host proxy actor address 與 RN actor address，產生 `[proxyActorAddress; "proxy-v1"; rnActorAddress; targetKind]`；實際 payload 由 append input value 承載。PTCS core 只會把第一段 `proxyActorAddress` 放進 `ActorArguTargetCommand.ActorAddress`，不會把 `rnActorAddress` 自動帶給 proxy。Current no-core-change rule 是 one target key -> one proxy actor/spec；`rnActorAddress` 只作 binding/diagnostic，真正 native/RN target 必須由 proxy actor/spec 建構時固定。
- 單段 Dynamic key `[actorAddress]` 的 append input 應由 PTCS fallback textarea 處理，讓使用者可直接輸入 canvas JSON DSL。
- Dynamic message renderer 只能依 payload schema 判斷 canvas，不可依 page type 或 key shape 強制 canvas。
