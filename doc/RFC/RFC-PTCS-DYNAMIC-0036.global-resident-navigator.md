# RFC-PTCS-DYNAMIC-0036 Global Resident Navigator

- ID: RFC-PTCS-DYNAMIC-0036
- Status: Accepted / owner package and browser validation passed; formal consumer validation pending
- Date: 2026-10-21
- Owners: PTCS.Dynamic Renderer / Aster; Research host consumer / Daedalus
- Related: RFC-PTCS-DYNAMIC-0032, RFC-PTCS-DYNAMIC-0033, RFC-PTCS-DYNAMIC-0034

## Background

The existing navigator describes the observations currently resident in the client and moves the visible chart window inside that resident interval. A Research document may have a much larger durable series, for example 2018 through 2026, while the client intentionally keeps only 1,500, 3,000, or 4,500 observations resident. Reusing the existing navigator for both domains makes a global relocation look like an inner viewport move and couples resident capacity to the 4,000-detail validation path.

## Goals

1. Add an optional whole-series overview above the existing resident navigator.
2. Let one pointer release request one atomic relocation of the resident interval.
3. Preserve the inner viewport width and resident-relative offset when the resident interval moves.
4. Keep the existing projection, actions, renderer, and wire behavior unchanged when the optional overview is absent.

## Non-goals

- No Research provider, query, storage, or derived-TA implementation in the owner package.
- No second coverage authority, polling loop, or per-pointer-move remote action.
- No change to the existing visible-range action or its 4,000-detail policy; D6 explicitly corrects capped inner-handle symmetry.
- No hard-coded 4,500 limit in generic contracts; the consumer advertises the current resident width.

## Decision

### D1. Additive whole-series projection

`TaLoadedCoverageProjection` gains `GlobalOverview: TaGlobalCoverageOverview option`. The overview carries the bounded whole-series axis and the resident interval within it:

```fsharp
type TaGlobalCoverageOverview =
    { TotalObservationCount: int64
      AxisRef: string
      Anchors: TaOverviewAnchor array
      ResidentStartObservationOrdinal: int64
      ResidentObservationCount: int }
```

The existing outer `CoverageIdentity`, `CoverageRevision`, and `QueryGeneration` remain the only authority and settlement identity. `GlobalOverview=None` preserves the 0.2.2 wire and renderer behavior.

### D2. Separate resident relocation action

The action is independent of `VisibleRangeChanged`:

```fsharp
ResidentCoverageChanged of
    CanvasInstanceId *
    baseRowId: string *
    relocation: TaCoverageWindowIntent
```

`BaseRowId` identifies the current document/base series and remains an action field. `TaCoverageWindowIntent` remains a pure ordinal relocation intent. The new path validates positive count, matching advertised resident count, and containment inside the whole-series total. It does not apply `VisibleRangeChange.MaximumBasePoints`, `MaximumVisibleRangeBasePoints`, or `MaximumActiveDetailBars`.

### D3. Local draft and settlement

Global drag changes only a renderer-local draft. Pointer release emits at most one `ResidentCoverageChanged`. Matching coverage revision and query generation commit the projection; rejected, conflicted, or superseded actions clear or replace only their own global draft. No timer, polling loop, or action is emitted during pointer movement.

### D4. Preserve the inner viewport by relative offset

When the resident interval moves, the inner viewport keeps its width and its offset relative to the resident start. For resident width 3,000, inner offset 1,500, and viewport width 700, relocation to `newResidentStart` produces `newResidentStart + 1,500` with width 700. Near a boundary, clamp the offset while preserving width; shrink width only when the complete series itself is shorter. The old absolute ordinal is not retained.

### D5. The visible selection owns the next gesture

While a remote coverage action awaits its response or matching projection, the renderer may show a newer optimistic selection than the authoritative projection. A new pointerdown therefore takes its index baseline from the rendered or published draft and its ratio baseline from the navigator's currently rendered `data-selection-left-ratio` / `data-selection-right-ratio`. It must not clear that draft before choosing the baseline. Internal ratio overrides are fallback state only; CSS/SVG bounding-box conversion is not authority because borders and view-box scaling can introduce visible edge drift.

## Impact

### D6. 2026-10-23 capped resident handle mirror

Owner daedilus轉交使用者指定：保留既有left外拉在maximum visible後維持寬度並平移的行為，right作幾何鏡像；內拉仍縮小。4500 resident、start1500/count1500、maximum1500時，left delta-300為start1200/count1500，right delta+300須為start1800/count1500。跨maximum時僅超出部分平移，resident端點仍clamp；不改left、body move、cancel、drop-only action、outside paging或wire。DYN-WBS-587 / DYN-T-685先對0.2.23加失敗回歸，再最小修right計算；package與consumer restart須等待owner完成latency baseline。

GLOBAL更新接近兩分鐘另列latency P0（DYN-WBS-588）：四項functional真3M PASS不代表latency通過。首輪15.096秒含已知probe干擾，非正式基線；須以低干擾drop/request/body/decode/prepare/projection-commit分段定位，不以放大timeout或改queue掩蓋。

### D7. 2026-10-23 bounded browser work turns

低干擾0.2.23實測body→prepare約5.7–6.2秒、prepare→commit約3.07–3.14秒，CPU以idle為主；逐256-point decode及逐entry prepare都強制等一個RAF造成排程下限。0.2.24候選共用既有schedule seam的FIFO executor，每turn以4ms/64 atomic steps雙上限處理廉價工作，達任一上限才再排RAF；保留256-point task、順序、每step generation檢查、last-good及final projection paint barrier。單一atomic step仍不可搶占，4ms是soft budget，並非單task硬上限。沒有新worker、service、action queue或timeout。

