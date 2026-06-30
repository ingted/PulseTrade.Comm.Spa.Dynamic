//
// Start a PTCS + PTCS.Dynamic host from NuGet packages with a durable Akka
// Persistence journal plus PTCS.ACL/PTCS.Login integration.
//
// Port 81 uses GitHub OAuth browser auth. Port 82 uses the built-in PTCS.Login
// username/password provider. Both ports share the same hub, journal, actor
// registry, Dynamic SDUI extension, and ACL policy.
//
// PCSL is treated as projection/cache: clearing the PCSL root should not delete
// the SQL journal, and startup warms the projection back from journaled stream
// actors.
//
// Default Visual Studio FSI use:
// 1. Ensure PulseTrade.Comm.Spa / PulseTrade.Comm.Spa.Dynamic nupkgs are built
//    and available in the #i package roots below.
// 2. Edit defaultArgumentsText only if you want fixed ports, OAuth paths, or PCSL root.
// 3. Select all and run. Call stopPingPongActor() to observe /actors reload.
//    Echo actor helpers:
//      ensureEchoActorRegistered()
//      stopEchoActor()
//      recreateEchoActor()
//    Do not rerun ActorOfRegistered with the same actorName while that actor
//    is still live. After stop + path release, the same name is reusable.
//    Call stopPocFullNugetJournalAclHosts() to stop both web hosts.
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
#r "nuget: PulseTrade.Comm.Actor.Registry, [0.1.0-alpha5]"
#r "nuget: PulseTrade.Comm.ACL.Core, [0.1.0-alpha2]"
#r "nuget: PulseTrade.Comm.Login.Core, [0.1.0-alpha5]"
#r "nuget: PulseTrade.Comm.Spa, [0.2.5-beta58]"
#r "nuget: PulseTrade.Comm.Spa.Dynamic, [0.1.3-beta48]"

#load @"C:\Users\Administrator\.codex\lib\ParseLine.fsx"

open System
open System.Collections.Concurrent
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
open PulseTrade.Comm.ACL.Core
open PulseTrade.Comm.Actor.Registry
open PulseTrade.Comm.Login.Core
open PulseTrade.Comm.Spa
open PulseTrade.Comm.Spa.Dynamic.Server

let defaultArtifactRoot =
    @"G:\PulseTrade.fs.Comm.Log\manual\ptcsDynamicNugetJournalAcl"

let defaultPcslRoot =
    Path.Combine(defaultArtifactRoot, "pcsl_journal_001")

let defaultClusterPort = 9787
let defaultGitHubOAuthClientIdPath =
    @"G:\GITHUB\ChatTest\GitHubOAuthClientId.txt"

let defaultGitHubOAuthClientSecretPath =
    @"G:\GITHUB\ChatTest\GitHubOAuthClientSecret.txt"

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
    printfn "default ports: github-oauth=81, local-login=82, cluster=%d" defaultClusterPort
    $"""--host 0.0.0.0 --github-port 81 --local-port 82 --github-public-base-url "https://my-ai.co.in:81" --github-oauth-client-id-path "{pathArg defaultGitHubOAuthClientIdPath}" --github-oauth-client-secret-path "{pathArg defaultGitHubOAuthClientSecretPath}" --site-sharing isolated --pcsl-root "{pathArg defaultPcslRoot}" --delivery-profile nuget-journal-acl-live --actor-name nuget-journal-acl-echo --cluster-port {defaultClusterPort} --sql-connection-string "Data Source=.;Initial Catalog=master;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Application Name=PTCSDynamicJournalAcl" """

type CliArgs =
    | Host of string
    | Github_Port of int
    | Local_Port of int
    | Github_Public_Base_Url of string
    | Github_Oauth_Client_Id_Path of string
    | Github_Oauth_Client_Secret_Path of string
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
            | Github_Port _ -> "HTTP bind port for GitHub OAuth. Use 0 for a random free port."
            | Local_Port _ -> "HTTP bind port for local username/password login. Use 0 for a random free port."
            | Github_Public_Base_Url _ -> "External public base URL for GitHub OAuth callback, for example https://my-ai.co.in:81."
            | Github_Oauth_Client_Id_Path _ -> "Local file path containing GitHub OAuth client id. The value is not printed."
            | Github_Oauth_Client_Secret_Path _ -> "Local file path containing GitHub OAuth client secret. The value is not printed."
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

        this.Receive<ActorArguTargetCommand>(fun (command: ActorArguTargetCommand) ->
            let reply: ActorArguTargetReply =
                { Value = fCell2.S("poc.full.nuget.journal.acl pingpong raw=" + command.RawArgu)
                  Direction = Some "inbound-message"
                  Tags = Some [ "poc-full-nuget-journal-acl"; "pingpong" ] }

            this.ActorCtx.Sender.Tell(reply, this.ActorCtx.Self))
        |> ignore

    member _.ActorCtx: IActorContext = ActorBase.Context

