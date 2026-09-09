namespace PulseTrade.Comm.Spa.Dynamic.Contracts

open System

[<WebSharper.JavaScript; RequireQualifiedAccess>]
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
        | SduiValue.Text text when text.TrimStart().ToLower().StartsWith("javascript:") ->
            [ error "script-forbidden" field "Script URLs are forbidden." ]
        | SduiValue.Text text when text.TrimStart().ToLower().StartsWith("http://")
                                   || text.TrimStart().ToLower().StartsWith("https://") ->
            [ error "url-forbidden" field "Arbitrary URLs are forbidden in the shared runtime contract." ]
        | SduiValue.Array values -> values |> Array.toList |> List.collect (unsafeValue field)
        | SduiValue.Object values ->
            values
            |> Map.toList
            |> List.collect (fun (key, item) ->
                let keyErrors =
                    if Set.contains (key.Trim().ToLower()) (Set.ofList [ "script"; "selector"; "url"; "href" ]) then
                        [ error "unsafe-key" field $"Unsafe option key `{key}` is forbidden." ]
                    else
                        []

                keyErrors @ unsafeValue ($"{field}.{key}") item)
        | _ -> []

    let rowErrors index (row: TaRowSpec) =
        let traces = TaRowSpec.effectiveTraces row
        [ yield! identifier $"rows[{index}].rowId" row.RowId
          yield! identifier $"rows[{index}].dataRef" row.DataRef

          for traceIndex, trace in traces |> Array.indexed do
              yield! identifier $"rows[{index}].traces[{traceIndex}].traceId" trace.TraceId
              yield! identifier $"rows[{index}].traces[{traceIndex}].dataRef" trace.DataRef
              match trace.CandleDataRefs with
              | Some refs when trace.Kind <> TaTraceKind.Candlestick ->
                  yield error "candle-refs-kind-mismatch" $"rows[{index}].traces[{traceIndex}].candleDataRefs" "Candle component refs require a Candlestick trace."
              | Some refs ->
                  for field, value in
                      [ "openRef", refs.OpenRef
                        "highRef", refs.HighRef
                        "lowRef", refs.LowRef
                        "closeRef", refs.CloseRef
                        "volumeRef", refs.VolumeRef ] do
                      yield! identifier $"rows[{index}].traces[{traceIndex}].candleDataRefs.{field}" value
                  let values = [| refs.OpenRef; refs.HighRef; refs.LowRef; refs.CloseRef; refs.VolumeRef |]
                  if values |> Array.distinct |> Array.length <> values.Length then
                      yield error "duplicate-candle-data-ref" $"rows[{index}].traces[{traceIndex}].candleDataRefs" "Candle component refs must be distinct."
              | None -> ()
              if trace.Width <= 0.0 || trace.Width > 12.0 then
                  yield error "invalid-trace-width" $"rows[{index}].traces[{traceIndex}].width" "Trace width must be greater than 0 and at most 12."
              for KeyValue(key, value) in trace.Options do
                  yield! unsafeValue $"rows[{index}].traces[{traceIndex}].options.{key}" value

          let duplicateTraceIds = traces |> Array.countBy _.TraceId |> Array.exists (fun (_, count) -> count > 1)
          if duplicateTraceIds then
              yield error "duplicate-trace-id" $"rows[{index}].traces" "Trace ids must be unique within a row."

          if row.HeightWeight <= 0.0 || row.HeightWeight > 20.0 then
              yield error "invalid-height-weight" $"rows[{index}].heightWeight" "HeightWeight must be greater than 0 and at most 20."

          for KeyValue(key, value) in row.Options do
              yield! unsafeValue $"rows[{index}].options.{key}" value ]

    let rec editorKindErrors field depth kind =
        [ if depth > 8 then
              yield error "limit-schema-depth" field "Editor schema depth exceeds hard limit 8."
          match kind with
          | EditorValueKind.Integer(minimum, maximum) ->
              match minimum, maximum with
              | Some lower, Some upper when lower > upper -> yield error "invalid-range" field "Editor integer minimum exceeds maximum."
              | _ -> ()
          | EditorValueKind.Decimal(minimum, maximum) ->
              match minimum, maximum with
              | Some lower, Some upper when lower > upper -> yield error "invalid-range" field "Editor decimal minimum exceeds maximum."
              | _ -> ()
          | EditorValueKind.Choice choices ->
              let values = if isNull choices then [||] else choices
              if values.Length = 0 || values.Length > 128 then
                  yield error "invalid-choice-count" field "Editor choice count must be between 1 and 128."
              if values |> Array.countBy _.Key |> Array.exists (fun (_, count) -> count > 1) then
                  yield error "duplicate-choice-key" field "Editor choice keys must be unique."
              for index, choice in values |> Array.indexed do
                  yield! identifier $"{field}.choices[{index}].key" choice.Key
                  yield! identifier $"{field}.choices[{index}].label" choice.Label
                  yield! unsafeValue $"{field}.choices[{index}].value" choice.Value
          | EditorValueKind.Scale scaleKeys ->
              let values = if isNull scaleKeys then [||] else scaleKeys
              if values.Length = 0 || values.Length > 128 then
                  yield error "invalid-scale-count" field "Editor scale count must be between 1 and 128."
              if values |> Array.countBy id |> Array.exists (fun (_, count) -> count > 1) then
                  yield error "duplicate-scale-key" field "Editor scale keys must be unique."
              for index, scaleKey in values |> Array.indexed do
                  yield! identifier $"{field}.scaleKeys[{index}]" scaleKey
          | EditorValueKind.List(item, minimum, maximum) ->
              match minimum, maximum with
              | Some lower, Some upper when lower < 0 || upper < lower || upper > 128 ->
                  yield error "invalid-list-range" field "Editor list bounds must be ordered within 0..128."
              | Some lower, None when lower < 0 || lower > 128 ->
                  yield error "invalid-list-range" field "Editor list minimum must be within 0..128."
              | None, Some upper when upper < 0 || upper > 128 ->
                  yield error "invalid-list-range" field "Editor list maximum must be within 0..128."
              | _ -> ()
              yield! editorKindErrors (field + ".item") (depth + 1) item
          | EditorValueKind.Group fields ->
              let values = if isNull fields then [||] else fields
              if values |> Array.countBy _.Key |> Array.exists (fun (_, count) -> count > 1) then
                  yield error "duplicate-editor-field" field "Editor field keys must be unique."
              for index, child in values |> Array.indexed do
                  yield! editorFieldErrors $"{field}.fields[{index}]" (depth + 1) child
          | _ -> () ]

    and editorFieldErrors field depth (schema: EditorFieldSchema) =
        [ yield! identifier (field + ".key") schema.Key
          yield! identifier (field + ".label") schema.Label
          yield! editorKindErrors (field + ".kind") depth schema.Kind
          match schema.DefaultValue with
          | Some value -> yield! unsafeValue (field + ".defaultValue") value
          | None -> () ]

    let editorSchemaErrors index (schema: DynamicTemplateSchema) =
        let fields = if isNull schema.Fields then [||] else schema.Fields
        [ yield! identifier $"document.editorSchemas[{index}].templateKey" schema.TemplateKey
          yield! identifier $"document.editorSchemas[{index}].displayName" schema.DisplayName
          if schema.SchemaRevision < 0L then
              yield error "invalid-schema-revision" $"document.editorSchemas[{index}].schemaRevision" "Schema revision must be non-negative."
          if fields.Length > 128 then
              yield error "limit-editor-fields" $"document.editorSchemas[{index}].fields" "Editor field count exceeds hard limit 128."
          if fields |> Array.countBy _.Key |> Array.exists (fun (_, count) -> count > 1) then
              yield error "duplicate-editor-field" $"document.editorSchemas[{index}].fields" "Editor field keys must be unique."
          for fieldIndex, field in fields |> Array.indexed do
              yield! editorFieldErrors $"document.editorSchemas[{index}].fields[{fieldIndex}]" 1 field ]

    let documentErrors limits (document: TaWorkspaceDocument) =
        let allowedActions =
            Set.ofList
                [ "reset-view"
                  "reset-canvas"
                  "add-row"
                  "remove-row"
                  "change-query"
                  "shared-cursor-changed"
                  "visible-range-changed"
                  "poll-delta"
                  "request-full-snapshot" ]
        let editorSchemas = if isNull document.EditorSchemas then [||] else document.EditorSchemas
        let temporalAxisRefs = if isNull document.TemporalAxisRefs then [||] else document.TemporalAxisRefs

        [ yield! identifier "document.workspaceId" document.WorkspaceId
          yield! identifier "document.rowsRef" document.RowsRef
          yield! identifier "document.statusRef" document.StatusRef

          for index, axisRef in temporalAxisRefs |> Array.indexed do
              yield! identifier $"document.temporalAxisRefs[{index}]" axisRef

          if temporalAxisRefs |> Array.countBy id |> Array.exists (fun (_, count) -> count > 1) then
              yield error "duplicate-temporal-axis-ref" "document.temporalAxisRefs" "Temporal axis refs must be unique."

          let nonAxisRefs =
              [ yield document.RowsRef
                yield document.StatusRef
                for row in document.Rows do
                    yield! TaRowSpec.dataRefs row ]
              |> Set.ofList
          if temporalAxisRefs |> Array.exists (fun axisRef -> Set.contains axisRef nonAxisRefs) then
              yield error "temporal-axis-ref-collision" "document.temporalAxisRefs" "Temporal axis refs must not collide with row, status or series refs."

          match document.BaseRowId with
          | Some baseRowId ->
              yield! identifier "document.baseRowId" baseRowId
              match document.Rows |> Array.tryFind (fun row -> row.RowId = baseRowId) with
              | None ->
                  yield error "base-row-not-found" "document.baseRowId" "BaseRowId must identify a document row."
              | Some row when not row.Visible ->
                  yield error "base-row-not-visible" "document.baseRowId" "BaseRowId must identify a visible row."
              | Some _ -> ()
          | None -> ()

          if document.Rows.Length > limits.MaxRowsPerCanvas then
              yield error "limit-rows" "document.rows" $"Rows exceed hard limit {limits.MaxRowsPerCanvas}."

          if document.Rows |> Array.exists (fun row -> TaRowSpec.effectiveTraces row |> Array.length > limits.MaxTracesPerRow) then
              yield error "limit-traces-per-row" "document.rows" $"A row exceeds trace hard limit {limits.MaxTracesPerRow}."

          let totalTraceCount = document.Rows |> Array.sumBy (TaRowSpec.effectiveTraces >> Array.length)
          if totalTraceCount > limits.MaxTotalTraces then
              yield error "limit-total-traces" "document.rows" $"Canvas traces exceed hard limit {limits.MaxTotalTraces}."

          yield! document.Rows |> Array.toList |> List.mapi rowErrors |> List.concat

          let duplicateIds =
              document.Rows
              |> Array.countBy _.RowId
              |> Array.filter (fun (_, count) -> count > 1)

          if duplicateIds.Length > 0 then
              yield error "duplicate-row-id" "document.rows" "Row ids must be unique."

          if editorSchemas.Length > 128 then
              yield error "limit-editor-schemas" "document.editorSchemas" "Editor schema count exceeds hard limit 128."
          if editorSchemas |> Array.countBy _.TemplateKey |> Array.exists (fun (_, count) -> count > 1) then
              yield error "duplicate-template-key" "document.editorSchemas" "Editor template keys must be unique."
          yield! editorSchemas |> Array.toList |> List.mapi editorSchemaErrors |> List.concat

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
              |> Array.sumBy (function
                  | PatchOperation.UpsertSeriesPoints(_, _, items)
                  | PatchOperation.UpsertTemporalAxisPoints(_, _, _, items)
                  | PatchOperation.UpsertTemporalSeriesPoints(_, _, _, items) -> items.Length
                  | _ -> 0)

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
               | PatchOperation.UpsertTemporalAxisPoints(axisRef, expectedRevision, newRevision, items) ->
                   yield! identifier "patch.axisRef" axisRef
                   if expectedRevision < 0L || newRevision <= expectedRevision then
                       yield error "invalid-axis-revision" "patch.axisRevision" "Temporal axis revision must advance from a non-negative expected revision."
                   for item in items do
                       match Map.tryFind "position" item with
                       | Some(SduiValue.Number value) when value >= 0.0 && value = System.Math.Truncate value -> ()
                       | _ -> yield error "invalid-position" "patch.axisPoints" "Every temporal axis point must contain a non-negative integer position."
                       yield! unsafeValue "patch.axisPoint" (SduiValue.Object item)
               | PatchOperation.RemoveTemporalAxisBefore(axisRef, expectedRevision, newRevision, position) ->
                   yield! identifier "patch.axisRef" axisRef
                   if expectedRevision < 0L || newRevision <= expectedRevision then
                       yield error "invalid-axis-revision" "patch.axisRevision" "Temporal axis revision must advance from a non-negative expected revision."
                   if position < 0L then
                       yield error "invalid-position" "patch.position" "Temporal axis trim position must be non-negative."
               | PatchOperation.UpsertTemporalSeriesPoints(dataRef, axisRef, axisRevision, items) ->
                   yield! identifier "patch.dataRef" dataRef
                   yield! identifier "patch.axisRef" axisRef
                   if axisRevision < 0L then
                       yield error "invalid-axis-revision" "patch.axisRevision" "Temporal series axis revision must be non-negative."
                   for item in items do
                       match Map.tryFind "position" item, Map.tryFind "value" item with
                       | Some(SduiValue.Number value), Some payload when value >= 0.0 && value = System.Math.Truncate value ->
                           yield! unsafeValue "patch.seriesPoint.value" payload
                       | _ -> yield error "invalid-temporal-series-point" "patch.seriesPoints" "Every temporal series point must contain a non-negative integer position and value."
               | PatchOperation.RemoveTemporalSeriesBefore(dataRef, axisRef, axisRevision, position) ->
                   yield! identifier "patch.dataRef" dataRef
                   yield! identifier "patch.axisRef" axisRef
                   if axisRevision < 0L then
                       yield error "invalid-axis-revision" "patch.axisRevision" "Temporal series axis revision must be non-negative."
                   if position < 0L then
                       yield error "invalid-position" "patch.position" "Temporal series trim position must be non-negative."
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

              let itemCount =
                  match value with
                  | SduiValue.Array items -> Some items.Length
                  | SduiValue.Object fields ->
                      match Map.tryFind "_type" fields, Map.tryFind "points" fields with
                      | Some(SduiValue.Text kind), Some(SduiValue.Array items)
                          when kind = "temporal-axis.v1" || kind = "temporal-series.v1" -> Some items.Length
                      | _ -> None
                  | _ -> None
              match itemCount with
              | Some count when count > limits.MaxInitialBarsPerSeries ->
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
