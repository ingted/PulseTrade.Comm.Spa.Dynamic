# Issue hypothesis: Renderer returned-tail host contract

## 現象

- Renderer/Interactive.Client 0.3.11 已修復 initial global-backed edge drag。
- returned tail earlier gesture 仍產生 `Direction=Earlier`、`ObservationCount=1491`、`MaximumBasePoints=1500`，Research true host 依 `SPAA-ACTION-COVERAGE-WINDOW` 拒絕。
- action UTC range 仍為 `1970-01-01T08:00+08 .. 2026-05-29T11:00+08`，未走 provider-open earlier。
- Source commit：`b6a283fb8376c95d3543a42b375a25593dd38458`；evidence/package commit：`ec252885e75689093593e72e278bb560b340fa49`。consumer package/DLL/ProductVersion parity 已由 Daedilus 證實。

## 預期契約

1. 有 direction 的 coverage action，其 `ObservationCount` 必須等於 `MaximumBasePoints`。
2. fetch/request count 與 gesture settled viewport width 是不同責任：host 可要求 1500 點，Renderer 最終 viewport 仍維持 1491 點。
3. provider-open earlier 以 loaded head 作零寬 anchor並攜帶 `ProviderOpenEarlier` authority，不把 normalized epoch 當 explicit query lower bound。
4. initial global-backed gesture不發 adjacent action，DYN-T-737 持續通過。

## 根因候選與驗法

### H1：adjacent dispatcher 混用 fetch count 與 viewport count

- 證據：現行 dispatcher 以 `min maximumVisibleBars observationCount` 建立 intent，並把 intent count寫入 pending settlement；當 timeline 只有1491而 configured max為1500，兩者被錯誤耦合。
- 區分實驗：新增 focused test，要求 outbound directional intent count=1500，同時 pending settlement count=1491。
- 最小修復方向：顯式分開 request/fetch observation count 與 viewport observation count；不放寬 host validator。

### H2：Renderer 把 normalized Unix epoch 誤判為 explicit lower bound

- 證據：現行 earlier range 只用 `String.IsNullOrWhiteSpace query.FromUtc` 判斷 provider-open；true-host state回傳 `1970-01-01T08:00+08`，因此走 explicit bounds。
- 區分實驗：核對 Research host 的 `HasProviderHistoricalAuthority` 與 query draft normalization；只有確認 epoch 是 canonical open sentinel 後，focused test才要求 start=end=loaded head 且 `ProviderOpenEarlier=true`。
- 最小修復方向：復用 canonical open-boundary helper/authority；若沒有可辨識欄位，先回報 contract 缺口，不以日期字串猜測。

## Pseudocode 與規模

```text
inspect host authority + normalized query source
plan = adjacentPlan(fetchCount = max, viewportCount = visible)
action.intent.count = plan.fetchCount
pending.observationCount = plan.viewportCount
if earlier query boundary is canonical provider-open:
    range = loadedHead .. loadedHead; authority = ProviderOpenEarlier
else:
    range = explicitQueryStart .. loadedHead
```

預估 production code 變更不超過約 25 個非型別宣告行；超過 50 行時先在本檔追加 `## Experiment` 解釋再繼續。

## Scope / ownership

- Owner：agent.asteer。
- 可改：Dynamic repo Renderer/Interactive.Client及其 direct tests、相關 current docs/log。
- 不可改：Research host validator/consumer、Daedilus source、外部 dirty paths。

