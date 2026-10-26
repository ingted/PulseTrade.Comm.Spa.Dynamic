namespace PulseTrade.Comm.Spa.Dynamic.Tagging.Client

open System
open System.Collections.Generic
open WebSharper
open WebSharper.JavaScript
open WebSharper.JavaScript.Dom

[<JavaScript; RequireQualifiedAccess>]
type TaggingSearchMatcher =
    | Literal
    | Wildcard
    | Regex

[<JavaScript; RequireQualifiedAccess>]
type TaggingSearchField =
    | ContentText
    | TagText
    | SummaryText
    | TargetKey
    | TimeFrame

[<JavaScript>]
type TaggingSearchFieldSpec =
    { Field: TaggingSearchField
      Label: string
      InitiallySelected: bool }

[<JavaScript>]
type TaggingSearchQuery =
    { Text: string
      Matcher: TaggingSearchMatcher
      CaseSensitive: bool
      Fields: TaggingSearchField array
      Limit: int
      Cursor: string }

[<JavaScript>]
type TaggingSearchPage =
    { Items: TaggingExtraInfo array
      NextCursor: string
      ScanLimited: bool }

[<JavaScript>]
type TaggingSearchBinding =
    { ScopeLabel: string
      Fields: TaggingSearchFieldSpec array
      Dispatch: TaggingSearchQuery -> (Result<TaggingSearchPage,string> -> unit) -> unit
      Activate: TaggingExtraInfo -> (Result<unit,string> -> unit) -> unit }

[<JavaScript>]
type TaggingSearchOptions =
    { MaxOpenWindows: int
      DefaultLimit: int }

[<JavaScript>]
type TaggingSearchHandle =
    { SetBinding: TaggingSearchBinding option -> unit
      Dispose: unit -> unit }

[<JavaScript>]
type TaggingSearchWindow =
    { Key: int
      Binding: TaggingSearchBinding
      Query: TaggingSearchQuery
      Card: Element
      Results: Element
      Status: Element
      More: Element
      mutable NextCursor: string
      mutable SearchGeneration: int
      mutable ActivationGeneration: int
      mutable Left: float
      mutable Top: float }

