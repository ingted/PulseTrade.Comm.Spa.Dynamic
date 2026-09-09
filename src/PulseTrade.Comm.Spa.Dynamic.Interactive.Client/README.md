# PulseTrade.Comm.Spa.Dynamic.Interactive.Client

Interactive notebook iframe 的 WebSharper browser client。它只處理三件事：接收 authoritative `RuntimeFrame`、以 shared reducer 更新 SDUI、透過 correlated action channel 將使用者 mutation 送回 host。

NuGet package自`0.1.0-alpha3`起在`contentFiles/any/any/ptcs-dynamic-interactive/`攜帶可直接serve的`client.js`、`client.min.js`、`WebSharper.Core.JavaScript/Runtime.js`與`bundle.manifest.json`。Application module已包含Contracts/Renderer/UI/FSharp邏輯，唯一external import是同目錄樹內的Runtime；host不需建立assembly resource/module graph server。

## Action wire

Browser 送出：

```fsharp
{ Protocol = "ptcs-dynamic-action.v1"
  Kind = "action-request"
  Request =
    { RequestId = "<canvas>:ui:<sequence>"
      ExpectedDocumentRevision = Some revision
      Action = action } }
```

Host 必須回傳同一個 `RequestId` 的 `DynamicActionServerFrame`，其 result 為 `Accepted`、`Rejected` 或 `RevisionConflict`。`Accepted` 只解除 pending 狀態；真正的 document/data mutation 仍必須由 host 發布後續 `RuntimeFrame`，client 不做 optimistic state mutation。

Client 同時間只保留一個 pending request。socket 未開啟、30 秒 timeout、disconnect、send failure 或 correlation mismatch 都會回傳 `DynamicHostError` 並 fail closed。既有 `RuntimeClientFrame` 的 mounted/unmounted/resync path 保持相容。

## Application lifecycle

SPA entry point會呼叫`Client.Application.start()`。Host需要自行管理生命週期時可使用相同single-instance API：

```fsharp
let handle =
    Client.Application.startWithOptions
        { Client.defaultOptions with
            RootElementId = "app"
            StatusElementId = "sdui-runtime-status" }

// idempotent；不會建立第二條WebSocket
handle.Start()

// terminal；送Unmounted、取消timer、關閉transport且不再重連
handle.Dispose()
```

client只從目前page的`/view/`推導same-origin`/frames/` WebSocket，不接受remote URL、header或credential。transport close會保留原`RuntimeState`與renderer，以1秒起、30秒封頂的bounded backoff重連。replacement transport有既有runtime identity時只送一次Mounted與`RequestFullSnapshot`；valid Snapshot前畫面維持last-good。snapshot逾時會淘汰該socket generation並重連；舊generation callback不得排第二個timer或覆蓋新state。

## Host integration

Host WebSocket receive loop 應先嘗試：

```fsharp
match BrowserRuntimeCodec.decodeActionRequest text with
| Ok request ->
    let! result = executeAgainstAuthoritativeState request
    socket.Send(BrowserRuntimeCodec.encodeActionResult result)
| Error _ ->
    match BrowserRuntimeCodec.decodeClient text with
    | Ok legacyFrame -> handleLegacyFrame legacyFrame
    | Error message -> rejectMalformedFrame message
```

Host 不得只收到 request 就回 `Accepted`。它必須先檢查 `ExpectedDocumentRevision`、完成 provider/resource prepare 與 authoritative swap，再回結果並發布新 revision。

authoritative document必須同時帶`Rows`與`EditorSchemas`。可修改row帶`ptcs.dynamic.editor.binding.v1`；client由document catalog生成Add/Edit表單，不接受host另外維護一份renderer-local schema registry。Edit request保留stable RowId，Host完成原位resource transition後以同一份new document revision發布row與binding。

Current exact package：`PulseTrade.Comm.Spa.Dynamic.Interactive.Client 0.1.3`，exact依賴Contracts `[0.1.1]`、Renderer `[0.1.3]`與FSharp.Core `[10.1.400]`；bundle manifest版本須與nuspec一致。Host需處理`SharedCursorChanged`與`VisibleRangeChanged`，accepted只解除pending，authoritative range/data仍由後續RuntimeFrame提交。Reconnect只有在authoritative Snapshot被接受後才重設backoff，單純WebSocket open不視為恢復成功。shared temporal axis使3,820 bars x 28 scalar series只傳一份時間metadata，client retention hard cap為4,000。
