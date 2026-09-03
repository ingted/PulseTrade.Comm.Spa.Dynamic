# TEST-PTCS-DYNAMIC-TA-0001 Transport-Neutral Realtime TA Canvas Runtime

Status: Accepted / Active implementation
Date: 2026-07-11
REQ: `doc/TAResearch/REQ.md`
SD: `doc/TAResearch/SD.md`
WBS: `doc/TAResearch/WBS.md`

## 1. Test policy

- Contracts/reducer使用deterministic F# tests；所有UI milestone使用F# Playwright或Playwright MCP實際操作。
- PTCS/E2EQ adapter parity使用同一frame/action fixture。
- fake/internal fixture只驗pure component，不取代真host E2E。
- browser evidence檢查geometry、visible values、network count、history/IndexedDB count、focus、scroll與console errors。
- 新runtime source禁止`JS.Inline`、手寫JS與string-built script。

## 2. Matrix

| Test ID | Requirement | Level | Scenario / Expected | WBS | Status |
| --- | --- | --- | --- | --- | --- |
| DYN-TA-T-000A | Legacy readiness | Regression/Playwright | direct static DSL target renders exact reply；invalid schema preserves surface；strict page schema avoids token classification；FormInput remains intact | DYN-TA-00A | Pass |
| DYN-TA-T-001 | REQ-001/002/018 | Contract | all frame kinds strict roundtrip；static payload not misclassified | DYN-TA-001 | Pass：Document/Snapshot/Patch/Error/Heartbeat strict roundtrip，unknown case JSON fail closed；Contracts tests 7/7。 |
| DYN-TA-T-002 | REQ-003/014/015 | Negative | unknown op/node/script/URL/selector/oversize fail visibly and do not execute | DYN-TA-00A/001 | Pass：unknown protocol/case/payload mismatch/dataRef、script/URL/selector key、frame/row/snapshot/patch limits均controlled error或resync，保留last-good。 |
| DYN-TA-T-003 | REQ-010 | Reducer | duplicate no-op；gap/out-of-order/base mismatch requests resync and keeps last-good data | DYN-TA-002 | Pass：duplicate no-op；sequence gap/base mismatch/identity或target mismatch保留data並typed resync。 |
| DYN-TA-T-004 | REQ-006 | Reducer | ResetView local-only；ResetCanvas sends one snapshot action and restores defaults | DYN-TA-002 | Pass：ResetView回default view且NoEffect；ResetCanvas只產生一個typed SubmitAction。 |
| DYN-TA-T-005 | REQ-004/005 | Component | all TA row kinds, shared x-axis, separate y-scale, unknown-kind error | DYN-TA-003 | Partial：alpha5 render七種row、shared window/x-axis/crosshair、compact timestamp與separate y-scale；exact model tests 11/11。研究級DMI/MACD多線與legend仍待。 |
| DYN-TA-T-006 | REQ-005 | Browser | zoom/pan/crosshair/toggle/visibility send no network；cursor values match visible bars | DYN-TA-003/012 | Regression found：原gate只操作cursor slider，沒有pointer event，也沒有loaded-range navigator；2026-07-14回到Active，改由T-031..034關閉。 |
| DYN-TA-T-007 | REQ-007 | Browser | instrument/interval/range/Add/Remove Row each send one typed action with coherent disabled/in-flight state | DYN-TA-003/004 | Pass：query、Add/Remove Row、Reset Canvas送typed action；renderer與真PTCS deployed browser gate證明in-flight禁用、server ack與連續document/transport revision。 |
| DYN-TA-T-008 | REQ-008/009/010 | Lifecycle | only visible/expanded/ready polls；one in-flight；timeout/backoff/reconnect/resync | DYN-TA-002/004 | Pass for PTCS path：pure lifecycle與WebSharper interpreter涵蓋active/connected gate、one-in-flight、timeout/backoff、bounded reconnect/full snapshot；same Host第二browser context重新bootstrap並READY。Process restart仍列cross-host T-020。 |
| DYN-TA-T-009 | REQ-009/010A | Lifecycle | hidden/collapse/unmount/disconnect cancels timer/channel/subscription；abrupt disconnect必須呼叫backend `Unmounted`且兩層state歸零 | DYN-TA-002 | Active：client lifecycle已有suspend/dispose；2026-07-14補server adapter abrupt-disconnect backend cleanup regression。 |
| DYN-TA-T-010 | REQ-011/012 | PTCS E2E | 500+ bars + 20 polls update revision only；message/PCSL/IndexedDB history count stable | DYN-TA-004/006 | Pass：PTCS beta85 isolated真host送500 bars並完成desktop/mobile各20 polls、PCSL event count前後0；正式beta87 service另以Playwright CDP證明20 polls前後IndexedDB `pendingCommands/streamWatermarks/uiSnapshots` counts完全不變。 |
| DYN-TA-T-011 | REQ-013 | Bounds | every hard limit preserves last-good Canvas and reports reason | DYN-TA-001..003 | Partial：Contracts/reducer hard limits、Renderer bounded window與stale/error last-good Canvas body-count preservation pass；invalid frame真host visual preservation仍待。 |
| DYN-TA-T-012 | REQ-016 | Browser | Live/Delayed/Stale/Backfill/Unavailable and watermark/lag/quality visible | DYN-TA-003 | Partial：typed model覆蓋五種freshness；Playwright覆蓋Live/Stale/Unavailable、watermark、quality、recoverable error與Ready recovery。Delayed/Backfill browser matrix仍待。 |
| DYN-TA-T-013 | REQ-017 | Contract parity | PTCS/E2EQ adapters produce identical final reducer state | DYN-TA-004/005 | Pass：PTCS transient adapter與E2EQ server/browser adapters均使用canonical Dynamic TA document/state/action vocabulary；E2EQ exact-package test涵蓋server/browser `dataRef`/action parity、bounded snapshot、local view preservation、fractional revision與non-finite point fail-closed，E2EQ suite 187/187 pass。 |
| DYN-TA-T-014 | REQ-017 | Browser parity | two hosts have equivalent chart/rows/toolbar geometry and actions | DYN-TA-005/006 | Blocked：adapter packages已完成；legacy E2EQ main client clean WebSharper merge以`wsfsc.exe -532462766`終止，stale bundle不得作browser evidence。見`G:\PulseTrade.fs\Blocker.md` `BLK-20260712-001`。 |
| DYN-TA-T-015 | REQ-015 | Source gate | new runtime has no JavaScript/inline/global callback workaround | DYN-TA-001..003 | Pass：Contracts與Renderer source gate無`JS.Inline`、script、eval或global callback workaround。 |
| DYN-TA-T-016 | REQ-018 | Regression | static Canvas/FormInput/Argu/ActorsPage and facade remain compatible | DYN-TA-00A/004/007 | Partial：beta74 canonical classifier保留legacy/explicit Canvas、FormInput、ActorsPage與runtime v1；package tests 23/23 pass。真browser facade delegation仍待DYN-TA-007。 |
| DYN-TA-T-017 | REQ-014 | Extension behavior | absent uses host fallback；present-invalid shows controlled error | DYN-TA-00A/004/007 | Partial：unrelated/missing schema分類為`NonSdui`；present unsupported protocol/surface/document type分類為typed `InvalidSdui reasonCode`。Browser absent/present-invalid visual gate待補。 |
| DYN-TA-T-018 | REQ-008 | Dependency | Contracts/Renderer graphs exclude forbidden dependencies | DYN-TA-001 | Pass：Contracts只增加WebSharper metadata/typed browser codec並排除PTCS/fCell2/PTMD/MDCQ/TradeCore/FsStl/SQL；Renderer排除PTCS/fCell2/PTMD/SQL。 |
| DYN-TA-T-019 | REQ-017 | E2EQ AgentE2E | Historical/RT source/symbol/range/hover/tag/viewport/navigator regression | DYN-TA-005 | Blocked：real Binance collector與seeded E2EQ host可啟動，但目前只會送出stale legacy bundle；需先完成isolated clean bundle/route，才可執行AgentE2E。 |
| DYN-TA-T-020 | REQ-010A/013/016 | Soak | bounded polling does not grow timers/channels/DOM series/history/server backend state | DYN-TA-006 | Pass for formal bounded gate：5 polls + second-context reconnect，memory total +542 MiB、reconnect +420 MiB低於1024/512；長時間soak仍由DYN-TA-006追蹤。 |
| DYN-TA-T-021 | REQ-019 | Contract/Browser | legacy row導出單trace；四列ordered traces共享row viewport，K+6 SMA、DMI/ADX、兩組MACD可辨識且large history只render bounded visible DOM | DYN-TA-009 | Passed：正式Playwright四列/17 series、desktop/mobile/reconnect |
| DYN-TA-T-022 | REQ-020 | Wire/E2E | initial/reconnect bounded full；stable poll只送changed keyed points/remove-before/status，client merge等價full reducer state，base mismatch resync | DYN-TA-010 | Passed：server 7/7、client 7/7、deployed delta 978→979→empty |
| DYN-TA-T-023 | DYN-CHAT-REQ-001..021 | Document | RFC/REQ/SA/SD/WBS/Test ownership、composer boundary、lifecycle與stop boundary一致 | DYN-TA-011 | Pass：2026-07-13 accepted chain |
| DYN-TA-T-024 | DYN-CHAT-REQ-001/002/018 | Unit | direct、JSON-string、canonical fCell2 envelope、fCell2.A與Case/Fields payload解成同一RuntimeFrame set | DYN-TA-011 | Pass |
| DYN-TA-T-025 | DYN-CHAT-REQ-010/018/019 | Negative | oversize/depth/unknown/malformed runtime回bounded controlled error；plain回None；DOM不含raw point arrays | DYN-TA-011 | Pass |
| DYN-TA-T-026 | DYN-CHAT-REQ-003/004/019 | Unit/Browser | 四列摘要含instrument/range/scale、SMA/DMI/ADX/MACD參數與freshness，沒有full JSON | DYN-TA-011 | Pass |
| DYN-TA-T-027 | DYN-CHAT-REQ-005..009 | Browser | Collapsed chart/open/poll均0；inline才mount/open/poll；collapse回baseline | DYN-TA-011 | Pass |
| DYN-TA-T-028 | DYN-CHAT-REQ-006..009 | Browser | fullscreen close回原inline/collapsed、scroll/focus/view revision；不雙channel/poll，多reply隔離 | DYN-TA-011 | Pass |
| DYN-TA-T-029 | DYN-CHAT-REQ-021 | Reducer/E2E | in-flight invalid/base mismatch取消舊timeout並只送一次full snapshot，成功後恢復poll | DYN-TA-011 | Pass |
| DYN-TA-T-030 | DYN-CHAT-REQ-020 | PTCS E2E | Plain/Form切換不由Dynamic強迫；Plain single input，Form既有renderer，mixed replies保持 | DYN-TA-011 | Pass：formal beta96 82 gate |
| DYN-TA-T-031 | DYN-CHAT-REQ-022..024 | Document | requested/loaded/visible、canonical navigator、follow-latest與pointer責任可追溯 | DYN-TA-012 | Pass：2026-07-14 correction chain |
| DYN-TA-T-032 | DYN-CHAT-REQ-022..024 | Unit | viewport clamp/tail/history/delta follow、pointer ratio→index、short/empty series deterministic | DYN-TA-012 | Pass：renderer/model suite 15/15 |
| DYN-TA-T-033 | DYN-CHAT-REQ-016/017/023 | F# Playwright | inline展開首屏直接可見navigator；navigator從tail移到history；pointer跨三個X；四列crosshair X誤差<=1px且timestamp/value同步；network delta=0 | DYN-TA-012 | Pass at isolated renderer/package gate；formal route另由T-034追蹤 |
| DYN-TA-T-034 | DYN-CHAT-REQ-005..009/024 | Formal PTCS 82 | actual loaded/readout、bounded DOM、inline/fullscreen state、collapse zero resource、delta/reconnect不重置history viewport | DYN-TA-012 | Pass：beta111/Dynamic beta100/RN alpha60；200 loaded/48 visible、四列17 traces、inline/fullscreen/cursor/reconnect及memory gate通過。 |
| DYN-TA-T-035 | DYN-CHAT-REQ-025/028 | Unit/Wire | 2000-point full保留2000；empty-to-first-data為full；stable delta<=200；compact JSON省略不適用default fields | DYN-TA-013 | Pass：Dynamic.Ptcs `7/7`；`ta-browser.v3` columnar roundtrip與cap通過。 |
| DYN-TA-T-036 | DYN-CHAT-REQ-026/027 | Model/Browser | draft start不改committed window；release clamp/follow-latest deterministic；visible<=160 | DYN-TA-013 | Pass：Renderer `17/17`；timestamp-aligned warm-up traces不再造成insufficient sequence。 |
| DYN-TA-T-037 | DYN-CHAT-REQ-026/027 | F# Playwright | real mouse drag期間render sequence不變，release後恰增1；可由2000-range tail移至head；network action delta=0 | DYN-TA-013 | Pass：isolated real mouse drag與正式82 verifier均通過；實際visible=48。 |
| DYN-TA-T-038 | DYN-CHAT-REQ-025..028 | Formal PTCS 82 | actual loaded>=2000、head/tail navigation、bounded visible DOM、inline/fullscreen/reconnect與console error=0 | DYN-TA-013 | Pass：`run-ta2000-final-bounded-win39-alpha45-20260715022108`；loaded=2000、5 polls、同連線rejection recovery、reconnect與memory bounded。 |


