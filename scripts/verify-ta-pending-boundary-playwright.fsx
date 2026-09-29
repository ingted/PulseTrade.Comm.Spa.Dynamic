// Focused real-browser regression for a navigator boundary drag made while a remote viewport action is pending.

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
            | Output_Dir _ -> "Directory for the regression screenshot."
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
        "--url \"http://127.0.0.1:18882/\" --output-dir \"artifacts/ta-pending-boundary-playwright\" --browser-executable-path \"%s\""
        (defaultBrowserPath.Replace('\\', '/'))

let parser = ArgumentParser.Create<CliArgs>(programName = "verify-ta-pending-boundary-playwright.fsx")
let defaultArguments = PL.parseLine [| ' ' |] (Some '"') None true defaultArgumentsText
let automationArguments = fsi.CommandLineArgs |> Array.skip 1 |> Array.skipWhile ((=) "--")
let defaults = parser.Parse defaultArguments
let automation = parser.Parse automationArguments
let pick tryAutomation tryDefault fallback =
    tryAutomation () |> Option.orElseWith tryDefault |> Option.defaultValue fallback

let url = pick (fun () -> automation.TryGetResult(<@ Url @>)) (fun () -> defaults.TryGetResult(<@ Url @>)) "http://127.0.0.1:18882/"
let outputDirectory =
    pick (fun () -> automation.TryGetResult(<@ Output_Dir @>)) (fun () -> defaults.TryGetResult(<@ Output_Dir @>)) "artifacts/ta-pending-boundary-playwright"
    |> Path.GetFullPath
let browserExecutablePath =
    pick (fun () -> automation.TryGetResult(<@ Browser_Executable_Path @>)) (fun () -> defaults.TryGetResult(<@ Browser_Executable_Path @>)) defaultBrowserPath
let headed = automation.Contains Headed || defaults.Contains Headed

