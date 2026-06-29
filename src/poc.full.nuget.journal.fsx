//
// Start a PTCS + PTCS.Dynamic host from NuGet packages with a durable Akka
// Persistence journal. PCSL is treated as projection/cache: clearing the PCSL
// root should not delete the SQL journal, and startup warms the projection back
// from journaled stream actors.
//
// Default Visual Studio FSI use:
// 1. Ensure PulseTrade.Comm.Spa / PulseTrade.Comm.Spa.Dynamic nupkgs are built
//    and available in the #i package roots below.
// 2. Edit defaultArgumentsText only if you want a fixed port or PCSL root.
// 3. Select all and run. Call stopPingPongActor() to observe /actors reload,
//    then call stopPocFullNugetJournalHost() to stop the host.
//

#i @"nuget: C:\Program Files\dotnet\sdk\10.0.301\FSharp\library-packs"
#r "nuget: FAkka.Argu, 10.1.301"
#r "nuget: FAkka.FCell2, 10.1.301"
#r "nuget: Akka, 1.5.69"
#r "nuget: Akka.Cluster, 1.5.69"
#r "nuget: Akka.Cluster.Sharding, 1.5.69"
#r "nuget: Akka.Persistence, 1.5.69"
#r "nuget: Akka.Persistence.Sql, 1.5.67"
#r "nuget: Microsoft.Data.SqlClient, 7.0.1"
#r "nuget: System.Data.SqlClient, 4.9.1"
#r "nuget: PersistedConcurrentSortedList, 10.1.301"
#r "nuget: PulseTrade.Comm.Actor.Registry, [0.1.0-alpha4]"
#r "nuget: PulseTrade.Comm.Spa, [0.2.5-beta43]"
#r "nuget: PulseTrade.Comm.Spa.Dynamic, [0.1.3-beta33]"

#load @"C:\Users\Administrator\.codex\lib\ParseLine.fsx"

open System
open System.IO
open System.Net
open System.Net.Http
open System.Net.Sockets
open System.Security.Cryptography
open System.Text
open System.Text.Json
open System.Threading
open Akka.Actor
open Argu
open PersistedConcurrentSortedList.Type
open PulseTrade.Comm.Actor.Registry
open PulseTrade.Comm.Spa
open PulseTrade.Comm.Spa.Dynamic.Server

let defaultArtifactRoot =
    @"G:\PulseTrade.fs.Comm.Log\manual\ptcsDynamicNugetJournal"

let defaultPcslRoot =
    Path.Combine(defaultArtifactRoot, "pcsl_journal_001")

let defaultClusterPort = 9787

let pathArg (path: string) =
    (if isNull path then "" else path).Replace("\\", "/")

let normalizePathForKey (path: string) =
    Path.GetFullPath(path)
        .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
        .ToLowerInvariant()

let stableHash (text: string) =
    let bytes = Encoding.UTF8.GetBytes text
    let hash = SHA256.HashData bytes
    BitConverter.ToString(hash, 0, 8).Replace("-", "").ToLowerInvariant()

let sqlDbNameForPcslRoot pcslRoot =
    "PTCSDynJ_" + stableHash (normalizePathForKey pcslRoot)

let freePort () =
    use listener = new TcpListener(IPAddress.Loopback, 0)
    listener.Start()
    let port = (listener.LocalEndpoint :?> IPEndPoint).Port
    listener.Stop()
    port

let defaultArgumentsText =
    let webPort = freePort ()
    printfn "allocated web port: %d, default cluster port: %d" webPort defaultClusterPort
    $"""--host 127.0.0.1 --port {webPort} --site-sharing isolated --pcsl-root "{pathArg defaultPcslRoot}" --delivery-profile nuget-journal-live --actor-name nuget-journal-echo --cluster-port {defaultClusterPort}"""

