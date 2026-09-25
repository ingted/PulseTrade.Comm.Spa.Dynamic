# System Analysis (SA) - PulseTrade.Comm.Spa.Dynamic

## 1. 系統架構概念 (Architectural Concept)
本系統作為 `PulseTrade.Comm.Spa` 的 NuGet 擴充套件，採用 **外掛架構 (Plugin Architecture)**：
- 宿主 (Host) 為 `PulseTrade.Comm.Spa` (PTCS) 0.2.4-beta7 核心。
- 擴充模組 (Extension) 提供 FSkynet SDUI 特定的渲染元件與 `Actor Dynamic` 等型別。

## 2. 抽取方法論 (Extraction Methodology)
為了將原本位於 `G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa` 中的 `fskynet-sdui` 內容抽取並封裝，我們採用以下方法論：
1. **反向依賴注入 (Dependency Inversion)**：
   原本 PTCS 核心需要認識 `"schema": "fskynet-sdui"`。抽取後，核心退化為一個廣播者，僅提供 `IMessageRenderer` 的註冊介面。Extension 包將負責實作此介面並主動「掛載 (Inject)」到核心註冊表中。
2. **領域層級隔離 (Domain Segregation)**：
   Server 端的 F# AST (`FAkka.FCell2`) 轉換邏輯 (`FCell2Interop.fs`) 與特製的 Actor (`ShowcaseDemoActor`) 將從 PTCS 中移除，並以獨立的 `.fs` 模組放入 `PulseTrade.Comm.Spa.Dynamic` 中。透過 `CommHub` 的擴充方法 (`Extension Methods`) 將其路由註冊回去。
3. **前端元件解耦 (Client Component Decoupling)**：
   WebSharper 中的 `createSduiSummaryCard` 等 DOM 構建邏輯，將連同其相依的 CSS/狀態管理，全部複製到 Dynamic 專案的 `DynamicRenderer.fs` 中，並透過 `[<JavaScript>]` 屬性確保其在瀏覽器端編譯與執行。

## 3. 技術風險與潛在阻礙 (Technical Risks & Potential Obstacles)
雖然目前在 PTCS 內直接實作的 SDUI 運作良好，但抽取到 Extension 專案會面臨以下技術挑戰：
1. **WebSharper 跨組件掛載 (Cross-Assembly WebSharper Interop)**：
   WebSharper 對於跨組件 (Cross-Project) 的 DOM 操作與全域變數註冊可能會有初始化順序 (Initialization Order) 的問題。若 `DynamicRenderer.Start()` 在核心 `Client.fs` 完成載入前就被呼叫，可能會導致 NullReference 或找不到註冊表。
   *對策*：需利用 WebSharper 的 `[<SPAEntryPoint>]` 或是確定的生命週期 Hooks 來確保宿主準備好後再掛載。
2. **內部 API 存取限制 (Internal Access Restrictions)**：
   原本在 PTCS 內部實作時，SDUI 的 UI 元件可以存取 PTCS 內部 (internal) 的狀態或 Helper Functions。抽離為外部 NuGet 依賴後，只能存取 `public` API。若有必須用到的 internal API，將面臨無法存取的困境。
   *對策*：若發現依賴了 internal API，必須透過發佈 `UPSTREAM_RFC` 要求 PTCS 將該 API 升級為 public，或開放對應的 Getter/Setter。
3. **Actor 註冊與狀態還原 (Actor Registry & Replay)**：
   包含 `Actor Dynamic` 等特殊 Actor 在進行系統重啟與 Akka Journal Replay 時，如果系統只認識基礎的 Actor 型別，可能會導致 Dynamic Actor 反序列化失敗或無法重新啟動。
   *對策*：在 Server 掛載 `useDynamicSdui()` 時，必須明確包含 Actor 的 Factory Method 註冊，確保 Replay 引擎知道如何實例化這些擴充 Actor。

## 4. 模組分析 (Module Breakdown)
### 4.1 Server-Side Extension Module
- **FCell2 Interop**：負責將 F# DSL (`fCell2`) 轉換為 JSON SDUI Payload。
- **Dynamic Actors**：負責動態派發任務的 Actor，包含支援 `Actor Dynamic` Tab Page 的主體。
- **Mount API**：提供 `CommHub.useDynamicSdui()`。

### 4.2 Client-Side Extension Module (WebSharper)
- **SDUI Renderer**：處理 JSON Payload 渲染為 Canvas/Grid 等。
- **Dynamic Tab Page**：實作 `Actor Dynamic` 對應的頁面掛載。

## 5. RFC-PTCS-DYNAMIC-0002 Argu Form Boundary

