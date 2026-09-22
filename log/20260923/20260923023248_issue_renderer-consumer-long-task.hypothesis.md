# Renderer consumer long-task hypothesis

## 現象

- 真Chrome workload：3,819 true 1K points x 7 rows。
- initial max 893–914ms；48→All max 116–119ms；backtest/accounting max 135–168ms。
- pointer p95 <50ms；axis、crosshair、progressive merge與domain功能皆GREEN。

## 核心假設

1. `prepareDataScheduled`只在data entries間yield；單一shared axis或series仍同步parse 3,819 points，而單一row Doc／geometry仍一次建完，initial task因此可接近一秒。
2. RuntimeFrame／document replacement的client decode、reducer與Var publication可能把完整projection與Renderer反應串在同一browser task；0.1.37的row scheduling發生得太晚。

## 區分實驗

- 在owner BrowserDemo加入與consumer相同的phase markers及每階段PerformanceObserver evidence，分離frame receive/decode/reducer、prepare entry、row geometry、Doc mount與post-action replacement。
- 若單一row geometry >100ms，將row preparation再切為bounded trace／point chunks，先完成immutable prepared geometry，current generation完成後才mount Doc。
- 若client frame ingestion >100ms，增加generic queued frame scheduler，但authoritative reducer commit仍保持有序且不發布partial state。

## 預估

- 非型別宣告實作預估80–180行，測試／fixture預估120–240行。
- 若超過360行，須回到本檔說明為何不能以既有scheduler seam完成。

## Experiment

- 真consumer verifier已重現initial 879ms、48→All 117ms、backtest/accounting 176ms；workload與功能結果符合原報告。
- `RuntimeValidation.unsafeValue`會在安全資料上遞迴配置完整field path與error list；snapshot後續temporal validation又掃一次。這是initial receive/reducer path的確定重複工作，不依賴consumer domain。
- 第一個最小修補採兩階段安全檢查：allocation-light scan只回答是否存在unsafe value；僅unsafe subtree進精確collector。4,000-point安全資料與nested unsafe diagnostics均已由Contracts source suite鎖定。
- 48→All不經RuntimeFrame decode，仍有117ms，故Renderer geometry的中間陣列、Map projection與重複candle path掃描仍須獨立處理；不能把全部改善歸因於validation fast path。
- 真consumer host在CDP attribution前退出，因此目前不宣稱owner package已關閉consumer long-task gate。
