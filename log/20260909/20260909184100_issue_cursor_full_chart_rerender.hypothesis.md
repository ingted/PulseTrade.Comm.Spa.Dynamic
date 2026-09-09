# Cursor full chart rerender

## 現象

Daedalus以M16 true-provider gate量得visible=48、loaded=3,820時，單次shared cursor移動約1,300ms，`data-chart-render-sequence`由3變4；人類操作可感知3至5秒延遲。

## 核心假設

`compositeSvg`的mousemove呼叫`setCursorIndex`，而`sameChartUiState`把`CursorIndex`納入main chart `View.Map2` cache key，造成每次hover重算reference timeline、全部candle/line projection並重建chart stack。cursor本應只是overlay與detail presentation state，不應使series geometry失效。

## 不變事項

- shared cursor仍跨所有row對齊同一base-axis timestamp，canonical source interval/value不變。
- remote `SharedCursorChanged` dispatch語意與paused suppression不變。
- 不修改MDCQ/provider、FSSTL或cache authority。
- 只用F#/WebSharper，不新增JavaScript或inline script。

## 最小修正

1. 將cursor index移出main chart reconciliation key；series/document/viewport/visibility變化仍可重建geometry。
2. crosshair與cursor detail改為獨立reactive overlay，只更新必要DOM attributes/text。
3. 補renderer fixture與F# Playwright gate，驗cursor move前後chart render sequence不變且延遲低於500ms。

## 驗收

- M16 final exact-package gate：shared cursor跨7 rows，render sequence不變，headless Chrome單次操作低於500ms。
- 既有3,820×28、same-revision replacement、constituent candle、paused interaction與console 0 gates不回歸。

## Operations

- 18:41：建立假設；尚未修改產品碼。
- 18:46：`CursorIndex`已移出`sameChartUiState`；row crosshair改為persistent SVG line搭配`Attr.Dynamic`，cursor detail改為獨立`Doc.EmbedView`。document/series/viewport改變仍走原main chart reconciliation。
- 18:50：Renderer 0.1.13 unit 27/27及3,820-position/28-series F# Playwright通過；7-row cursor移動180ms、`data-chart-render-sequence`不變、desktop/mobile無overlap、console/page error 0。
- 18:55：Interactive.Client 0.1.10 unit 4/4、lifecycle Playwright通過；Ptcs.Client 0.1.7 unit 15/15及LiveDemo full WebSharper build通過。BrowserCache Playwright亦通過finalized-prefix、rehydrate與clear gates。
- 19:00：Renderer 0.1.13、Interactive.Client 0.1.10、Ptcs.Client 0.1.7經既有encrypted-key PostBuild流程push成功；等待public index與Daedalus M16真provider gate。
- 19:04：三顆package的public flat-container index均可讀；已將exact versions與SHA-256交付Daedalus，等待不改M16 gate的真provider回歸。
- 19:05：Daedalus以0.1.13跑原M16兩次，chart sequence不變但latency為543ms/508ms，略高於500ms；保留原gate並進第二實驗。
- 19:18：發現cursor detail仍在每次mousemove對28條完整series重做`resolvedSeries`/decode，且`try*ForCursor`以`Array.filter`配置暫存陣列。chart建立時已完成同一解析，應保存bounded readers供cursor重用。
- 19:21：Renderer 0.1.14以prepared cursor readers重用已解析series，並改`Array.tryFindBack`；本地原gate量得79ms、chart sequence不變。Interactive.Client 0.1.11與Ptcs.Client 0.1.8 local packages及unit/package gates通過，已交Daedalus重跑原M16；通過前不push。
- 19:28：F# Playwright改以第一條SVG crosshair的`x1`實際變更作停止點，本機3,820x28/7-row結果62ms、7列`x1`一致、chart sequence不變；desktop/mobile截圖無重疊。
- 19:29：Daedalus M16真provider原failing gate以0.1.14/0.1.11通過：loaded=3,820、visible=48、7 rows、projected candles、30K horizontal-step、cursor 125ms、7列`x1`一致且no-rerender。
- 19:30：Daedalus M17通過：loaded=4,000、current 1K preview、IndexedDB finalized-prefix cache hit、authoritative provider reattach及live revision 2->3；未發現generic package缺口。
- 19:36：Renderer 0.1.14、Interactive.Client 0.1.11、Ptcs.Client 0.1.8的push均回`Created`；不重複推送。
- 19:38：三顆package public flat-container與nuspec可讀。public exact edges為Renderer->Contracts `[0.1.7]`、Interactive.Client->Contracts `[0.1.7]`/Renderer `[0.1.14]`、Ptcs.Client->PTCS `[0.2.18]`/Contracts `[0.1.7]`/Renderer `[0.1.14]`。
- 19:39：Interactive.Client final exact lifecycle與browser-cache F# Playwright通過；connection `1->2`、max active `1`、snapshot request `1`、dispose active `0`，finalized-prefix/cache authority與clear gate亦通過。