Dynamic Argu Form 是 Dynamic package 的 server-driven input extension。責任分工如下：

| Boundary | Owner | Responsibility |
| --- | --- | --- |
| Argu metadata / DU reflection | PTCS.Dynamic | 從 allowlisted DU type / union case metadata 產生 form schema。 |
| SDUI form rendering | PTCS.Dynamic | 使用 `DynamicRenderer` 渲染 `schema = "fskynet-sdui"` / `formMode = "argu-form"`。 |
| SubmitArguForm | PTCS.Dynamic | 收集 input state，輸出 complete raw Argu args string。 |
| Append input renderer seam | PTCS core | `RFC-PTC-SPA-0007` 提供 mount point、fallback、submit callback。 |
| Add-key dialog seam | PTCS core | `RFC-PTC-SPA-0007` 提供 guided key builder mount point 與 key validation/readback。 |
| Durable proxy / ShardingDelivery | PTC RN | `RFC-PTC-0016` 讓 RN DurableProxy 消費 `ActorArguTargetCommand.RawArgu` 並處理 legacy actor delivery/reply。 |

`RFC-PTCS-DYNAMIC-0002` 的 `actorAddress :: duTypeName :: unionCaseNames` 屬 Argu-first first slice。`RFC-PTCS-DYNAMIC-0003` 修正後的新 key model 使用 `actorAddress :: duTypeOrTemplateKey :: canonicalArgString`。Dynamic 擁有 parser validation、ordering、metadata resolution 與 DSL generation；PTCS core 只維持 append key registry semantics。

Dynamic 不應把 PTCS seam、RN DurableProxy 與 FSkynet renderer 混在同一 module。PTCS.Dynamic 的第一波實作應可在 PTCS seam 尚未發版時以 local shim 測 schema generator 與 renderer codec；真正 browser E2E 則需等 PTCS WBS-051B/C 可用。

## 6. RFC-PTCS-DYNAMIC-0003 Unified SDUI / Form DSL Boundary

`RFC-PTCS-DYNAMIC-0003.unified-sdui-form-dsl-roadmap.md` 將 Dynamic Argu Form 從 Argu-first 修正為 DSL-first。

架構分層：

| Layer | Owner | Responsibility |
| --- | --- | --- |
| SDUI document model | PTCS.Dynamic | `SduiDocument`、node tree、bindings、actions、Canvas/Form render surfaces。 |
| Canvas renderer | PTCS.Dynamic | 將同一份 DSL render 成展開畫布，主要用於 readonly / local manipulation。 |
| Canvas Tree renderer | PTCS.Dynamic | 渲染 `Tree` node，支援 `id/parentId/label/status` 欄位、直角線、帶框 `+` / `-` toggle；不得 owns Actor Registry projection/storage。 |
| FormInput renderer | PTCS.Dynamic | 將同一份 DSL render 成 append input UI，管理 state、validation、submit、option query。 |
| Argu parser-backed adapter | PTCS.Dynamic | 從 host-registered `IArgParserTemplate` / DU metadata + canonical arg string 產生 Form DSL document。 |
| Alias binding | PTCS.Dynamic / PTCS.Host | PTCS.Host 註冊中文 case/field/option aliases；PTCS.Dynamic 將 alias 寫進 DSL label，但 canonical submit/raw command 不變。 |
| Extension seam | PTCS core | 提供 selected key context、renderer mount point、submit callback、safe fallback、registered-provider query shell。 |
| Demo DU / live deployment | PTCS.Host | 載入 extension DLL、註冊 demo DU / DSL target、部署 81/443。 |

重要修正：

- Renderer 不直接了解 DU union case；它只 render backend-resolved DSL。
- Adapter 可以了解 Argu / DU，且以 registered parser parse target key 第三段 canonical arg string；但 adapter 不擁有 PTCS command path。
- PTCS core 不判斷 DSL id、DU type、template key 或 arg string 是否有效；沒有 `hub.useDynamicSdui(...)` 時只取 `keys[0]` actor address 走 built-in path。
- Unknown target、DU/template parse failure、unsupported subcommand 是 Dynamic validation error；extension absent 才是 PTCS fallback。
- Add target key UI 由 Dynamic renderer 完整擁有時，PTCS built-in raw JSON key input / key filter 不應同屏顯示。
- ActorTree integration 的 truth source 在 PTC Actor Registry / PTCS projection。Dynamic 僅把 PTCS 提供的 `ActorTreeDocument` 轉成 Canvas `Tree` 顯示；fallback table、IndexedDB cache 與 report endpoint 仍由 PTCS core owns。

