namespace PulseTrade.Comm.Spa.Dynamic

open System
open System.Collections.Generic
open System.Text.Json
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
type TaTransientRowWire =
    { rowId: string
      kind: string
      dataRef: string
      heightWeight: float
      visible: bool
      options: TaTransientFieldWire array }

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
        match text value with
        | "volume" -> TaRowKind.Volume
        | "sma" -> TaRowKind.Sma
        | "dmi" -> TaRowKind.Dmi
        | "adx" -> TaRowKind.Adx
        | "macd" -> TaRowKind.Macd
        | "heikin-ashi" -> TaRowKind.HeikinAshi
        | _ -> TaRowKind.Candlestick

    let rowToWire (row: TaRowSpec) : TaTransientRowWire =
        { rowId = row.RowId
          kind = rowKindText row.Kind
          dataRef = row.DataRef
          heightWeight = row.HeightWeight
          visible = row.Visible
          options = mapToWire row.Options }

    let rowFromWire (wire: TaTransientRowWire) : TaRowSpec =
        { RowId = text wire.rowId
          Kind = rowKind wire.kind
          DataRef = text wire.dataRef
          HeightWeight = wire.heightWeight
          Visible = wire.visible
          Options = mapFromWire wire.options }

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
        { rowId = ""; kind = ""; dataRef = ""; heightWeight = 0.0; visible = false; options = [||] }

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

type TaResearchTransientBackend =
    { HandleAsync: ClientExtensionTransientCommandContext -> RuntimeClientFrame -> Async<Result<RuntimeFrame, string>> }

[<RequireQualifiedAccess>]
module TaResearchTransientServer =
    let jsonOptions = JsonSerializerOptions(PropertyNamingPolicy = JsonNamingPolicy.CamelCase)

    let createHandler backend : ClientExtensionTransientHandler =
        let gate = obj()
        let states = Dictionary<string, RuntimeState>(StringComparer.Ordinal)

        let stateKey context =
            context.Session.SessionId + "\u001f" + context.ExtensionId + "\u001f" + context.ChannelId

        fun context ->
            async {
                let key = stateKey context

                if context.Operation = "disconnect" then
                    lock gate (fun () -> states.Remove key |> ignore)
                    return Ok "{}"
                else
                    try
                        let wire = JsonSerializer.Deserialize<TaTransientClientFrameWire>(context.Payload, jsonOptions)

                        if isNull (box wire) then
                            return Error "TA transient client frame decoded to null."
                        else
                            match TaResearchTransientWire.clientFrameFromWire wire with
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

                                        if context.Operation = "close" then
                                            lock gate (fun () -> states.Remove key |> ignore)

                                        return
                                            next
                                            |> TaResearchTransientWire.stateToWire
                                            |> fun value -> JsonSerializer.Serialize(value, jsonOptions)
                                            |> Ok
                    with ex ->
                        return Error(ex.GetBaseException().Message)
            }

    let register extensionId backend (hub: CommHub) =
        hub.RegisterClientExtensionTransientHandler(extensionId, createHandler backend)