## 3. Playwright operation and viewport gates

### 3.1 First viewport

1. Open TA surface; title/status/query toolbar and most of chart must be visible without vertical hunting。
2. Toolbar order：instrument -> interval -> range -> Load/Apply；row actions are secondary and do not occupy multiple empty bands。
3. Chart owns primary width；status is compact；row controls do not cover data or right-side detail panel。
4. At desktop and mobile widths, controls wrap/stack without overlap, clipping or dynamic size shift。

### 3.2 Research workflow

1. Load initial Candlestick + Volume and verify ascending time axis/data status。
2. Add SMA/MACD row through a compact editor; confirm/cancel/validation are explicit and editor collapses after success/cancel。
3. Zoom/pan/crosshair/toggle; verify network action count remains zero and focus/viewport persists after patch。
4. Change instrument/interval/range; one remote action, visible in-flight state, authoritative snapshot, old Canvas remains until success。
5. Trigger stale/error/gap/resync; status is visible but Canvas/FormInput remains usable。
6. Reset View and Reset Canvas have visibly different results and request counts。

### 3.3 Cross-host and resource

1. Run same frame fixture through E2EQ and PTCS hosts。
2. Compare row count, series bounds, viewport, toolbar labels and major `getBoundingClientRect` relationships。
3. Run 20 polls, hide/show, collapse/expand, disconnect/reconnect, close/reopen；assert one timer/channel maximum and stable history counts。
4. Capture screenshots before/after significant interactions and inspect console/page errors。

