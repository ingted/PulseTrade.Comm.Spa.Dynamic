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
            
            // 目前 CommHub 核心設計（依據 UPSTREAM_RFC）將允許動態註冊 Actor 或 renderer
            // 在上游未開放註冊 API 之前，此處作為 dummy 掛載點，未來會透過 this.RegisterActor() 之類的方式綁定
            
            this // Return self for fluent API
