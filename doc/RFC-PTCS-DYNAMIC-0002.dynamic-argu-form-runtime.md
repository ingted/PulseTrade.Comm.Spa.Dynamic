# RFC-PTCS-DYNAMIC-0002 Dynamic Argu Form Runtime

狀態：Draft / Review

日期：2026-06-25

來源文件：

- `doc/REQ_Dynamic_Argu_Form.md`
- `doc/RFC_Dynamic_Argu_Form.md`

關聯上游 / 下游：

- `G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\doc\RFC-PTC-SPA-0007.dynamic-argu-form-extensions.md`
- `G:\PulseTrade.fs\Libs\PulseTrade.Comm\doc\RFC-PTC-0016.resource-node-sharded-function-proxy.md`
- `G:\PulseTrade.fs\Libs\PulseTrade.Comm\doc\WBS.md` rows `PTC3-063`..`PTC3-067`

## 背景

`PulseTrade.Comm.Spa.Dynamic` 已有 FSkynet / SDUI renderer、`fskynet-sdui` message renderer、`actor-dynamic` shape registration 與 `ShowcaseDemoActor` first slice。PTCS core 也已透過 `RFC-PTC-SPA-0006` 提供 client extension manifest、runtime script asset、custom shape 與 message renderer seam。

目前缺口在輸入端：operator 對 `actor-dynamic` / ActorArgu page 仍需手寫 raw textarea 或 raw JSON key。使用者要求 Dynamic package 以 server-driven SDUI 方式，根據 F# DU / Argu-style command metadata 產生 form-style input，並把使用者輸入組成完整 raw Argu args string 送回 PTCS core 既有 append / actor-argu path。

本 RFC 正式化 Dynamic package 端責任。PTCS core 的 extension seam 由 `RFC-PTC-SPA-0007` 定義；PTC/RN generic durable proxy 與 ShardingDelivery 由 `RFC-PTC-0016` 定義。

## 目標

1. 在 Dynamic package 內建立 Argu metadata / schema provider，能從 allowlisted F# DU type 與 union case set 產生 `schema = "fskynet-sdui"` 的 form SDUI JSON。
2. 擴充 Dynamic browser renderer，使 `formMode = "argu-form"` 的 SDUI 可以管理 input state，並支援 `SubmitArguForm` action。
3. 透過 PTCS `RFC-PTC-SPA-0007` 的 append input renderer seam，把 Dynamic form submit 轉為 `AppendInputSubmission.ValueText = complete raw Argu args string`。
4. 透過 PTCS `RFC-PTC-SPA-0007` 的 add-key dialog seam，建立 canonical variable-length key：`actorAddress :: duTypeName :: unionCaseNames`。
5. 保持 Dynamic package 不直接 reference PTC RN package；RN durable proxy 只透過 actor address / `ActorArguTargetCommand.RawArgu` boundary 對接。
6. 建立 Dynamic 自身 verifier 與跨專案 E2E gate，使 `PTC3-067` 能從 concept gate 進入 browser/form/runtime E2E。

## 非目標

1. 不在 Dynamic package 內實作 PTCS core append input registry 或 add-key registry；那是 PTCS `RFC-PTC-SPA-0007` 的責任。
2. 不在 Dynamic package 內實作 RN DurableProxy、Akka ShardingDelivery、ConsumerController confirm discipline、SQL/PCSL result vault provider 或 ProcSupervisor deployment。
3. 不讓 browser renderer 直接寫 PCSL、Akka Journal、MessageFabric、ActorFabric、RN endpoint 或 MCP transport。
4. 不讓使用者從 browser 任意輸入 assembly/type name 後做 unrestricted reflection；type / union case metadata 必須由 Dynamic host allowlist 或 explicit registry 控制。
5. 不把 raw Argu args 當 shell command；它只是 PTCS ActorArgu path 的文字 payload。

## 決策

### D1. Dynamic owns Argu metadata and SDUI form

Dynamic package 擁有：

- DU / Argu command metadata discovery；
- `ArguFormSchemaGenerator`；
- FSkynet SDUI JSON schema for form controls；
- `DynamicRenderer` form state；
- `SubmitArguForm` action；
- add-key guided UI。

PTCS core 只提供 mount point、context、fallback 與 submit callback；PTCS core 不認識 DU reflection、field metadata 或 FSkynet form schema。

### D2. Key convention is variable-length string list

Dynamic Argu Form key 使用：

```text
AppendPageKey.Keys = actorAddress :: duTypeName :: unionCaseNames
```

`unionCaseNames` 是 `string list` tail，不是以 comma、slash、semicolon、pipe 或 JSON string delimiter join 成單一字串。Dynamic owns ordering/canonicalization；PTCS core 只做 non-empty safe string list validation、bucket uniqueness、registry persistence 與 readback。

