open System
open Expecto
open PulseTrade.Comm.Spa.Dynamic.Contracts
open PulseTrade.Comm.Spa.Dynamic.Ptcs.Client

let point =
    { time = "2026-07-11T09:00:00+08:00"
      openValue = 100.0
      highValue = 105.0
      lowValue = 98.0
      closeValue = 103.0
      volumeValue = 20.0
      lineValue = 0.0
      hasOpen = true
      hasHigh = true
      hasLow = true
      hasClose = true
      hasVolume = true
      hasLineValue = false }

let wire =
    { wireVersion = "ta-browser.v1"
      documentId = "document"
      canvasInstanceId = "canvas"
      workspaceId = "workspace"
      title = "TA Research"
      rowsRef = "rows"
      statusRef = "status"
      sharedTimeAxis = true
      rows =
        [| { rowId = "price"
             kind = "candlestick"
             dataRef = "series.price"
             heightWeight = 2.0
             visible = true } |]
      allowedActions = [| "change-query" |]
      querySourceId = "binance"
      queryInstrument = "BTCUSDT"
      queryIntervalMinutes = 1
      queryFromUtc = ""
      queryToUtcExclusive = ""
      queryIncludePartial = true
      series = [| { dataRef = "series.price"; points = [| point |] } |]
      statusLabel = "LIVE"
      freshness = "live"
      watermarkUtc = "2026-07-11T09:00:00Z"
      quality = "complete"
      lagSeconds = 0.0
      reasonCode = "within-live-threshold"
      documentRevision = 2L
      dataRevision = 9L
      transportSequence = 12L
      pollKind = "ready"
      errorCode = ""
      errorMessage = ""
      errorRecoverable = false }

