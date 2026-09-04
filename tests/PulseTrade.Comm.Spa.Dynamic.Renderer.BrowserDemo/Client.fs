namespace PulseTrade.Comm.Spa.Dynamic.Renderer.BrowserDemo

open System
open PulseTrade.Comm.Spa.Dynamic.Contracts
open PulseTrade.Comm.Spa.Dynamic.Renderer
open WebSharper
open WebSharper.UI
open WebSharper.UI.Html
open WebSharper.UI.Client

[<JavaScript>]
module Client =
    let candle timestamp openValue closeValue volume =
        let high = max openValue closeValue + 4.0
        let low = min openValue closeValue - 3.0

        SduiValue.Object(
            Map [
                "t", SduiValue.Text timestamp
                "o", SduiValue.Number openValue
                "h", SduiValue.Number high
                "l", SduiValue.Number low
                "c", SduiValue.Number closeValue
                "v", SduiValue.Number volume
            ])

    let linePoint timestamp value =
        SduiValue.Object(Map [ "t", SduiValue.Text timestamp; "v", SduiValue.Number value ])

    let timestamp index =
        let day = 1 + index / 1440
        let hour = (index / 60) % 24
        let minute = index % 60
        let pad2 value = if value < 10 then "0" + string value else string value
        "2026-09-" + pad2 day + "T" + pad2 hour + ":" + pad2 minute + ":00.0000000+00:00"

    let temporalPoint sourceIntervalId scale startUtc endUtc observedThroughUtc availableAt finality projection quality payload =
        SduiValue.Object(
            Map [
                "_type", SduiValue.Text "temporal-point.v1"
                "sourceIntervalId", SduiValue.Text sourceIntervalId
                "scaleKey", SduiValue.Text scale
                "intervalStartUtc", SduiValue.Text startUtc
                "intervalEndUtc", SduiValue.Text endUtc
                "observedThroughUtc", SduiValue.Text observedThroughUtc
                "finality", SduiValue.Text finality
                "projection", SduiValue.Text projection
                "quality", SduiValue.Text quality
                "value", payload
                match availableAt with
                | Some value -> "availableAtUtc", SduiValue.Text value
                | None -> ()
            ])

    let candlePayload openValue closeValue volume =
        let high = max openValue closeValue + 6.0
        let low = min openValue closeValue - 5.0
        SduiValue.Object(
            Map [ "o", SduiValue.Number openValue
                  "h", SduiValue.Number high
                  "l", SduiValue.Number low
                  "c", SduiValue.Number closeValue
                  "v", SduiValue.Number volume ])

    let linePayload value = SduiValue.Object(Map [ "v", SduiValue.Number value ])

    let sampleSeries count =
        let candles =
            Array.init count (fun index ->
                let baseline = 21800.0 + float index * 1.7 + Math.Sin(float index / 4.0) * 24.0
                let closeValue = baseline + Math.Cos(float index / 3.0) * 9.0
                candle (timestamp index) baseline closeValue (900.0 + float ((index * 73) % 520)))

        let heikin =
            Array.init count (fun index ->
                let baseline = 21792.0 + float index * 1.65 + Math.Sin(float index / 5.0) * 18.0
                candle (timestamp index) baseline (baseline + Math.Cos(float index / 2.5) * 7.0) (700.0 + float ((index * 41) % 430)))

        let line offset amplitude phase =
            Array.init count (fun index ->
                linePoint (timestamp index) (offset + Math.Sin(float index / phase) * amplitude))

        let fiveMinuteCandles =
            [| for startIndex in 0 .. 5 .. count - 1 do
                   let endIndex = min count (startIndex + 5)
                   let openValue = 21800.0 + float startIndex * 1.7 + Math.Sin(float startIndex / 4.0) * 24.0
                   let closeIndex = endIndex - 1
                   let closeValue = 21800.0 + float closeIndex * 1.7 + Math.Sin(float closeIndex / 4.0) * 24.0 + Math.Cos(float closeIndex / 3.0) * 9.0
                   yield
                       temporalPoint
                           ("es-5k:" + string startIndex)
                           "5K"
                           (timestamp startIndex)
                           (timestamp (startIndex + 5))
                           (timestamp endIndex)
                           (Some(timestamp endIndex))
                           "final"
                           "candle-span"
                           "complete"
                           (candlePayload openValue closeValue (4500.0 + float ((startIndex * 37) % 900))) |]

        let fiveMinuteSma =
            [| for startIndex in 0 .. 5 .. count - 1 do
                   let endIndex = min count (startIndex + 5)
                   yield
                       temporalPoint
                           ("es-5k-sma:" + string startIndex)
                           "5K"
                           (timestamp startIndex)
                           (timestamp (startIndex + 5))
                           (timestamp endIndex)
                           (Some(timestamp endIndex))
                           "final"
                           "repeat-across-base-buckets"
                           "complete"
                           (linePayload (21815.0 + Math.Sin(float startIndex / 30.0) * 31.0)) |]

        let thirtyMinuteMacd =
            [| for startIndex in 0 .. 30 .. count - 1 do
                   let endIndex = startIndex + 30
                   let observedIndex = min count endIndex
                   let isFinal = endIndex <= count
                   yield
                       temporalPoint
                           ("es-30k-macd:" + string startIndex)
                           "30K"
                           (timestamp startIndex)
                           (timestamp endIndex)
                           (timestamp observedIndex)
                           (if isFinal then Some(timestamp endIndex) else None)
                           (if isFinal then "final" else "preview")
                           "step-after-close"
                           (if isFinal then "complete" else "partial")
                           (linePayload (Math.Sin(float startIndex / 90.0) * 22.0)) |]

        Map [
            "series.price", SduiValue.Array candles
            "series.price-5k", SduiValue.Array fiveMinuteCandles
            "series.volume", SduiValue.Array candles
            "series.sma", SduiValue.Array(line 21820.0 28.0 6.0)
            "series.sma-5k", SduiValue.Array fiveMinuteSma
            "series.dmi", SduiValue.Array(line 25.0 11.0 4.5)
            "series.adx", SduiValue.Array(line 22.0 8.0 7.0)
            "series.macd", SduiValue.Array(line 0.0 18.0 5.5)
            "series.macd-30k", SduiValue.Array thirtyMinuteMacd
            "series.heikin", SduiValue.Array heikin
            "ta.status",
            SduiValue.Object(
                Map [
                    "freshness", SduiValue.Text "live"
                    "label", SduiValue.Text "LIVE / revision 42"
                    "watermarkUtc", SduiValue.Text "2026-07-11T09:30:00Z"
                    "quality", SduiValue.Text "complete"
                ])
        ]

    let row rowId kind dataRef weight =
        { RowId = rowId
          Kind = kind
          DataRef = dataRef
          HeightWeight = weight
          Visible = true
          Options = Map.empty
          Traces = [||] }

    let trace traceId kind dataRef label color width =
        { TraceId = traceId
          Kind = kind
          DataRef = dataRef
          Label = label
          Color = color
          Width = width
          Visible = true
          Options = Map.empty }

    let compositeRow rowId kind dataRef weight traces =
        { row rowId kind dataRef weight with Traces = traces }

    let choice key label value =
        { Key = key
          Label = label
          Value = SduiValue.Text value }

    let field key label kind required defaultValue =
        { Key = key
          Label = label
          Kind = kind
          Required = required
          DefaultValue = defaultValue }

    let sampleEditorSchemas =
        [| { TemplateKey = "ta.sma"
             DisplayName = "SMA overlay"
             SchemaRevision = 1L
             Fields =
                [| field
                       "scales"
                       "Scales"
                       (EditorValueKind.List(EditorValueKind.Scale [| "1k"; "5k"; "30k" |], Some 1, Some 4))
                       true
                       (Some(SduiValue.Array [| SduiValue.Text "1k"; SduiValue.Text "5k" |]))
                   field
                       "periods"
                       "Periods"
                       (EditorValueKind.List(EditorValueKind.Integer(Some 1L, Some 500L), Some 1, Some 8))
                       true
                       (Some(SduiValue.Array [| SduiValue.Number 13.0; SduiValue.Number 21.0 |]))
                   field
                       "style"
                       "Style"
                       (EditorValueKind.Group
                           [| field
                                  "source"
                                  "Price source"
                                  (EditorValueKind.Choice
                                      [| choice "close" "Close" "close"
                                         choice "hl2" "High / low mean" "hl2" |])
                                  true
                                  (Some(SduiValue.Text "close"))
                              field "visible" "Visible" EditorValueKind.Boolean true (Some(SduiValue.Bool true)) |])
                       true
                       None |] }
           { TemplateKey = "ta.macd"
             DisplayName = "MACD panel"
             SchemaRevision = 1L
             Fields =
                [| field "scale" "Scale" (EditorValueKind.Scale [| "1k"; "5k"; "30k" |]) true (Some(SduiValue.Text "5k"))
                   field
                       "periods"
                       "Periods"
                       (EditorValueKind.Group
                           [| field "fast" "Fast" (EditorValueKind.Integer(Some 1L, Some 500L)) true (Some(SduiValue.Number 12.0))
                              field "slow" "Slow" (EditorValueKind.Integer(Some 2L, Some 500L)) true (Some(SduiValue.Number 26.0))
                              field "signal" "Signal" (EditorValueKind.Integer(Some 1L, Some 500L)) true (Some(SduiValue.Number 9.0)) |])
                       true
                       None |] } |]

    let sampleState () =
        let identity =
            { DocumentId = DocumentId "ta-demo-document"
              CanvasInstanceId = CanvasInstanceId "ta-demo-canvas" }

        { Identity = identity
          Document =
            Some
                { WorkspaceId = "ta-research-demo"
                  Title = "PTMD TA Research"
                  RowsRef = "ta.rows"
                  StatusRef = "ta.status"
                  SharedTimeAxis = true
                  Rows =
                    [| compositeRow
                           "price"
                           TaRowKind.Candlestick
                           "series.price"
                           3.0
                           [| trace "price-1k" TaTraceKind.Candlestick "series.price" "1K K Bar" "" 1.0
                              trace "price-5k" TaTraceKind.Candlestick "series.price-5k" "5K K Bar" "#7c3aed" 1.8 |]
                       row "volume" TaRowKind.Volume "series.volume" 1.0
                       compositeRow
                           "sma"
                           TaRowKind.Sma
                           "series.sma"
                           1.0
                           [| trace "sma-1k" TaTraceKind.Line "series.sma" "1K SMA" "#2563eb" 1.3
                              trace "sma-5k" TaTraceKind.Line "series.sma-5k" "5K SMA" "#b45309" 1.8 |]
                       row "dmi" TaRowKind.Dmi "series.dmi" 1.0
                       row "adx" TaRowKind.Adx "series.adx" 1.0
                       compositeRow
                           "macd"
                           TaRowKind.Macd
                           "series.macd"
                           1.0
                           [| trace "macd-1k" TaTraceKind.Line "series.macd" "1K MACD" "#0f766e" 1.3
                              trace "macd-30k" TaTraceKind.Line "series.macd-30k" "30K MACD causal" "#be185d" 1.8 |]
                       row "heikin" TaRowKind.HeikinAshi "series.heikin" 2.0 |]
                  AllowedActions = [| "reset-view"; "reset-canvas"; "add-row"; "change-query" |]
                  DefaultView = Map [ "visibleBars", SduiValue.Number 48.0 ] }
          Data = sampleSeries 2000
          DocumentRevision = 1L
          DataRevision = 42L
          LastTransportSequence = 2L
          View = { Values = Map.empty }
          Poll = RuntimePollState.Ready
          LastError = None }

    let actionName = function
        | SduiAction.ResetView _ -> "ResetView"
        | SduiAction.ResetCanvas _ -> "ResetCanvas"
        | SduiAction.AddTaRow _ -> "AddTaRow"
        | SduiAction.ApplyTemplate(_, _, templateKey, values) ->
            "ApplyTemplate " + templateKey + " / " + string values.Length + " inputs"
        | SduiAction.RemoveTaRow _ -> "RemoveTaRow"
        | SduiAction.ChangeTaQuery _ -> "ChangeTaQuery"
        | SduiAction.PollDelta _ -> "PollDelta"
        | SduiAction.RequestFullSnapshot _ -> "RequestFullSnapshot"

    let statusData freshness label lag reason quality =
        SduiValue.Object(
            Map [
                "freshness", SduiValue.Text freshness
                "label", SduiValue.Text label
                "lagSeconds", SduiValue.Number lag
                "reasonCode", SduiValue.Text reason
                "watermarkUtc", SduiValue.Text "2026-07-11T09:30:00Z"
                "quality", SduiValue.Text quality
            ])

    [<SPAEntryPoint>]
    let Main () =
        let initialState = sampleState ()
        let runtimeState = Var.Create initialState
        let actionCount = Var.Create 0
        let lastAction = Var.Create "none"
        let rejectNext = Var.Create false
        let applyAuthoritativeAction action =
            let current = runtimeState.Value

            match action, current.Document with
            | SduiAction.AddTaRow(_, row), Some document ->
                runtimeState.Value <-
                    { current with
                        Document = Some { document with Rows = Array.append document.Rows [| row |] }
                        DocumentRevision = current.DocumentRevision + 1L
                        LastTransportSequence = current.LastTransportSequence + 1L }
            | SduiAction.RemoveTaRow(_, rowId), Some document ->
                runtimeState.Value <-
                    { current with
                        Document = Some { document with Rows = document.Rows |> Array.filter (fun row -> row.RowId <> rowId) }
                        DocumentRevision = current.DocumentRevision + 1L
                        LastTransportSequence = current.LastTransportSequence + 1L }
            | SduiAction.ApplyTemplate(_, _, templateKey, values), Some document ->
                let rowId = "template-" + templateKey.Replace(".", "-") + "-" + string (document.Rows.Length + 1)
                let kind = if templateKey = "ta.macd" then TaRowKind.Macd else TaRowKind.Sma
                runtimeState.Value <-
                    { current with
                        Document =
                            Some
                                { document with
                                    Rows = Array.append document.Rows [| row rowId kind ("series." + rowId) 1.0 |] }
                        DocumentRevision = current.DocumentRevision + 1L
                        LastTransportSequence = current.LastTransportSequence + 1L
                        LastError =
                            Some
                                { ReasonCode = "demo-action-received"
                                  Message = templateKey + " accepted with " + string values.Length + " editor inputs"
                                  Recoverable = true } }
            | SduiAction.ResetCanvas _, _ ->
                runtimeState.Value <-
                    { initialState with
                        DocumentRevision = current.DocumentRevision + 1L
                        DataRevision = current.DataRevision + 1L
                        LastTransportSequence = current.LastTransportSequence + 1L }
            | _ -> ()

        let callbacks =
            { SubmitAction =
                fun request ->
                    async {
                        do! Async.Sleep 5000
                        actionCount.Value <- actionCount.Value + 1
                        lastAction.Value <- actionName request.Action
                        if rejectNext.Value then
                            rejectNext.Value <- false
                            return Ok(DynamicActionResult.Rejected(request.RequestId, "demo-rejected", "The demo rejected this action without changing the canvas."))
                        else
                            applyAuthoritativeAction request.Action
                            return Ok(DynamicActionResult.Accepted(request.RequestId, runtimeState.Value.DocumentRevision))
                    } }

        let setLive () =
            runtimeState.Value <-
                { runtimeState.Value with
                    Data = runtimeState.Value.Data |> Map.add "ta.status" (statusData "live" "LIVE / revision 42" 0.0 "within-live-threshold" "complete")
                    Poll = RuntimePollState.Ready
                    LastError = None }

        let setInFlight () =
            runtimeState.Value <- { runtimeState.Value with Poll = RuntimePollState.PollInFlight }

        let setStale () =
            runtimeState.Value <-
                { runtimeState.Value with
                    Data = runtimeState.Value.Data |> Map.add "ta.status" (statusData "stale" "STALE / 45s" 45.0 "source-stopped" "gap suspected")
                    Poll = RuntimePollState.Backoff(DateTimeOffset.UtcNow.AddSeconds 5.0)
                    LastError = Some { ReasonCode = "delta-timeout"; Message = "retaining last good canvas"; Recoverable = true } }

        div [ attr.style "max-width:1460px; margin:0 auto; min-width:0;" ] [
            let demoButtonStyle = attr.style "min-height:24px; padding:2px 6px; white-space:nowrap;"
            div [ Attr.Create "data-testid" "ta-demo-callback-state"; attr.style "min-height:32px; height:auto; display:flex; flex-wrap:wrap; gap:4px; align-items:center; justify-content:flex-end; padding:4px 12px; background:#182a42; color:#d9e5f3; font-size:11px;" ] [
                button [ demoButtonStyle; Attr.Create "data-testid" "ta-demo-live"; on.click (fun _ _ -> setLive ()) ] [ text "Live" ]
                button [ demoButtonStyle; Attr.Create "data-testid" "ta-demo-inflight"; on.click (fun _ _ -> setInFlight ()) ] [ text "In-flight" ]
                button [ demoButtonStyle; Attr.Create "data-testid" "ta-demo-stale"; on.click (fun _ _ -> setStale ()) ] [ text "Stale" ]
                button [ demoButtonStyle; Attr.Create "data-testid" "ta-demo-reject-next"; on.click (fun _ _ -> rejectNext.Value <- true) ] [ text "Reject next" ]
                text "callback actions "
                textView (actionCount.View |> View.Map string)
                text " / last "
                textView lastAction.View
            ]
            TaWorkspaceRenderer.render
                { TaWorkspaceRenderer.defaultOptions with EditorSchemas = sampleEditorSchemas }
                callbacks
                runtimeState
        ]
        |> Doc.RunById "app"
