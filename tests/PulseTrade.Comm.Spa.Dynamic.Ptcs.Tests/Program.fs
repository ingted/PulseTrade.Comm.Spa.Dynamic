open System
open System.Text.Json
open Expecto
open PulseTrade.Comm.Spa
open PulseTrade.Comm.Spa.Dynamic
open PulseTrade.Comm.Spa.Dynamic.Contracts

let canvasId = CanvasInstanceId "canvas-main"
let documentId = DocumentId "document-main"

let editorSchema =
    { TemplateKey = "ta.sma"
      DisplayName = "SMA overlay"
      SchemaRevision = 3L
      Fields =
        [| { Key = "periods"
             Label = "Periods"
             Kind = EditorValueKind.List(EditorValueKind.Integer(Some 1L, Some 500L), Some 1, Some 8)
             Required = true
             DefaultValue = Some(SduiValue.Array [| SduiValue.Number 13.0 |]) } |] }

let editorBinding =
    { TemplateKey = editorSchema.TemplateKey
      Values = [| { Path = "periods[0]"; Value = EditorScalarValue.Number 21.0 } |] }

let row =
    { RowId = "price"
      Kind = TaRowKind.Candlestick
      DataRef = "series.price"
      HeightWeight = 2.0
      Visible = true
      Traces = [||]
      Options = Map.empty }
    |> TaRowEditorBinding.attach editorBinding
    |> Result.defaultWith (List.map _.Message >> String.concat "; " >> failwith)

let document =
    { WorkspaceId = "workspace-main"
      Title = "TA Research"
      RowsRef = "rows"
      StatusRef = "status"
      SharedTimeAxis = true
      Rows = [| row |]
      EditorSchemas = [| editorSchema |]
      AllowedActions = [| "change-query"; "poll-delta" |]
      DefaultView =
        Map [
            "query.sourceId", SduiValue.Text "binance"
            "query.instrument", SduiValue.Text "BTCUSDT"
            "query.intervalMinutes", SduiValue.Number 1.0
            "query.includePartial", SduiValue.Bool true
        ] }

let frame sequence dataRevision payload =
    { Protocol = DynamicRuntimeDefaults.protocol
      Kind =
        match payload with
        | RuntimePayload.Document _ -> RuntimeFrameKind.Document
        | RuntimePayload.Snapshot _ -> RuntimeFrameKind.Snapshot
        | RuntimePayload.Patch _ -> RuntimeFrameKind.Patch
        | RuntimePayload.Error _ -> RuntimeFrameKind.Error
        | RuntimePayload.Heartbeat _ -> RuntimeFrameKind.Heartbeat
      DocumentId = documentId
      CanvasInstanceId = canvasId
      DocumentRevision = 1L
      BaseDataRevision = if sequence <= 2L then None else Some(dataRevision - 1L)
      DataRevision = dataRevision
      TransportSequence = sequence
      Payload = payload }

let backend =
    { HandleAsync =
        fun _ clientFrame ->
            async {
                return
                    match clientFrame with
                    | RuntimeClientFrame.Mounted _ ->
                        Ok(frame 1L 0L (RuntimePayload.Document document))
                    | RuntimeClientFrame.Action(SduiAction.ChangeTaQuery _) ->
                        let snapshot =
                            { Data =
                                Map.ofList
                                    [ "status",
                                      SduiValue.Object(
                                          Map.ofList
                                              [ "label", SduiValue.Text "BACKFILL"
                                                "freshness", SduiValue.Text "backfill"
                                                "watermarkUtc", SduiValue.Text "2026-07-11T09:00:00Z"
                                                "quality", SduiValue.Text "complete"
                                                "lagSeconds", SduiValue.Number 0.0
                                                "reasonCode", SduiValue.Text "historical-query" ])
                                      "series.price", SduiValue.Array [||] ]
                              Freshness = TaFreshness.Backfill "query" }

                        Ok(frame 2L 1L (RuntimePayload.Snapshot snapshot))
                    | RuntimeClientFrame.Action(SduiAction.RequestFullSnapshot _) ->
                        Ok(frame 3L 1L (RuntimePayload.Heartbeat { ObservedAtUtc = DateTimeOffset.UtcNow }))
                    | RuntimeClientFrame.Action(SduiAction.PollDelta _) ->
                        let patch =
                            { Operations =
                                [| PatchOperation.SetStatus("status", Map.ofList [ "label", SduiValue.Text "LIVE" ]) |] }

                        Ok(frame 3L 2L (RuntimePayload.Patch patch))
                    | RuntimeClientFrame.Unmounted _ ->
                        Ok(frame 4L 2L (RuntimePayload.Heartbeat { ObservedAtUtc = DateTimeOffset.UtcNow }))
                    | _ -> Error "unsupported-test-frame"
            } }

