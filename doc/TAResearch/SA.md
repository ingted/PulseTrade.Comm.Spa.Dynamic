# SA-PTCS-DYNAMIC-TA-0001 Transport-Neutral Realtime TA Canvas Runtime

Status: Accepted / Ready for DEV
Date: 2026-07-11
REQ: `doc/TAResearch/REQ.md`
RFC: `doc/RFC/RFC-PTCS-DYNAMIC-0007.realtime-ta-canvas-runtime.md`
Current change: `doc/RFC/RFC-PTCS-DYNAMIC-0011.ta-export-draft-cursor-defaults.md`
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
| `Dynamic.Contracts` | DU/DTO、strict server/browser codec、WebSharper metadata、schema version、limits、revision validation | DOM、network、PTMD math、PTCS/domain packages |
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
- server-derived disconnect需由adapter映射成backend `Unmounted`；Dynamic reducer state與Host channel state是兩個獨立owner，任一層未清都會形成retention；
- report visibility/expanded state；
- schedule/cancel poll；
- report diagnostics。

PTCS path使用authenticated same-session channel，host決定user action是否audit、poll/heartbeat不進journal/history。E2EQ path使用既有backend/WebSocket/HTTP orchestration，但必須映射成相同frames；Renderer不辨識transport type。

正常collapse走client `close + Unmounted`；browser abort、refresh或transport failure可能來不及送close，因此PTCS以proxy `Terminated`發出server-derived `disconnect`。adapter需從既有per-channel `RuntimeState.Identity.CanvasInstanceId`建構`Unmounted`，不可相信client補送的identity。cleanup採finally語意：backend result可供diagnostic，但不能阻止adapter移除state。

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

## 2026-07-14 viewport / cursor system analysis

`query.lastBars=2000`是server request，不代表snapshot實際有2000 points；renderer只能從reduced state各series長度計算loaded count。UI狀態拆成：`LoadedCount`（data authority）、`VisibleWindow`（browser local）、`FollowLatest`（tail policy）、`CursorIndex`（visible-window local）。

Canonical navigator只改`VisibleWindow.StartIndex`，zoom只改`Count`；兩者不送network。若`FollowLatest=true`，delta後window重新anchor到tail；使用者pan到history後為false，delta不得移動window。pointer hit-test從任一row DOM rect取得normalized X並映射成visible index；所有row使用同一index/domain畫vertical cursor，避免各row依自身缺值數量算不同X。

PTCS container只負責不裁切、不攔截pointer與保留chat scroll；Dynamic owns navigator/cursor。Host/PTMD owns actual data量與coverage，不能由summary request文字冒充loaded evidence。

browser delta責任在PTCS adapter邊界，而非Renderer。server reducer已有authoritative previous/next state，能以`dataRef + t`比較changed points並產生upsert/remove-before；client以同一keyed merge規則更新bounded state。Renderer只觀察新的RuntimeState，不理解transport delta。provider仍可能先回authoritative latest window，這是SQL/query效率議題，不得與browser wire bandwidth混為一談。
# 2026-07-14 Loaded range / interaction analysis

現有lag不是native range本身，而是range `input`直接修改renderer committed state。每一pixel/step都重新select window、建四列17 traces與SVG/DOM。資料transport另有獨立缺口：full與delta共用200-point cap，導致`LastBars 2000`只在request存在，browser authority永遠只有200。

新state boundary為`Loaded series state -> Committed visible window -> Draft navigator preview`。chart只依賴前兩者；draft不進chart dependency graph。這樣pointer拖曳可保持thumb/label回應，但昂貴render只在release發生一次。loaded 2000保留在typed reduced state，visible最多160，local navigation不產生server action。

首次document可能先於data snapshot到達；因此「previous存在」不代表可送delta。只要relevant series由empty轉為non-empty，就必須送authoritative full。stable revision後才使用200-point delta。

## 2026-07-15 overview / editor / reset analysis

single-thumb只表示固定Count的StartIndex，無法表達「看全部2000」或左右縮放。新state仍維持`Loaded -> Committed -> Draft`三層，但Draft改為完整`TaVisibleWindow`。overview與主圖共用同一reference timeline；overview永遠涵蓋Loaded，selection才投影Committed。這不是第二套資料來源，也不觸發server query。

弱機瓶頸不是array持有2000 points，而是每次pointer move重建四列SVG與過多primitive。overview先做bounded bucket；Committed Count超過detail budget時主圖也做deterministic aggregate。資料完整度以loaded count/from/to證明，render primitive數與資料點數明確分離。

Add Row draft目前在`runtimeState.View.Map`內重建，poll revision可替換DOM並造成focus/editor消失。draft Vars與editor shell需提升到renderer instance scope；runtime state只影響disabled/status與document rows。`TaRowSpec.Options`已是transport-neutral extension point，可直接承載typed periods，不需新增TA-specific action union。

Reset failure來自Host把mutated current command當initial。Dynamic不能猜原始rows；它只送ResetCanvas並在authoritative fresh snapshot後套用document default view。copy action則透過PTCS generic action seam，不把clipboard責任塞進renderer或Host。

