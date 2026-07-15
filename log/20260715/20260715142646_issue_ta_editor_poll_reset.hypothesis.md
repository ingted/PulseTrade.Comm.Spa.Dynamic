# TA editor poll / reset DEBUG hypothesis

## 現象

1. Add Row後展開row-kind原生select，不操作約五秒會自行收合。
2. 依序刪除MACD與DMI rows後，Reset只恢復DMI，未完整恢復MACD。

## Repo commit status

| Repo | Branch | HEAD | Dirty at investigation start |
| --- | --- | --- | --- |
| Dynamic | `20260715_030.win.TACanvas_cross.bar.cursor.first.done` | `ea139fd` | No |
| PTC/Host | `20260715_030.win.TACanvas_cross.bar.cursor.first.done` | `3a2d7d41f` | `notes_new/` only；先獨立baseline commit |

## 假設表

| ID | 假設 / root cause | 影響位置 | 支持證據 | 反證條件 | 關聯/因果機率 | 驗證 |
| --- | --- | --- | --- | --- | ---: | --- |
| H1 | `RuntimeState`每次poll變更觸發整個renderer `View.Map`重建，native select node被替換而關閉 | Renderer `render` / editor DOM | 約五秒與poll cadence一致 | node identity跨poll不變 | 0.75 / 0.85 | Playwright記錄element handle/node marker與poll前後focus/open行為 |
| H2 | editor DOM其實穩定，但document revision每次poll增加，revision sync覆寫draft並重建controls | Renderer document synchronization | 現有sync以DocumentRevision判斷 | poll只增加DataRevision | 0.50 / 0.70 | 記錄兩revision與editor input值/DOM identity |
| H3 | Reset以目前document或最後mutation作base，不是mount-time initial command | reducer / transient session state | 只恢復DMI符合partial base或mutation replay | reset response含完整原始四列 | 0.75 / 0.90 | remove多列後直接檢查Reset action responsedocument row ids |
| H4 | 初始command保存正確，但多次remove/reset action競態或delta wire遺漏多個rows | client lifecycle / full-vs-delta wire | 五秒poll與action可能交錯 | server reducer單元測試也失敗 | 0.45 / 0.65 | server純測試與browser gate分離；比較full response及client-applied state |

## Pseudocode / 預估非型別宣告行數

```text
render:
  mount stable editor shell once
  update chart/data subtree from runtime View
  synchronize draft only when authoritative DocumentRevision changes

reset:
  session.InitialCommand |> validate |> reduce from empty/initial runtime
  return authoritative full document/snapshot
```

預估實作約80至140行，測試約80至160行；若實作超過280行，先建立experiment記錄再繼續。

## 結論

- H1確認：poll狀態更新會替換整個editor DOM，是select自行收合的直接原因。
- H2部分確認：不是DocumentRevision增加，而是過寬的reactive dependency；修正為stable document shell與nested status/chart view。
- H3反證：Host已分離`initialCommands`與current commands，Reset authority正確；真正缺口是multi-row formal regression。
- H4揭露額外cache缺陷：只以Document/Data revision判斷chart等價會濾除reopen後same-revision full frame；納入`LastTransportSequence`後通過isolated與正式82 gate。
