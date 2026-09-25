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

大型Snapshot可用`RuntimeSnapshotTransportCodec.encodeFrames`展開為bounded `start / item / commit` packets；非Snapshot仍輸出單一legacy frame。packet只負責transport framing，完整batch仍須通過count、順序、batch id、generation與canonical reducer後才成為runtime truth；partial或invalid batch不得發布、寫cache或觸發accepted lifecycle。

純.NET、WebSharper與machine E2E應直接以owner API消費完整ordered wire stream，不可自行辨識或切割chunk batches：

```fsharp
match RuntimeSnapshotTransportAssembler.decodeFrames transportGeneration wireMessages with
| Ok frames -> frames |> Array.iter consumeCanonicalFrame
| Error issue ->
    printfn "snapshot wire rejected code=%s packet=%d message=%s" issue.Code issue.PacketIndex issue.Message
```

`decodeFrames`保序接受legacy `RuntimeFrame`與零或多個完整chunk batches，並拒絕partial、orphan、duplicate、out-of-order、interleaved、trailing、錯誤schema/kind及invalid value。`PacketIndex`是整條wire stream的global zero-based index。需要逐packet接收時使用`create/createAt`、`decodePacket`、`acceptPacket/acceptEncoded`與`finish`；同一batch的每次呼叫必須傳入相同transport generation，generation不符不得接續舊batch。`acceptPacket/acceptEncoded`只完成framing與item-local validation，`CompletedFrame`是尚待canonical validation的candidate；machine consumer必須呼叫`finish`，browser則把candidate交給既有phased reducer，避免commit task同步重複掃描整個snapshot。

Current local candidate：`PulseTrade.Comm.Spa.Dynamic.Contracts 0.1.28`，exact依賴FSharp.Core `[10.1.400]`。`RuntimeProjectionCommitReceiptV1`以document/canvas identity、document/data revision、transport sequence與projection sequence描述browser已完成的projection；`RuntimeProjectionCommit.create/validate/satisfies`供renderer與consumer共用，不取代action ACK或authoritative reducer acceptance。合法大型RuntimeFrame的unsafe validation先走allocation-light scan，只有發現unsafe subtree才建立精確diagnostic path；temporal/schema validation與fail-closed語意不變。current marker encoder只輸出strict/bounded `ta-marker.v2`；public shape為`TriangleUp | TriangleDown | Circle | Square | Diamond`，方向不由`AboveBar／BelowBar`推導。decoder可讀legacy v1，v2 unknown shape fail closed。marker browser cache current schema為3；schema 2一律miss/resync。

`RuntimeCache`只接受reducer已確認的bounded projection；OPEN_END projection只保存每條temporal axis的`Final` positions，所有temporal series依其axis position set同步裁切。rehydrate只供display-first並固定為`PausedForResync`，cached revision不可作authoritative delta continuation。`DataRef`是immutable series identity；document內`RowId`唯一，`TraceId`只須在所屬row內唯一。Typed與decoded frame使用同一validation；single patch最多64 operations，另受500 items與16MiB frame限制。

## Marker v2 typed authoring

Producer只建立typed marker與runtime frame，不自行畫SVG：

```fsharp
let entryMarker : TaMarker =
    { MarkerId = "long-entry-0001"
      EventTimeUtc = "2026-09-21T01:10:30Z"
      Anchor = TaMarkerAnchor.BelowBar
      Shape = TaMarkerShape.TriangleUp
      Fill = TaMarkerFill.Outline
      Color = "#000000"
      Label = Some "Long entry"
      Tooltip = [||] }

let markerSeriesValue = TaMarkerCodec.encodeBucket [| entryMarker |]

let snapshotFrame : RuntimeFrame =
    { Protocol = DynamicRuntimeDefaults.markerProtocol
      Kind = RuntimeFrameKind.Snapshot
      DocumentId = documentId
      CanvasInstanceId = canvasInstanceId
      DocumentRevision = 1L
      BaseDataRevision = None
      DataRevision = 1L
      TransportSequence = 2L
      Payload = RuntimePayload.Snapshot { Data = data; Freshness = TaFreshness.Live } }
```

Marker trace須以`TaMarkerTraceOptionsCodec.encode`指定同row的candlestick `TargetTraceId`。Entry時間由consumer使用strategy signal time；exit使用actual simulated fill time。Contracts不解析long／short、entry／exit、PnL或exit reason。

`TaTraceKind.OverviewStripe`是navigator事件的domain-neutral wire。`TaOverviewStripe`只攜stable id、UTC event time、color、CSS pixel width、optional label與bounded tooltip；trace options指定同row candlestick target、collision group及layer order。Stripe的event time必須對上canonical temporal axis position，且patch candidate會連同marker／candle refs整批驗證。marker wire bucket上限為64；四筆direct glyph及`+N`是Renderer presentation budget，不會截斷contract truth。

Browser-facing numeric使用JSON number/`float`，query range使用canonical ISO-8601 string。host/server必須重新驗證range並轉成domain `DateTimeOffset`；Contracts不把browser parser當authorization或domain validation。

## Security boundary

- protocol/case/row/action allowlist。
- frame bytes、rows、patch operations/items hard limits。
- shared contract拒絕script、selector與arbitrary URL key/value。
- contract只傳typed data/effect，不傳credential、SQL、DOM selector或transport URL。
