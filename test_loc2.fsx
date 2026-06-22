#r "src/bin/Debug/net10.0/PulseTrade.Comm.Spa.Dynamic.dll"
open PulseTrade.Comm.Spa.Dynamic.Server
let dir = System.IO.Path.GetDirectoryName(typeof<ShowcaseDemoActor>.Assembly.Location)
printfn "Dir: %s" dir
