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
      DefaultValues: string array
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
    | ArguTemplateTarget of actorAddress: string * templateKey: string * canonicalArgString: string
    | LegacyArguCaseTarget of actorAddress: string * duTypeName: string * unionCaseNames: string list

type DynamicArguAliasBinding =
    { CaseAliases: Map<string, string>
      FieldAliases: Map<string * string, string>
      OptionAliases: Map<string * string, string> }

type DynamicArguTemplateRegistration =
    { TemplateKey: string
      DuTypeName: string
      TemplateType: Type
      Aliases: DynamicArguAliasBinding
      DefaultArgString: string option }

[<CLIMutable>]
type ParsedArguValue =
    { FieldName: string
      Values: string array }

[<CLIMutable>]
type ParsedArguCase =
    { CaseName: string
      ArguName: string
      Values: ParsedArguValue array }

[<CLIMutable>]
type ParsedArguSubcommand =
    { CaseName: string
      CommandToken: string
      Cases: ParsedArguCase array }

[<CLIMutable>]
type ParsedArguTarget =
    { ActorAddress: string
      TemplateKey: string
      CanonicalArgString: string
      RootCases: ParsedArguCase array
      TailSubcommands: ParsedArguSubcommand array }

[<CLIMutable>]
type DynamicArguResolveTargetRequest =
    { Keys: string array }

[<CLIMutable>]
type DynamicArguResolveTargetReply =
    { Ok: bool
      Error: string
      ActorAddress: string
      TemplateKey: string
      CanonicalArgString: string
      Document: SduiFormDocument }

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

        if String.IsNullOrWhiteSpace name then
            property.PropertyType.Name
        elif name.StartsWith("Item", StringComparison.Ordinal) then
            "Value " + name.Substring("Item".Length)
        else
            name.Replace('_', ' ')

    let fieldName (property: PropertyInfo) =
        let name = property.Name

        if String.IsNullOrWhiteSpace name then
            "value"
        elif name.StartsWith("Item", StringComparison.Ordinal) then
            "value" + name.Substring("Item".Length)
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
          DefaultValues = [||]
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
            Title =
                if String.IsNullOrWhiteSpace unionCase.Label then
                    unionCase.Name
                else
                    unionCase.Label
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
module DynamicArguAliasBinding =
    let empty =
        { CaseAliases = Map.empty
          FieldAliases = Map.empty
          OptionAliases = Map.empty }

    let caseLabel caseName aliases =
        aliases.CaseAliases
        |> Map.tryFind caseName
        |> Option.defaultValue caseName

    let fieldLabel caseName fieldName fallback aliases =
        aliases.FieldAliases
        |> Map.tryFind (caseName, fieldName)
        |> Option.defaultValue fallback

    let optionLabel caseName optionValue aliases =
        aliases.OptionAliases
        |> Map.tryFind (caseName, optionValue)
        |> Option.defaultValue optionValue

