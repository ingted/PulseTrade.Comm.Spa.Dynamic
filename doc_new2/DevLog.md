
## 2026-11-10 daedilus：GLOBAL 排程候選，等待 18882 切換

13 FSSTL 與原 09 FSXSTL 的四筆 run 共存已在 Research 0.1.88 完成真資料 native 驗收。後續追查 00002，發現資料準備逐批等待 requestAnimationFrame；在約每秒一輪的環境中，同一 GLOBAL 手勢的準備 queue 約 36 秒。改用既有 MessageChannel bounded work scheduler 後降至約 0.4 秒；整段手勢由 59.6 秒降至 20.5 秒，尚有後端與解碼延遲，不能宣告 00002 完成。

更快的更新也重現「第二次拖曳仍按住時，前次回覆讓邊界日期預覽消失」。保留舊 frame 的試驗無效，已移除；改為 workspace reactive preview，避免依附可替換的 DOM。Renderer 346、Client 59 項通過，Research 0.1.89 完整建置 0 errors，18882 新候選已凍結，但 native 複驗尚未完成，0.3.55／0.1.35 尚未發布。

切換 18882 的動作被自動審核拒絕（blocked by policy），未執行。已備妥 misc/switch.spaa.research.18882.reactive-preview.ps1，限定既知 18882 process、hash 檢查、背景隱藏啟動與 ready gate。18883／18884 不變，正常 18882 啟動器仍指向已驗收 0.1.88；目前在線的上一份 0.1.89 僅屬預覽候選。詳細失敗試驗、時序與續接步驟記於 Research00014-00015.Acceptance.md 及本日 op_log。
## 2026-11-10 Research 0.1.89 native 複驗完成

使用者完成候選切換後，重新核對 18882 的目前 process 與 HTTP bundle。DLL SHA-256 B1C1E4B1BAD176A02708FA4F7B1E996B7E619426814ED490F9A1C2B5C0C10950；bundle D04905984191D950BBE59AC66B0B7CD31C0F48A82892756D38C59D793B218646，均與 frozen manifest 相同。

- GLOBAL 真實滑鼠拖到相同 43% 位置：仍為 2021-10-05 06:30 至 2021-11-18 21:00 UTC、1500 根。這次整個手勢 28.969 秒，放開後 19.867 秒；準備階段 work 659 ms、queue 208 ms、wall 867 ms。排程等待問題已修，但 00002 整體延遲仍 PARTIAL；不得用此數據宣稱已瞬間更新。
- 第一筆向右拖曳送出後，立刻反向拖曳並保持左鍵按住：query generation 1 到 2 的舊回覆完成時，預覽一直為 2021-10-01 15:30 至 2021-11-17 07:00 UTC。放開後 query 3 的實際範圍完全相同、仍 1500 根，額外等待後未回跳。
- 拖曳中按右鍵切換模式：取消預覽、日期與 query 不變。框選模式有可見矩形與日期預覽；放開後 450 根可視範圍與預覽精確一致。截圖已實際檢查。
- 在最終相依組合重新載入 13，執行兩個回測入口，共有四個 Completed 結果；逐筆等待 scenario 選單與 trades 真正呈現再檢查。兩個 DMI 結果各 21 trades；FSSTL 固定下單 entry UTC 11/4 23:03，FSXSTL 固定下單 entry UTC 6/17 22:23。四個結果切換後均保留。status 文字會先於結果 DOM 更新，因此驗收以實際 scenario/trades 為準，早期取樣另保留，未當成 PASS。
- 新頁 console 0 errors / 0 warnings。Renderer 346/346、Client 59/59 與 WebSharper build PASS 沿用本候選同一來源，不重跑未變更的測試。
- 證據：00002-reactive-preview-native-pass.json、00002-reactive-cancel-selection-native-pass.json、00002-reactive-selection-preview.png、00015-four-runs-final-0.1.89-native-pass.json、00015-four-runs-final-0.1.89.png、00002-00015-final-0.1.89-console.log，均在 canonical log/20261110。
- 正常 18882 launcher 預設更新到本次驗收候選；18883／18884 的 launcher、process、artifact 不變。套件在 source checkpoint 後才發布，發布結果另追加。

Git inventory 發現 15 個舊 Research/temp 追蹤紀錄已由外部流程刪除，保留該清理並納管，不復原或刪除其他檔案。這些是舊 bump 工具產生的 op_log／版本測試紀錄，非目前 runtime 或策略來源。