type CliArgs =
    | Host of string
    | Port of int
    | Site_Sharing of string
    | Pcsl_Root of string
    | Sql_Db of string
    | Sql_Connection_String of string
    | Delivery_Profile of string
    | Actor_Name of string
    | Cluster_Port of int
    | Clear_Pcsl_Before_Start
    | No_Wait
    | Block
    | Verbose_Startup
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Host _ -> "HTTP bind host."
            | Port _ -> "HTTP bind port. Use 0 for a random free port."
            | Site_Sharing _ -> "Site sharing mode: isolated or shared."
            | Pcsl_Root _ -> "PCSL projection root for this live host."
            | Sql_Db _ -> "SQL Server database used by the durable journal. Defaults to a hash of --pcsl-root."
            | Sql_Connection_String _ -> "SQL Server connection string. The value is never printed."
            | Delivery_Profile _ -> "Durable ingress profile id."
            | Actor_Name _ -> "Echo actor name under /user."
            | Cluster_Port _ -> "Local Akka cluster port."
            | Clear_Pcsl_Before_Start -> "Clear the PCSL projection root before startup. This does not delete the SQL journal."
            | No_Wait -> "Start, verify /healthz and /chat markers, then stop."
            | Block -> "Block on ActorSystem termination for browser testing."
            | Verbose_Startup -> "Do not suppress PTCS/Dynamic startup asset logs."

type PocMode =
    | Fast
    | Safe
    | Audit

type PocFullNugetJournalArgu =
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

type PocFullNugetJournalEchoActor() as this =
    inherit ReceiveActor()

    do
        this.Receive<ActorArguTargetCommand>(fun (command: ActorArguTargetCommand) ->
            let replyText =
                "poc.full.nuget.journal echo raw="
                + command.RawArgu
                + "; argv="
                + String.concat "|" command.ParsedArgv

            let reply: ActorArguTargetReply =
                { Value = fCell2.S replyText
                  Direction = Some "inbound-message"
                  Tags = Some [ "poc-full-nuget-journal"; "echo" ] }

            this.ActorCtx.Sender.Tell(reply, this.ActorCtx.Self))
        |> ignore

    member _.ActorCtx: IActorContext = ActorBase.Context

type PocFullNugetJournalPingPongMessage =
    | Ping of text: string
    | Stop

type PocFullNugetJournalPingPongActor() as this =
    inherit ReceiveActor()

    do
        this.Receive<string>(fun text ->
            let reply =
                if String.Equals(text, "ping", StringComparison.OrdinalIgnoreCase) then
                    "pong"
                else
                    "pong:" + text

            this.ActorCtx.Sender.Tell(reply, this.ActorCtx.Self))
        |> ignore

        this.Receive<PocFullNugetJournalPingPongMessage>(fun message ->
            match message with
            | Ping text -> this.ActorCtx.Sender.Tell("pong:" + text, this.ActorCtx.Self)
            | Stop -> this.ActorCtx.Stop(this.ActorCtx.Self))
        |> ignore

    member _.ActorCtx: IActorContext = ActorBase.Context

let parser =
    ArgumentParser.Create<CliArgs>(programName = "poc.full.nuget.journal.fsx")

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
    let profileId = textOr "nuget-journal-live" profileId

    { CommSpaDurableIngressOptions.volatileLocal() with
        Mode = DurableIngressMode.DurableDelivery
        ProfileId = profileId
        ServerReality =
            { CommSpaServerReality.localVolatile profileId with
                DeliveryProfileId = profileId }
        Retry =
            { DurableDeliveryRetryOptions.defaults with
                DeadLetterStreamKey = "ptcs.dynamic.poc-full-nuget-journal.dead-letter" } }

let host =
    let defaultValue = defaultParsed.GetResult(Host, "127.0.0.1")
    overrideParsed |> Option.map (fun parsed -> parsed.GetResult(Host, defaultValue)) |> Option.defaultValue defaultValue

let port =
    let defaultValue = defaultParsed.GetResult(Port, 0)
    overrideParsed |> Option.map (fun parsed -> parsed.GetResult(Port, defaultValue)) |> Option.defaultValue defaultValue

