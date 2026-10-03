# System Design (SD) - PulseTrade.Comm.Spa.Dynamic

## 1. 專案結構設計
```text
C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic
├── doc
│   ├── REQ.md, SA.md, SD.md, WBS.md, TEST.md, UPSTREAM_RFC.md
├── src
│   ├── PulseTrade.Comm.Spa.Dynamic.fsproj
│   ├── Server
│   │   ├── FCell2Interop.fs   (負責 fCell AST 解析與轉換)
│   │   ├── DynamicActors.fs   (包含 Actor Dynamic 的後端邏輯)
│   │   └── Extension.fs       (包含 CommHub 擴充與掛載介面)
│   └── Client
│       ├── DynamicRenderer.fs (WebSharper 渲染器與 DOM 生成邏輯)
│       └── ActorDynamicTab.fs (處理 "Actor Dynamic" Tab Page 類型的頁面渲染)
└── tests
    ├── PulseTrade.Comm.Spa.Dynamic.Tests.fsproj
    └── Program.fs
```

## 2. API 介面與元件設計 (Interface & Component Specification)

### 2.1 "Actor Dynamic" Tab Page 掛載設計 (Client-Side)
在現有的 PTCS 中，頁面型態可能包含 Chat、Sets 等。為了支援 `Actor Dynamic` 的展示頁面（一個概念展示用的 Tab Page，內部會呈現 SDUI 元件的互動），我們需要定義專屬的 `Tab Page Type` 處理器。

**Code Snippet: 前端 Tab 頁面與 UI 綁定**
```fsharp
namespace PulseTrade.Comm.Spa.Dynamic.Client

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Html
// 假設上游提供了註冊 Tab 型態的介面 (此部分亦可能需要 UPSTREAM_RFC 支援)
open PulseTrade.Comm.Spa.Client

[<JavaScript>]
module ActorDynamicTab =
    
    /// 渲染 Actor Dynamic 專屬的 Tab 頁面內容
    let renderActorDynamicPage (pageId: string) =
        div [ attr.``class`` "actor-dynamic-container" ] [
            h2 [] [ text "Actor Dynamic 展示頁面" ]
            div [ attr.``class`` "sdui-canvas-area" ] [
                // 這裡將放置 FSkynet CanvasComponent + GridFeatures 
                // 以及擴充的 App Loader, Color Picker 等元件
                text "動態元件載入中..."
            ]
        ]

    /// 提供一個註冊點給宿主
    let Start () =
        // 註冊客製化 Renderer 攔截 fskynet-sdui 訊息
        PulseTrade.Comm.Spa.Client.RegisterRenderer(DynamicSduiRenderer.create())
        
        // 註冊 Actor Dynamic 的 Tab Page 型態
        PulseTrade.Comm.Spa.Client.RegisterTabPageHandler("actor-dynamic", renderActorDynamicPage)
```

### 2.2 後端擴充點 (Server Extension) 與 Actor 註冊
```fsharp
namespace PulseTrade.Comm.Spa.Dynamic.Server

open PulseTrade.Comm.Spa
open Akka.Actor

[<AutoOpen>]
module CommHubExtensions =
    type CommHub with
        /// 將 Dynamic Sdui Actor 與路由掛載至現有的 CommHub
        member this.useDynamicSdui(actorSystem: ActorSystem) =
            // 1. 註冊 "Actor Dynamic" 展示用的後端 Actor
            let props = Props.Create(fun () -> new ShowcaseDemoActor())
            let showcaseActorRef = actorSystem.ActorOf(props, "showcase-dynamic-actor")
            
            // 2. 將 Actor 與 PTCS 的路由或 CommHub 做綁定
            // (假設 CommHub 有公開的 RegisterActor 介面)
            // this.RegisterActor("actor-dynamic", showcaseActorRef)
            
            this
```

## 3. 類別庫封裝與相依 (NuGet Packaging)
- Target Framework: `net10.0`
- 目前 `src/PulseTrade.Comm.Spa.Dynamic.fsproj` 以 exact `PackageReference Include="PulseTrade.Comm.Spa" Version="[0.2.5-beta58]"` 消費 PTCS，不再使用 local `ProjectReference`。本輪 package 化先使用自己編譯並 local deploy 到 SDK `FSharp\library-packs` 的 PTCS / PTCS.Dynamic；NuGet.org push 需等 operator-provided key/path，禁止在 repo/log 中寫入 secret。
- 透過 WebSharper 將 `Client/*.fs` 翻譯為前端 JS，並保證 `ActorDynamicTab.Start()` 能在 PTCS 核心啟動時正確呼叫。

## 4. RFC-PTCS-DYNAMIC-0002 Dynamic Argu Form Design

Formal RFC: `doc/RFC-PTCS-DYNAMIC-0002.dynamic-argu-form-runtime.md`

### 4.1 Server metadata and schema generator

新增 server-side metadata layer：

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
```

`ArguFormSchemaGenerator.generateSduiJson` 將 `ArguUnionCaseMetadata` 轉為：

```text
{ schema = "fskynet-sdui"; formMode = "argu-form"; sdui = [...] }
```

Reflection 只能作為 allowlisted metadata registry 的 producer；browser-supplied type name 不可直接 unrestricted resolve。

### 4.2 Browser SubmitArguForm

`DynamicRenderer` 需為 `formMode = "argu-form"` 建立 scoped form state。Button action `SubmitArguForm` 的流程：

```text
includeStateOf ids
  -> collect field value + arguParam
  -> ArguFormCommandLine.encode
  -> submitFn rawArgu
```

`ArguFormCommandLine.encode` 必須集中測試 whitespace / quote escaping；輸出字串只作 PTCS ActorArgu payload，不作 shell command。

### 4.3 PTCS seam integration

當 PTCS `RFC-PTC-SPA-0007` seam 可用後，Dynamic browser bundle registers：

- append input renderer：依 page shape + selected key 判斷 `actor-dynamic` / Dynamic Argu key；
- add-key dialog renderer：新 canonical 回傳 `actorAddress :: duTypeOrTemplateKey :: canonicalArgString`；舊 `actorAddress :: duTypeName :: unionCaseNames` 只作 migration / historical reference；
- message renderer：保留既有 `fskynet-sdui` rendering。

若 seam 尚未存在或 renderer 失敗，Dynamic 必須讓 PTCS fallback 到既有 textarea/raw key path。

### 4.4 RN proxy integration

Dynamic 不 reference RN package。若 key 的 actor address 指向 RN DurableProxy，PTCS core 仍只送出 `ActorArguTargetCommand.RawArgu`；RN side 由 `CommandToCell` / `InvokeLegacy` / `LegacyReplyToCell` 處理 legacy actor adaptation、delivery、confirm 與 result completion。

## 5. RFC-PTCS-DYNAMIC-0003 Unified SDUI / Form DSL Design

Formal RFC: `doc/RFC-PTCS-DYNAMIC-0003.unified-sdui-form-dsl-roadmap.md`

### 5.1 Canonical DSL types

```fsharp
type SduiRenderSurface =
    | Canvas
    | FormInput

type SduiOptionSource =
    | StaticOptions of string list
    | QueryOptions of providerId: string * dependsOn: string list
    | StreamOptions of streamId: string * dependsOn: string list

type SduiTreeConnector =
    | Orthogonal

type SduiTreeToggle =
    | BoxedPlusMinus

