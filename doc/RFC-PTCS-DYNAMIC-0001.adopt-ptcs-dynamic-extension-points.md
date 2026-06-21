# RFC-PTCS-DYNAMIC-0001 Adopt PTCS Dynamic Extension Points

狀態：Proposed

日期：2026-06-22

## 背景

`PulseTrade.Comm.Spa.Dynamic` 目前已能在 server 端透過 `hub.useDynamicSdui(fabric.System)` 建立展示 actor，但 PTCS core 在 `0.2.4-beta7` 前缺少 browser 端 extension points，因此 `/chat` 無法知道：

- `actor-dynamic` tab shape；
- Dynamic browser renderer；
- `fskynet-sdui` payload 要先交給 Dynamic renderer。

PTCS 端已依 `RFC-PTC-SPA-0006.dynamic-client-extension-points.md` 補上：

- `CommHub.RegisterClientExtension` / `ListClientExtensions`；
- `CommHub.RegisterClientExtensionScriptAsset` / `TryGetClientExtensionScriptAsset`；
- `/chat` manifest injection 與 same-origin script tag injection；
- browser `Client.RegisterAppendPageShape`；
- browser `Client.RegisterRenderer`；
- custom safe shape persistence。

## 目標

1. Dynamic package 以 PTCS 新 extension points 完成真正的 browser integration。
2. 現有 `hub.useDynamicSdui(fabric.System)` 用法維持可用：即使 Suave server 已經 start，Dynamic 仍可透過 hub 註冊 manifest 與 runtime script asset。
3. `/chat` page creator 下拉選單應顯示 `Actor Dynamic`。
4. `fskynet-sdui` payload 應由 Dynamic renderer 顯示，不再只落回 plain text。

## 非目標

1. Dynamic 不 fork PTCS core。
2. Dynamic 不改 PTCS OAuth / PCSL / journal / Akka delivery 行為。
3. Dynamic 不在此 RFC 重新設計 FSkynet AST；只把既有 renderer 接到 PTCS public API。

## 設計

### Server extension method

`CommHub.useDynamicSdui(actorSystem)` 應做三件事：

1. 建立或確認 `/user/showcase-dynamic-actor`。
2. 將 Dynamic browser bundle 內容註冊為 runtime script asset：

```fsharp
hub.RegisterClientExtensionScriptAsset(
  { Url = "/client-extensions/dynamic/PulseTrade.Comm.Spa.Dynamic.js"
    ContentType = "text/javascript"
    Content = dynamicBrowserBundleText })
```

3. 註冊 browser manifest：

```fsharp
hub.RegisterClientExtension(
  { ExtensionId = "pulse-trade-comm-spa-dynamic"
    DisplayName = Some "PulseTrade.Comm.Spa.Dynamic"
    ScriptUrls = [ "/client-extensions/dynamic/PulseTrade.Comm.Spa.Dynamic.js" ]
    AppendPageShapes =
      [ { Shape = "actor-dynamic"
          Label = Some "Actor Dynamic"
          Badge = Some "D"
          ClassName = Some "actor-dynamic" } ] })
```

### Browser bundle

Dynamic project 需要產生 browser 可載入的 JavaScript bundle，並把內容以 embedded resource 或 package content 方式交給 `useDynamicSdui`。

最低要求：

- bundle 載入後呼叫 `PulseTrade.Comm.Spa.Client.RegisterAppendPageShape("actor-dynamic", "Actor Dynamic", "D", "actor-dynamic")`；
- bundle 載入後呼叫 `PulseTrade.Comm.Spa.Client.RegisterRenderer(...)`；
- renderer 只處理 `schema = "fskynet-sdui"` 的 payload，其他 payload 回 `None`。

### Renderer return type

PTCS public API 的 renderer contract 是：

```fsharp
string -> WebSharper.JavaScript.Dom.Node option
```

Dynamic 目前 `DynamicRenderer.TryRender` 若仍回 `WebSharper.UI.Doc option`，需在 Dynamic 端提供 bridge，或直接改為 DOM node renderer。

## 驗收

1. `poc.dynamic.fsx` 執行 `hub.useDynamicSdui(fabric.System)` 後開啟 `/chat`，page creator 下拉選單可見 `Actor Dynamic`。
2. `ShowcaseDemoActor` 回傳的 `fskynet-sdui` JSON 在 browser 中由 Dynamic renderer 顯示。
3. 未命中的 plain text / fCell payload 仍由 PTCS core fallback rendering 顯示。
4. `hub.useDynamicSdui` 在 server start 後呼叫仍可生效；不要求 caller 另外修改 Suave route。

## 需要注意

1. 若 Dynamic bundle 是 WebSharper 產物，需確認 package build 會產生 stable JS 檔或 embedded resource。
2. PTCS core 不會替 Dynamic 產生 JS；Dynamic 必須自己管理 bundle source。
3. 若將來 Dynamic 需要 CSS，應同樣走 runtime asset 或明確的 PTCS asset extension RFC，不要把 CSS 字串塞入 renderer。