## 4. Release gate

`DYN-TA-001..008`完成、T-001..020 Pass、PTCS transient seam與E2EQ parity都有真路徑證據後才可標記Implemented。`T-000A`只關閉legacy readiness。

## DYN-TA-014 Overview / typed row editor / reset / copy

| ID | Layer | Acceptance | WBS | Status |
| --- | --- | --- | --- | --- |
| DYN-TA-T-039 | Document | RFC-0010與REQ/SA/SD/WBS/Test/Verification ownership及supersede關係一致。 | DYN-TA-014A | Pass |
| DYN-TA-T-040 | Pure model | dual-bound clamp/move/left/right/full-range、selection ratios、OHLC/line bucket determinism與bounds。 | DYN-TA-014B | Pass：Renderer model 19/19。 |
| DYN-TA-T-041 | F# Playwright | loaded=2000 overview、left/right/move、48/200/2000 views；drag render delta=0、release=1、remote action delta=0、primitive bounded。 | DYN-TA-014C | Pass：isolated verifier `artifacts/ta-renderer-playwright`。 |
| DYN-TA-T-042 | Package/browser | editor跨poll保持；SMA/DMI/ADX/MACD fields/options；remove/re-add；invalid periods fail visibly。 | DYN-TA-014D | Pass：正式82驗ADX與MACD typed fields、poll穩定及remove/re-add。 |
| DYN-TA-T-043 | PTCS browser | copy action鄰接展開，clipboard canonical JSON；不mount/poll/mode change；error bounded。 | DYN-TA-014E | Pass：正式82以clipboard readback驗canonical JSON且presentation lifecycle不變。 |
| DYN-TA-T-044 | Formal 82 | Reset restores initial rows/query；mixed reply/ACL/inline/fullscreen/reconnect及2000 SQL/browser evidence通過。 | DYN-TA-014F | Pass：`verify.ptcsHostTaResearchFormal82.20260715111500`。 |

