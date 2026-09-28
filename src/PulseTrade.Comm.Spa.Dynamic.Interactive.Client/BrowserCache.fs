namespace PulseTrade.Comm.Spa.Dynamic.Interactive.Client

open System
open PulseTrade.Comm.Spa.Dynamic.Contracts
open WebSharper
open WebSharper.JavaScript

[<JavaScript>]
type BrowserRuntimeCacheDataItem =
    { DataRef: string
      ValueJson: string }

[<JavaScript>]
type BrowserRuntimeCacheRecord =
    { Key: string
      LookupKey: string
      EntryHeaderJson: string
      DataItems: BrowserRuntimeCacheDataItem array
      TouchedAtTicks: string }

[<JavaScript; RequireQualifiedAccess>]
type BrowserRuntimeCacheReadResult =
    | Hit of RuntimeCacheEntry
    | Miss
    | Unavailable of reasonCode: string

[<JavaScript>]
type BrowserRuntimeCacheAdjacentQuery =
    { AcceptedProjection: TaLoadedCoverageProjection
      Direction: TaCoverageDirection
      MaximumObservations: int }

[<JavaScript>]
type BrowserRuntimeCacheAdjacentHit =
    { SourceEntry: RuntimeCacheEntry
      RebasedEntry: RuntimeCacheEntry }

[<JavaScript>]
type BrowserRuntimeCacheKnownEmptySpan =
    { StartEventTimeUtc: string
      EndEventTimeExclusiveUtc: string }

[<JavaScript; RequireQualifiedAccess>]
type BrowserRuntimeCacheAdjacentSelection =
    | Hit of BrowserRuntimeCacheAdjacentHit
    | KnownEmpty of BrowserRuntimeCacheKnownEmptySpan
    | Miss

[<JavaScript; RequireQualifiedAccess>]
type BrowserRuntimeCacheAdjacentReadResult =
    | Hit of RuntimeCacheEntry
    | KnownEmpty of BrowserRuntimeCacheKnownEmptySpan
    | Miss
    | Unavailable of reasonCode: string

[<JavaScript; RequireQualifiedAccess>]
type BrowserRuntimeCacheWriteResult =
    | Written
    | Unavailable of reasonCode: string

[<JavaScript; RequireQualifiedAccess>]
type BrowserRuntimeCacheAcceptedStateWriteResult =
    | Written
    | Rejected of errors: DynamicValidationError list
    | Unavailable of reasonCode: string

[<JavaScript; RequireQualifiedAccess>]
type BrowserRuntimeCachePhasedWriteOutcome =
    | Completed of BrowserRuntimeCacheAcceptedStateWriteResult
    | CancelledBeforeWrite

[<JavaScript; RequireQualifiedAccess>]
type BrowserRuntimeCachePhasedRehydrateOutcome =
    | Rehydrated of RuntimeState
    | Miss
    | Rejected of errors: DynamicValidationError list
    | Unavailable of reasonCode: string
    | Superseded

