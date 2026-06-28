namespace PulseTrade.Comm.Spa.Dynamic.Client

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Html
open WebSharper.UI.Client

[<JavaScript>]
module ActorDynamicTab =

    let IsActorsPagePayload (rawContent: string) =
        rawContent.IndexOf("ActorTopologyPage") >= 0

    let E name attrs (children: seq<#Doc>) =
        Doc.Element name attrs (children |> Seq.cast<Doc>) :> Doc

    let V name attrs =
        Doc.Element name attrs [] :> Doc

    let isBlank (value: string) =
        value = null || value.Trim() = ""

    let actorNodes (rawContent: string) =
        try
            let payload = JS.Global?JSON?parse(rawContent)
            let data: obj = payload?data

            if data = null || JS.TypeOf data = JS.Kind.Undefined then
                [||]
            else
                let nodes: obj = data?actorTreeNodes

                if nodes = null || JS.TypeOf nodes = JS.Kind.Undefined then
                    [||]
                else
                    unbox<obj[]> nodes
        with _ ->
            [||]

    let projectionText (rawContent: string) (fieldName: string) fallback =
        try
            let payload = JS.Global?JSON?parse(rawContent)
            let value =
                match fieldName with
                | "projectionId" -> payload?projectionId
                | "projectionVersion" -> payload?projectionVersion
                | _ -> null

            if value = null || JS.TypeOf value = JS.Kind.Undefined then fallback else string value
        with _ ->
            fallback

    let nodeId (node: obj) =
        try JS.Inline<string>("$0.id || ''", node) with _ -> ""

    let nodeParentId (node: obj) =
        try JS.Inline<string>("$0.parentId || ''", node) with _ -> ""

    let nodeLabel (node: obj) =
        let label =
            try JS.Inline<string>("$0.label || ''", node) with _ -> ""

        if isBlank label then nodeId node else label

    let nodeFullPath (node: obj) =
        try JS.Inline<string>("$0.fullPath || ''", node) with _ -> ""

    let nodeRawAddress (node: obj) =
        try JS.Inline<string>("$0.address || ''", node) with _ -> ""

    let nodeAddress (node: obj) =
        let address = nodeRawAddress node
        if isBlank address then nodeFullPath node else address

    let nodeKind (node: obj) =
        try JS.Inline<string>("$0.kind || ''", node) with _ -> ""

    let nodeStatus (node: obj) =
        try JS.Inline<string>("$0.status || ''", node) with _ -> ""

    let actorSystemAddress (address: string) =
        if isBlank address then
            "unknown"
        else
            let userIndex = address.IndexOf("/user")
            let systemIndex = address.IndexOf("/system")
            let cutIndex =
                if userIndex > 0 && systemIndex > 0 then
                    if userIndex < systemIndex then userIndex else systemIndex
                elif userIndex > 0 then userIndex
                elif systemIndex > 0 then systemIndex
                else -1

            if address.IndexOf("://") >= 0 && cutIndex > 0 then
                address.Substring(0, cutIndex)
            elif address.IndexOf("://") >= 0 then
                address
            else
                "unknown"

    let lower (value: string) =
        if value = null then "" else value.ToLower()

    let hasToken (token: string) (value: string) =
        (lower value).IndexOf(token) >= 0

    let classifyNodeBlock (key: string) (nodes: obj[]) =
        let sample =
            nodes
            |> Array.map (fun node -> nodeLabel node + " " + nodeKind node + " " + nodeAddress node)
            |> String.concat " "
            |> fun text -> key + " " + text

        if key = "unknown" || isBlank key then
            3, "Unknown"
        elif hasToken "ptcshost" key || hasToken "ptcs-host" key || hasToken "ptcs" key || hasToken "commspa" key then
            0, "PTCS Host"
        elif hasToken "gwhost" key || hasToken "gw-host" key || hasToken "gateway" key then
            1, "GW Host"
        elif hasToken "rnhost" key || hasToken "rn-host" key || hasToken "resourcenode" key || hasToken "resource-node" key then
            2, "RN Host"
        elif hasToken "ptcs" sample || hasToken "spa" sample || hasToken "commspa" sample then
            0, "PTCS Host"
        elif hasToken "gw" sample || hasToken "gateway" sample then
            1, "GW Host"
        elif hasToken "rn" sample || hasToken "resource" sample then
            2, "RN Host"
        else
            3, "Unknown"

    let distinctValues (values: string[]) =
        let mutable known : string list = []

        for value in values do
            let normalized = if value = null then "" else value.Trim()
            if normalized <> "" && not (known |> List.exists (fun current -> current = normalized)) then
                known <- known @ [ normalized ]

        known |> List.toArray

    let renderCountCard title value =
        div [ attr.style "border:1px solid #d9e3f0; border-radius:6px; background:#fff; padding:10px 12px;" ] [
            div [ attr.style "font-size:11px; color:#667891;" ] [ text title ]
            div [ attr.style "margin-top:4px; font-size:18px; font-weight:700;" ] [ text value ]
        ]

    let renderStatusChip status =
        let normalized = lower status
        let color =
            if normalized.IndexOf("active") >= 0 || normalized.IndexOf("running") >= 0 then "#0b6b3a"
            elif normalized.IndexOf("stale") >= 0 || normalized.IndexOf("changed") >= 0 then "#8a5a00"
            elif normalized.IndexOf("terminated") >= 0 || normalized.IndexOf("dead") >= 0 then "#8b1e2d"
            else "#46566b"

        span [
            attr.style ("display:inline-block; border:1px solid " + color + "; color:" + color + "; border-radius:999px; padding:2px 7px; font-size:11px; line-height:16px; white-space:nowrap;")
        ] [
            text (if isBlank status then "unknown" else status)
        ]

    let renderStatusDot status =
        let normalized = lower status
        let color, halo =
            if normalized.IndexOf("active") >= 0 || normalized.IndexOf("running") >= 0 then "#16a34a", "#dcfce7"
            elif normalized.IndexOf("passivated") >= 0 then "#d97706", "#fef3c7"
            elif normalized.IndexOf("stale") >= 0 || normalized.IndexOf("changed") >= 0 then "#d97706", "#fef3c7"
            elif normalized.IndexOf("terminated") >= 0 || normalized.IndexOf("dead") >= 0 then "#dc2626", "#fee2e2"
            else "#64748b", "#e2e8f0"

        span [
            attr.style ("width:8px; height:8px; border-radius:999px; background:" + color + "; box-shadow:0 0 0 2px " + halo + "; display:inline-block;")
            on.afterRender (fun node ->
                node.SetAttribute("data-testid", "dynamic-actor-tree-status-dot")
                node.SetAttribute("data-status", if isBlank status then "unknown" else status))
        ] []

    let renderSmallPill value =
        span [
            attr.style "white-space:nowrap; border:1px solid #d4ddec; border-radius:999px; background:#fff; color:#44546d; padding:1px 7px; font-size:11px; line-height:1.45;"
        ] [
            text (if isBlank value then "unknown" else value)
        ]

    let withAncestors (allNodes: obj[]) (seedNodes: obj[]) =
        let mutable result: obj list = []

        let contains id (items: obj list) =
            items |> List.exists (fun item -> nodeId item = id)

        let add node =
            let id = nodeId node
            if not (isBlank id) && not (contains id result) then
                result <- result @ [ node ]

        let tryFindNode id =
            allNodes |> Array.tryFind (fun node -> nodeId node = id)

        let rec addAncestors node =
            let parentId = nodeParentId node
            if not (isBlank parentId) then
                match tryFindNode parentId with
                | Some parent ->
                    addAncestors parent
                    add parent
                | None -> ()

        for node in seedNodes do
            addAncestors node
            add node

        result |> List.toArray

    let createNodeGroups (nodes: obj[]) =
        let groupKey node =
            actorSystemAddress (nodeRawAddress node)

        let concreteNodes =
            nodes
            |> Array.filter (fun node -> groupKey node <> "unknown")

        let knownGroups =
            concreteNodes
            |> Array.groupBy groupKey
            |> Array.map (fun (key, groupNodes) ->
                let augmentedNodes = withAncestors nodes groupNodes
                let rank, label = classifyNodeBlock key augmentedNodes
                rank, key, label, augmentedNodes)

        let isKnownNode node =
            let id = nodeId node
            knownGroups
            |> Array.exists (fun (_, _, _, groupNodes) ->
                groupNodes |> Array.exists (fun known -> nodeId known = id))

        let unknownSeeds =
            nodes
            |> Array.filter (fun node -> not (isKnownNode node) && groupKey node = "unknown")

        let unknownGroups =
            if unknownSeeds.Length = 0 then
                [||]
            else
                let key = "unknown"
                let augmentedNodes = withAncestors nodes unknownSeeds
                let rank, label = classifyNodeBlock key augmentedNodes
                [| rank, key, label, augmentedNodes |]

        Array.append knownGroups unknownGroups
        |> Array.sortBy (fun (rank, key, _, _) -> rank, key)

    let renderTree (groupNodes: obj[]) =
        let collapsedIds = Var.Create [||]

        let containsId id (ids: string[]) =
            ids |> Array.exists (fun current -> current = id)

        let toggleId id =
            let current = collapsedIds.Value
            collapsedIds.Value <-
                if containsId id current then
                    current |> Array.filter (fun value -> value <> id)
                else
                    Array.append current [| id |]

        let nodeExists id =
            groupNodes |> Array.exists (fun node -> nodeId node = id)

        let childrenOf parentId =
            groupNodes
            |> Array.filter (fun node -> nodeParentId node = parentId)
            |> Array.sortBy nodeLabel

        let roots =
            groupNodes
            |> Array.filter (fun node ->
                let parentId = nodeParentId node
                isBlank parentId || not (nodeExists parentId))
            |> Array.sortBy nodeLabel

        let rec renderNode collapsed depth (node: obj) =
            let id = nodeId node
            let children = childrenOf id
            let depthValue = min 12 (max 0 depth)
            let margin = string (depthValue * 18)
            let address = nodeAddress node
            let fullPath = nodeFullPath node
            let displayAddress = if isBlank address then fullPath else address
            let isCollapsed = containsId id collapsed

            let row =
                div [
                    attr.``class`` "dynamic-actor-tree-row"
                    attr.style ("position:relative; display:grid; grid-template-columns:20px 10px max-content max-content max-content; gap:8px; align-items:center; width:max-content; min-width:100%; padding:4px 10px 4px 6px; border-radius:5px; font-size:12px; line-height:1.35; margin-left:" + margin + "px;")
                    on.afterRender (fun node ->
                        node.SetAttribute("data-testid", "dynamic-actor-tree-row")
                        node.SetAttribute("data-node-id", id)
                        node.SetAttribute("data-parent-id", nodeParentId node)
                        node.SetAttribute("data-depth", string depthValue))
                ] [
                    if depthValue > 0 then
                        span [
                            attr.``class`` "dynamic-actor-tree-connector-h"
                            attr.style "position:absolute; left:-12px; top:50%; width:12px; border-top:1px solid #aeb8c8;"
                            on.afterRender (fun node -> node.SetAttribute("data-testid", "dynamic-actor-tree-connector"))
                        ] []
                        span [
                            attr.``class`` "dynamic-actor-tree-connector-v"
                            attr.style "position:absolute; left:-12px; top:-5px; height:calc(100% + 5px); border-left:1px solid #aeb8c8;"
                        ] []
                    if children.Length > 0 then
                        button [
                            attr.``type`` "button"
                            attr.title (if isCollapsed then "Expand actor node" else "Collapse actor node")
                            attr.style "display:inline-flex; align-items:center; justify-content:center; width:18px; height:18px; border:1px solid #7d92ad; background:#fff; color:#21354f; font-size:12px; line-height:16px; padding:0; margin-top:3px; cursor:pointer; font-family:Consolas, 'Cascadia Mono', monospace;"
                            on.click (fun _ _ -> toggleId id)
                            on.afterRender (fun node ->
                                node.SetAttribute("data-testid", "dynamic-actor-tree-toggle")
                                node.SetAttribute("aria-expanded", if isCollapsed then "false" else "true"))
                        ] [
                            text (if isCollapsed then "+" else "-")
                        ]
                    else
                        span [
                            attr.style "display:inline-flex; width:18px; height:18px; margin-top:3px;"
                            on.afterRender (fun node -> node.SetAttribute("data-testid", "dynamic-actor-tree-toggle-placeholder"))
                        ] []
                    renderStatusDot (nodeStatus node)
                    span [
                        attr.``class`` "dynamic-actor-tree-label"
                        attr.title displayAddress
                        attr.style "white-space:nowrap; color:#172033; font-weight:600; overflow:visible; text-overflow:clip; font-family:Consolas, 'Cascadia Mono', monospace;"
                    ] [
                        text displayAddress
                    ]
                    renderSmallPill (nodeKind node)
                    renderStatusChip (nodeStatus node)
                ]

            let childDocs =
                if isCollapsed || depth >= 24 then
                    []
                else
                    children
                    |> Array.toList
                    |> List.collect (renderNode collapsed (depth + 1))

            row :: childDocs

        let treeRows collapsed =
            roots
            |> Array.toList
            |> List.collect (renderNode collapsed 0)

        div [
            attr.style "border:1px solid #d8e2ef; background:#f8fafc; border-radius:6px; padding:8px 10px; overflow-x:scroll; overflow-y:auto; scrollbar-gutter:stable; max-height:430px;"
            on.afterRender (fun node -> node.SetAttribute("data-testid", "dynamic-actor-tree-viewport"))
        ] [
            collapsedIds.View
            |> View.Map (fun collapsed ->
                let rows = treeRows collapsed
                if rows.IsEmpty then
                    div [ attr.style "color:#667891; font-size:12px;" ] [ text "No actor tree rows." ]
                else
                    Doc.Concat rows)
            |> Doc.EmbedView
        ]

    let renderGrid (groupNodes: obj[]) =
        let headerCell label =
            E "th" [ attr.style "text-align:left; padding:8px 10px; border-bottom:1px solid #d7e2ef; color:#53677f; font-size:11px; white-space:nowrap;" ] [ text label ]

        let bodyRow node =
            E "tr" [] [
                E "td" [ attr.style "padding:8px 10px; border-bottom:1px solid #edf2f8; white-space:nowrap;" ] [ text (nodeKind node) ]
                E "td" [ attr.style "padding:8px 10px; border-bottom:1px solid #edf2f8; white-space:nowrap;" ] [ renderStatusChip (nodeStatus node) ]
                E "td" [ attr.style "padding:8px 10px; border-bottom:1px solid #edf2f8; font-family:Consolas, 'Cascadia Mono', monospace; font-size:12px; white-space:nowrap;" ] [ text (nodeAddress node) ]
                E "td" [ attr.style "padding:8px 10px; border-bottom:1px solid #edf2f8; font-family:Consolas, 'Cascadia Mono', monospace; font-size:12px; white-space:nowrap;" ] [ text (nodeFullPath node) ]
            ]

        div [
            attr.style "overflow-x:auto; border:1px solid #d8e2ef; border-radius:6px; background:#fff;"
            on.afterRender (fun node -> node.SetAttribute("data-testid", "dynamic-actor-grid"))
        ] [
            E "table" [ attr.style "border-collapse:collapse; min-width:980px; width:100%;" ] [
                E "thead" [] [
                    E "tr" [] [
                        headerCell "Kind"
                        headerCell "Status"
                        headerCell "Address"
                        headerCell "Full path"
                    ]
                ]
                E "tbody" [] (groupNodes |> Array.map bodyRow |> Array.toList)
            ]
        ]

    let renderNodeBlock (key: string) (roleLabel: string) (groupNodes: obj[]) =
        let statuses =
            groupNodes
            |> Array.map nodeStatus
            |> distinctValues
            |> String.concat ", "

        section [
            attr.style "display:flex; flex-direction:column; gap:10px; border:1px solid #cfdcec; background:#fff; border-radius:7px; padding:12px;"
            on.afterRender (fun node -> node.SetAttribute("data-testid", "dynamic-actor-node-block"))
        ] [
            div [ attr.style "display:flex; align-items:flex-start; justify-content:space-between; gap:12px; flex-wrap:wrap;" ] [
                div [ attr.style "min-width:0;" ] [
                    div [ attr.style "font-size:11px; color:#667891;" ] [ text roleLabel ]
                    h3 [
                        attr.style "margin:2px 0 0 0; font-size:15px; font-weight:700; color:#16263c; font-family:Consolas, 'Cascadia Mono', monospace; white-space:nowrap; overflow-x:auto;"
                    ] [
                        text key
                    ]
                ]
                div [ attr.style "font-size:12px; color:#53677f; white-space:nowrap;" ] [
                    let concreteCount =
                        groupNodes
                        |> Array.filter (fun node -> not (isBlank (nodeRawAddress node)))
                        |> Array.length

                    text (string concreteCount + " actor node(s)")
                ]
            ]
            div [ attr.style "display:flex; gap:8px; align-items:center; flex-wrap:wrap; font-size:12px; color:#53677f;" ] [
                span [ attr.style "font-weight:650;" ] [ text "Status" ]
                span [] [ text (if isBlank statuses then "unknown" else statuses) ]
            ]
            renderTree groupNodes
            renderGrid groupNodes
        ]

    let createActorsPageDocument (rawContent: string) =
        let nodes: obj[] = actorNodes rawContent
        let projectionId = projectionText rawContent "projectionId" "ptcs-actors"
        let projectionVersion = projectionText rawContent "projectionVersion" "0"
        let groups = createNodeGroups nodes

        let activeCount =
            nodes
            |> Array.filter (fun node ->
                let status = lower (nodeStatus node)
                status.IndexOf("active") >= 0 || status.IndexOf("running") >= 0)
            |> Array.length

        div [
            attr.``class`` "ptcs-dynamic-actors-page"
            on.afterRender (fun node -> node.SetAttribute("data-testid", "dynamic-actors-page"))
            attr.style "display:flex; flex-direction:column; gap:12px; color:#142033; min-width:0;"
        ] [
            div [
                attr.style "display:flex; justify-content:space-between; gap:12px; align-items:flex-start; border-bottom:1px solid #d8e1ee; padding-bottom:10px; flex-wrap:wrap;"
            ] [
                div [] [
                    h2 [ attr.style "margin:0; font-size:18px; font-weight:700;" ] [ text "Actors / Dynamic" ]
                    div [ attr.style "color:#50627a; font-size:12px;" ] [
                        text ("projection " + projectionId + " / v" + projectionVersion)
                    ]
                ]
                div [ attr.style "display:flex; gap:6px; flex-wrap:wrap;" ] [
                    button [
                        attr.``type`` "button"
                        attr.style "border:1px solid #b8c7dc; background:#fff; color:#22344d; border-radius:5px; padding:5px 9px; font-size:12px; cursor:pointer;"
                        on.afterRender (fun node -> node.SetAttribute("data-testid", "dynamic-actors-reload"))
                        on.click (fun _ _ -> JS.Window.Location.Reload())
                    ] [
                        text "Reload"
                    ]
                    button [
                        attr.``type`` "button"
                        attr.style "border:1px solid #cfd8e6; background:#f4f7fb; color:#738299; border-radius:5px; padding:5px 9px; font-size:12px;"
                        on.afterRender (fun node ->
                            node.SetAttribute("data-testid", "dynamic-actors-generate-report")
                            node.SetAttribute("disabled", "disabled"))
                    ] [
                        text "Generate report"
                    ]
                    button [
                        attr.``type`` "button"
                        attr.style "border:1px solid #cfd8e6; background:#f4f7fb; color:#738299; border-radius:5px; padding:5px 9px; font-size:12px;"
                        on.afterRender (fun node ->
                            node.SetAttribute("data-testid", "dynamic-actors-schedule-report")
                            node.SetAttribute("disabled", "disabled"))
                    ] [
                        text "Schedule"
                    ]
                ]
            ]
            div [
                attr.style "display:grid; grid-template-columns:repeat(auto-fit,minmax(180px,1fr)); gap:10px;"
            ] [
                renderCountCard "Renderer" "ActorsPage"
                renderCountCard "Node groups" (string groups.Length)
                renderCountCard "Actor tree rows" (string nodes.Length)
                renderCountCard "Active" (string activeCount)
            ]
            if nodes.Length = 0 then
                div [
                    attr.style "border:1px solid #c9d7e8; border-radius:6px; background:#fff; padding:12px; color:#4b5e76; font-size:12px;"
                ] [
                    text "No actor topology rows are available in this projection."
                ]
            else
                div [
                    attr.style "display:flex; flex-direction:column; gap:12px;"
                    on.afterRender (fun node -> node.SetAttribute("data-testid", "dynamic-actor-node-blocks"))
                ] [
                    groups
                    |> Array.map (fun (_, key, label, groupNodes) -> renderNodeBlock key label groupNodes)
                    |> Array.toList
                    |> Doc.Concat
                ]
        ]

    let registerActorsPageRenderer () =
        let renderer (rawContent: string) =
            try
                if IsActorsPagePayload rawContent then
                    let container = JS.Document.CreateElement("div")
                    Doc.Run container (createActorsPageDocument rawContent)
                    Some (container :> WebSharper.JavaScript.Dom.Node)
                else
                    None
            with e ->
                JS.Global?console?error("ActorsPage renderer failed:", e)
                None

        let isRegistered () =
            JS.Inline<bool>("!!(window.PulseTrade && window.PulseTrade.PageRenderers && window.PulseTrade.PageRenderers.some(function(r){ return r && r.name === 'ptcs-actors-page'; }))")

        let registerOnce () =
            if JS.In "PulseTradeRegisterPageRenderer" JS.Window && not (isRegistered ()) then
                JS.Inline("window.PulseTradeRegisterPageRenderer('ptcs-actors-page', 100, $0)", renderer)
                JS.Global?console?log("PulseTrade.Comm.Spa.Dynamic registered ActorsPage renderer.")

        let rec ensure attempts =
            registerOnce ()

            if attempts > 0 then
                JS.SetTimeout (fun () -> ensure (attempts - 1)) 200 |> ignore

        ensure 20
    
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
                let docOpt =
                    if IsActorsPagePayload text then
                        Some (createActorsPageDocument text)
                    else
                        DynamicRenderer.TryRender text

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
        registerActorsPageRenderer ()
        ArguFormRenderer.Register()
