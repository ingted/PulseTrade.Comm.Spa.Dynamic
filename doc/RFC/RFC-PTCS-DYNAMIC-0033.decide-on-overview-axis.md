# RFC-PTCS-DYNAMIC-0033：DECIDE_ON Overview Axis／8,000 Anchors

- ID：`RFC-PTCS-DYNAMIC-0033`
- 狀態：`Accepted / owner implementation and official release verified / consumer gate pending`
- 日期：`2026-09-29`
- Owner：Aster（PTCS Dynamic Contracts／Renderer）
- Consumer：Daedalus（TradeCore SPAA／DIB）
- 上游：`RFC-TRADECORE-0031.PTCS-DecideOnOverview8000.Handoff.md`
- 關聯：`RFC-PTCS-DYNAMIC-0024`、`0026`、`0032`

## 背景

Backtest scenario 的detail可使用1K，overview則可使用DECIDE_ON 60K。現行`TaLoadedCoverageProjection`把`OverviewAnchors.ObservationOrdinal`與`ActiveDetail.StartObservationOrdinal`視為同一domain，且Renderer把OverviewStripe對到detail timeline；跨尺度時selection與stripe會錯位。`MaximumOverviewAnchors=1024`也不足以承載consumer要求的最多8,000 summary samples。

## 目標

1. overview最多8,000 anchors，detail仍最多4,000 bars。
2. explicit區分overview axis與active detail base axis；跨軸只以canonical UTC event time對齊。
3. overview維持OHLC／OC candlestick；up綠、down紅、flat中性色，不退化成close-only line。
4. scenario presentation以既有完整runtime candidate一次發布overview projection、stripes、selection/status；不得partial switch。
5. 不建立第二套navigator，不引入TradeCore／Backtest domain DU。

## 非目標

- 不在PTCS解析DECIDE_ON、scenario或交易策略。
- 不把8,000 detail bars塞進active temporal series；`MaximumActiveDetailBars=4000`與retained detail hard limit不變。
- 不讓Renderer依scale文字推算時間或補造gap。

## 決策

### D1. LoadedCoverage與overview axis職責分離

`TaLoadedCoverageProjection`新增explicit `OverviewAxisRef`。`ActiveDetail.BaseAxisRef`仍是detail/query authority；`OverviewAxisRef`只識別overview anchors的source axis。舊wire缺少此欄位時以`ActiveDetail.BaseAxisRef`解碼，維持same-axis相容；新encoder一律輸出explicit值。

`OverviewAnchors.ObservationOrdinal`只在`OverviewAxisRef`內排序。coverage observation domain只由`TotalObservationCount`、segments及active detail計算，不再用overview ordinal延長detail domain。

### D2. Event-time alignment

same-axis文件沿用global ordinal selection。axis不同時：

- detail visible window的首尾canonical UTC time映射至overview anchor timeline，得到selection ratio；
- OverviewStripe以自身validated canonical event time映射至overview anchor slot，不再要求其position存在於detail target candle；
- 無法解析overview anchors時fail closed／fallback last-good，不以ordinal猜測跨軸位置。

### D3. Bounded overview

`MaximumOverviewAnchors=8000`。`<=8000`逐筆保留；consumer超過上限時須均勻取樣且保留首尾，或提供由真observations聚合的OHLC bucket。Renderer仍用batched SVG paths及bounded visual sampling，不建立8,000個DOM node。

### D4. Candlestick

anchor value接受既有number、OHLC或OC object。OHLC只使用authoritative high/low；OC與number維持body-only，不得以open/close補造wick。缺open時才以close作degenerate body。up/down/flat各自batched path，flat使用neutral palette。跨多筆的visual reduction依RFC-0034採連續bucket聚合。

### D5. Scenario atomic candidate

PTCS不新增scenario revision。Document只以`viewport.loadedCoverageDataRef`宣告穩定projection ref；scenario-varying的loaded-coverage projection、OverviewStripe、selected status與其他結果資料同在單一`RuntimeSnapshot.Data` candidate。browser frame pump完整驗證後只發布一次RuntimeState；Document不屬於Snapshot payload，也不應為每次scenario切換重送。

Legacy inline `viewport.loadedCoverage`仍可讀；同一Document同時宣告inline與dataRef時以`ambiguous-loaded-coverage-authority`拒絕，避免兩個authority競賽。Renderer current-generation rows完成並跨paint boundary後，以既有`RuntimeProjectionCommitReceiptV1`證明presentation完成。把projection與stripes拆成不同Patch不符合本RFC。

## 相容性與取捨

- Public renderer entry signatures不變；LoadedCoverage wire仍為v1 additive field。
- F# record新增field會要求source consumer升版並重編；舊wire仍可讀。
- event-time mapping比直接ordinal稍多CPU，但只對最多8,000 anchors做bounded binary-search／batched geometry，不逐event掃DOM。

## 驗收

1. 8,000 anchors encode/decode/validation通過；8,001明確拒絕。
2. 1K detail＋60K overview時selection與stripe依event time定位，不使用跨軸ordinal。
3. 3,000 observations保留3,000；12,000 consumer summary為8,000且首尾一致。
4. OHLC、OC及up/down/flat三色path通過pure/browser tests；OC不得產生wick，並禁止close-line fallback。
5. 60K→1K→60K完整snapshot切換，candles/stripes/selection/status同candidate；stale candidate不發布projection receipt。
6. 8,000 anchors下正式browser main-thread phase不得超過100ms。

## Consumer handoff

Daedalus提供exact `OverviewAxisRef`、canonical anchors與OverviewStripe data，scenario切換只送完整snapshot candidate；採用owner exact packages後重跑真SPAA與fresh DIB。

## 實作結果

- Contracts將overview上限提高至8,000，新增explicit `OverviewAxisRef`，並使coverage observation domain不再受overview ordinal延長。跨軸selection與stripe只依canonical event time映射。
- `TaCoverageWindowIntent`新增`RangeAuthority = ExplicitBounds | ProviderOpenEarlier`。range-less BARS在Earlier boundary以`ProviderOpenEarlier`及相同首端anchor表達open-left request；不產生UnixEpoch假起點，舊wire缺欄位仍解為`ExplicitBounds`。
- Renderer在current generation的`data-ready-row-count`追上`data-row-count`前，停用toolbar與navigator boundary outcome。row replacement尚未完成時不會送出基於舊geometry的相鄰頁請求。
- Owner tests通過Contracts `49/49`、Renderer `58/58`、Interactive lifecycle `15/15`、Dynamic.Ptcs `15/15`、Ptcs.Client `17/17`。完整F# Playwright通過8,000-bounded overview、跨軸OHLC／selection／stripe、progressive 500 coverage／250 detail、tiny selection及既有interaction gates；正式phase無大於100ms long task。
- Final exact graph為Contracts `0.1.34`、Renderer `0.1.106`、Interactive.Client `0.1.97`、Dynamic.Ptcs `0.1.55`、Ptcs.Client `0.1.106`，source/repository commit=`f0cd0d3500b2cfdb81a2a58584f19d6fb79b6906`。五包NuGet.org repository signature、exact dependency、provenance及entry parity readback均通過；Daedalus真SPAA／fresh DIB仍是未完成停止條件。
