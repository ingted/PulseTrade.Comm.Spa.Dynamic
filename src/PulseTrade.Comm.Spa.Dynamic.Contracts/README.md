# PulseTrade.Comm.Spa.Dynamic.Contracts

SDUI runtime/frame/action vocabulary、strict server codec、validation limits、pure reducer與poll lifecycle。方案 1 讓同一套 RuntimeFrame/reducer 產生 WebSharper browser metadata，並提供 loopback WebSocket 專用的 typed-JSON codec；package 仍不依賴 PTCS、fCell2、PTMD、SQL 或 provider SDK。

## Data flow

```text
host adapter -> RuntimeFrame -> RuntimeCodec/Validation -> RuntimeReducer -> Renderer model
local action -> reducer effect
remote action/poll/resync -> typed SduiAction -> host adapter
```

`RuntimeCodec` 的既有 System.Text.Json wire 與 `BrowserRuntimeCodec` 的 WebSharper typed-JSON wire 是兩條明確分離的 encoding；兩者共用相同 RuntimeFrame/RuntimeClientFrame 型別，但 JSON bytes 不可交叉解碼。Interactive mutation 另用 `ptcs-dynamic-action.v1` 的 `DynamicActionClientFrame` / `DynamicActionServerFrame`，以 `RequestId`、`ExpectedDocumentRevision` 與 explicit result 建立 correlation；action result 不屬於 authoritative `RuntimePayload`。

Patch retention以整個ordered operation batch套用後的candidate data為驗證單位。替換既有key或先trim再append可維持在hard limit；多個operation合計超限時，整批patch不會進入authoritative state，reducer保留last-good data/revision並回`RequestResync`。

`SourceSnapshotEnvelope` / `SourceEventEnvelope` 是跨 domain 的 ordering seam。`SourceProjection`只驗stream identity、epoch、sequence、source revision並在gap/conflict/reducer reject時要求authoritative snapshot；payload reducer仍由domain owner adapter注入，package不擁有MDCQ、TradeCore、FsStl、SOR或FCell2型別。source revision不會直接提升browser `DocumentRevision`。

`DynamicTemplateSchema`以`EditorValueKind`描述Text/Integer/Decimal/Boolean/Choice/Scale/List/Group，不包含domain union或provider client。`DynamicEditorValidation`遞迴限制depth/fields/choices/list items，驗default value型別與safe payload；同template的不同參數實例以`TaRowSpec.RowId`區分，document validator拒絕重複RowId。

`TaWorkspaceDocument.EditorSchemas`是authoritative editor catalog，與Rows共用DocumentRevision。可重新設定的row以`TaRowEditorBinding.attach`把`TemplateKey + EditorInputValue[]`寫入`TaRowSpec.Options["ptcs.dynamic.editor.binding.v1"]`；`tryResolve`只在binding與catalog schema都有效時回傳editor state。legacy row沒有binding仍可顯示與移除，但不會被猜成可編輯。

`DynamicTemplateSchemaCodec.toValue/fromValue`提供transport-neutral `SduiValue` representation，供PTCS與Interactive adapter傳遞catalog；consumer須將invalid catalog視為整體錯誤，不可靜默刪除壞項後繼續。

`DynamicActionLifecycle`只管理單一pending request與correlated result。revision conflict不送出request；accepted/rejected只清除相符request並保存bounded feedback，均不直接修改authoritative document。Host完成resource prepare/swap後，必須以新的`RuntimeFrame`發布document revision。

`Error`與invalid/gapped frame保留last-good document/data/view；duplicate frame no-op；sequence gap、identity mismatch或patch base mismatch只產生typed resync effect。

`TaWorkspaceDocument.BaseRowId`指定shared event-time axis。`SharedCursorChanged`傳actual base datapoint timestamp；`VisibleRangeChanged`傳`[start,end)`與`MaximumBasePoints <= 4000`，兩者沿用correlated action lifecycle。

`TemporalAxis`/`TemporalSeries`是provider-neutral的shared temporal representation。axis point保存唯一`Position`及完整interval/frontier/finality/projection；scalar series只保存`Position + SduiValue`並exact-pin `AxisRevision`。Position只作join key，不依scale推算或補空K；current-K preview以相同Position和新的axis/series revision原位替換。`TaCandleDataRefs`把O/H/L/C/V五條scalar series組成candlestick，避免每個TA scalar重複28份時間metadata。既有`temporal-point.v1`仍可解碼。

Current exact package：`PulseTrade.Comm.Spa.Dynamic.Contracts 0.1.8`，exact依賴FSharp.Core `[10.1.400]`；current contract gate 22/22。`DataRef`代表immutable series identity，可由overlay row與separate rows重用；document內`RowId`仍須唯一，`TraceId`只須在所屬row內唯一。Typed與decoded frame均執行相同validation；single patch最多64 operations，另受500 items與16MiB frame限制。`RuntimeCache`只接受reducer已確認的bounded projection；OPEN_END projection只保存每條temporal axis的`Final` positions，所有temporal series依其axis position set同步裁切，coverage以裁切後最密的base axis計算，preview-only state不建立空cache。generic identity為owner提供的stable `OwnerFingerprint`與Dynamic schema revision。`RuntimeCacheEntryValidation`是WebSharper browser與server共用的完整entry語意gate；JSON合法但Document/Snapshot不一致的entry同樣fail closed。`RuntimeCacheProjection`另提供WebSharper-safe accepted-state write與rehydrate reducer；rehydrate只供display-first並固定為`PausedForResync`，cached data revision不成為authoritative delta continuation。SPAA的OwnerFingerprint必須由ProgramFingerprint、DataSourceFingerprint及cache semantic/schema version組成，不得含requested/cached time range；QueryFingerprint可含range但只供單次query診斷。range只進`RuntimeCacheCoverage`，且browser cache不得取代authoritative host state。

Browser-facing numeric使用JSON number/`float`，query range使用canonical ISO-8601 string。host/server必須重新驗證range並轉成domain `DateTimeOffset`；Contracts不把browser parser當authorization或domain validation。

## Security boundary

- protocol/case/row/action allowlist。
- frame bytes、rows、patch operations/items hard limits。
- shared contract拒絕script、selector與arbitrary URL key/value。
- contract只傳typed data/effect，不傳credential、SQL、DOM selector或transport URL。
