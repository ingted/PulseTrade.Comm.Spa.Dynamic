// Generic marker browser verifier for the existing TA Renderer BrowserDemo host.

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
    $"--url \"http://127.0.0.1:18882/\" --output-dir \"artifacts/ta-generic-marker-playwright\" --browser-executable-path \"{defaultBrowserPath.Replace('\\', '/')}\""

let parser = ArgumentParser.Create<CliArgs>(programName = "verify-ta-generic-marker-playwright.fsx")
let defaults = parser.Parse(PL.parseLine [| ' ' |] (Some '"') None true defaultArgumentsText)
let automation = parser.Parse(fsi.CommandLineArgs |> Array.skip 1 |> Array.skipWhile ((=) "--"))
let pick tryAutomation tryDefault fallback =
    tryAutomation () |> Option.orElseWith tryDefault |> Option.defaultValue fallback

let url = pick (fun () -> automation.TryGetResult(<@ Url @>)) (fun () -> defaults.TryGetResult(<@ Url @>)) "http://127.0.0.1:18882/"
let outputDirectory = pick (fun () -> automation.TryGetResult(<@ Output_Dir @>)) (fun () -> defaults.TryGetResult(<@ Output_Dir @>)) "artifacts/ta-generic-marker-playwright" |> Path.GetFullPath
let browserExecutablePath = pick (fun () -> automation.TryGetResult(<@ Browser_Executable_Path @>)) (fun () -> defaults.TryGetResult(<@ Browser_Executable_Path @>)) defaultBrowserPath
let headed = automation.Contains Headed || defaults.Contains Headed

