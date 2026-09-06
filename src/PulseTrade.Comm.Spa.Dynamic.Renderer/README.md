# PulseTrade.Comm.Spa.Dynamic.Renderer

基於`PulseTrade.Comm.Spa.Dynamic.Contracts`的pure WebSharper F# TA workspace renderer。

## Responsibility

- owns：DOM/SVG chart layout、bounded local viewport、row visibility、compact query/Add Row UI、typed action callback。
- does not own：PTCS、WebSocket、SQL、market-data provider、credential、ACL或authorization。
- local-only：pan、zoom、row toggle、Reset View。
- remote typed action：Change Query、Add Row、Remove Row、Reset Canvas；每個row以toggle + 固定寬度remove control呈現，remote command in-flight時remove同樣disabled。
- shared research view：同一visible index驅動各row crosshair/cursor values；transport timestamp壓成`MM-dd HH:mm`，OHLC detail可換行，時間標籤在SVG外以HTML grid呈現，避免mobile非等比縮放文字。
- status：保留freshness、watermark、quality與recoverable last-good error；remote in-flight只禁用remote submit，不凍結local view。
- query draft：只從`TaWorkspaceDocument.DefaultView`的`query.*` metadata初始化；document revision不變的poll不覆蓋使用者輸入，metadata缺失時保持空白，禁止回退到demo symbol/interval/date。
- Apply boundary：instrument/interval/range只更新local draft；選擇interval不送action、不改authoritative query、不重render，按`Load / Apply`才送一次`ChangeTaQuery`。
- loaded range：browser可保留2000 points；overview以bounded bucket呈現全range，左右handle可resize，中段可move，48/200/All可切換。drag只更新draft，pointer release/`change`才commit一次render，local navigation不送server action；All模式可把2000 points壓縮成bounded SVG primitives觀察長趨勢。
- Add/Edit Row：由`TaWorkspaceDocument.EditorSchemas`生成generic Text/Integer/Decimal/Boolean/Choice/Scale/List/Group表單；同template可建立多個參數實例。帶`ptcs.dynamic.editor.binding.v1`的row可預填並以stable RowId重新設定；legacy無binding row保持read-only。editor draft不會被poll覆蓋。
- Reset Canvas：送remote typed action恢復mount時initial ordered rows/query；Reset View只恢復local viewport。
- timeline：各trace依timestamp對齊reference timeline；SMA/ADX/MACD warm-up縮短不會用array index錯位或造成sequence failure。
- temporal projection：`TemporalPoint`明確指定source interval與projection；coarse K棒用`CandleSpan`跨base slots，coarse line用`RepeatAcrossBaseBuckets`，只在close後可知的indicator用`StepAfterClose`，避免look-ahead。
- multi-candle：同一row可同時畫1K/5K等多個candlestick traces；base candle維持實心，coarse candle以trace色outline/dashed wick呈現並保留source interval metadata。
- cursor/style：K棒、line point與cross-row cursor共用slot-center幾何；indicator line width為1.25，histogram維持1.0。

## API

```fsharp
TaWorkspaceRenderer.render
    TaWorkspaceRenderer.defaultOptions
    callbacks
    runtimeState
```

`runtimeState`是`Var<RuntimeState>`；host adapter負責strict frame decode/reducer與更新Var。正式runtime從current document讀editor catalog；`TaRendererOptions.EditorSchemas`只作standalone/test fallback。`callbacks.SubmitAction`接收含RequestId/revision的`DynamicActionRequest`並回`DynamicActionResult`；Add使用`ApplyTemplate(None, ...)`，Edit使用`ApplyTemplate(Some rowId, ...)`，不提供arbitrary URL、SQL或raw credential。

## Verification

- exact-package model/dependency/source tests：`tests/PulseTrade.Comm.Spa.Dynamic.Renderer.Tests`。
- exact-package live bundle：`tests/PulseTrade.Comm.Spa.Dynamic.Renderer.BrowserDemo`。
- desktop/mobile F# Playwright：`scripts/verify-ta-renderer-playwright.fsx`。
- current exact package：`PulseTrade.Comm.Spa.Dynamic.Renderer 0.1.0-alpha38`，exact依賴Contracts `[0.1.0-alpha16]`與FSharp.Core `[10.1.400]`。Current model gate 22/22；alpha38不改renderer/domain behavior。
