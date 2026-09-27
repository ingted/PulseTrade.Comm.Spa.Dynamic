# PulseTrade.Comm.Spa.Dynamic.Renderer

基於`PulseTrade.Comm.Spa.Dynamic.Contracts`的pure WebSharper F# TA workspace renderer。

## Responsibility

- owns：DOM/SVG chart layout、bounded local viewport、row visibility、compact query/Add Row UI、typed action callback。
- does not own：PTCS、WebSocket、SQL、market-data provider、credential、ACL或authorization。
- local-only：pan、zoom、row toggle、Reset View。
- remote typed action：Change Query、Add Row、Remove Row、Reset Canvas；每個row以toggle + 固定寬度remove control呈現，remote command in-flight時remove同樣disabled。
- shared research view：同一visible index驅動各row crosshair/cursor values；transport timestamp壓成`MM-dd HH:mm`，OHLC detail可換行，時間標籤在SVG外以HTML grid呈現，避免mobile非等比縮放文字。
- status：保留freshness、watermark、quality與recoverable last-good error；remote in-flight只禁用remote submit，不凍結local view。cache rehydrate進入`PausedForResync`時仍允許local pan/zoom/hover/cursor，但不送`VisibleRangeChanged`或`SharedCursorChanged`，直到authoritative resync完成。
- query draft：只從`TaWorkspaceDocument.DefaultView`的`query.*` metadata初始化；document revision不變的poll不覆蓋使用者輸入，metadata缺失時保持空白，禁止回退到demo symbol/interval/date。
- Apply boundary：instrument/interval/range只更新local draft；選擇interval不送action、不改authoritative query、不重render。按`Load / Apply`送`ChangeTaQuery`；accepted後依merged base/reference temporal axis選`[FromUtc, ToUtcExclusive)` local window，不改authoritative data/revision或清cache。pending期間的新query取代queue中的舊intent，transport仍維持one-in-flight。
- loaded range：shared-axis working set可保留最多4000 positions；overview以bounded bucket呈現全range，左右handle可resize，中段可move，48/200/All可切換。drag只更新draft，pointer release/`change`才commit一次render，local navigation不送server action；All模式把完整working set壓縮成bounded SVG primitives觀察長趨勢。
- Add/Edit Row：由`TaWorkspaceDocument.EditorSchemas`生成generic Text/Integer/Decimal/Boolean/Choice/Scale/List/Group表單；同template可建立多個參數實例。帶`ptcs.dynamic.editor.binding.v1`的row可預填並以stable RowId重新設定；legacy無binding row保持read-only。editor draft不會被poll覆蓋。
- Reset Canvas：送remote typed action恢復mount時initial ordered rows/query；Reset View只恢復local viewport。
- timeline：各trace依timestamp對齊reference timeline；SMA/ADX/MACD warm-up縮短不會用array index錯位或造成sequence failure。
- temporal projection：`TemporalPoint`明確指定source interval與projection；coarse K棒用`CandleSpan`跨base slots，coarse line用`RepeatAcrossBaseBuckets`，只在close後可知的indicator用`StepAfterClose`，避免look-ahead。
- shared temporal data：`temporal-axis.v1`提供唯一sparse Position/time authority，`temporal-series.v1`以Position join。Renderer不依scale推算缺失點；同一K的preview revision原位替換。candlestick可由`TaCandleDataRefs`指定的O/H/L/C/V五條scalar series合成。
- multi-candle：同一row可同時畫1K/5K等多個candlestick traces；base candle維持實心，coarse candle以trace色outline/dashed wick呈現並保留source interval metadata。
- generic marker：`TaTraceKind.Marker`以同row的candlestick trace作target；Position是唯一空間authority，EventTime只進tooltip。marker不參與reference timeline、Y autoscale或numeric legend；每個visible bucket依stable wire order直接畫前4筆，其餘以keyboard-accessible `+N` cluster逐筆檢視完整label/tooltip。Plot不畫文字label；Label/Tooltip進SVG title與row-local OFI。hollow marker保留semantic stroke，另以pointer-inert contrast halo確保dark plot可辨識。
- overview stripe：`TaTraceKind.OverviewStripe`以canonical event time定位navigator X；同trace同X合併成單一full-height線與count，同X不同trace依collision group/layer order分固定lane。paths按trace批次，hover lookup bounded；stripe不參與close sampling、Y-domain或K-bar plot pointer ownership。
- row resize：每列separator支援pointer、Arrow 8px、Shift+Arrow 32px、Home／double-click reset。override只以`CanvasInstanceId + RowId`保存在active renderer；同canvas revision/scenario replacement保留，reload／unmount／canvas identity替換清除。candlestick/composite範圍180..720px，scalar範圍96..480px；marker presence不再改row baseline。
- row-local data window：每列從自身resolved datapoint同時取得timestamp與value；candlestick固定顯示`O/H/L/C/V`，line/histogram顯示value。shared cursor頂端為兩行`yyyy-MM-dd`／`HH:mm:ss`；missing或invalid timestamp明示`Unavailable`，不借用base row。
- row composition：同一immutable `DataRef`可同時出現在overlay row與一或多個separate rows；Renderer逐row獨立解析與呈現，不依`DataRef`合併row。overlay/分列完全由owner提供的`Rows`/`Traces`決定。
- cursor/style：K棒、line point與cross-row cursor共用slot-center幾何；indicator line width為1.25，histogram維持1.0。
- event-time interaction：`BaseRowId`的真實timestamp驅動shared cursor；coarse row只使用finalized containing/as-of point，否則missing。viewport release送半開event-time range，pending期間controls不可重入。
- plot surface theme：consumer以`TaPlotSurfacePresentationCodec.apply`在`DefaultView`選Light／Dark；Renderer用同一palette繪製candle、TA、overview、grid、cursor、axis、legend與tooltip。未指定維持Light，不讀browser preference或consumer CSS作authority。
- histogram polarity：consumer以`TaHistogramTraceOptionsCodec.apply`設定正／負色；Renderer依value符號產生兩個batched paths。未指定則兩色皆沿用`TaTraceSpec.Color`，不從trace name／label推論MACD或交易語意。

