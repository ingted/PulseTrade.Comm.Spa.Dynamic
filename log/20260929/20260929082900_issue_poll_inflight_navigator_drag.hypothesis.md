# PollInFlight 吞掉連續 navigator boundary drag

## 現象

真SPAA先成功請求Earlier coverage，runtime仍處於`PollInFlight`時立即由selection center拖越左界；action count維持`3 -> 3`。Owner先前fixture只覆蓋`PendingActionId`，未建立真poll-in-flight狀態。

## 根因假設

`localViewportDisabled`把`PollInFlight`與真正不可操作的`Unmounted/Disposed`混為一談；`startNavigatorDrag`因此在mousedown直接返回，gesture甚至進不到`queuedViewportIntent`。

## 契約決策

- `PollInFlight`只禁止第二個remote request立即送出，不禁止local viewport preview／drag。
- mouseup將最後boundary intent放入既有single-in-flight latest-wins queue；poll轉idle後依最新runtime重算並送出唯一adjacent request。
- `Unmounted/Disposed`仍禁止local gesture；identity replacement仍清queue。

## 驗證

BrowserDemo建立真`PollInFlight` runtime state後，以selection center為mousedown點拖越左界；驗action count增加、query generation與visible range前移，且未平行送出第二個remote action。

## Experiment

- 第一個RED揭露實際consumer artifact仍把union tag 3（`PollInFlight`）列為local disabled；原因是incremental pack包入修正前Release DLL。改用新版本並強制Rebuild，不覆寫已restore的同版package。
- 真`PollInFlight`gate通過：拖曳期間出現`Preview 251-298`，settled後callbacks `0->2`，query generation `2`，visible `405-452`。
- `PausedForResync -> Ready`gate通過：mouseup時queue但callback不前進，READY transition由runtime sink自動flush，callbacks `0->1`且只送一次adjacent request。
