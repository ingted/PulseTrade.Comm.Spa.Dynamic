namespace PulseTrade.Comm.Spa.Dynamic.Contracts

open System

type EditorChoice =
    { Key: string
      Label: string
      Value: SduiValue }

[<RequireQualifiedAccess>]
type EditorValueKind =
    | Text
    | Integer of minValue: int64 option * maxValue: int64 option
    | Decimal of minValue: decimal option * maxValue: decimal option
    | Boolean
    | Choice of EditorChoice array
    | Scale of allowedScaleKeys: string array
    | List of item: EditorValueKind * minItems: int option * maxItems: int option
    | Group of fields: EditorFieldSchema array

and EditorFieldSchema =
    { Key: string
      Label: string
      Kind: EditorValueKind
      Required: bool
      DefaultValue: SduiValue option }

type DynamicTemplateSchema =
    { TemplateKey: string
      DisplayName: string
      SchemaRevision: int64
      Fields: EditorFieldSchema array }

type DynamicEditorLimits =
    { MaxSchemaDepth: int
      MaxFields: int
      MaxChoicesPerField: int
      MaxListItems: int }

[<RequireQualifiedAccess>]
module DynamicEditorDefaults =
    let limits =
        { MaxSchemaDepth = 8
          MaxFields = 128
          MaxChoicesPerField = 128
          MaxListItems = 128 }

type DynamicActionRequest =
    { RequestId: string
      ExpectedDocumentRevision: int64 option
      Action: SduiAction }

[<RequireQualifiedAccess>]
type DynamicActionResult =
    | Accepted of requestId: string * resultingRevision: int64
    | Rejected of requestId: string * code: string * message: string
    | RevisionConflict of requestId: string * actualRevision: int64

type DynamicActionLifecycleState =
    { Pending: DynamicActionRequest option
      LastResult: DynamicActionResult option }

