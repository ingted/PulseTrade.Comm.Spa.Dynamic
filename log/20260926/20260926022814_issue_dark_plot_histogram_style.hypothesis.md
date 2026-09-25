# Hypothesis: edge-clipped boundary, plot palette, histogram polarity

## 現象

1. Overview visual boundary宣告2 CSS px，但初始selection貼SVG edge時外側一半被clip；drag離edge後正常。
2. SPAA需要所有TA／candle／overview plot surface採black presentation，且所有輔助元素可讀。
3. Histogram需要正值紅、負值綠等generic polarity style，目前單一`Color`不足；禁止series-name inference。

## 核心假設

1. visual line center位於viewBox 0/width造成half-stroke clip；只對visual x做CSS-pixel-aware inset可修復，不需改selection或drag target。
2. Renderer現有色彩為多處literal；應由單一resolved plot palette提供，不應由consumer CSS覆蓋。
3. `TaTraceSpec.Options`若已有typed extensibility，可新增histogram polarity option並保持legacy absent/default；若只是opaque map，須補最小typed codec helper而非新增平行style state。

## 驗證方式

- Source/DOM audit定位line center與SVG clip bounds。
- Contracts round-trip／legacy decode測試histogram style。
- Renderer unit與F# Playwright量computed 2px edge/drag、surface/readability及positive/negative bars。

## 預估實作

- 非型別宣告約80–160行；若超過320行，需回查是否theme抽象或browser verifier擴張失控。

## Experiment

- Source audit確認visual line中心位於viewBox `0/1000`，`vector-effect=non-scaling-stroke`使半個CSS stroke落在clip外。修正採只作用於boundary visual line的`translateX(+1px/-1px)`，而非改selection X或viewBox user-unit；原因是viewBox user-unit會隨實際SVG CSS寬度變化，不能穩定代表1 CSS px。
- `TaTraceSpec.Options`與`TaWorkspaceDocument.DefaultView`已有typed codec先例，無需新增wire record或consumer CSS seam。
- Contracts以`TaPlotSurfacePresentationCodec`與`TaHistogramTraceOptionsCodec`完成typed boundary；runtime validation拒絕未知theme、partial colors及wrong trace kind，legacy absent仍合法。
- Renderer用單一resolved palette貫穿plot／overview／axis／legend／cursor／tooltip；Histogram只依typed options分正負兩個batched paths，不做name inference。
- Release exact package graph及F# Playwright驗證支持三項核心假設；初始edge與drag後visible line皆2 CSS px，transparent hit rect維持8 units，未觀察selection regression。
