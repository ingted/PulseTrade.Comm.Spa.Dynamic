namespace PulseTrade.Comm.Spa.Dynamic.Client

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Html
open WebSharper.UI.Client

[<JavaScript>]
module DynamicRenderer =

    let E name attrs children =
        Doc.Element name attrs children
    let V name attrs =
        Doc.Element name attrs []

    let private tryGetSchema (jsonStr: string) =
        try
            if IsClient then
                let obj = JS.Global?JSON?parse(jsonStr)
                if JS.In "schema" obj then Some (obj?schema : string) else None
            else
                if jsonStr.Contains("\"schema\":\"fskynet-sdui\"") then Some "fskynet-sdui" else None
        with _ -> None

    let rec private renderNode (obj: obj) : Doc =
        if JS.TypeOf obj = JS.Kind.Undefined || obj = null then Doc.Empty
        else
            let t = JS.Inline<string>("$0.type", obj)
            match t with
            | "Heading" ->
                let textStr = JS.Inline<string>("$0.text || ''", obj)
                E "h2" [ attr.style "color: #5bc0de; margin-bottom: 15px;" ] [ text textStr ]
            | "Label" ->
                let textStr = JS.Inline<string>("$0.text || ''", obj)
                E "span" [ attr.style "margin-right: 10px; color: #ccc;" ] [ text textStr ]
            | "TextInput" ->
                let placeholderStr = JS.Inline<string>("$0.placeholder || ''", obj)
                let idStr = JS.Inline<string>("$0.id || ''", obj)
                let attrs = 
                    [ attr.``type`` "text"; attr.placeholder placeholderStr; attr.style "padding: 8px; background: #333; color: white; border: 1px solid #555; border-radius: 4px; display: block; width: 100%; box-sizing: border-box; margin: 5px 0;" ]
                    @ (if not (System.String.IsNullOrEmpty idStr) then [ on.afterRender (fun el -> el.SetAttribute("id", idStr)) ] else [])
                V "input" attrs
            | "Row" ->
                let childrenObj = JS.Inline<obj[]>("$0.children || []", obj)
                let childrenDocs = childrenObj |> Array.map renderNode |> Array.toList
                E "div" [ attr.style "display: flex; flex-direction: row; gap: 15px; margin-bottom: 10px; align-items: center;" ] childrenDocs
            | "Column" ->
                let childrenObj = JS.Inline<obj[]>("$0.children || []", obj)
                let childrenDocs = childrenObj |> Array.map renderNode |> Array.toList
                E "div" [ attr.style "display: flex; flex-direction: column; gap: 10px;" ] childrenDocs
            | "Divider" ->
                V "hr" [ attr.style "border: 0; border-top: 1px solid #444; margin: 15px 0; width: 100%;" ]
            | "Dropdown" | "SelectBox" ->
                let isMultiple = JS.Inline<bool>("!!$0.multiple", obj)
                let optionsArr = JS.Inline<string[]>("$0.options || []", obj)
                let optionDocs = optionsArr |> Array.map (fun opt -> E "option" [] [ text opt ]) |> Array.toList
                let attrs = 
                    [ attr.style "padding: 8px; margin: 5px 0; background: #333; color: white; border: 1px solid #555; border-radius: 4px; font-size: 1rem; display: block; width: 200px;" ]
                    @ (if isMultiple then [ on.afterRender (fun el -> el.SetAttribute("multiple", "multiple")) ] else [])
                E "select" attrs optionDocs
            | "DataGrid" ->
                let gridContainer = E "div" [ attr.style "background: #1e1e1e; border-radius: 8px; overflow: hidden; border: 1px solid #444; margin: 20px 0;" ] [ text "DataGrid rendered (Requires DataRef bindings)" ]
                gridContainer
            | "Button" ->
                let btnText = JS.Inline<string>("$0.text || 'Button'", obj)
                E "button" [
                    attr.``class`` "btn btn-success canvas-btn"
                    attr.style "margin-top: 15px; padding: 10px 20px; font-weight: bold; background: #5cb85c; color: white; border: none; border-radius: 4px; cursor: pointer; font-size: 1rem;"
                    on.click (fun _ _ -> JS.Window.Alert("Dispatcher: Sending command..."))
                ] [ text btnText ]
            | "AppLoader" ->
                let textStr = JS.Inline<string>("$0.text || 'Loading...'", obj)
                E "div" [ attr.style "display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 20px; color: #5bc0de;" ] [
                    V "div" [ attr.style "border: 4px solid rgba(255,255,255,0.1); border-top: 4px solid #5bc0de; border-radius: 50%; width: 40px; height: 40px; animation: spin 1s linear infinite;" ]
                    E "span" [ attr.style "margin-top: 10px;" ] [ text textStr ]
                ]
            | "ColorPicker" ->
                let defaultColor = JS.Inline<string>("$0.defaultColor || '#000000'", obj)
                let idStr = JS.Inline<string>("$0.id || ''", obj)
                let attrs =
                    [ attr.``type`` "color"; attr.value defaultColor; attr.style "padding: 0; margin: 5px 0; background: none; border: 1px solid #555; border-radius: 4px; cursor: pointer; height: 40px; width: 60px;" ]
                    @ (if not (System.String.IsNullOrEmpty idStr) then [ on.afterRender (fun el -> el.SetAttribute("id", idStr)) ] else [])
                V "input" attrs
            | "DatePicker" ->
                let picker = V "input" [ attr.``type`` "date"; attr.style "padding: 8px; margin: 5px 0; background: #333; color: white; border: 1px solid #555; border-radius: 4px; display: inline-block;" ]
                picker
            | "TimePicker" ->
                let picker = V "input" [ attr.``type`` "time"; attr.style "padding: 8px; margin: 5px 0; background: #333; color: white; border: 1px solid #555; border-radius: 4px; display: inline-block;" ]
                picker
            | "Pagination" ->
                E "div" [ attr.style "display: flex; gap: 5px; margin: 15px 0; justify-content: center;" ] [
                    E "button" [ attr.style "padding: 5px 10px; background: #444; color: white; border: 1px solid #555; cursor: pointer;" ] [ text "Prev" ]
                    E "button" [ attr.style "padding: 5px 10px; background: #444; color: white; border: 1px solid #555; cursor: pointer;" ] [ text "Next" ]
                ]
            | "AutoComplete" ->
                E "div" [ attr.style "position: relative; display: inline-block; width: 100%; margin: 5px 0;" ] [
                    V "input" [ attr.``type`` "text"; attr.placeholder "Search..."; attr.style "padding: 8px; background: #333; color: white; border: 1px solid #555; border-radius: 4px; display: block; width: 100%; box-sizing: border-box;" ]
                ]
            | "Rolling" ->
                E "div" [ attr.style "padding: 10px; background: #222; color: #5bc0de; border-radius: 4px; border: 1px solid #444; margin: 10px 0;" ] [ text "Rolling..." ]
            | "Tree" ->
                let dataRefStr = JS.Inline<string>("$0.dataRef || ''", obj)
                E "ul" [ attr.style "list-style-type: none; padding-left: 20px; color: #ccc;" ] [
                    E "li" [ attr.style "padding: 5px 0; cursor: pointer;" ] [ text ("Tree Node bound to: " + dataRefStr) ]
                ]
            | "ContextMenu" ->
                V "div" [ attr.style "display: none; position: absolute; background: #333; border: 1px solid #555; box-shadow: 2px 2px 5px rgba(0,0,0,0.5); z-index: 1000;" ]
            | _ -> Doc.Empty

    let createSduiCanvas (jsonStr: string) =
        let isExpanded = Var.Create false
        
        let injectStyles () =
            if IsClient && JS.Document.Head <> null then
                let styleId = "sdui-dynamic-styles"
                if JS.Document.GetElementById(styleId) = null then
                    let style = JS.Document.CreateElement("style")
                    style.SetAttribute("id", styleId)
                    style.TextContent <- """
                        @keyframes spin { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }
                    """
                    JS.Document.Head.AppendChild(style) |> ignore
                    
        injectStyles ()
        
        E "div" [ attr.style "border: 1px solid #5bc0de; border-radius: 8px; padding: 15px; margin: 10px 0; background: #2b2b2b; color: #eee; font-family: sans-serif; box-shadow: 0 4px 6px rgba(0,0,0,0.3);" ] [
            E "div" [ attr.style "display: flex; justify-content: space-between; align-items: center; border-bottom: 1px solid #444; padding-bottom: 10px; margin-bottom: 15px;" ] [
                E "span" [ attr.style "font-weight: bold; font-size: 1.2rem; color: #5bc0de;" ] [ text "FSkynet SDUI Canvas" ]
                E "button" [
                    attr.style "background: #444; color: #aaa; border: 1px solid #555; border-radius: 4px; padding: 5px 10px; cursor: pointer;"
                    on.click (fun _ _ -> isExpanded.Value <- not isExpanded.Value)
                ] [ textView (isExpanded.View |> View.Map (fun e -> if e then "Hide JSON" else "View JSON")) ]
            ]
            
            isExpanded.View |> View.Map (fun e ->
                if e then
                    E "pre" [ attr.style "background: #1e1e1e; padding: 10px; border-radius: 4px; overflow-x: auto; color: #aaa; font-size: 0.85rem;" ] [ text jsonStr ]
                else Doc.Empty
            ) |> Doc.EmbedView
            
            E "div" [ attr.style "margin-top: 10px;" ] [
                try
                    let items = JS.Inline<obj[]>("""
                        var payloadObj = JSON.parse($0);
                        var sduiNode = payloadObj.sdui;
                        if (!sduiNode) return [];
                        var unwrapped = window.unwrapFCell ? window.unwrapFCell(sduiNode) : sduiNode;
                        return Array.isArray(unwrapped) ? unwrapped : [unwrapped];
                    """, jsonStr)
                    
                    let elements = items |> Array.map renderNode |> Array.toList
                    E "div" [] [ Doc.Concat elements ]
                with ex ->
                    E "pre" [ attr.style "color: #d9534f;" ] [ text ("Error parsing SDUI Canvas: " + ex.Message) ]
            ]
        ]

    let TryRender (content: string) : option<Doc> =
        match tryGetSchema content with
        | Some "fskynet-sdui" ->
            if IsClient then
                Some (createSduiCanvas content)
            else
                Some Doc.Empty 
        | _ ->
            None
