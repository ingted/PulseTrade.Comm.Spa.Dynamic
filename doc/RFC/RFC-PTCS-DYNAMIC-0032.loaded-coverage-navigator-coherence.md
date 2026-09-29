# RFC-PTCS-DYNAMIC-0032：Loaded Coverage／Navigator Coherence

- ID：`RFC-PTCS-DYNAMIC-0032`
- 狀態：`Accepted / owner implementation and immutable publication verified / consumer gate pending`
- 日期：`2026-09-28`
- Owner：Aster（PTCS Dynamic Contracts／Renderer／Clients）
- Consumer owner：Daedalus（TradeCore／SPAA／DIB）
- 上游：`G:\coldfar_py\coldfar-symbolics\doc_new2\RFC\RFC-TRADECORE-0030.spaa-loaded-coverage-navigator-coherence.md`
- 延續：`RFC-PTCS-DYNAMIC-0020.time-axis-crosshair-progressive-coverage.md`

## 1. 背景

現行 Renderer 以resolved `TaVisibleWindow`計算真實比例，但SVG另把selection寬度強制為至少24個viewBox unit。長coverage或minimum-bars window會因此出現「K棒仍縮小、navigator框停止縮小」。此外三個重疊透明rect各自決定drag mode，小selection的gesture取決於DOM疊放順序。

第二個邊界是現行`referenceLength`來自base-row temporal axis，而overview price只downsample到280點；兩者已分離。但RuntimeReducer仍以`MaxRetainedBarsPerSeries=4000`限制axis／series，因此「active detail <=4000、完整LoadedCoverage >4000、bounded overview」不能靠現有wire完整表達。不得提高retained hard limit後宣稱完成，因那會把多年detail搬入active browser state。

Daedalus於`RFC-TRADECORE-0030.PTCS-LoadedCoverageProjection.Handoff.md`選定shape B，並提供真MDCQ RED：initial 250、prepend 250後，Earlier應切到固定頁`1-250`，現行`Count/4`得到`189-438`。本RFC據此解除W030-3B blocker。

## 2. 目標

1. selection visual永遠精確投影resolved window／完整reference domain，不設固定像素或viewBox寬度下限。
2. 2 CSS px邊界、灰色selection與透明hit target彼此分離。
3. 小selection的left resize／move／right resize仍可穩定選取，preview與commit使用相同drag mode與window resolver。
4. `MaximumVisibleBars=4000`只限制active detail；長coverage由bounded generic projection表達，不把FSSTL、MDCQ或Backtest型別放入PTCS。
5. overview DOM與pointer成本維持bounded。

## 3. 非目標

- 不改FSSTL authority、MarketDataSlot、provider cache或Backtest執行。
- 不以增加4,000上限、補造gap或把多年bars放入SVG／active series解題。
- 不建立第二套navigator、scrollbar或consumer-specific renderer。

## 4. 決策

### D1. Visual geometry只有一個來源

`selectionRatios total resolvedWindow`是唯一selection authority；`navigatorSelectionBounds trackWidth ratios`只做clamp與比例投影。selection fill及左右visual line讀同一bounds，不再套`minimumSelectionWidth`。

### D2. CSS-pixel hit resolver

實際pointer以browser CSS pixel解析，預設透明hit target為24 CSS px。selection寬度至少兩個hit radius時，靠近左／右界選resize，內部選move；兩側hit area重疊時，把完整interaction union固定切成左／中／右三區。SVG root只呼叫一次resolver；透明rect僅呈現可操作區與cursor，不再各自決定mode。

gesture開始後保存同一drag mode與committed window。mousemove只產生draft；mouseup以既有`commitWindowBounds`提交一次。button、query response、drag preview／commit仍共同使用`clampWindow`／`resolveWindow`，到`MinimumVisibleBars`時model與visual同時停止。

### D3. Loaded coverage採validated DefaultView projection

現行wire可以表達「完整base axis <=4000、overview sampled到280、visible window更小」；sample count從不作coverage比例authority。它不能表達「active detail <=4000但coverage ordinal >4000」，因reference axis本身被retained hard limit限制。

W030-3拆成：

