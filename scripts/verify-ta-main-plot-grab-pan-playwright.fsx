// Focused real-browser verifier for explicit main-plot Cursor/Resize interaction and row-local cursor visibility.

#i @"nuget: C:\Program Files\dotnet\sdk\10.0.402\FSharp\library-packs"
#r "nuget: FAkka.Argu, [10.1.301]"
#r "nuget: Expecto, 10.2.3"
#r "nuget: Microsoft.Playwright, 1.52.0"

#load "ParseLine.fsx"

open System
open System.IO
open System.Threading
open System.Threading.Tasks
open Argu
open Expecto
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
            | Output_Dir _ -> "Directory for the focused acceptance screenshot."
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
        "--url \"http://127.0.0.1:18882/\" --output-dir \"artifacts/ta-main-plot-grab-pan\" --browser-executable-path \"%s\""
        (defaultBrowserPath.Replace('\\', '/'))

let parser = ArgumentParser.Create<CliArgs>(programName = "verify-ta-main-plot-grab-pan-playwright.fsx")
let defaults = parser.Parse(PL.parseLine [| ' ' |] (Some '"') None true defaultArgumentsText)
let automationArguments = fsi.CommandLineArgs |> Array.skip 1 |> Array.skipWhile ((=) "--")
let automation = parser.Parse automationArguments
let pick tryAutomation tryDefault fallback =
    tryAutomation () |> Option.orElseWith tryDefault |> Option.defaultValue fallback

let url = pick (fun () -> automation.TryGetResult(<@ Url @>)) (fun () -> defaults.TryGetResult(<@ Url @>)) "http://127.0.0.1:18882/"
let outputDirectory = pick (fun () -> automation.TryGetResult(<@ Output_Dir @>)) (fun () -> defaults.TryGetResult(<@ Output_Dir @>)) "artifacts/ta-main-plot-grab-pan" |> Path.GetFullPath
let browserExecutablePath = pick (fun () -> automation.TryGetResult(<@ Browser_Executable_Path @>)) (fun () -> defaults.TryGetResult(<@ Browser_Executable_Path @>)) defaultBrowserPath
let headed = automation.Contains Headed || defaults.Contains Headed

