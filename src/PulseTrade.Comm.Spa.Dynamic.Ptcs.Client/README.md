# PulseTrade.Comm.Spa.Dynamic.Ptcs.Client

Pure WebSharper F# client adapter for the PTCS same-session transient channel.

## Boundary

- Connects only to the current origin `/sync/ws`; callers cannot inject an upstream URL or credential.
- Sends `extension-transient` frames and consumes `ta-browser.v1..v4` columnar payloads；v4 reconstructs validated `TemporalPoint` values。
- Projects bounded, non-recursive browser wire data into `RuntimeState` and renders through `PulseTrade.Comm.Spa.Dynamic.Renderer`.
- Projects server query metadata into `TaWorkspaceDocument.DefaultView`；browser不再自行發明TXF/5m/date defaults。
- Does not own PTCS authentication, ACL, SQL, market-data access, persistence, or the canonical reducer.

## API

```fsharp
TaResearchTransientClient.mountById
    "app"
    "ta-research"
    "workspace-main"
    "canvas-main"
```

需要由host page控制active/dispose時使用：

```fsharp
let handle =
    TaResearchTransientClient.mountByIdWithOptions
        "app"
        "ta-research"
        "workspace-main"
        "canvas-main"
        TaClientLifecycle.defaults

handle.SetActive false
handle.SetActive true
handle.RequestJsonExport()
handle.Dispose()
```

client lifecycle保證one-in-flight action/poll、timeout/backoff、bounded reconnect、full snapshot resync與terminal dispose。server-side validation rejection是`CommandRejected`，不會關閉仍健康的WebSocket；它只使用same-origin WebSocket，不做HTTP polling。

TA durable reply只保存compact layout/query descriptor，不含OHLCV或indicator arrays。`RequestJsonExport`會要求authoritative full wire，browser下載`yyyyMMddHHmmss-<GUID>.json`，其中含timeline、OHLCV、indicator series、query/provider metadata與revision。collapsed reply平時仍為zero-resource；明確點下載時使用`requestJsonExportOnce`建立bounded headless channel，完成後立即dispose。

The PTCS host must register the same `extensionId` with `PulseTrade.Comm.Spa.Dynamic.Ptcs.TaResearchTransientServer.register`.

Current exact package is `PulseTrade.Comm.Spa.Dynamic.Ptcs.Client 0.1.0-alpha8-win68`, consuming Contracts `[0.1.0-alpha11]` and Renderer `[0.1.0-alpha32]`。Browser revision fields use JSON-safe numbers and are validated back to domain `int64` at the server boundary；dispose waits for the transient close response before closing its dedicated socket。Add/Remove Row、Change Query、full-data export與Reset Canvas都走同一one-in-flight typed action channel。Export在data revision為0時先送一次immediate PollDelta取得snapshot，再要求authoritative full state；不依賴固定poll sleep。Initial transient bootstrap now renders as lifecycle progress rather than `document unavailable`。
