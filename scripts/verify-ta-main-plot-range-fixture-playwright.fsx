// DYN-VFY-069 revision 0.1. Isolated renderer UI fixture; NOT Research real-data acceptance.
// Client only: connect to an existing BrowserDemo host; never build/start/stop the host.
#i @"nuget: C:\Program Files\dotnet\sdk\10.0.402\FSharp\library-packs"
#r "nuget: FAkka.Argu, [10.1.301]"
#r "nuget: Expecto, 10.2.3"
#r "nuget: Microsoft.Playwright, 1.52.0"
#load "ParseLine.fsx"

open System
open System.IO
open System.Globalization
open System.Threading
open System.Threading.Tasks
open System.Text.Json
open Argu
open Expecto
open Microsoft.Playwright

type Arguments =
    | Url of string
    | Output_Dir of string
    | Browser_Executable_Path of string
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Url _ -> "Existing renderer fixture URL."
            | Output_Dir _ -> "Ignored fixture-only evidence directory."
            | Browser_Executable_Path _ -> "Local Chrome executable."

let defaultArgumentsText =
    "--url http://127.0.0.1:18882/ --output-dir temp/agent.asteer/main-chart-range --browser-executable-path \"C:/Program Files/Google/Chrome/Application/chrome.exe\""
let parser = ArgumentParser.Create<Arguments>(programName = "verify-ta-main-plot-range-fixture-playwright.fsx")
let defaults = parser.Parse(PL.parseLine [| ' ' |] (Some '"') None true defaultArgumentsText)
let automation = parser.Parse(fsi.CommandLineArgs |> Array.skip 1 |> Array.skipWhile ((=) "--"))
let url = automation.TryGetResult(<@ Url @>) |> Option.defaultWith (fun () -> defaults.GetResult(<@ Url @>))
let outputDirectory = automation.TryGetResult(<@ Output_Dir @>) |> Option.defaultWith (fun () -> defaults.GetResult(<@ Output_Dir @>)) |> Path.GetFullPath
let browserPath = automation.TryGetResult(<@ Browser_Executable_Path @>) |> Option.defaultWith (fun () -> defaults.GetResult(<@ Browser_Executable_Path @>))
let awaitTask (task: Task<'T>) = task.GetAwaiter().GetResult()
let awaitUnit (task: Task) = task.GetAwaiter().GetResult()
let attribute (locator: ILocator) name = locator.GetAttributeAsync(name) |> awaitTask |> Option.ofObj |> Option.defaultValue ""
let number locator name = Double.Parse(attribute locator name, CultureInfo.InvariantCulture)
let wait label predicate =
    let deadline = DateTime.UtcNow.AddSeconds 15.0
    while not (predicate ()) && DateTime.UtcNow < deadline do Thread.Sleep 50
    Expect.isTrue (predicate ()) label

let tests =
    testList "FIXTURE-ONLY main plot range" [
        testCase "both endpoints, reverse, cancellation and mode restore" <| fun _ ->
            Directory.CreateDirectory(outputDirectory) |> ignore
            use playwright = Playwright.CreateAsync() |> awaitTask
            let browser = playwright.Chromium.LaunchAsync(BrowserTypeLaunchOptions(Headless = true, ExecutablePath = browserPath)) |> awaitTask
            try
                let page = browser.NewPageAsync(BrowserNewPageOptions(ViewportSize = ViewportSize(Width = 1600, Height = 1400))) |> awaitTask
                let errors = ResizeArray<string>()
                page.Console.Add(fun message -> if message.Type = "error" then errors.Add(message.Text))
                page.GotoAsync(url, PageGotoOptions(WaitUntil = WaitUntilState.NetworkIdle)) |> awaitTask |> ignore
                page.Locator("[data-testid='ta-demo-global-coverage']").ClickAsync() |> awaitUnit
                let chart = page.Locator("[data-testid='ta-chart-stack']")
                let plot = page.Locator("[data-testid='ta-candle-price']")
                let navigator = page.Locator("[data-testid='ta-overview-navigator']")
                let preview = plot.Locator("[data-testid='ta-main-plot-range-preview']")
                let callback = page.Locator("[data-testid='ta-demo-callback-state']")
                let resident = page.Locator("[data-testid='ta-global-overview-with-axis']")
                wait "fixture ready" (fun () -> number plot "data-point-count" > 0.0)
                let residentStart = number resident "data-resident-start-observation-ordinal"
                let residentCount = number resident "data-resident-observation-count"
                let box () = plot.BoundingBoxAsync() |> awaitTask |> Option.ofObj |> Option.defaultWith (fun () -> failtest "plot has no geometry")
                let initial = box ()
                page.Mouse.ClickAsync(initial.X + initial.Width * 0.5f, initial.Y + initial.Height * 0.4f, MouseClickOptions(Button = MouseButton.Right)) |> awaitUnit
                wait "non-CURSOR mode" (fun () -> attribute plot "data-main-plot-interaction-mode" = "resize")

                for name, downRatio, upRatio in [ "forward", 0.25f, 0.65f; "reverse", 0.85f, 0.15f ] do
                    let beforeStart = number chart "data-local-visible-start"
                    let beforeCount = number chart "data-local-visible-count"
                    let beforeNavigatorLeft = number navigator "data-selection-left-ratio"
                    let beforeCalls = number callback "data-callback-count"
                    let bounds = box ()
                    let y = bounds.Y + bounds.Height * 0.4f
                    page.Mouse.MoveAsync(bounds.X + bounds.Width * downRatio, y) |> awaitUnit
                    page.Mouse.DownAsync() |> awaitUnit
                    page.Mouse.MoveAsync(bounds.X + bounds.Width * upRatio, y, MouseMoveOptions(Steps = 8)) |> awaitUnit
                    Expect.equal (number callback "data-callback-count") beforeCalls "pointermove must not submit"
                    wait "range preview is visible" (fun () -> attribute preview "data-preview-visible" = "true" && number preview "data-preview-width" > 0.0)
                    Expect.isLessThan (number preview "data-preview-start-x") (number preview "data-preview-end-x") "preview endpoints normalize"
                    page.Mouse.UpAsync() |> awaitUnit
                    wait "one range commit" (fun () -> number callback "data-callback-count" = beforeCalls + 1.0)
                    wait "preview cleared" (fun () -> attribute preview "data-preview-visible" = "false")
                    let expectedStart = beforeStart + Math.Round(beforeCount * float (min downRatio upRatio))
                    let expectedCount = Math.Round(beforeCount * float (abs (upRatio - downRatio)))
                    wait "framed range settled" (fun () ->
                        abs (number chart "data-local-visible-start" - expectedStart) <= 2.0
                        && abs (number chart "data-local-visible-count" - expectedCount) <= 2.0)
                    let actualStart = number chart "data-local-visible-start"
                    let actualCount = number chart "data-local-visible-count"
                    Expect.equal (attribute callback "data-last-action") "VisibleRangeChanged" "selection uses the existing range action"
                    Expect.equal (attribute callback "data-last-coverage-start") "" "selection must not relocate the resident"
                    Expect.equal (number resident "data-resident-start-observation-ordinal") residentStart "resident origin stays fixed"
                    Expect.equal (number resident "data-resident-observation-count") residentCount "resident count stays fixed"
                    let left = number navigator "data-selection-left-ratio"
                    let right = number navigator "data-selection-right-ratio"
                    let expectedNavigatorStart = beforeNavigatorLeft * residentCount + expectedStart - beforeStart
                    Expect.isLessThanOrEqual (abs (left * residentCount - expectedNavigatorStart)) 2.0 "navigator left matches the framed TA left"
                    Expect.isLessThanOrEqual (abs (right * residentCount - expectedNavigatorStart - expectedCount)) 2.0 "navigator right matches the framed TA right"
                    File.WriteAllText(Path.Combine(outputDirectory, name + ".json"), JsonSerializer.Serialize({| fixtureOnly = true; beforeStart = beforeStart; beforeCount = beforeCount; expectedStart = expectedStart; expectedCount = expectedCount; actualStart = actualStart; actualCount = actualCount; navigatorLeft = left; navigatorRight = right |}))
                    page.ScreenshotAsync(PageScreenshotOptions(Path = Path.Combine(outputDirectory, name + ".png"), FullPage = true)) |> awaitTask |> ignore

                let cancelCalls = number callback "data-callback-count"
                let cancelStart = number chart "data-local-visible-start"
                let cancelCount = number chart "data-local-visible-count"
                let cancelBox = box ()
                let cancelY = cancelBox.Y + cancelBox.Height * 0.4f
                page.Mouse.MoveAsync(cancelBox.X + cancelBox.Width * 0.2f, cancelY) |> awaitUnit
                page.Mouse.DownAsync() |> awaitUnit
                page.Mouse.MoveAsync(cancelBox.X + cancelBox.Width * 0.8f, cancelY, MouseMoveOptions(Steps = 4)) |> awaitUnit
                page.Mouse.ClickAsync(cancelBox.X + cancelBox.Width * 0.8f, cancelY, MouseClickOptions(Button = MouseButton.Right)) |> awaitUnit
                page.Mouse.UpAsync() |> awaitUnit
                wait "CURSOR restored after cancelling range" (fun () -> attribute plot "data-main-plot-interaction-mode" = "cursor")
                wait "cancel restores the committed range" (fun () -> number chart "data-local-visible-start" = cancelStart && number chart "data-local-visible-count" = cancelCount)
                Expect.equal (number callback "data-callback-count") cancelCalls "cancel must not submit"
                Expect.equal (attribute preview "data-preview-visible") "false" "cancel clears preview"
                Expect.isEmpty errors "fixture browser console errors"
                printfn "FIXTURE-ONLY PASS forward/reverse endpoints, navigator parity, cancel, mode restore; Research real-data gate remains separate."
            finally
                browser.CloseAsync() |> awaitUnit
    ]

runTestsWithCLIArgs [] [||] tests |> exit