let awaitTask (task: Task<'T>) = task.GetAwaiter().GetResult()
let awaitUnit (task: Task) = task.GetAwaiter().GetResult()
let require condition message =
    if not condition then failwith ("Pending boundary Playwright verification failed: " + message)

let intAttribute (locator: ILocator) name =
    let raw = locator.GetAttributeAsync(name) |> awaitTask |> Option.ofObj |> Option.defaultValue ""
    match Int32.TryParse raw with
    | true, value -> value
    | _ -> failwith $"Pending boundary Playwright verification failed: `{name}` is not an integer: `{raw}`"

let stringAttribute (locator: ILocator) name =
    locator.GetAttributeAsync(name) |> awaitTask |> Option.ofObj |> Option.defaultValue ""

let waitUntil label probe predicate =
    let deadline = DateTime.UtcNow.AddSeconds 12.0
    let mutable value = probe ()
    while not (predicate value) && DateTime.UtcNow < deadline do
        Thread.Sleep 25
        value <- probe ()
    require (predicate value) $"{label}; last={value}"
    value

let waitInt locator name expected =
    waitUntil $"expected `{name}`={expected}" (fun () -> intAttribute locator name) ((=) expected) |> ignore

let waitText (locator: ILocator) (expected: string) =
    waitUntil
        $"expected text `{expected}`"
        (fun () -> locator.TextContentAsync() |> awaitTask |> Option.ofObj |> Option.defaultValue "")
        (fun value -> value.Contains expected)
    |> ignore

require (File.Exists browserExecutablePath) $"browser executable not found: {browserExecutablePath}"
Directory.CreateDirectory outputDirectory |> ignore

let playwright = Playwright.CreateAsync() |> awaitTask
let launchOptions = BrowserTypeLaunchOptions(Headless = not headed, ExecutablePath = browserExecutablePath)
let browser = playwright.Chromium.LaunchAsync(launchOptions) |> awaitTask
let page = browser.NewPageAsync(BrowserNewPageOptions(ViewportSize = ViewportSize(Width = 1440, Height = 980))) |> awaitTask
let consoleErrors = ResizeArray<string>()
page.Console.Add(fun message -> if message.Type = "error" then consoleErrors.Add message.Text)
page.PageError.Add(fun message -> consoleErrors.Add message)

page.GotoAsync(url, PageGotoOptions(WaitUntil = WaitUntilState.NetworkIdle)) |> awaitTask |> ignore
page.Locator("[data-testid='ta-workspace']").WaitForAsync(LocatorWaitForOptions(Timeout = 15000.0f)) |> awaitUnit
page.Locator("[data-testid='ta-demo-loaded-coverage']").ClickAsync() |> awaitUnit
page.Locator("[data-testid='ta-demo-model-poll-in-flight']").ClickAsync() |> awaitUnit

let chartStack = page.Locator("[data-testid='ta-chart-stack']")
let callbackState = page.Locator("[data-testid='ta-demo-callback-state']")
let dragSelectionToEarlierBoundary scenario =
    let navigator = page.Locator("[data-testid='ta-overview-navigator']")
    let interactionSurface = page.Locator("[data-testid='ta-overview-interaction-surface']")
    require
        (stringAttribute interactionSurface "data-drag-event-binding" = "interaction-surface")
        "navigator drag start is not bound to the canonical interaction surface"
    let bounds = navigator.BoundingBoxAsync() |> awaitTask
    require (not (isNull bounds)) "navigator geometry is unavailable"
    let selectionBounds = page.Locator("[data-testid='ta-overview-selection']").BoundingBoxAsync() |> awaitTask
    require (not (isNull selectionBounds)) "navigator selection geometry is unavailable"
    let y = selectionBounds.Y + selectionBounds.Height / 2.0f
    let selectionCenterX = selectionBounds.X + selectionBounds.Width / 2.0f
    let beyondLeftX = max 1.0f (bounds.X - 12.0f)
    printfn "%s.geometry navigator=(%.1f,%.1f %.1fx%.1f) selection=(%.1f,%.1f %.1fx%.1f) drag=(%.1f,%.1f)->(%.1f,%.1f)" scenario bounds.X bounds.Y bounds.Width bounds.Height selectionBounds.X selectionBounds.Y selectionBounds.Width selectionBounds.Height selectionCenterX y beyondLeftX y
    page.Mouse.MoveAsync(selectionCenterX, y) |> awaitUnit
    page.Mouse.DownAsync(MouseDownOptions(Button = MouseButton.Left)) |> awaitUnit
    page.Mouse.MoveAsync(beyondLeftX, y) |> awaitUnit
    printfn "%s.after-move range=%s" scenario (page.Locator("[data-testid='ta-viewport-range']").TextContentAsync() |> awaitTask |> Option.ofObj |> Option.defaultValue "")
    waitText (page.Locator("[data-testid='ta-viewport-range']")) "Preview"
    page.Mouse.UpAsync(MouseUpOptions(Button = MouseButton.Left)) |> awaitUnit

waitInt chartStack "data-loaded-bars" 500
waitInt chartStack "data-visible-start" 251
waitInt chartStack "data-visible-end" 500
let callbacksBefore = intAttribute callbackState "data-callback-count"

page.Locator("[data-testid='ta-view-48']").ClickAsync() |> awaitUnit
waitInt chartStack "data-visible-start" 453
waitInt chartStack "data-visible-end" 500
waitText (page.Locator("[data-testid='ta-poll-state']")) "UPDATING"

dragSelectionToEarlierBoundary "poll-in-flight"

waitText (page.Locator("[data-testid='ta-feedback']")) "Earlier coverage queued."
waitInt callbackState "data-callback-count" (callbacksBefore + 2)
waitInt chartStack "data-query-generation" 2
waitInt chartStack "data-visible-start" 405
waitInt chartStack "data-visible-end" 452
require (stringAttribute callbackState "data-last-action" = "VisibleRangeChanged") "the queued boundary action was not dispatched last"
require (consoleErrors.Count = 0) ("browser errors: " + String.concat " | " consoleErrors)

let screenshotPath = Path.Combine(outputDirectory, "pending-boundary-latest-intent.png")
page.ScreenshotAsync(PageScreenshotOptions(Path = screenshotPath, FullPage = true)) |> awaitTask |> ignore
printfn "pending-boundary.pass callbacks=%d->%d queryGeneration=2 visible=405-452 screenshot=%s" callbacksBefore (callbacksBefore + 2) screenshotPath

page.ReloadAsync(PageReloadOptions(WaitUntil = WaitUntilState.NetworkIdle)) |> awaitTask |> ignore
page.Locator("[data-testid='ta-workspace']").WaitForAsync(LocatorWaitForOptions(Timeout = 15000.0f)) |> awaitUnit
page.Locator("[data-testid='ta-demo-loaded-coverage']").ClickAsync() |> awaitUnit
waitInt chartStack "data-loaded-bars" 500
waitInt chartStack "data-ready-row-count" 7
let resyncView48 = page.Locator("[data-testid='ta-view-48']")
waitUntil "resync view-48 enabled" (fun () -> resyncView48.IsEnabledAsync() |> awaitTask) id |> ignore
resyncView48.ClickAsync() |> awaitUnit
waitInt chartStack "data-visible-start" 453
waitInt chartStack "data-visible-end" 500
waitText (page.Locator("[data-testid='ta-poll-state']")) "READY"
let callbacksBeforeResync = intAttribute callbackState "data-callback-count"
page.Locator("[data-testid='ta-demo-paused']").ClickAsync() |> awaitUnit
waitText (page.Locator("[data-testid='ta-poll-state']")) "RESYNC"
dragSelectionToEarlierBoundary "paused-for-resync"
waitText (page.Locator("[data-testid='ta-feedback']")) "Earlier coverage queued."
require (intAttribute callbackState "data-callback-count" = callbacksBeforeResync) "PausedForResync queue must not dispatch before Ready"
page.Locator("[data-testid='ta-demo-live']").ClickAsync() |> awaitUnit
waitText (page.Locator("[data-testid='ta-poll-state']")) "READY"
waitInt callbackState "data-callback-count" (callbacksBeforeResync + 1)
waitInt chartStack "data-query-generation" 2
waitInt chartStack "data-visible-start" 405
waitInt chartStack "data-visible-end" 452
let resyncScreenshotPath = Path.Combine(outputDirectory, "paused-for-resync-boundary-latest-intent.png")
page.ScreenshotAsync(PageScreenshotOptions(Path = resyncScreenshotPath, FullPage = true)) |> awaitTask |> ignore
printfn "paused-for-resync.pass callbacks=%d->%d queryGeneration=2 visible=405-452 screenshot=%s" callbacksBeforeResync (callbacksBeforeResync + 1) resyncScreenshotPath

page.ReloadAsync(PageReloadOptions(WaitUntil = WaitUntilState.NetworkIdle)) |> awaitTask |> ignore
page.Locator("[data-testid='ta-workspace']").WaitForAsync(LocatorWaitForOptions(Timeout = 15000.0f)) |> awaitUnit
page.Locator("[data-testid='ta-demo-loaded-coverage']").ClickAsync() |> awaitUnit
waitInt chartStack "data-loaded-bars" 500
waitInt chartStack "data-ready-row-count" 7
let interleavedCallbacksBefore = intAttribute callbackState "data-callback-count"
page.Locator("[data-testid='ta-demo-model-poll-in-flight']").ClickAsync() |> awaitUnit
page.Locator("[data-testid='ta-view-48']").ClickAsync() |> awaitUnit
waitText (page.Locator("[data-testid='ta-poll-state']")) "UPDATING"
page.Locator("[data-testid='ta-demo-paused']").ClickAsync() |> awaitUnit
waitText (page.Locator("[data-testid='ta-poll-state']")) "RESYNC"
dragSelectionToEarlierBoundary "pending-action-resync"
waitText (page.Locator("[data-testid='ta-feedback']")) "Earlier coverage queued."
waitInt callbackState "data-callback-count" (interleavedCallbacksBefore + 2)
waitText (page.Locator("[data-testid='ta-poll-state']")) "READY"
waitInt chartStack "data-query-generation" 2
waitInt chartStack "data-visible-start" 405
waitInt chartStack "data-visible-end" 452
let interleavedScreenshotPath = Path.Combine(outputDirectory, "pending-action-resync-boundary-latest-intent.png")
page.ScreenshotAsync(PageScreenshotOptions(Path = interleavedScreenshotPath, FullPage = true)) |> awaitTask |> ignore
printfn "pending-action-resync.pass callbacks=%d->%d queryGeneration=2 visible=405-452 screenshot=%s" interleavedCallbacksBefore (interleavedCallbacksBefore + 2) interleavedScreenshotPath

browser.CloseAsync() |> awaitUnit
playwright.Dispose()
