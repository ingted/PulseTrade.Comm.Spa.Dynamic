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
      EditorSchemas = [||]
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

        testCase "DYN-TA-T-061 generic editor and correlated actions preserve authoritative document" <| fun _ ->
            let editorSchema =
                { TemplateKey = "ta.indicator"
                  DisplayName = "Technical indicator"
                  SchemaRevision = 3L
                  Fields =
                    [| { Key = "indicator"
                         Label = "Indicator"
                         Kind =
                            EditorValueKind.Choice
                                [| { Key = "sma"; Label = "SMA"; Value = SduiValue.Text "sma" }
                                   { Key = "macd"; Label = "MACD"; Value = SduiValue.Text "macd" } |]
                         Required = true
                         DefaultValue = Some(SduiValue.Text "sma") }
                       { Key = "scales"
                         Label = "Scales"
                         Kind = EditorValueKind.List(EditorValueKind.Scale [| "1k"; "5k"; "30k" |], Some 1, Some 3)
                         Required = true
                         DefaultValue = Some(SduiValue.Array [| SduiValue.Text "1k"; SduiValue.Text "5k" |]) }
                       { Key = "parameters"
                         Label = "Parameters"
                         Kind =
                            EditorValueKind.Group
                                [| { Key = "period"; Label = "Period"; Kind = EditorValueKind.Integer(Some 1L, Some 500L); Required = true; DefaultValue = Some(SduiValue.Number 13.0) }
                                   { Key = "alpha"; Label = "Alpha"; Kind = EditorValueKind.Decimal(Some 0.0, Some 1.0); Required = false; DefaultValue = Some(SduiValue.Number 0.5) }
                                   { Key = "visible"; Label = "Visible"; Kind = EditorValueKind.Boolean; Required = true; DefaultValue = Some(SduiValue.Bool true) } |]
                         Required = true
                         DefaultValue =
                            Some(
                                SduiValue.Object
                                    (Map
                                        [ "period", SduiValue.Number 13.0
                                          "alpha", SduiValue.Number 0.5
                                          "visible", SduiValue.Bool true ])) } |] }

            Expect.isOk (DynamicEditorValidation.validateSchema DynamicEditorDefaults.limits editorSchema) "List/group/choice editor schema should validate."
            let editorDefaults = DynamicEditorValidation.defaultInputs editorSchema
            Expect.sequenceEqual
                (editorDefaults |> Array.map _.Path)
                [| "indicator"; "scales[0]"; "scales[1]"; "parameters.period"; "parameters.alpha"; "parameters.visible" |]
                "Defaults should flatten list/group values into stable input paths."
            Expect.isOk (DynamicEditorValidation.validateInputs DynamicEditorDefaults.limits editorSchema editorDefaults) "Flattened defaults should validate against the same schema."
            let decodedSchema =
                editorSchema
                |> DynamicTemplateSchemaCodec.toValue
                |> DynamicTemplateSchemaCodec.fromValue
                |> Result.defaultWith (fun errors -> failtest (errors |> List.map _.Message |> String.concat "; "))
            Expect.equal decodedSchema editorSchema "Template schema must round-trip through the transport-neutral SDUI value codec."

            let documentWithSchema = { document with EditorSchemas = [| editorSchema |] }
            Expect.isOk
                (RuntimeValidation.validateFrame DynamicRuntimeDefaults.limits { documentFrame with Payload = RuntimePayload.Document documentWithSchema })
                "A document must atomically validate its editor schema catalog with its rows."
            let duplicateSchemaDocument = { documentWithSchema with EditorSchemas = [| editorSchema; editorSchema |] }
            Expect.isError
                (RuntimeValidation.validateFrame DynamicRuntimeDefaults.limits { documentFrame with Payload = RuntimePayload.Document duplicateSchemaDocument })
                "Duplicate template keys in one document revision must fail closed."

            let editorBinding =
                { TemplateKey = editorSchema.TemplateKey
                  Values = editorDefaults }

            let boundRow =
                TaRowEditorBinding.attach editorBinding row
                |> Result.defaultWith (fun errors -> failtest (errors |> List.map _.Message |> String.concat "; "))

            let resolvedBinding =
                TaRowEditorBinding.tryResolve [| editorSchema |] boundRow
                |> Result.defaultWith (fun errors -> failtest (errors |> List.map _.Message |> String.concat "; "))
                |> Option.defaultWith (fun () -> failtest "Bound rows must expose a reconfigure editor binding.")

            Expect.equal (fst resolvedBinding).TemplateKey editorSchema.TemplateKey "Editor binding must retain the authoritative template key."
            Expect.sequenceEqual (snd resolvedBinding) editorDefaults "Editor binding must round-trip all typed flat values."
            Expect.equal
                (TaRowEditorBinding.tryFind row)
                (Ok None)
                "Legacy rows without editor binding metadata must remain valid and read-only."

            let invalidBindingRow =
                { row with
                    Options = row.Options |> Map.add TaRowEditorBinding.OptionKey (SduiValue.Text "invalid") }

            Expect.isError
                (TaRowEditorBinding.tryResolve [| editorSchema |] invalidBindingRow)
                "Malformed editor binding metadata must fail closed instead of opening an empty editor."

            let duplicateFieldSchema =
                { editorSchema with
                    Fields = [| editorSchema.Fields[0]; editorSchema.Fields[0] |] }

            Expect.isError (DynamicEditorValidation.validateSchema DynamicEditorDefaults.limits duplicateFieldSchema) "Duplicate editor keys must fail closed."

            let applyTemplate =
                SduiAction.ApplyTemplate(identity.CanvasInstanceId, None, editorSchema.TemplateKey, editorDefaults)

            Expect.isEmpty (DynamicActionValidation.actionErrors applyTemplate) "Generic template action should accept typed flat editor values."
            let duplicateInputs = Array.append editorDefaults [| editorDefaults[0] |]
            Expect.isNonEmpty
                (DynamicActionValidation.actionErrors (SduiAction.ApplyTemplate(identity.CanvasInstanceId, None, editorSchema.TemplateKey, duplicateInputs)))
                "Duplicate editor input paths must fail closed."

            let sma13 =
                { row with
                    RowId = "sma-13"
                    Kind = TaRowKind.Sma
                    DataRef = "series.sma-13" }

            let sma21 =
                { row with
                    RowId = "sma-21"
                    Kind = TaRowKind.Sma
                    DataRef = "series.sma-21" }

            let stableDocument = { document with Rows = [| sma13; sma21 |] }
            Expect.isOk (RuntimeValidation.validateFrame DynamicRuntimeDefaults.limits { documentFrame with Payload = RuntimePayload.Document stableDocument }) "Same template kind with different stable row ids should coexist."

            let duplicateRowDocument = { stableDocument with Rows = [| sma13; { sma21 with RowId = sma13.RowId } |] }
            Expect.isError (RuntimeValidation.validateFrame DynamicRuntimeDefaults.limits { documentFrame with Payload = RuntimePayload.Document duplicateRowDocument }) "Duplicate stable row ids must fail closed."

            let originalDocument = stableDocument
            let request =
                { RequestId = "remove-sma-13"
                  ExpectedDocumentRevision = Some 7L
                  Action = SduiAction.RemoveTaRow(identity.CanvasInstanceId, sma13.RowId) }

            let pending =
                DynamicActionLifecycle.beginRequest 7L request DynamicActionLifecycle.empty
                |> Result.defaultWith (fun errors -> failwith (errors |> List.map _.Message |> String.concat "; "))

            Expect.equal pending.Pending (Some request) "Accepted request enters pending exactly once."
            Expect.isError (DynamicActionLifecycle.beginRequest 7L request pending) "A second action must not replace an in-flight request."

            let rejected = DynamicActionResult.Rejected(request.RequestId, "provider-unavailable", "Provider is unavailable.")
            let afterReject =
                DynamicActionLifecycle.complete rejected pending
                |> Result.defaultWith (fun errors -> failwith (errors |> List.map _.Message |> String.concat "; "))

            Expect.equal afterReject.Pending None "Rejected result clears the matching pending request."
            Expect.equal afterReject.LastResult (Some rejected) "Rejected result remains available for bounded UI feedback."
            Expect.equal stableDocument originalDocument "Action lifecycle must not mutate the authoritative document."

            let conflict =
                DynamicActionLifecycle.beginRequest 8L request DynamicActionLifecycle.empty
                |> Result.defaultWith (fun errors -> failwith (errors |> List.map _.Message |> String.concat "; "))

            Expect.equal conflict.Pending None "Revision conflict must not submit an action."
            Expect.equal conflict.LastResult (Some(DynamicActionResult.RevisionConflict(request.RequestId, 8L))) "Conflict reports the actual revision."

            let acceptedRequest = { request with RequestId = "remove-sma-21"; Action = SduiAction.RemoveTaRow(identity.CanvasInstanceId, sma21.RowId) }
            let acceptedPending =
                DynamicActionLifecycle.beginRequest 7L acceptedRequest DynamicActionLifecycle.empty
                |> Result.defaultWith (fun errors -> failwith (errors |> List.map _.Message |> String.concat "; "))
            let acceptedResult = DynamicActionResult.Accepted(acceptedRequest.RequestId, 8L)
            let acceptedState =
                DynamicActionLifecycle.complete acceptedResult acceptedPending
                |> Result.defaultWith (fun errors -> failwith (errors |> List.map _.Message |> String.concat "; "))

            Expect.equal acceptedState.LastResult (Some acceptedResult) "Accepted result correlates and clears pending."
            Expect.isError (DynamicActionLifecycle.complete rejected acceptedPending) "Mismatched result id must preserve pending state through a controlled error."

        testCase "DYN-TA-T-063 browser action wire round-trips correlated request and results" <| fun _ ->
            let request =
                { RequestId = "interactive:42"
                  ExpectedDocumentRevision = Some 7L
                  Action = SduiAction.RemoveTaRow(identity.CanvasInstanceId, "sma-13") }

            let requestJson = BrowserRuntimeCodec.encodeActionRequest request
            let decodedRequest =
                BrowserRuntimeCodec.decodeActionRequest requestJson
                |> Result.defaultWith failtest

            Expect.equal decodedRequest request "Action request envelope must preserve request id, expected revision and action."

            [ DynamicActionResult.Accepted(request.RequestId, 8L)
              DynamicActionResult.Rejected(request.RequestId, "provider-unavailable", "Provider is unavailable.")
              DynamicActionResult.RevisionConflict(request.RequestId, 9L) ]
            |> List.iter (fun expected ->
                let actual =
                    expected
                    |> BrowserRuntimeCodec.encodeActionResult
                    |> BrowserRuntimeCodec.decodeActionResult
                    |> Result.defaultWith failtest

                Expect.equal actual expected "Action result envelope must preserve every correlated result case.")

            let badProtocol =
                { DynamicActionWireDefaults.requestFrame request with Protocol = "ptcs-dynamic-action.v0" }

            Expect.isError (DynamicActionValidation.clientFrameErrors badProtocol |> function [] -> Ok() | errors -> Error errors) "Unsupported action protocol must fail closed."
            Expect.isError (BrowserRuntimeCodec.decodeActionRequest (WebSharper.Json.Serialize badProtocol)) "Browser codec must reject unsupported action protocol."

            let missingRequestId =
                { request with RequestId = "" }

            Expect.isError (BrowserRuntimeCodec.decodeActionRequest (BrowserRuntimeCodec.encodeActionRequest missingRequestId)) "Browser codec must reject an uncorrelatable request."

            let badKind =
                { DynamicActionWireDefaults.resultFrame (DynamicActionResult.Accepted(request.RequestId, 8L)) with Kind = "runtime-frame" }

            Expect.isError (DynamicActionValidation.serverFrameErrors badKind |> function [] -> Ok() | errors -> Error errors) "Wrong action result kind must fail closed."
            Expect.isError (BrowserRuntimeCodec.decodeActionResult (WebSharper.Json.Serialize badKind)) "Browser codec must reject wrong action result kind."

        testCase "DYN-TA-T-062 temporal point codec preserves causal presentation evidence" <| fun _ ->
            let intervalStart = DateTimeOffset.Parse("2026-09-03T13:00:00Z")
            let intervalEnd = DateTimeOffset.Parse("2026-09-03T13:05:00Z")
            let point =
                { SourceIntervalId = "es-5k:20260903T1300Z"
                  ScaleKey = "5K"
                  IntervalStartUtc = intervalStart
                  IntervalEndUtc = intervalEnd
                  ObservedThroughUtc = intervalEnd
                  AvailableAtUtc = Some(intervalEnd.AddMilliseconds 25.0)
                  Finality = PointFinality.Final
                  Projection = TemporalProjection.RepeatAcrossBaseBuckets
                  Quality = Some "complete"
                  Value = Some(SduiValue.Object(Map [ "t", SduiValue.Text "2026-09-03T13:00:00Z"; "v", SduiValue.Number 42.0 ])) }

            let encoded = TemporalPointCodec.encode point
            let decoded =
                TemporalPointCodec.decode encoded
                |> Result.defaultWith (fun errors -> failwith (errors |> List.map _.Message |> String.concat "; "))

            Expect.equal decoded point "Temporal point wire must round-trip interval, frontier, availability, finality, projection, quality and payload."

        testCase "DYN-TA-T-062 temporal point validation rejects non-causal final and availability" <| fun _ ->
            let intervalStart = DateTimeOffset.Parse("2026-09-03T13:00:00Z")
            let intervalEnd = DateTimeOffset.Parse("2026-09-03T13:05:00Z")
            let invalid =
                { SourceIntervalId = "es-5k:20260903T1300Z"
                  ScaleKey = "5K"
                  IntervalStartUtc = intervalStart
                  IntervalEndUtc = intervalEnd
                  ObservedThroughUtc = intervalEnd.AddMinutes(-1.0)
                  AvailableAtUtc = Some intervalStart
                  Finality = PointFinality.Final
                  Projection = TemporalProjection.CandleSpan
                  Quality = Some "complete"
                  Value = None }

            let errors =
                TemporalPointCodec.validate invalid
                |> function
                    | Error values -> values
                    | Ok _ -> failtest "Invalid causal metadata must be rejected."

            Expect.isTrue (errors |> List.exists (fun error -> error.Code = "invalid-final-frontier")) "Final points must reach interval end."
            Expect.isTrue (errors |> List.exists (fun error -> error.Code = "invalid-availability")) "Availability cannot precede observed frontier."
    ]
