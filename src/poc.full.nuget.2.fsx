//
// Start a PTCS + PTCS.Dynamic host from NuGet packages.
//
// This POC intentionally does not expose the Actor Dynamic append-page shape in
// the +page selector. It still loads the Dynamic bundle, ActorsPage renderer,
// and Actor Argu FormInput target-key renderer.
//
// Default Visual Studio FSI use:
// 1. Ensure PulseTrade.Comm.Spa / PulseTrade.Comm.Spa.Dynamic nupkgs are built
//    and available in the #i package roots below.
// 2. Edit defaultArgumentsText only if you want a fixed port or PCSL root.
// 3. Select all and run. Call stopPocFullNuget2Host() to stop.
//

#i @"nuget: G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\bin"
#i @"nuget: C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic\src\bin\Release"
#i @"nuget: C:\Program Files\dotnet\sdk\10.0.301\FSharp\library-packs"
#r "nuget: FAkka.Argu, 10.1.301"
#r "nuget: FAkka.FCell2, 10.1.301"
#r "nuget: Akka, 1.5.69"
#r "nuget: Akka.Cluster, 1.5.69"
#r "nuget: Akka.Cluster.Sharding, 1.5.69"
#r "nuget: PersistedConcurrentSortedList, 10.1.301"
#r "nuget: PulseTrade.Comm.Spa, [0.2.5-beta43]"
#r "nuget: PulseTrade.Comm.Spa.Dynamic, [0.1.3-beta31]"

#load @"C:\Users\Administrator\.codex\lib\ParseLine.fsx"

open System
open System.IO
open System.Net
open System.Net.Http
open System.Net.Sockets
open System.Text
open System.Text.Json
open System.Threading
open Akka.Actor
open Argu
open PersistedConcurrentSortedList.Type
open PulseTrade.Comm.Spa
open PulseTrade.Comm.Spa.Dynamic.Server

let defaultArtifactRoot =
    @"G:\PulseTrade.fs.Comm.Log\manual\ptcsDynamicNuget2"

let defaultPcslRoot =
    Path.Combine(defaultArtifactRoot, "pcsl_001")

let pathArg (path: string) =
    (if isNull path then "" else path).Replace("\\", "/")

let freePort () =
    use listener = new TcpListener(IPAddress.Loopback, 0)
    listener.Start()
    let port = (listener.LocalEndpoint :?> IPEndPoint).Port
    listener.Stop()
    port

let defaultArgumentsText =
    let webPort = freePort ()
    let clusterPort = freePort ()
    printfn "allocated web port: %d, cluster port: %d" webPort clusterPort
    $"""--host 127.0.0.1 --port {webPort} --site-sharing isolated --pcsl-root "{pathArg defaultPcslRoot}" --delivery-profile nuget2-live --actor-name nuget2-echo --cluster-port {clusterPort}"""

type CliArgs =
    | Host of string
    | Port of int
    | Site_Sharing of string
    | Pcsl_Root of string
    | Delivery_Profile of string
    | Actor_Name of string
    | Cluster_Port of int
    | No_Wait
    | Block
    | Verbose_Startup
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Host _ -> "HTTP bind host."
            | Port _ -> "HTTP bind port. Use 0 for a random free port."
            | Site_Sharing _ -> "Site sharing mode: isolated or shared."
            | Pcsl_Root _ -> "PCSL root for this live host."
            | Delivery_Profile _ -> "Durable ingress profile id."
            | Actor_Name _ -> "Echo actor name under /user."
            | Cluster_Port _ -> "Local Akka cluster port."
            | No_Wait -> "Start, verify /healthz and /chat markers, then stop."
            | Block -> "Block on ActorSystem termination for browser testing."
            | Verbose_Startup -> "Do not suppress PTCS/Dynamic startup asset logs."

type PocMode =
    | Fast
    | Safe
    | Audit

type PocFullNuget2Argu =
    | Say of text: string
    | Set_Count of count: int
    | Mode of mode: PocMode
    | At of symbol: string * quantity: int
    | Tag of tag: string list
    | Verbose
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Say _ -> "Text input sample."
            | Set_Count _ -> "Number input sample."
            | Mode _ -> "Enum/select sample."
            | At _ -> "Tuple input sample."
            | Tag _ -> "Repeatable list input sample."
            | Verbose -> "Boolean flag sample."

type PocFullNuget2EchoActor() as this =
    inherit ReceiveActor()

    do
        this.Receive<ActorArguTargetCommand>(fun (command: ActorArguTargetCommand) ->
            let replyText =
                "poc.full.nuget.2 echo raw="
                + command.RawArgu
                + "; argv="
                + String.concat "|" command.ParsedArgv

            let reply: ActorArguTargetReply =
                { Value = fCell2.S replyText
                  Direction = Some "inbound-message"
                  Tags = Some [ "poc-full-nuget-2"; "echo" ] }

            this.ActorCtx.Sender.Tell(reply, this.ActorCtx.Self))
        |> ignore

    member _.ActorCtx: IActorContext = ActorBase.Context

