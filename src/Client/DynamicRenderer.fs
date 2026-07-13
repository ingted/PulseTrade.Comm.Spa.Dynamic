namespace PulseTrade.Comm.Spa.Dynamic.Client

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Html
open WebSharper.UI.Client

[<JavaScript>]
module DynamicRenderer =

    let E name attrs (children: seq<#Doc>) =
        Doc.Element name attrs (children |> Seq.cast<Doc>) :> Doc
    let V name attrs =
        Doc.Element name attrs [] :> Doc

    let tryGet<'T> name (value: obj) =
        try
            if isNull value || not (JS.HasOwnProperty value name) then None
            else Some(JS.Get<'T> name value)
        with _ -> None

    let getText name fallback value =
        tryGet<string> name value
        |> Option.filter (System.String.IsNullOrWhiteSpace >> not)
        |> Option.defaultValue fallback

    let getArray<'T> name value =
        tryGet<'T array> name value |> Option.defaultValue [||]

    let unwrapFCell value =
        try
            if JS.In "unwrapFCell" JS.Window then
                let unwrap = JS.Get<obj -> obj> "unwrapFCell" JS.Window
                unwrap value
            else
                value
        with _ -> value

    let tryGetSchema (jsonStr: string) =
        try
            if IsClient then
                let value = JSON.Parse jsonStr
                tryGet<string> "schema" value
            else
                if jsonStr.Contains("\"schema\":\"fskynet-sdui\"") then Some "fskynet-sdui" else None
        with _ -> None

    let rec renderNode (obj: obj) (payloadObj: obj) : Doc =
        if JS.TypeOf obj = JS.Kind.Undefined || obj = null then Doc.Empty
        else
            let t = getText "type" "" obj
            match t with
            | "Heading" ->
                let textStr = getText "text" "" obj
                E "h2" [ attr.style "color: #5bc0de; margin-bottom: 15px;" ] [ text textStr ]
            | "Label" ->
                let textStr = getText "text" "" obj
                E "span" [ attr.style "margin-right: 10px; color: #ccc;" ] [ text textStr ]
            | "TextInput" ->
                let placeholderStr = getText "placeholder" "" obj
                let idStr = getText "id" "" obj
                let attrs = 
                    [ attr.``type`` "text"; attr.placeholder placeholderStr; attr.style "padding: 8px; background: #333; color: white; border: 1px solid #555; border-radius: 4px; display: block; width: 100%; box-sizing: border-box; margin: 5px 0;" ]
                    @ (if not (System.String.IsNullOrEmpty idStr) then [ on.afterRender (fun el -> el.SetAttribute("id", idStr)) ] else [])
                V "input" attrs
            | "Row" ->
                let childrenObj = getArray<obj> "children" obj
                let childrenDocs = childrenObj |> Array.map (fun c -> renderNode c payloadObj) |> Array.toList
                E "div" [ attr.style "display: flex; flex-direction: row; gap: 15px; margin-bottom: 10px; align-items: center;" ] childrenDocs
            | "Column" ->
                let childrenObj = getArray<obj> "children" obj
                let childrenDocs = childrenObj |> Array.map (fun c -> renderNode c payloadObj) |> Array.toList
                E "div" [ attr.style "display: flex; flex-direction: column; gap: 10px;" ] childrenDocs
            | "Divider" ->
                V "hr" [ attr.style "border: 0; border-top: 1px solid #444; margin: 15px 0; width: 100%;" ]
            | "Dropdown" | "SelectBox" ->
                let isMultiple = tryGet<bool> "multiple" obj |> Option.defaultValue false
                let optionsArr = getArray<string> "options" obj
                let optionDocs = optionsArr |> Array.map (fun opt -> E "option" [] [ text opt ]) |> Array.toList
                let attrs = 
                    [ attr.style "padding: 8px; margin: 5px 0; background: #333; color: white; border: 1px solid #555; border-radius: 4px; font-size: 1rem; display: block; width: 200px;" ]
                    @ (if isMultiple then [ on.afterRender (fun el -> el.SetAttribute("multiple", "multiple")) ] else [])
                E "select" attrs optionDocs
            | "DataGrid" ->
                let dataRefStr = getText "dataRef" "" obj
                let rows =
                    tryGet<obj> "data" payloadObj
                    |> Option.map unwrapFCell
                    |> Option.bind (tryGet<obj array> dataRefStr)
                    |> Option.defaultValue [||]
                
                let gridContainer = E "div" [ attr.style "background: #1e1e1e; border-radius: 8px; overflow: hidden; border: 1px solid #444; margin: 20px 0;" ] [
                    if rows.Length > 0 then
                        let firstRow = rows.[0]
                        let keys = JS.GetFieldNames firstRow
                        
                        let thead = E "thead" [] [
                            E "tr" [ attr.style "background: #333; color: #aaa;" ] (
                                keys |> Array.map (fun k -> E "th" [ attr.style "padding: 12px 15px; border-bottom: 1px solid #555;" ] [ text k ]) |> Array.toList
                            )
                        ]
                        let tbody = E "tbody" [] (
                            rows |> Array.map (fun rowObj ->
                                E "tr" [ attr.style "border-bottom: 1px solid #444;" ] (
                                    keys |> Array.map (fun k ->
                                        let cellVal =
                                            tryGet<obj> k rowObj
                                            |> Option.map string
                                            |> Option.defaultValue ""
                                        E "td" [ attr.style "padding: 12px 15px;" ] [ text cellVal ]
                                    ) |> Array.toList
                                )
                            ) |> Array.toList
                        )
                        E "table" [ attr.style "width: 100%; border-collapse: collapse; text-align: left;" ] [ thead; tbody ]
                    else
                        E "div" [ attr.style "padding: 20px; color: #ccc;" ] [ text ("No data found for dataRef: " + dataRefStr) ]
                ]
                gridContainer
            | "Button" ->
                let btnText = getText "text" "Button" obj
                E "button" [
                    attr.``class`` "btn btn-success canvas-btn"
                    attr.style "margin-top: 15px; padding: 10px 20px; font-weight: bold; background: #5cb85c; color: white; border: none; border-radius: 4px; cursor: pointer; font-size: 1rem;"
                    on.click (fun _ _ -> JS.Window.Alert("Dispatcher: Sending command..."))
                ] [ text btnText ]
            | "AppLoader" ->
                let textStr = getText "text" "Loading..." obj
                E "div" [ attr.style "display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 20px; color: #5bc0de;" ] [
                    V "div" [ attr.style "border: 4px solid rgba(255,255,255,0.1); border-top: 4px solid #5bc0de; border-radius: 50%; width: 40px; height: 40px; animation: spin 1s linear infinite;" ]
                    E "span" [ attr.style "margin-top: 10px;" ] [ text textStr ]
                ]
            | "ColorPicker" ->
                let defaultColor = getText "defaultColor" "#000000" obj
                let idStr = getText "id" "" obj
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
                let direction = getText "direction" "left" obj
                let dataRefStr = getText "dataRef" "" obj
                let items =
                    tryGet<obj> "data" payloadObj
                    |> Option.map unwrapFCell
                    |> Option.bind (tryGet<obj array> dataRefStr)
                    |> Option.defaultValue [||]
                    |> Array.map string
                
                let contentText = if items.Length > 0 then String.concat " | " items else "No data for Rolling."
                V "marquee" [ 
                    attr.style "padding: 10px; background: #222; color: #5bc0de; border-radius: 4px; border: 1px solid #444; margin: 10px 0;"
                    on.afterRender (fun el -> 
                        el.SetAttribute("direction", direction)
                        el.TextContent <- contentText
                    )
                ]
            | "Tree" ->
                let dataRefStr = getText "dataRef" "" obj
                // A simple placeholder for tree. You can expand this to recursive tree parsing.
                E "ul" [ attr.style "list-style-type: none; padding-left: 20px; color: #ccc;" ] [
                    E "li" [ attr.style "padding: 5px 0; cursor: pointer;" ] [ text ("Tree Node bound to: " + dataRefStr) ]
                ]
            | "ContextMenu" ->
                V "div" [ attr.style "display: none; position: absolute; background: #333; border: 1px solid #555; box-shadow: 2px 2px 5px rgba(0,0,0,0.5); z-index: 1000;" ]
            | _ -> Doc.Empty

    let createSduiCanvasBody (jsonStr: string) =
        try
            let payloadObj = JSON.Parse jsonStr
            let sduiNode =
                tryGet<obj> "ui" payloadObj
                |> Option.orElseWith (fun () -> tryGet<obj> "sdui" payloadObj)

            match sduiNode with
            | None ->
                E "div" [ attr.``class`` "sdui-canvas-error" ] [ text "SDUI Canvas has no ui or sdui document." ]
            | Some rawNode ->
                let unwrapped = unwrapFCell rawNode
                let items =
                    if JS.Global?Array?isArray(unwrapped) then As<obj array> unwrapped else [| unwrapped |]

                items
                |> Array.map (fun item -> renderNode item payloadObj)
                |> Array.toList
                |> Doc.Concat
                |> fun content -> E "div" [ attr.``class`` "sdui-canvas-content" ] [ content ]
        with error ->
            E "pre" [ attr.``class`` "sdui-canvas-error" ] [ text ("Error parsing SDUI Canvas: " + error.Message) ]

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
                        .sdui-json-snippet {
                            background: #222; color: #aaa; padding: 10px; border-radius: 4px; font-size: 0.85em;
                            cursor: pointer; margin-bottom: 12px; white-space: pre-wrap; word-break: break-all;
                            max-height: 80px; overflow: hidden; width: 100%; box-sizing: border-box;
                        }
                        .sdui-json-snippet.expanded {
                            max-height: 400px; overflow-y: auto;
                        }
                    """
                    JS.Document.Head.AppendChild(style) |> ignore
                    
        injectStyles ()
        
        let jsonSnippet = 
            let t = jsonStr
            if t.Length > 100 then t.Substring(0, 100) + "..." else t
            
        let isCodeExpanded = Var.Create false
        
        E "div" [ attr.``class`` "sdui-summary-card"; attr.style "border: 1px solid #5bc0de; padding: 15px; border-radius: 6px; background: rgba(91, 192, 222, 0.1); margin-top: 10px; display: flex; flex-direction: column; align-items: flex-start;" ] [
            E "strong" [ attr.style "display: block; margin-bottom: 5px; color: #5bc0de; font-size: 1.1em;" ] [ text "📈 FSkynet 動態畫布 (Canvas)" ]
            E "span" [ attr.``class`` "muted"; attr.style "display: block; font-size: 0.9em; margin-bottom: 12px; color: #aaa;" ] [ text "點擊展開以顯示具備排序、篩選及下單功能的互動式網格與圖表。" ]
            
            E "pre" [ 
                attr.classDyn (isCodeExpanded.View |> View.Map (fun e -> if e then "sdui-json-snippet expanded" else "sdui-json-snippet"))
                attr.title "點擊檢視完整 JSON"
                on.click (fun _ _ -> isCodeExpanded.Value <- not isCodeExpanded.Value)
            ] [ 
                textView (isCodeExpanded.View |> View.Map (fun e -> if e then jsonStr else jsonSnippet))
            ]
            
            E "button" [ 
                attr.``class`` "btn btn-info"
                attr.style "background: #5bc0de; color: #111; font-weight: bold; border: none; padding: 6px 12px; border-radius: 4px; cursor: pointer; margin-bottom: 10px;"
                on.click (fun _ _ -> isExpanded.Value <- not isExpanded.Value)
            ] [ 
                textView (isExpanded.View |> View.Map (fun e -> if e then "收合 Canvas" else "展開 Canvas"))
            ]
            
            (isExpanded.View |> View.Map (fun expanded ->
                if expanded then
                    // Render Full Screen Overlay Modal
                    let overlayStyle = "position: fixed; top: 0; left: 0; width: 100vw; height: 100vh; background: rgba(0,0,0,0.8); z-index: 9999; display: flex; flex-direction: column; padding: 40px; box-sizing: border-box;"
                    let headerStyle = "display: flex; justify-content: space-between; align-items: center; background: #1e1e1e; padding: 15px 25px; border-radius: 8px 8px 0 0; color: #fff; font-family: sans-serif; box-shadow: 0 4px 6px rgba(0,0,0,0.3);"
                    let bodyStyle = "flex: 1; background: #2b2b2b; padding: 30px; overflow-y: auto; border-radius: 0 0 8px 8px; color: #eee; font-family: sans-serif;"
                    
                    E "div" [ attr.style overlayStyle ] [
                        E "div" [ attr.style headerStyle ] [
                            E "h2" [ attr.style "margin: 0; font-size: 1.5rem; font-weight: normal;" ] [ text "FSkynet SDUI Canvas" ]
                            E "button" [
                                attr.``class`` "btn btn-danger"
                                attr.style "background: #d9534f; color: white; border: none; padding: 8px 16px; border-radius: 4px; cursor: pointer;"
                                on.click (fun _ _ -> isExpanded.Value <- false)
                            ] [ text "關閉 Canvas" ]
                        ]
                        E "div" [ attr.style bodyStyle ] [
                            createSduiCanvasBody jsonStr
                        ]
                    ]
                else
                    Doc.Empty
            )) |> Doc.EmbedView
        ]

    let TryRender (rawContent: string) : option<Doc> =
        if IsClient then JS.Global?console?log("DynamicRenderer.TryRender called with:", rawContent)

        let replyIndex = rawContent.IndexOf("replied msg:")
        let outboundOnly =
            replyIndex < 0
            && rawContent.IndexOf("argu msg:") >= 0

        let content =
            let idx = replyIndex
            if idx >= 0 then
                let jsonPart = rawContent.Substring(idx + "replied msg:".Length).Trim()
                jsonPart
            else
                rawContent

        if IsClient then JS.Global?console?log("Content after strip:", content)
        if outboundOnly then
            None
        else
            match tryGetSchema content with
            | Some "fskynet-sdui" ->
                if IsClient then JS.Global?console?log("Schema is fskynet-sdui, rendering canvas!")
                if IsClient then
                    Some (createSduiCanvas content)
                else
                    Some Doc.Empty
            | _ ->
                if IsClient then JS.Global?console?log("Schema not matched:", tryGetSchema content)
                None