type SduiTreeBinding =
    { DataRef: string
      RootNodeIds: string list
      NodeIdField: string
      ParentIdField: string
      LabelField: string
      StatusField: string
      Columns: string list
      Connector: SduiTreeConnector
      Toggle: SduiTreeToggle }

type SduiNode =
    | Stack of id: string * children: SduiNode list
    | Section of id: string * title: string option * children: SduiNode list
    | TextBlock of id: string * text: string
    | Input of id: string * label: string * kind: string * binding: string
    | Select of id: string * label: string * options: SduiOptionSource * binding: string
    | Button of id: string * label: string * actionId: string
    | Tree of id: string * binding: SduiTreeBinding * onNodeClickActionId: string option

type SduiDocument =
    { Schema: string
      Version: string
      DocumentId: string
      Surface: SduiRenderSurface
      Nodes: SduiNode list
      Actions: Map<string, string>
      Bindings: Map<string, string> }
```

Implementation may refine union names, but the separation is mandatory：renderer consumes `SduiDocument`; adapter consumes Argu / DU metadata。

Canvas `Tree` is the required renderer target for PTC ActorTree integration. The upstream `ActorTreeDocument` is produced by PTCS/PTC Actor Registry projection and then converted into `SduiDocument Surface=Canvas` with one `Tree` node. Dynamic does not persist actor registry data, does not rebuild PCSL projection, and does not write actor state reports. If Dynamic is missing or the Tree renderer fails, PTCS core must keep using its fallback table with `parentId`。

### 5.2 Target resolver

```fsharp
type DynamicTarget =
    | DirectDslTarget of actorAddress: string * formDslId: string
    | ArguTemplateTarget of actorAddress: string * templateKey: string * canonicalArgString: string

module DynamicTargetKey =
    val tryParse : string list -> Result<DynamicTarget, string>
```

Parse rules：

1. key list length must be at least 2；
2. first item is actor address；
3. `[ actor; formDslId ]` resolves only when second item matches Form DSL registry；
4. `[ actor; templateKey; canonicalArgString ]` resolves only when second item matches Argu adapter registry；
5. canonical arg string is parsed server-side with the registered Argu parser before DSL generation；
6. unknown second item or parse failure returns controlled error。

### 5.3 Argu-to-FormDsl adapter

```fsharp
type ArguTemplateRegistration =
    { DuTypeName: string
      TemplateKey: string
      TemplateType: Type
      Aliases: DynamicArguAliasBinding
      DefaultArgString: string option }

type DynamicArguAliasBinding =
    { CaseAliases: Map<string, string>
      FieldAliases: Map<string * string, string>
      OptionAliases: Map<string * string, string> }

type ParsedArguValue =
    { FieldName: string
      Values: string list }

type ParsedArguCase =
    { CaseName: string
      Values: ParsedArguValue list }

type ParsedArguSubcommand =
    { CommandToken: string
      TemplateType: Type
      Cases: ParsedArguCase list }

type ParsedArguTarget =
    { ActorAddress: string
      TemplateKey: string
      CanonicalArgString: string
      RootCases: ParsedArguCase list
      TailSubcommands: ParsedArguSubcommand list }

module ArguToFormDsl =
    val parseTarget : ArguTemplateRegistration -> canonicalArgString: string -> Result<ParsedArguTarget, string>
    val generate : ArguTemplateRegistration -> ParsedArguTarget -> SduiDocument
```

Each parsed root case and supported subcommand becomes a visible form section. The adapter maps:

- string -> text input；
- numeric -> number input；
- bool flag -> checkbox；
- enum / zero-field DU enum -> select；
- tuple -> ordered input group；
- list -> repeatable input group；
- nested `ParseResults<'T>` / Argu `ArgumentType.SubCommand` -> tail subcommand section when supported, otherwise controlled unsupported-field error。

Alias mapping is applied during DSL generation only:

```text
canonical case/field/option name
  -> alias lookup from DynamicArguAliasBinding
  -> label/title in SduiDocument
```

Submit/raw command building always uses canonical Argu names. Alias text must not enter `ActorArguTargetCommand.RawArgu`.

Composite raw command builder rules:

```text
root cases in token/configured order
  -> tail subcommand token, e.g. datarange
  -> subcommand args
```

Expected PFCF data-range example:

```text
--pfcfedx trivial --pfcfgtcconf OIInf TAIFEX FillSquareCombine OrderByTXDT CathayBKTaifexFill --to 90000 --parentchilds 2 5 --bba F008 000 9910357 --decimalquote 6 0 --round 6 4 2 datarange --referencedatemode ModeAccountingDate --between 20251104 20251104 --calibrate2curdayiflargerthancurday
```

### 5.4 Backend-linked options

`QueryOptions(providerId, dependsOn)` is a declared provider lookup。Renderer may call the PTCS core safe extension query callback only for registered providers。The DSL must not contain arbitrary URL, headers, tokens, script text or executable code。

### 5.5 PTCS.Host demo integration

PTCS.Host registers:

1. a direct form DSL target derived from `example DU.txt`；
2. an Argu adapter target for a host-local `PFCF_AKKA_CMD` demo subset；
3. a durable proxy/echo actor target for E2E。

`example DU.txt` is cp950 encoded and contains Chinese identifiers/comments。The host demo should either preserve valid identifiers or map them to stable ASCII labels while keeping display labels in metadata。Missing external types such as `DataTypeT.RTTables` must be represented by host-local stubs or excluded from the first demo subset with a documented controlled unsupported-case message。

## 6. Actor Dynamic action mode design

`RFC-PTCS-DYNAMIC-0004` keeps Dynamic renderer logic mode-aware without requiring PTCS core to parse Dynamic target semantics.

### 6.1 Add-key renderer mode dispatch

```fsharp
let renderAddKey (ctx: obj) =
    let context = ctx |> As<AddKeyContextDto>
    match asText context.shape with
    | "actor-argu-target" -> renderArguTargetKey context
    | "actor-dynamic-target" -> renderDynamicTargetKey context
    | "actor-dynamic-proxy" -> renderDynamicProxyKey context
    | _ -> None
```

`renderArguTargetKey` requires explicit proxy and native target parts:

```text
proxyActorAddress
targetActorAddress
duTypeOrTemplateKey
canonicalArgString
```

Submit payload:

```fsharp
{ keys = [| proxyActorAddress; "target-v1"; targetActorAddress; duTypeOrTemplateKey; canonicalArgString |]
  displayName = displayName }
```

The first key segment remains the PTCS route actor. PTCS `ActorArguTargetCommand.TargetActorAddress` carries the native target actor address. Dynamic must not rely on `BeforeAddKey` / Host script hooks to create a per-target proxy and rewrite the persisted key.

`renderDynamicTargetKey` uses the same UI when DU/template is present. Direct actor key without DU/template is intentionally handled by PTCS core Add actor key fallback.

`renderDynamicProxyKey` requires:

```text
proxyActorAddress
rnActorAddress
targetKind
displayName optional
```

Submit payload:

```fsharp
{ keys = [| proxyActorAddress; "proxy-v1"; rnActorAddress; targetKind |]
  displayName = displayName }
```

### 6.2 Append input renderer mode dispatch

