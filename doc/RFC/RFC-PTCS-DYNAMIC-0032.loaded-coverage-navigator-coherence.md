# RFC-PTCS-DYNAMIC-0032：Loaded Coverage／Navigator Coherence

- ID：`RFC-PTCS-DYNAMIC-0032`
- 狀態：`Accepted / owner implementation verified / immutable publication pending / consumer gate pending`
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
