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

Current exact package：`PulseTrade.Comm.Spa.Dynamic.Interactive.Client 0.1.0-alpha9`，exact依賴Contracts `[0.1.0-alpha16]`、Renderer `[0.1.0-alpha38]`與FSharp.Core `[10.1.400]`；bundle manifest版本須與nuspec一致。這是Daedalus production consumer唯一相容組。
