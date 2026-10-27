# Issue hypothesis: cross-axis navigator event-time materialization

## 現象

0.2.50 real-data 流程 `historical4500 -> resize79 -> middle localStart1201 -> drag left`：preview 為 `1-79`、action `StartEventTimeUtc=2022-03-11T10:00:00Z`，但 settle 後 ActiveDetail 僅 `49646 -> 49567`（-79），而非落到 action 指定時間。

## 已證實事實與不變量

- cross-axis pointer 到 validated overview event-time 的映射正確；不是 ordinal mapping defect。
- `setNavigatorWindowFromPrepared` 固定以 `CoverageIntent=None` 呼叫 `setWindowFromPreparedRange`。
- 0.2.51 已能以 action event-time reanchor returned projection，並拒絕 stale/missing/terminal-missing settlement。
- same-axis navigator 既有 resident intent、preview 與 wire contract 不得改變。

## 根因候選

唯一有證據的候選：cross-axis exact-timeline navigator action 未宣告 materialization intent，consumer 無 exact-time fetch contract，只能採既有 visible-page shift。最早偏離點是 Renderer 建 action 時傳入 `None`，不是 Daedilus pointer geometry 或 event-time lookup。

## 最小實驗與設計

1. 先以 DYN-T-717 證明 cross-axis validated overview timeline 會產生 `ExplicitBounds`、`StartObservationOrdinal=None`、`Direction=None`、當前 revision、下一 generation 與 exact observation count；same-axis/mismatched timeline 必須回 `None`。
2. `setNavigatorWindowFromPrepared` 僅在採用 cross-axis validated overview timeline 時把該 intent 傳給 `setWindowFromPreparedRange`。
3. 將 Renderer local pending state 名稱泛化為 event-range settlement，讓 0.2.51 同一個 UTC reanchor invariant 同時涵蓋 main-plot pan 與 cross-axis navigator materialization；不改公開 action shape。

## 預估實作量

非型別宣告約 25 行；若超過 50 行，須在本檔追加 `## Experiment` 說明原因。

## 驗證切點

- RED：0.2.51 package 不存在新的 cross-axis intent helper／wiring。
- GREEN：DYN-T-717、Renderer full、Interactive full、package verifier、BrowserDemo。
- Deployment E2E：Daedilus 18884 以原 left-edge sequence 驗 Action event time、settled ActiveDetail absolute relocation、selection ratio 與 console。
