# PulseTrade.Comm.Spa.Dynamic.Interactive.Client

Interactive notebook iframe 的 WebSharper browser client。它只處理三件事：接收 authoritative `RuntimeFrame`、以 shared reducer 更新 SDUI、透過 correlated action channel 將使用者 mutation 送回 host。

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
