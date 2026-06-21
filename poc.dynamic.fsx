#i @"nuget: C:\Program Files\dotnet\sdk\10.0.300\FSharp\library-packs"
#r "nuget: PulseTrade.Comm.Spa, 0.2.4-beta7"
#r "nuget: FAkka.Argu, 10.1.301"
#r "nuget: FAkka.FCell2, 10.1.301"
#r "nuget: Akka, 1.5.69"
#I __SOURCE_DIRECTORY__
#r "src/bin/Release/net10.0/PulseTrade.Comm.Spa.Dynamic.dll"

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

let defaultPcslRoot =
    Path.Combine(__SOURCE_DIRECTORY__, ".pcsl", "poc.dynamic.fsx")

let defaultArgPath (path: string) =
    if String.IsNullOrWhiteSpace path then "" else path.Replace('\\', '/')

let defaultArgumentsText =
    $"""--host 127.0.0.1 --port 0 --site-sharing isolated --pcsl-root "{defaultArgPath defaultPcslRoot}" --delivery-profile poc-durable --actor-name durable-echo"""

type CliArguments =
    | Host of host: string
    | Port of port: int
    | Site_Sharing of mode: string
    | Pcsl_Root of path: string
    | Delivery_Profile of profile: string
    | Actor_Name of name: string
    | No_Wait
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Host _ -> "Local HTTP bind host."
            | Port _ -> "Local HTTP port. Use 0 for a random free port."
            | Site_Sharing _ -> "Site sharing mode: shared or isolated."
            | Pcsl_Root _ -> "Root directory for PCSL files."
            | Delivery_Profile _ -> "Durable ingress profile id."
            | Actor_Name _ -> "Echo actor name under /user."
            | No_Wait -> "Start and stop immediately for smoke tests."

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

let parseArguments () =
    let parser = ArgumentParser.Create<CliArguments>(programName = "poc.dynamic.fsx")
    let args = fsiArgs ()
    let effectiveArgs = if args.Length = 0 then defaultArgs () else args
    parser.ParseCommandLine effectiveArgs

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

let parsed = parseArguments ()

let host =
    parsed.TryGetResult(<@ Host @>)
    |> Option.defaultValue "127.0.0.1"

let port =
    parsed.TryGetResult(<@ Port @>)
    |> Option.defaultValue 0

let pcslRoot =
    parsed.TryGetResult(<@ Pcsl_Root @>)
    |> Option.defaultValue defaultPcslRoot
    |> fullPath

let siteSharing =
    parsed.TryGetResult(<@ Site_Sharing @>)
    |> Option.defaultValue "isolated"
    |> siteSharingOf

let deliveryProfile =
    parsed.TryGetResult(<@ Delivery_Profile @>)
    |> Option.defaultValue "poc-durable"
    |> textOr "poc-durable"

let actorName =
    parsed.TryGetResult(<@ Actor_Name @>)
    |> Option.defaultValue "durable-echo"
    |> textOr "durable-echo"

let noWait = parsed.Contains <@ No_Wait @>

Directory.CreateDirectory pcslRoot |> ignore

let hub = CommHub.createEmptyWithPcslRoot pcslRoot

let fabricOptions =
    { CommSpaActorFabricOptions.defaults with
        SystemName = "PulseTradeCommSpaDynamicPoc"
        ShardTypeName = "comm-spa-dynamic-poc"
        ClusterPort = 0 }

let options =
    ServerOptions.localRandomWithHub hub
    |> ServerOptions.withWebBinding (webBinding host port)
    |> Server.withActorFabricOptions fabricOptions

let app = Server.startWithSharing siteSharing options

let fabric =
    app.ActorFabric
    |> Option.defaultWith (fun () -> failwith "Expected Server.startWithSharing to expose ActorFabric.")

// ---- DYNAMIC EXTENSION 掛載 ----
hub.useDynamicSdui(fabric.System) |> ignore
// --------------------------------

let ingress = durableIngressOptions deliveryProfile |> CommSpaDurableIngress.createVolatile
let messageFabric = CommSpaMessageFabric.createDurable hub ingress

let actorRef =
    fabric.System.ActorOf(Props.Create(fun () -> DurableEchoActor()), actorName)

let actorAddress =
    fabric.NodeAddress.TrimEnd('/') + actorRef.Path.ToStringWithoutAddress()

let actorPage =
    ActorArgu.fCellChatPage "actor-argu-durable-poc" "ActorArgu Durable POC" "actor-argu-durable-poc"

let cancellationToken = CancellationToken.None

try
    let registerUser =
        messageFabric.RegisterParticipantDurableAsync
            { ParticipantId = "user.poc"
              DisplayName = Some "POC User"
              Kind = Some "user"
              Labels = Some [ "poc"; "durable"; "without-oauth" ] }
        |> fun task -> task.Result

    let registerAgent =
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
              Body = "hello from dynamic POC"
              Tags = [ "poc"; "durable"; "dynamic" ]
              CorrelationId = Some("poc-dynamic-" + Guid.NewGuid().ToString("N"))
              CreatedAtUtc = None }
        |> fun task -> task.Result

    let actorResult =
        ActorArgu.sendDurableAsync
            ingress
            fabric
            { Send =
                { Page = actorPage
                  ActorAddress = actorAddress
                  RawArgu = "--echo \"hello dynamic\""
                  Tags = Some [ "poc"; "durable"; "actor-argu" ] }
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
    printfn "PCSL root   %s" pcslRoot
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
    else
        printfn "Press Enter to stop."
        Console.ReadLine() |> ignore
finally
    (app :> IDisposable).Dispose()
