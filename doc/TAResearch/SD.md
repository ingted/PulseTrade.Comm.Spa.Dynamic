# SD-PTCS-DYNAMIC-TA-0001 Transport-Neutral Realtime TA Canvas Runtime

Status: Accepted / Ready for DEV
Date: 2026-07-11
REQ: `doc/TAResearch/REQ.md`
RFC: `doc/RFC/RFC-PTCS-DYNAMIC-0007.realtime-ta-canvas-runtime.md`
Current change: `doc/RFC/RFC-PTCS-DYNAMIC-0011.ta-export-draft-cursor-defaults.md`
SA: `doc/TAResearch/SA.md`
Test: `doc/TAResearch/Test.md`
WBS: `doc/TAResearch/WBS.md`

## 1. Compile/package design

```text
PulseTrade.Comm.Spa.Dynamic.Contracts.fsproj
  RuntimeTypes.fs
  RuntimeCodec.fs
  RuntimeValidation.fs
  RuntimeReducer.fs

PulseTrade.Comm.Spa.Dynamic.Renderer.fsproj
  TaViewport.fs
  TaCanvasRenderer.fs
  DynamicSduiRenderer.fs

PulseTrade.Comm.Spa.Dynamic.fsproj
  existing static/Argu integration
  PtcsDynamicAdapter.fs
  Extension.fs compatibility facade
```

Contracts同時擁有transport vocabulary與pure reducer/registry/poll state machine，讓PTCS與E2EQ得到相同state transition。Contracts只允許WebSharper metadata與typed browser codec；server仍使用System.Text.Json codec，兩種wire bytes不可混用。Contracts不得reference PTCS/fCell2/PTMD/MDCQ/TradeCore/FsStl/SQL。Renderer不得referencePTCS/fCell2/PTMD/SQL。facade以exact NuGet package reference消費Contracts/Renderer，禁止ProjectReference作release-facing integration。

## 2. Runtime types

```fsharp
type CanvasInstanceId = CanvasInstanceId of string
type DocumentId = DocumentId of string

type TaFreshness =
    | Live
    | Delayed of lag: TimeSpan
    | Stale of lag: TimeSpan * reasonCode: string
    | Backfill of reasonCode: string
    | Unavailable of reasonCode: string

type TaRowKind =
    | Candlestick
    | Volume
    | Sma
    | Dmi
    | Adx
    | Macd
    | HeikinAshi

type TaRowSpec =
    { RowId: string
      Kind: TaRowKind
      DataRef: string
      HeightWeight: float
      Visible: bool
      Options: Map<string, SduiValue> }

type RuntimeFrameKind =
    | Document
    | Snapshot
    | Patch
    | Error
    | Heartbeat

type RuntimeFrame =
    { Protocol: string
      Kind: RuntimeFrameKind
      DocumentId: DocumentId
      CanvasInstanceId: CanvasInstanceId
      DocumentRevision: int64
      BaseDataRevision: int64 option
      DataRevision: int64
      TransportSequence: int64
      Payload: RuntimePayload }
```

Codec只接受`protocol = "sdui-runtime.v1"`與allowlisted DU cases。unknown required field/operation/node回typed validation error；不得dynamic invoke。

## 3. Patch operations

```fsharp
type PatchOperation =
    | ReplaceDataRef of dataRef: string * value: SduiValue
    | UpsertSeriesPoints of dataRef: string * keyField: string * items: SduiObject list
    | RemoveSeriesBefore of dataRef: string * key: SduiValue
    | SetStatus of dataRef: string * value: SduiObject
    | SetOptions of targetId: string * value: SduiObject
```

`dataRef`與`targetId`必須存在document registry。`keyField`由document node/series spec allowlist決定，client frame不可任意改成DOM/JSON path。單frame operations/items/bytes皆受limit。

## 4. Reducer

```fsharp
type RuntimeEffect =
    | NoEffect
    | RequestResync of CanvasInstanceId * lastDataRevision: int64
    | SubmitAction of SduiAction
    | SchedulePoll of TimeSpan
    | CancelPoll
    | ReportDiagnostic of DynamicDiagnostic

let reduce state frame =
    if frame.CanvasInstanceId <> state.Identity.CanvasInstanceId then
        state, RequestResync(state.Identity.CanvasInstanceId, state.DataRevision)
    elif frame.TransportSequence <= state.LastTransportSequence then
        state, NoEffect
    elif frame.TransportSequence <> state.LastTransportSequence + 1L then
        { state with Poll = PausedForResync }, RequestResync(frame.CanvasInstanceId, state.DataRevision)
    elif frame.BaseDataRevision <> Some state.DataRevision && frame.Kind = Patch then
        { state with Poll = PausedForResync }, RequestResync(frame.CanvasInstanceId, state.DataRevision)
    else
        applyValidatedFrame state frame
```

