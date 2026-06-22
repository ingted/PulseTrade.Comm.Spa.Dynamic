
#r "src/bin/Debug/net10.0/PulseTrade.Comm.Spa.Dynamic.dll"
open PulseTrade.Comm.Spa.Dynamic.Server
open System.Reflection
let assembly = typeof<ShowcaseDemoActor>.Assembly
printfn "Assembly location: %s" assembly.Location
let dir = System.IO.Path.GetDirectoryName(assembly.Location)
let jsPath = System.IO.Path.Combine(dir, "wwwroot", "js", "PulseTrade.Comm.Spa.Dynamic.js")
printfn "JS Path: %s" jsPath
printfn "File exists: %b" (System.IO.File.Exists(jsPath))

