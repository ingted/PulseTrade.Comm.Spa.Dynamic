namespace PulseTrade.Comm.Spa.Dynamic.Ptcs.Client

open System
open PulseTrade.Comm.Spa.Dynamic.Contracts
open PulseTrade.Comm.Spa.Dynamic.Renderer
open WebSharper
open WebSharper.JavaScript
open WebSharper.JavaScript.Dom
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
      hasLineValue: bool
      hasTemporal: bool
      sourceIntervalId: string
      scaleKey: string
      intervalStartUtc: string
      intervalEndUtc: string
      observedThroughUtc: string
      availableAtUtc: string
      hasAvailableAtUtc: bool
      finality: string
      projection: string
      quality: string }

[<JavaScript; CLIMutable>]
type TaBrowserSeriesWire =
    { dataRef: string
      mode: string
      removeBeforeTime: string
      hasRemoveBeforeTime: bool
      points: TaBrowserPointWire array
      pointCount: int
      startIndex: int
      timeIndices: int array
      openValues: float array
      highValues: float array
      lowValues: float array
      closeValues: float array
      volumeValues: float array
      lineValues: float array
      hasTemporal: bool
      sourceIntervalIds: string array
      scaleKeys: string array
      intervalStartUtc: string array
      intervalEndUtc: string array
      observedThroughUtc: string array
      availableAtUtc: string array
      hasAvailableAtUtc: bool array
      finality: string array
      projections: string array
      qualities: string array }

[<JavaScript; CLIMutable>]
type TaBrowserTraceWire =
    { traceId: string
      kind: string
      dataRef: string
      label: string
      color: string
      width: float
      visible: bool }

[<JavaScript; CLIMutable>]
type TaBrowserRowWire =
    { rowId: string
      kind: string
      dataRef: string
      heightWeight: float
      visible: bool
      traces: TaBrowserTraceWire array }

[<JavaScript; CLIMutable>]
type TaBrowserStateWire =
    { wireVersion: string
      updateKind: string
      baseDataRevision: int64
      documentId: string
      canvasInstanceId: string
      workspaceId: string
      title: string
      rowsRef: string
      statusRef: string
      sharedTimeAxis: bool
      rows: TaBrowserRowWire array
      allowedActions: string array
      querySourceId: string
      queryInstrument: string
      queryIntervalMinutes: int
      queryFromUtc: string
      queryToUtcExclusive: string
      queryIncludePartial: bool
      timeline: string array
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
type TaResearchJsonExportWire =
    { schema: string
      exportedAtUtc: string
      documentRevision: int64
      dataRevision: int64
      state: TaBrowserStateWire }

[<JavaScript; CLIMutable>]
type TaBrowserEditorInputWire =
    { path: string
      kind: string
      textValue: string
      numberValue: float
      boolValue: bool }

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
      reasonCode: string
      templateKey: string
      hasTemplateRowId: bool
      editorValues: TaBrowserEditorInputWire array
      expectedDocumentRevision: float
      hasExpectedDocumentRevision: bool }

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
      PollEnabled: bool
      Connected: bool
      Active: bool
      InFlight: bool
      DataRevision: int64
      ReconnectAttempt: int
      DisposePending: bool
      Disposed: bool }

[<JavaScript; RequireQualifiedAccess>]
type TaClientLifecycleEvent =
    | Connected
    | StateAccepted of dataRevision: int64 * pollEnabled: bool
    | CommandRejected
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
    | CloseTransport