let parser =
    ArgumentParser.Create<CliArgs>(programName = "poc.full.nuget.2.fsx")

let fsiArgs () =
    let values = fsi.CommandLineArgs

    if values.Length <= 1 then
        [||]
    else
        values
        |> Array.skip 1
        |> Array.filter ((<>) "--")

let defaultArgs () =
    PL.parseLine [| ' ' |] (Some '"') None true defaultArgumentsText

let defaultParsed =
    parser.ParseCommandLine(defaultArgs ())

let overrideParsed =
    let args = fsiArgs ()

    if args.Length = 0 then
        None
    else
        Some(parser.ParseCommandLine(args))

let textOr fallback (value: string) =
    if String.IsNullOrWhiteSpace value then fallback else value.Trim()

let fullPath fallback (value: string) =
    value
    |> textOr fallback
    |> Path.GetFullPath

let siteSharingOf value =
    match textOr "isolated" value |> _.ToLowerInvariant() with
    | "isolated" -> Isolated
    | "shared" -> Shared
    | other -> invalidArg "site-sharing" $"Unsupported site-sharing: {other}."

let webBinding host port =
    let host = textOr "127.0.0.1" host

    if port <= 0 then
        WebBinding.randomHost host
    else
        WebBinding.fixedHostPort host port

let durableIngressOptions profileId =
    let profileId = textOr "nuget2-live" profileId

    { CommSpaDurableIngressOptions.volatileLocal() with
        Mode = DurableIngressMode.DurableDelivery
        ProfileId = profileId
        ServerReality =
            { CommSpaServerReality.localVolatile profileId with
                DeliveryProfileId = profileId }
        Retry =
            { DurableDeliveryRetryOptions.defaults with
                DeadLetterStreamKey = "ptcs.dynamic.poc-full-nuget-2.dead-letter" } }

let host =
    let defaultValue = defaultParsed.GetResult(Host, "127.0.0.1")
    overrideParsed |> Option.map (fun parsed -> parsed.GetResult(Host, defaultValue)) |> Option.defaultValue defaultValue

let port =
    let defaultValue = defaultParsed.GetResult(Port, 0)
    overrideParsed |> Option.map (fun parsed -> parsed.GetResult(Port, defaultValue)) |> Option.defaultValue defaultValue

let pcslRoot =
    let defaultValue = defaultParsed.GetResult(Pcsl_Root, defaultPcslRoot)
    overrideParsed |> Option.map (fun parsed -> parsed.GetResult(Pcsl_Root, defaultValue)) |> Option.defaultValue defaultValue |> fullPath defaultPcslRoot

let siteSharing =
    let defaultValue = defaultParsed.GetResult(Site_Sharing, "isolated")
    overrideParsed |> Option.map (fun parsed -> parsed.GetResult(Site_Sharing, defaultValue)) |> Option.defaultValue defaultValue |> siteSharingOf

let deliveryProfile =
    let defaultValue = defaultParsed.GetResult(Delivery_Profile, "nuget2-live")
    overrideParsed |> Option.map (fun parsed -> parsed.GetResult(Delivery_Profile, defaultValue)) |> Option.defaultValue defaultValue

let actorName =
    let defaultValue = defaultParsed.GetResult(Actor_Name, "nuget2-echo")
    overrideParsed |> Option.map (fun parsed -> parsed.GetResult(Actor_Name, defaultValue)) |> Option.defaultValue defaultValue |> textOr "nuget2-echo"

let clusterPort =
    let defaultValue = defaultParsed.GetResult(Cluster_Port, 7788)
    overrideParsed |> Option.map (fun parsed -> parsed.GetResult(Cluster_Port, defaultValue)) |> Option.defaultValue defaultValue

let noWait =
    defaultParsed.Contains No_Wait
    || (overrideParsed |> Option.exists (fun parsed -> parsed.Contains No_Wait))

let block =
    defaultParsed.Contains Block
    || (overrideParsed |> Option.exists (fun parsed -> parsed.Contains Block))

let verboseStartup =
    defaultParsed.Contains Verbose_Startup
    || (overrideParsed |> Option.exists (fun parsed -> parsed.Contains Verbose_Startup))

let withStartupOutput f =
    if verboseStartup then
        f ()
    else
        let originalOut = Console.Out
        use sink = new StringWriter()

        try
            Console.SetOut(sink)
            f ()
        finally
            Console.SetOut(originalOut)

