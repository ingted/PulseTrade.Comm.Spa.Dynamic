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
      afterDataRevision: int64
      dataRevision: int64
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
                          "freshness", SduiValue.Text(text wire.freshness) ])

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
          afterDataRevision = 0L
          dataRevision = 0L
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
            { emptyFrame "action" "poll-delta" (canvasText canvas) with afterDataRevision = revision }
        | SduiAction.RequestFullSnapshot(canvas, reason) ->
            { emptyFrame "action" "full-snapshot" (canvasText canvas) with reasonCode = reason }

[<JavaScript; RequireQualifiedAccess>]
module TaResearchTransientClient =
    let syncWebSocketUrl () =
        let protocol = if JS.Window.Location.Protocol = "https:" then "wss://" else "ws://"
        protocol + JS.Window.Location.Host + "/sync/ws"

    let mountById rootId extensionId channelId canvasId =
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
        let queued = ResizeArray<string>()

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
            | Some value when value.ReadyState = WebSocketReadyState.Open -> value.Send text
            | _ -> queued.Add text

        let callbacks =
            { SubmitAction =
                fun action ->
                    async {
                        match socket with
                        | Some value when value.ReadyState = WebSocketReadyState.Open ->
                            sendPayload "action" (TaResearchClientWire.actionToWire action)
                            return Result.Ok()
                        | _ ->
                            return
                                Result.Error
                                    { Code = "transient-channel-not-open"
                                      Message = "TA transient channel is not open." }
                    } }

        TaWorkspaceRenderer.render TaWorkspaceRenderer.defaultOptions callbacks runtimeState
        |> Doc.RunById rootId

        let value = new WebSocket(syncWebSocketUrl ())
        socket <- Some value

        value.OnOpen <-
            fun () ->
                sendPayload "open" (TaResearchClientWire.emptyFrame "mounted" "" canvasId)

                while queued.Count > 0 do
                    let frame = queued[0]
                    queued.RemoveAt 0
                    value.Send frame

        value.OnMessage <-
            fun event ->
                try
                    let response = JSON.Parse(string event.Data) |> As<ExtensionTransientResponseWire>

                    if response.``type`` = "extension-transient" && response.status = "ok" then
                        let wire = JSON.Parse(response.payload) |> As<TaBrowserStateWire>

                        match TaResearchClientWire.stateFromWire wire with
                        | Result.Ok state -> runtimeState.Value <- state
                        | Result.Error _ -> ()
                with _ ->
                    ()

        value.OnClose <- fun () -> socket <- None
        value.OnError <- fun () -> ()
