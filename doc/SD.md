# System Design (SD) - PulseTrade.Comm.Spa.Dynamic

## 2026-10-27 physical resident-track boundary gate（DYN-WBS-617）

新增pure `navigatorGestureBoundaryDirection hasGlobalResidentTrack ... pointerX trackWidth`。無GLOBAL resident track時維持既有logical `navigatorBoundaryDirection`；有GLOBAL resident track時沿用`residentNavigatorBoundaryDirection`，只有pointerX小於0或大於trackWidth才回Earlier/Later。Production finish從captured `loadedCoverageSnapshot`判定`GlobalOverview.IsSome`，pointer仍在track內便走既有`commitLocalDraft`，由`setNavigatorWindowFromPrepared`送event-time `NavigatorWindow` intent；ordinal mapping與consumer不變。

DYN-T-731鎖真值`start1198/count80/rawDelta-1200/pointerX10`：preview為local0，resident-track boundary=None；pointerX=-0.1才為Earlier。這直接覆蓋真B的pixel quantization，不以更改expected或比例換算繞過。

## 2026-10-27 bounded navigator publication telemetry（DYN-WBS-616）

`navigatorCoverageTelemetryStatus defaultView data`以bounded text區分inline/ref-present/ref-missing/ref-invalid，再輸出none／some revision+generation+active range／第一個decode error code+field。Navigator mapper每次publication與persistent listener每次pointerdown共用`publishNavigatorGestureTelemetry`，在document root寫event、renderer instance、publication sequence、document source、state/prepared map counts與decode states、projection、document/data/transport revisions；`data-renderer-navigator-frame-trace`只保留最後12筆。

DYN-T-730鎖production publication/pointerdown seam，DYN-T-730B鎖missing dataRef、resolvable projection與bounded trace。0.3.3 source實跑0/1 RED；0.3.4 source `aea3d7602d55853b5508e57559f803c8c630ae78`已push。Final immutable Renderer／Interactive.Client SHA-256=`EACB0830E07F81DE16ABC5D7F536AFC3A1F6CA4DD14BF1006B8A520DF28D06D1 / 1A8A909CB7AD55B1B1EDE22F871079207768A521A2098EBCBAA958A76E6C2376`，provenance exact；fresh final Renderer120/120、Interactive20/20、verifierPASS，package-only BrowserDemo exact0.3.4且generated JS含trace attribute。此版只是觀測性gate；必須先取得Daedilus同一真B的publication/pointer trace，再修根因，不以忽略None或保留舊Some作workaround。

## 2026-10-27 retained mappers publish one frame document（DYN-WBS-615）

新增純函式`navigatorDocumentForRenderedFrame cachedDocument renderedState`：有frame document時必選frame值，只有None才fallback cached shell。Navigator mapper收到`(renderedState, currentPreparedData)`後先算`currentDocument`，timeline、maximum-visible-bars、coverage decode與`navigatorGestureFrame` publication全用該document；禁止再出現`tryLoadedCoverageResolved document.DefaultView currentPreparedData.RawData`。因此多個retained mapper即使都收到同一shell frame，也會發布相同document/projection，不再由舊lexical document覆寫None。

DYN-T-729鎖actual production wiring，0.3.2 source 0/1 RED；DYN-T-729B以兩個不同document證frame document優先、None才fallback。0.3.3 source commit `a03c41f1c22aea50c8ad778e0c280bab538c149c`已push；final immutable Renderer／Interactive.Client SHA-256分別為`1C7AFF43870BD63F09155646D8FB116685211FF9EDEB8972EADFF44DBA3DC2A1`／`8EB30D4DF0620F4828E567BD116E6E08F5B20EF0E37FBBDF87B74CD4F7680654`，RepositoryCommit／ProductVersion exact。Fresh final package cache下Renderer118/118、Interactive20/20、verifierPASS；BrowserDemo exact0.3.3 Release僅既有WS9002／0 errors，min bundle SHA-256=`308D4D7F714541913DC714F859C83AD2C50D89EBF5568A2DA8823EFA251DEC80`。Daedilus同一真B仍pending。

## 2026-10-27 renderer-published persistent gesture frame（DYN-WBS-614）

`renderWithProjectionCommitAndDisplayTimeZone`建立單一`navigatorGestureFrame : Var<option<RuntimeState * TaWorkspaceDocument * TaPreparedRendererData * TaLoadedCoverageProjection option>>`。既有navigator `ShellPreparedFrame.View` mapper以outer shell document解projection後，在建立DOM前發布完整tuple；pointerdown callback只在event invocation時match `navigatorGestureFrame.Value`並把同一tuple傳給`startNavigatorDrag`。Var位於renderer instance scope，不隨document branch／Doc diff重建；沒有多個`View.Sink`競寫，也不要求WebSharper替換既存listener。

DYN-T-728鎖stable Var、DOM mapper publication與callback invocation read；0.3.1舊碼0/1 RED。修後DYN-T-726／727／728各1/1。Final source `70f2d1a0b901f09360e71ecda34ea4535d07f22b`；exact 0.3.2 package-backed Renderer116/116、Interactive20/20、package verifier PASS，package-only BrowserDemo exact chain僅既有WS9002／0 errors。Immutable package metadata與DLL ProductVersion皆對應source commit；Daedilus同一真B仍是後續gate。

## 2026-10-27 navigator captures the exact rendered document/projection（DYN-WBS-613）

`viewportControls`的`ShellPreparedFrame.View` mapper已持有四個實際render inputs：`renderedState`、outer shell `document`、`currentPreparedData`、`currentCoverageProjection`。`startNavigatorDrag`簽名擴充為顯式接收四者；local timeline以captured document＋prepared計算，`loadedCoverageSnapshot`直接map captured projection並以rendered state建立commit key，不再執行`tryLoadedCoverageResolved renderedState.Document.DefaultView prepared.RawData`。

DYN-T-727鎖actual production call與handler body：0.3.0 source 0/1 RED；修後DYN-T-726與727各1/1、Renderer full115/115、Interactive20/20、package verifier PASS、package-only BrowserDemo exact 0.3.1 chain 0 warnings／0 errors。Renderer／Interactive.Client與兩個direct test refs同步升0.3.1；final immutable package及Daedilus同序列local0仍是後續gate。

## 2026-10-27 navigator captures atomic rendered shell frame（DYN-WBS-612）

`shellPreparedData: Var<TaPreparedRendererData>`改為`ShellPreparedFrame: Var<RuntimeState * TaPreparedRendererData>`；full initial shell、topology commit、row refresh與full/incremental accept都以同一candidate/current state和prepared data成對publication。`viewportControls`只投影pair的prepared half；inner navigator的`View.Map`同時取得`renderedState,currentPreparedData`，其DOM handler closure把兩者顯式傳給`startNavigatorDrag`。該函式不再讀`chartCommittedFrame.Value`，loaded projection與timeline仍只讀captured prepared RawData，document/key讀captured state。

DYN-T-726要求atomic shell Var、render closure與四參數handler wiring存在，並禁止pointerdown內再讀chart frame；0.2.59 source為0/1 RED，修後focused1/1、Renderer full114/114、source Release 0 errors。依stable rollover政策，Renderer／Interactive.Client由0.2.59升0.3.0；Contracts維持0.1.41/schema v2，consumer與wire不變。

## 2026-10-27 navigator projection reads committed prepared data（DYN-WBS-611）

`startNavigatorDrag`已凍結`let dragState, dragPreparedData = chartCommittedFrame.Value`。`loadedCoverageSnapshot`保留document與`ProjectionCommitGate.key dragState`，但projection decode改為`tryLoadedCoverageResolved document.DefaultView dragPreparedData.RawData`，與render path的`preparedDataForShell.RawData`／`currentPreparedData.RawData`一致；timeline仍取同一`dragPreparedData`。不讀`runtimeState.Value`或`latestPreparedData`，故不引入跨generation混配。

DYN-T-725在既有P0 source-contract gate同時要求prepared expression存在且`dragState.Data` expression不存在；0.2.58為112/113 RED。Renderer／Interactive升0.2.59，Contracts仍0.1.41/schema v2；wire、queue、consumer、count classifier及DYN-WBS-610 unconditional purpose builder不變。

## 2026-10-27 navigator event-range origin survives axis settlement（DYN-WBS-610）

RendererModel新增`tryNavigatorEventRangeIntent projection targetWindow`，無條件以既有`tryEventRangeIntent`建立`Purpose=NavigatorWindow`、no-ordinal/no-direction `ExplicitBounds` intent；舊`tryCrossAxisNavigatorEventRangeIntent`保留相容surface及predicate，只委派新builder。`Renderer.setNavigatorWindowFromPrepared`改用unconditional helper，因該函式本身就是navigator-origin boundary；range仍由pointerdown frozen prepared data、projection與`navigatorVisibleEventRangePrepared`取得，main plot／resident ordinal／boundary producers不變。

DYN-T-724以accepted same-axis projection鎖定purpose、range authority、absence of ordinal/direction及exact count；DYN-T-717原cross-axis與old-helper same-axis compatibility assertions保留。Renderer／Interactive.Client升0.2.58，Contracts維持0.1.41／schema v2；Daedilus以resize→middle move→long body drag local0及A/C regression作真E2E gate。

## 2026-10-27 explicit coverage-window purpose（DYN-WBS-609）

Contracts新增qualified DU `TaCoverageWindowPurpose = MainPlotPan | NavigatorWindow`，並把required `Purpose`加入`TaCoverageWindowIntent`；required wire改變使`WindowIntentSchema`由v1升v2。`TaLoadedCoverageCodec.tryWindowIntent`／resident relocation的generic default是NavigatorWindow；`DynamicActionValidation`拒絕以MainPlotPan purpose送`ResidentCoverageChanged`。BrowserRuntimeCodec沿用typed JSON，Contracts test直接驗purpose round-trip，不新增第二套DTO或optional compatibility fallback。

RendererModel以`tryEventRangeIntent purpose`集中no-ordinal exact-event-range建構；`tryMainPlotEventRangeIntent`固定MainPlotPan，`tryCrossAxisNavigatorEventRangeIntent`固定NavigatorWindow。main boundary ordinal intent與provider-open fallback也在main producer boundary覆寫MainPlotPan；StartOrdinal／Direction／RangeAuthority語意不變。版本鏈為Contracts0.1.41 → Renderer0.2.57 → Interactive.Client0.2.57；active Tagging.Client同步Contracts0.1.41並升0.1.12。Daedilus consumer必須total match Purpose：MainPlotPan保visible count並relocate together；NavigatorWindow只更新fixed resident內active span。

## 2026-10-26 axis-bound navigator domain與main-plot mode（DYN-WBS-598）

`tryResidentNavigatorWindow`要求`GlobalOverview.AxisRef = ActiveDetail.BaseAxisRef`才走ordinal mapping。0.2.41真remote settlement證明accepted GLOBAL target也不能建立cross-axis `TaResidentNavigatorAuthority`：normalized 1K ordinal與30K rendered slots不在同一domain。0.2.42保留相容型別/函式surface但builder永遠回`None`、legacy retained value永不match；`tryResidentNavigatorWindowWithAuthority`只委派same-axis resolver。`resolveNavigatorGestureDomain`因此在cross-axis固定使用clamped active-local domain，render/draft亦用active timeline ratios；release以`setWindow`送event-time `VisibleRangeChanged(CoverageIntent=None)`，不合成ordinal。GLOBAL navigator／resident strip維持原typed resident intent；0.2.43起main-plot CURSOR改走formal-base event range及可區分的Some/no-ordinal typed intent，不再使用GLOBAL resident intent。

