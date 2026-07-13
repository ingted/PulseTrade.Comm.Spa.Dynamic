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
| DYN-TA-T-006 | REQ-005 | Browser | zoom/pan/crosshair/toggle/visibility send no network；cursor values match visible bars | DYN-TA-003 | Pass：F# Playwright證明pan/zoom/reset-view/row toggle callback count保持0，slider由B48移到B1時七列crosshair同為x=0，cursor OHLC/indicator values對齊visible index；desktop/mobile geometry與console gate通過。 |
| DYN-TA-T-007 | REQ-007 | Browser | instrument/interval/range/Add/Remove Row each send one typed action with coherent disabled/in-flight state | DYN-TA-003/004 | Pass：query、Add/Remove Row、Reset Canvas送typed action；renderer與真PTCS deployed browser gate證明in-flight禁用、server ack與連續document/transport revision。 |
| DYN-TA-T-008 | REQ-008/009/010 | Lifecycle | only visible/expanded/ready polls；one in-flight；timeout/backoff/reconnect/resync | DYN-TA-002/004 | Pass for PTCS path：pure lifecycle與WebSharper interpreter涵蓋active/connected gate、one-in-flight、timeout/backoff、bounded reconnect/full snapshot；same Host第二browser context重新bootstrap並READY。Process restart仍列cross-host T-020。 |
| DYN-TA-T-009 | REQ-009 | Lifecycle | hidden/collapse/unmount/disconnect cancels timer/channel/subscription | DYN-TA-002 | Partial：registry與Ptcs.Client handle均有suspend/dispose terminal、CancelPoll/Timeout/Reconnect；browser hide/show/disconnect resource observation留DYN-TA-006。 |
| DYN-TA-T-010 | REQ-011/012 | PTCS E2E | 500+ bars + 20 polls update revision only；message/PCSL/IndexedDB history count stable | DYN-TA-004/006 | Pass：PTCS beta85 isolated真host送500 bars並完成desktop/mobile各20 polls、PCSL event count前後0；正式beta87 service另以Playwright CDP證明20 polls前後IndexedDB `pendingCommands/streamWatermarks/uiSnapshots` counts完全不變。 |
| DYN-TA-T-011 | REQ-013 | Bounds | every hard limit preserves last-good Canvas and reports reason | DYN-TA-001..003 | Partial：Contracts/reducer hard limits、Renderer bounded window與stale/error last-good Canvas body-count preservation pass；invalid frame真host visual preservation仍待。 |
| DYN-TA-T-012 | REQ-016 | Browser | Live/Delayed/Stale/Backfill/Unavailable and watermark/lag/quality visible | DYN-TA-003 | Partial：typed model覆蓋五種freshness；Playwright覆蓋Live/Stale/Unavailable、watermark、quality、recoverable error與Ready recovery。Delayed/Backfill browser matrix仍待。 |
| DYN-TA-T-013 | REQ-017 | Contract parity | PTCS/E2EQ adapters produce identical final reducer state | DYN-TA-004/005 | Pass：PTCS transient adapter與E2EQ server/browser adapters均使用canonical Dynamic TA document/state/action vocabulary；E2EQ exact-package test涵蓋server/browser `dataRef`/action parity、bounded snapshot、local view preservation、fractional revision與non-finite point fail-closed，E2EQ suite 187/187 pass。 |
| DYN-TA-T-014 | REQ-017 | Browser parity | two hosts have equivalent chart/rows/toolbar geometry and actions | DYN-TA-005/006 | Blocked：adapter packages已完成；legacy E2EQ main client clean WebSharper merge以`wsfsc.exe -532462766`終止，stale bundle不得作browser evidence。見`G:\PulseTrade.fs\Blocker.md` `BLK-20260712-001`。 |
| DYN-TA-T-015 | REQ-015 | Source gate | new runtime has no JavaScript/inline/global callback workaround | DYN-TA-001..003 | Pass：Contracts與Renderer source gate無`JS.Inline`、script、eval或global callback workaround。 |
| DYN-TA-T-016 | REQ-018 | Regression | static Canvas/FormInput/Argu/ActorsPage and facade remain compatible | DYN-TA-00A/004/007 | Partial：beta74 canonical classifier保留legacy/explicit Canvas、FormInput、ActorsPage與runtime v1；package tests 23/23 pass。真browser facade delegation仍待DYN-TA-007。 |
| DYN-TA-T-017 | REQ-014 | Extension behavior | absent uses host fallback；present-invalid shows controlled error | DYN-TA-00A/004/007 | Partial：unrelated/missing schema分類為`NonSdui`；present unsupported protocol/surface/document type分類為typed `InvalidSdui reasonCode`。Browser absent/present-invalid visual gate待補。 |
| DYN-TA-T-018 | REQ-008 | Dependency | Contracts/Renderer graphs exclude forbidden dependencies | DYN-TA-001 | Pass：Contracts無WebSharper/PTCS/fCell2/PTMD/SQL；Renderer僅增加WebSharper並排除PTCS/fCell2/PTMD/SQL。 |
| DYN-TA-T-019 | REQ-017 | E2EQ AgentE2E | Historical/RT source/symbol/range/hover/tag/viewport/navigator regression | DYN-TA-005 | Blocked：real Binance collector與seeded E2EQ host可啟動，但目前只會送出stale legacy bundle；需先完成isolated clean bundle/route，才可執行AgentE2E。 |
| DYN-TA-T-020 | REQ-013/016 | Soak | bounded polling does not grow timers/channels/DOM series/history | DYN-TA-006 | Partial：20-poll、one-in-flight、500-bar bounded DOM、PCSL history 0、IndexedDB counts stable、dispose close與正式Host process restart後reconnect pass；長時resource/channel observation與E2EQ cross-host gate仍待。 |
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

## 5. 2026-07-12 PTCS production integration evidence

- Renderer model：12/12 Pass；涵蓋query metadata與missing-metadata不使用demo default。
- Dynamic.Ptcs server：5/5 Pass；bounded browser wire保留query identity/range。
- Ptcs.Client：6/6 Pass；Add Row輸出canonical lowercase `sma`。
- PTCS.Host focused：24/24 Pass。
- `G:\PulseTrade.fs.Comm.Log\verification\ptcsHostTaResearchLive\run-d095bba2885846d0aa88a755f3a2d92c`：真SQL、FormInput、BTCUSDT/1m readback、Add SMA Row、Apply、20 polls、desktop/mobile geometry；PCSL metric polling前後相同，console/page error為0。
- `G:\PulseTrade.fs.Comm.Log\verification\ptcsHostTaResearchLive\deployed-channel-rebase-20260712204956`：正式82 local-login、controlled error/recovery、Add/Remove、Apply、Reset、poll、desktop/mobile與第二browser context bootstrap；production projection empty時明確UNAVAILABLE。
- `G:\PulseTrade.fs.Comm.Log\verification\ptcsHostTaResearchLive\deployed-restart-indexeddb-202607122114`：正式service process replacement後20 polls；Playwright CDP直讀IndexedDB三個object store，前後counts相同且無JavaScript/EvaluateAsync。
- 尚未覆蓋：Host process restart中的last-good/resync、E2EQ host parity、present-invalid static visual gate。
