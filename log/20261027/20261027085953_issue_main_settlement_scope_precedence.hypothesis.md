# Issue hypothesis: accepted settlement loses to coverage scope reset

## 現象與排除

- exact 0.2.51 nupkg、host ProductVersion與新browser context均已證，排除舊包／舊bundle。
- action為`ExplicitBounds`、count1215、ExpectedRevision5、QueryGeneration2；server ActiveDetail `50090 -> 50430`、GLOBAL resident `1433048 -> 1443248`、coverage `5 -> 6`、query `1 -> 2`，排除consumer拒絕或server未materialize。
- DOM local仍`1645 -> 1985`，證明owner pure UTC settlement未成為live viewport authority。

## 根因候選

`viewportScopeKey = canvasId + CoverageIdentity`。accepted relocation更新CoverageIdentity時，`acceptPreparedDataCore`先把它視為全新viewport scope，清除pending event-range settlement，再以document default套用local1985；因此DYN-T-716的pure Reanchor helper從未能守住live branch，或其結果立刻被default覆寫。

## 最小修正

matching base-row的pending event-range action在coverage scope change時擁有viewport precedence：Reanchor／Reject已consume時禁止default覆寫並完成新scope；Await時保留pending與現有optimistic viewport；pending不match新base row時仍清除並套default。以pure policy helper供DYN-T-718驗normal／Await／mismatch／unchanged scope。

## 預估實作量

非型別宣告約25行；超過50行須追加Experiment說明。

## 驗證

- RED：0.2.52缺scope ownership helper，且source仍在`viewportScopeChanged`分支直接清pending。
- GREEN：DYN-T-718＋Renderer full、Interactive full、package verifier、BrowserDemo。
- E2E：Daedilus原ui:4 resident setup → ui:5 main accepted projection，local start維持1645；再驗0.2.53 cross-axis left drop落local0並可續overscroll。
