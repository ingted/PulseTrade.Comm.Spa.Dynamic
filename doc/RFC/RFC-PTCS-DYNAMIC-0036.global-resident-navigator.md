# RFC-PTCS-DYNAMIC-0036 Global Resident Navigator

- ID: RFC-PTCS-DYNAMIC-0036
- Status: Accepted / owner package and formal consumer validation passed
- Date: 2026-10-21
- Owners: PTCS.Dynamic Renderer / Aster; Research host consumer / Daedalus
- Related: RFC-PTCS-DYNAMIC-0032, RFC-PTCS-DYNAMIC-0033, RFC-PTCS-DYNAMIC-0034

## Background

The existing navigator describes the observations currently resident in the client and moves the visible chart window inside that resident interval. A Research document may have a much larger durable series, for example 2018 through 2026, while the client intentionally keeps only 1,500, 3,000, or 4,500 observations resident. Reusing the existing navigator for both domains makes a global relocation look like an inner viewport move and couples resident capacity to the 4,000-detail validation path.

## Goals

1. Add an optional whole-series overview above the existing resident navigator.
2. Let one pointer release request one atomic relocation of the resident interval.
3. Preserve the inner viewport width and resident-relative offset when the resident interval moves.
4. Keep the existing projection, actions, renderer, and wire behavior unchanged when the optional overview is absent.

## Non-goals

- No Research provider, query, storage, or derived-TA implementation in the owner package.
- No second coverage authority, polling loop, or per-pointer-move remote action.
- No change to the existing visible-range action or its 4,000-detail policy; D6 explicitly corrects capped inner-handle symmetry.
- No hard-coded 4,500 limit in generic contracts; the consumer advertises the current resident width.

## Decision

### D1. Additive whole-series projection

`TaLoadedCoverageProjection` gains `GlobalOverview: TaGlobalCoverageOverview option`. The overview carries the bounded whole-series axis and the resident interval within it:

```fsharp
type TaGlobalCoverageOverview =
    { TotalObservationCount: int64
      AxisRef: string
      Anchors: TaOverviewAnchor array
      ResidentStartObservationOrdinal: int64
      ResidentObservationCount: int }
```

The existing outer `CoverageIdentity`, `CoverageRevision`, and `QueryGeneration` remain the only authority and settlement identity. `GlobalOverview=None` preserves the 0.2.2 wire and renderer behavior.

### D2. Separate resident relocation action

The action is independent of `VisibleRangeChanged`:

```fsharp
ResidentCoverageChanged of
    CanvasInstanceId *
    baseRowId: string *
    relocation: TaCoverageWindowIntent
```

`BaseRowId` identifies the current document/base series and remains an action field. `TaCoverageWindowIntent` remains a pure ordinal relocation intent. The new path validates positive count, matching advertised resident count, and containment inside the whole-series total. It does not apply `VisibleRangeChange.MaximumBasePoints`, `MaximumVisibleRangeBasePoints`, or `MaximumActiveDetailBars`.

### D3. Local draft and settlement

Global drag changes only a renderer-local draft. Pointer release emits at most one `ResidentCoverageChanged`. Matching coverage revision and query generation commit the projection; rejected, conflicted, or superseded actions clear or replace only their own global draft. No timer, polling loop, or action is emitted during pointer movement.

### D4. Preserve the inner viewport by relative offset

When the resident interval moves, the inner viewport keeps its width and its offset relative to the resident start. For resident width 3,000, inner offset 1,500, and viewport width 700, relocation to `newResidentStart` produces `newResidentStart + 1,500` with width 700. Near a boundary, clamp the offset while preserving width; shrink width only when the complete series itself is shorter. The old absolute ordinal is not retained.

### D5. The visible selection owns the next gesture

While a remote coverage action awaits its response or matching projection, the renderer may show a newer optimistic selection than the authoritative projection. A new pointerdown therefore takes its index baseline from the rendered or published draft and its ratio baseline from the navigator's currently rendered `data-selection-left-ratio` / `data-selection-right-ratio`. It must not clear that draft before choosing the baseline. Internal ratio overrides are fallback state only; CSS/SVG bounding-box conversion is not authority because borders and view-box scaling can introduce visible edge drift.

## Impact

### D6. 2026-10-23 capped resident handle mirror

