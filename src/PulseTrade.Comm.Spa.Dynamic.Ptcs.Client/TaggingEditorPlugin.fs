namespace PulseTrade.Comm.Spa.Dynamic.Ptcs.Client

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
type TaggingEditorRequest =
    | List
    | Create of TaggingExtraInfoDraft
    | Update of exInfoId: string * expectedRevision: float * draft: TaggingExtraInfoDraft
    | Delete of exInfoId: string * expectedRevision: float

[<JavaScript; RequireQualifiedAccess>]
type TaggingEditorReply =
    | Listed of TaggingExtraInfo array
    | MutationAccepted
    | Conflict of serverCurrent: TaggingExtraInfo option

[<JavaScript>]
type TaggingEditorBinding =
    { Target: TaggingEditorTarget
      Dispatch: TaggingEditorRequest -> (Result<TaggingEditorReply, string> -> unit) -> unit }

[<JavaScript>]
type TaggingEditorOptions =
    { MaxOpenEditors: int
      Schema: DynamicTemplateSchema }

[<JavaScript>]
type TaggingEditorHandle =
    { Open: TaggingEditorBinding -> unit
      Close: TaggingEditorTarget -> unit
      Focus: TaggingEditorTarget -> unit
      Dispose: unit -> unit }

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
      TagsInput: Element
      SummaryInput: Element
      FramesHost: Element
      Status: Element
      mutable ExtraInfo: TaggingExtraInfo option
      mutable FrameInputs: TaggingEditorFrameInput array
      mutable InitialFingerprint: string
      mutable RequestGeneration: int }

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
      mutable ListGeneration: int
      mutable Left: float
      mutable Top: float
      mutable ZIndex: int }

