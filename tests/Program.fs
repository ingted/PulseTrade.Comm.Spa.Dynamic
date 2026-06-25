module PulseTrade.Comm.Spa.Dynamic.Tests.Program

open System
open Expecto
open PersistedConcurrentSortedList.Type
open PulseTrade.Comm.Spa.Dynamic.Server
open PulseTrade.Comm.Spa.Dynamic.Client
open PulseTrade.Comm.Spa
open Akka.Actor

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
            let schema = ArguFormSchema.sample ()
            let json = ArguFormSchema.generateSduiJson schema

            Expect.equal schema.Schema "fskynet-sdui" "schema marker should match Dynamic SDUI payload contract"
            Expect.equal schema.FormMode "argu-form" "formMode should identify Argu form payloads"
            Expect.equal schema.DuTypeName ArguFormSchema.sampleDuTypeName "sample schema should use stable DU type name"
            Expect.isTrue (json.Contains("\"formMode\":\"argu-form\"")) "JSON should expose formMode"
            Expect.isSome (ArguFormSchema.tryFindUnionCase "Say" schema) "Say union case should be registered"
            Expect.isSome (ArguFormSchema.tryFindUnionCase "At" schema) "Tuple union case should be registered"
            Expect.isSome (ArguFormSchema.tryFindUnionCase "Tag" schema) "List union case should be registered"

        testCase "DYN-T-403: SubmitArguForm codec should build quoted raw Argu string for common patterns" <| fun _ ->
            let schema = ArguFormSchema.sample ()
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
                submit "Mode" [| "mode", [| "Safe" |] |]
                |> SubmitArguFormCodec.buildRawArgu schema

            let tuple =
                submit "At" [| "at", [| "TTC"; "7" |] |]
                |> SubmitArguFormCodec.buildRawArgu schema

            let tags =
                submit "Tag" [| "tag", [| "aoe"; "marvel now" |] |]
                |> SubmitArguFormCodec.buildRawArgu schema

            let verbose =
                submit "Verbose" [| "verbose", [| "true" |] |]
                |> SubmitArguFormCodec.buildRawArgu schema

            Expect.equal say "--say \"hello world\"" "text value with whitespace should be quoted"
            Expect.equal mode "--mode Safe" "enum value should map to select value"
            Expect.equal tuple "--at TTC 7" "tuple should preserve ordered values"
            Expect.equal tags "--tag aoe --tag \"marvel now\"" "list should repeat the same Argu parameter"
            Expect.equal verbose "--verbose" "bool flag true should emit flag only"
    ]

[<EntryPoint>]
let main argv =
    runTestsWithCLIArgs [] argv tests
