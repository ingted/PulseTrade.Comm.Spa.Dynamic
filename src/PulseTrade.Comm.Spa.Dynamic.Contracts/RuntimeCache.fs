namespace PulseTrade.Comm.Spa.Dynamic.Contracts

open System
open System.Text
open System.Text.Json

[<WebSharper.JavaScript; RequireQualifiedAccess>]
module RuntimeCacheEntryValidation =
    let identityErrors field (identity: RuntimeCacheIdentity) =
        if isNull (box identity) then
            [ RuntimeValidation.error "cache-identity-required" field "Runtime cache identity is required." ]
        else
            [ yield! RuntimeValidation.identifier (field + ".ownerFingerprint") identity.OwnerFingerprint

              if identity.SchemaRevision <= 0L then
                  yield
                      RuntimeValidation.error
                          "invalid-cache-schema-revision"
                          (field + ".schemaRevision")
                          "Runtime cache schema revision must be positive." ]

    let coverageErrors field (coverage: RuntimeCacheCoverage) =
        if isNull (box coverage) then
            [ RuntimeValidation.error "cache-coverage-required" field "Runtime cache coverage is required." ]
        else
            [ if coverage.StartEventTimeUtc.Offset <> TimeSpan.Zero then
                  yield RuntimeValidation.error "utc-required" (field + ".startEventTimeUtc") "Cache coverage start must use UTC."

              if coverage.EndEventTimeExclusiveUtc.Offset <> TimeSpan.Zero then
                  yield RuntimeValidation.error "utc-required" (field + ".endEventTimeExclusiveUtc") "Cache coverage end must use UTC."

              if coverage.EndEventTimeExclusiveUtc <= coverage.StartEventTimeUtc then
                  yield RuntimeValidation.error "invalid-cache-coverage" field "Cache coverage end must be later than its start." ]

    let validate limits (entry: RuntimeCacheEntry) =
        if isNull (box entry) then
            Error [ RuntimeValidation.error "cache-entry-required" "cache" "Runtime cache entry is required." ]
        else
            let errors =
                [ yield! identityErrors "cache.cacheIdentity" entry.CacheIdentity
                  yield! RuntimeValidation.identifier "cache.workspaceId" entry.WorkspaceId
                  yield! coverageErrors "cache.coverage" entry.Coverage

                  if entry.DocumentRevision < 0L then
                      yield RuntimeValidation.error "invalid-document-revision" "cache.documentRevision" "Document revision must be non-negative."

                  if entry.DataRevision < 0L then
                      yield RuntimeValidation.error "invalid-data-revision" "cache.dataRevision" "Data revision must be non-negative."

                  if entry.CapturedAtUtc.Offset <> TimeSpan.Zero then
                      yield RuntimeValidation.error "utc-required" "cache.capturedAtUtc" "Cache capture time must use UTC."

                  if isNull (box entry.Document) then
                      yield RuntimeValidation.error "cache-document-required" "cache.document" "Cache document is required."
                  else
                      yield! RuntimeValidation.documentErrors limits entry.Document

                      if entry.Document.WorkspaceId <> entry.WorkspaceId then
                          yield RuntimeValidation.error "cache-workspace-mismatch" "cache.workspaceId" "Cache workspace does not match its document."

                  if isNull (box entry.Snapshot) then
                      yield RuntimeValidation.error "cache-snapshot-required" "cache.snapshot" "Cache snapshot is required."
                  else
                      yield! RuntimeValidation.snapshotErrors limits entry.Snapshot ]

            match errors with
            | _ :: _ -> Error errors
            | [] ->
                let validationIdentity =
                    { DocumentId = DocumentId "cache-validation-document"
                      CanvasInstanceId = CanvasInstanceId "cache-validation-canvas" }

                let documentFrame =
                    { Protocol = DynamicRuntimeDefaults.protocol
                      Kind = RuntimeFrameKind.Document
                      DocumentId = validationIdentity.DocumentId
                      CanvasInstanceId = validationIdentity.CanvasInstanceId
                      DocumentRevision = entry.DocumentRevision
                      BaseDataRevision = None
                      DataRevision = 0L
                      TransportSequence = 1L
                      Payload = RuntimePayload.Document entry.Document }

                let afterDocument, documentEffect = RuntimeReducer.reduce (RuntimeReducer.initial validationIdentity) documentFrame

                match documentEffect with
                | RuntimeEffect.RequestResync _ ->
                    Error [ RuntimeValidation.error "cache-document-invalid" "cache.document" "Cache document cannot seed a valid runtime state." ]
                | _ ->
                    let snapshotFrame =
                        { documentFrame with
                            Kind = RuntimeFrameKind.Snapshot
                            DataRevision = entry.DataRevision
                            TransportSequence = 2L
                            Payload = RuntimePayload.Snapshot entry.Snapshot }

                    let _, snapshotEffect = RuntimeReducer.reduce afterDocument snapshotFrame

                    match snapshotEffect with
                    | RuntimeEffect.RequestResync _ ->
                        Error [ RuntimeValidation.error "cache-snapshot-invalid" "cache.snapshot" "Cache snapshot is incompatible with its document." ]
                    | _ -> Ok entry

