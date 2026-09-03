module PulseTrade.Comm.Spa.Dynamic.Tests.TAResearchContractsTests

open System
open Expecto
open PulseTrade.Comm.Spa.Dynamic.Contracts

let identity =
    { DocumentId = DocumentId "ta-doc"
      CanvasInstanceId = CanvasInstanceId "ta-main" }

let row =
    { RowId = "price"
      Kind = TaRowKind.Candlestick
      DataRef = "series.price"
      HeightWeight = 3.0
      Visible = true
      Options = Map.empty
      Traces = [||] }

let document =
    { WorkspaceId = "ta-main"
      Title = "TA Research"
      RowsRef = "ta.rows"
      StatusRef = "ta.status"
      SharedTimeAxis = true
      Rows = [| row |]
      AllowedActions = [| "reset-view"; "reset-canvas"; "add-row"; "change-query" |]
      DefaultView = Map [ "zoom", SduiValue.Number 1.0 ] }

let frame kind sequence baseRevision dataRevision payload =
    { Protocol = DynamicRuntimeDefaults.protocol
      Kind = kind
      DocumentId = identity.DocumentId
      CanvasInstanceId = identity.CanvasInstanceId
      DocumentRevision = 1L
      BaseDataRevision = baseRevision
      DataRevision = dataRevision
      TransportSequence = sequence
      Payload = payload }

let documentFrame = frame RuntimeFrameKind.Document 1L None 0L (RuntimePayload.Document document)

let sourceStream =
    { SourceId = "mdcq.es.1k"
      SchemaKey = "tradecore.structured-series.v1"
      Epoch = "epoch-20260904" }

let sourceTime = DateTimeOffset(2026, 9, 4, 0, 0, 0, TimeSpan.Zero)

let sourceSnapshot : SourceSnapshotEnvelope =
    { Stream = sourceStream
      SourceRevision = 10L
      LastSequence = 20L
      CapturedAtUtc = sourceTime
      Payload = SduiValue.Object(Map [ "value", SduiValue.Number 10.0 ]) }

let sourceEvent sequence baseRevision newRevision payload : SourceEventEnvelope =
    { Stream = sourceStream
      BaseSourceRevision = baseRevision
      NewSourceRevision = newRevision
      Sequence = sequence
      EventTimeUtc = sourceTime.AddSeconds(float sequence)
      Payload = payload }