[<JavaScript; RequireQualifiedAccess>]
module TaClientLifecycle =
    let defaults =
        { PollIntervalMs = 5000
          RequestTimeoutMs = 150000
          PollRetryMs = 2000
          ReconnectBaseMs = 1000
          ReconnectMaximumMs = 30000 }

    let initial canvasInstanceId =
        { CanvasInstanceId = canvasInstanceId
          Poll = RuntimePollState.Unmounted
          PollEnabled = false
          Connected = false
          Active = true
          InFlight = false
          DataRevision = 0L
          ReconnectAttempt = 0
          DisposePending = false
          Disposed = false }

    let reconnectDelay options attempt =
        let rec expand current remaining =
            if remaining <= 1 then current
            else expand (min options.ReconnectMaximumMs (current * 2)) (remaining - 1)

        expand options.ReconnectBaseMs (max 1 attempt)

    let transition options event state =
        if state.Disposed && event <> TaClientLifecycleEvent.Dispose then
            state, [||]
        elif state.DisposePending then
            match event with
            | TaClientLifecycleEvent.StateAccepted(revision, pollEnabled) ->
                { state with
                    Poll = RuntimePollState.Disposed
                    PollEnabled = pollEnabled
                    InFlight = true
                    DataRevision = revision },
                [| TaClientLifecycleEffect.CancelPoll
                   TaClientLifecycleEffect.CancelTimeout
                   TaClientLifecycleEffect.CancelReconnect
                   TaClientLifecycleEffect.SendUnmounted
                   TaClientLifecycleEffect.ScheduleTimeout options.RequestTimeoutMs |]
            | TaClientLifecycleEvent.RequestTimedOut _
            | TaClientLifecycleEvent.Disconnected ->
                { state with
                    Poll = RuntimePollState.Disposed
                    Connected = false
                    InFlight = false
                    DisposePending = false
                    Disposed = true },
                [| TaClientLifecycleEffect.CancelPoll
                   TaClientLifecycleEffect.CancelTimeout
                   TaClientLifecycleEffect.CancelReconnect
                   TaClientLifecycleEffect.CloseTransport |]
            | TaClientLifecycleEvent.Dispose -> state, [||]
            | _ -> state, [||]
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
            | TaClientLifecycleEvent.StateAccepted(revision, pollEnabled) ->
                let nextPoll = if state.Active && pollEnabled then RuntimePollState.Ready else RuntimePollState.Suspended
                let schedule = if state.Active && pollEnabled then [| TaClientLifecycleEffect.SchedulePoll options.PollIntervalMs |] else [||]

                { state with Poll = nextPoll; PollEnabled = pollEnabled; InFlight = false; DataRevision = revision },
                Array.append [| TaClientLifecycleEffect.CancelTimeout |] schedule
            | TaClientLifecycleEvent.CommandRejected when state.Connected && state.InFlight ->
                let nextPoll = if state.Active && state.PollEnabled then RuntimePollState.Ready else RuntimePollState.Suspended
                let schedule = if state.Active && state.PollEnabled then [| TaClientLifecycleEffect.SchedulePoll options.PollIntervalMs |] else [||]

                { state with Poll = nextPoll; InFlight = false },
                Array.append [| TaClientLifecycleEffect.CancelTimeout |] schedule
            | TaClientLifecycleEvent.StartAction action when state.Connected && state.Active && not state.InFlight ->
                { state with Poll = RuntimePollState.PollInFlight; InFlight = true },
                [| TaClientLifecycleEffect.CancelPoll
                   TaClientLifecycleEffect.SendAction action
                   TaClientLifecycleEffect.ScheduleTimeout options.RequestTimeoutMs |]
            | TaClientLifecycleEvent.PollDue _ when state.Connected && state.Active && state.PollEnabled && not state.InFlight ->
                { state with Poll = RuntimePollState.PollInFlight; InFlight = true },
                [| TaClientLifecycleEffect.SendAction(SduiAction.PollDelta(state.CanvasInstanceId, state.DataRevision))
                   TaClientLifecycleEffect.ScheduleTimeout options.RequestTimeoutMs |]
            | TaClientLifecycleEvent.RequestTimedOut _ when state.InFlight ->
                let attempt = state.ReconnectAttempt + 1
                { state with
                    Poll = RuntimePollState.Suspended
                    Connected = false
                    InFlight = false
                    ReconnectAttempt = attempt },
                [| TaClientLifecycleEffect.CancelPoll
                   TaClientLifecycleEffect.CancelTimeout
                   TaClientLifecycleEffect.CancelReconnect
                   TaClientLifecycleEffect.CloseTransport
                   TaClientLifecycleEffect.ScheduleReconnect(reconnectDelay options attempt) |]
            | TaClientLifecycleEvent.Disconnected when not state.Connected ->
                state, [||]
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
                if active && state.Connected && state.PollEnabled && not state.InFlight then
                    { state with Active = true; Poll = RuntimePollState.Ready },
                    [| TaClientLifecycleEffect.SchedulePoll options.PollIntervalMs |]
                elif active then
                    { state with Active = true }, [||]
                else
                    { state with Active = false; Poll = RuntimePollState.Suspended },
                    [| TaClientLifecycleEffect.CancelPoll |]
            | TaClientLifecycleEvent.ResyncRequired reason when state.Connected && state.Active ->
                { state with Poll = RuntimePollState.PausedForResync; InFlight = true },
                [| TaClientLifecycleEffect.CancelPoll
                   TaClientLifecycleEffect.CancelTimeout
                   TaClientLifecycleEffect.SendAction(SduiAction.RequestFullSnapshot(state.CanvasInstanceId, reason))
                   TaClientLifecycleEffect.ScheduleTimeout options.RequestTimeoutMs |]
            | TaClientLifecycleEvent.Dispose ->
                if state.Connected then
                    { state with
                        Poll = RuntimePollState.Disposed
                        Active = false
                        InFlight = true
                        DisposePending = true },
                    [| TaClientLifecycleEffect.CancelPoll
                       TaClientLifecycleEffect.CancelReconnect
                       if not state.InFlight then
                           TaClientLifecycleEffect.SendUnmounted
                           TaClientLifecycleEffect.ScheduleTimeout options.RequestTimeoutMs |]
                else
                    { state with
                        Poll = RuntimePollState.Disposed
                        Connected = false
                        Active = false
                        InFlight = false
                        Disposed = true },
                    [| TaClientLifecycleEffect.CancelPoll
                       TaClientLifecycleEffect.CancelTimeout
                       TaClientLifecycleEffect.CancelReconnect
                       TaClientLifecycleEffect.CloseTransport |]
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

    let rowKindText = function
        | TaRowKind.Candlestick -> "candlestick"
        | TaRowKind.Volume -> "volume"
        | TaRowKind.Sma -> "sma"
        | TaRowKind.Dmi -> "dmi"
        | TaRowKind.Adx -> "adx"
        | TaRowKind.Macd -> "macd"
        | TaRowKind.HeikinAshi -> "heikin-ashi"

    let traceKind value =
        match text value with
        | "volume" -> TaTraceKind.Volume
        | "line" -> TaTraceKind.Line
        | "histogram" -> TaTraceKind.Histogram
        | _ -> TaTraceKind.Candlestick

    let pollState value =
        match text value with
        | "mounted-idle" -> RuntimePollState.MountedIdle
        | "ready" -> RuntimePollState.Ready
        | "poll-in-flight" -> RuntimePollState.PollInFlight
        | "suspended" -> RuntimePollState.Suspended
        | "paused-for-resync" -> RuntimePollState.PausedForResync
        | "disposed" -> RuntimePollState.Disposed
        | _ -> RuntimePollState.Unmounted

    let pointPayload time hasOpen openValue hasHigh highValue hasLow lowValue hasClose closeValue hasVolume volumeValue hasLineValue lineValue =
        [ if not (String.IsNullOrWhiteSpace time) then "t", SduiValue.Text time
          if hasOpen then "o", SduiValue.Number openValue
          if hasHigh then "h", SduiValue.Number highValue
          if hasLow then "l", SduiValue.Number lowValue
          if hasClose then "c", SduiValue.Number closeValue
          if hasVolume then "v", SduiValue.Number volumeValue
          if hasLineValue then "v", SduiValue.Number lineValue ]
        |> Map.ofList
        |> SduiValue.Object

    let temporalPointValue sourceIntervalId scaleKey intervalStartUtc intervalEndUtc observedThroughUtc availableAtUtc hasAvailableAtUtc finality projection quality payload =
        [ "_type", SduiValue.Text "temporal-point.v1"
          "sourceIntervalId", SduiValue.Text sourceIntervalId
          "scaleKey", SduiValue.Text scaleKey
          "intervalStartUtc", SduiValue.Text intervalStartUtc
          "intervalEndUtc", SduiValue.Text intervalEndUtc
          "observedThroughUtc", SduiValue.Text observedThroughUtc
          "finality", SduiValue.Text finality
          "projection", SduiValue.Text projection
          "value", payload
          if hasAvailableAtUtc then "availableAtUtc", SduiValue.Text availableAtUtc
          if not (String.IsNullOrWhiteSpace quality) then "quality", SduiValue.Text quality ]
        |> Map.ofList
        |> SduiValue.Object

    let pointValue (point: TaBrowserPointWire) =
        let payload =
            pointPayload point.time point.hasOpen point.openValue point.hasHigh point.highValue point.hasLow point.lowValue point.hasClose point.closeValue point.hasVolume point.volumeValue point.hasLineValue point.lineValue

        if point.hasTemporal then
            temporalPointValue point.sourceIntervalId point.scaleKey point.intervalStartUtc point.intervalEndUtc point.observedThroughUtc point.availableAtUtc point.hasAvailableAtUtc point.finality point.projection point.quality payload
        else payload

    let temporalSeriesMetadataIsValid count (series: TaBrowserSeriesWire) =
        not series.hasTemporal
        || (not (isNull series.sourceIntervalIds) && series.sourceIntervalIds.Length = count
            && not (isNull series.scaleKeys) && series.scaleKeys.Length = count
            && not (isNull series.intervalStartUtc) && series.intervalStartUtc.Length = count
            && not (isNull series.intervalEndUtc) && series.intervalEndUtc.Length = count
            && not (isNull series.observedThroughUtc) && series.observedThroughUtc.Length = count
            && not (isNull series.availableAtUtc) && series.availableAtUtc.Length = count
            && not (isNull series.hasAvailableAtUtc) && series.hasAvailableAtUtc.Length = count
            && not (isNull series.finality) && series.finality.Length = count
            && not (isNull series.projections) && series.projections.Length = count
            && not (isNull series.qualities) && series.qualities.Length = count)

    let columnarPointValues (timeline: string array) (series: TaBrowserSeriesWire) =
        let timeline = if isNull timeline then [||] else timeline
        let count = max 0 series.pointCount
        let indices = if isNull series.timeIndices then [||] else series.timeIndices

        Array.init count (fun offset ->
            let timelineIndex =
                if indices.Length = count then indices[offset]
                else series.startIndex + offset
            let timestamp =
                if timelineIndex >= 0 && timelineIndex < timeline.Length then text timeline[timelineIndex]
                else ""
            let payload =
                pointPayload
                    timestamp
                    (not (isNull series.openValues) && series.openValues.Length = count)
                    (if isNull series.openValues || series.openValues.Length <> count then 0.0 else series.openValues[offset])
                    (not (isNull series.highValues) && series.highValues.Length = count)
                    (if isNull series.highValues || series.highValues.Length <> count then 0.0 else series.highValues[offset])
                    (not (isNull series.lowValues) && series.lowValues.Length = count)
                    (if isNull series.lowValues || series.lowValues.Length <> count then 0.0 else series.lowValues[offset])
                    (not (isNull series.closeValues) && series.closeValues.Length = count)
                    (if isNull series.closeValues || series.closeValues.Length <> count then 0.0 else series.closeValues[offset])
                    (not (isNull series.volumeValues) && series.volumeValues.Length = count)
                    (if isNull series.volumeValues || series.volumeValues.Length <> count then 0.0 else series.volumeValues[offset])
                    (not (isNull series.lineValues) && series.lineValues.Length = count)
                    (if isNull series.lineValues || series.lineValues.Length <> count then 0.0 else series.lineValues[offset])

            if series.hasTemporal then
                temporalPointValue series.sourceIntervalIds[offset] series.scaleKeys[offset] series.intervalStartUtc[offset] series.intervalEndUtc[offset] series.observedThroughUtc[offset] series.availableAtUtc[offset] series.hasAvailableAtUtc[offset] series.finality[offset] series.projections[offset] series.qualities[offset] payload
            else payload)

    let seriesPointValues (wire: TaBrowserStateWire) (series: TaBrowserSeriesWire) =
        if wire.wireVersion = "ta-browser.v3" || wire.wireVersion = "ta-browser.v4" then columnarPointValues wire.timeline series
        elif isNull series.points then [||]
        else series.points |> Array.map pointValue

    let stateFromWire (wire: TaBrowserStateWire) =
        if isNull (box wire) || (wire.wireVersion <> "ta-browser.v1" && wire.wireVersion <> "ta-browser.v2" && wire.wireVersion <> "ta-browser.v3" && wire.wireVersion <> "ta-browser.v4") then
            Result.Error "Unsupported TA browser state wire."
        elif wire.wireVersion = "ta-browser.v4"
             && not (isNull wire.series)
             && wire.series |> Array.exists (fun series -> not (temporalSeriesMetadataIsValid (max 0 series.pointCount) series)) then
            Result.Error "TA browser temporal metadata arrays do not match pointCount."
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
                          Traces =
                            if isNull row.traces then [||]
                            else
                                row.traces
                                |> Array.map (fun trace ->
                                    { TraceId = text trace.traceId
                                      Kind = traceKind trace.kind
                                      DataRef = text trace.dataRef
                                      Label = text trace.label
                                      Color = text trace.color
                                      Width = trace.width
                                      Visible = trace.visible
                                      Options = Map.empty })
                          Options = Map.empty })

            let seriesData =
                if isNull wire.series then Map.empty
                else
                    wire.series
                    |> Array.map (fun series ->
                        let points = seriesPointValues wire series

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
            let defaultView =
                [ if not (String.IsNullOrWhiteSpace wire.querySourceId) then
                      "query.sourceId", SduiValue.Text(text wire.querySourceId)
                  if not (String.IsNullOrWhiteSpace wire.queryInstrument) then
                      "query.instrument", SduiValue.Text(text wire.queryInstrument)
                  if wire.queryIntervalMinutes > 0 then
                      "query.intervalMinutes", SduiValue.Number(float wire.queryIntervalMinutes)
                  if not (String.IsNullOrWhiteSpace wire.queryFromUtc) then
                      "query.fromUtc", SduiValue.Text(text wire.queryFromUtc)
                  if not (String.IsNullOrWhiteSpace wire.queryToUtcExclusive) then
                      "query.toUtcExclusive", SduiValue.Text(text wire.queryToUtcExclusive)
                  "query.includePartial", SduiValue.Bool wire.queryIncludePartial ]
                |> Map.ofList
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
                          DefaultView = defaultView }
                  Data = data
                  DocumentRevision = wire.documentRevision
                  DataRevision = wire.dataRevision
                  LastTransportSequence = wire.transportSequence
                  View = { Values = Map.empty }
                  Poll = pollState wire.pollKind
                  LastError = lastError }

    let pointTime value =
        match value with
        | SduiValue.Object values ->
            match Map.tryFind "_type" values, Map.tryFind "intervalStartUtc" values, Map.tryFind "t" values with
            | Some(SduiValue.Text "temporal-point.v1"), Some(SduiValue.Text value), _ -> text value
            | _, _, Some(SduiValue.Text value) -> text value
            | _ -> ""
        | _ -> ""

    let mergeSeries (current: RuntimeState) (timeline: string array) (wire: TaBrowserSeriesWire) =
        let dataRef = text wire.dataRef
        let currentPoints =
            match Map.tryFind dataRef current.Data with
            | Some(SduiValue.Array points) -> points
            | _ -> [||]
        let retained =
            if wire.hasRemoveBeforeTime && not (String.IsNullOrWhiteSpace wire.removeBeforeTime) then
                currentPoints |> Array.filter (fun point -> String.Compare(pointTime point, wire.removeBeforeTime) >= 0)
            else currentPoints
        let incoming =
            if wire.pointCount > 0 then columnarPointValues timeline wire
            elif isNull wire.points then [||]
            else wire.points |> Array.map pointValue
        let merged =
            Array.append retained incoming
            |> Array.filter (pointTime >> String.IsNullOrWhiteSpace >> not)
            |> Array.map (fun point -> pointTime point, point)
            |> Map.ofArray
            |> Map.toArray
            |> Array.map snd

        dataRef, SduiValue.Array merged

    let applyWire (current: RuntimeState) (wire: TaBrowserStateWire) =
        stateFromWire wire
        |> Result.bind (fun decoded ->
            if wire.wireVersion = "ta-browser.v1" || text wire.updateKind = "full" then
                Result.Ok decoded
            elif text wire.updateKind <> "delta" then
                Result.Error "Unsupported TA browser update kind."
            elif wire.baseDataRevision <> current.DataRevision then
                Result.Error "TA browser delta base revision does not match current state."
            elif decoded.Identity <> current.Identity || decoded.DocumentRevision <> current.DocumentRevision then
                Result.Error "TA browser delta document identity or revision changed."
            else
                let mergedSeries =
                    if isNull wire.series then current.Data
                    else
                        wire.series
                        |> Array.fold (fun data series ->
                            let dataRef, value = mergeSeries { current with Data = data } wire.timeline series
                            Map.add dataRef value data) current.Data
                let statusRef = decoded.Document |> Option.map _.StatusRef |> Option.defaultValue "status"
                let mergedData =
                    match Map.tryFind statusRef decoded.Data with
                    | Some status -> Map.add statusRef status mergedSeries
                    | None -> mergedSeries

                Result.Ok
                    { decoded with
                        Data = mergedData
                        View = current.View })

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
          reasonCode = ""
          templateKey = ""
          hasTemplateRowId = false
          editorValues = [||]
          expectedDocumentRevision = 0.0
          hasExpectedDocumentRevision = false }

    let optionText value = value |> Option.defaultValue ""
    let optionInt value = value |> Option.defaultValue 0
    let optionBool value = value |> Option.defaultValue false
    let canvasText (CanvasInstanceId value) = value

    let editorInputToWire (input: EditorInputValue) =
        match input.Value with
        | EditorScalarValue.Text value ->
            { path = input.Path; kind = "text"; textValue = value; numberValue = 0.0; boolValue = false }
        | EditorScalarValue.Number value ->
            { path = input.Path; kind = "number"; textValue = ""; numberValue = value; boolValue = false }
        | EditorScalarValue.Bool value ->
            { path = input.Path; kind = "bool"; textValue = ""; numberValue = 0.0; boolValue = value }

    let actionToWire action =
        match action with
        | SduiAction.ResetView canvas -> emptyFrame "action" "reset-view" (canvasText canvas)
        | SduiAction.ResetCanvas canvas -> emptyFrame "action" "reset-canvas" (canvasText canvas)
        | SduiAction.AddTaRow(canvas, row) ->
            { emptyFrame "action" "add-row" (canvasText canvas) with
                rowId = row.RowId
                rowKind = rowKindText row.Kind
                dataRef = row.DataRef
                heightWeight = row.HeightWeight
                visible = row.Visible }
        | SduiAction.ApplyTemplate(canvas, rowId, templateKey, values) ->
            { emptyFrame "action" "apply-template" (canvasText canvas) with
                rowId = rowId |> Option.defaultValue ""
                templateKey = templateKey
                hasTemplateRowId = rowId.IsSome
                editorValues = (if isNull values then [||] else values) |> Array.map editorInputToWire }
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

    let actionRequestToWire request =
        { actionToWire request.Action with
            expectedDocumentRevision = request.ExpectedDocumentRevision |> Option.map float |> Option.defaultValue 0.0
            hasExpectedDocumentRevision = request.ExpectedDocumentRevision.IsSome }