## API

```fsharp
TaWorkspaceRenderer.render
    TaWorkspaceRenderer.defaultOptions
    callbacks
    runtimeState

TaWorkspaceRenderer.renderWithProjectionCommit
    TaWorkspaceRenderer.defaultOptions
    callbacks
    (fun committedState -> publishReceipt committedState)
    runtimeState
```

`render`維持相容；需要browser completion contract的host使用`renderWithProjectionCommit`。callback只在current generation的full mount或same-topology visible-row refresh完成，並跨下一個animation-frame boundary後觸發；stale、superseded、current-state mismatch與相同tuple重複候選不觸發。`runtimeState`是`Var<RuntimeState>`；host adapter負責strict frame decode/reducer與更新Var。正式runtime從current document讀editor catalog；`TaRendererOptions.EditorSchemas`只作standalone/test fallback。`callbacks.SubmitAction`接收含RequestId/revision的`DynamicActionRequest`並回`DynamicActionResult`；Add使用`ApplyTemplate(None, ...)`，Edit使用`ApplyTemplate(Some rowId, ...)`，不提供arbitrary URL、SQL或raw credential。

## Verification

- exact-package model/dependency/source tests：`tests/PulseTrade.Comm.Spa.Dynamic.Renderer.Tests`。
- exact-package live bundle：`tests/PulseTrade.Comm.Spa.Dynamic.Renderer.BrowserDemo`。
- desktop/mobile F# Playwright：`scripts/verify-ta-generic-marker-playwright.fsx`。
- current owner candidate：`PulseTrade.Comm.Spa.Dynamic.Renderer 0.1.82`，exact依賴Contracts `[0.1.30]`與FSharp.Core `[10.1.400]`。RFC-0030把每個authored row的row/trace controls收進獨立`ta-row-control-line-{rowId}`；control line與trace region皆nowrap，窄版只在row-local trace region水平捲動。RFC-0029的system trace排除、hide/show/remove/reset與cursor/marker語意不變。這是owner-gated candidate，Daedalus真SPAA驗收及public push仍待完成。

RFC-0028 release為Renderer `0.1.76`、Contracts `[0.1.29]`、Interactive.Client `0.1.67`。它包含RFC-0027 typed dark plot／Histogram polarity，並以`DefaultView.visibleBars`初始化fresh canvas；plot marker只保留glyph/tooltip，每列在cursor gutter與plot間提供固定24px OFI band，最多4筆＋`+N`。一般plot hover依axis snap；直接marker／cluster hit以accepted placement exact slot更新同一shared cursor並停止冒泡，避免高密度同CSS pixel時偏至鄰slot。Overview selection為淺灰，visual boundaries為亮綠2 CSS px且不改transparent hit targets。

RFC-0030 candidate為Renderer `0.1.82`、Contracts `[0.1.30]`、Interactive.Client `0.1.73`。每個row control line固定single-line Y band；右側trace region使用nowrap與row-local overflow。Marker／OverviewStripe不出現在trace controls，Reset Canvas與canvas identity replacement仍清除local hidden/removed state。

RFC-0017之後，cache rehydrate即使保留current identity/revision，只要validated Data object替換仍會重畫。row legend以`rowId + local trace index`隔離；All模式保留完整scale/cursor arrays，但以bounded candle/line paths呈現。non-base candle cursor使用一次range projection，不再對每個base timestamp掃描完整source。

RFC-0018之後，accepted `ChangeTaQuery`在host frames已合併後選local viewport。invalid/no-intersection/stale query保留原window並顯示feedback；local selection不送第二個range action。中間Renderer `0.1.34`不可使用，因其callback concurrency與正式client one-in-flight不一致。

RFC-0019/0020之後，每列依自身event-time與plot width產生adaptive axis，crosshair只覆蓋該列plot。loaded coverage可大於visible cap；越過boundary時以`VisibleRangeChanged`要求document query範圍內的相鄰coverage，merge後依event-time anchor重定位。Renderer prepare與row mount/refresh使用generation-aware frame scheduling；consumer仍負責provider query、source merge與cache authority。
