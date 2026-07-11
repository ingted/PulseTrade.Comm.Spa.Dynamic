# PulseTrade.Comm.Spa.Dynamic.Contracts

Transport-neutral SDUI runtime/frame/action vocabulary、strict codec、validation limits、pure reducer與poll lifecycle。package不依賴WebSharper、PTCS、fCell2、PTMD、SQL或provider SDK。

## Data flow

```text
host adapter -> RuntimeFrame -> RuntimeCodec/Validation -> RuntimeReducer -> Renderer model
local action -> reducer effect
remote action/poll/resync -> typed SduiAction -> host adapter
```

`Error`與invalid/gapped frame保留last-good document/data/view；duplicate frame no-op；sequence gap、identity mismatch或patch base mismatch只產生typed resync effect。

## Security boundary

- protocol/case/row/action allowlist。
- frame bytes、rows、patch operations/items hard limits。
- shared contract拒絕script、selector與arbitrary URL key/value。
- contract只傳typed data/effect，不傳credential、SQL、DOM selector或transport URL。
