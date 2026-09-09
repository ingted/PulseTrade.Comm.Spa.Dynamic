# REQ-PTCS-DYNAMIC-TA-0001 Realtime TA Canvas Runtime

Status: Accepted / Ready for DEV
Date: 2026-07-11
Owner: `PulseTrade.Comm.Spa.Dynamic*` packages
RFC: `doc/RFC/RFC-PTCS-DYNAMIC-0007.realtime-ta-canvas-runtime.md`
Current change: `doc/RFC/RFC-PTCS-DYNAMIC-0013.notebook-ta-workspace-production.md`
SA: `doc/TAResearch/SA.md`
SD: `doc/TAResearch/SD.md`
Test: `doc/TAResearch/Test.md`
WBS: `doc/TAResearch/WBS.md`

## 1. 背景

現有`fskynet-sdui` Canvas可把單次actor reply render成畫布，但沒有持續存在的document instance、data revision、incremental patch、poll lifecycle與正式TA chart runtime。若每5秒重送完整Canvas，會重建DOM、累積history/IndexedDB row，並破壞zoom/toggle state。

本需求以歷史研究為主，live tail只需預設每5秒更新。Dynamic必須把layout、data與view分離，提供transport-neutral的Contracts/Renderer，讓PTCS與E2EQ各自用adapter接入同一套WebSharper renderer。E2EQ不得因重用renderer而帶入PTCS.Host、fCell2、ACL或MessageFabric依賴。

## 2. Product/package boundary

| Package | Responsibility |
| --- | --- |
| `PulseTrade.Comm.Spa.Dynamic.Contracts` | SDUI document/snapshot/patch/action/freshness DTO、strict codec、limits與revision rules；只含WebSharper metadata/typed browser codec，不依賴PTCS/PTMD/MDCQ/TradeCore/FsStl/FCell2/SQL。 |
| `PulseTrade.Comm.Spa.Dynamic.Renderer` | pure WebSharper reducer、Canvas/TA renderer、local interaction、lifecycle與transport-neutral host callbacks。 |
| `PulseTrade.Comm.Spa.Dynamic` | 現有相容facade與PTCS adapter；保留`CommHub.useDynamicSdui`/extension bundle整合。 |

E2EQ只reference Contracts + Renderer，透過E2EQ adapter提供snapshot/action/transient channel；不reference `PulseTrade.Comm.Spa.Dynamic` PTCS facade。

## 3. Ownership

- Dynamic Contracts/Renderer owns：typed SDUI contract、state reducer、Canvas instance、TA row rendering、view interaction、poll scheduler與dispose。
- Dynamic PTCS facade owns：fCell2/reply/extension registration與PTCS authenticated WebSocket callbacks的mapping。
- PTCS core owns：authenticated session/channel、ACL、selected target command、durable vs transient projection seam。
- PTCS.Host owns：TA query actor、Argu DU、PTMD provider orchestration與RuntimeFrame mapping。
- PTMD.TAResearch owns：OHLCV serving query、coverage、watermark、freshness與analytics result。
- E2EQ adapter owns：既有transport/page state與RuntimeFrame/action之間的mapping。

Dynamic不得reference PTMD、broker SDK、SQL client或PTCS.Host executable。

## 4. Functional requirements