[<RequireQualifiedAccess>]
module DynamicEditorValidation =
    let nonNull values =
        if isNull values then [||] else values

    let duplicateKey field (keyOf: 'T -> string) (values: 'T array) =
        values
        |> Array.map keyOf
        |> Array.countBy id
        |> Array.exists (fun (_, count) -> count > 1)
        |> function
            | true -> [ RuntimeValidation.error "duplicate-key" field $"{field} keys must be unique." ]
            | false -> []

    let rangeErrors field minimum maximum =
        match minimum, maximum with
        | Some lower, Some upper when lower > upper ->
            [ RuntimeValidation.error "invalid-range" field $"{field} minimum must not exceed maximum." ]
        | _ -> []

    let listRangeErrors limits field minimum maximum =
        [ match minimum with
          | Some value when value < 0 ->
              yield RuntimeValidation.error "invalid-list-minimum" field $"{field} minimum must be non-negative."
          | _ -> ()

          match maximum with
          | Some value when value < 0 || value > limits.MaxListItems ->
              yield RuntimeValidation.error "invalid-list-maximum" field $"{field} maximum must be between zero and {limits.MaxListItems}."
          | _ -> ()

          yield! rangeErrors field minimum maximum ]

    let rec fieldCount kind =
        match kind with
        | EditorValueKind.Group fields ->
            nonNull fields
            |> Array.sumBy (fun field -> 1 + fieldCount field.Kind)
        | EditorValueKind.List(item, _, _) -> fieldCount item
        | _ -> 0

    let rec valueErrors limits field depth kind value =
        let nestedErrors nextKind nextValue =
            valueErrors limits field (depth + 1) nextKind nextValue

        [ yield! RuntimeValidation.unsafeValue field value

          match kind, value with
          | EditorValueKind.Text, SduiValue.Text _
          | EditorValueKind.Boolean, SduiValue.Bool _ -> ()
          | EditorValueKind.Integer(minimum, maximum), SduiValue.Number number ->
              if Double.IsNaN number || Double.IsInfinity number || Math.Truncate number <> number then
                  yield RuntimeValidation.error "invalid-integer" field $"{field} must be an integer."
              elif number < float Int64.MinValue || number > float Int64.MaxValue then
                  yield RuntimeValidation.error "integer-out-of-range" field $"{field} is outside Int64 range."
              else
                  let integer = int64 number
                  match minimum with
                  | Some lower when integer < lower -> yield RuntimeValidation.error "below-minimum" field $"{field} is below its minimum."
                  | _ -> ()
                  match maximum with
                  | Some upper when integer > upper -> yield RuntimeValidation.error "above-maximum" field $"{field} exceeds its maximum."
                  | _ -> ()
          | EditorValueKind.Decimal(minimum, maximum), SduiValue.Number number ->
              if Double.IsNaN number || Double.IsInfinity number then
                  yield RuntimeValidation.error "invalid-decimal" field $"{field} must be finite."
              else
                  let decimalValue = decimal number
                  match minimum with
                  | Some lower when decimalValue < lower -> yield RuntimeValidation.error "below-minimum" field $"{field} is below its minimum."
                  | _ -> ()
                  match maximum with
                  | Some upper when decimalValue > upper -> yield RuntimeValidation.error "above-maximum" field $"{field} exceeds its maximum."
                  | _ -> ()
          | EditorValueKind.Choice choices, _ ->
              if nonNull choices |> Array.exists (fun choice -> choice.Value = value) |> not then
                  yield RuntimeValidation.error "choice-not-allowed" field $"{field} is not one of the declared choices."
          | EditorValueKind.Scale scaleKeys, SduiValue.Text scaleKey ->
              if nonNull scaleKeys |> Array.contains scaleKey |> not then
                  yield RuntimeValidation.error "scale-not-allowed" field $"{field} is not one of the declared scales."
          | EditorValueKind.List(itemKind, minimum, maximum), SduiValue.Array items ->
              let values = nonNull items
              match minimum with
              | Some lower when values.Length < lower -> yield RuntimeValidation.error "list-too-short" field $"{field} has fewer than the required items."
              | _ -> ()
              match maximum with
              | Some upper when values.Length > upper -> yield RuntimeValidation.error "list-too-long" field $"{field} exceeds its item limit."
              | _ -> ()
              if values.Length > limits.MaxListItems then
                  yield RuntimeValidation.error "limit-list-items" field $"{field} exceeds hard limit {limits.MaxListItems}."
              for index, item in values |> Array.indexed do
                  yield! valueErrors limits $"{field}[{index}]" (depth + 1) itemKind item
          | EditorValueKind.Group fields, SduiValue.Object values ->
              for child in nonNull fields do
                  match Map.tryFind child.Key values with
                  | Some childValue -> yield! valueErrors limits $"{field}.{child.Key}" (depth + 1) child.Kind childValue
                  | None when child.Required -> yield RuntimeValidation.error "required" $"{field}.{child.Key}" $"{child.Key} is required."
                  | None -> ()
          | _ ->
              yield RuntimeValidation.error "editor-value-kind-mismatch" field $"{field} does not match its editor kind." ]

    and kindErrors limits field depth kind =
        [ if depth > limits.MaxSchemaDepth then
              yield RuntimeValidation.error "limit-schema-depth" field $"Schema depth exceeds hard limit {limits.MaxSchemaDepth}."

          match kind with
          | EditorValueKind.Integer(minimum, maximum) -> yield! rangeErrors field minimum maximum
          | EditorValueKind.Decimal(minimum, maximum) -> yield! rangeErrors field minimum maximum
          | EditorValueKind.Choice choices ->
              let values = nonNull choices
              if values.Length = 0 then
                  yield RuntimeValidation.error "choice-required" field "Choice editor requires at least one choice."
              if values.Length > limits.MaxChoicesPerField then
                  yield RuntimeValidation.error "limit-editor-choices" field $"Choices exceed hard limit {limits.MaxChoicesPerField}."
              yield! duplicateKey field (fun (choice: EditorChoice) -> choice.Key) values
              for index, choice in values |> Array.indexed do
                  yield! RuntimeValidation.identifier $"{field}.choices[{index}].key" choice.Key
                  yield! RuntimeValidation.identifier $"{field}.choices[{index}].label" choice.Label
                  yield! RuntimeValidation.unsafeValue $"{field}.choices[{index}].value" choice.Value
          | EditorValueKind.Scale scaleKeys ->
              let values = nonNull scaleKeys
              if values.Length = 0 then
                  yield RuntimeValidation.error "scale-required" field "Scale editor requires at least one allowed key."
              yield! duplicateKey field id values
              for index, scaleKey in values |> Array.indexed do
                  yield! RuntimeValidation.identifier $"{field}.scaleKeys[{index}]" scaleKey
          | EditorValueKind.List(itemKind, minimum, maximum) ->
              yield! listRangeErrors limits field minimum maximum
              yield! kindErrors limits $"{field}.item" (depth + 1) itemKind
          | EditorValueKind.Group fields ->
              let values = nonNull fields
              yield! duplicateKey field (fun (child: EditorFieldSchema) -> child.Key) values
              for index, child in values |> Array.indexed do
                  yield! fieldErrors limits $"{field}.fields[{index}]" (depth + 1) child
          | _ -> () ]

    and fieldErrors limits field depth (schema: EditorFieldSchema) =
        [ yield! RuntimeValidation.identifier $"{field}.key" schema.Key
          yield! RuntimeValidation.identifier $"{field}.label" schema.Label
          yield! kindErrors limits $"{field}.kind" depth schema.Kind
          match schema.DefaultValue with
          | Some value -> yield! valueErrors limits $"{field}.defaultValue" depth schema.Kind value
          | None -> () ]

    let schemaErrors limits (schema: DynamicTemplateSchema) =
        let fields = nonNull schema.Fields
        [ yield! RuntimeValidation.identifier "schema.templateKey" schema.TemplateKey
          yield! RuntimeValidation.identifier "schema.displayName" schema.DisplayName
          if schema.SchemaRevision < 0L then
              yield RuntimeValidation.error "invalid-schema-revision" "schema.schemaRevision" "Schema revision must be non-negative."
          let totalFields = fields.Length + (fields |> Array.sumBy (fun field -> fieldCount field.Kind))
          if totalFields > limits.MaxFields then
              yield RuntimeValidation.error "limit-editor-fields" "schema.fields" $"Editor fields exceed hard limit {limits.MaxFields}."
          yield! duplicateKey "schema.fields" (fun (field: EditorFieldSchema) -> field.Key) fields
          for index, field in fields |> Array.indexed do
              yield! fieldErrors limits $"schema.fields[{index}]" 1 field ]

    let validateSchema limits schema =
        match schemaErrors limits schema with
        | [] -> Ok schema
        | errors -> Error errors