/// Bounded, non-authoritative browser persistence for accepted Dynamic runtime projections.
/// Every returned entry still has to pass the runtime reducer before it may be rendered.
[<JavaScript; RequireQualifiedAccess>]
module BrowserRuntimeCache =
    [<Literal>]
    let DatabaseName = "PulseTrade.Comm.Spa.Dynamic.Interactive"

    [<Literal>]
    let StoreName = "runtimeSnapshots"

    [<Literal>]
    let DatabaseVersion = 3

    [<Literal>]
    let LookupIndexName = "lookupKey"

    [<Literal>]
    let TouchedAtIndexName = "touchedAtTicks"

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

    let recreateStore (db: obj) =
        let names = JS.Get<obj> "objectStoreNames" db

        let exists =
            if isMissing names then
                false
            else
                try
                    JS.Apply<bool> names "contains" [| box StoreName |]
                with _ ->
                    false

        if exists then
            JS.Apply<obj> db "deleteObjectStore" [| box StoreName |] |> ignore

        let options = New [ "keyPath" => box "Key" ]
        let store = JS.Apply<obj> db "createObjectStore" [| box StoreName; box options |]
        JS.Apply<obj> store "createIndex" [| box LookupIndexName; box "LookupKey" |] |> ignore
        JS.Apply<obj> store "createIndex" [| box TouchedAtIndexName; box "TouchedAtTicks" |] |> ignore

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
                        if not (isMissing db) then recreateStore db))

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

    let validateRecord (record: BrowserRuntimeCacheRecord) =
        if isNull (box record)
           || System.String.IsNullOrWhiteSpace record.Key
           || System.String.IsNullOrWhiteSpace record.LookupKey
           || System.String.IsNullOrWhiteSpace record.EntryHeaderJson
           || isNull record.DataItems then
            None
        else
            Some record

    let decodeRecordValue (value: obj) =
        try
            if isMissing value then None
            elif JS.TypeOf value = JS.Kind.String then
                let record: BrowserRuntimeCacheRecord = Json.Deserialize(As<string> value)
                validateRecord record
            else
                value |> As<BrowserRuntimeCacheRecord> |> validateRecord
        with _ ->
            None

    let touchedAt record =
        match System.Int64.TryParse record.TouchedAtTicks with
        | true, value -> value
        | false, _ -> 0L

    let intervalCoveredBySegments
        (startOrdinal: int64)
        (observationCount: int)
        (segments: TaLoadedCoverageSegment array)
        =
        if startOrdinal < 0L || observationCount <= 0 then
            false
        else
            let endOrdinal = startOrdinal + int64 observationCount

            if endOrdinal <= startOrdinal then
                false
            else
                segments
                |> Array.filter (fun segment -> segment.ObservationCount > 0L)
                |> Array.sortBy _.StartObservationOrdinal
                |> Array.fold
                    (fun (coveredUntil, hasGap) segment ->
                        if hasGap || coveredUntil >= endOrdinal then
                            coveredUntil, hasGap
                        else
                            let segmentStart = segment.StartObservationOrdinal
                            let segmentEnd = segmentStart + segment.ObservationCount

                            if segmentEnd <= coveredUntil || segmentEnd <= startOrdinal then
                                coveredUntil, false
                            elif segmentStart > coveredUntil then
                                coveredUntil, true
                            else
                                max coveredUntil segmentEnd, false)
                    (startOrdinal, false)
                |> fun (coveredUntil, hasGap) -> not hasGap && coveredUntil >= endOrdinal

    let tryRebaseAdjacentEntry (query: BrowserRuntimeCacheAdjacentQuery) (entry: RuntimeCacheEntry) =
        let accepted = query.AcceptedProjection

        if
            query.MaximumObservations <= 0
            || query.MaximumObservations > TaLoadedCoverageCodec.MaximumActiveDetailBars
            || not (TaLoadedCoverageCodec.validationErrors accepted |> List.isEmpty)
        then
            None
        else
            match TaLoadedCoverageCodec.tryDecode entry.Document.DefaultView with
            | Ok(Some cached)
                when cached.CoverageIdentity = accepted.CoverageIdentity
                     && cached.CoverageRevision <= accepted.CoverageRevision
                     && cached.ActiveDetail.ObservationCount <= query.MaximumObservations ->
                let rebasedDetail =
                    if cached.CoverageRevision = accepted.CoverageRevision then
                        if
                            intervalCoveredBySegments
                                cached.ActiveDetail.StartObservationOrdinal
                                cached.ActiveDetail.ObservationCount
                                accepted.Segments
                        then
                            Some cached.ActiveDetail
                        else
                            None
                    else
                        let cachedPageSegments =
                            cached.Segments
                            |> Array.filter (fun segment ->
                                segment.StartObservationOrdinal = cached.ActiveDetail.StartObservationOrdinal
                                && segment.ObservationCount = int64 cached.ActiveDetail.ObservationCount)
                            |> Array.distinctBy (fun segment -> segment.StartObservationOrdinal, segment.ObservationCount)

                        if cachedPageSegments.Length = 1 then
                            let cachedPage = cachedPageSegments[0]
                            let exactTemporalSegments =
                                accepted.Segments
                                |> Array.filter (fun segment ->
                                    segment.ObservationCount = cachedPage.ObservationCount
                                    && segment.StartEventTimeUtc = cachedPage.StartEventTimeUtc
                                    && segment.EndEventTimeExclusiveUtc = cachedPage.EndEventTimeExclusiveUtc)
                                |> Array.distinctBy (fun segment -> segment.StartObservationOrdinal, segment.ObservationCount)

                            if exactTemporalSegments.Length = 1 then
                                Some
                                    { cached.ActiveDetail with
                                        StartObservationOrdinal = exactTemporalSegments[0].StartObservationOrdinal }
                            else
                                None
                        else
                            None

                rebasedDetail
                |> Option.map (fun activeDetail ->
                    let rebased =
                        { accepted with
                            ActiveDetail = activeDetail }

                    { entry with
                        Document =
                            { entry.Document with
                                DefaultView = entry.Document.DefaultView |> TaLoadedCoverageCodec.apply rebased } })
            | _ -> None

    let selectAdjacent query (entries: RuntimeCacheEntry array) =
        let ordinalDistance projection =
            let current = query.AcceptedProjection.ActiveDetail
            let candidate = projection.ActiveDetail
            match query.Direction with
            | TaCoverageDirection.Earlier -> current.StartObservationOrdinal - (candidate.StartObservationOrdinal + int64 candidate.ObservationCount)
            | TaCoverageDirection.Later -> candidate.StartObservationOrdinal - (current.StartObservationOrdinal + int64 current.ObservationCount)

        let directionCompatible projection =
            let current = query.AcceptedProjection.ActiveDetail
            let candidate = projection.ActiveDetail
            match query.Direction with
            | TaCoverageDirection.Earlier ->
                candidate.StartObservationOrdinal + int64 candidate.ObservationCount <= current.StartObservationOrdinal
            | TaCoverageDirection.Later ->
                candidate.StartObservationOrdinal >= current.StartObservationOrdinal + int64 current.ObservationCount

        let hit =
            entries
            |> Array.choose (fun entry -> tryRebaseAdjacentEntry query entry |> Option.map (fun rebased -> entry, rebased))
            |> Array.choose (fun (entry, rebased) ->
                match TaLoadedCoverageCodec.tryDecode rebased.Document.DefaultView with
                | Ok(Some projection)
                    when projection.ActiveDetail.ObservationCount > 0
                         && directionCompatible projection -> Some(entry, rebased, projection)
                | _ -> None)
            |> Array.sortBy (fun (_, _, projection) -> ordinalDistance projection)
            |> Array.tryHead
            |> Option.map (fun (source, rebased, _) ->
                { SourceEntry = source
                  RebasedEntry = rebased })

        match hit with
        | Some hit -> BrowserRuntimeCacheAdjacentSelection.Hit hit
        | None ->
            let knownEmpty =
                query.AcceptedProjection.Segments
                |> Array.filter (fun segment -> segment.ObservationCount = 0L)
                |> Array.filter (fun segment ->
                    match query.Direction with
                    | TaCoverageDirection.Earlier -> segment.StartObservationOrdinal <= query.AcceptedProjection.ActiveDetail.StartObservationOrdinal
                    | TaCoverageDirection.Later ->
                        segment.StartObservationOrdinal
                        >= query.AcceptedProjection.ActiveDetail.StartObservationOrdinal
                           + int64 query.AcceptedProjection.ActiveDetail.ObservationCount)
                |> Array.sortBy (fun segment -> abs (segment.StartObservationOrdinal - query.AcceptedProjection.ActiveDetail.StartObservationOrdinal))
                |> Array.tryHead
                |> Option.map (fun segment ->
                    { StartEventTimeUtc = segment.StartEventTimeUtc
                      EndEventTimeExclusiveUtc = segment.EndEventTimeExclusiveUtc })

            knownEmpty
            |> Option.map BrowserRuntimeCacheAdjacentSelection.KnownEmpty
            |> Option.defaultValue BrowserRuntimeCacheAdjacentSelection.Miss

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
                                    |> As<obj[]>
                                    |> Array.choose decodeRecordValue
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
        let complete = once (fun _ -> onCompleted ())

        withStore
            "readwrite"
            (fun tx store ->
                JS.Set tx "oncomplete" (System.Action<obj>(fun _ -> complete ()))
                JS.Set tx "onabort" (System.Action<obj>(fun _ -> complete ()))
                JS.Set tx "onerror" (System.Action<obj>(fun _ -> complete ()))

                try
                    let index = JS.Apply<obj> store "index" [| box TouchedAtIndexName |]
                    let request = JS.Apply<obj> index "openCursor" [| null; box "prev" |]
                    let mutable retained = 0

                    JS.Set
                        request
                        "onsuccess"
                        (System.Action<obj>(fun event ->
                            let cursor = eventResult event

                            if not (isMissing cursor) then
                                if retained < RuntimeCache.MaximumEntries then
                                    retained <- retained + 1
                                else
                                    JS.Apply<obj> cursor "delete" [||] |> ignore

                                JS.Apply<obj> cursor "continue" [||] |> ignore))

                    JS.Set request "onerror" (System.Action<obj>(fun _ -> complete ()))
                with _ ->
                    complete ())
            (fun _ -> complete ())

    let lookupKeyFor cacheIdentity workspaceId =
        System.String.Concat(
            cacheIdentity.OwnerFingerprint,
            "|",
            string cacheIdentity.SchemaRevision,
            "|",
            workspaceId)

    let keyFor entry nowTicks =
        System.String.Concat(
            lookupKeyFor entry.CacheIdentity entry.WorkspaceId,
            "|",
            nowTicks)

    let recordForEntry key touchedAtTicks (entry: RuntimeCacheEntry) =
        let header =
            { entry with
                Snapshot =
                    { entry.Snapshot with
                        Data = Map.empty } }

        { Key = key
          LookupKey = lookupKeyFor entry.CacheIdentity entry.WorkspaceId
          EntryHeaderJson = BrowserRuntimeCodec.encodeCacheEntry header
          DataItems =
            entry.Snapshot.Data
            |> Map.toArray
            |> Array.map (fun (dataRef, value) ->
                { DataRef = dataRef
                  ValueJson = Json.Serialize value })
          TouchedAtTicks = touchedAtTicks }

    let decodeDataItemsPhased schedule (record: BrowserRuntimeCacheRecord) (header: RuntimeCacheEntry) continuation =
        let values = ResizeArray<string * SduiValue>()
        let seen = System.Collections.Generic.HashSet<string>()

        let rec decode index =
            if index >= record.DataItems.Length then
                let snapshot =
                    { header.Snapshot with
                        Data = values.ToArray() |> Map.ofArray }

                continuation (Result.Ok { header with Snapshot = snapshot })
            else
                schedule (fun () ->
                    try
                        let item = record.DataItems[index]

                        if isNull (box item)
                           || System.String.IsNullOrWhiteSpace item.DataRef
                           || System.String.IsNullOrWhiteSpace item.ValueJson
                           || not (seen.Add item.DataRef) then
                            continuation (Result.Error "cache-data-item-invalid")
                        else
                            let value: SduiValue = Json.Deserialize item.ValueJson
                            values.Add(item.DataRef, value)
                            decode (index + 1)
                    with _ ->
                        continuation (Result.Error "cache-data-item-decode-failed"))

        decode 0

    let write entry continuation =
        let complete = once continuation

        let nowTicks = string System.DateTime.UtcNow.Ticks
        let key = keyFor entry nowTicks
        let record = recordForEntry key nowTicks entry

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
                    JS.Apply<obj> store "put" [| box record |] |> ignore
                with _ ->
                    complete (BrowserRuntimeCacheWriteResult.Unavailable "indexeddb-write-exception"))
            (fun reason -> complete (BrowserRuntimeCacheWriteResult.Unavailable reason))

    /// Validate and persist one accepted runtime projection without making browser storage authoritative.
    /// Rejected contains canonical contract validation failures; Unavailable is limited to IndexedDB failures.
    let writeAcceptedState cacheIdentity runtimeState continuation =
        match RuntimeCacheProjection.tryCreateEntry System.DateTimeOffset.UtcNow cacheIdentity runtimeState with
        | Error errors -> continuation (BrowserRuntimeCacheAcceptedStateWriteResult.Rejected errors)
        | Ok entry ->
            write
                entry
                (function
                    | BrowserRuntimeCacheWriteResult.Written -> continuation BrowserRuntimeCacheAcceptedStateWriteResult.Written
                    | BrowserRuntimeCacheWriteResult.Unavailable reason -> continuation (BrowserRuntimeCacheAcceptedStateWriteResult.Unavailable reason))

    /// Project and persist an accepted state in separate browser tasks. The supplied generation
    /// guard is checked before projection and again before IndexedDB encoding/write begins.
    let writeAcceptedStatePhased cacheIdentity runtimeState isCurrent continuation =
        let complete = once continuation
        let schedule work = JS.RequestAnimationFrame(fun _ -> work ()) |> ignore

        schedule (fun () ->
            if not (isCurrent ()) then
                complete BrowserRuntimeCachePhasedWriteOutcome.CancelledBeforeWrite
            else
                match RuntimeCacheProjection.tryCreateEntry System.DateTimeOffset.UtcNow cacheIdentity runtimeState with
                | Error errors ->
                    complete(
                        BrowserRuntimeCachePhasedWriteOutcome.Completed(
                            BrowserRuntimeCacheAcceptedStateWriteResult.Rejected errors))
                | Ok entry ->
                    schedule (fun () ->
                        if not (isCurrent ()) then
                            complete BrowserRuntimeCachePhasedWriteOutcome.CancelledBeforeWrite
                        else
                            write
                                entry
                                (fun result ->
                                    let acceptedResult =
                                        match result with
                                        | BrowserRuntimeCacheWriteResult.Written -> BrowserRuntimeCacheAcceptedStateWriteResult.Written
                                        | BrowserRuntimeCacheWriteResult.Unavailable reason ->
                                            BrowserRuntimeCacheAcceptedStateWriteResult.Unavailable reason

                                    complete (BrowserRuntimeCachePhasedWriteOutcome.Completed acceptedResult))))

    /// Rebase a validated cache entry onto the current authoritative document and pause remote commands until resync.
    let tryRehydrate cacheIdentity currentState entry =
        RuntimeCacheProjection.tryRehydrate DynamicRuntimeDefaults.limits cacheIdentity currentState entry

    let readMatching identityMatches cacheIdentity workspaceId (requestedCoverage: RuntimeCacheCoverage option) continuation =
        let complete = once continuation
        let lookupKey = lookupKeyFor cacheIdentity workspaceId
        let invalidKeys = ResizeArray<string>()
        let candidates = ResizeArray<BrowserRuntimeCacheRecord>()
        let schedule work = JS.RequestAnimationFrame(fun _ -> work ()) |> ignore

        let finish result =
            deleteKeys (invalidKeys.ToArray()) (fun () -> complete result)

        let rec inspectCandidate candidateIndex =
            if candidateIndex >= candidates.Count then
                finish BrowserRuntimeCacheReadResult.Miss
            else
                let record = candidates[candidateIndex]

                match BrowserRuntimeCodec.decodeCacheEntry record.EntryHeaderJson with
                | Error _ ->
                    invalidKeys.Add record.Key
                    inspectCandidate (candidateIndex + 1)
                | Ok header ->
                    match RuntimeCacheEntryValidation.validateHeader header with
                    | Error _ ->
                        invalidKeys.Add record.Key
                        inspectCandidate (candidateIndex + 1)
                    | Ok valid when identityMatches valid.CacheIdentity cacheIdentity && valid.WorkspaceId = workspaceId ->
                        let covers =
                            match requestedCoverage with
                            | None -> true
                            | Some requested ->
                                valid.Coverage.StartEventTimeUtc <= requested.StartEventTimeUtc
                                && valid.Coverage.EndEventTimeExclusiveUtc >= requested.EndEventTimeExclusiveUtc

                        if covers then
                            decodeDataItemsPhased
                                schedule
                                record
                                valid
                                (function
                                    | Ok entry -> finish (BrowserRuntimeCacheReadResult.Hit entry)
                                    | Error _ ->
                                        invalidKeys.Add record.Key
                                        inspectCandidate (candidateIndex + 1))
                        else
                            inspectCandidate (candidateIndex + 1)
                    | Ok _ ->
                        inspectCandidate (candidateIndex + 1)

        withStore
            "readonly"
            (fun _ store ->
                try
                    let index = JS.Apply<obj> store "index" [| box LookupIndexName |]
                    let keyRangeFactory = JS.Get<obj> "IDBKeyRange" JS.Window
                    let range = JS.Apply<obj> keyRangeFactory "only" [| box lookupKey |]
                    let request = JS.Apply<obj> index "openCursor" [| box range; box "prev" |]

                    let continueCursor cursor = JS.Apply<obj> cursor "continue" [||] |> ignore

                    JS.Set
                        request
                        "onsuccess"
                        (System.Action<obj>(fun event ->
                            let cursor = eventResult event

                            if isMissing cursor then
                                inspectCandidate 0
                            else
                                let key = JS.Get<obj> "primaryKey" cursor |> As<string>
                                let value = JS.Get<obj> "value" cursor

                                match decodeRecordValue value with
                                | None ->
                                    invalidKeys.Add key
                                | Some record ->
                                    candidates.Add record

                                continueCursor cursor))

                    JS.Set request "onerror" (System.Action<obj>(fun _ -> complete (BrowserRuntimeCacheReadResult.Unavailable "indexeddb-cursor-failed")))
                with _ ->
                    complete (BrowserRuntimeCacheReadResult.Unavailable "indexeddb-cursor-exception"))
            (fun reason -> complete (BrowserRuntimeCacheReadResult.Unavailable reason))

    let readLatest cacheIdentity workspaceId requestedCoverage continuation =
        readMatching (=) cacheIdentity workspaceId requestedCoverage continuation

    let readCovering cacheIdentity workspaceId requestedCoverage continuation =
        readMatching (=) cacheIdentity workspaceId (Some requestedCoverage) continuation

    let readAdjacent cacheIdentity workspaceId query continuation =
        let complete = once continuation
        let invalidKeys = ResizeArray<string>()
        let schedule work = JS.RequestAnimationFrame(fun _ -> work ()) |> ignore

        readAll
            (fun records ->
                let candidates =
                    records
                    |> Array.choose (fun record ->
                        match BrowserRuntimeCodec.decodeCacheEntry record.EntryHeaderJson with
                        | Error _ ->
                            invalidKeys.Add record.Key
                            None
                        | Ok header ->
                            match RuntimeCacheEntryValidation.validateHeader header with
                            | Ok valid when valid.CacheIdentity = cacheIdentity && valid.WorkspaceId = workspaceId -> Some(record, valid)
                            | Ok _ -> None
                            | Error _ ->
                                invalidKeys.Add record.Key
                                None)

                let finish result = deleteKeys (invalidKeys.ToArray()) (fun () -> complete result)

                let rec select remaining =
                    match remaining |> Array.map snd |> selectAdjacent query with
                    | BrowserRuntimeCacheAdjacentSelection.Miss -> finish BrowserRuntimeCacheAdjacentReadResult.Miss
                    | BrowserRuntimeCacheAdjacentSelection.KnownEmpty span ->
                        finish (BrowserRuntimeCacheAdjacentReadResult.KnownEmpty span)
                    | BrowserRuntimeCacheAdjacentSelection.Hit hit ->
                        match remaining |> Array.tryFind (fun (_, header) -> header = hit.SourceEntry) with
                        | None -> finish BrowserRuntimeCacheAdjacentReadResult.Miss
                        | Some(record, header) ->
                            decodeDataItemsPhased
                                schedule
                                record
                                header
                                (function
                                    | Ok entry ->
                                        match tryRebaseAdjacentEntry query entry with
                                        | Some rebased -> finish (BrowserRuntimeCacheAdjacentReadResult.Hit rebased)
                                        | None ->
                                            remaining
                                            |> Array.filter (fun (candidate, _) -> candidate.Key <> record.Key)
                                            |> select
                                    | Error _ ->
                                        invalidKeys.Add record.Key
                                        remaining
                                        |> Array.filter (fun (candidate, _) -> candidate.Key <> record.Key)
                                        |> select)

                select candidates)
            (fun reason -> complete (BrowserRuntimeCacheAdjacentReadResult.Unavailable reason))

    let rehydratePhased
        read
        cacheIdentity
        (currentState: unit -> RuntimeState)
        isCurrent
        continuation
        =
        let complete = once continuation
        let schedule work = JS.RequestAnimationFrame(fun _ -> work ()) |> ignore

        if not (isCurrent ()) then
            complete BrowserRuntimeCachePhasedRehydrateOutcome.Superseded
        else
            read (function
                | _ when not (isCurrent ()) ->
                    complete BrowserRuntimeCachePhasedRehydrateOutcome.Superseded
                | BrowserRuntimeCacheReadResult.Miss ->
                    complete BrowserRuntimeCachePhasedRehydrateOutcome.Miss
                | BrowserRuntimeCacheReadResult.Unavailable reason ->
                    complete (BrowserRuntimeCachePhasedRehydrateOutcome.Unavailable reason)
                | BrowserRuntimeCacheReadResult.Hit entry ->
                    schedule (fun () ->
                        if not (isCurrent ()) then
                            complete BrowserRuntimeCachePhasedRehydrateOutcome.Superseded
                        else
                            let current = currentState ()

                            match RuntimeCacheProjection.tryPrepareRehydrate DynamicRuntimeDefaults.limits cacheIdentity current entry with
                            | Error errors ->
                                complete (BrowserRuntimeCachePhasedRehydrateOutcome.Rejected errors)
                            | Ok(prepared, frame) ->
                                BrowserRuntimeFramePump.reduceFrameWith
                                    (fun _ work -> schedule work)
                                    prepared
                                    frame
                                    isCurrent
                                    0
                                    (function
                                        | BrowserRuntimeFramePumpOutcome.Applied candidate ->
                                            candidate
                                            |> RuntimeCacheProjection.completeRehydrateEntry current entry
                                            |> BrowserRuntimeCachePhasedRehydrateOutcome.Rehydrated
                                            |> complete
                                        | BrowserRuntimeFramePumpOutcome.Rejected failure ->
                                            [ RuntimeValidation.error failure.Code "cache.snapshot" failure.Message ]
                                            |> BrowserRuntimeCachePhasedRehydrateOutcome.Rejected
                                            |> complete
                                        | BrowserRuntimeFramePumpOutcome.Superseded ->
                                            complete BrowserRuntimeCachePhasedRehydrateOutcome.Superseded)))

    let rehydrateLatestPhased cacheIdentity workspaceId currentState isCurrent continuation =
        rehydratePhased
            (fun next -> readLatest cacheIdentity workspaceId None next)
            cacheIdentity
            currentState
            isCurrent
            continuation

    let rehydrateCoveringPhased cacheIdentity workspaceId requestedCoverage currentState isCurrent continuation =
        rehydratePhased
            (fun next -> readCovering cacheIdentity workspaceId requestedCoverage next)
            cacheIdentity
            currentState
            isCurrent
            continuation

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