`documentElement`的`data-frame-pump-*`及`data-renderer-prepare-*`公開work-count/turn-count/work-ms/queue-ms/wall-ms/max-turn-ms/pending-count；前者sequence識別最後pump batch，後者generation+instance識別prepare，建立新batch時清零，舊generation不覆寫。這是per-batch觀測，不冒充整個HTTP action；原onStageCompleted仍只表示該atomic task執行時間，signature不改。FramePump四條browser wrappers、WebSocket decode/reduce入口及Renderer full/incremental preparation用同一executor；action settlement、row refresh與最終paint barrier維持原路徑。

- Contracts and wire codecs gain optional/additive fields and one action discriminator.
- Renderer gains a second, visually separate navigator and an independent draft/pending state machine.
- Interactive.Client transports the new action without Research-specific vocabulary.
- Research host owns whole-series anchors, durable relocation authority, resident materialization, and derived-series shared-axis updates.

## Current owner state (2026-10-22)

- Contracts and renderer source now contain the additive action allowlist, independent move-only global strip, local draft/pending settlement, and a BrowserDemo 10,000-total/1,500-resident fixture. Contracts Expecto passes 51/51 and Renderer model/source-contract Expecto passes 71/71.
- The first attempted `0.1.37 -> 0.2.9 -> 0.2.9` artifacts are retired: `GeneratePackageOnBuild=true` caused direct `dotnet pack` to reuse an older Release DLL, so those packages do not contain this UI.
- Owner candidates are Contracts `0.1.38`, Renderer `0.2.10`, and Interactive.Client `0.2.10`. All three passed fresh local-first package publication; Renderer and Interactive.Client local package SHA-256 values are `ECCD46DADDEEAD7AAE3F9BAD488552D84923E658FD4139FC88DA6387FDB6DCE1` and `84BB347B698CDC9BC15791CC842A78C29BBE2804CCA7F2378E5B4EAE72F460AA`. The package-consuming BrowserDemo build passes, and its Expecto Playwright gate passes the existing resident gesture plus the independent global relocation/drop-only action. Daedalus formal 18884 consumer validation remains pending.
- Existing `0.2.8` remains the valid chained-gesture release and does not contain the global strip.

## 2026-10-23 resident drop correction

- DYN-WBS-587 / DYN-T-683：resident drop保留rendered draft，等action settlement與matching committed coverage兩者滿足才清除；同resident中的較新revision不得取消正在操作的手勢，排送時使用最新revision/generation。
- DYN-WBS-588 / DYN-T-684：body pointer仍在track內時先clamp至端點；只有pointer真的越出track才要求相鄰頁，目標以resident邊界加減完整page，保持visible width。
- Renderer / Interactive.Client候選0.2.21沿用既有wire；focused與真3M快速連續drag、舊兩黑閃/global回歸須分開驗。尚無獨立完整resident payload descriptor，不能將ActiveDetail width改4500；此資料契約缺口保持open，consumer owner為daedilus。

## Acceptance

0.2.23 authority clarification（DYN-WBS-588）：GlobalOverview存在時，其TotalObservationCount是導航上限；頂層loaded total未知不能把全域導航clamp到已載入尾端。renderer intent/queue/followLatest與adjacent-cache同用此authority；GlobalOverview=None保留legacy。不因此把未載入ordinal視為local payload。

0.2.22 clarification（DYN-WBS-587）：最後pointer位置而非上一個painted frame為drop intent authority；pointerup必須吸收尚未paint的最後move並同步hold geometry。0.2.21實例local1498→1423的75bars偏差為獨立RED；release不得要求人類等下一RAF再放開。右外拉無action及完整resident payload仍open。

1. Existing 0.2.2 tests remain unchanged and pass with `GlobalOverview=None`.
2. Projection codec round-trips both absent and present global overview; old payloads decode to `None`.
3. Action codec round-trips `ResidentCoverageChanged` with `BaseRowId` outside the relocation intent.
4. A 4,500-observation resident relocation is accepted by the generic path and is not clamped to 4,000.
5. Drag emits no remote action; one drop emits exactly one action.
6. Settlement is revision/generation-safe and stale results cannot clear a newer draft.
7. Inner viewport width and resident-relative offset are preserved across relocation, with bounded edge clamping.
8. Daedalus formal consumer proves whole-series navigation, 2018/2026 relocation, derived-TA shared axis, and existing inner navigator behavior.
9. Pointerdown freezes the rendered selection ratios and semantic left/right/body ownership. An authoritative projection arriving before pointerup may rebuild the shell but cannot move the active preview, its fixed edge, or body width; pointerup still emits exactly one intent.
10. If a second gesture starts while the first gesture's remote response is still pending, pointerdown keeps the exact rendered edge as its baseline. The first response may arrive while the second gesture is held without moving its fixed edge, changing its body width, clearing its semantic mode, or suppressing its one later intent.
11. At and across the advertised maximum visible width, right resize mirrors the existing left resize, including inward shrink, minimum width, empty/short domains and both resident boundaries. Unit geometry and real 4500/1500 consumer gestures are separate gates.
12. GLOBAL latency acceptance records low-overhead drop-to-ready phases and probe overhead; functional frame continuity alone does not close this performance gate.
