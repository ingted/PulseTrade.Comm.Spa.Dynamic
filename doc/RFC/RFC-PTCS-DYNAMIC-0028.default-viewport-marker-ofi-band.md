# RFC-PTCS-DYNAMIC-0028：Default Viewport 與 Marker OFI Band

- ID：`RFC-PTCS-DYNAMIC-0028`
- 狀態：Released
- Owner：PTCS Dynamic Renderer／Interactive.Client（Aster）
- Consumer：PulseTrade.Comm.Spa.Dynamic.Interactive.Extension／SPAA（Daedalus）
- 關聯：`RFC-PTCS-DYNAMIC-0022`、`RFC-PTCS-DYNAMIC-0024`、`RFC-PTCS-DYNAMIC-0027`、`DYN-WBS-568`、`DYN-T-615..618`、Daedalus message `msg-fsi-55dcc87ce84f4ba88405201207acbf92`

## 背景

真 SPAA 需要文件設定的初始可視 bars 成為 authority，而不是 Renderer 固定顯示 48 根。舊 `RFC-0022` 又把 marker label 畫在 plot glyph 旁；在多成交、縮放及多列 shared cursor 下，文字會遮住 K bar。Daedalus 確認 marker label 是既有 `TaMarker.Label/Tooltip` 的 presentation，不新增交易 domain contract，並要求每列在 cursor gutter 與 plot 之間保留固定高度的 OFI band。

## 目標

1. 初次 application 讀 `DefaultView["visibleBars"]`；合法正整數取其值，並以 loaded bars／renderer max clamp。缺值或不合法維持相容 fallback 48。
2. Plot 只繪 marker glyph 與 `<title>` tooltip，不在 glyph 旁繪 inline label。
3. 每列固定保留 24 CSS px OFI band，順序為 row title／values、cursor date-time gutter、OFI、plot。
4. Shared cursor 所在 slot 的 marker labels 投影到該列 OFI band；同時最多直列四筆，其餘顯示 `+N`，無事件時保留空白高度。
5. Overview selection 使用淺灰；左右可見 boundary 使用亮綠、精確 2 CSS px，transparent 8-unit hit target 與 drag math 不變。

## 非目標

- 不新增 Signal／Order／Fill／BUY／SELL 等 domain DU。
- 不改 marker wire、Label、Tooltip、shape、anchor、fill 或 color contract。
- 不把 OFI presentation 寫回 document、cache 或 provider。
- 不因 cursor 移動重跑 projection、重建 chart 或送 provider action。

## 決策

### Initial viewport

`visibleBars` 只在新的 canvas application 初始化時解析一次。值須為 finite、positive、integral number；resolved count 為 `min(requested, loadedCount, rendererMaximum)`。同 canvas 的 user viewport、navigator drag 與 data patch 不得被 default 重設。

### OFI projection

Renderer 從已接受的 marker placements 建立 slot-indexed reader。Pointer hot path沿用 single-rAF latest-wins，只更新既有 OFI DOM：

```fsharp
type TaMarkerCursorItem =
    { MarkerId: string
      Label: string
      Color: string
      Tooltip: string
      EventTimeUtc: string }

markerCursorItems slotIndex placements
```

排序固定為 trace order、lane、marker id；空白 label 不產生 item。前四筆以 compact item 呈現，總數超過四筆時另外顯示 `+N`。完整 bounded Tooltip 留在 item title；plot marker `<title>`仍保留。一般 plot hover 依 row axis 解析 slot；直接命中 marker glyph 或 overflow item 時，Renderer 以 accepted `TaMarkerPlacement.SlotIndex` 作 exact authority、更新 shared cursor 並停止事件冒泡，避免高密度下多個 reference slots 落在同一 CSS pixel 後再由滑鼠 X 反推到相鄰 slot。

### DOM contract

- OFI band：`ta-row-ofi-band-{rowId}`，`data-fixed-height="24"`。
- compact item：`data-ta-row-ofi-item-index="0..3"`。
- overflow：`data-ta-row-ofi-overflow="true"`。
- inline plot label：不得存在 `[data-testid^='ta-marker-label-']`。
- overview visual boundaries：`ta-overview-left-handle-visual`／`ta-overview-right-handle-visual`，`#4ade80`、2 CSS px。
- overview selection：`rgba(203,213,225,.20)`。

## 失敗與相容性

