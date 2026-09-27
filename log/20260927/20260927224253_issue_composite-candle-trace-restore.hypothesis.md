# Composite candlestick trace restore hypothesis

- 現象：3,563-point composite row含Candlestick + 6 Line；hide candlestick後DOM count=0，等待row ready再show，button已`aria-pressed=true`且title=`Hide ... trace`，5秒至180秒candlestick path仍為0，lines正常。
- Baseline：branch `20260915_033.ptcs_group_support`，HEAD `cdb2b53`，working tree clean。
- 核心假設A：hide/show只切DOM visibility，但candlestick group/path在某個same-topology refresh被移除或clear，show沒有rebuild。
- 核心假設B：scheduled row refresh沒有把local trace visibility納入generation/refresh signal，control reactive state更新但prepared candle DOM不更新。
- 區分實驗：在owner fixture加入3563-point composite row，等待ready boundary後hide/show，分別記control state、row render sequence、path/group existence與display；若node仍存在是visibility bug，若node消失是topology/refresh bug。
- 預估修正：非型別宣告約20-45行；若超過90行需追加Experiment說明。
- 驗證：Renderer focused suite、generic marker Playwright新增composite candle case、renderer performance gate及Daedalus真SPAA。

## Correction / Result

- 假設A、B均被owner fixture反證：正式Renderer在同一4,000-point composite row中可獨立hide candlestick、保留sibling，show後恢復全部8條batched candle paths。
- 真正根因是consumer gate把candlestick locator寫成`ta-trace-*`，正確selector為`ta-candle-*`；修正後又對8-node locator直接`WaitForAsync`觸發strict-mode violation，應等待`.First`再驗完整count。
- Daedalus修正gate後真SPAA 3,563-point完整GREEN。沒有Renderer產品缺陷，不修改source、不升版；只保留owner回歸避免未來混用selector與batched-locator contract。
