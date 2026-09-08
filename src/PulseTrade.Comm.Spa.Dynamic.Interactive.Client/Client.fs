namespace PulseTrade.Comm.Spa.Dynamic.Interactive.Client

open PulseTrade.Comm.Spa.Dynamic.Contracts
open PulseTrade.Comm.Spa.Dynamic.Renderer
open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client

[<JavaScript>]
type InteractiveApplicationOptions =
    { RootElementId: string
      StatusElementId: string
      ActionTimeoutMs: int
      Lifecycle: InteractiveClientLifecycleOptions }

[<JavaScript>]
type InteractiveApplicationHandle =
    { Start: unit -> unit
      Dispose: unit -> unit
      IsDisposed: unit -> bool
      IsConnected: unit -> bool }

/// Interactive notebook iframe browser application.
///
/// RuntimeFrame validation and TA calculation stay in the host. The application owns one
/// same-origin WebSocket generation, preserves last-good state while reconnecting and requests
/// an authoritative full snapshot before accepting the replacement stream.
[<JavaScript>]
module Client =
    let webSocketUrl () =
        let scheme = if JS.Window.Location.Protocol = "https:" then "wss://" else "ws://"
        let framesPath = JS.Window.Location.Pathname.Replace("/view/", "/frames/")
        scheme + JS.Window.Location.Host + framesPath

    let defaultOptions =
        { RootElementId = "app"
          StatusElementId = "sdui-runtime-status"
          ActionTimeoutMs = 30000
          Lifecycle = InteractiveClientLifecycle.defaults }

    let setStatus elementId text =
        match JS.Document.GetElementById elementId with
        | null -> ()
        | element -> element.TextContent <- text

    let mutable currentApplication: InteractiveApplicationHandle option = None

    let createApplication options =
        let mutable socket: WebSocket option = None
        let mutable transportGeneration = 0
        let mutable lifecycle = InteractiveClientLifecycle.initial
        let mutable reconnectTimer: JS.Handle option = None
        let mutable snapshotTimer: JS.Handle option = None
        let mutable awaitingSnapshot = false
        let mutable runtimeState: Var<RuntimeState> option = None
        let mutable rendererMounted = false
        let mutable pendingAction: (string * (Result<DynamicActionResult, DynamicHostError> -> unit) * JS.Handle) option = None
        let mutable removeUnloadHandler = fun () -> ()

        let clearPendingAction () =
            pendingAction
            |> Option.iter (fun (_, _, timeout) -> JS.ClearTimeout timeout)

            pendingAction <- None

        let failPendingAction code message =
            match pendingAction with
            | Some(_, continuation, _) ->
                clearPendingAction ()
                continuation (Result.Error { Code = code; Message = message })
            | None -> ()

        let completePendingAction result =
            let requestId =
                match result with
                | DynamicActionResult.Accepted(value, _)
                | DynamicActionResult.Rejected(value, _, _)
                | DynamicActionResult.RevisionConflict(value, _) -> value

            match pendingAction with
            | Some(pendingRequestId, continuation, _) when pendingRequestId = requestId ->
                clearPendingAction ()
                continuation (Result.Ok result)
                Result.Ok()
            | Some _ ->
                failPendingAction "interactive-action-correlation-mismatch" "The action result does not match the pending request."
                Result.Error "ACTION CORRELATION ERROR"
            | None -> Result.Error "UNEXPECTED ACTION RESULT"

        let cancelReconnectTimer () =
            reconnectTimer |> Option.iter JS.ClearTimeout
            reconnectTimer <- None

        let cancelSnapshotTimer () =
            snapshotTimer |> Option.iter JS.ClearTimeout
            snapshotTimer <- None

        let send frame =
            match socket with
            | Some value when value.ReadyState = WebSocketReadyState.Open ->
                value.Send(BrowserRuntimeCodec.encodeClient frame)
                true
            | _ -> false

        let sendMounted () =
            runtimeState
            |> Option.iter (fun state -> send (RuntimeClientFrame.Mounted state.Value.Identity.CanvasInstanceId) |> ignore)

        let sendUnmounted () =
            runtimeState
            |> Option.iter (fun state -> send (RuntimeClientFrame.Unmounted state.Value.Identity.CanvasInstanceId) |> ignore)

        let requestFullSnapshot reason =
            match runtimeState with
            | Some state ->
                send
                    (RuntimeClientFrame.Action(
                        SduiAction.RequestFullSnapshot(state.Value.Identity.CanvasInstanceId, reason)))
            | None -> false

        let closeTransport () =
            cancelSnapshotTimer ()
            transportGeneration <- transportGeneration + 1

            socket
            |> Option.iter (fun value ->
                if value.ReadyState = WebSocketReadyState.Open || value.ReadyState = WebSocketReadyState.Connecting then
                    value.Close())

            socket <- None

        let rec applyLifecycle event =
            let next, effects = InteractiveClientLifecycle.transition options.Lifecycle event lifecycle
            lifecycle <- next
            interpretLifecycle effects

        and interpretLifecycle effects =
            for effect in effects do
                match effect with
                | InteractiveClientLifecycleEffect.OpenTransport -> connect ()
                | InteractiveClientLifecycleEffect.SendMounted -> sendMounted ()
                | InteractiveClientLifecycleEffect.RequestFullSnapshot ->
                    if requestFullSnapshot "interactive-reconnect" then
                        scheduleSnapshotTimeout transportGeneration
                | InteractiveClientLifecycleEffect.SendUnmounted -> sendUnmounted ()
                | InteractiveClientLifecycleEffect.ScheduleReconnect delayMs ->
                    cancelReconnectTimer ()
                    reconnectTimer <-
                        Some(
                            JS.SetTimeout
                                (fun () ->
                                    reconnectTimer <- None
                                    applyLifecycle InteractiveClientLifecycleEvent.ReconnectDue)
                                delayMs)
                | InteractiveClientLifecycleEffect.CancelReconnect -> cancelReconnectTimer ()
                | InteractiveClientLifecycleEffect.CloseTransport -> closeTransport ()

        and scheduleSnapshotTimeout generation =
            awaitingSnapshot <- true
            cancelSnapshotTimer ()
            snapshotTimer <-
                Some(
                    JS.SetTimeout
                        (fun () ->
                            snapshotTimer <- None

                            if generation = transportGeneration && not lifecycle.Disposed then
                                failPendingAction
                                    "interactive-snapshot-timeout"
                                    "The interactive host did not provide an authoritative snapshot before timeout."

                                transportGeneration <- transportGeneration + 1
                                let staleSocket = socket
                                socket <- None

                                staleSocket
                                |> Option.iter (fun value ->
                                    if value.ReadyState = WebSocketReadyState.Open || value.ReadyState = WebSocketReadyState.Connecting then
                                        value.Close())

                                applyLifecycle InteractiveClientLifecycleEvent.TransportClosed
                                setStatus options.StatusElementId "RECONNECTING")
                        options.ActionTimeoutMs)

        and connect () =
            if not lifecycle.Disposed && socket.IsNone then
                transportGeneration <- transportGeneration + 1
                let generation = transportGeneration
                let value = new WebSocket(webSocketUrl ())
                socket <- Some value
                setStatus options.StatusElementId "CONNECTING"

                value.OnOpen <-
                    fun () ->
                        if generation = transportGeneration && not lifecycle.Disposed then
                            applyLifecycle (InteractiveClientLifecycleEvent.TransportOpened runtimeState.IsSome)

                            if runtimeState.IsNone then
                                scheduleSnapshotTimeout generation

                            setStatus options.StatusElementId (if awaitingSnapshot then "RESYNCING" else "CONNECTED")

                value.OnMessage <-
                    fun event ->
                        if generation = transportGeneration && not lifecycle.Disposed then
                            let text = string event.Data

                            match BrowserRuntimeCodec.decodeActionResult text with
                            | Ok result ->
                                match completePendingAction result with
                                | Ok _ -> setStatus options.StatusElementId (if awaitingSnapshot then "RESYNCING" else "READY")
                                | Error message -> setStatus options.StatusElementId message
                            | Error _ ->
                                match BrowserRuntimeCodec.decode text with
                                | Ok frame ->
                                    let accepted = applyFrame frame

                                    if accepted then
                                        setStatus options.StatusElementId (if awaitingSnapshot then "RESYNCING" else "READY")
                                | Error message -> setStatus options.StatusElementId ("FRAME ERROR: " + message)

                value.OnError <-
                    fun () ->
                        if generation = transportGeneration && not lifecycle.Disposed then
                            setStatus options.StatusElementId "CONNECTION ERROR"

                value.OnClose <-
                    fun () ->
                        if generation = transportGeneration && not lifecycle.Disposed then
                            cancelSnapshotTimer ()
                            socket <- None
                            failPendingAction
                                "interactive-channel-disconnected"
                                "The interactive action channel disconnected before the action completed."
                            applyLifecycle InteractiveClientLifecycleEvent.TransportClosed
                            setStatus options.StatusElementId "RECONNECTING"

        and interpretRuntime effect =
            match effect with
            | RuntimeEffect.RequestResync(canvasId, _) ->
                if send
                       (RuntimeClientFrame.Action(
                           SduiAction.RequestFullSnapshot(canvasId, "browser-reducer-resync"))) then
                    scheduleSnapshotTimeout transportGeneration
            | _ -> ()

        and mount state =
            let callbacks =
                { SubmitAction =
                    fun request ->
                        Async.FromContinuations(fun (continuation, _, _) ->
                            if pendingAction.IsSome then
                                continuation
                                    (Result.Error
                                        { Code = "interactive-action-busy"
                                          Message = "A dynamic action is already in flight." })
                            elif not lifecycle.Connected then
                                continuation
                                    (Result.Error
                                        { Code = "interactive-channel-not-open"
                                          Message = "The interactive action channel is not open." })
                            else
                                let timeout =
                                    JS.SetTimeout
                                        (fun () ->
                                            failPendingAction
                                                "interactive-action-timeout"
                                                "The interactive host did not acknowledge the action before timeout.")
                                        options.ActionTimeoutMs

                                pendingAction <- Some(request.RequestId, continuation, timeout)

                                match socket with
                                | Some value when value.ReadyState = WebSocketReadyState.Open ->
                                    try
                                        value.Send(BrowserRuntimeCodec.encodeActionRequest request)
                                    with error ->
                                        failPendingAction "interactive-action-send-failed" error.Message
                                | _ ->
                                    failPendingAction
                                        "interactive-channel-not-open"
                                        "The interactive action channel is not open.") }

            TaWorkspaceRenderer.render TaWorkspaceRenderer.defaultOptions callbacks state
            |> Doc.RunById options.RootElementId

        and applyFrame frame =
            let next, effect =
                match runtimeState with
                | None ->
                    let identity =
                        { DocumentId = frame.DocumentId
                          CanvasInstanceId = frame.CanvasInstanceId }

                    RuntimeReducer.reduce (RuntimeReducer.initial identity) frame
                | Some state -> RuntimeReducer.reduce state.Value frame

            let accepted =
                match effect with
                | RuntimeEffect.RequestResync _ -> false
                | _ -> true

            match runtimeState with
            | None ->
                let state = Var.Create next
                runtimeState <- Some state
                interpretRuntime effect

                match next.Document with
                | Some _ ->
                    mount state
                    rendererMounted <- true
                    sendMounted ()
                | None -> ()
            | Some state ->
                state.Value <- next
                interpretRuntime effect

                if not rendererMounted && next.Document.IsSome then
                    mount state
                    rendererMounted <- true
                    sendMounted ()

            if accepted && frame.Kind = RuntimeFrameKind.Snapshot then
                awaitingSnapshot <- false
                cancelSnapshotTimer ()
                applyLifecycle InteractiveClientLifecycleEvent.SnapshotAccepted

            accepted

        let dispose () =
            if not lifecycle.Disposed then
                failPendingAction "interactive-channel-disposed" "The interactive action channel was disposed."
                removeUnloadHandler ()
                applyLifecycle InteractiveClientLifecycleEvent.Dispose
                setStatus options.StatusElementId "DISPOSED"

        let beforeUnloadHandler =
            fun (_: WebSharper.JavaScript.Dom.Event) -> dispose ()

        JS.Window.AddEventListener("beforeunload", beforeUnloadHandler)
        removeUnloadHandler <- fun () -> JS.Window.RemoveEventListener("beforeunload", beforeUnloadHandler)

        let handle =
            { Start = fun () -> applyLifecycle InteractiveClientLifecycleEvent.Start
              Dispose = dispose
              IsDisposed = fun () -> lifecycle.Disposed
              IsConnected = fun () -> lifecycle.Connected }

        handle.Start()
        handle

    [<RequireQualifiedAccess>]
    module Application =
        let startWithOptions options =
            match currentApplication with
            | Some handle when not (handle.IsDisposed()) -> handle
            | _ ->
                let handle = createApplication options
                currentApplication <- Some handle
                handle

        let start () = startWithOptions defaultOptions

        let dispose () =
            currentApplication |> Option.iter (fun handle -> handle.Dispose())

    [<SPAEntryPoint>]
    let Main () =
        Application.start () |> ignore
