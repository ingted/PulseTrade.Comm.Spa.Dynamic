# OverviewStripe same-topology refresh hypothesis

## 現象

- 真 SPAA scenario 已產生marker，row內Order／Fill glyph與label正常。
- Navigator `ta-overview-stripe-path` 的non-empty count始終為0；close path與selection window正常。
- 失敗畫面：`C:\Windows\Temp\rfc0028-spaa-stripe-blocked.png`。

## 核心假設

1. `Renderer.fs`的runtime sink在`topologyChanged=false && dataChanged=true`時只呼叫`scheduleRowDataRefresh nextPreparedData`。
2. Navigator從mutable `preparedDataForShell`計算OverviewStripe，但該值只在full preparation或topology change更新，也沒有獨立reactive Var觸發shell render。
3. 因此同topology `ReplaceDataRef`能更新row reactive data，卻無法更新navigator；consumer mapping不是根因。

## 實驗與修正方向

- 先補focused regression：初始empty OverviewStripe，同document/topology只替換stripe dataRef後，shell visual必須變non-empty。
- 將shell prepared data改為獨立、低成本reactive authority；same-topology data patch只更新該signal及row data Vars，不改`chartRuntimeState`，避免所有row remount/rebuild。
- 用BrowserDemo instrumentation驗row mount identity／render phase不增加，並重跑4,000-slot Playwright gate。

## Baseline

- Repo：`C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic`
- Branch：`20260915_033.ptcs_group_support`
- Commit：`97b4c434901d531e8768d860c35edf91a628aecb`
- Dirty：建立本hypothesis與active log前為clean。

## Experiment

- 舊published Renderer `0.1.44`下，BrowserDemo同topology清空stripe後DOM仍保留2條path，重現真SPAA缺陷並排除consumer mapping。
- Renderer改為`Var<TaPreparedRendererData>` shell authority；full preparation、topology change及same-topology data patch都更新該Var，navigator以nested `Doc.EmbedView`重算，chart runtime state不變。
- 新Renderer `0.1.45`下，F# Playwright驗stripe path `2 → 0 → 2`，`data-chart-render-sequence`與`data-ready-row-count`全程不變；4,000-slot與long-task suite全通過。