let parser =
    ArgumentParser.Create<CliArgs>(programName = "poc.full.nuget.journal.ACL.fsx")

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

let githubPort =
    let defaultValue = defaultParsed.GetResult(Github_Port, 81)
    overrideParsed |> Option.map (fun parsed -> parsed.GetResult(Github_Port, defaultValue)) |> Option.defaultValue defaultValue

let localPort =
    let defaultValue = defaultParsed.GetResult(Local_Port, 82)
    overrideParsed |> Option.map (fun parsed -> parsed.GetResult(Local_Port, defaultValue)) |> Option.defaultValue defaultValue

let githubPublicBaseUrl =
    let defaultValue = defaultParsed.GetResult(Github_Public_Base_Url, "https://my-ai.co.in:81")
    overrideParsed |> Option.map (fun parsed -> parsed.GetResult(Github_Public_Base_Url, defaultValue)) |> Option.defaultValue defaultValue |> textOr "https://my-ai.co.in:81"

let githubOAuthClientIdPath =
    let defaultValue = defaultParsed.GetResult(Github_Oauth_Client_Id_Path, defaultGitHubOAuthClientIdPath)
    overrideParsed |> Option.map (fun parsed -> parsed.GetResult(Github_Oauth_Client_Id_Path, defaultValue)) |> Option.defaultValue defaultValue |> fullPath defaultGitHubOAuthClientIdPath

let githubOAuthClientSecretPath =
    let defaultValue = defaultParsed.GetResult(Github_Oauth_Client_Secret_Path, defaultGitHubOAuthClientSecretPath)
    overrideParsed |> Option.map (fun parsed -> parsed.GetResult(Github_Oauth_Client_Secret_Path, defaultValue)) |> Option.defaultValue defaultValue |> fullPath defaultGitHubOAuthClientSecretPath

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

let aclOptions =
    match PtcsAcl.create (AclPolicyConfig.demo()) with
    | Ok value -> value
    | Error error -> invalidOp $"PTCS ACL demo policy decode failed: {error.Message}"

let loginOptions =
    match PtcsLogin.demoLocalDev () with
    | Ok value -> value
    | Error error -> invalidOp $"PTCS.Login demo config decode failed: {error.Message}"

let readRequiredSecret path label =
    if not (File.Exists path) then
        invalidOp $"Missing {label} file for GitHub OAuth: {path}"

    let value = File.ReadAllText(path, Encoding.UTF8).Trim()

    if String.IsNullOrWhiteSpace value then
        invalidOp $"{label} file is empty: {path}"

    value

let githubClientId =
    readRequiredSecret githubOAuthClientIdPath "GitHub OAuth client id"

let githubOptions =
    ServerOptions.localRandomWithHub hub
    |> ServerOptions.withWebBinding (webBinding host githubPort)
    |> Server.withActorFabric fabric
    |> Server.withGitHubOAuth githubClientId githubOAuthClientSecretPath (Some githubPublicBaseUrl)
    |> Server.withAcl aclOptions

let localLoginOptions =
    ServerOptions.localRandomWithHub hub
    |> ServerOptions.withWebBinding (webBinding host localPort)
    |> Server.withActorFabric fabric
    |> Server.withPtcsLogin loginOptions
    |> Server.withAcl aclOptions

let githubApp =
    withStartupOutput (fun () -> Server.startWithSharing siteSharing githubOptions)

let localApp =
    withStartupOutput (fun () -> Server.startWithSharing siteSharing localLoginOptions)

withStartupOutput (fun () ->
    hub.useDynamicSdui(fabric.System, DynamicArguMetadata.empty, [ templateRegistration ])
    |> ignore

    hub.ListClientExtensions()
    |> List.tryFind (fun extension -> extension.ExtensionId = "pulse-trade-comm-spa-dynamic")
    |> Option.iter (fun extension ->
        hub.RegisterClientExtension({ extension with AppendPageShapes = [] })
        |> ignore))