let session sessionId =
    { SessionId = sessionId
      Authenticated = true
      UserId = Some("user." + sessionId)
      Groups = [| "research" |]
      Roles = [| "operator" |]
      Provider = Some "test" }

let payload clientFrame =
    clientFrame
    |> TaResearchTransientWire.clientFrameToWire
    |> fun wire -> JsonSerializer.Serialize(wire, TaResearchTransientServer.jsonOptions)

let context sessionId operation requestId clientFrame =
    { Session = session sessionId
      ExtensionId = "ta-research"
      ChannelId = "main"
      Operation = operation
      RequestId = requestId
      Payload = payload clientFrame
      BrowserId = Some "browser"
      TabId = Some "tab" }

let decodeState (text: string) =
    JsonSerializer.Deserialize<TaTransientStateWire>(text, TaResearchTransientServer.jsonOptions)

let browserPayload kind actionKind =
    { wireVersion = "ta-browser.v1"
      kind = kind
      actionKind = actionKind
      canvasInstanceId = "canvas-main"
      rowId = ""
      rowKind = ""
      dataRef = ""
      heightWeight = 0.0
      visible = false
      sourceId = ""
      instrument = ""
      intervalMinutes = 0
      fromUtc = ""
      toUtcExclusive = ""
      includePartial = false
      afterDataRevision = 0.0
      dataRevision = 0.0
      reasonCode = ""
      templateKey = ""
      hasTemplateRowId = false
      editorValues = [||]
      expectedDocumentRevision = 0.0
      hasExpectedDocumentRevision = false }
    |> fun wire -> JsonSerializer.Serialize(wire, TaResearchTransientServer.jsonOptions)

let browserContext sessionId operation requestId payloadText =
    { Session = session sessionId
      ExtensionId = "ta-research"
      ChannelId = "browser-main"
      Operation = operation
      RequestId = requestId
      Payload = payloadText
      BrowserId = Some "browser"
      TabId = Some "tab" }

let decodeBrowserState (text: string) =
    JsonSerializer.Deserialize<TaBrowserStateWire>(text, TaResearchTransientServer.jsonOptions)

let requireOk = function
    | Ok value -> value
    | Error error -> failtest error