`TaMainPlotInteractionMode = Cursor | Resize`由workspace root `Var`持有，因此row reactive replacement不重置。每個plot surface以dynamic attribute/style公開mode，`contextmenu`阻止browser menu後切換並清cursor。Cursor mode才處理shared vertical/horizontal cursor與point activation；pointer-down凍結`TaWorkspaceDocument`、`TaPreparedRendererData`、`TaLoadedCoverageProjection`與committed window，`mainPlotAnchoredPanDelta`取得離散slot差，`tryMainPlotPanWindow`只接受captured formal-base範圍內的target。`tryMainPlotEventRangeIntent`產生`ExpectedCoverageRevision=Some capturedRevision`、`QueryGeneration+1`、`StartObservationOrdinal=None`、`ObservationCount=target.Count`、`Direction=None`、`RangeAuthority=ExplicitBounds`；release經`setWindowFromPrepared`與`visibleEventRangePrepared`送唯一`VisibleRangeChanged`。document identity漂移、prepared未ready、projection缺失或target越界皆fail closed。Resize沿既有`setWindow`並保持intent None；兩個release branch分離，main CURSOR不得呼叫GLOBAL sampled-anchor mapping或`ResidentCoverageChanged`。

Interactive.Client的`publishRuntimeState`仍以`authoritativeGlobalOverviewReady`作唯一mount gate。DYN-T-709把該bounded body固定為不得出現materialization Completed或rows非空條件，讓Research可在RUN早期先發布formal GLOBAL/4500，derived rows後到而不造成第二套GLOBAL lifecycle。

## 2026-10-25 0.2.39 retained resident navigator authority（DYN-WBS-597）

`TaResidentNavigatorAuthority`保存CoverageIdentity、GLOBAL resident start/count、`OverviewAnchors.Length` navigation domain及accepted target相對resident start的active-detail base offset。它只由`coverageIntentProjectionSettlement=Reanchor`且GLOBAL／active axis不同時建立；identity change與viewport scope change清除，projection scope不再相符時各resolver fail closed。

`tryResidentNavigatorWindowWithAuthority`先走既有same-axis absolute mapping，只有該mapping失敗才把active-local window加到retained base offset；`coverageNavigatorWindowWithAuthority`、`navigatorSelectionRatiosForRenderWithAuthority`、drag pointerdown與queued semantic dispatch共用此resolver。terminal grow後base0/domain1484仍畫`0..862`；右移148的accepted target把base更新為148，下一輪不得fallback成active start600240+862=`601102`。舊無GLOBAL與same-axis路徑、typed action shape、coverage generation／revision gate都不變。

## 2026-10-25 0.2.36 resident grab-pan／discrete anchor seam（DYN-WBS-596）

`RendererModel.mainPlotAnchoredPanDelta`用既有`cursorIndexFromClientX`分別解析 down/current slot，再回傳`downSlot-currentSlot`；不再用連續距離 round。`tryMainPlotResidentRelocationIntent`以 signed delta 移動固定寬 resident，沿用 coverage revision/query generation guard。Renderer 的 main-plot release 在 validated GLOBAL 且允許`resident-coverage-changed`時送`ResidentCoverageChanged`並保持 inner viewport resident-relative offset；pending draft維持視覺位置，authoritative projection settle時一起清除。無 GLOBAL／未授權該action時仍走既有 local/adjacent pan，不改 toolbar與navigator語意。

每個 row SVG 有一條`data-ta-row-horizontal-cursor='true'`的`pointer-events:none` line。row mousemove將 client Y換算為該SVG viewBox Y並只顯示自身 line；mouseleave隱藏。主plot宣告`touch-action:none; user-select:none; cursor:crosshair`，gesture down阻止browser default/冒泡，但仍以document listeners持有既有正常的pointer lifecycle。focused browser gate以真Mouse驗resident／absolute visible同delta、inner offset不變、exact anchor與horizontal cursor row isolation。

## 2026-10-25 0.2.31–0.2.33 exact viewport／axis／lifecycle seams（DYN-WBS-594 / 595）

`RendererModel.tryMainPlotPanCoverageIntent`由 committed active start、local window及 signed drag delta產生 exact `TaCoverageWindowIntent`；`ObservationCount`不變，只有實際 start 改變才送 direction。`tryCoverageIntentReanchorWindow`驗 returned active interval containment，成功時回 `{ StartIndex = requestedStart - returnedActiveStart; Count = requestedCount }`，避免 resident 1500→3000／4500後 viewport落在 index 0；不完整回傳不會 settle pending pan。既有`tryAdjacentCoverageIntent`只留給整頁相鄰導航。

`yAxisTickValues 5 low high`產生 top-to-bottom 共五值；row view以 `68px / chart / 68px` grid包住原SVG。DOM seam為`ta-y-axis-{rowId}-left|right`、`data-ta-y-axis-side`、`data-ta-y-axis-tick-index/value`，兩側來自同一 reactive marker geometry，gutter `pointer-events:none`。

`RendererModel.authoritativeGlobalOverviewReady requiresGlobalOverview state`集中 lifecycle gate。Renderer bootstrap overload與Interactive.Client mount path都呼叫它：有loader時需 resolved validated `GlobalOverview`，無loader時維持 document-ready行為。切換仍monotonic，bootstrap不取得HTTP／IDB／action／projection authority。Research以既有`GlobalOverview`欄位供應獨立1K total/resident並自行把global gesture映回active scale；0.2.31沒有新增或猜測cross-axis wire mapping。

0.2.33把Client handoff改為`globalOverviewBootstrapHost`與`globalOverviewAuthoritativeHost`兩個一次掛載的`display:contents`容器。authoritative renderer只在後者`Doc.Run`；`retireGlobalOverviewBootstrapWhenVisible`以RAF查詢`ta-global-overview-with-axis`並要求`GetBoundingClientRect()`寬高非零，之後才對bootstrap host執行`Doc.Run ... Doc.Empty`及移除host。這使DOM mutation ordering保持「先有新visible GLOBAL，再移除最後一個舊GLOBAL」；final authority、projection root、loader ownership及無loader `Doc.RunById`路徑不變。

## 2026-10-24 typed actual GLOBAL bootstrap（DYN-WBS-593）

`RendererModel.tryGlobalOverviewBootstrap`驗`TotalObservationCount`、axis、anchor上限/範圍/ordinal與time嚴格遞增、resident邊界。`renderGlobalOverviewBootstrapWithDisplayTimeZone`接受consumer loader，立即啟動Async，成功且合法才把overview放入local `Var<option<_>>`；presentation使用既有overview points、resident ratios及GLOBAL calendar axis，root標`data-global-overview-source=bootstrap`／`phase-1-actual`／`data-bootstrap-authoritative=false`且`pointer-events:none`。

`renderWithProjectionCommitAndDisplayTimeZoneAndGlobalOverviewBootstrap`預先建立bootstrap與既有authoritative Docs，以monotonic `authoritativeReady`監看`RuntimeState.Document.IsSome`；第一次true後永久原位切換authoritative Doc，之後loader完成也不能回寫畫面。Interactive.Client另提供`createApplicationWithDisplayTimeZoneAndGlobalOverviewBootstrap`與對應Application start overload；它在transport start前掛bootstrap，既有第一次authoritative `Doc.RunById`自然取代。不含DOM event、HTTP、IndexedDB、Research vocabulary或假runtime。

## 2026-10-24 0.2.29 interaction contract（DYN-WBS-592）

主圖 pointerdown 快照 committed visible window 與 SVG bounds；document-level pointermove 以 `mainPlotPanDelta` 將 CSS displacement 換成 bar delta、RAF coalesce `draftWindow`，pointerup 超過4px才 suppress 隨後 click 並委派既有 `panWindow`，pointercancel／identity change 清掉 listener/draft。未過 threshold 的 pointer sequence仍走原 click cursor與activation。GLOBAL-first phase把當前 runtime raw data先放入空 axes/series的shell，使 `GlobalOverview.Anchors`／resident selection能先渲染；row docs、cursor panel及 RESIDENT overview在相同 full-preparation candidate通過 generation/runtime/projection gates後下一個frame才公開。

`RuntimeBodyTiming`在 body enqueue／pump start建立 correlation，runtime publish按 `LastTransportSequence`暫存；projection commit寫 body→publish／commit，double RAF 寫 commit→paint與 body→paint後移除 bounded timing。Renderer另寫 body→pending、pending→commit；原 `data-frame-pump-*`與`data-renderer-prepare-*`保留，用同一 DOM snapshot判斷45s/35s屬 transport/backend、queue、scheduler或paint。`TaPointActivationReceiptV2`包含 clicked value/scale與同 timestamp series array；candlestick value為OHLCV object，scalar traces為Number，v2/v1各自以獨立 bubbling CustomEvent 發送。

## 2026-10-24 GLOBAL calendar axis（DYN-WBS-591）

`RendererModel.globalCalendarTicks`只接受`TaOverviewAnchor[]`、viewport寬與最小spacing；按source順序辨識年度及季度月份，不生成日期、不補缺月，輸出保留`AnchorIndex`、原始`EventTimeUtc`、`IsYearBoundary`與numeric `Lane`。year與quarter分組後，各自依anchor換算的pixel位置使用first-fit greedy packing：放入第一個與該lane末筆相距至少24px的lane，無可用lane即新增，因此不以viewport擁擠為由drop mandatory candidate；quarter lanes從year lane count之後起算，同一anchor兩類必定分lane。輸出最後依anchor與year-first排序。`globalCalendarAxisWithPalette`以原anchor index比例定位，axis高度為`laneCount * 15 + 2`，DOM保留`data-canonical-event-time`、numeric `data-global-calendar-tick-lane`及`data-global-calendar-lane-count`；只有`ta-global-overview-time-axis`改用此helper，一般／RESIDENT維持原generic formatter與display-time行為。

## 2026-10-24 TA datapoint activation（DYN-WBS-589）

`TaPointHitCandidate`描述row/trace/data-ref/event-time/trace-kind/component及rendered x/y range；`RendererModel.tryNearestPointHit`以x距離加點／range的y距離做deterministic nearest selection。Renderer保留reactive最新prepared geometry：candle使用slot center與high/low normalize；line使用point y；histogram/volume使用zero-to-value range。click把browser座標映射到SVG viewBox，仍執行原cursor commit，再嘗試browser-local activation。

`TaPointActivation.tryCreate`只在`ProjectionCommitGate.LastCommitted = key state`時建立`ta-point-activation.v1` receipt；payload包含完整runtime/coverage key與stable point identity，不含visible index。`ptcs-dynamic-ta-point-activated-v1`由標記`data-ptcs-ta-point-surface=true`的clicked SVG dispatch並bubble；舊public render helpers以no-op callback wrapper保持source surface，`SharedCursorChanged`與wire不變。

## 2026-10-23 resident gesture交接（DYN-WBS-587 / 588）

DYN-T-685最小設計：只調整previewWindowBounds的right分支；先沿用minimumEnd/nextEnd限制，再以min(maximumCount,total) cap width，start=nextEnd-width，最後走既有clampWindow。不改left/Move、pointer routing、boundary判斷或action queue。測試用獨立區間literal加domain reflection，舊0.2.23先RED；package/restart等consumer latency基線。效能觀測沿既有FramePump stage callback與Research projection receipt，wall time須另量，callback CPU總和不包含RAF queue wait。