### D3. Submit path returns complete raw Argu args only

`SubmitArguForm` 收集帶有 `arguParam` metadata 的 input 值，經 Dynamic command-line codec 形成完整 raw Argu args string，例如：

```text
--price 100 --volume 50 --side Buy
```

該字串交給 PTCS append input renderer callback：

```text
AppendInputSubmission.ValueText = "--price 100 --volume 50 --side Buy"
```

PTCS core 再沿用既有 append / actor-argu path 形成 `ActorArguTargetCommand.RawArgu`。Dynamic 不直接呼叫 `ActorArguCore`、不自建 HTTP route，也不 bypass pending command / IndexedDB replay。

### D4. Schema source is explicit registry first

第一波採 explicit registry / allowlist：

```fsharp
type ArguUnionCaseMetadata =
  { DuTypeName: string
    UnionCaseName: string
    DisplayName: string option
    Fields: ArguFormFieldMetadata list }

type ArguFormMetadataRegistry =
  { ActorAddress: string
    DuTypeName: string
    UnionCases: ArguUnionCaseMetadata list }
```

Reflection 可以作為 registry producer，但不得對 browser-supplied type name unrestricted resolve。後續可再支援 runtime actor query / hybrid metadata provider。

### D5. Dynamic can target legacy actor or RN proxy by address

Dynamic add-key UI 的 `actorAddress` 可以是：

- normal PTCS ActorArgu target actor；
- legacy actor proxy；
- PTC RN generic durable proxy actor address。

Dynamic 不需要知道該 address 背後是否是 RN DurableProxy。若使用 RN proxy，資料流由 PTC/RN 負責：

```text
ActorArguTargetCommand.RawArgu
  -> RN CommandToCell / cmd2cell
  -> InvokeLegacy / ivkLegacy
  -> LegacyReplyToCell / legacy2cell
  -> ActorArguTargetReply
```

### D6. Fallback remains mandatory

若 PTCS core 尚未提供 `RFC-PTC-SPA-0007` renderer seam、Dynamic script 載入失敗、schema request 失敗、renderer throw、或 submission invalid，UI 必須退回既有 textarea / raw key flow。Fallback 是 compatibility requirement，不是 success acceptance。

## 預期資料流

### Add key

```text
User clicks Add Key on actor-dynamic page
  -> PTCS core calls registered Dynamic AddKeyDialogRenderer
  -> Dynamic UI selects actorAddress, duTypeName, one or more unionCaseNames
  -> Dynamic returns AddKeyDialogResult.Keys = actorAddress :: duTypeName :: unionCaseNames
  -> PTCS core validates safe string list and writes existing key registry
  -> reload/readback shows the same key list
```

### Render form and submit

```text
User selects Dynamic key
  -> PTCS core builds AppendInputContext
  -> Dynamic AppendInputRenderer CanRender=true
  -> Dynamic resolves allowlisted metadata/schema for actorAddress + duTypeName + unionCaseNames
  -> DynamicRenderer renders schema = "fskynet-sdui", formMode = "argu-form"
  -> User fills fields
  -> SubmitArguForm collects arguParam values
  -> Dynamic command-line codec emits complete raw Argu args string
  -> submitFn { ValueText = rawArgu; Direction = None; Tags = [ "dynamic"; "argu-form" ] }
  -> PTCS core existing append / actor-argu handler
  -> ActorArguTargetCommand.RawArgu
```

### RN durable proxy integration

```text
ActorArguTargetCommand.RawArgu
  -> actor address points to RN DurableProxy target
  -> RN proxy CommandToCell extracts RawArgu
  -> InvokeLegacy sends string/'T to legacy actor/service with replyer
  -> legacy obj reply
  -> LegacyReplyToCell converts reply to fCell2<string>
  -> ActorArguTargetReply is persisted / returned through PTCS history/result readback
```

## Expected code shape

Names may be adjusted during implementation, but responsibilities should remain stable.

Server-side metadata / schema:

```fsharp
type ArguFormFieldKind =
    | Text
    | Integer
    | Decimal
    | Boolean
    | Enum of string list
    | Date
    | Time
    | Color

type ArguFormFieldMetadata =
    { FieldName: string
      ArguParam: string
      Kind: ArguFormFieldKind
      Required: bool
      DefaultValue: string option
      Placeholder: string option }

type ArguUnionCaseMetadata =
    { DuTypeName: string
      UnionCaseName: string
      DisplayName: string option
      Fields: ArguFormFieldMetadata list }

module ArguFormSchemaGenerator =
    val generateSduiJson : ArguUnionCaseMetadata -> string
```

Browser renderer:

