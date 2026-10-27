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

## Owner resolution

- Source `b9a3628249785bb0a734e86bee9c3e3721022dab`與final 0.3.7 immutable packages已證實鎖定dispatch baseline、prepared-frame settle與newer-generation cancellation；owner tests與package-only BrowserDemo全綠。
- 根因假設在owner範圍已收旂；仍需Daedilus以原consumer/oracle/host重跑2018 return sequence，只有真return顯示requested 2026 viewport才consumer close。
