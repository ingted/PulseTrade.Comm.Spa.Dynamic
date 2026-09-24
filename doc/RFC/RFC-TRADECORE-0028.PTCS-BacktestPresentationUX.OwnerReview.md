# RFC-TRADECORE-0028 PTCS Backtest Presentation UX Owner Review

狀態：`Changes required before DEV`

Reviewer：Aster（PTCS Dynamic generic contract／renderer／client owner）

日期：2026-09-24

Review target：

- `G:/coldfar_py/coldfar-symbolics/doc_new2/RFC/RFC-TRADECORE-0028.PTCS-BacktestPresentationUX.REQ.md`
- `G:/coldfar_py/coldfar-symbolics/doc_new2/RFC/RFC-TRADECORE-0028.PTCS-BacktestPresentationUX.RFC.md`

## 1. 結論

REQ 的 trader-facing 目標、Signal／Order／Fill 分離、scenario atomic replacement、SPAA／`.dib` 共用 renderer 與 4,000-bar performance gate均接受。RFC 可作 owner RFC 的輸入，但進 DEV 前必須修正以下 contract／現況敘述：

1. `TaNavigatorEventKind = Signal | Fill` 把交易 domain 寫入 generic PTCS contract，與 non-goal 衝突；須改成 generic overview timed-stripe trace。
2. 現行 client 沒有 scenario-level monotonic `selectionGeneration`；只有單一 `backtestSelectionInFlight` 與 server presentation revision。RFC 應把 generation 明列為待開發，不可列為現況能力。
3. 現行 timeline 是 overlay frame commit 後才另行載入；尚未達到 overlay／summary／trades／timeline／downloads 同 revision 原子切換。consumer 必須先完整 prepare 並驗證 identity，才 publish active presentation。
4. 現行 marker contract 每 bucket／lane 上限均為 4；若 New／Replace／Cancel 各自保留 identity，RFC 必須同時定義 bounded collision cluster，不能靠 producer 靜默丟事件。

## 2. 五項 owner 決策

### D1. Signal／Fill 同一 navigator pixel

採「相同 X、垂直分 lane」，不採相鄰 X，也不採紅線覆蓋藍線：

- 同一 pixel 只有一個 stripe trace 時，畫完整 navigator 高度的 1 CSS px line。
- 同一 pixel 有多個不同 stripe traces 時，依 authored `LayerOrder` 將 navigator 高度等分；Signal projection 使用上 lane，Fill projection 使用下 lane。
- 相同 trace／pixel 的多筆事件只畫一條線，但 prepared state保留所有 stable ids與count。
- stripe使用batched SVG path且`pointer-events:none`；tooltip／count lookup使用預先建立的pixel bucket index，不在pointer move重掃事件。

這能保留正確event-time X位置，亦不會用水平位移暗示錯誤時間。

### D2. Order lifecycle 圓點與聚合

不做producer-level domain merge。每個canonical重大Order event保有獨立stable identity；同一Order的New／Reduce／ReplacePrice／Cancel／Rejected在事件時間不同時各自可操作。

- `Accepted／Working／PartiallyFilled／Filled`若只是狀態欄位，不另外憑空產生circle；它們出現在對應Order event tooltip。
- 同一render slot／anchor的事件由generic renderer做visual collision layout；distinct marker資料不合併。
- 超過可展開glyph budget時顯示generic `+N` cluster；keyboard／focus／tooltip可逐筆存取cluster內marker。
- Owner RFC須把wire decode hard limit與visual glyph budget分開。建議wire每lane最多64筆、畫面直接展開最多4筆，其餘cluster；超限回structured denial，不截斷。
- Fill只由triangle表達；Order狀態進入Filled不再加一顆「成交圓點」。

### D3. `HeightWeight` 與 resolved pixel height

接受雙層模型，但本輪不新增server-side preference或wire欄位：