Owner daedilus轉交使用者指定：保留既有left外拉在maximum visible後維持寬度並平移的行為，right作幾何鏡像；內拉仍縮小。4500 resident、start1500/count1500、maximum1500時，left delta-300為start1200/count1500，right delta+300須為start1800/count1500。跨maximum時僅超出部分平移，resident端點仍clamp；不改left、body move、cancel、drop-only action、outside paging或wire。DYN-WBS-587 / DYN-T-685先對0.2.23加失敗回歸，再最小修right計算；package與consumer restart須等待owner完成latency baseline。

GLOBAL更新接近兩分鐘另列latency P0（DYN-WBS-588）：四項functional真3M PASS不代表latency通過。首輪15.096秒含已知probe干擾，非正式基線；須以低干擾drop/request/body/decode/prepare/projection-commit分段定位，不以放大timeout或改queue掩蓋。

### D7. 2026-10-23 bounded browser work turns

低干擾0.2.23實測body→prepare約5.7–6.2秒、prepare→commit約3.07–3.14秒，CPU以idle為主；逐256-point decode及逐entry prepare都強制等一個RAF造成排程下限。0.2.24候選共用既有schedule seam的FIFO executor，每turn以4ms/64 atomic steps雙上限處理廉價工作，達任一上限才再排RAF；保留256-point task、順序、每step generation檢查、last-good及final projection paint barrier。單一atomic step仍不可搶占，4ms是soft budget，並非單task硬上限。沒有新worker、service、action queue或timeout。

`documentElement`的`data-frame-pump-*`及`data-renderer-prepare-*`公開work-count/turn-count/work-ms/queue-ms/wall-ms/max-turn-ms/pending-count；前者sequence識別最後pump batch，後者generation+instance識別prepare，建立新batch時清零，舊generation不覆寫。這是per-batch觀測，不冒充整個HTTP action；原onStageCompleted仍只表示該atomic task執行時間，signature不改。FramePump四條browser wrappers、WebSocket decode/reduce入口及Renderer full/incremental preparation用同一executor；action settlement、row refresh與最終paint barrier維持原路徑。

### D8. 2026-10-24 committed datapoint activation

Main TA chart click新增獨立browser-local bubbling event `ptcs-dynamic-ta-point-activated-v1`；不得把tagging語意塞入既有`SharedCursorChanged` server action。event detail包含canvas/document runtime identity、document/data/transport revision、coverage identity/revision/query generation、base/row/trace/data-ref、canonical event time、trace kind及component，禁止viewport index。只有payload snapshot等於Renderer最後committed projection key時才能dispatch。

Candlestick依已render的slot及high/low範圍命中並回傳`BAR`；line、histogram、volume依目前rendered x/y geometry選最近visible datapoint並回傳`POINT`。SVG以`[data-ptcs-ta-point-surface='true']`提供穩定surface selector，事件由實際clicked surface發出並向上bubble。Research consumer仍負責以`RunId + DataRef/EventTimeUtc`解析committed `PointId`／component並開official editor；初次GLOBAL accepted後仍顯示舊resident/TA是另一個commit-receipt調查，不由本決策猜修。

### D9. 2026-10-24 GLOBAL calendar axis

GLOBAL overview的長期domain不得沿用一般TA／RESIDENT的`MM-dd HH:mm`軸。專用calendar axis只從`GlobalOverview.Anchors`選tick：每年第一個真anchor標`YYYY`，3／6／9／12月第一個真anchor標兩位月份，不生成假日期。所有mandatory candidates皆保留；viewport擁擠時按實際pixel spacing動態增加垂直lane，各lane守最小水平間距，同anchor年度與季度分lane。DOM tick保留原canonical event time及numeric lane供驗證；此決策不改anchor contract、selection ratio、drag／preview／commit、hit target、一般TA／RESIDENT軸或consumer API。

0.2.27 correction：0.2.26把year與quarter放同lane並互相排除，真3M／1600px使2019–2025每年03／12缺失。year與quarter改為垂直分lane，各lane獨立24px spacing；同anchor可同時顯示`YYYY`與季度，完整年度不得因另一lane標籤而少月份。其他D9邊界不變。

0.2.28 correction：0.2.27固定雙lane在1600px通過，但900px的單一quarter lane仍因24px suppression缺四個03。D9改為不drop candidate的dynamic lane packing，axis高度依lane count成長；viewport不再是省略03／06／09／12的例外。其他D9邊界不變。