let actorRegistryEvents = ConcurrentQueue<ActorRegistryLifecycleEvent>()

let actorRegistrySink : ActorRegistryEventSink =
    let hubSink = hub.ActorRegistrySink()

    fun event ->
        task {
            actorRegistryEvents.Enqueue event

            try
                do! hubSink event
            with ex ->
                printfn "ActorRegistry sink failed: event=%A status=%A path=%s error=%s" event.EventKind event.Status event.FullPath ex.Message
                return raise ex
        }

let ingress =
    durableIngressOptions deliveryProfile |> CommSpaDurableIngress.createVolatile

let actorRegistrySettings =
    ActorRegistrySettings.create actorRegistrySink
    |> ActorRegistrySettings.withRegistryId "ptcs-dynamic-poc-full-nuget-journal"
    |> ActorRegistrySettings.withNodeId (Some "ptcs.dynamic.poc-full-nuget-journal")
    |> ActorRegistrySettings.withRole (Some "ptcs-dynamic-journal")
    |> ActorRegistrySettings.withTags [ "ptcs-dynamic"; "poc-full-nuget-journal"; "echo"; "actor-argu" ]
    |> ActorRegistrySettings.withMetadata (
        Map.ofList
            [ "ptcs.dynamic.poc", "poc.full.nuget.journal.ACL.fsx"
              "ptcs.dynamic.journal.db", journalBootstrap.DatabaseName
              "ptcs.dynamic.pcsl.root", pcslRoot ])

let userActorPath actorName =
    "/user/" + actorName

let tryResolveUserActor actorName =
    try
        Some(fabric.System.ActorSelection(userActorPath actorName).ResolveOne(TimeSpan.FromMilliseconds 750.0).GetAwaiter().GetResult())
    with _ ->
        None

let waitUntilUserActorGone actorName timeout =
    let deadline = DateTimeOffset.UtcNow.Add(timeout)
    let mutable resolved = tryResolveUserActor actorName

    while resolved.IsSome && DateTimeOffset.UtcNow < deadline do
        Thread.Sleep 100
        resolved <- tryResolveUserActor actorName

    resolved.IsNone

let isActorNameNotUnique (error: ActorRegistryActorOfError) =
    error.Kind = ActorRegistryActorOfErrorKind.ActorSpawnFailed
    && error.Message.IndexOf("not unique", StringComparison.OrdinalIgnoreCase) >= 0

let actorOfRegisteredEcho () =
    fabric.System.ActorOfRegistered(actorRegistrySettings, Props.Create(fun () -> PocFullNugetJournalEchoActor()), actorName)

let spawnEchoActorRegisteredStrict () =
    match actorOfRegisteredEcho () with
    | Ok registered -> registered.Actor
    | Error error ->
        invalidOp $"ActorRegistry ActorOfRegistered failed actor={actorName} kind={error.Kind} message={error.Message}"

let spawnEchoActorRegisteredOrReuseLive () =
    match actorOfRegisteredEcho () with
    | Ok registered -> registered.Actor
    | Error error when isActorNameNotUnique error ->
        match tryResolveUserActor actorName with
        | Some existing ->
            printfn "Echo actor already exists; reusing %s. Use recreateEchoActor() to stop, wait, and reuse the same actor name." (existing.Path.ToStringWithoutAddress())
            existing
        | None ->
            invalidOp $"ActorRegistry ActorOfRegistered found duplicate name but could not resolve existing actor={actorName} message={error.Message}"
    | Error error ->
        invalidOp $"ActorRegistry ActorOfRegistered failed actor={actorName} kind={error.Kind} message={error.Message}"

let mutable echoRef = spawnEchoActorRegisteredOrReuseLive ()
let mutable echoActorStopped = false

let actorAddress =
    fabric.NodeAddress.TrimEnd('/') + echoRef.Path.ToStringWithoutAddress()

tryForceReplay "actor-registry-after-register" CommSpaActorRegistry.registryStreamKey |> ignore

let pingPongActorName =
    actorName + "-pingpong"

