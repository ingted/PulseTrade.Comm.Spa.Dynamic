# @DYN-TA-004 Host adapters, parity and release

Status: Active / 38%

## Deliverables

1. PTCS compatibility adapter over authenticated transient seam。
2. E2EQ adapter and parallel feature path using shared Renderer。
3. Cross-host Playwright parity and bounded poll/resource tests。
4. Static compatibility/DSL sync, exact NuGet release and downstream bump。

## 2026-07-11 server adapter alpha2

- 新增packable `PulseTrade.Comm.Spa.Dynamic.Ptcs 0.1.0-alpha2`，exact references：PTCS beta82、Contracts alpha4。
- `TaResearchTransientServer.register/createHandler`把server-derived PTCS session context與typed `RuntimeClientFrame`交給host backend，驗證`RuntimeFrame`後使用canonical `RuntimeReducer`，回browser-neutral full `RuntimeState` wire。
- reducer state以`sessionId + extensionId + channelId`隔離；disconnect移除；invalid JSON/unsupported frame fail closed。
- exact-package tests 3/3 pass：nested SDUI wire、document/snapshot reducer、same channel cross-session isolation、invalid payload、disconnect cleanup。
- 未完成browser adapter。WebSharper 10.1.5在recursive generic `SduiValue` browser wire與PTCS beta81/82 metadata dependency merge時會讓`wsfsc.exe`無診斷結束。下一slice改採bounded non-recursive TA-specific browser wire；禁止raw JavaScript/HTTP polling。
- Legacy `PulseTrade.Comm.Spa.Dynamic`仍exact PTCS beta80且不reference新server adapter，避免破壞現有Bundle。PTCS.Host後續直接reference`Dynamic.Ptcs alpha2`；待browser adapter穩定後再決定facade delegation。

## Acceptance

`DYN-TA-T-007/010/013/014/016/017/019/020` pass on real host paths；fixture-only evidence is insufficient。