[<RequireQualifiedAccess>]
module RuntimeCache =
    [<Literal>]
    let CurrentSchemaRevision = 1L

    [<Literal>]
    let MaximumEntries = 8

    let identityErrors field identity = RuntimeCacheEntryValidation.identityErrors field identity

    let coverageErrors field coverage = RuntimeCacheEntryValidation.coverageErrors field coverage

    let covers (requested: RuntimeCacheCoverage) (cached: RuntimeCacheCoverage) =
        cached.StartEventTimeUtc <= requested.StartEventTimeUtc
        && cached.EndEventTimeExclusiveUtc >= requested.EndEventTimeExclusiveUtc

    let tryBaseAxisCoverage (state: RuntimeState) =
        match state.Document with
        | None ->
            Error [ RuntimeValidation.error "cache-document-required" "cache.document" "A document is required before cache coverage can be derived." ]
        | Some document ->
            let axisRefs = if isNull document.TemporalAxisRefs then [||] else document.TemporalAxisRefs

            let decoded =
                axisRefs
                |> Array.map (fun axisRef ->
                    match Map.tryFind axisRef state.Data with
                    | None ->
                        Error
                            [ RuntimeValidation.error
                                  "cache-axis-missing"
                                  "cache.snapshot.data"
                                  $"Declared temporal axis `{axisRef}` is missing from the accepted runtime data." ]
                    | Some value -> TemporalAxisCodec.decode value)

            let errors =
                decoded
                |> Array.choose (function
                    | Error values -> Some values
                    | Ok _ -> None)
                |> Array.toList
                |> List.concat

            if not (List.isEmpty errors) then
                Error errors
            else
                let baseAxis =
                    decoded
                    |> Array.choose (function
                        | Ok axis when not (isNull axis.Points) && axis.Points.Length > 0 -> Some axis
                        | _ -> None)
                    |> Array.sortByDescending (fun axis -> axis.Points.Length)
                    |> Array.tryHead

                match baseAxis with
                | None ->
                    Error
                        [ RuntimeValidation.error
                              "cache-coverage-unavailable"
                              "cache.coverage"
                              "Accepted runtime data has no non-empty temporal axis from which cache coverage can be derived." ]
                | Some axis ->
                    Ok
                        { StartEventTimeUtc = axis.Points |> Array.minBy _.IntervalStartUtc |> _.IntervalStartUtc
                          EndEventTimeExclusiveUtc = axis.Points |> Array.maxBy _.IntervalEndUtc |> _.IntervalEndUtc }

    let stateIsCacheable state =
        state.Document.IsSome
        && state.LastError.IsNone
        && state.Poll <> RuntimePollState.PausedForResync
        && state.Poll <> RuntimePollState.Disposed

    let shouldPersistFrame (frame: RuntimeFrame) effect state =
        let accepted =
            match effect with
            | RuntimeEffect.RequestResync _ -> false
            | _ -> true

        accepted
        && stateIsCacheable state
        && (frame.Kind = RuntimeFrameKind.Snapshot || frame.Kind = RuntimeFrameKind.Patch)

    let tryCreateEntry (capturedAtUtc: DateTimeOffset) (cacheIdentity: RuntimeCacheIdentity) (state: RuntimeState) =
        let initialErrors =
            [ yield! identityErrors "cache.cacheIdentity" cacheIdentity

              if capturedAtUtc.Offset <> TimeSpan.Zero then
                  yield RuntimeValidation.error "utc-required" "cache.capturedAtUtc" "Cache capture time must use UTC."

              if state.DocumentRevision < 0L then
                  yield RuntimeValidation.error "invalid-document-revision" "cache.documentRevision" "Document revision must be non-negative."

              if state.DataRevision < 0L then
                  yield RuntimeValidation.error "invalid-data-revision" "cache.dataRevision" "Data revision must be non-negative."

              if not (stateIsCacheable state) then
                  yield
                      RuntimeValidation.error
                          "runtime-state-not-cacheable"
                          "cache.runtimeState"
                          "Only an accepted runtime state without a pending resync or error can be cached." ]

        match initialErrors, state.Document with
        | _ :: _, _ -> Error initialErrors
        | [], None ->
            Error [ RuntimeValidation.error "cache-document-required" "cache.document" "A cache entry requires an accepted document." ]
        | [], Some document ->
            match tryBaseAxisCoverage state with
            | Error errors -> Error errors
            | Ok coverage ->
                Ok
                    { CacheIdentity = cacheIdentity
                      WorkspaceId = document.WorkspaceId
                      Document = document
                      Snapshot =
                        { Data = state.Data
                          Freshness = TaFreshness.Stale(TimeSpan.Zero, "browser-cache-awaiting-authority") }
                      DocumentRevision = state.DocumentRevision
                      DataRevision = state.DataRevision
                      Coverage = coverage
                      CapturedAtUtc = capturedAtUtc }

    let validateEntry limits entry = RuntimeCacheEntryValidation.validate limits entry

    let tryRehydrate limits expectedCacheIdentity (current: RuntimeState) entry =
        validateEntry limits entry
        |> Result.bind (fun valid ->
            if valid.CacheIdentity <> expectedCacheIdentity then
                Error [ RuntimeValidation.error "cache-identity-mismatch" "cache.cacheIdentity" "Cache identity does not match the current owner fingerprint." ]
            else
                match current.Document with
                | None ->
                    Error [ RuntimeValidation.error "cache-document-required" "runtimeState.document" "The current authoritative document must be accepted before cache rehydration." ]
                | Some document when document.WorkspaceId <> valid.WorkspaceId ->
                    Error [ RuntimeValidation.error "cache-workspace-mismatch" "cache.workspaceId" "Cache workspace does not match the current document." ]
                | Some _ ->
                    let snapshotFrame =
                        { Protocol = DynamicRuntimeDefaults.protocol
                          Kind = RuntimeFrameKind.Snapshot
                          DocumentId = current.Identity.DocumentId
                          CanvasInstanceId = current.Identity.CanvasInstanceId
                          DocumentRevision = current.DocumentRevision
                          BaseDataRevision = None
                          DataRevision = valid.DataRevision
                          TransportSequence = current.LastTransportSequence + 1L
                          Payload = RuntimePayload.Snapshot valid.Snapshot }

                    let candidate, effect = RuntimeReducer.reduce current snapshotFrame

                    match effect with
                    | RuntimeEffect.RequestResync _ ->
                        Error [ RuntimeValidation.error "cache-rehydrate-invalid" "cache.snapshot" "Cache snapshot is incompatible with the current authoritative document." ]
                    | _ ->
                        Ok
                            { candidate with
                                DocumentRevision = current.DocumentRevision
                                LastTransportSequence = current.LastTransportSequence
                                Poll = RuntimePollState.PausedForResync
                                LastError = None })

[<RequireQualifiedAccess>]
module RuntimeCacheCodec =
    let encode entry = JsonSerializer.Serialize(entry, RuntimeCodec.options)

    let decode limits (text: string) =
        if isNull text then
            Error [ RuntimeValidation.error "cache-entry-required" "cache" "Runtime cache entry is required." ]
        elif Encoding.UTF8.GetByteCount text > limits.MaxFrameBytes then
            Error [ RuntimeValidation.error "limit-cache-entry-bytes" "cache" $"Cache entry exceeds hard limit {limits.MaxFrameBytes} bytes." ]
        else
            try
                let entry = JsonSerializer.Deserialize<RuntimeCacheEntry>(text, RuntimeCodec.options)
                RuntimeCache.validateEntry limits entry
            with :? JsonException as error ->
                Error [ RuntimeValidation.error "cache-entry-json-invalid" "cache" error.Message ]
