namespace PulseTrade.Comm.Spa.Dynamic.Interactive.Client

open PulseTrade.Comm.Spa.Dynamic.Contracts
open WebSharper
open WebSharper.JavaScript

[<JavaScript>]
type BrowserRuntimeFramePumpFailure =
    { Code: string
      Message: string
      FrameIndex: int }

[<JavaScript; RequireQualifiedAccess>]
type BrowserRuntimeFramePumpOutcome =
    | Applied of RuntimeState
    | Rejected of BrowserRuntimeFramePumpFailure
    | Superseded

[<JavaScript; RequireQualifiedAccess>]
type BrowserRuntimeFramePumpStage =
    | PrepareFrame of frameIndex: int
    | ParseTransportPacket of packetIndex: int
    | DecodeSnapshotValue of dataRef: string
    | DecodeArrayBatch of dataRef: string
    | ReduceFrame of frameIndex: int
    | BuildAxisAuthority of dataRef: string
    | ValidateTemporalItem of dataRef: string
    | ApplyOverlays

[<JavaScript>]
type BrowserRuntimeSnapshotBatchMetadata =
    { Generation: int
      BatchId: string
      ExpectedItemCount: int
      NextItemIndex: int
      DataRefs: Set<string> }

[<JavaScript; RequireQualifiedAccess>]
module BrowserRuntimeSnapshotBatch =
    let create generation batchId itemCount =
        if System.String.IsNullOrWhiteSpace batchId || batchId.Length > 512 then
            Result.Error(
                "runtime-snapshot-chunk-start-invalid",
                "Snapshot start batch id is invalid.")
        elif itemCount < 0 || itemCount > RuntimeSnapshotTransportDefaults.MaximumItems then
            Result.Error(
                "runtime-snapshot-chunk-start-invalid",
                "Snapshot start item count is invalid.")
        else
            Result.Ok
                { Generation = generation
                  BatchId = batchId
                  ExpectedItemCount = itemCount
                  NextItemIndex = 0
                  DataRefs = Set.empty }

    let validateItem generation batchId itemIndex dataRef batch =
        if generation <> batch.Generation || batchId <> batch.BatchId then
            Result.Error(
                "runtime-snapshot-chunk-item-invalid",
                "Snapshot item belongs to another generation or batch.")
        elif itemIndex <> batch.NextItemIndex || itemIndex >= batch.ExpectedItemCount then
            Result.Error(
                "runtime-snapshot-chunk-item-invalid",
                "Snapshot item is missing or out of order.")
        elif System.String.IsNullOrWhiteSpace dataRef || Set.contains dataRef batch.DataRefs then
            Result.Error(
                "runtime-snapshot-chunk-item-invalid",
                "Snapshot item dataRef is invalid or duplicate.")
        else
            Result.Ok()

    let acceptItem dataRef batch =
        { batch with
            NextItemIndex = batch.NextItemIndex + 1
            DataRefs = Set.add dataRef batch.DataRefs }

    let validateCommit generation batchId itemCount batch =
        if generation <> batch.Generation || batchId <> batch.BatchId then
            Result.Error(
                "runtime-snapshot-chunk-commit-invalid",
                "Snapshot commit belongs to another generation or batch.")
        elif itemCount <> batch.ExpectedItemCount || batch.NextItemIndex <> itemCount then
            Result.Error(
                "runtime-snapshot-chunk-commit-invalid",
                "Snapshot commit does not match a complete active batch.")
        else
            Result.Ok()

[<JavaScript; RequireQualifiedAccess>]
type BrowserRuntimeFrameDecodePlan =
    | Complete of RuntimeFrame
    | Snapshot of header: RuntimeFrame * dataItems: (string * obj) array

