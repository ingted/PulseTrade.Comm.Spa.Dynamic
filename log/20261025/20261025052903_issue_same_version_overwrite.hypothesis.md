# Hypothesis — Renderer / Interactive.Client 0.2.31 same-version overwrite

時間：2026-10-25 05:29:03 +08:00
Owner：agent.asteer

## 現象

Daedilus 的 consumer 已 restore 0.2.31；global NuGet cache 仍保留較早 0.2.31 bytes，而 final feed 的同版 package 已換成 source commit `6d2f885bf94886d82443ab9ca89cbefbf6c80408` 對應 bytes。consumer 因 immutable cache 不會自動刷新，仍以舊 package 執行，no-1K lifecycle focused E2E 失敗。

## 根因與最小驗證

- 根因：不同 package bytes 共用 `0.2.31` identity，破壞 immutable package version 不變量。
- 非根因：consumer cache 未清除；NuGet 對已 restore package 的重用屬預期行為。
- 最小修復：不改功能邏輯，將 final source 的 Renderer、Interactive.Client 與 tests direct dependencies 升為新 stable `0.2.32`，其中 Client exact 依賴 Renderer `[0.2.32]`。
- 修後驗證：fresh isolated package cache restore/build/test；核對 package SHA、nuspec version／RepositoryCommit、DLL ProductVersion 與 exact dependency；consumer 不清 cache、直接 bump 0.2.32 後重驗。

## 變更估算

預計非型別宣告程式邏輯變更 0 行；只有 project metadata/version references 與追溯文件。

## Result

- Renderer／Interactive.Client與兩個owner test project已改用0.2.32；無功能程式碼或wire contract變更。
- Source commit `f89e5083574c9b131caace522f9a2b59353657ca`已push；兩包RepositoryCommit／DLL ProductVersion與該commit一致，Client exact Renderer `[0.2.32]`。
- Fresh isolated package cache實選0.2.32，Renderer 94/94、Client 18/18及package verifier全通過；consumer未清shared cache，待Daedilus direct bump後真18884重驗。
