namespace PulseTrade.Comm.Spa.Dynamic.Ptcs.LiveDemo

open PulseTrade.Comm.Spa.Dynamic.Ptcs.Client
open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Html
open WebSharper.UI.Client

[<JavaScript>]
module Client =
    /// PTCS loads this bundle from its same-origin client-extension manifest.
    [<SPAEntryPoint>]
    let Main () =
        let shellId = "ta-ptcs-live-shell"
        let appId = "ta-ptcs-live-app"
        let mutable handle: TaResearchTransientClientHandle option = None

        match JS.Document.GetElementById shellId with
        | null ->
            let container = JS.Document.CreateElement("section")
            container.Id <- shellId

            match JS.Document.Body.FirstChild with
            | null -> JS.Document.Body.AppendChild container |> ignore
            | first -> JS.Document.Body.InsertBefore(container, first) |> ignore

            let shell =
                div [ attr.style "margin:12px; border:1px solid #ccd7e5; background:#fff; min-width:0; max-height:calc(100vh - 24px); overflow:auto;" ] [
                    div [ attr.style "display:flex; align-items:center; gap:6px; padding:6px 8px; border-bottom:1px solid #dce4ef; font-size:11px;" ] [
                        strong [ Attr.Create "data-testid" "ta-ptcs-live-marker" ] [ text "PTCS transient TA live" ]
                        button [ Attr.Create "data-testid" "ta-ptcs-deactivate"; on.click (fun _ _ -> handle |> Option.iter (fun value -> value.SetActive false)) ] [ text "Deactivate" ]
                        button [ Attr.Create "data-testid" "ta-ptcs-activate"; on.click (fun _ _ -> handle |> Option.iter (fun value -> value.SetActive true)) ] [ text "Activate" ]
                        button [ Attr.Create "data-testid" "ta-ptcs-dispose"; on.click (fun _ _ -> handle |> Option.iter (fun value -> value.Dispose())) ] [ text "Dispose" ]
                    ]
                    div [ attr.id appId ] []
                ]

            Doc.Run container shell

            let options =
                { TaClientLifecycle.defaults with
                    PollIntervalMs = 150
                    RequestTimeoutMs = 2000
                    PollRetryMs = 250
                    ReconnectBaseMs = 250
                    ReconnectMaximumMs = 2000 }

            handle <-
                Some(
                    TaResearchTransientClient.mountByIdWithOptions
                        appId
                        "ta-research"
                        "ta-live-main"
                        "ta-live-canvas"
                        options)
        | _ -> ()
