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
    { wireVersion = "ta-browser.v2"
      updateKind = "full"
      baseDataRevision = 0L
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
             visible = true
             traces =
                [| { traceId = "kbar"
                     kind = "candlestick"
                     dataRef = "series.price"
                     label = "K Bar"
                     color = "#0f766e"
                     width = 2.0
                     visible = true } |] } |]
      allowedActions = [| "change-query" |]
      querySourceId = "binance"
      queryInstrument = "BTCUSDT"
      queryIntervalMinutes = 1
      queryFromUtc = ""
      queryToUtcExclusive = ""
      queryIncludePartial = true
      series =
        [| { dataRef = "series.price"
             mode = "replace"
             removeBeforeTime = ""
             hasRemoveBeforeTime = false
             points = [| point |] } |]
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
              Expect.equal state.Document.Value.Rows[0].Traces.Length 1 "composite trace metadata should be projected."
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
                        Traces = [||]
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

          testCase "delta wire upserts points, trims rolling prefixes and rejects revision gaps" (fun _ ->
              let initial = TaResearchClientWire.stateFromWire wire |> Result.defaultWith failtest
              let nextPoint = { point with time = "2026-07-11T09:01:00+08:00"; closeValue = 104.0 }
              let delta =
                  { wire with
                      updateKind = "delta"
                      baseDataRevision = 9L
                      dataRevision = 10L
                      transportSequence = 13L
                      series =
                        [| { dataRef = "series.price"
                             mode = "upsert"
                             removeBeforeTime = ""
                             hasRemoveBeforeTime = false
                             points = [| nextPoint |] } |] }
              let merged = TaResearchClientWire.applyWire initial delta |> Result.defaultWith failtest
              match merged.Data["series.price"] with
              | SduiValue.Array points -> Expect.equal points.Length 2 "delta append retains the prior point."
              | value -> failtestf "Unexpected merged series: %A" value

              let trimmedWire =
                  { delta with
                      baseDataRevision = 10L
                      dataRevision = 11L
                      transportSequence = 14L
                      series =
                        [| { dataRef = "series.price"
                             mode = "upsert"
                             removeBeforeTime = nextPoint.time
                             hasRemoveBeforeTime = true
                             points = [||] } |] }
              let trimmed = TaResearchClientWire.applyWire merged trimmedWire |> Result.defaultWith failtest
              match trimmed.Data["series.price"] with
              | SduiValue.Array points -> Expect.equal points.Length 1 "rolling-window trim removes only the stale prefix."
              | value -> failtestf "Unexpected trimmed series: %A" value

              let mismatched = { delta with baseDataRevision = 8L }
              Expect.isError (TaResearchClientWire.applyWire initial mismatched) "revision gaps must request resync rather than corrupt client state.")

          testCase "lifecycle enforces one in-flight request and reconnects without overlap after timeout" (fun _ ->
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

              let inactivePolling, inactivePollingEffects =
                  TaClientLifecycle.transition options (TaClientLifecycleEvent.ActiveChanged false) polling
              Expect.isTrue inactivePolling.InFlight "hiding a mounted surface must not forget its in-flight request."
              Expect.isFalse
                  (inactivePollingEffects |> Array.contains TaClientLifecycleEffect.CancelTimeout)
                  "the in-flight request timeout must remain armed while the surface is hidden."
              let pendingDispose, pendingDisposeEffects =
                  TaClientLifecycle.transition options TaClientLifecycleEvent.Dispose inactivePolling
              Expect.isTrue pendingDispose.DisposePending "hidden in-flight surfaces must defer disposal."
              Expect.isFalse
                  (pendingDisposeEffects |> Array.contains TaClientLifecycleEffect.SendUnmounted)
                  "hidden in-flight surfaces must not overlap close with the active request."

              let timedOut, retryEffects =
                  TaClientLifecycle.transition options (TaClientLifecycleEvent.RequestTimedOut DateTimeOffset.UtcNow) polling
              Expect.equal timedOut.DataRevision 9L "timeout must retain last-good revision."
              Expect.isFalse timedOut.Connected "timeout must retire the still-busy transport before retrying."
              Expect.isTrue (retryEffects |> Array.contains TaClientLifecycleEffect.CloseTransport) "timeout must close the channel that may still own an in-flight server request."
              Expect.isTrue (retryEffects |> Array.contains (TaClientLifecycleEffect.ScheduleReconnect 1000)) "timeout should schedule bounded reconnect."
              Expect.isFalse
                  (retryEffects |> Array.exists (function TaClientLifecycleEffect.SchedulePoll _ -> true | _ -> false))
                  "timeout must not overlap the server request with another poll on the same channel."

              let afterClose, afterCloseEffects =
                  TaClientLifecycle.transition options TaClientLifecycleEvent.Disconnected timedOut
              Expect.equal afterClose timedOut "the close callback for an already retired transport must be idempotent."
              Expect.isEmpty afterCloseEffects "the close callback must not schedule a duplicate reconnect."

              let resyncing, resyncEffects =
                  TaClientLifecycle.transition options (TaClientLifecycleEvent.ResyncRequired "invalid-browser-state") polling
              Expect.equal resyncing.Poll RuntimePollState.PausedForResync "invalid in-flight reply should enter explicit resync."
              Expect.isTrue (resyncEffects |> Array.contains TaClientLifecycleEffect.CancelTimeout) "resync must cancel the failed request timeout."
              Expect.isTrue
                  (resyncEffects
                   |> Array.exists (function
                       | TaClientLifecycleEffect.SendAction(SduiAction.RequestFullSnapshot(_, "invalid-browser-state")) -> true
                       | _ -> false))
                  "resync must replace the failed in-flight request with a full snapshot command.")

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

              let disposePending, disposeEffects = TaClientLifecycle.transition options TaClientLifecycleEvent.Dispose resync
              Expect.equal disposePending.Poll RuntimePollState.Disposed "dispose should hide the mounted surface immediately."
              Expect.isTrue disposePending.DisposePending "an in-flight request must settle before close is sent."
              Expect.isFalse
                  (disposeEffects |> Array.contains TaClientLifecycleEffect.SendUnmounted)
                  "dispose must not overlap close with the in-flight resync request."
              Expect.isTrue (disposeEffects |> Array.contains TaClientLifecycleEffect.CancelReconnect) "dispose should cancel reconnect timer."

              let closing, closeEffects =
                  TaClientLifecycle.transition options (TaClientLifecycleEvent.StateAccepted 4L) disposePending
              Expect.isTrue closing.DisposePending "close remains pending until the transport confirms or disconnects."
              Expect.isTrue (closeEffects |> Array.contains TaClientLifecycleEffect.SendUnmounted) "the settled request should release exactly one channel."

              let disposed, disconnectedEffects =
                  TaClientLifecycle.transition options TaClientLifecycleEvent.Disconnected closing
              Expect.isTrue disposed.Disposed "disconnect after close should finish disposal."
              Expect.isTrue
                  (disconnectedEffects |> Array.contains TaClientLifecycleEffect.CloseTransport)
                  "finished disposal must release the transport without reconnecting."

              let afterDispose, afterEffects = TaClientLifecycle.transition options TaClientLifecycleEvent.Connected disposed
              Expect.equal afterDispose disposed "disposed channel must not reconnect."
              Expect.isEmpty afterEffects "disposed channel must emit no effects.") ]

[<EntryPoint>]
let main argv = runTestsWithCLIArgs [] argv tests
