# RFC-PTCS-DYNAMIC-0016：Marker Direction 與 Hollow Contract 修正

- ID：`RFC-PTCS-DYNAMIC-0016`
- 狀態：`Implemented / Consumer Handoff`
- Owner：Aster（PTCS Dynamic Contracts／Reducer／Renderer／Client）
- Consumer owner：Daedalus（FSSTL／TradeCore／SPAA／Interactive Extension）
- 日期：2026-09-21
- 修正基線：`RFC-PTCS-DYNAMIC-0015.generic-marker-overlay.md`
- Consumer review evidence：`G:/coldfar_py/coldfar-symbolics/notes/20260921/00249.backtest_engine.ta_research_engine.visualization.txt` 第 840–934 行

## 1. 背景

`RFC-PTCS-DYNAMIC-0015` 已建立 transport-neutral generic marker、candidate-state validation、fixed overlay lane 與 runtime v2。不過 consumer review 對照 current source 後確認三項 contract 偏差：

1. `TaMarkerShape.Arrow` 的方向由 `Anchor` 推導。`AboveBar` 會畫向下三角形，`BelowBar` 會畫向上三角形；producer 無法獨立表達「位置」與「方向」。
2. `TaMarkerFill.Outline` 目前以白色、`0.92` opacity 填滿。這不是 hollow，會遮住下方 candle／indicator。
3. codec 的 `MaxMarkersPerBucket = 4` 是每個 position bucket **總量**，原 RFC geometry 卻可被讀成每個 anchor 各 4。renderer 的 lane index 另只在單一 marker trace bucket 內計算，多個 marker traces 指向同一 candle 時會產生 lane collision。

這些不是 TradeCore domain projection 問題，而是 PTCS Dynamic public presentation contract 與 renderer conformance 問題，應由 PTCS Dynamic owner 修正。

## 2. 目標

1. 將 marker 的 spatial anchor 與 glyph direction 定義為兩個正交維度。
2. 讓 `Outline` 具有真正透明內部，且不犧牲完整 hover／tooltip hit area。
3. 統一 wire bucket、candidate validation 與 renderer lane 的數量語意。
4. 保留既有 `ta-marker.v1` durable payload 的可讀性，但禁止舊 client 靜默誤解新方向。
5. 維持 `RFC-PTCS-DYNAMIC-0015` 的 Position authority、last-good、Y-scale isolation、bounded state 與 owner 邊界。

## 3. 非目標

- 不新增 signal／entry／exit／fill 等 domain-specific marker case。
- 不由 PTCS Dynamic 判斷交易方向、策略語意或 entry／exit 配色。
- 不新增任意 SVG、custom path、icon URL、CSS function 或 click action。
- 不改變 marker 的 target candle、temporal Position、tooltip 或 revision model。
- 不把 SPAA transaction、Backtest workspace 或 FSSTL projection 移入 PTCS Dynamic。
- 不在本 RFC 擴大 marker size、lane height或每個 DataRef 的總量上限。

## 4. 使用情境

### 4.1 方向與位置獨立

以下四種組合都必須合法且得到不同、可預測的畫面：

| Anchor | Shape | 語意 |
| --- | --- | --- |
| `BelowBar` | `TriangleUp` | K 棒下方、尖端朝上 |
| `BelowBar` | `TriangleDown` | K 棒下方、尖端朝下 |
| `AboveBar` | `TriangleUp` | K 棒上方、尖端朝上 |
| `AboveBar` | `TriangleDown` | K 棒上方、尖端朝下 |

`Anchor` 只決定 glyph 位於 candle high 上方或 low 下方；`Shape` 只決定 glyph 幾何。renderer 不得再從 anchor 推論方向。

### 4.2 DMI／交易事件 projection

Daedalus consumer acceptance mapping 已凍結為：

| Consumer event | Anchor | Shape | Fill | Color |
| --- | --- | --- | --- | --- |
| long entry | `BelowBar` | `TriangleUp` | `Outline` | `#000000` |
| short entry | `AboveBar` | `TriangleDown` | `Solid` | `#000000` |
| long exit | `AboveBar` | `TriangleDown` | `Solid` | take-profit red／stop-loss green |
| short exit | `BelowBar` | `TriangleUp` | `Solid` | take-profit red／stop-loss green |