```fsharp
let renderAppendInput (ctx: obj) =
    let context = ctx |> As<AppendInputContextDto>
    let keys = normalizeDynamicTargetKeyParts context.keyParts
    match asText context.shape, keys |> Array.toList with
    | "actor-argu", proxy :: "target-v1" :: target :: template :: raw :: _ -> renderResolvedFormInput context template raw
    | "actor-argu", _ :: template :: raw :: _ -> renderResolvedFormInput context template raw
    | "actor-dynamic", _ :: "proxy-v1" :: _rnTarget :: _targetKind :: _ -> None
    | "actor-dynamic", _ :: template :: raw :: _ -> renderResolvedFormInput context template raw
    | "actor-dynamic", [ _actor ] -> None
    | _ -> None
```

Returning `None` for single-key Actor Dynamic is intentional: PTCS fallback textarea becomes arbitrary string / JSON DSL input, and message rendering later decides whether reply is canvas.

### 6.3 Canvas message renderer

```fsharp
match tryGetSchema payload with
| Some "fskynet-sdui" -> Some(createSduiCanvas payload)
| _ -> None
```

The renderer must not treat page type, key shape, or actor address as proof of canvas content.
## 2026-06-28 ActorsPage Renderer Design

### Module placement

Current first slice extends `Client/ActorDynamicTab.fs`:

```fsharp
let IsActorsPagePayload (rawContent: string) =
    rawContent.IndexOf("ActorTopologyPage") >= 0

let registerActorsPageRenderer () =
    // register string -> Dom.Node option through PulseTradeRegisterPageRenderer
```

Do not move this first slice to a new `[<JavaScript>]` client file until WebSharper compiler behavior is fixed or re-verified. Clean short-path builds showed:

- new client compile unit, even no-op, can crash `wsfsc.exe`;
- `String.Contains` in `[<JavaScript>]` code can crash `wsfsc.exe`;
- a single `IndexOf` predicate compiles.

### Runtime flow

```text
ActorDynamicTab.Main()
  -> _registerRenderer()              // generic Canvas message renderer
  -> registerActorsPageRenderer()     // page-level ActorsPage renderer
  -> ArguFormRenderer.Register()

PTCS /actors
  -> builds ActorsPage / ActorTopologyPage DSL
  -> calls registered page renderers
  -> Dynamic returns Some Dom.Node for ActorTopologyPage
  -> PTCS mounts only Dynamic page host
```

### First-slice output

`createActorsPageDocument` renders a page-level Dynamic Actors UI, not the generic `FSkynet 動態畫布 (Canvas)` summary card. The current output includes:

- action shell for reload / report / schedule report, with report actions still disabled until PTCS report wiring is ready;
- count cards for renderer identity, node groups, actor tree rows, and active rows;
- node blocks derived from actor-system host/port in `actorTreeNodes` address data;
- role ordering: PTCS Host -> GW Host -> RN Host -> Unknown;
- hierarchy rows with full labels, active/degraded status, and boxed `+` / `-` toggles;
- grid rows with full actor addresses and path/status metadata.

The Playwright gate now verifies page ownership, clean host/port grouping, role ordering, and real collapse/expand behavior. Tree toggles are backed by WebSharper `Var` state: collapse removes child rows from the rendered tree and updates `aria-expanded`.

### Next design gates

1. Replace token classifier with strict DSL codec once WebSharper-safe parsing is available.
2. Replace the current renderer-side inferred node grouping with explicit `nodeGroups` codec once PTCS emits it.
3. Polish actor hierarchy tree connector geometry and card/action layout.
4. Add grid/cards/actions from the same ActorsPage DSL document.
5. Replace the current source-host Playwright proof with reusable F# verifier coverage through PTCS `/actors`, including Dynamic accepted, Dynamic absent, and unsupported renderer fallback paths.

## RFC-PTCS-DYNAMIC-0008 Reply Presentation Adapter Design

### Bounded envelope decoder

`TaResearchReplyPresentation.runtimeFrames`改為明確pipeline：

```text
payload
  -> trim / size-depth guard
  -> unwrap JSON string at bounded depth
  -> inspect canonical fCell2 envelope or Case/Fields DU
  -> select inbound/replied cells only
  -> strict RuntimeFrameCodec decode
  -> validate protocol/document/canvas identity
```

decoder不得用target alias、page title或substring猜測。成功回`RuntimeFrame array`；真正NonSdui回`None`；present-invalid回bounded error model。raw points只留在runtime data，不進summary DOM/text。

### Summary projection

`summaryFromFrames`只投影instrument、interval/scale、requested range/coverage、ordered rows與trace parameters、freshness/watermark/quality。summary Node不得序列化完整frame、series或point array；diagnostic只保留stable reason code與bounded message。

### Lazy runtime handle

`tryResolve`回傳`ReplyPresentation`時只建立summary closure與canvas identity，不呼叫`mountByIdWithOptions`。`MountInline`/`MountFullscreen`才建立或移轉唯一handle；disposer必須執行`SetActive false`與`Dispose`。Collapsed、reply removal、page unmount及disconnect後timer/socket/subscription均回baseline。

`TaClientLifecycle.ResyncRequired`在connected/active狀態即使`InFlight=true`也可執行：先`CancelPoll`與`CancelTimeout`，再送唯一`RequestFullSnapshot`並排新timeout。這是invalid production delta/reply的恢復路徑。

### Composer boundary

`ArguFormRenderer`不持有Plain/Form mode。它只在PTCS Form host被建立時render，submit仍回raw Argu string；Plain single textarea、mode switch、selected target與history由PTCS core owns。

## 2026-09-21 Generic Marker Overlay Design

完整決策見 `doc/RFC/RFC-PTCS-DYNAMIC-0015.generic-marker-overlay.md`。

```text
producer generic TaMarker[]
  -> TaMarkerCodec.encodeBucket
  -> TemporalSeriesPoint(Position, Value)
  -> RuntimeFrame v2
  -> frame shape validation
  -> apply whole frame to candidate
  -> MarkerValidation.validateCandidate(document, candidate)
  -> commit OR retain last-good + RequestResync/RejectFrame
  -> RendererModel resolves target candle by Position
  -> fixed row-local marker lanes + deterministic stack
```

Public seams：

- `TaMarkerCodec`：strict marker/bucket encode/decode，preserve tooltip與bucket order。
- `TaMarkerTraceOptionsCodec`：typed `marker.targetTraceId` options helper。
- `MarkerValidation`：document與candidate semantic validation；只掃marker refs及target candle refs。
- `RuntimeEffect.RejectFrame`：structured non-recoverable rejection，不觸發resync loop。
- `MarkerGeometry`：pure row-local layout，size 9px、first gap 4px、stack gap 2px、top/bottom lanes各44px。

Failure paths：sequence/base revision/axis gap/missing authoritative state走`PausedForResync + RequestResync`；malformed marker/duplicate id/illegal color/enum/target/hard limit走`Suspended + RejectFrame(Recoverable=false)`。兩者都保留last-good。Unknown v2 trace kind與v1 marker fail closed。Marker-only patch重用未變DataRef與prepared candle資料，不全量重decode。

Test seams：codec round-trip/strict shape；snapshot/patch/clear/move/idempotency；composite/split candle resolver；stack/lane/Y-domain/viewport；v1/v2/cache/unknown-kind compatibility。

### Marker v2 correction