[<RequireQualifiedAccess>]
module DynamicFormDsl =
    let rec applyAliasesToField caseName aliases (field: ArguFormField) =
        { field with
            Label = DynamicArguAliasBinding.fieldLabel caseName field.Name field.Label aliases
            Items = field.Items |> Array.map (applyAliasesToField caseName aliases) }

    let applyAliasesToUnionCase aliases (unionCase: ArguFormUnionCase) =
        { unionCase with
            Label = DynamicArguAliasBinding.caseLabel unionCase.Name aliases
            Fields = unionCase.Fields |> Array.map (applyAliasesToField unionCase.Name aliases) }

    let applyAliasesToSchema aliases (schema: ArguFormSchema) =
        { schema with
            UnionCases = schema.UnionCases |> Array.map (applyAliasesToUnionCase aliases) }

    let caseNameFromBinding (binding: string) =
        if String.IsNullOrWhiteSpace binding then
            ""
        else
            let index = binding.IndexOf(".", StringComparison.Ordinal)

            if index <= 0 then
                ""
            else
                binding.Substring(0, index)

    let rec applyOptionAliasesToNode aliases (node: SduiFormNode) =
        let caseName = caseNameFromBinding node.Binding

        { node with
            Options =
                node.Options
                |> Array.map (fun option ->
                    { option with
                        Label = DynamicArguAliasBinding.optionLabel caseName option.Value aliases })
            Children = node.Children |> Array.map (applyOptionAliasesToNode aliases)
            Items = node.Items |> Array.map (applyOptionAliasesToNode aliases) }

    let applyOptionAliasesToDocument aliases (document: SduiFormDocument) =
        { document with
            Nodes = document.Nodes |> Array.map (applyOptionAliasesToNode aliases) }

    let defaultsFromParsedTarget (target: ParsedArguTarget) =
        target.RootCases
        |> Array.collect (fun parsedCase ->
            parsedCase.Values
            |> Array.map (fun parsedValue -> $"{parsedCase.CaseName}.{parsedValue.FieldName}", parsedValue.Values))
        |> Map.ofArray

    let rec applyDefaultsToNode defaults (node: SduiFormNode) =
        let defaultValues =
            if String.IsNullOrWhiteSpace node.Binding then
                node.DefaultValues
            else
                defaults
                |> Map.tryFind node.Binding
                |> Option.defaultValue node.DefaultValues

        { node with
            DefaultValues = defaultValues
            Children = node.Children |> Array.map (applyDefaultsToNode defaults)
            Items = node.Items |> Array.map (applyDefaultsToNode defaults) }

    let applyParsedDefaultsToDocument parsedTarget (document: SduiFormDocument) =
        let defaults = defaultsFromParsedTarget parsedTarget

        { document with
            Nodes = document.Nodes |> Array.map (applyDefaultsToNode defaults) }

    let filterSchemaByParsedRootCases (parsedTarget: ParsedArguTarget) (schema: ArguFormSchema) =
        let unionCasesByName =
            schema.UnionCases
            |> Array.map (fun unionCase -> unionCase.Name, unionCase)
            |> Map.ofArray

        { schema with
            UnionCases =
                parsedTarget.RootCases
                |> Array.choose (fun parsedCase -> unionCasesByName |> Map.tryFind parsedCase.CaseName) }

    let fromArguFormSchemaWithAliases documentId aliases schema =
        schema
        |> applyAliasesToSchema aliases
        |> SduiFormDocument.fromArguFormSchema documentId
        |> applyOptionAliasesToDocument aliases

    let fromParsedArguTarget documentId aliases schema parsedTarget =
        schema
        |> filterSchemaByParsedRootCases parsedTarget
        |> fromArguFormSchemaWithAliases documentId aliases
        |> applyParsedDefaultsToDocument parsedTarget

[<RequireQualifiedAccess>]
module DynamicArguTemplateRegistration =
    let create (templateKey: string) (templateType: Type) (aliases: DynamicArguAliasBinding) (defaultArgString: string option) =
        if String.IsNullOrWhiteSpace templateKey then
            invalidArg "templateKey" "Template key is required."

        if isNull templateType || not (typeof<IArgParserTemplate>.IsAssignableFrom templateType) then
            invalidArg "templateType" "Template type must implement Argu.IArgParserTemplate."

        { TemplateKey = templateKey.Trim()
          DuTypeName = templateType.FullName
          TemplateType = templateType
          Aliases = aliases
          DefaultArgString = defaultArgString }

    let fromTemplateType (templateType: Type) aliases defaultArgString =
        create templateType.FullName templateType aliases defaultArgString

    let fromTemplate<'Template when 'Template :> IArgParserTemplate> aliases defaultArgString =
        fromTemplateType typeof<'Template> aliases defaultArgString

    let schema (registration: DynamicArguTemplateRegistration) =
        registration.TemplateType
        |> ArguFormSchema.fromArgParserTemplateType
        |> DynamicFormDsl.applyAliasesToSchema registration.Aliases

    let metadata registrations =
        let schemas =
            registrations
            |> Seq.map schema
            |> Seq.toArray

        DynamicArguMetadata.create schemas [||]

