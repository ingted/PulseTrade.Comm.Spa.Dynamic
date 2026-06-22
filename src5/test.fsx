#r "C:/Users/Administrator/.nuget/packages/websharper.ui/10.1.4.674/lib/net10.0/WebSharper.UI.dll"
let m = typeof<WebSharper.UI.Doc>.GetMethod("Element")
printfn "%A" m