`Error`更新status/diagnostic並保留last-good document/data/view。只有初始document invalid時mount失敗。`Snapshot`在resync後替換bounded server data；除非frame明示`ResetView`，合法local view不被覆蓋。

## 5. Generic host callbacks

```fsharp
type IDynamicFrameChannel =
    abstract member Send: RuntimeClientFrame -> Async<Result<unit, DynamicHostError>>
    abstract member Frames: IObservable<RuntimeFrame>
    abstract member State: IObservable<DynamicChannelState>
    abstract member Dispose: unit -> unit

type DynamicHostCallbacks =
    { SubmitAction: SduiAction -> Async<Result<unit, DynamicHostError>>
      OpenTransientChannel: CanvasInstanceId -> IDynamicFrameChannel
      IsSurfaceVisible: unit -> bool
      IsSurfaceExpanded: CanvasInstanceId -> bool
      Schedule: TimeSpan -> (unit -> unit) -> IDisposable
      ReportDiagnostic: DynamicDiagnostic -> unit }

module DynamicSduiRenderer =
    val install:
        DynamicRendererOptions -> DynamicHostCallbacks -> DynamicRendererRegistration
```

Callbacks不得提供raw credential、SQL query或arbitrary URL。Renderer只能送typed `SduiAction`/`RuntimeClientFrame`。

## 6. Host adapters

### 6.1 PTCS

```text
PTCS authenticated WebSocket session
  -> caller/ACL/capability resolution
  -> PtcsDynamicAdapter.DynamicHostCallbacks
  -> shared Renderer

shared action/poll effect
  -> PTCS selected target command / transient channel
  -> PTCS.Host actor
  -> RuntimeFrame
```

- user query/reset/add-row可依server policy產生audit，但不必是chat message。
- heartbeat/poll/snapshot/patch不append chat journal、PCSL message stream或IndexedDB message row。
- PTCS adapter不得以new HttpClient或global socket繞過core seam。

### 6.2 E2EQ

```text
E2EQ backend reply/delta
  -> E2EqDynamicFrameMapper
  -> shared Renderer

shared SduiAction
  -> E2EqDynamicActionMapper
  -> existing E2EQ query/subscription command
```

E2EQ adapter可使用自己的WebSocket/HTTP orchestration，但transport只存在adapter。它不得修改shared frame語意來容納provider-specific object。

## 7. TA DSL/data model

Canonical document fragment：

```json
{
  "protocol": "sdui-runtime.v1",
  "type": "TaWorkspace",
  "id": "ta-main",
  "rowsRef": "ta.rows",
  "statusRef": "ta.status",
  "sharedTimeAxis": true,
  "actions": ["reset-view", "reset-canvas", "add-row", "change-query"]
}
```

Row data：

```json
{
  "rowId": "price",
  "kind": "Candlestick",
  "dataRef": "series.price",
  "heightWeight": 3,
  "visible": true,
  "options": { "showVolumeOverlay": false }
}
```

status data至少含backend、coverage、source watermark、observed-at、lag、freshness、partial/sealed、quality、data revision。DSL不含SQL/provider URL。

## 8. Actions

```fsharp
type TaQueryChange =
    { SourceId: string option
      Instrument: string option
      IntervalMinutes: int option
      FromUtc: string option
      ToUtcExclusive: string option
      IncludePartial: bool option }

type SduiAction =
    | ResetView of CanvasInstanceId
    | ResetCanvas of CanvasInstanceId
    | AddTaRow of CanvasInstanceId * TaRowSpec
    | RemoveTaRow of CanvasInstanceId * rowId: string
    | ChangeTaQuery of CanvasInstanceId * TaQueryChange
    | PollDelta of CanvasInstanceId * afterDataRevision: int64
    | RequestFullSnapshot of CanvasInstanceId * reasonCode: string
```

`TaQueryChange.FromUtc/ToUtcExclusive`是browser-safe canonical ISO-8601 string，不是已驗證domain time。Renderer不自行解析成`DateTimeOffset`；PTCS/E2EQ adapter與server必須驗證格式、UTC/offset、range順序與最大範圍後才建立provider query。