[<RequireQualifiedAccess>]
module DynamicCommandLine =
    let split (text: string) =
        let text = if isNull text then "" else text
        let tokens = ResizeArray<string>()
        let buffer = System.Text.StringBuilder()
        let mutable inQuote = false
        let mutable escaped = false

        let flush () =
            if buffer.Length > 0 then
                tokens.Add(buffer.ToString())
                buffer.Clear() |> ignore

        for ch in text do
            if escaped then
                buffer.Append(ch) |> ignore
                escaped <- false
            elif ch = '\\' && inQuote then
                escaped <- true
            elif ch = '"' then
                inQuote <- not inQuote
            elif Char.IsWhiteSpace ch && not inQuote then
                flush ()
            else
                buffer.Append(ch) |> ignore

        if escaped then
            buffer.Append('\\') |> ignore

        if inQuote then
            invalidArg "text" "Unclosed quote in canonical Argu string."

        flush ()
        tokens.ToArray()

    let quote value =
        let text = if isNull value then "" else string value

        if text = "" then
            "\"\""
        elif text |> Seq.exists Char.IsWhiteSpace || text.Contains("\"", StringComparison.Ordinal) then
            "\"" + text.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\""
        else
            text

    let join tokens =
        tokens
        |> Seq.map quote
        |> String.concat " "

