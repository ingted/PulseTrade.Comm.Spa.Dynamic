# REQ-PTCS-DYNAMIC-TA-0001 Realtime TA Canvas Runtime

Status: Accepted / Ready for DEV
Date: 2026-07-11
Owner: `PulseTrade.Comm.Spa.Dynamic*` packages
RFC: `doc/RFC/RFC-PTCS-DYNAMIC-0007.realtime-ta-canvas-runtime.md`
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
| `PulseTrade.Comm.Spa.Dynamic.Contracts` | SDUI document/snapshot/patch/action/freshness DTO、strict codec、limits與revision rules；無WebSharper/PTCS/PTMD依賴。 |
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
| DYN-TA-REQ-011 | revision gap、unknown instance、base mismatch產生resync effect；duplicate為no-op，out-of-order不得silent套用。 |
| DYN-TA-REQ-012 | snapshot/patch/heartbeat不得每次新增chat history或IndexedDB message row；durable audit政策由host adapter決定。 |
| DYN-TA-REQ-013 | initial snapshot、rows、bars、patch items與browser working set皆有hard limits；超限保留last-good canvas並顯示controlled error。 |
| DYN-TA-REQ-014 | extension absent時host可維持原fallback；extension存在但schema/type invalid時fail visibly，不silent當成功。 |
| DYN-TA-REQ-015 | 新runtime只用typed F# codec與WebSharper API；禁止`JS.Inline`、手寫`.js`或string-built script。 |
| DYN-TA-REQ-016 | Canvas顯示backend、coverage、watermark、lag、partial/sealed、quality與`Live/Delayed/Stale/Backfill/Unavailable`。 |
| DYN-TA-REQ-017 | PTCS adapter與E2EQ adapter對同一RuntimeFrame sequence必須得到等價renderer state/geometry；不得維護兩套TA renderer。 |
| DYN-TA-REQ-018 | 現有static Canvas payload保持相容；runtime v1只有在明確protocol時啟用。 |

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
