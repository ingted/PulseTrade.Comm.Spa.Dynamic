# RFC-PTCS-DYNAMIC-0031：Page-scoped Display Time Zone

- ID：`RFC-PTCS-DYNAMIC-0031`
- 狀態：Accepted / owner official release PASS / consumer validation pending
- Owner：PTCS Dynamic Contracts／Renderer（Aster）
- Consumer：PulseTrade.Comm.Spa.Dynamic.Interactive.Extension／SPAA／DIB（Daedalus）
- 上游需求：`G:\coldfar_py\coldfar-symbolics\doc_new2\RFC\RFC-TRADECORE-0029.page-display-time-zone.md`
- 關聯：`DYN-WBS-571`、`DYN-T-629..633`

## 背景

TA／Backtest workspace目前直接顯示canonical UTC字串。Trader需要在同一頁切換`UTC`、`CT`、`ET`與固定`UTC+8`，但資料lookup、patch identity、cursor correlation、CSV與query instant仍須維持UTC。若SPAA、DIB與Renderer各自格式化，DST邊界、snapshot／live patch及tooltip很容易產生不同語意。

## 目標

1. Contracts提供四種穩定display-zone value與wire id：`UTC`、`America/Chicago`、`America/New_York`、`UTC+08:00`。
2. Renderer提供唯一timestamp projection／formatter與page-scoped reactive input。
3. Axis、shared cursor、row data window、temporal metadata、Marker／OverviewStripe／OFI可見時間與tooltip使用同一selection。
4. CT／ET依event instant套用CST／CDT及EST／EDT；UTC+8固定offset。
5. Snapshot與後續patch沿用同一selection；切換不觸發provider、FSSTL或Backtest action。

## 非目標

- 不改`RuntimeFrame`、document／patch schema、IndexedDB、provider、TradeCore或Backtest wire。
- 不改`End K`等query欄位的UTC parsing。
- 不改canonical UTC DOM attribute、marker slot、revision、scenario identity或download欄位。
- 不讓Renderer推論交易時區、商品session或browser local timezone。
- 不在consumer複製timestamp parser或DST規則。

## 決策

### Package ownership

- `PulseTrade.Comm.Spa.Dynamic.Contracts`：`SduiDisplayTimeZone`與stable id／label codec。
- `PulseTrade.Comm.Spa.Dynamic.Renderer`：`TaDisplayTimestamp`與唯一formatter；Renderer reactive wiring。
- `Interactive.Client`／`Ptcs.Client`：只精確引用新版package，不另造formatter。Consumer以typed zone `View`呼叫新版renderer overload。

### Public API

```fsharp
type SduiDisplayTimeZone =
    | Utc
    | AmericaChicago
    | AmericaNewYork
    | FixedUtcPlus8

TaDisplayTimeFormatter.tryFormat : SduiDisplayTimeZone -> string -> TaDisplayTimestamp option

TaWorkspaceRenderer.renderWithProjectionCommitAndDisplayTimeZone :
    TaRendererOptions ->
    TaRendererCallbacks ->
    (RuntimeState -> unit) ->
    View<SduiDisplayTimeZone> ->
    Var<RuntimeState> -> Doc

TaWorkspaceRenderer.renderWithDisplayTimeZone :
    TaRendererOptions ->
    TaRendererCallbacks ->
    View<SduiDisplayTimeZone> ->
    Var<RuntimeState> -> Doc
```

既有`render`及`renderWithProjectionCommit`維持並委派到`UTC` selection，避免既有host被迫同步改碼。

### Presentation and identity

- Formatter輸出full、compact、date、clock、zone id與abbreviation。
- 可見文字帶zone abbreviation；CT夏／冬分別顯示CDT／CST，ET為EDT／EST。
- Stable DOM保留canonical UTC attribute，另提供`data-display-time-zone`。
- Zone切換只重新投影文字／tooltip；不得變更`DocumentRevision`、`DataRevision`、`LastTransportSequence`、viewport、cursor canonical event time或selected scenario。
- 無法解析的非時間label維持原字串；格式不合法的canonical event-time不偽造local instant。

## 失敗與相容性

- Unknown zone id由codec拒絕；consumer不得默認browser local timezone。
- Invalid timestamp由formatter回`None`；Renderer顯示原字串或`Unavailable`並保留canonical attribute。
- 同頁多個renderer各持有自己的reactive selection，不使用global mutable zone。
- Browser bundle必須由F# source重建；禁止inline JavaScript／`.js` patch。

## 驗收

1. `2026-06-17T22:01:00Z`顯示UTC `22:01`、CT `17:01 CDT`、ET `18:01 EDT`、UTC+8次日`06:01`。
2. `2026-01-02T15:00:00Z`顯示CT `09:00 CST`、ET `10:00 EST`；另驗DST切換邊界。
3. Synthetic snapshot後patch，在不換selection下新增的visible time使用相同zone。
4. 切換前後canonical axis／cursor／marker attributes、runtime revisions、loaded bars、viewport及cursor slot相同。
5. Axis、row cursor、data window、metadata、Marker／Stripe／OFI tooltip在同一selection更新。
6. 既有UTC API與renderer lifecycle／4,000-slot performance gates維持全綠；console/page error為零。
7. Daedalus以真SPAA驗Backtest visible times及no-fetch/no-run/no-revision-drift；owner browser gate不冒充consumer E2E。

## 實作順序

1. Contracts typed zone／codec與unit tests。
2. Renderer formatter與summer／winter／boundary tests。
3. Reactive renderer API與所有owner-visible timestamp wiring。
4. Browser snapshot→patch、zone switching、identity/performance regression。
5. Exact package graph、consumer handoff、official release/readback。
