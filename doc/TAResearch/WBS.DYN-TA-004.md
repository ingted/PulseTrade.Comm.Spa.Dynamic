# @DYN-TA-004 Host adapters, parity and release

Status: Active / 74%

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

## 2026-07-11 bounded browser adapter slice

- `PulseTrade.Comm.Spa.Dynamic.Ptcs 0.1.0-alpha3`新增bounded、non-recursive `ta-browser.v1` command/state wire；同一handler按明確`wireVersion`分流，legacy recursive wire仍保留。exact-package tests 4/4通過。
- `PulseTrade.Comm.Spa.Dynamic.Ptcs.Client 0.1.0-alpha2`提供pure WebSharper `mountById`：只連same-origin `/sync/ws`，送typed transient action，把bounded wire投影為Renderer的`RuntimeState`。不接受URL、credential、raw handler或HTTP polling。
- client exact-package model tests 2/2通過：candlestick/status/poll projection與typed query action mapping。
- WebSharper 10.1.5.674的stale `wsfscservice`會讓後續`wsfsc.exe`以`-532462766`無診斷退出；停止22:33起持續存活的helper後，canonical `src/PulseTrade.Comm.Spa.Dynamic.Ptcs.Client`正常build。pack使用已驗證的build後`dotnet pack --no-build`，不縮短project path或關閉compiler。
- 未完成：真PTCS host bundle掛載、server ack/in-flight UI、poll timer/reconnect/resync、500 bars/20 polls、desktop/mobile Playwright。以上仍阻擋DYN-TA-004完成，不可用model tests替代。

## 2026-07-12 lifecycle and bounded status slice

- `PulseTrade.Comm.Spa.Dynamic.Ptcs 0.1.0-alpha4`與`Ptcs.Client 0.1.0-alpha5`使用exact package refs；未public push，local nupkg已放SDK 10.0.301 library-packs。
- bounded browser state wire補齊`watermarkUtc/quality/lagSeconds/reasonCode`，server/client exact-package tests各`4/4`通過；renderer不再只能看到label/freshness。
- `TaClientLifecycle`以pure typed transition實作connected/mounted、one-in-flight action/poll、timeout/backoff、active suspension、bounded reconnect、full-snapshot resync與terminal dispose；actual WebSharper client interpreter使用typed`WebSocket`與`JS.SetTimeout/ClearTimeout`，無raw JavaScript或HTTP polling。
- `mountByIdWithOptions`回傳`TaResearchTransientClientHandle`，提供runtime state、`SetActive`與`Dispose`；既有`mountById`保留並使用defaults。
- canonical F# build與一次full WebSharper compile均通過；lifecycle tests驗證第二個poll不送、retry保留last-good revision、inactive取消timer、resync送full snapshot、disposed不再reconnect。
- 尚未完成：client bundle掛入真PTCS shell、WebSocket斷線/host restart browser證據、500 bars/20 polls、history/DOM/timer bounded observation及desktop/mobile真host Playwright。

## Acceptance

`DYN-TA-T-007/010/013/014/016/017/019/020` pass on real host paths；fixture-only evidence is insufficient。
