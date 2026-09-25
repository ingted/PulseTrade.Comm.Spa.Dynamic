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

let temporalPoint sourceIntervalId scale startUtc endUtc observedThroughUtc availableAt finality projection quality value =
    TemporalPointCodec.encode
        { SourceIntervalId = sourceIntervalId
          ScaleKey = scale
          IntervalStartUtc = DateTimeOffset.Parse startUtc
          IntervalEndUtc = DateTimeOffset.Parse endUtc
          ObservedThroughUtc = DateTimeOffset.Parse observedThroughUtc
          AvailableAtUtc = availableAt |> Option.map DateTimeOffset.Parse
          Finality = finality
          Projection = projection
          Quality = quality
          Value = value }

let tests =
    testList "TA renderer model" [
        testCase "DYN-T-602 projection commit gate rejects stale superseded and duplicate candidates" <| fun _ ->
            let identity =
                { DocumentId = DocumentId "projection-commit"
                  CanvasInstanceId = CanvasInstanceId "projection-canvas" }
            let state revision =
                { RuntimeReducer.initial identity with
                    DocumentRevision = 1L
                    DataRevision = revision
                    LastTransportSequence = revision }

            let revision1 = state 1L
            let revision2 = state 2L
            let gate1, generation1 = ProjectionCommitGate.beginCandidate revision1 ProjectionCommitGate.initial
            let gate2, generation2 = ProjectionCommitGate.beginCandidate revision2 gate1

            Expect.equal
                (ProjectionCommitGate.pendingGenerationFor revision2 gate2)
                (Some generation2)
                "the newest candidate must own the pending generation"

            let staleGate, staleCommit = ProjectionCommitGate.tryCommit revision2 generation1 revision1 gate2
            Expect.isNone staleCommit "a superseded candidate must not commit"
            Expect.equal staleGate gate2 "a stale completion must not mutate the active gate"

            let mismatchGate, mismatchCommit = ProjectionCommitGate.tryCommit revision1 generation2 revision2 gate2
            Expect.isNone mismatchCommit "a candidate that is no longer the current runtime state must not commit"
            Expect.equal mismatchGate gate2 "a current-state mismatch must preserve the pending candidate"

            let committedGate, committed = ProjectionCommitGate.tryCommit revision2 generation2 revision2 gate2
            Expect.equal committed (Some revision2) "the current candidate must commit exactly once"
            Expect.isNone committedGate.Pending "a successful commit must clear the pending candidate"

            let duplicateGate, duplicateGeneration = ProjectionCommitGate.beginCandidate revision2 committedGate
            let duplicateCommittedGate, duplicateCommit =
                ProjectionCommitGate.tryCommit revision2 duplicateGeneration revision2 duplicateGate
            Expect.isNone duplicateCommit "the same projected revision must not publish a duplicate receipt"
            Expect.equal
                duplicateCommittedGate.LastCommitted
                committedGate.LastCommitted
                "deduplication must preserve the committed watermark"

        testCase "visible K-bar domain ignores remote extrema and keeps fixed CSS padding after resize" <| fun _ ->
            let fullSourceBounds = Some(5.0, 104.0)
            let visibleBounds = Some(100.0, 104.0)
            let assertPadding chartPixelHeight =
                let low, high =
                    RendererModel.paddedBoundsForCssPixels
                        0.0
                        1.0
                        250.0
                        250.0
                        chartPixelHeight
                        15.0
                        visibleBounds
                let highY = RendererModel.normalize low high 0.0 250.0 104.0
                let lowY = RendererModel.normalize low high 0.0 250.0 100.0
                let scale = chartPixelHeight / 250.0
                let topPaddingFromSvgEdge = highY * scale
                let bottomPaddingFromSvgEdge = (250.0 - lowY) * scale
                Expect.floatClose Accuracy.high topPaddingFromSvgEdge 15.0 "top padding is measured from the SVG edge"
                Expect.floatClose Accuracy.high bottomPaddingFromSvgEdge 15.0 "bottom padding is measured from the SVG edge"
                low, high

            let defaultLow, _ = assertPadding 250.0
            let resizedLow, _ = assertPadding 720.0
            let fullSourceLow, _ =
                RendererModel.paddedBoundsForCssPixels 0.0 1.0 250.0 250.0 250.0 15.0 fullSourceBounds
            Expect.isGreaterThan defaultLow 90.0 "the visible-domain result must exclude the remote source low"
            Expect.isLessThan fullSourceLow 5.0 "the control result confirms that including the full source would compress the viewport"
            Expect.isGreaterThan resizedLow defaultLow "resizing must reduce data padding while preserving CSS padding"

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

        testCase "same-position preview updates preserve chart topology while appended bars rebuild it" <| fun _ ->
            let identity =
                { DocumentId = DocumentId "live-preview"
                  CanvasInstanceId = CanvasInstanceId "live-preview-canvas" }
            let row =
                { RowId = "price"
                  Kind = TaRowKind.Candlestick
                  DataRef = "price"
                  HeightWeight = 1.0
                  Visible = true
                  Traces = [||]
                  Options = Map.empty }
            let document =
                { WorkspaceId = "live-preview"
                  Title = "Live preview"
                  RowsRef = "rows"
                  StatusRef = "status"
                  SharedTimeAxis = true
                  TemporalAxisRefs = [||]
                  BaseRowId = Some "price"
                  Rows = [| row |]
                  EditorSchemas = [||]
                  AllowedActions = [||]
                  DefaultView = Map.empty }
            let state data dataRevision =
                { RuntimeReducer.initial identity with
                    Document = Some document
                    Data = Map [ "price", SduiValue.Array data ]
                    DocumentRevision = 1L
                    DataRevision = dataRevision
                    LastTransportSequence = dataRevision }
            let initial =
                state
                    [| candle "B1" 10.0 12.0 9.0 11.0 100.0
                       candle "B2" 11.0 13.0 10.0 12.0 120.0 |]
                    1L
            let revisedPreview =
                state
                    [| candle "B1" 10.0 12.0 9.0 11.0 100.0
                       candle "B2" 11.0 14.0 10.0 13.5 135.0 |]
                    2L
            let appended =
                state
                    [| candle "B1" 10.0 12.0 9.0 11.0 100.0
                       candle "B2" 11.0 14.0 10.0 13.5 135.0
                       candle "B3" 13.5 15.0 13.0 14.5 80.0 |]
                    3L

            Expect.isTrue
                (TaWorkspaceRenderer.sameChartTopology initial revisedPreview)
                "replacing the current bar at the same timestamp must preserve the mounted SVG topology"
            Expect.isFalse
                (TaWorkspaceRenderer.sameChartTopology revisedPreview appended)
                "appending a new timestamp must rebuild the visible chart topology"

        testCase "cache rehydrate data reference triggers repaint without revision inflation" <| fun _ ->
            let identity =
                { DocumentId = DocumentId "cache-repaint"
                  CanvasInstanceId = CanvasInstanceId "cache-repaint-canvas" }
            let initial =
                { RuntimeReducer.initial identity with
                    Data = Map [ "price", SduiValue.Array [||] ]
                    DataRevision = 7L }
            let pollOnly = { initial with Poll = RuntimePollState.Ready }
            let rehydrated =
                { initial with
                    Data =
                        Map [
                            "price",
                            SduiValue.Array [| candle "B1" 10.0 12.0 9.0 11.0 100.0 |]
                        ] }

            Expect.isFalse
                (TaWorkspaceRenderer.runtimeDataChanged initial pollOnly)
                "poll lifecycle changes must not prepare or repaint unchanged chart data"
            Expect.isTrue
                (TaWorkspaceRenderer.runtimeDataChanged initial rehydrated)
                "cache rehydrate must repaint when Data changes while identity and authoritative revision stay fixed"
            Expect.equal rehydrated.DataRevision initial.DataRevision "browser cache must not invent an authoritative revision"

        testCase "legend lookup is qualified by row and row-local trace index" <| fun _ ->
            let readers =
                Map [
                    "price", [| (fun _ -> Some "6025.50") |]
                    "dmi", [| (fun _ -> Some "17.25"); (fun _ -> Some "21.75") |]
                ]

            Expect.equal
                (TaWorkspaceRenderer.tryLegendValue readers "dmi" 0 12)
                (Some "17.25")
                "DMI index zero must not reuse price row index zero"
            Expect.equal
                (TaWorkspaceRenderer.tryLegendValue readers "dmi" 1 12)
                (Some "21.75")
                "each row-local trace index must resolve within its own reader collection"
            Expect.equal
                (TaWorkspaceRenderer.tryLegendValue readers "missing" 0 12)
                None
                "unknown rows fail closed instead of falling back to another row"

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

        testCase "query viewport uses temporal intervals preserves gaps and ignores stale replies" <| fun _ ->
            let row =
                { RowId = "base"
                  Kind = TaRowKind.Sma
                  DataRef = "base"
                  HeightWeight = 1.0
                  Visible = true
                  Options = Map.empty
                  Traces = [||] }
            let document =
                { WorkspaceId = "query-window"
                  Title = "Query viewport"
                  RowsRef = "rows"
                  StatusRef = "status"
                  SharedTimeAxis = true
                  TemporalAxisRefs = [||]
                  BaseRowId = Some "base"
                  Rows = [| row |]
                  EditorSchemas = [||]
                  AllowedActions = [| "change-query" |]
                  DefaultView = Map.empty }
            let point minute =
                let startUtc = sprintf "2026-09-22T00:%02d:00Z" minute
                let endUtc = sprintf "2026-09-22T00:%02d:00Z" (minute + 1)
                temporalPoint
                    ("query:" + string minute)
                    "1K"
                    startUtc
                    endUtc
                    endUtc
                    (Some endUtc)
                    PointFinality.Final
                    TemporalProjection.CandleSpan
                    (Some "complete")
                    (Some(SduiValue.Object(Map [ "v", SduiValue.Number(float minute) ])))
            let dataFor minutes =
                Map [ "base", SduiValue.Array(minutes |> Array.map point) ]
            let query fromUtc toUtc : TaQueryChange =
                { SourceId = None
                  Instrument = None
                  IntervalMinutes = None
                  FromUtc = Some fromUtc
                  ToUtcExclusive = Some toUtc
                  IncludePartial = Some true }
            let loaded = dataFor [| 0; 1; 4; 6 |]

            Expect.equal
                (RendererModel.queryViewportSelection 4 4 (query "2026-09-22T00:00:30Z" "2026-09-22T00:02:00Z") document loaded)
                (TaQueryViewportSelection.Selected { StartIndex = 0; Count = 2 })
                "selection begins where interval end is after From and stops where interval start reaches exclusive To"
            Expect.equal
                (RendererModel.queryViewportSelection 4 4 (query "2026-09-22T00:02:30Z" "2026-09-22T00:03:30Z") document loaded)
                TaQueryViewportSelection.NoIntersection
                "a gap-only range must not invent observations"
            Expect.equal
                (RendererModel.queryViewportSelection 4 4 (query "2026-09-22T00:00:00Z" "2026-09-22T00:07:00Z") document loaded)
                (TaQueryViewportSelection.Selected { StartIndex = 0; Count = 4 })
                "the full loaded range maps to the full local window"
            Expect.equal
                (RendererModel.queryViewportSelection 5 4 (query "2026-09-22T00:00:00Z" "2026-09-22T00:07:00Z") document loaded)
                TaQueryViewportSelection.Stale
                "an older reply cannot override a newer query generation"
            Expect.equal
                (RendererModel.queryViewportSelection 4 4 (query "2026-09-22T00:07:00Z" "2026-09-22T00:06:00Z") document loaded)
                (TaQueryViewportSelection.Invalid "query-range-invalid: FromUtc must be earlier than ToUtcExclusive.")
                "invalid half-open bounds are explicit"
            Expect.equal
                (RendererModel.queryViewportSelection 4 4 (query "2026-02-31" "2026-03-02") document loaded)
                (TaQueryViewportSelection.Invalid "query-range-invalid: FromUtc and ToUtcExclusive must be valid UTC timestamps.")
                "invalid calendar dates are rejected"
            Expect.equal
                (RendererModel.queryViewportSelection 4 4 (query "2026-09-22T00:04:00Z" "2026-09-22T00:05:00Z") document (dataFor [| 0; 1 |]))
                TaQueryViewportSelection.NoIntersection
                "an unloaded range is not selected before its patch is merged"
            Expect.equal
                (RendererModel.queryViewportSelection 4 4 (query "2026-09-22T00:04:00Z" "2026-09-22T00:05:00Z") document (dataFor [| 0; 1; 4 |]))
                (TaQueryViewportSelection.Selected { StartIndex = 2; Count = 1 })
                "the same query selects after the bounded patch is merged"

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

        testCase "navigator draft clamps without changing committed window until release" <| fun _ ->
            let committed = { StartIndex = 1952; Count = 48 }
            let preview = RendererModel.previewWindow 2000 committed -25
            let followLatestAtHead, committedAtHead = RendererModel.commitPreview 2000 committed -25
            let followLatestAtTail, committedAtTail = RendererModel.commitPreview 2000 committed 9999

            Expect.equal committed { StartIndex = 1952; Count = 48 } "preview must not mutate the committed value"
            Expect.equal preview { StartIndex = 0; Count = 48 } "preview clamps to the loaded-range head"
            Expect.equal committedAtHead preview "release commits the same clamped preview window"
            Expect.isFalse followLatestAtHead "historical release leaves follow-latest mode"
            Expect.equal committedAtTail committed "tail release clamps to the maximum start"
            Expect.isTrue followLatestAtTail "tail release restores follow-latest mode"

        testCase "dual-bound navigator previews move and both resize handles" <| fun _ ->
            let committed = { StartIndex = 1952; Count = 48 }
            let moved = RendererModel.previewWindowBounds 12 2000 2000 committed TaWindowDrag.Move -952
            let left = RendererModel.previewWindowBounds 12 2000 2000 committed TaWindowDrag.ResizeLeft -152
            let right = RendererModel.previewWindowBounds 12 2000 2000 committed TaWindowDrag.ResizeRight -20
            let full = RendererModel.previewWindowBounds 12 2000 2000 committed TaWindowDrag.ResizeLeft -9999
            let followLatest, committedFull = RendererModel.commitWindowBounds 12 2000 2000 full

            Expect.equal moved { StartIndex = 1000; Count = 48 } "selection move preserves width"
            Expect.equal left { StartIndex = 1800; Count = 200 } "left handle expands toward history"
            Expect.equal right { StartIndex = 1952; Count = 28 } "right handle shrinks at the tail"
            Expect.equal committedFull { StartIndex = 0; Count = 2000 } "left handle can expose the complete loaded range"
            Expect.isTrue followLatest "a full-range window still ends at the loaded tail"
            Expect.equal (RendererModel.selectionRatios 2000 committedFull) (0.0, 1.0) "full range occupies the complete overview"

        testCase "overview sampling is deterministic and bounded" <| fun _ ->
            let values = [| 0 .. 1999 |]
            let sampled = RendererModel.sampleEvenly 250 values
            Expect.equal sampled.Length 250 "overview must not create one visual item per loaded bar"
            Expect.equal sampled[0] 0 "sampling preserves the loaded head"
            Expect.equal sampled[sampled.Length - 1] 1999 "sampling preserves the loaded tail"
            Expect.sequenceEqual (RendererModel.sampleEvenly 8 [| 1; 2; 3 |]) [| 1; 2; 3 |] "short inputs remain exact"

        testCase "projected line compaction preserves ordered bucket extrema without group allocations" <| fun _ ->
            let point index value =
                index,
                { Timestamp = "T" + string index
                  Value = value
                  Temporal = None }
            let values =
                [| point 0 4.0
                   point 1 1.0
                   point 2 9.0
                   point 3 3.0
                   point 4 8.0
                   point 5 2.0
                   point 6 7.0
                   point 7 5.0 |]

            let compacted = RendererModel.compactProjectedLinePoints 4 8 values
            Expect.sequenceEqual
                (compacted |> Array.map (fun (slotIndex, point) -> slotIndex, point.Value))
                [| 1, 1.0; 2, 9.0; 4, 8.0; 5, 2.0 |]
                "each bucket must retain its first min/max points in chronological order"
            Expect.sequenceEqual
                (RendererModel.compactProjectedLinePoints 8 8 values)
                values
                "a viewport within the visual budget must remain exact"

        testCase "pointer ratio maps deterministically to a visible bar" <| fun _ ->
            Expect.equal (RendererModel.cursorIndexFromRatio 48 0.0) (Some 0) "left edge should select the first visible bar"
            Expect.equal (RendererModel.cursorIndexFromRatio 48 0.5) (Some 24) "middle should select the nearest visible bar"
            Expect.equal (RendererModel.cursorIndexFromRatio 48 1.0) (Some 47) "right edge should select the last visible bar"
            Expect.equal (RendererModel.cursorIndexFromClientX 48 100.0 800.0 300.0) (Some 12) "client coordinates should be normalized by the actual row width"
            Expect.equal (RendererModel.cursorIndexFromRatio 0 0.5) None "empty series has no cursor index"
            Expect.equal (RendererModel.cursorIndexFromClientX 48 0.0 0.0 10.0) None "zero-width row cannot be hit-tested"

        testCase "cursor, line and candle use the same slot centers" <| fun _ ->
            Expect.equal (RendererModel.slotCenter 1000.0 48 0) (Some(1000.0 / 96.0)) "first cursor is centered in the first candle slot"
            Expect.equal (RendererModel.slotCenter 1000.0 48 24) (Some(1000.0 / 48.0 * 24.5)) "middle cursor shares the line and candle center"
            Expect.equal (RendererModel.slotCenter 1000.0 48 47) (Some(1000.0 / 48.0 * 47.5)) "last cursor remains half a slot inside the plot"
            Expect.equal (RendererModel.slotCenter 1000.0 0 0) None "empty plots have no slot center"

        testCase "select window is deterministic" <| fun _ ->
            let actual = RendererModel.selectWindow { StartIndex = 2; Count = 3 } [| 0; 1; 2; 3; 4; 5 |]
            Expect.sequenceEqual actual [| 2; 3; 4 |] "window must preserve ordering"

            let beyondShortWarmup = RendererModel.selectWindow { StartIndex = 1952; Count = 48 } [| 0 .. 1767 |]
            Expect.isEmpty beyondShortWarmup "a short warm-up trace must not throw when the canonical viewport starts after its last point"

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
                  TemporalAxisRefs = [||]
                  BaseRowId = Some "price"
                  Rows = [| row "price" TaRowKind.Candlestick "price"; row "sma" TaRowKind.Sma "sma" |]
                  EditorSchemas = [||]
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

        testCase "shared cursor aligns short warm-up traces by timestamp rather than local index" <| fun _ ->
            let line timestamp value =
                SduiValue.Object(Map [ "t", SduiValue.Text timestamp; "v", SduiValue.Number value ])

            let trace traceId kind dataRef =
                { TraceId = traceId
                  Kind = kind
                  DataRef = dataRef
                  Label = traceId
                  Color = ""
                  Width = 1.0
                  Visible = true
                  CandleDataRefs = None
                  Options = Map.empty }

            let row =
                { RowId = "price"
                  Kind = TaRowKind.Candlestick
                  DataRef = "price"
                  HeightWeight = 1.0
                  Visible = true
                  Traces =
                    [| trace "price" TaTraceKind.Candlestick "price"
                       trace "sma233" TaTraceKind.Line "sma233" |]
                  Options = Map.empty }

            let document =
                { WorkspaceId = "warm-up-test"
                  Title = "Warm-up"
                  RowsRef = "rows"
                  StatusRef = "status"
                  SharedTimeAxis = true
                  TemporalAxisRefs = [||]
                  BaseRowId = Some "price"
                  Rows = [| row |]
                  EditorSchemas = [||]
                  AllowedActions = [||]
                  DefaultView = Map.empty }

            let data =
                Map [
                    "price",
                    SduiValue.Array
                        [| candle "B1" 10.0 12.0 9.0 11.0 100.0
                           candle "B2" 11.0 13.0 10.0 12.0 120.0
                           candle "B3" 12.0 14.0 11.0 13.0 130.0 |]
                    "sma233", SduiValue.Array [| line "B2" 11.5; line "B3" 12.5 |]
                ]

            let timeline = RendererModel.referenceTimeline document.Rows data
            Expect.sequenceEqual timeline [| "B1"; "B2"; "B3" |] "candles define the canonical viewport timeline"

            let first = RendererModel.cursorSnapshot document data { StartIndex = 0; Count = 3 } 0 |> Option.defaultWith (fun () -> failwith "first cursor missing")
            Expect.equal first.Timestamp "B1" "the first canonical bar remains addressable"
            Expect.equal (first.Values |> Array.map _.Label) [| "price" |] "an indicator without a warm-up value stays absent instead of shifting B2 onto B1"

            let last = RendererModel.cursorSnapshot document data { StartIndex = 0; Count = 3 } 2 |> Option.defaultWith (fun () -> failwith "last cursor missing")
            Expect.equal last.Timestamp "B3" "the cursor targets the canonical timestamp"
            Expect.equal (last.Values |> Array.map _.Label) [| "price"; "sma233" |] "the warmed-up indicator joins at its exact timestamp"
            Expect.equal last.Values[1].Value "12.5" "the indicator value must not be offset by its shorter history"

        testCase "explicit base row controls event-time axis and visible range" <| fun _ ->
            let line timestamp value =
                SduiValue.Object(Map [ "t", SduiValue.Text timestamp; "v", SduiValue.Number value ])
            let row rowId dataRef =
                { RowId = rowId
                  Kind = TaRowKind.Sma
                  DataRef = dataRef
                  HeightWeight = 1.0
                  Visible = true
                  Options = Map.empty
                  Traces = [||] }
            let baseTimes =
                [| "2026-09-08T01:00:00Z"
                   "2026-09-08T01:01:00Z"
                   "2026-09-08T01:02:00Z" |]
            let document =
                { WorkspaceId = "base-row-test"
                  Title = "Base row"
                  RowsRef = "rows"
                  StatusRef = "status"
                  SharedTimeAxis = true
                  TemporalAxisRefs = [||]
                  BaseRowId = Some "base"
                  Rows = [| row "base" "base"; row "longer" "longer" |]
                  EditorSchemas = [||]
                  AllowedActions = [| "shared-cursor-changed"; "visible-range-changed" |]
                  DefaultView = Map.empty }
            let data =
                Map [
                    "base", SduiValue.Array(baseTimes |> Array.mapi (fun index timestamp -> line timestamp (float index)))
                    "longer",
                    SduiValue.Array
                        [| line "2026-09-08T00:59:00Z" 0.0
                           line "2026-09-08T01:00:00Z" 1.0
                           line "2026-09-08T01:01:00Z" 2.0
                           line "2026-09-08T01:02:00Z" 3.0
                           line "2026-09-08T01:03:00Z" 4.0 |] ]

            Expect.sequenceEqual (RendererModel.referenceTimelineForDocument document data) baseTimes "Host-selected base row must win over a longer visible series."
            let range =
                RendererModel.visibleEventRange document data { StartIndex = 0; Count = 2 }
                |> Option.defaultWith (fun () -> failtest "Visible event range missing.")
            Expect.equal range.BaseRowId "base" "Range must retain the explicit base identity."
            Expect.equal range.StartEventTimeUtc baseTimes[0] "Range starts at the first visible base datapoint."
            Expect.equal range.EndEventTimeExclusiveUtc baseTimes[2] "Range ends at the next base datapoint."

            let cursor = RendererModel.cursorSnapshot document data { StartIndex = 0; Count = 3 } 1 |> Option.get
            Expect.equal cursor.Timestamp baseTimes[1] "Cursor must land on a real base datapoint."

        testCase "coarse cursor values require finalized event-time evidence" <| fun _ ->
            let temporalLine sourceId scale startTime endTime availableAt finality projection value =
                SduiValue.Object(
                    Map [
                        "_type", SduiValue.Text "temporal-point.v1"
                        "sourceIntervalId", SduiValue.Text sourceId
                        "scaleKey", SduiValue.Text scale
                        "intervalStartUtc", SduiValue.Text startTime
                        "intervalEndUtc", SduiValue.Text endTime
                        "observedThroughUtc", SduiValue.Text endTime
                        "availableAtUtc", SduiValue.Text availableAt
                        "finality", SduiValue.Text finality
                        "projection", SduiValue.Text projection
                        "value", SduiValue.Object(Map [ "t", SduiValue.Text startTime; "v", SduiValue.Number value ]) ])
            let row rowId dataRef =
                { RowId = rowId
                  Kind = TaRowKind.Sma
                  DataRef = dataRef
                  HeightWeight = 1.0
                  Visible = true
                  Options = Map.empty
                  Traces = [||] }
            let document =
                { WorkspaceId = "finality-test"
                  Title = "Finality"
                  RowsRef = "rows"
                  StatusRef = "status"
                  SharedTimeAxis = true
                  TemporalAxisRefs = [||]
                  BaseRowId = Some "base"
                  Rows = [| row "base" "base"; row "coarse" "coarse" |]
                  EditorSchemas = [||]
                  AllowedActions = [||]
                  DefaultView = Map.empty }
            let cursorTime = "2026-09-08T01:04:00Z"
            let basePreview =
                temporalLine "base-0104" "1k" cursorTime "2026-09-08T01:05:00Z" "2026-09-08T01:04:30Z" "preview" "candle-span" 104.0
            let priorFinal =
                temporalLine "coarse-0055" "5k" "2026-09-08T00:55:00Z" "2026-09-08T01:00:00Z" "2026-09-08T01:00:00Z" "final" "candle-span" 10.0
            let currentPreview =
                temporalLine "coarse-0100-preview" "5k" "2026-09-08T01:00:00Z" "2026-09-08T01:05:00Z" "2026-09-08T01:04:30Z" "preview" "candle-span" 99.0
            let currentFinal =
                temporalLine "coarse-0100-final" "5k" "2026-09-08T01:00:00Z" "2026-09-08T01:05:00Z" "2026-09-08T01:05:00Z" "final" "candle-span" 20.0
            let snapshot coarse =
                RendererModel.cursorSnapshot
                    document
                    (Map [ "base", SduiValue.Array [| basePreview |]; "coarse", SduiValue.Array coarse ])
                    { StartIndex = 0; Count = 1 }
                    0
                |> Option.defaultWith (fun () -> failtest "cursor snapshot missing")

            let fallback = snapshot [| priorFinal; currentPreview |]
            Expect.stringStarts fallback.Values[0].Value "104" "base row may expose its current preview datapoint."
            Expect.stringStarts fallback.Values[1].Value "10" "unfinished coarse data must fall back to the latest available finalized value."

            let containing = snapshot [| priorFinal; currentPreview; currentFinal |]
            Expect.stringStarts containing.Values[1].Value "20" "a finalized coarse interval containing base event-time wins over as-of fallback."

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

        testCase "authored row label is the shared card and toolbar display name" <| fun _ ->
            let trace =
                { TraceId = "es-1k"
                  Kind = TaTraceKind.Candlestick
                  DataRef = "es-1k"
                  Label = "ES_1K / 1K"
                  Color = ""
                  Width = 1.0
                  Visible = true
                  CandleDataRefs = None
                  Options = Map.empty }
            let row =
                { RowId = "price-1k"
                  Kind = TaRowKind.Candlestick
                  DataRef = trace.DataRef
                  HeightWeight = 1.0
                  Visible = true
                  Options = Map [ "label", SduiValue.Text "ES 1K + SMA(20)" ]
                  Traces = [| trace |] }

            Expect.equal (TaWorkspaceRenderer.rowDisplayLabel row) "ES 1K + SMA(20)" "Toolbar should use the authored TA_ROW label."
            Expect.equal (TaWorkspaceRenderer.rowTitle row row.Traces) "ES 1K + SMA(20)" "Row card should use the same authored TA_ROW label."
            let unlabeled = { row with Options = Map.empty }
            Expect.equal (TaWorkspaceRenderer.rowDisplayLabel unlabeled) "Candlestick" "Toolbar fallback should remain the typed row kind."
            Expect.equal (TaWorkspaceRenderer.rowTitle unlabeled unlabeled.Traces) "ES_1K / 1K" "Card fallback should retain the trace label."

        testCase "document shell cache key includes runtime document and canvas identity" <| fun _ ->
            let first =
                RuntimeReducer.initial
                    { DocumentId = DocumentId "run-29"
                      CanvasInstanceId = CanvasInstanceId "canvas-29" }
            let same = RuntimeReducer.initial first.Identity
            let replacement =
                RuntimeReducer.initial
                    { DocumentId = DocumentId "run-30"
                      CanvasInstanceId = CanvasInstanceId "canvas-30" }

            Expect.isTrue (TaWorkspaceRenderer.sameDocumentShell first same) "The same runtime identity and revision should reuse its shell."
            Expect.isFalse (TaWorkspaceRenderer.sameDocumentShell first replacement) "A replacement run must rebuild the shell even when both revisions start at zero."
            Expect.isFalse (TaWorkspaceRenderer.sameDocumentShell first { same with DocumentRevision = 1L }) "A newer document revision must rebuild the shell."

        testCase "latest row legend falls back to the last available presentation slot" <| fun _ ->
            let read index =
                if index = 3 then
                    Some
                        { Timestamp = "2026-09-03T13:03:00Z"
                          Value = "101.25" }
                else
                    None
            Expect.equal
                (TaWorkspaceRenderer.tryReadAtOrBefore read 4 |> Option.map _.Value)
                (Some "101.25")
                "No-cursor legend state must retain the latest available value when the final presentation slot is sparse."
            Expect.isNone (read 4) "Exact cursor reads must remain sparse instead of silently moving to another slot."

        testCase "incremental temporal null retains an explicit projected source slot" <| fun _ ->
            let axisRef = "axis.legend-null"
            let axis =
                { AxisRef = axisRef
                  Revision = 1L
                  Points =
                    [| for position in 0L .. 2L do
                           let startUtc = DateTimeOffset(2026, 9, 25, 1, int position, 0, TimeSpan.Zero)
                           yield
                               { Position = position
                                 SourceIntervalId = $"legend-null:{position}"
                                 ScaleKey = "1K"
                                 IntervalStartUtc = startUtc
                                 IntervalEndUtc = startUtc.AddMinutes 1.0
                                 EventTimeUtc = Some(startUtc.AddMinutes 1.0)
                                 ObservedThroughUtc = startUtc.AddMinutes 1.0
                                 AvailableAtUtc = Some(startUtc.AddMinutes 1.0)
                                 Finality = PointFinality.Final
                                 Projection = TemporalProjection.CandleSpan
                                 Quality = Some "complete" } |] }
            let series values =
                { AxisRef = axisRef
                  AxisRevision = 1L
                  Points = values |> Array.mapi (fun index value -> { Position = int64 index; Value = value }) }
            let data values =
                Map [ axisRef, TemporalAxisCodec.encode axis; "series.sma", TemporalSeriesCodec.encode (series values) ]
            let initial = RendererModel.prepareData (data [| SduiValue.Number 10.0; SduiValue.Number 11.0; SduiValue.Number 12.0 |])
            let revised = RendererModel.prepareDataIncremental initial (data [| SduiValue.Number 10.0; SduiValue.Number 11.0; SduiValue.Null |])
            let reference = axis.Points |> Array.map (fun point -> point.EventTimeUtc.Value.ToString("O"))
            let projected = RendererModel.projectedSourceTimestamps reference "series.sma" revised
            let projectedUnavailable =
                RendererModel.projectedLastSourceTimestampWhere
                    (fun point -> RendererModel.parseLineResolved point.Temporal point.Payload |> Option.isNone)
                    reference
                    "series.sma"
                    revised
            Expect.equal projected[1] (Some reference[2]) "The explicit null point must retain its source interval at the projected slot."
            Expect.equal projectedUnavailable[1] (Some reference[2]) "The bounded last-source path must retain the explicit null slot without rescanning the series."
            let revisedSeries = revised.ResolvedSeries |> Map.find "series.sma"
            Expect.equal revisedSeries[2].Payload (Some SduiValue.Null) "Incremental preparation must preserve the explicit null payload."

        testCase "multi-scale temporal projection aligns candle spans repeated lines and causal step values" <| fun _ ->
            let timestamps =
                [| for minute in 0 .. 9 -> sprintf "2026-09-03T13:%02d:00.0000000+00:00" minute |]
            let baseCandles =
                timestamps
                |> Array.mapi (fun index timestamp -> candle timestamp (100.0 + float index) (102.0 + float index) (99.0 + float index) (101.0 + float index) 20.0)
            let coarseLine =
                [| temporalPoint
                       "es-5k:1300"
                       "5K"
                       "2026-09-03T13:00:00Z"
                       "2026-09-03T13:05:00Z"
                       "2026-09-03T13:05:00Z"
                       (Some "2026-09-03T13:05:00Z")
                       PointFinality.Final
                       TemporalProjection.RepeatAcrossBaseBuckets
                       (Some "complete")
                       (Some(SduiValue.Object(Map [ "v", SduiValue.Number 10.0 ])))
                   temporalPoint
                       "es-5k:1305"
                       "5K"
                       "2026-09-03T13:05:00Z"
                       "2026-09-03T13:10:00Z"
                       "2026-09-03T13:10:00Z"
                       (Some "2026-09-03T13:10:00Z")
                       PointFinality.Final
                       TemporalProjection.RepeatAcrossBaseBuckets
                       (Some "complete")
                       (Some(SduiValue.Object(Map [ "v", SduiValue.Number 20.0 ]))) |]
            let coarseCandle =
                temporalPoint
                    "es-5k:1300"
                    "5K"
                    "2026-09-03T13:00:00Z"
                    "2026-09-03T13:05:00Z"
                    "2026-09-03T13:05:00Z"
                    (Some "2026-09-03T13:05:00Z")
                    PointFinality.Final
                    TemporalProjection.CandleSpan
                    (Some "complete")
                    (Some(SduiValue.Object(Map [ "o", SduiValue.Number 100.0; "h", SduiValue.Number 110.0; "l", SduiValue.Number 95.0; "c", SduiValue.Number 108.0; "v", SduiValue.Number 90.0 ])) )
            let stepLine =
                temporalPoint
                    "es-5k:1300-step"
                    "5K"
                    "2026-09-03T13:00:00Z"
                    "2026-09-03T13:05:00Z"
                    "2026-09-03T13:05:00Z"
                    (Some "2026-09-03T13:05:00Z")
                    PointFinality.Final
                    TemporalProjection.StepAfterClose
                    (Some "complete")
                    (Some(SduiValue.Object(Map [ "v", SduiValue.Number 33.0 ])))
            let laterStepLine =
                temporalPoint
                    "es-5k:1305-step"
                    "5K"
                    "2026-09-03T13:05:00Z"
                    "2026-09-03T13:10:00Z"
                    "2026-09-03T13:08:00Z"
                    (Some "2026-09-03T13:08:00Z")
                    PointFinality.Final
                    TemporalProjection.StepAfterClose
                    (Some "complete")
                    (Some(SduiValue.Object(Map [ "v", SduiValue.Number 44.0 ])))
            let trace traceId kind dataRef label =
                { TraceId = traceId; Kind = kind; DataRef = dataRef; Label = label; Color = ""; Width = 1.0; Visible = true; CandleDataRefs = None; Options = Map.empty }
            let row =
                { RowId = "multi-scale"
                  Kind = TaRowKind.Candlestick
                  DataRef = "series.base"
                  HeightWeight = 1.0
                  Visible = true
                  Options = Map.empty
                  Traces =
                    [| trace "base" TaTraceKind.Candlestick "series.base" "1K"
                       trace "coarse-candle" TaTraceKind.Candlestick "series.coarse-candle" "5K candle"
                       trace "coarse-line" TaTraceKind.Line "series.coarse-line" "5K SMA"
                       trace "step-line" TaTraceKind.Line "series.step-line" "5K causal step" |] }
            let data =
                Map [
                    "series.base", SduiValue.Array baseCandles
                    "series.coarse-candle", SduiValue.Array [| coarseCandle |]
                    "series.coarse-line", SduiValue.Array coarseLine
                    "series.step-line", SduiValue.Array [| stepLine; laterStepLine |]
                ]

            Expect.sequenceEqual (RendererModel.referenceTimeline [| row |] data) timestamps "The longest real timestamp series is the base axis."
            let parsedCandle = RendererModel.candleSeries "series.coarse-candle" data |> Array.exactlyOne
            Expect.equal (RendererModel.candleSlotRange timestamps parsedCandle) (Some(0, 5)) "A 5K source candle spans five real 1K slots without cloning source records."
            let projectedCandles = RendererModel.projectedCandleSlots timestamps parsedCandle
            Expect.equal projectedCandles.Length 5 "A 5K source candle must render once at each real constituent 1K slot."
            Expect.sequenceEqual (projectedCandles |> Array.map (fun (index, _, _) -> index)) [| 0; 1; 2; 3; 4 |] "Projected candles retain the actual base-axis slot positions."
            Expect.isTrue (projectedCandles |> Array.forall (fun (_, span, point) -> span = 5 && (point.Temporal |> Option.exists (fun value -> value.SourceIntervalId = "es-5k:1300")))) "Every projected candle retains its canonical source interval identity."
            let cursorValues = RendererModel.projectedCandleCursorValues false timestamps [| parsedCandle |]
            let legacyCursorValues = timestamps |> Array.map (fun timestamp -> RendererModel.tryCandleForCursor false timestamp [| parsedCandle |])
            Expect.sequenceEqual cursorValues legacyCursorValues "The linear cursor projection preserves finalized match and as-of fallback semantics."
            let sparseTimestamps = timestamps |> Array.removeAt 2
            let sparseProjected = RendererModel.projectedCandleSlots sparseTimestamps parsedCandle
            Expect.equal sparseProjected.Length 4 "Projection must not invent a missing base-axis slot."
            let repeated = RendererModel.lineSeries "series.coarse-line" data |> RendererModel.projectedLinePoints timestamps
            Expect.equal repeated.Length 10 "Two final 5K values project across ten 1K presentation cells."
            Expect.sequenceEqual (repeated |> Array.map (snd >> _.Value)) [| 10.0; 10.0; 10.0; 10.0; 10.0; 20.0; 20.0; 20.0; 20.0; 20.0 |] "Repeated cells preserve each source interval value."
            let stepped = RendererModel.lineSeries "series.step-line" data |> RendererModel.projectedLinePoints timestamps
            Expect.sequenceEqual (stepped |> Array.map fst) [| 5; 6; 7; 8; 9 |] "Step-after-close remains invisible before owner-provided availability."
            Expect.sequenceEqual (stepped |> Array.map (snd >> _.Value)) [| 33.0; 33.0; 33.0; 44.0; 44.0 |] "A later causal step replaces the prior suffix without expanding every overlapping source range."
            let metadata = RendererModel.rowTemporalMetadata row data
            Expect.isTrue (metadata |> Array.exists (fun value -> value.ScaleKey = "5K" && value.Quality = Some "complete")) "Legend metadata preserves scale and quality."

            let document =
                { WorkspaceId = "multi-scale"
                  Title = "Multi scale"
                  RowsRef = "rows"
                  StatusRef = "status"
                  SharedTimeAxis = true
                  TemporalAxisRefs = [||]
                  BaseRowId = Some "multi-scale"
                  Rows = [| row |]
                  EditorSchemas = [||]
                  AllowedActions = [||]
                  DefaultView = Map.empty }
            let cursor = RendererModel.cursorSnapshot document data { StartIndex = 0; Count = 10 } 3 |> Option.defaultWith (fun () -> failwith "cursor missing")
            Expect.isTrue (cursor.Values |> Array.exists (fun value -> value.Value.Contains("5K final | es-5k:1300"))) "Cursor traces a repeated presentation cell back to its source interval."

        testCase "overlay and separate rows independently render reused data refs" <| fun _ ->
            let timestamps =
                [| "2026-09-03T13:00:00.0000000+00:00"
                   "2026-09-03T13:01:00.0000000+00:00" |]
            let series offset =
                timestamps
                |> Array.mapi (fun index timestamp -> candle timestamp (offset + float index) (offset + float index + 2.0) (offset + float index - 1.0) (offset + float index + 1.0) 20.0)
            let trace traceId dataRef label =
                { TraceId = traceId
                  Kind = TaTraceKind.Candlestick
                  DataRef = dataRef
                  Label = label
                  Color = ""
                  Width = 1.0
                  Visible = true
                  CandleDataRefs = None
                  Options = Map.empty }
            let row rowId dataRef traces =
                { RowId = rowId
                  Kind = TaRowKind.Candlestick
                  DataRef = dataRef
                  HeightWeight = 1.0
                  Visible = true
                  Options = Map.empty
                  Traces = traces }
            let rows =
                [| row "overlay" "series.1k" [| trace "overlay-1k" "series.1k" "Overlay 1K"; trace "overlay-5k" "series.5k" "Overlay 5K" |]
                   row "separate-1k" "series.1k" [| trace "separate-1k" "series.1k" "Separate 1K" |]
                   row "separate-5k" "series.5k" [| trace "separate-5k" "series.5k" "Separate 5K" |] |]
            let document =
                { WorkspaceId = "overlay-and-separate"
                  Title = "Overlay and separate rows"
                  RowsRef = "rows"
                  StatusRef = "status"
                  SharedTimeAxis = true
                  TemporalAxisRefs = [||]
                  BaseRowId = Some "overlay"
                  Rows = rows
                  EditorSchemas = [||]
                  AllowedActions = [||]
                  DefaultView = Map.empty }
            let data = Map [ "series.1k", SduiValue.Array(series 100.0); "series.5k", SduiValue.Array(series 200.0) ]

            Expect.isEmpty (RuntimeValidation.documentErrors DynamicRuntimeDefaults.limits document) "Cross-row DataRef reuse is a valid document contract."
            Expect.sequenceEqual (RendererModel.referenceTimelineForDocument document data) timestamps "The overlay base row owns the shared real-time axis."
            let cursor = RendererModel.cursorSnapshotForRows document rows data { StartIndex = 0; Count = 2 } 1 |> Option.defaultWith (fun () -> failwith "cursor missing")
            Expect.sequenceEqual
                (cursor.Values |> Array.map _.Label)
                [| "Overlay 1K"; "Overlay 5K"; "Separate 1K"; "Separate 5K" |]
                "Overlay and separate rows must resolve the reused immutable series independently."

        testCase "shared temporal axis joins five scalar candle components without filling gaps" <| fun _ ->
            let axisRef = "axis.ha.1k"
            let time minute = DateTimeOffset(2026, 9, 8, 1, minute, 0, TimeSpan.Zero)
            let axisPoint position minute =
                { Position = position
                  SourceIntervalId = $"ha:{minute}"
                  ScaleKey = "1K"
                  IntervalStartUtc = time minute
                  IntervalEndUtc = (time minute).AddMinutes 1.0
                  EventTimeUtc = Some((time minute).AddMinutes 1.0)
                  ObservedThroughUtc = (time minute).AddMinutes 1.0
                  AvailableAtUtc = Some((time minute).AddMinutes 1.0)
                  Finality = PointFinality.Final
                  Projection = TemporalProjection.CandleSpan
                  Quality = Some "complete" }
            let series values =
                { AxisRef = axisRef
                  AxisRevision = 7L
                  Points =
                    values
                    |> Array.mapi (fun index value ->
                        { Position = if index = 0 then 40L else 41L
                          Value = SduiValue.Number value }) }
                |> TemporalSeriesCodec.encode
            let refs =
                { OpenRef = "ha.open"
                  HighRef = "ha.high"
                  LowRef = "ha.low"
                  CloseRef = "ha.close"
                  VolumeRef = "ha.volume" }
            let trace =
                { TraceId = "ha"
                  Kind = TaTraceKind.Candlestick
                  DataRef = refs.OpenRef
                  Label = "Heikin-Ashi 1K"
                  Color = ""
                  Width = 1.0
                  Visible = true
                  CandleDataRefs = Some refs
                  Options = Map.empty }
            let row =
                { RowId = "ha"
                  Kind = TaRowKind.HeikinAshi
                  DataRef = refs.OpenRef
                  HeightWeight = 1.0
                  Visible = true
                  Traces = [| trace |]
                  Options = Map.empty }
            let data =
                Map [ axisRef, TemporalAxisCodec.encode { AxisRef = axisRef; Revision = 7L; Points = [| axisPoint 40L 0; axisPoint 41L 5 |] }
                      refs.OpenRef, series [| 100.0; 105.0 |]
                      refs.HighRef, series [| 110.0; 115.0 |]
                      refs.LowRef, series [| 95.0; 101.0 |]
                      refs.CloseRef, series [| 108.0; 112.0 |]
                      refs.VolumeRef, series [| 900.0; 1200.0 |] ]
            let candles = RendererModel.candleSeriesForTrace trace data
            let prepared = RendererModel.prepareData data
            let preparedCandles = RendererModel.candleSeriesForTracePrepared trace prepared
            Expect.equal candles.Length 2 "Five shared-axis scalar series should synthesize two candles."
            Expect.sequenceEqual preparedCandles candles "Prepared shared-axis candle projection must preserve raw-path semantics."
            Expect.sequenceEqual
                (RendererModel.lineSeriesPrepared refs.CloseRef prepared)
                (RendererModel.lineSeries refs.CloseRef data)
                "Prepared shared-axis line projection must preserve raw-path semantics."
            Expect.equal (candles |> Array.map _.Timestamp) [| "2026-09-08T01:01:00.0000000+00:00"; "2026-09-08T01:06:00.0000000+00:00" |] "Candle presentation must use the owner-authored event time without filling the irregular gap."
            Expect.equal candles[1].Open 105.0 "Open component should join by axis position."
            Expect.equal candles[1].High 115.0 "High component should join by axis position."
            Expect.equal candles[1].Low 101.0 "Low component should join by axis position."
            Expect.equal candles[1].Close 112.0 "Close component should join by axis position."
            Expect.equal candles[1].Volume 1200.0 "Volume component should join by axis position."
            Expect.equal (RendererModel.referenceTimeline [| row |] data).Length 2 "Shared timeline should expose only actual axis positions."
            Expect.equal TaWorkspaceRenderer.defaultOptions.MaximumVisibleBars 4000 "Default renderer viewport must accept the stakeholder 4,000-bar gate."

            Expect.equal
                (RendererModel.initialViewportWindow 12 48 4000 3563 (Map [ "visibleBars", SduiValue.Number 4000.0 ]))
                { StartIndex = 0; Count = 3563 }
                "A fresh renderer must honor document visibleBars and clamp it to the reference length."
            Expect.equal
                (RendererModel.initialViewportWindow 12 48 4000 5000 (Map [ "visibleBars", SduiValue.Number 9000.0 ]))
                { StartIndex = 1000; Count = 4000 }
                "Document visibleBars must remain bounded by the renderer maximum."
            Expect.equal
                (RendererModel.initialViewportWindow 12 48 4000 3563 Map.empty)
                { StartIndex = 3515; Count = 48 }
                "A missing document visibleBars value must retain the renderer fallback."
            Expect.equal
                (RendererModel.initialViewportWindow 12 48 4000 3563 (Map [ "visibleBars", SduiValue.Number 12.5 ]))
                { StartIndex = 3515; Count = 48 }
                "A non-integral document visibleBars value must not corrupt the viewport."

            let missingCloseData = data |> Map.add refs.CloseRef (series [| 108.0 |])
            let missingCloseCandles =
                missingCloseData
                |> RendererModel.prepareData
                |> RendererModel.candleSeriesForTracePrepared trace
            Expect.equal missingCloseCandles.Length 1 "A missing close component must omit the incomplete candle instead of filling the gap."
            Expect.equal missingCloseCandles[0].Timestamp candles[0].Timestamp "The retained candle must preserve the authored temporal presentation."

            let replaceTemporalTail revisionKey revision replacement value =
                match value with
                | SduiValue.Object fields ->
                    match fields["points"] with
                    | SduiValue.Array points ->
                        let nextPoints = Array.copy points
                        nextPoints[nextPoints.Length - 1] <- replacement
                        fields
                        |> Map.add revisionKey (SduiValue.Number revision)
                        |> Map.add "points" (SduiValue.Array nextPoints)
                        |> SduiValue.Object
                    | _ -> failtest "Temporal points must be an array."
                | _ -> failtest "Temporal data must be an object."
            let repinTemporalSeries revision value =
                match value with
                | SduiValue.Object fields ->
                    fields
                    |> Map.add "axisRevision" (SduiValue.Number revision)
                    |> SduiValue.Object
                | _ -> failtest "Temporal series must be an object."
            let revisedAxisPoint = axisPoint 41L 5 |> TemporalAxisCodec.encodePoint
            let revisedClosePoint =
                TemporalSeriesCodec.encodePoint { Position = 41L; Value = SduiValue.Number 113.25 }
            let revisedData =
                data
                |> Map.map (fun dataRef value ->
                    if dataRef = axisRef then replaceTemporalTail "revision" 8.0 revisedAxisPoint value
                    elif dataRef = refs.CloseRef then replaceTemporalTail "axisRevision" 8.0 revisedClosePoint value
                    else repinTemporalSeries 8.0 value)
            let revisedPrepared = RendererModel.prepareDataIncremental prepared revisedData
            let revisedCandles = RendererModel.candleSeriesForTracePrepared trace revisedPrepared
            Expect.equal revisedCandles[1].Close 113.25 "Incremental preparation must expose same-position close replacement."
            Expect.isTrue
                (Object.ReferenceEquals(prepared.ResolvedSeries[refs.OpenRef][0], revisedPrepared.ResolvedSeries[refs.OpenRef][0]))
                "Incremental preparation must retain an unaffected prefix instead of rematerializing all retained points."
            Expect.isFalse
                (Object.ReferenceEquals(prepared.ResolvedSeries[refs.OpenRef][1], revisedPrepared.ResolvedSeries[refs.OpenRef][1]))
                "A changed axis position must refresh dependent temporal metadata."

        testCase "DYN-TA-T-099 canonical event time drives presentation without changing topology identity" <| fun _ ->
            let axisRef = "axis.event-time.5k"
            let intervalStart minute = DateTimeOffset(2026, 9, 24, 3, minute, 0, TimeSpan.Zero)
            let point position startMinute eventMinute finality =
                let startUtc = intervalStart startMinute
                { Position = position
                  SourceIntervalId = $"5k:{startMinute}"
                  ScaleKey = "5K"
                  IntervalStartUtc = startUtc
                  IntervalEndUtc = startUtc.AddMinutes 1.0
                  EventTimeUtc = Some(intervalStart eventMinute)
                  ObservedThroughUtc = intervalStart eventMinute
                  AvailableAtUtc = if finality = PointFinality.Final then Some(intervalStart eventMinute) else None
                  Finality = finality
                  Projection = TemporalProjection.CandleSpan
                  Quality = Some "authoritative" }
            let lineTrace =
                { TraceId = "sma-5k"
                  Kind = TaTraceKind.Line
                  DataRef = "series.event-time.sma"
                  Label = "SMA 5K"
                  Color = "#2563eb"
                  Width = 1.0
                  Visible = true
                  CandleDataRefs = None
                  Options = Map.empty }
            let data eventMinute revision =
                let axis =
                    { AxisRef = axisRef
                      Revision = revision
                      Points =
                        [| point 0L 4 5 PointFinality.Final
                           point 1L 5 eventMinute PointFinality.Preview |] }
                let series =
                    { AxisRef = axisRef
                      AxisRevision = revision
                      Points =
                        [| { Position = 0L; Value = SduiValue.Number 100.0 }
                           { Position = 1L; Value = SduiValue.Number 101.0 } |] }
                Map [ axisRef, TemporalAxisCodec.encode axis; lineTrace.DataRef, TemporalSeriesCodec.encode series ]

            let initial = RendererModel.prepareData (data 6 1L)
            let revised = RendererModel.prepareData (data 7 2L)
            Expect.sequenceEqual
                (RendererModel.traceTimestampsPrepared lineTrace initial)
                [| "2026-09-24T03:05:00.0000000+00:00"; "2026-09-24T03:06:00.0000000+00:00" |]
                "Completed and forming points must expose the owner-authored presentation time."
            Expect.sequenceEqual
                (RendererModel.traceTimestampsPrepared lineTrace revised)
                [| "2026-09-24T03:05:00.0000000+00:00"; "2026-09-24T03:07:00.0000000+00:00" |]
                "A forming preview may advance its presentation time in-place."
            Expect.sequenceEqual
                (RendererModel.traceTopologyTimestampsPrepared lineTrace initial)
                (RendererModel.traceTopologyTimestampsPrepared lineTrace revised)
                "Same-position preview event-time updates must not change chart topology identity."
            Expect.equal
                ((RendererModel.lineSeriesPrepared lineTrace.DataRef revised |> Array.item 1).Timestamp)
                "2026-09-24T03:07:00.0000000+00:00"
                "The row value and cursor reader must receive the revised canonical event time."

        testCase "DYN-T-548 triangle direction is independent from anchor" <| fun _ ->
            let up = RendererModel.markerTrianglePoints TaMarkerShape.TriangleUp 10.0 20.0 4.5 |> Option.get
            let down = RendererModel.markerTrianglePoints TaMarkerShape.TriangleDown 10.0 20.0 4.5 |> Option.get
            Expect.equal up [| 5.5, 24.5; 14.5, 24.5; 10.0, 15.5 |] "TriangleUp must point toward the top of the screen."
            Expect.equal down [| 5.5, 15.5; 14.5, 15.5; 10.0, 24.5 |] "TriangleDown must point toward the bottom of the screen."
            Expect.isNone (RendererModel.markerTrianglePoints TaMarkerShape.Circle 10.0 20.0 4.5) "Non-triangle shapes do not use triangle geometry."

        testCase "DYN-T-563 marker label geometry is bounded and deterministic" <| fun _ ->
            let right = RendererModel.markerLabelGeometry 1000.0 310.0 10.0 100.0 25.0 "BUY 7588.25" |> Option.get
            Expect.equal right.Text "BUY 7588.25" "The renderer must preserve the producer-authored presentation string."
            Expect.equal right.TextAnchor "start" "Labels prefer the marker's right side."
            Expect.equal right.X 108.0 "The marker-to-label gap is deterministic."

            let left = RendererModel.markerLabelGeometry 1000.0 310.0 10.0 990.0 400.0 "SELL 7603.50 PnL +762.50" |> Option.get
            Expect.equal left.TextAnchor "end" "Right-edge labels move to the marker's left side."
            Expect.equal left.X 982.0 "Left-side placement preserves the deterministic gap."
            Expect.equal left.Y 302.0 "Labels clamp vertically inside the row viewBox."

            let longLabel = String.replicate 60 "X" |> RendererModel.markerLabelGeometry 1000.0 310.0 10.0 500.0 100.0 |> Option.get
            Expect.equal longLabel.Text.Length 48 "Visible labels are bounded without changing the tooltip contract."
            Expect.stringEnds longLabel.Text "..." "Truncation must be explicit."
            Expect.isNone (RendererModel.markerLabelGeometry 1000.0 310.0 10.0 100.0 50.0 "  ") "Blank labels do not create SVG text."

            let nearA = RendererModel.markerLabelGeometry 1000.0 310.0 30.0 700.0 60.0 "SELL 7603.50 PnL +762.50" |> Option.get
            let nearB = RendererModel.markerLabelGeometry 1000.0 310.0 30.0 900.0 64.0 "SELL 7591.00" |> Option.get
            let far = RendererModel.markerLabelGeometry 1000.0 310.0 30.0 100.0 70.0 "BUY 7588.25" |> Option.get
            Expect.equal
                (RendererModel.markerLabelCollisionLanes 6.0 [| TaMarkerAnchor.AboveBar, nearA; TaMarkerAnchor.AboveBar, nearB; TaMarkerAnchor.AboveBar, far |])
                [| 1; 0; 0 |]
                "Mobile-size adjacent labels get separate left-edge-ordered lanes while disjoint labels reuse the first lane."
            Expect.equal
                (RendererModel.markerLabelCollisionLanes 6.0 [| TaMarkerAnchor.AboveBar, nearA; TaMarkerAnchor.BelowBar, nearB |])
                [| 0; 1 |]
                "Above-bar and below-bar labels share collision lanes because their visible text can still overlap."
            Expect.equal
                (RendererModel.markerLabelLaneY 310.0 32.0 TaMarkerAnchor.AboveBar 20.0 1)
                52.0
                "An above-bar label near the top edge expands downward instead of clamping onto another label."
            Expect.equal
                (RendererModel.markerLabelLaneY 310.0 32.0 TaMarkerAnchor.BelowBar 300.0 1)
                268.0
                "A below-bar label near the bottom edge expands upward instead of clamping onto another label."

        testCase "DYN-T-573 marker budget is four direct glyphs plus one deterministic overflow cluster" <| fun _ ->
            let target =
                { Timestamp = "2026-09-24T01:00:00Z"
                  Open = 100.0
                  High = 102.0
                  Low = 99.0
                  Close = 101.0
                  Volume = 10.0
                  Temporal = None }
            let placement index =
                { TraceId = if index % 2 = 0 then "orders-a" else "orders-b"
                  TargetTraceId = "price"
                  Position = 10.0
                  SlotIndex = 7
                  Lane = index
                  Target = target
                  Marker =
                    { MarkerId = $"order-{index}"
                      EventTimeUtc = "2026-09-24T01:00:00Z"
                      Anchor = TaMarkerAnchor.AboveBar
                      Shape = TaMarkerShape.Circle
                      Fill = TaMarkerFill.Outline
                      Color = "#2563eb"
                      Label = Some $"Order {index}"
                      Tooltip = [||] } }
            let direct, overflow = Array.init 7 placement |> RendererModel.markerPresentation
            Expect.equal direct.Length 4 "Only four marker glyphs render directly in one aggregate lane."
            Expect.sequenceEqual (direct |> Array.map (fun value -> value.Marker.MarkerId)) [| "order-0"; "order-1"; "order-2"; "order-3" |] "Direct glyph order is deterministic."
            Expect.equal overflow.Length 1 "Overflow collapses into one cluster control."
            Expect.equal overflow[0].Markers.Length 3 "The cluster preserves every hidden marker."
            Expect.equal overflow[0].Lane 4 "The cluster occupies the fixed lane after the direct budget."
            let cursorItems = Array.init 7 placement |> RendererModel.markerCursorItems 7
            Expect.equal cursorItems.Length 7 "The fixed-height OFI band retains every labeled marker at the selected slot."
            Expect.sequenceEqual
                (cursorItems |> Array.map _.MarkerId)
                [| "order-0"; "order-2"; "order-4"; "order-6"; "order-1"; "order-3"; "order-5" |]
                "OFI compact items are deterministic across marker traces."
            Expect.isTrue (cursorItems |> Array.forall (fun item -> item.EventTimeUtc = "2026-09-24T01:00:00Z")) "OFI items retain the marker's exact event time rather than reconstructing it from the row axis."
            Expect.isTrue (cursorItems |> Array.forall (fun item -> item.Tooltip.Contains "Event time:")) "OFI items retain the glyph tooltip payload."
            Expect.isEmpty (Array.init 7 placement |> RendererModel.markerCursorItems 6) "A cursor slot without events keeps an empty OFI band."

        testCase "DYN-T-572 overview stripes collapse same trace and assign cross-trace lanes" <| fun _ ->
            let stripe id color =
                { StripeId = id
                  EventTimeUtc = "2026-09-24T01:00:00Z"
                  Color = color
                  StrokeWidthCssPixels = 1.0
                  Label = None
                  Tooltip = [||] }
            let placement traceId order stripeValue =
                { TraceId = traceId
                  TargetTraceId = "price"
                  CollisionGroup = "trade-events"
                  LayerOrder = order
                  Position = 10.0
                  SlotIndex = 7
                  Stripe = stripeValue }
            let visuals =
                [| placement "signals" 2 (stripe "signal-a" "#2563eb")
                   placement "signals" 2 (stripe "signal-b" "#2563eb")
                   placement "fills" 1 (stripe "fill-a" "#dc2626") |]
                |> RendererModel.overviewStripeVisuals
            Expect.equal visuals.Length 2 "Same trace and pixel collapse into one stripe visual."
            Expect.equal visuals[0].TraceId "fills" "LayerOrder determines the first collision lane."
            Expect.equal visuals[0].Lane 0 "Lowest LayerOrder owns lane zero."
            Expect.equal visuals[1].Lane 1 "The next trace receives the next deterministic lane."
            Expect.equal visuals[1].Stripes.Length 2 "Collapsed stripe ids remain available to the interaction bucket."

        testCase "DYN-T-576 row height policy is marker-independent and bounded" <| fun _ ->
            let baseRow =
                { RowId = "price"
                  Kind = TaRowKind.Candlestick
                  DataRef = "price"
                  HeightWeight = 1.0
                  Visible = true
                  Options = Map.empty
                  Traces = [||] }
            let candle =
                { TraceId = "price"
                  Kind = TaTraceKind.Candlestick
                  DataRef = "price"
                  Label = "Price"
                  Color = "#334155"
                  Width = 1.0
                  Visible = true
                  CandleDataRefs = None
                  Options = Map.empty }
            let marker = { candle with TraceId = "markers"; Kind = TaTraceKind.Marker; DataRef = "markers" }
            let candleBounds = RendererModel.rowHeightBounds baseRow [| candle |]
            let markerBounds = RendererModel.rowHeightBounds baseRow [| candle; marker |]
            let tallBounds = RendererModel.rowHeightBounds { baseRow with HeightWeight = 9.0 } [| candle; marker |]
            let scalarBounds = RendererModel.rowHeightBounds baseRow [| { candle with Kind = TaTraceKind.Line } |]
            let tallScalarBounds = RendererModel.rowHeightBounds { baseRow with HeightWeight = 9.0 } [| { candle with Kind = TaTraceKind.Line } |]
            Expect.equal candleBounds.DefaultHeight 250 "Candle default follows HeightWeight."
            Expect.equal markerBounds.DefaultHeight candleBounds.DefaultHeight "Markers do not inflate row height."
            Expect.equal tallBounds.DefaultHeight 250 "Authored candle defaults cap at 250px."
            Expect.equal tallBounds.Maximum 720 "Manual candle resizing remains available above the authored default cap."
            Expect.equal scalarBounds.DefaultHeight 112 "Scalar rows use the compact baseline."
            Expect.equal tallScalarBounds.DefaultHeight 250 "Authored scalar defaults cap at 250px."
            Expect.equal tallScalarBounds.Maximum 480 "Manual scalar resizing remains available above the authored default cap."
            Expect.equal (RendererModel.rowHeightStorageKey (CanvasInstanceId "canvas-a") "price") "canvas-a:price" "Local row height state is isolated by canvas and row."

        testCase "DYN-T-564 timestamp presentation is locale-independent" <| fun _ ->
            let value = "2026-09-24T13:14:15.1234567+00:00"
            Expect.equal (RendererModel.timestampParts value) (Some("2026-09-24", "13:14:15")) "Cursor labels use fixed date and clock lines."
            Expect.equal (RendererModel.fullTimestamp value) (Some "2026-09-24 13:14:15") "Data windows use a fixed-width timestamp prefix."
            Expect.isNone (RendererModel.timestampParts "B1") "Non-canonical timestamps fail closed instead of using browser locale parsing."

        testCase "DYN-T-541 DYN-T-551 marker placement uses candle position and aggregate lanes" <| fun _ ->
            let axisRef = "axis.marker.renderer"
            let time minute = DateTimeOffset(2026, 9, 21, 1, minute, 0, TimeSpan.Zero)
            let axisPoint position minute =
                { Position = position
                  SourceIntervalId = $"marker-renderer-{position}"
                  ScaleKey = "1K"
                  IntervalStartUtc = time minute
                  IntervalEndUtc = (time minute).AddMinutes 1.0
                  EventTimeUtc = Some((time minute).AddMinutes 1.0)
                  ObservedThroughUtc = (time minute).AddMinutes 1.0
                  AvailableAtUtc = Some((time minute).AddMinutes 1.0)
                  Finality = PointFinality.Final
                  Projection = TemporalProjection.CandleSpan
                  Quality = Some "complete" }
            let candleValue openValue =
                SduiValue.Object(
                    Map [ "o", SduiValue.Number openValue
                          "h", SduiValue.Number(openValue + 3.0)
                          "l", SduiValue.Number(openValue - 2.0)
                          "c", SduiValue.Number(openValue + 1.0)
                          "v", SduiValue.Number 100.0 ])
            let candleTrace =
                { TraceId = "price"
                  Kind = TaTraceKind.Candlestick
                  DataRef = "series.marker.price"
                  Label = "Price"
                  Color = "#334155"
                  Width = 1.0
                  Visible = true
                  CandleDataRefs = None
                  Options = Map.empty }
            let markerTrace =
                { candleTrace with
                    TraceId = "signals"
                    Kind = TaTraceKind.Marker
                    DataRef = "series.marker.signals"
                    Label = "Signals"
                    Options = TaMarkerTraceOptionsCodec.encode { TargetTraceId = candleTrace.TraceId } }
            let marker markerId anchor label =
                { MarkerId = markerId
                  EventTimeUtc = "2026-09-21T01:00:30Z"
                  Anchor = anchor
                  Shape = TaMarkerShape.TriangleDown
                  Fill = TaMarkerFill.Solid
                  Color = "#dc2626"
                  Label = Some label
                  Tooltip =
                    [| { Key = "first"; Label = "第一"; Value = "A" }
                       { Key = "second"; Label = "第二"; Value = "B" } |] }
            let markers =
                [| marker "above-1" TaMarkerAnchor.AboveBar "Above 1"
                   marker "below-1" TaMarkerAnchor.BelowBar "Below 1"
                   marker "above-2" TaMarkerAnchor.AboveBar "Above 2" |]
            let axis =
                { AxisRef = axisRef
                  Revision = 3L
                  Points = [| axisPoint 10L 0; axisPoint 11L 1 |] }
            let data =
                Map [ axisRef, TemporalAxisCodec.encode axis
                      candleTrace.DataRef,
                      TemporalSeriesCodec.encode
                          { AxisRef = axisRef
                            AxisRevision = axis.Revision
                            Points =
                                [| { Position = 10L; Value = candleValue 100.0 }
                                   { Position = 11L; Value = candleValue 101.0 } |] }
                      markerTrace.DataRef,
                      TemporalSeriesCodec.encode
                          { AxisRef = axisRef
                            AxisRevision = axis.Revision
                            Points =
                                [| { Position = 10L; Value = TaMarkerCodec.encodeBucket markers }
                                   { Position = 11L; Value = TaMarkerCodec.encodeBucket [||] } |] } ]
            let prepared = RendererModel.prepareData data
            let timeline = RendererModel.traceTimestampsPrepared candleTrace prepared
            let placements =
                RendererModel.markerPlacementsPrepared markerTrace candleTrace prepared timeline
                |> RendererModel.assignAggregateMarkerLanes
            let targetByTimestamp =
                RendererModel.candleSeriesForTracePrepared candleTrace prepared
                |> RendererModel.candlePointsByTimestamp
            let duplicatedTimeline = Array.append [| timeline[0] |] timeline
            let indexedPlacements =
                RendererModel.markerPlacementsPreparedWithIndexes
                    markerTrace
                    candleTrace.TraceId
                    targetByTimestamp
                    prepared
                    (RendererModel.referenceSlotsByTimestamp duplicatedTimeline)
                |> RendererModel.assignAggregateMarkerLanes
            Expect.equal placements.Length 3 "Every accepted marker receives one placement."
            Expect.sequenceEqual indexedPlacements placements "The reusable indexed marker seam must preserve legacy placement and lane semantics."
            Expect.sequenceEqual (placements |> Array.map _.SlotIndex) [| 0; 0; 0 |] "Position 10 maps to the first candle slot regardless of EventTime evidence."
            Expect.sequenceEqual (indexedPlacements |> Array.map _.SlotIndex) [| 0; 0; 0 |] "Duplicate reference timestamps must retain the first visible slot."
            Expect.sequenceEqual (placements |> Array.map _.Lane) [| 0; 0; 1 |] "Above and below anchors own deterministic independent lanes."
            Expect.isTrue (placements |> Array.forall (fun placement -> placement.TargetTraceId = candleTrace.TraceId)) "Every placement retains its target trace identity."
            Expect.isTrue (placements |> Array.forall (fun placement -> placement.Target.High = 103.0 && placement.Target.Low = 98.0)) "Marker anchors use the target candle at the same position."
            Expect.equal
                (RendererModel.markerTooltipText placements[0])
                "Above 1\nEvent time: 2026-09-21T01:00:30Z\n第一: A\n第二: B"
                "Tooltip fields preserve authored order."

            let secondTracePlacement =
                { placements[0] with
                    TraceId = "signals-2"
                    Lane = 0
                    Marker = { placements[0].Marker with MarkerId = "above-3" } }
            let aggregate =
                Array.append placements [| secondTracePlacement |]
                |> RendererModel.assignAggregateMarkerLanes
            Expect.sequenceEqual
                (aggregate |> Array.map _.Lane)
                [| 0; 0; 1; 2 |]
                "Document trace order followed by bucket order must allocate unique lanes across marker traces."

            let markerRow =
                { RowId = "marker-row"
                  Kind = TaRowKind.Candlestick
                  DataRef = candleTrace.DataRef
                  HeightWeight = 1.0
                  Visible = true
                  Options = Map.empty
                  Traces = [| candleTrace; markerTrace |] }
            let markerDocument =
                { WorkspaceId = "marker-renderer"
                  Title = "Marker renderer"
                  RowsRef = "rows"
                  StatusRef = "status"
                  SharedTimeAxis = true
                  TemporalAxisRefs = [| axisRef |]
                  BaseRowId = Some markerRow.RowId
                  Rows = [| markerRow |]
                  EditorSchemas = [||]
                  AllowedActions = [||]
                  DefaultView = Map.empty }
            Expect.sequenceEqual (RendererModel.referenceTimelineForDocument markerDocument data) timeline "Marker data never becomes the reference timeline."
            let cursor = RendererModel.cursorSnapshot markerDocument data { StartIndex = 0; Count = 2 } 0 |> Option.get
            Expect.sequenceEqual (cursor.Values |> Array.map _.Label) [| "Price" |] "Marker is an overlay, not a numeric cursor value."

        testCase "generic editor list operations retain stable paths and validation" <| fun _ ->
            let schema =
                { TemplateKey = "ta.sma"
                  DisplayName = "SMA rows"
                  SchemaRevision = 1L
                  Fields =
                    [| { Key = "scales"
                         Label = "Scales"
                         Kind = EditorValueKind.List(EditorValueKind.Scale [| "1k"; "5k"; "30k" |], Some 1, Some 3)
                         Required = true
                         DefaultValue = Some(SduiValue.Array [| SduiValue.Text "1k"; SduiValue.Text "5k" |]) }
                       { Key = "period"
                         Label = "Period"
                         Kind = EditorValueKind.Integer(Some 1L, Some 500L)
                         Required = true
                         DefaultValue = Some(SduiValue.Number 13.0) } |] }

            let initial = RendererModel.initialEditorInputs schema
            Expect.sequenceEqual (RendererModel.listIndexes "scales" initial) [| 0; 1 |] "Two scale defaults should preserve their list positions."
            Expect.isEmpty (RendererModel.validateEditorSubmission schema initial) "Initial editor values should validate."

            let removed = RendererModel.removeListItem "scales" 0 initial
            Expect.sequenceEqual (RendererModel.listIndexes "scales" removed) [| 0 |] "Remove should compact list indexes."
            Expect.equal (RendererModel.tryEditorInput "scales[0]" removed) (Some(EditorScalarValue.Text "5k")) "The retained value should move with its compacted path."

            let added = RendererModel.addListItem "scales" (EditorValueKind.Scale [| "1k"; "5k"; "30k" |]) removed
            Expect.sequenceEqual (RendererModel.listIndexes "scales" added) [| 0; 1 |] "Add should append one stable list position."
            let moved = RendererModel.moveListItem "scales" 1 0 added
            Expect.equal (RendererModel.tryEditorInput "scales[1]" moved) (Some(EditorScalarValue.Text "5k")) "Move should swap complete list-item path groups."

            let missingPeriod = moved |> Array.filter (fun value -> value.Path <> "period")
            Expect.isNonEmpty (RendererModel.validateEditorSubmission schema missingPeriod) "Required scalar omission should fail before submit."

        testCase "scheduled preparation preserves synchronous prepared data semantics" <| fun _ ->
            let values =
                [| for index in 0 .. 31 do
                       yield candle $"2026-09-22T00:{index:D2}:00Z" 100.0 104.0 98.0 (100.0 + float index) 50.0 |]
            let data = Map [ "series.price", SduiValue.Array values ]
            let expected = RendererModel.prepareData data
            let queue = Collections.Generic.Queue<unit -> unit>()
            let mutable actual: TaPreparedRendererData option = None
            RendererModel.prepareDataScheduled queue.Enqueue data (fun prepared -> actual <- Some prepared)
            Expect.isNone actual "scheduled preparation must not complete synchronously"
            while queue.Count > 0 do queue.Dequeue() ()
            Expect.equal actual (Some expected) "scheduled preparation must preserve the pure prepared-data result"

        testCase "adaptive axes and coverage reanchor preserve event-time intent" <| fun _ ->
            let hourly = [| for index in 0 .. 3999 -> DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero).AddHours(float index).ToString("O") |]
            let fiveMinute = [| for index in 0 .. 299 -> DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero).AddMinutes(float index * 5.0).ToString("O") |]
            let hourlyLabels = RendererModel.adaptiveTimeLabels 92.0 1552.0 hourly
            let fiveMinuteLabels = RendererModel.adaptiveTimeLabels 92.0 1552.0 fiveMinute
            Expect.isTrue (hourlyLabels.Length >= 12 && hourlyLabels.Length <= 16) "wide 60K coverage needs bounded day-readable ticks"
            Expect.isTrue (hourlyLabels |> Array.map snd |> Array.distinctBy (fun value -> value.Substring(0, 10)) |> Array.length > 1) "60K labels must expose multiple dates"
            Expect.isTrue (fiveMinuteLabels |> Array.map snd |> Array.exists (fun value -> value.Substring(11, 2) <> "00")) "5K labels must retain hour positions"

            let oldTimeline = [| for index in 200 .. 4199 -> $"T{index:D4}" |]
            let prependedTimeline = [| for index in 0 .. 4199 -> $"T{index:D4}" |]
            let earlier =
                RendererModel.tryReanchorWindow 12 4000 -100 oldTimeline prependedTimeline { StartIndex = 0; Count = 4000 }
                |> Option.get
            Expect.equal earlier { StartIndex = 100; Count = 4000 } "prepend merge must apply the pending leftward pan relative to the old event-time anchor"
            Expect.isTrue (RendererModel.coverageExtended TaCoverageDirection.Earlier oldTimeline prependedTimeline) "prepend must be classified as earlier coverage"

            let initialTimeline = [| for index in 0 .. 3999 -> $"T{index:D4}" |]
            let appendedTimeline = [| for index in 0 .. 4199 -> $"T{index:D4}" |]
            let later =
                RendererModel.tryReanchorWindow 12 4000 100 initialTimeline appendedTimeline { StartIndex = 0; Count = 4000 }
                |> Option.get
            Expect.equal later { StartIndex = 100; Count = 4000 } "append merge must apply the pending rightward pan without exceeding the visible cap"
            Expect.isTrue (RendererModel.coverageExtended TaCoverageDirection.Later initialTimeline appendedTimeline) "append must be classified as later coverage"
            Expect.isFalse (RendererModel.coverageExtended TaCoverageDirection.Later oldTimeline prependedTimeline) "opposite-direction extension must not satisfy a later intent"

        testCase "adjacent coverage uses explicit document authority boundaries" <| fun _ ->
            let start0 = "2026-09-22T01:00:00Z"
            let end0 = "2026-09-22T01:01:00Z"
            let end1 = "2026-09-22T01:02:00Z"
            let price =
                SduiValue.Array
                    [| temporalPoint "p0" "1K" start0 end0 end0 None PointFinality.Final TemporalProjection.CandleSpan None (Some(candle start0 100.0 102.0 99.0 101.0 10.0))
                       temporalPoint "p1" "1K" end0 end1 end1 None PointFinality.Final TemporalProjection.CandleSpan None (Some(candle end0 101.0 103.0 100.0 102.0 11.0)) |]
            let row =
                { RowId = "price"
                  Kind = TaRowKind.Candlestick
                  DataRef = "series.price"
                  HeightWeight = 1.0
                  Visible = true
                  Traces = [||]
                  Options = Map.empty }
            let document =
                { WorkspaceId = "coverage"
                  Title = "Coverage"
                  RowsRef = "rows"
                  StatusRef = "status"
                  SharedTimeAxis = true
                  TemporalAxisRefs = [||]
                  BaseRowId = Some row.RowId
                  Rows = [| row |]
                  EditorSchemas = [||]
                  AllowedActions = [| "visible-range-changed" |]
                  DefaultView =
                    Map [
                        "query.fromUtc", SduiValue.Text "2026-09-01T00:00:00Z"
                        "query.toUtcExclusive", SduiValue.Text "2026-10-01T00:00:00Z"
                    ] }
            let prepared = RendererModel.prepareData (Map [ row.DataRef, price ])
            let earlier = RendererModel.tryAdjacentCoverageRange TaCoverageDirection.Earlier 4000 document prepared |> Option.get
            let later = RendererModel.tryAdjacentCoverageRange TaCoverageDirection.Later 4000 document prepared |> Option.get
            Expect.equal earlier.StartEventTimeUtc "2026-09-01T00:00:00Z" "earlier request must use the explicit authorized query boundary"
            Expect.equal (DateTimeOffset.Parse earlier.EndEventTimeExclusiveUtc) (DateTimeOffset.Parse start0) "earlier request must stop at the first loaded event time"
            Expect.equal (DateTimeOffset.Parse later.StartEventTimeUtc) (DateTimeOffset.Parse end1) "later request must begin at the actual final interval end"
            Expect.equal later.EndEventTimeExclusiveUtc "2026-10-01T00:00:00Z" "later request must use the explicit authorized query boundary"
            Expect.equal later.MaximumBasePoints 4000 "coverage request must preserve the visible cap"

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
