# DYN-WBS-614 hypothesis — persistent navigator listener捕獲舊render authority

## 現象

Exact 0.3.1 真 Gate B 中，首次／middle gesture 能產生 NavigatorWindow intent；accepted rerender 後，在同一 navigator DOM 上的第二次 long gesture 已算出 target0 與正確 preview，但 action 不含 CoverageIntent，settle local1122。

## 已證實邊界

- ProductVersion 與 canonical bundle 對應 source `168de8b15f3b0d477cbec40ba0c582ada975d870`。
- Generated JS 的 pointerdown call 已傳 `currentCoverageProjection`，排除 0.3.0 的 handler re-decode 雙authority。
- 同一頁面 visual／attributes 可隨 reactive render 更新；wire 上第二次 action仍 bare，符合 persistent DOM listener保留舊 closure。
- Consumer、oracle、host、wire schema與2018 tail mapping不在本輪修改範圍。

## 根因與區分實驗

主要假設：WebSharper Doc diff 保留 navigator SVG 與原 event listener；該 callback 捕獲建立節點時的 `renderedState`／`document`／`currentPreparedData`／`currentCoverageProjection`。後續 View render只更新DOM內容，沒有替換 listener，所以第二次 pointerdown仍讀早期 `None` projection。

DYN-T-728 要求 production callback 在事件發生時才讀 stable authority `Var.Value`，且該 Var由與DOM render相同的 derived shell frame View原子更新。舊碼只直接傳 closure variables，必須 RED。

## 預計最小修復

在 `viewportControls document` 內建立一個 stable gesture authority `Var`，值為 `(RuntimeState * TaWorkspaceDocument * TaPreparedRendererData * TaLoadedCoverageProjection option)`；由 `shellPreparedFrame.View` 與 outer document 派生唯一 rendered-frame View，再以 `View.Sink` 更新 authority。render與listener共用此 View；listener invocation 先讀 `.Value`，再將該完整 tuple交給 `startNavigatorDrag`。

預估非型別宣告修改少於30行；若超過60行，先在本檔追加 `## Experiment` 說明才擴張。

## 不變事項

不改 wire contract、serializer、queue、consumer/oracle、main plot、GLOBAL ordinal、2018 tail mapping；不以隨機 latest state取代 renderer authority；保持 A／D／highlight／Tagging／原 C 與 middle NavigatorWindow 行為。

## Experiment

初版將derived frame與`View.Sink`放入`viewportControls document`，source contract可判綠但正式build以FS0039證明inner navigator DOM不在該local scope。若改成每個outer document branch各建sink，persistent listener可能保留舊branch Var，且多個subscription會競寫authority，反而無法守住唯一authority不變量。

因此最小根治調整為renderer instance級stable optional `Var`：同一個實際DOM mapper在計算`currentCoverageProjection`後立即發布`Some(renderedState, document, currentPreparedData, currentCoverageProjection)`；callback每次事件才match該Var。這保留「render與gesture同一完整frame」且不依賴listener被WebSharper替換。
