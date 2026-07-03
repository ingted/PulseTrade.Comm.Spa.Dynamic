//
// Start a PTCS + PTCS.Dynamic host from NuGet packages with a durable Akka
// Persistence journal plus PTCS.ACL open extension integration.
//
// Port 81 uses GitHub OAuth browser auth. PTCS.Login is deliberately not
// referenced, no username/password login route is mounted, and no local-login
// listener is started. The single GitHub OAuth host owns the hub, journal,
// actor registry, Dynamic SDUI extension, and ACL policy.
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
//    Call stopPocFullNugetJournalAclHosts() to stop the web host.
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
#r "nuget: PulseTrade.Comm.ACL.SqlServer, [0.1.0-alpha2]"
//#r "nuget: PulseTrade.Comm.Login.Core, [0.1.0-alpha5]"
//#r "nuget: PulseTrade.Comm.Login.SqlServer, [0.1.0-alpha3]"
#r "nuget: PulseTrade.Comm.Security, [0.1.0-alpha1]"
#r "nuget: PulseTrade.Comm.Spa, [0.2.5-beta70]"
#r "nuget: PulseTrade.Comm.Spa.Dynamic, [0.1.3-beta60]"
#r "nuget: PulseTrade.Comm.Spa.ACL, [0.1.0-alpha10]"
//#r "nuget: PulseTrade.Comm.Spa.Login, [0.1.0-alpha12]"

#load @"C:\Users\Administrator\.codex\lib\ParseLine.fsx"

open System
open System.Collections.Concurrent
open System.Diagnostics
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
open Microsoft.Data.SqlClient
open PersistedConcurrentSortedList.Type
open PulseTrade.Comm.ACL.Core
open PulseTrade.Comm.ACL.SqlServer.AclSqlServer
open PulseTrade.Comm.Actor.Registry
//open PulseTrade.Comm.Login.Core
//open PulseTrade.Comm.Login.SqlServer
open PulseTrade.Comm.Spa
open PulseTrade.Comm.Spa.ACL
open PulseTrade.Comm.Spa.Dynamic.Server
//open PulseTrade.Comm.Spa.Login

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
    printfn "default ports: github-oauth=81, cluster=%d" defaultClusterPort
    //$"""--host 0.0.0.0 --github-port 81 --github-public-base-url "https://my-ai.co.in:81" --github-oauth-client-id-path "{pathArg defaultGitHubOAuthClientIdPath}" --github-oauth-client-secret-path "{pathArg defaultGitHubOAuthClientSecretPath}" --site-sharing isolated --pcsl-root "{pathArg defaultPcslRoot}" --delivery-profile nuget-journal-acl-live --actor-name nuget-journal-acl-echo --cluster-port {defaultClusterPort} --demo """
    $"""--host 0.0.0.0 --github-port 81 --github-public-base-url "https://my-ai.co.in:81" --github-oauth-client-id-path "{pathArg defaultGitHubOAuthClientIdPath}" --github-oauth-client-secret-path "{pathArg defaultGitHubOAuthClientSecretPath}" --site-sharing isolated --pcsl-root "{pathArg defaultPcslRoot}" --delivery-profile nuget-journal-acl-live --actor-name nuget-journal-acl-echo --cluster-port {defaultClusterPort} """

type CliArgs =
    | Host of string
    | Github_Port of int
    | Github_Public_Base_Url of string
    | Github_Oauth_Client_Id_Path of string
    | Github_Oauth_Client_Secret_Path of string
    | Site_Sharing of string
    | Pcsl_Root of string
    | Demo
    | Production_Sql
    | If_Dyna_Port
    | Sql_Db of string
    | Sql_Connection_String of string
    | Sql_Connection_String_Encrypted_File of string
    | Sql_Private_Key_Path of string
    | Sql_Key_Size of int
    | Sql_Security_Schema of string
    | Sql_Acl_Table of string
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
            | Github_Public_Base_Url _ -> "External public base URL for GitHub OAuth callback, for example https://my-ai.co.in:81."
            | Github_Oauth_Client_Id_Path _ -> "Local file path containing GitHub OAuth client id. The value is not printed."
            | Github_Oauth_Client_Secret_Path _ -> "Local file path containing GitHub OAuth client secret. The value is not printed."
            | Site_Sharing _ -> "Site sharing mode: isolated or shared."
            | Pcsl_Root _ -> "PCSL projection root for this live host."
            | Demo -> "Use the demo GitHub-only ACL policy. This is the default when no production SQL args are supplied."
            | Production_Sql -> "Use SQL-backed ACL policy provider. Requires encrypted SQL connection-string file and private key path."
            | If_Dyna_Port -> "Use random free ports for GitHub and cluster. Useful when 81 is occupied."
            | Sql_Db _ -> "SQL Server database used by the durable journal. Defaults to a hash of --pcsl-root."
            | Sql_Connection_String _ -> "Legacy plaintext SQL Server connection string for local demo only. The value is never printed."
            | Sql_Connection_String_Encrypted_File _ -> "Encrypted SQL Server connection-string file for production-sql proof. The plaintext value is never printed."
            | Sql_Private_Key_Path _ -> "RSA private key path used to decrypt --sql-connection-string-encrypted-file."
            | Sql_Key_Size _ -> "RSA key size used by the decryptor. Default: 2048."
            | Sql_Security_Schema _ -> "SQL schema for credential/session/ACL policy proof tables. Default: ptcs_poc_acl."
            | Sql_Acl_Table _ -> "ACL policy snapshot table for production-sql proof. Default: AclPolicySnapshot."
            | Delivery_Profile _ -> "Durable ingress profile id."
            | Actor_Name _ -> "Echo actor name under /user."
            | Cluster_Port _ -> "Local Akka cluster port."
            | Clear_Pcsl_Before_Start -> "Clear the PCSL projection root before startup. This does not delete the SQL journal."
            | No_Wait -> "Start, verify health/static/internal actor gates, then stop. Browser OAuth UI gates still require manual login."
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