let pcslRoot =
    let defaultValue = defaultParsed.GetResult(Pcsl_Root, defaultPcslRoot)
    overrideParsed |> Option.map (fun parsed -> parsed.GetResult(Pcsl_Root, defaultValue)) |> Option.defaultValue defaultValue |> fullPath defaultPcslRoot

let sqlDb =
    let explicitValue =
        overrideParsed
        |> Option.bind (fun parsed -> parsed.TryGetResult <@ Sql_Db @>)
        |> Option.map (textOr "")
        |> Option.filter (String.IsNullOrWhiteSpace >> not)

    explicitValue |> Option.defaultValue (sqlDbNameForPcslRoot pcslRoot)

let siteSharing =
    let defaultValue = defaultParsed.GetResult(Site_Sharing, "isolated")
    overrideParsed |> Option.map (fun parsed -> parsed.GetResult(Site_Sharing, defaultValue)) |> Option.defaultValue defaultValue |> siteSharingOf

let deliveryProfile =
    let defaultValue = defaultParsed.GetResult(Delivery_Profile, "nuget-journal-live")
    overrideParsed |> Option.map (fun parsed -> parsed.GetResult(Delivery_Profile, defaultValue)) |> Option.defaultValue defaultValue

let actorName =
    let defaultValue = defaultParsed.GetResult(Actor_Name, "nuget-journal-echo")
    overrideParsed |> Option.map (fun parsed -> parsed.GetResult(Actor_Name, defaultValue)) |> Option.defaultValue defaultValue |> textOr "nuget-journal-echo"

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

let clearPcslBeforeStart =
    defaultParsed.Contains Clear_Pcsl_Before_Start
    || (overrideParsed |> Option.exists (fun parsed -> parsed.Contains Clear_Pcsl_Before_Start))

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

let ensureSafeClearPcslRoot (root: string) =
    let rootFull = Path.GetFullPath root
    let allowedFull = Path.GetFullPath defaultArtifactRoot

    if not (rootFull.StartsWith(allowedFull, StringComparison.OrdinalIgnoreCase)) then
        invalidOp $"Refusing to clear PCSL root outside default artifact root: {rootFull}"

    if Directory.Exists rootFull then
        for entry in Directory.EnumerateFileSystemEntries rootFull do
            let entryFull = Path.GetFullPath entry

            if not (entryFull.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase)) then
                invalidOp $"Refusing to clear unexpected path: {entryFull}"

            if Directory.Exists entryFull then
                Directory.Delete(entryFull, true)
            elif File.Exists entryFull then
                File.Delete entryFull

let defaultCanonicalArgString =
    "--say \"hello from journal poc\" --set-count 3 --mode safe --at TTC 7 --tag aoe \"marvel now\" --verbose"

let templateKey = "poc-full-nuget-journal-argu"

let templateRegistration =
    DynamicArguTemplateRegistration.create
        templateKey
        typeof<PocFullNugetJournalArgu>
        DynamicArguAliasBinding.empty
        (Some defaultCanonicalArgString)

if clearPcslBeforeStart then
    ensureSafeClearPcslRoot pcslRoot

Directory.CreateDirectory pcslRoot |> ignore

let journal =
    match overrideParsed |> Option.bind (fun parsed -> parsed.TryGetResult <@ Sql_Connection_String @>) with
    | Some connectionString when not (String.IsNullOrWhiteSpace connectionString) ->
        Journal.sqlServer(connectionString.Trim(), autoCreateDatabase = true)
    | _ ->
        Journal.sqlServerLocal(dbName = sqlDb, autoCreateDatabase = true)

let journalBootstrap =
    Journal.ensureSqlServerDatabase journal

let persistenceHash = stableHash (sqlDb + "|" + normalizePathForKey pcslRoot)
let persistenceNamespace =
    { CommSpaPersistenceNamespaceOptions.defaults with
        PersistenceNamespace = "ptcs-dynamic-journal"
        PersistenceIdPrefix = "ptcsdyn-" + persistenceHash + "-" }
    |> CommSpaPersistenceNamespaceOptions.normalize

