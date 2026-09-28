# RFC-0030 Loaded Coverage Projection 假設

- 日期：2026-09-28
- Repo：`C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic`
- Branch：`20260915_033.ptcs_group_support`
- 上游證據：`G:\coldfar_py\coldfar-symbolics\doc_new2\RFC\RFC-TRADECORE-0030.PTCS-LoadedCoverageProjection.Handoff.md`

## 現象

真 SPAA initial 250 bars，prepend 250 bars 後 active reference 變成 500。按 Earlier 正確固定頁應為 `Viewing 1-250`，現行是 `Viewing 189-438`。同時 consumer 的 run cap 250 未進 Renderer，且 navigator move 越過 active detail 邊界後只會 clamp，不會發布 adjacent intent。

## 核心假設

1. `panWindow (-visible.Count / 4)` 與 `tryReanchorWindow delta`共同造成 250-bar page 只位移 62 bars；改為完整 `visible.Count` 可直接消除 `189-438`。
2. `options.MaximumVisibleBars`是唯一 cap authority，document 的 per-run 250 無 typed/validated seam；需新增 document-first resolution。
3. `draftWindow`只保存 clamp 後結果，mouseup 無法知道 move 曾越界；需保存 gesture 的 raw delta／global coverage window，並讓 button 與 drag 共用單次 adjacent intent。
4. base axis 同時被當 active detail 與完整 coverage authority，無法在 4,000 retained cap 下表達多年 coverage；需分離 bounded active detail、coverage ordinal/segments 與 bounded overview anchors。

## 實驗與驗收

- Pure：250→500 prepend，Earlier 固定頁得到 1-250；document cap 250 優先於 host 4000。
- Contract：coverage projection保留gap、revision、query generation、global ordinal，invalid shape fail closed。
- Browser：button／drag越界各只送一次相同 direction/count 的 intent；selection使用coverage ratio，detail仍bounded。
- Cache：adjacent lookup可區分 Hit／KnownEmpty／Miss／Unavailable，並依 temporal adjacency而非 touched time選擇。

## 預估

非型別宣告實作約 240 行；若超過 480 行，需回到本檔補充複雜度原因與拆分方案。

## 結果

- 假設1-4皆成立；最小通用解不是提高retained cap，而是新增validated loaded-coverage projection、versioned intent與cache temporal adjacency。
- active-detail projection改變必須以candidate data完整取代prepared map；同projection live patch才可incremental merge。
- Browser gate另發現舊navigator action在新document建立後才完成。authority若不比對`ExpectedDocumentRevision`會把舊page append回新page；正式contract已有revision，本次把demo改為回`RevisionConflict`，沒有新增協議。
- Fresh gate最終為coverage=500、active reference=250、visible=251-500；Earlier/Later與tiny gesture共用同一window authority。
