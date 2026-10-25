# Hypothesis: navigator stale draft across authoritative frame replacement

## 已證實事實

- 0.2.36真consumer初始maximum為`Loaded1484 / View1-1484`；right shrink後`View1-595`，right grow後`View1-862`。
- 隨後body向右143.6px時，晚到的authoritative `$=8 VisibleRangeChanged`把loaded domain替換成862；畫面永久停在`Loaded862 / Preview149-1010 / release to render`。
- selection ratio為`0.09973..0.68059`、`moveSettled=false`，browser/bad response皆0；因此不是pointer lifecycle或transport error。
- Horizontal cursor line已有row-local Y與leave hide，但左右Y軸沒有同值浮動label。

## 預期不變量與最早偏離點

- Gesture只能提交基於同一authoritative frame/domain的draft；若frame在gesture中變更，必須以latest projection重建或明確取消並清draft。
- Pointer release/cancel、action rejection或authoritative settle後，DOM不得永久保留Preview。
- Shrink→grow→move-right成功時visible count不變、start增加。
- Row horizontal pointer Y須映射為同一value，左右Y軸label同值、同Y、leave hidden；label只作presentation，不成為資料authority。

## 候選原因與區分實驗

1. `startNavigatorDrag` closure凍結down時total/resident/visible，而runtime/frame更新只替換projection，不取消或重建active gesture。檢查document listeners、draft state與projection settle/cancel seam；用frame token/domain change regression重現。
2. Action settle只清pending coverage intent，未清gesture-owned preview state。追蹤所有release/cancel/success/failure路徑，要求每條terminal path清draft。
3. Y label可復用既有row low/high及SVG viewBox Y interpolation；若目前horizontal line只有DOM mutable element，需以同一mousemove更新左右label而非另建cursor state。

## 實驗停止條件

- 修前focused model/browser gate明確RED；修後同一shrink→grow→move-right與frame replacement案例GREEN。
- 實作若超過最小gesture token/domain guard與label presentation seam，先回查是否誤把consumer/backend責任移入Renderer。

## Experiment

- Daedilus v4反證「GLOBAL/resident authority縮成862」：authority仍為total 2,996,994／resident 7,420，變動的是active detail與本地prepared timeline。舊inner gesture保留1484-domain的absolute draft；前一action完成後queue沒有latest-domain semantic可重建，且無法dispatch／拒絕時只改Feedback，故draft永久存活。
- 修復讓blocked resident gesture只保存`drag + pointer delta ratio`；flush時以latest committed projection重算。resident mapping可用時走resident intent；no-1K跨軸映射不可用時以latest active-local domain重算delta，經既有typed pan coverage intent產生較新absolute start並保留count。
- 每一latest-projection拒絕路徑統一`rejectNavigatorDraft`清除draft與ratio override；replacement chart-stack重掛queue diagnostics，避免只看old DOM node。
- Y label不新增authority：直接以row既有low/high及同一SVG pointer Y線性插值，單次mousemove同步更新左右label，mouseleave同步隱藏。
- 驗證：source-project Expecto 96/96；staging full WebSharper Release Rebuild成功（39既有warnings、0 errors）。同一真consumer pointer驗收待0.2.37 immutable package交給Daedilus。
- 0.2.37真consumer反證：latest semantic rebase確實送出較新typed target，但action callback完成、projection committed ready後，`pendingBoundaryPan`仍存在。原因是projection generation已達pending generation而absolute request無法由該active frame完整reanchor時，現碼把`None`同時表示「尚待frame」與「terminal frame拒絕request」，缺少action-settled狀態區分。
- 下一實驗：引入純settlement DU（Await／Reanchor／Reject）；generation未達或action仍pending時Await，完整包含時Reanchor，generation已達且action已settled但不包含時Reject。projection先到／action先到兩個順序都須走同一terminal判定；Reject必清pending與Preview，不能假造不存在的visible data。
- Daedilus final F23再反證單純terminal reject：request authority是GLOBAL `axis.1k` ordinal 2,989,722，而authoritative ActiveDetail已切為`axis.5k` ordinal 600,240；兩者ordinal不可直接比較。final active count與DOM local visible皆862，證明host已交付cross-axis target。
- 修正settlement不變量：matching generation下，若GLOBAL／active axis不同且final resident涵蓋requested GLOBAL window，Reanchor至final authoritative active-local window；同axis仍走exact ordinal containment；不涵蓋時依action pending判Await／Reject。新增真值同構case修前95/96、修後96/96。
- 0.2.38 Run A/B時序反證：cross-axis terminal settlement本身已能清pending，但settle後renderer只保留active-local window0／862，沒有保留request在原1484 navigator domain的位置。下一gesture因`tryResidentNavigatorWindow`直接比1K／5K ordinal失敗，fallback以5K start600240產生601102；與GLOBAL resident start2989574矛盾。
- 下一實驗：accepted cross-axis request建立typed navigator authority：CoverageIdentity、GLOBAL resident scope、`OverviewAnchors.Length` domain與`targetStart-residentStart` active-detail base offset。所有render／label／pointer／queued gesture先用同一validated authority；local window只在base offset上投影。scope或resident變動時fail closed。Run B oracle為settle後仍Loaded1484/window0-862，右移148後GLOBAL target2989722，不能出現601102。
- 實驗結果：`TaResidentNavigatorAuthority`只在accepted cross-axis settlement建立，identity／resident start／resident count／domain count不符即失效；render range、selection、pointer baseline及延後dispatch均從同一authority投影。真值同構Expecto覆蓋base0與move-right base148，並拒絕active-axis601102；source與candidate package皆96/96，Client19/19，完整WebSharper與exact-package BrowserDemo 0 errors。剩餘驗收是Daedilus以human-paced真consumer重跑0.2.39 immutable package。
