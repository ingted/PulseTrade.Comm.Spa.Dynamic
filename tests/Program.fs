module PulseTrade.Comm.Spa.Dynamic.Tests.Program

open System
open System.Text.Json
open Expecto
open Argu
open PersistedConcurrentSortedList.Type
open PulseTrade.Comm.Spa.Dynamic.Server
open PulseTrade.Comm.Spa.Dynamic.Client
open PulseTrade.Comm.Spa
open Akka.Actor

type TestMode =
    | Fast
    | Safe
    | Audit

type TestArgu =
    | Say of text: string
    | Set_Count of count: int
    | Mode of mode: TestMode
    | At of symbol: string * quantity: int
    | Tag of tag: string list
    | Verbose
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Say _ -> "Send a text payload."
            | Set_Count _ -> "Send a numeric count payload."
            | Mode _ -> "Select a mode."
            | At _ -> "Send an ordered tuple payload."
            | Tag _ -> "Send repeated tags."
            | Verbose -> "Enable verbose mode."

type GenEnum =
    | CreateTable
    | FSRecord

type ReferenceDateMode =
    | ModeAccountingDate
    | ModeTradingDate

type PFCF_AKKA_CMD_DATA_RANGE =
    | ReferenceDateMode of ReferenceDateMode
    | Between of decimal * decimal
    | After of decimal
    | NoFilter
    | Calibrate2CurDayIfLargerThanCurDay
    interface IArgParserTemplate with
        member _.Usage = ""

type PFCF_AKKA_CMD_GM =
    | RiskScore of decimal
    | Branch of string
    | IfRealTime of bool
    interface IArgParserTemplate with
        member _.Usage = ""

type PFCF_AKKA_Terry =
    | List
    interface IArgParserTemplate with
        member _.Usage = ""

type PFCF_AKKA_WenZone =
    | List
    interface IArgParserTemplate with
        member _.Usage = ""

type ClosingNoMode =
    | ParentChildShared
    | ParentChildSeparated

type PFCFGTC =
    | GF
    | GC
    | GOD
    | GOI
    | GMA

type CooperativeType =
    | CONTRACTS
    | TRADING
    | COVER
    | MARGIN
    | OI
    | ORDERS

type PFCF_AKKA_CMD_BANK_EDX =
    | CathayBKNonTaifexFill
    | CathayBKNonTaifexOI
    | SCSBAfterHoursFill
    | SCSBTradeSummary
    | SCSBIntradayOrder
    | SCSBIntradayFill
    | SCSBOpenPositionSummaryF
    | SCSBOpenPositionSummaryT
    | SCSBIntradayOI
    | SCSBIntradayOIByFill
    | SCSBRiskFactor
    | SCSBMargin
    interface IArgParserTemplate with
        member _.Usage = ""

type PFCF_GTC_CONF =
    | FillDTFormatYYYYMMDD
    | ShowOrderSN
    | OrderByTXDT
    | OrderBySQDT
    | FillSquareCombine
    | CathayBKTaifexFill
    | CathayBKTaifexOI
    | OIInf
    | TAIFEX
    | ASIA
    | EURUS
    | Empty
    | Default

type PFCF_AKKA_CMD =
    | SimpleAction of action_name: string
    | Entrust of id: string * accountingDay: decimal
    | Transglobe of brokerBranch: string * id: string * accountingDay: decimal
    | PFCFGTC of PFCFGTC list
    | PFCFEDX of mode: string
    | PFCFGTCCONF of PFCF_GTC_CONF list
    | BBA of 期貨商: string * 分公司: string * 母帳帳號: string
    | Entie of mode: int
    | TSIT
    | USITC
    | PGIM
    | RID of string
    | Cooperative of CooperativeType
    | BankEdx of PFCF_AKKA_CMD_BANK_EDX list
    | MLI of brokerBranch: string * id: string
    | ParentChild of int
    | ParentChilds of int list
    | FractionalQuote of if全轉分數報價orOnlyFor原分數報價: bool * 自訂分母值: float
    | DecimalQuote of 小數位數: int * ``0四捨五入1無條件捨去2無條件進位3無條件截斷``: int
    | Round of 非分數報價小數位數: int * 分數報價分數部分小數位數: int * 收支欄位小數位數: int
    | TW
    | NonTW
    | Futures
    | Options
    | TO of float
    | Debug of string
    | ClosingNoMode of ClosingNoMode
    | [<CliPrefix(CliPrefix.None)>] DataRange of ParseResults<PFCF_AKKA_CMD_DATA_RANGE>
    | [<CliPrefix(CliPrefix.None)>] GM of ParseResults<PFCF_AKKA_CMD_GM>
    | [<CliPrefix(CliPrefix.None)>] Terry of ParseResults<PFCF_AKKA_Terry>
    | [<CliPrefix(CliPrefix.None)>] WZ of ParseResults<PFCF_AKKA_WenZone>
    | GenByColMeta of ifTw: bool * ifOrig: bool * schemaName: string * genType: GenEnum
    | GenFromDWQuery of sql0F1T2F: int * sqlB64: string * schName: string * tblName: string * genType: GenEnum * ifOrig: bool * ifSchemaOnly: bool
    | TableName of string list
    | OutCreateTable of string
    interface IArgParserTemplate with
        member _.Usage = ""

