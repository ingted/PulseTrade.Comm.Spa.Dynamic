# RFC-PTCS-DYNAMIC-0025 Chunked Snapshot Transport

- ID：RFC-PTCS-DYNAMIC-0025
- 狀態：Accepted / Implemented
- Owner：PTCS Dynamic Contracts／Interactive.Client
- 關聯：RFC-PTCS-DYNAMIC-0021、RFC-PTCS-DYNAMIC-0024、DYN-WBS-546、DYN-WBS-557

## 背景

Owner browser gate以4,000個canonical slots、28個data refs與5列candlestick量測完整Snapshot。既有FramePump已把SduiValue array切成256-point browser tasks，但仍須先對17.3MB encoded RuntimeFrame做一次`JSON.Parse`；具名stage量測為約74ms，忙碌／GC條件下trace曾達115–160ms。後續單一decode batch約12ms，證明主要尖峰在完整wire envelope parse，而非point decoder。

RFC-0021明列不改wire schema；因此不能在該RFC內隱性改約。RFC-0025以獨立、向後相容的transport framing解除單一巨大parse，同時保留canonical RuntimeFrame、reducer、cache與provider authority。

## 目標

1. Snapshot可被編碼為bounded start／item／commit packets；Document、Patch及legacy完整RuntimeFrame維持可用。
2. Browser message callback只enqueue；每個packet在獨立scheduled task解析，單一dataRef的point array仍使用既有256-point phased decode。
3. 完整batch通過count、index、batch id、dataRef uniqueness、generation及canonical reducer validation後，才一次發布candidate並觸發client-local `SnapshotAccepted` lifecycle。
4. 任何partial、duplicate、out-of-order、stale或invalid packet都保留last-good，不推進revision、不具cache write eligibility，也不觸發accepted lifecycle。
5. 4,000 slots／28 refs／5 candle rows的cold ingest與same-topology replacement均不得留下大於100ms的target-renderer task。

## 非目標

- 不改`RuntimeFrame`／`RuntimePayload` domain DU、provider authority、document/data revision、cache schema或renderer contract。
- 不引入partial reducer state、partial UI publish、資料截斷、worker-only第二套codec或consumer私有wire格式。
- 不要求一次移除legacy完整Snapshot；舊producer與舊recorded frame仍須相容。
- 不在PTCS owner package實作FSSTL、MDCQ、TA/backtest domain mapping。

## 情境

### 初次連線與resync

Producer把一個canonical Snapshot轉成同一batch的start、依dataRef排序的item packets與commit。Client逐包stage；commit完成前維持既有畫面。驗證成功後以原Snapshot sequence/revision一次替換state，成為可供`BrowserRuntimeCache`持久化的accepted projection，並觸發client-local `SnapshotAccepted` lifecycle；現行wire沒有snapshot ACK frame。

### Legacy producer

收到沒有`ptcs-dynamic-snapshot-chunk.v1` schema的訊息時，Client走既有RuntimeFrame decode／reduce。相容路徑仍須保留last-good與generation cancellation。

### 中斷或交錯

socket generation改變、下一個start到達、item缺漏／重複／亂序、batch id或count不符時，未完成batch作廢。非Snapshot canonical frame不得插入active batch；Client回structured recoverable error並請求完整resync，不發布staged data。

## 決策

### Wire envelope

Transport schema固定為`ptcs-dynamic-snapshot-chunk.v1`：

- `start`：`batchId`、`itemCount`、`header`。`header`是原RuntimeFrame，但Snapshot.Data必須為空。
- `item`：`batchId`、zero-based `itemIndex`、`dataRef`、raw `SduiValue`。
- `commit`：`batchId`、`itemCount`。

Contracts提供唯一encoder，把合法Snapshot轉成deterministic packet string array；Map依canonical dataRef順序輸出。非Snapshot回單一legacy RuntimeFrame string。Consumer host只呼叫encoder，不手刻JSON或自行決定batch順序。

### Browser ingestion

Interactive.Client維護每個transport generation最多一個active batch及bounded encoded-message queue。WebSocket `OnMessage`不直接decode/reduce；scheduled pump依序處理。item先做envelope metadata檢查，再沿用FramePump phased SduiValue decoder。commit組回一個canonical Snapshot，沿用既有`RuntimeReducer.preflight`、axis authority、temporal與overlay validation。