let tests =
    testList
        "PTCS TA client"
        [ testCase "bounded browser state projects to renderer RuntimeState" (fun _ ->
              let state: RuntimeState =
                  match TaResearchClientWire.stateFromWire wire with
                  | Result.Ok value -> value
                  | Result.Error error -> failtest error

              Expect.equal state.DataRevision 9L "data revision should be retained."
              Expect.equal state.Poll RuntimePollState.Ready "poll state should be retained."
              Expect.equal state.Document.Value.Rows[0].Kind TaRowKind.Candlestick "row kind should be projected."
              Expect.equal state.Document.Value.DefaultView["query.instrument"] (SduiValue.Text "BTCUSDT") "server query identity should reach the renderer document."
              Expect.equal state.Document.Value.DefaultView["query.intervalMinutes"] (SduiValue.Number 1.0) "server interval should reach the renderer document."

              match state.Data["status"] with
              | SduiValue.Object status ->
                  Expect.equal status["watermarkUtc"] (SduiValue.Text "2026-07-11T09:00:00Z") "watermark should reach the renderer state."
                  Expect.equal status["quality"] (SduiValue.Text "complete") "quality should reach the renderer state."
              | other -> failtestf "Unexpected projected status: %A" other

              match state.Data["series.price"] with
              | SduiValue.Array [| SduiValue.Object values |] ->
                  Expect.equal values["c"] (SduiValue.Number 103.0) "candle close should use renderer field vocabulary."
              | other -> failtestf "Unexpected projected series: %A" other)

          testCase "add-row action emits canonical lowercase row kind" (fun _ ->
              let action =
                  SduiAction.AddTaRow(
                      CanvasInstanceId "canvas",
                      { RowId = "row-sma"
                        Kind = TaRowKind.Sma
                        DataRef = "series.sma"
                        HeightWeight = 1.0
                        Visible = true
                        Options = Map.empty })

              let encoded = TaResearchClientWire.actionToWire action
              Expect.equal encoded.rowKind "sma" "Browser wire must not leak F# union-case casing.")

          testCase "typed query action maps to bounded browser command" (fun _ ->
              let action =
                  SduiAction.ChangeTaQuery(
                      CanvasInstanceId "canvas",
                      { SourceId = Some "sql"
                        Instrument = Some "TXF"
                        IntervalMinutes = Some 5
                        FromUtc = Some "2026-07-01"
                        ToUtcExclusive = Some "2026-07-12"
                        IncludePartial = Some true })

              let actual = TaResearchClientWire.actionToWire action
              Expect.equal actual.actionKind "change-query" "query must remain typed on the browser wire."
              Expect.equal actual.instrument "TXF" "instrument should be retained."
              Expect.equal actual.intervalMinutes 5 "interval should be retained.")

          testCase "poll revision maps to a JSON-safe browser number" (fun _ ->
              let actual =
                  SduiAction.PollDelta(CanvasInstanceId "canvas", 9L)
                  |> TaResearchClientWire.actionToWire
              Expect.equal actual.afterDataRevision 9.0 "browser command revisions must not become JavaScript BigInt values.")

          testCase "lifecycle enforces one in-flight poll and retry without losing revision" (fun _ ->
              let canvas = CanvasInstanceId "canvas"
              let options =
                  { TaClientLifecycle.defaults with
                      PollIntervalMs = 5000
                      RequestTimeoutMs = 10000
                      PollRetryMs = 2000 }
              let initial = TaClientLifecycle.initial canvas
              let connected, connectEffects = TaClientLifecycle.transition options TaClientLifecycleEvent.Connected initial
              Expect.equal connected.Poll RuntimePollState.MountedIdle "connected channel should begin with mounted handshake."
              Expect.isTrue (connectEffects |> Array.contains TaClientLifecycleEffect.SendMounted) "connect should send one mounted frame."

              let ready, readyEffects = TaClientLifecycle.transition options (TaClientLifecycleEvent.StateAccepted 9L) connected
              Expect.equal ready.Poll RuntimePollState.Ready "accepted state should make polling ready."
              Expect.isTrue (readyEffects |> Array.contains (TaClientLifecycleEffect.SchedulePoll 5000)) "accepted state should schedule one poll."

              let polling, pollEffects = TaClientLifecycle.transition options (TaClientLifecycleEvent.PollDue DateTimeOffset.UtcNow) ready
              Expect.equal polling.Poll RuntimePollState.PollInFlight "poll due should enter one in-flight state."
              Expect.isTrue
                  (pollEffects
                   |> Array.exists (function TaClientLifecycleEffect.SendAction(SduiAction.PollDelta(_, 9L)) -> true | _ -> false))
                  "poll must use the last accepted revision."

              let stillPolling, duplicateEffects =
                  TaClientLifecycle.transition options (TaClientLifecycleEvent.PollDue DateTimeOffset.UtcNow) polling
              Expect.equal stillPolling polling "second poll while in flight must be a no-op."
              Expect.isEmpty duplicateEffects "second poll must emit no command."

              let timedOut, retryEffects =
                  TaClientLifecycle.transition options (TaClientLifecycleEvent.RequestTimedOut DateTimeOffset.UtcNow) polling
              Expect.equal timedOut.DataRevision 9L "timeout must retain last-good revision."
              Expect.isTrue (retryEffects |> Array.contains (TaClientLifecycleEffect.SchedulePoll 2000)) "timeout should schedule bounded retry.")

          testCase "lifecycle reconnect backoff, active suspension, resync and dispose fail closed" (fun _ ->
              let canvas = CanvasInstanceId "canvas"
              let options = { TaClientLifecycle.defaults with ReconnectBaseMs = 1000; ReconnectMaximumMs = 4000 }
              let initial = TaClientLifecycle.initial canvas
              let connected, _ = TaClientLifecycle.transition options TaClientLifecycleEvent.Connected initial
              let ready, _ = TaClientLifecycle.transition options (TaClientLifecycleEvent.StateAccepted 3L) connected
              let disconnected, effects = TaClientLifecycle.transition options TaClientLifecycleEvent.Disconnected ready
              Expect.equal disconnected.Poll RuntimePollState.Suspended "disconnect should preserve data but suspend polling."
              Expect.isTrue (effects |> Array.contains (TaClientLifecycleEffect.ScheduleReconnect 1000)) "first reconnect should use base delay."

              let reconnected, _ = TaClientLifecycle.transition options TaClientLifecycleEvent.Connected disconnected
              let readyAgain, _ = TaClientLifecycle.transition options (TaClientLifecycleEvent.StateAccepted 3L) reconnected
              let inactive, inactiveEffects = TaClientLifecycle.transition options (TaClientLifecycleEvent.ActiveChanged false) readyAgain
              Expect.equal inactive.Poll RuntimePollState.Suspended "inactive surface should not poll."
              Expect.isTrue (inactiveEffects |> Array.contains TaClientLifecycleEffect.CancelPoll) "inactive surface should cancel poll timer."

              let active, _ = TaClientLifecycle.transition options (TaClientLifecycleEvent.ActiveChanged true) inactive
              let resync, resyncEffects = TaClientLifecycle.transition options (TaClientLifecycleEvent.ResyncRequired "sequence-gap") active
              Expect.equal resync.Poll RuntimePollState.PausedForResync "resync should be explicit."
              Expect.isTrue
                  (resyncEffects
                   |> Array.exists (function TaClientLifecycleEffect.SendAction(SduiAction.RequestFullSnapshot(_, "sequence-gap")) -> true | _ -> false))
                  "resync should request a full snapshot."

              let disposed, disposeEffects = TaClientLifecycle.transition options TaClientLifecycleEvent.Dispose resync
              Expect.equal disposed.Poll RuntimePollState.Disposed "dispose should be terminal."
              Expect.isTrue (disposeEffects |> Array.contains TaClientLifecycleEffect.CancelReconnect) "dispose should cancel reconnect timer."
              let afterDispose, afterEffects = TaClientLifecycle.transition options TaClientLifecycleEvent.Connected disposed
              Expect.equal afterDispose disposed "disposed channel must not reconnect."
              Expect.isEmpty afterEffects "disposed channel must emit no effects.") ]

[<EntryPoint>]
let main argv = runTestsWithCLIArgs [] argv tests
