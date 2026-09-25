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

Test seams：deterministic encoder/legacy singleton；zero-item snapshot；missing/duplicate/out-of-order/mismatch；interleaved legacy；generation/disconnect；invalid SduiValue/canonical reducer failure；commit-only publish/accepted lifecycle/cache eligibility；4,000×28×5 candle target-renderer long-task；legacy/cache regression；exact package graph。

Current exact graph：Contracts `0.1.22` → Renderer `0.1.51` → Interactive.Client `0.1.43`；Dynamic.Ptcs `0.1.44` exact Contracts；Ptcs.Client `0.1.65` exact Contracts/Renderer；兩個PTCS adapters維持PTCS `[0.2.46]`。大型candle projection以固定bucket array單次聚合；line projection以固定bucket min/max arrays保留chronological extrema；Y-domain以first-value initialized accumulator單次掃描，避免WebSharper對Infinity sentinel的錯誤轉譯。Shared-cursor click只查accepted prepared timeline，不再從raw data重做projection。Prepared geometry同時保存每條trace的latest presentation；當shared cursor未固定時，visible-value refresh直接讀此cache，不再對missing/sparse higher-scale trace逐條自tail反向掃描。Source interval、cursor action payload、bounded cursor lookup與Y-domain padding語意不變。
