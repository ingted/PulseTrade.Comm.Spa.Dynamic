namespace PulseTrade.Comm.Spa.Login

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

    let usePtcsLogin (login: PtcsLoginOptions) (options: ServerOptions) =
        Server.withPtcsLogin login options

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
