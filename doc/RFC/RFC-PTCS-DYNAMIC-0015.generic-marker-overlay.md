# RFC-PTCS-DYNAMIC-0015：Generic Marker Overlay

- ID：`RFC-PTCS-DYNAMIC-0015`
- 狀態：`Implemented / Consumer Handoff`
- Owner：Aster（PTCS Dynamic Contracts／Reducer／Renderer／Client）
- Consumer owner：Daedalus（FSSTL／TradeCore／SPAA／Interactive Extension）
- 日期：2026-09-21
- 上游需求：`G:/coldfar_py/coldfar-symbolics/doc_new2/RFC/RFC-TRADECORE-0025.PTCS-GenericMarker.REQ.md`
- 邊界回饋：`G:/coldfar_py/coldfar-symbolics/doc_new2/RFC/RFC-TRADECORE-0025.PTCS-GenericMarker_review_feedback.md`

## 1. 背景

TradeCore／SPAA 需要把 signal、entry、exit、fill 等 domain event 投影成通用 TA chart marker。PTCS Dynamic 目前只有 candlestick、volume、line、histogram trace；consumer 若自行畫 SVG，會造成 reducer validation、position 語意、stacking、版本與 package closure 分裂。

本 RFC 只定義 generic presentation contract。FSSTL、DMI、ARM、order、fill、PnL、run identity、summary／trades 原子切換仍由 Daedalus 的 projection／`BacktestWorkspaceState` 負責。

## 2. 目標

1. 在既有 `RuntimeFrame -> RuntimeValidation -> RuntimeReducer -> Renderer` 管線加入 generic marker。
2. Document、Snapshot、Patch 均在 authoritative commit 前驗證完整 candidate state；拒絕時保留 last-good。
3. `TemporalSeriesPoint.Position` 是唯一 spatial authority；`EventTimeUtc` 只作 evidence／tooltip。
4. 同 position／anchor marker deterministic stacking、bounded、不可改變 Y autoscale或侵入相鄰 row。
5. 不相容 client 在 commit 前明確拒絕 marker frame，不能降級成 candle／line／text。
6. SPAA 與 Interactive Extension 只需引用相同 Contracts／Renderer package，即得到相同結果。

## 3. 非目標

- 不理解或保存 signal／order／fill／PnL domain state。
- 不替 producer 決定 event 應落在哪一根 base-axis candle。
- 不把 summary、trades 與 marker 放進同一 RuntimeFrame transaction。
- 不新增通用 capability negotiation framework。
- 第一版不支援任意 SVG、HTML、CSS function、URL、script、任意 price anchor或 marker click action。

## 4. 決策

### 4.1 Public types

`TaTraceKind` additive 新增 `Marker`。Marker point 仍使用既有 `TemporalSeries`；point 的 `Position` 是 bucket key，`Value` 是 marker array。

```fsharp
[<RequireQualifiedAccess>]
type TaMarkerAnchor = AboveBar | BelowBar

[<RequireQualifiedAccess>]
type TaMarkerShape = Arrow | Circle | Square | Diamond

[<RequireQualifiedAccess>]
type TaMarkerFill = Solid | Outline

type TaMarkerTooltipField =
    { Key: string
      Label: string
      Value: string }

type TaMarker =
    { MarkerId: string
      EventTimeUtc: string
      Anchor: TaMarkerAnchor
      Shape: TaMarkerShape
      Fill: TaMarkerFill
      Color: string
      Label: string option
      Tooltip: TaMarkerTooltipField array }

type TaMarkerTraceOptions =
    { TargetTraceId: string }
```

`TaMarkerCodec` 負責 strict `TaMarker <-> SduiValue`；`TaMarkerTraceOptionsCodec` 透過 trace `Options["marker.targetTraceId"]` 編解碼。第一版不在 `TaTraceSpec` 新增欄位，避免所有非 marker record constructor 被無意義打破；typed helper 是唯一合法 options authoring API。

