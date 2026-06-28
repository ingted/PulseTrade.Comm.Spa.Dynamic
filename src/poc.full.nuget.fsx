#i @"nuget: C:\Program Files\dotnet\sdk\10.0.300\FSharp\library-packs"
#r "nuget: FAkka.Argu, 10.1.301"
#r "nuget: FAkka.FCell2, 10.1.301"
#r "nuget: Akka, 1.5.69"
#r "nuget: Akka.Cluster, 1.5.69"
#r "nuget: Akka.Cluster.Sharding, 1.5.69"
#r "nuget: Akka.Persistence, 1.5.69"
#r "nuget: Suave, 3.4.3"
#r "nuget: PersistedConcurrentSortedList, 10.1.301"
#r @"nuget: PulseTrade.Comm.Spa, 0.2.5-beta40"
#r @"nuget: PulseTrade.Comm.Spa.Dynamic, 0.1.3-beta29"
//#I __SOURCE_DIRECTORY__
//#I "bin/Release/net10.0"
//#r "PulseTrade.Comm.Spa.Dynamic.dll"

#load @"C:\Users\Administrator\.codex\lib\ParseLine.fsx"

open System
open System.IO
open System.Net
open System.Net.Http
open System.Net.Sockets
open System.Threading
open Akka.Actor
open Argu
open PersistedConcurrentSortedList.Type
open PulseTrade.Comm.Spa
open PulseTrade.Comm.Spa.Dynamic.Server

// Demo 目標：
// - 展示 PulseTrade.Comm.Spa.Dynamic 的 ShowcaseDemoActor 與 fskynet-sdui JSON 生成。
// - 建立 Echo actor 測試原有的 durable 功能。

let defaultPcslRoot = __SOURCE_DIRECTORY__ + "/.pcsl/poc.dynamic.v17.fsx"

let defaultArgPath (path: string) =
    if String.IsNullOrWhiteSpace path then "" else path.Replace('\\', '/')

let defaultArgumentsText =
    $"""--host 127.0.0.1 --port 0 --site-sharing isolated --pcsl-root "{defaultArgPath defaultPcslRoot}" --delivery-profile poc-durable --actor-name durable-echo --cluster-port 0"""

type CliArguments =
    | Host of host: string
    | Port of port: int
    | Site_Sharing of mode: string
    | Pcsl_Root of path: string
    | Delivery_Profile of profile: string
    | Actor_Name of name: string
    | Cluster_Port of port: int
    | No_Wait
    | Verbose_Startup
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Host _ -> "Local HTTP bind host."
            | Port _ -> "Local HTTP port. Use 0 for a random free port."
            | Site_Sharing _ -> "Site sharing mode: shared or isolated."
            | Pcsl_Root _ -> "Root directory for PCSL files."
            | Delivery_Profile _ -> "Durable ingress profile id."
            | Actor_Name _ -> "Echo actor name under /user."
            | Cluster_Port _ -> "Akka.NET cluster port (default: 7705)."
            | No_Wait -> "Start and stop immediately for smoke tests."
            | Verbose_Startup -> "Print PTCS/WebSharper startup asset listing."

type DurableEchoActor() as this =
    inherit ReceiveActor()

    do
        this.Receive<ActorArguTargetCommand>(fun (command: ActorArguTargetCommand) ->
            let replyText =
                "durable echo raw="
                + command.RawArgu
                + "; argv="
                + String.concat "|" command.ParsedArgv

            let reply: ActorArguTargetReply =
                { Value = fCell2.S replyText
                  Direction = Some "inbound-message"
                  Tags = Some [ "poc"; "durable"; "echo" ] }

            this.ActorCtx.Sender.Tell(reply, this.ActorCtx.Self))
        |> ignore

    member _.ActorCtx: IActorContext = ActorBase.Context

type DynamicEchoActor() as this =
    inherit ReceiveActor()

    do
        this.Receive<ActorArguTargetCommand>(fun (command: ActorArguTargetCommand) ->
            printfn "DynamicEchoActor received message: %s" command.RawArgu
            let rawArgu = command.RawArgu.Trim()
            
            let replyValue =
                if rawArgu.StartsWith("{") || rawArgu.StartsWith("[") then
                    // 嘗試解析 JSON 為 fCell2，然後打包成 fskynet-sdui Payload
                    try
                        let ast = PulseTrade.Comm.Spa.Dynamic.Server.FCell2Interop.fromJsonString rawArgu
                        match ast with
                        | fCell2.N _ -> 
                            printfn "DynamicEchoActor failed to parse JSON"
                            fCell2.S ("Failed to parse JSON: " + rawArgu)
                        | _ -> 
                            let payload = PulseTrade.Comm.Spa.Dynamic.Server.FCell2Interop.toMessagePayload ast
                            printfn "DynamicEchoActor parsed JSON successfully, payload length: %d" payload.Length
                            fCell2.S payload
                    with ex ->
                        printfn "DynamicEchoActor exception during parsing/payload: %A" ex
                        fCell2.S ("Error: " + ex.Message)
                else
                    let replyText =
                        "dynamic echo raw="
                        + command.RawArgu
                        + "; argv="
                        + String.concat "|" command.ParsedArgv
                    fCell2.S replyText

            let reply: ActorArguTargetReply =
                { Value = replyValue
                  Direction = Some "inbound-message"
                  Tags = Some [ "poc"; "dynamic"; "echo"; "sdui" ] }

            printfn "DynamicEchoActor sending reply..."
            this.ActorCtx.Sender.Tell(reply, this.ActorCtx.Self)
            printfn "DynamicEchoActor reply sent.")
        |> ignore

    member _.ActorCtx: IActorContext = ActorBase.Context

