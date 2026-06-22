
#I "bin/Debug/net10.0"
#r "PulseTrade.Comm.Spa.Dynamic.dll"
#r "nuget: Akka"
#r "nuget: PulseTrade.Comm.Spa, 0.2.4-beta8"

open PulseTrade.Comm.Spa

let methods = typeof<CommHub>.GetMethods()
for m in methods do
    if m.Name.Contains("Extension") then
        printfn "%A" m

