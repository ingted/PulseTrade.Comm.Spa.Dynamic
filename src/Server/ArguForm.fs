namespace PulseTrade.Comm.Spa.Dynamic.Server

open System
open System.Reflection
open System.Text.Json
open System.Text.Json.Serialization
open Argu
open Microsoft.FSharp.Reflection

[<CLIMutable>]
type ArguFormField =
    { Name: string
      Label: string
      Kind: string
      ArguName: string
      Options: string array
      Items: ArguFormField array }

[<CLIMutable>]
type ArguFormUnionCase =
    { Name: string
      Label: string
      ArguName: string
      Fields: ArguFormField array }

[<CLIMutable>]
type ArguFormSchema =
    { Schema: string
      FormMode: string
      DuTypeName: string
      UnionCases: ArguFormUnionCase array }

[<CLIMutable>]
type SduiFormOption =
    { Value: string
      Label: string }

[<CLIMutable>]
type SduiFormNode =
    { Type: string
      Id: string
      Title: string
      Label: string
      Kind: string
      Binding: string
      ArguName: string
      Children: SduiFormNode array
      Options: SduiFormOption array
      Items: SduiFormNode array }

[<CLIMutable>]
type SduiFormBinding =
    { Id: string
      Path: string
      Required: bool }

[<CLIMutable>]
type SduiFormActionAdapter =
    { Type: string
      DuTypeName: string
      UnionCaseName: string
      ArguName: string }

[<CLIMutable>]
type SduiFormAction =
    { ActionId: string
      Type: string
      TargetBindingId: string
      IncludeStateOf: string array
      Adapter: SduiFormActionAdapter }

[<CLIMutable>]
type SduiFormDocument =
    { Schema: string
      Version: string
      DocumentId: string
      Surface: string
      DuTypeName: string
      Nodes: SduiFormNode array
      Actions: SduiFormAction array
      Bindings: SduiFormBinding array
      ArguFormSchema: ArguFormSchema }

[<CLIMutable>]
type DynamicArguMetadata =
    { DynamicArguSchemas: ArguFormSchema array
      DynamicFormDocuments: SduiFormDocument array }

type DynamicTarget =
    | DirectDslTarget of actorAddress: string * formDslId: string
    | ArguTemplateTarget of actorAddress: string * duTypeName: string * unionCaseNames: string list

[<CLIMutable>]
type SubmitArguFieldValue =
    { Name: string
      Values: string array }

[<CLIMutable>]
type SubmitArguForm =
    { DuTypeName: string
      UnionCaseName: string
      Fields: SubmitArguFieldValue array }

[<RequireQualifiedAccess>]
module ArguFormField =
    let text name label arguName =
        { Name = name
          Label = label
          Kind = "text"
          ArguName = arguName
          Options = [||]
          Items = [||] }

    let number name label arguName =
        { text name label arguName with Kind = "number" }

    let boolFlag name label arguName =
        { text name label arguName with Kind = "bool" }

    let boolValue name label arguName =
        { text name label arguName with Kind = "bool-value" }

    let enum name label arguName options =
        { text name label arguName with
            Kind = "enum"
            Options = options |> Array.ofSeq }

    let tuple name label arguName items =
        { text name label arguName with
            Kind = "tuple"
            Items = items |> Array.ofSeq }

    let list name label arguName item =
        { text name label arguName with
            Kind = "list"
            Items = [| item |] }