`TaMarkerShape` current cases為`TriangleUp | TriangleDown | Circle | Square | Diamond`。`TaMarkerCodec`只encode `ta-marker.v2`；decode同時接受v2與legacy v1，v1 `arrow + above-bar`映射`TriangleDown`、`arrow + below-bar`映射`TriangleUp`，其餘舊shape同名映射。cache schema 3才可rehydrate current marker；schema 2直接miss/resync。

```text
candidate marker traces
  -> validate each DataRef/Position bucket total <= 4
  -> group decoded markers by (rowId, targetTraceId, Position, Anchor)
  -> validate aggregate count <= 4
  -> preserve document trace order then bucket array order
  -> assign global lane 0..3
  -> render shape independent of anchor
```

`Outline` visible glyph使用`fill=none`及`stroke=marker.Color`。若SVG原生paint hit testing不足，renderer增加同geometry透明interaction target；該target只提供pointer/tooltip命中，不得阻止shared cursor依x slot更新，不進Y-domain/numeric legend/time axis，也不得造成candle series rebuild。

Owner release gate以PTCS browser-demo／Interactive Client的desktop/mobile F# Playwright驗direction、fill、tooltip、cursor parity及exact graph。Daedalus升級SPAA／Interactive Extension、真`BacktestPresentationEvent`映射與Notebook `.dib` parity是發布後consumer gate。

## 2026-09-24 Backtest Presentation UX Design

完整決策：`doc/RFC/RFC-PTCS-DYNAMIC-0024.backtest-presentation-ux.md`。

### Contract and codec

```fsharp
type TaOverviewStripe =
    { StripeId: string
      EventTimeUtc: string
      Color: string
      StrokeWidthCssPixels: float
      Label: string option
      Tooltip: TaMarkerTooltipField array }

type TaOverviewStripeTraceOptions =
    { TargetTraceId: string
      CollisionGroup: string
      LayerOrder: int }
```

`TaTraceKind.OverviewStripe`的data ref沿用`TemporalSeries`：每個`TemporalSeriesPoint.Position` value是`TaOverviewStripeCodec.encodeBucket`產生的array；item `_type=ta-overview-stripe.v1`。`EventTimeUtc`須與該position的canonical axis event time一致，target須為同row candlestick。Options keys固定為`overviewStripe.targetTraceId`、`overviewStripe.collisionGroup`、`overviewStripe.layerOrder`。

`TaOverviewStripeContract.documentErrors/candidateErrors`只掃stripe refs、target candle refs及axis，不掃全部TA values。Limits為bucket 64、dataRef 10,000、frame 20,000；id 128、label 64、collision group 64、tooltip 16、layer 0..63、stroke width 0.5..4.0。Marker bucket/lane hard limit同步由4升為64，dataRef/frame維持10,000/20,000。

### Candidate flow

```text
RuntimePatch [ReplaceDataRef stripes; ReplaceDataRef orders; ReplaceDataRef fills]
  -> validate patch operation shape/reference
  -> fold every operation into private candidate map
  -> validate temporal axis/series
  -> validate marker candidate across traces
  -> validate overview stripe candidate across traces
  -> commit revision/data once OR retain last-good
```

Malformed／duplicate／hard-limit使用`RejectFrame Recoverable=false`；missing authoritative axis/data與revision gap使用`RequestResync`。Validation不得修改state、DOM或先發布任何單一dataRef。

### Overview prepared renderer

```text
stripe EventTimeUtc
  -> target canonical axis position
  -> active visible-window slot
  -> navigator X pixel
  -> group(targetTraceId, collisionGroup, xPixel)
  -> order distinct traces by LayerOrder/document order
  -> equal vertical lanes; same-trace ids remain in pixel bucket
  -> batch SVG paths by trace/lane/color/width
```

Stripe path為`pointer-events:none`。Navigator共用interaction layer查prepared pixel bucket並提供count／tooltip，不在pointer move掃全資料。Z-order為close path、stripe path、selection mask／handle。Close path sampling與stripe index各自處理，禁止拿sampled close index當event位置。

### Dense marker presentation

Candidate以document trace order＋bucket order保留最多64筆。Renderer直接layout前4筆；remaining markers建立單一cluster model：`Count`、ordered marker references、active index。Cluster button支援Enter／Space toggle、ArrowUp／Down選擇、Escape close，detail逐筆復用既有label／tooltip。Cluster與glyph總數是bounded DOM，不改Y-domain、candle prepared data或shared cursor listener。

### Row resize state machine

```text
NoOverride
  -> pointer/keyboard resize -> Preview(resolvedPx)
  -> pointer-up/keyup commit -> Override(resolvedPx)
  -> Reset/Home/double-click -> NoOverride
  -> row removed/unmount -> dispose
```

Local map key為`CanvasInstanceId + RowId`。Default height：candle/composite `clamp 180 720 (round(250 * HeightWeight))`；scalar `clamp 96 480 (round(112 * HeightWeight))`。Scenario data revision不清除override；fresh mount／reload不rehydrateoverride。Pointer move只排一個requestAnimationFrame，data readers與document fingerprint保持不變。

Total row height不再由marker presence決定。Plot height由resolved total扣除header／axis／small padding；scalar plot不留固定空band。Row cursor timestamp使用SVG plot外固定32px top gutter中的92×28px兩行HTML tag，字型、font size、line-height、padding、border、width與height都是固定CSS pixels；X跟shared crosshair並左右clamp。Row resize只改plot height及tag的top/left，不得縮放或重排tag。

### Package closure

RFC-0024 closure：Contracts `0.1.19` → Renderer `0.1.45` → Interactive.Client `0.1.37`；Dynamic.Ptcs `0.1.41` exact Contracts；Ptcs.Client `0.1.59` exact Contracts/Renderer；兩個PTCS adapters維持PTCS `[0.2.46]`。Interactive.Client的bundle manifest由pack target以`$(Version)`生成，避免nuspec與內嵌bundle版本漂移。不得ProjectReference或partial graph。Navigator使用獨立shell prepared-data signal；same-topology資料patch更新該signal與row data Vars，但不更新chart runtime mount identity。

Test seams：strict codec/unknown field/limits；candidate atomicity/last-good；same-X lane/pixel bucket；same-topology empty→non-empty OverviewStripe且chart render sequence不變；marker cluster keyboard/focus；row resize/default/reset/dispose；4,000 slots long-task；exact nupkg/bundle/readback。

## 2026-09-25 Runtime Snapshot Transport Framing

完整決策：`doc/RFC/RFC-PTCS-DYNAMIC-0025.chunked-snapshot-transport.md`。

### Wire shape

Wire使用explicit schema/kind fields，不擴充`RuntimePayload`：

```fsharp
type RuntimeSnapshotTransportPacket =
    | Start of batchId: string * itemCount: int * header: RuntimeFrame
    | Item of batchId: string * itemIndex: int * dataRef: string * value: SduiValue
    | Commit of batchId: string * itemCount: int

RuntimeSnapshotTransportCodec.encodeFrame : RuntimeFrame -> Result<string array, string>
```

實際JSON root皆含`schema = "ptcs-dynamic-snapshot-chunk.v1"`與`kind = "start" | "item" | "commit"`。Start的header必須是原frame identity／sequence／revision／freshness，但`Snapshot.Data = Map.empty`。Item依F# Map canonical key order編號0..N-1。Commit重申batch id與count。Batch id由frame identity與transport sequence的deterministic fingerprint產生，不依browser state。

