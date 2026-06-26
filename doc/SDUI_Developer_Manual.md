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