`ResetView`由reducer本地處理，不呼叫host；其他remote cases經callback。server仍需驗證interval/range/row kind/ACL，client allowlist不等於authorization。

## 2026-07-14 canonical viewport and pointer cursor

```fsharp
type TaRendererUiState =
    { Window: TaVisibleWindow
      FollowLatest: bool
      CursorIndex: int option
      HiddenRows: Set<string>
      AddRowOpen: bool
      Feedback: string }

resolveWindow total state =
    let bounded = clampWindow limits total state.Window
    if state.FollowLatest then tail total bounded.Count else bounded

pointerIndex rect clientX visibleCount =
    clamp 0 (visibleCount - 1)
        (round ((clientX - rect.left) / rect.width * (visibleCount - 1)))
```

Renderer輸出stable hooks：

- `ta-viewport-panel`、`ta-viewport-slider`、`ta-viewport-range`；attributes含`data-loaded-bars`、`data-visible-start`、`data-visible-end`、`data-follow-latest`。
- navigator必須位於cursor panel之後、rows之前；inline展開後不需先捲過四列圖即可看到`Loaded N bars · Viewing A-B`與range track。rows之後不得再藏一份重複navigator。
- 每個row SVG使用typed WebSharper `afterRender/AddEventListener`綁pointer/mouse move；不得用`JS.Inline`或string-built JavaScript。
- `ta-chart-stack`持有唯一shared cursor state；row crosshair以同一normalized X render。X axis只在最後一個visible row後render一次。
- viewport change清除或重新clamp cursor；delta只在follow-latest時移動window。Collapsed不建立上述DOM/event handler。

Summary將`lastBars`寫成`requested last N bars`；actual coverage只由展開後reduced snapshot的series count/readout宣告。

## 9. Poll state machine

```text
Unmounted
  -> MountedIdle
  -> Ready
  -> PollInFlight
  -> Ready

PollInFlight -- timeout --> Backoff
Ready -- hidden/collapsed/disconnected --> Suspended
any -- revision gap --> PausedForResync
any -- unmount --> Disposed
```

default/minimum poll interval為5秒。每canvas一個in-flight；backoff capped。`Disposed`後channel/timer/subscription不得觸發callback。

## 10. Limits

| Limit | Default | Hard behavior |
| --- | ---: | --- |
| rows per canvas | 8 | reject action/frame, keep last good |
| initial bars per series | 5000 | reject oversized snapshot |
| retained bars per series | 2000 | server/client typed remove-before policy |
| patch operations | 32 | reject frame + resync |
| patch items | 500 | reject frame + resync |
| frame bytes | 2 MiB | channel rejects before decode |
| poll interval | 5 sec minimum | clamp/reject invalid config |
| in-flight polls | 1 | coalesce/no-op |

Server可下調限制，不能由payload放寬hard maximum。

## 11. WebSharper implementation constraints

1. JSON經typed codec；不使用dynamic object與`JS.Inline`。
2. SVG/chart/controls以WebSharper nodes/Vars/View建立。
3. visibility、timer與pointer/keyboard interaction使用WebSharper/browser typed APIs。
4. stable container dimensions與shared x-domain避免patch造成layout shift。
5. renderer root只在document revision變更時replace；snapshot/patch更新reactive models。

## 12. Compatibility/migration

1. static payload無runtime protocol時走existing renderer。
2. current `CommHub.useDynamicSdui`簽名先保留，內部建立PTCS callbacks並install shared renderer。
3. E2EQ先以parallel feature gate mount shared renderer；parity後才替換canonical TA path。
4. global `doc/SDUI_DSL_zh-Hant.md`在DEV前同步runtime/TA vocabulary與versioning；本SD是review source，不宣稱DSL已實作。

## 13. Diagnostics

Diagnostic只含document/instance/revision/sequence/reason code/limit name，不含payload全文、token、credential或connection string。UI error需可恢復，並保留last-good Canvas與FormInput/host surface。

## 14. Verification design

完整矩陣見`doc/TAResearch/Test.md`。UI每個milestone都需Playwright操作PTCS與E2EQ兩條host path；不能只做DOM static assertion或build。

## 2026-07-11 PTCS transient server adapter alpha2

```text
PTCS ClientExtensionTransientCommandContext
  -> TaTransientClientFrameWire decode
  -> RuntimeClientFrame
  -> host TaResearchTransientBackend.HandleAsync
  -> RuntimeFrame validation
  -> canonical RuntimeReducer.reduce
  -> session/channel RuntimeState
  -> TaTransientStateWire JSON
  -> PTCS opaque transient response
```

