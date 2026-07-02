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
    let mutable latestSnapshotJson = ""

    let asText value =
        if isNull value then "" else string value

    let isBlank value =
        String.IsNullOrWhiteSpace(asText value)

    let sameText left right =
        (asText left).Trim().ToLower() = (asText right).Trim().ToLower()

    let arrayOrEmpty values =
        if isNull (box values) then [||] else values

    let decodeSnapshot snapshotJson =
        let source =
            if isBlank snapshotJson then latestSnapshotJson else snapshotJson

        if isBlank source then
            None
        else
            try
                Some(JSON.Parse(source) |> As<BrowserAclSnapshotDto>)
            with _ ->
                None

    let capabilityAllowed action (capabilities: BrowserAclCapabilityDto[]) =
        arrayOrEmpty capabilities
        |> Array.tryFind (fun item -> sameText item.action action)
        |> Option.map _.allowed

    let evaluateCapability (snapshot: BrowserAclSnapshotDto) action resourceKind resourceId =
        if isNull (box snapshot) || not snapshot.enabled then
            Some true
        else
            let resourceDecision =
                arrayOrEmpty snapshot.resources
                |> Array.tryFind (fun resource ->
                    sameText resource.resourceKind resourceKind
                    && sameText resource.resourceId resourceId)
                |> Option.bind (fun resource -> capabilityAllowed action resource.capabilities)

            resourceDecision
            |> Option.orElseWith (fun () -> capabilityAllowed action snapshot.globalCapabilities)

    let capabilityDecision snapshotJson action resourceKind resourceId =
        match decodeSnapshot snapshotJson |> Option.bind (fun snapshot -> evaluateCapability snapshot action resourceKind resourceId) with
        | Some true -> "allow"
        | Some false -> "deny"
        | None -> "unknown"

    let observeAclSnapshot (snapshotJson: string) =
        let length =
            if isNull snapshotJson then 0 else snapshotJson.Length

        latestSnapshotJson <- if isNull snapshotJson then "" else snapshotJson

        if not (isNull (box JS.Document.Body)) then
            JS.Document.Body.SetAttribute("data-ptcs-acl-snapshot-observed", "true")
            JS.Document.Body.SetAttribute("data-ptcs-acl-snapshot-observer", extensionId)
            JS.Document.Body.SetAttribute("data-ptcs-acl-snapshot-length", string length)

        true

    let registerAclSnapshotObserver () =
        if JS.In "PulseTradeRegisterAclSnapshotObserver" JS.Window then
            let observer = Func<string, bool>(fun snapshotJson -> observeAclSnapshot snapshotJson)
            JS.Inline("window.PulseTradeRegisterAclSnapshotObserver('ptcs-acl-snapshot-observer', 100, $0)", observer)

    let registerAclCapabilityProvider () =
        if JS.In "PulseTradeRegisterAclCapabilityProvider" JS.Window then
            let provider =
                Func<string, string, string, string, string>(fun action resourceKind resourceId snapshotJson ->
                    let decision = capabilityDecision snapshotJson action resourceKind resourceId

                    if not (isNull (box JS.Document.Body)) then
                        JS.Document.Body.SetAttribute("data-ptcs-acl-capability-provider", extensionId)
                        JS.Document.Body.SetAttribute("data-ptcs-acl-capability-action", asText action)
                        JS.Document.Body.SetAttribute("data-ptcs-acl-capability-resource-kind", asText resourceKind)
                        JS.Document.Body.SetAttribute("data-ptcs-acl-capability-resource-id", asText resourceId)
                        JS.Document.Body.SetAttribute("data-ptcs-acl-capability-decision", decision)

                    decision)

            JS.Inline("window.PulseTradeRegisterAclCapabilityProvider('ptcs-acl-capability-provider', 100, $0)", provider)

    [<SPAEntryPoint>]
    let Main () =
        registerAclSnapshotObserver ()
        registerAclCapabilityProvider ()
        JS.Global?console?log("PulseTrade.Comm.Spa.ACL bundle loaded and registered ACL snapshot observer/provider")

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
              MetadataJson = Some """{"kind":"ptcs-acl","package":"PulseTrade.Comm.Spa.ACL","version":"0.1.0-alpha10","clientCapabilityProvider":true}"""
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
