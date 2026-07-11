# RFC-PTCS-DYNAMIC-0007 Transport-Neutral Realtime TA Canvas Runtime

Status: Accepted / DEV authorized
Date: 2026-07-11
REQ: `doc/TAResearch/REQ.md`
SA: `doc/TAResearch/SA.md`
SD: `doc/TAResearch/SD.md`
Test: `doc/TAResearch/Test.md`
WBS: `doc/TAResearch/WBS.md`
DSL: `doc/SDUI_DSL_zh-Hant.md`

## 背景

Static Canvas目前以單一reply JSON建立畫布。TA研究需要layout固定、data可持續更新、view由browser持有的runtime。直接每5秒回完整`fskynet-sdui`會重建DOM、累積history/IndexedDB、破壞zoom/toggle並放大transport負擔。

前版RFC把runtime直接綁在PTCS authenticated WebSocket。經review確認E2EQ最終也應使用同一Dynamic renderer，但E2EQ不是PTCS.Host，直接reference現有PTCS facade會引入fCell2、ACL、MessageFabric與host integration。故本版把Contracts/Renderer設為transport-neutral，PTCS與E2EQ只各自實作adapter。

## 目標

1. 在同一Canvas instance套用snapshot/patch，不重建document。
2. 支援TA row stack、研究型interaction與5秒live-tail pull。
3. 拆出可由PTCS與E2EQ共用的Contracts + Renderer packages。
4. 保持pure WebSharper、provider-neutral、bounded與可resync。
5. 保留static Canvas與現有`CommHub.useDynamicSdui`相容性。

## 非目標

1. 不在browser查SQL/PCSL/provider或執行backfill。
2. 不承諾tick-by-tick；預設/最小5秒poll。
3. 不引入Plotly或其他JavaScript runtime。
4. 不把PTCS authentication/fCell2或E2EQ socket寫入shared renderer。
5. 不在本RFC實作PTMD SQL或PTCS.Host actor。

## 方案比較

| Option | Benefit | Cost | Decision |
| --- | --- | --- | --- |
| 每5秒append完整Canvas | 沿用現有renderer | history/DOM爆量、state重置 | Reject |
| Renderer直接reference PTCS | PTCS接入快 | E2EQ帶入不必要依賴，runtime不可重用 | Reject |
| E2EQ另寫第二套TA chart | 可獨立演進 | renderer/interaction/test雙軌漂移 | Reject |
| Contracts + Renderer + host adapters | 單一renderer、transport可替換 | 需清楚adapter seam與package split | Accept |
| Dynamic直接查PTMD/SQL | 少一層 | security/ownership錯誤 | Reject |

## 決策

### D1. 三層package

```text
PulseTrade.Comm.Spa.Dynamic.Contracts
  document/frame/action/codec/limits

PulseTrade.Comm.Spa.Dynamic.Renderer
  reducer/runtime registry/WebSharper renderer/host callbacks

PulseTrade.Comm.Spa.Dynamic
  PTCS facade + bundle + compatibility extension
```

- Contracts不得referenceWebSharper、PTCS、fCell2、PTMD或SQL。
- Renderer只referenceContracts與必要WebSharper/browser abstractions。
- 現有Dynamic package保留`CommHub.useDynamicSdui`，內部改呼叫Renderer install。
- E2EQ reference Contracts + Renderer，新增自身adapter；不referencePTCS facade。

### D2. Generic renderer host seam

```fsharp
type DynamicHostCallbacks =
    { SubmitAction: SduiAction -> Async<Result<unit, DynamicHostError>>
      OpenTransientChannel: CanvasInstanceId -> IDynamicFrameChannel
      IsSurfaceVisible: unit -> bool
      Schedule: TimeSpan -> (unit -> unit) -> IDisposable
      ReportDiagnostic: DynamicDiagnostic -> unit }

type DynamicRendererRegistration =
    { TryMount: string -> Result<MountedDynamicSurface option, DynamicRenderError>
      Dispose: unit -> unit }

module DynamicSduiRenderer =
    val install: DynamicRendererOptions -> DynamicHostCallbacks -> DynamicRendererRegistration
```

PTCS adapter提供authenticated session/ACL/target submit/transient channel；E2EQ adapter提供其既有transport與page visibility。shared package不判斷caller identity，也不保存credential。

### D3. Document/data/view separation