Package：`PulseTrade.Comm.Spa.Dynamic.Ptcs 0.1.0-alpha2`，exact PTCS beta82 + Contracts alpha4。state key固定為`sessionId + extensionId + channelId`；disconnect移除。Client提供的user/session欄位不參與identity，authoritative identity只來自PTCS context。

2026-07-14 correction：`disconnect` 需從 `states[key]` 取得 `RuntimeState.Identity.CanvasInstanceId`，先移除adapter state，再呼叫 `backend.HandleAsync context (RuntimeClientFrame.Unmounted canvasId)`。backend exception/result error不得讓adapter state復活；沒有既有state時cleanup為idempotent success。

Browser alpha2不交付。下一版wire不得直接把recursive generic `SduiValue` graph交給WebSharper compiler，改用bounded TA-specific rows/points/status DTO；server端再與canonical `SduiValue`互轉。browser adapter需獨立package、pure WebSharper、same-origin PTCS channel、無URL/header/credential參數，並以Playwright驗證last-good/in-flight/reconnect/history invariants。

## 2026-07-12 E2EQuotation adapter isolation

E2EQ integration採兩個可獨立pack的package，避免把legacy `Client.fs` graph併入Dynamic contracts/renderer：

```text
E2EQ provider/status/action
  -> PulseTrade.MarketData.E2EQuotation.Dynamic.Adapter
  -> canonical RuntimeDocument/RuntimeFrame/SduiAction

E2EQ browser flat DTO
  -> PulseTrade.MarketData.E2EQuotation.Dynamic.Browser
  -> validated RuntimeState
  -> PulseTrade.Comm.Spa.Dynamic.Renderer
```

server adapter負責bounded 2000-point snapshot、七列document與remote action mapping；browser adapter只接受flat bounded DTO、finite integral revision/sequence，並直接委派shared Renderer。兩者不得依賴PTCS host、SQL或fCell2。

E2EQ main host的feature-gated mount仍是獨立交付條件。Clean WebSharper compiler目前在legacy 512 KB client graph merge階段以`-532462766`終止；任何先前incremental build若引用舊bundle，不可作T-014/T-019證據。後續需提供isolated clean bundle/route，再執行PTCS/E2EQ Playwright geometry與AgentE2E parity。

## 2026-07-12 Query metadata and canonical action wire

`TaWorkspaceDocument.DefaultView`同時承載local view default與server-authoritative query metadata。TA adapter使用以下bounded keys：`query.sourceId`、`query.instrument`、`query.intervalMinutes`、`query.fromUtc`、`query.toUtcExclusive`、`query.includePartial`。`ta-browser.v1`以flat fields傳遞這些值，Ptcs.Client重建DefaultView後交給Renderer。

Renderer只在新的`DocumentRevision`同步query draft；Snapshot/Patch/Heartbeat不得重設使用者正在編輯的欄位。metadata缺失時欄位保持空白，禁止使用demo symbol/interval/date。Add Row `rowKind`一律使用lowercase canonical text，server parser仍case-insensitive以相容舊client。

## 2026-07-12 Composite trace 與 browser wire v2

```fsharp
type TaTraceKind = Candlestick | Volume | Line | Histogram
type TaTraceSpec =
    { TraceId: string; Kind: TaTraceKind; DataRef: string; Label: string
      Color: string; Width: float; Visible: bool; Options: Map<string,SduiValue> }
type TaRowSpec =
    { RowId: string; Kind: TaRowKind; DataRef: string; HeightWeight: float
      Visible: bool; Options: Map<string,SduiValue>; Traces: TaTraceSpec array }
```

`TaRowSpec.effectiveTraces`在`Traces`空時由legacy欄位導出一個trace。validation將row primary ref與所有trace refs加入document registry，限制trace id/dataRef uniqueness、每row trace count與style bounds。

`ta-browser.v2` series wire包含`mode=replace|upsert`、changed points與optional `removeBeforeTime`。server在document revision改變、初始、gap/resync時送replace；穩定document比較previous/next keyed state後只送upsert/remove-before/status。client先驗base revision，再merge、sort、套用2000-point retention；不符即`RequestFullSnapshot`。
# 2026-07-14 Commit-on-release and wire design