let defaultCanonicalArgString =
    "--say \"hello from poc2\" --set-count 3 --mode safe --at TTC 7 --tag aoe \"marvel now\" --verbose"

let templateRegistration =
    DynamicArguTemplateRegistration.fromTemplate<PocFullNuget2Argu>
        DynamicArguAliasBinding.empty
        (Some defaultCanonicalArgString)

Directory.CreateDirectory pcslRoot |> ignore

let hub =
    CommHub.createEmptyWithPcslRoot pcslRoot

let fabricOptions =
    { CommSpaActorFabricOptions.defaults with
        SystemName = "PtcsDynamicPocFullNuget2"
        ShardTypeName = "ptcs-dynamic-poc-full-nuget-2"
        ClusterPort = clusterPort }

let options =
    ServerOptions.localRandomWithHub hub
    |> ServerOptions.withWebBinding (webBinding host port)
    |> Server.withActorFabricOptions fabricOptions

let app =
    withStartupOutput (fun () -> Server.startWithSharing siteSharing options)

let fabric =
    app.ActorFabric
    |> Option.defaultWith (fun () -> failwith "Expected Server.startWithSharing to expose ActorFabric.")

withStartupOutput (fun () ->
    hub.useDynamicSdui(fabric.System, DynamicArguMetadata.empty, [ templateRegistration ])
    |> ignore

    hub.ListClientExtensions()
    |> List.tryFind (fun extension -> extension.ExtensionId = "pulse-trade-comm-spa-dynamic")
    |> Option.iter (fun extension ->
        hub.RegisterClientExtension({ extension with AppendPageShapes = [] })
        |> ignore))

let ingress =
    durableIngressOptions deliveryProfile |> CommSpaDurableIngress.createVolatile

let echoRef =
    fabric.System.ActorOf(Props.Create(fun () -> PocFullNuget2EchoActor()), actorName)

let actorAddress =
    fabric.NodeAddress.TrimEnd('/') + echoRef.Path.ToStringWithoutAddress()

let actorPath =
    echoRef.Path.ToStringWithoutAddress()

hub.RegisterActor
    { NodeId = "ptcs.dynamic.poc-full-nuget-2"
      NodeAddress = Some fabric.NodeAddress
      ActorId = actorPath
      DisplayName = Some actorName
      Kind = Some "actor"
      Status = Some "active"
      Roles = Some [ "ptcs"; "dynamic"; "poc" ]
      Routees = None
      Tags = Some [ "ptcs-dynamic"; "poc-full-nuget-2"; "echo"; "actor-argu" ] }
|> ignore

let templateKey =
    typeof<PocFullNuget2Argu>.FullName

let targetKeys =
    [ actorAddress; templateKey; defaultCanonicalArgString ]

let actorPage =
    let basePage = ActorArgu.fCellChatPage "actor-argu-poc-full-nuget-2" "ActorArgu POC Full NuGet 2" "actor-argu-poc-full-nuget-2"
    { basePage with
        Shape = "actor-argu"
        Description = "NuGet-loaded PTCS.Dynamic FormInput POC with Actor Dynamic page creation disabled."
        KeyPlaceholder = "[\"actor-address\", \"template-key\", \"--say \\\"hello\\\"\"]"
        DefaultKey = JsonSerializer.Serialize(targetKeys |> List.toArray)
        ValuePlaceholder = defaultCanonicalArgString
        Tags = [ "actor-argu"; "dynamic"; "nuget"; "poc2" ] }

let mutable pocFullNuget2Stopped = false

let require condition message =
    if not condition then
        failwith message

let requireActorsSnapshotNonEmpty (jsonText: string) =
    use doc = JsonDocument.Parse(jsonText)
    let root = doc.RootElement

    let intProperty (name: string) (alternateName: string) =
        let mutable value = Unchecked.defaultof<JsonElement>

        if root.TryGetProperty(name, &value) || root.TryGetProperty(alternateName, &value) then
            value.GetInt32()
        else
            failwith $"actors snapshot missing property {name}/{alternateName}: {jsonText}"

    let nodeCount = intProperty "nodeCount" "NodeCount"
    let actorCount = intProperty "actorCount" "ActorCount"

    require (nodeCount > 0) $"actors snapshot should have nodes, got {nodeCount}."
    require (actorCount > 0) $"actors snapshot should have actors, got {actorCount}."
    require (jsonText.Contains(actorPath)) $"actors snapshot should contain actor path {actorPath}."
    require (jsonText.Contains(fabric.NodeAddress)) $"actors snapshot should contain node address {fabric.NodeAddress}."

    nodeCount, actorCount

let stopPocFullNuget2Host () =
    if pocFullNuget2Stopped then
        printfn "poc.full.nuget.2 host already stopped."
    else
        pocFullNuget2Stopped <- true
        (app :> IDisposable).Dispose()
        fabric.System.WhenTerminated.Wait(TimeSpan.FromSeconds 10.0) |> ignore
        printfn "poc.full.nuget.2 host stopped."

