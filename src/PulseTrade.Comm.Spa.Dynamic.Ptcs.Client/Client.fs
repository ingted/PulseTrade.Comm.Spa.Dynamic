namespace PulseTrade.Comm.Spa.Dynamic.Ptcs.Client

open System
open PulseTrade.Comm.Spa.Dynamic.Contracts
open PulseTrade.Comm.Spa.Dynamic.Renderer
open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client

[<JavaScript; CLIMutable>]
type TaBrowserPointWire =
    { time: string
      openValue: float
      highValue: float
      lowValue: float
      closeValue: float
      volumeValue: float
      lineValue: float
      hasOpen: bool
      hasHigh: bool
      hasLow: bool
      hasClose: bool
      hasVolume: bool
      hasLineValue: bool }

[<JavaScript; CLIMutable>]
type TaBrowserSeriesWire =
    { dataRef: string
      points: TaBrowserPointWire array }

[<JavaScript; CLIMutable>]
type TaBrowserRowWire =
    { rowId: string
      kind: string
      dataRef: string
      heightWeight: float
      visible: bool }

[<JavaScript; CLIMutable>]
type TaBrowserStateWire =
    { wireVersion: string
      documentId: string
      canvasInstanceId: string
      workspaceId: string
      title: string
      rowsRef: string
      statusRef: string
      sharedTimeAxis: bool
      rows: TaBrowserRowWire array
      allowedActions: string array
      series: TaBrowserSeriesWire array
      statusLabel: string
      freshness: string
      watermarkUtc: string
      quality: string
      lagSeconds: float
      reasonCode: string
      documentRevision: int64
      dataRevision: int64
      transportSequence: int64
      pollKind: string
      errorCode: string
      errorMessage: string
      errorRecoverable: bool }

[<JavaScript; CLIMutable>]
type TaBrowserClientFrameWire =
    { wireVersion: string
      kind: string
      actionKind: string
      canvasInstanceId: string
      rowId: string
      rowKind: string
      dataRef: string
      heightWeight: float
      visible: bool
      sourceId: string
      instrument: string
      intervalMinutes: int
      fromUtc: string
      toUtcExclusive: string
      includePartial: bool
      afterDataRevision: float
      dataRevision: float
      reasonCode: string }

[<JavaScript; CLIMutable>]
type ExtensionTransientRequestWire =
    { ``type``: string
      requestId: string
      extensionId: string
      channelId: string
      operation: string
      payload: string }

[<JavaScript; CLIMutable>]
type ExtensionTransientResponseWire =
    { ``type``: string
      requestId: string
      status: string
      extensionId: string
      channelId: string
      operation: string
      channelSequence: int64
      payload: string
      error: string }

[<JavaScript>]
type TaClientLifecycleOptions =
    { PollIntervalMs: int
      RequestTimeoutMs: int
      PollRetryMs: int
      ReconnectBaseMs: int
      ReconnectMaximumMs: int }

[<JavaScript>]
type TaClientLifecycleState =
    { CanvasInstanceId: CanvasInstanceId
      Poll: RuntimePollState
      Connected: bool
      Active: bool
      InFlight: bool
      DataRevision: int64
      ReconnectAttempt: int
      Disposed: bool }

[<JavaScript; RequireQualifiedAccess>]
type TaClientLifecycleEvent =
    | Connected
    | StateAccepted of dataRevision: int64
    | StartAction of SduiAction
    | PollDue of nowUtc: DateTimeOffset
    | RequestTimedOut of nowUtc: DateTimeOffset
    | Disconnected
    | ActiveChanged of bool
    | ResyncRequired of reasonCode: string
    | Dispose

[<JavaScript; RequireQualifiedAccess>]
type TaClientLifecycleEffect =
    | SendMounted
    | SendUnmounted
    | SendAction of SduiAction
    | SchedulePoll of delayMs: int
    | ScheduleTimeout of delayMs: int
    | ScheduleReconnect of delayMs: int
    | CancelPoll
    | CancelTimeout
    | CancelReconnect