0.2.24：RendererModel.createBudgetedWorkScheduler注入requestTurn/clock，FIFO重入只維持一個pending turn，4ms或64 tasks即yield；exception清掉該batch待執行work並重拋，不執行殘餘工作。FramePump.scheduleBrowserStages包既有stage callback，四種public browser reduce wrappers與WS入口共享；Renderer full/incremental prepare每次generation建立scheduler並逐task拒舊generation。純模型準備算法、chunk驗證與最終commit paint barrier不改。typed metrics由既有renderer helper寫documentElement，sequence/generation隔離舊batch，建立時清零；queue/wall與work時間分開，caller必須把多batch及HTTP另外關聯。

0.2.44：first-paint telemetry把server／action-frame與Renderer owner phase分開。真3M warm run中RUN response約5.96s，`/api/workspace/actions` response約11.47s（HTTP/body約4.52s、30,926,931 chars），action frame約3.36s；這些不由Renderer排程修正。Renderer最後generation的100個prepare work原以4ms budget分34–37個RAF turns，wall 1.08–1.23s中只有0.28–0.33s為work、0.80–0.90s為queue。blank initial full preparation改用16ms／64 tasks bounded turn；incremental update與coverage replacement仍用4ms／64 tasks，保留每turn yield、FIFO、generation cancellation、atomic preparation與final commit barrier。`preparationWorkBudget`是唯一policy seam，避免把初始吞吐policy擴散到scheduler implementation。

0.2.23新增既有RendererModel內的`navigationDomainCount`：GlobalOverview存在時取其TotalObservationCount，否則委派Codec.observationDomainCount。resident/legacy adjacent intent、queued absolute validation、edge/view-all與followLatest、Client adjacent cache的known-empty判斷共用同一導航authority；resident selection geometry仍只用resident count，資料payload未擴大。最後pointer修復保留，release helper的歷史bool參數顯式定型維持API。

0.2.22：element與document pointerup均將event交同一finish；move/up共用pointerDraft，up在finished前擷取最後座標。release優先pending draft並同步frozen ratio/draft後dispatch；舊排定RAF因finished而不再寫回。cancel契約不變。chart的`data-viewport-dispatch-*`只記最後一次coverage dispatch前的target/count/pending action/pending boundary/committed-ready/poll，供右端無action問題判斷，無新增輪詢或wire。

沿用`TaQueuedViewportIntent`、`TaPendingBoundaryPan`、`ProjectionCommitGate`：pointerdown快照committed state/data；drop保留draft；同identity/resident允許revision前進，queued absolute ordinal在matching coverage frame完成後用當前projection重建typed intent。action response與projection commit共用settlement gate，active gesture或queue仍存在時不得清draft。resident外拉另以pointer是否越出track判定，Earlier=`residentStart-pageCount`、Later=`residentEnd-visibleCount+pageCount`，再按全域邊界clamp。拒絕identity變更、resident移動/resize、舊revision/generation；不新增timer或wire欄位。對應RFC-0036、Test DYN-T-683/684。

## RF-12（20261006）：局部修改

`createActorsPageDocument`: root追加`contain:inline-size;width:100%`→外層grid依可用寬分配；report controls改flex-wrap，input flex-basis260/min-width0；統計卡minmax下限不超父容器。內層actor tree/grid保留水平scroll與完整identity。五行style修改，只有umbrella Dynamic0.1.32→0.1.33需進版；Ptcs/Client adapters未依賴umbrella，維持版本。Main SPAHost與GW.Tests的exact reference及asset version同步0.1.33；Renderer.fs WIP保持。正式封包從committed source fullWebSharper生成，禁手改JS。真瀏覽器寬/窄/長path geometry、controls可見、focus及截圖作驗收。

## 1. 專案結構設計
```text
C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic
├── doc
│   ├── REQ.md, SA.md, SD.md, WBS.md, TEST.md, UPSTREAM_RFC.md
├── src
│   ├── PulseTrade.Comm.Spa.Dynamic.fsproj
│   ├── Server
│   │   ├── FCell2Interop.fs   (負責 fCell AST 解析與轉換)
│   │   ├── DynamicActors.fs   (包含 Actor Dynamic 的後端邏輯)
│   │   └── Extension.fs       (包含 CommHub 擴充與掛載介面)
│   └── Client
│       ├── DynamicRenderer.fs (WebSharper 渲染器與 DOM 生成邏輯)
│       └── ActorDynamicTab.fs (處理 "Actor Dynamic" Tab Page 類型的頁面渲染)
└── tests
    ├── PulseTrade.Comm.Spa.Dynamic.Tests.fsproj
    └── Program.fs
```

## 2. API 介面與元件設計 (Interface & Component Specification)

### 2.1 "Actor Dynamic" Tab Page 掛載設計 (Client-Side)
在現有的 PTCS 中，頁面型態可能包含 Chat、Sets 等。為了支援 `Actor Dynamic` 的展示頁面（一個概念展示用的 Tab Page，內部會呈現 SDUI 元件的互動），我們需要定義專屬的 `Tab Page Type` 處理器。

**Code Snippet: 前端 Tab 頁面與 UI 綁定**
```fsharp
namespace PulseTrade.Comm.Spa.Dynamic.Client

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Html
// 假設上游提供了註冊 Tab 型態的介面 (此部分亦可能需要 UPSTREAM_RFC 支援)
open PulseTrade.Comm.Spa.Client

[<JavaScript>]
module ActorDynamicTab =
    
    /// 渲染 Actor Dynamic 專屬的 Tab 頁面內容
    let renderActorDynamicPage (pageId: string) =
        div [ attr.``class`` "actor-dynamic-container" ] [
            h2 [] [ text "Actor Dynamic 展示頁面" ]
            div [ attr.``class`` "sdui-canvas-area" ] [
                // 這裡將放置 FSkynet CanvasComponent + GridFeatures 
                // 以及擴充的 App Loader, Color Picker 等元件
                text "動態元件載入中..."
            ]
        ]

    /// 提供一個註冊點給宿主
    let Start () =
        // 註冊客製化 Renderer 攔截 fskynet-sdui 訊息
        PulseTrade.Comm.Spa.Client.RegisterRenderer(DynamicSduiRenderer.create())
        
        // 註冊 Actor Dynamic 的 Tab Page 型態
        PulseTrade.Comm.Spa.Client.RegisterTabPageHandler("actor-dynamic", renderActorDynamicPage)
```

### 2.2 後端擴充點 (Server Extension) 與 Actor 註冊
```fsharp
namespace PulseTrade.Comm.Spa.Dynamic.Server

open PulseTrade.Comm.Spa
open Akka.Actor

[<AutoOpen>]
module CommHubExtensions =
    type CommHub with
        /// 將 Dynamic Sdui Actor 與路由掛載至現有的 CommHub
        member this.useDynamicSdui(actorSystem: ActorSystem) =
            // 1. 註冊 "Actor Dynamic" 展示用的後端 Actor
            let props = Props.Create(fun () -> new ShowcaseDemoActor())
            let showcaseActorRef = actorSystem.ActorOf(props, "showcase-dynamic-actor")
            
            // 2. 將 Actor 與 PTCS 的路由或 CommHub 做綁定
            // (假設 CommHub 有公開的 RegisterActor 介面)
            // this.RegisterActor("actor-dynamic", showcaseActorRef)
            
            this
```

## 3. 類別庫封裝與相依 (NuGet Packaging)
- Target Framework: `net10.0`
- 目前 `src/PulseTrade.Comm.Spa.Dynamic.fsproj` 以 exact `PackageReference Include="PulseTrade.Comm.Spa" Version="[0.2.5-beta58]"` 消費 PTCS，不再使用 local `ProjectReference`。本輪 package 化先使用自己編譯並 local deploy 到 SDK `FSharp\library-packs` 的 PTCS / PTCS.Dynamic；NuGet.org push 需等 operator-provided key/path，禁止在 repo/log 中寫入 secret。
- 透過 WebSharper 將 `Client/*.fs` 翻譯為前端 JS，並保證 `ActorDynamicTab.Start()` 能在 PTCS 核心啟動時正確呼叫。

## 4. RFC-PTCS-DYNAMIC-0002 Dynamic Argu Form Design

Formal RFC: `doc/RFC-PTCS-DYNAMIC-0002.dynamic-argu-form-runtime.md`

### 4.1 Server metadata and schema generator

新增 server-side metadata layer：

```fsharp
type ArguFormFieldKind =
    | Text
    | Integer
    | Decimal
    | Boolean
    | Enum of string list
    | Date
    | Time
    | Color

type ArguFormFieldMetadata =
    { FieldName: string
      ArguParam: string
      Kind: ArguFormFieldKind
      Required: bool
      DefaultValue: string option
      Placeholder: string option }

type ArguUnionCaseMetadata =
    { DuTypeName: string
      UnionCaseName: string
      DisplayName: string option
      Fields: ArguFormFieldMetadata list }
```

`ArguFormSchemaGenerator.generateSduiJson` 將 `ArguUnionCaseMetadata` 轉為：

```text
{ schema = "fskynet-sdui"; formMode = "argu-form"; sdui = [...] }
```

Reflection 只能作為 allowlisted metadata registry 的 producer；browser-supplied type name 不可直接 unrestricted resolve。

### 4.2 Browser SubmitArguForm

`DynamicRenderer` 需為 `formMode = "argu-form"` 建立 scoped form state。Button action `SubmitArguForm` 的流程：

```text
includeStateOf ids
  -> collect field value + arguParam
  -> ArguFormCommandLine.encode
  -> submitFn rawArgu
```

`ArguFormCommandLine.encode` 必須集中測試 whitespace / quote escaping；輸出字串只作 PTCS ActorArgu payload，不作 shell command。

### 4.3 PTCS seam integration

當 PTCS `RFC-PTC-SPA-0007` seam 可用後，Dynamic browser bundle registers：

- append input renderer：依 page shape + selected key 判斷 `actor-dynamic` / Dynamic Argu key；
- add-key dialog renderer：新 canonical 回傳 `actorAddress :: duTypeOrTemplateKey :: canonicalArgString`；舊 `actorAddress :: duTypeName :: unionCaseNames` 只作 migration / historical reference；
- message renderer：保留既有 `fskynet-sdui` rendering。

若 seam 尚未存在或 renderer 失敗，Dynamic 必須讓 PTCS fallback 到既有 textarea/raw key path。

### 4.4 RN proxy integration

Dynamic 不 reference RN package。若 key 的 actor address 指向 RN DurableProxy，PTCS core 仍只送出 `ActorArguTargetCommand.RawArgu`；RN side 由 `CommandToCell` / `InvokeLegacy` / `LegacyReplyToCell` 處理 legacy actor adaptation、delivery、confirm 與 result completion。

## 5. RFC-PTCS-DYNAMIC-0003 Unified SDUI / Form DSL Design

Formal RFC: `doc/RFC-PTCS-DYNAMIC-0003.unified-sdui-form-dsl-roadmap.md`

### 5.1 Canonical DSL types

```fsharp
type SduiRenderSurface =
    | Canvas
    | FormInput

type SduiOptionSource =
    | StaticOptions of string list
    | QueryOptions of providerId: string * dependsOn: string list
    | StreamOptions of streamId: string * dependsOn: string list

type SduiTreeConnector =
    | Orthogonal

type SduiTreeToggle =
    | BoxedPlusMinus

type SduiTreeBinding =
    { DataRef: string
      RootNodeIds: string list
      NodeIdField: string
      ParentIdField: string
      LabelField: string
      StatusField: string
      Columns: string list
      Connector: SduiTreeConnector
      Toggle: SduiTreeToggle }

type SduiNode =
    | Stack of id: string * children: SduiNode list
    | Section of id: string * title: string option * children: SduiNode list
    | TextBlock of id: string * text: string
    | Input of id: string * label: string * kind: string * binding: string
    | Select of id: string * label: string * options: SduiOptionSource * binding: string
    | Button of id: string * label: string * actionId: string
    | Tree of id: string * binding: SduiTreeBinding * onNodeClickActionId: string option

type SduiDocument =
    { Schema: string
      Version: string
      DocumentId: string
      Surface: SduiRenderSurface
      Nodes: SduiNode list
      Actions: Map<string, string>
      Bindings: Map<string, string> }
```

