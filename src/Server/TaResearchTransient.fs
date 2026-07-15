namespace PulseTrade.Comm.Spa.Dynamic

open System
open System.Collections.Generic
open System.Text.Json
open System.Text.Json.Serialization
open System.Text.Json.Serialization.Metadata
open PulseTrade.Comm.Spa
open PulseTrade.Comm.Spa.Dynamic.Contracts

[<CLIMutable>]
type TaTransientFieldWire =
    { key: string
      value: TaTransientValueWire }

and [<CLIMutable>] TaTransientValueWire =
    { kind: string
      boolValue: bool
      numberValue: float
      textValue: string
      items: TaTransientValueWire array
      fields: TaTransientFieldWire array }

[<CLIMutable>]
type TaTransientTraceWire =
    { traceId: string
      kind: string
      dataRef: string
      label: string
      color: string
      width: float
      visible: bool
      options: TaTransientFieldWire array }

[<CLIMutable>]
type TaTransientRowWire =
    { rowId: string
      kind: string
      dataRef: string
      heightWeight: float
      visible: bool
      options: TaTransientFieldWire array
      traces: TaTransientTraceWire array }

[<CLIMutable>]
type TaTransientDocumentWire =
    { workspaceId: string
      title: string
      rowsRef: string
      statusRef: string
      sharedTimeAxis: bool
      rows: TaTransientRowWire array
      allowedActions: string array
      defaultView: TaTransientFieldWire array }

[<CLIMutable>]
type TaTransientErrorWire =
    { reasonCode: string
      message: string
      recoverable: bool }

[<CLIMutable>]
type TaTransientStateWire =
    { documentId: string
      canvasInstanceId: string
      document: TaTransientDocumentWire
      data: TaTransientFieldWire array
      documentRevision: int64
      dataRevision: int64
      transportSequence: int64
      view: TaTransientFieldWire array
      pollKind: string
      retryAtUtc: string
      error: TaTransientErrorWire }

[<CLIMutable>]
type TaTransientQueryWire =
    { sourceId: string
      instrument: string
      intervalMinutes: int
      fromUtc: string
      toUtcExclusive: string
      includePartial: bool
      hasSourceId: bool
      hasInstrument: bool
      hasIntervalMinutes: bool
      hasFromUtc: bool
      hasToUtcExclusive: bool
      hasIncludePartial: bool }

[<CLIMutable>]
type TaTransientClientFrameWire =
    { kind: string
      actionKind: string
      canvasInstanceId: string
      row: TaTransientRowWire
      rowId: string
      query: TaTransientQueryWire
      afterDataRevision: int64
      dataRevision: int64
      reasonCode: string }

[<CLIMutable>]
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

[<CLIMutable>]
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
      lineValues: float array }

[<CLIMutable>]
type TaBrowserTraceWire =
    { traceId: string
      kind: string
      dataRef: string
      label: string
      color: string
      width: float
      visible: bool }

[<CLIMutable>]
type TaBrowserRowWire =
    { rowId: string
      kind: string
      dataRef: string
      heightWeight: float
      visible: bool
      traces: TaBrowserTraceWire array }

[<CLIMutable>]
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

[<CLIMutable>]
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