[<RequireQualifiedAccess>]
module ArguFormSchema =
    let allBindings =
        BindingFlags.Public ||| BindingFlags.NonPublic

    let kebabName (name: string) =
        (if isNull name then "" else name).Trim().ToLowerInvariant().Replace('_', '-')

    let optionElementType (ty: Type) =
        if ty.IsGenericType && ty.GetGenericTypeDefinition() = typedefof<option<_>> then
            Some(ty.GetGenericArguments()[0])
        else
            None

    let listElementType (ty: Type) =
        if ty.IsGenericType && ty.GetGenericTypeDefinition() = typedefof<list<_>> then
            Some(ty.GetGenericArguments()[0])
        else
            None

    let isDuEnum (ty: Type) =
        FSharpType.IsUnion(ty, allBindings)
        && (FSharpType.GetUnionCases(ty, allBindings) |> Array.forall (fun case -> case.GetFields().Length = 0))

    let enumOptions (ty: Type) =
        if ty.IsEnum then
            Enum.GetNames ty |> Array.map kebabName
        elif isDuEnum ty then
            FSharpType.GetUnionCases(ty, allBindings) |> Array.map (fun case -> kebabName case.Name)
        else
            [||]

    let isNumeric (ty: Type) =
        ty = typeof<byte>
        || ty = typeof<sbyte>
        || ty = typeof<int16>
        || ty = typeof<int>
        || ty = typeof<int64>
        || ty = typeof<uint16>
        || ty = typeof<uint32>
        || ty = typeof<uint64>
        || ty = typeof<float32>
        || ty = typeof<float>
        || ty = typeof<decimal>
        || ty = typeof<System.Numerics.BigInteger>

    let fieldLabel (property: PropertyInfo) =
        let name = property.Name

        if String.IsNullOrWhiteSpace name || name.StartsWith("Item", StringComparison.Ordinal) then
            property.PropertyType.Name
        else
            name.Replace('_', ' ')

    let fieldName (property: PropertyInfo) =
        let name = property.Name

        if String.IsNullOrWhiteSpace name || name.StartsWith("Item", StringComparison.Ordinal) then
            "value"
        else
            name

    let scalarField name label arguName (ty: Type) =
        let valueType = optionElementType ty |> Option.defaultValue ty
        let options = enumOptions valueType

        if options.Length > 0 then
            ArguFormField.enum name label arguName options
        elif valueType = typeof<bool> then
            ArguFormField.boolValue name label arguName
        elif isNumeric valueType then
            ArguFormField.number name label arguName
        else
            ArguFormField.text name label arguName

    let itemField (property: PropertyInfo) =
        scalarField (fieldName property) (fieldLabel property) "" property.PropertyType

    let singleField arguName (property: PropertyInfo) (argumentType: ArgumentType) =
        match argumentType, listElementType property.PropertyType with
        | ArgumentType.List, Some elementType ->
            let item = scalarField (fieldName property + "Item") (fieldLabel property) "" elementType
            ArguFormField.list (fieldName property) (fieldLabel property) arguName item
        | _, _ ->
            scalarField (fieldName property) (fieldLabel property) arguName property.PropertyType

    let fieldsFromCase (caseInfo: ArgumentCaseInfo) =
        let fields = caseInfo.UnionCaseInfo.GetFields()
        let arguName = caseInfo.CommandLineNames.Value |> List.tryHead |> Option.defaultValue caseInfo.Name.Value

        match fields with
        | [||] ->
            [| ArguFormField.boolFlag (kebabName caseInfo.UnionCaseInfo.Name) caseInfo.UnionCaseInfo.Name arguName |]
        | [| field |] ->
            [| singleField arguName field caseInfo.ArgumentType |]
        | many ->
            let items = many |> Array.map itemField
            [| ArguFormField.tuple (kebabName caseInfo.UnionCaseInfo.Name + "-args") caseInfo.UnionCaseInfo.Name arguName items |]

    let unionCaseFromArgu (caseInfo: ArgumentCaseInfo) =
        let arguName = caseInfo.CommandLineNames.Value |> List.tryHead |> Option.defaultValue caseInfo.Name.Value

        { Name = caseInfo.UnionCaseInfo.Name
          Label = caseInfo.UnionCaseInfo.Name.Replace('_', ' ')
          ArguName = arguName
          Fields = fieldsFromCase caseInfo }

    let createParserFromTemplateType (templateType: Type) =
        if isNull templateType then
            invalidArg "templateType" "Argu template type is required."

        if not (typeof<IArgParserTemplate>.IsAssignableFrom templateType) then
            invalidArg "templateType" $"Type {templateType.FullName} must implement Argu.IArgParserTemplate."

        let methodInfo =
            typeof<ArgumentParser>.GetMethods(BindingFlags.Public ||| BindingFlags.Static)
            |> Array.find (fun methodInfo ->
                methodInfo.Name = "Create"
                && methodInfo.IsGenericMethodDefinition
                && methodInfo.GetGenericArguments().Length = 1)

        methodInfo.MakeGenericMethod(templateType).Invoke(
            null,
            [| box (None: string option)
               box (None: string option)
               box (None: int option)
               box (None: IExiter option)
               box (Some true) |])
        :?> ArgumentParser

    let fromArgParserTemplateType (templateType: Type) =
        let parser = createParserFromTemplateType templateType

        { Schema = "fskynet-sdui"
          FormMode = "argu-form"
          DuTypeName = templateType.FullName
          UnionCases =
            parser.GetArgumentCases()
            |> Seq.filter (fun caseInfo ->
                not caseInfo.IsHidden.Value
                && caseInfo.ArgumentType <> ArgumentType.SubCommand
                && caseInfo.CommandLineNames.Value.Length > 0)
            |> Seq.map unionCaseFromArgu
            |> Seq.toArray }

    let fromArgParserTemplate<'Template when 'Template :> IArgParserTemplate> () =
        fromArgParserTemplateType typeof<'Template>

    let jsonOptions =
        let options = JsonSerializerOptions(PropertyNamingPolicy = JsonNamingPolicy.CamelCase)
        options.DefaultIgnoreCondition <- JsonIgnoreCondition.WhenWritingNull
        options

    let generateSduiJson schema =
        JsonSerializer.Serialize(schema, jsonOptions)

    let generateSduiJsonFromArgParserTemplateType (templateType: Type) =
        templateType
        |> fromArgParserTemplateType
        |> generateSduiJson

    let tryFindUnionCase name schema =
        schema.UnionCases
        |> Array.tryFind (fun item -> String.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase))

