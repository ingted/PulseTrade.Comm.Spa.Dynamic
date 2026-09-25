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

Host 必須回傳同一個 `RequestId` 的 `DynamicActionServerFrame`，其 result 為 `Accepted`、`Rejected` 或 `RevisionConflict`。`Accepted` 不可直接修改document/data；真正的 mutation仍必須由host發布`RuntimeFrame`。Renderer唯一的presentation例外是accepted `ChangeTaQuery`：在callback已合併同批frames後，依current temporal axis選local viewport，不改authoritative state。

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

let latest = handle.GetLastProjectionCommit()
let unsubscribe = handle.SubscribeProjectionCommitted(fun receipt -> consume receipt)

// terminal；送Unmounted、取消timer、關閉transport且不再重連
handle.Dispose()
```

每次current projection完成時，client同時更新application root的`data-ptcs-runtime-commit-*`level watermark，並dispatch bubbling `ptcs-dynamic-runtime-committed-v1`edge event；兩者內容同源。晚加入的consumer讀level，已先訂閱者讀edge或typed subscription。application重建／dispose不沿用舊canvas watermark，consumer必須以identity及`DataRevision >= expected`判斷完成。

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

## Large initial frame batches

HTTP/Notebook host一次收到多個encoded `RuntimeFrame`時，不應在同一browser callback內同步decode、reduce、cache projection與write。使用frame pump先建立完整candidate，完成前不發布partial state：

```fsharp
let generation = currentGeneration

BrowserRuntimeFramePump.reduceIsolatedEncodedFrames
    response.Frames
    (fun () -> generation = currentGeneration)
    (function
        | BrowserRuntimeFramePumpOutcome.Applied candidate ->
            runtimeState.Value <- candidate

            BrowserRuntimeCache.writeAcceptedStatePhased
                cacheIdentity
                candidate
                (fun () -> generation = currentGeneration)
                ignore
        | BrowserRuntimeFramePumpOutcome.Rejected failure ->
            report failure.Code failure.Message
        | BrowserRuntimeFramePumpOutcome.Superseded -> ())
```

`reduceFromEncodedFrames`用於已有authoritative state的batch；`reduceIsolatedEncodedFrames`用於新run。每個frame的decode與reduce分屬不同animation-frame task，全部成功才回`Applied`。generation失效回`Superseded`；decode、resync或structured reducer denial回`Rejected`且保留last-good。`writeAcceptedStatePhased`再把finalized projection與IndexedDB encode/write分成兩個task，並於write開始前再驗generation。既有同步`writeAcceptedState`保留相容性，不適合大型initial state。

Production WebSocket `OnMessage`只enqueue；單一requestAnimationFrame pump依socket generation處理legacy frame或chunked `start / item / commit`。chunk framing與ordered batch transition由Contracts的`RuntimeSnapshotTransportAssembler`唯一決定；Interactive.Client只保留requestAnimationFrame分段`SduiValue` decode、canonical reducer與commit後publish，避免browser與machine consumer分叉協議。initial與reconnect producer都應先以`RuntimeSnapshotTransportCodec.encodeFrames`展開再flatten；invalid、stale或中斷batch保留last-good並只要求authoritative resync，不建立第二個pump。

Current public release：`PulseTrade.Comm.Spa.Dynamic.Interactive.Client 0.1.61`，exact依賴Contracts `[0.1.28]`、Renderer `[0.1.70]`與FSharp.Core `[10.1.400]`；`0.1.60`已作廢，bundle manifest版本須與nuspec一致。此bundle包含Renderer的fixed CSS-pixel stroke、獨立overview visual/hit targets，以及所有chart authored/default plot height `<=250px`、manual resize可超過的presentation contract。runtime frame沿用同一reducer/renderer，unknown kind/version與invalid candidate fail closed並保留last-good；合法大型snapshot使用Contracts allocation-light安全掃描。live preview只更新實際變動的SVG element/trace與row-value band；mousemove由Renderer的單一rAF固定DOM hot path處理。Host需處理`SharedCursorChanged`與`VisibleRangeChanged`；authoritative range/data仍由RuntimeFrame提交。`BrowserRuntimeCache`只作display-first last-good projection且由consumer顯式寫入。SPAA與DIB共用此bundle，不另做consumer overlay。

RFC-0027 local candidate為Interactive.Client `0.1.62`，exact依賴Contracts `[0.1.29]`與Renderer `[0.1.71]`。bundle直接包含typed dark plot、typed Histogram polarity及Overview edge-stroke修正；consumer只需升Contracts與Interactive.Client direct references，不新增Renderer direct reference。真SPAA GREEN與public push前，current public release仍是上一段的`0.1.61`。
