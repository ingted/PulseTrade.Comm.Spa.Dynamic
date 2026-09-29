namespace PulseTrade.Comm.Spa.Dynamic.Interactive.Client.BrowserCacheDemo

open PulseTrade.Comm.Spa.Dynamic.Contracts
open PulseTrade.Comm.Spa.Dynamic.Interactive.Client
open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Html

[<JavaScript>]
module Client =
    let fixtureEntry elementId =
        let node = JS.Document.GetElementById elementId

        if isNull node then
            None
        else
            match BrowserRuntimeCodec.decodeCacheEntry node.TextContent with
            | Ok entry -> Some entry
            | Error _ -> None

    let fixtureEntries () =
        [| 0..9 |]
        |> Array.choose (fun index ->
            fixtureEntry ("cache-entry-" + string index))

    let rec writeSequentially (entries: RuntimeCacheEntry array) (index: int) (completed: unit -> unit) =
        if index >= entries.Length then
            completed ()
        else
            BrowserRuntimeCache.write
                entries[index]
                (function
                    | BrowserRuntimeCacheWriteResult.Written -> writeSequentially entries (index + 1) completed
                    | BrowserRuntimeCacheWriteResult.Unavailable _ -> completed ())

    let runtimeState poll (entry: RuntimeCacheEntry) =
        { Identity =
            { DocumentId = DocumentId "browser-cache-demo-document"
              CanvasInstanceId = CanvasInstanceId "browser-cache-demo-canvas" }
          Document = Some entry.Document
          Data = entry.Snapshot.Data
          DocumentRevision = entry.DocumentRevision
          DataRevision = entry.DataRevision
          LastTransportSequence = 2L
          View = { Values = Map.empty }
          Poll = poll
          LastError = None }

    [<SPAEntryPoint>]
    let Main () =
        let status = Var.Create "READY"
        let entries = fixtureEntries ()

        let count () =
            BrowserRuntimeCache.count (function
                | Ok value -> status.Value <- "COUNT:" + string value
                | Error reason -> status.Value <- "UNAVAILABLE:" + reason)

        let seed () =
            status.Value <- "SEEDING"

            BrowserRuntimeCache.clear (fun _ ->
                writeSequentially entries 0 (fun () ->
                    BrowserRuntimeCache.count (function
                        | Ok value -> status.Value <- "SEEDED:" + string value
                        | Error reason -> status.Value <- "UNAVAILABLE:" + reason)))

        let readAt index label requestedCoverage =
            if entries.Length <= index then
                status.Value <- "FIXTURE-MISSING"
            else
                let entry = entries[index]

                BrowserRuntimeCache.readLatest
                    entry.CacheIdentity
                    entry.WorkspaceId
                    requestedCoverage
                    (function
                        | BrowserRuntimeCacheReadResult.Hit value -> status.Value <- label + ":HIT:" + string value.DataRevision
                        | BrowserRuntimeCacheReadResult.Miss -> status.Value <- label + ":MISS"
                        | BrowserRuntimeCacheReadResult.Unavailable reason -> status.Value <- "UNAVAILABLE:" + reason)

        let readCovering index coverageIndex =
            if entries.Length <= index || entries.Length <= coverageIndex then
                status.Value <- "FIXTURE-MISSING"
            else
                let entry = entries[index]

                BrowserRuntimeCache.readCovering
                    entry.CacheIdentity
                    entry.WorkspaceId
                    entries[coverageIndex].Coverage
                    (function
                        | BrowserRuntimeCacheReadResult.Hit value -> status.Value <- "COVERING:HIT:" + string value.DataRevision
                        | BrowserRuntimeCacheReadResult.Miss -> status.Value <- "COVERING:MISS"
                        | BrowserRuntimeCacheReadResult.Unavailable reason -> status.Value <- "UNAVAILABLE:" + reason)

        let clear () =
            BrowserRuntimeCache.clear (function
            | Ok _ -> status.Value <- "CLEARED"
            | Error reason -> status.Value <- "UNAVAILABLE:" + reason)

        let measureEventLoop completed =
            let intervalMs = 10.0
            let startedAt = float (Date.Now())
            let mutable previousTick = startedAt
            let mutable maximumCallbackGapMs = 0.0
            let intervalHandle =
                JS.Window.SetInterval(
                    (fun () ->
                        let now = float (Date.Now())
                        maximumCallbackGapMs <- max maximumCallbackGapMs (now - previousTick)
                        previousTick <- now),
                    int intervalMs)

            fun outcome ->
                JS.SetTimeout
                    (fun () ->
                        JS.Window.ClearInterval intervalHandle
                        let elapsedMs = int (System.Math.Ceiling(float (Date.Now()) - startedAt))
                        let maximumGapMs = int (System.Math.Ceiling maximumCallbackGapMs)
                        completed outcome maximumGapMs elapsedMs)
                    25
                |> ignore

        let seedLarge () =
            status.Value <- "LARGE:SEEDING"

            match fixtureEntry "cache-large-entry" with
            | None -> status.Value <- "LARGE:FIXTURE-MISSING"
            | Some entry ->
                BrowserRuntimeCache.clear (fun _ ->
                    BrowserRuntimeCache.write
                        entry
                        (function
                            | BrowserRuntimeCacheWriteResult.Written -> status.Value <- "LARGE:SEEDED"
                            | BrowserRuntimeCacheWriteResult.Unavailable reason -> status.Value <- "UNAVAILABLE:" + reason))

        let readLarge () =
            match fixtureEntry "cache-large-entry" with
            | None -> status.Value <- "LARGE:FIXTURE-MISSING"
            | Some entry ->
                status.Value <- "LARGE:READING"
                BrowserRuntimeCache.readLatest
                    entry.CacheIdentity
                    entry.WorkspaceId
                    None
                    (measureEventLoop (fun outcome maximumGapMs elapsedMs ->
                        match outcome with
                        | BrowserRuntimeCacheReadResult.Hit cached ->
                            status.Value <- $"LARGE:READ:{cached.Snapshot.Data.Count}:{maximumGapMs}:{elapsedMs}"
                        | BrowserRuntimeCacheReadResult.Miss -> status.Value <- "LARGE:MISS"
                        | BrowserRuntimeCacheReadResult.Unavailable reason -> status.Value <- "UNAVAILABLE:" + reason))

        let rehydrateLarge () =
            match fixtureEntry "cache-large-entry" with
            | None -> status.Value <- "LARGE:FIXTURE-MISSING"
            | Some entry ->
                let current =
                    { runtimeState RuntimePollState.MountedIdle entry with
                        Data = Map.empty
                        DataRevision = 0L
                        LastTransportSequence = 19L }
                let finish =
                    measureEventLoop (fun outcome maximumGapMs elapsedMs ->
                        match outcome with
                        | BrowserRuntimeCachePhasedRehydrateOutcome.Rehydrated hydrated ->
                            let pointCount =
                                match Map.tryFind entry.Document.TemporalAxisRefs[0] hydrated.Data with
                                | Some(SduiValue.Object fields) ->
                                    match Map.tryFind "points" fields with
                                    | Some(SduiValue.Array points) -> points.Length
                                    | _ -> 0
                                | _ -> 0

                            status.Value <-
                                $"LARGE:REHYDRATED:{hydrated.Data.Count}:{pointCount}:{maximumGapMs}:{elapsedMs}"
                        | BrowserRuntimeCachePhasedRehydrateOutcome.Superseded -> status.Value <- "LARGE:SUPERSEDED"
                        | BrowserRuntimeCachePhasedRehydrateOutcome.Miss -> status.Value <- "LARGE:MISS"
                        | BrowserRuntimeCachePhasedRehydrateOutcome.Rejected errors -> status.Value <- $"LARGE:REJECTED:{errors.Length}"
                        | BrowserRuntimeCachePhasedRehydrateOutcome.Unavailable reason -> status.Value <- "UNAVAILABLE:" + reason)

                status.Value <- "LARGE:REHYDRATING"
                BrowserRuntimeCache.rehydrateLatestPhased
                    entry.CacheIdentity
                    entry.WorkspaceId
                    (fun () -> current)
                    (fun () -> true)
                    finish

        let supersedeLargeRehydrate () =
            match fixtureEntry "cache-large-entry" with
            | None -> status.Value <- "LARGE:FIXTURE-MISSING"
            | Some entry ->
                let current =
                    { runtimeState RuntimePollState.MountedIdle entry with
                        Data = Map.empty
                        DataRevision = 0L
                        LastTransportSequence = 19L }
                let mutable currentGeneration = true
                let mutable callbackCount = 0

                status.Value <- "LARGE:SUPERSEDING"
                BrowserRuntimeCache.rehydrateLatestPhased
                    entry.CacheIdentity
                    entry.WorkspaceId
                    (fun () -> current)
                    (fun () -> currentGeneration)
                    (fun outcome ->
                        callbackCount <- callbackCount + 1

                        match outcome with
                        | BrowserRuntimeCachePhasedRehydrateOutcome.Superseded ->
                            status.Value <- $"LARGE:SUPERSEDED:{callbackCount}"
                        | BrowserRuntimeCachePhasedRehydrateOutcome.Rehydrated _ ->
                            status.Value <- $"LARGE:UNEXPECTED-REHYDRATED:{callbackCount}"
                        | BrowserRuntimeCachePhasedRehydrateOutcome.Miss -> status.Value <- "LARGE:MISS"
                        | BrowserRuntimeCachePhasedRehydrateOutcome.Rejected errors -> status.Value <- $"LARGE:REJECTED:{errors.Length}"
                        | BrowserRuntimeCachePhasedRehydrateOutcome.Unavailable reason -> status.Value <- "UNAVAILABLE:" + reason)

                JS.SetTimeout (fun () -> currentGeneration <- false) 25 |> ignore

        let writeAccepted () =
            if entries.Length <= 9 then
                status.Value <- "FIXTURE-MISSING"
            else
                let entry = entries[9]
                BrowserRuntimeCache.clear (fun _ ->
                    BrowserRuntimeCache.writeAcceptedState
                        entry.CacheIdentity
                        (runtimeState RuntimePollState.Ready entry)
                        (function
                            | BrowserRuntimeCacheAcceptedStateWriteResult.Written -> status.Value <- "ACCEPTED:WRITTEN"
                            | BrowserRuntimeCacheAcceptedStateWriteResult.Rejected errors ->
                                let reason = errors |> List.tryHead |> Option.map (fun error -> error.Code + ":" + error.Field) |> Option.defaultValue "unknown"
                                status.Value <- "ACCEPTED:REJECTED:" + reason
                            | BrowserRuntimeCacheAcceptedStateWriteResult.Unavailable reason -> status.Value <- "UNAVAILABLE:" + reason))

        let rejectPausedState () =
            if entries.Length <= 9 then
                status.Value <- "FIXTURE-MISSING"
            else
                let entry = entries[9]
                BrowserRuntimeCache.writeAcceptedState
                    entry.CacheIdentity
                    (runtimeState RuntimePollState.PausedForResync entry)
                    (function
                        | BrowserRuntimeCacheAcceptedStateWriteResult.Rejected errors ->
                            let reason = errors |> List.tryHead |> Option.map (fun error -> error.Code + ":" + error.Field) |> Option.defaultValue "unknown"
                            status.Value <- "PAUSED:REJECTED:" + reason
                        | BrowserRuntimeCacheAcceptedStateWriteResult.Written -> status.Value <- "PAUSED:WRITTEN"
                        | BrowserRuntimeCacheAcceptedStateWriteResult.Unavailable reason -> status.Value <- "UNAVAILABLE:" + reason)

        let readAcceptedProjection () =
            if entries.Length <= 9 then
                status.Value <- "FIXTURE-MISSING"
            else
                let source = entries[9]

                BrowserRuntimeCache.readLatest
                    source.CacheIdentity
                    source.WorkspaceId
                    None
                    (function
                        | BrowserRuntimeCacheReadResult.Hit cached ->
                            let axisRef = cached.Document.TemporalAxisRefs[0]

                            match Map.tryFind axisRef cached.Snapshot.Data with
                            | Some axisValue ->
                                let previewCount =
                                    RuntimeReducer.temporalObject "temporal-axis.v1" axisValue
                                    |> Option.map RuntimeReducer.temporalPointMaps
                                    |> Option.defaultValue [||]
                                    |> Array.filter (fun point -> RuntimeReducer.temporalText "finality" point = Some "preview")
                                    |> Array.length

                                match RuntimeCacheBrowserCoverage.decodeAxis axisValue with
                                | Ok points -> status.Value <- "ACCEPTED-PROJECTION:" + string points.Length + ":" + string previewCount
                                | Error _ -> status.Value <- "ACCEPTED-PROJECTION:INVALID"
                            | _ -> status.Value <- "ACCEPTED-PROJECTION:INVALID"
                        | BrowserRuntimeCacheReadResult.Miss -> status.Value <- "ACCEPTED-PROJECTION:MISS"
                        | BrowserRuntimeCacheReadResult.Unavailable reason -> status.Value <- "UNAVAILABLE:" + reason)

        let rehydrate () =
            if entries.Length <= 9 then
                status.Value <- "FIXTURE-MISSING"
            else
                let entry = entries[9]
                let current =
                    { runtimeState RuntimePollState.MountedIdle entry with
                        Data = Map.empty
                        DataRevision = 0L
                        LastTransportSequence = 19L }

                BrowserRuntimeCache.readLatest
                    entry.CacheIdentity
                    entry.WorkspaceId
                    None
                    (function
                        | BrowserRuntimeCacheReadResult.Hit cached ->
                            match BrowserRuntimeCache.tryRehydrate entry.CacheIdentity current cached with
                            | Ok hydrated ->
                                let poll = if hydrated.Poll = RuntimePollState.PausedForResync then "PAUSED" else "UNEXPECTED"
                                status.Value <- "REHYDRATED:" + string hydrated.DataRevision + ":" + string hydrated.LastTransportSequence + ":" + poll
                            | Error errors -> status.Value <- "REHYDRATE:REJECTED:" + string errors.Length
                        | BrowserRuntimeCacheReadResult.Miss -> status.Value <- "REHYDRATE:MISS"
                        | BrowserRuntimeCacheReadResult.Unavailable reason -> status.Value <- "UNAVAILABLE:" + reason)

        let seedCorrupt () =
            if entries.Length <= 9 then
                status.Value <- "FIXTURE-MISSING"
            else
                let source = entries[9]
                let key = "browser-cache-corrupt"
                let record =
                    { Key = key
                      LookupKey = BrowserRuntimeCache.lookupKeyFor source.CacheIdentity source.WorkspaceId
                      EntryHeaderJson = "{not-json"
                      DataItems = [||]
                      TouchedAtTicks = string System.DateTime.UtcNow.Ticks }

                BrowserRuntimeCache.withStore
                    "readwrite"
                    (fun tx store ->
                        JS.Set tx "oncomplete" (System.Action<obj>(fun _ -> status.Value <- "CORRUPT:SEEDED"))
                        JS.Set tx "onabort" (System.Action<obj>(fun _ -> status.Value <- "UNAVAILABLE:indexeddb-corrupt-seed-aborted"))
                        JS.Set tx "onerror" (System.Action<obj>(fun _ -> status.Value <- "UNAVAILABLE:indexeddb-corrupt-seed-failed"))
                        JS.Apply<obj> store "put" [| box record |] |> ignore)
                    (fun reason -> status.Value <- "UNAVAILABLE:" + reason)

        let seedSemanticInvalid () =
            if entries.Length <= 9 then
                status.Value <- "FIXTURE-MISSING"
            else
                let source = entries[9]
                let invalid =
                    { source with
                        Document =
                            { source.Document with
                                WorkspaceId = source.WorkspaceId + "-mismatch" } }
                let key = "browser-cache-semantic-invalid"
                let record = BrowserRuntimeCache.recordForEntry key (string System.DateTime.UtcNow.Ticks) invalid
                let record =
                    { record with
                        LookupKey = BrowserRuntimeCache.lookupKeyFor source.CacheIdentity source.WorkspaceId }

                BrowserRuntimeCache.withStore
                    "readwrite"
                    (fun tx store ->
                        JS.Set tx "oncomplete" (System.Action<obj>(fun _ -> status.Value <- "SEMANTIC-INVALID:SEEDED"))
                        JS.Set tx "onabort" (System.Action<obj>(fun _ -> status.Value <- "UNAVAILABLE:indexeddb-semantic-seed-aborted"))
                        JS.Set tx "onerror" (System.Action<obj>(fun _ -> status.Value <- "UNAVAILABLE:indexeddb-semantic-seed-failed"))
                        JS.Apply<obj> store "put" [| box record |] |> ignore)
                    (fun reason -> status.Value <- "UNAVAILABLE:" + reason)

        let buttonStyle = attr.style "min-height:32px; padding:4px 10px; border:1px solid #8795a6; background:#fff; cursor:pointer;"

        div [ attr.style "max-width:720px; margin:32px auto; padding:20px; font-family:Segoe UI,sans-serif;" ] [
            h1 [ attr.style "font-size:20px; font-weight:600;" ] [ text "Interactive browser cache gate" ]
            div [ attr.style "display:flex; flex-wrap:wrap; gap:8px;" ] [
                button [ buttonStyle; Attr.Create "data-testid" "cache-seed"; on.click (fun _ _ -> seed ()) ] [ text "Seed 10" ]
                button [ buttonStyle; Attr.Create "data-testid" "cache-count"; on.click (fun _ _ -> count ()) ] [ text "Count" ]
                button [ buttonStyle; Attr.Create "data-testid" "cache-hit"; on.click (fun _ _ -> readAt 9 "LATEST" None) ] [ text "Read latest" ]
                button [ buttonStyle; Attr.Create "data-testid" "cache-evicted"; on.click (fun _ _ -> readAt 0 "OLDEST" None) ] [ text "Read evicted" ]
                button [ buttonStyle; Attr.Create "data-testid" "cache-covering-hit"; on.click (fun _ _ -> readCovering 9 9) ] [ text "Covering hit" ]
                button [ buttonStyle; Attr.Create "data-testid" "cache-coverage-miss"; on.click (fun _ _ -> readCovering 9 0) ] [ text "Coverage miss" ]
                button [ buttonStyle; Attr.Create "data-testid" "cache-clear"; on.click (fun _ _ -> clear ()) ] [ text "Clear" ]
                button [ buttonStyle; Attr.Create "data-testid" "cache-seed-large"; on.click (fun _ _ -> seedLarge ()) ] [ text "Seed large" ]
                button [ buttonStyle; Attr.Create "data-testid" "cache-read-large"; on.click (fun _ _ -> readLarge ()) ] [ text "Read large" ]
                button [ buttonStyle; Attr.Create "data-testid" "cache-rehydrate-large"; on.click (fun _ _ -> rehydrateLarge ()) ] [ text "Rehydrate large" ]
                button [ buttonStyle; Attr.Create "data-testid" "cache-supersede-large"; on.click (fun _ _ -> supersedeLargeRehydrate ()) ] [ text "Supersede large" ]
                button [ buttonStyle; Attr.Create "data-testid" "cache-write-accepted"; on.click (fun _ _ -> writeAccepted ()) ] [ text "Write accepted state" ]
                button [ buttonStyle; Attr.Create "data-testid" "cache-read-accepted-projection"; on.click (fun _ _ -> readAcceptedProjection ()) ] [ text "Read accepted projection" ]
                button [ buttonStyle; Attr.Create "data-testid" "cache-reject-paused"; on.click (fun _ _ -> rejectPausedState ()) ] [ text "Reject paused state" ]
                button [ buttonStyle; Attr.Create "data-testid" "cache-rehydrate"; on.click (fun _ _ -> rehydrate ()) ] [ text "Rehydrate" ]
                button [ buttonStyle; Attr.Create "data-testid" "cache-seed-corrupt"; on.click (fun _ _ -> seedCorrupt ()) ] [ text "Seed corrupt" ]
                button [ buttonStyle; Attr.Create "data-testid" "cache-read-corrupt"; on.click (fun _ _ -> readAt 9 "CORRUPT" None) ] [ text "Read corrupt" ]
                button [ buttonStyle; Attr.Create "data-testid" "cache-seed-semantic-invalid"; on.click (fun _ _ -> seedSemanticInvalid ()) ] [ text "Seed semantic invalid" ]
                button [ buttonStyle; Attr.Create "data-testid" "cache-read-semantic-invalid"; on.click (fun _ _ -> readAt 9 "SEMANTIC-INVALID" None) ] [ text "Read semantic invalid" ]
            ]
            output [ Attr.Create "data-testid" "cache-status"; attr.style "display:block; margin-top:16px; padding:10px; border:1px solid #c7ced8; font-family:Consolas,monospace;" ] [ textView status.View ]
        ]
        |> Doc.RunById "app"
