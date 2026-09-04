# PulseTrade.Comm.Spa.Dynamic.Ptcs

PTCS-specific adapter for the transport-neutral Dynamic Contracts package.

- Exact PTCS dependency: `PulseTrade.Comm.Spa 0.2.5-beta116`.
- `TaResearchTransientServer.register` maps PTCS session context and typed `RuntimeClientFrame` to a host backend, applies the canonical reducer, and returns either the compatibility wire or bounded `ta-browser.v4` columnar state。
- v4對homogeneous temporal series使用bounded parallel arrays保存source interval、scale、observed/available frontier、finality、projection與quality；shape不一致或invalid metadata fail closed。
- Bounded state includes rows/series、query identity/range plus freshness label、watermark、quality、lag、reason與recoverable error；不把recursive `SduiValue` graph帶入browser bundle。
- Add Row的`rowKind`在wire上使用canonical lowercase；server parser同時case-insensitive，避免F# union case casing被錯判為Candlestick。
- Transient runtime data does not enter PTCS page/chat history; the host backend remains authoritative for provider queries and ACL policy.
- Full snapshot/reconnect每series最多2000 points；stable delta最多200。empty-to-first-data必須輸出authoritative full。
- explicit `RequestFullSnapshot`一律回authoritative full wire，供離線JSON export；不得因current/next revision相同退化成空delta。
- Current exact package：`PulseTrade.Comm.Spa.Dynamic.Ptcs 0.1.0-alpha7-win50`。
