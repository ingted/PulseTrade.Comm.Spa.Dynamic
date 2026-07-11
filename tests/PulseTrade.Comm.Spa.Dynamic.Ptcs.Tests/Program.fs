open System
open System.Text.Json
open Expecto
open PulseTrade.Comm.Spa
open PulseTrade.Comm.Spa.Dynamic
open PulseTrade.Comm.Spa.Dynamic.Contracts

let canvasId = CanvasInstanceId "canvas-main"
let documentId = DocumentId "document-main"

let row =
    { RowId = "price"
      Kind = TaRowKind.Candlestick
      DataRef = "series.price"
      HeightWeight = 2.0
      Visible = true
      Options = Map.empty }

let document =
    { WorkspaceId = "workspace-main"
      Title = "TA Research"
      RowsRef = "rows"
      StatusRef = "status"
      SharedTimeAxis = true
      Rows = [| row |]
      AllowedActions = [| "change-query"; "poll-delta" |]
      DefaultView = Map.empty }

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
                                    [ "status", SduiValue.Object(Map.ofList [ "label", SduiValue.Text "BACKFILL" ])
                                      "series.price", SduiValue.Array [||] ]
                              Freshness = TaFreshness.Backfill "query" }

                        Ok(frame 2L 1L (RuntimePayload.Snapshot snapshot))
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
              let handler = TaResearchTransientServer.createHandler backend
              let invalid = { context "invalid" "open" "bad" (RuntimeClientFrame.Mounted canvasId) with Payload = "{" }
              let! invalidResult = handler invalid
              Expect.isError invalidResult "invalid JSON must fail closed."

              let! _ = handler (context "cleanup" "open" "open" (RuntimeClientFrame.Mounted canvasId))
              let disconnect =
                  { context "cleanup" "disconnect" "disconnect" (RuntimeClientFrame.Unmounted canvasId) with Payload = "" }

              let! disconnected = handler disconnect
              Expect.equal disconnected (Ok "{}") "disconnect should remove channel reducer state without invoking backend." }) ]

[<EntryPoint>]
let main argv =
    runTestsWithCLIArgs [] argv tests