Encoder以structured JSON writer嵌入`Json.Serialize`產生的header/value raw JSON，禁止字串拼接／雙重JSON字串。非Snapshot frame直接回`[| BrowserRuntimeCodec.encode frame |]`，因此host可一律flatten結果。

### Browser state machine

```text
Idle
  + legacy RuntimeFrame -> scheduled legacy decode/reduce -> publish or reject
  + Start(generation,batch,count,header) -> Staging(nextIndex=0, seen={}, items=[])

Staging
  + Item(same batch,index=nextIndex,unique ref) -> phased decode -> append -> nextIndex+1
  + Commit(same batch,count=nextIndex=expected) -> assemble canonical Snapshot
      -> phased canonical reducer validation
      -> one publish + accepted projection eligible for optional cache write + local SnapshotAccepted lifecycle
  + wrong/missing/duplicate/interleaved/disconnect/new generation -> discard -> last-good + resync
```

`OnMessage`只把`socketGeneration + encoded text`加入bounded FIFO並排一個pump task。Pump每次只處理一個envelope；同一item內Array／TemporalSeries points沿用`PointDecodeBatchSize=256`。所有continuation再次比對socket/pump generation；舊callback只回`Superseded`，不得清除新batch或建立第二個timer。

### Limits and failure

- start itemCount必須`0..MaxDataRefsPerSnapshot`；queue與staged item總量同受runtime limits限制。
- item index必須嚴格遞增，dataRef非空且唯一；batch id/count需exact match。
- start header需通過frame envelope validation且Snapshot Data為空；item value沿用snapshot value validation。
- commit後仍完整執行unknown dataRef、axis authority、temporal series、marker/stripe overlay與runtime revision validation。
- metadata錯誤使用`runtime-snapshot-chunk-*` structured reason；canonical validation沿用既有error code。所有錯誤保留last-good、停止該batch、request resync，且不得觸發accepted lifecycle或讓partial state進入cache。現行wire沒有snapshot ACK frame。

### Producer integration

```fsharp
let encodedFrames =
    runtimeFrames
    |> Array.collect (fun frame ->
        RuntimeSnapshotTransportCodec.encodeFrame frame
        |> Result.defaultWith failwith)
```

SPAA initial `Frames`與reconnect `sendFullSnapshot`都使用同一helper；Document／Patch仍各是一個訊息。Owner tests須直接消費`encodeFrame`輸出，避免測試專用framing與production client分叉。

### Owner decoder / assembler

Contracts提供純.NET與WebSharper共用的ordered stream API：

```fsharp
RuntimeSnapshotTransportAssembler.decodeFrames
    transportGeneration
    orderedWireMessages
// Result<RuntimeFrame array, RuntimeSnapshotTransportAssemblyError>
```

`decodeFrames`接受legacy frame與零或多個`start/item/commit` batch混合的單一ordered stream；`PacketIndex`永遠是整條stream的zero-based global index。逐packet consumer使用`create/createAt -> decodePacket -> acceptPacket/acceptEncoded -> finish`，不可自行切batch或複製schema判斷。`acceptPacket/acceptEncoded`只完成framing、generation、順序、count、duplicate ref與item-local validation；commit後的`CompletedFrame`是candidate。純.NET machine consumer必須呼叫`finish`做完整canonical frame validation；browser將candidate交給既有phased reducer做同等validation與atomic publish，避免commit RAF同步重複掃描完整snapshot。

錯誤優先序固定：active batch中的packet若無法decode為chunk，只有同一wire item可成功decode且validate為legacy frame時才回`runtime-snapshot-chunk-interleaved`；兩者都失敗時保留原chunk schema/kind/decode error。這使machine E2E與browser對malformed envelope取得相同reason code與global packet index。

Test seams：deterministic encoder/legacy singleton；zero-item snapshot；mixed legacy＋multi-batch stream；missing/duplicate/out-of-order/orphan/trailing/mismatch；interleaved legacy；global packet index；generation/disconnect；wrong schema/kind/malformed envelope；invalid SduiValue/canonical reducer failure；commit candidate與machine `finish` validation boundary；commit-only publish/accepted lifecycle/cache eligibility；4,000×28×5 candle target-renderer long-task；legacy/cache regression；exact package graph。

### 2026-09-29 Renderer correction

- Overview candle geometry對當前reference domain做bounded等距取樣，最多280筆；一次建立wick／up-body／down-body三條path，顏色取plot palette。OverviewStripe維持獨立batched paths，沒有trace就不猜domain、不補線。
- Row control的label區為`flex:1 1 auto; min-width:0`並ellipsis；`Edit`為`flex:0 0 52px`。完整label放title，binding lookup仍是唯一可編輯authority。
- Viewport local reducer不再因`PendingActionId`停用；`queuedVisibleRangeAction`只保存latest intent。settlement後若runtime可送，使用current revision flush；dispose/generation change清queue。
- Cursor效能authority為browser event handler記錄request time，shared crosshair／labels／visible value完成後記event-to-render latency。Playwright host round-trip只作driver診斷，不作UI gate。

Current released exact graph：Contracts `0.1.26` → Renderer `0.1.62` → Interactive.Client `0.1.54`；Dynamic.Ptcs `0.1.49` exact Contracts；Ptcs.Client `0.1.76` exact Contracts/Renderer；兩個PTCS adapters維持PTCS `[0.2.46]`。Daedalus fresh-cache machine consumer已用owner `decodeFrames`解出4,000 workspace，1,140 functional E2E與3,820 standard trace皆PASS；48→All為59.41ms／over100=0。五包public push回`Created`；NuGet repository signatures有效，official nuspec／DLL／bundle functional entries與local immutable artifact相同。大型candle projection以固定bucket array單次聚合；line projection以固定bucket min/max arrays保留chronological extrema；Y-domain以first-value initialized accumulator單次掃描，避免WebSharper對Infinity sentinel的錯誤轉譯。Shared-cursor click只查accepted prepared timeline，不再從raw data重做projection。Prepared geometry同時保存每條trace的latest presentation；當shared cursor未固定時，visible-value refresh直接讀此cache，不再對missing/sparse higher-scale trace逐條自tail反向掃描。Source interval、cursor action payload、bounded cursor lookup與Y-domain padding語意不變。

### Trace lifecycle／cursor event／contrast contract

`SduiAction.RemoveTaTrace(CanvasInstanceId,rowId,traceId)`編碼為`remove-trace`，PTCS transient DTO另帶`traceId`。Renderer維護`HiddenTraces`與`RemovedTraces`兩個set；hide不送action，remove走既有single-in-flight callback。accepted/no-frame先更新removed set；Rejected／RevisionConflict不改local state。same-canvas patch保留set，Reset Canvas／identity replacement／dispose清空。

Row controls只列non-system traces。`Marker`與`OverviewStripe`排除；最後一條未remove的non-system trace移除後整列hidden，hidden trace仍算存在。DOM visibility helper先保存`data-ptcs-authored-display`，解除hidden時恢復 authored display，避免inline `display`覆蓋hidden attribute。

Cursor event reader將Marker placements與OverviewStripe events映射為`TaMarkerCursorItem`。stable id重複時Marker勝出；排序先依authored trace，再round-robin取item，visible budget為4，其餘顯示`+N`。capability存在但slot空白為`None`，缺capability為`Unavailable`。OverviewStripe event time先由canonical temporal axis解position，不從pixel或sampled close path反推。

