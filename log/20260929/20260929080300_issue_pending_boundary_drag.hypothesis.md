# Pending viewport action 吞掉 navigator boundary drag

## 現象

真SPAA在toolbar Earlier成功後，緊接著將navigator whole selection拖越左界；action count維持`3 -> 3`。先前觀察到的Document＋Snapshot response屬前一個toolbar action，並非boundary drag response。

## 假設與範圍

- Renderer以`PendingActionId`同時禁止remote submission與local navigator mousedown/release，造成使用者最後一個boundary intent在建立前就被丟棄。
- 修正只涵蓋Renderer local viewport serialization；不改public contract、wire shape、provider、FSSTL或consumer query authority。
- 必須維持single remote in-flight；不能為了接受drag而平行送出第二個request。

## 驗證方式

BrowserDemo以750ms callback建立pending window：先送48-bar action，pending期間把whole selection拖越左界。預期local preview成立，settled後送出唯一queued Earlier，callback count增加兩次且active detail移至相鄰前頁。

## 結果

- 根因成立。Navigator local drag不再受`PendingActionId`阻擋；preset與boundary共用單一latest-wins queue，settled後依最新runtime重算coverage intent，identity replacement清除queue。
- Focused F# Playwright連續兩次PASS：callback `0 -> 2`、query generation `1 -> 2`、active detail由`453-500`移至`405-452`。完整Renderer BrowserDemo gate亦PASS。
- Exact graph `0.1.95/0.1.86/0.1.96`已發布並完成NuGet.org readback；真SPAA progressive gate仍由Daedalus驗收。

## 流程修正

本結果最初誤追加到較早的accepted-redraw hypothesis；closeout checker正確以`log_readonly_modify`阻擋。舊檔已還原，後續progressive證據只記於本issue檔與active `.log`。
