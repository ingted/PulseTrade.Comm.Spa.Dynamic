// Real-browser operation and geometry verifier for the pure WebSharper TA renderer demo.

#i @"nuget: C:\Program Files\dotnet\sdk\10.0.401\FSharp\library-packs"
#r "nuget: FAkka.Argu, [10.1.301]"
#r "nuget: Microsoft.Playwright, 1.52.0"

#load "ParseLine.fsx"

open System
open System.IO
open System.Collections.Generic
open System.Text
open System.Text.Json
open System.Threading.Tasks
open Argu
open Microsoft.Playwright

type CliArgs =
    | Url of string
    | Output_Dir of string
    | Browser_Executable_Path of string
    | Headed
    | Skip_Performance_Gates
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Url _ -> "Existing TA renderer browser-demo URL."
            | Output_Dir _ -> "Directory for deterministic desktop/mobile screenshots."
            | Browser_Executable_Path _ -> "Chrome or Edge executable path."
            | Headed -> "Run the browser headed."
            | Skip_Performance_Gates -> "Run functional and geometry gates while reporting, but not enforcing, host-load-sensitive performance limits."

let knownBrowserPaths =
    [ @"C:\Program Files\Google\Chrome\Application\chrome.exe"
      @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe"
      @"C:\Program Files\Microsoft\Edge\Application\msedge.exe"
      @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe" ]

let defaultBrowserPath =
    knownBrowserPaths |> List.tryFind File.Exists |> Option.defaultValue ""

let defaultBrowserArgument = defaultBrowserPath.Replace('\\', '/')

let defaultArgumentsText =
    sprintf
        "--url \"http://127.0.0.1:18882/\" --output-dir \"artifacts/ta-renderer-playwright\" --browser-executable-path \"%s\""
        defaultBrowserArgument

let parser = ArgumentParser.Create<CliArgs>(programName = "verify-ta-renderer-playwright.fsx")
let defaultArguments = PL.parseLine [| ' ' |] (Some '"') None true defaultArgumentsText
let automationArguments = fsi.CommandLineArgs |> Array.skip 1 |> Array.skipWhile ((=) "--")
let defaults = parser.Parse defaultArguments
let automation = parser.Parse automationArguments
let pick tryAutomation tryDefault fallback =
    tryAutomation ()
    |> Option.orElseWith tryDefault
    |> Option.defaultValue fallback

let url = pick (fun () -> automation.TryGetResult(<@ Url @>)) (fun () -> defaults.TryGetResult(<@ Url @>)) "http://127.0.0.1:18882/"
let outputDirectory = pick (fun () -> automation.TryGetResult(<@ Output_Dir @>)) (fun () -> defaults.TryGetResult(<@ Output_Dir @>)) "artifacts/ta-renderer-playwright" |> Path.GetFullPath
let browserExecutablePath = pick (fun () -> automation.TryGetResult(<@ Browser_Executable_Path @>)) (fun () -> defaults.TryGetResult(<@ Browser_Executable_Path @>)) defaultBrowserPath
let headed = automation.Contains Headed || defaults.Contains Headed
let skipPerformanceGates = automation.Contains Skip_Performance_Gates || defaults.Contains Skip_Performance_Gates

let awaitTask (task: Task<'T>) = task.GetAwaiter().GetResult()
let awaitUnit (task: Task) = task.GetAwaiter().GetResult()

let require condition message =
    if not condition then failwith ("TA renderer Playwright verification failed: " + message)

let capacityPointCount = 4000
let capacitySeriesCount = 28
let visiblePointCount = 48
let initialVisibleStart = capacityPointCount - visiblePointCount + 1

let textOf (locator: ILocator) =
    locator.TextContentAsync() |> awaitTask |> Option.ofObj |> Option.defaultValue ""

let requireText (locator: ILocator) (expected: string) =
    let actualText = textOf locator
    require (actualText.Contains expected) $"expected `{expected}` in `{actualText}`"

let requiredIntAttribute (locator: ILocator) name =
    let value = locator.GetAttributeAsync(name) |> awaitTask |> Option.ofObj |> Option.defaultValue ""
    match Int32.TryParse value with
    | true, parsed -> parsed
    | _ -> failwith $"TA renderer Playwright verification failed: `{name}` is not an integer: `{value}`"

let requiredFloatAttribute (locator: ILocator) name =
    let value = locator.GetAttributeAsync(name) |> awaitTask |> Option.ofObj |> Option.defaultValue ""
    match Double.TryParse(value, Globalization.NumberStyles.Float, Globalization.CultureInfo.InvariantCulture) with
    | true, parsed -> parsed
    | _ -> failwith $"TA renderer Playwright verification failed: `{name}` is not a number: `{value}`"

let attributeOrEmpty (locator: ILocator) name =
    locator.GetAttributeAsync(name) |> awaitTask |> Option.ofObj |> Option.defaultValue ""

let printVisibleValueTelemetry label (page: IPage) (chartStack: ILocator) =
    let root = page.Locator("html")
    printfn
        "browser.visible-values phase=%s instance=%s sequence=%s queryMs=%s resolveMs=%s writeMs=%s visibilityMs=%s totalMs=%s publicationMs=%s schedulerMs=%s maxTotalMs=%s textWrites=%s attributeWrites=%s visibilityWrites=%s nodes=%s globalLastMs=%s globalMaxMs=%s globalLastInstance=%s globalMaxInstance=%s globalLastRender=%s globalMaxRender=%s globalLastScheduler=%s globalMaxScheduler=%s"
        label
        (attributeOrEmpty chartStack "data-visible-value-renderer-instance")
        (attributeOrEmpty chartStack "data-visible-value-telemetry-sequence")
        (attributeOrEmpty chartStack "data-visible-value-query-ms")
        (attributeOrEmpty chartStack "data-visible-value-resolve-ms")
        (attributeOrEmpty chartStack "data-visible-value-write-ms")
        (attributeOrEmpty chartStack "data-visible-value-visibility-ms")
        (attributeOrEmpty chartStack "data-visible-value-total-ms")
        (attributeOrEmpty chartStack "data-visible-value-publication-ms")
        (attributeOrEmpty chartStack "data-visible-value-scheduler-ms")
        (attributeOrEmpty chartStack "data-visible-value-max-total-ms")
        (attributeOrEmpty chartStack "data-visible-value-text-writes")
        (attributeOrEmpty chartStack "data-visible-value-attribute-writes")
        (attributeOrEmpty chartStack "data-visible-value-visibility-writes")
        (attributeOrEmpty chartStack "data-visible-value-node-count")
        (attributeOrEmpty root "data-visible-value-global-last-scheduler-ms")
        (attributeOrEmpty root "data-visible-value-global-max-scheduler-ms")
        (attributeOrEmpty root "data-visible-value-global-last-instance")
        (attributeOrEmpty root "data-visible-value-global-max-instance")
        (attributeOrEmpty root "data-visible-value-global-last-render-sequence")
        (attributeOrEmpty root "data-visible-value-global-max-render-sequence")
        (attributeOrEmpty root "data-visible-value-global-last-scheduler-sequence")
        (attributeOrEmpty root "data-visible-value-global-max-scheduler-sequence")

let waitForIntAttribute (locator: ILocator) name expected =
    let deadline = DateTime.UtcNow.AddSeconds 8.0
    let mutable actual = requiredIntAttribute locator name

    while actual <> expected && DateTime.UtcNow < deadline do
        Threading.Thread.Sleep 40
        actual <- requiredIntAttribute locator name

    require (actual = expected) $"expected `{name}`={expected}, actual={actual}"

let waitForIntAttributeAtLeast (locator: ILocator) name minimum =
    let deadline = DateTime.UtcNow.AddSeconds 8.0
    let mutable actual = requiredIntAttribute locator name

    while actual < minimum && DateTime.UtcNow < deadline do
        Threading.Thread.Sleep 25
        actual <- requiredIntAttribute locator name

    require (actual >= minimum) $"expected `{name}` >= {minimum}, actual={actual}"
    actual

let waitForStableIntAttribute (locator: ILocator) name =
    let deadline = DateTime.UtcNow.AddSeconds 8.0
    let mutable actual = requiredIntAttribute locator name
    let mutable stableSamples = 0

    while stableSamples < 3 && DateTime.UtcNow < deadline do
        Threading.Thread.Sleep 25
        let next = requiredIntAttribute locator name
        if next = actual then
            stableSamples <- stableSamples + 1
        else
            actual <- next
            stableSamples <- 0

    require (stableSamples >= 3) $"`{name}` did not stabilize, last={actual}"
    actual

let waitForAttributeChange (locator: ILocator) name previous =
    let deadline = DateTime.UtcNow.AddSeconds 8.0
    let mutable actual = locator.GetAttributeAsync(name) |> awaitTask |> Option.ofObj |> Option.defaultValue ""

    while actual = previous && DateTime.UtcNow < deadline do
        actual <- locator.GetAttributeAsync(name) |> awaitTask |> Option.ofObj |> Option.defaultValue ""

    require (actual <> previous) $"expected `{name}` to change from `{previous}`"
    actual

let waitForAttributeValue (locator: ILocator) name expected =
    let deadline = DateTime.UtcNow.AddSeconds 8.0
    let mutable actual = locator.GetAttributeAsync(name) |> awaitTask |> Option.ofObj |> Option.defaultValue ""

    while actual <> expected && DateTime.UtcNow < deadline do
        Threading.Thread.Sleep 25
        actual <- locator.GetAttributeAsync(name) |> awaitTask |> Option.ofObj |> Option.defaultValue ""

    require (actual = expected) $"expected `{name}`={expected}, actual={actual}"

let waitForCount (locator: ILocator) expected =
    let deadline = DateTime.UtcNow.AddSeconds 8.0
    let mutable actual = locator.CountAsync() |> awaitTask

    while actual <> expected && DateTime.UtcNow < deadline do
        Threading.Thread.Sleep 25
        actual <- locator.CountAsync() |> awaitTask

    require (actual = expected) $"expected locator count={expected}, actual={actual}"

let waitForText (locator: ILocator) (expected: string) =
    let deadline = DateTime.UtcNow.AddSeconds 8.0
    let mutable matched = false

    while not matched && DateTime.UtcNow < deadline do
        let actualText = locator.TextContentAsync() |> awaitTask |> Option.ofObj |> Option.defaultValue ""
        matched <- actualText.Contains expected
        if not matched then Threading.Thread.Sleep 50

    requireText locator expected

let waitForTextChange (locator: ILocator) previous =
    let deadline = DateTime.UtcNow.AddSeconds 8.0
    let mutable actual = textOf locator

    while actual = previous && DateTime.UtcNow < deadline do
        Threading.Thread.Sleep 10
        actual <- textOf locator

    require (actual <> previous) $"expected text to change from `{previous}`"
    actual

let waitForEnabled (locator: ILocator) label =
    let deadline = DateTime.UtcNow.AddSeconds 8.0
    let mutable enabled = locator.IsEnabledAsync() |> awaitTask

    while not enabled && DateTime.UtcNow < deadline do
        Threading.Thread.Sleep 40
        enabled <- locator.IsEnabledAsync() |> awaitTask

    require enabled (label + " did not become enabled")

let waitForDisabled (locator: ILocator) label =
    let deadline = DateTime.UtcNow.AddSeconds 8.0
    let mutable disabled = locator.IsDisabledAsync() |> awaitTask

    while not disabled && DateTime.UtcNow < deadline do
        Threading.Thread.Sleep 40
        disabled <- locator.IsDisabledAsync() |> awaitTask

    require disabled (label + " did not become disabled")

let requireBoxInside viewportWidth label (box: LocatorBoundingBoxResult) =
    require (not (isNull box)) (label + " has no bounding box")
    require (box.X >= -0.5f) $"{label} starts outside viewport: x={box.X}"
    require (box.X + box.Width <= float32 viewportWidth + 0.5f) $"{label} exceeds viewport: right={box.X + box.Width}, viewport={viewportWidth}"

type CdpTraceCapture =
    { Completion: TaskCompletionSource<string>
      Emitter: ICDPSessionEvent
      Handler: EventHandler<Nullable<JsonElement>> }

let dictionary values =
    let result = Dictionary<string, obj>()
    for key, value in values do result[key] <- value
    result

let computedStyleProperties (session: ICDPSession) selector propertyNames =
    let rec read attempt =
        try
            let document = session.SendAsync("DOM.getDocument") |> awaitTask
            require document.HasValue "CDP DOM.getDocument returned no payload"
            let rootNodeId = document.Value.GetProperty("root").GetProperty("nodeId").GetInt32()
            let query =
                session.SendAsync(
                    "DOM.querySelector",
                    dictionary [ "nodeId", box rootNodeId; "selector", box selector ])
                |> awaitTask
            require query.HasValue $"CDP DOM.querySelector returned no payload for {selector}"
            let nodeId = query.Value.GetProperty("nodeId").GetInt32()
            require (nodeId > 0) $"CDP DOM.querySelector did not find {selector}"
            let response =
                session.SendAsync("CSS.getComputedStyleForNode", dictionary (List.singleton ("nodeId", box nodeId)))
                |> awaitTask
            require response.HasValue $"CDP CSS.getComputedStyleForNode returned no payload for {selector}"
            let selected = propertyNames |> Set.ofArray
            response.Value.GetProperty("computedStyle").EnumerateArray()
            |> Seq.choose (fun entry ->
                let name = entry.GetProperty("name").GetString()
                if selected.Contains name then Some(name, entry.GetProperty("value").GetString()) else None)
            |> Map.ofSeq
        with
        | :? PlaywrightException when attempt < 3 ->
            Threading.Thread.Sleep 25
            read (attempt + 1)
    read 1

let requireFixedCssStroke (session: ICDPSession) selector (locator: ILocator) minimum maximum =
    let widthText = attributeOrEmpty locator "data-stroke-width-css-pixels"
    let mutable width = 0.0
    require
        (Double.TryParse(widthText, Globalization.NumberStyles.Float, Globalization.CultureInfo.InvariantCulture, &width))
        $"{selector} must publish numeric data-stroke-width-css-pixels, actual={widthText}"
    require (width >= minimum && width <= maximum) $"{selector} stroke width must stay within {minimum}-{maximum} CSS px, actual={width}"
    require (attributeOrEmpty locator "vector-effect" = "non-scaling-stroke") $"{selector} must use non-scaling-stroke"
    let computed = computedStyleProperties session selector [| "stroke-width" |]
    let expected = widthText + "px"
    require
        (Map.tryFind "stroke-width" computed = Some expected)
        $"{selector} computed stroke width must be {expected}, actual={computed}"
    computed

let rowControlIds = [| "price"; "volume"; "sma"; "dmi"; "adx"; "macd"; "heikin" |]

let verifyRowControlLines viewportLabel viewportWidth (session: ICDPSession) (page: IPage) =
    let lines = page.Locator("[data-testid^='ta-row-control-line-']")
    require (lines.CountAsync() |> awaitTask = rowControlIds.Length) $"{viewportLabel} must render one control line per authored row"

    let mutable previousBottom = -1.0f
    for rowId in rowControlIds do
        let line = page.Locator($"[data-testid='ta-row-control-line-{rowId}']")
        let rowControls = line.Locator($":scope > [data-testid='ta-row-controls-{rowId}']")
        let traceRegion = line.Locator($":scope > [data-testid='ta-trace-toggles-{rowId}']")
        require (line.CountAsync() |> awaitTask = 1) $"{viewportLabel} row {rowId} must have exactly one stable control line"
        require (rowControls.CountAsync() |> awaitTask = 1) $"{viewportLabel} row {rowId} controls must be owned by its line"
        require (traceRegion.CountAsync() |> awaitTask = 1) $"{viewportLabel} row {rowId} trace controls must be owned by its line"
        require (rowControls.Locator($"[data-testid='ta-toggle-row-{rowId}']").CountAsync() |> awaitTask = 1) $"{viewportLabel} row {rowId} visibility control escaped its line"

        let lineBox = line.BoundingBoxAsync() |> awaitTask
        requireBoxInside viewportWidth ($"{viewportLabel} row control line {rowId}") lineBox
        require (abs (lineBox.Height - 40.0f) <= 0.5f) $"{viewportLabel} row {rowId} control line must remain fixed at 40px, actual={lineBox.Height}"
        require (lineBox.Y > previousBottom + 0.5f) $"{viewportLabel} row {rowId} must occupy a separate Y band"
        previousBottom <- lineBox.Y + lineBox.Height

        let traceButtons = traceRegion.Locator("[data-testid^='ta-toggle-trace-']")
        require (traceButtons.CountAsync() |> awaitTask > 0) $"{viewportLabel} fixture row {rowId} must expose a controllable trace"
        let editButtons = rowControls.Locator("[data-testid^='ta-edit-row-']")
        if editButtons.CountAsync() |> awaitTask > 0 then
            let editBox = editButtons.First.BoundingBoxAsync() |> awaitTask
            require (not (isNull editBox)) $"{viewportLabel} row {rowId} Edit control must expose geometry"
            require (editBox.Width >= 51.0f) $"{viewportLabel} row {rowId} Edit control must retain its fixed visible width"
            require (editBox.X >= lineBox.X - 0.5f) $"{viewportLabel} row {rowId} Edit control begins outside its row line"
            require (editBox.X + editBox.Width <= lineBox.X + lineBox.Width + 0.5f) $"{viewportLabel} row {rowId} Edit control is clipped by its row line"
            let labelControl = rowControls.Locator($"[data-testid='ta-toggle-row-{rowId}']")
            require (not (String.IsNullOrWhiteSpace(attributeOrEmpty labelControl "title"))) $"{viewportLabel} row {rowId} truncated label must retain a full title"
        let firstTraceBox = traceButtons.First.BoundingBoxAsync() |> awaitTask
        require (not (isNull firstTraceBox)) $"{viewportLabel} row {rowId} first trace control must expose geometry"
        for traceIndex in 1 .. (traceButtons.CountAsync() |> awaitTask) - 1 do
            let traceBox = traceButtons.Nth(traceIndex).BoundingBoxAsync() |> awaitTask
            require (not (isNull traceBox)) $"{viewportLabel} row {rowId} trace {traceIndex} must expose geometry"
            require (abs (traceBox.Y - firstTraceBox.Y) <= 0.5f) $"{viewportLabel} row {rowId} trace controls must share one Y band"

    require (page.Locator("[data-testid='ta-toggle-trace-price-signals']").CountAsync() |> awaitTask = 0) $"{viewportLabel} Marker system trace must not enter controls"
    require (page.Locator("[data-testid='ta-toggle-trace-price-overview-order']").CountAsync() |> awaitTask = 0) $"{viewportLabel} OverviewStripe system trace must not enter controls"
    require (page.Locator("[data-testid='ta-toggle-trace-price-overview-fill']").CountAsync() |> awaitTask = 0) $"{viewportLabel} OverviewStripe system trace must not enter controls"

    let traceStyle =
        computedStyleProperties
            session
            "[data-testid='ta-trace-toggles-macd']"
            [| "display"; "flex-wrap"; "overflow-x"; "overflow-y" |]
    require (Map.tryFind "display" traceStyle = Some "flex") $"{viewportLabel} trace region must remain flex, actual={traceStyle}"
    require (Map.tryFind "flex-wrap" traceStyle = Some "nowrap") $"{viewportLabel} trace region must not wrap, actual={traceStyle}"
    require (Map.tryFind "overflow-x" traceStyle = Some "auto") $"{viewportLabel} trace region must own horizontal overflow, actual={traceStyle}"
    require (Map.tryFind "overflow-y" traceStyle = Some "hidden") $"{viewportLabel} trace region must not create vertical overflow, actual={traceStyle}"
    let traceStyleAttribute = attributeOrEmpty (page.Locator("[data-testid='ta-trace-toggles-macd']")) "style"
    require (traceStyleAttribute.Replace(" ", "").Contains("white-space:nowrap", StringComparison.OrdinalIgnoreCase)) $"{viewportLabel} trace text must stay on one line, actual={traceStyleAttribute}"

    if viewportWidth <= 390 then
        let macdRegion = page.Locator("[data-testid='ta-trace-toggles-macd']")
        let macdButtons = macdRegion.Locator("[data-testid^='ta-toggle-trace-']")
        let regionBox = macdRegion.BoundingBoxAsync() |> awaitTask
        let lastBox = macdButtons.Last.BoundingBoxAsync() |> awaitTask
        require (not (isNull regionBox) && not (isNull lastBox)) "narrow MACD trace controls must expose geometry"
        require (lastBox.X + lastBox.Width > regionBox.X + regionBox.Width + 1.0f) "narrow MACD controls must overflow only inside their row-local scroll region"

let startMainThreadTrace (session: ICDPSession) =
    let completion = TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously)
    let emitter = session.Event("Tracing.tracingComplete")
    let handler =
        EventHandler<Nullable<JsonElement>>(fun _ payload ->
            if payload.HasValue then
                let root = payload.Value
                let mutable stream = Unchecked.defaultof<JsonElement>
                if root.TryGetProperty("stream", &stream) && stream.ValueKind = JsonValueKind.String then
                    completion.TrySetResult(stream.GetString()) |> ignore)
    emitter.OnEvent.AddHandler handler
    session.SendAsync(
        "Tracing.start",
        dictionary
            [ "categories", box "devtools.timeline,disabled-by-default-devtools.timeline"
              "options", box "record-as-much-as-possible"
              "transferMode", box "ReturnAsStream" ])
    |> awaitTask
    |> ignore
    { Completion = completion; Emitter = emitter; Handler = handler }

let stopMainThreadTrace label (session: ICDPSession) capture =
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
    let traceJson = buffer.ToString()
    let tracePath = Path.Combine(outputDirectory, $"trace-{label}.json")
    File.WriteAllText(tracePath, traceJson, UTF8Encoding(false))
    use document = JsonDocument.Parse(traceJson)
    let events = document.RootElement.GetProperty("traceEvents").EnumerateArray() |> Seq.toArray
    let threadKey (event: JsonElement) =
        string (event.GetProperty("pid").GetInt32()) + ":" + string (event.GetProperty("tid").GetInt32())
    let targetRendererProcesses =
        events
        |> Array.collect (fun event ->
            let mutable name = Unchecked.defaultof<JsonElement>
            let mutable args = Unchecked.defaultof<JsonElement>
            let mutable data = Unchecked.defaultof<JsonElement>
            let mutable frames = Unchecked.defaultof<JsonElement>
            if event.TryGetProperty("name", &name)
               && name.GetString() = "TracingStartedInBrowser"
               && event.TryGetProperty("args", &args)
               && args.TryGetProperty("data", &data)
               && data.TryGetProperty("frames", &frames)
               && frames.ValueKind = JsonValueKind.Array then
                frames.EnumerateArray()
                |> Seq.choose (fun frame ->
                    let mutable processId = Unchecked.defaultof<JsonElement>
                    let mutable primary = Unchecked.defaultof<JsonElement>
                    let mutable outermost = Unchecked.defaultof<JsonElement>
                    if frame.TryGetProperty("processId", &processId)
                       && frame.TryGetProperty("isInPrimaryMainFrame", &primary)
                       && primary.GetBoolean()
                       && frame.TryGetProperty("isOutermostMainFrame", &outermost)
                       && outermost.GetBoolean() then
                        Some(processId.GetInt32())
                    else
                        None)
                |> Seq.toArray
            else
                [||])
        |> Set.ofArray
    require
        (not targetRendererProcesses.IsEmpty)
        $"CDP trace `{label}` did not identify the target primary renderer process"
    let rendererMainThreads =
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
               && targetRendererProcesses.Contains(event.GetProperty("pid").GetInt32())
               && threadName.GetString().Contains("RendererMain", StringComparison.Ordinal) then
                Some(threadKey event)
            else None)
        |> Set.ofArray
    let tryNumber (propertyName: string) (event: JsonElement) =
        let mutable value = Unchecked.defaultof<JsonElement>
        if event.TryGetProperty(propertyName, &value) && value.ValueKind = JsonValueKind.Number then
            Some(value.GetDouble())
        else
            None
    let eventDescription (event: JsonElement) =
        let name = event.GetProperty("name").GetString()
        let mutable args = Unchecked.defaultof<JsonElement>
        let mutable data = Unchecked.defaultof<JsonElement>
        let mutable functionName = Unchecked.defaultof<JsonElement>
        let mutable url = Unchecked.defaultof<JsonElement>
        let mutable lineNumber = Unchecked.defaultof<JsonElement>
        if event.TryGetProperty("args", &args)
           && args.TryGetProperty("data", &data) then
            let functionText =
                if data.TryGetProperty("functionName", &functionName) && functionName.ValueKind = JsonValueKind.String then
                    functionName.GetString()
                else
                    ""
            let urlText =
                if data.TryGetProperty("url", &url) && url.ValueKind = JsonValueKind.String then
                    url.GetString()
                else
                    ""
            let lineText =
                if data.TryGetProperty("lineNumber", &lineNumber) && lineNumber.ValueKind = JsonValueKind.Number then
                    string (lineNumber.GetInt32() + 1)
                else
                    ""
            if String.IsNullOrWhiteSpace functionText && String.IsNullOrWhiteSpace urlText then name
            else $"{name}[{functionText}@{urlText}:{lineText}]"
        else
            name
    let runTasks =
        events
        |> Array.choose (fun event ->
            let mutable name = Unchecked.defaultof<JsonElement>
            let mutable phase = Unchecked.defaultof<JsonElement>
            if rendererMainThreads.Contains(threadKey event)
               && event.TryGetProperty("name", &name)
               && name.GetString().EndsWith("RunTask", StringComparison.Ordinal)
               && event.TryGetProperty("ph", &phase)
               && phase.GetString() = "X" then
                match tryNumber "ts" event, tryNumber "dur" event with
                | Some startedAt, Some duration -> Some(threadKey event, startedAt, duration)
                | _ -> None
            else None)
    let overBudgetTasks = runTasks |> Array.filter (fun (_, _, duration) -> duration > 100000.0)
    for thread, startedAt, duration in overBudgetTasks |> Array.sortByDescending (fun (_, _, duration) -> duration) do
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
                        Some(eventDescription event, childDuration / 1000.0)
                    | _ -> None
                else
                    None)
            |> Array.sortByDescending snd
            |> Array.truncate 8
            |> Array.map (fun (name, milliseconds) -> $"{name}:{milliseconds:F2}ms")
            |> String.concat ", "
        printfn "browser.long-task.detail phase=%s task=%.2fms children=[%s]" label (duration / 1000.0) children
    let overBudget = overBudgetTasks |> Array.map (fun (_, _, duration) -> duration / 1000.0)
    let maximum = runTasks |> Array.fold (fun current (_, _, duration) -> max current (duration / 1000.0)) 0.0
    printfn "browser.long-task phase=%s runTasks=%d over100=%d max=%.2fms" label runTasks.Length overBudget.Length maximum
    label, overBudget, maximum

