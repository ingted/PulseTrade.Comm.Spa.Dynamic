# Issue hypothesis: 2018 return-tail viewport intent is consumed before authoritative frames

## Phenomenon

ChangeTaQuery response已帶24 authoritative frames，activeDetail為start0/count1450、coverageRev24/queryGen19，但return後DOM仍是Loaded1450/Viewing1-1028；requested range為2026-06-01..07-15。Post-drag在settled gate後已GREEN，RED只存在return path。

## Earliest divergence

`submitQuery` accepted callback立即呼叫`applyAcceptedQuery`，讀到authoritative frames套入前的`runtimeState.Value`。與2018 timeline求requested 2026 range得NoIntersection，因此保留舊visible count1028；後續frame commit又因coverage identity/canvas scope未變而沒有重套query viewport intent。

## Primary hypothesis

Accepted-query viewport intent缺generation-scoped pending settlement；它被callback在旧runtime提前消耗，而新authoritative frame commit沒有single-consumption seam。

## Distinguishing evidence

1. 以pure/state-level回歸重現：accepted generation已回，舊runtime NoIntersection，新runtime對requested range有intersection。
2. 回歸先在0.3.6源碼RED，不改expected。
3. 修後只有matching generation + authoritative commit可消耗pending intent；stale/rejected/mismatched generation不改viewport。

## Planned implementation bound

Production非型別宣告預計15–35行，tests預計30–60行。若production超過70行或需要wire/contract變更，先追加Experiment重建設計，不直接擴大。

## Experiment

- 實作diff的production新增約83行，超過70行停檢線；原因不是擴大protocol，而是要同時覆蓋incremental preparation、full preparation、queued query supersession、identity reset與feedback single-consumption。
- 重查後維持同一最小機制：一個4-field pending record、一個pure revision/generation gate、一個settlement function；無wire/contract/consumer變更，不新增timer、retry、poll或第二套viewport authority。
- 可觀察貢獻：每個新增呼叫點只對應一條已存在的authoritative prepared-frame commit path；DYN-T-733同時鎖old frame Await、data-only Await、matching frame Selected、newer local generation Stale。

## Experiment 2: 0.3.7 consumer falsification

- 真18884的0.3.7 response已前進到DataRevision24、coverageRevision23、queryGeneration18，ActiveDetail start0/count1491；DOM也已render Loaded1491，但local window仍是start0/count1028。因此package/frames/query selection資料未缺，0.3.7的pending intent沒有成功將prepared frame結果套到ui state。
- Daedilus證實`submitWorkspaceAction`先`applyIncrementalFrames`/`runtimeState.Value <- state`，後complete Accepted。這使「prepared frame先完成、Accepted callback後建pending」成為真實時序，不是單純假設。
- Source中`shellPreparedFrame`在topology、row refresh、full preparation的accepted prepared frame都會更新；`chartCommittedFrame`則不保證每個row-refresh prepared frame更新。Accepted callback目前只讀`chartCommittedFrame.Value`，所以可在pending建立後永久停在舊frame Await。
- 更精確的根因假設：accepted callback的prepared-frame authority選錯；應讀與render shell同一權威的`shellPreparedFrame`，而非只表示topology/chart commit的`chartCommittedFrame`。
- 區分證據：0.3.8在chart root發佈bounded settlement telemetry，僅包pending、intent/latest generation、baseline/current data/query revisions、selection/applied start/count與最後outcome；不暴露bars/raw payload。原consumer必須顯示`selected-applied`、selection/applied count1491且Viewing=ActiveDetail才完成。

## Owner resolution

- Source `b9a3628249785bb0a734e86bee9c3e3721022dab`與final 0.3.7 immutable packages已證實鎖定dispatch baseline、prepared-frame settle與newer-generation cancellation；owner tests與package-only BrowserDemo全綠。
- 根因假設在owner範圍已收旂；仍需Daedilus以原consumer/oracle/host重跑2018 return sequence，只有真return顯示requested 2026 viewport才consumer close。
