namespace PulseTrade.Comm.Spa.Dynamic.Server

open System
open System.Text.Json
open System.Text.Json.Serialization

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
    let sampleDuTypeName = "PulseTrade.Comm.Spa.Dynamic.SampleArgu"

    let sample () =
        let tupleItems =
            [| ArguFormField.text "symbol" "1. symbol" ""
               ArguFormField.number "quantity" "2. quantity" "" |]

        { Schema = "fskynet-sdui"
          FormMode = "argu-form"
          DuTypeName = sampleDuTypeName
          UnionCases =
            [| { Name = "Say"
                 Label = "Say"
                 ArguName = "--say"
                 Fields = [| ArguFormField.text "text" "Text" "--say" |] }
               { Name = "SetCount"
                 Label = "Set Count"
                 ArguName = "--set-count"
                 Fields = [| ArguFormField.number "count" "Count" "--set-count" |] }
               { Name = "Mode"
                 Label = "Mode"
                 ArguName = "--mode"
                 Fields = [| ArguFormField.enum "mode" "Mode" "--mode" [| "Fast"; "Safe"; "Audit" |] |] }
               { Name = "At"
                 Label = "Tuple At"
                 ArguName = "--at"
                 Fields = [| ArguFormField.tuple "at" "At" "--at" tupleItems |] }
               { Name = "Tag"
                 Label = "Tag List"
                 ArguName = "--tag"
                 Fields = [| ArguFormField.list "tag" "Tags" "--tag" (ArguFormField.text "tagItem" "Tag" "") |] }
               { Name = "Verbose"
                 Label = "Verbose"
                 ArguName = "--verbose"
                 Fields = [| ArguFormField.boolFlag "verbose" "Verbose" "--verbose" |] } |] }

    let jsonOptions =
        let options = JsonSerializerOptions(PropertyNamingPolicy = JsonNamingPolicy.CamelCase)
        options.DefaultIgnoreCondition <- JsonIgnoreCondition.WhenWritingNull
        options

    let generateSduiJson schema =
        JsonSerializer.Serialize(schema, jsonOptions)

    let tryFindUnionCase name schema =
        schema.UnionCases
        |> Array.tryFind (fun item -> String.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase))

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
