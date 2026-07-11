# REQ-PTCS-DYNAMIC-TA-0001 Realtime TA Canvas Runtime

Status: Proposed / Review required
Date: 2026-07-11
Owner: `PulseTrade.Comm.Spa.Dynamic`
RFC: `doc/RFC/RFC-PTCS-DYNAMIC-0007.realtime-ta-canvas-runtime.md`
SA: `doc/TAResearch/SA.md`

## 1. 背景

現有 `fskynet-sdui` Canvas 可把單次 actor reply render 成展開式畫布，但沒有持續存在的 document instance、data revision、incremental patch、poll lifecycle或TA chart node。文件曾列出 `RealtimeChart`，正式 renderer尚未實作。

新需求以歷史研究為主：第一次載入歷史OHLCV/TA rows，之後每5秒透過WebSocket pull最新delta。互動體驗需接近Plotly的zoom/pan/toggle/mode switch，但實作必須是純WebSharper F#，不得新增JavaScript/inline JavaScript。

## 2. 邊界

- Dynamic owns：SDUI runtime contract、typed codec、Canvas state、TA renderer、local interaction、poll controller與snapshot/patch reducer。
- PTCS owns：authenticated WebSocket extension channel、selected target command callback、ACL/session context與transient update delivery seam。
- PTCS.Host owns：native TA query actor、Argu DU、PTMD query provider與response mapping。
- PTMD owns：market-data persistence/query/analytics。

Dynamic不得reference PTMD、broker SDK、SQL client或PTCS.Host executable。

## 3. 功能需求

| ID | Requirement |
| --- | --- |
| DYN-TA-REQ-001 | 第一個成功reply建立immutable `SduiDocument`，以`documentId + canvasInstanceId + documentRevision`識別；一般data update不得替換layout。 |
| DYN-TA-REQ-002 | runtime envelope需區分`document`、`snapshot`、`patch`、`error`、`heartbeat`；data revision與transport sequence獨立。 |
| DYN-TA-REQ-003 | patch只允許typed operations：replace dataRef、upsert series points、remove-before、set status/options；禁止arbitrary script、DOM selector、JSON pointer或URL。 |
| DYN-TA-REQ-004 | TA Canvas至少支援Candlestick、Volume、SMA、DMI/ADX、MACD、Heikin-Ashi row，並可由data-driven row list新增/移除。 |
| DYN-TA-REQ-005 | chart支援zoom、pan、crosshair、legend toggle、row visibility、mode switch、parameter change；純view操作不得送network request。 |
| DYN-TA-REQ-006 | `ResetView`只還原zoom/pan/toggle；`ResetCanvas`還原initial rows/query/view並要求fresh snapshot。 |
| DYN-TA-REQ-007 | remote parameter/range/scale/change與Add Row透過registered target command callback送出，不得直接呼叫SQL/provider/arbitrary HTTP。 |
| DYN-TA-REQ-008 | client-pull預設5秒，最小5秒；只在Canvas mounted、expanded、page visible且socket ready時poll。 |
| DYN-TA-REQ-009 | 同一canvas最多一個in-flight poll；timeout/backoff後可恢復，close/unmount必須取消timer與socket subscription。 |
| DYN-TA-REQ-010 | revision gap、unknown instance、base revision mismatch需發resync request；不得silent套用out-of-order patch。 |
| DYN-TA-REQ-011 | history/document與poll update分流：poll patch不得每5秒新增chat history card或IndexedDB message row。 |
| DYN-TA-REQ-012 | initial snapshot、series count、bars/row、patch items與browser working set都有硬上限；超限顯示controlled error。 |
| DYN-TA-REQ-013 | Dynamic extension absent時PTCS維持raw/fallback UI；extension present但invalid runtime schema時顯示controlled error，不silent fallback成看似成功Canvas。 |
| DYN-TA-REQ-014 | TA runtime path必須用typed F# codec與WebSharper APIs；禁止`JS.Inline`、手寫`.js`或string-built script。 |
| DYN-TA-REQ-015 | Canvas顯示backend、watermark、lag、partial/sealed、quality/stale狀態，避免研究者把舊資料誤認realtime。 |

## 4. 操作情境

1. User選TA target，送出source/symbol/range/rows；actor reply `document` + initial snapshot，Canvas展開。
2. User zoom/pan/toggle；只改local view state。
3. 每5秒Dynamic透過PTCS WebSocket channel送`poll`，收到patch後只更新尾端K棒。
4. User選Add Row，指定Scale/Kind/Range/indicator parameters；送remote command，收到snapshot/patch後row stack更新。
5. User按Reset Canvas；回到initial query/rows/view並resync snapshot。
6. socket斷線後停止poll；重連時以last revision要求delta，gap則full resync。

## 5. 驗收

1. 500根history初始load後，連續至少20個5秒poll不增加message cards，只有data revision前進。
2. 新K棒upsert後Candlestick/Volume/indicator rows同步更新，layout document revision不變。
3. zoom/pan/toggle不發送WebSocket frame；remote parameter change會送一次command。
4. gap/out-of-order/duplicate patch都有deterministic reducer test。
5. page切走、Canvas close、browser disconnect後timer與subscription均釋放。
6. Playwright以人類操作驗證zoom/pan/reset/add-row/toggle/resize與錯誤狀態，console無error。

## 6. Upstream dependency

目前PTCS renderer seam只有`string -> Node option`，沒有duplex lifecycle/transient update contract。進入DEV前必須接受一份PTCS core companion RFC，提供authenticated WebSocket extension channel、mount/unmount與target submit callback；不得以每5秒新增history reply或Dynamic自行猜PTCS internal socket作workaround。
