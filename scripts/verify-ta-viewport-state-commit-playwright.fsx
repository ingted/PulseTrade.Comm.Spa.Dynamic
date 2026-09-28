// Focused real-browser gate for committed viewport state versus scheduled row readiness.

#i @"nuget: C:\Program Files\dotnet\sdk\10.0.401\FSharp\library-packs"
#r "nuget: FAkka.Argu, [10.1.301]"
#r "nuget: Microsoft.Playwright, 1.52.0"

#load "ParseLine.fsx"

open System
open System.Collections.Generic
open System.Diagnostics
open System.Threading
open System.Threading.Tasks
open Argu
open Microsoft.Playwright

type CliArgs =
    | Url of string
    | Browser_Executable_Path of string
    | Headed
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Url _ -> "Existing TA renderer BrowserDemo URL."
            | Browser_Executable_Path _ -> "Chrome or Edge executable path."
            | Headed -> "Run the browser headed."

let knownBrowserPaths =
    [ @"C:\Program Files\Google\Chrome\Application\chrome.exe"
      @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe"
      @"C:\Program Files\Microsoft\Edge\Application\msedge.exe"
      @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe" ]

let defaultBrowserPath = knownBrowserPaths |> List.tryFind IO.File.Exists |> Option.defaultValue ""
let defaultArgumentsText =
    $"--url \"http://127.0.0.1:18882/\" --browser-executable-path \"{defaultBrowserPath.Replace('\\', '/')}\""

let parser = ArgumentParser.Create<CliArgs>(programName = "verify-ta-viewport-state-commit-playwright.fsx")
let defaults = parser.Parse(PL.parseLine [| ' ' |] (Some '"') None true defaultArgumentsText)
let automation = parser.Parse(fsi.CommandLineArgs |> Array.skip 1 |> Array.skipWhile ((=) "--"))
let pick tryAutomation tryDefault fallback =
    tryAutomation () |> Option.orElseWith tryDefault |> Option.defaultValue fallback

let url = pick (fun () -> automation.TryGetResult(<@ Url @>)) (fun () -> defaults.TryGetResult(<@ Url @>)) "http://127.0.0.1:18882/"
let browserExecutablePath =
    pick
        (fun () -> automation.TryGetResult(<@ Browser_Executable_Path @>))
        (fun () -> defaults.TryGetResult(<@ Browser_Executable_Path @>))
        defaultBrowserPath
let headed = automation.Contains Headed || defaults.Contains Headed

let awaitTask (task: Task<'T>) = task.GetAwaiter().GetResult()
let awaitUnit (task: Task) = task.GetAwaiter().GetResult()
let require condition message = if not condition then failwith ("TA viewport commit verification failed: " + message)

let textOf (locator: ILocator) =
    locator.TextContentAsync() |> awaitTask |> Option.ofObj |> Option.defaultValue ""

let attribute (locator: ILocator) name =
    locator.GetAttributeAsync(name) |> awaitTask |> Option.ofObj |> Option.defaultValue ""

let waitUntil seconds label predicate =
    let deadline = DateTime.UtcNow.AddSeconds seconds
    while not (predicate ()) && DateTime.UtcNow < deadline do Thread.Sleep 10
    require (predicate ()) $"timeout waiting for {label}"

let waitForText (locator: ILocator) (expected: string) =
    waitUntil 15.0 expected (fun () -> (textOf locator).Contains(expected, StringComparison.Ordinal))

let waitForRows (chartStack: ILocator) =
    waitUntil 15.0 "all scheduled rows" (fun () ->
        let count = attribute chartStack "data-row-count"
        count <> "" && count = attribute chartStack "data-ready-row-count")

let playwright = Playwright.CreateAsync() |> awaitTask
let browser =
    playwright.Chromium.LaunchAsync(
        BrowserTypeLaunchOptions(
            Headless = not headed,
            ExecutablePath = browserExecutablePath))
    |> awaitTask

let context = browser.NewContextAsync() |> awaitTask
let page = context.NewPageAsync() |> awaitTask
let consoleErrors = ResizeArray<string>()
let pageErrors = ResizeArray<string>()
page.Console.Add(fun message -> if message.Type = "error" then consoleErrors.Add message.Text)
page.PageError.Add(fun error -> pageErrors.Add error)

page.GotoAsync(url, PageGotoOptions(WaitUntil = WaitUntilState.NetworkIdle, Timeout = 30000.0f)) |> awaitTask |> ignore
let chartStack = page.Locator("[data-testid='ta-chart-stack']")
let viewportRange = page.Locator("[data-testid='ta-viewport-range']")
let view200 = page.Locator("[data-testid='ta-view-200']")
let viewAll = page.Locator("[data-testid='ta-view-all']")
let callbackState = page.Locator("[data-testid='ta-demo-callback-state']")

chartStack.WaitForAsync(LocatorWaitForOptions(State = WaitForSelectorState.Visible, Timeout = 30000.0f)) |> awaitUnit
waitForText viewportRange "Loaded 4000 bars"
waitForRows chartStack

let callbackCountBefore200 = Int32.Parse(attribute callbackState "data-callback-count")
view200.ClickAsync() |> awaitUnit
waitForText viewportRange "Viewing 3801-4000"
waitForRows chartStack
waitUntil 10.0 "200 preset callback settlement" (fun () ->
    Int32.Parse(attribute callbackState "data-callback-count") = callbackCountBefore200 + 1)
waitUntil 10.0 "All preset enabled" (fun () -> viewAll.IsEnabledAsync() |> awaitTask)
let renderBeforeAll = Int32.Parse(attribute chartStack "data-chart-render-sequence")
let callbackCountBeforeAll = Int32.Parse(attribute callbackState "data-callback-count")

let stateWatch = Stopwatch.StartNew()
viewAll.ClickAsync() |> awaitUnit
waitForText viewportRange "Viewing 1-4000"
stateWatch.Stop()

let rowsWatch = Stopwatch.StartNew()
waitForRows chartStack
rowsWatch.Stop()
waitUntil 10.0 "All preset callback settlement" (fun () ->
    Int32.Parse(attribute callbackState "data-callback-count") = callbackCountBeforeAll + 1)
waitForRows chartStack
let totalMilliseconds = stateWatch.Elapsed.TotalMilliseconds + rowsWatch.Elapsed.TotalMilliseconds
let renderAfterAll = Int32.Parse(attribute chartStack "data-chart-render-sequence")

require (stateWatch.Elapsed.TotalMilliseconds <= 750.0) $"committed state took {stateWatch.Elapsed.TotalMilliseconds:F2}ms; limit=750ms"
require (totalMilliseconds <= 1500.0) $"rows-ready transition took {totalMilliseconds:F2}ms; limit=1500ms"
require (renderAfterAll = renderBeforeAll + 1) $"expected one chart render, before={renderBeforeAll} after={renderAfterAll}"
require (attribute chartStack "data-visible-start" = "1") "chart visible start did not commit to 1"
require (attribute chartStack "data-visible-end" = "4000") "chart visible end did not commit to 4000"
let consoleErrorText = String.concat " | " consoleErrors
let pageErrorText = String.concat " | " pageErrors
require (consoleErrors.Count = 0) $"console errors: {consoleErrorText}"
require (pageErrors.Count = 0) $"page errors: {pageErrorText}"

printfn
    "TA viewport commit Playwright PASS url=%s stateMs=%.2f rowsReadyTotalMs=%.2f render=%d->%d"
    url
    stateWatch.Elapsed.TotalMilliseconds
    totalMilliseconds
    renderBeforeAll
    renderAfterAll

context.DisposeAsync().AsTask() |> awaitUnit
browser.DisposeAsync().AsTask() |> awaitUnit
playwright.Dispose()