Dynamic key model vNext：

```text
[ actorAddress; formDslId ]
[ actorAddress; duTypeOrTemplateKey; canonicalArgString ]
```

上一版 `[ actorAddress; duTypeName; unionCase1; unionCase2; ... ]` 是 first-slice historical contract，不再是新 UX 的 canonical target key。Canonical arg string 由 backend 以 Argu parser 驗證，並以原 token order 輔助 DSL default value 與 raw command rebuild。`ParseResults<'T>` subcommand 以 tail group 表示，例如 `datarange` 必須在 root args 後、subcommand args 前。

## 7. RFC-PTCS-DYNAMIC-0004 Actor Dynamic action modes

Dynamic package owns the semantics behind PTCS core add-key action modes.

Mode matrix:

| PTCS shape discriminator | Dynamic behavior |
| --- | --- |
| `actor-argu-target` | Claim renderer; require proxy actor address, target actor address, DU/template key, canonical arg string; produce explicit `[proxy; "target-v1"; target; template; raw]` FormInput target key. |
| `actor-argu-proxy` | Deprecated beta64 workaround; do not claim as a new user-facing renderer. |
| `actor-dynamic-target` | Claim renderer; if DU/template + canonical arg string are supplied, produce FormInput target key; if DU is blank, leave direct actor-key behavior to PTCS core. |
| `actor-dynamic-proxy` | Claim renderer; require proxy actor address and RN actor address; produce proxy-v1 key. |
| plain `actor-argu` / `actor-dynamic` | Legacy compatibility only; new PTCS UI should pass explicit mode discriminator. |

Actor Dynamic has two render layers:

1. append input layer: FormInput only when selected key carries DU/template or proxy metadata; single actor key uses PTCS fallback arbitrary text input.
2. message render layer: Canvas only when reply payload is `fskynet-sdui` JSON DSL; otherwise Dynamic returns `None` and lets PTCS render text/fCell history.

Proxy key analysis:

- First segment must remain `proxyActorAddress`, because PTCS core actor-argu route currently sends to the first key segment.
- Second segment `"proxy-v1"` is the Dynamic/proxy discriminator.
- Third segment is `rnActorAddress`, which identifies the RN Host target actor/sharding entity/logical durable endpoint.
- Fourth segment is `targetKind` (`raw`, `canvas-json`, `argu`, etc.). Payload is not part of the key; it is the append input value delivered to the proxy actor.
- Dynamic package does not guarantee the proxy actor exists or is split-service; verification must expose whether the live host is still using PTCS Host same-process demo actors.

Actor Argu proxy target is different from Actor Dynamic proxy key. For Actor Argu, Dynamic sends the explicit target tuple `[proxyActorAddress; "target-v1"; targetActorAddress; template; raw]`; PTCS core routes to the first segment and projects the third segment into `ActorArguTargetCommand.TargetActorAddress`. Dynamic does not spawn/reuse proxy actors and does not rely on PTCS Host/script hooks to rewrite persisted keys.
## 2026-06-28 ActorsPage Renderer System Analysis

ActorsPage renderer 是 PTCS `/actors` 的 page-level presentation extension，不是 Actor Dynamic message reply canvas。系統邊界如下：

- PTCS owns：Actor Registry projection、`ActorTreeDocument`、`/actors/api/tree`、`/actors/api/report`、fallback tree/table、browser page renderer registry。
- PTCS.Dynamic owns：browser-side `ActorsPage` renderer registration and presentation。
- Dynamic 不直接 reference PTCS Host runtime state，不 query ActorSystem，不寫 PCSL，不產生 report markdown。

First slice 的實作落在既有 `Client/ActorDynamicTab.fs`，不是新 client module。原因是本 repo + WebSharper 10.1.5.674 對新增 `[<JavaScript>]` compile unit 會在 `wsfsc.exe` 階段無診斷 crash；同時 `String.Contains` 也會 crash。這是 implementation constraint，不是產品設計。後續若升級 WebSharper 或調整 bundle project，可再拆成 `ActorsPageRenderer.fs`。

風險與補償：

- classifier 目前用 `IndexOf("ActorTopologyPage")`，不是 final strict JSON parser；PTCS page renderer registry 只會傳 ActorsPage payload，因此 first slice 可接受。後續 DYN-WBS-519 必須補 strict parser/codec test。
- first slice renderer 只接管 whole page host 並顯示 summary；尚未完成 node grouping/tree/grid/actions。
- 若 renderer throw 或回 `None`，PTCS fallback 仍是權威 UI。

## RFC-PTCS-DYNAMIC-0008 Mixed-reply Presentation Analysis

