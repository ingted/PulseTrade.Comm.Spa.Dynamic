namespace PulseTrade.Comm.Spa.Dynamic.Renderer

open System
open PulseTrade.Comm.Spa.Dynamic.Contracts
open WebSharper

type TaTemporalPointPresentation =
    { SourceIntervalId: string
      ScaleKey: string
      IntervalStartUtc: string
      IntervalEndUtc: string
      ObservedThroughUtc: string
      AvailableAtUtc: string option
      Finality: string
      Projection: string
      Quality: string option }

type TaCandlePoint =
    { Timestamp: string
      Open: float
      High: float
      Low: float
      Close: float
      Volume: float
      Temporal: TaTemporalPointPresentation option }

type TaLinePoint =
    { Timestamp: string
      Value: float
      Temporal: TaTemporalPointPresentation option }

type TaVisibleWindow =
    { StartIndex: int
      Count: int }

[<JavaScript; RequireQualifiedAccess>]
module TaWindowDrag =
    [<Literal>]
    let Move = "move"

    [<Literal>]
    let ResizeLeft = "resize-left"

    [<Literal>]
    let ResizeRight = "resize-right"

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
    [<Literal>]
    let TemporalPointTypeKey = "_type"

    [<Literal>]
    let TemporalPointTypeValue = "temporal-point.v1"

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

    let requiredObjectText name value =
        objectText name value
        |> Option.filter (String.IsNullOrWhiteSpace >> not)

    let tryTemporalPoint value =
        value
        |> tryObject
        |> Option.bind (fun fields ->
            if objectText TemporalPointTypeKey fields <> Some TemporalPointTypeValue then
                None
            else
                match
                    requiredObjectText "sourceIntervalId" fields,
                    requiredObjectText "scaleKey" fields,
                    requiredObjectText "intervalStartUtc" fields,
                    requiredObjectText "intervalEndUtc" fields,
                    requiredObjectText "observedThroughUtc" fields,
                    requiredObjectText "finality" fields,
                    requiredObjectText "projection" fields
                with
                | Some sourceIntervalId, Some scaleKey, Some intervalStartUtc, Some intervalEndUtc, Some observedThroughUtc, Some finality, Some projection ->
                    let metadata =
                        { SourceIntervalId = sourceIntervalId
                          ScaleKey = scaleKey
                          IntervalStartUtc = intervalStartUtc
                          IntervalEndUtc = intervalEndUtc
                          ObservedThroughUtc = observedThroughUtc
                          AvailableAtUtc = requiredObjectText "availableAtUtc" fields
                          Finality = finality
                          Projection = projection
                          Quality = requiredObjectText "quality" fields }
                    let payload =
                        match Map.tryFind "value" fields with
                        | Some SduiValue.Null
                        | None -> None
                        | Some item -> Some item
                    Some(metadata, payload)
                | _ -> None)

    let pointPayload value =
        match tryTemporalPoint value with
        | Some(metadata, payload) -> Some metadata, payload
        | None -> None, Some value

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
        let temporal, payload = pointPayload value
        payload
        |> Option.bind tryObject
        |> Option.bind (fun item ->
            match
                (temporal |> Option.map _.IntervalStartUtc |> Option.orElseWith (fun () -> objectText "t" item)),
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
                      Volume = volume
                      Temporal = temporal }
            | _ -> None)

    let parseLine value =
        let temporal, payload = pointPayload value
        payload
        |> Option.bind tryObject
        |> Option.bind (fun item ->
            match temporal |> Option.map _.IntervalStartUtc |> Option.orElseWith (fun () -> objectText "t" item), objectNumber "v" item with
            | Some timestamp, Some lineValue -> Some { Timestamp = timestamp; Value = lineValue; Temporal = temporal }
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
                 Width = 1.25
                 Visible = true
                 Options = Map.empty } |]

    let rowReferenceLength (row: TaRowSpec) data =
        effectiveTraces row
        |> Array.filter _.Visible
        |> Array.map (fun trace -> seriesValues trace.DataRef data |> Array.length)
        |> Array.sortDescending
        |> Array.tryHead
        |> Option.defaultValue 0

    let traceTimestamps (trace: TaTraceSpec) data =
        match trace.Kind with
        | TaTraceKind.Candlestick
        | TaTraceKind.Volume -> candleSeries trace.DataRef data |> Array.map _.Timestamp
        | TaTraceKind.Line
        | TaTraceKind.Histogram -> lineSeries trace.DataRef data |> Array.map _.Timestamp

    let referenceTimeline (rows: TaRowSpec array) data =
        let traces =
            rows
            |> Array.filter _.Visible
            |> Array.collect effectiveTraces
            |> Array.filter _.Visible

        traces
        |> Array.map (fun trace -> trace, traceTimestamps trace data |> Array.distinct)
        |> Array.filter (fun (_, timestamps) -> timestamps.Length > 0)
        |> Array.sortByDescending (fun (trace, timestamps) -> timestamps.Length, trace.Kind = TaTraceKind.Candlestick)
        |> Array.tryHead
        |> Option.map snd
        |> Option.defaultValue [||]

    let timestampInInterval timestamp (metadata: TaTemporalPointPresentation) =
        compare timestamp metadata.IntervalStartUtc >= 0
        && compare timestamp metadata.IntervalEndUtc < 0

    let availableAtOrAfter timestamp (metadata: TaTemporalPointPresentation) =
        metadata.AvailableAtUtc
        |> Option.exists (fun availableAt -> compare availableAt timestamp <= 0)

    let pointMatchesTimestamp timestamp pointTimestamp temporal =
        match temporal with
        | Some metadata when metadata.Projection = "repeat-across-base-buckets" || metadata.Projection = "candle-span" ->
            timestampInInterval timestamp metadata
        | Some metadata when metadata.Projection = "step-after-close" ->
            availableAtOrAfter timestamp metadata
        | _ -> pointTimestamp = timestamp

    let tryCandleAt timestamp (values: TaCandlePoint array) =
        values
        |> Array.filter (fun value -> pointMatchesTimestamp timestamp value.Timestamp value.Temporal)
        |> Array.tryLast

    let tryLineAt timestamp (values: TaLinePoint array) =
        values
        |> Array.filter (fun value -> pointMatchesTimestamp timestamp value.Timestamp value.Temporal)
        |> Array.tryLast

    let projectedLinePoints (referenceTimestamps: string array) (points: TaLinePoint array) =
        referenceTimestamps
        |> Array.indexed
        |> Array.choose (fun (index, timestamp) ->
            tryLineAt timestamp points
            |> Option.map (fun point -> index, point))

    let candleSlotRange (referenceTimestamps: string array) (point: TaCandlePoint) =
        let matching =
            referenceTimestamps
            |> Array.indexed
            |> Array.choose (fun (index, timestamp) ->
                if pointMatchesTimestamp timestamp point.Timestamp point.Temporal then Some index else None)

        match matching |> Array.tryHead, matching |> Array.tryLast with
        | Some first, Some last -> Some(first, last + 1)
        | _ -> None

    let temporalDetail (metadata: TaTemporalPointPresentation) =
        let availability = metadata.AvailableAtUtc |> Option.defaultValue "unknown"
        let quality = metadata.Quality |> Option.defaultValue "unknown"
        $"{metadata.ScaleKey} | {metadata.Finality} | quality {quality} | frontier {metadata.ObservedThroughUtc} | available {availability}"

    let latestTemporalMetadata (trace: TaTraceSpec) data =
        seriesValues trace.DataRef data
        |> Array.choose (tryTemporalPoint >> Option.map fst)
        |> Array.tryLast

    let rowTemporalMetadata (row: TaRowSpec) data =
        effectiveTraces row
        |> Array.filter _.Visible
        |> Array.choose (fun trace -> latestTemporalMetadata trace data)
        |> Array.distinctBy (fun value -> value.ScaleKey, value.Finality, value.ObservedThroughUtc, value.Quality)

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

    let previewWindow total window candidateStart =
        { window with
            StartIndex = max 0 (min candidateStart (viewportMaximumStart total window)) }

    let commitPreview total window candidateStart =
        let next = previewWindow total window candidateStart
        next.StartIndex = viewportMaximumStart total next, next

    let previewWindowBounds minimumCount maximumCount total committed drag delta =
        let committed = clampWindow minimumCount maximumCount total committed

        if committed.Count <= 0 then
            committed
        else
            let startIndex = committed.StartIndex
            let endExclusive = startIndex + committed.Count

            match drag with
            | TaWindowDrag.Move ->
                { committed with
                    StartIndex = max 0 (min (startIndex + delta) (total - committed.Count)) }
            | TaWindowDrag.ResizeLeft ->
                let maximumStart = endExclusive - min minimumCount committed.Count
                let nextStart = max 0 (min (startIndex + delta) maximumStart)
                clampWindow minimumCount maximumCount total { StartIndex = nextStart; Count = endExclusive - nextStart }
            | _ ->
                let minimumEnd = startIndex + min minimumCount (max 1 total)
                let nextEnd = max minimumEnd (min total (endExclusive + delta))
                clampWindow minimumCount maximumCount total { StartIndex = startIndex; Count = nextEnd - startIndex }

    let commitWindowBounds minimumCount maximumCount total draft =
        let next = clampWindow minimumCount maximumCount total draft
        next.StartIndex = viewportMaximumStart total next, next

    let selectionRatios total window =
        if total <= 0 || window.Count <= 0 then
            0.0, 0.0
        else
            let bounded = clampWindow 1 Int32.MaxValue total window
            float bounded.StartIndex / float total,
            float (bounded.StartIndex + bounded.Count) / float total

    let sampleEvenly maximumCount (values: 'T array) =
        if maximumCount <= 0 || values.Length = 0 then
            [||]
        elif values.Length <= maximumCount then
            Array.copy values
        elif maximumCount = 1 then
            [| values[values.Length - 1] |]
        else
            [| for sampleIndex in 0 .. maximumCount - 1 do
                   let sourceIndex =
                       int (Math.Round(float sampleIndex * float (values.Length - 1) / float (maximumCount - 1)))
                   yield values[sourceIndex] |]

    let cursorIndexFromRatio visibleCount ratio =
        if visibleCount <= 0 then
            None
        else
            let boundedRatio = max 0.0 (min 1.0 ratio)
            Some(min (visibleCount - 1) (int (Math.Floor(boundedRatio * float visibleCount))))

    let slotCenter width visibleCount index =
        if visibleCount <= 0 then
            None
        else
            let boundedIndex = max 0 (min index (visibleCount - 1))
            Some(width / float visibleCount * (float boundedIndex + 0.5))

    let cursorIndexFromClientX visibleCount left width clientX =
        if width <= 0.0 then None
        else cursorIndexFromRatio visibleCount ((clientX - left) / width)

    let selectWindow window (values: 'T array) =
        if window.Count <= 0 || values.Length = 0 then [||]
        else
            let startIndex = max 0 (min window.StartIndex values.Length)
            values |> Array.skip startIndex |> Array.truncate window.Count

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
        let timeline = referenceTimeline visibleRows data
        let referenceLength = timeline.Length
        let effectiveWindow = clampWindow 1 Int32.MaxValue referenceLength window
        let visibleTimestamps = selectWindow effectiveWindow timeline

        if visibleTimestamps.Length = 0 then None
        else
            let index = max 0 (min cursorIndex (visibleTimestamps.Length - 1))
            let timestamp = visibleTimestamps[index]
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
                            |> tryCandleAt timestamp
                            |> Option.map (fun point ->
                                let baseValue =
                                    if trace.Kind = TaTraceKind.Volume then fixedNumber point.Volume
                                    else
                                        "O " + fixedNumber point.Open
                                        + " H " + fixedNumber point.High
                                        + " L " + fixedNumber point.Low
                                        + " C " + fixedNumber point.Close
                                let value =
                                    match point.Temporal with
                                    | Some metadata -> baseValue + " | " + metadata.ScaleKey + " " + metadata.Finality + " | " + metadata.SourceIntervalId
                                    | None -> baseValue
                                point.Timestamp, { Label = label; Value = value })
                        | TaTraceKind.Line
                        | TaTraceKind.Histogram ->
                            lineSeries trace.DataRef data
                            |> tryLineAt timestamp
                            |> Option.map (fun point ->
                                let value =
                                    match point.Temporal with
                                    | Some metadata -> fixedNumber point.Value + " | " + metadata.ScaleKey + " " + metadata.Finality + " | " + metadata.SourceIntervalId
                                    | None -> fixedNumber point.Value
                                point.Timestamp, { Label = label; Value = value })))

            Some
                { VisibleIndex = index
                  Timestamp = timestamp
                  Values = values |> Array.map snd }

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

    let rec fallbackInputs path kind =
        match kind with
        | EditorValueKind.Text -> [| { Path = path; Value = EditorScalarValue.Text "" } |]
        | EditorValueKind.Integer(minimum, _) -> [| { Path = path; Value = EditorScalarValue.Number(float (defaultArg minimum 0L)) } |]
        | EditorValueKind.Decimal(minimum, _) -> [| { Path = path; Value = EditorScalarValue.Number(defaultArg minimum 0.0) } |]
        | EditorValueKind.Boolean -> [| { Path = path; Value = EditorScalarValue.Bool false } |]
        | EditorValueKind.Choice choices ->
            choices
            |> Array.tryHead
            |> Option.bind (fun choice ->
                match choice.Value with
                | SduiValue.Text value -> Some(EditorScalarValue.Text value)
                | SduiValue.Number value -> Some(EditorScalarValue.Number value)
                | SduiValue.Bool value -> Some(EditorScalarValue.Bool value)
                | _ -> None)
            |> Option.map (fun value -> [| { Path = path; Value = value } |])
            |> Option.defaultValue [||]
        | EditorValueKind.Scale scaleKeys ->
            scaleKeys
            |> Array.tryHead
            |> Option.map (fun value -> [| { Path = path; Value = EditorScalarValue.Text value } |])
            |> Option.defaultValue [||]
        | EditorValueKind.List(itemKind, minimum, _) ->
            Array.init (defaultArg minimum 0) (fun index -> fallbackInputs ($"{path}[{index}]") itemKind)
            |> Array.concat
        | EditorValueKind.Group fields ->
            fields
            |> Array.collect (fun field ->
                let childPath = $"{path}.{field.Key}"
                match field.DefaultValue with
                | Some value -> flattenEditorValue childPath field.Kind value
                | None -> fallbackInputs childPath field.Kind)

    and flattenEditorValue path kind value =
        match kind, value with
        | EditorValueKind.Group fields, SduiValue.Object values ->
            fields
            |> Array.collect (fun field ->
                match Map.tryFind field.Key values with
                | Some child -> flattenEditorValue ($"{path}.{field.Key}") field.Kind child
                | None -> [||])
        | EditorValueKind.List(itemKind, _, _), SduiValue.Array values ->
            values
            |> Array.indexed
            |> Array.collect (fun (index, item) -> flattenEditorValue ($"{path}[{index}]") itemKind item)
        | _, SduiValue.Text value -> [| { Path = path; Value = EditorScalarValue.Text value } |]
        | _, SduiValue.Number value -> [| { Path = path; Value = EditorScalarValue.Number value } |]
        | _, SduiValue.Bool value -> [| { Path = path; Value = EditorScalarValue.Bool value } |]
        | _ -> [||]

    let initialEditorInputs (schema: DynamicTemplateSchema) =
        schema.Fields
        |> Array.collect (fun field ->
            match field.DefaultValue with
            | Some value -> flattenEditorValue field.Key field.Kind value
            | None -> fallbackInputs field.Key field.Kind)

    let setEditorInput input values =
        values
        |> Array.filter (fun current -> current.Path <> input.Path)
        |> Array.append [| input |]
        |> Array.sortBy _.Path

    let tryEditorInput path values =
        values |> Array.tryFind (fun value -> value.Path = path) |> Option.map _.Value

    let tryListIndex listPath (path: string) =
        let prefix = listPath + "["
        if isNull path || not (path.StartsWith(prefix)) then None
        else
            let closeIndex = path.IndexOf(']', prefix.Length)
            if closeIndex < prefix.Length then None
            else
                match Int32.TryParse(path.Substring(prefix.Length, closeIndex - prefix.Length)) with
                | true, index when index >= 0 -> Some index
                | _ -> None

    let listIndexes listPath values =
        values
        |> Array.choose (fun value -> tryListIndex listPath value.Path)
        |> Array.distinct
        |> Array.sort

    let replaceListIndex listPath oldIndex newIndex (path: string) =
        let oldPrefix = $"{listPath}[{oldIndex}]"
        if path.StartsWith(oldPrefix) then
            $"{listPath}[{newIndex}]" + path.Substring(oldPrefix.Length)
        else
            path

    let addListItem listPath itemKind values =
        let nextIndex =
            match listIndexes listPath values |> Array.tryLast with
            | Some index -> index + 1
            | None -> 0
        Array.append values (fallbackInputs ($"{listPath}[{nextIndex}]") itemKind)
        |> Array.sortBy _.Path

    let removeListItem listPath index values =
        values
        |> Array.choose (fun value ->
            match tryListIndex listPath value.Path with
            | Some current when current = index -> None
            | Some current when current > index ->
                Some { value with Path = replaceListIndex listPath current (current - 1) value.Path }
            | _ -> Some value)
        |> Array.sortBy _.Path

    let moveListItem listPath fromIndex toIndex values =
        values
        |> Array.map (fun value ->
            match tryListIndex listPath value.Path with
            | Some current when current = fromIndex -> { value with Path = replaceListIndex listPath fromIndex toIndex value.Path }
            | Some current when current = toIndex -> { value with Path = replaceListIndex listPath toIndex fromIndex value.Path }
            | _ -> value)
        |> Array.sortBy _.Path

    let editorScalarText = function
        | EditorScalarValue.Text value -> value
        | EditorScalarValue.Number value -> fixedNumber value
        | EditorScalarValue.Bool value -> if value then "true" else "false"

    let editorScalarEqualsSdui scalar value =
        match scalar, value with
        | EditorScalarValue.Text left, SduiValue.Text right -> left = right
        | EditorScalarValue.Number left, SduiValue.Number right -> left = right
        | EditorScalarValue.Bool left, SduiValue.Bool right -> left = right
        | _ -> false

    let rec editorSubmissionErrors path required kind values =
        let missing () =
            if required then [| path + " is required." |] else [||]

        match kind with
        | EditorValueKind.Group fields ->
            fields
            |> Array.collect (fun field -> editorSubmissionErrors ($"{path}.{field.Key}") field.Required field.Kind values)
        | EditorValueKind.List(itemKind, minimum, maximum) ->
            let indexes = listIndexes path values
            [| match minimum with
               | Some count when indexes.Length < count -> yield $"{path} requires at least {count} item(s)."
               | _ -> ()
               match maximum with
               | Some count when indexes.Length > count -> yield $"{path} allows at most {count} item(s)."
               | _ -> ()
               for index in indexes do
                   yield! editorSubmissionErrors ($"{path}[{index}]") true itemKind values |]
        | _ ->
            match tryEditorInput path values with
            | None -> missing ()
            | Some scalar ->
                match kind, scalar with
                | EditorValueKind.Text, EditorScalarValue.Text value when required && String.IsNullOrWhiteSpace value -> missing ()
                | EditorValueKind.Text, EditorScalarValue.Text _
                | EditorValueKind.Boolean, EditorScalarValue.Bool _ -> [||]
                | EditorValueKind.Integer(minimum, maximum), EditorScalarValue.Number value ->
                    [| if Double.IsNaN value || Double.IsInfinity value || Math.Truncate value <> value then yield path + " must be an integer."
                       match minimum with Some lower when value < float lower -> yield path + " is below its minimum." | _ -> ()
                       match maximum with Some upper when value > float upper -> yield path + " exceeds its maximum." | _ -> () |]
                | EditorValueKind.Decimal(minimum, maximum), EditorScalarValue.Number value ->
                    [| if Double.IsNaN value || Double.IsInfinity value then yield path + " must be finite."
                       match minimum with Some lower when value < lower -> yield path + " is below its minimum." | _ -> ()
                       match maximum with Some upper when value > upper -> yield path + " exceeds its maximum." | _ -> () |]
                | EditorValueKind.Choice choices, _ when choices |> Array.exists (fun choice -> editorScalarEqualsSdui scalar choice.Value) -> [||]
                | EditorValueKind.Scale scaleKeys, EditorScalarValue.Text value when Array.contains value scaleKeys -> [||]
                | _ -> [| path + " does not match its editor kind." |]

    let validateEditorSubmission (schema: DynamicTemplateSchema) values =
        schema.Fields
        |> Array.collect (fun field -> editorSubmissionErrors field.Key field.Required field.Kind values)