Hollow marker先畫`data-marker-contrast-halo` path，再畫semantic marker；halo帶`data-marker-halo-for`且`pointer-events:none`。Plot不得建立`ta-marker-label-*`。Data window使用wrap／auto height／visible overflow，無固定30px或內部vertical scroll。

Test seams：remove-trace contract/adapter round-trip；accepted/reject/stale generation；hide/remove/reset/identity；last non-system trace row collapse；Marker/Stripe stable-id dedupe與round-robin；None/Unavailable；dark halo visible pixels且semantic stroke不變；plot label count 0；data window geometry；4,000-bar long-task與exact package graph。

### Visible K-bar row Y-domain

每個含candlestick的row以該row當前viewport投影後的candles與同row projected line/histogram points建立獨立bounds。完整source中viewport外的extrema、其他row及不可見point不得進入bounds；scalar-only row維持既有8% data padding。

Candlestick row使用完整SVG viewBox高度，SVG edge到visible extrema的centerline padding固定為15 CSS px，而不是另留固定plot inset再加data range百分比。Renderer以viewBox height與目前`rowHeight: Var<int>`反算value padding；row keyboard/pointer resize時同一reactive geometry重新計算，但不重掛row/SVG topology。實際path外緣可因stroke向外擴張而略小於15px；空bounds回fallback，single-value維持既有`value +/- 1`語意。測試切點為遠端extrema隔離、250/720px centerline padding、真DOM path外緣、48/200/All、pan/zoom、resize與真SPAA逐row geometry。

### Fixed CSS-pixel line / navigator boundary

一般`TaTraceKind.Line`與SMA path沿用`ta-trace-{rowId}-{traceId}`，authored width在render邊界夾為1–2 CSS px，並同時輸出`data-stroke-width-css-pixels`與`vector-effect="non-scaling-stroke"`。因此SVG viewBox、viewport與row resize只改幾何，不縮放線寬；histogram/volume仍走fill path，不套此契約。

Overview selection rect只負責半透明選取填色。左右可見邊界為`ta-overview-left-handle-visual`／`ta-overview-right-handle-visual` line，固定`#155f73`、2 CSS px、`non-scaling-stroke`及`pointer-events=none`。既有`ta-overview-left-handle`／`ta-overview-right-handle`保留為transparent rect drag hit target，避免把可操作寬度誤當畫面線寬或破壞既有操作。Owner browser gate須在default、row resize及navigator viewport commit後分別驗computed stroke與DOM contract；真SPAA consumer另量實際geometry、截圖與互動效能。

Navigator drag以live outer SVG currentTarget同時作bounds與event owner。`pointerdown`取得pointer id後必須呼叫`setPointerCapture`，後續`pointermove`／`pointerup`／`pointercancel`皆綁同一root；release即使落在SVG或嵌入iframe之外，仍完成一次commit／adjacent coverage request。capture不可用時才退回child document pointer listeners。`pointercancel`只清draft與handlers，不送remote action；cleanup必須release capture並移除三種listener。Owner gate除同document root外12px release外，另以iframe fixture驗child browsing-context外release及callback唯一增量。

所有chart row的authored/default plot height在Renderer初始化、reset與browser reload時封頂250 CSS px；此限制不改寫文件中的`HeightWeight`，也不縮小resize handle的manual range。Candlestick與scalar row的手動上限仍分別為720／480px，trader操作後可超過250px；同canvas、同row的local override在authoritative data replacement時保留。Owner gate必須先在任何resize前量測全部chart SVG，再驗manual `>250`、reset/reload回到capped default及cursor垂直幾何同步。

### Default viewport / marker OFI band

`RendererModel.initialViewportWindow`讀取`DefaultView["visibleBars"]`，只接受finite positive integral number；resolved count夾於loaded count與renderer maximum，否則fallback 48。`renderWithProjectionCommit`只在新canvas application套用一次，same-application patch、navigator drag與user selection不重設。

`RendererModel.markerCursorItems`從prepared placements依slot取出`TaMarkerCursorItem`，排序固定為trace order／lane／marker id，保留Label、Color、Tooltip與EventTimeUtc。每列建立固定24px `ta-row-ofi-band-{rowId}`，位於32px cursor gutter與SVG plot之間；single-rAF cursor callback直接更新四個預建item slots與overflow `+N`，無事件清空內容但不改高度。plot marker仍保留glyph與`<title>`，不再建立`ta-marker-label-*` inline SVG text。一般plot `mousemove`以row axis解析slot；marker glyph／overflow item的`mousemove`與click直接以accepted `TaMarkerPlacement.SlotIndex`更新同一cursor state並停止冒泡，防止高密度同CSS pixel二次snap到相鄰slot。stale generation/dispose沿用renderer lifecycle gate，不得持有可更新的舊DOM reader。

Overview selection填色為`rgba(203,213,225,.20)`；左右可見boundary為`#4ade80`、2 CSS px、non-scaling stroke。transparent 8-unit hit rect與drag geometry維持原contract。

Test seams：fresh 4,000 visibleBars、fallback/clamp；single／dense64／empty OFI；24px固定高度與DOM順序；inline label absent；超過3,000 points且marker／相鄰slot同CSS pixel時，直接hover glyph仍精確選marker slot與EventTimeUtc；一般plot axis snap不變；cursor不重掛chart；overview initial/drag palette與2px；exact package bundle manifest。
## TA row-local control layout（RFC-0030）

Renderer依 `document.Rows` 順序建立：

```fsharp
div [ dataTestId $"ta-row-control-line-{row.RowId}"; fixedSingleLine ] [
    div [ dataTestId $"ta-row-controls-{row.RowId}"; noWrap ] rowControls
    if controllableTraces.Length > 0 then
        div [ dataTestId $"ta-trace-toggles-{row.RowId}"; noWrap; rowLocalOverflowX ] traceControls
]
```

Outer `ta-row-toggles` 使用column layout；line固定高度／`overflow:hidden`。Trace region使用`flex:1 1 auto; min-width:0; overflow-x:auto; overflow-y:hidden; white-space:nowrap; flex-wrap:nowrap`，因此窄版只產生row-local水平scroll。Filter仍排除`Marker`與`OverviewStripe`，所有click/action handler保持既有實作。

測試以F# Playwright locator/BoundingBox量測：line count、descendant ownership、desktop/narrow相同高度、同row controls Y band、跨row Y band分離及narrow `scrollWidth > clientWidth`。不得用inline JavaScript或直接改generated bundle。

### Navigator local-first state flow

`pointermove -> pendingDraft <- latest -> requestAnimationFrame -> draftWindow -> viewport range afterRender receipt`。普通`pointerup`依`rendered receipt |> published draft |> pending draft |> committed`選唯一target，確保Preview、wire與authoritative final逐bar一致；若pointer明確跨出loaded boundary，則依`pending boundary |> rendered receipt |> published draft |> committed`保留clamped Start/Latest intent。清理capture後只提交一次。Preset／pan先寫`uiState`，再以nested animation frame把相同chart fields交給`chartUiState`；非chart feedback更新不得取消pending viewport render。`viewportDataReady`在full preparation開始時false、accepted prepared data完成後true，後續單純row remount不歸零；`preparedRowsReady`仍在每次chart mount前false並於所有authored rows完成後true。Overview time axis使用overview reference timeline與page display-time formatter。