### D10. 2026-10-24 consumer-owned actual GLOBAL bootstrap

`initialGlobalPhase`只排程已進入`RuntimeState`的overview，不能讓約271.6ms已由Research取得的真overview早於約+3.296秒chart stack顯示，因authoritative document約+10.412秒才帶入GLOBAL。新增唯一typed input `unit -> Async<TaGlobalCoverageOverview option>`：consumer擁有execution選擇、IDB/DB與resident policy；owner只驗證並render read-only actual GLOBAL。第一個`RuntimeState.Document=Some`是monotonic replacement authority，原位換成既有authoritative renderer；bootstrap不建立action、drag、projection receipt、RuntimeState、HTTP/IDB/cache或第二authority。Research已證實自有SPAEntryPoint與同一Doc tree，故DOM event/ready attribute候選退役。

### D11. 2026-10-25 exact main-plot boundary pan and bilateral Y axes

主圖drag的authority是`mainPlotPanDelta`算出的實際K-bar delta，不是整頁adjacent gesture。跨local boundary時，intent start=`current absolute start + delta`且visible count不變；provider可擴大resident，但projection回來後必須以requested absolute start相對returned active start補local offset，只有完整containment才settle。整頁navigator gesture沿用既有helper。每row左右各顯示五個由同一committed low/high geometry導出的read-only Y ticks，預留gutter且不得攔pointer或改trace／cursor／navigator資料流。

### D12. 2026-10-25 authoritative GLOBAL replacement and independent consumer domain

D10的「第一個Document」依真no-1K RED修正：若有actual GLOBAL loader，`Document.IsSome`不是replacement authority；只有loaded-coverage decode/validation成功且含`GlobalOverview`才可monotonic卸載bootstrap。無loader client仍以document mount。Research/DB擁有1K GLOBAL total/resident與active 5K間的time/ordinal mapping，已在既有Dynamic contract下證明2,996,994 total、7420-point resident與global drag回映；因此0.2.31不新增axis mapping wire，也不讓Renderer猜跨scale arithmetic。

### D13. 2026-10-25 stable GLOBAL host handoff

0.2.32真no-1K cold-cache驗證證明D12只解決「何時可replace」，未解決WebSharper replacement的DOM ordering；bootstrap detach到authoritative attach間仍有5個MutationObserver callback batch無visible GLOBAL。Client改為先在root內一次掛載bootstrap與authoritative stable hosts，authoritative內容先進入DOM並由RAF確認GLOBAL geometry可見，才清空／移除bootstrap host。期間允許短暫兩者並存，禁止短暫為零；最後仍只留一個authoritative GLOBAL。此決策不改typed loader、RuntimeState／projection authority、no-loader lifecycle、Dynamic wire或consumer cross-scale mapping。

### D14. 2026-10-27 cross-axis inner navigator event-time authority

跨axis inner navigator仍只送`VisibleRangeChanged` event-time seam，不得將GLOBAL observation ordinal當active/base index。pointer preview與drop action必須使用同一frozen timeline authority：當`GlobalOverview.AxisRef <> ActiveDetail.BaseAxisRef`、`OverviewAxisRef = ActiveDetail.BaseAxisRef`，且validated overview anchors與active prepared timeline為exact one-to-one長度時，drop range取overview anchor的原始UTC timestamp；這使resident overview local0映到同一local0 event time。任一axis／長度／timestamp validation不精確即沿用既有active timeline，禁止比例插值或推測ordinal換算。same-axis resident coverage intent、main CURSOR與consumer mapping責任不變。

### D15. 2026-10-27 accepted main-pan event-time reanchor

non-CURSOR main exact-pan送出的`VisibleRangeChanged`以UTC半開range為viewport authority，`CoverageIntent`只要求consumer materialize所需active detail。當accepted projection已把resident／ActiveDetail按同一pan delta relocation，Renderer不得把optimistic local delta再次套用；它須保存該action的base row、exact start event time、count與query generation，並在matching prepared timeline中以normalized UTC exact match重定位。較舊generation等待；action尚pending且range未出現時等待；terminal frame仍無完整range則拒絕並清draft。identity／coverage scope／action reject均清pending。禁止用GLOBAL ordinal、比例插值或保留舊local index猜測settlement；Resize、navigator、CURSOR與wire不變。