let testArguDuTypeName = typeof<TestArgu>.FullName
let pfcfDuTypeName = typeof<PFCF_AKKA_CMD>.FullName
let pfcfDataRangeExpectedRaw =
    "--pfcfedx trivial --pfcfgtcconf OIInf TAIFEX FillSquareCombine OrderByTXDT CathayBKTaifexFill --to 90000 --parentchilds 2 5 --bba F008 000 9910357 --decimalquote 6 0 --round 6 4 2 datarange --referencedatemode ModeAccountingDate --between 20251104 20251104 --calibrate2curdayiflargerthancurday"
let pfcfPartialExpectedRaw =
    "--pfcfedx trivial --pfcfgtcconf OIInf TAIFEX"

let findCase caseName (schema: ArguFormSchema) =
    ArguFormSchema.tryFindUnionCase caseName schema
    |> Option.defaultWith (fun () -> failwithf "Union case not found: %s" caseName)

let onlyFieldName caseName schema =
    let unionCase = findCase caseName schema

    unionCase.Fields
    |> Array.tryHead
    |> Option.map _.Name
    |> Option.defaultWith (fun () -> failwithf "Union case has no field: %s" caseName)

let submitArgu (schema: ArguFormSchema) unionCaseName fields =
    { DuTypeName = schema.DuTypeName
      UnionCaseName = unionCaseName
      Fields =
        fields
        |> Array.map (fun (name, values) ->
            { Name = name
              Values = values }) }

let rec toClientField (field: ArguFormField) : ArguFormFieldDto =
    { name = field.Name
      label = field.Label
      kind = field.Kind
      arguName = field.ArguName
      options = field.Options
      items = field.Items |> Array.map toClientField }

let clientRawArgu (schema: ArguFormSchema) unionCaseName fields =
    let unionCase = findCase unionCaseName schema
    let fieldMap = fields |> Map.ofArray

    unionCase.Fields
    |> Array.map (fun field ->
        let values =
            fieldMap
            |> Map.tryFind field.Name
            |> Option.defaultValue [||]

        toClientField field, values)
    |> ClientRawArguCodec.buildRawArguFromValues

