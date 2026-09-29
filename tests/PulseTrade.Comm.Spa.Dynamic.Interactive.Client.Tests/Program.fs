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
                        BaseAxisRef = "axis.1k" } }
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
                        BaseAxisRef = "axis.1k" } }
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
                        BaseAxisRef = "axis.1k" } }

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
                  "A stale generation cannot commit even an empty snapshot.") ]

[<EntryPoint>]
let main argv =
    runTestsWithCLIArgs [] argv tests