### D16. 2026-10-27 cross-axis exact event-time materialization intent

D14只使cross-axis navigator preview與action取得正確UTC range，沒有要求consumer materialize該range；`CoverageIntent=None`時consumer只能沿用既有visible-page shift，真18884因而在local0 drop後只移動79 bars。當且僅當D14的validated cross-axis one-to-one timeline成立時，Renderer須為同一frozen window附加`ExplicitBounds` intent：`StartObservationOrdinal=None`、`Direction=None`、當前coverage revision、下一query generation與exact visible count；UTC半開range仍是位置authority，禁止建立GLOBAL ordinal。accepted frame沿用D15 exact UTC settlement，但pending state泛化為event-range relocation。same-axis、長度或timestamp validation不精確的fallback仍不附加此intent，既有resident/local action、preview與wire shape不變。

### D17. 2026-10-27 accepted settlement precedes coverage-identity viewport reset

D15的「coverage scope清pending」只適用pending action與next document/base row不相符的真正scope切換。`viewportScopeKey`包含CoverageIdentity，而accepted relocation本來就可更新CoverageIdentity；若matching pending event-range已Reanchor／Reject，該settlement擁有viewport且不得再被document default覆寫，並把新scope標為已套用；若matching action仍Await，保留pending與當前optimistic viewport，直到matching／terminal frame。只有base-row不符或沒有matching pending時才清舊context並套default。canvas／identity切換既有clear path、wire與server materialization契約不變。

### D18. 2026-10-27 matching settlement survives full-preparation scheduling

D17的precedence必須從`runtimeState.View`收到accepted data起生效，不能只在`acceptPreparedDataCore`套用。當data與coverage scope同時改變、pending event-range與next document/base row相符時，full-preparation排程前保留pending與optimistic draft，讓prepared timeline執行D15的Reanchor／Await／Reject；否則accept-stage永遠看不到settlement context而退回local continuation。identity／canvas切換及base-row不符仍立即清除，queued viewport、boundary pan、resident navigator authority的既有scope reset不變。

### D19. 2026-10-27 settlement state and prepared timeline are one frame

event-range settlement的projection authority與prepared timeline必須來自同一committed frame。action completion callback不得把已前進到新query generation的`runtimeState.Value`與仍屬舊generation的`latestPreparedData`混配；只能以`chartCommittedFrame`的atomic state/prepared pair重試。新frame真正prepared後，accept path仍以同一次`next + nextPreparedData`執行D15 settlement。舊committed frame只會Await，不得提前Reanchor／Reject或清pending。

### D20. 2026-10-27 cross-axis materialization is independent of timeline substitution

D14的one-to-one overview validation只決定navigator event-time range是否改由overview anchors取得，不應決定server是否需要materialize。只要`GlobalOverview.AxisRef <> ActiveDetail.BaseAxisRef`，navigator已從任一exact prepared timeline取得UTC半開range後，就必須附D16的no-ordinal `ExplicitBounds` intent；overview substitution不可用時沿用exact active prepared timeline，仍不得省略intent。same-axis仍維持既有local／resident path，禁止比例插值或推測GLOBAL ordinal。

### D21. 2026-10-27 main-plot boundary pan uses active-detail authority

main CURSOR pan在captured formal-base timeline內仍用exact UTC半開range；當signed delta明確越過timeline頭／尾時，不得一律fail closed，必須沿既有adjacent-coverage path發typed intent並保留visible count與實際delta。cross-axis resident frame的absolute active ordinal以`ActiveDetail.StartObservationOrdinal + delta`計算；local window start是resident buffer offset，不可再加到active start。same-axis仍沿用active start + local start + delta。delta=0、空timeline或非boundary invalid state不送action，也不用GLOBAL sampled anchor。

### D22. 2026-10-27 coverage window carries explicit producer purpose

no-ordinal／no-direction／`ExplicitBounds`只描述range authority，不能區分main pan與cross-axis navigator window；ObservationCount也不能作分類，因navigator resize後的等寬move會與current visible count相同。`TaCoverageWindowIntent`新增required `Purpose: TaCoverageWindowPurpose`，值只允許`MainPlotPan`或`NavigatorWindow`，`WindowIntentSchema`同步由v1升v2。main plot event-range、boundary ordinal及provider-open producer固定MainPlotPan；cross-axis navigator event-range與resident navigation固定NavigatorWindow。

