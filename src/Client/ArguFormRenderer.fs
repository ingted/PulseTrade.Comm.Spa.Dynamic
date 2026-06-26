namespace PulseTrade.Comm.Spa.Dynamic.Client

open System
open WebSharper
open WebSharper.JavaScript
open WebSharper.JavaScript.Dom

[<JavaScript>]
type ArguFormFieldDto =
    { name: string
      label: string
      kind: string
      arguName: string
      options: string[]
      items: ArguFormFieldDto[] }

[<JavaScript>]
type ArguFormUnionCaseDto =
    { name: string
      label: string
      arguName: string
      fields: ArguFormFieldDto[] }

[<JavaScript>]
type ArguFormSchemaDto =
    { schema: string
      formMode: string
      duTypeName: string
      unionCases: ArguFormUnionCaseDto[] }

[<JavaScript>]
type SduiFormOptionDto =
    { value: string
      label: string }

[<JavaScript>]
type SduiFormNodeDto =
    { ``type``: string
      id: string
      title: string
      label: string
      kind: string
      binding: string
      arguName: string
      defaultValues: string[]
      children: SduiFormNodeDto[]
      options: SduiFormOptionDto[]
      items: SduiFormNodeDto[] }

[<JavaScript>]
type SduiFormDocumentDto =
    { schema: string
      version: string
      documentId: string
      surface: string
      duTypeName: string
      nodes: SduiFormNodeDto[]
      arguFormSchema: ArguFormSchemaDto }

[<JavaScript>]
type DynamicArguMetadataDto =
    { dynamicArguSchemas: ArguFormSchemaDto[]
      dynamicFormDocuments: SduiFormDocumentDto[] }

[<JavaScript>]
type ClientExtensionRegistrationDto =
    { extensionId: string
      displayName: string
      metadataJson: string
      scriptUrls: string[]
      appendPageShapes: obj[] }

[<JavaScript>]
type AddKeyContextDto =
    { shape: string
      defaultKey: string
      submitKey: obj -> unit }

[<JavaScript>]
type AppendInputContextDto =
    { shape: string
      selectedKeyJson: string
      selectedKeys: string[]
      keyParts: string[]
      actorAddress: string
      duTypeName: string
      unionCaseNames: string[]
      submit: obj -> unit }

[<JavaScript>]
type KeySubmitPayloadDto =
    { keys: string[] }

[<JavaScript>]
type AppendSubmitPayloadDto =
    { rawArgu: string
      duTypeName: string
      unionCaseName: string }

[<JavaScript>]
type ResolveTargetRequestDto =
    { keys: string[] }

[<JavaScript>]
type ResolveTargetReplyDto =
    { ok: bool
      error: string
      actorAddress: string
      templateKey: string
      canonicalArgString: string
      document: SduiFormDocumentDto }

