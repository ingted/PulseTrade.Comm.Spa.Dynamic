namespace PulseTrade.Comm.Spa.Dynamic.Ptcs.LiveDemo

open System
open System.Collections.Generic
open System.IO
open System.Threading
open PulseTrade.Comm.Spa
open PulseTrade.Comm.Spa.Dynamic
open PulseTrade.Comm.Spa.Dynamic.Contracts

module Program =
    type LiveChannelState =
        { Sequence: int64
          Revision: int64
          PollCount: int }

    let port = 18883
    let extensionId = "ta-research"
    let assetUrl = "/client-extensions/ta-research/PulseTrade.Comm.Spa.Dynamic.Ptcs.LiveDemo.js"
    let runtimeAssetUrl = "/client-extensions/ta-research/WebSharper.Core.JavaScript/Runtime.js"
    let canvas = CanvasInstanceId "ta-live-canvas"
    let documentId = DocumentId "ta-live-document"

    let row rowId kind dataRef weight =
        { RowId = rowId
          Kind = kind
          DataRef = dataRef
          HeightWeight = weight
          Visible = true
          Options = Map.empty
          Traces = [||] }

    let document =
        { WorkspaceId = "ta-live-workspace"
          Title = "PTCS / PTMD TA Research"
          RowsRef = "rows"
          StatusRef = "ta.status"
          SharedTimeAxis = true
          Rows =
            [| row "price" TaRowKind.Candlestick "series.price" 2.4
               row "volume" TaRowKind.Volume "series.volume" 1.0
               row "sma" TaRowKind.Sma "series.sma" 1.0 |]
          AllowedActions = [| "change-query"; "poll-delta"; "request-full-snapshot" |]
          DefaultView = Map.empty }

    let point index =
        let baseline = 21800.0 + float index * 0.75 + Math.Sin(float index / 13.0) * 24.0
        let closeValue = baseline + Math.Cos(float index / 7.0) * 8.0
        let timestamp = DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero).AddMinutes(float (index * 5)).ToString("O")

        SduiValue.Object(
            Map [ "time", SduiValue.Text timestamp
                  "open", SduiValue.Number baseline
                  "high", SduiValue.Number(max baseline closeValue + 3.0)
                  "low", SduiValue.Number(min baseline closeValue - 3.0)
                  "close", SduiValue.Number closeValue
                  "volume", SduiValue.Number(900.0 + float ((index * 47) % 600)) ])

    let linePoint index =
        let timestamp = DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero).AddMinutes(float (index * 5)).ToString("O")
        SduiValue.Object(Map [ "time", SduiValue.Text timestamp; "value", SduiValue.Number(21820.0 + Math.Sin(float index / 16.0) * 20.0) ])

    let status poll revision =
        SduiValue.Object(
            Map [ "label", SduiValue.Text($"LIVE / poll {poll}")
                  "freshness", SduiValue.Text "live"
                  "watermarkUtc", SduiValue.Text(DateTimeOffset.UtcNow.ToString("O"))
                  "quality", SduiValue.Text "complete"
                  "lagSeconds", SduiValue.Number 0.0
                  "reasonCode", SduiValue.Text "within-live-threshold"
                  "revision", SduiValue.Number(float revision) ])

    let frame kind sequence baseRevision revision payload =
        { Protocol = DynamicRuntimeDefaults.protocol
          Kind = kind
          DocumentId = documentId
          CanvasInstanceId = canvas
          DocumentRevision = 1L
          BaseDataRevision = baseRevision
          DataRevision = revision
          TransportSequence = sequence
          Payload = payload }

    let createBackend () =
        let gate = obj()
        let states = Dictionary<string, LiveChannelState>(StringComparer.Ordinal)

        let stateKey (context: ClientExtensionTransientCommandContext) =
            context.Session.SessionId + "\u001f" + context.ChannelId

        let read context =
            lock gate (fun () ->
                match states.TryGetValue(stateKey context) with
                | true, value -> value
                | _ -> { Sequence = 0L; Revision = 0L; PollCount = 0 })

        let next context nextRevision nextPollCount =
            lock gate (fun () ->
                let current = read context
                let value =
                    { Sequence = current.Sequence + 1L
                      Revision = nextRevision
                      PollCount = nextPollCount }
                states[stateKey context] <- value
                value)

        { HandleAsync =
            fun context command ->
                async {
                    let current = read context
                    match command with
                    | RuntimeClientFrame.Mounted _
                    | RuntimeClientFrame.Unmounted _ ->
                        printfn "TA transient backend operation=%s channel=%s command=%A" context.Operation context.ChannelId command
                    | _ -> ()

                    match command with
                    | RuntimeClientFrame.Mounted _ ->
                        let initialized =
                            lock gate (fun () ->
                                let value = { Sequence = 1L; Revision = 0L; PollCount = 0 }
                                states[stateKey context] <- value
                                value)
                        return Ok(frame RuntimeFrameKind.Document initialized.Sequence None initialized.Revision (RuntimePayload.Document document))
                    | RuntimeClientFrame.Action(SduiAction.PollDelta _) when current.Revision = 0L ->
                        let advanced = next context 1L (current.PollCount + 1)
                        let points = Array.init 500 point
                        let lines = Array.init 500 linePoint
                        let snapshot =
                            { Data =
                                Map [ "series.price", SduiValue.Array points
                                      "series.volume", SduiValue.Array points
                                      "series.sma", SduiValue.Array lines
                                      "ta.status", status advanced.PollCount advanced.Revision ]
                              Freshness = TaFreshness.Live }
                        return Ok(frame RuntimeFrameKind.Snapshot advanced.Sequence None advanced.Revision (RuntimePayload.Snapshot snapshot))
                    | RuntimeClientFrame.Action(SduiAction.PollDelta _) ->
                        let advanced = next context (current.Revision + 1L) (current.PollCount + 1)
                        let index = 499 + advanced.PollCount
                        let patch =
                            { Operations =
                                [| PatchOperation.UpsertSeriesPoints("series.price", "time", [| match point index with SduiValue.Object value -> value | _ -> Map.empty |])
                                   PatchOperation.UpsertSeriesPoints("series.volume", "time", [| match point index with SduiValue.Object value -> value | _ -> Map.empty |])
                                   PatchOperation.UpsertSeriesPoints("series.sma", "time", [| match linePoint index with SduiValue.Object value -> value | _ -> Map.empty |])
                                   match status advanced.PollCount advanced.Revision with
                                   | SduiValue.Object value -> PatchOperation.SetStatus("ta.status", value)
                                   | _ -> PatchOperation.SetStatus("ta.status", Map.empty) |] }
                        return Ok(frame RuntimeFrameKind.Patch advanced.Sequence (Some current.Revision) advanced.Revision (RuntimePayload.Patch patch))
                    | RuntimeClientFrame.Action(SduiAction.RequestFullSnapshot _) ->
                        let points = Array.init 500 point
                        let lines = Array.init 500 linePoint
                        let advanced = next context (current.Revision + 1L) current.PollCount
                        let snapshot =
                            { Data =
                                Map [ "series.price", SduiValue.Array points
                                      "series.volume", SduiValue.Array points
                                      "series.sma", SduiValue.Array lines
                                      "ta.status", status advanced.PollCount advanced.Revision ]
                              Freshness = TaFreshness.Live }
                        return Ok(frame RuntimeFrameKind.Snapshot advanced.Sequence None advanced.Revision (RuntimePayload.Snapshot snapshot))
                    | RuntimeClientFrame.Unmounted _ ->
                        let advanced = next context current.Revision current.PollCount
                        return Ok(frame RuntimeFrameKind.Heartbeat advanced.Sequence None advanced.Revision (RuntimePayload.Heartbeat { ObservedAtUtc = DateTimeOffset.UtcNow }))
                    | _ -> return Error "unsupported-live-demo-command"
                } }

    let findBundle relativePath =
        let candidates =
            [ Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "js", relativePath)
              Path.Combine(AppContext.BaseDirectory, "wwwroot", "js", relativePath) ]

        candidates
        |> List.tryFind File.Exists
        |> Option.defaultWith (fun () -> failwith ("PTCS live demo bundle not found. Checked: " + String.concat "; " candidates))

    [<EntryPoint>]
    let main _ =
        let pcslRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "artifacts", "ptcs-ta-live", "pcsl"))
        let hub = CommHub.createEmptyWithPcslRoot pcslRoot
        TaResearchTransientServer.register extensionId (createBackend ()) hub |> ignore

        let bundle = findBundle "PulseTrade.Comm.Spa.Dynamic.Ptcs.LiveDemo.js" |> File.ReadAllText
        let runtime = findBundle (Path.Combine("WebSharper.Core.JavaScript", "Runtime.js")) |> File.ReadAllText
        hub.RegisterClientExtensionScriptAsset({ Url = assetUrl; ContentType = "application/javascript; charset=utf-8"; Content = bundle }) |> ignore
        hub.RegisterClientExtensionScriptAsset({ Url = runtimeAssetUrl; ContentType = "application/javascript; charset=utf-8"; Content = runtime }) |> ignore
        hub.RegisterClientExtension(
            { ExtensionId = extensionId
              DisplayName = Some "TA Research Live"
              MetadataJson = None
              ScriptUrls = [ assetUrl ]
              AppendPageShapes = [] })
        |> ignore

        let suffix = Guid.NewGuid().ToString("N").Substring(0, 8)
        let fabricOptions =
            { CommSpaActorFabricOptions.defaults with
                SystemName = "PtcsTaLive" + suffix
                ShardTypeName = "ptcs-ta-live-" + suffix
                AskTimeout = TimeSpan.FromSeconds 5.0 }
        let fabric = CommSpaActorFabric.startWithOptions fabricOptions hub.PersistenceBackend
        let running =
            Server.start
                { Host = "127.0.0.1"
                  Port = port
                  Hub = hub
                  OAuth = OAuth.disabled
                  Auth = None
                  Acl = None
                  ActorFabric = External fabric }

        use stopped = new ManualResetEventSlim(false)
        Console.CancelKeyPress.Add(fun event -> event.Cancel <- true; stopped.Set())
        AppDomain.CurrentDomain.ProcessExit.Add(fun _ -> stopped.Set())
        printfn "PTCS TA live demo url=http://127.0.0.1:%d/chat pcsl=%s" port pcslRoot

        try stopped.Wait()
        finally
            (running :> IDisposable).Dispose()
            fabric.Stop()

        0
