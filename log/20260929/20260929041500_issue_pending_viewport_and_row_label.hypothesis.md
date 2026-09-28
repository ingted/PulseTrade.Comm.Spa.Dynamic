# Pending viewport / long row label hypothesis

## Repository state

- Root: `C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic`
- Branch: `20260915_033.ptcs_group_support`
- Baseline: `7ca1d81`
- Dirty at investigation start: only active task log `log/20260929/20260929041123.spaa_consumer_state_and_row_label.log`.

## Symptom

1. 真SPAA formal gate的200->All為`2144/2073ms`，click僅`51/48ms`、state wait為`2093/2024ms`，long task皆0。
2. 長row label時DMI/MACD/HA等row的Edit控制被裁切。

## Hypothesis 1 - latest viewport intent is dropped while remote action is pending

`setWindow`先受`viewportCommandsDisabledNow`阻擋；該predicate在`PendingActionId.IsSome`時為true。preset本身使用未disabled的`compactButton`，因此200的remote action pending期間點All會看似成功但handler不提交local window。formal gate之後看到的約2秒state是前一action settled／authority refresh，不是All local commit。

驗證：BrowserDemo令SubmitAction延遲，快速點200再All；舊版應無法立即呈現All。修正後All應立即提交，且remote actions最多保留in-flight加一筆latest queued intent。

## Hypothesis 2 - intrinsic label width consumes fixed control group

row line為`overflow:hidden`；`ta-row-controls-*`使用`flex:0 0 auto`，row toggle label沒有shrink/ellipsis。長label擴張控制組，Edit位於label後方而被line裁切。

驗證：以長label fixture在desktop與窄viewport量測toggle/Edit bounding boxes；Edit必須完整落在control line，label可ellipsis且title保留完整文字。

## Proposed pseudocode / estimated size

非型別宣告約35-55行：

1. local viewport disabled只依runtime lifecycle，不依PendingAction。
2. `queuedVisibleRangeAction : VisibleRangeChange option`保存latest intent。
3. 任一remote action settled後排程flush；若無pending且runtime允許，依當下revision送latest action。
4. row control parent改為shrinkable；label button使用`flex:1 1 auto/min-width:0/text-overflow:ellipsis`並保留title。
5. F# Playwright補rapid presets與long-label geometry。

## Reverse / safety

若remote lifecycle或revision測試失敗，只反向還原本輪上述局部diff；不修改contracts、host、Daedalus consumer或既有package artifacts。

## Experiment / result

- Hypothesis 1 confirmed：`PendingActionId`曾阻止local window。修正為local reducer立即提交、remote單槽latest queue；快速`200 -> All` gate通過，沒有擴充wire或API。
- Hypothesis 2 confirmed：label intrinsic width擠掉Edit。shrinkable label＋52px fixed Edit在desktop、768、375px均完整可見，full label由title保留。
- 相鄰presentation修正：overview close-only形狀不足以辨識OHLC，改為bounded wick/up/down batched paths；OverviewStripe authoring責任維持consumer，Renderer不推論domain。
- Verifier correction：price row marker攔截pointer且absolute mouse coordinate受scroll影響；改以marker-free SMA SVG及locator-relative hover。Driver round-trip不等於UI render latency，正式gate改讀browser event-to-render sequence；host round-trip只保留診斷值。