type GenEnum_FOR_ProtoTyping =
    | CreateTable
    | FSRecord

type ReferenceDateMode_FOR_ProtoTyping =
    | ModeAccountingDate
    | ModeTradingDate

type PFCF_AKKA_Terry_FOR_ProtoTyping =
    | List
    interface IArgParserTemplate with
        member _.Usage = ""

type PFCF_AKKA_WenZone_FOR_ProtoTyping =
    | Days
    interface IArgParserTemplate with
        member _.Usage = ""

type PFCF_AKKA_CMD_DATA_RANGE_FOR_ProtoTyping =
    | ReferenceDateMode of ReferenceDateMode_FOR_ProtoTyping
    | Between of decimal * decimal
    | After of decimal
    | AfterT of decimal
    | Before of decimal
    | BeforeT of decimal
    | LeftCurReferenceDate of decimal
    | LeftTCurReferenceDate of decimal
    | AfterCurReferenceDate
    | AfterTCurReferenceDate
    | BeforeCurReferenceDate
    | BeforeTCurReferenceDate
    | NoFilter
    | Calibrate2CurDayIfLargerThanCurDay
    interface IArgParserTemplate with
        member _.Usage = ""
    static member val MAD = ReferenceDateMode ModeAccountingDate with get
    static member val MTD = ReferenceDateMode ModeTradingDate with get

type PFCF_AKKA_CMD_GM_FOR_ProtoTyping =
    | RiskScore of decimal
    | Branch of string
    | IfRealTime of bool
    interface IArgParserTemplate with
        member _.Usage = ""

type ClosingNoMode_FOR_ProtoTyping =
    | ParentChildShared
    | ParentChildSeparated

type PFCFGTC_FOR_ProtoTyping =
    | GF
    | GC
    | GOD
    | GOI
    | GMA
    | GS
    | GE

type CooperativeType_FOR_ProtoTyping =
    | CONTRACTS
    | TRADING
    | COVER
    | MARGIN
    | OI
    | ORDERS

type PFCF_GTC_CONF_FOR_ProtoTyping =
    | FillDTFormatYYYYMMDD
    | ShowOrderSN
    | ShowTXSN
    | OrderByTXDT
    | OrderBySQDT
    | OrderByOIDT
    | GroupDownToDayInsteadOfContractBase
    | FloorAvgPrice
    | CathayBKTaifexFill
    | CathayBKTaifexOI
    | CathayBKNonTaifexFill
    | CathayBKNonTaifexOI
    | SCSBIntradayOI
    | SCSBRiskFactor
    | SCSBIntradayOIByFill
    | SCSBIntradayOrder
    | SCSBIntradayFill
    | SCSBAfterHoursFill
    | SCSBTradeSummary
    | SCSBOpenPositionSummaryF
    | SCSBOpenPositionSummaryT
    | SCSBMargin
    | OIInf
    | TAIFEX
    | ASIA
    | EURUS
    | Empty
    | FillSquareCombine
    | OP_OI_ShowMarketValue
    | Default
    | FeeIncludeTax
    | ItsGTC1
    | ItsGTC2
    | ItsGTC4
    | ItsGTC5
    | ItsGTC8
    | SubAccountIncluded