let journalQueryAdapter =
    Journal.sqlServerQueryHealthAdapter(commandTimeoutSeconds = 5, persistenceIdPrefix = persistenceNamespace.PersistenceIdPrefix)

let pcslOptions =
    { PcslCommSpaPersistenceOptions.defaults with
        BasePath = pcslRoot
        SeedInitialStateWhenEmpty = false }
    |> fun options -> Journal.withPcslProjectionJournalId(journal, options)

let projectionHub =
    CommHub.createEmptyWithPcslOptions pcslOptions

let fabricOptions =
    { CommSpaActorFabricOptions.defaults with
        SystemName = "PtcsDynamicPocJournal" + persistenceHash.Substring(0, 8)
        ShardTypeName = "ptcs-dynamic-poc-journal-" + persistenceHash.Substring(0, 8)
        ClusterPort = clusterPort }
    |> CommSpaActorFabricOptions.withJournal journal
    |> CommSpaActorFabricOptions.withJournalQueryAdapter journalQueryAdapter
    |> CommSpaActorFabricOptions.withPersistenceNamespace persistenceNamespace

let fabric =
    CommSpaActorFabric.startWithOptions fabricOptions projectionHub.PersistenceBackend

let writerProfile =
    PcslWriter.forwardToWriter(
        fabric.Region.Path.ToString(),
        nodeId = "poc-full-nuget-journal-ui",
        ownerId = "poc-full-nuget-journal-region",
        projectionId = "poc-full-nuget-journal")

let proxyBackend =
    PcslActorProxyCommSpaPersistenceBackend(
        projectionHub.PersistenceBackend,
        fabric.Region,
        writerProfile,
        askTimeout = fabricOptions.AskTimeout,
        remoteWire = true)
    :> ICommSpaPersistenceBackend

let hub =
    CommHub(Domain.empty, proxyBackend)

let forceReplay streamKey =
    fabric.SnapshotAsync
        { StreamKey = streamKey
          DesiredTailCount = 1000
          BrowserWatermark = None
          IncludeMetadata = true }
    |> Async.RunSynchronously
    |> ignore

let tryForceReplay label streamKey =
    try
        forceReplay streamKey
        true
    with ex ->
        printfn "journal warm-up skipped %s: %s" label ex.Message
        false

let warmUpProjectionFromJournal () =
    let mutable forced = 0

    let force label key =
        if tryForceReplay label key then
            forced <- forced + 1

    force "append-page-registry" CommSpaShardedAppendPage.pageRegistryStreamKey
    force "actor-registry" CommSpaActorRegistry.registryStreamKey
    force "participant-registry" CommSpaParticipantRegistry.registryStreamKey
    force "generic-set-registry" CommSpaGenericSet.registryStreamKey

    let genericSetStreamKeys =
        projectionHub.ReadStreamTail(CommSpaGenericSet.registryStreamKey, 1000)
        |> List.choose CommSpaGenericSet.tryReadStreamKey
        |> List.distinctBy CommSpaStreamKey.canonical

    for streamKey in genericSetStreamKeys do
        force "generic-set-value" streamKey

    let pagesAfterRegistry =
        hub.ListAppendPages().Pages

    for page in pagesAfterRegistry do
        for keyStream in CommSpaShardedAppendPage.keyRegistryStreamKeys page do
            force ("append-page-key-registry:" + page.PageId) keyStream

    let pagesAfterKeys =
        hub.ListAppendPages().Pages

    for page in pagesAfterKeys do
        let keys = hub.ListAppendPageKeys(page.PageId).Keys

        for key in keys do
            for valueStream in CommSpaShardedAppendPage.valueStreamKeys page key.Keys do
                force ("append-page-value:" + page.PageId + ":" + key.KeyId) valueStream

    let pageCount = (hub.ListAppendPages()).Count
    let actorCount = (hub.ActorsSnapshot()).ActorCount
    printfn "journal warm-up streams=%d pages=%d actors=%d" forced pageCount actorCount

warmUpProjectionFromJournal ()

