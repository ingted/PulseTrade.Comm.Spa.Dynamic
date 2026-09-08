namespace PulseTrade.Comm.Spa.Dynamic.Interactive.Client.LiveDemo

open System
open System.IO
open System.Net.WebSockets
open System.Text
open System.Threading
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open PulseTrade.Comm.Spa.Dynamic.Contracts

module Program =
    let gate = obj()
    let mutable currentSocket: WebSocket option = None
    let mutable connectionCount = 0
    let mutable activeConnections = 0
    let mutable maximumActiveConnections = 0
    let mutable mountedCount = 0
    let mutable fullSnapshotRequestCount = 0
    let mutable unmountedCount = 0
    let mutable frameCount = 0

    let canvasId = CanvasInstanceId "interactive-lifecycle-canvas"
    let documentId = DocumentId "interactive-lifecycle-document"

    let row =
        { RowId = "price"
          Kind = TaRowKind.Candlestick
          DataRef = "series.price"
          HeightWeight = 1.0
          Visible = true
          Options = Map.empty
          Traces = [||] }

    let document =
        { WorkspaceId = "interactive-lifecycle"
          Title = "Interactive lifecycle workspace"
          RowsRef = "rows"
          StatusRef = "ta.status"
          SharedTimeAxis = true
          BaseRowId = Some "price"
          Rows = [| row |]
          EditorSchemas = [||]
          AllowedActions = [| "request-full-snapshot" |]
          DefaultView = Map.empty }

    let candle revision index =
        let eventTime = DateTimeOffset(2026, 9, 8, 0, index, 0, TimeSpan.Zero)
        let startTime = eventTime.ToString("O")
        let endTime = eventTime.AddMinutes(1.0).ToString("O")
        let baseline = 21800.0 + float revision * 20.0 + float index

        SduiValue.Object(
            Map [ "_type", SduiValue.Text "temporal-point.v1"
                  "sourceIntervalId", SduiValue.Text($"lifecycle:{revision}:{index}")
                  "scaleKey", SduiValue.Text "1K"
                  "intervalStartUtc", SduiValue.Text startTime
                  "intervalEndUtc", SduiValue.Text endTime
                  "observedThroughUtc", SduiValue.Text endTime
                  "availableAtUtc", SduiValue.Text endTime
                  "finality", SduiValue.Text "final"
                  "projection", SduiValue.Text "candle-span"
                  "quality", SduiValue.Text "complete"
                  "value",
                  SduiValue.Object(
                      Map [ "o", SduiValue.Number baseline
                            "h", SduiValue.Number(baseline + 4.0)
                            "l", SduiValue.Number(baseline - 3.0)
                            "c", SduiValue.Number(baseline + 1.5)
                            "v", SduiValue.Number(900.0 + float index) ]) ])

    let status label =
        SduiValue.Object(
            Map [ "label", SduiValue.Text label
                  "freshness", SduiValue.Text "live"
                  "watermarkUtc", SduiValue.Text(DateTimeOffset.UtcNow.ToString("O"))
                  "quality", SduiValue.Text "complete" ])

    let frame kind sequence dataRevision payload =
        { Protocol = DynamicRuntimeDefaults.protocol
          Kind = kind
          DocumentId = documentId
          CanvasInstanceId = canvasId
          DocumentRevision = 1L
          BaseDataRevision = None
          DataRevision = dataRevision
          TransportSequence = sequence
          Payload = payload }

    let documentFrame = frame RuntimeFrameKind.Document 1L 0L (RuntimePayload.Document document)

    let snapshotFrame revision sequence label =
        let snapshot =
            { Data =
                Map [ "series.price", SduiValue.Array(Array.init 24 (candle revision))
                      "ta.status", status label ]
              Freshness = TaFreshness.Live }

        frame RuntimeFrameKind.Snapshot sequence revision (RuntimePayload.Snapshot snapshot)

    let sendText (socket: WebSocket) (text: string) =
        task {
            let bytes = Encoding.UTF8.GetBytes text
            do! socket.SendAsync(ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None)
        }

    let sendFrame socket runtimeFrame =
        task {
            do! sendText socket (BrowserRuntimeCodec.encode runtimeFrame)
            lock gate (fun () -> frameCount <- frameCount + 1)
        }

    let readText (socket: WebSocket) =
        task {
            let buffer = Array.zeroCreate<byte> 16384
            use stream = new MemoryStream()
            let mutable completed = false
            let mutable closed = false

            while not completed && not closed do
                let! result = socket.ReceiveAsync(ArraySegment<byte>(buffer), CancellationToken.None)
                closed <- result.MessageType = WebSocketMessageType.Close

                if not closed then
                    stream.Write(buffer, 0, result.Count)
                    completed <- result.EndOfMessage

            if closed then
                return None
            else
                return Some(Encoding.UTF8.GetString(stream.ToArray()))
        }

    let receiveLoop (socket: WebSocket) connectionOrdinal =
        task {
            let mutable running = true

            while running && socket.State = WebSocketState.Open do
                try
                    let! message = readText socket

                    match message with
                    | None -> running <- false
                    | Some text ->
                        match BrowserRuntimeCodec.decodeClient text with
                        | Ok(RuntimeClientFrame.Mounted _) ->
                            lock gate (fun () -> mountedCount <- mountedCount + 1)
                        | Ok(RuntimeClientFrame.Unmounted _) ->
                            lock gate (fun () -> unmountedCount <- unmountedCount + 1)
                            running <- false
                        | Ok(RuntimeClientFrame.Action(SduiAction.RequestFullSnapshot _)) ->
                            lock gate (fun () -> fullSnapshotRequestCount <- fullSnapshotRequestCount + 1)
                            do! Task.Delay 1200

                            if socket.State = WebSocketState.Open then
                                do! sendFrame socket (snapshotFrame 2L 3L "RESYNCED V2")
                        | _ -> ()
                with
                | :? WebSocketException -> running <- false
                | :? OperationCanceledException -> running <- false

            lock gate (fun () ->
                activeConnections <- max 0 (activeConnections - 1)

                match currentSocket with
                | Some current when obj.ReferenceEquals(current, socket) -> currentSocket <- None
                | _ -> ())

            if socket.State = WebSocketState.Open || socket.State = WebSocketState.CloseReceived then
                try
                    do! socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "fixture-complete", CancellationToken.None)
                with _ -> ()

            printfn "interactive lifecycle websocket closed ordinal=%d" connectionOrdinal
        }

    let handleWebSocket (context: HttpContext) =
        task {
            if not context.WebSockets.IsWebSocketRequest then
                context.Response.StatusCode <- StatusCodes.Status400BadRequest
            else
                let! socket = context.WebSockets.AcceptWebSocketAsync()
                let connectionOrdinal =
                    lock gate (fun () ->
                        connectionCount <- connectionCount + 1
                        activeConnections <- activeConnections + 1
                        maximumActiveConnections <- max maximumActiveConnections activeConnections
                        currentSocket <- Some socket
                        connectionCount)

                printfn "interactive lifecycle websocket opened ordinal=%d" connectionOrdinal

                if connectionOrdinal = 1 then
                    do! sendFrame socket documentFrame
                    do! sendFrame socket (snapshotFrame 1L 2L "LAST GOOD V1")

                do! receiveLoop socket connectionOrdinal
        }

    let html =
        """<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width,initial-scale=1">
  <title>Interactive client lifecycle</title>
  <style>
    body { margin: 0; background: #f4f6f8; color: #15202b; font-family: Segoe UI, sans-serif; }
    header { display: flex; align-items: center; gap: 12px; padding: 10px 18px; border-bottom: 1px solid #cbd3dc; background: #fff; }
    header strong { font-size: 14px; }
    #sdui-runtime-status { font: 12px Consolas, monospace; color: #255a46; }
    #app { min-height: calc(100vh - 45px); }
  </style>
</head>
<body>
  <header><strong>Interactive.Client lifecycle gate</strong><span id="sdui-runtime-status">STARTING</span></header>
  <main id="app"></main>
  <script type="module" src="/assets/client.js"></script>
</body>
</html>"""

    let contentType (path: string) =
        if path.EndsWith(".js", StringComparison.OrdinalIgnoreCase) then "application/javascript; charset=utf-8"
        else "application/octet-stream"

    let serveAsset (context: HttpContext) relativePath =
        task {
            let fullPath = Path.Combine(AppContext.BaseDirectory, "ptcs-dynamic-interactive", relativePath)

            if File.Exists fullPath then
                context.Response.ContentType <- contentType fullPath
                do! context.Response.SendFileAsync fullPath
            else
                context.Response.StatusCode <- StatusCodes.Status404NotFound
        }

    let route (context: HttpContext) =
        task {
            match context.Request.Method, context.Request.Path.Value with
            | "GET", "/session/demo/view/canvas" ->
                context.Response.ContentType <- "text/html; charset=utf-8"
                do! context.Response.WriteAsync html
            | "GET", "/session/demo/frames/canvas" ->
                do! handleWebSocket context
            | "GET", "/assets/client.js" ->
                do! serveAsset context "client.js"
            | "GET", "/assets/WebSharper.Core.JavaScript/Runtime.js" ->
                do! serveAsset context (Path.Combine("WebSharper.Core.JavaScript", "Runtime.js"))
            | "GET", "/favicon.ico" ->
                context.Response.StatusCode <- StatusCodes.Status204NoContent
            | "GET", "/test/state" ->
                let state =
                    lock gate (fun () ->
                        {| connectionCount = connectionCount
                           activeConnections = activeConnections
                           maximumActiveConnections = maximumActiveConnections
                           mountedCount = mountedCount
                           fullSnapshotRequestCount = fullSnapshotRequestCount
                           unmountedCount = unmountedCount
                           frameCount = frameCount |})

                do! context.Response.WriteAsJsonAsync state
            | "POST", "/test/drop" ->
                let dropped =
                    lock gate (fun () ->
                        match currentSocket with
                        | Some socket ->
                            currentSocket <- None
                            socket.Abort()
                            true
                        | None -> false)

                context.Response.StatusCode <- if dropped then StatusCodes.Status202Accepted else StatusCodes.Status409Conflict
                do! context.Response.WriteAsJsonAsync {| dropped = dropped |}
            | _ -> context.Response.StatusCode <- StatusCodes.Status404NotFound
        }

    [<EntryPoint>]
    let main _ =
        let builder = WebApplication.CreateBuilder()
        builder.WebHost.UseUrls("http://127.0.0.1:18884") |> ignore
        let app = builder.Build()
        app.UseWebSockets() |> ignore
        app.Run(RequestDelegate(fun context -> route context :> Task))
        printfn "Interactive.Client lifecycle demo URL=http://127.0.0.1:18884/session/demo/view/canvas"
        app.Run()
        0