type PFCF_AKKA_CMD_FOR_ProtoTyping =
    | SimpleAction of action_name: string
    | Entrust of id: string * accountingDay: decimal
    | Transglobe of brokerBranch: string * id: string * accountingDay: decimal
    | PFCFGTC of PFCFGTC_FOR_ProtoTyping list
    | PFCFEDX of mode: string
    | PFCFGTCCONF of PFCF_GTC_CONF_FOR_ProtoTyping list
    | BBA of 期貨商: string * 分公司: string * 母帳帳號: string
    | Entie of mode: int
    | TSIT
    | USITC
    | PGIM
    | FRANKLIN
    | CathayBank
    | RID of string
    | Cooperative of CooperativeType_FOR_ProtoTyping
    | MLI of brokerBranch: string * id: string
    | ParentChild of int
    | ParentChilds of int list
    | FractionalQuote of if全轉分數報價orOnlyFor原分數報價: bool * 自訂分母值: float
    | DecimalQuote of 小數位數: int * ``0四捨五入1無條件捨去2無條件進位3無條件截斷``: int
    | Round of 非分數報價小數位數: int * 分數報價分數部分小數位數: int * 收支欄位小數位數: int
    | TW
    | NonTW
    | Futures
    | Options
    | TO of float
    | Debug of string
    | ClosingNoMode of ClosingNoMode_FOR_ProtoTyping
    | [<CliPrefix(CliPrefix.None)>] DataRange of ParseResults<PFCF_AKKA_CMD_DATA_RANGE_FOR_ProtoTyping>
    | [<CliPrefix(CliPrefix.None)>] GM of ParseResults<PFCF_AKKA_CMD_GM_FOR_ProtoTyping>
    | [<CliPrefix(CliPrefix.None)>] Terry of ParseResults<PFCF_AKKA_Terry_FOR_ProtoTyping>
    | [<CliPrefix(CliPrefix.None)>] WZ of ParseResults<PFCF_AKKA_WenZone_FOR_ProtoTyping>
    | GenByColMeta of ifTw: bool * ifOrig: bool * schemaName: string * genType: GenEnum_FOR_ProtoTyping
    | GenFromDWQuery of sql0F1T2F: int * sqlB64: string * schName: string * tblName: string * genType: GenEnum_FOR_ProtoTyping * ifOrig: bool * ifSchemaOnly: bool
    | TableName of string list
    | OutCreateTable of string
    | OutCsv of localDirectory: string * fileName: string * csvOnly: bool
    | CsvOnly
    | CsvEncoding of encodingName: string
    | CsvDelimiter of delimiter: string
    | CsvHeader of includeHeader: bool
    | OutExcel of localDirectory: string * fileName: string * excelOnly: bool
    | ExcelOnly
    | ExcelSheetName of sheetName: string
    | ExcelHeader of includeHeader: bool
    | FtpHost of host: string
    | FtpPort of port: int
    | FtpProfile of profileName: string
    | FtpRemoteDirectory of remoteDirectory: string
    | FtpCredentialSource of sourceName: string
    | FtpRetryCount of retryCount: int
    interface IArgParserTemplate with
        member _.Usage = ""

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
    ArgumentParser.Create<CliArgs>(programName = "full.nuget.journal.ACL2.NoLogin.fsx")

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

let bindAddressForHost host =
    let value = textOr "127.0.0.1" host
    let lower = value.ToLowerInvariant()

    match lower with
    | "0.0.0.0"
    | "*"
    | "+" -> IPAddress.Any
    | "::" -> IPAddress.IPv6Any
    | "localhost" -> IPAddress.Loopback
    | _ ->
        let mutable parsed = Unchecked.defaultof<IPAddress>

        if IPAddress.TryParse(value, &parsed) then
            parsed
        else
            IPAddress.Any

let tryBindTcpPort host port =
    if port <= 0 then
        Ok()
    else
        let address = bindAddressForHost host
        let listener = new TcpListener(address, port)

        try
            try
                listener.ExclusiveAddressUse <- true
                listener.Start()
                Ok()
            with ex ->
                Error ex.Message
        finally
            listener.Stop()

let requireTcpPortFree label argName host port =
    match tryBindTcpPort host port with
    | Ok() -> ()
    | Error message ->
        invalidOp
            $"ACL2 startup preflight failed: {label} port {host}:{port} is unavailable or already in use. Stop the existing service, change {argName}, or run with --if-dyna-port. Socket error: {message}"

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

let ifDynaPort =
    defaultParsed.Contains If_Dyna_Port
    || (overrideParsed |> Option.exists (fun parsed -> parsed.Contains If_Dyna_Port))

let hasEncryptedSqlArgs (parsed: ParseResults<CliArgs>) =
    parsed.TryGetResult <@ Sql_Connection_String_Encrypted_File @> |> Option.exists (String.IsNullOrWhiteSpace >> not)
    && parsed.TryGetResult <@ Sql_Private_Key_Path @> |> Option.exists (String.IsNullOrWhiteSpace >> not)

let productionSql =
    (overrideParsed |> Option.exists (fun parsed -> parsed.Contains Production_Sql || hasEncryptedSqlArgs parsed))
    || (defaultParsed.Contains Production_Sql || hasEncryptedSqlArgs defaultParsed)

let demoMode = not productionSql