[<RequireQualifiedAccess>]
module DynamicActionValidation =
    let canvasErrors field (CanvasInstanceId canvasId) =
        RuntimeValidation.identifier field canvasId

    let queryErrors (query: TaQueryChange) =
        [ match query.IntervalMinutes with
          | Some value when value <= 0 ->
              yield RuntimeValidation.error "invalid-interval" "action.query.intervalMinutes" "Interval must be positive."
          | _ -> ()
          for field, value in
              [ "sourceId", query.SourceId
                "instrument", query.Instrument
                "fromUtc", query.FromUtc
                "toUtcExclusive", query.ToUtcExclusive ] do
              match value with
              | Some text -> yield! RuntimeValidation.identifier $"action.query.{field}" text
              | None -> () ]

    let actionErrors action =
        match action with
        | SduiAction.ResetView canvas
        | SduiAction.ResetCanvas canvas -> canvasErrors "action.canvasInstanceId" canvas
        | SduiAction.AddTaRow(canvas, row) ->
            canvasErrors "action.canvasInstanceId" canvas @ RuntimeValidation.rowErrors 0 row
        | SduiAction.RemoveTaRow(canvas, rowId) ->
            canvasErrors "action.canvasInstanceId" canvas @ RuntimeValidation.identifier "action.rowId" rowId
        | SduiAction.ChangeTaQuery(canvas, query) ->
            canvasErrors "action.canvasInstanceId" canvas @ queryErrors query
        | SduiAction.PollDelta(canvas, revision) ->
            [ yield! canvasErrors "action.canvasInstanceId" canvas
              if revision < 0L then
                  yield RuntimeValidation.error "invalid-data-revision" "action.afterDataRevision" "Data revision must be non-negative." ]
        | SduiAction.RequestFullSnapshot(canvas, reasonCode) ->
            canvasErrors "action.canvasInstanceId" canvas @ RuntimeValidation.identifier "action.reasonCode" reasonCode

    let requestErrors (request: DynamicActionRequest) =
        [ yield! RuntimeValidation.identifier "action.requestId" request.RequestId
          match request.ExpectedDocumentRevision with
          | Some revision when revision < 0L ->
              yield RuntimeValidation.error "invalid-document-revision" "action.expectedDocumentRevision" "Expected document revision must be non-negative."
          | _ -> ()
          yield! actionErrors request.Action ]

    let resultRequestId result =
        match result with
        | DynamicActionResult.Accepted(requestId, _)
        | DynamicActionResult.Rejected(requestId, _, _)
        | DynamicActionResult.RevisionConflict(requestId, _) -> requestId

    let resultErrors result =
        [ yield! RuntimeValidation.identifier "actionResult.requestId" (resultRequestId result)
          match result with
          | DynamicActionResult.Accepted(_, revision)
          | DynamicActionResult.RevisionConflict(_, revision) when revision < 0L ->
              yield RuntimeValidation.error "invalid-document-revision" "actionResult.revision" "Result revision must be non-negative."
          | DynamicActionResult.Rejected(_, code, message) ->
              yield! RuntimeValidation.identifier "actionResult.code" code
              if isNull message || message.Length > 512 then
                  yield RuntimeValidation.error "invalid-action-message" "actionResult.message" "Action result message must not exceed 512 characters."
          | _ -> () ]

