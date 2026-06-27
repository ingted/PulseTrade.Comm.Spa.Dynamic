# RFC-PTCS-DYNAMIC-0003 Unified SDUI / Form DSL Roadmap

狀態：Draft / Review

日期：2026-06-26

關聯文件：

- `C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic\doc\RFC-PTCS-DYNAMIC-0002.dynamic-argu-form-runtime.md`
- `C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic\doc\SDUI_DSL_zh-Hant.md`
- `G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\doc\RFC-PTC-SPA-0008.unified-sdui-target-extension-contract.md`
- `C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic\doc\example DU.txt`

## 背景

2026-06-26 的 Dynamic Argu Form first implementation 暴露出產品邊界錯置：

1. `PulseTrade.Comm.Spa.Dynamic` 應是通用 NuGet package，但實作曾把 demo schema / sample DU 意圖帶進 package。
2. Form renderer 曾直接吃 Argu / DU schema，讓 renderer 和 adapter 邏輯混在一起。
3. Append form UI 曾以 union case dropdown 切換輸入，違反「全部 union case 同屏可見、方便輸入」的產品需求。
4. Add target key 曾同時顯示 raw JSON key、actor address textbox、filter textbox 與 unclear dropdown，造成 UI 語意混亂。
5. Canvas DSL 與 Form Input DSL 本質上都是 SDUI，只是 render surface 與 interaction model 不同，應收斂成同一套 DSL 與不同 renderer。

因此本 RFC 取代 `RFC-PTCS-DYNAMIC-0002` 中「Argu form schema 是主要 payload」的設計方向。Argu / DU 只是一種 metadata producer；canonical product artifact 必須是通用 SDUI Form DSL。

## 目標

1. 將 PTCS.Dynamic 定位為通用 NuGet package，提供 SDUI DSL、Canvas renderer、Form Input renderer、Argu-to-FormDsl adapter 與 extension registration。
2. 將 Canvas 與 Form Input 收斂為同一套 `fskynet-sdui` document model，由 render surface 決定 readonly canvas、可操作 canvas 或 stateful form。
3. 支援 Form Input 需要的 state、validation、submit、backend-linked option provider 與 WebSocket / same-origin callback interaction。
4. 支援兩種 target binding：
   - actor key 綁已註冊 DSL target。
   - actor key 綁 DU type / template key + canonical Argu command string，由 PTCS.Dynamic backend 使用該 DU parser parse，再依 parse result 轉為 DSL target。
5. PTCS.Host 才放 PulseTrade 自有 demo DU 與部署 wiring；PTCS.Dynamic package 不含 `SampleArgu` 或業務專屬 DU。

## 非目標

1. 不把 PTCS.Dynamic 變成 PTCS.Host 專案的一部分。
2. 不讓 PTCS core 反射 DU、解析 Argu、或渲染 FSkynet widgets。
3. 不在 browser 端 unrestricted resolve arbitrary assembly/type name。
4. 不以 JavaScript 手寫 renderer 邏輯；Dynamic browser renderer 仍使用 F# / WebSharper。
5. 不把 fake/mock/internal-only proof 當成產品驗收。

## 產品邊界

| Package / Host | Ownership |
| --- | --- |
| `PulseTrade.Comm.Spa` (PTCS) | append page、key registry、command path、pending replay、extension seam、same-origin safe callback shell。 |
| `PulseTrade.Comm.Spa.Dynamic` (PTCS.Dynamic) | SDUI DSL types、Canvas/Form renderer、Form state、Argu parser-backed adapter、target resolver、alias binding、client extension registration。 |
| `PulseTrade.Comm.Spa.Host` (PTCS.Host) | PulseTrade deployment、extension DLL loading、demo DU、demo actor/proxy、public 81/443 service wiring。 |
| PTC RN / RN.Host | durable proxy、CommandToCell / InvokeLegacy / LegacyReplyToCell、delivery/retry/result completion。 |

## 決策

### D1. Canonical artifact is SDUI document, not Argu schema

Argu / DU metadata 只產生 `FormDslDocument`；renderer 不直接依賴 `IArgParserTemplate`、DU union cases 或 Argu reflection。

```fsharp
type SduiSchema =
    | FskynetSdui

type SduiRenderSurface =
    | Canvas
    | FormInput

type SduiDocument =
    { Schema: SduiSchema
      Version: string
      DocumentId: string
      Surface: SduiRenderSurface
      Data: Map<string, obj>
      Nodes: SduiNode list
      Actions: Map<string, SduiAction>
      Bindings: SduiBinding list }
```

### D2. Canvas and FormInput share nodes, actions and bindings

Canvas renderer uses the same `SduiNode` tree as FormInput renderer. The difference is lifecycle:

- Canvas: usually readonly, may support local UI manipulation such as toggle, mode switch, local sorting, open panel。
- FormInput: stateful, validates inputs, can query backend-linked options, and submits into PTCS append / actor-argu command path。