## 5. 2026-07-12 PTCS production integration evidence

- Renderer model：12/12 Pass；涵蓋query metadata與missing-metadata不使用demo default。
- Dynamic.Ptcs server：5/5 Pass；bounded browser wire保留query identity/range。
- Ptcs.Client：6/6 Pass；Add Row輸出canonical lowercase `sma`。
- PTCS.Host focused：24/24 Pass。
- `G:\PulseTrade.fs.Comm.Log\verification\ptcsHostTaResearchLive\run-d095bba2885846d0aa88a755f3a2d92c`：真SQL、FormInput、BTCUSDT/1m readback、Add SMA Row、Apply、20 polls、desktop/mobile geometry；PCSL metric polling前後相同，console/page error為0。
- `G:\PulseTrade.fs.Comm.Log\verification\ptcsHostTaResearchLive\deployed-channel-rebase-20260712204956`：正式82 local-login、controlled error/recovery、Add/Remove、Apply、Reset、poll、desktop/mobile與第二browser context bootstrap；production projection empty時明確UNAVAILABLE。
- `G:\PulseTrade.fs.Comm.Log\verification\ptcsHostTaResearchLive\deployed-restart-indexeddb-202607122114`：正式service process replacement後20 polls；Playwright CDP直讀IndexedDB三個object store，前後counts相同且無JavaScript/EvaluateAsync。
- 尚未覆蓋：Host process restart中的last-good/resync、E2EQ host parity、present-invalid static visual gate。