## 15. 2026-07-15 Export、draft與geometry authority analysis

compact Document與full browser state是兩種不同產品artifact。前者可journal/replay、只描述怎麼取得與呈現資料；後者是當次runtime的bounded research dataset。把後者塞進前者會破壞durable event size與lazy lifecycle，所以offline export只能從authenticated transient state產生。下載不是另一個provider query API：展開時沿用active channel；收合時以使用者明確動作建立one-shot channel，取得authoritative full wire後立即dispose，保留ACL、revision、range與既有2000-point limit。

query select立即重render的根因是draft與authoritative RuntimeState共用reactive `Var`。draft應位於renderer control local state，只在document revision變更時重新基準化；data poll不具覆蓋使用者輸入的authority。

cursor偏移的根因是兩套X公式：K棒使用slot center，line/cursor使用`index/(count-1)` endpoint。這不是CSS微調；所有series與pointer mapping都必須改用同一slot domain，否則first/last與跨row永遠無法對齊。

## 16. 2026-07-15 Editor shell / poll capability / Reset correction

先前只把draft值提升到renderer instance，沒有把editor DOM本身移出`RuntimeState.View.Map`。因此`PollInFlight -> Ready`即使Document revision不變，仍會替換整個workspace node；「editor仍visible」不足以證明focus/select identity穩定。正確邊界是Document shell依Document revision cache，status與chart另訂閱live runtime state。

poll cadence是Document capability，不是renderer type推論。`AllowedActions`含`poll-delta`才代表live provider；static document沒有該action時必須zero-poll。這同時避免無live data source的Canvas每5秒產生無意義transport/action。

Reset由Host initial command決定，Dynamic只接受authoritative Document。browser端仍須single in-flight，避免Remove response與Reset request競爭造成看似partial restore。正式驗證需從單列reset提高為連續刪除多列後一次完整restore。

## 17. 2026-09-04 Production Notebook owner-boundary analysis

`StructuredSeries`是Daedalus-owned provider ingest contract，Dynamic source envelope是Aster-owned projection ordering contract，兩者不可合併成共享巨型DTO。MdcQuote adapter先把Arrow/provider資料交給TradeCore sink；Daedalus workspace adapter完成domain transition與resource ownership後，才輸出Dynamic envelope/frame。這維持依賴方向 `provider -> owner adapter -> generic Dynamic contract`，Dynamic不反向依賴MDCQ/TradeCore/FsStl/FCell2。

source sequence/revision與document/data revision解決不同問題：前者證明upstream incarnation/order，後者管理browser projection。直接共用會讓失敗的domain mutation也提升UI revision。故source reducer只回Applied/Duplicate/ResyncRequired，workspace controller成功prepare/swap後才產生新的document revision。

風險最高處是部分建立resource後action失敗。renderer不得optimistic刪除row；Daedalus Interactive.Extension controller 的 `ApplyTemplate` 是 authoritative mutation，須prepare next resource、驗ready、在send gate內swap，再exactly-once release old lease。Aster只提供request/result與pending/reject UX，不控制TradeCore resource，也不實作第二套template mutation。

930K/1380K、availability與current partial都不是前端可推論的presentation shortcut。renderer只消費actual interval/frontier/optional availability與quality；缺失時標unknown/unavailable，不能用event/receive/render time補造。

generic editor schema只描述欄位與限制，不承載FsStl/TradeCore union。Choice/Scale選項由owner adapter提供；List/Group讓同一套renderer容納SMA/DMI/MACD/BBO等不同參數形狀。`TaRowSpec.RowId`是workspace instance identity，不是indicator kind；因此同template不同參數可並存，duplicate RowId則在document validation fail closed。

action state與document state分離。client同時只允許一筆pending request；request id必須與result correlation一致。revision mismatch在送出前形成typed conflict；backend rejection只更新bounded feedback。只有backend完成resource transition後發布的新Document frame，才可改變rows與DocumentRevision。

## 18. 2026-09-04 Editor catalog delivery gap

既有generic editor schema只存在renderer options；Interactive.Client用`defaultOptions`，PTCS `ta-browser.v4`也未承載schema，因此package consumer即使建立了TradeCore template catalog，browser仍只能看到rows，無法Add/Edit。這是contract delivery gap，不是Interactive.Extension應以另一份UI registry繞過的問題。

修正依賴方向為 `owner catalog -> TaWorkspaceDocument.EditorSchemas -> RuntimeFrame/PTCS browser wire -> Renderer`。`EditorSchemas`與`Rows`由相同DocumentRevision保證一致，舊wire缺欄位時解成empty catalog並隱藏mutation controls。row reconfigure另需在row options保存保留key binding；binding只含generic template key/value，不含provider、instrument、source lease或TradeCore型別。

這個邊界讓Daedalus只負責把`IndicatorTemplateCatalog`映射為generic schema與執行authoritative resource transition；Aster負責codec、validation、wire、renderer與相容性。invalid catalog/binding保留last-good workspace並關閉對應Edit，不得讓UI自行推測domain參數。
