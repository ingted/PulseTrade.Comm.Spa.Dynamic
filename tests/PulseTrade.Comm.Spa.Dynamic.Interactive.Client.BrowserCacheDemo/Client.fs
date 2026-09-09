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
    let fixtureEntries () =
        [| 0..9 |]
        |> Array.choose (fun index ->
            let node = JS.Document.GetElementById("cache-entry-" + string index)

            if isNull node then
                None
            else
                match BrowserRuntimeCodec.decodeCacheEntry node.TextContent with
                | Ok entry -> Some entry
                | Error _ -> None)

    let rec writeSequentially (entries: RuntimeCacheEntry array) (index: int) (completed: unit -> unit) =
        if index >= entries.Length then
            completed ()
        else
            BrowserRuntimeCache.write
                entries[index]
                (function
                    | BrowserRuntimeCacheWriteResult.Written -> writeSequentially entries (index + 1) completed
                    | BrowserRuntimeCacheWriteResult.Unavailable _ -> completed ())

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

        let seedCorrupt () =
            let key = "browser-cache-corrupt"
            let record =
                { Key = key
                  EntryJson = "{not-json"
                  TouchedAtTicks = string System.DateTime.UtcNow.Ticks }

            BrowserRuntimeCache.withStore
                "readwrite"
                (fun tx store ->
                    JS.Set tx "oncomplete" (System.Action<obj>(fun _ -> status.Value <- "CORRUPT:SEEDED"))
                    JS.Set tx "onabort" (System.Action<obj>(fun _ -> status.Value <- "UNAVAILABLE:indexeddb-corrupt-seed-aborted"))
                    JS.Set tx "onerror" (System.Action<obj>(fun _ -> status.Value <- "UNAVAILABLE:indexeddb-corrupt-seed-failed"))
                    JS.Apply<obj> store "put" [| box (Json.Serialize record); box key |] |> ignore)
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
                button [ buttonStyle; Attr.Create "data-testid" "cache-seed-corrupt"; on.click (fun _ _ -> seedCorrupt ()) ] [ text "Seed corrupt" ]
                button [ buttonStyle; Attr.Create "data-testid" "cache-read-corrupt"; on.click (fun _ _ -> readAt 9 "CORRUPT" None) ] [ text "Read corrupt" ]
            ]
            output [ Attr.Create "data-testid" "cache-status"; attr.style "display:block; margin-top:16px; padding:10px; border:1px solid #c7ced8; font-family:Consolas,monospace;" ] [ textView status.View ]
        ]
        |> Doc.RunById "app"
