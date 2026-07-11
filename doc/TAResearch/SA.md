# SA-PTCS-DYNAMIC-TA-0001 Realtime TA Canvas Runtime

Status: Proposed / Review required
Date: 2026-07-11
REQ: `doc/TAResearch/REQ.md`
RFC: `doc/RFC/RFC-PTCS-DYNAMIC-0007.realtime-ta-canvas-runtime.md`
DSL: `doc/SDUI_DSL_zh-Hant.md`

## 1. 系統目的

`PulseTrade.Comm.Spa.Dynamic` 應把 provider 回傳的 typed SDUI document/render frames 呈現為可互動 Canvas。它不擁有 market-data provider、SQL query、broker session或PTCS authentication；它只擁有 contract、browser state reducer、renderer與lifecycle。

本需求不是在現有static Canvas多加一種chart node。TA研究畫面需要把三種生命週期分開：

1. document：固定layout、row identity、initial query/view defaults；
2. data：history snapshot與後續incremental patches；
3. view：zoom、pan、crosshair、toggle等browser-local state。

## 2. 現況盤點

PTCS core evidence inspected read-only at repo root `G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa`, branch `20260707_022.win.PTC_rw.Beta_Agy`, commit `114324be526db2caaccef7ac611301277b92f71d`。該checkout有既有dirty files，本輪未修改；下表的PTCS `Client.fs`判讀適用於此current source snapshot。

| 現況 | 證據位置 | 影響 |
| --- | --- | --- |
| Canvas以單一reply建立summary/overlay，沒有stable runtime instance | `src/Client/DynamicRenderer.fs` | 每次完整reply會重建畫面並失去view state。 |
| DSL文件列出`RealtimeChart`，renderer沒有對應case | `doc/SDUI_DSL_zh-Hant.md`、`src/Client/DynamicRenderer.fs` | 文件名詞不是已交付能力。 |
| Button與Tree仍含placeholder行為 | `src/Client/DynamicRenderer.fs` | 不能作為TA toolbar/tree的正式互動基礎。 |
| 目前renderer大量以`JS.Inline`讀dynamic object或註冊global callback | `src/Client/DynamicRenderer.fs`、`src/Client/ArguFormRenderer.fs` | 新runtime不能延續此模式；需typed codec與WebSharper API。 |
| Argu Form已有submit callback概念 | `src/Client/ArguFormRenderer.fs` | 可借用command callback精神，但不可每5秒append durable form/history event。 |
| PTCS目前Canvas seam本質為`string -> Node option` | `G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\Client.fs` renderer registry | 沒有mount/unmount、authenticated duplex channel、transient frame或dispose contract。 |

## 3. 關鍵差異

單一`RealtimeChart` node無法解決以下問題：

- layout與data revision無法分辨；
- 5秒更新會累積message card、journal與IndexedDB row；
- duplicate、gap、out-of-order frame無法deterministic處理；
- close page後timer/socket subscription無法由renderer registry回收；
- zoom/pan/toggle與server query state會互相覆蓋；
- provider error可能把已成功建立的Canvas整個替換成錯誤文字。

因此系統邊界必須從「render one payload」提升為「mount one runtime document and reduce bounded frames」。

## 4. 目標架構

```text
PTCS.Host TA actor
  -> Dynamic.Contracts document/snapshot/patch JSON
  -> PTCS authenticated WebSocket extension channel
  -> Dynamic typed codec
  -> CanvasRuntimeRegistry
  -> SduiRuntimeReducer
  -> TaCanvasRenderer (pure WebSharper)

browser action
  -> PTCS target command callback
  -> PTCS.Host TA actor
  -> snapshot/patch/error
```

### 4.1 Package boundary

| Package/module | Responsibility | Forbidden dependency |
| --- | --- | --- |
| `PulseTrade.Comm.Spa.Dynamic.Contracts` | DTO、DU、strict codec、limits、revision rules | WebSharper browser runtime、PTMD、SQL |
| `SduiRuntimeReducer` | pure state transition與resync decision | DOM、network |
| `CanvasRuntimeRegistry` | instance/mount/dispose/in-flight lifecycle | provider-specific logic |
| `TaViewport` | zoom/pan/crosshair/toggle state | server query execution |
| `TaCanvasRenderer` | SVG/layout/controls/status | raw JavaScript、SQL |
| `TaCanvasTransport` | 經PTCS提供的authenticated channel送poll/action並收frame | 自建OAuth、arbitrary HTTP |

`PTCS.Host`只reference輕量Contracts package；Dynamic不reference PTMD。所有release-facing reference使用exact NuGet version，不以ProjectReference串接三套系統。

### 4.2 Runtime state

```text
CanvasRuntimeState =
  DocumentIdentity
  DocumentRevision
  DataRevision
  LastTransportSequence
  InitialQuery
  CurrentQuery
  InitialView
  CurrentView
  Rows
  SeriesByDataRef
  BackendStatus
  PollState
```

Reducer只接受base revision正確且sequence連續的frame。duplicate為no-op；gap、unknown instance、base mismatch產生`RequestResync` effect，不直接猜測合併。

### 4.3 Data與view ownership

