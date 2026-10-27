# Main-plot resident-boundary pan hypothesis

## 現象與最早偏離

0.2.55 Gate A 在 pointerdown 前為local `3150/1215`、active detail start `50090`、reference length4500。delta `+340` 要求的local start3490超過maximum3285；`tryMainPlotPanWindow`回None，`commitMainPlotPan`只清draft並回報outside captured formal-base，完全未進入既有adjacent coverage path。

## 不變量

- captured timeline內的CURSOR pan仍用exact event-time half-open range。
- 明確越過頭／尾的CURSOR pan使用既有typed adjacent coverage contract，保留目前visible count與實際signed delta。
- cross-axis resident frame的active ordinal基準是`projection.ActiveDetail.StartObservationOrdinal`；local start是resident buffer offset，不可再加到active start冒充absolute active ordinal。
- delta0、空timeline或非boundary invalid狀態不送action。

## 假設與最小實驗

1. 新增pure `mainPlotPanBoundaryDirection`，重現`3150/1215 +340 @4500 => Later`，within-range與delta0仍None。
2. `tryMainPlotPanCoverageIntent`在cross-axis projection以ActiveDetail start為current start；A shape應得到start50430、count1215、Later、query+1。
3. live `commitMainPlotPan`只在pure helper判定Earlier/Later時呼叫現有`requestAdjacentCoverage`；其他None維持fail closed。
4. Daedilus以exact package重跑A，核對ui action與accepted settlement。

## 預估修改

RendererModel約18行、Renderer live routing約8行、tests約45行；若實作超過52行，先追加Experiment說明。

