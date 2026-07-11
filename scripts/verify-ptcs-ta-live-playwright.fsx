// Real PTCS /sync/ws browser gate for the Dynamic TA transient client.

#i @"nuget: C:\Program Files\dotnet\sdk\10.0.301\FSharp\library-packs"
#r "nuget: FAkka.Argu, [10.1.301]"
#r "nuget: Microsoft.Playwright, 1.52.0"

#load "ParseLine.fsx"

open System
open System.Globalization
open System.IO
open System.Net.Http
open System.Text.Json
open System.Text.RegularExpressions
open System.Threading
open System.Threading.Tasks
open Argu
open Microsoft.Playwright

type CliArgs =
    | Url of string
    | Output_Dir of string
    | Browser_Executable_Path of string
    | Poll_Target of int
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Url _ -> "Existing PTCS TA live-demo chat URL."
            | Output_Dir _ -> "Directory for ignored screenshots."
            | Browser_Executable_Path _ -> "Chrome or Edge executable path."
            | Poll_Target _ -> "Minimum poll number observed before lifecycle assertions."

let knownBrowserPaths =
    [ @"C:\Program Files\Google\Chrome\Application\chrome.exe"
      @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe"
      @"C:\Program Files\Microsoft\Edge\Application\msedge.exe"
      @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe" ]

let defaultBrowserPath = knownBrowserPaths |> List.tryFind File.Exists |> Option.defaultValue ""
let defaultArgumentsText =
    sprintf
        "--url \"http://127.0.0.1:18883/chat\" --output-dir \"artifacts/ptcs-ta-live-playwright\" --browser-executable-path \"%s\" --poll-target 20"
        (defaultBrowserPath.Replace('\\', '/'))

let parser = ArgumentParser.Create<CliArgs>(programName = "verify-ptcs-ta-live-playwright.fsx")
let defaults = PL.parseLine [| ' ' |] (Some '"') None true defaultArgumentsText
let actual = fsi.CommandLineArgs |> Array.skip 1 |> Array.filter ((<>) "--")
let parsed = parser.ParseCommandLine(if actual.Length = 0 then defaults else actual)
let url = parsed.GetResult(Url, "http://127.0.0.1:18883/chat")
let outputDirectory = parsed.GetResult(Output_Dir, "artifacts/ptcs-ta-live-playwright") |> Path.GetFullPath
let browserExecutablePath = parsed.GetResult(Browser_Executable_Path, defaultBrowserPath)
let pollTarget = parsed.GetResult(Poll_Target, 20)

let awaitTask (task: Task<'T>) = task.GetAwaiter().GetResult()
let awaitUnit (task: Task) = task.GetAwaiter().GetResult()
let require condition message = if not condition then failwith ("PTCS TA live Playwright failed: " + message)

let textOf (locator: ILocator) =
    locator.TextContentAsync() |> awaitTask |> Option.ofObj |> Option.defaultValue ""

let pollNumber (text: string) =
    let matched = Regex.Match(text, @"poll\s+(\d+)", RegexOptions.IgnoreCase)
    if matched.Success then Int32.Parse(matched.Groups[1].Value, CultureInfo.InvariantCulture) else -1

let waitForPoll (page: IPage) minimum =
    let freshness = page.Locator("[data-testid='ta-freshness']")
    let deadline = DateTime.UtcNow.AddSeconds 15.0
    let mutable value = -1

    while value < minimum && DateTime.UtcNow < deadline do
        value <- freshness |> textOf |> pollNumber
        if value < minimum then Thread.Sleep 75

    require (value >= minimum) $"poll did not reach {minimum}; actual={value}; text={textOf freshness}"
    value

let waitForPollState (page: IPage) (expected: string) =
    let locator = page.Locator("[data-testid='ta-poll-state']")
    let deadline = DateTime.UtcNow.AddSeconds 5.0
    let mutable current = ""

    while not (current.Contains expected) && DateTime.UtcNow < deadline do
        current <- textOf locator
        if not (current.Contains expected) then Thread.Sleep 50

    require (current.Contains expected) $"expected poll state {expected}; actual={current}"

let eventCount () =
    let baseUri = Uri(url)
    use client = new HttpClient(Timeout = TimeSpan.FromSeconds 5.0)
    let health = client.GetStringAsync(Uri(baseUri, "/healthz")) |> awaitTask
    use document = JsonDocument.Parse health
    document.RootElement.GetProperty("persistence").GetProperty("pcslProjection").GetProperty("eventCount").GetInt64()

let verify viewportWidth viewportHeight (label: string) (browser: IBrowser) =
    let context =
        browser.NewContextAsync(
            BrowserNewContextOptions(
                ViewportSize = ViewportSize(Width = viewportWidth, Height = viewportHeight),
                IsMobile = (viewportWidth < 600)))
        |> awaitTask
    let page = context.NewPageAsync() |> awaitTask
    let errors = ResizeArray<string>()
    page.Console.Add(fun message ->
        printfn "[%s][console:%s] %s" label message.Type message.Text
        if message.Type = "error" then errors.Add message.Text)
    page.PageError.Add(fun error ->
        printfn "[%s][page-error] %s" label error
        errors.Add error)
    page.GotoAsync(url, PageGotoOptions(WaitUntil = WaitUntilState.NetworkIdle)) |> awaitTask |> ignore
    try
        page.Locator("[data-testid='ta-ptcs-live-marker']").WaitForAsync(LocatorWaitForOptions(Timeout = 8000.0f)) |> awaitUnit
    with error ->
        let scripts =
            page.Locator("script")
                .EvaluateAllAsync<string array>("nodes => nodes.map(node => node.src || '<inline>')")
            |> awaitTask
        let bodyText = page.Locator("body").InnerTextAsync() |> awaitTask
        printfn "[%s][diagnostic] scripts=%A" label scripts
        printfn "[%s][diagnostic] body=%s" label (if bodyText.Length > 1200 then bodyText.Substring(0, 1200) else bodyText)
        reraise ()
    page.Locator("[data-testid='ta-workspace']").WaitForAsync(LocatorWaitForOptions(Timeout = 8000.0f)) |> awaitUnit

    let rows = page.Locator("[data-testid='ta-chart-stack'] section")
    try
        rows.Nth(2).WaitForAsync(LocatorWaitForOptions(Timeout = 8000.0f)) |> awaitUnit
    with _ ->
        printfn "[%s][state-timeout] app=%s" label (page.Locator("#ta-ptcs-live-app").InnerTextAsync() |> awaitTask)
        reraise ()
    let candles = page.Locator("[data-testid='ta-candle-price'] rect")
    candles.Nth(47).WaitForAsync(LocatorWaitForOptions(Timeout = 8000.0f)) |> awaitUnit
    require ((rows.CountAsync() |> awaitTask) = 3) (label + " must render three server-defined rows")
    require ((candles.CountAsync() |> awaitTask) >= 48) (label + " must render the 500-bar bounded snapshot")
    let cursorItems = page.Locator("[data-testid='ta-cursor-values'] > *")
    let cursorText = textOf (page.Locator("[data-testid='ta-cursor-values']"))
    require (cursorText.Contains "07-01" && not (cursorText.Contains "2026-07-01T")) (label + " cursor timestamp must use compact display text")
    let cursorBoxes =
        [| for index in 0 .. (cursorItems.CountAsync() |> awaitTask) - 1 do
               let box = cursorItems.Nth(index).BoundingBoxAsync() |> awaitTask
               if not (isNull box) then yield box |]
    for leftIndex in 0 .. cursorBoxes.Length - 1 do
        for rightIndex in leftIndex + 1 .. cursorBoxes.Length - 1 do
            let left = cursorBoxes[leftIndex]
            let right = cursorBoxes[rightIndex]
            let overlaps = left.X < right.X + right.Width && right.X < left.X + left.Width && left.Y < right.Y + right.Height && right.Y < left.Y + left.Height
            require (not overlaps) (label + " cursor detail items overlap")
    let reached = waitForPoll page pollTarget
    let priceBox = page.Locator("[data-testid='ta-candle-price']").BoundingBoxAsync() |> awaitTask
    require (not (isNull priceBox)) (label + " price chart must be visible")
    require (priceBox.X >= -0.5f && priceBox.X + priceBox.Width <= float32 viewportWidth + 0.5f) (label + " price chart exceeds viewport")
    require (priceBox.Y < float32 viewportHeight) (label + " primary chart must enter first viewport")

    page.Locator("[data-testid='ta-ptcs-deactivate']").ClickAsync() |> awaitUnit
    waitForPollState page "SUSPENDED"
    let pausedPoll = pollNumber (textOf (page.Locator("[data-testid='ta-freshness']")))
    Thread.Sleep 600
    require (pollNumber (textOf (page.Locator("[data-testid='ta-freshness']"))) = pausedPoll) (label + " deactivated channel continued polling")

    page.Locator("[data-testid='ta-ptcs-activate']").ClickAsync() |> awaitUnit
    let resumed = waitForPoll page (pausedPoll + 1)
    require (resumed > pausedPoll) (label + " activation did not resume polling")
    Directory.CreateDirectory outputDirectory |> ignore
    page.ScreenshotAsync(PageScreenshotOptions(Path = Path.Combine(outputDirectory, label + ".png"), FullPage = true)) |> awaitTask |> ignore
    if viewportWidth < 600 then
        page.Locator("[data-testid='ta-ptcs-deactivate']").ClickAsync() |> awaitUnit
        waitForPollState page "SUSPENDED"
        let beforeScrollPoll = pollNumber (textOf (page.Locator("[data-testid='ta-freshness']")))
        let sma = page.Locator("[data-testid='ta-row-sma']")
        sma.ScrollIntoViewIfNeededAsync() |> awaitUnit
        let smaBox = sma.BoundingBoxAsync() |> awaitTask
        require (not (isNull smaBox) && smaBox.Y < float32 viewportHeight && smaBox.Y + smaBox.Height > 0.0f) "mobile SMA row must be reachable through the extension scroll surface"
        page.ScreenshotAsync(PageScreenshotOptions(Path = Path.Combine(outputDirectory, label + "-sma.png"))) |> awaitTask |> ignore
        page.Locator("[data-testid='ta-ptcs-activate']").ClickAsync() |> awaitUnit
        waitForPoll page (beforeScrollPoll + 1) |> ignore
    page.Locator("[data-testid='ta-ptcs-dispose']").ClickAsync() |> awaitUnit
    waitForPollState page "DISPOSED"
    Thread.Sleep 300
    require (errors.Count = 0) (label + " console/page errors: " + String.concat " | " errors)
    context.CloseAsync() |> awaitUnit
    reached, resumed

let beforeEvents = eventCount ()
let playwright = Playwright.CreateAsync() |> awaitTask
let launch = BrowserTypeLaunchOptions(Headless = true)
if not (String.IsNullOrWhiteSpace browserExecutablePath) && File.Exists browserExecutablePath then
    launch.ExecutablePath <- browserExecutablePath
    launch.Args <- [| "--no-sandbox"; "--disable-dev-shm-usage" |]
let browser = playwright.Chromium.LaunchAsync(launch) |> awaitTask

try
    let desktopReached, desktopResumed = verify 1440 900 "desktop" browser
    let mobileReached, mobileResumed = verify 390 844 "mobile" browser
    let afterEvents = eventCount ()
    require (afterEvents = beforeEvents) $"PTCS transient polling mutated PCSL history: before={beforeEvents} after={afterEvents}"
    printfn "PTCS TA live Playwright PASS url=%s desktop=%d->%d mobile=%d->%d pcslEvents=%d output=%s" url desktopReached desktopResumed mobileReached mobileResumed afterEvents outputDirectory
finally
    browser.CloseAsync() |> awaitUnit
    playwright.Dispose()