Implementation may refine union names, but the separation is mandatory：renderer consumes `SduiDocument`; adapter consumes Argu / DU metadata。

Canvas `Tree` is the required renderer target for PTC ActorTree integration. The upstream `ActorTreeDocument` is produced by PTCS/PTC Actor Registry projection and then converted into `SduiDocument Surface=Canvas` with one `Tree` node. Dynamic does not persist actor registry data, does not rebuild PCSL projection, and does not write actor state reports. If Dynamic is missing or the Tree renderer fails, PTCS core must keep using its fallback table with `parentId`。

### 5.2 Target resolver

```fsharp
type DynamicTarget =
    | DirectDslTarget of actorAddress: string * formDslId: string
    | ArguTemplateTarget of actorAddress: string * templateKey: string * canonicalArgString: string

module DynamicTargetKey =
    val tryParse : string list -> Result<DynamicTarget, string>
```

Parse rules：

1. key list length must be at least 2；
2. first item is actor address；
3. `[ actor; formDslId ]` resolves only when second item matches Form DSL registry；
4. `[ actor; templateKey; canonicalArgString ]` resolves only when second item matches Argu adapter registry；
5. canonical arg string is parsed server-side with the registered Argu parser before DSL generation；
6. unknown second item or parse failure returns controlled error。

### 5.3 Argu-to-FormDsl adapter

```fsharp
type ArguTemplateRegistration =
    { DuTypeName: string
      TemplateKey: string
      TemplateType: Type
      Aliases: DynamicArguAliasBinding
      DefaultArgString: string option }

type DynamicArguAliasBinding =
    { CaseAliases: Map<string, string>
      FieldAliases: Map<string * string, string>
      OptionAliases: Map<string * string, string> }

type ParsedArguValue =
    { FieldName: string
      Values: string list }

type ParsedArguCase =
    { CaseName: string
      Values: ParsedArguValue list }

type ParsedArguSubcommand =
    { CommandToken: string
      TemplateType: Type
      Cases: ParsedArguCase list }

type ParsedArguTarget =
    { ActorAddress: string
      TemplateKey: string
      CanonicalArgString: string
      RootCases: ParsedArguCase list
      TailSubcommands: ParsedArguSubcommand list }

module ArguToFormDsl =
    val parseTarget : ArguTemplateRegistration -> canonicalArgString: string -> Result<ParsedArguTarget, string>
    val generate : ArguTemplateRegistration -> ParsedArguTarget -> SduiDocument
```

Each parsed root case and supported subcommand becomes a visible form section. The adapter maps:

- string -> text input；
- numeric -> number input；
- bool flag -> checkbox；
- enum / zero-field DU enum -> select；
- tuple -> ordered input group；
- list -> repeatable input group；
- nested `ParseResults<'T>` / Argu `ArgumentType.SubCommand` -> tail subcommand section when supported, otherwise controlled unsupported-field error。

Alias mapping is applied during DSL generation only:

```text
canonical case/field/option name
  -> alias lookup from DynamicArguAliasBinding
  -> label/title in SduiDocument
```

Submit/raw command building always uses canonical Argu names. Alias text must not enter `ActorArguTargetCommand.RawArgu`.

Composite raw command builder rules:

```text
root cases in token/configured order
  -> tail subcommand token, e.g. datarange
  -> subcommand args
```

Expected PFCF data-range example:

```text
--pfcfedx trivial --pfcfgtcconf OIInf TAIFEX FillSquareCombine OrderByTXDT CathayBKTaifexFill --to 90000 --parentchilds 2 5 --bba F008 000 9910357 --decimalquote 6 0 --round 6 4 2 datarange --referencedatemode ModeAccountingDate --between 20251104 20251104 --calibrate2curdayiflargerthancurday
```

### 5.4 Backend-linked options

`QueryOptions(providerId, dependsOn)` is a declared provider lookup。Renderer may call the PTCS core safe extension query callback only for registered providers。The DSL must not contain arbitrary URL, headers, tokens, script text or executable code。

### 5.5 PTCS.Host demo integration

PTCS.Host registers:

1. a direct form DSL target derived from `example DU.txt`；
2. an Argu adapter target for a host-local `PFCF_AKKA_CMD` demo subset；
3. a durable proxy/echo actor target for E2E。

`example DU.txt` is cp950 encoded and contains Chinese identifiers/comments。The host demo should either preserve valid identifiers or map them to stable ASCII labels while keeping display labels in metadata。Missing external types such as `DataTypeT.RTTables` must be represented by host-local stubs or excluded from the first demo subset with a documented controlled unsupported-case message。

## 6. Actor Dynamic action mode design

`RFC-PTCS-DYNAMIC-0004` keeps Dynamic renderer logic mode-aware without requiring PTCS core to parse Dynamic target semantics.

### 6.1 Add-key renderer mode dispatch

```fsharp
let renderAddKey (ctx: obj) =
    let context = ctx |> As<AddKeyContextDto>
    match asText context.shape with
    | "actor-argu-target" -> renderArguTargetKey context
    | "actor-dynamic-target" -> renderDynamicTargetKey context
    | "actor-dynamic-proxy" -> renderDynamicProxyKey context
    | _ -> None
```

`renderArguTargetKey` requires explicit proxy and native target parts:

```text
proxyActorAddress
targetActorAddress
duTypeOrTemplateKey
canonicalArgString
```

Submit payload:

```fsharp
{ keys = [| proxyActorAddress; "target-v1"; targetActorAddress; duTypeOrTemplateKey; canonicalArgString |]
  displayName = displayName }
```

The first key segment remains the PTCS route actor. PTCS `ActorArguTargetCommand.TargetActorAddress` carries the native target actor address. Dynamic must not rely on `BeforeAddKey` / Host script hooks to create a per-target proxy and rewrite the persisted key.

`renderDynamicTargetKey` uses the same UI when DU/template is present. Direct actor key without DU/template is intentionally handled by PTCS core Add actor key fallback.

`renderDynamicProxyKey` requires:

```text
proxyActorAddress
rnActorAddress
targetKind
displayName optional
```

Submit payload:

```fsharp
{ keys = [| proxyActorAddress; "proxy-v1"; rnActorAddress; targetKind |]
  displayName = displayName }
```

### 6.2 Append input renderer mode dispatch

```fsharp
let renderAppendInput (ctx: obj) =
    let context = ctx |> As<AppendInputContextDto>
    let keys = normalizeDynamicTargetKeyParts context.keyParts
    match asText context.shape, keys |> Array.toList with
    | "actor-argu", proxy :: "target-v1" :: target :: template :: raw :: _ -> renderResolvedFormInput context template raw
    | "actor-argu", _ :: template :: raw :: _ -> renderResolvedFormInput context template raw
    | "actor-dynamic", _ :: "proxy-v1" :: _rnTarget :: _targetKind :: _ -> None
    | "actor-dynamic", _ :: template :: raw :: _ -> renderResolvedFormInput context template raw
    | "actor-dynamic", [ _actor ] -> None
    | _ -> None
```

Returning `None` for single-key Actor Dynamic is intentional: PTCS fallback textarea becomes arbitrary string / JSON DSL input, and message rendering later decides whether reply is canvas.

### 6.3 Canvas message renderer

```fsharp
match tryGetSchema payload with
| Some "fskynet-sdui" -> Some(createSduiCanvas payload)
| _ -> None
```

The renderer must not treat page type, key shape, or actor address as proof of canvas content.
## 2026-06-28 ActorsPage Renderer Design

### Module placement

Current first slice extends `Client/ActorDynamicTab.fs`:

```fsharp
let IsActorsPagePayload (rawContent: string) =
    rawContent.IndexOf("ActorTopologyPage") >= 0

let registerActorsPageRenderer () =
    // register string -> Dom.Node option through PulseTradeRegisterPageRenderer
```

Do not move this first slice to a new `[<JavaScript>]` client file until WebSharper compiler behavior is fixed or re-verified. Clean short-path builds showed:

- new client compile unit, even no-op, can crash `wsfsc.exe`;
- `String.Contains` in `[<JavaScript>]` code can crash `wsfsc.exe`;
- a single `IndexOf` predicate compiles.

### Runtime flow

```text
ActorDynamicTab.Main()
  -> _registerRenderer()              // generic Canvas message renderer
  -> registerActorsPageRenderer()     // page-level ActorsPage renderer
  -> ArguFormRenderer.Register()

PTCS /actors
  -> builds ActorsPage / ActorTopologyPage DSL
  -> calls registered page renderers
  -> Dynamic returns Some Dom.Node for ActorTopologyPage
  -> PTCS mounts only Dynamic page host
```

### First-slice output

`createActorsPageDocument` renders a page-level Dynamic Actors UI, not the generic `FSkynet 動態畫布 (Canvas)` summary card. The current output includes:

- action shell for reload / report / schedule report, with report actions still disabled until PTCS report wiring is ready;
- count cards for renderer identity, node groups, actor tree rows, and active rows;
- node blocks derived from actor-system host/port in `actorTreeNodes` address data;
- role ordering: PTCS Host -> GW Host -> RN Host -> Unknown;
- hierarchy rows with full labels, active/degraded status, and boxed `+` / `-` toggles;
- grid rows with full actor addresses and path/status metadata.

The Playwright gate now verifies page ownership, clean host/port grouping, role ordering, and real collapse/expand behavior. Tree toggles are backed by WebSharper `Var` state: collapse removes child rows from the rendered tree and updates `aria-expanded`.

### Next design gates

1. Replace token classifier with strict DSL codec once WebSharper-safe parsing is available.
2. Replace the current renderer-side inferred node grouping with explicit `nodeGroups` codec once PTCS emits it.
3. Polish actor hierarchy tree connector geometry and card/action layout.
4. Add grid/cards/actions from the same ActorsPage DSL document.
5. Replace the current source-host Playwright proof with reusable F# verifier coverage through PTCS `/actors`, including Dynamic accepted, Dynamic absent, and unsupported renderer fallback paths.

## RFC-PTCS-DYNAMIC-0008 Reply Presentation Adapter Design

### Bounded envelope decoder

`TaResearchReplyPresentation.runtimeFrames`改為明確pipeline：

```text
payload
  -> trim / size-depth guard
  -> unwrap JSON string at bounded depth
  -> inspect canonical fCell2 envelope or Case/Fields DU
  -> select inbound/replied cells only
  -> strict RuntimeFrameCodec decode
  -> validate protocol/document/canvas identity
```

decoder不得用target alias、page title或substring猜測。成功回`RuntimeFrame array`；真正NonSdui回`None`；present-invalid回bounded error model。raw points只留在runtime data，不進summary DOM/text。

### Summary projection

`summaryFromFrames`只投影instrument、interval/scale、requested range/coverage、ordered rows與trace parameters、freshness/watermark/quality。summary Node不得序列化完整frame、series或point array；diagnostic只保留stable reason code與bounded message。

### Lazy runtime handle

`tryResolve`回傳`ReplyPresentation`時只建立summary closure與canvas identity，不呼叫`mountByIdWithOptions`。`MountInline`/`MountFullscreen`才建立或移轉唯一handle；disposer必須執行`SetActive false`與`Dispose`。Collapsed、reply removal、page unmount及disconnect後timer/socket/subscription均回baseline。

