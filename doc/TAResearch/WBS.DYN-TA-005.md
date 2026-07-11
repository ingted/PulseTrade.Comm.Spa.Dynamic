# DYN-TA-005 E2EQ adapter / parallel path

Status: Active
Progress: 55%
Updated: 2026-07-12

## Scope

把既有 E2EQuotation host 的 TA data/action vocabulary 映射到 canonical Dynamic TA runtime，並以獨立、可 pack、exact-package 的 server/browser adapter 隔離 legacy E2EQ 512 KB WebSharper client graph。此 slice 不以 stale bundle 或停用 WebSharper compiler 作 UI 驗收。

## Completed

- `PulseTrade.MarketData.E2EQuotation.Dynamic.Adapter 0.1.0-alpha1`：bounded 2000-point snapshot、七列 canonical document、status/action mapping、local-only `ResetView`。
- `PulseTrade.MarketData.E2EQuotation.Dynamic.Browser 0.1.0-alpha2`：flat WebSharper DTO、finite integral revision/sequence、null/non-finite point/status validation、exact action vocabulary與shared Renderer delegation。
- 兩個 package 均能 clean pack，並以 exact package reference 納入 E2EQuotation tests。
- E2EQuotation tests 187/187 pass；涵蓋 canonical document/snapshot、local view preservation、action mapping、server/browser `dataRef`/action parity、fractional revision與non-finite point fail-closed。

## Remaining

- 在 E2EQ production host feature gate mount shared browser adapter，不能由 stale `build/PulseTrade.MarketData.E2EQuotation.js` 代替。
- 通過 clean WebSharper bundle、AgentE2E Historical/RT source、symbol/range、hover/tag/viewport/navigator regression。
- 與 PTCS host 比較 chart/rows/toolbar geometry 與 action parity。

## Blocker

Legacy E2EQ main `Client.fs` clean WebSharper merge 目前以 `wsfsc.exe -532462766` 終止；初次 incremental success 實際使用 2026-07-06 stale bundle。根因與後續 isolated route/bundle 工作追蹤於 `G:\PulseTrade.fs\Blocker.md` 的 `BLK-20260712-001`，因此 T-014/T-019 尚未通過。

## Evidence

- Root package source：`Sinopac/src/PulseTrade.MarketData.E2EQuotation.Dynamic.Adapter/`、`Sinopac/src/PulseTrade.MarketData.E2EQuotation.Dynamic.Browser/`。
- Root tests：`Sinopac/demo/e2eQuotation/tests/PulseTrade.MarketData.E2EQuotation.Tests/DynamicTaAdapterTests.fs`。
- Root operation log：`log/20260712/20260712042500.dyn-ta-e2eq-adapter.op_log`。