let options =
    ServerOptions.localRandomWithHub hub
    |> ServerOptions.withWebBinding (webBinding host port)
    |> Server.withActorFabric fabric

let app =
    withStartupOutput (fun () -> Server.startWithSharing siteSharing options)

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

let actorRegistrySettings =
    ActorRegistrySettings.create (hub.ActorRegistrySink())
    |> ActorRegistrySettings.withRegistryId "ptcs-dynamic-poc-full-nuget-journal"
    |> ActorRegistrySettings.withNodeId (Some "ptcs.dynamic.poc-full-nuget-journal")
    |> ActorRegistrySettings.withRole (Some "ptcs-dynamic-journal")
    |> ActorRegistrySettings.withTags [ "ptcs-dynamic"; "poc-full-nuget-journal"; "echo"; "actor-argu" ]
    |> ActorRegistrySettings.withMetadata (
        Map.ofList
            [ "ptcs.dynamic.poc", "poc.full.nuget.journal.fsx"
              "ptcs.dynamic.journal.db", journalBootstrap.DatabaseName
              "ptcs.dynamic.pcsl.root", pcslRoot ])

let echoRef =
    match fabric.System.ActorOfRegistered(actorRegistrySettings, Props.Create(fun () -> PocFullNugetJournalEchoActor()), actorName) with
    | Ok registered -> registered.Actor
    | Error error ->
        invalidOp $"ActorRegistry ActorOfRegistered failed actor={actorName} kind={error.Kind} message={error.Message}"

let actorAddress =
    fabric.NodeAddress.TrimEnd('/') + echoRef.Path.ToStringWithoutAddress()

tryForceReplay "actor-registry-after-register" CommSpaActorRegistry.registryStreamKey |> ignore

let pingPongActorName =
    actorName + "-pingpong"

let pingPongRegistrySettings =
    ActorRegistrySettings.create (hub.ActorRegistrySink())
    |> ActorRegistrySettings.withRegistryId "ptcs-dynamic-poc-full-nuget-journal-pingpong"
    |> ActorRegistrySettings.withNodeId (Some "ptcs.dynamic.poc-full-nuget-journal")
    |> ActorRegistrySettings.withRole (Some "ptcs-dynamic-journal")
    |> ActorRegistrySettings.withTags [ "ptcs-dynamic"; "poc-full-nuget-journal"; "pingpong"; "actor-registry-reload" ]
    |> ActorRegistrySettings.withMetadata (
        Map.ofList
            [ "ptcs.dynamic.poc", "poc.full.nuget.journal.fsx"
              "ptcs.dynamic.actor.kind", "pingpong"
              "ptcs.dynamic.journal.db", journalBootstrap.DatabaseName
              "ptcs.dynamic.pcsl.root", pcslRoot ])

let pingPongRef =
    match fabric.System.ActorOfRegistered(pingPongRegistrySettings, Props.Create(fun () -> PocFullNugetJournalPingPongActor()), pingPongActorName) with
    | Ok registered -> registered.Actor
    | Error error ->
        invalidOp $"ActorRegistry ActorOfRegistered failed actor={pingPongActorName} kind={error.Kind} message={error.Message}"

let pingPongActorAddress =
    fabric.NodeAddress.TrimEnd('/') + pingPongRef.Path.ToStringWithoutAddress()

tryForceReplay "actor-registry-after-pingpong-register" CommSpaActorRegistry.registryStreamKey |> ignore

let targetKeys =
    [ actorAddress; templateKey; defaultCanonicalArgString ]

let actorPage =
    let basePage = ActorArgu.fCellChatPage "actor-argu-poc-full-nuget-journal" "ActorArgu POC Full NuGet Journal" "actor-argu-poc-full-nuget-journal"
    { basePage with
        Shape = "actor-argu"
        Description = "NuGet-loaded PTCS.Dynamic FormInput POC using SQL Akka journal plus PCSL projection warm-up."
        KeyPlaceholder = "[\"actor-address\", \"template-key\", \"--say \\\"hello\\\"\"]"
        DefaultKey = JsonSerializer.Serialize(targetKeys |> List.toArray)
        ValuePlaceholder = defaultCanonicalArgString
        Tags = [ "actor-argu"; "dynamic"; "nuget"; "journal" ] }

