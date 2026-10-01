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
    { SubmitAction: DynamicActionRequest -> Async<Result<DynamicActionResult, DynamicHostError>> }

type TaRendererOptions =
    { MinimumVisibleBars: int
      DefaultVisibleBars: int
      MaximumVisibleBars: int
      EditorSchemas: DynamicTemplateSchema array }

type TaRendererUiState =
    { Window: TaVisibleWindow
      FollowLatest: bool
      HiddenRows: Set<string>
      HiddenTraces: Set<string * string>
      RemovedTraces: Set<string * string>
      AddRowOpen: bool
      CursorIndex: int option
      PendingActionId: string option
      Feedback: string }

type TaPendingBoundaryPan =
    { Direction: PulseTrade.Comm.Spa.Dynamic.Renderer.TaCoverageDirection
      LegacyDelta: int
      IntentTimeline: string array
      IntentWindow: TaVisibleWindow
      TargetStartObservationOrdinal: int64 option
      ObservationCount: int
      QueryGeneration: int64 }

[<RequireQualifiedAccess>]
type TaQueuedViewportIntent =
    | LocalRange of SduiAction
    | AdjacentCoverage of PulseTrade.Comm.Spa.Dynamic.Renderer.TaCoverageDirection * int
    | CoverageWindow of int64 * int * string

type TaRendererDisplayTime =
    { Zone: View<SduiDisplayTimeZone>
      Current: unit -> SduiDisplayTimeZone }