let tests =
    testList "PulseTrade.Comm.Spa.Dynamic Tests" [
        
        testCase "WBS-102: FCell2Interop should serialize GridFeatures correctly" <| fun _ ->
            let cell = 
                fCell2<string>.A [|
                    fCell2<string>.T (Map [
                        "id", fCell2.S "grid-1"
                        "mode", fCell2.S "table"
                        "theme", fCell2.S "dark"
                    ])
                |]
            
            let json = FCell2Interop.toJsonString cell
            let payload = FCell2Interop.toMessagePayload cell
            
            Expect.isTrue (json.Contains("\"theme\":\"dark\"")) "JSON should contain theme property"
            Expect.isTrue (payload.StartsWith("{\"schema\":\"fskynet-sdui\",\"ui\":")) "Payload should wrap with schema"
            
        testCase "WBS-101: CommHub.useDynamicSdui should mount without exception" <| fun _ ->
            use system = ActorSystem.Create("TestSystem")
            let dummyState = Unchecked.defaultof<CommState>
            let dummyBackend = Unchecked.defaultof<ICommSpaPersistenceBackend>
            let hub = CommHub(dummyState, dummyBackend)
            let _ = hub.useDynamicSdui(system)
            
            Expect.isTrue true "useDynamicSdui executed successfully"
            
        testCase "WBS-103: DynamicRenderer.TryRender should return Some for fskynet-sdui schema" <| fun _ ->
            let payload = "{\"schema\":\"fskynet-sdui\",\"ui\":[]}"
            let result = DynamicRenderer.TryRender payload
            Expect.isSome result "TryRender should return Some Doc for valid fskynet-sdui schema"
            
            let invalidPayload = "{\"schema\":\"other\",\"ui\":[]}"
            let invalidResult = DynamicRenderer.TryRender invalidPayload
            Expect.isNone invalidResult "TryRender should return None for other schemas"

        testCase "WBS-104: ActorDynamicTab.renderActorDynamicPage should generate valid page Doc" <| fun _ ->
            let _ = ActorDynamicTab.renderActorDynamicPage "actor-dynamic"
            Expect.isTrue true "Tab page generation should succeed"

        testCase "DYN-T-402: Argu form schema should expose fskynet-sdui argu-form metadata" <| fun _ ->
            let schema = ArguFormSchema.fromArgParserTemplate<TestArgu>()
            let json = ArguFormSchema.generateSduiJson schema

            Expect.equal schema.Schema "fskynet-sdui" "schema marker should match Dynamic SDUI payload contract"
            Expect.equal schema.FormMode "argu-form" "formMode should identify Argu form payloads"
            Expect.equal schema.DuTypeName testArguDuTypeName "schema should use the actual Argu DU type name"
            Expect.isTrue (json.Contains("\"formMode\":\"argu-form\"")) "JSON should expose formMode"
            Expect.isSome (ArguFormSchema.tryFindUnionCase "Say" schema) "Say union case should be registered"
            Expect.isSome (ArguFormSchema.tryFindUnionCase "At" schema) "Tuple union case should be registered"
            Expect.isSome (ArguFormSchema.tryFindUnionCase "Tag" schema) "List union case should be registered"
            Expect.equal (ArguFormSchema.tryFindUnionCase "Set_Count" schema |> Option.map _.ArguName) (Some "--set-count") "Argu command line name should come from FAkka.Argu"
            Expect.equal (ArguFormSchema.tryFindUnionCase "Mode" schema |> Option.bind (fun c -> c.Fields |> Array.tryHead) |> Option.map _.Options) (Some [| "fast"; "safe"; "audit" |]) "DU enum options should use Argu enum token names"

        testCase "DYN-T-403: SubmitArguForm codec should build quoted raw Argu string for common patterns" <| fun _ ->
            let schema = ArguFormSchema.fromArgParserTemplate<TestArgu>()
            let submit unionCase fields =
                { DuTypeName = schema.DuTypeName
                  UnionCaseName = unionCase
                  Fields =
                    fields
                    |> Array.map (fun (name, values) ->
                        { Name = name
                          Values = values }) }

            let say =
                submit "Say" [| "text", [| "hello world" |] |]
                |> SubmitArguFormCodec.buildRawArgu schema

            let mode =
                submit "Mode" [| "mode", [| "safe" |] |]
                |> SubmitArguFormCodec.buildRawArgu schema

            let tuple =
                submit "At" [| onlyFieldName "At" schema, [| "TTC"; "7" |] |]
                |> SubmitArguFormCodec.buildRawArgu schema

            let tags =
                submit "Tag" [| "tag", [| "aoe"; "marvel now" |] |]
                |> SubmitArguFormCodec.buildRawArgu schema

            let verbose =
                submit "Verbose" [| "verbose", [| "true" |] |]
                |> SubmitArguFormCodec.buildRawArgu schema

            Expect.equal say "--say \"hello world\"" "text value with whitespace should be quoted"
            Expect.equal mode "--mode safe" "enum value should map to Argu select value"
            Expect.equal tuple "--at TTC 7" "tuple should preserve ordered values"
            Expect.equal tags "--tag aoe \"marvel now\"" "list should emit canonical inline Argu values"
            Expect.equal verbose "--verbose" "bool flag true should emit flag only"

        testCase "DYN-T-501: Argu adapter should generate FormInput DSL document for PFCF_AKKA_CMD fixture" <| fun _ ->
            let schema = ArguFormSchema.fromArgParserTemplate<PFCF_AKKA_CMD>()
            let document = SduiFormDocument.fromArguFormSchema "ptcs.host.tests.pfcf.form" schema

            Expect.equal document.Schema "fskynet-sdui" "document schema marker should be SDUI"
            Expect.equal document.Surface "FormInput" "Argu adapter should generate a FormInput surface"
            Expect.equal document.DocumentId "ptcs.host.tests.pfcf.form" "document id should support direct DSL target key"
            Expect.equal document.ArguFormSchema.DuTypeName pfcfDuTypeName "document should retain source DU type"
            Expect.isSome (ArguFormSchema.tryFindUnionCase "SimpleAction" schema) "SimpleAction should be reflected"
            Expect.isSome (ArguFormSchema.tryFindUnionCase "BBA" schema) "BBA should preserve Chinese field labels"
            Expect.isSome (document.Nodes |> Array.tryFind (fun node -> node.Id = "case-simpleaction")) "document should expose SimpleAction section"

            let genByColMeta = findCase "GenByColMeta" schema
            let tupleField = genByColMeta.Fields |> Array.exactlyOne
            let tupleKinds = tupleField.Items |> Array.map _.Kind

            Expect.equal tupleField.Kind "tuple" "GenByColMeta should be represented as ordered tuple input"
            Expect.sequenceEqual tupleKinds [| "bool-value"; "bool-value"; "text"; "enum" |] "tuple item kinds should drive checkbox/text/select controls"

        testCase "DYN-T-503: Dynamic target key resolver should support direct DSL and DU target keys" <| fun _ ->
            let schema = ArguFormSchema.fromArgParserTemplate<PFCF_AKKA_CMD>()
            let document = SduiFormDocument.fromArguFormSchema "ptcs.host.tests.pfcf.form" schema
            let metadata = DynamicArguMetadata.fromDocuments [ document ]
            let actorAddress = "akka.tcp://PulseTradeCommSpaDynamic@127.0.0.1:8039/user/pfcf"

            match DynamicTargetKey.tryResolve metadata [ actorAddress; document.DocumentId ] with
            | Ok(DirectDslTarget(actor, formDslId)) ->
                Expect.equal actor actorAddress "direct target should preserve actor address"
                Expect.equal formDslId document.DocumentId "direct target should resolve document id"
            | other -> failwithf "direct target should resolve. got=%A" other

            match DynamicTargetKey.tryResolve metadata [ actorAddress; pfcfDuTypeName; "SimpleAction"; "BBA"; "GenByColMeta" ] with
            | Ok(LegacyArguCaseTarget(actor, duTypeName, unionCases)) ->
                Expect.equal actor actorAddress "DU target should preserve actor address"
                Expect.equal duTypeName pfcfDuTypeName "DU target should resolve type name"
                Expect.sequenceEqual unionCases [ "SimpleAction"; "BBA"; "GenByColMeta" ] "DU target should preserve union case tail order"
            | other -> failwithf "DU target should resolve. got=%A" other

            Expect.isError (DynamicTargetKey.tryResolve metadata [ actorAddress; "missing.dsl" ]) "unknown direct/form discriminator should fail"
            Expect.isError (DynamicTargetKey.tryResolve metadata [ actorAddress; pfcfDuTypeName; "MissingCase" ]) "unknown union case should fail"
            Expect.isError (DynamicTargetKey.tryResolve metadata [ actorAddress; document.DocumentId; "SimpleAction" ]) "direct DSL target should reject union-case tail"

        testCase "DYN-T-507: Dynamic arg-string target resolver should require actor template and canonical arg string" <| fun _ ->
            let actorAddress = "akka.tcp://PulseTradeCommSpaDynamic@127.0.0.1:8039/user/pfcf"
            let canonicalArgString = "--simpleaction rebuild"
            let registration =
                DynamicArguTemplateRegistration.fromTemplate<PFCF_AKKA_CMD>
                    DynamicArguAliasBinding.empty
                    (Some canonicalArgString)

            match DynamicArgStringTarget.tryResolve [ registration ] [ actorAddress; pfcfDuTypeName; canonicalArgString ] with
            | Ok(ArguTemplateTarget(actor, templateKey, argString)) ->
                Expect.equal actor actorAddress "arg-string target should preserve actor address"
                Expect.equal templateKey pfcfDuTypeName "arg-string target should resolve registered template"
                Expect.equal argString canonicalArgString "arg-string target should preserve canonical arg string"
            | other -> failwithf "arg-string target should resolve. got=%A" other

            Expect.isError (DynamicArgStringTarget.tryResolve [ registration ] [ actorAddress; pfcfDuTypeName ]) "missing canonical arg string should fail"
            Expect.isError (DynamicArgStringTarget.tryResolve [ registration ] [ actorAddress; "missing.template"; canonicalArgString ]) "unknown template key should fail"
            Expect.isError (DynamicArgStringTarget.tryResolve [ registration ] [ actorAddress; pfcfDuTypeName; "--missing value" ]) "parser failure should fail"

        testCase "DYN-T-508: Alias binding should affect FormInput DSL labels without changing canonical option values" <| fun _ ->
            let schema = ArguFormSchema.fromArgParserTemplate<PFCF_AKKA_CMD>()
            let aliases =
                { CaseAliases = Map [ "BBA", "買報帳號" ]
                  FieldAliases = Map [ ("BBA", "期貨商"), "期貨商代號"; ("BBA", "分公司"), "分公司代號" ]
                  OptionAliases = Map [ ("GenByColMeta", "fsrecord"), "FS 記錄" ] }

            let document = DynamicFormDsl.fromArguFormSchemaWithAliases "ptcs.host.tests.pfcf.form" aliases schema
            let bbaSection =
                document.Nodes
                |> Array.find (fun node -> node.Id = "case-bba")

            Expect.equal bbaSection.Title "買報帳號" "case alias should be used as section title"

            let bbaFieldLabels =
                bbaSection.Children
                |> Array.filter (fun node -> node.Type = "Tuple")
                |> Array.collect _.Items
                |> Array.map _.Label

            Expect.isTrue
                (Array.contains "期貨商代號" bbaFieldLabels && Array.contains "分公司代號" bbaFieldLabels)
                "field aliases should be used as input labels"

            let genByColMeta =
                document.Nodes
                |> Array.find (fun node -> node.Id = "case-genbycolmeta")

            let genTypeSelect =
                genByColMeta.Children
                |> Array.filter (fun node -> node.Type = "Tuple")
                |> Array.collect _.Items
                |> Array.find (fun node -> node.Kind = "Select")

            let fsrecordOption = genTypeSelect.Options |> Array.find (fun option -> option.Value = "fsrecord")
            Expect.equal fsrecordOption.Label "FS 記錄" "option alias should affect label"
            Expect.equal fsrecordOption.Value "fsrecord" "option alias must not change canonical value"

        testCase "DYN-T-502: PFCF_AKKA_CMD raw Argu strings should match server and frontend codec expectations" <| fun _ ->
            let schema = ArguFormSchema.fromArgParserTemplate<PFCF_AKKA_CMD>()

            let assertRaw unionCaseName fields expected =
                let serverRaw =
                    submitArgu schema unionCaseName fields
                    |> SubmitArguFormCodec.buildRawArgu schema

                let frontendRaw = clientRawArgu schema unionCaseName fields

                Expect.equal serverRaw expected $"{unionCaseName} server codec should build expected raw Argu"
                Expect.equal frontendRaw expected $"{unionCaseName} frontend codec should build expected raw Argu"

            assertRaw
                "SimpleAction"
                [| onlyFieldName "SimpleAction" schema, [| "rebuild all" |] |]
                "--simpleaction \"rebuild all\""

            assertRaw
                "Entrust"
                [| onlyFieldName "Entrust" schema, [| "A123456789"; "20260626" |] |]
                "--entrust A123456789 20260626"

            assertRaw
                "PFCFGTC"
                [| onlyFieldName "PFCFGTC" schema, [| "gf"; "goi" |] |]
                "--pfcfgtc gf goi"

            assertRaw
                "BBA"
                [| onlyFieldName "BBA" schema, [| "F001"; "B001"; "M123" |] |]
                "--bba F001 B001 M123"

            assertRaw
                "Cooperative"
                [| onlyFieldName "Cooperative" schema, [| "trading" |] |]
                "--cooperative trading"

            assertRaw
                "ParentChilds"
                [| onlyFieldName "ParentChilds" schema, [| "7"; "9" |] |]
                "--parentchilds 7 9"

            assertRaw
                "FractionalQuote"
                [| onlyFieldName "FractionalQuote" schema, [| "true"; "32" |] |]
                "--fractionalquote true 32"

            assertRaw
                "GenByColMeta"
                [| onlyFieldName "GenByColMeta" schema, [| "true"; "false"; "dbo"; "fsrecord" |] |]
                "--genbycolmeta true false dbo fsrecord"

            assertRaw
                "TableName"
                [| onlyFieldName "TableName" schema, [| "Orders"; "Positions Today" |] |]
                "--tablename Orders \"Positions Today\""

        testCase "DYN-T-509: Parser-backed target scan should expose root cases and DataRange defaults" <| fun _ ->
            let registration =
                DynamicArguTemplateRegistration.fromTemplate<PFCF_AKKA_CMD>
                    DynamicArguAliasBinding.empty
                    (Some pfcfDataRangeExpectedRaw)

            let parsed =
                DynamicArgStringTarget.scan registration "akka://pfcf" pfcfDataRangeExpectedRaw

            let rootCaseNames = parsed.RootCases |> Array.map _.CaseName
            Expect.sequenceEqual
                rootCaseNames
                [| "PFCFEDX"; "PFCFGTCCONF"; "TO"; "ParentChilds"; "BBA"; "DecimalQuote"; "Round" |]
                "root cases should preserve canonical arg string order"

            let pfcfedx = parsed.RootCases |> Array.find (fun item -> item.CaseName = "PFCFEDX")
            Expect.sequenceEqual (pfcfedx.Values |> Array.collect _.Values) [| "trivial" |] "PFCFEDX default should come from arg string"

            let dataRange = parsed.TailSubcommands |> Array.exactlyOne
            Expect.equal dataRange.CaseName "DataRange" "tail subcommand should be DataRange"
            Expect.equal dataRange.CommandToken "datarange" "tail subcommand token should be datarange"

            let nestedCaseNames = dataRange.Cases |> Array.map _.CaseName
            Expect.sequenceEqual
                nestedCaseNames
                [| "ReferenceDateMode"; "Between"; "Calibrate2CurDayIfLargerThanCurDay" |]
                "nested DataRange cases should preserve subcommand arg order"

            let document = DynamicArgStringTarget.buildFormDocument "ptcs.host.tests.pfcf.data-range" registration parsed
            Expect.sequenceEqual
                (document.Nodes |> Array.map _.Id)
                [| "case-pfcfedx"; "case-pfcfgtcconf"; "case-to"; "case-parentchilds"; "case-bba"; "case-decimalquote"; "case-round"; "case-datarange" |]
                "Form DSL sections should follow parsed root case order and include tail subcommand"

            let rec flattenNode (node: SduiFormNode) =
                seq {
                    yield node
                    for child in node.Children do
                        yield! flattenNode child
                    for item in node.Items do
                        yield! flattenNode item
                }

            let defaultValues binding =
                document.Nodes
                |> Seq.collect flattenNode
                |> Seq.find (fun node -> node.Binding = binding)
                |> _.DefaultValues

            Expect.sequenceEqual (defaultValues "PFCFEDX.mode") [| "trivial" |] "PFCFEDX default should be projected into Form DSL"
            Expect.sequenceEqual
                (defaultValues "PFCFGTCCONF.value")
                [| "OIInf"; "TAIFEX"; "FillSquareCombine"; "OrderByTXDT"; "CathayBKTaifexFill" |]
                "list defaults should be projected into Form DSL"
            Expect.sequenceEqual (defaultValues "BBA.期貨商") [| "F008" |] "tuple default item 1 should be projected into Form DSL"
            Expect.sequenceEqual (defaultValues "BBA.分公司") [| "000" |] "tuple default item 2 should be projected into Form DSL"
            Expect.sequenceEqual (defaultValues "BBA.母帳帳號") [| "9910357" |] "tuple default item 3 should be projected into Form DSL"

        testCase "DYN-T-510: ParseResults DataRange raw command builder should keep datarange tail ordering" <| fun _ ->
            let registration =
                DynamicArguTemplateRegistration.fromTemplate<PFCF_AKKA_CMD>
                    DynamicArguAliasBinding.empty
                    (Some pfcfDataRangeExpectedRaw)

            let argv = DynamicCommandLine.split pfcfDataRangeExpectedRaw
            Expect.isOk (DynamicArgStringTarget.validateByParser registration argv) "Argu parser should accept PFCF data-range command"

            let parsed = DynamicArgStringTarget.scan registration "akka://pfcf" pfcfDataRangeExpectedRaw
            let rebuilt = DynamicArgStringTarget.buildRawArgu parsed

            Expect.equal rebuilt pfcfDataRangeExpectedRaw "rebuilt raw command should match exact expected datarange command"

            let datarangeIndex = rebuilt.IndexOf(" datarange ", StringComparison.Ordinal)
            let roundIndex = rebuilt.IndexOf("--round 6 4 2", StringComparison.Ordinal)
            let referenceDateIndex = rebuilt.IndexOf("--referencedatemode", StringComparison.Ordinal)

            Expect.isGreaterThan datarangeIndex roundIndex "datarange should come after root --round"
            Expect.isGreaterThan referenceDateIndex datarangeIndex "subcommand args should come after datarange"

        testCase "DYN-T-511: Resolve endpoint should return backend FormInput DSL for canonical arg-string target" <| fun _ ->
            let registration =
                DynamicArguTemplateRegistration.fromTemplate<PFCF_AKKA_CMD>
                    DynamicArguAliasBinding.empty
                    (Some pfcfDataRangeExpectedRaw)

            let request: DynamicArguResolveTargetRequest =
                { Keys = [| "akka://pfcf"; pfcfDuTypeName; pfcfDataRangeExpectedRaw |] }

            let requestJson = JsonSerializer.Serialize(request, ArguFormSchema.jsonOptions)
            let replyJson = DynamicArguResolveEndpoint.handle [ registration ] requestJson
            let reply = JsonSerializer.Deserialize<DynamicArguResolveTargetReply>(replyJson, ArguFormSchema.jsonOptions)

            Expect.isTrue reply.Ok reply.Error
            Expect.equal reply.TemplateKey pfcfDuTypeName "endpoint should resolve template key"
            Expect.equal reply.Document.DocumentId pfcfDuTypeName "endpoint document id should be template key"
            Expect.sequenceEqual
                (reply.Document.ArguFormSchema.UnionCases |> Array.map _.Name)
                [| "PFCFEDX"; "PFCFGTCCONF"; "TO"; "ParentChilds"; "BBA"; "DecimalQuote"; "Round"; "DataRange" |]
                "endpoint document should contain parsed root cases and tail subcommand in order"

            let rec flattenNode (node: SduiFormNode) =
                seq {
                    yield node
                    for child in node.Children do
                        yield! flattenNode child
                    for item in node.Items do
                        yield! flattenNode item
                }

            let pfcfedxDefault =
                reply.Document.Nodes
                |> Seq.collect flattenNode
                |> Seq.find (fun node -> node.Binding = "PFCFEDX.mode")
                |> _.DefaultValues

            Expect.sequenceEqual pfcfedxDefault [| "trivial" |] "endpoint document should include FormInput defaults"

            let defaultEntriesInOrder =
                reply.Document.Nodes
                |> Seq.collect flattenNode
                |> Seq.filter (fun node -> not (String.IsNullOrWhiteSpace node.Binding))
                |> Seq.map (fun node -> node.Binding, node.DefaultValues)
                |> Seq.toArray

            let defaultsByBinding = defaultEntriesInOrder |> Map.ofArray
            Expect.sequenceEqual
                (defaultsByBinding |> Map.find "BBA.期貨商")
                [| "F008" |]
                "endpoint document should preserve root tuple item default 1"
            Expect.sequenceEqual
                (defaultsByBinding |> Map.find "BBA.分公司")
                [| "000" |]
                "endpoint document should preserve root tuple item default 2"
            Expect.sequenceEqual
                (defaultsByBinding |> Map.find "BBA.母帳帳號")
                [| "9910357" |]
                "endpoint document should preserve root tuple item default 3"
            Expect.sequenceEqual
                (defaultsByBinding |> Map.find "DataRange.Between.value1")
                [| "20251104" |]
                "endpoint document should preserve tail tuple item default 1"
            Expect.sequenceEqual
                (defaultsByBinding |> Map.find "DataRange.Between.value2")
                [| "20251104" |]
                "endpoint document should preserve tail tuple item default 2"

            let rec flattenField (field: ArguFormField) =
                seq {
                    yield field

                    for item in field.Items do
                        yield! flattenField item
                }

            let schemaField unionCaseName fieldName =
                reply.Document.ArguFormSchema.UnionCases
                |> Seq.find (fun unionCase -> unionCase.Name = unionCaseName)
                |> fun unionCase -> unionCase.Fields |> Seq.collect flattenField
                |> Seq.find (fun field -> field.Name = fieldName)

            let pfcfgtcconfItem = schemaField "PFCFGTCCONF" "valueItem"
            Expect.equal pfcfgtcconfItem.Kind "text" "Argu list item schema should render as text, not dropdown/select"
            Expect.isEmpty pfcfgtcconfItem.Options "Argu list item schema should not carry enum options that imply dropdown lock-in"
            Expect.contains
                (schemaField "DataRange" "ReferenceDateMode.value").Options
                "ModeAccountingDate"
                "endpoint schema should append canonical tail enum defaults so frontend select can preserve raw casing"

            let rec fieldValues unionCaseName (field: ArguFormField) =
                let collectByPrefix () =
                    let prefix =
                        let fieldName = field.Name
                        let dotIndex = fieldName.LastIndexOf(".", StringComparison.Ordinal)

                        if dotIndex > 0 then
                            $"{unionCaseName}.{fieldName.Substring(0, dotIndex + 1)}"
                        else
                            $"{unionCaseName}."

                    defaultEntriesInOrder
                    |> Array.choose (fun (binding, values) ->
                        if values.Length > 0 && binding.StartsWith(prefix, StringComparison.Ordinal) then
                            Some values
                        else
                            None)
                    |> Seq.concat
                    |> Seq.toArray

                match defaultsByBinding |> Map.tryFind $"{unionCaseName}.{field.Name}" with
                | Some values when values.Length > 0 -> values
                | _ ->
                    let itemValues = field.Items |> Array.collect (fieldValues unionCaseName)

                    if itemValues.Length > 0 then
                        itemValues
                    else
                        collectByPrefix ()

            let rawForUnionCase (unionCase: ArguFormUnionCase) =
                { DuTypeName = reply.Document.ArguFormSchema.DuTypeName
                  UnionCaseName = unionCase.Name
                  Fields =
                    unionCase.Fields
                    |> Array.map (fun field ->
                        { Name = field.Name
                          Values = fieldValues unionCase.Name field }) }
                |> SubmitArguFormCodec.buildRawArgu reply.Document.ArguFormSchema

            let rebuilt =
                reply.Document.ArguFormSchema.UnionCases
                |> Array.map rawForUnionCase
                |> Array.filter (not << String.IsNullOrWhiteSpace)
                |> String.concat " "

            Expect.equal rebuilt pfcfDataRangeExpectedRaw "frontend-style full-form raw Argu should preserve datarange tail ordering"

        testCase "DYN-T-512: Resolve endpoint should render only parsed partial arg-string cases" <| fun _ ->
            let registration =
                DynamicArguTemplateRegistration.fromTemplate<PFCF_AKKA_CMD>
                    DynamicArguAliasBinding.empty
                    (Some pfcfDataRangeExpectedRaw)

            let request: DynamicArguResolveTargetRequest =
                { Keys = [| "akka://pfcf"; pfcfDuTypeName; pfcfPartialExpectedRaw |] }

            let requestJson = JsonSerializer.Serialize(request, ArguFormSchema.jsonOptions)
            let replyJson = DynamicArguResolveEndpoint.handle [ registration ] requestJson
            let reply = JsonSerializer.Deserialize<DynamicArguResolveTargetReply>(replyJson, ArguFormSchema.jsonOptions)

            Expect.isTrue reply.Ok reply.Error
            Expect.equal reply.CanonicalArgString pfcfPartialExpectedRaw "endpoint should preserve partial canonical arg string"
            Expect.sequenceEqual
                (reply.Document.ArguFormSchema.UnionCases |> Array.map _.Name)
                [| "PFCFEDX"; "PFCFGTCCONF" |]
                "partial arg string should not render unparsed union cases"
            Expect.sequenceEqual
                (reply.Document.Nodes |> Array.map _.Id)
                [| "case-pfcfedx"; "case-pfcfgtcconf" |]
                "partial Form DSL nodes should follow parsed case order only"

            let rec flattenNode (node: SduiFormNode) =
                seq {
                    yield node
                    for child in node.Children do
                        yield! flattenNode child
                    for item in node.Items do
                        yield! flattenNode item
                }

            let defaultsByBinding =
                reply.Document.Nodes
                |> Seq.collect flattenNode
                |> Seq.filter (fun node -> not (String.IsNullOrWhiteSpace node.Binding))
                |> Seq.map (fun node -> node.Binding, node.DefaultValues)
                |> Map.ofSeq

            Expect.sequenceEqual
                (defaultsByBinding |> Map.find "PFCFEDX.mode")
                [| "trivial" |]
                "partial form should keep PFCFEDX string default"
            Expect.sequenceEqual
                (defaultsByBinding |> Map.find "PFCFGTCCONF.value")
                [| "OIInf"; "TAIFEX" |]
                "partial form should keep PFCFGTCCONF list defaults only"
    ]

[<EntryPoint>]
let main argv =
    runTestsWithCLIArgs [] argv tests
