# RFC-PTCS-DYNAMIC-0024：Backtest Presentation UX Generic Contracts

- ID：RFC-PTCS-DYNAMIC-0024
- 狀態：Implemented／owner release complete；consumer adoption pending
- 日期：2026-09-24
- Owner：Aster／PTCS Dynamic
- Consumer owner：Daedalus／TradeCore、DIExt、SPAA
- 上游需求：`G:/coldfar_py/coldfar-symbolics/doc_new2/RFC/RFC-TRADECORE-0028.PTCS-BacktestPresentationUX.REQ.md`
- 協作提案：`G:/coldfar_py/coldfar-symbolics/doc_new2/RFC/RFC-TRADECORE-0028.PTCS-BacktestPresentationUX.RFC.md`
- Owner review：`doc/RFC/RFC-TRADECORE-0028.PTCS-BacktestPresentationUX.OwnerReview.md`
- Consumer feedback：`G:/coldfar_py/coldfar-symbolics/doc_new2/RFC/RFC-TRADECORE-0028.PTCS-BacktestPresentationUX.OwnerReview.Feedback.md`

## 1. 背景

Backtest workspace 已能把 Fill 映射為 generic marker，但 navigator 尚無 event-time stripe、marker wire hard limit 與畫面 glyph budget 尚未分離、row 高度仍由固定像素與 marker presence 決定。Scenario 切換也需要在不重建 market／TA base graph 的前提下，原子替換結果 overlay。

TradeCore 擁有 Signal／Order／Fill、scenario identity 與帳務結果；PTCS Dynamic 只擁有 domain-neutral wire、candidate validation、renderer geometry、local interaction 及 package closure。兩邊已接受 owner review 的五項決策，本 RFC 將其固定成可開發的 owner contract。

## 2. 目標

1. 提供 generic event-time overview stripe trace，不在 PTCS contract 出現 Signal／Order／Fill domain DU。
2. 將 marker wire capacity 與直接可見 glyph budget 分開，保留全部 stable identities 並提供可操作的 `+N` cluster。
3. 讓 `TaRowSpec.HeightWeight` 成為 authored default，並提供 canvas-local row resize／reset。
4. 保證單一 `RuntimePatch` 內多個 overlay `ReplaceDataRef` 經完整 candidate validation 後一次 commit，失敗維持 last-good。
5. 保持 K／TA base graph、data readers、Y-domain、shared cursor 與 provider query 不因 scenario overlay 或 row resize重建。
6. 提供 deterministic unit、browser、performance 與 exact-package gates，讓 Daedalus 可安全承接真 SPAA／`.dib` integration。

## 3. 非目標

1. PTCS 不定義 Signal／Order／Fill、PnL、scenario 或 backtest result 型別。
2. PTCS 不解析 scenario id 字串，也不產生 domain palette、display name 或 stable domain identity。
3. PTCS 不擁有 `selectionGeneration`、summary／trades／timeline／download manifest candidate；這些由 Daedalus consumer prepare／commit。
4. 本輪不新增 durable row-height preference、server API、provider query 或 browser reload persistence。
5. 本輪不另建 websocket、store、navigator renderer 或 Notebook-specific renderer。

## 4. 情境

### 4.1 Overview event stripes

Consumer 將 strategy signal 與 fill 各自投影成 generic stripe trace。Renderer 依 canonical temporal axis 對 event time 定位；close path 可以 sampling，但 stripe 不可先 sampling 再定位。

同一 collision group、相同 X pixel：

- 單一 trace 佔滿 navigator plot height；
- 多個 trace 依 `LayerOrder`、再依 document trace order切成垂直 lanes；
- 同一 trace 多筆事件只畫一條線，但 prepared bucket保留全部 ids、count、label、tooltip；
- 不以水平位移偽造 event time。

### 4.2 Dense marker lane

同一 row／target／position／anchor 最多接受 64 筆 marker。前四筆依既有 deterministic order直接畫 glyph，其餘由一個 `+N` control代表；wire 不截斷，cluster 可以 keyboard／focus逐筆查看。