`TaClientLifecycle.ResyncRequired`在connected/active狀態即使`InFlight=true`也可執行：先`CancelPoll`與`CancelTimeout`，再送唯一`RequestFullSnapshot`並排新timeout。這是invalid production delta/reply的恢復路徑。

### Composer boundary

`ArguFormRenderer`不持有Plain/Form mode。它只在PTCS Form host被建立時render，submit仍回raw Argu string；Plain single textarea、mode switch、selected target與history由PTCS core owns。

## 2026-09-21 Generic Marker Overlay Design

完整決策見 `doc/RFC/RFC-PTCS-DYNAMIC-0015.generic-marker-overlay.md`。

```text
producer generic TaMarker[]
  -> TaMarkerCodec.encodeBucket
  -> TemporalSeriesPoint(Position, Value)
  -> RuntimeFrame v2
  -> frame shape validation
  -> apply whole frame to candidate
  -> MarkerValidation.validateCandidate(document, candidate)
  -> commit OR retain last-good + RequestResync/RejectFrame
  -> RendererModel resolves target candle by Position
  -> fixed row-local marker lanes + deterministic stack
```

Public seams：

- `TaMarkerCodec`：strict marker/bucket encode/decode，preserve tooltip與bucket order。
- `TaMarkerTraceOptionsCodec`：typed `marker.targetTraceId` options helper。
- `MarkerValidation`：document與candidate semantic validation；只掃marker refs及target candle refs。
- `RuntimeEffect.RejectFrame`：structured non-recoverable rejection，不觸發resync loop。
- `MarkerGeometry`：pure row-local layout，size 9px、first gap 4px、stack gap 2px、top/bottom lanes各44px。

Failure paths：sequence/base revision/axis gap/missing authoritative state走`PausedForResync + RequestResync`；malformed marker/duplicate id/illegal color/enum/target/hard limit走`Suspended + RejectFrame(Recoverable=false)`。兩者都保留last-good。Unknown v2 trace kind與v1 marker fail closed。Marker-only patch重用未變DataRef與prepared candle資料，不全量重decode。

Test seams：codec round-trip/strict shape；snapshot/patch/clear/move/idempotency；composite/split candle resolver；stack/lane/Y-domain/viewport；v1/v2/cache/unknown-kind compatibility。

### Marker v2 correction

`TaMarkerShape` current cases為`TriangleUp | TriangleDown | Circle | Square | Diamond`。`TaMarkerCodec`只encode `ta-marker.v2`；decode同時接受v2與legacy v1，v1 `arrow + above-bar`映射`TriangleDown`、`arrow + below-bar`映射`TriangleUp`，其餘舊shape同名映射。cache schema 3才可rehydrate current marker；schema 2直接miss/resync。

```text
candidate marker traces
  -> validate each DataRef/Position bucket total <= 4
  -> group decoded markers by (rowId, targetTraceId, Position, Anchor)
  -> validate aggregate count <= 4
  -> preserve document trace order then bucket array order
  -> assign global lane 0..3
  -> render shape independent of anchor
```

`Outline` visible glyph使用`fill=none`及`stroke=marker.Color`。若SVG原生paint hit testing不足，renderer增加同geometry透明interaction target；該target只提供pointer/tooltip命中，不得阻止shared cursor依x slot更新，不進Y-domain/numeric legend/time axis，也不得造成candle series rebuild。

Owner release gate以PTCS browser-demo／Interactive Client的desktop/mobile F# Playwright驗direction、fill、tooltip、cursor parity及exact graph。Daedalus升級SPAA／Interactive Extension、真`BacktestPresentationEvent`映射與Notebook `.dib` parity是發布後consumer gate。

## 2026-09-24 Backtest Presentation UX Design

完整決策：`doc/RFC/RFC-PTCS-DYNAMIC-0024.backtest-presentation-ux.md`。

### Contract and codec

```fsharp
type TaOverviewStripe =
    { StripeId: string
      EventTimeUtc: string
      Color: string
      StrokeWidthCssPixels: float
      Label: string option
      Tooltip: TaMarkerTooltipField array }

type TaOverviewStripeTraceOptions =
    { TargetTraceId: string
      CollisionGroup: string
      LayerOrder: int }
```

`TaTraceKind.OverviewStripe`的data ref沿用`TemporalSeries`：每個`TemporalSeriesPoint.Position` value是`TaOverviewStripeCodec.encodeBucket`產生的array；item `_type=ta-overview-stripe.v1`。`EventTimeUtc`須與該position的canonical axis event time一致，target須為同row candlestick。Options keys固定為`overviewStripe.targetTraceId`、`overviewStripe.collisionGroup`、`overviewStripe.layerOrder`。

`TaOverviewStripeContract.documentErrors/candidateErrors`只掃stripe refs、target candle refs及axis，不掃全部TA values。Limits為bucket 64、dataRef 10,000、frame 20,000；id 128、label 64、collision group 64、tooltip 16、layer 0..63、stroke width 0.5..4.0。Marker bucket/lane hard limit同步由4升為64，dataRef/frame維持10,000/20,000。

### Candidate flow

```text
RuntimePatch [ReplaceDataRef stripes; ReplaceDataRef orders; ReplaceDataRef fills]
  -> validate patch operation shape/reference
  -> fold every operation into private candidate map
  -> validate temporal axis/series
  -> validate marker candidate across traces
  -> validate overview stripe candidate across traces
  -> commit revision/data once OR retain last-good
```

Malformed／duplicate／hard-limit使用`RejectFrame Recoverable=false`；missing authoritative axis/data與revision gap使用`RequestResync`。Validation不得修改state、DOM或先發布任何單一dataRef。

### Overview prepared renderer

```text
stripe EventTimeUtc
  -> target canonical axis position
  -> active visible-window slot
  -> navigator X pixel
  -> group(targetTraceId, collisionGroup, xPixel)
  -> order distinct traces by LayerOrder/document order
  -> equal vertical lanes; same-trace ids remain in pixel bucket
  -> batch SVG paths by trace/lane/color/width
```

Stripe path為`pointer-events:none`。Navigator共用interaction layer查prepared pixel bucket並提供count／tooltip，不在pointer move掃全資料。Z-order為close path、stripe path、selection mask／handle。Close path sampling與stripe index各自處理，禁止拿sampled close index當event位置。

### Dense marker presentation

Candidate以document trace order＋bucket order保留最多64筆。Renderer直接layout前4筆；remaining markers建立單一cluster model：`Count`、ordered marker references、active index。Cluster button支援Enter／Space toggle、ArrowUp／Down選擇、Escape close，detail逐筆復用既有label／tooltip。Cluster與glyph總數是bounded DOM，不改Y-domain、candle prepared data或shared cursor listener。

### Row resize state machine

```text
NoOverride
  -> pointer/keyboard resize -> Preview(resolvedPx)
  -> pointer-up/keyup commit -> Override(resolvedPx)
  -> Reset/Home/double-click -> NoOverride
  -> row removed/unmount -> dispose
```

Local map key為`CanvasInstanceId + RowId`。Default height：candle/composite `clamp 180 720 (round(250 * HeightWeight))`；scalar `clamp 96 480 (round(112 * HeightWeight))`。Scenario data revision不清除override；fresh mount／reload不rehydrateoverride。Pointer move只排一個requestAnimationFrame，data readers與document fingerprint保持不變。

Total row height不再由marker presence決定。Plot height由resolved total扣除header／axis／small padding；scalar plot不留固定空band。Row cursor timestamp使用SVG plot外固定32px top gutter中的92×28px兩行HTML tag，字型、font size、line-height、padding、border、width與height都是固定CSS pixels；X跟shared crosshair並左右clamp。Row resize只改plot height及tag的top/left，不得縮放或重排tag。

### Package closure

RFC-0024 closure：Contracts `0.1.19` → Renderer `0.1.45` → Interactive.Client `0.1.37`；Dynamic.Ptcs `0.1.41` exact Contracts；Ptcs.Client `0.1.59` exact Contracts/Renderer；兩個PTCS adapters維持PTCS `[0.2.46]`。Interactive.Client的bundle manifest由pack target以`$(Version)`生成，避免nuspec與內嵌bundle版本漂移。不得ProjectReference或partial graph。Navigator使用獨立shell prepared-data signal；same-topology資料patch更新該signal與row data Vars，但不更新chart runtime mount identity。

Test seams：strict codec/unknown field/limits；candidate atomicity/last-good；same-X lane/pixel bucket；same-topology empty→non-empty OverviewStripe且chart render sequence不變；marker cluster keyboard/focus；row resize/default/reset/dispose；4,000 slots long-task；exact nupkg/bundle/readback。

## 2026-09-25 Runtime Snapshot Transport Framing

完整決策：`doc/RFC/RFC-PTCS-DYNAMIC-0025.chunked-snapshot-transport.md`。

### Wire shape

Wire使用explicit schema/kind fields，不擴充`RuntimePayload`：

```fsharp
type RuntimeSnapshotTransportPacket =
    | Start of batchId: string * itemCount: int * header: RuntimeFrame
    | Item of batchId: string * itemIndex: int * dataRef: string * value: SduiValue
    | Commit of batchId: string * itemCount: int

RuntimeSnapshotTransportCodec.encodeFrame : RuntimeFrame -> Result<string array, string>
```

實際JSON root皆含`schema = "ptcs-dynamic-snapshot-chunk.v1"`與`kind = "start" | "item" | "commit"`。Start的header必須是原frame identity／sequence／revision／freshness，但`Snapshot.Data = Map.empty`。Item依F# Map canonical key order編號0..N-1。Commit重申batch id與count。Batch id由frame identity與transport sequence的deterministic fingerprint產生，不依browser state。

Encoder以structured JSON writer嵌入`Json.Serialize`產生的header/value raw JSON，禁止字串拼接／雙重JSON字串。非Snapshot frame直接回`[| BrowserRuntimeCodec.encode frame |]`，因此host可一律flatten結果。

### Browser state machine

```text
Idle
  + legacy RuntimeFrame -> scheduled legacy decode/reduce -> publish or reject
  + Start(generation,batch,count,header) -> Staging(nextIndex=0, seen={}, items=[])

Staging
  + Item(same batch,index=nextIndex,unique ref) -> phased decode -> append -> nextIndex+1
  + Commit(same batch,count=nextIndex=expected) -> assemble canonical Snapshot
      -> phased canonical reducer validation
      -> one publish + accepted projection eligible for optional cache write + local SnapshotAccepted lifecycle
  + wrong/missing/duplicate/interleaved/disconnect/new generation -> discard -> last-good + resync
```

`OnMessage`只把`socketGeneration + encoded text`加入bounded FIFO並排一個pump task。Pump每次只處理一個envelope；同一item內Array／TemporalSeries points沿用`PointDecodeBatchSize=256`。所有continuation再次比對socket/pump generation；舊callback只回`Superseded`，不得清除新batch或建立第二個timer。

### Limits and failure

- start itemCount必須`0..MaxDataRefsPerSnapshot`；queue與staged item總量同受runtime limits限制。
- item index必須嚴格遞增，dataRef非空且唯一；batch id/count需exact match。
- start header需通過frame envelope validation且Snapshot Data為空；item value沿用snapshot value validation。
- commit後仍完整執行unknown dataRef、axis authority、temporal series、marker/stripe overlay與runtime revision validation。
- metadata錯誤使用`runtime-snapshot-chunk-*` structured reason；canonical validation沿用既有error code。所有錯誤保留last-good、停止該batch、request resync，且不得觸發accepted lifecycle或讓partial state進入cache。現行wire沒有snapshot ACK frame。

### Producer integration

```fsharp
let encodedFrames =
    runtimeFrames
    |> Array.collect (fun frame ->
        RuntimeSnapshotTransportCodec.encodeFrame frame
        |> Result.defaultWith failwith)
```

