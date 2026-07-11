//
// Start a PTCS + PTCS.Dynamic host from NuGet packages with a durable Akka
// Persistence journal plus PTCS.ACL/PTCS.Login open extension integration.
//
// No GitHub OAuth listener is mounted. Port 82 uses the PTCS.Login
// username/password provider and owns the hub, journal, actor registry,
// Dynamic SDUI extension, ACL policy, and PFCF prototype FormInput target.
//
// PCSL is treated as projection/cache: clearing the PCSL root should not delete
// the SQL journal, and startup warms the projection back from journaled stream
// actors.
//
// Default Visual Studio FSI use:
// 1. Ensure PulseTrade.Comm.Spa / PulseTrade.Comm.Spa.Dynamic nupkgs are built
//    and available in the #i package roots below.
// 2. Edit defaultArgumentsText only if you want fixed ports or PCSL root.
// 3. Select all and run. Call stopPingPongActor() to observe /actors reload.
//    Echo actor helpers:
//      ensureEchoActorRegistered()
//      stopEchoActor()
//      recreateEchoActor()
//    Cross-machine native actor calls require:
//      --cluster-host <this-machine-reachable-ip-or-dns>
//    The built-in PingPong proof starts a second Akka.Remote node with:
//      --native-node-host <this-machine-reachable-ip-or-dns>
//      --native-node-port <port-or-0>
//    Add Target Key stores [proxy; "target-v1"; native; template; raw].
//    PTCS sends to the proxy actor and includes native as
//    ActorArguTargetCommand.TargetActorAddress; no add-key hook rewrites the key.
//    Live-host mode does not send automatically unless --startup-probe is set.
//    After startup, call sendEchoProbe(), sendRawEchoProbe "...", or sendPfcfProbe().
//    Call stopPocFullNugetJournalAclHosts() to stop the web host.
//

//#i @"nuget: C:\Program Files\dotnet\sdk\10.0.301\FSharp\library-packs"
#r "nuget: FAkka.Argu, 10.1.301"
#r "nuget: FAkka.FCell2, 10.1.301.3"
#r "nuget: Akka, 1.5.69"
#r "nuget: Akka.Remote, 1.5.69"
#r "nuget: Akka.Cluster, 1.5.69"
#r "nuget: Akka.Cluster.Sharding, 1.5.69"
#r "nuget: Akka.Persistence, 1.5.69"
#r "nuget: Akka.Serialization.Hyperion, 1.5.69"
#r "nuget: Akka.Persistence.Sql, 1.5.67"
#r "nuget: Microsoft.Data.SqlClient, 7.0.1"
#r "nuget: Newtonsoft.Json, 13.0.4"
#r "nuget: System.Data.SqlClient, 4.9.1"
#r "nuget: PersistedConcurrentSortedList, 10.1.301.3"
#r "nuget: PulseTrade.Comm.Actor.Registry, [0.1.0-alpha5]"
#r "nuget: PulseTrade.Comm.ACL.Core, [0.1.0-alpha2]"
#r "nuget: PulseTrade.Comm.ACL.SqlServer, [0.1.0-alpha2]"
#r "nuget: PulseTrade.Comm.Login.Core, [0.1.0-alpha5]"
#r "nuget: PulseTrade.Comm.Login.SqlServer, [0.1.0-alpha3]"
#r "nuget: PulseTrade.Comm.Security, [0.1.0-alpha1]"
#r "nuget: PulseTrade.Comm.Spa, [0.2.5-beta75]"
#r "nuget: PulseTrade.Comm.Spa.Dynamic, [0.1.3-beta65]"
#r "nuget: PulseTrade.Comm.Spa.ACL, [0.1.0-alpha14]"
#r "nuget: PulseTrade.Comm.Spa.Login, [0.1.0-alpha16]"

#load @"C:\Users\Administrator\.codex\lib\ParseLine.fsx"

open System
open System.Collections.Concurrent
open System.Diagnostics
open System.IO
open System.Net
open System.Net.Http
open System.Net.NetworkInformation
open System.Net.Sockets
open System.Security.Cryptography
open System.Text
open System.Text.Json
open System.Text.RegularExpressions
open System.Threading
open Akka.Actor
open Akka.Configuration
open Argu
open Microsoft.Data.SqlClient
open Newtonsoft.Json.Linq
open PersistedConcurrentSortedList.Type
open PulseTrade.Comm.ACL.Core
open PulseTrade.Comm.ACL.SqlServer.AclSqlServer
open PulseTrade.Comm.Actor.Registry
open PulseTrade.Comm.Login.Core
open PulseTrade.Comm.Login.SqlServer
open PulseTrade.Comm.Spa
open PulseTrade.Comm.Spa.ACL
open PulseTrade.Comm.Spa.Dynamic.Server
open PulseTrade.Comm.Spa.Login

//your PCSL root path
let defaultArtifactRoot = @"E:\akka.net\PCSL"

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
    (*
    E:\akka.net\src\SQLConn.enc.txt
    redacted SQL connection string example; use --sql-connection-string-encrypted-file and --sql-private-key-path.
    *)
    printfn "default ports: local-login=82, cluster=%d" defaultClusterPort
    //$"""--host 0.0.0.0 --github-port 81 --local-port 82 --github-public-base-url "https://my-ai.co.in:81" --github-oauth-client-id-path "{pathArg defaultGitHubOAuthClientIdPath}" --github-oauth-client-secret-path "{pathArg defaultGitHubOAuthClientSecretPath}" --site-sharing isolated --pcsl-root "{pathArg defaultPcslRoot}" --delivery-profile nuget-journal-acl-live --actor-name nuget-journal-acl-echo --cluster-port {defaultClusterPort} --demo """
    $"""--sql-connection-string-encrypted-file "D:/ingted.com/ptcs-sql-connection.enc.txt" --sql-private-key-path "D:/ingted.com/myKey.private.txt" --host 10.28.112.109 --local-port 82 --site-sharing isolated --pcsl-root "{pathArg defaultPcslRoot}" --delivery-profile nuget-journal-acl-live --actor-name nuget-journal-acl-echo --cluster-host 10.28.112.109 --cluster-port {defaultClusterPort} --native-node-host 10.28.112.93 --native-node-port 0 --startup-probe --demo """


type CliArgs =
    | Host of string
    | Local_Port of int
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
    | Native_Actor_Address of string
    | Cluster_Host of string
    | Cluster_Port of int
    | Native_Node_Host of string
    | Native_Node_Port of int
    | Startup_Probe
    | Clear_Pcsl_Before_Start
    | No_Wait
    | Block
    | Verbose_Startup
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Host _ -> "HTTP bind host."
            | Local_Port _ -> "HTTP bind port for local username/password login. Use 0 for a random free port."
            | Site_Sharing _ -> "Site sharing mode: isolated or shared."
            | Pcsl_Root _ -> "PCSL projection root for this live host."
            | Demo -> "Use demo ACL/Login providers. This is the default when no production SQL args are supplied."
            | Production_Sql -> "Use SQL-backed credential/session/ACL policy providers. Requires encrypted SQL connection-string file and private key path."
            | If_Dyna_Port -> "Use random free ports for local login and cluster. Useful when 82 is occupied."
            | Sql_Db _ -> "SQL Server database used by the durable journal. Defaults to a hash of --pcsl-root."
            | Sql_Connection_String _ -> "Legacy plaintext SQL Server connection string for local demo only. The value is never printed."
            | Sql_Connection_String_Encrypted_File _ -> "Encrypted SQL Server connection-string file for production-sql proof. The plaintext value is never printed."
            | Sql_Private_Key_Path _ -> "RSA private key path used to decrypt --sql-connection-string-encrypted-file."
            | Sql_Key_Size _ -> "RSA key size used by the decryptor. Default: 2048."
            | Sql_Security_Schema _ -> "SQL schema for credential/session/ACL policy proof tables. Default: ptcs_poc_acl."
            | Sql_Acl_Table _ -> "ACL policy snapshot table for production-sql proof. Default: AclPolicySnapshot."
            | Delivery_Profile _ -> "Durable ingress profile id."
            | Actor_Name _ -> "Proxy actor name under /user."
            | Native_Actor_Address _ -> "Optional native actor address behind the local generic proxy. When absent, the script spawns a local string -> fCell2<string> demo actor."
            | Cluster_Host _ -> "Akka.Remote hostname advertised by this script. For cross-machine native actors, set this to this machine's reachable IP/DNS, not 127.0.0.1."
            | Cluster_Port _ -> "Local Akka cluster port."
            | Native_Node_Host _ -> "Akka.Remote hostname advertised by the built-in native PingPong node. Defaults to --cluster-host."
            | Native_Node_Port _ -> "Akka.Remote port for the built-in native PingPong node. Use 0 or omit for a random free port."
            | Startup_Probe -> "In live-host mode, send one startup ActorArgu probe to the proxy/native actor after the host starts. --no-wait always runs the verifier probe."
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

let debugConsoleLock = obj()

let debugPrint label message =
    lock debugConsoleLock (fun () ->
        printfn "[%s][%s][tid=%d] %s" (DateTimeOffset.Now.ToString("o")) label Thread.CurrentThread.ManagedThreadId message
        Console.Out.Flush())

