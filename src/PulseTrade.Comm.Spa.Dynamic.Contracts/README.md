# PulseTrade.Comm.Spa.Dynamic.Contracts

SDUI runtime/frame/action vocabulary、strict server codec、validation limits、pure reducer與poll lifecycle。方案 1 讓同一套 RuntimeFrame/reducer 產生 WebSharper browser metadata，並提供 loopback WebSocket 專用的 typed-JSON codec；package 仍不依賴 PTCS、fCell2、PTMD、SQL 或 provider SDK。

## Data flow

```text
host adapter -> RuntimeFrame -> RuntimeCodec/Validation -> RuntimeReducer -> Renderer model
local action -> reducer effect
remote action/poll/resync -> typed SduiAction -> host adapter
```

`RuntimeCodec` 的既有 System.Text.Json wire 與 `BrowserRuntimeCodec` 的 WebSharper typed-JSON wire 是兩條明確分離的 encoding；兩者共用相同 RuntimeFrame/RuntimeClientFrame 型別，但 JSON bytes 不可交叉解碼。

`Error`與invalid/gapped frame保留last-good document/data/view；duplicate frame no-op；sequence gap、identity mismatch或patch base mismatch只產生typed resync effect。

Browser-facing numeric使用JSON number/`float`，query range使用canonical ISO-8601 string。host/server必須重新驗證range並轉成domain `DateTimeOffset`；Contracts不把browser parser當authorization或domain validation。

## Security boundary

- protocol/case/row/action allowlist。
- frame bytes、rows、patch operations/items hard limits。
- shared contract拒絕script、selector與arbitrary URL key/value。
- contract只傳typed data/effect，不傳credential、SQL、DOM selector或transport URL。