Loaded projection存在時，drag session在pointerdown凍結`domainCount + global committed window`。pointermove只在該domain計算global draft並更新selection/range文字；不送callback、不重建rows。pointerup若global draft完整落在active detail，經`tryLocalWindowForLoadedCoverage`轉local window；否則以`tryLoadedCoverageWindowIntent`建立既有`ta-coverage-window.v1`。single latest queue只保存global window與feedback；排出時重新讀current projection並產生current coverage revision/query generation，禁止重送pointerdown時建立的stale action。

### Navigator overview compaction and loaded edges

`overviewPointsForCoverage`保留`High/Low option`與source range；OC／scalar為`None`。`compactOverviewCandles maximum`把ordered source切成contiguous ranges，輸出first open、last close、volume sum；只有range內全部point都有wick時才輸出max high/min low。SVG Y-domain對body-only使用open/close，wick path只讀`Some high, Some low`。

`tryLoadedCoverageEdgeIntent Start|End requested maximum projection`先驗loaded/maximum，再回既有`TaCoverageWindowIntent`。Start固定`0..count`，End固定`loaded-count..loaded`；controls共用既有`applyLoadedCoverageIntent`，因此local state、prepared-row barrier與remote latest-wins沒有分叉。

## Spa48 exact adapter release (Aster / upstream RFC-PTC-SPA-0039)
ActorRegistry/report schedule runtime source83bd67d has exactSpa0.2.48 local typed6/HTTP-PCSL/browser proof. Dynamic active package consumers require exact same Spa version; this is package compatibility, not new TA behavior. Ptcs0.1.57→0.1.58, Ptcs.Client0.1.132→0.1.133, umbrella0.1.26→0.1.27; all change only `<PackageReference Include="PulseTrade.Comm.Spa" Version="[0.2.48]"/>`. Contracts0.1.35/Renderer0.1.131/Interactive.Client0.1.123 unchanged. Legacy ACL/Login copies exactSpa18 excluded because canonical active producers are main Libs/PulseTrade.Comm/src.
Formal downstream main Spa.Host Dynamic.Ptcs58, GW.Tests umbrella27, TAResearch.Client29 with Ptcs.Client133 must cascade together. Build GeneratePackageOnBuild=false, local pack `_GetRestoreProjectStyle;GenerateNuspec` + ContinuePackingAfterGeneratingNuspec=true/NoBuild=true only after new source/DLL identity; no Pack/AfterPack/key read. Source checkpoint precedes final source-commit build; local/official/deployed gates distinct. No renderer source changes in this slice; previous own11 diagnostic attrs stay separate pending validation.
Active local package test/demo closure: Ptcs.Tests old[0.1.56]→[0.1.58], PtcsTaClient.Tests old[0.1.132]→[0.1.133], Ptcs.LiveDemo oldSpa46/Ptcs56/Client132→Spa48/Ptcs58/Client133. These known active clients must restore/build against final candidate before PASS; no source test expectations change. No Run of LiveDemo/automatic hostspawn asbuild gate. Legacy sourceACL/Login18 remain excluded.

### DYN-WBS-581 active assets
LiveDemo exactSpa48/Ptcs58/Client133並同步ContentUpdate NuGetassetpath0.2.37→0.2.48；不留compileversion與copyassetsversion不一致。Source292三nupkg finalDLL/RepositoryCommit核對，Clients loadedSpa48+83/hashAF43；Renderer131/Contracts35exact未變。失敗compilerlogs與immutable manifest保留C:artifactroot，不resetACL/停服務。

DYN-WBS-581 consumer metadata closure：LiveDemo asset path37→48修正後 fullWebSharper build6.93s/0errors/1existingWS9002 PASS；loadedSpa48+83/hashAF43及7buildassets rawSHA/copy PASS，report C:/Users/Administrator/AppData/Local/Temp/aster-ptcs-package-0.2.48/dynamic-livedemo48.asset-proof.json。未啟demoHost；local unsigned candidate的build/assetproof，不是正式部署/browser/TA semanticfix。

## Spa49 exact adapter cascade（DYN-WBS-582 / DYN-VFY-050r4）
引用位置與 snippet：
- `src/PulseTrade.Comm.Spa.Dynamic.fsproj`：`<Version>0.1.28</Version>`，Spa `[0.2.48]` → `[0.2.49]`。
- `src/PulseTrade.Comm.Spa.Dynamic.Ptcs/PulseTrade.Comm.Spa.Dynamic.Ptcs.fsproj`：Version `0.1.58` → `0.1.59`，Spa → `[0.2.49]`。
- `src/PulseTrade.Comm.Spa.Dynamic.Ptcs.Client/PulseTrade.Comm.Spa.Dynamic.Ptcs.Client.fsproj`：Version `0.1.133` → `0.1.134`，Spa → `[0.2.49]`。
- `tests/PulseTrade.Comm.Spa.Dynamic.Ptcs.Tests/PulseTrade.Comm.Spa.Dynamic.Ptcs.Tests.fsproj`：Ptcs → `[0.1.59]`。
- `tests/PtcsTaClient.Tests/PtcsTaClient.Tests.fsproj`：Ptcs.Client → `[0.1.134]`。
- `tests/PulseTrade.Comm.Spa.Dynamic.Ptcs.LiveDemo/PulseTrade.Comm.Spa.Dynamic.Ptcs.LiveDemo.fsproj`：Spa/Ptcs/Client → `[0.2.49]/[0.1.59]/[0.1.134]`；`Content Update` 的 Spa build asset path 同步 `0.2.49`。
實作只作上述精確字串替換，估 12–18 非型別行；無新 abstraction。沿用 865 input allowlist 與 raw/Git-clean blob/hash manifest；pre-checkpoint 僅允三 producer fsproj 的已記錄差異，final build 必須全數匹配固定 source HEAD。CLI 關閉 GeneratePackageOnBuild/PublishNuGetAfterPack/BuildingInsideVisualStudio，完整 WebSharper 保持啟用。用 `_GetRestoreProjectStyle;GenerateNuspec`、NoBuild=true 繞過所有 Pack/AfterPack publisher hooks。驗 archive DLL/product version/RepositoryCommit/dependencies/assets 後複製 SDK library-packs；同版本不同 bytes 必須拒絕覆蓋。消費者真 runner 須 15+17=32，LiveDemo full compiler、Spa49 DLL/raw asset copy 七檔一致。風險為 stale asset path、autocrlf raw/clean blob差異、未受 flag 保護的 Pack hook；不以 ProjectReference 或假包繞過。

DYN-WBS-582 final local closure：三 producer provenance固定 `cfb6f3b1aef86de5f8adbd676284d0d4c06f6532`；Dynamic28/Ptcs59/Client134 GenerateNuspec archives與SDK401feed同bytes，未覆寫同版不同包。865 final inputs與真compiler bundle匹配。Active三consumer使用原exact PackageReference，32tests/LiveDemo fullWeb/七copy assets全PASS；兩consumer generated JS原樣回canonical，不重新pack producer。細項見 DYN-VFY-050r4。


