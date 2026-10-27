# Issue hypothesis: event-range settlement mixes runtime and prepared generations

## 已證實事實

- 0.2.54 ProductVersion/source/action wire正確；pending intent要求query generation2、count1215。
- authoritative runtime frame到coverage6/query2，server resident／ActiveDetail已`+340`；DOM卻保留optimistic local1985。
- DYN-T-716 pure helper對relocated timeline會Reanchor1645，對projection query1會Await；演算法本身可區分舊projection。

## 最早分歧

action completion callback呼叫`settlePendingEventRangeProjectionFor runtimeState.Value latestPreparedData`。full preparation期間這兩個authority不是atomic pair：runtime可能已是query2，`latestPreparedData`仍是query1舊timeline。generation fence因此被新runtime繞過，exact UTC在舊timeline命中1985，pending被清；真正relocated prepared frame到達時已無context。

## 最小修正

callback只讀`chartCommittedFrame.Value`的state/prepared pair；accepted preparation仍直接傳同一次`next + nextPreparedData`。舊committed projection若query1會Await並保留pending，直到query2 prepared frame原子接受後Reanchor1645。

## 預估實作量

非型別宣告約3行；超過6行須追加Experiment。

## 驗證

- RED：DYN-T-720禁止callback混用live runtime與latest prepared，要求coherent committed pair。
- GREEN：DYN-T-720＋Renderer full、Interactive full、verifier、BrowserDemo。
- E2E：Daedilus A main local1645；B cross-axis local0/續overscroll；C rapid held/regression0；D global latency settle。
