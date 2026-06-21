namespace PulseTrade.Comm.Spa.Dynamic.Client

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Html
open WebSharper.UI.Client

[<JavaScript>]
module DynamicRenderer =

    /// 嘗試從 JSON 中讀取 schema 屬性
    let private tryGetSchema (jsonStr: string) =
        try
            if IsClient then
                let obj = JS.Global?JSON?parse(jsonStr)
                if JS.In "schema" obj then
                    Some (obj?schema : string)
                else
                    None
            else
                // .NET mock implementation for Expecto tests
                if jsonStr.Contains("\"schema\":\"fskynet-sdui\"") then
                    Some "fskynet-sdui"
                else
                    None
        with _ ->
            None

    /// SDUI Summary Card 渲染邏輯
    let createSduiSummaryCard (jsonStr: string) =
        // 解析並展示 Canvas 按鈕與 JSON String
        let isExpanded = Var.Create false
        
        let toggleExpand () =
            isExpanded.Value <- not isExpanded.Value
            
        div [ attr.style "border: 1px solid #ccc; padding: 8px; border-radius: 4px; margin: 4px 0;" ] [
            div [ attr.style "display: flex; justify-content: space-between; align-items: center;" ] [
                span [ attr.style "font-weight: bold; color: #007bff;" ] [ text "FSkynet SDUI Component" ]
                button [ 
                    attr.style "background-color: #f0f0f0; border: 1px solid #aaa; border-radius: 4px; cursor: pointer;"
                    on.click (fun _ _ -> toggleExpand ()) 
                ] [ 
                    textView (isExpanded.View |> View.Map (fun e -> if e then "Collapse JSON" else "View JSON")) 
                ]
                button [
                    attr.style "background-color: #28a745; color: white; border: none; border-radius: 4px; cursor: pointer; padding: 2px 8px; margin-left: 8px;"
                    on.click (fun _ _ -> 
                        if IsClient then JS.Window.Alert "SDUI Canvas Popup: Not fully implemented in POC")
                ] [ text "Open Canvas" ]
            ]
            
            // JSON 預覽區域 (動態顯示)
            isExpanded.View 
            |> View.Map (fun e -> 
                if e then
                    div [ 
                        attr.style "margin-top: 8px; background-color: #f8f9fa; padding: 8px; border-radius: 4px; overflow-x: auto; white-space: pre-wrap; font-family: monospace; font-size: 0.9em;"
                    ] [
                        text jsonStr
                    ]
                else
                    Doc.Empty
            )
            |> Doc.EmbedView
        ]

    /// 攔截器：若內容符合 schema = fskynet-sdui，則回傳 Some Doc，否則回傳 None
    let TryRender (content: string) : option<Doc> =
        match tryGetSchema content with
        | Some "fskynet-sdui" ->
            if IsClient then
                Some (createSduiSummaryCard content)
            else
                Some Doc.Empty // 在 .NET 測試環境下直接回傳 Dummy，避免呼叫 Client-Side 限定的 DOM 事件
        | _ ->
            None
