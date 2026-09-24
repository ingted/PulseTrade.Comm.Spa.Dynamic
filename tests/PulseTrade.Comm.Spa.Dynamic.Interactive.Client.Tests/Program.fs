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
