namespace PulseTrade.Comm.Spa.Dynamic.Interactive.Client

open PulseTrade.Comm.Spa.Dynamic.Contracts
open WebSharper
open WebSharper.JavaScript

[<JavaScript>]
type BrowserRuntimeCacheRecord =
    { Key: string
      EntryJson: string
      TouchedAtTicks: string }

[<JavaScript; RequireQualifiedAccess>]
type BrowserRuntimeCacheReadResult =
    | Hit of RuntimeCacheEntry
    | Miss
    | Unavailable of reasonCode: string

[<JavaScript; RequireQualifiedAccess>]
type BrowserRuntimeCacheWriteResult =
    | Written
    | Unavailable of reasonCode: string

/// Bounded, non-authoritative browser persistence for accepted Dynamic runtime projections.
/// Every returned entry still has to pass the runtime reducer before it may be rendered.
[<JavaScript; RequireQualifiedAccess>]
module BrowserRuntimeCache =
    [<Literal>]
    let DatabaseName = "PulseTrade.Comm.Spa.Dynamic.Interactive"

    [<Literal>]
    let StoreName = "runtimeSnapshots"

    [<Literal>]
    let DatabaseVersion = 1

    let isMissing (value: obj) =
        isNull value || JS.TypeOf value = JS.Kind.Undefined

    let eventResult (event: obj) =
        let target = JS.Get<obj> "target" event
        if isMissing target then null else JS.Get<obj> "result" target

    let once callback =
        let mutable completed = false

        fun value ->
            if not completed then
                completed <- true
                callback value

    let ensureStore (db: obj) =
        let names = JS.Get<obj> "objectStoreNames" db

        let exists =
            if isMissing names then
                false
            else
                try
                    JS.Apply<bool> names "contains" [| box StoreName |]
                with _ ->
                    false

        if not exists then
            JS.Apply<obj> db "createObjectStore" [| box StoreName |] |> ignore

    let openDb onReady onUnavailable =
        let unavailable = once onUnavailable

        try
            let indexedDb = JS.Get<obj> "indexedDB" JS.Window

            if isMissing indexedDb then
                unavailable "indexeddb-unavailable"
            else
                let request = JS.Apply<obj> indexedDb "open" [| box DatabaseName; box DatabaseVersion |]

                JS.Set
                    request
                    "onupgradeneeded"
                    (System.Action<obj>(fun event ->
                        let db = eventResult event
                        if not (isMissing db) then ensureStore db))

                JS.Set
                    request
                    "onsuccess"
                    (System.Action<obj>(fun event ->
                        let db = eventResult event
                        if isMissing db then unavailable "indexeddb-open-empty" else onReady db))

                JS.Set request "onerror" (System.Action<obj>(fun _ -> unavailable "indexeddb-open-failed"))
                JS.Set request "onblocked" (System.Action<obj>(fun _ -> unavailable "indexeddb-open-blocked"))
        with _ ->
            unavailable "indexeddb-open-exception"

    let withStore mode onStore onUnavailable =
        openDb
            (fun db ->
                try
                    let tx = JS.Apply<obj> db "transaction" [| box [| StoreName |]; box mode |]
                    let store = JS.Apply<obj> tx "objectStore" [| box StoreName |]
                    onStore tx store
                with _ ->
                    onUnavailable "indexeddb-transaction-failed")
            onUnavailable

    let decodeRecord text =
        try
            let record: BrowserRuntimeCacheRecord = Json.Deserialize text

            if isNull (box record)
               || System.String.IsNullOrWhiteSpace record.Key
               || System.String.IsNullOrWhiteSpace record.EntryJson then
                None
            else
                Some record
        with _ ->
            None

    let touchedAt record =
        match System.Int64.TryParse record.TouchedAtTicks with
        | true, value -> value
        | false, _ -> 0L

    let readAll onRead onUnavailable =
        let complete = once onRead

        withStore
            "readonly"
            (fun _ store ->
                try
                    let request = JS.Apply<obj> store "getAll" [||]

                    JS.Set
                        request
                        "onsuccess"
                        (System.Action<obj>(fun event ->
                            let value = eventResult event

                            if isMissing value then
                                complete [||]
                            else
                                try
                                    value
                                    |> As<string[]>
                                    |> Array.choose decodeRecord
                                    |> complete
                                with _ ->
                                    complete [||]))

                    JS.Set request "onerror" (System.Action<obj>(fun _ -> onUnavailable "indexeddb-read-failed"))
                with _ ->
                    onUnavailable "indexeddb-read-exception")
            onUnavailable

    let deleteKeys keys onCompleted =
        let keys = keys |> Array.filter (System.String.IsNullOrWhiteSpace >> not) |> Array.distinct

        if keys.Length = 0 then
            onCompleted ()
        else
            let complete = once (fun _ -> onCompleted ())

            withStore
                "readwrite"
                (fun tx store ->
                    JS.Set tx "oncomplete" (System.Action<obj>(fun _ -> complete ()))
                    JS.Set tx "onabort" (System.Action<obj>(fun _ -> complete ()))
                    JS.Set tx "onerror" (System.Action<obj>(fun _ -> complete ()))

                    try
                        keys |> Array.iter (fun key -> JS.Apply<obj> store "delete" [| box key |] |> ignore)
                    with _ ->
                        complete ())
                (fun _ -> complete ())

    let compact onCompleted =
        readAll
            (fun records ->
                let overflow = records.Length - RuntimeCache.MaximumEntries

                if overflow <= 0 then
                    onCompleted ()
                else
                    records
                    |> Array.sortBy touchedAt
                    |> Array.truncate overflow
                    |> Array.map _.Key
                    |> fun keys -> deleteKeys keys onCompleted)
            (fun _ -> onCompleted ())

    let keyFor entry nowTicks =
        System.String.Concat(
            entry.CacheIdentity.OwnerFingerprint,
            "|",
            string entry.CacheIdentity.SchemaRevision,
            "|",
            entry.WorkspaceId,
            "|",
            nowTicks)

    let write entry continuation =
        let complete = once continuation

        match BrowserRuntimeCodec.encodeCacheEntry entry with
        | null -> complete (BrowserRuntimeCacheWriteResult.Unavailable "cache-encode-empty")
        | entryJson ->
            let nowTicks = string System.DateTime.UtcNow.Ticks
            let key = keyFor entry nowTicks
            let record =
                { Key = key
                  EntryJson = entryJson
                  TouchedAtTicks = nowTicks }

            withStore
                "readwrite"
                (fun tx store ->
                    JS.Set
                        tx
                        "oncomplete"
                        (System.Action<obj>(fun _ ->
                            compact (fun () -> complete BrowserRuntimeCacheWriteResult.Written)))

                    JS.Set tx "onabort" (System.Action<obj>(fun _ -> complete (BrowserRuntimeCacheWriteResult.Unavailable "indexeddb-write-aborted")))
                    JS.Set tx "onerror" (System.Action<obj>(fun _ -> complete (BrowserRuntimeCacheWriteResult.Unavailable "indexeddb-write-failed")))

                    try
                        JS.Apply<obj> store "put" [| box (Json.Serialize record); box key |] |> ignore
                    with _ ->
                        complete (BrowserRuntimeCacheWriteResult.Unavailable "indexeddb-write-exception"))
                (fun reason -> complete (BrowserRuntimeCacheWriteResult.Unavailable reason))

    let readMatching identityMatches cacheIdentity workspaceId (requestedCoverage: RuntimeCacheCoverage option) continuation =
        let complete = once continuation

        readAll
            (fun records ->
                let invalidKeys = ResizeArray<string>()

                let matches =
                    records
                    |> Array.choose (fun record ->
                        match BrowserRuntimeCodec.decodeCacheEntry record.EntryJson with
                        | Error _ ->
                            invalidKeys.Add record.Key
                            None
                        | Ok entry ->
                            match RuntimeCacheEntryValidation.validate DynamicRuntimeDefaults.limits entry with
                            | Error _ ->
                                invalidKeys.Add record.Key
                                None
                            | Ok valid when identityMatches valid.CacheIdentity cacheIdentity && valid.WorkspaceId = workspaceId ->
                                let covers =
                                    match requestedCoverage with
                                    | None -> true
                                    | Some requested ->
                                        valid.Coverage.StartEventTimeUtc <= requested.StartEventTimeUtc
                                        && valid.Coverage.EndEventTimeExclusiveUtc >= requested.EndEventTimeExclusiveUtc

                                if covers then Some(record, valid) else None
                            | Ok _ -> None)
                    |> Array.sortByDescending (fst >> touchedAt)

                let result =
                    match Array.tryHead matches with
                    | Some(_, entry) -> BrowserRuntimeCacheReadResult.Hit entry
                    | None -> BrowserRuntimeCacheReadResult.Miss

                deleteKeys (invalidKeys.ToArray()) (fun () -> complete result))
            (fun reason -> complete (BrowserRuntimeCacheReadResult.Unavailable reason))

    let readLatest cacheIdentity workspaceId requestedCoverage continuation =
        readMatching (=) cacheIdentity workspaceId requestedCoverage continuation

    let readCovering cacheIdentity workspaceId requestedCoverage continuation =
        readMatching (=) cacheIdentity workspaceId (Some requestedCoverage) continuation

    let clear continuation =
        let complete = once continuation

        withStore
            "readwrite"
            (fun tx store ->
                JS.Set tx "oncomplete" (System.Action<obj>(fun _ -> complete (Result.Ok())))
                JS.Set tx "onabort" (System.Action<obj>(fun _ -> complete (Result.Error "indexeddb-clear-aborted")))
                JS.Set tx "onerror" (System.Action<obj>(fun _ -> complete (Result.Error "indexeddb-clear-failed")))

                try
                    JS.Apply<obj> store "clear" [||] |> ignore
                with _ ->
                    complete (Result.Error "indexeddb-clear-exception"))
            (fun reason -> complete (Result.Error reason))

    let count continuation =
        let complete = once continuation

        withStore
            "readonly"
            (fun _ store ->
                try
                    let request = JS.Apply<obj> store "count" [||]

                    JS.Set
                        request
                        "onsuccess"
                        (System.Action<obj>(fun event ->
                            let value = eventResult event
                            if isMissing value then complete (Result.Error "indexeddb-count-empty") else complete (Result.Ok(As<int> value))))

                    JS.Set request "onerror" (System.Action<obj>(fun _ -> complete (Result.Error "indexeddb-count-failed")))
                with _ ->
                    complete (Result.Error "indexeddb-count-exception"))
            (fun reason -> complete (Result.Error reason))
