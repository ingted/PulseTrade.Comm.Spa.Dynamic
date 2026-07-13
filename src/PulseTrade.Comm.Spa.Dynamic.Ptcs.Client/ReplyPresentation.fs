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
        let index = value.IndexOf(marker)
        if index >= 0 then value.Substring(index + marker.Length).Trim() else value

    let runtimeFrames rawContent =
        let content = extractReplyPayload rawContent

        try
            let parsed = JSON.Parse content
            let candidates = if JS.Global?Array?isArray(parsed) then As<obj array> parsed else [| parsed |]
            let frames =
                candidates
                |> Array.filter (fun candidate ->
                    tryGet<string> "protocol" candidate
                    |> Option.exists (fun protocol -> protocol = "sdui-runtime.v1"))

            if frames.Length = candidates.Length && frames.Length > 0 then Some frames else None
        with _ -> None

    let unionCase value =
        tryGetAny<string> [| "Case"; "case" |] value |> Option.defaultValue ""

    let unionFields value =
        tryGetAny<obj array> [| "Fields"; "fields" |] value |> Option.defaultValue [||]

    let documentFromFrame frame =
        match tryGet<obj> "payload" frame with
        | None -> None
        | Some payload when unionCase payload = "Document" -> unionFields payload |> Array.tryHead
        | Some payload -> tryGetAny<obj> [| "Document"; "document" |] payload

    let mapValue key mapObject =
        tryGet<obj> key mapObject |> Option.map textValue |> Option.defaultValue ""

    let rowSummary (row: obj) =
        let rowId = tryGet<string> "rowId" row |> Option.defaultValue "row"
        let traces = tryGet<obj array> "traces" row |> Option.defaultValue [||]
        let labels =
            traces
            |> Array.choose (fun trace -> tryGet<string> "label" trace)
            |> Array.filter (String.IsNullOrWhiteSpace >> not)

        if labels.Length = 0 then rowId else rowId + ": " + String.concat ", " labels

    let summaryFromFrames frames =
        let document = frames |> Array.tryPick documentFromFrame

        match document with
        | None -> None
        | Some value ->
            let canvasInstanceId =
                frames
                |> Array.tryPick (fun frame -> tryGet<obj> "canvasInstanceId" frame |> Option.map textValue)
                |> Option.defaultValue ""

            if String.IsNullOrWhiteSpace canvasInstanceId then
                None
            else
                let title = tryGet<string> "title" value |> Option.defaultValue "TA Research"
                let rows = tryGet<obj array> "rows" value |> Option.defaultValue [||] |> Array.map rowSummary
                let defaultView = tryGet<obj> "defaultView" value |> Option.defaultValue null
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
                        | count -> "last " + count + " bars"
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
                      Coverage = "loads on expand"
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
        runtimeFrames context.Payload
        |> Option.bind summaryFromFrames
        |> Option.map (fun summary ->
            { Kind = "runtime-ta"
              RenderSummary = fun () -> renderSummary summary
              Mount =
                fun _ host ->
                    clearHost host
                    let identity = safeIdentity context.ValueId
                    host.Id <- "ptcs-ta-reply-" + identity
                    let channelId = "ta-reply-" + identity
                    let handle =
                        TaResearchTransientClient.mountByIdWithOptions
                            host.Id
                            extensionId
                            channelId
                            summary.CanvasInstanceId
                            TaClientLifecycle.defaults

                    fun () ->
                        handle.Dispose()
                        clearHost host })

    let register extensionId =
        Client.RegisterReplyPresentation("dynamic-runtime-ta-v2", tryResolve extensionId)