### Authority與accepted lifecycle

Transport packet不是runtime truth，也不得單獨進cache。唯一truth仍是commit後被canonical reducer接受的RuntimeState。`SnapshotAccepted`只在完整candidate發布後觸發一次，用來結束resync/backoff；reject／superseded／disconnect不得觸發。Browser cache由consumer顯式使用`BrowserRuntimeCache`保存accepted projection，`Client.fs`不隱式寫入。現行`RuntimeClientFrame`沒有snapshot transport ACK。

### Consumer integration

Daedalus SessionHost現有`Frames: string array`與`sendFullSnapshot`可保持形狀，只把每個RuntimeFrame經Contracts encoder展開後flatten。Aster發布final exact graph與helper API後，consumer才升版；不得在owner gate前移動其現行exact graph。

## 取捨

- packet數增加，但每包parse與decode有明確上限，並可由既有WebSocket ordering及generation guard維持順序。
- staged candidate短暫占用一份完整typed data；這與legacy reducer相同量級，但避免同時保留17MB parsed envelope的單一main-thread尖峰。queue與item count受runtime limits限制。
- legacy完整Snapshot仍可能超過100ms；它是相容入口，不是新producer的production large-snapshot路徑。large-snapshot硬gate使用chunked encoder。

## 影響

- Contracts：新增transport packet schema與deterministic encoder，不改RuntimePayload。
- Interactive.Client：新增packet staging／generation-safe queue，production WebSocket client改用同一pump。
- Renderer：不新增domain行為；只重跑regression與long-task gate。
- Dynamic.Ptcs／Ptcs.Client：只因exact dependency closure升版。
- SPAA／Interactive Extension：final graph後把initial/reconnect Snapshot交給encoder展開。

## 驗收

1. Encoder對Snapshot產生1 start、N items、1 commit；header Data為空，items順序穩定，非Snapshot維持單一legacy frame。
2. Client unit/browser tests覆蓋legacy、success、missing、duplicate、out-of-order、batch/count mismatch、stale generation、disconnect與invalid item；失敗皆保留last-good且不觸發accepted lifecycle。
3. Production `Client.fs`使用message queue/pump，不再於WebSocket callback直接decode大型frame。
4. BrowserDemo以同一production packet pump驗4,000 slots／28 refs／5 candle rows；cold ingest與replacement target renderer皆無大於100ms task。
5. Browser cache cold/cache gate、Renderer focused suite及PTCS exact downstream suites通過。
6. 發布唯一全新exact package graph，fresh cache只走official source readback後，才交付Daedalus consumer integration。

## 關聯追溯

- REQ：`doc/REQ.md`「2026-09-25 Chunked Snapshot Transport」
- SA：`doc/SA.md`「Chunked Snapshot transport boundary」
- SD：`doc/SD.md`「Runtime snapshot transport framing」
- WBS：DYN-WBS-557
- Test：DYN-T-583..588

## 實作結果

- Contracts提供deterministic start／ordered item／commit encoder；Interactive.Client以generation-safe queue、packet staging與single-candidate reducer提交，production lifecycle與cache browser gates皆通過。
- 五列4,000-slot candle fixture揭露舊projection的per-slot tuple＋`groupBy`配置成本；Renderer改為固定bucket array單次聚合first-open／last-close／high／low／volume，保留source interval、Y-domain與cursor語意。Official exact package browser gate的replacement max為92.04ms，所有受驗renderer phase皆無大於100ms task。
- Row cursor timestamp依consumer feedback移至SVG外固定32px gutter的固定CSS-pixel兩行tag；row resize前後以bounding box及CDP computed style驗證，除top/left定位外不得改變。
- Final exact graph：Contracts `0.1.22`、Renderer `0.1.49`、Interactive.Client `0.1.41`、Dynamic.Ptcs `0.1.44`、Ptcs.Client `0.1.63`。五包均由NuGet.org official readback驗repository signature；fresh official caches完成34/45/12/14/16 focused tests及三組F# Playwright gates。