| ID | Requirement |
| --- | --- |
| DYN-TA-REQ-001 | 第一個成功reply建立immutable `SduiDocument`，以`documentId + canvasInstanceId + documentRevision`識別；一般data update不得替換layout。 |
| DYN-TA-REQ-002 | runtime envelope區分`document | snapshot | patch | error | heartbeat`；data revision與transport sequence獨立。 |
| DYN-TA-REQ-003 | patch只允許typed operations：replace dataRef、upsert points、remove-before、set status/options；禁止script、DOM selector、JSON pointer或URL。 |
| DYN-TA-REQ-004 | TA Canvas初始支援Candlestick、Volume、SMA、DMI/ADX、MACD、Heikin-Ashi，rows由data/DSL驅動。 |
| DYN-TA-REQ-005 | chart支援zoom、pan、crosshair、legend toggle、row visibility、mode switch與parameter change；純view操作不得送network request。 |
| DYN-TA-REQ-006 | `ResetView`只還原view；`ResetCanvas`還原initial rows/query/view並要求fresh snapshot。 |
| DYN-TA-REQ-007 | remote parameter/range/interval/instrument/Add Row透過registered host callback送typed action；renderer不得直接查SQL/provider/arbitrary HTTP。 |
| DYN-TA-REQ-008 | transport-neutral host callback需支援submit、open/close transient channel、visibility、clock/scheduler與dispose；不得假設PTCS global socket。 |
| DYN-TA-REQ-009 | client-pull預設/最小5秒，只在Canvas mounted、expanded、page visible且channel ready時poll；同一canvas最多一個in-flight。 |
| DYN-TA-REQ-010 | timeout/backoff可恢復；unmount/close/disconnect必須取消timer、request與subscription。 |
| DYN-TA-REQ-010A | abrupt WebSocket disconnect必須通知 host transient backend執行同等於`Unmounted`的cleanup，再移除server reducer state；不得只清其中一層。 |
| DYN-TA-REQ-011 | revision gap、unknown instance、base mismatch產生resync effect；duplicate為no-op，out-of-order不得silent套用。 |
| DYN-TA-REQ-012 | snapshot/patch/heartbeat不得每次新增chat history或IndexedDB message row；durable audit政策由host adapter決定。 |
| DYN-TA-REQ-013 | initial snapshot、rows、bars、patch items與browser working set皆有hard limits；超限保留last-good canvas並顯示controlled error。 |
| DYN-TA-REQ-014 | extension absent時host可維持原fallback；extension存在但schema/type invalid時fail visibly，不silent當成功。 |
| DYN-TA-REQ-015 | 新runtime只用typed F# codec與WebSharper API；禁止`JS.Inline`、手寫`.js`或string-built script。 |
| DYN-TA-REQ-016 | Canvas顯示backend、coverage、watermark、lag、partial/sealed、quality與`Live/Delayed/Stale/Backfill/Unavailable`。 |
| DYN-TA-REQ-017 | PTCS adapter與E2EQ adapter對同一RuntimeFrame sequence必須得到等價renderer state/geometry；不得維護兩套TA renderer。 |
| DYN-TA-REQ-018 | 現有static Canvas payload保持相容；runtime v1只有在明確protocol時啟用。 |
| DYN-TA-REQ-019 | 一個TA row必須支援ordered multi-trace；legacy single `Kind/DataRef`自動導出一個trace，同row的K棒/線/柱共享viewport與time axis。 |
| DYN-TA-REQ-020 | browser wire在initial/document change/gap使用bounded full state；穩定document的poll只傳changed keyed points、remove-before與status delta，client deterministic merge且base mismatch要求resync。 |
| DYN-TA-REQ-021 | loaded range須有bounded OHLC overview；selection提供left/right handles與move，能選minimum bars至actual loaded count，draft期間不重畫主圖、release只commit一次。 |
| DYN-TA-REQ-022 | full-range最多2000 bars可壓縮呈現完整from/to與長趨勢；overview與主圖primitive須依pixel budget deterministic aggregation，不得用2000組DOM bar拖垮弱機。 |
| DYN-TA-REQ-023 | overview只投影browser reduced state，不是fetch、IndexedDB、PCSL或provider authority；local viewport操作network action count為零。 |
| DYN-TA-REQ-024 | Add Row editor draft跨snapshot/patch/heartbeat保持開啟與可編輯；SMA/DMI/ADX/MACD須提供kind-specific typed parameters，不能只用單一generic欄位。 |
| DYN-TA-REQ-025 | remove後可重新加入同kind row；client產生unique row id，server仍負責authoritative uniqueness與parameter validation。 |
| DYN-TA-REQ-026 | Reset Canvas成功後須恢復initial ordered rows/query/view；Reset View仍只改local viewport。 |
| DYN-TA-REQ-027 | TA reply須提供copy canonical SDUI JSON action；copy不得expand、mount、poll、改history或複製fCell envelope。 |


## 5. User scenarios