let pingPongRegistrySettings =
    ActorRegistrySettings.create actorRegistrySink
    |> ActorRegistrySettings.withRegistryId "ptcs-dynamic-poc-full-nuget-journal-pingpong"
    |> ActorRegistrySettings.withNodeId (Some "ptcs.dynamic.poc-full-nuget-journal")
    |> ActorRegistrySettings.withRole (Some "ptcs-dynamic-journal")
    |> ActorRegistrySettings.withTags [ "ptcs-dynamic"; "poc-full-nuget-journal"; "pingpong"; "actor-registry-reload" ]
    |> ActorRegistrySettings.withMetadata (
        Map.ofList
            [ "ptcs.dynamic.poc", "poc.full.nuget.journal.ACL.fsx"
              "ptcs.dynamic.actor.kind", "pingpong"
              "ptcs.dynamic.journal.db", journalBootstrap.DatabaseName
              "ptcs.dynamic.pcsl.root", pcslRoot ])

let pingPongRegistered =
    match fabric.System.ActorOfRegistered(pingPongRegistrySettings, Props.Create(fun () -> PocFullNugetJournalPingPongActor()), pingPongActorName) with
    | Ok registered ->
        if registered.Watcher.IsNone then
            invalidOp $"ActorRegistry watcher was not created for actor={pingPongActorName}; stop/reload lifecycle cannot be verified."

        registered
    | Error error ->
        invalidOp $"ActorRegistry ActorOfRegistered failed actor={pingPongActorName} kind={error.Kind} message={error.Message}"

let pingPongRef = pingPongRegistered.Actor

let pingPongActorAddress =
    fabric.NodeAddress.TrimEnd('/') + pingPongRef.Path.ToStringWithoutAddress()

tryForceReplay "actor-registry-after-pingpong-register" CommSpaActorRegistry.registryStreamKey |> ignore

let echoTargetKeys =
    [ actorAddress; templateKey; defaultCanonicalArgString ]

let pingPongTargetKeys =
    [ pingPongActorAddress; templateKey; "--say \"ping\" --set-count 2 --mode fast --tag acl pingpong" ]

let actorArguPage pageId title description =
    let basePage = ActorArgu.fCellChatPage pageId title pageId

    { basePage with
        Shape = "actor-argu"
        Description = description
        KeyPlaceholder = "[\"actor-address\", \"template-key\", \"--say \\\"hello\\\"\"]"
        DefaultKey = JsonSerializer.Serialize(echoTargetKeys |> List.toArray)
        ValuePlaceholder = defaultCanonicalArgString
        Tags = [ "actor-argu"; "dynamic"; "nuget"; "journal"; "acl" ] }

let damnWzPage =
    actorArguPage
        "DamnWZ"
        "DamnWZ"
        "ACL demo page for sys-admin. The admin user should have every page/action capability."

let assTerryPage =
    actorArguPage
        "AssTerry"
        "AssTerry"
        "ACL demo page for Terry黑粉. The Terry user may send/remove targets here but must not add new targets."

let actorPages =
    [ damnWzPage; assTerryPage ]

let targetBindings =
    [ echoTargetKeys, "Echo target"
      pingPongTargetKeys, "PingPong target" ]

let registerDemoPageIfMissing (page: AppendPageDefinition) =
    let existingPage = hub.TryFindAppendPage page.PageId
    let pageAdded =
        match existingPage with
        | Some _ -> false
        | None ->
            hub.RegisterAppendPage page |> ignore
            true

    let existingKeys =
        hub.ListAppendPageKeys(page.PageId).Keys
        |> List.map _.Keys
        |> Set.ofList

    let mutable addedKeys = 0

    for keys, displayName in targetBindings do
        if not (existingKeys.Contains keys) then
            hub.RegisterAppendPageKeyWithDisplayName(page.PageId, keys, displayName) |> ignore
            addedKeys <- addedKeys + 1

    pageAdded, addedKeys

let seededDefaultPage =
    actorPages
    |> List.map registerDemoPageIfMissing
    |> List.exists (fun (pageAdded, addedKeys) -> pageAdded || addedKeys > 0)

let defaultTargetVisible () =
    actorPages
    |> List.forall (fun page ->
        match hub.TryFindAppendPage page.PageId with
        | None -> false
        | Some _ ->
            let existing =
                hub.ListAppendPageKeys(page.PageId).Keys
                |> List.map _.Keys
                |> Set.ofList

            targetBindings |> List.forall (fun (keys, _) -> existing.Contains keys))

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

