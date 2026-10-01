# Navigator preview/release parity hypothesis

## 現象

真 SPAA loaded domain `900`、active detail `200`。selection interior 向左拖曳後，250ms 內 local preview 顯示 `678-877/900`；pointerup 唯一 wire action為 `StartObservationOrdinal=675, ObservationCount=200`，authoritative final 顯示 `676-875/900`。

## Repo state

- Root：`C:/Users/Administrator/test_gemini/PulseTrade.Comm.Spa.Dynamic`
- Branch：`20260915_033.ptcs_group_support`
- Baseline：`667e963`
- Source implementation：`f27b43b`
- Dirty：僅既有 `.pcsl/nuget-rfc0034*` untracked cache，不屬本輪。

## 核心假設

1. pointermove rAF 已建立正確 global draft，但 pointerup 再以 release event 座標重算 target，兩條路徑使用不同 sample 時點或 round 規則，因此偏移固定 bars。
2. pointerup 雖取最後 draft，後續 release／queue path又從 local active window或 ratio 反算 global target，造成第二次 quantization/rebase。

## 驗證方式

- 讀取 `startNavigatorDrag` 的 pointermove/rAF/pointerup state flow，列出唯一 target source。
- 先加 deterministic 900/200 regression，斷言 preview start/count與 release target相同。
- 最小修正為保存／提交同一個 integer global draft window；禁止 pointerup以不同座標或 ratio重算。
- 跑 focused unit、F# Playwright global drag與pending/resync/iframe gates；必要時由 Daedalus重跑真 SPAA consumer gate。

## 預估修改

非型別宣告約 15-35 行，主要是 drag session pending target與release讀取順序；測試另約 20-50 行。

## Experiment

- BrowserDemo 900/200 fixture先穩定RED：Preview start=`677`、wire ordinal=`674`。將published `draftWindow.Value`置於pending之前仍維持相同RED，排除單純option order；bundle確認已含新helper，非stale package。
- 根因修正：viewport range的WebSharper `afterRender`保存exact receipt。普通release優先receipt；loaded pointer明確越界時優先final clamped pending target。
- 修正後focused parity為Preview=`677`、wire ordinal=`676`、count=`200`，authoritative final一致。Pending/resync/iframe五情境與完整browser gate通過；pointermove仍不送action，pointerup仍只有一筆。
- 實作約23行（不含tests/docs），未超過預估2倍。第一次完整browser run在既有rapid 200→All 750ms gate偶發1256.92ms，但正式renderer phases無>100ms；同artifact立即重跑全綠，判為環境抖動並保留證據。
