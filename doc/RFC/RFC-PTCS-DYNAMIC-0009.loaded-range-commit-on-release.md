# RFC-PTCS-DYNAMIC-0009 Loaded Range Commit-on-Release

- Status：Accepted / DEV authorized
- Date：2026-07-14
- Owners：PTCS.Dynamic Renderer / PTCS adapter
- Related：`doc/TAResearch/REQ.ChatReplyCanvasPresentation.md`、`doc/TAResearch/WBS.DYN-TA-013.md`

## 背景

正式TA reply雖請求2000 bars，PTCS browser adapter對full snapshot與delta共用200-point cap，因此browser loaded authority只有200。renderer的native range在每次`input`事件直接commit viewport並重建四列17 traces，弱機拖一格就卡一次。

## 目標

1. full bootstrap/reconnect可保留最多2000 points per series；stable delta仍維持200-point上限。
2. horizontal navigator拖曳中只更新preview；release/change才commit一次viewport render。
3. 完整loaded range在browser local移動，不送network action；DOM只render bounded visible window。
4. 以compact wire降低2000x17 initial payload的default-field成本。

## 非目標

- 不增加thumbnail navigator。
- 不一次mount 2000根DOM。
- 不在每次release向server做viewport paging。
- 不用request文字或fake points冒充actual loaded count。

## 決策

### Full與delta分流

`MaxFullSnapshotPointsPerSeries = 2000`；`MaxDeltaPointsPerSeries = 200`。初始document後第一次series由empty變non-empty時視為authoritative full bootstrap，不能被誤編成200-point delta。

### Draft與committed viewport分離

`input`只寫`draftStartIndex`與preview label；chart仍訂閱committed viewport。`change`/release驗證clamp後呼叫一次`setWindow`並清draft。render sequence只在committed chart composition增加。

### Wire compactness

JSON忽略default numeric/bool fields。line points只送time/value；OHLC fields只在corresponding flags為true時送。client以既有flag判斷欄位，缺省仍是合法default。

## 方案取捨

| 方案 | 決策 | 原因 |
| --- | --- | --- |
| 2000 bars全DOM | Rejected | 四列17 traces在弱機成本不可接受 |
| release後server paging | Rejected | loaded 2000可local navigation；paging增加latency與第二套state |
| thumbnail navigator | Rejected | 使用者明確不需要，且會增加render負擔 |
| native range preview + release commit | Accepted | 拖曳手感即時，chart只重畫一次 |

## 影響範圍

- `PulseTrade.Comm.Spa.Dynamic.Renderer`
- `PulseTrade.Comm.Spa.Dynamic.Ptcs`
- `PulseTrade.Comm.Spa.Dynamic.Ptcs.Client`與exact downstream package references
- renderer/Ptcs tests與F# Playwright verifier

## 驗收

1. 2000-point full roundtrip保留2000；delta仍<=200。
2. empty -> first data輸出`full`。
3. drag期間render sequence不變；release後恰增1。
4. loaded>=2000時可從tail移到head；visible<=160；network action count不變。
5. pure WebSharper/F#，無handwritten JavaScript。

