namespace PulseTrade.Comm.Spa.Dynamic.Renderer

open System
open PulseTrade.Comm.Spa.Dynamic.Contracts
open WebSharper
open WebSharper.JavaScript
open WebSharper.JavaScript.Dom
open WebSharper.UI
open WebSharper.UI.Html
open WebSharper.UI.Client

type DynamicHostError =
    { Code: string
      Message: string }

type TaRendererCallbacks =
    { SubmitAction: SduiAction -> Async<Result<unit, DynamicHostError>> }

type TaRendererOptions =
    { MinimumVisibleBars: int
      DefaultVisibleBars: int
      MaximumVisibleBars: int }

type TaRendererUiState =
    { Window: TaVisibleWindow
      HiddenRows: Set<string>
      AddRowOpen: bool
      CursorIndex: int option
      Feedback: string }

[<JavaScript>]
module TaWorkspaceRenderer =
    let defaultOptions =
        { MinimumVisibleBars = 12
          DefaultVisibleBars = 48
          MaximumVisibleBars = 160 }

    let element name attrs (children: seq<#Doc>) =
        Doc.Element name attrs (children |> Seq.cast<Doc>) :> Doc

    let svgElement name attrs children =
        Doc.SvgElement name attrs children :> Doc

    let svgAttr name value = Attr.Create name value

    let fixedText (value: float) =
        string value

    let canvasIdText = function CanvasInstanceId value -> value

    let rowKindText = function
        | TaRowKind.Candlestick -> "Candlestick"
        | TaRowKind.Volume -> "Volume"
        | TaRowKind.Sma -> "SMA"
        | TaRowKind.Dmi -> "DMI"
        | TaRowKind.Adx -> "ADX"
        | TaRowKind.Macd -> "MACD"
        | TaRowKind.HeikinAshi -> "Heikin-Ashi"

    let freshnessText (freshness: TaFreshness) =
        match freshness with
        | TaFreshness.Live -> "LIVE"
        | TaFreshness.Delayed lag -> "DELAYED " + fixedText lag.TotalSeconds + "s"
        | TaFreshness.Stale(lag, reason) -> "STALE " + fixedText lag.TotalSeconds + "s / " + reason
        | TaFreshness.Backfill reason -> "BACKFILL / " + reason
        | TaFreshness.Unavailable reason -> "UNAVAILABLE / " + reason

    let freshnessClass (freshness: TaFreshness) =
        match freshness with
        | TaFreshness.Live -> "live"
        | TaFreshness.Delayed _
        | TaFreshness.Backfill _ -> "delayed"
        | TaFreshness.Stale _
        | TaFreshness.Unavailable _ -> "stale"

    let pollText = function
        | RuntimePollState.Unmounted -> "UNMOUNTED"
        | RuntimePollState.MountedIdle -> "MOUNTED"
        | RuntimePollState.Ready -> "READY"
        | RuntimePollState.PollInFlight -> "UPDATING"
        | RuntimePollState.Suspended -> "SUSPENDED"
        | RuntimePollState.PausedForResync -> "RESYNC"
        | RuntimePollState.Backoff _ -> "BACKOFF"
        | RuntimePollState.Disposed -> "DISPOSED"

    let remoteDisabled = function
        | RuntimePollState.PollInFlight
        | RuntimePollState.PausedForResync
        | RuntimePollState.Unmounted
        | RuntimePollState.Disposed -> true
        | _ -> false

    let submit (callbacks: TaRendererCallbacks) (uiState: Var<TaRendererUiState>) action successText =
        async {
            let! result = callbacks.SubmitAction action

            match result with
            | Ok () -> uiState.Value <- { uiState.Value with Feedback = successText }
            | Error error -> uiState.Value <- { uiState.Value with Feedback = error.Code + ": " + error.Message }
        }
        |> Async.StartImmediate

    let inputText (testId: string) (placeholder: string) (initial: string) (onChanged: string -> unit) =
        element "input" [
            Attr.Create "data-testid" testId
            attr.``type`` "text"
            attr.placeholder placeholder
            attr.value initial
            attr.style "height:30px; min-width:0; width:100%; border:1px solid #b9c6d8; border-radius:4px; background:#fff; color:#142033; padding:4px 7px; box-sizing:border-box; font-size:12px;"
            on.afterRender (fun node ->
                let input = node |> As<HTMLInputElement>
                input.AddEventListener("input", fun () -> onChanged input.Value))
        ] []

    let selectInput (testId: string) (initial: string) (values: (string * string) list) (onChanged: string -> unit) =
        element "select" [
            Attr.Create "data-testid" testId
            attr.style "height:30px; min-width:0; width:100%; border:1px solid #b9c6d8; border-radius:4px; background:#fff; color:#142033; padding:3px 6px; box-sizing:border-box; font-size:12px;"
            on.afterRender (fun node ->
                let input = node |> As<HTMLInputElement>
                input.Value <- initial
                input.AddEventListener("change", fun () -> onChanged input.Value))
        ] [
            for value, label in values do
                yield element "option" [ attr.value value ] [ text label ]
        ]

    let compactButton (testId: string) (label: string) (titleText: string) (onClick: unit -> unit) =
        button [
            attr.``type`` "button"
            Attr.Create "data-testid" testId
            attr.title titleText
            attr.style "height:30px; border:1px solid #9fb0c6; border-radius:4px; background:#f8fafc; color:#20344f; padding:3px 9px; font-size:12px; cursor:pointer; white-space:nowrap;"
            on.click (fun _ _ -> onClick ())
        ] [ text label ]

    let primaryButtonState (testId: string) (label: string) disabled (onClick: unit -> unit) =
        button [
            attr.``type`` "button"
            Attr.Create "data-testid" testId
            if disabled then attr.disabled "disabled"
            attr.style (if disabled then "height:30px; border:1px solid #9aa8b8; border-radius:4px; background:#d8e0e8; color:#667587; padding:3px 11px; font-size:12px; cursor:not-allowed; white-space:nowrap;" else "height:30px; border:1px solid #0f766e; border-radius:4px; background:#0f766e; color:#fff; padding:3px 11px; font-size:12px; cursor:pointer; white-space:nowrap;")
            on.click (fun _ _ -> if not disabled then onClick ())
        ] [ text label ]

    let primaryButton testId label onClick =
        primaryButtonState testId label false onClick

    let chartFrame titleText testId height children =
        section [
            Attr.Create "data-testid" testId
            attr.style ("display:flex; flex-direction:column; min-width:0; min-height:" + string height + "px; border-top:1px solid #e1e7ef; background:#fff;")
        ] [
            div [ attr.style "display:flex; align-items:center; justify-content:space-between; height:28px; padding:0 8px; color:#40536d; font-size:11px;" ] [
                strong [] [ text titleText ]
            ]
            element "div" [ attr.style "min-width:0; overflow:hidden;" ] children
        ]

    let cursorPosition width pointCount cursorIndex =
        cursorIndex
        |> Option.map (fun index ->
            let bounded = max 0 (min index (max 0 (pointCount - 1)))
            if pointCount <= 1 then width / 2.0 else width * float bounded / float (pointCount - 1))

    let compactTimestamp (value: string) =
        if String.IsNullOrWhiteSpace value then ""
        elif value.Length >= 16 && value[4] = '-' && value[7] = '-' && (value[10] = 'T' || value[10] = ' ') then
            value.Substring(5, 5) + " " + value.Substring(11, 5)
        else
            value

    let timeAxis testId (timestamps: string array) =
        RendererModel.timeLabels timestamps
        |> Array.mapi (fun position (_, label) ->
            let alignment = if position = 0 then "left" elif position = 2 then "right" else "center"
            span [ attr.style ("min-width:0; text-align:" + alignment + "; color:#708198; font-size:10px; line-height:16px; white-space:nowrap; overflow:hidden; text-overflow:ellipsis;") ] [ text (compactTimestamp label) ] :> Doc)
        |> fun labels -> div [ Attr.Create "data-testid" testId; attr.style "display:grid; grid-template-columns:1fr 1fr 1fr; min-width:0; height:16px; padding:0 1px;" ] labels

    let candleSvg testId (points: TaCandlePoint array) cursorIndex =
        let width = 1000.0
        let height = 250.0
        let top = 12.0
        let plotHeight = 214.0
        let lows = points |> Array.map _.Low
        let highs = points |> Array.map _.High
        let low, high = RendererModel.paddedRange 0.0 1.0 (Array.append lows highs)
        let slot = if points.Length = 0 then width else width / float points.Length
        let bodyWidth = max 2.0 (slot * 0.56)

        svgElement "svg" [
            svgAttr "viewBox" "0 0 1000 250"
            svgAttr "preserveAspectRatio" "none"
            svgAttr "role" "img"
            svgAttr "aria-label" "Candlestick chart"
            Attr.Create "data-testid" testId
            attr.style "display:block; width:100%; height:250px; background:#fbfcfe;"
        ] [
            for gridIndex in 0 .. 4 do
                let y = top + plotHeight * float gridIndex / 4.0
                yield svgElement "line" [ svgAttr "x1" "0"; svgAttr "x2" "1000"; svgAttr "y1" (fixedText y); svgAttr "y2" (fixedText y); svgAttr "stroke" "#e7ecf3"; svgAttr "stroke-width" "1" ] []

            for index in 0 .. points.Length - 1 do
                let point = points[index]
                let x = slot * (float index + 0.5)
                let openY = RendererModel.normalize low high top plotHeight point.Open
                let closeY = RendererModel.normalize low high top plotHeight point.Close
                let highY = RendererModel.normalize low high top plotHeight point.High
                let lowY = RendererModel.normalize low high top plotHeight point.Low
                let rising = point.Close >= point.Open
                let color = if rising then "#0f8a78" else "#c2414b"
                let bodyY = min openY closeY
                let bodyHeight = max 1.2 (abs (closeY - openY))
                yield svgElement "line" [ svgAttr "x1" (fixedText x); svgAttr "x2" (fixedText x); svgAttr "y1" (fixedText highY); svgAttr "y2" (fixedText lowY); svgAttr "stroke" color; svgAttr "stroke-width" "1.4" ] []
                yield svgElement "rect" [ svgAttr "x" (fixedText (x - bodyWidth / 2.0)); svgAttr "y" (fixedText bodyY); svgAttr "width" (fixedText bodyWidth); svgAttr "height" (fixedText bodyHeight); svgAttr "fill" color; svgAttr "rx" "0.6" ] []

            match cursorPosition width points.Length cursorIndex with
            | Some x ->
                yield svgElement "line" [ Attr.Create "data-testid" (testId + "-crosshair"); svgAttr "x1" (fixedText x); svgAttr "x2" (fixedText x); svgAttr "y1" "0"; svgAttr "y2" "226"; svgAttr "stroke" "#1f4f73"; svgAttr "stroke-width" "1"; svgAttr "stroke-dasharray" "3 3" ] []
            | None -> ()

        ]

    let lineSvg testId color (points: TaLinePoint array) cursorIndex =
        let width = 1000.0
        let height = 112.0
        let top = 10.0
        let plotHeight = 82.0
        let low, high = points |> Array.map _.Value |> RendererModel.paddedRange 0.0 1.0
        let step = if points.Length <= 1 then width else width / float (points.Length - 1)
        let path =
            points
            |> Array.mapi (fun index point ->
                let command = if index = 0 then "M" else "L"
                command + " " + fixedText (float index * step) + " " + fixedText (RendererModel.normalize low high top plotHeight point.Value))
            |> String.concat " "

        svgElement "svg" [
            svgAttr "viewBox" "0 0 1000 112"
            svgAttr "preserveAspectRatio" "none"
            Attr.Create "data-testid" testId
            attr.style "display:block; width:100%; height:112px; background:#fbfcfe;"
        ] [
            yield svgElement "line" [ svgAttr "x1" "0"; svgAttr "x2" "1000"; svgAttr "y1" "51"; svgAttr "y2" "51"; svgAttr "stroke" "#e7ecf3"; svgAttr "stroke-width" "1" ] []
            yield svgElement "path" [ svgAttr "d" path; svgAttr "fill" "none"; svgAttr "stroke" color; svgAttr "stroke-width" "2"; svgAttr "stroke-linejoin" "round"; svgAttr "stroke-linecap" "round" ] []
            match cursorPosition width points.Length cursorIndex with
            | Some x -> yield svgElement "line" [ Attr.Create "data-testid" (testId + "-crosshair"); svgAttr "x1" (fixedText x); svgAttr "x2" (fixedText x); svgAttr "y1" "0"; svgAttr "y2" "92"; svgAttr "stroke" "#1f4f73"; svgAttr "stroke-width" "1"; svgAttr "stroke-dasharray" "3 3" ] []
            | None -> ()
        ]

    let volumeSvg (points: TaCandlePoint array) cursorIndex =
        let width = 1000.0
        let height = 100.0
        let maximum = points |> Array.map _.Volume |> Array.fold max 1.0
        let slot = if points.Length = 0 then width else width / float points.Length

        svgElement "svg" [
            svgAttr "viewBox" "0 0 1000 100"
            svgAttr "preserveAspectRatio" "none"
            Attr.Create "data-testid" "ta-volume-svg"
            attr.style "display:block; width:100%; height:100px; background:#fbfcfe;"
        ] [
            for index in 0 .. points.Length - 1 do
                let point = points[index]
                let barHeight = max 1.0 (point.Volume / maximum * 86.0)
                let color = if point.Close >= point.Open then "#6bb5a9" else "#d4868d"
                yield svgElement "rect" [ svgAttr "x" (fixedText (slot * float index + slot * 0.18)); svgAttr "y" (fixedText (94.0 - barHeight)); svgAttr "width" (fixedText (max 1.0 (slot * 0.64))); svgAttr "height" (fixedText barHeight); svgAttr "fill" color ] []

            match cursorPosition width points.Length cursorIndex with
            | Some x -> yield svgElement "line" [ Attr.Create "data-testid" "ta-volume-crosshair"; svgAttr "x1" (fixedText x); svgAttr "x2" (fixedText x); svgAttr "y1" "0"; svgAttr "y2" "100"; svgAttr "stroke" "#1f4f73"; svgAttr "stroke-width" "1"; svgAttr "stroke-dasharray" "3 3" ] []
            | None -> ()
        ]

    let compositeSvg rowId (traces: TaTraceSpec array) data window cursorIndex =
        let width = 1000.0
        let hasCandles = traces |> Array.exists (fun trace -> trace.Kind = TaTraceKind.Candlestick)
        let height = if hasCandles then 250.0 else 112.0
        let top = 10.0
        let plotHeight = if hasCandles then 214.0 else 82.0
        let palette = [| "#2764b0"; "#9b5b24"; "#6a4ca3"; "#0f766e"; "#b45309"; "#be185d"; "#475569"; "#0891b2" |]
        let color index (trace: TaTraceSpec) =
            if String.IsNullOrWhiteSpace trace.Color then palette[index % palette.Length] else trace.Color

        let candleSeries =
            traces
            |> Array.tryFind (fun trace -> trace.Kind = TaTraceKind.Candlestick)
            |> Option.map (fun trace -> RendererModel.candleSeries trace.DataRef data |> RendererModel.selectWindow window)
            |> Option.defaultValue [||]

        let referenceTimestamps =
            if candleSeries.Length > 0 then candleSeries |> Array.map _.Timestamp
            else
                traces
                |> Array.tryPick (fun trace ->
                    match trace.Kind with
                    | TaTraceKind.Volume ->
                        let points = RendererModel.candleSeries trace.DataRef data |> RendererModel.selectWindow window
                        if points.Length = 0 then None else Some(points |> Array.map _.Timestamp)
                    | TaTraceKind.Line
                    | TaTraceKind.Histogram ->
                        let points = RendererModel.lineSeries trace.DataRef data |> RendererModel.selectWindow window
                        if points.Length = 0 then None else Some(points |> Array.map _.Timestamp)
                    | _ -> None)
                |> Option.defaultValue [||]

        let timestampIndex timestamp = referenceTimestamps |> Array.tryFindIndex ((=) timestamp)
        let xAt index =
            if referenceTimestamps.Length <= 1 then width / 2.0
            else width * float index / float (referenceTimestamps.Length - 1)

        let linePoints =
            traces
            |> Array.mapi (fun index trace ->
                let points =
                    (match trace.Kind with
                     | TaTraceKind.Volume ->
                         RendererModel.candleSeries trace.DataRef data
                         |> Array.map (fun point -> { Timestamp = point.Timestamp; Value = point.Volume })
                     | TaTraceKind.Line
                     | TaTraceKind.Histogram -> RendererModel.lineSeries trace.DataRef data
                     | _ -> [||])
                    |> Array.filter (fun point -> timestampIndex point.Timestamp |> Option.isSome)
                index, trace, points)

        let scaleValues =
            [| yield! candleSeries |> Array.collect (fun point -> [| point.Low; point.High |])
               yield! linePoints |> Array.collect (fun (_, trace, points) ->
                   let values = points |> Array.map _.Value
                   if trace.Kind = TaTraceKind.Histogram then Array.append [| 0.0 |] values else values) |]
        let low, high = RendererModel.paddedRange 0.0 1.0 scaleValues
        let slot = if referenceTimestamps.Length = 0 then width else width / float referenceTimestamps.Length
        let bodyWidth = max 2.0 (slot * 0.56)
        let svgTestId = if hasCandles then "ta-candle-" + rowId else "ta-composite-" + rowId

        svgElement "svg" [
            svgAttr "viewBox" (if hasCandles then "0 0 1000 250" else "0 0 1000 112")
            svgAttr "preserveAspectRatio" "none"
            svgAttr "role" "img"
            svgAttr "aria-label" ("Composite TA row " + rowId)
            Attr.Create "data-testid" svgTestId
            attr.style ("display:block; width:100%; height:" + fixedText height + "px; background:#fbfcfe;")
        ] [
            for gridIndex in 0 .. 4 do
                let y = top + plotHeight * float gridIndex / 4.0
                yield svgElement "line" [ svgAttr "x1" "0"; svgAttr "x2" "1000"; svgAttr "y1" (fixedText y); svgAttr "y2" (fixedText y); svgAttr "stroke" "#e7ecf3"; svgAttr "stroke-width" "1" ] []

            for index in 0 .. candleSeries.Length - 1 do
                let point = candleSeries[index]
                let x = slot * (float index + 0.5)
                let openY = RendererModel.normalize low high top plotHeight point.Open
                let closeY = RendererModel.normalize low high top plotHeight point.Close
                let highY = RendererModel.normalize low high top plotHeight point.High
                let lowY = RendererModel.normalize low high top plotHeight point.Low
                let candleColor = if point.Close >= point.Open then "#0f8a78" else "#c2414b"
                let bodyY = min openY closeY
                let bodyHeight = max 1.2 (abs (closeY - openY))
                yield svgElement "line" [ svgAttr "x1" (fixedText x); svgAttr "x2" (fixedText x); svgAttr "y1" (fixedText highY); svgAttr "y2" (fixedText lowY); svgAttr "stroke" candleColor; svgAttr "stroke-width" "1.4" ] []
                yield svgElement "rect" [ svgAttr "x" (fixedText (x - bodyWidth / 2.0)); svgAttr "y" (fixedText bodyY); svgAttr "width" (fixedText bodyWidth); svgAttr "height" (fixedText bodyHeight); svgAttr "fill" candleColor; svgAttr "rx" "0.6" ] []

            for traceIndex, trace, points in linePoints do
                let traceColor = color traceIndex trace
                match trace.Kind with
                | TaTraceKind.Histogram
                | TaTraceKind.Volume ->
                    let zeroY = RendererModel.normalize low high top plotHeight 0.0
                    for point in points do
                        match timestampIndex point.Timestamp with
                        | Some index ->
                            let x = slot * (float index + 0.18)
                            let valueY = RendererModel.normalize low high top plotHeight point.Value
                            let y = min zeroY valueY
                            let barHeight = max 1.0 (abs (zeroY - valueY))
                            yield svgElement "rect" [ Attr.Create "data-testid" ("ta-trace-" + rowId + "-" + trace.TraceId); svgAttr "x" (fixedText x); svgAttr "y" (fixedText y); svgAttr "width" (fixedText (max 1.0 (slot * 0.64))); svgAttr "height" (fixedText barHeight); svgAttr "fill" traceColor; svgAttr "fill-opacity" "0.62" ] []
                        | None -> ()
                | TaTraceKind.Line ->
                    let path =
                        points
                        |> Array.choose (fun point ->
                            timestampIndex point.Timestamp
                            |> Option.map (fun index -> xAt index, RendererModel.normalize low high top plotHeight point.Value))
                        |> Array.mapi (fun index (x, y) -> (if index = 0 then "M" else "L") + " " + fixedText x + " " + fixedText y)
                        |> String.concat " "
                    yield svgElement "path" [ Attr.Create "data-testid" ("ta-trace-" + rowId + "-" + trace.TraceId); svgAttr "d" path; svgAttr "fill" "none"; svgAttr "stroke" traceColor; svgAttr "stroke-width" (fixedText trace.Width); svgAttr "stroke-linejoin" "round"; svgAttr "stroke-linecap" "round" ] []
                | _ -> ()

            match cursorPosition width referenceTimestamps.Length cursorIndex with
            | Some x -> yield svgElement "line" [ Attr.Create "data-testid" (svgTestId + "-crosshair"); svgAttr "x1" (fixedText x); svgAttr "x2" (fixedText x); svgAttr "y1" "0"; svgAttr "y2" (fixedText plotHeight); svgAttr "stroke" "#1f4f73"; svgAttr "stroke-width" "1"; svgAttr "stroke-dasharray" "3 3" ] []
            | None -> ()
        ], referenceTimestamps

    let renderRow (options: TaRendererOptions) (state: RuntimeState) (ui: TaRendererUiState) (row: TaRowSpec) =
        let traces = RendererModel.effectiveTraces row |> Array.filter _.Visible
        let referenceLength = RendererModel.rowReferenceLength row state.Data
        let window = RendererModel.clampWindow options.MinimumVisibleBars options.MaximumVisibleBars referenceLength ui.Window
        let chart, timestamps = compositeSvg row.RowId traces state.Data window ui.CursorIndex
        let title =
            if isNull row.Traces || row.Traces.Length = 0 then
                rowKindText row.Kind
            else
                traces
                |> Array.map (fun trace -> if String.IsNullOrWhiteSpace trace.Label then trace.TraceId else trace.Label)
                |> String.concat " / "
                |> fun value -> if String.IsNullOrWhiteSpace value then rowKindText row.Kind else value
        let height = if traces |> Array.exists (fun trace -> trace.Kind = TaTraceKind.Candlestick) then 278 else 140
        chartFrame title ("ta-row-" + row.RowId) height [ chart; timeAxis ("ta-time-axis-" + row.RowId) timestamps ]

    let render (options: TaRendererOptions) (callbacks: TaRendererCallbacks) (runtimeState: Var<RuntimeState>) =
        let canvasId = runtimeState.Value.Identity.CanvasInstanceId
        let instrument = Var.Create ""
        let interval = Var.Create ""
        let fromDate = Var.Create ""
        let toDate = Var.Create ""
        let mutable synchronizedDocumentRevision = -1L
        let addKind = Var.Create "Sma"
        let addDataRef = Var.Create "series.sma"
        let uiState =
            Var.Create
                { Window = { StartIndex = 0; Count = options.DefaultVisibleBars }
                  HiddenRows = Set.empty
                  AddRowOpen = false
                  CursorIndex = Some(options.DefaultVisibleBars - 1)
                  Feedback = "" }

        let updateWindow transform =
            let current = uiState.Value
            uiState.Value <- { current with Window = transform current.Window }

        let applyQuery () =
            let parsedInterval =
                match Int32.TryParse interval.Value with
                | true, value when value > 0 -> Some value
                | _ -> None

            let query =
                { SourceId = None
                  Instrument = if String.IsNullOrWhiteSpace instrument.Value then None else Some instrument.Value
                  IntervalMinutes = parsedInterval
                  FromUtc = if String.IsNullOrWhiteSpace fromDate.Value then None else Some fromDate.Value
                  ToUtcExclusive = if String.IsNullOrWhiteSpace toDate.Value then None else Some toDate.Value
                  IncludePartial = Some true }

            submit callbacks uiState (SduiAction.ChangeTaQuery(canvasId, query)) "Query submitted."

        let addRow () =
            let kind =
                match addKind.Value with
                | "Volume" -> TaRowKind.Volume
                | "Dmi" -> TaRowKind.Dmi
                | "Adx" -> TaRowKind.Adx
                | "Macd" -> TaRowKind.Macd
                | "HeikinAshi" -> TaRowKind.HeikinAshi
                | _ -> TaRowKind.Sma

            let rowId = "row-" + addKind.Value.ToLower()
            let spec =
                { RowId = rowId
                  Kind = kind
                  DataRef = addDataRef.Value
                  HeightWeight = 1.0
                  Visible = true
                  Options = Map.empty
                  Traces = [||] }

            submit callbacks uiState (SduiAction.AddTaRow(canvasId, spec)) "Row request submitted."
            uiState.Value <- { uiState.Value with AddRowOpen = false }

        div [
            attr.``class`` "ptcs-ta-workspace"
            Attr.Create "data-testid" "ta-workspace"
            attr.style "display:flex; flex-direction:column; min-width:0; width:100%; min-height:640px; color:#142033; background:#f4f7fb; font-family:Segoe UI, Arial, sans-serif; letter-spacing:0;"
        ] [
            runtimeState.View
            |> View.Map (fun state ->
                match state.Document with
                | None ->
                    let pending = RendererModel.workspaceBootstrapPresentation state
                    let color = if pending.IsError then "#9a2f2f" else "#5d6d83"

                    div [
                        Attr.Create "data-testid" "ta-workspace-bootstrap"
                        Attr.Create "data-state" pending.State
                        attr.style ("display:flex; flex-direction:column; gap:4px; padding:18px; color:" + color + ";")
                    ] [
                        strong [] [ text pending.Title ]
                        span [ attr.style "font-size:12px;" ] [ text pending.Detail ]
                    ] :> Doc
                | Some document ->
                    if state.DocumentRevision <> synchronizedDocumentRevision then
                        let query = RendererModel.queryDraft document.DefaultView
                        instrument.Value <- query.Instrument
                        interval.Value <- query.IntervalMinutes
                        fromDate.Value <- query.FromUtc
                        toDate.Value <- query.ToUtcExclusive
                        synchronizedDocumentRevision <- state.DocumentRevision

                    let status = RendererModel.statusPresentation document.StatusRef state
                    let commandsDisabled = remoteDisabled state.Poll

                    div [ attr.style "display:flex; flex-direction:column; min-width:0;" ] [
                        header [ attr.style "display:flex; flex-direction:column; gap:7px; padding:10px 12px 8px; background:#fff; border-bottom:1px solid #dbe3ee;" ] [
                            div [ attr.style "display:flex; align-items:center; justify-content:space-between; gap:10px; flex-wrap:wrap;" ] [
                                div [ attr.style "min-width:0;" ] [
                                    h2 [ Attr.Create "data-testid" "ta-workspace-title"; attr.style "margin:0; font-size:17px; line-height:22px; font-weight:700; color:#152944;" ] [ text document.Title ]
                                    div [ attr.style "font-size:11px; color:#667891; white-space:nowrap; overflow:hidden; text-overflow:ellipsis;" ] [ text ("canvas " + canvasIdText canvasId + " / revision " + string state.DataRevision) ]
                                ]
                                div [ attr.style "display:flex; align-items:center; gap:5px; flex-wrap:wrap; justify-content:flex-end;" ] [
                                    div [ Attr.Create "data-testid" "ta-freshness"; Attr.Create "data-freshness" (freshnessClass status.Freshness); attr.style "border:1px solid #9fb0c6; border-radius:4px; padding:3px 7px; font-size:11px; font-weight:650; color:#27415f; background:#f8fafc;" ] [
                                        text status.Label
                                    ]
                                    div [ Attr.Create "data-testid" "ta-poll-state"; Attr.Create "data-poll-state" (pollText state.Poll); attr.style "border:1px solid #c3cfdd; border-radius:4px; padding:3px 7px; font-size:10px; color:#53667d; background:#fff;" ] [
                                        text (pollText state.Poll)
                                    ]
                                ]
                            ]
                            div [ Attr.Create "data-testid" "ta-status-detail"; attr.style "display:flex; gap:10px; flex-wrap:wrap; min-height:16px; font-size:10px; color:#60738b;" ] [
                                match status.Watermark with
                                | Some value -> yield span [] [ text ("watermark " + value) ]
                                | None -> ()
                                match status.Quality with
                                | Some value -> yield span [] [ text ("quality " + value) ]
                                | None -> ()
                                match status.Error with
                                | Some value -> yield span [ Attr.Create "data-testid" "ta-last-good-error"; attr.style "color:#a33b43; font-weight:600;" ] [ text value ]
                                | None -> ()
                            ]
                            div [ Attr.Create "data-testid" "ta-query-toolbar"; attr.style "display:grid; grid-template-columns:repeat(auto-fit,minmax(120px,1fr)); gap:6px; align-items:end;" ] [
                                label [ attr.style "display:flex; flex-direction:column; gap:2px; min-width:0; font-size:10px; color:#60738b;" ] [ text "Instrument"; inputText "ta-instrument" "Instrument" instrument.Value (fun value -> instrument.Value <- value) ]
                                label [ attr.style "display:flex; flex-direction:column; gap:2px; min-width:0; font-size:10px; color:#60738b;" ] [ text "Interval"; selectInput "ta-interval" interval.Value [ "1", "1m"; "5", "5m"; "30", "30m"; "60", "60m"; "930", "Session" ] (fun value -> interval.Value <- value) ]
                                label [ attr.style "display:flex; flex-direction:column; gap:2px; min-width:0; font-size:10px; color:#60738b;" ] [ text "From"; inputText "ta-from" "YYYY-MM-DD" fromDate.Value (fun value -> fromDate.Value <- value) ]
                                label [ attr.style "display:flex; flex-direction:column; gap:2px; min-width:0; font-size:10px; color:#60738b;" ] [ text "To"; inputText "ta-to" "YYYY-MM-DD" toDate.Value (fun value -> toDate.Value <- value) ]
                                primaryButtonState "ta-apply-query" "Load / Apply" commandsDisabled applyQuery
                            ]
                            div [ Attr.Create "data-testid" "ta-local-toolbar"; attr.style "display:flex; align-items:center; gap:5px; flex-wrap:wrap;" ] [
                                compactButton "ta-pan-left" "←" "Pan earlier" (fun () -> updateWindow (fun window -> { window with StartIndex = max 0 (window.StartIndex - max 1 (window.Count / 4)) }))
                                compactButton "ta-pan-right" "→" "Pan later" (fun () -> updateWindow (fun window -> { window with StartIndex = window.StartIndex + max 1 (window.Count / 4) }))
                                compactButton "ta-zoom-in" "+" "Show fewer bars" (fun () -> updateWindow (fun window -> { window with Count = max options.MinimumVisibleBars (window.Count - 8) }))
                                compactButton "ta-zoom-out" "−" "Show more bars" (fun () -> updateWindow (fun window -> { window with Count = min options.MaximumVisibleBars (window.Count + 8) }))
                                compactButton "ta-reset-view" "Reset View" "Reset local viewport only" (fun () -> uiState.Value <- { uiState.Value with Window = { StartIndex = 0; Count = options.DefaultVisibleBars }; Feedback = "Local view reset." })
                                compactButton "ta-reset-canvas" "Reset Canvas" "Request server canvas reset" (fun () -> submit callbacks uiState (SduiAction.ResetCanvas canvasId) "Canvas reset requested.")
                                compactButton "ta-add-row-toggle" "Add Row" "Open row request editor" (fun () -> uiState.Value <- { uiState.Value with AddRowOpen = not uiState.Value.AddRowOpen })
                                span [ attr.style "margin-left:auto; color:#60738b; font-size:11px;" ] [ text "local view controls do not query the backend" ]
                            ]
                            uiState.View
                            |> View.Map (fun ui ->
                                div [ Attr.Create "data-testid" "ta-row-toggles"; attr.style "display:flex; align-items:center; gap:5px; flex-wrap:wrap;" ] [
                                    for row in document.Rows do
                                        let hidden = Set.contains row.RowId ui.HiddenRows
                                        yield
                                            div [ attr.style "display:inline-flex; align-items:stretch; height:26px;" ] [
                                                button [
                                                    attr.``type`` "button"
                                                    Attr.Create "data-testid" ("ta-toggle-row-" + row.RowId)
                                                    Attr.Create "aria-pressed" (if hidden then "false" else "true")
                                                    attr.style (if hidden then "height:26px; border:1px solid #c8d2df; border-right:0; border-radius:4px 0 0 4px; background:#fff; color:#7a8798; padding:2px 7px; font-size:11px; cursor:pointer;" else "height:26px; border:1px solid #7da39d; border-right:0; border-radius:4px 0 0 4px; background:#edf8f6; color:#155d55; padding:2px 7px; font-size:11px; cursor:pointer;")
                                                    on.click (fun _ _ ->
                                                        let nextHidden =
                                                            if hidden then Set.remove row.RowId uiState.Value.HiddenRows
                                                            else Set.add row.RowId uiState.Value.HiddenRows

                                                        uiState.Value <- { uiState.Value with HiddenRows = nextHidden })
                                                ] [ text (rowKindText row.Kind) ]
                                                button [
                                                    attr.``type`` "button"
                                                    Attr.Create "data-testid" ("ta-remove-row-" + row.RowId)
                                                    attr.title ("Remove " + rowKindText row.Kind + " row")
                                                    if commandsDisabled then attr.disabled "disabled"
                                                    attr.style (if commandsDisabled then "width:26px; height:26px; border:1px solid #c8d2df; border-radius:0 4px 4px 0; background:#edf1f5; color:#8b98a8; padding:0; font-size:14px; cursor:not-allowed;" else "width:26px; height:26px; border:1px solid #c8a7ab; border-radius:0 4px 4px 0; background:#fff; color:#8d3039; padding:0; font-size:14px; cursor:pointer;")
                                                    on.click (fun _ _ ->
                                                        if not commandsDisabled then
                                                            submit callbacks uiState (SduiAction.RemoveTaRow(canvasId, row.RowId)) (rowKindText row.Kind + " row removal requested."))
                                                ] [ text "×" ]
                                            ]
                                ] :> Doc)
                            |> Doc.EmbedView
                            uiState.View
                            |> View.Map (fun ui ->
                                if not ui.AddRowOpen then Doc.Empty
                                else
                                    div [ Attr.Create "data-testid" "ta-add-row-editor"; attr.style "display:grid; grid-template-columns:repeat(auto-fit,minmax(120px,1fr)); gap:6px; align-items:end; padding:7px; border:1px solid #cbd6e5; border-radius:5px; background:#f8fafc;" ] [
                                        label [ attr.style "display:flex; flex-direction:column; gap:2px; font-size:10px; color:#60738b;" ] [ text "Row kind"; selectInput "ta-add-row-kind" addKind.Value [ "Sma", "SMA"; "Volume", "Volume"; "Dmi", "DMI"; "Adx", "ADX"; "Macd", "MACD"; "HeikinAshi", "Heikin-Ashi" ] (fun value -> addKind.Value <- value) ]
                                        label [ attr.style "display:flex; flex-direction:column; gap:2px; font-size:10px; color:#60738b;" ] [ text "Data ref"; inputText "ta-add-row-data-ref" "series.sma" addDataRef.Value (fun value -> addDataRef.Value <- value) ]
                                        compactButton "ta-add-row-cancel" "Cancel" "Close without submitting" (fun () -> uiState.Value <- { uiState.Value with AddRowOpen = false })
                                        primaryButtonState "ta-add-row-submit" "Add" commandsDisabled addRow
                                    ] :> Doc)
                            |> Doc.EmbedView
                            uiState.View
                            |> View.Map (fun ui ->
                                if String.IsNullOrWhiteSpace ui.Feedback then Doc.Empty
                                else div [ Attr.Create "data-testid" "ta-feedback"; attr.style "font-size:11px; color:#40536d; min-height:15px;" ] [ text ui.Feedback ] :> Doc)
                            |> Doc.EmbedView
                        ]
                        uiState.View
                        |> View.Map (fun ui ->
                            let visibleRows =
                                document.Rows
                                |> Array.filter (fun row -> row.Visible && not (Set.contains row.RowId ui.HiddenRows))

                            let referenceLength =
                                visibleRows
                                |> Array.tryHead
                                |> Option.map (fun row -> RendererModel.seriesValues row.DataRef state.Data |> Array.length)
                                |> Option.defaultValue 0
                            let visibleWindow = RendererModel.clampWindow options.MinimumVisibleBars options.MaximumVisibleBars referenceLength ui.Window
                            let cursorIndex = ui.CursorIndex |> Option.defaultValue (max 0 (visibleWindow.Count - 1)) |> max 0 |> min (max 0 (visibleWindow.Count - 1))
                            let cursor = RendererModel.cursorSnapshot document state.Data visibleWindow cursorIndex
                            let cursorValues =
                                match cursor with
                                | None -> div [ attr.style "font-size:11px; color:#718197;" ] [ text "No cursor data." ]
                                | Some value ->
                                    div [ Attr.Create "data-testid" "ta-cursor-values"; attr.style "display:flex; align-items:center; gap:4px 12px; min-width:0; flex-wrap:wrap; white-space:normal; overflow-wrap:anywhere; font-family:Consolas, monospace; font-size:11px; line-height:16px; color:#263b55;" ] [
                                        yield strong [ attr.style "white-space:nowrap;" ] [ text (compactTimestamp value.Timestamp) ]
                                        for item in value.Values do
                                            yield span [ Attr.Create "data-cursor-row" item.Label; attr.style "min-width:0;" ] [ text (item.Label + " " + item.Value) ]
                                    ]

                            div [ Attr.Create "data-testid" "ta-chart-stack"; attr.style "display:flex; flex-direction:column; min-width:0; padding:0 12px 14px;" ] [
                                yield div [ Attr.Create "data-testid" "ta-cursor-panel"; attr.style "display:flex; flex-direction:column; gap:5px; align-items:stretch; min-height:50px; padding:6px 8px; border-bottom:1px solid #dce4ef; background:#f8fafc;" ] [
                                    yield element "input" [
                                        Attr.Create "data-testid" "ta-cursor-slider"
                                        attr.``type`` "range"
                                        Attr.Create "min" "0"
                                        Attr.Create "max" (string (max 0 (visibleWindow.Count - 1)))
                                        attr.value (string cursorIndex)
                                        attr.style "width:100%; min-width:0; accent-color:#1f6f78;"
                                        on.afterRender (fun node ->
                                            let input = node |> As<HTMLInputElement>
                                            input.AddEventListener("input", fun () ->
                                                match Int32.TryParse input.Value with
                                                | true, value -> uiState.Value <- { uiState.Value with CursorIndex = Some value }
                                                | _ -> ()))
                                    ] []
                                    yield cursorValues
                                ]
                                if visibleRows.Length = 0 then
                                    yield div [ attr.style "padding:18px; color:#667891;" ] [ text "No visible TA rows." ]
                                else
                                    for row in visibleRows do
                                        yield renderRow options state ui row
                            ] :> Doc)
                        |> Doc.EmbedView
                    ] :> Doc)
            |> Doc.EmbedView
        ]
