# RFC-PTCS-DYNAMIC-0007 Realtime TA Canvas Runtime

Status: Proposed / Review required
Date: 2026-07-11
REQ: `doc/TAResearch/REQ.md`
SA: `doc/TAResearch/SA.md`
DSL: `doc/SDUI_DSL_zh-Hant.md`

## 背景

Static Canvas目前以單一reply JSON建立summary card與overlay。新需求需要一份layout固定、data可持續更新的TA workspace。直接每5秒回完整`fskynet-sdui`會重建DOM、累積history、破壞zoom/toggle state，也會放大journal/IndexedDB與WebSocket負擔。

## 目標

1. 在同一Canvas instance上套snapshot/patch，而非重建document。
2. 支援TA row stack與研究型chart interaction。
3. 以authenticated PTCS WebSocket做5秒client-pull。
4. 保持Dynamic provider-neutral與pure WebSharper。
5. 保留static v1 Canvas相容性。

## 非目標

1. 不在browser計算provider backfill或直接讀SQL/PCSL。
2. 不承諾tick-by-tick低延遲；預設5秒poll。
3. 不把Plotly/其他JavaScript runtime加入bundle。
4. 不讓DSL執行arbitrary code、URL、DOM selector或SQL。
5. 不在本RFC實作PTMD storage或PTCS.Host actor。

## 方案比較

| Option | 優點 | 缺點 | Decision |
| --- | --- | --- | --- |
| 每5秒append完整Canvas reply | 可沿用現有renderer | history/DOM爆量、interaction state重置 | Reject |
| Dynamic直接開provider/SQL連線 | 快速 | security、ownership、package dependency錯誤 | Reject |
| Immutable document + typed patch runtime + PTCS WS channel | state清楚、bounded、可resync | 需新增runtime/transport seam | Accept |
| 直接嵌Plotly | interaction成熟 | JavaScript dependency與現行純WebSharper規則衝突 | Reject |

## 決策

### D1. Contract package拆分

新增輕量 `PulseTrade.Comm.Spa.Dynamic.Contracts` package，包含typed SDUI document/runtime envelope/patch codec；不得依賴WebSharper browser runtime或PTMD。`PulseTrade.Comm.Spa.Dynamic` renderer依賴Contracts；PTCS.Host只需reference Contracts，不需要compile-time reference整個Dynamic bundle。

### D2. Document與data revision分離

```text
documentId          stable layout identity
canvasInstanceId    one browser/runtime instance
documentRevision    changes only for explicit replace/new session
dataRevision        increments for snapshot/patch
transportSequence   detects duplicate/gap/out-of-order frames
```

一般poll只改`dataRevision`。`ResetCanvas`還原initial document-owned query/view defaults並要求snapshot；不產生新layout。

### D3. Runtime envelopes

Initial document：

```json
{
  "schema": "fskynet-sdui",
  "protocol": "sdui-runtime.v1",
  "kind": "document",
  "documentId": "ta-research-workspace",
  "canvasInstanceId": "ta:session:1",
  "documentRevision": 1,
  "dataRevision": 1,
  "surface": "Canvas",
  "ui": [{ "type": "TaWorkspace", "id": "ta-main", "rowsRef": "taRows" }],
  "data": { "taRows": [], "series": {}, "status": {} },
  "runtime": { "channel": "ptcs-extension-ws", "mode": "client-pull", "pollSeconds": 5 }
}
```

Patch：

```json
{
  "schema": "fskynet-sdui",
  "protocol": "sdui-runtime.v1",
  "kind": "patch",
  "canvasInstanceId": "ta:session:1",
  "documentId": "ta-research-workspace",
  "baseDataRevision": 7,
  "dataRevision": 8,
  "transportSequence": 19,
  "operations": [
    { "op": "upsert-series-points", "dataRef": "series.price", "keyField": "openTimeUtc", "items": [] },
    { "op": "set-status", "dataRef": "status", "value": { "stale": false } }
  ]
}
```

Allowed operations fixed為：