正式82畫面把大量`SduiValue Case/Fields` series直接輸出到history，表示問題不是chart CSS，而是production envelope沒有被`TaResearchReplyPresentation.runtimeFrames`正確解開。Dynamic必須在message boundary完成strict/bounded decode；PTCS不應知道`fCell2`或TA document schema。

| Input shape | Required result |
| --- | --- |
| plain string | `None`，由PTCS顯示文字 |
| valid static `fskynet-sdui` | static presentation，不開TA channel |
| direct RuntimeFrame JSON | RuntimeTa presentation |
| JSON-string包RuntimeFrame | bounded unwrap後RuntimeTa |
| `ptc.comm.fcell2.value.v1`/`fCell2.A`/F# DU `Case/Fields` | 只擷取合法reply frames，RuntimeTa |
| declared runtime但invalid/oversized | controlled error presentation，不顯示全量raw payload |

summary與chart lifecycle是同一presentation的不同成本層級。分類時只建立bounded metadata model；Collapsed不建立`TaResearchTransientClientHandle`。Inline或Fullscreen才建立handle，collapse/unmount/disconnect必須dispose。decode/base revision失敗發生在in-flight response，因此resync reducer必須能取消該request並替換成full snapshot，不能以`not InFlight` guard忽略。

Dynamic FormInput renderer仍只負責「Host已選Form模式時如何畫表單」。它不得因selected target含DU/template就接管整個page或自動改變PTCS composer mode。

## 2026-09-21 Generic Marker Overlay System Analysis

Marker 是 TA presentation primitive，不是交易domain event。Daedalus擁有 domain event→base-axis position／style projection及Backtest workspace revision gate；Dynamic只擁有wire、validation、reducer與render。

既有runtime可直接擴充：marker data沿用`TemporalSeries`與shared axis，document以`TaTraceKind.Marker`聲明，candidate validation在既有reducer commit前執行。另建marker websocket/store會複製sequence、revision、cache與last-good語意，故拒絕。

相容策略採最小protocol bump：v1只承載既有non-marker；v2才承載marker。Contracts／Renderer／browser clients exact package closure處理compile-time相容，protocol與cache schema處理runtime／stale bundle相容；不另造通用capability service。

主要風險：

- F# DU新增case會暴露未處理的exhaustive match；關鍵consumer必須同步build/test。
- patch shape validation目前早於apply，但marker identity/target/position需看完整candidate；必須在commit前加第二層semantic gate。
- renderer若把marker納入price points會改Y domain；geometry必須是獨立fixed lane overlay。
- manual browser wire parser目前unknown trace kind可fallback；marker migration必須改成fail closed。

`RFC-PTCS-DYNAMIC-0016`修正presentation contract而不改架構。Anchor是相對candle的空間位置，triangle direction是glyph語意；兩者不可互相推導。Current wire升為`ta-marker.v2`，v1只由compatibility decoder依舊renderer方向映射，browser cache schema升至3並讓schema 2 marker cache miss/resync。`Outline`使用透明內部，但interaction hit target必須與shared cursor同時作用。

數量限制分成wire與visual兩層：每個DataRef/Position bucket跨anchor總量最多4；跨marker traces共享同row/target/Position/anchor時，candidate aggregate亦最多4。後者避免各trace lane從0開始造成重疊，仍由document trace order與bucket order產生唯一順序。Aster擁有package/browser contract；Daedalus擁有DMI event mapping、SPAA transaction與Notebook真路徑，兩者以exact-package deployment closure銜接而非跨repo同commit。

## 2026-09-24 Backtest Presentation UX System Analysis

`RFC-PTCS-DYNAMIC-0024`延伸既有TA runtime，不建立backtest-specific renderer。系統責任如下：

| Layer | Owner | Responsibility |
| --- | --- | --- |
| Backtest domain truth | TradeCore／Daedalus | Signal、Order、Fill、scenario/result identity、PnL與stable domain ids。 |
| Consumer projection | DIExt／SPAA／Daedalus | 將domain event轉generic stripe／marker；準備summary、trades、timeline、downloads及monotonic selection generation。 |
| Runtime contracts | PTCS.Dynamic Contracts／Aster | OverviewStripe wire、marker capacity、strict validation、multi-operation candidate atomicity及last-good。 |
| Presentation renderer | PTCS.Dynamic Renderer／Aster | Navigator lanes、marker cluster、row resize、shared cursor及bounded prepared geometry。 |
| Host adapters | Interactive／PTCS clients／Aster | Exact package/bundle closure與既有runtime transport；不理解backtest domain。 |

