# UPSTREAM RFC: Extension Points for PulseTrade.Comm.Spa.Dynamic

## 1. 摘要 (Abstract)
為了讓 `PulseTrade.Comm.Spa` (PTCS) 的核心保持精簡，並將 `SDUI/FSkynet` 等動態渲染與 Actor 處理邏輯抽離至外部 Extension NuGet Package (`PulseTrade.Comm.Spa.Dynamic`)，我們需要上游 (Upstream) 也就是 `G:\PulseTrade2.fs` 專案提供對應的**掛載點 (Extension Points)**。

本 RFC 定義了在 `PulseTrade.Comm.Spa` 中需要被修改或新增的 API，以便我們的 Extension 專案能夠順利掛載。

## 2. 前端 (Client-Side) 掛載點需求

目前的 `Client.fs` 中，處理 `fskynet-sdui` 字串以及 SDUI Canvas UI 的邏輯是寫死的。
我們需要將其拔除，並提供一個全域註冊表 (Registry) 讓擴充套件註冊自訂的訊息渲染器。

### 2.1 介面與註冊表定義
請在 `PulseTrade.Comm.Spa.Client` (或適合的模組) 內新增：

```fsharp
type IMessageRenderer =
    abstract member TryRender: sourceText: string -> WebSharper.JavaScript.Dom.Node option

let mutable private customRenderers : IMessageRenderer list = []

[<JavaScript>]
let RegisterRenderer (renderer: IMessageRenderer) =
    customRenderers <- renderer :: customRenderers
```

### 2.2 攔截渲染邏輯
在 `Client.fs` 的 `renderFCell2Rendered` 或 `chatMessageBubble` 處理 `mode = _` 預設回退 (Fallback) 文字的區段中：

**修改前：**
```fsharp
if sourceText.Contains("\"fskynet-sdui\"") then
    // 寫死的 createSduiSummaryCard
else
    // 預設的 <pre>
```

**修改後：**
```fsharp
let mutable handled = false
for renderer in customRenderers do
    if not handled then
        match renderer.TryRender(sourceText) with
        | Some node ->
            card.AppendChild node |> ignore
            handled <- true
        | None -> ()

if not handled then
    card.AppendChild(element "pre" "fcell-source" sourceText) |> ignore
```

## 3. 後端 (Server-Side) 掛載點需求

`PulseTrade.Comm.Spa.Dynamic` 會包含如 `ShowcaseDemoActor` 這種實驗性或高階動態的 Actor。
雖然 Akka.NET 的架構本來就允許自由 Spawn Actor，但為了讓依賴管理更乾淨，我們期望提供一個簡單的 Fluent Extension API，讓主程式的 `CommHub` 可以掛載動態路由與 Actor。

### 3.1 擴充方法介面
建議 `CommHub` 提供可供 Extension 呼叫的註冊/擴充介面，或者單純讓 Extension 自己定義 `CommHub.useDynamicSdui(hub)` 的 extension method，而不需要改動上游核心。
由於 F# 允許在外部專案宣告針對 `CommHub` 的 Type Extension，後端幾乎**不需要**修改，唯一需要的是將 `FAkka.FCell2` 相關的基礎模型 (如果被拔除的話) 正確定義於依賴中。
(上游只要確保 `CommHub` 是可以讓外部透過 Extension Method 進行擴充與 `ActorSystem` 存取即可，目前應該已符合需求。)

## 4. 影響範圍與相容性
- **相容性**：前端引入 `RegisterRenderer` 是 Non-breaking change，原先寫死的 SDUI 程式碼可以直接刪除，轉由 Dynamic 擴充載入，或作為核心的 Default Renderer 註冊。
- **依賴乾淨**：PTCS 核心將不必理解什麼是 `fskynet-sdui`，達到職責分離。