[<JavaScript; RequireQualifiedAccess>]
module TaggingEditorPlugin =
    let emptyDraft =
        { Tags = [||]
          Summary = ""
          TimeFrames = [||] }

    let defaultSchema =
        { TemplateKey = "tagging.extra.v1"
          DisplayName = "Tagging extra info"
          SchemaRevision = 1L
          Fields =
            [| { Key = "tags"
                 Label = "Tags"
                 Kind = EditorValueKind.List(EditorValueKind.Text, Some 0, Some 32)
                 Required = false
                 DefaultValue = Some(SduiValue.Array [||]) }
               { Key = "summary"
                 Label = "Summary"
                 Kind = EditorValueKind.Text
                 Required = false
                 DefaultValue = Some(SduiValue.Text "") }
               { Key = "timeFrames"
                 Label = "Time frames"
                 Kind =
                    EditorValueKind.List(
                        EditorValueKind.Group
                            [| { Key = "scale"
                                 Label = "Scale"
                                 Kind = EditorValueKind.Integer(Some -128L, Some 127L)
                                 Required = true
                                 DefaultValue = Some(SduiValue.Number 1.0) }
                               { Key = "startKey"
                                 Label = "Start"
                                 Kind = EditorValueKind.Text
                                 Required = true
                                 DefaultValue = Some(SduiValue.Text "") }
                               { Key = "endKey"
                                 Label = "Optional end"
                                 Kind = EditorValueKind.Text
                                 Required = false
                                 DefaultValue = Some(SduiValue.Text "") } |],
                        Some 0,
                        Some 32)
                 Required = false
                 DefaultValue = Some(SduiValue.Array [||]) } |] }

    let defaultOptions =
        { MaxOpenEditors = 8
          Schema = defaultSchema }

    let targetKey target =
        let segment (value: string) =
            let safe = if isNull value then "" else value
            string safe.Length + ":" + safe
        segment target.SpaceId + segment target.TargetKind + segment target.TargetId

    let validateOptions options =
        if options.MaxOpenEditors < 1 || options.MaxOpenEditors > 12 then
            Result.Error "tagging-editor-max-open-invalid"
        else
            match DynamicEditorValidation.validateSchema DynamicEditorDefaults.limits options.Schema with
            | Result.Error _ -> Result.Error "tagging-editor-schema-invalid"
            | Result.Ok _ ->
                let fields = options.Schema.Fields |> Array.map _.Key |> Set.ofArray
                if Set.isSubset (Set.ofList [ "tags"; "summary"; "timeFrames" ]) fields then Result.Ok options
                else Result.Error "tagging-editor-schema-fields-missing"

    let fieldLabel options key fallback =
        options.Schema.Fields
        |> Array.tryFind (fun field -> field.Key = key)
        |> Option.map _.Label
        |> Option.filter (String.IsNullOrWhiteSpace >> not)
        |> Option.defaultValue fallback

    let node tag text =
        let element = JS.Document.CreateElement tag
        element.TextContent <- text
        element

    let append (parent: Node) (child: Node) = parent.AppendChild child |> ignore
    let clear (parent: Node) =
        while not (isNull parent.FirstChild) do parent.RemoveChild(parent.FirstChild) |> ignore
    let inputValue (element: Element) = JS.Get<string> "value" element
    let setInputValue text (element: Element) = JS.Set element "value" text
    let html (element: Element) = element |> As<HTMLElement>
    let setStyle name value (element: Element) = (html element).Style.SetProperty(name,value)
    let testId value (element: Element) = element.SetAttribute("data-testid",value);element

    let styleText =
        """
.ptcs-tagging-layer{position:fixed;inset:0;z-index:60;pointer-events:none}.ptcs-tagging-card{position:fixed;width:min(540px,calc(100vw - 32px));max-height:min(78vh,760px);overflow:auto;pointer-events:auto;background:#fff;color:#17212b;border:1px solid #9aa8b8;border-radius:12px;box-shadow:0 18px 54px #17202a52;padding:0 12px 12px;box-sizing:border-box}.ptcs-tagging-header{position:sticky;top:0;z-index:3;display:flex;align-items:center;gap:8px;background:#fff;padding:10px 0 8px;cursor:move;border-top:4px solid #2d5f9a;border-bottom:2px solid #9eacba}.ptcs-tagging-heading{min-width:0;flex:1}.ptcs-tagging-heading h3{margin:0;font-size:17px;font-weight:800;overflow-wrap:anywhere}.ptcs-tagging-context{display:block;color:#617083;font-size:12px;font-weight:600;overflow-wrap:anywhere}.ptcs-tagging-header button{flex:0 0 auto}.ptcs-tagging-row{margin-top:10px;padding:10px;border:1px solid #cbd4dd;border-radius:9px;background:#fbfcfd}.ptcs-tagging-row h4{margin:0 0 5px;font-size:12px;color:#536173;overflow-wrap:anywhere}.ptcs-tagging-row label{display:block;margin-top:6px}.ptcs-tagging-row input,.ptcs-tagging-row textarea{box-sizing:border-box;width:100%;padding:7px;margin:3px 0 6px}.ptcs-tagging-row textarea{min-height:86px;resize:vertical}.ptcs-tagging-frames{display:grid;gap:5px}.ptcs-tagging-frame{display:grid;grid-template-columns:64px 1fr 1fr auto;gap:5px;align-items:center}.ptcs-tagging-frame input{margin:0}.ptcs-tagging-actions{display:flex;flex-wrap:nowrap;gap:7px;margin-top:8px;overflow-x:auto}.ptcs-tagging-actions button{white-space:nowrap}.ptcs-tagging-status{min-height:19px;color:#495869}.ptcs-tagging-dirty{margin:8px 0;padding:9px;border:1px solid #c48b1a;background:#fff7df;border-radius:8px}.ptcs-tagging-dirty[hidden]{display:none}.ptcs-tagging-tabs{display:none}
@media(max-width:720px){.ptcs-tagging-tabs{position:fixed;display:flex;pointer-events:auto;left:0;right:0;bottom:min(82vh,700px);overflow-x:auto;gap:5px;padding:5px 8px;background:#e8edf2;border-top:1px solid #aeb8c2}.ptcs-tagging-tabs button{white-space:nowrap}.ptcs-tagging-card{display:none;left:0!important;right:0;top:auto!important;bottom:0;width:100%;max-height:min(82vh,700px);border-radius:14px 14px 0 0;padding-bottom:max(12px,env(safe-area-inset-bottom))}.ptcs-tagging-card[data-active=true]{display:block}.ptcs-tagging-header{cursor:default}.ptcs-tagging-frame{grid-template-columns:56px 1fr}.ptcs-tagging-frame input:nth-child(3){grid-column:1/3}.ptcs-tagging-frame button{grid-column:2}}
"""

    let draftFingerprint draft = JSON.Stringify draft

    let draftFromView view =
        let tags =
            (inputValue view.TagsInput).Split(',',StringSplitOptions.RemoveEmptyEntries)
            |> Array.map _.Trim()
            |> Array.filter (String.IsNullOrWhiteSpace >> not)
        let frames =
            view.FrameInputs
            |> Array.map (fun input ->
                let mutable scale = 0
                Int32.TryParse(inputValue input.Scale,&scale) |> ignore
                { Scale=scale;StartKey=inputValue input.StartKey;EndKey=inputValue input.EndKey })
        { Tags=tags;Summary=inputValue view.SummaryInput;TimeFrames=frames }

    let isRowDirty view =
        match view.ExtraInfo with
        | None -> true
        | Some _ -> view.InitialFingerprint<>"" && draftFingerprint(draftFromView view)<>view.InitialFingerprint

    let setPosition instance left top =
        instance.Left <- left
        instance.Top <- top
        setStyle "left" (string left+"px") instance.Card
        setStyle "top" (string top+"px") instance.Card

    let mount (host: Element) options =
        let options = validateOptions options |> Result.defaultWith invalidOp
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
        let mutable dragging: (string*float*float) option = None

        let rec focusKey key =
            match instances.TryGetValue key with
            | true, instance ->
                zSequence <- zSequence+1
                instance.ZIndex <- zSequence
                setStyle "z-index" (string zSequence) instance.Card
                activeKey <- key
                instances.Values |> Seq.iter (fun item -> item.Card.SetAttribute("data-active",if item.Key=key then "true" else "false"))
                clear tabs
                openOrder |> List.iter (fun itemKey ->
                    match instances.TryGetValue itemKey with
                    | true, item ->
                        let button = node "button" item.Binding.Target.TargetId
                        button.SetAttribute("type","button")
                        button.SetAttribute("aria-current",if itemKey=key then "true" else "false")
                        button.AddEventListener("click",fun (_:Event) -> focusKey itemKey)
                        append tabs button
                    | _ -> ())
            | _ -> ()

        let removeInstance key =
            match instances.TryGetValue key with
            | true, instance ->
                if JS.Document.Contains instance.Card then instance.Card.Remove()
                instances.Remove key |> ignore
                openOrder <- openOrder |> List.filter ((<>) key)
                if activeKey=key then activeKey <- openOrder |> List.tryLast |> Option.defaultValue ""
                if activeKey<>"" then focusKey activeKey else clear tabs
            | _ -> ()

        let addFrame view scaleValue startValue endValue =
            let root = node "div" ""
            root.ClassName <- "ptcs-tagging-frame"
            let scale = node "input" ""
            scale.SetAttribute("type","number");scale.SetAttribute("min","-128");scale.SetAttribute("max","127");scale.SetAttribute("aria-label","Scale")
            setInputValue scaleValue scale
            let startKey = node "input" ""
            startKey.SetAttribute("inputmode","numeric");startKey.SetAttribute("placeholder","yyyyMMddHHmm");startKey.SetAttribute("aria-label","Start")
            setInputValue startValue startKey
            let endKey = node "input" ""
            endKey.SetAttribute("inputmode","numeric");endKey.SetAttribute("placeholder","optional end");endKey.SetAttribute("aria-label","End")
            setInputValue endValue endKey
            let remove = node "button" "×"
            remove.SetAttribute("type","button");remove.SetAttribute("aria-label","Remove time frame")
            let input = { Root=root;Scale=scale;StartKey=startKey;EndKey=endKey }
            remove.AddEventListener("click",fun (_:Event) ->
                view.FrameInputs <- view.FrameInputs |> Array.filter (fun candidate -> not(obj.ReferenceEquals(candidate.Root,root)))
                if JS.Document.Contains root then root.Remove())
            [|scale;startKey;endKey;remove|] |> Array.iter (append root)
            append view.FramesHost root
            view.FrameInputs <- Array.append view.FrameInputs [|input|]

        let fillView view row draft =
            view.ExtraInfo <- row
            view.Heading.TextContent <- row |> Option.map _.ExInfoId |> Option.defaultValue "New ExtraInfo"
            setInputValue (String.concat ", " draft.Tags) view.TagsInput
            setInputValue draft.Summary view.SummaryInput
            clear view.FramesHost
            view.FrameInputs <- [||]
            draft.TimeFrames |> Array.iter (fun frame -> addFrame view (string frame.Scale) frame.StartKey frame.EndKey)
            view.InitialFingerprint <- if row.IsSome then draftFingerprint(draftFromView view) else "__new__"

        let removeRowView instance view =
            instance.ExtraInfoViews <- instance.ExtraInfoViews |> Array.filter (fun item -> not(obj.ReferenceEquals(item,view)))
            if JS.Document.Contains view.Root then view.Root.Remove()

        let rec dispatchList instance finalStatus =
            instance.ListGeneration <- instance.ListGeneration+1
            let generation = instance.ListGeneration
            instance.Status.TextContent <- "Loading ExtraInfo…"
            instance.Binding.Dispatch TaggingEditorRequest.List (fun result ->
                match instances.TryGetValue instance.Key with
                | true, current when not disposed && obj.ReferenceEquals(current,instance) && generation=instance.ListGeneration ->
                    match result with
                    | Result.Error error -> instance.Status.TextContent <- error
                    | Result.Ok(TaggingEditorReply.Listed rows) ->
                        let dirtyExisting = Dictionary<string,TaggingExtraInfoDraft>()
                        let dirtyNew = ResizeArray<TaggingExtraInfoDraft>()
                        instance.ExtraInfoViews |> Array.iter (fun view ->
                            if isRowDirty view then
                                match view.ExtraInfo with
                                | Some row -> dirtyExisting[row.ExInfoId] <- draftFromView view
                                | None -> dirtyNew.Add(draftFromView view))
                        instance.ExtraInfos <- if isNull rows then [||] else rows
                        clear instance.RowsHost
                        instance.ExtraInfoViews <- [||]
                        instance.ExtraInfos |> Array.iter (fun row ->
                            let draft = match dirtyExisting.TryGetValue row.ExInfoId with true,value -> value | _ -> row.Draft
                            let view = createRowView instance (Some row) draft
                            instance.ExtraInfoViews <- Array.append instance.ExtraInfoViews [|view|])
                        dirtyNew |> Seq.iter (fun draft ->
                            let view = createRowView instance None draft
                            instance.ExtraInfoViews <- Array.append instance.ExtraInfoViews [|view|])
                        instance.Status.TextContent <-
                            if finalStatus<>"" then finalStatus
                            elif instance.ExtraInfoViews.Length=0 then "No ExtraInfo. Use New ExtraInfo to add one."
                            else ""
                    | Result.Ok _ -> instance.Status.TextContent <- "tagging-editor-list-reply-invalid"
                | _ -> ())

        and dispatchMutation instance view request =
            view.RequestGeneration <- view.RequestGeneration+1
            let generation = view.RequestGeneration
            view.Status.TextContent <- "Saving…"
            instance.Binding.Dispatch request (fun result ->
                match instances.TryGetValue instance.Key with
                | true, current when not disposed && obj.ReferenceEquals(current,instance)
                                     && (instance.ExtraInfoViews |> Array.exists (fun item -> obj.ReferenceEquals(item,view)))
                                     && generation=view.RequestGeneration ->
                    match result with
                    | Result.Error error -> view.Status.TextContent <- error
                    | Result.Ok TaggingEditorReply.MutationAccepted ->
                        match request with
                        | TaggingEditorRequest.Create _ | TaggingEditorRequest.Delete _ -> removeRowView instance view
                        | TaggingEditorRequest.Update _ -> view.InitialFingerprint <- draftFingerprint(draftFromView view)
                        | _ -> ()
                        dispatchList instance ""
                    | Result.Ok(TaggingEditorReply.Conflict serverCurrent) ->
                        view.Status.TextContent <- "Revision conflict. The local draft was kept."
                        match serverCurrent with Some row -> view.ExtraInfo<-Some row;view.Heading.TextContent<-row.ExInfoId | None -> ()
                    | Result.Ok(TaggingEditorReply.Listed _) -> view.Status.TextContent <- "tagging-editor-mutation-reply-invalid"
                | _ -> ())

        and saveAll instance =
            let dirtyViews = instance.ExtraInfoViews |> Array.filter isRowDirty
            if dirtyViews.Length=0 then instance.Status.TextContent <- "All ExtraInfo is already saved."
            else
                instance.Status.TextContent <- "Saving all ExtraInfo…"
                let mutable issueCount = 0
                let rec saveAt index =
                    if index>=dirtyViews.Length then
                        let message =
                            if issueCount=0 then "All ExtraInfo saved."
                            else string issueCount+" ExtraInfo item(s) need attention; their drafts were kept."
                        dispatchList instance message
                    else
                        let view = dirtyViews[index]
                        let draft = draftFromView view
                        let request =
                            match view.ExtraInfo with
                            | None -> TaggingEditorRequest.Create draft
                            | Some row -> TaggingEditorRequest.Update(row.ExInfoId,row.Revision,draft)
                        view.RequestGeneration <- view.RequestGeneration+1
                        let generation = view.RequestGeneration
                        view.Status.TextContent <- "Saving…"
                        instance.Binding.Dispatch request (fun result ->
                            let stillCurrent =
                                match instances.TryGetValue instance.Key with
                                | true,current -> not disposed && obj.ReferenceEquals(current,instance) && generation=view.RequestGeneration
                                | _ -> false
                            if not stillCurrent then issueCount<-issueCount+1;saveAt(index+1)
                            else
                                match result with
                                | Result.Error error ->
                                    issueCount<-issueCount+1
                                    view.Status.TextContent<-error
                                    saveAt(index+1)
                                | Result.Ok TaggingEditorReply.MutationAccepted ->
                                    match request with
                                    | TaggingEditorRequest.Create _ -> removeRowView instance view
                                    | TaggingEditorRequest.Update _ -> view.InitialFingerprint<-draftFingerprint(draftFromView view);view.Status.TextContent<-"Saved."
                                    | _ -> ()
                                    saveAt(index+1)
                                | Result.Ok(TaggingEditorReply.Conflict serverCurrent) ->
                                    issueCount<-issueCount+1
                                    view.Status.TextContent<-"Revision conflict. The local draft was kept."
                                    match serverCurrent with Some row -> view.ExtraInfo<-Some row;view.Heading.TextContent<-row.ExInfoId | None -> ()
                                    saveAt(index+1)
                                | Result.Ok(TaggingEditorReply.Listed _) ->
                                    issueCount<-issueCount+1
                                    view.Status.TextContent<-"tagging-editor-mutation-reply-invalid"
                                    saveAt(index+1))
                saveAt 0

        and createRowView instance row draft =
            let root = node "section" ""
            root.ClassName <- "ptcs-tagging-row"
            let heading = node "h4" ""
            append root heading
            append root (node "label" (fieldLabel options "tags" "Tags"))
            let tags = node "input" ""
            tags.SetAttribute("placeholder","tag1, tag2");append root tags
            append root (node "label" (fieldLabel options "summary" "Summary"))
            let summary = node "textarea" ""
            append root summary
            append root (node "label" (fieldLabel options "timeFrames" "Time frames"))
            let frames = node "div" ""
            frames.ClassName <- "ptcs-tagging-frames";append root frames
            let status = node "div" ""
            status.ClassName <- "ptcs-tagging-status";status.SetAttribute("role","status");append root status
            let actions = node "div" ""
            actions.ClassName <- "ptcs-tagging-actions";append root actions
            let action text =
                let button = node "button" text
                button.SetAttribute("type","button");append actions button;button
            let addFrameButton = action "新增時框"
            let saveButton = action "儲存"
            let deleteButton = action "刪除"
            let view =
                { ViewKey=Guid.NewGuid().ToString("N");Root=root;Heading=heading;TagsInput=tags;SummaryInput=summary
                  FramesHost=frames;Status=status;ExtraInfo=row;FrameInputs=[||];InitialFingerprint="";RequestGeneration=0 }
            fillView view row draft
            addFrameButton.AddEventListener("click",fun (_:Event) -> addFrame view "1" "" "")
            saveButton.AddEventListener("click",fun (_:Event) ->
                let value = draftFromView view
                match view.ExtraInfo with
                | None -> dispatchMutation instance view (TaggingEditorRequest.Create value)
                | Some current -> dispatchMutation instance view (TaggingEditorRequest.Update(current.ExInfoId,current.Revision,value)))
            deleteButton.AddEventListener("click",fun (_:Event) ->
                match view.ExtraInfo with
                | None -> removeRowView instance view
                | Some current -> dispatchMutation instance view (TaggingEditorRequest.Delete(current.ExInfoId,current.Revision)))
            append instance.RowsHost root
            view

        let createInstance binding =
            let key = targetKey binding.Target
            let card = node "article" "" |> testId ("ptcs-tagging-editor-"+string instances.Count)
            card.ClassName <- "ptcs-tagging-card";card.SetAttribute("data-target-id",binding.Target.TargetId);card.SetAttribute("tabindex","-1")
            let header = node "header" ""
            header.ClassName <- "ptcs-tagging-header";append card header
            let heading = node "div" ""
            heading.ClassName <- "ptcs-tagging-heading"
            append heading (node "h3" binding.Target.TargetId)
            let context = node "small" binding.Target.ContextLabel
            context.ClassName <- "ptcs-tagging-context";append heading context;append header heading
            let newButton = node "button" "新增 ExtraInfo"
            newButton.SetAttribute("type","button");append header newButton
            let saveAllButton = node "button" "全部儲存"
            saveAllButton.SetAttribute("type","button");append header saveAllButton
            let closeButton = node "button" "×"
            closeButton.SetAttribute("type","button");closeButton.SetAttribute("aria-label","Close ExtraInfo editor for "+binding.Target.TargetId);append header closeButton
            let rowsHost = node "div" ""
            append card rowsHost
            let status = node "div" ""
            status.ClassName <- "ptcs-tagging-status";status.SetAttribute("role","status");append card status
            let dirtyPrompt = node "div" ""
            dirtyPrompt.ClassName <- "ptcs-tagging-dirty";dirtyPrompt.SetAttribute("hidden","")
            append dirtyPrompt (node "strong" "One or more ExtraInfo drafts have not been saved.");append card dirtyPrompt
            let instance =
                { Key=key;Binding=binding;Card=card;RowsHost=rowsHost;Status=status;DirtyPrompt=dirtyPrompt
                  ExtraInfos=[||];ExtraInfoViews=[||];ListGeneration=0;Left=0.;Top=0.;ZIndex=0 }
            let dirtyAction text (handler: Event -> unit) =
                let button = node "button" text
                button.SetAttribute("type","button");button.AddEventListener("click",handler);append dirtyPrompt button
            dirtyAction "Discard all and close" (fun (_:Event) -> removeInstance key)
            dirtyAction "Stay" (fun (_:Event) -> dirtyPrompt.SetAttribute("hidden",""))
            newButton.AddEventListener("mousedown",fun (event:Event) -> event.StopPropagation())
            saveAllButton.AddEventListener("mousedown",fun (event:Event) -> event.StopPropagation())
            closeButton.AddEventListener("mousedown",fun (event:Event) -> event.StopPropagation())
            newButton.AddEventListener("click",fun (_:Event) ->
                let view = createRowView instance None emptyDraft
                instance.ExtraInfoViews <- Array.append instance.ExtraInfoViews [|view|]
                instance.Status.TextContent <- "")
            saveAllButton.AddEventListener("click",fun (_:Event) -> saveAll instance)
            closeButton.AddEventListener("click",fun (_:Event) ->
                if instance.ExtraInfoViews |> Array.exists isRowDirty then dirtyPrompt.RemoveAttribute("hidden") else removeInstance key)
            card.AddEventListener("mousedown",fun (_:Event) -> focusKey key)
            header.AddEventListener("mousedown",fun (raw:Event) ->
                let event = raw :?> MouseEvent
                if event.Button=0 then
                    let bounds = card.GetBoundingClientRect()
                    dragging <- Some(key,float event.ClientX-bounds.Left,float event.ClientY-bounds.Top)
                    event.PreventDefault())
            card.AddEventListener("keydown",fun (raw:Event) ->
                let event = raw :?> KeyboardEvent
                if event.Key="Escape" then
                    event.PreventDefault()
                    if instance.ExtraInfoViews |> Array.exists isRowDirty then dirtyPrompt.RemoveAttribute("hidden") else removeInstance key)
            append layer card
            let offset = float(instances.Count%5)*28.
            setPosition instance (max 12. (float JS.Window.InnerWidth-560.-offset)) (82.+offset)
            instance

        let openTarget binding =
            if not disposed then
                let key = targetKey binding.Target
                match instances.TryGetValue key with
                | true, _ -> focusKey key
                | _ when instances.Count>=options.MaxOpenEditors ->
                    if activeKey<>"" then
                        focusKey activeKey
                        match instances.TryGetValue activeKey with true,item -> item.Status.TextContent<-"Close an editor before opening another target." | _ -> ()
                | _ ->
                    let instance = createInstance binding
                    instances[key] <- instance
                    openOrder <- openOrder@[key]
                    focusKey key
                    dispatchList instance ""

        let closeTarget target =
            let key = targetKey target
            match instances.TryGetValue key with
            | true, instance when instance.ExtraInfoViews |> Array.exists isRowDirty -> focusKey key;instance.DirtyPrompt.RemoveAttribute("hidden")
            | true, _ -> removeInstance key
            | _ -> ()

        let mouseMove (raw:Event) =
            match dragging with
            | Some(key,offsetX,offsetY) ->
                match instances.TryGetValue key with
                | true, instance ->
                    let event = raw :?> MouseEvent
                    let bounds = instance.Card.GetBoundingClientRect()
                    let left = min (max 0. (float JS.Window.InnerWidth-bounds.Width)) (max 0. (float event.ClientX-offsetX))
                    let top = min (max 0. (float JS.Window.InnerHeight-48.)) (max 0. (float event.ClientY-offsetY))
                    setPosition instance left top
                | _ -> dragging <- None
            | None -> ()
        let mouseUp (_:Event) = dragging <- None
        JS.Window.AddEventListener("mousemove",mouseMove)
        JS.Window.AddEventListener("mouseup",mouseUp)

        let dispose () =
            if not disposed then
                disposed <- true
                JS.Window.RemoveEventListener("mousemove",mouseMove)
                JS.Window.RemoveEventListener("mouseup",mouseUp)
                instances.Values |> Seq.toArray |> Array.iter (fun item -> if JS.Document.Contains item.Card then item.Card.Remove())
                instances.Clear();openOrder<-[]
                if JS.Document.Contains layer then layer.Remove()
                if JS.Document.Contains style then style.Remove()

        { Open=openTarget
          Close=closeTarget
          Focus=targetKey>>focusKey
          Dispose=dispose }
