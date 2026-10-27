# Hypothesis: navigator preview and release assemble from different frozen authorities

## Proven facts

- 0.2.59 source/package/deployment identity is exact; stale package或bundle已排除。
- 同一long body gesture的diagnostic preview得到`dragLocalStart=1201`、`dragLocalCount=79`、`dragLastDelta=-1202`、`targetLocalStart=0`與`canonicalReady=true`。
- 實際POST只有event-time Start/End，沒有`CoverageIntent`；settlement只移79根到local1122。
- 因此UI hit testing、drag delta與canonical event-range preview皆已成功；缺口在release選擇／組裝／dispatch CoverageIntent，而非目標計算本身。
- 0.2.59已讓pointerdown coverage snapshot讀`dragPreparedData.RawData`；此單點修正仍未改變真POST。

## Expected invariant and earliest divergence

Pointerdown凍結的committed prepared frame應同時決定preview range、loaded-coverage snapshot、purpose與release action。只要preview宣告canonical ready且目標在有效event domain，release不得退化為bare `setWindow`；失敗必須fail closed並留下typed reason，而不是靜默換另一個authority。

## Candidate causes and minimal evidence

1. pointerup的`loadedCoverageSnapshot`仍為None或與preview使用不同prepared payload，因此走明確bare fallback。以同一source branch/data flow與source contract區分。
2. snapshot為Some，但`tryNavigatorEventRangeIntent`使用與preview不同的event-range/range source而回None。以helper inputs與pure focused case區分。
3. intent已組出，但後續queue/dispatch用另一個bare action覆蓋。以`SduiAction`建立點到serializer前的typed flow區分。

先檢查實際branch與資料來源；只有能重現Daedilus byte semantics的最小test RED後才改production expression。

## Experiment

- Navigator DOM真正由`currentPreparedData`（`shellPreparedData.View`）建立，selection/projection與event handler都位在該closure。
- `startNavigatorDrag`卻在pointerdown重新讀另一個獨立Var `chartCommittedFrame.Value`；這個frame不保證就是使用者目前按下的navigator所render的shell frame。
- 因此0.2.59改用`dragPreparedData.RawData`仍可能解不到rendered projection：改對了資料欄位，卻仍讀錯frame authority。當snapshot None，line 4358明確退化成`setWindow`，其event-time range可正確但CoverageIntent必為None，與真wire byte semantics一致。
- 新source gate要求shell以atomic `(RuntimeState * TaPreparedRendererData)`發布，render closure把同一pair直接capture進`startNavigatorDrag`，並禁止該handler重讀`chartCommittedFrame.Value`。現行0.2.59得到0/1 RED，已精確區分候選1；候選2/3不符合此明確bare fallback。

## Estimated patch

目標是一個release authority/branch修正加focused regression；若需跨多函式或超過約兩倍估計，先在本檔追加Experiment與新證據，不擴張consumer。
