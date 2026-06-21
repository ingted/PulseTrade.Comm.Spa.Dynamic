# 20260622060227_create_demo_script.op_log.md

## 歷程與 StdIO
- [2026-06-22 06:02:27] 建立 `20260622060227_create_demo_script.log` 記錄計畫與預估時間 (30 分鐘)。
- 建立並撰寫 `poc.dynamic.fsx`。該腳本利用 `#r "nuget: PulseTrade.Comm.Spa, 0.2.4-beta7"` 作為基底伺服器框架，並載入我們剛打包完成的 `#r "src/bin/Release/net10.0/PulseTrade.Comm.Spa.Dynamic.dll"`。
- 在腳本的伺服器啟動流程中，注入了 `hub.useDynamicSdui(fabric.System)` 掛載點，這會動態將 `ShowcaseDemoActor` 註冊進系統中 (`"showcase-dynamic-actor"`)。
- 新增對該 Actor 送出 `init` 測試請求的邏輯 (`showcaseRef.Ask("init")`)，並將回傳結果印出。
- **測試結果**：
  ```
  [ShowcaseDemoActor Reply (fskynet-sdui JSON Schema)]:
  {"schema":"fskynet-sdui","ui":[{"component":"CanvasComponent","id":"demo-canvas","title":"PulseTrade Actor Dynamic Dashboard"},{"component":"GridFeatures","id":"demo-grid","theme":"dark"},{"component":"AppLoader","status":"loaded"},{"component":"ColorPicker","default":"#ff0000"}]}
  ```
  伺服器能成功載入並執行上述流程無誤 (`dotnet fsi poc.dynamic.fsx --no-wait` 返回退出碼 0)。

## 自我審查 (Review)
- **相容性與隔離性**：這份 POC 成功展示了不用修改上游程式碼也能透過 `.dll` 擴充 `CommHub` 邏輯的彈性。只要日後主專案將這個 POC 整合進 UI 註冊流程，就可以直接看見結果。
- **時間控制**：本次實作耗時約 **15 分鐘**。

## 下一步 (Next Step)
POC 展示用腳本已備妥。專案可被打包成 NuGet 或提供此腳本做為功能展演使用。
