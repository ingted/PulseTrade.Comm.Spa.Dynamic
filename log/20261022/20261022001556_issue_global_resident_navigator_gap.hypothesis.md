# Global resident navigator gap

## Observed facts

- RendererModel contains global resident window, ratio, preview, intent, projection-match, and inner-viewport helpers.
- Contracts contains ResidentCoverageChanged and its direct action validation.
- Renderer.fs has no call sites for the global helpers and no independent global strip DOM.
- RuntimeValidation document allowed-action whitelist omits resident-coverage-changed.
- Ptcs.Client and Server transient SduiAction matches omit ResidentCoverageChanged.

## Root-cause hypothesis

RFC-0036 was released in contract/model slices without the terminal renderer and transport slice. The earlier package statement inferred feature completion from helper presence instead of executable call sites and owner UI tests.

## Experiment

Add tests that require the global DOM/action/wire call sites before implementation. They must fail against 0.2.8, then pass only after the additive path is wired. Existing GlobalOverview=None tests remain unchanged.

## Experiment result

- RED成立：Contracts 50/51只缺action allowlist；Renderer 70/71只缺global DOM call site。
- source GREEN：Contracts 51/51、Renderer 71/71。global path採獨立move-only draft/pending state，pointermove不送action，pointerup送單一`ResidentCoverageChanged`；`GlobalOverview=None`沿用原resident navigator。
- package probe揭露第二個獨立根因：Contracts/Renderer的`GeneratePackageOnBuild=true`使direct Pack重用舊Release DLL。0.1.37/0.2.9因此retired，設定已移除並升0.1.38/0.2.10。
- Contracts0.1.38 fresh package已驗。Renderer0.2.10的fresh WebSharper compile被異常ACL的generated `websharper.log`阻擋；此為目前唯一owner release blocker，不是renderer契約或Playwright assertion failure。

## Next discriminating step

以elevated shell精確刪除`C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic\src\PulseTrade.Comm.Spa.Dynamic.Renderer\websharper.log`，fresh pack Renderer/Interactive.Client0.2.10，再確認BrowserDemo bundle含`ta-global-overview-with-axis`並執行DYN-T-684 Playwright。禁止沿用0.1.37/0.2.9 artifact。
