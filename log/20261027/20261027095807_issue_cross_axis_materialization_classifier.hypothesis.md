# Issue hypothesis: cross-axis materialization is coupled to timeline substitution

## 已證實事實

- 0.2.54 Gate B ui:4 event-time range正確、target local0，但wire完全沒有`CoverageIntent`，因此server只做既有79-bar continuation並停在local1122。
- `tryCrossAxisNavigatorEventRangeIntent`目前只在`usesCrossAxisNavigatorEventTimeline`為true時附intent；該predicate同時要求GlobalOverview/ActiveDetail跨axis、top-level overview axis等於active axis、overview anchors與active timeline等長。
- 真實投影可由exact prepared active timeline產生正確UTC range，即使overview-anchor substitution不成立；是否需要server materialize與range選自哪條exact timeline是兩個不同決策。

## 最早分歧

D16把「跨axis global domain需要no-ordinal event-time materialization」錯綁到D14的overview timeline substitution validation。當真host的top-level `OverviewAxisRef`不是active base axis或anchors不是一對一時，range正確但intent被省略。

## 最小修正

保留`usesCrossAxisNavigatorEventTimeline`只決定event-time timeline選擇；新增cross-axis materialization predicate，只要GlobalOverview axis與ActiveDetail base axis不同，就為已由prepared range取得的exact UTC window附`ExplicitBounds` no-ordinal intent。same-axis仍不附；不做比例插值。

## 預估實作量

非型別宣告約6行並調整兩個tests；超過12行須追加Experiment。

## 驗證

- RED：DYN-T-721以真host形狀（cross-axis、overview substitution不可用、active prepared range exact）要求materialization intent。
- GREEN：DYN-T-717/721＋Renderer full、Interactive full、verifier、BrowserDemo。
- E2E：Daedilus Gate B wire含`CoverageIntent ExplicitBounds`、StartOrdinal/Direction none、local0 settle與續overscroll。