### 4.3 Row resize

Trader 拖曳 row 底部 separator，只改該 canvas instance 的 resolved height。Scenario 切換保留 override；Reset、double-click、browser reload 或 fresh kernel 回到 `HeightWeight` default。

### 4.4 Atomic scenario overlay replacement

Consumer 在一個 `RuntimeFrame`／revision 中送出單一 `RuntimePatch`，至少包含 overview stripes、order markers、fill markers 三個 `ReplaceDataRef`。Reducer 對整個 candidate 執行 shape／semantic validation；任一 operation 失敗都不得發布部分新狀態。

## 5. 決策

### 5.1 Public F# contracts

`PulseTrade.Comm.Spa.Dynamic.Contracts` 新增：

```fsharp
[<RequireQualifiedAccess>]
type TaTraceKind =
    | Candlestick
    | Volume
    | Line
    | Histogram
    | Marker
    | OverviewStripe

type TaOverviewStripe =
    { StripeId: string
      EventTimeUtc: string
      Color: string
      StrokeWidthCssPixels: float
      Label: string option
      Tooltip: TaMarkerTooltipField array }

type TaOverviewStripeTraceOptions =
    { TargetTraceId: string
      CollisionGroup: string
      LayerOrder: int }
```

Public modules：

- `TaOverviewStripeLimits`
- `TaOverviewStripeCodec`
- `TaOverviewStripeTraceOptionsCodec`
- `TaOverviewStripeContract`

`TaMarkerTooltipField` 是現有 domain-neutral key／label／value contract，stripe 復用而不另造 tooltip 型別。

### 5.2 Wire schema and option keys

Stripe item：

```text
_type                    = ta-overview-stripe.v1
stripeId                 = nonblank stable id
eventTimeUtc             = ISO-8601 UTC, Z or +00:00
color                    = #RGB / #RRGGBB / #RRGGBBAA
strokeWidthCssPixels     = 0.5 .. 4.0
label                    = optional bounded text
tooltip                  = bounded TaMarkerTooltipField[]
```

Trace options：

```text
overviewStripe.targetTraceId
overviewStripe.collisionGroup
overviewStripe.layerOrder
```

Target trace 必須存在、同 row、使用 shared temporal axis，且為 `Candlestick`。Consumer 不得傳 SVG 座標、pixel index、sample index 或 domain event kind。

### 5.3 Limits

| Contract | Limit |
| --- | ---: |
| Stripe id | 128 chars |
| Stripe label | 64 chars |
| Collision group | 64 chars |
| Tooltip fields | 16 |
| Tooltip key／label／value | 沿用 marker 的 64／64／256 chars |
| Layer order | 0..63 |
| Stripes per temporal bucket | 64 |
| Stripes per data ref | 10,000 |
| Stripes per frame | 20,000 |
| Markers per bucket | 64（由4提升） |
| Markers per aggregate lane | 64（由4提升） |
| Markers per data ref／frame | 維持10,000／20,000 |
| Direct marker glyphs per lane | 4（renderer budget，不是wire denial） |

同一 lane 的 marker order 沿用 document trace order，再依 bucket array order。Wire 5..64 筆不得拒絕或截斷；第65筆以 structured denial 拒絕完整 candidate。

### 5.4 Structured failures

Stripe codec／semantic validation 固定使用下列 reason codes：

- `overview-stripe-type-required`
- `overview-stripe-object-required`
- `unknown-overview-stripe-field`
- `invalid-overview-stripe-id`
- `duplicate-overview-stripe-id`
- `invalid-overview-stripe-timestamp`
- `invalid-overview-stripe-color`
- `invalid-overview-stripe-width`
- `invalid-overview-stripe-label`
- `limit-overview-stripe-tooltip`
- `limit-overview-stripe-bucket`
- `limit-overview-stripe-series`
- `limit-overview-stripe-frame`
- `overview-stripe-target-required`
- `overview-stripe-target-missing`
- `overview-stripe-target-kind`
- `overview-stripe-axis-mismatch`
- `overview-stripe-position-missing`
- `invalid-overview-stripe-collision-group`
- `invalid-overview-stripe-layer-order`