let awaitTask (task: Task<'T>) = task.GetAwaiter().GetResult()
let awaitUnit (task: Task) = task.GetAwaiter().GetResult()
let require condition message = if not condition then failwith ("Generic marker Playwright verification failed: " + message)
let textOf (locator: ILocator) = locator.TextContentAsync() |> awaitTask |> Option.ofObj |> Option.defaultValue ""
let attribute (locator: ILocator) name = locator.GetAttributeAsync(name) |> awaitTask |> Option.ofObj |> Option.defaultValue ""
let intAttribute locator name =
    match Int32.TryParse(attribute locator name) with
    | true, value -> value
    | _ -> failwith $"Generic marker Playwright verification failed: invalid integer attribute `{name}`."

let verify viewportWidth viewportHeight screenshotName runCursorGate (browser: IBrowser) =
    let context = browser.NewContextAsync(BrowserNewContextOptions(ViewportSize = ViewportSize(Width = viewportWidth, Height = viewportHeight))) |> awaitTask
    let page = context.NewPageAsync() |> awaitTask
    let errors = ResizeArray<string>()
    page.Console.Add(fun message -> if message.Type = "error" then errors.Add message.Text)
    page.PageError.Add(errors.Add)
    page.GotoAsync(url, PageGotoOptions(WaitUntil = WaitUntilState.NetworkIdle)) |> awaitTask |> ignore
    page.Locator("[data-testid='ta-workspace']").WaitForAsync(LocatorWaitForOptions(Timeout = 15000.0f)) |> awaitUnit

    let layer = page.Locator("[data-testid='ta-marker-layer-price']")
    let entry = page.Locator("[data-testid='ta-marker-signals-entry-long']")
    let signalA = page.Locator("[data-testid='ta-marker-signals-signal-a']")
    let signalB = page.Locator("[data-testid='ta-marker-signals-signal-b']")
    let exitMarker = page.Locator("[data-testid='ta-marker-signals-exit-long']")
    require (layer.CountAsync() |> awaitTask = 1) "price row must mount one marker layer"
    require (intAttribute layer "data-marker-count" = 4) "four visible markers must render"
    require (page.Locator("[data-testid='ta-row-value-price-signals']").CountAsync() |> awaitTask = 0) "marker event overlays must not create a numeric legend token"
    require (not ((page.Locator("[data-testid='ta-row-values-price']").InnerTextAsync() |> awaitTask).Contains("Signals Undef", StringComparison.Ordinal))) "marker event overlays leaked an undefined numeric value"
    require (attribute entry "data-marker-anchor" = "below-bar") "entry marker anchor changed"
    require (attribute signalA "data-marker-position" = "3812") "Position must remain the spatial authority"
    require (intAttribute signalA "data-marker-lane" = 0 && intAttribute signalB "data-marker-lane" = 1) "same-position markers must stack deterministically"
    let tooltip = textOf (signalA.Locator("title"))
    require (tooltip.Contains "A" && tooltip.Contains "Reason: signal A" && tooltip.Contains "Source: BrowserDemo") "tooltip order/content changed"

    let rowBox = page.Locator("[data-testid='ta-row-price']").BoundingBoxAsync() |> awaitTask
    for label, marker in [ "entry", entry; "signal-a", signalA; "signal-b", signalB; "exit", exitMarker ] do
        let markerBox = marker.BoundingBoxAsync() |> awaitTask
        require (not (isNull rowBox) && not (isNull markerBox)) (label + " geometry is missing")
        require (markerBox.Y >= rowBox.Y - 0.5f && markerBox.Y + markerBox.Height <= rowBox.Y + rowBox.Height + 0.5f) (label + " escaped its row")

    if runCursorGate then
        let chart = page.Locator("[data-testid='ta-candle-price']")
        let crosshair = page.Locator("[data-testid='ta-candle-price-crosshair']")
        let chartBox = chart.BoundingBoxAsync() |> awaitTask
        require (not (isNull chartBox)) "price chart geometry is missing"
        let mutable prior = attribute crosshair "x1"
        let mutable maximumMs = 0L
        let total = Diagnostics.Stopwatch.StartNew()
        for index in 0 .. 199 do
            let ratio = if index % 2 = 0 then 0.2f else 0.8f
            let transition = Diagnostics.Stopwatch.StartNew()
            page.Mouse.MoveAsync(chartBox.X + chartBox.Width * ratio, chartBox.Y + chartBox.Height / 2.0f) |> awaitUnit
            let deadline = DateTime.UtcNow.AddSeconds 2.0
            let mutable current = attribute crosshair "x1"
            while current = prior && DateTime.UtcNow < deadline do
                Threading.Thread.Sleep 5
                current <- attribute crosshair "x1"
            transition.Stop()
            require (current <> prior) $"cursor transition {index} did not render"
            maximumMs <- max maximumMs transition.ElapsedMilliseconds
            prior <- current
        total.Stop()
        require (maximumMs < 250L) $"cursor transition stalled for {maximumMs}ms"
        require (total.Elapsed < TimeSpan.FromSeconds 8.0) $"200 cursor transitions took {total.Elapsed}"
        printfn "marker cursor gate transitions=200 elapsedMs=%d maxMs=%d" total.ElapsedMilliseconds maximumMs

    require (errors.Count = 0) ("browser errors: " + String.concat " | " errors)
    Directory.CreateDirectory outputDirectory |> ignore
    page.ScreenshotAsync(PageScreenshotOptions(Path = Path.Combine(outputDirectory, screenshotName), FullPage = true)) |> awaitTask |> ignore
    context.CloseAsync() |> awaitUnit

require (not (String.IsNullOrWhiteSpace browserExecutablePath) && File.Exists browserExecutablePath) "Chrome or Edge executable was not found"
let playwright = Playwright.CreateAsync() |> awaitTask
let browser =
    playwright.Chromium.LaunchAsync(
        BrowserTypeLaunchOptions(
            ExecutablePath = browserExecutablePath,
            Headless = not headed,
            Args = [| "--disable-gpu" |]))
    |> awaitTask

verify 1440 900 "marker-desktop.png" true browser
verify 390 844 "marker-mobile.png" false browser
browser.CloseAsync() |> awaitUnit
printfn "PASS generic marker browser verification url=%s output=%s" url outputDirectory
