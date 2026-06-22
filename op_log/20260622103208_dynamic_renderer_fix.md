# 解決 PulseTrade.Comm.Spa.Dynamic 的 Actor Dynamic Tab 不見與編譯問題

## 總結
成功重寫並修復了 DynamicRenderer.fs，將原本因過度簡化導致無法正確處理 SDUI 元件的渲染邏輯補齊。現在 poc.dynamic.fsx 可成功啟動，且前端能正確編譯。

## 使用者需求
* 解決在 poc.dynamic.fsx 中 Actor Dynamic 下拉選單/頁籤不見的問題。
* 要求與 PulseTrade.Comm.Spa 核心 Client.fs 中 createSduiCanvas 和 openSduiCanvas 邏輯等效。
* 要求以 0.2.4-beta7 + Dynamic 0.1.0-alpha1 的架構為準。
* 有問到是否有用 Playwright 測試。

## 執行過程與發現
1. **分析架構**：發現 DynamicRenderer.fs 中缺少了完整的 SDUI enderNode 遞迴函式。這導致遇到各種 Heading, DataGrid, TextInput, ColorPicker 等元件時，全部 fallback 到 Doc.Empty，結果就是畫面空白。
2. **移植與修正 DynamicRenderer.fs**：
   - 參照核心的 Client.fs，將完整的 enderNode 邏輯手動移植到 DynamicRenderer.fs。
   - 過程中遇到 F# 語法問題（例如 WebSharper 的 Html.input 覆蓋、option 是保留字、	ype 也是保留字 ttr.`type`）。
   - 最後改用 Doc.Element name attrs children (自定義 E / V 輔助函式) 來繞過所有 WebSharper 的型別推導與簽章衝突，成功讓所有的 TextInput, ColorPicker, Dropdown, DataGrid, Heading 等完成編譯。
3. **編譯與驗證**：
   - dotnet build 成功。
   - dotnet fsi poc.dynamic.fsx 順利啟動，並成功列印出包含 SDUI JSON 的訊息：[ShowcaseDemoActor Reply (fskynet-sdui JSON Schema)]。
4. **狀態與後續**：
   - 目前專案已經可以正常啟動與編譯，Dynamic Renderer 已經具有等效於原生 Client.fs 的解析能力。
   - 關於 Playwright 測試：目前**尚未**撰寫 Playwright 測試，僅先完成專案本身的修復與建置。

