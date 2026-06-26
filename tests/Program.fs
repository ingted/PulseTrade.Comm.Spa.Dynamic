module PulseTrade.Comm.Spa.Dynamic.Tests.Program

open System
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
    | PFCFEDX
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
            Expect.equal tags "--tag aoe --tag \"marvel now\"" "list should repeat the same Argu parameter"
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
            | Ok(ArguTemplateTarget(actor, duTypeName, unionCases)) ->
                Expect.equal actor actorAddress "DU target should preserve actor address"
                Expect.equal duTypeName pfcfDuTypeName "DU target should resolve type name"
                Expect.sequenceEqual unionCases [ "SimpleAction"; "BBA"; "GenByColMeta" ] "DU target should preserve union case tail order"
            | other -> failwithf "DU target should resolve. got=%A" other

            Expect.isError (DynamicTargetKey.tryResolve metadata [ actorAddress; "missing.dsl" ]) "unknown direct/form discriminator should fail"
            Expect.isError (DynamicTargetKey.tryResolve metadata [ actorAddress; pfcfDuTypeName; "MissingCase" ]) "unknown union case should fail"
            Expect.isError (DynamicTargetKey.tryResolve metadata [ actorAddress; document.DocumentId; "SimpleAction" ]) "direct DSL target should reject union-case tail"

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
                "--pfcfgtc gf --pfcfgtc goi"

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
                "--parentchilds 7 --parentchilds 9"

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
                "--tablename Orders --tablename \"Positions Today\""
    ]

[<EntryPoint>]
let main argv =
    runTestsWithCLIArgs [] argv tests