```text
DocumentIdentity / DocumentRevision = layout + initial defaults
DataRevision / TransportSequence    = snapshot/patch ordering
ViewState                           = zoom/pan/crosshair/toggle/visibility
QueryState                          = instrument/interval/range/rows/parameters
```

一般poll只改data revision。view action不送server；query action由adapter送server並等authoritative frame。

### D4. Runtime frames

Runtime v1 frame kinds：`document | snapshot | patch | error | heartbeat`。Allowed patch operations固定為：

- `replace-data-ref`
- `upsert-series-points`
- `remove-series-before`
- `set-status`
- `set-options`

unknown operation、wrong base revision或unknown instance必須停止套用並產生resync effect，不best-effort修改DOM。

### D5. TA DSL vocabulary

```text
TaWorkspace
  TaToolbar
  TaChartStack
    TaChartRow[]
  TaLegend
  TaCrosshairGrid
  TaDataStatus
```

初始row kinds：`Candlestick | Volume | Sma | Dmi | Adx | Macd | HeikinAshi`。rows/series/status由dataRef驅動；indicator值由PTMD/host提供，Renderer不重新定義domain calculation。

### D6. Poll lifecycle

1. mount document建立唯一runtime instance。
2. page visible、surface expanded、channel ready且無in-flight時才poll。
3. 預設/最小5秒；timeout後bounded backoff。
4. duplicate no-op；gap/base mismatch要求resync。
5. unmount/close/disconnect取消timer、request、subscription與registry entry。
6. heartbeat/patch不進chat history；是否audit user action由host adapter決定。

### D7. PTCS adapter

PTCS companion seam需提供caller/ACL context、selected target submit、authenticated transient channel、mount/unmount與projection policy。Dynamic facade不得讀global/internal socket或改走arbitrary HTTP workaround。

### D8. E2EQ adapter and migration

E2EQ保留既有provider/backend/page transport，只把reply/delta映射為RuntimeFrame，把toolbar action映射回E2EQ command。遷移順序：

1. shared renderer以deterministic fixture達成既有E2EQ TA geometry/interaction parity；
2. E2EQ新增隱藏/parallel adapter path；
3. Playwright比較Candlestick/indicator/viewport/crosshair/resize；
4. parity gate通過後，以shared renderer取代舊TA render path；
5. 移除重複renderer，但保留E2EQ-specific orchestration。

因此「E2EQ改用Dynamic」成立，但重用點是Contracts + Renderer；不是讓E2EQ偽裝成PTCS.Host。

### D9. Pure WebSharper

新runtime不得使用`JS.Inline`、手寫JavaScript、string-built script或global callback。typed JSON codec、timer/WebSocket/visibility/SVG/DOM interaction使用F#/WebSharper API。若WebSharper缺API，需先以RFC記錄interop exception，不可在本RFC偷渡。

## Compatibility

- 無`protocol=sdui-runtime.v1`的payload走既有static Canvas。
- Dynamic absent時host保留raw/fallback。
- extension存在但runtime invalid時顯示controlled error，不silent回退成看似成功Canvas。
- current PTCS facade API保持source compatibility；內部逐步委派shared Renderer。

## Impact

| Area | Change |
| --- | --- |
| Contracts | typed runtime/frame/action/limits/codec package |
| Renderer | pure reducer、instance lifecycle、TA Canvas與generic host seam |
| Dynamic facade | PTCS adapter、bundle registration、static compatibility |
| PTCS core | authenticated transient lifecycle companion seam |
| E2EQ | adapter與逐步renderer parity migration |
| DSL docs | RuntimeFrame、TA nodes、actions與status vocabulary |

## Acceptance

1. Contracts assembly graph無PTCS/PTMD/SQL/WebSharper依賴；Renderer無PTCS/fCell2依賴。
2. reducer tests涵蓋duplicate/gap/out-of-order/resync/bounds。
3. PTCS與E2EQ adapters餵相同frames，得到等價state與主要geometry。
4. 500+ bars與20次poll不重建root、不增加message rows。
5. Playwright在兩種host path操作zoom/pan/toggle/add-row/reset/stale/error，console無error。
6. static Canvas與既有PTCS Dynamic behavior回歸通過。

## 後續流程

本RFC已accepted並授權依`DYN-TA-00A -> 001..008`順序開發。package split、NuGet push、E2EQ renderer replacement與PTCS core修改仍需各自通過Test/WBS detail gate。