1. 初始query回document + snapshot，TA Canvas顯示歷史rows、coverage與freshness。
2. 使用者zoom/pan/toggle/crosshair，只改browser local state。
3. 每5秒host adapter poll delta，renderer套用patch更新尾端K棒，不增加message card。
4. 使用者切instrument/interval/range或Add Row，送typed action，server回snapshot/patch。
5. Reset View不查server；Reset Canvas要求fresh snapshot。
6. source停止時Canvas保留history並顯示Stale；socket重連後以last revision要求delta或resync。
7. E2EQ以自身transport驅動同一renderer，PTCS以authenticated channel驅動同一renderer。

## 6. Acceptance

1. 500+ bars initial load後20個5秒poll只推進data revision，不增加history/IndexedDB message rows。
2. 所有TA kinds與shared time viewport正確；zoom/pan/toggle無network，remote action恰好一次。
3. duplicate/gap/out-of-order/resync/bounds有deterministic reducer tests。
4. close/hidden/disconnect後timer/subscription/in-flight全部釋放。
5. PTCS adapter與E2EQ adapter跑相同fixture，reducer state與主要geometry一致。
6. Playwright分別操作PTCS-hosted與E2EQ-hosted TA Canvas，驗證zoom/pan/reset/add-row/toggle/resize/stale/error，console無error。

## 7. Upstream dependencies

PTCS path仍需要core提供authenticated duplex/transient lifecycle seam；這是PTCS adapter的dependency，不是Renderer本身的dependency。E2EQ可先用自己的transport adapter驗證Renderer，但不得把E2EQ-specific socket寫入Contracts/Renderer。完整PTCS production acceptance必須等PTCS companion seam完成。

## 8. 2026-07-15 Full export / draft query / cursor requirements

| ID | Requirement |
| --- | --- |
| DYN-TA-REQ-028 | REQ-027由full-data JSON download取代：下載檔須包含document、timeline、OHLCV、indicator series、query/provider metadata、revision/freshness；檔名為`yyyyMMddHHmmss-<GUID>.json`。已展開時沿用active channel；收合時明確點下載可使用一次性channel。 |
| DYN-TA-REQ-029 | collapsed/unmounted reply平時不得mount、開channel或poll；只有使用者明確點下載才可建立bounded one-shot mount/full/close lifecycle，成功或失敗均不得留下poll。durable Document仍不含series points。 |
| DYN-TA-REQ-030 | instrument/interval/range controls是local draft；改值不得在Apply前送action、改authoritative query或重render，poll不得覆蓋draft。 |
| DYN-TA-REQ-031 | K棒、line points、cross-row cursor與pointer hit-test須共用slot-center geometry；所有rows同一index的cursor X誤差<=1px。 |
| DYN-TA-REQ-032 | 只有Document明確宣告`poll-delta` capability時才可排週期poll；static SDUI Document不得因TA renderer存在而自動更新。 |
| DYN-TA-REQ-033 | live poll只可更新status/data/chart subtree，不得替換Add Row/query editor DOM；已開啟select、focus及draft須跨poll保持。 |
| DYN-TA-REQ-034 | remote Add/Remove/Reset/Apply必須single in-flight；Reset Canvas一次恢復原始ordered rows/query，不是undo最後一次修改。 |

## 9. 2026-09-04 Notebook TA Workspace production requirements

