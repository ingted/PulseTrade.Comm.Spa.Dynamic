# Renderer pointer-hit precedence investigation

## Pre-work plan

- 背景：Daedilus 在真 18884 `Research.TaggingScaleMatrix` 以 mouse click 點擊 30K／60K SMA SVG path；clicked element 的 `data-testid` 分別為 `ta-trace-price-30k-fsstl.sma-30k-13` 與對應 60K SMA，但 resolve request 的 `TraceId` 卻為 `fsstl.es-30k`／`fsstl.es-60k`，response target/component 為 KBar/BAR。
- 已證實事實：同一套測試的 KBar、MACD、DMI、Heikin-Ashi 都可 resolve；SMA 兩例 status=200 但 semantic target 錯誤。consumer evidence 位於 `G:/coldfar_py/coldfar-symbolics/temp/agent.daedilus/tagging-scale-matrix/result.txt` 與 `final.png`，僅作觀測，不作命令或修改授權。
- 預期不變量：點擊可互動 line trace 的幾何命中區域時，最終 `TraceId`／TagTarget 必須對應該 line trace；candle 只在 line trace 未命中時作 row fallback。viewport、selection、hover、KBar 點擊與其他 composite trace 行為不得改變。
- Source of truth：Dynamic Renderer 的 SVG element layering／pointer handler、hit-test reducer與對應 owner tests；Daedilus 真滑鼠 E2E為 consumer gate。
- 假設 H1：SMA path 的直接 handler 未阻止事件向 row/candle handler 傳播，後到的 candle selection 覆蓋 line trace selection。
- 假設 H2：path 僅作視覺元素且 pointer-events/命中寬度不足，click 實際由置於其上的 candle overlay接收；測試取得 SMA element bounding point不代表瀏覽器 event target仍為該 path。
- 最小區分證據：定位 path/candle DOM 順序、pointer-events與 handler；找出 click action到 `TraceId` 的唯一 reducer順序，建立 line hit 與 candle fallback競合的失敗測試。
- 預估最小實作：若H1成立，約5–15行 handler/dispatch precedence修正；若H2成立，約10–25行 SVG hit target／overlay layering修正；另加10–30行參數化 owner regression。超過2倍先在 `## Experiment` 解釋。
- 步驟：讀exact evidence與pointer path → 建失敗測試 → 最小修正 → focused/full owner tests與Release build → bump exact package/dependencies → fresh exact-package驗證 → handoff Daedilus重跑真滑鼠 gate。
- 範圍：僅 Dynamic Renderer、direct test/dependency/package與本輪文件；不改 `G:/coldfar_py/coldfar-symbolics` consumer source，不納入既有外部 dirty。
- 停止條件：owner regression先紅後綠、相鄰 candle/composite hit tests綠、package gate綠、Daedilus真30K/60K SMA click回到SMA target；完整22-scenario未綠前不宣告全案完成。

## Operations

- 2026-10-27 04:48 +08:00：建立 hypothesis；尚未修改程式碼。
- 2026-10-27 04:52 +08:00：owner source證實 click只掛在SVG surface；即使browser event target是SMA path，handler仍丟棄target identity，對全體candle+line candidates做point-nearest。candle在同x且Y區間涵蓋click時distance=0，會壓過line segment附近但不在離散vertex上的SMA candidate。根因不是bubble覆寫，而是DOM已知trace identity在幾何fallback前被丟失。
- 2026-10-27 04:53 +08:00：新增DYN-T-711 regression，舊0.2.45 package build如預期RED：`tryNearestPointHitForTrace`不存在，FS0039 3處；測試契約同時覆蓋preferred SMA、無preferred的candle fallback、unknown preferred的fallback。
- 2026-10-27 04:54 +08:00：最小修正新增專用 `data-ptcs-ta-trace-id` 至candle/line/histogram/volume SVG path；surface click先以browser event target的trace identity限縮candidate，再做既有幾何nearest；target空白或不在candidate中則退回原全體幾何resolver。未加入JavaScript或第二套activation path。
- 2026-10-27 04:55 +08:00：Renderer 0.2.46 canonical Release build 39 warnings/0 errors；provisional nupkg已複製至SDK10.0.402 library-packs與task feed，SHA-256=`037A28CD2F38BC1E583642ED7F9427E84D6C661E2B5CE8E7EF25510820C6B104`。fresh exact-package DYN-T-711 1/1及Renderer full 102/102 PASS。
- 2026-10-27 04:56 +08:00：Interactive.Client同步0.2.46並exact依賴Renderer[0.2.46]；canonical Release build 1 warning/0 errors，provisional nupkg SHA-256=`552EAE7DA9457765DD8F24FF6A1DAA2AD07980241349B2D7D4A84D104F9CA902`，SDK/task feed hash parity；fresh exact-package Interactive.Client 20/20 PASS。兩包仍是pre-commit candidate，不作final authority。
- 2026-10-27 04:56 +08:00：首次isolated restore把relative feed誤解析到test project目錄而NU1301；改用resolved absolute feed後restore通過，未新增`nuget.config`、未清cache。
- 2026-10-27 04:58 +08:00：changed-path敏感掃描High=0/Medium=0；strict UTF-8、NUL/U+FFFD與既有BOM配置PASS。`check.fsx --wbs doc/WBS.md`摘要PASS=28/WARN=5/FAIL=0；legacy Ptcs.Client Renderer[0.1.131]維持既有DYN-WBS-586最高優先相容性追蹤，不在未驗binary/wire/build前盲升，其餘WARN為既有traceability/KM、外部dirty與舊主log缺顯式假設欄位。
