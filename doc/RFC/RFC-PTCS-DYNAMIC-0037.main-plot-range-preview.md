# RFC-PTCS-DYNAMIC-0037：主圖非 CURSOR 拖曳範圍預覽

- ID：RFC-PTCS-DYNAMIC-0037
- 狀態：Accepted（2026-10-27，使用者經 Daedilus 明確提出）
- Owner：Dynamic Renderer（agent.asteer）

## 背景

主 TA plot 在非 CURSOR 模式已有 pointerdown / move / pointerup 的區間提交語意，但拖曳中沒有清楚顯示將提交的左右範圍。使用者無法在放開前核對選取寬度與方向。

## 目標

- 非 CURSOR 模式拖曳期間顯示半透明範圍 preview。
- preview 隨 pointer 即時更新，左右拖皆正規化成遞增 start/end，移動後寬度必須大於零。
- 提供穩定 DOM testid 與 visible/start/end/geometry attributes，供真瀏覽器驗收。
- pointerup、pointercancel 或 mode/lifecycle 結束後清除 preview。

## 非目標

- 不改 CURSOR grab-pan 行為。
- 不改非 CURSOR 模式既有區間計算、commit action、coverage 或 event-time 語意。
- 不新增 JavaScript escape hatch。

## 情境與決策

1. pointerdown 時記錄 plot-local anchor。
2. pointermove 超過既有 threshold 後，以 `min(anchor,current)` / `max(anchor,current)` 建立 preview。
3. Renderer 在既有 SVG plot 內繪製不接收 pointer event 的半透明 rect；geometry 使用相同 plot coordinate，避免第二套 mapping。
4. pointerup 先解除gesture listeners並清除 preview，再沿原本 commit path提交；cancel只清除 preview，不提交。
5. DOM contract：`data-testid="ta-main-plot-range-preview"`，並發布 `data-preview-visible`、`data-preview-start-x`、`data-preview-end-x`、`data-preview-width`。

## 取捨

- 使用 browser-local mutable preview state與既有 frame scheduling，不引入 server action或新協議。
- preview 只表達 geometry；正式可視範圍仍由既有 authoritative commit/render 接手。

## 影響

- `PulseTrade.Comm.Spa.Dynamic.Renderer` patch 版本上升。
- `Interactive.Client` 因 direct dependency 同步 patch 版本。
- consumer 無需改 wire contract，只新增 Playwright DOM/geometry 驗收。

## 驗收

- owner focused test 先 RED 後 GREEN，涵蓋左右拖正規化、零寬隱藏、up/cancel 清除與 source wiring。
- Renderer / Interactive.Client direct tests 全綠，Release pack與 package-only build通過。
- Daedilus 真瀏覽器：左右方向 drag 中 preview 可見且寬度>0，pointerup 後 preview 清除，既有正式範圍 commit不變。

## 關聯

- `doc/RFC/RFC-PTCS-DYNAMIC-0020.time-axis-crosshair-progressive-coverage.md`
- `doc/RFC/RFC-PTCS-DYNAMIC-0034.navigator-overview-integrity.md`
- `log/20261027/20261027070917.main_plot_range_preview.log`
