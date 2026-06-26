namespace PulseTrade.Comm.Spa.Dynamic.Server

open System
open Akka.Actor
open PulseTrade.Comm.Spa

[<AutoOpen>]
module CommHubExtensions =

    let mergeMetadata left right =
        { DynamicArguSchemas =
            Array.append left.DynamicArguSchemas right.DynamicArguSchemas
            |> Array.distinctBy _.DuTypeName
          DynamicFormDocuments =
            Array.append left.DynamicFormDocuments right.DynamicFormDocuments
            |> Array.distinctBy _.DocumentId }

    type CommHub with
        /// 將 Dynamic Sdui Actor、路由與 FormInput metadata 掛載至現有的 CommHub。
        /// Host 應在自己的 assembly 宣告 IArgParserTemplate DU，轉成 SduiFormDocument 後傳入；package 本身不內建 host-specific demo DU。
        member this.useDynamicSdui(actorSystem: ActorSystem, metadata: DynamicArguMetadata, registrations: DynamicArguTemplateRegistration seq) =
            let metadata =
                if isNull (box metadata) then
                    DynamicArguMetadata.empty
                else
                    metadata

            let registrations =
                if isNull (box registrations) then
                    [||]
                else
                    registrations |> Seq.toArray

            let metadata =
                registrations
                |> DynamicArguTemplateRegistration.metadata
                |> mergeMetadata metadata

            if registrations.Length > 0 then
                this.RegisterClientExtensionJsonPostHandler(
                    DynamicArguResolveEndpoint.path,
                    DynamicArguResolveEndpoint.handle registrations)
                |> ignore

            // 註冊 "Actor Dynamic" 展示用的後端 Actor
            let props = Props.Create(fun () -> new ShowcaseDemoActor())
            let showcaseActorRef = actorSystem.ActorOf(props, "showcase-dynamic-actor")

            // 註冊 browser 可見的 append page shape。
            // Dynamic browser bundle 必須由 WebSharper/F# 產生後再接入，禁止用手寫 JavaScript 字串補洞。
            let assembly = typeof<ShowcaseDemoActor>.Assembly
            let dllPath = assembly.Location
            let dir = System.IO.Path.GetDirectoryName(dllPath)

            let localJsDir = System.IO.Path.Combine(dir, "wwwroot", "js")
            let nugetJsDir = System.IO.Path.Combine(dir, "..", "..", "contentFiles", "any", "net10.0", "wwwroot", "js")
            let jsDir = if System.IO.Directory.Exists(localJsDir) then localJsDir else nugetJsDir

            let mutable scripts = []
            if System.IO.Directory.Exists(jsDir) then
                let allJsFiles = System.IO.Directory.GetFiles(jsDir, "*.js", System.IO.SearchOption.AllDirectories)
                for file in allJsFiles do
                    let relativePath = file.Substring(jsDir.Length).Replace("\\", "/")
                    let relativePath = if relativePath.StartsWith("/") then relativePath.Substring(1) else relativePath
                    let url = "/ext/js/" + relativePath
                    let rawContent = System.IO.File.ReadAllText(file)
                    let content = if file.EndsWith("head.js") then rawContent.Replace("document.write(\"\")", "// no-op") else rawContent
                    printfn "Asset %s length: %d" url content.Length
                    let asset: ClientExtensionScriptAsset = {
                        Url = url
                        ContentType = "application/javascript"
                        Content = content
                    }
                    this.RegisterClientExtensionScriptAsset asset |> ignore

                    if file.EndsWith("PulseTrade.Comm.Spa.Dynamic.head.js") then
                        scripts <- url :: scripts
                    else if file.EndsWith("PulseTrade.Comm.Spa.Dynamic.js") && not (file.EndsWith("min.js")) then
                        scripts <- scripts @ [ url ]

            printfn "Scripts list count before registering: %d" scripts.Length

            this.RegisterClientExtension
                { ExtensionId = "pulse-trade-comm-spa-dynamic"
                  DisplayName = Some "PulseTrade.Comm.Spa.Dynamic"
                  MetadataJson = Some(DynamicArguMetadata.generateJson metadata)
                  ScriptUrls = scripts
                  AppendPageShapes =
                    [ { Shape = "actor-dynamic"
                        Label = Some "Actor Dynamic"
                        Badge = Some "D"
                        ClassName = Some "actor-dynamic" } ] }
            |> ignore

            this.RegisterAppendPageShapeTemplate
                { Shape = "actor-dynamic"
                  Description = Some "Send an Argu-style command to a dynamic actor and render the reply through extension renderers."
                  KeyPlaceholder = Some "\"akka.tcp://PulseTradeCommSpaDynamicPoc@127.0.0.1:7705/user/showcase-dynamic-actor\""
                  ValuePlaceholder = Some "--render --topic canvas"
                  DefaultKey = Some "\"akka.tcp://PulseTradeCommSpaDynamicPoc@127.0.0.1:7705/user/showcase-dynamic-actor\""
                  Tags = [ "actor-argu"; "dynamic"; "custom-shape" ] }
            |> ignore

            this // Return self for fluent API

        /// 將 Dynamic Sdui Actor、路由與 FormInput metadata 掛載至現有的 CommHub。
        /// Host 應在自己的 assembly 宣告 IArgParserTemplate DU，轉成 SduiFormDocument 後傳入；package 本身不內建 host-specific demo DU。
        member this.useDynamicSdui(actorSystem: ActorSystem, metadata: DynamicArguMetadata) =
            this.useDynamicSdui(actorSystem, metadata, Seq.empty)

        /// 將 Dynamic Sdui Actor 與路由掛載至現有的 CommHub
        member this.useDynamicSdui(actorSystem: ActorSystem) =
            this.useDynamicSdui(actorSystem, DynamicArguMetadata.empty)
