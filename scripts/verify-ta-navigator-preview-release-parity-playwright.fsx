// Focused real-browser regression for loaded-domain preview/release target parity.

#i @"nuget: C:\Program Files\dotnet\sdk\10.0.401\FSharp\library-packs"
#r "nuget: FAkka.Argu, [10.1.301]"
#r "nuget: Microsoft.Playwright, 1.52.0"

#load "ParseLine.fsx"

open System
open System.IO
open System.Text.RegularExpressions
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
            | Output_Dir _ -> "Directory for browser evidence."
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
        "--url \"http://127.0.0.1:18882/\" --output-dir \"artifacts/ta-navigator-preview-release-parity\" --browser-executable-path \"%s\""
        (defaultBrowserPath.Replace('\\', '/'))

let parser = ArgumentParser.Create<CliArgs>(programName = "verify-ta-navigator-preview-release-parity-playwright.fsx")
let defaultArguments = PL.parseLine [| ' ' |] (Some '"') None true defaultArgumentsText
let automationArguments = fsi.CommandLineArgs |> Array.skip 1 |> Array.skipWhile ((=) "--")
let defaults = parser.Parse defaultArguments
let automation = parser.Parse automationArguments
let pick tryAutomation tryDefault fallback = tryAutomation () |> Option.orElseWith tryDefault |> Option.defaultValue fallback
let url = pick (fun () -> automation.TryGetResult(<@ Url @>)) (fun () -> defaults.TryGetResult(<@ Url @>)) "http://127.0.0.1:18882/"
let outputDirectory =
    pick (fun () -> automation.TryGetResult(<@ Output_Dir @>)) (fun () -> defaults.TryGetResult(<@ Output_Dir @>)) "artifacts/ta-navigator-preview-release-parity"
    |> Path.GetFullPath
let browserExecutablePath =
    pick (fun () -> automation.TryGetResult(<@ Browser_Executable_Path @>)) (fun () -> defaults.TryGetResult(<@ Browser_Executable_Path @>)) defaultBrowserPath
let headed = automation.Contains Headed || defaults.Contains Headed

let awaitTask (task: Task<'T>) = task.GetAwaiter().GetResult()
let awaitUnit (task: Task) = task.GetAwaiter().GetResult()
let require condition message = if not condition then failwith ("Navigator preview/release parity failed: " + message)
let textOf (locator: ILocator) = locator.TextContentAsync() |> awaitTask |> Option.ofObj |> Option.defaultValue ""
let attributeOf (locator: ILocator) name = locator.GetAttributeAsync(name) |> awaitTask |> Option.ofObj |> Option.defaultValue ""
let intAttribute locator name =
    match Int32.TryParse(attributeOf locator name) with
    | true, value -> value
    | _ -> failwith $"Navigator preview/release parity failed: `{name}` is not an integer."
let waitUntil label probe predicate =
    let deadline = DateTime.UtcNow.AddSeconds 12.0
    let mutable value = probe ()
    while not (predicate value) && DateTime.UtcNow < deadline do
        Thread.Sleep 20
        value <- probe ()
    require (predicate value) $"{label}; last={value}"
    value
let waitInt locator name expected = waitUntil $"expected {name}={expected}" (fun () -> intAttribute locator name) ((=) expected) |> ignore
let parsePreviewStart text =
    let matched = Regex.Match(text, @"Preview\s+(\d+)-(\d+)")
    require matched.Success ("preview range is missing: " + text)
    Int32.Parse matched.Groups[1].Value

require (File.Exists browserExecutablePath) $"browser executable not found: {browserExecutablePath}"
Directory.CreateDirectory outputDirectory |> ignore

let playwright = Playwright.CreateAsync() |> awaitTask
let browser = playwright.Chromium.LaunchAsync(BrowserTypeLaunchOptions(Headless = not headed, ExecutablePath = browserExecutablePath)) |> awaitTask
let page = browser.NewPageAsync(BrowserNewPageOptions(ViewportSize = ViewportSize(Width = 1440, Height = 980))) |> awaitTask
let consoleErrors = ResizeArray<string>()
page.Console.Add(fun message -> if message.Type = "error" then consoleErrors.Add message.Text)
page.PageError.Add(fun message -> consoleErrors.Add message)

try
    page.GotoAsync(url, PageGotoOptions(WaitUntil = WaitUntilState.NetworkIdle)) |> awaitTask |> ignore
    page.Locator("[data-testid='ta-workspace']").WaitForAsync(LocatorWaitForOptions(Timeout = 15000.0f)) |> awaitUnit
    page.Locator("[data-testid='ta-demo-loaded-coverage-parity']").ClickAsync() |> awaitUnit

    let chartStack = page.Locator("[data-testid='ta-chart-stack']")
    let callbackState = page.Locator("[data-testid='ta-demo-callback-state']")
    let navigator = page.Locator("[data-testid='ta-overview-navigator']")
    let selection = page.Locator("[data-testid='ta-overview-selection']")
    let range = page.Locator("[data-testid='ta-viewport-range']")
    waitInt chartStack "data-loaded-bars" 900
    waitInt chartStack "data-visible-start" 701
    waitInt chartStack "data-visible-end" 900

    let navigatorBox = navigator.BoundingBoxAsync() |> awaitTask
    let selectionBox = selection.BoundingBoxAsync() |> awaitTask
    require (not (isNull navigatorBox) && not (isNull selectionBox)) "navigator geometry is unavailable"
    let y = selectionBox.Y + selectionBox.Height / 2.0f
    let startX = selectionBox.X + selectionBox.Width / 2.0f

    page.Mouse.MoveAsync(startX, y) |> awaitUnit
    page.Mouse.DownAsync(MouseDownOptions(Button = MouseButton.Left)) |> awaitUnit
    page.Mouse.MoveAsync(startX - 40.0f, y, MouseMoveOptions(Steps = 12)) |> awaitUnit
    waitUntil "preview must become visible" (fun () -> textOf range) (fun value -> value.Contains "Preview") |> ignore
    Thread.Sleep 250
    let previewText = textOf range
    require (previewText.Contains "Preview") ("settled preview range is missing: " + previewText)
    let previewStart = parsePreviewStart previewText
    page.Mouse.UpAsync(MouseUpOptions(Button = MouseButton.Left)) |> awaitUnit

    let wireStartText = waitUntil "wire coverage target" (fun () -> attributeOf callbackState "data-last-coverage-start") (String.IsNullOrWhiteSpace >> not)
    let wireStart = Int64.Parse wireStartText
    let wireCount = intAttribute callbackState "data-last-coverage-count"
    require (wireStart = int64 (previewStart - 1)) $"preview/wire start mismatch: preview={previewStart}, wireOrdinal={wireStart}"
    require (wireCount = 200) $"wire count changed: {wireCount}"
    waitInt chartStack "data-visible-start" previewStart
    waitInt chartStack "data-visible-end" (previewStart + wireCount - 1)
    require (consoleErrors.Count = 0) ("browser console errors: " + String.concat " | " consoleErrors)

    let screenshotPath = Path.Combine(outputDirectory, "preview-release-parity.png")
    page.ScreenshotAsync(PageScreenshotOptions(Path = screenshotPath, FullPage = true)) |> awaitTask |> ignore
    printfn "navigator-preview-release-parity PASS preview=%d wireOrdinal=%d count=%d screenshot=%s" previewStart wireStart wireCount screenshotPath
finally
    browser.CloseAsync() |> awaitUnit
    playwright.Dispose()