Purpose不替代StartOrdinal／Direction／RangeAuthority：有ordinal/direction的overscroll仍按既有materialization path處理。Consumer不得由count、axis或missing purpose推論；升級時total match此DU，MainPlotPan保visible count並同步平移resident+visible，NavigatorWindow只套用fixed resident內durable visible window。Contracts wire round-trip、resident action validation與兩個Renderer origin tests為owner gate。

### D23. 2026-10-27 navigator purpose survives accepted axis settlement

D22的purpose由producer interaction seam決定，不由每次action當下的axis equality重新推論。Accepted navigator materialization可能改變Global／Active axis關係；後續同一inner navigator的resize、move或長body drag仍須附`NavigatorWindow`。Same/cross-axis只選擇exact prepared event timeline及ordinal是否可安全使用；它不得把typed origin降成`CoverageIntent=None`。Event-time range繼續作位置authority，same-axis navigator event-range亦為`ExplicitBounds`、`StartObservationOrdinal=None`、`Direction=None`。Main plot、resident ordinal與adjacent boundary path不變，禁止count fallback。

### D24. 2026-10-27 rendered navigator projection comes from prepared frame data

D23只有在pointerdown取得projection時才生效。`chartCommittedFrame`是`RuntimeState * TaPreparedRendererData` atomic pair；navigator、timeline與coverage UI由prepared `RawData`實際render，故pointerdown也必須由同一prepared `RawData`解析loaded-coverage projection。Paired runtime `Data`可仍是未resolved envelope；用它解析失敗後降成bare `VisibleRangeChanged`會違反D22。禁止改讀較新的`runtimeState.Value`／`latestPreparedData`，也禁止由count或DOM反推；document/revision/key仍取paired runtime state，projection/timeline/range取paired prepared data。

### D25. 2026-10-27 pointerdown captures the exact rendered shell frame

D24把prepared欄位選對但把frame authority認錯：實際inner navigator由`ShellPrepared` view render，`chartCommittedFrame`是另一個可獨立前進的chart authority。0.2.59真B證明前者已顯示canonical target0，pointerdown重讀後者仍可解不到projection並落入bare fallback。Shell authority因此改為atomic `RuntimeState * TaPreparedRendererData` pair；每次shell publication同時更新兩半，navigator render closure把該exact pair直接capture進event handler。Pointerdown不得再讀`chartCommittedFrame.Value`、`runtimeState.Value`或`latestPreparedData`。D24的prepared-RawData要求仍有效，但由D25 rendered shell pair提供；chart frame只繼續服務chart commit/settlement，不再冒充navigator interaction authority。

### D26. 2026-10-27 rendered navigator inputs include the shell document and decoded projection

0.3.0真B反證D25仍少列兩個render authority：`shellPreparedFrame.View`的navigator不是只由pair內的state/prepared render；外層cached shell `document`與由該document＋prepared RawData得到的`currentCoverageProjection`也直接決定GLOBAL／RESIDENT UI、selection與event-range materialization。Handler若再以`renderedState.Document`重解projection，便建立第三個authority；兩個document envelope語意相容但loaded-coverage表示不同時，render已持有projection而pointerdown仍會得到None並送bare action。決策改為render closure顯式傳入`renderedState + shell document + prepared data + decoded projection`；state只供identity／commit key，document／projection／timeline皆使用實際render輸入，handler不得再decode。Wire、consumer、oracle、event-time range與2018 tail mapping不變。

### D27. 2026-10-27 persistent DOM listener reads renderer-published gesture authority

0.3.1真B與deployed JS證D26 call已存在，但accepted rerender後第二次pointerdown仍走`currentCoverageProjection=None`的舊closure。WebSharper可保留同一navigator SVG與event listener，只更新reactive visual／attributes；因此把完整frame放在render callback lexical closure仍不足，listener本身未被替換時會永遠讀建立DOM當下的frame。決策是在renderer instance層保留唯一stable `navigatorGestureFrame` Var；實際建立navigator DOM的`ShellPreparedFrame.View` mapper算出shell document/projection後，原子發布`renderedState + document + prepared data + projection`，persistent callback每次事件發生時才讀該Var。這是render path明確發布的authority，不是重讀任意current/latest state；wire、consumer、oracle、GLOBAL ordinal與2018 tail mapping不變。

