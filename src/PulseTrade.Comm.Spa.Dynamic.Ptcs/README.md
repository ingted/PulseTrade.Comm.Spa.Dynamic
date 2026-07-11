# PulseTrade.Comm.Spa.Dynamic.Ptcs

PTCS-specific adapter for the transport-neutral Dynamic Contracts package.

- Exact PTCS dependency: `PulseTrade.Comm.Spa 0.2.5-beta82`.
- `TaResearchTransientServer.register` maps PTCS session context and typed `RuntimeClientFrame` to a host backend, applies the canonical reducer, and returns either the compatibility wire or bounded `ta-browser.v1` state。
- Bounded state includes rows/series plus freshness label、watermark、quality、lag、reason與recoverable error；不把recursive `SduiValue` graph帶入browser bundle。
- Transient runtime data does not enter PTCS page/chat history; the host backend remains authoritative for provider queries and ACL policy.
- Current exact package：`PulseTrade.Comm.Spa.Dynamic.Ptcs 0.1.0-alpha4`。
