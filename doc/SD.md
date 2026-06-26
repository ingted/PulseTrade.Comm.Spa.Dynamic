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
- 目前 `src/PulseTrade.Comm.Spa.Dynamic.fsproj` 以 local `ProjectReference` 參考 `G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\PulseTrade.Comm.Spa.fsproj`；NuGet pack 時會產生對 `PulseTrade.Comm.Spa` package version `0.2.5-beta15` 的 dependency。本輪 package 化先使用自己編譯的 PTCS，再一起 push PTCS / PTCS.Dynamic。
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
- add-key dialog renderer：回傳 `actorAddress :: duTypeName :: unionCaseNames`；
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

type SduiNode =
    | Stack of id: string * children: SduiNode list
    | Section of id: string * title: string option * children: SduiNode list
    | TextBlock of id: string * text: string
    | Input of id: string * label: string * kind: string * binding: string
    | Select of id: string * label: string * options: SduiOptionSource * binding: string
    | Button of id: string * label: string * actionId: string

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

### 5.2 Target resolver

```fsharp
type DynamicTarget =
    | DirectDslTarget of actorAddress: string * formDslId: string
    | ArguTemplateTarget of actorAddress: string * duTypeName: string * unionCaseNames: string list

module DynamicTargetKey =
    val tryParse : string list -> Result<DynamicTarget, string>
```

Parse rules：

1. key list length must be at least 2；
2. first item is actor address；
3. second item is looked up in Form DSL registry first；
4. if not found, second item is looked up in Argu adapter registry；
5. remaining tail belongs to the matched resolver；
6. unknown second item returns controlled error。

### 5.3 Argu-to-FormDsl adapter

```fsharp
type ArguTemplateRegistration =
    { DuTypeName: string
      TemplateType: Type
      AllowedUnionCases: string list option }

module ArguToFormDsl =
    val generate : ArguTemplateRegistration -> requestedCases: string list -> SduiDocument
```

Each requested union case becomes a visible form section with its own inputs and submit button。The adapter maps:

- string -> text input；
- numeric -> number input；
- bool flag -> checkbox；
- enum / zero-field DU enum -> select；
- tuple -> ordered input group；
- list -> repeatable input group；
- nested `ParseResults<'T>` -> nested section when supported, otherwise controlled unsupported-field error。

### 5.4 Backend-linked options

`QueryOptions(providerId, dependsOn)` is a declared provider lookup。Renderer may call the PTCS core safe extension query callback only for registered providers。The DSL must not contain arbitrary URL, headers, tokens, script text or executable code。

### 5.5 PTCS.Host demo integration

PTCS.Host registers:

1. a direct form DSL target derived from `example DU.txt`；
2. an Argu adapter target for a host-local `PFCF_AKKA_CMD` demo subset；
3. a durable proxy/echo actor target for E2E。

`example DU.txt` is cp950 encoded and contains Chinese identifiers/comments。The host demo should either preserve valid identifiers or map them to stable ASCII labels while keeping display labels in metadata。Missing external types such as `DataTypeT.RTTables` must be represented by host-local stubs or excluded from the first demo subset with a documented controlled unsupported-case message。
