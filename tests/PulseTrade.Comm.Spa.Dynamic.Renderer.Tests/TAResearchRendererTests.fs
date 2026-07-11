module PulseTrade.Comm.Spa.Dynamic.Renderer.Tests

open System
open Expecto
open PulseTrade.Comm.Spa.Dynamic.Contracts
open PulseTrade.Comm.Spa.Dynamic.Renderer

let candle timestamp openValue high low close volume =
    SduiValue.Object(
        Map [
            "t", SduiValue.Text timestamp
            "o", SduiValue.Number openValue
            "h", SduiValue.Number high
            "l", SduiValue.Number low
            "c", SduiValue.Number close
            "v", SduiValue.Number volume
        ])

let tests =
    testList "TA renderer model" [
        testCase "typed candle series rejects malformed items" <| fun _ ->
            let data =
                Map [
                    "price",
                    SduiValue.Array
                        [| candle "B1" 10.0 12.0 9.0 11.0 100.0
                           SduiValue.Object(Map [ "t", SduiValue.Text "missing-fields" ]) |]
                ]

            let actual = RendererModel.candleSeries "price" data
            Expect.equal actual.Length 1 "only the complete typed candle should survive"
            Expect.equal actual[0].Close 11.0 "close must preserve transport value"

        testCase "visible window clamps count and start" <| fun _ ->
            let actual =
                RendererModel.clampWindow 12 160 96 { StartIndex = 90; Count = 48 }

            Expect.equal actual.Count 48 "requested count remains when bounded"
            Expect.equal actual.StartIndex 48 "start clamps so the window ends at the series tail"

        testCase "visible window handles short series" <| fun _ ->
            let actual =
                RendererModel.clampWindow 12 160 5 { StartIndex = 20; Count = 48 }

            Expect.equal actual { StartIndex = 0; Count = 5 } "short data renders all available points"

        testCase "select window is deterministic" <| fun _ ->
            let actual = RendererModel.selectWindow { StartIndex = 2; Count = 3 } [| 0; 1; 2; 3; 4; 5 |]
            Expect.sequenceEqual actual [| 2; 3; 4 |] "window must preserve ordering"

        testCase "normalization keeps higher value visually above lower" <| fun _ ->
            let lowY = RendererModel.normalize 10.0 20.0 5.0 100.0 10.0
            let highY = RendererModel.normalize 10.0 20.0 5.0 100.0 20.0
            Expect.isLessThan highY lowY "SVG y decreases as value rises"

        testCase "renderer package remains host neutral" <| fun _ ->
            let assembly = typeof<TaRendererOptions>.Assembly
            let dependencies = assembly.GetReferencedAssemblies() |> Array.map _.Name |> Set.ofArray
            for forbidden in [ "PulseTrade.Comm.Spa"; "FAkka.FCell2"; "PulseTrade.MarketData"; "Microsoft.Data.SqlClient" ] do
                Expect.isFalse (Set.contains forbidden dependencies) ("forbidden renderer dependency: " + forbidden)

        testCase "renderer source contains no JavaScript escape hatch" <| fun _ ->
            let source =
                IO.File.ReadAllText(IO.Path.Combine(__SOURCE_DIRECTORY__, "..", "..", "src", "PulseTrade.Comm.Spa.Dynamic.Renderer", "Renderer.fs"))

            for forbidden in [ "JS.Inline"; "JavaScriptExport"; "<script"; "eval(" ] do
                Expect.isFalse (source.Contains(forbidden, StringComparison.Ordinal)) ("forbidden source escape hatch: " + forbidden)
    ]

[<EntryPoint>]
let main args = runTestsWithCLIArgs [] args tests