Marker overflow延續既有 `limit-marker-bucket`、`limit-marker-lane`、`limit-marker-series`、`limit-marker-frame`，但前兩者的 threshold改為64。

Malformed、duplicate、illegal option／hard limit回 `RuntimeEffect.RejectFrame(... Recoverable=false)`；sequence／base revision gap或缺 authoritative axis/data仍走 `RequestResync`。兩者都保留 last-good revision／data／DOM。

### 5.5 Navigator renderer

1. Prepared model先以 `EventTimeUtc -> canonical axis position -> visible slot -> X pixel`建立 bounded index。
2. Stripe path以 trace／lane／color batch，不為每筆事件建立全域 pointer handler。
3. Path使用1px CSS stroke、`vector-effect=non-scaling-stroke`等價行為及 `pointer-events:none`。
4. Navigator既有interaction surface在 hover／focus時查 prepared pixel bucket，顯示 count與bounded tooltip；drag handle與move hit region維持最高interaction priority。
5. Z order固定為 close path < stripe paths < selection mask／handles。

### 5.6 Marker cluster interaction

1. 前四筆 marker直接畫既有 glyph；其餘以單一 `+N` control放在下一個固定 overlay lane位置。
2. Cluster control可focus，`aria-label`包含row、time、anchor與remaining count。
3. `Enter`／`Space`開關 bounded detail list；`ArrowUp`／`ArrowDown`切換 active marker；`Escape`關閉並將focus留在cluster。
4. Detail依stable wire order逐筆呈現既有 label及完整 bounded tooltip；不可把多筆 tooltip串成一個超限 wire value。
5. Cluster只影響overlay，不能改Y-domain、time slot、candle data、shared cursor或全域mouse handlers。

### 5.7 Row-height state

Renderer-local state key：`CanvasInstanceId + RowId`。

```text
baseline: candle/composite = 250px, scalar = 112px
authored default = clamp(row minimum, row maximum,
                         round(baseline * HeightWeight))
candle/composite range = 180..720px
scalar range = 96..480px
```

Marker presence不得再把250px row切成310px特例。Plot geometry由resolved total height減固定header／axis／small padding計算；line/hist row不得保留無資料用途的上下band。Timestamp移到plot外的row overlay/header並做左右clamp。

Drag handle使用pointer capture；move期間每animation frame最多一次preview；pointer-up只commit一次local state。Handle使用`role=separator`、`aria-orientation=horizontal`，Arrow key每次8px、Shift+Arrow每次32px；Home／double-click／Reset移除override。Unmount／row刪除要釋放state與listeners。

### 5.8 Atomic RuntimePatch boundary

PTCS 不新增 scenario response type。既有 `RuntimePatch.Operations` 是 owner atomic boundary：

```fsharp
{ Operations =
    [| ReplaceDataRef(overviewStripeRef, overviewValue)
       ReplaceDataRef(orderMarkerRef, orderValue)
       ReplaceDataRef(fillMarkerRef, fillValue) |] }
```

`RuntimeReducer.patchCandidate` 必須先fold全部operations，再執行所有 document／temporal／marker／stripe semantic validation，最後才更新revision與publish。不得在operation fold中暴露 intermediate data。

Daedalus另行擁有：monotonic `selectionGeneration`、完整candidate identity、summary／trades／timeline／download manifest驗證及單一UI publish。PTCS scheduled renderer仍以 runtime revision／render generation拒絕stale prepared result，不假裝等同consumer selection generation。

### 5.9 Exact package graph

本RFC implementation release使用下列 immutable graph：