let attributeSignature (locator: ILocator) name =
    [| for index in 0 .. (locator.CountAsync() |> awaitTask) - 1 do
           yield locator.Nth(index).GetAttributeAsync(name) |> awaitTask |> Option.ofObj |> Option.defaultValue "" |]
    |> String.concat "|"

let waitForAttributeSignatureChange (locator: ILocator) name previous =
    let deadline = DateTime.UtcNow.AddSeconds 8.0
    let mutable actual = attributeSignature locator name
    while actual = previous && DateTime.UtcNow < deadline do
        Threading.Thread.Sleep 25
        actual <- attributeSignature locator name
    require (actual <> previous) $"expected `{name}` signature to change"
    actual

let requireCandleEdgePadding label expected tolerance (chart: ILocator) (paths: ILocator) =
    let chartBox = chart.BoundingBoxAsync() |> awaitTask
    require (not (isNull chartBox)) (label + " chart must expose geometry")
    let pathBoxes =
        [| for index in 0 .. (paths.CountAsync() |> awaitTask) - 1 do
               let pathBox = paths.Nth(index).BoundingBoxAsync() |> awaitTask
               if not (isNull pathBox) && pathBox.Height > 0.0f then
                   yield pathBox |]
    require (pathBoxes.Length > 0) (label + " must expose non-empty candle geometry")
    let candleTop = pathBoxes |> Array.minBy (fun box -> box.Y) |> fun box -> box.Y
    let candleBottom = pathBoxes |> Array.maxBy (fun box -> box.Y + box.Height) |> fun box -> box.Y + box.Height
    let topGap = float (candleTop - chartBox.Y)
    let bottomGap = float ((chartBox.Y + chartBox.Height) - candleBottom)
    printfn "browser.candle-edge-padding phase=%s top=%.2fpx bottom=%.2fpx" label topGap bottomGap
    require (abs (topGap - expected) <= tolerance) $"{label} top edge padding expected {expected}+/-{tolerance}px, actual={topGap:F2}px"
    require (abs (bottomGap - expected) <= tolerance) $"{label} bottom edge padding expected {expected}+/-{tolerance}px, actual={bottomGap:F2}px"

