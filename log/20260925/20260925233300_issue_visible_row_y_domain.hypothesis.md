# Visible-row Y-domain hypothesis

## 現象

所有composite K-bar row的可視價格被viewport外極值壓縮；row下方出現大量空白。row拖高後現行8%資料padding換算成更多CSS pixels。

## Repo state

- Branch：`20260915_033.ptcs_group_support`
- Baseline commit：`302b76f Add runtime projection commit contract`
- 開始時worktree clean。

## 核心假設

1. `prepareGeometry`的candlestick scale loop使用`preparedTraces`完整 candle series，而實際繪圖已使用current window的`candleSeries`；因此domain與visible geometry資料集不同。
2. 現有padding按資料range比例計算；SVG viewBox固定而CSS row height可變，造成實際pixel padding隨resize變動。

## 實驗與驗證

- 建立pure geometry test：source含遠端低價，visible window集中；期望domain不含遠端值。
- 同一visible values分別用default/resized CSS plot height計算；期望換算後上下pixel padding維持約15。
- 修正後跑Renderer exact-package suite與browser geometry gate。

## Pseudocode / size estimate

```text
visible candles = current projected candleSeries
visible lines = current projectedLinePoints
domain values = visible candle lows/highs + visible line values
padding in viewBox units = desiredCssPx * viewBoxHeight / actualCssPlotHeight
map values into [plotTop + paddingUnits, plotBottom - paddingUnits]
```

預估非型別宣告修改30至55行，測試35至60行。
