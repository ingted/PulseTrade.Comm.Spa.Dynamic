namespace PulseTrade.Comm.Spa.Dynamic.Client

open PulseTrade.Comm.Spa
open WebSharper
open WebSharper.JavaScript
open WebSharper.JavaScript.Dom
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Html

/// Context-aware static SDUI reply adapter for PTCS message cards.
[<JavaScript; RequireQualifiedAccess>]
module ReplyPresentation =
    let extractReplyPayload (rawContent: string) =
        let value = if isNull rawContent then "" else rawContent.Trim()
        let marker = "replied msg:"
        let index = value.IndexOf(marker)
        if index >= 0 then value.Substring(index + marker.Length).Trim() else value

    let tryStaticCanvasPayload rawContent =
        let content = extractReplyPayload rawContent

        try
            let payload = JSON.Parse content
            let schema = DynamicRenderer.tryGet<string> "schema" payload |> Option.defaultValue ""
            let protocol = DynamicRenderer.tryGet<string> "protocol" payload |> Option.defaultValue ""
            let hasDocument =
                DynamicRenderer.tryGet<obj> "ui" payload |> Option.isSome
                || DynamicRenderer.tryGet<obj> "sdui" payload |> Option.isSome

            if schema = "fskynet-sdui" && protocol <> "sdui-runtime.v1" && hasDocument then Some(content, payload)
            else None
        with _ -> None

    let staticCanvasSummary (payload: obj) =
        let nodes =
            DynamicRenderer.tryGet<obj> "ui" payload
            |> Option.orElseWith (fun () -> DynamicRenderer.tryGet<obj> "sdui" payload)
            |> Option.map DynamicRenderer.unwrapFCell
            |> Option.map (fun value ->
                if JS.Global?Array?isArray(value) then As<obj array> value else [| value |])
            |> Option.defaultValue [||]

        let title =
            nodes
            |> Array.tryPick (fun node ->
                if DynamicRenderer.getText "type" "" node = "Heading" then
                    DynamicRenderer.tryGet<string> "text" node
                else
                    None)
            |> Option.defaultValue "SDUI Canvas"

        title, nodes.Length

    let clearHost (host: Element) =
        while not (isNull host.FirstChild) do
            host.RemoveChild(host.FirstChild) |> ignore

    let renderSummary title elementCount =
        let root = JS.Document.CreateElement("div")
        let summary =
            div [
                attr.style "min-width:0; display:grid; grid-template-columns:minmax(0,1fr) auto; gap:6px 12px; align-items:center;"
            ] [
                strong [ attr.style "min-width:0; color:#163f9f; overflow-wrap:anywhere;" ] [ text title ]
                span [ attr.style "color:#596579; font-size:12px; white-space:nowrap;" ] [ text $"{elementCount} element(s)" ]
                span [ attr.style "grid-column:1/-1; color:#4f5b6e; font-size:12px;" ] [ text "Static SDUI reply; expand inline or open near-fullscreen." ]
            ]

        Doc.Run root summary
        root :> Node

    let tryResolve (context: ReplyPresentationContext) =
        match tryStaticCanvasPayload context.Payload with
        | None -> None
        | Some(content, payload) ->
            let title, elementCount = staticCanvasSummary payload

            Some
                { Kind = "static-sdui"
                  RenderSummary = fun () -> renderSummary title elementCount
                  Actions = [||]
                  Mount =
                    fun _ host ->
                        clearHost host
                        Doc.Run host (DynamicRenderer.createSduiCanvasBody content)
                        fun () -> clearHost host }

    let register () =
        Client.RegisterReplyPresentation("dynamic-static-sdui-v2", tryResolve)
