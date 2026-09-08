module PulseTrade.Comm.Spa.Dynamic.Interactive.Client.Tests

open Expecto
open PulseTrade.Comm.Spa.Dynamic.Interactive.Client

let options =
    { InteractiveClientLifecycle.defaults with
        ReconnectBaseMs = 1000
        ReconnectMaximumMs = 8000 }

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
                  Expect.isEmpty afterEffects "disposed lifecycle must emit no later effects") ]

[<EntryPoint>]
let main argv =
    runTestsWithCLIArgs [] argv tests
