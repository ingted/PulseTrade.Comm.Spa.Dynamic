# Issue: SPAA consumer scheduler jitter

## Observation

- owner BrowserDemo同package graph的4,000-bar 200→All為435.30ms。
- 真SPAA 960 bars／8 rows會間歇性無法在10秒內完成48 preset，或200→All為2240ms。
- 真consumer已使用official immutable packages，故不是stale package identity。

## Hypotheses

1. zero-delay cooperative row mount雖移除固定frame延遲，但與WebSharper reactive lifecycle交錯時，舊
   generation callback可能佔用或覆蓋新viewport row barrier，造成ready/render completion jitter。
2. consumer在viewport action後仍有相鄰frame/cache projection工作與Renderer mount競爭main thread；現行
   owner fixture未同時施加該lifecycle負載。

## Experiment

1. 原樣連續執行Daedalus `Rfc0026.Spaa.Backtest.playwright.fsx`，保存48 diagnostic、render sequence、
   ready-row count與200→All latency。
2. 在不改public contract下加入generation/phase timing evidence，確認stall位於prepare、row mount或DOM settle。
3. 先寫可重現owner regression，再作最小scheduler修正，最後跑owner與真consumer連續gates。

## Stop Condition

不得提高timeout、放寬2秒gate、跳過rows或減少資料來換綠燈；若瓶頸在consumer-owned pre-render，
只交付bounded evidence，不修改consumer source。

## Finding

- 同一真consumer 960 bars／8 rows以direct test-id觀測時，48／200 click分別119ms／318ms，
  200→All為1315ms，其中All click本身144ms；Chrome在2,444 bars量到interaction INP 17ms。
- formal gate原本用`GetByText(Regex("Loaded ..."))`，其輪詢會在大型DOM反覆執行全文文字定位，
  把verifier觀測成本算入2秒產品門檻；先前2153／2240ms不能單獨證明Renderer stall。
- generation overlap仍是錯誤使用方式，但同步48→200 barrier後未重現owner scheduler stall。

## Disposition

Renderer public contract與package graph維持不變。consumer gate應保留2秒門檻，改以既有
`data-testid=ta-viewport-range`觀測，並在initial state明確等待非零Loaded；由consumer owner連續重跑後
再關閉此issue。