[<JavaScript; RequireQualifiedAccess>]
module TaClientLifecycle =
    let defaults =
        { PollIntervalMs = 5000
          RequestTimeoutMs = 10000
          PollRetryMs = 2000
          ReconnectBaseMs = 1000
          ReconnectMaximumMs = 30000 }

    let initial canvasInstanceId =
        { CanvasInstanceId = canvasInstanceId
          Poll = RuntimePollState.Unmounted
          Connected = false
          Active = true
          InFlight = false
          DataRevision = 0L
          ReconnectAttempt = 0
          Disposed = false }

    let reconnectDelay options attempt =
        let rec expand current remaining =
            if remaining <= 1 then current
            else expand (min options.ReconnectMaximumMs (current * 2)) (remaining - 1)

        expand options.ReconnectBaseMs (max 1 attempt)

    let transition options event state =
        if state.Disposed && event <> TaClientLifecycleEvent.Dispose then
            state, [||]
        else
            match event with
            | TaClientLifecycleEvent.Connected ->
                { state with
                    Poll = RuntimePollState.MountedIdle
                    Connected = true
                    InFlight = true
                    ReconnectAttempt = 0 },
                [| TaClientLifecycleEffect.CancelReconnect
                   TaClientLifecycleEffect.SendMounted
                   TaClientLifecycleEffect.ScheduleTimeout options.RequestTimeoutMs |]
            | TaClientLifecycleEvent.StateAccepted revision ->
                let nextPoll = if state.Active then RuntimePollState.Ready else RuntimePollState.Suspended
                let schedule = if state.Active then [| TaClientLifecycleEffect.SchedulePoll options.PollIntervalMs |] else [||]

                { state with Poll = nextPoll; InFlight = false; DataRevision = revision },
                Array.append [| TaClientLifecycleEffect.CancelTimeout |] schedule
            | TaClientLifecycleEvent.StartAction action when state.Connected && state.Active && not state.InFlight ->
                { state with Poll = RuntimePollState.PollInFlight; InFlight = true },
                [| TaClientLifecycleEffect.CancelPoll
                   TaClientLifecycleEffect.SendAction action
                   TaClientLifecycleEffect.ScheduleTimeout options.RequestTimeoutMs |]
            | TaClientLifecycleEvent.PollDue _ when state.Connected && state.Active && not state.InFlight ->
                { state with Poll = RuntimePollState.PollInFlight; InFlight = true },
                [| TaClientLifecycleEffect.SendAction(SduiAction.PollDelta(state.CanvasInstanceId, state.DataRevision))
                   TaClientLifecycleEffect.ScheduleTimeout options.RequestTimeoutMs |]
            | TaClientLifecycleEvent.RequestTimedOut nowUtc when state.InFlight ->
                let nextPoll = RuntimePollState.Backoff(nowUtc.AddMilliseconds(float options.PollRetryMs))
                let retry =
                    if state.Connected && state.Active then [| TaClientLifecycleEffect.SchedulePoll options.PollRetryMs |]
                    else [||]

                { state with Poll = nextPoll; InFlight = false },
                Array.append [| TaClientLifecycleEffect.CancelTimeout |] retry
            | TaClientLifecycleEvent.Disconnected ->
                let attempt = state.ReconnectAttempt + 1
                { state with
                    Poll = RuntimePollState.Suspended
                    Connected = false
                    InFlight = false
                    ReconnectAttempt = attempt },
                [| TaClientLifecycleEffect.CancelPoll
                   TaClientLifecycleEffect.CancelTimeout
                   TaClientLifecycleEffect.ScheduleReconnect(reconnectDelay options attempt) |]
            | TaClientLifecycleEvent.ActiveChanged active ->
                let ready = active && state.Connected
                let effects =
                    if ready then [| TaClientLifecycleEffect.SchedulePoll options.PollIntervalMs |]
                    else [| TaClientLifecycleEffect.CancelPoll; TaClientLifecycleEffect.CancelTimeout |]

                { state with
                    Active = active
                    Poll = if ready then RuntimePollState.Ready else RuntimePollState.Suspended
                    InFlight = if ready then state.InFlight else false },
                effects
            | TaClientLifecycleEvent.ResyncRequired reason when state.Connected && state.Active && not state.InFlight ->
                { state with Poll = RuntimePollState.PausedForResync; InFlight = true },
                [| TaClientLifecycleEffect.CancelPoll
                   TaClientLifecycleEffect.SendAction(SduiAction.RequestFullSnapshot(state.CanvasInstanceId, reason))
                   TaClientLifecycleEffect.ScheduleTimeout options.RequestTimeoutMs |]
            | TaClientLifecycleEvent.Dispose ->
                { state with
                    Poll = RuntimePollState.Disposed
                    Connected = false
                    InFlight = false
                    Disposed = true },
                [| TaClientLifecycleEffect.CancelPoll
                   TaClientLifecycleEffect.CancelTimeout
                   TaClientLifecycleEffect.CancelReconnect
                   if state.Connected then TaClientLifecycleEffect.SendUnmounted |]
            | _ -> state, [||]

