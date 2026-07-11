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
| DYN-TA-T-005 | REQ-004/005 | Component | all TA row kinds, shared x-axis, separate y-scale, unknown-kind error | DYN-TA-003 | Partial：alpha3 render七種row、shared window/x-axis/crosshair與separate y-scale；exact model tests 10/10。研究級DMI/MACD多線與legend仍待。 |
| DYN-TA-T-006 | REQ-005 | Browser | zoom/pan/crosshair/toggle/visibility send no network；cursor values match visible bars | DYN-TA-003 | Pass：F# Playwright證明pan/zoom/reset-view/row toggle callback count保持0，slider由B48移到B1時七列crosshair同為x=0，cursor OHLC/indicator values對齊visible index；desktop/mobile geometry與console gate通過。 |
| DYN-TA-T-007 | REQ-007 | Browser | instrument/interval/range/Add Row each send one typed action with coherent disabled/in-flight state | DYN-TA-003/004 | Partial：query/Add Row/Reset Canvas各送typed action；renderer browser gate證明PollInFlight禁用query/Add submit並可恢復；Ptcs.Client alpha4把typed query映射為bounded transient frame。真host ack仍待。 |
| DYN-TA-T-008 | REQ-008/009/010 | Lifecycle | only visible/expanded/ready polls；one in-flight；timeout/backoff/reconnect/resync | DYN-TA-002/004 | Pass at pure lifecycle：visible/expanded/connected gate、one-in-flight/coalesce、timeout/backoff/resync states；host timer/channel E2E留DYN-TA-004/006。 |
| DYN-TA-T-009 | REQ-009 | Lifecycle | hidden/collapse/unmount/disconnect cancels timer/channel/subscription | DYN-TA-002 | Pass at registry contract：suspend/dispose terminal、registry remove、CancelPoll；browser resource observation留DYN-TA-006。 |
| DYN-TA-T-010 | REQ-011/012 | PTCS E2E | 500+ bars + 20 polls update revision only；message/PCSL/IndexedDB history count stable | DYN-TA-004/006 | Partial：PTCS beta82 real transient no-history gate pass；Dynamic alpha3 bounded wire與alpha1 same-origin client已編譯，500 bars/20 polls真host Playwright仍待。 |
| DYN-TA-T-011 | REQ-013 | Bounds | every hard limit preserves last-good Canvas and reports reason | DYN-TA-001..003 | Partial：Contracts/reducer hard limits、Renderer bounded window與stale/error last-good Canvas body-count preservation pass；invalid frame真host visual preservation仍待。 |
| DYN-TA-T-012 | REQ-016 | Browser | Live/Delayed/Stale/Backfill/Unavailable and watermark/lag/quality visible | DYN-TA-003 | Partial：typed model覆蓋五種freshness；Playwright覆蓋Live/Stale、watermark、quality、recoverable error與Ready recovery。Delayed/Backfill/Unavailable browser matrix仍待。 |
| DYN-TA-T-013 | REQ-017 | Contract parity | PTCS/E2EQ adapters produce identical final reducer state | DYN-TA-004/005 | Partial：Dynamic.Ptcs alpha3使用canonical reducer、legacy/browser wire與session isolation 4/4 pass；Ptcs.Client exact-package projection/action tests 2/2；E2EQ adapter待補。 |
| DYN-TA-T-014 | REQ-017 | Browser parity | two hosts have equivalent chart/rows/toolbar geometry and actions | DYN-TA-005/006 | Ready |
| DYN-TA-T-015 | REQ-015 | Source gate | new runtime has no JavaScript/inline/global callback workaround | DYN-TA-001..003 | Pass：Contracts與Renderer source gate無`JS.Inline`、script、eval或global callback workaround。 |
| DYN-TA-T-016 | REQ-018 | Regression | static Canvas/FormInput/Argu/ActorsPage and facade remain compatible | DYN-TA-00A/004/007 | Partial：Contracts加入後legacy Dynamic Release WebSharper build與Expecto 21/21通過；facade delegation/absence-invalid behavior仍待DYN-TA-004/007。 |
| DYN-TA-T-017 | REQ-014 | Extension behavior | absent uses host fallback；present-invalid shows controlled error | DYN-TA-00A/004 | Partial：server invalid payload fail closed；browser absent/present-invalid visual gate待補 |
| DYN-TA-T-018 | REQ-008 | Dependency | Contracts/Renderer graphs exclude forbidden dependencies | DYN-TA-001 | Pass：Contracts無WebSharper/PTCS/fCell2/PTMD/SQL；Renderer僅增加WebSharper並排除PTCS/fCell2/PTMD/SQL。 |
| DYN-TA-T-019 | REQ-017 | E2EQ AgentE2E | Historical/RT source/symbol/range/hover/tag/viewport/navigator regression | DYN-TA-005 | Ready |
| DYN-TA-T-020 | REQ-013/016 | Soak | bounded polling does not grow timers/channels/DOM series/history | DYN-TA-006 | Ready |

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