/// Reduces encoded RuntimeFrames into one candidate without publishing partial state.
/// Decode and reduce are intentionally scheduled as separate browser tasks so a large
/// accepted snapshot cannot combine both costs into one main-thread long task.
[<JavaScript; RequireQualifiedAccess>]
module BrowserRuntimeFramePump =
    [<Literal>]
    let PointDecodeBatchSize = 256

    let isMissing value =
        isNull value || JS.TypeOf value = JS.Kind.Undefined

    let validateDecodedValue dataRef value =
        RuntimeValidation.snapshotErrors
            DynamicRuntimeDefaults.limits
            { Data = Map.ofList [ dataRef, value ]
              Freshness = TaFreshness.Live }

    let rejectValidation onRejected (errors: DynamicValidationError list) =
        match errors with
        | error :: _ -> onRejected error.Code error.Message
        | [] -> ()

    let decodeArrayItemsWith
        (schedule: BrowserRuntimeFramePumpStage -> (unit -> unit) -> unit)
        dataRef
        (rawItems: obj array)
        (isCurrent: unit -> bool)
        onDecoded
        onRejected
        onSuperseded
        =
        let decoded = ResizeArray<SduiValue>()

        let rec decodeBatch offset =
            if not (isCurrent ()) then
                onSuperseded ()
            elif offset >= rawItems.Length then
                onDecoded (decoded.ToArray())
            else
                schedule (BrowserRuntimeFramePumpStage.DecodeArrayBatch dataRef) (fun () ->
                    if not (isCurrent ()) then
                        onSuperseded ()
                    else
                        try
                            let count = min PointDecodeBatchSize (rawItems.Length - offset)
                            let rawBatch = rawItems[offset .. offset + count - 1]
                            let rawArray = New [ "$" => box 4; "Item" => box rawBatch ]

                            match Json.Deserialize<SduiValue>(JSON.Stringify rawArray) with
                            | SduiValue.Array values ->
                                match validateDecodedValue dataRef (SduiValue.Array values) with
                                | [] ->
                                    decoded.AddRange values
                                    decodeBatch (offset + count)
                                | errors -> rejectValidation onRejected errors
                            | _ ->
                                onRejected
                                    "runtime-snapshot-data-decode-failed"
                                    "A phased SduiValue array batch decoded to an unexpected shape."
                        with error ->
                            onRejected "runtime-snapshot-data-decode-failed" error.Message)

        if rawItems.Length > DynamicRuntimeDefaults.limits.MaxInitialBarsPerSeries then
            onRejected
                "limit-initial-bars"
                $"Series exceeds hard limit {DynamicRuntimeDefaults.limits.MaxInitialBarsPerSeries}."
        else
            decodeBatch 0

    let decodeSnapshotValueWith
        (schedule: BrowserRuntimeFramePumpStage -> (unit -> unit) -> unit)
        dataRef
        rawValue
        (isCurrent: unit -> bool)
        onDecoded
        onRejected
        onSuperseded
        =
        try
            let tag = JS.Get<int> "$" rawValue

            if tag = 4 then
                let rawItems = JS.Get<obj array> "Item" rawValue

                if isNull rawItems then
                    onRejected "runtime-snapshot-data-decode-failed" "An SduiValue array has no item payload."
                else
                    decodeArrayItemsWith
                        schedule
                        dataRef
                        rawItems
                        isCurrent
                        (SduiValue.Array >> onDecoded)
                        onRejected
                        onSuperseded
            elif tag = 5 then
                let rawFields = JS.Get<obj> "Item" rawValue
                let rawPoints =
                    if isMissing rawFields then null
                    else JS.Get<obj> "points" rawFields

                if isMissing rawPoints || JS.Get<int> "$" rawPoints <> 4 then
                    let value = Json.Deserialize<SduiValue>(JSON.Stringify rawValue)
                    match validateDecodedValue dataRef value with
                    | [] -> onDecoded value
                    | errors -> rejectValidation onRejected errors
                else
                    let rawPointItems = JS.Get<obj array> "Item" rawPoints

                    if isNull rawPointItems then
                        onRejected "runtime-snapshot-data-decode-failed" "A temporal points array has no item payload."
                    else
                        JS.Set rawPoints "Item" [||]

                        match Json.Deserialize<SduiValue>(JSON.Stringify rawValue) with
                        | SduiValue.Object fields ->
                            match validateDecodedValue dataRef (SduiValue.Object fields) with
                            | [] ->
                                decodeArrayItemsWith
                                    schedule
                                    dataRef
                                    rawPointItems
                                    isCurrent
                                    (fun points ->
                                        fields
                                        |> Map.add "points" (SduiValue.Array points)
                                        |> SduiValue.Object
                                        |> onDecoded)
                                    onRejected
                                    onSuperseded
                            | errors -> rejectValidation onRejected errors
                        | _ ->
                            onRejected
                                "runtime-snapshot-data-decode-failed"
                                "A phased temporal value decoded to an unexpected shape."
            else
                let value = Json.Deserialize<SduiValue>(JSON.Stringify rawValue)
                match validateDecodedValue dataRef value with
                | [] -> onDecoded value
                | errors -> rejectValidation onRejected errors
        with error ->
            onRejected "runtime-snapshot-data-decode-failed" error.Message

    let prepareParsedDecodePlan rawFrame =
        try
            let rawPayload = JS.Get<obj> "Payload" rawFrame

            if isNull rawPayload || JS.Get<int> "$" rawPayload <> 1 then
                BrowserRuntimeCodec.decode (JSON.Stringify rawFrame)
                |> Result.map BrowserRuntimeFrameDecodePlan.Complete
            else
                // RuntimePayload uses WebSharper's flattened union-record encoding, so
                // Snapshot fields live on the payload object beside the discriminator.
                let rawSnapshot = rawPayload
                let rawData = JS.Get<obj> "Data" rawSnapshot

                if isNull rawSnapshot || isNull rawData then
                    Result.Error "Runtime snapshot payload is malformed."
                else
                    let dataItems = JS.GetFields rawData
                    JS.Set rawSnapshot "Data" (New [||])

                    match BrowserRuntimeCodec.decode (JSON.Stringify rawFrame) with
                    | Result.Ok header -> Result.Ok(BrowserRuntimeFrameDecodePlan.Snapshot(header, dataItems))
                    | Result.Error issue -> Result.Error issue
        with error ->
            Result.Error error.Message

    let prepareDecodePlan (text: string) =
        if IsClient then
            try
                JSON.Parse text |> prepareParsedDecodePlan
            with error ->
                Result.Error error.Message
        else
            BrowserRuntimeCodec.decode text
            |> Result.map BrowserRuntimeFrameDecodePlan.Complete

    let decodeSnapshotItemsWith
        (schedule: BrowserRuntimeFramePumpStage -> (unit -> unit) -> unit)
        (header: RuntimeFrame)
        (items: (string * obj) array)
        (isCurrent: unit -> bool)
        onDecoded
        onRejected
        onSuperseded
        =
        let values = ResizeArray<string * SduiValue>()
        let seen = System.Collections.Generic.HashSet<string>()

        let rec decodeItem index =
            if not (isCurrent ()) then
                onSuperseded ()
            elif index >= items.Length then
                match header.Payload with
                | RuntimePayload.Snapshot snapshot ->
                    onDecoded
                        { header with
                            Payload = RuntimePayload.Snapshot { snapshot with Data = values.ToArray() |> Map.ofArray } }
                | _ -> onRejected "runtime-snapshot-header-invalid" "The phased snapshot header is invalid."
            else
                let dataRef, rawValue = items[index]

                if System.String.IsNullOrWhiteSpace dataRef || not (seen.Add dataRef) then
                    onRejected "runtime-snapshot-data-ref-invalid" "The snapshot contains an invalid or duplicate dataRef."
                else
                    schedule (BrowserRuntimeFramePumpStage.DecodeSnapshotValue dataRef) (fun () ->
                        decodeSnapshotValueWith
                            schedule
                            dataRef
                            rawValue
                            isCurrent
                            (fun value ->
                                values.Add(dataRef, value)
                                decodeItem (index + 1))
                            onRejected
                            onSuperseded)

        decodeItem 0

    let reduceFrameWith
        (schedule: BrowserRuntimeFramePumpStage -> (unit -> unit) -> unit)
        (current: RuntimeState)
        (frame: RuntimeFrame)
        (isCurrent: unit -> bool)
        frameIndex
        (continuation: BrowserRuntimeFramePumpOutcome -> unit)
        =
        let mutable completed = false

        let complete outcome =
            if not completed then
                completed <- true
                continuation outcome

        let reject code message =
            complete(
                BrowserRuntimeFramePumpOutcome.Rejected
                    { Code = code
                      Message = message
                      FrameIndex = frameIndex })

        let acceptEffect next = function
            | RuntimeEffect.RequestResync _ ->
                match next.LastError with
                | Some error -> reject error.ReasonCode error.Message
                | None -> reject "runtime-frame-resync-required" "The server frame sequence requires a full resync."
            | RuntimeEffect.RejectFrame(_, error) ->
                reject error.ReasonCode error.Message
            | _ -> complete (BrowserRuntimeFramePumpOutcome.Applied next)

        let rejectValidation (errors: DynamicValidationError list) =
            match errors with
            | error :: _ -> reject error.Code error.Message
            | [] -> reject "runtime-frame-validation-failed" "The runtime frame is invalid."

        if not (isCurrent ()) then
            complete BrowserRuntimeFramePumpOutcome.Superseded
        else
            match RuntimeReducer.preflight current frame with
            | RuntimeFramePreflight.Complete(next, effect) -> acceptEffect next effect
            | RuntimeFramePreflight.RequiresValidation ->
                match frame.Payload with
                | RuntimePayload.Snapshot snapshot ->
                    let envelope =
                        { frame with
                            Payload =
                                RuntimePayload.Snapshot
                                    { snapshot with
                                        Data = Map.empty } }

                    match RuntimeValidation.validateFrame DynamicRuntimeDefaults.limits envelope with
                    | Error errors -> rejectValidation errors
                    | Ok _ ->
                        let items = snapshot.Data |> Map.toArray
                        let axisRefs = RuntimeReducer.documentAxisRefs current
                        let axisRefItems = axisRefs |> Set.toArray

                        let rec buildAuthority axisIndex axes =
                            if not (isCurrent ()) then
                                complete BrowserRuntimeFramePumpOutcome.Superseded
                            elif axisIndex >= axisRefItems.Length then
                                validateTemporalItems
                                    { AxisRefs = axisRefs
                                      Axes = axes }
                                    0
                            else
                                schedule (BrowserRuntimeFramePumpStage.BuildAxisAuthority axisRefItems[axisIndex]) (fun () ->
                                    if not (isCurrent ()) then
                                        complete BrowserRuntimeFramePumpOutcome.Superseded
                                    else
                                        match RuntimeReducer.addSnapshotAxisAuthority snapshot.Data axisRefItems[axisIndex] axes with
                                        | Ok next -> buildAuthority (axisIndex + 1) next
                                        | Error(code, message) -> reject code message)

                        and validateTemporalItems authority itemIndex =
                            if not (isCurrent ()) then
                                complete BrowserRuntimeFramePumpOutcome.Superseded
                            elif itemIndex >= items.Length then
                                validateOverlays ()
                            else
                                let dataRef, value = items[itemIndex]

                                schedule (BrowserRuntimeFramePumpStage.ValidateTemporalItem dataRef) (fun () ->
                                    if not (isCurrent ()) then
                                        complete BrowserRuntimeFramePumpOutcome.Superseded
                                    else
                                        match RuntimeReducer.temporalSeriesError authority dataRef value with
                                        | None -> validateTemporalItems authority (itemIndex + 1)
                                        | Some(code, message) -> reject code message)

                        and validateOverlays () =
                            schedule BrowserRuntimeFramePumpStage.ApplyOverlays (fun () ->
                                if not (isCurrent ()) then
                                    complete BrowserRuntimeFramePumpOutcome.Superseded
                                else
                                    match RuntimeReducer.overlayCandidateError current snapshot.Data with
                                    | Some error ->
                                        let next, effect = RuntimeReducer.overlayFailure current frame error
                                        acceptEffect next effect
                                    | None ->
                                        let next, effect = RuntimeReducer.applyPrevalidatedSnapshot current frame snapshot
                                        acceptEffect next effect)

                        match RuntimeReducer.snapshotUnknownDataRef current snapshot.Data with
                        | Some dataRef -> reject "unknown-data-ref" $"Snapshot dataRef `{dataRef}` is not registered by the document."
                        | None -> buildAuthority 0 Map.empty
                | _ ->
                    let next, effect = RuntimeReducer.reduce current frame
                    acceptEffect next effect

    let reduceEncodedFramesWith
        (schedule: BrowserRuntimeFramePumpStage -> (unit -> unit) -> unit)
        (initialState: RuntimeState option)
        (frames: string array)
        (isCurrent: unit -> bool)
        (continuation: BrowserRuntimeFramePumpOutcome -> unit)
        =
        let mutable completed = false

        let complete outcome =
            if not completed then
                completed <- true
                continuation outcome

        let reject code message frameIndex =
            complete(
                BrowserRuntimeFramePumpOutcome.Rejected
                    { Code = code
                      Message = message
                      FrameIndex = frameIndex })

        let rec scheduleDecode frameIndex candidate =
            if not (isCurrent ()) then
                complete BrowserRuntimeFramePumpOutcome.Superseded
            elif frameIndex >= frames.Length then
                match candidate with
                | Some state -> complete (BrowserRuntimeFramePumpOutcome.Applied state)
                | None -> reject "runtime-frame-required" "The server returned no runtime frames." frameIndex
            else
                schedule (BrowserRuntimeFramePumpStage.PrepareFrame frameIndex) (fun () ->
                    if not (isCurrent ()) then
                        complete BrowserRuntimeFramePumpOutcome.Superseded
                    else
                        match prepareDecodePlan frames[frameIndex] with
                        | Error issue -> reject "runtime-frame-decode-failed" issue frameIndex
                        | Ok(BrowserRuntimeFrameDecodePlan.Complete frame) ->
                            scheduleReduce frameIndex candidate frame
                        | Ok(BrowserRuntimeFrameDecodePlan.Snapshot(header, dataItems)) ->
                            decodeSnapshotItemsWith
                                schedule
                                header
                                dataItems
                                isCurrent
                                (scheduleReduce frameIndex candidate)
                                (fun code message -> reject code message frameIndex)
                                (fun () -> complete BrowserRuntimeFramePumpOutcome.Superseded))

        and scheduleReduce frameIndex candidate frame =
            schedule (BrowserRuntimeFramePumpStage.ReduceFrame frameIndex) (fun () ->
                if not (isCurrent ()) then
                    complete BrowserRuntimeFramePumpOutcome.Superseded
                else
                    let current =
                        candidate
                        |> Option.defaultWith (fun () ->
                            RuntimeReducer.initial
                                { DocumentId = frame.DocumentId
                                  CanvasInstanceId = frame.CanvasInstanceId })

                    reduceFrameWith
                        schedule
                        current
                        frame
                        isCurrent
                        frameIndex
                        (function
                            | BrowserRuntimeFramePumpOutcome.Applied next ->
                                scheduleDecode (frameIndex + 1) (Some next)
                            | BrowserRuntimeFramePumpOutcome.Rejected failure ->
                                complete (BrowserRuntimeFramePumpOutcome.Rejected failure)
                            | BrowserRuntimeFramePumpOutcome.Superseded ->
                                complete BrowserRuntimeFramePumpOutcome.Superseded))

        scheduleDecode 0 initialState

    let reduceParsedFrameWith
        (schedule: BrowserRuntimeFramePumpStage -> (unit -> unit) -> unit)
        (initialState: RuntimeState option)
        rawFrame
        (isCurrent: unit -> bool)
        (continuation: BrowserRuntimeFramePumpOutcome -> unit)
        =
        let reduce frame =
            schedule (BrowserRuntimeFramePumpStage.ReduceFrame 0) (fun () ->
                if not (isCurrent ()) then
                    continuation BrowserRuntimeFramePumpOutcome.Superseded
                else
                    let current =
                        initialState
                        |> Option.defaultWith (fun () ->
                            RuntimeReducer.initial
                                { DocumentId = frame.DocumentId
                                  CanvasInstanceId = frame.CanvasInstanceId })

                    reduceFrameWith schedule current frame isCurrent 0 continuation)

        if not (isCurrent ()) then
            continuation BrowserRuntimeFramePumpOutcome.Superseded
        else
            match prepareParsedDecodePlan rawFrame with
            | Error issue ->
                continuation(
                    BrowserRuntimeFramePumpOutcome.Rejected
                        { Code = "runtime-frame-decode-failed"
                          Message = issue
                          FrameIndex = 0 })
            | Ok(BrowserRuntimeFrameDecodePlan.Complete frame) -> reduce frame
            | Ok(BrowserRuntimeFrameDecodePlan.Snapshot(header, dataItems)) ->
                decodeSnapshotItemsWith
                    schedule
                    header
                    dataItems
                    isCurrent
                    reduce
                    (fun code message ->
                        continuation(
                            BrowserRuntimeFramePumpOutcome.Rejected
                                { Code = code
                                  Message = message
                                  FrameIndex = 0 }))
                    (fun () -> continuation BrowserRuntimeFramePumpOutcome.Superseded)

    let reduceIsolatedEncodedFrames frames isCurrent continuation =
        reduceEncodedFramesWith
            (fun _ work -> JS.RequestAnimationFrame(fun _ -> work ()) |> ignore)
            None
            frames
            isCurrent
            continuation

    let reduceFromEncodedFrames initialState frames isCurrent continuation =
        reduceEncodedFramesWith
            (fun _ work -> JS.RequestAnimationFrame(fun _ -> work ()) |> ignore)
            (Some initialState)
            frames
            isCurrent
            continuation

    let reduceFromEncodedFramesObserved initialState frames isCurrent onStageCompleted continuation =
        let schedule stage work =
            JS.RequestAnimationFrame(fun _ ->
                let startedAt = System.DateTime.UtcNow.Ticks
                work ()
                let elapsedMilliseconds =
                    float (System.DateTime.UtcNow.Ticks - startedAt)
                    / float System.TimeSpan.TicksPerMillisecond

                onStageCompleted stage elapsedMilliseconds)
            |> ignore

        reduceEncodedFramesWith schedule (Some initialState) frames isCurrent continuation

    let reduceChunkedSnapshotPacketsWith
        (schedule: BrowserRuntimeFramePumpStage -> (unit -> unit) -> unit)
        (initialState: RuntimeState)
        (packets: string array)
        (isCurrent: unit -> bool)
        (continuation: BrowserRuntimeFramePumpOutcome -> unit)
        =
        let mutable completed = false
        let mutable metadata: BrowserRuntimeSnapshotBatchMetadata option = None
        let mutable header: RuntimeFrame option = None
        let values = ResizeArray<string * SduiValue>()

        let complete outcome =
            if not completed then
                completed <- true
                continuation outcome

        let reject packetIndex code message =
            complete(
                BrowserRuntimeFramePumpOutcome.Rejected
                    { Code = code
                      Message = message
                      FrameIndex = packetIndex })

        let tryStringField name raw =
            let field = JS.Get<obj> name raw
            if isMissing field then None
            else
                let text = As<string> field
                if System.String.IsNullOrWhiteSpace text then None else Some text

        let tryIntField name raw =
            let field = JS.Get<obj> name raw
            if isMissing field then None else Some(As<int> field)

        let rec processPacket packetIndex =
            if not (isCurrent ()) then
                complete BrowserRuntimeFramePumpOutcome.Superseded
            elif packetIndex >= packets.Length then
                reject packetIndex "runtime-snapshot-chunk-commit-required" "Snapshot packet sequence ended before commit."
            else
                schedule (BrowserRuntimeFramePumpStage.ParseTransportPacket packetIndex) (fun () ->
                    if not (isCurrent ()) then
                        complete BrowserRuntimeFramePumpOutcome.Superseded
                    else
                        try
                            let raw = JSON.Parse packets[packetIndex]

                            match tryStringField "Schema" raw, tryStringField "Kind" raw with
                            | Some schema, Some kind when schema = RuntimeSnapshotTransportDefaults.Schema ->
                                if kind = RuntimeSnapshotTransportDefaults.StartKind then
                                    match tryStringField "BatchId" raw, tryIntField "ItemCount" raw with
                                    | Some batchId, Some itemCount ->
                                        match BrowserRuntimeSnapshotBatch.create 1 batchId itemCount with
                                        | Error(code, message) -> reject packetIndex code message
                                        | Ok batch ->
                                            let rawHeader = JS.Get<obj> "Header" raw
                                            if isMissing rawHeader then
                                                reject packetIndex "runtime-snapshot-chunk-header-required" "Snapshot start requires a header."
                                            else
                                                match BrowserRuntimeCodec.decode (JSON.Stringify rawHeader) with
                                                | Ok frame ->
                                                    match frame.Kind, frame.Payload with
                                                    | RuntimeFrameKind.Snapshot, RuntimePayload.Snapshot snapshot when snapshot.Data.IsEmpty ->
                                                        metadata <- Some batch
                                                        header <- Some frame
                                                        processPacket (packetIndex + 1)
                                                    | _ ->
                                                        reject packetIndex "runtime-snapshot-chunk-header-invalid" "Snapshot start header is invalid."
                                                | Error message -> reject packetIndex "runtime-snapshot-chunk-header-invalid" message
                                    | _ -> reject packetIndex "runtime-snapshot-chunk-start-invalid" "Snapshot start metadata is invalid."
                                elif kind = RuntimeSnapshotTransportDefaults.ItemKind then
                                    match tryStringField "BatchId" raw, tryIntField "ItemIndex" raw, tryStringField "DataRef" raw, metadata with
                                    | Some batchId, Some itemIndex, Some dataRef, Some batch ->
                                        match BrowserRuntimeSnapshotBatch.validateItem 1 batchId itemIndex dataRef batch with
                                        | Error(code, message) -> reject packetIndex code message
                                        | Ok _ ->
                                            let rawValue = JS.Get<obj> "Value" raw
                                            if isMissing rawValue then
                                                reject packetIndex "runtime-snapshot-chunk-value-required" "Snapshot item requires a value."
                                            else
                                                decodeSnapshotValueWith
                                                    schedule
                                                    dataRef
                                                    rawValue
                                                    isCurrent
                                                    (fun decoded ->
                                                        values.Add(dataRef, decoded)
                                                        metadata <- Some(BrowserRuntimeSnapshotBatch.acceptItem dataRef batch)
                                                        processPacket (packetIndex + 1))
                                                    (reject packetIndex)
                                                    (fun () -> complete BrowserRuntimeFramePumpOutcome.Superseded)
                                    | _ -> reject packetIndex "runtime-snapshot-chunk-item-invalid" "Snapshot item metadata is invalid."
                                elif kind = RuntimeSnapshotTransportDefaults.CommitKind then
                                    match tryStringField "BatchId" raw, tryIntField "ItemCount" raw, metadata, header with
                                    | Some batchId, Some itemCount, Some batch, Some frame ->
                                        match BrowserRuntimeSnapshotBatch.validateCommit 1 batchId itemCount batch with
                                        | Error(code, message) -> reject packetIndex code message
                                        | Ok _ when packetIndex <> packets.Length - 1 ->
                                            reject packetIndex "runtime-snapshot-chunk-trailing-packet" "Snapshot commit must be the final packet."
                                        | Ok _ ->
                                            let assembled =
                                                match frame.Payload with
                                                | RuntimePayload.Snapshot snapshot ->
                                                    { frame with
                                                        Payload =
                                                            RuntimePayload.Snapshot
                                                                { snapshot with
                                                                    Data = values.ToArray() |> Map.ofArray } }
                                                | _ -> frame

                                            reduceFrameWith schedule initialState assembled isCurrent packetIndex complete
                                    | _ -> reject packetIndex "runtime-snapshot-chunk-commit-invalid" "Snapshot commit metadata is invalid."
                                else
                                    reject packetIndex "runtime-snapshot-chunk-kind-invalid" "Snapshot chunk kind is unsupported."
                            | _ ->
                                reject packetIndex "runtime-snapshot-chunk-schema-invalid" "Snapshot chunk schema is unsupported."
                        with error ->
                            reject packetIndex "runtime-snapshot-chunk-decode-failed" error.Message)

        if packets.Length = 0 then
            reject 0 "runtime-snapshot-chunk-start-required" "Snapshot packet sequence is empty."
        else
            processPacket 0

    let reduceChunkedSnapshotPacketsObserved initialState packets isCurrent onStageCompleted continuation =
        let schedule stage work =
            JS.RequestAnimationFrame(fun _ ->
                let startedAt = System.DateTime.UtcNow.Ticks
                work ()
                let elapsedMilliseconds =
                    float (System.DateTime.UtcNow.Ticks - startedAt)
                    / float System.TimeSpan.TicksPerMillisecond

                onStageCompleted stage elapsedMilliseconds)
            |> ignore

        reduceChunkedSnapshotPacketsWith schedule initialState packets isCurrent continuation
