// Focused browser gate for authored TA row editor visibility.

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
    | Output_Path of string
    | Browser_Executable_Path of string
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Url _ -> "Existing TA renderer browser-demo URL."
            | Output_Path _ -> "Screenshot path."
            | Browser_Executable_Path _ -> "Chrome or Edge executable path."

let browserPaths =
    [ @"C:\Program Files\Google\Chrome\Application\chrome.exe"
      @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe"
      @"C:\Program Files\Microsoft\Edge\Application\msedge.exe"
      @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe" ]

let defaultBrowserPath = browserPaths |> List.tryFind File.Exists |> Option.defaultValue ""
let defaultArgumentsText =
    $"--url \"http://127.0.0.1:18882/\" --output-path \"artifacts/ta-edit-playwright.png\" --browser-executable-path \"{defaultBrowserPath.Replace('\\', '/')}\""

let parser = ArgumentParser.Create<CliArgs>(programName = "verify-ta-edit-playwright.fsx")
let defaultArguments = PL.parseLine [| ' ' |] (Some '"') None true defaultArgumentsText
let automationArguments = fsi.CommandLineArgs |> Array.skip 1 |> Array.skipWhile ((=) "--")
let defaults = parser.Parse defaultArguments
let automation = parser.Parse automationArguments
let pick tryAutomation tryDefault fallback =
    tryAutomation () |> Option.orElseWith tryDefault |> Option.defaultValue fallback

let url = pick (fun () -> automation.TryGetResult(<@ Url @>)) (fun () -> defaults.TryGetResult(<@ Url @>)) "http://127.0.0.1:18882/"
let outputPath = pick (fun () -> automation.TryGetResult(<@ Output_Path @>)) (fun () -> defaults.TryGetResult(<@ Output_Path @>)) "artifacts/ta-edit-playwright.png" |> Path.GetFullPath
let browserPath = pick (fun () -> automation.TryGetResult(<@ Browser_Executable_Path @>)) (fun () -> defaults.TryGetResult(<@ Browser_Executable_Path @>)) defaultBrowserPath
let awaitTask (task: Task<'T>) = task.GetAwaiter().GetResult()
let awaitUnit (task: Task) = task.GetAwaiter().GetResult()

Directory.CreateDirectory(Path.GetDirectoryName outputPath) |> ignore
use playwright = Playwright.CreateAsync() |> awaitTask
let launch = BrowserTypeLaunchOptions(Headless = true)
if not (String.IsNullOrWhiteSpace browserPath) && File.Exists browserPath then launch.ExecutablePath <- browserPath
use browser = playwright.Chromium.LaunchAsync(launch) |> awaitTask
use context = browser.NewContextAsync(BrowserNewContextOptions(ViewportSize = ViewportSize(Width = 1440, Height = 900))) |> awaitTask
let page = context.NewPageAsync() |> awaitTask
page.GotoAsync(url) |> awaitTask |> ignore
page.Locator("[data-testid='ta-chart-stack']").WaitForAsync(LocatorWaitForOptions(State = WaitForSelectorState.Visible, Timeout = 15000.0f)) |> awaitUnit

let edits = page.Locator("[data-testid^='ta-edit-row-']")
let count = edits.CountAsync() |> awaitTask
printfn "ta-edit count=%d" count
for index in 0 .. count - 1 do
    let edit = edits.Nth(index)
    let testId = edit.GetAttributeAsync("data-testid") |> awaitTask |> Option.ofObj |> Option.defaultValue ""
    let visible = edit.IsVisibleAsync() |> awaitTask
    let box = edit.BoundingBoxAsync() |> awaitTask
    if isNull box then printfn "ta-edit item=%s visible=%b geometry=none" testId visible
    else printfn "ta-edit item=%s visible=%b geometry=%.1f,%.1f %.1fx%.1f" testId visible box.X box.Y box.Width box.Height

page.ScreenshotAsync(PageScreenshotOptions(Path = outputPath, FullPage = true)) |> awaitTask |> ignore
if count <> 1 then failwith $"Expected exactly one authored Edit control, actual={count}."
let smaEdit = page.Locator("[data-testid='ta-edit-row-sma']")
if not (smaEdit.IsVisibleAsync() |> awaitTask) then failwith "SMA Edit control is not visible."
printfn "ta-edit PASS screenshot=%s" outputPath
