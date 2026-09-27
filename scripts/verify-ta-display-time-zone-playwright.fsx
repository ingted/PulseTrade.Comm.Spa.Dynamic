// Page-scoped display-time projection gate for the existing TA Renderer BrowserDemo host.

#i @"nuget: C:\Program Files\dotnet\sdk\10.0.401\FSharp\library-packs"
#r "nuget: FAkka.Argu, [10.1.301]"
#r "nuget: Microsoft.Playwright, 1.52.0"

#load "ParseLine.fsx"

open System
open System.IO
open System.Threading
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
            | Url _ -> "Existing TA renderer BrowserDemo URL."
            | Output_Dir _ -> "Directory for deterministic screenshots."
            | Browser_Executable_Path _ -> "Chrome or Edge executable path."
            | Headed -> "Run the browser headed."

let knownBrowserPaths =
    [ @"C:\Program Files\Google\Chrome\Application\chrome.exe"
      @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe"
      @"C:\Program Files\Microsoft\Edge\Application\msedge.exe"
      @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe" ]

let defaultBrowserPath = knownBrowserPaths |> List.tryFind File.Exists |> Option.defaultValue ""
let defaultArgumentsText =
    $"--url \"http://127.0.0.1:18882/\" --output-dir \"artifacts/ta-display-time-zone-playwright\" --browser-executable-path \"{defaultBrowserPath.Replace('\\', '/')}\""

let parser = ArgumentParser.Create<CliArgs>(programName = "verify-ta-display-time-zone-playwright.fsx")
let defaults = parser.Parse(PL.parseLine [| ' ' |] (Some '"') None true defaultArgumentsText)
let automation = parser.Parse(fsi.CommandLineArgs |> Array.skip 1 |> Array.skipWhile ((=) "--"))
let pick tryAutomation tryDefault fallback =
    tryAutomation () |> Option.orElseWith tryDefault |> Option.defaultValue fallback

let url = pick (fun () -> automation.TryGetResult(<@ Url @>)) (fun () -> defaults.TryGetResult(<@ Url @>)) "http://127.0.0.1:18882/"
let outputDirectory = pick (fun () -> automation.TryGetResult(<@ Output_Dir @>)) (fun () -> defaults.TryGetResult(<@ Output_Dir @>)) "artifacts/ta-display-time-zone-playwright" |> Path.GetFullPath
let browserExecutablePath = pick (fun () -> automation.TryGetResult(<@ Browser_Executable_Path @>)) (fun () -> defaults.TryGetResult(<@ Browser_Executable_Path @>)) defaultBrowserPath
let headed = automation.Contains Headed || defaults.Contains Headed

let awaitTask (task: Task<'T>) = task.GetAwaiter().GetResult()
let awaitUnit (task: Task) = task.GetAwaiter().GetResult()
let require condition message = if not condition then failwith ("TA display-time-zone Playwright verification failed: " + message)
let textOf (locator: ILocator) = locator.TextContentAsync() |> awaitTask |> Option.ofObj |> Option.defaultValue ""
let attribute (locator: ILocator) name = locator.GetAttributeAsync(name) |> awaitTask |> Option.ofObj |> Option.defaultValue ""
let waitUntil timeoutMs predicate message =
    let deadline = DateTime.UtcNow.AddMilliseconds(float timeoutMs)
    while not (predicate ()) && DateTime.UtcNow < deadline do Thread.Sleep 20
    require (predicate ()) message

require (not (String.IsNullOrWhiteSpace browserExecutablePath) && File.Exists browserExecutablePath) "Chrome or Edge executable was not found"
let playwright = Playwright.CreateAsync() |> awaitTask
let browser =
    playwright.Chromium.LaunchAsync(
        BrowserTypeLaunchOptions(
            ExecutablePath = browserExecutablePath,
            Headless = not headed,
            Args = [| "--disable-gpu" |]))
    |> awaitTask

let context = browser.NewContextAsync(BrowserNewContextOptions(ViewportSize = ViewportSize(Width = 1440, Height = 900))) |> awaitTask
let page = context.NewPageAsync() |> awaitTask
let errors = ResizeArray<string>()
page.Console.Add(fun message -> if message.Type = "error" then errors.Add message.Text)
page.PageError.Add(errors.Add)
page.GotoAsync(url, PageGotoOptions(WaitUntil = WaitUntilState.NetworkIdle)) |> awaitTask |> ignore

let workspace = page.Locator("[data-testid='ta-workspace']")
let selector = page.Locator("[data-testid='ta-demo-display-time-zone']")
let axisTick = page.Locator("[data-testid='ta-time-axis-price'] [data-canonical-event-time]").First
let rowTime = page.Locator("[data-testid='ta-row-data-time-price']")
let chartStack = page.Locator("[data-testid='ta-chart-stack']")
let callbackState = page.Locator("[data-testid='ta-demo-callback-state']")
let markerTitle = page.Locator("[data-testid='ta-marker-signals-long-entry'] title")

workspace.WaitForAsync(LocatorWaitForOptions(Timeout = 30000.0f)) |> awaitUnit
axisTick.WaitForAsync(LocatorWaitForOptions(Timeout = 30000.0f)) |> awaitUnit
let canonicalAxis = attribute axisTick "data-canonical-event-time"
let canonicalCursor = attribute rowTime "data-canonical-event-time"
let visibleStart = attribute chartStack "data-visible-start"
let visibleEnd = attribute chartStack "data-visible-end"
let loadedBars = attribute chartStack "data-loaded-bars"
let callbackCount = attribute callbackState "data-callback-count"
let utcText = textOf axisTick

require (attribute workspace "data-display-time-zone" = "UTC") "legacy initial display zone must be UTC"
require (utcText.Contains("UTC", StringComparison.Ordinal)) "UTC axis text must include its zone abbreviation"
require (not (String.IsNullOrWhiteSpace canonicalAxis)) "axis must retain a canonical UTC event-time attribute"

selector.SelectOptionAsync("America/Chicago") |> awaitTask |> ignore
waitUntil 5000 (fun () -> attribute workspace "data-display-time-zone" = "America/Chicago") "CT selection did not reach the workspace"
waitUntil 5000 (fun () -> (textOf axisTick).Contains("CDT", StringComparison.Ordinal)) "summer CT axis text did not use CDT"
require ((textOf rowTime).Contains("CDT", StringComparison.Ordinal)) "row data window did not follow the CT selection"
require (attribute axisTick "data-canonical-event-time" = canonicalAxis) "zone switch changed the canonical axis instant"
require (attribute rowTime "data-canonical-event-time" = canonicalCursor) "zone switch changed the canonical row cursor instant"
require (attribute chartStack "data-visible-start" = visibleStart && attribute chartStack "data-visible-end" = visibleEnd && attribute chartStack "data-loaded-bars" = loadedBars) "zone switch changed the viewport or loaded bars"
require (attribute callbackState "data-callback-count" = callbackCount) "zone switch emitted a provider/action request"

page.Locator("[data-testid='ta-demo-replace-markers']").ClickAsync() |> awaitUnit
waitUntil 5000 (fun () -> (textOf markerTitle).Contains("CDT", StringComparison.Ordinal)) "snapshot-to-patch marker tooltip lost the selected CT zone"
require (attribute workspace "data-display-time-zone" = "America/Chicago") "patch reset the page-scoped display zone"

selector.SelectOptionAsync("America/New_York") |> awaitTask |> ignore
waitUntil 5000 (fun () -> (textOf axisTick).Contains("EDT", StringComparison.Ordinal)) "summer ET axis text did not use EDT"

selector.SelectOptionAsync("UTC+08:00") |> awaitTask |> ignore
waitUntil 5000 (fun () -> (textOf axisTick).Contains("UTC+8", StringComparison.Ordinal)) "UTC+8 axis text was not projected"
require (attribute axisTick "data-canonical-event-time" = canonicalAxis) "second zone switch changed the canonical axis instant"
require (attribute callbackState "data-callback-count" = callbackCount) "repeated zone switching emitted a provider/action request"
require (errors.Count = 0) ("browser errors: " + String.concat " | " errors)

Directory.CreateDirectory outputDirectory |> ignore
page.ScreenshotAsync(PageScreenshotOptions(Path = Path.Combine(outputDirectory, "display-time-zone.png"), FullPage = true)) |> awaitTask |> ignore
context.CloseAsync() |> awaitUnit
browser.CloseAsync() |> awaitUnit
printfn "PASS TA display-time-zone browser verification url=%s canonical=%s viewport=%s..%s loaded=%s output=%s" url canonicalAxis visibleStart visibleEnd loadedBars outputDirectory