[<JavaScript; RequireQualifiedAccess>]
module TaResearchClientWire =
    let text value = if isNull value then "" else value

    let rowKind value =
        match text value |> fun item -> item.ToLower() with
        | "volume" -> TaRowKind.Volume
        | "sma" -> TaRowKind.Sma
        | "dmi" -> TaRowKind.Dmi
        | "adx" -> TaRowKind.Adx
        | "macd" -> TaRowKind.Macd
        | "heikin-ashi" -> TaRowKind.HeikinAshi
        | _ -> TaRowKind.Candlestick

    let pollState value =
        match text value with
        | "mounted-idle" -> RuntimePollState.MountedIdle
        | "ready" -> RuntimePollState.Ready
        | "poll-in-flight" -> RuntimePollState.PollInFlight
        | "suspended" -> RuntimePollState.Suspended
        | "paused-for-resync" -> RuntimePollState.PausedForResync
        | "disposed" -> RuntimePollState.Disposed
        | _ -> RuntimePollState.Unmounted

    let pointValue (point: TaBrowserPointWire) =
        [ if not (String.IsNullOrWhiteSpace point.time) then
              "t", SduiValue.Text point.time
          if point.hasOpen then "o", SduiValue.Number point.openValue
          if point.hasHigh then "h", SduiValue.Number point.highValue
          if point.hasLow then "l", SduiValue.Number point.lowValue
          if point.hasClose then "c", SduiValue.Number point.closeValue
          if point.hasVolume then "v", SduiValue.Number point.volumeValue
          if point.hasLineValue then "v", SduiValue.Number point.lineValue ]
        |> Map.ofList
        |> SduiValue.Object

    let stateFromWire (wire: TaBrowserStateWire) =
        if isNull (box wire) || wire.wireVersion <> "ta-browser.v1" then
            Result.Error "Unsupported TA browser state wire."
        else
            let rows =
                if isNull wire.rows then [||]
                else
                    wire.rows
                    |> Array.map (fun row ->
                        { RowId = text row.rowId
                          Kind = rowKind row.kind
                          DataRef = text row.dataRef
                          HeightWeight = row.heightWeight
                          Visible = row.visible
                          Options = Map.empty })

            let seriesData =
                if isNull wire.series then Map.empty
                else
                    wire.series
                    |> Array.map (fun series ->
                        let points =
                            if isNull series.points then [||]
                            else series.points |> Array.map pointValue

                        text series.dataRef, SduiValue.Array points)
                    |> Map.ofArray

            let status =
                SduiValue.Object(
                    Map [ "label", SduiValue.Text(text wire.statusLabel)
                          "freshness", SduiValue.Text(text wire.freshness)
                          "watermarkUtc", SduiValue.Text(text wire.watermarkUtc)
                          "quality", SduiValue.Text(text wire.quality)
                          "lagSeconds", SduiValue.Number wire.lagSeconds
                          "reasonCode", SduiValue.Text(text wire.reasonCode) ])

            let data = Map.add (text wire.statusRef) status seriesData
            let lastError =
                if String.IsNullOrWhiteSpace wire.errorCode && String.IsNullOrWhiteSpace wire.errorMessage then None
                else
                    Some
                        { ReasonCode = text wire.errorCode
                          Message = text wire.errorMessage
                          Recoverable = wire.errorRecoverable }

            Result.Ok
                { Identity =
                    { DocumentId = DocumentId(text wire.documentId)
                      CanvasInstanceId = CanvasInstanceId(text wire.canvasInstanceId) }
                  Document =
                    Some
                        { WorkspaceId = text wire.workspaceId
                          Title = text wire.title
                          RowsRef = text wire.rowsRef
                          StatusRef = text wire.statusRef
                          SharedTimeAxis = wire.sharedTimeAxis
                          Rows = rows
                          AllowedActions = if isNull wire.allowedActions then [||] else wire.allowedActions
                          DefaultView = Map.empty }
                  Data = data
                  DocumentRevision = wire.documentRevision
                  DataRevision = wire.dataRevision
                  LastTransportSequence = wire.transportSequence
                  View = { Values = Map.empty }
                  Poll = pollState wire.pollKind
                  LastError = lastError }

    let emptyFrame kind actionKind canvasId =
        { wireVersion = "ta-browser.v1"
          kind = kind
          actionKind = actionKind
          canvasInstanceId = canvasId
          rowId = ""
          rowKind = ""
          dataRef = ""
          heightWeight = 0.0
          visible = false
          sourceId = ""
          instrument = ""
          intervalMinutes = 0
          fromUtc = ""
          toUtcExclusive = ""
          includePartial = false
          afterDataRevision = 0.0
          dataRevision = 0.0
          reasonCode = "" }

    let optionText value = value |> Option.defaultValue ""
    let optionInt value = value |> Option.defaultValue 0
    let optionBool value = value |> Option.defaultValue false
    let canvasText (CanvasInstanceId value) = value

    let actionToWire action =
        match action with
        | SduiAction.ResetView canvas -> emptyFrame "action" "reset-view" (canvasText canvas)
        | SduiAction.ResetCanvas canvas -> emptyFrame "action" "reset-canvas" (canvasText canvas)
        | SduiAction.AddTaRow(canvas, row) ->
            { emptyFrame "action" "add-row" (canvasText canvas) with
                rowId = row.RowId
                rowKind = string row.Kind
                dataRef = row.DataRef
                heightWeight = row.HeightWeight
                visible = row.Visible }
        | SduiAction.RemoveTaRow(canvas, rowId) ->
            { emptyFrame "action" "remove-row" (canvasText canvas) with rowId = rowId }
        | SduiAction.ChangeTaQuery(canvas, query) ->
            { emptyFrame "action" "change-query" (canvasText canvas) with
                sourceId = optionText query.SourceId
                instrument = optionText query.Instrument
                intervalMinutes = optionInt query.IntervalMinutes
                fromUtc = optionText query.FromUtc
                toUtcExclusive = optionText query.ToUtcExclusive
                includePartial = optionBool query.IncludePartial }
        | SduiAction.PollDelta(canvas, revision) ->
            { emptyFrame "action" "poll-delta" (canvasText canvas) with afterDataRevision = float revision }
        | SduiAction.RequestFullSnapshot(canvas, reason) ->
            { emptyFrame "action" "full-snapshot" (canvasText canvas) with reasonCode = reason }