Entry marker 使用 strategy signal time；exit marker 使用 actual simulated fill time。其他 exit reason 由 consumer 傳 neutral color，renderer 不得猜測停利／停損。這仍是 Daedalus-owned consumer mapping，不是 PTCS Dynamic 內建的 long／short、entry／exit或 PnL 規則。

## 5. 決策

### 5.1 Public type

`TaMarkerShape` 改為：

```fsharp
[<RequireQualifiedAccess>]
type TaMarkerShape =
    | TriangleUp
    | TriangleDown
    | Circle
    | Square
    | Diamond
```

`Arrow` 從 current public DU 移除，避免同一 case 繼續隱含兩種方向。這是 source-breaking correction；generic marker 尚未完成 consumer adoption，應在廣泛使用前修正，而不是永久保留含糊 public API。

`TaMarkerAnchor` 與 `TaMarkerFill` 保持不變：

```fsharp
type TaMarkerAnchor = AboveBar | BelowBar
type TaMarkerFill = Solid | Outline
```

### 5.2 Wire schema 與 legacy decode

新 encoder 只輸出 `_type = "ta-marker.v2"`：

```json
{
  "_type": "ta-marker.v2",
  "markerId": "entry-0001",
  "eventTimeUtc": "2026-09-21T01:02:03.4560000+00:00",
  "anchor": "below-bar",
  "shape": "triangle-up",
  "fill": "solid",
  "color": "#16a34a",
  "tooltip": []
}
```

v2 shape 只接受：

- `triangle-up`
- `triangle-down`
- `circle`
- `square`
- `diamond`

decoder 保留 `ta-marker.v1` ingest compatibility：

| v1 shape | v1 anchor | current DU 結果 |
| --- | --- | --- |
| `arrow` | `above-bar` | `TriangleDown` |
| `arrow` | `below-bar` | `TriangleUp` |
| `circle`／`square`／`diamond` | 任意 | 同名 current case |

這個 mapping 精確保存舊 renderer 的視覺方向。v1 decode 後若重新 encode，一律產生 v2；public authoring API 不再產生 v1／`arrow`。

`sdui-runtime.v2` 保持不變，因為 trace capability 與 reducer transaction 沒變；marker item 自身的 `_type` 已足以 fail closed。舊 package 遇到 `ta-marker.v2` 必須 structured reject，不得 fallback 成 v1、circle、candle、line 或 text。

### 5.3 Browser cache 與 durable state

- marker-capable browser cache schema 由 2 升至 3。
- schema 2 marker cache 不做原地猜測或 rewrite；視為 miss，要求 authoritative snapshot。
- durable/runtime history 中的 v1 marker 可經 compatibility decoder 讀取，再由 current state／export 輸出 v2。
- cache miss、wire incompatibility與 malformed marker 的既有 last-good／`RejectFrame` 規則不變。

### 5.4 Hollow rendering

`Fill = Outline` 的 normative SVG 語意：

- visible glyph：`fill="none"`。
- stroke：使用 `marker.Color`；需要黑框時 producer 明確傳 `#000000`。
- 不使用白色、背景色或半透明實心 fill 模擬 hollow。
- stroke width 與 vector effect 沿用既有 bounded geometry。
- tooltip／hover 仍覆蓋完整 9px glyph bbox；可使用不顯示的 interaction hit target 或等價 `pointer-events` 設定，但不得加入可見填色。
- invisible hit target 不得吞掉或阻止 row/shared vertical cursor 的 pointer move。
- marker tooltip 與 shared cursor 必須可在同一次 pointer interaction 共存；pointer 穿越 hollow 內部時，cursor 仍移至相同 slot。
- hit target 不得加入 Y-domain、numeric legend、額外 time slot，亦不得觸發 candle series rebuild。

`Fill = Solid` 維持 fill/stroke 都使用 `marker.Color`。

