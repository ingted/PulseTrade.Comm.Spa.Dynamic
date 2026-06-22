
#I "bin/Debug/net10.0"
#r "PulseTrade.Comm.Spa.Dynamic.dll"
#r "nuget: Akka"
open PulseTrade.Comm.Spa.Dynamic.Server
let asm = typeof<ShowcaseDemoActor>.Assembly
printfn "DLL Path: %s" asm.Location
let dir = System.IO.Path.GetDirectoryName(asm.Location)
printfn "Dir: %s" dir
let jsPath = System.IO.Path.Combine(dir, "wwwroot", "js", "PulseTrade.Comm.Spa.Dynamic.js")
printfn "JS Path: %s" jsPath
printfn "Exists: %b" (System.IO.File.Exists(jsPath))

