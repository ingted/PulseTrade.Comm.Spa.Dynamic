# SD-PTCS-DYNAMIC-TA-0001 Transport-Neutral Realtime TA Canvas Runtime

Status: Accepted / Ready for DEV
Date: 2026-07-11
REQ: `doc/TAResearch/REQ.md`
RFC: `doc/RFC/RFC-PTCS-DYNAMIC-0007.realtime-ta-canvas-runtime.md`
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

Contracts同時擁有transport vocabulary與pure reducer/registry/poll state machine，讓PTCS與E2EQ可得到完全相同的state transition而不載入WebSharper。Contracts不得referenceWebSharper/PTCS/fCell2/PTMD。Renderer不得referencePTCS/fCell2/PTMD/SQL。facade以exact NuGet package reference消費Contracts/Renderer，禁止ProjectReference作release-facing integration。

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
