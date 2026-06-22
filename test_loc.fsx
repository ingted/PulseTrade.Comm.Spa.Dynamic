open System
open System.Reflection

let dir = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
printfn "Dir: %s" dir