### 5.5 Bucket limit 與跨 trace stacking

數量 contract 分兩層：

1. **Wire bucket**：單一 marker DataRef 的單一 `TemporalSeriesPoint.Value` array，跨 `AboveBar`／`BelowBar` 合計最多 4 個 marker。第五個必須以 `limit-marker-bucket` 拒絕。
2. **Visual lane aggregate**：同一 row、target trace、Position、anchor，跨所有 marker traces 合計最多 4 個 marker。超過時整個 candidate 以 structured `limit-marker-lane` 拒絕，保留 last-good。

renderer 的 aggregate lane order 固定為：

```text
document marker trace order
  -> bucket array order
```

同一 `(rowId, targetTraceId, Position, Anchor)` 的 lane index 必須由合併後序列計算，不可在各 trace 從 0 重新開始。如此可維持 top／bottom 各 44px fixed lane，不重疊、不侵入相鄰 row，且不改 Y autoscale。

### 5.6 Validation 與錯誤處理

Document／Snapshot／Patch 仍先形成完整 candidate，再一次驗證：

- v2 shape 與 strict fields。
- v1 compatibility mapping。
- wire bucket total limit。
- cross-trace visual lane aggregate limit。
- target candle／Position／axis revision／MarkerId uniqueness 等既有規則。

任何 producer contract violation 都回 non-recoverable structured `RejectFrame`，不得部分過濾、不得推進 revision、不得覆蓋 last-good。缺 snapshot／axis／position 等 recoverable gap 仍走既有 `RequestResync`。

## 6. Package 與 consumer 影響

| Package／project | 必要變更 |
| --- | --- |
| `PulseTrade.Comm.Spa.Dynamic.Contracts` | public DU、v2 encoder、v1/v2 strict decoder、cache schema 3、candidate aggregate validation |
| `PulseTrade.Comm.Spa.Dynamic.Renderer` | direction-independent triangle geometry、true hollow、cross-trace lane composition與hit target |
| `PulseTrade.Comm.Spa.Dynamic.Ptcs` | 保留 v2 marker payload與structured rejection |
| `PulseTrade.Comm.Spa.Dynamic.Ptcs.Client` | current codec/cache schema、unknown marker version fail closed |
| `PulseTrade.Comm.Spa.Dynamic.Interactive.Client` | current codec/cache schema與exact Contracts/Renderer |
| aggregate Dynamic／tests／LiveDemo | exact package closure與版本同步 |
| SPAA／Interactive Extension | 使用 `TriangleUp`／`TriangleDown` authoring，exact-pin current packages，執行 Notebook real path gate |

Exact package closure 是啟用 v2 marker producer 前的 deployment gate，不要求 Aster 跨 repo 修改 SPAA／Interactive Extension，也不要求兩個 repo 在同一 commit 或同一 owner task 完成。順序為：Aster 發布 owner packages與migration matrix；Daedalus 升級 SPAA／Interactive Extension並完成 focused build/runtime v2驗證；Daedalus 確認兩個 consumer bundle 相容後才啟用 producer 輸出。不得以 binding redirect、ProjectReference 或容忍 NU1608 混版。

## 7. 替代方案

1. **保留 `Arrow`，新增 `Direction` 欄位**：拒絕。會留下 `Arrow + Direction` 與其他 shape 無效 direction 的組合，增加 validation 狀態。
2. **保留 `Arrow` 並繼續由 anchor 推方向**：拒絕。無法表達四種合法組合，且混淆 spatial 與 glyph semantics。
3. **只新增 `TriangleUp`／`TriangleDown`，wire 仍叫 v1**：拒絕。舊 v2 runtime client 無法辨識 schema capability，cache 也可能把同版 payload 當成可相容。
4. **Outline 固定黑色 stroke**：拒絕。generic renderer 應服從 `Color`；黑色由 producer 傳 `#000000`。
5. **Outline 使用背景色填充**：拒絕。背景可能不是白色，也會遮蔽底層圖形。
6. **只修文件，不修跨 trace lane**：拒絕。current renderer 會讓不同 traces 的 lane 都從 0 起算，與原 RFC 的 deterministic stacking 不變量衝突。

