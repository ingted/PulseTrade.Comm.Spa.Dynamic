# Issue hypothesis: coverage window intent lacks origin purpose

## 現象

- Gate B initial navigator resize count1500→79若以count分類可暫時成功；下一個navigator move的count仍為79且等於新的visible count，會被同一heuristic誤判main pan並搬動resident。
- main pan與cross-axis navigator event-range目前都由`tryMainPlotEventRangeIntent`建立相同no-ordinal／no-direction／ExplicitBounds intent。

## 已證實事實與不變量

- 真frame axes為GlobalOverview `research-overview-v1|499|1`、ActiveDetail `axis.30k`，故走cross-axis path。
- Consumer不得由ObservationCount推論gesture origin。
- main pan保留visible count並relocate together；navigator window只改fixed resident內active span；StartOrdinal/Direction overscroll仍是唯一materialization path。

## 最早偏離與單一假設

最早偏離是`TaCoverageWindowIntent`只有range authority、沒有producer purpose。新增required `TaCoverageWindowPurpose`，並在兩個producer boundary明確賦值，可讓consumer total match且不依runtime count。

## RED／GREEN 驗法

1. RED：Contracts／Renderer tests要求required Purpose及兩個origin值；0.2.56 surface缺欄位或source contract缺routing而失敗。
2. GREEN：Contracts wire roundtrip保留兩個case；main-pan helper/commit送`MainPlotPan`；navigator event-range送`NavigatorWindow`；overscroll StartOrdinal/Direction不改。
3. Regression：Renderer full、Interactive.Client direct test、fresh exact packages及package-only BrowserDemo。

## Pseudocode / estimate

```text
type TaCoverageWindowPurpose = MainPlotPan | NavigatorWindow
TaCoverageWindowIntent += Purpose
main plot event-range builder accepts MainPlotPan
navigator cross-axis event-range builder accepts NavigatorWindow
consumer total match supplied to Daedilus
```

預估非type宣告實作約10–20行，tests／version／docs另計；若超過40行，先回查是否重複建立builder或擴張到consumer owner。
