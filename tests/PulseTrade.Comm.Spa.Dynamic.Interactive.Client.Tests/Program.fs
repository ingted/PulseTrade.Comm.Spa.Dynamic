module PulseTrade.Comm.Spa.Dynamic.Interactive.Client.Tests

open System
open System.Collections.Generic
open Expecto
open PulseTrade.Comm.Spa.Dynamic.Contracts
open PulseTrade.Comm.Spa.Dynamic.Interactive.Client

let options =
    { InteractiveClientLifecycle.defaults with
        ReconnectBaseMs = 1000
        ReconnectMaximumMs = 8000 }

let framePumpIdentity =
    { DocumentId = DocumentId "frame-pump-document"
      CanvasInstanceId = CanvasInstanceId "frame-pump-canvas" }

let framePumpDocument =
    { WorkspaceId = "frame-pump-workspace"
      Title = "Frame pump"
      RowsRef = "ta.rows"
      StatusRef = "ta.status"
      SharedTimeAxis = true
      TemporalAxisRefs = [||]
      BaseRowId = None
      Rows = [||]
      EditorSchemas = [||]
      AllowedActions = [||]
      DefaultView = Map.empty }

let documentFrame =
    { Protocol = DynamicRuntimeDefaults.protocol
      Kind = RuntimeFrameKind.Document
      DocumentId = framePumpIdentity.DocumentId
      CanvasInstanceId = framePumpIdentity.CanvasInstanceId
      DocumentRevision = 1L
      BaseDataRevision = None
      DataRevision = 0L
      TransportSequence = 1L
      Payload = RuntimePayload.Document framePumpDocument }

let heartbeat sequence =
    { Protocol = DynamicRuntimeDefaults.protocol
      Kind = RuntimeFrameKind.Heartbeat
      DocumentId = framePumpIdentity.DocumentId
      CanvasInstanceId = framePumpIdentity.CanvasInstanceId
      DocumentRevision = 1L
      BaseDataRevision = None
      DataRevision = 0L
      TransportSequence = sequence
      Payload = RuntimePayload.Heartbeat { ObservedAtUtc = DateTimeOffset.UtcNow } }

let snapshotFrame sequence =
    { Protocol = DynamicRuntimeDefaults.protocol
      Kind = RuntimeFrameKind.Snapshot
      DocumentId = framePumpIdentity.DocumentId
      CanvasInstanceId = framePumpIdentity.CanvasInstanceId
      DocumentRevision = 1L
      BaseDataRevision = None
      DataRevision = 1L
      TransportSequence = sequence
      Payload =
        RuntimePayload.Snapshot
            { Data =
                Map [ "ta.rows", SduiValue.Array [| SduiValue.Number 1.0 |]
                      "ta.status", SduiValue.Array [| SduiValue.Number 2.0 |] ]
              Freshness = TaFreshness.Live } }

// Synthetic market values belong only to this Test project.
let cacheReuseFixture includePreview =
    let start = DateTimeOffset(2026, 11, 1, 0, 0, 0, TimeSpan.Zero)
    let count = if includePreview then 1501 else 1500
    let axis: TemporalAxis =
        { AxisRef = "axis.cache"
          Revision = 1L
          Points =
            Array.init count (fun index ->
                let last = start.AddMinutes(float (index + 1))
                { Position = int64 index
                  SourceIntervalId = $"cache-fixture-{index}"
                  ScaleKey = "1K"
                  IntervalStartUtc = last.AddMinutes -1.0
                  IntervalEndUtc = last
                  EventTimeUtc = Some last
                  ObservedThroughUtc = last
                  AvailableAtUtc = Some last
                  Finality = if index = 1500 then PointFinality.Preview else PointFinality.Final
                  Projection = TemporalProjection.CandleSpan
                  Quality = None }) }
    let series: TemporalSeries =
        { AxisRef = axis.AxisRef
          AxisRevision = axis.Revision
          Points = Array.init count (fun i -> { Position = int64 i; Value = SduiValue.Number(float i) }) }
    let coverage: TaLoadedCoverageProjection =
        { CoverageIdentity = "cache-fixture"
          CoverageRevision = 1L
          QueryGeneration = 1L
          Completeness = TaCoverageCompleteness.Complete
          TotalObservationCount = Some(int64 count)
          Segments =
            [| { SegmentId = "resident"
                 StartEventTimeUtc = start.ToString("O")
                 EndEventTimeExclusiveUtc = start.AddMinutes(float count).ToString("O")
                 StartObservationOrdinal = 0L
                 ObservationCount = int64 count } |]
          OverviewAxisRef = axis.AxisRef
          OverviewAnchors = [||]
          ActiveDetail = { StartObservationOrdinal = 0L; ObservationCount = 1500; BaseAxisRef = axis.AxisRef }
          GlobalOverview =
            Some { TotalObservationCount = int64 count
                   AxisRef = axis.AxisRef
                   Anchors = [||]
                   ResidentStartObservationOrdinal = 0L
                   ResidentObservationCount = count } }
    let document =
        { framePumpDocument with
            TemporalAxisRefs = [| axis.AxisRef |]
            Rows =
                [| { RowId = "cache-series"; Kind = TaRowKind.Sma; DataRef = "series.cache"
                     HeightWeight = 1.0; Visible = true; Options = Map.empty; Traces = [||] } |]
            DefaultView = Map.empty |> TaLoadedCoverageCodec.applyDataRef "coverage.cache" }
    let mounted, _ =
        RuntimeReducer.reduce (RuntimeReducer.initial framePumpIdentity)
            { documentFrame with Payload = RuntimePayload.Document document }
    let state, _ =
        RuntimeReducer.reduce mounted
            { snapshotFrame 2L with
                Payload = RuntimePayload.Snapshot
                    { Data = Map [ document.RowsRef, SduiValue.Array [||]
                                   document.StatusRef, SduiValue.Array [||]
                                   axis.AxisRef, TemporalAxisCodec.encode axis
                                   "series.cache", TemporalSeriesCodec.encode series
                                   "coverage.cache", TaLoadedCoverageCodec.encode coverage ]
                      Freshness = TaFreshness.Live } }
    Expect.isNone state.LastError "The fixture must pass the canonical document/snapshot reducer."
    Expect.equal state.DataRevision 1L "The fixture snapshot must be accepted."
    state

let cacheReuseIdentity =
    { OwnerFingerprint = "cache-fixture-owner"; SchemaRevision = RuntimeCacheProjection.CurrentSchemaRevision }

let cacheReuseCoverage (state: RuntimeState) =
    TaLoadedCoverageCodec.tryDecodeResolved state.Document.Value.DefaultView state.Data
    |> Result.defaultWith (fun errors -> failtestf "Invalid fixture coverage: %A" errors)
    |> Option.defaultWith (fun () -> failtest "Missing fixture coverage")

let cacheReusePatch (state: RuntimeState) start count =
    let previous = cacheReuseCoverage state
    let coverage =
        { previous with
            CoverageRevision = previous.CoverageRevision + 1L
            QueryGeneration = previous.QueryGeneration + 1L
            ActiveDetail = { previous.ActiveDetail with StartObservationOrdinal = start; ObservationCount = count } }
    let next, _ =
        RuntimeReducer.reduce state
            { documentFrame with
                Kind = RuntimeFrameKind.Patch
                BaseDataRevision = Some state.DataRevision
                DataRevision = state.DataRevision + 1L
                TransportSequence = state.LastTransportSequence + 1L
                Payload = RuntimePayload.Patch
                    { Operations = [| PatchOperation.ReplaceDataRef("coverage.cache", TaLoadedCoverageCodec.encode coverage) |] } }
    Expect.isNone next.LastError "A coverage-only patch must pass the canonical reducer."
    Expect.equal next.DataRevision (state.DataRevision + 1L) "The patch must commit."
    next

let cacheReuseEncodeValue (value: SduiValue) =
    System.Text.Json.JsonSerializer.Serialize(value, RuntimeCodec.options)

let cacheReuseWriter () =
    let scheduled = Queue<unit -> unit>()
    let pending = ResizeArray<BrowserRuntimeCacheRecord * (BrowserRuntimeCacheWriteResult -> unit)>()
    let projected = ref 0
    let encoded = ResizeArray<SduiValue>()
    let hits = ResizeArray<bool>()
    let now = ref (DateTimeOffset(2026, 11, 7, 0, 0, 0, TimeSpan.Zero))
    let project capturedAt identity state =
        projected.Value <- projected.Value + 1
        RuntimeCacheProjection.tryCreateEntry capturedAt identity state
    let writer =
        BrowserRuntimeCache.createPhasedWriter
            scheduled.Enqueue (fun () -> now.Value)
            (BrowserRuntimeCache.tryPrepareEntry project)
            (fun reused key ticks entry ->
                hits.Add(Option.isSome reused)
                BrowserRuntimeCache.recordForEntryWith RuntimeCacheCodec.encode
                    (fun value -> encoded.Add value; cacheReuseEncodeValue value) reused key ticks entry)
            (fun record complete -> pending.Add(record, complete))
    writer, scheduled, pending, projected, encoded, hits, now