let awaitTask (task: Task<'T>) = task.GetAwaiter().GetResult()
let awaitUnit (task: Task) = task.GetAwaiter().GetResult()
let require condition message = if not condition then failwith ("Main-plot interaction verification failed: " + message)

let attributeOf (locator: ILocator) name =
    locator.GetAttributeAsync(name) |> awaitTask |> Option.ofObj |> Option.defaultValue ""

let intAttribute (locator: ILocator) name =
    match Int32.TryParse(attributeOf locator name) with
    | true, value -> value
    | _ -> failwith $"Main-plot interaction verification failed: `{name}` is not an integer on `{locator}`."

let boundingBox (locator: ILocator) =
    locator.BoundingBoxAsync() |> awaitTask |> Option.ofObj |> Option.defaultWith (fun () -> failwith $"No geometry for `{locator}`.")

let waitUntil label probe predicate =
    let deadline = DateTime.UtcNow.AddSeconds 15.0
    let mutable value = probe ()
    while not (predicate value) && DateTime.UtcNow < deadline do
        Thread.Sleep 50
        value <- probe ()
    require (predicate value) (label + "; last=" + string value)
    value

let tests =
    testList "main plot explicit interaction" [
        testCase "real mouse pans visible window and right-click owns both boundary resizes" <| fun _ ->
            Directory.CreateDirectory(outputDirectory) |> ignore
            use playwright = Playwright.CreateAsync() |> awaitTask
            let launch = BrowserTypeLaunchOptions(Headless = not headed)
            if not (String.IsNullOrWhiteSpace browserExecutablePath) then launch.ExecutablePath <- browserExecutablePath
            let browser = playwright.Chromium.LaunchAsync(launch) |> awaitTask
            try
                let page = browser.NewPageAsync(BrowserNewPageOptions(ViewportSize = ViewportSize(Width = 1600, Height = 1400))) |> awaitTask
                let consoleErrors = ResizeArray<string>()
                page.Console.Add(fun message -> if message.Type = "error" then consoleErrors.Add(message.Text))
                page.GotoAsync(url, PageGotoOptions(WaitUntil = WaitUntilState.NetworkIdle)) |> awaitTask |> ignore
                page.Locator("[data-testid='ta-workspace']").WaitForAsync(LocatorWaitForOptions(Timeout = 15000.0f)) |> awaitUnit
                page.Locator("[data-testid='ta-demo-global-coverage']").ClickAsync() |> awaitUnit

                let chartStack = page.Locator("[data-testid='ta-chart-stack']")
                let globalWrapper = chartStack.Locator("[data-testid='ta-global-overview-with-axis']")
                let callbackState = page.Locator("[data-testid='ta-demo-callback-state']")
                let mainPlot = page.Locator("[data-testid='ta-candle-price']")
                let rowCursor = page.Locator("[data-testid='ta-row-cursor-label-price']")
                let horizontalCursors = page.Locator("[data-ta-row-horizontal-cursor='true']")
                let priceHorizontalCursor = page.Locator("[data-ta-row-horizontal-cursor-row-id='price']")
                let sharedCrosshairs = page.Locator("[data-ta-shared-crosshair='true']")

                waitUntil "GLOBAL total" (fun () -> intAttribute globalWrapper "data-total-observation-count") ((=) 10000) |> ignore
                waitUntil "resident start" (fun () -> intAttribute globalWrapper "data-resident-start-observation-ordinal") ((=) 3000) |> ignore
                waitUntil "main plot points" (fun () -> intAttribute mainPlot "data-point-count") (fun value -> value > 0) |> ignore

                let plotBounds = boundingBox mainPlot
                let downX = plotBounds.X + plotBounds.Width * 0.75f
                let upX = plotBounds.X + plotBounds.Width * 0.55f
                let pointerY = plotBounds.Y + plotBounds.Height * 0.45f
                let pointerDelta visibleCount startX currentX =
                    let raw = (float startX - float currentX) * float visibleCount / float plotBounds.Width
                    if raw >= 0.0 then int (Math.Floor(raw + 0.5)) else int (Math.Ceiling(raw - 0.5))
                let expectedDelta = pointerDelta (intAttribute mainPlot "data-point-count") downX upX
                require (expectedDelta > 0) "left drag did not produce a positive reverse pan delta"
                require (attributeOf mainPlot "data-main-plot-interaction-mode" = "cursor") "main plot did not default to CURSOR mode"

                require (horizontalCursors.CountAsync() |> awaitTask > 1) "fixture must expose multiple TA rows"
                require ((page.Locator("[data-ta-row-horizontal-cursor='true'][visibility='hidden']").CountAsync() |> awaitTask) = (horizontalCursors.CountAsync() |> awaitTask)) "horizontal cursors were visible before hover"
                page.Mouse.MoveAsync(downX, pointerY) |> awaitUnit
                waitUntil "hovered row horizontal cursor" (fun () -> attributeOf priceHorizontalCursor "visibility") ((=) "visible") |> ignore
                require ((page.Locator("[data-ta-row-horizontal-cursor='true'][visibility='visible']").CountAsync() |> awaitTask) = 1) "more than the hovered row exposed a horizontal cursor"
                let firstY = attributeOf priceHorizontalCursor "y1"
                require (firstY <> "0" && attributeOf priceHorizontalCursor "y2" = firstY) "horizontal cursor endpoints did not share pointer Y"
                page.Mouse.MoveAsync(downX, plotBounds.Y + plotBounds.Height * 0.72f) |> awaitUnit
                let secondY = waitUntil "horizontal cursor Y change" (fun () -> attributeOf priceHorizontalCursor "y1") ((<>) firstY)
                require (attributeOf priceHorizontalCursor "y2" = secondY) "horizontal cursor second endpoint did not follow pointer Y"
                page.Mouse.MoveAsync(plotBounds.X - 4.0f, pointerY) |> awaitUnit
                waitUntil "horizontal cursor leave" (fun () -> attributeOf priceHorizontalCursor "visibility") ((=) "hidden") |> ignore

                page.Mouse.MoveAsync(downX, pointerY) |> awaitUnit
                let anchoredEventTime = waitUntil "down anchor" (fun () -> attributeOf rowCursor "data-canonical-event-time") (String.IsNullOrWhiteSpace >> not)
                let residentBefore = intAttribute globalWrapper "data-resident-start-observation-ordinal"
                let visibleBefore = intAttribute chartStack "data-visible-start"
                let localBefore = intAttribute chartStack "data-local-visible-start"
                let callbackBefore = intAttribute callbackState "data-callback-count"

                page.Mouse.DownAsync(MouseDownOptions(Button = MouseButton.Left)) |> awaitUnit
                page.Mouse.MoveAsync(upX, pointerY, MouseMoveOptions(Steps = 8)) |> awaitUnit
                require (intAttribute callbackState "data-callback-count" = callbackBefore) "pointermove submitted before pointerup"
                page.Mouse.UpAsync(MouseUpOptions(Button = MouseButton.Left)) |> awaitUnit
                waitUntil "exactly one action" (fun () -> intAttribute callbackState "data-callback-count") ((=) (callbackBefore + 1)) |> ignore
                waitUntil "visible action kind" (fun () -> attributeOf callbackState "data-last-action") ((=) "VisibleRangeChanged") |> ignore

                require (intAttribute globalWrapper "data-resident-start-observation-ordinal" = residentBefore) "CURSOR pan relocated the GLOBAL resident"
                waitUntil "resident-relative visible offset" (fun () -> intAttribute chartStack "data-local-visible-start") ((=) (localBefore + expectedDelta)) |> ignore
                let visibleAfter = intAttribute chartStack "data-visible-start"
                require (visibleAfter = visibleBefore + expectedDelta) "visible start did not move by the actual pointer distance"

                page.Mouse.MoveAsync(upX + 2.0f, pointerY) |> awaitUnit
                page.Mouse.MoveAsync(upX, pointerY) |> awaitUnit
                let anchoredEventTimeAfter = waitUntil "anchor after pan" (fun () -> attributeOf rowCursor "data-canonical-event-time") ((=) anchoredEventTime)
                require (anchoredEventTimeAfter = anchoredEventTime) $"anchor drifted: before={anchoredEventTime}; after={anchoredEventTimeAfter}"

                page.Mouse.ClickAsync(downX, pointerY, MouseClickOptions(Button = MouseButton.Right)) |> awaitUnit
                waitUntil "Resize mode" (fun () -> attributeOf mainPlot "data-main-plot-interaction-mode") ((=) "resize") |> ignore
                require ((page.Locator("[data-ta-row-horizontal-cursor='true'][visibility='visible']").CountAsync() |> awaitTask) = 0) "Resize mode retained a horizontal crosshair"
                require ((page.Locator("[data-ta-shared-crosshair='true'][visibility='visible']").CountAsync() |> awaitTask) = 0) "Resize mode retained a shared vertical crosshair"

                let resizeLeftDownX = plotBounds.X + plotBounds.Width * 0.25f
                let resizeLeftUpX = plotBounds.X + plotBounds.Width * 0.35f
                let leftCountBefore = intAttribute chartStack "data-local-visible-count"
                let leftStartBefore = intAttribute chartStack "data-local-visible-start"
                let leftDelta = -(pointerDelta leftCountBefore resizeLeftDownX resizeLeftUpX)
                let leftCallbackBefore = intAttribute callbackState "data-callback-count"
                page.Mouse.MoveAsync(resizeLeftDownX, pointerY) |> awaitUnit
                page.Mouse.DownAsync(MouseDownOptions(Button = MouseButton.Left)) |> awaitUnit
                page.Mouse.MoveAsync(resizeLeftUpX, pointerY, MouseMoveOptions(Steps = 8)) |> awaitUnit
                page.Mouse.UpAsync(MouseUpOptions(Button = MouseButton.Left)) |> awaitUnit
                waitUntil "left resize action" (fun () -> intAttribute callbackState "data-callback-count") ((=) (leftCallbackBefore + 1)) |> ignore
                waitUntil "left resize start" (fun () -> intAttribute chartStack "data-local-visible-start") ((=) (leftStartBefore + leftDelta)) |> ignore
                require (intAttribute chartStack "data-local-visible-count" = leftCountBefore - leftDelta) "left resize changed the wrong boundary"

                let resizeRightDownX = plotBounds.X + plotBounds.Width * 0.70f
                let resizeRightUpX = plotBounds.X + plotBounds.Width * 0.80f
                let rightCountBefore = intAttribute chartStack "data-local-visible-count"
                let rightStartBefore = intAttribute chartStack "data-local-visible-start"
                let rightDelta = -(pointerDelta rightCountBefore resizeRightDownX resizeRightUpX)
                let rightCallbackBefore = intAttribute callbackState "data-callback-count"
                page.Mouse.MoveAsync(resizeRightDownX, pointerY) |> awaitUnit
                page.Mouse.DownAsync(MouseDownOptions(Button = MouseButton.Left)) |> awaitUnit
                page.Mouse.MoveAsync(resizeRightUpX, pointerY, MouseMoveOptions(Steps = 8)) |> awaitUnit
                page.Mouse.UpAsync(MouseUpOptions(Button = MouseButton.Left)) |> awaitUnit
                waitUntil "right resize action" (fun () -> intAttribute callbackState "data-callback-count") ((=) (rightCallbackBefore + 1)) |> ignore
                require (intAttribute chartStack "data-local-visible-start" = rightStartBefore) "right resize moved the left boundary"
                waitUntil "right resize count" (fun () -> intAttribute chartStack "data-local-visible-count") ((=) (rightCountBefore + rightDelta)) |> ignore
                require (intAttribute globalWrapper "data-resident-start-observation-ordinal" = residentBefore) "Resize mode relocated the GLOBAL resident"

                page.Mouse.ClickAsync(resizeRightUpX, pointerY, MouseClickOptions(Button = MouseButton.Right)) |> awaitUnit
                waitUntil "CURSOR mode restored" (fun () -> attributeOf mainPlot "data-main-plot-interaction-mode") ((=) "cursor") |> ignore
                page.Mouse.MoveAsync(resizeRightUpX - 2.0f, pointerY) |> awaitUnit
                page.Mouse.MoveAsync(resizeRightUpX, pointerY) |> awaitUnit
                waitUntil "crosshair restored" (fun () -> attributeOf priceHorizontalCursor "visibility") ((=) "visible") |> ignore
                require (consoleErrors.Count = 0) ("browser console errors: " + String.concat " | " consoleErrors)

                let screenshotPath = Path.Combine(outputDirectory, "main-plot-cursor-resize.png")
                page.ScreenshotAsync(PageScreenshotOptions(Path = screenshotPath, FullPage = true)) |> awaitTask |> ignore
                printfn "main-plot-interaction PASS panDelta=%d resident=%d leftDelta=%d rightDelta=%d anchor=%s screenshot=%s" expectedDelta residentBefore leftDelta rightDelta anchoredEventTime screenshotPath
            finally
                browser.CloseAsync() |> awaitUnit
    ]

runTestsWithCLIArgs [] [||] tests |> exit
