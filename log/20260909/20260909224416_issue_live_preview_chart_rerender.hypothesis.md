# Live preview chart rerender

## 現象

SPAA 真實 feed 的 current 1K close 已在 runtime frame 變動，但畫面最新 bar 不連續更新；cursor 約每秒被卡住一次。

## 核心假設

`TaWorkspaceRenderer.sameChartState` 比較 `DataRevision` 與 `LastTransportSequence`，所以每個同 position 的 current-K preview patch 都經 `View.Map2` 重建 chart stack。既有 cursor hot path只避免 cursor 本身觸發重建，沒有避免 live data patch 替換 DOM。

## 不變事項

- preview 的 close/high/low/volume 與依賴 TA 必須以 authoritative frame 更新。
- final/new temporal position、document、row、visibility與viewport變更仍須重建 topology。
- 不提前 ACK、不丟 revision、不降低 persistence 或 provider 正確性。
- 純 F#/WebSharper，不新增 JavaScript。

## 驗證

- 同 temporal position 的 preview replacement：值更新且 chart render sequence 不變。
- 新 temporal position/finalization：topology cache 失效並重建。
- 真實 SPAA API close gate與 F# Playwright cursor/live gate。

## Result

假設成立，但第一版只把SVG attributes改成dynamic仍不足：`View.MapCachedBy`的cached value會在上游每次emission後繼續進入下游`View.Map2`，chart依然重建。最終修正以兩個明確`Var`隔離topology/data emissions；generic gate已證visible close更新且render sequence不變。真SPAA browser closure由Daedalus以exact package執行中。