### D28. 2026-10-27 retained mappers converge on the atomic frame document

0.3.2真B與deployed generated JS證listener已在event invocation讀stable Var，但Var內projection仍可為None。根因是Var只有單一reader authority，publication仍有多個保留的`ShellPreparedFrame.View` mapper；每個mapper以自己的lexical cached shell `document`搭配最新prepared RawData解projection，舊mapper可在新mapper後發布None並覆寫Var。Shell cache刻意用normalized presentation忽略coverage/query counters，故lexical document不是current coverage authority。決策改為每個mapper先從atomic `renderedState.Document`取得`currentDocument`，再以同一frame的prepared RawData解projection並發布tuple；outer document只在frame暫無document時作顯式fallback。如此所有retained mapper即使並存也收斂到同一frame document/projection，不靠subscription執行順序。Wire、consumer、oracle、host與event-time intent不變。

### D29. 2026-10-27 runtime publication authority must be directly observable

0.3.3真B證明source-level atomic frame reasoning仍不足以區分`projection=None`的publication來源。在改變行為前，Renderer必須以bounded document-root telemetry將publication與pointerdown對齊到renderer instance/publication sequence，並分別記錄state.Data、prepared.RawData與resolved projection。Trace只保留last-12 events，不含raw bars、不改wire/action/consumer。該instrumented package只是根因觀測gate，不能因owner tests綠就宣稱功能修復；必須由原封不動的真Gate B capture指出最早分歧，再實作永久修復。

### D30. 2026-10-27 accepted query viewport waits for its authoritative prepared frame

`ChangeTaQuery` action result與Renderer prepared-frame commit是不同authority。Client可先合併raw RuntimeFrame並完成accepted callback，但Renderer仍可持有舊prepared timeline；此時對requested range的`NoIntersection`不得成為最終結果。Dispatch必須凍結baseline DataRevision與loaded-coverage QueryGeneration，accepted只留generation-scoped pending intent；同identity且兩個revision皆前進的prepared frame才可單次套用temporal viewport。Newer local query、rejection或identity replacement取消舊intent。禁止timer/retry、重送action、直接對runtimeState求viewport或改consumer expected；wire與0.3.6 edge behavior不變。

### D31. 2026-10-27 accepted callback reads the latest shell-prepared authority

0.3.7真Gate證明只revision/generation pending不足：frames與preparation可在Accepted completion前完成，而render shell已顯示新ActiveDetail；若callback之後只回查不保證每個row-refresh前進的`chartCommittedFrame`，pending可永久Await。決策是以render shell實際使用、且在topology/row-refresh/full preparation都更新的`shellPreparedFrame`作Accepted callback回查authority；後續prepared completion仍走原settlement seam。同版公開bounded last-outcome DOM telemetry，但telemetry不參與決策。不改wire、raw payload、consumer expected或host/oracle。

### D32. 2026-10-27 loaded-coverage generation is not ChangeTaQuery acknowledgement

0.3.8真Gate證明D30的雙counter必要條件錯置authority：fresh `ChangeTaQuery`可使DataRevision與requested prepared data前進，但loaded-coverage QueryGeneration維持0；formal長序列的18只是先前coverage-window action counter。Wire上`TaQueryChange`沒有generation，只有`TaCoverageWindowIntent`與`TaLoadedCoverageProjection`持有此欄。決策是保留local IntentGeneration與DataRevision gate；coverage generation前進時可完成positive/negative selection，generation穩定時只有`Selected` positive materialization可完成，`NoIntersection`繼續Await。D32 supersedes D30的「兩個revision皆前進」句，不改D31 shell-prepared authority、wire、consumer expected或host/oracle。

- Contracts and wire codecs gain optional/additive fields and one action discriminator.
- Renderer gains a second, visually separate navigator and an independent draft/pending state machine.
- Interactive.Client transports the new action without Research-specific vocabulary.
- Research host owns whole-series anchors, durable relocation authority, resident materialization, and derived-series shared-axis updates.

## Current owner state (2026-10-22)