1. `W030-3A`：本RFC先完成精確geometry、deterministic hit resolver及既有long-reference model/browser gate。
2. `W030-3B`：以`viewport.loadedCoverage`保存`ta-loaded-coverage.v1` generic projection，分離coverage identity/revision、query generation、實際observation count／gap segments、bounded overview anchors及active detail global ordinal。此object經Contracts codec與document validation，納入既有snapshot/cache；不得只提供min/max後宣稱中間完整。

`viewport.maximumVisibleBars`是per-document active-detail cap，resolution順序為validated document value（1..4000）→host options→owner default。它不改provider retention，也不得把`MaxRetainedBarsPerSeries`直接提高到數百萬。

### D4. Bounded projection invariant

- selection比例用完整coverage observation domain，不用overview sample count。
- overview line／stripes維持bounded DOM；active rows只持有resolved detail與必要warm-up。
- coverage gap屬metadata authority；Renderer不推算分鐘、不補K。
- coverage replacement、query及drag都有generation；stale結果可進owner cache，但不得回捲viewport。

### D5. 固定寬度相鄰頁與versioned intent

toolbar Earlier／Later每次位移完整visible count，不再使用`Count/4`。若目標超過active detail，button與navigator move共同發布一個`ta-coverage-window.v1` intent；內容含expected coverage revision、query generation、global start ordinal、observation count與direction。mousemove只改draft，mouseup只commit一次。revision不符由consumer回recoverable conflict/resync，不猜舊ordinal。

### D6. Cache-first adjacent resolver

Interactive.Client提供`readAdjacent`，依cache identity、workspace、coverage identity/revision、direction與boundary選最近的temporal adjacent snapshot，結果為`Hit | KnownEmpty | Miss | Unavailable`。TouchedAt只可作淘汰順序，不可作adjacency。corrupt entry刪除後視為miss；cache unavailable不得卡住remote fallback。

### D7. Cache entry atomic presentation commit

`readAdjacent`命中的cache entry是一個不可拆分的presentation candidate：cached `Document`、`View`與snapshot data必須先經完整entry validation，再由同一個rehydrate commit發布。不得把cached A data套入current B document，也不得在frame pump尚未完成時發布partial state。

rehydrate完成後保留current authoritative document/data revisions、transport sequence與cache identity，並固定進入`PausedForResync`；cached revisions不得成為delta continuation authority。相鄰頁rebase後的loaded-coverage segments、query generation與active-detail ordinal必須跟cached document一同發布，Renderer只能看到單一一致狀態。

## 5. 資料流

```text
AuthorityRange (consumer)
  -> chunk/cache + gap truth (consumer)
  -> generic bounded LoadedCoverage projection (W030-3B)
       |-- coverage ordinal / segments / overview anchors
       `-- active detail offset + <=4000 detail
  -> resolved VisibleWindow
  -> exact selection ratios
  -> visual bounds + CSS-pixel drag-mode resolver
  -> draft preview -> single commit