[<JavaScript>]
module ClientRawArguCodec =
    let asText (value: string) =
        if isNull value then "" else value

    let arrayOrEmpty (values: 'T[]) =
        if isNull (box values) then [||] else values

    let quoteArg value =
        let text = asText value

        if text.Length = 0 then
            "\"\""
        elif text |> Seq.exists Char.IsWhiteSpace || text.Contains("\"") then
            "\"" + text.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\""
        else
            text

    let appendFieldParts (parts: ResizeArray<string>) (field: ArguFormFieldDto) (values: string[]) =
        let values =
            values
            |> arrayOrEmpty
            |> Array.map asText
            |> Array.map _.Trim()
            |> Array.filter (fun value -> value.Length > 0)

        match asText field.kind with
        | "bool" ->
            values
            |> Array.tryHead
            |> Option.map _.ToLower()
            |> Option.iter (fun value ->
                if value = "true" || value = "1" || value = "yes" then
                    parts.Add(asText field.arguName))
        | "bool-value" ->
            values
            |> Array.tryHead
            |> Option.iter (fun value ->
                parts.Add(asText field.arguName)
                parts.Add(quoteArg value))
        | "list" ->
            values
            |> Array.iter (fun value ->
                parts.Add(asText field.arguName)
                parts.Add(quoteArg value))
        | "tuple" ->
            if values.Length > 0 then
                parts.Add(asText field.arguName)
                values |> Array.iter (quoteArg >> parts.Add)
        | _ ->
            values
            |> Array.tryHead
            |> Option.iter (fun value ->
                parts.Add(asText field.arguName)
                parts.Add(quoteArg value))

    let buildRawArguFromValues (fields: (ArguFormFieldDto * string[])[]) =
        let parts = ResizeArray<string>()

        fields
        |> arrayOrEmpty
        |> Array.iter (fun (field, values) -> appendFieldParts parts field values)

        String.Join(" ", parts)

[<JavaScript>]
module ArguFormRenderer =
    let doc = JS.Document
    let mutable schemas: ArguFormSchemaDto[] = [||]
    let mutable documents: SduiFormDocumentDto[] = [||]

    let asText (value: string) =
        if isNull value || JS.TypeOf(box value) = JS.Kind.Undefined then "" else value

    let isBlank value =
        String.IsNullOrWhiteSpace(asText value)

    let arrayOrEmpty (values: 'T[]) =
        if isNull (box values) || JS.TypeOf(box values) = JS.Kind.Undefined then [||] else values

    let element tag className textValue =
        let node = doc.CreateElement tag

        if not (isBlank className) then
            node.ClassName <- className

        if not (isNull textValue) then
            node.TextContent <- textValue

        node

    let setTestId id (node: #Element) =
        if not (isBlank id) then
            node.SetAttribute("data-testid", id)

        node

    let append (parent: Node) (children: Node[]) =
        children |> Array.iter (fun child -> parent.AppendChild child |> ignore)
        parent

    let input inputType className testId =
        let node = doc.CreateElement("input") :?> HTMLInputElement
        node.SetAttribute("type", inputType)
        node.ClassName <- className
        setTestId testId node |> ignore
        node

    let elementValue (node: #Element) =
        (node |> As<HTMLInputElement>).Value

    let setElementValue (node: #Element) value =
        JS.Inline("$0.value = $1", node, value)

    let queryInputs (root: #Element) selector =
        let nodes = root.QuerySelectorAll(selector)

        [| for index in 0 .. int nodes.Length - 1 do
               yield nodes.Item(index) |> As<HTMLInputElement> |]

    let button className testId label =
        let node = element "button" className label
        node.SetAttribute("type", "button")
        setTestId testId node |> ignore
        node

    let label text =
        element "label" "dynamic-argu-label" text

    let decodeJson<'T> text =
        JSON.Parse(asText text) |> As<'T>

    let tryDecodeJson<'T> text =
        try
            Some(decodeJson<'T> text)
        with _ ->
            None

    let errorMessage (error: obj) =
        if isNull error then
            "unknown error"
        else
            string error

    let postJson<'TRequest, 'TReply> url (body: 'TRequest) (onOk: 'TReply -> unit) onError =
        let headers = Headers()
        headers.Set("Content-Type", "application/json")

        let options = RequestOptions()
        options.Method <- "POST"
        options.Headers <- headers
        options.Body <- JSON.Stringify(body)

        let promise =
            JS.Window.Fetch(url, options)
                .Then<unit>(System.Func<Response, Promise<unit>>(fun response ->
                    response.Text()
                        .Then<unit>(System.Func<string, unit>(fun responseBody ->
                            if response.Ok then
                                let text = if isBlank responseBody then "{}" else responseBody
                                onOk (decodeJson<'TReply> text)
                            else
                                onError (if isBlank responseBody then $"POST {url} {response.Status}" else responseBody)))))

        promise.Catch<unit>(System.Func<obj, unit>(fun error -> onError (errorMessage error))) |> ignore

    let keyPartsFromJson text =
        match tryDecodeJson<string[]> text with
        | Some values -> values |> arrayOrEmpty |> Array.map asText
        | None ->
            match tryDecodeJson<string> text with
            | Some value when not (isBlank value) -> [| value |]
            | _ -> [||]

    let upsertSchema (schema: ArguFormSchemaDto) =
        if not (isNull (box schema)) && not (isBlank schema.duTypeName) then
            schemas <-
                Array.append
                    (schemas |> Array.filter (fun existing -> asText existing.duTypeName <> asText schema.duTypeName))
                    [| schema |]

    let upsertDocument (document: SduiFormDocumentDto) =
        if not (isNull (box document)) && not (isBlank document.documentId) then
            documents <-
                Array.append
                    (documents |> Array.filter (fun existing -> asText existing.documentId <> asText document.documentId))
                    [| document |]

            if not (isNull (box document.arguFormSchema)) then
                upsertSchema document.arguFormSchema

    let loadSchemasFromManifest () =
        let node = doc.GetElementById("ptc-comm-client-extensions")

        if not (isNull node) && not (isBlank node.TextContent) then
            match tryDecodeJson<ClientExtensionRegistrationDto[]> node.TextContent with
            | None -> ()
            | Some extensions ->
                extensions
                |> arrayOrEmpty
                |> Array.iter (fun extension ->
                    if not (isNull (box extension)) && not (isBlank extension.metadataJson) then
                        match tryDecodeJson<DynamicArguMetadataDto> extension.metadataJson with
                        | Some metadata ->
                            metadata.dynamicArguSchemas
                            |> arrayOrEmpty
                            |> Array.iter upsertSchema

                            metadata.dynamicFormDocuments
                            |> arrayOrEmpty
                            |> Array.iter upsertDocument
                        | None -> ())

    let tryFindSchema (duTypeName: string) =
        schemas
        |> Array.tryFind (fun schema -> asText schema.duTypeName = asText duTypeName)

    let tryFindDocument documentId =
        documents
        |> Array.tryFind (fun document -> asText document.documentId = asText documentId)

    let quoteArg value =
        ClientRawArguCodec.quoteArg value

    let appendFieldParts (parts: ResizeArray<string>) (field: ArguFormFieldDto) (values: string[]) =
        ClientRawArguCodec.appendFieldParts parts field values

    let errorNode message =
        let root = element "div" "dynamic-argu-error" message |> setTestId "dynamic-argu-error"
        root.SetAttribute("role", "alert")
        root

    let schemaKeys () =
        schemas
        |> Array.map _.duTypeName
        |> Array.filter (not << isBlank)
        |> Array.sort

    let documentKeys () =
        documents
        |> Array.map _.documentId
        |> Array.filter (not << isBlank)
        |> Array.sort

    let renderAddKey (ctx: obj) =
        let context = ctx |> As<AddKeyContextDto>
        let shape = asText context.shape |> _.ToLower()

        if shape <> "actor-dynamic" && shape <> "actor-argu" then
            None
        else
            let formDocuments = documentKeys ()
            let arguSchemas = schemaKeys ()
            let keys =
                Array.concat [ formDocuments; arguSchemas ]
                |> Array.distinct
                |> Array.sort
            let defaultKeyParts = keyPartsFromJson context.defaultKey
            let actorAddress =
                defaultKeyParts
                |> Array.tryHead
                |> Option.defaultValue ""

            if keys.Length = 0 then
                Some(errorNode "No Dynamic Argu schemas are registered." :> Node)
            elif isBlank actorAddress then
                Some(errorNode "Dynamic Argu default key must include actor address as the first JSON list item." :> Node)
            else
                let root = element "div" "dynamic-argu-add-key" null |> setTestId "dynamic-argu-add-key"
                let actor = element "code" "dynamic-argu-actor-address" actorAddress |> setTestId "dynamic-argu-key-actor"
                let mutable selectedTypeName = keys[0]
                let typeNode =
                    if keys.Length = 1 then
                        let node = element "code" "dynamic-argu-du-type" selectedTypeName |> setTestId "dynamic-argu-key-du-type"
                        node :> Node
                    else
                        let typeSelect = doc.CreateElement("select") :?> HTMLSelectElement
                        typeSelect.ClassName <- "dynamic-argu-du-type"
                        setTestId "dynamic-argu-key-du-type" typeSelect |> ignore

                        keys
                        |> Array.iter (fun key ->
                            let option = doc.CreateElement("option")
                            option.SetAttribute("value", key)
                            option.TextContent <- key
                            typeSelect.AppendChild option |> ignore)

                        typeSelect :> Node

                let targetConfig = element "div" "dynamic-argu-target-config" null |> setTestId "dynamic-argu-key-target-config"
                let argInput = doc.CreateElement("textarea") :?> HTMLTextAreaElement
                argInput.ClassName <- "dynamic-argu-canonical-arg-string"
                argInput.SetAttribute("rows", "3")
                argInput.SetAttribute("placeholder", "--say \"hello\"")
                setTestId "dynamic-argu-key-canonical-arg-string" argInput |> ignore

                let defaultArgString =
                    if defaultKeyParts.Length > 2 then
                        defaultKeyParts[2]
                    else
                        ""

                argInput.Value <- defaultArgString

                let renderTargetConfig () =
                    targetConfig.TextContent <- ""
                    let typeName = selectedTypeName

                    match tryFindDocument typeName with
                    | Some _ ->
                        targetConfig.AppendChild(element "div" "dynamic-argu-target-note" "Direct DSL document target; no canonical Argu string required.") |> ignore
                    | None ->
                        match tryFindSchema typeName with
                        | None -> targetConfig.AppendChild(errorNode ("Dynamic Argu schema not found for DU type: " + typeName)) |> ignore
                        | Some _ ->
                            let label = element "label" "dynamic-argu-label" "Canonical Argu string"
                            label.SetAttribute("for", "dynamic-argu-key-canonical-arg-string")
                            targetConfig.AppendChild label |> ignore
                            targetConfig.AppendChild argInput |> ignore

                match typeNode with
                | :? HTMLSelectElement as typeSelect ->
                    typeSelect.AddEventListener("change", fun () ->
                        selectedTypeName <- elementValue typeSelect
                        renderTargetConfig ())
                | _ -> ()

                renderTargetConfig ()

                let submit = button "dynamic-argu-key-submit" "dynamic-argu-key-submit" "Add target"
                submit.AddEventListener(
                    "click",
                    fun () ->
                        let keyTail =
                            match tryFindDocument selectedTypeName with
                            | Some _ -> [||]
                            | None ->
                                let canonicalArgString = argInput.Value.Trim()

                                if isBlank canonicalArgString then
                                    argInput.Focus()
                                    [||]
                                else
                                    [| canonicalArgString |]

                        if Option.isSome (tryFindDocument selectedTypeName) || keyTail.Length > 0 then
                            let payload: KeySubmitPayloadDto =
                                { keys =
                                    [| yield actorAddress
                                       yield selectedTypeName
                                       yield! keyTail |] }

                            context.submitKey(box payload))

                append root [| actor :> Node; typeNode; targetConfig :> Node; submit :> Node |] |> ignore
                Some(root :> Node)

    let unionCaseNamesFromContext (ctx: AppendInputContextDto) (schema: ArguFormSchemaDto) =
        let allowed = arrayOrEmpty ctx.unionCaseNames |> Array.map asText |> Array.filter (not << isBlank)

        if allowed.Length = 0 then
            schema.unionCases |> arrayOrEmpty |> Array.map _.name
        else
            allowed

    let rec flattenNodeDefaults (node: SduiFormNodeDto) =
        seq {
            if not (isNull (box node)) then
                yield node

                for child in arrayOrEmpty node.children do
                    yield! flattenNodeDefaults child

                for item in arrayOrEmpty node.items do
                    yield! flattenNodeDefaults item
        }

    let defaultsFromDocument (document: SduiFormDocumentDto option) =
        match document with
        | None -> Map.empty
        | Some document when isNull (box document) -> Map.empty
        | Some document ->
            document.nodes
            |> arrayOrEmpty
            |> Seq.collect flattenNodeDefaults
            |> Seq.choose (fun node ->
                let binding = asText node.binding
                let values = arrayOrEmpty node.defaultValues |> Array.map asText

                if isBlank binding || values.Length = 0 then
                    None
                else
                    Some(binding, values))
            |> Map.ofSeq

    let defaultValuesFor defaultMap caseName fieldName =
        let binding = asText caseName + "." + asText fieldName

        defaultMap
        |> Map.tryFind binding
        |> Option.defaultValue [||]

    let renderField (refresh: unit -> unit) defaultMap caseName (field: ArguFormFieldDto) =
        let row = element "div" "dynamic-argu-field" null |> setTestId ("dynamic-argu-field-" + asText field.name)
        row.SetAttribute("data-dynamic-argu-field", asText field.name)
        row.SetAttribute("data-dynamic-argu-kind", asText field.kind)
        row.AppendChild(label (if isBlank field.label then field.name else field.label)) |> ignore
        let fieldDefaults = defaultValuesFor defaultMap caseName field.name

        let mutable getter = fun () -> [||]
        let wireInputEvents (node: #Element) =
            node.AddEventListener("input", fun () -> refresh ())
            node.AddEventListener("change", fun () -> refresh ())

        let renderScalarInput testId itemKind options defaults =
            match asText itemKind with
            | "number" ->
                let node = input "number" "dynamic-argu-input" testId
                node.Value <- defaults |> Array.tryHead |> Option.defaultValue ""
                node.SetAttribute("data-dynamic-argu-input", "true")
                wireInputEvents node
                node :> Element, fun () -> [| elementValue node |]
            | "enum" ->
                let node = doc.CreateElement("select") :?> HTMLSelectElement
                node.ClassName <- "dynamic-argu-select"
                setTestId testId node |> ignore
                node.SetAttribute("data-dynamic-argu-input", "true")

                options
                |> arrayOrEmpty
                |> Array.iter (fun value ->
                    let option = doc.CreateElement("option")
                    option.SetAttribute("value", asText value)
                    option.TextContent <- asText value
                    node.AppendChild option |> ignore)

                setElementValue node (defaults |> Array.tryHead |> Option.defaultValue (elementValue node))
                wireInputEvents node
                node :> Element, fun () -> [| elementValue node |]
            | "bool" | "bool-value" ->
                let node = input "checkbox" "dynamic-argu-input" testId
                let value = defaults |> Array.tryHead |> Option.defaultValue ""
                node.Checked <- value.ToLower() = "true" || value = "1" || value.ToLower() = "yes"
                node.SetAttribute("data-dynamic-argu-input", "true")
                wireInputEvents node
                node :> Element, fun () -> [| if node.Checked then "true" else "false" |]
            | _ ->
                let node = input "text" "dynamic-argu-input" testId
                node.Value <- defaults |> Array.tryHead |> Option.defaultValue ""
                node.SetAttribute("data-dynamic-argu-input", "true")
                wireInputEvents node
                node :> Element, fun () -> [| node.Value |]

        match asText field.kind with
        | "number" ->
            let node = input "number" "dynamic-argu-input" ("dynamic-argu-number-" + asText field.name)
            node.Value <- fieldDefaults |> Array.tryHead |> Option.defaultValue ""
            node.SetAttribute("data-dynamic-argu-input", "true")
            wireInputEvents node
            row.AppendChild node |> ignore
            getter <- fun () -> [| elementValue node |]
        | "enum" ->
            let node = doc.CreateElement("select") :?> HTMLSelectElement
            node.ClassName <- "dynamic-argu-select"
            setTestId ("dynamic-argu-enum-" + asText field.name) node |> ignore
            node.SetAttribute("data-dynamic-argu-input", "true")

            field.options
            |> arrayOrEmpty
            |> Array.iter (fun value ->
                let option = doc.CreateElement("option")
                option.SetAttribute("value", asText value)
                option.TextContent <- asText value
                node.AppendChild option |> ignore)

            setElementValue node (fieldDefaults |> Array.tryHead |> Option.defaultValue (elementValue node))
            wireInputEvents node
            row.AppendChild node |> ignore
            getter <- fun () -> [| elementValue node |]
        | "tuple" ->
            let tuple = element "div" "dynamic-argu-tuple" null |> setTestId ("dynamic-argu-tuple-" + asText field.name)
            let itemGetters = ResizeArray<unit -> string[]>()

            field.items
            |> arrayOrEmpty
            |> Array.iteri (fun index item ->
                let itemRow = element "div" "dynamic-argu-tuple-item" null |> setTestId $"dynamic-argu-tuple-item-{asText field.name}-{index + 1}"
                itemRow.AppendChild(label $"{index + 1}. {if isBlank item.label then item.name else item.label}") |> ignore
                let node, valueGetter = renderScalarInput "" item.kind item.options (defaultValuesFor defaultMap caseName item.name)
                node.SetAttribute("data-dynamic-argu-tuple-item", string (index + 1))
                itemGetters.Add valueGetter
                itemRow.AppendChild node |> ignore
                tuple.AppendChild itemRow |> ignore)

            row.AppendChild tuple |> ignore
            getter <- fun () -> itemGetters |> Seq.collect (fun getter -> getter ()) |> Seq.toArray
        | "list" ->
            let list = element "div" "dynamic-argu-list" null |> setTestId ("dynamic-argu-list-" + asText field.name)
            let itemGetters = ResizeArray<unit -> string[]>()
            let add = button "dynamic-argu-add-list-item" ("dynamic-argu-list-add-" + asText field.name) "Add"

            let addInput defaults =
                let item =
                    field.items
                    |> arrayOrEmpty
                    |> Array.tryHead
                    |> Option.defaultValue
                        { name = field.name + "Item"
                          label = field.label
                          kind = "text"
                          arguName = ""
                          options = [||]
                          items = [||] }

                let node, getter = renderScalarInput ("dynamic-argu-list-item-" + asText field.name) item.kind item.options defaults
                node.SetAttribute("data-dynamic-argu-list-item", "true")
                itemGetters.Add getter
                list.InsertBefore(node, add) |> ignore

            add.AddEventListener("click", fun () ->
                addInput [||]
                refresh ())

            list.AppendChild add |> ignore

            if fieldDefaults.Length = 0 then
                addInput [||]
            else
                fieldDefaults |> Array.iter (fun value -> addInput [| value |])

            row.AppendChild list |> ignore
            getter <- fun () -> itemGetters |> Seq.collect (fun getter -> getter ()) |> Seq.toArray
        | "bool" | "bool-value" ->
            let node = input "checkbox" "dynamic-argu-input" ("dynamic-argu-bool-" + asText field.name)
            let value = fieldDefaults |> Array.tryHead |> Option.defaultValue ""
            node.Checked <- value.ToLower() = "true" || value = "1" || value.ToLower() = "yes"
            node.SetAttribute("data-dynamic-argu-input", "true")
            wireInputEvents node
            row.AppendChild node |> ignore
            getter <- fun () -> [| if node.Checked then "true" else "false" |]
        | _ ->
            let node = input "text" "dynamic-argu-input" ("dynamic-argu-text-" + asText field.name)
            node.Value <- fieldDefaults |> Array.tryHead |> Option.defaultValue ""
            node.SetAttribute("data-dynamic-argu-input", "true")
            wireInputEvents node
            row.AppendChild node |> ignore
            getter <- fun () -> [| node.Value |]

        row, getter

    let buildRawArgu unionCase (fieldGetters: (ArguFormFieldDto * (unit -> string[]))[]) =
        fieldGetters
        |> Array.map (fun (field, getter) -> field, getter ())
        |> ClientRawArguCodec.buildRawArguFromValues

    let renderSchemaIntoRoot (root: Element) (context: AppendInputContextDto) typeName document (schema: ArguFormSchemaDto) =
        root.TextContent <- ""
        let defaultMap = defaultsFromDocument document

        let allowed =
            match document with
            | Some _ -> schema.unionCases |> arrayOrEmpty |> Array.map _.name
            | None -> unionCaseNamesFromContext context schema

        let unionCases =
            schema.unionCases
            |> arrayOrEmpty
            |> Array.filter (fun item -> allowed |> Array.exists (fun name -> name = asText item.name))

        root.SetAttribute("data-dynamic-argu-du-type", typeName)
        root.SetAttribute("data-dynamic-form-document-id", match document with | Some document -> asText document.documentId | None -> "")
        root.SetAttribute("data-dynamic-argu-union-cases", String.concat "," allowed)

        if unionCases.Length = 0 then
            root.AppendChild(errorNode ("Dynamic Argu schema has no requested union cases for DU type: " + typeName)) |> ignore
        else
            unionCases
            |> Array.iter (fun unionCase ->
                let caseName = asText unionCase.name
                let caseRow =
                    element "section" "dynamic-argu-case-row" null
                    |> setTestId ("dynamic-argu-case-" + caseName)

                caseRow.SetAttribute("data-dynamic-argu-case", caseName)

                let headingText = if isBlank unionCase.label then caseName else asText unionCase.label
                let heading = element "div" "dynamic-argu-case-title" headingText |> setTestId ("dynamic-argu-case-title-" + caseName)
                let fields = element "div" "dynamic-argu-fields" null |> setTestId ("dynamic-argu-fields-" + caseName)
                let rawPreview = element "pre" "dynamic-argu-raw-preview" "" |> setTestId ("dynamic-argu-raw-preview-" + caseName)
                let send = button "dynamic-argu-send" ("dynamic-argu-send-" + caseName) "Send"
                let mutable fieldGetters: (ArguFormFieldDto * (unit -> string[]))[] = [||]

                let refreshPreview () =
                    rawPreview.TextContent <- buildRawArgu unionCase fieldGetters

                fieldGetters <-
                    unionCase.fields
                    |> arrayOrEmpty
                    |> Array.map (fun field ->
                        let row, getter = renderField refreshPreview defaultMap caseName field
                        fields.AppendChild row |> ignore
                        field, getter)

                send.AddEventListener(
                    "click",
                    fun () ->
                        let raw = buildRawArgu unionCase fieldGetters
                        rawPreview.TextContent <- raw
                        let payload: AppendSubmitPayloadDto =
                            { rawArgu = raw
                              duTypeName = typeName
                              unionCaseName = caseName }

                        context.submit(box payload))

                append caseRow [| heading :> Node; fields :> Node; rawPreview :> Node; send :> Node |] |> ignore
                root.AppendChild caseRow |> ignore
                refreshPreview ())

    let renderAppendInput (ctx: obj) =
        let context = ctx |> As<AppendInputContextDto>
        let typeName = asText context.duTypeName
        let keyParts = arrayOrEmpty context.keyParts |> Array.map asText
        let isBackendTarget = keyParts.Length = 3 && not (isBlank keyParts[2])

        if isBlank typeName then
            None
        else
            let root = element "div" "dynamic-argu-form" "Loading Dynamic Argu form..." |> setTestId "dynamic-argu-form"

            if isBackendTarget then
                let request: ResolveTargetRequestDto = { keys = keyParts }

                postJson<ResolveTargetRequestDto, ResolveTargetReplyDto>
                    "/client-extensions/dynamic/argu/resolve-target"
                    request
                    (fun reply ->
                        if reply.ok && not (isNull (box reply.document)) && not (isNull (box reply.document.arguFormSchema)) then
                            renderSchemaIntoRoot root context (asText reply.templateKey) (Some reply.document) reply.document.arguFormSchema
                        else
                            root.TextContent <- ""
                            root.AppendChild(errorNode (if isBlank reply.error then "Dynamic Argu target resolution failed." else reply.error)) |> ignore)
                    (fun error ->
                        root.TextContent <- ""
                        root.AppendChild(errorNode error) |> ignore)

                Some(root :> Node)
            else
                let document = tryFindDocument typeName

                let schema =
                    match document with
                    | Some document when not (isNull (box document.arguFormSchema)) -> Some document.arguFormSchema
                    | _ -> tryFindSchema typeName

                match schema with
                | None -> Some(errorNode ("Dynamic Form document or Argu schema not found for target: " + typeName) :> Node)
                | Some schema ->
                    renderSchemaIntoRoot root context typeName document schema
                    Some(root :> Node)

    let registerRenderer name priority renderer =
        let register = JS.Global?PulseTradeRegisterRenderer
        if JS.TypeOf register = JS.Kind.Function then
            JS.Global?PulseTradeRegisterRenderer(name, priority, renderer)

    let registerAppendInputRenderer name priority renderer =
        let register = JS.Global?PulseTradeRegisterAppendInputRenderer
        if JS.TypeOf register = JS.Kind.Function then
            JS.Global?PulseTradeRegisterAppendInputRenderer(name, priority, renderer)

    let registerAddKeyRenderer name priority renderer =
        let register = JS.Global?PulseTradeRegisterAddKeyRenderer
        if JS.TypeOf register = JS.Kind.Function then
            JS.Global?PulseTradeRegisterAddKeyRenderer(name, priority, renderer)

    [<JavaScriptExport>]
    let Register () =
        loadSchemasFromManifest ()
        registerAddKeyRenderer "dynamic-argu-add-key" 100 renderAddKey
        registerAppendInputRenderer "dynamic-argu-append-input" 100 renderAppendInput
