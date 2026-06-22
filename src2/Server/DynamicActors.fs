namespace PulseTrade.Comm.Spa.Dynamic.Server

open System
open Akka.Actor
open PersistedConcurrentSortedList.Type
open FAkka.FCell2
open PulseTrade.Comm.Spa

/// 展示 ActorDynamic 動態渲染的範例 Actor
type ShowcaseDemoActor() as this =
    inherit ReceiveActor()

    let buildSduiPayload () =
        let viewAst =
            fCell2<string>.A [|
                fCell2<string>.T (Map [
                    "component", fCell2.S "CanvasComponent"
                    "id", fCell2.S "demo-canvas"
                    "title", fCell2.S "PulseTrade Actor Dynamic Dashboard"
                ])
                fCell2<string>.T (Map [
                    "component", fCell2.S "GridFeatures"
                    "id", fCell2.S "demo-grid"
                    "theme", fCell2.S "dark"
                ])
                fCell2<string>.T (Map [
                    "component", fCell2.S "AppLoader"
                    "status", fCell2.S "loaded"
                ])
                fCell2<string>.T (Map [
                    "component", fCell2.S "ColorPicker"
                    "default", fCell2.S "#ff0000"
                ])
            |]
        FCell2Interop.toMessagePayload viewAst

    do
        this.Receive<string>(fun (msg: string) -> 
            if msg = "init" then
                this.ActorCtx.Sender.Tell(buildSduiPayload (), this.ActorCtx.Self)
        )

        this.Receive<ActorArguTargetCommand>(fun (command: ActorArguTargetCommand) ->
            let payload = buildSduiPayload ()
            
            let reply: ActorArguTargetReply =
                { Value = fCell2.S payload
                  Direction = Some "inbound-message"
                  Tags = Some [ "dynamic"; "sdui"; "showcase" ] }
                  
            this.ActorCtx.Sender.Tell(reply, this.ActorCtx.Self)
        )

    /// 提供給 lambda 使用的 context
    member _.ActorCtx: IActorContext = ActorBase.Context