[<RequireQualifiedAccess>]
module TaResearchTransientWire =
    let text value = if isNull value then "" else value

    let emptyValue () =
        { kind = "null"
          boolValue = false
          numberValue = 0.0
          textValue = ""
          items = [||]
          fields = [||] }

    let rec valueToWire (value: SduiValue) : TaTransientValueWire =
        match value with
        | SduiValue.Null -> emptyValue ()
        | SduiValue.Bool current -> { emptyValue () with kind = "bool"; boolValue = current }
        | SduiValue.Number current -> { emptyValue () with kind = "number"; numberValue = current }
        | SduiValue.Text current -> { emptyValue () with kind = "text"; textValue = text current }
        | SduiValue.Array current -> { emptyValue () with kind = "array"; items = current |> Array.map valueToWire }
        | SduiValue.Object current -> { emptyValue () with kind = "object"; fields = mapToWire current }

    and mapToWire (values: Map<string, SduiValue>) : TaTransientFieldWire array =
        values
        |> Map.toArray
        |> Array.map (fun (key, value) -> { key = key; value = valueToWire value })

    let rec valueFromWire (wire: TaTransientValueWire) : SduiValue =
        if isNull (box wire) then
            SduiValue.Null
        else
            match text wire.kind with
            | "bool" -> SduiValue.Bool wire.boolValue
            | "number" -> SduiValue.Number wire.numberValue
            | "text" -> SduiValue.Text(text wire.textValue)
            | "array" -> wire.items |> Array.map valueFromWire |> SduiValue.Array
            | "object" -> wire.fields |> mapFromWire |> SduiValue.Object
            | _ -> SduiValue.Null

    and mapFromWire (values: TaTransientFieldWire array) : Map<string, SduiValue> =
        if isNull values then
            Map.empty
        else
            values |> Array.map (fun field -> text field.key, valueFromWire field.value) |> Map.ofArray

    let rowKindText = function
        | TaRowKind.Candlestick -> "candlestick"
        | TaRowKind.Volume -> "volume"
        | TaRowKind.Sma -> "sma"
        | TaRowKind.Dmi -> "dmi"
        | TaRowKind.Adx -> "adx"
        | TaRowKind.Macd -> "macd"
        | TaRowKind.HeikinAshi -> "heikin-ashi"

    let rowKind value =
        match text value |> fun item -> item.Trim().ToLowerInvariant() with
        | "volume" -> TaRowKind.Volume
        | "sma" -> TaRowKind.Sma
        | "dmi" -> TaRowKind.Dmi
        | "adx" -> TaRowKind.Adx
        | "macd" -> TaRowKind.Macd
        | "heikin-ashi" -> TaRowKind.HeikinAshi
        | _ -> TaRowKind.Candlestick

    let traceKindText = function
        | TaTraceKind.Candlestick -> "candlestick"
        | TaTraceKind.Volume -> "volume"
        | TaTraceKind.Line -> "line"
        | TaTraceKind.Histogram -> "histogram"

    let traceKind value =
        match text value |> fun item -> item.Trim().ToLowerInvariant() with
        | "volume" -> TaTraceKind.Volume
        | "line" -> TaTraceKind.Line
        | "histogram" -> TaTraceKind.Histogram
        | _ -> TaTraceKind.Candlestick

    let traceToWire (trace: TaTraceSpec) : TaTransientTraceWire =
        { traceId = trace.TraceId
          kind = traceKindText trace.Kind
          dataRef = trace.DataRef
          label = trace.Label
          color = trace.Color
          width = trace.Width
          visible = trace.Visible
          options = mapToWire trace.Options }

    let traceFromWire (wire: TaTransientTraceWire) : TaTraceSpec =
        { TraceId = text wire.traceId
          Kind = traceKind wire.kind
          DataRef = text wire.dataRef
          Label = text wire.label
          Color = text wire.color
          Width = wire.width
          Visible = wire.visible
          Options = mapFromWire wire.options }

    let rowToWire (row: TaRowSpec) : TaTransientRowWire =
        { rowId = row.RowId
          kind = rowKindText row.Kind
          dataRef = row.DataRef
          heightWeight = row.HeightWeight
          visible = row.Visible
          options = mapToWire row.Options
          traces =
            if isNull row.Traces then [||]
            else row.Traces |> Array.map traceToWire }

    let rowFromWire (wire: TaTransientRowWire) : TaRowSpec =
        { RowId = text wire.rowId
          Kind = rowKind wire.kind
          DataRef = text wire.dataRef
          HeightWeight = wire.heightWeight
          Visible = wire.visible
          Options = mapFromWire wire.options
          Traces =
            if isNull wire.traces then [||]
            else wire.traces |> Array.map traceFromWire }

    let documentToWire (document: TaWorkspaceDocument) : TaTransientDocumentWire =
        { workspaceId = document.WorkspaceId
          title = document.Title
          rowsRef = document.RowsRef
          statusRef = document.StatusRef
          sharedTimeAxis = document.SharedTimeAxis
          rows = document.Rows |> Array.map rowToWire
          allowedActions = document.AllowedActions
          defaultView = mapToWire document.DefaultView }

    let documentFromWire (wire: TaTransientDocumentWire) : TaWorkspaceDocument =
        { WorkspaceId = text wire.workspaceId
          Title = text wire.title
          RowsRef = text wire.rowsRef
          StatusRef = text wire.statusRef
          SharedTimeAxis = wire.sharedTimeAxis
          Rows = if isNull wire.rows then [||] else wire.rows |> Array.map rowFromWire
          AllowedActions = if isNull wire.allowedActions then [||] else wire.allowedActions
          DefaultView = mapFromWire wire.defaultView }

    let pollToWire = function
        | RuntimePollState.Unmounted -> "unmounted", ""
        | RuntimePollState.MountedIdle -> "mounted-idle", ""
        | RuntimePollState.Ready -> "ready", ""
        | RuntimePollState.PollInFlight -> "poll-in-flight", ""
        | RuntimePollState.Backoff retryAt -> "backoff", retryAt.ToString("O")
        | RuntimePollState.Suspended -> "suspended", ""
        | RuntimePollState.PausedForResync -> "paused-for-resync", ""
        | RuntimePollState.Disposed -> "disposed", ""

    let pollFromWire kind retryAtUtc =
        match text kind with
        | "unmounted" -> RuntimePollState.Unmounted
        | "mounted-idle" -> RuntimePollState.MountedIdle
        | "poll-in-flight" -> RuntimePollState.PollInFlight
        | "backoff" -> RuntimePollState.Suspended
        | "suspended" -> RuntimePollState.Suspended
        | "paused-for-resync" -> RuntimePollState.PausedForResync
        | "disposed" -> RuntimePollState.Disposed
        | _ -> RuntimePollState.Ready

    let stateToWire (state: RuntimeState) : TaTransientStateWire =
        let pollKind, retryAtUtc = pollToWire state.Poll
        let document = state.Document |> Option.map documentToWire |> Option.toObj
        let error =
            state.LastError
            |> Option.map (fun value -> { reasonCode = value.ReasonCode; message = value.Message; recoverable = value.Recoverable })
            |> Option.toObj

        let (DocumentId documentId) = state.Identity.DocumentId
        let (CanvasInstanceId canvasId) = state.Identity.CanvasInstanceId

        { documentId = documentId
          canvasInstanceId = canvasId
          document = document
          data = mapToWire state.Data
          documentRevision = state.DocumentRevision
          dataRevision = state.DataRevision
          transportSequence = state.LastTransportSequence
          view = mapToWire state.View.Values
          pollKind = pollKind
          retryAtUtc = retryAtUtc
          error = error }

    let stateFromWire (wire: TaTransientStateWire) : RuntimeState =
        let document = if isNull (box wire.document) then None else Some(documentFromWire wire.document)
        let error =
            if isNull (box wire.error) then None
            else Some { ReasonCode = text wire.error.reasonCode; Message = text wire.error.message; Recoverable = wire.error.recoverable }

        { Identity =
            { DocumentId = DocumentId(text wire.documentId)
              CanvasInstanceId = CanvasInstanceId(text wire.canvasInstanceId) }
          Document = document
          Data = mapFromWire wire.data
          DocumentRevision = wire.documentRevision
          DataRevision = wire.dataRevision
          LastTransportSequence = wire.transportSequence
          View = { Values = mapFromWire wire.view }
          Poll = pollFromWire wire.pollKind wire.retryAtUtc
          LastError = error }

    let emptyRow () =
        { rowId = ""; kind = ""; dataRef = ""; heightWeight = 0.0; visible = false; options = [||]; traces = [||] }

    let emptyQuery () =
        { sourceId = ""; instrument = ""; intervalMinutes = 0; fromUtc = ""; toUtcExclusive = ""; includePartial = false
          hasSourceId = false; hasInstrument = false; hasIntervalMinutes = false; hasFromUtc = false; hasToUtcExclusive = false; hasIncludePartial = false }

    let queryToWire (query: TaQueryChange) : TaTransientQueryWire =
        { sourceId = query.SourceId |> Option.defaultValue ""
          instrument = query.Instrument |> Option.defaultValue ""
          intervalMinutes = query.IntervalMinutes |> Option.defaultValue 0
          fromUtc = query.FromUtc |> Option.defaultValue ""
          toUtcExclusive = query.ToUtcExclusive |> Option.defaultValue ""
          includePartial = query.IncludePartial |> Option.defaultValue false
          hasSourceId = query.SourceId.IsSome
          hasInstrument = query.Instrument.IsSome
          hasIntervalMinutes = query.IntervalMinutes.IsSome
          hasFromUtc = query.FromUtc.IsSome
          hasToUtcExclusive = query.ToUtcExclusive.IsSome
          hasIncludePartial = query.IncludePartial.IsSome }

    let queryFromWire (wire: TaTransientQueryWire) : TaQueryChange =
        { SourceId = if wire.hasSourceId then Some(text wire.sourceId) else None
          Instrument = if wire.hasInstrument then Some(text wire.instrument) else None
          IntervalMinutes = if wire.hasIntervalMinutes then Some wire.intervalMinutes else None
          FromUtc = if wire.hasFromUtc then Some(text wire.fromUtc) else None
          ToUtcExclusive = if wire.hasToUtcExclusive then Some(text wire.toUtcExclusive) else None
          IncludePartial = if wire.hasIncludePartial then Some wire.includePartial else None }

    let emptyClientFrame kind canvasId =
        { kind = kind
          actionKind = ""
          canvasInstanceId = canvasId
          row = emptyRow ()
          rowId = ""
          query = emptyQuery ()
          afterDataRevision = 0L
          dataRevision = 0L
          reasonCode = "" }

    let clientFrameToWire (frame: RuntimeClientFrame) : TaTransientClientFrameWire =
        match frame with
        | RuntimeClientFrame.Mounted(CanvasInstanceId canvasId) -> emptyClientFrame "mounted" canvasId
        | RuntimeClientFrame.Unmounted(CanvasInstanceId canvasId) -> emptyClientFrame "unmounted" canvasId
        | RuntimeClientFrame.PollCompleted(CanvasInstanceId canvasId, revision) ->
            { emptyClientFrame "poll-completed" canvasId with dataRevision = revision }
        | RuntimeClientFrame.Action action ->
            match action with
            | SduiAction.ResetView(CanvasInstanceId canvasId) -> { emptyClientFrame "action" canvasId with actionKind = "reset-view" }
            | SduiAction.ResetCanvas(CanvasInstanceId canvasId) -> { emptyClientFrame "action" canvasId with actionKind = "reset-canvas" }
            | SduiAction.AddTaRow(CanvasInstanceId canvasId, row) -> { emptyClientFrame "action" canvasId with actionKind = "add-row"; row = rowToWire row }
            | SduiAction.RemoveTaRow(CanvasInstanceId canvasId, rowId) -> { emptyClientFrame "action" canvasId with actionKind = "remove-row"; rowId = rowId }
            | SduiAction.ChangeTaQuery(CanvasInstanceId canvasId, query) -> { emptyClientFrame "action" canvasId with actionKind = "change-query"; query = queryToWire query }
            | SduiAction.PollDelta(CanvasInstanceId canvasId, revision) -> { emptyClientFrame "action" canvasId with actionKind = "poll-delta"; afterDataRevision = revision }
            | SduiAction.RequestFullSnapshot(CanvasInstanceId canvasId, reason) -> { emptyClientFrame "action" canvasId with actionKind = "full-snapshot"; reasonCode = reason }

    let clientFrameFromWire (wire: TaTransientClientFrameWire) : Result<RuntimeClientFrame, string> =
        let canvas = CanvasInstanceId(text wire.canvasInstanceId)

        match text wire.kind, text wire.actionKind with
        | "mounted", _ -> Ok(RuntimeClientFrame.Mounted canvas)
        | "unmounted", _ -> Ok(RuntimeClientFrame.Unmounted canvas)
        | "poll-completed", _ -> Ok(RuntimeClientFrame.PollCompleted(canvas, wire.dataRevision))
        | "action", "reset-view" -> Ok(RuntimeClientFrame.Action(SduiAction.ResetView canvas))
        | "action", "reset-canvas" -> Ok(RuntimeClientFrame.Action(SduiAction.ResetCanvas canvas))
        | "action", "add-row" -> Ok(RuntimeClientFrame.Action(SduiAction.AddTaRow(canvas, rowFromWire wire.row)))
        | "action", "remove-row" -> Ok(RuntimeClientFrame.Action(SduiAction.RemoveTaRow(canvas, text wire.rowId)))
        | "action", "change-query" -> Ok(RuntimeClientFrame.Action(SduiAction.ChangeTaQuery(canvas, queryFromWire wire.query)))
        | "action", "poll-delta" -> Ok(RuntimeClientFrame.Action(SduiAction.PollDelta(canvas, wire.afterDataRevision)))
        | "action", "full-snapshot" -> Ok(RuntimeClientFrame.Action(SduiAction.RequestFullSnapshot(canvas, text wire.reasonCode)))
        | _ -> Error "Unsupported TA transient client frame."

