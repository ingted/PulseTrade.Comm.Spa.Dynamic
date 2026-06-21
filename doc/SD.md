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
│   │   ├── Actors.fs          (包含 ShowcaseDemoActor)
│   │   └── Extension.fs       (包含 CommHub 擴充與掛載介面)
│   └── Client
│       └── DynamicRenderer.fs (WebSharper 渲染器與 DOM 生成邏輯)
└── tests
    ├── PulseTrade.Comm.Spa.Dynamic.Tests.fsproj
    └── Program.fs
```

## 2. API 介面設計 (Interface Specification)

### 2.1 後端擴充點 (Server Extension)
```fsharp
namespace PulseTrade.Comm.Spa.Dynamic

open PulseTrade.Comm.Spa

[<AutoOpen>]
module CommHubExtensions =
    type CommHub with
        /// 將 Dynamic Sdui Actor 與路由掛載至現有的 CommHub
        member this.useDynamicSdui() =
            // 註冊 ShowcaseDemoActor
            // 進行需要的環境設定
            this
```

### 2.2 前端渲染器 (Client Renderer)
```fsharp
namespace PulseTrade.Comm.Spa.Dynamic.Client

open WebSharper
open WebSharper.JavaScript
open PulseTrade.Comm.Spa.Client // (依據 UPSTREAM_RFC 假定有的 API)

[<JavaScript>]
module DynamicSduiRenderer =
    
    let private sduiRenderer =
        { new IMessageRenderer with
            member _.TryRender(sourceText: string) =
                if sourceText.Contains("\"schema\":\"fskynet-sdui\"") then
                    // 原本在 Client.fs 寫死的 createSduiSummaryCard 與解析邏輯
                    let card = createSduiSummaryCard sourceText
                    Some (card :> Dom.Node)
                else
                    None
        }

    let Start () =
        PulseTrade.Comm.Spa.Client.RegisterRenderer(sduiRenderer)
```

## 3. 類別庫封裝與相依 (NuGet Packaging)
- Target Framework: `net10.0`
- 透過 `<PackageReference Include="PulseTrade.Comm.Spa" Version="0.2.4-beta7" />` 將基礎庫引入。
- 透過 WebSharper 將 `DynamicRenderer.fs` 翻譯為前端 JS。