- `TaRowSpec.HeightWeight`仍是document-authored default及Reset目標，參與document fingerprint。
- `ResolvedRowHeightPx`是renderer instance的client presentation state，以`CanvasInstanceId + RowId`索引，不改document、provider query、viewport或fingerprint。
- pointer move只做`requestAnimationFrame` local preview；pointer-up才更新一次resolved state。
- scenario切換因canvas與row identity不變而保留高度。
- browser reload／fresh `.dib` kernel回到`HeightWeight` default。跨reload durable preference不在本RFC範圍；若未來需要另立workspace-preference RFC。
- double-click／Reset Canvas刪除resolved override並回到authored default。

### D4. Navigator contract

新增獨立generic typed contract；不可沿用chart marker硬塞，也不可出現`Signal | Fill` domain DU。建議owner contract形狀：

```fsharp
type TaOverviewStripe =
    { StripeId: string
      EventTimeUtc: string
      Color: string
      StrokeWidthCssPixels: float
      Label: string option
      Tooltip: TaMarkerTooltipField array }

type TaOverviewStripeTraceOptions =
    { TargetTraceId: string
      CollisionGroup: string
      LayerOrder: int }
```

搭配generic `TaTraceKind.OverviewStripe`（最終命名由owner RFC定案）。Signal／Fill由Daedalus投影成不同trace/dataRef、顏色與layer order；PTCS只理解event time、target axis、collision group與render order。

Scenario可用三個overlay dataRefs，但必須在同一`RuntimePatch.Operations`中提交三個`ReplaceDataRef`，形成一個RuntimeFrame／presentation revision。Contracts reducer已先產生candidate並完整驗證，再接受frame；不得拆成三個frame。

### D5. Order circle palette

Order不使用Fill PnL的紅／綠。建議Daedalus projection採以下profile；PTCS只執行generic `Color + Fill`，不解析lifecycle：

| Order event | Circle | Color |
|---|---|---|
| New／Accepted／Working | Outline | blue-gray `#2563EB` |
| Reduce／ReplacePrice | Outline | amber `#D97706` |
| Cancelled | Solid | neutral gray `#6B7280` |
| Rejected／Indeterminate | Solid | violet `#7C3AED` |

Buy／Sell由AboveBar／BelowBar位置、tooltip與accessible label共同表達，不靠顏色。Fill仍使用triangle及既有進出場／PnL語意。顏色不得是唯一辨識手段。

## 3. 必修資料流

Scenario selection須改成prepare／commit，而不是overlay先commit、timeline後補：

```text
Select(run, scenario, expectedRevision, clientGeneration)
  -> resolve immutable result
  -> prepare one multi-dataRef RuntimePatch
  -> prepare summary/trades/timeline/download identity
  -> verify all use RunId + ScenarioId + ResultHash + nextRevision
  -> return one complete candidate response
  -> browser rejects stale generation
  -> reducer validates/applies frame candidate
  -> one synchronous publish updates runtime + selector + form + timeline + downloads
```

任一步失敗保留上一個完整presentation。`backtestSelectionInFlight`可作UI disable，但不能取代monotonic generation與stale-response rejection。

## 4. 必要驗收補強

1. 同pixel僅Signal、僅Fill、Signal+Fill、同類多事件四組navigator cases；assert exact X、上下lane、count及無event丟失。
2. 同Order跨K的New→Replace→Cancel與同K多事件；assert stable ids、cluster focus及完整tooltip。
3. 超過4筆但不超過wire上限的marker lane不得拒絕整個scenario；超過wire上限須structured denial並保留last-good presentation。
4. A→B→A快速切換與out-of-order response；只有latest generation可commit。
5. timeline／downloads prepare失敗時，selector、overlay、form與active revision全部維持A。
6. resize後scenario切換保持高度；Reset及reload回到`HeightWeight` default。
7. 4,000 bars切換不重建base candle／TA topology，owner browser phase無超過100ms long task。

## 5. Ownership／下一步

- Daedalus先依本review修訂RFC：移除domain-specific navigator DU、校正現況敘述、補prepare／commit及marker overflow規則。
- Aster收到修訂／feedback後建立PTCS Dynamic正式RFC，走REQ→SA→SD→WBS→Test流程。
- 未修訂前不得進DEV；其餘REQ內容不需重新討論。
