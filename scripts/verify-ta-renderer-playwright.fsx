// Real-browser operation and geometry verifier for the pure WebSharper TA renderer demo.

#i @"nuget: C:\Program Files\dotnet\sdk\10.0.301\FSharp\library-packs"
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

let waitForText (locator: ILocator) (expected: string) =
    let deadline = DateTime.UtcNow.AddSeconds 8.0
    let mutable matched = false

    while not matched && DateTime.UtcNow < deadline do
        let actualText = locator.TextContentAsync() |> awaitTask |> Option.ofObj |> Option.defaultValue ""
        matched <- actualText.Contains expected
        if not matched then Threading.Thread.Sleep 50

    requireText locator expected

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
    page.Locator("[data-testid='ta-workspace']").WaitForAsync(LocatorWaitForOptions(Timeout = 5000.0f)) |> awaitUnit

    requireText (page.Locator("[data-testid='ta-workspace-title']")) "PTMD TA Research"
    requireText (page.Locator("[data-testid='ta-freshness']")) "LIVE"
    require ((page.Locator("[data-testid='ta-chart-stack'] section").CountAsync() |> awaitTask) = 7) "all seven configured TA rows must render"
    require (requiredIntAttribute (page.Locator("[data-testid='ta-candle-price']")) "data-point-count" = 48) "candlestick chart must retain all 48 committed visible points"
    requireText (page.Locator("[data-testid='ta-status-detail']")) "watermark 2026-07-11T09:30:00Z"
    requireText (page.Locator("[data-testid='ta-status-detail']")) "quality complete"

    let chartStack = page.Locator("[data-testid='ta-chart-stack']")
    require (chartStack.GetAttributeAsync("data-loaded-bars") |> awaitTask = "2000") "loaded-range metadata must report all 2000 browser-demo bars"
    require (chartStack.GetAttributeAsync("data-visible-start") |> awaitTask = "1953") "follow-latest viewport must begin at loaded bar 1953"
    require (chartStack.GetAttributeAsync("data-visible-end") |> awaitTask = "2000") "follow-latest viewport must end at loaded bar 2000"
    requireText (page.Locator("[data-testid='ta-viewport-range']")) "Loaded 2000 bars"
    requireText (page.Locator("[data-testid='ta-viewport-range']")) "Viewing 1953-2000"
    let viewportBox = page.Locator("[data-testid='ta-viewport-panel']").BoundingBoxAsync() |> awaitTask
    let initialPriceBox = page.Locator("[data-testid='ta-candle-price']").BoundingBoxAsync() |> awaitTask
    require (not (isNull viewportBox) && not (isNull initialPriceBox)) "viewport navigator and first chart row must expose geometry"
    require (viewportBox.Y + viewportBox.Height <= initialPriceBox.Y + 1.0f) "viewport navigator must be visible before the first chart row"
    require ((page.Locator("[data-testid$='-crosshair']").CountAsync() |> awaitTask) = 0) "cross-row cursor must not be fabricated before pointer movement"
    require ((page.Locator("[data-testid='ta-time-axis-shared']").CountAsync() |> awaitTask) = 1) "all rows must share one X axis"

    let navigator = page.Locator("[data-testid='ta-overview-navigator']")
    let navigatorBox = navigator.BoundingBoxAsync() |> awaitTask
    let selectionBox = page.Locator("[data-testid='ta-overview-selection']").BoundingBoxAsync() |> awaitTask
    require (not (isNull navigatorBox) && not (isNull selectionBox)) "overview navigator and selection must expose pointer geometry"
    require (page.Locator("[data-testid='ta-overview-left-handle']").IsVisibleAsync() |> awaitTask) "overview must expose a left resize handle"
    require (page.Locator("[data-testid='ta-overview-right-handle']").IsVisibleAsync() |> awaitTask) "overview must expose a right resize handle"
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
    require (chartStack.GetAttributeAsync("data-visible-start") |> awaitTask = "1953") "committed viewport must remain stable before release"
    page.Mouse.UpAsync(MouseUpOptions(Button = MouseButton.Left)) |> awaitUnit
    waitForText viewportRange "Viewing"
    let committedText = textOf viewportRange
    let committedMatch = Text.RegularExpressions.Regex.Match(committedText, "Viewing ([0-9]+)-([0-9]+)")
    require committedMatch.Success ("release did not publish committed bounds: " + committedText)
    let committedStart = Int32.Parse committedMatch.Groups[1].Value
    let committedEnd = Int32.Parse committedMatch.Groups[2].Value
    require
        (committedEnd - committedStart + 1 = 48 && committedStart < 1953)
        ("move release must commit one historical 48-bar window: " + committedText)
    require (requiredIntAttribute chartStack "data-chart-render-sequence" = renderSequenceBeforeDrag + 1) "release must commit exactly one chart render"
    require (chartStack.GetAttributeAsync("data-follow-latest") |> awaitTask = "false") "historical viewport navigation must leave follow-latest mode"

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
    priceChart.HoverAsync() |> awaitUnit
    let cursorValues = page.Locator("[data-testid='ta-cursor-values']")
    waitForText cursorValues expectedMiddleLabel
    require ((page.Locator("[data-testid$='-crosshair']").CountAsync() |> awaitTask) = 7) "pointer movement on one row must create one shared crosshair in every visible row"
    let crosshairPositions =
        page.Locator("[data-testid$='-crosshair']").AllAsync()
        |> awaitTask
        |> Seq.map (fun locator -> locator.GetAttributeAsync("x1") |> awaitTask |> Option.ofObj |> Option.defaultValue "missing")
        |> Seq.distinct
        |> Seq.toArray
    require (crosshairPositions.Length = 1 && crosshairPositions[0] <> "0" && crosshairPositions[0] <> "100") ("shared pointer crosshair positions diverged: " + String.concat "," crosshairPositions)
    Directory.CreateDirectory outputDirectory |> ignore
    page.ScreenshotAsync(PageScreenshotOptions(Path = Path.Combine(outputDirectory, "desktop-crossrow-cursor.png"), FullPage = true)) |> awaitTask |> ignore

    let chartPointsBeforeStatusChange = requiredIntAttribute (page.Locator("[data-testid='ta-candle-price']")) "data-point-count"
    page.Locator("[data-testid='ta-demo-inflight']").ClickAsync() |> awaitUnit
    waitForText (page.Locator("[data-testid='ta-poll-state']")) "UPDATING"
    require (page.Locator("[data-testid='ta-apply-query']").IsDisabledAsync() |> awaitTask) "remote query must be disabled while a poll is in flight"
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

    let callbackState = page.Locator("[data-testid='ta-demo-callback-state']")
    requireText callbackState "callback actions 0"
    page.Locator("[data-testid='ta-pan-right']").ClickAsync() |> awaitUnit
    page.Locator("[data-testid='ta-zoom-in']").ClickAsync() |> awaitUnit
    requireText callbackState "callback actions 0"

    page.Locator("[data-testid='ta-reset-view']").ClickAsync() |> awaitUnit
    requireText (page.Locator("[data-testid='ta-feedback']")) "Local view reset."
    requireText callbackState "callback actions 0"

    let volumeRow = page.Locator("[data-testid='ta-row-volume']")
    require (volumeRow.IsVisibleAsync() |> awaitTask) "volume row must begin visible"
    page.Locator("[data-testid='ta-toggle-row-volume']").ClickAsync() |> awaitUnit
    volumeRow.WaitForAsync(LocatorWaitForOptions(State = WaitForSelectorState.Hidden, Timeout = 3000.0f)) |> awaitUnit
    requireText callbackState "callback actions 0"
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
    require (page.Locator("[data-testid='ta-add-row-submit']").IsDisabledAsync() |> awaitTask) "pending Add must disable duplicate submission"
    editor.WaitForAsync(LocatorWaitForOptions(State = WaitForSelectorState.Hidden, Timeout = 8000.0f)) |> awaitUnit
    waitForText callbackState "last ApplyTemplate ta.macd"
    require ((page.Locator("[data-testid='ta-toggle-row-template-ta-macd-8']").CountAsync() |> awaitTask) = 1) "generic Add must append one authoritative MACD row"

    let rowCountBeforeEdit = page.Locator("[data-testid^='ta-toggle-row-']").CountAsync() |> awaitTask
    page.Locator("[data-testid='ta-edit-row-sma']").ClickAsync() |> awaitUnit
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
    page.Locator("[data-testid='ta-edit-row-sma']").ClickAsync() |> awaitUnit
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

    let renderBeforeRightHandle = requiredIntAttribute chartStack "data-chart-render-sequence"
    page.Locator("[data-testid='ta-view-all']").ClickAsync() |> awaitUnit
    waitForText (page.Locator("[data-testid='ta-viewport-range']")) "Viewing 1-2000"
    require (requiredIntAttribute (page.Locator("[data-testid='ta-candle-price']")) "data-point-count" = 2000) "All preset must render the full loaded 2000-bar range"
    let allNavigatorBox = navigator.BoundingBoxAsync() |> awaitTask
    let rightHandle = page.Locator("[data-testid='ta-overview-right-handle']")
    let rightHandleBox = rightHandle.BoundingBoxAsync() |> awaitTask
    require (not (isNull rightHandleBox)) "right overview handle must expose geometry"
    rightHandle.HoverAsync() |> awaitUnit
    page.Mouse.DownAsync(MouseDownOptions(Button = MouseButton.Left)) |> awaitUnit
    page.Mouse.MoveAsync(allNavigatorBox.X + allNavigatorBox.Width * 0.75f, allNavigatorBox.Y + allNavigatorBox.Height / 2.0f, MouseMoveOptions(Steps = 8)) |> awaitUnit
    System.Threading.Thread.Sleep 50
    require ((textOf (page.Locator("[data-testid='ta-viewport-range']"))).Contains "Preview") "right-handle drag must publish preview bounds"
    require (requiredIntAttribute chartStack "data-chart-render-sequence" = renderBeforeRightHandle + 1) "right-handle preview must not rebuild after the All preset render"
    page.Mouse.UpAsync(MouseUpOptions(Button = MouseButton.Left)) |> awaitUnit
    waitForIntAttribute chartStack "data-chart-render-sequence" (renderBeforeRightHandle + 2)
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
    require (consoleErrors.Count = 0) ("desktop console errors: " + String.concat " | " consoleErrors)

    Directory.CreateDirectory outputDirectory |> ignore
    page.ScreenshotAsync(PageScreenshotOptions(Path = Path.Combine(outputDirectory, "desktop.png"), FullPage = true)) |> awaitTask |> ignore
    context.CloseAsync() |> awaitUnit
    titleBox, priceBox

