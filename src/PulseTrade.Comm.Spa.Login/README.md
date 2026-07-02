# PulseTrade.Comm.Spa.Login

Open PTCS Login extension package.

This package provides the final NuGet boundary for PTCS local-login wiring. The
closed packages `PulseTrade.Comm.Spa` and `PulseTrade.Comm.Login.Core` retain the
host SPI and token/session lifecycle contracts, while this open package owns the
PTCS local-login route/page glue, `BrowserAuthProvider` adapter, and browser
extension manifest/script registration used by PTCS:

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

- creates a PTCS `BrowserAuthProvider` from this package's `PtcsLogin.provider`
  and installs it through `Server.withBrowserAuth`;
- registers extension id `pulse-trade-comm-spa-login`, script url
  `/client-extensions/login/PulseTrade.Comm.Spa.Login.js`, and the package
  `contentFiles` script asset in the PTCS client-extension manifest;
- registers the WebSharper runtime asset required by the package bundle at
  `/client-extensions/login/WebSharper.Core.JavaScript/Runtime.js`;
- registers a login page renderer through `PulseTradeRegisterLoginRenderer`.

`PtcsLogin` in this package supplies:

- `fromLoginCore`;
- `coreFromConfigWithDependencies`;
- `localDevDependenciesWithSessionStore`;
- `demoLocalDevWithSessionStore`;
- `demoLocalDev`;
- `/login`, `/login/api/submit`, `/login/api/session`, `/login/logout`,
  `/chat/login`, and `/chat/logout` route composition;
- HttpOnly SameSite session cookie handling and session-to-principal resolution.

PTCS core still keeps a transitional fallback `PtcsLogin` implementation for
closed-package compatibility. New open-extension consumers should prefer
`PulseTrade.Comm.Spa.Login.PtcsLogin` from this package and mount it with
`PtcsLoginExtension.usePtcsLogin`. Remaining extraction work is public 81/82
redeployment on the extracted package set and any follow-up cleanup that removes
dead fallback code from PTCS core after downstream users have migrated.

`PulseTrade.Comm.Spa.Login` must reference `PulseTrade.Comm.Login.Core` as an exact
binary NuGet package. It must not require Core source access.
