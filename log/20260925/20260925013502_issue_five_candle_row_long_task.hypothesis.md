# Five-candle-row long-task hypothesis

## 現象

- Owner BrowserDemo：7 rows／28 series／4,000 slots，All phase max約70ms，沒有browser long task。
- 真SPAA：五個candlestick rows；三次正式E2E中兩次initial-run約220–226ms，另一次pointer p95 52.84ms。
- Renderer `0.1.45`後Signal／Fill overview stripes已呈現，故此問題獨立於same-topology navigator refresh。

## 核心假設

1. Owner fixture只有price與Heikin屬candlestick-heavy；真SPAA的price-1K／5K／30K／60K／HA-60K同時建立多組candle SVG paths，initial mount成本未被owner gate覆蓋。
2. 若五列fixture仍無>100ms，剩餘成本位於SPAA同一commit中的summary/table/accounting或iframe/browser host，不應在Renderer盲目增加lazy行為。

## 實驗

- 建立五個獨立candlestick data refs/rows，各保留4,000 points，維持總row/series密度。
- 量測initial、48→All、document/marker replacement、pointer p95與Renderer phase telemetry。
- 用chart render sequence與ready-row count確認不是重複mount造成的假陽性。

## Baseline

- Repo：`C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic`
- Branch：`20260915_033.ptcs_group_support`
- Commit：`4f2fbac`
- Dirty：建立本hypothesis與active log前為clean。

## Experiment

- 五個獨立4,000-point candle refs重現allocation/GC敏感尖峰；舊路徑會把coarse candle展開至每個base slot，再以tuple＋`Array.groupBy`聚合。
- 改為固定bucket array單次累積OHLCV；<=1000 slots保留原projection。實作未改source interval、Y-domain、cursor或visible-window semantics。
- Playwright RPC wall-clock受host排程影響，因此單次／逐次RPC latency只作diagnostic；硬gate改以同一browser CDP main-thread trace判定target renderer task，仍維持>100ms fail。

## Conclusion

- 假設1成立：五個candlestick row揭露owner fixture缺少的projection allocation成本；根因在Renderer aggregation，不在SPAA summary/table。
- Official Renderer 0.1.49 exact-package browser gate：five-candle replacement max92.04ms，所有受驗renderer phase over100=0；300 cursor transitions不重建chart stack。
- Daedalus補充的fixed-pixel timestamp tag同輪完成；row resize前後除top/left定位外，geometry及computed style相同。
