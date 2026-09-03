namespace PulseTrade.Comm.Spa.Dynamic.Interactive.Client

open PulseTrade.Comm.Spa.Dynamic.Contracts
open PulseTrade.Comm.Spa.Dynamic.Renderer
open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client

/// Interactive notebook iframe 的極小 browser client。
///
/// RuntimeFrame 的驗證、K 聚合與 TA 計算都在 kernel/backend；此 client 只負責
/// browser typed decode、pure reducer、renderer interaction 與 typed action 回送。
[<JavaScript>]
module Client =
    let webSocketUrl () =
        let scheme = if JS.Window.Location.Protocol = "https:" then "wss://" else "ws://"
        let framesPath = JS.Window.Location.Pathname.Replace("/view/", "/frames/")
        scheme + JS.Window.Location.Host + framesPath

    let setStatus text =
        match JS.Document.GetElementById "sdui-runtime-status" with
        | null -> ()
        | element -> element.TextContent <- text

    [<SPAEntryPoint>]
    let Main () =
        let socket = new WebSocket(webSocketUrl ())
        let mutable runtimeState: Var<RuntimeState> option = None
        let mutable mounted = false

        let send clientFrame =
            if socket.ReadyState = WebSocketReadyState.Open then
                socket.Send(BrowserRuntimeCodec.encodeClient clientFrame)

        let interpret effect =
            match effect with
            | RuntimeEffect.RequestResync(canvasId, _) ->
                send (RuntimeClientFrame.Action(SduiAction.RequestFullSnapshot(canvasId, "browser-reducer-resync")))
            | _ -> ()

        let mount (state: Var<RuntimeState>) =
            let callbacks =
                { SubmitAction =
                    fun action ->
                        async {
                            send (RuntimeClientFrame.Action action)
                            return Ok()
                        } }

            TaWorkspaceRenderer.render TaWorkspaceRenderer.defaultOptions callbacks state
            |> Doc.RunById "app"

        let applyFrame frame =
            match runtimeState with
            | None ->
                let identity =
                    { DocumentId = frame.DocumentId
                      CanvasInstanceId = frame.CanvasInstanceId }

                let next, effect = RuntimeReducer.reduce (RuntimeReducer.initial identity) frame
                let state = Var.Create next
                runtimeState <- Some state
                interpret effect

                match next.Document with
                | Some _ ->
                    mount state
                    mounted <- true
                    send (RuntimeClientFrame.Mounted frame.CanvasInstanceId)
                | None -> ()
            | Some state ->
                let next, effect = RuntimeReducer.reduce state.Value frame
                state.Value <- next
                interpret effect

                if not mounted && next.Document.IsSome then
                    mount state
                    mounted <- true
                    send (RuntimeClientFrame.Mounted frame.CanvasInstanceId)

        socket.OnOpen <- fun () -> setStatus "CONNECTED"

        socket.OnMessage <-
            fun event ->
                match BrowserRuntimeCodec.decode (string event.Data) with
                | Ok frame ->
                    applyFrame frame
                    setStatus "READY"
                | Error message -> setStatus ("FRAME ERROR: " + message)

        socket.OnError <- fun _ -> setStatus "CONNECTION ERROR"
        socket.OnClose <- fun _ -> setStatus "DISCONNECTED"

        JS.Window.AddEventListener(
            "beforeunload",
            fun (_: WebSharper.JavaScript.Dom.Event) ->
                runtimeState
                |> Option.iter (fun state -> send (RuntimeClientFrame.Unmounted state.Value.Identity.CanvasInstanceId))

                if socket.ReadyState = WebSocketReadyState.Open || socket.ReadyState = WebSocketReadyState.Connecting then
                    socket.Close())
