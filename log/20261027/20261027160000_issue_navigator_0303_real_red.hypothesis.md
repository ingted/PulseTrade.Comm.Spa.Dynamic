# Issue hypothesis: navigator 0.3.3 real Gate B still publishes None

時間：2026-10-27 16:00:00 +08:00

## Phenomenon

0.3.3 exact真Gate B的middle action可以從local3000 relocation到1201，但其後long target0只發bare `ui:4`，final local1122。Handler只在gesture frame為`Some`時才能發action，所以pointerdown實際讀到`Some(..., projection=None)`。

## Repository state

- Repo：`C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic`
- Branch：`20260915_033.ptcs_group_support`
- HEAD/upstream：`220f63dccfa266e566c2550947739e696c45502d`
- 外部dirty：`lib-packs.txt`、`tests/PulseTrade.Comm.Spa.Dynamic.Ptcs.Tests/Program.fs`、`.pcsl/**`、`temp/**`，本輪不納入／不改寫。

## Hypotheses and distinguishing evidence

1. `ShellPreparedFrame` tuple本身非atomic generation：`renderedState.Document`有coverage，但`currentPreparedData.RawData`的coverage metadata屬另generation或缺失。預期證據：同一publication id的document/prepared identifiers或coverage counts分歧，projection為None。
2. 另一retained mapper或renderer instance在valid publication後發布一個真實但缺coverage的frame。預期證據：publication sequence先Some後None，pointerdown讀到後者；writer identity/frame id不同。

## Experiment

- 在production publication點記錄bounded telemetry：writer/render instance id、monotonic publication sequence、document present／loaded coverage摘要、prepared/raw coverage摘要、projection Some/None與必要的key/window摘要。
- 在pointerdown記錄所讀publication sequence/writer id/projection state，不寫raw bars或大型JSON。
- 新增test鎖production source/generated contract與telemetry formatter邊界；舊0.3.3 source必須RED，instrumented candidate才GREEN。
- 以Daedilus原封不動真B收一次runtime evidence，再決定修復；未觀測前不忽略None或保留舊Some。

## Estimated change

觀測性實作預計不含type declaration約25–40行，測試約30–50行；若超過2x，先在本檔追加原因再擴寫。

## Experiment

實際Renderer source新增83行，略超過預估上限40行的2x（3行）。超出來自將兩個decode authority各自記錄、12個固定bounded attributes、與最後12筆cross-publication/pointer trace同時保留，使真browser一次就能區分state-data mismatch、prepared-data missing ref、multi-writer order與pointer實際讀值。若移除任一類會回到只見`projection=None`的無法區分狀態；因此保留此最小bounded telemetry，不擴展wire、action或raw data output。