### 4.2 Exact wire schema

Marker bucket：

```json
[
  {
    "_type": "ta-marker.v1",
    "markerId": "entry-0001",
    "eventTimeUtc": "2026-09-21T01:02:03.4560000+00:00",
    "anchor": "below-bar",
    "shape": "arrow",
    "fill": "solid",
    "color": "#16a34a",
    "label": "L",
    "tooltip": [
      { "key": "strategy", "label": "策略", "value": "DMI" }
    ]
  }
]
```

規則：

- required：`_type`、`markerId`、`eventTimeUtc`、`anchor`、`shape`、`fill`、`color`、`tooltip`。
- optional：`label`；省略與 JSON null 都映射 `None`。
- unknown field：拒絕；duplicate JSON field 由 wire decoder 拒絕。
- time：canonical bounded ISO-8601 UTC string，必須以`Z`或`+00:00`結尾；採string是因同一Contracts codec須可由WebSharper server/browser共用，renderer不解析或用它定位。
- color：只接受 `#RGB`、`#RRGGBB`、`#RRGGBBAA` ASCII hex；拒絕 CSS function、named color、`url()`。
- empty bucket：snapshot 可省略；patch 以該 position 的 empty array 表示 clear。
- tooltip 保持 array order；不可轉成 `Map`。

Hard limits：

| 項目 | 限制 |
| --- | ---: |
| MarkerId | 128 chars |
| Label | 64 chars |
| Tooltip fields / marker | 16 |
| Tooltip key／label | 64 chars |
| Tooltip value | 256 chars |
| Markers / position bucket | 4 |
| Markers / DataRef | 10,000 |
| Markers / accepted frame candidate | 20,000 |

既有 `MaxInitialBarsPerSeries`、`MaxRetainedBarsPerSeries`、`MaxPatchItems`、`MaxFrameBytes` 仍同時生效。Marker count 是 bucket 內 item 總數，不是 temporal point count。

### 4.3 Document contract與target candle

Marker trace 必須：

1. 與 target trace 位於同一 row。
2. `Options["marker.targetTraceId"]` 經 typed helper解析後指向該 row 的另一個 trace。
3. target kind 是 `Candlestick`，不得 self-reference。
4. target candle 可由現有 canonical candle resolver解析；composite `DataRef` 與 `CandleDataRefs` 都合法。
5. marker trace `DataRef` 指向 `TemporalSeries`，其 `AxisRef/AxisRevision` 必須與 target可解析位置相容。

Document validation 不硬性要求 `CandleDataRefs.IsSome`，也不理解 producer domain。

### 4.4 Authoritative validation與reducer result

Validation 分兩層：

```text
wire/frame shape validation
  -> apply document/snapshot/whole patch to candidate
  -> validate document + candidate marker semantics
  -> commit candidate revision OR retain exact last-good
```

Candidate gate 驗：target trace、temporal axis revision、每個 marker position 存在、target candle同 position可解析、bucket/series/frame limits、MarkerId 在單一 marker DataRef 全域唯一。

Identity／mutation：

- 同 id、同 position、相同 payload重送為 idempotent。
- 同 frame move 必須 clear old bucket並寫 new bucket，或 `ReplaceDataRef`；只寫 new position造成 duplicate id時整批拒絕。
- 多 operation patch以最終 candidate判斷，不能逐 operation 提前 commit。
- invalid candidate 不得過濾壞 marker後 commit，也不得推進 data revision／transport sequence。

Failure classification：

| 類型 | Runtime state | Effect |
| --- | --- | --- |
| revision gap、missing axis/snapshot、unknown current position | last-good + `PausedForResync` | `RequestResync` |
| malformed marker、duplicate id、illegal enum/color、hard-limit violation、invalid target contract | last-good + `Suspended` | `RejectFrame(canvasId, RuntimeError Recoverable=false)` |

`RejectFrame` 是 structured non-recoverable receipt，避免同一份壞 payload無限 resync。Consumer可記 reason code；不得把 raw payload灌進 UI。