- `replace-data-ref`
- `upsert-series-points`
- `remove-series-before`
- `set-status`
- `set-options`

unknown operation必須controlled error + resync；不得best-effort執行。

### D4. TA node vocabulary

`TaWorkspace`是固定layout container；rows由`rowsRef`資料驅動：

```text
TaWorkspace
  TaToolbar       reset/add-row/poll status
  TaChartStack
    TaChartRow[]  rowId/scale/kind/range/dataRef/axis/height/visible
  TaLegend
  TaCrosshairGrid
  TaDataStatus
```

`kind`初始allowlist：`Candlestick`、`Volume`、`Sma`、`Dmi`、`Adx`、`Macd`、`HeikinAshi`。未知kind不得變成empty canvas。

### D5. View state與query state分離

- View state：zoom/pan/crosshair/toggle/row visibility/local mode；browser-owned，不送server。
- Query state：source/instrument/scale/range/indicator parameters/includePartial；remote action後由server validation結果更新。
- Reset View：只還原view defaults。
- Reset Canvas：還原initial query + rows + view並要求fresh snapshot。

### D6. Poll lifecycle

1. initial document mount後建立一個`CanvasRuntime`。
2. Canvas expanded且page visible才poll。
3. 預設/最小5秒；one in-flight；timeout後bounded exponential backoff。
4. patch sequence gap或base revision mismatch送`resync`。
5. unmount/close/socket close取消timer、in-flight request與runtime registry entry。
6. poll frame走transient Canvas channel，不append一般chat history；user-triggeredAdd Row/Reset可留audit event。

### D7. PTCS extension channel是必要upstream seam

Dynamic不得直接操作PTCS internal WebSocket variable。PTCS companion RFC需提供：

```text
registerRealtimeCanvasRenderer
mount context: pageId/key/caller/ACL/submit/openChannel/onFrame/onDispose
authenticated same-origin WS multiplexing
transient frame vs durable user-action projection policy
```

在該seam accepted前，DEV只能做pure codec/reducer/renderer tests，不能把HTTP polling或history spam當E2E acceptance。

### D8. Pure WebSharper implementation

TA runtime新module不得使用`JS.Inline`或手寫JavaScript。JSON解析用typed codec；timer/WebSocket/visibility/SVG interaction用WebSharper API；indicator math沿用`FSharp.Indicators`/PTMD Analytics結果。可參考e2eQuotation的viewport state/reducer與interaction需求，但不可複製其inline JavaScript helpers。

## Compatibility

- 沒有`protocol=sdui-runtime.v1`的既有`fskynet-sdui` payload繼續走static Canvas。
- Dynamic absent時PTCS顯示raw JSON/fallback。
- Runtime schema invalid時extension present表示controlled error，不回退成看似成功static Canvas。
- `documentRevision`變更只能由explicit new/replace document frame觸發。

## 影響範圍

| Module | Expected change |
| --- | --- |
| Dynamic.Contracts | typed document/runtime/patch/action DTO與strict codec。 |
| DynamicRenderer | static renderer改吃typed node；保留v1 compatibility。 |
| TaCanvasRuntime | instance registry、reducer、lifecycle、resync。 |
| TaCanvasRenderer | WebSharper SVG/controls/rows/crosshair/viewport。 |
| Extension | 註冊realtime renderer/channel capability。 |
| PTCS core | companion WS lifecycle/submit/transient projection seam。 |

## Acceptance

1. reducer property tests涵蓋duplicate/gap/out-of-order/resync/bounds。
2. pure WebSharper component tests涵蓋全部TA kinds與unknown-kind error。
3. Playwright在500+bars下操作zoom/pan/toggle/add-row/reset，不重建root canvas。
4. 20個5秒poll後message/history row count不增加，data revision正確前進。
5. hidden tab/closed Canvas/disconnect都停止poll並釋放runtime。

## 後續流程

Review accepted後先補PTCS companion RFC，再進 `SD -> WBS -> Test -> DEV`。本RFC不授權用HTTP polling、history append或JavaScript workaround。
