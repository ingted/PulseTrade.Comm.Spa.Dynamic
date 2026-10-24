# Main TA real pointer pan hypothesis

## Symptom

`0.2.33` 的 pure delta 與 synthetic `data-main-plot-drag-delta=-420` gate 通過，但 Daedilus 實機以真 mouse down/move/up 無法拖動主 TA 圖。另缺少 row-local horizontal cursor line。

## Confirmed facts

- `mainPlotPanDelta = (startClientX - currentClientX) * visibleCount / trackWidth`；pointer 向左會得到正 delta。
- `panWindow` 以 `visible.StartIndex + delta` 送入 local window／adjacent coverage，因此方向符合需求。
- 主 plot SVG 綁 `pointerdown`；gesture 開始後監聽 `document` 的 `pointermove`／`pointerup`／`pointercancel`。
- down 階段沒有 `PreventDefault`、pointer capture 或 pointer-id filtering；SVG style 沒有 `touch-action:none`／`user-select:none`。
- draft 經 RAF 更新，可能在 gesture 中引起 reactive row update；現有 source test 不能證明真 browser pointer lifecycle。

## Expected invariant

pointer down 錨定資料 T。若 pointer 向左移 n 根後 up：

- resident `[T-1500,T+3000)` 變為 `[T-1500+n,T+3000+n)`；
- visible `[T-300,T+200)` 變為 `[T-300+n,T+200+n)`；
- 重繪後 pointer 所在 data time 仍為 T；
- gesture 只 commit 一次，drag 後 click 不觸發 point activation；
- 只有 pointer 所在 row 顯示水平 cursor，Y 由該 row SVG 的 client-to-viewBox geometry 決定。

## Root-cause candidates and discriminating evidence

1. Pointerdown 未進入 owner handler：down 後 `data-main-plot-drag-outcome` 不成為 `tracking`。查 `elementFromPoint`、disabled/readiness 與 event bubbling。
2. Owner handler 已進入但 browser default action／pointer ownership 中斷後續事件：down=`tracking`，move/up 未形成 `moving/committed`。以真 mouse event sequence、pointer capture／default-prevention 比較。
3. Gesture 計算已 commit，但 consumer authority/re-anchor 抵銷：outcome=`committed` 且 delta 正確，但 visible/resident start 未依 contract 移動。查 action、local/remote boundary 與重繪後 anchor。

目前候選 1–3 仍待 Daedilus exact RED 區分；禁止先以版本 bump、重啟或 synthetic attribute 取代真 pointer evidence。

## Evidence update

Daedilus 的真 DMI 5K mouse gate 證實 pointer lifecycle 正常：drag outcome=`committed`、requested delta=`+420`。resident 保持 `2994894+2100`，只有 local visible 從 0 變 420；因此最早 semantic 偏離點是 GLOBAL authority 下仍走 inner `setWindow`。同一物理 pointer X 重驗時 canonical anchor 從 22:34 漂至 22:29；原因是 pan delta 對 continuous pixel distance 四捨五入，而 cursor index 對各端 ratio 分別 floor，fractional slot 會相差一根。修正 contract 為 `delta = downSlot - currentSlot`，直接守住 `oldAbsoluteStart + downSlot = newAbsoluteStart + currentSlot`。

## Planned minimal experiment

在 owner BrowserDemo 對主 plot 實際執行 Playwright mouse down/move/up，同時記錄 hit target、drag outcome/delta、render sequence、visible/resident start 與 pointer 下 data time。先讓測試 RED，再只修最早偏離點。水平 cursor 以 row-local SVG overlay 驗 y geometry、row isolation、mouseleave hide 與無 chart rebuild。

## Estimated implementation size

預估非型別宣告程式碼 35–70 行，tests 40–90 行；若超過兩倍，先在本檔追加 experiment 原因。
