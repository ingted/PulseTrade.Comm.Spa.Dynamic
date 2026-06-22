#i @"nuget: C:\Program Files\dotnet\sdk\10.0.300\FSharp\library-packs"
#r "nuget: PulseTrade.Comm.Spa, 0.2.5-beta9"
#r "nuget: FAkka.Argu, 10.1.301"
#r "nuget: FAkka.FCell2, 10.1.301"
#r "nuget: Akka, 1.5.69"
#I __SOURCE_DIRECTORY__
#I "bin/Release/net10.0"
#r "PulseTrade.Comm.Spa.Dynamic.dll"

#load @"C:\Users\Administrator\.codex\lib\ParseLine.fsx"

open System
open System.IO
open System.Net.Http
open Argu
open PulseTrade.Comm.Spa
open PulseTrade.Comm.Spa.Dynamic.Server

// Demo 目標：
// - 使用 PulseTrade.Comm.Spa 0.2.4-beta8 NuGet package，而不是 source project reference。
// - 驗證 Dynamic consumer 可以在 PTCS server start 之後，透過 CommHub 註冊 client extension manifest。
// - 驗證 actor-dynamic custom shape 可透過 /pages/api/register-page round-trip。
// - 這是 upstream API smoke，不宣稱 Dynamic browser bundle 已完成正式 renderer integration。
// - 依專案規範，本檔禁止手寫 JavaScript；正式 browser extension 應由 WebSharper/F# 產生 client bundle。

let defaultPcslRoot =
    Path.Combine(__SOURCE_DIRECTORY__, ".pcsl", "poc.dynamic.beta8.extension-smoke")

let defaultArgPath (path: string) =
    if String.IsNullOrWhiteSpace path then "" else path.Replace('\\', '/')

let defaultArgumentsText =
    $"""--host 127.0.0.1 --port 0 --pcsl-root "{defaultArgPath defaultPcslRoot}" --no-wait"""

type CliArguments =
    | Host of host: string
    | Port of port: int
    | Pcsl_Root of path: string
    | No_Wait
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Host _ -> "Local HTTP bind host."
            | Port _ -> "Local HTTP port. Use 0 for a random free port."
            | Pcsl_Root _ -> "Root directory for PCSL files."
            | No_Wait -> "Run smoke checks and stop immediately."

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
    let parser = ArgumentParser.Create<CliArguments>(programName = "poc.dynamic.beta8.extension-smoke.fsx")
    let args = fsiArgs ()
    let effectiveArgs = if args.Length = 0 then defaultArgs () else args
    parser.ParseCommandLine effectiveArgs

let textOr fallback (value: string) =
    if String.IsNullOrWhiteSpace value then fallback else value.Trim()

let fullPath (path: string) =
    Path.GetFullPath(textOr defaultPcslRoot path)

let require condition message =
    if not condition then
        failwithf "poc.dynamic.beta8.extension-smoke failed: %s" message

let parsed = parseArguments ()

let host =
    parsed.TryGetResult(<@ Host @>)
    |> Option.defaultValue "127.0.0.1"
    |> textOr "127.0.0.1"

let port =
    parsed.TryGetResult(<@ Port @>)
    |> Option.defaultValue 0

let pcslRoot =
    parsed.TryGetResult(<@ Pcsl_Root @>)
    |> Option.defaultValue defaultPcslRoot
    |> fullPath

let noWait = parsed.Contains <@ No_Wait @>

Directory.CreateDirectory pcslRoot |> ignore

let hub = CommHub.createEmptyWithPcslRoot pcslRoot

let options =
    let fabricOptions =
        { CommSpaActorFabricOptions.defaults with
            SystemName = "PulseTradeCommSpaDynamicBeta8Smoke"
            ShardTypeName = "comm-spa-dynamic-beta8-smoke"
            ClusterPort = 0 }

    ServerOptions.minimalWithHub hub
    |> ServerOptions.withWebBinding (if port <= 0 then WebBinding.randomHost host else WebBinding.fixedHostPort host port)
    |> Server.withActorFabricOptions fabricOptions

let app = Server.start options

try
    // 現有 Dynamic extension method 仍只處理 server actor；這裡保留呼叫，確認與 beta8 core 相容。
    // 後續 Dynamic 正式 RFC 會把下方 manifest registration 與 WebSharper/F# 產生的 client bundle 註冊封裝進 useDynamicSdui。
    let fabric =
        app.ActorFabric
        |> Option.defaultWith (fun () -> failwith "Expected ActorFabric for Dynamic smoke.")

    hub.useDynamicSdui(fabric.System) |> ignore

    let registeredExtension =
        hub.ListClientExtensions()
        |> List.tryFind (fun extension -> extension.ExtensionId = "pulse-trade-comm-spa-dynamic")
        |> Option.defaultWith (fun () -> failwith "Expected useDynamicSdui to register pulse-trade-comm-spa-dynamic manifest.")

    require (registeredExtension.AppendPageShapes |> List.exists (fun shape -> shape.Shape = "actor-dynamic")) "actor-dynamic manifest shape should be registered."

    use http = new HttpClient()
    let html = http.GetStringAsync(app.Url + "/chat").GetAwaiter().GetResult()
    require (html.Contains("ptc-comm-client-extensions")) "chat shell should inject client extension manifest."
    require (html.Contains("actor-dynamic")) "chat shell manifest should contain actor-dynamic."

    let requestJson =
        Json.serialize
            {| pageId = ""
               title = "Dynamic Smoke"
               setName = ""
               shape = "actor-dynamic"
               tabId = ""
               tabMode = ""
               path = ""
               description = "" |}

    use requestBody = new StringContent(requestJson, Text.Encoding.UTF8, "application/json")
    let response = http.PostAsync(app.Url + "/pages/api/register-page", requestBody).GetAwaiter().GetResult()
    let body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult()

    require response.IsSuccessStatusCode $"register-page actor-dynamic should succeed. status={response.StatusCode} body={body}"
    require (body.Contains("\"shape\":\"actor-dynamic\"")) $"actor-dynamic should round-trip. body={body}"

    printfn "dynamicBeta8ExtensionSmoke.ok url=%s manifestOnly=true pcsl=%s noWait=%b" app.Url pcslRoot noWait
finally
    (app :> IDisposable).Dispose()
