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

type TaTraceSpec =
    { TraceId: string
      Kind: TaTraceKind
      DataRef: string
      Label: string
      Color: string
      Width: float
      Visible: bool
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
                 Options = Map.empty } |]

    let dataRefs row =
        effectiveTraces row
        |> Array.map _.DataRef
        |> Array.append [| row.DataRef |]
        |> Array.filter (String.IsNullOrWhiteSpace >> not)
        |> Array.distinct

type TaWorkspaceDocument =
    { WorkspaceId: string
      Title: string
      RowsRef: string
      StatusRef: string
      SharedTimeAxis: bool
      Rows: TaRowSpec array
      AllowedActions: string array
      DefaultView: Map<string, SduiValue> }

type RuntimeSnapshot =
    { Data: Map<string, SduiValue>
      Freshness: TaFreshness }

[<RequireQualifiedAccess>]
type PatchOperation =
    | ReplaceDataRef of dataRef: string * value: SduiValue
    | UpsertSeriesPoints of dataRef: string * keyField: string * items: Map<string, SduiValue> array
    | RemoveSeriesBefore of dataRef: string * keyField: string * key: SduiValue
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

    let limits =
        { MaxRowsPerCanvas = 8
          MaxTracesPerRow = 32
          MaxTotalTraces = 64
          MaxInitialBarsPerSeries = 5000
          MaxRetainedBarsPerSeries = 2000
          MaxPatchOperations = 32
          MaxPatchItems = 500
          MaxFrameBytes = 16 * 1024 * 1024
          MinimumPollInterval = TimeSpan.FromSeconds 5.0 }
