# PulseTrade.Comm.Spa.ACL

Open PTCS ACL extension facade.

This package is intentionally small in the first extraction slice. The closed packages
`PulseTrade.Comm.Spa` and `PulseTrade.Comm.ACL.Core` still own the runtime contracts
and evaluator implementation. This open package gives NuGet consumers a stable final
boundary for ACL wiring:

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

`PulseTrade.Comm.Spa.ACL` must reference `PulseTrade.Comm.ACL.Core` as an exact
binary NuGet package. It must not require Core source access.
