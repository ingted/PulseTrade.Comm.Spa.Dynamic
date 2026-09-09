# @DYN-TA-019 Live Preview Chart Hot Path

- Requirement: `DYN-TA-REQ-057`
- Test: `DYN-TA-T-077`
- Status: Active
- Progress: 95%

## Problem

真實SPAA frame已持續更新同一current 1K Position的close，但Renderer原本把每次`DataRevision`都視為chart重建條件。結果是最新K棒不穩定更新，且整個chart stack約每秒被替換，打斷shared cursor。

## Deliverables

| Slice | Deliverable | Progress | Status |
| --- | --- | ---: | --- |
| DYN-TA-019A | 區分Document/visible timestamp topology與live data revision | 100% | Done |
| DYN-TA-019B | live geometry以element/trace差異更新；cursor以單一rAF直接更新固定DOM | 100% | Done |
| DYN-TA-019C | same-position preview與new-position topology unit/browser回歸 | 100% | Done |
| DYN-TA-019D | exact packages與active client dependency graph發布 | 90% | 0.1.25 candidate staged；public push待真M17 |
| DYN-TA-019E | 真FSSTL `02-history-live-1k-5k-sma.fsstl`可見close/no-rerender/cursor gate | 80% | Owner integration running |

## Evidence

- SPAA actual API verifier觀察到同一current 1K close `7656 -> 7655.75`，證明MDCQ/provider/SPAA projection/runtime reducer資料有前進。
- Renderer unit `29/29`；same timestamp close改變時`sameChartTopology=true`，append timestamp時為false。
- Renderer `0.1.20`真SPAA M17反證舊generic gate不足：第二次cursor 2650ms，12秒僅174個rAF sample／28次crosshair change；SPAA host CPU約6.97%，瓶頸在browser reactive fan-out。
- Renderer `0.1.21`真SPAA M17仍失敗：12秒219 frames、最大rAF gap 1383ms、page main thread約80.4%；mousemove約0.6ms且layout/style低於1%，根因縮至RuntimeReducer／`prepareData`全retained materialization。
- Contracts `0.1.10`改為tail update保留shared prefix、patch candidate只apply一次，並以精確4,000-point append/trim T-078驗final revision。Renderer `0.1.25`按`dataRef`、suffix與changed axis position增量prepare，且以mutable reader cell避免WebSharper把latest reader編譯成initial snapshot。exact-package BrowserDemo驗Follow Latest close、Undef/長值固定30px band、bottom summary，再於historical viewport與5次live revision並行驗300次crosshair transition共5602ms、單次最大132ms、`data-chart-render-sequence`不變、historical close不被覆寫、console/page error 0。
- current local candidate為Contracts `0.1.10`、Renderer `0.1.25`、Interactive.Client `0.1.21`、Ptcs.Client `0.1.18`與Ptcs `0.1.10`；真SPAA gate完成前不public push、不標final。
- 上一版public graph Renderer `0.1.21`、Interactive.Client `0.1.17`與Ptcs.Client `0.1.14`均已可讀；排除repository signing metadata後，public/local package entries mismatch皆為0。它們是已被本地candidate取代的中間版本。

## Boundary

- Aster負責generic RuntimeState topology/data分流、Renderer hot path與package gates。
- Daedalus負責MDCQ/SPAA provider ACK、frame journal、Interactive.Extension整合及真`.dib` browser gate。
- MdcQuoteAgent仍擁有tick/current-1K source truth；Renderer不自行合成成交價或K棒。

## Completion Gate

Daedalus以新版exact graph重跑真SPAA，確認同一根visible close變動、chart render sequence不變、single cursor <=250ms、12秒至少300個rAF samples、最大rAF gap <250ms且console 0後，本項才可100% Done。
