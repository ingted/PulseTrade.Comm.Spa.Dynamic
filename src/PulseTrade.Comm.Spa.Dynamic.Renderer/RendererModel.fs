namespace PulseTrade.Comm.Spa.Dynamic.Renderer

open System
open PulseTrade.Comm.Spa.Dynamic.Contracts
open WebSharper

type TaCandlePoint =
    { Timestamp: string
      Open: float
      High: float
      Low: float
      Close: float
      Volume: float }

type TaLinePoint =
    { Timestamp: string
      Value: float }

type TaVisibleWindow =
    { StartIndex: int
      Count: int }

type TaCursorValue =
    { Label: string
      Value: string }

type TaCursorSnapshot =
    { VisibleIndex: int
      Timestamp: string
      Values: TaCursorValue array }

type TaStatusPresentation =
    { Freshness: TaFreshness
      Label: string
      Watermark: string option
      Quality: string option
      Error: string option }

type TaQueryDraft =
    { SourceId: string
      Instrument: string
      IntervalMinutes: string
      FromUtc: string
      ToUtcExclusive: string
      IncludePartial: bool }

type TaWorkspaceBootstrapPresentation =
    { State: string
      Title: string
      Detail: string
      IsError: bool }

[<JavaScript; RequireQualifiedAccess>]
module RendererModel =
    let workspaceBootstrapPresentation (state: RuntimeState) =
        match state.LastError with
        | Some error when not error.Recoverable ->
            { State = "unavailable"
              Title = "TA workspace unavailable"
              Detail = error.ReasonCode + ": " + error.Message
              IsError = true }
        | Some error ->
            { State = "recovering"
              Title = "Restoring TA workspace"
              Detail = error.ReasonCode + ": " + error.Message
              IsError = false }
        | None ->
            match state.Poll with
            | RuntimePollState.Unmounted ->
                { State = "preparing"
                  Title = "Preparing TA workspace"
                  Detail = "Waiting for the workspace channel to mount."
                  IsError = false }
            | RuntimePollState.MountedIdle ->
                { State = "connecting"
                  Title = "Connecting TA workspace"
                  Detail = "Waiting for the initial workspace document."
                  IsError = false }
            | RuntimePollState.Backoff _ ->
                { State = "retrying"
                  Title = "Restoring TA workspace"
                  Detail = "A reconnect attempt is scheduled."
                  IsError = false }
            | RuntimePollState.PausedForResync ->
                { State = "resyncing"
                  Title = "Resynchronizing TA workspace"
                  Detail = "Requesting a full workspace document."
                  IsError = false }
            | RuntimePollState.Disposed ->
                { State = "closed"
                  Title = "TA workspace closed"
                  Detail = "Open the page again to reconnect."
                  IsError = false }
            | RuntimePollState.Ready
            | RuntimePollState.PollInFlight
            | RuntimePollState.Suspended ->
                { State = "loading"
                  Title = "Loading TA workspace"
                  Detail = "Waiting for the workspace document."
                  IsError = false }

    let tryObject = function
        | SduiValue.Object value -> Some value
        | _ -> None

    let tryText = function
        | SduiValue.Text value -> Some value
        | _ -> None

    let tryNumber = function
        | SduiValue.Number value -> Some value
        | _ -> None

    let tryBool = function
        | SduiValue.Bool value -> Some value
        | _ -> None

    let objectField name value =
        value |> Map.tryFind name

    let objectText name value =
        objectField name value |> Option.bind tryText

    let objectNumber name value =
        objectField name value |> Option.bind tryNumber

    let queryDraft (values: Map<string, SduiValue>) =
        let textValue name =
            values
            |> Map.tryFind name
            |> Option.bind tryText
            |> Option.defaultValue ""

        let interval =
            values
            |> Map.tryFind "query.intervalMinutes"
            |> Option.bind tryNumber
            |> Option.map (int >> string)
            |> Option.defaultValue ""

        let includePartial =
            values
            |> Map.tryFind "query.includePartial"
            |> Option.bind tryBool
            |> Option.defaultValue true

        { SourceId = textValue "query.sourceId"
          Instrument = textValue "query.instrument"
          IntervalMinutes = interval
          FromUtc = textValue "query.fromUtc"
          ToUtcExclusive = textValue "query.toUtcExclusive"
          IncludePartial = includePartial }

    let fixedNumber (value: float) =
        string value

    let parseCandle value =
        value
        |> tryObject
        |> Option.bind (fun item ->
            match
                objectText "t" item,
                objectNumber "o" item,
                objectNumber "h" item,
                objectNumber "l" item,
                objectNumber "c" item,
                objectNumber "v" item
            with
            | Some timestamp, Some openValue, Some high, Some low, Some close, Some volume ->
                Some
                    { Timestamp = timestamp
                      Open = openValue
                      High = high
                      Low = low
                      Close = close
                      Volume = volume }
            | _ -> None)

    let parseLine value =
        value
        |> tryObject
        |> Option.bind (fun item ->
            match objectText "t" item, objectNumber "v" item with
            | Some timestamp, Some lineValue -> Some { Timestamp = timestamp; Value = lineValue }
            | _ -> None)

    let seriesValues dataRef data =
        match Map.tryFind dataRef data with
        | Some(SduiValue.Array values) -> values
        | _ -> [||]

    let candleSeries dataRef data =
        seriesValues dataRef data |> Array.choose parseCandle

    let lineSeries dataRef data =
        seriesValues dataRef data |> Array.choose parseLine

    let effectiveTraces (row: TaRowSpec) =
        if not (isNull row.Traces) && row.Traces.Length > 0 then
            row.Traces
        else
            let kind =
                match row.Kind with
                | TaRowKind.Candlestick
                | TaRowKind.HeikinAshi -> TaTraceKind.Candlestick
                | TaRowKind.Volume -> TaTraceKind.Volume
                | _ -> TaTraceKind.Line

            [| { TraceId = row.RowId
                 Kind = kind
                 DataRef = row.DataRef
                 Label = row.RowId
                 Color = ""
                 Width = 2.0
                 Visible = true
                 Options = Map.empty } |]

    let rowReferenceLength (row: TaRowSpec) data =
        effectiveTraces row
        |> Array.filter _.Visible
        |> Array.map (fun trace -> seriesValues trace.DataRef data |> Array.length)
        |> Array.tryHead
        |> Option.defaultValue 0

    let clampWindow minimumCount maximumCount total requested =
        if total <= 0 then
            { StartIndex = 0; Count = 0 }
        else
            let upper = min maximumCount total
            let lower = min minimumCount upper
            let count = max lower (min requested.Count upper)
            let startIndex = max 0 (min requested.StartIndex (total - count))
            { StartIndex = startIndex; Count = count }

    let resolveWindow minimumCount maximumCount total followLatest requested =
        let bounded = clampWindow minimumCount maximumCount total requested

        if followLatest && bounded.Count > 0 then
            { bounded with StartIndex = max 0 (total - bounded.Count) }
        else
            bounded

    let viewportMaximumStart total window =
        max 0 (total - max 0 window.Count)

    let cursorIndexFromRatio visibleCount ratio =
        if visibleCount <= 0 then
            None
        else
            let boundedRatio = max 0.0 (min 1.0 ratio)
            let maximumIndex = visibleCount - 1
            Some(int (Math.Round(boundedRatio * float maximumIndex)))

    let cursorIndexFromClientX visibleCount left width clientX =
        if width <= 0.0 then None
        else cursorIndexFromRatio visibleCount ((clientX - left) / width)

    let selectWindow window (values: 'T array) =
        if window.Count <= 0 || values.Length = 0 then [||]
        else values |> Array.skip window.StartIndex |> Array.truncate window.Count

    let paddedRange fallbackLow fallbackHigh values =
        if Array.isEmpty values then fallbackLow, fallbackHigh
        else
            let low = Array.min values
            let high = Array.max values

            if low = high then low - 1.0, high + 1.0
            else
                let padding = max ((high - low) * 0.08) 0.0001
                low - padding, high + padding

    let normalize low high top height value =
        if low = high then top + height / 2.0
        else top + height - ((value - low) / (high - low)) * height

    let timeLabels (timestamps: string array) =
        if timestamps.Length = 0 then [||]
        elif timestamps.Length = 1 then [| 0, timestamps[0] |]
        else
            [| 0; timestamps.Length / 2; timestamps.Length - 1 |]
            |> Array.distinct
            |> Array.map (fun index -> index, timestamps[index])

    let cursorSnapshot (document: TaWorkspaceDocument) data window cursorIndex =
        let visibleRows = document.Rows |> Array.filter _.Visible
        let referenceLength =
            visibleRows
            |> Array.tryHead
            |> Option.map (fun row -> rowReferenceLength row data)
            |> Option.defaultValue 0
        let effectiveWindow = clampWindow 1 Int32.MaxValue referenceLength window

        if effectiveWindow.Count = 0 then None
        else
            let index = max 0 (min cursorIndex (effectiveWindow.Count - 1))
            let values =
                visibleRows
                |> Array.collect (fun row ->
                    effectiveTraces row
                    |> Array.filter _.Visible
                    |> Array.choose (fun trace ->
                        let label = if String.IsNullOrWhiteSpace trace.Label then trace.TraceId else trace.Label
                        match trace.Kind with
                        | TaTraceKind.Candlestick
                        | TaTraceKind.Volume ->
                            candleSeries trace.DataRef data
                            |> selectWindow effectiveWindow
                            |> Array.tryItem index
                            |> Option.map (fun point ->
                                let value =
                                    if trace.Kind = TaTraceKind.Volume then fixedNumber point.Volume
                                    else
                                        "O " + fixedNumber point.Open
                                        + " H " + fixedNumber point.High
                                        + " L " + fixedNumber point.Low
                                        + " C " + fixedNumber point.Close
                                point.Timestamp, { Label = label; Value = value })
                        | TaTraceKind.Line
                        | TaTraceKind.Histogram ->
                            lineSeries trace.DataRef data
                            |> selectWindow effectiveWindow
                            |> Array.tryItem index
                            |> Option.map (fun point -> point.Timestamp, { Label = label; Value = fixedNumber point.Value })))

            values
            |> Array.tryHead
            |> Option.map (fun (timestamp, _) ->
                { VisibleIndex = index
                  Timestamp = timestamp
                  Values = values |> Array.map snd })

    let freshnessFromStatus status =
        let kind = objectText "freshness" status |> Option.defaultValue "unavailable" |> fun value -> value.ToLower()
        let lag = objectNumber "lagSeconds" status |> Option.defaultValue 0.0 |> TimeSpan.FromSeconds
        let reason = objectText "reasonCode" status |> Option.defaultValue kind

        match kind with
        | "live" -> TaFreshness.Live
        | "delayed" -> TaFreshness.Delayed lag
        | "stale" -> TaFreshness.Stale(lag, reason)
        | "backfill" -> TaFreshness.Backfill reason
        | _ -> TaFreshness.Unavailable reason

    let statusPresentation statusRef (state: RuntimeState) =
        let status =
            state.Data
            |> Map.tryFind statusRef
            |> Option.bind tryObject
            |> Option.defaultValue Map.empty
        let freshness = freshnessFromStatus status
        let label = objectText "label" status |> Option.defaultValue (string freshness)

        { Freshness = freshness
          Label = label
          Watermark = objectText "watermarkUtc" status
          Quality = objectText "quality" status
          Error = state.LastError |> Option.map (fun error -> error.ReasonCode + ": " + error.Message) }
