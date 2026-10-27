# Blocker

## BLK-PTCS-PKCS-20261027：PTCS active graph仍解析到已知弱點的System.Security.Cryptography.Pkcs

- 摘要：Dynamic.Ptcs 0.2.1與Ptcs.Client 0.2.0 active-consumer同步時，restore/build均對transitive `System.Security.Cryptography.Pkcs 9.0.18`回報`NU1902`與`NU1903`。
- 症狀：來源、兩個Expecto projects及PTCS LiveDemo都可成功restore/build/test，但NuGet audit持續回報moderate與high severity advisory。
- 影響：不影響DYN-WBS-620 Renderer/Interactive功能與Daedilus true-host驗收；但在上游依賴改用無該advisory的版本前，不得宣稱PTCS adapter graph為security-clean，也不把本輪local consumer-sync packages視為已獲准public rollout。
- 決策：不壓掉NU1902/NU1903、不新增`nuget.config`、不以降版處理。本輪保留local source/build/test evidence，Renderer/Interactive正式驗收可獨立繼續。
- 狀態：Open（non-stopping for DYN-WBS-620 consumer E2E；blocking for PTCS adapter security-clean/public rollout claim）。
- Workaround：無。現有warning只作風險揭露，不視為修復。
- 下一步：PulseTrade.Comm.Spa dependency owner確認並升級其PKCS dependency至advisory已解除的版本；重建Dynamic.Ptcs／Ptcs.Client／兩個tests／LiveDemo並要求NU1902/NU1903消失。
- 建立時間：2026-10-27 21:46 +08:00。
