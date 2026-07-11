#r "nuget: Microsoft.Playwright, 1.44.0"
open Microsoft.Playwright
open System
open System.Threading.Tasks

let run () = task {
    use! playwright = Playwright.CreateAsync()
    let! browser = playwright.Chromium.LaunchAsync(BrowserTypeLaunchOptions(Headless = true))
    let! context = browser.NewContextAsync()
    let! page = context.NewPageAsync()

    printfn "Navigating to PTCS..."
    let! _ = page.GotoAsync("http://10.28.112.109:82/page/actor-dynamic-dd")
    
    // Wait for the Actions dropdown
    printfn "Waiting for Actions dropdown..."
    let! _ = page.WaitForSelectorAsync("select")
    
    // Select Add actor key
    printfn "Selecting Add actor key..."
    let! _ = page.SelectOptionAsync("select", [| "add-actor-key" |])
    do! Task.Delay(1000)
    let! _ = page.ScreenshotAsync(PageScreenshotOptions(Path = "C:/Users/Administrator/test_gemini/add_actor_key.png"))
    
    // Select Add target key
    printfn "Selecting Add target key..."
    let! _ = page.SelectOptionAsync("select", [| "add-target-key" |])
    do! Task.Delay(1000)
    let! _ = page.ScreenshotAsync(PageScreenshotOptions(Path = "C:/Users/Administrator/test_gemini/add_target_key.png"))
    
    // Select Add proxy key
    printfn "Selecting Add proxy key..."
    let! _ = page.SelectOptionAsync("select", [| "add-proxy-key" |])
    do! Task.Delay(1000)
    let! _ = page.ScreenshotAsync(PageScreenshotOptions(Path = "C:/Users/Administrator/test_gemini/add_proxy_key.png"))
    
    printfn "Screenshots saved."
    do! browser.CloseAsync()
}

run().Wait()