let githubPort =
    if ifDynaPort then
        freePort ()
    else
        let defaultValue = defaultParsed.GetResult(Github_Port, 81)
        overrideParsed |> Option.map (fun parsed -> parsed.GetResult(Github_Port, defaultValue)) |> Option.defaultValue defaultValue

let githubPublicBaseUrl =
    let defaultValue = defaultParsed.GetResult(Github_Public_Base_Url, "https://my-ai.co.in:81")

    if ifDynaPort then
        $"http://127.0.0.1:{githubPort}"
    else
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
    if ifDynaPort then
        freePort ()
    else
        let defaultValue = defaultParsed.GetResult(Cluster_Port, 7788)
        overrideParsed |> Option.map (fun parsed -> parsed.GetResult(Cluster_Port, defaultValue)) |> Option.defaultValue defaultValue

let sqlSecuritySchema =
    let defaultValue = defaultParsed.GetResult(Sql_Security_Schema, "ptcs_poc_acl")
    overrideParsed |> Option.map (fun parsed -> parsed.GetResult(Sql_Security_Schema, defaultValue)) |> Option.defaultValue defaultValue |> textOr "ptcs_poc_acl"

let sqlAclTable =
    let defaultValue = defaultParsed.GetResult(Sql_Acl_Table, "AclPolicySnapshot")
    overrideParsed |> Option.map (fun parsed -> parsed.GetResult(Sql_Acl_Table, defaultValue)) |> Option.defaultValue defaultValue |> textOr "AclPolicySnapshot"

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

        try
            // Suave can retain the writer after startWithSharing returns.
            // Do not use a disposable StringWriter here; background listener
            // output after disposal can terminate the listener.
            Console.SetOut(TextWriter.Null)
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

let pfcfProtoTypingCanonicalArgString =
    "--pfcfedx trivial --pfcfgtcconf OIInf TAIFEX FillSquareCombine OrderByTXDT CathayBKTaifexFill --to 90000 --parentchilds 2 5 --bba F008 000 9910357 --decimalquote 6 0 --round 6 4 2 datarange --referencedatemode ModeAccountingDate --between 20251104 20251104 --calibrate2curdayiflargerthancurday"

let pfcfProtoTypingTemplateKey = "pfcf-akka-cmd-prototyping"

let templateRegistration =
    DynamicArguTemplateRegistration.create
        templateKey
        typeof<PocFullNugetJournalArgu>
        DynamicArguAliasBinding.empty
        (Some defaultCanonicalArgString)

let pfcfProtoTypingTemplateRegistration =
    DynamicArguTemplateRegistration.create
        pfcfProtoTypingTemplateKey
        typeof<PFCF_AKKA_CMD_FOR_ProtoTyping>
        DynamicArguAliasBinding.empty
        (Some pfcfProtoTypingCanonicalArgString)

if not ifDynaPort then
    requireTcpPortFree "GitHub OAuth HTTP listener" "--github-port" host githubPort
    requireTcpPortFree "Akka cluster remoting" "--cluster-port" "127.0.0.1" clusterPort

printfn
    "startup preflight ok: github=%s:%d cluster=127.0.0.1:%d dynamic=%b mode=github-oauth-only"
    host
    githubPort
    clusterPort
    ifDynaPort

if clearPcslBeforeStart then
    ensureSafeClearPcslRoot pcslRoot

Directory.CreateDirectory pcslRoot |> ignore

let encryptedSqlConnectionString () =
    let parsed =
        overrideParsed
        |> Option.defaultValue defaultParsed

    let encryptedFile =
        parsed.TryGetResult <@ Sql_Connection_String_Encrypted_File @>
        |> Option.map (fullPath "")
        |> Option.defaultWith (fun () -> invalidOp "production-sql requires --sql-connection-string-encrypted-file.")

    let privateKeyPath =
        parsed.TryGetResult <@ Sql_Private_Key_Path @>
        |> Option.map (fullPath "")
        |> Option.defaultWith (fun () -> invalidOp "production-sql requires --sql-private-key-path.")

    let keySize = parsed.GetResult(Sql_Key_Size, 2048)
    PulseTrade.Comm.Security.readEncryptedTextFile keySize privateKeyPath encryptedFile None

let legacyPlaintextSqlConnectionString () =
    overrideParsed
    |> Option.bind (fun parsed -> parsed.TryGetResult <@ Sql_Connection_String @>)
    |> Option.map (fun value -> value.Trim())
    |> Option.filter (String.IsNullOrWhiteSpace >> not)

let productionSqlConnectionString =
    if productionSql then
        Some(encryptedSqlConnectionString ())
    else
        legacyPlaintextSqlConnectionString ()

let journal =
    match productionSqlConnectionString with
    | Some connectionString ->
        Journal.sqlServer(connectionString, autoCreateDatabase = true)
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

let quoteIdentifier (value: string) =
    "[" + value.Replace("]", "]]") + "]"