```fsharp
let draftStart = Var.Create None

onInput value =
    draftStart.Value <- Some (clampStart loadedCount visibleCount value)
    // preview only; no setWindow and no chart rebuild

onChange value =
    let startIndex = clampStart loadedCount visibleCount value
    draftStart.Value <- None
    if startIndex <> committed.StartIndex then
        setWindow { committed with StartIndex = startIndex; FollowLatest = isTail startIndex }
```

chart root暴露`data-chart-render-sequence`。sequence只在committed chart composition增加，Playwright用它驗證pointer down/move期間不變、pointer up/change後恰增1；不得用sleep推測效能。

```fsharp
MaxFullSnapshotPointsPerSeries = DynamicRuntimeDefaults.limits.MaxRetainedBarsPerSeries // 2000
MaxDeltaPointsPerSeries = 200

if previousRelevantSeriesIsEmpty && nextRelevantSeriesHasData then
    Full (tail MaxFullSnapshotPointsPerSeries next)
else
    Delta (tail MaxDeltaPointsPerSeries changed)
```

server JSON options使用`DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault`。client仍以`hasOpenValue/hasHighValue/...`判定OHLC fields；缺省false/0不改變point semantics。

Playwright需以real mouse drag驗證，不以`FillAsync`代替release lifecycle；同時驗loaded=2000、head/tail皆可到達、visible<=160、network action count不變。

# 2026-07-15 Overview / typed row editor / reset / copy design

```fsharp
type TaVisibleWindow = { StartIndex: int; Count: int }
type TaWindowDrag = Move | ResizeLeft | ResizeRight

previewWindowBounds minimum total committed drag delta
commitWindowBounds minimum total committed draft
selectionRatios total window
aggregateOhlc targetBuckets points
aggregateLine targetBuckets points
```

overview target bucket count由實際chart width換算並受hard limit；無geometry時使用deterministic default。left/right hit area大於可見handle。pointer move只更新`draftWindow`與selection/readout；pointer up才`setWindow`。selection擴到total時顯示`Full trend · compressed N bars`。主圖projection budget獨立於loaded retention，aggregation保留OHLC envelope及line/histogram first/last/min/max。

Add Row canonical options：

| Kind | Fields | `TaRowSpec.Options` |
| --- | --- | --- |
| SMA | Period | `period` |
| DMI | DI period | `period` |
| ADX | DI period, ADX period | `diPeriod`, `adxPeriod` |
| MACD | Fast, Slow, Signal | `fastPeriod`, `slowPeriod`, `signalPeriod` |
| Volume / Heikin-Ashi | none | empty |

editor Vars與DOM shell建立於renderer instance，不置於runtime revision的document composition內。submit前檢查positive period及`fast < slow`；row id為`row-{kind}-{sequence}`。server rejection保留editor/draft並顯示feedback；accepted document才收合或清理。

`ReplyPresentation.Actions`註冊`copy-json`。payload使用`extractReplyPayload context.Payload`的canonical結果；typed clipboard promise成功/失敗只更新action feedback，不能更動presentation mode。ResetCanvas由Host回initial command fresh snapshot後，renderer清hidden rows、cursor與draft window並依DefaultView重建Committed。

# 2026-07-15 Full export / draft query / slot geometry design

```text
download action
  -> mounted handle RequestJsonExport
  -> or collapsed requestJsonExportOnce (headless, bounded)
  -> RequestFullSnapshot("json-export")
  -> server stateToWire current (full, not delta)
  -> validate/apply wire
  -> ptcs-ta-research-export.v1 envelope
  -> typed Blob download yyyyMMddHHmmss-GUID.json
```

`TaResearchTransientClientHandle`持有`RequestJsonExport`，pending export只允許一個；busy時排在目前response之後。`requestJsonExportOnce`只在使用者點擊時建立不mount renderer的client，initial state後送explicit full request，下載完成即dispose。response只有`updateKind=full`才可完成export，error/close/dispose均清pending。download不更動history、viewport或IndexedDB；one-shot不得進入週期poll。

query draft以renderer-instance mutable state保存。`DocumentRevision`改變時才從`DefaultView.query`同步；`DataRevision` poll不得同步。Apply讀draft並送一次`ChangeQuery`，accepted document再更新authoritative值。

所有row使用：`slot=width/count`、`x=slot*(index+0.5)`；pointer使用`floor(ratio*count)`並bounded。K棒body/wick、line/histogram、cross-row cursor及floating values都使用同一visible index/domain。

