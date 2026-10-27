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

## BLK-DYN-UIHOST-20261028：owner fixture host啟動遭tool policy拒絕

- 摘要/症狀：WBS621已完成source及exact-package146/20/17測試；04:25左右啟動canonical Renderer.BrowserDemo的Start-Process（已帶WindowStyle Hidden）被exec tool拒絕，未建立host process。
- 影響/狀態：Open（owner fixture only）；`verify-ta-main-plot-range-fixture-playwright.fsx`為NOT RUN，不得報該fixture browser PASS。真Research consumer已於0.3.17 MainChartDrag與ResidentWidthAndEdges各1/1 PASS；此限制不否定獨立真consumer evidence，亦未被其消除。
- 決策/Workaround：不改shell/tool/agent繞過被拒的fixture啟動，不請daedilus代啟該host；他原負責的18884 consumer整合與真資料矩陣保持原scope。沒有把compile/fixture/unit冒充真UI。
- 下一步：真consumer evidence已回並核source/deployed一致（DYN-VFY-069）；若之後仍需owner fixture，須先取得合法可用的host啟動機制/授權。MainChartDrag corrected oracle共享dirty的安全提交另由consumer owner處理；不請其代啟被拒host。明細log/20261028/20261028034056.main_chart_range_contract.log。
- 建立時間：2026-10-28 04:29 +08:00。
