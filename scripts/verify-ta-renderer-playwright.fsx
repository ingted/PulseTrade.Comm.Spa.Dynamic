// Real-browser operation and geometry verifier for the pure WebSharper TA renderer demo.

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
            | Url _ -> "Existing TA renderer browser-demo URL."
            | Output_Dir _ -> "Directory for deterministic desktop/mobile screenshots."
            | Browser_Executable_Path _ -> "Chrome or Edge executable path."
            | Headed -> "Run the browser headed."

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

let awaitTask (task: Task<'T>) = task.GetAwaiter().GetResult()
let awaitUnit (task: Task) = task.GetAwaiter().GetResult()

let require condition message =
    if not condition then failwith ("TA renderer Playwright verification failed: " + message)

let capacityPointCount = 3820
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

let waitForAttributeChange (locator: ILocator) name previous =
    let deadline = DateTime.UtcNow.AddSeconds 8.0
    let mutable actual = locator.GetAttributeAsync(name) |> awaitTask |> Option.ofObj |> Option.defaultValue ""

    while actual = previous && DateTime.UtcNow < deadline do
        Threading.Thread.Sleep 10
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

let verifyDesktop (browser: IBrowser) =
    let context = browser.NewContextAsync(BrowserNewContextOptions(ViewportSize = ViewportSize(Width = 1440, Height = 900))) |> awaitTask
    let page = context.NewPageAsync() |> awaitTask
    let consoleErrors = ResizeArray<string>()
    page.Console.Add(fun (message: IConsoleMessage) -> if message.Type = "error" then consoleErrors.Add message.Text; printfn "desktop console error: %s" message.Text)
    page.PageError.Add(fun (error: string) -> consoleErrors.Add error; printfn "desktop page error: %s" error)

    page.GotoAsync(url, PageGotoOptions(WaitUntil = WaitUntilState.NetworkIdle)) |> awaitTask |> ignore
    page.Locator("[data-testid='ta-workspace']").WaitForAsync(LocatorWaitForOptions(Timeout = 15000.0f)) |> awaitUnit

    requireText (page.Locator("[data-testid='ta-workspace-title']")) "PTMD TA Research"
    requireText (page.Locator("[data-testid='ta-freshness']")) "LIVE"
    require ((page.Locator("[data-testid='ta-chart-stack'] section").CountAsync() |> awaitTask) = 7) "all seven configured TA rows must render"
    requireText (page.Locator("[data-testid='ta-toggle-row-price']")) "ES 1K + SMA(20)"
    requireText (page.Locator("[data-testid='ta-row-price']")) "ES 1K + SMA(20)"
    require (requiredIntAttribute (page.Locator("[data-testid='ta-candle-price']")) "data-point-count" = visiblePointCount) "candlestick chart must retain all committed visible points"
    let projectedCoarseCandles = page.Locator("[data-testid='ta-candle-price-price-5k'][data-candle-part='body']")
    let projectedCoarseCount = projectedCoarseCandles.CountAsync() |> awaitTask
    require (projectedCoarseCount = visiblePointCount) $"5K source candles must project onto each of the {visiblePointCount} actual visible base slots; actual={projectedCoarseCount}"
    let projectedIndexes =
        [| for index in 0 .. projectedCoarseCount - 1 -> requiredIntAttribute (projectedCoarseCandles.Nth(index)) "data-projected-slot-index" |]
    require (projectedIndexes |> Array.distinct |> Array.length = visiblePointCount) "projected high-scale candles must use distinct actual base-axis slots"
    let projectedSourceIds =
        [| for index in 0 .. projectedCoarseCount - 1 -> projectedCoarseCandles.Nth(index).GetAttributeAsync("data-source-interval-id") |> awaitTask |]
    require (projectedSourceIds |> Array.forall (String.IsNullOrWhiteSpace >> not)) "every projected candle must retain its canonical source interval id"
    require (projectedSourceIds |> Array.distinct |> Array.length < projectedCoarseCount) "multiple projected slots must reference the same sparse high-scale source candle"
    requireText (page.Locator("[data-testid='ta-status-detail']")) "watermark 2026-07-11T09:30:00Z"
    requireText (page.Locator("[data-testid='ta-status-detail']")) "quality complete"

    let chartStack = page.Locator("[data-testid='ta-chart-stack']")
    require (chartStack.GetAttributeAsync("data-loaded-bars") |> awaitTask = string capacityPointCount) "loaded-range metadata must report the full capacity fixture"
    require (chartStack.GetAttributeAsync("data-visible-start") |> awaitTask = string initialVisibleStart) "follow-latest viewport must begin at the expected capacity position"
    require (chartStack.GetAttributeAsync("data-visible-end") |> awaitTask = string capacityPointCount) "follow-latest viewport must end at the capacity tail"
    require (page.Locator("[data-capacity-positions='3820']").CountAsync() |> awaitTask = 1) "browser fixture must declare 3,820 positions"
    require (page.Locator("[data-capacity-shared-series='28']").CountAsync() |> awaitTask = 1) "browser fixture must declare 28 shared scalar series"
    requireText (page.Locator("[data-testid='ta-viewport-range']")) $"Loaded {capacityPointCount} bars"
    requireText (page.Locator("[data-testid='ta-viewport-range']")) $"Viewing {initialVisibleStart}-{capacityPointCount}"
    let sharedSma = page.Locator("[data-testid='ta-trace-sma-sma-1k']")
    require (sharedSma.CountAsync() |> awaitTask = 1) "shared-axis SMA trace must be mounted exactly once"
    require
        (sharedSma.GetAttributeAsync("d") |> awaitTask |> Option.ofObj |> Option.exists (String.IsNullOrWhiteSpace >> not))
        "shared-axis SMA trace path must be non-empty"
    let viewportBox = page.Locator("[data-testid='ta-viewport-panel']").BoundingBoxAsync() |> awaitTask
    let initialPriceBox = page.Locator("[data-testid='ta-candle-price']").BoundingBoxAsync() |> awaitTask
    require (not (isNull viewportBox) && not (isNull initialPriceBox)) "viewport navigator and first chart row must expose geometry"
    require (viewportBox.Y + viewportBox.Height <= initialPriceBox.Y + 1.0f) "viewport navigator must be visible before the first chart row"
    let crosshairs = page.Locator("[data-testid$='-crosshair']")
    require ((crosshairs.CountAsync() |> awaitTask) = 7) "every visible row must mount one stable crosshair overlay"
    require ((page.Locator("[data-testid$='-crosshair'][visibility='hidden']").CountAsync() |> awaitTask) = 7) "crosshair overlays must remain hidden before pointer movement"
    require ((page.Locator("[data-testid='ta-time-axis-shared']").CountAsync() |> awaitTask) = 1) "all rows must share one X axis"

    let rowLegends = page.Locator("[data-ta-row-values='true']")
    require ((rowLegends.CountAsync() |> awaitTask) = 7) "every visible row must expose one fixed legend/value band"
    let initialLegendHeights =
        [| for index in 0 .. 6 do
               let box = rowLegends.Nth(index).BoundingBoxAsync() |> awaitTask
               require (not (isNull box)) $"row legend {index} must expose geometry"
               yield box.Height |]
    require (initialLegendHeights |> Array.forall (fun height -> abs (height - 30.0f) <= 0.5f)) ("row legend heights must remain fixed at 30px: " + String.concat "," (initialLegendHeights |> Array.map string))

    let smaLegend = page.Locator("[data-testid='ta-row-values-sma']")
    let smaLegendToken = page.Locator("[data-testid='ta-row-value-sma-sma-1k']")
    let smaLegendLabel = smaLegendToken.Locator("[data-ta-row-value-label='true']")
    let smaLegendValue = smaLegendToken.Locator("[data-ta-row-value-text='true']")
    require (smaLegend.GetAttributeAsync("data-fixed-height") |> awaitTask = "30") "the SMA row value band must publish its fixed-height contract"
    waitForAttributeValue smaLegendValue "data-value-state" "defined"
    let labelBox = smaLegendLabel.BoundingBoxAsync() |> awaitTask
    let initialValueBox = smaLegendValue.BoundingBoxAsync() |> awaitTask
    let smaRowBox = page.Locator("[data-testid='ta-row-sma']").BoundingBoxAsync() |> awaitTask
    require (not (isNull labelBox) && not (isNull initialValueBox) && not (isNull smaRowBox)) "SMA row legend must expose stable label/value/row geometry"
    require (initialValueBox.X >= labelBox.X + labelBox.Width && initialValueBox.X - (labelBox.X + labelBox.Width) <= 6.0f) "the SMA value must sit immediately to the right of its own label"
    let renderSequenceBeforeLegendValues = requiredIntAttribute chartStack "data-chart-render-sequence"
    page.Locator("[data-testid='ta-demo-legend-undef']").ClickAsync() |> awaitUnit
    waitForAttributeValue smaLegendValue "data-value-state" "undefined"
    require (textOf smaLegendValue = "Undef") "an unavailable TA value must render as Undef in the existing value node"
    let undefLegendBox = smaLegend.BoundingBoxAsync() |> awaitTask
    let undefValueBox = smaLegendValue.BoundingBoxAsync() |> awaitTask
    let undefRowBox = page.Locator("[data-testid='ta-row-sma']").BoundingBoxAsync() |> awaitTask
    require (abs (undefLegendBox.Height - initialLegendHeights[2]) <= 0.5f && abs (undefValueBox.Width - initialValueBox.Width) <= 0.5f && abs (undefRowBox.Height - smaRowBox.Height) <= 0.5f) "Undef must not change legend/value/row geometry"
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

    let latestPriceCandle = page.Locator("[data-testid='ta-candle-price-price-1k'][data-candle-part='body']").Last
    let previewCloseBefore = latestPriceCandle.GetAttributeAsync("data-close") |> awaitTask |> Option.ofObj |> Option.defaultValue ""
    let renderSequenceBeforePreview = requiredIntAttribute chartStack "data-chart-render-sequence"
    page.Locator("[data-testid='ta-demo-preview-update']").ClickAsync() |> awaitUnit
    let previewCloseAfter = waitForAttributeChange latestPriceCandle "data-close" previewCloseBefore
    require (previewCloseAfter <> previewCloseBefore) "same-position live preview must update the latest candle close"
    let renderSequenceAfterPreview = requiredIntAttribute chartStack "data-chart-render-sequence"
    require
        (renderSequenceAfterPreview = renderSequenceBeforePreview)
        $"same-position live preview must update SVG attributes without rebuilding the chart stack; render={renderSequenceBeforePreview}->{renderSequenceAfterPreview}; close={previewCloseBefore}->{previewCloseAfter}"
    let fixtureRoot = page.Locator("[data-capacity-positions]")
    let streamCloseBefore = latestPriceCandle.GetAttributeAsync("data-close") |> awaitTask |> Option.ofObj |> Option.defaultValue ""
    page.Locator("[data-testid='ta-demo-preview-stream']").ClickAsync() |> awaitUnit
    waitForIntAttributeAtLeast fixtureRoot "data-preview-stream-updates" 1 |> ignore
    let streamCloseAfter = waitForAttributeChange latestPriceCandle "data-close" streamCloseBefore
    require (streamCloseAfter <> streamCloseBefore) "the live preview stream must advance the visible close while follow-latest is active"

    let navigator = page.Locator("[data-testid='ta-overview-navigator']")
    let navigatorBox = navigator.BoundingBoxAsync() |> awaitTask
    let selectionBox = page.Locator("[data-testid='ta-overview-selection']").BoundingBoxAsync() |> awaitTask
    require (not (isNull navigatorBox) && not (isNull selectionBox)) "overview navigator and selection must expose pointer geometry"
    require (page.Locator("[data-testid='ta-overview-left-handle']").IsVisibleAsync() |> awaitTask) "overview must expose a left resize handle"
    require (page.Locator("[data-testid='ta-overview-right-handle']").IsVisibleAsync() |> awaitTask) "overview must expose a right resize handle"
    let callbackState = page.Locator("[data-testid='ta-demo-callback-state']")
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
    require
        (committedEnd - committedStart + 1 = visiblePointCount && committedStart < initialVisibleStart)
        ("move release must commit one historical 48-bar window: " + committedText)
    require (requiredIntAttribute chartStack "data-chart-render-sequence" = renderSequenceBeforeDrag + 1) "release must commit exactly one chart render"
    require (chartStack.GetAttributeAsync("data-follow-latest") |> awaitTask = "false") "historical viewport navigation must leave follow-latest mode"
    waitForText callbackState "callback actions 1 / last VisibleRangeChanged"

    let priceChart = page.Locator("[data-testid='ta-candle-price']")
    let pointerBox = priceChart.BoundingBoxAsync() |> awaitTask
    require (not (isNull pointerBox)) "price chart must expose pointer geometry"
    let timeLabels = page.Locator("[data-testid='ta-time-axis-shared'] span")
    require ((timeLabels.CountAsync() |> awaitTask) = 3) "shared X axis must expose first, middle, and last time labels"
    let firstTimeLabel = textOf (timeLabels.Nth(0))
    let expectedMiddleLabel = textOf (timeLabels.Nth(1))
    let lastTimeLabel = textOf (timeLabels.Nth(2))
    require (not (String.IsNullOrWhiteSpace firstTimeLabel)) "shared X axis first label must not be empty"
    require (not (String.IsNullOrWhiteSpace expectedMiddleLabel)) "shared X axis middle label must not be empty"
    require (not (String.IsNullOrWhiteSpace lastTimeLabel)) "shared X axis last label must not be empty"
    require (firstTimeLabel <> lastTimeLabel) "shared X axis endpoints must represent different bars"
    let renderSequenceBeforeCursor = requiredIntAttribute chartStack "data-chart-render-sequence"
    let firstCrosshair = crosshairs.First
    let crosshairXBefore = firstCrosshair.GetAttributeAsync("x1") |> awaitTask |> Option.ofObj |> Option.defaultValue ""
    let smaLegendValueBeforeCursor = textOf smaLegendValue
    let cursorLatency = Diagnostics.Stopwatch.StartNew()
    priceChart.HoverAsync() |> awaitUnit
    let crosshairXAfter = waitForAttributeChange firstCrosshair "x1" crosshairXBefore
    let smaLegendValueAfterCursor = waitForTextChange smaLegendValue smaLegendValueBeforeCursor
    cursorLatency.Stop()
    let cursorValues = page.Locator("[data-testid='ta-cross-scale-values']")
    waitForText cursorValues expectedMiddleLabel
    require (requiredIntAttribute chartStack "data-chart-render-sequence" = renderSequenceBeforeCursor) "pointer movement must update only the cursor overlay, not rebuild the chart stack"
    require (smaLegendValueAfterCursor <> "Undef") "cursor movement must update the existing row legend value node"
    require (cursorLatency.ElapsedMilliseconds <= 250L) $"shared cursor update exceeded 250ms: {cursorLatency.ElapsedMilliseconds}ms"
    require ((page.Locator("[data-testid$='-crosshair'][visibility='visible']").CountAsync() |> awaitTask) = 7) "pointer movement on one row must reveal one shared crosshair in every visible row"
    let crosshairPositions =
        page.Locator("[data-testid$='-crosshair']").AllAsync()
        |> awaitTask
        |> Seq.map (fun locator -> locator.GetAttributeAsync("x1") |> awaitTask |> Option.ofObj |> Option.defaultValue "missing")
        |> Seq.distinct
        |> Seq.toArray
    require (crosshairPositions.Length = 1 && crosshairPositions[0] = crosshairXAfter && crosshairPositions[0] <> "0" && crosshairPositions[0] <> "100") ("shared pointer crosshair positions diverged: " + String.concat "," crosshairPositions)

    let previewUpdatesBeforeCursor = requiredIntAttribute fixtureRoot "data-preview-stream-updates"
    let historicalCloseBeforeCursor = latestPriceCandle.GetAttributeAsync("data-close") |> awaitTask |> Option.ofObj |> Option.defaultValue ""
    let sustainedCursor = Diagnostics.Stopwatch.StartNew()
    let mutable previousCrosshairX = crosshairXAfter
    let mutable cursorTransitions = 0
    let mutable maximumCursorLatencyMs = 0L
    for sample in 0 .. 299 do
        let ratio = if sample % 2 = 0 then 0.18f else 0.82f
        let movement = Diagnostics.Stopwatch.StartNew()
        page.Mouse.MoveAsync(pointerBox.X + pointerBox.Width * ratio, pointerBox.Y + pointerBox.Height / 2.0f) |> awaitUnit
        let currentCrosshairX = waitForAttributeChange firstCrosshair "x1" previousCrosshairX
        movement.Stop()
        cursorTransitions <- cursorTransitions + 1
        maximumCursorLatencyMs <- max maximumCursorLatencyMs movement.ElapsedMilliseconds
        previousCrosshairX <- currentCrosshairX
    sustainedCursor.Stop()
    require (cursorTransitions >= 300) $"sustained cursor movement produced too few crosshair transitions: {cursorTransitions}"
    require (maximumCursorLatencyMs < 250L) $"sustained cursor movement stalled for {maximumCursorLatencyMs}ms"
    require (sustainedCursor.Elapsed < TimeSpan.FromSeconds 12.0) $"sustained cursor movement exceeded 12 seconds: {sustainedCursor.Elapsed}"
    let concurrentPreviewUpdates = requiredIntAttribute fixtureRoot "data-preview-stream-updates"
    let concurrentPreviewUpdateCount = concurrentPreviewUpdates - previewUpdatesBeforeCursor
    require (concurrentPreviewUpdateCount >= 3) $"sustained cursor gate observed only {concurrentPreviewUpdateCount} concurrent live preview updates"
    let historicalCloseAfterCursor = latestPriceCandle.GetAttributeAsync("data-close") |> awaitTask |> Option.ofObj |> Option.defaultValue ""
    require (historicalCloseAfterCursor = historicalCloseBeforeCursor) "realtime tail updates must not overwrite the committed historical viewport"
    require (requiredIntAttribute chartStack "data-chart-render-sequence" = renderSequenceBeforeCursor) "sustained pointer movement must not rebuild the chart stack"
    let sustainedLegendBox = smaLegend.BoundingBoxAsync() |> awaitTask
    let sustainedValueBox = smaLegendValue.BoundingBoxAsync() |> awaitTask
    let sustainedRowBox = page.Locator("[data-testid='ta-row-sma']").BoundingBoxAsync() |> awaitTask
    require (abs (sustainedLegendBox.Height - initialLegendHeights[2]) <= 0.5f && abs (sustainedValueBox.Width - initialValueBox.Width) <= 0.5f && abs (sustainedRowBox.Height - smaRowBox.Height) <= 0.5f) "cursor and concurrent live revisions must not change row legend geometry"

    priceChart.ClickAsync() |> awaitUnit
    waitForText callbackState "callback actions 2 / last SharedCursorChanged"
    Directory.CreateDirectory outputDirectory |> ignore
    page.ScreenshotAsync(PageScreenshotOptions(Path = Path.Combine(outputDirectory, "desktop-crossrow-cursor.png"), FullPage = true)) |> awaitTask |> ignore

    page.Locator("[data-testid='ta-demo-paused']").ClickAsync() |> awaitUnit
    waitForText (page.Locator("[data-testid='ta-poll-state']")) "RESYNC"
    require (page.Locator("[data-testid='ta-apply-query']").IsDisabledAsync() |> awaitTask) "paused cache must suppress remote query commands"
    require (not (page.Locator("[data-testid='ta-pan-left']").IsDisabledAsync() |> awaitTask)) "paused cache must retain local viewport navigation"
    let pausedViewportBefore = textOf viewportRange
    page.Locator("[data-testid='ta-pan-left']").ClickAsync() |> awaitUnit
    System.Threading.Thread.Sleep 250
    require (textOf viewportRange <> pausedViewportBefore) "paused local pan must update the visible viewport"
    priceChart.HoverAsync() |> awaitUnit
    require ((page.Locator("[data-testid$='-crosshair']").CountAsync() |> awaitTask) = 7) "paused cache must retain local hover/crosshair"
    priceChart.ClickAsync() |> awaitUnit
    System.Threading.Thread.Sleep 250
    requireText callbackState "callback actions 2 / last SharedCursorChanged"

    let chartPointsBeforeStatusChange = requiredIntAttribute (page.Locator("[data-testid='ta-candle-price']")) "data-point-count"
    page.Locator("[data-testid='ta-demo-inflight']").ClickAsync() |> awaitUnit
    waitForText (page.Locator("[data-testid='ta-poll-state']")) "UPDATING"
    require (page.Locator("[data-testid='ta-apply-query']").IsDisabledAsync() |> awaitTask) "remote query must be disabled while a poll is in flight"
    require (page.Locator("[data-testid='ta-pan-left']").IsDisabledAsync() |> awaitTask) "event-range viewport controls must be disabled while a poll is in flight"
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

    requireText callbackState "callback actions 2"
    page.Locator("[data-testid='ta-pan-right']").ClickAsync() |> awaitUnit
    waitForText callbackState "callback actions 3 / last VisibleRangeChanged"
    page.Locator("[data-testid='ta-zoom-in']").ClickAsync() |> awaitUnit
    waitForText callbackState "callback actions 4 / last VisibleRangeChanged"

    page.Locator("[data-testid='ta-reset-view']").ClickAsync() |> awaitUnit
    waitForAttributeValue (page.Locator("[data-testid='ta-chart-stack']")) "data-follow-latest" "true"
    waitForText callbackState "callback actions 5 / last VisibleRangeChanged"

    let volumeRow = page.Locator("[data-testid='ta-row-volume']")
    require (volumeRow.IsVisibleAsync() |> awaitTask) "volume row must begin visible"
    page.Locator("[data-testid='ta-toggle-row-volume']").ClickAsync() |> awaitUnit
    volumeRow.WaitForAsync(LocatorWaitForOptions(State = WaitForSelectorState.Hidden, Timeout = 3000.0f)) |> awaitUnit
    requireText callbackState "callback actions 5"
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

    page.Locator("[data-testid='ta-remove-row-volume']").ClickAsync() |> awaitUnit
    volumeRow.WaitForAsync(LocatorWaitForOptions(State = WaitForSelectorState.Hidden, Timeout = 3000.0f)) |> awaitUnit

    page.Locator("[data-testid='ta-apply-query']").ClickAsync() |> awaitUnit
    waitForText callbackState "last ChangeTaQuery"
    page.Locator("[data-testid='ta-reset-canvas']").ClickAsync() |> awaitUnit
    waitForText callbackState "last ResetCanvas"
    volumeRow.WaitForAsync(LocatorWaitForOptions(State = WaitForSelectorState.Visible, Timeout = 3000.0f)) |> awaitUnit
    require ((page.Locator("[data-testid='ta-row-template-ta-macd-8']").CountAsync() |> awaitTask) = 0) "Reset Canvas must remove post-mount added rows"

    page.Locator("[data-testid='ta-view-all']").ClickAsync() |> awaitUnit
    waitForText (page.Locator("[data-testid='ta-viewport-range']")) $"Viewing 1-{capacityPointCount}"
    waitForText callbackState "last VisibleRangeChanged"
    waitForEnabled (page.Locator("[data-testid='ta-pan-left']")) "viewport controls after All"
    let renderBeforeRightHandle = requiredIntAttribute chartStack "data-chart-render-sequence"
    require (requiredIntAttribute (page.Locator("[data-testid='ta-candle-price']")) "data-point-count" = capacityPointCount) "All preset must render the full loaded capacity range"
    let allNavigatorBox = navigator.BoundingBoxAsync() |> awaitTask
    let rightHandle = page.Locator("[data-testid='ta-overview-right-handle']")
    let rightHandleBox = rightHandle.BoundingBoxAsync() |> awaitTask
    require (not (isNull rightHandleBox)) "right overview handle must expose geometry"
    rightHandle.HoverAsync() |> awaitUnit
    page.Mouse.DownAsync(MouseDownOptions(Button = MouseButton.Left)) |> awaitUnit
    page.Mouse.MoveAsync(allNavigatorBox.X + allNavigatorBox.Width * 0.75f, allNavigatorBox.Y + allNavigatorBox.Height / 2.0f, MouseMoveOptions(Steps = 8)) |> awaitUnit
    System.Threading.Thread.Sleep 50
    require ((textOf (page.Locator("[data-testid='ta-viewport-range']"))).Contains "Preview") "right-handle drag must publish preview bounds"
    require (requiredIntAttribute chartStack "data-chart-render-sequence" = renderBeforeRightHandle) "right-handle preview must not rebuild after the All preset render"
    page.Mouse.UpAsync(MouseUpOptions(Button = MouseButton.Left)) |> awaitUnit
    waitForIntAttribute chartStack "data-chart-render-sequence" (renderBeforeRightHandle + 1)
    let resizedNavigatorBox = navigator.BoundingBoxAsync() |> awaitTask
    let leftHandle = page.Locator("[data-testid='ta-overview-left-handle']")
    let leftHandleBox = leftHandle.BoundingBoxAsync() |> awaitTask
    let renderBeforeLeftHandle = requiredIntAttribute chartStack "data-chart-render-sequence"
    require (not (isNull leftHandleBox)) "left overview handle must expose geometry"
    leftHandle.HoverAsync() |> awaitUnit
    page.Mouse.DownAsync(MouseDownOptions(Button = MouseButton.Left)) |> awaitUnit
    page.Mouse.MoveAsync(resizedNavigatorBox.X + resizedNavigatorBox.Width * 0.25f, resizedNavigatorBox.Y + resizedNavigatorBox.Height / 2.0f, MouseMoveOptions(Steps = 8)) |> awaitUnit
    System.Threading.Thread.Sleep 50
    require ((textOf (page.Locator("[data-testid='ta-viewport-range']"))).Contains "Preview") "left-handle drag must publish preview bounds"
    require (requiredIntAttribute chartStack "data-chart-render-sequence" = renderBeforeLeftHandle) "left-handle preview must not rebuild the chart"
    page.Mouse.UpAsync(MouseUpOptions(Button = MouseButton.Left)) |> awaitUnit
    waitForIntAttribute chartStack "data-chart-render-sequence" (renderBeforeLeftHandle + 1)

    page.Locator("[data-testid='ta-view-48']").ClickAsync() |> awaitUnit
    waitForText (page.Locator("[data-testid='ta-viewport-range']")) $"Viewing {initialVisibleStart}-{capacityPointCount}"
    waitForEnabled (page.Locator("[data-testid='ta-pan-left']")) "viewport controls before document replacement"
    page.Locator("[data-testid='ta-demo-replace-document']").ClickAsync() |> awaitUnit
    waitForText (page.Locator("[data-testid='ta-workspace-title']")) "SMA(30)"
    waitForText (page.Locator("[data-testid='ta-canvas-identity']")) "ta-demo-canvas-replacement"
    requireText (page.Locator("[data-testid='ta-toggle-row-price']")) "ES 1K + SMA(30)"
    requireText (page.Locator("[data-testid='ta-row-price']")) "ES 1K + SMA(30)"
    require (not ((textOf (page.Locator("[data-testid='ta-row-price']"))).Contains "SMA(20)")) "replacement document must not retain the prior static row label"

    require (consoleErrors.Count = 0) ("desktop console errors: " + String.concat " | " consoleErrors)

    Directory.CreateDirectory outputDirectory |> ignore
    page.ScreenshotAsync(PageScreenshotOptions(Path = Path.Combine(outputDirectory, "desktop.png"), FullPage = true)) |> awaitTask |> ignore
    context.CloseAsync() |> awaitUnit
    titleBox, priceBox, cursorLatency.ElapsedMilliseconds, cursorTransitions, maximumCursorLatencyMs, sustainedCursor.ElapsedMilliseconds, concurrentPreviewUpdateCount

let verifyMobile (browser: IBrowser) =
    let viewportWidth = 390
    let context = browser.NewContextAsync(BrowserNewContextOptions(ViewportSize = ViewportSize(Width = viewportWidth, Height = 844), IsMobile = true)) |> awaitTask
    let page = context.NewPageAsync() |> awaitTask
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

    page.Locator("[data-testid='ta-add-row-toggle']").ClickAsync() |> awaitUnit
    requireBoxInside viewportWidth "mobile Add Row editor" (page.Locator("[data-testid='ta-add-row-editor']").BoundingBoxAsync() |> awaitTask)
    require (consoleErrors.Count = 0) ("mobile console errors: " + String.concat " | " consoleErrors)

    Directory.CreateDirectory outputDirectory |> ignore
    page.ScreenshotAsync(PageScreenshotOptions(Path = Path.Combine(outputDirectory, "mobile.png"), FullPage = true)) |> awaitTask |> ignore
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
