namespace PulseTrade.Comm.Spa.Dynamic.Contracts

open System
open WebSharper

[<Struct>]
type CanvasInstanceId = CanvasInstanceId of string

[<Struct>]
type DocumentId = DocumentId of string

[<RequireQualifiedAccess>]
type SduiValue =
    | Null
    | Bool of bool
    | Number of float
    | Text of string
    | Array of SduiValue array
    | Object of Map<string, SduiValue>

[<RequireQualifiedAccess>]
type PointFinality =
    | Preview
    | Final

[<RequireQualifiedAccess>]
type TemporalProjection =
    | CandleSpan
    | RepeatAcrossBaseBuckets
    | StepAfterClose

type TemporalPoint =
    { SourceIntervalId: string
      ScaleKey: string
      IntervalStartUtc: DateTimeOffset
      IntervalEndUtc: DateTimeOffset
      ObservedThroughUtc: DateTimeOffset
      AvailableAtUtc: DateTimeOffset option
      Finality: PointFinality
      Projection: TemporalProjection
      Quality: string option
      Value: SduiValue option }

type TemporalAxisPoint =
    { Position: int64
      SourceIntervalId: string
      ScaleKey: string
      IntervalStartUtc: DateTimeOffset
      IntervalEndUtc: DateTimeOffset
      ObservedThroughUtc: DateTimeOffset
      AvailableAtUtc: DateTimeOffset option
      Finality: PointFinality
      Projection: TemporalProjection
      Quality: string option }

type TemporalAxis =
    { AxisRef: string
      Revision: int64
      Points: TemporalAxisPoint array }

type TemporalSeriesPoint =
    { Position: int64
      Value: SduiValue }

type TemporalSeries =
    { AxisRef: string
      AxisRevision: int64
      Points: TemporalSeriesPoint array }

[<RequireQualifiedAccess>]
type TaFreshness =
    | Live
    | Delayed of lag: TimeSpan
    | Stale of lag: TimeSpan * reasonCode: string
    | Backfill of reasonCode: string
    | Unavailable of reasonCode: string

[<RequireQualifiedAccess>]
type TaRowKind =
    | Candlestick
    | Volume
    | Sma
    | Dmi
    | Adx
    | Macd
    | HeikinAshi

[<RequireQualifiedAccess>]
type TaTraceKind =
    | Candlestick
    | Volume
    | Line
    | Histogram
    | Marker

[<RequireQualifiedAccess>]
type TaMarkerAnchor =
    | AboveBar
    | BelowBar

[<RequireQualifiedAccess>]
type TaMarkerShape =
    | TriangleUp
    | TriangleDown
    | Circle
    | Square
    | Diamond

[<RequireQualifiedAccess>]
type TaMarkerFill =
    | Solid
    | Outline

type TaMarkerTooltipField =
    { Key: string
      Label: string
      Value: string }

type TaMarker =
    { MarkerId: string
      EventTimeUtc: string
      Anchor: TaMarkerAnchor
      Shape: TaMarkerShape
      Fill: TaMarkerFill
      Color: string
      Label: string option
      Tooltip: TaMarkerTooltipField array }

type TaMarkerTraceOptions =
    { TargetTraceId: string }

type TaCandleDataRefs =
    { OpenRef: string
      HighRef: string
      LowRef: string
      CloseRef: string
      VolumeRef: string }

type TaTraceSpec =
    { TraceId: string
      Kind: TaTraceKind
      DataRef: string
      Label: string
      Color: string
      Width: float
      Visible: bool
      CandleDataRefs: TaCandleDataRefs option
      Options: Map<string, SduiValue> }

type TaRowSpec =
    { RowId: string
      Kind: TaRowKind
      DataRef: string
      HeightWeight: float
      Visible: bool
      Options: Map<string, SduiValue>
      Traces: TaTraceSpec array }

