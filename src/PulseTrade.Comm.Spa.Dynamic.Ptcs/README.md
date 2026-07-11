# PulseTrade.Comm.Spa.Dynamic.Ptcs

PTCS-specific adapter for the transport-neutral Dynamic Contracts package.

- Exact PTCS dependency: `PulseTrade.Comm.Spa 0.2.5-beta82`.
- Alpha2 server slice: `TaResearchTransientServer.register` maps PTCS session context and typed `RuntimeClientFrame` to a host backend, applies the canonical reducer, and returns a browser-neutral full `RuntimeState` wire.
- Browser adapter remains pending. WebSharper 10.1.5 crashes without diagnostics on the recursive generic `SduiValue` wire graph; the next slice will use a bounded non-recursive TA-specific browser wire instead of raw JavaScript or HTTP polling.
- Transient runtime data does not enter PTCS page/chat history; the host backend remains authoritative for provider queries and ACL policy.