[<RequireQualifiedAccess>]
module SduiFormDocument =
    let nodeDefaults nodeType id =
        { Type = nodeType
          Id = id
          Title = ""
          Label = ""
          Kind = ""
          Binding = ""
          ArguName = ""
          Children = [||]
          Options = [||]
          Items = [||] }

    let bindingId (unionCase: ArguFormUnionCase) (field: ArguFormField) =
        $"{unionCase.Name}.{field.Name}"

    let option value =
        { Value = value
          Label = value }

    let rec nodeFromField unionCase (field: ArguFormField) =
        let id = bindingId unionCase field
        let nodeType =
            match field.Kind with
            | "tuple" -> "Tuple"
            | "list" -> "List"
            | _ -> "Input"

        { nodeDefaults nodeType id with
            Label = field.Label
            Kind =
                match field.Kind with
                | "enum" -> "Select"
                | "bool" -> "Bool"
                | "bool-value" -> "Bool"
                | "number" -> "Number"
                | "tuple" -> "Tuple"
                | "list" -> "List"
                | _ -> "Text"
            Binding = id
            ArguName = field.ArguName
            Options = field.Options |> Array.map option
            Items = field.Items |> Array.map (nodeFromField unionCase) }

    let sectionFromUnionCase (unionCase: ArguFormUnionCase) =
        let actionId = "submit-" + ArguFormSchema.kebabName unionCase.Name
        let fieldNodes = unionCase.Fields |> Array.map (nodeFromField unionCase)
        let sendButton =
            { nodeDefaults "Button" actionId with
                Label = "Send" }

        { nodeDefaults "Section" ("case-" + ArguFormSchema.kebabName unionCase.Name) with
            Title = unionCase.Name
            Children = Array.append fieldNodes [| sendButton |] }

    let bindingFromField unionCase (field: ArguFormField) =
        let id = bindingId unionCase field

        { Id = id
          Path = "$." + id
          Required = field.Kind <> "bool" }

    let actionFromUnionCase (schema: ArguFormSchema) (unionCase: ArguFormUnionCase) =
        { ActionId = "submit-" + ArguFormSchema.kebabName unionCase.Name
          Type = "SubmitForm"
          TargetBindingId = "ptcs.actor-argu.raw"
          IncludeStateOf = unionCase.Fields |> Array.map (bindingId unionCase)
          Adapter =
            { Type = "ArguRaw"
              DuTypeName = schema.DuTypeName
              UnionCaseName = unionCase.Name
              ArguName = unionCase.ArguName } }

    let fromArguFormSchema documentId (schema: ArguFormSchema) =
        let id =
            if String.IsNullOrWhiteSpace documentId then
                schema.DuTypeName
            else
                documentId.Trim()

        { Schema = "fskynet-sdui"
          Version = "0.3"
          DocumentId = id
          Surface = "FormInput"
          DuTypeName = schema.DuTypeName
          Nodes = schema.UnionCases |> Array.map sectionFromUnionCase
          Actions = schema.UnionCases |> Array.map (actionFromUnionCase schema)
          Bindings =
            schema.UnionCases
            |> Array.collect (fun unionCase -> unionCase.Fields |> Array.map (bindingFromField unionCase))
          ArguFormSchema = schema }

    let fromArgParserTemplateType documentId templateType =
        templateType
        |> ArguFormSchema.fromArgParserTemplateType
        |> fromArguFormSchema documentId

    let fromArgParserTemplate<'Template when 'Template :> IArgParserTemplate> documentId =
        fromArgParserTemplateType documentId typeof<'Template>

    let generateJson document =
        JsonSerializer.Serialize(document, ArguFormSchema.jsonOptions)

[<RequireQualifiedAccess>]
module DynamicArguMetadata =
    let empty =
        { DynamicArguSchemas = [||]
          DynamicFormDocuments = [||] }

    let create schemas documents =
        { DynamicArguSchemas = schemas |> Array.ofSeq
          DynamicFormDocuments = documents |> Array.ofSeq }

    let fromDocuments documents =
        let documents = documents |> Array.ofSeq

        { DynamicArguSchemas =
            documents
            |> Array.map _.ArguFormSchema
            |> Array.distinctBy _.DuTypeName
          DynamicFormDocuments = documents }

    let generateJson metadata =
        JsonSerializer.Serialize(metadata, ArguFormSchema.jsonOptions)

