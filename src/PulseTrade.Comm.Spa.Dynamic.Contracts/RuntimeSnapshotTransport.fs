namespace PulseTrade.Comm.Spa.Dynamic.Contracts

open WebSharper

[<JavaScript>]
type RuntimeSnapshotTransportStart =
    { Schema: string
      Kind: string
      BatchId: string
      ItemCount: int
      Header: RuntimeFrame }

[<JavaScript>]
type RuntimeSnapshotTransportItem =
    { Schema: string
      Kind: string
      BatchId: string
      ItemIndex: int
      DataRef: string
      Value: SduiValue }

[<JavaScript>]
type RuntimeSnapshotTransportCommit =
    { Schema: string
      Kind: string
      BatchId: string
      ItemCount: int }

[<JavaScript>]
type RuntimeSnapshotTransportEnvelope =
    { Schema: string
      Kind: string }

[<JavaScript; RequireQualifiedAccess>]
type RuntimeSnapshotTransportPacket =
    | Start of RuntimeSnapshotTransportStart
    | Item of RuntimeSnapshotTransportItem
    | Commit of RuntimeSnapshotTransportCommit

[<JavaScript>]
type RuntimeSnapshotTransportAssemblyError =
    { Code: string
      Message: string
      PacketIndex: int }

[<JavaScript>]
type RuntimeSnapshotTransportAssemblyState =
    { Generation: int
      PacketIndex: int
      BatchId: string option
      ExpectedItemCount: int
      NextItemIndex: int
      DataRefs: Set<string>
      Header: RuntimeFrame option
      Values: Map<string, SduiValue>
      CompletedFrame: RuntimeFrame option }

[<JavaScript; RequireQualifiedAccess>]
module RuntimeSnapshotTransportDefaults =
    [<Literal>]
    let Schema = "ptcs-dynamic-snapshot-chunk.v1"

    [<Literal>]
    let StartKind = "start"

    [<Literal>]
    let ItemKind = "item"

    [<Literal>]
    let CommitKind = "commit"

    [<Literal>]
    let MaximumItems = 256

[<JavaScript; RequireQualifiedAccess>]
module RuntimeSnapshotTransportCodec =
    let batchIdFor (frame: RuntimeFrame) =
        let (DocumentId documentId) = frame.DocumentId
        let (CanvasInstanceId canvasInstanceId) = frame.CanvasInstanceId

        System.String.Concat(
            documentId,
            ":",
            canvasInstanceId,
            ":",
            string frame.TransportSequence,
            ":",
            string frame.DataRevision)

    let encodeFrame (frame: RuntimeFrame) =
        match frame.Payload with
        | RuntimePayload.Snapshot snapshot ->
            let items = snapshot.Data |> Map.toArray

            if items.Length > RuntimeSnapshotTransportDefaults.MaximumItems then
                Result.Error(
                    $"Snapshot contains {items.Length} data refs; maximum is {RuntimeSnapshotTransportDefaults.MaximumItems}.")
            else
                let batchId = batchIdFor frame
                let header =
                    { frame with
                        Payload =
                            RuntimePayload.Snapshot
                                { snapshot with
                                    Data = Map.empty } }

                let start =
                    Json.Serialize
                        { Schema = RuntimeSnapshotTransportDefaults.Schema
                          Kind = RuntimeSnapshotTransportDefaults.StartKind
                          BatchId = batchId
                          ItemCount = items.Length
                          Header = header }

                let encodedItems =
                    items
                    |> Array.mapi (fun itemIndex (dataRef, value) ->
                        Json.Serialize
                            { Schema = RuntimeSnapshotTransportDefaults.Schema
                              Kind = RuntimeSnapshotTransportDefaults.ItemKind
                              BatchId = batchId
                              ItemIndex = itemIndex
                              DataRef = dataRef
                              Value = value })

                let commit =
                    Json.Serialize
                        { Schema = RuntimeSnapshotTransportDefaults.Schema
                          Kind = RuntimeSnapshotTransportDefaults.CommitKind
                          BatchId = batchId
                          ItemCount = items.Length }

                Result.Ok(Array.concat [ [| start |]; encodedItems; [| commit |] ])
        | _ -> Result.Ok [| BrowserRuntimeCodec.encode frame |]

    let encodeFrames (frames: RuntimeFrame array) =
        if isNull (box frames) then
            Result.Error "Runtime frame collection is required."
        else
            ((Result.Ok [||]), frames)
            ||> Array.fold (fun accumulated frame ->
                match accumulated, encodeFrame frame with
                | Result.Ok packets, Result.Ok encoded -> Result.Ok(Array.append packets encoded)
                | Result.Error error, _ -> Result.Error error
                | _, Result.Error error -> Result.Error error)

    let decodePacket packetIndex (text: string) =
        let error code message =
            Result.Error
                { Code = code
                  Message = message
                  PacketIndex = packetIndex }

        if System.String.IsNullOrWhiteSpace text then
            error "runtime-snapshot-chunk-decode-failed" "Snapshot packet is required."
        else
            try
                let envelope = Json.Deserialize<RuntimeSnapshotTransportEnvelope> text

                if isNull (box envelope) || envelope.Schema <> RuntimeSnapshotTransportDefaults.Schema then
                    error "runtime-snapshot-chunk-schema-invalid" "Snapshot chunk schema is unsupported."
                elif envelope.Kind = RuntimeSnapshotTransportDefaults.StartKind then
                    Json.Deserialize<RuntimeSnapshotTransportStart> text
                    |> RuntimeSnapshotTransportPacket.Start
                    |> Result.Ok
                elif envelope.Kind = RuntimeSnapshotTransportDefaults.ItemKind then
                    Json.Deserialize<RuntimeSnapshotTransportItem> text
                    |> RuntimeSnapshotTransportPacket.Item
                    |> Result.Ok
                elif envelope.Kind = RuntimeSnapshotTransportDefaults.CommitKind then
                    Json.Deserialize<RuntimeSnapshotTransportCommit> text
                    |> RuntimeSnapshotTransportPacket.Commit
                    |> Result.Ok
                else
                    error "runtime-snapshot-chunk-kind-invalid" "Snapshot chunk kind is unsupported."
            with decodeError ->
                error "runtime-snapshot-chunk-decode-failed" decodeError.Message

