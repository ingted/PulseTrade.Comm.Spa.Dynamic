# RFC-PTCS-DYNAMIC-0019 Renderer main-thread scheduling

- ID：RFC-PTCS-DYNAMIC-0019
- 狀態：Accepted / Implemented
- Owner：PTCS Dynamic Renderer
- 關聯：RFC-PTCS-DYNAMIC-0017、`log/20260922/20260922203251.renderer-long-task-gate.log`

## 背景

3,820 個 base positions、7 rows 的真實工作量曾把初次 render、`All` 與 marker replacement 分別壓成約 987ms、682ms、280ms 的 main-thread long task。總耗時小於兩秒不能代表互動可用；單次同步工作仍會凍結 cursor、scroll 與輸入。

## 目標與非目標

目標是讓 Renderer 自有的資料準備、row mount、row refresh 與 topology replacement 成為 bounded scheduling slices，並以 render/data generation 丟棄 stale work。不得改變 source/cursor/Y-domain、document identity/revision、one-in-flight action 或 authoritative RuntimeFrame 語意。

BrowserDemo 建立大量 fixture 的成本須與 Renderer API setup 分開量測；owner gate不把 consumer 在呼叫 Renderer 前同步建資料的時間誤算成 Renderer，但仍輸出 diagnostic evidence。

## 情境

1. 首次載入3,820 positions／7 rows時先呈現可辨識的workspace shell，資料準備完成後再提交current generation。
2. 使用者切All、替換marker或document時，舊row work可被新generation取消，不阻塞cursor／scroll。
3. consumer在呼叫Renderer前同步產生fixture時，該時間另列diagnostic，不歸咎Renderer，也不因此豁免owner phase gate。

## 取捨

逐frame排程會讓row在很短時間內依序出現，而不是一次同步完成；換得的是可中斷、可取消且不凍結UI。只有完整current generation能commit registry，避免用partial reader換流暢度。未採Web Worker，因WebSharper DOM／prepared geometry仍需主執行緒，跨執行緒序列化會增加另一套state與copy成本。

## 決策

1. `prepareDataScheduled` 依 data entry 分兩階段準備 temporal axes 與 resolved series，每個 entry 經 scheduler 讓出 event loop。
2. initial preparation 未完成前只 render 輕量 chart shell；完成後一次發布該 generation。identity/data 在途中改變時，舊 generation 不得 commit。
3. row Doc 依序逐 frame mount；row data refresh 同樣逐 row，新的 generation 取消舊工作。
4. overview 只取 bounded evenly-sampled data；visible range 與 topology 使用 prepared metadata，不重複 parse 全資料。
5. CDP trace gate分成 module-bootstrap diagnostic 與 owner interaction phases。`All`、marker replacement、document replacement 任一 owner phase 出現 >100ms task 即失敗。

## 影響

- Renderer內部prepare與row lifecycle改為非同步；public document/frame/action contract不變。
- host/client不需新增transport或設定；stale work只在presentation層被取消。
- browser tests須等待current generation完成，不能以第一個shell DOM出現就宣稱render完成。

## 驗收

- 3,820 positions／7 rows：Renderer setup、`All`、marker replacement、document replacement owner phases 無 >100ms task。
- cursor 300 次 sweep：transport-observed p95 <125ms、max <400ms、沒有 chart-wide rerender；owner CDP phase仍嚴格不得出現 >100ms task。
- generation replacement、latest query、marker identity、navigator commit 與 existing unit suites不回歸。

## 關聯

- `doc/RFC/RFC-PTCS-DYNAMIC-0017.renderer-cache-row-performance-gate.md`
- `doc/RFC/RFC-PTCS-DYNAMIC-0020.time-axis-crosshair-progressive-coverage.md`
- `doc/TAResearch/REQ.md`：DYN-TA-REQ-069
- `doc/TAResearch/Test.md`：DYN-TA-T-085、DYN-TA-T-090