[<RequireQualifiedAccess>]
module DynamicTargetKey =
    let normalizeKeyParts keys =
        keys
        |> Seq.choose (fun key ->
            if String.IsNullOrWhiteSpace key then
                None
            else
                Some(key.Trim()))
        |> Seq.toList

    let tryFindDocument documentId (metadata: DynamicArguMetadata) =
        metadata.DynamicFormDocuments
        |> Array.tryFind (fun document -> String.Equals(document.DocumentId, documentId, StringComparison.OrdinalIgnoreCase))

    let tryFindSchema duTypeName (metadata: DynamicArguMetadata) =
        metadata.DynamicArguSchemas
        |> Array.tryFind (fun schema -> String.Equals(schema.DuTypeName, duTypeName, StringComparison.OrdinalIgnoreCase))

    let validateUnionCases requestedCases (schema: ArguFormSchema) =
        let missing =
            requestedCases
            |> List.filter (fun caseName -> ArguFormSchema.tryFindUnionCase caseName schema |> Option.isNone)

        if not missing.IsEmpty then
            let missingText = String.Join(", ", missing)
            Error $"Unknown union case(s) for {schema.DuTypeName}: {missingText}."
        else
            Ok requestedCases

    let tryResolve (metadata: DynamicArguMetadata) keys =
        match normalizeKeyParts keys with
        | actorAddress :: discriminator :: tail ->
            match tryFindDocument discriminator metadata with
            | Some document when tail.IsEmpty ->
                Ok(DirectDslTarget(actorAddress, document.DocumentId))
            | Some document ->
                Error $"Direct DSL target {document.DocumentId} does not accept union-case tail segments."
            | None ->
                match tryFindSchema discriminator metadata with
                | Some schema ->
                    if tail.IsEmpty then
                        Error $"DU target {schema.DuTypeName} requires at least one union case."
                    else
                        validateUnionCases tail schema
                        |> Result.map (fun unionCases -> ArguTemplateTarget(actorAddress, schema.DuTypeName, unionCases))
                | None -> Error $"Unknown Dynamic target discriminator: {discriminator}."
        | _ -> Error "Dynamic target key must contain at least actor address and document/type discriminator."

[<RequireQualifiedAccess>]
module SubmitArguFormCodec =
    let fieldValues (name: string) (submission: SubmitArguForm) =
        submission.Fields
        |> Array.tryFind (fun (field: SubmitArguFieldValue) -> String.Equals(field.Name, name, StringComparison.OrdinalIgnoreCase))
        |> Option.map (fun field -> field.Values |> Array.filter (fun value -> not (String.IsNullOrWhiteSpace value)))
        |> Option.defaultValue [||]

    let quote value =
        let text = if isNull value then "" else string value

        if text = "" then
            "\"\""
        elif text |> Seq.exists Char.IsWhiteSpace || text.Contains("\"", StringComparison.Ordinal) then
            "\"" + text.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\""
        else
            text

    let appendField (parts: ResizeArray<string>) (field: ArguFormField) (values: string array) =
        match field.Kind with
        | "bool" ->
            values
            |> Array.tryHead
            |> Option.map (fun value -> value.Trim().ToLowerInvariant())
            |> Option.iter (fun value ->
                if value = "true" || value = "1" || value = "yes" then
                    parts.Add(field.ArguName))
        | "list" ->
            values
            |> Array.iter (fun value ->
                parts.Add(field.ArguName)
                parts.Add(quote value))
        | "tuple" ->
            if values.Length > 0 then
                parts.Add(field.ArguName)
                values |> Array.iter (quote >> parts.Add)
        | _ ->
            values
            |> Array.tryHead
            |> Option.iter (fun value ->
                parts.Add(field.ArguName)
                parts.Add(quote value))

    let buildRawArgu (schema: ArguFormSchema) (submission: SubmitArguForm) =
        if not (String.Equals(schema.DuTypeName, submission.DuTypeName, StringComparison.Ordinal)) then
            invalidArg "submission" $"DU type mismatch: expected {schema.DuTypeName}, got {submission.DuTypeName}."

        let unionCase =
            ArguFormSchema.tryFindUnionCase submission.UnionCaseName schema
            |> Option.defaultWith (fun () -> invalidArg "submission" $"Unknown union case: {submission.UnionCaseName}.")

        let parts = ResizeArray<string>()

        unionCase.Fields
        |> Array.iter (fun field ->
            let values = fieldValues field.Name submission
            appendField parts field values)

        String.Join(" ", parts)
