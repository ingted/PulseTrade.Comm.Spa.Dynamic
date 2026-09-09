// Real-browser IndexedDB cache verifier. Start BrowserCacheDemo before running this client.

#i @"nuget: C:\Program Files\dotnet\sdk\10.0.401\FSharp\library-packs"
#r "nuget: FAkka.Argu, [10.1.301]"
#r "nuget: Microsoft.Playwright, 1.52.0"

#load "ParseLine.fsx"

open System
open System.IO
open System.Threading.Tasks
open Argu
open Microsoft.Playwright

type CliArgs =
    | Url of string
    | Output_Dir of string
    | Browser_Executable_Path of string
    | Headed
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Url _ -> "Existing browser cache demo URL."
            | Output_Dir _ -> "Directory for deterministic browser evidence."
            | Browser_Executable_Path _ -> "Chrome or Edge executable path."
            | Headed -> "Run the browser headed."

let knownBrowserPaths =
    [ @"C:\Program Files\Google\Chrome\Application\chrome.exe"
      @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe"
      @"C:\Program Files\Microsoft\Edge\Application\msedge.exe"
      @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe" ]

let defaultBrowserPath = knownBrowserPaths |> List.tryFind File.Exists |> Option.defaultValue ""

let defaultArgumentsText =
    sprintf
        "--url \"http://127.0.0.1:18885/\" --output-dir \"artifacts/interactive-client-browser-cache-playwright\" --browser-executable-path \"%s\""
        (defaultBrowserPath.Replace('\\', '/'))

let parser = ArgumentParser.Create<CliArgs>(programName = "verify-interactive-client-browser-cache-playwright.fsx")
let parse text = parser.Parse(PL.parseLine [| ' ' |] (Some '"') None true text, raiseOnUsage = true)
let defaults = parse defaultArgumentsText
let automationArgs = fsi.CommandLineArgs |> Array.skip 1 |> Array.skipWhile ((=) "--")
let automation = parser.Parse automationArgs

let pick tryAutomation tryDefault fallback =
    tryAutomation () |> Option.orElseWith tryDefault |> Option.defaultValue fallback

let url = pick (fun () -> automation.TryGetResult(<@ Url @>)) (fun () -> defaults.TryGetResult(<@ Url @>)) ""
let outputDirectory =
    pick (fun () -> automation.TryGetResult(<@ Output_Dir @>)) (fun () -> defaults.TryGetResult(<@ Output_Dir @>)) "artifacts/interactive-client-browser-cache-playwright"
    |> Path.GetFullPath
let browserExecutablePath = pick (fun () -> automation.TryGetResult(<@ Browser_Executable_Path @>)) (fun () -> defaults.TryGetResult(<@ Browser_Executable_Path @>)) defaultBrowserPath
let headed = automation.Contains Headed || defaults.Contains Headed

let awaitTask (task: Task<'T>) = task.GetAwaiter().GetResult()
let awaitUnit (task: Task) = task.GetAwaiter().GetResult()
let require condition message = if not condition then failwith ("Browser cache verification failed: " + message)

let waitStatus (page: IPage) (expected: string) =
    try
        page.GetByTestId("cache-status").GetByText(expected, LocatorGetByTextOptions(Exact = true)).WaitForAsync(LocatorWaitForOptions(Timeout = 10000.0f))
        |> awaitUnit
    with error ->
        let actual = page.GetByTestId("cache-status").TextContentAsync() |> awaitTask
        failwith $"Expected cache status `{expected}`, actual `{actual}`: {error.Message}"

Directory.CreateDirectory outputDirectory |> ignore
require (not (String.IsNullOrWhiteSpace browserExecutablePath) && File.Exists browserExecutablePath) "Chrome or Edge executable is unavailable"

let playwright = Playwright.CreateAsync() |> awaitTask
let browser = playwright.Chromium.LaunchAsync(BrowserTypeLaunchOptions(Headless = not headed, ExecutablePath = browserExecutablePath)) |> awaitTask
let context = browser.NewContextAsync(BrowserNewContextOptions(ViewportSize = ViewportSize(Width = 1280, Height = 720))) |> awaitTask
let page = context.NewPageAsync() |> awaitTask
let errors = ResizeArray<string>()
page.Console.Add(fun message -> if message.Type = "error" then errors.Add message.Text)
page.PageError.Add(fun error -> errors.Add error)

page.GotoAsync(url, PageGotoOptions(WaitUntil = WaitUntilState.NetworkIdle)) |> awaitTask |> ignore
page.GetByTestId("cache-seed").ClickAsync() |> awaitUnit
waitStatus page "SEEDED:8"
page.ReloadAsync(PageReloadOptions(WaitUntil = WaitUntilState.NetworkIdle)) |> awaitTask |> ignore
page.GetByTestId("cache-count").ClickAsync() |> awaitUnit
waitStatus page "COUNT:8"
page.GetByTestId("cache-hit").ClickAsync() |> awaitUnit
waitStatus page "LATEST:HIT:10"
page.GetByTestId("cache-evicted").ClickAsync() |> awaitUnit
waitStatus page "OLDEST:MISS"
page.GetByTestId("cache-covering-hit").ClickAsync() |> awaitUnit
waitStatus page "COVERING:HIT:10"
page.GetByTestId("cache-coverage-miss").ClickAsync() |> awaitUnit
waitStatus page "COVERING:MISS"
page.ScreenshotAsync(PageScreenshotOptions(Path = Path.Combine(outputDirectory, "browser-cache-persisted.png"), FullPage = true)) |> awaitTask |> ignore
page.GetByTestId("cache-clear").ClickAsync() |> awaitUnit
waitStatus page "CLEARED"
page.GetByTestId("cache-count").ClickAsync() |> awaitUnit
waitStatus page "COUNT:0"
page.GetByTestId("cache-seed-corrupt").ClickAsync() |> awaitUnit
waitStatus page "CORRUPT:SEEDED"
page.GetByTestId("cache-read-corrupt").ClickAsync() |> awaitUnit
waitStatus page "CORRUPT:MISS"
page.GetByTestId("cache-count").ClickAsync() |> awaitUnit
waitStatus page "COUNT:0"
page.GetByTestId("cache-seed-semantic-invalid").ClickAsync() |> awaitUnit
waitStatus page "SEMANTIC-INVALID:SEEDED"
page.GetByTestId("cache-read-semantic-invalid").ClickAsync() |> awaitUnit
waitStatus page "SEMANTIC-INVALID:MISS"
page.GetByTestId("cache-count").ClickAsync() |> awaitUnit
waitStatus page "COUNT:0"
page.GetByTestId("cache-write-accepted").ClickAsync() |> awaitUnit
waitStatus page "ACCEPTED:WRITTEN"
page.GetByTestId("cache-count").ClickAsync() |> awaitUnit
waitStatus page "COUNT:1"
page.GetByTestId("cache-read-accepted-projection").ClickAsync() |> awaitUnit
waitStatus page "ACCEPTED-PROJECTION:1:0"
page.GetByTestId("cache-rehydrate").ClickAsync() |> awaitUnit
waitStatus page "REHYDRATED:0:19:PAUSED"
page.GetByTestId("cache-reject-paused").ClickAsync() |> awaitUnit
waitStatus page "PAUSED:REJECTED:runtime-state-not-cacheable:cache.runtimeState"
page.GetByTestId("cache-count").ClickAsync() |> awaitUnit
waitStatus page "COUNT:1"
page.GetByTestId("cache-clear").ClickAsync() |> awaitUnit
waitStatus page "CLEARED"
page.GetByTestId("cache-count").ClickAsync() |> awaitUnit
waitStatus page "COUNT:0"
require (errors.Count = 0) ("browser console/page errors: " + String.concat " | " errors)

context.CloseAsync() |> awaitUnit
browser.CloseAsync() |> awaitUnit
playwright.Dispose()

printfn "PASS interactive-client-browser-cache-playwright"
printfn "persisted-count=8 latest-revision=10 evicted-oldest=true covering-hit=true coverage-miss=true corrupt-removed=true semantic-invalid-removed=true finalized-prefix=1 preview-count=0 accepted-state-write=true paused-state-rejected=true rehydrate=paused revision-continuation=false cleared-count=0"
printfn "evidence=%s" (Path.Combine(outputDirectory, "browser-cache-persisted.png"))
