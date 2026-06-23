namespace PulseTrade.Comm.Spa.Dynamic.Server

open System
open Akka.Actor
open PulseTrade.Comm.Spa

[<AutoOpen>]
module CommHubExtensions =

    type CommHub with
        /// 將 Dynamic Sdui Actor 與路由掛載至現有的 CommHub
        member this.useDynamicSdui(actorSystem: ActorSystem) =
            // 註冊 "Actor Dynamic" 展示用的後端 Actor
            let props = Props.Create(fun () -> new ShowcaseDemoActor())
            let showcaseActorRef = actorSystem.ActorOf(props, "showcase-dynamic-actor")

            // 註冊 browser 可見的 append page shape。
            // 目前先不註冊 ScriptUrls；Dynamic browser bundle 必須由 WebSharper/F# 產生後再接入，禁止用手寫 JavaScript 字串補洞。
            let assembly = typeof<ShowcaseDemoActor>.Assembly
            let dllPath = assembly.Location
            let dir = System.IO.Path.GetDirectoryName(dllPath)
            
            // In a real nuget package, we'd ensure wwwroot is packaged.
            // For this POC in src5, it will be in bin/Debug/net10.0/wwwroot/js/
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

