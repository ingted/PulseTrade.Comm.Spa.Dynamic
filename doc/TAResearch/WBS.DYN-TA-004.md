# @DYN-TA-004 Host adapters, parity and release

Status: Active / 99%

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

## 2026-07-12 true PTCS live browser slice

- 新增真`CommHub + CommSpaActorFabric + Server.start` host與same-origin extension bundle，不以fake host或HTTP polling代替；PTCS core/runtime與extension asset均由exact packages載入。
- browser wire revision改為JS-safe number，server邊界只接受finite、非負整數後轉回`int64`；fractional revision fail closed。`Dynamic.Ptcs 0.1.0-alpha6-win1` tests 5/5，`Ptcs.Client 0.1.0-alpha7-win4` tests 5/5。
- `Renderer 0.1.0-alpha5`提供500-bar bounded chart、shared cursor、compact timestamp、mobile-safe OHLC wrap、SMA可捲達；model/source tests 11/11。
- `scripts/verify-ptcs-ta-live-playwright.fsx`在desktop/mobile驗證三列、500 bars、20 polls、suspend/resume/dispose、cursor geometry、mobile SMA scroll、PCSL event count 0、console/page error 0。
- close gate發現並修復FAkka.WebSocket/Suave upgraded stream污染：Close response後等待client TCP shutdown，避免續寫`HTTP/1.1 404`；live host使用PTCS beta85 / FAkka.WebSocket win16。
- 尚未完成：host restart中保留last-good並自動resync、present-invalid visual gate、E2EQ adapter與cross-host parity；因此DYN-TA-004維持94%，不宣稱完成。

## Acceptance

`DYN-TA-T-007/010/013/014/016/017/019/020` pass on real host paths；fixture-only evidence is insufficient。

## 2026-07-12 PTCS.Host production query/action integration

- `Renderer 0.1.0-alpha8`移除TXF/5m/固定日期demo literals；query draft由RuntimeDocument `query.*` metadata初始化，poll不覆寫使用者輸入。
- `Dynamic.Ptcs 0.1.0-alpha6-win5`與`Ptcs.Client 0.1.0-alpha7-win9`在bounded `ta-browser.v1`加入query identity/range，並將Add Row kind正規化為lowercase wire vocabulary。
- PTCS.Host真SQL F# Playwright完成FormInput、BTCUSDT/1m query readback、Add SMA Row、Apply、20 polls、desktop/mobile geometry；poll前後PCSL metric相同，console/page error為0。
- package tests：Renderer 12/12、Ptcs server 5/5、Ptcs client 6/6。Remaining：host restart中last-good/resync、E2EQ cross-host parity。

## 2026-07-12 typed Remove/Reset/error/reconnect closure

- Renderer `0.1.0-alpha9` adds a compact row toggle + remove control；remote in-flight disables removal consistently。Add/Remove Row、Change Query and Reset Canvas remain typed `SduiAction` only。
- Ptcs.Client `0.1.0-alpha7-win10` and Host client `0.1.0-alpha8` were published with exact dependency readback from NuGet public registration。
- PTC-VFY-027 now triggers a real one-sided-date error，proves last-good Canvas/FormInput retention and successful recovery，then executes Add/Remove、Apply、Reset、poll。
- A second fresh browser context against the same Host proves channel-scoped sequence bootstrap and reconstructs the document instead of entering `paused-for-resync`。Latest deployed artifact：`G:\PulseTrade.fs.Comm.Log\verification\ptcsHostTaResearchLive\deployed-channel-rebase-20260712204956`。
- Formal Host process restart後相同browser gate通過；Playwright CDP證明20 polls不增加IndexedDB counts。Latest artifact：`G:\PulseTrade.fs.Comm.Log\verification\ptcsHostTaResearchLive\deployed-restart-indexeddb-202607122114`。
- Remaining for this combined detail：E2EQ parallel-path parity與長時cross-host resource observation；PTCS adapter/browser path itself is functionally closed。
