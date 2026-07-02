namespace PulseTrade.Comm.Spa.Login

open System
open System.IO
open System.Reflection
open System.Text
open PulseTrade.Comm.Login.Core
open PulseTrade.Comm.Spa
open WebSharper
open WebSharper.JavaScript

[<JavaScript>]
module ClientBundle =
    let extensionId = "pulse-trade-comm-spa-login"

    [<SPAEntryPoint>]
    let Main () =
        JS.Global?console?log("PulseTrade.Comm.Spa.Login bundle loaded")

[<RequireQualifiedAccess>]
module PtcsLoginExtension =
    let extensionId = ClientBundle.extensionId
    let scriptUrl = "/client-extensions/login/PulseTrade.Comm.Spa.Login.js"
    let scriptFileName = "PulseTrade.Comm.Spa.Login.js"

    let readScriptFile () =
        let assembly = Assembly.GetExecutingAssembly()
        let assemblyDir = assembly.Location |> Path.GetDirectoryName

        let candidates =
            [ Path.Combine(assemblyDir, "wwwroot", "js", scriptFileName)
              Path.GetFullPath(Path.Combine(assemblyDir, "..", "..", "contentFiles", "any", "net10.0", "wwwroot", "js", scriptFileName)) ]

        candidates
        |> List.tryFind File.Exists
        |> function
            | Some path -> File.ReadAllText(path, Encoding.UTF8)
            | None ->
                let locations = String.Join("; ", candidates)
                invalidOp $"PTCS.Login script asset not found. Tried: {locations}"

    let scriptContent () =
        readScriptFile ()

    let registerClientBundle (options: ServerOptions) =
        options.Hub.RegisterClientExtensionScriptAsset(
            { Url = scriptUrl
              ContentType = "text/javascript"
              Content = scriptContent () })
        |> ignore

        options.Hub.RegisterClientExtension(
            { ExtensionId = extensionId
              DisplayName = Some "PTCS.Login"
              MetadataJson = Some """{"kind":"ptcs-login","package":"PulseTrade.Comm.Spa.Login","version":"0.1.0-alpha3"}"""
              ScriptUrls = [ scriptUrl ]
              AppendPageShapes = [] })
        |> ignore

        options

    let usePtcsLogin (login: PtcsLoginOptions) (options: ServerOptions) =
        options
        |> registerClientBundle
        |> Server.withPtcsLogin login

    let provider login =
        PtcsLogin.provider login

    let fromLoginCore core =
        PtcsLogin.fromLoginCore core

    let coreFromConfigWithDependencies dependencies config =
        PtcsLogin.coreFromConfigWithDependencies dependencies config

    let localDevDependenciesWithSessionStore sessionStore =
        PtcsLogin.localDevDependenciesWithSessionStore sessionStore

    let demoLocalDevWithSessionStore sessionStore =
        PtcsLogin.demoLocalDevWithSessionStore sessionStore

    let demoLocalDev () =
        PtcsLogin.demoLocalDev ()

[<AutoOpen>]
module ServerOptionsLoginExtensions =
    type ServerOptions with
        member this.usePtcsLogin(login: PtcsLoginOptions) =
            PtcsLoginExtension.usePtcsLogin login this