[<JavaScript; RequireQualifiedAccess>]
module RuntimeSnapshotTransportAssembler =
    let createAt generation packetIndex =
        { Generation = generation
          PacketIndex = packetIndex
          BatchId = None
          ExpectedItemCount = 0
          NextItemIndex = 0
          DataRefs = Set.empty
          Header = None
          Values = Map.empty
          CompletedFrame = None }

    let create generation = createAt generation 0

    let error (state: RuntimeSnapshotTransportAssemblyState) code message =
        Result.Error
            { Code = code
              Message = message
              PacketIndex = state.PacketIndex }

    let validatePacketEnvelope expectedKind (schema: string) (kind: string) state =
        if schema <> RuntimeSnapshotTransportDefaults.Schema then
            error state "runtime-snapshot-chunk-schema-invalid" "Snapshot chunk schema is unsupported."
        elif kind <> expectedKind then
            error state "runtime-snapshot-chunk-kind-invalid" "Snapshot chunk kind is unsupported."
        else
            Result.Ok()

    let acceptStart generation (start: RuntimeSnapshotTransportStart) state =
        match validatePacketEnvelope RuntimeSnapshotTransportDefaults.StartKind start.Schema start.Kind state with
        | Result.Error issue -> Result.Error issue
        | Result.Ok _ when state.CompletedFrame.IsSome ->
            error state "runtime-snapshot-chunk-trailing-packet" "Snapshot commit must be the final packet."
        | Result.Ok _ when state.Header.IsSome ->
            error state "runtime-snapshot-chunk-interleaved" "A snapshot start interrupted an incomplete snapshot batch."
        | Result.Ok _ when generation <> state.Generation ->
            error state "runtime-snapshot-chunk-start-invalid" "Snapshot start belongs to another generation."
        | Result.Ok _ when System.String.IsNullOrWhiteSpace start.BatchId || start.BatchId.Length > 512 ->
            error state "runtime-snapshot-chunk-start-invalid" "Snapshot start batch id is invalid."
        | Result.Ok _
            when start.ItemCount < 0
                 || start.ItemCount > RuntimeSnapshotTransportDefaults.MaximumItems ->
            error state "runtime-snapshot-chunk-start-invalid" "Snapshot start item count is invalid."
        | Result.Ok _ when isNull (box start.Header) ->
            error state "runtime-snapshot-chunk-header-required" "Snapshot start requires a header."
        | Result.Ok _ ->
            match start.Header.Kind, start.Header.Payload with
            | RuntimeFrameKind.Snapshot, RuntimePayload.Snapshot snapshot when snapshot.Data.IsEmpty ->
                Result.Ok
                    { state with
                        PacketIndex = state.PacketIndex + 1
                        BatchId = Some start.BatchId
                        ExpectedItemCount = start.ItemCount
                        Header = Some start.Header }
            | _ ->
                error
                    state
                    "runtime-snapshot-chunk-header-invalid"
                    "Snapshot start header must contain an empty canonical Snapshot payload."

    let validateItemMetadata generation batchId itemIndex dataRef state =
        if state.CompletedFrame.IsSome then
            error state "runtime-snapshot-chunk-trailing-packet" "Snapshot commit must be the final packet."
        elif state.Header.IsNone || state.BatchId.IsNone then
            error state "runtime-snapshot-chunk-start-required" "Snapshot item requires an active batch."
        elif generation <> state.Generation || Some batchId <> state.BatchId then
            error state "runtime-snapshot-chunk-item-invalid" "Snapshot item belongs to another generation or batch."
        elif itemIndex <> state.NextItemIndex || itemIndex >= state.ExpectedItemCount then
            error state "runtime-snapshot-chunk-item-invalid" "Snapshot item is missing or out of order."
        elif System.String.IsNullOrWhiteSpace dataRef || Set.contains dataRef state.DataRefs then
            error state "runtime-snapshot-chunk-item-invalid" "Snapshot item dataRef is invalid or duplicate."
        else
            Result.Ok()

    let acceptItem generation (item: RuntimeSnapshotTransportItem) state =
        match validatePacketEnvelope RuntimeSnapshotTransportDefaults.ItemKind item.Schema item.Kind state with
        | Result.Error issue -> Result.Error issue
        | Result.Ok _ ->
            match validateItemMetadata generation item.BatchId item.ItemIndex item.DataRef state with
            | Result.Error issue -> Result.Error issue
            | Result.Ok _ when isNull (box item.Value) ->
                error state "runtime-snapshot-chunk-value-required" "Snapshot item requires a value."
            | Result.Ok _ ->
                match
                    RuntimeValidation.snapshotErrors
                        DynamicRuntimeDefaults.limits
                        { Data = Map.ofList [ item.DataRef, item.Value ]
                          Freshness = TaFreshness.Live }
                with
                | issue :: _ -> error state issue.Code issue.Message
                | [] ->
                    Result.Ok
                        { state with
                            PacketIndex = state.PacketIndex + 1
                            NextItemIndex = state.NextItemIndex + 1
                            DataRefs = Set.add item.DataRef state.DataRefs
                            Values = Map.add item.DataRef item.Value state.Values }

    let acceptCommit generation (commit: RuntimeSnapshotTransportCommit) state =
        match validatePacketEnvelope RuntimeSnapshotTransportDefaults.CommitKind commit.Schema commit.Kind state with
        | Result.Error issue -> Result.Error issue
        | Result.Ok _ when state.CompletedFrame.IsSome ->
            error state "runtime-snapshot-chunk-trailing-packet" "Snapshot commit must be the final packet."
        | Result.Ok _ when state.Header.IsNone || state.BatchId.IsNone ->
            error state "runtime-snapshot-chunk-commit-invalid" "Snapshot commit requires an active batch."
        | Result.Ok _ when generation <> state.Generation || Some commit.BatchId <> state.BatchId ->
            error state "runtime-snapshot-chunk-commit-invalid" "Snapshot commit belongs to another generation or batch."
        | Result.Ok _
            when commit.ItemCount <> state.ExpectedItemCount
                 || state.NextItemIndex <> commit.ItemCount ->
            error state "runtime-snapshot-chunk-commit-invalid" "Snapshot commit does not match a complete active batch."
        | Result.Ok _ ->
            match state.Header with
            | Some header ->
                match header.Payload with
                | RuntimePayload.Snapshot snapshot ->
                    let frame =
                        { header with
                            Payload = RuntimePayload.Snapshot { snapshot with Data = state.Values } }

                    Result.Ok
                        { state with
                            PacketIndex = state.PacketIndex + 1
                            CompletedFrame = Some frame }
                | _ ->
                    error state "runtime-snapshot-chunk-header-invalid" "Snapshot start header is invalid."
            | None ->
                error state "runtime-snapshot-chunk-header-required" "Snapshot start requires a header."

    let acceptPacket generation packet state =
        match packet with
        | RuntimeSnapshotTransportPacket.Start start -> acceptStart generation start state
        | RuntimeSnapshotTransportPacket.Item item -> acceptItem generation item state
        | RuntimeSnapshotTransportPacket.Commit commit -> acceptCommit generation commit state

    let acceptEncoded generation text state =
        RuntimeSnapshotTransportCodec.decodePacket state.PacketIndex text
        |> Result.bind (fun packet -> acceptPacket generation packet state)

    let finish (state: RuntimeSnapshotTransportAssemblyState) =
        match state.CompletedFrame, state.Header with
        | Some frame, _ ->
            match RuntimeValidation.validateFrame DynamicRuntimeDefaults.limits frame with
            | Result.Ok validated -> Result.Ok validated
            | Result.Error(issue :: _) ->
                error { state with PacketIndex = max 0 (state.PacketIndex - 1) } issue.Code issue.Message
            | Result.Error [] ->
                error
                    { state with PacketIndex = max 0 (state.PacketIndex - 1) }
                    "runtime-snapshot-chunk-commit-invalid"
                    "Snapshot validation failed."
        | None, Some _ ->
            error state "runtime-snapshot-chunk-commit-required" "Snapshot packet sequence ended before commit."
        | None, None ->
            error state "runtime-snapshot-chunk-start-required" "Snapshot packet sequence is empty."

    let reassemble generation (packets: string array) =
        if isNull (box packets) then
            error
                (create generation)
                "runtime-snapshot-chunk-start-required"
                "Snapshot packet sequence is required."
        else
            ((Result.Ok(create generation)), packets)
            ||> Array.fold (fun state packet ->
                state |> Result.bind (acceptEncoded generation packet))
            |> Result.bind finish

    let decodeLegacyFrame packetIndex text =
        match BrowserRuntimeCodec.decode text with
        | Result.Error message ->
            Result.Error
                { Code = "runtime-frame-decode-failed"
                  Message = message
                  PacketIndex = packetIndex }
        | Result.Ok frame when isNull (box frame) ->
            Result.Error
                { Code = "runtime-frame-decode-failed"
                  Message = "Runtime frame decoded to null."
                  PacketIndex = packetIndex }
        | Result.Ok frame ->
            match RuntimeValidation.validateFrame DynamicRuntimeDefaults.limits frame with
            | Result.Ok validated -> Result.Ok validated
            | Result.Error(issue :: _) ->
                Result.Error
                    { Code = issue.Code
                      Message = issue.Message
                      PacketIndex = packetIndex }
            | Result.Error [] ->
                Result.Error
                    { Code = "runtime-frame-invalid"
                      Message = "Runtime frame validation failed."
                      PacketIndex = packetIndex }

    let decodeFrames generation (wire: string array) =
        if isNull (box wire) then
            Result.Error
                { Code = "runtime-frame-stream-required"
                  Message = "Runtime frame stream is required."
                  PacketIndex = 0 }
        else
            let frames = ResizeArray<RuntimeFrame>()

            let rec decodeAt packetIndex activeBatch =
                if packetIndex >= wire.Length then
                    match activeBatch with
                    | None -> Result.Ok(frames.ToArray())
                    | Some state -> finish state |> Result.map (fun frame -> Array.append (frames.ToArray()) [| frame |])
                else
                    match activeBatch with
                    | Some state ->
                        match RuntimeSnapshotTransportCodec.decodePacket packetIndex wire[packetIndex] with
                        | Result.Ok packet ->
                            match acceptPacket generation packet state with
                            | Result.Error issue -> Result.Error issue
                            | Result.Ok next ->
                                match next.CompletedFrame with
                                | Some _ ->
                                    match finish next with
                                    | Result.Error issue -> Result.Error issue
                                    | Result.Ok frame ->
                                        frames.Add frame
                                        decodeAt (packetIndex + 1) None
                                | None -> decodeAt (packetIndex + 1) (Some next)
                        | Result.Error packetIssue ->
                            match decodeLegacyFrame packetIndex wire[packetIndex] with
                            | Result.Ok _ ->
                                Result.Error
                                    { Code = "runtime-snapshot-chunk-interleaved"
                                      Message = "A canonical runtime frame interrupted an incomplete snapshot batch."
                                      PacketIndex = packetIndex }
                            | Result.Error _ -> Result.Error packetIssue
                    | None ->
                        match RuntimeSnapshotTransportCodec.decodePacket packetIndex wire[packetIndex] with
                        | Result.Ok packet ->
                            let initial = createAt generation packetIndex
                            match acceptPacket generation packet initial with
                            | Result.Error issue -> Result.Error issue
                            | Result.Ok next ->
                                match next.CompletedFrame with
                                | Some _ ->
                                    match finish next with
                                    | Result.Error issue -> Result.Error issue
                                    | Result.Ok frame ->
                                        frames.Add frame
                                        decodeAt (packetIndex + 1) None
                                | None -> decodeAt (packetIndex + 1) (Some next)
                        | Result.Error packetIssue ->
                            match decodeLegacyFrame packetIndex wire[packetIndex] with
                            | Result.Ok frame ->
                                frames.Add frame
                                decodeAt (packetIndex + 1) None
                            | Result.Error _ -> Result.Error packetIssue

            decodeAt 0 None
