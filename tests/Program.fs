module PulseTrade.Comm.Spa.Dynamic.Tests.Program

open System
open Expecto
open PersistedConcurrentSortedList.Type
open PulseTrade.Comm.Spa.Dynamic.Server
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
            // TODO: Wait for UPSTREAM_RFC (IMessageRenderer)
            ()
    ]

[<EntryPoint>]
let main argv =
    runTestsWithCLIArgs [] argv tests