## 2026-07-15 Stable editor shell / capability poll design

```fsharp
type TaClientLifecycleState =
    { PollEnabled: bool
      // existing lifecycle fields
    }

type TaClientLifecycleEvent =
    | StateAccepted of dataRevision: int64 * pollEnabled: bool

let pollEnabled document =
    document.AllowedActions |> Array.contains "poll-delta"
```

`StateAccepted`只有在`Active && PollEnabled`時產生`SchedulePoll`；`PollDue`與重新active亦須檢查同一flag。renderer以Document identity/revision作shell cache key；status/poll及chart使用nested `runtimeState.View`。所有remote button使用`attr.disabledBool`衍生live狀態，click callback再次讀`runtimeState.Value.Poll`，不得capture舊的`commandsDisabled`。

Document revision變更可重建rows/query shell；單純Poll/DataRevision不得重建`ta-add-row-kind`。Reset response抵達後，以authoritative initial Document取代rows並清HiddenRows/CursorIndex/draft window。

## 2026-09-04 Generic source envelope design

```fsharp
type SourceProjectionState =
    { Stream: SourceStreamIdentity
      SourceRevision: int64
      LastSequence: int64
      CapturedAtUtc: DateTimeOffset
      Payload: SduiValue }

type SourceResyncReason =
    | StreamChanged
    | SequenceGap
    | RevisionMismatch
    | SnapshotOrderConflict
    | DomainReducerRejected of reasonCode:string

type SourceApplyResult =
    | Applied of SourceProjectionState
    | Duplicate of SourceProjectionState
    | ResyncRequired of lastGood:SourceProjectionState * request:SourceSnapshotRequest

SourceProjection.applySnapshot :
    SourceProjectionState option -> SourceSnapshotEnvelope
        -> Result<SourceApplyResult, DynamicValidationError list>

SourceProjection.applyEvent :
    (SduiValue -> SduiValue -> Result<SduiValue, string>) ->
    SourceProjectionState -> SourceEventEnvelope
        -> Result<SourceApplyResult, DynamicValidationError list>
```

validation要求identity非空且bounded、revision/sequence非負、event new revision大於base revision、UTC timestamp與safe payload。codec沿用同一System.Text.Json options但以獨立source encode/decode入口維持frame/source type safety與byte limit。

event只有在stream identity相同、`Sequence = LastSequence + 1`、`BaseSourceRevision = SourceRevision`且domain reducer成功時套用。duplicate/stale no-op；gap、stream/schema/epoch改變、revision mismatch、crossed snapshot order或domain reducer reject保留last-good並回typed snapshot request。request不含payload或例外全文。

Daedalus adapter負責把authoritative `StructuredSeriesBatch`及其他owner payload轉成`SduiValue`與domain reducer；Aster source projection不引用owner type。apply成功也不直接變更DocumentRevision，workspace controller完成resource transition後才建立RuntimeFrame。

## 2026-09-04 Generic editor and correlated action design

```fsharp
type EditorValueKind =
    | Text | Integer of int64 option * int64 option
    | Decimal of decimal option * decimal option | Boolean
    | Choice of EditorChoice array | Scale of string array
    | List of EditorValueKind * int option * int option
    | Group of EditorFieldSchema array

type DynamicActionLifecycleState =
    { Pending: DynamicActionRequest option
      LastResult: DynamicActionResult option }

DynamicActionLifecycle.beginRequest actualDocumentRevision request state
DynamicActionLifecycle.complete result state
```

`DynamicEditorValidation`遞迴驗schema depth、總field數、choice/list上限、key uniqueness、numeric/list range與default value型別；所有default/choice payload亦套用shared unsafe-value規則。owner adapter只能提供data/choices，不可把script、URL、selector或credential塞入schema。

same-template instance以backend產生且跨revision穩定的`TaRowSpec.RowId`識別。renderer不得用array index、display label或參數hash當identity；document validator在duplicate RowId時拒絕整個frame並保留last-good。

`beginRequest`先驗request/canvas/action與expected revision；已pending時回`action-in-flight`，revision mismatch回typed `RevisionConflict`且不送backend。`complete`只接受相同RequestId；accepted/rejected均只清pending與記錄feedback，不修改RuntimeState document。下一個authoritative Document frame才提交add/remove/reconfigure結果。`RuntimeClientFrame` wire本slice不變，correlated transport與renderer pending UX由DYN-TA-017D接線，避免未同步Server/PTCS adapter時出現半套protocol。