[<JavaScript; RequireQualifiedAccess>]
module TaRowSpec =
    let legacyTraceKind = function
        | TaRowKind.Candlestick
        | TaRowKind.HeikinAshi -> TaTraceKind.Candlestick
        | TaRowKind.Volume -> TaTraceKind.Volume
        | _ -> TaTraceKind.Line

    let effectiveTraces (row: TaRowSpec) =
        if not (isNull row.Traces) && row.Traces.Length > 0 then
            row.Traces
        else
            [| { TraceId = row.RowId
                 Kind = legacyTraceKind row.Kind
                 DataRef = row.DataRef
                 Label = row.RowId
                 Color = ""
                 Width = 2.0
                 Visible = true
                 CandleDataRefs = None
                 Options = Map.empty } |]

    let dataRefs row =
        effectiveTraces row
        |> Array.collect (fun trace ->
            match trace.CandleDataRefs with
            | Some refs -> [| trace.DataRef; refs.OpenRef; refs.HighRef; refs.LowRef; refs.CloseRef; refs.VolumeRef |]
            | None -> [| trace.DataRef |])
        |> Array.append [| row.DataRef |]
        |> Array.filter (String.IsNullOrWhiteSpace >> not)
        |> Array.distinct

type EditorChoice =
    { Key: string
      Label: string
      Value: SduiValue }

[<RequireQualifiedAccess>]
type EditorValueKind =
    | Text
    | Integer of minValue: int64 option * maxValue: int64 option
    | Decimal of minValue: float option * maxValue: float option
    | Boolean
    | Choice of EditorChoice array
    | Scale of allowedScaleKeys: string array
    | List of item: EditorValueKind * minItems: int option * maxItems: int option
    | Group of fields: EditorFieldSchema array

and EditorFieldSchema =
    { Key: string
      Label: string
      Kind: EditorValueKind
      Required: bool
      DefaultValue: SduiValue option }

type DynamicTemplateSchema =
    { TemplateKey: string
      DisplayName: string
      SchemaRevision: int64
      Fields: EditorFieldSchema array }

type TaWorkspaceDocument =
    { WorkspaceId: string
      Title: string
      RowsRef: string
      StatusRef: string
      SharedTimeAxis: bool
      TemporalAxisRefs: string array
      BaseRowId: string option
      Rows: TaRowSpec array
      EditorSchemas: DynamicTemplateSchema array
      AllowedActions: string array
      DefaultView: Map<string, SduiValue> }

type RuntimeSnapshot =
    { Data: Map<string, SduiValue>
      Freshness: TaFreshness }

type RuntimeCacheIdentity =
    { OwnerFingerprint: string
      SchemaRevision: int64 }

type RuntimeCacheCoverage =
    { StartEventTimeUtc: DateTimeOffset
      EndEventTimeExclusiveUtc: DateTimeOffset }

type RuntimeCacheEntry =
    { CacheIdentity: RuntimeCacheIdentity
      WorkspaceId: string
      Document: TaWorkspaceDocument
      Snapshot: RuntimeSnapshot
      DocumentRevision: int64
      DataRevision: int64
      Coverage: RuntimeCacheCoverage
      CapturedAtUtc: DateTimeOffset }

[<RequireQualifiedAccess>]
type PatchOperation =
    | ReplaceDataRef of dataRef: string * value: SduiValue
    | UpsertSeriesPoints of dataRef: string * keyField: string * items: Map<string, SduiValue> array
    | RemoveSeriesBefore of dataRef: string * keyField: string * key: SduiValue
    | UpsertTemporalAxisPoints of axisRef: string * expectedRevision: int64 * newRevision: int64 * items: Map<string, SduiValue> array
    | RemoveTemporalAxisBefore of axisRef: string * expectedRevision: int64 * newRevision: int64 * position: int64
    | UpsertTemporalSeriesPoints of dataRef: string * axisRef: string * axisRevision: int64 * items: Map<string, SduiValue> array
    | RemoveTemporalSeriesBefore of dataRef: string * axisRef: string * axisRevision: int64 * position: int64
    | SetStatus of dataRef: string * value: Map<string, SduiValue>
    | SetOptions of targetId: string * value: Map<string, SduiValue>

type RuntimePatch =
    { Operations: PatchOperation array }

type RuntimeError =
    { ReasonCode: string
      Message: string
      Recoverable: bool }

type RuntimeHeartbeat =
    { ObservedAtUtc: DateTimeOffset }

[<RequireQualifiedAccess>]
type RuntimePayload =
    | Document of TaWorkspaceDocument
    | Snapshot of RuntimeSnapshot
    | Patch of RuntimePatch
    | Error of RuntimeError
    | Heartbeat of RuntimeHeartbeat

[<RequireQualifiedAccess>]
type RuntimeFrameKind =
    | Document
    | Snapshot
    | Patch
    | Error
    | Heartbeat

