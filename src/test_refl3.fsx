
#I "bin/Debug/net10.0"
#r "PulseTrade.Comm.Spa.Dynamic.dll"
#r "nuget: Akka"
#r "nuget: PulseTrade.Comm.Spa, 0.2.4-beta8"

open PulseTrade.Comm.Spa
open System.Net.Http

let hub = CommHub.create()
let asset: ClientExtensionScriptAsset = {
    Url = "/ext/js/PulseTrade.Comm.Spa.Dynamic.js"
    ContentType = "application/javascript"
    Content = "console.log(\"hello\");"
}
hub.RegisterClientExtensionScriptAsset asset |> ignore

let options = ServerOptions.defaults |> ServerOptions.withWebBinding (WebBinding.fixedPort 13333)
let server = Server.start { options with Hub = hub }

let js = (new HttpClient()).GetStringAsync("http://127.0.0.1:13333/ext/js/PulseTrade.Comm.Spa.Dynamic.js").Result
System.IO.File.WriteAllText("test_refl3.js", js)
server.Stop()

