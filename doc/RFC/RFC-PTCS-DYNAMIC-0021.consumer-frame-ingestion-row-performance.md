# RFC-PTCS-DYNAMIC-0021 Consumer frame ingestion 與 row geometry 效能

- ID：RFC-PTCS-DYNAMIC-0021
- 狀態：Owner implemented / Consumer acceptance pending
- Owner：PTCS Dynamic Contracts／Renderer／Interactive.Client
- 關聯：RFC-PTCS-DYNAMIC-0019、RFC-PTCS-DYNAMIC-0020、`log/20260923/20260923023248_issue_renderer-consumer-long-task.hypothesis.md`

## 背景

Daedalus以final exact package graph在真FSSTL／SPAA workload量得3,819個1K points x 7 rows：initial 893–914ms、48→All 116–119ms、backtest/accounting replacement 135–168ms。pointer p95低於50ms且功能契約通過，表示RFC-0019解掉owner fixture的row scheduling後，client收 frame 的驗證配置量與row geometry中間資料仍可形成long task。

另依stakeholder最新驗收，cross-row vertical cursor須覆蓋每個TA row的完整SVG高度；RFC-0020的plot-only決策在此被明確修訂。

## 目標

1. 合法大型RuntimeFrame的unsafe scan不建立逐節點diagnostic path/list；只有發現unsafe subtree才建立精確錯誤。
2. Renderer避免每列重複建`Map`、逐slot lookup、每根candle中間陣列與八次candle path全掃。
3. shared vertical cursor覆蓋每列完整SVG高度，所有列仍共享同一X/bar identity。
4. owner exact-package browser phases維持無大於100ms task；真consumer相同workload重跑後才關閉本RFC。

## 非目標

- 不改wire schema、provider authority、document/data revision、one-in-flight action或4000 visible cap。
- 不把FSSTL formatter、MDCQ query或consumer host lifecycle搬進owner package。
- 不用partial reducer state、跳過validation、提前ACK或丟資料換速度。

## 決策

### Runtime validation fast path

`unsafeValue`先用不帶field path的allocation-light traversal回答是否含unsafe key/text。安全資料直接通過；只有命中unsafe subtree才執行既有精確collector，維持error code與field path。temporal/schema validation仍照常執行，不能用fast path略過。

### Renderer geometry

- candle source interval直接用`candleSlotRange`寫入單一buffer，不為每根bar建立slot array再flatten。
- line cursor lookup使用reference-slot indexed option array，不先建`Map`再逐slot查找。
- 每個candle trace單次掃描即分流八種normal/projected、up/down、wick/body path，維持原順序、顏色、Y-domain與source span語意。

### Cursor row height

每列shared cursor的`y1=0`、`y2=row SVG viewBox height`。它覆蓋該TA繪圖列的完整可視SVG，但不延伸到SVG外的toolbar、editor或其他row。resize/topology重建仍取current row geometry。

## 影響

- Contracts、Renderer、Interactive.Client、Dynamic.Ptcs與Ptcs.Client升exact版本；active demos/tests同步。
- 只有內部驗證與projection配置策略變動，wire/cache schema不變。
- consumer必須採同一exact graph後重跑，舊package證據不得冒充本RFC完成。

Owner release graph：Contracts `0.1.14`、Renderer `0.1.38`、Interactive.Client `0.1.31`、Dynamic.Ptcs `0.1.39`、Ptcs.Client `0.1.54`。五包已發布；RFC仍等待真consumer CDP gate，不以owner fixture代替。

## 驗收

1. 4,000-point合法nested snapshot無unsafe errors；nested unsafe key/text保留精確code/path。
2. Renderer focused suite、PTCS/Interactive client suites全綠；package manifest與exact refs一致。
3. 3,820→4,220 x 7 owner browser phases無大於100ms task；cursor跨列完整SVG高度且無chart rerender。
4. Daedalus真3,819 x 7 consumer重跑initial、48→All、backtest/accounting；若仍有大於100ms task，以CDP child attribution指出owner或consumer責任，不以generic fixture替代。
