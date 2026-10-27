# Returned2991 left-resize sequence hypothesis

## Facts / source of truth

- Source5c098e8（package sourcefe213fc），既有dirty如同名operations；所有產品/套件暫不改。
- full `formal-overview-edge-drag-0315-consumerfix` ui27請求694/gen25，accepted/1frame，但沒有該步最後DOM/frame，只有30s未達約700 oracle。
- focused `returned2991-left-resize694-r5`同UTC anchor與694 count，ACK coverage rev4/gen2，final797/694、selected-applied，1/1 PASS。
- 不變量：acknowledged event-range的prepared generation、時間錨點及window必須一致；其他歷史意圖不可覆蓋最後accepted intent。

## Hypotheses / smallest experiment

1. 前段global/query/rapid操作留下pending/draft或舊非同步commit，在最後ACK後覆蓋window；需要最後request→ACK→prepared→DOM的generation/window/feedback與drag owner欄位。
2. formal讀取對象/時點不是最後committed viewport；同一bounded trace可比較DOM selector與chart telemetry，不能只延長timeout或改expected。

- producer修碼估計：尚未定位，0行；先補證據。owner負責原序列必要prefix的一次instrumented run，失敗終點也必須落盤；不重跑全套無關尾段。
- 相關程式：Renderer.fs `settlePendingEventRangeProjectionFor`、`acceptPreparedDataCore`、`committedCoverageReady`；consumer `acknowledgeDurableCoverage`已在focused實證更新generation，單憑1 frame不是根因。
- 已讀focused source d32d16fc：217–219直接套2026 query，沒有原full的2018/global/rapid歷史，故兩者不是相同state history。維持H1/H2待一次prefix終點採樣，不改產品。