let urlForLocalClient (url: string) =
    let uri = Uri(url)
    let host =
        if String.Equals(uri.Host, "0.0.0.0", StringComparison.OrdinalIgnoreCase)
           || String.Equals(uri.Host, "::", StringComparison.OrdinalIgnoreCase) then
            "127.0.0.1"
        else
            uri.Host

    UriBuilder(uri.Scheme, host, uri.Port).Uri.ToString().TrimEnd('/')

let localClientBaseUrl =
    urlForLocalClient localApp.Url

let githubClientBaseUrl =
    urlForLocalClient githubApp.Url

let loginLocalClientAsAdmin (client: HttpClient) =
    let body =
        JsonSerializer.Serialize(
            {| userName = "admin"
               password = "demo:admin"
               returnUrl = "/actors"
               keepSession = true |})

    use request = new HttpRequestMessage(HttpMethod.Post, localClientBaseUrl + "/login/api/submit")
    request.Headers.TryAddWithoutValidation("Origin", localClientBaseUrl) |> ignore
    request.Headers.Referrer <- Uri(localClientBaseUrl + "/login?returnUrl=/actors")
    request.Content <- new StringContent(body, Encoding.UTF8, "application/json")

    use response = client.SendAsync(request).GetAwaiter().GetResult()
    let responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult()

    if not response.IsSuccessStatusCode then
        invalidOp $"Local PTCS.Login admin login failed: status={int response.StatusCode} body={responseBody}"

    let mutable cookieValues = Seq.empty<string>

    if not (response.Headers.TryGetValues("Set-Cookie", &cookieValues)) then
        invalidOp "Local PTCS.Login did not return Set-Cookie."

    let sessionCookie =
        cookieValues
        |> Seq.tryPick (fun value ->
            value.Split(';', StringSplitOptions.RemoveEmptyEntries)
            |> Array.tryHead
            |> Option.map _.Trim()
            |> Option.filter (String.IsNullOrWhiteSpace >> not))
        |> Option.defaultWith (fun () -> invalidOp "Local PTCS.Login returned an empty Set-Cookie header.")

    client.DefaultRequestHeaders.Remove("Cookie") |> ignore
    client.DefaultRequestHeaders.TryAddWithoutValidation("Cookie", sessionCookie) |> ignore
    responseBody

let actorPathHasExactName (actorName: string) (pathText: string) =
    pathText.EndsWith("/" + actorName, StringComparison.Ordinal)

let tryFindHubActorStatus (actorName: string) =
    hub.ActorsSnapshot().Nodes
    |> List.collect _.Actors
    |> List.tryFind (fun actor -> actorPathHasExactName actorName actor.ActorId)
    |> Option.map _.Status

let waitForHubActorStatus (actorName: string) (expectedStatus: string) timeout =
    let deadline = DateTimeOffset.UtcNow.Add(timeout)
    let mutable observed = tryFindHubActorStatus actorName

    while observed <> Some expectedStatus && DateTimeOffset.UtcNow < deadline do
        Thread.Sleep 100
        observed <- tryFindHubActorStatus actorName

    observed

let registryEventSummary (actorName: string) =
    actorRegistryEvents
    |> Seq.filter (fun event -> actorPathHasExactName actorName event.FullPath)
    |> Seq.map (fun event -> $"{event.EventKind}/{event.Status}@{event.StatusVersion}")
    |> String.concat ", "

let ensureEchoActorRegistered () =
    match tryResolveUserActor actorName with
    | Some existing ->
        echoRef <- existing
        echoActorStopped <- false
        printfn "Echo actor is already live: %s" (fabric.NodeAddress.TrimEnd('/') + existing.Path.ToStringWithoutAddress())
        existing
    | None ->
        let spawned = spawnEchoActorRegisteredStrict ()
        echoRef <- spawned
        echoActorStopped <- false
        tryForceReplay "actor-registry-after-echo-ensure" CommSpaActorRegistry.registryStreamKey |> ignore
        printfn "Echo actor registered: %s" (fabric.NodeAddress.TrimEnd('/') + spawned.Path.ToStringWithoutAddress())
        spawned