### 4.5 Spatial authority、stacking與edge geometry

- x：只由 marker point `Position` 對應 base-axis slot。
- y anchor：只由 canonical target candle同 position的 `High/Low` 決定 stem起點。
- `EventTimeUtc` 不參與 bar lookup、修正或 fallback。
- presentation order = bucket array order；codec 必須保序，producer必須 deterministic。

Marker row 使用固定 geometry：

- size `9px`、first gap `4px`、stack gap `2px`。
- max 4 markers／bucket／anchor。
- 有 marker trace 的 row 永久保留 top/bottom 各 `44px` overlay lane；是否當下有 marker不改 layout或Y domain。
- above marker 由 top lane靠近plot的一側往上 stack；below marker由 bottom lane靠近plot的一側往下 stack。
- stem連接 candle high/low與lane；marker bbox clamp在該 row SVG範圍。
- marker geometry不加入 price min/max；overlay clip到本 row，不能侵入相鄰 row。

同 position／anchor以bucket order堆疊；不同 marker trace以 document trace order後接 bucket order，形成唯一 deterministic order。

### 4.6 Visibility與tooltip

- marker trace `Visible=false` 時，不畫 marker/stem，也不建立 hit target。
- target trace可隱藏但資料必須可解析；marker仍可顯示。
- viewport外 marker不建立 DOM/SVG node。
- tooltip欄位依 wire array order顯示；固定先顯示 label（若有）、EventTimeUtc，再顯示 bounded fields。
- cursor/detail不得把 marker count加入Y-scale或重建未變動candle series。

### 4.7 Protocol、舊cache與package closure

選擇 runtime protocol bump，不建立 marker專用 negotiation framework：

- legacy：`sdui-runtime.v1`
- marker-capable：`sdui-runtime.v2`
- 含 Marker trace的 Document/Snapshot/Patch 必須使用 v2。
- v1 frame若宣告 Marker，structured reject `marker-requires-runtime-v2`。
- v2 client仍接受沒有 marker的v1文件，維持既有TA流程。
- manual browser wire parser不得把 unknown trace kind fallback成Candlestick；unknown kind fail closed。

Browser cache identity 的 schema revision必須升版；舊 entry decode後若 protocol/capability不符即刪除並要求authoritative snapshot，不遷移 marker payload。

最小consumer矩陣：

| Consumer | 必須同步 |
| --- | --- |
| Contracts | marker types/codec/reducer v2 |
| Renderer | marker overlay與exact Contracts |
| Interactive.Client | v2 decode/rejection、exact Contracts/Renderer |
| Ptcs.Client | marker wire projection、unknown kind fail closed |
| SPAA／Interactive Extension | exact新Contracts/Renderer；producer以typed codec建立marker |

### 4.8 Performance invariant

- marker-only patch不得重新decode未變 candle／TA dataRef。
- candidate marker validation成本與 marker bucket/marker count線性相關；不得掃描無關 non-marker series payload。
- renderer只建立visible marker nodes。
- 基準記錄 total bars、visible bars、marker density、browser與硬體；量測 codec/validation、patch allocation、node count、zoom/pan/cursor long task與frame cadence。
- release gate以同環境相對無marker baseline比較；具體門檻在PTCS Dynamic `SD/Test`凍結，不在本RFC捏造跨機絕對值。

## 5. 替代方案

1. **Consumer私有SVG**：拒絕。會分裂 validation、layout與package語意。
2. **Marker直接帶Timestamp/Value/AtValue**：拒絕。會讓PTCS重做domain/calendar mapping並產生雙spatial authority。
3. **在TaTraceSpec新增MarkerOptions欄位**：首版拒絕。會迫使所有非marker F# record constructor source migration；typed options helper可提供同等安全性。
4. **新增通用capability negotiation service**：拒絕。v2 protocol＋exact package closure已能提供最小隔離；若真實部署證明無法阻止舊bundle，再另RFC擴充registration handshake。
5. **invalid marker過濾後繼續commit**：拒絕。會讓revision聲稱的authoritative state與producer不同。

