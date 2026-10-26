namespace PulseTrade.Comm.Spa.Dynamic.Tagging.Client

open System
open System.Collections.Generic
open PulseTrade.Comm.Spa.Dynamic.Contracts
open WebSharper
open WebSharper.JavaScript
open WebSharper.JavaScript.Dom

[<JavaScript>]
type TaggingEditorTarget =
    { SpaceId: string
      TargetKind: string
      TargetId: string
      ContextLabel: string }

[<JavaScript>]
type TaggingEditorTimeFrame =
    { Scale: int
      StartKey: string
      EndKey: string }

[<JavaScript>]
type TaggingExtraInfoDraft =
    { Tags: string array
      Summary: string
      TimeFrames: TaggingEditorTimeFrame array }

[<JavaScript>]
type TaggingExtraInfo =
    { ExInfoId: string
      Target: TaggingEditorTarget
      Draft: TaggingExtraInfoDraft
      Revision: float
      CreatedAtUtc: string
      UpdatedAtUtc: string }

[<JavaScript; RequireQualifiedAccess>]
type TaggingEditorRowSeed =
    | DraftRow of TaggingExtraInfoDraft
    | PersistedRow of row: TaggingExtraInfo * draft: TaggingExtraInfoDraft

[<JavaScript; RequireQualifiedAccess>]
type TaggingEditorRequest =
    | List
    | Create of TaggingExtraInfoDraft
    | Update of exInfoId:string * expectedRevision:float * draft:TaggingExtraInfoDraft
    | Delete of exInfoId:string * expectedRevision:float
    | SuggestTags of prefix:string * limit:int

[<JavaScript; RequireQualifiedAccess>]
type TaggingEditorReply =
    | Listed of TaggingExtraInfo array
    | MutationAccepted
    | Conflict of serverCurrent:TaggingExtraInfo option
    | Suggested of string array

[<JavaScript>]
type TaggingEditorBinding =
    { Target: TaggingEditorTarget
      Dispatch: TaggingEditorRequest -> (Result<TaggingEditorReply,string> -> unit) -> unit
      Changed: TaggingExtraInfo array -> unit
      InitialDraft: TaggingExtraInfoDraft option }

[<JavaScript>]
type TaggingEditorOptions =
    { MaxOpenEditors: int
      Schema: DynamicTemplateSchema }

[<JavaScript>]
type TaggingEditorFrameInput =
    { Root: Element
      Scale: Element
      StartKey: Element
      EndKey: Element }

[<JavaScript>]
type TaggingExtraInfoView =
    { ViewKey: string
      Root: Element
      Heading: Element
      TagsHost: Element
      TagsInput: Element
      TagSuggestions: Element
      SummaryInput: Element
      FramesHost: Element
      Status: Element
      mutable Tags: string array
      mutable ExtraInfo: TaggingExtraInfo option
      mutable FrameInputs: TaggingEditorFrameInput array
      mutable InitialFingerprint: string
      mutable RequestGeneration: int
      mutable SuggestionGeneration: int
      mutable Suggestions: string array
      mutable ActiveSuggestionIndex: int
      mutable Composing: bool }

[<JavaScript>]
type TaggingEditorInstance =
    { Key: string
      Binding: TaggingEditorBinding
      Card: Element
      RowsHost: Element
      Status: Element
      DirtyPrompt: Element
      mutable ExtraInfos: TaggingExtraInfo array
      mutable ExtraInfoViews: TaggingExtraInfoView array
      mutable PendingInitialDraft: TaggingExtraInfoDraft option
      mutable ListGeneration: int
      mutable Left: float
      mutable Top: float
      mutable ZIndex: int }

[<JavaScript>]
type TaggingEditorHandle =
    { Open: TaggingEditorBinding -> unit
      Close: TaggingEditorTarget -> unit
      Focus: TaggingEditorTarget -> unit
      Dispose: unit -> unit }