```

## 6. 相容性

- `TaWorkspaceRenderer.render*` signatures不變。
- W030-3A只改presentation／interaction，不改RuntimeFrame、action或cache identity。
- 舊文件沒有long-coverage projection時沿用base-row axis；行為除移除24-unit visual floor外不變。
- W030-3B新增optional/versioned coverage object與action intent；舊文件／action仍可decode，並同步Contracts／Renderer／Interactive／PTCS exact graph。

## 7. 測試

1. Pure：1,000,000 reference observations與12-bar window投影寬度為0.012／1000，不被放大為24。
2. Pure：ordinary與overlapping hit zones的left／move／right／outside決策固定。
3. Browser：4,000或更長reference下，48／12 bars selection的DOM比例與`data-visible-*`一致；左右resize、move、preview／commit不跳動。
4. Browser：viewport resize後重新按ratio投影；visual boundary仍2 CSS px，hit resolver仍24 CSS px。
5. Performance：pointer sweep／drag／commit無大於100ms main-thread task，overview nodes bounded。
6. Consumer：Daedalus以真SPAA驗Earlier From擴coverage、main chart<=4000、selection變窄、query race及DIB parity。
7. Pure/Browser：document cap 250在loaded 250→500後仍顯示250；Earlier得到1-250，Later對稱。
8. Cache：active hit、IndexedDB adjacent hit、KnownEmpty、Miss與Unavailable各有deterministic gate。
9. Cache atomicity：B active時命中cached A，A的實際temporal/data identity、最新coverage segments/query generation及rebased ordinal 250須一次發布；Renderer同一publish顯示`Viewing 251-500`，不得出現A data＋B document。

## 8. 驗收與停止條件

Owner已完成W030-3A/3B source與fresh exact-package gate。`ta-loaded-coverage.v1`保留完整ordinal/gap truth與bounded overview，active detail page replacement走full prepare；同projection live patch才走incremental prepare。cache entry的Document/View/data亦須完整prepare後atomic commit。延遲回來且`ExpectedDocumentRevision`過期的action必須回`RevisionConflict`，不得污染新page。先前candidate graph `0.1.32/0.1.85/0.1.76/0.1.53/0.1.86`因缺少D7而retired；W030-3整體完成仍需新graph official publication、真SPAA與fresh-kernel DIB gate，任何一項缺失都須標示owner/consumer pending，不得以unit綠燈或NuGet push代替。

## 9. Committed viewport state hotfix

真SPAA以direct `ta-viewport-range`仍量到200→All `2,072/2,137ms`，證明whole-page locator雖有observer cost，卻不是完整根因。range文字原本在chart stack computation內捕捉static resolved window；接受`uiState`後仍要等整棵SVG subtree replacement才可觀察。

Renderer改以`View.Map2 uiState.View draftWindow.View`直接解析range文字。這個文字表示「viewport state已接受」，不冒充rows已繪完；`data-ready-row-count`仍是scheduled row completion authority。public renderer API、action、generation、single-render與loaded-coverage contract不變。Owner gate分別限制committed state `<=750ms`及rows-ready total `<=1500ms`，consumer既有2秒產品門檻不得放寬。

Exact graph為Contracts `0.1.33`、Renderer `0.1.89`、Interactive.Client `0.1.80`、Dynamic.Ptcs `0.1.54`、Ptcs.Client `0.1.90`。三顆新package已完成NuGet.org repository signature、exact dependency、bundle manifest與entry parity readback；真SPAA同一2秒gate仍由Daedalus驗收。

## 10. Stable controls與projection refresh correction

真SPAA使用direct locator仍有四次`1788..2012ms` committed-state延遲，且在render quiescence後按48，accepted response仍會把viewport覆蓋回All。`View.Map2 uiState`不足以解決前者，因range/presets節點本身仍由`chartRuntimeView × chartUiState`整棵替換；後者則是`coverageProjection`任一revision/detail變更被當成`CoverageIdentity`改變而重套document default。

決策：range與preset controls移到document stable shell，chart rows/navigator仍走原scheduled subtree。LoadedCoverage projection變更只決定是否full prepare；只有真正`CoverageIdentity`改變才建立新viewport scope。same-identity revision、active-detail refresh與accepted response均須保留最新local viewport intent。Public API、wire shape、action與generation不變。

Owner regression同時驗direct state barrier與same-identity accepted callback：4,000 bars focused state/rows為`160.21/373.24ms`，完整gate為`586.83/690.59ms`且正式phase無>100ms task；500 coverage／250 detail由48提交global `453-500`後，revision refresh仍保持相同window。Exact graph `0.1.33/0.1.91/0.1.82/0.1.54/0.1.92`已由source commit `83df45b`發布並完成NuGet.org簽章、dependency及entry parity回讀；真SPAA仍為consumer acceptance停止條件。

## 11. Pending action latest-wins correction

真操作允許前一個viewport command尚在flight時再選新preset。新intent必須立即更新local committed viewport；remote callback以一個in-flight加一個latest queued intent序列化，後來intent覆蓋尚未送出的舊queued intent。當前action完成後以當下revision送出latest intent；stale completion不得覆蓋local window。這不改public action或wire shape。

Owner gate快速執行`200 -> All`，要求local state `<=750ms`、rows-ready `<=1500ms`且remote callback只保留latest intent。正式browser量得state `655.91ms`、rows-ready `734.35ms`；真SPAA仍須用同一exact graph驗產品2秒門檻。

## 12. Accepted semantic-equivalence correction

真SPAA進一步重現同一`CoverageIdentity`、相同active detail與rows/data的accepted callback只提高`DocumentRevision`、`CoverageRevision`、`QueryGeneration`及transport sequence，卻再增加一次chart render。這不是authority correction，而是transport acknowledgement；若把revision counter本身當presentation identity，會讓一次local viewport intent產生兩次重畫。

決策：Renderer以runtime identity加semantic document presentation判斷shell/topology。比較時只忽略loaded-coverage的`CoverageRevision`與`QueryGeneration`；`CoverageIdentity`、segments、overview anchors、active detail、rows、schemas、actions、query/default view其餘內容仍是authority。revision-only accepted ack重用stable shell，diagnostic revision/sequence attributes以reactive binding更新；active detail、document capability、row topology或data變更仍走既有prepare／render。不得以此短路真正page replacement或放寬T-123 single-render gate。

Owner regression包含三組對照：純`DocumentRevision`前進重用shell、coverage/query counter-only前進重用shell、active-detail ordinal改變必須replacement。BrowserDemo在200→All callback settlement後要求render sequence恰好`+1`；five-candle資料替換仍更新五列且不得因semantic shell reuse漏畫。

Official exact graph為Contracts `0.1.33`、Renderer `0.1.94`、Interactive.Client `0.1.85`、Dynamic.Ptcs `0.1.54`、Ptcs.Client `0.1.95`；source commit `2c7bf76`。三顆新package已完成NuGet.org repository signature、exact dependency、bundle manifest與排除`.signature.p7s`後entry parity readback。真SPAA T-123/T-124仍由Daedalus驗收。

## 13. Pending boundary drag correction

真SPAA證明toolbar Earlier完成後若前一個remote action仍在settlement window，使用者把navigator whole selection拖越左界，舊Renderer會因`PendingActionId`拒絕mousedown，action count維持不變。這不是provider或coverage response問題，而是local intent誤用remote-disabled gate。

決策：poll/resync仍控制local interaction availability；`PendingActionId`只控制remote submission。Navigator draft/release在pending期間可繼續，越界intent與48/200/All preset共用單一latest-wins slot。當前request settled後依最新runtime重算adjacent coverage，確保不平行送request、不使用stale revision，也不丟掉trader最後操作。Identity replacement清除queue。Public contract與wire shape不變。

Owner gate以BrowserDemo 750ms callback建立race：先送48，再於pending期間向左越界拖曳，要求callback count `N->N+2`、queue feedback、`QueryGeneration=2`及相鄰前頁`405-452`。正式package發布後仍須由Daedalus真SPAA progressive gate確認。

Official correction graph為Renderer `0.1.95`、Interactive.Client `0.1.86`、Ptcs.Client `0.1.96`，source commit `67d8ea2`。三包已完成NuGet.org repository signature、exact dependencies、bundle manifest與entry parity readback；真SPAA progressive gate仍是停止條件。

## 14. Poll-state drain與cache rehydrate correction

真SPAA進一步證明失敗不只存在於`PendingActionId`：mouse-down前runtime可為`PollInFlight`或`PausedForResync`。前者若被local gate阻擋，gesture完全消失；後者即使gesture可排隊，若queue只由local action settlement觸發，純`RESYNC -> READY`也永遠不會drain。

決策：`PollInFlight`／`PausedForResync`只阻止remote dispatch，不阻止navigator local preview/release；`Unmounted`／`Disposed`才禁止local gesture。Queue同時由action settlement與runtime轉回可dispatch狀態觸發，並在dispatch前再次檢查無pending action，以single-slot latest-wins保證exactly once。

同輪cache-hit performance調查確認phased rehydrate在frame pump前仍同步執行完整snapshot semantic validation，等同掃兩次大型data。Interactive.Client改為同步驗header/document/workspace/current authority並建立frame，nested data只由phased pump驗證；generation、atomic publish與fail-closed語意不變。

Official correction graph為Renderer `0.1.98`、Interactive.Client `0.1.90`、Ptcs.Client `0.1.99`，source commit `16dd825`。Focused `55/15/17`、真PollInFlight、`PausedForResync -> Ready`、3820x28 cache rehydrate及完整Renderer browser gate均PASS。三包NuGet.org repository signatures、exact dependencies、Interactive manifest及排除`.signature.p7s`後entry parity皆PASS；Daedalus真SPAA progressive/cache-hit仍是停止條件。