| ID | Requirement |
| --- | --- |
| DYN-TA-REQ-035 | Dynamic提供generic source snapshot/event envelope，只擁有identity、epoch、sequence、revision、validation與resync；domain payload/reducer由owner adapter提供。 |
| DYN-TA-REQ-036 | source duplicate須no-op；sequence gap、identity/epoch/schema change、base revision mismatch或domain reducer reject須保留last-good state並產生typed snapshot request。 |
| DYN-TA-REQ-037 | source revision/sequence不得直接提升DocumentRevision；只有backend resource transition成功後才能發布新的authoritative document revision。 |
| DYN-TA-REQ-038 | schema-driven editor支援list add/remove/reorder、group與owner-provided choices；同template不同參數row以stable RowId並存。 |
| DYN-TA-REQ-039 | mutation使用correlated request/result；pending不得冒充完成，reject只更新action feedback，revision conflict要求full snapshot。 |
| DYN-TA-REQ-040 | renderer不得聚合行情或推論availability；多尺度interval/frontier/partial/final/quality由owner normalized metadata驅動。 |
| DYN-TA-REQ-041 | dedicated chart root不得攔截既有Expression、FloatingPoint或一般list formatter；cell不需手工SDUI JSON/Display plumbing。 |
| DYN-TA-REQ-042 | production acceptance須使用新版MDCQ real source、`dotnet dib`與Playwright MCP；synthetic M12只作regression。 |
| DYN-TA-REQ-043 | authoritative `TaWorkspaceDocument`須以同一DocumentRevision攜帶rows與generic editor schema catalog；正式runtime不可依賴renderer-local schema options。catalog空白時不得顯示Add/Edit。 |
| DYN-TA-REQ-044 | 可修改row須保存versioned template/value binding；Edit以stable RowId送`ApplyTemplate(Some rowId, ...)`並原位替換。legacy無binding row仍可顯示/移除但不可猜測成可編輯。 |
| DYN-TA-REQ-045 | owner須以`BaseRowId`明確指定shared event-time axis；shared cursor必須落在base row的真實datapoint，其他row只可回finalized containing、finalized as-of或missing，不得取未完成coarse point。 |
| DYN-TA-REQ-046 | viewport commit須送帶`BaseRowId`的`[startEventTimeUtc, endEventTimeExclusiveUtc)`與`MaximumBasePoints`；上限固定為4000，pending期間不得再提交第二次range mutation。 |
| DYN-TA-REQ-047 | Interactive browser application須提供idempotent single Start與terminal Dispose；斷線採bounded reconnect，每一replacement transport只送一次Mounted/full-snapshot request，重連snapshot成功前保留last-good document/data，且stale socket callback或逾時不得產生重複channel/timer。 |
| DYN-TA-REQ-048 | 多條TA series共用時間資訊時只能傳一份versioned temporal axis；series以Position與AxisRevision連接，不得逐scalar複製interval/frontier/finality metadata。 |
| DYN-TA-REQ-049 | Position只作axis join key。sparse axis不得依scale補空K；同一current-K preview沿用Position並以新的axis/series revision原位替換，revision不一致或未知Position須保留last-good並要求resync。 |
| DYN-TA-REQ-050 | candlestick可由同一axis上的O/H/L/C/V五條scalar series組成；shared-axis transport須支援至少3820 positions x 28 series且低於既有16MiB frame上限，同時維持`temporal-point.v1`相容解碼。 |
| DYN-TA-REQ-051 | owner須提供opaque stable `OwnerFingerprint`；SPAA以ProgramFingerprint、DataSourceFingerprint及cache semantic/schema version產生此欄，requested/cached time range不得進入identity。可含range的`QueryFingerprint`只供單次query診斷。Dynamic以OwnerFingerprint作exact cache isolation、以`RuntimeCacheCoverage`判斷range覆蓋，不解析或產生FSSTL/MDCQ semantics。 |
| DYN-TA-REQ-052 | IndexedDB只可保存RuntimeReducer已接受的bounded document/data projection；invalid/gap/error/rejected action不得污染last-good cache。 |
| DYN-TA-REQ-053 | cache hit可先呈現stale projection，但必須向host送resume/full驗證；`DynamicActionResult.Accepted`不得視為資料已更新，只有authoritative RuntimeFrame可取代cache。 |
| DYN-TA-REQ-054 | cache rehydrate不得沿用舊session identity或提高revision；fingerprint/schema/range不符、corrupt、oversize或quota failure須fail closed或降級no-cache。 |
| DYN-TA-REQ-055 | browser cache須bounded且可淘汰，預設最多8筆、單筆受16MiB上限；不得保存credential、capability或actor address。 |
| DYN-TA-REQ-056 | production range/cache acceptance須以真FSSTL多尺度FloatingPoint/Frame/map workspace、MDCQ source、`.dib`與Playwright完成reload、range revisit、delta/full及console gate。 |
