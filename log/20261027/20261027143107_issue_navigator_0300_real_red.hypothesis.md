# DYN-WBS-613 hypothesis — 0.3.0 rendered pair仍在target0 release fallback

## 現象

Exact 0.3.0真Gate B中，resize與middle NavigatorWindow action可用；post-middle long body drag已算出target local start 0與正確preview/event-time，但wire action缺CoverageIntent，settle local1122而非0。

## 已證實邊界

- Package、ProductVersion與canonical min.js皆對應source `acd00422892211f2662dc1fe3a83c968c210bc4f`，不是舊bundle。
- Event handler已帶rendered atomic state/prepared pair；0.3.0因此排除「handler重讀另一chart frame」作完整root cause。
- 同一序列middle action有NavigatorWindow intent，故serializer／queue／backend可保留field；差異出現在post-middle long target0 owner release branch。
- Consumer、oracle、browser error與action failure未改／未報錯。

## 根因候選與區分實驗

1. 主要候選：post-middle rendered prepared projection或其coverage snapshot在target0 materialization的Option gate回None；caller以bare `setWindow`吞掉原因。沿production expression列出每個Option與target/boundary predicate，加入post-middle＋target0 actual-wiring test，舊碼應RED。
2. 次要候選：pointerup使用drag preview/event range，但coverage helper仍以另一個start/count或delta branch判定「不需載入」；用同一frozen pair直接比較middle target1201與long target0的builder結果，確認第一個分歧條件。

## 預計最小修復

尚未決定；先讓actual production seam在post-middle rendered pair＋target0下重現bare fallback。預估非型別宣告修改小於20行；若超過40行，在本檔追加Experiment解釋後才擴張。

## Experiment / 根因

正式0.3.0 generated JS已含D25 branch：snapshot為Some時cross-axis local commit必定呼叫`tryNavigatorEventRangeIntent`，count79／revision/query非負時該helper必定Some；真wire absence因此把分歧縮到snapshot建立前。Render mapper用outer cached shell `document`＋`currentPreparedData.RawData`計算`currentCoverageProjection`，event handler卻用`renderedState.Document`對RawData再decode。這兩個document authority在cached shell與新runtime envelope可不同，D25的state/prepared pair並未包含真正render projection。

最小修正共少於20個非型別宣告行：closure把outer document與已decoded projection傳handler；handler timeline讀document/prepared，snapshot直接map projection並保留rendered state key，不再decode。DYN-T-727舊碼0/1 RED，修後1/1；相鄰DYN-T-726與full115/115保持GREEN。

## 不變事項

不改wire schema、serializer、queue、consumer/oracle、main plot、ordinal path、2018 tail mapping；保持A／D／highlight／Tagging／原C與middle NavigatorWindow既有行為。