- 缺少／錯誤 `visibleBars` 不拒絕 document，回 fallback 48。
- cursor slot 無 marker 時 OFI 清空內容但不收合高度，避免 plot 跳動。
- stale render generation 不得更新 OFI；canvas disposal 清除 reader 與 DOM references。
- 舊 consumer 不提供 marker label 時 glyph／tooltip與空白 OFI仍正常。
- `RFC-0022` 的 inline marker label presentation 由本 RFC 明確取代；其 row-local OHLCV data window 與 cursor timestamp contract不變。

## Package Graph

- `PulseTrade.Comm.Spa.Dynamic.Contracts 0.1.29`
- `PulseTrade.Comm.Spa.Dynamic.Renderer 0.1.76` exact `Contracts [0.1.29]`
- `PulseTrade.Comm.Spa.Dynamic.Interactive.Client 0.1.67` exact `Contracts [0.1.29]`、`Renderer [0.1.76]`

## 驗收

1. Pure tests 驗合法／缺少／錯誤／超量 visibleBars，以及 marker slot deterministic items、空 slot 與大於四筆。
2. F# Playwright fresh canvas 驗 4,000 loaded bars 初始 `Viewing 1-4000`，再切 48 進行既有 regression。
3. Playwright 驗 OFI 固定 24px、位於 cursor gutter 與 plot 之間；單筆、64筆 `+60`、空 slot及無 inline marker label。
4. 高密度回歸使用超過3,000個 reference points，先證明 marker slot 與相鄰 slot 落在同一 CSS pixel，再以真實 glyph hover 驗 shared cursor／OFI 精確選回 marker slot與事件。
5. Overview initial／drag 後 selection與2px boundary contract通過，hit target維持transparent／width 8。
6. 4,000-bar browser acceptance phases無 >100ms owner long task，console/page error為零。
7. Daedalus 以真 SPAA clean cache驗 initial visibleBars、OFI內容、marker glyph/tooltip與overview palette；GREEN 後才 public push/readback。

## Owner Evidence（2026-09-26）

- Renderer focused suite `48/48`；Interactive.Client focused suite `12/12`。
- fresh source-identical WebSharper Release builds：Renderer `0.1.76`、Interactive.Client `0.1.67`、BrowserDemo全部成功。
- F# Playwright：fresh viewport 4,000；相鄰 slot 同 CSS pixel 的 direct marker exact-slot、single/dense/empty OFI及overview contract通過；five-candle/scenario/All/marker/document/progressive max=`72.54/79.73/36.53/41.93/38.27/40.59ms`，acceptance phases `over100=0`。
- Interactive package verifier通過，bundle manifest與nuspec版本一致。
- Release SHA-256：沿用已凍結且未變更contract的Contracts `7A59C0A671E5E995B88D1BCD23F074D2ADC0280D6DFD9EC09C354208D51FFC9E`、Renderer `F7A27C19BD48732B82B291BFB285C2F6AA573873418448AB8A8B203348F23AB0`、Interactive.Client `45052F853CEBFAE0711595D62463F2CD51D998A1DD656AECAD421D95E4D7D9FC`。Contracts同版後重打包的`A93F...`不得交付；中間`0.1.75/0.1.66`不得交付。
- Daedalus final exact-identity真SPAA 3,563-point gate通過：fixed strategy marker/band=`1119/1119`、SMA scenario=`2123/2123`，兩者OFI count=2；fresh 4,000、overview、dark/TA colors皆通。
- Renderer `0.1.76`／Interactive.Client `0.1.67` public push均回`Created`。Official signed nupkg SHA-256為`96B9D779411E115FC1A0DFCCD2EACA2F22676133165C190A671DC1EC28164376`／`90C070B7D0142EE2D9F0EA1F1744CB410ABFE36B24A3ABAC7C5A41E77D89657E`；排除repository signature後local／official entries `Different=0`。

## High-density Consumer Correction（2026-09-26）

第二次真SPAA證據使用3,563 points，實際glyph為slot 1119，但瀏覽器X座標經plot snap落到slot 1118，OFI count=0；相鄰reference slots可共享同一CSS pixel，因此只靠X座標不能還原已命中的marker identity。先前「consumer測法錯誤」的forward correction未提交且由此反證取代。Renderer保留單一shared cursor state，但直接glyph／cluster hit以accepted placement exact slot優先；一般plot區仍走axis snap。中間Renderer `0.1.75`／Interactive.Client `0.1.66`已凍結後又新增高密度fixture，故退休而不換同號bytes；canonical candidate升為`0.1.76/0.1.67`，等待真SPAA gate。
