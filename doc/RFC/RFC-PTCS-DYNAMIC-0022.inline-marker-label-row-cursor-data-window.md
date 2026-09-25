# RFC-PTCS-DYNAMIC-0022：可見交易 Marker Label 與 Row-local Cursor Data Window

- ID：RFC-PTCS-DYNAMIC-0022
- 狀態：Accepted / DEV Authorized
- 日期：2026-09-24
- Owner：Aster / PTCS Dynamic Renderer
- Consumers：Daedalus / TradeCore Backtest presentation、SPAA、ColdFar Notebook DIExt
- 關聯：`RFC-PTCS-DYNAMIC-0015`、`0016`、`0019`、`0020`、`0021`
- 上游 feedback：
  - `G:/coldfar_py/coldfar-symbolics/doc_new2/RFC/RFC-TRADECORE-0027.PTCS-InlineTradeMarkerLabel.Feedback.md`
  - `G:/coldfar_py/coldfar-symbolics/doc_new2/RFC/RFC-TRADECORE-0026.PTCS-CursorTimestamp.Feedback.md`

## 背景

Generic Marker contract 已有 bounded `TaMarker.Label` 與完整 `Tooltip`，但 Renderer 只把 Label 放進 SVG `<title>`。使用者只能看到三角形，無法直接判讀 BUY／SELL、成交價與出場 PnL。

同時 shared cursor 已能跨 row 對齊，但 row value band 只呈現 close／indicator value，vertical cursor 也沒有 row-local timestamp。若直接把 base row 時間複製到所有 row，5K／30K／60K 會顯示錯誤 bucket；若 timestamp 與 value 分開 resolve，則會產生 tearing。

## 目標

1. 將 producer 已提供的 `TaMarker.Label` 畫成 marker 附近的可見 SVG 文字。
2. 每個 row 的 vertical cursor 頂端顯示兩行 row-local 日期與時間。
3. 每個 row 的固定 data window 以前綴 timestamp 呈現該 row 的值；candlestick 顯示完整 OHLCV。
4. SPAA 與 DIB 由同一 Renderer 產生相同 DOM／SVG。
5. 維持 4,000 bars 的 bounded working set、既有 rAF cursor hot path及 Y-domain 不變。

## 非目標

1. PTCS 不解析 BUY／SELL／PnL，不新增 TradeCore order/fill domain 欄位。
2. 不由 Renderer 聚合 K bar、推算交易日或補 missing datapoint。
3. 不新增 server request、WebSocket action或另一套 SPAA overlay。
4. 不以 label 或 tooltip 值參與 Y autoscale。

## 使用情境

1. 多單進場顯示 `BUY <fill price>`；空單進場顯示 `SELL <fill price>`。
2. 多單出場顯示 `SELL <fill price> PnL <net pnl>`；空單出場顯示 `BUY <fill price> PnL <net pnl>`。
3. 同一 shared cursor 落在 1K 與 60K row 時，各 row 顯示自己實際命中的 completed datapoint 時間與值。
4. row 沒有可用 datapoint時，時間與值都顯示 `Unavailable`，不借用其他 row。

## 決策

### 1. Marker label contract 不變

Renderer 直接顯示 bounded `TaMarker.Label`。完整內容仍保留於既有 `<title>`；空白 label 不建立可見文字。producer 對成交語意與格式負責，Renderer 只負責 generic layout。

### 2. Label geometry

- label 與 marker 使用同一 `TaMarkerPlacement`、slot與anchor；marker shape仍使用既有aggregate lane，文字另以可視X區間配置collision lane。
- 預設在 marker 右側，右側空間不足改放左側；仍不足時 clamp 在 row SVG 內。
- 同anchor或不同anchor的相鄰label只要可視X區間重疊，就分配不同文字lane；垂直lane撞到row邊界時改往marker另一側展開，不把多筆文字clamp到同一Y。
- label字級依實際viewport調整，使1000-unit viewBox在desktop/mobile都維持可讀；使用white halo與`pointer-events:none`，不攔截marker hit target。
- 只變更 overlay；不增加 row 高度、不加入 scale samples、不裁到相鄰 row。

### 3. Row-local resolved presentation

每個 non-marker trace 的 cursor reader回傳同一個 immutable presentation：

```fsharp
type TaRowValuePresentation = {
    Timestamp: string
    Value: string
}
```

Candlestick reader 由同一個 resolved `TaCandlePoint` 產生 timestamp及 `O/H/L/C/V`；line／histogram 由同一個 resolved `TaSeriesPoint` 產生 timestamp及 value。row timestamp 選用該 row 第一個當下可用的 non-marker reader；若所有 reader 都沒有值，整列顯示 `Unavailable`。

### 4. 格式與 DOM contract

- cursor top：第一行 `yyyy-MM-dd`，第二行 `HH:mm:ss`。
- candlestick data window：`yyyy-MM-dd HH:mm:ss  <binding> O <o> H <h> L <l> C <c> V <v>`。
- line／histogram：`yyyy-MM-dd HH:mm:ss  <binding> <value>`。
- data window timestamp 固定 `19ch`，採 tabular numerals且維持既有 fixed height。
- DOM IDs：`ta-marker-label-*`、`ta-row-cursor-label-*`、`ta-row-cursor-date-*`、`ta-row-cursor-time-*`、`ta-row-data-time-*`。

### 5. Cursor hot path

prepare／refresh 時建立 indexed reader；pointer event只換算 bounded visible index，既有 single-rAF latest-wins callback在同一批次更新 crosshair、row cursor timestamp與row values。禁止 pointer-time decode、series scan、network action或 SVG topology rebuild。

## 失敗路徑

1. invalid timestamp：該 presentation 視為 unavailable，不以本機 locale 猜測。
2. missing row point：cursor label與data window prefix/value皆為 `Unavailable`。
3. oversize marker label：畫面使用 deterministic bounded text；`<title>`保留完整 bounded contract值。
4. stale generation：沿用 scheduled renderer generation gate，不得 commit 舊 reader／label topology。

## 影響

- Contracts：無 wire/public type變更。
- Renderer：row reader、SVG marker overlay、row data window與cursor label。
- Browser demo／Playwright：加入四種成交 marker、row-local timestamp、OHLCV與geometry/performance驗證。
- Consumer：只需升級 exact Renderer owner graph；Daedalus 不須修改 marker projection。

## 驗收

1. 四種交易 label 可見；shape／anchor／fill／color與tooltip不退化。
2. label與marker pan／zoom後仍黏同 slot；同slot及相鄰slot文字不互相遮蔽；edge fallback不溢出row。
3. 1K＋coarse row顯示各自實際 resolved timestamp；missing明示 unavailable。
4. candlestick data window固定順序顯示 timestamp＋OHLCV；line/histogram顯示timestamp＋value。
5. 300次 pointer sweep不送HTTP、不重建chart；p95 <125ms、max <400ms；owner phase無 >100ms task。
6. 4,000 bars bounded；desktop/mobile與SPAA/DIB exact-package DOM contract一致。

## Presentation Correction（2026-09-26）

`RFC-PTCS-DYNAMIC-0028`取代本RFC的inline plot marker label：plot只保留glyph與`<title>` tooltip，Label在shared cursor落點時投影到每列固定24px OFI band，密集事件以4筆＋`+N`呈現。row-local cursor timestamp與OHLCV／TA data window仍維持本RFC契約。
