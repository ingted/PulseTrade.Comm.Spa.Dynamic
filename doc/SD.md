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
- 透過 `<PackageReference Include="PulseTrade.Comm.Spa" Version="0.2.4-beta7" />` 將基礎庫引入。
- 透過 WebSharper 將 `Client/*.fs` 翻譯為前端 JS，並保證 `ActorDynamicTab.Start()` 能在 PTCS 核心啟動時正確呼叫。