let cacheReuseDrain (scheduled: Queue<unit -> unit>) =
    while scheduled.Count > 0 do scheduled.Dequeue() ()

let cacheReuseChangeCoverage change (state: RuntimeState) =
    { state with Data = Map.add "coverage.cache" (TaLoadedCoverageCodec.encode (change (cacheReuseCoverage state))) state.Data }

let cacheReuseCloneValue value =
    System.Text.Json.JsonSerializer.Deserialize<SduiValue>(cacheReuseEncodeValue value, RuntimeCodec.options)

let cacheReuseTests =
    testList "cache coverage reuse" [
        testCase "public phased writer preserves its four-argument CLR method" <| fun _ ->
            let cacheModule = typeof<BrowserRuntimeCachePreparedWrite>.Assembly.GetType("PulseTrade.Comm.Spa.Dynamic.Interactive.Client.BrowserRuntimeCache")
            let methodInfo = cacheModule.GetMethod("writeAcceptedStatePhased")
            Expect.isNotNull methodInfo "Existing compiled consumers require the method, not a new function-valued property."
            Expect.equal (methodInfo.GetParameters().Length) 4 "The original callable boundary must remain intact."

        for preview in [ false; true ] do
            testCase (if preview then "projected finals survive raw preview reuse" else "resize and pan reuse unchanged projected items") <| fun _ ->
                let initial = cacheReuseFixture preview
                let writer, scheduled, pending, projected, encoded, hits, now = cacheReuseWriter ()
                writer cacheReuseIdentity initial (fun () -> true) ignore
                cacheReuseDrain scheduled
                Expect.equal projected.Value 1 "Cold write must project once."
                snd pending[0] BrowserRuntimeCacheWriteResult.Written
                let mutable previousRecord = fst pending[0]
                let mutable state = initial
                for start in [ 0L; 100L ] do
                    state <- cacheReusePatch state start 48
                    now.Value <- now.Value.AddSeconds 1.0
                    projected.Value <- 0
                    encoded.Clear()
                    writer cacheReuseIdentity state (fun () -> true) ignore
                    cacheReuseDrain scheduled
                    let record, complete = pending[pending.Count - 1]
                    let slowEntry = RuntimeCacheProjection.tryCreateEntry now.Value cacheReuseIdentity state |> Result.defaultWith (fun e -> failtestf "%A" e)
                    let slowRecord = BrowserRuntimeCache.recordForEntryWith RuntimeCacheCodec.encode cacheReuseEncodeValue None record.Key record.TouchedAtTicks slowEntry
                    Expect.equal record slowRecord "Full and reused records, including current header and coverage, must be equivalent."
                    Expect.equal projected.Value 0 "An accepted metadata-only patch must skip full temporal projection."
                    Expect.equal encoded.Count 1 "Only coverage may be encoded again."
                    Expect.isTrue hits[hits.Count - 1] "The writer must report a reuse hit."
                    for item in record.DataItems do
                        let old = previousRecord.DataItems |> Array.find (fun old -> old.DataRef = item.DataRef)
                        Expect.equal (Object.ReferenceEquals(item, old)) (item.DataRef <> "coverage.cache") "Unchanged item objects must be retained."
                    let header = System.Text.Json.JsonSerializer.Deserialize<RuntimeCacheEntry>(record.EntryHeaderJson, RuntimeCodec.options)
                    let entry =
                        { header with Snapshot = { header.Snapshot with Data = record.DataItems |> Array.map (fun item -> item.DataRef, System.Text.Json.JsonSerializer.Deserialize<SduiValue>(item.ValueJson, RuntimeCodec.options)) |> Map.ofArray } }
                    let axis = TemporalAxisCodec.decode entry.Snapshot.Data["axis.cache"] |> Result.defaultWith (fun e -> failtestf "%A" e)
                    let series = TemporalSeriesCodec.decode entry.Snapshot.Data["series.cache"] |> Result.defaultWith (fun e -> failtestf "%A" e)
                    Expect.equal axis.Points.Length 1500 "Preview axis points must never reappear."
                    Expect.equal series.Points.Length 1500 "Preview series values must never reappear."
                    let hydrated = BrowserRuntimeCache.tryRehydrate cacheReuseIdentity state entry |> Result.defaultWith (fun e -> failtestf "Rehydrate rejected: %A" e)
                    Expect.equal (cacheReuseCoverage hydrated).ActiveDetail (cacheReuseCoverage state).ActiveDetail "Rehydrate must retain the newest viewport."
                    complete BrowserRuntimeCacheWriteResult.Written
                    previousRecord <- record

        let replaceData key change (state: RuntimeState) =
            { state with Data = Map.add key (change state.Data[key]) state.Data }
        let replaceDocument change (state: RuntimeState) = { state with Document = state.Document |> Option.map change }
        let guardMisses: (string * (RuntimeState -> RuntimeState)) list =
            [ "equal axis with new reference", replaceData "axis.cache" cacheReuseCloneValue
              "equal series with new reference", replaceData "series.cache" cacheReuseCloneValue
              "non-temporal metadata new reference", replaceData "ta.status" cacheReuseCloneValue
              "finality changed", replaceData "axis.cache" (fun value ->
                  let axis = TemporalAxisCodec.decode value |> Result.defaultWith (fun e -> failtestf "%A" e)
                  TemporalAxisCodec.encode { axis with Points = axis.Points |> Array.mapi (fun i p -> if i = 1499 then { p with Finality = PointFinality.Preview } else p) })
              "series changed", replaceData "series.cache" (fun value ->
                  let series = TemporalSeriesCodec.decode value |> Result.defaultWith (fun e -> failtestf "%A" e)
                  TemporalSeriesCodec.encode { series with Points = series.Points |> Array.mapi (fun i p -> if i = 0 then { p with Value = SduiValue.Number -1.0 } else p) })
              "marker key added", fun s -> { s with Data = Map.add "marker.extra" (SduiValue.Array [||]) s.Data }
              "key removed", fun s -> { s with Data = Map.remove "ta.status" s.Data }
              "equal-size key replacement", fun s -> { s with Data = s.Data |> Map.remove "ta.status" |> Map.add "marker.extra" (SduiValue.Array [||]) }
              "runtime document identity", fun s -> { s with Identity = { s.Identity with DocumentId = DocumentId "new-document" } }
              "canvas identity", fun s -> { s with Identity = { s.Identity with CanvasInstanceId = CanvasInstanceId "new-canvas" } }
              "document revision", fun s -> { s with DocumentRevision = s.DocumentRevision + 1L }
              "document object", replaceDocument (fun d -> { d with Title = d.Title })
              "workspace", replaceDocument (fun d -> { d with WorkspaceId = "new-workspace" })
              "missing document", fun s -> { s with Document = None }
              "coverage identity", cacheReuseChangeCoverage (fun c -> { c with CoverageIdentity = "new-coverage" })
              "resident segments", cacheReuseChangeCoverage (fun c -> { c with Segments = c.Segments |> Array.map (fun s -> { s with SegmentId = "refilled" }) })
              "resident start", cacheReuseChangeCoverage (fun c -> { c with GlobalOverview = c.GlobalOverview |> Option.map (fun g -> { g with ResidentStartObservationOrdinal = 1L; ResidentObservationCount = 1499 }) })
              "resident count", cacheReuseChangeCoverage (fun c -> { c with GlobalOverview = c.GlobalOverview |> Option.map (fun g -> { g with ResidentObservationCount = 1499 }) })
              "global coverage removed", cacheReuseChangeCoverage (fun c -> { c with GlobalOverview = None })
              "base axis", cacheReuseChangeCoverage (fun c -> { c with ActiveDetail = { c.ActiveDetail with BaseAxisRef = "axis.other" } })
              "malformed coverage", replaceData "coverage.cache" (fun _ -> SduiValue.Null)
              "coverage ref removed", fun s -> { s with Data = Map.remove "coverage.cache" s.Data } ]
        for name, change in guardMisses do
            testCase ("fallback: " + name) <| fun _ ->
                let state = cacheReuseFixture false
                let writer, scheduled, pending, projected, _, hits, _ = cacheReuseWriter ()
                writer cacheReuseIdentity state (fun () -> true) ignore
                cacheReuseDrain scheduled
                snd pending[0] BrowserRuntimeCacheWriteResult.Written
                projected.Value <- 0
                writer cacheReuseIdentity (change (cacheReusePatch state 100L 48)) (fun () -> true) ignore
                cacheReuseDrain scheduled
                Expect.equal projected.Value 1 "Every failed guard must run the original full projector."
                Expect.isTrue (hits |> Seq.forall not) "A fallback must never be reported as reuse."

        for name, identity in
            [ "owner", { cacheReuseIdentity with OwnerFingerprint = "other-owner" }
              "schema", { cacheReuseIdentity with SchemaRevision = cacheReuseIdentity.SchemaRevision + 1L } ] do
            testCase ("fallback: " + name) <| fun _ ->
                let state = cacheReuseFixture false
                let writer, scheduled, pending, projected, _, hits, _ = cacheReuseWriter ()
                writer cacheReuseIdentity state (fun () -> true) ignore
                cacheReuseDrain scheduled
                snd pending[0] BrowserRuntimeCacheWriteResult.Written
                projected.Value <- 0
                writer identity (cacheReusePatch state 0L 48) (fun () -> true) ignore
                cacheReuseDrain scheduled
                Expect.equal projected.Value 1 "Changed cache identity must project independently."
                Expect.isTrue (hits |> Seq.forall not) "Identity changes cannot reuse previous records."

        for inlineCoverage in [ false; true ] do
            testCase (if inlineCoverage then "inline coverage remains on full path" else "missing data-ref remains on full path") <| fun _ ->
                let initial = cacheReuseFixture false
                let view = if inlineCoverage then TaLoadedCoverageCodec.apply (cacheReuseCoverage initial) Map.empty else Map.empty
                let state = { initial with Document = Some { initial.Document.Value with DefaultView = view } }
                let writer, scheduled, pending, projected, _, hits, _ = cacheReuseWriter ()
                writer cacheReuseIdentity state (fun () -> true) ignore
                cacheReuseDrain scheduled
                snd pending[0] BrowserRuntimeCacheWriteResult.Written
                writer cacheReuseIdentity state (fun () -> true) ignore
                cacheReuseDrain scheduled
                Expect.equal projected.Value 2 "Only a valid stable coverage data-ref is eligible."
                Expect.isTrue (hits |> Seq.forall not) "Inline/absent coverage cannot reuse."

        for stage in [ 1; 2 ] do
            testCase ($"cancellation before stage {stage} neither writes nor promotes") <| fun _ ->
                let state = cacheReuseFixture false
                let writer, scheduled, pending, projected, encoded, _, _ = cacheReuseWriter ()
                let mutable current = true
                let outcomes = ResizeArray<BrowserRuntimeCachePhasedWriteOutcome>()
                writer cacheReuseIdentity state (fun () -> current) outcomes.Add
                if stage = 2 then scheduled.Dequeue() ()
                current <- false
                cacheReuseDrain scheduled
                Expect.sequenceEqual outcomes [ BrowserRuntimeCachePhasedWriteOutcome.CancelledBeforeWrite ] "Cancellation must complete once."
                Expect.isEmpty pending "Cancellation must not reach IndexedDB."
                Expect.isEmpty encoded "Cancellation must not encode the record."
                projected.Value <- 0
                writer cacheReuseIdentity state (fun () -> true) ignore
                cacheReuseDrain scheduled
                Expect.equal projected.Value 1 "Cancelled preparation must not seed the memo."

        for reason in [ "indexeddb-unavailable"; "indexeddb-write-aborted"; "indexeddb-write-failed" ] do
            testCase ("failed write does not promote: " + reason) <| fun _ ->
                let state = cacheReuseFixture false
                let writer, scheduled, pending, projected, _, _, _ = cacheReuseWriter ()
                let outcomes = ResizeArray<BrowserRuntimeCachePhasedWriteOutcome>()
                writer cacheReuseIdentity state (fun () -> true) outcomes.Add
                cacheReuseDrain scheduled
                snd pending[0] (BrowserRuntimeCacheWriteResult.Unavailable reason)
                Expect.sequenceEqual outcomes [ BrowserRuntimeCachePhasedWriteOutcome.Completed(BrowserRuntimeCacheAcceptedStateWriteResult.Unavailable reason) ] "Failures must remain failures."
                projected.Value <- 0
                writer cacheReuseIdentity (cacheReusePatch state 0L 48) (fun () -> true) ignore
                cacheReuseDrain scheduled
                Expect.equal projected.Value 1 "A failed write must not seed reuse."

        testCase "reverse write completion cannot replace the newest successful memo" <| fun _ ->
            let state = cacheReuseFixture false
            let newer = replaceData "ta.status" (fun _ -> SduiValue.Array [| SduiValue.Text "newer" |]) state
            let writer, scheduled, pending, projected, _, _, _ = cacheReuseWriter ()
            writer cacheReuseIdentity state (fun () -> true) ignore
            cacheReuseDrain scheduled
            writer cacheReuseIdentity newer (fun () -> true) ignore
            cacheReuseDrain scheduled
            snd pending[1] BrowserRuntimeCacheWriteResult.Written
            snd pending[0] BrowserRuntimeCacheWriteResult.Written
            projected.Value <- 0
            writer cacheReuseIdentity (cacheReusePatch newer 100L 48) (fun () -> true) ignore
            cacheReuseDrain scheduled
            Expect.equal projected.Value 0 "The newer completed record must remain the reusable authority."
            let item record = record.DataItems |> Array.find (fun i -> i.DataRef = "ta.status")
            Expect.isTrue (Object.ReferenceEquals(item (fst pending[1]), item (fst pending[2]))) "Older completion must not replace newer item references."

        testCase "superseded completion and duplicate callbacks cannot promote" <| fun _ ->
            let state = cacheReuseFixture false
            let writer, scheduled, pending, projected, _, _, _ = cacheReuseWriter ()
            let mutable current = true
            let outcomes = ResizeArray<BrowserRuntimeCachePhasedWriteOutcome>()
            writer cacheReuseIdentity state (fun () -> current) outcomes.Add
            cacheReuseDrain scheduled
            current <- false
            snd pending[0] BrowserRuntimeCacheWriteResult.Written
            current <- true
            snd pending[0] BrowserRuntimeCacheWriteResult.Written
            Expect.equal outcomes.Count 1 "A duplicate storage callback must be ignored."
            projected.Value <- 0
            writer cacheReuseIdentity (cacheReusePatch state 100L 48) (fun () -> true) ignore
            cacheReuseDrain scheduled
            Expect.equal projected.Value 1 "A successful but superseded write cannot seed reuse."

        testCase "new identity releases the memo even if its write is cancelled" <| fun _ ->
            let state = cacheReuseFixture false
            let writer, scheduled, pending, projected, _, _, _ = cacheReuseWriter ()
            writer cacheReuseIdentity state (fun () -> true) ignore
            cacheReuseDrain scheduled
            snd pending[0] BrowserRuntimeCacheWriteResult.Written
            writer cacheReuseIdentity { state with Identity = { state.Identity with DocumentId = DocumentId "different" } } (fun () -> false) ignore
            cacheReuseDrain scheduled
            projected.Value <- 0
            writer cacheReuseIdentity (cacheReusePatch state 100L 48) (fun () -> true) ignore
            cacheReuseDrain scheduled
            Expect.equal projected.Value 1 "Returning to an old identity must rebuild after the single memo was released."

        testCase "fast projection retains canonical rejection and never writes an invalid state" <| fun _ ->
            let state = cacheReuseFixture false
            let writer, scheduled, pending, projected, encoded, _, _ = cacheReuseWriter ()
            writer cacheReuseIdentity state (fun () -> true) ignore
            cacheReuseDrain scheduled
            snd pending[0] BrowserRuntimeCacheWriteResult.Written
            let invalid = { cacheReusePatch state 100L 48 with Poll = RuntimePollState.PausedForResync }
            let expected = RuntimeCacheProjection.tryCreateEntry (DateTimeOffset(2026, 11, 7, 0, 0, 0, TimeSpan.Zero)) cacheReuseIdentity invalid
            let outcomes = ResizeArray<BrowserRuntimeCachePhasedWriteOutcome>()
            projected.Value <- 0
            encoded.Clear()
            writer cacheReuseIdentity invalid (fun () -> true) outcomes.Add
            cacheReuseDrain scheduled
            match expected, Seq.toList outcomes with
            | Error expectedErrors, [ BrowserRuntimeCachePhasedWriteOutcome.Completed(BrowserRuntimeCacheAcceptedStateWriteResult.Rejected actual) ] ->
                Expect.equal actual expectedErrors "The fast path must preserve canonical state validation."
            | _ -> failtest "Both paths must reject the paused state."
            Expect.equal projected.Value 0 "The case must exercise fast-path validation."
            Expect.isEmpty encoded "Rejected state must not encode."
            Expect.equal pending.Count 1 "Rejected state must not reach storage."
    ]

