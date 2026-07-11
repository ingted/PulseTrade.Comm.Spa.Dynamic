# PulseTrade.Comm.Spa.Dynamic.Renderer

基於`PulseTrade.Comm.Spa.Dynamic.Contracts`的pure WebSharper F# TA workspace renderer。

## Responsibility

- owns：DOM/SVG chart layout、bounded local viewport、row visibility、compact query/Add Row UI、typed action callback。
- does not own：PTCS、WebSocket、SQL、market-data provider、credential、ACL或authorization。
- local-only：pan、zoom、row toggle、Reset View。
- remote typed action：Change Query、Add Row、Reset Canvas。
- shared research view：同一visible index驅動各row crosshair/cursor values；時間標籤在SVG外以HTML grid呈現，避免mobile非等比縮放文字。
- status：保留freshness、watermark、quality與recoverable last-good error；remote in-flight只禁用remote submit，不凍結local view。

## API

```fsharp
TaWorkspaceRenderer.render
    TaWorkspaceRenderer.defaultOptions
    callbacks
    runtimeState
```

`runtimeState`是`Var<RuntimeState>`；host adapter負責strict frame decode/reducer與更新Var。`callbacks.SubmitAction`只能接收`SduiAction`，不提供arbitrary URL、SQL或raw credential。

## Verification

- exact-package model/dependency/source tests：`tests/PulseTrade.Comm.Spa.Dynamic.Renderer.Tests`。
- exact-package live bundle：`tests/PulseTrade.Comm.Spa.Dynamic.Renderer.BrowserDemo`。
- desktop/mobile F# Playwright：`scripts/verify-ta-renderer-playwright.fsx`。
- current exact package：`PulseTrade.Comm.Spa.Dynamic.Renderer 0.1.0-alpha3`。
