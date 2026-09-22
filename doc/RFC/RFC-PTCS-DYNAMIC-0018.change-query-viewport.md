# RFC-PTCS-DYNAMIC-0018 ChangeQuery accepted viewport

- ID：RFC-PTCS-DYNAMIC-0018
- 狀態：Accepted / Implemented
- 日期：2026-09-22
- 上游關聯：COMM `rfc-tradecore-0026-ptcs-renderer-gate-20260922`
- 上游規格：`G:\coldfar_py\coldfar-symbolics\doc_new2\RFC\RFC-TRADECORE-0026.PTCS-ChangeQueryViewport.Feedback.md`
- 工項：DYN-TA-021
- 測試：DYN-TA-T-082..084、DYN-VFY-021 revision 4

## 背景

TradeCore 的 `ChangeTaQuery` server contract 已完成。回覆前會先將缺失的 bounded patch 合併進 Dynamic runtime；若資料已載入則回零個 frames，最後回 correlated `Accepted`。舊 Renderer 只把 `Accepted` 視為解除 pending，因此 From/To + Load/Apply 不會改變 local visible window，真 SPAA Playwright gate 停在「等待 From/To viewport selection」。

這不是資料 authority 缺口。server 已正確決定 query、patch 與 revision；缺的是 Renderer 在 accepted response 後，依已合併的 generic temporal data 選擇 local viewport。

## 目標

1. `ChangeTaQuery` accepted 後，從 merged base/reference temporal axis 選出 `[FromUtc, ToUtcExclusive)` local window。
2. 保留 document/data identity、authoritative revision、loaded cache 與 gaps；不可用 query response optimistic 改資料。
3. transport 維持 one-in-flight，但使用者可在 pending 時提出新 query；只執行最新 queued intent，舊 response 不得覆蓋新 viewport。
4. invalid range、無交集與 stale response 保留原 viewport並顯示明確 feedback。

## 非目標

- 不在 Dynamic 解析 FSSTL、MDCQ、SPAA query 或交易日曆。
- 不以 `Accepted` 取代後續 authoritative frame，也不清除或重建 browser cache。
- 不建立第二條 concurrent action transport；Add/Remove/Edit/Reset 仍遵循既有 one-in-flight。
- 不把 gap 補成假 timestamp，也不由 intervalMinutes 推造缺失 observations。

## 決策

### Generic temporal selection

Renderer 先從 current merged `RuntimeState` 取得 reference points：優先 base row，否則採可見 traces 中 reference points 最多者。`TemporalPoint` 使用 authored `IntervalStartUtc/IntervalEndUtc`；plain timestamp 視為 instant。時間字串只接受 UTC `Z`、`+00:00` 或 UTC date shorthand，正規化後以 ordinal compare，不依 browser locale。

半開區間選擇規則：

```text
start = first point whose intervalEnd > FromUtc
stop  = first point whose intervalStart >= ToUtcExclusive
window = [start, stop)
```

沒有交集時不切 All，也不清畫面；保留 current viewport並回 `requested range has no loaded observations`。

### Merge before select

Interactive/PTCS callback 既有順序是 frames merge 完成後才完成 `SubmitAction` task。Renderer 的 accepted callback在 task completion 時重新讀 current `RuntimeState`，所以 bounded patch 自然先進 reducer，再做 selection。selection 只修改 Renderer local committed/draft window，不送第二個 `VisibleRangeChanged`，避免 query→range action loop。

### Latest query wins, transport remains one-in-flight

Renderer 維持一筆 `queryInFlight` 與一筆 replaceable `queuedQuery`。pending期間再次 Apply 只覆蓋 queued intent；舊 query settled 後，若 generation 已過期就不套 viewport，接著送出唯一最新 query。正式 client 仍只有一筆 pending request，沒有 callback concurrency。

```text
Q1 send -> Q2 queued -> Q3 replaces Q2
Q1 result merges frames -> stale generation, no viewport side effect
Q3 send -> accepted -> select current merged temporal axis
```

## 影響

- 變更：Renderer query/window orchestration、Interactive/Ptcs client embedded bundle、focused tests/verifier。
- 不變：Contracts `0.1.13`、wire schema、server reducer、provider/cache authority。
- final exact graph：Renderer `0.1.36`、Interactive.Client `0.1.29`、Ptcs.Client `0.1.52`。
- 中間版 Renderer `0.1.34`、Interactive.Client `0.1.27`、Ptcs.Client `0.1.50` 已發布但不可採用：其 Renderer callback 可並行，與正式 client one-in-flight contract 不一致。

## 驗收

1. unit：half-open interval、gap/no-intersection、invalid range、patch前後、stale generation與full range。
2. BrowserDemo callback 主動拒絕 concurrent submit；連續 Q1/Q2 最後只顯示 Q2 window，loaded count與canvas identity不變。
3. 3,820 bars owner gate仍維持 All <2秒、pointer p95 <50ms、bounded SVG與console/page error 0。
4. exact package bundle包含 query feedback/latest-wins markers，三顆package及active focused consumers通過。
5. Daedalus採final graph後執行 `Rfc0026.Spaa.Backtest.playwright.fsx`；consumer真MDCQ結果不由owner fixture冒充。

## 回退

只能整組退回 Renderer `0.1.33`、Interactive.Client `0.1.26`、Ptcs.Client `0.1.49`；此時 RFC-0026 From/To viewport仍失效。不得採中間版 graph、已被UTC calendar validation取代的`0.1.35/0.1.28/0.1.51`，或混用不同 Renderer bundle。
