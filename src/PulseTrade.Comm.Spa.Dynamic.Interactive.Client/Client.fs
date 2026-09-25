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
        let messageQueue = ResizeArray<int * string>()
        let mutable messagePumpRunning = false
        let mutable activeBatch: RuntimeSnapshotTransportAssemblyState option = None

        let resetActiveBatch () =
            activeBatch <- None

        let resetMessagePump () =
            messageQueue.Clear()
            messagePumpRunning <- false
            resetActiveBatch ()

        let tryStringField name value =
            let field = JS.Get<obj> name value
            if BrowserRuntimeFramePump.isMissing field then None
            else
                let text = As<string> field
                if System.String.IsNullOrWhiteSpace text then None else Some text

        let tryIntField name value =
            let field = JS.Get<obj> name value
            if BrowserRuntimeFramePump.isMissing field then None
            else Some(As<int> field)

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
            resetMessagePump ()

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

        and requestSnapshotAfterFrameFailure code message =
            resetActiveBatch ()
            setStatus options.StatusElementId ("FRAME REJECTED: " + code + " - " + message)

            if requestFullSnapshot code then
                scheduleSnapshotTimeout transportGeneration

        and abortTransport code message =
            cancelSnapshotTimer ()
            failPendingAction code message
            transportGeneration <- transportGeneration + 1
            let staleSocket = socket
            socket <- None
            resetMessagePump ()

            staleSocket
            |> Option.iter (fun value ->
                if value.ReadyState = WebSocketReadyState.Open || value.ReadyState = WebSocketReadyState.Connecting then
                    value.Close())

            applyLifecycle InteractiveClientLifecycleEvent.TransportClosed
            setStatus options.StatusElementId "RECONNECTING"

        and scheduleMessagePump () =
            if not messagePumpRunning && messageQueue.Count > 0 && not lifecycle.Disposed then
                messagePumpRunning <- true
                let generation, text = messageQueue[0]
                messageQueue.RemoveAt 0

                JS.RequestAnimationFrame(fun _ ->
                    if generation <> transportGeneration || lifecycle.Disposed then
                        messagePumpRunning <- false
                        scheduleMessagePump ()
                    else
                        processQueuedMessage generation text (fun () ->
                            messagePumpRunning <- false
                            scheduleMessagePump ()))
                |> ignore

        and enqueueMessage generation text =
            if messageQueue.Count >= RuntimeSnapshotTransportDefaults.MaximumItems + 16 then
                abortTransport
                    "interactive-frame-queue-overflow"
                    "The interactive frame queue exceeded its bounded capacity."
            else
                messageQueue.Add(generation, text)
                scheduleMessagePump ()

        and processQueuedMessage generation text completed =
            let isCurrent () = generation = transportGeneration && not lifecycle.Disposed
            let schedule _ work = JS.RequestAnimationFrame(fun _ -> work ()) |> ignore

            let reject code message =
                requestSnapshotAfterFrameFailure code message
                completed ()

            try
                let raw = JSON.Parse text
                let schema = tryStringField "Schema" raw
                let protocol = tryStringField "Protocol" raw

                match schema, protocol with
                | Some value, _ when value = RuntimeSnapshotTransportDefaults.Schema ->
                    processSnapshotPacket generation raw completed
                | _, Some value when value = DynamicActionWireDefaults.Protocol ->
                    match BrowserRuntimeCodec.decodeActionResult text with
                    | Ok result ->
                        match completePendingAction result with
                        | Ok _ -> setStatus options.StatusElementId (if awaitingSnapshot then "RESYNCING" else "READY")
                        | Error message -> setStatus options.StatusElementId message
                    | Error message -> setStatus options.StatusElementId ("ACTION ERROR: " + message)

                    completed ()
                | _ when activeBatch.IsSome ->
                    reject
                        "runtime-snapshot-chunk-interleaved"
                        "A canonical runtime frame interrupted an incomplete snapshot batch."
                | _ ->
                    let rawPayload = JS.Get<obj> "Payload" raw
                    let isSnapshot =
                        not (BrowserRuntimeFramePump.isMissing rawPayload)
                        && JS.Get<int> "$" rawPayload = 1

                    if isSnapshot then
                        BrowserRuntimeFramePump.reduceParsedFrameWith
                            schedule
                            (runtimeState |> Option.map _.Value)
                            raw
                            isCurrent
                            (function
                                | BrowserRuntimeFramePumpOutcome.Applied candidate ->
                                    publishSnapshotCandidate candidate
                                    completed ()
                                | BrowserRuntimeFramePumpOutcome.Rejected failure ->
                                    reject failure.Code failure.Message
                                | BrowserRuntimeFramePumpOutcome.Superseded -> completed ())
                    else
                        match BrowserRuntimeCodec.decode text with
                        | Ok frame ->
                            let accepted = applyFrame frame
                            if accepted then setStatus options.StatusElementId (if awaitingSnapshot then "RESYNCING" else "READY")
                        | Error message -> setStatus options.StatusElementId ("FRAME ERROR: " + message)

                        completed ()
            with error ->
                reject "runtime-frame-decode-failed" error.Message

        and processSnapshotPacket generation raw completed =
            let kind = tryStringField "Kind" raw

            let reject code message =
                requestSnapshotAfterFrameFailure code message
                completed ()

            match kind with
            | Some value when value = RuntimeSnapshotTransportDefaults.StartKind ->
                match tryStringField "BatchId" raw, tryIntField "ItemCount" raw with
                | Some batchId, Some itemCount ->
                    let rawHeader = JS.Get<obj> "Header" raw

                    if BrowserRuntimeFramePump.isMissing rawHeader then
                        reject "runtime-snapshot-chunk-header-required" "Snapshot start requires a header."
                    else
                        match BrowserRuntimeCodec.decode (JSON.Stringify rawHeader) with
                        | Error message -> reject "runtime-snapshot-chunk-header-invalid" message
                        | Ok header ->
                            let packet =
                                RuntimeSnapshotTransportPacket.Start
                                    { Schema = RuntimeSnapshotTransportDefaults.Schema
                                      Kind = RuntimeSnapshotTransportDefaults.StartKind
                                      BatchId = batchId
                                      ItemCount = itemCount
                                      Header = header }

                            let state =
                                activeBatch
                                |> Option.defaultWith (fun () -> RuntimeSnapshotTransportAssembler.create generation)

                            match RuntimeSnapshotTransportAssembler.acceptPacket generation packet state with
                            | Error issue -> reject issue.Code issue.Message
                            | Ok next ->
                                activeBatch <- Some next
                                completed ()
                | _ ->
                    reject
                        "runtime-snapshot-chunk-start-invalid"
                        "Snapshot start batch id or item count is invalid."

            | Some value when value = RuntimeSnapshotTransportDefaults.ItemKind ->
                match tryStringField "BatchId" raw, tryIntField "ItemIndex" raw, tryStringField "DataRef" raw with
                | Some batchId, Some itemIndex, Some dataRef ->
                    let state =
                        activeBatch
                        |> Option.defaultWith (fun () -> RuntimeSnapshotTransportAssembler.create generation)

                    match RuntimeSnapshotTransportAssembler.validateItemMetadata generation batchId itemIndex dataRef state with
                    | Error issue -> reject issue.Code issue.Message
                    | Ok _ ->
                        let rawValue = JS.Get<obj> "Value" raw

                        if BrowserRuntimeFramePump.isMissing rawValue then
                            reject "runtime-snapshot-chunk-value-required" "Snapshot item requires a value."
                        else
                            let schedule _ work = JS.RequestAnimationFrame(fun _ -> work ()) |> ignore
                            let isCurrent () =
                                generation = transportGeneration
                                && (activeBatch
                                    |> Option.exists (fun current ->
                                        current.Generation = generation && current.BatchId = Some batchId))
                                && not lifecycle.Disposed

                            BrowserRuntimeFramePump.decodeSnapshotValueWith
                                schedule
                                dataRef
                                rawValue
                                isCurrent
                                (fun decoded ->
                                    let packet =
                                        RuntimeSnapshotTransportPacket.Item
                                            { Schema = RuntimeSnapshotTransportDefaults.Schema
                                              Kind = RuntimeSnapshotTransportDefaults.ItemKind
                                              BatchId = batchId
                                              ItemIndex = itemIndex
                                              DataRef = dataRef
                                              Value = decoded }

                                    match RuntimeSnapshotTransportAssembler.acceptPacket generation packet state with
                                    | Error issue -> reject issue.Code issue.Message
                                    | Ok next ->
                                        activeBatch <- Some next
                                        completed ())
                                reject
                                completed
                | _ ->
                    reject
                        "runtime-snapshot-chunk-item-invalid"
                        "Snapshot item is missing, duplicate, out of order or belongs to another batch."

            | Some value when value = RuntimeSnapshotTransportDefaults.CommitKind ->
                match tryStringField "BatchId" raw, tryIntField "ItemCount" raw with
                | Some batchId, Some itemCount ->
                    let packet =
                        RuntimeSnapshotTransportPacket.Commit
                            { Schema = RuntimeSnapshotTransportDefaults.Schema
                              Kind = RuntimeSnapshotTransportDefaults.CommitKind
                              BatchId = batchId
                              ItemCount = itemCount }

                    let state =
                        activeBatch
                        |> Option.defaultWith (fun () -> RuntimeSnapshotTransportAssembler.create generation)

                    match RuntimeSnapshotTransportAssembler.acceptPacket generation packet state with
                    | Error issue -> reject issue.Code issue.Message
                    | Ok next ->
                        match next.CompletedFrame with
                        | None -> reject "runtime-snapshot-chunk-commit-invalid" "Snapshot commit did not complete the active batch."
                        | Some frame ->
                            resetActiveBatch ()
                            let schedule _ work = JS.RequestAnimationFrame(fun _ -> work ()) |> ignore
                            let isCurrent () = generation = transportGeneration && not lifecycle.Disposed
                            let current =
                                runtimeState
                                |> Option.map _.Value
                                |> Option.defaultWith (fun () ->
                                    RuntimeReducer.initial
                                        { DocumentId = frame.DocumentId
                                          CanvasInstanceId = frame.CanvasInstanceId })

                            BrowserRuntimeFramePump.reduceFrameWith
                                schedule
                                current
                                frame
                                isCurrent
                                0
                                (function
                                    | BrowserRuntimeFramePumpOutcome.Applied candidate ->
                                        publishSnapshotCandidate candidate
                                        completed ()
                                    | BrowserRuntimeFramePumpOutcome.Rejected failure ->
                                        reject failure.Code failure.Message
                                    | BrowserRuntimeFramePumpOutcome.Superseded -> completed ())
                | _ ->
                    reject
                        "runtime-snapshot-chunk-commit-invalid"
                        "Snapshot commit does not match a complete active batch."

            | _ ->
                reject "runtime-snapshot-chunk-kind-invalid" "Snapshot chunk kind is unsupported."

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
                            enqueueMessage generation (string event.Data)

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
            | RuntimeEffect.RejectFrame(_, runtimeError) ->
                setStatus options.StatusElementId ("FRAME REJECTED: " + runtimeError.ReasonCode)
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

        and publishRuntimeState next =
            match runtimeState with
            | None ->
                let state = Var.Create next
                runtimeState <- Some state

                match next.Document with
                | Some _ ->
                    mount state
                    rendererMounted <- true
                    sendMounted ()
                | None -> ()
            | Some state ->
                state.Value <- next

                if not rendererMounted && next.Document.IsSome then
                    mount state
                    rendererMounted <- true
                    sendMounted ()

        and publishSnapshotCandidate candidate =
            publishRuntimeState candidate
            awaitingSnapshot <- false
            cancelSnapshotTimer ()
            applyLifecycle InteractiveClientLifecycleEvent.SnapshotAccepted
            setStatus options.StatusElementId "READY"

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
                | RuntimeEffect.RequestResync _
                | RuntimeEffect.RejectFrame _ -> false
                | _ -> true

            if accepted && frame.Kind = RuntimeFrameKind.Snapshot then
                publishSnapshotCandidate next
            else
                publishRuntimeState next

            interpretRuntime effect

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
