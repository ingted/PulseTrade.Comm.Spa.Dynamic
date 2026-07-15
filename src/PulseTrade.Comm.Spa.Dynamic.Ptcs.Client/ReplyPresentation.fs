namespace PulseTrade.Comm.Spa.Dynamic.Ptcs.Client

open System
open PulseTrade.Comm.Spa
open WebSharper
open WebSharper.JavaScript
open WebSharper.JavaScript.Dom
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Html

[<JavaScript>]
type TaRuntimeReplySummary =
    { Title: string
      CanvasInstanceId: string
      Instrument: string
      Interval: string
      RequestedRange: string
      Coverage: string
      Freshness: string
      Rows: string array }

/// PTCS reply presentation adapter for Dynamic TA RuntimeFrame payloads.
[<JavaScript; RequireQualifiedAccess>]
module TaResearchReplyPresentation =
    let setClassifierState state =
        if not (isNull (box JS.Document.Body)) then
            JS.Document.Body.SetAttribute("data-ta-reply-presentation", state)

    let tryGet<'T> name (value: obj) =
        try
            if isNull value || not (JS.HasOwnProperty value name) then None
            else Some(JS.Get<'T> name value)
        with _ -> None

    let tryGetAny<'T> names value =
        names |> Array.tryPick (fun name -> tryGet<'T> name value)

    let textValue (value: obj) =
        try
            if isNull value then ""
            elif JS.TypeOf value = JS.Kind.String then string value
            elif JS.TypeOf value = JS.Kind.Number || JS.TypeOf value = JS.Kind.Boolean then string value
            else
                let fields = tryGetAny<obj array> [| "Fields"; "fields" |] value |> Option.defaultValue [||]
                if fields.Length = 0 then string value else string fields[0]
        with _ -> ""

    let extractReplyPayload (rawContent: string) =
        let value = if isNull rawContent then "" else rawContent.Trim()
        let marker = "replied msg:"
        let afterMarker (candidate: string) =
            let index = candidate.IndexOf(marker)
            if index >= 0 then candidate.Substring(index + marker.Length).Trim() else candidate

        try
            let envelope = JSON.Parse value
            let schema = tryGet<string> "schema" envelope |> Option.defaultValue ""

            if schema = "ptc.comm.fcell2.value.v1" then
                tryGet<obj array> "rows" envelope
                |> Option.defaultValue [||]
                |> Array.map textValue
                |> Array.tryFind (fun row -> row.IndexOf(marker) >= 0)
                |> Option.map afterMarker
                |> Option.defaultValue value
            else
                afterMarker value
        with _ ->
            afterMarker value

    let runtimeFrames rawContent =
        let content = extractReplyPayload rawContent

        try
            let parsed = JSON.Parse content
            let protocol candidate =
                tryGetAny<string> [| "protocol"; "Protocol" |] candidate
                |> Option.defaultValue ""

            let rec collect depth candidate =
                if depth > 6 || isNull candidate then
                    [||]
                elif protocol candidate = "sdui-runtime.v1" then
                    [| candidate |]
                elif JS.TypeOf candidate = JS.Kind.String then
                    try collect (depth + 1) (JSON.Parse(string candidate)) with _ -> [||]
                elif JS.Global?Array?isArray(candidate) then
                    As<obj array> candidate |> Array.collect (collect (depth + 1))
                else
                    let caseName = tryGetAny<string> [| "Case"; "case" |] candidate |> Option.defaultValue ""
                    let fields = tryGetAny<obj array> [| "Fields"; "fields" |] candidate |> Option.defaultValue [||]

                    match caseName with
                    | "S"
                    | "A" -> fields |> Array.collect (collect (depth + 1))
                    | _ -> [||]

            let frames = collect 0 parsed
            if frames.Length > 0 then Some frames else None
        with _ -> None

    let unionCase value =
        tryGetAny<string> [| "Case"; "case" |] value |> Option.defaultValue ""

    let unionFields value =
        tryGetAny<obj array> [| "Fields"; "fields" |] value |> Option.defaultValue [||]

    let documentFromFrame frame =
        match tryGetAny<obj> [| "payload"; "Payload" |] frame with
        | None -> None
        | Some payload when unionCase payload = "Document" -> unionFields payload |> Array.tryHead
        | Some payload -> tryGetAny<obj> [| "Document"; "document" |] payload

    let mapValue key mapObject =
        let direct = tryGet<obj> key mapObject

        let fromObjectUnion () =
            if unionCase mapObject <> "Object" then
                None
            else
                unionFields mapObject
                |> Array.tryHead
                |> Option.bind (fun entries ->
                    if JS.Global?Array?isArray(entries) then
                        As<obj array> entries
                        |> Array.tryPick (fun entry ->
                            if JS.Global?Array?isArray(entry) then
                                let pair = As<obj array> entry
                                if pair.Length >= 2 && textValue pair[0] = key then Some pair[1] else None
                            else
                                None)
                    else
                        None)

        direct |> Option.orElseWith fromObjectUnion |> Option.map textValue |> Option.defaultValue ""

    let rowSummary (row: obj) =
        let rowId = tryGetAny<string> [| "rowId"; "RowId" |] row |> Option.defaultValue "row"
        let traces = tryGetAny<obj array> [| "traces"; "Traces" |] row |> Option.defaultValue [||]
        let labels =
            traces
            |> Array.choose (fun trace -> tryGetAny<string> [| "label"; "Label" |] trace)
            |> Array.filter (String.IsNullOrWhiteSpace >> not)

        if labels.Length = 0 then rowId else rowId + ": " + String.concat ", " labels

    let summaryFromFrames frames =
        let document = frames |> Array.tryPick documentFromFrame

        match document with
        | None -> None
        | Some value ->
            let canvasInstanceId =
                frames
                |> Array.tryPick (fun frame -> tryGetAny<obj> [| "canvasInstanceId"; "CanvasInstanceId" |] frame |> Option.map textValue)
                |> Option.defaultValue ""

            if String.IsNullOrWhiteSpace canvasInstanceId then
                None
            else
                let title = tryGetAny<string> [| "title"; "Title" |] value |> Option.defaultValue "TA Research"
                let rows = tryGetAny<obj array> [| "rows"; "Rows" |] value |> Option.defaultValue [||] |> Array.map rowSummary
                let defaultView = tryGetAny<obj> [| "defaultView"; "DefaultView" |] value |> Option.defaultValue null
                let instrument = mapValue "query.instrument" defaultView
                let interval =
                    match mapValue "query.intervalMinutes" defaultView with
                    | "" -> "dynamic interval"
                    | minutes -> minutes + "m"

                let requestedRange =
                    match mapValue "query.rangeKind" defaultView with
                    | "last-bars" ->
                        match mapValue "query.lastBars" defaultView with
                        | "" -> "last bars"
                        | count -> "requested last " + count + " bars"
                    | "between-utc" ->
                        mapValue "query.fromUtc" defaultView + " .. " + mapValue "query.toUtcExclusive" defaultView
                    | "trading-day" -> mapValue "query.fromUtc" defaultView
                    | _ -> "runtime request"

                Some
                    { Title = title
                      CanvasInstanceId = canvasInstanceId
                      Instrument = if String.IsNullOrWhiteSpace instrument then title else instrument
                      Interval = interval
                      RequestedRange = requestedRange
                      Coverage = "actual coverage shown after expand"
                      Freshness = "initial snapshot"
                      Rows = rows }

    let safeIdentity value =
        let normalized =
            value
            |> Seq.map (fun character ->
                if Char.IsLetterOrDigit character || character = '-' || character = '_' then character else '-')
            |> Seq.toArray
            |> fun characters -> new System.String(characters)

        if String.IsNullOrWhiteSpace normalized then "reply" else normalized

    let clearHost (host: Element) =
        while not (isNull host.FirstChild) do
            host.RemoveChild(host.FirstChild) |> ignore

    let copyCanonicalJson content =
        if isNull (box JS.Document.Body) then
            Result.Error "Document body is unavailable."
        else
            let textarea = JS.Document.CreateElement("textarea") |> As<HTMLTextAreaElement>
            textarea.Value <- content
            textarea.SetAttribute("readonly", "readonly")
            textarea.SetAttribute("aria-hidden", "true")
            textarea.SetAttribute("style", "position:fixed; left:-10000px; top:0; width:1px; height:1px; opacity:0;")
            JS.Document.Body.AppendChild textarea |> ignore
            textarea.Select()

            let copied = JS.Document.ExecCommand("copy", false, "")
            JS.Document.Body.RemoveChild textarea |> ignore

            if copied then Result.Ok "JSON copied." else Result.Error "Clipboard copy was rejected."

    let renderSummary (summary: TaRuntimeReplySummary) =
        let root = JS.Document.CreateElement("div")
        let rowText = if summary.Rows.Length = 0 then "rows pending" else String.concat " | " summary.Rows
        let document =
            div [ attr.style "min-width:0; display:grid; gap:5px;" ] [
                div [ attr.style "display:flex; align-items:center; justify-content:space-between; gap:8px; min-width:0;" ] [
                    strong [ attr.style "min-width:0; color:#163f9f; overflow-wrap:anywhere;" ] [ text summary.Title ]
                    span [ attr.style "color:#596579; font-size:12px; white-space:nowrap;" ] [ text $"{summary.Interval} / {summary.Rows.Length} rows" ]
                ]
                div [ attr.style "display:flex; flex-wrap:wrap; gap:5px 10px; color:#4f5b6e; font-size:12px;" ] [
                    span [] [ text summary.Instrument ]
                    span [] [ text summary.RequestedRange ]
                    span [] [ text summary.Coverage ]
                    span [] [ text summary.Freshness ]
                ]
                div [ attr.style "color:#334056; font-size:12px; overflow-wrap:anywhere;" ] [ text rowText ]
            ]

        Doc.Run root document
        root :> Node

    let tryResolve extensionId (context: ReplyPresentationContext) =
        match runtimeFrames context.Payload with
        | None ->
            setClassifierState "frames-missing"
            None
        | Some frames ->
            match summaryFromFrames frames with
            | None ->
                setClassifierState "summary-missing"
                None
            | Some summary ->
                setClassifierState "matched"
                Some
                    { Kind = "runtime-ta"
                      RenderSummary = fun () -> renderSummary summary
                      Actions =
                        [| { ActionId = "copy-json"
                             Label = "複製 JSON"
                             Title = "Copy canonical SDUI JSON"
                             Invoke = fun () -> copyCanonicalJson (extractReplyPayload context.Payload) } |]
                      Mount =
                        fun _ host ->
                            clearHost host
                            let identity = safeIdentity context.ValueId
                            host.Id <- "ptcs-ta-reply-" + identity
                            let channelId = "ta-reply-" + identity
                            let handle =
                                TaResearchTransientClient.mountOnElementWithOptions
                                    host
                                    extensionId
                                    channelId
                                    summary.CanvasInstanceId
                                    TaClientLifecycle.defaults

                            fun () ->
                                handle.Dispose()
                                clearHost host }

    let register extensionId =
        setClassifierState "registered"
        Client.RegisterReplyPresentation("dynamic-runtime-ta-v2", tryResolve extensionId)