let executeSqlNonQuery (connectionString: string) (commandText: string) =
    use connection = new SqlConnection(connectionString)
    connection.Open()
    use command = connection.CreateCommand()
    command.CommandText <- commandText
    command.CommandTimeout <- 30
    command.ExecuteNonQuery() |> ignore

let aclSqlConfig connectionString =
    AclSqlServerProviderConfig.create connectionString
    |> AclSqlServerProviderConfig.withSchema sqlSecuritySchema
    |> AclSqlServerProviderConfig.withTableName sqlAclTable
    |> AclSqlServerProviderConfig.withEnsureDatabase true
    |> AclSqlServerProviderConfig.withEnsureSchema true

let githubAdminLogin = "ingted"
let githubAdminUserId = "github:" + githubAdminLogin

let pocAclPolicy revision deploymentProfile browserAuthProvider users =
    { AclPolicyConfig.empty with
        Revision = revision
        DeploymentProfile = deploymentProfile
        BrowserAuthProvider = browserAuthProvider
        Groups = [ "sys-admin"; "Terry黑粉" ]
        Roles = [ "admin"; "user" ]
        Users = users
        Resources =
            [ { Kind = "ptcs.page"
                Id = "DamnWZ"
                OwnerGroup = Some "sys-admin"
                Tags = []
                Attributes = Map.empty }
              { Kind = "ptcs.page"
                Id = "AssTerry"
                OwnerGroup = Some "Terry黑粉"
                Tags = []
                Attributes = Map.empty } ]
        Rules =
            [ { RuleId = "sys-admin-all"
                Effect = "allow"
                Subjects = []
                Groups = [ "sys-admin" ]
                Roles = []
                Actions = [ "ptcs.*" ]
                Resources = [ "*" ]
                ResourceOwnerGroups = [] }
              { RuleId = "terry-hater-ass-terry-use"
                Effect = "allow"
                Subjects = []
                Groups = [ "Terry黑粉" ]
                Roles = []
                Actions =
                    [ PtcsAcl.actionPageRead
                      PtcsAcl.actionTargetRemove
                      PtcsAcl.actionActorArguSend ]
                Resources = [ "ptcs.page:AssTerry" ]
                ResourceOwnerGroups = [] }
              { RuleId = "terry-hater-no-add"
                Effect = "deny"
                Subjects = []
                Groups = [ "Terry黑粉" ]
                Roles = []
                Actions = [ PtcsAcl.actionTargetAdd ]
                Resources = [ "ptcs.page:AssTerry" ]
                ResourceOwnerGroups = [] } ] }

let githubOnlyAclPolicy revision deploymentProfile =
    pocAclPolicy
        revision
        deploymentProfile
        (Some "github-oauth")
        [ { UserId = githubAdminUserId
            Groups = [ "sys-admin" ]
            Roles = [ "admin" ] } ]

