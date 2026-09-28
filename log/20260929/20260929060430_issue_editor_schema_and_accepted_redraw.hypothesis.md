# Editor schema authority and accepted redraw hypotheses

## 現象

- 真SPAA document `EditorSchemas=[]`，因此不呈現row Edit。
- 200→All低於2秒且無>100ms long task，但chart render sequence由10增加至12。

## 核心假設

1. Schema authority：Edit只在consumer author `EditorSchemas`時成立；Renderer僅依capability render，不能補造schema。
2. Redraw：Interactive client將server accepted的等價viewport再次送入Renderer，Renderer未辨識與local immediate committed viewport相同，造成第二次重畫。

## 驗證方式

- 搜尋schema contract與fixture authoring位置。
- 搜尋viewport command local apply、accepted callback與render-sequence instrumentation。
- 以等價ack與不同authority correction做對照測試。

## Experiment

- `EditorSchemas=[]`符合consumer/document capability ownership；Renderer不得合成，故不列為owner bug。
- `DocumentRevision`與loaded coverage的`CoverageRevision/QueryGeneration`單獨前進時，document presentation、active detail、rows與data未變；舊判斷仍replace shell，驗證假設2。
- 修正後純revision與counter-only case重用shell；active-detail ordinal變更仍replacement。Browser gate等待accepted callback settled後render恰好`+1`，資料替換仍更新五列。
- 結論：假設1、2成立。Release仍須official NuGet readback與Daedalus真SPAA consumer gate。

## Correction：rapid intent race

Official `0.1.94/0.1.85/0.1.95`只關閉已settled accepted ack；真SPAA證明舊48-bar action仍在flight時，較新local 200／All之後抵達的stale returned document會觸發第二次render。正確不變量是latest local viewport intent優先，不是要求trader等待舊callback。

待區分假設：

1. stale host frame修改了`DefaultView.visibleBars`等只供fresh canvas初始化的presentation default，semantic comparator不應把它當current viewport topology。
2. callback/frame republish在Renderer queue之外先發布完整舊document；需以viewport intent generation fence stale document projection，而非擴大semantic ignore範圍。
## Correction: true SPAA delayed projection

- 本機以 Daedalus 同一 `Rfc0026.Spaa.Backtest.playwright.fsx` 重現 `renderSequence=10 -> 12`。最後兩個 action body 是 `ui:3 ChangeQuery` 與 `ui:4 VisibleRangeChanged(48)`；`200/All` 尚未送到 server，`ui:4` HTTP 200 且 `frames=0`。
- 公開 `data-chart-document-revision`／`data-chart-data-revision` 綁的是最新 `runtimeState`，不是已完成 row projection 的 `chartRuntimeState`，所以 before/after 都顯示 `3/3` 無法證明 revision 3 已繪完。
- `ChangeQuery` response 可先發布 runtime revision，再由 Renderer 的 scheduled preparation/row barrier 完成 projection。500ms render quiescence不是 RFC-0026 projection commit；晚到的 revision 3 render是新 query authority的必要繪製，不可因 viewport latest-wins 而抑制。
- 原先「accepted no-frame viewport action造成第二次 redraw」假設被反證。正確 consumer gate須在開始48/200/All量測前等待 typed projection commit receipt 達到該 query 的 document/data revision；action HTTP response、render quiescence或runtime diagnostic attrs均不可替代。

## Progressive coverage navigator hypothesis

- 真SPAA在toolbar earlier成功後立刻把完整selection拖過左界，action count維持不變。source顯示`startNavigatorDrag`以`viewportCommandsDisabledNow`拒絕pending action期間的mousedown，`requestAdjacentCoverage`也以`commandsDisabledNow`丟棄release intent。
- 修正不另造第二套boundary protocol：navigator local drag不受`PendingActionId`阻擋；remote intent與preset共用單槽latest-wins queue。後來的preset或boundary intent覆蓋尚未送出的舊intent，當前action settled後再依最新runtime/document建立並送出boundary request。

## Progressive experiment result

- BrowserDemo的750ms callback建立deterministic pending window。舊行為不產生第二個action；修正後48-bar action與queued Earlier依序完成，callback count `0->2`、query generation `1->2`、active detail由`453-500`移至相鄰前頁`405-452`。
- Focused verifier連續兩次PASS，完整renderer browser gate亦PASS；因此hypothesis成立。真SPAA仍是consumer acceptance，不由generic fixture替代。