type RuntimeFrame =
    { Protocol: string
      Kind: RuntimeFrameKind
      DocumentId: DocumentId
      CanvasInstanceId: CanvasInstanceId
      DocumentRevision: int64
      BaseDataRevision: int64 option
      DataRevision: int64
      TransportSequence: int64
      Payload: RuntimePayload }

type TaQueryChange =
    { SourceId: string option
      Instrument: string option
      IntervalMinutes: int option
      FromUtc: string option
      ToUtcExclusive: string option
      IncludePartial: bool option }

type SharedCursorChange =
    { BaseRowId: string
      EventTimeUtc: string }

type VisibleRangeChange =
    { BaseRowId: string
      StartEventTimeUtc: string
      EndEventTimeExclusiveUtc: string
      MaximumBasePoints: int }

[<RequireQualifiedAccess>]
type EditorScalarValue =
    | Text of string
    | Number of float
    | Bool of bool

type EditorInputValue =
    { Path: string
      Value: EditorScalarValue }

[<RequireQualifiedAccess>]
type SduiAction =
    | ResetView of CanvasInstanceId
    | ResetCanvas of CanvasInstanceId
    | AddTaRow of CanvasInstanceId * TaRowSpec
    | ApplyTemplate of CanvasInstanceId * rowId: string option * templateKey: string * values: EditorInputValue array
    | RemoveTaRow of CanvasInstanceId * rowId: string
    | ChangeTaQuery of CanvasInstanceId * TaQueryChange
    | SharedCursorChanged of CanvasInstanceId * SharedCursorChange
    | VisibleRangeChanged of CanvasInstanceId * VisibleRangeChange
    | PollDelta of CanvasInstanceId * afterDataRevision: int64
    | RequestFullSnapshot of CanvasInstanceId * reasonCode: string

[<RequireQualifiedAccess>]
type RuntimeClientFrame =
    | Action of SduiAction
    | Mounted of CanvasInstanceId
    | Unmounted of CanvasInstanceId
    | PollCompleted of CanvasInstanceId * dataRevision: int64

type DynamicDiagnostic =
    { CanvasInstanceId: CanvasInstanceId option
      DocumentRevision: int64 option
      DataRevision: int64 option
      TransportSequence: int64 option
      ReasonCode: string
      LimitName: string option }

type DynamicValidationError =
    { Code: string
      Field: string
      Message: string }

type DynamicRuntimeLimits =
    { MaxRowsPerCanvas: int
      MaxTracesPerRow: int
      MaxTotalTraces: int
      MaxInitialBarsPerSeries: int
      MaxRetainedBarsPerSeries: int
      MaxPatchOperations: int
      MaxPatchItems: int
      MaxFrameBytes: int
      MinimumPollInterval: TimeSpan }

[<JavaScript; RequireQualifiedAccess>]
module DynamicRuntimeDefaults =
    [<Literal>]
    let protocol = "sdui-runtime.v1"

    [<Literal>]
    let markerProtocol = "sdui-runtime.v2"

    [<Literal>]
    let MaximumVisibleRangeBasePoints = 4000

    let limits =
        { MaxRowsPerCanvas = 8
          MaxTracesPerRow = 32
          MaxTotalTraces = 64
          MaxInitialBarsPerSeries = 5000
          MaxRetainedBarsPerSeries = 4000
          MaxPatchOperations = 64
          MaxPatchItems = 500
          MaxFrameBytes = 16 * 1024 * 1024
          MinimumPollInterval = TimeSpan.FromSeconds 5.0 }

[<JavaScript; RequireQualifiedAccess>]
module TaMarkerLimits =
    [<Literal>]
    let MaxMarkerIdLength = 128

    [<Literal>]
    let MaxLabelLength = 64

    [<Literal>]
    let MaxTooltipFields = 16

    [<Literal>]
    let MaxTooltipKeyLength = 64

    [<Literal>]
    let MaxTooltipLabelLength = 64

    [<Literal>]
    let MaxTooltipValueLength = 256

    [<Literal>]
    let MaxMarkersPerBucket = 4

    [<Literal>]
    let MaxMarkersPerLane = 4

    [<Literal>]
    let MaxMarkersPerDataRef = 10000

    [<Literal>]
    let MaxMarkersPerFrame = 20000