[<RequireQualifiedAccess>]
module DynamicArgStringTarget =
    let normalizeKeyParts keys =
        keys
        |> Seq.choose (fun key ->
            if String.IsNullOrWhiteSpace key then
                None
            else
                Some(key.Trim()))
        |> Seq.toList

    let validateByParser (registration: DynamicArguTemplateRegistration) (argv: string array) =
        let parser = ArguFormSchema.createParserFromTemplateType registration.TemplateType

        try
            parser.Accept {
                new IArgumentParserVisitor<obj list> with
                    member _.Visit<'Template when 'Template :> IArgParserTemplate>(typedParser: ArgumentParser<'Template>) =
                        let results =
                            typedParser.ParseCommandLine(
                                inputs = argv,
                                ignoreMissing = true,
                                ignoreUnrecognized = false,
                                raiseOnUsage = true)

                        results.GetAllResults() |> List.map box
            }
            |> Ok
        with ex ->
            Error ex.Message

    let commandNames (caseInfo: ArgumentCaseInfo) =
        caseInfo.CommandLineNames.Value |> List.toArray

    let canonicalName (caseInfo: ArgumentCaseInfo) =
        caseInfo.UnionCaseInfo.Name

    let caseByCommandName commandName (cases: ArgumentCaseInfo array) =
        cases
        |> Array.tryFind (fun caseInfo ->
            commandNames caseInfo
            |> Array.exists (fun name -> String.Equals(name, commandName, StringComparison.OrdinalIgnoreCase)))

    let readCaseValues startIndex stopTokens argv =
        let values = ResizeArray<string>()
        let mutable index = startIndex

        while index < Array.length argv && not (Set.contains argv[index] stopTokens) do
            values.Add(argv[index])
            index <- index + 1

        values.ToArray(), index

    let parsedValuesFromTokens (caseInfo: ArgumentCaseInfo) values =
        let fields = caseInfo.UnionCaseInfo.GetFields()

        match fields with
        | [||] -> [||]
        | [| field |] ->
            [| { FieldName = ArguFormSchema.fieldName field
                 Values = values } |]
        | many ->
            many
            |> Array.mapi (fun index field ->
                { FieldName = ArguFormSchema.fieldName field
                  Values =
                    if index < Array.length values then
                        [| values[index] |]
                    else
                        [||] })

    let parsedCaseFromTokens (caseInfo: ArgumentCaseInfo) values =
        { CaseName = canonicalName caseInfo
          ArguName = commandNames caseInfo |> Array.tryHead |> Option.defaultValue caseInfo.Name.Value
          Values =
            match values with
            | [||] when caseInfo.UnionCaseInfo.GetFields().Length = 0 -> [||]
            | _ -> parsedValuesFromTokens caseInfo values }

    let scanCases (cases: ArgumentCaseInfo array) argv =
        let commandTokenSet =
            cases
            |> Array.collect commandNames
            |> Set.ofArray

        let stopTokens = commandTokenSet

        let parsed = ResizeArray<ParsedArguCase>()
        let mutable index = 0

        while index < Array.length argv do
            let token = argv[index]

            match caseByCommandName token cases with
            | None ->
                index <- index + 1
            | Some caseInfo ->
                let values, nextIndex = readCaseValues (index + 1) stopTokens argv
                parsed.Add(parsedCaseFromTokens caseInfo values)
                index <- nextIndex

        parsed.ToArray()

    let subcommandTemplateType (caseInfo: ArgumentCaseInfo) =
        match caseInfo.UnionCaseInfo.GetFields() with
        | [| field |]
            when field.PropertyType.IsGenericType
                 && field.PropertyType.GetGenericTypeDefinition() = typedefof<ParseResults<_>> ->
            Some(field.PropertyType.GetGenericArguments()[0])
        | _ -> None

    let scan (registration: DynamicArguTemplateRegistration) actorAddress canonicalArgString =
        let argv = DynamicCommandLine.split canonicalArgString
        let parser = ArguFormSchema.createParserFromTemplateType registration.TemplateType
        let caseInfos = parser.GetArgumentCases() |> Seq.toArray
        let rootCases = caseInfos |> Array.filter (fun caseInfo -> caseInfo.ArgumentType <> ArgumentType.SubCommand)
        let subcommands = caseInfos |> Array.filter (fun caseInfo -> caseInfo.ArgumentType = ArgumentType.SubCommand)
        let mutable subcommandStart = Array.length argv
        let mutable subcommandInfo: ArgumentCaseInfo option = None

        argv
        |> Array.iteri (fun index token ->
            if subcommandStart = Array.length argv then
                match caseByCommandName token subcommands with
                | Some caseInfo ->
                    subcommandStart <- index
                    subcommandInfo <- Some caseInfo
                | None -> ())

        let rootArgv = argv |> Array.take subcommandStart
        let rootParsed = scanCases rootCases rootArgv

        let tailSubcommands =
            match subcommandInfo with
            | None -> [||]
            | Some subcommand ->
                let nestedArgv = argv |> Array.skip (subcommandStart + 1)
                let nestedCases =
                    match subcommandTemplateType subcommand with
                    | None -> [||]
                    | Some nestedType ->
                        let nestedParser = ArguFormSchema.createParserFromTemplateType nestedType
                        nestedParser.GetArgumentCases()
                        |> Seq.filter (fun caseInfo -> caseInfo.ArgumentType <> ArgumentType.SubCommand)
                        |> Seq.toArray
                        |> fun cases -> scanCases cases nestedArgv

                [| { CaseName = canonicalName subcommand
                     CommandToken = commandNames subcommand |> Array.tryHead |> Option.defaultValue (ArguFormSchema.kebabName subcommand.Name.Value)
                     Cases = nestedCases } |]

        { ActorAddress = actorAddress
          TemplateKey = registration.TemplateKey
          CanonicalArgString = canonicalArgString
          RootCases = rootParsed
          TailSubcommands = tailSubcommands }

    let buildFormDocument documentId (registration: DynamicArguTemplateRegistration) parsedTarget =
        let schema = ArguFormSchema.fromArgParserTemplateType registration.TemplateType

        DynamicFormDsl.fromParsedArguTarget documentId registration.Aliases schema parsedTarget

    let tryResolve (registrations: DynamicArguTemplateRegistration seq) keys =
        let registrations = registrations |> Seq.toArray

        match normalizeKeyParts keys with
        | actorAddress :: templateKey :: canonicalArgString :: [] ->
            match registrations |> Array.tryFind (fun item -> String.Equals(item.TemplateKey, templateKey, StringComparison.OrdinalIgnoreCase) || String.Equals(item.DuTypeName, templateKey, StringComparison.OrdinalIgnoreCase)) with
            | None -> Error $"Unknown Dynamic Argu template: {templateKey}."
            | Some registration ->
                let argv =
                    try
                        DynamicCommandLine.split canonicalArgString |> Ok
                    with ex ->
                        Error ex.Message

                match argv with
                | Error error -> Error error
                | Ok argv ->
                    match validateByParser registration argv with
                    | Error error -> Error $"Argu parse failed for {registration.TemplateKey}: {error}"
                    | Ok _ -> Ok(ArguTemplateTarget(actorAddress, registration.TemplateKey, canonicalArgString))
        | actorAddress :: templateKey :: [] ->
            Error $"Dynamic Argu target {templateKey} requires a canonical arg string."
        | _ -> Error "Dynamic Argu target key must be [ actorAddress; duTypeOrTemplateKey; canonicalArgString ]."

    let buildRawArgu (target: ParsedArguTarget) =
        let parts = ResizeArray<string>()

        let appendCase (parsed: ParsedArguCase) =
            parts.Add(parsed.ArguName)

            parsed.Values
            |> Array.collect _.Values
            |> Array.iter parts.Add

        target.RootCases |> Array.iter appendCase

        target.TailSubcommands
        |> Array.iter (fun subcommand ->
            parts.Add(subcommand.CommandToken)
            subcommand.Cases |> Array.iter appendCase)

        parts
        |> Seq.map DynamicCommandLine.quote
        |> String.concat " "

[<RequireQualifiedAccess>]
module DynamicArguResolveEndpoint =
    let path = "/client-extensions/dynamic/argu/resolve-target"

    let ok actorAddress templateKey canonicalArgString document =
        { Ok = true
          Error = ""
          ActorAddress = actorAddress
          TemplateKey = templateKey
          CanonicalArgString = canonicalArgString
          Document = document }

    let error message =
        { Ok = false
          Error = if String.IsNullOrWhiteSpace message then "Dynamic Argu target resolution failed." else message
          ActorAddress = ""
          TemplateKey = ""
          CanonicalArgString = ""
          Document = Unchecked.defaultof<SduiFormDocument> }

    let findRegistration (templateKey: string) (registrations: DynamicArguTemplateRegistration seq) =
        registrations
        |> Seq.tryFind (fun item ->
            String.Equals(item.TemplateKey, templateKey, StringComparison.OrdinalIgnoreCase)
            || String.Equals(item.DuTypeName, templateKey, StringComparison.OrdinalIgnoreCase))

    let resolve (registrations: DynamicArguTemplateRegistration seq) keys =
        let registrations = registrations |> Seq.toArray

        match DynamicArgStringTarget.tryResolve registrations keys with
        | Error message -> Error message
        | Ok(DirectDslTarget _) -> Error "Direct DSL target does not require Dynamic Argu backend resolution."
        | Ok(LegacyArguCaseTarget _) -> Error "Legacy Dynamic Argu target key is not supported by the backend resolver."
        | Ok(ArguTemplateTarget(actorAddress, templateKey, canonicalArgString)) ->
            match findRegistration templateKey registrations with
            | None -> Error $"Unknown Dynamic Argu template: {templateKey}."
            | Some registration ->
                let parsed = DynamicArgStringTarget.scan registration actorAddress canonicalArgString
                let document = DynamicArgStringTarget.buildFormDocument templateKey registration parsed
                Ok(ok actorAddress registration.TemplateKey canonicalArgString document)

    let handle (registrations: DynamicArguTemplateRegistration seq) (body: string) =
        try
            let request: DynamicArguResolveTargetRequest =
                JsonSerializer.Deserialize<DynamicArguResolveTargetRequest>(body, ArguFormSchema.jsonOptions)

            if isNull (box request) then
                error "Invalid Dynamic Argu resolve request."
            else
                match resolve registrations request.Keys with
                | Ok reply -> reply
                | Error message -> error message
        with ex ->
            error ex.Message
        |> fun reply -> JsonSerializer.Serialize(reply, ArguFormSchema.jsonOptions)

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
                        |> Result.map (fun unionCases -> LegacyArguCaseTarget(actorAddress, schema.DuTypeName, unionCases))
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