let fsiArgs () =
    let values = fsi.CommandLineArgs

    if values.Length <= 1 then
        [||]
    else
        values
        |> Array.skip 1
        |> Array.filter (fun value -> value <> "--")

let defaultArgs () =
    PL.parseLine [| ' ' |] (Some '"') None true defaultArgumentsText

let freeTcpPort () =
    use listener = new TcpListener(IPAddress.Loopback, 0)
    listener.Start()
    let endpoint = listener.LocalEndpoint :?> IPEndPoint
    endpoint.Port

let parser = ArgumentParser.Create<CliArguments>(programName = "poc.full.nuget.fsx")

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

let fullPath (path: string) =
    Path.GetFullPath(textOr defaultPcslRoot path)

let siteSharingOf (value: string) =
    match textOr "isolated" value |> fun text -> text.ToLowerInvariant() with
    | "shared" -> Shared
    | "isolated" -> Isolated
    | other -> invalidArg "site-sharing" $"Unsupported site sharing mode: {other}. Use shared or isolated."

let webBinding host port =
    let host = textOr "127.0.0.1" host

    if port <= 0 then
        WebBinding.randomHost host
    else
        WebBinding.fixedHostPort host port

let durableIngressOptions profileId =
    let profileId = textOr "poc-durable" profileId
    let retry =
        { DurableDeliveryRetryOptions.defaults with
            DeadLetterStreamKey = "poc.durable.dead-letter" }

    { CommSpaDurableIngressOptions.volatileLocal() with
        Mode = DurableIngressMode.DurableDelivery
        ProfileId = profileId
        ServerReality =
            { CommSpaServerReality.localVolatile profileId with
                DeliveryProfileId = profileId }
        Retry = retry }

let require condition message =
    if not condition then
        failwithf "poc.dynamic failed: %s" message

let host =
    let defaultValue = defaultParsed.GetResult(Host, "127.0.0.1")
    overrideParsed
    |> Option.map (fun parsed -> parsed.GetResult(Host, defaultValue))
    |> Option.defaultValue defaultValue

let port =
    let defaultValue = defaultParsed.GetResult(Port, 0)
    overrideParsed
    |> Option.map (fun parsed -> parsed.GetResult(Port, defaultValue))
    |> Option.defaultValue defaultValue

let pcslRoot =
    let defaultValue = defaultParsed.GetResult(Pcsl_Root, defaultPcslRoot)
    overrideParsed
    |> Option.map (fun parsed -> parsed.GetResult(Pcsl_Root, defaultValue))
    |> Option.defaultValue defaultValue
    |> fullPath

let siteSharing =
    let defaultValue = defaultParsed.GetResult(Site_Sharing, "isolated")
    overrideParsed
    |> Option.map (fun parsed -> parsed.GetResult(Site_Sharing, defaultValue))
    |> Option.defaultValue defaultValue
    |> siteSharingOf

let deliveryProfile =
    let defaultValue = defaultParsed.GetResult(Delivery_Profile, "poc-durable")
    overrideParsed
    |> Option.map (fun parsed -> parsed.GetResult(Delivery_Profile, defaultValue))
    |> Option.defaultValue defaultValue
    |> textOr "poc-durable"

let actorName =
    let defaultValue = defaultParsed.GetResult(Actor_Name, "durable-echo")
    overrideParsed
    |> Option.map (fun parsed -> parsed.GetResult(Actor_Name, defaultValue))
    |> Option.defaultValue defaultValue
    |> textOr "durable-echo"

let clusterPort =
    let defaultValue = defaultParsed.GetResult(Cluster_Port, 7705)
    overrideParsed
    |> Option.map (fun parsed -> parsed.GetResult(Cluster_Port, defaultValue))
    |> Option.defaultValue defaultValue
    |> fun port -> if port <= 0 then freeTcpPort () else port

