# Main-plot pointer frame coherence hypothesis

## 現象

0.2.55 真實瀏覽器 Gate A 的 CURSOR pan 顯示 `moving -> committed`，`localVisibleStart=3150`、`localVisibleCount=1215`，但 15 秒內沒有 workspace action response。

## 已證實事實

- `startMainPlotDrag` 在 pointerdown 以 `runtimeState.Value.Document`、`runtimeState.Value.Data` 與獨立 mutable `latestPreparedData` 組成 gesture snapshot。
- `chartCommittedFrame` 已是 renderer 對 state/prepared data 的原子 committed authority。
- `setWindowFromPreparedRange` 先執行 `commitLocalWindow`；若 `visibleEventRangePrepared` 回 `None`，只清 `draftWindow`，不送 `VisibleRangeChanged`。
- 同一 lifecycle 的 action-completion settlement 已在 DYN-T-720 改用 `chartCommittedFrame.Value`，證明 state/prepared 分離讀取不是安全 authority。

## 預期不變量

一次 pointer gesture 只能使用同一個 committed chart frame 的 document、data、prepared timeline 與 coverage projection。若 CURSOR pan 產生有效 target window，event range 必須可解析並送出唯一 action；不得 local-only commit。

## 根因候選

1. 高信心：pointerdown capture 非原子，prepared timeline 與 document/data 不同代，event-range resolution 失敗。
2. 待 hardened rerun 排除：pointerdown 時 document identity 或 projection scope 已變更，正確行為應 fail closed 而非 local commit。

## 最小區分實驗

1. RED source contract：要求 pointerdown 先 destructure `chartCommittedFrame.Value`，並禁止在該 capture 使用 `runtimeState.Value.Document` 搭配 `latestPreparedData`。
2. 最小修補後跑 Renderer focused suite。
3. Daedilus 重跑真實 Gate A，核對 stateBefore/after、workspace action count、event-time range、accepted response 與 final DOM。

## 預估修改

實作約 4 行：一次 destructure committed frame，document/projection 改從 committed state 取得。測試與文件另計；若超過 8 行實作，先在本檔追加 Experiment 說明。

## Experiment

- DYN-T-722 source contract先0/1 RED，atomic pointer capture後focused1/1、full Renderer113/113 GREEN，candidate pack也完成。
- Daedilus hardened Gate A後續證據反證本假設：before/after local start/count、active start/count、coverage/query/render均不變；請求delta340使target start超出captured formal-base range，因此在`tryMainPlotPanWindow`即fail closed，沒有進入event-range resolution。
- 結論：atomic capture是可辨識的一致性改善，但不是本次Gate A no-op根因；依DEBUG規範已將source/test/version/docs實驗精確回退0.2.55 baseline。下一假設應直接覆蓋resident boundary越界時的typed request contract。