```fsharp
type DynamicArguFormRenderContext =
    { ActorAddress: string
      DuTypeName: string
      UnionCaseNames: string list
      Submit: string -> unit }

module DynamicArguFormRenderer =
    val tryRenderAppendInput : obj -> option<Dom.Node>
    val tryRenderAddKeyDialog : obj -> option<Dom.Node>
```

Command-line codec:

```fsharp
module ArguFormCommandLine =
    val encode : (string * string) list -> string
```

The codec must quote values containing whitespace and preserve literal text as data. It must not execute or shell-expand the generated string.

## 跨專案相依排程

| Order | Project | WBS / RFC | Work | Blocks / Enables |
| ---: | --- | --- | --- | --- |
| 1 | PTCS.Dynamic | `RFC-PTCS-DYNAMIC-0002`, `DYN-WBS-401` | 完成 formal RFC flow 與文件鏈 | Enables Dynamic implementation planning |
| 2 | PTCS | `WBS-051B`, `WBS-051C`, `RFC-PTC-SPA-0007` | 實作 append input renderer registry 與 add-key dialog registry，包含 fallback/source gates | Enables Dynamic browser integration without forking `Client.fs` |
| 3 | PTCS.Dynamic | `DYN-WBS-402`, `DYN-WBS-403` | Argu metadata/schema generator、SubmitArguForm state/action codec | Can proceed in parallel with PTCS seam using local shim tests |
| 4 | PTCS.Dynamic | `DYN-WBS-404`, `DYN-WBS-405` | 註冊 Dynamic append input renderer / add-key renderer against PTCS seam | Requires PTCS seam package or project reference |
| 5 | PTCS | `WBS-051D` | Browser + registry gates: form renderer submit, add-key readback, fallback, geometry, built-in regression | Enables cross-project browser proof |
| 6 | PTC RN / RN.Host | `PTC3-063`, `PTC3-065`, `PTC3-066` | Finish ProducerController/region restart redelivery, SQL/PCSL shared-provider service proof, service-window/rolling/soak policy | Enables production-strength `PTC3-067`, not required for first UI-only proof |
| 7 | PTCS.Dynamic + PTCS + PTC RN | `DYN-WBS-406`, `PTC3-067`, `WBS-051E` | End-to-end: form submit -> `ActorArguTargetCommand.RawArgu` -> RN proxy -> legacy reply -> fresh history/result readback | Closes `PTC3-067` browser/form runtime acceptance |

Implementation can overlap where contracts are stable: Dynamic schema generator and browser local renderer tests do not need RN production redelivery; RN redelivery does not need Dynamic UI. The final `PTC3-067` acceptance needs both sides.

## 驗收

1. Dynamic docs: `REQ.md` / `SA.md` / `SD.md` / `WBS.md` / `TEST.md` / `Traceability.md` / `DevLog.md` reference this RFC and source drafts.
2. Dynamic server verifier proves schema generation from allowlisted metadata, safe field mapping, invalid type/case controlled failure, and JSON schema marker `fskynet-sdui`.
3. Dynamic renderer verifier proves `SubmitArguForm` collects state, encodes raw Argu args, and does not submit empty/invalid fields silently.
4. Dynamic browser verifier proves form renderer and add-key renderer work through PTCS `RFC-PTC-SPA-0007` seam, including fallback and reload/readback.
5. Cross-project E2E proves a real Dynamic form submission reaches RN DurableProxy and returns `ActorArguTargetReply` with fresh PTCS readback.
6. All verifiers are F# where practical; Playwright browser verifier can be driven by F# scripts. No fake/mock/internal-only gate may be used as final acceptance.

## 風險

1. Metadata drift: generated forms may not match deployed actor reality. Mitigation: explicit registry version, actor address, DU type name and case set in key/readback.
2. Value quoting: raw Argu string must escape whitespace/quotes predictably. Mitigation: central codec tests.
3. Extension lifecycle: PTCS may render before Dynamic script registers. Mitigation: PTCS seam must re-render or fallback; Dynamic must tolerate late registration.
4. UI density: dynamic form controls can overflow append panel. Mitigation: browser geometry gate on desktop/mobile.
5. Legacy idempotency: RN DurableProxy can provide retry/redelivery, but non-idempotent legacy actor side effects need explicit operation key/result projection policy.

## Open questions

1. First metadata provider should be static registry only, or also support runtime actor query in the same slice?
2. Should Dynamic expose form schema as actor reply payload, local registry lookup, or both?
3. Should `SubmitArguForm` include `Direction` / `Tags` beyond the default `dynamic` / `argu-form` tags in first slice?
4. Which actor address should be the first live `PTC3-067` target: `ttc.pingpong` RN proxy, AOE scout proxy, or a Dynamic showcase actor?