let stopEchoActor () =
    match tryResolveUserActor actorName with
    | None ->
        echoActorStopped <- true
        printfn "Echo actor already stopped: %s" actorAddress
    | Some live ->
        echoActorStopped <- true
        fabric.System.Stop(live)
        let pathReleased = waitUntilUserActorGone actorName (TimeSpan.FromSeconds 10.0)
        let observedStatus = waitForHubActorStatus actorName "terminated" (TimeSpan.FromSeconds 5.0)
        tryForceReplay "actor-registry-after-echo-stop" CommSpaActorRegistry.registryStreamKey |> ignore
        printfn "Echo actor stop requested: %s" (fabric.NodeAddress.TrimEnd('/') + live.Path.ToStringWithoutAddress())
        printfn "Echo actor path released: %b" pathReleased
        printfn "Echo actor projected status: %A" observedStatus
        printfn "Echo registry events: %s" (registryEventSummary actorName)

let recreateEchoActor () =
    match tryResolveUserActor actorName with
    | Some _ -> stopEchoActor ()
    | None -> ()

    if not (waitUntilUserActorGone actorName (TimeSpan.FromSeconds 10.0)) then
        invalidOp $"Echo actor path did not release before recreate: {userActorPath actorName}"

    let spawned = spawnEchoActorRegisteredStrict ()
    echoRef <- spawned
    echoActorStopped <- false
    tryForceReplay "actor-registry-after-echo-recreate" CommSpaActorRegistry.registryStreamKey |> ignore
    printfn "Echo actor recreated: %s" (fabric.NodeAddress.TrimEnd('/') + spawned.Path.ToStringWithoutAddress())
    spawned

let verifyEchoActorReuseAfterStop () =
    let beforePath = echoRef.Path.ToStringWithoutAddress()
    let recreated = recreateEchoActor ()
    let afterPath = recreated.Path.ToStringWithoutAddress()

    require
        (String.Equals(beforePath, afterPath, StringComparison.Ordinal))
        $"Echo actor should reuse the same path after stop/recreate. before={beforePath} after={afterPath}"

    require
        ((tryResolveUserActor actorName).IsSome)
        $"Echo actor should resolve after recreate: {userActorPath actorName}"

    printfn "Echo actor reuse-after-stop verified: %s" (fabric.NodeAddress.TrimEnd('/') + afterPath)
    true

let stopPingPongActor () =
    if pingPongActorStopped then
        printfn "PingPong actor already stopped: %s" pingPongActorAddress
    else
        pingPongActorStopped <- true
        fabric.System.Stop(pingPongRef)
        let observedStatus = waitForHubActorStatus pingPongActorName "terminated" (TimeSpan.FromSeconds 5.0)
        tryForceReplay "actor-registry-after-pingpong-stop" CommSpaActorRegistry.registryStreamKey |> ignore
        let hubActorCount = (hub.ActorsSnapshot()).ActorCount
        printfn "PingPong actor stop requested: %s" pingPongActorAddress
        printfn "PingPong actor projected status: %A" observedStatus
        printfn "PingPong registry events: %s" (registryEventSummary pingPongActorName)
        printfn "Reload actors page after stop: %s/actors" localClientBaseUrl
        printfn "Current hub actor projection count: %d" hubActorCount

let stopPocFullNugetJournalAclHosts () =
    if pocFullNugetJournalStopped then
        printfn "poc.full.nuget.journal.ACL hosts already stopped."
    else
        pocFullNugetJournalStopped <- true
        (localApp :> IDisposable).Dispose()
        (githubApp :> IDisposable).Dispose()
        fabric.Stop()
        fabric.System.WhenTerminated.Wait(TimeSpan.FromSeconds 10.0) |> ignore
        printfn "poc.full.nuget.journal.ACL hosts stopped."