- Contracts and renderer source now contain the additive action allowlist, independent move-only global strip, local draft/pending settlement, and a BrowserDemo 10,000-total/1,500-resident fixture. Contracts Expecto passes 51/51 and Renderer model/source-contract Expecto passes 71/71.
- The first attempted `0.1.37 -> 0.2.9 -> 0.2.9` artifacts are retired: `GeneratePackageOnBuild=true` caused direct `dotnet pack` to reuse an older Release DLL, so those packages do not contain this UI.
- Owner candidates are Contracts `0.1.38`, Renderer `0.2.10`, and Interactive.Client `0.2.10`. All three passed fresh local-first package publication; Renderer and Interactive.Client local package SHA-256 values are `ECCD46DADDEEAD7AAE3F9BAD488552D84923E658FD4139FC88DA6387FDB6DCE1` and `84BB347B698CDC9BC15791CC842A78C29BBE2804CCA7F2378E5B4EAE72F460AA`. The package-consuming BrowserDemo build passes, and its Expecto Playwright gate passes the existing resident gesture plus the independent global relocation/drop-only action. Daedalus formal 18884 consumer validation remains pending.
- Existing `0.2.8` remains the valid chained-gesture release and does not contain the global strip.

## 2026-10-23 resident drop correction

- DYN-WBS-587 / DYN-T-683：resident drop保留rendered draft，等action settlement與matching committed coverage兩者滿足才清除；同resident中的較新revision不得取消正在操作的手勢，排送時使用最新revision/generation。
- DYN-WBS-588 / DYN-T-684：body pointer仍在track內時先clamp至端點；只有pointer真的越出track才要求相鄰頁，目標以resident邊界加減完整page，保持visible width。
- Renderer / Interactive.Client候選0.2.21沿用既有wire；focused與真3M快速連續drag、舊兩黑閃/global回歸須分開驗。尚無獨立完整resident payload descriptor，不能將ActiveDetail width改4500；此資料契約缺口保持open，consumer owner為daedilus。

## Acceptance

0.2.23 authority clarification（DYN-WBS-588）：GlobalOverview存在時，其TotalObservationCount是導航上限；頂層loaded total未知不能把全域導航clamp到已載入尾端。renderer intent/queue/followLatest與adjacent-cache同用此authority；GlobalOverview=None保留legacy。不因此把未載入ordinal視為local payload。

0.2.22 clarification（DYN-WBS-587）：最後pointer位置而非上一個painted frame為drop intent authority；pointerup必須吸收尚未paint的最後move並同步hold geometry。0.2.21實例local1498→1423的75bars偏差為獨立RED；release不得要求人類等下一RAF再放開。右外拉無action及完整resident payload仍open。

1. Existing 0.2.2 tests remain unchanged and pass with `GlobalOverview=None`.
2. Projection codec round-trips both absent and present global overview; old payloads decode to `None`.
3. Action codec round-trips `ResidentCoverageChanged` with `BaseRowId` outside the relocation intent.
4. A 4,500-observation resident relocation is accepted by the generic path and is not clamped to 4,000.
5. Drag emits no remote action; one drop emits exactly one action.
6. Settlement is revision/generation-safe and stale results cannot clear a newer draft.
7. Inner viewport width and resident-relative offset are preserved across relocation, with bounded edge clamping.
8. Daedalus formal consumer proves whole-series navigation, 2018/2026 relocation, derived-TA shared axis, and existing inner navigator behavior.
9. Pointerdown freezes the rendered selection ratios and semantic left/right/body ownership. An authoritative projection arriving before pointerup may rebuild the shell but cannot move the active preview, its fixed edge, or body width; pointerup still emits exactly one intent.
10. If a second gesture starts while the first gesture's remote response is still pending, pointerdown keeps the exact rendered edge as its baseline. The first response may arrive while the second gesture is held without moving its fixed edge, changing its body width, clearing its semantic mode, or suppressing its one later intent.
11. At and across the advertised maximum visible width, right resize mirrors the existing left resize, including inward shrink, minimum width, empty/short domains and both resident boundaries. Unit geometry and real 4500/1500 consumer gestures are separate gates.
12. GLOBAL latency acceptance records low-overhead drop-to-ready phases and probe overhead; functional frame continuity alone does not close this performance gate.
13. Main TA chart activation is a separate bubbling browser event, carries only committed stable identity, and never uses a viewport index or changes `SharedCursorChanged` semantics.
