module PulseTrade.Comm.Spa.Dynamic.Tests.TAResearchContractsTestProgram

open Expecto

[<EntryPoint>]
let main argv =
    runTestsWithCLIArgs [] argv TAResearchContractsTests.tests
