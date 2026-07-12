# SA-PTCS-DYNAMIC-TA-0001 Transport-Neutral Realtime TA Canvas Runtime

Status: Accepted / Ready for DEV
Date: 2026-07-11
REQ: `doc/TAResearch/REQ.md`
RFC: `doc/RFC/RFC-PTCS-DYNAMIC-0007.realtime-ta-canvas-runtime.md`
SD: `doc/TAResearch/SD.md`
Test: `doc/TAResearch/Test.md`
WBS: `doc/TAResearch/WBS.md`
DSL: `doc/SDUI_DSL_zh-Hant.md`

## 1. Analysis conclusion

TA Canvas需求不是多加一個chart node，而是把「單次payload renderer」提升為「可mount、可reduce bounded frames、可dispose的runtime」。為讓E2EQ最終使用同一套畫面，核心runtime不得依賴PTCS session、fCell2或MessageFabric；這些只屬於PTCS adapter。

合理拆分為：

```text
Dynamic.Contracts   = transport-neutral protocol
Dynamic.Renderer    = pure WebSharper state + view runtime
Dynamic facade      = PTCS-specific adapter + compatibility bundle
E2EQ adapter        = E2EQ-specific transport + page integration
```

## 2. Current reality

Canonical source locations：

- Dynamic：`C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic`
- PTCS core：`G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa`
- E2EQ：`G:\PulseTrade.fs\Sinopac\demo\e2eQuotation`

| Reality | Impact |
| --- | --- |
| Canvas renderer以單一reply建立畫面，沒有stable runtime instance | 完整reply會重建view state。 |
| Current extension API在PTCS `CommHub`上註冊 | E2EQ直接reference會帶入不必要的PTCS/fCell2 boundary。 |
| Argu Form已有submit callback概念 | 可抽象成generic action callback，但不能把poll當form/history submit。 |
| Current renderer仍有dynamic/inline JS legacy code | 新TA runtime必須另走typed codec + pure WebSharper；不可延續。 |
| E2EQ已有TA viewport、crosshair、navigator、IndexedDB與provider query | 可作interaction/reference，不可複製成第二套shared runtime。 |
| E2EQ沒有以fCell2作其UI transport contract | 證明shared renderer不能要求fCell2；fCell2只由PTCS adapter處理。 |

## 3. Why one package is insufficient

若保留單一PTCS-specific Dynamic package：

1. E2EQ會被迫reference PTCS core與host integration。
2. renderer tests需要啟動不必要的session/actor/WebSocket runtime。
3. fCell2與ACL會滲入純UI protocol，其他host無法重用。
4. E2EQ與PTCS很可能各寫一套TA chart，interaction與bug fix逐漸漂移。

拆分後，Contracts/Renderer提供單一真實實作，adapter只負責transport與host policy。

## 4. Target component analysis

| Component | Owns | Does not own |
| --- | --- | --- |
| `Dynamic.Contracts` | DU/DTO、strict codec、schema version、limits、revision validation | DOM、network、PTMD math |
| `SduiRuntimeReducer` | pure transition、effects、last-good-state、resync decision | timers/socket |
| `CanvasRuntimeRegistry` | mount/instance/in-flight/dispose | caller auth/domain query |
| `Dynamic.Renderer` | WebSharper nodes、TA charts、view state、interaction | provider/SQL/fCell2 |
| `DynamicHostCallbacks` | submit/channel/visibility/scheduler/diagnostics interface | concrete PTCS/E2EQ implementation |
| PTCS adapter | fCell2、caller/ACL、target command、transient projection | TA renderer logic |
| E2EQ adapter | E2EQ command/reply/page/visibility mapping | PTCS semantics |

## 5. State model

```fsharp
type CanvasRuntimeState =
    { Identity: DocumentIdentity
      DocumentRevision: int64
      DataRevision: int64
      LastTransportSequence: int64
      InitialQuery: TaQueryState
      CurrentQuery: TaQueryState
      InitialView: TaViewState
      CurrentView: TaViewState
      Rows: TaRowSpec list
      SeriesByDataRef: Map<string, TaSeries>
      Status: TaDataStatus
      Poll: PollState }
```

Ownership：

| State | Authoritative owner | Network behavior |
| --- | --- | --- |
| document/layout/defaults | server document | explicit document/replace only |
| OHLCV/indicator/status | server snapshot/patch | snapshot/delta |
| instrument/interval/range/parameters | server-validated query | typed user action |
| zoom/pan/crosshair/toggle/row visibility | browser | none |

last-good canvas不可因query/parse/provider error消失；error只更新status/diagnostic，除非document本身從未成功mount。

## 6. Protocol analysis

Frame identity需分離：

```text
documentId          stable document identity
canvasInstanceId    mounted runtime instance
documentRevision    layout/default changes
dataRevision        data snapshot/patch changes
transportSequence   duplicate/gap/out-of-order detection
```

Reducer規則：

- same sequence/revision duplicate：no-op。
- next sequence + matching base：apply。
- gap/base mismatch/unknown instance：不apply，effect=`RequestResync`。
- error/heartbeat：不重建document。
- snapshot可在resync後replace bounded data，但保留合法local view或依explicit reset policy還原。

