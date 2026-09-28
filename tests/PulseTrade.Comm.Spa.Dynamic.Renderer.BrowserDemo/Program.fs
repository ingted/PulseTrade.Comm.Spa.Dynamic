namespace PulseTrade.Comm.Spa.Dynamic.Renderer.BrowserDemo

open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection

module Program =
    [<EntryPoint>]
    let main args =
        let builder = WebApplication.CreateBuilder(args)
        builder.Services.AddRouting() |> ignore
        let app = builder.Build()
        app.Urls.Add("http://127.0.0.1:18882")
        app.UseDefaultFiles() |> ignore
        let staticFiles =
            StaticFileOptions(
                OnPrepareResponse = fun context ->
                    context.Context.Response.Headers.CacheControl <- "no-store, no-cache, must-revalidate"
                    context.Context.Response.Headers.Pragma <- "no-cache"
                    context.Context.Response.Headers.Expires <- "0")
        app.UseStaticFiles(staticFiles) |> ignore
        printfn "TA Renderer browser demo: http://127.0.0.1:18882/"
        app.Run()
        0
