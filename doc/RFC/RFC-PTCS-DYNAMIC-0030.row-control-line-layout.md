# RFC-PTCS-DYNAMIC-0030：TA Row Control Line Layout

- ID：`RFC-PTCS-DYNAMIC-0030`
- 狀態：Official immutable candidate published / consumer acceptance pending
- Owner：PTCS Dynamic Renderer（Aster）
- Consumer：PulseTrade.Comm.Spa.Dynamic.Interactive.Extension／SPAA（Daedalus）
- 關聯：`RFC-PTCS-DYNAMIC-0029`、`DYN-WBS-570`、`DYN-T-626..628`

## 背景

目前 `ta-row-toggles` 是單一 `flex-wrap`，每個 row control 與其 trace controls 都作為同層 sibling 加入同一 wrapping flow。多 row／多 trace 或窄 viewport 時，下一個 row 可能接在上一列尾端，trader 無法辨識 trace 的 row ownership。這不是 SPAA domain 或 document contract 問題，而是 generic Renderer 沒有建立 row-local layout boundary。

## 目標

1. 每個 `document.Rows` item 固定產生一條 `ta-row-control-line-{rowId}`。
2. 同列左側只放 row visibility、名稱、edit/remove；右側只放該 row 的 controllable traces。
3. Row line 與 trace region 都維持 single-line；不同 row 永不共享 wrapping flow。
4. 窄 viewport 寬度不足時，只有該 row 的 trace region可水平 overflow/scroll，row line 不增高。
5. Marker／OverviewStripe 維持 system trace，不出現在 controls，也不可移除。
6. Hide/show/remove/reset 與 action callback semantics 維持不變。

## 非目標

- 不改 `TaRuntimeDocument`、row/trace schema、action wire、host reducer或consumer projection。
- 不讓 Renderer 推論 Signal／Order／Fill 或 SPAA domain。
- 不把 row-local presentation state持久化。
- 不直接修改 generated JavaScript。

## 決策

`ta-row-toggles` 改為垂直 row list。每個 row 建立 full-width、固定高度、`overflow:hidden` 的 control line：

```text
ta-row-toggles
  ta-row-control-line-{rowId}  (single line)
    ta-row-controls-{rowId}    (fixed, no wrap)
    ta-trace-toggles-{rowId}   (flexible, no wrap, overflow-x:auto)
```

Trace region 必須有 `min-width:0`，讓 flex item 能在剩餘寬度內縮小並啟用自身 horizontal scroll；不可用 outer wrap、不可讓 line auto-grow。沒有 controllable trace 的 row 仍保留 row control line，但不建立虛假 trace control。既有 selector 保留，新增 stable line/left-region selector供 owner/consumer geometry gate。

## 失敗與相容性

- 若 row id 含特殊字元，`data-testid`仍只作 opaque locator value，不作 CSS id 或 domain key。
- Horizontal overflow只影響 controls presentation；按鈕與 action state仍由既有 WebSharper handlers建立。
- 舊 consumer若只使用 `ta-row-toggles`／`ta-trace-toggles-*` selector仍可運作；新增 wrapper不改 action payload。
- Renderer bundle必須由 F# source重建；不得以舊 bundle 搭配新 DLL。

## Package Graph

此變更不改 Contracts 或 Dynamic.Ptcs server adapter：

- `PulseTrade.Comm.Spa.Dynamic.Contracts 0.1.30`（不變）
- `PulseTrade.Comm.Spa.Dynamic.Renderer 0.1.82` exact Contracts `[0.1.30]`
- `PulseTrade.Comm.Spa.Dynamic.Interactive.Client 0.1.73` exact Contracts `[0.1.30]`、Renderer `[0.1.82]`
- `PulseTrade.Comm.Spa.Dynamic.Ptcs 0.1.51`（不變）
- `PulseTrade.Comm.Spa.Dynamic.Ptcs.Client 0.1.83` exact Contracts `[0.1.30]`、Renderer `[0.1.82]`

原決策是先以 local immutable nupkg 取得 consumer GREEN 再 public push；consumer 明確要求以 official immutable readback 避免同版覆寫風險，因此 release gate 改為先發布唯一版本，再由 Daedalus 以該官方 artifact 執行真 SPAA gate。Consumer acceptance仍是RFC結案條件，發布成功不等同產品驗收完成。

## 驗收

1. Desktop與390px narrow viewport皆有 `document.Rows.Length`條 control line，且每條只擁有同 row controls/traces。
2. 每條 line 高度在desktop/narrow一致；同 row trace buttons位於同一Y band，不因窄版換行。
3. 不同 row control line 的Y band分離；窄版長trace列顯示row-local horizontal overflow。
4. Marker／OverviewStripe不出現在trace controls；既有 hide/show/remove/reset regressions全綠。
5. Browser console/page error為零；source test、full WebSharper build、package manifest/exact dependency驗證通過。
6. Daedalus只以 official immutable package graph執行真 SPAA 8-row workspace geometry／functional gate；consumer GREEN後才結案。

## Owner Evidence（2026-09-28）

- Source-built Renderer `49/49`、Interactive.Client `12/12`、Ptcs.Client `17/17`。
- `verify-ta-renderer-playwright.fsx` PASS：desktop／390px皆為7條唯一40px control line；同row trace Y band一致，MACD窄版只在row-local region overflow。既有4,000-bar、300 cursor transitions與lifecycle/performance gates全綠，正式phase皆無>100ms task。
- `verify-ta-generic-marker-playwright.fsx` PASS：4,000 bars、48 candle paths、all=64ms、pointer p95=19.89ms/max42ms；system trace與hide/show/remove/reset regressions維持。
- `verify-interactive-client-package.fsx` PASS，manifest為Interactive.Client `0.1.73`，exact Renderer `[0.1.82]`。
- Source commit `15b1f8b` 已push。Renderer `0.1.82`、Interactive.Client `0.1.73`、Ptcs.Client `0.1.83` public push均回`Created`。
- Official signed nupkg SHA-256：Renderer `20D89407F1588618E88E297EB86E5DC4C658ED7510D581519EE29AC76515A073`；Interactive.Client `21CC8DDA7DAE5C1D3C8D6C16668AAC02C0E67CC7161F248A017627DB37407C8B`；Ptcs.Client `5EE7C03DC3308E26BD58063BC51D0831F95F4349F89CDB5BF71C6A47F95F36CB`。三包`dotnet nuget verify --all`均確認有效NuGet.org repository signature，official nuspec exact dependencies符合Package Graph。
- Daedalus真SPAA 8-row desktop/640px consumer gate仍待回覆，不因public artifact存在而宣稱GREEN。
