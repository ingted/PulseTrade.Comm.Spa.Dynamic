namespace PulseTrade.Comm.Spa.Dynamic.Client

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Html
open WebSharper.UI.Client

[<JavaScript>]
module ActorDynamicTab =
    
    /// renderActorDynamicPage
    let renderActorDynamicPage (pageId: string) =
        div [ attr.``class`` "actor-dynamic-container"; attr.style "padding: 16px;" ] [
            h2 [ attr.style "color: #333; margin-bottom: 16px;" ] [ text "Actor Dynamic" ]
            
            p [ attr.style "color: #666; margin-bottom: 24px;" ] [
                text "Actor Dynamic POC"
            ]
            
            div [ attr.``class`` "sdui-canvas-area"; attr.style "display: grid; grid-template-columns: 1fr 1fr; gap: 16px;" ] [
                div [ attr.style "border: 1px solid #ccc; border-radius: 8px; padding: 16px; background-color: #fff;" ] [
                    h3 [ attr.style "margin-top: 0;" ] [ text "Canvas" ]
                    div [ attr.id "sdui-canvas-mount"; attr.style "min-height: 300px; border: 1px dashed #aaa; display: flex; align-items: center; justify-content: center; color: #888;" ] [
                        text "Loading... (WebSocket fskynet-sdui Payload)"
                    ]
                ]
                
                div [ attr.style "border: 1px solid #ccc; border-radius: 8px; padding: 16px; background-color: #f9f9f9;" ] [
                    h3 [ attr.style "margin-top: 0;" ] [ text "PropertyGrid" ]
                    p [] [ text "Select element" ]
                ]
            ]
        ]

    [<JavaScriptExport>]
    let _registerRenderer () =
        let renderer (text: string) =
            try
                JS.Global?console?log("Inside fskynet-sdui renderer wrapper! Text length:", text.Length)
                let docOpt = DynamicRenderer.TryRender text
                match docOpt with
                | Some doc ->
                    JS.Global?console?log("Got Some doc! Creating container...")
                    let container = JS.Document.CreateElement("div")
                    WebSharper.UI.Client.Doc.Run container doc
                    JS.Global?console?log("Rendered doc to container!")
                    Some (container :> WebSharper.JavaScript.Dom.Node)
                | None ->
                    JS.Global?console?log("Got None from TryRender")
                    None
            with e ->
                JS.Global?console?error("Extension renderer threw an exception:", e)
                None

        JS.Inline("window.PulseTradeRegisterRenderer('fskynet-sdui', $0)", renderer)
        JS.Global?console?log("PulseTrade.Comm.Spa.Dynamic Client Extension Started and registered fskynet-sdui!")

    [<SPAEntryPoint>]
    let Main () =
        JS.Global?console?log("EVALUATING SPAEntryPoint Main in ActorDynamicTab!")
        _registerRenderer ()
        ArguFormRenderer.Register()
