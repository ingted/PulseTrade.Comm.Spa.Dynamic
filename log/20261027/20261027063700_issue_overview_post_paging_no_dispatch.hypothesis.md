# Overview post-paging no-dispatch investigation

## Pre-work plan

- 現象：Daedilus以0.2.48 exact packages重驗FormalOverviewEdgeDrag；初段edge action為Action `$8`、HTTP 200、coverage revision 1→2，但完成兩次earlier page後，resident-local overview body向右拖一個selection width，pointerup後5秒內`/api/workspace/actions`完全沒有進route（`delayedWorkspaceActionEntered=false`）。
- 已證實事實：SMA matrix 1/1 PASS；package ProductVersion包含`3649f1ee`；failure不是先前6.34秒route latency，因這次5秒內route count為零。共享evidence `G:/coldfar_py/coldfar-symbolics/temp/agent.daedilus/formal-overview-edge-drag-0248/evidence.json`證實初段Action `$8`與revision前進，但尚未含第二段無dispatch compact trace。
- 預期不變量：正式resident 4500內的body drag在非零delta pointerup後，若target window可由已載resident表達，應產生唯一typed viewport action；若有pending authority則queue後在最早合法unblock同步drain。不得靜默commit-only、不得依低頻poll/RAF、不得把active slot當GLOBAL ordinal。
- Source of truth：Renderer `startNavigatorDrag`／pointerup、local/global draft commit、`tryFlushQueuedViewportIntent`與pending action/boundary/coverage/generation gates；consumer route只作E2E觀測。
- 假設H1：兩次earlier page後，optimistic/committed local window與fresh resident projection不一致，使pointerup的semantic result被判為no-op或outside-window而不建立intent。
- 假設H2：intent已建立但被pending boundary、coverage key/revision或action id gate拒絕；0.2.47只在unblock同步drain，未修正該gate永不轉為ready或錯誤丟棄。
- 最小區分證據：同run在pointerdown/up記committed/draft window、resident/revision、drag mode/domain/delta/outcome、queued intent、pending action/boundary、coverage key/revision，以及immediate/RAF flush的拒絕原因；owner test固定「初段edge accepted→兩次resident replacement→body drag」先RED。
- 預估最小實作：若H1為5–20行state rebase/no-op判定＋20–50行regression；若H2為10–30行gate transition/typed decision＋30–60行regression。超過2x先補Experiment。
- 範圍：Dynamic Renderer/direct tests/package/docs；consumer只讀evidence，不改其source。
- 停止條件：owner regression修前RED修後GREEN、full direct tests與exact package綠，且真18884在5秒內route進唯一action並完成revision；未有真證據不結案。

## Operations

- 2026-10-27 06:37 +08:00：建立hypothesis；Daedilus handoff=`msg-fsi-446dc9093a924015b730ae29dcbbbae8`，receipt/diagnostic request=`msg-fsi-e65f5ddee29a4840b1b28a491c5a5c50`。尚未修改程式碼。
- 2026-10-27 06:41 +08:00：consumer compact trace證實首個FAIL是setup race：第二個earlier `ui:2 Action$8`仍pending時，測試只因optimistic firstAxis變動便啟動middle drag，因而把前一action誤當middle gate。補revision/preparation/response settlement後，同一0.2.48 rerun於pointerup後約385ms進`ui:3 Action$8`，HTTP 200，revision3→4、Viewing1499-2998；H2 product no-dispatch不成立，未改Renderer。
- 2026-10-27 06:45 +08:00：後段fixed-seed stress的12個failure全為consumer cross-axis oracle：測試以GLOBAL resident ordinal加formal local slot，再與30K ActiveDetail ordinal比較，導致12/12 `localTarget=false`且匹配不到event-time Action$8。owner contract刻意禁止此混算；已以`msg-fsi-67751360c2214ed1b1aa83a4dc382d43`要求改驗唯一event-time action、accepted request與chart/viewport commit。現有geometry證據left/right mode exact、固定edge error=0、body width error近0、rapid committed=true。
