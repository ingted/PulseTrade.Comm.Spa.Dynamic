# PulseTrade.Comm.Spa.ACL

Open PTCS ACL extension package.

This package is still small in the current extraction slice. The closed packages
`PulseTrade.Comm.Spa` and `PulseTrade.Comm.ACL.Core` still own the runtime contracts
and evaluator implementation. This open package gives NuGet consumers a stable final
boundary for ACL wiring and now also owns the browser extension manifest/script
registration used by PTCS:

```fsharp
#r "nuget: PulseTrade.Comm.Spa"
#r "nuget: PulseTrade.Comm.ACL.Core"
#r "nuget: PulseTrade.Comm.Spa.ACL"

open PulseTrade.Comm.Spa
open PulseTrade.Comm.Spa.ACL

let options =
    ServerOptions.localRandom()
    |> PtcsAclExtension.useAcl aclOptions
```

`PtcsAclExtension.useAcl` performs two actions:

- registers `PtcsAclOptions` into the PTCS server runtime by calling the PTCS SPI;
- registers extension id `pulse-trade-comm-spa-acl`, script url
  `/client-extensions/acl/PulseTrade.Comm.Spa.ACL.js`, and the package `contentFiles`
  script asset in the PTCS client-extension manifest.

This is not yet the final extraction of every ACL-owned browser behavior from PTCS
core. The current package proves the open NuGet boundary, manifest/script ownership,
and ACL2 browser/runtime gate. Remaining work is moving the ACL client logic that
still lives in PTCS core into this package.

`PulseTrade.Comm.Spa.ACL` must reference `PulseTrade.Comm.ACL.Core` as an exact
binary NuGet package. It must not require Core source access.