## DYN-TA-015 Full export / draft query / cursor

| ID | Requirement | Level | Acceptance | Status |
| --- | --- | --- | --- | --- |
| DYN-TA-T-045 | REQ-028..031 | Document | RFC/current REQ/SA/SD/WBS/Test與Host companion責任一致。 | Pass：2026-07-15 accepted chain。 |
| DYN-TA-T-046 | REQ-028/029 | Server/client | explicit full request回full wire；mounted handle產生完整export；collapsed平時zero-resource，明確下載只走one-shot mount/full/close且不留下poll。 | Pass：Ptcs `7/7`、Ptcs.Client `8/8`；正式browser wire為open -> bootstrap poll -> full -> close。 |
| DYN-TA-T-047 | REQ-028 | F# Playwright download | 捕獲檔名、parse schema；2000 timeline、OHLCV、indicator series、metadata/revisions完整。 | Pass：下載`20260715135129-34ab4eed-1e4c-48f3-8cf2-b3b2c440367b.json`，729355 bytes，schema與2000筆完整資料回讀通過。 |
| DYN-TA-T-048 | REQ-030 | Component/Playwright | interval draft跨poll不改render/query/action count；Apply後恰一次authoritative update。 | Pass：正式82在select後render/action count不變，Apply後action delta恰為1。 |
| DYN-TA-T-049 | REQ-031 | Model/geometry | first/middle/last pointer mapping、K棒/line/cursor slot center與cross-row X<=1px。 | Pass：Renderer `20/20`與正式cross-row screenshot/geometry gate通過。 |
| DYN-TA-T-050 | REQ-028..031 | Package/formal 82 | exact packages、mixed reply/ACL/collapse/inline/fullscreen/reconnect與console/page error gate。 | Pass：Renderer alpha25、Dynamic.Ptcs win41、Ptcs.Client win55；formal artifact `G:\PulseTrade.fs\Libs\PulseTrade.Comm\.pcsl\verify.ptcsHostTaResearchExport.alpha53.final.20260715150500`。 |