let noWait =
    defaultParsed.Contains No_Wait
    || (overrideParsed |> Option.exists (fun parsed -> parsed.Contains No_Wait))

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

Directory.CreateDirectory pcslRoot |> ignore

let hub = CommHub.createEmptyWithPcslRoot pcslRoot

let fabricOptions =
    { CommSpaActorFabricOptions.defaults with
        SystemName = "PulseTradeCommSpaDynamicPoc"
        ShardTypeName = "comm-spa-dynamic-poc"
        ClusterPort = clusterPort }

let options =
    ServerOptions.localRandomWithHub hub
    |> ServerOptions.withWebBinding (webBinding host port)
    |> Server.withActorFabricOptions fabricOptions

let app =
    withStartupOutput (fun () -> Server.startWithSharing siteSharing options)

let mutable pocFullNugetStopped = false

let stopPocFullNuget () =
    if not pocFullNugetStopped then
        pocFullNugetStopped <- true
        (app :> IDisposable).Dispose()
        printfn "poc.full.nuget stopped."

let fabric =
    app.ActorFabric
    |> Option.defaultWith (fun () -> failwith "Expected Server.startWithSharing to expose ActorFabric.")

// ---- DYNAMIC EXTENSION 掛載 ----
withStartupOutput (fun () -> hub.useDynamicSdui(fabric.System)) |> ignore
// --------------------------------

let ingress = durableIngressOptions deliveryProfile |> CommSpaDurableIngress.createVolatile
let messageFabric = CommSpaMessageFabric.createDurable hub ingress

let actorRef =
    fabric.System.ActorOf(Props.Create(fun () -> DurableEchoActor()), actorName)

let actorAddress =
    fabric.NodeAddress.TrimEnd('/') + actorRef.Path.ToStringWithoutAddress()

let actorPage =
    let basePage = ActorArgu.fCellChatPage "actor-argu-durable-poc" "ActorArgu Durable POC" "actor-argu-durable-poc"
    { basePage with DefaultKey = "\"" + actorAddress + "\"" }

let dynamicEchoActorRef =
    fabric.System.ActorOf(Props.Create(fun () -> DynamicEchoActor()), "dynamic-echo-actor")

let dynamicEchoActorAddress =
    fabric.NodeAddress.TrimEnd('/') + dynamicEchoActorRef.Path.ToStringWithoutAddress()

let dynamicEchoPage =
    let basePage = ActorArgu.fCellChatPage "actor-dynamic-echo-v2" "Actor Dynamic Echo V2" "actor-dynamic-echo-v2"
    { basePage with DefaultKey = "\"" + dynamicEchoActorAddress + "\""; Shape = "actor-dynamic" }

let showcaseActorAddress = fabric.NodeAddress.TrimEnd('/') + "/user/showcase-dynamic-actor"

let showcasePage =
    let basePage = ActorArgu.fCellChatPage "actor-dynamic-showcase" "Actor Dynamic Showcase" "actor-dynamic-showcase"
    { basePage with DefaultKey = "\"" + showcaseActorAddress + "\""; Shape = "actor-dynamic" }

let cancellationToken = CancellationToken.None