| State | Owner | Network |
| --- | --- | --- |
| Candles/indicator series、quality、watermark | server frame | snapshot/patch |
| source/symbol/scale/range/indicator parameters | server-validated query | user action/poll |
| zoom/pan/crosshair/legend toggle/row visibility | browser | none |
| document layout與initial defaults | document | explicit replace/reset only |

## 5. PTCS upstream seam

Dynamic不能可靠地從現有renderer callback自行取得PTCS session socket，也不能自行開一條無ACL context的WebSocket。PTCS companion RFC至少需提供：

```text
RealtimeCanvasMountContext
  PageId / KeyId / Caller / Capabilities
  SubmitTargetCommand
  OpenTransientChannel
  OnFrame
  OnDispose
```

同一authenticated WebSocket可multiplex durable user actions與transient poll frames，但兩者projection policy不同：

- Add Row、remote parameter change、Reset Canvas：可audit，不必成為chat message；
- poll/heartbeat/patch：transient，不append journal/PCSL message history；
- query response與ACL decision仍由server authoritative。

PTCS companion seam尚未accepted前，Dynamic可完成Contracts/reducer/renderer pure tests，但不得以HTTP polling、global socket或history append聲稱E2E完成。

## 6. Bounded resource policy

初始建議值，進SD/Test時可在不放寬安全上限的前提下調整：

| Limit | Proposed default |
| --- | ---: |
| TA rows per canvas | 8 |
| Initial bars per row | 5000 |
| Browser retained bars per row | 2000 |
| Patch items per frame | 500 |
| Poll interval | 5 seconds minimum/default |
| Concurrent poll per canvas | 1 |

server可回更嚴格限制。超限必須保留已成功Canvas並呈現controlled error/status，不清空FormInput或整個runtime。

## 7. Interaction分析

- zoom/pan以shared time range驅動所有rows，避免price與indicator時間軸錯位；
- legend toggle與row visibility只改local state；
- Add Row或scale/range change送typed target action，等待server snapshot/patch；
- `ResetView`恢復initial viewport/toggle，不query server；
- `ResetCanvas`恢復initial query/rows/view並要求fresh snapshot；
- hidden tab、collapsed Canvas、socket unavailable、unmounted都停止poll；
- reopen後以last known revision要求delta，server不能補齊時回snapshot。

## 8. Failure model

| Failure | Required behavior |
| --- | --- |
| invalid document/schema/node | controlled error，拒絕mount |
| indicator/provider query error | 保留last good Canvas，status標示error/stale |
| sequence gap/revision mismatch | pause patch apply，request resync |
| duplicate frame | no-op，保留state |
| socket disconnect | pause poll，顯示disconnected；重連後resync |
| unknown Canvas instance | fail closed，不建立implicit instance |
| unsupported TA kind | controlled error，不畫空白row |

## 9. e2eQuotation可重用知識

Reference source: PTC worktree `G:\PulseTrade.worktrees\20260710-rn-dual-route\ptc`, branch `20260710_025.rn_durableproxy_dual_route`, base commit `bf821a1e67286960bc2702292067b0992e187e18`, path `Sinopac\demo\e2eQuotation`。此專用app的行為與測試適合作為TA Canvas分析來源，但不是Dynamic runtime dependency。

| Existing slice | Reuse in Dynamic | Do not copy |
| --- | --- | --- |
| `SpaModel.fs`的bounded TA model與projection | row/series DTO與pure reducer測試精神 | e2e專用page state與source assumptions |
| `Client.fs`的shared viewport、crosshair、hover KV、visible-bar bound | Canvas local view state與interaction acceptance | inline/raw JavaScript helper |
| Historical TA IndexedDB/read-through/prefetch tests | bounded cache、inactive surface不投影、in-flight guard | 與e2e page key綁死的cache schema |
| `Server.fs`的historical viewport、coverage與provider strategy | snapshot/coverage/status語義 | Dynamic直接呼叫provider或backfill |
| `subscribePush`/PubSub delta | 驗證push可行與socket lifecycle edge cases | 本需求的預設transport；TA Canvas仍採5秒client-pull |
| Playwright crosshair/scroll/cache-hit cases | 新TA Canvas E2E基線 | fake-only smoke作production acceptance |

Dynamic需重用其state separation與人類操作驗收，不把e2eQuotation變成package dependency，也不複製`Server.fs`內的fallback JavaScript UI。

## 10. 測試分析

1. Contracts：strict decode/encode、unknown field/operation policy、bounds。
2. Reducer：document -> snapshot -> patch、duplicate、gap、out-of-order、reset、resync。
3. Component：每種TA kind、shared viewport、toggle、resize、last-good-state error。
4. Lifecycle：mount/hidden/collapse/unmount/reconnect，確認timer與subscription釋放。
5. E2E：以PTMD deterministic fixture加live-tail adapter，Playwright操作zoom/pan/add-row/reset並確認20次poll不增加history row。

## 11. 結論

採用immutable document + typed bounded runtime frames。歷史snapshot是主要研究資料；5秒poll只更新tail，不能主導layout。此方案需要PTCS core提供authenticated transient WebSocket lifecycle seam，並由PTCS.Host/ PTMD分別負責domain query與storage，才能維持Dynamic作為通用NuGet extension的邊界。
