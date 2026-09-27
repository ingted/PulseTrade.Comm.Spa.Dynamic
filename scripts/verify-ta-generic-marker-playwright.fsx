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
let percentile95 values =
    let ordered = values |> Array.sort
    if ordered.Length = 0 then 0.0
    else ordered[min (ordered.Length - 1) (int (Math.Ceiling(float ordered.Length * 0.95)) - 1)]

let demoTimestamp index =
    let day = 1 + index / 1440
    let hour = (index / 60) % 24
    let minute = index % 60
    sprintf "2026-09-%02dT%02d:%02d:00.0000000+00:00" day hour minute

let verify viewportWidth viewportHeight screenshotName runCursorGate (browser: IBrowser) =
    let context = browser.NewContextAsync(BrowserNewContextOptions(ViewportSize = ViewportSize(Width = viewportWidth, Height = viewportHeight))) |> awaitTask
    let page = context.NewPageAsync() |> awaitTask
    let errors = ResizeArray<string>()
    page.Console.Add(fun message -> if message.Type = "error" then errors.Add message.Text)
    page.PageError.Add(errors.Add)
    page.GotoAsync(url, PageGotoOptions(WaitUntil = WaitUntilState.NetworkIdle)) |> awaitTask |> ignore
    page.Locator("[data-testid='ta-workspace']").WaitForAsync(LocatorWaitForOptions(Timeout = 15000.0f)) |> awaitUnit

    let layer = page.Locator("[data-testid='ta-marker-layer-price']")
    let longEntry = page.Locator("[data-testid='ta-marker-signals-long-entry']")
    let shortEntry = page.Locator("[data-testid='ta-marker-signals-short-entry']")
    let longExit = page.Locator("[data-testid='ta-marker-signals-long-exit']")
    let shortExit = page.Locator("[data-testid='ta-marker-signals-short-exit']")
    layer.WaitForAsync(LocatorWaitForOptions(Timeout = 30000.0f)) |> awaitUnit
    require (layer.CountAsync() |> awaitTask = 1) "price row must mount one marker layer"
    let acceptedMarkerCount = intAttribute layer "data-marker-count"
    let directMarkerCount = intAttribute layer "data-direct-marker-count"
    require (acceptedMarkerCount = 66) $"all named and dense-fixture markers must remain accepted; actual={acceptedMarkerCount}"
    require (directMarkerCount = 6) $"the per-slot glyph budget must render six direct markers across three occupied slots; actual={directMarkerCount}"
    require (longEntry.CountAsync() |> awaitTask = 1 && shortEntry.CountAsync() |> awaitTask = 1 && longExit.CountAsync() |> awaitTask = 1 && shortExit.CountAsync() |> awaitTask = 1) "the four authored trade markers must remain visible"
    require (page.Locator("[data-testid='ta-row-value-price-signals']").CountAsync() |> awaitTask = 0) "marker event overlays must not create a numeric legend token"
    require (page.Locator("[data-testid='ta-row-value-price-overview-signal']").CountAsync() |> awaitTask = 0) "overview signal event overlays must not create a numeric legend token"
    require (page.Locator("[data-testid='ta-row-value-price-overview-fill']").CountAsync() |> awaitTask = 0) "overview fill event overlays must not create a numeric legend token"
    require (not ((page.Locator("[data-testid='ta-row-values-price']").InnerTextAsync() |> awaitTask).Contains("Signals Unavailable", StringComparison.Ordinal))) "marker event overlays leaked an undefined numeric value"
    require (page.Locator("[data-testid^='ta-marker-label-']").CountAsync() |> awaitTask = 0) "marker text must remain in the OFI event band, not inside the K-bar plot"
    require (attribute longEntry "data-marker-anchor" = "below-bar" && attribute longEntry "data-marker-shape" = "triangle-up") "long entry mapping changed"
    require (attribute longEntry "data-marker-fill" = "outline" && attribute longEntry "fill" = "none") "long entry must render as a true hollow triangle"
    require (attribute longEntry "stroke" = "#000000" && attribute longEntry "pointer-events" = "all") "hollow marker stroke or hit target changed"
    let longEntryHalo = page.Locator("[data-marker-halo-for='long-entry']")
    require (longEntryHalo.CountAsync() |> awaitTask = 1) "hollow marker is missing its contrast halo"
    require (attribute longEntryHalo "stroke" <> attribute longEntry "stroke") "contrast halo must differ from the authored semantic stroke"
    require (attribute longEntryHalo "stroke-width" = "4.4" && attribute longEntryHalo "pointer-events" = "none") "contrast halo geometry or hit-testing changed"
    require (attribute shortEntry "data-marker-anchor" = "above-bar" && attribute shortEntry "data-marker-shape" = "triangle-down") "short entry mapping changed"
    require (attribute longExit "data-marker-shape" = "triangle-down" && attribute longExit "data-marker-fill" = "solid") "long exit mapping changed"
    require (attribute shortExit "data-marker-anchor" = "below-bar" && attribute shortExit "data-marker-shape" = "triangle-up") "short exit mapping changed"
    let shortEntryPosition = attribute shortEntry "data-marker-position"
    require (shortEntryPosition = "3992") $"Position must remain the spatial authority; actual={shortEntryPosition}"
    require (intAttribute shortEntry "data-marker-lane" = 0 && intAttribute longExit "data-marker-lane" = 1) "same-position markers must stack deterministically"
    let tooltip = textOf (shortEntry.Locator("title"))
    require (tooltip.Contains "SELL 7591.00" && tooltip.Contains "Reason: short entry signal" && tooltip.Contains "Source: BrowserDemo") "tooltip order/content changed"

    let rowBox = page.Locator("[data-testid='ta-row-price']").BoundingBoxAsync() |> awaitTask
    for label, marker in [ "long-entry", longEntry; "short-entry", shortEntry; "long-exit", longExit; "short-exit", shortExit ] do
        let markerBox = marker.BoundingBoxAsync() |> awaitTask
        require (not (isNull rowBox) && not (isNull markerBox)) (label + " geometry is missing")
        require (markerBox.Y >= rowBox.Y - 0.5f && markerBox.Y + markerBox.Height <= rowBox.Y + rowBox.Height + 0.5f) (label + " escaped its row")
    if runCursorGate then
        let chartStack = page.Locator("[data-testid='ta-chart-stack']")
        let loadedBars = intAttribute chartStack "data-loaded-bars"
        require (loadedBars = 4000) $"capacity fixture loaded {loadedBars} bars instead of 4000"
        let batchedCandlePaths = page.Locator("path[data-candle-batched='true']").CountAsync() |> awaitTask
        require (batchedCandlePaths = 48) $"six candle traces must remain bounded to eight batched paths each; actual={batchedCandlePaths}"

        let pollState = page.Locator("[data-testid='ta-poll-state']")
        let readyDeadline = DateTime.UtcNow.AddSeconds 5.0
        while attribute pollState "data-poll-state" <> "READY" && DateTime.UtcNow < readyDeadline do
            Threading.Thread.Sleep 5
        require (attribute pollState "data-poll-state" = "READY") "viewport gate did not reach READY before user interaction"

        let allWatch = Diagnostics.Stopwatch.StartNew()
        page.Locator("[data-testid='ta-view-all']").ClickAsync() |> awaitUnit
        let allDeadline = DateTime.UtcNow.AddSeconds 5.0
        while (attribute chartStack "data-visible-start" <> "1" || attribute chartStack "data-visible-end" <> string loadedBars)
              && DateTime.UtcNow < allDeadline do
            Threading.Thread.Sleep 5
        allWatch.Stop()
        let allVisibleStart = attribute chartStack "data-visible-start"
        let allVisibleEnd = attribute chartStack "data-visible-end"
        let pollStateText = attribute pollState "data-poll-state"
        require (allVisibleStart = "1" && allVisibleEnd = string loadedBars)
            ($"All viewport did not expose the complete loaded range: start={allVisibleStart} end={allVisibleEnd} loaded={loadedBars} poll={pollStateText}")
        let allRowsDeadline = DateTime.UtcNow.AddSeconds 5.0
        while attribute chartStack "data-ready-row-count" <> attribute chartStack "data-row-count"
              && DateTime.UtcNow < allRowsDeadline do
            Threading.Thread.Sleep 5
        require (attribute chartStack "data-ready-row-count" = attribute chartStack "data-row-count")
            "All viewport shell completed before its row mount generation"
        require (allWatch.ElapsedMilliseconds <= 2000L) $"48-to-All took {allWatch.ElapsedMilliseconds}ms"

        let smaLegendText = textOf (page.Locator("[title='1K SMA value']"))
        match Double.TryParse(smaLegendText, Globalization.NumberStyles.Float, Globalization.CultureInfo.InvariantCulture) with
        | true, value -> require (value > 0.0) $"SMA legend reused another row reader: {value}"
        | _ -> failwith $"Generic marker Playwright verification failed: SMA legend is not numeric: {smaLegendText}"

        let view48 = page.Locator("[data-testid='ta-view-48']")
        Threading.Thread.Sleep 1000
        view48.ClickAsync() |> awaitUnit
        Threading.Thread.Sleep 250
        if attribute chartStack "data-visible-start" = "1" then
            view48.ClickAsync() |> awaitUnit
        let latestDeadline = DateTime.UtcNow.AddSeconds 5.0
        let expectedLatestStart = string (loadedBars - 47)
        while (attribute chartStack "data-visible-start" <> expectedLatestStart || attribute chartStack "data-visible-end" <> string loadedBars)
              && DateTime.UtcNow < latestDeadline do
            Threading.Thread.Sleep 5
        let latestStart = attribute chartStack "data-visible-start"
        let latestEnd = attribute chartStack "data-visible-end"
        require (latestStart = expectedLatestStart && latestEnd = string loadedBars)
            ($"48 viewport did not return to the loaded tail: start={latestStart} end={latestEnd}")

        let chart = page.Locator("[data-testid='ta-candle-price']")
        let crosshair = page.Locator("[data-testid='ta-candle-price-crosshair']")
        let chartBox = chart.BoundingBoxAsync() |> awaitTask
        require (not (isNull chartBox)) "price chart geometry is missing"
        page.Mouse.MoveAsync(chartBox.X + chartBox.Width * 0.8f, chartBox.Y + chartBox.Height / 2.0f) |> awaitUnit
        let rowCursorDate = page.Locator("[data-testid='ta-row-cursor-date-price']")
        let rowCursorClock = page.Locator("[data-testid='ta-row-cursor-time-price']")
        let rowDataTime = page.Locator("[data-testid='ta-row-data-time-price']")
        let sharedAxisDataTime = page.Locator("[data-testid='ta-row-data-time-sma']")
        let rowDataWindow = page.Locator("[data-testid='ta-row-values-price']")
        require (attribute rowDataWindow "data-auto-height" = "true") "row data window must use auto height"
        require (not ((attribute rowDataWindow "style").Contains("overflow-x:auto", StringComparison.Ordinal))) "row data window must not use an internal horizontal scrollbar"
        require ((attribute rowDataWindow "style").Contains("flex-wrap:wrap", StringComparison.Ordinal)) "row data window must wrap value tokens"
        require ((textOf rowCursorDate).StartsWith("2026-09-", StringComparison.Ordinal)) "row cursor date is missing"
        require ((textOf rowCursorClock).Length = 8) "row cursor clock must use HH:mm:ss"
        require ((textOf rowDataTime).Length = 19) "row data window timestamp must use yyyy-MM-dd HH:mm:ss"
        let initialSharedAxisTime = textOf sharedAxisDataTime
        require (initialSharedAxisTime = "2026-09-03 18:32:00")
            ($"shared-axis cursor must display authored EventTimeUtc instead of interval start; actual={initialSharedAxisTime}")
        let priceWindowText = textOf rowDataWindow
        for token in [ "O "; " H "; " L "; " C "; " V " ] do
            require (priceWindowText.Contains(token, StringComparison.Ordinal)) ("candlestick data window is missing " + token.Trim())
        let mutable prior = attribute crosshair "x1"
        let mutable maximumMs = 0L
        let transitionDurations = ResizeArray<float>()
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
            transitionDurations.Add transition.Elapsed.TotalMilliseconds
            prior <- current
        total.Stop()
        let pointerP95 = transitionDurations.ToArray() |> percentile95
        require (pointerP95 < 50.0) $"pointer transition p95 was {pointerP95:F2}ms"
        require (total.Elapsed < TimeSpan.FromSeconds 8.0) $"200 cursor transitions took {total.Elapsed}"

        printfn "renderer gate bars=%d paths=%d allMs=%d pointerP95Ms=%.2f maxMs=%d" loadedBars batchedCandlePaths allWatch.ElapsedMilliseconds pointerP95 maximumMs

        let cursorEvents = page.Locator("[data-testid='ta-row-ofi-band-price']")
        let visibleStartZero = int (attribute chartStack "data-visible-start") - 1
        let overviewAbsoluteSlot = loadedBars - 8
        let overviewLocalSlot = overviewAbsoluteSlot - visibleStartZero
        page.Mouse.MoveAsync(chartBox.X + chartBox.Width * ((float32 overviewLocalSlot + 0.5f) / 48.0f), chartBox.Y + chartBox.Height / 2.0f) |> awaitUnit
        let overviewEventDeadline = DateTime.UtcNow.AddSeconds 2.0
        let overviewEvents = cursorEvents.Locator("[data-cursor-event-source-kind='overview-stripe']")
        while ((overviewEvents.CountAsync() |> awaitTask) < 2 || attribute chartStack "data-cursor-index" <> string overviewLocalSlot)
              && DateTime.UtcNow < overviewEventDeadline do
            Threading.Thread.Sleep 5
        let overviewEventCount = overviewEvents.CountAsync() |> awaitTask
        let overviewCursorIndex = attribute chartStack "data-cursor-index"
        let overviewPathCount = page.Locator("[data-testid='ta-overview-stripe-path']").CountAsync() |> awaitTask
        let probe = ResizeArray<string>()
        if overviewEventCount <> 2 then
            for slot in max 0 (overviewLocalSlot - 2) .. min 47 (overviewLocalSlot + 2) do
                page.Mouse.MoveAsync(chartBox.X + chartBox.Width * ((float32 slot + 0.5f) / 48.0f), chartBox.Y + chartBox.Height / 2.0f) |> awaitUnit
                Threading.Thread.Sleep 80
                let currentCount = overviewEvents.CountAsync() |> awaitTask
                let currentCursor = attribute chartStack "data-cursor-index"
                probe.Add($"{slot}:{currentCount}:{currentCursor}")
        let probeText = String.concat "," probe
        require (overviewEventCount = 2)
            ($"OverviewStripe signal/fill events were not merged into the cursor event band: count={overviewEventCount} cursor={overviewCursorIndex} expectedCursor={overviewLocalSlot} navigatorPaths={overviewPathCount} probe={probeText}")
        let overviewCategories =
            [| for index in 0 .. 1 -> attribute (overviewEvents.Nth(index)) "data-cursor-event-category" |]
        require (overviewCategories |> Array.contains "Signal stripe" && overviewCategories |> Array.contains "Fill stripe") "OverviewStripe cursor events lost their authored categories"

        let beforeHollowHover = attribute crosshair "x1"
        longEntry.HoverAsync() |> awaitUnit
        let deadline = DateTime.UtcNow.AddSeconds 2.0
        let mutable afterHollowHover = attribute crosshair "x1"
        while afterHollowHover = beforeHollowHover && DateTime.UtcNow < deadline do
            Threading.Thread.Sleep 5
            afterHollowHover <- attribute crosshair "x1"
        require (afterHollowHover <> beforeHollowHover) "hollow marker hit target blocked the shared cursor"
        require ((textOf (longEntry.Locator("title"))).Contains "long entry signal") "hollow marker tooltip disappeared during cursor interaction"
        let emptyCursorEvent = cursorEvents.Locator("[data-ta-row-ofi-empty='true']")
        require (intAttribute cursorEvents "data-marker-event-count" > 0) "marker hover did not publish cursor events"
        require (not (emptyCursorEvent.IsVisibleAsync() |> awaitTask)) "None placeholder remained visible while cursor events existed"

        page.Mouse.MoveAsync(chartBox.X + chartBox.Width * 0.1f, chartBox.Y + chartBox.Height / 2.0f) |> awaitUnit
        let emptyEventDeadline = DateTime.UtcNow.AddSeconds 2.0
        while intAttribute cursorEvents "data-marker-event-count" <> 0 && DateTime.UtcNow < emptyEventDeadline do
            Threading.Thread.Sleep 5
        require (intAttribute cursorEvents "data-marker-event-count" = 0) "empty cursor slot retained stale marker events"
        require (emptyCursorEvent.IsVisibleAsync() |> awaitTask && textOf emptyCursorEvent = "None") "available cursor event capability must show None for an empty slot"

        let priceCandleToggle = page.Locator("[data-testid='ta-toggle-trace-price-price-1k']")
        let priceCandleTrace = page.Locator("[data-testid='ta-candle-price-price-1k']")
        let siblingPriceCandleTrace = page.Locator("[data-testid='ta-candle-price-price-5k']")
        require (priceCandleToggle.CountAsync() |> awaitTask = 1) "the high-density composite row is missing its candlestick toggle"
        require (priceCandleTrace.CountAsync() |> awaitTask = 8) "the high-density composite row did not start with all eight batched candlestick paths"
        require (siblingPriceCandleTrace.CountAsync() |> awaitTask = 8) "the high-density composite row is missing its sibling candlestick trace"
        priceCandleToggle.ClickAsync() |> awaitUnit
        page.Locator("[data-testid='ta-toggle-trace-price-price-1k'][aria-pressed='false']").WaitForAsync(LocatorWaitForOptions(Timeout = 5000.0f)) |> awaitUnit
        require (priceCandleTrace.CountAsync() |> awaitTask = 0 && siblingPriceCandleTrace.CountAsync() |> awaitTask = 8) "hiding one composite candlestick changed its sibling trace"
        page.Locator("[data-testid='ta-toggle-trace-price-price-1k']").ClickAsync() |> awaitUnit
        page.Locator("[data-testid='ta-toggle-trace-price-price-1k'][aria-pressed='true']").WaitForAsync(LocatorWaitForOptions(Timeout = 5000.0f)) |> awaitUnit
        let restoredReadyDeadline = DateTime.UtcNow.AddSeconds 5.0
        while attribute chartStack "data-ready-row-count" <> attribute chartStack "data-row-count"
              && DateTime.UtcNow < restoredReadyDeadline do
            Threading.Thread.Sleep 10
        require (attribute chartStack "data-ready-row-count" = attribute chartStack "data-row-count") "the high-density composite chart did not become ready after showing its candlestick"
        priceCandleTrace.First.WaitForAsync(LocatorWaitForOptions(Timeout = 5000.0f)) |> awaitUnit
        require (priceCandleTrace.CountAsync() |> awaitTask = 8 && siblingPriceCandleTrace.CountAsync() |> awaitTask = 8) "showing the high-density composite candlestick did not restore all candle paths"

        let sma1Toggle = page.Locator("[data-testid='ta-toggle-trace-sma-sma-1k']")
        let sma1Trace = page.Locator("[data-testid='ta-trace-sma-sma-1k']")
        let sma5Trace = page.Locator("[data-testid='ta-trace-sma-sma-5k']")
        require (sma1Toggle.CountAsync() |> awaitTask = 1 && sma1Trace.CountAsync() |> awaitTask > 0 && sma5Trace.CountAsync() |> awaitTask > 0) "per-trace controls or initial SMA traces are missing"
        sma1Toggle.ClickAsync() |> awaitUnit
        page.Locator("[data-testid='ta-toggle-trace-sma-sma-1k'][aria-pressed='false']").WaitForAsync(LocatorWaitForOptions(Timeout = 5000.0f)) |> awaitUnit
        require (sma1Trace.CountAsync() |> awaitTask = 0 && sma5Trace.CountAsync() |> awaitTask > 0) "hiding one trace changed the wrong row/trace set"
        page.Locator("[data-testid='ta-toggle-trace-sma-sma-1k']").ClickAsync() |> awaitUnit
        page.Locator("[data-testid='ta-trace-sma-sma-1k']").WaitForAsync(LocatorWaitForOptions(Timeout = 5000.0f)) |> awaitUnit
        let removeSma1 = page.Locator("[data-testid='ta-remove-trace-sma-sma-1k']")
        require (removeSma1.CountAsync() |> awaitTask = 1) "allowed remove-trace action did not expose the trace remove control"
        removeSma1.ClickAsync() |> awaitUnit
        page.Locator("[data-testid='ta-toggle-trace-sma-sma-1k']").WaitForAsync(LocatorWaitForOptions(State = WaitForSelectorState.Detached, Timeout = 5000.0f)) |> awaitUnit
        require (sma1Trace.CountAsync() |> awaitTask = 0 && sma5Trace.CountAsync() |> awaitTask > 0) "accepted trace removal removed the wrong trace or entire row"
        let removeSma5 = page.Locator("[data-testid='ta-remove-trace-sma-sma-5k']")
        require (removeSma5.CountAsync() |> awaitTask = 1) "the remaining SMA trace is missing its remove control"
        removeSma5.ClickAsync() |> awaitUnit
        page.Locator("[data-testid='ta-row-sma']").WaitForAsync(LocatorWaitForOptions(State = WaitForSelectorState.Detached, Timeout = 5000.0f)) |> awaitUnit
        require (page.Locator("[data-testid='ta-row-sma']").CountAsync() |> awaitTask = 0) "a row without unremoved non-system traces remained visible"
        page.Locator("[data-testid='ta-reset-canvas']").ClickAsync() |> awaitUnit
        page.Locator("[data-testid='ta-trace-sma-sma-1k']").WaitForAsync(LocatorWaitForOptions(Timeout = 5000.0f)) |> awaitUnit
        require (
            page.Locator("[data-testid='ta-toggle-trace-sma-sma-1k']").CountAsync() |> awaitTask = 1
            && page.Locator("[data-testid='ta-toggle-trace-sma-sma-5k']").CountAsync() |> awaitTask = 1
            && page.Locator("[data-testid='ta-row-sma']").CountAsync() |> awaitTask = 1)
            "Reset Canvas did not restore the removed traces and their row"

        let identityBeforeQuery = textOf (page.Locator("[data-testid='ta-canvas-identity']"))
        let fromInput = page.Locator("[data-testid='ta-from']")
        let toInput = page.Locator("[data-testid='ta-to']")
        fromInput.FillAsync(demoTimestamp 1000) |> awaitUnit
        toInput.FillAsync(demoTimestamp 2000) |> awaitUnit
        page.Locator("[data-testid='ta-apply-query']").ClickAsync() |> awaitUnit
        fromInput.FillAsync(demoTimestamp 2000) |> awaitUnit
        toInput.FillAsync(demoTimestamp 3000) |> awaitUnit
        page.Locator("[data-testid='ta-apply-query']").ClickAsync() |> awaitUnit
        let latestQueryDeadline = DateTime.UtcNow.AddSeconds 5.0
        while (attribute chartStack "data-visible-start" <> "2001" || attribute chartStack "data-visible-end" <> "3000")
              && DateTime.UtcNow < latestQueryDeadline do
            Threading.Thread.Sleep 10
        require (attribute chartStack "data-visible-start" = "2001" && attribute chartStack "data-visible-end" = "3000")
            "latest ChangeTaQuery response did not win the viewport race"
        require (intAttribute chartStack "data-loaded-bars" = loadedBars) "ChangeTaQuery replaced loaded data instead of selecting a local window"
        require (textOf (page.Locator("[data-testid='ta-canvas-identity']")) = identityBeforeQuery)
            "ChangeTaQuery replaced document/canvas identity"

        fromInput.FillAsync(demoTimestamp 0) |> awaitUnit
        toInput.FillAsync(demoTimestamp loadedBars) |> awaitUnit
        page.Locator("[data-testid='ta-apply-query']").ClickAsync() |> awaitUnit
        let fullQueryDeadline = DateTime.UtcNow.AddSeconds 5.0
        while (attribute chartStack "data-visible-start" <> "1" || attribute chartStack "data-visible-end" <> string loadedBars)
              && DateTime.UtcNow < fullQueryDeadline do
            Threading.Thread.Sleep 10
        require (attribute chartStack "data-visible-start" = "1" && attribute chartStack "data-visible-end" = string loadedBars)
            "full ChangeTaQuery range did not restore the complete loaded viewport"

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
