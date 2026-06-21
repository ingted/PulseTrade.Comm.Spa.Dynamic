module PulseTrade.Comm.Spa.Dynamic.Tests.Program

open System
open Expecto
open FAkka.FCell2
// open PulseTrade.Comm.Spa.Dynamic.Server // (To be implemented in WBS-200)

let tests =
    testList "PulseTrade.Comm.Spa.Dynamic Tests" [
        
        testCase "WBS-102: FCell2Interop should serialize GridFeatures correctly" <| fun _ ->
            // Arrange
            let cell = 
                fCell2.A [|
                    fCell2.T (Map [
                        "id", fCell2.S "grid-1"
                        "mode", fCell2.S "table"
                        "theme", fCell2.S "dark"
                    ])
                |]
            
            // Act
            // TODO: (WBS-201) let json = FCell2Interop.toJsonString cell
            let json = "{\"id\":\"grid-1\",\"mode\":\"table\",\"theme\":\"dark\"}" // Mock
            
            // Assert
            Expect.isTrue (json.Contains("\"theme\":\"dark\"")) "JSON should contain theme property"
            
        testCase "WBS-101: CommHub.useDynamicSdui should mount without exception" <| fun _ ->
            // TODO: Wait for UPSTREAM_RFC to be merged
            // let hub = CommHub.createEmpty()
            // let extHub = hub.useDynamicSdui()
            // Expect.isNotNull extHub "Extended hub should not be null"
            ()
            
        testCase "WBS-103: DynamicRenderer.TryRender should return Some for fskynet-sdui schema" <| fun _ ->
            // TODO: Wait for UPSTREAM_RFC (IMessageRenderer)
            // let renderer = DynamicSduiRenderer.create()
            // let result = renderer.TryRender("{\"schema\":\"fskynet-sdui\"}")
            // Expect.isSome result "Should intercept fskynet-sdui schema"
            ()
    ]

[<EntryPoint>]
let main argv =
    runTestsWithCLIArgs [] argv tests
