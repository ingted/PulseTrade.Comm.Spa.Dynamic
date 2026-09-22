# RFC-PTCS-DYNAMIC-0020 Time axis、crosshair 與 progressive coverage

- ID：RFC-PTCS-DYNAMIC-0020
- 狀態：Accepted / Implemented
- Owner：PTCS Dynamic Contracts／Renderer／Client
- Consumer feedback：`G:\coldfar_py\coldfar-symbolics\doc_new2\RFC\RFC-TRADECORE-0026.PTCS-TimeAxis-Crosshair-ProgressiveCoverage.Feedback.md`
- Parent：RFC-TRADECORE-0026

## 背景

目前只有 canvas 最後一列顯示三個 time labels，shared crosshair 的線段也沒有精確使用 plot top/bottom。`MaximumVisibleBars`雖已限制 window，pan 到 loaded boundary 時卻只 clamp，無法要求 consumer 延伸 authoritative coverage；prepend merge 亦可能因 index 改變而讓 viewport跳動。

## 目標

1. 每列都以實際 event-time 顯示不重疊的 adaptive time axis。
2. shared cursor 在每列完整覆蓋 plot bounds，resize／不同 row 高度後仍正確。
3. loaded temporal domain 可大於 `MaximumVisibleBars`；越界 pan 對稱要求 adjacent coverage，authoritative merge後維持使用者 pan intent，visible count永遠不超過 cap。
4. contract維持通用，不引入 FSSTL、DMI、MDCQ、交易所日曆或 backtest type。

## 非目標

- Renderer 不查資料、不推算交易日、不填補 gap，也不自行突破 compiled source authority。
- PTCS 不擁有 provider merge；source identity＋event-time去重、cache與查詢由 consumer負責。

## 情境

1. 60K長區間與5K短區間同時可見時，每列都能直接讀出自身時間尺度。
2. 使用者在3,820筆loaded domain向右越界，consumer補入400筆後overview擴至4,220，但主圖最多顯示4,000。
3. prepend使array index整體位移時，畫面仍停在同一event-time附近；較晚到達的stale response不回捲current pan。

## 取捨

本設計復用`VisibleRangeChanged`，避免新增第二套coverage action；代價是consumer必須把同一action解讀為authoritative range request並發布RuntimeFrame。Renderer只保存短暫pending intent，不保存provider cursor或query cache。每列各有axis會增加少量bounded DOM，但tick數依pixel budget受限，換得跨尺度row可獨立判讀。

## 決策

### Row-local adaptive axis

- 每列產生自己的 axis DOM；tick x 由該列採用的 reference event-time slots決定，不用 browser local timezone轉換。
- tick count依實際 row width與最低 label spacing計算，以 bounded evenly-spaced indices選點；label保留 timestamp文字中的日期／時間語意。
- 1600px 下約 60K×4000 必須能辨識日期，5K×300 必須能辨識小時；窄 viewport自動減少 tick，不重疊。

### Cursor bounds

- cursor `y1=plotTop`、`y2=plotTop+plotHeight`；不延伸進 header/legend。
- 每次 row Doc重建都從該 row kind/trace topology重新計算，不沿用另一列或舊 row height。

### Progressive coverage

- `MaximumVisibleBars`只限制 visible base bars；`data-loaded-bars`與 overview使用完整 loaded domain。
- 使用者在 loaded 左／右 boundary繼續 pan 時，Renderer沿用 `VisibleRangeChanged`，不新增 domain action：
  - Earlier：`[document.DefaultView.query.fromUtc, loadedFirstEventTime)`；
  - Later：`[loadedLastIntervalEnd, document.DefaultView.query.toUtcExclusive)`；
  - `MaximumBasePoints=MaximumVisibleBars`。
- query boundary缺失、範圍無效或該方向已達 authority boundary時 fail closed並顯示 feedback；不得用固定分鐘數猜範圍。
- boundary intent記錄 generation、舊 loaded first/last、舊 visible event-time anchor與 pan delta。authoritative data擴張後，只有符合該方向的 coverage change可套用 intent；舊／反方向 response可擴張 loaded cache，但不得回捲最新 viewport。
- 一般 prepend merge以舊 visible start event-time重新定位；follow-latest 在 append 時維持 latest。gap保留，不以 index補 observation。

### Capped maximum preset

loaded count超過 cap後，按鈕顯示 `Max 4000`（實際 cap值），語意為「以目前 anchor/follow-latest顯示最多 cap 筆」，不再宣稱 `All`。loaded count未超過 cap時可顯示 `All`。

## 責任邊界

- PTCS Dynamic：axis layout、cursor bounds、boundary intent、viewport re-anchor、latest-intent protection、owner tests。
- Daedalus consumer：compiled FSSTL authority range、provider query、source identity＋event-time merge、cache與真 SPAA E2E。

## 影響

- Renderer與兩個client package升版；Contracts wire不新增domain type。
- consumer需處理左右相鄰range並合併authoritative frames，否則UI會保留last-good並顯示feedback。
- loaded／visible／overview的測試資料與DOM標記分開，避免把`MaximumVisibleBars`誤當cache上限。

## 驗收

1. 每個 visible row都有 axis；1600px 的 60K/4000 與 5K/300 labels不重疊且分別可辨識日期／小時。
2. 每列 crosshair覆蓋完整 plot，pointer sweep不重建 geometry。
3. 右延伸 fixture由3820增至4220後，overview loaded domain增大、visible<=4000、viewport依pan intent前進；左延伸用純 model test驗 prepend event-time re-anchor。
4. out-of-order/反方向 extension不套用 stale intent；duplicate/gap ownership仍在 consumer，不由 Renderer造資料。
5. owner browser interaction phases無 >100ms task。
