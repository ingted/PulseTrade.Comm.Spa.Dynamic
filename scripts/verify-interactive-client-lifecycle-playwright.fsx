// Real-browser lifecycle verifier. Start the dedicated LiveDemo host before running this client.

#i @"nuget: C:\Program Files\dotnet\sdk\10.0.400\FSharp\library-packs"
#r "nuget: FAkka.Argu, [10.1.301]"
#r "nuget: Microsoft.Playwright, 1.52.0"

#load "ParseLine.fsx"

open System
open System.IO
open System.Net.Http
open System.Text.Json
open System.Threading
open System.Threading.Tasks
open Argu
open Microsoft.Playwright

type CliArgs =
    | Url of string
    | Control_Base_Url of string
    | Output_Dir of string
    | Browser_Executable_Path of string
    | Headed
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Url _ -> "Existing Interactive.Client lifecycle demo page URL."
            | Control_Base_Url _ -> "Existing lifecycle fixture control base URL."
            | Output_Dir _ -> "Directory for deterministic browser evidence."
            | Browser_Executable_Path _ -> "Chrome or Edge executable path."
            | Headed -> "Run the browser headed."

let knownBrowserPaths =
    [ @"C:\Program Files\Google\Chrome\Application\chrome.exe"
      @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe"
      @"C:\Program Files\Microsoft\Edge\Application\msedge.exe"
      @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe" ]

let defaultBrowserPath =
    knownBrowserPaths |> List.tryFind File.Exists |> Option.defaultValue ""

let defaultArgumentsText =
    sprintf
        "--url \"http://127.0.0.1:18884/session/demo/view/canvas\" --control-base-url \"http://127.0.0.1:18884\" --output-dir \"artifacts/interactive-client-lifecycle-playwright\" --browser-executable-path \"%s\""
        (defaultBrowserPath.Replace('\\', '/'))

let parser = ArgumentParser.Create<CliArgs>(programName = "verify-interactive-client-lifecycle-playwright.fsx")
let parse text = parser.Parse(PL.parseLine [| ' ' |] (Some '"') None true text, raiseOnUsage = true)
let defaults = parse defaultArgumentsText
let automationArgs = fsi.CommandLineArgs |> Array.skip 1 |> Array.skipWhile ((=) "--")
let automation = parser.Parse automationArgs

let pick tryAutomation tryDefault fallback =
    tryAutomation ()
    |> Option.orElseWith tryDefault
    |> Option.defaultValue fallback

let url = pick (fun () -> automation.TryGetResult(<@ Url @>)) (fun () -> defaults.TryGetResult(<@ Url @>)) ""
let controlBaseUrl = pick (fun () -> automation.TryGetResult(<@ Control_Base_Url @>)) (fun () -> defaults.TryGetResult(<@ Control_Base_Url @>)) ""
let outputDirectory =
    pick (fun () -> automation.TryGetResult(<@ Output_Dir @>)) (fun () -> defaults.TryGetResult(<@ Output_Dir @>)) "artifacts/interactive-client-lifecycle-playwright"
    |> Path.GetFullPath
let browserExecutablePath = pick (fun () -> automation.TryGetResult(<@ Browser_Executable_Path @>)) (fun () -> defaults.TryGetResult(<@ Browser_Executable_Path @>)) defaultBrowserPath
let headed = automation.Contains Headed || defaults.Contains Headed

let awaitTask (task: Task<'T>) = task.GetAwaiter().GetResult()
let awaitUnit (task: Task) = task.GetAwaiter().GetResult()

let require condition message =
    if not condition then failwith ("Interactive.Client lifecycle verification failed: " + message)

let requireBodyContains (page: IPage) (expected: string) =
    let body = page.Locator("body").InnerTextAsync() |> awaitTask
    require (body.Contains expected) $"expected `{expected}` in browser body"

type FixtureState =
    { ConnectionCount: int
      ActiveConnections: int
      MaximumActiveConnections: int
      MountedCount: int
      FullSnapshotRequestCount: int
      UnmountedCount: int
      FrameCount: int }

let http = new HttpClient()

let readState () =
    let json = http.GetStringAsync(controlBaseUrl + "/test/state") |> awaitTask
    use document = JsonDocument.Parse json
    let root = document.RootElement
    let integer (name: string) = root.GetProperty(name).GetInt32()
    { ConnectionCount = integer "connectionCount"
      ActiveConnections = integer "activeConnections"
      MaximumActiveConnections = integer "maximumActiveConnections"
      MountedCount = integer "mountedCount"
      FullSnapshotRequestCount = integer "fullSnapshotRequestCount"
      UnmountedCount = integer "unmountedCount"
      FrameCount = integer "frameCount" }

let waitForState timeout predicate description =
    let deadline = DateTime.UtcNow.Add timeout
    let mutable state = readState ()

    while not (predicate state) && DateTime.UtcNow < deadline do
        Thread.Sleep 25
        state <- readState ()

    require (predicate state) $"timed out waiting for {description}; state={state}"
    state

Directory.CreateDirectory outputDirectory |> ignore
require (not (String.IsNullOrWhiteSpace browserExecutablePath) && File.Exists browserExecutablePath) "Chrome or Edge executable is unavailable"

let playwright = Playwright.CreateAsync() |> awaitTask
let launchOptions = BrowserTypeLaunchOptions(Headless = not headed, ExecutablePath = browserExecutablePath)
let browser = playwright.Chromium.LaunchAsync(launchOptions) |> awaitTask
let context = browser.NewContextAsync(BrowserNewContextOptions(ViewportSize = ViewportSize(Width = 1440, Height = 900))) |> awaitTask
let page = context.NewPageAsync() |> awaitTask
let consoleErrors = ResizeArray<string>()
page.Console.Add(fun message -> if message.Type = "error" then consoleErrors.Add message.Text)
page.PageError.Add(fun error -> consoleErrors.Add error)

page.GotoAsync(url, PageGotoOptions(WaitUntil = WaitUntilState.NetworkIdle)) |> awaitTask |> ignore
page.Locator("[data-testid='ta-workspace']").WaitForAsync(LocatorWaitForOptions(Timeout = 10000.0f)) |> awaitUnit
requireBodyContains page "Interactive lifecycle workspace"
requireBodyContains page "LAST GOOD V1"

let initial = waitForState (TimeSpan.FromSeconds 5.0) (fun state -> state.ConnectionCount = 1 && state.ActiveConnections = 1 && state.MountedCount = 1) "initial mount"
require (initial.MaximumActiveConnections = 1) $"initial transport overlap: {initial}"

let dropResponse = http.PostAsync(controlBaseUrl + "/test/drop", new StringContent("")) |> awaitTask
require (int dropResponse.StatusCode = 202) $"fixture drop returned HTTP {int dropResponse.StatusCode}"

let synchronizing =
    waitForState
        (TimeSpan.FromSeconds 5.0)
        (fun state -> state.ConnectionCount = 2 && state.FullSnapshotRequestCount = 1)
        "replacement transport full-snapshot request"

requireBodyContains page "LAST GOOD V1"
require (synchronizing.ActiveConnections = 1) $"replacement transport should be singular: {synchronizing}"
require (synchronizing.MaximumActiveConnections = 1) $"old and replacement transports overlapped: {synchronizing}"

page.GetByText("RESYNCED V2", PageGetByTextOptions(Exact = true)).WaitForAsync(LocatorWaitForOptions(Timeout = 10000.0f)) |> awaitUnit
let resynced = waitForState (TimeSpan.FromSeconds 5.0) (fun state -> state.FrameCount = 3 && state.MountedCount = 2) "authoritative replacement snapshot"
require (resynced.FullSnapshotRequestCount = 1) $"reconnect emitted duplicate full-snapshot requests: {resynced}"

page.ScreenshotAsync(PageScreenshotOptions(Path = Path.Combine(outputDirectory, "interactive-client-resynced.png"), FullPage = true)) |> awaitTask |> ignore
page.GotoAsync("about:blank") |> awaitTask |> ignore
Thread.Sleep 2200

let disposed = readState ()
require (disposed.ConnectionCount = 2) $"terminal dispose unexpectedly reconnected: {disposed}"
require (disposed.ActiveConnections = 0) $"terminal dispose left an active socket: {disposed}"
require (disposed.UnmountedCount = 1) $"terminal dispose should send one unmounted frame: {disposed}"
require (consoleErrors.Count = 0) ("browser console/page errors: " + String.concat " | " consoleErrors)

dropResponse.Dispose()
context.CloseAsync() |> awaitUnit
browser.CloseAsync() |> awaitUnit
playwright.Dispose()
http.Dispose()

printfn "PASS interactive-client-lifecycle-playwright"
printfn "initial=%A" initial
printfn "resynced=%A" resynced
printfn "disposed=%A" disposed
printfn "evidence=%s" (Path.Combine(outputDirectory, "interactive-client-resynced.png"))