SPAA initial `Frames`與reconnect `sendFullSnapshot`都使用同一helper；Document／Patch仍各是一個訊息。Owner tests須直接消費`encodeFrame`輸出，避免測試專用framing與production client分叉。

### Owner decoder / assembler

Contracts提供純.NET與WebSharper共用的ordered stream API：

```fsharp
RuntimeSnapshotTransportAssembler.decodeFrames
    transportGeneration
    orderedWireMessages
// Result<RuntimeFrame array, RuntimeSnapshotTransportAssemblyError>
```

`decodeFrames`接受legacy frame與零或多個`start/item/commit` batch混合的單一ordered stream；`PacketIndex`永遠是整條stream的zero-based global index。逐packet consumer使用`create/createAt -> decodePacket -> acceptPacket/acceptEncoded -> finish`，不可自行切batch或複製schema判斷。`acceptPacket/acceptEncoded`只完成framing、generation、順序、count、duplicate ref與item-local validation；commit後的`CompletedFrame`是candidate。純.NET machine consumer必須呼叫`finish`做完整canonical frame validation；browser將candidate交給既有phased reducer做同等validation與atomic publish，避免commit RAF同步重複掃描完整snapshot。

錯誤優先序固定：active batch中的packet若無法decode為chunk，只有同一wire item可成功decode且validate為legacy frame時才回`runtime-snapshot-chunk-interleaved`；兩者都失敗時保留原chunk schema/kind/decode error。這使machine E2E與browser對malformed envelope取得相同reason code與global packet index。

Test seams：deterministic encoder/legacy singleton；zero-item snapshot；mixed legacy＋multi-batch stream；missing/duplicate/out-of-order/orphan/trailing/mismatch；interleaved legacy；global packet index；generation/disconnect；wrong schema/kind/malformed envelope；invalid SduiValue/canonical reducer failure；commit candidate與machine `finish` validation boundary；commit-only publish/accepted lifecycle/cache eligibility；4,000×28×5 candle target-renderer long-task；legacy/cache regression；exact package graph。

### 2026-09-29 Renderer correction

- Overview candle geometry對當前reference domain做bounded等距取樣，最多280筆；一次建立wick／up-body／down-body三條path，顏色取plot palette。OverviewStripe維持獨立batched paths，沒有trace就不猜domain、不補線。
- Row control的label區為`flex:1 1 auto; min-width:0`並ellipsis；`Edit`為`flex:0 0 52px`。完整label放title，binding lookup仍是唯一可編輯authority。
- Viewport local reducer不再因`PendingActionId`停用；`queuedVisibleRangeAction`只保存latest intent。settlement後若runtime可送，使用current revision flush；dispose/generation change清queue。
- Cursor效能authority為browser event handler記錄request time，shared crosshair／labels／visible value完成後記event-to-render latency。Playwright host round-trip只作driver診斷，不作UI gate。

Current released exact graph：Contracts `0.1.26` → Renderer `0.1.62` → Interactive.Client `0.1.54`；Dynamic.Ptcs `0.1.49` exact Contracts；Ptcs.Client `0.1.76` exact Contracts/Renderer；兩個PTCS adapters維持PTCS `[0.2.46]`。Daedalus fresh-cache machine consumer已用owner `decodeFrames`解出4,000 workspace，1,140 functional E2E與3,820 standard trace皆PASS；48→All為59.41ms／over100=0。五包public push回`Created`；NuGet repository signatures有效，official nuspec／DLL／bundle functional entries與local immutable artifact相同。大型candle projection以固定bucket array單次聚合；line projection以固定bucket min/max arrays保留chronological extrema；Y-domain以first-value initialized accumulator單次掃描，避免WebSharper對Infinity sentinel的錯誤轉譯。Shared-cursor click只查accepted prepared timeline，不再從raw data重做projection。Prepared geometry同時保存每條trace的latest presentation；當shared cursor未固定時，visible-value refresh直接讀此cache，不再對missing/sparse higher-scale trace逐條自tail反向掃描。Source interval、cursor action payload、bounded cursor lookup與Y-domain padding語意不變。

### Trace lifecycle／cursor event／contrast contract

`SduiAction.RemoveTaTrace(CanvasInstanceId,rowId,traceId)`編碼為`remove-trace`，PTCS transient DTO另帶`traceId`。Renderer維護`HiddenTraces`與`RemovedTraces`兩個set；hide不送action，remove走既有single-in-flight callback。accepted/no-frame先更新removed set；Rejected／RevisionConflict不改local state。same-canvas patch保留set，Reset Canvas／identity replacement／dispose清空。

Row controls只列non-system traces。`Marker`與`OverviewStripe`排除；最後一條未remove的non-system trace移除後整列hidden，hidden trace仍算存在。DOM visibility helper先保存`data-ptcs-authored-display`，解除hidden時恢復 authored display，避免inline `display`覆蓋hidden attribute。

Cursor event reader將Marker placements與OverviewStripe events映射為`TaMarkerCursorItem`。stable id重複時Marker勝出；排序先依authored trace，再round-robin取item，visible budget為4，其餘顯示`+N`。capability存在但slot空白為`None`，缺capability為`Unavailable`。OverviewStripe event time先由canonical temporal axis解position，不從pixel或sampled close path反推。

Hollow marker先畫`data-marker-contrast-halo` path，再畫semantic marker；halo帶`data-marker-halo-for`且`pointer-events:none`。Plot不得建立`ta-marker-label-*`。Data window使用wrap／auto height／visible overflow，無固定30px或內部vertical scroll。

Test seams：remove-trace contract/adapter round-trip；accepted/reject/stale generation；hide/remove/reset/identity；last non-system trace row collapse；Marker/Stripe stable-id dedupe與round-robin；None/Unavailable；dark halo visible pixels且semantic stroke不變；plot label count 0；data window geometry；4,000-bar long-task與exact package graph。

### Visible K-bar row Y-domain

每個含candlestick的row以該row當前viewport投影後的candles與同row projected line/histogram points建立獨立bounds。完整source中viewport外的extrema、其他row及不可見point不得進入bounds；scalar-only row維持既有8% data padding。

Candlestick row使用完整SVG viewBox高度，SVG edge到visible extrema的centerline padding固定為15 CSS px，而不是另留固定plot inset再加data range百分比。Renderer以viewBox height與目前`rowHeight: Var<int>`反算value padding；row keyboard/pointer resize時同一reactive geometry重新計算，但不重掛row/SVG topology。實際path外緣可因stroke向外擴張而略小於15px；空bounds回fallback，single-value維持既有`value +/- 1`語意。測試切點為遠端extrema隔離、250/720px centerline padding、真DOM path外緣、48/200/All、pan/zoom、resize與真SPAA逐row geometry。

### Fixed CSS-pixel line / navigator boundary

一般`TaTraceKind.Line`與SMA path沿用`ta-trace-{rowId}-{traceId}`，authored width在render邊界夾為1–2 CSS px，並同時輸出`data-stroke-width-css-pixels`與`vector-effect="non-scaling-stroke"`。因此SVG viewBox、viewport與row resize只改幾何，不縮放線寬；histogram/volume仍走fill path，不套此契約。

Overview selection rect只負責半透明選取填色。左右可見邊界為`ta-overview-left-handle-visual`／`ta-overview-right-handle-visual` line，固定`#155f73`、2 CSS px、`non-scaling-stroke`及`pointer-events=none`。既有`ta-overview-left-handle`／`ta-overview-right-handle`保留為transparent rect drag hit target，避免把可操作寬度誤當畫面線寬或破壞既有操作。Owner browser gate須在default、row resize及navigator viewport commit後分別驗computed stroke與DOM contract；真SPAA consumer另量實際geometry、截圖與互動效能。

Navigator drag以live outer SVG currentTarget同時作bounds與event owner。`pointerdown`取得pointer id後必須呼叫`setPointerCapture`，後續`pointermove`／`pointerup`／`pointercancel`皆綁同一root；release即使落在SVG或嵌入iframe之外，仍完成一次commit／adjacent coverage request。capture不可用時才退回child document pointer listeners。`pointercancel`只清draft與handlers，不送remote action；cleanup必須release capture並移除三種listener。Owner gate除同document root外12px release外，另以iframe fixture驗child browsing-context外release及callback唯一增量。

所有chart row的authored/default plot height在Renderer初始化、reset與browser reload時封頂250 CSS px；此限制不改寫文件中的`HeightWeight`，也不縮小resize handle的manual range。Candlestick與scalar row的手動上限仍分別為720／480px，trader操作後可超過250px；同canvas、同row的local override在authoritative data replacement時保留。Owner gate必須先在任何resize前量測全部chart SVG，再驗manual `>250`、reset/reload回到capped default及cursor垂直幾何同步。

### Default viewport / marker OFI band

`RendererModel.initialViewportWindow`讀取`DefaultView["visibleBars"]`，只接受finite positive integral number；resolved count夾於loaded count與renderer maximum，否則fallback 48。`renderWithProjectionCommit`只在新canvas application套用一次，same-application patch、navigator drag與user selection不重設。

`RendererModel.markerCursorItems`從prepared placements依slot取出`TaMarkerCursorItem`，排序固定為trace order／lane／marker id，保留Label、Color、Tooltip與EventTimeUtc。每列建立固定24px `ta-row-ofi-band-{rowId}`，位於32px cursor gutter與SVG plot之間；single-rAF cursor callback直接更新四個預建item slots與overflow `+N`，無事件清空內容但不改高度。plot marker仍保留glyph與`<title>`，不再建立`ta-marker-label-*` inline SVG text。一般plot `mousemove`以row axis解析slot；marker glyph／overflow item的`mousemove`與click直接以accepted `TaMarkerPlacement.SlotIndex`更新同一cursor state並停止冒泡，防止高密度同CSS pixel二次snap到相鄰slot。stale generation/dispose沿用renderer lifecycle gate，不得持有可更新的舊DOM reader。

Overview selection填色為`rgba(203,213,225,.20)`；左右可見boundary為`#4ade80`、2 CSS px、non-scaling stroke。transparent 8-unit hit rect與drag geometry維持原contract。

Test seams：fresh 4,000 visibleBars、fallback/clamp；single／dense64／empty OFI；24px固定高度與DOM順序；inline label absent；超過3,000 points且marker／相鄰slot同CSS pixel時，直接hover glyph仍精確選marker slot與EventTimeUtc；一般plot axis snap不變；cursor不重掛chart；overview initial/drag palette與2px；exact package bundle manifest。
## TA row-local control layout（RFC-0030）

Renderer依 `document.Rows` 順序建立：

```fsharp
div [ dataTestId $"ta-row-control-line-{row.RowId}"; fixedSingleLine ] [
    div [ dataTestId $"ta-row-controls-{row.RowId}"; noWrap ] rowControls
    if controllableTraces.Length > 0 then
        div [ dataTestId $"ta-trace-toggles-{row.RowId}"; noWrap; rowLocalOverflowX ] traceControls
]
```

Outer `ta-row-toggles` 使用column layout；line固定高度／`overflow:hidden`。Trace region使用`flex:1 1 auto; min-width:0; overflow-x:auto; overflow-y:hidden; white-space:nowrap; flex-wrap:nowrap`，因此窄版只產生row-local水平scroll。Filter仍排除`Marker`與`OverviewStripe`，所有click/action handler保持既有實作。

測試以F# Playwright locator/BoundingBox量測：line count、descendant ownership、desktop/narrow相同高度、同row controls Y band、跨row Y band分離及narrow `scrollWidth > clientWidth`。不得用inline JavaScript或直接改generated bundle。

