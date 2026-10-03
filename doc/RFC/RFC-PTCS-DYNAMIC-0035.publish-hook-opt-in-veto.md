# RFC-PTCS-DYNAMIC-0035: AfterPack 專屬開關與全域否決

- 狀態：Proposed；等待 root 確認 conditions 後才修改 project。
- 背景：註解的 opt-in 未生效；既有 CLI global false 不能阻擋尚未使用它的 targets。
- 目標：八個 project 的九個發布 targets 都要求既有專屬 flag=true，並受 PublishNuGetAfterPack=false 否決。
- 非目標：更改 VS 發布預設政策、版本/reference、publisher 內部、NuGet 發布、產品/runtime。
- 決策提案：原 Condition 保留，append `and '$(PublishNuGetAfterPack)' != 'false'`；umbrella 兩 target 再加 `and '$(PulseTradeCommSpaDynamicPushNuGet)' == 'true'`。七個 VS auto-true defaults 保持；umbrella 不新增 default。
- 情境：VS既有 auto-true 可維持，但明傳 per-package=false 或 global=false 必須阻擋；CLI umbrella 空 flag 不執行。
- 取捨：不以此根因修復擴大 VS UX 政策；global true 本身不代替專屬 opt-in。
- 驗收：DYN-VFY-051r1 使用真 MSBuild 的隔離 marker unit；原 Exec 從 fixture 完全移除，沒有 real Pack/restore/build/key/network side effect。
- 關聯：DYN-WBS-584、DYN-T-679；本輪 log/hypothesis 見 Verification 意圖。

## Accepted 2026-10-03T20:37:03.3649550+08:00
root 確認上列 exact contract；七個 VS auto-true 保留，umbrella 不新增 defaults。僅八專案九條 gate，版本/reference/Exec 不改。
