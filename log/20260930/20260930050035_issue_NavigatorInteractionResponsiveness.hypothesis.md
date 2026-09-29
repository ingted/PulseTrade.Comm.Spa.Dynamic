# Navigator Interaction Responsiveness Hypothesis

## 現象

1. selection/handle drag 的 local draft geometry 有時延遲數秒至分鐘。
2. `ta-view-all` 在 loaded bars 不超過 visible cap 時未立即覆蓋 overview 全寬。
3. `ta-pan-left` / `ta-pan-right` 偶爾沒有可見反應。
4. overview 下方沒有 event-time/date axis labels。

## 基準

- Repo：`C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic`
- Branch：`20260915_033.ptcs_group_support`
- 起點：`31463f9 Record DECIDE_ON official package graph`
- 起始 worktree：clean。

## 核心假設

### H1：drag reactive storm

`startNavigatorDrag` 每個 `mousemove` 都直接寫 `draftWindow.Value`，使 WebSharper 在高頻 pointer event 下同步重建或更新 navigator subtree；缺少單一 pending draft 加 `requestAnimationFrame` coalescing。驗證方式：以 browser gate 量測 pointermove 到 selection geometry 的 frame 延遲、callback 次數及 long task，並確認 pointerup flush 最新 pending draft後只送一次 action。

### H2：command local state 與 axis output 缺口

View All/pan 可能被 pending/canonical state 回寫、no-op 判斷或 boundary request path 吃掉本地可見變更；overview SVG 本身未配置 event-time axis label geometry。驗證方式：讀 `setVisibleWindow`、`moveWindow`、`showLatestCount`、overview render tree，加入 DOM geometry/data attributes 與 callback assertions，區分 local update、remote dispatch及 boundary request。

### H3：adjacent cache KnownEmpty scope 過寬

`BrowserRuntimeCache.selectAdjacent` 在沒有cache hit時，只要方向上任意位置存在零長segment就回`KnownEmpty`。這沒有先判斷requested adjacent ordinal是否仍位於loaded domain，也沒有要求empty span緊貼active boundary；因此`active=0+724 / loaded domain=4724 / remote empty=4724`會錯誤短路provider。驗證方式：單元測試先固定上述projection必須回`Miss`，並以IndexedDB seed、reload、`readAdjacent Later`驗persistent browser path不再回`KnownEmpty`。

## 不變事項

- `VisibleRangeChanged` 仍只在 interaction commit 時送出，pointermove 不送 remote action。
- coverage boundary 仍走既有 adjacent request，不偽造 loaded data。
- event-time authority來自 generic overview/base timeline，不引入 TradeCore/DECIDE_ON domain型別。
- local interaction不可等待 server action round-trip才呈現。

## 預估

- Renderer非型別宣告修改：約 80–140 行。
- Tests/verifier：約 120–220 行。
- 若超過兩倍，需在本檔追加 Experiment 說明原因。

## 結論

H1成立：直接寫reactive draft會放大pointer event工作；latest draft＋單一rAF恢復下一frame更新，pointerup同步flush避免stale commit。H2部分成立：View All geometry需要直接依current UI state投影；pan失效的直接根因是chart remount把`preparedRowsReady`暫時歸零並誤作local toolbar gate。修正為accepted data readiness控制local toolbar，row-ready只保護需要完整geometry的路徑。Overview原先確實沒有時間軸，已以generic canonical timeline補上。

H3成立：舊`.112`在domain內仍有target ordinal時回KnownEmpty。收緊為ordinal-domain-first與exact boundary empty後，pure unit與persistent IndexedDB reload gate皆回Miss，true boundary empty相容測試仍通過。
