namespace PulseTrade.Comm.Spa.Dynamic.Renderer

open System
open System.Collections.Generic
open PulseTrade.Comm.Spa.Dynamic.Contracts
open WebSharper

type TaTemporalPointPresentation =
    { SourceIntervalId: string
      ScaleKey: string
      IntervalStartUtc: string
      IntervalEndUtc: string
      EventTimeUtc: string option
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

type TaMarkerPlacement =
    { TraceId: string
      TargetTraceId: string
      Position: float
      SlotIndex: int
      Lane: int
      Target: TaCandlePoint
      Marker: TaMarker }

type TaMarkerOverflowCluster =
    { ClusterId: string
      TargetTraceId: string
      Position: float
      SlotIndex: int
      Anchor: TaMarkerAnchor
      Lane: int
      Markers: TaMarkerPlacement array }

type TaOverviewStripePlacement =
    { TraceId: string
      TargetTraceId: string
      CollisionGroup: string
      LayerOrder: int
      Position: float
      SlotIndex: int
      Stripe: TaOverviewStripe }

type TaOverviewStripeVisual =
    { TraceId: string
      TargetTraceId: string
      CollisionGroup: string
      LayerOrder: int
      Lane: int
      SlotIndex: int
      Stripes: TaOverviewStripe array }

type TaMarkerLabelGeometry =
    { Text: string
      X: float
      Y: float
      TextAnchor: string
      EstimatedWidth: float }

type TaRowValuePresentation =
    { Timestamp: string
      Value: string }

type TaResolvedSeriesPoint =
    { Payload: SduiValue option
      Temporal: TaTemporalPointPresentation option }

type TaPreparedTemporalAxis =
    { Revision: float
      RawPoints: SduiValue array
      Points: Map<float, TaTemporalPointPresentation> }

type TaPreparedRendererData =
    { RawData: Map<string, SduiValue>
      ResolvedAxes: Map<string, TaPreparedTemporalAxis>
      ResolvedSeries: Map<string, TaResolvedSeriesPoint array> }

type TaVisibleWindow =
    { StartIndex: int
      Count: int }

type TaRowHeightBounds =
    { Minimum: int
      Maximum: int
      DefaultHeight: int }

[<RequireQualifiedAccess>]
type TaCoverageDirection =
    | Earlier
    | Later

[<RequireQualifiedAccess>]
type TaQueryViewportSelection =
    | NotRequested
    | Selected of TaVisibleWindow
    | NoIntersection
    | Invalid of string
    | Stale

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

type TaVisibleEventRange =
    { BaseRowId: string
      StartEventTimeUtc: string
      EndEventTimeExclusiveUtc: string }

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

    [<Literal>]
    let TemporalAxisTypeValue = "temporal-axis.v1"

    [<Literal>]
    let TemporalSeriesTypeValue = "temporal-series.v1"

    let isOverlayTraceKind = function
        | TaTraceKind.Marker
        | TaTraceKind.OverviewStripe -> true
        | _ -> false

    let clamp minimum maximum value = max minimum (min maximum value)

    let rowHeightBounds (row: TaRowSpec) (traces: TaTraceSpec array) =
        let hasCandles = traces |> Array.exists (fun trace -> trace.Kind = TaTraceKind.Candlestick)
        if hasCandles then
            { Minimum = 180
              Maximum = 720
              DefaultHeight = clamp 180 720 (int (Math.Round(250.0 * row.HeightWeight))) }
        else
            { Minimum = 96
              Maximum = 480
              DefaultHeight = clamp 96 480 (int (Math.Round(112.0 * row.HeightWeight))) }

    let rowHeightStorageKey (CanvasInstanceId canvasInstanceId) rowId = canvasInstanceId + ":" + rowId

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
                          EventTimeUtc = None
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

    let nonNegativeInteger name fields =
        objectNumber name fields
        |> Option.filter (fun value -> value >= 0.0 && value = Math.Truncate value)

    let tryTemporalAxisPoint value =
        value
        |> tryObject
        |> Option.bind (fun fields ->
            match
                nonNegativeInteger "position" fields,
                requiredObjectText "sourceIntervalId" fields,
                requiredObjectText "scaleKey" fields,
                requiredObjectText "intervalStartUtc" fields,
                requiredObjectText "intervalEndUtc" fields,
                requiredObjectText "observedThroughUtc" fields,
                requiredObjectText "finality" fields,
                requiredObjectText "projection" fields
            with
            | Some position, Some sourceIntervalId, Some scaleKey, Some intervalStartUtc, Some intervalEndUtc, Some observedThroughUtc, Some finality, Some projection ->
                Some(
                    position,
                    { SourceIntervalId = sourceIntervalId
                      ScaleKey = scaleKey
                      IntervalStartUtc = intervalStartUtc
                      IntervalEndUtc = intervalEndUtc
                      EventTimeUtc = requiredObjectText "eventTimeUtc" fields
                      ObservedThroughUtc = observedThroughUtc
                      AvailableAtUtc = requiredObjectText "availableAtUtc" fields
                      Finality = finality
                      Projection = projection
                      Quality = requiredObjectText "quality" fields })
            | _ -> None)

    let tryTemporalAxisRaw value =
        value
        |> tryObject
        |> Option.bind (fun fields ->
            if objectText TemporalPointTypeKey fields <> Some TemporalAxisTypeValue then None
            else
                match requiredObjectText "axisRef" fields, nonNegativeInteger "revision" fields, Map.tryFind "points" fields with
                | Some axisRef, Some revision, Some(SduiValue.Array points) ->
                    Some(axisRef, revision, points)
                | _ -> None)

    let tryTemporalAxis value =
        tryTemporalAxisRaw value
        |> Option.bind (fun (axisRef, revision, points) ->
            let decoded = points |> Array.choose tryTemporalAxisPoint
            if decoded.Length <> points.Length then None
            else Some(axisRef, revision, decoded |> Map.ofArray))

    let tryTemporalSeriesPoint value =
        value
        |> tryObject
        |> Option.bind (fun values ->
            match nonNegativeInteger "position" values, Map.tryFind "value" values with
            | Some position, Some payload -> Some(position, payload)
            | _ -> None)

    let tryTemporalSeriesRaw value =
        value
        |> tryObject
        |> Option.bind (fun fields ->
            if objectText TemporalPointTypeKey fields <> Some TemporalSeriesTypeValue then None
            else
                match requiredObjectText "axisRef" fields, nonNegativeInteger "axisRevision" fields, Map.tryFind "points" fields with
                | Some axisRef, Some revision, Some(SduiValue.Array points) ->
                    Some(axisRef, revision, points)
                | _ -> None)

    let tryTemporalSeries value =
        tryTemporalSeriesRaw value
        |> Option.bind (fun (axisRef, revision, points) ->
            let decoded = points |> Array.choose tryTemporalSeriesPoint
            if decoded.Length <> points.Length then None
            else Some(axisRef, revision, decoded))

    [<Inline "$left === $right">]
    let sameReference (left: obj) (right: obj) = Object.ReferenceEquals(left, right)

    let sharedPrefixLength (left: SduiValue array) (right: SduiValue array) =
        let limit = min left.Length right.Length
        let mutable index = 0
        while index < limit && sameReference (box left[index]) (box right[index]) do
            index <- index + 1
        index

    let resolveTemporalPoints axisPoints rawPoints =
        rawPoints
        |> Array.choose (fun point ->
            tryTemporalSeriesPoint point
            |> Option.bind (fun (position, payload) ->
                Map.tryFind position axisPoints
                |> Option.map (fun temporal -> { Payload = Some payload; Temporal = Some temporal })))

    let prepareAxis value =
        tryTemporalAxisRaw value
        |> Option.bind (fun (axisRef, revision, rawPoints) ->
            let decoded = rawPoints |> Array.choose tryTemporalAxisPoint
            if decoded.Length <> rawPoints.Length then None
            else
                Some(
                    axisRef,
                    { Revision = revision
                      RawPoints = rawPoints
                      Points = decoded |> Map.ofArray }))

    let updateAxis previous value =
        match tryTemporalAxisRaw value with
        | None -> None
        | Some(axisRef, revision, rawPoints) ->
            match Map.tryFind axisRef previous.ResolvedAxes with
            | Some oldAxis when sameReference (box oldAxis.RawPoints) (box rawPoints) ->
                Some(
                    axisRef,
                    { oldAxis with Revision = revision },
                    Some Set.empty)
            | Some oldAxis ->
                let prefix = sharedPrefixLength oldAxis.RawPoints rawPoints
                let incrementalShape =
                    prefix = min oldAxis.RawPoints.Length rawPoints.Length
                    || oldAxis.RawPoints.Length = rawPoints.Length
                if incrementalShape then
                    let oldSuffix = oldAxis.RawPoints |> Array.skip prefix |> Array.choose tryTemporalAxisPoint
                    let newSuffix = rawPoints |> Array.skip prefix |> Array.choose tryTemporalAxisPoint
                    if oldSuffix.Length = oldAxis.RawPoints.Length - prefix
                       && newSuffix.Length = rawPoints.Length - prefix then
                        let changedPositions =
                            Array.append (oldSuffix |> Array.map fst) (newSuffix |> Array.map fst)
                            |> Set.ofArray
                        let points =
                            oldSuffix
                            |> Array.fold (fun state (position, _) -> Map.remove position state) oldAxis.Points
                            |> fun state -> newSuffix |> Array.fold (fun current (position, metadata) -> Map.add position metadata current) state
                        Some(
                            axisRef,
                            { Revision = revision
                              RawPoints = rawPoints
                              Points = points },
                            Some changedPositions)
                    else
                        prepareAxis value |> Option.map (fun (key, axis) -> key, axis, None)
                else
                    prepareAxis value |> Option.map (fun (key, axis) -> key, axis, None)
            | None -> prepareAxis value |> Option.map (fun (key, axis) -> key, axis, None)

    let tryFindTemporalPointIndex (position: float) (rawPoints: SduiValue array) =
        let rec search low high =
            if low > high then None
            else
                let middle = low + ((high - low) / 2)
                match tryTemporalSeriesPoint rawPoints.[middle] with
                | Some(candidate, _) when candidate = position -> Some middle
                | Some(candidate, _) when candidate < position -> search (middle + 1) high
                | Some _ -> search low (middle - 1)
                | None -> None
        search 0 (rawPoints.Length - 1)

    let updateTemporalSeries
        (previousRaw: SduiValue array)
        (previousResolved: TaResolvedSeriesPoint array)
        (rawPoints: SduiValue array)
        (axisPoints: Map<float, TaTemporalPointPresentation>)
        (changedAxisPositions: Set<float> option) =
        let full () = resolveTemporalPoints axisPoints rawPoints
        let baseResolved =
            if previousResolved.Length <> previousRaw.Length then full ()
            elif sameReference (box previousRaw) (box rawPoints) then previousResolved
            else
                let prefix = sharedPrefixLength previousRaw rawPoints
                let incrementalShape = prefix = min previousRaw.Length rawPoints.Length || previousRaw.Length = rawPoints.Length
                if not incrementalShape then full ()
                else
                    let suffix = rawPoints |> Array.skip prefix |> resolveTemporalPoints axisPoints
                    if suffix.Length <> rawPoints.Length - prefix then full ()
                    else Array.append (previousResolved |> Array.take prefix) suffix

        match changedAxisPositions with
        | Some positions when not positions.IsEmpty && baseResolved.Length = rawPoints.Length ->
            let next = Array.copy baseResolved
            for position in positions do
                match tryFindTemporalPointIndex position rawPoints with
                | Some index ->
                    match tryTemporalSeriesPoint rawPoints.[index], Map.tryFind position axisPoints with
                    | Some(_, payload), Some temporal ->
                        next[index] <- { Payload = Some payload; Temporal = Some temporal }
                    | _ -> ()
                | None -> ()
            next
        | Some _ -> baseResolved
        | None -> full ()

    let updateInlineSeries
        (previousRaw: SduiValue array)
        (previousResolved: TaResolvedSeriesPoint array)
        (rawPoints: SduiValue array) =
        let mapPoint item =
            let temporal, payload = pointPayload item
            { Payload = payload; Temporal = temporal }
        if sameReference (box previousRaw) (box rawPoints) then previousResolved
        elif previousResolved.Length <> previousRaw.Length then rawPoints |> Array.map mapPoint
        else
            let prefix = sharedPrefixLength previousRaw rawPoints
            let incrementalShape = prefix = min previousRaw.Length rawPoints.Length || previousRaw.Length = rawPoints.Length
            if not incrementalShape then rawPoints |> Array.map mapPoint
            else Array.append (previousResolved |> Array.take prefix) (rawPoints |> Array.skip prefix |> Array.map mapPoint)

    let prepareData data =
        let axes =
            data
            |> Map.toArray
            |> Array.choose (fun (_, value) ->
                prepareAxis value)
            |> Map.ofArray

        let resolved =
            data
            |> Map.toArray
            |> Array.choose (fun (dataRef, value) ->
                match value with
                | SduiValue.Array values ->
                    values
                    |> Array.map (fun item ->
                        let temporal, payload = pointPayload item
                        { Payload = payload; Temporal = temporal })
                    |> fun points -> Some(dataRef, points)
                | _ ->
                    match tryTemporalSeries value with
                    | Some(axisRef, axisRevision, points) ->
                        match Map.tryFind axisRef axes with
                        | Some axis when axis.Revision = axisRevision ->
                            points
                            |> Array.choose (fun (position, payload) ->
                                Map.tryFind position axis.Points
                                |> Option.map (fun temporal -> { Payload = Some payload; Temporal = Some temporal }))
                            |> fun resolvedPoints -> Some(dataRef, resolvedPoints)
                        | _ -> Some(dataRef, [||])
                    | None -> None)
            |> Map.ofArray

        { RawData = data
          ResolvedAxes = axes
          ResolvedSeries = resolved }

    let prepareDataScheduled schedule data onCompleted =
        let entries = data |> Map.toArray
        let mutable axes = Map.empty
        let mutable resolved = Map.empty

        let rec prepareSeries index =
            if index >= entries.Length then
                onCompleted
                    { RawData = data
                      ResolvedAxes = axes
                      ResolvedSeries = resolved }
            else
                schedule (fun () ->
                    let dataRef, value = entries[index]
                    let next =
                        match value with
                        | SduiValue.Array values ->
                            values
                            |> Array.map (fun item ->
                                let temporal, payload = pointPayload item
                                { Payload = payload; Temporal = temporal })
                            |> Some
                        | _ ->
                            match tryTemporalSeries value with
                            | Some(axisRef, axisRevision, points) ->
                                match Map.tryFind axisRef axes with
                                | Some axis when axis.Revision = axisRevision ->
                                    points
                                    |> Array.choose (fun (position, payload) ->
                                        Map.tryFind position axis.Points
                                        |> Option.map (fun temporal -> { Payload = Some payload; Temporal = Some temporal }))
                                    |> Some
                                | _ -> Some [||]
                            | None -> None

                    match next with
                    | Some points -> resolved <- Map.add dataRef points resolved
                    | None -> ()
                    prepareSeries (index + 1))

        let rec prepareAxes index =
            if index >= entries.Length then
                prepareSeries 0
            else
                schedule (fun () ->
                    let _, value = entries[index]
                    match prepareAxis value with
                    | Some(axisRef, axis) -> axes <- Map.add axisRef axis axes
                    | None -> ()
                    prepareAxes (index + 1))

        prepareAxes 0

    let prepareDataIncremental previous data =
        let axisUpdates =
            data
            |> Map.toArray
            |> Array.choose (fun (_, value) -> updateAxis previous value)

        let axes =
            axisUpdates
            |> Array.map (fun (axisRef, axis, _) -> axisRef, axis)
            |> Map.ofArray

        let axisChanges =
            axisUpdates
            |> Array.map (fun (axisRef, _, changedPositions) -> axisRef, changedPositions)
            |> Map.ofArray

        let resolved =
            data
            |> Map.toArray
            |> Array.choose (fun (dataRef, value) ->
                match value with
                | SduiValue.Array rawPoints ->
                    match Map.tryFind dataRef previous.RawData, Map.tryFind dataRef previous.ResolvedSeries with
                    | Some(SduiValue.Array previousRaw), Some previousResolved ->
                        Some(dataRef, updateInlineSeries previousRaw previousResolved rawPoints)
                    | _ ->
                        rawPoints
                        |> Array.map (fun item ->
                            let temporal, payload = pointPayload item
                            { Payload = payload; Temporal = temporal })
                        |> fun points -> Some(dataRef, points)
                | _ ->
                    match tryTemporalSeriesRaw value with
                    | Some(axisRef, axisRevision, rawPoints) ->
                        match Map.tryFind axisRef axes with
                        | Some axis when axis.Revision = axisRevision ->
                            match Map.tryFind dataRef previous.RawData, Map.tryFind dataRef previous.ResolvedSeries with
                            | Some previousValue, Some previousResolved ->
                                match tryTemporalSeriesRaw previousValue with
                                | Some(previousAxisRef, _, previousRaw) when previousAxisRef = axisRef ->
                                    let changedAxisPositions = Map.tryFind axisRef axisChanges |> Option.defaultValue None
                                    Some(dataRef, updateTemporalSeries previousRaw previousResolved rawPoints axis.Points changedAxisPositions)
                                | _ -> Some(dataRef, resolveTemporalPoints axis.Points rawPoints)
                            | _ -> Some(dataRef, resolveTemporalPoints axis.Points rawPoints)
                        | _ -> Some(dataRef, [||])
                    | None -> None)
            |> Map.ofArray

        { RawData = data
          ResolvedAxes = axes
          ResolvedSeries = resolved }

    let prepareDataIncrementalScheduled schedule previous data onCompleted =
        let entries = data |> Map.toArray
        let mutable axes = Map.empty
        let mutable axisChanges = Map.empty
        let mutable resolved = Map.empty

        let rec prepareSeries index =
            if index >= entries.Length then
                onCompleted
                    { RawData = data
                      ResolvedAxes = axes
                      ResolvedSeries = resolved }
            else
                schedule (fun () ->
                    let dataRef, value = entries[index]
                    let next =
                        match value with
                        | SduiValue.Array rawPoints ->
                            match Map.tryFind dataRef previous.RawData, Map.tryFind dataRef previous.ResolvedSeries with
                            | Some(SduiValue.Array previousRaw), Some previousResolved ->
                                Some(updateInlineSeries previousRaw previousResolved rawPoints)
                            | _ ->
                                rawPoints
                                |> Array.map (fun item ->
                                    let temporal, payload = pointPayload item
                                    { Payload = payload; Temporal = temporal })
                                |> Some
                        | _ ->
                            match tryTemporalSeriesRaw value with
                            | Some(axisRef, axisRevision, rawPoints) ->
                                match Map.tryFind axisRef axes with
                                | Some axis when axis.Revision = axisRevision ->
                                    match Map.tryFind dataRef previous.RawData, Map.tryFind dataRef previous.ResolvedSeries with
                                    | Some previousValue, Some previousResolved ->
                                        match tryTemporalSeriesRaw previousValue with
                                        | Some(previousAxisRef, _, previousRaw) when previousAxisRef = axisRef ->
                                            let changedAxisPositions = Map.tryFind axisRef axisChanges |> Option.defaultValue None
                                            Some(updateTemporalSeries previousRaw previousResolved rawPoints axis.Points changedAxisPositions)
                                        | _ -> Some(resolveTemporalPoints axis.Points rawPoints)
                                    | _ -> Some(resolveTemporalPoints axis.Points rawPoints)
                                | _ -> Some [||]
                            | None -> None

                    match next with
                    | Some points -> resolved <- Map.add dataRef points resolved
                    | None -> ()
                    prepareSeries (index + 1))

        let rec prepareAxes index =
            if index >= entries.Length then
                prepareSeries 0
            else
                schedule (fun () ->
                    let _, value = entries[index]
                    match updateAxis previous value with
                    | Some(axisRef, axis, changedPositions) ->
                        axes <- Map.add axisRef axis axes
                        axisChanges <- Map.add axisRef changedPositions axisChanges
                    | None -> ()
                    prepareAxes (index + 1))

        prepareAxes 0

    let resolvedSeriesPrepared dataRef prepared =
        prepared.ResolvedSeries
        |> Map.tryFind dataRef
        |> Option.defaultValue [||]

    let resolvedSeries dataRef data =
        match Map.tryFind dataRef data with
        | Some(SduiValue.Array values) ->
            values
            |> Array.map (fun value ->
                let temporal, payload = pointPayload value
                { Payload = payload; Temporal = temporal })
        | Some value ->
            match tryTemporalSeries value with
            | Some(axisRef, axisRevision, points) ->
                match Map.tryFind axisRef data |> Option.bind tryTemporalAxis with
                | Some(encodedAxisRef, revision, axis) when encodedAxisRef = axisRef && revision = axisRevision ->
                    points
                    |> Array.choose (fun (position, payload) ->
                        Map.tryFind position axis
                        |> Option.map (fun temporal -> { Payload = Some payload; Temporal = Some temporal }))
                | _ -> [||]
            | None -> [||]
        | None -> [||]

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

    let presentationTimestamp (metadata: TaTemporalPointPresentation) =
        metadata.EventTimeUtc |> Option.defaultValue metadata.IntervalStartUtc

    let parseCandleResolved temporal payload =
        payload
        |> Option.bind tryObject
        |> Option.bind (fun item ->
            match
                (temporal |> Option.map presentationTimestamp |> Option.orElseWith (fun () -> objectText "t" item)),
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

    let parseCandle value =
        let temporal, payload = pointPayload value
        parseCandleResolved temporal payload

    let parseLineResolved temporal payload =
        match payload, temporal with
        | Some(SduiValue.Number lineValue), Some metadata ->
            Some { Timestamp = presentationTimestamp metadata; Value = lineValue; Temporal = temporal }
        | _ ->
            payload
            |> Option.bind tryObject
            |> Option.bind (fun item ->
                match temporal |> Option.map presentationTimestamp |> Option.orElseWith (fun () -> objectText "t" item), objectNumber "v" item with
                | Some timestamp, Some lineValue -> Some { Timestamp = timestamp; Value = lineValue; Temporal = temporal }
                | _ -> None)

    let parseLine value =
        let temporal, payload = pointPayload value
        parseLineResolved temporal payload

    let seriesValues dataRef data =
        resolvedSeries dataRef data
        |> Array.choose _.Payload

    let candleSeriesFromResolved resolved =
        resolved
        |> Array.choose (fun point -> parseCandleResolved point.Temporal point.Payload)

    let lineSeriesFromResolved resolved =
        resolved
        |> Array.choose (fun point -> parseLineResolved point.Temporal point.Payload)

    let candleSeries dataRef data =
        resolvedSeries dataRef data |> candleSeriesFromResolved

    let lineSeries dataRef data =
        resolvedSeries dataRef data |> lineSeriesFromResolved

    let candleSeriesPrepared dataRef prepared =
        resolvedSeriesPrepared dataRef prepared |> candleSeriesFromResolved

    let lineSeriesPrepared dataRef prepared =
        resolvedSeriesPrepared dataRef prepared |> lineSeriesFromResolved

    let linePointsByTimestamp (values: TaLinePoint array) =
        let lookup = Dictionary<string, TaLinePoint>()
        for point in values do
            lookup[point.Timestamp] <- point
        lookup

    let candlePointsByTimestamp (values: TaCandlePoint array) =
        let lookup = Dictionary<string, TaCandlePoint>()
        for point in values do
            lookup[point.Timestamp] <- point
        lookup

    let referenceSlotsByTimestamp (timestamps: string array) =
        let lookup = Dictionary<string, int>()
        for index in 0 .. timestamps.Length - 1 do
            if not (lookup.ContainsKey timestamps[index]) then
                lookup[timestamps[index]] <- index
        lookup

    let tryFindLinePoint key (lookup: Dictionary<string, TaLinePoint>) =
        match lookup.TryGetValue key with
        | true, value -> Some value
        | _ -> None

    let tryFindCandlePoint key (lookup: Dictionary<string, TaCandlePoint>) =
        match lookup.TryGetValue key with
        | true, value -> Some value
        | _ -> None

    let tryFindReferenceSlot key (lookup: Dictionary<string, int>) =
        match lookup.TryGetValue key with
        | true, value -> Some value
        | _ -> None

    let candleSeriesForTracePrepared (trace: TaTraceSpec) prepared =
        match trace.CandleDataRefs with
        | None -> candleSeriesPrepared trace.DataRef prepared
        | Some refs ->
            let valuesByTimestamp dataRef =
                lineSeriesPrepared dataRef prepared
                |> linePointsByTimestamp

            let opens = lineSeriesPrepared refs.OpenRef prepared
            let highs = valuesByTimestamp refs.HighRef
            let lows = valuesByTimestamp refs.LowRef
            let closes = valuesByTimestamp refs.CloseRef
            let volumes = valuesByTimestamp refs.VolumeRef

            opens
            |> Array.choose (fun openPoint ->
                match
                    tryFindLinePoint openPoint.Timestamp highs,
                    tryFindLinePoint openPoint.Timestamp lows,
                    tryFindLinePoint openPoint.Timestamp closes,
                    tryFindLinePoint openPoint.Timestamp volumes
                with
                | Some high, Some low, Some close, Some volume ->
                    Some
                        { Timestamp = openPoint.Timestamp
                          Open = openPoint.Value
                          High = high.Value
                          Low = low.Value
                          Close = close.Value
                          Volume = volume.Value
                          Temporal = openPoint.Temporal }
                | _ -> None)

    let sampleResolvedEvenly maximumCount (values: TaResolvedSeriesPoint array) =
        if maximumCount <= 0 || values.Length = 0 then
            [||]
        elif values.Length <= maximumCount then
            values
        elif maximumCount = 1 then
            [| values[values.Length - 1] |]
        else
            [| for sampleIndex in 0 .. maximumCount - 1 do
                   let sourceIndex =
                       int (Math.Round(float sampleIndex * float (values.Length - 1) / float (maximumCount - 1)))
                   yield values[sourceIndex] |]

    let candleSeriesForTracePreparedSampled maximumCount (trace: TaTraceSpec) prepared =
        match trace.CandleDataRefs with
        | None ->
            resolvedSeriesPrepared trace.DataRef prepared
            |> sampleResolvedEvenly maximumCount
            |> candleSeriesFromResolved
        | Some _ ->
            let values = candleSeriesForTracePrepared trace prepared
            if maximumCount <= 0 || values.Length = 0 then
                [||]
            elif values.Length <= maximumCount then
                values
            elif maximumCount = 1 then
                [| values[values.Length - 1] |]
            else
                [| for sampleIndex in 0 .. maximumCount - 1 do
                       let sourceIndex =
                           int (Math.Round(float sampleIndex * float (values.Length - 1) / float (maximumCount - 1)))
                       yield values[sourceIndex] |]

    let candleSeriesForTrace (trace: TaTraceSpec) data =
        candleSeriesForTracePrepared trace (prepareData data)

    let markerPlacementsPreparedWithIndexes
        (trace: TaTraceSpec)
        targetTraceId
        (targetByTimestamp: Dictionary<string, TaCandlePoint>)
        prepared
        (referenceSlots: Dictionary<string, int>)
        =
        prepared.RawData
        |> Map.tryFind trace.DataRef
        |> Option.bind tryTemporalSeries
        |> Option.bind (fun (axisRef, axisRevision, points) ->
            prepared.ResolvedAxes
            |> Map.tryFind axisRef
            |> Option.filter (fun axis -> axis.Revision = axisRevision)
            |> Option.map (fun axis ->
                points
                |> Array.collect (fun (position, payload) ->
                    match Map.tryFind position axis.Points, TaMarkerCodec.decodeBucket trace.DataRef payload with
                    | Some temporal, Ok markers ->
                        let timestamp = presentationTimestamp temporal
                        match
                            tryFindReferenceSlot timestamp referenceSlots,
                            tryFindCandlePoint timestamp targetByTimestamp
                        with
                        | Some slotIndex, Some targetPoint ->
                            markers
                            |> Array.map (fun marker ->
                                { TraceId = trace.TraceId
                                  TargetTraceId = targetTraceId
                                  Position = position
                                  SlotIndex = slotIndex
                                  Lane = 0
                                  Target = targetPoint
                                  Marker = marker })
                        | _ -> [||]
                    | _ -> [||])))
        |> Option.defaultValue [||]

    let markerPlacementsPrepared (trace: TaTraceSpec) (target: TaTraceSpec) prepared referenceTimestamps =
        let targetByTimestamp =
            candleSeriesForTracePrepared target prepared
            |> candlePointsByTimestamp
        let referenceSlots = referenceSlotsByTimestamp referenceTimestamps
        markerPlacementsPreparedWithIndexes trace target.TraceId targetByTimestamp prepared referenceSlots

    let assignAggregateMarkerLanes (placements: TaMarkerPlacement array) =
        let rec assign index counts assigned =
            if index >= placements.Length then
                assigned |> List.rev |> List.toArray
            else
                let placement = placements[index]
                let key = placement.TargetTraceId, placement.Position, placement.Marker.Anchor
                let lane = Map.tryFind key counts |> Option.defaultValue 0
                assign
                    (index + 1)
                    (Map.add key (lane + 1) counts)
                    ({ placement with Lane = lane } :: assigned)

        assign 0 Map.empty []

    [<Literal>]
    let DirectMarkerGlyphBudget = 4

    let markerPresentation (placements: TaMarkerPlacement array) =
        let groups =
            placements
            |> Array.groupBy (fun placement -> placement.TargetTraceId, placement.Position, placement.Marker.Anchor)

        let direct =
            groups
            |> Array.collect (fun (_, values) -> values |> Array.sortBy _.Lane |> Array.truncate DirectMarkerGlyphBudget)
            |> Array.sortBy (fun placement -> placement.SlotIndex, placement.Lane, placement.TraceId, placement.Marker.MarkerId)

        let overflow =
            groups
            |> Array.choose (fun ((targetTraceId, position, anchor), values) ->
                let ordered = values |> Array.sortBy _.Lane
                if ordered.Length <= DirectMarkerGlyphBudget then
                    None
                else
                    let hidden = ordered |> Array.skip DirectMarkerGlyphBudget
                    Some
                        { ClusterId = targetTraceId + ":" + string position + ":" + TaMarkerCodec.anchorText anchor
                          TargetTraceId = targetTraceId
                          Position = position
                          SlotIndex = ordered[0].SlotIndex
                          Anchor = anchor
                          Lane = DirectMarkerGlyphBudget
                          Markers = hidden })
            |> Array.sortBy (fun cluster -> cluster.SlotIndex, cluster.Lane, cluster.TargetTraceId)

        direct, overflow

    let overviewStripePlacementsPrepared (trace: TaTraceSpec) prepared referenceTimestamps =
        match TaOverviewStripeTraceOptionsCodec.tryDecode trace.Options with
        | None -> [||]
        | Some options ->
            prepared.RawData
            |> Map.tryFind trace.DataRef
            |> Option.bind tryTemporalSeries
            |> Option.bind (fun (axisRef, axisRevision, points) ->
                prepared.ResolvedAxes
                |> Map.tryFind axisRef
                |> Option.filter (fun axis -> axis.Revision = axisRevision)
                |> Option.map (fun axis ->
                    points
                    |> Array.collect (fun (position, payload) ->
                        match Map.tryFind position axis.Points, TaOverviewStripeCodec.decodeBucket trace.DataRef payload with
                        | Some temporal, Ok stripes ->
                            let timestamp = presentationTimestamp temporal
                            match referenceTimestamps |> Array.tryFindIndex ((=) timestamp) with
                            | Some slotIndex ->
                                stripes
                                |> Array.map (fun stripe ->
                                    { TraceId = trace.TraceId
                                      TargetTraceId = options.TargetTraceId
                                      CollisionGroup = options.CollisionGroup
                                      LayerOrder = options.LayerOrder
                                      Position = position
                                      SlotIndex = slotIndex
                                      Stripe = stripe })
                            | None -> [||]
                        | _ -> [||])))
            |> Option.defaultValue [||]

    let overviewStripeVisuals (placements: TaOverviewStripePlacement array) =
        let collapsed =
            placements
            |> Array.groupBy (fun placement ->
                placement.TargetTraceId,
                placement.CollisionGroup,
                placement.SlotIndex,
                placement.TraceId,
                placement.LayerOrder)
            |> Array.map (fun ((targetTraceId, collisionGroup, slotIndex, traceId, layerOrder), values) ->
                targetTraceId, collisionGroup, slotIndex, traceId, layerOrder, values |> Array.map _.Stripe)

        collapsed
        |> Array.groupBy (fun (targetTraceId, collisionGroup, slotIndex, _, _, _) -> targetTraceId, collisionGroup, slotIndex)
        |> Array.collect (fun (_, values) ->
            values
            |> Array.sortBy (fun (_, _, _, traceId, layerOrder, _) -> layerOrder, traceId)
            |> Array.mapi (fun lane (targetTraceId, collisionGroup, slotIndex, traceId, layerOrder, stripes) ->
                { TraceId = traceId
                  TargetTraceId = targetTraceId
                  CollisionGroup = collisionGroup
                  LayerOrder = layerOrder
                  Lane = lane
                  SlotIndex = slotIndex
                  Stripes = stripes }))
        |> Array.sortBy (fun value -> value.SlotIndex, value.LayerOrder, value.TraceId)

    let markerTrianglePoints (shape: TaMarkerShape) (x: float) (y: float) (half: float) =
        match shape with
        | TaMarkerShape.TriangleUp ->
            Some [| x - half, y + half; x + half, y + half; x, y - half |]
        | TaMarkerShape.TriangleDown ->
            Some [| x - half, y - half; x + half, y - half; x, y + half |]
        | _ -> None

    let markerTooltipText (placement: TaMarkerPlacement) =
        [| match placement.Marker.Label with
           | Some label when not (String.IsNullOrWhiteSpace label) -> yield label
           | _ -> ()
           yield "Event time: " + placement.Marker.EventTimeUtc
           for field in placement.Marker.Tooltip do
               yield field.Label + ": " + field.Value |]
        |> String.concat "\n"

    let markerLabelGeometry width height fontSize markerX markerY (label: string) =
        if String.IsNullOrWhiteSpace label then
            None
        else
            let trimmed = label.Trim()
            let bounded = if trimmed.Length <= 48 then trimmed else trimmed.Substring(0, 45) + "..."
            let estimatedWidth = min (width - 4.0) (max 24.0 (float bounded.Length * fontSize * 0.62))
            let gap = 8.0
            let edge = 2.0
            let x, anchor =
                if markerX + gap + estimatedWidth <= width - edge then markerX + gap, "start"
                elif markerX - gap - estimatedWidth >= edge then markerX - gap, "end"
                else edge, "start"
            let y = max 8.0 (min (height - 8.0) markerY)
            Some
                { Text = bounded
                  X = x
                  Y = y
                  TextAnchor = anchor
                  EstimatedWidth = estimatedWidth }

    let markerLabelCollisionLanes padding (candidates: (TaMarkerAnchor * TaMarkerLabelGeometry) array) =
        let interval geometry =
            if geometry.TextAnchor = "end" then
                geometry.X - geometry.EstimatedWidth, geometry.X
            else
                geometry.X, geometry.X + geometry.EstimatedWidth

        let ordered =
            candidates
            |> Array.mapi (fun index (_, geometry) ->
                let left, right = interval geometry
                index, left, right)
            |> Array.sortBy (fun (index, left, _) -> left, index)

        let rec firstAvailable (left: float) (laneEnds: float array) (lane: int) =
            if lane >= laneEnds.Length || left > laneEnds[lane] + padding then lane
            else firstAvailable left laneEnds (lane + 1)

        let rec assign (index: int) (laneEnds: float array) (assigned: Map<int, int>) =
            if index >= ordered.Length then assigned
            else
                let candidateIndex, left, right = ordered[index]
                let lane = firstAvailable left laneEnds 0
                let revisedLaneEnds =
                    if lane >= laneEnds.Length then Array.append laneEnds [| right |]
                    else laneEnds |> Array.mapi (fun current value -> if current = lane then right else value)
                assign
                    (index + 1)
                    revisedLaneEnds
                    (Map.add candidateIndex lane assigned)

        let assignments = assign 0 [||] Map.empty
        Array.init candidates.Length (fun index -> Map.find index assignments)

    let markerLabelLaneY height lineStep anchor baseY lane =
        let edge = 8.0
        let direction = if anchor = TaMarkerAnchor.AboveBar then -1.0 else 1.0
        let offset = float lane * lineStep
        let preferred = baseY + direction * offset
        let alternate = baseY - direction * offset
        if preferred >= edge && preferred <= height - edge then preferred
        else max edge (min (height - edge) alternate)

    let timestampParts (value: string) =
        if String.IsNullOrWhiteSpace value
           || value.Length < 19
           || value[4] <> '-'
           || value[7] <> '-'
           || (value[10] <> 'T' && value[10] <> ' ')
           || value[13] <> ':'
           || value[16] <> ':' then
            None
        else
            Some(value.Substring(0, 10), value.Substring(11, 8))

    let fullTimestamp value =
        timestampParts value
        |> Option.map (fun (date, time) -> date + " " + time)

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
                 CandleDataRefs = None
                 Options = Map.empty } |]

    let rowReferenceLength (row: TaRowSpec) data =
        effectiveTraces row
        |> Array.filter (fun trace -> trace.Visible && not (isOverlayTraceKind trace.Kind))
        |> Array.map (fun trace -> seriesValues trace.DataRef data |> Array.length)
        |> Array.sortDescending
        |> Array.tryHead
        |> Option.defaultValue 0

    let traceTimestamps (trace: TaTraceSpec) data =
        match trace.Kind with
        | TaTraceKind.Candlestick
        | TaTraceKind.Volume -> candleSeriesForTrace trace data |> Array.map _.Timestamp
        | TaTraceKind.Line
        | TaTraceKind.Histogram -> lineSeries trace.DataRef data |> Array.map _.Timestamp
        | TaTraceKind.Marker
        | TaTraceKind.OverviewStripe -> [||]

    let traceTimestampsPrepared (trace: TaTraceSpec) prepared =
        if isOverlayTraceKind trace.Kind then
            [||]
        else
            let dataRef =
                match trace.CandleDataRefs with
                | Some refs -> refs.OpenRef
                | None -> trace.DataRef
            let resolved = resolvedSeriesPrepared dataRef prepared
            let temporal =
                resolved
                |> Array.choose (fun point -> point.Temporal |> Option.map presentationTimestamp)
            if temporal.Length > 0 then
                temporal
            else
                match trace.Kind with
                | TaTraceKind.Candlestick
                | TaTraceKind.Volume -> candleSeriesForTracePrepared trace prepared |> Array.map _.Timestamp
                | TaTraceKind.Line
                | TaTraceKind.Histogram -> lineSeriesFromResolved resolved |> Array.map _.Timestamp
                | TaTraceKind.Marker
                | TaTraceKind.OverviewStripe -> [||]

    let traceTopologyTimestampsPrepared (trace: TaTraceSpec) prepared =
        if isOverlayTraceKind trace.Kind then
            [||]
        else
            let temporalPositions =
                resolvedSeriesPrepared trace.DataRef prepared
                |> Array.choose (fun point -> point.Temporal |> Option.map _.IntervalStartUtc)

            if temporalPositions.Length > 0 then temporalPositions
            else traceTimestampsPrepared trace prepared

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

    let referenceTimelinePrepared (rows: TaRowSpec array) prepared =
        let traces =
            rows
            |> Array.filter _.Visible
            |> Array.collect effectiveTraces
            |> Array.filter _.Visible

        traces
        |> Array.map (fun trace -> trace, traceTimestampsPrepared trace prepared |> Array.distinct)
        |> Array.filter (fun (_, timestamps) -> timestamps.Length > 0)
        |> Array.sortByDescending (fun (trace, timestamps) -> timestamps.Length, trace.Kind = TaTraceKind.Candlestick)
        |> Array.tryHead
        |> Option.map snd
        |> Option.defaultValue [||]

    let rowTimeline (row: TaRowSpec) data =
        let traces = effectiveTraces row |> Array.filter _.Visible
        traces
        |> Array.tryFind (fun trace -> trace.DataRef = row.DataRef)
        |> Option.orElseWith (fun () -> traces |> Array.tryHead)
        |> Option.map (fun trace -> traceTimestamps trace data |> Array.distinct)
        |> Option.defaultValue [||]

    let rowTimelinePrepared (row: TaRowSpec) prepared =
        let traces = effectiveTraces row |> Array.filter _.Visible
        traces
        |> Array.tryFind (fun trace -> trace.DataRef = row.DataRef)
        |> Option.orElseWith (fun () -> traces |> Array.tryHead)
        |> Option.map (fun trace -> traceTimestampsPrepared trace prepared |> Array.distinct)
        |> Option.defaultValue [||]

    let referenceTimelineForDocument (document: TaWorkspaceDocument) data =
        match document.BaseRowId with
        | Some baseRowId ->
            document.Rows
            |> Array.tryFind (fun row -> row.Visible && row.RowId = baseRowId)
            |> Option.map (fun row -> rowTimeline row data)
            |> Option.defaultValue [||]
        | None -> referenceTimeline document.Rows data

    let referenceTimelineForDocumentPrepared (document: TaWorkspaceDocument) prepared =
        match document.BaseRowId with
        | Some baseRowId ->
            document.Rows
            |> Array.tryFind (fun row -> row.Visible && row.RowId = baseRowId)
            |> Option.map (fun row -> rowTimelinePrepared row prepared)
            |> Option.defaultValue [||]
        | None -> referenceTimelinePrepared document.Rows prepared

    let traceReferencePoints (trace: TaTraceSpec) data =
        match trace.Kind with
        | TaTraceKind.Candlestick
        | TaTraceKind.Volume ->
            candleSeriesForTrace trace data
            |> Array.map (fun point -> point.Timestamp, point.Temporal)
        | TaTraceKind.Line
        | TaTraceKind.Histogram ->
            lineSeries trace.DataRef data
            |> Array.map (fun point -> point.Timestamp, point.Temporal)
        | TaTraceKind.Marker
        | TaTraceKind.OverviewStripe -> [||]
        |> Array.distinctBy fst

    let rowReferencePoints (row: TaRowSpec) data =
        let traces = effectiveTraces row |> Array.filter _.Visible
        traces
        |> Array.tryFind (fun trace -> trace.DataRef = row.DataRef)
        |> Option.orElseWith (fun () -> traces |> Array.tryHead)
        |> Option.map (fun trace -> traceReferencePoints trace data)
        |> Option.defaultValue [||]

    let referencePointsForDocument (document: TaWorkspaceDocument) data =
        match document.BaseRowId with
        | Some baseRowId ->
            document.Rows
            |> Array.tryFind (fun row -> row.Visible && row.RowId = baseRowId)
            |> Option.map (fun row -> rowReferencePoints row data)
            |> Option.defaultValue [||]
        | None ->
            document.Rows
            |> Array.filter _.Visible
            |> Array.collect effectiveTraces
            |> Array.filter _.Visible
            |> Array.map (fun trace -> trace, traceReferencePoints trace data)
            |> Array.filter (fun (_, points) -> points.Length > 0)
            |> Array.sortByDescending (fun (trace, points) -> points.Length, trace.Kind = TaTraceKind.Candlestick)
            |> Array.tryHead
            |> Option.map snd
            |> Option.defaultValue [||]

    let tryUtcTimestamp (value: string) =
        let trimmed = if isNull value then "" else value.Trim()
        let candidate =
            if trimmed.Length = 10 && trimmed[4] = '-' && trimmed[7] = '-' then
                trimmed + "T00:00:00Z"
            else
                trimmed
        let suffixStart =
            if candidate.EndsWith("Z") then candidate.Length - 1
            elif candidate.EndsWith("+00:00") then candidate.Length - 6
            else -1
        let fixedShape =
            suffixStart >= 19
            && candidate.Length >= 20
            && candidate[4] = '-'
            && candidate[7] = '-'
            && (candidate[10] = 'T' || candidate[10] = 't')
            && candidate[13] = ':'
            && candidate[16] = ':'
        let digitIndexes = [| 0; 1; 2; 3; 5; 6; 8; 9; 11; 12; 14; 15; 17; 18 |]
        let digitsValid =
            fixedShape
            && digitIndexes
               |> Array.forall (fun index ->
                   let character = candidate[index]
                   character >= '0' && character <= '9')
        let fraction =
            if suffixStart = 19 then Some ""
            elif suffixStart > 20 && candidate[19] = '.' then Some(candidate.Substring(20, suffixStart - 20))
            else None
        let fractionValid =
            fraction
            |> Option.exists (fun digits ->
                digits.Length <= 7
                && digits
                   |> Seq.forall (fun character -> character >= '0' && character <= '9'))

        if not digitsValid || not fractionValid then
            None
        else
            let number start count = Int32.Parse(candidate.Substring(start, count))
            let month = number 5 2
            let day = number 8 2
            let hour = number 11 2
            let minute = number 14 2
            let second = number 17 2
            let year = number 0 4
            let leapYear = year % 400 = 0 || (year % 4 = 0 && year % 100 <> 0)
            let daysInMonth =
                match month with
                | 2 -> if leapYear then 29 else 28
                | 4 | 6 | 9 | 11 -> 30
                | 1 | 3 | 5 | 7 | 8 | 10 | 12 -> 31
                | _ -> 0
            if day < 1 || day > daysInMonth || hour > 23 || minute > 59 || second > 59 then
                None
            else
                let normalizedFraction = ((fraction |> Option.defaultValue "") + "0000000").Substring(0, 7)
                Some(candidate.Substring(0, 10) + "T" + candidate.Substring(11, 8) + "." + normalizedFraction)

    let queryViewportSelection latestGeneration intentGeneration (query: TaQueryChange) (document: TaWorkspaceDocument) data =
        if intentGeneration <> latestGeneration then
            TaQueryViewportSelection.Stale
        else
            match query.FromUtc, query.ToUtcExclusive with
            | None, None -> TaQueryViewportSelection.NotRequested
            | Some fromText, Some toText ->
                match tryUtcTimestamp fromText, tryUtcTimestamp toText with
                | Some fromUtc, Some toUtc when fromUtc < toUtc ->
                    let parsedPoints =
                        referencePointsForDocument document data
                        |> Array.map (fun (timestamp, temporal) ->
                            match temporal with
                            | Some metadata ->
                                match tryUtcTimestamp metadata.IntervalStartUtc, tryUtcTimestamp metadata.IntervalEndUtc with
                                | Some startUtc, Some endUtc when startUtc < endUtc -> Some(startUtc, endUtc, true)
                                | _ -> None
                            | None ->
                                tryUtcTimestamp timestamp
                                |> Option.map (fun instant -> instant, instant, false))

                    if parsedPoints |> Array.exists Option.isNone then
                        TaQueryViewportSelection.Invalid "query-axis-invalid: one or more reference timestamps are not valid UTC values."
                    else
                        let points = parsedPoints |> Array.choose id
                        let mutable low = 0
                        let mutable high = points.Length
                        while low < high do
                            let middle = low + (high - low) / 2
                            let startUtc, endUtc, hasInterval = points[middle]
                            let startsAfterFrom = if hasInterval then endUtc > fromUtc else startUtc >= fromUtc
                            if startsAfterFrom then high <- middle else low <- middle + 1
                        let first = low

                        low <- 0
                        high <- points.Length
                        while low < high do
                            let middle = low + (high - low) / 2
                            let startUtc, _, _ = points[middle]
                            if startUtc >= toUtc then high <- middle else low <- middle + 1
                        let lastExclusive = low

                        if first >= lastExclusive then
                            TaQueryViewportSelection.NoIntersection
                        else
                            TaQueryViewportSelection.Selected
                                { StartIndex = first
                                  Count = lastExclusive - first }
                | Some _, Some _ ->
                    TaQueryViewportSelection.Invalid "query-range-invalid: FromUtc must be earlier than ToUtcExclusive."
                | _ ->
                    TaQueryViewportSelection.Invalid "query-range-invalid: FromUtc and ToUtcExclusive must be valid UTC timestamps."
            | _ ->
                TaQueryViewportSelection.Invalid "query-range-invalid: FromUtc and ToUtcExclusive must be supplied together."

    let tryBaseRow (document: TaWorkspaceDocument) : (string * TaRowSpec) option =
        document.BaseRowId
        |> Option.bind (fun baseRowId ->
            document.Rows
            |> Array.tryFind (fun row -> row.Visible && row.RowId = baseRowId)
            |> Option.map (fun row -> baseRowId, row))

    let tryBasePointIntervalEnd row data timestamp =
        effectiveTraces row
        |> Array.filter _.Visible
        |> Array.tryFind (fun trace -> trace.DataRef = row.DataRef)
        |> Option.bind (fun trace ->
            match trace.Kind with
            | TaTraceKind.Candlestick
            | TaTraceKind.Volume ->
                candleSeriesForTrace trace data
                |> Array.tryFind (fun point -> point.Timestamp = timestamp)
                |> Option.bind _.Temporal
                |> Option.map _.IntervalEndUtc
            | TaTraceKind.Line
            | TaTraceKind.Histogram ->
                lineSeries trace.DataRef data
                |> Array.tryFind (fun point -> point.Timestamp = timestamp)
                |> Option.bind _.Temporal
                |> Option.map _.IntervalEndUtc
            | TaTraceKind.Marker
            | TaTraceKind.OverviewStripe -> None)

    let tryBasePointIntervalEndPrepared row prepared timestamp =
        effectiveTraces row
        |> Array.filter _.Visible
        |> Array.tryFind (fun trace -> trace.DataRef = row.DataRef)
        |> Option.bind (fun trace ->
            let dataRef =
                match trace.CandleDataRefs with
                | Some refs -> refs.OpenRef
                | None -> trace.DataRef
            resolvedSeriesPrepared dataRef prepared
            |> Array.tryPick (fun point ->
                point.Temporal
                |> Option.filter (fun temporal -> presentationTimestamp temporal = timestamp)
                |> Option.map _.IntervalEndUtc))

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

    let finalizedTemporal (metadata: TaTemporalPointPresentation) =
        metadata.Finality.Trim().ToLower() = "final"

    let finalizedCursorMatch timestamp pointTimestamp temporal =
        match temporal with
        | None -> pointTimestamp = timestamp
        | Some metadata when not (finalizedTemporal metadata) -> false
        | Some metadata when metadata.Projection = "repeat-across-base-buckets" || metadata.Projection = "candle-span" ->
            timestampInInterval timestamp metadata
        | Some metadata when metadata.Projection = "step-after-close" ->
            availableAtOrAfter timestamp metadata
        | Some _ -> pointTimestamp = timestamp

    let finalizedAsOf timestamp temporal =
        temporal
        |> Option.exists (fun metadata -> finalizedTemporal metadata && availableAtOrAfter timestamp metadata)

    let tryCandleForCursor isBaseRow timestamp (values: TaCandlePoint array) =
        if isBaseRow then tryCandleAt timestamp values
        else
            values
            |> Array.tryFindBack (fun value -> finalizedCursorMatch timestamp value.Timestamp value.Temporal)
            |> Option.orElseWith (fun () ->
                values
                |> Array.tryFindBack (fun value -> finalizedAsOf timestamp value.Temporal))

    let tryLineForCursor isBaseRow timestamp (values: TaLinePoint array) =
        if isBaseRow then tryLineAt timestamp values
        else
            values
            |> Array.tryFindBack (fun value -> finalizedCursorMatch timestamp value.Timestamp value.Temporal)
            |> Option.orElseWith (fun () ->
                values
                |> Array.tryFindBack (fun value -> finalizedAsOf timestamp value.Temporal))

    let candleCursorPointValue label kind (point: TaCandlePoint) =
        let baseValue =
            if kind = TaTraceKind.Volume then fixedNumber point.Volume
            else
                "O " + fixedNumber point.Open
                + " H " + fixedNumber point.High
                + " L " + fixedNumber point.Low
                + " C " + fixedNumber point.Close
        let value =
            match point.Temporal with
            | Some metadata -> baseValue + " | " + metadata.ScaleKey + " " + metadata.Finality + " | " + metadata.SourceIntervalId
            | None -> baseValue
        { Label = label; Value = value }

    let candleCursorValue label kind isBaseRow timestamp values =
        tryCandleForCursor isBaseRow timestamp values
        |> Option.map (candleCursorPointValue label kind)

    let lineCursorPointValue label (point: TaLinePoint) =
        let value =
            match point.Temporal with
            | Some metadata -> fixedNumber point.Value + " | " + metadata.ScaleKey + " " + metadata.Finality + " | " + metadata.SourceIntervalId
            | None -> fixedNumber point.Value
        { Label = label; Value = value }

    let lineCursorValue label isBaseRow timestamp values =
        tryLineForCursor isBaseRow timestamp values
        |> Option.map (lineCursorPointValue label)

    let lowerTimestampBound (referenceTimestamps: string array) value =
        let mutable low = 0
        let mutable high = referenceTimestamps.Length
        while low < high do
            let middle = low + (high - low) / 2
            if compare referenceTimestamps[middle] value < 0 then low <- middle + 1
            else high <- middle
        low

    let upperTimestampBound (referenceTimestamps: string array) value =
        let mutable low = 0
        let mutable high = referenceTimestamps.Length
        while low < high do
            let middle = low + (high - low) / 2
            if compare referenceTimestamps[middle] value <= 0 then low <- middle + 1
            else high <- middle
        low

    let matchingReferenceRange (referenceTimestamps: string array) pointTimestamp temporal =
        let first, lastExclusive =
            match temporal with
            | Some metadata when metadata.Projection = "repeat-across-base-buckets" || metadata.Projection = "candle-span" ->
                lowerTimestampBound referenceTimestamps metadata.IntervalStartUtc,
                lowerTimestampBound referenceTimestamps metadata.IntervalEndUtc
            | Some metadata when metadata.Projection = "step-after-close" ->
                match metadata.AvailableAtUtc with
                | Some availableAt -> lowerTimestampBound referenceTimestamps availableAt, referenceTimestamps.Length
                | None -> 0, 0
            | _ ->
                lowerTimestampBound referenceTimestamps pointTimestamp,
                upperTimestampBound referenceTimestamps pointTimestamp

        if first >= lastExclusive then None
        else Some(first, lastExclusive)

    let matchingReferenceIndexes (referenceTimestamps: string array) pointTimestamp temporal =
        match matchingReferenceRange referenceTimestamps pointTimestamp temporal with
        | Some(first, lastExclusive) -> [| first .. lastExclusive - 1 |]
        | None -> [||]

    let projectedSourceTimestampsWhere includePoint (referenceTimestamps: string array) dataRef prepared =
        let projected: string option array = Array.create referenceTimestamps.Length None
        resolvedSeriesPrepared dataRef prepared
        |> Array.iteri (fun sourceIndex point ->
            if includePoint point then
                let sourceTimestamp = point.Temporal |> Option.map presentationTimestamp
                let range =
                    match sourceTimestamp, point.Temporal with
                    | Some timestamp, temporal -> matchingReferenceRange referenceTimestamps timestamp temporal
                    | None, _ when sourceIndex < referenceTimestamps.Length -> Some(sourceIndex, sourceIndex + 1)
                    | _ -> None

                match range with
                | Some(first, lastExclusive) ->
                    let timestamp = sourceTimestamp |> Option.defaultValue referenceTimestamps[first]
                    for index in first .. lastExclusive - 1 do
                        projected[index] <- Some timestamp
                | None -> ())
        projected

    let projectedSourceTimestamps referenceTimestamps dataRef prepared =
        projectedSourceTimestampsWhere (fun _ -> true) referenceTimestamps dataRef prepared

    let projectedLastSourceTimestampWhere includePoint (referenceTimestamps: string array) dataRef prepared =
        let projected: string option array = Array.create referenceTimestamps.Length None
        resolvedSeriesPrepared dataRef prepared
        |> Array.tryLast
        |> Option.filter includePoint
        |> Option.iter (fun point ->
            let sourceTimestamp = point.Temporal |> Option.map presentationTimestamp
            let range =
                match sourceTimestamp, point.Temporal with
                | Some timestamp, temporal -> matchingReferenceRange referenceTimestamps timestamp temporal
                | None, _ when referenceTimestamps.Length > 0 -> Some(referenceTimestamps.Length - 1, referenceTimestamps.Length)
                | _ -> None

            match range with
            | Some(first, lastExclusive) ->
                let timestamp = sourceTimestamp |> Option.defaultValue referenceTimestamps[first]
                for index in first .. lastExclusive - 1 do
                    projected[index] <- Some timestamp
            | None -> ())
        projected

    let projectedLinePoints (referenceTimestamps: string array) (points: TaLinePoint array) =
        let projected: TaLinePoint option array = Array.create referenceTimestamps.Length None
        let nextUnassignedSlot = Array.init (referenceTimestamps.Length + 1) id

        let rec findNextUnassigned index =
            let parent = nextUnassignedSlot[index]
            if parent = index then index
            else
                let root = findNextUnassigned parent
                nextUnassignedSlot[index] <- root
                root

        // Reverse assignment preserves the prior "last matching source point wins" rule,
        // while path compression ensures every reference slot is materialized at most once.
        let mutable sourceIndex = points.Length - 1
        while sourceIndex >= 0 do
            let point = points[sourceIndex]
            match matchingReferenceRange referenceTimestamps point.Timestamp point.Temporal with
            | Some(first, lastExclusive) ->
                let mutable targetIndex = findNextUnassigned first
                while targetIndex < lastExclusive do
                    projected[targetIndex] <- Some point
                    nextUnassignedSlot[targetIndex] <- findNextUnassigned (targetIndex + 1)
                    targetIndex <- nextUnassignedSlot[targetIndex]
            | None -> ()
            sourceIndex <- sourceIndex - 1

        projected
        |> Array.mapi (fun index point -> point |> Option.map (fun value -> index, value))
        |> Array.choose id

    let compactProjectedLinePoints maximumVisualPoints referenceLength (values: (int * TaLinePoint) array) =
        if referenceLength <= maximumVisualPoints || values.Length <= maximumVisualPoints then
            values
        else
            let bucketCount = max 1 (maximumVisualPoints / 2)
            let occupied = Array.create bucketCount false
            let minimumSlots = Array.zeroCreate<int> bucketCount
            let minimumPoints = Array.zeroCreate<TaLinePoint> bucketCount
            let maximumSlots = Array.zeroCreate<int> bucketCount
            let maximumPoints = Array.zeroCreate<TaLinePoint> bucketCount

            for slotIndex, point in values do
                let bucketIndex = min (bucketCount - 1) (slotIndex * bucketCount / referenceLength)
                if not occupied[bucketIndex] then
                    occupied[bucketIndex] <- true
                    minimumSlots[bucketIndex] <- slotIndex
                    minimumPoints[bucketIndex] <- point
                    maximumSlots[bucketIndex] <- slotIndex
                    maximumPoints[bucketIndex] <- point
                else
                    if point.Value < minimumPoints[bucketIndex].Value then
                        minimumSlots[bucketIndex] <- slotIndex
                        minimumPoints[bucketIndex] <- point
                    if point.Value > maximumPoints[bucketIndex].Value then
                        maximumSlots[bucketIndex] <- slotIndex
                        maximumPoints[bucketIndex] <- point

            let compacted = ResizeArray<int * TaLinePoint>(maximumVisualPoints)
            for bucketIndex in 0 .. bucketCount - 1 do
                if occupied[bucketIndex] then
                    let minimum = minimumSlots[bucketIndex], minimumPoints[bucketIndex]
                    let maximum = maximumSlots[bucketIndex], maximumPoints[bucketIndex]
                    if fst minimum = fst maximum then
                        compacted.Add minimum
                    elif fst minimum < fst maximum then
                        compacted.Add minimum
                        compacted.Add maximum
                    else
                        compacted.Add maximum
                        compacted.Add minimum

            compacted.ToArray()

    let candleSlotRange (referenceTimestamps: string array) (point: TaCandlePoint) =
        matchingReferenceRange referenceTimestamps point.Timestamp point.Temporal

    let projectedCandleSlots (referenceTimestamps: string array) (point: TaCandlePoint) =
        let matchingSlots = matchingReferenceIndexes referenceTimestamps point.Timestamp point.Temporal
        let sourceSpanCount = matchingSlots.Length
        matchingSlots |> Array.map (fun slotIndex -> slotIndex, sourceSpanCount, point)

    let projectRanges referenceCount (values: 'T array) rangeForValue =
        let projected: 'T option array = Array.create referenceCount None
        let nextUnassignedSlot = Array.init (referenceCount + 1) id

        let rec findNextUnassigned index =
            let parent = nextUnassignedSlot[index]
            if parent = index then index
            else
                let root = findNextUnassigned parent
                nextUnassignedSlot[index] <- root
                root

        let mutable sourceIndex = values.Length - 1
        while sourceIndex >= 0 do
            let value = values[sourceIndex]
            match rangeForValue value with
            | Some(first, lastExclusive) when first < lastExclusive ->
                let mutable targetIndex = findNextUnassigned first
                while targetIndex < lastExclusive do
                    projected[targetIndex] <- Some value
                    nextUnassignedSlot[targetIndex] <- findNextUnassigned (targetIndex + 1)
                    targetIndex <- nextUnassignedSlot[targetIndex]
            | _ -> ()
            sourceIndex <- sourceIndex - 1

        projected

    let projectedCandleCursorValues isBaseRow (referenceTimestamps: string array) (values: TaCandlePoint array) =
        if isBaseRow then
            projectRanges
                referenceTimestamps.Length
                values
                (fun value -> matchingReferenceRange referenceTimestamps value.Timestamp value.Temporal)
        else
            let primary =
                projectRanges
                    referenceTimestamps.Length
                    values
                    (fun value ->
                        match value.Temporal with
                        | Some metadata when finalizedTemporal metadata ->
                            matchingReferenceRange referenceTimestamps value.Timestamp value.Temporal
                        | _ -> None)

            let fallback =
                projectRanges
                    referenceTimestamps.Length
                    values
                    (fun value ->
                        match value.Temporal with
                        | Some metadata when finalizedTemporal metadata ->
                            metadata.AvailableAtUtc
                            |> Option.bind (fun availableAt ->
                                let first = lowerTimestampBound referenceTimestamps availableAt
                                if first < referenceTimestamps.Length then Some(first, referenceTimestamps.Length)
                                else None)
                        | _ -> None)

            Array.map2 (fun direct prior -> direct |> Option.orElse prior) primary fallback

    let temporalDetail (metadata: TaTemporalPointPresentation) =
        let availability = metadata.AvailableAtUtc |> Option.defaultValue "unknown"
        let quality = metadata.Quality |> Option.defaultValue "unknown"
        $"{metadata.ScaleKey} | {metadata.Finality} | quality {quality} | frontier {metadata.ObservedThroughUtc} | available {availability}"

    let latestTemporalMetadata (trace: TaTraceSpec) data =
        resolvedSeries trace.DataRef data
        |> Array.choose _.Temporal
        |> Array.tryLast

    let latestTemporalMetadataPrepared (trace: TaTraceSpec) prepared =
        resolvedSeriesPrepared trace.DataRef prepared
        |> Array.choose _.Temporal
        |> Array.tryLast

    let rowTemporalMetadata (row: TaRowSpec) data =
        effectiveTraces row
        |> Array.filter (fun trace -> trace.Visible && not (isOverlayTraceKind trace.Kind))
        |> Array.choose (fun trace -> latestTemporalMetadata trace data)
        |> Array.distinctBy (fun value -> value.ScaleKey, value.Finality, value.ObservedThroughUtc, value.Quality)

    let rowTemporalMetadataPrepared (row: TaRowSpec) prepared =
        effectiveTraces row
        |> Array.filter (fun trace -> trace.Visible && not (isOverlayTraceKind trace.Kind))
        |> Array.choose (fun trace -> latestTemporalMetadataPrepared trace prepared)
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

    let visibleEventRange (document: TaWorkspaceDocument) data window =
        match tryBaseRow document with
        | None -> None
        | Some(baseRowId, baseRow) ->
            let timeline = rowTimeline baseRow data
            let selected = selectWindow window timeline
            if selected.Length = 0 then None
            else
                let startTime = selected[0]
                let endIndex = window.StartIndex + selected.Length
                let endExclusive =
                    if endIndex < timeline.Length then Some timeline[endIndex]
                    else tryBasePointIntervalEnd baseRow data selected[selected.Length - 1]

                endExclusive
                |> Option.filter (fun value -> compare value startTime > 0)
                |> Option.map (fun value ->
                    { BaseRowId = baseRowId
                      StartEventTimeUtc = startTime
                      EndEventTimeExclusiveUtc = value })

    let visibleEventRangePrepared (document: TaWorkspaceDocument) prepared window =
        match tryBaseRow document with
        | None -> None
        | Some(baseRowId, baseRow) ->
            let timeline = rowTimelinePrepared baseRow prepared
            let selected = selectWindow window timeline
            if selected.Length = 0 then None
            else
                let startTime = selected[0]
                let endIndex = window.StartIndex + selected.Length
                let endExclusive =
                    if endIndex < timeline.Length then Some timeline[endIndex]
                    else tryBasePointIntervalEndPrepared baseRow prepared selected[selected.Length - 1]

                endExclusive
                |> Option.filter (fun value -> compare value startTime > 0)
                |> Option.map (fun value ->
                    { BaseRowId = baseRowId
                      StartEventTimeUtc = startTime
                      EndEventTimeExclusiveUtc = value })

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

    let adaptiveTimeLabels minimumSpacing width (timestamps: string array) =
        if timestamps.Length = 0 then
            [||]
        elif timestamps.Length = 1 then
            [| 0, timestamps[0] |]
        else
            let spacing = max 48.0 minimumSpacing
            let targetCount = max 2 (min 16 (int (Math.Floor(max spacing width / spacing))))
            let count = min timestamps.Length targetCount
            [| for tickIndex in 0 .. count - 1 do
                   let sourceIndex =
                       int (Math.Round(float tickIndex * float (timestamps.Length - 1) / float (count - 1)))
                   yield sourceIndex, timestamps[sourceIndex] |]
            |> Array.distinctBy fst

    let coverageExtended direction (oldTimeline: string array) (newTimeline: string array) =
        if oldTimeline.Length = 0 || newTimeline.Length <= oldTimeline.Length then
            false
        else
            match direction with
            | TaCoverageDirection.Earlier ->
                newTimeline |> Array.tryFindIndex ((=) oldTimeline[0]) |> Option.exists (fun index -> index > 0)
            | TaCoverageDirection.Later ->
                newTimeline
                |> Array.tryFindIndex ((=) oldTimeline[oldTimeline.Length - 1])
                |> Option.exists (fun index -> index < newTimeline.Length - 1)

    let tryReanchorWindow minimumCount maximumCount delta (oldTimeline: string array) (newTimeline: string array) oldWindow =
        let oldBounded = clampWindow minimumCount maximumCount oldTimeline.Length oldWindow
        oldTimeline
        |> Array.tryItem oldBounded.StartIndex
        |> Option.bind (fun anchor -> newTimeline |> Array.tryFindIndex ((=) anchor))
        |> Option.map (fun anchorIndex ->
            clampWindow
                minimumCount
                maximumCount
                newTimeline.Length
                { StartIndex = anchorIndex + delta
                  Count = oldBounded.Count })

    let tryAdjacentCoverageRange direction maximumBasePoints (document: TaWorkspaceDocument) prepared =
        document.BaseRowId
        |> Option.bind (fun baseRowId ->
            document.Rows
            |> Array.tryFind (fun row -> row.RowId = baseRowId)
            |> Option.bind (fun baseRow ->
                let timeline = referenceTimelineForDocumentPrepared document prepared
                let query = queryDraft document.DefaultView
                let candidate =
                    match direction, Array.tryHead timeline, Array.tryLast timeline with
                    | TaCoverageDirection.Earlier, Some loadedFirst, _ ->
                        Some(query.FromUtc, loadedFirst)
                    | TaCoverageDirection.Later, _, Some loadedLast ->
                        match tryBasePointIntervalEndPrepared baseRow prepared loadedLast with
                        | Some loadedEnd -> Some(loadedEnd, query.ToUtcExclusive)
                        | _ -> None
                    | _ -> None

                candidate
                |> Option.bind (fun (startUtc, endUtc) ->
                    match tryUtcTimestamp startUtc, tryUtcTimestamp endUtc with
                    | Some normalizedStart, Some normalizedEnd when normalizedStart.CompareTo(normalizedEnd) < 0 ->
                        Some
                            { BaseRowId = baseRowId
                              StartEventTimeUtc = startUtc
                              EndEventTimeExclusiveUtc = endUtc
                              MaximumBasePoints = max 1 maximumBasePoints }
                    | _ -> None)))

    let cursorSnapshotForRows (document: TaWorkspaceDocument) visibleRows data window cursorIndex =
        let timeline = referenceTimelineForDocument document data
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
                    let isBaseRow = document.BaseRowId = Some row.RowId
                    effectiveTraces row
                    |> Array.filter _.Visible
                    |> Array.choose (fun trace ->
                        let label = if String.IsNullOrWhiteSpace trace.Label then trace.TraceId else trace.Label
                        match trace.Kind with
                        | TaTraceKind.Candlestick
                        | TaTraceKind.Volume ->
                            candleSeriesForTrace trace data
                            |> candleCursorValue label trace.Kind isBaseRow timestamp
                            |> Option.map (fun value -> timestamp, value)
                        | TaTraceKind.Line
                        | TaTraceKind.Histogram ->
                            lineSeries trace.DataRef data
                            |> lineCursorValue label isBaseRow timestamp
                            |> Option.map (fun value -> timestamp, value)
                        | TaTraceKind.Marker
                        | TaTraceKind.OverviewStripe -> None))

            Some
                { VisibleIndex = index
                  Timestamp = timestamp
                  Values = values |> Array.map snd }

    let cursorSnapshot (document: TaWorkspaceDocument) data window cursorIndex =
        cursorSnapshotForRows document (document.Rows |> Array.filter _.Visible) data window cursorIndex

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