[<JavaScript>]
module TaggingEditorPlugin =
    let node tag text =
        let element = JS.Document.CreateElement tag
        element.TextContent <- text
        element

    let append (parent: Node) (child: Node) = parent.AppendChild child |> ignore
    let clear (parent: Element) = while not(isNull parent.FirstChild) do parent.RemoveChild(parent.FirstChild) |> ignore
    let testId value (element: Element) = element.SetAttribute("data-testid",value);element
    let inputValue (element: Element) = JS.Get<string> "value" element
    let setInputValue text (element: Element) = JS.Set element "value" text
    let setStyle name value (element: Element) = JS.Set (JS.Get<obj> "style" element) name value
    let documentContains (element: Element) = JS.Document.Contains element

    let emptyDraft = { Tags=[||];Summary="";TimeFrames=[||] }

    let defaultSchema =
        { TemplateKey="tagging.extra.v1"
          DisplayName="Tagging extra info"
          SchemaRevision=2L
          Fields=
            [| { Key="tags";Label="Tags"
                 Kind=EditorValueKind.List(EditorValueKind.Text,Some 0,Some 32)
                 Required=false;DefaultValue=Some(SduiValue.Array [||]) }
               { Key="summary";Label="Summary";Kind=EditorValueKind.Text
                 Required=false;DefaultValue=Some(SduiValue.Text "") }
               { Key="timeFrames";Label="Time frames"
                 Kind=
                    EditorValueKind.List(
                        EditorValueKind.Group
                            [| { Key="scale";Label="Scale";Kind=EditorValueKind.Integer(Some 1L,Some 32767L)
                                 Required=true;DefaultValue=Some(SduiValue.Number 1.) }
                               { Key="startKey";Label="Start";Kind=EditorValueKind.Text
                                 Required=true;DefaultValue=Some(SduiValue.Text "") }
                               { Key="endKey";Label="Optional end";Kind=EditorValueKind.Text
                                 Required=false;DefaultValue=Some(SduiValue.Text "") } |],
                        Some 0,Some 32)
                 Required=false;DefaultValue=Some(SduiValue.Array [||]) } |] }

    let defaultOptions = { MaxOpenEditors=8;Schema=defaultSchema }

    let styleText =
        """
.ptcs-tagging-layer{position:fixed;inset:0;z-index:60;pointer-events:none}.ptcs-tagging-card{position:fixed;width:min(540px,calc(100vw - 32px));max-height:min(78vh,760px);overflow:auto;pointer-events:auto;background:#fff;color:#17212b;border:1px solid #9aa8b8;border-radius:12px;box-shadow:0 18px 54px #17202a52;padding:0 12px 12px;box-sizing:border-box}.ptcs-tagging-header{position:sticky;top:0;z-index:3;display:flex;align-items:center;gap:8px;background:#fff;padding:10px 0 8px;cursor:move;border-top:4px solid #2d5f9a;border-bottom:2px solid #9eacba}.ptcs-tagging-heading{min-width:0;flex:1}.ptcs-tagging-heading h3{margin:0;font-size:17px;font-weight:800;overflow-wrap:anywhere}.ptcs-tagging-context{display:block;color:#617083;font-size:12px;font-weight:600;overflow-wrap:anywhere}.ptcs-tagging-header button{flex:0 0 auto}.ptcs-tagging-row{margin-top:10px;padding:10px;border:1px solid #cbd4dd;border-radius:9px;background:#fbfcfd}.ptcs-tagging-row h4{margin:0 0 5px;font-size:12px;color:#536173;overflow-wrap:anywhere}.ptcs-tagging-row label{display:block;margin-top:6px}.ptcs-tagging-row input,.ptcs-tagging-row textarea{box-sizing:border-box;width:100%;padding:7px;margin:3px 0 6px}.ptcs-tagging-row textarea{min-height:86px;resize:vertical}.ptcs-tagging-chip-editor{position:relative;display:flex;flex-wrap:wrap;align-items:center;gap:5px;min-height:39px;margin:3px 0 6px;padding:5px;border:1px solid #b8c2cc;border-radius:7px;background:#fff}.ptcs-tagging-chip-editor:focus-within{outline:2px solid #3978c5;outline-offset:1px}.ptcs-tagging-chips{display:contents}.ptcs-tagging-chip{display:inline-flex;align-items:center;gap:4px;padding:3px 7px;border-radius:12px;background:#e7f0fb;color:#204f84}.ptcs-tagging-chip button{border:0;background:transparent;padding:0;color:inherit;cursor:pointer}.ptcs-tagging-tag-input{flex:1 1 110px!important;min-width:90px;border:0!important;outline:0!important;margin:0!important;padding:4px!important}.ptcs-tagging-suggestions{position:absolute;left:0;right:0;top:calc(100% + 2px);z-index:6;display:grid;max-height:180px;overflow:auto;padding:4px;background:#fff;border:1px solid #aeb8c2;border-radius:7px;box-shadow:0 8px 22px #17202a2e}.ptcs-tagging-suggestions[hidden]{display:none}.ptcs-tagging-suggestions button{text-align:left;border:0;background:#fff;padding:7px}.ptcs-tagging-suggestions button:hover,.ptcs-tagging-suggestions button:focus{background:#e7f0fb}.ptcs-tagging-frames{display:grid;gap:5px}.ptcs-tagging-frame{display:grid;grid-template-columns:64px 1fr 1fr auto;gap:5px;align-items:center}.ptcs-tagging-frame input{margin:0}.ptcs-tagging-actions{display:flex;flex-wrap:nowrap;gap:7px;margin-top:8px;overflow-x:auto}.ptcs-tagging-actions button{white-space:nowrap}.ptcs-tagging-status{min-height:19px;color:#495869}.ptcs-tagging-status.error{color:#a31d1d}.ptcs-tagging-dirty{margin:8px 0;padding:9px;border:1px solid #c48b1a;background:#fff7df;border-radius:8px}.ptcs-tagging-dirty[hidden]{display:none}.ptcs-tagging-tabs{display:none}
@media(max-width:720px){.ptcs-tagging-tabs{position:fixed;display:flex;pointer-events:auto;left:0;right:0;bottom:min(82vh,700px);overflow-x:auto;gap:5px;padding:5px 8px;background:#e8edf2;border-top:1px solid #aeb8c2}.ptcs-tagging-tabs button{white-space:nowrap}.ptcs-tagging-card{display:none;left:0!important;right:0;top:auto!important;bottom:0;width:100%;max-height:min(82vh,700px);border-radius:14px 14px 0 0;padding-bottom:max(12px,env(safe-area-inset-bottom))}.ptcs-tagging-card[data-active=true]{display:block}.ptcs-tagging-header{cursor:default}.ptcs-tagging-frame{grid-template-columns:56px 1fr}.ptcs-tagging-frame input:nth-child(3){grid-column:1/3}.ptcs-tagging-frame button{grid-column:2}}
"""

    let targetKey (target: TaggingEditorTarget) =
        let segment (value: string) = let text=if isNull value then "" else value in string text.Length+":"+text
        segment target.SpaceId + segment target.TargetKind + segment target.TargetId

    let displayLabel target =
        if String.IsNullOrWhiteSpace target.ContextLabel then target.TargetId else target.ContextLabel.Trim()

    let orderedRowSeeds dirtyDrafts persistedRows =
        Array.append
            (dirtyDrafts |> Array.map TaggingEditorRowSeed.DraftRow)
            (persistedRows |> Array.map TaggingEditorRowSeed.PersistedRow)

    let saveRequest row draft =
        match row with
        | None -> TaggingEditorRequest.Create draft
        | Some current -> TaggingEditorRequest.Update(current.ExInfoId,current.Revision,draft)

    let deleteRequest row =
        row
        |> Option.map (fun current -> TaggingEditorRequest.Delete(current.ExInfoId,current.Revision))

    let fieldLabel (options: TaggingEditorOptions) (key: string) (fallback: string) =
        options.Schema.Fields
        |> Array.tryFind (fun field -> field.Key=key)
        |> Option.map _.Label
        |> Option.filter (String.IsNullOrWhiteSpace >> not)
        |> Option.defaultValue fallback

    let canonicalClientTag (value: string) = if isNull value then "" else value.Trim()
    let sameTag (left: string) (right: string) = left.ToUpper()=right.ToUpper()
    let digits12 (value: string) =
        not(isNull value) && value.Length=12 && (value |> Seq.forall (fun c -> c>='0' && c<='9'))

    let validateOptions (options: TaggingEditorOptions) : Result<TaggingEditorOptions,string> =
        if options.MaxOpenEditors<1 || options.MaxOpenEditors>12 then Result.Error "tagging-editor-max-open-invalid"
        else
            match DynamicEditorValidation.validateSchema DynamicEditorDefaults.limits options.Schema with
            | Result.Error _ -> Result.Error "tagging-editor-schema-invalid"
            | Result.Ok _ ->
                let fields = options.Schema.Fields |> Array.map _.Key |> Set.ofArray
                if Set.isSubset (set ["tags";"summary";"timeFrames"]) fields then Result.Ok options
                else Result.Error "tagging-editor-schema-fields-missing"

    let mount (host: Element) (inputOptions: TaggingEditorOptions) =
        let options = validateOptions inputOptions |> function Result.Ok value -> value | Result.Error error -> invalidOp error
        let style = node "style" styleText
        append host style
        let layer = node "section" "" |> testId "ptcs-tagging-editor-layer"
        layer.ClassName <- "ptcs-tagging-layer"
        append host layer
        let tabs = node "nav" "" |> testId "ptcs-tagging-editor-tabs"
        tabs.ClassName <- "ptcs-tagging-tabs"
        append layer tabs
        let instances = Dictionary<string,TaggingEditorInstance>()
        let mutable openOrder: string list = []
        let mutable activeKey = ""
        let mutable disposed = false
        let mutable zSequence = 60
        let mutable rowSequence = 0
        let mutable dragging: (string*float*float) option = None

        let setPosition (instance: TaggingEditorInstance) (left: float) (top: float) =
            instance.Left <- left
            instance.Top <- top
            setStyle "left" (string left+"px") instance.Card
            setStyle "top" (string top+"px") instance.Card

        let rec focusKey key =
            match instances.TryGetValue key with
            | true,instance ->
                zSequence <- zSequence+1
                instance.ZIndex <- zSequence
                setStyle "z-index" (string zSequence) instance.Card
                activeKey <- key
                instances.Values |> Seq.iter (fun item -> item.Card.SetAttribute("data-active",if item.Key=key then "true" else "false"))
                clear tabs
                openOrder |> List.iter (fun itemKey ->
                    match instances.TryGetValue itemKey with
                    | true,item ->
                        let button = node "button" (displayLabel item.Binding.Target)
                        button.SetAttribute("type","button")
                        button.SetAttribute("aria-current",if itemKey=key then "true" else "false")
                        button.AddEventListener("click",fun (_: Event) -> focusKey itemKey)
                        append tabs button
                    | _ -> ())
            | _ -> ()

        let removeInstance (key: string) =
            match instances.TryGetValue key with
            | true,instance ->
                if documentContains instance.Card then instance.Card.Remove()
                instances.Remove key |> ignore
                openOrder <- openOrder |> List.filter ((<>) key)
                if activeKey=key then activeKey <- openOrder |> List.tryLast |> Option.defaultValue ""
                if activeKey<>"" then focusKey activeKey else clear tabs
            | _ -> ()

        let addFrame (view: TaggingExtraInfoView) scaleValue startValue endValue =
            let root = node "div" ""
            root.ClassName <- "ptcs-tagging-frame"
            let scale = node "input" ""
            scale.SetAttribute("type","number");scale.SetAttribute("min","1");scale.SetAttribute("max","32767");scale.SetAttribute("aria-label","Scale")
            setInputValue scaleValue scale
            let startKey = node "input" ""
            startKey.SetAttribute("inputmode","numeric");startKey.SetAttribute("placeholder","yyyyMMddHHmm");startKey.SetAttribute("aria-label","Start")
            setInputValue startValue startKey
            let endKey = node "input" ""
            endKey.SetAttribute("inputmode","numeric");endKey.SetAttribute("placeholder","optional end");endKey.SetAttribute("aria-label","End")
            setInputValue endValue endKey
            let remove = node "button" "×"
            remove.SetAttribute("type","button");remove.SetAttribute("aria-label","Remove time frame")
            remove.AddEventListener("click",fun (_: Event) -> view.FrameInputs<-view.FrameInputs|>Array.filter(fun candidate -> not(obj.ReferenceEquals(candidate.Root,root)));if documentContains root then root.Remove())
            [|scale;startKey;endKey;remove|] |> Array.iter (append root)
            append view.FramesHost root
            view.FrameInputs <- Array.append view.FrameInputs [|{Root=root;Scale=scale;StartKey=startKey;EndKey=endKey}|]

        let rec renderTags (view: TaggingExtraInfoView) =
            clear view.TagsHost
            view.Tags |> Array.iter (fun tag ->
                let chip = node "span" ""
                chip.ClassName <- "ptcs-tagging-chip"
                append chip (node "span" tag)
                let remove = node "button" "×"
                remove.SetAttribute("type","button")
                remove.SetAttribute("aria-label","Remove tag "+tag)
                remove.AddEventListener("click",fun (_: Event) -> view.Tags<-view.Tags|>Array.filter(fun item -> not(sameTag item tag));renderTags view)
                append chip remove
                append view.TagsHost chip)

        let addTag (view: TaggingExtraInfoView) (raw: string) =
            let tag = canonicalClientTag raw
            if tag<>"" && tag.Length<=128 && not(view.Tags |> Array.exists (sameTag tag)) then
                view.Tags <- Array.append view.Tags [|tag|]
                renderTags view

        let commitTagInput (view: TaggingExtraInfoView) =
            (inputValue view.TagsInput).Split([|',';' '|],StringSplitOptions.RemoveEmptyEntries)
            |> Array.iter (addTag view)
            setInputValue "" view.TagsInput
            view.Suggestions <- [||]
            view.ActiveSuggestionIndex <- -1
            clear view.TagSuggestions
            view.TagSuggestions.SetAttribute("hidden","")

        let hideTagSuggestions (view: TaggingExtraInfoView) =
            view.Suggestions <- [||]
            view.ActiveSuggestionIndex <- -1
            clear view.TagSuggestions
            view.TagSuggestions.SetAttribute("hidden","")

        let applyTagSuggestion (view: TaggingExtraInfoView) (tag: string) =
            addTag view tag
            setInputValue "" view.TagsInput
            hideTagSuggestions view

        let renderTagSuggestions (view: TaggingExtraInfoView) =
            clear view.TagSuggestions
            if view.Suggestions.Length=0 then
                view.TagSuggestions.SetAttribute("hidden","")
            else
                view.Suggestions |> Array.iteri(fun index tag ->
                    let button=node "button" tag
                    button.SetAttribute("type","button")
                    button.SetAttribute("role","option")
                    button.SetAttribute("aria-selected",if index=view.ActiveSuggestionIndex then "true" else "false")
                    if index=view.ActiveSuggestionIndex then
                        setStyle "background" "#2d67b1" button
                        setStyle "color" "#fff" button
                    button.AddEventListener("mousedown",fun (event: Event) -> event.PreventDefault())
                    button.AddEventListener("click",fun (_: Event) -> applyTagSuggestion view tag)
                    append view.TagSuggestions button)
                view.TagSuggestions.RemoveAttribute("hidden")

        let moveTagSuggestionSelection (view: TaggingExtraInfoView) delta =
            if view.Suggestions.Length>0 then
                let count=view.Suggestions.Length
                view.ActiveSuggestionIndex <- (view.ActiveSuggestionIndex+delta+count)%count
                renderTagSuggestions view

        let draftFromView (view: TaggingExtraInfoView) =
            let frames =
                view.FrameInputs
                |> Array.map (fun input ->
                    let mutable scale = 0
                    Int32.TryParse(inputValue input.Scale,&scale) |> ignore
                    {Scale=scale;StartKey=inputValue input.StartKey;EndKey=inputValue input.EndKey})
            {Tags=view.Tags;Summary=inputValue view.SummaryInput;TimeFrames=frames}

        let draftFingerprint (draft: TaggingExtraInfoDraft) = JSON.Stringify draft
        let isRowDirty (view: TaggingExtraInfoView) =
            match view.ExtraInfo with
            | Some _ -> view.InitialFingerprint<>"" && draftFingerprint(draftFromView view)<>view.InitialFingerprint
            | None -> true

        let validateView (view: TaggingExtraInfoView) =
            view.Status.ClassName <- "ptcs-tagging-status"
            let mutable result: string option = None
            for input in view.FrameInputs do
                if result.IsNone then
                    let mutable scale = 0
                    if not(Int32.TryParse(inputValue input.Scale,&scale)) || scale < 1 || scale > 32767 then result<-Some "Scale must be between 1 and 32767."
                    else
                        let startKey=inputValue input.StartKey
                        let endKey=inputValue input.EndKey
                        if not(digits12 startKey) then result<-Some "Start must be 12 digits (yyyyMMddHHmm)."
                        elif endKey<>"" && not(digits12 endKey) then result<-Some "End must be empty or 12 digits (yyyyMMddHHmm)."
                        elif endKey<>"" && endKey.CompareTo(startKey)<0 then result<-Some "End must not be before Start."
            match result with
            | Some error -> view.Status.ClassName<-"ptcs-tagging-status error";view.Status.TextContent<-error;false
            | None -> true

        let removeRowView (instance: TaggingEditorInstance) (view: TaggingExtraInfoView) =
            instance.ExtraInfoViews <- instance.ExtraInfoViews |> Array.filter(fun item -> not(obj.ReferenceEquals(item,view)))
            if documentContains view.Root then view.Root.Remove()

        let rec dispatchList (instance: TaggingEditorInstance) (finalStatus: string) =
            instance.ListGeneration <- instance.ListGeneration+1
            let generation = instance.ListGeneration
            instance.Status.TextContent <- "Loading ExtraInfo…"
            instance.Binding.Dispatch TaggingEditorRequest.List (fun result ->
                match instances.TryGetValue instance.Key with
                | true,current when not disposed && obj.ReferenceEquals(current,instance) && generation=instance.ListGeneration ->
                    match result with
                    | Result.Ok(TaggingEditorReply.Listed rows) ->
                        let dirtyExisting = Dictionary<string,TaggingExtraInfoDraft>()
                        let dirtyNew = ResizeArray<TaggingExtraInfoDraft>()
                        instance.ExtraInfoViews |> Array.iter(fun view -> if isRowDirty view then match view.ExtraInfo with Some row->dirtyExisting[row.ExInfoId]<-draftFromView view | None->dirtyNew.Add(draftFromView view))
                        instance.ExtraInfos <- if isNull rows then [||] else rows
                        instance.Binding.Changed instance.ExtraInfos
                        clear instance.RowsHost
                        instance.ExtraInfoViews <- [||]
                        let persistedRows =
                            instance.ExtraInfos
                            |> Array.map(fun row ->
                                let draft = match dirtyExisting.TryGetValue row.ExInfoId with true,value->value | _->row.Draft
                                row,draft)
                        let pendingDrafts =
                            let drafts = dirtyNew.ToArray()
                            match instance.PendingInitialDraft with
                            | Some draft ->
                                instance.PendingInitialDraft <- None
                                Array.append drafts [|draft|]
                            | None -> drafts
                        orderedRowSeeds pendingDrafts persistedRows
                        |> Array.iter(function
                            | TaggingEditorRowSeed.DraftRow draft ->
                                instance.ExtraInfoViews <- Array.append instance.ExtraInfoViews [|createRowView instance None draft|]
                            | TaggingEditorRowSeed.PersistedRow(row,draft) ->
                                instance.ExtraInfoViews <- Array.append instance.ExtraInfoViews [|createRowView instance (Some row) draft|])
                        instance.Status.TextContent <- if finalStatus<>"" then finalStatus elif instance.ExtraInfoViews.Length=0 then "No ExtraInfo. Use New ExtraInfo to add one." else ""
                    | Result.Ok _ -> instance.Status.TextContent <- "tagging-editor-list-reply-invalid"
                    | Result.Error error -> instance.Status.TextContent <- error
                | _ -> ())

        and dispatchMutation (instance: TaggingEditorInstance) (view: TaggingExtraInfoView) (request: TaggingEditorRequest) =
            view.RequestGeneration <- view.RequestGeneration+1
            let generation = view.RequestGeneration
            view.Status.TextContent <- "Saving…"
            instance.Binding.Dispatch request (fun result ->
                match instances.TryGetValue instance.Key with
                | true,current when not disposed && obj.ReferenceEquals(current,instance) && instance.ExtraInfoViews|>Array.exists(fun item->obj.ReferenceEquals(item,view)) && generation=view.RequestGeneration ->
                    match result with
                    | Result.Ok(TaggingEditorReply.Conflict serverCurrent) ->
                        view.Status.TextContent <- "Revision conflict. The local draft was kept."
                        match serverCurrent with Some row->view.ExtraInfo<-Some row;view.Heading.TextContent<-row.ExInfoId | None->()
                    | Result.Ok TaggingEditorReply.MutationAccepted ->
                        match request with
                        | TaggingEditorRequest.Create _ -> removeRowView instance view
                        | _ -> ()
                        dispatchList instance ""
                    | Result.Ok _ -> view.Status.TextContent <- "tagging-editor-mutation-reply-invalid"
                    | Result.Error error -> view.Status.TextContent <- error
                | _ -> ())

        and requestSuggestions (instance: TaggingEditorInstance) (view: TaggingExtraInfoView) =
            let prefix = canonicalClientTag(inputValue view.TagsInput)
            view.SuggestionGeneration <- view.SuggestionGeneration+1
            let generation = view.SuggestionGeneration
            hideTagSuggestions view
            if prefix="" then ()
            else
                instance.Binding.Dispatch (TaggingEditorRequest.SuggestTags(prefix,8)) (fun result ->
                    if not disposed && generation=view.SuggestionGeneration && documentContains view.Root then
                        match result with
                        | Result.Ok(TaggingEditorReply.Suggested items) ->
                            view.Suggestions <-
                                (if isNull items then [||] else items)
                                |> Array.filter(fun tag -> not(view.Tags|>Array.exists(sameTag tag)))
                                |> Array.truncate 8
                            view.ActiveSuggestionIndex <- if view.Suggestions.Length=0 then -1 else 0
                            renderTagSuggestions view
                        | _ -> hideTagSuggestions view)

        and saveAll (instance: TaggingEditorInstance) =
            let dirtyViews = instance.ExtraInfoViews |> Array.filter isRowDirty
            if dirtyViews.Length=0 then instance.Status.TextContent<-"All ExtraInfo is already saved."
            else
                instance.Status.TextContent <- "Saving all ExtraInfo…"
                let mutable issueCount = 0
                let rec saveAt index =
                    if index>=dirtyViews.Length then dispatchList instance (if issueCount=0 then "All ExtraInfo saved." else string issueCount+" ExtraInfo item(s) need attention; their drafts were kept.")
                    else
                        let view=dirtyViews[index]
                        if not(validateView view) then issueCount<-issueCount+1;saveAt(index+1)
                        else
                            let draft=draftFromView view
                            let request=saveRequest view.ExtraInfo draft
                            view.RequestGeneration<-view.RequestGeneration+1
                            let generation=view.RequestGeneration
                            view.Status.TextContent<-"Saving…"
                            instance.Binding.Dispatch request (fun result ->
                                if disposed || generation<>view.RequestGeneration then issueCount<-issueCount+1;saveAt(index+1)
                                else
                                    match result with
                                    | Result.Ok TaggingEditorReply.MutationAccepted ->
                                        match request with
                                        | TaggingEditorRequest.Create _ -> removeRowView instance view
                                        | _ -> ()
                                        saveAt(index+1)
                                    | Result.Ok(TaggingEditorReply.Conflict current) ->
                                        issueCount<-issueCount+1;view.Status.TextContent<-"Revision conflict. The local draft was kept."
                                        match current with Some row->view.ExtraInfo<-Some row;view.Heading.TextContent<-row.ExInfoId | None->()
                                        saveAt(index+1)
                                    | Result.Ok _ -> issueCount<-issueCount+1;view.Status.TextContent<-"tagging-editor-mutation-reply-invalid";saveAt(index+1)
                                    | Result.Error error -> issueCount<-issueCount+1;view.Status.TextContent<-error;saveAt(index+1))
                saveAt 0

        and createRowView (instance: TaggingEditorInstance) (row: TaggingExtraInfo option) (draft: TaggingExtraInfoDraft) =
            let root=node "section" ""
            root.ClassName<-"ptcs-tagging-row"
            let heading=node "h4" ""
            append root heading
            let actions=node "div" ""
            actions.ClassName<-"ptcs-tagging-actions"
            append root actions
            let action text = let button=node "button" text in button.SetAttribute("type","button");append actions button;button
            let addFrameButton=action "新增時框"
            let saveButton=action "儲存"
            let deleteButton=action "刪除"
            append root (node "label" (fieldLabel options "tags" "Tags"))
            let tagEditor=node "div" ""
            tagEditor.ClassName<-"ptcs-tagging-chip-editor"
            let tagsHost=node "span" ""
            tagsHost.ClassName<-"ptcs-tagging-chips"
            append tagEditor tagsHost
            let tagsInput=node "input" ""
            tagsInput.ClassName<-"ptcs-tagging-tag-input"
            tagsInput.SetAttribute("placeholder","新增 tag")
            tagsInput.SetAttribute("aria-label","Tag input")
            append tagEditor tagsInput
            let suggestions=node "div" ""
            suggestions.ClassName<-"ptcs-tagging-suggestions"
            suggestions.SetAttribute("hidden","")
            suggestions.SetAttribute("role","listbox")
            append tagEditor suggestions
            append root tagEditor
            append root (node "label" (fieldLabel options "summary" "Summary"))
            let summary=node "textarea" ""
            append root summary
            append root (node "label" (fieldLabel options "timeFrames" "Time frames"))
            let frames=node "div" ""
            frames.ClassName<-"ptcs-tagging-frames"
            append root frames
            let status=node "div" ""
            status.ClassName<-"ptcs-tagging-status";status.SetAttribute("role","status")
            append root status
            rowSequence<-rowSequence+1
            let view=
                { ViewKey="row-"+string rowSequence;Root=root;Heading=heading;TagsHost=tagsHost;TagsInput=tagsInput;TagSuggestions=suggestions
                  SummaryInput=summary;FramesHost=frames;Status=status;Tags=[||];ExtraInfo=row;FrameInputs=[||]
                  InitialFingerprint="";RequestGeneration=0;SuggestionGeneration=0;Suggestions=[||];ActiveSuggestionIndex=(-1);Composing=false }
            view.Heading.TextContent<-match row with Some value->value.ExInfoId | None->"DRAFT RECORD"
            view.Tags<-if isNull draft.Tags then [||] else draft.Tags
            renderTags view
            setInputValue draft.Summary summary
            draft.TimeFrames |> Array.iter(fun frame->addFrame view (string frame.Scale) frame.StartKey frame.EndKey)
            view.InitialFingerprint<-match row with Some _->draftFingerprint(draftFromView view)|None->"__new__"
            tagsInput.AddEventListener("compositionstart",fun (_: Event)->view.Composing<-true)
            tagsInput.AddEventListener("compositionend",fun (_: Event)->view.Composing<-false;requestSuggestions instance view)
            tagsInput.AddEventListener("input",fun (_: Event)->if not view.Composing then requestSuggestions instance view)
            tagsInput.AddEventListener("keydown",fun (raw: Event)->
                let event=raw :?> KeyboardEvent
                if not view.Composing then
                    if event.Key="ArrowDown" && view.Suggestions.Length>0 then event.PreventDefault();moveTagSuggestionSelection view 1
                    elif event.Key="ArrowUp" && view.Suggestions.Length>0 then event.PreventDefault();moveTagSuggestionSelection view -1
                    elif event.Key="Enter" && view.ActiveSuggestionIndex>=0 && view.ActiveSuggestionIndex<view.Suggestions.Length then
                        event.PreventDefault();applyTagSuggestion view view.Suggestions[view.ActiveSuggestionIndex]
                    elif event.Key=" " || event.Key="Spacebar" || event.Key="Enter" || event.Key="," then event.PreventDefault();commitTagInput view
                    elif event.Key="Escape" && view.Suggestions.Length>0 then
                        event.PreventDefault()
                        event.StopPropagation()
                        hideTagSuggestions view
                    elif event.Key="Backspace" && inputValue tagsInput="" && view.Tags.Length>0 then view.Tags<-view.Tags[0..view.Tags.Length-2];renderTags view)
            addFrameButton.AddEventListener("click",fun (_: Event)->addFrame view "1" "" "")
            saveButton.AddEventListener("click",fun (_: Event)->
                commitTagInput view
                if validateView view then
                    let value=draftFromView view
                    dispatchMutation instance view (saveRequest view.ExtraInfo value))
            deleteButton.AddEventListener("click",fun (_: Event)->
                match deleteRequest view.ExtraInfo with
                | Some request -> dispatchMutation instance view request
                | None -> removeRowView instance view)
            append instance.RowsHost root
            view

        let mouseMove (raw: Event) =
            match dragging with
            | Some(key,offsetX,offsetY) ->
                match instances.TryGetValue key with
                | true,instance ->
                    let event=raw :?> MouseEvent
                    let bounds=instance.Card.GetBoundingClientRect()
                    let left=max 0. (min (max 0. (JS.Get<float> "innerWidth" JS.Window-bounds.Width)) (float event.ClientX-offsetX))
                    let top=max 0. (min (max 0. (JS.Get<float> "innerHeight" JS.Window-48.)) (float event.ClientY-offsetY))
                    setPosition instance left top
                | _ -> dragging<-None
            | None -> ()
        let mouseUp (_: Event) = dragging<-None
        JS.Window.AddEventListener("mousemove",mouseMove)
        JS.Window.AddEventListener("mouseup",mouseUp)

        let openTarget (binding: TaggingEditorBinding) =
            if not disposed then
                let key=targetKey binding.Target
                match instances.TryGetValue key with
                | true,_ -> focusKey key
                | _ when instances.Count>=options.MaxOpenEditors ->
                    if activeKey<>"" then
                        focusKey activeKey
                        match instances.TryGetValue activeKey with
                        | true,item -> item.Status.TextContent<-"Close an editor before opening another target."
                        | _ -> ()
                | _ ->
                    let card=node "article" "" |> testId ("ptcs-tagging-editor-"+string instances.Count)
                    card.ClassName<-"ptcs-tagging-card"
                    card.SetAttribute("data-target-id",binding.Target.TargetId)
                    card.SetAttribute("tabindex","-1")
                    let dirtyPrompt=node "div" ""
                    dirtyPrompt.ClassName<-"ptcs-tagging-dirty"
                    dirtyPrompt.SetAttribute("hidden","")
                    append dirtyPrompt (node "strong" "One or more ExtraInfo drafts have not been saved.")
                    append card dirtyPrompt
                    let header=node "header" ""
                    header.ClassName<-"ptcs-tagging-header"
                    append card header
                    let heading=node "div" ""
                    heading.ClassName<-"ptcs-tagging-heading"
                    append heading (node "h3" (displayLabel binding.Target))
                    let context=node "small" ("Target ID: "+binding.Target.TargetId)
                    context.ClassName<-"ptcs-tagging-context"
                    append heading context
                    append header heading
                    let newButton=node "button" "新增 ExtraInfo"
                    newButton.SetAttribute("type","button")
                    append header newButton
                    let saveAllButton=node "button" "全部儲存"
                    saveAllButton.SetAttribute("type","button")
                    append header saveAllButton
                    let closeButton=node "button" "×"
                    closeButton.SetAttribute("type","button")
                    closeButton.SetAttribute("aria-label","Close ExtraInfo editor for "+displayLabel binding.Target)
                    append header closeButton
                    let rowsHost=node "div" ""
                    append card rowsHost
                    let status=node "div" ""
                    status.ClassName<-"ptcs-tagging-status"
                    status.SetAttribute("role","status")
                    append card status
                    let instance={Key=key;Binding=binding;Card=card;RowsHost=rowsHost;Status=status;DirtyPrompt=dirtyPrompt;ExtraInfos=[||];ExtraInfoViews=[||];PendingInitialDraft=binding.InitialDraft;ListGeneration=0;Left=0.;Top=0.;ZIndex=0}
                    let dirtyAction text (handler: Event->unit) =
                        let button=node "button" text
                        button.SetAttribute("type","button")
                        button.AddEventListener("click",handler)
                        append dirtyPrompt button
                    dirtyAction "DISCARD ALL AND CLOSE" (fun (_: Event)->removeInstance key)
                    dirtyAction "STAY" (fun (_: Event)->dirtyPrompt.SetAttribute("hidden",""))
                    [|newButton;saveAllButton;closeButton|] |> Array.iter(fun button->button.AddEventListener("mousedown",fun (event: Event)->event.StopPropagation()))
                    newButton.AddEventListener("click",fun (_: Event)->
                        let view = createRowView instance None emptyDraft
                        if not(isNull view.Root.PreviousSibling) then
                            instance.RowsHost.InsertBefore(view.Root,instance.RowsHost.FirstChild) |> ignore
                        instance.ExtraInfoViews <- Array.append [|view|] instance.ExtraInfoViews
                        instance.Status.TextContent<-"")
                    saveAllButton.AddEventListener("click",fun (_: Event)->saveAll instance)
                    closeButton.AddEventListener("click",fun (_: Event)->if instance.ExtraInfoViews|>Array.exists isRowDirty then dirtyPrompt.RemoveAttribute("hidden") else removeInstance key)
                    card.AddEventListener("mousedown",fun (_: Event)->focusKey key)
                    header.AddEventListener("mousedown",fun (raw: Event)->
                        let event=raw :?> MouseEvent
                        if event.Button=0 then
                            let bounds=card.GetBoundingClientRect()
                            dragging<-Some(key,float event.ClientX-bounds.Left,float event.ClientY-bounds.Top)
                            event.PreventDefault())
                    card.AddEventListener("keydown",fun (raw: Event)->
                        let event=raw :?> KeyboardEvent
                        if event.Key="Escape" then
                            event.PreventDefault()
                            if instance.ExtraInfoViews|>Array.exists isRowDirty then dirtyPrompt.RemoveAttribute("hidden") else removeInstance key)
                    append layer card
                    let offset=float(instances.Count%5*28)
                    let left=max 12. (JS.Get<float> "innerWidth" JS.Window-560.-offset)
                    setPosition instance left (82.+offset)
                    instances[key]<-instance
                    openOrder<-List.append openOrder [key]
                    focusKey key
                    dispatchList instance ""

        { Open=openTarget
          Close=fun target->
            let key=targetKey target
            match instances.TryGetValue key with
            | true,instance when instance.ExtraInfoViews|>Array.exists isRowDirty -> focusKey key;instance.DirtyPrompt.RemoveAttribute("hidden")
            | true,_ -> removeInstance key
            | _ -> ()
          Focus=targetKey>>focusKey
          Dispose=fun ()->
            if not disposed then
                disposed<-true
                JS.Window.RemoveEventListener("mousemove",mouseMove)
                JS.Window.RemoveEventListener("mouseup",mouseUp)
                instances.Values|>Seq.iter(fun item->if documentContains item.Card then item.Card.Remove())
                instances.Clear()
                openOrder<-[]
                if documentContains layer then layer.Remove()
                if documentContains style then style.Remove() }