採獨立OverviewStripe而非Marker變體，因為navigator line沒有bar anchor／shape／fill，且需要close sampling之外的event-time index。共用只會讓wire欄位失真。Stripe與Marker仍共用`TaMarkerTooltipField`，避免重造bounded tooltip vocabulary。

## Default Viewport／OFI Presentation 修正（RFC-0028）

`DefaultView.visibleBars`屬document-authored initial presentation；Renderer只在canvas application建立時解析，user viewport仍是local runtime authority。Marker Label/Tooltip已是足夠的generic payload，因此不新增交易domain contract。inline plot label在縮放與密集事件下會遮蔽K bar，改由每列固定24px OFI band承接shared-cursor projection；plot保留glyph與tooltip。

OFI reader由accepted prepared marker placements建立slot index，pointer只更新固定DOM。此邊界保留Daedalus對BUY/SELL、價格與PnL文字的ownership，同時讓PTCS負責bounded layout、排序、overflow與generation safety。真SPAA clean-cache視覺／效能驗收仍屬consumer gate。

Atomicity分兩層：PTCS reducer保證單一RuntimeFrame內所有`ReplaceDataRef`先形成candidate、全驗證後才commit；Daedalus consumer保證同scenario的runtime、summary、trades、timeline與download manifest都完成prepare後才一次publish。前者不能取代`selectionGeneration`，後者也不能繞過PTCS candidate validation。

Marker現有4筆限制把wire truth與DOM budget混在一起。新模型以64作transport/candidate hard limit、4作direct glyph budget；`+N` cluster只壓縮presentation，不壓縮identity。這會增加decode/candidate成本，但仍受dataRef/frame limits與prepared index約束，且避免consumer自行丟事件。

Row height是local presentation state，不是document mutation。`HeightWeight`只提供deterministic authored default；canvas-local override不進fingerprint、cache、provider command或scenario revision。這避免多人／多kernel preference同步問題，也讓fresh reload可回到canonical document。

## Chunked Snapshot transport boundary

4,000-slot owner fixture的完整Snapshot wire約17.3MB；即使point decode已分批，browser仍須先在單一task做完整`JSON.Parse`。具名stage量測顯示parse約74ms並在busy/GC條件越過100ms，故繼續拆reducer無法解除根因。選擇在transport boundary增加向後相容chunk framing，而不是改canonical RuntimeFrame或降低驗證。

Contracts擁有唯一packet schema/encoder；Interactive.Client擁有generation-safe staging與atomic commit；Renderer只接收accepted RuntimeState。Producer host可把既有`string array` frame輸出經encoder展開，不需要理解staging state。Browser cache仍只保存accepted projection，不保存partial packet。

此邊界保留legacy replay與舊producer，但production large Snapshot必須走chunk path。失敗策略為last-good + resync：item缺漏、重複、亂序、batch/count mismatch、socket generation切換或invalid candidate都丟棄整批，不得partial publish或提前ACK。

## 2026-09-25 Renderer prepared-geometry hot path correction

真SPAA的shared-cursor click反證consumer accounting coalescing假設：click不收新frame，仍可形成超過100ms的EventDispatch，因此責任在owner renderer。舊click path從raw `RuntimeState.Data`重建reference timeline；新path改讀已接受且與畫面同revision的prepared data。大型line geometry原本以`groupBy`建立bucket arrays，Y-domain再以多層`Array.collect/map/append`重建暫存陣列；兩者改為固定bucket extrema arrays與單次bounds accumulator，保持slot順序、bucket min/max、histogram zero baseline及padding語意。

這些修正不改wire、canonical reducer、cursor action、scenario identity或consumer projection。Interactive.Client必須重包，因WebSharper package metadata會帶入其建置時的Renderer graph；只替換transitive Renderer DLL不足以證明browser bundle已更新。Owner gate因此直接檢查生成bundle、console、click/commit long task與resize前後32px gutter；consumer仍須在真3,820-bar SPAA上重跑。

主要風險：

- `TaTraceKind`新增case會要求active consumers同步compile；以exact package graph與full WebSharper build關閉。
- marker 4→64若直接建立64個SVG group會放大DOM；renderer必須以4 glyph＋cluster維持bounded nodes。
- overview stripe若跟close path一起sampling會時間錯位；prepared model必須先走canonical axis lookup。
- drag resize若每pointer event重建rows/data readers會卡頓；只准rAF local geometry preview及pointer-up單次state commit。
- consumer若分批更新timeline/downloads，即使runtime patch原子仍會出現混合revision；這是Daedalus acceptance gate，不得被owner package PASS掩蓋。
