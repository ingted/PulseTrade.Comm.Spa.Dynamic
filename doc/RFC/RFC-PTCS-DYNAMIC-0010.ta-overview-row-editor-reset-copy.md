# RFC-PTCS-DYNAMIC-0010 TA Overview / Typed Row Editor / Reset / Copy

- Status：Accepted / DEV authorized
- Date：2026-07-15
- Owners：PTCS.Dynamic Contracts / Renderer / PTCS adapter
- Supersedes：RFC-PTCS-DYNAMIC-0009「不增加thumbnail navigator」的非目標
- Related：`doc/TAResearch/REQ.md`、`doc/TAResearch/WBS.DYN-TA-014.md`
- PTCS companion：`G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\doc\RFC\RFC-PTC-SPA-0021.reply-presentation-actions.md`
- Host companion：`G:\PulseTrade.fs\Libs\PulseTrade.Comm\doc\RFC\RFC-PTC-PTCSH-0003.TAOverviewRowResetCopy.md`

## 背景

目前single-thumb range只可移動固定48-bar viewport，無法用2000根觀察長趨勢；Add Row editor在runtime revision重建時收合，且只送kind/dataRef；Reset Canvas從已修改狀態重建，無法復原被移除row。Collapsed reply也缺少取得canonical SDUI JSON的操作。

## 目標

1. 顯示E2EQ精神的loaded-range OHLC overview，selection具有左右把手與可拖動區域。
2. selection可從minimum bars擴至actual loaded count；拖曳只改draft，release只commit一次。
3. 2000-bar full-range主圖使用deterministic pixel-bucket aggregation，不建立2000組bar DOM，仍顯示完整時間跨度。
4. Add Row editor不被poll/revision關閉，並依kind顯示typed參數。
5. Reset Canvas恢復initial query/rows，再要求fresh snapshot；Reset View仍只改browser viewport。
6. TA reply提供copy action，複製extension解出的canonical SDUI JSON。

## 非目標

- overview不是第二套fetch、paging或storage authority。
- 不新增IndexedDB/PCSL的TA series副本；browser reduced state持有目前loaded snapshot。
- 不修改E2EQ；只參考interaction model。
- 不在renderer直接查SQL/provider。

## 決策

### Loaded、overview、viewport

`RuntimeState.Data`是browser loaded authority，最多既有2000 points per series。overview從reference OHLC series做bounded min/max/open/close bucket projection，primitive數量受renderer width target限制。selection以`StartIndex + Count`表示，left/right/move各自產生draft window；pointer release才更新committed window與render sequence。

當`Count`大於detail primitive budget時，主圖每個pixel bucket保留OHLC envelope，line/histogram保留first/last/min/max語意。UI標示`compressed N bars`，不能假裝逐bar寬度。cross-row cursor以bucket代表range與聚合值顯示。

### Typed Add Row

`TaRowSpec.Options`使用canonical keys：`period`（SMA/DMI）、`diPeriod`與`adxPeriod`（ADX）、`fastPeriod/slowPeriod/signalPeriod`（MACD）。editor draft Vars不放在runtime-state `View.Map`重建邊界；kind切換只換欄位與kind default，不關閉editor。row id使用kind + monotonic suffix，允許remove後重新加入。

### Reset

Renderer只送`ResetCanvas`。initial/current command的分離與fresh snapshot由Host companion RFC負責；成功snapshot後renderer以document default view重設local viewport與hidden rows。

### Copy JSON

Dynamic presentation action取得`extractReplyPayload`後的canonical payload，透過PTCS typed action slot與typed clipboard API複製。action不mount Canvas、不開channel、不poll。

## 驗收

1. loaded=2000時overview呈現完整range；left/right/move均可操作，release前chart render sequence不變，release後恰增1。
2. 48、200、2000 visible modes皆可達；2000模式顯示完整from/to且DOM/primitive bounded。
3. SMA/DMI/ADX/MACD editor欄位與wire options正確；慢速操作跨poll不收合。
4. Remove DMI/MACD後可重新Add；Reset Canvas恢復initial ordered rows/query。
5. copy action位於`展開`旁，clipboard parse為相同canonical SDUI JSON，presentation mode與remote action count不變。
6. pure WebSharper F#，無handwritten JavaScript。
