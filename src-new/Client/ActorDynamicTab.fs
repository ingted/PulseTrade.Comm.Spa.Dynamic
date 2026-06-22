namespace PulseTrade.Comm.Spa.Dynamic.Client

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Html
open WebSharper.UI.Client

[<JavaScript>]
module ActorDynamicTab =
    
    /// 渲染 Actor Dynamic 專屬的 Tab 頁面內容
    let renderActorDynamicPage (pageId: string) =
        div [ attr.``class`` "actor-dynamic-container"; attr.style "padding: 16px;" ] [
            h2 [ attr.style "color: #333; margin-bottom: 16px;" ] [ text "Actor Dynamic 展示頁面" ]
            
            p [ attr.style "color: #666; margin-bottom: 24px;" ] [
                text "這是一個概念驗證用的展示頁面，展示多種 SDUI 元件的動態掛載與互動效果。"
            ]
            
            div [ attr.``class`` "sdui-canvas-area"; attr.style "display: grid; grid-template-columns: 1fr 1fr; gap: 16px;" ] [
                // 左側: 主要展示區
                div [ attr.style "border: 1px solid #ccc; border-radius: 8px; padding: 16px; background-color: #fff;" ] [
                    h3 [ attr.style "margin-top: 0;" ] [ text "Canvas 預覽區" ]
                    div [ attr.id "sdui-canvas-mount"; attr.style "min-height: 300px; border: 1px dashed #aaa; display: flex; align-items: center; justify-content: center; color: #888;" ] [
                        text "動態元件載入中... (等候 WebSocket 傳入 fskynet-sdui Payload)"
                    ]
                ]
                
                // 右側: 控制項與屬性
                div [ attr.style "border: 1px solid #ccc; border-radius: 8px; padding: 16px; background-color: #f9f9f9;" ] [
                    h3 [ attr.style "margin-top: 0;" ] [ text "元件屬性 (PropertyGrid)" ]
                    p [] [ text "選擇左側的元件以檢視與修改屬性" ]
                    // 此處可擴充 Tui-Chart 或其他常見元件控制
                ]
            ]
        ]

    /// 提供一個註冊點給宿主 PTCS (待 UPSTREAM_RFC 實裝)
    [<SPAEntryPoint>]
    let Start () =
        let renderer (text: string) =
            DynamicRenderer.TryRender text
            |> Option.map (fun doc ->
                let container = JS.Document.CreateElement("div")
                WebSharper.UI.Client.Doc.Run container doc
                container :> WebSharper.JavaScript.Dom.Node)

        // U SDUI Renderer ϱob@ Chat ]V fskynet-sdui
        PulseTrade.Comm.Spa.Client.RegisterRenderer("fskynet-sdui", renderer)
        
        // 註冊一個專屬的 AppendPageShape 讓下拉選單能直接出現這個獨立的 Tab Page
        PulseTrade.Comm.Spa.Client.RegisterAppendPageShape("actor-dynamic", "Actor Dynamic", "A", "actor-dynamic-page")
        
        JS.Window.Alert("PulseTrade.Comm.Spa.Dynamic Client Extension Started!")