let tests =
    testList
        "PTCS Dynamic transient adapter"
        [ testCase "wire round-trip preserves nested SDUI values" (fun _ ->
              let value =
                  SduiValue.Object(
                      Map.ofList
                          [ "enabled", SduiValue.Bool true
                            "values", SduiValue.Array [| SduiValue.Number 1.5; SduiValue.Text "x" |] ])

              let actual = value |> TaResearchTransientWire.valueToWire |> TaResearchTransientWire.valueFromWire
              Expect.equal actual value "recursive SDUI value wire should round-trip." )

          testCase "document editor catalog and row binding survive transient and browser wires" (fun _ ->
              let transient = document |> TaResearchTransientWire.documentToWire |> TaResearchTransientWire.documentFromWire
              Expect.equal transient.EditorSchemas [| editorSchema |] "document schema catalog must survive the transient wire."
              Expect.equal transient.Rows[0].Options row.Options "versioned row binding must survive the transient wire."

              let state =
                  { Identity = { DocumentId = documentId; CanvasInstanceId = canvasId }
                    Document = Some document
                    Data = Map.empty
                    DocumentRevision = 7L
                    DataRevision = 0L
                    LastTransportSequence = 1L
                    View = { Values = Map.empty }
                    Poll = RuntimePollState.Ready
                    LastError = None }
              let browser = TaResearchBrowserWire.stateToWire state
              Expect.equal browser.editorSchemas.Length 1 "browser state must carry the document editor catalog."
              Expect.isTrue
                  (browser.rows[0].options |> Array.exists (fun field -> field.key = TaRowEditorBinding.OptionKey))
                  "browser row options must carry the versioned editor binding." )

          testCase "generic editor action survives transient and flat browser wires" (fun _ ->
              let expected =
                  RuntimeClientFrame.Action(
                      SduiAction.ApplyTemplate(
                          canvasId,
                          Some "sma-1k",
                          "ta.sma",
                          [| { Path = "scales[0]"; Value = EditorScalarValue.Text "1k" }
                             { Path = "periods[0]"; Value = EditorScalarValue.Number 13.0 }
                             { Path = "style.visible"; Value = EditorScalarValue.Bool true } |]))

              let transient =
                  expected
                  |> TaResearchTransientWire.clientFrameToWire
                  |> TaResearchTransientWire.clientFrameFromWire

              Expect.equal transient (Ok expected) "server-side transient wire must preserve typed editor values."

              let browserBase =
                  browserPayload "action" "apply-template"
                  |> fun text -> JsonSerializer.Deserialize<TaBrowserClientFrameWire>(text, TaResearchTransientServer.jsonOptions)

              let browser =
                  { browserBase with
                      rowId = "sma-1k"
                      templateKey = "ta.sma"
                      hasTemplateRowId = true
                      editorValues =
                          [| { path = "scales[0]"; kind = "text"; textValue = "1k"; numberValue = 0.0; boolValue = false }
                             { path = "periods[0]"; kind = "number"; textValue = ""; numberValue = 13.0; boolValue = false }
                             { path = "style.visible"; kind = "bool"; textValue = ""; numberValue = 0.0; boolValue = true } |] }
                  |> TaResearchBrowserWire.clientFrameFromWire

              Expect.equal browser (Ok expected) "browser wire must preserve the same typed editor action without recursive JSON." )

          testCase "browser point wire accepts canonical compact keys and legacy aliases" (fun _ ->
              let compactCandle =
                  SduiValue.Object(
                      Map [
                          "t", SduiValue.Text "2026-07-12T00:00:00Z"
                          "o", SduiValue.Number 100.0
                          "h", SduiValue.Number 102.0
                          "l", SduiValue.Number 99.0
                          "c", SduiValue.Number 101.0
                          "v", SduiValue.Number 12.5
                      ])
                  |> TaResearchBrowserWire.pointFromValue
                  |> Option.defaultWith (fun () -> failtest "compact candle point was rejected")

              Expect.isTrue compactCandle.hasOpen "compact o must set hasOpen."
              Expect.isTrue compactCandle.hasHigh "compact h must set hasHigh."
              Expect.isTrue compactCandle.hasLow "compact l must set hasLow."
              Expect.isTrue compactCandle.hasClose "compact c must set hasClose."
              Expect.isTrue compactCandle.hasVolume "compact v must set hasVolume."
              Expect.equal compactCandle.time "2026-07-12T00:00:00Z" "compact t must preserve timestamp."
              Expect.equal compactCandle.closeValue 101.0 "compact c must preserve close."

              let compactLine =
                  SduiValue.Object(Map [ "t", SduiValue.Text "2026-07-12T00:01:00Z"; "v", SduiValue.Number 7.25 ])
                  |> TaResearchBrowserWire.pointFromValue
                  |> Option.defaultWith (fun () -> failtest "compact line point was rejected")

              Expect.isTrue compactLine.hasLineValue "compact line v must set hasLineValue."
              Expect.equal compactLine.lineValue 7.25 "compact line v must preserve value."

              let legacy =
                  SduiValue.Object(
                      Map [
                          "time", SduiValue.Text "2026-07-12T00:02:00Z"
                          "open", SduiValue.Number 1.0
                          "high", SduiValue.Number 2.0
                          "low", SduiValue.Number 0.5
                          "close", SduiValue.Number 1.5
                          "volume", SduiValue.Number 3.0
                      ])
                  |> TaResearchBrowserWire.pointFromValue
                  |> Option.defaultWith (fun () -> failtest "legacy candle point was rejected")

              Expect.isTrue legacy.hasOpen "legacy open alias remains supported."
              Expect.equal legacy.volumeValue 3.0 "legacy volume alias remains supported." )

          testCase "browser delta wire sends only changed points and rolling-prefix tombstone" (fun _ ->
              let pointValue timestamp closeValue =
                  SduiValue.Object(
                      Map [ "t", SduiValue.Text timestamp
                            "o", SduiValue.Number closeValue
                            "h", SduiValue.Number closeValue
                            "l", SduiValue.Number closeValue
                            "c", SduiValue.Number closeValue
                            "v", SduiValue.Number 1.0 ])
              let state revision points =
                  { Identity = { DocumentId = documentId; CanvasInstanceId = canvasId }
                    Document = Some document
                    Data = Map [ "series.price", SduiValue.Array points ]
                    DocumentRevision = 1L
                    DataRevision = revision
                    LastTransportSequence = revision
                    View = { Values = Map.empty }
                    Poll = RuntimePollState.Ready
                    LastError = None }
              let previous =
                  state 10L [| pointValue "2026-07-12T00:00:00Z" 100.0; pointValue "2026-07-12T00:01:00Z" 101.0 |]
              let next =
                  state 11L [| pointValue "2026-07-12T00:01:00Z" 101.0; pointValue "2026-07-12T00:02:00Z" 102.0 |]
              let wire = TaResearchBrowserWire.stateToWireAgainst (Some previous) next

              Expect.equal wire.updateKind "delta" "stable document revisions emit a delta."
              Expect.equal wire.baseDataRevision 10L "delta records its exact base revision."
              Expect.equal wire.series.Length 1 "one trace produces one bounded series delta."
              Expect.equal wire.series[0].mode "upsert" "series delta uses timestamp-keyed upsert semantics."
              Expect.equal wire.series[0].pointCount 1 "only the newly appended point crosses the wire."
              Expect.isTrue wire.series[0].hasRemoveBeforeTime "rolling removal is explicit."
              Expect.equal wire.series[0].removeBeforeTime "2026-07-12T00:01:00Z" "the retained-window boundary is stable."

              let largePoints =
                  Array.init 2500 (fun index ->
                      pointValue (sprintf "2026-07-%02dT%02d:%02d:00Z" (12 + index / 1440) ((index / 60) % 24) (index % 60)) (100.0 + float index))
              let large = largePoints |> state 12L |> TaResearchBrowserWire.stateToWire
              Expect.equal
                  large.series[0].pointCount
                  TaResearchBrowserWire.MaxFullSnapshotPointsPerSeries
                  "full bootstrap must retain the complete product working set while server RuntimeState remains authoritative."
              Expect.equal large.timeline.Length 2000 "the v4 bootstrap timeline contains all retained timestamps."
              Expect.equal large.series[0].closeValues.Length 2000 "the v4 candle close column contains all retained values."
              Expect.isEmpty large.series[0].points "the v4 wire does not duplicate row objects."

              let empty = state 11L [||]
              let firstData = largePoints |> state 12L |> TaResearchBrowserWire.stateToWireAgainst (Some empty)
              Expect.equal firstData.updateKind "full" "empty-to-first-data must not be truncated by the stable delta cap."
              Expect.equal firstData.series[0].pointCount 2000 "first data bootstrap retains 2000 points."

              let changed =
                  largePoints
                  |> Array.map (function
                      | SduiValue.Object values ->
                          let closeValue = values["c"] |> function SduiValue.Number value -> value | _ -> 0.0
                          SduiValue.Object(values |> Map.add "c" (SduiValue.Number(closeValue + 1.0)))
                      | value -> value)
                  |> state 13L
                  |> TaResearchBrowserWire.stateToWireAgainst (Some (state 12L largePoints))
              Expect.equal changed.updateKind "delta" "stable non-empty revisions use delta."
              Expect.equal changed.series[0].pointCount TaResearchBrowserWire.MaxDeltaPointsPerSeries "delta remains independently bounded."

              let linePoint =
                  SduiValue.Object(Map [ "t", SduiValue.Text "2026-07-12T00:00:00Z"; "v", SduiValue.Number 42.0 ])
                  |> TaResearchBrowserWire.pointFromValue
                  |> Option.defaultWith (fun () -> failtest "line point was rejected")
              let compactJson = JsonSerializer.Serialize(linePoint, TaResearchTransientServer.jsonOptions)
              Expect.stringContains compactJson "\"lineValue\":42" "line value must remain present."
              Expect.isFalse (compactJson.Contains("openValue", StringComparison.Ordinal)) "unused OHLC defaults must be omitted."
              Expect.isFalse (compactJson.Contains("hasOpen", StringComparison.Ordinal)) "false field flags must be omitted."

              let zeroRevisionJson =
                  state 0L [||]
                  |> TaResearchBrowserWire.stateToWire
                  |> fun value -> JsonSerializer.Serialize(value, TaResearchTransientServer.jsonOptions)

              Expect.stringContains zeroRevisionJson "\"baseDataRevision\":0" "wire base revision zero is protocol state and must not be omitted."
              Expect.stringContains zeroRevisionJson "\"documentRevision\":1" "wire document revision must remain explicit."
              Expect.stringContains zeroRevisionJson "\"dataRevision\":0" "wire data revision zero is protocol state and must not be omitted."
              Expect.stringContains zeroRevisionJson "\"transportSequence\":0" "wire transport sequence zero is protocol state and must not be omitted."

              let zeroPoll =
                  browserPayload "action" "poll-delta"
                  |> fun text -> JsonSerializer.Deserialize<TaBrowserClientFrameWire>(text, TaResearchTransientServer.jsonOptions)
                  |> TaResearchBrowserWire.clientFrameFromWire

              Expect.equal
                  zeroPoll
                  (Ok(RuntimeClientFrame.Action(SduiAction.PollDelta(canvasId, 0L))))
                  "poll-delta revision zero must survive the browser wire round trip." )

          testCase "browser v4 preserves temporal projection metadata in bounded columns" (fun _ ->
              let point =
                  TemporalPointCodec.encode
                      { SourceIntervalId = "es-5k:1300"
                        ScaleKey = "5K"
                        IntervalStartUtc = DateTimeOffset.Parse "2026-09-03T13:00:00Z"
                        IntervalEndUtc = DateTimeOffset.Parse "2026-09-03T13:05:00Z"
                        ObservedThroughUtc = DateTimeOffset.Parse "2026-09-03T13:05:00Z"
                        AvailableAtUtc = Some(DateTimeOffset.Parse "2026-09-03T13:05:01Z")
                        Finality = PointFinality.Final
                        Projection = TemporalProjection.CandleSpan
                        Quality = Some "complete"
                        Value =
                            Some(
                                SduiValue.Object(
                                    Map [ "o", SduiValue.Number 100.0
                                          "h", SduiValue.Number 110.0
                                          "l", SduiValue.Number 95.0
                                          "c", SduiValue.Number 108.0
                                          "v", SduiValue.Number 90.0 ])) }
              let state =
                  { Identity = { DocumentId = documentId; CanvasInstanceId = canvasId }
                    Document = Some document
                    Data = Map [ "series.price", SduiValue.Array [| point |] ]
                    DocumentRevision = 1L
                    DataRevision = 20L
                    LastTransportSequence = 20L
                    View = { Values = Map.empty }
                    Poll = RuntimePollState.Ready
                    LastError = None }

              let wire = TaResearchBrowserWire.stateToWire state
              Expect.equal wire.wireVersion "ta-browser.v4" "temporal metadata requires the v4 bounded browser wire."
              Expect.isTrue wire.series[0].hasTemporal "temporal columns are explicit per homogeneous series."
              Expect.sequenceEqual wire.series[0].sourceIntervalIds [| "es-5k:1300" |] "source interval identity must survive transport."
              Expect.sequenceEqual wire.series[0].scaleKeys [| "5K" |] "scale must survive transport."
              Expect.sequenceEqual wire.series[0].finality [| "final" |] "finality must survive transport."
              Expect.sequenceEqual wire.series[0].projections [| "candle-span" |] "presentation projection must survive transport."
              Expect.sequenceEqual wire.series[0].hasAvailableAtUtc [| true |] "availability presence is not inferred from empty text.")

          testCaseAsync "server adapter applies canonical reducer and keeps sessions isolated" (async {
              let handler = TaResearchTransientServer.createHandler backend
              let mounted = RuntimeClientFrame.Mounted canvasId
              let query =
                  RuntimeClientFrame.Action(
                      SduiAction.ChangeTaQuery(
                          canvasId,
                          { SourceId = Some "sql"
                            Instrument = Some "TXF"
                            IntervalMinutes = Some 5
                            FromUtc = Some "2026-07-01T00:00:00+08:00"
                            ToUtcExclusive = Some "2026-07-12T00:00:00+08:00"
                            IncludePartial = Some true }))

              let! openA = handler (context "a" "open" "a-open" mounted)
              let stateA1 = openA |> requireOk |> decodeState |> TaResearchTransientWire.stateFromWire
              Expect.equal stateA1.Document (Some document) "open should materialize the document through the canonical reducer."
              Expect.equal stateA1.LastTransportSequence 1L "session A starts at transport sequence 1."

              let! actionA = handler (context "a" "action" "a-action" query)
              let stateA2 = actionA |> requireOk |> decodeState |> TaResearchTransientWire.stateFromWire
              Expect.equal stateA2.DataRevision 1L "query snapshot should advance data revision."
              Expect.isSome (Map.tryFind "series.price" stateA2.Data) "query snapshot should expose the price data ref."

              let! openB = handler (context "b" "open" "b-open" mounted)
              let stateB1 = openB |> requireOk |> decodeState |> TaResearchTransientWire.stateFromWire
              Expect.equal stateB1.LastTransportSequence 1L "same channel id in session B must have an independent reducer state."
              Expect.equal stateB1.DataRevision 0L "session B must not inherit session A data revision." })

          testCaseAsync "invalid payload fails closed and disconnect removes reducer state" (async {
              let unmounted = ResizeArray<CanvasInstanceId>()
              let cleanupBackend =
                  { HandleAsync =
                      fun context frameValue ->
                          async {
                              match frameValue with
                              | RuntimeClientFrame.Unmounted canvas -> unmounted.Add canvas
                              | _ -> ()

                              return! backend.HandleAsync context frameValue
                          } }
              let handler = TaResearchTransientServer.createHandler cleanupBackend
              let invalid = { context "invalid" "open" "bad" (RuntimeClientFrame.Mounted canvasId) with Payload = "{" }
              let! invalidResult = handler invalid
              Expect.isError invalidResult "invalid JSON must fail closed."

              let! _ = handler (context "cleanup" "open" "open" (RuntimeClientFrame.Mounted canvasId))
              let disconnect =
                  { context "cleanup" "disconnect" "disconnect" (RuntimeClientFrame.Unmounted canvasId) with Payload = "" }

              let! disconnected = handler disconnect
              Expect.equal disconnected (Ok "{}") "disconnect should remove channel reducer state."
              Expect.sequenceEqual unmounted [| canvasId |] "disconnect must notify the host backend with authoritative Unmounted identity."

              let! disconnectedAgain = handler disconnect
              Expect.equal disconnectedAgain (Ok "{}") "repeated disconnect should be idempotent."
              Expect.equal unmounted.Count 1 "repeated disconnect must not invoke backend cleanup twice." })

          testCaseAsync "stale browser action is rejected before the owner backend" (async {
              let handler = TaResearchTransientServer.createHandler backend
              let! opened = handler (browserContext "stale-action" "open" "open" (browserPayload "mounted" ""))
              Expect.isOk opened "the browser session must establish authoritative revision state first."

              let stale =
                  browserPayload "action" "reset-canvas"
                  |> fun text -> JsonSerializer.Deserialize<TaBrowserClientFrameWire>(text, TaResearchTransientServer.jsonOptions)
                  |> fun wire ->
                      { wire with
                          expectedDocumentRevision = 99.0
                          hasExpectedDocumentRevision = true }
                  |> fun wire -> JsonSerializer.Serialize(wire, TaResearchTransientServer.jsonOptions)

              let! result = handler (browserContext "stale-action" "action" "stale-reset" stale)
              Expect.equal result (Error "ta-revision-conflict:1") "stale action must not enter the owner backend or mutate the session canvas." })

          testCaseAsync "bounded browser wire uses the same reducer without recursive SDUI values" (async {
              let handler = TaResearchTransientServer.createHandler backend
              let! opened =
                  handler (browserContext "browser-a" "open" "browser-open" (browserPayload "mounted" ""))

              let openedState = opened |> requireOk |> decodeBrowserState
              Expect.equal openedState.wireVersion "ta-browser.v4" "browser response must use the temporal-capable compact columnar wire version."
              Expect.equal openedState.updateKind "full" "first browser response must be authoritative."
              Expect.equal openedState.title "TA Research" "document metadata should be projected for the browser."
              Expect.equal openedState.rows.Length 1 "document rows should be projected without recursive values."
              Expect.equal openedState.rows[0].kind "candlestick" "row kind should use the canonical text representation."
              Expect.equal openedState.querySourceId "binance" "browser wire must carry the source query identity."
              Expect.equal openedState.queryInstrument "BTCUSDT" "browser wire must carry the instrument query identity."
              Expect.equal openedState.queryIntervalMinutes 1 "browser wire must carry the interval query identity."

              let queryBase =
                  browserPayload "action" "change-query"
                  |> fun text -> JsonSerializer.Deserialize<TaBrowserClientFrameWire>(text, TaResearchTransientServer.jsonOptions)

              let query =
                  { queryBase with
                      sourceId = "sql"
                      instrument = "TXF"
                      intervalMinutes = 5
                      fromUtc = "2026-07-01T00:00:00+08:00"
                      toUtcExclusive = "2026-07-12T00:00:00+08:00"
                      includePartial = true }
                  |> fun wire -> JsonSerializer.Serialize(wire, TaResearchTransientServer.jsonOptions)

              let! queried = handler (browserContext "browser-a" "action" "browser-query" query)
              let queriedState = queried |> requireOk |> decodeBrowserState
              Expect.equal queriedState.updateKind "delta" "stable document revisions use a delta response."
              Expect.equal queriedState.baseDataRevision 0L "delta response identifies the accepted client base revision."
              Expect.equal queriedState.dataRevision 1L "browser query must advance the canonical reducer revision."
              Expect.equal queriedState.statusLabel "BACKFILL" "browser status should come from the canonical snapshot."
              Expect.equal queriedState.watermarkUtc "2026-07-11T09:00:00Z" "watermark should survive the bounded browser wire."
              Expect.equal queriedState.quality "complete" "quality should survive the bounded browser wire."
              Expect.equal queriedState.reasonCode "historical-query" "freshness reason should survive the bounded browser wire."
              Expect.equal queriedState.series.Length 1 "browser wire should expose one bounded series per row."
              Expect.equal queriedState.series[0].pointCount 0 "the empty backend series should remain empty."

              let fullRequest =
                  browserPayload "action" "full-snapshot"
                  |> fun text -> JsonSerializer.Deserialize<TaBrowserClientFrameWire>(text, TaResearchTransientServer.jsonOptions)
                  |> fun wire -> { wire with reasonCode = "json-export" }
                  |> fun wire -> JsonSerializer.Serialize(wire, TaResearchTransientServer.jsonOptions)
              let! exported = handler (browserContext "browser-a" "action" "browser-export" fullRequest)
              let exportedState = exported |> requireOk |> decodeBrowserState
              Expect.equal exportedState.updateKind "full" "explicit export/resync requests must return a complete browser state, not an empty delta."
              Expect.equal exportedState.dataRevision 1L "full export preserves the authoritative data revision."
              Expect.equal exportedState.rows.Length 1 "full export includes document rows."
              Expect.equal exportedState.series.Length 1 "full export includes every available series." })

          testCaseAsync "browser revision rejects fractional JS numbers" (async {
              let handler = TaResearchTransientServer.createHandler backend
              let invalidBase =
                  browserPayload "action" "poll-delta"
                  |> fun text -> JsonSerializer.Deserialize<TaBrowserClientFrameWire>(text, TaResearchTransientServer.jsonOptions)
              let invalid =
                  { invalidBase with afterDataRevision = 1.5 }
                  |> fun wire -> JsonSerializer.Serialize(wire, TaResearchTransientServer.jsonOptions)
              let! result = handler (browserContext "browser-invalid" "action" "browser-invalid-revision" invalid)
              Expect.isError result "fractional browser revisions must fail closed before entering the runtime reducer." }) ]

[<EntryPoint>]
let main argv =
    runTestsWithCLIArgs [] argv tests
