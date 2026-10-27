# Returned2991 left-resize sequence hypothesis

## Facts / source of truth

- Source5c098e8（package sourcefe213fc），既有dirty如同名operations；所有產品/套件暫不改。
- full `formal-overview-edge-drag-0315-consumerfix` ui27請求694/gen25，accepted/1frame，但沒有該步最後DOM/frame，只有30s未達約700 oracle。
- focused `returned2991-left-resize694-r5`同UTC anchor與694 count，ACK coverage rev4/gen2，final797/694、selected-applied，1/1 PASS。
- 不變量：acknowledged event-range的prepared generation、時間錨點及window必須一致；其他歷史意圖不可覆蓋最後accepted intent。

## Hypotheses / smallest experiment

1. 前段global/query/rapid操作留下pending/draft或舊非同步commit，在最後ACK後覆蓋window；需要最後request→ACK→prepared→DOM的generation/window/feedback與drag owner欄位。
2. formal讀取對象/時點不是最後committed viewport；同一bounded trace可比較DOM selector與chart telemetry，不能只延長timeout或改expected。

- producer修碼估計：尚未定位，0行；先補證據。owner負責原序列必要prefix的一次instrumented run，失敗終點也必須落盤；不重跑全套無關尾段。
- 相關程式：Renderer.fs `settlePendingEventRangeProjectionFor`、`acceptPreparedDataCore`、`committedCoverageReady`；consumer `acknowledgeDurableCoverage`已在focused實證更新generation，單憑1 frame不是根因。
- 已讀focused source d32d16fc：217–219直接套2026 query，沒有原full的2018/global/rapid歷史，故兩者不是相同state history。維持H1/H2待一次prefix終點採樣，不改產品。

## Experiment / root cause — 2026-10-28 00:36 +08:00

- owner r8原prefix完整終點取樣：`G:\coldfar_py\coldfar-symbolics\temp\agent.daedilus\formal-overview-edge-drag-0315-prefix-terminal-r8\formal-prefix-left-resize-terminal.json`及`formal-prefix-left-resize-ack-frame.json`。已用欄位投影讀取，不dump完整ACK。
- H1在本次原失敗步驟被反證：drop前rev27/gen24；request694/gen25；ACK及terminal rev28/gen25、Viewing798-1491/count694、selected-applied、pending空、ready=true，requestStart與visibleTimelineFirst皆2026-05-08T08:00+08。沒有被舊意圖覆蓋。
- H2收斂為verifier幾何/oracle不一致：formal腳本1350–1357用handle中心加selection比例計pointer-up，產品RendererModel.fs `navigatorDragAnchorClientX`對ResizeLeft卻以logical selection-left edge為anchor；最小r5及原prefix都因此請求694，產品忠實套用。formal1365硬編碼700±5，將正確694誤判RED。
- 修正owner/範圍：daedilus僅修formal測試，以selection/track實際邊界獨立計預期；handle中心僅pointer-down命中。保留request→ACK count/time/generation與最終frame一致性及反例，不放寬oracle，不動producer canonical anchor、不發新套件。原r8 RED保留；修後完整長序列及原未到達尾段仍待實跑。
- 非型別產品實作仍0行；這次不新增產品patch。後續驗收由修正後verifier的獨立正反例與一次真實full formal決定，不能以r5或r8產品狀態正確替代全套PASS。

## Next tail hypothesis — 2026-10-28 00:45 +08:00

- r11舊resize步已過且oracle unit6/6；下一move約600步送Later1500，final startDelta1500。只有此尾段是新未定位項，不能否定已證實resize根因，也不能冒稱full PASS。
- 候選一：test以loaded2991計pointer增量，但實際navigator selection/domain不同，pointer落在edge/overflow並依契約觸發Later。候選二：同resident內的合法move被renderer誤判adjacent。
- 最小證據：before track/selection/start/end、實際pointer-down/up、canonical domain/drag projection，以及最早產生Later的request。先用r11已存資料與source核算，不新增整套重跑或產品patch。producer實作估計0行，定位後再記變更。

### D39實作前設計與估計

- ui28 ACK實際projection：GlobalOverview=None、ActiveDetail axis.30k/start0/count1500；segments連續1500+1491。`coverageNavigatorWindow`已用2991顯示label，但`resolveNavigatorGestureDomain`與selection只對GLOBAL取resident，無GLOBAL退active1500。這是產品共同authority缺口，不能再沿最初H2推論認為所有後段RED都是測試。
- pseudocode：將gesture domain函式移到既有coverage helper後→gesture/selection共用coverage window→無GLOBAL intent用native loaded ordinal→scope比較identity/axis/segment且revision/gen不倒退→queued rebase同路→無GLOBAL外界沿event-time adjacent。估計runtime非型別變更約80行，含函式移位不算新邏輯；不新增型別/project/service。
- DYN-T-745需先在0.3.15紅：2991 loaded/1500 active、start797/count694移600應1397/694而非Later1500；同驗resize、非零active offset、同scope ACK、不同segments/axis/revision拒絕。舊無GLOBAL local-only selection assertion需依D39共同座標明確更正；GLOBAL跨axis測試不放寬。

