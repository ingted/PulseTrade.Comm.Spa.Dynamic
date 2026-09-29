# DYN-TA-027 Loaded coverage／navigator coherence

- RFC：[`RFC-PTCS-DYNAMIC-0032`](../RFC/RFC-PTCS-DYNAMIC-0032.loaded-coverage-navigator-coherence.md)
- Upstream：`RFC-TRADECORE-0030`
- Owner：Aster；consumer：Daedalus

| Slice | Deliverable | Tests | Progress | Status |
| --- | --- | --- | ---: | --- |
| DYN-TA-027A | 移除24-unit visual floor；selection依resolved ratio投影 | T-117 | 100% | Owner PASS |
| DYN-TA-027B | CSS-pixel deterministic overlap resolver；single root pointer route | T-118/T-119 | 100% | Owner PASS；tiny move/left/right resize與browser resize全綠 |
| DYN-TA-027C | 長coverage generic projection、active detail分離與cache atomic presentation commit | T-120/T-122 | 100% | Owner PASS；validated projection、page replacement、entry-level cache rehydrate、rebase與revision conflict完成 |
| DYN-TA-027D | Exact packages與真SPAA/DIB integration | T-121/T-123/T-124 | 95% | Official graph發布/readback完成；consumer SPAA/DIB pending |

## Invariants

- `VisibleWindow.Count <= 4000`；selection visual沒有固定寬度下限。
- overview sample count不是coverage count。
- 不提高retained hard limit來冒充bounded active detail。
- gap、provider identity與authority由consumer持有；PTCS只接generic projection。
- preview／commit／buttons／query response共用window resolver。

## Current evidence

- Baseline Renderer `0.1.83`：51/51。
- Retired graph：Contracts `0.1.32`、Renderer `0.1.85`、Interactive.Client `0.1.76`、Dynamic.Ptcs `0.1.53`、Ptcs.Client `0.1.86`；缺少cached Document/View/data atomic commit，不得供consumer採用。
- Fresh final candidate graph：Contracts `0.1.33`、Renderer `0.1.86`、Interactive.Client `0.1.77`、Dynamic.Ptcs `0.1.54`、Ptcs.Client `0.1.87`。
- Focused suites `47/55/15/15/17`；DYN-T-648C驗cached A真實temporal/data identity、rebase ordinal 250、latest segments/query generation及單次Renderer publish顯示`Viewing 251-500`。F# Playwright另以4,000 bars驗tiny navigator、500 coverage／250 active detail、Earlier/Later及stale action revision conflict，正式phase無大於100ms task。
- Daedalus真SPAA／fresh-kernel DIB仍是consumer gate，不以owner BrowserDemo代替。
- Accepted semantic-equivalence official graph：Contracts `0.1.33`、Renderer `0.1.94`、Interactive.Client `0.1.85`、Dynamic.Ptcs `0.1.54`、Ptcs.Client `0.1.95`。Focused `55/15/17`；callback-settled 200→All為state `175.88ms`、rows-ready `378.52ms`、render `3->4`。完整browser gate五列資料替換、cursor與正式phase效能全綠；source `2c7bf76`與三包official readback已完成，真SPAA仍pending。
- Pending-boundary owner candidate：Renderer `0.1.95`、Interactive.Client `0.1.86`、Ptcs.Client `0.1.96`。single in-flight＋latest queued intent支援pending期間whole-selection boundary drag；focused `55/15/17`、focused與完整F# Playwright PASS，official publication/readback與真SPAA progressive gate pending。
- Pending-boundary official release：source `67d8ea2`與三包NuGet.org readback完成；repository signatures、exact dependencies、bundle manifest及entry parity皆PASS。DYN-TA-027D仍維持95%，等待Daedalus真SPAA progressive與fresh-kernel DIB，不以release integrity代替consumer acceptance。