let pageRegistryHasHistory =
    hub.ReadStreamTail(CommSpaShardedAppendPage.pageRegistryStreamKey, 1000).Length > 0

let seededDefaultPage =
    if pageRegistryHasHistory then
        false
    else
        hub.RegisterAppendPage actorPage |> ignore
        hub.RegisterAppendPageKeyWithDisplayName(actorPage.PageId, targetKeys, "Journal FormInput target") |> ignore
        true

let defaultTargetVisible () =
    match hub.TryFindAppendPage actorPage.PageId with
    | None -> false
    | Some _ ->
        hub.ListAppendPageKeys(actorPage.PageId).Keys
        |> List.exists (fun key -> key.Keys = targetKeys)

let mutable pocFullNugetJournalStopped = false
let mutable pingPongActorStopped = false

let require condition message =
    if not condition then
        failwith message

let readActorsSnapshotCounts (jsonText: string) =
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

    nodeCount, actorCount

let stopPingPongActor () =
    if pingPongActorStopped then
        printfn "PingPong actor already stopped: %s" pingPongActorAddress
    else
        pingPongActorStopped <- true
        pingPongRef.Tell(PocFullNugetJournalPingPongMessage.Stop)
        Thread.Sleep 750
        tryForceReplay "actor-registry-after-pingpong-stop" CommSpaActorRegistry.registryStreamKey |> ignore
        let hubActorCount = (hub.ActorsSnapshot()).ActorCount
        printfn "PingPong actor stop requested: %s" pingPongActorAddress
        printfn "Reload actors page after stop: %s/actors" app.Url
        printfn "Current hub actor projection count: %d" hubActorCount

let stopPocFullNugetJournalHost () =
    if pocFullNugetJournalStopped then
        printfn "poc.full.nuget.journal host already stopped."
    else
        pocFullNugetJournalStopped <- true
        (app :> IDisposable).Dispose()
        fabric.Stop()
        fabric.System.WhenTerminated.Wait(TimeSpan.FromSeconds 10.0) |> ignore
        printfn "poc.full.nuget.journal host stopped."