## 7. TA rendering analysis

TA rows共用time axis與viewport；price、volume、indicator採獨立y-scale。初始kinds：Candlestick、Volume、SMA、DMI、ADX、MACD、Heikin-Ashi。Renderer只畫server提供的series；PTMD Analytics負責domain calculation，避免同一指標在E2EQ/PTCS/browser有三套定義。

Browser working set必須bounded。移除舊點以typed `remove-series-before`進行；zoom/pan在loaded range內不送request，只有越界或明確query action才由adapter送server。

## 8. Transport and host analysis

`DynamicHostCallbacks`是effect boundary：

- submit typed action；
- open/close transient frame channel；
- report visibility/expanded state；
- schedule/cancel poll；
- report diagnostics。

PTCS path使用authenticated same-session channel，host決定user action是否audit、poll/heartbeat不進journal/history。E2EQ path使用既有backend/WebSocket/HTTP orchestration，但必須映射成相同frames；Renderer不辨識transport type。

## 9. E2EQ migration feasibility

可行，且應分段：

| Stage | Change | Gate |
| --- | --- | --- |
| 1 | Contracts/Renderer以deterministic TA fixtures完成 | reducer/component tests |
| 2 | E2EQ adapter parallel mount，不替換現有TA | equivalent state + geometry |
| 3 | Playwright比較Historical/RT TA interaction | viewport/crosshair/navigator/resize parity |
| 4 | shared renderer成為E2EQ TA canonical path | full AgentE2E matrix |
| 5 | 移除舊duplicated render code | no regression/no duplicate renderer |

E2EQ仍擁有其provider/backfill/page orchestration；只把render/reducer交給Dynamic.Renderer。這回饋了既有E2EQ成果，也避免讓Dynamic知道Sinopac-specific runtime。

## 10. PTMD alignment

Dynamic只吃generic runtime frames。PTMD.TAResearch response中的OHLCV、coverage、watermark、freshness經PTCS.Host或E2EQ adapter映射成dataRefs/status；Dynamic不reference `ITaResearchQueryProvider`。source停止時adapter仍可傳Stale status與last snapshot，Renderer保留畫面。

## 11. Resource and security analysis

| Concern | Design |
| --- | --- |
| frame/script injection | typed codec + operation/node allowlist；無script/URL/DOM selector |
| memory growth | row/bar/frame hard limits + remove-before + dispose |
| poll storm | visible/expanded/ready + one-in-flight + min 5 sec + backoff |
| stale confusion | persistent freshness/watermark/lag status |
| unauthorized action | adapter/server authoritative；Renderer capability只影響呈現，不取代ACL |
| secret leakage | shared protocol無credential；diagnostic不得含token/connection string |

## 12. Risks

| Risk | Mitigation |
| --- | --- |
| package split破壞現有facade | compatibility tests鎖`CommHub.useDynamicSdui`與static Canvas |
| adapter行為不一致 | same-frame/same-action cross-host contract fixtures |
| renderer migration失去E2EQ細節 | parallel path + Playwright geometry/interaction parity gate |
| PTCS transient seam延遲 | Renderer可先完成；PTCS E2E明確blocked，不以HTTP/history workaround替代 |
| WebSharper API不足 | 先記錄interop RFC，不偷渡inline JS |

## 13. SA conclusion

E2EQ使用PTCS.Dynamic能力是可行的，但共享點必須是transport-neutral Contracts + Renderer，而不是PTCS-specific facade。現有`PulseTrade.Comm.Spa.Dynamic`保留對PTCS的相容入口；E2EQ用自己的adapter安裝同一renderer。如此才能讓TA Canvas成為通用NuGet能力，同時保留PTCS的ACL/fCell2/durability與E2EQ的既有transport邊界。

### 2026-07-11 PTCS adapter package boundary correction

PTCS-specific transient integration落在獨立`PulseTrade.Comm.Spa.Dynamic.Ptcs`，不直接塞入legacy Dynamic Bundle。理由是WebSharper 10.1.5會在recursive generic `SduiValue` browser wire或PTCS beta81/82新增metadata進入Bundle dependency merge時無診斷崩潰。Alpha2先交付server adapter；browser adapter必須使用bounded non-recursive TA-specific wire並在獨立WebSharper package驗證。Legacy facade暫留PTCS beta80，Host可直接reference新adapter，不以raw JavaScript或HTTP polling繞過。

## 14. Composite row 與 delta wire 系統分析

單一`row.Kind + row.DataRef`把layout、indicator語意與presentation綁成同一層，無法表達K棒疊多條SMA、DMI/ADX共列或MACD三component。修正採additive兩層模型：row擁有viewport/time axis/height；ordered traces擁有dataRef、presentation、label/style。legacy row由contract helper投影成單trace，讓既有E2EQ/PTCS fixture可漸進更新。

browser delta責任在PTCS adapter邊界，而非Renderer。server reducer已有authoritative previous/next state，能以`dataRef + t`比較changed points並產生upsert/remove-before；client以同一keyed merge規則更新bounded state。Renderer只觀察新的RuntimeState，不理解transport delta。provider仍可能先回authoritative latest window，這是SQL/query效率議題，不得與browser wire bandwidth混為一談。