| Package | Final version | Exact dependencies |
| --- | --- | --- |
| `PulseTrade.Comm.Spa.Dynamic.Contracts` | `0.1.19` | `FSharp.Core [10.1.400]` |
| `PulseTrade.Comm.Spa.Dynamic.Renderer` | `0.1.45` | Contracts `[0.1.19]` |
| `PulseTrade.Comm.Spa.Dynamic.Interactive.Client` | `0.1.37` | Contracts `[0.1.19]`, Renderer `[0.1.45]` |
| `PulseTrade.Comm.Spa.Dynamic.Ptcs` | `0.1.41` | Contracts `[0.1.19]`, PTCS `[0.2.46]` |
| `PulseTrade.Comm.Spa.Dynamic.Ptcs.Client` | `0.1.59` | Contracts `[0.1.19]`, Renderer `[0.1.45]`, PTCS `[0.2.46]` |

原預定Contracts `0.1.16`／Renderer `0.1.42`在本機pack-before-full-build階段即淘汰；`0.1.17`暴露marker codec型別推斷錯誤，Renderer `0.1.43`亦未通過full WebSharper bundle gate。上述候選均未push；最終immutable graph使用表列版本。

Implementation correction：真SPAA揭露`0.1.44`只在full preparation／topology change更新navigator shell authority；same-topology `ReplaceDataRef(OverviewStripe)`只刷新row data，因此Signal／Fill stripe不呈現。`0.1.45`改用獨立shell prepared-data signal；same-topology patch只刷新navigator及既有row Vars，不重掛chart stack。對應Interactive／Ptcs client immutable closure為`0.1.37`／`0.1.59`。

Aggregate `PulseTrade.Comm.Spa.Dynamic 0.1.25`不依賴上述runtime packages，本輪不為湊齊「六包」無意義升版。若實作證明root bundle需要新contract，須先修訂本RFC package graph。

## 6. 取捨

1. 採獨立 `OverviewStripe` 而非擴充 marker：navigator line與K-bar glyph的geometry、hit testing及密度策略不同；共用會讓shape contract失真。
2. 採renderer-local row height而非server preference：先解交易操作需求，不提前引入帳號設定、同步衝突與migration。
3. 採64 wire／4 direct glyph：保留事件真相，同時bounded DOM；不由producer截斷資料換效能。
4. 採既有 multi-operation patch而非新transaction service：reducer已具candidate-before-commit seam，新增服務只會複製revision與recovery。
5. Scenario prepare／commit留consumer：只有TradeCore知道summary、trades、timeline、download identity是否完整一致。

## 7. 影響

- Contracts：新增trace kind、types、strict codecs、limits、candidate validation；marker bucket/lane threshold調整。
- Renderer：navigator stripe model/path/index、marker cluster、row resize、dynamic geometry及performance instrumentation。
- Interactive／PTCS clients：因DU與renderer bundle變更須exact dependency closure與full WebSharper rebuild；不新增業務UI。
- Daedalus consumer：升版後投影domain資料、實作scenario generation與complete candidate publish。
- 相容性：`sdui-runtime.v2`維持；legacy frame沒有OverviewStripe仍正常。Unknown trace kind繼續fail closed，不fallback成Line／Marker。

## 8. 驗收

1. Contracts strict codec、limits、candidate／last-good tests全部通過。
2. Navigator desktop/mobile驗single/multi trace、same-pixel lanes、same-trace count、exact X及drag不受阻。
3. Marker 5..64 accepted並出現可keyboard操作的`+N`；65 structured denial且last-good不變。
4. Candlestick/scalar resize、min/max、keyboard、reset、scenario retention、reload reset與listener cleanup通過。
5. 單一patch三個overlay refs全數commit；任一invalid時三者皆不變。
6. 4,000 slots＋bounded dense events，selection/render owner phase不得產生 >100ms browser long task；pointer move不掃全部events或呼叫server。
7. Planned exact packages push/readback、full WebSharper bundle及PTCS／Interactive demos通過。
8. Daedalus以同一packages完成真SPAA與fresh-kernel `.dib`；此consumer gate不阻擋owner package發布，但未完成不得宣稱整體產品驗收。

## 9. 關聯工項與測試

- WBS：`DYN-WBS-549..556`
- Tests：`DYN-T-570..578`
- Verification：`DYN-VFY-025`
- Consumer RFC：`RFC-TRADECORE-0028`