[<JavaScript>]
module TaWorkspaceRenderer =
    let axisViewportWidth = Var.Create 1440.0
    let mutable axisResizeBound = false
    let mutable rendererTelemetryInstanceSequence = 0

    type TaPlotPalette =
        { ThemeName: string
          Surface: string
          OverviewSurface: string
          Grid: string
          Cursor: string
          AxisSurface: string
          AxisText: string
          LegendText: string
          Border: string
          OverviewPrice: string
          OverviewCandleUp: string
          OverviewCandleDown: string
          OverviewCandleFlat: string
          OverviewSelection: string
          OverviewBoundary: string
          TooltipSurface: string
          TooltipText: string
          TooltipBorder: string }

    let lightPlotPalette =
        { ThemeName = "light"
          Surface = "#fbfcfe"
          OverviewSurface = "#eef3f8"
          Grid = "#e7ecf3"
          Cursor = "#1f4f73"
          AxisSurface = "#f8fafc"
          AxisText = "#708198"
          LegendText = "#263b55"
          Border = "#c7d3e2"
          OverviewPrice = "#3d718e"
          OverviewCandleUp = "#138a59"
          OverviewCandleDown = "#c53d3d"
          OverviewCandleFlat = "#64748b"
          OverviewSelection = "rgba(203,213,225,.20)"
          OverviewBoundary = "#4ade80"
          TooltipSurface = "#ffffff"
          TooltipText = "#263b55"
          TooltipBorder = "#8ca0b8" }

    let darkPlotPalette =
        { ThemeName = "dark"
          Surface = "#000000"
          OverviewSurface = "#000000"
          Grid = "#334155"
          Cursor = "#7dd3fc"
          AxisSurface = "#0b1017"
          AxisText = "#cbd5e1"
          LegendText = "#e2e8f0"
          Border = "#475569"
          OverviewPrice = "#60a5fa"
          OverviewCandleUp = "#4ade80"
          OverviewCandleDown = "#f87171"
          OverviewCandleFlat = "#94a3b8"
          OverviewSelection = "rgba(203,213,225,.20)"
          OverviewBoundary = "#4ade80"
          TooltipSurface = "#111827"
          TooltipText = "#e2e8f0"
          TooltipBorder = "#64748b" }

    let plotPalette defaultView =
        match (TaPlotSurfacePresentationCodec.resolve defaultView).Theme with
        | TaPlotSurfaceTheme.Dark -> darkPlotPalette
        | TaPlotSurfaceTheme.Light -> lightPlotPalette

    let nextRendererTelemetryInstanceId () =
        rendererTelemetryInstanceSequence <- rendererTelemetryInstanceSequence + 1
        string rendererTelemetryInstanceSequence

    let ensureAxisResizeTracking () =
        if not axisResizeBound then
            axisResizeBound <- true
            let refresh () = axisViewportWidth.Value <- max 320.0 (float JS.Window.InnerWidth - 48.0)
            refresh ()
            JS.Window.AddEventListener("resize", Action<Event>(fun _ -> refresh ()))

    let defaultOptions =
        { MinimumVisibleBars = 12
          DefaultVisibleBars = 48
          MaximumVisibleBars = 4000
          EditorSchemas = [||] }

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

    let rowExplicitLabel (row: TaRowSpec) =
        if isNull (box row.Options) then
            None
        else
            match row.Options |> Map.tryFind "label" with
            | Some(SduiValue.Text value) when not (String.IsNullOrWhiteSpace value) -> Some value
            | _ -> None

    let rowDisplayLabel (row: TaRowSpec) =
        rowExplicitLabel row |> Option.defaultValue (rowKindText row.Kind)

    let editorScalarText = function
        | EditorScalarValue.Text value -> value
        | EditorScalarValue.Number value -> fixedText value
        | EditorScalarValue.Bool value -> if value then "true" else "false"

    let editorPathRoot (path: string) =
        let dotIndex = path.IndexOf('.')
        let bracketIndex = path.IndexOf('[')
        [ dotIndex; bracketIndex ]
        |> List.filter (fun index -> index >= 0)
        |> List.sort
        |> List.tryHead
        |> Option.map (fun index -> path.Substring(0, index))
        |> Option.defaultValue path

    let rowEditorSummary schemas (row: TaRowSpec) =
        match TaRowEditorBinding.tryResolve schemas row with
        | Ok(Some(schema, values)) ->
            let details =
                schema.Fields
                |> Array.choose (fun field ->
                    let fieldValues =
                        values
                        |> Array.filter (fun input -> editorPathRoot input.Path = field.Key)
                        |> Array.map (fun input -> editorScalarText input.Value)
                    if fieldValues.Length = 0 then None
                    else Some(field.Label + " " + String.concat ", " fieldValues))
            let parameters = String.concat "; " details
            if String.IsNullOrWhiteSpace parameters then Some schema.DisplayName
            else Some(schema.DisplayName + " · " + parameters)
        | _ -> None

    let rowDisplayLabelWithEditor schemas row =
        match rowExplicitLabel row, rowEditorSummary schemas row with
        | Some label, Some summary -> label + " · " + summary
        | None, Some summary -> summary
        | Some label, None -> label
        | None, None -> rowKindText row.Kind

    let rowTitle (row: TaRowSpec) (traces: TaTraceSpec array) =
        match rowExplicitLabel row with
        | Some label -> label
        | None when isNull traces || traces.Length = 0 -> rowKindText row.Kind
        | None ->
            traces
            |> Array.map (fun trace -> if String.IsNullOrWhiteSpace trace.Label then trace.TraceId else trace.Label)
            |> String.concat " / "
            |> fun value -> if String.IsNullOrWhiteSpace value then rowKindText row.Kind else value

    let normalizeDocumentPresentation (document: TaWorkspaceDocument) =
        let defaultView =
            match TaLoadedCoverageCodec.tryDecode document.DefaultView with
            | Ok(Some projection) ->
                document.DefaultView
                |> TaLoadedCoverageCodec.apply
                    { projection with
                        CoverageRevision = 0L
                        QueryGeneration = 0L }
            | _ -> document.DefaultView

        { document with DefaultView = defaultView }

    let sameDocumentPresentation (left: RuntimeState) (right: RuntimeState) =
        match left.Document, right.Document with
        | None, None -> true
        | Some leftDocument, Some rightDocument ->
            normalizeDocumentPresentation leftDocument = normalizeDocumentPresentation rightDocument
        | _ -> false

    let sameDocumentShell (left: RuntimeState) (right: RuntimeState) =
        left.Identity = right.Identity && sameDocumentPresentation left right

    let chartTopologySignaturePrepared (state: RuntimeState) prepared =
        match state.Document with
        | None -> [||]
        | Some document ->
            document.Rows
            |> Array.collect (fun row ->
                RendererModel.effectiveTraces row
                |> Array.filter _.Visible
                |> Array.map (fun trace ->
                    let timestamps = RendererModel.traceTopologyTimestampsPrepared trace prepared
                    let first = timestamps |> Array.tryHead |> Option.defaultValue ""
                    let last = timestamps |> Array.tryLast |> Option.defaultValue ""
                    row.RowId, trace.TraceId, timestamps.Length, first, last))

    let chartTopologySignature (state: RuntimeState) =
        chartTopologySignaturePrepared state (RendererModel.prepareData state.Data)

    let sameChartTopology (left: RuntimeState) (right: RuntimeState) =
        left.Identity = right.Identity
        && sameDocumentPresentation left right
        && chartTopologySignature left = chartTopologySignature right

    let runtimeDataChanged (left: RuntimeState) (right: RuntimeState) =
        left.Identity <> right.Identity
        || left.DataRevision <> right.DataRevision
        || not (Object.ReferenceEquals(left.Data, right.Data))

    let tryLegendValue readersByRow rowId traceIndex cursorIndex =
        readersByRow
        |> Map.tryFind rowId
        |> Option.bind (Array.tryItem traceIndex)
        |> Option.bind (fun readLegend -> readLegend cursorIndex)

    let tryRowPresentation readersByRow rowId cursorIndex =
        readersByRow
        |> Map.tryFind rowId
        |> Option.bind (fun readers -> readers |> Array.tryPick (fun readValue -> readValue cursorIndex))

    let rec tryReadAtOrBefore readValue index =
        if index < 0 then None
        else
            match readValue index with
            | Some value -> Some value
            | None -> tryReadAtOrBefore readValue (index - 1)

    let tryLegendValueAtOrBefore readersByRow rowId traceIndex cursorIndex =
        readersByRow
        |> Map.tryFind rowId
        |> Option.bind (Array.tryItem traceIndex)
        |> Option.bind (fun readLegend -> tryReadAtOrBefore readLegend cursorIndex)

    let tryRowPresentationAtOrBefore readersByRow rowId cursorIndex =
        readersByRow
        |> Map.tryFind rowId
        |> Option.bind (fun readers -> readers |> Array.tryPick (fun readValue -> tryReadAtOrBefore readValue cursorIndex))

    let tryLatestLegendValue readersByRow rowId traceIndex =
        readersByRow
        |> Map.tryFind rowId
        |> Option.bind (Array.tryItem traceIndex)
        |> Option.bind (fun readLatest -> readLatest ())

    let tryLatestRowPresentation readersByRow rowId =
        readersByRow
        |> Map.tryFind rowId
        |> Option.bind (Array.tryPick (fun readLatest -> readLatest ()))

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

    let localViewportDisabled = function
        | RuntimePollState.Unmounted
        | RuntimePollState.Disposed -> true
        | _ -> false

    let submit
        (callbacks: TaRendererCallbacks)
        (uiState: Var<TaRendererUiState>)
        actualDocumentRevision
        request
        successText
        onAccepted
        onRejected
        afterSettled =
        let expectedRevisionMatches =
            match request.ExpectedDocumentRevision with
            | Some expected -> expected = actualDocumentRevision
            | None -> true

        if uiState.Value.PendingActionId.IsSome then
            uiState.Value <- { uiState.Value with Feedback = "action-in-flight: wait for the pending action result." }
        elif not expectedRevisionMatches then
            onRejected ()
            uiState.Value <-
                { uiState.Value with
                    PendingActionId = None
                    Feedback = "revision-conflict: workspace is at revision " + string actualDocumentRevision + "." }
            afterSettled ()
        else
            uiState.Value <-
                { uiState.Value with
                    PendingActionId = Some request.RequestId
                    Feedback = "Submitting " + request.RequestId + "..." }

            async {
                let! response = callbacks.SubmitAction request
                let result =
                    match response with
                    | Ok accepted -> accepted
                    | Error error -> DynamicActionResult.Rejected(request.RequestId, error.Code, error.Message)

                let resultRequestId =
                    match result with
                    | DynamicActionResult.Accepted(requestId, _)
                    | DynamicActionResult.Rejected(requestId, _, _)
                    | DynamicActionResult.RevisionConflict(requestId, _) -> requestId

                if uiState.Value.PendingActionId <> Some request.RequestId then
                    ()
                elif resultRequestId <> request.RequestId then
                    onRejected ()
                    uiState.Value <-
                        { uiState.Value with
                            PendingActionId = None
                            Feedback = "action-correlation-mismatch: result does not match the pending request." }
                else
                    match result with
                    | DynamicActionResult.Accepted(_, revision) ->
                        let feedbackOverride = onAccepted ()
                        uiState.Value <-
                            { uiState.Value with
                                PendingActionId = None
                                Feedback =
                                    feedbackOverride
                                    |> Option.defaultValue (successText + " Revision " + string revision + ".") }
                    | DynamicActionResult.Rejected(_, code, message) ->
                        onRejected ()
                        uiState.Value <-
                            { uiState.Value with
                                PendingActionId = None
                                Feedback = code + ": " + message }
                    | DynamicActionResult.RevisionConflict(_, actualRevision) ->
                        onRejected ()
                        uiState.Value <-
                            { uiState.Value with
                                PendingActionId = None
                                Feedback = "revision-conflict: workspace is at revision " + string actualRevision + "." }
                afterSettled ()
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

    let primaryButtonView (testId: string) (label: string) (disabled: View<bool>) (isDisabled: unit -> bool) (onClick: unit -> unit) =
        let enabledStyle =
            "height:30px; border:1px solid #0f766e; border-radius:4px; background:#0f766e; color:#fff; padding:3px 11px; font-size:12px; cursor:pointer; white-space:nowrap;"
        let disabledStyle =
            "height:30px; border:1px solid #9aa8b8; border-radius:4px; background:#d8e0e8; color:#667587; padding:3px 11px; font-size:12px; cursor:not-allowed; white-space:nowrap;"

        button [
            attr.``type`` "button"
            Attr.Create "data-testid" testId
            attr.disabledBool disabled
            Attr.Dynamic "style" (disabled |> View.Map (fun value -> if value then disabledStyle else enabledStyle))
            on.click (fun _ _ -> if not (isDisabled ()) then onClick ())
        ] [ text label ]

    let compactRemoteButton (testId: string) (label: string) (titleText: string) (disabled: View<bool>) (isDisabled: unit -> bool) (onClick: unit -> unit) =
        let enabledStyle =
            "height:30px; border:1px solid #9fb0c6; border-radius:4px; background:#f8fafc; color:#20344f; padding:3px 9px; font-size:12px; cursor:pointer; white-space:nowrap;"
        let disabledStyle =
            "height:30px; border:1px solid #c8d2df; border-radius:4px; background:#edf1f5; color:#8b98a8; padding:3px 9px; font-size:12px; cursor:not-allowed; white-space:nowrap;"

        button [
            attr.``type`` "button"
            Attr.Create "data-testid" testId
            attr.title titleText
            attr.disabledBool disabled
            Attr.Dynamic "style" (disabled |> View.Map (fun value -> if value then disabledStyle else enabledStyle))
            on.click (fun _ _ -> if not (isDisabled ()) then onClick ())
        ] [ text label ]

    let primaryButton testId label onClick =
        primaryButtonState testId label false onClick

    let chartFrame titleText metadata legend testId (height: View<int>) children =
        section [
            Attr.Create "data-testid" testId
            Attr.Dynamic "data-row-frame-height" (height |> View.Map string)
            Attr.Dynamic "style" (height |> View.Map (fun value -> "display:flex; flex-direction:column; min-width:0; min-height:" + string value + "px; border-top:1px solid #e1e7ef; background:#fff;"))
        ] [
            div [ attr.style "display:flex; align-items:center; gap:6px 10px; height:28px; min-height:28px; padding:0 8px; color:#40536d; font-size:11px; flex-wrap:nowrap; overflow-x:auto; overflow-y:hidden; white-space:nowrap;" ] [
                strong [ attr.style "margin-right:auto; flex:0 0 auto;" ] [ text titleText ]
                yield! metadata
            ]
            legend
            element "div" [ attr.style "min-width:0; overflow:hidden;" ] children
        ]

    let rowCursorTagStyle leftPercent visible =
        "position:absolute; z-index:3; top:2px; left:" + leftPercent
        + "%; transform:translateX(-50%); box-sizing:border-box; display:grid; grid-template-rows:12px 12px;"
        + " width:92px; min-width:92px; max-width:92px; height:28px; min-height:28px; max-height:28px;"
        + " padding:1px 5px; border:1px solid #9eabba; border-radius:2px; background:#f8fafc; color:#263b55;"
        + " font-family:Consolas,monospace; font-size:10px; font-weight:500; line-height:12px;"
        + " text-align:center; font-variant-numeric:tabular-nums; white-space:nowrap; pointer-events:none;"
        + (if visible then " visibility:visible;" else " visibility:hidden;")

    let rowResizeHandle rowId (bounds: TaRowHeightBounds) (height: Var<int>) =
        let setHeight value = height.Value <- RendererModel.clamp bounds.Minimum bounds.Maximum value
        let resetHeight () = setHeight bounds.DefaultHeight
        let startResize (event: MouseEvent) =
            event.PreventDefault()
            let startClientY = event.ClientY
            let startHeight = height.Value
            let mutable pendingHeight = startHeight
            let mutable framePending = false
            let mutable moveHandler: Action<Event> = null
            let mutable upHandler: Action<Event> = null
            let flush () =
                framePending <- false
                setHeight pendingHeight
            let cleanup () =
                if not (isNull moveHandler) then JS.Document.RemoveEventListener("mousemove", moveHandler)
                if not (isNull upHandler) then JS.Document.RemoveEventListener("mouseup", upHandler)
                if framePending then flush ()
            moveHandler <-
                Action<Event>(fun rawEvent ->
                    let mouse = rawEvent :?> MouseEvent
                    pendingHeight <- startHeight + mouse.ClientY - startClientY
                    if not framePending then
                        framePending <- true
                        JS.RequestAnimationFrame(fun _ -> flush ()) |> ignore)
            upHandler <- Action<Event>(fun _ -> cleanup ())
            JS.Document.AddEventListener("mousemove", moveHandler)
            JS.Document.AddEventListener("mouseup", upHandler)

        button [
            attr.``type`` "button"
            Attr.Create "data-testid" ("ta-row-resize-" + rowId)
            Attr.Create "role" "separator"
            Attr.Create "aria-orientation" "horizontal"
            Attr.Create "aria-label" ("Resize " + rowId + " chart height")
            Attr.Create "aria-valuemin" (string bounds.Minimum)
            Attr.Create "aria-valuemax" (string bounds.Maximum)
            Attr.Dynamic "aria-valuenow" (height.View |> View.Map string)
            attr.title "Drag to resize. Arrow keys resize; Home or double-click resets."
            attr.style "display:block; width:100%; height:8px; min-height:8px; padding:0; border:0; border-top:1px solid #d6e0eb; border-bottom:1px solid #edf1f6; background:#f5f8fb; cursor:ns-resize;"
            on.mouseDown (fun _ event -> startResize event)
            on.click (fun _ event -> if event.Detail >= 2 then resetHeight ())
            on.keyDown (fun _ event ->
                let key = event :?> KeyboardEvent
                let step = if key.ShiftKey then 32 else 8
                match key.Key with
                | "ArrowUp" -> key.PreventDefault(); setHeight (height.Value - step)
                | "ArrowDown" -> key.PreventDefault(); setHeight (height.Value + step)
                | "Home" -> key.PreventDefault(); resetHeight ()
                | _ -> ())
        ] []

    let cursorPosition width pointCount cursorIndex =
        cursorIndex
        |> Option.bind (RendererModel.slotCenter width pointCount)

    let compactTimestamp (value: string) =
        if String.IsNullOrWhiteSpace value then ""
        elif value.Length >= 16 && value[4] = '-' && value[7] = '-' && (value[10] = 'T' || value[10] = ' ') then
            value.Substring(5, 5) + " " + value.Substring(11, 5)
        else
            value

    let timeAxisWithPalette palette (displayTime: TaRendererDisplayTime) testId rowId (timestamps: string array) =
        View.Map2 (fun width zone -> width, zone) axisViewportWidth.View displayTime.Zone
        |> View.Map (fun (width, zone) ->
            // Edge labels are anchored to the plot boundary rather than centered. Reserve
            // one full edge label plus half of its neighbour so compact UTC labels do not overlap.
            let labels = RendererModel.adaptiveTimeLabels 124.0 width timestamps
            div [
                Attr.Create "data-testid" testId
                Attr.Create "data-time-axis-row-id" rowId
                Attr.Create "data-time-axis-tick-count" (string labels.Length)
                Attr.Create "data-display-time-zone" (SduiDisplayTimeZone.id zone)
                Attr.Create "data-plot-surface-theme" palette.ThemeName
                attr.style ("position:relative; min-width:0; height:18px; padding:0 1px; overflow:hidden; background:" + palette.AxisSurface + ";")
            ] [
                for position in 0 .. labels.Length - 1 do
                    let index, label = labels[position]
                    let left = if timestamps.Length <= 1 then 50.0 else float index / float (timestamps.Length - 1) * 100.0
                    let transform = if position = 0 then "none" elif position = labels.Length - 1 then "translateX(-100%)" else "translateX(-50%)"
                    yield
                        span [
                            Attr.Create "data-time-axis-event-time" label
                            Attr.Create "data-canonical-event-time" label
                            Attr.Create "data-display-time-zone" (SduiDisplayTimeZone.id zone)
                            attr.style (
                                "position:absolute; left:" + fixedText left + "%; transform:" + transform
                                + "; max-width:92px; color:" + palette.AxisText + "; font-size:10px; line-height:16px; white-space:nowrap; overflow:hidden; text-overflow:ellipsis;")
                        ] [ text (TaDisplayTimeFormatter.compactOrOriginal zone label) ]
            ] :> Doc)
        |> Doc.EmbedView

    let timeAxis testId rowId timestamps =
        let zone = Var.Create SduiDisplayTimeZone.Utc
        timeAxisWithPalette lightPlotPalette { Zone = zone.View; Current = fun () -> zone.Value } testId rowId timestamps

    let rectanglePath x y width height =
        "M " + fixedText x + " " + fixedText y
        + " h " + fixedText width
        + " v " + fixedText height
        + " h " + fixedText (-width)
        + " Z"

    let overviewSvgWithPalette palette (points: TaOverviewCandlePoint array) (stripeVisuals: TaOverviewStripeVisual array) referenceLength selectionWindow onReady onPointerDown onDragEnd =
        let width = 1000.0
        let height = 82.0
        let stripeTooltip = Var.Create<Option<float * string>>(None)
        let sampled = RendererModel.compactOverviewCandles 280 points
        let authoredWickCount =
            sampled
            |> Array.sumBy (fun point -> if point.High.IsSome && point.Low.IsSome then 1 else 0)
        let bodyOnlyCount = sampled.Length - authoredWickCount
        let low, high =
            sampled
            |> Array.collect (fun point ->
                let bodyLow = min point.Open point.Close
                let bodyHigh = max point.Open point.Close
                [| point.Low |> Option.defaultValue bodyLow
                   point.High |> Option.defaultValue bodyHigh |])
            |> RendererModel.paddedRange 0.0 1.0
        let candleSlot = if sampled.Length = 0 then width else width / float sampled.Length
        let candleBodyWidth = max 1.0 (min 3.2 (candleSlot * 0.58))
        let xAt index = candleSlot * (float index + 0.5)
        let yAt value = RendererModel.normalize low high 8.0 62.0 value
        let wickPath =
            sampled
            |> Array.mapi (fun index point -> index, point)
            |> Array.choose (fun (index, point) ->
                match point.High, point.Low with
                | Some pointHigh, Some pointLow ->
                    let x = fixedText (xAt index)
                    Some("M " + x + " " + fixedText (yAt pointHigh) + " L " + x + " " + fixedText (yAt pointLow))
                | _ -> None)
            |> String.concat " "
        let bodyPath keep =
            sampled
            |> Array.mapi (fun index point -> index, point)
            |> Array.filter (snd >> keep)
            |> Array.map (fun (index, point) ->
                let openY = yAt point.Open
                let closeY = yAt point.Close
                rectanglePath
                    (xAt index - candleBodyWidth / 2.0)
                    (min openY closeY)
                    candleBodyWidth
                    (max 0.8 (abs (closeY - openY))))
            |> String.concat " "
        let upBodyPath = bodyPath (fun point -> point.Close > point.Open)
        let downBodyPath = bodyPath (fun point -> point.Close < point.Open)
        let flatBodyPath = bodyPath (fun point -> point.Close = point.Open)
        let handleWidth = 8.0
        let selectionGeometry ratios = RendererModel.navigatorSelectionBounds width ratios
        let geometryText projection = selectionWindow |> View.Map (selectionGeometry >> projection >> fixedText)
        let selectionX = geometryText fst
        let selectionWidth = geometryText snd
        let selectionRightX = geometryText (fun (selectionX, selectionWidth) -> selectionX + selectionWidth)
        let leftHandleX = geometryText (fun (selectionX, _) -> max 0.0 (min (width - handleWidth) (selectionX - handleWidth / 2.0)))
        let rightHandleX = geometryText (fun (selectionX, selectionWidth) -> max 0.0 (min (width - handleWidth) (selectionX + selectionWidth - handleWidth / 2.0)))
        let moveHitGeometry ratios =
            let selectionX, selectionWidth = selectionGeometry ratios
            let inset = min handleWidth (selectionWidth / 2.0)
            selectionX + inset, max 0.0 (selectionWidth - inset * 2.0)
        let moveHitX = geometryText (moveHitGeometry >> fst)
        let moveHitWidth = geometryText (moveHitGeometry >> snd)
        let mutable latestSelectionRatios = 0.0, 0.0
        let observedSelectionWindow =
            selectionWindow
            |> View.Map (fun ratios ->
                latestSelectionRatios <- ratios
                ratios)
        let leftRatioText = observedSelectionWindow |> View.Map (fst >> fixedText)
        let rightRatioText = observedSelectionWindow |> View.Map (snd >> fixedText)
        let leftVisualStyle =
            selectionWindow
            |> View.Map (fun ratios ->
                let selectionX, _ = selectionGeometry ratios
                if selectionX <= 0.0001 then "transform:translateX(1px);" else "")
        let rightVisualStyle =
            selectionWindow
            |> View.Map (fun ratios ->
                let selectionX, selectionWidth = selectionGeometry ratios
                if selectionX + selectionWidth >= width - 0.0001 then "transform:translateX(-1px);" else "")
        let stripeX visual =
            RendererModel.slotCenter width referenceLength visual.SlotIndex
            |> Option.defaultValue (width / 2.0)
            |> fun value -> max 0.0 (min width (value + float visual.Lane * 1.5))
        let stripePaths =
            stripeVisuals
            |> Array.groupBy (fun visual ->
                let first = visual.Stripes[0]
                first.Color, first.StrokeWidthCssPixels)
            |> Array.map (fun ((color, strokeWidth), visuals) ->
                let path =
                    visuals
                    |> Array.map (fun visual ->
                        let x = stripeX visual |> fixedText
                        "M " + x + " 0 L " + x + " 82")
                    |> String.concat " "
                color, strokeWidth, visuals |> Array.sumBy (fun visual -> visual.Stripes.Length), path)
        let stripeBuckets =
            stripeVisuals
            |> Array.groupBy (stripeX >> Math.Round >> int)
            |> Map.ofArray
        let stripeTooltipText (visuals: TaOverviewStripeVisual array) =
            visuals
            |> Array.collect (fun visual ->
                visual.Stripes
                |> Array.map (fun stripe ->
                    let label = stripe.Label |> Option.defaultValue stripe.StripeId
                    label + " · " + stripe.EventTimeUtc))
            |> Array.truncate 8
            |> String.concat " | "

        svgElement "svg" [
            Attr.Create "data-testid" "ta-overview-navigator"
            Attr.Create "data-plot-surface-theme" palette.ThemeName
            Attr.Create "data-loaded-sample-count" (string sampled.Length)
            Attr.Create "data-overview-source-count" (string points.Length)
            Attr.Create "data-drag-hit-target-css-pixels" "24"
            Attr.Dynamic "data-selection-left-ratio" leftRatioText
            Attr.Dynamic "data-selection-right-ratio" rightRatioText
            svgAttr "viewBox" "0 0 1000 82"
            svgAttr "preserveAspectRatio" "none"
            attr.style ("display:block; width:100%; height:82px; min-width:0; background:" + palette.OverviewSurface + "; border:1px solid " + palette.Border + "; border-radius:4px; box-sizing:border-box; touch-action:none; cursor:default;")
            on.afterRender onReady
            Attr.Create "data-drag-event-binding" "navigator-root"
            Attr.Create "data-drag-bounds-source" "navigator-root"
            Attr.Handler "pointerdown" (fun element event -> onPointerDown (element |> As<Element>) event)
            Attr.Handler "pointerup" (fun _ event -> onDragEnd event)
            on.mouseMove (fun element event ->
                let bounds = element.GetBoundingClientRect()
                if bounds.Width > 0.0 then
                    let html = element |> As<HTMLElement>
                    let dragOutcome = element.GetAttribute("data-drag-outcome")
                    if dragOutcome = "tracking" || dragOutcome = "moving" then
                        html.Style.SetProperty("cursor", "grabbing")
                    else
                        let pointerX = float event.ClientX - bounds.Left
                        let cursor =
                            match RendererModel.navigatorDragMode bounds.Width 24.0 latestSelectionRatios pointerX with
                            | Some TaWindowDrag.Move -> "grab"
                            | Some _ -> "ew-resize"
                            | None -> "default"
                        html.Style.SetProperty("cursor", cursor)
                    let x = max 0.0 (min width ((float event.ClientX - bounds.Left) / bounds.Width * width))
                    let pixel = int (Math.Round x)
                    let candidates =
                        [| pixel; pixel - 1; pixel + 1; pixel - 2; pixel + 2 |]
                        |> Array.tryPick (fun key -> Map.tryFind key stripeBuckets)
                    stripeTooltip.Value <- candidates |> Option.map (fun values -> x, stripeTooltipText values))
            on.mouseLeave (fun element _ ->
                stripeTooltip.Value <- None
                let dragOutcome = element.GetAttribute("data-drag-outcome")
                if dragOutcome <> "tracking" && dragOutcome <> "moving" then
                    (element |> As<HTMLElement>).Style.SetProperty("cursor", "default"))
        ] [
            yield svgElement "rect" [
                Attr.Create "data-testid" "ta-overview-interaction-surface"
                Attr.Create "data-drag-event-binding" "bubbles-to-navigator-root"
                Attr.Create "data-drag-bounds-source" "navigator-root"
                svgAttr "x" "0"; svgAttr "y" "0"; svgAttr "width" "1000"; svgAttr "height" "82"
                svgAttr "fill" "transparent"; svgAttr "pointer-events" "all"
            ] []
            yield svgElement "path" [
                Attr.Create "data-testid" "ta-overview-candle-wicks"
                Attr.Create "data-candle-sample-count" (string sampled.Length)
                Attr.Create "data-authored-wick-count" (string authoredWickCount)
                Attr.Create "data-body-only-count" (string bodyOnlyCount)
                svgAttr "d" wickPath
                svgAttr "fill" "none"
                svgAttr "stroke" palette.OverviewPrice
                svgAttr "stroke-width" "0.8"
                svgAttr "vector-effect" "non-scaling-stroke"
                svgAttr "pointer-events" "none"
            ] []
            yield svgElement "path" [
                Attr.Create "data-testid" "ta-overview-candle-up-bodies"
                svgAttr "d" upBodyPath
                svgAttr "fill" palette.OverviewCandleUp
                svgAttr "stroke" "none"
                svgAttr "pointer-events" "none"
            ] []
            yield svgElement "path" [
                Attr.Create "data-testid" "ta-overview-candle-down-bodies"
                svgAttr "d" downBodyPath
                svgAttr "fill" palette.OverviewCandleDown
                svgAttr "stroke" "none"
                svgAttr "pointer-events" "none"
            ] []
            yield svgElement "path" [
                Attr.Create "data-testid" "ta-overview-candle-flat-bodies"
                svgAttr "d" flatBodyPath
                svgAttr "fill" palette.OverviewCandleFlat
                svgAttr "stroke" "none"
                svgAttr "pointer-events" "none"
            ] []
            for color, strokeWidth, stripeCount, path in stripePaths do
                yield
                    svgElement "path" [
                        Attr.Create "data-testid" "ta-overview-stripe-path"
                        Attr.Create "data-stripe-count" (string stripeCount)
                        svgAttr "d" path
                        svgAttr "fill" "none"
                        svgAttr "stroke" color
                        svgAttr "stroke-width" (fixedText strokeWidth)
                        svgAttr "vector-effect" "non-scaling-stroke"
                        svgAttr "pointer-events" "none"
                    ] []
            yield svgElement "rect" [
                Attr.Create "data-testid" "ta-overview-selection"
                Attr.Dynamic "x" selectionX; svgAttr "y" "1"; Attr.Dynamic "width" selectionWidth; svgAttr "height" "80"
                svgAttr "fill" palette.OverviewSelection; svgAttr "stroke" "none"; svgAttr "pointer-events" "none"
            ] []
            yield svgElement "line" [
                Attr.Create "data-testid" "ta-overview-left-handle-visual"
                Attr.Create "data-stroke-width-css-pixels" "2"
                Attr.Dynamic "style" leftVisualStyle
                Attr.Dynamic "x1" selectionX; Attr.Dynamic "x2" selectionX
                svgAttr "y1" "1"; svgAttr "y2" "81"
                svgAttr "stroke" palette.OverviewBoundary; svgAttr "stroke-width" "2"
                svgAttr "vector-effect" "non-scaling-stroke"; svgAttr "pointer-events" "none"
            ] []
            yield svgElement "rect" [
                Attr.Create "data-testid" "ta-overview-left-handle"
                Attr.Dynamic "x" leftHandleX; svgAttr "y" "0"; svgAttr "width" (fixedText handleWidth); svgAttr "height" "82"
                svgAttr "fill" "transparent"; svgAttr "pointer-events" "none"; svgAttr "style" "cursor:ew-resize;"
            ] []
            yield svgElement "line" [
                Attr.Create "data-testid" "ta-overview-right-handle-visual"
                Attr.Create "data-stroke-width-css-pixels" "2"
                Attr.Dynamic "style" rightVisualStyle
                Attr.Dynamic "x1" selectionRightX; Attr.Dynamic "x2" selectionRightX
                svgAttr "y1" "1"; svgAttr "y2" "81"
                svgAttr "stroke" palette.OverviewBoundary; svgAttr "stroke-width" "2"
                svgAttr "vector-effect" "non-scaling-stroke"; svgAttr "pointer-events" "none"
            ] []
            yield svgElement "rect" [
                Attr.Create "data-testid" "ta-overview-right-handle"
                Attr.Dynamic "x" rightHandleX; svgAttr "y" "0"; svgAttr "width" (fixedText handleWidth); svgAttr "height" "82"
                svgAttr "fill" "transparent"; svgAttr "pointer-events" "none"; svgAttr "style" "cursor:ew-resize;"
            ] []
            yield svgElement "rect" [
                Attr.Create "data-testid" "ta-overview-move-hit"
                Attr.Dynamic "x" moveHitX; svgAttr "y" "0"; Attr.Dynamic "width" moveHitWidth; svgAttr "height" "82"
                svgAttr "fill" "transparent"; svgAttr "pointer-events" "none"; svgAttr "style" "cursor:grab;"
            ] []
            yield
                stripeTooltip.View
                |> View.Map (function
                    | None -> Doc.Empty
                    | Some(x, value) ->
                        let bounded = if value.Length <= 180 then value else value.Substring(0, 177) + "..."
                        let boxX = max 4.0 (min 716.0 (x + 6.0))
                        svgElement "g" [ Attr.Create "data-testid" "ta-overview-stripe-tooltip"; svgAttr "pointer-events" "none" ] [
                            svgElement "rect" [ svgAttr "x" (fixedText boxX); svgAttr "y" "3"; svgAttr "width" "280"; svgAttr "height" "18"; svgAttr "rx" "2"; svgAttr "fill" palette.TooltipSurface; svgAttr "fill-opacity" "0.95"; svgAttr "stroke" palette.TooltipBorder; svgAttr "stroke-width" "0.7" ] []
                            svgElement "text" [ svgAttr "x" (fixedText (boxX + 5.0)); svgAttr "y" "15"; svgAttr "fill" palette.TooltipText; svgAttr "font-family" "Consolas,monospace"; svgAttr "font-size" "9" ] [ text bounded ]
                        ] :> Doc)
                |> Doc.EmbedView
        ]

    let overviewSvg points stripeVisuals referenceLength selectionWindow onReady onPointerDown onDragEnd =
        overviewSvgWithPalette lightPlotPalette points stripeVisuals referenceLength selectionWindow onReady onPointerDown onDragEnd

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
            yield svgElement "path" [ svgAttr "d" path; svgAttr "fill" "none"; svgAttr "stroke" color; svgAttr "stroke-width" "2"; svgAttr "stroke-linejoin" "round"; svgAttr "stroke-linecap" "round"; svgAttr "vector-effect" "non-scaling-stroke" ] []
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

    let compositeSvgReactivePreparedLiveWithHeightPalette palette (displayTime: TaRendererDisplayTime) rowId isBaseRow (traces: TaTraceSpec array) preparedData (dataView: View<TaPreparedRendererData>) (referenceTimestamps: string array) (cursorIndex: View<int option>) setCursorIndex commitCursorIndex (chartPixelHeight: Var<int>) scheduleValueRefresh =
        let width = 1000.0
        let hasCandles = traces |> Array.exists (fun trace -> trace.Kind = TaTraceKind.Candlestick)
        let height = if hasCandles then 250.0 else 112.0
        let top = if hasCandles then 0.0 else 10.0
        let plotHeight = if hasCandles then height else 92.0
        let tracePalette = [| "#2764b0"; "#9b5b24"; "#6a4ca3"; "#0f766e"; "#b45309"; "#be185d"; "#475569"; "#0891b2" |]
        let color index (trace: TaTraceSpec) =
            if String.IsNullOrWhiteSpace trace.Color then tracePalette[index % tracePalette.Length] else trace.Color

        let xAt index =
            RendererModel.slotCenter width referenceTimestamps.Length index
            |> Option.defaultValue (width / 2.0)

        let maximumVisualPoints = 1000
        let referenceSlots = RendererModel.referenceSlotsByTimestamp referenceTimestamps

        let compactLinePoints (values: (int * TaLinePoint) array) =
            RendererModel.compactProjectedLinePoints maximumVisualPoints referenceTimestamps.Length values

        let prepareGeometry currentPixelHeight currentData =
            let sourcePresentationTimestamps (trace: TaTraceSpec) =
                let dataRef =
                    match trace.CandleDataRefs with
                    | Some refs -> refs.OpenRef
                    | None -> trace.DataRef
                let isUnavailable point =
                    match trace.Kind with
                    | TaTraceKind.Candlestick
                    | TaTraceKind.Volume -> RendererModel.parseCandleResolved point.Temporal point.Payload |> Option.isNone
                    | TaTraceKind.Line
                    | TaTraceKind.Histogram -> RendererModel.parseLineResolved point.Temporal point.Payload |> Option.isNone
                    | TaTraceKind.Marker
                    | TaTraceKind.OverviewStripe -> false
                RendererModel.projectedLastSourceTimestampWhere isUnavailable referenceTimestamps dataRef currentData

            let preparedTraces: (int * TaTraceSpec * TaCandlePoint array * TaLinePoint array) array =
                traces
                |> Array.mapi (fun traceIndex trace ->
                    match trace.Kind with
                    | TaTraceKind.Candlestick
                    | TaTraceKind.Volume ->
                        let candles = RendererModel.candleSeriesForTracePrepared trace currentData
                        traceIndex, trace, candles, [||]
                    | TaTraceKind.Line
                    | TaTraceKind.Histogram ->
                        let lines = RendererModel.lineSeriesPrepared trace.DataRef currentData
                        traceIndex, trace, [||], lines
                    | TaTraceKind.Marker
                    | TaTraceKind.OverviewStripe ->
                        traceIndex, trace, [||], [||])

            let candleTargets = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, TaCandlePoint>>()
            for _, trace, candles, _ in preparedTraces do
                if trace.Kind = TaTraceKind.Candlestick then
                    candleTargets[trace.TraceId] <- RendererModel.candlePointsByTimestamp candles

            let candleSeries =
                if referenceTimestamps.Length <= maximumVisualPoints then
                    let projected = ResizeArray<int * TaTraceSpec * int * int * TaCandlePoint>()
                    for traceIndex, trace, candles, _ in preparedTraces do
                        if trace.Kind = TaTraceKind.Candlestick then
                            for point in candles do
                                match RendererModel.candleSlotRange referenceTimestamps point with
                                | Some(first, lastExclusive) ->
                                    let sourceSpanCount = lastExclusive - first
                                    for slotIndex in first .. lastExclusive - 1 do
                                        projected.Add(traceIndex, trace, slotIndex, sourceSpanCount, point)
                                | None -> ()
                    projected.ToArray()
                else
                    // Keep the full coarse-candle projection semantics without allocating one
                    // tuple per repeated base slot and then grouping/sorting those tuples.
                    let projectionKinds = 2
                    let bucketCount = maximumVisualPoints
                    let accumulatorCount = traces.Length * projectionKinds * bucketCount
                    let firstSlots = Array.create accumulatorCount Int32.MaxValue
                    let lastSlots = Array.create accumulatorCount -1
                    let sourceSpans = Array.zeroCreate<int> accumulatorCount
                    let opens = Array.zeroCreate<float> accumulatorCount
                    let highs = Array.zeroCreate<float> accumulatorCount
                    let lows = Array.zeroCreate<float> accumulatorCount
                    let closes = Array.zeroCreate<float> accumulatorCount
                    let volumes = Array.zeroCreate<float> accumulatorCount
                    let lastPoints: TaCandlePoint option array = Array.create accumulatorCount None

                    let accumulatorIndex traceIndex projected bucketIndex =
                        ((traceIndex * projectionKinds + projected) * bucketCount) + bucketIndex

                    for traceIndex, trace, candles, _ in preparedTraces do
                        if trace.Kind = TaTraceKind.Candlestick then
                            for point in candles do
                                match RendererModel.candleSlotRange referenceTimestamps point with
                                | Some(first, lastExclusive) ->
                                    let sourceSpanCount = lastExclusive - first
                                    let projected = if sourceSpanCount > 1 then 1 else 0
                                    for slotIndex in first .. lastExclusive - 1 do
                                        let bucketIndex = min (bucketCount - 1) (slotIndex * bucketCount / referenceTimestamps.Length)
                                        let index = accumulatorIndex traceIndex projected bucketIndex
                                        if slotIndex < firstSlots[index] then
                                            firstSlots[index] <- slotIndex
                                            opens[index] <- point.Open
                                        if slotIndex >= lastSlots[index] then
                                            lastSlots[index] <- slotIndex
                                            closes[index] <- point.Close
                                            lastPoints[index] <- Some point
                                        if sourceSpans[index] = 0 then
                                            highs[index] <- point.High
                                            lows[index] <- point.Low
                                        else
                                            highs[index] <- max highs[index] point.High
                                            lows[index] <- min lows[index] point.Low
                                        sourceSpans[index] <- max sourceSpans[index] sourceSpanCount
                                        volumes[index] <- volumes[index] + point.Volume
                                | None -> ()

                    [| for traceIndex in 0 .. traces.Length - 1 do
                           for projected in 0 .. projectionKinds - 1 do
                               for bucketIndex in 0 .. bucketCount - 1 do
                                   let index = accumulatorIndex traceIndex projected bucketIndex
                                   match lastPoints[index] with
                                   | Some lastPoint ->
                                       let aggregate =
                                           { lastPoint with
                                               Open = opens[index]
                                               High = highs[index]
                                               Low = lows[index]
                                               Close = closes[index]
                                               Volume = volumes[index] }
                                       yield
                                           traceIndex,
                                           traces[traceIndex],
                                           (firstSlots[index] + lastSlots[index]) / 2,
                                           sourceSpans[index],
                                           aggregate
                                   | None -> () |]

            let projectedLinePoints =
                preparedTraces
                |> Array.map (fun (index, trace, candles, lines) ->
                    let points =
                        (match trace.Kind with
                         | TaTraceKind.Volume ->
                             candles
                             |> Array.map (fun point -> { Timestamp = point.Timestamp; Value = point.Volume; Temporal = point.Temporal })
                         | TaTraceKind.Line
                         | TaTraceKind.Histogram -> lines
                         | _ -> [||])
                        |> RendererModel.projectedLinePoints referenceTimestamps
                    index, trace, points)

            let linePoints =
                projectedLinePoints
                |> Array.map (fun (index, trace, points) -> index, trace, compactLinePoints points)

            let markerPlacements =
                preparedTraces
                |> Array.collect (fun (_, trace, _, _) ->
                    if trace.Kind <> TaTraceKind.Marker then
                        [||]
                    else
                        match TaMarkerTraceOptionsCodec.tryDecode trace.Options with
                        | None -> [||]
                        | Some options ->
                            traces
                            |> Array.tryFind (fun candidate -> candidate.TraceId = options.TargetTraceId && candidate.Kind = TaTraceKind.Candlestick)
                            |> Option.bind (fun target ->
                                match candleTargets.TryGetValue target.TraceId with
                                | true, targetByTimestamp ->
                                    Some(
                                        RendererModel.markerPlacementsPreparedWithIndexes
                                            trace
                                            target.TraceId
                                            targetByTimestamp
                                            currentData
                                            referenceSlots)
                                | _ -> None)
                            |> Option.defaultValue [||])
                |> RendererModel.assignAggregateMarkerLanes

            let overviewStripePlacements =
                preparedTraces
                |> Array.collect (fun (_, trace, _, _) ->
                    if trace.Kind = TaTraceKind.OverviewStripe then
                        RendererModel.overviewStripePlacementsPrepared trace currentData referenceTimestamps
                    else
                        [||])

            let mutable hasScaleValue = false
            let mutable scaleLow = 0.0
            let mutable scaleHigh = 0.0
            let includeScaleValue value =
                if hasScaleValue then
                    scaleLow <- min scaleLow value
                    scaleHigh <- max scaleHigh value
                else
                    hasScaleValue <- true
                    scaleLow <- value
                    scaleHigh <- value

            for _, _, _, _, point in candleSeries do
                includeScaleValue point.Low
                includeScaleValue point.High

            for _, trace, points in projectedLinePoints do
                if trace.Kind = TaTraceKind.Histogram then
                    includeScaleValue 0.0
                for _, point in points do
                    includeScaleValue point.Value

            let bounds =
                if not hasScaleValue then
                    None
                else
                    Some(scaleLow, scaleHigh)
            let low, high =
                if hasCandles then
                    RendererModel.paddedBoundsForCssPixels
                        0.0
                        1.0
                        height
                        plotHeight
                        (float currentPixelHeight)
                        15.0
                        bounds
                elif not hasScaleValue then
                    0.0, 1.0
                elif scaleLow = scaleHigh then
                    scaleLow - 1.0, scaleHigh + 1.0
                else
                    let padding = max ((scaleHigh - scaleLow) * 0.08) 0.0001
                    scaleLow - padding, scaleHigh + padding
            let readers =
                preparedTraces
                |> Array.map (fun (traceIndex, trace, candles, _) ->
                    let label = if String.IsNullOrWhiteSpace trace.Label then trace.TraceId else trace.Label
                    let sourceTimestamps = sourcePresentationTimestamps trace
                    let unavailablePresentation index =
                        sourceTimestamps
                        |> Array.tryItem index
                        |> Option.flatten
                        |> Option.map (fun timestamp -> { Timestamp = timestamp; Value = "Unavailable" })
                    match trace.Kind with
                    | TaTraceKind.Candlestick
                    | TaTraceKind.Volume ->
                        let values =
                            RendererModel.projectedCandleCursorValues isBaseRow referenceTimestamps candles
                        let cursorReader index =
                            values
                            |> Array.tryItem index
                            |> Option.flatten
                            |> Option.map (RendererModel.candleCursorPointValue label trace.Kind)
                        let legendReader index =
                            values
                            |> Array.tryItem index
                            |> Option.flatten
                            |> Option.map (fun point ->
                                { Timestamp = point.Timestamp
                                  Value =
                                    if trace.Kind = TaTraceKind.Volume then fixedText point.Volume
                                    else
                                        "O " + fixedText point.Open
                                        + " H " + fixedText point.High
                                        + " L " + fixedText point.Low
                                        + " C " + fixedText point.Close
                                        + " V " + fixedText point.Volume })
                            |> Option.orElseWith (fun () -> unavailablePresentation index)
                        let latestLegend = tryReadAtOrBefore legendReader (referenceTimestamps.Length - 1)
                        cursorReader, legendReader, latestLegend
                    | TaTraceKind.Line
                    | TaTraceKind.Histogram ->
                        let values: TaLinePoint option array = Array.create referenceTimestamps.Length None
                        projectedLinePoints
                        |> Array.tryFind (fun (index, _, _) -> index = traceIndex)
                        |> Option.iter (fun (_, _, points) ->
                            for index, point in points do
                                if index >= 0 && index < values.Length then
                                    values[index] <- Some point)
                        let cursorReader index =
                            values
                            |> Array.tryItem index
                            |> Option.flatten
                            |> Option.map (RendererModel.lineCursorPointValue label)
                        let legendReader index =
                            values
                            |> Array.tryItem index
                            |> Option.flatten
                            |> Option.map (fun point -> { Timestamp = point.Timestamp; Value = fixedText point.Value })
                            |> Option.orElseWith (fun () -> unavailablePresentation index)
                        let latestLegend = tryReadAtOrBefore legendReader (referenceTimestamps.Length - 1)
                        cursorReader, legendReader, latestLegend
                    | TaTraceKind.Marker
                    | TaTraceKind.OverviewStripe ->
                        (fun _ -> None), (fun _ -> None), None)
            let cursorReaders = readers |> Array.map (fun (cursorReader, _, _) -> cursorReader)
            let legendReaders = readers |> Array.map (fun (_, legendReader, _) -> legendReader)
            let latestLegendValues = readers |> Array.map (fun (_, _, latestLegend) -> latestLegend)

            preparedTraces, candleSeries, linePoints, markerPlacements, overviewStripePlacements, cursorReaders, legendReaders, latestLegendValues, low, high

        let initialGeometry = prepareGeometry chartPixelHeight.Value preparedData
        let _, initialCandleSeries, initialLinePoints, initialMarkerPlacements, initialOverviewStripePlacements, initialCursorReaders, initialLegendReaders, initialLatestLegendValues, initialLow, initialHigh = initialGeometry
        let markerVisualState = Var.Create(initialMarkerPlacements, initialOverviewStripePlacements, initialLow, initialHigh)
        let readerStates =
            Array.map3
                (fun cursorReader legendReader latestLegend -> ref (cursorReader, legendReader, latestLegend))
                initialCursorReaders
                initialLegendReaders
                initialLatestLegendValues

        let slot = if referenceTimestamps.Length = 0 then width else width / float referenceTimestamps.Length
        let svgTestId = if hasCandles then "ta-candle-" + rowId else "ta-composite-" + rowId

        let candlePaths traceIndex (_, currentCandles, _, _, _, _, _, _, low, high) =
            let buckets = Array.init 8 (fun _ -> ResizeArray<string>())
            for currentTraceIndex, _, slotIndex, sourceSpanCount, (point: TaCandlePoint) in currentCandles do
                if currentTraceIndex = traceIndex then
                    let projectedOffset = if sourceSpanCount > 1 then 4 else 0
                    let directionOffset = if point.Close >= point.Open then 0 else 2
                    let wickIndex = projectedOffset + directionOffset
                    let center = xAt slotIndex
                    let bodyWidth = max 2.0 (slot * 0.64)
                    let highY = RendererModel.normalize low high top plotHeight point.High
                    let lowY = RendererModel.normalize low high top plotHeight point.Low
                    let openY = RendererModel.normalize low high top plotHeight point.Open
                    let closeY = RendererModel.normalize low high top plotHeight point.Close
                    buckets[wickIndex].Add($"M {fixedText center} {fixedText highY} L {fixedText center} {fixedText lowY}")
                    buckets[wickIndex + 1].Add(
                        rectanglePath
                            (center - bodyWidth / 2.0)
                            (min openY closeY)
                            bodyWidth
                            (max 1.2 (abs (closeY - openY))))

            buckets |> Array.map (String.concat " ")

        let lineGeometry traceIndex (_, _, currentLines, _, _, _, _, _, low, high) =
            let _, _, points =
                currentLines
                |> Array.tryFind (fun (index, _, _) -> index = traceIndex)
                |> Option.defaultWith (fun () -> initialLinePoints |> Array.find (fun (index, _, _) -> index = traceIndex))
            points, low, high

        let linePaths traceIndex (trace: TaTraceSpec) geometry =
            let points, low, high = lineGeometry traceIndex geometry
            match trace.Kind with
            | TaTraceKind.Histogram ->
                let zeroY = RendererModel.normalize low high top plotHeight 0.0
                let barWidth = max 1.0 (slot * 0.64)
                let path predicate =
                    points
                    |> Array.choose (fun (index, point: TaLinePoint) ->
                        if predicate point.Value then
                            let x = slot * (float index + 0.18)
                            let valueY = RendererModel.normalize low high top plotHeight point.Value
                            Some(rectanglePath x (min zeroY valueY) barWidth (max 1.0 (abs (zeroY - valueY))))
                        else
                            None)
                    |> String.concat " "
                path (fun value -> value >= 0.0), path (fun value -> value < 0.0)
            | TaTraceKind.Volume ->
                let zeroY = RendererModel.normalize low high top plotHeight 0.0
                let barWidth = max 1.0 (slot * 0.64)
                (points
                 |> Array.map (fun (index, point: TaLinePoint) ->
                     let x = slot * (float index + 0.18)
                     let valueY = RendererModel.normalize low high top plotHeight point.Value
                     rectanglePath x (min zeroY valueY) barWidth (max 1.0 (abs (zeroY - valueY))))
                 |> String.concat " "), ""
            | TaTraceKind.Line ->
                (points
                 |> Array.map (fun (index, point: TaLinePoint) -> xAt index, RendererModel.normalize low high top plotHeight point.Value)
                 |> Array.mapi (fun index (x, y) -> (if index = 0 then "M" else "L") + " " + fixedText x + " " + fixedText y)
                 |> String.concat " "), ""
            | _ -> "", ""

        let lineLastValue traceIndex geometry =
            let points, _, _ = lineGeometry traceIndex geometry
            points
            |> Array.tryLast
            |> Option.map (snd >> _.Value >> fixedText)
            |> Option.defaultValue ""

        let markerCenter (placement: TaMarkerPlacement) low high =
            let size = 9.0
            let half = size / 2.0
            let laneStep = size + 2.0
            let x = xAt placement.SlotIndex
            let anchorY =
                match placement.Marker.Anchor with
                | TaMarkerAnchor.AboveBar -> RendererModel.normalize low high top plotHeight placement.Target.High
                | TaMarkerAnchor.BelowBar -> RendererModel.normalize low high top plotHeight placement.Target.Low
            let proposedY =
                match placement.Marker.Anchor with
                | TaMarkerAnchor.AboveBar -> anchorY - 4.0 - half - float placement.Lane * laneStep
                | TaMarkerAnchor.BelowBar -> anchorY + 4.0 + half + float placement.Lane * laneStep
            let y = max half (min (height - half) proposedY)
            x, y

        let markerShape zone (placement: TaMarkerPlacement) low high =
            let size = 9.0
            let half = size / 2.0
            let x, y = markerCenter placement low high
            let fill, fillOpacity =
                match placement.Marker.Fill with
                | TaMarkerFill.Solid -> placement.Marker.Color, "1"
                | TaMarkerFill.Outline -> "none", "1"
            let common =
                [ Attr.Create "data-testid" ("ta-marker-" + placement.TraceId + "-" + placement.Marker.MarkerId)
                  Attr.Create "data-marker-id" placement.Marker.MarkerId
                  Attr.Create "data-marker-position" (fixedText placement.Position)
                  Attr.Create "data-marker-slot" (string placement.SlotIndex)
                  Attr.Create "data-marker-event-time" placement.Marker.EventTimeUtc
                  Attr.Create "data-marker-lane" (string placement.Lane)
                  Attr.Create "data-marker-anchor" (if placement.Marker.Anchor = TaMarkerAnchor.AboveBar then "above-bar" else "below-bar")
                  Attr.Create "data-marker-shape" (TaMarkerCodec.shapeText placement.Marker.Shape)
                  Attr.Create "data-marker-fill" (TaMarkerCodec.fillText placement.Marker.Fill)
                  svgAttr "fill" fill
                  svgAttr "fill-opacity" fillOpacity
                  svgAttr "stroke" placement.Marker.Color
                  svgAttr "stroke-width" "1.4"
                  svgAttr "pointer-events" "all"
                  svgAttr "vector-effect" "non-scaling-stroke"
                  on.mouseMove (fun _ event ->
                      event.StopPropagation()
                      setCursorIndex (Some placement.SlotIndex))
                  on.click (fun _ event ->
                      event.StopPropagation()
                      commitCursorIndex placement.SlotIndex) ]
            let title =
                svgElement "title" [] [
                    text (RendererModel.markerTooltipTextWith (TaDisplayTimeFormatter.fullOrOriginal zone) placement)
                ]
            let elementName, geometry =
                match placement.Marker.Shape with
                | TaMarkerShape.Circle ->
                    "circle", [ svgAttr "cx" (fixedText x); svgAttr "cy" (fixedText y); svgAttr "r" (fixedText half) ]
                | TaMarkerShape.Square ->
                    "rect", [ svgAttr "x" (fixedText (x - half)); svgAttr "y" (fixedText (y - half)); svgAttr "width" (fixedText size); svgAttr "height" (fixedText size) ]
                | TaMarkerShape.Diamond ->
                    let points = $"{fixedText x},{fixedText (y - half)} {fixedText (x + half)},{fixedText y} {fixedText x},{fixedText (y + half)} {fixedText (x - half)},{fixedText y}"
                    "polygon", [ svgAttr "points" points ]
                | TaMarkerShape.TriangleUp
                | TaMarkerShape.TriangleDown ->
                    let points =
                        RendererModel.markerTrianglePoints placement.Marker.Shape x y half
                        |> Option.defaultValue [||]
                        |> Array.map (fun (pointX, pointY) -> $"{fixedText pointX},{fixedText pointY}")
                        |> String.concat " "
                    "polygon", [ svgAttr "points" points ]
            let contrastStroke = if palette.ThemeName = "dark" then "#f8fafc" else "#0f172a"
            let halo =
                svgElement elementName
                    ([ Attr.Create "data-marker-contrast-halo" "true"
                       Attr.Create "data-marker-halo-for" placement.Marker.MarkerId
                       svgAttr "fill" "none"
                       svgAttr "stroke" contrastStroke
                       svgAttr "stroke-width" "4.4"
                       svgAttr "stroke-opacity" "0.95"
                       svgAttr "pointer-events" "none"
                       svgAttr "vector-effect" "non-scaling-stroke" ] @ geometry)
                    []
            let semanticShape = svgElement elementName (common @ geometry) [ title ]
            svgElement "g" [ Attr.Create "data-marker-visual" placement.Marker.MarkerId ] [ halo; semanticShape ]

        let markerClusterSelection = Var.Create<Option<string * int>>(None)

        let markerClusterPosition low high (cluster: TaMarkerOverflowCluster) =
            let size = 9.0
            let half = size / 2.0
            let laneStep = size + 2.0
            let target = cluster.Markers[0].Target
            let anchorY =
                match cluster.Anchor with
                | TaMarkerAnchor.AboveBar -> RendererModel.normalize low high top plotHeight target.High
                | TaMarkerAnchor.BelowBar -> RendererModel.normalize low high top plotHeight target.Low
            let proposedY =
                match cluster.Anchor with
                | TaMarkerAnchor.AboveBar -> anchorY - 4.0 - half - float cluster.Lane * laneStep
                | TaMarkerAnchor.BelowBar -> anchorY + 4.0 + half + float cluster.Lane * laneStep
            xAt cluster.SlotIndex, max 8.0 (min (height - 8.0) proposedY)

        let markerLayer =
            View.Map2 (fun markerVisual zone -> markerVisual, zone) markerVisualState.View displayTime.Zone
            |> View.Map (fun (((placements: TaMarkerPlacement array), _, low, high), zone) ->
                let directPlacements, overflowClusters = RendererModel.markerPresentation placements
                svgElement "g" [
                    Attr.Create "data-testid" ("ta-marker-layer-" + rowId)
                    Attr.Create "data-marker-count" (string placements.Length)
                    Attr.Create "data-direct-marker-count" (string directPlacements.Length)
                    Attr.Create "data-marker-overflow-count" (string overflowClusters.Length)
                ] [
                    for placement in directPlacements do
                        yield markerShape zone placement low high
                    for cluster in overflowClusters do
                        let x, y = markerClusterPosition low high cluster
                        let hiddenCount = cluster.Markers.Length
                        let selectCluster index =
                            markerClusterSelection.Value <- Some(cluster.ClusterId, max 0 (min (hiddenCount - 1) index))
                        let toggleCluster () =
                            match markerClusterSelection.Value with
                            | Some(clusterId, _) when clusterId = cluster.ClusterId -> markerClusterSelection.Value <- None
                            | _ -> selectCluster 0
                        yield
                            svgElement "g" [
                                Attr.Create "data-testid" ("ta-marker-overflow-" + cluster.ClusterId)
                                Attr.Create "data-marker-overflow-count" (string hiddenCount)
                                Attr.Create "role" "button"
                                Attr.Create "tabindex" "0"
                                Attr.Create "aria-label" ($"{hiddenCount} additional markers. Activate to inspect.")
                                svgAttr "style" "cursor:pointer;"
                                on.mouseMove (fun _ event ->
                                    event.StopPropagation()
                                    setCursorIndex (Some cluster.SlotIndex))
                                on.click (fun _ event ->
                                    event.StopPropagation()
                                    commitCursorIndex cluster.SlotIndex
                                    toggleCluster ())
                                on.keyDown (fun _ event ->
                                    let key = event :?> KeyboardEvent
                                    match key.Key with
                                    | "Enter"
                                    | " " ->
                                        key.PreventDefault()
                                        toggleCluster ()
                                    | "ArrowDown"
                                    | "ArrowRight" ->
                                        key.PreventDefault()
                                        match markerClusterSelection.Value with
                                        | Some(clusterId, index) when clusterId = cluster.ClusterId -> selectCluster (index + 1)
                                        | _ -> selectCluster 0
                                    | "ArrowUp"
                                    | "ArrowLeft" ->
                                        key.PreventDefault()
                                        match markerClusterSelection.Value with
                                        | Some(clusterId, index) when clusterId = cluster.ClusterId -> selectCluster (index - 1)
                                        | _ -> selectCluster (hiddenCount - 1)
                                    | "Escape" ->
                                        key.PreventDefault()
                                        markerClusterSelection.Value <- None
                                    | _ -> ())
                            ] [
                                svgElement "rect" [
                                    svgAttr "x" (fixedText (x - 10.0)); svgAttr "y" (fixedText (y - 7.0))
                                    svgAttr "width" "20"; svgAttr "height" "14"; svgAttr "rx" "3"
                                    svgAttr "fill" palette.TooltipSurface; svgAttr "stroke" palette.TooltipBorder; svgAttr "stroke-width" "1.2"
                                    svgAttr "vector-effect" "non-scaling-stroke"
                                ] []
                                svgElement "text" [
                                    svgAttr "x" (fixedText x); svgAttr "y" (fixedText (y + 0.5))
                                    svgAttr "text-anchor" "middle"; svgAttr "dominant-baseline" "middle"
                                    svgAttr "font-family" "Consolas,monospace"; svgAttr "font-size" "8"
                                    svgAttr "font-weight" "700"; svgAttr "fill" palette.TooltipText
                                    svgAttr "pointer-events" "none"
                                ] [ text ("+" + string hiddenCount) ]
                                svgElement "title" [] [ text ($"{hiddenCount} additional markers") ]
                            ]
                    yield
                        markerClusterSelection.View
                        |> View.Map (function
                            | None -> Doc.Empty
                            | Some(clusterId, selectedIndex) ->
                                match overflowClusters |> Array.tryFind (fun cluster -> cluster.ClusterId = clusterId) with
                                | None -> Doc.Empty
                                | Some cluster ->
                                    let index = max 0 (min (cluster.Markers.Length - 1) selectedIndex)
                                    let selected = cluster.Markers[index]
                                    let x, y = markerClusterPosition low high cluster
                                    let textValue = RendererModel.markerTooltipTextWith (TaDisplayTimeFormatter.fullOrOriginal zone) selected
                                    let bounded = if textValue.Length <= 160 then textValue else textValue.Substring(0, 157) + "..."
                                    let boxX = max 6.0 (min 684.0 (x + 12.0))
                                    let boxY = max 4.0 (min (height - 28.0) (y - 12.0))
                                    svgElement "g" [
                                        Attr.Create "data-testid" "ta-marker-overflow-detail"
                                        Attr.Create "data-cluster-id" cluster.ClusterId
                                        Attr.Create "data-cluster-selected-index" (string index)
                                        Attr.Create "aria-live" "polite"
                                        svgAttr "pointer-events" "none"
                                    ] [
                                        svgElement "rect" [ svgAttr "x" (fixedText boxX); svgAttr "y" (fixedText boxY); svgAttr "width" "304"; svgAttr "height" "24"; svgAttr "rx" "3"; svgAttr "fill" palette.TooltipSurface; svgAttr "fill-opacity" "0.97"; svgAttr "stroke" palette.TooltipBorder; svgAttr "stroke-width" "0.8" ] []
                                        svgElement "text" [ svgAttr "x" (fixedText (boxX + 6.0)); svgAttr "y" (fixedText (boxY + 15.0)); svgAttr "fill" palette.TooltipText; svgAttr "font-family" "Consolas,monospace"; svgAttr "font-size" "9" ] [ text ($"{index + 1}/{cluster.Markers.Length} {bounded}") ]
                                    ] :> Doc)
                        |> Doc.EmbedView
                ])
            |> Doc.EmbedView

        let candlePathStates =
            initialCandleSeries
            |> Array.map (fun (traceIndex, trace, _, _, _) -> traceIndex, trace)
            |> Array.distinctBy fst
            |> Array.map (fun (traceIndex, trace) ->
                traceIndex,
                trace,
                (candlePaths traceIndex initialGeometry |> Array.map Var.Create))

        let lineVisualStates =
            initialLinePoints
            |> Array.map (fun (traceIndex, trace, _) ->
                let positivePath, negativePath = linePaths traceIndex trace initialGeometry
                traceIndex,
                trace,
                Var.Create positivePath,
                Var.Create negativePath,
                Var.Create(lineLastValue traceIndex initialGeometry))

        let mutable observedPreparedData = preparedData
        let mutable observedChartPixelHeight = chartPixelHeight.Value
        View.Map2 (fun currentData currentPixelHeight -> currentData, currentPixelHeight) dataView chartPixelHeight.View
        |> View.Sink (fun (currentData, currentPixelHeight) ->
            if not (Object.ReferenceEquals(currentData, observedPreparedData))
               || currentPixelHeight <> observedChartPixelHeight then
                observedPreparedData <- currentData
                observedChartPixelHeight <- currentPixelHeight
                let geometry = prepareGeometry currentPixelHeight currentData
                let _, _, _, currentMarkerPlacements, currentOverviewStripePlacements, currentCursorReaders, currentLegendReaders, currentLatestLegendValues, currentLow, currentHigh = geometry

                let nextMarkerVisual = currentMarkerPlacements, currentOverviewStripePlacements, currentLow, currentHigh
                if markerVisualState.Value <> nextMarkerVisual then markerVisualState.Value <- nextMarkerVisual

                for index in 0 .. readerStates.Length - 1 do
                    readerStates[index].Value <- currentCursorReaders[index], currentLegendReaders[index], currentLatestLegendValues[index]

                for traceIndex, _, pathStates in candlePathStates do
                    let nextPaths = candlePaths traceIndex geometry
                    for index in 0 .. pathStates.Length - 1 do
                        if pathStates[index].Value <> nextPaths[index] then
                            pathStates[index].Value <- nextPaths[index]

                for traceIndex, trace, positivePathState, negativePathState, lastValueState in lineVisualStates do
                    let nextPositivePath, nextNegativePath = linePaths traceIndex trace geometry
                    let nextLastValue = lineLastValue traceIndex geometry
                    if positivePathState.Value <> nextPositivePath then positivePathState.Value <- nextPositivePath
                    if negativePathState.Value <> nextNegativePath then negativePathState.Value <- nextNegativePath
                    if lastValueState.Value <> nextLastValue then lastValueState.Value <- nextLastValue

                scheduleValueRefresh ())

        svgElement "svg" [
            svgAttr "viewBox" ("0 0 1000 " + fixedText height)
            svgAttr "preserveAspectRatio" "none"
            svgAttr "role" "img"
            svgAttr "aria-label" ("Composite TA row " + rowId)
            Attr.Create "data-testid" svgTestId
            Attr.Create "data-plot-surface-theme" palette.ThemeName
            Attr.Create "data-point-count" (string referenceTimestamps.Length)
            Attr.Dynamic "style" (chartPixelHeight.View |> View.Map (fun value -> "display:block; width:100%; height:" + string value + "px; background:" + palette.Surface + ";"))
            on.mouseMove (fun element event ->
                let bounds = element.GetBoundingClientRect()
                match RendererModel.cursorIndexFromClientX referenceTimestamps.Length bounds.Left bounds.Width event.ClientX with
                | Some index -> setCursorIndex (Some index)
                | None -> ())
            on.click (fun element event ->
                let bounds = element.GetBoundingClientRect()
                match RendererModel.cursorIndexFromClientX referenceTimestamps.Length bounds.Left bounds.Width event.ClientX with
                | Some index -> commitCursorIndex index
                | None -> ())
        ] [
            for gridIndex in 0 .. 4 do
                let y = top + plotHeight * float gridIndex / 4.0
                yield svgElement "line" [ svgAttr "x1" "0"; svgAttr "x2" "1000"; svgAttr "y1" (fixedText y); svgAttr "y2" (fixedText y); svgAttr "stroke" palette.Grid; svgAttr "stroke-width" "1" ] []

            for traceIndex, trace, pathStates in candlePathStates do
                let traceTestId = "ta-candle-" + rowId + "-" + trace.TraceId
                let projectedColor = color traceIndex trace
                let styles =
                    [| "normal-up-wick", "none", "#0f8a78", "1.2", "1"
                       "normal-up-body", "#0f8a78", "none", "0", "1"
                       "normal-down-wick", "none", "#c2414b", "1.2", "1"
                       "normal-down-body", "#c2414b", "none", "0", "1"
                       "projected-up-wick", "none", projectedColor, "1.2", "1"
                       "projected-up-body", "#0f8a78", projectedColor, "1", "0.48"
                       "projected-down-wick", "none", projectedColor, "1.2", "1"
                       "projected-down-body", "#c2414b", projectedColor, "1", "0.48" |]
                for index in 0 .. styles.Length - 1 do
                    let part, fill, stroke, strokeWidth, fillOpacity = styles[index]
                    yield
                        svgElement "path" [
                            Attr.Create "data-testid" traceTestId
                            Attr.Create "data-candle-part" part
                            Attr.Create "data-candle-batched" "true"
                            Attr.Dynamic "d" pathStates[index].View
                            svgAttr "fill" fill
                            svgAttr "fill-opacity" fillOpacity
                            svgAttr "stroke" stroke
                            svgAttr "stroke-width" strokeWidth
                            svgAttr "vector-effect" "non-scaling-stroke"
                        ] []

            for traceIndex, trace, _ in initialLinePoints do
                let traceColor = color traceIndex trace
                let _, _, positivePathState, negativePathState, lastValueState = lineVisualStates |> Array.find (fun (index, _, _, _, _) -> index = traceIndex)
                let positivePath = positivePathState.View
                let negativePath = negativePathState.View
                let lastValue = lastValueState.View
                match trace.Kind with
                | TaTraceKind.Histogram ->
                    let histogramStyle =
                        TaHistogramTraceOptionsCodec.tryDecode trace.Options
                        |> Option.defaultValue
                            { PositiveColor = traceColor
                              NegativeColor = traceColor }
                    let traceTestId = "ta-trace-" + rowId + "-" + trace.TraceId
                    yield svgElement "path" [ Attr.Create "data-testid" traceTestId; Attr.Create "data-histogram-polarity" "positive"; Attr.Dynamic "d" positivePath; Attr.Dynamic "data-last-value" lastValue; svgAttr "fill" histogramStyle.PositiveColor; svgAttr "fill-opacity" "0.74" ] []
                    yield svgElement "path" [ Attr.Create "data-testid" (traceTestId + "-negative"); Attr.Create "data-histogram-polarity" "negative"; Attr.Dynamic "d" negativePath; svgAttr "fill" histogramStyle.NegativeColor; svgAttr "fill-opacity" "0.74" ] []
                | TaTraceKind.Volume ->
                    yield svgElement "path" [ Attr.Create "data-testid" ("ta-trace-" + rowId + "-" + trace.TraceId); Attr.Dynamic "d" positivePath; Attr.Dynamic "data-last-value" lastValue; svgAttr "fill" traceColor; svgAttr "fill-opacity" "0.62" ] []
                | TaTraceKind.Line ->
                    let strokeWidthCssPixels = max 1.0 (min 2.0 trace.Width) |> fixedText
                    yield svgElement "path" [
                        Attr.Create "data-testid" ("ta-trace-" + rowId + "-" + trace.TraceId)
                        Attr.Create "data-stroke-width-css-pixels" strokeWidthCssPixels
                        Attr.Dynamic "d" positivePath
                        Attr.Dynamic "data-last-value" lastValue
                        svgAttr "fill" "none"
                        svgAttr "stroke" traceColor
                        svgAttr "stroke-width" strokeWidthCssPixels
                        svgAttr "stroke-linejoin" "round"
                        svgAttr "stroke-linecap" "round"
                        svgAttr "vector-effect" "non-scaling-stroke"
                    ] []
                | _ -> ()

            yield markerLayer

            yield
                svgElement "line" [
                    Attr.Create "data-testid" (svgTestId + "-crosshair")
                    Attr.Create "data-ta-shared-crosshair" "true"
                    svgAttr "x1" "0"
                    svgAttr "x2" "0"
                    svgAttr "visibility" "hidden"
                    svgAttr "y1" "0"
                    svgAttr "y2" (fixedText height)
                    svgAttr "stroke" palette.Cursor
                    svgAttr "stroke-width" "1"
                    svgAttr "stroke-dasharray" "3 3"
                    svgAttr "pointer-events" "none"
                ] []

        ],
        referenceTimestamps,
        (traces
         |> Array.mapi (fun index _ ->
             fun cursorIndex ->
                  let currentReader, _, _ = readerStates[index].Value
                  currentReader cursorIndex)),
        (traces
         |> Array.mapi (fun index _ ->
             fun cursorIndex ->
                  let _, currentReader, _ = readerStates[index].Value
                  currentReader cursorIndex)),
        (traces
         |> Array.mapi (fun index _ ->
             fun () ->
                  let _, _, current = readerStates[index].Value
                  current)),
        (fun cursorIndex ->
            let placements, stripes, _, _ = markerVisualState.Value
            RendererModel.cursorEventItemsWith
                (TaDisplayTimeFormatter.fullOrOriginal (displayTime.Current ()))
                cursorIndex
                traces
                placements
                stripes)

    let compositeSvgReactivePreparedLiveWithHeight rowId isBaseRow traces preparedData dataView referenceTimestamps cursorIndex setCursorIndex commitCursorIndex chartPixelHeight scheduleValueRefresh =
        let zone = Var.Create SduiDisplayTimeZone.Utc
        compositeSvgReactivePreparedLiveWithHeightPalette lightPlotPalette { Zone = zone.View; Current = fun () -> zone.Value } rowId isBaseRow traces preparedData dataView referenceTimestamps cursorIndex setCursorIndex commitCursorIndex chartPixelHeight scheduleValueRefresh

    let compositeSvgReactivePreparedLiveWithValueRefresh rowId isBaseRow (traces: TaTraceSpec array) preparedData dataView referenceTimestamps cursorIndex setCursorIndex commitCursorIndex scheduleValueRefresh =
        let hasCandles = traces |> Array.exists (fun trace -> trace.Kind = TaTraceKind.Candlestick)
        let height = Var.Create(if hasCandles then 250 else 112)
        compositeSvgReactivePreparedLiveWithHeight rowId isBaseRow traces preparedData dataView referenceTimestamps cursorIndex setCursorIndex commitCursorIndex height scheduleValueRefresh

    let compositeSvgReactivePreparedLive rowId isBaseRow traces preparedData dataView referenceTimestamps cursorIndex setCursorIndex commitCursorIndex =
        compositeSvgReactivePreparedLiveWithValueRefresh rowId isBaseRow traces preparedData dataView referenceTimestamps cursorIndex setCursorIndex commitCursorIndex ignore

    let compositeSvgReactivePrepared rowId isBaseRow traces data referenceTimestamps cursorIndex setCursorIndex commitCursorIndex =
        let preparedData = RendererModel.prepareData data
        let dataState = Var.Create preparedData
        compositeSvgReactivePreparedLive rowId isBaseRow traces preparedData dataState.View referenceTimestamps cursorIndex setCursorIndex commitCursorIndex

    let compositeSvgReactive rowId traces data referenceTimestamps cursorIndex setCursorIndex commitCursorIndex =
        let chart, timestamps, _, _, _, _ = compositeSvgReactivePrepared rowId false traces data referenceTimestamps cursorIndex setCursorIndex commitCursorIndex
        chart, timestamps

    let compositeSvg rowId traces data referenceTimestamps cursorIndex setCursorIndex commitCursorIndex =
        let cursor = Var.Create cursorIndex
        compositeSvgReactive rowId traces data referenceTimestamps cursor.View setCursorIndex commitCursorIndex

    let renderRowReactivePreparedLiveWithHeightPalette palette (displayTime: TaRendererDisplayTime) (state: RuntimeState) (ui: TaRendererUiState) preparedData (dataView: View<TaPreparedRendererData>) visibleTimestamps cursorIndex setCursorIndex commitCursorIndex showSharedTimeAxis isBaseRow (rowHeight: Var<int>) scheduleValueRefresh registerLegendElement (row: TaRowSpec) =
        let traces =
            RendererModel.effectiveTraces row
            |> Array.filter (fun trace ->
                let key = row.RowId, trace.TraceId
                trace.Visible && not (Set.contains key ui.HiddenTraces) && not (Set.contains key ui.RemovedTraces))
        let hasCursorEventCapability =
            traces
            |> Array.exists (fun trace -> trace.Kind = TaTraceKind.Marker || trace.Kind = TaTraceKind.OverviewStripe)
        let chart, timestamps, cursorReaders, legendReaders, latestLegendReaders, markerCursorReader = compositeSvgReactivePreparedLiveWithHeightPalette palette displayTime row.RowId isBaseRow traces preparedData dataView visibleTimestamps cursorIndex setCursorIndex commitCursorIndex rowHeight scheduleValueRefresh
        let title = rowTitle row traces
        let heightBounds = RendererModel.rowHeightBounds row traces
        let cursorTag =
            div [
                Attr.Create "data-testid" ("ta-row-cursor-label-" + row.RowId)
                Attr.Create "data-ta-row-cursor-label" "true"
                Attr.Create "data-ta-row-cursor-row-id" row.RowId
                Attr.Create "data-fixed-css-overlay" "true"
                attr.style (rowCursorTagStyle "50" false)
            ] [
                span [
                    Attr.Create "data-testid" ("ta-row-cursor-date-" + row.RowId)
                    Attr.Create "data-ta-row-cursor-date" "true"
                    attr.style "display:block; width:80px; height:12px; line-height:12px; overflow:hidden;"
                ] [ text "Unavailable" ]
                span [
                    Attr.Create "data-testid" ("ta-row-cursor-time-" + row.RowId)
                    Attr.Create "data-ta-row-cursor-clock" "true"
                    attr.style "display:block; width:80px; height:12px; line-height:12px; overflow:hidden;"
                ] [ text "Unavailable" ]
            ]
        let rowPlot =
            div [
                Attr.Create "data-testid" ("ta-row-plot-shell-" + row.RowId)
                attr.style "position:relative; display:flex; flex-direction:column; min-width:0; overflow:hidden;"
            ] [
                yield div [
                    Attr.Create "data-testid" ("ta-row-cursor-gutter-" + row.RowId)
                    Attr.Create "data-fixed-height" "32"
                    Attr.Create "data-plot-surface-theme" palette.ThemeName
                    attr.style ("box-sizing:border-box; height:32px; min-height:32px; max-height:32px; flex:0 0 32px; border-bottom:1px solid " + palette.Grid + "; background:" + palette.AxisSurface + ";")
                ] []
                yield cursorTag
                yield div [
                    Attr.Create "data-testid" ("ta-row-ofi-band-" + row.RowId)
                    Attr.Create "data-ta-row-ofi-band" "true"
                    Attr.Create "data-ta-row-ofi-row-id" row.RowId
                    Attr.Create "data-ta-row-cursor-events" "true"
                    Attr.Create "data-cursor-event-capability" (if hasCursorEventCapability then "available" else "unavailable")
                    Attr.Create "data-marker-event-count" "0"
                    Attr.Create "data-fixed-height" "24"
                    attr.style ("box-sizing:border-box; display:flex; align-items:center; gap:4px; height:24px; min-height:24px; max-height:24px; flex:0 0 24px; padding:2px 8px; border-bottom:1px solid " + palette.Grid + "; background:" + palette.AxisSurface + "; overflow:hidden; white-space:nowrap; font-family:Consolas,monospace; font-size:10px; color:" + palette.LegendText + ";")
                ] [
                    yield span [
                        Attr.Create "data-ta-row-ofi-empty" "true"
                        Attr.Create "data-cursor-event-state" (if hasCursorEventCapability then "none" else "unavailable")
                        attr.style "display:inline-flex; align-items:center; height:18px; color:#708198;"
                    ] [ text "" ]
                    for index in 0 .. RendererModel.MarkerCursorItemBudget - 1 do
                        yield span [
                            Attr.Create "data-ta-row-ofi-item-index" (string index)
                            Attr.Create "hidden" "hidden"
                            attr.style ("display:inline-flex; align-items:center; max-width:220px; height:18px; padding:0 5px; border:1px solid " + palette.Border + "; border-radius:3px; overflow:hidden; text-overflow:ellipsis; white-space:nowrap;")
                        ] [ text "" ]
                    yield span [
                        Attr.Create "data-ta-row-ofi-overflow" "true"
                        Attr.Create "hidden" "hidden"
                        attr.style ("display:inline-flex; align-items:center; height:18px; padding:0 5px; border:1px solid " + palette.Border + "; border-radius:3px; white-space:nowrap;")
                    ] [ text "" ]
                ]
                yield chart
                if showSharedTimeAxis then
                    yield timeAxisWithPalette palette displayTime ("ta-time-axis-" + row.RowId) row.RowId timestamps
            ]
        let children = [ rowPlot :> Doc ]
        let metadata =
            View.Map2 (fun currentData zone -> currentData, zone) dataView displayTime.Zone
            |> View.Map (fun (currentData, zone) ->
                span [ attr.style "display:inline-flex; align-items:center; gap:6px 10px; flex:0 0 auto; flex-wrap:nowrap; white-space:nowrap;" ] [
                    for value in RendererModel.rowTemporalMetadataPrepared row currentData do
                        let availability = value.AvailableAtUtc |> Option.map (TaDisplayTimeFormatter.compactOrOriginal zone) |> Option.defaultValue "unknown"
                        let quality = value.Quality |> Option.defaultValue "unknown"
                        yield
                            span [
                                Attr.Create "data-testid" ("ta-row-meta-" + row.RowId + "-" + value.ScaleKey)
                                Attr.Create "data-scale-key" value.ScaleKey
                                Attr.Create "data-finality" value.Finality
                                Attr.Create "data-quality" quality
                                Attr.Create "data-display-time-zone" (SduiDisplayTimeZone.id zone)
                                attr.title (RendererModel.temporalDetailWith (TaDisplayTimeFormatter.fullOrOriginal zone) value)
                                attr.style "display:inline-flex; align-items:center; min-height:20px; padding:1px 6px; border:1px solid #bcc9d8; border-radius:4px; background:#f7fafc; color:#465b74; font-family:Consolas,monospace; font-size:10px; white-space:nowrap;"
                            ] [ text (value.ScaleKey + " | " + value.Finality + " | " + quality + " | frontier " + TaDisplayTimeFormatter.compactOrOriginal zone value.ObservedThroughUtc + " | available " + availability) ]
                ] :> Doc)
            |> Doc.EmbedView
        let legend =
            div [
                Attr.Create "data-testid" ("ta-row-values-" + row.RowId)
                Attr.Create "data-ta-row-values" "true"
                Attr.Create "data-auto-height" "true"
                Attr.Create "data-plot-surface-theme" palette.ThemeName
                attr.style ("box-sizing:border-box; display:flex; align-items:center; align-content:center; gap:4px 14px; min-height:30px; padding:4px 8px; border-top:1px solid " + palette.Grid + "; border-bottom:1px solid " + palette.Grid + "; background:" + palette.AxisSurface + "; overflow:visible; flex-wrap:wrap; white-space:normal; font-family:Consolas,monospace; font-size:11px; line-height:16px; color:" + palette.LegendText + ";")
                on.afterRender registerLegendElement
            ] [
                let initialPresentation =
                    if timestamps.Length = 0 then None
                    else latestLegendReaders |> Array.tryPick (fun readLatest -> readLatest ())
                let initialTimestamp =
                    initialPresentation
                    |> Option.map (fun value -> TaDisplayTimeFormatter.fullOrOriginal (displayTime.Current ()) value.Timestamp)
                    |> Option.defaultValue "Unavailable"
                yield
                    span [
                        Attr.Create "data-testid" ("ta-row-data-time-" + row.RowId)
                        Attr.Create "data-ta-row-data-time" "true"
                        Attr.Create "data-ta-row-data-time-row-id" row.RowId
                        Attr.Dynamic "data-display-time-zone" (displayTime.Zone |> View.Map SduiDisplayTimeZone.id)
                        attr.style "display:inline-block; width:19ch; min-width:19ch; max-width:19ch; overflow:hidden; white-space:nowrap; font-variant-numeric:tabular-nums; font-weight:650;"
                    ] [ text initialTimestamp ]
                for index in 0 .. traces.Length - 1 do
                    let trace = traces[index]
                    if trace.Kind <> TaTraceKind.Marker && trace.Kind <> TaTraceKind.OverviewStripe then
                        let label = if String.IsNullOrWhiteSpace trace.Label then trace.TraceId else trace.Label
                        let initialValue =
                            if timestamps.Length = 0 then "Unavailable"
                            else latestLegendReaders[index] () |> Option.map _.Value |> Option.defaultValue "Unavailable"
                        let valueWidth =
                            match trace.Kind with
                            | TaTraceKind.Candlestick -> "46ch"
                            | _ -> "16ch"
                        yield
                            span [
                                Attr.Create "data-testid" ("ta-row-value-" + row.RowId + "-" + trace.TraceId)
                                Attr.Create "data-ta-row-value-token" "true"
                                attr.style "display:inline-flex; align-items:baseline; gap:4px; flex:0 0 auto; height:20px; line-height:20px; white-space:nowrap;"
                            ] [
                                span [ Attr.Create "data-ta-row-value-label" "true"; attr.style "font-weight:650;" ] [ text label ]
                                span [
                                    Attr.Create "data-ta-row-value-index" (string index)
                                    Attr.Create "data-ta-row-value-row-id" row.RowId
                                    Attr.Create "data-ta-row-value-text" "true"
                                    Attr.Create "data-value-state" (if initialValue = "Unavailable" then "undefined" else "defined")
                                    attr.title (label + " value")
                                    attr.style ("display:inline-block; width:" + valueWidth + "; min-width:" + valueWidth + "; max-width:" + valueWidth + "; overflow:hidden; text-overflow:ellipsis; white-space:nowrap; font-variant-numeric:tabular-nums;")
                                ] [ text initialValue ]
                            ]
            ]
        let frameHeight = rowHeight.View |> View.Map (fun value -> value + 114 + if showSharedTimeAxis then 16 else 0)
        div [ Attr.Create "data-testid" ("ta-row-shell-" + row.RowId); attr.style "display:flex; flex-direction:column; min-width:0;" ] [
            chartFrame title [ metadata ] legend ("ta-row-" + row.RowId) frameHeight children
            rowResizeHandle row.RowId heightBounds rowHeight
        ], cursorReaders, legendReaders, latestLegendReaders, markerCursorReader

    let renderRowReactivePreparedLiveWithHeight state ui preparedData dataView visibleTimestamps cursorIndex setCursorIndex commitCursorIndex showSharedTimeAxis isBaseRow rowHeight scheduleValueRefresh registerLegendElement row =
        let zone = Var.Create SduiDisplayTimeZone.Utc
        renderRowReactivePreparedLiveWithHeightPalette lightPlotPalette { Zone = zone.View; Current = fun () -> zone.Value } state ui preparedData dataView visibleTimestamps cursorIndex setCursorIndex commitCursorIndex showSharedTimeAxis isBaseRow rowHeight scheduleValueRefresh registerLegendElement row

    let renderRowReactivePreparedLiveWithValueRefresh (state: RuntimeState) (ui: TaRendererUiState) preparedData (dataView: View<TaPreparedRendererData>) visibleTimestamps cursorIndex setCursorIndex commitCursorIndex showSharedTimeAxis isBaseRow scheduleValueRefresh (row: TaRowSpec) =
        let traces =
            RendererModel.effectiveTraces row
            |> Array.filter (fun trace ->
                let key = row.RowId, trace.TraceId
                trace.Visible && not (Set.contains key ui.HiddenTraces) && not (Set.contains key ui.RemovedTraces))
        let height = Var.Create((RendererModel.rowHeightBounds row traces).DefaultHeight)
        renderRowReactivePreparedLiveWithHeight state ui preparedData dataView visibleTimestamps cursorIndex setCursorIndex commitCursorIndex showSharedTimeAxis isBaseRow height scheduleValueRefresh ignore row

    let renderRowReactivePreparedLive state ui preparedData dataView visibleTimestamps cursorIndex setCursorIndex commitCursorIndex showSharedTimeAxis isBaseRow row =
        renderRowReactivePreparedLiveWithValueRefresh state ui preparedData dataView visibleTimestamps cursorIndex setCursorIndex commitCursorIndex showSharedTimeAxis isBaseRow ignore row

    let renderRowReactivePrepared (state: RuntimeState) (ui: TaRendererUiState) visibleTimestamps cursorIndex setCursorIndex commitCursorIndex showSharedTimeAxis isBaseRow (row: TaRowSpec) =
        let dataState = Var.Create(RendererModel.prepareData state.Data)
        renderRowReactivePreparedLive state ui dataState.Value dataState.View visibleTimestamps cursorIndex setCursorIndex commitCursorIndex showSharedTimeAxis isBaseRow row

    let renderRowReactive (state: RuntimeState) (ui: TaRendererUiState) visibleTimestamps cursorIndex setCursorIndex commitCursorIndex showSharedTimeAxis (row: TaRowSpec) =
        renderRowReactivePrepared state ui visibleTimestamps cursorIndex setCursorIndex commitCursorIndex showSharedTimeAxis false row
        |> fun (rowDoc, _, _, _, _) -> rowDoc

    let renderRow state ui visibleTimestamps setCursorIndex commitCursorIndex showSharedTimeAxis row =
        let cursor = Var.Create ui.CursorIndex
        renderRowReactive state ui visibleTimestamps cursor.View setCursorIndex commitCursorIndex showSharedTimeAxis row

    let renderWithProjectionCommitAndDisplayTimeZone
        (options: TaRendererOptions)
        (callbacks: TaRendererCallbacks)
        (onProjectionCommitted: RuntimeState -> unit)
        (displayTimeZone: View<SduiDisplayTimeZone>)
        (runtimeState: Var<RuntimeState>) =
        ensureAxisResizeTracking ()
        let mutable currentDisplayTimeZone = SduiDisplayTimeZone.Utc
        let displayTime =
            { Zone = displayTimeZone
              Current = fun () -> currentDisplayTimeZone }
        let mutable refreshDisplayTime: (unit -> unit) = ignore
        let rendererTelemetryInstanceId = nextRendererTelemetryInstanceId ()
        let currentCanvasId () = runtimeState.Value.Identity.CanvasInstanceId
        let rowHeightStates = System.Collections.Generic.Dictionary<string, Var<int>>()
        let rowHeightStateFor (row: TaRowSpec) (traces: TaTraceSpec array) =
            let key = RendererModel.rowHeightStorageKey (currentCanvasId ()) row.RowId
            match rowHeightStates.TryGetValue key with
            | true, value -> value
            | _ ->
                let value = Var.Create((RendererModel.rowHeightBounds row traces).DefaultHeight)
                rowHeightStates[key] <- value
                value
        let configuredEditorSchemas = if isNull options.EditorSchemas then [||] else options.EditorSchemas
        let editorSchemasNow () =
            let documentSchemas =
                runtimeState.Value.Document
                |> Option.map _.EditorSchemas
                |> Option.defaultValue [||]
                |> fun values -> if isNull values then [||] else values
            let schemas = if documentSchemas.Length > 0 then documentSchemas else configuredEditorSchemas
            schemas
            |> Array.filter (fun schema ->
                match DynamicEditorValidation.validateSchema DynamicEditorDefaults.limits schema with
                | Ok _ -> true
                | Error _ -> false)
        let initialEditorSchema = editorSchemasNow () |> Array.tryHead
        let selectedTemplate = Var.Create(initialEditorSchema |> Option.map _.TemplateKey |> Option.defaultValue "")
        let editorValues = Var.Create(initialEditorSchema |> Option.map RendererModel.initialEditorInputs |> Option.defaultValue [||])
        let mutable instrumentDraft = ""
        let mutable intervalDraft = ""
        let mutable fromDateDraft = ""
        let mutable toDateDraft = ""
        let mutable synchronizedDocumentKey: (RuntimeIdentity * int64) option = None
        let addKind = Var.Create "Sma"
        let addDataRef = Var.Create "series.sma"
        let addPeriod = Var.Create "20"
        let addDiPeriod = Var.Create "14"
        let addAdxPeriod = Var.Create "14"
        let addFastPeriod = Var.Create "12"
        let addSlowPeriod = Var.Create "26"
        let addSignalPeriod = Var.Create "9"
        let draftWindow = Var.Create<TaVisibleWindow option> None
        let mutable renderedNavigatorDraft: TaVisibleWindow option = None
        let mutable addRowSequence = 0
        let mutable pendingAddRowId: string option = None
        let mutable editingRowId: string option = None
        let mutable pendingEditorMutation: (RuntimeIdentity * int64 * string option * Set<string> * TaRowEditorBinding) option = None
        let mutable finishNavigatorDrag: (unit -> unit) option = None
        let mutable activeNavigatorCursor: string option = None
        let mutable navigatorCursorAfterRender: string option = None
        let mutable chartRenderSequence = 0
        let mutable chartRenderReason = "initial"
        let cursorIndex = Var.Create<int option> None
        let mutable chartStackElement: Element = null
        let mutable latestCursorTimestamps: string array = [||]
        let mutable latestCursorReaders: (int -> TaCursorValue option) array = [||]
        let mutable latestLegendReaders: Map<string, (int -> TaRowValuePresentation option) array> = Map.empty
        let mutable latestLegendValueReaders: Map<string, (unit -> TaRowValuePresentation option) array> = Map.empty
        let mutable latestMarkerCursorReaders: Map<string, (int -> TaMarkerCursorItem array)> = Map.empty
        let mutable cursorPanelElement: Element = null
        let mutable latestRowLegendElements: Element array = [||]
        let mutable displayedCursorIndex: int option = None
        let mutable refreshVisibleValues: (unit -> unit) = ignore
        let mutable visibleValueRefreshScheduled = false
        let mutable visibleValueSchedulerSequence = 0
        let mutable visibleValueTelemetrySequence = 0
        let mutable visibleValueTelemetryMaxTotalMs = 0.0
        let publishGlobalSchedulerTelemetry elapsedMs =
            let root = JS.Document.DocumentElement
            if not (isNull root) then
                visibleValueSchedulerSequence <- visibleValueSchedulerSequence + 1
                let previousMax =
                    match Double.TryParse(root.GetAttribute("data-visible-value-global-max-scheduler-ms")) with
                    | true, value -> value
                    | _ -> 0.0
                root.SetAttribute("data-visible-value-global-last-scheduler-ms", fixedText elapsedMs)
                root.SetAttribute("data-visible-value-global-last-instance", rendererTelemetryInstanceId)
                root.SetAttribute("data-visible-value-global-last-render-sequence", string chartRenderSequence)
                root.SetAttribute("data-visible-value-global-last-scheduler-sequence", string visibleValueSchedulerSequence)
                if elapsedMs > previousMax then
                    root.SetAttribute("data-visible-value-global-max-scheduler-ms", fixedText elapsedMs)
                    root.SetAttribute("data-visible-value-global-max-instance", rendererTelemetryInstanceId)
                    root.SetAttribute("data-visible-value-global-max-render-sequence", string chartRenderSequence)
                    root.SetAttribute("data-visible-value-global-max-scheduler-sequence", string visibleValueSchedulerSequence)
        let scheduleVisibleValueRefresh () =
            if not visibleValueRefreshScheduled then
                visibleValueRefreshScheduled <- true
                JS.RequestAnimationFrame(fun _ ->
                    let schedulerStarted = float (Date.Now())
                    visibleValueRefreshScheduled <- false
                    refreshVisibleValues ()
                    let schedulerCompleted = float (Date.Now())
                    let schedulerElapsed = schedulerCompleted - schedulerStarted
                    publishGlobalSchedulerTelemetry schedulerElapsed
                    if not (isNull chartStackElement) then
                        chartStackElement.SetAttribute("data-visible-value-scheduler-ms", fixedText schedulerElapsed)
                        chartStackElement.SetAttribute("data-visible-value-renderer-instance", rendererTelemetryInstanceId))
                |> ignore
        displayTimeZone
        |> View.Sink (fun zone ->
            currentDisplayTimeZone <- zone
            refreshDisplayTime ())
        let mutable chartWorkGeneration = 0
        let mutable dataWorkGeneration = 0
        let mutable activeRowDataStates: Var<TaPreparedRendererData> array = [||]
        let scheduleNextFrame work =
            JS.RequestAnimationFrame(fun _ -> work ()) |> ignore
        let mutable projectionCommitGate = ProjectionCommitGate.initial
        let beginProjection state =
            let nextGate, generation = ProjectionCommitGate.beginCandidate state projectionCommitGate
            projectionCommitGate <- nextGate
            generation
        let pendingProjectionFor state =
            ProjectionCommitGate.pendingGenerationFor state projectionCommitGate
        let completeProjection generation state =
            scheduleNextFrame (fun () ->
                let nextGate, committed =
                    ProjectionCommitGate.tryCommit runtimeState.Value generation state projectionCommitGate
                projectionCommitGate <- nextGate
                committed |> Option.iter onProjectionCommitted)
        let scheduleRowDataRefresh projectionCandidateGeneration candidateState prepared =
            dataWorkGeneration <- dataWorkGeneration + 1
            let generation = dataWorkGeneration
            let targets = activeRowDataStates
            let rec update index =
                if generation = dataWorkGeneration then
                    if index < targets.Length then
                        scheduleNextFrame (fun () ->
                            if generation = dataWorkGeneration then
                                targets[index].Value <- prepared
                                update (index + 1))
                    else
                        completeProjection projectionCandidateGeneration candidateState
            update 0
        let mutable pendingCursorIndex: int option option = None
        let mutable cursorFrameScheduled = false
        let mutable pendingCursorRequestedAtMs = 0.0
        let mutable cursorRenderLatencySequence = 0
        let crossScaleSummaryOpen = Var.Create false
        let uiState =
            Var.Create
                { Window = { StartIndex = 0; Count = options.DefaultVisibleBars }
                  FollowLatest = true
                  HiddenRows = Set.empty
                  HiddenTraces = Set.empty
                  RemovedTraces = Set.empty
                  AddRowOpen = false
                  CursorIndex = None
                  PendingActionId = None
                  Feedback = "" }
        let sameChartUiState (left: TaRendererUiState) (right: TaRendererUiState) =
            left.Window = right.Window
            && left.FollowLatest = right.FollowLatest
            && left.HiddenRows = right.HiddenRows
            && left.HiddenTraces = right.HiddenTraces
            && left.RemovedTraces = right.RemovedTraces
        let chartUiState = Var.Create uiState.Value
        let mutable pendingViewportChartState: TaRendererUiState option = None
        let mutable viewportChartFrameScheduled = false
        let scheduleViewportChartState next =
            pendingViewportChartState <- Some next
            if not viewportChartFrameScheduled then
                viewportChartFrameScheduled <- true
                // Keep the lightweight controls/navigator responsive for one paint before
                // rebuilding the chart stack for the committed viewport.
                JS.RequestAnimationFrame(fun _ ->
                    JS.RequestAnimationFrame(fun _ ->
                        viewportChartFrameScheduled <- false
                        match pendingViewportChartState with
                        | Some pending ->
                            pendingViewportChartState <- None
                            if not (sameChartUiState chartUiState.Value pending) then
                                chartRenderReason <- "viewport"
                                chartUiState.Value <- pending
                        | None -> ())
                    |> ignore)
                |> ignore
        let setViewportUiState next =
            uiState.Value <- next
            if not (sameChartUiState chartUiState.Value next) then
                scheduleViewportChartState next
        let setUiState next =
            let previousChartState = chartUiState.Value
            uiState.Value <- next
            match pendingViewportChartState with
            | Some pending when sameChartUiState pending next ->
                pendingViewportChartState <- Some next
            | _ ->
                pendingViewportChartState <- None
                if not (sameChartUiState previousChartState next) then
                    chartRenderReason <- "ui-state"
                    chartUiState.Value <- next
        let mutable defaultViewportAppliedToCanvas: string option = None
        let maximumVisibleBarsFor document =
            RendererModel.documentMaximumVisibleBars options.MaximumVisibleBars document.DefaultView

        let coverageIdentity (state: RuntimeState) (document: TaWorkspaceDocument) =
            RendererModel.tryLoadedCoverageResolved document.DefaultView state.Data
            |> Option.map _.CoverageIdentity

        let viewportScopeKey (state: RuntimeState) (document: TaWorkspaceDocument) =
            let canvasKey = canvasIdText state.Identity.CanvasInstanceId
            canvasKey + "|" + (coverageIdentity state document |> Option.defaultValue "legacy")

        let coverageProjection (state: RuntimeState) =
            state.Document
            |> Option.bind (fun document -> RendererModel.tryLoadedCoverageResolved document.DefaultView state.Data)

        let applyDocumentDefaultViewport (state: RuntimeState) prepared =
            match state.Document with
            | None -> ()
            | Some document ->
                let scopeKey = viewportScopeKey state document
                if defaultViewportAppliedToCanvas <> Some scopeKey then
                    let total = RendererModel.referenceTimelineForDocumentPrepared document prepared |> Array.length
                    if total > 0 then
                        let maximumVisibleBars = maximumVisibleBarsFor document
                        let window =
                            RendererModel.initialViewportWindow
                                options.MinimumVisibleBars
                                options.DefaultVisibleBars
                                maximumVisibleBars
                                total
                                document.DefaultView
                        setUiState
                            { uiState.Value with
                                Window = window
                                FollowLatest = true
                                CursorIndex = None }
                        cursorIndex.Value <- None
                        defaultViewportAppliedToCanvas <- Some scopeKey
        let mutable actionSequence = 0
        let mutable querySelectionGeneration = 0
        let mutable queryInFlight = false
        let mutable queuedQuery: (TaQueryChange * int) option = None
        let mutable pendingBoundaryPan: TaPendingBoundaryPan option = None
        let mutable queuedViewportIntent: TaQueuedViewportIntent option = None
        let mutable flushQueuedViewportIntent = ignore
        let mutable dispatchAdjacentCoverage = fun (_: PulseTrade.Comm.Spa.Dynamic.Renderer.TaCoverageDirection) (_: int) -> ()
        let mutable dispatchCoverageWindow = fun (_: int64) (_: int) (_: string) -> ()
        let preparedRowsReady = Var.Create false
        let viewportDataReady = Var.Create false
        let commandsDisabledView =
            View.Map2
                (fun state ui -> remoteDisabled state.Poll || ui.PendingActionId.IsSome)
                runtimeState.View
                uiState.View
        let commandsDisabledNow () =
            remoteDisabled runtimeState.Value.Poll || uiState.Value.PendingActionId.IsSome
        let visibleRangeActionAllowed (state: RuntimeState) =
            state.Document
            |> Option.map _.AllowedActions
            |> Option.defaultValue [||]
            |> Array.contains "visible-range-changed"
        let viewportCommandsDisabledView =
            View.Map2
                (fun disabled ready -> disabled || not ready)
                (runtimeState.View |> View.Map (fun state -> localViewportDisabled state.Poll))
                viewportDataReady.View
        let viewportCommandsDisabledNow () =
            not viewportDataReady.Value
            || localViewportDisabled runtimeState.Value.Poll
        let startActionWithFeedback action successText onAccepted onRejected afterSettled =
            actionSequence <- actionSequence + 1
            let request =
                { RequestId = canvasIdText (currentCanvasId ()) + ":ui:" + string actionSequence
                  ExpectedDocumentRevision = Some runtimeState.Value.DocumentRevision
                  Action = action }
            submit
                callbacks
                uiState
                runtimeState.Value.DocumentRevision
                request
                successText
                onAccepted
                onRejected
                (fun () ->
                    afterSettled ()
                    scheduleNextFrame flushQueuedViewportIntent)
        let startActionWith action successText onAccepted onRejected =
            startActionWithFeedback action successText (fun () -> onAccepted (); None) onRejected ignore
        let startAction action successText onAccepted =
            startActionWith action successText onAccepted ignore
        let startCoverageWindowAction action pending successText =
            pendingBoundaryPan <- Some pending
            startActionWith
                action
                successText
                ignore
                (fun () -> pendingBoundaryPan <- None)
        let sendOrQueueVisibleRangeAction action =
            if uiState.Value.PendingActionId.IsSome || remoteDisabled runtimeState.Value.Poll then
                queuedViewportIntent <- Some(TaQueuedViewportIntent.LocalRange action)
            else
                startAction action "Visible range synchronized." ignore
        let sendOrQueueCoverageWindowAction targetStart targetCount successText =
            if uiState.Value.PendingActionId.IsSome || remoteDisabled runtimeState.Value.Poll then
                queuedViewportIntent <- Some(TaQueuedViewportIntent.CoverageWindow(targetStart, targetCount, successText))
                setUiState { uiState.Value with Feedback = "Loaded coverage request queued." }
            else
                dispatchCoverageWindow targetStart targetCount successText
        flushQueuedViewportIntent <- fun () ->
            match queuedViewportIntent with
            | Some intent when uiState.Value.PendingActionId.IsNone && not (remoteDisabled runtimeState.Value.Poll) ->
                queuedViewportIntent <- None
                match intent with
                | TaQueuedViewportIntent.LocalRange action ->
                    startAction action "Visible range synchronized." ignore
                | TaQueuedViewportIntent.AdjacentCoverage(direction, delta) ->
                    dispatchAdjacentCoverage direction delta
                | TaQueuedViewportIntent.CoverageWindow(targetStart, targetCount, successText) ->
                    dispatchCoverageWindow targetStart targetCount successText
            | _ -> ()

        View.Map2
            (fun state ui -> state.Poll, ui.PendingActionId)
            runtimeState.View
            uiState.View
        |> View.Sink (fun _ -> scheduleNextFrame flushQueuedViewportIntent)

        let chartRuntimeState = Var.Create runtimeState.Value
        let initialPreparedData =
            { RawData = Map.empty
              ResolvedAxes = Map.empty
              ResolvedSeries = Map.empty }
        let mutable latestPreparedData = initialPreparedData
        let shellPreparedData = Var.Create initialPreparedData
        let chartShellPreparedData = Var.Create initialPreparedData
        let mutable preparedDataReady = false
        let mutable preparationGeneration = 0
        let mutable observedChartTopology = chartTopologySignaturePrepared runtimeState.Value initialPreparedData
        let mutable observedDataState = runtimeState.Value

        let acceptPreparedDataCore refreshRows (next: RuntimeState) nextPreparedData =
            let nextChartTopology = chartTopologySignaturePrepared next nextPreparedData
            let viewportScopeChanged =
                next.Document
                |> Option.map (fun document -> defaultViewportAppliedToCanvas <> Some(viewportScopeKey next document))
                |> Option.defaultValue false
            let topologyChanged =
                next.Identity <> chartRuntimeState.Value.Identity
                || not (sameDocumentPresentation chartRuntimeState.Value next)
                || nextChartTopology <> observedChartTopology
            if topologyChanged then
                beginProjection next |> ignore
                chartRenderReason <-
                    if next.Identity <> chartRuntimeState.Value.Identity then "identity"
                    elif next.DocumentRevision <> chartRuntimeState.Value.DocumentRevision then "document-revision"
                    else "topology-signature"
                observedChartTopology <- nextChartTopology
                shellPreparedData.Value <- nextPreparedData
                chartShellPreparedData.Value <- nextPreparedData
                match next.Document with
                | Some document ->
                    let maximumVisibleBars = maximumVisibleBarsFor document
                    let nextTimeline = RendererModel.referenceTimelineForDocumentPrepared document nextPreparedData
                    let currentUi = uiState.Value
                    let generalOldTimeline = RendererModel.referenceTimelineForDocumentPrepared document latestPreparedData
                    let generalOldWindow =
                        RendererModel.resolveWindow
                            options.MinimumVisibleBars
                            maximumVisibleBars
                            generalOldTimeline.Length
                            currentUi.FollowLatest
                            currentUi.Window
                    let reanchored =
                        if viewportScopeChanged then
                            pendingBoundaryPan <- None
                            Some(
                                RendererModel.initialViewportWindow
                                    options.MinimumVisibleBars
                                    options.DefaultVisibleBars
                                    maximumVisibleBars
                                    nextTimeline.Length
                                    document.DefaultView)
                        else
                            match pendingBoundaryPan with
                            | Some pending ->
                                match RendererModel.tryLoadedCoverageResolved document.DefaultView next.Data with
                                | Some projection
                                    when projection.QueryGeneration >= pending.QueryGeneration
                                         && (pending.TargetStartObservationOrdinal
                                             |> Option.forall ((=) projection.ActiveDetail.StartObservationOrdinal)) ->
                                    pendingBoundaryPan <- None
                                    Some(
                                        RendererModel.clampWindow
                                            options.MinimumVisibleBars
                                            maximumVisibleBars
                                            nextTimeline.Length
                                            { StartIndex = 0
                                              Count = min pending.ObservationCount nextTimeline.Length })
                                | _ when RendererModel.coverageExtended pending.Direction pending.IntentTimeline nextTimeline ->
                                    pendingBoundaryPan <- None
                                    RendererModel.tryReanchorWindow
                                        options.MinimumVisibleBars
                                        maximumVisibleBars
                                        pending.LegacyDelta
                                        pending.IntentTimeline
                                        nextTimeline
                                        pending.IntentWindow
                                | _ -> None
                            | _ when not currentUi.FollowLatest
                                     && generalOldTimeline.Length > 0
                                     && nextTimeline.Length > generalOldTimeline.Length ->
                                RendererModel.tryReanchorWindow
                                    options.MinimumVisibleBars
                                    maximumVisibleBars
                                    0
                                    generalOldTimeline
                                    nextTimeline
                                    generalOldWindow
                            | _ -> None
                    match reanchored with
                    | Some window ->
                        let followLatest = window.StartIndex = RendererModel.viewportMaximumStart nextTimeline.Length window
                        setUiState
                            { currentUi with
                                Window = window
                                FollowLatest = followLatest
                                CursorIndex = None }
                        cursorIndex.Value <- None
                        if viewportScopeChanged then
                            defaultViewportAppliedToCanvas <- Some(viewportScopeKey next document)
                    | None -> ()
                | None -> pendingBoundaryPan <- None
                chartRuntimeState.Value <- next
            elif refreshRows then
                let projectionCandidateGeneration = beginProjection next
                shellPreparedData.Value <- nextPreparedData
                scheduleRowDataRefresh projectionCandidateGeneration next nextPreparedData
            latestPreparedData <- nextPreparedData
            observedDataState <- next

        let acceptPreparedData refreshRows (next: RuntimeState) nextPreparedData =
            let staleCoverageCandidate =
                match pendingBoundaryPan, next.Document with
                | Some pending, Some document ->
                    RendererModel.isStaleCoverageCandidate pending.QueryGeneration document.DefaultView
                | _ -> false

            if not staleCoverageCandidate then
                acceptPreparedDataCore refreshRows next nextPreparedData

        let scheduleFullPreparation () =
            preparationGeneration <- preparationGeneration + 1
            let generation = preparationGeneration
            preparedDataReady <- false
            viewportDataReady.Value <- false
            let candidate = runtimeState.Value
            chartRuntimeState.Value <- candidate
            let data = candidate.Data
            RendererModel.prepareDataScheduled
                scheduleNextFrame
                data
                (fun prepared ->
                    if generation = preparationGeneration then
                        let current = runtimeState.Value
                        // A scheduled candidate belongs to the exact data snapshot it decoded.
                        // Never pair stale prepared rows with a newer runtime envelope.
                        if not (runtimeDataChanged candidate current) then
                            latestPreparedData <- prepared
                            shellPreparedData.Value <- prepared
                            chartShellPreparedData.Value <- prepared
                            observedChartTopology <- chartTopologySignaturePrepared current prepared
                            observedDataState <- current
                            preparedDataReady <- true
                            viewportDataReady.Value <- true
                            applyDocumentDefaultViewport current prepared
                            beginProjection current |> ignore
                            chartRuntimeState.Value <- current)

        let scheduleIncrementalPreparation next =
            preparationGeneration <- preparationGeneration + 1
            let generation = preparationGeneration
            let previous = latestPreparedData
            let previousCoverageProjection =
                observedDataState.Document
                |> Option.bind (fun document ->
                    RendererModel.tryLoadedCoverageResolved document.DefaultView previous.RawData)
            observedDataState <- next
            let coverageChanged = previousCoverageProjection <> coverageProjection next
            let accept prepared =
                if generation = preparationGeneration then
                    let current = runtimeState.Value
                    if not (runtimeDataChanged next current) then
                        // Document/view metadata may advance without scheduling another data preparation.
                        // Pair the completed candidate with the latest compatible runtime envelope so
                        // metadata can advance without attaching stale decoded data.
                        acceptPreparedData true current prepared
                        if coverageChanged then
                            chartShellPreparedData.Value <- prepared
            if coverageChanged then
                // An active-detail page is a replacement, not an append-only patch. Reusing the
                // previous prepared map would retain observations absent from the accepted page.
                RendererModel.prepareDataScheduled scheduleNextFrame next.Data accept
            else
                RendererModel.prepareDataIncrementalScheduled scheduleNextFrame previous next.Data accept

        runtimeState.View
        |> View.Sink (fun next ->
            let dataChanged = runtimeDataChanged observedDataState next
            let viewportScopeChanged =
                next.Document
                |> Option.map (fun document -> defaultViewportAppliedToCanvas <> Some(viewportScopeKey next document))
                |> Option.defaultValue false
            if next.Identity <> observedDataState.Identity then
                pendingBoundaryPan <- None
                queuedViewportIntent <- None
                setUiState
                    { uiState.Value with
                        HiddenRows = Set.empty
                        HiddenTraces = Set.empty
                        RemovedTraces = Set.empty
                        CursorIndex = None }
                cursorIndex.Value <- None
                observedDataState <- next
                scheduleFullPreparation ()
            elif not preparedDataReady then
                if dataChanged then
                    observedDataState <- next
                    scheduleFullPreparation ()
                else
                    observedDataState <- next
            else
                if dataChanged then
                    if viewportScopeChanged then
                        queuedViewportIntent <- None
                        pendingBoundaryPan <- None
                        observedDataState <- next
                        scheduleFullPreparation ()
                    else
                        scheduleIncrementalPreparation next
                else
                    acceptPreparedData false next latestPreparedData
            flushQueuedViewportIntent ())
        scheduleFullPreparation ()
        let chartRuntimeView: View<RuntimeState> = chartRuntimeState.View

        let actionAllowed actionName =
            runtimeState.Value.Document
            |> Option.map _.AllowedActions
            |> Option.defaultValue [||]
            |> Array.contains actionName

        let referenceLength () =
            match runtimeState.Value.Document with
            | None -> 0
            | Some document -> RendererModel.referenceTimelineForDocumentPrepared document latestPreparedData |> Array.length

        let maximumVisibleBarsNow () =
            runtimeState.Value.Document
            |> Option.map maximumVisibleBarsFor
            |> Option.defaultValue options.MaximumVisibleBars

        let resolvedWindow ui =
            RendererModel.resolveWindow
                options.MinimumVisibleBars
                (maximumVisibleBarsNow ())
                (referenceLength ())
                ui.FollowLatest
                ui.Window

        let navigatorRatios window =
            match runtimeState.Value.Document with
            | None -> RendererModel.selectionRatios (referenceLength ()) window
            | Some document ->
                let timeline = RendererModel.referenceTimelineForDocumentPrepared document latestPreparedData
                match RendererModel.tryLoadedCoverageResolved document.DefaultView runtimeState.Value.Data with
                | Some projection ->
                    RendererModel.overviewSelectionRatios projection timeline window
                    |> Option.defaultWith (fun () ->
                        RendererModel.coverageNavigatorWindow timeline.Length window projection
                        |> Option.map (fun (_, total, globalWindow) -> RendererModel.selectionRatios total globalWindow)
                        |> Option.defaultValue (RendererModel.selectionRatios timeline.Length window))
                | None -> RendererModel.selectionRatios timeline.Length window

        let commitLocalWindow followLatest window =
            let current = uiState.Value
            let total = referenceLength ()
            let bounded = RendererModel.resolveWindow options.MinimumVisibleBars (maximumVisibleBarsNow ()) total followLatest window
            let changed = bounded <> resolvedWindow current || followLatest <> current.FollowLatest
            setViewportUiState
                { current with
                    Window = bounded
                    FollowLatest = followLatest
                    CursorIndex = None }
            cursorIndex.Value <- None
            draftWindow.Value <- None
            changed, bounded

        let setWindow followLatest window =
            if not (localViewportDisabled runtimeState.Value.Poll) then
                let changed, bounded = commitLocalWindow followLatest window
                if changed && actionAllowed "visible-range-changed" then
                    match runtimeState.Value.Document with
                    | Some document ->
                        match RendererModel.visibleEventRangePrepared document latestPreparedData bounded with
                        | Some range ->
                            sendOrQueueVisibleRangeAction
                                (SduiAction.VisibleRangeChanged(
                                    currentCanvasId (),
                                    { BaseRowId = range.BaseRowId
                                      StartEventTimeUtc = range.StartEventTimeUtc
                                      EndEventTimeExclusiveUtc = range.EndEventTimeExclusiveUtc
                                      MaximumBasePoints = min DynamicRuntimeDefaults.MaximumVisibleRangeBasePoints (max 1 (maximumVisibleBarsFor document))
                                      CoverageIntent = None }))
                        | None -> ()
                    | None -> ()

        let dispatchAdjacentCoverageNow direction delta =
            if actionAllowed "visible-range-changed" && preparedRowsReady.Value && not (localViewportDisabled runtimeState.Value.Poll) then
                match runtimeState.Value.Document with
                | Some document ->
                    match
                        RendererModel.tryAdjacentCoverageRange
                            direction
                            (min DynamicRuntimeDefaults.MaximumVisibleRangeBasePoints (max 1 (maximumVisibleBarsFor document)))
                            document
                            latestPreparedData
                    with
                    | Some change ->
                        let timeline = RendererModel.referenceTimelineForDocumentPrepared document latestPreparedData
                        let maximumVisibleBars = maximumVisibleBarsFor document
                        let window =
                            RendererModel.resolveWindow
                                options.MinimumVisibleBars
                                maximumVisibleBars
                                timeline.Length
                                uiState.Value.FollowLatest
                                uiState.Value.Window
                        let projection = RendererModel.tryLoadedCoverageResolved document.DefaultView runtimeState.Value.Data
                        let count = max 1 (min maximumVisibleBars window.Count)
                        let coverageIntent =
                            match change.CoverageIntent with
                            | Some seed when seed.RangeAuthority = TaCoverageRangeAuthority.ProviderOpenEarlier ->
                                match projection with
                                | Some coverage ->
                                    TaLoadedCoverageCodec.tryProviderOpenEarlierWindowIntent
                                        (Some coverage.CoverageRevision)
                                        (coverage.QueryGeneration + 1L)
                                        count
                                | None ->
                                    TaLoadedCoverageCodec.tryProviderOpenEarlierWindowIntent None 0L count
                            | _ ->
                                projection
                                |> Option.bind (fun coverage ->
                                    RendererModel.tryAdjacentCoverageIntent direction maximumVisibleBars timeline.Length window coverage)
                        let targetStart = coverageIntent |> Option.bind _.StartObservationOrdinal
                        let queryGeneration = coverageIntent |> Option.map _.QueryGeneration |> Option.defaultValue 0L
                        let request =
                            { change with
                                MaximumBasePoints = count
                                CoverageIntent = coverageIntent }
                        pendingBoundaryPan <-
                            Some
                                { Direction = direction
                                  LegacyDelta = delta
                                  IntentTimeline = timeline
                                  IntentWindow = window
                                  TargetStartObservationOrdinal = targetStart
                                  ObservationCount = count
                                  QueryGeneration = queryGeneration }
                        startActionWith
                            (SduiAction.VisibleRangeChanged(currentCanvasId (), request))
                            (if direction = PulseTrade.Comm.Spa.Dynamic.Renderer.TaCoverageDirection.Earlier then "Earlier coverage requested." else "Later coverage requested.")
                            ignore
                            (fun () -> pendingBoundaryPan <- None)
                    | None ->
                        setUiState
                            { uiState.Value with
                                Feedback =
                                    if direction = PulseTrade.Comm.Spa.Dynamic.Renderer.TaCoverageDirection.Earlier then
                                        "Earlier coverage is outside the configured query boundary."
                                    else
                                        "Later coverage is outside the configured query boundary." }
                | None -> ()

        dispatchAdjacentCoverage <- dispatchAdjacentCoverageNow

        let requestAdjacentCoverage direction delta =
            if actionAllowed "visible-range-changed" && preparedRowsReady.Value && not (localViewportDisabled runtimeState.Value.Poll) then
                if uiState.Value.PendingActionId.IsSome || remoteDisabled runtimeState.Value.Poll then
                    queuedViewportIntent <- Some(TaQueuedViewportIntent.AdjacentCoverage(direction, delta))
                    setUiState
                        { uiState.Value with
                            Feedback =
                                if direction = PulseTrade.Comm.Spa.Dynamic.Renderer.TaCoverageDirection.Earlier then
                                    "Earlier coverage queued."
                                else
                                    "Later coverage queued." }
                else
                    dispatchAdjacentCoverage direction delta

        let panWindow delta =
            let current = uiState.Value
            let total = referenceLength ()
            let visible = resolvedWindow current
            let requestedStart = visible.StartIndex + delta
            let maximumStart = RendererModel.viewportMaximumStart total visible
            if requestedStart < 0 then
                if remoteDisabled runtimeState.Value.Poll then
                    setWindow false { visible with StartIndex = 0 }
                else
                    requestAdjacentCoverage PulseTrade.Comm.Spa.Dynamic.Renderer.TaCoverageDirection.Earlier delta
            elif requestedStart > maximumStart then
                if remoteDisabled runtimeState.Value.Poll then
                    setWindow true { visible with StartIndex = maximumStart }
                else
                    requestAdjacentCoverage PulseTrade.Comm.Spa.Dynamic.Renderer.TaCoverageDirection.Later delta
            else
                let candidate =
                    RendererModel.clampWindow
                        options.MinimumVisibleBars
                        (maximumVisibleBarsNow ())
                        total
                        { visible with StartIndex = requestedStart }
                let followLatest = candidate.StartIndex = RendererModel.viewportMaximumStart total candidate
                setWindow followLatest candidate

        let zoomWindow delta =
            let current = uiState.Value
            let visible = resolvedWindow current
            setWindow current.FollowLatest { visible with Count = visible.Count + delta }

        let resetWindow () =
            setWindow true { StartIndex = 0; Count = options.DefaultVisibleBars }
            setUiState { uiState.Value with Feedback = "Local view reset." }

        let setWindowCount count =
            let total = referenceLength ()
            let boundedCount = max options.MinimumVisibleBars (min total count)
            setWindow true { StartIndex = max 0 (total - boundedCount); Count = boundedCount }

        let applyLoadedCoverageIntent
            (document: TaWorkspaceDocument)
            (timeline: string array)
            (currentWindow: TaVisibleWindow)
            (projection: TaLoadedCoverageProjection)
            (intent: TaCoverageWindowIntent)
            successText =
            let targetStart = intent.StartObservationOrdinal |> Option.defaultValue 0L
            let activeStart = projection.ActiveDetail.StartObservationOrdinal
            let activeEnd = activeStart + int64 timeline.Length
            let targetEnd = targetStart + int64 intent.ObservationCount
            if targetStart >= activeStart && targetEnd <= activeEnd then
                setWindow
                    (targetEnd = TaLoadedCoverageCodec.observationDomainCount projection)
                    { StartIndex = int (targetStart - activeStart)
                      Count = intent.ObservationCount }
            else
                let currentGlobalStart = activeStart + int64 currentWindow.StartIndex
                let direction =
                    if targetStart < currentGlobalStart then
                        PulseTrade.Comm.Spa.Dynamic.Renderer.TaCoverageDirection.Earlier
                    else
                        PulseTrade.Comm.Spa.Dynamic.Renderer.TaCoverageDirection.Later
                match RendererModel.tryAdjacentCoverageRange direction intent.ObservationCount document latestPreparedData with
                | Some range ->
                    let pending =
                        { Direction = direction
                          LegacyDelta = if direction = PulseTrade.Comm.Spa.Dynamic.Renderer.TaCoverageDirection.Earlier then -currentWindow.Count else currentWindow.Count
                          IntentTimeline = timeline
                          IntentWindow = currentWindow
                          TargetStartObservationOrdinal = intent.StartObservationOrdinal
                          ObservationCount = intent.ObservationCount
                          QueryGeneration = intent.QueryGeneration }
                    let action =
                        SduiAction.VisibleRangeChanged(
                            currentCanvasId (),
                            { range with
                                MaximumBasePoints = intent.ObservationCount
                                CoverageIntent = Some intent })
                    startCoverageWindowAction action pending successText
                | None ->
                    setUiState { uiState.Value with Feedback = "Loaded coverage is outside the configured query boundary." }

        dispatchCoverageWindow <- fun targetStart targetCount successText ->
            match runtimeState.Value.Document with
            | Some document ->
                let timeline = RendererModel.referenceTimelineForDocumentPrepared document latestPreparedData
                let currentWindow = resolvedWindow uiState.Value
                match RendererModel.tryLoadedCoverageResolved document.DefaultView runtimeState.Value.Data with
                | Some projection ->
                    match RendererModel.tryLoadedCoverageWindowIntentAt projection targetStart targetCount with
                    | Some intent ->
                        applyLoadedCoverageIntent document timeline currentWindow projection intent successText
                    | None ->
                        setUiState { uiState.Value with Feedback = "Loaded coverage target is no longer available." }
                | None ->
                    setUiState { uiState.Value with Feedback = "Loaded coverage is unavailable." }
            | None -> ()

        let withLoadedCoverage operation =
            match runtimeState.Value.Document with
            | Some document ->
                let timeline = RendererModel.referenceTimelineForDocumentPrepared document latestPreparedData
                let currentWindow = resolvedWindow uiState.Value
                match RendererModel.tryLoadedCoverageResolved document.DefaultView runtimeState.Value.Data with
                | Some projection -> operation document timeline currentWindow projection
                | None -> setWindowCount (maximumVisibleBarsFor document)
            | None -> ()

        let showLoadedCoverage () =
            withLoadedCoverage (fun document timeline currentWindow projection ->
                    match RendererModel.tryViewAllCoverageIntent (maximumVisibleBarsFor document) projection with
                    | Some intent ->
                        intent.StartObservationOrdinal
                        |> Option.iter (fun start ->
                            sendOrQueueCoverageWindowAction
                                start
                                intent.ObservationCount
                                "Loaded coverage requested.")
                    | None -> setWindowCount (maximumVisibleBarsFor document)
                )

        let jumpToLoadedCoverageEdge edge =
            withLoadedCoverage (fun document timeline currentWindow projection ->
                match
                    RendererModel.tryLoadedCoverageEdgeIntent
                        edge
                        currentWindow.Count
                        (maximumVisibleBarsFor document)
                        projection
                with
                | Some intent ->
                    intent.StartObservationOrdinal
                    |> Option.iter (fun start ->
                        sendOrQueueCoverageWindowAction
                            start
                            intent.ObservationCount
                            (if edge = TaLoadedCoverageEdge.Start then "Loaded start requested." else "Loaded end requested."))
                | None -> setUiState { uiState.Value with Feedback = "Loaded coverage is unavailable." })

        let startNavigatorDrag (navigatorRoot: Element) (rawEvent: Event) =
            if not (isNull navigatorRoot) then
                let event = rawEvent :?> MouseEvent
                let setDragDiagnostic name value =
                    navigatorRoot.SetAttribute(name, value)
                    if not (isNull chartStackElement) then chartStackElement.SetAttribute(name, value)
                setDragDiagnostic "data-drag-handler-invoked" "true"
                setDragDiagnostic "data-drag-mode" "pending"
                setDragDiagnostic "data-drag-last-delta" "0"
                setDragDiagnostic "data-drag-outcome" "started"

                if not preparedRowsReady.Value || localViewportDisabled runtimeState.Value.Poll then
                    setDragDiagnostic "data-drag-outcome" "disabled"
                else
                    let bounds = navigatorRoot.GetBoundingClientRect()
                    let localTotal = referenceLength ()
                    let localCommitted = resolvedWindow uiState.Value
                    let loadedDragDomain =
                        match runtimeState.Value.Document with
                        | Some document ->
                            RendererModel.tryLoadedCoverageResolved document.DefaultView runtimeState.Value.Data
                            |> Option.bind (fun projection ->
                                RendererModel.coverageNavigatorWindow localTotal localCommitted projection
                                |> Option.map (fun (_, total, committed) -> document, projection, total, committed))
                        | None -> None
                    let total, committed =
                        loadedDragDomain
                        |> Option.map (fun (_, _, total, committed) -> total, committed)
                        |> Option.defaultValue (localTotal, localCommitted)
                    let pointerX = float event.ClientX - bounds.Left
                    let ratios = navigatorRatios localCommitted
                    renderedNavigatorDraft <- None
                    draftWindow.Value <- None
                    setDragDiagnostic "data-drag-domain" (if loadedDragDomain.IsSome then "global-loaded" else "active-detail")
                    setDragDiagnostic "data-drag-domain-count" (string total)

                    match RendererModel.navigatorDragMode bounds.Width 24.0 ratios pointerX with
                    | None ->
                        setDragDiagnostic "data-drag-mode" "none"
                        setDragDiagnostic "data-drag-outcome" "outside-selection"
                    | Some drag ->
                        setDragDiagnostic "data-drag-mode" drag
                        setDragDiagnostic "data-drag-outcome" "tracking"
                        event.PreventDefault()
                        event.StopPropagation()
                        activeNavigatorCursor <- Some "grabbing"
                        (navigatorRoot |> As<HTMLElement>).Style.SetProperty("cursor", "grabbing")
                        let startClientX = event.ClientX
                        let mutable latestRawDelta = 0
                        let mutable moveHandler: Action<Event> = null
                        let mutable upHandler: Action<Event> = null
                        let mutable cancelHandler: Action<Event> = null
                        let mutable finished = false
                        let mutable pendingDraft: TaVisibleWindow option = None
                        let mutable draftFrameScheduled = false
                        let pointerId: int = JS.Get "pointerId" rawEvent
                        let mutable documentFallback = false

                        let publishDraftOnFrame () =
                            if not finished then
                                draftFrameScheduled <- false
                                setDragDiagnostic "data-drag-last-delta" (string latestRawDelta)
                                setDragDiagnostic "data-drag-outcome" "moving"
                                match pendingDraft with
                                | Some draft -> draftWindow.Value <- Some draft
                                | None -> ()

                        let scheduleDraft draft =
                            pendingDraft <- Some draft
                            if not draftFrameScheduled then
                                draftFrameScheduled <- true
                                JS.RequestAnimationFrame(fun _ -> publishDraftOnFrame ()) |> ignore

                        let cleanup () =
                            if documentFallback then
                                if not (isNull moveHandler) then JS.Document.RemoveEventListener("pointermove", moveHandler)
                                if not (isNull upHandler) then JS.Document.RemoveEventListener("pointerup", upHandler)
                                if not (isNull cancelHandler) then JS.Document.RemoveEventListener("pointercancel", cancelHandler)
                            else
                                if not (isNull moveHandler) then navigatorRoot.RemoveEventListener("pointermove", moveHandler)
                                if not (isNull upHandler) then navigatorRoot.RemoveEventListener("pointerup", upHandler)
                                if not (isNull cancelHandler) then navigatorRoot.RemoveEventListener("pointercancel", cancelHandler)
                                try navigatorRoot.ReleasePointerCapture(pointerId) with _ -> ()
                            let restoredCursor = if drag = TaWindowDrag.Move then "grab" else "ew-resize"
                            (navigatorRoot |> As<HTMLElement>).Style.SetProperty("cursor", restoredCursor)
                            JS.RequestAnimationFrame(fun _ ->
                                if not (isNull chartStackElement) then
                                    let currentNavigator = chartStackElement.QuerySelector("[data-testid='ta-overview-navigator']")
                                    if not (isNull currentNavigator) then
                                        (currentNavigator |> As<HTMLElement>).Style.SetProperty("cursor", restoredCursor))
                            |> ignore

                        let finish () =
                            if not finished then
                                finished <- true
                                finishNavigatorDrag <- None
                                activeNavigatorCursor <- None
                                let preferPendingBoundary =
                                    loadedDragDomain.IsSome
                                    && (RendererModel.navigatorBoundaryDirection total committed drag latestRawDelta |> Option.isSome)
                                let draft =
                                    RendererModel.releaseNavigatorDraft
                                        committed
                                        preferPendingBoundary
                                        renderedNavigatorDraft
                                        draftWindow.Value
                                        pendingDraft
                                let requestedStart = committed.StartIndex + latestRawDelta
                                setDragDiagnostic "data-drag-committed-start" (string committed.StartIndex)
                                setDragDiagnostic "data-drag-draft-start" (string draft.StartIndex)
                                setDragDiagnostic "data-drag-requested-start" (string requestedStart)
                                match loadedDragDomain with
                                | Some(document, projection, _, _) when draft <> committed ->
                                    match RendererModel.tryLocalWindowForLoadedCoverage localTotal projection draft with
                                    | Some localDraft ->
                                        let followLatest, next =
                                            RendererModel.commitWindowBounds
                                                options.MinimumVisibleBars
                                                (maximumVisibleBarsNow ())
                                                localTotal
                                                localDraft
                                        setDragDiagnostic "data-drag-outcome" "commit-local"
                                        navigatorCursorAfterRender <- Some(if drag = TaWindowDrag.Move then "grab" else "ew-resize")
                                        setWindow followLatest next
                                    | None ->
                                        match RendererModel.tryLoadedCoverageWindowIntent projection draft with
                                        | Some intent ->
                                            let direction =
                                                if draft.StartIndex < committed.StartIndex then
                                                    PulseTrade.Comm.Spa.Dynamic.Renderer.TaCoverageDirection.Earlier
                                                else
                                                    PulseTrade.Comm.Spa.Dynamic.Renderer.TaCoverageDirection.Later
                                            setDragDiagnostic
                                                "data-drag-outcome"
                                                (if direction = PulseTrade.Comm.Spa.Dynamic.Renderer.TaCoverageDirection.Earlier then "request-earlier" else "request-later")
                                            draftWindow.Value <- None
                                            sendOrQueueCoverageWindowAction
                                                (intent.StartObservationOrdinal |> Option.defaultValue 0L)
                                                intent.ObservationCount
                                                "Loaded coverage requested."
                                        | None ->
                                            setDragDiagnostic "data-drag-outcome" "invalid-loaded-draft"
                                            draftWindow.Value <- None
                                | Some _ ->
                                    setDragDiagnostic "data-drag-outcome" "no-change"
                                    draftWindow.Value <- None
                                | None ->
                                    match RendererModel.navigatorBoundaryDirection total committed drag latestRawDelta with
                                    | Some direction ->
                                        setDragDiagnostic
                                            "data-drag-outcome"
                                            (if direction = PulseTrade.Comm.Spa.Dynamic.Renderer.TaCoverageDirection.Earlier then "request-earlier" else "request-later")
                                        draftWindow.Value <- None
                                        requestAdjacentCoverage
                                            direction
                                            (if direction = PulseTrade.Comm.Spa.Dynamic.Renderer.TaCoverageDirection.Earlier then -committed.Count else committed.Count)
                                    | None ->
                                        let followLatest, next =
                                            RendererModel.commitWindowBounds options.MinimumVisibleBars (maximumVisibleBarsNow ()) total draft
                                        if next <> committed || followLatest <> uiState.Value.FollowLatest then
                                            setDragDiagnostic "data-drag-outcome" "commit-local"
                                            navigatorCursorAfterRender <- Some(if drag = TaWindowDrag.Move then "grab" else "ew-resize")
                                            setWindow followLatest next
                                        else
                                            setDragDiagnostic "data-drag-outcome" "no-change"
                                            draftWindow.Value <- None
                                cleanup ()

                        moveHandler <-
                            Action<Event>(fun rawEvent ->
                                let mouse = rawEvent :?> MouseEvent
                                let delta =
                                    if bounds.Width <= 0.0 || total <= 0 then 0
                                    else int (Math.Round(float (mouse.ClientX - startClientX) / bounds.Width * float total))
                                latestRawDelta <- delta
                                RendererModel.previewWindowBounds options.MinimumVisibleBars (maximumVisibleBarsNow ()) total committed drag delta
                                |> scheduleDraft)

                        upHandler <-
                            Action<Event>(fun _ -> finish ())

                        cancelHandler <-
                            Action<Event>(fun _ ->
                                if not finished then
                                    finished <- true
                                    finishNavigatorDrag <- None
                                    activeNavigatorCursor <- None
                                    draftWindow.Value <- None
                                    setDragDiagnostic "data-drag-outcome" "cancelled"
                                    cleanup ())

                        finishNavigatorDrag <- Some finish
                        try
                            navigatorRoot.SetPointerCapture(pointerId)
                            setDragDiagnostic "data-drag-capture" "pointer"
                            navigatorRoot.AddEventListener("pointermove", moveHandler)
                            navigatorRoot.AddEventListener("pointerup", upHandler)
                            navigatorRoot.AddEventListener("pointercancel", cancelHandler)
                        with _ ->
                            documentFallback <- true
                            setDragDiagnostic "data-drag-capture" "document-fallback"
                            JS.Document.AddEventListener("pointermove", moveHandler)
                            JS.Document.AddEventListener("pointerup", upHandler)
                            JS.Document.AddEventListener("pointercancel", cancelHandler)

        let finishNavigatorDragFromElement (event: Event) =
            event.PreventDefault()
            finishNavigatorDrag |> Option.iter (fun finish -> finish ())

        let cursorElements selector =
            if isNull chartStackElement then [||]
            else
                let nodes = chartStackElement.QuerySelectorAll(selector)
                [| for index in 0 .. int nodes.Length - 1 do
                       yield nodes.Item(index) |> As<Element> |]

        let scopedElements (root: Element) selector =
            if isNull root then [||]
            else
                let nodes = root.QuerySelectorAll(selector)
                [| for index in 0 .. int nodes.Length - 1 do
                       yield nodes.Item(index) |> As<Element> |]

        let cursorPanelElements selector = scopedElements cursorPanelElement selector

        let rowLegendElements selector =
            latestRowLegendElements
            |> Array.collect (fun root -> scopedElements root selector)

        let setElementTextIfChanged value (element: Element) =
            if element.TextContent <> value then
                element.TextContent <- value
                true
            else
                false

        let setElementAttributeIfChanged name value (element: Element) =
            if element.GetAttribute(name) <> value then
                element.SetAttribute(name, value)
                true
            else
                false

        let removeElementAttributeIfPresent name (element: Element) =
            if element.HasAttribute(name) then
                element.RemoveAttribute(name)
                true
            else
                false

        let setElementHiddenIfChanged hidden (element: Element) =
            let html = element |> As<HTMLElement>
            let authoredDisplayAttribute = "data-ptcs-authored-display"
            let mutable styleChanged = false
            if hidden then
                if not (element.HasAttribute(authoredDisplayAttribute)) then
                    element.SetAttribute(authoredDisplayAttribute, html.Style.GetProperty("display"))
                if html.Style.GetProperty("display") <> "none" then
                    html.Style.SetProperty("display", "none")
                    styleChanged <- true
            elif element.HasAttribute(authoredDisplayAttribute) then
                let authoredDisplay = element.GetAttribute(authoredDisplayAttribute)
                if html.Style.GetProperty("display") <> authoredDisplay then
                    html.Style.SetProperty("display", authoredDisplay)
                    styleChanged <- true
                element.RemoveAttribute(authoredDisplayAttribute)
            if hidden then
                if not (element.HasAttribute("hidden")) then
                    element.SetAttribute("hidden", "hidden")
                    true
                else
                    styleChanged
            else
                removeElementAttributeIfPresent "hidden" element || styleChanged

        let setElementHidden hidden element = setElementHiddenIfChanged hidden element |> ignore

        let browserNowMs () = float (Date.Now())

        let applyVisibleCursorValues bounded =
            if not (isNull chartStackElement) then
                let totalStarted = browserNowMs ()
                let hint = cursorPanelElements "[data-ta-cursor-hint]" |> Array.tryHead
                let time = cursorPanelElements "[data-ta-cursor-time]" |> Array.tryHead
                let valueNodes = cursorPanelElements "[data-ta-cursor-value-index]"
                let legendValueNodes = rowLegendElements "[data-ta-row-value-index]"
                let rowTimeNodes = rowLegendElements "[data-ta-row-data-time='true']"
                let ofiBands = cursorElements "[data-ta-row-ofi-band='true']"
                let queryCompleted = browserNowMs ()
                let legendIndex =
                    match bounded with
                    | Some index -> Some index
                    | None when latestCursorTimestamps.Length > 0 -> Some(latestCursorTimestamps.Length - 1)
                    | None -> None

                let legendUpdates =
                    legendValueNodes
                    |> Array.mapi (fun valueIndex node ->
                        let traceIndex =
                            match Int32.TryParse(node.GetAttribute("data-ta-row-value-index")) with
                            | true, parsed -> parsed
                            | _ -> valueIndex
                        let rowId = node.GetAttribute("data-ta-row-value-row-id")
                        let nextValue =
                            legendIndex
                            |> Option.bind (fun index ->
                                match bounded with
                                | Some _ -> tryLegendValue latestLegendReaders rowId traceIndex index
                                | None -> tryLatestLegendValue latestLegendValueReaders rowId traceIndex)
                            |> Option.map _.Value
                            |> Option.defaultValue "Unavailable"
                        node, nextValue, if nextValue = "Unavailable" then "undefined" else "defined")

                let rowTimeUpdates =
                    rowTimeNodes
                    |> Array.map (fun node ->
                        let rowId = node.GetAttribute("data-ta-row-data-time-row-id")
                        let nextTime =
                            legendIndex
                            |> Option.bind (fun index ->
                                match bounded with
                                | Some _ -> tryRowPresentation latestLegendReaders rowId index
                                | None -> tryLatestRowPresentation latestLegendValueReaders rowId)
                            |> Option.map (fun value -> TaDisplayTimeFormatter.fullOrOriginal (displayTime.Current ()) value.Timestamp)
                            |> Option.defaultValue "Unavailable"
                        node, nextTime)

                let cursorTimeUpdate =
                    bounded
                    |> Option.map (fun index -> TaDisplayTimeFormatter.compactOrOriginal (displayTime.Current ()) latestCursorTimestamps[index])
                let cursorValueUpdates =
                    match bounded with
                    | None -> [||]
                    | Some index ->
                        valueNodes
                        |> Array.mapi (fun valueIndex node ->
                            let current = latestCursorReaders |> Array.tryItem valueIndex |> Option.bind (fun readCursor -> readCursor index)
                            node, current)
                let ofiUpdates =
                    ofiBands
                    |> Array.map (fun band ->
                        let rowId = band.GetAttribute("data-ta-row-ofi-row-id")
                        let items =
                            bounded
                            |> Option.bind (fun index -> latestMarkerCursorReaders |> Map.tryFind rowId |> Option.map (fun read -> read index))
                            |> Option.defaultValue [||]
                        band, items)
                let resolveCompleted = browserNowMs ()

                let mutable textWrites = 0
                let mutable attributeWrites = 0
                for node, nextValue, nextState in legendUpdates do
                    if setElementTextIfChanged nextValue node then textWrites <- textWrites + 1
                    if setElementAttributeIfChanged "data-value-state" nextState node then attributeWrites <- attributeWrites + 1
                for node, nextTime in rowTimeUpdates do
                    if setElementTextIfChanged nextTime node then textWrites <- textWrites + 1
                match cursorTimeUpdate, time with
                | Some nextTime, Some node ->
                    if setElementTextIfChanged nextTime node then textWrites <- textWrites + 1
                    if setElementAttributeIfChanged "data-display-time-zone" (SduiDisplayTimeZone.id (displayTime.Current ())) node then attributeWrites <- attributeWrites + 1
                    match bounded with
                    | Some index ->
                        if setElementAttributeIfChanged "data-canonical-event-time" latestCursorTimestamps[index] node then attributeWrites <- attributeWrites + 1
                    | None -> ()
                | _ -> ()
                for node, current in cursorValueUpdates do
                    match current with
                    | Some value ->
                        if setElementTextIfChanged (value.Label + " " + value.Value) node then textWrites <- textWrites + 1
                        if setElementAttributeIfChanged "data-cursor-row" value.Label node then attributeWrites <- attributeWrites + 1
                    | None ->
                        if setElementTextIfChanged "" node then textWrites <- textWrites + 1
                        if removeElementAttributeIfPresent "data-cursor-row" node then attributeWrites <- attributeWrites + 1
                for band, items in ofiUpdates do
                    if setElementAttributeIfChanged "data-display-time-zone" (SduiDisplayTimeZone.id (displayTime.Current ())) band then attributeWrites <- attributeWrites + 1
                    if setElementAttributeIfChanged "data-marker-event-count" (string items.Length) band then attributeWrites <- attributeWrites + 1
                    match scopedElements band "[data-ta-row-ofi-empty='true']" |> Array.tryHead with
                    | Some node ->
                        let capabilityAvailable = band.GetAttribute("data-cursor-event-capability") = "available"
                        let nextState, nextText = if capabilityAvailable then "none", "" else "unavailable", ""
                        if setElementTextIfChanged nextText node then textWrites <- textWrites + 1
                        if setElementAttributeIfChanged "data-cursor-event-state" nextState node then attributeWrites <- attributeWrites + 1
                    | None -> ()
                    match bounded with
                    | Some index ->
                        if setElementAttributeIfChanged "data-cursor-slot" (string index) band then attributeWrites <- attributeWrites + 1
                        if setElementAttributeIfChanged "data-cursor-event-time" latestCursorTimestamps[index] band then attributeWrites <- attributeWrites + 1
                    | None ->
                        if removeElementAttributeIfPresent "data-cursor-slot" band then attributeWrites <- attributeWrites + 1
                        if removeElementAttributeIfPresent "data-cursor-event-time" band then attributeWrites <- attributeWrites + 1

                    let itemNodes = scopedElements band "[data-ta-row-ofi-item-index]"
                    for node in itemNodes do
                        let index =
                            match Int32.TryParse(node.GetAttribute("data-ta-row-ofi-item-index")) with
                            | true, parsed -> parsed
                            | _ -> -1
                        match items |> Array.tryItem index with
                        | Some item ->
                            let displayText = if item.Label = item.Category then item.Category else item.Category + ": " + item.Label
                            if setElementTextIfChanged displayText node then textWrites <- textWrites + 1
                            if setElementAttributeIfChanged "title" item.Tooltip node then attributeWrites <- attributeWrites + 1
                            if setElementAttributeIfChanged "data-marker-id" item.MarkerId node then attributeWrites <- attributeWrites + 1
                            if setElementAttributeIfChanged "data-marker-event-time" item.EventTimeUtc node then attributeWrites <- attributeWrites + 1
                            if setElementAttributeIfChanged "data-marker-color" item.Color node then attributeWrites <- attributeWrites + 1
                            if setElementAttributeIfChanged "data-cursor-event-id" item.MarkerId node then attributeWrites <- attributeWrites + 1
                            if setElementAttributeIfChanged "data-cursor-event-time" item.EventTimeUtc node then attributeWrites <- attributeWrites + 1
                            if setElementAttributeIfChanged "data-cursor-event-category" item.Category node then attributeWrites <- attributeWrites + 1
                            if setElementAttributeIfChanged "data-cursor-event-source-kind" item.SourceKind node then attributeWrites <- attributeWrites + 1
                            if setElementAttributeIfChanged "data-cursor-event-color" item.Color node then attributeWrites <- attributeWrites + 1
                        | None ->
                            if setElementTextIfChanged "" node then textWrites <- textWrites + 1
                            if removeElementAttributeIfPresent "title" node then attributeWrites <- attributeWrites + 1
                            if removeElementAttributeIfPresent "data-marker-id" node then attributeWrites <- attributeWrites + 1
                            if removeElementAttributeIfPresent "data-marker-event-time" node then attributeWrites <- attributeWrites + 1
                            if removeElementAttributeIfPresent "data-marker-color" node then attributeWrites <- attributeWrites + 1
                            if removeElementAttributeIfPresent "data-cursor-event-id" node then attributeWrites <- attributeWrites + 1
                            if removeElementAttributeIfPresent "data-cursor-event-time" node then attributeWrites <- attributeWrites + 1
                            if removeElementAttributeIfPresent "data-cursor-event-category" node then attributeWrites <- attributeWrites + 1
                            if removeElementAttributeIfPresent "data-cursor-event-source-kind" node then attributeWrites <- attributeWrites + 1
                            if removeElementAttributeIfPresent "data-cursor-event-color" node then attributeWrites <- attributeWrites + 1

                    let overflowNode = scopedElements band "[data-ta-row-ofi-overflow='true']" |> Array.tryHead
                    match overflowNode with
                    | Some node when items.Length > RendererModel.MarkerCursorItemBudget ->
                        let overflow = items |> Array.skip RendererModel.MarkerCursorItemBudget
                        if setElementTextIfChanged ("+" + string overflow.Length) node then textWrites <- textWrites + 1
                        let tooltip = overflow |> Array.truncate 8 |> Array.map _.Tooltip |> String.concat "\n---\n"
                        if setElementAttributeIfChanged "title" tooltip node then attributeWrites <- attributeWrites + 1
                    | Some node ->
                        if setElementTextIfChanged "" node then textWrites <- textWrites + 1
                        if removeElementAttributeIfPresent "title" node then attributeWrites <- attributeWrites + 1
                    | None -> ()
                let writeCompleted = browserNowMs ()

                let mutable visibilityWrites = 0
                match bounded with
                | None ->
                    hint |> Option.iter (fun node -> if setElementHiddenIfChanged false node then visibilityWrites <- visibilityWrites + 1)
                    time |> Option.iter (fun node -> if setElementHiddenIfChanged true node then visibilityWrites <- visibilityWrites + 1)
                    for node in valueNodes do
                        if setElementHiddenIfChanged true node then visibilityWrites <- visibilityWrites + 1
                | Some _ ->
                    hint |> Option.iter (fun node -> if setElementHiddenIfChanged true node then visibilityWrites <- visibilityWrites + 1)
                    time |> Option.iter (fun node -> if setElementHiddenIfChanged false node then visibilityWrites <- visibilityWrites + 1)
                    for node, current in cursorValueUpdates do
                        if setElementHiddenIfChanged current.IsNone node then visibilityWrites <- visibilityWrites + 1
                for band, items in ofiUpdates do
                    match scopedElements band "[data-ta-row-ofi-empty='true']" |> Array.tryHead with
                    | Some node ->
                        if setElementHiddenIfChanged (items.Length > 0) node then visibilityWrites <- visibilityWrites + 1
                    | None -> ()
                    let itemNodes = scopedElements band "[data-ta-row-ofi-item-index]"
                    for node in itemNodes do
                        let index =
                            match Int32.TryParse(node.GetAttribute("data-ta-row-ofi-item-index")) with
                            | true, parsed -> parsed
                            | _ -> -1
                        if setElementHiddenIfChanged (index < 0 || index >= items.Length) node then visibilityWrites <- visibilityWrites + 1
                    match scopedElements band "[data-ta-row-ofi-overflow='true']" |> Array.tryHead with
                    | Some node ->
                        if setElementHiddenIfChanged (items.Length <= RendererModel.MarkerCursorItemBudget) node then visibilityWrites <- visibilityWrites + 1
                    | None -> ()

                let visibilityCompleted = browserNowMs ()
                let queryMs = queryCompleted - totalStarted
                let resolveMs = resolveCompleted - queryCompleted
                let writeMs = writeCompleted - resolveCompleted
                let visibilityMs = visibilityCompleted - writeCompleted
                let totalMs = visibilityCompleted - totalStarted
                visibleValueTelemetrySequence <- visibleValueTelemetrySequence + 1
                visibleValueTelemetryMaxTotalMs <- max visibleValueTelemetryMaxTotalMs totalMs
                setElementAttributeIfChanged "data-visible-value-telemetry-sequence" (string visibleValueTelemetrySequence) chartStackElement |> ignore
                setElementAttributeIfChanged "data-visible-value-query-ms" (fixedText queryMs) chartStackElement |> ignore
                setElementAttributeIfChanged "data-visible-value-resolve-ms" (fixedText resolveMs) chartStackElement |> ignore
                setElementAttributeIfChanged "data-visible-value-write-ms" (fixedText writeMs) chartStackElement |> ignore
                setElementAttributeIfChanged "data-visible-value-visibility-ms" (fixedText visibilityMs) chartStackElement |> ignore
                setElementAttributeIfChanged "data-visible-value-total-ms" (fixedText totalMs) chartStackElement |> ignore
                setElementAttributeIfChanged "data-visible-value-max-total-ms" (fixedText visibleValueTelemetryMaxTotalMs) chartStackElement |> ignore
                setElementAttributeIfChanged "data-visible-value-text-writes" (string textWrites) chartStackElement |> ignore
                setElementAttributeIfChanged "data-visible-value-attribute-writes" (string attributeWrites) chartStackElement |> ignore
                setElementAttributeIfChanged "data-visible-value-visibility-writes" (string visibilityWrites) chartStackElement |> ignore
                setElementAttributeIfChanged "data-visible-value-node-count" (string (valueNodes.Length + legendValueNodes.Length + rowTimeNodes.Length + ofiBands.Length * (RendererModel.MarkerCursorItemBudget + 1) + hint.IsSome.GetHashCode() + time.IsSome.GetHashCode())) chartStackElement |> ignore
                let publicationCompleted = browserNowMs ()
                setElementAttributeIfChanged "data-visible-value-publication-ms" (fixedText (publicationCompleted - visibilityCompleted)) chartStackElement |> ignore

        refreshVisibleValues <- fun () -> applyVisibleCursorValues displayedCursorIndex

        let applyCursorIndex value =
            if not (isNull chartStackElement) then
                let bounded =
                    value
                    |> Option.bind (fun index ->
                        if latestCursorTimestamps.Length = 0 then None
                        else Some(max 0 (min index (latestCursorTimestamps.Length - 1))))
                displayedCursorIndex <- bounded
                chartStackElement.SetAttribute("data-cursor-index", bounded |> Option.map string |> Option.defaultValue "")

                let crosshairs = cursorElements "[data-ta-shared-crosshair='true']"
                let rowCursorLabels = cursorElements "[data-ta-row-cursor-label='true']"
                match cursorPosition 1000.0 latestCursorTimestamps.Length bounded with
                | Some x ->
                    let xText = fixedText x
                    for line in crosshairs do
                        line.SetAttribute("x1", xText)
                        line.SetAttribute("x2", xText)
                        line.SetAttribute("visibility", "visible")
                    let labelLeftPercent = max 48.0 (min 952.0 x) / 10.0 |> fixedText
                    for group in rowCursorLabels do
                        let rowId = group.GetAttribute("data-ta-row-cursor-row-id")
                        let presentation = bounded |> Option.bind (tryRowPresentation latestLegendReaders rowId)
                        let dateText, timeText =
                            presentation
                            |> Option.map (fun value -> TaDisplayTimeFormatter.dateAndClockOrUnavailable (displayTime.Current ()) value.Timestamp)
                            |> Option.defaultValue ("Unavailable", "Unavailable")
                        let canonicalEventTime = presentation |> Option.map _.Timestamp |> Option.defaultValue ""
                        group.SetAttribute("style", rowCursorTagStyle labelLeftPercent true)
                        group.SetAttribute("data-display-time-zone", SduiDisplayTimeZone.id (displayTime.Current ()))
                        group.SetAttribute("data-canonical-event-time", canonicalEventTime)
                        let dateNode = group.QuerySelector("[data-ta-row-cursor-date='true']")
                        let timeNode = group.QuerySelector("[data-ta-row-cursor-clock='true']")
                        if not (isNull dateNode) then dateNode.TextContent <- dateText
                        if not (isNull timeNode) then timeNode.TextContent <- timeText
                | None ->
                    for line in crosshairs do line.SetAttribute("visibility", "hidden")
                    for group in rowCursorLabels do group.SetAttribute("style", rowCursorTagStyle "50" false)

                applyVisibleCursorValues bounded

        refreshDisplayTime <- fun () -> applyCursorIndex displayedCursorIndex

        let flushCursorFrame () =
            cursorFrameScheduled <- false
            match pendingCursorIndex with
            | Some value ->
                let requestedAtMs = pendingCursorRequestedAtMs
                pendingCursorIndex <- None
                applyCursorIndex value
                cursorRenderLatencySequence <- cursorRenderLatencySequence + 1
                if not (isNull chartStackElement) then
                    chartStackElement.SetAttribute("data-cursor-render-latency-sequence", string cursorRenderLatencySequence)
                    chartStackElement.SetAttribute("data-cursor-render-latency-ms", fixedText (max 0.0 (browserNowMs () - requestedAtMs)))
            | None -> ()

        let setCursorIndex value =
            pendingCursorIndex <- Some value
            pendingCursorRequestedAtMs <- browserNowMs ()
            if not cursorFrameScheduled then
                cursorFrameScheduled <- true
                JS.RequestAnimationFrame(fun _ -> flushCursorFrame ()) |> ignore

        let scheduleCursorGeometryRefresh () =
            JS.RequestAnimationFrame(fun _ -> setCursorIndex displayedCursorIndex) |> ignore

        let commitCursorIndex index =
            setCursorIndex (Some index)
            if cursorIndex.Value <> Some index then cursorIndex.Value <- Some index
            if actionAllowed "shared-cursor-changed" && not (commandsDisabledNow ()) then
                match runtimeState.Value.Document with
                | Some document ->
                    let timeline = RendererModel.referenceTimelineForDocumentPrepared document latestPreparedData
                    let visible = resolvedWindow uiState.Value |> fun window -> RendererModel.selectWindow window timeline
                    match document.BaseRowId with
                    | Some baseRowId when index >= 0 && index < visible.Length ->
                        startAction
                            (SduiAction.SharedCursorChanged(
                                currentCanvasId (),
                                { BaseRowId = baseRowId
                                  EventTimeUtc = visible[index] }))
                            "Shared cursor synchronized."
                            ignore
                    | _ -> ()
                | None -> ()

        let selectedEditorSchema () =
            editorSchemasNow () |> Array.tryFind (fun schema -> schema.TemplateKey = selectedTemplate.Value)

        let resetEditorFor templateKey =
            selectedTemplate.Value <- templateKey
            editorValues.Value <-
                editorSchemasNow ()
                |> Array.tryFind (fun schema -> schema.TemplateKey = templateKey)
                |> Option.map RendererModel.initialEditorInputs
                |> Option.defaultValue [||]

        let forceCloseRowEditor () =
            editingRowId <- None
            pendingEditorMutation <- None
            setUiState { uiState.Value with AddRowOpen = false }

        let closeRowEditor () =
            if uiState.Value.PendingActionId.IsNone then forceCloseRowEditor ()

        let openNewRowEditor () =
            editingRowId <- None
            match editorSchemasNow () |> Array.tryHead with
            | Some schema -> resetEditorFor schema.TemplateKey
            | None -> ()
            setUiState { uiState.Value with AddRowOpen = true; Feedback = "" }

        let openRowEditor row =
            match TaRowEditorBinding.tryResolve (editorSchemasNow ()) row with
            | Ok(Some(schema, values)) ->
                editingRowId <- Some row.RowId
                selectedTemplate.Value <- schema.TemplateKey
                editorValues.Value <- values
                setUiState { uiState.Value with AddRowOpen = true; Feedback = "" }
            | Ok None -> ()
            | Error _ ->
                editingRowId <- None
                setUiState
                    { uiState.Value with
                        AddRowOpen = false
                        Feedback = "This row's editor metadata is invalid; the row remains read-only." }

        let completeEditorMutation identity (document: TaWorkspaceDocument) documentRevision =
            let bindingOf row =
                TaRowEditorBinding.tryFind row
                |> Result.toOption
                |> Option.flatten

            match pendingEditorMutation with
            | Some(baseIdentity, baseRevision, targetRowId, priorRowIds, expectedBinding) ->
                let matched =
                    match targetRowId with
                    | Some rowId ->
                        document.Rows
                        |> Array.tryFind (fun row -> row.RowId = rowId)
                        |> Option.bind bindingOf
                        |> Option.exists ((=) expectedBinding)
                    | None ->
                        document.Rows
                        |> Array.exists (fun row ->
                            not (Set.contains row.RowId priorRowIds)
                            && bindingOf row = Some expectedBinding)

                if
                    EditorMutationGate.authoritativeDocumentAdvanced
                        baseIdentity
                        baseRevision
                        identity
                        documentRevision
                        matched
                then
                    let feedback = if targetRowId.IsSome then "Row updated." else "Row added."
                    forceCloseRowEditor ()
                    setUiState { uiState.Value with Feedback = feedback }
            | _ -> ()

        let editorTestId (path: string) =
            "ta-editor-" + path.Replace(".", "-").Replace("[", "-").Replace("]", "")

        let setEditorScalar path value =
            editorValues.Value <- RendererModel.setEditorInput { Path = path; Value = value } editorValues.Value

        let removeEditorScalar path =
            editorValues.Value <- editorValues.Value |> Array.filter (fun current -> current.Path <> path)

        let scalarText path =
            RendererModel.tryEditorInput path editorValues.Value
            |> Option.map RendererModel.editorScalarText
            |> Option.defaultValue ""

        let scalarEditor path labelText required kind =
            let caption = if required then labelText + " *" else labelText
            let shell control =
                label [ attr.style "display:flex; flex-direction:column; gap:3px; min-width:0; font-size:10px; color:#60738b;" ] [
                    text caption
                    control
                ] :> Doc

            match kind with
            | EditorValueKind.Text ->
                inputText (editorTestId path) labelText (scalarText path) (fun value -> setEditorScalar path (EditorScalarValue.Text value))
                |> shell
            | EditorValueKind.Integer(minimum, maximum) ->
                let attrs =
                    [ Attr.Create "data-testid" (editorTestId path)
                      attr.``type`` "number"
                      attr.value (scalarText path)
                      Attr.Create "step" "1"
                      attr.style "height:30px; min-width:0; width:100%; border:1px solid #b9c6d8; border-radius:4px; background:#fff; color:#142033; padding:4px 7px; box-sizing:border-box; font-size:12px;"
                      on.afterRender (fun node ->
                          let input = node |> As<HTMLInputElement>
                          input.AddEventListener("input", fun () ->
                              match Int64.TryParse input.Value with
                              | true, value -> setEditorScalar path (EditorScalarValue.Number(float value))
                              | _ -> removeEditorScalar path))
                      match minimum with Some value -> Attr.Create "min" (string value) | None -> Attr.Empty
                      match maximum with Some value -> Attr.Create "max" (string value) | None -> Attr.Empty ]
                element "input" attrs [] |> shell
            | EditorValueKind.Decimal(minimum, maximum) ->
                let attrs =
                    [ Attr.Create "data-testid" (editorTestId path)
                      attr.``type`` "number"
                      attr.value (scalarText path)
                      Attr.Create "step" "any"
                      attr.style "height:30px; min-width:0; width:100%; border:1px solid #b9c6d8; border-radius:4px; background:#fff; color:#142033; padding:4px 7px; box-sizing:border-box; font-size:12px;"
                      on.afterRender (fun node ->
                          let input = node |> As<HTMLInputElement>
                          input.AddEventListener("input", fun () ->
                              match Double.TryParse input.Value with
                              | true, value -> setEditorScalar path (EditorScalarValue.Number value)
                              | _ -> removeEditorScalar path))
                      match minimum with Some value -> Attr.Create "min" (fixedText value) | None -> Attr.Empty
                      match maximum with Some value -> Attr.Create "max" (fixedText value) | None -> Attr.Empty ]
                element "input" attrs [] |> shell
            | EditorValueKind.Boolean ->
                let isChecked =
                    match RendererModel.tryEditorInput path editorValues.Value with
                    | Some(EditorScalarValue.Bool value) -> value
                    | _ -> false
                label [ attr.style "display:flex; align-items:center; gap:6px; min-height:30px; font-size:11px; color:#40536d;" ] [
                    element "input" [
                        Attr.Create "data-testid" (editorTestId path)
                        attr.``type`` "checkbox"
                        if isChecked then Attr.Create "checked" "checked"
                        on.afterRender (fun node ->
                            let input = node |> As<HTMLInputElement>
                            input.AddEventListener("change", fun () -> setEditorScalar path (EditorScalarValue.Bool input.Checked)))
                    ] []
                    text caption
                ] :> Doc
            | EditorValueKind.Choice choices ->
                let selectedKey =
                    choices
                    |> Array.tryFind (fun choice ->
                        RendererModel.tryEditorInput path editorValues.Value
                        |> Option.exists (fun current -> RendererModel.editorScalarEqualsSdui current choice.Value))
                    |> Option.map _.Key
                    |> Option.defaultValue ""
                selectInput (editorTestId path) selectedKey (choices |> Array.map (fun choice -> choice.Key, choice.Label) |> Array.toList) (fun key ->
                    choices
                    |> Array.tryFind (fun choice -> choice.Key = key)
                    |> Option.bind (fun choice ->
                        match choice.Value with
                        | SduiValue.Text value -> Some(EditorScalarValue.Text value)
                        | SduiValue.Number value -> Some(EditorScalarValue.Number value)
                        | SduiValue.Bool value -> Some(EditorScalarValue.Bool value)
                        | _ -> None)
                    |> Option.iter (setEditorScalar path))
                |> shell
            | EditorValueKind.Scale scaleKeys ->
                selectInput (editorTestId path) (scalarText path) (scaleKeys |> Array.map (fun value -> value, value) |> Array.toList) (fun value -> setEditorScalar path (EditorScalarValue.Text value))
                |> shell
            | _ -> Doc.Empty

        let rec editorKind path labelText required kind =
            match kind with
            | EditorValueKind.Group fields ->
                fieldset [ attr.style "min-width:0; margin:0; padding:7px; border:1px solid #cbd6e5; border-radius:5px;" ] [
                    legend [ attr.style "padding:0 4px; font-size:11px; color:#40536d;" ] [ text labelText ]
                    div [ attr.style "display:grid; grid-template-columns:repeat(auto-fit,minmax(130px,1fr)); gap:7px; min-width:0;" ] [
                        for field in fields do
                            yield editorKind ($"{path}.{field.Key}") field.Label field.Required field.Kind
                    ]
                ] :> Doc
            | EditorValueKind.List(itemKind, _, maximum) ->
                let indexesView =
                    editorValues.View
                    |> View.Map (RendererModel.listIndexes path)
                    |> View.MapCachedBy (=) id

                div [ attr.style "display:flex; flex-direction:column; gap:5px; min-width:0;" ] [
                    span [ attr.style "font-size:10px; color:#60738b;" ] [ text (if required then labelText + " *" else labelText) ]
                    indexesView
                    |> View.Map (fun indexes ->
                        div [ Attr.Create "data-testid" (editorTestId path + "-items"); attr.style "display:flex; flex-direction:column; gap:5px;" ] [
                            for position, index in indexes |> Array.indexed do
                                let itemPath = $"{path}[{index}]"
                                yield div [ attr.style "display:grid; grid-template-columns:auto minmax(0,1fr); gap:5px; align-items:end;" ] [
                                    div [ attr.style "display:flex; align-items:center; gap:3px; height:30px;" ] [
                                        compactButton (editorTestId itemPath + "-up") "↑" "Move item up" (fun () ->
                                            if position > 0 then editorValues.Value <- RendererModel.moveListItem path index indexes[position - 1] editorValues.Value)
                                        compactButton (editorTestId itemPath + "-down") "↓" "Move item down" (fun () ->
                                            if position < indexes.Length - 1 then editorValues.Value <- RendererModel.moveListItem path index indexes[position + 1] editorValues.Value)
                                        compactButton (editorTestId itemPath + "-remove") "×" "Remove item" (fun () ->
                                            editorValues.Value <- RendererModel.removeListItem path index editorValues.Value)
                                    ]
                                    editorKind itemPath $"Item {position + 1}" true itemKind
                                ]
                        ] :> Doc)
                    |> Doc.EmbedView
                    compactButton (editorTestId path + "-add") "+ Add" ("Add " + labelText) (fun () ->
                        let count = RendererModel.listIndexes path editorValues.Value |> Array.length
                        match maximum with
                        | Some limit when count >= limit -> setUiState { uiState.Value with Feedback = $"{labelText} allows at most {limit} item(s)." }
                        | _ -> editorValues.Value <- RendererModel.addListItem path itemKind editorValues.Value)
                ] :> Doc
            | scalar -> scalarEditor path labelText required scalar

        let genericEditor () =
            div [ Attr.Create "data-testid" "ta-generic-row-editor"; attr.style "display:flex; flex-direction:column; gap:7px; min-width:0;" ] [
                label [ attr.style "display:flex; flex-direction:column; gap:2px; min-width:0; font-size:10px; color:#60738b;" ] [
                    text "Template"
                    selectInput "ta-editor-template" selectedTemplate.Value (editorSchemasNow () |> Array.map (fun schema -> schema.TemplateKey, schema.DisplayName) |> Array.toList) resetEditorFor
                ]
                selectedTemplate.View
                |> View.Map (fun templateKey ->
                    match editorSchemasNow () |> Array.tryFind (fun schema -> schema.TemplateKey = templateKey) with
                    | None -> div [ attr.style "font-size:11px; color:#9a2f2f;" ] [ text "Template schema is unavailable." ] :> Doc
                    | Some schema ->
                        div [ attr.style "display:grid; grid-template-columns:repeat(auto-fit,minmax(150px,1fr)); gap:7px; min-width:0;" ] [
                            for field in schema.Fields do
                                yield editorKind field.Key field.Label field.Required field.Kind
                        ] :> Doc)
                |> Doc.EmbedView
            ]

        let rec submitQuery query intentGeneration =
            queryInFlight <- true
            let applyAcceptedQuery () =
                match runtimeState.Value.Document with
                | None -> Some "query-viewport-unavailable: the workspace document is not loaded."
                | Some currentDocument ->
                    match
                        RendererModel.queryViewportSelection
                            querySelectionGeneration
                            intentGeneration
                            query
                            currentDocument
                            runtimeState.Value.Data
                    with
                    | TaQueryViewportSelection.NotRequested -> None
                    | TaQueryViewportSelection.Selected window ->
                        commitLocalWindow false window |> ignore
                        None
                    | TaQueryViewportSelection.NoIntersection ->
                        Some "Query accepted, but the requested range has no loaded observations; the current viewport was preserved."
                    | TaQueryViewportSelection.Invalid reason -> Some reason
                    | TaQueryViewportSelection.Stale ->
                        Some "A newer query superseded this response; the current viewport was preserved."
            let dispatchLatestQueuedQuery () =
                queryInFlight <- false
                match queuedQuery with
                | Some(nextQuery, nextGeneration) ->
                    queuedQuery <- None
                    submitQuery nextQuery nextGeneration
                | None -> ()

            startActionWithFeedback
                (SduiAction.ChangeTaQuery(currentCanvasId (), query))
                "Query accepted."
                applyAcceptedQuery
                ignore
                dispatchLatestQueuedQuery

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

            if not (remoteDisabled runtimeState.Value.Poll) then
                querySelectionGeneration <- querySelectionGeneration + 1
                let intentGeneration = querySelectionGeneration
                if queryInFlight then
                    queuedQuery <- Some(query, intentGeneration)
                    setUiState { uiState.Value with Feedback = "A newer query is queued and will supersede the pending viewport selection." }
                elif uiState.Value.PendingActionId.IsSome then
                    setUiState { uiState.Value with Feedback = "action-in-flight: wait for the pending action result." }
                else
                    submitQuery query intentGeneration

        let addLegacyRow () =
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
            | Result.Error message -> setUiState { uiState.Value with Feedback = message }
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
                startAction (SduiAction.AddTaRow(currentCanvasId (), spec)) "Row accepted." ignore

        let addRow () =
            match selectedEditorSchema () with
            | None -> addLegacyRow ()
            | Some schema ->
                let errors = RendererModel.validateEditorSubmission schema editorValues.Value
                if errors.Length > 0 then
                    setUiState { uiState.Value with Feedback = String.concat " " errors }
                else
                    let currentRows =
                        runtimeState.Value.Document
                        |> Option.map _.Rows
                        |> Option.defaultValue [||]
                    let binding =
                        { TemplateKey = schema.TemplateKey
                          Values = Array.copy editorValues.Value }
                    pendingEditorMutation <-
                        Some(
                            runtimeState.Value.Identity,
                            runtimeState.Value.DocumentRevision,
                            editingRowId,
                            currentRows |> Array.map _.RowId |> Set.ofArray,
                            binding)
                    startActionWith
                        (SduiAction.ApplyTemplate(currentCanvasId (), editingRowId, schema.TemplateKey, editorValues.Value))
                        (schema.DisplayName + " accepted; awaiting authoritative document.")
                        ignore
                        (fun () -> pendingEditorMutation <- None)

        let viewportControls (document: TaWorkspaceDocument) =
            let currentViewport =
                View.Map2
                    (fun ui prepared ->
                        let referenceLength =
                            RendererModel.referenceTimelineForDocumentPrepared document prepared
                            |> Array.length
                        let maximumVisibleBars = maximumVisibleBarsFor document
                        let currentWindow =
                            RendererModel.resolveWindow
                                options.MinimumVisibleBars
                                maximumVisibleBars
                                referenceLength
                                ui.FollowLatest
                                ui.Window
                        let loadedObservationCount, globalCurrentWindow =
                            RendererModel.tryCoverageNavigatorWindowResolved referenceLength currentWindow document.DefaultView prepared.RawData
                            |> Option.map (fun (_, total, globalWindow) -> total, globalWindow)
                            |> Option.defaultValue (referenceLength, currentWindow)
                        referenceLength, maximumVisibleBars, loadedObservationCount, globalCurrentWindow)
                    uiState.View
                    shellPreparedData.View

            div [
                Attr.Create "data-testid" "ta-viewport-panel"
                attr.style "display:grid; grid-template-columns:minmax(220px,1fr) auto; gap:6px 10px; align-items:center; margin:0 12px; padding:8px; border-bottom:1px solid #d4deea; background:#f8fafc;"
            ] [
                span [
                    Attr.Create "data-testid" "ta-viewport-range"
                    attr.style "font-family:Consolas,monospace; font-size:11px; color:#344a65; white-space:nowrap;"
                ] [
                    View.Map2
                        (fun (currentReferenceLength, _, loadedObservationCount, globalCurrentWindow) draft ->
                            let rangeText prefix window =
                                let startIndex = if window.Count = 0 then 0 else window.StartIndex + 1
                                let endIndex = window.StartIndex + window.Count
                                $"Loaded {loadedObservationCount} bars · {prefix} {startIndex}-{endIndex}"
                            match draft with
                            | None -> rangeText "Viewing" globalCurrentWindow
                            | Some preview ->
                                rangeText "Preview" preview + " · release to render"
                            |> fun value ->
                                span [
                                    Attr.Create "data-rendered-preview-start" (draft |> Option.map _.StartIndex |> Option.map string |> Option.defaultValue "")
                                    Attr.Create "data-rendered-preview-count" (draft |> Option.map _.Count |> Option.map string |> Option.defaultValue "")
                                    on.afterRender (fun _ -> renderedNavigatorDraft <- draft)
                                ] [ text value ] :> Doc)
                        currentViewport
                        draftWindow.View
                    |> Doc.EmbedView
                ]
                currentViewport
                |> View.Map (fun (_, maximumVisibleBars, loadedObservationCount, _) ->
                    let capped = min loadedObservationCount maximumVisibleBars
                    let label = if loadedObservationCount > maximumVisibleBars then "Max " + string maximumVisibleBars else "All"
                    div [ Attr.Create "data-testid" "ta-viewport-presets"; attr.style "display:flex; gap:4px; align-items:center;" ] [
                        compactButton "ta-jump-loaded-start" "|←" "Jump to the first loaded bars" (fun () -> jumpToLoadedCoverageEdge TaLoadedCoverageEdge.Start)
                        compactButton "ta-view-48" "48" "Show latest 48 bars" (fun () -> setWindowCount 48)
                        compactButton "ta-view-200" "200" "Show latest 200 bars" (fun () -> setWindowCount 200)
                        compactButton "ta-view-all" label ("Show up to " + string capped + " loaded bars") showLoadedCoverage
                        compactButton "ta-jump-loaded-end" "→|" "Jump to the latest loaded bars" (fun () -> jumpToLoadedCoverageEdge TaLoadedCoverageEdge.End)
                    ] :> Doc)
                |> Doc.EmbedView
            ]

        div [
            attr.``class`` "ptcs-ta-workspace"
            Attr.Create "data-testid" "ta-workspace"
            Attr.Dynamic "data-display-time-zone" (displayTime.Zone |> View.Map SduiDisplayTimeZone.id)
            attr.style "display:flex; flex-direction:column; min-width:0; width:100%; min-height:640px; color:#142033; background:#f4f7fb; font-family:Segoe UI, Arial, sans-serif; letter-spacing:0;"
        ] [
            runtimeState.View
            |> View.MapCachedBy sameDocumentShell (fun state ->
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
                    let currentPlotPalette = plotPalette document.DefaultView
                    let currentDocumentKey = state.Identity, state.DocumentRevision
                    if synchronizedDocumentKey <> Some currentDocumentKey then
                        let query = RendererModel.queryDraft document.DefaultView
                        instrumentDraft <- query.Instrument
                        intervalDraft <- query.IntervalMinutes
                        fromDateDraft <- query.FromUtc
                        toDateDraft <- query.ToUtcExclusive
                        let currentSchemas = editorSchemasNow ()
                        if currentSchemas |> Array.exists (fun schema -> schema.TemplateKey = selectedTemplate.Value) |> not then
                            match currentSchemas |> Array.tryHead with
                            | Some schema -> resetEditorFor schema.TemplateKey
                            | None -> closeRowEditor ()
                        match pendingAddRowId with
                        | Some rowId when document.Rows |> Array.exists (fun row -> row.RowId = rowId) ->
                            pendingAddRowId <- None
                            setUiState { uiState.Value with AddRowOpen = false; Feedback = "Row added." }
                        | _ -> ()
                        completeEditorMutation state.Identity document state.DocumentRevision
                        synchronizedDocumentKey <- Some currentDocumentKey

                    div [ attr.style "display:flex; flex-direction:column; min-width:0;" ] [
                        header [ attr.style "display:flex; flex-direction:column; gap:7px; padding:10px 12px 8px; background:#fff; border-bottom:1px solid #dbe3ee;" ] [
                            div [ attr.style "display:flex; align-items:center; justify-content:space-between; gap:10px; flex-wrap:wrap;" ] [
                                div [ attr.style "min-width:0;" ] [
                                    h2 [ Attr.Create "data-testid" "ta-workspace-title"; attr.style "margin:0; font-size:17px; line-height:22px; font-weight:700; color:#152944;" ] [ text document.Title ]
                                    div [ Attr.Create "data-testid" "ta-canvas-identity"; attr.style "font-size:11px; color:#667891; white-space:nowrap; overflow:hidden; text-overflow:ellipsis;" ] [
                                        runtimeState.View
                                        |> View.Map (fun current -> "canvas " + canvasIdText current.Identity.CanvasInstanceId + " / revision " + string current.DataRevision)
                                        |> textView
                                    ]
                                ]
                                View.Map2 (fun current zone -> current, zone) runtimeState.View displayTime.Zone
                                |> View.Map (fun (current, zone) ->
                                    let status = RendererModel.statusPresentation document.StatusRef current
                                    div [ attr.style "display:flex; align-items:center; gap:5px; flex-wrap:wrap; justify-content:flex-end;" ] [
                                        div [ Attr.Create "data-testid" "ta-freshness"; Attr.Create "data-freshness" (freshnessClass status.Freshness); attr.style "border:1px solid #9fb0c6; border-radius:4px; padding:3px 7px; font-size:11px; font-weight:650; color:#27415f; background:#f8fafc;" ] [ text status.Label ]
                                        div [ Attr.Create "data-testid" "ta-poll-state"; Attr.Create "data-poll-state" (pollText current.Poll); attr.style "border:1px solid #c3cfdd; border-radius:4px; padding:3px 7px; font-size:10px; color:#53667d; background:#fff;" ] [ text (pollText current.Poll) ]
                                    ] :> Doc)
                                |> Doc.EmbedView
                            ]
                            View.Map2 (fun current zone -> current, zone) runtimeState.View displayTime.Zone
                            |> View.Map (fun (current, zone) ->
                                let status = RendererModel.statusPresentation document.StatusRef current
                                div [ Attr.Create "data-testid" "ta-status-detail"; Attr.Create "data-display-time-zone" (SduiDisplayTimeZone.id zone); attr.style "display:flex; gap:10px; flex-wrap:wrap; min-height:16px; font-size:10px; color:#60738b;" ] [
                                    match status.Watermark with
                                    | Some value -> yield span [ Attr.Create "data-canonical-event-time" value ] [ text ("watermark " + TaDisplayTimeFormatter.fullOrOriginal zone value) ]
                                    | None -> ()
                                    match status.Quality with
                                    | Some value -> yield span [] [ text ("quality " + value) ]
                                    | None -> ()
                                    match status.Error with
                                    | Some value -> yield span [ Attr.Create "data-testid" "ta-last-good-error"; attr.style "color:#a33b43; font-weight:600;" ] [ text value ]
                                    | None -> ()
                                ] :> Doc)
                            |> Doc.EmbedView
                            div [ Attr.Create "data-testid" "ta-query-toolbar"; attr.style "display:grid; grid-template-columns:repeat(auto-fit,minmax(120px,1fr)); gap:6px; align-items:end;" ] [
                                label [ attr.style "display:flex; flex-direction:column; gap:2px; min-width:0; font-size:10px; color:#60738b;" ] [ text "Instrument"; inputText "ta-instrument" "Instrument" instrumentDraft (fun value -> instrumentDraft <- value) ]
                                label [ attr.style "display:flex; flex-direction:column; gap:2px; min-width:0; font-size:10px; color:#60738b;" ] [ text "Interval"; selectInput "ta-interval" intervalDraft [ "1", "1m"; "5", "5m"; "30", "30m"; "60", "60m"; "930", "Session" ] (fun value -> intervalDraft <- value) ]
                                label [ attr.style "display:flex; flex-direction:column; gap:2px; min-width:0; font-size:10px; color:#60738b;" ] [ text "From"; inputText "ta-from" "YYYY-MM-DD" fromDateDraft (fun value -> fromDateDraft <- value) ]
                                label [ attr.style "display:flex; flex-direction:column; gap:2px; min-width:0; font-size:10px; color:#60738b;" ] [ text "To"; inputText "ta-to" "YYYY-MM-DD" toDateDraft (fun value -> toDateDraft <- value) ]
                                primaryButtonView
                                    "ta-apply-query"
                                    "Load / Apply"
                                    commandsDisabledView
                                    commandsDisabledNow
                                    applyQuery
                            ]
                            div [ Attr.Create "data-testid" "ta-local-toolbar"; attr.style "display:flex; align-items:center; gap:5px; flex-wrap:wrap;" ]
                                ([ compactRemoteButton "ta-pan-left" "←" "Pan earlier" viewportCommandsDisabledView viewportCommandsDisabledNow (fun () ->
                                       let visible = resolvedWindow uiState.Value
                                       panWindow (-max 1 visible.Count))
                                   compactRemoteButton "ta-pan-right" "→" "Pan later" viewportCommandsDisabledView viewportCommandsDisabledNow (fun () ->
                                       let visible = resolvedWindow uiState.Value
                                       panWindow (max 1 visible.Count))
                                   compactRemoteButton "ta-zoom-in" "+" "Show fewer bars" viewportCommandsDisabledView viewportCommandsDisabledNow (fun () -> zoomWindow -8)
                                   compactRemoteButton "ta-zoom-out" "−" "Show more bars" viewportCommandsDisabledView viewportCommandsDisabledNow (fun () -> zoomWindow 8)
                                   compactRemoteButton "ta-reset-view" "Reset View" "Reset local viewport to the latest bars" viewportCommandsDisabledView viewportCommandsDisabledNow resetWindow
                                   compactRemoteButton "ta-reset-canvas" "Reset Canvas" "Request server canvas reset" commandsDisabledView commandsDisabledNow (fun () ->
                                       startAction
                                           (SduiAction.ResetCanvas(currentCanvasId ()))
                                           "Canvas reset accepted."
                                           (fun () ->
                                               setUiState
                                                   { uiState.Value with
                                                       HiddenRows = Set.empty
                                                       HiddenTraces = Set.empty
                                                       RemovedTraces = Set.empty })) ]
                                 @ (if (editorSchemasNow ()).Length > 0 then
                                        [ compactButton "ta-add-row-toggle" "Add Row" "Open row request editor" (fun () ->
                                              if uiState.Value.AddRowOpen then closeRowEditor ()
                                              else openNewRowEditor ()) ]
                                    else
                                        [])
                                  @ [ span [ attr.style "margin-left:auto; color:#60738b; font-size:11px;" ] [ text "viewport changes request the selected event-time range when enabled" ] ])
                            uiState.View
                            |> View.Map (fun ui ->
                                div [ Attr.Create "data-testid" "ta-row-toggles"; attr.style "display:flex; flex-direction:column; align-items:stretch; gap:4px; width:100%; min-width:0;" ] [
                                    for row in document.Rows do
                                        let hidden = Set.contains row.RowId ui.HiddenRows
                                        let displayLabel = rowDisplayLabelWithEditor (editorSchemasNow ()) row
                                        let editable =
                                            match TaRowEditorBinding.tryResolve (editorSchemasNow ()) row with
                                            | Ok(Some _) -> true
                                            | _ -> false
                                        let controllableTraces =
                                            RendererModel.effectiveTraces row
                                            |> Array.filter (fun trace ->
                                                trace.Visible
                                                && trace.Kind <> TaTraceKind.Marker
                                                && trace.Kind <> TaTraceKind.OverviewStripe
                                                && not (Set.contains (row.RowId, trace.TraceId) ui.RemovedTraces))
                                        yield
                                            div [
                                                Attr.Create "data-testid" ("ta-row-control-line-" + row.RowId)
                                                Attr.Create "data-row-id" row.RowId
                                                attr.style "display:flex; align-items:center; gap:5px; width:100%; min-width:0; height:40px; max-height:40px; overflow:hidden; box-sizing:border-box;"
                                            ] [
                                                yield div [
                                                    Attr.Create "data-testid" ("ta-row-controls-" + row.RowId)
                                                    attr.style "display:inline-flex; align-items:stretch; flex:0 1 auto; min-width:0; max-width:100%; height:26px; white-space:nowrap;"
                                                ] [
                                                    yield button [
                                                        attr.``type`` "button"
                                                        Attr.Create "data-testid" ("ta-toggle-row-" + row.RowId)
                                                        Attr.Create "aria-pressed" (if hidden then "false" else "true")
                                                        attr.title displayLabel
                                                        attr.style (if hidden then "height:26px; min-width:0; max-width:360px; flex:1 1 auto; overflow:hidden; text-overflow:ellipsis; white-space:nowrap; border:1px solid #c8d2df; border-right:0; border-radius:4px 0 0 4px; background:#fff; color:#7a8798; padding:2px 7px; font-size:11px; cursor:pointer;" else "height:26px; min-width:0; max-width:360px; flex:1 1 auto; overflow:hidden; text-overflow:ellipsis; white-space:nowrap; border:1px solid #7da39d; border-right:0; border-radius:4px 0 0 4px; background:#edf8f6; color:#155d55; padding:2px 7px; font-size:11px; cursor:pointer;")
                                                        on.click (fun _ _ ->
                                                            let nextHidden =
                                                                if hidden then Set.remove row.RowId uiState.Value.HiddenRows
                                                                else Set.add row.RowId uiState.Value.HiddenRows

                                                            setUiState { uiState.Value with HiddenRows = nextHidden })
                                                    ] [ text displayLabel ]
                                                    if editable then
                                                        yield button [
                                                            attr.``type`` "button"
                                                            Attr.Create "data-testid" ("ta-edit-row-" + row.RowId)
                                                            attr.title ("Edit " + displayLabel + " parameters")
                                                            attr.disabledBool commandsDisabledView
                                                            Attr.Dynamic "style" (commandsDisabledView |> View.Map (fun disabled ->
                                                                if disabled then "width:52px; min-width:52px; height:26px; flex:0 0 52px; border:1px solid #c8d2df; border-right:0; background:#edf1f5; color:#8b98a8; padding:2px 7px; font-size:11px; font-weight:600; cursor:not-allowed;"
                                                                else "width:52px; min-width:52px; height:26px; flex:0 0 52px; border:1px solid #7f9fbe; border-right:0; background:#e8f2ff; color:#174f82; padding:2px 7px; font-size:11px; font-weight:600; cursor:pointer;"))
                                                            on.click (fun _ _ ->
                                                                if not (commandsDisabledNow ()) then openRowEditor row)
                                                        ] [ text "Edit" ]
                                                    if not (actionAllowed "remove-trace") then
                                                        yield button [
                                                            attr.``type`` "button"
                                                            Attr.Create "data-testid" ("ta-remove-row-" + row.RowId)
                                                            attr.title ("Remove " + displayLabel + " row")
                                                            attr.disabledBool commandsDisabledView
                                                            Attr.Dynamic "style" (commandsDisabledView |> View.Map (fun disabled ->
                                                                if disabled then "width:26px; height:26px; border:1px solid #c8d2df; border-radius:0 4px 4px 0; background:#edf1f5; color:#8b98a8; padding:0; font-size:14px; cursor:not-allowed;"
                                                                else "width:26px; height:26px; border:1px solid #c8a7ab; border-radius:0 4px 4px 0; background:#fff; color:#8d3039; padding:0; font-size:14px; cursor:pointer;"))
                                                            on.click (fun _ _ ->
                                                                if not (commandsDisabledNow ()) then
                                                                    startAction (SduiAction.RemoveTaRow(currentCanvasId (), row.RowId)) (displayLabel + " row removal accepted.") ignore)
                                                        ] [ text "×" ]
                                                ]
                                                if controllableTraces.Length > 0 then
                                                    yield div [
                                                        Attr.Create "data-testid" ("ta-trace-toggles-" + row.RowId)
                                                        attr.style "display:flex; align-items:center; flex:1 1 auto; min-width:0; height:38px; gap:3px; padding:0 0 2px 3px; border-left:1px solid #d7e0eb; box-sizing:border-box; overflow-x:auto; overflow-y:hidden; flex-wrap:nowrap; white-space:nowrap;"
                                                    ] [
                                                        for trace in controllableTraces do
                                                            let traceHidden = Set.contains (row.RowId, trace.TraceId) ui.HiddenTraces
                                                            let traceLabel = if String.IsNullOrWhiteSpace trace.Label then trace.TraceId else trace.Label
                                                            yield div [ attr.style "display:inline-flex; align-items:stretch; flex:0 0 auto; height:24px;" ] [
                                                                yield button [
                                                                    attr.``type`` "button"
                                                                    Attr.Create "data-testid" ("ta-toggle-trace-" + row.RowId + "-" + trace.TraceId)
                                                                    Attr.Create "data-row-id" row.RowId
                                                                    Attr.Create "data-trace-id" trace.TraceId
                                                                    Attr.Create "aria-pressed" (if traceHidden then "false" else "true")
                                                                    attr.title ((if traceHidden then "Show " else "Hide ") + traceLabel + " trace")
                                                                    attr.style (if traceHidden then "height:24px; border:1px solid #c8d2df; border-radius:4px 0 0 4px; background:#fff; color:#7a8798; padding:2px 7px; font-size:10px; cursor:pointer;" else "height:24px; border:1px solid #9cb3cc; border-radius:4px 0 0 4px; background:#f4f8fc; color:#315d88; padding:2px 7px; font-size:10px; cursor:pointer;")
                                                                    on.click (fun _ _ ->
                                                                        let key = row.RowId, trace.TraceId
                                                                        let nextHidden =
                                                                            if traceHidden then Set.remove key uiState.Value.HiddenTraces
                                                                            else Set.add key uiState.Value.HiddenTraces
                                                                        setUiState { uiState.Value with HiddenTraces = nextHidden })
                                                                ] [ text traceLabel ]
                                                                if actionAllowed "remove-trace" then
                                                                    yield button [
                                                                        attr.``type`` "button"
                                                                        Attr.Create "data-testid" ("ta-remove-trace-" + row.RowId + "-" + trace.TraceId)
                                                                        attr.title ("Remove " + traceLabel + " trace until Reset Canvas")
                                                                        attr.disabledBool commandsDisabledView
                                                                        Attr.Dynamic "style" (commandsDisabledView |> View.Map (fun disabled ->
                                                                            if disabled then "width:24px; height:24px; border:1px solid #c8d2df; border-left:0; border-radius:0 4px 4px 0; background:#edf1f5; color:#8b98a8; padding:0; font-size:13px; cursor:not-allowed;"
                                                                            else "width:24px; height:24px; border:1px solid #c8a7ab; border-left:0; border-radius:0 4px 4px 0; background:#fff; color:#8d3039; padding:0; font-size:13px; cursor:pointer;"))
                                                                        on.click (fun _ _ ->
                                                                            if not (commandsDisabledNow ()) then
                                                                                let key = row.RowId, trace.TraceId
                                                                                startAction
                                                                                    (SduiAction.RemoveTaTrace(currentCanvasId (), row.RowId, trace.TraceId))
                                                                                    (traceLabel + " trace removal accepted.")
                                                                                    (fun () ->
                                                                                        setUiState
                                                                                            { uiState.Value with
                                                                                                HiddenTraces = Set.remove key uiState.Value.HiddenTraces
                                                                                                RemovedTraces = Set.add key uiState.Value.RemovedTraces }))
                                                                    ] [ text "×" ]
                                                            ]
                                                    ]
                                            ]
                                ] :> Doc)
                            |> Doc.EmbedView
                            uiState.View
                            |> View.Map (fun ui ->
                                if not ui.AddRowOpen then Doc.Empty
                                else
                                    div [ Attr.Create "data-testid" "ta-add-row-editor"; attr.style "display:flex; flex-direction:column; gap:7px; padding:7px; border:1px solid #cbd6e5; border-radius:5px; background:#f8fafc;" ] [
                                        genericEditor ()
                                        div [ attr.style "display:flex; align-items:center; justify-content:flex-end; gap:6px;" ] [
                                            match editingRowId with
                                            | Some rowId ->
                                                yield span [ Attr.Create "data-testid" "ta-row-editor-mode"; attr.style "margin-right:auto; font-size:11px; color:#40536d;" ] [ text ("Editing " + rowId) ]
                                            | None -> ()
                                            yield compactButton "ta-add-row-cancel" "Cancel" "Close without submitting" closeRowEditor
                                            yield primaryButtonView "ta-add-row-submit" (if editingRowId.IsSome then "Apply" else "Add") commandsDisabledView commandsDisabledNow addRow
                                        ]
                                    ] :> Doc)
                            |> Doc.EmbedView
                            uiState.View
                            |> View.Map (fun ui ->
                                if String.IsNullOrWhiteSpace ui.Feedback then Doc.Empty
                                else div [ Attr.Create "data-testid" "ta-feedback"; attr.style "font-size:11px; color:#40536d; min-height:15px;" ] [ text ui.Feedback ] :> Doc)
                            |> Doc.EmbedView
                        ]
                        viewportControls document
                        View.Map3 (fun (state: RuntimeState) ui preparedDataForShell ->
                            chartRenderSequence <- chartRenderSequence + 1
                            let renderSequence = chartRenderSequence
                            chartWorkGeneration <- chartWorkGeneration + 1
                            dataWorkGeneration <- dataWorkGeneration + 1
                            let workGeneration = chartWorkGeneration
                            preparedRowsReady.Value <- false
                            let projectionCandidateGeneration = pendingProjectionFor state
                            chartStackElement <- null
                            cursorPanelElement <- null
                            let visibleRows =
                                if preparedDataReady then
                                    document.Rows
                                    |> Array.filter (fun row ->
                                        row.Visible
                                        && not (Set.contains row.RowId ui.HiddenRows)
                                        && (RendererModel.effectiveTraces row
                                            |> Array.exists (fun trace ->
                                                trace.Visible
                                                && trace.Kind <> TaTraceKind.Marker
                                                && trace.Kind <> TaTraceKind.OverviewStripe
                                                && not (Set.contains (row.RowId, trace.TraceId) ui.RemovedTraces))))
                                else
                                    [||]
                            let cursorReaderCount =
                                visibleRows
                                |> Array.sumBy (fun row ->
                                    RendererModel.effectiveTraces row
                                    |> Array.filter (fun trace ->
                                        let key = row.RowId, trace.TraceId
                                        trace.Visible && not (Set.contains key ui.HiddenTraces) && not (Set.contains key ui.RemovedTraces))
                                    |> Array.length)
                            let referenceTimeline = RendererModel.referenceTimelineForDocumentPrepared document preparedDataForShell
                            let referenceLength = referenceTimeline.Length
                            let maximumVisibleBars = maximumVisibleBarsFor document
                            let visibleWindow =
                                RendererModel.resolveWindow
                                    options.MinimumVisibleBars
                                    maximumVisibleBars
                                    referenceLength
                                    ui.FollowLatest
                                    ui.Window
                            let coverageProjection = RendererModel.tryLoadedCoverageResolved document.DefaultView preparedDataForShell.RawData
                            let navigatorWindow window =
                                RendererModel.tryCoverageNavigatorWindowResolved referenceLength window document.DefaultView preparedDataForShell.RawData
                                |> Option.map (fun (_, total, globalWindow) -> total, globalWindow)
                                |> Option.defaultValue (referenceLength, window)
                            let loadedObservationCount, globalVisibleWindow = navigatorWindow visibleWindow
                            let activeDetailStart =
                                coverageProjection
                                |> Option.map (fun value -> string value.ActiveDetail.StartObservationOrdinal)
                                |> Option.defaultValue ""
                            let activeDetailCount =
                                coverageProjection
                                |> Option.map (fun value -> string value.ActiveDetail.ObservationCount)
                                |> Option.defaultValue ""
                            let visibleTimestamps = RendererModel.selectWindow visibleWindow referenceTimeline
                            let rowDataStates = visibleRows |> Array.map (fun _ -> Var.Create preparedDataForShell)
                            let rowHeights =
                                visibleRows
                                |> Array.map (fun row ->
                                    let traces =
                                        RendererModel.effectiveTraces row
                                        |> Array.filter (fun trace ->
                                            let key = row.RowId, trace.TraceId
                                            trace.Visible && not (Set.contains key ui.HiddenTraces) && not (Set.contains key ui.RemovedTraces))
                                    rowHeightStateFor row traces)
                            let readyRowCount = Var.Create 0
                            let currentRowLegendElements: Element array = Array.create visibleRows.Length null
                            latestRowLegendElements <- currentRowLegendElements
                            let rowDocs =
                                visibleRows
                                |> Array.mapi (fun index row ->
                                    let reservedHeight = rowHeights[index].Value + 82
                                    Var.Create<Doc>(
                                        div [
                                            Attr.Create "data-testid" ("ta-row-loading-" + row.RowId)
                                            attr.style ($"height:{reservedHeight}px; min-height:{reservedHeight}px; padding:12px; border-top:1px solid #e1e7ef; box-sizing:border-box; color:#718197; background:#fff;")
                                        ] [ text ("Preparing " + rowDisplayLabel row + "...") ] :> Doc))
                            let stagedCursorReaders: ((int -> TaCursorValue option) array option) array = Array.create visibleRows.Length None
                            let stagedLegendReaders: ((int -> TaRowValuePresentation option) array option) array = Array.create visibleRows.Length None
                            let stagedLegendValueReaders: ((unit -> TaRowValuePresentation option) array option) array = Array.create visibleRows.Length None
                            let stagedMarkerCursorReaders: ((int -> TaMarkerCursorItem array) option) array = Array.create visibleRows.Length None
                            activeRowDataStates <- rowDataStates
                            latestCursorTimestamps <- visibleTimestamps
                            latestCursorReaders <- [||]
                            latestLegendReaders <- Map.empty
                            latestLegendValueReaders <- Map.empty
                            latestMarkerCursorReaders <- Map.empty

                            let synchronizeReaders () =
                                latestCursorReaders <- stagedCursorReaders |> Array.choose id |> Array.collect id
                                latestLegendReaders <-
                                    stagedLegendReaders
                                    |> Array.mapi (fun index readers -> readers |> Option.map (fun values -> visibleRows[index].RowId, values))
                                    |> Array.choose id
                                    |> Map.ofArray
                                latestLegendValueReaders <-
                                    stagedLegendValueReaders
                                    |> Array.mapi (fun index readers -> readers |> Option.map (fun values -> visibleRows[index].RowId, values))
                                    |> Array.choose id
                                    |> Map.ofArray
                                latestMarkerCursorReaders <-
                                    stagedMarkerCursorReaders
                                    |> Array.mapi (fun index reader -> reader |> Option.map (fun value -> visibleRows[index].RowId, value))
                                    |> Array.choose id
                                    |> Map.ofArray
                                applyCursorIndex (displayedCursorIndex |> Option.orElse cursorIndex.Value)

                            let rec mountRow index =
                                if workGeneration = chartWorkGeneration then
                                    if index < visibleRows.Length then
                                        JS.SetTimeout (fun () ->
                                            if workGeneration = chartWorkGeneration then
                                                let prepared = rowDataStates[index].Value
                                                let rowDoc, cursorReaders, legendReaders, latestLegendReadersForRow, markerCursorReader =
                                                    renderRowReactivePreparedLiveWithHeightPalette
                                                        currentPlotPalette
                                                        displayTime
                                                        state
                                                        ui
                                                        prepared
                                                        rowDataStates[index].View
                                                        visibleTimestamps
                                                        cursorIndex.View
                                                        setCursorIndex
                                                        commitCursorIndex
                                                        true
                                                        (document.BaseRowId = Some visibleRows[index].RowId)
                                                        rowHeights[index]
                                                        scheduleCursorGeometryRefresh
                                                        (fun node ->
                                                            currentRowLegendElements[index] <- node
                                                            scheduleVisibleValueRefresh ())
                                                        visibleRows[index]
                                                stagedCursorReaders[index] <- Some cursorReaders
                                                stagedLegendReaders[index] <- Some legendReaders
                                                stagedLegendValueReaders[index] <- Some latestLegendReadersForRow
                                                stagedMarkerCursorReaders[index] <- Some markerCursorReader
                                                rowDocs[index].Value <- rowDoc
                                                readyRowCount.Value <- index + 1
                                                mountRow (index + 1)) 0
                                        |> ignore
                                    else
                                        synchronizeReaders ()
                                        preparedRowsReady.Value <-
                                            RendererModel.arePreparedRowsReady visibleRows.Length readyRowCount.Value
                                        projectionCandidateGeneration
                                        |> Option.iter (fun generation -> completeProjection generation state)
                            mountRow 0

                            let visibleStart = if globalVisibleWindow.Count = 0 then 0 else globalVisibleWindow.StartIndex + 1
                            let visibleEnd = globalVisibleWindow.StartIndex + globalVisibleWindow.Count
                            div [
                                Attr.Create "data-testid" "ta-chart-stack"
                                Attr.Create "data-chart-render-sequence" (string renderSequence)
                                Attr.Create "data-chart-preparation-generation" (string preparationGeneration)
                                Attr.Create "data-chart-render-reason" chartRenderReason
                                Attr.Dynamic "data-chart-document-revision" (runtimeState.View |> View.Map (fun current -> string current.DocumentRevision))
                                Attr.Dynamic "data-chart-data-revision" (runtimeState.View |> View.Map (fun current -> string current.DataRevision))
                                Attr.Dynamic "data-chart-transport-sequence" (runtimeState.View |> View.Map (fun current -> string current.LastTransportSequence))
                                Attr.Create "data-loaded-bars" (string loadedObservationCount)
                                Attr.Create "data-active-reference-bars" (string referenceLength)
                                Attr.Create "data-local-visible-start" (string visibleWindow.StartIndex)
                                Attr.Create "data-local-visible-count" (string visibleWindow.Count)
                                Attr.Create "data-active-detail-start" activeDetailStart
                                Attr.Create "data-active-detail-count" activeDetailCount
                                Attr.Create "data-visible-start" (string visibleStart)
                                Attr.Create "data-visible-end" (string visibleEnd)
                                Attr.Create "data-maximum-visible-bars" (string maximumVisibleBars)
                                Attr.Create "data-coverage-identity" (coverageProjection |> Option.map _.CoverageIdentity |> Option.defaultValue "")
                                Attr.Dynamic "data-coverage-revision" (runtimeState.View |> View.Map (fun current -> current.Document |> Option.bind (fun value -> RendererModel.tryLoadedCoverageResolved value.DefaultView current.Data) |> Option.map (fun value -> string value.CoverageRevision) |> Option.defaultValue ""))
                                Attr.Dynamic "data-query-generation" (runtimeState.View |> View.Map (fun current -> current.Document |> Option.bind (fun value -> RendererModel.tryLoadedCoverageResolved value.DefaultView current.Data) |> Option.map (fun value -> string value.QueryGeneration) |> Option.defaultValue ""))
                                Attr.Create "data-follow-latest" (if ui.FollowLatest then "true" else "false")
                                Attr.Create "data-row-count" (string visibleRows.Length)
                                Attr.Create "data-visible-value-query-scope" "cursor-panel+row-legends"
                                Attr.Dynamic "data-ready-row-count" (readyRowCount.View |> View.Map string)
                                Attr.Create "data-cursor-index" ""
                                attr.style "display:flex; flex-direction:column; min-width:0; padding:0 12px 14px;"
                                on.afterRender (fun node ->
                                    chartStackElement <- node
                                    latestCursorTimestamps <- visibleTimestamps
                                    applyCursorIndex cursorIndex.Value)
                            ] [
                                yield div [
                                    Attr.Create "data-testid" "ta-cursor-panel"
                                    attr.style "order:1; display:flex; flex-direction:column; align-items:stretch; border-top:1px solid #dce4ef; background:#f8fafc;"
                                    on.afterRender (fun node ->
                                        cursorPanelElement <- node
                                        scheduleVisibleValueRefresh ())
                                ] [
                                    yield button [
                                        attr.``type`` "button"
                                        Attr.Create "data-testid" "ta-cross-scale-values-toggle"
                                        Attr.Dynamic "aria-expanded" (crossScaleSummaryOpen.View |> View.Map (fun expanded -> if expanded then "true" else "false"))
                                        attr.style "height:28px; min-height:28px; padding:0 8px; border:0; background:#f8fafc; color:#40536d; font-size:11px; font-weight:650; text-align:left; cursor:pointer;"
                                        on.click (fun _ _ -> crossScaleSummaryOpen.Value <- not crossScaleSummaryOpen.Value)
                                    ] [ textView (crossScaleSummaryOpen.View |> View.Map (fun expanded -> if expanded then "Hide cross-scale values" else "Show cross-scale values")) ]
                                    yield div [
                                        Attr.Create "data-testid" "ta-cross-scale-values"
                                        Attr.Dynamic "data-expanded" (crossScaleSummaryOpen.View |> View.Map (fun expanded -> if expanded then "true" else "false"))
                                        Attr.Dynamic "style" (crossScaleSummaryOpen.View |> View.Map (fun expanded ->
                                            if expanded then "display:flex; align-items:center; gap:4px 12px; height:30px; min-height:30px; padding:0 8px; overflow-x:auto; overflow-y:hidden; white-space:nowrap; font-family:Consolas,monospace; font-size:11px; line-height:16px; color:#263b55;"
                                            else "display:none; height:30px; min-height:30px;"))
                                    ] [
                                        yield span [ Attr.Create "data-ta-cursor-hint" "true"; attr.style "flex:0 0 auto; font-size:11px; color:#718197;" ] [ text "Move the pointer over any chart row to inspect one shared bar." ]
                                        yield strong [ Attr.Create "data-ta-cursor-time" "true"; Attr.Create "hidden" "hidden"; attr.style "flex:0 0 auto; white-space:nowrap;" ] [ text "" ]
                                        for index in 0 .. cursorReaderCount - 1 do
                                            yield span [ Attr.Create "data-ta-cursor-value-index" (string index); Attr.Create "hidden" "hidden"; attr.style "flex:0 0 auto; white-space:nowrap;" ] [ text "" ]
                                    ]
                                ]
                                if visibleRows.Length = 0 then
                                    yield div [ attr.style "padding:18px; color:#667891;" ] [ text "No visible TA rows." ]
                                else
                                    for rowDoc in rowDocs do
                                        yield rowDoc.View |> Doc.EmbedView
                                yield div [
                                    Attr.Create "data-testid" "ta-viewport-navigator"
                                    attr.style "order:-1; min-width:0; padding:0 8px 8px; border-bottom:1px solid #d4deea; background:#f8fafc;"
                                ] [
                                    div [ attr.style "min-width:0;" ] [
                                        shellPreparedData.View
                                        |> View.Map (fun currentPreparedData ->
                                            let currentReferenceTimeline =
                                                RendererModel.referenceTimelineForDocumentPrepared document currentPreparedData
                                            let currentReferenceLength = currentReferenceTimeline.Length
                                            let currentCoverageProjection =
                                                RendererModel.tryLoadedCoverageResolved document.DefaultView currentPreparedData.RawData
                                            let overviewReferenceTimeline =
                                                currentCoverageProjection
                                                |> Option.bind RendererModel.tryOverviewTimeline
                                                |> Option.defaultValue currentReferenceTimeline
                                            let overviewPoints =
                                                let projected =
                                                    currentCoverageProjection
                                                    |> Option.map RendererModel.overviewPointsForCoverage
                                                    |> Option.defaultValue [||]
                                                if projected.Length > 0 then projected
                                                else
                                                    visibleRows
                                                    |> Array.collect RendererModel.effectiveTraces
                                                    |> Array.tryFind (fun trace -> trace.Visible && trace.Kind = TaTraceKind.Candlestick)
                                                    |> Option.map (fun trace ->
                                                        RendererModel.candleSeriesForTracePrepared trace currentPreparedData
                                                        |> RendererModel.overviewPointsFromCandles)
                                                    |> Option.defaultValue [||]
                                            let overviewStripeVisuals =
                                                visibleRows
                                                |> Array.collect RendererModel.effectiveTraces
                                                |> Array.filter (fun trace -> trace.Visible && trace.Kind = TaTraceKind.OverviewStripe)
                                                |> Array.collect (fun trace ->
                                                    RendererModel.overviewStripePlacementsPrepared
                                                        trace
                                                        currentPreparedData
                                                        overviewReferenceTimeline)
                                                |> RendererModel.overviewStripeVisuals
                                            let selectionWindow =
                                                View.Map2
                                                    (fun draft currentUi ->
                                                        let currentVisibleWindow =
                                                            RendererModel.resolveWindow
                                                                options.MinimumVisibleBars
                                                                (maximumVisibleBarsFor document)
                                                                currentReferenceLength
                                                                currentUi.FollowLatest
                                                                currentUi.Window
                                                        match draft, currentCoverageProjection with
                                                        | Some globalDraft, Some projection ->
                                                            let domainCount = TaLoadedCoverageCodec.observationDomainCount projection
                                                            if domainCount > 0L && domainCount <= int64 Int32.MaxValue then
                                                                RendererModel.selectionRatios (int domainCount) globalDraft
                                                            else
                                                                RendererModel.selectionRatios currentReferenceLength currentVisibleWindow
                                                        | _ ->
                                                            currentCoverageProjection
                                                            |> Option.bind (fun projection ->
                                                                RendererModel.overviewSelectionRatios projection currentReferenceTimeline currentVisibleWindow)
                                                            |> Option.defaultWith (fun () ->
                                                                currentCoverageProjection
                                                                |> Option.bind (RendererModel.coverageNavigatorWindow currentReferenceLength currentVisibleWindow)
                                                                |> Option.map (fun (_, total, globalSelection) -> RendererModel.selectionRatios total globalSelection)
                                                                |> Option.defaultValue (RendererModel.selectionRatios currentReferenceLength currentVisibleWindow)))
                                                    draftWindow.View
                                                    uiState.View
                                            div [ Attr.Create "data-testid" "ta-overview-with-axis"; attr.style "min-width:0;" ] [
                                                overviewSvgWithPalette
                                                    currentPlotPalette
                                                    overviewPoints
                                                    overviewStripeVisuals
                                                    overviewReferenceTimeline.Length
                                                    selectionWindow
                                                    (fun element ->
                                                        match activeNavigatorCursor with
                                                        | Some cursor ->
                                                            (element |> As<HTMLElement>).Style.SetProperty("cursor", cursor)
                                                        | None ->
                                                            match navigatorCursorAfterRender with
                                                            | Some cursor ->
                                                                navigatorCursorAfterRender <- None
                                                                (element |> As<HTMLElement>).Style.SetProperty("cursor", cursor)
                                                            | None -> ())
                                                    startNavigatorDrag
                                                    finishNavigatorDragFromElement
                                                timeAxisWithPalette
                                                    currentPlotPalette
                                                    displayTime
                                                    "ta-overview-time-axis"
                                                    "overview"
                                                    overviewReferenceTimeline
                                            ] :> Doc)
                                        |> Doc.EmbedView
                                    ]
                                ]
                            ] :> Doc) chartRuntimeView chartUiState.View chartShellPreparedData.View
                        |> Doc.EmbedView
                    ] :> Doc)
            |> Doc.EmbedView
        ]

    let renderWithProjectionCommit
        (options: TaRendererOptions)
        (callbacks: TaRendererCallbacks)
        (onProjectionCommitted: RuntimeState -> unit)
        (runtimeState: Var<RuntimeState>) =
        let zone = Var.Create SduiDisplayTimeZone.Utc
        renderWithProjectionCommitAndDisplayTimeZone options callbacks onProjectionCommitted zone.View runtimeState

    let renderWithDisplayTimeZone
        (options: TaRendererOptions)
        (callbacks: TaRendererCallbacks)
        (displayTimeZone: View<SduiDisplayTimeZone>)
        (runtimeState: Var<RuntimeState>) =
        renderWithProjectionCommitAndDisplayTimeZone options callbacks ignore displayTimeZone runtimeState

    let render (options: TaRendererOptions) (callbacks: TaRendererCallbacks) (runtimeState: Var<RuntimeState>) =
        renderWithProjectionCommit options callbacks ignore runtimeState
