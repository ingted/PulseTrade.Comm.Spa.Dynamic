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
      FollowLatest: bool
      HiddenRows: Set<string>
      AddRowOpen: bool
      CursorIndex: int option
      Feedback: string }

[<JavaScript>]
module TaWorkspaceRenderer =
    let defaultOptions =
        { MinimumVisibleBars = 12
          DefaultVisibleBars = 48
          MaximumVisibleBars = 2000 }

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
                // WebSharper's DOM wrapper exposes the shared value property through HTMLInputElement.
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
        |> Option.bind (RendererModel.slotCenter width pointCount)

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

    let rectanglePath x y width height =
        "M " + fixedText x + " " + fixedText y
        + " h " + fixedText width
        + " v " + fixedText height
        + " h " + fixedText (-width)
        + " Z"

    let overviewSvg points selectionWindow onReady onDragStart =
        let width = 1000.0
        let height = 82.0
        let sampled = RendererModel.sampleEvenly 280 points
        let low, high =
            sampled
            |> Array.collect (fun point -> [| point.Low; point.High |])
            |> RendererModel.paddedRange 0.0 1.0
        let xAt index =
            if sampled.Length <= 1 then width / 2.0
            else width * float index / float (sampled.Length - 1)
        let closePath =
            sampled
            |> Array.mapi (fun index point ->
                (if index = 0 then "M " else "L ")
                + fixedText (xAt index) + " "
                + fixedText (RendererModel.normalize low high 8.0 62.0 point.Close))
            |> String.concat " "
        let leftRatio, rightRatio = selectionWindow
        let selectionX = leftRatio * width
        let selectionWidth = max 4.0 ((rightRatio - leftRatio) * width)
        let handleWidth = 8.0
        let handleX edge = max 0.0 (min (width - handleWidth) (edge - handleWidth / 2.0))

        svgElement "svg" [
            Attr.Create "data-testid" "ta-overview-navigator"
            Attr.Create "data-loaded-sample-count" (string sampled.Length)
            svgAttr "viewBox" "0 0 1000 82"
            svgAttr "preserveAspectRatio" "none"
            attr.style "display:block; width:100%; height:82px; min-width:0; background:#eef3f8; border:1px solid #c7d3e2; border-radius:4px; box-sizing:border-box; touch-action:none;"
            on.afterRender onReady
        ] [
            svgElement "path" [ svgAttr "d" closePath; svgAttr "fill" "none"; svgAttr "stroke" "#3d718e"; svgAttr "stroke-width" "1.5" ] []
            svgElement "rect" [
                Attr.Create "data-testid" "ta-overview-selection"
                svgAttr "x" (fixedText selectionX); svgAttr "y" "1"; svgAttr "width" (fixedText selectionWidth); svgAttr "height" "80"
                svgAttr "fill" "rgba(15,118,110,.10)"; svgAttr "stroke" "#0f766e"; svgAttr "stroke-width" "2"; svgAttr "style" "cursor:grab;"
                on.mouseDown (fun _ event -> onDragStart TaWindowDrag.Move event)
            ] []
            svgElement "rect" [
                Attr.Create "data-testid" "ta-overview-left-handle"
                svgAttr "x" (fixedText (handleX selectionX)); svgAttr "y" "0"; svgAttr "width" (fixedText handleWidth); svgAttr "height" "82"
                svgAttr "fill" "#155f73"; svgAttr "fill-opacity" "0.82"; svgAttr "style" "cursor:ew-resize;"
                on.mouseDown (fun _ event -> onDragStart TaWindowDrag.ResizeLeft event)
            ] []
            svgElement "rect" [
                Attr.Create "data-testid" "ta-overview-right-handle"
                svgAttr "x" (fixedText (handleX (selectionX + selectionWidth))); svgAttr "y" "0"; svgAttr "width" (fixedText handleWidth); svgAttr "height" "82"
                svgAttr "fill" "#155f73"; svgAttr "fill-opacity" "0.82"; svgAttr "style" "cursor:ew-resize;"
                on.mouseDown (fun _ event -> onDragStart TaWindowDrag.ResizeRight event)
            ] []
        ]

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

    let compositeSvg rowId (traces: TaTraceSpec array) data (referenceTimestamps: string array) cursorIndex setCursorIndex =
        let width = 1000.0
        let hasCandles = traces |> Array.exists (fun trace -> trace.Kind = TaTraceKind.Candlestick)
        let height = if hasCandles then 250.0 else 112.0
        let top = 10.0
        let plotHeight = if hasCandles then 214.0 else 82.0
        let palette = [| "#2764b0"; "#9b5b24"; "#6a4ca3"; "#0f766e"; "#b45309"; "#be185d"; "#475569"; "#0891b2" |]
        let color index (trace: TaTraceSpec) =
            if String.IsNullOrWhiteSpace trace.Color then palette[index % palette.Length] else trace.Color

        let timestampIndex =
            referenceTimestamps
            |> Array.mapi (fun index timestamp -> timestamp, index)
            |> Map.ofArray

        let containsTimestamp timestamp = timestampIndex |> Map.containsKey timestamp
        let tryTimestampIndex timestamp = timestampIndex |> Map.tryFind timestamp

        let candleSeries =
            traces
            |> Array.tryFind (fun trace -> trace.Kind = TaTraceKind.Candlestick)
            |> Option.map (fun trace ->
                RendererModel.candleSeries trace.DataRef data
                |> Array.filter (fun point -> containsTimestamp point.Timestamp))
            |> Option.defaultValue [||]
        let xAt index =
            RendererModel.slotCenter width referenceTimestamps.Length index
            |> Option.defaultValue (width / 2.0)

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
                    |> Array.filter (fun point -> containsTimestamp point.Timestamp)
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
            Attr.Create "data-point-count" (string referenceTimestamps.Length)
            Attr.Create "data-cursor-index" (cursorIndex |> Option.map string |> Option.defaultValue "")
            attr.style ("display:block; width:100%; height:" + fixedText height + "px; background:#fbfcfe;")
            on.mouseMove (fun element event ->
                let bounds = element.GetBoundingClientRect()
                match RendererModel.cursorIndexFromClientX referenceTimestamps.Length bounds.Left bounds.Width event.ClientX with
                | Some index -> setCursorIndex (Some index)
                | None -> ())
        ] [
            for gridIndex in 0 .. 4 do
                let y = top + plotHeight * float gridIndex / 4.0
                yield svgElement "line" [ svgAttr "x1" "0"; svgAttr "x2" "1000"; svgAttr "y1" (fixedText y); svgAttr "y2" (fixedText y); svgAttr "stroke" "#e7ecf3"; svgAttr "stroke-width" "1" ] []

            for rising, candleColor in [ true, "#0f8a78"; false, "#c2414b" ] do
                let selected = candleSeries |> Array.filter (fun point -> (point.Close >= point.Open) = rising)
                let wickPath =
                    selected
                    |> Array.choose (fun point ->
                        tryTimestampIndex point.Timestamp
                        |> Option.map (fun index ->
                            let x = slot * (float index + 0.5)
                            let highY = RendererModel.normalize low high top plotHeight point.High
                            let lowY = RendererModel.normalize low high top plotHeight point.Low
                            "M " + fixedText x + " " + fixedText highY + " V " + fixedText lowY))
                    |> String.concat " "
                let bodyPath =
                    selected
                    |> Array.choose (fun point ->
                        tryTimestampIndex point.Timestamp
                        |> Option.map (fun index ->
                            let x = slot * (float index + 0.5)
                            let openY = RendererModel.normalize low high top plotHeight point.Open
                            let closeY = RendererModel.normalize low high top plotHeight point.Close
                            rectanglePath (x - bodyWidth / 2.0) (min openY closeY) bodyWidth (max 1.2 (abs (closeY - openY)))))
                    |> String.concat " "
                yield svgElement "path" [ Attr.Create "data-candle-part" (if rising then "rising-wicks" else "falling-wicks"); svgAttr "d" wickPath; svgAttr "fill" "none"; svgAttr "stroke" candleColor; svgAttr "stroke-width" "1.2" ] []
                yield svgElement "path" [ Attr.Create "data-candle-part" (if rising then "rising-bodies" else "falling-bodies"); svgAttr "d" bodyPath; svgAttr "fill" candleColor ] []

            for traceIndex, trace, points in linePoints do
                let traceColor = color traceIndex trace
                match trace.Kind with
                | TaTraceKind.Histogram
                | TaTraceKind.Volume ->
                    let zeroY = RendererModel.normalize low high top plotHeight 0.0
                    let barWidth = max 1.0 (slot * 0.64)
                    let path =
                        points
                        |> Array.choose (fun point ->
                            tryTimestampIndex point.Timestamp
                            |> Option.map (fun index ->
                                let x = slot * (float index + 0.18)
                                let valueY = RendererModel.normalize low high top plotHeight point.Value
                                rectanglePath x (min zeroY valueY) barWidth (max 1.0 (abs (zeroY - valueY)))))
                        |> String.concat " "
                    yield svgElement "path" [ Attr.Create "data-testid" ("ta-trace-" + rowId + "-" + trace.TraceId); svgAttr "d" path; svgAttr "fill" traceColor; svgAttr "fill-opacity" "0.62" ] []
                | TaTraceKind.Line ->
                    let path =
                        points
                        |> Array.choose (fun point ->
                            tryTimestampIndex point.Timestamp
                            |> Option.map (fun index -> xAt index, RendererModel.normalize low high top plotHeight point.Value))
                        |> Array.mapi (fun index (x, y) -> (if index = 0 then "M" else "L") + " " + fixedText x + " " + fixedText y)
                        |> String.concat " "
                    yield svgElement "path" [ Attr.Create "data-testid" ("ta-trace-" + rowId + "-" + trace.TraceId); svgAttr "d" path; svgAttr "fill" "none"; svgAttr "stroke" traceColor; svgAttr "stroke-width" (fixedText trace.Width); svgAttr "stroke-linejoin" "round"; svgAttr "stroke-linecap" "round" ] []
                | _ -> ()

            match cursorPosition width referenceTimestamps.Length cursorIndex with
            | Some x -> yield svgElement "line" [ Attr.Create "data-testid" (svgTestId + "-crosshair"); svgAttr "x1" (fixedText x); svgAttr "x2" (fixedText x); svgAttr "y1" "0"; svgAttr "y2" (fixedText plotHeight); svgAttr "stroke" "#1f4f73"; svgAttr "stroke-width" "1"; svgAttr "stroke-dasharray" "3 3" ] []
            | None -> ()
        ], referenceTimestamps

    let renderRow (state: RuntimeState) (ui: TaRendererUiState) visibleTimestamps setCursorIndex showSharedTimeAxis (row: TaRowSpec) =
        let traces = RendererModel.effectiveTraces row |> Array.filter _.Visible
        let chart, timestamps = compositeSvg row.RowId traces state.Data visibleTimestamps ui.CursorIndex setCursorIndex
        let title =
            if isNull row.Traces || row.Traces.Length = 0 then
                rowKindText row.Kind
            else
                traces
                |> Array.map (fun trace -> if String.IsNullOrWhiteSpace trace.Label then trace.TraceId else trace.Label)
                |> String.concat " / "
                |> fun value -> if String.IsNullOrWhiteSpace value then rowKindText row.Kind else value
        let chartHeight = if traces |> Array.exists (fun trace -> trace.Kind = TaTraceKind.Candlestick) then 262 else 124
        let children =
            if showSharedTimeAxis then [ chart; timeAxis "ta-time-axis-shared" timestamps ]
            else [ chart ]
        chartFrame title ("ta-row-" + row.RowId) (chartHeight + if showSharedTimeAxis then 16 else 0) children

    let render (options: TaRendererOptions) (callbacks: TaRendererCallbacks) (runtimeState: Var<RuntimeState>) =
        let canvasId = runtimeState.Value.Identity.CanvasInstanceId
        let mutable instrumentDraft = ""
        let mutable intervalDraft = ""
        let mutable fromDateDraft = ""
        let mutable toDateDraft = ""
        let mutable synchronizedDocumentRevision = -1L
        let addKind = Var.Create "Sma"
        let addDataRef = Var.Create "series.sma"
        let addPeriod = Var.Create "20"
        let addDiPeriod = Var.Create "14"
        let addAdxPeriod = Var.Create "14"
        let addFastPeriod = Var.Create "12"
        let addSlowPeriod = Var.Create "26"
        let addSignalPeriod = Var.Create "9"
        let draftWindow = Var.Create<TaVisibleWindow option> None
        let mutable addRowSequence = 0
        let mutable pendingAddRowId: string option = None
        let mutable navigatorElement: Element = null
        let mutable chartRenderSequence = 0
        let uiState =
            Var.Create
                { Window = { StartIndex = 0; Count = options.DefaultVisibleBars }
                  FollowLatest = true
                  HiddenRows = Set.empty
                  AddRowOpen = false
                  CursorIndex = None
                  Feedback = "" }

        let referenceLength () =
            match runtimeState.Value.Document with
            | None -> 0
            | Some document ->
                document.Rows
                |> Array.tryFind _.Visible
                |> Option.map (fun row -> RendererModel.rowReferenceLength row runtimeState.Value.Data)
                |> Option.defaultValue 0

        let resolvedWindow ui =
            RendererModel.resolveWindow
                options.MinimumVisibleBars
                options.MaximumVisibleBars
                (referenceLength ())
                ui.FollowLatest
                ui.Window

        let setWindow followLatest window =
            let current = uiState.Value
            let total = referenceLength ()
            let bounded = RendererModel.resolveWindow options.MinimumVisibleBars options.MaximumVisibleBars total followLatest window
            uiState.Value <-
                { current with
                    Window = bounded
                    FollowLatest = followLatest
                    CursorIndex = None }
            draftWindow.Value <- None

        let panWindow delta =
            let current = uiState.Value
            let total = referenceLength ()
            let visible = resolvedWindow current
            let candidate =
                RendererModel.clampWindow
                    options.MinimumVisibleBars
                    options.MaximumVisibleBars
                    total
                    { visible with StartIndex = visible.StartIndex + delta }
            let followLatest = candidate.StartIndex = RendererModel.viewportMaximumStart total candidate
            setWindow followLatest candidate

        let zoomWindow delta =
            let current = uiState.Value
            let visible = resolvedWindow current
            setWindow current.FollowLatest { visible with Count = visible.Count + delta }

        let resetWindow () =
            setWindow true { StartIndex = 0; Count = options.DefaultVisibleBars }
            uiState.Value <- { uiState.Value with Feedback = "Local view reset." }

        let setWindowCount count =
            let total = referenceLength ()
            let boundedCount = max options.MinimumVisibleBars (min total count)
            setWindow true { StartIndex = max 0 (total - boundedCount); Count = boundedCount }

        let startNavigatorDrag drag (event: MouseEvent) =
            if not (isNull navigatorElement) then
                event.PreventDefault()
                event.StopPropagation()
                let bounds = navigatorElement.GetBoundingClientRect()
                let total = referenceLength ()
                let committed = resolvedWindow uiState.Value
                let startClientX = event.ClientX
                let mutable moveHandler: Action<Event> = null
                let mutable upHandler: Action<Event> = null

                let cleanup () =
                    if not (isNull moveHandler) then JS.Document.RemoveEventListener("mousemove", moveHandler)
                    if not (isNull upHandler) then JS.Document.RemoveEventListener("mouseup", upHandler)

                moveHandler <-
                    Action<Event>(fun rawEvent ->
                        let mouse = rawEvent :?> MouseEvent
                        let delta =
                            if bounds.Width <= 0.0 || total <= 0 then 0
                            else int (Math.Round(float (mouse.ClientX - startClientX) / bounds.Width * float total))
                        draftWindow.Value <-
                            Some(RendererModel.previewWindowBounds options.MinimumVisibleBars options.MaximumVisibleBars total committed drag delta))

                upHandler <-
                    Action<Event>(fun _ ->
                        let draft = defaultArg draftWindow.Value committed
                        let followLatest, next =
                            RendererModel.commitWindowBounds options.MinimumVisibleBars options.MaximumVisibleBars total draft
                        cleanup ()
                        if next <> committed || followLatest <> uiState.Value.FollowLatest then setWindow followLatest next
                        else draftWindow.Value <- None)

                JS.Document.AddEventListener("mousemove", moveHandler)
                JS.Document.AddEventListener("mouseup", upHandler)

        let setCursorIndex value =
            if uiState.Value.CursorIndex <> value then
                uiState.Value <- { uiState.Value with CursorIndex = value }

        let applyQuery () =
            let parsedInterval =
                match Int32.TryParse intervalDraft with
                | true, value when value > 0 -> Some value
                | _ -> None

            let query =
                { SourceId = None
                  Instrument = if String.IsNullOrWhiteSpace instrumentDraft then None else Some instrumentDraft
                  IntervalMinutes = parsedInterval
                  FromUtc = if String.IsNullOrWhiteSpace fromDateDraft then None else Some fromDateDraft
                  ToUtcExclusive = if String.IsNullOrWhiteSpace toDateDraft then None else Some toDateDraft
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

            let positive fieldName (textValue: string) : Result<int, string> =
                match Int32.TryParse textValue with
                | true, value when value > 0 -> Result.Ok value
                | _ -> Result.Error(fieldName + " must be a positive integer.")

            let optionsResult: Result<Map<string, SduiValue>, string> =
                match kind with
                | TaRowKind.Sma
                | TaRowKind.Dmi ->
                    positive "Period" addPeriod.Value
                    |> Result.map (fun value -> Map [ "period", SduiValue.Number(float value) ])
                | TaRowKind.Adx ->
                    match positive "DI period" addDiPeriod.Value, positive "ADX period" addAdxPeriod.Value with
                    | Ok diPeriod, Ok adxPeriod ->
                        Ok(Map [ "diPeriod", SduiValue.Number(float diPeriod); "adxPeriod", SduiValue.Number(float adxPeriod) ])
                    | Result.Error message, _
                    | _, Result.Error message -> Result.Error message
                | TaRowKind.Macd ->
                    match positive "Fast period" addFastPeriod.Value, positive "Slow period" addSlowPeriod.Value, positive "Signal period" addSignalPeriod.Value with
                    | Ok fast, Ok slow, Ok signal when fast < slow ->
                        Ok(Map [ "fastPeriod", SduiValue.Number(float fast); "slowPeriod", SduiValue.Number(float slow); "signalPeriod", SduiValue.Number(float signal) ])
                    | Ok _, Ok _, Ok _ -> Result.Error "MACD fast period must be smaller than slow period."
                    | Result.Error message, _, _
                    | _, Result.Error message, _
                    | _, _, Result.Error message -> Result.Error message
                | _ -> Result.Ok Map.empty

            match optionsResult with
            | Result.Error message -> uiState.Value <- { uiState.Value with Feedback = message }
            | Ok rowOptions ->
                addRowSequence <- addRowSequence + 1
                let rowId = "row-" + addKind.Value.ToLower() + "-" + string addRowSequence
                let dataRef =
                    if String.IsNullOrWhiteSpace addDataRef.Value then "series." + rowId
                    else addDataRef.Value.Trim()
                let spec =
                    { RowId = rowId
                      Kind = kind
                      DataRef = dataRef
                      HeightWeight = 1.0
                      Visible = true
                      Options = rowOptions
                      Traces = [||] }

                pendingAddRowId <- Some rowId
                submit callbacks uiState (SduiAction.AddTaRow(canvasId, spec)) "Row request submitted."

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
                        instrumentDraft <- query.Instrument
                        intervalDraft <- query.IntervalMinutes
                        fromDateDraft <- query.FromUtc
                        toDateDraft <- query.ToUtcExclusive
                        match pendingAddRowId with
                        | Some rowId when document.Rows |> Array.exists (fun row -> row.RowId = rowId) ->
                            pendingAddRowId <- None
                            uiState.Value <- { uiState.Value with AddRowOpen = false; Feedback = "Row added." }
                        | _ -> ()
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
                                label [ attr.style "display:flex; flex-direction:column; gap:2px; min-width:0; font-size:10px; color:#60738b;" ] [ text "Instrument"; inputText "ta-instrument" "Instrument" instrumentDraft (fun value -> instrumentDraft <- value) ]
                                label [ attr.style "display:flex; flex-direction:column; gap:2px; min-width:0; font-size:10px; color:#60738b;" ] [ text "Interval"; selectInput "ta-interval" intervalDraft [ "1", "1m"; "5", "5m"; "30", "30m"; "60", "60m"; "930", "Session" ] (fun value -> intervalDraft <- value) ]
                                label [ attr.style "display:flex; flex-direction:column; gap:2px; min-width:0; font-size:10px; color:#60738b;" ] [ text "From"; inputText "ta-from" "YYYY-MM-DD" fromDateDraft (fun value -> fromDateDraft <- value) ]
                                label [ attr.style "display:flex; flex-direction:column; gap:2px; min-width:0; font-size:10px; color:#60738b;" ] [ text "To"; inputText "ta-to" "YYYY-MM-DD" toDateDraft (fun value -> toDateDraft <- value) ]
                                primaryButtonState "ta-apply-query" "Load / Apply" commandsDisabled applyQuery
                            ]
                            div [ Attr.Create "data-testid" "ta-local-toolbar"; attr.style "display:flex; align-items:center; gap:5px; flex-wrap:wrap;" ] [
                                compactButton "ta-pan-left" "←" "Pan earlier" (fun () ->
                                    let visible = resolvedWindow uiState.Value
                                    panWindow (-max 1 (visible.Count / 4)))
                                compactButton "ta-pan-right" "→" "Pan later" (fun () ->
                                    let visible = resolvedWindow uiState.Value
                                    panWindow (max 1 (visible.Count / 4)))
                                compactButton "ta-zoom-in" "+" "Show fewer bars" (fun () -> zoomWindow -8)
                                compactButton "ta-zoom-out" "−" "Show more bars" (fun () -> zoomWindow 8)
                                compactButton "ta-reset-view" "Reset View" "Reset local viewport to the latest bars" resetWindow
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
                                        label [ attr.style "display:flex; flex-direction:column; gap:2px; font-size:10px; color:#60738b;" ] [
                                            text "Row kind"
                                            selectInput "ta-add-row-kind" addKind.Value [ "Sma", "SMA"; "Volume", "Volume"; "Dmi", "DMI"; "Adx", "ADX"; "Macd", "MACD"; "HeikinAshi", "Heikin-Ashi" ] (fun value ->
                                                addKind.Value <- value
                                                addDataRef.Value <- "series." + value.ToLower())
                                        ]
                                        label [ attr.style "display:flex; flex-direction:column; gap:2px; font-size:10px; color:#60738b;" ] [ text "Data ref"; inputText "ta-add-row-data-ref" "series.sma" addDataRef.Value (fun value -> addDataRef.Value <- value) ]
                                        addKind.View
                                        |> View.Map (fun kind ->
                                            let field labelText testId value onChanged =
                                                label [ attr.style "display:flex; flex-direction:column; gap:2px; font-size:10px; color:#60738b;" ] [ text labelText; inputText testId labelText value onChanged ] :> Doc

                                            match kind with
                                            | "Sma"
                                            | "Dmi" -> field "Period" "ta-add-row-period" addPeriod.Value (fun value -> addPeriod.Value <- value)
                                            | "Adx" ->
                                                div [ attr.style "display:grid; grid-template-columns:1fr 1fr; gap:6px; min-width:0;" ] [
                                                    field "DI period" "ta-add-row-di-period" addDiPeriod.Value (fun value -> addDiPeriod.Value <- value)
                                                    field "ADX period" "ta-add-row-adx-period" addAdxPeriod.Value (fun value -> addAdxPeriod.Value <- value)
                                                ] :> Doc
                                            | "Macd" ->
                                                div [ attr.style "display:grid; grid-template-columns:repeat(3,1fr); gap:6px; min-width:0;" ] [
                                                    field "Fast" "ta-add-row-fast-period" addFastPeriod.Value (fun value -> addFastPeriod.Value <- value)
                                                    field "Slow" "ta-add-row-slow-period" addSlowPeriod.Value (fun value -> addSlowPeriod.Value <- value)
                                                    field "Signal" "ta-add-row-signal-period" addSignalPeriod.Value (fun value -> addSignalPeriod.Value <- value)
                                                ] :> Doc
                                            | _ -> span [ attr.style "font-size:11px; color:#728196;" ] [ text "No indicator parameters." ] :> Doc)
                                        |> Doc.EmbedView
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
                            chartRenderSequence <- chartRenderSequence + 1
                            let renderSequence = chartRenderSequence
                            let visibleRows =
                                document.Rows
                                |> Array.filter (fun row -> row.Visible && not (Set.contains row.RowId ui.HiddenRows))

                            let referenceTimeline = RendererModel.referenceTimeline visibleRows state.Data
                            let referenceLength = referenceTimeline.Length
                            let overviewPoints =
                                visibleRows
                                |> Array.collect RendererModel.effectiveTraces
                                |> Array.tryFind (fun trace -> trace.Visible && trace.Kind = TaTraceKind.Candlestick)
                                |> Option.map (fun trace -> RendererModel.candleSeries trace.DataRef state.Data)
                                |> Option.defaultValue [||]
                            let visibleWindow =
                                RendererModel.resolveWindow
                                    options.MinimumVisibleBars
                                    options.MaximumVisibleBars
                                    referenceLength
                                    ui.FollowLatest
                                    ui.Window
                            let visibleTimestamps = RendererModel.selectWindow visibleWindow referenceTimeline
                            let cursorIndex =
                                ui.CursorIndex
                                |> Option.map (fun value -> value |> max 0 |> min (max 0 (visibleTimestamps.Length - 1)))
                            let cursorDocument = { document with Rows = visibleRows }
                            let cursor = cursorIndex |> Option.bind (RendererModel.cursorSnapshot cursorDocument state.Data visibleWindow)
                            let cursorValues =
                                match cursor with
                                | None -> div [ attr.style "font-size:11px; color:#718197;" ] [ text "Move the pointer over any chart row to inspect one shared bar." ]
                                | Some value ->
                                    div [ Attr.Create "data-testid" "ta-cursor-values"; attr.style "display:flex; align-items:center; gap:4px 12px; min-width:0; flex-wrap:wrap; white-space:normal; overflow-wrap:anywhere; font-family:Consolas, monospace; font-size:11px; line-height:16px; color:#263b55;" ] [
                                        yield strong [ attr.style "white-space:nowrap;" ] [ text (compactTimestamp value.Timestamp) ]
                                        for item in value.Values do
                                            yield span [ Attr.Create "data-cursor-row" item.Label; attr.style "min-width:0;" ] [ text (item.Label + " " + item.Value) ]
                                    ]

                            let visibleStart = if visibleWindow.Count = 0 then 0 else visibleWindow.StartIndex + 1
                            let visibleEnd = visibleWindow.StartIndex + visibleWindow.Count
                            let viewportRangeText =
                                draftWindow.View
                                |> View.Map (fun draft ->
                                    match draft with
                                    | None -> $"Loaded {referenceLength} bars · Viewing {visibleStart}-{visibleEnd}"
                                    | Some preview ->
                                        let previewStart = if preview.Count = 0 then 0 else preview.StartIndex + 1
                                        let previewEnd = preview.StartIndex + preview.Count
                                        $"Loaded {referenceLength} bars · Preview {previewStart}-{previewEnd} · release to render")
                            div [
                                Attr.Create "data-testid" "ta-chart-stack"
                                Attr.Create "data-chart-render-sequence" (string renderSequence)
                                Attr.Create "data-loaded-bars" (string referenceLength)
                                Attr.Create "data-visible-start" (string visibleStart)
                                Attr.Create "data-visible-end" (string visibleEnd)
                                Attr.Create "data-follow-latest" (if ui.FollowLatest then "true" else "false")
                                Attr.Create "data-cursor-index" (cursorIndex |> Option.map string |> Option.defaultValue "")
                                attr.style "display:flex; flex-direction:column; min-width:0; padding:0 12px 14px;"
                            ] [
                                yield div [ Attr.Create "data-testid" "ta-cursor-panel"; attr.style "order:-2; display:flex; flex-direction:column; gap:5px; align-items:stretch; min-height:34px; padding:6px 8px; border-bottom:1px solid #dce4ef; background:#f8fafc;" ] [
                                    yield cursorValues
                                ]
                                if visibleRows.Length = 0 then
                                    yield div [ attr.style "padding:18px; color:#667891;" ] [ text "No visible TA rows." ]
                                else
                                    for index in 0 .. visibleRows.Length - 1 do
                                        yield renderRow state { ui with CursorIndex = cursorIndex } visibleTimestamps setCursorIndex (index = visibleRows.Length - 1) visibleRows[index]
                                yield div [
                                    Attr.Create "data-testid" "ta-viewport-panel"
                                    attr.style "order:-1; display:grid; grid-template-columns:minmax(220px,1fr) auto; gap:6px 10px; align-items:center; padding:8px; border-bottom:1px solid #d4deea; background:#f8fafc;"
                                ] [
                                    span [
                                        Attr.Create "data-testid" "ta-viewport-range"
                                        attr.style "font-family:Consolas,monospace; font-size:11px; color:#344a65; white-space:nowrap;"
                                    ] [ textView viewportRangeText ]
                                    div [ Attr.Create "data-testid" "ta-viewport-presets"; attr.style "display:flex; gap:4px; align-items:center;" ] [
                                        compactButton "ta-view-48" "48" "Show latest 48 bars" (fun () -> setWindowCount 48)
                                        compactButton "ta-view-200" "200" "Show latest 200 bars" (fun () -> setWindowCount 200)
                                        compactButton "ta-view-all" "All" "Show the complete loaded range" (fun () -> setWindowCount referenceLength)
                                    ]
                                    div [ attr.style "grid-column:1 / -1; min-width:0;" ] [
                                        draftWindow.View
                                        |> View.Map (fun draft ->
                                            let selection = defaultArg draft visibleWindow
                                            let ratios = RendererModel.selectionRatios referenceLength selection
                                            overviewSvg overviewPoints ratios (fun node -> navigatorElement <- node |> As<Element>) startNavigatorDrag :> Doc)
                                        |> Doc.EmbedView
                                    ]
                                ]
                            ] :> Doc)
                        |> Doc.EmbedView
                    ] :> Doc)
            |> Doc.EmbedView
        ]