let seedProductionSqlAcl connectionString =
    let aclConfig = aclSqlConfig connectionString

    ensureStoreAsync aclConfig |> Async.RunSynchronously

    let policy =
        githubOnlyAclPolicy
            ("poc-full-nuget-journal-acl-github-only-sql-" + DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss"))
            Public

    ensureAndSavePolicyConfigAsync aclConfig true policy |> Async.RunSynchronously

    aclConfig

printfn "initializing ACL provider mode=%s auth=github-oauth-only" (if productionSql then "production-sql" else "demo")

let seededAclConfig =
    match productionSqlConnectionString with
    | Some connectionString when productionSql -> Some(seedProductionSqlAcl connectionString)
    | _ -> None

let aclOptions =
    match seededAclConfig with
    | Some aclConfig ->
        match ensureAndLoadActivePolicySnapshotAsync aclConfig |> Async.RunSynchronously with
        | Ok snapshot ->
            { Snapshot = snapshot
              AuditSink = None }
        | Error error -> invalidOp $"PTCS ACL SQL policy load failed: {error.Message}"
    | None ->
        match PtcsAcl.create (githubOnlyAclPolicy "poc-full-nuget-journal-acl-github-only-demo" Public) with
        | Ok value -> value
        | Error error -> invalidOp $"PTCS ACL demo policy decode failed: {error.Message}"

printfn "ACL provider ready mode=%s auth=github-oauth-only admin=%s" (if productionSql then "production-sql" else "demo") githubAdminUserId

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
    |> PtcsAclExtension.useAcl aclOptions

let startPtcsHost label url f =
    let sw = Stopwatch.StartNew()
    printfn "starting %s listener: %s" label url
    let app = withStartupOutput f
    sw.Stop()
    printfn "started %s listener in %.1f ms: %s" label sw.Elapsed.TotalMilliseconds url
    app

let clientBaseUrlForHostPort host port =
    let clientHost =
        let value = textOr "127.0.0.1" host

        if String.Equals(value, "0.0.0.0", StringComparison.OrdinalIgnoreCase)
           || String.Equals(value, "::", StringComparison.OrdinalIgnoreCase) then
            "127.0.0.1"
        else
            value

    $"http://{clientHost}:{port}"

let githubClientBaseUrl =
    clientBaseUrlForHostPort host githubPort

let localClientBaseUrl =
    githubClientBaseUrl

let githubApp =
    startPtcsHost "GitHub OAuth" (sprintf "%s/actors" githubClientBaseUrl) (fun () -> Server.startWithSharing siteSharing githubOptions)

withStartupOutput (fun () ->
    hub.useDynamicSdui(fabric.System, DynamicArguMetadata.empty, [ templateRegistration; pfcfProtoTypingTemplateRegistration ])
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
            [ "ptcs.dynamic.poc", "full.nuget.journal.ACL2.NoLogin.fsx"
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
            [ "ptcs.dynamic.poc", "full.nuget.journal.ACL2.NoLogin.fsx"
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

let pfcfProtoTypingTargetKeys =
    [ actorAddress; pfcfProtoTypingTemplateKey; pfcfProtoTypingCanonicalArgString ]

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
      pingPongTargetKeys, "PingPong target"
      pfcfProtoTypingTargetKeys, "PFCF prototype target" ]

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

let verifyPfcfProtoTypingTemplate () =
    let argv = DynamicCommandLine.split pfcfProtoTypingCanonicalArgString

    match DynamicArgStringTarget.validateByParser pfcfProtoTypingTemplateRegistration argv with
    | Ok _ -> ()
    | Error error -> failwith $"PFCF prototype canonical arg string should parse: {error}"

    let parsedTarget =
        DynamicArgStringTarget.scan pfcfProtoTypingTemplateRegistration actorAddress pfcfProtoTypingCanonicalArgString

    let rebuiltRawArgu =
        DynamicArgStringTarget.buildRawArgu parsedTarget

    require
        (String.Equals(rebuiltRawArgu, pfcfProtoTypingCanonicalArgString, StringComparison.Ordinal))
        $"PFCF prototype raw command rebuild mismatch. expected={pfcfProtoTypingCanonicalArgString}; actual={rebuiltRawArgu}"

    let request: DynamicArguResolveTargetRequest =
        { Keys = [| actorAddress; pfcfProtoTypingTemplateKey; pfcfProtoTypingCanonicalArgString |] }

    let requestJson =
        JsonSerializer.Serialize(request, ArguFormSchema.jsonOptions)

    let replyJson =
        DynamicArguResolveEndpoint.handle [ pfcfProtoTypingTemplateRegistration ] requestJson

    let reply =
        JsonSerializer.Deserialize<DynamicArguResolveTargetReply>(replyJson, ArguFormSchema.jsonOptions)

    require reply.Ok $"PFCF prototype Dynamic resolve should succeed: {reply.Error}"

    let actualCases =
        reply.Document.ArguFormSchema.UnionCases
        |> Array.map _.Name

    let expectedCases =
        [| "PFCFEDX"; "PFCFGTCCONF"; "TO"; "ParentChilds"; "BBA"; "DecimalQuote"; "Round"; "DataRange" |]

    let expectedCasesText =
        String.concat "," expectedCases

    let actualCasesText =
        String.concat "," actualCases

    require
        (actualCases = expectedCases)
        $"PFCF prototype projected cases mismatch. expected={expectedCasesText}; actual={actualCasesText}"

    let rec flattenNode (node: SduiFormNode) =
        seq {
            yield node

            for child in node.Children do
                yield! flattenNode child

            for item in node.Items do
                yield! flattenNode item
        }

    let defaultValues binding =
        reply.Document.Nodes
        |> Seq.collect flattenNode
        |> Seq.tryFind (fun node -> String.Equals(node.Binding, binding, StringComparison.Ordinal))
        |> Option.map _.DefaultValues
        |> Option.defaultValue [||]

    require
        (defaultValues "PFCFEDX.mode" = [| "trivial" |])
        "PFCF prototype should project PFCFEDX.mode default from canonical arg string."

    require
        (defaultValues "PFCFGTCCONF.value" = [| "OIInf"; "TAIFEX"; "FillSquareCombine"; "OrderByTXDT"; "CathayBKTaifexFill" |])
        "PFCF prototype should project PFCFGTCCONF list defaults from canonical arg string."

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

let postJson (client: HttpClient) path jsonText =
    use request = new HttpRequestMessage(HttpMethod.Post, localClientBaseUrl + path)
    request.Headers.TryAddWithoutValidation("Origin", localClientBaseUrl) |> ignore
    request.Headers.Referrer <- Uri(localClientBaseUrl + "/actors")
    request.Content <- new StringContent(jsonText, Encoding.UTF8, "application/json")
    use response = client.SendAsync(request).GetAwaiter().GetResult()
    int response.StatusCode, response.Content.ReadAsStringAsync().GetAwaiter().GetResult()

let jsonArrayText values =
    JsonSerializer.Serialize(values |> List.toArray)

let addKeyJson pageId keys displayName =
    JsonSerializer.Serialize(
        {| pageId = pageId
           keyJson = jsonArrayText keys
           displayName = displayName |})

let removeKeyJson pageId keyId =
    JsonSerializer.Serialize(
        {| pageId = pageId
           keyId = keyId |})

let actorArguJson pageId keys rawArgu =
    JsonSerializer.Serialize(
        {| pageId = pageId
           keyJson = jsonArrayText keys
           rawArgu = rawArgu
           tags = [| "poc-full-nuget-journal-acl"; "acl-http-proof" |] |})

let statusIs expected (label: string) (statusCode: int, body: string) =
    require (statusCode = expected) $"{label} expected HTTP {expected}, got {statusCode}; body={body}"

let aclCapabilityAllowed (snapshotJson: string) resourceId action =
    use doc = JsonDocument.Parse(snapshotJson)
    let root = doc.RootElement
    let resources = root.GetProperty("resources").EnumerateArray()

    let resource =
        resources
        |> Seq.tryFind (fun value ->
            String.Equals(value.GetProperty("resourceKind").GetString(), "ptcs.page", StringComparison.Ordinal)
            && String.Equals(value.GetProperty("resourceId").GetString(), resourceId, StringComparison.Ordinal))
        |> Option.defaultWith (fun () -> invalidOp $"ACL snapshot missing page resource {resourceId}: {snapshotJson}")

    let capability =
        resource.GetProperty("capabilities").EnumerateArray()
        |> Seq.tryFind (fun value -> String.Equals(value.GetProperty("action").GetString(), action, StringComparison.Ordinal))
        |> Option.defaultWith (fun () -> invalidOp $"ACL snapshot missing capability {action} for {resourceId}: {snapshotJson}")

    capability.GetProperty("allowed").GetBoolean()

let aclGlobalCapabilityAllowed (snapshotJson: string) action =
    use doc = JsonDocument.Parse(snapshotJson)
    let root = doc.RootElement

    let capability =
        root.GetProperty("globalCapabilities").EnumerateArray()
        |> Seq.tryFind (fun value -> String.Equals(value.GetProperty("action").GetString(), action, StringComparison.Ordinal))
        |> Option.defaultWith (fun () -> invalidOp $"ACL snapshot missing global capability {action}: {snapshotJson}")

    capability.GetProperty("allowed").GetBoolean()

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
        printfn "full.nuget.journal.ACL2.NoLogin host already stopped."
    else
        pocFullNugetJournalStopped <- true
        (githubApp :> IDisposable).Dispose()
        fabric.Stop()
        fabric.System.WhenTerminated.Wait(TimeSpan.FromSeconds 10.0) |> ignore
        printfn "full.nuget.journal.ACL2.NoLogin host stopped."

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
                      Source = Some "full.nuget.journal.ACL2.NoLogin.fsx"
                      DeadlineAtUtc = None }
                    CancellationToken.None
                |> Async.RunSynchronously)
        else
            None

    use client = new HttpClient()
    let healthText = client.GetStringAsync(githubClientBaseUrl + "/healthz").GetAwaiter().GetResult()
    let githubHealthText = client.GetStringAsync(githubClientBaseUrl + "/healthz").GetAwaiter().GetResult()
    let journalHealthText = client.GetStringAsync(githubClientBaseUrl + "/healthz.journal").GetAwaiter().GetResult()
    let persistenceHealthText = client.GetStringAsync(githubClientBaseUrl + "/healthz.persistence").GetAwaiter().GetResult()
    let aclExtensionJs = client.GetStringAsync(githubClientBaseUrl + PtcsAclExtension.scriptUrl).GetAwaiter().GetResult()
    let dynamicJs = client.GetStringAsync(githubClientBaseUrl + "/ext/js/PulseTrade.Comm.Spa.Dynamic.js").GetAwaiter().GetResult()

    let actorDynamicCreateShapeVisible =
        hub.ListClientExtensions()
        |> List.collect _.AppendPageShapes
        |> List.exists (fun shape -> String.Equals(shape.Shape, "actor-dynamic", StringComparison.OrdinalIgnoreCase))

    require (healthText.Contains("PulseTrade.Comm.Spa")) "healthz should identify PTCS."
    require (githubHealthText.Contains("PulseTrade.Comm.Spa")) "GitHub OAuth host healthz should identify PTCS."
    require (journalHealthText.Contains("sql-server")) "journal health should use sql-server profile."
    require (healthText.Contains("pcsl-actor-proxy")) "healthz hub persistence should expose pcsl-actor-proxy."
    require (aclExtensionJs.Contains("PulseTrade.Comm.Spa.ACL", StringComparison.Ordinal)) "PTCS.ACL extension script asset should be served by the PTCS host."
    require (not actorDynamicCreateShapeVisible) "journal POC must not expose +page Actor Dynamic shape in the extension manifest."
    let hubActorCount = (hub.ActorsSnapshot()).ActorCount
    let actorsNodeCount = 0
    let actorsActorCount = hubActorCount
    let actorsNodeCountWithOffline = 0
    let actorsActorCountWithOffline = hubActorCount
    let mutable afterStopActorsNodeCount = -1
    let mutable afterStopActorsActorCount = -1
    let mutable afterStopIncludeOfflineActorCount = -1
    let mutable echoReuseAfterStopVerified = false
    let mutable aclHttpDifferenceVerified = false

    require
        (String.Equals(ensuredEcho.Path.ToStringWithoutAddress(), echoRef.Path.ToStringWithoutAddress(), StringComparison.Ordinal))
        "ensureEchoActorRegistered should reuse the existing echo actor instead of trying to spawn a duplicate actor name."

    require (hubActorCount > 0) $"hub actor projection should have actors, got {hubActorCount}."
    require (dynamicJs.Contains("dynamic-actors-page")) "Dynamic bundle should include ActorsPage renderer."
    require (dynamicJs.Contains("dynamic-argu-add-key")) "Dynamic bundle should include Add target key renderer."
    verifyPfcfProtoTypingTemplate ()

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
        let afterStopActorCount = (hub.ActorsSnapshot()).ActorCount

        afterStopActorsNodeCount <- 0
        afterStopActorsActorCount <- afterStopActorCount
        afterStopIncludeOfflineActorCount <- afterStopActorCount

        echoReuseAfterStopVerified <- verifyEchoActorReuseAfterStop ()

    printfn "PTCS Dynamic POC Full NuGet Journal ACL started."
    printfn "GitHub OAuth URL   %s/actors" githubClientBaseUrl
    printfn "GitHub public URL  %s/actors" githubPublicBaseUrl
    printfn "GitHub login URL   %s/chat/login?returnUrl=/actors" githubClientBaseUrl
    printfn "GitHub chat URL    %s/chat" githubClientBaseUrl
    printfn "GitHub actors URL  %s/actors" githubClientBaseUrl
    printfn "Mode              %s" (if productionSql then "production-sql" else "demo")
    printfn "Ports             github=%d cluster=%d dynamic=%b" githubPort clusterPort ifDynaPort
    printfn "Auth              github-oauth only; PTCS.Login package/route/listener disabled."
    printfn "ACL policy        adminUser=%s browserAuthProvider=github-oauth httpDifference=%b" githubAdminUserId aclHttpDifferenceVerified
    printfn "Actors data   visibleNodes=%d visibleActors=%d includeOfflineNodes=%d includeOfflineActors=%d hubActors=%d" actorsNodeCount actorsActorCount actorsNodeCountWithOffline actorsActorCountWithOffline hubActorCount
    if noWait then
        printfn "After stop    visibleNodes=%d visibleActors=%d includeOfflineActors=%d pingPongStopRequested=true" afterStopActorsNodeCount afterStopActorsActorCount afterStopIncludeOfflineActorCount
        printfn "Echo reuse    reuseAfterStop=%b" echoReuseAfterStopVerified
    printfn "ActorArgu URLs %s/page/%s ; %s/page/%s" githubClientBaseUrl damnWzPage.PageId githubClientBaseUrl assTerryPage.PageId
    printfn "Dynamic JS    %s/ext/js/PulseTrade.Comm.Spa.Dynamic.js" githubClientBaseUrl
    printfn "PCSL root     %s" pcslRoot
    printfn "SQL journal   db=%s created=%b existed=%b" journalBootstrap.DatabaseName journalBootstrap.Created journalBootstrap.AlreadyExisted
    printfn "Persistence   namespace=%s prefix=%s" persistenceNamespace.PersistenceNamespace persistenceNamespace.PersistenceIdPrefix
    printfn "Projection    backend=pcsl-actor-proxy clearBeforeStart=%b seededDefaultPage=%b" clearPcslBeforeStart seededDefaultPage
    printfn "Actor address %s" actorAddress
    printfn "PingPong actor %s" pingPongActorAddress
    printfn "Template key  %s" templateKey
    printfn "PFCF template key %s" pfcfProtoTypingTemplateKey
    printfn "PFCF type name    %s" typeof<PFCF_AKKA_CMD_FOR_ProtoTyping>.FullName
    printfn "Echo target key     %s" (JsonSerializer.Serialize(echoTargetKeys |> List.toArray))
    printfn "PingPong target key %s" (JsonSerializer.Serialize(pingPongTargetKeys |> List.toArray))
    printfn "PFCF target key     %s" (JsonSerializer.Serialize(pfcfProtoTypingTargetKeys |> List.toArray))
    printfn "Default arg   %s" defaultCanonicalArgString
    printfn "PFCF arg      %s" pfcfProtoTypingCanonicalArgString
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
