namespace PulseTrade.Comm.Spa.Dynamic.Contracts

open System

type DynamicEditorLimits =
    { MaxSchemaDepth: int
      MaxFields: int
      MaxChoicesPerField: int
      MaxListItems: int }

[<WebSharper.JavaScript; RequireQualifiedAccess>]
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

type DynamicActionClientFrame =
    { Protocol: string
      Kind: string
      Request: DynamicActionRequest }

type DynamicActionServerFrame =
    { Protocol: string
      Kind: string
      Result: DynamicActionResult }

[<WebSharper.JavaScript; RequireQualifiedAccess>]
module DynamicActionWireDefaults =
    [<Literal>]
    let Protocol = "ptcs-dynamic-action.v1"

    [<Literal>]
    let RequestKind = "action-request"

    [<Literal>]
    let ResultKind = "action-result"

    let requestFrame request =
        { Protocol = Protocol
          Kind = RequestKind
          Request = request }

    let resultFrame result =
        { Protocol = Protocol
          Kind = ResultKind
          Result = result }

type DynamicActionLifecycleState =
    { Pending: DynamicActionRequest option
      LastResult: DynamicActionResult option }

[<WebSharper.JavaScript; RequireQualifiedAccess>]
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
                  match minimum with
                  | Some lower when number < lower -> yield RuntimeValidation.error "below-minimum" field $"{field} is below its minimum."
                  | _ -> ()
                  match maximum with
                  | Some upper when number > upper -> yield RuntimeValidation.error "above-maximum" field $"{field} exceeds its maximum."
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

    let scalarOfValue = function
        | SduiValue.Text value -> Some(EditorScalarValue.Text value)
        | SduiValue.Number value -> Some(EditorScalarValue.Number value)
        | SduiValue.Bool value -> Some(EditorScalarValue.Bool value)
        | _ -> None

    let scalarAsValue = function
        | EditorScalarValue.Text value -> SduiValue.Text value
        | EditorScalarValue.Number value -> SduiValue.Number value
        | EditorScalarValue.Bool value -> SduiValue.Bool value

    let rec flattenValue path kind value =
        match kind, value with
        | EditorValueKind.Group fields, SduiValue.Object values ->
            nonNull fields
            |> Array.collect (fun field ->
                match Map.tryFind field.Key values with
                | Some child -> flattenValue ($"{path}.{field.Key}") field.Kind child
                | None -> [||])
        | EditorValueKind.List(itemKind, _, _), SduiValue.Array values ->
            nonNull values
            |> Array.indexed
            |> Array.collect (fun (index, item) -> flattenValue ($"{path}[{index}]") itemKind item)
        | _, scalar ->
            match scalarOfValue scalar with
            | Some value -> [| { Path = path; Value = value } |]
            | None -> [||]

    let rec fieldDefaults path (field: EditorFieldSchema) =
        match field.DefaultValue with
        | Some value -> flattenValue path field.Kind value
        | None ->
            match field.Kind with
            | EditorValueKind.Group fields ->
                nonNull fields
                |> Array.collect (fun child -> fieldDefaults ($"{path}.{child.Key}") child)
            | _ -> [||]

    let defaultInputs schema =
        nonNull schema.Fields
        |> Array.collect (fun field -> fieldDefaults field.Key field)

    let takeListIndex limits field (text: string) =
        if String.IsNullOrEmpty text || text[0] <> '[' then
            Error [ RuntimeValidation.error "editor-list-index-required" field $"{field} requires a list index." ]
        else
            let closeIndex = text.IndexOf(']')
            if closeIndex <= 1 then
                Error [ RuntimeValidation.error "editor-list-index-invalid" field $"{field} has an invalid list index." ]
            else
                match Int32.TryParse(text.Substring(1, closeIndex - 1)) with
                | true, index when index >= 0 && index < limits.MaxListItems -> Ok(index, text.Substring(closeIndex + 1))
                | _ -> Error [ RuntimeValidation.error "editor-list-index-invalid" field $"{field} list index is outside the allowed range." ]

    let firstPathSeparator (text: string) =
        let dotIndex = text.IndexOf('.')
        let bracketIndex = text.IndexOf('[')
        if dotIndex < 0 then bracketIndex
        elif bracketIndex < 0 then dotIndex
        else min dotIndex bracketIndex

    let rec resolveKind (limits: DynamicEditorLimits) (field: string) (kind: EditorValueKind) (remainder: string) =
        match kind with
        | EditorValueKind.Group fields when remainder.StartsWith(".") ->
            let childPath = remainder.Substring(1)
            let separator = firstPathSeparator childPath
            let key = if separator < 0 then childPath else childPath.Substring(0, separator)
            let childRemainder = if separator < 0 then "" else childPath.Substring(separator)
            match nonNull fields |> Array.tryFind (fun child -> child.Key = key) with
            | Some child -> resolveKind limits field child.Kind childRemainder
            | None -> Error [ RuntimeValidation.error "editor-path-unknown" field $"{field} does not exist in the template schema." ]
        | EditorValueKind.List(itemKind, _, _) ->
            takeListIndex limits field remainder
            |> Result.bind (fun (_, childRemainder) -> resolveKind limits field itemKind childRemainder)
        | _ when remainder = "" -> Ok kind
        | _ -> Error [ RuntimeValidation.error "editor-path-invalid" field $"{field} does not match the template schema." ]

    let resolvePath limits (schema: DynamicTemplateSchema) (path: string) =
        let separator = if isNull path then -1 else firstPathSeparator path
        let key =
            if String.IsNullOrWhiteSpace path then ""
            elif separator < 0 then path
            else path.Substring(0, separator)
        let remainder = if separator < 0 then "" else path.Substring(separator)
        match nonNull schema.Fields |> Array.tryFind (fun field -> field.Key = key) with
        | Some field -> resolveKind limits path field.Kind remainder
        | None -> Error [ RuntimeValidation.error "editor-path-unknown" path $"{path} does not exist in the template schema." ]

    let scalarErrors field kind scalar =
        let value = scalarAsValue scalar
        valueErrors DynamicEditorDefaults.limits field 1 kind value

    let inputErrors limits schema values =
        let inputs = nonNull values
        [ yield! schemaErrors limits schema
          if inputs.Length > limits.MaxFields then
              yield RuntimeValidation.error "limit-editor-inputs" "editor.values" $"Editor inputs exceed hard limit {limits.MaxFields}."
          if inputs |> Array.countBy _.Path |> Array.exists (fun (_, count) -> count > 1) then
              yield RuntimeValidation.error "duplicate-editor-input" "editor.values" "Editor input paths must be unique."
          for index, input in inputs |> Array.indexed do
              yield! RuntimeValidation.identifier $"editor.values[{index}].path" input.Path
              match resolvePath limits schema input.Path with
              | Ok kind -> yield! scalarErrors $"editor.values[{index}].value" kind input.Value
              | Error errors -> yield! errors ]

    let validateInputs limits schema values =
        match inputErrors limits schema values with
        | [] -> Ok values
        | errors -> Error errors