let verifyDesktop (browser: IBrowser) =
    let context = browser.NewContextAsync(BrowserNewContextOptions(ViewportSize = ViewportSize(Width = 1440, Height = 900))) |> awaitTask
    let page = context.NewPageAsync() |> awaitTask
    let longTaskSession = context.NewCDPSessionAsync(page) |> awaitTask
    longTaskSession.SendAsync("DOM.enable") |> awaitTask |> ignore
    longTaskSession.SendAsync("CSS.enable") |> awaitTask |> ignore
    let longTaskPhases = ResizeArray<string * float array * float>()
    let consoleErrors = ResizeArray<string>()
    page.Console.Add(fun (message: IConsoleMessage) -> if message.Type = "error" then consoleErrors.Add message.Text; printfn "desktop console error: %s" message.Text)
    page.PageError.Add(fun (error: string) -> consoleErrors.Add error; printfn "desktop page error: %s" error)

    let initialTrace = startMainThreadTrace longTaskSession
    page.GotoAsync(url, PageGotoOptions(WaitUntil = WaitUntilState.NetworkIdle)) |> awaitTask |> ignore
    page.Locator("[data-testid='ta-workspace']").WaitForAsync(LocatorWaitForOptions(Timeout = 15000.0f)) |> awaitUnit
    page.Locator("[data-testid='ta-row-heikin']").WaitForAsync(LocatorWaitForOptions(Timeout = 15000.0f)) |> awaitUnit
    waitForIntAttribute (page.Locator("[data-testid='ta-chart-stack']")) "data-ready-row-count" 7
    Threading.Thread.Sleep 180
    printfn
        "browser.initial-setup sampleMs=%s rendererMs=%s"
        (page.Locator("[data-capacity-positions='4000']").GetAttributeAsync("data-sample-build-ms") |> awaitTask)
        (page.Locator("[data-capacity-positions='4000']").GetAttributeAsync("data-renderer-setup-ms") |> awaitTask)
    let _, bootstrapLongTasks, bootstrapMaximum = stopMainThreadTrace "module-bootstrap" longTaskSession initialTrace
    printfn
        "browser.module-bootstrap diagnosticOnly=true fixtureSampleMs=%s rendererSetupMs=%s over100=%d max=%.2fms"
        (page.Locator("[data-capacity-positions='4000']").GetAttributeAsync("data-sample-build-ms") |> awaitTask)
        (page.Locator("[data-capacity-positions='4000']").GetAttributeAsync("data-renderer-setup-ms") |> awaitTask)
        bootstrapLongTasks.Length
        bootstrapMaximum

    let freshChartStack = page.Locator("[data-testid='ta-chart-stack']")
    require (requiredIntAttribute freshChartStack "data-visible-start" = 1) "fresh renderer must honor document visibleBars=4000 instead of the local 48-bar fallback"
    require (requiredIntAttribute freshChartStack "data-visible-end" = capacityPointCount) "fresh document viewport must include the loaded tail"
    require (requiredIntAttribute (page.Locator("[data-testid='ta-candle-price']")) "data-point-count" = capacityPointCount) "fresh document viewport must render the document-requested 4,000 points"
    // Use the topmost marker in the dense fixture so Playwright exercises real SVG hit testing.
    let highDensityMarker = page.Locator("[data-testid='ta-marker-signals-short-exit']")
    let highDensityBand = page.Locator("[data-testid='ta-row-ofi-band-price']")
    let highDensityChart = page.Locator("[data-testid='ta-candle-price']")
    let highDensitySlot = requiredIntAttribute highDensityMarker "data-marker-slot"
    let highDensityChartBox = highDensityChart.BoundingBoxAsync() |> awaitTask
    require (not (isNull highDensityChartBox)) "4,000-point chart must expose geometry for marker hit testing"
    require (highDensityChartBox.Width / float32 capacityPointCount < 1.0f) "high-density regression requires multiple reference slots per CSS pixel"
    let markerPixel = Math.Floor(float highDensityChartBox.X + float highDensityChartBox.Width * ((float highDensitySlot + 0.5) / float capacityPointCount))
    let adjacentPixel = Math.Floor(float highDensityChartBox.X + float highDensityChartBox.Width * ((float highDensitySlot - 0.5) / float capacityPointCount))
    require (markerPixel = adjacentPixel) $"high-density fixture must place marker slot {highDensitySlot} and its preceding slot in the same CSS pixel"
    highDensityMarker.HoverAsync() |> awaitUnit
    waitForIntAttribute highDensityBand "data-cursor-slot" highDensitySlot
    waitForIntAttribute highDensityBand "data-marker-event-count" 1
    page.Locator("[data-testid='ta-view-48']").ClickAsync() |> awaitUnit
    waitForIntAttribute (page.Locator("[data-testid='ta-candle-price']")) "data-point-count" visiblePointCount
    waitForCount (page.Locator("[data-testid='ta-chart-stack'] section")) 7
    let setupCallbackState = page.Locator("[data-testid='ta-demo-callback-state']")
    waitForIntAttribute setupCallbackState "data-callback-count" 1
    let setupCallbackCount = requiredIntAttribute setupCallbackState "data-callback-count"
    let callbackText relativeCount actionName =
        $"callback actions {setupCallbackCount + relativeCount} / last {actionName}"

    requireText (page.Locator("[data-testid='ta-workspace-title']")) "PTMD TA Research"
    requireText (page.Locator("[data-testid='ta-freshness']")) "LIVE"
    require ((page.Locator("[data-testid='ta-chart-stack'] section").CountAsync() |> awaitTask) = 7) "all seven configured TA rows must render"
    verifyRowControlLines "desktop" 1440 longTaskSession page
    let plotSurfaces = page.Locator("svg[role='img'][data-testid^='ta-candle-'], svg[role='img'][data-testid^='ta-composite-']")
    require (plotSurfaces.CountAsync() |> awaitTask = 7) "all chart surfaces must expose the generic plot contract"
    for index in 0 .. (plotSurfaces.CountAsync() |> awaitTask) - 1 do
        require (attributeOrEmpty (plotSurfaces.Nth(index)) "data-plot-surface-theme" = "dark") "every candle and TA surface must use the selected dark theme"
    let priceSurfaceStyle = computedStyleProperties longTaskSession "[data-testid='ta-candle-price']" [| "background-color" |]
    require (Map.tryFind "background-color" priceSurfaceStyle = Some "rgb(0, 0, 0)") $"dark candle surface must be black, actual={priceSurfaceStyle}"
    let timeAxisStyle = computedStyleProperties longTaskSession "[data-testid='ta-time-axis-price']" [| "background-color" |]
    require (Map.tryFind "background-color" timeAxisStyle = Some "rgb(11, 16, 23)") $"dark time axis must retain a readable surface, actual={timeAxisStyle}"
    let timeAxisTextStyle = computedStyleProperties longTaskSession "[data-testid='ta-time-axis-price'] span" [| "color" |]
    require (Map.tryFind "color" timeAxisTextStyle = Some "rgb(203, 213, 225)") $"dark time axis text must remain readable, actual={timeAxisTextStyle}"
    let legendStyle = computedStyleProperties longTaskSession "[data-testid='ta-row-values-price']" [| "background-color"; "color" |]
    require
        (Map.tryFind "background-color" legendStyle = Some "rgb(11, 16, 23)"
         && Map.tryFind "color" legendStyle = Some "rgb(226, 232, 240)")
        $"dark legend must retain readable surface and text, actual={legendStyle}"
    let histogramPositive = page.Locator("[data-testid='ta-trace-macd-macd-histogram']")
    let histogramNegative = page.Locator("[data-testid='ta-trace-macd-macd-histogram-negative']")
    require (attributeOrEmpty histogramPositive "data-histogram-polarity" = "positive") "histogram positive path must expose typed polarity"
    require (attributeOrEmpty histogramNegative "data-histogram-polarity" = "negative") "histogram negative path must expose typed polarity"
    require (attributeOrEmpty histogramPositive "fill" = "#dc2626") "positive histogram bars must use the typed red color"
    require (attributeOrEmpty histogramNegative "fill" = "#16a34a") "negative histogram bars must use the typed green color"
    require (not (String.IsNullOrWhiteSpace(attributeOrEmpty histogramPositive "d"))) "positive histogram geometry must be non-empty"
    require (not (String.IsNullOrWhiteSpace(attributeOrEmpty histogramNegative "d"))) "negative histogram geometry must be non-empty"
    require (attributeOrEmpty (page.Locator("[data-testid='ta-candle-price-crosshair']")) "stroke" = "#7dd3fc") "dark shared cursor must remain readable"
    requireText (page.Locator("[data-testid='ta-toggle-row-price']")) "ES 1K + SMA(20)"
    requireText (page.Locator("[data-testid='ta-row-price']")) "ES 1K + SMA(20)"
    require (requiredIntAttribute (page.Locator("[data-testid='ta-candle-price']")) "data-point-count" = visiblePointCount) "candlestick chart must retain all committed visible points"
    let projectedCoarseCandles = page.Locator("[data-testid='ta-candle-price-price-5k'][data-candle-batched='true']")
    let projectedCoarseCount = projectedCoarseCandles.CountAsync() |> awaitTask
    require (projectedCoarseCount = 8) $"5K source candles must use the fixed eight batched paths; actual={projectedCoarseCount}"
    require (projectedCoarseCandles |> fun paths -> attributeSignature paths "d" |> String.IsNullOrWhiteSpace |> not) "projected 5K batched geometry must be non-empty"
    requireCandleEdgePadding
        "initial"
        15.0
        3.0
        (page.Locator("[data-testid='ta-candle-price']"))
        (page.Locator("[data-testid='ta-candle-price'] [data-candle-batched='true']"))
    requireText (page.Locator("[data-testid='ta-status-detail']")) "watermark 2026-07-11 09:30:00 UTC"
    requireText (page.Locator("[data-testid='ta-status-detail']")) "quality complete"

    let markerLayer = page.Locator("[data-testid='ta-marker-layer-price']")
    require (markerLayer.CountAsync() |> awaitTask = 1) "price row must mount one marker overlay layer"
    require (requiredIntAttribute markerLayer "data-marker-count" = 66) "all 66 wire markers must remain represented"
    require (requiredIntAttribute markerLayer "data-direct-marker-count" = 6) "only six direct glyphs must render across the three visible buckets"
    require (requiredIntAttribute markerLayer "data-marker-overflow-count" = 1) "the dense bucket must produce one overflow cluster"
    let entryMarker = page.Locator("[data-testid='ta-marker-signals-long-entry']")
    let signalA = page.Locator("[data-testid='ta-marker-signals-short-entry']")
    let signalB = page.Locator("[data-testid='ta-marker-signals-long-exit']")
    let exitMarker = page.Locator("[data-testid='ta-marker-signals-short-exit']")
    require (entryMarker.GetAttributeAsync("data-marker-anchor") |> awaitTask = "below-bar") "entry marker must retain its below-bar anchor"
    require (signalA.GetAttributeAsync("data-marker-position") |> awaitTask = string (capacityPointCount - 8)) "marker placement must use authoritative position"
    require (requiredIntAttribute signalA "data-marker-lane" = 0) "first same-anchor marker must use lane zero"
    require (requiredIntAttribute signalB "data-marker-lane" = 1) "second same-anchor marker must stack in lane one"
    requireText (signalA.Locator("title")) "SE"
    requireText (signalA.Locator("title")) "Reason: short entry signal"
    requireText (signalA.Locator("title")) "Source: BrowserDemo"
    require (page.Locator("[data-testid^='ta-marker-label-']").CountAsync() |> awaitTask = 0) "plot glyphs must not render marker Label as inline SVG text"
    let priceOfiBand = page.Locator("[data-testid='ta-row-ofi-band-price']")
    let priceOfiBandBox = priceOfiBand.BoundingBoxAsync() |> awaitTask
    require (not (isNull priceOfiBandBox) && abs (priceOfiBandBox.Height - 24.0f) <= 0.1f) "price OFI band must reserve exactly 24 CSS pixels"
    require (requiredIntAttribute priceOfiBand "data-marker-event-count" = 0) "OFI band starts empty before shared-cursor selection"
    let markerChart = page.Locator("[data-testid='ta-candle-price']")
    let markerChartBox = markerChart.BoundingBoxAsync() |> awaitTask
    require (not (isNull markerChartBox)) "price chart must expose geometry for OFI cursor projection"
    let markerCursorGutterBox = page.Locator("[data-testid='ta-row-cursor-gutter-price']").BoundingBoxAsync() |> awaitTask
    require
        (not (isNull markerCursorGutterBox)
         && markerCursorGutterBox.Y + markerCursorGutterBox.Height <= priceOfiBandBox.Y + 0.5f
         && priceOfiBandBox.Y + priceOfiBandBox.Height <= markerChartBox.Y + 0.5f)
        "row order must be cursor date-time strip, fixed-height OFI band, then plot"
    let markerCluster = page.Locator("g[role='button'][data-marker-overflow-count]")
    require (markerCluster.CountAsync() |> awaitTask = 1) "the dense marker bucket must expose one +N control"
    requireText markerCluster "+60"
    markerCluster.FocusAsync() |> awaitUnit
    markerCluster.PressAsync("Enter") |> awaitUnit
    let markerClusterDetail = page.Locator("[data-testid='ta-marker-overflow-detail']")
    markerClusterDetail.WaitForAsync(LocatorWaitForOptions(State = WaitForSelectorState.Visible, Timeout = 3000.0f)) |> awaitUnit
    require (requiredIntAttribute markerClusterDetail "data-cluster-selected-index" = 0) "Enter must open the first hidden marker"
    requireText markerClusterDetail "1/60"
    markerCluster.PressAsync("ArrowDown") |> awaitUnit
    waitForAttributeValue markerClusterDetail "data-cluster-selected-index" "1"
    requireText markerClusterDetail "2/60"
    markerCluster.PressAsync("Escape") |> awaitUnit
    markerClusterDetail.WaitForAsync(LocatorWaitForOptions(State = WaitForSelectorState.Detached, Timeout = 3000.0f)) |> awaitUnit
    let priceRowBox = page.Locator("[data-testid='ta-row-price']").BoundingBoxAsync() |> awaitTask
    for label, markerNode in [ "entry", entryMarker; "signal-a", signalA; "signal-b", signalB; "exit", exitMarker ] do
        let markerBox = markerNode.BoundingBoxAsync() |> awaitTask
        require (not (isNull markerBox) && not (isNull priceRowBox)) (label + " marker and price row must expose geometry")
        require (markerBox.Y >= priceRowBox.Y - 0.5f && markerBox.Y + markerBox.Height <= priceRowBox.Y + priceRowBox.Height + 0.5f) (label + " marker must remain clipped to its row")

    let chartStack = page.Locator("[data-testid='ta-chart-stack']")
    require (chartStack.GetAttributeAsync("data-loaded-bars") |> awaitTask = string capacityPointCount) "loaded-range metadata must report the full capacity fixture"
    require (chartStack.GetAttributeAsync("data-visible-start") |> awaitTask = string initialVisibleStart) "follow-latest viewport must begin at the expected capacity position"
    require (chartStack.GetAttributeAsync("data-visible-end") |> awaitTask = string capacityPointCount) "follow-latest viewport must end at the capacity tail"
    require (chartStack.GetAttributeAsync("data-visible-value-query-scope") |> awaitTask = "cursor-panel+row-legends") "visible-value refresh must stay scoped away from the full SVG chart tree"
    require (page.Locator("[data-capacity-positions='4000']").CountAsync() |> awaitTask = 1) "browser fixture must declare 4,000 positions"
    require (page.Locator("[data-capacity-shared-series='28']").CountAsync() |> awaitTask = 1) "browser fixture must declare 28 shared scalar series"
    require (page.Locator("[data-sparse-empty-traces='20']").CountAsync() |> awaitTask = 1) "browser fixture must retain 20 sparse empty traces"
    requireText (page.Locator("[data-testid='ta-viewport-range']")) $"Loaded {capacityPointCount} bars"
    requireText (page.Locator("[data-testid='ta-viewport-range']")) $"Viewing {initialVisibleStart}-{capacityPointCount}"
    let sharedSma = page.Locator("[data-testid='ta-trace-sma-sma-1k']")
    require (sharedSma.CountAsync() |> awaitTask = 1) "shared-axis SMA trace must be mounted exactly once"
    require
        (sharedSma.GetAttributeAsync("d") |> awaitTask |> Option.ofObj |> Option.exists (String.IsNullOrWhiteSpace >> not))
        "shared-axis SMA trace path must be non-empty"
    let defaultChartSvgs = page.Locator("svg[role='img'][data-testid^='ta-candle-'], svg[role='img'][data-testid^='ta-composite-']")
    let defaultChartSvgCount = defaultChartSvgs.CountAsync() |> awaitTask
    require (defaultChartSvgCount > 0) "renderer must expose chart SVGs before row resize"
    for index in 0 .. defaultChartSvgCount - 1 do
        let chart = defaultChartSvgs.Nth(index)
        let testId = chart.GetAttributeAsync("data-testid") |> awaitTask |> Option.ofObj |> Option.defaultValue $"chart-{index}"
        let box = chart.BoundingBoxAsync() |> awaitTask
        require (not (isNull box)) $"default chart SVG {testId} must expose geometry"
        require (box.Height <= 250.1f) $"default chart SVG {testId} must be <=250 CSS px before resize; actual={box.Height}"
    let sharedSmaSelector = "[data-testid='ta-trace-sma-sma-1k']"
    let sharedSmaStrokeBeforeResize = requireFixedCssStroke longTaskSession sharedSmaSelector sharedSma 1.0 2.0
    let smaResize = page.Locator("[data-testid='ta-row-resize-sma']")
    let smaHeightBeforeResize = requiredIntAttribute smaResize "aria-valuenow"
    smaResize.FocusAsync() |> awaitUnit
    smaResize.PressAsync("Shift+ArrowUp") |> awaitUnit
    waitForAttributeChange smaResize "aria-valuenow" (string smaHeightBeforeResize) |> ignore
    let sharedSmaStrokeAfterResize = requireFixedCssStroke longTaskSession sharedSmaSelector sharedSma 1.0 2.0
    require
        (sharedSmaStrokeAfterResize = sharedSmaStrokeBeforeResize)
        $"SMA line CSS-pixel stroke changed after row resize: before={sharedSmaStrokeBeforeResize}; after={sharedSmaStrokeAfterResize}"
    smaResize.DblClickAsync() |> awaitUnit
    waitForAttributeValue smaResize "aria-valuenow" (string smaHeightBeforeResize)
    let priceCandlePaths = page.Locator("[data-testid='ta-candle-price-price-1k'][data-candle-batched='true']")
    let candleRows = [| "price"; "volume"; "dmi"; "adx"; "heikin" |]
    let candlePathLocators =
        candleRows
        |> Array.map (fun rowId -> page.Locator($"[data-testid='ta-candle-{rowId}'] [data-candle-batched='true']"))
    let candlePathSignaturesBefore =
        candlePathLocators |> Array.map (fun locator -> attributeSignature locator "d")
    require
        (Array.forall2 (fun _ signature -> not (String.IsNullOrWhiteSpace signature)) candleRows candlePathSignaturesBefore)
        "all five candle-heavy rows must expose non-empty batched paths before replacement"
    let fixtureRoot = page.Locator("[data-capacity-positions]")
    let renderSequenceBeforeCandleReplacement = requiredIntAttribute chartStack "data-chart-render-sequence"
    let runtimeDataRevisionBeforeCandleReplacement = requiredIntAttribute fixtureRoot "data-runtime-data-revision"
    let runtimeTransportSequenceBeforeCandleReplacement = requiredIntAttribute fixtureRoot "data-runtime-transport-sequence"
    printfn
        "browser.five-candle-fixture wireChars=%s packets=%s dataRefs=%d positions=%d"
        (fixtureRoot.GetAttributeAsync("data-candle-workload-wire-chars") |> awaitTask)
        (fixtureRoot.GetAttributeAsync("data-candle-workload-packets") |> awaitTask)
        capacitySeriesCount
        capacityPointCount
    let candleReplacementTrace = startMainThreadTrace longTaskSession
    page.Locator("[data-testid='ta-demo-replace-five-candle-rows']").ClickAsync() |> awaitUnit
    let candleWorkloadOutcome = page.Locator("[data-testid='ta-demo-candle-workload-outcome']")
    let outcomeDeadline = DateTime.UtcNow.AddSeconds 8.0
    let mutable workloadOutcome = textOf candleWorkloadOutcome
    while (workloadOutcome = "idle" || workloadOutcome = "pending") && DateTime.UtcNow < outcomeDeadline do
        Threading.Thread.Sleep 25
        workloadOutcome <- textOf candleWorkloadOutcome
    if workloadOutcome <> "applied" then
        printfn
            "browser.five-candle-fixture outcome=%s error=%s"
            workloadOutcome
            (fixtureRoot.GetAttributeAsync("data-candle-workload-error") |> awaitTask)
    require (workloadOutcome = "applied") $"five-candle workload must apply, actual={workloadOutcome}"
    waitForAttributeValue fixtureRoot "data-candle-workload-replacements" "1"
    printfn
        "browser.five-candle-stages %s runtimeData=%d->%d runtimeTransport=%d->%d chartData=%s chartTransport=%s render=%d"
        (fixtureRoot.GetAttributeAsync("data-candle-workload-stage-diagnostics") |> awaitTask)
        runtimeDataRevisionBeforeCandleReplacement
        (requiredIntAttribute fixtureRoot "data-runtime-data-revision")
        runtimeTransportSequenceBeforeCandleReplacement
        (requiredIntAttribute fixtureRoot "data-runtime-transport-sequence")
        (chartStack.GetAttributeAsync("data-chart-data-revision") |> awaitTask)
        (chartStack.GetAttributeAsync("data-chart-transport-sequence") |> awaitTask)
        (requiredIntAttribute chartStack "data-chart-render-sequence")
    let candlePathSignaturesAfter =
        candlePathLocators
        |> Array.mapi (fun index locator ->
            printfn "browser.five-candle-await row=%s" candleRows[index]
            waitForAttributeSignatureChange locator "d" candlePathSignaturesBefore[index])
    Threading.Thread.Sleep 180
    longTaskPhases.Add(stopMainThreadTrace "five-candle-replacement" longTaskSession candleReplacementTrace)
    printVisibleValueTelemetry "five-candle-replacement" page chartStack
    Array.zip3 candleRows candlePathSignaturesBefore candlePathSignaturesAfter
    |> Array.iter (fun (rowId, before, after) ->
        require (after <> before) $"{rowId} candle row must render the replacement payload")
    require
        (requiredIntAttribute chartStack "data-chart-render-sequence" = renderSequenceBeforeCandleReplacement)
        "same-topology five-candle replacement must update row Vars without rebuilding the chart stack"
    let scenarioReplacementTrace = startMainThreadTrace longTaskSession
    page.Locator("[data-testid='ta-demo-replace-scenario-overlays']").ClickAsync() |> awaitUnit
    waitForAttributeValue fixtureRoot "data-scenario-replacement-outcome" "applied"
    waitForAttributeValue fixtureRoot "data-scenario-replacements" "1"
    waitForText (page.Locator("[data-testid='ta-marker-signals-long-entry'] title")) "scenario 1"
    Threading.Thread.Sleep 180
    longTaskPhases.Add(stopMainThreadTrace "scenario-overlay-replacement" longTaskSession scenarioReplacementTrace)
    let viewportBox = page.Locator("[data-testid='ta-viewport-panel']").BoundingBoxAsync() |> awaitTask
    let initialPriceBox = page.Locator("[data-testid='ta-candle-price']").BoundingBoxAsync() |> awaitTask
    require (not (isNull viewportBox) && not (isNull initialPriceBox)) "viewport navigator and first chart row must expose geometry"
    require (viewportBox.Y + viewportBox.Height <= initialPriceBox.Y + 1.0f) "viewport navigator must be visible before the first chart row"
    let priceCursorTagSelector = "[data-testid='ta-row-cursor-label-price']"
    let priceCursorTag = page.Locator(priceCursorTagSelector)
    let priceCursorGutter = page.Locator("[data-testid='ta-row-cursor-gutter-price']")
    let cursorTagStyleProperties =
        [| "display"; "position"; "box-sizing"; "width"; "min-width"; "max-width"
           "height"; "min-height"; "max-height"; "padding-top"; "padding-right"; "padding-bottom"; "padding-left"
           "border-top-width"; "border-top-style"; "border-top-color"; "border-radius"
           "font-family"; "font-size"; "font-weight"; "line-height"; "grid-template-rows" |]
    let cursorTagBoxBeforeResize = priceCursorTag.BoundingBoxAsync() |> awaitTask
    let cursorGutterBox = priceCursorGutter.BoundingBoxAsync() |> awaitTask
    require (not (isNull cursorTagBoxBeforeResize) && not (isNull cursorGutterBox)) "row cursor tag and fixed gutter must expose geometry while hidden"
    require
        (abs (cursorGutterBox.Height - 32.0f) <= 0.1f)
        $"fixed cursor gutter must occupy exactly 32 CSS pixels; actual={cursorGutterBox.Height}"
    require
        (cursorTagBoxBeforeResize.Y >= cursorGutterBox.Y - 0.5f
         && cursorTagBoxBeforeResize.Y + cursorTagBoxBeforeResize.Height <= cursorGutterBox.Y + cursorGutterBox.Height + 0.5f)
        "row cursor tag must remain completely inside the fixed top gutter"
    require
        (cursorGutterBox.Y + cursorGutterBox.Height <= initialPriceBox.Y + 0.5f)
        "fixed cursor gutter must remain outside and above the SVG plot"
    let cursorTagStyleBeforeResize = computedStyleProperties longTaskSession priceCursorTagSelector cursorTagStyleProperties
    let priceResize = page.Locator("[data-testid='ta-row-resize-price']")
    require (requiredIntAttribute priceResize "aria-valuenow" = 250) "Authored candle-row default must cap at 250 CSS pixels"
    priceResize.FocusAsync() |> awaitUnit
    priceResize.PressAsync("Shift+ArrowDown") |> awaitUnit
    waitForAttributeValue priceResize "aria-valuenow" "282"
    let manuallyExpandedPriceBox = page.Locator("[data-testid='ta-candle-price']").BoundingBoxAsync() |> awaitTask
    require (not (isNull manuallyExpandedPriceBox) && manuallyExpandedPriceBox.Height > 250.0f) "Trader manual resize must remain able to exceed the authored 250px cap"
    priceResize.DblClickAsync() |> awaitUnit
    waitForAttributeValue priceResize "aria-valuenow" "250"
    priceResize.FocusAsync() |> awaitUnit
    priceResize.PressAsync("Shift+ArrowUp") |> awaitUnit
    waitForAttributeValue priceResize "aria-valuenow" "218"
    let resizedPriceBox = page.Locator("[data-testid='ta-candle-price']").BoundingBoxAsync() |> awaitTask
    require (not (isNull resizedPriceBox) && resizedPriceBox.Height < initialPriceBox.Height - 20.0f) "keyboard resize must reduce only the price chart height"
    let resizeHandleBox = priceResize.BoundingBoxAsync() |> awaitTask
    require (not (isNull resizeHandleBox)) "row resize separator must expose pointer geometry"
    page.Mouse.MoveAsync(resizeHandleBox.X + resizeHandleBox.Width / 2.0f, resizeHandleBox.Y + resizeHandleBox.Height / 2.0f) |> awaitUnit
    page.Mouse.DownAsync(MouseDownOptions(Button = MouseButton.Left)) |> awaitUnit
    page.Mouse.MoveAsync(resizeHandleBox.X + resizeHandleBox.Width / 2.0f, resizeHandleBox.Y - 48.0f, MouseMoveOptions(Steps = 6)) |> awaitUnit
    page.Mouse.UpAsync(MouseUpOptions(Button = MouseButton.Left)) |> awaitUnit
    waitForAttributeChange priceResize "aria-valuenow" "218" |> ignore
    let pointerHeight = waitForStableIntAttribute priceResize "aria-valuenow"
    require (pointerHeight >= 180 && pointerHeight <= 186) $"pointer resize must apply the requested reduction down to the 180px candle minimum within handle geometry tolerance, actual={pointerHeight}"
    Threading.Thread.Sleep 50
    requireCandleEdgePadding
        "pointer-resize"
        15.0
        3.0
        (page.Locator("[data-testid='ta-candle-price']"))
        (page.Locator("[data-testid='ta-candle-price'] [data-candle-batched='true']"))
    page.Locator("[data-testid='ta-demo-replace-markers']").ClickAsync() |> awaitUnit
    waitForText (page.Locator("[data-testid='ta-marker-signals-long-entry'] title")) "replacement 1"
    let heightAfterMarkerReplacement = requiredIntAttribute priceResize "aria-valuenow"
    require
        (heightAfterMarkerReplacement = pointerHeight)
        $"same-canvas authoritative data replacement must retain the local row-height override; expected={pointerHeight}; actual={heightAfterMarkerReplacement}"
    let cursorTagBoxAfterResize = priceCursorTag.BoundingBoxAsync() |> awaitTask
    let cursorGutterBoxAfterResize = priceCursorGutter.BoundingBoxAsync() |> awaitTask
    require
        (not (isNull cursorTagBoxAfterResize) && not (isNull cursorGutterBoxAfterResize))
        "row cursor tag and gutter must retain geometry after row resize"
    require
        (abs (cursorGutterBoxAfterResize.Height - 32.0f) <= 0.1f
         && abs (cursorGutterBoxAfterResize.Height - cursorGutterBox.Height) <= 0.1f)
        $"row resize must retain the exact 32px cursor gutter; before={cursorGutterBox.Height}; after={cursorGutterBoxAfterResize.Height}"
    require
        (abs (cursorTagBoxAfterResize.Width - cursorTagBoxBeforeResize.Width) <= 0.5f
         && abs (cursorTagBoxAfterResize.Height - cursorTagBoxBeforeResize.Height) <= 0.5f)
        "row resize must not change the cursor tag CSS-pixel width or height"
    let cursorTagStyleAfterResize = computedStyleProperties longTaskSession priceCursorTagSelector cursorTagStyleProperties
    require
        (cursorTagStyleAfterResize = cursorTagStyleBeforeResize)
        $"row resize changed fixed cursor tag computed style: before={cursorTagStyleBeforeResize}; after={cursorTagStyleAfterResize}"
    priceResize.DblClickAsync() |> awaitUnit
    waitForAttributeValue priceResize "aria-valuenow" "250"
    requireCandleEdgePadding
        "resize-reset"
        15.0
        3.0
        (page.Locator("[data-testid='ta-candle-price']"))
        (page.Locator("[data-testid='ta-candle-price'] [data-candle-batched='true']"))
    let crosshairs = page.Locator("[data-testid$='-crosshair']")
    require ((crosshairs.CountAsync() |> awaitTask) = 7) "every visible row must mount one stable crosshair overlay"
    require ((page.Locator("[data-testid$='-crosshair'][visibility='hidden']").CountAsync() |> awaitTask) = 7) "crosshair overlays must remain hidden before pointer movement"
    let rowTimeAxes = page.Locator("[data-time-axis-row-id]")
    require ((rowTimeAxes.CountAsync() |> awaitTask) = 7) "every visible row must mount its own event-time axis"
    for axisIndex in 0 .. 6 do
        let axis = rowTimeAxes.Nth(axisIndex)
        let labels = axis.Locator("span")
        let labelCount = labels.CountAsync() |> awaitTask
        require (labelCount >= 2 && labelCount <= 16) $"row axis {axisIndex} must expose a bounded adaptive label count"
        let mutable previousRight = Single.NegativeInfinity
        for labelIndex in 0 .. labelCount - 1 do
            let labelBox = labels.Nth(labelIndex).BoundingBoxAsync() |> awaitTask
            require (not (isNull labelBox)) $"row axis {axisIndex} label {labelIndex} must expose geometry"
            require
                (labelBox.X + 0.5f >= previousRight)
                $"row axis {axisIndex} labels must not overlap; label={labelIndex}; left={labelBox.X:F2}; previousRight={previousRight:F2}; width={labelBox.Width:F2}"
            previousRight <- labelBox.X + labelBox.Width
    for crosshairIndex in 0 .. 6 do
        let crosshair = crosshairs.Nth(crosshairIndex)
        let y1 = Double.Parse(crosshair.GetAttributeAsync("y1") |> awaitTask, Globalization.CultureInfo.InvariantCulture)
        let y2 = Double.Parse(crosshair.GetAttributeAsync("y2") |> awaitTask, Globalization.CultureInfo.InvariantCulture)
        let chart = crosshair.Locator("xpath=ancestor::*[local-name()='svg'][1]")
        let viewBox = chart.GetAttributeAsync("viewBox") |> awaitTask
        let viewBoxHeight =
            viewBox.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            |> Array.last
            |> fun value -> Double.Parse(value, Globalization.CultureInfo.InvariantCulture)
        require (y1 = 0.0 && abs (y2 - viewBoxHeight) < 0.001)
            $"row crosshair {crosshairIndex} must span the full SVG row height: y1={y1}, y2={y2}, height={viewBoxHeight}"

    let rowLegends = page.Locator("[data-ta-row-values='true']")
    require ((rowLegends.CountAsync() |> awaitTask) = 7) "every visible row must expose one legend/value band"
    let initialLegendHeights =
        [| for index in 0 .. 6 do
               let legend = rowLegends.Nth(index)
               let box = legend.BoundingBoxAsync() |> awaitTask
               require (not (isNull box)) $"row legend {index} must expose geometry"
               let style = legend.GetAttributeAsync("style") |> awaitTask
               require (legend.GetAttributeAsync("data-auto-height") |> awaitTask = "true") $"row legend {index} must publish its auto-height contract"
               require (style.Contains("flex-wrap:wrap") && style.Contains("overflow:visible")) $"row legend {index} must wrap without an internal scrollbar"
               yield box.Height |]
    require (initialLegendHeights |> Array.forall (fun height -> height >= 29.5f)) ("row legend height fell below its 30px minimum: " + String.concat "," (initialLegendHeights |> Array.map string))
    require (initialLegendHeights |> Array.exists (fun height -> height > 30.5f)) ("long data-window content did not expand its row: " + String.concat "," (initialLegendHeights |> Array.map string))

    let smaLegend = page.Locator("[data-testid='ta-row-values-sma']")
    let smaLegendToken = page.Locator("[data-testid='ta-row-value-sma-sma-1k']")
    let smaLegendLabel = smaLegendToken.Locator("[data-ta-row-value-label='true']")
    let smaLegendValue = smaLegendToken.Locator("[data-ta-row-value-text='true']")
    require (smaLegend.GetAttributeAsync("data-auto-height") |> awaitTask = "true") "the SMA row value band must publish its auto-height contract"
    waitForAttributeValue smaLegendValue "data-value-state" "defined"
    let labelBox = smaLegendLabel.BoundingBoxAsync() |> awaitTask
    let initialValueBox = smaLegendValue.BoundingBoxAsync() |> awaitTask
    let smaRowBox = page.Locator("[data-testid='ta-row-sma']").BoundingBoxAsync() |> awaitTask
    require (not (isNull labelBox) && not (isNull initialValueBox) && not (isNull smaRowBox)) "SMA row legend must expose stable label/value/row geometry"
    require (initialValueBox.X >= labelBox.X + labelBox.Width && initialValueBox.X - (labelBox.X + labelBox.Width) <= 6.0f) "the SMA value must sit immediately to the right of its own label"
    let renderSequenceBeforeLegendValues = requiredIntAttribute chartStack "data-chart-render-sequence"
    page.Locator("[data-testid='ta-demo-legend-undef']").ClickAsync() |> awaitUnit
    waitForAttributeValue smaLegendValue "data-value-state" "undefined"
    requireText smaLegendValue "Unavailable"
    let undefLegendBox = smaLegend.BoundingBoxAsync() |> awaitTask
    let undefValueBox = smaLegendValue.BoundingBoxAsync() |> awaitTask
    let undefRowBox = page.Locator("[data-testid='ta-row-sma']").BoundingBoxAsync() |> awaitTask
    require (abs (undefLegendBox.Height - initialLegendHeights[2]) <= 0.5f && abs (undefValueBox.Width - initialValueBox.Width) <= 0.5f && abs (undefRowBox.Height - smaRowBox.Height) <= 0.5f) "Unavailable must not change legend/value/row geometry"
    page.Locator("[data-testid='ta-demo-legend-long']").ClickAsync() |> awaitUnit
    waitForAttributeValue smaLegendValue "data-value-state" "defined"
    waitForText smaLegendValue "123456789"
    let longLegendBox = smaLegend.BoundingBoxAsync() |> awaitTask
    let longValueBox = smaLegendValue.BoundingBoxAsync() |> awaitTask
    let longRowBox = page.Locator("[data-testid='ta-row-sma']").BoundingBoxAsync() |> awaitTask
    require (abs (longLegendBox.Height - initialLegendHeights[2]) <= 0.5f && abs (longValueBox.Width - initialValueBox.Width) <= 0.5f && abs (longRowBox.Height - smaRowBox.Height) <= 0.5f) "a long realtime value must not change legend/value/row geometry"
    require (requiredIntAttribute chartStack "data-chart-render-sequence" = renderSequenceBeforeLegendValues) "legend value changes must update existing text without rebuilding the chart stack"

    let summaryToggle = page.Locator("[data-testid='ta-cross-scale-values-toggle']")
    let crossScaleValues = page.Locator("[data-testid='ta-cross-scale-values']")
    require (summaryToggle.GetAttributeAsync("aria-expanded") |> awaitTask = "false") "cross-scale summary must default to collapsed"
    require (crossScaleValues.GetAttributeAsync("data-expanded") |> awaitTask = "false") "cross-scale panel must expose its collapsed state"
    require (not (crossScaleValues.IsVisibleAsync() |> awaitTask)) "collapsed cross-scale values must not consume value-band height"
    let priceRowBoxBeforeSummary = page.Locator("[data-testid='ta-row-price']").BoundingBoxAsync() |> awaitTask
    let chartStackBoxBeforeSummary = chartStack.BoundingBoxAsync() |> awaitTask
    require (not (isNull priceRowBoxBeforeSummary) && not (isNull chartStackBoxBeforeSummary)) "price row and chart stack must expose geometry before summary expansion"
    summaryToggle.ClickAsync() |> awaitUnit
    waitForAttributeValue summaryToggle "aria-expanded" "true"
    waitForAttributeValue crossScaleValues "data-expanded" "true"
    require (crossScaleValues.IsVisibleAsync() |> awaitTask) "human toggle must expand the cross-scale summary"
    let summaryBox = crossScaleValues.BoundingBoxAsync() |> awaitTask
    require (not (isNull summaryBox) && abs (summaryBox.Height - 30.0f) <= 0.5f) "expanded cross-scale values must use one fixed-height line"
    let priceRowBoxAfterSummary = page.Locator("[data-testid='ta-row-price']").BoundingBoxAsync() |> awaitTask
    let chartStackBoxAfterSummary = chartStack.BoundingBoxAsync() |> awaitTask
    require
        (not (isNull priceRowBoxAfterSummary)
         && not (isNull chartStackBoxAfterSummary)
         && abs ((priceRowBoxAfterSummary.Y - chartStackBoxAfterSummary.Y) - (priceRowBoxBeforeSummary.Y - chartStackBoxBeforeSummary.Y)) <= 0.5f)
        "bottom summary expansion must not move chart rows within the chart stack"
    summaryToggle.ClickAsync() |> awaitUnit
    waitForAttributeValue summaryToggle "aria-expanded" "false"
    waitForAttributeValue crossScaleValues "data-expanded" "false"

    let previewCloseBefore = attributeSignature priceCandlePaths "d"
    let renderSequenceBeforePreview = requiredIntAttribute chartStack "data-chart-render-sequence"
    page.Locator("[data-testid='ta-demo-preview-update']").ClickAsync() |> awaitUnit
    let previewCloseAfter = waitForAttributeSignatureChange priceCandlePaths "d" previewCloseBefore
    require (previewCloseAfter <> previewCloseBefore) "same-position live preview must update the latest candle close"
    let renderSequenceAfterPreview = requiredIntAttribute chartStack "data-chart-render-sequence"
    require
        (renderSequenceAfterPreview = renderSequenceBeforePreview)
        $"same-position live preview must update SVG attributes without rebuilding the chart stack; render={renderSequenceBeforePreview}->{renderSequenceAfterPreview}; close={previewCloseBefore}->{previewCloseAfter}"
    let streamCloseBefore = attributeSignature priceCandlePaths "d"
    page.Locator("[data-testid='ta-demo-preview-stream']").ClickAsync() |> awaitUnit
    waitForIntAttributeAtLeast fixtureRoot "data-preview-stream-updates" 1 |> ignore
    let streamCloseAfter = waitForAttributeSignatureChange priceCandlePaths "d" streamCloseBefore
    require (streamCloseAfter <> streamCloseBefore) "the live preview stream must advance the visible close while follow-latest is active"

    let moveToMarkerSlot slotIndex =
        let currentBox = markerChart.BoundingBoxAsync() |> awaitTask
        require (not (isNull currentBox)) "price chart must retain geometry for OFI cursor projection"
        let ratio = (float32 slotIndex + 0.5f) / float32 visiblePointCount
        page.Mouse.MoveAsync(currentBox.X + currentBox.Width * ratio, currentBox.Y + currentBox.Height / 2.0f) |> awaitUnit
    let entryMarkerEventTime = attributeOrEmpty entryMarker "data-marker-event-time"
    require (entryMarkerEventTime.EndsWith(":30.0000000+00:00")) "entry marker fixture must use an intra-bar event time that differs from the row axis timestamp"
    entryMarker.HoverAsync() |> awaitUnit
    waitForIntAttribute priceOfiBand "data-marker-event-count" 1
    let firstOfiItem = priceOfiBand.Locator("[data-ta-row-ofi-item-index='0']")
    requireText firstOfiItem "BUY 7588.25"
    require (attributeOrEmpty firstOfiItem "data-marker-event-time" = entryMarkerEventTime) "marker hover must project the exact marker event time into OFI without row-axis reconstruction"
    require ((attributeOrEmpty firstOfiItem "title").Contains "Reason: long entry signal") "OFI item must retain the marker tooltip payload"
    moveToMarkerSlot (requiredIntAttribute signalA "data-marker-slot")
    waitForIntAttribute priceOfiBand "data-marker-event-count" 66
    require (priceOfiBand.Locator("[data-cursor-event-source-kind='overview-stripe']").CountAsync() |> awaitTask = 2)
        "dense cursor slot must merge both OverviewStripe events with marker events"
    require (priceOfiBand.Locator("[data-cursor-event-source-kind='marker']").CountAsync() |> awaitTask = 2)
        "round-robin event budget must retain marker details alongside OverviewStripe events"
    requireText (priceOfiBand.Locator("[data-ta-row-ofi-overflow='true']")) "+62"
    moveToMarkerSlot 0
    waitForIntAttribute priceOfiBand "data-marker-event-count" 0
    require (textOf priceOfiBand = "None") "cursor slot without events must retain the fixed-height event band and show None"

    let navigator = page.Locator("[data-testid='ta-overview-navigator']")
    require (attributeOrEmpty navigator "data-plot-surface-theme" = "dark") "overview must use the selected generic dark theme"
    let overviewStyle = computedStyleProperties longTaskSession "[data-testid='ta-overview-navigator']" [| "background-color" |]
    require (Map.tryFind "background-color" overviewStyle = Some "rgb(0, 0, 0)") $"dark overview surface must be black, actual={overviewStyle}"
    let overviewWicks = page.Locator("[data-testid='ta-overview-candle-wicks']")
    let overviewUpBodies = page.Locator("[data-testid='ta-overview-candle-up-bodies']")
    let overviewDownBodies = page.Locator("[data-testid='ta-overview-candle-down-bodies']")
    require (attributeOrEmpty overviewWicks "stroke" = "#60a5fa") "dark overview candle wicks must remain readable"
    require (requiredIntAttribute overviewWicks "data-candle-sample-count" <= 280) "overview candlesticks must retain the bounded sample budget"
    require (not (String.IsNullOrWhiteSpace(attributeOrEmpty overviewWicks "d"))) "overview candlestick wicks must retain OHLC geometry"
    require (attributeOrEmpty overviewUpBodies "fill" = "#4ade80") "dark overview up candles must remain readable"
    require (attributeOrEmpty overviewDownBodies "fill" = "#f87171") "dark overview down candles must remain readable"
    require (not (String.IsNullOrWhiteSpace(attributeOrEmpty overviewUpBodies "d"))) "overview must retain up candle bodies"
    require (not (String.IsNullOrWhiteSpace(attributeOrEmpty overviewDownBodies "d"))) "overview must retain down candle bodies"
    require (page.Locator("[data-testid='ta-overview-price-line']").CountAsync() |> awaitTask = 0) "overview must not regress to a close-only polyline"
    require (attributeOrEmpty (page.Locator("[data-testid='ta-overview-selection']")) "fill" = "rgba(203,213,225,.20)") "overview selection must use the agreed light-gray fill"
    let overviewStripePaths = page.Locator("[data-testid='ta-overview-stripe-path']")
    require (overviewStripePaths.CountAsync() |> awaitTask = 2) "order and fill overview stripes must render as two batched paths"
    require (requiredIntAttribute (overviewStripePaths.Nth(0)) "data-stripe-count" = 1) "order stripe path must retain its item count"
    require (requiredIntAttribute (overviewStripePaths.Nth(1)) "data-stripe-count" = 1) "fill stripe path must retain its item count"
    let orderStripePath = overviewStripePaths.Nth(0).GetAttributeAsync("d") |> awaitTask
    let fillStripePath = overviewStripePaths.Nth(1).GetAttributeAsync("d") |> awaitTask
    require (not (String.IsNullOrWhiteSpace orderStripePath) && not (String.IsNullOrWhiteSpace fillStripePath) && orderStripePath <> fillStripePath) "same-time cross-trace stripes must occupy deterministic adjacent lanes"
    let renderSequenceBeforeStripeRefresh = requiredIntAttribute chartStack "data-chart-render-sequence"
    let readyRowsBeforeStripeRefresh = requiredIntAttribute chartStack "data-ready-row-count"
    page.Locator("[data-testid='ta-demo-clear-overview-stripes']").ClickAsync() |> awaitUnit
    waitForCount overviewStripePaths 0
    require
        (requiredIntAttribute chartStack "data-chart-render-sequence" = renderSequenceBeforeStripeRefresh
         && requiredIntAttribute chartStack "data-ready-row-count" = readyRowsBeforeStripeRefresh)
        "clearing same-topology OverviewStripe data must refresh only the navigator without remounting the chart stack"
    page.Locator("[data-testid='ta-demo-populate-overview-stripes']").ClickAsync() |> awaitUnit
    waitForCount overviewStripePaths 2
    waitForAttributeValue (overviewStripePaths.Nth(0)) "data-stripe-count" "1"
    waitForAttributeValue (overviewStripePaths.Nth(1)) "data-stripe-count" "1"
    require
        (requiredIntAttribute chartStack "data-chart-render-sequence" = renderSequenceBeforeStripeRefresh
         && requiredIntAttribute chartStack "data-ready-row-count" = readyRowsBeforeStripeRefresh)
        "populating same-topology OverviewStripe data must refresh only the navigator without remounting the chart stack"
    let orderStripePath = overviewStripePaths.Nth(0).GetAttributeAsync("d") |> awaitTask
    navigator.WaitForAsync(LocatorWaitForOptions(State = WaitForSelectorState.Visible, Timeout = 3000.0f)) |> awaitUnit
    let overviewSelection = page.Locator("[data-testid='ta-overview-selection']")
    overviewSelection.WaitForAsync(LocatorWaitForOptions(State = WaitForSelectorState.Visible, Timeout = 3000.0f)) |> awaitUnit
    let navigatorBox = navigator.BoundingBoxAsync() |> awaitTask
    let selectionBox = overviewSelection.BoundingBoxAsync() |> awaitTask
    require (not (isNull navigatorBox) && not (isNull selectionBox)) "overview navigator and selection must expose pointer geometry"
    require (page.Locator("[data-testid='ta-overview-left-handle']").IsVisibleAsync() |> awaitTask) "overview must expose a left resize handle"
    require (page.Locator("[data-testid='ta-overview-right-handle']").IsVisibleAsync() |> awaitTask) "overview must expose a right resize handle"
    let leftVisibleHandleSelector = "[data-testid='ta-overview-left-handle-visual']"
    let rightVisibleHandleSelector = "[data-testid='ta-overview-right-handle-visual']"
    let leftVisibleHandle = page.Locator(leftVisibleHandleSelector)
    let rightVisibleHandle = page.Locator(rightVisibleHandleSelector)
    require (attributeOrEmpty leftVisibleHandle "stroke" = "#4ade80") "initial left overview boundary must use the agreed bright-green color"
    require (attributeOrEmpty rightVisibleHandle "stroke" = "#4ade80") "initial right overview boundary must use the agreed bright-green color"
    let leftHandleStroke = requireFixedCssStroke longTaskSession leftVisibleHandleSelector leftVisibleHandle 2.0 2.0
    let rightHandleStroke = requireFixedCssStroke longTaskSession rightVisibleHandleSelector rightVisibleHandle 2.0 2.0
    require (not ((attributeOrEmpty leftVisibleHandle "style").Contains "translateX")) "non-edge left overview boundary must not be shifted"
    require ((attributeOrEmpty rightVisibleHandle "style").Contains "translateX(-1px)") "right-edge overview boundary must shift inward by half its two-pixel stroke"
    let leftHandleHit = page.Locator("rect[data-testid='ta-overview-left-handle']")
    let rightHandleHit = page.Locator("rect[data-testid='ta-overview-right-handle']")
    require (attributeOrEmpty leftHandleHit "fill" = "transparent") "left overview drag hit target must remain transparent"
    require (attributeOrEmpty rightHandleHit "fill" = "transparent") "right overview drag hit target must remain transparent"
    require (attributeOrEmpty leftHandleHit "pointer-events" = "none") "left cursor hint must not bypass the root drag resolver"
    require (attributeOrEmpty rightHandleHit "pointer-events" = "none") "right cursor hint must not bypass the root drag resolver"
    require (attributeOrEmpty (page.Locator("[data-testid='ta-overview-move-hit']")) "pointer-events" = "none") "move cursor hint must not bypass the root drag resolver"
    require (attributeOrEmpty leftHandleHit "width" = "8") "left overview drag hit target must retain the existing width"
    require (attributeOrEmpty rightHandleHit "width" = "8") "right overview drag hit target must retain the existing width"
    let stripeXMatch = Text.RegularExpressions.Regex.Match(orderStripePath, "M ([0-9.]+) 0")
    require stripeXMatch.Success ("overview stripe path did not expose canonical X: " + orderStripePath)
    let stripeX = Single.Parse(stripeXMatch.Groups[1].Value, Globalization.CultureInfo.InvariantCulture)
    page.Mouse.MoveAsync(navigatorBox.X + navigatorBox.Width * stripeX / 1000.0f, navigatorBox.Y + 8.0f) |> awaitUnit
    page.Locator("[data-testid='ta-overview-stripe-tooltip']").WaitForAsync(LocatorWaitForOptions(State = WaitForSelectorState.Visible, Timeout = 3000.0f)) |> awaitUnit
    let callbackState = setupCallbackState
    let renderSequenceBeforeDrag = requiredIntAttribute chartStack "data-chart-render-sequence"
    let navigatorY = navigatorBox.Y + navigatorBox.Height / 2.0f
    page.Mouse.MoveAsync(selectionBox.X + selectionBox.Width / 2.0f, navigatorY) |> awaitUnit
    page.Mouse.DownAsync(MouseDownOptions(Button = MouseButton.Left)) |> awaitUnit
    page.Mouse.MoveAsync(navigatorBox.X + selectionBox.Width / 2.0f, navigatorY, MouseMoveOptions(Steps = 12)) |> awaitUnit
    let viewportRange = page.Locator("[data-testid='ta-viewport-range']")
    waitForText viewportRange "Preview"
    let previewText = textOf viewportRange
    let previewMatch = Text.RegularExpressions.Regex.Match(previewText, "Preview ([0-9]+-[0-9]+)")
    require previewMatch.Success ("move drag did not expose bounded preview range: " + previewText)
    require (requiredIntAttribute chartStack "data-chart-render-sequence" = renderSequenceBeforeDrag) "drag preview must not rebuild the chart"
    require (chartStack.GetAttributeAsync("data-visible-start") |> awaitTask = string initialVisibleStart) "committed viewport must remain stable before release"
    page.Mouse.UpAsync(MouseUpOptions(Button = MouseButton.Left)) |> awaitUnit
    waitForText viewportRange "Viewing"
    let committedText = textOf viewportRange
    let committedMatch = Text.RegularExpressions.Regex.Match(committedText, "Viewing ([0-9]+)-([0-9]+)")
    require committedMatch.Success ("release did not publish committed bounds: " + committedText)
    let committedStart = Int32.Parse committedMatch.Groups[1].Value
    let committedEnd = Int32.Parse committedMatch.Groups[2].Value
    printfn
        "browser.navigator-drag mode=%s delta=%s outcome=%s committedStart=%s draftStart=%s requestedStart=%s preview=%s committed=%s"
        (attributeOrEmpty chartStack "data-drag-mode")
        (attributeOrEmpty chartStack "data-drag-last-delta")
        (attributeOrEmpty chartStack "data-drag-outcome")
        (attributeOrEmpty chartStack "data-drag-committed-start")
        (attributeOrEmpty chartStack "data-drag-draft-start")
        (attributeOrEmpty chartStack "data-drag-requested-start")
        previewText
        committedText
    require
        (committedEnd - committedStart + 1 = visiblePointCount && committedStart < initialVisibleStart)
        ("move release must commit one historical 48-bar window: " + committedText)
    require (requiredIntAttribute chartStack "data-chart-render-sequence" = renderSequenceBeforeDrag + 1) "release must commit exactly one chart render"
    require (chartStack.GetAttributeAsync("data-follow-latest") |> awaitTask = "false") "historical viewport navigation must leave follow-latest mode"
    requireFixedCssStroke longTaskSession leftVisibleHandleSelector leftVisibleHandle 2.0 2.0 |> ignore
    requireFixedCssStroke longTaskSession rightVisibleHandleSelector rightVisibleHandle 2.0 2.0 |> ignore
    require (not ((attributeOrEmpty leftVisibleHandle "style").Contains "translateX")) "moved left overview boundary must use its unshifted two-pixel visual"
    require (not ((attributeOrEmpty rightVisibleHandle "style").Contains "translateX")) "moved right overview boundary must use its unshifted two-pixel visual"
    require (attributeOrEmpty leftVisibleHandle "stroke" = "#4ade80" && attributeOrEmpty rightVisibleHandle "stroke" = "#4ade80") "moved overview boundaries must retain the agreed bright-green color"
    waitForText callbackState (callbackText 1 "VisibleRangeChanged")

    let priceChart = page.Locator("[data-testid='ta-candle-price']")
    let pointerBox = priceChart.BoundingBoxAsync() |> awaitTask
    require (not (isNull pointerBox)) "price chart must expose pointer geometry"
    let timeLabels = page.Locator("[data-testid='ta-time-axis-price'] span")
    let timeLabelCount = timeLabels.CountAsync() |> awaitTask
    require (timeLabelCount >= 3) "price row X axis must expose adaptive event-time labels"
    let firstTimeLabel = textOf (timeLabels.Nth(0))
    let expectedMiddleLabel = textOf (timeLabels.Nth(timeLabelCount / 2))
    let lastTimeLabel = textOf (timeLabels.Nth(timeLabelCount - 1))
    require (not (String.IsNullOrWhiteSpace firstTimeLabel)) "row X axis first label must not be empty"
    require (not (String.IsNullOrWhiteSpace expectedMiddleLabel)) "row X axis middle label must not be empty"
    require (not (String.IsNullOrWhiteSpace lastTimeLabel)) "row X axis last label must not be empty"
    require (firstTimeLabel <> lastTimeLabel) "row X axis endpoints must represent different bars"
    let renderSequenceBeforeCursor = requiredIntAttribute chartStack "data-chart-render-sequence"
    let firstCrosshair = crosshairs.First
    let crosshairXBefore = firstCrosshair.GetAttributeAsync("x1") |> awaitTask |> Option.ofObj |> Option.defaultValue ""
    let smaLegendValueBeforeCursor = textOf smaLegendValue
    let cursorLatency = Diagnostics.Stopwatch.StartNew()
    priceChart.HoverAsync() |> awaitUnit
    let crosshairXAfter = waitForAttributeChange firstCrosshair "x1" crosshairXBefore
    let activePointerBox = priceChart.BoundingBoxAsync() |> awaitTask
    require (not (isNull activePointerBox)) "price chart must retain pointer geometry after Playwright scrolls it into view"
    let smaLegendValueAfterCursor = waitForTextChange smaLegendValue smaLegendValueBeforeCursor
    cursorLatency.Stop()
    let cursorValues = page.Locator("[data-testid='ta-cross-scale-values']")
    waitForText cursorValues expectedMiddleLabel
    require (requiredIntAttribute chartStack "data-chart-render-sequence" = renderSequenceBeforeCursor) "pointer movement must update only the cursor overlay, not rebuild the chart stack"
    require (smaLegendValueAfterCursor <> "Undef") "cursor movement must update the existing row legend value node"
    printfn "browser.cursor-first-update hostRoundTrip=%dms diagnosticOnly=true" cursorLatency.ElapsedMilliseconds
    require ((page.Locator("[data-testid$='-crosshair'][visibility='visible']").CountAsync() |> awaitTask) = 7) "pointer movement on one row must reveal one shared crosshair in every visible row"
    let crosshairPositions =
        page.Locator("[data-testid$='-crosshair']").AllAsync()
        |> awaitTask
        |> Seq.map (fun locator -> locator.GetAttributeAsync("x1") |> awaitTask |> Option.ofObj |> Option.defaultValue "missing")
        |> Seq.distinct
        |> Seq.toArray
    require (crosshairPositions.Length = 1 && crosshairPositions[0] = crosshairXAfter && crosshairPositions[0] <> "0" && crosshairPositions[0] <> "100") ("shared pointer crosshair positions diverged: " + String.concat "," crosshairPositions)

    let visibleCursorLabels = page.Locator("[data-ta-row-cursor-label='true']")
    let cursorLabelsAreVisible () =
        visibleCursorLabels.AllAsync()
        |> awaitTask
        |> Seq.forall (fun locator -> locator.IsVisibleAsync() |> awaitTask)
    require (visibleCursorLabels.CountAsync() |> awaitTask = 7) "every visible row must expose one cursor label before resize"
    require (cursorLabelsAreVisible ()) "all row cursor labels must be visible before resize"
    let visibleCursorIndexBeforeResize = attributeOrEmpty chartStack "data-cursor-index"
    let renderSequenceBeforeVisibleCursorResize = requiredIntAttribute chartStack "data-chart-render-sequence"
    let visibleCursorBoxBeforeResize = visibleCursorLabels.First.BoundingBoxAsync() |> awaitTask
    require (not (isNull visibleCursorBoxBeforeResize)) "visible cursor label must expose geometry before resize"
    let visibleCursorResizeHeight = requiredIntAttribute priceResize "aria-valuenow"
    priceResize.FocusAsync() |> awaitUnit
    priceResize.PressAsync("Shift+ArrowUp") |> awaitUnit
    waitForAttributeChange priceResize "aria-valuenow" (string visibleCursorResizeHeight) |> ignore
    let visibleCursorBoxAfterResize = visibleCursorLabels.First.BoundingBoxAsync() |> awaitTask
    require (not (isNull visibleCursorBoxAfterResize)) "row resize must preserve visible cursor label geometry"
    require (cursorLabelsAreVisible ()) "row resize must preserve every visible cursor label"
    require ((page.Locator("[data-testid$='-crosshair'][visibility='visible']").CountAsync() |> awaitTask) = 7) "row resize must preserve every visible shared crosshair"
    require (attributeOrEmpty chartStack "data-cursor-index" = visibleCursorIndexBeforeResize) "row resize must preserve the displayed cursor index"
    require (requiredIntAttribute chartStack "data-chart-render-sequence" = renderSequenceBeforeVisibleCursorResize) "row resize must not rebuild the chart stack while preserving the cursor"
    Threading.Thread.Sleep 220
    let settledVisibleCursorBoxAfterResize = visibleCursorLabels.First.BoundingBoxAsync() |> awaitTask
    require (not (isNull settledVisibleCursorBoxAfterResize)) "row resize must preserve visible cursor label after reactive DOM settles"
    require (cursorLabelsAreVisible ()) "row resize must preserve every visible cursor label after reactive DOM settles"
    require (attributeOrEmpty chartStack "data-cursor-index" = visibleCursorIndexBeforeResize) "row resize settle must preserve the displayed cursor index"
    priceResize.PressAsync("Home") |> awaitUnit
    waitForAttributeValue priceResize "aria-valuenow" "250"
    let sustainedCursorSurface = page.Locator("svg:has([data-testid='ta-trace-sma-sma-1k'])")
    sustainedCursorSurface.ScrollIntoViewIfNeededAsync() |> awaitUnit
    let sustainedPointerBox = sustainedCursorSurface.BoundingBoxAsync() |> awaitTask
    require (not (isNull sustainedPointerBox)) "marker-free SMA chart must expose fresh pointer geometry for sustained cursor measurement"

    page.Locator("[data-testid='ta-demo-preview-stream']").ClickAsync() |> awaitUnit
    waitForIntAttributeAtLeast fixtureRoot "data-preview-stream-updates" 1 |> ignore
    let previewUpdatesBeforeCursor = requiredIntAttribute fixtureRoot "data-preview-stream-updates"
    let historicalCloseBeforeCursor = attributeSignature priceCandlePaths "d"
    let cursorTrace = startMainThreadTrace longTaskSession
    let sustainedCursor = Diagnostics.Stopwatch.StartNew()
    let mutable previousCrosshairX = crosshairXAfter
    let mutable cursorTransitions = 0
    let mutable maximumCursorLatencyMs = 0L
    let cursorLatencies = ResizeArray<int64>()
    let browserCursorLatencies = ResizeArray<float>()
    let waitForCrosshairSide rightSide =
        let deadline = DateTime.UtcNow.AddSeconds 3.0
        let mutable observed = attributeOrEmpty firstCrosshair "x1"
        let mutable reached = false
        while not reached && DateTime.UtcNow < deadline do
            match Double.TryParse(observed, Globalization.NumberStyles.Float, Globalization.CultureInfo.InvariantCulture) with
            | true, value -> reached <- if rightSide then value >= 600.0 else value <= 400.0
            | _ -> ()
            if not reached then
                Threading.Thread.Sleep 10
                observed <- attributeOrEmpty firstCrosshair "x1"
        let sideLabel = if rightSide then "right" else "left"
        require reached $"crosshair did not reach the expected {sideLabel} region; x1={observed}"
        observed
    let dispatchCursorMove ratio =
        sustainedCursorSurface.HoverAsync(
            LocatorHoverOptions(
                Force = true,
                Position = Position(X = sustainedPointerBox.Width * ratio, Y = sustainedPointerBox.Height / 2.0f)))
        |> awaitUnit
    let waitForCursorAttributeChange previous ratio =
        let deadline = DateTime.UtcNow.AddSeconds 3.0
        let mutable retryAt = DateTime.UtcNow.AddMilliseconds 250.0
        let mutable current = attributeOrEmpty firstCrosshair "x1"
        while current = previous && DateTime.UtcNow < deadline do
            Threading.Thread.Sleep 10
            if DateTime.UtcNow >= retryAt then
                dispatchCursorMove ratio
                retryAt <- DateTime.UtcNow.AddMilliseconds 250.0
            current <- attributeOrEmpty firstCrosshair "x1"
        require (current <> previous) $"expected `x1` to change from `{previous}`"
        current
    dispatchCursorMove 0.18f
    previousCrosshairX <- waitForCrosshairSide false
    let mutable browserCursorLatencySequence = requiredIntAttribute chartStack "data-cursor-render-latency-sequence"
    for sample in 0 .. 299 do
        let rightSide = sample % 2 = 0
        let ratio = if rightSide then 0.82f else 0.18f
        let movement = Diagnostics.Stopwatch.StartNew()
        dispatchCursorMove ratio
        let currentCrosshairX = waitForCursorAttributeChange previousCrosshairX ratio
        browserCursorLatencySequence <-
            waitForIntAttributeAtLeast
                chartStack
                "data-cursor-render-latency-sequence"
                (browserCursorLatencySequence + 1)
        browserCursorLatencies.Add(requiredFloatAttribute chartStack "data-cursor-render-latency-ms")
        movement.Stop()
        match Double.TryParse(currentCrosshairX, Globalization.NumberStyles.Float, Globalization.CultureInfo.InvariantCulture) with
        | true, value ->
            let reachedExpectedSide = if rightSide then value >= 600.0 else value <= 400.0
            let sideLabel = if rightSide then "right" else "left"
            require reachedExpectedSide $"crosshair did not reach the expected {sideLabel} region; x1={currentCrosshairX}"
        | _ -> require false $"crosshair x1 is not numeric: {currentCrosshairX}"
        cursorTransitions <- cursorTransitions + 1
        maximumCursorLatencyMs <- max maximumCursorLatencyMs movement.ElapsedMilliseconds
        cursorLatencies.Add movement.ElapsedMilliseconds
        previousCrosshairX <- currentCrosshairX
    sustainedCursor.Stop()
    Threading.Thread.Sleep 180
    longTaskPhases.Add(stopMainThreadTrace "cursor-movement" longTaskSession cursorTrace)
    let sortedCursorLatencies = cursorLatencies |> Seq.sort |> Seq.toArray
    let cursorP95 = sortedCursorLatencies[int (Math.Ceiling(float sortedCursorLatencies.Length * 0.95)) - 1]
    let sortedBrowserCursorLatencies = browserCursorLatencies |> Seq.sort |> Seq.toArray
    let browserCursorP95 = sortedBrowserCursorLatencies[int (Math.Ceiling(float sortedBrowserCursorLatencies.Length * 0.95)) - 1]
    let browserCursorMax = sortedBrowserCursorLatencies |> Array.max
    printfn "browser.cursor sustainedTransitions=%d eventToRenderP95=%.2fms eventToRenderMax=%.2fms hostRoundTripP95=%dms hostRoundTripMax=%dms elapsed=%dms" cursorTransitions browserCursorP95 browserCursorMax cursorP95 maximumCursorLatencyMs sustainedCursor.ElapsedMilliseconds
    require (cursorTransitions >= 300) $"sustained cursor movement produced too few crosshair transitions: {cursorTransitions}"
    if not skipPerformanceGates then
        require (browserCursorP95 < 125.0) $"sustained cursor browser event-to-render p95 exceeded 125ms: {browserCursorP95:F2}ms"
        require (browserCursorMax < 400.0) $"sustained cursor browser event-to-render max exceeded 400ms: {browserCursorMax:F2}ms"
    let concurrentPreviewUpdates = requiredIntAttribute fixtureRoot "data-preview-stream-updates"
    let concurrentPreviewUpdateCount = concurrentPreviewUpdates - previewUpdatesBeforeCursor
    require (concurrentPreviewUpdateCount >= 2) $"sustained cursor gate observed only {concurrentPreviewUpdateCount} concurrent live preview updates"
    let historicalCloseAfterCursor = attributeSignature priceCandlePaths "d"
    require (historicalCloseAfterCursor = historicalCloseBeforeCursor) "realtime tail updates must not overwrite the committed historical viewport"
    require (requiredIntAttribute chartStack "data-chart-render-sequence" = renderSequenceBeforeCursor) "sustained pointer movement must not rebuild the chart stack"
    let sustainedLegendBox = smaLegend.BoundingBoxAsync() |> awaitTask
    let sustainedValueBox = smaLegendValue.BoundingBoxAsync() |> awaitTask
    let sustainedRowBox = page.Locator("[data-testid='ta-row-sma']").BoundingBoxAsync() |> awaitTask
    require (abs (sustainedLegendBox.Height - initialLegendHeights[2]) <= 0.5f && abs (sustainedValueBox.Width - initialValueBox.Width) <= 0.5f && abs (sustainedRowBox.Height - smaRowBox.Height) <= 0.5f) "cursor and concurrent live revisions must not change row legend geometry"

    let cursorCommitTrace = startMainThreadTrace longTaskSession
    priceChart.ClickAsync() |> awaitUnit
    waitForText callbackState (callbackText 2 "SharedCursorChanged")
    Threading.Thread.Sleep 180
    longTaskPhases.Add(stopMainThreadTrace "cursor-commit" longTaskSession cursorCommitTrace)
    Directory.CreateDirectory outputDirectory |> ignore
    page.ScreenshotAsync(PageScreenshotOptions(Path = Path.Combine(outputDirectory, "desktop-crossrow-cursor.png"), FullPage = true)) |> awaitTask |> ignore

    page.Locator("[data-testid='ta-demo-paused']").ClickAsync() |> awaitUnit
    waitForText (page.Locator("[data-testid='ta-poll-state']")) "RESYNC"
    require (page.Locator("[data-testid='ta-apply-query']").IsDisabledAsync() |> awaitTask) "paused cache must suppress remote query commands"
    let pausedVisibleStartBefore = requiredIntAttribute chartStack "data-visible-start"
    let pausedPanControl =
        if pausedVisibleStartBefore > 0 then page.Locator("[data-testid='ta-pan-left']")
        else page.Locator("[data-testid='ta-pan-right']")
    require (not (pausedPanControl.IsDisabledAsync() |> awaitTask)) "paused cache must retain local viewport navigation"
    let pausedViewportBefore = textOf viewportRange
    pausedPanControl.ClickAsync() |> awaitUnit
    waitForAttributeChange chartStack "data-visible-start" (string pausedVisibleStartBefore) |> ignore
    require (textOf viewportRange <> pausedViewportBefore) "paused local pan must update the visible viewport"
    priceChart.HoverAsync() |> awaitUnit
    require ((page.Locator("[data-testid$='-crosshair']").CountAsync() |> awaitTask) = 7) "paused cache must retain local hover/crosshair"
    priceChart.ClickAsync() |> awaitUnit
    System.Threading.Thread.Sleep 250
    requireText callbackState (callbackText 2 "SharedCursorChanged")

    let chartPointsBeforeStatusChange = requiredIntAttribute (page.Locator("[data-testid='ta-candle-price']")) "data-point-count"
    page.Locator("[data-testid='ta-demo-inflight']").ClickAsync() |> awaitUnit
    waitForText (page.Locator("[data-testid='ta-poll-state']")) "UPDATING"
    require (page.Locator("[data-testid='ta-apply-query']").IsDisabledAsync() |> awaitTask) "remote query must be disabled while a poll is in flight"
    require (not (page.Locator("[data-testid='ta-pan-left']").IsDisabledAsync() |> awaitTask)) "local viewport controls must remain available while a poll is in flight"
    page.Locator("[data-testid='ta-add-row-toggle']").ClickAsync() |> awaitUnit
    require (page.Locator("[data-testid='ta-add-row-submit']").IsDisabledAsync() |> awaitTask) "remote Add Row submit must be disabled while a poll is in flight"
    page.Locator("[data-testid='ta-add-row-cancel']").ClickAsync() |> awaitUnit
    page.Locator("[data-testid='ta-demo-stale']").ClickAsync() |> awaitUnit
    waitForText (page.Locator("[data-testid='ta-freshness']")) "STALE"
    requireText (page.Locator("[data-testid='ta-status-detail']")) "quality gap suspected"
    requireText (page.Locator("[data-testid='ta-last-good-error']")) "retaining last good canvas"
    require (requiredIntAttribute (page.Locator("[data-testid='ta-candle-price']")) "data-point-count" = chartPointsBeforeStatusChange) "stale transport status must retain the last-good canvas"
    page.Locator("[data-testid='ta-demo-live']").ClickAsync() |> awaitUnit
    waitForText (page.Locator("[data-testid='ta-poll-state']")) "READY"
    require (not (page.Locator("[data-testid='ta-apply-query']").IsDisabledAsync() |> awaitTask)) "remote query must recover after the runtime returns to ready"

    let titleBox = page.Locator("[data-testid='ta-workspace-title']").BoundingBoxAsync() |> awaitTask
    let priceBox = page.Locator("[data-testid='ta-candle-price']").BoundingBoxAsync() |> awaitTask
    require (not (isNull titleBox)) "title must be visible"
    require (not (isNull priceBox)) "price chart must be visible"
    require (priceBox.Y < 900.0f) $"primary price chart must enter first viewport, y={priceBox.Y}"
    require (priceBox.Width > 1100.0f) $"desktop chart should use available width, width={priceBox.Width}"

    requireText callbackState $"callback actions {setupCallbackCount + 2}"
    page.Locator("[data-testid='ta-pan-right']").ClickAsync() |> awaitUnit
    waitForText callbackState (callbackText 3 "VisibleRangeChanged")
    page.Locator("[data-testid='ta-zoom-in']").ClickAsync() |> awaitUnit
    waitForText callbackState (callbackText 4 "VisibleRangeChanged")

    page.Locator("[data-testid='ta-reset-view']").ClickAsync() |> awaitUnit
    waitForAttributeValue (page.Locator("[data-testid='ta-chart-stack']")) "data-follow-latest" "true"
    waitForText callbackState (callbackText 5 "VisibleRangeChanged")

    let volumeRow = page.Locator("[data-testid='ta-row-volume']")
    require (volumeRow.IsVisibleAsync() |> awaitTask) "volume row must begin visible"
    page.Locator("[data-testid='ta-toggle-row-volume']").ClickAsync() |> awaitUnit
    volumeRow.WaitForAsync(LocatorWaitForOptions(State = WaitForSelectorState.Hidden, Timeout = 3000.0f)) |> awaitUnit
    requireText callbackState $"callback actions {setupCallbackCount + 5}"
    page.Locator("[data-testid='ta-toggle-row-volume']").ClickAsync() |> awaitUnit
    volumeRow.WaitForAsync(LocatorWaitForOptions(State = WaitForSelectorState.Visible, Timeout = 3000.0f)) |> awaitUnit

    page.Locator("[data-testid='ta-add-row-toggle']").ClickAsync() |> awaitUnit
    let editor = page.Locator("[data-testid='ta-add-row-editor']")
    editor.WaitForAsync(LocatorWaitForOptions(State = WaitForSelectorState.Visible, Timeout = 3000.0f)) |> awaitUnit
    page.Locator("[data-testid='ta-demo-live']").ClickAsync() |> awaitUnit
    System.Threading.Thread.Sleep 350
    require (editor.IsVisibleAsync() |> awaitTask) "status refresh must not collapse an unfinished Add Row editor"
    page.Locator("[data-testid='ta-editor-template']").SelectOptionAsync("ta.macd") |> awaitTask |> ignore
    page.Locator("[data-testid='ta-editor-periods-fast']").FillAsync("13") |> awaitUnit
    page.Locator("[data-testid='ta-editor-periods-slow']").FillAsync("21") |> awaitUnit
    page.Locator("[data-testid='ta-editor-periods-signal']").FillAsync("7") |> awaitUnit
    require ((page.Locator("[data-testid^='ta-editor-periods-']").CountAsync() |> awaitTask) = 3) "generic MACD schema must expose fast, slow and signal fields"
    page.Locator("[data-testid='ta-add-row-submit']").ClickAsync() |> awaitUnit
    require (editor.IsVisibleAsync() |> awaitTask) "accepted Add must remain pending until the authoritative document arrives"
    waitForDisabled (page.Locator("[data-testid='ta-add-row-submit']")) "pending Add submit"
    editor.WaitForAsync(LocatorWaitForOptions(State = WaitForSelectorState.Hidden, Timeout = 8000.0f)) |> awaitUnit
    waitForText callbackState "last ApplyTemplate ta.macd"
    require ((page.Locator("[data-testid='ta-toggle-row-template-ta-macd-8']").CountAsync() |> awaitTask) = 1) "generic Add must append one authoritative MACD row"

    let rowCountBeforeEdit = page.Locator("[data-testid^='ta-toggle-row-']").CountAsync() |> awaitTask
    let editSma = page.Locator("[data-testid='ta-edit-row-sma']")
    waitForEnabled editSma "SMA Edit after Add"
    editSma.ClickAsync() |> awaitUnit
    editor.WaitForAsync(LocatorWaitForOptions(State = WaitForSelectorState.Visible, Timeout = 3000.0f)) |> awaitUnit
    requireText (page.Locator("[data-testid='ta-row-editor-mode']")) "Editing sma"
    require ((page.Locator("[data-testid='ta-editor-periods-0']").InputValueAsync() |> awaitTask) = "13") "Edit must prefill the persisted row binding"
    page.Locator("[data-testid='ta-editor-periods-0']").FillAsync("34") |> awaitUnit
    page.Locator("[data-testid='ta-demo-reject-next']").ClickAsync() |> awaitUnit
    page.Locator("[data-testid='ta-add-row-submit']").ClickAsync() |> awaitUnit
    waitForText (page.Locator("[data-testid='ta-feedback']")) "rejected"
    require (editor.IsVisibleAsync() |> awaitTask) "rejected Edit must preserve the editor"
    require ((page.Locator("[data-testid^='ta-toggle-row-']").CountAsync() |> awaitTask) = rowCountBeforeEdit) "rejected Edit must preserve the row set"
    require ((page.Locator("[data-testid='ta-editor-periods-0']").InputValueAsync() |> awaitTask) = "34") "rejected Edit must preserve draft values"
    page.Locator("[data-testid='ta-add-row-submit']").ClickAsync() |> awaitUnit
    editor.WaitForAsync(LocatorWaitForOptions(State = WaitForSelectorState.Hidden, Timeout = 8000.0f)) |> awaitUnit
    require ((page.Locator("[data-testid^='ta-toggle-row-']").CountAsync() |> awaitTask) = rowCountBeforeEdit) "accepted Edit must preserve row count"
    waitForEnabled editSma "SMA Edit after accepted Edit"
    editSma.ClickAsync() |> awaitUnit
    require ((page.Locator("[data-testid='ta-editor-periods-0']").InputValueAsync() |> awaitTask) = "34") "accepted Edit must retain the new binding for the same row"
    page.Locator("[data-testid='ta-add-row-cancel']").ClickAsync() |> awaitUnit

    page.Locator("[data-testid='ta-remove-trace-volume-volume']").ClickAsync() |> awaitUnit
    volumeRow.WaitForAsync(LocatorWaitForOptions(State = WaitForSelectorState.Hidden, Timeout = 3000.0f)) |> awaitUnit

    page.Locator("[data-testid='ta-apply-query']").ClickAsync() |> awaitUnit
    waitForText callbackState "last ChangeTaQuery"
    let renderBeforeResetCanvas = requiredIntAttribute chartStack "data-chart-render-sequence"
    page.Locator("[data-testid='ta-reset-canvas']").ClickAsync() |> awaitUnit
    waitForText callbackState "last ResetCanvas"
    waitForIntAttributeAtLeast chartStack "data-chart-render-sequence" (renderBeforeResetCanvas + 1) |> ignore
    waitForIntAttribute chartStack "data-ready-row-count" 7
    volumeRow.WaitForAsync(LocatorWaitForOptions(State = WaitForSelectorState.Visible, Timeout = 3000.0f)) |> awaitUnit
    require ((page.Locator("[data-testid='ta-row-template-ta-macd-8']").CountAsync() |> awaitTask) = 0) "Reset Canvas must remove post-mount added rows"

    let callbackCountBeforePreset200 = requiredIntAttribute callbackState "data-callback-count"
    page.Locator("[data-testid='ta-view-200']").ClickAsync() |> awaitUnit
    waitForText (page.Locator("[data-testid='ta-viewport-range']")) "Viewing 3801-4000"
    waitForIntAttribute chartStack "data-ready-row-count" 7
    waitForIntAttribute callbackState "data-callback-count" (callbackCountBeforePreset200 + 1)
    waitForEnabled (page.Locator("[data-testid='ta-view-all']")) "All preset after 200"
    let callbackCountBeforeAll = requiredIntAttribute callbackState "data-callback-count"
    let allTransition = Diagnostics.Stopwatch.StartNew()
    let allStateTransition = Diagnostics.Stopwatch.StartNew()
    let allTrace = startMainThreadTrace longTaskSession
    page.Locator("[data-testid='ta-view-all']").ClickAsync() |> awaitUnit
    waitForText (page.Locator("[data-testid='ta-viewport-range']")) $"Viewing 1-{capacityPointCount}"
    allStateTransition.Stop()
    page.Locator("[data-testid='ta-row-heikin']").WaitForAsync(LocatorWaitForOptions(State = WaitForSelectorState.Visible, Timeout = 15000.0f)) |> awaitUnit
    waitForIntAttribute chartStack "data-ready-row-count" 7
    allTransition.Stop()
    printfn "browser.200-to-all stateMs=%.2f rowsReadyMs=%.2f" allStateTransition.Elapsed.TotalMilliseconds allTransition.Elapsed.TotalMilliseconds
    require (allStateTransition.Elapsed.TotalMilliseconds <= 750.0) $"owner 200-to-All committed state exceeded 750ms: {allStateTransition.Elapsed.TotalMilliseconds:F2}ms"
    require (allTransition.Elapsed.TotalMilliseconds <= 1500.0) $"owner 200-to-All transition exceeded 1500ms: {allTransition.Elapsed.TotalMilliseconds:F2}ms"
    Threading.Thread.Sleep 180
    longTaskPhases.Add(stopMainThreadTrace "all" longTaskSession allTrace)
    printVisibleValueTelemetry "all" page chartStack
    waitForIntAttribute callbackState "data-callback-count" (callbackCountBeforeAll + 1)
    waitForAttributeValue callbackState "data-last-action" "VisibleRangeChanged"
    waitForEnabled (page.Locator("[data-testid='ta-pan-left']")) "viewport controls after All"

    let callbackCountBeforeRapidPresets = requiredIntAttribute callbackState "data-callback-count"
    page.Locator("[data-testid='ta-view-200']").ClickAsync() |> awaitUnit
    waitForText (page.Locator("[data-testid='ta-viewport-range']")) "Viewing 3801-4000"
    require (requiredIntAttribute callbackState "data-callback-count" = callbackCountBeforeRapidPresets) "200 action must still be pending before the rapid All intent"
    let rapidAllState = Diagnostics.Stopwatch.StartNew()
    page.Locator("[data-testid='ta-view-all']").ClickAsync() |> awaitUnit
    waitForText (page.Locator("[data-testid='ta-viewport-range']")) $"Viewing 1-{capacityPointCount}"
    rapidAllState.Stop()
    require (rapidAllState.Elapsed.TotalMilliseconds <= 750.0) $"rapid pending 200-to-All local commit exceeded 750ms: {rapidAllState.Elapsed.TotalMilliseconds:F2}ms"
    waitForIntAttribute callbackState "data-callback-count" (callbackCountBeforeRapidPresets + 2)
    waitForAttributeValue callbackState "data-last-action" "VisibleRangeChanged"

    let renderBeforeRightHandle = requiredIntAttribute chartStack "data-chart-render-sequence"
    let renderReasonBeforeRightHandle = chartStack.GetAttributeAsync("data-chart-render-reason") |> awaitTask
    let documentRevisionBeforeRightHandle = chartStack.GetAttributeAsync("data-chart-document-revision") |> awaitTask
    require (requiredIntAttribute (page.Locator("[data-testid='ta-candle-price']")) "data-point-count" = capacityPointCount) "All preset must render the full loaded capacity range"

    let markerTrace = startMainThreadTrace longTaskSession
    page.Locator("[data-testid='ta-demo-replace-markers']").ClickAsync() |> awaitUnit
    waitForText (page.Locator("[data-testid='ta-marker-signals-long-entry'] title")) "replacement 2"
    Threading.Thread.Sleep 180
    longTaskPhases.Add(stopMainThreadTrace "marker-replacement" longTaskSession markerTrace)
    printVisibleValueTelemetry "marker-replacement" page chartStack
    let renderAfterMarkerReplacement = requiredIntAttribute chartStack "data-chart-render-sequence"
    let renderReasonAfterMarkerReplacement = chartStack.GetAttributeAsync("data-chart-render-reason") |> awaitTask
    printfn
        "browser.navigator-sequence all=%d(%s,doc=%s) marker=%d(%s) docRevision=%s dataRevision=%s transportSequence=%s"
        renderBeforeRightHandle
        renderReasonBeforeRightHandle
        documentRevisionBeforeRightHandle
        renderAfterMarkerReplacement
        renderReasonAfterMarkerReplacement
        (chartStack.GetAttributeAsync("data-chart-document-revision") |> awaitTask)
        (chartStack.GetAttributeAsync("data-chart-data-revision") |> awaitTask)
        (chartStack.GetAttributeAsync("data-chart-transport-sequence") |> awaitTask)
    require
        (renderAfterMarkerReplacement = renderBeforeRightHandle)
        "same-topology marker replacement must refresh overlay row data without rebuilding the chart stack"
    let allNavigatorBox = navigator.BoundingBoxAsync() |> awaitTask
    let rightHandle = rightHandleHit
    let rightHandleBox = rightHandle.BoundingBoxAsync() |> awaitTask
    require (not (isNull rightHandleBox)) "right overview handle must expose geometry"
    page.Mouse.MoveAsync(rightHandleBox.X + rightHandleBox.Width / 2.0f, rightHandleBox.Y + rightHandleBox.Height / 2.0f) |> awaitUnit
    page.Mouse.DownAsync(MouseDownOptions(Button = MouseButton.Left)) |> awaitUnit
    page.Mouse.MoveAsync(allNavigatorBox.X + allNavigatorBox.Width * 0.75f, allNavigatorBox.Y + allNavigatorBox.Height / 2.0f, MouseMoveOptions(Steps = 8)) |> awaitUnit
    System.Threading.Thread.Sleep 50
    let renderAfterRightHandlePreview = requiredIntAttribute chartStack "data-chart-render-sequence"
    printfn "browser.navigator-sequence preview=%d" renderAfterRightHandlePreview
    require ((textOf (page.Locator("[data-testid='ta-viewport-range']"))).Contains "Preview") "right-handle drag must publish preview bounds"
    require (renderAfterRightHandlePreview = renderAfterMarkerReplacement) "right-handle preview must not rebuild after the All preset render"
    page.Mouse.UpAsync(MouseUpOptions(Button = MouseButton.Left)) |> awaitUnit
    waitForIntAttribute chartStack "data-chart-render-sequence" (renderAfterMarkerReplacement + 1)
    waitForEnabled (page.Locator("[data-testid='ta-pan-left']")) "viewport controls after right-handle commit"
    let resizedNavigatorBox = navigator.BoundingBoxAsync() |> awaitTask
    let leftHandle = leftHandleHit
    let leftHandleBox = leftHandle.BoundingBoxAsync() |> awaitTask
    let renderBeforeLeftHandle = requiredIntAttribute chartStack "data-chart-render-sequence"
    require (not (isNull leftHandleBox)) "left overview handle must expose geometry"
    page.Mouse.MoveAsync(leftHandleBox.X + leftHandleBox.Width / 2.0f, leftHandleBox.Y + leftHandleBox.Height / 2.0f) |> awaitUnit
    page.Mouse.DownAsync(MouseDownOptions(Button = MouseButton.Left)) |> awaitUnit
    page.Mouse.MoveAsync(resizedNavigatorBox.X + resizedNavigatorBox.Width * 0.25f, resizedNavigatorBox.Y + resizedNavigatorBox.Height / 2.0f, MouseMoveOptions(Steps = 8)) |> awaitUnit
    System.Threading.Thread.Sleep 50
    require ((textOf (page.Locator("[data-testid='ta-viewport-range']"))).Contains "Preview") "left-handle drag must publish preview bounds"
    require (requiredIntAttribute chartStack "data-chart-render-sequence" = renderBeforeLeftHandle) "left-handle preview must not rebuild the chart"
    page.Mouse.UpAsync(MouseUpOptions(Button = MouseButton.Left)) |> awaitUnit
    waitForIntAttribute chartStack "data-chart-render-sequence" (renderBeforeLeftHandle + 1)
    waitForEnabled (page.Locator("[data-testid='ta-pan-left']")) "viewport controls after left-handle commit"
    require
        (requireFixedCssStroke longTaskSession leftVisibleHandleSelector leftVisibleHandle 2.0 2.0 = leftHandleStroke
         && requireFixedCssStroke longTaskSession rightVisibleHandleSelector rightVisibleHandle 2.0 2.0 = rightHandleStroke)
        "overview visible boundary CSS-pixel strokes must survive viewport resize commits"

    page.Locator("[data-testid='ta-view-48']").ClickAsync() |> awaitUnit
    waitForText (page.Locator("[data-testid='ta-viewport-range']")) $"Viewing {initialVisibleStart}-{capacityPointCount}"
    waitForEnabled (page.Locator("[data-testid='ta-pan-left']")) "viewport controls before document replacement"
    let replacementTrace = startMainThreadTrace longTaskSession
    page.Locator("[data-testid='ta-demo-replace-document']").ClickAsync() |> awaitUnit
    waitForText (page.Locator("[data-testid='ta-workspace-title']")) "SMA(30)"
    waitForText (page.Locator("[data-testid='ta-canvas-identity']")) "ta-demo-canvas-replacement"
    waitForIntAttribute chartStack "data-ready-row-count" 7
    Threading.Thread.Sleep 180
    longTaskPhases.Add(stopMainThreadTrace "document-replacement" longTaskSession replacementTrace)
    printVisibleValueTelemetry "document-replacement" page chartStack
    requireText (page.Locator("[data-testid='ta-toggle-row-price']")) "ES 1K + SMA(30)"
    requireText (page.Locator("[data-testid='ta-row-price']")) "ES 1K + SMA(30)"
    require (not ((textOf (page.Locator("[data-testid='ta-row-price']"))).Contains "SMA(20)")) "replacement document must not retain the prior static row label"

    page.Locator("[data-testid='ta-view-all']").ClickAsync() |> awaitUnit
    waitForText (page.Locator("[data-testid='ta-viewport-range']")) $"Viewing 1-{capacityPointCount}"
    waitForEnabled (page.Locator("[data-testid='ta-pan-right']")) "viewport controls before later coverage request"
    let overviewPath = navigator.Locator("path").First
    let overviewBeforeExtension = overviewPath.GetAttributeAsync("d") |> awaitTask
    let coverageTrace = startMainThreadTrace longTaskSession
    page.Locator("[data-testid='ta-pan-right']").ClickAsync() |> awaitUnit
    waitForIntAttribute chartStack "data-loaded-bars" (capacityPointCount + 400)
    waitForIntAttribute chartStack "data-ready-row-count" 7
    let extendedStart = requiredIntAttribute chartStack "data-visible-start"
    let extendedEnd = requiredIntAttribute chartStack "data-visible-end"
    require (extendedEnd - extendedStart + 1 <= 4000) "progressive coverage must not exceed MaximumVisibleBars"
    require (extendedStart > 1 && extendedEnd = capacityPointCount + 400) "later coverage must preserve the rightward pan intent"
    requireText (page.Locator("[data-testid='ta-view-all']")) "Max 4000"
    let overviewAfterExtension = waitForAttributeChange overviewPath "d" overviewBeforeExtension
    require (overviewAfterExtension <> overviewBeforeExtension) "overview must densify against the expanded loaded domain"
    Threading.Thread.Sleep 180
    longTaskPhases.Add(stopMainThreadTrace "progressive-coverage" longTaskSession coverageTrace)
    printVisibleValueTelemetry "progressive-coverage" page chartStack

    let loadedForTinySelection = requiredIntAttribute chartStack "data-loaded-bars"
    page.Locator("[data-testid='ta-view-48']").ClickAsync() |> awaitUnit
    waitForIntAttribute chartStack "data-visible-start" (loadedForTinySelection - 47)
    for expectedCount in [| 40; 32; 24; 16; 12 |] do
        page.Locator("[data-testid='ta-zoom-in']").ClickAsync() |> awaitUnit
        waitForIntAttribute chartStack "data-visible-start" (loadedForTinySelection - expectedCount + 1)
        waitForIntAttribute chartStack "data-visible-end" loadedForTinySelection
    waitForEnabled (page.Locator("[data-testid='ta-zoom-in']")) "viewport controls before tiny-selection drag"

    let tinySelection = page.Locator("[data-testid='ta-overview-selection']")
    let tinyWidth = requiredFloatAttribute tinySelection "width"
    let expectedTinyWidth = 1000.0 * 12.0 / float loadedForTinySelection
    require (abs (tinyWidth - expectedTinyWidth) < 0.002) $"minimum-bars visual width must preserve the exact ratio, expected={expectedTinyWidth:F6}, actual={tinyWidth:F6}"
    require (tinyWidth < 24.0) $"minimum-bars selection must not retain the legacy 24-unit floor, actual={tinyWidth:F6}"
    require (requiredFloatAttribute navigator "data-drag-hit-target-css-pixels" = 24.0) "navigator must publish the CSS-pixel hit-target contract"

    let tinyNavigatorBox = navigator.BoundingBoxAsync() |> awaitTask
    let tinySelectionBox = tinySelection.BoundingBoxAsync() |> awaitTask
    require (not (isNull tinyNavigatorBox) && not (isNull tinySelectionBox)) "minimum-bars navigator geometry must be measurable"
    let tinyY = tinyNavigatorBox.Y + tinyNavigatorBox.Height / 2.0f
    let tinyStart = requiredIntAttribute chartStack "data-visible-start"
    let tinyLeftRatioText = attributeOrEmpty navigator "data-selection-left-ratio"
    let tinyLeftRatio = requiredFloatAttribute navigator "data-selection-left-ratio"
    let tinyRightRatio = requiredFloatAttribute navigator "data-selection-right-ratio"
    let tinyCenterX = tinySelectionBox.X + tinySelectionBox.Width / 2.0f
    printfn
        "browser.navigator-tiny navX=%.3f navWidth=%.3f selectionX=%.3f selectionWidth=%.3f leftRatio=%.9f rightRatio=%.9f centerX=%.3f"
        tinyNavigatorBox.X
        tinyNavigatorBox.Width
        tinySelectionBox.X
        tinySelectionBox.Width
        tinyLeftRatio
        tinyRightRatio
        tinyCenterX
    page.Mouse.MoveAsync(tinyCenterX, tinyY) |> awaitUnit
    Threading.Thread.Sleep 50
    page.Mouse.DownAsync(MouseDownOptions(Button = MouseButton.Left)) |> awaitUnit
    page.Mouse.MoveAsync(tinyCenterX - 80.0f, tinyY, MouseMoveOptions(Steps = 8)) |> awaitUnit
    waitForText viewportRange "Preview"
    page.Mouse.UpAsync(MouseUpOptions(Button = MouseButton.Left)) |> awaitUnit
    waitForAttributeChange chartStack "data-visible-start" (string tinyStart) |> ignore
    waitForAttributeChange navigator "data-selection-left-ratio" tinyLeftRatioText |> ignore
    require (requiredIntAttribute chartStack "data-visible-end" - requiredIntAttribute chartStack "data-visible-start" + 1 = 12) "tiny-selection middle zone must move without resizing"
    waitForEnabled (page.Locator("[data-testid='ta-pan-left']")) "viewport controls before tiny-selection resize"

    let movedTinyLeftRatio = requiredFloatAttribute navigator "data-selection-left-ratio"
    let movedTinyLeftX = tinyNavigatorBox.X + tinyNavigatorBox.Width * float32 movedTinyLeftRatio
    let startBeforeLeftResize = requiredIntAttribute chartStack "data-visible-start"
    page.Mouse.MoveAsync(movedTinyLeftX - 8.0f, tinyY) |> awaitUnit
    Threading.Thread.Sleep 50
    page.Mouse.DownAsync(MouseDownOptions(Button = MouseButton.Left)) |> awaitUnit
    page.Mouse.MoveAsync(movedTinyLeftX - 48.0f, tinyY, MouseMoveOptions(Steps = 8)) |> awaitUnit
    waitForText viewportRange "Preview"
    page.Mouse.UpAsync(MouseUpOptions(Button = MouseButton.Left)) |> awaitUnit
    waitForAttributeChange chartStack "data-visible-start" (string startBeforeLeftResize) |> ignore
    require (requiredIntAttribute chartStack "data-visible-end" - requiredIntAttribute chartStack "data-visible-start" + 1 > 12) "tiny-selection left zone must resize instead of moving"

    page.Locator("[data-testid='ta-demo-loaded-coverage']").ClickAsync() |> awaitUnit
    waitForIntAttribute chartStack "data-loaded-bars" 500
    waitForIntAttribute chartStack "data-visible-start" 251
    waitForIntAttribute chartStack "data-visible-end" 500
    printfn
        "browser.loaded-coverage activeReference=%s localStart=%s localCount=%s detailStart=%s detailCount=%s globalStart=%s globalEnd=%s"
        (attributeOrEmpty chartStack "data-active-reference-bars")
        (attributeOrEmpty chartStack "data-local-visible-start")
        (attributeOrEmpty chartStack "data-local-visible-count")
        (attributeOrEmpty chartStack "data-active-detail-start")
        (attributeOrEmpty chartStack "data-active-detail-count")
        (attributeOrEmpty chartStack "data-visible-start")
        (attributeOrEmpty chartStack "data-visible-end")
    require (attributeOrEmpty chartStack "data-coverage-identity" = "browser-demo:loaded-coverage") "loaded-coverage fixture must expose its stable identity"
    require (requiredIntAttribute chartStack "data-coverage-revision" = 1) "loaded-coverage fixture must begin at revision 1"
    require (requiredIntAttribute chartStack "data-query-generation" = 1) "loaded-coverage fixture must begin at generation 1"
    require (requiredIntAttribute (page.Locator("[data-testid='ta-candle-price']")) "data-point-count" = 250) "active detail must remain bounded to the document cap"
    let coverageSelection = page.Locator("[data-testid='ta-overview-selection']")
    require (abs (requiredFloatAttribute coverageSelection "width" - 500.0) < 0.002) "250 of 500 loaded bars must occupy exactly half of the navigator"

    let callbackCountBeforeCoverageRefresh = requiredIntAttribute callbackState "data-callback-count"
    page.Locator("[data-testid='ta-demo-refresh-coverage-next-visible-range']").ClickAsync() |> awaitUnit
    page.Locator("[data-testid='ta-view-48']").ClickAsync() |> awaitUnit
    waitForIntAttribute chartStack "data-visible-start" 453
    waitForIntAttribute chartStack "data-visible-end" 500
    waitForIntAttribute callbackState "data-callback-count" (callbackCountBeforeCoverageRefresh + 1)
    waitForIntAttribute chartStack "data-coverage-revision" 2
    Threading.Thread.Sleep 500
    require (requiredIntAttribute chartStack "data-visible-start" = 453) "same-identity coverage revision must not overwrite the accepted 48-bar viewport"
    require (requiredIntAttribute chartStack "data-visible-end" = 500) "same-identity coverage revision must preserve the accepted viewport end"
    waitForEnabled (page.Locator("[data-testid='ta-view-all']")) "All preset after same-identity coverage refresh"
    page.Locator("[data-testid='ta-view-all']").ClickAsync() |> awaitUnit
    waitForIntAttribute chartStack "data-visible-start" 251
    waitForIntAttribute chartStack "data-visible-end" 500
    waitForIntAttribute callbackState "data-callback-count" (callbackCountBeforeCoverageRefresh + 2)
    waitForText (page.Locator("[data-testid='ta-poll-state']")) "READY"

    page.Locator("[data-testid='ta-pan-left']").ClickAsync() |> awaitUnit
    waitForIntAttribute chartStack "data-query-generation" 2
    waitForIntAttribute chartStack "data-visible-start" 1
    waitForIntAttribute chartStack "data-visible-end" 250
    require (requiredIntAttribute chartStack "data-loaded-bars" = 500) "adjacent page switch must preserve full loaded coverage"
    require (requiredIntAttribute chartStack "data-coverage-revision" = 3) "accepted adjacent page must atomically advance coverage revision"
    require (requiredIntAttribute (page.Locator("[data-testid='ta-candle-price']")) "data-point-count" = 250) "adjacent page switch must not widen active detail"

    page.ReloadAsync(PageReloadOptions(WaitUntil = WaitUntilState.NetworkIdle)) |> awaitTask |> ignore
    page.Locator("[data-testid='ta-workspace']").WaitForAsync(LocatorWaitForOptions(Timeout = 15000.0f)) |> awaitUnit
    page.Locator("[data-testid='ta-demo-loaded-coverage']").ClickAsync() |> awaitUnit
    let reloadedChartStack = page.Locator("[data-testid='ta-chart-stack']")
    waitForIntAttribute reloadedChartStack "data-visible-start" 251
    waitForIntAttribute reloadedChartStack "data-visible-end" 500
    let reloadedNavigator = page.Locator("[data-testid='ta-overview-navigator']")
    let reloadedNavigatorBox = reloadedNavigator.BoundingBoxAsync() |> awaitTask
    require (not (isNull reloadedNavigatorBox)) "loaded-coverage navigator geometry must be measurable"
    let reloadedY = reloadedNavigatorBox.Y + reloadedNavigatorBox.Height / 2.0f
    let dragFromX = reloadedNavigatorBox.X + reloadedNavigatorBox.Width * 0.75f
    let dragToX = reloadedNavigatorBox.X + reloadedNavigatorBox.Width * 0.25f
    page.Mouse.MoveAsync(dragFromX, reloadedY) |> awaitUnit
    Threading.Thread.Sleep 50
    page.Mouse.DownAsync(MouseDownOptions(Button = MouseButton.Left)) |> awaitUnit
    page.Mouse.MoveAsync(dragToX, reloadedY, MouseMoveOptions(Steps = 8)) |> awaitUnit
    waitForText (page.Locator("[data-testid='ta-viewport-range']")) "Preview"
    page.Mouse.UpAsync(MouseUpOptions(Button = MouseButton.Left)) |> awaitUnit
    waitForIntAttribute reloadedChartStack "data-query-generation" 2
    waitForIntAttribute reloadedChartStack "data-visible-start" 1
    waitForIntAttribute reloadedChartStack "data-visible-end" 250
    require (requiredIntAttribute reloadedChartStack "data-loaded-bars" = 500) "drag and toolbar must resolve against the same loaded coverage"

    page.ReloadAsync(PageReloadOptions(WaitUntil = WaitUntilState.NetworkIdle)) |> awaitTask |> ignore
    page.Locator("[data-testid='ta-workspace']").WaitForAsync(LocatorWaitForOptions(Timeout = 15000.0f)) |> awaitUnit
    page.Locator("[data-testid='ta-demo-loaded-coverage']").ClickAsync() |> awaitUnit
    let queuedDragChartStack = page.Locator("[data-testid='ta-chart-stack']")
    waitForIntAttribute queuedDragChartStack "data-visible-start" 251
    waitForIntAttribute queuedDragChartStack "data-visible-end" 500
    let queuedDragCallbackState = page.Locator("[data-testid='ta-demo-callback-state']")
    let callbackCountBeforeQueuedDrag = requiredIntAttribute queuedDragCallbackState "data-callback-count"
    page.Locator("[data-testid='ta-view-48']").ClickAsync() |> awaitUnit
    waitForIntAttribute queuedDragChartStack "data-visible-start" 453
    waitForIntAttribute queuedDragChartStack "data-visible-end" 500
    let queuedDragNavigator = page.Locator("[data-testid='ta-overview-navigator']")
    let queuedDragNavigatorBox = queuedDragNavigator.BoundingBoxAsync() |> awaitTask
    require (not (isNull queuedDragNavigatorBox)) "pending-action navigator geometry must be measurable"
    let queuedDragY = queuedDragNavigatorBox.Y + queuedDragNavigatorBox.Height / 2.0f
    let queuedDragFromX = queuedDragNavigatorBox.X + queuedDragNavigatorBox.Width * 0.95f
    let queuedDragToX = queuedDragNavigatorBox.X
    page.Mouse.MoveAsync(queuedDragFromX, queuedDragY) |> awaitUnit
    page.Mouse.DownAsync(MouseDownOptions(Button = MouseButton.Left)) |> awaitUnit
    page.Mouse.MoveAsync(queuedDragToX, queuedDragY, MouseMoveOptions(Steps = 8)) |> awaitUnit
    waitForText (page.Locator("[data-testid='ta-viewport-range']")) "Preview"
    page.Mouse.UpAsync(MouseUpOptions(Button = MouseButton.Left)) |> awaitUnit
    waitForText (page.Locator("[data-testid='ta-feedback']")) "Earlier coverage queued."
    waitForIntAttribute queuedDragCallbackState "data-callback-count" (callbackCountBeforeQueuedDrag + 2)
    waitForIntAttribute queuedDragChartStack "data-query-generation" 2
    waitForIntAttribute queuedDragChartStack "data-visible-start" 405
    waitForIntAttribute queuedDragChartStack "data-visible-end" 452
    require (attributeOrEmpty queuedDragCallbackState "data-last-action" = "VisibleRangeChanged") "queued boundary drag must dispatch after the pending visible-range action settles"

    require (consoleErrors.Count = 0) ("desktop console errors: " + String.concat " | " consoleErrors)
    let overBudgetPhases = longTaskPhases |> Seq.filter (fun (_, values, _) -> values.Length > 0) |> Seq.toArray
    if not skipPerformanceGates then
        require
            (overBudgetPhases.Length = 0)
            (overBudgetPhases
             |> Array.map (fun (label, values, maximum) -> $"{label}: count={values.Length}, max={maximum:F2}ms")
             |> String.concat "; "
             |> fun details -> "renderer workload retained >100ms long tasks: " + details)
    else
        printfn "browser.performance-gates skipped=true overBudgetPhases=%d" overBudgetPhases.Length

    let reloadResize = page.Locator("[data-testid='ta-row-resize-price']")
    reloadResize.FocusAsync() |> awaitUnit
    reloadResize.PressAsync("Shift+ArrowUp") |> awaitUnit
    waitForAttributeValue reloadResize "aria-valuenow" "218"
    page.ReloadAsync(PageReloadOptions(WaitUntil = WaitUntilState.NetworkIdle)) |> awaitTask |> ignore
    page.Locator("[data-testid='ta-workspace']").WaitForAsync(LocatorWaitForOptions(Timeout = 15000.0f)) |> awaitUnit
    require (requiredIntAttribute (page.Locator("[data-testid='ta-row-resize-price']")) "aria-valuenow" = 250) "browser reload must discard renderer-local row-height overrides and restore the capped authored default"
    require (consoleErrors.Count = 0) ("desktop console errors after reload: " + String.concat " | " consoleErrors)

    Directory.CreateDirectory outputDirectory |> ignore
    page.ScreenshotAsync(PageScreenshotOptions(Path = Path.Combine(outputDirectory, "desktop.png"), FullPage = true)) |> awaitTask |> ignore
    longTaskSession.DetachAsync() |> awaitUnit
    context.CloseAsync() |> awaitUnit
    titleBox, priceBox, cursorLatency.ElapsedMilliseconds, cursorTransitions, maximumCursorLatencyMs, sustainedCursor.ElapsedMilliseconds, concurrentPreviewUpdateCount