try
    hub.RegisterAppendPage actorPage |> ignore
    hub.RegisterAppendPageKeyWithDisplayName(actorPage.PageId, targetKeys, "POC2 FormInput target") |> ignore

    let serverProbe =
        ActorArgu.sendDurableAsync
            ingress
            fabric
            { Send =
                { Page = actorPage
                  ActorAddress = actorAddress
                  HistoryKeys = Some targetKeys
                  RawArgu = "--say \"server probe\" --set-count 1 --mode fast"
                  Tags = Some [ "poc-full-nuget-2"; "server-probe" ] }
              IdempotencyKey = None
              Source = Some "poc.full.nuget.2.fsx"
              DeadlineAtUtc = None }
            CancellationToken.None
        |> Async.RunSynchronously

    use client = new HttpClient()
    let healthText = client.GetStringAsync(app.Url + "/healthz").GetAwaiter().GetResult()
    let chatHtml = client.GetStringAsync(app.Url + "/chat").GetAwaiter().GetResult()
    let actorsHtml = client.GetStringAsync(app.Url + "/actors").GetAwaiter().GetResult()
    let actorsSnapshotJson = client.GetStringAsync(app.Url + "/actors/api/snapshot").GetAwaiter().GetResult()
    let dynamicJs = client.GetStringAsync(app.Url + "/ext/js/PulseTrade.Comm.Spa.Dynamic.js").GetAwaiter().GetResult()
    let actorDynamicCreateShapeVisible =
        hub.ListClientExtensions()
        |> List.collect _.AppendPageShapes
        |> List.exists (fun shape -> String.Equals(shape.Shape, "actor-dynamic", StringComparison.OrdinalIgnoreCase))

    require (healthText.Contains("PulseTrade.Comm.Spa")) "healthz should identify PTCS."
    require (chatHtml.Length > 0) "chat page should be served by PTCS."
    require (not actorDynamicCreateShapeVisible) "poc2 must not expose +page Actor Dynamic shape in the extension manifest."
    require (not (chatHtml.Contains("option value=\"actor-dynamic\""))) "poc2 must not expose +page Actor Dynamic shape."
    require (actorsHtml.Length > 0) "actors page should be served."
    let actorsNodeCount, actorsActorCount = requireActorsSnapshotNonEmpty actorsSnapshotJson
    require (dynamicJs.Contains("dynamic-actors-page")) "Dynamic bundle should include ActorsPage renderer."
    require (dynamicJs.Contains("dynamic-argu-add-key")) "Dynamic bundle should include Add target key renderer."
    require (serverProbe.ActorArgu.IsSome) "server probe should append an ActorArgu reply."
    let deliveryStatusText = string serverProbe.DeliveryStatus.Status
    require
        (String.Equals(deliveryStatusText, "completed", StringComparison.OrdinalIgnoreCase))
        ("server probe delivery should complete: " + deliveryStatusText)

    let targetKeyAfterProbe: AppendPageKey =
        hub.ListAppendPageKeys(actorPage.PageId).Keys
        |> List.tryFind (fun key -> key.Keys = targetKeys)
        |> Option.defaultWith (fun () -> failwith "POC2 target key should remain registered after server probe.")

    require
        (String.Equals(targetKeyAfterProbe.DisplayName, "POC2 FormInput target", StringComparison.Ordinal))
        ("POC2 target key alias should survive server probe; got: " + targetKeyAfterProbe.DisplayName)

    printfn "PTCS Dynamic POC Full NuGet 2 started."
    printfn "Base URL      %s" app.Url
    printfn "Chat URL      %s/chat" app.Url
    printfn "Actors URL    %s/actors" app.Url
    printfn "Actors data   nodes=%d actors=%d" actorsNodeCount actorsActorCount
    printfn "ActorArgu URL %s/page/%s" app.Url actorPage.PageId
    printfn "Dynamic JS    %s/ext/js/PulseTrade.Comm.Spa.Dynamic.js" app.Url
    printfn "PCSL root     %s" pcslRoot
    printfn "Actor address %s" actorAddress
    printfn "Template key  %s" templateKey
    printfn "Target key    %s" (JsonSerializer.Serialize(targetKeys |> List.toArray))
    printfn "Default arg   %s" defaultCanonicalArgString
    printfn "Stop with     stopPocFullNuget2Host()"

    if noWait then
        stopPocFullNuget2Host ()
    elif block then
        printfn "Blocking on ActorSystem termination."
        fabric.System.WhenTerminated.Wait() |> ignore
with ex ->
    stopPocFullNuget2Host ()
    reraise ()