try
    let registerUser =
        messageFabric.RegisterParticipantDurableAsync
            { ParticipantId = "user.poc"
              DisplayName = Some "POC User"
              Kind = Some "user"
              Labels = Some [ "poc"; "durable"; "without-oauth" ] }
        |> fun task -> task.Result

    let registerShowcaseAgent =
        messageFabric.RegisterParticipantDurableAsync
            { ParticipantId = "agent.showcase"
              DisplayName = Some "Showcase Actor"
              Kind = Some "agent"
              Labels = Some [ "poc"; "durable"; "showcase"; "agent" ] }
        |> fun task -> task.Result

    let registerDynamicEchoAgent =
        messageFabric.RegisterParticipantDurableAsync
            { ParticipantId = "agent.dynamic-echo"
              DisplayName = Some "Dynamic Echo Actor"
              Kind = Some "agent"
              Labels = Some [ "poc"; "durable"; "echo"; "agent" ] }
        |> fun task -> task.Result

    let registerAgent =
        messageFabric.RegisterParticipantDurableAsync
            { ParticipantId = "agent.poc"
              DisplayName = Some "POC Agent"
              Kind = Some "agent"
              Labels = Some [ "poc"; "durable"; "agent" ] }
        |> fun task -> task.Result

    let showcaseDirectMessage =
        messageFabric.SendDurableAsync
            { FromParticipantId = "user.poc"
              Scope = MessageFabricScope.Direct "agent.showcase"
              Body = "hello from showcase"
              Tags = [ "poc"; "dynamic"; "showcase" ]
              CorrelationId = Some("poc-dynamic-showcase-" + Guid.NewGuid().ToString("N"))
              CreatedAtUtc = None }
        |> fun task -> task.Result

    // 原始 durable-echo agent 註冊
    let registerDurableEchoAgent =
        messageFabric.RegisterParticipantDurableAsync
            { ParticipantId = "agent.durable-echo"
              DisplayName = Some "Durable Echo"
              Kind = Some "agent"
              Labels = Some [ "poc"; "durable"; "actor-argu" ] }
        |> fun task -> task.Result

    let directMessage =
        messageFabric.SendDurableAsync
            { FromParticipantId = "user.poc"
              Scope = MessageFabricScope.Direct "agent.durable-echo"
              Body = "hello from durable echo"
              Tags = [ "poc"; "durable"; "actor-argu" ]
              CorrelationId = Some("poc-durable-echo-" + Guid.NewGuid().ToString("N"))
              CreatedAtUtc = None }
        |> fun task -> task.Result

    let showcaseResult =
        ActorArgu.sendDurableAsync
            ingress
            fabric
            { Send =
                { Page = showcasePage
                  ActorAddress = showcaseActorAddress
                  HistoryKeys = None
                  RawArgu = "--render"
                  Tags = Some [ "poc"; "dynamic"; "showcase" ] }
              IdempotencyKey = None
              Source = Some "poc.dynamic.fsx"
              DeadlineAtUtc = None }
        |> fun asyncFunc -> asyncFunc cancellationToken
        |> Async.RunSynchronously

    let actorResult =
        ActorArgu.sendDurableAsync
            ingress
            fabric
            { Send =
                { Page = actorPage
                  ActorAddress = actorAddress
                  HistoryKeys = None
                  RawArgu = "--echo \"hello dynamic\""
                  Tags = Some [ "poc"; "durable"; "actor-argu" ] }
              IdempotencyKey = None
              Source = Some "poc.dynamic.fsx"
              DeadlineAtUtc = None }
            cancellationToken
        |> Async.RunSynchronously

    let dynamicEchoResult =
        ActorArgu.sendDurableAsync
            ingress
            fabric
            { Send =
                { Page = dynamicEchoPage
                  ActorAddress = dynamicEchoActorAddress
                  HistoryKeys = None
                  RawArgu = System.IO.File.ReadAllText(System.IO.Path.Combine(__SOURCE_DIRECTORY__, "canvas_demo.json"))
                  Tags = Some [ "poc"; "dynamic"; "echo"; "sdui" ] }
              IdempotencyKey = None
              Source = Some "poc.dynamic.fsx"
              DeadlineAtUtc = None }
            cancellationToken
        |> Async.RunSynchronously

    // 測試 ShowcaseDemoActor 回應
    let showcaseRef = fabric.System.ActorSelection("user/showcase-dynamic-actor")
    let showcaseResponse = showcaseRef.Ask<string>("init", TimeSpan.FromSeconds(5.0)) |> Async.AwaitTask |> Async.RunSynchronously

    let health = ingress.HealthAsync cancellationToken |> fun task -> task.Result
    use client = new HttpClient()
    let healthText = client.GetStringAsync(app.Url + "/healthz").Result


    printfn "PTC.Comm.Spa + Dynamic Extension POC is running."
    printfn "Chat        %s/chat" app.Url
    printfn "Sets        %s/sets" app.Url
    printfn "Actors      %s/actors" app.Url
    printfn "ActorArgu   %s/page/%s" app.Url actorPage.PageId
    printfn "DynEcho     %s/page/%s" app.Url dynamicEchoPage.PageId
    printfn "PCSL root   %s" pcslRoot
    printfn "Showcase    %s" showcaseActorAddress
    printfn "DynEcho     %s" dynamicEchoActorAddress
    printfn "Actor       %s" actorAddress
    printfn "Tickets     user=%s agent=%s direct=%s actor=%s"
        registerUser.Accepted.TicketId.Value
        registerAgent.Accepted.TicketId.Value
        directMessage.Accepted.TicketId.Value
        actorResult.Accepted.TicketId.Value
    printfn "Ingress     mode=%A crashDurable=%b pending=%d deadLetters=%d"
        health.Mode
        health.IsCrashDurable
        health.PendingCount
        health.DeadLetterCount
        
    printfn "--------------------------------------------------------"
    printfn "[ShowcaseDemoActor Reply (fskynet-sdui JSON Schema)]:"
    printfn "%s" showcaseResponse
    printfn "--------------------------------------------------------"

    if noWait then
        printfn "No-wait smoke completed; stopping server."
        stopPocFullNuget ()
    else
        printfn "Host remains running in this FSI session."
        printfn "Run stopPocFullNuget() to stop."
with ex ->
    stopPocFullNuget ()
    reraise()