[<RequireQualifiedAccess>]
module TaResearchBrowserWire =
    [<Literal>]
    let MaxFullSnapshotPointsPerSeries = 2000

    [<Literal>]
    let MaxDeltaPointsPerSeries = 200

    // Retained source compatibility; full and delta now have separate budgets.
    [<Literal>]
    let MaxBootstrapPointsPerSeries = MaxFullSnapshotPointsPerSeries

    let revisionFromBrowser fieldName value =
        if Double.IsNaN value || Double.IsInfinity value || value < 0.0 || Math.Truncate value <> value || value > float Int64.MaxValue then
            Error($"Invalid JS-safe {fieldName} revision.")
        else
            Ok(int64 value)

    let text value = if isNull value then "" else value

    let tryNumber = function
        | SduiValue.Number value -> Some value
        | _ -> None

    let tryText = function
        | SduiValue.Text value -> Some(text value)
        | _ -> None

    let tryBool = function
        | SduiValue.Bool value -> Some value
        | _ -> None

    let valueOrZero field values =
        values |> Map.tryFind field |> Option.bind tryNumber |> Option.defaultValue 0.0

    let has field values = Map.containsKey field values

    let valueOrZeroAny fields values =
        fields
        |> List.tryPick (fun field -> values |> Map.tryFind field |> Option.bind tryNumber)
        |> Option.defaultValue 0.0

    let textAny fields values =
        fields
        |> List.tryPick (fun field -> values |> Map.tryFind field |> Option.bind tryText)
        |> Option.defaultValue ""

    let hasAny fields values =
        fields |> List.exists (fun field -> has field values)

    let pointFromValue = function
        | SduiValue.Object values ->
            Some
                { time = textAny [ "t"; "time" ] values
                  openValue = valueOrZeroAny [ "o"; "open" ] values
                  highValue = valueOrZeroAny [ "h"; "high" ] values
                  lowValue = valueOrZeroAny [ "l"; "low" ] values
                  closeValue = valueOrZeroAny [ "c"; "close" ] values
                  volumeValue = valueOrZeroAny [ "v"; "volume" ] values
                  lineValue = valueOrZeroAny [ "v"; "value" ] values
                  hasOpen = hasAny [ "o"; "open" ] values
                  hasHigh = hasAny [ "h"; "high" ] values
                  hasLow = hasAny [ "l"; "low" ] values
                  hasClose = hasAny [ "c"; "close" ] values
                  hasVolume = hasAny [ "v"; "volume" ] values
                  hasLineValue = hasAny [ "v"; "value" ] values }
        | _ -> None

    let seriesPoints (state: RuntimeState) dataRef =
        let points =
            match Map.tryFind dataRef state.Data with
            | Some(SduiValue.Array values) -> values |> Array.choose pointFromValue
            | _ -> [||]

        points

    let boundedTail maximum (values: 'T array) =
        if values.Length <= maximum then values
        else values |> Array.skip (values.Length - maximum)

    let fullSeriesFromState (state: RuntimeState) dataRef =
        { dataRef = dataRef
          mode = "replace"
          removeBeforeTime = ""
          hasRemoveBeforeTime = false
          points = seriesPoints state dataRef |> boundedTail MaxFullSnapshotPointsPerSeries
          pointCount = 0
          startIndex = 0
          timeIndices = [||]
          openValues = [||]
          highValues = [||]
          lowValues = [||]
          closeValues = [||]
          volumeValues = [||]
          lineValues = [||] }

    let deltaSeriesFromState (previous: RuntimeState) (next: RuntimeState) dataRef =
        let previousPoints = seriesPoints previous dataRef
        let nextPoints = seriesPoints next dataRef
        let previousByTime = previousPoints |> Array.map (fun point -> point.time, point) |> Map.ofArray
        let changed =
            nextPoints
            |> Array.filter (fun point ->
                match Map.tryFind point.time previousByTime with
                | Some previousPoint -> previousPoint <> point
                | None -> true)
            |> boundedTail MaxDeltaPointsPerSeries
        let removeBeforeTime = if nextPoints.Length = 0 then "" else nextPoints[0].time
        let hasRemovedPrefix =
            not (String.IsNullOrWhiteSpace removeBeforeTime)
            && previousPoints |> Array.exists (fun point -> String.CompareOrdinal(point.time, removeBeforeTime) < 0)

        { dataRef = dataRef
          mode = "upsert"
          removeBeforeTime = removeBeforeTime
          hasRemoveBeforeTime = hasRemovedPrefix
          points = changed
          pointCount = 0
          startIndex = 0
          timeIndices = [||]
          openValues = [||]
          highValues = [||]
          lowValues = [||]
          closeValues = [||]
          volumeValues = [||]
          lineValues = [||] }

    let columnarSeries (timelineIndex: IDictionary<string, int>) (series: TaBrowserSeriesWire) =
        let points = if isNull series.points then [||] else series.points
        let indices =
            points
            |> Array.map (fun point ->
                match timelineIndex.TryGetValue point.time with
                | true, index -> index
                | _ -> invalidOp $"TA browser timeline does not contain point time `{point.time}`.")
        let contiguous =
            indices.Length = 0
            || indices |> Array.mapi (fun offset index -> index = indices[0] + offset) |> Array.forall id
        let candle = points.Length > 0 && points[0].hasOpen
        let values predicate selector =
            if points.Length > 0 && points |> Array.forall predicate then points |> Array.map selector else [||]

        { series with
            points = [||]
            pointCount = points.Length
            startIndex = if indices.Length = 0 then 0 else indices[0]
            timeIndices = if contiguous then [||] else indices
            openValues = if candle then values _.hasOpen _.openValue else [||]
            highValues = if candle then values _.hasHigh _.highValue else [||]
            lowValues = if candle then values _.hasLow _.lowValue else [||]
            closeValues = if candle then values _.hasClose _.closeValue else [||]
            volumeValues = if candle then values _.hasVolume _.volumeValue else [||]
            lineValues = if candle then [||] else values _.hasLineValue _.lineValue }

    let browserTrace (trace: TaTraceSpec) =
        { traceId = trace.TraceId
          kind = TaResearchTransientWire.traceKindText trace.Kind
          dataRef = trace.DataRef
          label = trace.Label
          color = trace.Color
          width = trace.Width
          visible = trace.Visible }

    let stateToWireAgainst (previous: RuntimeState option) (state: RuntimeState) =
        let document = state.Document
        let rows =
            document
            |> Option.map (fun value ->
                value.Rows
                |> Array.map (fun row ->
                    { rowId = row.RowId
                      kind = TaResearchTransientWire.rowKindText row.Kind
                      dataRef = row.DataRef
                      heightWeight = row.HeightWeight
                      visible = row.Visible
                      traces = if isNull row.Traces then [||] else row.Traces |> Array.map browserTrace }))
            |> Option.defaultValue [||]

        let dataRefs =
            rows
            |> Array.collect (fun row ->
                if isNull row.traces || row.traces.Length = 0 then [| row.dataRef |]
                else row.traces |> Array.map _.dataRef)
            |> Array.filter (String.IsNullOrWhiteSpace >> not)
            |> Array.distinct
        let sendFull =
            match previous with
            | None -> true
            | Some value ->
                let previousHasData = dataRefs |> Array.exists (seriesPoints value >> Array.isEmpty >> not)
                let nextHasData = dataRefs |> Array.exists (seriesPoints state >> Array.isEmpty >> not)

                value.Identity <> state.Identity
                || value.DocumentRevision <> state.DocumentRevision
                || (not previousHasData && nextHasData)
        let series =
            if sendFull then dataRefs |> Array.map (fullSeriesFromState state)
            else
                let value = previous |> Option.get
                dataRefs |> Array.map (deltaSeriesFromState value state)
        let timeline =
            series
            |> Array.collect _.points
            |> Array.map _.time
            |> Array.filter (String.IsNullOrWhiteSpace >> not)
            |> Array.distinct
            |> Array.sort
        let timelineIndex =
            timeline
            |> Array.mapi (fun index timestamp -> timestamp, index)
            |> dict
        let series = series |> Array.map (columnarSeries timelineIndex)
        let defaultView = document |> Option.map _.DefaultView |> Option.defaultValue Map.empty
        let queryText key = defaultView |> Map.tryFind key |> Option.bind tryText |> Option.defaultValue ""
        let queryInterval =
            defaultView
            |> Map.tryFind "query.intervalMinutes"
            |> Option.bind tryNumber
            |> Option.map int
            |> Option.defaultValue 0
        let queryIncludePartial =
            defaultView
            |> Map.tryFind "query.includePartial"
            |> Option.bind tryBool
            |> Option.defaultValue true
        let statusRef = document |> Option.map _.StatusRef |> Option.defaultValue "status"
        let statusValues =
            match Map.tryFind statusRef state.Data with
            | Some(SduiValue.Object values) -> values
            | _ -> Map.empty

        let statusLabel = statusValues |> Map.tryFind "label" |> Option.bind tryText |> Option.defaultValue ""
        let freshness = statusValues |> Map.tryFind "freshness" |> Option.bind tryText |> Option.defaultValue ""
        let watermarkUtc = statusValues |> Map.tryFind "watermarkUtc" |> Option.bind tryText |> Option.defaultValue ""
        let quality = statusValues |> Map.tryFind "quality" |> Option.bind tryText |> Option.defaultValue ""
        let lagSeconds = statusValues |> Map.tryFind "lagSeconds" |> Option.bind tryNumber |> Option.defaultValue 0.0
        let reasonCode = statusValues |> Map.tryFind "reasonCode" |> Option.bind tryText |> Option.defaultValue ""
        let pollKind, _ = TaResearchTransientWire.pollToWire state.Poll
        let (DocumentId documentId) = state.Identity.DocumentId
        let (CanvasInstanceId canvasId) = state.Identity.CanvasInstanceId
        let errorCode, errorMessage, errorRecoverable =
            match state.LastError with
            | Some error -> error.ReasonCode, error.Message, error.Recoverable
            | None -> "", "", false

        { wireVersion = "ta-browser.v3"
          updateKind = if sendFull then "full" else "delta"
          baseDataRevision = previous |> Option.map _.DataRevision |> Option.defaultValue 0L
          documentId = documentId
          canvasInstanceId = canvasId
          workspaceId = document |> Option.map _.WorkspaceId |> Option.defaultValue ""
          title = document |> Option.map _.Title |> Option.defaultValue ""
          rowsRef = document |> Option.map _.RowsRef |> Option.defaultValue "rows"
          statusRef = statusRef
          sharedTimeAxis = document |> Option.map _.SharedTimeAxis |> Option.defaultValue true
          rows = rows
          allowedActions = document |> Option.map _.AllowedActions |> Option.defaultValue [||]
          querySourceId = queryText "query.sourceId"
          queryInstrument = queryText "query.instrument"
          queryIntervalMinutes = queryInterval
          queryFromUtc = queryText "query.fromUtc"
          queryToUtcExclusive = queryText "query.toUtcExclusive"
          queryIncludePartial = queryIncludePartial
          timeline = timeline
          series = series
          statusLabel = statusLabel
          freshness = freshness
          watermarkUtc = watermarkUtc
          quality = quality
          lagSeconds = lagSeconds
          reasonCode = reasonCode
          documentRevision = state.DocumentRevision
          dataRevision = state.DataRevision
          transportSequence = state.LastTransportSequence
          pollKind = pollKind
          errorCode = errorCode
          errorMessage = errorMessage
          errorRecoverable = errorRecoverable }

    let stateToWire (state: RuntimeState) = stateToWireAgainst None state

    let optionalText value =
        if String.IsNullOrWhiteSpace value then None else Some(value.Trim())

    let clientFrameFromWire (wire: TaBrowserClientFrameWire) =
        if isNull (box wire) || not (String.Equals(wire.wireVersion, "ta-browser.v1", StringComparison.Ordinal)) then
            Error "Unsupported TA browser wire version."
        else
            let canvas = CanvasInstanceId(text wire.canvasInstanceId)

            match text wire.kind, text wire.actionKind with
            | "mounted", _ -> Ok(RuntimeClientFrame.Mounted canvas)
            | "unmounted", _ -> Ok(RuntimeClientFrame.Unmounted canvas)
            | "poll-completed", _ ->
                revisionFromBrowser "data" wire.dataRevision
                |> Result.map (fun revision -> RuntimeClientFrame.PollCompleted(canvas, revision))
            | "action", "reset-view" -> Ok(RuntimeClientFrame.Action(SduiAction.ResetView canvas))
            | "action", "reset-canvas" -> Ok(RuntimeClientFrame.Action(SduiAction.ResetCanvas canvas))
            | "action", "add-row" ->
                Ok(
                    RuntimeClientFrame.Action(
                        SduiAction.AddTaRow(
                            canvas,
                             { RowId = text wire.rowId
                               Kind = TaResearchTransientWire.rowKind wire.rowKind
                               DataRef = text wire.dataRef
                               HeightWeight = wire.heightWeight
                               Visible = wire.visible
                               Traces = [||]
                               Options = Map.empty })))
            | "action", "remove-row" -> Ok(RuntimeClientFrame.Action(SduiAction.RemoveTaRow(canvas, text wire.rowId)))
            | "action", "change-query" ->
                Ok(
                    RuntimeClientFrame.Action(
                        SduiAction.ChangeTaQuery(
                            canvas,
                            { SourceId = optionalText wire.sourceId
                              Instrument = optionalText wire.instrument
                              IntervalMinutes = if wire.intervalMinutes > 0 then Some wire.intervalMinutes else None
                              FromUtc = optionalText wire.fromUtc
                              ToUtcExclusive = optionalText wire.toUtcExclusive
                              IncludePartial = Some wire.includePartial })))
            | "action", "poll-delta" ->
                revisionFromBrowser "after-data" wire.afterDataRevision
                |> Result.map (fun revision -> RuntimeClientFrame.Action(SduiAction.PollDelta(canvas, revision)))
            | "action", "full-snapshot" -> Ok(RuntimeClientFrame.Action(SduiAction.RequestFullSnapshot(canvas, text wire.reasonCode)))
            | _ -> Error "Unsupported TA browser client frame."

type TaResearchTransientBackend =
    { HandleAsync: ClientExtensionTransientCommandContext -> RuntimeClientFrame -> Async<Result<RuntimeFrame, string>> }

[<RequireQualifiedAccess>]
module TaResearchTransientServer =
    let pointWireResolver =
        let resolver = DefaultJsonTypeInfoResolver()
        resolver.Modifiers.Add(fun typeInfo ->
            if typeInfo.Type = typeof<TaBrowserPointWire> then
                let predicates =
                    dict [
                        "openValue", fun (point: TaBrowserPointWire) -> point.hasOpen
                        "highValue", fun point -> point.hasHigh
                        "lowValue", fun point -> point.hasLow
                        "closeValue", fun point -> point.hasClose
                        "volumeValue", fun point -> point.hasVolume
                        "lineValue", fun point -> point.hasLineValue
                        "hasOpen", fun point -> point.hasOpen
                        "hasHigh", fun point -> point.hasHigh
                        "hasLow", fun point -> point.hasLow
                        "hasClose", fun point -> point.hasClose
                        "hasVolume", fun point -> point.hasVolume
                        "hasLineValue", fun point -> point.hasLineValue
                    ]

                for property in typeInfo.Properties do
                    match predicates.TryGetValue property.Name with
                    | true, predicate ->
                        property.ShouldSerialize <- Func<obj, obj, bool>(fun owner _ -> predicate (unbox owner))
                    | _ -> ())
        resolver

    let jsonOptions =
        JsonSerializerOptions(
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            TypeInfoResolver = pointWireResolver)

    let createHandler backend : ClientExtensionTransientHandler =
        let gate = obj()
        let states = Dictionary<string, RuntimeState>(StringComparer.Ordinal)

        let stateKey context =
            context.Session.SessionId + "\u001f" + context.ExtensionId + "\u001f" + context.ChannelId

        fun context ->
            async {
                let key = stateKey context

                if context.Operation = "disconnect" then
                    let existing =
                        lock gate (fun () ->
                            match states.TryGetValue key with
                            | true, state ->
                                states.Remove key |> ignore
                                Some state
                            | _ -> None)

                    match existing with
                    | None -> return Ok "{}"
                    | Some state ->
                        let! cleanup =
                            backend.HandleAsync
                                context
                                (RuntimeClientFrame.Unmounted state.Identity.CanvasInstanceId)

                        return cleanup |> Result.map (fun _ -> "{}")
                else
                    try
                        let browserWire = JsonSerializer.Deserialize<TaBrowserClientFrameWire>(context.Payload, jsonOptions)
                        let isBrowserWire =
                            not (isNull (box browserWire))
                            && String.Equals(browserWire.wireVersion, "ta-browser.v1", StringComparison.Ordinal)

                        let decoded =
                            if isBrowserWire then
                                TaResearchBrowserWire.clientFrameFromWire browserWire
                            else
                                let wire = JsonSerializer.Deserialize<TaTransientClientFrameWire>(context.Payload, jsonOptions)

                                if isNull (box wire) then
                                    Error "TA transient client frame decoded to null."
                                else
                                    TaResearchTransientWire.clientFrameFromWire wire

                        match decoded with
                            | Error error -> return Error error
                            | Ok clientFrame ->
                                let! result = backend.HandleAsync context clientFrame

                                match result with
                                | Error error -> return Error error
                                | Ok frame ->
                                    match RuntimeValidation.validateFrame DynamicRuntimeDefaults.limits frame with
                                    | Error errors -> return Error(errors |> List.map _.Message |> String.concat "; ")
                                    | Ok validated ->
                                        let current =
                                            lock gate (fun () ->
                                                match states.TryGetValue key with
                                                | true, value -> value
                                                | _ ->
                                                    RuntimeReducer.initial
                                                        { DocumentId = validated.DocumentId
                                                          CanvasInstanceId = validated.CanvasInstanceId })

                                        let next, _ = RuntimeReducer.reduce current validated
                                        lock gate (fun () -> states[key] <- next)

                                        let fullSnapshotRequested =
                                            match clientFrame with
                                            | RuntimeClientFrame.Action(SduiAction.RequestFullSnapshot _) -> true
                                            | _ -> false

                                        if context.Operation = "close" then
                                            lock gate (fun () -> states.Remove key |> ignore)

                                        let payload =
                                            if isBrowserWire then
                                                (if fullSnapshotRequested then
                                                     TaResearchBrowserWire.stateToWire next
                                                 else
                                                     TaResearchBrowserWire.stateToWireAgainst (Some current) next)
                                                |> fun value -> JsonSerializer.Serialize(value, jsonOptions)
                                            else
                                                next
                                                |> TaResearchTransientWire.stateToWire
                                                |> fun value -> JsonSerializer.Serialize(value, jsonOptions)

                                        return Ok payload
                    with ex ->
                        return Error(ex.GetBaseException().Message)
            }

    let register extensionId backend (hub: CommHub) =
        hub.RegisterClientExtensionTransientHandler(extensionId, createHandler backend)