### Navigator local-first state flow

`pointermove -> pendingDraft <- latest -> requestAnimationFrame -> draftWindow -> viewport range afterRender receipt`。普通`pointerup`依`rendered receipt |> published draft |> pending draft |> committed`選唯一target，確保Preview、wire與authoritative final逐bar一致；若pointer明確跨出loaded boundary，則依`pending boundary |> rendered receipt |> published draft |> committed`保留clamped Start/Latest intent。清理capture後只提交一次。Preset／pan先寫`uiState`，再以nested animation frame把相同chart fields交給`chartUiState`；非chart feedback更新不得取消pending viewport render。`viewportDataReady`在full preparation開始時false、accepted prepared data完成後true，後續單純row remount不歸零；`preparedRowsReady`仍在每次chart mount前false並於所有authored rows完成後true。Overview time axis使用overview reference timeline與page display-time formatter。

Loaded projection存在時，drag session在pointerdown凍結`domainCount + global committed window`。pointermove只在該domain計算global draft並更新selection/range文字；不送callback、不重建rows。pointerup若global draft完整落在active detail，經`tryLocalWindowForLoadedCoverage`轉local window；否則以`tryLoadedCoverageWindowIntent`建立既有`ta-coverage-window.v1`。single latest queue只保存global window與feedback；排出時重新讀current projection並產生current coverage revision/query generation，禁止重送pointerdown時建立的stale action。

### Navigator overview compaction and loaded edges

`overviewPointsForCoverage`保留`High/Low option`與source range；OC／scalar為`None`。`compactOverviewCandles maximum`把ordered source切成contiguous ranges，輸出first open、last close、volume sum；只有range內全部point都有wick時才輸出max high/min low。SVG Y-domain對body-only使用open/close，wick path只讀`Some high, Some low`。

`tryLoadedCoverageEdgeIntent Start|End requested maximum projection`先驗loaded/maximum，再回既有`TaCoverageWindowIntent`。Start固定`0..count`，End固定`loaded-count..loaded`；controls共用既有`applyLoadedCoverageIntent`，因此local state、prepared-row barrier與remote latest-wins沒有分叉。

## Spa48 exact adapter release (Aster / upstream RFC-PTC-SPA-0039)
ActorRegistry/report schedule runtime source83bd67d has exactSpa0.2.48 local typed6/HTTP-PCSL/browser proof. Dynamic active package consumers require exact same Spa version; this is package compatibility, not new TA behavior. Ptcs0.1.57→0.1.58, Ptcs.Client0.1.132→0.1.133, umbrella0.1.26→0.1.27; all change only `<PackageReference Include="PulseTrade.Comm.Spa" Version="[0.2.48]"/>`. Contracts0.1.35/Renderer0.1.131/Interactive.Client0.1.123 unchanged. Legacy ACL/Login copies exactSpa18 excluded because canonical active producers are main Libs/PulseTrade.Comm/src.
Formal downstream main Spa.Host Dynamic.Ptcs58, GW.Tests umbrella27, TAResearch.Client29 with Ptcs.Client133 must cascade together. Build GeneratePackageOnBuild=false, local pack `_GetRestoreProjectStyle;GenerateNuspec` + ContinuePackingAfterGeneratingNuspec=true/NoBuild=true only after new source/DLL identity; no Pack/AfterPack/key read. Source checkpoint precedes final source-commit build; local/official/deployed gates distinct. No renderer source changes in this slice; previous own11 diagnostic attrs stay separate pending validation.
Active local package test/demo closure: Ptcs.Tests old[0.1.56]→[0.1.58], PtcsTaClient.Tests old[0.1.132]→[0.1.133], Ptcs.LiveDemo oldSpa46/Ptcs56/Client132→Spa48/Ptcs58/Client133. These known active clients must restore/build against final candidate before PASS; no source test expectations change. No Run of LiveDemo/automatic hostspawn asbuild gate. Legacy sourceACL/Login18 remain excluded.

### DYN-WBS-581 active assets
LiveDemo exactSpa48/Ptcs58/Client133並同步ContentUpdate NuGetassetpath0.2.37→0.2.48；不留compileversion與copyassetsversion不一致。Source292三nupkg finalDLL/RepositoryCommit核對，Clients loadedSpa48+83/hashAF43；Renderer131/Contracts35exact未變。失敗compilerlogs與immutable manifest保留C:artifactroot，不resetACL/停服務。

DYN-WBS-581 consumer metadata closure：LiveDemo asset path37→48修正後 fullWebSharper build6.93s/0errors/1existingWS9002 PASS；loadedSpa48+83/hashAF43及7buildassets rawSHA/copy PASS，report C:/Users/Administrator/AppData/Local/Temp/aster-ptcs-package-0.2.48/dynamic-livedemo48.asset-proof.json。未啟demoHost；local unsigned candidate的build/assetproof，不是正式部署/browser/TA semanticfix。

## Spa49 exact adapter cascade（DYN-WBS-582 / DYN-VFY-050r4）
引用位置與 snippet：
- `src/PulseTrade.Comm.Spa.Dynamic.fsproj`：`<Version>0.1.28</Version>`，Spa `[0.2.48]` → `[0.2.49]`。
- `src/PulseTrade.Comm.Spa.Dynamic.Ptcs/PulseTrade.Comm.Spa.Dynamic.Ptcs.fsproj`：Version `0.1.58` → `0.1.59`，Spa → `[0.2.49]`。
- `src/PulseTrade.Comm.Spa.Dynamic.Ptcs.Client/PulseTrade.Comm.Spa.Dynamic.Ptcs.Client.fsproj`：Version `0.1.133` → `0.1.134`，Spa → `[0.2.49]`。
- `tests/PulseTrade.Comm.Spa.Dynamic.Ptcs.Tests/PulseTrade.Comm.Spa.Dynamic.Ptcs.Tests.fsproj`：Ptcs → `[0.1.59]`。
- `tests/PtcsTaClient.Tests/PtcsTaClient.Tests.fsproj`：Ptcs.Client → `[0.1.134]`。
- `tests/PulseTrade.Comm.Spa.Dynamic.Ptcs.LiveDemo/PulseTrade.Comm.Spa.Dynamic.Ptcs.LiveDemo.fsproj`：Spa/Ptcs/Client → `[0.2.49]/[0.1.59]/[0.1.134]`；`Content Update` 的 Spa build asset path 同步 `0.2.49`。
實作只作上述精確字串替換，估 12–18 非型別行；無新 abstraction。沿用 865 input allowlist 與 raw/Git-clean blob/hash manifest；pre-checkpoint 僅允三 producer fsproj 的已記錄差異，final build 必須全數匹配固定 source HEAD。CLI 關閉 GeneratePackageOnBuild/PublishNuGetAfterPack/BuildingInsideVisualStudio，完整 WebSharper 保持啟用。用 `_GetRestoreProjectStyle;GenerateNuspec`、NoBuild=true 繞過所有 Pack/AfterPack publisher hooks。驗 archive DLL/product version/RepositoryCommit/dependencies/assets 後複製 SDK library-packs；同版本不同 bytes 必須拒絕覆蓋。消費者真 runner 須 15+17=32，LiveDemo full compiler、Spa49 DLL/raw asset copy 七檔一致。風險為 stale asset path、autocrlf raw/clean blob差異、未受 flag 保護的 Pack hook；不以 ProjectReference 或假包繞過。

DYN-WBS-582 final local closure：三 producer provenance固定 `cfb6f3b1aef86de5f8adbd676284d0d4c06f6532`；Dynamic28/Ptcs59/Client134 GenerateNuspec archives與SDK401feed同bytes，未覆寫同版不同包。865 final inputs與真compiler bundle匹配。Active三consumer使用原exact PackageReference，32tests/LiveDemo fullWeb/七copy assets全PASS；兩consumer generated JS原樣回canonical，不重新pack producer。細項見 DYN-VFY-050r4。


## Spa50 exact metadata 與固定archive（DYN-WBS-583 / DYN-VFY-050r5）
- `src/PulseTrade.Comm.Spa.Dynamic.fsproj`：`<Version>0.1.28</Version>` -> `0.1.29`；Spa `[0.2.49]` -> `[0.2.50]`。
- `src/PulseTrade.Comm.Spa.Dynamic.Ptcs/PulseTrade.Comm.Spa.Dynamic.Ptcs.fsproj`：Version `0.1.59` -> `0.1.60`；Spa -> `[0.2.50]`。
- `src/PulseTrade.Comm.Spa.Dynamic.Ptcs.Client/PulseTrade.Comm.Spa.Dynamic.Ptcs.Client.fsproj`：Version `0.1.134` -> `0.1.135`；Spa -> `[0.2.50]`。
- `tests/PulseTrade.Comm.Spa.Dynamic.Ptcs.Tests/PulseTrade.Comm.Spa.Dynamic.Ptcs.Tests.fsproj`：Ptcs `[0.1.59]` -> `[0.1.60]`。
- `tests/PtcsTaClient.Tests/PtcsTaClient.Tests.fsproj`：Ptcs.Client `[0.1.134]` -> `[0.1.135]`。
- `tests/PulseTrade.Comm.Spa.Dynamic.Ptcs.LiveDemo/PulseTrade.Comm.Spa.Dynamic.Ptcs.LiveDemo.fsproj`：Spa/Ptcs/Client -> `[0.2.50]/[0.1.60]/[0.1.135]`；`Content Update` 的 `pulsetrade.comm.spa\0.2.49` -> `0.2.50`。
只改12個metadata token，估12非型別行；無新framework。保留原BOM/mixed newline，逐檔before/after raw SHA。Precheckpoint只允三已記錄producer fsproj差異；source checkpoint後865input全匹配HEAD clean blob，同時raw SHA不得改。Full WebSharper保持；GenerateNuspec使用NoBuild/false publication flags。Archive比對ID/version/RepositoryCommit/product/DLL/deps/assets後才copy SDKfeed，existingdifferentbytes一律拒絕。Consumer由core50archive manifest定義七asset exact集合，合法零byte檔不被nonempty假條件排除。

DYN-WBS-583 / DYN-VFY-050r5 本機範圍完成：producer source `636c19b1bb62f9425591c2d2fdb543de1445a9e5`，三fixed fullWeb 10.865/8.550/8.839s；三archive/exactdeps/DLL/assets與SDK401feed逐hash相符。真15+17=32/32（0ignored/failed/errored），LiveDemo fullWeb8.309s、三consumer runtime Spa50 DLL87569E35…與core f217c898匹配、七asset exact集合/bytes/hash與七ownbundle齊全。公開發布、main Host/browser與正式部署仍由upstream另驗。 詳細來源见 `doc/Verification.md` revision5。

## DYN-WBS-584 / DYN-VFY-051r1: 最小 Pack Condition 修正
`src/PulseTrade.Comm.Spa.Dynamic.fsproj` 的 PostBuildR/PostBuildD 恢復 `and '$(PulseTradeCommSpaDynamicPushNuGet)' == 'true'`；八個 project 九個 targets 皆 append `and '$(PublishNuGetAfterPack)' != 'false'`。ACL/Contracts/Interactive.Client/Ptcs/Ptcs.Client/Renderer/Login 原 Release/Windows/專屬 true 條件及 VS defaults 保留。預估 9–12 非型別行；不修改版本、PackageReference 或 Exec。
可重用 `scripts/verify-publish-hook-guards.ps1` 接 typed ProjectPath[]/EvidenceRoot/TimeoutSeconds，零參數安全執行本 repo 八專案。逐 project 從來源複製專屬 properties/target Condition 到無 SDK/import 的 fixture，全部原 task body 換為 WriteLinesToFile marker。單一真 MSBuild driver 評估每案獨立 fixture；20 cases/project 覆蓋 CLI/VS、false/global false、positive true、Debug/non-Windows/other Configuration。保留 source before/after SHA、raw source copies、stdout/stderr/result，任何來源漂移或 engine failure 都不得 finalized。無真 Pack/build/restore/Exec/key/network 行為；main 可重用同 script 驗自己的 targets。