type PocFullNugetJournalEchoActor() as this =
    inherit ReceiveActor()

    do
        this.Receive<ActorArguTargetCommand>(fun (command: ActorArguTargetCommand) ->
            debugPrint
                "echo-actor"
                $"self={this.ActorCtx.Self.Path}; sender={this.ActorCtx.Sender.Path}; commandId={command.CommandId}; raw={command.RawArgu}"

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

let pingPongReplyText text =
    let text = if isNull text then "" else text

    if String.Equals(text, "ping", StringComparison.OrdinalIgnoreCase) then
        "pong"
    else
        "pong:" + text

let pingPongReplyCell text =
    let raw = if isNull text then "" else text

    fCell2<string>.T (
        Map [
            "schema", fCell2<string>.S "ptcs.dynamic.poc.pingpong.reply.v1"
            "kind", fCell2<string>.S "pingpong"
            "raw", fCell2<string>.S raw
            "reply", fCell2<string>.S(pingPongReplyText raw)
            "marker", fCell2<string>.S "native-pingpong-fcell2-t"
        ])

type PocFullNugetJournalPingPongActor() as this =
    inherit ReceiveActor()

    do
        this.Receive<string>(fun text ->
            debugPrint "pingpong-string" $"self={this.ActorCtx.Self.Path}; sender={this.ActorCtx.Sender.Path}; text={text}"
            this.ActorCtx.Sender.Tell(pingPongReplyCell text, this.ActorCtx.Self))
        |> ignore

        this.Receive<PocFullNugetJournalPingPongMessage>(fun message ->
            match message with
            | Ping text -> this.ActorCtx.Sender.Tell(pingPongReplyCell text, this.ActorCtx.Self)
            | Stop -> this.ActorCtx.Stop(this.ActorCtx.Self))
        |> ignore

        this.Receive<ActorArguTargetCommand>(fun (command: ActorArguTargetCommand) ->
            debugPrint
                "pingpong-actor-argu"
                $"self={this.ActorCtx.Self.Path}; sender={this.ActorCtx.Sender.Path}; commandId={command.CommandId}; raw={command.RawArgu}"

            this.ActorCtx.Sender.Tell(pingPongReplyCell command.RawArgu, this.ActorCtx.Self))
        |> ignore

    member _.ActorCtx: IActorContext = ActorBase.Context

let nullstr = "<null>"
let emptystr = ""

type PocFullNugetJournalNativeStringActor() as this =
    inherit ReceiveActor()

    do
        this.Receive<string>(fun rawArgu ->
            debugPrint
                "native-string-actor"
                $"RECEIVED self={this.ActorCtx.Self.Path}; sender={this.ActorCtx.Sender.Path}; raw={if isNull rawArgu then nullstr else rawArgu}"

            let replyText =
                "native-string-actor fcell2 reply raw="
                + (if isNull rawArgu then emptystr else rawArgu)

            debugPrint "native-string-actor" $"REPLY self={this.ActorCtx.Self.Path}; reply={replyText}"
            this.ActorCtx.Sender.Tell(fCell2<string>.S replyText, this.ActorCtx.Self))
        |> ignore

    member _.ActorCtx: IActorContext = ActorBase.Context

type PocFullNugetJournalActorArguStringProxyControl =
    | GetNativeActorAddress

let rec jTokenToFCell2 (token: JToken) =
    match token with
    | null -> fCell2<string>.N()
    | :? JObject as json ->
        let caseToken = json.["Case"]
        let fieldsToken = json.["Fields"]

        if not (isNull caseToken) && not (isNull fieldsToken) then
            let caseName = caseToken.ToObject<string>()

            match caseName, fieldsToken with
            | "S", (:? JArray as fields) when fields.Count >= 1 ->
                fCell2<string>.S(fields.[0].ToObject<string>())
            | "T", (:? JArray as fields) when fields.Count >= 1 ->
                jTokenToFCell2 fields.[0]
            | "A", (:? JArray as fields) when fields.Count >= 1 ->
                match fields.[0] with
                | :? JArray as values ->
                    values |> Seq.cast<JToken> |> Seq.map jTokenToFCell2 |> Seq.toArray |> fCell2<string>.A
                | other -> fCell2<string>.A [| jTokenToFCell2 other |]
            | "N", _ -> fCell2<string>.N()
            | _ -> fCell2<string>.S(json.ToString(Newtonsoft.Json.Formatting.None))
        else
            json.Properties()
            |> Seq.filter (fun property -> not (property.Name.StartsWith("$", StringComparison.Ordinal)))
            |> Seq.map (fun property -> property.Name, jTokenToFCell2 property.Value)
            |> Map.ofSeq
            |> fCell2<string>.T
    | :? JArray as values ->
        values |> Seq.cast<JToken> |> Seq.map jTokenToFCell2 |> Seq.toArray |> fCell2<string>.A
    | :? JValue as value ->
        if isNull value.Value then
            fCell2<string>.N()
        else
            fCell2<string>.S(string value.Value)
    | other -> fCell2<string>.S(other.ToString(Newtonsoft.Json.Formatting.None))

let nativeReplyToCell (reply: obj) =
    match reply with
    | null -> Error "native actor returned null"
    | :? fCell2<string> as cell -> Ok cell
    | :? string as text -> Ok(fCell2<string>.S text)
    | :? JObject as json ->
        try
            Ok(jTokenToFCell2 json)
        with ex ->
            Error("native actor returned JObject but fCell2 conversion failed: " + ex.Message)
    | other ->
        Error($"native actor returned unsupported type {other.GetType().FullName}: {other}")

let cellStringField fieldName (fields: Map<string, fCell2<string>>) =
    fields
    |> Map.tryFind fieldName
    |> Option.map (function
        | fCell2.S text -> if isNull text then "" else text
        | cell -> cell.toJsonString())
    |> Option.defaultValue ""

let actorArguReplyCellHandler (cell: fCell2<string>) =
    match cell with
    | fCell2.T fields ->
        let schema = cellStringField "schema" fields

        if String.Equals(schema, "ptcs.dynamic.poc.pingpong.reply.v1", StringComparison.Ordinal) then
            let raw = cellStringField "raw" fields
            let reply = cellStringField "reply" fields
            let marker = cellStringField "marker" fields

            fCell2<string>.S(
                "poc.full.nuget.journal.acl pingpong fCell2.T->S raw="
                + raw
                + "; reply="
                + reply
                + "; marker="
                + marker)
        else
            fCell2<string>.S("actor-argu proxy converted fCell2.T reply=" + cell.toJsonString())
    | other -> other

let containsFCellTToSMarker (text: string) =
    let value = if isNull text then "" else text

    value.Contains("fCell2.T->S", StringComparison.Ordinal)
    || value.Contains("fCell2.T-\\u003ES", StringComparison.Ordinal)
    || value.Contains("fCell2.T-\\\\u003ES", StringComparison.Ordinal)

type PocFullNugetJournalActorArguStringProxyActor(
    actorSystem: ActorSystem,
    defaultNativeActorAddress: string,
    askTimeout: TimeSpan,
    replyCellHandler: fCell2<string> -> fCell2<string>) as this =
    inherit ReceiveActor()

    do
        this.Receive<PocFullNugetJournalActorArguStringProxyControl>(fun message ->
            match message with
            | GetNativeActorAddress -> this.ActorCtx.Sender.Tell(defaultNativeActorAddress, this.ActorCtx.Self))
        |> ignore

        this.Receive<ActorArguTargetCommand>(fun (command: ActorArguTargetCommand) ->
            let replyTo = this.ActorCtx.Sender
            let selfRef = this.ActorCtx.Self
            let rawArgu = if isNull command.RawArgu then "" else command.RawArgu
            let nativeActorAddress =
                command.TargetActorAddress
                |> Option.map (fun value -> if isNull value then "" else value.Trim())
                |> Option.filter (fun value -> not (String.IsNullOrWhiteSpace value))
                |> Option.defaultValue defaultNativeActorAddress

            debugPrint
                "actor-argu-proxy"
                $"RECEIVED self={selfRef.Path}; sender={replyTo.Path}; commandId={command.CommandId}; command.ActorAddress={command.ActorAddress}; command.TargetActorAddress={command.TargetActorAddress}; nativeActorAddress={nativeActorAddress}; raw={rawArgu}"

            async {
                let! nativeResult =
                    async {
                        try
                            let nativeSelection =
                                actorSystem.ActorSelection(nativeActorAddress)

                            debugPrint "actor-argu-proxy" $"ASK native={nativeActorAddress}; timeout={askTimeout}; raw={rawArgu}"
                            let! nativeReply =
                                nativeSelection.Ask<obj>(rawArgu, askTimeout)
                                |> Async.AwaitTask

                            match nativeReplyToCell nativeReply with
                            | Ok cell ->
                                let handledCell = replyCellHandler cell
                                debugPrint "actor-argu-proxy" $"NATIVE-REPLY native={nativeActorAddress}; type={nativeReply.GetType().FullName}; reply={cell}"
                                debugPrint "actor-argu-proxy" $"HANDLED-REPLY native={nativeActorAddress}; reply={handledCell}"
                                return Ok handledCell
                            | Error message ->
                                let ex = InvalidOperationException message
                                debugPrint "actor-argu-proxy" $"NATIVE-ERROR native={nativeActorAddress}; error={message}"
                                return Error(ex :> exn)
                        with ex ->
                            debugPrint "actor-argu-proxy" $"NATIVE-ERROR native={nativeActorAddress}; error={ex.GetType().FullName}: {ex.Message}"
                            return Error ex
                    }

                let reply: ActorArguTargetReply =
                    match nativeResult with
                    | Ok cell ->
                        { Value = cell
                          Direction = Some "inbound-message"
                          Tags = Some [ "poc-full-nuget-journal"; "by-proxy"; "native-fcell2" ] }
                    | Error ex ->
                        { Value = fCell2<string>.S("by-proxy native fCell2 actor failed: " + ex.GetType().Name + ": " + ex.Message)
                          Direction = Some "durable-proxy-error"
                          Tags = Some [ "poc-full-nuget-journal"; "by-proxy"; "error" ] }

                debugPrint "actor-argu-proxy" $"REPLY-TO-PTCS self={selfRef.Path}; replyTo={replyTo.Path}; reply={reply.Value}"
                replyTo.Tell(reply, selfRef)
            }
            |> Async.StartImmediate)
        |> ignore

    member _.ActorCtx: IActorContext = ActorBase.Context

let parser =
    ArgumentParser.Create<CliArgs>(programName = "full.nuget.journal.ACL2.NoGithubOAuth.ByProxy.fsx")

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

let truncateForLog maxChars (value: string) =
    let value = if isNull value then "" else value

    if value.Length <= maxChars then
        value
    else
        value.Substring(0, maxChars) + sprintf "...<truncated:%d chars>" value.Length

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
    let host = textOr "10.28.112.109" host

    if port <= 0 then
        WebBinding.randomHost host
    else
        WebBinding.fixedHostPort host port

let bindAddressForHost host =
    let value = textOr "10.28.112.109" host
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
    let defaultValue = defaultParsed.GetResult(Host, "10.28.112.109")
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

let defaultRequestedDemo = defaultParsed.Contains Demo
let overrideRequestedDemo = overrideParsed |> Option.exists (fun parsed -> parsed.Contains Demo)
let defaultRequestedProductionSql = defaultParsed.Contains Production_Sql
let overrideRequestedProductionSql = overrideParsed |> Option.exists (fun parsed -> parsed.Contains Production_Sql)
let defaultHasEncryptedSqlArgs = hasEncryptedSqlArgs defaultParsed
let overrideHasEncryptedSqlArgs = overrideParsed |> Option.exists hasEncryptedSqlArgs

let loginProviderModeReason =
    [ $"productionSql={productionSql}"
      $"demoMode={demoMode}"
      $"defaultDemoFlag={defaultRequestedDemo}"
      $"overrideDemoFlag={overrideRequestedDemo}"
      $"defaultProductionSqlFlag={defaultRequestedProductionSql}"
      $"overrideProductionSqlFlag={overrideRequestedProductionSql}"
      $"defaultEncryptedSqlArgs={defaultHasEncryptedSqlArgs}"
      $"overrideEncryptedSqlArgs={overrideHasEncryptedSqlArgs}" ]
    |> String.concat "; "

let localPort =
    if ifDynaPort then
        freePort ()
    else
        let defaultValue = defaultParsed.GetResult(Local_Port, 82)
        overrideParsed |> Option.map (fun parsed -> parsed.GetResult(Local_Port, defaultValue)) |> Option.defaultValue defaultValue

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
    let defaultValue = defaultParsed.GetResult(Actor_Name, "nuget-journal-acl-byproxy")
    overrideParsed |> Option.map (fun parsed -> parsed.GetResult(Actor_Name, defaultValue)) |> Option.defaultValue defaultValue |> textOr "nuget-journal-acl-byproxy"

let nativeActorAddressOverride =
    overrideParsed
    |> Option.bind (fun parsed -> parsed.TryGetResult <@ Native_Actor_Address @>)
    |> Option.orElseWith (fun () -> defaultParsed.TryGetResult <@ Native_Actor_Address @>)
    |> Option.map (fun value -> if isNull value then "" else value.Trim())
    |> Option.filter (String.IsNullOrWhiteSpace >> not)

let clusterHost =
    let defaultValue = defaultParsed.GetResult(Cluster_Host, "10.28.112.109")
    overrideParsed
    |> Option.map (fun parsed -> parsed.GetResult(Cluster_Host, defaultValue))
    |> Option.defaultValue defaultValue
    |> textOr "10.28.112.109"

let clusterPort =
    if ifDynaPort then
        freePort ()
    else
        let defaultValue = defaultParsed.GetResult(Cluster_Port, 7788)
        overrideParsed |> Option.map (fun parsed -> parsed.GetResult(Cluster_Port, defaultValue)) |> Option.defaultValue defaultValue

let nativeNodeHost =
    let defaultValue = defaultParsed.GetResult(Native_Node_Host, clusterHost)
    overrideParsed
    |> Option.map (fun parsed -> parsed.GetResult(Native_Node_Host, defaultValue))
    |> Option.defaultValue defaultValue
    |> textOr clusterHost

let nativeNodePort =
    let requested =
        overrideParsed
        |> Option.map (fun parsed -> parsed.GetResult(Native_Node_Port, defaultParsed.GetResult(Native_Node_Port, 0)))
        |> Option.defaultValue (defaultParsed.GetResult(Native_Node_Port, 0))

    if requested <= 0 then freePort () else requested

let sqlSecuritySchema =
    let defaultValue = defaultParsed.GetResult(Sql_Security_Schema, "ptcs_poc_acl")
    overrideParsed |> Option.map (fun parsed -> parsed.GetResult(Sql_Security_Schema, defaultValue)) |> Option.defaultValue defaultValue |> textOr "ptcs_poc_acl"

let sqlAclTable =
    let defaultValue = defaultParsed.GetResult(Sql_Acl_Table, "AclPolicySnapshot")
    overrideParsed |> Option.map (fun parsed -> parsed.GetResult(Sql_Acl_Table, defaultValue)) |> Option.defaultValue defaultValue |> textOr "AclPolicySnapshot"

let noWait =
    defaultParsed.Contains No_Wait
    || (overrideParsed |> Option.exists (fun parsed -> parsed.Contains No_Wait))

let startupProbe =
    defaultParsed.Contains Startup_Probe
    || (overrideParsed |> Option.exists (fun parsed -> parsed.Contains Startup_Probe))

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

let normalizeHostText (value: string) =
    (if isNull value then "" else value.Trim())
        .TrimStart('[')
        .TrimEnd(']')
        .ToLowerInvariant()

let isLoopbackHost value =
    let host = normalizeHostText value
    host = "localhost"
    || host = "::1"
    || host.StartsWith("127.")

let localIPv4AddressSet () =
    NetworkInterface.GetAllNetworkInterfaces()
    |> Array.collect (fun ni ->
        ni.GetIPProperties().UnicastAddresses
        |> Seq.filter (fun address -> address.Address.AddressFamily = AddressFamily.InterNetwork)
        |> Seq.map (fun address -> address.Address.ToString())
        |> Seq.toArray)
    |> Set.ofArray

let requireLocalIPv4Address argName host =
    let normalized = normalizeHostText host

    if not (String.IsNullOrWhiteSpace normalized)
       && not (isLoopbackHost normalized)
       && normalized <> "0.0.0.0"
       && normalized <> "*"
       && normalized <> "+" then
        let mutable parsed = Unchecked.defaultof<IPAddress>

        if IPAddress.TryParse(normalized, &parsed) && parsed.AddressFamily = AddressFamily.InterNetwork then
            let localAddresses = localIPv4AddressSet ()

            if not (localAddresses.Contains normalized) then
                let localAddressText =
                    localAddresses |> Set.toList |> String.concat ", "

                invalidOp
                    $"{argName}={host} is not assigned to this machine. Local IPv4 addresses: {localAddressText}"
        else
            ()

let tryActorAddressHost (address: string) =
    let text = if isNull address then "" else address.Trim()
    let m = Regex.Match(text, @"@(?<host>\[[^\]]+\]|[^:/]+)(?::\d+)?/")

    if m.Success then
        Some(normalizeHostText m.Groups["host"].Value)
    else
        None

let validateExternalNativeActorAddress label nativeActorAddress =
    match nativeActorAddress with
    | Some nativeAddress ->
        match tryActorAddressHost nativeAddress with
        | Some nativeHost when not (isLoopbackHost nativeHost) && isLoopbackHost clusterHost ->
            invalidOp
                $"{label} host={nativeHost} requires --cluster-host <this-machine-reachable-ip-or-dns>. Current --cluster-host is {clusterHost}; remote replies cannot reach 127.0.0.1."
        | _ -> ()
    | None -> ()

validateExternalNativeActorAddress "--native-actor-address" nativeActorAddressOverride

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
    requireTcpPortFree "local PTCS.Login HTTP listener" "--local-port" host localPort
    requireTcpPortFree "Akka cluster remoting" "--cluster-port" clusterHost clusterPort

if String.Equals(normalizeHostText clusterHost, normalizeHostText nativeNodeHost, StringComparison.OrdinalIgnoreCase)
   && clusterPort = nativeNodePort then
    invalidOp "The native PingPong Akka node must not reuse the PTCS fabric node address. Change --native-node-port or use --if-dyna-port."

if noWait then
    if String.Equals(normalizeHostText clusterHost, normalizeHostText nativeNodeHost, StringComparison.OrdinalIgnoreCase) then
        invalidOp
            $"No-wait dual-IP proof requires --cluster-host and --native-node-host to be different LAN IPs. Current cluster={clusterHost}; native={nativeNodeHost}."

    requireLocalIPv4Address "--cluster-host" clusterHost
    requireLocalIPv4Address "--native-node-host" nativeNodeHost

requireTcpPortFree "native PingPong Akka.Remote listener" "--native-node-port" nativeNodeHost nativeNodePort

printfn
    "startup preflight ok: local=%s:%d cluster=%s:%d native-pingpong=%s:%d dynamic=%b startupProbe=%b"
    host
    localPort
    clusterHost
    clusterPort
    nativeNodeHost
    nativeNodePort
    ifDynaPort
    startupProbe

if clearPcslBeforeStart then
    ensureSafeClearPcslRoot pcslRoot

Directory.CreateDirectory pcslRoot |> ignore

let encryptedSqlConnectionString () =
    let tryGetMerged selector =
        overrideParsed
        |> Option.bind selector
        |> Option.orElseWith (fun () -> selector defaultParsed)

    let encryptedFile =
        tryGetMerged (fun parsed -> parsed.TryGetResult <@ Sql_Connection_String_Encrypted_File @>)
        |> Option.map (fullPath @"D:\ingted.com")
        |> Option.defaultWith (fun () -> invalidOp "production-sql requires --sql-connection-string-encrypted-file.")

    let privateKeyPath =
        tryGetMerged (fun parsed -> parsed.TryGetResult <@ Sql_Private_Key_Path @>)
        |> Option.map (fullPath @"D:\ingted.com")
        |> Option.defaultWith (fun () -> invalidOp "production-sql requires --sql-private-key-path.")

    let keySize =
        overrideParsed
        |> Option.map (fun parsed -> parsed.GetResult(Sql_Key_Size, defaultParsed.GetResult(Sql_Key_Size, 2048)))
        |> Option.defaultValue (defaultParsed.GetResult(Sql_Key_Size, 2048))

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

let redactedSqlConnectionSummary (connectionString: string) =
    try
        let builder = SqlConnectionStringBuilder(connectionString)

        let auth =
            if builder.IntegratedSecurity then
                "integrated-security=true"
            elif String.IsNullOrWhiteSpace builder.UserID then
                "sql-user=<empty>"
            else
                "sql-user=" + builder.UserID

        [ "dataSource=" + builder.DataSource
          "initialCatalog=" + builder.InitialCatalog
          auth
          "encrypt=" + string builder.Encrypt
          "trustServerCertificate=" + string builder.TrustServerCertificate
          "connectTimeout=" + string builder.ConnectTimeout ]
        |> String.concat "; "
    with ex ->
        $"unparseable connection string metadata: {ex.GetType().Name}: {ex.Message}"

match productionSqlConnectionString with
| Some connectionString ->
    printfn "SQL connection metadata (redacted) %s" (redactedSqlConnectionSummary connectionString)
| None ->
    printfn "SQL connection metadata (redacted) no explicit SQL connection string; using Journal.sqlServerLocal db=%s" sqlDb

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

let commandHooks =
    CommSpaCommandHooks.noop
    |> CommSpaCommandHooks.withHookId "genfile4-explicit-actor-argu-target"

let pcslOptions =
    { PcslCommSpaPersistenceOptions.defaults with
        BasePath = pcslRoot
        SeedInitialStateWhenEmpty = false }
    |> fun options -> Journal.withPcslProjectionJournalId(journal, options)

let projectionHub =
    CommHub.createEmptyWithPcslOptions pcslOptions

let hoconQuote (value: string) =
    JsonSerializer.Serialize(if isNull value then "" else value)

let hyperionSerializerTypeName =
    typeof<Akka.Serialization.HyperionSerializer>.AssemblyQualifiedName

let hyperionSerializerConfigText =
    $"""
akka.actor {{
  serializers {{
    hyperion = {hoconQuote hyperionSerializerTypeName}
  }}
  serialization-identifiers {{
    {hoconQuote hyperionSerializerTypeName} = -5
  }}
}}
"""

let fabricOptions =
    { CommSpaActorFabricOptions.defaults with
        SystemName = "PFCF" //+ persistenceHash.Substring(0, 8)
        ShardTypeName = "ptcs-dynamic-poc-journal-" + persistenceHash.Substring(0, 8)
        ClusterHost = clusterHost
        ClusterPort = clusterPort }
    |> CommSpaActorFabricOptions.withJournal journal
    |> CommSpaActorFabricOptions.withJournalQueryAdapter journalQueryAdapter
    |> CommSpaActorFabricOptions.withPersistenceNamespace persistenceNamespace
    |> CommSpaActorFabricOptions.withCommandHooks commandHooks

let fabricActorSystemConfigText =
    $"""
akka {{
  loglevel = "WARNING"
  stdout-loglevel = "WARNING"
  actor.provider = cluster
  remote.dot-netty.tcp.hostname = {hoconQuote fabricOptions.ClusterHost}
  remote.dot-netty.tcp.port = {fabricOptions.ClusterPort}
  cluster.seed-nodes = []
  cluster.roles = []
  cluster.failure-detector.acceptable-heartbeat-pause = 30s
}}

{hyperionSerializerConfigText}
"""

let fabricActorSystemConfig =
    ConfigurationFactory
        .ParseString(fabricActorSystemConfigText)
        .WithFallback(CommSpaActorFabric.requiredConfig fabricOptions CommSpaActorFabricConfigPurpose.RegionHost)

let fabricActorSystem =
    ActorSystem.Create(fabricOptions.SystemName, fabricActorSystemConfig)

let fabric =
    try
        let attachmentOptions =
            { CommSpaActorFabricAttachmentOptions.regionHost with
                Ownership = CommSpaActorFabricOwnership.OwnsActorSystem
                JoinClusterIfNeeded = true
                WaitForSelfUp = true }

        CommSpaActorFabric.attachToSystem fabricOptions attachmentOptions fabricActorSystem (Some projectionHub.PersistenceBackend)
    with _ ->
        fabricActorSystem.Terminate().Wait(TimeSpan.FromSeconds 10.0) |> ignore
        reraise()

let nativePingPongSystemName =
    "PFCFNative" + persistenceHash.Substring(0, 8)

let nativePingPongNodeAddress =
    $"akka.tcp://{nativePingPongSystemName}@{nativeNodeHost}:{nativeNodePort}"

let nativePingPongConfigText =
    $"""
akka {{
  loglevel = "WARNING"
  stdout-loglevel = "WARNING"
  actor {{
    provider = remote
    serializers {{
      comm-spa-json = "Akka.Serialization.NewtonSoftJsonSerializer, Akka"
    }}
    serialization-bindings {{
      "PersistedConcurrentSortedList.Type.fCell2`1, FAkka.FCell2" = comm-spa-json
    }}
    serialization-settings {{
      comm-spa-json {{
        encode-type-names = on
        preserve-object-references = on
      }}
    }}
  }}
  remote.dot-netty.tcp.hostname = {hoconQuote nativeNodeHost}
  remote.dot-netty.tcp.port = {nativeNodePort}
}}
"""

let nativePingPongConfig =
    ConfigurationFactory.ParseString(nativePingPongConfigText).WithFallback(ConfigurationFactory.Default())

let nativePingPongSystem =
    ActorSystem.Create(nativePingPongSystemName, nativePingPongConfig)

printfn "Native PingPong node started: %s" nativePingPongNodeAddress

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

let executeSqlQueryRows (connectionString: string) (commandText: string) (readRow: SqlDataReader -> string) =
    use connection = new SqlConnection(connectionString)
    connection.Open()
    use command = connection.CreateCommand()
    command.CommandText <- commandText
    command.CommandTimeout <- 30
    use reader = command.ExecuteReader()
    let rows = ResizeArray<string>()

    while reader.Read() do
        rows.Add(readRow reader)

    rows |> Seq.toList

let loginSqlConfig connectionString =
    LoginSqlServer.LoginSqlServerProviderConfig.create connectionString
    |> LoginSqlServer.LoginSqlServerProviderConfig.withSchema sqlSecuritySchema
    |> LoginSqlServer.LoginSqlServerProviderConfig.withEnsureDatabase true
    |> LoginSqlServer.LoginSqlServerProviderConfig.withEnsureSchema true

let aclSqlConfig connectionString =
    AclSqlServerProviderConfig.create connectionString
    |> AclSqlServerProviderConfig.withSchema sqlSecuritySchema
    |> AclSqlServerProviderConfig.withTableName sqlAclTable
    |> AclSqlServerProviderConfig.withEnsureDatabase true
    |> AclSqlServerProviderConfig.withEnsureSchema true

let adminLoginName = "admin"
let wzLoginName = "wz"
let terryLoginName = "terry"
let disabledLoginName = "disabled-terry"
let adminPassword = if productionSql then "admin" else "demo:admin"
let wzPassword = if productionSql then "wz" else "demo:wz"
let terryPassword = if productionSql then "terry" else "demo:terry"
let disabledPassword = if productionSql then "disabled-terry" else "demo:disabled-terry"
let sysAdminLoginName = if productionSql then wzLoginName else adminLoginName
let sysAdminPassword = if productionSql then wzPassword else adminPassword

let loginCredentialHints =
    [ "sys-admin verifier", sysAdminLoginName, sysAdminPassword
      "Terry黑粉 verifier", terryLoginName, terryPassword
      "legacy admin seed", adminLoginName, adminPassword
      "disabled negative test", disabledLoginName, disabledPassword ]

let printLoginStartupDiagnostics () =
    printfn "Login provider mode %s" loginProviderModeReason

    if productionSql && defaultRequestedDemo && defaultHasEncryptedSqlArgs then
        printfn "Login provider note defaultArgumentsText contains --demo and encrypted SQL args; current script chooses production-sql because SQL args are present."

    printfn "Login test credential hints (POC users only; SQL connection secret is never printed):"

    for label, userName, password in loginCredentialHints do
        printfn "  %s -> user=%s password=%s" label userName password

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

let seedProductionSqlSecurity connectionString =
    let loginConfig = loginSqlConfig connectionString
    let aclConfig = aclSqlConfig connectionString

    LoginSqlServer.ensureStoreAsync loginConfig |> Async.RunSynchronously
    LoginSqlServer.ensureCredentialStoreAsync loginConfig |> Async.RunSynchronously
    ensureStoreAsync aclConfig |> Async.RunSynchronously

    let schema = quoteIdentifier sqlSecuritySchema
    let loginUser = schema + "." + quoteIdentifier "LoginUser"
    let loginUserGroup = schema + "." + quoteIdentifier "LoginUserGroup"
    let loginUserRole = schema + "." + quoteIdentifier "LoginUserRole"
    let pocUserIds =
        [ "user.admin"; "user.wz"; "user.terry-hater"; "user.disabled-terry" ]

    let quotedUserIds =
        pocUserIds
        |> List.map (fun value -> "N'" + value.Replace("'", "''") + "'")
        |> String.concat ", "

    executeSqlNonQuery
        connectionString
        $"DELETE FROM {loginUserGroup} WHERE UserId IN ({quotedUserIds});
DELETE FROM {loginUserRole} WHERE UserId IN ({quotedUserIds});
DELETE FROM {loginUser} WHERE UserId IN ({quotedUserIds});"

    let adminSeed: LoginSqlServer.SqlServerLoginCredentialSeed =
        { UserId = "user.admin"
          LoginName = adminLoginName
          DisplayName = Some "Admin"
          Provider = "ptcs-login"
          Enabled = true
          PasswordSecret = adminPassword
          Groups = [ "sys-admin" ]
          Roles = [ "admin" ]
          Iterations = Some 210000 }

    let terrySeed: LoginSqlServer.SqlServerLoginCredentialSeed =
        { UserId = "user.terry-hater"
          LoginName = terryLoginName
          DisplayName = Some "Terry黑粉"
          Provider = "ptcs-login"
          Enabled = true
          PasswordSecret = terryPassword
          Groups = [ "Terry黑粉" ]
          Roles = [ "user" ]
          Iterations = Some 210000 }

    let wzSeed: LoginSqlServer.SqlServerLoginCredentialSeed =
        { UserId = "user.wz"
          LoginName = wzLoginName
          DisplayName = Some "DamnWZ"
          Provider = "ptcs-login"
          Enabled = true
          PasswordSecret = wzPassword
          Groups = [ "sys-admin" ]
          Roles = [ "admin" ]
          Iterations = Some 210000 }

    let disabledSeed: LoginSqlServer.SqlServerLoginCredentialSeed =
        { UserId = "user.disabled-terry"
          LoginName = disabledLoginName
          DisplayName = Some "Disabled Terry"
          Provider = "ptcs-login"
          Enabled = false
          PasswordSecret = disabledPassword
          Groups = [ "Terry黑粉" ]
          Roles = [ "user" ]
          Iterations = Some 210000 }

    LoginSqlServer.upsertCredentialUserAsync loginConfig adminSeed |> Async.RunSynchronously
    LoginSqlServer.upsertCredentialUserAsync loginConfig wzSeed |> Async.RunSynchronously
    LoginSqlServer.upsertCredentialUserAsync loginConfig terrySeed |> Async.RunSynchronously
    LoginSqlServer.upsertCredentialUserAsync loginConfig disabledSeed |> Async.RunSynchronously

    let policy =
        pocAclPolicy
            ("poc-full-nuget-journal-acl-sql-" + DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss"))
            PrivateLan
            (Some "ptcs-login")
            [ { UserId = adminSeed.UserId
                Groups = adminSeed.Groups
                Roles = adminSeed.Roles }
              { UserId = wzSeed.UserId
                Groups = wzSeed.Groups
                Roles = wzSeed.Roles }
              { UserId = terrySeed.UserId
                Groups = terrySeed.Groups
                Roles = terrySeed.Roles }
              { UserId = disabledSeed.UserId
                Groups = disabledSeed.Groups
                Roles = disabledSeed.Roles } ]

    ensureAndSavePolicyConfigAsync aclConfig true policy |> Async.RunSynchronously

    let credentialRows =
        executeSqlQueryRows
            connectionString
            $"SELECT LoginName, UserId, Provider, Enabled
FROM {loginUser}
WHERE UserId IN ({quotedUserIds})
ORDER BY LoginName;"
            (fun reader ->
                let loginName = reader.GetString(0)
                let userId = reader.GetString(1)
                let provider = reader.GetString(2)
                let enabled = reader.GetBoolean(3)
                $"loginName={loginName}; userId={userId}; provider={provider}; enabled={enabled}")

    printfn "SQL credential seed readback schema=%s table=LoginUser rows=%d" sqlSecuritySchema credentialRows.Length

    for row in credentialRows do
        printfn "  SQL credential row %s" row

    printfn "SQL ACL policy seeded schema=%s table=%s revision=%s users=%d rules=%d" sqlSecuritySchema sqlAclTable policy.Revision policy.Users.Length policy.Rules.Length

    loginConfig, aclConfig

printfn "initializing ACL/Login providers mode=%s" (if productionSql then "production-sql" else "demo")
printLoginStartupDiagnostics ()

let seededSqlConfigs =
    match productionSqlConnectionString with
    | Some connectionString when productionSql -> Some(seedProductionSqlSecurity connectionString)
    | _ -> None

let aclOptions =
    match seededSqlConfigs with
    | Some(_, aclConfig) ->
        match ensureAndLoadActivePolicySnapshotAsync aclConfig |> Async.RunSynchronously with
        | Ok snapshot ->
            { Snapshot = snapshot
              AuditSink = None }
        | Error error -> invalidOp $"PTCS ACL SQL policy load failed: {error.Message}"
    | None ->
        match PtcsAcl.create (AclPolicyConfig.demo()) with
        | Ok value -> value
        | Error error -> invalidOp $"PTCS ACL demo policy decode failed: {error.Message}"

let loginOptions =
    match seededSqlConfigs with
    | Some(loginConfig, _) ->
        let sessionStore = LoginSqlServer.createSessionStore loginConfig
        let baseConfig = LoginConfig.demo()

        let productionConfig =
            { baseConfig with
                DeploymentProfile = PrivateLan
                Users = []
                DurableSessionStore = true
                Token =
                    { baseConfig.Token with
                        Issuer = "ptcs-login"
                        Audience = "ptcs"
                        HostId = "poc-full-nuget-journal-acl" } }

        let dependencies =
            let baseDependencies = PtcsLogin.localDevDependenciesWithSessionStore sessionStore

            { baseDependencies with
                CredentialVerifier = fun _ -> LoginSqlServer.createCredentialVerifier loginConfig
                SessionStore = sessionStore }

        match PtcsLogin.coreFromConfigWithDependencies dependencies productionConfig with
        | Ok core -> PtcsLogin.fromLoginCore core
        | Error error -> invalidOp $"PTCS.Login production SQL config decode failed: {error.Message}"
    | None ->
        match PtcsLogin.demoLocalDev () with
        | Ok value -> value
        | Error error -> invalidOp $"PTCS.Login demo config decode failed: {error.Message}"

printfn "ACL/Login providers ready mode=%s" (if productionSql then "production-sql" else "demo")

let localLoginOptions =
    ServerOptions.localRandomWithHub hub
    |> ServerOptions.withWebBinding (webBinding host localPort)
    |> Server.withActorFabric fabric
    |> PtcsLoginExtension.usePtcsLogin loginOptions
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
        let value = textOr "10.28.112.109" host

        if String.Equals(value, "0.0.0.0", StringComparison.OrdinalIgnoreCase)
           || String.Equals(value, "::", StringComparison.OrdinalIgnoreCase) then
            "10.28.112.109"
        else
            value

    $"http://{clientHost}:{port}"

let localClientBaseUrl =
    clientBaseUrlForHostPort host localPort

let localApp =
    startPtcsHost "Local PTCS.Login" (sprintf "%s/login?returnUrl=/actors" localClientBaseUrl) (fun () -> Server.startWithSharing siteSharing localLoginOptions)

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

let proxyActorRegistrySettings proxyKind nativeAddress =
    ActorRegistrySettings.create actorRegistrySink
    |> ActorRegistrySettings.withRegistryId ("ptcs-dynamic-poc-full-nuget-journal-byproxy-" + proxyKind)
    |> ActorRegistrySettings.withNodeId (Some "ptcs.dynamic.poc-full-nuget-journal")
    |> ActorRegistrySettings.withRole (Some "ptcs-dynamic-journal")
    |> ActorRegistrySettings.withTags [ "ptcs-dynamic"; "poc-full-nuget-journal"; "by-proxy"; "actor-argu"; proxyKind ]
    |> ActorRegistrySettings.withMetadata (
        Map.ofList
            [ "ptcs.dynamic.poc", "full.nuget.journal.ACL2.NoGithubOAuth.ByProxy.fsx"
              "ptcs.dynamic.actor.kind", "actor-argu-string-proxy"
              "ptcs.dynamic.proxy.kind", proxyKind
              "ptcs.dynamic.proxy.nativeActorAddress", nativeAddress
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

let nativeStringActorName =
    actorName + "-native-string"

let nativeStringRegistrySettings =
    ActorRegistrySettings.create actorRegistrySink
    |> ActorRegistrySettings.withRegistryId "ptcs-dynamic-poc-full-nuget-journal-native-string"
    |> ActorRegistrySettings.withNodeId (Some "ptcs.dynamic.poc-full-nuget-journal")
    |> ActorRegistrySettings.withRole (Some "ptcs-dynamic-journal")
    |> ActorRegistrySettings.withTags [ "ptcs-dynamic"; "poc-full-nuget-journal"; "native-string"; "by-proxy-target" ]
    |> ActorRegistrySettings.withMetadata (
        Map.ofList
            [ "ptcs.dynamic.poc", "full.nuget.journal.ACL2.NoGithubOAuth.ByProxy.fsx"
              "ptcs.dynamic.actor.kind", "native-string"
              "ptcs.dynamic.journal.db", journalBootstrap.DatabaseName
              "ptcs.dynamic.pcsl.root", pcslRoot ])

let actorOfRegisteredNativeString () =
    fabric.System.ActorOfRegistered(nativeStringRegistrySettings, Props.Create(fun () -> PocFullNugetJournalNativeStringActor()), nativeStringActorName)

let spawnNativeStringActorRegisteredStrict () =
    match actorOfRegisteredNativeString () with
    | Ok registered -> registered.Actor
    | Error error ->
        invalidOp $"ActorRegistry ActorOfRegistered failed actor={nativeStringActorName} kind={error.Kind} message={error.Message}"

let spawnNativeStringActorRegisteredOrReuseLive () =
    match actorOfRegisteredNativeString () with
    | Ok registered -> registered.Actor
    | Error error when isActorNameNotUnique error ->
        match tryResolveUserActor nativeStringActorName with
        | Some existing ->
            printfn "Native string actor already exists; reusing %s." (existing.Path.ToStringWithoutAddress())
            existing
        | None ->
            invalidOp $"ActorRegistry ActorOfRegistered found duplicate name but could not resolve existing actor={nativeStringActorName} message={error.Message}"
    | Error error ->
        invalidOp $"ActorRegistry ActorOfRegistered failed actor={nativeStringActorName} kind={error.Kind} message={error.Message}"

let localNativeStringRef =
    match nativeActorAddressOverride with
    | Some externalAddress ->
        printfn "Using external native string actor address: %s" externalAddress
        None
    | None ->
        Some(spawnNativeStringActorRegisteredOrReuseLive ())

let nativeStringActorAddress =
    match nativeActorAddressOverride, localNativeStringRef with
    | Some externalAddress, _ -> externalAddress
    | None, Some actorRef -> fabric.NodeAddress.TrimEnd('/') + actorRef.Path.ToStringWithoutAddress()
    | None, None -> invalidOp "Local native string actor was not spawned."

match localNativeStringRef with
| Some _ -> tryForceReplay "actor-registry-after-native-string-register" CommSpaActorRegistry.registryStreamKey |> ignore
| None -> ()

let actorOfRegisteredProxy proxyName proxyKind nativeAddress =
    fabric.System.ActorOfRegistered(
        proxyActorRegistrySettings proxyKind nativeAddress,
        Props.Create(fun () -> PocFullNugetJournalActorArguStringProxyActor(fabric.System, nativeAddress, TimeSpan.FromSeconds 15.0, actorArguReplyCellHandler)),
        proxyName)

let spawnProxyActorRegisteredStrict proxyName proxyKind nativeAddress =
    match actorOfRegisteredProxy proxyName proxyKind nativeAddress with
    | Ok registered -> registered.Actor
    | Error error ->
        invalidOp $"ActorRegistry ActorOfRegistered failed actor={proxyName} kind={error.Kind} message={error.Message}"

let tryAskProxyNativeActorAddress (actorRef: IActorRef) =
    try
        actorRef
            .Ask<string>(GetNativeActorAddress, TimeSpan.FromMilliseconds 750.0)
            .GetAwaiter()
            .GetResult()
        |> Some
    with _ ->
        None

let spawnProxyActorRegisteredOrReuseLive proxyName proxyKind nativeAddress =
    match actorOfRegisteredProxy proxyName proxyKind nativeAddress with
    | Ok registered -> registered.Actor
    | Error error when isActorNameNotUnique error ->
        match tryResolveUserActor proxyName with
        | Some existing ->
            match tryAskProxyNativeActorAddress existing with
            | Some currentNativeAddress when String.Equals(currentNativeAddress, nativeAddress, StringComparison.Ordinal) ->
                printfn "Proxy actor already exists with matching native route; reusing %s -> %s." (existing.Path.ToStringWithoutAddress()) currentNativeAddress
                existing
            | Some currentNativeAddress ->
                printfn "Proxy actor already exists with stale native route; recreating %s current=%s expected=%s." proxyName currentNativeAddress nativeAddress
                fabric.System.Stop(existing)
                waitUntilUserActorGone proxyName (TimeSpan.FromSeconds 5.0) |> ignore

                match actorOfRegisteredProxy proxyName proxyKind nativeAddress with
                | Ok registered -> registered.Actor
                | Error recreateError ->
                    invalidOp $"ActorRegistry ActorOfRegistered failed after stale proxy recreate actor={proxyName} kind={recreateError.Kind} message={recreateError.Message}"
            | None ->
                invalidOp $"Proxy actor {proxyName} already exists but did not answer GetNativeActorAddress; stop the old FSI host or use a different --actor-name."
        | None ->
            invalidOp $"ActorRegistry ActorOfRegistered found duplicate name but could not resolve existing actor={proxyName} message={error.Message}"
    | Error error ->
        invalidOp $"ActorRegistry ActorOfRegistered failed actor={proxyName} kind={error.Kind} message={error.Message}"

let spawnEchoActorRegisteredStrict () =
    spawnProxyActorRegisteredStrict actorName "echo-target" nativeStringActorAddress

let spawnEchoActorRegisteredOrReuseLive () =
    spawnProxyActorRegisteredOrReuseLive actorName "echo-target" nativeStringActorAddress

let mutable echoRef = spawnEchoActorRegisteredOrReuseLive ()
let mutable echoActorStopped = false

let actorAddress =
    fabric.NodeAddress.TrimEnd('/') + echoRef.Path.ToStringWithoutAddress()

let pfcfProtoTypingProxyActorName =
    actorName + "-pfcf-proxy"

let pfcfProtoTypingProxyRef =
    spawnProxyActorRegisteredOrReuseLive pfcfProtoTypingProxyActorName "pfcf-prototype-target" nativeStringActorAddress

let pfcfProxyActorAddress =
    fabric.NodeAddress.TrimEnd('/') + pfcfProtoTypingProxyRef.Path.ToStringWithoutAddress()

tryForceReplay "actor-registry-after-register" CommSpaActorRegistry.registryStreamKey |> ignore

let pingPongActorName =
    actorName + "-pingpong"

let pingPongRegistrySettings =
    ActorRegistrySettings.create actorRegistrySink
    |> ActorRegistrySettings.withRegistryId "ptcs-dynamic-poc-full-nuget-journal-pingpong"
    |> ActorRegistrySettings.withNodeId (Some "ptcs.dynamic.poc-full-nuget-journal.native-pingpong")
    |> ActorRegistrySettings.withRole (Some "ptcs-dynamic-native-pingpong")
    |> ActorRegistrySettings.withTags [ "ptcs-dynamic"; "poc-full-nuget-journal"; "pingpong"; "actor-registry-reload" ]
    |> ActorRegistrySettings.withMetadata (
        Map.ofList
            [ "ptcs.dynamic.poc", "full.nuget.journal.ACL2.NoGithubOAuth.ByProxy.fsx"
              "ptcs.dynamic.actor.kind", "pingpong"
              "ptcs.dynamic.actor.node", nativePingPongNodeAddress
              "ptcs.dynamic.journal.db", journalBootstrap.DatabaseName
              "ptcs.dynamic.pcsl.root", pcslRoot ])

let pingPongRegistered =
    match nativePingPongSystem.ActorOfRegistered(pingPongRegistrySettings, Props.Create(fun () -> PocFullNugetJournalPingPongActor()), pingPongActorName) with
    | Ok registered ->
        if registered.Watcher.IsNone then
            invalidOp $"ActorRegistry watcher was not created for actor={pingPongActorName}; stop/reload lifecycle cannot be verified."

        registered
    | Error error ->
        invalidOp $"ActorRegistry ActorOfRegistered failed actor={pingPongActorName} kind={error.Kind} message={error.Message}"

let pingPongRef = pingPongRegistered.Actor

let pingPongActorAddress =
    nativePingPongNodeAddress.TrimEnd('/') + pingPongRef.Path.ToStringWithoutAddress()

if pingPongActorAddress.StartsWith(fabric.NodeAddress.TrimEnd('/'), StringComparison.OrdinalIgnoreCase) then
    invalidOp $"PingPong native actor must be on a different node than PTCS fabric. fabric={fabric.NodeAddress}; pingpong={pingPongActorAddress}"

let pingPongProxyActorName =
    actorName + "-pingpong-proxy"

let pingPongProxyRef =
    spawnProxyActorRegisteredOrReuseLive pingPongProxyActorName "pingpong-target" pingPongActorAddress

let pingPongProxyActorAddress =
    fabric.NodeAddress.TrimEnd('/') + pingPongProxyRef.Path.ToStringWithoutAddress()

tryForceReplay "actor-registry-after-pingpong-register" CommSpaActorRegistry.registryStreamKey |> ignore
tryForceReplay "actor-registry-after-pingpong-proxy-register" CommSpaActorRegistry.registryStreamKey |> ignore

let echoTargetKeys =
    [ actorAddress; "target-v1"; nativeStringActorAddress; templateKey; defaultCanonicalArgString ]

let pingPongTargetKeys =
    [ pingPongProxyActorAddress; "target-v1"; pingPongActorAddress; templateKey; "--say \"ping\" --set-count 2 --mode fast --tag acl pingpong" ]

let pfcfProtoTypingTargetKeys =
    [ pfcfProxyActorAddress; "target-v1"; nativeStringActorAddress; pfcfProtoTypingTemplateKey; pfcfProtoTypingCanonicalArgString ]

let actorArguPage pageId title description =
    let basePage = ActorArgu.fCellChatPage pageId title pageId

    { basePage with
        Shape = "actor-argu"
        Description = description
        KeyPlaceholder = "[\"proxy-actor-address\", \"target-v1\", \"target-actor-address\", \"template-key\", \"--say \\\"hello\\\"\"]"
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
    [ echoTargetKeys, "Proxy target"
      pingPongTargetKeys, "PingPong target"
      pfcfProtoTypingTargetKeys, "PFCF prototype by-proxy target" ]

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
        DynamicArgStringTarget.scan pfcfProtoTypingTemplateRegistration pfcfProxyActorAddress pfcfProtoTypingCanonicalArgString

    let rebuiltRawArgu =
        DynamicArgStringTarget.buildRawArgu parsedTarget

    require
        (String.Equals(rebuiltRawArgu, pfcfProtoTypingCanonicalArgString, StringComparison.Ordinal))
        $"PFCF prototype raw command rebuild mismatch. expected={pfcfProtoTypingCanonicalArgString}; actual={rebuiltRawArgu}"

    let request: DynamicArguResolveTargetRequest =
        { Keys = pfcfProtoTypingTargetKeys |> List.toArray }

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

let loginLocalClient (client: HttpClient) loginName password label =
    let body =
        JsonSerializer.Serialize(
            {| userName = loginName
               password = password
               returnUrl = "/actors"
               keepSession = true |})

    printfn "[login:%s] mode=%s POST %s/login/api/submit user=%s bodyBytes=%d" label (if productionSql then "production-sql" else "demo") localClientBaseUrl loginName (Encoding.UTF8.GetByteCount body)

    use request = new HttpRequestMessage(HttpMethod.Post, localClientBaseUrl + "/login/api/submit")
    request.Headers.TryAddWithoutValidation("Origin", localClientBaseUrl) |> ignore
    request.Headers.Referrer <- Uri(localClientBaseUrl + "/login?returnUrl=/actors")
    request.Content <- new StringContent(body, Encoding.UTF8, "application/json")

    use response = client.SendAsync(request).GetAwaiter().GetResult()
    let responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
    printfn "[login:%s] status=%d reason=%s body=%s" label (int response.StatusCode) (string response.ReasonPhrase) (truncateForLog 1200 responseBody)

    let setCookieValues =
        let mutable values = Seq.empty<string>

        if response.Headers.TryGetValues("Set-Cookie", &values) then
            values |> Seq.toList
        else
            []

    let setCookieNames =
        setCookieValues
        |> List.choose (fun value ->
            value.Split(';', StringSplitOptions.RemoveEmptyEntries)
            |> Array.tryHead
            |> Option.map _.Trim()
            |> Option.filter (String.IsNullOrWhiteSpace >> not)
            |> Option.map (fun cookie ->
                let equalsIndex = cookie.IndexOf('=')
                if equalsIndex > 0 then cookie.Substring(0, equalsIndex) else cookie))
        |> String.concat ","

    printfn "[login:%s] set-cookie-count=%d set-cookie-names=%s" label setCookieValues.Length (if String.IsNullOrWhiteSpace setCookieNames then "<none>" else setCookieNames)

    if not response.IsSuccessStatusCode then
        invalidOp $"Local PTCS.Login {label} login failed: status={int response.StatusCode} body={responseBody}"

    if List.isEmpty setCookieValues then
        invalidOp "Local PTCS.Login did not return Set-Cookie."

    let sessionCookie =
        setCookieValues
        |> List.tryPick (fun value ->
            value.Split(';', StringSplitOptions.RemoveEmptyEntries)
            |> Array.tryHead
            |> Option.map _.Trim()
            |> Option.filter (String.IsNullOrWhiteSpace >> not))
        |> Option.defaultWith (fun () -> invalidOp "Local PTCS.Login returned an empty Set-Cookie header.")

    client.DefaultRequestHeaders.Remove("Cookie") |> ignore
    client.DefaultRequestHeaders.TryAddWithoutValidation("Cookie", sessionCookie) |> ignore

    try
        use probe = new HttpRequestMessage(HttpMethod.Get, localClientBaseUrl + "/acl/api/snapshot")
        probe.Headers.TryAddWithoutValidation("Origin", localClientBaseUrl) |> ignore
        probe.Headers.Referrer <- Uri(localClientBaseUrl + "/actors")
        probe.Headers.TryAddWithoutValidation("Cookie", sessionCookie) |> ignore
        use probeResponse = client.SendAsync(probe).GetAwaiter().GetResult()
        let probeBody = probeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult()
        printfn "[login:%s] session-probe GET /acl/api/snapshot status=%d body=%s" label (int probeResponse.StatusCode) (truncateForLog 1200 probeBody)
    with ex ->
        printfn "[login:%s] session-probe failed %s: %s" label (ex.GetType().FullName) ex.Message

    responseBody

let tryLoginLocalClient loginName password =
    use client = new HttpClient()
    let body =
        JsonSerializer.Serialize(
            {| userName = loginName
               password = password
               returnUrl = "/actors"
               keepSession = true |})

    use request = new HttpRequestMessage(HttpMethod.Post, localClientBaseUrl + "/login/api/submit")
    request.Headers.TryAddWithoutValidation("Origin", localClientBaseUrl) |> ignore
    request.Headers.Referrer <- Uri(localClientBaseUrl + "/login?returnUrl=/actors")
    request.Content <- new StringContent(body, Encoding.UTF8, "application/json")

    use response = client.SendAsync(request).GetAwaiter().GetResult()
    let responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
    printfn "[login-negative] user=%s status=%d body=%s" loginName (int response.StatusCode) (truncateForLog 1200 responseBody)
    int response.StatusCode, responseBody

let postJson (client: HttpClient) path jsonText =
    use request = new HttpRequestMessage(HttpMethod.Post, localClientBaseUrl + path)
    request.Headers.TryAddWithoutValidation("Origin", localClientBaseUrl) |> ignore
    request.Headers.Referrer <- Uri(localClientBaseUrl + "/actors")
    request.Content <- new StringContent(jsonText, Encoding.UTF8, "application/json")
    use response = client.SendAsync(request).GetAwaiter().GetResult()
    int response.StatusCode, response.Content.ReadAsStringAsync().GetAwaiter().GetResult()

let jsonArrayText values =
    JsonSerializer.Serialize(values |> List.toArray)

let addKeyJsonWithMode pageId keys displayName keyMode =
    JsonSerializer.Serialize(
        {| pageId = pageId
           keyJson = jsonArrayText keys
           keyMode = keyMode
           displayName = displayName |})

let addKeyJson pageId keys displayName =
    addKeyJsonWithMode pageId keys displayName "target"

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

let jsonContainsExactActorPathName (actorName: string) (jsonText: string) =
    jsonText.Contains("/" + actorName + "\"", StringComparison.Ordinal)

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
        printfn "Proxy actor is already live: %s" (fabric.NodeAddress.TrimEnd('/') + existing.Path.ToStringWithoutAddress())
        existing
    | None ->
        let spawned = spawnEchoActorRegisteredStrict ()
        echoRef <- spawned
        echoActorStopped <- false
        tryForceReplay "actor-registry-after-echo-ensure" CommSpaActorRegistry.registryStreamKey |> ignore
        printfn "Proxy actor registered: %s" (fabric.NodeAddress.TrimEnd('/') + spawned.Path.ToStringWithoutAddress())
        spawned

let stopEchoActor () =
    match tryResolveUserActor actorName with
    | None ->
        echoActorStopped <- true
        printfn "Proxy actor already stopped: %s" actorAddress
    | Some live ->
        echoActorStopped <- true
        fabric.System.Stop(live)
        let pathReleased = waitUntilUserActorGone actorName (TimeSpan.FromSeconds 10.0)
        let observedStatus = waitForHubActorStatus actorName "terminated" (TimeSpan.FromSeconds 5.0)
        tryForceReplay "actor-registry-after-echo-stop" CommSpaActorRegistry.registryStreamKey |> ignore
        printfn "Proxy actor stop requested: %s" (fabric.NodeAddress.TrimEnd('/') + live.Path.ToStringWithoutAddress())
        printfn "Proxy actor path released: %b" pathReleased
        printfn "Proxy actor projected status: %A" observedStatus
        printfn "Proxy registry events: %s" (registryEventSummary actorName)

let recreateEchoActor () =
    match tryResolveUserActor actorName with
    | Some _ -> stopEchoActor ()
    | None -> ()

    if not (waitUntilUserActorGone actorName (TimeSpan.FromSeconds 10.0)) then
        invalidOp $"Proxy actor path did not release before recreate: {userActorPath actorName}"

    let spawned = spawnEchoActorRegisteredStrict ()
    echoRef <- spawned
    echoActorStopped <- false
    tryForceReplay "actor-registry-after-echo-recreate" CommSpaActorRegistry.registryStreamKey |> ignore
    printfn "Proxy actor recreated: %s" (fabric.NodeAddress.TrimEnd('/') + spawned.Path.ToStringWithoutAddress())
    spawned

let verifyEchoActorReuseAfterStop () =
    let beforePath = echoRef.Path.ToStringWithoutAddress()
    let recreated = recreateEchoActor ()
    let afterPath = recreated.Path.ToStringWithoutAddress()

    require
        (String.Equals(beforePath, afterPath, StringComparison.Ordinal))
        $"Proxy actor should reuse the same path after stop/recreate. before={beforePath} after={afterPath}"

    require
        ((tryResolveUserActor actorName).IsSome)
        $"Proxy actor should resolve after recreate: {userActorPath actorName}"

    printfn "Proxy actor reuse-after-stop verified: %s" (fabric.NodeAddress.TrimEnd('/') + afterPath)
    true

let stopPingPongActor () =
    if pingPongActorStopped then
        printfn "PingPong actor already stopped: %s" pingPongActorAddress
    else
        pingPongActorStopped <- true
        nativePingPongSystem.Stop(pingPongRef)
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
        printfn "full.nuget.journal.ACL2.NoGithubOAuth.ByProxy host already stopped."
    else
        pocFullNugetJournalStopped <- true
        (localApp :> IDisposable).Dispose()
        fabric.Stop()
        fabric.System.WhenTerminated.Wait(TimeSpan.FromSeconds 10.0) |> ignore
        nativePingPongSystem.Terminate().Wait(TimeSpan.FromSeconds 10.0) |> ignore
        printfn "full.nuget.journal.ACL2.NoGithubOAuth.ByProxy host stopped."

let sendActorArguProbe label page routeActor keys rawArgu =
    let result =
        ActorArgu.sendDurableAsync
            ingress
            fabric
            { Send =
                { Page = page
                  ActorAddress = routeActor
                  HistoryKeys = Some keys
                  RawArgu = rawArgu
                  Tags = Some [ "poc-full-nuget-journal-acl"; "manual-probe"; label ] }
              IdempotencyKey = None
              Source = Some("GenFileActorInvocationTest4.fsx:" + label)
              DeadlineAtUtc = None }
            CancellationToken.None
        |> Async.RunSynchronously

    let payload =
        result.ActorArgu
        |> Option.map (fun reply -> reply.Event.Payload)
        |> Option.defaultValue "<no ActorArgu reply event>"

    printfn "ActorArgu probe %s delivery=%A payload=%s" label result.DeliveryStatus.Status payload
    result

let sendEchoProbe () =
    sendActorArguProbe
        "echo"
        assTerryPage
        actorAddress
        echoTargetKeys
        "--say \"manual echo probe\" --set-count 1 --mode fast"

let sendRawEchoProbe rawArgu =
    sendActorArguProbe "echo-raw" assTerryPage actorAddress echoTargetKeys rawArgu

let sendPfcfProbe () =
    sendActorArguProbe
        "pfcf"
        assTerryPage
        pfcfProxyActorAddress
        pfcfProtoTypingTargetKeys
        pfcfProtoTypingCanonicalArgString

let sendPingPongProbe () =
    sendActorArguProbe
        "pingpong"
        assTerryPage
        pingPongProxyActorAddress
        pingPongTargetKeys
        "--say \"ping\" --set-count 2 --mode fast --tag acl pingpong"

try
    let ensuredEcho = ensureEchoActorRegistered ()
    let shouldRunStartupProbe = noWait || startupProbe
    let targetVisible = shouldRunStartupProbe && defaultTargetVisible ()

    let serverProbe =
        if targetVisible then
            Some(sendActorArguProbe "startup" assTerryPage actorAddress echoTargetKeys "--say \"server probe\" --set-count 1 --mode fast")
        else
            None

    use client = new HttpClient()
    let localLoginReply = loginLocalClient client sysAdminLoginName sysAdminPassword "sys-admin"
    let healthText = client.GetStringAsync(localClientBaseUrl + "/healthz").GetAwaiter().GetResult()
    let journalHealthText = client.GetStringAsync(localClientBaseUrl + "/healthz.journal").GetAwaiter().GetResult()
    let persistenceHealthText = client.GetStringAsync(localClientBaseUrl + "/healthz.persistence").GetAwaiter().GetResult()
    let chatHtml = client.GetStringAsync(localClientBaseUrl + "/chat").GetAwaiter().GetResult()
    let actorsHtml = client.GetStringAsync(localClientBaseUrl + "/actors").GetAwaiter().GetResult()
    let actorsSnapshotJson = client.GetStringAsync(localClientBaseUrl + "/actors/api/snapshot").GetAwaiter().GetResult()
    let actorsSnapshotWithOfflineJson = client.GetStringAsync(localClientBaseUrl + "/actors/api/snapshot?includeOffline=1").GetAwaiter().GetResult()
    let aclSnapshotJson = client.GetStringAsync(localClientBaseUrl + "/acl/api/snapshot").GetAwaiter().GetResult()
    let aclExtensionJs = client.GetStringAsync(localClientBaseUrl + PtcsAclExtension.scriptUrl).GetAwaiter().GetResult()
    let loginExtensionJs = client.GetStringAsync(localClientBaseUrl + PtcsLoginExtension.scriptUrl).GetAwaiter().GetResult()
    let dynamicJs = client.GetStringAsync(localClientBaseUrl + "/ext/js/PulseTrade.Comm.Spa.Dynamic.js").GetAwaiter().GetResult()

    use terryClient = new HttpClient()
    let terryLoginReply = loginLocalClient terryClient terryLoginName terryPassword "Terry黑粉"
    let terryAclSnapshotJson = terryClient.GetStringAsync(localClientBaseUrl + "/acl/api/snapshot").GetAwaiter().GetResult()

    if productionSql then
        tryLoginLocalClient sysAdminLoginName ("wrong-" + sysAdminPassword)
        |> statusIs 401 "wrong password login"

        tryLoginLocalClient disabledLoginName disabledPassword
        |> statusIs 401 "disabled user login"

    let actorDynamicCreateShapeVisible =
        hub.ListClientExtensions()
        |> List.collect _.AppendPageShapes
        |> List.exists (fun shape -> String.Equals(shape.Shape, "actor-dynamic", StringComparison.OrdinalIgnoreCase))

    require (healthText.Contains("PulseTrade.Comm.Spa")) "healthz should identify PTCS."
    require (localLoginReply.Contains("user.", StringComparison.OrdinalIgnoreCase)) "local PTCS.Login sys-admin login should identify a user id."
    require (terryLoginReply.Contains("user.terry-hater", StringComparison.OrdinalIgnoreCase)) "local PTCS.Login Terry login should identify user.terry-hater."
    require (aclSnapshotJson.Contains("ptcs.page.create", StringComparison.Ordinal)) "ACL snapshot should expose PTCS capability keys."
    require (aclGlobalCapabilityAllowed aclSnapshotJson PtcsAcl.actionPageCreate) "WZ/sys-admin ACL snapshot should allow page create."
    require (aclCapabilityAllowed aclSnapshotJson "DamnWZ" PtcsAcl.actionTargetAdd) "WZ/sys-admin should be able to add target on DamnWZ."
    require (not (aclGlobalCapabilityAllowed terryAclSnapshotJson PtcsAcl.actionPageCreate)) "Terry黑粉 ACL snapshot should deny page create."
    require (aclCapabilityAllowed terryAclSnapshotJson "AssTerry" PtcsAcl.actionTargetRemove) "Terry黑粉 should be able to remove target on AssTerry."
    require (aclCapabilityAllowed terryAclSnapshotJson "AssTerry" PtcsAcl.actionActorArguSend) "Terry黑粉 should be able to send ActorArgu on AssTerry."
    require (not (aclCapabilityAllowed terryAclSnapshotJson "AssTerry" PtcsAcl.actionTargetAdd)) "Terry黑粉 should not be able to add target on AssTerry."
    require (not (aclCapabilityAllowed terryAclSnapshotJson "DamnWZ" PtcsAcl.actionActorArguSend)) "Terry黑粉 should not be able to send ActorArgu on DamnWZ."
    require (journalHealthText.Contains("sql-server")) "journal health should use sql-server profile."
    require (healthText.Contains("pcsl-actor-proxy")) "healthz hub persistence should expose pcsl-actor-proxy."
    require (chatHtml.Length > 0) "chat page should be served by PTCS."
    require (chatHtml.Contains(PtcsAclExtension.extensionId, StringComparison.Ordinal)) "chat page should include PTCS.ACL extension manifest."
    require (chatHtml.Contains(PtcsLoginExtension.extensionId, StringComparison.Ordinal)) "chat page should include PTCS.Login extension manifest."
    require (chatHtml.Contains(PtcsAclExtension.scriptUrl, StringComparison.Ordinal)) "chat page should load PTCS.ACL extension script URL."
    require (chatHtml.Contains(PtcsLoginExtension.scriptUrl, StringComparison.Ordinal)) "chat page should load PTCS.Login extension script URL."
    require (aclExtensionJs.Contains("PulseTrade.Comm.Spa.ACL", StringComparison.Ordinal)) "PTCS.ACL extension script asset should be served by the PTCS host."
    require (loginExtensionJs.Contains("PulseTrade.Comm.Spa.Login", StringComparison.Ordinal)) "PTCS.Login extension script asset should be served by the PTCS host."
    require (not actorDynamicCreateShapeVisible) "journal POC must not expose +page Actor Dynamic shape in the extension manifest."
    require (not (chatHtml.Contains("option value=\"actor-dynamic\""))) "journal POC must not expose +page Actor Dynamic shape."
    require (actorsHtml.Length > 0) "actors page should be served."
    require (actorsSnapshotJson.Contains(actorName, StringComparison.Ordinal)) "actors snapshot should include the ActorArgu proxy actor."
    require (actorsSnapshotJson.Contains(pfcfProtoTypingProxyActorName, StringComparison.Ordinal)) "actors snapshot should include the PFCF ActorArgu proxy actor."
    require
        (not (String.Equals(actorAddress, pfcfProxyActorAddress, StringComparison.Ordinal)))
        "echo target and PFCF target should use different per-target proxy actors."
    require
        (String.Equals(echoTargetKeys.Head, actorAddress, StringComparison.Ordinal)
         && String.Equals(pingPongTargetKeys.Head, pingPongProxyActorAddress, StringComparison.Ordinal)
         && String.Equals(pfcfProtoTypingTargetKeys.Head, pfcfProxyActorAddress, StringComparison.Ordinal))
        "target key[0] should remain the PTCS route actor and should point to the per-target proxy actors."
    require (actorsSnapshotJson.Contains(pingPongProxyActorName, StringComparison.Ordinal)) "actors snapshot should include the PingPong proxy actor."
    if localNativeStringRef.IsSome then
        require (actorsSnapshotJson.Contains(nativeStringActorName, StringComparison.Ordinal)) "actors snapshot should include the local native string actor behind the proxy."
    let actorsNodeCount, actorsActorCount = readActorsSnapshotCounts actorsSnapshotJson
    let actorsNodeCountWithOffline, actorsActorCountWithOffline = readActorsSnapshotCounts actorsSnapshotWithOfflineJson
    let hubActorCount = (hub.ActorsSnapshot()).ActorCount
    let mutable afterStopActorsNodeCount = -1
    let mutable afterStopActorsActorCount = -1
    let mutable afterStopIncludeOfflineActorCount = -1
    let mutable echoReuseAfterStopVerified = false
    let mutable aclHttpDifferenceVerified = false

    require
        (String.Equals(ensuredEcho.Path.ToStringWithoutAddress(), echoRef.Path.ToStringWithoutAddress(), StringComparison.Ordinal))
        "ensureEchoActorRegistered should reuse the existing proxy actor instead of trying to spawn a duplicate actor name."

    require (hubActorCount > 0) $"hub actor projection should have actors, got {hubActorCount}."
    require (dynamicJs.Contains("dynamic-actors-page")) "Dynamic bundle should include ActorsPage renderer."
    require (dynamicJs.Contains("dynamic-argu-add-key")) "Dynamic bundle should include Add target key renderer."
    verifyPfcfProtoTypingTemplate ()

    match serverProbe with
    | Some probe ->
        require probe.ActorArgu.IsSome "server probe should append an ActorArgu reply."
        let probePayload = probe.ActorArgu.Value.Event.Payload
        require
            (probePayload.Contains("native-string-actor fcell2 reply raw=", StringComparison.Ordinal))
            ("server probe should be replied by the native string actor through proxy; payload=" + probePayload)
        let deliveryStatusText = string probe.DeliveryStatus.Status
        require
            (String.Equals(deliveryStatusText, "completed", StringComparison.OrdinalIgnoreCase))
            ("server probe delivery should complete: " + deliveryStatusText)
    | None when not shouldRunStartupProbe ->
        printfn "Startup server probe skipped in live-host mode. Use sendEchoProbe(), sendPfcfProbe(), --startup-probe, or --no-wait to run a send proof."
    | None ->
        printfn "Default target probe skipped because the journal projection currently has no visible default target."

    if noWait then
        let tempTargetKeys =
            [ pingPongProxyActorAddress; "target-v1"; pingPongActorAddress; templateKey; "--say \"wz temp\" --set-count 1 --mode fast --tag acl-temp" ]

        postJson client "/pages/api/add-key" (addKeyJson assTerryPage.PageId tempTargetKeys "WZ temp target")
        |> statusIs 200 "WZ/sys-admin add temp target on AssTerry"

        let tempExplicitProxyKeys =
            [ pingPongProxyActorAddress; "target-v1"; pingPongActorAddress; templateKey; "--say \"wz temp explicit proxy\" --set-count 1 --mode fast --tag acl-temp-proxy" ]

        postJson client "/pages/api/add-key" (addKeyJson assTerryPage.PageId tempExplicitProxyKeys "WZ temp explicit proxy target")
        |> statusIs 200 "WZ/sys-admin add explicit proxy target on AssTerry"

        let tempExplicitProxyKey =
            hub.ListAppendPageKeys(assTerryPage.PageId).Keys
            |> List.tryFind (fun key -> key.DisplayName = "WZ temp explicit proxy target")
            |> Option.defaultWith (fun () -> invalidOp "WZ temp explicit proxy target key was not projected.")

        require
            (String.Equals(tempExplicitProxyKey.Keys.Head, pingPongProxyActorAddress, StringComparison.Ordinal))
            "explicit target key should persist the user-provided proxy actor as key[0]."
        require
            (tempExplicitProxyKey.Keys.Length >= 3 && String.Equals(tempExplicitProxyKey.Keys[2], pingPongActorAddress, StringComparison.Ordinal))
            "explicit target key should persist the native PingPong actor as key[2]."

        let tempExplicitProxySend =
            postJson
                client
                "/pages/api/actor-argu/send"
                (actorArguJson assTerryPage.PageId tempExplicitProxyKey.Keys "--say \"wz temp explicit proxy\" --set-count 1 --mode fast --tag acl-temp-proxy")

        tempExplicitProxySend |> statusIs 200 "WZ/sys-admin ActorArgu send through explicit temp proxy target"
        require
            (containsFCellTToSMarker (snd tempExplicitProxySend)
             && (snd tempExplicitProxySend).Contains("native-pingpong-fcell2-t", StringComparison.Ordinal))
            ("explicit temp proxy target should reply with script-converted native PingPong fCell2.T->S marker; body=" + snd tempExplicitProxySend)

        postJson terryClient "/pages/api/add-key" (addKeyJson assTerryPage.PageId [ actorAddress; "target-v1"; nativeStringActorAddress; templateKey; "--say \"denied\"" ] "Terry forbidden target")
        |> statusIs 403 "Terry add target denied on AssTerry"

        postJson terryClient "/pages/api/actor-argu/send" (actorArguJson assTerryPage.PageId echoTargetKeys "--say \"terry allowed\" --set-count 1 --mode fast")
        |> statusIs 200 "Terry ActorArgu send allowed on AssTerry"

        let pingPongProbe = sendPingPongProbe ()
        require pingPongProbe.ActorArgu.IsSome "PingPong proxy probe should append an ActorArgu reply."
        let pingPongPayload = pingPongProbe.ActorArgu.Value.Event.Payload
        require
            (containsFCellTToSMarker pingPongPayload
             && pingPongPayload.Contains("native-pingpong-fcell2-t", StringComparison.Ordinal))
            ("PingPong proxy probe should be replied by native fCell2.T and converted by script handler to fCell2.S; payload=" + pingPongPayload)

        postJson terryClient "/pages/api/actor-argu/send" (actorArguJson damnWzPage.PageId echoTargetKeys "--say \"terry denied\" --set-count 1 --mode fast")
        |> statusIs 403 "Terry ActorArgu send denied on DamnWZ"

        let tempKey =
            hub.ListAppendPageKeys(assTerryPage.PageId).Keys
            |> List.tryFind (fun key -> key.Keys = tempTargetKeys)
            |> Option.defaultWith (fun () -> invalidOp "WZ temp target key was not projected for Terry remove proof.")

        postJson terryClient "/pages/api/remove-key" (removeKeyJson assTerryPage.PageId tempKey.KeyId)
        |> statusIs 200 "Terry remove target allowed on AssTerry"

        aclHttpDifferenceVerified <- true
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
            (not (jsonContainsExactActorPathName pingPongActorName afterStopActorsSnapshotJson))
            $"active actors snapshot should not include stopped PingPong actor {pingPongActorName}."

        require
            (not (jsonContainsExactActorPathName pingPongActorName afterStopActorsTreeJson))
            $"active ActorTopology tree should not include stopped PingPong actor {pingPongActorName}."

        require
            (jsonContainsExactActorPathName pingPongActorName afterStopActorsSnapshotWithOfflineJson)
            $"includeOffline actors snapshot should retain stopped PingPong actor {pingPongActorName} for diagnostics."

        require
            (afterStopActorsSnapshotWithOfflineJson.Contains("terminated", StringComparison.OrdinalIgnoreCase))
            "includeOffline actors snapshot should expose the stopped actor terminated status."

        echoReuseAfterStopVerified <- verifyEchoActorReuseAfterStop ()

    printfn "PTCS Dynamic POC Full NuGet Journal ACL started."
    printfn "Local login URL    %s/login?returnUrl=/actors" localClientBaseUrl
    printfn "Local chat URL     %s/chat" localClientBaseUrl
    printfn "Local actors URL   %s/actors" localClientBaseUrl
    printfn "Mode              %s" (if productionSql then "production-sql" else "demo")
    printfn "Ports             local=%d cluster=%s:%d native-pingpong=%s:%d dynamic=%b startupProbe=%b" localPort clusterHost clusterPort nativeNodeHost nativeNodePort ifDynaPort startupProbe
    printfn "Local users       WZ/sys-admin=%s / Terry黑粉=%s / legacy-admin=%s; passwords are %s." sysAdminLoginName terryLoginName adminLoginName (if productionSql then "SQL-seeded for this POC" else "demo-only and defined by LoginConfig.demo()")
    printfn "ACL proof         wzGlobalCreate=%b terryAddAssTerry=%b terryRemoveAssTerry=%b terrySendAssTerry=%b terrySendDamnWZ=%b httpDifference=%b" (aclGlobalCapabilityAllowed aclSnapshotJson PtcsAcl.actionPageCreate) (aclCapabilityAllowed terryAclSnapshotJson "AssTerry" PtcsAcl.actionTargetAdd) (aclCapabilityAllowed terryAclSnapshotJson "AssTerry" PtcsAcl.actionTargetRemove) (aclCapabilityAllowed terryAclSnapshotJson "AssTerry" PtcsAcl.actionActorArguSend) (aclCapabilityAllowed terryAclSnapshotJson "DamnWZ" PtcsAcl.actionActorArguSend) aclHttpDifferenceVerified
    printfn "Actors data   visibleNodes=%d visibleActors=%d includeOfflineNodes=%d includeOfflineActors=%d hubActors=%d" actorsNodeCount actorsActorCount actorsNodeCountWithOffline actorsActorCountWithOffline hubActorCount
    if noWait then
        printfn "After stop    visibleNodes=%d visibleActors=%d includeOfflineActors=%d pingPongFiltered=true" afterStopActorsNodeCount afterStopActorsActorCount afterStopIncludeOfflineActorCount
        printfn "Proxy reuse   reuseAfterStop=%b" echoReuseAfterStopVerified
    printfn "ActorArgu URLs %s/page/%s ; %s/page/%s" localClientBaseUrl damnWzPage.PageId localClientBaseUrl assTerryPage.PageId
    printfn "Dynamic JS    %s/ext/js/PulseTrade.Comm.Spa.Dynamic.js" localClientBaseUrl
    printfn "PCSL root     %s" pcslRoot
    printfn "SQL journal   db=%s created=%b existed=%b" journalBootstrap.DatabaseName journalBootstrap.Created journalBootstrap.AlreadyExisted
    printfn "Persistence   namespace=%s prefix=%s" persistenceNamespace.PersistenceNamespace persistenceNamespace.PersistenceIdPrefix
    printfn "Projection    backend=pcsl-actor-proxy clearBeforeStart=%b seededDefaultPage=%b" clearPcslBeforeStart seededDefaultPage
    printfn "Echo proxy actor %s -> %s" actorAddress nativeStringActorAddress
    printfn "PFCF proxy actor %s -> %s" pfcfProxyActorAddress nativeStringActorAddress
    printfn "PingPong native node %s" nativePingPongNodeAddress
    printfn "PingPong proxy actor %s -> %s" pingPongProxyActorAddress pingPongActorAddress
    printfn "Native actor     %s (%s)" nativeStringActorAddress (if localNativeStringRef.IsSome then "local-demo" else "external")
    printfn "PingPong actor %s" pingPongActorAddress
    printfn "Template key  %s" templateKey
    printfn "PFCF template key %s" pfcfProtoTypingTemplateKey
    printfn "PFCF type name    %s" typeof<PFCF_AKKA_CMD_FOR_ProtoTyping>.FullName
    printfn "Proxy target key    %s" (JsonSerializer.Serialize(echoTargetKeys |> List.toArray))
    printfn "PingPong target key %s" (JsonSerializer.Serialize(pingPongTargetKeys |> List.toArray))
    printfn "PFCF target key     %s" (JsonSerializer.Serialize(pfcfProtoTypingTargetKeys |> List.toArray))
    printfn "Default arg   %s" defaultCanonicalArgString
    printfn "PFCF arg      %s" pfcfProtoTypingCanonicalArgString
    printfn "Proxy helpers ensureEchoActorRegistered(); stopEchoActor(); recreateEchoActor()"
    printfn "Probe helpers sendEchoProbe(); sendRawEchoProbe \"--say \\\"text\\\" --set-count 1 --mode fast\"; sendPingPongProbe(); sendPfcfProbe()"
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
