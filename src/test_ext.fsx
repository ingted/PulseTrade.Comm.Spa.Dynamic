
#I "bin/Debug/net10.0"
#r "PulseTrade.Comm.Spa.Dynamic.dll"
#r "nuget: Akka"
#r "nuget: PulseTrade.Comm.Spa, 0.2.4-beta8"

open PulseTrade.Comm.Spa

let hub = CommHub.create()
hub.RegisterClientExtension { 
    ExtensionId = "test"
    DisplayName = Some "test"
    ScriptUrls = ["http://127.0.0.1:23456/test.js"; "data:application/javascript;base64,aaa"; "/js/test.js"]
    AppendPageShapes = []
} |> ignore
printfn "Extensions: %A" (hub.ListClientExtensions())