## 8. 實作切片

| Slice | 內容 | Gate |
| --- | --- | --- |
| MDC-001 | Contracts DU、v2 codec、v1 mapping、cache schema | strict round-trip、legacy visual parity、old cache miss |
| MDC-002 | candidate aggregate lane validation | bucket 4/5、mixed anchor、cross-trace 4/5、last-good |
| MDC-003 | renderer triangle／hollow／global lane | geometry model、DOM attrs、tooltip hit area、Y-domain invariant |
| MDC-004 | PTCS／Interactive client exact cascade | unknown version fail closed、same frame parity、package graph |
| MDC-005 | Aster owner browser/client/package gate | PTCS browser-demo desktop/mobile、shared cursor、exact graph、typed runtime v2 sample |
| MDC-006 | Daedalus consumer handoff acceptance | SPAA／Interactive Extension升版、真Backtest event mapping、active run transaction與Notebook `.dib` parity；external owner |

## 9. Test matrix

| ID | Case | 預期 |
| --- | --- | --- |
| DYN-T-545 | v2 five shapes round-trip | exact case/order preserved |
| DYN-T-546 | v1 arrow above/below decode | 分別映射 `TriangleDown`／`TriangleUp` |
| DYN-T-547 | v2 rejects `arrow`／unknown shape | structured non-recoverable rejection |
| DYN-T-548 | four anchor × triangle combinations | 四種 geometry 皆獨立正確 |
| DYN-T-549 | Outline DOM/SVG + interaction | `fill=none`、stroke=color、完整hover hitbox；tooltip與shared cursor同時更新且不重建candle series |
| DYN-T-550 | bucket total 4/5 including mixed anchors | 4 accepted；5 rejected；last-good unchanged |
| DYN-T-551 | two traces share target/position/anchor | lanes globally unique and deterministic |
| DYN-T-552 | aggregate lane 4/5 | 4 accepted；5 rejected with `limit-marker-lane` |
| DYN-T-553 | cache schema 2/3 | 2 miss/resync；3 rehydrate then paused-for-resync |
| DYN-T-554 | marker-only patch | candle refs/Y-domain/cursor values unchanged |
| DYN-T-555 | Aster desktop/mobile browser-demo／Interactive Client gate | up/down、solid/hollow、tooltip、shared cursor與runtime v2 minimal frame；不要求真FSSTL／TradeCore run |

Consumer handoff acceptance 由 Daedalus 擁有：升級 SPAA／Interactive Extension exact package closure，將真 `BacktestPresentationEvent` 映射成 §4.2 四種 marker，並驗 active run、summary／trades／marker presentation revision及 ColdFar Notebook `.dib` 與 SPAA identical。此 gate 不阻擋 Aster owner package 發布；Aster browser demo也不能替代真 Notebook 驗收。

UI gate 必須用 F# Playwright verifier；不得以 JavaScript workaround 驗收。

## 10. 驗收條件

1. Producer 可在任一 anchor 使用任一 triangle direction，renderer 不從 anchor 改寫方向。
2. Hollow marker 不遮住 candle／indicator，且 hover／tooltip 操作區不退化成只有細 stroke。
3. 舊 v1 arrow payload 的畫面方向與修正前完全一致；新 encoder 不再輸出 v1／arrow。
4. 單 bucket 總量與跨 trace visual lane 上限皆 deterministic、bounded，超限不改 last-good。
5. Marker correction 不改 temporal Position authority、Y autoscale、cursor numeric value、row clipping或 revision semantics。
6. PTCS／Interactive owner browser path 使用同一 exact Contracts／Renderer graph，無混版 warning，並提供 typed runtime v2最小範例與migration matrix。
7. Consumer handoff 明確允許 Daedalus 使用 trader-facing FSSTL／TradeCore projection產生DMI marker而不自行畫SVG；真Notebook驗收是Daedalus-owned follow-up，不阻擋owner release。