let verifyMobile (browser: IBrowser) =
    let viewportWidth = 390
    let context = browser.NewContextAsync(BrowserNewContextOptions(ViewportSize = ViewportSize(Width = viewportWidth, Height = 844), IsMobile = true)) |> awaitTask
    let page = context.NewPageAsync() |> awaitTask
    let styleSession = context.NewCDPSessionAsync(page) |> awaitTask
    styleSession.SendAsync("DOM.enable") |> awaitTask |> ignore
    styleSession.SendAsync("CSS.enable") |> awaitTask |> ignore
    let consoleErrors = ResizeArray<string>()
    page.Console.Add(fun (message: IConsoleMessage) -> if message.Type = "error" then consoleErrors.Add message.Text; printfn "mobile console error: %s" message.Text)
    page.PageError.Add(fun (error: string) -> consoleErrors.Add error; printfn "mobile page error: %s" error)

    page.GotoAsync(url, PageGotoOptions(WaitUntil = WaitUntilState.NetworkIdle)) |> awaitTask |> ignore
    page.Locator("[data-testid='ta-workspace']").WaitForAsync(LocatorWaitForOptions(Timeout = 15000.0f)) |> awaitUnit
    requireBoxInside viewportWidth "workspace" (page.Locator("[data-testid='ta-workspace']").BoundingBoxAsync() |> awaitTask)
    requireBoxInside viewportWidth "query toolbar" (page.Locator("[data-testid='ta-query-toolbar']").BoundingBoxAsync() |> awaitTask)
    requireBoxInside viewportWidth "cursor panel" (page.Locator("[data-testid='ta-cursor-panel']").BoundingBoxAsync() |> awaitTask)
    requireBoxInside viewportWidth "viewport navigator" (page.Locator("[data-testid='ta-viewport-panel']").BoundingBoxAsync() |> awaitTask)
    requireBoxInside viewportWidth "price chart" (page.Locator("[data-testid='ta-candle-price']").BoundingBoxAsync() |> awaitTask)
    require ((page.Locator("[data-testid='ta-query-toolbar'] input").CountAsync() |> awaitTask) = 3) "mobile query form must retain all text inputs"
    require (page.Locator("[data-testid='ta-apply-query']").IsVisibleAsync() |> awaitTask) "mobile Load / Apply must remain visible"
    require (page.Locator("[data-testid='ta-add-row-toggle']").IsVisibleAsync() |> awaitTask) "mobile Add Row must remain visible"
    require (page.Locator("[data-testid='ta-edit-row-sma']").IsVisibleAsync() |> awaitTask) "mobile bound row Edit must remain visible"
    verifyRowControlLines "mobile" viewportWidth styleSession page

    page.Locator("[data-testid='ta-add-row-toggle']").ClickAsync() |> awaitUnit
    requireBoxInside viewportWidth "mobile Add Row editor" (page.Locator("[data-testid='ta-add-row-editor']").BoundingBoxAsync() |> awaitTask)
    require (consoleErrors.Count = 0) ("mobile console errors: " + String.concat " | " consoleErrors)

    Directory.CreateDirectory outputDirectory |> ignore
    page.ScreenshotAsync(PageScreenshotOptions(Path = Path.Combine(outputDirectory, "mobile.png"), FullPage = true)) |> awaitTask |> ignore
    styleSession.DetachAsync() |> awaitUnit
    context.CloseAsync() |> awaitUnit

