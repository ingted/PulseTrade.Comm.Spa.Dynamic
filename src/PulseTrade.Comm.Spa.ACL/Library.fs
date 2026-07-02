namespace PulseTrade.Comm.Spa.ACL

open System
open System.IO
open System.Reflection
open System.Text
open PulseTrade.Comm.ACL.Core
open PulseTrade.Comm.Spa
open WebSharper
open WebSharper.JavaScript
open WebSharper.JavaScript.Dom

[<JavaScript>]
module ClientBundle =
    let extensionId = "pulse-trade-comm-spa-acl"

    let observeAclSnapshot (snapshotJson: string) =
        let length =
            if isNull snapshotJson then 0 else snapshotJson.Length

        if not (isNull (box JS.Document.Body)) then
            JS.Document.Body.SetAttribute("data-ptcs-acl-snapshot-observed", "true")
            JS.Document.Body.SetAttribute("data-ptcs-acl-snapshot-observer", extensionId)
            JS.Document.Body.SetAttribute("data-ptcs-acl-snapshot-length", string length)

        true

    let registerAclSnapshotObserver () =
        if JS.In "PulseTradeRegisterAclSnapshotObserver" JS.Window then
            let observer = Func<string, bool>(fun snapshotJson -> observeAclSnapshot snapshotJson)
            JS.Inline("window.PulseTradeRegisterAclSnapshotObserver('ptcs-acl-snapshot-observer', 100, $0)", observer)

    [<SPAEntryPoint>]
    let Main () =
        registerAclSnapshotObserver ()
        JS.Global?console?log("PulseTrade.Comm.Spa.ACL bundle loaded and registered ACL snapshot observer")

[<RequireQualifiedAccess>]
module PtcsAclExtension =
    let extensionId = "pulse-trade-comm-spa-acl"
    let scriptUrl = "/client-extensions/acl/PulseTrade.Comm.Spa.ACL.js"
    let scriptBaseUrl = "/client-extensions/acl/"
    let scriptFileName = "PulseTrade.Comm.Spa.ACL.js"
    let runtimeScriptRelativePath = "WebSharper.Core.JavaScript/Runtime.js"
    let runtimeScriptUrl = scriptBaseUrl + runtimeScriptRelativePath

    let assetFileCandidates (relativePath: string) =
        let assembly = Assembly.GetExecutingAssembly()
        let assemblyDir = assembly.Location |> Path.GetDirectoryName
        let relativeParts = relativePath.Split([| '/'; '\\' |], StringSplitOptions.RemoveEmptyEntries)
        let combine (root: string) =
            relativeParts |> Array.fold (fun path part -> Path.Combine(path, part)) root

        [ combine (Path.Combine(assemblyDir, "wwwroot", "js"))
          Path.GetFullPath(combine (Path.Combine(assemblyDir, "..", "..", "contentFiles", "any", "net10.0", "wwwroot", "js"))) ]

    let readScriptAsset (relativePath: string) =
        let candidates =
            assetFileCandidates relativePath

        candidates
        |> List.tryFind File.Exists
        |> function
            | Some path -> File.ReadAllText(path, Encoding.UTF8)
            | None ->
                let locations = String.Join("; ", candidates)
                invalidOp $"PTCS.ACL script asset {relativePath} not found. Tried: {locations}"

    let readScriptFile () =
        readScriptAsset scriptFileName

    let scriptContent () =
        readScriptFile ()

    let registerScriptAsset (url: string) (relativePath: string) (options: ServerOptions) =
        options.Hub.RegisterClientExtensionScriptAsset(
            { Url = url
              ContentType = "application/javascript"
              Content = readScriptAsset relativePath })
        |> ignore

    let registerClientBundle (options: ServerOptions) =
        options |> registerScriptAsset scriptUrl scriptFileName
        options |> registerScriptAsset runtimeScriptUrl runtimeScriptRelativePath

        options.Hub.RegisterClientExtension(
            { ExtensionId = extensionId
              DisplayName = Some "PTCS.ACL"
              MetadataJson = Some """{"kind":"ptcs-acl","package":"PulseTrade.Comm.Spa.ACL","version":"0.1.0-alpha8"}"""
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
