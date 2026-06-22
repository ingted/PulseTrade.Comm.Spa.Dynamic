
#I "bin/Debug/net10.0"
#r "PulseTrade.Comm.Spa.Dynamic.dll"
#r "nuget: Akka"
#r "nuget: PulseTrade.Comm.Spa, 0.2.4-beta8"

open PulseTrade.Comm.Spa

let props = typeof<ClientExtensionScriptAsset>.GetProperties()
for p in props do
    printfn "%s %s" p.PropertyType.Name p.Name