[<JavaScript>]
module TaggingSearchPlugin =
    let node tag text =
        let element=JS.Document.CreateElement tag
        element.TextContent<-text
        element

    let append (parent: Node) (child: Node) = parent.AppendChild child |> ignore
    let clear (parent: Element) = while not(isNull parent.FirstChild) do parent.RemoveChild(parent.FirstChild)|>ignore
    let testId value (element: Element) = element.SetAttribute("data-testid",value);element
    let inputValue (element: Element) = JS.Get<string> "value" element
    let setInputValue text (element: Element) = JS.Set element "value" text
    let checkedValue (element: Element) = JS.Get<bool> "checked" element
    let setChecked value (element: Element) = JS.Set element "checked" value
    let setStyle name value (element: Element) = JS.Set (JS.Get<obj> "style" element) name value
    let documentContains (element: Element) = JS.Document.Contains element

    let defaultOptions = {MaxOpenWindows=8;DefaultLimit=50}

    let styleText =
        """
.ptcs-tag-search-launcher{display:flex;align-items:center;gap:6px;min-width:320px;flex:1}.ptcs-tag-search-launcher input[type=search]{min-width:140px;flex:1;padding:7px}.ptcs-tag-search-launcher select,.ptcs-tag-search-launcher button{padding:7px}.ptcs-tag-search-toggle{display:inline-flex;align-items:center;gap:3px;white-space:nowrap}.ptcs-tag-search-fields{position:relative}.ptcs-tag-search-fields summary{cursor:pointer;white-space:nowrap}.ptcs-tag-search-fields-panel{position:absolute;z-index:90;right:0;top:100%;display:grid;gap:5px;min-width:170px;padding:8px;background:#fff;border:1px solid #aeb8c2;border-radius:7px;box-shadow:0 8px 22px #17202a2e}.ptcs-tag-search-fields label{display:flex;gap:6px;align-items:center}
.ptcs-tag-search-layer{position:fixed;inset:0;z-index:70;pointer-events:none}.ptcs-tag-search-card{position:fixed;width:min(620px,calc(100vw - 32px));max-height:min(75vh,720px);overflow:auto;pointer-events:auto;background:#fff;color:#17212b;border:1px solid #8797a8;border-radius:12px;box-shadow:0 18px 54px #17202a52;padding:0 12px 12px;box-sizing:border-box}.ptcs-tag-search-header{position:sticky;top:0;z-index:2;display:flex;align-items:center;gap:8px;padding:10px 0 8px;background:#fff;cursor:move;border-top:4px solid #7046a3;border-bottom:2px solid #a89bb7}.ptcs-tag-search-header h3{flex:1;min-width:0;margin:0;font-size:16px;font-weight:800;overflow-wrap:anywhere}.ptcs-tag-search-context{display:block;margin:7px 0;color:#5f6c7a;font-size:12px}.ptcs-tag-search-results{display:grid;gap:7px}.ptcs-tag-search-result{display:block;width:100%;text-align:left;padding:9px;border:1px solid #ccd4dd;border-radius:8px;background:#fafbfc}.ptcs-tag-search-result:hover,.ptcs-tag-search-result:focus{background:#edf4ff;outline:2px solid #3978c5}.ptcs-tag-search-result strong,.ptcs-tag-search-result span{display:block;overflow-wrap:anywhere}.ptcs-tag-search-result-tags{color:#6a4b00}.ptcs-tag-search-status{min-height:20px;margin-top:7px;color:#536173}.ptcs-tag-search-status.error{color:#a31d1d}.ptcs-tag-search-more{margin-top:7px}.ptcs-tag-search-tabs{display:none}
@media(max-width:900px){.ptcs-tag-search-launcher{min-width:0;flex-wrap:wrap}.ptcs-tag-search-launcher input[type=search]{flex-basis:180px}}
@media(max-width:720px){.ptcs-tag-search-tabs{position:fixed;display:flex;pointer-events:auto;left:0;right:0;bottom:min(78vh,680px);overflow-x:auto;gap:5px;padding:5px 8px;background:#ece8f2;border-top:1px solid #a89bb7}.ptcs-tag-search-tabs button{white-space:nowrap}.ptcs-tag-search-card{display:none;left:0!important;right:0;top:auto!important;bottom:0;width:100%;max-height:min(78vh,680px);border-radius:14px 14px 0 0;padding-bottom:max(12px,env(safe-area-inset-bottom))}.ptcs-tag-search-card[data-active=true]{display:block}.ptcs-tag-search-header{cursor:default}}
"""

    let validateOptions options =
        if options.MaxOpenWindows<1 || options.MaxOpenWindows>12 then invalidArg "options" "tagging-search-max-open-invalid"
        if options.DefaultLimit<1 || options.DefaultLimit>100 then invalidArg "options" "tagging-search-limit-invalid"
        options

    let mount (launcherHost: Element) (layerHost: Element) inputOptions =
        let options=validateOptions inputOptions
        let style=node "style" styleText
        append layerHost style
        let launcher=node "div" "" |> testId "ptcs-tag-search-launcher"
        launcher.ClassName<-"ptcs-tag-search-launcher"
        append launcherHost launcher
        let queryInput=node "input" "" |> testId "ptcs-tag-search-input"
        queryInput.SetAttribute("type","search")
        queryInput.SetAttribute("placeholder","搜尋選取欄位")
        append launcher queryInput
        let matcher=node "select" "" |> testId "ptcs-tag-search-matcher"
        [|"literal","包含";"wildcard","Wildcard";"regex","Regex"|]
        |> Array.iter(fun (key,label) -> let option=node "option" label in setInputValue key option;append matcher option)
        append launcher matcher
        let caseLabel=node "label" ""
        caseLabel.ClassName<-"ptcs-tag-search-toggle"
        let caseSensitive=node "input" ""
        caseSensitive.SetAttribute("type","checkbox")
        append caseLabel caseSensitive;append caseLabel (node "span" "Aa")
        append launcher caseLabel
        let multiLabel=node "label" ""
        multiLabel.ClassName<-"ptcs-tag-search-toggle"
        let multiOpen=node "input" ""
        multiOpen.SetAttribute("type","checkbox")
        append multiLabel multiOpen;append multiLabel (node "span" "多開")
        append launcher multiLabel
        let fields=node "details" ""
        fields.ClassName<-"ptcs-tag-search-fields"
        append fields (node "summary" "欄位")
        let fieldsPanel=node "div" ""
        fieldsPanel.ClassName<-"ptcs-tag-search-fields-panel"
        append fields fieldsPanel
        let fieldInputs=ResizeArray<TaggingSearchField*Element>()
        let renderFieldInputs (specs: TaggingSearchFieldSpec array) =
            clear fieldsPanel
            fieldInputs.Clear()
            let seen=HashSet<TaggingSearchField>()
            let declared=if isNull specs then [||] else specs
            for spec in declared do
                if seen.Add spec.Field then
                    let labelHost=node "label" ""
                    let input=node "input" ""
                    input.SetAttribute("type","checkbox");setChecked spec.InitiallySelected input
                    append labelHost input;append labelHost (node "span" spec.Label);append fieldsPanel labelHost
                    fieldInputs.Add(spec.Field,input)
        append launcher fields
        let searchButton=node "button" "搜尋" |> testId "ptcs-tag-search-submit"
        searchButton.SetAttribute("type","button")
        append launcher searchButton
        let launcherStatus=node "span" ""
        launcherStatus.SetAttribute("role","status")
        append launcher launcherStatus

        let layer=node "section" "" |> testId "ptcs-tag-search-layer"
        layer.ClassName<-"ptcs-tag-search-layer"
        append layerHost layer
        let tabs=node "nav" ""
        tabs.ClassName<-"ptcs-tag-search-tabs"
        append layer tabs
        let windows=Dictionary<int,TaggingSearchWindow>()
        let mutable binding: TaggingSearchBinding option=None
        let mutable sequence=0
        let mutable activeKey = -1
        let mutable dragging: (int*float*float) option=None
        let mutable disposed=false

        let setPosition window left top =
            window.Left<-left;window.Top<-top
            setStyle "left" (string left+"px") window.Card
            setStyle "top" (string top+"px") window.Card

        let rec focusWindow key =
            if windows.ContainsKey key then
                activeKey<-key
                windows.Values |> Seq.iter(fun window -> window.Card.SetAttribute("data-active",if window.Key=key then "true" else "false"))
                clear tabs
                windows.Values |> Seq.sortBy _.Key |> Seq.iter(fun window ->
                    let label=if window.Query.Text.Length>14 then window.Query.Text.Substring(0,14)+"…" else window.Query.Text
                    let button=node "button" label
                    button.SetAttribute("type","button")
                    button.AddEventListener("click",fun (_:Event)->focusWindow window.Key)
                    append tabs button)

        let closeWindow key =
            match windows.TryGetValue key with
            | true,window ->
                if documentContains window.Card then window.Card.Remove()
                windows.Remove key |> ignore
                if activeKey=key then activeKey <- windows.Keys |> Seq.tryLast |> Option.defaultValue -1
                if activeKey>=0 then focusWindow activeKey else clear tabs
            | _ -> ()

        let renderResult (window: TaggingSearchWindow) (item: TaggingExtraInfo) =
            let button=node "button" ""
            button.ClassName<-"ptcs-tag-search-result"
            button.SetAttribute("type","button")
            button.SetAttribute("data-target-id",item.Target.TargetId)
            append button (node "strong" item.Target.TargetId)
            let tags=if isNull item.Draft.Tags then [||] else item.Draft.Tags
            let tagLine=node "span" (if tags.Length=0 then "(no tags)" else String.concat ", " tags)
            tagLine.ClassName<-"ptcs-tag-search-result-tags"
            append button tagLine
            if not(String.IsNullOrWhiteSpace item.Draft.Summary) then append button (node "span" item.Draft.Summary)
            button.AddEventListener("click",fun (_:Event)->
                window.ActivationGeneration<-window.ActivationGeneration+1
                let generation=window.ActivationGeneration
                window.Status.ClassName<-"ptcs-tag-search-status"
                window.Status.TextContent<-"定位中…"
                window.Binding.Activate item (fun result ->
                    if documentContains window.Card && generation=window.ActivationGeneration then
                        match result with
                        | Result.Ok () -> window.Status.TextContent<-"已定位 "+item.Target.TargetId
                        | Result.Error error -> window.Status.ClassName<-"ptcs-tag-search-status error";window.Status.TextContent<-error))
            append window.Results button

        let rec dispatchPage (window: TaggingSearchWindow) cursor appendResults =
            window.SearchGeneration<-window.SearchGeneration+1
            let generation=window.SearchGeneration
            window.Status.ClassName<-"ptcs-tag-search-status"
            window.Status.TextContent<-"搜尋中…"
            window.More.SetAttribute("hidden","")
            let query={window.Query with Cursor=cursor}
            window.Binding.Dispatch query (fun result ->
                if documentContains window.Card && generation=window.SearchGeneration then
                    match result with
                    | Result.Error error -> window.Status.ClassName<-"ptcs-tag-search-status error";window.Status.TextContent<-error
                    | Result.Ok page ->
                        if not appendResults then clear window.Results
                        let items=if isNull page.Items then [||] else page.Items
                        items |> Array.iter(renderResult window)
                        window.NextCursor<-if isNull page.NextCursor then "" else page.NextCursor
                        if window.NextCursor<>"" then window.More.RemoveAttribute("hidden") else window.More.SetAttribute("hidden","")
                        let count=window.Results.ChildElementCount
                        window.Status.TextContent<-
                            (if count=0 then "沒有符合結果" else string count+" 筆結果")+
                            (if page.ScanLimited then "（已達掃描上限，可繼續）" else ""))

        let openWindow currentBinding query =
            let key = if checkedValue multiOpen then sequence<-sequence+1;sequence else 0
            if key=0 && windows.ContainsKey key then closeWindow key
            elif key<>0 && windows.Count>=options.MaxOpenWindows then
                launcherStatus.TextContent<-"請先關閉一個搜尋結果視窗"
            if not(windows.ContainsKey key) && windows.Count<options.MaxOpenWindows then
                let card=node "article" "" |> testId ("ptcs-tag-search-window-"+string key)
                card.ClassName<-"ptcs-tag-search-card"
                card.SetAttribute("tabindex","-1")
                let header=node "header" ""
                header.ClassName<-"ptcs-tag-search-header"
                append card header
                append header (node "h3" ("搜尋："+query.Text))
                let closeButton=node "button" "×"
                closeButton.SetAttribute("type","button")
                closeButton.SetAttribute("aria-label","關閉搜尋結果")
                append header closeButton
                let context=node "small" (currentBinding.ScopeLabel+" · "+inputValue matcher+(if query.CaseSensitive then " · Aa" else ""))
                context.ClassName<-"ptcs-tag-search-context"
                append card context
                let results=node "div" ""
                results.ClassName<-"ptcs-tag-search-results"
                append card results
                let more=node "button" "載入更多"
                more.ClassName<-"ptcs-tag-search-more"
                more.SetAttribute("type","button");more.SetAttribute("hidden","")
                append card more
                let status=node "div" ""
                status.ClassName<-"ptcs-tag-search-status";status.SetAttribute("role","status")
                append card status
                let window={Key=key;Binding=currentBinding;Query=query;Card=card;Results=results;Status=status;More=more;NextCursor="";SearchGeneration=0;ActivationGeneration=0;Left=0.;Top=0.}
                closeButton.AddEventListener("mousedown",fun (event:Event)->event.StopPropagation())
                closeButton.AddEventListener("click",fun (_:Event)->closeWindow key)
                more.AddEventListener("click",fun (_:Event)->if window.NextCursor<>"" then dispatchPage window window.NextCursor true)
                card.AddEventListener("mousedown",fun (_:Event)->focusWindow key)
                header.AddEventListener("mousedown",fun (raw:Event)->
                    let event=raw :?> MouseEvent
                    if event.Button=0 then
                        let bounds=card.GetBoundingClientRect()
                        dragging<-Some(key,float event.ClientX-bounds.Left,float event.ClientY-bounds.Top)
                        event.PreventDefault())
                append layer card
                windows[key]<-window
                let offset=float(windows.Count%6*26)
                setPosition window (max 12. (JS.Get<float> "innerWidth" JS.Window-640.-offset)) (84.+offset)
                focusWindow key
                dispatchPage window "" false

        let launch () =
            launcherStatus.TextContent<-""
            match binding with
            | None -> launcherStatus.TextContent<-"尚未選擇搜尋範圍"
            | Some currentBinding ->
                let text=inputValue queryInput
                let selectedFields=fieldInputs |> Seq.choose(fun (field,input)->if checkedValue input then Some field else None)|>Seq.toArray
                if String.IsNullOrWhiteSpace text then launcherStatus.TextContent<-"請輸入搜尋文字"
                elif selectedFields.Length=0 then launcherStatus.TextContent<-"請至少選一個搜尋欄位"
                else
                    let mode=
                        match inputValue matcher with
                        | "wildcard" -> TaggingSearchMatcher.Wildcard
                        | "regex" -> TaggingSearchMatcher.Regex
                        | _ -> TaggingSearchMatcher.Literal
                    openWindow currentBinding
                        {Text=text;Matcher=mode;CaseSensitive=checkedValue caseSensitive
                         Fields=selectedFields;Limit=options.DefaultLimit;Cursor=""}

        let mouseMove (raw:Event) =
            match dragging with
            | Some(key,offsetX,offsetY) when windows.ContainsKey key ->
                let event=raw :?> MouseEvent
                let window=windows[key]
                let bounds=window.Card.GetBoundingClientRect()
                let left=max 0. (min (max 0. (JS.Get<float> "innerWidth" JS.Window-bounds.Width)) (float event.ClientX-offsetX))
                let top=max 0. (min (max 0. (JS.Get<float> "innerHeight" JS.Window-48.)) (float event.ClientY-offsetY))
                setPosition window left top
            | Some _ -> dragging<-None
            | None -> ()
        let mouseUp (_:Event) = dragging<-None
        JS.Window.AddEventListener("mousemove",mouseMove)
        JS.Window.AddEventListener("mouseup",mouseUp)
        searchButton.AddEventListener("click",fun (_:Event)->launch())
        queryInput.AddEventListener("keydown",fun (raw:Event)->let event=raw :?> KeyboardEvent in if event.Key="Enter" then event.PreventDefault();launch())

        { SetBinding=fun value->
            binding<-value
            match value with
            | Some currentBinding -> renderFieldInputs currentBinding.Fields;launcherStatus.TextContent<-""
            | None -> renderFieldInputs [||];launcherStatus.TextContent<-"尚無可搜尋範圍"
          Dispose=fun ()->
            if not disposed then
                disposed<-true
                JS.Window.RemoveEventListener("mousemove",mouseMove)
                JS.Window.RemoveEventListener("mouseup",mouseUp)
                windows.Values|>Seq.iter(fun window->if documentContains window.Card then window.Card.Remove())
                windows.Clear()
                if documentContains layer then layer.Remove()
                if documentContains launcher then launcher.Remove()
                if documentContains style then style.Remove() }
