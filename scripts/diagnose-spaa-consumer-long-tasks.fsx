// CDP main-thread attribution for the existing SPAA RFC-TRADECORE-0026 browser host.

#i @"nuget: C:\Program Files\dotnet\sdk\10.0.401\FSharp\library-packs"
#r "nuget: FAkka.Argu, [10.1.301]"
#r "nuget: Microsoft.Playwright, 1.52.0"

#load "ParseLine.fsx"

open System
open System.Collections.Generic
open System.IO
open System.Text
open System.Text.Json
open System.Text.RegularExpressions
open System.Threading
open System.Threading.Tasks
open Argu
open Microsoft.Playwright

type Arguments =
    | Url of string
    | Output_Dir of string
    | Browser_Executable_Path of string
    | Timeout_Seconds of int
    | Headed
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Url _ -> "Existing SPAA browser host URL."
            | Output_Dir _ -> "Directory for deterministic CDP trace artifacts."
            | Browser_Executable_Path _ -> "Chrome or Edge executable path."
            | Timeout_Seconds _ -> "Bounded provider and browser wait timeout."
            | Headed -> "Run the browser headed."

let knownBrowserPaths =
    [ @"C:\Program Files\Google\Chrome\Application\chrome.exe"
      @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe"
      @"C:\Program Files\Microsoft\Edge\Application\msedge.exe"
      @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe" ]

let defaultBrowserPath = knownBrowserPaths |> List.tryFind File.Exists |> Option.defaultValue ""

let defaultArgumentsText =
    sprintf
        "--url \"http://127.0.0.1:18883/\" --output-dir \"artifacts/spaa-consumer-long-tasks\" --browser-executable-path \"%s\" --timeout-seconds 180"
        (defaultBrowserPath.Replace('\\', '/'))

let parser = ArgumentParser.Create<Arguments>(programName = "diagnose-spaa-consumer-long-tasks.fsx")
let defaults = parser.Parse(PL.parseLine [| ' ' |] (Some '"') None true defaultArgumentsText)
let automation = parser.Parse(fsi.CommandLineArgs |> Array.skip 1 |> Array.skipWhile ((=) "--"))
let pick tryAutomation tryDefault fallback =
    tryAutomation () |> Option.orElseWith tryDefault |> Option.defaultValue fallback

let url = pick (fun () -> automation.TryGetResult(<@ Url @>)) (fun () -> defaults.TryGetResult(<@ Url @>)) "http://127.0.0.1:18883/"
let outputDirectory =
    pick (fun () -> automation.TryGetResult(<@ Output_Dir @>)) (fun () -> defaults.TryGetResult(<@ Output_Dir @>)) "artifacts/spaa-consumer-long-tasks"
    |> Path.GetFullPath
let browserExecutablePath =
    pick (fun () -> automation.TryGetResult(<@ Browser_Executable_Path @>)) (fun () -> defaults.TryGetResult(<@ Browser_Executable_Path @>)) defaultBrowserPath
let timeoutSeconds = pick (fun () -> automation.TryGetResult(<@ Timeout_Seconds @>)) (fun () -> defaults.TryGetResult(<@ Timeout_Seconds @>)) 180
let headed = automation.Contains Headed || defaults.Contains Headed

let awaitTask (task: Task<'T>) = task.GetAwaiter().GetResult()
let awaitUnit (task: Task) = task.GetAwaiter().GetResult()
let require condition message = if not condition then failwith message
let textContent (locator: ILocator) = locator.TextContentAsync() |> awaitTask |> Option.ofObj |> Option.defaultValue ""
let attribute (locator: ILocator) name = locator.GetAttributeAsync(name) |> awaitTask |> Option.ofObj |> Option.defaultValue ""

let waitUntil label condition =
    let deadline = DateTimeOffset.UtcNow.AddSeconds(float timeoutSeconds)
    let mutable satisfied = condition ()
    while not satisfied && DateTimeOffset.UtcNow < deadline do
        Thread.Sleep 50
        satisfied <- condition ()
    require satisfied ("Timed out waiting for " + label)

type TraceCapture =
    { Completion: TaskCompletionSource<string>
      Emitter: ICDPSessionEvent
      Handler: EventHandler<Nullable<JsonElement>> }

let dictionary values =
    let result = Dictionary<string, obj>()
    for key, value in values do result[key] <- value
    result

let startTrace (session: ICDPSession) =
    let completion = TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously)
    let emitter = session.Event("Tracing.tracingComplete")
    let handler =
        EventHandler<Nullable<JsonElement>>(fun _ payload ->
            if payload.HasValue then
                let mutable stream = Unchecked.defaultof<JsonElement>
                if payload.Value.TryGetProperty("stream", &stream) && stream.ValueKind = JsonValueKind.String then
                    completion.TrySetResult(stream.GetString()) |> ignore)
    emitter.OnEvent.AddHandler handler
    session.SendAsync(
        "Tracing.start",
        dictionary
            [ ("categories", box "devtools.timeline,disabled-by-default-devtools.timeline")
              ("options", box "record-as-much-as-possible")
              ("transferMode", box "ReturnAsStream") ])
    |> awaitTask
    |> ignore
    { Completion = completion; Emitter = emitter; Handler = handler }

let stopTrace label (session: ICDPSession) capture =
    session.SendAsync("Tracing.end") |> awaitTask |> ignore
    let stream = capture.Completion.Task.WaitAsync(TimeSpan.FromSeconds 15.0) |> awaitTask
    capture.Emitter.OnEvent.RemoveHandler capture.Handler
    let buffer = StringBuilder()
    let mutable complete = false
    while not complete do
        let response = session.SendAsync("IO.read", dictionary [ ("handle", box stream) ]) |> awaitTask
        require response.HasValue "CDP IO.read returned no trace payload"
        let root = response.Value
        let data = root.GetProperty("data").GetString()
        let mutable base64Encoded = Unchecked.defaultof<JsonElement>
        if root.TryGetProperty("base64Encoded", &base64Encoded) && base64Encoded.GetBoolean() then
            Convert.FromBase64String(data) |> Encoding.UTF8.GetString |> buffer.Append |> ignore
        else
            buffer.Append(data) |> ignore
        let mutable eof = Unchecked.defaultof<JsonElement>
        complete <- root.TryGetProperty("eof", &eof) && eof.GetBoolean()
    session.SendAsync("IO.close", dictionary [ ("handle", box stream) ]) |> awaitTask |> ignore

    Directory.CreateDirectory outputDirectory |> ignore
    let tracePath = Path.Combine(outputDirectory, "trace-" + label + ".json")
    File.WriteAllText(tracePath, buffer.ToString(), UTF8Encoding(false))
    use document = JsonDocument.Parse(buffer.ToString())
    let events = document.RootElement.GetProperty("traceEvents").EnumerateArray() |> Seq.toArray
    let threadKey (event: JsonElement) =
        string (event.GetProperty("pid").GetInt32()) + ":" + string (event.GetProperty("tid").GetInt32())
    let mainThreads =
        events
        |> Array.choose (fun event ->
            let mutable name = Unchecked.defaultof<JsonElement>
            let mutable args = Unchecked.defaultof<JsonElement>
            let mutable threadName = Unchecked.defaultof<JsonElement>
            if event.TryGetProperty("name", &name)
               && name.GetString() = "thread_name"
               && event.TryGetProperty("args", &args)
               && args.TryGetProperty("name", &threadName)
               && threadName.ValueKind = JsonValueKind.String
               && threadName.GetString().Contains("RendererMain", StringComparison.Ordinal) then
                Some(threadKey event)
            else None)
        |> Set.ofArray
    let tryNumber (propertyName: string) (event: JsonElement) =
        let mutable value = Unchecked.defaultof<JsonElement>
        if event.TryGetProperty(propertyName, &value) && value.ValueKind = JsonValueKind.Number then Some(value.GetDouble())
        else None
    let runTasks =
        events
        |> Array.choose (fun event ->
            let mutable name = Unchecked.defaultof<JsonElement>
            let mutable phase = Unchecked.defaultof<JsonElement>
            if mainThreads.Contains(threadKey event)
               && event.TryGetProperty("name", &name)
               && name.GetString().EndsWith("RunTask", StringComparison.Ordinal)
               && event.TryGetProperty("ph", &phase)
               && phase.GetString() = "X" then
                match tryNumber "ts" event, tryNumber "dur" event with
                | Some startedAt, Some duration -> Some(threadKey event, startedAt, duration)
                | _ -> None
            else None)
    let overBudget = runTasks |> Array.filter (fun (_, _, duration) -> duration > 100000.0)
    for thread, startedAt, duration in overBudget |> Array.sortByDescending (fun (_, _, duration) -> duration) do
        let taskEnd = startedAt + duration
        let children =
            events
            |> Array.choose (fun event ->
                let mutable name = Unchecked.defaultof<JsonElement>
                let mutable phase = Unchecked.defaultof<JsonElement>
                if threadKey event = thread
                   && event.TryGetProperty("name", &name)
                   && not (name.GetString().EndsWith("RunTask", StringComparison.Ordinal))
                   && event.TryGetProperty("ph", &phase)
                   && phase.GetString() = "X" then
                    match tryNumber "ts" event, tryNumber "dur" event with
                    | Some childStart, Some childDuration
                        when childStart >= startedAt && childStart + childDuration <= taskEnd && childDuration >= 1000.0 ->
                        Some(name.GetString(), childDuration / 1000.0)
                    | _ -> None
                else None)
            |> Array.sortByDescending snd
            |> Array.truncate 12
            |> Array.map (fun (name, milliseconds) -> $"{name}:{milliseconds:F2}ms")
            |> String.concat ", "
        printfn "spaa.long-task.detail phase=%s task=%.2fms children=[%s]" label (duration / 1000.0) children
    let maximum = runTasks |> Array.fold (fun current (_, _, duration) -> max current (duration / 1000.0)) 0.0
    printfn "spaa.long-task phase=%s runTasks=%d over100=%d max=%.2fms trace=%s" label runTasks.Length overBudget.Length maximum tracePath

let loadedPattern = Regex("Loaded (?<total>[0-9]+) bars · Viewing (?<first>[0-9]+)-(?<last>[0-9]+)", RegexOptions.CultureInvariant)

let playwright = Playwright.CreateAsync() |> awaitTask
let launch = BrowserTypeLaunchOptions(Headless = not headed, ExecutablePath = browserExecutablePath)
launch.Args <- [| "--no-sandbox"; "--disable-dev-shm-usage" |]
let browser = playwright.Chromium.LaunchAsync(launch) |> awaitTask
let context = browser.NewContextAsync(BrowserNewContextOptions(ViewportSize = ViewportSize(Width = 1600, Height = 1400))) |> awaitTask
let page = context.NewPageAsync() |> awaitTask
page.SetDefaultTimeout(float32 (min timeoutSeconds 15 * 1000))
page.SetDefaultNavigationTimeout(float32 (timeoutSeconds * 1000))
let session = context.NewCDPSessionAsync(page) |> awaitTask

let initial = startTrace session
page.GotoAsync(url, PageGotoOptions(WaitUntil = WaitUntilState.DOMContentLoaded)) |> awaitTask |> ignore
page.GetByRole(AriaRole.Button, PageGetByRoleOptions(Name = "Run", Exact = true)).ClickAsync() |> awaitUnit
let loaded = page.GetByText(Regex("Loaded [1-9][0-9]* bars"))
loaded.WaitForAsync(LocatorWaitForOptions(State = WaitForSelectorState.Visible, Timeout = float32 (timeoutSeconds * 1000))) |> awaitUnit
let loadedMatch = loadedPattern.Match(textContent loaded)
require loadedMatch.Success "Loaded-range label is malformed"
let totalBars = Int32.Parse loadedMatch.Groups["total"].Value
let chartStack = page.Locator("[data-testid='ta-chart-stack']")
waitUntil "scheduled chart rows" (fun () -> attribute chartStack "data-ready-row-count" = attribute chartStack "data-row-count")
Thread.Sleep 180
stopTrace "initial-run" session initial

page.Locator("[data-testid='ta-view-48']").ClickAsync() |> awaitUnit
let all = startTrace session
page.Locator("[data-testid='ta-view-all']").ClickAsync() |> awaitUnit
waitUntil "complete loaded viewport" (fun () ->
    let current = loadedPattern.Match(textContent loaded)
    current.Success && current.Groups["first"].Value = "1" && current.Groups["last"].Value = string totalBars)
waitUntil "all scheduled chart rows" (fun () -> attribute chartStack "data-ready-row-count" = attribute chartStack "data-row-count")
Thread.Sleep 180
stopTrace "48-to-all" session all

let backtest = startTrace session
page.Locator("[data-testid='run-backtest']").ClickAsync() |> awaitUnit
let backtestStatus = page.Locator("[data-testid='backtest-status']")
waitUntil "selected Backtest result" (fun () -> textContent backtestStatus |> _.StartsWith("SELECTED /", StringComparison.Ordinal))
let marker = page.Locator("[data-marker-count]:not([data-marker-count='0'])").First.Locator("polygon").First
marker.ScrollIntoViewIfNeededAsync() |> awaitUnit
let markerBounds = marker.BoundingBoxAsync() |> awaitTask
require (not (isNull markerBounds)) "The first marker has no browser bounds"
let decisionChart = page.Locator("[data-testid='ta-candle-price-60k']")
let decisionBounds = decisionChart.BoundingBoxAsync() |> awaitTask
require (not (isNull decisionBounds)) "The decision chart has no browser bounds"
page.Mouse.ClickAsync(markerBounds.X + markerBounds.Width * 0.5f, decisionBounds.Y + decisionBounds.Height * 0.5f) |> awaitUnit
let accounting = page.Locator("[data-testid='backtest-cursor-accounting']")
waitUntil "cursor accounting" (fun () -> textContent accounting |> _.Contains("WORKING ORDERS", StringComparison.OrdinalIgnoreCase))
Thread.Sleep 180
stopTrace "backtest-accounting" session backtest

printfn "SPAA_CONSUMER_LONG_TASK_DIAGNOSTIC_PASS bars=%d" totalBars
context.CloseAsync() |> awaitUnit
browser.CloseAsync() |> awaitUnit
playwright.Dispose()
