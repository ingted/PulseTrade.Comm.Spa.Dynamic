module PulseTrade.Comm.Spa.Dynamic.Renderer.Tests

open System
open Expecto
open PulseTrade.Comm.Spa.Dynamic.Contracts
open PulseTrade.Comm.Spa.Dynamic.Renderer

let candle timestamp openValue high low close volume =
    SduiValue.Object(
        Map [
            "t", SduiValue.Text timestamp
            "o", SduiValue.Number openValue
            "h", SduiValue.Number high
            "l", SduiValue.Number low
            "c", SduiValue.Number close
            "v", SduiValue.Number volume
        ])

let tests =
    testList "TA renderer model" [
        testCase "workspace bootstrap distinguishes lifecycle progress from terminal failure" <| fun _ ->
            let identity = { DocumentId = DocumentId "pending"; CanvasInstanceId = CanvasInstanceId "canvas" }
            let initial = RuntimeReducer.initial identity
            let preparing = RendererModel.workspaceBootstrapPresentation initial
            Expect.equal preparing.State "preparing" "an unmounted channel is a normal bootstrap state"
            Expect.isFalse preparing.IsError "bootstrap must not be presented as a terminal error"

            let connecting = RendererModel.workspaceBootstrapPresentation { initial with Poll = RuntimePollState.MountedIdle }
            Expect.equal connecting.State "connecting" "a mounted channel waits for its first document"
            Expect.stringContains connecting.Detail "initial workspace document" "the user should see the actual wait condition"

            let recovering =
                RendererModel.workspaceBootstrapPresentation
                    { initial with
                        Poll = RuntimePollState.Backoff(DateTimeOffset.Parse("2026-07-13T07:00:00Z"))
                        LastError = Some { ReasonCode = "transient-timeout"; Message = "retrying"; Recoverable = true } }
            Expect.equal recovering.State "recovering" "recoverable errors keep the workspace lifecycle alive"
            Expect.isFalse recovering.IsError "recoverable transport failures are not terminal"

            let unavailable =
                RendererModel.workspaceBootstrapPresentation
                    { initial with LastError = Some { ReasonCode = "invalid-document"; Message = "rejected"; Recoverable = false } }
            Expect.equal unavailable.State "unavailable" "non-recoverable errors are explicit"
            Expect.isTrue unavailable.IsError "only terminal errors use error presentation"

        testCase "typed candle series rejects malformed items" <| fun _ ->
            let data =
                Map [
                    "price",
                    SduiValue.Array
                        [| candle "B1" 10.0 12.0 9.0 11.0 100.0
                           SduiValue.Object(Map [ "t", SduiValue.Text "missing-fields" ]) |]
                ]

            let actual = RendererModel.candleSeries "price" data
            Expect.equal actual.Length 1 "only the complete typed candle should survive"
            Expect.equal actual[0].Close 11.0 "close must preserve transport value"

        testCase "visible window clamps count and start" <| fun _ ->
            let actual =
                RendererModel.clampWindow 12 160 96 { StartIndex = 90; Count = 48 }

            Expect.equal actual.Count 48 "requested count remains when bounded"
            Expect.equal actual.StartIndex 48 "start clamps so the window ends at the series tail"

        testCase "query draft is initialized from server document metadata without demo literals" <| fun _ ->
            let actual =
                RendererModel.queryDraft
                    (Map [
                        "query.sourceId", SduiValue.Text "binance"
                        "query.instrument", SduiValue.Text "BTCUSDT"
                        "query.intervalMinutes", SduiValue.Number 1.0
                        "query.fromUtc", SduiValue.Text "2026-07-01"
                        "query.toUtcExclusive", SduiValue.Text "2026-07-12"
                        "query.includePartial", SduiValue.Bool false
                    ])

            Expect.equal actual.SourceId "binance" "Source identity should remain available to the renderer."
            Expect.equal actual.Instrument "BTCUSDT" "Instrument must come from the RuntimeDocument."
            Expect.equal actual.IntervalMinutes "1" "Interval must come from the RuntimeDocument."
            Expect.equal actual.FromUtc "2026-07-01" "From boundary must remain server authoritative."
            Expect.equal actual.ToUtcExclusive "2026-07-12" "Exclusive boundary must remain server authoritative."
            Expect.isFalse actual.IncludePartial "Partial-bar policy must survive the SDUI document."

            let empty = RendererModel.queryDraft Map.empty
            Expect.equal empty.Instrument "" "Missing metadata must not fall back to a demo instrument."
            Expect.equal empty.IntervalMinutes "" "Missing metadata must not fall back to a demo interval."

        testCase "visible window handles short series" <| fun _ ->
            let actual =
                RendererModel.clampWindow 12 160 5 { StartIndex = 20; Count = 48 }

            Expect.equal actual { StartIndex = 0; Count = 5 } "short data renders all available points"

        testCase "follow latest resolves to tail while historical viewport remains stable" <| fun _ ->
            let requested = { StartIndex = 24; Count = 48 }
            let latest = RendererModel.resolveWindow 12 160 2000 true requested
            let historical = RendererModel.resolveWindow 12 160 2000 false requested
            let latestAfterDelta = RendererModel.resolveWindow 12 160 2001 true requested
            let historicalAfterDelta = RendererModel.resolveWindow 12 160 2001 false requested

            Expect.equal latest { StartIndex = 1952; Count = 48 } "follow-latest should anchor the bounded window to the loaded tail"
            Expect.equal latestAfterDelta { StartIndex = 1953; Count = 48 } "a delta should advance only a follow-latest viewport"
            Expect.equal historical requested "a historical viewport should preserve its explicit start"
            Expect.equal historicalAfterDelta requested "a delta must not force a historical viewport back to the tail"
            Expect.equal (RendererModel.viewportMaximumStart 2000 latest) 1952 "navigator maximum start should expose the full loaded range"

        testCase "pointer ratio maps deterministically to a visible bar" <| fun _ ->
            Expect.equal (RendererModel.cursorIndexFromRatio 48 0.0) (Some 0) "left edge should select the first visible bar"
            Expect.equal (RendererModel.cursorIndexFromRatio 48 0.5) (Some 24) "middle should select the nearest visible bar"
            Expect.equal (RendererModel.cursorIndexFromRatio 48 1.0) (Some 47) "right edge should select the last visible bar"
            Expect.equal (RendererModel.cursorIndexFromClientX 48 100.0 800.0 300.0) (Some 12) "client coordinates should be normalized by the actual row width"
            Expect.equal (RendererModel.cursorIndexFromRatio 0 0.5) None "empty series has no cursor index"
            Expect.equal (RendererModel.cursorIndexFromClientX 48 0.0 0.0 10.0) None "zero-width row cannot be hit-tested"

        testCase "select window is deterministic" <| fun _ ->
            let actual = RendererModel.selectWindow { StartIndex = 2; Count = 3 } [| 0; 1; 2; 3; 4; 5 |]
            Expect.sequenceEqual actual [| 2; 3; 4 |] "window must preserve ordering"

        testCase "normalization keeps higher value visually above lower" <| fun _ ->
            let lowY = RendererModel.normalize 10.0 20.0 5.0 100.0 10.0
            let highY = RendererModel.normalize 10.0 20.0 5.0 100.0 20.0
            Expect.isLessThan highY lowY "SVG y decreases as value rises"

        testCase "shared cursor snapshot aligns every visible row by window index" <| fun _ ->
            let line timestamp value =
                SduiValue.Object(Map [ "t", SduiValue.Text timestamp; "v", SduiValue.Number value ])

            let row rowId kind dataRef =
                { RowId = rowId
                  Kind = kind
                  DataRef = dataRef
                  HeightWeight = 1.0
                  Visible = true
                  Traces = [||]
                  Options = Map.empty }

            let document =
                { WorkspaceId = "cursor-test"
                  Title = "Cursor"
                  RowsRef = "rows"
                  StatusRef = "status"
                  SharedTimeAxis = true
                  Rows = [| row "price" TaRowKind.Candlestick "price"; row "sma" TaRowKind.Sma "sma" |]
                  AllowedActions = [||]
                  DefaultView = Map.empty }

            let data =
                Map [
                    "price", SduiValue.Array [| candle "B1" 10.0 12.0 9.0 11.0 100.0; candle "B2" 11.0 13.0 10.0 12.0 120.0 |]
                    "sma", SduiValue.Array [| line "B1" 10.5; line "B2" 11.5 |]
                ]

            let snapshot = RendererModel.cursorSnapshot document data { StartIndex = 0; Count = 2 } 1 |> Option.defaultWith (fun () -> failwith "cursor missing")
            Expect.equal snapshot.Timestamp "B2" "Cursor timestamp should come from the shared visible index."
            Expect.equal (snapshot.Values |> Array.map _.Label) [| "price"; "sma" |] "Every visible row should expose one cursor value."
            Expect.stringContains snapshot.Values[0].Value "C 12" "Price detail should expose OHLC."
            Expect.equal snapshot.Values[1].Value "11.5" "Indicator detail should align to the same bar."

        testCase "status presentation preserves all freshness quality and last-good error states" <| fun _ ->
            let identity = { DocumentId = DocumentId "status-doc"; CanvasInstanceId = CanvasInstanceId "status-canvas" }
            let state =
                { Identity = identity
                  Document = None
                  Data =
                    Map [
                        "status",
                        SduiValue.Object(
                            Map [
                                "freshness", SduiValue.Text "stale"
                                "lagSeconds", SduiValue.Number 45.0
                                "reasonCode", SduiValue.Text "source-stopped"
                                "label", SduiValue.Text "STALE / 45s"
                                "watermarkUtc", SduiValue.Text "2026-07-11T00:00:00Z"
                                "quality", SduiValue.Text "gap suspected"
                            ])
                    ]
                  DocumentRevision = 1L
                  DataRevision = 9L
                  LastTransportSequence = 3L
                  View = { Values = Map.empty }
                  Poll = RuntimePollState.Backoff(DateTimeOffset.Parse("2026-07-11T00:01:00Z"))
                  LastError = Some { ReasonCode = "delta-timeout"; Message = "retaining last good canvas"; Recoverable = true } }

            let actual = RendererModel.statusPresentation "status" state
            Expect.equal actual.Freshness (TaFreshness.Stale(TimeSpan.FromSeconds 45.0, "source-stopped")) "Stale kind and lag should remain typed."
            Expect.equal actual.Quality (Some "gap suspected") "Quality should remain visible."
            Expect.isTrue (actual.Error |> Option.exists (fun value -> value.Contains "delta-timeout")) "Recoverable error should be presented without dropping data."

        testCase "time labels expose first middle and last visible bar" <| fun _ ->
            Expect.equal (RendererModel.timeLabels [| "B1"; "B2"; "B3"; "B4"; "B5" |]) [| 0, "B1"; 2, "B3"; 4, "B5" |] "Shared time labels should be stable."

        testCase "browser timestamp labels remain compact" <| fun _ ->
            Expect.equal (TaWorkspaceRenderer.compactTimestamp "2026-07-01T03:55:00.0000000+00:00") "07-01 03:55" "TA labels should not expose the full transport timestamp."
            Expect.equal (TaWorkspaceRenderer.compactTimestamp "B1") "B1" "Non-ISO labels should remain unchanged."

        testCase "renderer package remains host neutral" <| fun _ ->
            let assembly = typeof<TaRendererOptions>.Assembly
            let dependencies = assembly.GetReferencedAssemblies() |> Array.map _.Name |> Set.ofArray
            for forbidden in [ "PulseTrade.Comm.Spa"; "FAkka.FCell2"; "PulseTrade.MarketData"; "Microsoft.Data.SqlClient" ] do
                Expect.isFalse (Set.contains forbidden dependencies) ("forbidden renderer dependency: " + forbidden)

        testCase "renderer source contains no JavaScript escape hatch" <| fun _ ->
            let source =
                IO.File.ReadAllText(IO.Path.Combine(__SOURCE_DIRECTORY__, "..", "..", "src", "PulseTrade.Comm.Spa.Dynamic.Renderer", "Renderer.fs"))

            for forbidden in [ "JS.Inline"; "JavaScriptExport"; "<script"; "eval(" ] do
                Expect.isFalse (source.Contains(forbidden, StringComparison.Ordinal)) ("forbidden source escape hatch: " + forbidden)
    ]

[<EntryPoint>]
let main args = runTestsWithCLIArgs [] args tests