let playwright = Playwright.CreateAsync() |> awaitTask
let launch = BrowserTypeLaunchOptions(Headless = not headed)

if not (String.IsNullOrWhiteSpace browserExecutablePath) && File.Exists browserExecutablePath then
    launch.ExecutablePath <- browserExecutablePath
    launch.Args <- [| "--no-sandbox"; "--disable-dev-shm-usage" |]

let browser = playwright.Chromium.LaunchAsync(launch) |> awaitTask

try
    let titleBox, priceBox, cursorLatencyMs, cursorTransitions, maximumCursorLatencyMs, sustainedCursorMs, concurrentPreviewUpdates = verifyDesktop browser
    verifyMobile browser
    printfn "TA renderer Playwright PASS url=%s cursorLatencyMs=%d sustainedTransitions=%d sustainedMaxLatencyMs=%d sustainedElapsedMs=%d concurrentPreviewUpdates=%d chartRerender=false desktopTitle=(%.1f,%.1f,%.1f,%.1f) desktopPrice=(%.1f,%.1f,%.1f,%.1f) output=%s" url cursorLatencyMs cursorTransitions maximumCursorLatencyMs sustainedCursorMs concurrentPreviewUpdates titleBox.X titleBox.Y titleBox.Width titleBox.Height priceBox.X priceBox.Y priceBox.Width priceBox.Height outputDirectory
finally
    browser.CloseAsync() |> awaitUnit
    playwright.Dispose()
