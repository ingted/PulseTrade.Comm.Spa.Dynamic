# 真SPAA RESYNC -> READY 未drain queued viewport intent

## 現象

- Official graph：Renderer `0.1.98`、Interactive.Client `0.1.90`。
- Earlier coverage response成功commit resulting revision 4，feedback亦顯示request accepted。
- 隨後adjacent left drag actionCount維持`3 -> 3`，Loaded 500／Viewing 1-250不變。
- Owner BrowserDemo的純`PausedForResync -> Ready` gate為`0 -> 1` PASS。

## 核心假設

正式Interactive/PTCS consumer沒有把runtime state transition送進Renderer新增的drain sink，或replacement commit在READY前清除queue；BrowserDemo直接控制fixture state，因此沒有覆蓋這個integration缺口。

## 驗證方式

比對`setRuntimeStateSink`／runtime view binding的所有呼叫點，並在exact package client test中建立與SPAA相同commit順序；先得到RED再修改。

## Experiment

正式 SPAA 重現排除了 renderer remount 與 provider 未 commit：同一 canvas 收到 revision 4，Poll 從 RESYNC 回 READY，但 action count 未增加。修正實驗將 drain authority 收斂為 `(RuntimePollState, PendingActionId)` 聯合 reactive gate，避免 state-ready 與 action-settled 的通知順序造成漏送；另以 pending-action + RESYNC + boundary drag 的瀏覽器情境驗證。

## Correction

原假設中「正式Interactive/PTCS consumer missing binding」不成立：真SPAA直接將Renderer綁到自己的HTTP/cache callback，且維持同一runtime `Var`。保留的根因候選是Poll-ready與PendingActionId-settled分屬兩個通知來源，舊版僅靠各自單次排程，正式交錯時漏掉queue drain；聯合gate讓兩者成為同一reactive authority。

## Correction 2

Daedalus以READY-before-drag與RESYNC-to-READY兩組A/B證明action皆`3 -> 3`，且feedback從未進入`Earlier coverage queued.`；因此queue甚至沒有建立，聯合gate不是此failure的根因。Owner gate改用相同的一次性框外座標後仍通過，排除document mouseup。剩餘可區分差異是consumer頁面的事件target／bubble路徑；最小實驗把mousedown authority直接綁到既有`ta-overview-interaction-surface`，不改gesture model、coverage request或provider。

## Correction 3

Direct interaction-surface不是最終修法。真SPAA用official `.100/.92/.101`仍維持action `3 -> 3`；owner完整4,000-bar gate亦反證inner rect bounds會因outer SVG一像素border產生比例差：48-bar selection原應提交start 0，實際得到delta `-3954`／requestedStart `-2`，誤判為adjacent Earlier。最終authority改為interaction surface只負責hit，mousedown bubble到live outer SVG；handler直接以currentTarget outer SVG取得bounds，不再依賴可能過期的mutable onReady reference。這同時保留精確outer geometry與真DOM生命週期。
