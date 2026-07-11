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

    let sampleSeries count =
        let candles =
            Array.init count (fun index ->
                let baseline = 21800.0 + float index * 1.7 + Math.Sin(float index / 4.0) * 24.0
                let closeValue = baseline + Math.Cos(float index / 3.0) * 9.0
                candle ("B" + string (index + 1)) baseline closeValue (900.0 + float ((index * 73) % 520)))

        let heikin =
            Array.init count (fun index ->
                let baseline = 21792.0 + float index * 1.65 + Math.Sin(float index / 5.0) * 18.0
                candle ("B" + string (index + 1)) baseline (baseline + Math.Cos(float index / 2.5) * 7.0) (700.0 + float ((index * 41) % 430)))

        let line offset amplitude phase =
            Array.init count (fun index ->
                linePoint ("B" + string (index + 1)) (offset + Math.Sin(float index / phase) * amplitude))

        Map [
            "series.price", SduiValue.Array candles
            "series.volume", SduiValue.Array candles
            "series.sma", SduiValue.Array(line 21820.0 28.0 6.0)
            "series.dmi", SduiValue.Array(line 25.0 11.0 4.5)
            "series.adx", SduiValue.Array(line 22.0 8.0 7.0)
            "series.macd", SduiValue.Array(line 0.0 18.0 5.5)
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
          Options = Map.empty }

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
                    [| row "price" TaRowKind.Candlestick "series.price" 3.0
                       row "volume" TaRowKind.Volume "series.volume" 1.0
                       row "sma" TaRowKind.Sma "series.sma" 1.0
                       row "dmi" TaRowKind.Dmi "series.dmi" 1.0
                       row "adx" TaRowKind.Adx "series.adx" 1.0
                       row "macd" TaRowKind.Macd "series.macd" 1.0
                       row "heikin" TaRowKind.HeikinAshi "series.heikin" 2.0 |]
                  AllowedActions = [| "reset-view"; "reset-canvas"; "add-row"; "change-query" |]
                  DefaultView = Map [ "visibleBars", SduiValue.Number 48.0 ] }
          Data = sampleSeries 96
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
        let runtimeState = Var.Create(sampleState ())
        let actionCount = Var.Create 0
        let lastAction = Var.Create "none"
        let callbacks =
            { SubmitAction =
                fun action ->
                    async {
                        actionCount.Value <- actionCount.Value + 1
                        lastAction.Value <- actionName action
                        return Ok()
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
            div [ Attr.Create "data-testid" "ta-demo-callback-state"; attr.style "height:24px; display:flex; align-items:center; justify-content:flex-end; padding:0 12px; background:#182a42; color:#d9e5f3; font-size:11px;" ] [
                button [ Attr.Create "data-testid" "ta-demo-live"; on.click (fun _ _ -> setLive ()) ] [ text "Live" ]
                button [ Attr.Create "data-testid" "ta-demo-inflight"; on.click (fun _ _ -> setInFlight ()) ] [ text "In-flight" ]
                button [ Attr.Create "data-testid" "ta-demo-stale"; on.click (fun _ _ -> setStale ()) ] [ text "Stale" ]
                text "callback actions "
                textView (actionCount.View |> View.Map string)
                text " / last "
                textView lastAction.View
            ]
            TaWorkspaceRenderer.render TaWorkspaceRenderer.defaultOptions callbacks runtimeState
        ]
        |> Doc.RunById "app"
