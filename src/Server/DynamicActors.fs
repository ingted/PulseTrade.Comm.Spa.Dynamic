namespace PulseTrade.Comm.Spa.Dynamic.Server

open System
open System.Text.Json
open Akka.Actor
open PersistedConcurrentSortedList.Type
open FAkka.FCell2
open PulseTrade.Comm.Spa

module ActorDynamicPayload =
    let isFskynetSduiPayload (text: string) =
        if String.IsNullOrWhiteSpace text then
            false
        else
            try
                use document = JsonDocument.Parse(text)

                if document.RootElement.ValueKind <> JsonValueKind.Object then
                    false
                else
                    let mutable schema = Unchecked.defaultof<JsonElement>

                    document.RootElement.TryGetProperty("schema", &schema)
                    && schema.ValueKind = JsonValueKind.String
                    && String.Equals(schema.GetString(), "fskynet-sdui", StringComparison.OrdinalIgnoreCase)
            with _ ->
                false

    let simpleShowcase () =
        let viewAst =
            fCell2<string>.A [|
                fCell2<string>.T (Map [
                    ("type", fCell2.S "Heading");
                    ("id", fCell2.S "demo-canvas");
                    ("text", fCell2.S "PulseTrade Actor Dynamic Dashboard")
                ])
                fCell2<string>.T (Map [
                    ("type", fCell2.S "DataGrid");
                    ("id", fCell2.S "demo-grid");
                    ("dataRef", fCell2.S "gridData")
                ])
                fCell2<string>.T (Map [
                    ("type", fCell2.S "AppLoader");
                    ("text", fCell2.S "Loaded")
                ])
                fCell2<string>.T (Map [
                    ("type", fCell2.S "ColorPicker");
                    ("defaultColor", fCell2.S "#ff0000")
                ])
            |]

        FCell2Interop.toMessagePayload viewAst

    let complexShowcase () =
        """{"schema":"fskynet-sdui","data":{"marqueeData":["PTCS.Dynamic beta showcase2 online","Actor Dynamic direct route","Canvas DSL ready"],"orderData":[{"symbol":"PTC","side":"Bid","price":"101.25","size":"12"},{"symbol":"PTCS","side":"Ask","price":"102.10","size":"8"},{"symbol":"RN","side":"Bid","price":"99.80","size":"21"}],"treeData":["PTCS Host","GW Host","RN Host"]},"sdui":[{"text":"PulseTrade Actor Dynamic Showcase 2","type":"Heading"},{"dataRef":"marqueeData","direction":"left","type":"Rolling"},{"type":"Divider"},{"type":"Row","children":[{"text":"Control strip:","type":"Label"},{"id":"sort-drp","options":["Price ascending","Price descending","Size descending"],"type":"Dropdown"},{"id":"multi-select","multiple":true,"options":["PTCS","GW","RN"],"type":"SelectBox"},{"defaultColor":"#5bc0de","type":"ColorPicker"},{"type":"DatePicker"},{"type":"TimePicker"}]},{"id":"order-book","dataRef":"orderData","features":{"AllowAggregation":true,"AllowSorting":true,"Pagination":false},"type":"DataGrid"},{"type":"Pagination"},{"type":"Column","children":[{"text":"Command panel:","type":"Label"},{"id":"symbol-search","type":"AutoComplete"},{"id":"txt-amount","placeholder":"Input quantity...","type":"TextInput"},{"id":"btn-buy","text":"Send command","onClick":{"action":"SendCommand","command":"place_order","includeStateOf":["order-book.selectedRow","txt-amount"]},"type":"Button"},{"text":"Loading Data...","type":"AppLoader"},{"dataRef":"treeData","type":"Tree"},{"id":"context-menu-1","menuItems":[{"text":"Inspect"},{"text":"Route diagnostics"}],"type":"ContextMenu"}]}]}"""

    let reply direction tags payload =
        { Value = fCell2.S payload
          Direction = direction
          Tags = tags }

/// 展示 ActorDynamic 動態渲染的範例 Actor
type ShowcaseDemoActor() as this =
    inherit ReceiveActor()

    do
        this.Receive<string>(fun (msg: string) -> 
            if msg = "init" then
                this.ActorCtx.Sender.Tell(ActorDynamicPayload.simpleShowcase (), this.ActorCtx.Self)
        )

        this.Receive<ActorArguTargetCommand>(fun (command: ActorArguTargetCommand) ->
            let payload =
                if ActorDynamicPayload.isFskynetSduiPayload command.RawArgu then
                    command.RawArgu
                else
                    ActorDynamicPayload.simpleShowcase ()
            
            let reply: ActorArguTargetReply =
                ActorDynamicPayload.reply
                    (Some "inbound-message")
                    (Some [ "dynamic"; "sdui"; "showcase" ])
                    payload
                  
            this.ActorCtx.Sender.Tell(reply, this.ActorCtx.Self)
        )

        this.Receive<fCell2<string>>(fun cell ->
            let payload =
                match cell with
                | fCell2.S text when ActorDynamicPayload.isFskynetSduiPayload text -> text
                | _ -> ActorDynamicPayload.simpleShowcase ()

            let reply: fCell2<string> = fCell2.S payload
            this.ActorCtx.Sender.Tell(reply, this.ActorCtx.Self))

    /// 提供給 lambda 使用的 context
    member _.ActorCtx: IActorContext = ActorBase.Context

/// Actor Dynamic direct echo actor：RawArgu 是 SDUI JSON 時會原樣回給 Canvas renderer。
type SduiEchoActor() as this =
    inherit ReceiveActor()

    do
        this.Receive<ActorArguTargetCommand>(fun (command: ActorArguTargetCommand) ->
            let reply: ActorArguTargetReply =
                ActorDynamicPayload.reply
                    (Some "inbound-message")
                    (Some [ "dynamic"; "sdui"; "echo" ])
                    command.RawArgu

            this.ActorCtx.Sender.Tell(reply, this.ActorCtx.Self))

        this.Receive<string>(fun (text: string) ->
            this.ActorCtx.Sender.Tell(text, this.ActorCtx.Self))

        this.Receive<fCell2<string>>(fun cell ->
            this.ActorCtx.Sender.Tell(cell, this.ActorCtx.Self))

    member _.ActorCtx: IActorContext = ActorBase.Context

/// 較完整的 Actor Dynamic showcase，固定回傳包含 data/sdui 的 Canvas DSL。
type ShowcaseDemoActor2() as this =
    inherit ReceiveActor()

    do
        this.Receive<ActorArguTargetCommand>(fun (_: ActorArguTargetCommand) ->
            let reply: ActorArguTargetReply =
                ActorDynamicPayload.reply
                    (Some "inbound-message")
                    (Some [ "dynamic"; "sdui"; "showcase2" ])
                    (ActorDynamicPayload.complexShowcase ())

            this.ActorCtx.Sender.Tell(reply, this.ActorCtx.Self))

        this.Receive<string>(fun (msg: string) ->
            if msg = "init" then
                this.ActorCtx.Sender.Tell(ActorDynamicPayload.complexShowcase (), this.ActorCtx.Self))

        this.Receive<fCell2<string>>(fun _ ->
            let reply: fCell2<string> = fCell2.S (ActorDynamicPayload.complexShowcase ())
            this.ActorCtx.Sender.Tell(reply, this.ActorCtx.Self))

    member _.ActorCtx: IActorContext = ActorBase.Context
