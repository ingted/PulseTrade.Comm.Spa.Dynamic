namespace PulseTrade.Comm.Spa.Dynamic.Server

open System
open Akka.Actor
open PersistedConcurrentSortedList.Type
open FAkka.FCell2

/// 表示 ActorDynamic 展示頁面的狀態機或回應邏輯
type ShowcaseDemoActor() as this =
    inherit ReceiveActor()

    do
        this.Receive<string>(fun (msg: string) -> 
            if msg = "init" then
                // 生成一組展示用的 fCell2 AST，這將被轉換為 sdui
                // 包括 CanvasComponent, GridFeatures, AppLoader, AutoComplete 等概念元件
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
                let payload = FCell2Interop.toMessagePayload viewAst
                this.ActorCtx.Sender.Tell(payload, this.ActorCtx.Self)
        )

    /// 提供給 lambda 使用的 context
    member _.ActorCtx: IActorContext = ActorBase.Context
