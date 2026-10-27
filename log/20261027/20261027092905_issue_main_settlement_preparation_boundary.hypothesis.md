# Issue hypothesis: runtime sink clears settlement before preparation

## 已證實事實

- 0.2.53 package、ProductVersion與action wire正確；ui:5帶no-ordinal `ExplicitBounds`、revision5、generation2、count1215。
- server accepted projection已materialize；ActiveDetail `50090 -> 50430`、coverage `5 -> 6`、render sequence `12 -> 14`。
- DOM仍套optimistic local1985，表示pending context未到達prepared settlement。

## 最早分歧

`runtimeState.View`收到changed coverage identity時，在排`full preparation`之前無條件把`pendingEventRangeSettlement <- None`，且drag已finish時清draft。0.2.53只在`acceptPreparedDataCore`抑制default，該時pending早已不存在，因此policy helper無法生效。

## 最小修正

factor local `pendingEventRangeMatchesDocument` predicate。sink的coverage-scope branch以既有`eventRangeSettlementOwnsViewportScope`判斷preparation ownership：matching base-row＋still pending時保留pending/draft並排full preparation；mismatch仍清。accept stage沿用0.2.53 Reanchor/Await/Reject precedence。

## 預估實作量

非型別宣告約15行；超過30行須追加Experiment。

## 驗證

- RED：0.2.53 source僅有一個scope ownership runtime call；DYN-T-719要求sink與accept兩階段都使用policy。
- GREEN：DYN-T-719＋Renderer full、Interactive full、verifier、BrowserDemo。
- E2E：Daedilus A main local1645；B cross-axis local0/續overscroll；C rapid第3 gesture geometry mounted；latency global1500-middle/4500-return settle。
