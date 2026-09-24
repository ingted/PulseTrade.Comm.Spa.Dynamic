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
