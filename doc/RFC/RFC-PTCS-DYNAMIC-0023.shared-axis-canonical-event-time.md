# RFC-PTCS-DYNAMIC-0023：Shared Axis Canonical Event Time

- ID：RFC-PTCS-DYNAMIC-0023
- 狀態：Accepted / DEV Authorized
- 日期：2026-09-24
- Owner：Aster / PTCS Dynamic Contracts and Renderer
- Consumer：Daedalus / SPAA、TradeCore、ColdFar Notebook DIExt
- 關聯：`RFC-PTCS-DYNAMIC-0014`、`0017`、`0020`、`0022`
- 上游修正：Daedalus direct feedback `msg-fsi-c70ea31330194155a6b4368034599ee7`

## 背景

SPAA 的 legacy row payload 有 top-level `t`，但 shared temporal compression 只把 interval metadata 寫入 `TemporalAxisPoint`，`TemporalSeriesPoint`只保留 Position＋Value。壓縮後 canonical row `EventTimeUtc` 因此遺失，Renderer 只能把 `IntervalStartUtc` 當顯示時間；若改猜 `IntervalEndUtc`，forming preview 又會錯。

實際需求同時包含：completed 03:04 minute 應標示 03:05；形成中的 latest preview 在 03:06 應標示 03:06。這個差異只有 owner 知道，generic Renderer 無法由 interval/finality/observed-through可靠推導。

## 目標

1. shared axis 明確攜帶 owner-authored canonical presentation timestamp。
2. Renderer 的 row timeline、chart point、marker attachment與cursor/data window使用同一 timestamp authority。
3. 保留 `temporal-axis.v1` missing-field decode，舊 payload 不因新增欄位失效。
4. 不複製 event time 到每條 scalar series，不把 interval metadata 改造成 presentation timestamp。

## 非目標

1. Dynamic 不計算 K bar close time、交易日、calendar或 live clock。
2. 不改 `TemporalSeriesPoint = Position + Value`。
3. 不用 `IntervalEndUtc`、`ObservedThroughUtc`或 `AvailableAtUtc`猜 canonical event time。
4. 不改 interval range、projection、availability或 query intersection semantics。

## 決策

### 1. Public contract

```fsharp
type TemporalAxisPoint =
    { Position: int64
      SourceIntervalId: string
      ScaleKey: string
      IntervalStartUtc: DateTimeOffset
      IntervalEndUtc: DateTimeOffset
      EventTimeUtc: DateTimeOffset option
      ObservedThroughUtc: DateTimeOffset
      AvailableAtUtc: DateTimeOffset option
      Finality: PointFinality
      Projection: TemporalProjection
      Quality: string option }
```

`EventTimeUtc`是該 Position 的 canonical presentation/timeline identity。它放在 axis，因同一 Position 的 O/H/L/C/V、indicator及marker series必須共用同一時間；放進series會重新引入 `bars x series` metadata duplication。

owner 建 axis 時須驗同一 Position 的來源 rows 對 EventTimeUtc 一致，再寫入一次。Dynamic codec只負責 UTC/shape validation，不從其他欄位合成值。

### 2. Wire compatibility

`temporal-axis.v1`新增 optional `eventTimeUtc`：

- encoder：`Some`時寫 ISO-8601 UTC；`None`不寫。
- decoder：缺欄位回 `None`；非法 timestamp或非 UTC offset fail closed。
- legacy `None`只為相容既有畫面而 fallback 到 `IntervalStartUtc`，不得宣稱取得 canonical owner time，也不得 fallback 到 interval end。

這是 wire-compatible additive field，但 F# record是 source/binary contract變更，因此 Contracts 與全部 exact-package consumers須同一 release graph。

### 3. Renderer boundary

Renderer 建立 `presentationTimestamp metadata = EventTimeUtc |> defaultValue IntervalStartUtc`，只用於：

1. candle/line point `Timestamp`。
2. trace/reference/row presentation timeline；`chartTopologySignature`仍以Position／IntervalStartUtc保持same-position preview穩定。
3. marker Position 對 target candle/reference slot 的 attachment。
4. prepared base-point timestamp lookup與row-local cursor/data window。

以下仍使用 interval fields，不得改用 EventTimeUtc：

- repeat/candle-span range projection。
- step-after-close availability。
- query range intersection、coverage與loaded boundary。
- `IntervalEndUtc`回傳及 source interval診斷。

### 4. Producer examples

| 狀態 | Interval | EventTimeUtc | UI timestamp |
| --- | --- | --- | --- |
| completed 1K | `[03:04,03:05)` | `03:05` | `03:05` |
| forming preview | current interval，latest event 03:06 | `03:06` | `03:06` |
| legacy payload | authored interval only | missing | compatibility start；非 canonical |

## 失敗路徑

1. `eventTimeUtc`不是 ISO-8601或不是 UTC：reject candidate，保留 last-good。
2. owner 對同一 Position產生不同 event time：owner adapter拒絕 frame；Dynamic 不選任一值。
3. legacy missing：可顯示既有 start-based geometry，但不能以 interval end補值。
4. exact graph未同步：compile/restore gate失敗，不以 ProjectReference或浮動版本繞過。

## 影響

- Contracts：`TemporalAxisPoint`與 `TemporalAxisCodec`。
- Renderer：temporal presentation model與 canonical timestamp lookup。
- Dynamic.Ptcs／Interactive.Client／Ptcs.Client：exact Contracts/Renderer graph進版。
- Daedalus：將 row `EventTimeUtc`寫入 axis point，並在聚合前驗同 Position一致。

## 驗收

1. codec round-trip保存 EventTimeUtc；legacy missing回None；non-UTC拒絕。
2. shared candle、line、reference timeline、marker與cursor全部使用 authored event time。
3. completed 03:04→03:05顯示03:05；forming latest 03:06顯示03:06。
4. interval projection/query/coverage仍使用 start/end，沒有 interval-end inference。
5. 同Position forming preview只更新EventTimeUtc時不改chart topology signature、不重建4,000-point chart。
6. focused tests、exact package readback、browser regression與Daedalus真SPAA consumer gate可追溯。
