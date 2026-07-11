# PulseTrade.Comm.Spa.Dynamic.Ptcs.Client

Pure WebSharper F# client adapter for the PTCS same-session transient channel.

## Boundary

- Connects only to the current origin `/sync/ws`; callers cannot inject an upstream URL or credential.
- Sends `extension-transient` frames with `ta-browser.v1` payloads.
- Projects bounded, non-recursive browser wire data into `RuntimeState` and renders through `PulseTrade.Comm.Spa.Dynamic.Renderer`.
- Does not own PTCS authentication, ACL, SQL, market-data access, persistence, or the canonical reducer.

## API

```fsharp
TaResearchTransientClient.mountById
    "app"
    "ta-research"
    "workspace-main"
    "canvas-main"
```

The PTCS host must register the same `extensionId` with `PulseTrade.Comm.Spa.Dynamic.Ptcs.TaResearchTransientServer.register`.