[<JavaScript>]
type TaResearchTransientClientHandle =
    { RuntimeState: Var<RuntimeState>
      SetActive: bool -> unit
      RequestJsonExport: unit -> Result<string, string>
      Dispose: unit -> unit }

[<JavaScript; RequireQualifiedAccess>]
module TaResearchTransientClient =
    let syncWebSocketUrl () =
        let protocol = if JS.Window.Location.Protocol = "https:" then "wss://" else "ws://"
        protocol + JS.Window.Location.Host + "/sync/ws"

    let twoDigits value =
        if value < 10 then "0" + string value else string value

    let exportFileName () =
        let now = new WebSharper.JavaScript.Date()
        let random = Random()
        let hex = "0123456789abcdef"
        let compactGuid =
            Array.init 32 (fun _ -> hex[random.Next(hex.Length)])
            |> Array.map string
            |> String.concat ""
        let guid =
            compactGuid.Substring(0, 8) + "-"
            + compactGuid.Substring(8, 4) + "-4" + compactGuid.Substring(13, 3) + "-"
            + "8" + compactGuid.Substring(17, 3) + "-" + compactGuid.Substring(20, 12)
        let timestamp =
            string (now.GetFullYear())
            + twoDigits (now.GetMonth() + 1)
            + twoDigits (now.GetDate())
            + twoDigits (now.GetHours())
            + twoDigits (now.GetMinutes())
            + twoDigits (now.GetSeconds())
        timestamp + "-" + guid + ".json"

    let downloadJsonExport (wire: TaBrowserStateWire) =
        if isNull (box JS.Document.Body) then
            Result.Error "Document body is unavailable."
        else
            try
                let export =
                    { schema = "ptcs-ta-research-export.v1"
                      exportedAtUtc = (new WebSharper.JavaScript.Date()).ToISOString()
                      documentRevision = wire.documentRevision
                      dataRevision = wire.dataRevision
                      state = wire }
                let options = new BlobPropertyBag()
                options.Type <- "application/json;charset=utf-8"
                let blob = new Blob([| JSON.Stringify export |], options)
                let url = URL.CreateObjectURL blob
                let anchor = JS.Document.CreateElement("a") |> As<HTMLElement>
                anchor.SetAttribute("href", url)
                anchor.SetAttribute("download", exportFileName ())
                anchor.SetAttribute("aria-hidden", "true")
                anchor.SetAttribute("style", "display:none;")
                JS.Document.Body.AppendChild anchor |> ignore
                anchor.Click()
                JS.Document.Body.RemoveChild anchor |> ignore
                JS.SetTimeout (fun () -> URL.RevokeObjectURL url) 250 |> ignore
                Result.Ok "TA Research JSON download started."
            with error ->
                Result.Error error.Message

    let mountCore (mountDocument: Doc -> unit) extensionId channelId canvasId lifecycleOptions disposeAfterJsonExport =
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
        let mutable jsonExportRequested = false
        let mutable jsonExportBootstrapAttempts = 0
        let mutable jsonExportBootstrapInFlight = false
        let mutable jsonExportInFlight = false
        let mutable actionRequestOverride: DynamicActionRequest option = None
        let mutable pendingActionCompletion: (string * (Result<DynamicActionResult, DynamicHostError> -> unit)) option = None

        let nextRequestId () =
            requestSequence <- requestSequence + 1
            channelId + ":" + string requestSequence

        let sendPayloadWithRequestId requestId operation payload =
            let request =
                { ``type`` = "extension-transient"
                  requestId = requestId
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

        let sendPayload operation payload =
            sendPayloadWithRequestId (nextRequestId ()) operation payload

        let completePendingAction requestId result =
            match pendingActionCompletion with
            | Some(pendingRequestId, continuation) when pendingRequestId = requestId ->
                pendingActionCompletion <- None
                continuation (Ok result)
            | _ -> ()

        let failPendingAction code message =
            match pendingActionCompletion with
            | Some(_, continuation) ->
                pendingActionCompletion <- None
                continuation (Result.Error { Code = code; Message = message })
            | None -> ()

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
            match event with
            | TaClientLifecycleEvent.CommandRejected
            | TaClientLifecycleEvent.RequestTimedOut _
            | TaClientLifecycleEvent.Disconnected
            | TaClientLifecycleEvent.Dispose ->
                jsonExportRequested <- false
                jsonExportBootstrapAttempts <- 0
                jsonExportBootstrapInFlight <- false
                jsonExportInFlight <- false
                match event with
                | TaClientLifecycleEvent.RequestTimedOut _ -> failPendingAction "transient-command-timeout" "The TA action response timed out."
                | TaClientLifecycleEvent.Disconnected -> failPendingAction "transient-channel-disconnected" "The TA transient channel disconnected before the action completed."
                | TaClientLifecycleEvent.Dispose -> failPendingAction "transient-channel-disposed" "The TA transient channel was disposed before the action completed."
                | _ -> ()
            | _ -> ()

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
                    let requestId, payload =
                        match actionRequestOverride with
                        | Some request ->
                            actionRequestOverride <- None
                            request.RequestId, TaResearchClientWire.actionRequestToWire request
                        | None -> nextRequestId (), TaResearchClientWire.actionToWire action
                    if not (sendPayloadWithRequestId requestId "action" payload) then
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
                | TaClientLifecycleEffect.CloseTransport -> closeSocket ()

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

                                match TaResearchClientWire.applyWire runtimeState.Value wire with
                                | Result.Ok state ->
                                    runtimeState.Value <- state

                                    let completesJsonExport = jsonExportInFlight
                                    let completesJsonExportBootstrap = jsonExportBootstrapInFlight
                                    let mutable jsonExportCompleted = false

                                    if completesJsonExportBootstrap then
                                        jsonExportBootstrapInFlight <- false

                                    if completesJsonExport then
                                        jsonExportRequested <- false
                                        jsonExportInFlight <- false

                                        if wire.updateKind = "full" then
                                            match downloadJsonExport wire with
                                            | Result.Ok _ -> jsonExportCompleted <- true
                                            | Result.Error message ->
                                                runtimeState.Value <-
                                                    { runtimeState.Value with
                                                        LastError =
                                                            Some
                                                                { ReasonCode = "ta-export-download-failed"
                                                                  Message = message
                                                                  Recoverable = true } }
                                        else
                                            runtimeState.Value <-
                                                { runtimeState.Value with
                                                    LastError =
                                                        Some
                                                            { ReasonCode = "ta-export-full-state-required"
                                                              Message = "The TA export response was not a full runtime state."
                                                              Recoverable = true } }

                                    let pollEnabled =
                                        state.Document
                                        |> Option.exists (fun document ->
                                            document.AllowedActions
                                            |> Array.exists (fun action -> action = "poll-delta"))

                                    apply (TaClientLifecycleEvent.StateAccepted(state.DataRevision, pollEnabled)) |> ignore
                                    completePendingAction
                                        response.requestId
                                        (DynamicActionResult.Accepted(response.requestId, state.DocumentRevision))

                                    if jsonExportCompleted && disposeAfterJsonExport then
                                        apply TaClientLifecycleEvent.Dispose |> ignore
                                    else
                                        tryStartJsonExport ()
                                | Result.Error _ ->
                                    apply (TaClientLifecycleEvent.ResyncRequired "invalid-browser-state") |> ignore
                            elif response.``type`` = "extension-transient" then
                                let responseError = TaResearchClientWire.text response.error
                                let conflictPrefix = "ta-revision-conflict:"
                                let actionResult =
                                    if responseError.StartsWith conflictPrefix then
                                        match Double.TryParse(responseError.Substring(conflictPrefix.Length)) with
                                        | true, revision when revision >= 0.0 && Math.Truncate revision = revision ->
                                            DynamicActionResult.RevisionConflict(response.requestId, int64 revision)
                                        | _ -> DynamicActionResult.Rejected(response.requestId, "transient-command-failed", responseError)
                                    else
                                        DynamicActionResult.Rejected(response.requestId, "transient-command-failed", responseError)
                                completePendingAction response.requestId actionResult
                                runtimeState.Value <-
                                    { runtimeState.Value with
                                        LastError =
                                            Some
                                                { ReasonCode = "transient-command-failed"
                                                  Message = TaResearchClientWire.text response.error
                                                  Recoverable = true } }
                                apply TaClientLifecycleEvent.CommandRejected |> ignore
                        with _ ->
                            failPendingAction "invalid-transient-response" "The TA transient response could not be decoded."
                            apply (TaClientLifecycleEvent.ResyncRequired "invalid-transient-response") |> ignore

                value.OnClose <-
                    fun () ->
                        socket <- None
                        apply TaClientLifecycleEvent.Disconnected |> ignore

                value.OnError <- fun () -> ()

        and tryStartJsonExport () =
            if jsonExportRequested && not jsonExportBootstrapInFlight && not jsonExportInFlight && lifecycle.Connected && lifecycle.Active && not lifecycle.InFlight then
                let hasRuntimeData = runtimeState.Value.DataRevision > 0L

                if not hasRuntimeData && jsonExportBootstrapAttempts >= 3 then
                    jsonExportRequested <- false
                    jsonExportBootstrapInFlight <- false
                    runtimeState.Value <-
                        { runtimeState.Value with
                            LastError =
                                Some
                                    { ReasonCode = "ta-export-bootstrap-empty"
                                      Message = "TA export bootstrap returned no runtime data after three attempts."
                                      Recoverable = true } }
                    if disposeAfterJsonExport then
                        apply TaClientLifecycleEvent.Dispose |> ignore
                else
                    let action =
                        if hasRuntimeData then
                            SduiAction.RequestFullSnapshot(identity.CanvasInstanceId, "json-export")
                        else
                            SduiAction.PollDelta(identity.CanvasInstanceId, runtimeState.Value.DataRevision)
                    let effects = apply (TaClientLifecycleEvent.StartAction action)
                    let accepted =
                        effects
                        |> Array.exists (function TaClientLifecycleEffect.SendAction _ -> true | _ -> false)

                    if accepted then
                        if hasRuntimeData then
                            jsonExportRequested <- false
                            jsonExportInFlight <- true
                        else
                            jsonExportBootstrapAttempts <- jsonExportBootstrapAttempts + 1
                            jsonExportBootstrapInFlight <- true

        let callbacks =
            { SubmitAction =
                fun request ->
                    Async.FromContinuations(fun (continuation, _, _) ->
                        if pendingActionCompletion.IsSome then
                            continuation
                                (Result.Error
                                    { Code = "transient-command-busy"
                                      Message = "A TA transient command is already in flight." })
                        else
                            pendingActionCompletion <- Some(request.RequestId, continuation)
                            actionRequestOverride <- Some request
                            let effects = apply (TaClientLifecycleEvent.StartAction request.Action)
                            let accepted =
                                effects
                                |> Array.exists (function TaClientLifecycleEffect.SendAction _ -> true | _ -> false)

                            if not accepted then
                                actionRequestOverride <- None
                                pendingActionCompletion <- None
                                continuation
                                    (Result.Error
                                        { Code = if lifecycle.Connected then "transient-command-busy" else "transient-channel-not-open"
                                          Message = if lifecycle.Connected then "A TA transient command is already in flight." else "TA transient channel is not open." })) }

        TaWorkspaceRenderer.render TaWorkspaceRenderer.defaultOptions callbacks runtimeState
        |> mountDocument

        connect ()

        let requestJsonExport () =
            if jsonExportRequested || jsonExportBootstrapInFlight || jsonExportInFlight then
                Result.Error "A TA Research JSON export is already pending."
            elif lifecycle.Disposed || lifecycle.DisposePending then
                Result.Error "The TA transient channel has already been disposed."
            else
                jsonExportBootstrapAttempts <- 0
                jsonExportRequested <- true
                tryStartJsonExport ()
                Result.Ok "TA Research JSON export requested."

        { RuntimeState = runtimeState
          SetActive = fun active -> apply (TaClientLifecycleEvent.ActiveChanged active) |> ignore
          RequestJsonExport = requestJsonExport
          Dispose =
             fun () -> apply TaClientLifecycleEvent.Dispose |> ignore }

    let mountWithOptions (mountDocument: Doc -> unit) extensionId channelId canvasId lifecycleOptions =
        mountCore mountDocument extensionId channelId canvasId lifecycleOptions false

    let requestJsonExportOnce extensionId channelId canvasId lifecycleOptions =
        let handle = mountCore ignore extensionId channelId canvasId lifecycleOptions true
        handle.RequestJsonExport()

    let mountOnElementWithOptions (root: Element) extensionId channelId canvasId lifecycleOptions =
        mountWithOptions (Doc.Run root) extensionId channelId canvasId lifecycleOptions

    let mountByIdWithOptions rootId extensionId channelId canvasId lifecycleOptions =
        mountWithOptions (Doc.RunById rootId) extensionId channelId canvasId lifecycleOptions

    let mountById rootId extensionId channelId canvasId =
        mountByIdWithOptions rootId extensionId channelId canvasId TaClientLifecycle.defaults