[<JavaScript; RequireQualifiedAccess>]
module TaMarkerTraceOptionsCodec =
    [<Literal>]
    let TargetTraceIdKey = "marker.targetTraceId"

    let encode value = Map [ TargetTraceIdKey, SduiValue.Text value.TargetTraceId ]

    let tryDecode (options: Map<string, SduiValue>) =
        match Map.tryFind TargetTraceIdKey options with
        | Some(SduiValue.Text value) when not (String.IsNullOrWhiteSpace value) -> Some { TargetTraceId = value }
        | _ -> None

[<JavaScript; RequireQualifiedAccess>]
module TaMarkerCodec =
    [<Literal>]
    let TypeKey = "_type"

    [<Literal>]
    let TypeValue = "ta-marker.v2"

    [<Literal>]
    let LegacyTypeValue = "ta-marker.v1"

    let error code field message =
        { Code = code; Field = field; Message = message }

    let anchorText = function TaMarkerAnchor.AboveBar -> "above-bar" | TaMarkerAnchor.BelowBar -> "below-bar"
    let shapeText = function
        | TaMarkerShape.TriangleUp -> "triangle-up"
        | TaMarkerShape.TriangleDown -> "triangle-down"
        | TaMarkerShape.Circle -> "circle"
        | TaMarkerShape.Square -> "square"
        | TaMarkerShape.Diamond -> "diamond"
    let fillText = function TaMarkerFill.Solid -> "solid" | TaMarkerFill.Outline -> "outline"

    let encodeTooltip (field: TaMarkerTooltipField) =
        SduiValue.Object(Map [ "key", SduiValue.Text field.Key; "label", SduiValue.Text field.Label; "value", SduiValue.Text field.Value ])

    let encode (marker: TaMarker) =
        SduiValue.Object(
            Map [ TypeKey, SduiValue.Text TypeValue
                  "markerId", SduiValue.Text marker.MarkerId
                  "eventTimeUtc", SduiValue.Text marker.EventTimeUtc
                  "anchor", SduiValue.Text(anchorText marker.Anchor)
                  "shape", SduiValue.Text(shapeText marker.Shape)
                  "fill", SduiValue.Text(fillText marker.Fill)
                  "color", SduiValue.Text marker.Color
                  "tooltip", SduiValue.Array(marker.Tooltip |> Array.map encodeTooltip)
                  match marker.Label with Some value -> "label", SduiValue.Text value | None -> () ])

    let encodeBucket markers = markers |> Array.map encode |> SduiValue.Array

    let objectText key values =
        match Map.tryFind key values with Some(SduiValue.Text value) -> Some value | _ -> None

    let requiredText maximum field key values =
        match objectText key values with
        | Some value when not (String.IsNullOrWhiteSpace value) && value.Length <= maximum -> Ok value
        | Some _ -> Error(error "invalid-text" field $"{field} must be nonblank and at most {maximum} characters.")
        | None -> Error(error "required" field $"{field} is required.")

    let isHex value =
        (value >= '0' && value <= '9') || (value >= 'a' && value <= 'f') || (value >= 'A' && value <= 'F')

    let validColor (value: string) =
        not (isNull value) && (value.Length = 4 || value.Length = 7 || value.Length = 9)
        && value[0] = '#' && value.Substring(1).ToCharArray() |> Array.forall isHex

    let isDigit value = value >= '0' && value <= '9'

    let validUtcTimestamp (value: string) =
        let suffix = not (isNull value) && (value.EndsWith("Z") || value.EndsWith("+00:00"))
        let shape =
            not (isNull value) && value.Length >= 20 && value.Length <= 35
            && value[4] = '-' && value[7] = '-' && (value[10] = 'T' || value[10] = 't')
            && value[13] = ':' && value[16] = ':'
        let digits =
            not (isNull value)
            && [| 0; 1; 2; 3; 5; 6; 8; 9; 11; 12; 14; 15; 17; 18 |]
               |> Array.forall (fun index -> index < value.Length && isDigit value[index])
        suffix && shape && digits

    let errors values = values |> List.choose (function Error item -> Some item | _ -> None)

    let decodeTooltipField field = function
        | SduiValue.Object values ->
            let expected = Set.ofList [ "key"; "label"; "value" ]
            let unknown = values |> Map.toList |> List.choose (fun (key, _) -> if Set.contains key expected then None else Some(error "unknown-marker-field" (field + "." + key) $"Unknown marker tooltip field `{key}`."))
            let key = requiredText TaMarkerLimits.MaxTooltipKeyLength (field + ".key") "key" values
            let label = requiredText TaMarkerLimits.MaxTooltipLabelLength (field + ".label") "label" values
            let value = requiredText TaMarkerLimits.MaxTooltipValueLength (field + ".value") "value" values
            match unknown @ errors [ key; label; value ] with
            | [] -> Ok ({ Key = Result.defaultValue "" key; Label = Result.defaultValue "" label; Value = Result.defaultValue "" value }: TaMarkerTooltipField)
            | items -> Error items
        | _ -> Error [ error "marker-tooltip-object-required" field "Marker tooltip item must be an object." ]

    let decode field = function
        | SduiValue.Object values ->
            let expected = Set.ofList [ TypeKey; "markerId"; "eventTimeUtc"; "anchor"; "shape"; "fill"; "color"; "label"; "tooltip" ]
            let unknown = values |> Map.toList |> List.choose (fun (key, _) -> if Set.contains key expected then None else Some(error "unknown-marker-field" (field + "." + key) $"Unknown marker field `{key}`."))
            let markerType = objectText TypeKey values
            let kind =
                match markerType with
                | Some value when value = TypeValue || value = LegacyTypeValue -> Ok ()
                | _ -> Error(error "marker-type-required" (field + "." + TypeKey) $"Expected `{TypeValue}` or legacy `{LegacyTypeValue}`.")
            let markerId = requiredText TaMarkerLimits.MaxMarkerIdLength (field + ".markerId") "markerId" values
            let eventTime =
                match objectText "eventTimeUtc" values with
                | Some value when validUtcTimestamp value -> Ok value
                | Some _ -> Error(error "invalid-timestamp" (field + ".eventTimeUtc") "eventTimeUtc must be a bounded ISO-8601 UTC timestamp ending in Z or +00:00.")
                | None -> Error(error "required" (field + ".eventTimeUtc") "eventTimeUtc is required.")
            let anchorTextValue = objectText "anchor" values
            let anchor = match anchorTextValue with Some "above-bar" -> Ok TaMarkerAnchor.AboveBar | Some "below-bar" -> Ok TaMarkerAnchor.BelowBar | _ -> Error(error "invalid-marker-anchor" (field + ".anchor") "anchor must be above-bar or below-bar.")
            let shape =
                match markerType, objectText "shape" values, anchorTextValue with
                | Some current, Some "triangle-up", _ when current = TypeValue -> Ok TaMarkerShape.TriangleUp
                | Some current, Some "triangle-down", _ when current = TypeValue -> Ok TaMarkerShape.TriangleDown
                | Some current, Some "circle", _ when current = TypeValue -> Ok TaMarkerShape.Circle
                | Some current, Some "square", _ when current = TypeValue -> Ok TaMarkerShape.Square
                | Some current, Some "diamond", _ when current = TypeValue -> Ok TaMarkerShape.Diamond
                | Some legacy, Some "arrow", Some "above-bar" when legacy = LegacyTypeValue -> Ok TaMarkerShape.TriangleDown
                | Some legacy, Some "arrow", Some "below-bar" when legacy = LegacyTypeValue -> Ok TaMarkerShape.TriangleUp
                | Some legacy, Some "circle", _ when legacy = LegacyTypeValue -> Ok TaMarkerShape.Circle
                | Some legacy, Some "square", _ when legacy = LegacyTypeValue -> Ok TaMarkerShape.Square
                | Some legacy, Some "diamond", _ when legacy = LegacyTypeValue -> Ok TaMarkerShape.Diamond
                | _ -> Error(error "invalid-marker-shape" (field + ".shape") "shape is not supported by the declared marker version.")
            let fill = match objectText "fill" values with Some "solid" -> Ok TaMarkerFill.Solid | Some "outline" -> Ok TaMarkerFill.Outline | _ -> Error(error "invalid-marker-fill" (field + ".fill") "fill must be solid or outline.")
            let color = match objectText "color" values with Some value when validColor value -> Ok value | _ -> Error(error "invalid-marker-color" (field + ".color") "color must be #RGB, #RRGGBB or #RRGGBBAA.")
            let label = match Map.tryFind "label" values with None | Some SduiValue.Null -> Ok None | Some(SduiValue.Text value) when value.Length <= TaMarkerLimits.MaxLabelLength -> Ok(Some value) | _ -> Error(error "invalid-marker-label" (field + ".label") $"label must be at most {TaMarkerLimits.MaxLabelLength} characters.")
            let tooltip =
                match Map.tryFind "tooltip" values with
                | Some(SduiValue.Array items) when items.Length <= TaMarkerLimits.MaxTooltipFields ->
                    let decoded = items |> Array.indexed |> Array.map (fun (index, item) -> decodeTooltipField $"{field}.tooltip[{index}]" item)
                    let failures = decoded |> Array.choose (function Error items -> Some items | _ -> None) |> Array.toList |> List.concat
                    if List.isEmpty failures then Ok(decoded |> Array.choose (function Ok item -> Some item | _ -> None)) else Error failures
                | Some(SduiValue.Array _) -> Error [ error "limit-marker-tooltip" (field + ".tooltip") $"tooltip exceeds {TaMarkerLimits.MaxTooltipFields} fields." ]
                | _ -> Error [ error "marker-tooltip-required" (field + ".tooltip") "tooltip must be an array." ]
            let failures =
                unknown @ errors [ kind; markerId |> Result.map ignore; eventTime |> Result.map ignore; anchor |> Result.map ignore; shape |> Result.map ignore; fill |> Result.map ignore; color |> Result.map ignore; label |> Result.map ignore ]
                @ (match tooltip with Error items -> items | _ -> [])
            match failures with
            | [] -> Ok ({ MarkerId = Result.defaultValue "" markerId; EventTimeUtc = Result.defaultValue "" eventTime; Anchor = Result.defaultValue TaMarkerAnchor.AboveBar anchor; Shape = Result.defaultValue TaMarkerShape.Circle shape; Fill = Result.defaultValue TaMarkerFill.Solid fill; Color = Result.defaultValue "#000000" color; Label = Result.defaultValue None label; Tooltip = Result.defaultValue [||] tooltip }: TaMarker)
            | items -> Error items
        | _ -> Error [ error "marker-object-required" field "Marker must be an object." ]

    let decodeBucket field = function
        | SduiValue.Array items when items.Length <= TaMarkerLimits.MaxMarkersPerBucket ->
            let decoded = items |> Array.indexed |> Array.map (fun (index, item) -> decode $"{field}[{index}]" item)
            let failures = decoded |> Array.choose (function Error items -> Some items | _ -> None) |> Array.toList |> List.concat
            if List.isEmpty failures then Ok(decoded |> Array.choose (function Ok item -> Some item | _ -> None)) else Error failures
        | SduiValue.Array _ -> Error [ error "limit-marker-bucket" field $"Marker bucket exceeds {TaMarkerLimits.MaxMarkersPerBucket} items." ]
        | _ -> Error [ error "marker-bucket-required" field "Marker bucket must be an array." ]