let tests =
    testList "Dynamic TA Contracts" [
        testCase "DYN-TA-T-001 all frame kinds strict roundtrip" <| fun _ ->
            let frames =
                [| documentFrame
                   frame RuntimeFrameKind.Snapshot 2L None 1L (RuntimePayload.Snapshot { Data = Map [ "series.price", SduiValue.Array [||] ]; Freshness = TaFreshness.Live })
                   frame RuntimeFrameKind.Patch 3L (Some 1L) 2L (RuntimePayload.Patch { Operations = [| PatchOperation.ReplaceDataRef("ta.status", SduiValue.Text "live") |] })
                   frame RuntimeFrameKind.Error 4L None 2L (RuntimePayload.Error { ReasonCode = "provider-timeout"; Message = "Timed out"; Recoverable = true })
                   frame RuntimeFrameKind.Heartbeat 5L None 2L (RuntimePayload.Heartbeat { ObservedAtUtc = DateTimeOffset.UtcNow }) |]

            for expected in frames do
                let encoded = RuntimeCodec.encode expected
                let actual = RuntimeCodec.decode DynamicRuntimeDefaults.limits encoded |> Result.defaultWith (fun errors -> failwith (errors |> List.map _.Message |> String.concat "; "))
                Expect.equal actual expected $"{expected.Kind} should roundtrip."

        testCase "DYN-TA-T-002 unknown protocol payload mismatch script URL and oversize fail" <| fun _ ->
            let unknown = { documentFrame with Protocol = "sdui-runtime.v999" }
            let mismatch = { documentFrame with Kind = RuntimeFrameKind.Patch }
            let unsafeDocument =
                { document with
                    Rows = [| { row with Options = Map [ "url", SduiValue.Text "https://example.invalid" ] } |] }
            let unsafeFrame = { documentFrame with Payload = RuntimePayload.Document unsafeDocument }
            let tinyLimits = { DynamicRuntimeDefaults.limits with MaxFrameBytes = 10 }

            Expect.isError (RuntimeValidation.validateFrame DynamicRuntimeDefaults.limits unknown) "Unknown protocol must fail."
            Expect.isError (RuntimeValidation.validateFrame DynamicRuntimeDefaults.limits mismatch) "Payload mismatch must fail."
            Expect.isError (RuntimeValidation.validateFrame DynamicRuntimeDefaults.limits unsafeFrame) "URL/options must fail."
            Expect.isError (RuntimeCodec.decode tinyLimits (RuntimeCodec.encode documentFrame)) "Oversized frame must fail before decode."
            Expect.isError (RuntimeCodec.decode DynamicRuntimeDefaults.limits "{\"protocol\":\"sdui-runtime.v1\",\"kind\":\"Unknown\"}") "Unknown case must fail."

        testCase "DYN-TA-T-003 duplicate gap and base mismatch keep last good and request resync" <| fun _ ->
            let state0 = RuntimeReducer.initial identity
            let state1, _ = RuntimeReducer.reduce state0 documentFrame
            let snapshot = frame RuntimeFrameKind.Snapshot 2L None 1L (RuntimePayload.Snapshot { Data = Map [ "series.price", SduiValue.Text "good" ]; Freshness = TaFreshness.Live })
            let state2, _ = RuntimeReducer.reduce state1 snapshot
            let duplicate, duplicateEffect = RuntimeReducer.reduce state2 snapshot
            Expect.equal duplicate state2 "Duplicate must be no-op."
            Expect.equal duplicateEffect RuntimeEffect.NoEffect "Duplicate effect must be none."

            let gap = frame RuntimeFrameKind.Heartbeat 4L None 1L (RuntimePayload.Heartbeat { ObservedAtUtc = DateTimeOffset.UtcNow })
            let gapState, gapEffect = RuntimeReducer.reduce state2 gap
            Expect.equal gapState.Data state2.Data "Gap must keep last-good data."
            Expect.equal gapState.Poll RuntimePollState.PausedForResync "Gap pauses polling."
            Expect.equal gapEffect (RuntimeEffect.RequestResync(identity.CanvasInstanceId, 1L)) "Gap requests resync."

            let badPatch = frame RuntimeFrameKind.Patch 3L (Some 0L) 2L (RuntimePayload.Patch { Operations = [||] })
            let badState, badEffect = RuntimeReducer.reduce state2 badPatch
            Expect.equal badState.Data state2.Data "Base mismatch keeps last good."
            Expect.equal badEffect (RuntimeEffect.RequestResync(identity.CanvasInstanceId, 1L)) "Base mismatch requests resync."

            let unknownPatch =
                frame RuntimeFrameKind.Patch 3L (Some 1L) 2L
                    (RuntimePayload.Patch { Operations = [| PatchOperation.ReplaceDataRef("series.unknown", SduiValue.Text "bad") |] })
            let unknownState, unknownEffect = RuntimeReducer.reduce state2 unknownPatch
            Expect.equal unknownState.Data state2.Data "Unknown dataRef must keep last-good data."
            Expect.equal unknownState.LastError.Value.ReasonCode "unknown-data-ref" "Unknown target should be visible as controlled error."
            Expect.equal unknownEffect (RuntimeEffect.RequestResync(identity.CanvasInstanceId, 1L)) "Unknown target requests resync."

        testCase "DYN-TA-T-004 ResetView is local and ResetCanvas submits once" <| fun _ ->
            let state0, _ = RuntimeReducer.reduce (RuntimeReducer.initial identity) documentFrame
            let changed = { state0 with View = { Values = Map [ "zoom", SduiValue.Number 9.0 ] } }
            let resetView, viewEffect = RuntimeReducer.resetView changed
            Expect.equal resetView.View.Values document.DefaultView "ResetView restores document defaults."
            Expect.equal viewEffect RuntimeEffect.NoEffect "ResetView must not send network effect."
            let _, canvasEffect = RuntimeReducer.resetCanvas changed
            Expect.equal canvasEffect (RuntimeEffect.SubmitAction(SduiAction.ResetCanvas identity.CanvasInstanceId)) "ResetCanvas submits one typed action."

        testCase "DYN-TA-T-008 and T-009 poll lifecycle is one-in-flight and disposed terminal" <| fun _ ->
            let mounted = RuntimePoll.mount RuntimePollState.Unmounted
            let ready = RuntimePoll.ready true true true mounted
            let inFlight, started = RuntimePoll.beginPoll ready
            let duplicate, startedTwice = RuntimePoll.beginPoll inFlight
            Expect.isTrue started "Ready starts one poll."
            Expect.isFalse startedTwice "Second poll is coalesced."
            Expect.equal duplicate RuntimePollState.PollInFlight "Only one poll remains in flight."
            Expect.equal (RuntimePoll.ready false true true (RuntimePoll.complete inFlight)) RuntimePollState.Suspended "Hidden surface suspends."
            let disposed = RuntimePoll.dispose inFlight
            Expect.equal (RuntimePoll.ready true true true disposed) RuntimePollState.Disposed "Disposed is terminal."

            let registry1, mountedState, _ = RuntimeRegistry.mount identity RuntimeRegistry.empty
            let registry2, duplicateState, duplicateEffect = RuntimeRegistry.mount identity registry1
            Expect.equal duplicateState mountedState "Duplicate mount should reuse state."
            Expect.equal duplicateEffect RuntimeEffect.NoEffect "Duplicate mount should have no effect."
            Expect.equal registry2.Instances.Count 1 "Registry keeps one instance."

            let disposedRegistry, disposeEffect = RuntimeRegistry.dispose identity.CanvasInstanceId registry2
            Expect.equal disposedRegistry.Instances.Count 0 "Dispose removes instance resources."
            Expect.equal disposeEffect RuntimeEffect.CancelPoll "Dispose cancels poll resources."

        testCase "DYN-TA-T-011 hard row and patch item limits fail" <| fun _ ->
            let tooManyRows = { document with Rows = Array.init 9 (fun index -> { row with RowId = $"r{index}"; DataRef = $"d{index}" }) }
            let tooManyItems =
                Array.init 501 (fun index -> Map [ "x", SduiValue.Number(float index) ])
                |> fun items -> { Operations = [| PatchOperation.UpsertSeriesPoints("series.price", "x", items) |] }

            Expect.isError (RuntimeValidation.validateFrame DynamicRuntimeDefaults.limits { documentFrame with Payload = RuntimePayload.Document tooManyRows }) "Rows hard limit must fail."
            Expect.isError (RuntimeValidation.validateFrame DynamicRuntimeDefaults.limits (frame RuntimeFrameKind.Patch 2L (Some 0L) 1L (RuntimePayload.Patch tooManyItems))) "Patch item hard limit must fail."

            let tooManyBars = Array.create 5001 SduiValue.Null |> SduiValue.Array
            let snapshot = RuntimePayload.Snapshot { Data = Map [ "series.price", tooManyBars ]; Freshness = TaFreshness.Live }
            Expect.isError (RuntimeValidation.validateFrame DynamicRuntimeDefaults.limits (frame RuntimeFrameKind.Snapshot 2L None 1L snapshot)) "Initial series hard limit must fail."

        testCase "DYN-TA-T-015 and T-018 contracts graph keeps only the approved browser bridge" <| fun _ ->
            let assembly = typeof<RuntimeFrame>.Assembly
            let names = assembly.GetReferencedAssemblies() |> Array.map _.Name |> Set.ofArray

            Expect.isTrue (Set.contains "WebSharper.Core" names) "Scheme 1 requires WebSharper metadata for the shared RuntimeFrame/reducer browser path."

            for forbidden in [ "PulseTrade.Comm.Spa"; "FAkka.FCell2"; "PulseTrade.MarketData.TAResearch.Contracts"; "Microsoft.Data.SqlClient" ] do
                Expect.isFalse (Set.contains forbidden names) $"Contracts must not reference {forbidden}."

            let source = IO.File.ReadAllText(IO.Path.Combine(__SOURCE_DIRECTORY__, "..", "src", "PulseTrade.Comm.Spa.Dynamic.Contracts", "RuntimeReducer.fs"))
            Expect.isFalse (source.Contains("JS.Inline", StringComparison.Ordinal)) "Reducer source must not use JS.Inline."

            let browserCodec = IO.File.ReadAllText(IO.Path.Combine(__SOURCE_DIRECTORY__, "..", "src", "PulseTrade.Comm.Spa.Dynamic.Contracts", "BrowserRuntimeCodec.fs"))
            Expect.stringContains browserCodec "Json.Serialize" "Browser bridge must use WebSharper typed JSON without a second wire DTO."
            Expect.stringContains browserCodec "Json.Deserialize<RuntimeFrame>" "Browser bridge must decode the existing RuntimeFrame type."

        testCase "DYN-TA-T-057 source envelope codec and validation are strict" <| fun _ ->
            let snapshotRoundtrip =
                sourceSnapshot
                |> SourceEnvelopeCodec.encodeSnapshot
                |> SourceEnvelopeCodec.decodeSnapshot DynamicRuntimeDefaults.limits

            Expect.equal snapshotRoundtrip (Ok sourceSnapshot) "Source snapshot must roundtrip."

            let event = sourceEvent 21L 10L 11L (SduiValue.Number 1.0)
            let eventRoundtrip =
                event
                |> SourceEnvelopeCodec.encodeEvent
                |> SourceEnvelopeCodec.decodeEvent DynamicRuntimeDefaults.limits

            Expect.equal eventRoundtrip (Ok event) "Source event must roundtrip."
            Expect.isError (SourceEnvelopeValidation.validateSnapshot { sourceSnapshot with Stream = { sourceStream with SourceId = "" } }) "Blank source identity must fail."
            Expect.isError (SourceEnvelopeValidation.validateSnapshot { sourceSnapshot with SourceRevision = -1L }) "Negative revision must fail."
            Expect.isError (SourceEnvelopeValidation.validateEvent { event with NewSourceRevision = event.BaseSourceRevision }) "Non-advancing revision must fail."
            Expect.isError (SourceEnvelopeValidation.validateEvent { event with EventTimeUtc = event.EventTimeUtc.ToOffset(TimeSpan.FromHours 8.0) }) "Non-UTC event time must fail."

            let unsafeEvent = { event with Payload = SduiValue.Object(Map [ "url", SduiValue.Text "https://example.invalid" ]) }
            Expect.isError (SourceEnvelopeValidation.validateEvent unsafeEvent) "Unsafe payload must fail."

            let tinyLimits = { DynamicRuntimeDefaults.limits with MaxFrameBytes = 10 }
            Expect.isError (SourceEnvelopeCodec.decodeSnapshot tinyLimits (SourceEnvelopeCodec.encodeSnapshot sourceSnapshot)) "Oversize envelope must fail before decode."

        testCase "DYN-TA-T-058 source projection applies owner reducer and ignores duplicates" <| fun _ ->
            let initial =
                SourceProjection.applySnapshot None sourceSnapshot
                |> Result.defaultWith (fun errors -> failwith (errors |> List.map _.Message |> String.concat "; "))

            let state =
                match initial with
                | SourceApplyResult.Applied value -> value
                | other -> failtestf "Expected applied snapshot, got %A" other

            let reduce current incoming =
                match current, incoming with
                | SduiValue.Object values, SduiValue.Number delta ->
                    match Map.tryFind "value" values with
                    | Some(SduiValue.Number value) -> Ok(SduiValue.Object(Map.add "value" (SduiValue.Number(value + delta)) values))
                    | _ -> Error "missing-value"
                | _ -> Error "shape-mismatch"

            let event = sourceEvent 21L 10L 11L (SduiValue.Number 2.0)
            let applied = SourceProjection.applyEvent reduce state event |> Result.defaultWith (fun errors -> failwith (errors |> List.map _.Message |> String.concat "; "))

            let next =
                match applied with
                | SourceApplyResult.Applied value -> value
                | other -> failtestf "Expected applied event, got %A" other

            Expect.equal next.SourceRevision 11L "Owner reducer advances source revision only after success."
            Expect.equal next.LastSequence 21L "Owner reducer advances source sequence."
            Expect.equal next.Payload (SduiValue.Object(Map [ "value", SduiValue.Number 12.0 ])) "Owner reducer controls payload transition."

            let duplicate = SourceProjection.applyEvent reduce next event |> Result.defaultWith (fun errors -> failwith (errors |> List.map _.Message |> String.concat "; "))
            Expect.equal duplicate (SourceApplyResult.Duplicate next) "Exact duplicate must be a no-op."

            let stale = sourceEvent 20L 9L 10L (SduiValue.Number 99.0)
            let staleResult = SourceProjection.applyEvent reduce next stale |> Result.defaultWith (fun errors -> failwith (errors |> List.map _.Message |> String.concat "; "))
            Expect.equal staleResult (SourceApplyResult.Duplicate next) "Older event must be a no-op."

        testCase "DYN-TA-T-059 gaps conflicts and reducer rejection preserve last good" <| fun _ ->
            let state = SourceProjection.stateOfSnapshot sourceSnapshot
            let reject _ _ = Error "owner-rejected"

            let expectResync expectedReason event =
                let result = SourceProjection.applyEvent reject state event |> Result.defaultWith (fun errors -> failwith (errors |> List.map _.Message |> String.concat "; "))

                match result with
                | SourceApplyResult.ResyncRequired(lastGood, request) ->
                    Expect.equal lastGood state "Resync must preserve last-good projection."
                    Expect.equal request.Reason expectedReason "Resync reason must be typed."
                | other -> failtestf "Expected resync, got %A" other

            expectResync SourceResyncReason.SequenceGap (sourceEvent 22L 10L 11L SduiValue.Null)
            expectResync SourceResyncReason.RevisionMismatch (sourceEvent 21L 9L 11L SduiValue.Null)
            expectResync (SourceResyncReason.DomainReducerRejected "owner-rejected") (sourceEvent 21L 10L 11L SduiValue.Null)

            let changedStream =
                { sourceEvent 21L 10L 11L SduiValue.Null with
                    Stream = { sourceStream with Epoch = "epoch-next" } }

            expectResync SourceResyncReason.StreamChanged changedStream

            let crossedSnapshot = { sourceSnapshot with SourceRevision = 11L; LastSequence = 19L }
            let crossed = SourceProjection.applySnapshot (Some state) crossedSnapshot |> Result.defaultWith (fun errors -> failwith (errors |> List.map _.Message |> String.concat "; "))

            match crossed with
            | SourceApplyResult.ResyncRequired(lastGood, request) ->
                Expect.equal lastGood state "Snapshot order conflict keeps last good."
                Expect.equal request.Reason SourceResyncReason.SnapshotOrderConflict "Snapshot order conflict is explicit."
            | other -> failtestf "Expected snapshot resync, got %A" other

        testCase "DYN-TA-T-060 source contract keeps owner dependencies out" <| fun _ ->
            let assemblyNames = typeof<SourceEventEnvelope>.Assembly.GetReferencedAssemblies() |> Array.map _.Name |> Set.ofArray

            for forbidden in [ "MdcQuote.Next.Client"; "SymbolicNet6.TradeCore.FsStl"; "SymbolicNet6"; "FAkka.FCell2"; "PulseTrade.Comm.Spa"; "Microsoft.Data.SqlClient" ] do
                Expect.isFalse (Set.contains forbidden assemblyNames) $"Source envelope must not reference owner package {forbidden}."
    ]
