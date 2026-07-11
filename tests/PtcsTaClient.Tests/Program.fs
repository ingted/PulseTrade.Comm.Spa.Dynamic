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
      series = [| { dataRef = "series.price"; points = [| point |] } |]
      statusLabel = "LIVE"
      freshness = "live"
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

              match state.Data["series.price"] with
              | SduiValue.Array [| SduiValue.Object values |] ->
                  Expect.equal values["c"] (SduiValue.Number 103.0) "candle close should use renderer field vocabulary."
              | other -> failtestf "Unexpected projected series: %A" other)

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
              Expect.equal actual.intervalMinutes 5 "interval should be retained.") ]

[<EntryPoint>]
let main argv = runTestsWithCLIArgs [] argv tests
