namespace PulseTrade.Comm.Spa.Dynamic.Contracts

open System

[<RequireQualifiedAccess>]
module RuntimeValidation =
    let error code field message =
        { Code = code
          Field = field
          Message = message }

    let identifier field value =
        if String.IsNullOrWhiteSpace value then
            [ error "required" field $"{field} is required." ]
        elif value.Length > 128 then
            [ error "too-long" field $"{field} exceeds 128 characters." ]
        else
            []

    let rec unsafeValue field value =
        match value with
        | SduiValue.Text text when text.TrimStart().StartsWith("javascript:", StringComparison.OrdinalIgnoreCase) ->
            [ error "script-forbidden" field "Script URLs are forbidden." ]
        | SduiValue.Text text when text.TrimStart().StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                                   || text.TrimStart().StartsWith("https://", StringComparison.OrdinalIgnoreCase) ->
            [ error "url-forbidden" field "Arbitrary URLs are forbidden in the shared runtime contract." ]
        | SduiValue.Array values -> values |> Array.toList |> List.collect (unsafeValue field)
        | SduiValue.Object values ->
            values
            |> Map.toList
            |> List.collect (fun (key, item) ->
                let keyErrors =
                    if Set.contains (key.Trim().ToLowerInvariant()) (Set.ofList [ "script"; "selector"; "url"; "href" ]) then
                        [ error "unsafe-key" field $"Unsafe option key `{key}` is forbidden." ]
                    else
                        []

                keyErrors @ unsafeValue ($"{field}.{key}") item)
        | _ -> []

    let rowErrors index (row: TaRowSpec) =
        [ yield! identifier $"rows[{index}].rowId" row.RowId
          yield! identifier $"rows[{index}].dataRef" row.DataRef

          if row.HeightWeight <= 0M || row.HeightWeight > 20M then
              yield error "invalid-height-weight" $"rows[{index}].heightWeight" "HeightWeight must be greater than 0 and at most 20."

          for KeyValue(key, value) in row.Options do
              yield! unsafeValue $"rows[{index}].options.{key}" value ]

    let documentErrors limits (document: TaWorkspaceDocument) =
        let allowedActions = Set.ofList [ "reset-view"; "reset-canvas"; "add-row"; "remove-row"; "change-query"; "poll-delta"; "request-full-snapshot" ]

        [ yield! identifier "document.workspaceId" document.WorkspaceId
          yield! identifier "document.rowsRef" document.RowsRef
          yield! identifier "document.statusRef" document.StatusRef

          if document.Rows.Length > limits.MaxRowsPerCanvas then
              yield error "limit-rows" "document.rows" $"Rows exceed hard limit {limits.MaxRowsPerCanvas}."

          yield! document.Rows |> Array.toList |> List.mapi rowErrors |> List.concat

          let duplicateIds =
              document.Rows
              |> Array.countBy _.RowId
              |> Array.filter (fun (_, count) -> count > 1)

          if duplicateIds.Length > 0 then
              yield error "duplicate-row-id" "document.rows" "Row ids must be unique."

          for action in document.AllowedActions do
              if not (Set.contains action allowedActions) then
                  yield error "unknown-action" "document.allowedActions" $"Unknown action `{action}`."

          for KeyValue(key, value) in document.DefaultView do
              yield! unsafeValue $"document.defaultView.{key}" value ]

    let patchErrors limits (patch: RuntimePatch) =
        [ if patch.Operations.Length > limits.MaxPatchOperations then
              yield error "limit-patch-operations" "patch.operations" $"Operations exceed hard limit {limits.MaxPatchOperations}."

          let itemCount =
              patch.Operations
              |> Array.sumBy (function PatchOperation.UpsertSeriesPoints(_, _, items) -> items.Length | _ -> 0)

          if itemCount > limits.MaxPatchItems then
              yield error "limit-patch-items" "patch.operations" $"Patch items exceed hard limit {limits.MaxPatchItems}."

          for operation in patch.Operations do
              match operation with
              | PatchOperation.ReplaceDataRef(dataRef, value) ->
                  yield! identifier "patch.dataRef" dataRef
                  yield! unsafeValue "patch.value" value
              | PatchOperation.UpsertSeriesPoints(dataRef, keyField, items) ->
                  yield! identifier "patch.dataRef" dataRef
                  yield! identifier "patch.keyField" keyField

                  if items |> Array.exists (Map.containsKey keyField >> not) then
                      yield error "missing-series-key" "patch.items" $"Every point must contain `{keyField}`."

                  for item in items do
                      yield! unsafeValue "patch.item" (SduiValue.Object item)
              | PatchOperation.RemoveSeriesBefore(dataRef, keyField, _) ->
                  yield! identifier "patch.dataRef" dataRef
                  yield! identifier "patch.keyField" keyField
              | PatchOperation.SetStatus(dataRef, value) ->
                  yield! identifier "patch.dataRef" dataRef
                  yield! unsafeValue "patch.status" (SduiValue.Object value)
              | PatchOperation.SetOptions(targetId, value) ->
                  yield! identifier "patch.targetId" targetId
                  yield! unsafeValue "patch.options" (SduiValue.Object value) ]

    let snapshotErrors limits (snapshot: RuntimeSnapshot) =
        [ for KeyValue(dataRef, value) in snapshot.Data do
              yield! identifier "snapshot.dataRef" dataRef
              yield! unsafeValue $"snapshot.{dataRef}" value

              match value with
              | SduiValue.Array items when items.Length > limits.MaxInitialBarsPerSeries ->
                  yield error "limit-initial-bars" $"snapshot.{dataRef}" $"Series exceeds hard limit {limits.MaxInitialBarsPerSeries}."
              | _ -> () ]

    let kindMatchesPayload kind payload =
        match kind, payload with
        | RuntimeFrameKind.Document, RuntimePayload.Document _
        | RuntimeFrameKind.Snapshot, RuntimePayload.Snapshot _
        | RuntimeFrameKind.Patch, RuntimePayload.Patch _
        | RuntimeFrameKind.Error, RuntimePayload.Error _
        | RuntimeFrameKind.Heartbeat, RuntimePayload.Heartbeat _ -> true
        | _ -> false

    let frameErrors limits (frame: RuntimeFrame) =
        [ if frame.Protocol <> DynamicRuntimeDefaults.protocol then
              yield error "unknown-protocol" "protocol" $"Expected `{DynamicRuntimeDefaults.protocol}`."

          let (DocumentId documentId) = frame.DocumentId
          let (CanvasInstanceId canvasId) = frame.CanvasInstanceId
          yield! identifier "documentId" documentId
          yield! identifier "canvasInstanceId" canvasId

          if frame.DocumentRevision < 0L || frame.DataRevision < 0L || frame.TransportSequence < 1L then
              yield error "invalid-revision" "revision" "Revisions must be non-negative and transport sequence must start at 1."

          if not (kindMatchesPayload frame.Kind frame.Payload) then
              yield error "payload-kind-mismatch" "payload" "Frame kind and payload case do not match."

          match frame.Payload with
          | RuntimePayload.Document document -> yield! documentErrors limits document
          | RuntimePayload.Snapshot snapshot -> yield! snapshotErrors limits snapshot
          | RuntimePayload.Patch patch -> yield! patchErrors limits patch
          | RuntimePayload.Error value ->
              yield! identifier "error.reasonCode" value.ReasonCode
              if value.Message.Length > 512 then yield error "too-long" "error.message" "Error message exceeds 512 characters."
          | _ -> () ]

    let validateFrame limits frame =
        match frameErrors limits frame with
        | [] -> Ok frame
        | errors -> Error errors