[<WebSharper.JavaScript; RequireQualifiedAccess>]
module DynamicTemplateSchemaCodec =
    let optionInt64Number (value: int64 option) = value |> Option.map (float >> SduiValue.Number)
    let optionIntNumber (value: int option) = value |> Option.map (float >> SduiValue.Number)

    let rec kindToValue kind =
        let fields =
            match kind with
            | EditorValueKind.Text -> Map [ "type", SduiValue.Text "text" ]
            | EditorValueKind.Integer(minimum, maximum) ->
                Map [
                    "type", SduiValue.Text "integer"
                    match optionInt64Number minimum with Some value -> "minimum", value | None -> ()
                    match optionInt64Number maximum with Some value -> "maximum", value | None -> ()
                ]
            | EditorValueKind.Decimal(minimum, maximum) ->
                Map [
                    "type", SduiValue.Text "decimal"
                    match minimum with Some value -> "minimum", SduiValue.Number value | None -> ()
                    match maximum with Some value -> "maximum", SduiValue.Number value | None -> ()
                ]
            | EditorValueKind.Boolean -> Map [ "type", SduiValue.Text "boolean" ]
            | EditorValueKind.Choice choices ->
                Map [
                    "type", SduiValue.Text "choice"
                    "choices",
                    SduiValue.Array(
                        DynamicEditorValidation.nonNull choices
                        |> Array.map (fun choice ->
                            SduiValue.Object(
                                Map [
                                    "key", SduiValue.Text choice.Key
                                    "label", SduiValue.Text choice.Label
                                    "value", choice.Value
                                ])))
                ]
            | EditorValueKind.Scale scaleKeys ->
                Map [
                    "type", SduiValue.Text "scale"
                    "scaleKeys", SduiValue.Array(DynamicEditorValidation.nonNull scaleKeys |> Array.map SduiValue.Text)
                ]
            | EditorValueKind.List(item, minimum, maximum) ->
                Map [
                    "type", SduiValue.Text "list"
                    "item", kindToValue item
                    match optionIntNumber minimum with Some value -> "minimum", value | None -> ()
                    match optionIntNumber maximum with Some value -> "maximum", value | None -> ()
                ]
            | EditorValueKind.Group childFields ->
                Map [
                    "type", SduiValue.Text "group"
                    "fields", SduiValue.Array(DynamicEditorValidation.nonNull childFields |> Array.map fieldToValue)
                ]

        SduiValue.Object fields

    and fieldToValue field =
        SduiValue.Object(
            Map [
                "key", SduiValue.Text field.Key
                "label", SduiValue.Text field.Label
                "kind", kindToValue field.Kind
                "required", SduiValue.Bool field.Required
                match field.DefaultValue with Some value -> "defaultValue", value | None -> ()
            ])

    let toValue schema =
        SduiValue.Object(
            Map [
                "templateKey", SduiValue.Text schema.TemplateKey
                "displayName", SduiValue.Text schema.DisplayName
                "schemaRevision", SduiValue.Number(float schema.SchemaRevision)
                "fields", SduiValue.Array(DynamicEditorValidation.nonNull schema.Fields |> Array.map fieldToValue)
            ])

    let error code field message = Error [ RuntimeValidation.error code field message ]

    let optionalInt field key values =
        match Map.tryFind key values with
        | None -> Ok None
        | Some(SduiValue.Number value) when value >= 0.0 && value <= float Int32.MaxValue && Math.Truncate value = value -> Ok(Some(int value))
        | _ -> error "editor-schema-number" field "Editor schema bound must be a non-negative integer."

    let optionalInt64 field key values =
        match Map.tryFind key values with
        | None -> Ok None
        | Some(SduiValue.Number value) when value >= float Int64.MinValue && value <= float Int64.MaxValue && Math.Truncate value = value -> Ok(Some(int64 value))
        | _ -> error "editor-schema-number" field "Editor schema bound must be an integer."

    let optionalFloat field key values =
        match Map.tryFind key values with
        | None -> Ok None
        | Some(SduiValue.Number value) when not (Double.IsNaN value || Double.IsInfinity value) -> Ok(Some value)
        | _ -> error "editor-schema-number" field "Editor schema bound must be finite."

    let sequence results =
        let errors = results |> Array.collect (function Error values -> List.toArray values | Ok _ -> [||])
        if errors.Length > 0 then Error(List.ofArray errors)
        else Ok(results |> Array.choose (function Ok value -> Some value | Error _ -> None))

    let rec kindFromValue field value =
        match value with
        | SduiValue.Object values ->
            match Map.tryFind "type" values with
            | Some(SduiValue.Text "text") -> Ok EditorValueKind.Text
            | Some(SduiValue.Text "boolean") -> Ok EditorValueKind.Boolean
            | Some(SduiValue.Text "integer") ->
                match optionalInt64 (field + ".minimum") "minimum" values, optionalInt64 (field + ".maximum") "maximum" values with
                | Ok minimum, Ok maximum -> Ok(EditorValueKind.Integer(minimum, maximum))
                | Error errors, Ok _
                | Ok _, Error errors -> Error errors
                | Error left, Error right -> Error(left @ right)
            | Some(SduiValue.Text "decimal") ->
                match optionalFloat (field + ".minimum") "minimum" values, optionalFloat (field + ".maximum") "maximum" values with
                | Ok minimum, Ok maximum -> Ok(EditorValueKind.Decimal(minimum, maximum))
                | Error errors, Ok _
                | Ok _, Error errors -> Error errors
                | Error left, Error right -> Error(left @ right)
            | Some(SduiValue.Text "choice") ->
                match Map.tryFind "choices" values with
                | Some(SduiValue.Array choices) ->
                    DynamicEditorValidation.nonNull choices
                    |> Array.mapi (fun index choice -> choiceFromValue $"{field}.choices[{index}]" choice)
                    |> sequence
                    |> Result.map EditorValueKind.Choice
                | _ -> error "editor-schema-choice" field "Choice schema requires choices."
            | Some(SduiValue.Text "scale") ->
                match Map.tryFind "scaleKeys" values with
                | Some(SduiValue.Array scaleKeys) ->
                    let decoded =
                        DynamicEditorValidation.nonNull scaleKeys
                        |> Array.mapi (fun index item ->
                            match item with
                            | SduiValue.Text key -> Ok key
                            | _ -> error "editor-schema-scale" $"{field}.scaleKeys[{index}]" "Scale key must be text.")
                    sequence decoded |> Result.map EditorValueKind.Scale
                | _ -> error "editor-schema-scale" field "Scale schema requires scaleKeys."
            | Some(SduiValue.Text "list") ->
                match Map.tryFind "item" values with
                | None -> error "editor-schema-list" field "List schema requires an item kind."
                | Some item ->
                    match kindFromValue (field + ".item") item, optionalInt (field + ".minimum") "minimum" values, optionalInt (field + ".maximum") "maximum" values with
                    | Ok itemKind, Ok minimum, Ok maximum -> Ok(EditorValueKind.List(itemKind, minimum, maximum))
                    | results ->
                        [ match results with
                          | Error errors, _, _ -> yield! errors
                          | _ -> ()
                          match results with
                          | _, Error errors, _ -> yield! errors
                          | _ -> ()
                          match results with
                          | _, _, Error errors -> yield! errors
                          | _ -> () ]
                        |> Error
            | Some(SduiValue.Text "group") ->
                match Map.tryFind "fields" values with
                | Some(SduiValue.Array fields) ->
                    DynamicEditorValidation.nonNull fields
                    |> Array.mapi (fun index child -> fieldFromValue $"{field}.fields[{index}]" child)
                    |> sequence
                    |> Result.map EditorValueKind.Group
                | _ -> error "editor-schema-group" field "Group schema requires fields."
            | _ -> error "editor-schema-kind" field "Unknown editor schema kind."
        | _ -> error "editor-schema-kind" field "Editor schema kind must be an object."

    and choiceFromValue field value =
        match value with
        | SduiValue.Object values ->
            match Map.tryFind "key" values, Map.tryFind "label" values, Map.tryFind "value" values with
            | Some(SduiValue.Text key), Some(SduiValue.Text label), Some choiceValue ->
                Ok { Key = key; Label = label; Value = choiceValue }
            | _ -> error "editor-schema-choice" field "Editor choice requires key, label and value."
        | _ -> error "editor-schema-choice" field "Editor choice must be an object."

    and fieldFromValue field value =
        match value with
        | SduiValue.Object values ->
            match Map.tryFind "key" values, Map.tryFind "label" values, Map.tryFind "kind" values, Map.tryFind "required" values with
            | Some(SduiValue.Text key), Some(SduiValue.Text label), Some kind, Some(SduiValue.Bool required) ->
                kindFromValue (field + ".kind") kind
                |> Result.map (fun decodedKind ->
                    { Key = key
                      Label = label
                      Kind = decodedKind
                      Required = required
                      DefaultValue = Map.tryFind "defaultValue" values })
            | _ -> error "editor-schema-field" field "Editor field requires key, label, kind and required."
        | _ -> error "editor-schema-field" field "Editor field must be an object."

    let fromValue value =
        match value with
        | SduiValue.Object values ->
            match Map.tryFind "templateKey" values, Map.tryFind "displayName" values, Map.tryFind "schemaRevision" values, Map.tryFind "fields" values with
            | Some(SduiValue.Text templateKey), Some(SduiValue.Text displayName), Some(SduiValue.Number revision), Some(SduiValue.Array fields)
                when revision >= 0.0 && revision <= float Int64.MaxValue && Math.Truncate revision = revision ->
                DynamicEditorValidation.nonNull fields
                |> Array.mapi (fun index field -> fieldFromValue $"schema.fields[{index}]" field)
                |> sequence
                |> Result.bind (fun decodedFields ->
                    { TemplateKey = templateKey
                      DisplayName = displayName
                      SchemaRevision = int64 revision
                      Fields = decodedFields }
                    |> DynamicEditorValidation.validateSchema DynamicEditorDefaults.limits)
            | _ -> error "editor-schema-shape" "schema" "Template schema requires templateKey, displayName, schemaRevision and fields."
        | _ -> error "editor-schema-shape" "schema" "Template schema must be an object."