## 6. 實作切片

| Slice | 內容 | Gate |
| --- | --- | --- |
| GM-001 | types、strict codec、limits、v2 protocol | codec round-trip／unknown field／color／bounds |
| GM-002 | document＋candidate semantic validation、RejectFrame | snapshot/patch保留last-good；recoverable/non-recoverable分類 |
| GM-003 | renderer model＋fixed marker lanes／stacking／tooltip | geometry model tests、Y-scale invariant、visible-only nodes |
| GM-004 | Browser/Ptcs client v2與unknown-kind fail closed | old v1、marker v2、old cache rejection |
| GM-005 | package cascade、SPAA/DIExt handoff、browser/perf | exact package matrix、same frame identical render、baseline report |

## 7. 驗收

1. Producer只靠Contracts public API可建立合法 marker frame。
2. Snapshot、ReplaceDataRef、upsert、clear、same-frame move、idempotent resend均有測試。
3. malformed schema、duplicate id、missing/invalid target、unknown position、stale axis revision、oversize bucket/series/frame均不改last-good。
4. recoverable gap只產生一次resync；producer violation回structured non-recoverable rejection。
5. composite candle與split OHLC candle都可作target。
6. 同 position/anchor順序固定、row edge不clip、不侵入相鄰row、marker不改Y autoscale。
7. v1 non-marker正常；v1 marker拒絕；v2 marker正常；unknown trace kind不降級。
8. SPAA與DIExt引用同一版Contracts/Renderer時，同frame輸出一致。

## 8. Rollback

Producer停止宣告 Marker trace並回到v1 non-marker frame即可回退。Renderer／Contracts新版本保留v1相容；已寫入的v2 marker browser cache因schema revision隔離，不回灌v1 runtime。Rollback不改TradeCore domain event或Backtest結果。

## 9. 關聯

- `RFC-PTCS-DYNAMIC-0007.realtime-ta-canvas-runtime.md`
- `RFC-PTCS-DYNAMIC-0013.notebook-ta-workspace-production.md`
- `RFC-PTCS-DYNAMIC-0014.browser-range-cache-resume.md`
- `RFC-TRADECORE-0025.PTCS-GenericMarker.REQ.md`
- `RFC-TRADECORE-0025.PTCS-GenericMarker.RFC.md`
- `RFC-TRADECORE-0025.PTCS-GenericMarker_review.md`
- `RFC-TRADECORE-0025.PTCS-GenericMarker_review_feedback.md`

## 10. 實作結果（2026-09-21）

- `GM-001..004`完成：aggregate Dynamic `0.1.24`、Contracts `0.1.12`、Renderer `0.1.29`、Dynamic.Ptcs `0.1.37`、Ptcs.Client `0.1.47`、Interactive.Client `0.1.24`均已發布。
- typed/decoded candidate在commit前共用semantic gate；malformed/duplicate回`RejectFrame`，recoverable missing data回`RequestResync`，兩者均不覆蓋last-good。
- Browser marker overlay通過desktop/mobile真DOM驗證；同position lane固定、tooltip與row clipping正常、marker不進Y-domain或numeric legend。
- Owner suites為Contracts 27/27、Renderer 30/30、PTCS adapter 14/14、PTCS client 16/16、Interactive client 4/4。3,820 bars fixture的200次cursor transition總3597ms、max54ms。
- PTCS Host active graph已同步Dynamic.Ptcs `0.1.37`、Contracts `0.1.12`與Host TA client bundle `0.1.26`；Release build通過。
- Consumer handoff：Daedalus需將Interactive Extension升至Contracts `[0.1.12]`、Interactive.Client `[0.1.24]`，完成同一runtime v2 frame的Notebook真路徑驗收；domain projection與SPAA transaction不移入PTCS Dynamic。