[<RequireQualifiedAccess>]
module DynamicActionLifecycle =
    let empty =
        { Pending = None
          LastResult = None }

    let beginRequest actualDocumentRevision request state =
        let errors =
            [ yield! DynamicActionValidation.requestErrors request
              if actualDocumentRevision < 0L then
                  yield RuntimeValidation.error "invalid-document-revision" "actualDocumentRevision" "Actual document revision must be non-negative."
              if state.Pending.IsSome then
                  yield RuntimeValidation.error "action-in-flight" "action.pending" "An action is already in flight." ]

        match errors with
        | _ :: _ -> Error errors
        | [] ->
            match request.ExpectedDocumentRevision with
            | Some expected when expected <> actualDocumentRevision ->
                Ok
                    { Pending = None
                      LastResult = Some(DynamicActionResult.RevisionConflict(request.RequestId, actualDocumentRevision)) }
            | _ ->
                Ok
                    { Pending = Some request
                      LastResult = state.LastResult }

    let complete result state =
        let validationErrors = DynamicActionValidation.resultErrors result

        match validationErrors, state.Pending with
        | _ :: _, _ -> Error validationErrors
        | [], None ->
            Error [ RuntimeValidation.error "no-action-in-flight" "action.pending" "No action is in flight." ]
        | [], Some pending when DynamicActionValidation.resultRequestId result <> pending.RequestId ->
            Error [ RuntimeValidation.error "action-correlation-mismatch" "actionResult.requestId" "Action result does not match the pending request." ]
        | [], Some _ ->
            Ok
                { Pending = None
                  LastResult = Some result }