try
    let ensuredEcho = ensureEchoActorRegistered ()
    let targetVisible = defaultTargetVisible ()

    let serverProbe =
        if targetVisible then
            Some(
                ActorArgu.sendDurableAsync
                    ingress
                    fabric
                    { Send =
                        { Page = assTerryPage
                          ActorAddress = actorAddress
                          HistoryKeys = Some echoTargetKeys
                          RawArgu = "--say \"server probe\" --set-count 1 --mode fast"
                          Tags = Some [ "poc-full-nuget-journal-acl"; "server-probe" ] }
                      IdempotencyKey = None
                      Source = Some "poc.full.nuget.journal.ACL.fsx"
                      DeadlineAtUtc = None }
                    CancellationToken.None
                |> Async.RunSynchronously)
        else
            None

    use client = new HttpClient()
    let localLoginReply = loginLocalClientAsAdmin client
    let healthText = client.GetStringAsync(localClientBaseUrl + "/healthz").GetAwaiter().GetResult()
    let githubHealthText = client.GetStringAsync(githubClientBaseUrl + "/healthz").GetAwaiter().GetResult()
    let journalHealthText = client.GetStringAsync(localClientBaseUrl + "/healthz.journal").GetAwaiter().GetResult()
    let persistenceHealthText = client.GetStringAsync(localClientBaseUrl + "/healthz.persistence").GetAwaiter().GetResult()
    let chatHtml = client.GetStringAsync(localClientBaseUrl + "/chat").GetAwaiter().GetResult()
    let actorsHtml = client.GetStringAsync(localClientBaseUrl + "/actors").GetAwaiter().GetResult()
    let actorsSnapshotJson = client.GetStringAsync(localClientBaseUrl + "/actors/api/snapshot").GetAwaiter().GetResult()
    let actorsSnapshotWithOfflineJson = client.GetStringAsync(localClientBaseUrl + "/actors/api/snapshot?includeOffline=1").GetAwaiter().GetResult()
    let aclSnapshotJson = client.GetStringAsync(localClientBaseUrl + "/acl/api/snapshot").GetAwaiter().GetResult()
    let dynamicJs = client.GetStringAsync(localClientBaseUrl + "/ext/js/PulseTrade.Comm.Spa.Dynamic.js").GetAwaiter().GetResult()
    let actorDynamicCreateShapeVisible =
        hub.ListClientExtensions()
        |> List.collect _.AppendPageShapes
        |> List.exists (fun shape -> String.Equals(shape.Shape, "actor-dynamic", StringComparison.OrdinalIgnoreCase))

    require (healthText.Contains("PulseTrade.Comm.Spa")) "healthz should identify PTCS."
    require (githubHealthText.Contains("PulseTrade.Comm.Spa")) "GitHub OAuth host healthz should identify PTCS."
    require (localLoginReply.Contains("user.admin", StringComparison.OrdinalIgnoreCase)) "local PTCS.Login admin login should identify user.admin."
    require (aclSnapshotJson.Contains("ptcs.page.create", StringComparison.Ordinal)) "ACL snapshot should expose PTCS capability keys."
    require (journalHealthText.Contains("sql-server")) "journal health should use sql-server profile."
    require (healthText.Contains("pcsl-actor-proxy")) "healthz hub persistence should expose pcsl-actor-proxy."
    require (chatHtml.Length > 0) "chat page should be served by PTCS."
    require (not actorDynamicCreateShapeVisible) "journal POC must not expose +page Actor Dynamic shape in the extension manifest."
    require (not (chatHtml.Contains("option value=\"actor-dynamic\""))) "journal POC must not expose +page Actor Dynamic shape."
    require (actorsHtml.Length > 0) "actors page should be served."
    let actorsNodeCount, actorsActorCount = readActorsSnapshotCounts actorsSnapshotJson
    let actorsNodeCountWithOffline, actorsActorCountWithOffline = readActorsSnapshotCounts actorsSnapshotWithOfflineJson
    let hubActorCount = (hub.ActorsSnapshot()).ActorCount
    let mutable afterStopActorsNodeCount = -1
    let mutable afterStopActorsActorCount = -1
    let mutable afterStopIncludeOfflineActorCount = -1
    let mutable echoReuseAfterStopVerified = false

    require
        (String.Equals(ensuredEcho.Path.ToStringWithoutAddress(), echoRef.Path.ToStringWithoutAddress(), StringComparison.Ordinal))
        "ensureEchoActorRegistered should reuse the existing echo actor instead of trying to spawn a duplicate actor name."

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

    if noWait then
        stopPingPongActor ()
        Thread.Sleep 250
        let afterStopActorsSnapshotJson = client.GetStringAsync(localClientBaseUrl + "/actors/api/snapshot").GetAwaiter().GetResult()
        let afterStopActorsSnapshotWithOfflineJson = client.GetStringAsync(localClientBaseUrl + "/actors/api/snapshot?includeOffline=1").GetAwaiter().GetResult()
        let afterStopActorsTreeJson = client.GetStringAsync(localClientBaseUrl + "/actors/api/tree").GetAwaiter().GetResult()
        let afterStopNodeCount, afterStopActorCount = readActorsSnapshotCounts afterStopActorsSnapshotJson
        let _, afterStopOfflineActorCount = readActorsSnapshotCounts afterStopActorsSnapshotWithOfflineJson

        afterStopActorsNodeCount <- afterStopNodeCount
        afterStopActorsActorCount <- afterStopActorCount
        afterStopIncludeOfflineActorCount <- afterStopOfflineActorCount

        require
            (not (afterStopActorsSnapshotJson.Contains(pingPongActorName, StringComparison.Ordinal)))
            $"active actors snapshot should not include stopped PingPong actor {pingPongActorName}."

        require
            (not (afterStopActorsTreeJson.Contains(pingPongActorName, StringComparison.Ordinal)))
            $"active ActorTopology tree should not include stopped PingPong actor {pingPongActorName}."

        require
            (afterStopActorsSnapshotWithOfflineJson.Contains(pingPongActorName, StringComparison.Ordinal))
            $"includeOffline actors snapshot should retain stopped PingPong actor {pingPongActorName} for diagnostics."

        require
            (afterStopActorsSnapshotWithOfflineJson.Contains("terminated", StringComparison.OrdinalIgnoreCase))
            "includeOffline actors snapshot should expose the stopped actor terminated status."

        echoReuseAfterStopVerified <- verifyEchoActorReuseAfterStop ()

    printfn "PTCS Dynamic POC Full NuGet Journal ACL started."
    printfn "GitHub OAuth URL   %s/actors" githubClientBaseUrl
    printfn "GitHub public URL  %s/actors" githubPublicBaseUrl
    printfn "Local login URL    %s/login?returnUrl=/actors" localClientBaseUrl
    printfn "Local chat URL     %s/chat" localClientBaseUrl
    printfn "Local actors URL   %s/actors" localClientBaseUrl
    printfn "Local demo users   admin=admin / terry=terry; passwords are demo-only and defined by LoginConfig.demo()."
    printfn "Actors data   visibleNodes=%d visibleActors=%d includeOfflineNodes=%d includeOfflineActors=%d hubActors=%d" actorsNodeCount actorsActorCount actorsNodeCountWithOffline actorsActorCountWithOffline hubActorCount
    if noWait then
        printfn "After stop    visibleNodes=%d visibleActors=%d includeOfflineActors=%d pingPongFiltered=true" afterStopActorsNodeCount afterStopActorsActorCount afterStopIncludeOfflineActorCount
        printfn "Echo reuse    reuseAfterStop=%b" echoReuseAfterStopVerified
    printfn "ActorArgu URLs %s/page/%s ; %s/page/%s" localClientBaseUrl damnWzPage.PageId localClientBaseUrl assTerryPage.PageId
    printfn "Dynamic JS    %s/ext/js/PulseTrade.Comm.Spa.Dynamic.js" localClientBaseUrl
    printfn "PCSL root     %s" pcslRoot
    printfn "SQL journal   db=%s created=%b existed=%b" journalBootstrap.DatabaseName journalBootstrap.Created journalBootstrap.AlreadyExisted
    printfn "Persistence   namespace=%s prefix=%s" persistenceNamespace.PersistenceNamespace persistenceNamespace.PersistenceIdPrefix
    printfn "Projection    backend=pcsl-actor-proxy clearBeforeStart=%b seededDefaultPage=%b" clearPcslBeforeStart seededDefaultPage
    printfn "Actor address %s" actorAddress
    printfn "PingPong actor %s" pingPongActorAddress
    printfn "Template key  %s" templateKey
    printfn "Echo target key     %s" (JsonSerializer.Serialize(echoTargetKeys |> List.toArray))
    printfn "PingPong target key %s" (JsonSerializer.Serialize(pingPongTargetKeys |> List.toArray))
    printfn "Default arg   %s" defaultCanonicalArgString
    printfn "Echo helpers  ensureEchoActorRegistered(); stopEchoActor(); recreateEchoActor()"
    printfn "Stop pingpong with stopPingPongActor()"
    printfn "Stop with     stopPocFullNugetJournalAclHosts()"

    if noWait then
        stopPocFullNugetJournalAclHosts ()
    elif block then
        printfn "Blocking on ActorSystem termination."
        fabric.System.WhenTerminated.Wait() |> ignore
with ex ->
    stopPocFullNugetJournalAclHosts ()
    reraise ()