type TaRowEditorBinding =
    { TemplateKey: string
      Values: EditorInputValue array }

[<WebSharper.JavaScript; RequireQualifiedAccess>]
module TaRowEditorBinding =
    [<Literal>]
    let OptionKey = "ptcs.dynamic.editor.binding.v1"

    let scalarValue = function
        | EditorScalarValue.Text value -> SduiValue.Text value
        | EditorScalarValue.Number value -> SduiValue.Number value
        | EditorScalarValue.Bool value -> SduiValue.Bool value

    let valueScalar = function
        | SduiValue.Text value -> Some(EditorScalarValue.Text value)
        | SduiValue.Number value -> Some(EditorScalarValue.Number value)
        | SduiValue.Bool value -> Some(EditorScalarValue.Bool value)
        | _ -> None

    let toValue binding =
        let values =
            DynamicEditorValidation.nonNull binding.Values
            |> Array.map (fun input ->
                SduiValue.Object(
                    Map [
                        "path", SduiValue.Text input.Path
                        "value", scalarValue input.Value
                    ]))

        SduiValue.Object(
            Map [
                "templateKey", SduiValue.Text binding.TemplateKey
                "values", SduiValue.Array values
            ])

    let bindingErrors binding =
        let values = DynamicEditorValidation.nonNull binding.Values

        [ yield! RuntimeValidation.identifier "row.editorBinding.templateKey" binding.TemplateKey
          if values.Length > DynamicEditorDefaults.limits.MaxFields then
              yield
                  RuntimeValidation.error
                      "limit-editor-inputs"
                      "row.editorBinding.values"
                      $"Editor binding inputs exceed hard limit {DynamicEditorDefaults.limits.MaxFields}."
          if values |> Array.countBy _.Path |> Array.exists (fun (_, count) -> count > 1) then
              yield
                  RuntimeValidation.error
                      "duplicate-editor-input"
                      "row.editorBinding.values"
                      "Editor binding paths must be unique."
          for index, input in values |> Array.indexed do
              yield! RuntimeValidation.identifier $"row.editorBinding.values[{index}].path" input.Path
              match input.Value with
              | EditorScalarValue.Text value ->
                  yield! RuntimeValidation.unsafeValue $"row.editorBinding.values[{index}].value" (SduiValue.Text value)
              | EditorScalarValue.Number value when Double.IsNaN value || Double.IsInfinity value ->
                  yield
                      RuntimeValidation.error
                          "invalid-editor-number"
                          $"row.editorBinding.values[{index}].value"
                          "Editor binding number must be finite."
              | _ -> () ]

    let validate binding =
        match bindingErrors binding with
        | [] -> Ok binding
        | errors -> Error errors

    let tryDecodeValue value =
        match value with
        | SduiValue.Object fields ->
            match Map.tryFind "templateKey" fields, Map.tryFind "values" fields with
            | Some(SduiValue.Text templateKey), Some(SduiValue.Array values) ->
                let decoded =
                    DynamicEditorValidation.nonNull values
                    |> Array.mapi (fun index item ->
                        match item with
                        | SduiValue.Object input ->
                            match Map.tryFind "path" input, Map.tryFind "value" input with
                            | Some(SduiValue.Text path), Some scalar ->
                                match valueScalar scalar with
                                | Some scalarValue -> Ok { Path = path; Value = scalarValue }
                                | None ->
                                    Error(
                                        RuntimeValidation.error
                                            "editor-binding-value-kind"
                                            $"row.editorBinding.values[{index}].value"
                                            "Editor binding values must be text, number or boolean.")
                            | _ ->
                                Error(
                                    RuntimeValidation.error
                                        "editor-binding-input-shape"
                                        $"row.editorBinding.values[{index}]"
                                        "Editor binding input requires path and value.")
                        | _ ->
                            Error(
                                RuntimeValidation.error
                                    "editor-binding-input-shape"
                                    $"row.editorBinding.values[{index}]"
                                    "Editor binding inputs must be objects."))

                let errors = decoded |> Array.choose (function Error error -> Some error | Ok _ -> None)

                if errors.Length > 0 then
                    Error(List.ofArray errors)
                else
                    { TemplateKey = templateKey
                      Values = decoded |> Array.choose (function Ok input -> Some input | Error _ -> None) }
                    |> validate
            | _ ->
                Error
                    [ RuntimeValidation.error
                          "editor-binding-shape"
                          "row.editorBinding"
                          "Editor binding requires templateKey and values." ]
        | _ ->
            Error
                [ RuntimeValidation.error
                      "editor-binding-shape"
                      "row.editorBinding"
                      "Editor binding must be an object." ]

    let attach (binding: TaRowEditorBinding) (row: TaRowSpec) =
        match validate binding with
        | Error errors -> Error errors
        | Ok valid ->
            Ok
                { row with
                    Options = row.Options |> Map.add OptionKey (toValue valid) }

    let tryFind (row: TaRowSpec) =
        match row.Options |> Map.tryFind OptionKey with
        | None -> Ok None
        | Some value -> tryDecodeValue value |> Result.map Some

    let validateForSchema (schema: DynamicTemplateSchema) (binding: TaRowEditorBinding) =
        if schema.TemplateKey <> binding.TemplateKey then
            Error
                [ RuntimeValidation.error
                      "editor-binding-template-mismatch"
                      "row.editorBinding.templateKey"
                      "Editor binding template does not match the selected schema." ]
        else
            DynamicEditorValidation.validateInputs DynamicEditorDefaults.limits schema binding.Values

    let tryResolve (schemas: DynamicTemplateSchema array) (row: TaRowSpec) =
        tryFind row
        |> Result.bind (function
            | None -> Ok None
            | Some binding ->
                match DynamicEditorValidation.nonNull schemas |> Array.tryFind (fun schema -> schema.TemplateKey = binding.TemplateKey) with
                | None ->
                    Error
                        [ RuntimeValidation.error
                              "editor-binding-schema-unavailable"
                              "row.editorBinding.templateKey"
                              "Editor binding schema is unavailable." ]
                | Some schema ->
                    validateForSchema schema binding
                    |> Result.map (fun values -> Some(schema, values)))

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
        | SduiAction.ApplyTemplate(canvas, rowId, templateKey, values) ->
            [ yield! canvasErrors "action.canvasInstanceId" canvas
              match rowId with
              | Some value -> yield! RuntimeValidation.identifier "action.rowId" value
              | None -> ()
              yield! RuntimeValidation.identifier "action.templateKey" templateKey
              let inputs = DynamicEditorValidation.nonNull values
              if inputs.Length > DynamicEditorDefaults.limits.MaxFields then
                  yield RuntimeValidation.error "limit-editor-inputs" "action.values" $"Editor inputs exceed hard limit {DynamicEditorDefaults.limits.MaxFields}."
              if inputs |> Array.countBy _.Path |> Array.exists (fun (_, count) -> count > 1) then
                  yield RuntimeValidation.error "duplicate-editor-input" "action.values" "Editor input paths must be unique."
              for index, input in inputs |> Array.indexed do
                  yield! RuntimeValidation.identifier $"action.values[{index}].path" input.Path
                  match input.Value with
                  | EditorScalarValue.Text value -> yield! RuntimeValidation.unsafeValue $"action.values[{index}].value" (SduiValue.Text value)
                  | EditorScalarValue.Number value when Double.IsNaN value || Double.IsInfinity value ->
                      yield RuntimeValidation.error "invalid-editor-number" $"action.values[{index}].value" "Editor number must be finite."
                  | _ -> () ]
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

    let clientFrameErrors (frame: DynamicActionClientFrame) =
        [ if frame.Protocol <> DynamicActionWireDefaults.Protocol then
              yield RuntimeValidation.error "unsupported-action-protocol" "actionFrame.protocol" "Unsupported dynamic action protocol."
          if frame.Kind <> DynamicActionWireDefaults.RequestKind then
              yield RuntimeValidation.error "invalid-action-frame-kind" "actionFrame.kind" "Dynamic action client frame must be an action request."
          if isNull (box frame.Request) then
              yield RuntimeValidation.error "missing-action-request" "actionFrame.request" "Dynamic action request is required."
          else
              yield! requestErrors frame.Request ]

    let serverFrameErrors (frame: DynamicActionServerFrame) =
        [ if frame.Protocol <> DynamicActionWireDefaults.Protocol then
              yield RuntimeValidation.error "unsupported-action-protocol" "actionFrame.protocol" "Unsupported dynamic action protocol."
          if frame.Kind <> DynamicActionWireDefaults.ResultKind then
              yield RuntimeValidation.error "invalid-action-frame-kind" "actionFrame.kind" "Dynamic action server frame must be an action result."
          if isNull (box frame.Result) then
              yield RuntimeValidation.error "missing-action-result" "actionFrame.result" "Dynamic action result is required."
          else
              yield! resultErrors frame.Result ]

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
