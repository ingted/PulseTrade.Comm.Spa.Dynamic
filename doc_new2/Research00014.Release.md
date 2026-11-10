# Research 00014：跨回覆拖曳

2026-11-10，agent.daedilus。

主目標為修復 TA 圖 pointerup 後時間區間偶發跳到錯誤位置。只操作隔離 18882；18883 人類驗證與 18884 正式版本保留。

修正捕捉時間軸／目前 resident 的 local index 混用，所有捕捉 gesture 按 UTC 重映射，送出時更新 coverage revision。較舊 resident 不含最新反向拖曳的完整區間時保留正確畫面；接受 transport 進度後喚醒最新 queued intent，不偽造 visual commit。窄區間跨邊界使用 active window 在完整 resident 中的原點，避免跳 local zero。

Renderer 新增三個回歸，涵蓋 UTC 等價、缺失目標與窄區間原點。exact local package gate：Renderer 346/346、Interactive.Client 59/59 PASS。原生滑鼠實測 1K、5K、30K 的連續反向／持續按住跨回覆、48 根跨邊界、GLOBAL／4500 根縮圖及非 CURSOR 框選，最終區間符合預覽與操作。完整證據位於 `G:/coldfar_py/coldfar-symbolics/doc_new2/Research00014-00015.Acceptance.md` 及該庫 `log/20261110/00014-*.json`。

官方 bumpVersion.fsx --apply 將 Renderer 0.3.53→0.3.54、Interactive.Client 0.3.53→0.3.54，限定兩 producer 與直接 tests；未改 Contracts、FSharp.Core 或其他基礎套件。先 stage producer local nupkg 再同步 consumer，避免要求不存在版本。lib-packs.txt 指定 SDK 10.0.402；目前 SDK 自動 source 為 10.0.403，因此僅本次 restore 以 RestoreAdditionalProjectSources 指向既有 10.0.402 feed，沒有新增 nuget.config 或改系統來源。

Source checkpoint／公開發布狀態另記；本文件不宣告 GLOBAL 延遲及完整歷史回測完成。