## DYN-TA-016 Editor shell / capability poll / Reset regression

| ID | Requirement | Level | Acceptance | Status |
| --- | --- | --- | --- | --- |
| DYN-TA-T-051 | REQ-032..034 | Document | `RFC-PTCS-DYNAMIC-0012`與REQ/SA/SD/WBS/Test、Host companion一致。 | Pass：兩個repo的RFC/current-state/WBS/Test互相回鏈。 |
| DYN-TA-T-052 | REQ-032 | Lifecycle unit | Document無`poll-delta`時跨兩個interval不產生poll；live capability維持既有cadence。 | Pass：Ptcs.Client `9/9`，static open/stray due/reactivate均不產生poll。 |
| DYN-TA-T-053 | REQ-033/034 | F# Playwright | Add Row select focus跨至少兩個live poll保持；chart/status仍更新；remote action in-flight不可重入。 | Pass：final isolated與正式82 gate跨兩次poll維持`ta-add-row-kind` focus，2000 bars仍更新。 |
| DYN-TA-T-054 | REQ-034 | Formal 82 | 連續刪除DMI、macd-short、macd-long後Reset一次恢復原始四列17 traces與順序。 | Pass：一次Reset恢復`price,dmi,macd-short,macd-long`與17 traces。 |
| DYN-TA-T-055 | REQ-032..034 | Release | exact packages、正式82、mixed reply/ACL/reconnect/console/page error通過。 | Pass：Renderer alpha27、Ptcs.Client win57、Host client alpha55；正式82 artifact `verify.ptcsHostTaEditorPollReset.alpha55.final.20260715153500`。 |

## DYN-TA-017 Notebook TA Workspace production

| Test ID | Requirements | Level | Expected | Status |
| --- | --- | --- | --- | --- |
| DYN-TA-T-056 | REQ-035..042 | Document | RFC/REQ/SA/SD/WBS/Test/Verification owner boundary一致，沒有複製Daedalus `StructuredSeries` DTO。 | Pass：RFC-0013與current-state鏈已同步。 |
| DYN-TA-T-057 | REQ-035/036 | Contract/codec | snapshot/event roundtrip；blank/oversize identity、negative revision/sequence、non-UTC timestamp、unsafe/oversize payload fail closed。 | Pass：Contracts suite 11/11。 |
| DYN-TA-T-058 | REQ-036 | Reducer | valid event透過owner reducer套用；exact/stale duplicate no-op。 | Pass：Contracts suite 11/11。 |
| DYN-TA-T-059 | REQ-036/037 | Reducer negative | sequence gap、stream/schema/epoch change、base revision mismatch、crossed snapshot order與domain reject保留last-good並回typed snapshot request。 | Pass：Contracts suite 11/11。 |
| DYN-TA-T-060 | REQ-035/037 | Dependency | Contracts不reference MDCQ、TradeCore、FsStl、FCell2、SQL/PTCS；source revision不直接變DocumentRevision。 | Pass：dependency reflection/source gate。 |
| DYN-TA-T-061 | REQ-038/039 | Contract/reducer | generic editor list/group/choice、stable row id、correlated accepted/rejected/conflict及reject-preserves-document。 | Planned |
| DYN-TA-T-062 | REQ-038..040 | F# Playwright | add/remove/reconfigure不同參數row；pending/reject；1K+5K及5K+30K geometry、missing/partial/final/quality/availability。 | Blocked by owner normalized metadata |
| DYN-TA-T-063 | REQ-041 | DIB integration | dedicated chart root auto-display，不攔截Expression/FloatingPoint/list formatter；resource prepare/swap/release可重現。 | Daedalus-owned integration |
| DYN-TA-T-064 | REQ-040..042 | Production E2E | 新版MDCQ real source + `dotnet dib` + Playwright MCP；history/live reconnect/resync、console、geometry與package manifest。 | External dependency |
