# PulseTrade.Comm.Spa.Login

Open PTCS Login extension facade.

This package provides the final NuGet boundary for PTCS local-login wiring while
the closed packages `PulseTrade.Comm.Spa` and `PulseTrade.Comm.Login.Core` retain
the runtime contracts, token/session lifecycle, and credential verification.

```fsharp
#r "nuget: PulseTrade.Comm.Spa"
#r "nuget: PulseTrade.Comm.Login.Core"
#r "nuget: PulseTrade.Comm.Spa.Login"

open PulseTrade.Comm.Spa
open PulseTrade.Comm.Spa.Login

let options =
    ServerOptions.localRandom()
    |> PtcsLoginExtension.usePtcsLogin loginOptions
```

`PulseTrade.Comm.Spa.Login` must reference `PulseTrade.Comm.Login.Core` as an exact
binary NuGet package. It must not require Core source access.