## Spa50 exact metadata 與固定archive（DYN-WBS-583 / DYN-VFY-050r5）
- `src/PulseTrade.Comm.Spa.Dynamic.fsproj`：`<Version>0.1.28</Version>` -> `0.1.29`；Spa `[0.2.49]` -> `[0.2.50]`。
- `src/PulseTrade.Comm.Spa.Dynamic.Ptcs/PulseTrade.Comm.Spa.Dynamic.Ptcs.fsproj`：Version `0.1.59` -> `0.1.60`；Spa -> `[0.2.50]`。
- `src/PulseTrade.Comm.Spa.Dynamic.Ptcs.Client/PulseTrade.Comm.Spa.Dynamic.Ptcs.Client.fsproj`：Version `0.1.134` -> `0.1.135`；Spa -> `[0.2.50]`。
- `tests/PulseTrade.Comm.Spa.Dynamic.Ptcs.Tests/PulseTrade.Comm.Spa.Dynamic.Ptcs.Tests.fsproj`：Ptcs `[0.1.59]` -> `[0.1.60]`。
- `tests/PtcsTaClient.Tests/PtcsTaClient.Tests.fsproj`：Ptcs.Client `[0.1.134]` -> `[0.1.135]`。
- `tests/PulseTrade.Comm.Spa.Dynamic.Ptcs.LiveDemo/PulseTrade.Comm.Spa.Dynamic.Ptcs.LiveDemo.fsproj`：Spa/Ptcs/Client -> `[0.2.50]/[0.1.60]/[0.1.135]`；`Content Update` 的 `pulsetrade.comm.spa\0.2.49` -> `0.2.50`。
只改12個metadata token，估12非型別行；無新framework。保留原BOM/mixed newline，逐檔before/after raw SHA。Precheckpoint只允三已記錄producer fsproj差異；source checkpoint後865input全匹配HEAD clean blob，同時raw SHA不得改。Full WebSharper保持；GenerateNuspec使用NoBuild/false publication flags。Archive比對ID/version/RepositoryCommit/product/DLL/deps/assets後才copy SDKfeed，existingdifferentbytes一律拒絕。Consumer由core50archive manifest定義七asset exact集合，合法零byte檔不被nonempty假條件排除。

DYN-WBS-583 / DYN-VFY-050r5 本機範圍完成：producer source `636c19b1bb62f9425591c2d2fdb543de1445a9e5`，三fixed fullWeb 10.865/8.550/8.839s；三archive/exactdeps/DLL/assets與SDK401feed逐hash相符。真15+17=32/32（0ignored/failed/errored），LiveDemo fullWeb8.309s、三consumer runtime Spa50 DLL87569E35…與core f217c898匹配、七asset exact集合/bytes/hash與七ownbundle齊全。公開發布、main Host/browser與正式部署仍由upstream另驗。 詳細來源见 `doc/Verification.md` revision5。

## DYN-WBS-584 / DYN-VFY-051r1: 最小 Pack Condition 修正
`src/PulseTrade.Comm.Spa.Dynamic.fsproj` 的 PostBuildR/PostBuildD 恢復 `and '$(PulseTradeCommSpaDynamicPushNuGet)' == 'true'`；八個 project 九個 targets 皆 append `and '$(PublishNuGetAfterPack)' != 'false'`。ACL/Contracts/Interactive.Client/Ptcs/Ptcs.Client/Renderer/Login 原 Release/Windows/專屬 true 條件及 VS defaults 保留。預估 9–12 非型別行；不修改版本、PackageReference 或 Exec。
可重用 `scripts/verify-publish-hook-guards.ps1` 接 typed ProjectPath[]/EvidenceRoot/TimeoutSeconds，零參數安全執行本 repo 八專案。逐 project 從來源複製專屬 properties/target Condition 到無 SDK/import 的 fixture，全部原 task body 換為 WriteLinesToFile marker。單一真 MSBuild driver 評估每案獨立 fixture；20 cases/project 覆蓋 CLI/VS、false/global false、positive true、Debug/non-Windows/other Configuration。保留 source before/after SHA、raw source copies、stdout/stderr/result，任何來源漂移或 engine failure 都不得 finalized。無真 Pack/build/restore/Exec/key/network 行為；main 可重用同 script 驗自己的 targets。

DYN-WBS-584 實作量為9條Condition、154行typed PS1 verifier；final PS7/PS5各160/160。XML差異核對與BOM檢查證明 defaults/Exec/version/reference未變；只有核准gate與兩個舊comment移除。證據見DYN-VFY-051r1。

完整追溯：RFC-PTCS-DYNAMIC-0035 / DYN-WBS-584 / DYN-T-679 / DYN-VFY-051r1。


## DYN-WBS-585 / DEP52 exact adapter cascade

本輪承接 core ACT-A2 actor projection / bounded maintenance，維持既有 package 邊界及發布流程。精確 metadata 共 7 檔、14 literal；不改 runtime source、Contracts 0.1.35、Renderer 0.1.131、Interactive.Client 0.1.123 或 legacy ACL/Login 0.1.1。Renderer.fs 並行 WIP 與 PCSL/Playwright 資料保留。

| 相對路徑 | old → new |
| --- | --- |
| src/PulseTrade.Comm.Spa.Dynamic.fsproj | Version 0.1.29 → 0.1.30；Spa [0.2.50] → [0.2.52]；Actor.Registry [0.1.3] → [0.1.4] |
| src/PulseTrade.Comm.Spa.Dynamic.Ptcs/PulseTrade.Comm.Spa.Dynamic.Ptcs.fsproj | Version 0.1.60 → 0.1.61；Spa [0.2.50] → [0.2.52] |
| src/PulseTrade.Comm.Spa.Dynamic.Ptcs.Client/PulseTrade.Comm.Spa.Dynamic.Ptcs.Client.fsproj | Version 0.1.135 → 0.1.136；Spa [0.2.50] → [0.2.52] |
| tests/PulseTrade.Comm.Spa.Dynamic.Tests.fsproj | Actor.Registry [0.1.3] → [0.1.4] |
| tests/PulseTrade.Comm.Spa.Dynamic.Ptcs.Tests/PulseTrade.Comm.Spa.Dynamic.Ptcs.Tests.fsproj | Ptcs [0.1.60] → [0.1.61] |
| tests/PtcsTaClient.Tests/PtcsTaClient.Tests.fsproj | Client [0.1.135] → [0.1.136] |
| tests/PulseTrade.Comm.Spa.Dynamic.Ptcs.LiveDemo/PulseTrade.Comm.Spa.Dynamic.Ptcs.LiveDemo.fsproj | Spa/Ptcs/Client exact 新版；Content Update 的 pulsetrade.comm.spa\0.2.50 目錄 → 0.2.52 |

Registry 的 PrivateAssets/ExcludeAssets 與既有 test ProjectReference 均不變。先前長路徑失敗已知，復用 865 個 allowlisted raw inputs 的 SHA/clean Git blob 檢查，將 build 放在 C:/ptc-d52 的隔離短路徑；不複製 dirty Renderer 或既有 bin/obj/cache。GeneratePackageOnBuild=false、PublishNuGetAfterPack=false、BuildingInsideVisualStudio=false；full WebSharper 保留。未有 root 固定 Spa52/Registry4 local proof 與候選放行前不 build/pack 新版；package 僅本機 immutable candidate，不 upload。

依賴風險與驗收：source checkpoint → 固定 inputs / 3 producer full build → exact deps/DLL/assets/RepositoryCommit → 必要 consumers。除原 Ptcs/Client 15+17 與 LiveDemo，直接 Registry 引用異動的 umbrella tests 亦須執行並核實 runner counts。細節及各 gate 實況集中 DYN-T-680 / DYN-VFY-050r6；歷史 Spa50 已公開結果不等於本輪52完成。