```fsharp
type SduiInputKind =
    | Text
    | Number
    | Bool
    | Enum
    | Tuple
    | List
    | Date
    | Time
    | Color

type SduiNode =
    | Stack of id: string * children: SduiNode list
    | Section of id: string * title: string option * children: SduiNode list
    | TextBlock of id: string * text: string
    | DataGrid of id: string * dataRef: string
    | Tree of id: string * dataRef: string * nodeIdField: string * parentIdField: string * labelField: string * statusField: string
    | Input of id: string * label: string * kind: SduiInputKind * binding: string
    | Select of id: string * label: string * options: SduiOptionSource * binding: string
    | Button of id: string * label: string * actionId: string
```

Canvas `Tree` is a first-class shared node. It is required by PTC Actor Registry / PTCS Actors tab integration: PTCS produces `ActorTreeDocument`, while Dynamic renders it through Canvas `Tree` with orthogonal connectors and boxed `+` / `-` toggles. Dynamic must not own Actor Registry truth source, PCSL projection, IndexedDB cache, or report generation; PTCS core remains responsible for fallback table rendering when Dynamic is absent.

### D3. Backend-linked inputs use declared option sources

Dynamic FormInput may need dropdown B to depend on dropdown A. This must be explicit in DSL and must use a registered provider, not arbitrary browser URL.

```fsharp
type SduiOptionSource =
    | StaticOptions of SduiOption list
    | QueryOptions of providerId: string * dependsOn: string list
    | StreamOptions of streamId: string * dependsOn: string list

type SduiAction =
    | LocalState of targetId: string * patch: string
    | QueryOptions of providerId: string * inputs: string list * outputBinding: string
    | SubmitForm of targetBindingId: string * includeStateOf: string list
```

PTCS core may provide a safe same-origin callback shell, but provider implementation belongs to the host / extension registration, not to PTCS core。

### D4. Target key has two canonical shapes

`AppendPageKey.Keys` remains `string list` and first item is always actor address.

Direct DSL target:

```text
[ actorAddress; formDslId ]
```

DU / Argu target:

```text
[ actorAddress; duTypeOrTemplateKey; canonicalArgString ]
```

The third segment is a canonical Argu command string for the registered DU / template. It is data, not a shell command. PTCS.Dynamic backend parses it with the registered `ArgumentParser<'Template>` and derives the FormInput DSL from the semantic parse result plus original token order.

No `1:duType:` / `2:unionCases:` prefix is part of the new canonical key. The prior `[ actorAddress; duTypeName; case1; ... ]` shape is legacy first-slice behavior and must not be used for new UX.

PTCS core stores and replays the list without understanding segments after `actorAddress`. If `hub.useDynamicSdui(...)` is not registered, PTCS built-in actor-argu behavior uses only `actorAddress` as the actor key and falls back to its normal textarea/raw path. If Dynamic is present, PTCS.Dynamic resolves the second segment by checking registered DSL targets first, then registered DU/template adapters. If the second segment matches neither registry, Dynamic shows a controlled error and does not silently fallback to textarea.

### D5. Alias binding belongs to Dynamic backend metadata

F# DU union cases and fields are often English/canonical, while FormInput labels need domain-localized display text. Alias is display metadata only; submit and raw command building always use canonical Argu names.

```fsharp
type DynamicArguAliasBinding =
    { CaseAliases: Map<string, string>
      FieldAliases: Map<string * string, string>
      OptionAliases: Map<string * string, string> }

type DynamicArguTemplateRegistration =
    { TemplateKey: string
      DuTypeName: string
      ParserTemplateType: Type
      Aliases: DynamicArguAliasBinding }
```

Alias precedence:

1. Host/template registration alias map.
2. Attribute/resource metadata if supported later.
3. Canonical union case / field / option name.

Target key does not carry alias pairs in the canonical design. A future import/export UX may serialize alias override metadata separately, but not in `AppendPageKey.Keys`.

### D6. Argu adapter parses canonical arg string before emitting DSL

Argu adapter output must show all parsed root union cases and supported subcommands concurrently. It must not use a union case dropdown for the primary interaction.

```fsharp
type ArguTemplateBinding =
    { DuTypeName: string
      TemplateKey: string
      CanonicalArgString: string
      ParserTemplateType: Type
      Aliases: DynamicArguAliasBinding }

module ArguToFormDsl =
    val generate : ArguTemplateBinding -> SduiDocument
```

The backend does two passes:

1. Parse `CanonicalArgString` with the registered Argu parser to validate semantics and determine selected union cases / subcommands / default values.
2. Token-scan the original command string to preserve stable UI ordering and rebuild ordering.

Generated form shape:

```text
Form document
  Section "Say"
    Text input "text"
    Send button for "Say"
  Section "Set_Count"
    Number input "count"
    Send button for "Set_Count"
  Section "PFCFGTC"
    List input of enum PFCFGTC
  Section "DataRange"
    Nested section for PFCF_AKKA_CMD_DATA_RANGE
  Send button for the composite target command
```

`ParseResults<'T>` / `ArgumentType.SubCommand` is represented as a tail subcommand group. Raw command rebuild rules:

- root-level args preserve configured/token order；
- tail subcommand token is emitted after all root-level args；
- subcommand args are emitted after the subcommand token；
- example: `... --round 6 4 2 datarange --referencedatemode ModeAccountingDate --between 20251104 20251104`。

### D7. Submit returns complete raw Argu only at the adapter boundary

The FormInput renderer submits a structured form state. The Argu adapter codec converts that state to raw Argu string at the boundary.

```text
Form state
  -> adapter codec
  -> raw Argu string
  -> AppendInputSubmission.ValueText
  -> PTCS ActorArguTargetCommand.RawArgu
```

The raw Argu string is data, not a shell command.

### D8. PTCS.Host owns demo DU

`C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic\doc\example DU.txt` is Big5/cp950 encoded source material. PTCS.Host demo should define a compilable host-local subset based on it:

- `PFCF_AKKA_CMD`
- `PFCF_AKKA_CMD_DATA_RANGE`
- `PFCF_AKKA_CMD_GM`
- `PFCFGTC`
- `PFCF_GTC_CONF`
- `PFCF_AKKA_CMD_BANK_EDX`
- necessary enum / missing type stubs, for example a host-local `DemoRtTable` if `DataTypeT.RTTables` is unavailable

The demo DU is not part of PTCS.Dynamic package API.

## 預期資料流

### Extension present

```text
PTCS.Host starts
  -> loads PTCS.Dynamic extension DLL if configured/present
  -> PTCS.Dynamic registers script asset, shape metadata, renderers, DSL registry, adapter registry
  -> PTCS.Host registers its demo DSL / demo DU target metadata
```

### Extension absent

```text
PTCS.Host starts without Dynamic DLL
  -> PTCS built-in chat/append/actor-argu behavior stays unchanged
  -> no canvas/form input custom renderer appears
```

### Actor key bound to DSL target

```text
Add target
  -> Dynamic add-key renderer picks actorAddress + formDslId
  -> PTCS stores [ actorAddress; formDslId ]
Selected key
  -> Dynamic resolves formDslId to SduiDocument
  -> FormInput renderer renders document
Submit
  -> document action maps state to ValueText
  -> PTCS existing append / actor-argu path
```

### Actor key bound to DU/template arg string

```text
Add target
  -> Dynamic add-key renderer picks actorAddress + duTypeOrTemplateKey + canonical Argu command string
  -> PTCS stores [ actorAddress; duTypeOrTemplateKey; canonicalArgString ]
Selected key
  -> PTCS.Dynamic backend parses canonicalArgString with registered IArgParserTemplate parser
  -> Dynamic Argu adapter generates SduiDocument from parse result and alias binding
  -> FormInput renderer shows parsed root cases and supported subcommands as visible sections
Submit
  -> Argu codec emits complete raw Argu args, preserving tail subcommand ordering
  -> PTCS ActorArguTargetCommand.RawArgu
```

## 影響範圍

| Area | Change |
| --- | --- |
| `doc/SDUI_DSL_zh-Hant.md` | Promote common document/node/action/binding DSL and define Canvas/Form surfaces。 |
| `Server/ArguForm.fs` | Refactor from direct Argu schema to parser-backed Argu-to-FormDsl adapter with alias binding and subcommand groups。 |
| `Client/ArguFormRenderer.fs` | Rename conceptually to FormInput renderer; renderer reads backend-resolved DSL, not DU schema。 |
| PTCS renderer context | Keep selected key, shape, callbacks; may add safe extension query callback for option providers。 |
| PTCS.Host | Register demo DU and demo target metadata based on `example DU.txt`。 |

## 測試要求

1. `DYN-T-501` DSL parser/serializer: validates Canvas and FormInput documents use the same document model.
2. `DYN-T-502` Argu adapter: converts registered `IArgParserTemplate` parse result into Form DSL sections, all selected cases/subcommands visible at once.
3. `DYN-T-503` target resolver: `[actor; formDslId]` and `[actor; duTypeOrTemplateKey; canonicalArgString]` both resolve; unknown target or parse failure shows controlled error.
4. `DYN-T-504` backend-linked options: dropdown dependency can query a registered provider without arbitrary URL execution.
5. `DYN-T-505` browser E2E: actor key bound to DSL target submits through PTCS existing append / actor-argu path.
6. `DYN-T-506` browser E2E: actor key bound to DU/template + canonical arg string renders parsed form and submits raw Argu through RN durable proxy path.
7. `DYN-T-507` subcommand command builder: verifies `ParseResults<PFCF_AKKA_CMD_DATA_RANGE>` can rebuild a composite command whose `datarange` token stays after root args and before subcommand args.

## Open Questions

1. PTCS core extension query callback first slice 是否只支援 option provider，或同時支援 form DSL resolve provider？
2. Direct DSL target 的 `formDslId` 是否需要 version segment，例如 `[actorAddress; formDslId; version]`，或由 registry metadata 管理？
3. PTCS.Host demo 是否要完整承載 `example DU.txt` 的全部 cases，或先用可編譯 subset 做 E2E，再逐步補齊？
4. Canonical arg string 的輸入 UI 是否需要提供 template preset picker，或先允許 host 以 registered default string 提供？