[<JavaScript>]
type TaResearchTransientClientHandle =
    { RuntimeState: Var<RuntimeState>
      SetActive: bool -> unit
      Dispose: unit -> unit }

[<JavaScript; RequireQualifiedAccess>]
module TaResearchTransientClient =
    let syncWebSocketUrl () =
        let protocol = if JS.Window.Location.Protocol = "https:" then "wss://" else "ws://"
        protocol + JS.Window.Location.Host + "/sync/ws"

    let mountByIdWithOptions rootId extensionId channelId canvasId lifecycleOptions =
        let identity =
            { DocumentId = DocumentId("pending-" + channelId)
              CanvasInstanceId = CanvasInstanceId canvasId }

        let runtimeState =
            Var.Create
                { Identity = identity
                  Document = None
                  Data = Map.empty
                  DocumentRevision = 0L
                  DataRevision = 0L
                  LastTransportSequence = 0L
                  View = { Values = Map.empty }
                  Poll = RuntimePollState.Unmounted
                  LastError = None }
        let mutable socket: WebSocket option = None
        let mutable requestSequence = 0
        let mutable lifecycle = TaClientLifecycle.initial identity.CanvasInstanceId
        let mutable pollTimer: JS.Handle option = None
        let mutable timeoutTimer: JS.Handle option = None
        let mutable reconnectTimer: JS.Handle option = None

        let nextRequestId () =
            requestSequence <- requestSequence + 1
            channelId + ":" + string requestSequence

        let sendPayload operation payload =
            let request =
                { ``type`` = "extension-transient"
                  requestId = nextRequestId ()
                  extensionId = extensionId
                  channelId = channelId
                  operation = operation
                  payload = JSON.Stringify payload }

            let text = JSON.Stringify request

            match socket with
            | Some value when value.ReadyState = WebSocketReadyState.Open ->
                value.Send text
                true
            | _ -> false

        let cancelPollTimer () =
            pollTimer |> Option.iter JS.ClearTimeout
            pollTimer <- None

        let cancelTimeoutTimer () =
            timeoutTimer |> Option.iter JS.ClearTimeout
            timeoutTimer <- None

        let cancelReconnectTimer () =
            reconnectTimer |> Option.iter JS.ClearTimeout
            reconnectTimer <- None

        let closeSocket () =
            cancelTimeoutTimer ()

            socket
            |> Option.iter (fun value ->
                if value.ReadyState = WebSocketReadyState.Open || value.ReadyState = WebSocketReadyState.Connecting then
                    value.Close())

            socket <- None

        let rec apply event =
            let next, effects = TaClientLifecycle.transition lifecycleOptions event lifecycle
            lifecycle <- next
            runtimeState.Value <- { runtimeState.Value with Poll = next.Poll }
            interpret effects
            effects

        and interpret effects =
            for effect in effects do
                match effect with
                | TaClientLifecycleEffect.SendMounted ->
                    if not (sendPayload "open" (TaResearchClientWire.emptyFrame "mounted" "" canvasId)) then
                        apply TaClientLifecycleEvent.Disconnected |> ignore
                | TaClientLifecycleEffect.SendUnmounted ->
                    sendPayload "close" (TaResearchClientWire.emptyFrame "unmounted" "" canvasId) |> ignore
                | TaClientLifecycleEffect.SendAction action ->
                    if not (sendPayload "action" (TaResearchClientWire.actionToWire action)) then
                        apply TaClientLifecycleEvent.Disconnected |> ignore
                | TaClientLifecycleEffect.SchedulePoll delayMs ->
                    cancelPollTimer ()
                    pollTimer <-
                        Some(
                            JS.SetTimeout
                                (fun () ->
                                    pollTimer <- None
                                    apply (TaClientLifecycleEvent.PollDue DateTimeOffset.UtcNow) |> ignore)
                                delayMs)
                | TaClientLifecycleEffect.ScheduleTimeout delayMs ->
                    cancelTimeoutTimer ()
                    timeoutTimer <-
                        Some(
                            JS.SetTimeout
                                (fun () ->
                                    timeoutTimer <- None
                                    apply (TaClientLifecycleEvent.RequestTimedOut DateTimeOffset.UtcNow) |> ignore)
                                delayMs)
                | TaClientLifecycleEffect.ScheduleReconnect delayMs ->
                    cancelReconnectTimer ()
                    reconnectTimer <-
                        Some(
                            JS.SetTimeout
                                (fun () ->
                                    reconnectTimer <- None
                                    connect ())
                                delayMs)
                | TaClientLifecycleEffect.CancelPoll -> cancelPollTimer ()
                | TaClientLifecycleEffect.CancelTimeout -> cancelTimeoutTimer ()
                | TaClientLifecycleEffect.CancelReconnect -> cancelReconnectTimer ()

        and connect () =
            if not lifecycle.Disposed then
                let value = new WebSocket(syncWebSocketUrl ())
                socket <- Some value

                value.OnOpen <- fun () -> apply TaClientLifecycleEvent.Connected |> ignore

                value.OnMessage <-
                    fun event ->
                        try
                            let response = JSON.Parse(string event.Data) |> As<ExtensionTransientResponseWire>

                            if response.``type`` = "extension-transient" && response.operation = "close" then
                                closeSocket ()
                            elif response.``type`` = "extension-transient" && response.status = "ok" then
                                let wire = JSON.Parse(response.payload) |> As<TaBrowserStateWire>

                                match TaResearchClientWire.stateFromWire wire with
                                | Result.Ok state ->
                                    runtimeState.Value <- state
                                    apply (TaClientLifecycleEvent.StateAccepted state.DataRevision) |> ignore
                                | Result.Error _ ->
                                    apply (TaClientLifecycleEvent.ResyncRequired "invalid-browser-state") |> ignore
                            elif response.``type`` = "extension-transient" then
                                runtimeState.Value <-
                                    { runtimeState.Value with
                                        LastError =
                                            Some
                                                { ReasonCode = "transient-command-failed"
                                                  Message = TaResearchClientWire.text response.error
                                                  Recoverable = true } }
                                apply (TaClientLifecycleEvent.RequestTimedOut DateTimeOffset.UtcNow) |> ignore
                        with _ ->
                            apply (TaClientLifecycleEvent.ResyncRequired "invalid-transient-response") |> ignore

                value.OnClose <-
                    fun () ->
                        socket <- None
                        apply TaClientLifecycleEvent.Disconnected |> ignore

                value.OnError <- fun () -> ()

        let callbacks =
            { SubmitAction =
                fun action ->
                    async {
                        let effects = apply (TaClientLifecycleEvent.StartAction action)
                        let accepted =
                            effects
                            |> Array.exists (function TaClientLifecycleEffect.SendAction _ -> true | _ -> false)

                        if accepted then
                            return Result.Ok()
                        else
                            return
                                Result.Error
                                    { Code = if lifecycle.Connected then "transient-command-busy" else "transient-channel-not-open"
                                      Message = if lifecycle.Connected then "A TA transient command is already in flight." else "TA transient channel is not open." }
                    } }

        TaWorkspaceRenderer.render TaWorkspaceRenderer.defaultOptions callbacks runtimeState
        |> Doc.RunById rootId

        connect ()

        { RuntimeState = runtimeState
          SetActive = fun active -> apply (TaClientLifecycleEvent.ActiveChanged active) |> ignore
          Dispose =
            fun () ->
                apply TaClientLifecycleEvent.Dispose |> ignore

                match socket with
                | Some value when value.ReadyState = WebSocketReadyState.Open ->
                    timeoutTimer <- Some(JS.SetTimeout closeSocket 1000)
                | _ -> closeSocket () }

    let mountById rootId extensionId channelId canvasId =
        mountByIdWithOptions rootId extensionId channelId canvasId TaClientLifecycle.defaults