DYN-WBS-584 實作量為9條Condition、154行typed PS1 verifier；final PS7/PS5各160/160。XML差異核對與BOM檢查證明 defaults/Exec/version/reference未變；只有核准gate與兩個舊comment移除。證據見DYN-VFY-051r1。

完整追溯：RFC-PTCS-DYNAMIC-0035 / DYN-WBS-584 / DYN-T-679 / DYN-VFY-051r1。


## DYN-WBS-585 / DEP52 exact adapter cascade

本輪承接 core ACT-A2 actor projection / bounded maintenance，維持既有 package 邊界及發布流程。精確 metadata 共 7 檔、14 literal；不改 runtime source、Contracts 0.1.35、Renderer 0.1.131、Interactive.Client 0.1.123 或 legacy ACL/Login 0.1.1。Renderer.fs 並行 WIP 與 PCSL/Playwright 資料保留。

| 相對路徑 | old → new |
| --- | --- |
| src/PulseTrade.Comm.Spa.Dynamic.fsproj | Version 0.1.29 → 0.1.30；Spa [0.2.50] → [0.2.52]；Actor.Registry [0.1.3] → [0.1.4] |
| src/PulseTrade.Comm.Spa.Dynamic.Ptcs/PulseTrade.Comm.Spa.Dynamic.Ptcs.fsproj | Version 0.1.60 → 0.1.61；Spa [0.2.50] → [0.2.52] |
| src/PulseTrade.Comm.Spa.Dynamic.Ptcs.Client/PulseTrade.Comm.Spa.Dynamic.Ptcs.Client.fsproj | Version 0.1.135 → 0.1.136；Spa [0.2.50] → [0.2.52] |
| tests/PulseTrade.Comm.Spa.Dynamic.Tests.fsproj | Actor.Registry [0.1.3] → [0.1.4] |
| tests/PulseTrade.Comm.Spa.Dynamic.Ptcs.Tests/PulseTrade.Comm.Spa.Dynamic.Ptcs.Tests.fsproj | Ptcs [0.1.60] → [0.1.61] |
| tests/PtcsTaClient.Tests/PtcsTaClient.Tests.fsproj | Client [0.1.135] → [0.1.136] |
| tests/PulseTrade.Comm.Spa.Dynamic.Ptcs.LiveDemo/PulseTrade.Comm.Spa.Dynamic.Ptcs.LiveDemo.fsproj | Spa/Ptcs/Client exact 新版；Content Update 的 pulsetrade.comm.spa\0.2.50 目錄 → 0.2.52 |

Registry 的 PrivateAssets/ExcludeAssets 與既有 test ProjectReference 均不變。先前長路徑失敗已知，復用 865 個 allowlisted raw inputs 的 SHA/clean Git blob 檢查，將 build 放在 C:/ptc-d52 的隔離短路徑；不複製 dirty Renderer 或既有 bin/obj/cache。GeneratePackageOnBuild=false、PublishNuGetAfterPack=false、BuildingInsideVisualStudio=false；full WebSharper 保留。未有 root 固定 Spa52/Registry4 local proof 與候選放行前不 build/pack 新版；package 僅本機 immutable candidate，不 upload。

依賴風險與驗收：source checkpoint → 固定 inputs / 3 producer full build → exact deps/DLL/assets/RepositoryCommit → 必要 consumers。除原 Ptcs/Client 15+17 與 LiveDemo，直接 Registry 引用異動的 umbrella tests 亦須執行並核實 runner counts。細節及各 gate 實況集中 DYN-T-680 / DYN-VFY-050r6；歷史 Spa50 已公開結果不等於本輪52完成。

## REL-01／delta exactfile與pseudocode
Source3producer：src/PulseTrade.Comm.Spa.Dynamic.fsproj Version30→31／Spaexact53／Registryexact6（原PrivateAssets/ExcludeAssets保留）；src/PulseTrade.Comm.Spa.Dynamic.Ptcs/PulseTrade.Comm.Spa.Dynamic.Ptcs.fsproj Version61→62／Spa53；src/PulseTrade.Comm.Spa.Dynamic.Ptcs.Client/PulseTrade.Comm.Spa.Dynamic.Ptcs.Client.fsproj Version136→137／Spa53，Rendererexact131不動。Tests/demo4consumers同步exactidentity，共10refs。
Pseudocode：metadata manifest grouping by path/id/old/new; require one Version and expected active reference count; ignorecommenttokens; replace only value spans descendingoffsets; preserve strictUTF8/BOM/newline/trivia; isolateforeignRenderer untouched; sourcecheckpoint scan/check/commit; freeze explicitCompile/None/resources/parentprops intoownTEMP; buildfullWebSharper SourceRevisionId actualDynamiccommit／publish=false usingfixedCore53; packGenerateNuspecNoBuild+_GetRestoreProjectStyle withoutAfterPack; validateexactnuspecID/version/deps/repoCommit andpayloadDLL/WSsidecars; SDKcopy onlyabsent/hashEqual; runregisteredproducer/Client/realHostbrowser gates actualcounts/hash; metadataReady不等於package/deployedReady。

## 2026-10-06 RF-09 Core54 相依整合 checkpoint

Core57a24a5已完成完整WebSharper及1378/1378真native/unit回歸；本repo僅metadata更新Dynamic0.1.31→0.1.32、Ptcs0.1.62→0.1.63、Ptcs.Client0.1.137→0.1.138與三個tests/demo references，Renderer source WIP保留。全域SA/SD/版本影響/pseudocode/驗收由G:/PulseTrade.fs/doc/20261006.REFACTOR/Release.Closure.md與RF-09持有；本repo尚未build/pack/publish/deploy，不宣稱完成。來源checkpoint後從各專案bin/net10.0/agent.aster內固定Git來源建置；bundle的wwwroot仍在該bin內，保持原封裝路徑。

## RF12 current: stable Actors projection controls
Live Core55 projection279->280 loses report draft/focus because Core clears treePanel before Dynamic recreates all controls. Core now retains a renderer's already-mounted node; Dynamic caches one attached projection ID/container and raw Var, keeps report controls and keyed node groups, suppresses equivalent topology updates, and retains tree/grid viewport elements plus collapse state. Detached route is not reused; existing report timer stops before work if detached. Pure F#/WebSharper, no new JS, storage, process or polling.
Canonical pseudocode/acceptance: G:/PulseTrade.fs/doc/20261006.REFACTOR/SD.md RF12 stable mounted projection. Source fullWS7assets PASS; actualnewUI stillpending. Own ActorDynamicTab only; unrelated Renderer/Renderer.fs WIP preserved. Dynamic0.1.34->0.1.35, Ptcs0.1.64->0.1.65, Ptcs.Client0.1.139->0.1.140 exactCore0.2.56. No public packages yet.
RF10 Core57 closure：Dynamic35→36、Ptcs65→66、Client140→141；六個active fsproj exact Core56→57與互相reference同步，Contracts35與Renderer131保持。Pseudocode沿上節REL-01；完整metadata及來源pin由Main ActorRetirement.Release.Inputs/Versions持有。不得把source/API相容推論成正式已換版。

202610070414 Core58 closure：Core57->58修正health request-time epoch，source1398PASS；本repo只overlay既有6active project metadata，Renderer WIP原bytes保留。Main doc/20261006.REFACTOR/SD.md與Release.Closure.md持有SA/SD與exactconsumer清單；版本map evidence/CurrentHealth.Release.Versions.json。12個next版本HEAD404已確認；NuGet與Host尚未發布/部署。

2026-10-07T07:49:40.2374080+08:00 Core59 closure metadataonly6fsproj；舊版32consumertests完成不重跑舊DLL，待新版exactpackages建成後跑同32。RendererWIP不變；新公開package/Hosts尚未完成，ownerAster。Main canonical doc/20261006.REFACTOR/evidence/Core59/Metadata.Plan.json列exactpaths/snippet Version與PackageReference value唯一變動。

| Package | 原版 | 新版 |
|---|---|---|
| PulseTrade.Comm.GW.PTCS | 1.0.39 | 1.0.40 |
| PulseTrade.Comm.Login.Pcsl | 0.1.41 | 0.1.42 |
| PulseTrade.Comm.ResourceNode.DurableProxy | 0.1.41 | 0.1.42 |
| PulseTrade.Comm.ResourceNode.Pcsl | 0.1.42 | 0.1.43 |
| PulseTrade.Comm.ResourceNode.SqlServer | 0.1.32 | 0.1.33 |
| PulseTrade.Comm.Spa | 0.2.58 | 0.2.59 |
| PulseTrade.Comm.Spa.ACL | 0.1.41 | 0.1.42 |
| PulseTrade.Comm.Spa.Dynamic | 0.1.37 | 0.1.38 |
| PulseTrade.Comm.Spa.Dynamic.Ptcs | 0.1.67 | 0.1.68 |
| PulseTrade.Comm.Spa.Dynamic.Ptcs.Client | 0.1.142 | 0.1.143 |
| PulseTrade.Comm.Spa.Host.TAResearch.Client | 0.1.38 | 0.1.39 |
| PulseTrade.Comm.Spa.Login | 0.1.40 | 0.1.41 |

202610071800 Core60 consumer planned：sourceMain doc/20261006.REFACTOR/Release.Closure.md列6fsproj exact metadata；Dynamic0.1.38→39、Ptcs0.1.68→69、Ptcs.Client0.1.143→144，Core59→60，其他package refs依Main12map同步。Renderer／actor／Line程式未改；只有metadata，新版本全HEAD404。Source checkpoint不代表tests/package/Hosts，既有15+17 consumer測試與實際Form沿原gate重驗，Main RF18-04/20仍InProgress。

## RF20 browser correction 202610071914
Core60 isolated browser proved Tab/native resize and real replies; two failures remain: narrow820x900 Send bottom1001.8 is clipped; Form90062 is reset on Plain/Form switch. Formal remains59. Reuse existing contracts; no new API/parser and no change to Baster Renderer.fs.

Pseudo code:
- Core append-work = flex column; fixed head/status, history flex1/min0, composer flex0/shrink1. Narrow sidebar clamp140/22svh/220; work vertical overflow fallback. Native resize bounds retained. Assert Send bottom<=viewport and key/value editable at1600 and820.
- Dynamic context reads existing valueText and setValue. resolveKeys([actor,template,default], draft) substitutes only read-request canonicalArgString when draft is nonempty; submitted target keys stay unchanged. Backend's existing typed resolver parses actual current draft.
- During render, initializing=true; construct defaults/previews; initializing=false. On user input refresh preview then setValue(current raw command) without submit. Mode rebuild therefore sees current draft. If nonempty draft cannot resolve, show error and Plain control, retain draft bytes, no fallback overwrite/dispatch.
- Tests: quote/Unicode/newline/whitespace/empty/null, unchanged actor/template/input array; actual browser Form->Plain->Form, editedPlain->Form, invalidPlain recovery, real echo, resize/focus and narrow Send.
Core61 supersedes unpublished-to-host60; reuse per-project outputs, new immutable versions and exact20consumer closure. Retain60 package/browser failure evidence. Source estimate20-30min, integration/browser30-45min; RF20 final ETA202610072025. Original20:10 integration ETA may shift within measured adjacent repair, not a new task clock.
