# 解決 PulseTrade.Comm.Spa.Dynamic 中 Canvas 未顯示與 Chat Session 空白的問題

## 摘要
修正了 DynamicRenderer.fs 的實作，使其能夠正確渲染與舊版 SPA 相同的 sdui-summary-card (FSkynet 動態畫布)。同時移除了會破壞版面的 ctor-dynamic-page 註冊，並釐清了 user 在測試時必須將訊息發送給 showcase-dynamic-actor 才會回傳 Canvas。

## 使用者需求
* 使用者反映在建立了 actor dynamic tab page 並對 durable-echo 發送訊息後，chat session 仍然沒東西。
* 使用者提供了舊版 SPA 成功顯示 Canvas 的截圖，要求恢復該功能。

## 分析與發現
1. **Chat Session 消失原因**：在先前的實作中，我們在 ActorDynamicTab.fs 註冊了 ctor-dynamic-page shape (PulseTrade.Comm.Spa.Client.RegisterAppendPageShape)，這導致 SPA 嘗試套用完全自訂的版面（缺少了預設的對話歷史與輸入框），因此使用者覺得 chat session 沒東西。
2. **Canvas 未顯示原因**：
   * 原本舊版 SPA 在 Client.fs 內部寫死了 createSduiSummaryCard。新的架構藉由 NuGet 擴充點機制將這部分抽離，但我們先前的 DynamicRenderer.fs 只是簡單印出 div 元素，並沒有還原舊版的 sdui-summary-card UI。
   * 使用者測試時輸入的 Actor Key 是 durable-echo，該 Actor 只會回傳純文字 "You sent: ..."，並非 skynet-sdui Schema，因此本來就不會觸發 Canvas 渲染。必須對 showcase-dynamic-actor 發送才會回傳 Canvas。

## 執行步驟
1. **修正 DynamicRenderer.fs**：將舊版 createSduiSummaryCard 的 HTML/CSS 結構 (包含「展開 Canvas」按鈕與樣式) 完整移植到 createSduiCanvas 中。
2. **修正 ActorDynamicTab.fs**：移除 RegisterAppendPageShape 的呼叫，讓 ctor-dynamic 或 ctor-argu 頁面能正常套用包含 Chat Input 的預設版面，僅保留 Renderer 註冊。
3. **清理 poc.dynamic.fsx**：移除了不必要的 SetActorArguTargetDurableAsync 呼叫。
4. **重新編譯**：執行 dotnet build 確保 WebSharper 將最新的 JS Bundle 打包進 wwwroot/js/。
5. **啟動測試**：啟動了 poc.dynamic.fsx 驗證其正常運作且印出正確的 Actor Path。

## 接下來的步驟
請使用者重新執行 poc.dynamic.fsx，並使用 Actor Argu 頁面發送訊息至 showcase-dynamic-actor 來驗證 Canvas。