try
    let targetVisible = defaultTargetVisible ()

    let serverProbe =
        if targetVisible then
            Some(
                ActorArgu.sendDurableAsync
                    ingress
                    fabric
                    { Send =
                        { Page = actorPage
                          ActorAddress = actorAddress
                          HistoryKeys = Some targetKeys
                          RawArgu = "--say \"server probe\" --set-count 1 --mode fast"
                          Tags = Some [ "poc-full-nuget-journal"; "server-probe" ] }
                      IdempotencyKey = None
                      Source = Some "poc.full.nuget.journal.fsx"
                      DeadlineAtUtc = None }
                    CancellationToken.None
                |> Async.RunSynchronously)
        else
            None

    use client = new HttpClient()
    let healthText = client.GetStringAsync(app.Url + "/healthz").GetAwaiter().GetResult()
    let journalHealthText = client.GetStringAsync(app.Url + "/healthz.journal").GetAwaiter().GetResult()
    let persistenceHealthText = client.GetStringAsync(app.Url + "/healthz.persistence").GetAwaiter().GetResult()
    let chatHtml = client.GetStringAsync(app.Url + "/chat").GetAwaiter().GetResult()
    let actorsHtml = client.GetStringAsync(app.Url + "/actors").GetAwaiter().GetResult()
    let actorsSnapshotJson = client.GetStringAsync(app.Url + "/actors/api/snapshot").GetAwaiter().GetResult()
    let actorsSnapshotWithOfflineJson = client.GetStringAsync(app.Url + "/actors/api/snapshot?includeOffline=1").GetAwaiter().GetResult()
    let dynamicJs = client.GetStringAsync(app.Url + "/ext/js/PulseTrade.Comm.Spa.Dynamic.js").GetAwaiter().GetResult()
    let actorDynamicCreateShapeVisible =
        hub.ListClientExtensions()
        |> List.collect _.AppendPageShapes
        |> List.exists (fun shape -> String.Equals(shape.Shape, "actor-dynamic", StringComparison.OrdinalIgnoreCase))

    require (healthText.Contains("PulseTrade.Comm.Spa")) "healthz should identify PTCS."
    require (journalHealthText.Contains("sql-server")) "journal health should use sql-server profile."
    require (healthText.Contains("pcsl-actor-proxy")) "healthz hub persistence should expose pcsl-actor-proxy."
    require (chatHtml.Length > 0) "chat page should be served by PTCS."
    require (not actorDynamicCreateShapeVisible) "journal POC must not expose +page Actor Dynamic shape in the extension manifest."
    require (not (chatHtml.Contains("option value=\"actor-dynamic\""))) "journal POC must not expose +page Actor Dynamic shape."
    require (actorsHtml.Length > 0) "actors page should be served."
    let actorsNodeCount, actorsActorCount = readActorsSnapshotCounts actorsSnapshotJson
    let actorsNodeCountWithOffline, actorsActorCountWithOffline = readActorsSnapshotCounts actorsSnapshotWithOfflineJson
    let hubActorCount = (hub.ActorsSnapshot()).ActorCount
    require (hubActorCount > 0) $"hub actor projection should have actors, got {hubActorCount}."
    require (dynamicJs.Contains("dynamic-actors-page")) "Dynamic bundle should include ActorsPage renderer."
    require (dynamicJs.Contains("dynamic-argu-add-key")) "Dynamic bundle should include Add target key renderer."

    match serverProbe with
    | Some probe ->
        require probe.ActorArgu.IsSome "server probe should append an ActorArgu reply."
        let deliveryStatusText = string probe.DeliveryStatus.Status
        require
            (String.Equals(deliveryStatusText, "completed", StringComparison.OrdinalIgnoreCase))
            ("server probe delivery should complete: " + deliveryStatusText)
    | None ->
        printfn "Default target probe skipped because the journal projection currently has no visible default target."

    printfn "PTCS Dynamic POC Full NuGet Journal started."
    printfn "Base URL      %s" app.Url
    printfn "Chat URL      %s/chat" app.Url
    printfn "Actors URL    %s/actors" app.Url
    printfn "Actors data   visibleNodes=%d visibleActors=%d includeOfflineNodes=%d includeOfflineActors=%d hubActors=%d" actorsNodeCount actorsActorCount actorsNodeCountWithOffline actorsActorCountWithOffline hubActorCount
    printfn "ActorArgu URL %s/page/%s" app.Url actorPage.PageId
    printfn "Dynamic JS    %s/ext/js/PulseTrade.Comm.Spa.Dynamic.js" app.Url
    printfn "PCSL root     %s" pcslRoot
    printfn "SQL journal   db=%s created=%b existed=%b" journalBootstrap.DatabaseName journalBootstrap.Created journalBootstrap.AlreadyExisted
    printfn "Persistence   namespace=%s prefix=%s" persistenceNamespace.PersistenceNamespace persistenceNamespace.PersistenceIdPrefix
    printfn "Projection    backend=pcsl-actor-proxy clearBeforeStart=%b seededDefaultPage=%b" clearPcslBeforeStart seededDefaultPage
    printfn "Actor address %s" actorAddress
    printfn "PingPong actor %s" pingPongActorAddress
    printfn "Template key  %s" templateKey
    printfn "Target key    %s" (JsonSerializer.Serialize(targetKeys |> List.toArray))
    printfn "Default arg   %s" defaultCanonicalArgString
    printfn "Stop pingpong with stopPingPongActor()"
    printfn "Stop with     stopPocFullNugetJournalHost()"

    if noWait then
        stopPocFullNugetJournalHost ()
    elif block then
        printfn "Blocking on ActorSystem termination."
        fabric.System.WhenTerminated.Wait() |> ignore
with ex ->
    stopPocFullNugetJournalHost ()
    reraise ()
