# 非CURSOR主圖框選預覽與提交分歧

- Baseline：canonical Dynamic branch20260915_033.ptcs_group_support，HEAD8adeb9c、owner source0b7e6b1、Renderer/Interactive0.3.16。外部dirty清單見同timestamp pre-work log，不覆蓋。
- 已證事實：consumer r2正向25%→65%左界差225 bars（owner回報，已獨立讀其canonical Expecto oracle）；`RendererModel.mainPlotInteractionDrag`在Resize依pointer半區回ResizeLeft/ResizeRight；`Renderer.startMainPlotDrag.updateRangePreview`則以min/max顯示兩端點，finish又以delta呼叫單邊previewWindowBounds。原RFC0037只准preview而明示不改commit，留下兩個不同意義的authority。
- 不變量/source of truth：非CURSOR反白兩端點應成為新visible兩端點；正反向normalize，時間範圍由既有prepared timeline與setWindowFromPrepared提交，CURSOR及navigator單邊resize不變。Consumer acceptance由Research.MainChartDrag.playwright.expecto.fsx獨立比例oracle判斷，不複製新helper；仍維持既有MinimumVisibleBars與clamp，狹於最低根數的框選明列限制。
- 最小驗法：DYN-T-707在0.3.16 exact package要求非CURSOR為select-range即RED；DYN-T-746以1000/1500根、正反向、越界、stationary、invalid geometry、最低根數驗兩端點window。source wiring證main finish不再依半區套單邊delta，owner browser驗25%→65%及85%→15%的TA/resident selection兩界。
- Pseudocode（預估runtime非型別行數65）：新增SelectRange constant；mode mapper只分Move/SelectRange；共用preview bounds夾兩端點，映成half-open bar slots（left floor/right ceil）再沿既有min/max clamp；update保存同一候選window，finish由捕獲document/prepared同一路徑提交；零寬/invalid/cancel不提交，queued draft不得在release後復活。無新type/project/service/protocol，無JavaScript escape hatch。
- 因果判準：改前unit/consumer RED；改後相同canonical oracle通過，CURSOR/navigator現有測試維持。B偶發resident stall尚無exact RED，與本hypothesis分開，不猜修。
