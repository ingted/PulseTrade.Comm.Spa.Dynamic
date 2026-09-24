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
      AddRowOpen: bool
      CursorIndex: int option
      PendingActionId: string option
      Feedback: string }

[<JavaScript>]
module TaWorkspaceRenderer =
    let axisViewportWidth = Var.Create 1440.0
    let mutable axisResizeBound = false

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

    let rowTitle (row: TaRowSpec) (traces: TaTraceSpec array) =
        match rowExplicitLabel row with
        | Some label -> label
        | None when isNull traces || traces.Length = 0 -> rowKindText row.Kind
        | None ->
            traces
            |> Array.map (fun trace -> if String.IsNullOrWhiteSpace trace.Label then trace.TraceId else trace.Label)
            |> String.concat " / "
            |> fun value -> if String.IsNullOrWhiteSpace value then rowKindText row.Kind else value

    let sameDocumentShell (left: RuntimeState) (right: RuntimeState) =
        let samePresence =
            match left.Document, right.Document with
            | None, None -> true
            | Some leftDocument, Some rightDocument -> leftDocument.WorkspaceId = rightDocument.WorkspaceId
            | _ -> false

        left.Identity = right.Identity
        && samePresence
        && left.DocumentRevision = right.DocumentRevision

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
        && left.DocumentRevision = right.DocumentRevision
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
        | RuntimePollState.PollInFlight
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

    let timeAxis testId rowId (timestamps: string array) =
        axisViewportWidth.View
        |> View.Map (fun width ->
            let labels = RendererModel.adaptiveTimeLabels 92.0 width timestamps
            div [
                Attr.Create "data-testid" testId
                Attr.Create "data-time-axis-row-id" rowId
                Attr.Create "data-time-axis-tick-count" (string labels.Length)
                attr.style "position:relative; min-width:0; height:18px; padding:0 1px; overflow:hidden;"
            ] [
                for position in 0 .. labels.Length - 1 do
                    let index, label = labels[position]
                    let left = if timestamps.Length <= 1 then 50.0 else float index / float (timestamps.Length - 1) * 100.0
                    let transform = if position = 0 then "none" elif position = labels.Length - 1 then "translateX(-100%)" else "translateX(-50%)"
                    yield
                        span [
                            Attr.Create "data-time-axis-event-time" label
                            attr.style (
                                "position:absolute; left:" + fixedText left + "%; transform:" + transform
                                + "; max-width:92px; color:#708198; font-size:10px; line-height:16px; white-space:nowrap; overflow:hidden; text-overflow:ellipsis;")
                        ] [ text (compactTimestamp label) ]
            ] :> Doc)
        |> Doc.EmbedView

    let rectanglePath x y width height =
        "M " + fixedText x + " " + fixedText y
        + " h " + fixedText width
        + " v " + fixedText height
        + " h " + fixedText (-width)
        + " Z"

    let overviewSvg points (stripeVisuals: TaOverviewStripeVisual array) referenceLength selectionWindow onReady onDragStart onDragEnd =
        let width = 1000.0
        let height = 82.0
        let stripeTooltip = Var.Create<Option<float * string>>(None)
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
        let maximumHandleWidth = 8.0
        let minimumSelectionWidth = 24.0
        let minimumMoveHitWidth = 4.0
        let selectionGeometry (leftRatio, rightRatio) =
            let requestedX = leftRatio * width
            let requestedWidth = max 0.0 ((rightRatio - leftRatio) * width)
            let displayedWidth = min width (max minimumSelectionWidth requestedWidth)
            let displayedX = max 0.0 (min (width - displayedWidth) requestedX)
            let handleWidth = min maximumHandleWidth ((displayedWidth - minimumMoveHitWidth) / 2.0)
            displayedX, displayedWidth, handleWidth, displayedX + handleWidth, displayedWidth - handleWidth * 2.0
        let geometryText projection = selectionWindow |> View.Map (selectionGeometry >> projection >> fixedText)
        let selectionX = geometryText (fun (selectionX, _, _, _, _) -> selectionX)
        let selectionWidth = geometryText (fun (_, selectionWidth, _, _, _) -> selectionWidth)
        let handleWidth = geometryText (fun (_, _, handleWidth, _, _) -> handleWidth)
        let leftHandleX = selectionX
        let rightHandleX = geometryText (fun (selectionX, selectionWidth, handleWidth, _, _) -> selectionX + selectionWidth - handleWidth)
        let moveHitX = geometryText (fun (_, _, _, moveHitX, _) -> moveHitX)
        let moveHitWidth = geometryText (fun (_, _, _, _, moveHitWidth) -> moveHitWidth)
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
            Attr.Create "data-loaded-sample-count" (string sampled.Length)
            svgAttr "viewBox" "0 0 1000 82"
            svgAttr "preserveAspectRatio" "none"
            attr.style "display:block; width:100%; height:82px; min-width:0; background:#eef3f8; border:1px solid #c7d3e2; border-radius:4px; box-sizing:border-box; touch-action:none;"
            on.afterRender onReady
            on.mouseUp (fun _ event -> onDragEnd event)
            on.mouseMove (fun element event ->
                let bounds = element.GetBoundingClientRect()
                if bounds.Width > 0.0 then
                    let x = max 0.0 (min width ((float event.ClientX - bounds.Left) / bounds.Width * width))
                    let pixel = int (Math.Round x)
                    let candidates =
                        [| pixel; pixel - 1; pixel + 1; pixel - 2; pixel + 2 |]
                        |> Array.tryPick (fun key -> Map.tryFind key stripeBuckets)
                    stripeTooltip.Value <- candidates |> Option.map (fun values -> x, stripeTooltipText values))
            on.mouseLeave (fun _ _ -> stripeTooltip.Value <- None)
        ] [
            yield svgElement "path" [ svgAttr "d" closePath; svgAttr "fill" "none"; svgAttr "stroke" "#3d718e"; svgAttr "stroke-width" "1.5" ] []
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
                svgAttr "fill" "rgba(15,118,110,.10)"; svgAttr "stroke" "#0f766e"; svgAttr "stroke-width" "2"; svgAttr "pointer-events" "none"
            ] []
            yield svgElement "rect" [
                Attr.Create "data-testid" "ta-overview-left-handle"
                Attr.Dynamic "x" leftHandleX; svgAttr "y" "0"; Attr.Dynamic "width" handleWidth; svgAttr "height" "82"
                svgAttr "fill" "#155f73"; svgAttr "fill-opacity" "0.82"; svgAttr "style" "cursor:ew-resize;"
                on.mouseDown (fun _ event -> onDragStart TaWindowDrag.ResizeLeft event)
            ] []
            yield svgElement "rect" [
                Attr.Create "data-testid" "ta-overview-right-handle"
                Attr.Dynamic "x" rightHandleX; svgAttr "y" "0"; Attr.Dynamic "width" handleWidth; svgAttr "height" "82"
                svgAttr "fill" "#155f73"; svgAttr "fill-opacity" "0.82"; svgAttr "style" "cursor:ew-resize;"
                on.mouseDown (fun _ event -> onDragStart TaWindowDrag.ResizeRight event)
            ] []
            yield svgElement "rect" [
                Attr.Create "data-testid" "ta-overview-move-hit"
                Attr.Dynamic "x" moveHitX; svgAttr "y" "0"; Attr.Dynamic "width" moveHitWidth; svgAttr "height" "82"
                svgAttr "fill" "transparent"; svgAttr "style" "cursor:grab;"
                on.mouseDown (fun _ event -> onDragStart TaWindowDrag.Move event)
            ] []
            yield
                stripeTooltip.View
                |> View.Map (function
                    | None -> Doc.Empty
                    | Some(x, value) ->
                        let bounded = if value.Length <= 180 then value else value.Substring(0, 177) + "..."
                        let boxX = max 4.0 (min 716.0 (x + 6.0))
                        svgElement "g" [ Attr.Create "data-testid" "ta-overview-stripe-tooltip"; svgAttr "pointer-events" "none" ] [
                            svgElement "rect" [ svgAttr "x" (fixedText boxX); svgAttr "y" "3"; svgAttr "width" "280"; svgAttr "height" "18"; svgAttr "rx" "2"; svgAttr "fill" "#ffffff"; svgAttr "fill-opacity" "0.95"; svgAttr "stroke" "#8ca0b8"; svgAttr "stroke-width" "0.7" ] []
                            svgElement "text" [ svgAttr "x" (fixedText (boxX + 5.0)); svgAttr "y" "15"; svgAttr "fill" "#263b55"; svgAttr "font-family" "Consolas,monospace"; svgAttr "font-size" "9" ] [ text bounded ]
                        ] :> Doc)
                |> Doc.EmbedView
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

    let compositeSvgReactivePreparedLiveWithHeight rowId isBaseRow (traces: TaTraceSpec array) preparedData (dataView: View<TaPreparedRendererData>) (referenceTimestamps: string array) (cursorIndex: View<int option>) setCursorIndex commitCursorIndex (chartPixelHeight: View<int>) scheduleValueRefresh =
        let width = 1000.0
        let hasCandles = traces |> Array.exists (fun trace -> trace.Kind = TaTraceKind.Candlestick)
        let height = if hasCandles then 250.0 else 112.0
        let top = 10.0
        let plotHeight = if hasCandles then 230.0 else 92.0
        let palette = [| "#2764b0"; "#9b5b24"; "#6a4ca3"; "#0f766e"; "#b45309"; "#be185d"; "#475569"; "#0891b2" |]
        let color index (trace: TaTraceSpec) =
            if String.IsNullOrWhiteSpace trace.Color then palette[index % palette.Length] else trace.Color

        let xAt index =
            RendererModel.slotCenter width referenceTimestamps.Length index
            |> Option.defaultValue (width / 2.0)

        let maximumVisualPoints = 1000

        let compactCandles (values: (int * TaTraceSpec * int * int * TaCandlePoint) array) =
            if referenceTimestamps.Length <= maximumVisualPoints then
                values
            else
                values
                |> Array.groupBy (fun (traceIndex, _, slotIndex, sourceSpanCount, _) ->
                    traceIndex,
                    sourceSpanCount > 1,
                    min (maximumVisualPoints - 1) (slotIndex * maximumVisualPoints / referenceTimestamps.Length))
                |> Array.map (fun (_, bucket) ->
                    let ordered = bucket |> Array.sortBy (fun (_, _, slotIndex, _, _) -> slotIndex)
                    let traceIndex, trace, firstSlot, _, firstPoint = ordered[0]
                    let _, _, lastSlot, _, lastPoint = ordered[ordered.Length - 1]
                    let sourceSpanCount = ordered |> Array.maxBy (fun (_, _, _, span, _) -> span) |> fun (_, _, _, span, _) -> span
                    let aggregate =
                        { lastPoint with
                            Open = firstPoint.Open
                            High = ordered |> Array.maxBy (fun (_, _, _, _, point) -> point.High) |> fun (_, _, _, _, point) -> point.High
                            Low = ordered |> Array.minBy (fun (_, _, _, _, point) -> point.Low) |> fun (_, _, _, _, point) -> point.Low
                            Close = lastPoint.Close
                            Volume = ordered |> Array.sumBy (fun (_, _, _, _, point) -> point.Volume) }
                    traceIndex, trace, (firstSlot + lastSlot) / 2, sourceSpanCount, aggregate)
                |> Array.sortBy (fun (traceIndex, _, slotIndex, _, _) -> traceIndex, slotIndex)

        let compactLinePoints (values: (int * TaLinePoint) array) =
            if referenceTimestamps.Length <= maximumVisualPoints || values.Length <= maximumVisualPoints then
                values
            else
                let bucketCount = max 1 (maximumVisualPoints / 2)
                values
                |> Array.groupBy (fun (slotIndex, _) ->
                    min (bucketCount - 1) (slotIndex * bucketCount / referenceTimestamps.Length))
                |> Array.collect (fun (_, bucket) ->
                    let minimum = bucket |> Array.minBy (fun (_, point) -> point.Value)
                    let maximum = bucket |> Array.maxBy (fun (_, point) -> point.Value)
                    if fst minimum = fst maximum then [| minimum |]
                    else [| minimum; maximum |] |> Array.sortBy fst)
                |> Array.sortBy fst

        let prepareGeometry currentData =
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

            let projectedCandleSeries =
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

            let candleSeries = compactCandles projectedCandleSeries

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
                            |> Option.map (fun target -> RendererModel.markerPlacementsPrepared trace target currentData referenceTimestamps)
                            |> Option.defaultValue [||])
                |> RendererModel.assignAggregateMarkerLanes

            let scaleValues =
                [| yield! projectedCandleSeries |> Array.collect (fun (_, _, _, _, point) -> [| point.Low; point.High |])
                   yield! projectedLinePoints |> Array.collect (fun (_, trace, points) ->
                       let values = points |> Array.map (fun (_, point: TaLinePoint) -> point.Value)
                       if trace.Kind = TaTraceKind.Histogram then Array.append [| 0.0 |] values else values) |]
            let low, high = RendererModel.paddedRange 0.0 1.0 scaleValues
            let readers =
                preparedTraces
                |> Array.map (fun (traceIndex, trace, candles, _) ->
                    let label = if String.IsNullOrWhiteSpace trace.Label then trace.TraceId else trace.Label
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
                        cursorReader, legendReader
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
                        cursorReader, legendReader
                    | TaTraceKind.Marker
                    | TaTraceKind.OverviewStripe ->
                        (fun _ -> None), (fun _ -> None))
            let cursorReaders = readers |> Array.map fst
            let legendReaders = readers |> Array.map snd

            preparedTraces, candleSeries, linePoints, markerPlacements, cursorReaders, legendReaders, low, high

        let initialGeometry = prepareGeometry preparedData
        let _, initialCandleSeries, initialLinePoints, initialMarkerPlacements, initialCursorReaders, initialLegendReaders, initialLow, initialHigh = initialGeometry
        let markerVisualState = Var.Create(initialMarkerPlacements, initialLow, initialHigh)
        let readerStates =
            Array.map2 (fun cursorReader legendReader -> ref (cursorReader, legendReader)) initialCursorReaders initialLegendReaders

        let slot = if referenceTimestamps.Length = 0 then width else width / float referenceTimestamps.Length
        let svgTestId = if hasCandles then "ta-candle-" + rowId else "ta-composite-" + rowId

        let candlePaths traceIndex (_, currentCandles, _, _, _, _, low, high) =
            let buckets = Array.init 8 (fun _ -> ResizeArray<string>())
            for currentTraceIndex, _, slotIndex, sourceSpanCount, point in currentCandles do
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

        let lineGeometry traceIndex (_, _, currentLines, _, _, _, low, high) =
            let _, _, points =
                currentLines
                |> Array.tryFind (fun (index, _, _) -> index = traceIndex)
                |> Option.defaultWith (fun () -> initialLinePoints |> Array.find (fun (index, _, _) -> index = traceIndex))
            points, low, high

        let linePath traceIndex (trace: TaTraceSpec) geometry =
            let points, low, high = lineGeometry traceIndex geometry
            match trace.Kind with
            | TaTraceKind.Histogram
            | TaTraceKind.Volume ->
                let zeroY = RendererModel.normalize low high top plotHeight 0.0
                let barWidth = max 1.0 (slot * 0.64)
                points
                |> Array.map (fun (index, point: TaLinePoint) ->
                    let x = slot * (float index + 0.18)
                    let valueY = RendererModel.normalize low high top plotHeight point.Value
                    rectanglePath x (min zeroY valueY) barWidth (max 1.0 (abs (zeroY - valueY))))
                |> String.concat " "
            | TaTraceKind.Line ->
                points
                |> Array.map (fun (index, point: TaLinePoint) -> xAt index, RendererModel.normalize low high top plotHeight point.Value)
                |> Array.mapi (fun index (x, y) -> (if index = 0 then "M" else "L") + " " + fixedText x + " " + fixedText y)
                |> String.concat " "
            | _ -> ""

        let lineLastValue traceIndex geometry =
            let points, _, _ = lineGeometry traceIndex geometry
            points
            |> Array.tryLast
            |> Option.map (snd >> _.Value >> fixedText)
            |> Option.defaultValue ""

        let markerShape (placement: TaMarkerPlacement) low high =
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
            let fill, fillOpacity =
                match placement.Marker.Fill with
                | TaMarkerFill.Solid -> placement.Marker.Color, "1"
                | TaMarkerFill.Outline -> "none", "1"
            let common =
                [ Attr.Create "data-testid" ("ta-marker-" + placement.TraceId + "-" + placement.Marker.MarkerId)
                  Attr.Create "data-marker-id" placement.Marker.MarkerId
                  Attr.Create "data-marker-position" (fixedText placement.Position)
                  Attr.Create "data-marker-slot" (string placement.SlotIndex)
                  Attr.Create "data-marker-lane" (string placement.Lane)
                  Attr.Create "data-marker-anchor" (if placement.Marker.Anchor = TaMarkerAnchor.AboveBar then "above-bar" else "below-bar")
                  Attr.Create "data-marker-shape" (TaMarkerCodec.shapeText placement.Marker.Shape)
                  Attr.Create "data-marker-fill" (TaMarkerCodec.fillText placement.Marker.Fill)
                  svgAttr "fill" fill
                  svgAttr "fill-opacity" fillOpacity
                  svgAttr "stroke" placement.Marker.Color
                  svgAttr "stroke-width" "1.4"
                  svgAttr "pointer-events" "all"
                  svgAttr "vector-effect" "non-scaling-stroke" ]
            let title = svgElement "title" [] [ text (RendererModel.markerTooltipText placement) ]
            match placement.Marker.Shape with
            | TaMarkerShape.Circle ->
                svgElement "circle" (common @ [ svgAttr "cx" (fixedText x); svgAttr "cy" (fixedText y); svgAttr "r" (fixedText half) ]) [ title ]
            | TaMarkerShape.Square ->
                svgElement "rect" (common @ [ svgAttr "x" (fixedText (x - half)); svgAttr "y" (fixedText (y - half)); svgAttr "width" (fixedText size); svgAttr "height" (fixedText size) ]) [ title ]
            | TaMarkerShape.Diamond ->
                let points = $"{fixedText x},{fixedText (y - half)} {fixedText (x + half)},{fixedText y} {fixedText x},{fixedText (y + half)} {fixedText (x - half)},{fixedText y}"
                svgElement "polygon" (common @ [ svgAttr "points" points ]) [ title ]
            | TaMarkerShape.TriangleUp
            | TaMarkerShape.TriangleDown ->
                let points =
                    RendererModel.markerTrianglePoints placement.Marker.Shape x y half
                    |> Option.defaultValue [||]
                    |> Array.map (fun (pointX, pointY) -> $"{fixedText pointX},{fixedText pointY}")
                    |> String.concat " "
                svgElement "polygon" (common @ [ svgAttr "points" points ]) [ title ]

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
            View.Map2 (fun ((placements: TaMarkerPlacement array), low, high) viewportWidth ->
                let directPlacements, overflowClusters = RendererModel.markerPresentation placements
                let size = 9.0
                let half = size / 2.0
                let fontSize = max 8.0 (min 30.0 (10000.0 / max 320.0 viewportWidth))
                let shapeLaneStep = size + 2.0
                let labelLaneStep = fontSize + 2.0
                let labelCandidates =
                    directPlacements
                    |> Array.choose (fun placement ->
                        let x = xAt placement.SlotIndex
                        let anchorY =
                            match placement.Marker.Anchor with
                            | TaMarkerAnchor.AboveBar -> RendererModel.normalize low high top plotHeight placement.Target.High
                            | TaMarkerAnchor.BelowBar -> RendererModel.normalize low high top plotHeight placement.Target.Low
                        let baseY =
                            match placement.Marker.Anchor with
                            | TaMarkerAnchor.AboveBar -> anchorY - 4.0 - half - float placement.Lane * shapeLaneStep
                            | TaMarkerAnchor.BelowBar -> anchorY + 4.0 + half + float placement.Lane * shapeLaneStep
                            |> max half
                            |> min (height - half)
                        placement.Marker.Label
                        |> Option.bind (RendererModel.markerLabelGeometry width height fontSize x baseY)
                        |> Option.map (fun geometry -> placement, geometry))
                let collisionLanes =
                    labelCandidates
                    |> Array.map (fun (placement, geometry) -> placement.Marker.Anchor, geometry)
                    |> RendererModel.markerLabelCollisionLanes 6.0
                svgElement "g" [
                    Attr.Create "data-testid" ("ta-marker-layer-" + rowId)
                    Attr.Create "data-marker-count" (string placements.Length)
                    Attr.Create "data-direct-marker-count" (string directPlacements.Length)
                    Attr.Create "data-marker-overflow-count" (string overflowClusters.Length)
                ] [
                    for placement in directPlacements do
                        yield markerShape placement low high
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
                                on.click (fun _ _ -> toggleCluster ())
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
                                    svgAttr "fill" "#ffffff"; svgAttr "stroke" "#40536d"; svgAttr "stroke-width" "1.2"
                                    svgAttr "vector-effect" "non-scaling-stroke"
                                ] []
                                svgElement "text" [
                                    svgAttr "x" (fixedText x); svgAttr "y" (fixedText (y + 0.5))
                                    svgAttr "text-anchor" "middle"; svgAttr "dominant-baseline" "middle"
                                    svgAttr "font-family" "Consolas,monospace"; svgAttr "font-size" "8"
                                    svgAttr "font-weight" "700"; svgAttr "fill" "#263b55"
                                    svgAttr "pointer-events" "none"
                                ] [ text ("+" + string hiddenCount) ]
                                svgElement "title" [] [ text ($"{hiddenCount} additional markers") ]
                            ]
                    for index in 0 .. labelCandidates.Length - 1 do
                        let placement, geometry = labelCandidates[index]
                        let lane = collisionLanes[index]
                        let labelY = RendererModel.markerLabelLaneY height labelLaneStep placement.Marker.Anchor geometry.Y lane
                        yield
                            svgElement "text" [
                                Attr.Create "data-testid" ("ta-marker-label-" + placement.TraceId + "-" + placement.Marker.MarkerId)
                                Attr.Create "data-marker-id" placement.Marker.MarkerId
                                Attr.Create "data-marker-slot" (string placement.SlotIndex)
                                Attr.Create "data-marker-lane" (string placement.Lane)
                                Attr.Create "data-marker-label-lane" (string lane)
                                svgAttr "x" (fixedText geometry.X)
                                svgAttr "y" (fixedText labelY)
                                svgAttr "text-anchor" geometry.TextAnchor
                                svgAttr "dominant-baseline" "middle"
                                svgAttr "fill" placement.Marker.Color
                                svgAttr "font-family" "Consolas,monospace"
                                svgAttr "font-size" (fixedText fontSize)
                                svgAttr "font-weight" "650"
                                svgAttr "paint-order" "stroke"
                                svgAttr "stroke" "#ffffff"
                                svgAttr "stroke-width" "2.5"
                                svgAttr "stroke-linejoin" "round"
                                svgAttr "pointer-events" "none"
                                svgAttr "vector-effect" "non-scaling-stroke"
                            ] [ text geometry.Text ]
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
                                    let textValue = RendererModel.markerTooltipText selected
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
                                        svgElement "rect" [ svgAttr "x" (fixedText boxX); svgAttr "y" (fixedText boxY); svgAttr "width" "304"; svgAttr "height" "24"; svgAttr "rx" "3"; svgAttr "fill" "#ffffff"; svgAttr "fill-opacity" "0.97"; svgAttr "stroke" "#8ca0b8"; svgAttr "stroke-width" "0.8" ] []
                                        svgElement "text" [ svgAttr "x" (fixedText (boxX + 6.0)); svgAttr "y" (fixedText (boxY + 15.0)); svgAttr "fill" "#263b55"; svgAttr "font-family" "Consolas,monospace"; svgAttr "font-size" "9" ] [ text ($"{index + 1}/{cluster.Markers.Length} {bounded}") ]
                                    ] :> Doc)
                        |> Doc.EmbedView
                ]) markerVisualState.View axisViewportWidth.View
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
                traceIndex,
                trace,
                Var.Create(linePath traceIndex trace initialGeometry),
                Var.Create(lineLastValue traceIndex initialGeometry))

        let mutable observedPreparedData = preparedData
        dataView
        |> View.Sink (fun currentData ->
            if not (Object.ReferenceEquals(currentData, observedPreparedData)) then
                observedPreparedData <- currentData
                let geometry = prepareGeometry currentData
                let _, _, _, currentMarkerPlacements, currentCursorReaders, currentLegendReaders, currentLow, currentHigh = geometry

                let nextMarkerVisual = currentMarkerPlacements, currentLow, currentHigh
                if markerVisualState.Value <> nextMarkerVisual then markerVisualState.Value <- nextMarkerVisual

                for index in 0 .. readerStates.Length - 1 do
                    readerStates[index].Value <- currentCursorReaders[index], currentLegendReaders[index]

                for traceIndex, _, pathStates in candlePathStates do
                    let nextPaths = candlePaths traceIndex geometry
                    for index in 0 .. pathStates.Length - 1 do
                        if pathStates[index].Value <> nextPaths[index] then
                            pathStates[index].Value <- nextPaths[index]

                for traceIndex, trace, pathState, lastValueState in lineVisualStates do
                    let nextPath = linePath traceIndex trace geometry
                    let nextLastValue = lineLastValue traceIndex geometry
                    if pathState.Value <> nextPath then pathState.Value <- nextPath
                    if lastValueState.Value <> nextLastValue then lastValueState.Value <- nextLastValue

                scheduleValueRefresh ())

        svgElement "svg" [
            svgAttr "viewBox" ("0 0 1000 " + fixedText height)
            svgAttr "preserveAspectRatio" "none"
            svgAttr "role" "img"
            svgAttr "aria-label" ("Composite TA row " + rowId)
            Attr.Create "data-testid" svgTestId
            Attr.Create "data-point-count" (string referenceTimestamps.Length)
            Attr.Dynamic "style" (chartPixelHeight |> View.Map (fun value -> "display:block; width:100%; height:" + string value + "px; background:#fbfcfe;"))
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
                yield svgElement "line" [ svgAttr "x1" "0"; svgAttr "x2" "1000"; svgAttr "y1" (fixedText y); svgAttr "y2" (fixedText y); svgAttr "stroke" "#e7ecf3"; svgAttr "stroke-width" "1" ] []

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
                let _, _, pathState, lastValueState = lineVisualStates |> Array.find (fun (index, _, _, _) -> index = traceIndex)
                let path = pathState.View
                let lastValue = lastValueState.View
                match trace.Kind with
                | TaTraceKind.Histogram
                | TaTraceKind.Volume ->
                    yield svgElement "path" [ Attr.Create "data-testid" ("ta-trace-" + rowId + "-" + trace.TraceId); Attr.Dynamic "d" path; Attr.Dynamic "data-last-value" lastValue; svgAttr "fill" traceColor; svgAttr "fill-opacity" "0.62" ] []
                | TaTraceKind.Line ->
                    yield svgElement "path" [ Attr.Create "data-testid" ("ta-trace-" + rowId + "-" + trace.TraceId); Attr.Dynamic "d" path; Attr.Dynamic "data-last-value" lastValue; svgAttr "fill" "none"; svgAttr "stroke" traceColor; svgAttr "stroke-width" (fixedText trace.Width); svgAttr "stroke-linejoin" "round"; svgAttr "stroke-linecap" "round" ] []
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
                    svgAttr "stroke" "#1f4f73"
                    svgAttr "stroke-width" "1"
                    svgAttr "stroke-dasharray" "3 3"
                    svgAttr "pointer-events" "none"
                ] []

            yield
                svgElement "g" [
                    Attr.Create "data-testid" ("ta-row-cursor-label-" + rowId)
                    Attr.Create "data-ta-row-cursor-label" "true"
                    Attr.Create "data-ta-row-cursor-row-id" rowId
                    svgAttr "visibility" "hidden"
                    svgAttr "pointer-events" "none"
                ] [
                    svgElement "rect" [
                        svgAttr "x" "-46"
                        svgAttr "y" "2"
                        svgAttr "width" "92"
                        svgAttr "height" "27"
                        svgAttr "rx" "2"
                        svgAttr "fill" "#ffffff"
                        svgAttr "fill-opacity" "0.92"
                        svgAttr "stroke" "#8ca0b8"
                        svgAttr "stroke-width" "0.8"
                    ] []
                    svgElement "text" [
                        Attr.Create "data-testid" ("ta-row-cursor-date-" + rowId)
                        Attr.Create "data-ta-row-cursor-date" "true"
                        svgAttr "x" "0"
                        svgAttr "y" "12"
                        svgAttr "text-anchor" "middle"
                        svgAttr "font-family" "Consolas,monospace"
                        svgAttr "font-size" "9"
                        svgAttr "fill" "#263b55"
                    ] [ text "Unavailable" ]
                    svgElement "text" [
                        Attr.Create "data-testid" ("ta-row-cursor-time-" + rowId)
                        Attr.Create "data-ta-row-cursor-clock" "true"
                        svgAttr "x" "0"
                        svgAttr "y" "23"
                        svgAttr "text-anchor" "middle"
                        svgAttr "font-family" "Consolas,monospace"
                        svgAttr "font-size" "9"
                        svgAttr "fill" "#263b55"
                    ] [ text "Unavailable" ]
                ]
        ],
        referenceTimestamps,
        (traces
         |> Array.mapi (fun index _ ->
             fun cursorIndex ->
                 let currentReader, _ = readerStates[index].Value
                 currentReader cursorIndex)),
        (traces
         |> Array.mapi (fun index _ ->
             fun cursorIndex ->
                 let _, currentReader = readerStates[index].Value
                 currentReader cursorIndex))

    let compositeSvgReactivePreparedLiveWithValueRefresh rowId isBaseRow (traces: TaTraceSpec array) preparedData dataView referenceTimestamps cursorIndex setCursorIndex commitCursorIndex scheduleValueRefresh =
        let hasCandles = traces |> Array.exists (fun trace -> trace.Kind = TaTraceKind.Candlestick)
        let height = Var.Create(if hasCandles then 250 else 112)
        compositeSvgReactivePreparedLiveWithHeight rowId isBaseRow traces preparedData dataView referenceTimestamps cursorIndex setCursorIndex commitCursorIndex height.View scheduleValueRefresh

    let compositeSvgReactivePreparedLive rowId isBaseRow traces preparedData dataView referenceTimestamps cursorIndex setCursorIndex commitCursorIndex =
        compositeSvgReactivePreparedLiveWithValueRefresh rowId isBaseRow traces preparedData dataView referenceTimestamps cursorIndex setCursorIndex commitCursorIndex ignore

    let compositeSvgReactivePrepared rowId isBaseRow traces data referenceTimestamps cursorIndex setCursorIndex commitCursorIndex =
        let preparedData = RendererModel.prepareData data
        let dataState = Var.Create preparedData
        compositeSvgReactivePreparedLive rowId isBaseRow traces preparedData dataState.View referenceTimestamps cursorIndex setCursorIndex commitCursorIndex

    let compositeSvgReactive rowId traces data referenceTimestamps cursorIndex setCursorIndex commitCursorIndex =
        let chart, timestamps, _, _ = compositeSvgReactivePrepared rowId false traces data referenceTimestamps cursorIndex setCursorIndex commitCursorIndex
        chart, timestamps

    let compositeSvg rowId traces data referenceTimestamps cursorIndex setCursorIndex commitCursorIndex =
        let cursor = Var.Create cursorIndex
        compositeSvgReactive rowId traces data referenceTimestamps cursor.View setCursorIndex commitCursorIndex

    let renderRowReactivePreparedLiveWithHeight (state: RuntimeState) (ui: TaRendererUiState) preparedData (dataView: View<TaPreparedRendererData>) visibleTimestamps cursorIndex setCursorIndex commitCursorIndex showSharedTimeAxis isBaseRow (rowHeight: Var<int>) scheduleValueRefresh (row: TaRowSpec) =
        let traces = RendererModel.effectiveTraces row |> Array.filter _.Visible
        let chart, timestamps, cursorReaders, legendReaders = compositeSvgReactivePreparedLiveWithHeight row.RowId isBaseRow traces preparedData dataView visibleTimestamps cursorIndex setCursorIndex commitCursorIndex rowHeight.View scheduleValueRefresh
        let title = rowTitle row traces
        let heightBounds = RendererModel.rowHeightBounds row traces
        let children =
            if showSharedTimeAxis then [ chart; timeAxis ("ta-time-axis-" + row.RowId) row.RowId timestamps ]
            else [ chart ]
        let metadata =
            dataView
            |> View.Map (fun currentData ->
                span [ attr.style "display:inline-flex; align-items:center; gap:6px 10px; flex:0 0 auto; flex-wrap:nowrap; white-space:nowrap;" ] [
                    for value in RendererModel.rowTemporalMetadataPrepared row currentData do
                        let availability = value.AvailableAtUtc |> Option.map compactTimestamp |> Option.defaultValue "unknown"
                        let quality = value.Quality |> Option.defaultValue "unknown"
                        yield
                            span [
                                Attr.Create "data-testid" ("ta-row-meta-" + row.RowId + "-" + value.ScaleKey)
                                Attr.Create "data-scale-key" value.ScaleKey
                                Attr.Create "data-finality" value.Finality
                                Attr.Create "data-quality" quality
                                attr.title (RendererModel.temporalDetail value)
                                attr.style "display:inline-flex; align-items:center; min-height:20px; padding:1px 6px; border:1px solid #bcc9d8; border-radius:4px; background:#f7fafc; color:#465b74; font-family:Consolas,monospace; font-size:10px; white-space:nowrap;"
                            ] [ text (value.ScaleKey + " | " + value.Finality + " | " + quality + " | frontier " + compactTimestamp value.ObservedThroughUtc + " | available " + availability) ]
                ] :> Doc)
            |> Doc.EmbedView
        let legend =
            div [
                Attr.Create "data-testid" ("ta-row-values-" + row.RowId)
                Attr.Create "data-ta-row-values" "true"
                Attr.Create "data-fixed-height" "30"
                attr.style "box-sizing:border-box; display:flex; align-items:center; gap:6px 14px; height:30px; min-height:30px; padding:0 8px; border-top:1px solid #edf1f6; border-bottom:1px solid #edf1f6; overflow-x:auto; overflow-y:hidden; white-space:nowrap; font-family:Consolas,monospace; font-size:11px; line-height:16px; color:#263b55;"
            ] [
                let initialPresentation =
                    if timestamps.Length = 0 then None
                    else legendReaders |> Array.tryPick (fun readValue -> readValue (timestamps.Length - 1))
                let initialTimestamp =
                    initialPresentation
                    |> Option.bind (fun value -> RendererModel.fullTimestamp value.Timestamp)
                    |> Option.defaultValue "Unavailable"
                yield
                    span [
                        Attr.Create "data-testid" ("ta-row-data-time-" + row.RowId)
                        Attr.Create "data-ta-row-data-time" "true"
                        Attr.Create "data-ta-row-data-time-row-id" row.RowId
                        attr.style "display:inline-block; width:19ch; min-width:19ch; max-width:19ch; overflow:hidden; white-space:nowrap; font-variant-numeric:tabular-nums; font-weight:650;"
                    ] [ text initialTimestamp ]
                for index in 0 .. traces.Length - 1 do
                    let trace = traces[index]
                    if trace.Kind <> TaTraceKind.Marker then
                        let label = if String.IsNullOrWhiteSpace trace.Label then trace.TraceId else trace.Label
                        let initialValue =
                            if timestamps.Length = 0 then "Unavailable"
                            else legendReaders[index] (timestamps.Length - 1) |> Option.map _.Value |> Option.defaultValue "Unavailable"
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
        let frameHeight = rowHeight.View |> View.Map (fun value -> value + 58 + if showSharedTimeAxis then 16 else 0)
        div [ Attr.Create "data-testid" ("ta-row-shell-" + row.RowId); attr.style "display:flex; flex-direction:column; min-width:0;" ] [
            chartFrame title [ metadata ] legend ("ta-row-" + row.RowId) frameHeight children
            rowResizeHandle row.RowId heightBounds rowHeight
        ], cursorReaders, legendReaders

    let renderRowReactivePreparedLiveWithValueRefresh (state: RuntimeState) (ui: TaRendererUiState) preparedData (dataView: View<TaPreparedRendererData>) visibleTimestamps cursorIndex setCursorIndex commitCursorIndex showSharedTimeAxis isBaseRow scheduleValueRefresh (row: TaRowSpec) =
        let traces = RendererModel.effectiveTraces row |> Array.filter _.Visible
        let height = Var.Create((RendererModel.rowHeightBounds row traces).DefaultHeight)
        renderRowReactivePreparedLiveWithHeight state ui preparedData dataView visibleTimestamps cursorIndex setCursorIndex commitCursorIndex showSharedTimeAxis isBaseRow height scheduleValueRefresh row

    let renderRowReactivePreparedLive state ui preparedData dataView visibleTimestamps cursorIndex setCursorIndex commitCursorIndex showSharedTimeAxis isBaseRow row =
        renderRowReactivePreparedLiveWithValueRefresh state ui preparedData dataView visibleTimestamps cursorIndex setCursorIndex commitCursorIndex showSharedTimeAxis isBaseRow ignore row

    let renderRowReactivePrepared (state: RuntimeState) (ui: TaRendererUiState) visibleTimestamps cursorIndex setCursorIndex commitCursorIndex showSharedTimeAxis isBaseRow (row: TaRowSpec) =
        let dataState = Var.Create(RendererModel.prepareData state.Data)
        renderRowReactivePreparedLive state ui dataState.Value dataState.View visibleTimestamps cursorIndex setCursorIndex commitCursorIndex showSharedTimeAxis isBaseRow row

    let renderRowReactive (state: RuntimeState) (ui: TaRendererUiState) visibleTimestamps cursorIndex setCursorIndex commitCursorIndex showSharedTimeAxis (row: TaRowSpec) =
        renderRowReactivePrepared state ui visibleTimestamps cursorIndex setCursorIndex commitCursorIndex showSharedTimeAxis false row
        |> fun (rowDoc, _, _) -> rowDoc

    let renderRow state ui visibleTimestamps setCursorIndex commitCursorIndex showSharedTimeAxis row =
        let cursor = Var.Create ui.CursorIndex
        renderRowReactive state ui visibleTimestamps cursor.View setCursorIndex commitCursorIndex showSharedTimeAxis row

    let render (options: TaRendererOptions) (callbacks: TaRendererCallbacks) (runtimeState: Var<RuntimeState>) =
        ensureAxisResizeTracking ()
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
        let mutable editingRowId: string option = None
        let mutable pendingEditorMutation: (int64 * string option * Set<string> * TaRowEditorBinding) option = None
        let mutable navigatorElement: Element = null
        let mutable finishNavigatorDrag: (unit -> unit) option = None
        let mutable chartRenderSequence = 0
        let cursorIndex = Var.Create<int option> None
        let mutable chartStackElement: Element = null
        let mutable latestCursorTimestamps: string array = [||]
        let mutable latestCursorReaders: (int -> TaCursorValue option) array = [||]
        let mutable latestLegendReaders: Map<string, (int -> TaRowValuePresentation option) array> = Map.empty
        let mutable displayedCursorIndex: int option = None
        let mutable refreshVisibleValues: (unit -> unit) = ignore
        let mutable visibleValueRefreshScheduled = false
        let scheduleVisibleValueRefresh () =
            if not visibleValueRefreshScheduled then
                visibleValueRefreshScheduled <- true
                JS.RequestAnimationFrame(fun _ ->
                    visibleValueRefreshScheduled <- false
                    refreshVisibleValues ())
                |> ignore
        let mutable chartWorkGeneration = 0
        let mutable dataWorkGeneration = 0
        let mutable activeRowDataStates: Var<TaPreparedRendererData> array = [||]
        let scheduleNextFrame work =
            JS.RequestAnimationFrame(fun _ -> work ()) |> ignore
        let scheduleRowDataRefresh prepared =
            dataWorkGeneration <- dataWorkGeneration + 1
            let generation = dataWorkGeneration
            let targets = activeRowDataStates
            let rec update index =
                if generation = dataWorkGeneration && index < targets.Length then
                    scheduleNextFrame (fun () ->
                        if generation = dataWorkGeneration then
                            targets[index].Value <- prepared
                            update (index + 1))
            update 0
        let mutable pendingCursorIndex: int option option = None
        let mutable cursorFrameScheduled = false
        let crossScaleSummaryOpen = Var.Create false
        let uiState =
            Var.Create
                { Window = { StartIndex = 0; Count = options.DefaultVisibleBars }
                  FollowLatest = true
                  HiddenRows = Set.empty
                  AddRowOpen = false
                  CursorIndex = None
                  PendingActionId = None
                  Feedback = "" }
        let sameChartUiState (left: TaRendererUiState) (right: TaRendererUiState) =
            left.Window = right.Window
            && left.FollowLatest = right.FollowLatest
            && left.HiddenRows = right.HiddenRows
        let chartUiState = Var.Create uiState.Value
        let setUiState next =
            let previousChartState = chartUiState.Value
            uiState.Value <- next
            if not (sameChartUiState previousChartState next) then chartUiState.Value <- next
        let mutable actionSequence = 0
        let mutable querySelectionGeneration = 0
        let mutable queryInFlight = false
        let mutable queuedQuery: (TaQueryChange * int) option = None
        let mutable boundaryPanGeneration = 0
        let mutable pendingBoundaryPan: (int * TaCoverageDirection * int * string array * TaVisibleWindow) option = None
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
                (fun state ui ->
                    visibleRangeActionAllowed state
                    && (localViewportDisabled state.Poll || ui.PendingActionId.IsSome))
                runtimeState.View
                uiState.View
        let viewportCommandsDisabledNow () =
            visibleRangeActionAllowed runtimeState.Value
            && (localViewportDisabled runtimeState.Value.Poll || uiState.Value.PendingActionId.IsSome)
        let startActionWithFeedback action successText onAccepted onRejected afterSettled =
            actionSequence <- actionSequence + 1
            let request =
                { RequestId = canvasIdText (currentCanvasId ()) + ":ui:" + string actionSequence
                  ExpectedDocumentRevision = Some runtimeState.Value.DocumentRevision
                  Action = action }
            submit callbacks uiState runtimeState.Value.DocumentRevision request successText onAccepted onRejected afterSettled
        let startActionWith action successText onAccepted onRejected =
            startActionWithFeedback action successText (fun () -> onAccepted (); None) onRejected ignore
        let startAction action successText onAccepted =
            startActionWith action successText onAccepted ignore
        let chartRuntimeState = Var.Create runtimeState.Value
        let initialPreparedData =
            { RawData = Map.empty
              ResolvedAxes = Map.empty
              ResolvedSeries = Map.empty }
        let mutable latestPreparedData = initialPreparedData
        let mutable preparedDataForShell = initialPreparedData
        let mutable preparedDataReady = false
        let mutable preparationGeneration = 0
        let mutable observedChartTopology = chartTopologySignaturePrepared runtimeState.Value initialPreparedData
        let mutable observedDataState = runtimeState.Value

        let scheduleFullPreparation () =
            preparationGeneration <- preparationGeneration + 1
            let generation = preparationGeneration
            preparedDataReady <- false
            chartRuntimeState.Value <- runtimeState.Value
            let data = runtimeState.Value.Data
            RendererModel.prepareDataScheduled
                scheduleNextFrame
                data
                (fun prepared ->
                    if generation = preparationGeneration then
                        let current = runtimeState.Value
                        latestPreparedData <- prepared
                        preparedDataForShell <- prepared
                        observedChartTopology <- chartTopologySignaturePrepared current prepared
                        observedDataState <- current
                        preparedDataReady <- true
                        chartRuntimeState.Value <- current)

        runtimeState.View
        |> View.Sink (fun next ->
            let dataChanged = runtimeDataChanged observedDataState next
            if next.Identity <> observedDataState.Identity then
                pendingBoundaryPan <- None
                observedDataState <- next
                scheduleFullPreparation ()
            elif not preparedDataReady then
                if dataChanged then
                    observedDataState <- next
                    scheduleFullPreparation ()
                else
                    observedDataState <- next
            else
                let nextPreparedData =
                    if dataChanged then RendererModel.prepareDataIncremental latestPreparedData next.Data
                    else latestPreparedData
                let nextChartTopology = chartTopologySignaturePrepared next nextPreparedData
                let topologyChanged =
                    next.Identity <> chartRuntimeState.Value.Identity
                    || next.DocumentRevision <> chartRuntimeState.Value.DocumentRevision
                    || nextChartTopology <> observedChartTopology
                if topologyChanged then
                    observedChartTopology <- nextChartTopology
                    preparedDataForShell <- nextPreparedData
                    match next.Document with
                    | Some document ->
                        let nextTimeline = RendererModel.referenceTimelineForDocumentPrepared document nextPreparedData
                        let currentUi = uiState.Value
                        let generalOldTimeline = RendererModel.referenceTimelineForDocumentPrepared document latestPreparedData
                        let generalOldWindow =
                            RendererModel.resolveWindow
                                options.MinimumVisibleBars
                                options.MaximumVisibleBars
                                generalOldTimeline.Length
                                currentUi.FollowLatest
                                currentUi.Window
                        let reanchored =
                            match pendingBoundaryPan with
                            | Some(_, direction, delta, intentTimeline, intentWindow)
                                when RendererModel.coverageExtended direction intentTimeline nextTimeline ->
                                pendingBoundaryPan <- None
                                RendererModel.tryReanchorWindow
                                    options.MinimumVisibleBars
                                    options.MaximumVisibleBars
                                    delta
                                    intentTimeline
                                    nextTimeline
                                    intentWindow
                            | _ when not currentUi.FollowLatest
                                     && generalOldTimeline.Length > 0
                                     && nextTimeline.Length > generalOldTimeline.Length ->
                                RendererModel.tryReanchorWindow
                                    options.MinimumVisibleBars
                                    options.MaximumVisibleBars
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
                        | None -> ()
                    | None -> pendingBoundaryPan <- None
                    chartRuntimeState.Value <- next
                elif dataChanged then
                    scheduleRowDataRefresh nextPreparedData
                latestPreparedData <- nextPreparedData
                observedDataState <- next)
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

        let resolvedWindow ui =
            RendererModel.resolveWindow
                options.MinimumVisibleBars
                options.MaximumVisibleBars
                (referenceLength ())
                ui.FollowLatest
                ui.Window

        let commitLocalWindow followLatest window =
            let current = uiState.Value
            let total = referenceLength ()
            let bounded = RendererModel.resolveWindow options.MinimumVisibleBars options.MaximumVisibleBars total followLatest window
            let changed = bounded <> resolvedWindow current || followLatest <> current.FollowLatest
            setUiState
                { current with
                    Window = bounded
                    FollowLatest = followLatest
                    CursorIndex = None }
            cursorIndex.Value <- None
            draftWindow.Value <- None
            changed, bounded

        let setWindow followLatest window =
            if not (viewportCommandsDisabledNow ()) then
                let changed, bounded = commitLocalWindow followLatest window
                if changed && actionAllowed "visible-range-changed" && not (commandsDisabledNow ()) then
                    match runtimeState.Value.Document with
                    | Some document ->
                        match RendererModel.visibleEventRangePrepared document latestPreparedData bounded with
                        | Some range ->
                            startAction
                                (SduiAction.VisibleRangeChanged(
                                    currentCanvasId (),
                                    { BaseRowId = range.BaseRowId
                                      StartEventTimeUtc = range.StartEventTimeUtc
                                      EndEventTimeExclusiveUtc = range.EndEventTimeExclusiveUtc
                                      MaximumBasePoints = min DynamicRuntimeDefaults.MaximumVisibleRangeBasePoints (max 1 options.MaximumVisibleBars) }))
                                "Visible range synchronized."
                                ignore
                        | None -> ()
                    | None -> ()

        let requestAdjacentCoverage direction delta =
            if actionAllowed "visible-range-changed" && not (commandsDisabledNow ()) then
                match runtimeState.Value.Document with
                | Some document ->
                    match
                        RendererModel.tryAdjacentCoverageRange
                            direction
                            (min DynamicRuntimeDefaults.MaximumVisibleRangeBasePoints (max 1 options.MaximumVisibleBars))
                            document
                            latestPreparedData
                    with
                    | Some change ->
                        let timeline = RendererModel.referenceTimelineForDocumentPrepared document latestPreparedData
                        let window =
                            RendererModel.resolveWindow
                                options.MinimumVisibleBars
                                options.MaximumVisibleBars
                                timeline.Length
                                uiState.Value.FollowLatest
                                uiState.Value.Window
                        boundaryPanGeneration <- boundaryPanGeneration + 1
                        pendingBoundaryPan <- Some(boundaryPanGeneration, direction, delta, timeline, window)
                        startActionWith
                            (SduiAction.VisibleRangeChanged(currentCanvasId (), change))
                            (if direction = TaCoverageDirection.Earlier then "Earlier coverage requested." else "Later coverage requested.")
                            ignore
                            (fun () -> pendingBoundaryPan <- None)
                    | None ->
                        setUiState
                            { uiState.Value with
                                Feedback =
                                    if direction = TaCoverageDirection.Earlier then
                                        "Earlier coverage is outside the configured query boundary."
                                    else
                                        "Later coverage is outside the configured query boundary." }
                | None -> ()

        let panWindow delta =
            let current = uiState.Value
            let total = referenceLength ()
            let visible = resolvedWindow current
            let requestedStart = visible.StartIndex + delta
            let maximumStart = RendererModel.viewportMaximumStart total visible
            if requestedStart < 0 then
                requestAdjacentCoverage TaCoverageDirection.Earlier delta
            elif requestedStart > maximumStart then
                requestAdjacentCoverage TaCoverageDirection.Later delta
            else
                let candidate =
                    RendererModel.clampWindow
                        options.MinimumVisibleBars
                        options.MaximumVisibleBars
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

        let startNavigatorDrag drag (event: MouseEvent) =
            if not (viewportCommandsDisabledNow ()) && not (isNull navigatorElement) then
                event.PreventDefault()
                event.StopPropagation()
                let bounds = navigatorElement.GetBoundingClientRect()
                let total = referenceLength ()
                let committed = resolvedWindow uiState.Value
                let startClientX = event.ClientX
                let mutable moveHandler: Action<Event> = null
                let mutable upHandler: Action<Event> = null
                let mutable finished = false

                let cleanup () =
                    if not (isNull moveHandler) then JS.Document.RemoveEventListener("mousemove", moveHandler)
                    if not (isNull upHandler) then JS.Document.RemoveEventListener("mouseup", upHandler)

                let finish () =
                    if not finished then
                        finished <- true
                        finishNavigatorDrag <- None
                        let draft = defaultArg draftWindow.Value committed
                        let followLatest, next =
                            RendererModel.commitWindowBounds options.MinimumVisibleBars options.MaximumVisibleBars total draft
                        if next <> committed || followLatest <> uiState.Value.FollowLatest then setWindow followLatest next
                        else draftWindow.Value <- None
                        cleanup ()

                moveHandler <-
                    Action<Event>(fun rawEvent ->
                        let mouse = rawEvent :?> MouseEvent
                        let delta =
                            if bounds.Width <= 0.0 || total <= 0 then 0
                            else int (Math.Round(float (mouse.ClientX - startClientX) / bounds.Width * float total))
                        draftWindow.Value <-
                            Some(RendererModel.previewWindowBounds options.MinimumVisibleBars options.MaximumVisibleBars total committed drag delta))

                upHandler <-
                    Action<Event>(fun _ -> finish ())

                finishNavigatorDrag <- Some finish
                JS.Document.AddEventListener("mousemove", moveHandler)
                JS.Document.AddEventListener("mouseup", upHandler)

        let finishNavigatorDragFromElement (event: MouseEvent) =
            event.PreventDefault()
            finishNavigatorDrag |> Option.iter (fun finish -> finish ())

        let cursorElements selector =
            if isNull chartStackElement then [||]
            else
                let nodes = chartStackElement.QuerySelectorAll(selector)
                [| for index in 0 .. int nodes.Length - 1 do
                       yield nodes.Item(index) |> As<Element> |]

        let setElementHidden hidden (element: Element) =
            if hidden then element.SetAttribute("hidden", "hidden")
            else element.RemoveAttribute("hidden")

        let applyVisibleCursorValues bounded =
            if not (isNull chartStackElement) then
                let hint = cursorElements "[data-ta-cursor-hint]" |> Array.tryHead
                let time = cursorElements "[data-ta-cursor-time]" |> Array.tryHead
                let valueNodes = cursorElements "[data-ta-cursor-value-index]"
                let legendValueNodes = cursorElements "[data-ta-row-value-index]"
                let rowTimeNodes = cursorElements "[data-ta-row-data-time='true']"
                let legendIndex =
                    match bounded with
                    | Some index -> Some index
                    | None when latestCursorTimestamps.Length > 0 -> Some(latestCursorTimestamps.Length - 1)
                    | None -> None
                for valueIndex in 0 .. legendValueNodes.Length - 1 do
                    let node = legendValueNodes[valueIndex]
                    let traceIndex =
                        match Int32.TryParse(node.GetAttribute("data-ta-row-value-index")) with
                        | true, parsed -> parsed
                        | _ -> valueIndex
                    let rowId = node.GetAttribute("data-ta-row-value-row-id")
                    let nextValue =
                        legendIndex
                        |> Option.bind (tryLegendValue latestLegendReaders rowId traceIndex)
                        |> Option.map _.Value
                        |> Option.defaultValue "Unavailable"
                    node.TextContent <- nextValue
                    node.SetAttribute("data-value-state", if nextValue = "Unavailable" then "undefined" else "defined")
                for node in rowTimeNodes do
                    let rowId = node.GetAttribute("data-ta-row-data-time-row-id")
                    let nextTime =
                        legendIndex
                        |> Option.bind (tryRowPresentation latestLegendReaders rowId)
                        |> Option.bind (fun value -> RendererModel.fullTimestamp value.Timestamp)
                        |> Option.defaultValue "Unavailable"
                    node.TextContent <- nextTime
                match bounded with
                | None ->
                    hint |> Option.iter (setElementHidden false)
                    time |> Option.iter (setElementHidden true)
                    for node in valueNodes do setElementHidden true node
                | Some index ->
                    hint |> Option.iter (setElementHidden true)
                    time
                    |> Option.iter (fun node ->
                        node.TextContent <- compactTimestamp latestCursorTimestamps[index]
                        setElementHidden false node)
                    for valueIndex in 0 .. valueNodes.Length - 1 do
                        let node = valueNodes[valueIndex]
                        match latestCursorReaders |> Array.tryItem valueIndex |> Option.bind (fun readCursor -> readCursor index) with
                        | Some current ->
                            node.TextContent <- current.Label + " " + current.Value
                            node.SetAttribute("data-cursor-row", current.Label)
                            setElementHidden false node
                        | None ->
                            node.TextContent <- ""
                            node.RemoveAttribute("data-cursor-row")
                            setElementHidden true node

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
                    let labelX = max 48.0 (min 952.0 x) |> fixedText
                    for group in rowCursorLabels do
                        let rowId = group.GetAttribute("data-ta-row-cursor-row-id")
                        let presentation = bounded |> Option.bind (tryRowPresentation latestLegendReaders rowId)
                        let dateText, timeText =
                            presentation
                            |> Option.bind (fun value -> RendererModel.timestampParts value.Timestamp)
                            |> Option.defaultValue ("Unavailable", "Unavailable")
                        group.SetAttribute("transform", "translate(" + labelX + " 0)")
                        group.SetAttribute("visibility", "visible")
                        let dateNode = group.QuerySelector("[data-ta-row-cursor-date='true']")
                        let timeNode = group.QuerySelector("[data-ta-row-cursor-clock='true']")
                        if not (isNull dateNode) then dateNode.TextContent <- dateText
                        if not (isNull timeNode) then timeNode.TextContent <- timeText
                | None ->
                    for line in crosshairs do line.SetAttribute("visibility", "hidden")
                    for group in rowCursorLabels do group.SetAttribute("visibility", "hidden")

                applyVisibleCursorValues bounded

        let flushCursorFrame () =
            cursorFrameScheduled <- false
            match pendingCursorIndex with
            | Some value ->
                pendingCursorIndex <- None
                applyCursorIndex value
            | None -> ()

        let setCursorIndex value =
            pendingCursorIndex <- Some value
            if not cursorFrameScheduled then
                cursorFrameScheduled <- true
                JS.RequestAnimationFrame(fun _ -> flushCursorFrame ()) |> ignore

        let commitCursorIndex index =
            setCursorIndex (Some index)
            if cursorIndex.Value <> Some index then cursorIndex.Value <- Some index
            if actionAllowed "shared-cursor-changed" && not (commandsDisabledNow ()) then
                match runtimeState.Value.Document with
                | Some document ->
                    let timeline = RendererModel.referenceTimelineForDocument document runtimeState.Value.Data
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

        let completeEditorMutation (document: TaWorkspaceDocument) documentRevision =
            let bindingOf row =
                TaRowEditorBinding.tryFind row
                |> Result.toOption
                |> Option.flatten

            match pendingEditorMutation with
            | Some(baseRevision, targetRowId, priorRowIds, expectedBinding) when documentRevision > baseRevision ->
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

                if matched then
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
                            runtimeState.Value.DocumentRevision,
                            editingRowId,
                            currentRows |> Array.map _.RowId |> Set.ofArray,
                            binding)
                    startActionWith
                        (SduiAction.ApplyTemplate(currentCanvasId (), editingRowId, schema.TemplateKey, editorValues.Value))
                        (schema.DisplayName + " accepted; awaiting authoritative document.")
                        ignore
                        (fun () -> pendingEditorMutation <- None)

        div [
            attr.``class`` "ptcs-ta-workspace"
            Attr.Create "data-testid" "ta-workspace"
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
                    if state.DocumentRevision <> synchronizedDocumentRevision then
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
                        completeEditorMutation document state.DocumentRevision
                        synchronizedDocumentRevision <- state.DocumentRevision

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
                                runtimeState.View
                                |> View.Map (fun current ->
                                    let status = RendererModel.statusPresentation document.StatusRef current
                                    div [ attr.style "display:flex; align-items:center; gap:5px; flex-wrap:wrap; justify-content:flex-end;" ] [
                                        div [ Attr.Create "data-testid" "ta-freshness"; Attr.Create "data-freshness" (freshnessClass status.Freshness); attr.style "border:1px solid #9fb0c6; border-radius:4px; padding:3px 7px; font-size:11px; font-weight:650; color:#27415f; background:#f8fafc;" ] [ text status.Label ]
                                        div [ Attr.Create "data-testid" "ta-poll-state"; Attr.Create "data-poll-state" (pollText current.Poll); attr.style "border:1px solid #c3cfdd; border-radius:4px; padding:3px 7px; font-size:10px; color:#53667d; background:#fff;" ] [ text (pollText current.Poll) ]
                                    ] :> Doc)
                                |> Doc.EmbedView
                            ]
                            runtimeState.View
                            |> View.Map (fun current ->
                                let status = RendererModel.statusPresentation document.StatusRef current
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
                                    (runtimeState.View |> View.Map (fun state -> remoteDisabled state.Poll))
                                    (fun () -> remoteDisabled runtimeState.Value.Poll)
                                    applyQuery
                            ]
                            div [ Attr.Create "data-testid" "ta-local-toolbar"; attr.style "display:flex; align-items:center; gap:5px; flex-wrap:wrap;" ]
                                ([ compactRemoteButton "ta-pan-left" "←" "Pan earlier" viewportCommandsDisabledView viewportCommandsDisabledNow (fun () ->
                                       let visible = resolvedWindow uiState.Value
                                       panWindow (-max 1 (visible.Count / 4)))
                                   compactRemoteButton "ta-pan-right" "→" "Pan later" viewportCommandsDisabledView viewportCommandsDisabledNow (fun () ->
                                       let visible = resolvedWindow uiState.Value
                                       panWindow (max 1 (visible.Count / 4)))
                                   compactRemoteButton "ta-zoom-in" "+" "Show fewer bars" viewportCommandsDisabledView viewportCommandsDisabledNow (fun () -> zoomWindow -8)
                                   compactRemoteButton "ta-zoom-out" "−" "Show more bars" viewportCommandsDisabledView viewportCommandsDisabledNow (fun () -> zoomWindow 8)
                                   compactRemoteButton "ta-reset-view" "Reset View" "Reset local viewport to the latest bars" viewportCommandsDisabledView viewportCommandsDisabledNow resetWindow
                                   compactRemoteButton "ta-reset-canvas" "Reset Canvas" "Request server canvas reset" commandsDisabledView commandsDisabledNow (fun () -> startAction (SduiAction.ResetCanvas(currentCanvasId ())) "Canvas reset accepted." ignore) ]
                                 @ (if (editorSchemasNow ()).Length > 0 then
                                        [ compactButton "ta-add-row-toggle" "Add Row" "Open row request editor" (fun () ->
                                              if uiState.Value.AddRowOpen then closeRowEditor ()
                                              else openNewRowEditor ()) ]
                                    else
                                        [])
                                  @ [ span [ attr.style "margin-left:auto; color:#60738b; font-size:11px;" ] [ text "viewport changes request the selected event-time range when enabled" ] ])
                            uiState.View
                            |> View.Map (fun ui ->
                                div [ Attr.Create "data-testid" "ta-row-toggles"; attr.style "display:flex; align-items:center; gap:5px; flex-wrap:wrap;" ] [
                                    for row in document.Rows do
                                        let hidden = Set.contains row.RowId ui.HiddenRows
                                        let displayLabel = rowDisplayLabel row
                                        let editable =
                                            match TaRowEditorBinding.tryResolve (editorSchemasNow ()) row with
                                            | Ok(Some _) -> true
                                            | _ -> false
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

                                                        setUiState { uiState.Value with HiddenRows = nextHidden })
                                                ] [ text displayLabel ]
                                                if editable then
                                                    button [
                                                        attr.``type`` "button"
                                                        Attr.Create "data-testid" ("ta-edit-row-" + row.RowId)
                                                        attr.title ("Edit " + displayLabel + " parameters")
                                                        attr.disabledBool commandsDisabledView
                                                        Attr.Dynamic "style" (commandsDisabledView |> View.Map (fun disabled ->
                                                            if disabled then "height:26px; border:1px solid #c8d2df; border-right:0; background:#edf1f5; color:#8b98a8; padding:2px 7px; font-size:11px; cursor:not-allowed;"
                                                            else "height:26px; border:1px solid #9cb3cc; border-right:0; background:#fff; color:#315d88; padding:2px 7px; font-size:11px; cursor:pointer;"))
                                                        on.click (fun _ _ ->
                                                            if not (commandsDisabledNow ()) then openRowEditor row)
                                                    ] [ text "Edit" ]
                                                button [
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
                        View.Map2 (fun (state: RuntimeState) ui ->
                            chartRenderSequence <- chartRenderSequence + 1
                            let renderSequence = chartRenderSequence
                            chartWorkGeneration <- chartWorkGeneration + 1
                            dataWorkGeneration <- dataWorkGeneration + 1
                            let workGeneration = chartWorkGeneration
                            let visibleRows =
                                if preparedDataReady then
                                    document.Rows
                                    |> Array.filter (fun row -> row.Visible && not (Set.contains row.RowId ui.HiddenRows))
                                else
                                    [||]
                            let cursorReaderCount =
                                visibleRows
                                |> Array.sumBy (fun row -> RendererModel.effectiveTraces row |> Array.filter _.Visible |> Array.length)
                            let shellPreparedData = preparedDataForShell
                            let referenceTimeline = RendererModel.referenceTimelineForDocumentPrepared document shellPreparedData
                            let referenceLength = referenceTimeline.Length
                            let overviewPoints =
                                visibleRows
                                |> Array.collect RendererModel.effectiveTraces
                                |> Array.tryFind (fun trace -> trace.Visible && trace.Kind = TaTraceKind.Candlestick)
                                |> Option.map (fun trace -> RendererModel.candleSeriesForTracePreparedSampled 280 trace shellPreparedData)
                                |> Option.defaultValue [||]
                            let overviewStripeVisuals =
                                visibleRows
                                |> Array.collect RendererModel.effectiveTraces
                                |> Array.filter (fun trace -> trace.Visible && trace.Kind = TaTraceKind.OverviewStripe)
                                |> Array.collect (fun trace -> RendererModel.overviewStripePlacementsPrepared trace shellPreparedData referenceTimeline)
                                |> RendererModel.overviewStripeVisuals
                            let visibleWindow =
                                RendererModel.resolveWindow
                                    options.MinimumVisibleBars
                                    options.MaximumVisibleBars
                                    referenceLength
                                    ui.FollowLatest
                                    ui.Window
                            let visibleTimestamps = RendererModel.selectWindow visibleWindow referenceTimeline
                            let rowDataStates = visibleRows |> Array.map (fun _ -> Var.Create shellPreparedData)
                            let rowHeights =
                                visibleRows
                                |> Array.map (fun row ->
                                    let traces = RendererModel.effectiveTraces row |> Array.filter _.Visible
                                    rowHeightStateFor row traces)
                            let readyRowCount = Var.Create 0
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
                            activeRowDataStates <- rowDataStates
                            latestCursorTimestamps <- visibleTimestamps
                            latestCursorReaders <- [||]
                            latestLegendReaders <- Map.empty

                            let synchronizeReaders () =
                                latestCursorReaders <- stagedCursorReaders |> Array.choose id |> Array.collect id
                                latestLegendReaders <-
                                    stagedLegendReaders
                                    |> Array.mapi (fun index readers -> readers |> Option.map (fun values -> visibleRows[index].RowId, values))
                                    |> Array.choose id
                                    |> Map.ofArray
                                applyCursorIndex cursorIndex.Value

                            let rec mountRow index =
                                if workGeneration = chartWorkGeneration && index < visibleRows.Length then
                                    scheduleNextFrame (fun () ->
                                        if workGeneration = chartWorkGeneration then
                                            let prepared = rowDataStates[index].Value
                                            let rowDoc, cursorReaders, legendReaders =
                                                renderRowReactivePreparedLiveWithHeight
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
                                                    scheduleVisibleValueRefresh
                                                    visibleRows[index]
                                            stagedCursorReaders[index] <- Some cursorReaders
                                            stagedLegendReaders[index] <- Some legendReaders
                                            rowDocs[index].Value <- rowDoc
                                            readyRowCount.Value <- index + 1
                                            synchronizeReaders ()
                                            mountRow (index + 1))
                            mountRow 0

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
                                Attr.Create "data-row-count" (string visibleRows.Length)
                                Attr.Dynamic "data-ready-row-count" (readyRowCount.View |> View.Map string)
                                Attr.Create "data-cursor-index" ""
                                attr.style "display:flex; flex-direction:column; min-width:0; padding:0 12px 14px;"
                                on.afterRender (fun node ->
                                    chartStackElement <- node
                                    latestCursorTimestamps <- visibleTimestamps
                                    applyCursorIndex cursorIndex.Value)
                            ] [
                                yield div [ Attr.Create "data-testid" "ta-cursor-panel"; attr.style "order:1; display:flex; flex-direction:column; align-items:stretch; border-top:1px solid #dce4ef; background:#f8fafc;" ] [
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
                                        let capped = min referenceLength options.MaximumVisibleBars
                                        let label = if referenceLength > options.MaximumVisibleBars then "Max " + string options.MaximumVisibleBars else "All"
                                        compactButton "ta-view-all" label ("Show up to " + string capped + " loaded bars") (fun () -> setWindowCount capped)
                                    ]
                                    div [ attr.style "grid-column:1 / -1; min-width:0;" ] [
                                        overviewSvg
                                            overviewPoints
                                            overviewStripeVisuals
                                            referenceLength
                                            (draftWindow.View
                                             |> View.Map (fun draft ->
                                             let selection = defaultArg draft visibleWindow
                                             RendererModel.selectionRatios referenceLength selection))
                                            (fun node -> navigatorElement <- node |> As<Element>)
                                            startNavigatorDrag
                                            finishNavigatorDragFromElement
                                    ]
                                ]
                            ] :> Doc) chartRuntimeView chartUiState.View
                        |> Doc.EmbedView
                    ] :> Doc)
            |> Doc.EmbedView
        ]
