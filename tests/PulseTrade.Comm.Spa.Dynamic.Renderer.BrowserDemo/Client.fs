namespace PulseTrade.Comm.Spa.Dynamic.Renderer.BrowserDemo

open System
open PulseTrade.Comm.Spa.Dynamic.Contracts
open PulseTrade.Comm.Spa.Dynamic.Interactive.Client
open PulseTrade.Comm.Spa.Dynamic.Renderer
open WebSharper
open WebSharper.UI
open WebSharper.UI.Html
open WebSharper.UI.Client

[<JavaScript>]
module Client =
    [<Literal>]
    let capacityPointCount = 4000

    [<Literal>]
    let capacitySeriesCount = 28

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

    let marker markerId eventTimeUtc anchor shape fill color label reason =
        { MarkerId = markerId
          EventTimeUtc = eventTimeUtc
          Anchor = anchor
          Shape = shape
          Fill = fill
          Color = color
          Label = label
          Tooltip =
            [| { Key = "reason"; Label = "Reason"; Value = reason }
               { Key = "source"; Label = "Source"; Value = "BrowserDemo" } |] }

    let markerSeries count replacementLabel =
        let point position markers =
            SduiValue.Object(
                Map [ "position", SduiValue.Number(float position)
                      "value", TaMarkerCodec.encodeBucket markers ])
        let stackedPosition = count - 8
        let stackedMarkers =
            Array.append
                [| marker "short-entry" (timestamp (stackedPosition + 1)) TaMarkerAnchor.AboveBar TaMarkerShape.TriangleDown TaMarkerFill.Solid "#000000" (Some "SELL 7591.00") "short entry signal"
                   marker "long-exit" (timestamp (stackedPosition + 1)) TaMarkerAnchor.AboveBar TaMarkerShape.TriangleDown TaMarkerFill.Solid "#dc2626" (Some "SELL 7603.50 PnL +762.50") "long take-profit fill"
                   marker "order-replace" (timestamp (stackedPosition + 1)) TaMarkerAnchor.AboveBar TaMarkerShape.Circle TaMarkerFill.Outline "#2563eb" (Some "REPLACE 7590.75") "replace limit order"
                   marker "order-cancel" (timestamp (stackedPosition + 1)) TaMarkerAnchor.AboveBar TaMarkerShape.Circle TaMarkerFill.Outline "#64748b" (Some "CANCEL 7590.75") "cancel limit order" |]
                (Array.init 60 (fun index ->
                    marker
                        ($"overflow-{index + 1}")
                        (timestamp (stackedPosition + 1))
                        TaMarkerAnchor.AboveBar
                        TaMarkerShape.Circle
                        TaMarkerFill.Outline
                        "#475569"
                        (Some($"ORDER {index + 1}"))
                        "overflow marker detail"))
        SduiValue.Object(
            Map [ "_type", SduiValue.Text "temporal-series.v1"
                  "axisRef", SduiValue.Text "axis.1k"
                  "axisRevision", SduiValue.Number 1.0
                  "points",
                  SduiValue.Array
                      [| point
                             (count - 12)
                             [| marker "long-entry" (timestamp (count - 11)) TaMarkerAnchor.BelowBar TaMarkerShape.TriangleUp TaMarkerFill.Outline "#000000" (Some("BUY 7588.25" + replacementLabel)) ("long entry signal" + replacementLabel) |]
                         point
                             stackedPosition
                             stackedMarkers
                         point
                             (count - 2)
                             [| marker "short-exit" (timestamp (count - 1)) TaMarkerAnchor.BelowBar TaMarkerShape.TriangleUp TaMarkerFill.Solid "#16a34a" (Some "BUY 7574.00 PnL +850.00") "short stop-loss fill" |] |] ])

    let sampleSeries count =
        let sharedAxisRef = "axis.1k"
        let sharedAxis =
            SduiValue.Object(
                Map [ "_type", SduiValue.Text "temporal-axis.v1"
                      "axisRef", SduiValue.Text sharedAxisRef
                      "revision", SduiValue.Number 1.0
                      "points",
                      SduiValue.Array(
                          Array.init count (fun index ->
                              SduiValue.Object(
                                  Map [ "position", SduiValue.Number(float index)
                                        "sourceIntervalId", SduiValue.Text("es-1k:" + string index)
                                        "scaleKey", SduiValue.Text "1K"
                                        "intervalStartUtc", SduiValue.Text(timestamp index)
                                        "intervalEndUtc", SduiValue.Text(timestamp (index + 1))
                                        "eventTimeUtc", SduiValue.Text(timestamp (index + 1))
                                        "observedThroughUtc", SduiValue.Text(timestamp (index + 1))
                                        "availableAtUtc", SduiValue.Text(timestamp (index + 1))
                                        "finality", SduiValue.Text "final"
                                        "projection", SduiValue.Text "candle-span"
                                        "quality", SduiValue.Text "complete" ]))) ])
        let sharedScalarSeries seriesIndex =
            SduiValue.Object(
                Map [ "_type", SduiValue.Text "temporal-series.v1"
                      "axisRef", SduiValue.Text sharedAxisRef
                      "axisRevision", SduiValue.Number 1.0
                      "points",
                      SduiValue.Array(
                          Array.init count (fun index ->
                              SduiValue.Object(
                                  Map [ "position", SduiValue.Number(float index)
                                        "value",
                                        SduiValue.Number(
                                            21820.0
                                            + float seriesIndex * 0.25
                                            + Math.Sin(float index / (6.0 + float (seriesIndex % 5))) * (28.0 + float (seriesIndex % 3))) ]))) ])
        let markers = markerSeries count ""
        let overviewStripeSeries dataRef color position label =
            let stripe =
                { StripeId = dataRef + ":" + string position
                  EventTimeUtc = timestamp (position + 1)
                  Color = color
                  StrokeWidthCssPixels = 1.0
                  Label = Some label
                  Tooltip = [| { Key = "source"; Label = "Source"; Value = "BrowserDemo" } |] }
            SduiValue.Object(
                Map [ "_type", SduiValue.Text "temporal-series.v1"
                      "axisRef", SduiValue.Text sharedAxisRef
                      "axisRevision", SduiValue.Number 1.0
                      "points",
                      SduiValue.Array(
                          [| SduiValue.Object(
                                 Map [ "position", SduiValue.Number(float position)
                                       "value", TaOverviewStripeCodec.encodeBucket [| stripe |] ]) |]) ])
        let candles =
            Array.init count (fun index ->
                let baseline = 21800.0 + float index * 1.7 + Math.Sin(float index / 4.0) * 24.0
                let closeValue = baseline + Math.Cos(float index / 3.0) * 9.0
                temporalPoint
                    ("es-1k:" + string index)
                    "1K"
                    (timestamp index)
                    (timestamp (index + 1))
                    (timestamp (index + 1))
                    (Some(timestamp (index + 1)))
                    "final"
                    "candle-span"
                    "complete"
                    (candlePayload baseline closeValue (900.0 + float ((index * 73) % 520))))

        let sharedCandleSeries =
            SduiValue.Object(
                Map [ "_type", SduiValue.Text "temporal-series.v1"
                      "axisRef", SduiValue.Text sharedAxisRef
                      "axisRevision", SduiValue.Number 1.0
                      "points",
                      SduiValue.Array(
                          candles
                          |> Array.mapi (fun index point ->
                              match point with
                              | SduiValue.Object fields ->
                                  SduiValue.Object(fields |> Map.add "position" (SduiValue.Number(float index)))
                              | value -> value)) ])

        let scaledCandles scaleKey sourcePrefix priceOffset phase volumeOffset =
            Array.init count (fun index ->
                let baseline = 21800.0 + priceOffset + float index * 1.65 + Math.Sin(float index / phase) * 21.0
                let closeValue = baseline + Math.Cos(float index / (phase - 0.75)) * 8.0
                temporalPoint
                    (sourcePrefix + ":" + string index)
                    scaleKey
                    (timestamp index)
                    (timestamp (index + 1))
                    (timestamp (index + 1))
                    (Some(timestamp (index + 1)))
                    "final"
                    "repeat-across-base-buckets"
                    "complete"
                    (candlePayload baseline closeValue (volumeOffset + float ((index * 47) % 610))))

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
            yield sharedAxisRef, sharedAxis
            yield "series.price", sharedCandleSeries
            yield "series.price-5k", SduiValue.Array fiveMinuteCandles
            yield "series.price-5k-heavy", SduiValue.Array(scaledCandles "5K" "es-5k-heavy" 12.0 5.5 1800.0)
            yield "series.price-30k-heavy", SduiValue.Array(scaledCandles "30K" "es-30k-heavy" 28.0 7.5 3200.0)
            yield "series.price-60k-heavy", SduiValue.Array(scaledCandles "60K" "es-60k-heavy" 44.0 9.5 4800.0)
            yield "series.volume", SduiValue.Array candles
            yield "series.sma", sharedScalarSeries 0
            yield "series.markers", markers
            yield "series.overview.signal", overviewStripeSeries "series.overview.signal" "#2563eb" (count - 8) "Signal"
            yield "series.overview.fill", overviewStripeSeries "series.overview.fill" "#dc2626" (count - 8) "Fill"
            for seriesIndex in 1 .. capacitySeriesCount - 1 do
                yield "series.capacity-" + string seriesIndex, sharedScalarSeries seriesIndex
            yield "series.sma-5k", SduiValue.Array fiveMinuteSma
            yield "series.dmi", SduiValue.Array(line 25.0 11.0 4.5)
            yield "series.adx", SduiValue.Array(line 22.0 8.0 7.0)
            yield "series.macd", SduiValue.Array(line 0.0 18.0 5.5)
            yield "series.macd-30k", SduiValue.Array thirtyMinuteMacd
            yield "series.heikin", SduiValue.Array heikin
            yield
                "ta.status",
                SduiValue.Object(
                    Map [
                        "freshness", SduiValue.Text "live"
                        "label", SduiValue.Text "LIVE / revision 42"
                        "watermarkUtc", SduiValue.Text "2026-07-11T09:30:00Z"
                        "quality", SduiValue.Text "complete"
                    ])
        ]

    let appendArrayValue values = function
        | SduiValue.Array existing -> SduiValue.Array(Array.append existing values)
        | existing -> existing

    let appendObjectArray propertyName values = function
        | SduiValue.Object fields ->
            match fields |> Map.tryFind propertyName with
            | Some(SduiValue.Array existing) ->
                SduiValue.Object(fields |> Map.add propertyName (SduiValue.Array(Array.append existing values)))
            | _ -> SduiValue.Object fields
        | existing -> existing

    let updateSeries key update data =
        match data |> Map.tryFind key with
        | Some existing -> data |> Map.add key (update existing)
        | None -> data

    let extendCoverageData startIndex endExclusive data =
        let indexes = [| startIndex .. endExclusive - 1 |]
        let axisPoints =
            indexes
            |> Array.map (fun index ->
                SduiValue.Object(
                    Map [ "position", SduiValue.Number(float index)
                          "sourceIntervalId", SduiValue.Text("es-1k:" + string index)
                          "scaleKey", SduiValue.Text "1K"
                          "intervalStartUtc", SduiValue.Text(timestamp index)
                          "intervalEndUtc", SduiValue.Text(timestamp (index + 1))
                          "eventTimeUtc", SduiValue.Text(timestamp (index + 1))
                          "observedThroughUtc", SduiValue.Text(timestamp (index + 1))
                          "availableAtUtc", SduiValue.Text(timestamp (index + 1))
                          "finality", SduiValue.Text "final"
                          "projection", SduiValue.Text "candle-span"
                          "quality", SduiValue.Text "complete" ]))
        let candles =
            indexes
            |> Array.map (fun index ->
                let baseline = 21800.0 + float index * 1.7 + Math.Sin(float index / 4.0) * 24.0
                let closeValue = baseline + Math.Cos(float index / 3.0) * 9.0
                SduiValue.Object(
                    Map [ "position", SduiValue.Number(float index)
                          "value", candlePayload baseline closeValue (900.0 + float ((index * 73) % 520)) ]))
        data
        |> updateSeries "axis.1k" (appendObjectArray "points" axisPoints)
        |> updateSeries "series.price" (appendObjectArray "points" candles)
        |> updateSeries "series.volume" (appendArrayValue (candles |> Array.map (function SduiValue.Object fields -> Map.find "value" fields | value -> value)))

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
          CandleDataRefs = None
          Options = Map.empty }

    let compositeRow rowId kind dataRef weight traces =
        { row rowId kind dataRef weight with Traces = traces }

    let withRowLabel label (rowSpec: TaRowSpec) =
        { rowSpec with Options = rowSpec.Options |> Map.add "label" (SduiValue.Text label) }

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

    let bindEditor templateKey values rowSpec =
        TaRowEditorBinding.attach
            { TemplateKey = templateKey
              Values = values }
            rowSpec
        |> Result.defaultWith (fun errors -> failwith (errors |> List.map _.Message |> String.concat "; "))

    let sampleState () =
        let identity =
            { DocumentId = DocumentId "ta-demo-document"
              CanvasInstanceId = CanvasInstanceId "ta-demo-canvas" }
        let sparseMissingTraces =
            Array.init 20 (fun index ->
                trace
                    ("sparse-empty-" + string (index + 1))
                    TaTraceKind.Line
                    ("series.sparse-empty-" + string (index + 1))
                    ("Sparse empty " + string (index + 1))
                    "#94a3b8"
                    1.0)

        { Identity = identity
          Document =
            Some
                { WorkspaceId = "ta-research-demo"
                  Title = "PTMD TA Research"
                  RowsRef = "ta.rows"
                  StatusRef = "ta.status"
                  SharedTimeAxis = true
                  TemporalAxisRefs = [| "axis.1k" |]
                  BaseRowId = Some "price"
                  Rows =
                    [| (compositeRow
                            "price"
                            TaRowKind.Candlestick
                            "series.price"
                            3.0
                            [| trace "price-1k" TaTraceKind.Candlestick "series.price" "1K K Bar" "" 1.0
                               trace "price-5k" TaTraceKind.Candlestick "series.price-5k" "5K K Bar" "#7c3aed" 1.8
                               { trace "signals" TaTraceKind.Marker "series.markers" "Signals" "#dc2626" 1.0 with
                                    Options = TaMarkerTraceOptionsCodec.encode ({ TargetTraceId = "price-1k" }: TaMarkerTraceOptions) }
                               { trace "overview-signal" TaTraceKind.OverviewStripe "series.overview.signal" "Signal stripe" "#2563eb" 1.0 with
                                    Options = TaOverviewStripeTraceOptionsCodec.encode { TargetTraceId = "price-1k"; CollisionGroup = "backtest-events"; LayerOrder = 0 } }
                               { trace "overview-fill" TaTraceKind.OverviewStripe "series.overview.fill" "Fill stripe" "#dc2626" 1.0 with
                                    Options = TaOverviewStripeTraceOptionsCodec.encode { TargetTraceId = "price-1k"; CollisionGroup = "backtest-events"; LayerOrder = 1 } } |]
                        |> withRowLabel "ES 1K + SMA(20)")
                       row "volume" TaRowKind.Candlestick "series.price-5k-heavy" 1.0
                       |> withRowLabel "ES 5K K Bar"
                       compositeRow
                           "sma"
                           TaRowKind.Sma
                           "series.sma"
                           1.0
                           [| trace "sma-1k" TaTraceKind.Line "series.sma" "1K SMA" "#2563eb" 1.3
                              trace "sma-5k" TaTraceKind.Line "series.sma-5k" "5K SMA" "#b45309" 1.8 |]
                       |> bindEditor "ta.sma" (DynamicEditorValidation.defaultInputs sampleEditorSchemas[0])
                       row "dmi" TaRowKind.Candlestick "series.price-30k-heavy" 1.0
                       |> withRowLabel "ES 30K K Bar"
                       row "adx" TaRowKind.Candlestick "series.price-60k-heavy" 1.0
                       |> withRowLabel "ES 60K K Bar"
                       compositeRow
                           "macd"
                           TaRowKind.Macd
                           "series.macd"
                           1.0
                           (Array.append
                               [| trace "macd-1k" TaTraceKind.Line "series.macd" "1K MACD" "#0f766e" 1.3
                                  trace "macd-30k" TaTraceKind.Line "series.macd-30k" "30K MACD causal" "#be185d" 1.8 |]
                               sparseMissingTraces)
                       row "heikin" TaRowKind.HeikinAshi "series.heikin" 2.0 |]
                  EditorSchemas = sampleEditorSchemas
                  AllowedActions =
                    [| "reset-view"
                       "reset-canvas"
                       "add-row"
                       "change-query"
                       "shared-cursor-changed"
                       "visible-range-changed" |]
                  DefaultView =
                    Map [
                        "visibleBars", SduiValue.Number 48.0
                        "query.fromUtc", SduiValue.Text "2026-08-01T00:00:00.0000000+00:00"
                        "query.toUtcExclusive", SduiValue.Text "2026-10-01T00:00:00.0000000+00:00"
                    ] }
          Data = sampleSeries capacityPointCount
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
        | SduiAction.SharedCursorChanged _ -> "SharedCursorChanged"
        | SduiAction.VisibleRangeChanged _ -> "VisibleRangeChanged"
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
        let mainStartedAt = DateTime.UtcNow
        let initialState = sampleState ()
        let candleDataRefs =
            [| "series.price"
               "series.price-5k-heavy"
               "series.price-30k-heavy"
               "series.price-60k-heavy"
               "series.heikin" |]

        let replaceCandleData replacement data =
            let incrementClose fields =
                match fields |> Map.tryFind "c" with
                | Some(SduiValue.Number currentClose) ->
                    fields |> Map.add "c" (SduiValue.Number(currentClose + float replacement * 0.125))
                | _ -> fields

            let updateCandle = function
                | SduiValue.Object fields ->
                    match fields |> Map.tryFind "value" with
                    | Some(SduiValue.Object candleFields) ->
                        SduiValue.Object(fields |> Map.add "value" (SduiValue.Object(incrementClose candleFields)))
                    | _ -> SduiValue.Object(incrementClose fields)
                | value -> value

            candleDataRefs
            |> Array.fold (fun currentData dataRef ->
                currentData
                |> Map.change dataRef (Option.map (function
                    | SduiValue.Array values -> SduiValue.Array(values |> Array.map updateCandle)
                    | SduiValue.Object fields ->
                        match fields |> Map.tryFind "points" with
                        | Some(SduiValue.Array values) ->
                            SduiValue.Object(fields |> Map.add "points" (SduiValue.Array(values |> Array.map updateCandle)))
                        | _ -> SduiValue.Object fields
                    | value -> value))) data

        let candleReplacementPacketsFor (state: RuntimeState) =
            let knownDataRefs = RuntimeReducer.knownDataRefs state
            let replacementData =
                replaceCandleData 1 state.Data
                |> Map.filter (fun dataRef _ -> Set.contains dataRef knownDataRefs)

            { Protocol = DynamicRuntimeDefaults.markerProtocol
              Kind = RuntimeFrameKind.Snapshot
              DocumentId = state.Identity.DocumentId
              CanvasInstanceId = state.Identity.CanvasInstanceId
              DocumentRevision = state.DocumentRevision
              BaseDataRevision = None
              DataRevision = state.DataRevision + 1L
              TransportSequence = state.LastTransportSequence + 1L
              Payload =
                RuntimePayload.Snapshot
                    { Data = replacementData
                      Freshness = TaFreshness.Live } }
            |> RuntimeSnapshotTransportCodec.encodeFrame
            |> function
                | Ok packets -> packets
                | Error message -> failwith message

        let candleReplacementPackets = candleReplacementPacketsFor initialState
        let candleReplacementWireChars = candleReplacementPackets |> Array.sumBy _.Length
        let sampleBuildMilliseconds = DateTime.UtcNow.Subtract(mainStartedAt).TotalMilliseconds
        let runtimeState = Var.Create initialState
        let actionCount = Var.Create 0
        let lastAction = Var.Create "none"
        let rejectNext = Var.Create false
        let mutable actionInFlight = false
        let previewStreamGeneration = Var.Create 0
        let previewStreamUpdates = Var.Create 0
        let markerReplacementCount = Var.Create 0
        let candleWorkloadReplacementCount = Var.Create 0
        let candleWorkloadOutcome = Var.Create "idle"
        let candleWorkloadError = Var.Create ""
        let candleWorkloadStageDiagnostics = Var.Create ""
        let scenarioReplacementCount = Var.Create 0
        let scenarioReplacementOutcome = Var.Create "idle"
        let mutable candleWorkloadGeneration = 0
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
            | SduiAction.ApplyTemplate(_, requestedRowId, templateKey, values), Some document ->
                let rowId =
                    requestedRowId
                    |> Option.defaultValue ("template-" + templateKey.Replace(".", "-") + "-" + string (document.Rows.Length + 1))
                let kind = if templateKey = "ta.macd" then TaRowKind.Macd else TaRowKind.Sma
                let binding = { TemplateKey = templateKey; Values = values }
                let nextRows =
                    match requestedRowId with
                    | None ->
                        row rowId kind ("series." + rowId) 1.0
                        |> TaRowEditorBinding.attach binding
                        |> Result.defaultWith (fun errors -> failwith (errors |> List.map _.Message |> String.concat "; "))
                        |> Array.singleton
                        |> Array.append document.Rows
                    | Some existingRowId ->
                        document.Rows
                        |> Array.map (fun existing ->
                            if existing.RowId <> existingRowId then existing
                            else
                                { existing with Kind = kind }
                                |> TaRowEditorBinding.attach binding
                                |> Result.defaultWith (fun errors -> failwith (errors |> List.map _.Message |> String.concat "; ")))
                runtimeState.Value <-
                    { current with
                        Document =
                            Some
                                { document with
                                    Rows = nextRows }
                        DocumentRevision = current.DocumentRevision + 1L
                        LastTransportSequence = current.LastTransportSequence + 1L
                        LastError =
                            Some
                                { ReasonCode = "demo-action-received"
                                  Message = templateKey + " accepted with " + string values.Length + " editor inputs"
                                  Recoverable = true } }
            | SduiAction.VisibleRangeChanged(_, change), _ ->
                let currentCount =
                    current.Data
                    |> Map.tryFind "series.price"
                    |> Option.bind (function
                        | SduiValue.Array values -> Some values.Length
                        | SduiValue.Object fields ->
                            fields
                            |> Map.tryFind "points"
                            |> Option.bind (function SduiValue.Array values -> Some values.Length | _ -> None)
                        | _ -> None)
                    |> Option.defaultValue 0
                let loadedEnd = timestamp currentCount
                if change.EndEventTimeExclusiveUtc.CompareTo(loadedEnd) > 0 then
                    let nextCount = max currentCount (capacityPointCount + 400)
                    let rec appendNextChunk startIndex =
                        if startIndex < nextCount then
                            WebSharper.JavaScript.JS.RequestAnimationFrame(fun _ ->
                                let latest = runtimeState.Value
                                let endExclusive = min nextCount (startIndex + 100)
                                runtimeState.Value <-
                                    { latest with
                                        Data = extendCoverageData startIndex endExclusive latest.Data
                                        DataRevision = latest.DataRevision + 1L
                                        LastTransportSequence = latest.LastTransportSequence + 1L }
                                appendNextChunk endExclusive)
                            |> ignore
                    appendNextChunk currentCount
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
                        if actionInFlight then
                            return Error { Code = "demo-action-in-flight"; Message = "The demo accepts only one remote action at a time." }
                        else
                            actionInFlight <- true
                            try
                                do! Async.Sleep 750
                                actionCount.Value <- actionCount.Value + 1
                                lastAction.Value <- actionName request.Action
                                if rejectNext.Value then
                                    rejectNext.Value <- false
                                    return Ok(DynamicActionResult.Rejected(request.RequestId, "demo-rejected", "The demo rejected this action without changing the canvas."))
                                else
                                    applyAuthoritativeAction request.Action
                                    return Ok(DynamicActionResult.Accepted(request.RequestId, runtimeState.Value.DocumentRevision))
                            finally
                                actionInFlight <- false
                    } }

        let setLive () =
            runtimeState.Value <-
                { runtimeState.Value with
                    Data = runtimeState.Value.Data |> Map.add "ta.status" (statusData "live" "LIVE / revision 42" 0.0 "within-live-threshold" "complete")
                    Poll = RuntimePollState.Ready
                    LastError = None }

        let updateLatestPreview () =
            let updateCandleValue = function
                | SduiValue.Object fields ->
                    match fields |> Map.tryFind "value" with
                    | Some(SduiValue.Object candleFields) ->
                        let currentClose =
                            candleFields
                            |> Map.tryFind "c"
                            |> Option.bind (function SduiValue.Number value -> Some value | _ -> None)
                            |> Option.defaultValue 0.0
                        SduiValue.Object(fields |> Map.add "value" (SduiValue.Object(candleFields |> Map.add "c" (SduiValue.Number(currentClose + 1.25)))))
                    | _ -> SduiValue.Object fields
                | value -> value

            let current = runtimeState.Value
            let nextPrice =
                current.Data
                |> Map.tryFind "series.price"
                |> Option.bind (function
                    | SduiValue.Object fields ->
                        fields
                        |> Map.tryFind "points"
                        |> Option.bind (function
                            | SduiValue.Array values ->
                                values
                                |> Array.mapi (fun index value -> if index = values.Length - 1 then updateCandleValue value else value)
                                |> SduiValue.Array
                                |> fun points -> SduiValue.Object(fields |> Map.add "points" points)
                                |> Some
                            | _ -> None)
                    | _ -> None)

            match nextPrice with
            | Some price ->
                runtimeState.Value <-
                    { current with
                        Data = current.Data |> Map.add "series.price" price
                        DataRevision = current.DataRevision + 1L
                        LastTransportSequence = current.LastTransportSequence + 1L }
            | None -> ()

        let replaceFiveCandleRows () =
            candleWorkloadGeneration <- candleWorkloadGeneration + 1
            let generation = candleWorkloadGeneration
            candleWorkloadOutcome.Value <- "pending"
            candleWorkloadError.Value <- ""
            candleWorkloadStageDiagnostics.Value <- ""
            let current = runtimeState.Value

            let maxima = System.Collections.Generic.Dictionary<string, float>()

            let stageKey = function
                | BrowserRuntimeFramePumpStage.PrepareFrame frameIndex -> $"prepare-frame:{frameIndex}"
                | BrowserRuntimeFramePumpStage.ParseTransportPacket packetIndex -> $"parse-packet:{packetIndex}"
                | BrowserRuntimeFramePumpStage.DecodeSnapshotValue dataRef -> "decode-value:" + dataRef
                | BrowserRuntimeFramePumpStage.DecodeArrayBatch dataRef -> "decode-array-batch:" + dataRef
                | BrowserRuntimeFramePumpStage.ReduceFrame frameIndex -> $"reduce-frame:{frameIndex}"
                | BrowserRuntimeFramePumpStage.BuildAxisAuthority dataRef -> "build-axis:" + dataRef
                | BrowserRuntimeFramePumpStage.ValidateTemporalItem dataRef -> "validate-temporal:" + dataRef
                | BrowserRuntimeFramePumpStage.ApplyOverlays -> "apply-overlays"

            let observeStage stage elapsedMilliseconds =
                let key = stageKey stage
                match maxima.TryGetValue key with
                | true, currentMaximum when currentMaximum >= elapsedMilliseconds -> ()
                | _ -> maxima[key] <- elapsedMilliseconds

            let publishStageDiagnostics () =
                candleWorkloadStageDiagnostics.Value <-
                    maxima
                    |> Seq.map (fun pair -> pair.Key, pair.Value)
                    |> Seq.sortByDescending snd
                    |> Seq.truncate 12
                    |> Seq.map (fun (key, milliseconds) -> key + "=" + sprintf "%.2f" milliseconds)
                    |> String.concat ";"

            BrowserRuntimeFramePump.reduceChunkedSnapshotPacketsObserved
                current
                candleReplacementPackets
                (fun () -> generation = candleWorkloadGeneration)
                observeStage
                (function
                    | BrowserRuntimeFramePumpOutcome.Applied candidate ->
                        publishStageDiagnostics ()
                        runtimeState.Value <- candidate
                        candleWorkloadReplacementCount.Value <- candleWorkloadReplacementCount.Value + 1
                        candleWorkloadOutcome.Value <- "applied"
                    | BrowserRuntimeFramePumpOutcome.Rejected failure ->
                        publishStageDiagnostics ()
                        candleWorkloadOutcome.Value <- "rejected:" + failure.Code
                        candleWorkloadError.Value <- failure.Message
                        let current = runtimeState.Value
                        runtimeState.Value <-
                            { current with
                                LastError =
                                    Some
                                        { ReasonCode = failure.Code
                                          Message = failure.Message
                                          Recoverable = false } }
                    | BrowserRuntimeFramePumpOutcome.Superseded ->
                        publishStageDiagnostics ()
                        candleWorkloadOutcome.Value <- "superseded")

        let updateLatestSmaValue nextValue =
            let updatePoint = function
                | SduiValue.Object fields -> SduiValue.Object(fields |> Map.add "value" nextValue)
                | value -> value

            let current = runtimeState.Value
            let nextSeries =
                current.Data
                |> Map.tryFind "series.sma"
                |> Option.bind (function
                    | SduiValue.Object fields ->
                        fields
                        |> Map.tryFind "points"
                        |> Option.bind (function
                            | SduiValue.Array values when values.Length > 0 ->
                                let nextPoints = values |> Array.mapi (fun index value -> if index = values.Length - 1 then updatePoint value else value)
                                Some(SduiValue.Object(fields |> Map.add "points" (SduiValue.Array nextPoints)))
                            | _ -> None)
                    | _ -> None)

            match nextSeries with
            | Some series ->
                runtimeState.Value <-
                    { current with
                        Data = current.Data |> Map.add "series.sma" series
                        DataRevision = current.DataRevision + 1L
                        LastTransportSequence = current.LastTransportSequence + 1L }
            | None -> ()

        let startPreviewStream () =
            let generation = previewStreamGeneration.Value + 1
            previewStreamGeneration.Value <- generation
            previewStreamUpdates.Value <- 0

            async {
                for _ in 1 .. 12 do
                    do! Async.Sleep 1000
                    if previewStreamGeneration.Value = generation then
                        updateLatestPreview ()
                        previewStreamUpdates.Value <- previewStreamUpdates.Value + 1
            }
            |> Async.StartImmediate

        let setInFlight () =
            runtimeState.Value <- { runtimeState.Value with Poll = RuntimePollState.PollInFlight }

        let setPaused () =
            runtimeState.Value <- { runtimeState.Value with Poll = RuntimePollState.PausedForResync }

        let setStale () =
            runtimeState.Value <-
                { runtimeState.Value with
                    Data = runtimeState.Value.Data |> Map.add "ta.status" (statusData "stale" "STALE / 45s" 45.0 "source-stopped" "gap suspected")
                    Poll = RuntimePollState.Backoff(DateTimeOffset.UtcNow.AddSeconds 5.0)
                    LastError = Some { ReasonCode = "delta-timeout"; Message = "retaining last good canvas"; Recoverable = true } }

        let replaceDocumentWithSameRevision () =
            let current = runtimeState.Value
            let nextDocument =
                current.Document
                |> Option.map (fun document ->
                    { document with
                        Title = "PTMD TA Research / SMA(30)"
                        Rows =
                            document.Rows
                            |> Array.map (fun row ->
                                if row.RowId <> "price" then row
                                else { row with Options = row.Options |> Map.add "label" (SduiValue.Text "ES 1K + SMA(30)") }) })

            runtimeState.Value <-
                { current with
                    Identity =
                        { DocumentId = DocumentId "ta-demo-document-replacement"
                          CanvasInstanceId = CanvasInstanceId "ta-demo-canvas-replacement" }
                    Document = nextDocument }

        let replaceMarkers () =
            let replacement = markerReplacementCount.Value + 1
            markerReplacementCount.Value <- replacement
            let current = runtimeState.Value
            runtimeState.Value <-
                { current with
                    Data = current.Data |> Map.add "series.markers" (markerSeries capacityPointCount (" replacement " + string replacement))
                    DataRevision = current.DataRevision + 1L
                    LastTransportSequence = current.LastTransportSequence + 1L }

        let overviewDataRefs = [| "series.overview.signal"; "series.overview.fill" |]
        let clearOverviewStripes () =
            let current = runtimeState.Value
            let clearedData =
                overviewDataRefs
                |> Array.fold (fun data dataRef ->
                    data
                    |> Map.change dataRef (Option.map (function
                        | SduiValue.Object fields ->
                            SduiValue.Object(fields |> Map.add "points" (SduiValue.Array [||]))
                        | value -> value))) current.Data
            runtimeState.Value <-
                { current with
                    Data = clearedData
                    DataRevision = current.DataRevision + 1L
                    LastTransportSequence = current.LastTransportSequence + 1L }

        let populateOverviewStripes () =
            let current = runtimeState.Value
            let populatedData =
                overviewDataRefs
                |> Array.fold (fun data dataRef ->
                    match Map.tryFind dataRef initialState.Data with
                    | Some value -> Map.add dataRef value data
                    | None -> data) current.Data
            runtimeState.Value <-
                { current with
                    Data = populatedData
                    DataRevision = current.DataRevision + 1L
                    LastTransportSequence = current.LastTransportSequence + 1L }

        let replaceScenarioOverlays () =
            let replacement = scenarioReplacementCount.Value + 1
            let current = runtimeState.Value
            let overlayValues =
                [| yield "series.markers", markerSeries capacityPointCount (" scenario " + string replacement)
                   for dataRef in overviewDataRefs do
                       match Map.tryFind dataRef current.Data with
                       | Some value -> yield dataRef, value
                       | None -> () |]
            let frame =
                { Protocol = DynamicRuntimeDefaults.markerProtocol
                  Kind = RuntimeFrameKind.Patch
                  DocumentId = current.Identity.DocumentId
                  CanvasInstanceId = current.Identity.CanvasInstanceId
                  DocumentRevision = current.DocumentRevision
                  BaseDataRevision = Some current.DataRevision
                  DataRevision = current.DataRevision + 1L
                  TransportSequence = current.LastTransportSequence + 1L
                  Payload =
                    RuntimePayload.Patch
                        { Operations =
                            overlayValues
                            |> Array.map (fun (dataRef, value) -> PatchOperation.ReplaceDataRef(dataRef, value)) } }
            let candidate, effect = RuntimeReducer.reduce current frame
            match effect with
            | RuntimeEffect.NoEffect when candidate.DataRevision = frame.DataRevision ->
                runtimeState.Value <- candidate
                scenarioReplacementCount.Value <- replacement
                scenarioReplacementOutcome.Value <- "applied"
            | RuntimeEffect.RequestResync _ -> scenarioReplacementOutcome.Value <- "resync"
            | RuntimeEffect.RejectFrame _ -> scenarioReplacementOutcome.Value <- "rejected"
            | _ -> scenarioReplacementOutcome.Value <- "unexpected-effect"

        let rendererStartedAt = DateTime.UtcNow
        let rendererDoc =
            TaWorkspaceRenderer.render
                TaWorkspaceRenderer.defaultOptions
                callbacks
                runtimeState
        let rendererSetupMilliseconds = DateTime.UtcNow.Subtract(rendererStartedAt).TotalMilliseconds

        div [
            attr.style "max-width:1460px; margin:0 auto; min-width:0;"
            Attr.Create "data-capacity-positions" (string capacityPointCount)
            Attr.Create "data-capacity-shared-series" (string capacitySeriesCount)
            Attr.Create "data-sparse-empty-traces" "20"
            Attr.Create "data-sample-build-ms" (string sampleBuildMilliseconds)
            Attr.Create "data-renderer-setup-ms" (string rendererSetupMilliseconds)
            Attr.Create "data-candle-workload-wire-chars" (string candleReplacementWireChars)
            Attr.Create "data-candle-workload-packets" (string candleReplacementPackets.Length)
            Attr.Dynamic "data-preview-stream-updates" (previewStreamUpdates.View |> View.Map string)
            Attr.Dynamic "data-marker-replacements" (markerReplacementCount.View |> View.Map string)
            Attr.Dynamic "data-candle-workload-replacements" (candleWorkloadReplacementCount.View |> View.Map string)
            Attr.Dynamic "data-candle-workload-outcome" candleWorkloadOutcome.View
            Attr.Dynamic "data-candle-workload-error" candleWorkloadError.View
            Attr.Dynamic "data-candle-workload-stage-diagnostics" candleWorkloadStageDiagnostics.View
            Attr.Dynamic "data-scenario-replacements" (scenarioReplacementCount.View |> View.Map string)
            Attr.Dynamic "data-scenario-replacement-outcome" scenarioReplacementOutcome.View
        ] [
            let demoButtonStyle = attr.style "min-height:24px; padding:2px 6px; white-space:nowrap;"
            div [
                Attr.Create "data-testid" "ta-demo-callback-state"
                Attr.Dynamic "data-callback-count" (actionCount.View |> View.Map string)
                Attr.Dynamic "data-last-action" lastAction.View
                attr.style "min-height:32px; height:auto; display:flex; flex-wrap:wrap; gap:4px; align-items:center; justify-content:flex-end; padding:4px 12px; background:#182a42; color:#d9e5f3; font-size:11px;"
            ] [
                button [ demoButtonStyle; Attr.Create "data-testid" "ta-demo-live"; on.click (fun _ _ -> setLive ()) ] [ text "Live" ]
                button [ demoButtonStyle; Attr.Create "data-testid" "ta-demo-preview-update"; on.click (fun _ _ -> updateLatestPreview ()) ] [ text "Update preview" ]
                button [ demoButtonStyle; Attr.Create "data-testid" "ta-demo-preview-stream"; on.click (fun _ _ -> startPreviewStream ()) ] [ text "Stream preview" ]
                button [ demoButtonStyle; Attr.Create "data-testid" "ta-demo-replace-five-candle-rows"; on.click (fun _ _ -> replaceFiveCandleRows ()) ] [ text "Replace 5 candle rows" ]
                span [ Attr.Create "data-testid" "ta-demo-candle-workload-outcome" ] [ textView candleWorkloadOutcome.View ]
                button [ demoButtonStyle; Attr.Create "data-testid" "ta-demo-legend-undef"; on.click (fun _ _ -> updateLatestSmaValue SduiValue.Null) ] [ text "Legend Undef" ]
                button [ demoButtonStyle; Attr.Create "data-testid" "ta-demo-legend-long"; on.click (fun _ _ -> updateLatestSmaValue (SduiValue.Number 123456789.123456)) ] [ text "Legend long" ]
                button [ demoButtonStyle; Attr.Create "data-testid" "ta-demo-inflight"; on.click (fun _ _ -> setInFlight ()) ] [ text "In-flight" ]
                button [ demoButtonStyle; Attr.Create "data-testid" "ta-demo-paused"; on.click (fun _ _ -> setPaused ()) ] [ text "Paused" ]
                button [ demoButtonStyle; Attr.Create "data-testid" "ta-demo-stale"; on.click (fun _ _ -> setStale ()) ] [ text "Stale" ]
                button [ demoButtonStyle; Attr.Create "data-testid" "ta-demo-replace-document"; on.click (fun _ _ -> replaceDocumentWithSameRevision ()) ] [ text "Replace document" ]
                button [ demoButtonStyle; Attr.Create "data-testid" "ta-demo-replace-markers"; on.click (fun _ _ -> replaceMarkers ()) ] [ text "Replace markers" ]
                button [ demoButtonStyle; Attr.Create "data-testid" "ta-demo-replace-scenario-overlays"; on.click (fun _ _ -> replaceScenarioOverlays ()) ] [ text "Replace scenario overlays" ]
                button [ demoButtonStyle; Attr.Create "data-testid" "ta-demo-clear-overview-stripes"; on.click (fun _ _ -> clearOverviewStripes ()) ] [ text "Clear stripes" ]
                button [ demoButtonStyle; Attr.Create "data-testid" "ta-demo-populate-overview-stripes"; on.click (fun _ _ -> populateOverviewStripes ()) ] [ text "Populate stripes" ]
                button [ demoButtonStyle; Attr.Create "data-testid" "ta-demo-reject-next"; on.click (fun _ _ -> rejectNext.Value <- true) ] [ text "Reject next" ]
                text "callback actions "
                textView (actionCount.View |> View.Map string)
                text " / last "
                textView lastAction.View
            ]
            rendererDoc
        ]
        |> Doc.RunById "app"