let verifyMobile (browser: IBrowser) =
    let viewportWidth = 390
    let context = browser.NewContextAsync(BrowserNewContextOptions(ViewportSize = ViewportSize(Width = viewportWidth, Height = 844), IsMobile = true)) |> awaitTask
    let page = context.NewPageAsync() |> awaitTask
    let consoleErrors = ResizeArray<string>()
    page.Console.Add(fun (message: IConsoleMessage) -> if message.Type = "error" then consoleErrors.Add message.Text; printfn "mobile console error: %s" message.Text)
    page.PageError.Add(fun (error: string) -> consoleErrors.Add error; printfn "mobile page error: %s" error)

    page.GotoAsync(url, PageGotoOptions(WaitUntil = WaitUntilState.NetworkIdle)) |> awaitTask |> ignore
    page.Locator("[data-testid='ta-workspace']").WaitForAsync(LocatorWaitForOptions(Timeout = 5000.0f)) |> awaitUnit
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
    let titleBox, priceBox = verifyDesktop browser
    verifyMobile browser
    printfn "TA renderer Playwright PASS url=%s desktopTitle=(%.1f,%.1f,%.1f,%.1f) desktopPrice=(%.1f,%.1f,%.1f,%.1f) output=%s" url titleBox.X titleBox.Y titleBox.Width titleBox.Height priceBox.X priceBox.Y priceBox.Width priceBox.Height outputDirectory
finally
    browser.CloseAsync() |> awaitUnit
    playwright.Dispose()
