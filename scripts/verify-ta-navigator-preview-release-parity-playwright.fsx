// Focused real-browser regression for loaded-domain preview/release target parity.

#i @"nuget: C:\Program Files\dotnet\sdk\10.0.401\FSharp\library-packs"
#r "nuget: FAkka.Argu, [10.1.301]"
#r "nuget: Expecto, [11.1.0]"
#r "nuget: Microsoft.Playwright, 1.52.0"

#load "ParseLine.fsx"

open System
open System.IO
open System.Text.RegularExpressions
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
let floatAttribute locator name =
    match Single.TryParse(attributeOf locator name, Globalization.NumberStyles.Float, Globalization.CultureInfo.InvariantCulture) with
    | true, value -> value
    | _ -> failwith $"Navigator preview/release parity failed: `{name}` is not a floating-point number."
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

let boundingBox (locator: ILocator) =
    let bounds = locator.BoundingBoxAsync() |> awaitTask
    require (not (isNull bounds)) "navigator geometry is unavailable"
    bounds

let approximately tolerance left right = abs (float left - float right) <= tolerance

let tests =
    testCase "semantic navigator gesture ownership survives reactive rerender" <| fun _ ->
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

            let fixture = page.Locator("[data-testid='ta-demo-loaded-coverage-parity']")
            let chartStack = page.Locator("[data-testid='ta-chart-stack']:visible")
            let callbackState = page.Locator("[data-testid='ta-demo-callback-state']")
            let navigator = chartStack.Locator("[data-testid='ta-overview-navigator']")
            let selection = navigator.Locator("[data-testid='ta-overview-selection']")
            let range = page.Locator("[data-testid='ta-viewport-range']")

            let resetFixture () =
                page.ReloadAsync(PageReloadOptions(WaitUntil = WaitUntilState.NetworkIdle)) |> awaitTask |> ignore
                page.Locator("[data-testid='ta-workspace']").WaitForAsync(LocatorWaitForOptions(Timeout = 15000.0f)) |> awaitUnit
                fixture.ClickAsync() |> awaitUnit
                waitInt chartStack "data-loaded-bars" 900
                waitInt chartStack "data-visible-start" 701
                waitInt chartStack "data-visible-end" 900
                waitUntil
                    "coverage identity must settle"
                    (fun () -> attributeOf chartStack "data-coverage-identity")
                    ((=) "browser-demo:preview-release-parity")
                |> ignore
                waitUntil
                    "overview source must be populated"
                    (fun () -> intAttribute navigator "data-overview-source-count")
                    (fun count -> count > 0)
                |> ignore
                waitUntil
                    "navigator selection ratios must be non-empty"
                    (fun () -> floatAttribute navigator "data-selection-right-ratio" - floatAttribute navigator "data-selection-left-ratio")
                    (fun width -> width > 0.01f)
                |> ignore
                waitUntil
                    "navigator selection rect must have body width"
                    (fun () -> floatAttribute selection "width")
                    (fun width -> width > 48.0f)
                |> ignore

            let refreshDuringGesture () =
                fixture.EvaluateAsync("element => element.click()") |> awaitTask |> ignore
                waitInt chartStack "data-loaded-bars" 1000
                Thread.Sleep 100

            let selectionGeometry () =
                floatAttribute selection "x", floatAttribute selection "width"

            let stableSelectionGeometry () =
                let deadline = DateTime.UtcNow.AddSeconds 2.0
                let mutable previous = selectionGeometry ()
                Thread.Sleep 50
                let mutable current = selectionGeometry ()
                while
                    (not (approximately 0.25 (fst previous) (fst current)
                          && approximately 0.25 (snd previous) (snd current)))
                    && DateTime.UtcNow < deadline do
                    previous <- current
                    Thread.Sleep 50
                    current <- selectionGeometry ()
                require
                    (approximately 0.25 (fst previous) (fst current)
                     && approximately 0.25 (snd previous) (snd current))
                    $"navigator preview geometry did not settle: previous={previous}, current={current}"
                current

            let dragWithRerender targetTestId expectedMode firstDelta finalDelta =
                resetFixture ()
                let initial = selectionGeometry ()
                let targetLocator = navigator.Locator($"[data-testid='{targetTestId}']")
                waitUntil
                    "semantic hit target must have width"
                    (fun () ->
                        let bounds = targetLocator.BoundingBoxAsync() |> awaitTask
                        if isNull bounds then 0.0f else bounds.Width)
                    (fun width -> width > 4.0f)
                |> ignore
                let target = boundingBox targetLocator
                let startX = target.X + target.Width / 2.0f
                let y = target.Y + target.Height / 2.0f
                let hitInfo =
                    targetLocator
                        .EvaluateAsync<string>("element => { const bounds = element.getBoundingClientRect(); const hit = document.elementFromPoint(bounds.left + bounds.width / 2, bounds.top + bounds.height / 2); return JSON.stringify({ targetX: bounds.left, targetWidth: bounds.width, targetMode: element.getAttribute('data-window-drag-mode') || '', targetPointerEvents: getComputedStyle(element).pointerEvents, hitTag: hit ? hit.tagName : '', hitTestId: hit ? (hit.getAttribute('data-testid') || '') : '', hitMode: hit ? (hit.getAttribute('data-window-drag-mode') || '') : '' }); }")
                    |> awaitTask
                require (hitInfo.Contains($"\"hitMode\":\"{expectedMode}\"")) $"semantic target is covered: expected={expectedMode}, info={hitInfo}"
                page.Mouse.MoveAsync(startX, y) |> awaitUnit
                page.Mouse.DownAsync(MouseDownOptions(Button = MouseButton.Left)) |> awaitUnit
                waitUntil "semantic drag mode after pointerdown" (fun () -> attributeOf chartStack "data-drag-mode") ((=) expectedMode) |> ignore
                page.Mouse.MoveAsync(startX + firstDelta, y, MouseMoveOptions(Steps = 6)) |> awaitUnit
                waitUntil "preview must become visible" (fun () -> textOf range) (fun value -> value.Contains "Preview") |> ignore
                let beforeAuthoritativeFrame = stableSelectionGeometry ()
                refreshDuringGesture ()
                let afterAuthoritativeFrame = stableSelectionGeometry ()
                let actualModeAfterRerender = attributeOf chartStack "data-drag-mode"
                require (actualModeAfterRerender = expectedMode) $"drag mode changed after rerender: expected={expectedMode}, actual={actualModeAfterRerender}"
                require
                    (approximately 0.75 (fst beforeAuthoritativeFrame) (fst afterAuthoritativeFrame)
                     && approximately 0.75 (snd beforeAuthoritativeFrame) (snd afterAuthoritativeFrame))
                    $"authoritative frame changed active preview geometry: before={beforeAuthoritativeFrame}, after={afterAuthoritativeFrame}"
                page.Mouse.MoveAsync(startX + finalDelta, y, MouseMoveOptions(Steps = 6)) |> awaitUnit
                waitUntil "semantic drag mode after rerender move" (fun () -> attributeOf chartStack "data-drag-mode") ((=) expectedMode) |> ignore
                Thread.Sleep 100
                initial, selectionGeometry ()

            let (leftInitialX, leftInitialWidth), (leftDraftX, leftDraftWidth) =
                dragWithRerender "ta-overview-left-handle" "resize-left" 12.0f 28.0f
            require (leftDraftX > leftInitialX + 1.0f) "left handle did not move the left boundary"
            require (approximately 2.0 (leftDraftX + leftDraftWidth) (leftInitialX + leftInitialWidth)) "left handle changed the right boundary"
            page.Mouse.UpAsync(MouseUpOptions(Button = MouseButton.Left)) |> awaitUnit

            let (rightInitialX, rightInitialWidth), (rightDraftX, rightDraftWidth) =
                dragWithRerender "ta-overview-right-handle" "resize-right" -12.0f -28.0f
            require
                (approximately 2.0 rightDraftX rightInitialX)
                $"right handle changed the left boundary: initialX={rightInitialX}, draftX={rightDraftX}, initialWidth={rightInitialWidth}, draftWidth={rightDraftWidth}"
            require (rightDraftX + rightDraftWidth < rightInitialX + rightInitialWidth - 1.0f) "right handle did not move the right boundary"
            page.Mouse.UpAsync(MouseUpOptions(Button = MouseButton.Left)) |> awaitUnit

            let (moveInitialX, moveInitialWidth), (moveDraftX, moveDraftWidth) =
                dragWithRerender "ta-overview-move-hit" "move" -20.0f -40.0f
            require (moveDraftX < moveInitialX - 1.0f) "body drag did not move the selection"
            require (approximately 2.0 moveDraftWidth moveInitialWidth) "body drag changed the selection width"
            page.Mouse.UpAsync(MouseUpOptions(Button = MouseButton.Left)) |> awaitUnit

            let _, _ = dragWithRerender "ta-overview-move-hit" "move" -160.0f -420.0f
            let previewText = textOf range
            require (previewText.Contains "Preview") ("settled preview range is missing: " + previewText)
            let previewStart = parsePreviewStart previewText
            let actionCountBefore = intAttribute callbackState "data-callback-count"
            page.Mouse.UpAsync(MouseUpOptions(Button = MouseButton.Left)) |> awaitUnit

            let wireStartText = waitUntil "wire coverage target" (fun () -> attributeOf callbackState "data-last-coverage-start") (String.IsNullOrWhiteSpace >> not)
            let wireStart = Int64.Parse wireStartText
            let wireCount = intAttribute callbackState "data-last-coverage-count"
            waitUntil "exactly one body action" (fun () -> intAttribute callbackState "data-callback-count") ((=) (actionCountBefore + 1)) |> ignore
            require (wireStart = int64 (previewStart - 1)) $"preview/wire start mismatch: preview={previewStart}, wireOrdinal={wireStart}"
            require (wireCount = 200) $"wire count changed: {wireCount}"
            waitInt chartStack "data-visible-start" previewStart
            waitInt chartStack "data-visible-end" (previewStart + wireCount - 1)
            require (consoleErrors.Count = 0) ("browser console errors: " + String.concat " | " consoleErrors)

            let screenshotPath = Path.Combine(outputDirectory, "preview-release-parity.png")
            page.ScreenshotAsync(PageScreenshotOptions(Path = screenshotPath, FullPage = true)) |> awaitTask |> ignore
            printfn "navigator-preview-release-parity PASS preview=%d wireOrdinal=%d count=%d actionCount=%d screenshot=%s" previewStart wireStart wireCount (actionCountBefore + 1) screenshotPath
        finally
            browser.CloseAsync() |> awaitUnit
            playwright.Dispose()

runTestsWithCLIArgs [] [||] tests |> exit
