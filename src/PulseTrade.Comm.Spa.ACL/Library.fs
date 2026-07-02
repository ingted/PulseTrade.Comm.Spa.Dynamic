namespace PulseTrade.Comm.Spa.ACL

open System
open System.IO
open System.Reflection
open System.Text
open PulseTrade.Comm.ACL.Core
open PulseTrade.Comm.Spa
open WebSharper
open WebSharper.JavaScript

[<JavaScript>]
module ClientBundle =
    let extensionId = "pulse-trade-comm-spa-acl"

    [<SPAEntryPoint>]
    let Main () =
        JS.Global?console?log("PulseTrade.Comm.Spa.ACL bundle loaded")

[<RequireQualifiedAccess>]
module PtcsAclExtension =
    let extensionId = ClientBundle.extensionId
    let scriptUrl = "/client-extensions/acl/PulseTrade.Comm.Spa.ACL.js"
    let scriptFileName = "PulseTrade.Comm.Spa.ACL.js"

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
                invalidOp $"PTCS.ACL script asset not found. Tried: {locations}"

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
              DisplayName = Some "PTCS.ACL"
              MetadataJson = Some """{"kind":"ptcs-acl","package":"PulseTrade.Comm.Spa.ACL","version":"0.1.0-alpha3"}"""
              ScriptUrls = [ scriptUrl ]
              AppendPageShapes = [] })
        |> ignore

        options

    let create policyConfig =
        PtcsAcl.create policyConfig

    let withAuditSink sink acl =
        PtcsAcl.withAuditSink sink acl

    let useAcl (acl: PtcsAclOptions) (options: ServerOptions) =
        options
        |> registerClientBundle
        |> Server.withAcl acl

    let currentSnapshot (acl: PtcsAclOptions) =
        PtcsAcl.currentSnapshot acl

    let currentRevision (acl: PtcsAclOptions) =
        PtcsAcl.currentRevision acl

    let reloadSnapshot (acl: PtcsAclOptions) policyConfig =
        PtcsAcl.reloadSnapshot acl policyConfig

    let evaluate (acl: PtcsAclOptions) principal actionKey resource =
        PtcsAcl.evaluate acl principal actionKey resource

    let routeManifest revision =
        PtcsAcl.routeManifest revision

    let clientSnapshot acl user resources =
        PtcsAcl.clientSnapshot acl user resources

[<RequireQualifiedAccess>]
module PtcsAclActions =
    let pageCreate = PtcsAcl.actionPageCreate
    let pageRemove = PtcsAcl.actionPageRemove
    let targetAdd = PtcsAcl.actionTargetAdd
    let targetRemove = PtcsAcl.actionTargetRemove
    let appendWrite = PtcsAcl.actionAppendWrite
    let actorArguSend = PtcsAcl.actionActorArguSend
    let actorRegister = PtcsAcl.actionActorRegister
    let actorReport = PtcsAcl.actionActorReport
    let extensionPost = PtcsAcl.actionExtensionPost
    let aclPolicyReload = PtcsAcl.actionAclPolicyReload

[<AutoOpen>]
module ServerOptionsAclExtensions =
    type ServerOptions with
        member this.useAcl(acl: PtcsAclOptions) =
            PtcsAclExtension.useAcl acl this