## 11. Rollout 與 rollback

Rollout 順序：Aster完成 Contracts -> Renderer -> PTCS／Interactive clients -> aggregate packages與owner browser gate；Daedalus再升級SPAA／Interactive Extension並完成focused build/runtime v2 gate；producer必須在Daedalus確認兩個consumer bundle相容後才輸出`ta-marker.v2`。Aster不跨repo代改consumer，Daedalus不以owner browser demo宣稱Notebook真路徑完成。

Rollback 時 producer 停止輸出 v2 marker，回到無 marker frame；或由 current decoder 暫讀既有 v1 payload。不得讓已回退的舊 client 接收 v2 marker。schema 3 cache 可直接丟棄並重新取得 authoritative snapshot，不逆向寫成 schema 2。

## 12. 關聯

- `doc/RFC/RFC-PTCS-DYNAMIC-0015.generic-marker-overlay.md`
- `doc/TAResearch/SD.md`
- `doc/TAResearch/Test.md`
- `G:/coldfar_py/coldfar-symbolics/doc_new2/RFC/RFC-TRADECORE-0025.PTCS-GenericMarker.REQ.md`
- `G:/coldfar_py/coldfar-symbolics/doc_new2/RFC/RFC-TRADECORE-0025.PTCS-GenericMarker_review_feedback.md`

本 RFC 已實作；owner package與browser gate完成，Daedalus consumer handoff仍須在啟用真producer前完成。

## 13. Review disposition

`RFC-PTCS-DYNAMIC-0016.marker-direction-hollow-contract_feedback.md` 的 required changes 已納入：

1. §4.2 改為 long/short entry/exit 四列 frozen mapping，並區分 signal time、simulated fill time與neutral exit reason。
2. MDC-005／DYN-T-555 改為 Aster owner browser/client gate；新增 Daedalus-owned consumer handoff acceptance，不互相冒充完成。
3. §5.4 凍結 hollow hit target與shared cursor共存、不得改Y-domain/time slot或重建candle series。
4. §6／§11 將exact-pin解釋為producer啟用前的跨owner deployment closure，而非Aster跨repo同commit修改。

上述修訂完成 feedback 的 DEV 前置條件；owner實作與release evidence見下一節。

## 14. Implementation record

2026-09-21完成owner contract與release：

| Package／consumer | 舊版 | 新版 | 結果 |
|---|---:|---:|---|
| `PulseTrade.Comm.Spa.Dynamic.Contracts` | `0.1.12` | `0.1.13` | current `ta-marker.v2` direction contract、v1 visual decode、cache schema 3 |
| `PulseTrade.Comm.Spa.Dynamic.Renderer` | `0.1.29` | `0.1.30` | true hollow、full hit target、aggregate lanes |
| `PulseTrade.Comm.Spa.Dynamic.Ptcs` | `0.1.37` | `0.1.38` | exact contract closure |
| `PulseTrade.Comm.Spa.Dynamic.Ptcs.Client` | `0.1.47` | `0.1.48` | exact renderer/contract closure |
| `PulseTrade.Comm.Spa.Dynamic.Interactive.Client` | `0.1.24` | `0.1.25` | exact renderer/contract closure與typed sample |
| `PulseTrade.Comm.Spa.Dynamic` | `0.1.24` | `0.1.25` | aggregate package closure |
| `PulseTrade.Comm.Spa.Host.TAResearch.Client` | `0.1.26` | `0.1.27` | Aster active Host consumer closure |
| E2EQuotation Adapter／Browser | `alpha8`／`alpha3` | `alpha10`／`alpha4` | current document constructor與action vocabulary；`alpha9`已被`alpha10`取代 |

Owner focused suites共118/118，desktop/mobile F# Playwright與3,820-bar cursor gate通過；Host Release build、E2EQuotation focused 10/10及GW 494/494通過。六個Dynamic owner packages與三個Aster consumer packages均已發布。SPAA／Interactive Extension exact-pin與真Notebook驗收仍由Daedalus完成，owner browser evidence不替代該gate。