[<JavaScript; RequireQualifiedAccess>]
module TaMarkerContract =
    let markerTraces (document: TaWorkspaceDocument) =
        document.Rows
        |> Array.collect (fun row -> TaRowSpec.effectiveTraces row |> Array.filter (fun trace -> trace.Kind = TaTraceKind.Marker) |> Array.map (fun trace -> row, trace))

    let hasMarkers document = markerTraces document |> Array.isEmpty |> not

    let documentErrors (document: TaWorkspaceDocument) =
        markerTraces document
        |> Array.toList
        |> List.collect (fun (row, trace) ->
            match TaMarkerTraceOptionsCodec.tryDecode trace.Options with
            | None -> [ TaMarkerCodec.error "marker-target-required" $"document.rows.{row.RowId}.traces.{trace.TraceId}.options" "Marker trace requires marker.targetTraceId." ]
            | Some options ->
                match TaRowSpec.effectiveTraces row |> Array.tryFind (fun candidate -> candidate.TraceId = options.TargetTraceId) with
                | None -> [ TaMarkerCodec.error "marker-target-not-found" $"document.rows.{row.RowId}.traces.{trace.TraceId}.options" $"Marker target trace `{options.TargetTraceId}` was not found in the same row." ]
                | Some target when target.TraceId = trace.TraceId -> [ TaMarkerCodec.error "marker-self-target" $"document.rows.{row.RowId}.traces.{trace.TraceId}.options" "Marker trace cannot target itself." ]
                | Some target when target.Kind <> TaTraceKind.Candlestick -> [ TaMarkerCodec.error "marker-target-not-candlestick" $"document.rows.{row.RowId}.traces.{trace.TraceId}.options" "Marker target trace must be Candlestick." ]
                | Some _ -> [])
