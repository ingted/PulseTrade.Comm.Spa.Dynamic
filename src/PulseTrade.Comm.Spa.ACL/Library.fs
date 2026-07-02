namespace PulseTrade.Comm.Spa.ACL

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

    let create policyConfig =
        PtcsAcl.create policyConfig

    let withAuditSink sink acl =
        PtcsAcl.withAuditSink sink acl

    let useAcl (acl: PtcsAclOptions) (options: ServerOptions) =
        Server.withAcl acl options

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
