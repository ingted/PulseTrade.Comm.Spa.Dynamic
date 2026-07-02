# PulseTrade.Comm.Spa.Login

Open PTCS Login extension package.

This package provides the final NuGet boundary for PTCS local-login wiring while
the closed packages `PulseTrade.Comm.Spa` and `PulseTrade.Comm.Login.Core` retain
the runtime contracts, token/session lifecycle, and credential verification. The
current slice also owns the browser extension manifest/script registration used by
PTCS:

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

`PtcsLoginExtension.usePtcsLogin` performs two actions:

- registers `PtcsLoginOptions` into the PTCS server runtime by calling the PTCS SPI;
- registers extension id `pulse-trade-comm-spa-login`, script url
  `/client-extensions/login/PulseTrade.Comm.Spa.Login.js`, and the package
  `contentFiles` script asset in the PTCS client-extension manifest.

This is not yet the final extraction of every Login-owned browser page/client
behavior from PTCS core. The current package proves the open NuGet boundary,
manifest/script ownership, and ACL2 browser/runtime gate. Remaining work is moving
the login page/client logic that still lives in PTCS core into this package.

`PulseTrade.Comm.Spa.Login` must reference `PulseTrade.Comm.Login.Core` as an exact
binary NuGet package. It must not require Core source access.
