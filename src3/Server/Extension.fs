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
            let assembly = System.Reflection.Assembly.GetExecutingAssembly()
            let getJsDataUri (name: string) =
                use stream = assembly.GetManifestResourceStream(name)
                if not (isNull stream) then
                    use ms = new System.IO.MemoryStream()
                    stream.CopyTo(ms)
                    "data:application/javascript;base64," + Convert.ToBase64String(ms.ToArray())
                else
                    ""

            let url1 = getJsDataUri "PulseTrade.Comm.Spa.Dynamic.Client.DynamicRenderer.js"
            let url2 = getJsDataUri "PulseTrade.Comm.Spa.Dynamic.Client.ActorDynamicTab.js"
            let scripts = [url1; url2] |> List.filter (System.String.IsNullOrWhiteSpace >> not)

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

            this // Return self for fluent API