let tests =
    testList
        "interactive client lifecycle"
        [ testCase "start is idempotent and opens exactly one transport" (fun _ ->
              let started, effects =
                  InteractiveClientLifecycle.transition
                      options
                      InteractiveClientLifecycleEvent.Start
                      InteractiveClientLifecycle.initial

              Expect.isTrue started.Started "the application should enter started state"
              Expect.sequenceEqual effects [| InteractiveClientLifecycleEffect.OpenTransport |] "start should open one transport"

              let unchanged, duplicateEffects =
                  InteractiveClientLifecycle.transition options InteractiveClientLifecycleEvent.Start started

              Expect.equal unchanged started "a duplicate start should preserve lifecycle state"
              Expect.isEmpty duplicateEffects "a duplicate start must not open another transport")

          testCase "reconnect open remounts and requests one authoritative snapshot" (fun _ ->
              let started, _ =
                  InteractiveClientLifecycle.transition
                      options
                      InteractiveClientLifecycleEvent.Start
                      InteractiveClientLifecycle.initial

              let firstOpen, firstEffects =
                  InteractiveClientLifecycle.transition
                      options
                      (InteractiveClientLifecycleEvent.TransportOpened false)
                      started

              Expect.isTrue firstOpen.Connected "the first transport should connect"
              Expect.isFalse
                  (firstEffects |> Array.contains InteractiveClientLifecycleEffect.RequestFullSnapshot)
                  "the first transport has no runtime identity to resync"

              let closed, _ =
                  InteractiveClientLifecycle.transition options InteractiveClientLifecycleEvent.TransportClosed firstOpen

              let reconnecting, _ =
                  InteractiveClientLifecycle.transition options InteractiveClientLifecycleEvent.ReconnectDue closed

              let reopened, effects =
                  InteractiveClientLifecycle.transition
                      options
                      (InteractiveClientLifecycleEvent.TransportOpened true)
                      reconnecting

              Expect.isTrue reopened.Connected "the replacement transport should connect"
              Expect.equal reopened.ReconnectAttempt 1 "opening a socket is not yet a successful workspace recovery"
              Expect.equal
                  (effects |> Array.filter ((=) InteractiveClientLifecycleEffect.SendMounted) |> Array.length)
                  1
                  "reconnect should remount exactly once"
              Expect.equal
                  (effects |> Array.filter ((=) InteractiveClientLifecycleEffect.RequestFullSnapshot) |> Array.length)
                  1
                  "reconnect should request exactly one full snapshot"

              let synchronized, synchronizedEffects =
                  InteractiveClientLifecycle.transition
                      options
                      InteractiveClientLifecycleEvent.SnapshotAccepted
                      reopened

              Expect.equal synchronized.ReconnectAttempt 0 "an accepted authoritative snapshot should reset backoff"
              Expect.isEmpty synchronizedEffects "snapshot acceptance should not add transport effects")

          testCase "duplicate close cannot duplicate reconnect timer and failures back off" (fun _ ->
              let started, _ =
                  InteractiveClientLifecycle.transition
                      options
                      InteractiveClientLifecycleEvent.Start
                      InteractiveClientLifecycle.initial

              let firstClosed, firstEffects =
                  InteractiveClientLifecycle.transition options InteractiveClientLifecycleEvent.TransportClosed started

              Expect.sequenceEqual
                  firstEffects
                  [| InteractiveClientLifecycleEffect.ScheduleReconnect 1000 |]
                  "first close should schedule the base reconnect delay"

              let duplicateClosed, duplicateEffects =
                  InteractiveClientLifecycle.transition options InteractiveClientLifecycleEvent.TransportClosed firstClosed

              Expect.equal duplicateClosed firstClosed "duplicate close should preserve the scheduled reconnect"
              Expect.isEmpty duplicateEffects "duplicate close must not add another timer"

              let attempting, dueEffects =
                  InteractiveClientLifecycle.transition options InteractiveClientLifecycleEvent.ReconnectDue firstClosed

              Expect.sequenceEqual dueEffects [| InteractiveClientLifecycleEffect.OpenTransport |] "timer should open one replacement transport"

              let failedAgain, secondEffects =
                  InteractiveClientLifecycle.transition options InteractiveClientLifecycleEvent.TransportClosed attempting

              Expect.sequenceEqual
                  secondEffects
                  [| InteractiveClientLifecycleEffect.ScheduleReconnect 2000 |]
                  "a transport that fails before opening should use the next backoff delay"
              Expect.equal failedAgain.ReconnectAttempt 2 "consecutive failures should retain their attempt count")

          testCase "dispose is terminal and cancels every reconnect path" (fun _ ->
              let started, _ =
                  InteractiveClientLifecycle.transition
                      options
                      InteractiveClientLifecycleEvent.Start
                      InteractiveClientLifecycle.initial

              let closed, _ =
                  InteractiveClientLifecycle.transition options InteractiveClientLifecycleEvent.TransportClosed started

              let disposed, effects =
                  InteractiveClientLifecycle.transition options InteractiveClientLifecycleEvent.Dispose closed

              Expect.isTrue disposed.Disposed "dispose should be terminal"
              Expect.isFalse disposed.ReconnectScheduled "dispose should clear scheduled reconnect state"
              Expect.contains effects InteractiveClientLifecycleEffect.CancelReconnect "dispose should cancel the reconnect timer"
              Expect.contains effects InteractiveClientLifecycleEffect.SendUnmounted "dispose should release the mounted session"
              Expect.contains effects InteractiveClientLifecycleEffect.CloseTransport "dispose should close the current transport"

              for event in
                  [ InteractiveClientLifecycleEvent.Start
                    InteractiveClientLifecycleEvent.ReconnectDue
                    InteractiveClientLifecycleEvent.TransportOpened true
                    InteractiveClientLifecycleEvent.TransportClosed ] do
                  let unchanged, afterEffects = InteractiveClientLifecycle.transition options event disposed
                  Expect.equal unchanged disposed "disposed lifecycle must ignore later callbacks"
                  Expect.isEmpty afterEffects "disposed lifecycle must emit no later effects")

          testCase "frame pump separates decode and reduce and publishes only the complete candidate" (fun _ ->
              let scheduled = Queue<unit -> unit>()
              let mutable outcome = None
              let initial = RuntimeReducer.initial framePumpIdentity
              let frames = [| documentFrame; heartbeat 2L |] |> Array.map BrowserRuntimeCodec.encode

              BrowserRuntimeFramePump.reduceEncodedFramesWith
                  (fun _ work -> scheduled.Enqueue work)
                  (Some initial)
                  frames
                  (fun () -> true)
                  (fun value -> outcome <- Some value)

              Expect.equal scheduled.Count 1 "only the first decode slice should be queued initially"

              for completedSlices in 1 .. 3 do
                  scheduled.Dequeue() ()
                  Expect.isNone outcome $"candidate must remain unpublished after slice {completedSlices}"
                  Expect.equal scheduled.Count 1 "every decode/reduce stage should queue exactly one successor"

              scheduled.Dequeue() ()

              match outcome with
              | Some(BrowserRuntimeFramePumpOutcome.Applied state) ->
                  Expect.equal state.LastTransportSequence 2L "the complete candidate should contain both frames"
              | other -> failtest $"Expected an applied candidate, got {other}.")

          testCase "frame pump returns structured decode failure without a candidate" (fun _ ->
              let scheduled = Queue<unit -> unit>()
              let mutable outcome = None

              BrowserRuntimeFramePump.reduceEncodedFramesWith
                  (fun _ work -> scheduled.Enqueue work)
                  (Some(RuntimeReducer.initial framePumpIdentity))
                  [| "not-json" |]
                  (fun () -> true)
                  (fun value -> outcome <- Some value)

              scheduled.Dequeue() ()

              match outcome with
              | Some(BrowserRuntimeFramePumpOutcome.Rejected failure) ->
                  Expect.equal failure.Code "runtime-frame-decode-failed" "decode failures need a stable reason code"
                  Expect.equal failure.FrameIndex 0 "decode failure should identify the source frame"
              | other -> failtest $"Expected a rejected outcome, got {other}.")

          testCase "budgeted frame pump keeps atomic publication and cancels between turns" (fun _ ->
              for cancelAfterFirstTurn in [ false; true ] do
                  let turns = Queue<unit -> unit>()
                  let mutable current = true
                  let mutable publicationCount = 0
                  let mutable outcome = None
                  let schedule = PulseTrade.Comm.Spa.Dynamic.Renderer.RendererModel.createBudgetedWorkScheduler 4.0 2 turns.Enqueue (fun () -> 0.0) ignore
                  let frames = [| documentFrame; heartbeat 2L |] |> Array.map BrowserRuntimeCodec.encode
                  BrowserRuntimeFramePump.reduceEncodedFramesWith
                      (fun _ work -> schedule work)
                      (Some(RuntimeReducer.initial framePumpIdentity))
                      frames
                      (fun () -> current)
                      (fun value -> publicationCount <- publicationCount + 1; outcome <- Some value)
                  turns.Dequeue() ()
                  Expect.isNone outcome "A two-step turn must not publish the intermediate document candidate."
                  if cancelAfterFirstTurn then current <- false
                  while turns.Count > 0 do turns.Dequeue() ()
                  Expect.equal publicationCount 1 "Completion or supersession is reported exactly once."
                  match cancelAfterFirstTurn, outcome with
                  | true, Some BrowserRuntimeFramePumpOutcome.Superseded -> ()
                  | false, Some(BrowserRuntimeFramePumpOutcome.Applied state) ->
                      Expect.equal state.LastTransportSequence 2L "Only the complete ordered candidate is published."
                  | other -> failtest $"Unexpected budgeted-pump outcome: {other}.")

          testCase "browser time budget preserves FIFO atomic completion and cancellation at a turn boundary" (fun _ ->
              for cancelAfterFirstTurn in [false;true] do
                  let turns = Queue<unit -> unit>()
                  let mutable clock = 0.0
                  let mutable worked = 0
                  let mutable current = true
                  let mutable publications = 0
                  let mutable outcome = None
                  let schedule =
                      PulseTrade.Comm.Spa.Dynamic.Renderer.RendererModel.createBudgetedWorkScheduler
                          BrowserRuntimeFramePump.BrowserWorkBudgetMilliseconds
                          PulseTrade.Comm.Spa.Dynamic.Renderer.RendererModel.ScheduledWorkMaximumCount
                          turns.Enqueue
                          (fun () -> clock)
                          ignore
                  let frames = [|yield documentFrame; for sequence in 2L..12L -> heartbeat sequence|] |> Array.map BrowserRuntimeCodec.encode
                  BrowserRuntimeFramePump.reduceEncodedFramesWith
                      (fun _ work -> schedule (fun () -> worked <- worked+1; work (); clock <- clock+2.0))
                      (Some(RuntimeReducer.initial framePumpIdentity))
                      frames
                      (fun () -> current)
                      (fun value -> publications <- publications+1; outcome <- Some value)
                  turns.Dequeue() ()
                  Expect.equal worked 4 "four two-millisecond stages share a bounded turn before yielding for input"
                  Expect.isNone outcome "a partial multi-frame candidate is never published"
                  if cancelAfterFirstTurn then current <- false
                  while turns.Count > 0 do turns.Dequeue() ()
                  Expect.equal publications 1 "cancellation or completion must be terminal exactly once"
                  match cancelAfterFirstTurn,outcome with
                  | true,Some BrowserRuntimeFramePumpOutcome.Superseded -> ()
                  | false,Some(BrowserRuntimeFramePumpOutcome.Applied state) ->
                      Expect.equal state.LastTransportSequence 12L "all frames must be reduced in order"
                      Expect.equal state.DocumentRevision documentFrame.DocumentRevision "the document survives the complete heartbeat sequence"
                  | other -> failtest $"Unexpected browser-budget outcome: {other}.")

          testCase "frame pump cancels stale generation before publishing" (fun _ ->
              let scheduled = Queue<unit -> unit>()
              let mutable current = true
              let mutable outcome = None

              BrowserRuntimeFramePump.reduceEncodedFramesWith
                  (fun _ work -> scheduled.Enqueue work)
                  (Some(RuntimeReducer.initial framePumpIdentity))
                  [| documentFrame |> BrowserRuntimeCodec.encode |]
                  (fun () -> current)
                  (fun value -> outcome <- Some value)

              current <- false
              scheduled.Dequeue() ()
              Expect.equal outcome (Some BrowserRuntimeFramePumpOutcome.Superseded) "stale generation must not publish a candidate")

          testCase "snapshot validation yields once per data ref and publishes one final candidate" (fun _ ->
              let scheduled = Queue<unit -> unit>()
              let mutable publishCount = 0
              let mutable outcome = None
              let initial = RuntimeReducer.initial framePumpIdentity
              let documentState, _ = RuntimeReducer.reduce initial documentFrame

              BrowserRuntimeFramePump.reduceFrameWith
                  (fun _ work -> scheduled.Enqueue work)
                  documentState
                  (snapshotFrame 2L)
                  (fun () -> true)
                  0
                  (fun value ->
                      publishCount <- publishCount + 1
                      outcome <- Some value)

              Expect.equal scheduled.Count 1 "the first data ref validation should be queued"

              for completedDataRefs in 1 .. 2 do
                  scheduled.Dequeue() ()
                  Expect.isNone outcome $"snapshot must remain unpublished after data ref slice {completedDataRefs}"
                  Expect.equal scheduled.Count 1 "each data ref should schedule exactly one successor"

              scheduled.Dequeue() ()

              match outcome with
              | Some(BrowserRuntimeFramePumpOutcome.Applied state) ->
                  Expect.equal state.Data.Count 2 "the final candidate should contain the complete snapshot"
                  Expect.equal publishCount 1 "the pump must publish only one complete candidate"
              | other -> failtest $"Expected one applied snapshot candidate, got {other}.")

          testCase "snapshot generation change during data ref validation publishes no partial candidate" (fun _ ->
              let scheduled = Queue<unit -> unit>()
              let mutable current = true
              let mutable outcome = None
              let initial = RuntimeReducer.initial framePumpIdentity
              let documentState, _ = RuntimeReducer.reduce initial documentFrame

              BrowserRuntimeFramePump.reduceFrameWith
                  (fun _ work -> scheduled.Enqueue work)
                  documentState
                  (snapshotFrame 2L)
                  (fun () -> current)
                  0
                  (fun value -> outcome <- Some value)

              scheduled.Dequeue() ()
              Expect.isNone outcome "the first validated data ref must not publish a partial snapshot"
              current <- false
              scheduled.Dequeue() ()
              Expect.equal outcome (Some BrowserRuntimeFramePumpOutcome.Superseded) "superseded snapshot work must publish no candidate")

          testCase "DYN-T-648 adjacent cache selection uses temporal proximity and preserves known-empty spans" (fun _ ->
              let projection startOrdinal observationCount segments =
                  { CoverageIdentity = "coverage:es-1k"
                    CoverageRevision = 7L
                    QueryGeneration = 3L
                    Completeness = TaCoverageCompleteness.Partial
                    TotalObservationCount = None
                    Segments = segments
                    OverviewAxisRef = "axis.1k"
                    OverviewAnchors = [||]
                    ActiveDetail =
                      { StartObservationOrdinal = startOrdinal
                        ObservationCount = observationCount
                        BaseAxisRef = "axis.1k" }
                    GlobalOverview = None }
              let entry startUtc endUtc capturedAt coverageProjection =
                  { CacheIdentity = { OwnerFingerprint = "owner"; SchemaRevision = 1L }
                    WorkspaceId = framePumpDocument.WorkspaceId
                    Document =
                      { framePumpDocument with
                          DefaultView = Map.empty |> TaLoadedCoverageCodec.apply coverageProjection }
                    Snapshot = { Data = Map.empty; Freshness = TaFreshness.Live }
                    DocumentRevision = 1L
                    DataRevision = 1L
                    Coverage =
                      { StartEventTimeUtc = DateTimeOffset.Parse startUtc
                        EndEventTimeExclusiveUtc = DateTimeOffset.Parse endUtc }
                    CapturedAtUtc = DateTimeOffset.Parse capturedAt }

              let far =
                  entry
                      "2026-08-01T00:00:00Z"
                      "2026-08-02T00:00:00Z"
                      "2026-09-10T00:00:00Z"
                      (projection 0L 250 [||])
              let nearest =
                  entry
                      "2026-09-01T00:00:00Z"
                      "2026-09-02T00:00:00Z"
                      "2026-09-01T00:00:00Z"
                      (projection 250L 250 [||])
              let query =
                  { AcceptedProjection =
                      projection
                          500L
                          250
                          [| { SegmentId = "loaded"
                               StartEventTimeUtc = "2026-08-01T00:00:00Z"
                               EndEventTimeExclusiveUtc = "2026-09-03T00:00:00Z"
                               StartObservationOrdinal = 0L
                               ObservationCount = 750L } |]
                    Direction = TaCoverageDirection.Earlier
                    MaximumObservations = 250 }

              match BrowserRuntimeCache.selectAdjacent query [| far; nearest |] with
              | BrowserRuntimeCacheAdjacentSelection.Hit hit ->
                  Expect.equal hit.SourceEntry nearest "Adjacent selection must choose temporal proximity instead of the newest captured record."
                  match TaLoadedCoverageCodec.tryDecode hit.RebasedEntry.Document.DefaultView with
                  | Ok(Some projection) ->
                      Expect.equal projection.ActiveDetail.StartObservationOrdinal 250L "Rebase must retain the selected page."
                      Expect.equal projection.Segments query.AcceptedProjection.Segments "Rebase must carry current coverage metadata."
                  | result -> failtestf "Expected a rebased adjacent projection, got %A" result
              | result -> failtestf "Expected adjacent cache hit, got %A" result

              let emptySegment =
                  { SegmentId = "holiday"
                    StartEventTimeUtc = "2026-09-04T00:00:00Z"
                    EndEventTimeExclusiveUtc = "2026-09-05T00:00:00Z"
                    StartObservationOrdinal = 750L
                    ObservationCount = 0L }
              let emptyEntry =
                  entry
                      "2026-09-01T00:00:00Z"
                      "2026-09-02T00:00:00Z"
                      "2026-09-02T00:00:00Z"
                      (projection 250L 251 [| emptySegment |])
              let laterQuery =
                  { query with
                      AcceptedProjection =
                        { query.AcceptedProjection with
                            Segments = [| emptySegment |] }
                      Direction = TaCoverageDirection.Later }
              match BrowserRuntimeCache.selectAdjacent laterQuery [| emptyEntry |] with
              | BrowserRuntimeCacheAdjacentSelection.KnownEmpty span ->
                  Expect.equal span.StartEventTimeUtc "2026-09-04T00:00:00Z" "Known-empty retains its exact UTC start."
                  Expect.equal span.EndEventTimeExclusiveUtc "2026-09-05T00:00:00Z" "Known-empty retains its exact UTC end."
              | result -> failtestf "Expected KnownEmpty, got %A" result

              let globalLaterQuery =
                  { laterQuery with
                      AcceptedProjection =
                        { laterQuery.AcceptedProjection with
                            TotalObservationCount = None
                            GlobalOverview =
                              Some
                                { TotalObservationCount = 2_996_994L
                                  AxisRef = "axis.global"
                                  Anchors = [||]
                                  ResidentStartObservationOrdinal = 0L
                                  ResidentObservationCount = 750 } } }
              Expect.equal
                  (BrowserRuntimeCache.selectAdjacent globalLaterQuery [| emptyEntry |])
                  BrowserRuntimeCacheAdjacentSelection.Miss
                  "A loaded-tail empty span must not suppress a remote fetch when the global authority still has later ordinals."

              let loadedDomain =
                  { SegmentId = "loaded-domain"
                    StartEventTimeUtc = "2026-08-01T00:00:00Z"
                    EndEventTimeExclusiveUtc = "2026-09-04T00:00:00Z"
                    StartObservationOrdinal = 0L
                    ObservationCount = 4724L }
              let remoteEmpty =
                  { emptySegment with
                      SegmentId = "remote-holiday"
                      StartObservationOrdinal = 4724L }
              let attachedLeftProjection = projection 0L 724 [| loadedDomain; remoteEmpty |]
              let attachedLeftEntry =
                  entry
                      "2026-08-01T00:00:00Z"
                      "2026-09-04T00:00:00Z"
                      "2026-09-04T00:00:00Z"
                      attachedLeftProjection
              let followRightQuery =
                  { query with
                      AcceptedProjection = attachedLeftProjection
                      Direction = TaCoverageDirection.Later
                      MaximumObservations = 724 }

              Expect.equal
                  (BrowserRuntimeCache.selectAdjacent followRightQuery [| attachedLeftEntry |])
                  BrowserRuntimeCacheAdjacentSelection.Miss
                  "A remote empty span must not short-circuit a requested adjacent window that still has ordinals in the loaded domain.")

          testCase "DYN-T-648B adjacent cache rebase preserves valid old pages without weakening revision gates" (fun _ ->
              let projection revision startOrdinal observationCount segments =
                  { CoverageIdentity = "coverage:es-1k"
                    CoverageRevision = revision
                    QueryGeneration = revision
                    Completeness = TaCoverageCompleteness.Partial
                    TotalObservationCount = None
                    Segments = segments
                    OverviewAxisRef = "axis.1k"
                    OverviewAnchors = [||]
                    ActiveDetail =
                      { StartObservationOrdinal = startOrdinal
                        ObservationCount = observationCount
                        BaseAxisRef = "axis.1k" }
                    GlobalOverview = None }
              let initialA =
                  { SegmentId = "page-a-initial"
                    StartEventTimeUtc = "2026-09-01T00:00:00Z"
                    EndEventTimeExclusiveUtc = "2026-09-02T00:00:00Z"
                    StartObservationOrdinal = 0L
                    ObservationCount = 250L }
              let prependedB =
                  { SegmentId = "page-b"
                    StartEventTimeUtc = "2026-08-01T00:00:00Z"
                    EndEventTimeExclusiveUtc = "2026-09-01T00:00:00Z"
                    StartObservationOrdinal = 0L
                    ObservationCount = 250L }
              let relocatedA =
                  { initialA with
                      SegmentId = "page-a-relocated"
                      StartObservationOrdinal = 250L }
              let cachedProjection = projection 1L 0L 250 [| initialA |]
              let acceptedProjection = projection 2L 0L 250 [| prependedB; relocatedA |]
              let cachedEntry =
                  { CacheIdentity = { OwnerFingerprint = "owner"; SchemaRevision = 1L }
                    WorkspaceId = framePumpDocument.WorkspaceId
                    Document =
                      { framePumpDocument with
                          DefaultView = Map.empty |> TaLoadedCoverageCodec.apply cachedProjection }
                    Snapshot = { Data = Map.empty; Freshness = TaFreshness.Live }
                    DocumentRevision = 1L
                    DataRevision = 1L
                    Coverage =
                      { StartEventTimeUtc = DateTimeOffset.Parse "2026-09-01T00:00:00Z"
                        EndEventTimeExclusiveUtc = DateTimeOffset.Parse "2026-09-02T00:00:00Z" }
                    CapturedAtUtc = DateTimeOffset.Parse "2026-09-02T00:00:00Z" }
              let query =
                  { AcceptedProjection = acceptedProjection
                    Direction = TaCoverageDirection.Earlier
                    MaximumObservations = 250 }

              let rebased =
                  BrowserRuntimeCache.tryRebaseAdjacentEntry query cachedEntry
                  |> Option.defaultWith (fun () -> failtest "A page still covered by the accepted projection must be reusable.")
              match TaLoadedCoverageCodec.tryDecode rebased.Document.DefaultView with
              | Ok(Some value) ->
                  Expect.equal value.CoverageRevision 2L "The cache hit must carry the latest accepted coverage revision."
                  Expect.equal value.QueryGeneration 2L "The cache hit must carry the latest accepted query generation."
                  Expect.equal value.ActiveDetail.StartObservationOrdinal 250L "Prepending B must relocate cached page A from ordinal 0 to 250."
                  Expect.equal value.ActiveDetail.ObservationCount 250 "Rebase must retain cached page A's observation count."
              | result -> failtestf "Expected rebased loaded coverage, got %A" result

              let coverageDataRef = "viewport.loadedCoverage.cache"
              let dataRefEntry =
                  { cachedEntry with
                      Document =
                        { cachedEntry.Document with
                            DefaultView = Map.empty |> TaLoadedCoverageCodec.applyDataRef coverageDataRef }
                      Snapshot =
                        { cachedEntry.Snapshot with
                            Data = Map [ coverageDataRef, TaLoadedCoverageCodec.encode cachedProjection ] } }
              let rebasedDataRef =
                  BrowserRuntimeCache.tryRebaseAdjacentEntry query dataRefEntry
                  |> Option.defaultWith (fun () -> failtest "A dataRef-backed cached page must remain reusable.")
              Expect.equal
                  (TaLoadedCoverageCodec.tryDataRef rebasedDataRef.Document.DefaultView)
                  (Ok(Some coverageDataRef))
                  "Adjacent rebase must preserve the stable document dataRef authority."
              match TaLoadedCoverageCodec.tryDecodeResolved rebasedDataRef.Document.DefaultView rebasedDataRef.Snapshot.Data with
              | Ok(Some value) ->
                  Expect.equal value.CoverageRevision 2L "The dataRef cache hit carries the latest accepted coverage revision."
                  Expect.equal value.ActiveDetail.StartObservationOrdinal 250L "The dataRef cache hit carries the relocated page ordinal."
              | result -> failtestf "Expected a dataRef-backed rebased loaded coverage projection, got %A" result

              let coalescedAccepted =
                  { acceptedProjection with
                      Segments =
                        [| { SegmentId = "b-plus-a"
                             StartEventTimeUtc = prependedB.StartEventTimeUtc
                             EndEventTimeExclusiveUtc = relocatedA.EndEventTimeExclusiveUtc
                             StartObservationOrdinal = 0L
                             ObservationCount = 500L } |] }
              Expect.isNone
                  (BrowserRuntimeCache.tryRebaseAdjacentEntry { query with AcceptedProjection = coalescedAccepted } cachedEntry)
                  "A coalesced segment cannot uniquely relocate cached page A and must remain a miss."

              let ambiguousAccepted =
                  { acceptedProjection with
                      Segments =
                        [| relocatedA
                           { relocatedA with SegmentId = "duplicate-a"; StartObservationOrdinal = 500L } |] }
              Expect.isNone
                  (BrowserRuntimeCache.tryRebaseAdjacentEntry { query with AcceptedProjection = ambiguousAccepted } cachedEntry)
                  "Multiple temporal matches must not guess a page ordinal."

              let foreign =
                  { acceptedProjection with
                      CoverageIdentity = "coverage:nq-1k" }
              Expect.isNone
                  (BrowserRuntimeCache.tryRebaseAdjacentEntry { query with AcceptedProjection = foreign } cachedEntry)
                  "A different coverage identity must never be rebased."

              let futureEntry =
                  { cachedEntry with
                      Document =
                        { cachedEntry.Document with
                            DefaultView =
                              Map.empty
                              |> TaLoadedCoverageCodec.apply (projection 3L 0L 250 [| initialA |]) } }
              Expect.isNone
                  (BrowserRuntimeCache.tryRebaseAdjacentEntry query futureEntry)
                  "A future cache revision must not be accepted by an older projection.")

          testCase "DYN-T-648C cached A rehydrate commits A data and rebased coverage atomically after B" (fun _ ->
              let cachedPageA =
                  { SegmentId = "page-a-cached"
                    StartEventTimeUtc = "2026-09-01T00:00:00Z"
                    EndEventTimeExclusiveUtc = "2026-09-02T00:00:00Z"
                    StartObservationOrdinal = 0L
                    ObservationCount = 250L }
              let prependedPageB =
                  { SegmentId = "page-b-prepended"
                    StartEventTimeUtc = "2026-08-01T00:00:00Z"
                    EndEventTimeExclusiveUtc = "2026-09-01T00:00:00Z"
                    StartObservationOrdinal = 0L
                    ObservationCount = 250L }
              let relocatedPageA =
                  { cachedPageA with
                      SegmentId = "page-a-relocated"
                      StartObservationOrdinal = 250L }
              let projection revision generation startOrdinal segments =
                  { CoverageIdentity = "coverage:es-1k"
                    CoverageRevision = revision
                    QueryGeneration = generation
                    Completeness = TaCoverageCompleteness.Partial
                    TotalObservationCount = None
                    Segments = segments
                    OverviewAxisRef = "axis.1k"
                    OverviewAnchors = [||]
                    ActiveDetail =
                      { StartObservationOrdinal = startOrdinal
                        ObservationCount = 250
                        BaseAxisRef = "axis.1k" }
                    GlobalOverview = None }

              let cacheIdentity =
                  { OwnerFingerprint = "owner"
                    SchemaRevision = RuntimeCache.CurrentSchemaRevision }
              let acceptedB = projection 9L 12L 0L [| prependedPageB; relocatedPageA |]
              let cachedA = projection 8L 11L 0L [| cachedPageA |]
              let currentDocument =
                  { framePumpDocument with
                      TemporalAxisRefs = [| "axis.1k" |]
                      DefaultView = Map.empty |> TaLoadedCoverageCodec.apply acceptedB }
              let currentFrame =
                  { documentFrame with
                      DocumentRevision = 27L
                      TransportSequence = 1L
                      Payload = RuntimePayload.Document currentDocument }
              let current, _ = RuntimeReducer.reduce (RuntimeReducer.initial framePumpIdentity) currentFrame
              let axisStart = DateTimeOffset.Parse "2026-09-01T00:00:00Z"
              let cachedAxis =
                  { AxisRef = "axis.1k"
                    Revision = 14L
                    Points =
                      Array.init 250 (fun index ->
                          let intervalStart = axisStart.AddMinutes(float index)
                          let intervalEnd = intervalStart.AddMinutes 1.0
                          { Position = int64 index
                            SourceIntervalId = $"cached-a-{index}"
                            ScaleKey = "1K"
                            IntervalStartUtc = intervalStart
                            IntervalEndUtc = intervalEnd
                            EventTimeUtc = Some intervalEnd
                            ObservedThroughUtc = intervalEnd
                            AvailableAtUtc = Some intervalEnd
                            Finality = PointFinality.Final
                            Projection = TemporalProjection.CandleSpan
                            Quality = Some "complete" }) }
              let sourceEntry =
                  { CacheIdentity = cacheIdentity
                    WorkspaceId = framePumpDocument.WorkspaceId
                    Document =
                      { framePumpDocument with
                          TemporalAxisRefs = [| "axis.1k" |]
                          DefaultView = Map.empty |> TaLoadedCoverageCodec.apply cachedA }
                    Snapshot =
                      { Data =
                          Map [ "ta.rows", SduiValue.Array [||]
                                "ta.status", SduiValue.Array [| SduiValue.Text "A" |]
                                "axis.1k", TemporalAxisCodec.encode cachedAxis ]
                        Freshness = TaFreshness.Live }
                    DocumentRevision = 8L
                    DataRevision = 14L
                    Coverage =
                      { StartEventTimeUtc = DateTimeOffset.Parse "2026-09-01T00:00:00Z"
                        EndEventTimeExclusiveUtc = DateTimeOffset.Parse "2026-09-02T00:00:00Z" }
                    CapturedAtUtc = DateTimeOffset.Parse "2026-09-02T00:00:00Z" }
              let query =
                  { AcceptedProjection = acceptedB
                    Direction = PulseTrade.Comm.Spa.Dynamic.Contracts.TaCoverageDirection.Later
                    MaximumObservations = 250 }
              let rebased =
                  BrowserRuntimeCache.tryRebaseAdjacentEntry query sourceEntry
                  |> Option.defaultWith (fun () -> failtest "Expected cached A to rebase against accepted B coverage.")
              let hydrated =
                  RuntimeCacheProjection.tryRehydrate DynamicRuntimeDefaults.limits cacheIdentity current rebased
                  |> Result.defaultWith (fun errors -> failtestf "Expected atomic cache rehydrate, got %A" errors)

              let hydratedAxis =
                  hydrated.Data.["axis.1k"]
                  |> TemporalAxisCodec.decode
                  |> Result.defaultWith (fun errors -> failtestf "Expected cached A temporal identity, got %A" errors)
              Expect.equal hydratedAxis.Points.Length 250 "The hydrated temporal data must be cached page A's complete active detail."
              Expect.equal hydratedAxis.Points[0].SourceIntervalId "cached-a-0" "The hydrated temporal identity must come from cached page A."
              match hydrated.Document |> Option.bind (fun document -> TaLoadedCoverageCodec.tryDecode document.DefaultView |> function Ok value -> value | Error _ -> None) with
              | Some coverage ->
                  Expect.equal coverage.ActiveDetail.StartObservationOrdinal 250L "The hydrated document must select relocated cached page A."
                  Expect.equal coverage.QueryGeneration 12L "The rebased document must retain the latest accepted query generation."
                  Expect.equal coverage.CoverageRevision 9L "The rebased document must retain the latest accepted coverage revision."
                  Expect.equal coverage.Segments acceptedB.Segments "The hydrated page must retain the latest accepted loaded segments."
              | None -> failtest "Expected hydrated loaded coverage metadata."
              let hydratedDocument = hydrated.Document |> Option.defaultWith (fun () -> failtest "Expected hydrated document.")
              match
                  PulseTrade.Comm.Spa.Dynamic.Renderer.RendererModel.tryCoverageNavigatorWindow
                      hydratedAxis.Points.Length
                      { StartIndex = 0; Count = hydratedAxis.Points.Length }
                      hydratedDocument.DefaultView
              with
              | Some(_, _, window) ->
                  let viewingStart = window.StartIndex + 1
                  let viewingEnd = window.StartIndex + window.Count
                  Expect.equal (viewingStart, viewingEnd) (251, 500) "One atomic publish must render Viewing 251-500 for relocated cached page A."
              | None -> failtest "Expected renderer coverage projection after cached A publish."
              Expect.equal hydrated.DocumentRevision current.DocumentRevision "Cache presentation must not replace authoritative document revision."
              Expect.equal hydrated.LastTransportSequence current.LastTransportSequence "Cache presentation must not consume transport sequence."
              Expect.equal hydrated.Poll RuntimePollState.PausedForResync "Cache presentation remains non-authoritative until server resync.")

          testCase "DYN-T-584 snapshot batch commits only after every ordered item" (fun _ ->
              let initial =
                  BrowserRuntimeSnapshotBatch.create 7 "batch-7" 2
                  |> Result.defaultWith (fun (code, message) -> failtestf "%s: %s" code message)

              Expect.isError
                  (BrowserRuntimeSnapshotBatch.validateCommit 7 "batch-7" 2 initial)
                  "Commit before all items must fail."

              BrowserRuntimeSnapshotBatch.validateItem 7 "batch-7" 0 "series.a" initial
              |> Result.defaultWith (fun (code, message) -> failtestf "%s: %s" code message)

              let afterFirst = BrowserRuntimeSnapshotBatch.acceptItem "series.a" initial

              Expect.isError
                  (BrowserRuntimeSnapshotBatch.validateItem 7 "batch-7" 0 "series.a" afterFirst)
                  "Duplicate or repeated item index must fail."

              BrowserRuntimeSnapshotBatch.validateItem 7 "batch-7" 1 "series.b" afterFirst
              |> Result.defaultWith (fun (code, message) -> failtestf "%s: %s" code message)

              let complete = BrowserRuntimeSnapshotBatch.acceptItem "series.b" afterFirst

              BrowserRuntimeSnapshotBatch.validateCommit 7 "batch-7" 2 complete
              |> Result.defaultWith (fun (code, message) -> failtestf "%s: %s" code message))

          testCase "DYN-T-585 snapshot batch rejects out-of-order duplicate and mismatched commit" (fun _ ->
              let batch =
                  BrowserRuntimeSnapshotBatch.create 3 "batch-3" 2
                  |> Result.defaultWith (fun (code, message) -> failtestf "%s: %s" code message)

              Expect.isError
                  (BrowserRuntimeSnapshotBatch.validateItem 3 "batch-3" 1 "series.a" batch)
                  "Out-of-order item must fail."

              let afterFirst = BrowserRuntimeSnapshotBatch.acceptItem "series.a" batch

              Expect.isError
                  (BrowserRuntimeSnapshotBatch.validateItem 3 "batch-3" 1 "series.a" afterFirst)
                  "Duplicate dataRef must fail."

              Expect.isError
                  (BrowserRuntimeSnapshotBatch.validateCommit 3 "other" 2 afterFirst)
                  "Mismatched batch id must fail."

              Expect.isError
                  (BrowserRuntimeSnapshotBatch.validateCommit 3 "batch-3" 1 afterFirst)
                  "Mismatched item count must fail.")

          testCase "DYN-T-586 stale snapshot generation cannot accept an item or commit" (fun _ ->
              let batch =
                  BrowserRuntimeSnapshotBatch.create 11 "batch-11" 0
                  |> Result.defaultWith (fun (code, message) -> failtestf "%s: %s" code message)

              Expect.isError
                  (BrowserRuntimeSnapshotBatch.validateItem 12 "batch-11" 0 "series.a" batch)
                  "A stale generation cannot accept an item."

              Expect.isError
                  (BrowserRuntimeSnapshotBatch.validateCommit 12 "batch-11" 0 batch)
                  "A stale generation cannot commit even an empty snapshot.")

          testCase "DYN-T-692 browser telemetry correlates receive queue publish commit and first paint" (fun _ ->
              let sourcePath =
                  IO.Path.GetFullPath(
                      IO.Path.Combine(
                          __SOURCE_DIRECTORY__,
                          "..",
                          "..",
                          "src",
                          "PulseTrade.Comm.Spa.Dynamic.Interactive.Client",
                          "Client.fs"))
              let source = IO.File.ReadAllText sourcePath
              [| "data-runtime-body-received-at-ms"
                 "data-runtime-queue-wait-ms"
                 "data-runtime-body-to-publish-ms"
                 "data-runtime-body-to-commit-ms"
                 "data-runtime-commit-to-paint-ms"
                 "data-runtime-body-to-paint-ms"
                 "publishedBodyTimings.Remove receipt.LastTransportSequence" |]
              |> Array.iter (fun marker ->
                  Expect.stringContains source marker ("missing correlated telemetry marker: " + marker))
              Expect.isGreaterThanOrEqual
                  (source.Split("JS.RequestAnimationFrame").Length - 1)
                  3
                  "Client must retain the pump frame and a two-frame first-paint boundary.")

          testCase "DYN-T-698 typed GLOBAL loader starts before transport and authoritative mount replaces it" (fun _ ->
              let sourcePath =
                  IO.Path.GetFullPath(
                      IO.Path.Combine(
                          __SOURCE_DIRECTORY__,
                          "..",
                          "..",
                          "src",
                          "PulseTrade.Comm.Spa.Dynamic.Interactive.Client",
                          "Client.fs"))
              let source = IO.File.ReadAllText sourcePath
              for required in
                  [ "(globalOverviewBootstrapLoader: (unit -> Async<TaGlobalCoverageOverview option>) option)"
                    "globalOverviewBootstrapLoader"
                    "|> Option.iter (fun loader ->"
                    "TaWorkspaceRenderer.renderGlobalOverviewBootstrapWithDisplayTimeZone"
                    "RendererModel.authoritativeGlobalOverviewReady"
                    "globalOverviewBootstrapLoader.IsSome"
                    "authoritativeRendererReady next"
                    "createApplicationWithDisplayTimeZoneCore options displayTimeZone (Some loader)"
                    "startWithOptionsAndDisplayTimeZoneAndGlobalOverviewBootstrap" ] do
                  Expect.stringContains source required ("missing GLOBAL bootstrap lifecycle contract: " + required)

              let clearIndex = source.IndexOf("clearProjectionWatermark ()", StringComparison.Ordinal)
              let bootstrapIndex = source.IndexOf("globalOverviewBootstrapLoader", clearIndex, StringComparison.Ordinal)
              let startIndex = source.IndexOf("handle.Start()", StringComparison.Ordinal)
              Expect.isGreaterThan bootstrapIndex -1 "The typed bootstrap loader must be installed."
              Expect.isGreaterThan startIndex bootstrapIndex "The actual-GLOBAL loader must start before the websocket transport."

              let bootstrapStateStart = bootstrapIndex
              let resetStart = source.IndexOf("let resetActiveBatch", bootstrapStateStart, StringComparison.Ordinal)
              let bootstrapStateBody = source.Substring(bootstrapStateStart, resetStart - bootstrapStateStart)
              Expect.isFalse (bootstrapStateBody.Contains("RuntimeReducer", StringComparison.Ordinal)) "Bootstrap cannot fabricate a runtime frame or state."
              Expect.isFalse (bootstrapStateBody.Contains("publishProjectionCommit", StringComparison.Ordinal)) "Bootstrap cannot publish an authoritative projection receipt."
              for forbidden in
                  [ "ptcs-dynamic-ta-global-overview-bootstrap-v1"
                    "bootstrapEvent.InitCustomEvent"
                    "ReadyAttribute"
                    "OfferGlobalOverviewBootstrap" ] do
                  Expect.isFalse (source.Contains(forbidden, StringComparison.Ordinal)) ("The typed loader seam must not retain DOM transport: " + forbidden))

          testCase "DYN-T-702 authoritative GLOBAL handoff keeps a visible stable host" (fun _ ->
              let sourcePath =
                  IO.Path.GetFullPath(
                      IO.Path.Combine(
                          __SOURCE_DIRECTORY__,
                          "..",
                          "..",
                          "src",
                          "PulseTrade.Comm.Spa.Dynamic.Interactive.Client",
                          "Client.fs"))
              let source = IO.File.ReadAllText sourcePath

              for required in
                  [ "globalOverviewBootstrapHost"
                    "globalOverviewAuthoritativeHost"
                    "Doc.Run bootstrapHost"
                    "Doc.Run authoritativeHost"
                    "retireGlobalOverviewBootstrapWhenVisible"
                    "authoritativeHost.QuerySelector(\"[data-testid='ta-global-overview-with-axis']\")"
                    "GetBoundingClientRect()"
                    "Doc.Run bootstrapHost Doc.Empty" ] do
                  Expect.isTrue (source.Contains(required, StringComparison.Ordinal)) ("missing stable GLOBAL handoff contract: " + required)

              let handoffIndex = source.IndexOf("let rec retireGlobalOverviewBootstrapWhenVisible", StringComparison.Ordinal)
              let visibleIndex = source.IndexOf("GetBoundingClientRect()", handoffIndex, StringComparison.Ordinal)
              let retireIndex = source.IndexOf("Doc.Run bootstrapHost Doc.Empty", visibleIndex, StringComparison.Ordinal)
              let mountIndex = source.IndexOf("Doc.Run authoritativeHost renderer", StringComparison.Ordinal)
              let retireCallIndex = source.IndexOf("retireGlobalOverviewBootstrapWhenVisible ()", mountIndex, StringComparison.Ordinal)
              Expect.isGreaterThan mountIndex -1 "Authoritative rendering must target its persistent host."
              Expect.isGreaterThan retireCallIndex mountIndex "The handoff watcher must start only after authoritative rendering is mounted."
              Expect.isGreaterThan visibleIndex handoffIndex "The handoff must observe authoritative visibility."
              Expect.isGreaterThan retireIndex visibleIndex "Bootstrap must retire only after authoritative GLOBAL is visible.")

          testCase "DYN-T-709 early formal GLOBAL mounts during RUN before derived rows complete" (fun _ ->
              let sourcePath =
                  IO.Path.GetFullPath(
                      IO.Path.Combine(
                          __SOURCE_DIRECTORY__,
                          "..",
                          "..",
                          "src",
                          "PulseTrade.Comm.Spa.Dynamic.Interactive.Client",
                          "Client.fs"))
              let source = IO.File.ReadAllText sourcePath
              let publishStart = source.IndexOf("and publishRuntimeState (next: RuntimeState) =", StringComparison.Ordinal)
              let publishEnd = source.IndexOf("and publishSnapshotCandidate", publishStart, StringComparison.Ordinal)
              Expect.isGreaterThan publishStart -1 "The runtime publication boundary must remain explicit."
              Expect.isGreaterThan publishEnd publishStart "The runtime publication body must remain inspectable."

              let publishBody = source.Substring(publishStart, publishEnd - publishStart)
              Expect.stringContains
                  publishBody
                  "authoritativeRendererReady next"
                  "Any early formal frame carrying GLOBAL authority must be eligible to mount."
              Expect.stringContains
                  publishBody
                  "mount state"
                  "The eligible early formal frame must replace the bootstrap in place."
              for forbidden in
                  [ "WorkspaceMaterializationPhase.Completed"
                    "MaterializationPhase.Completed"
                    "Rows.Length > 0"
                    "Rows.IsEmpty" ] do
                  Expect.isFalse
                      (publishBody.Contains(forbidden, StringComparison.Ordinal))
                      ("Formal GLOBAL mounting must not wait for derived TA completion: " + forbidden)) ]

[<EntryPoint>]
let main argv =
    runTestsWithCLIArgs [] argv (testList "interactive client" [ tests; cacheReuseTests ])
