# RFC-PTCS-DYNAMIC-0029：Trace Lifecycle、Cursor Events 與 Marker Contrast

- ID：`RFC-PTCS-DYNAMIC-0029`
- 狀態：Owner implemented / consumer acceptance pending
- Owner：PTCS Dynamic Contracts／Renderer／Interactive.Client／PTCS adapters（Aster）
- Consumer：PulseTrade.Comm.Spa.Dynamic.Interactive.Extension／SPAA（Daedalus）
- 關聯：`RFC-PTCS-DYNAMIC-0024`、`RFC-PTCS-DYNAMIC-0028`、`DYN-WBS-569`、`DYN-T-620..625`

## 背景

真 SPAA 驗收揭露四個同屬 renderer ownership 的缺口：dark plot 上語意為黑色的 hollow marker 雖存在但難以辨識；row data window 被固定高度與內部 scrollbar 截斷；cursor detail 只投影 Marker，無法同時呈現 navigator 的 OverviewStripe 事件；row 只能整列 hide/remove，trader 無法管理同列個別 TA trace。舊 `RFC-0022` 的 visible plot label 又與 `RFC-0028`「plot 不畫文字、文字進 OFI」衝突，必須以單一 current contract 收斂。

## 目標

1. 保留 marker semantic color，並以 renderer-owned contrast halo 讓 dark surface 上的 hollow marker 可見。
2. Plot 內不繪 marker 文字；Label／Tooltip 保留在 metadata、SVG title 與 row-local OFI event band。
3. OFI event band 同時投影 Marker 與 OverviewStripe，使用 stable event identity 去重、bounded 四筆呈現及公平 trace ordering。
4. Trader 可逐 trace hide/show/remove；remove 是 typed remote action，hide/show 是 canvas-local presentation state。
5. 最後一條 non-system trace 被 remove 後收合該 row；system overlays 不出現在 trace controls，也不獨自維持 row。
6. Data window 自動換行與增高，不使用內部 scrollbar。

## 非目標

- 不把 Signal／Order／Fill、PnL 或 scenario domain 寫入 PTCS contracts。
- 不改 OverviewStripe 的 navigator line、marker semantic stroke/fill、Y-domain 或 temporal authority。
- 不讓 Renderer 直接修改 authoritative document；`remove-trace` accepted 後仍由 host 發布新 RuntimeFrame。
- 不把 renderer-local hidden/removed state寫入 cache、provider、document fingerprint 或 scenario identity。

## 決策

### Marker contrast 與文字 ownership

Hollow marker 的 authored stroke仍是 semantic truth。Renderer先畫 pointer-inert contrast halo，再畫原 marker：halo只負責背景分離，不可改 `Color`、`Fill`、shape、anchor 或 hit target。Plot 必須維持零個 visible marker text labels；Label 與 Tooltip留在 `<title>`與 OFI，明確取代 `DYN-WBS-547/DYN-T-563` 的 plot-visible-label部分。

### Unified cursor event band

Renderer把 accepted Marker placements 與 OverviewStripe prepared events投影為共用的 generic cursor item：

```fsharp
type TaMarkerCursorItem =
    { MarkerId: string
      Label: string
      Color: string
      Tooltip: string
      EventTimeUtc: string
      Category: string
      SourceKind: string }
```

同一 stable event id 同時出現在 Marker 與 OverviewStripe 時，Marker是較完整的 row chip，stripe仍保留 navigator line但不重複顯示 chip。不同 trace 以 authored trace order round-robin，避免單一密集 marker trace吃掉四個 visible slots。空 slot且 capability存在顯示`None`；event capability不存在顯示`Unavailable`。

### Per-trace lifecycle

每個 non-system trace提供 hide/show與remove。local state以`CanvasInstanceId + RowId + TraceId`保存：

- hide/show只改 presentation，不送 host action；hidden trace仍保留 row。
- remove送 `SduiAction.RemoveTaTrace(canvasId,rowId,traceId)`，wire discriminator固定`remove-trace`。
- accepted/no-frame後本 application把 trace加入removed set，避免等待 host frame時操作無回饋；host後續frame仍是authoritative truth。
- same-canvas patch保留 local hidden/removed state；Reset Canvas、canvas identity替換、unmount清除。
- Marker／OverviewStripe屬system overlay，不提供controls；最後一條未移除的non-system trace被remove時收合row。

PTCS adapter只負責typed action round-trip，不自行刪document trace。Unknown／invalid trace action fail closed；in-flight action期間不得重入。

### Layout

Row data window使用wrap + auto height；不得固定30px或建立內部vertical scrollbar。 authored `display`由renderer記錄並在hidden state解除時恢復，不能讓inline display覆蓋hidden attribute。

## 失敗與相容性

- 舊host不接受`remove-trace`時回 controlled rejection，trace保持可見且last-good不變。
- OverviewStripe缺少label時可使用bounded tooltip/category；缺event capability才顯示Unavailable。
- stale action result、canvas generation或identity不得修改新的local state。
- 舊document無trace lifecycle metadata時仍可render；controls由renderer依generic trace kind建立。
- Renderer/Interactive.Client/PTCS adapter皆使用exact package graph，禁止以同版號替換bytes。

## Package Graph

- `PulseTrade.Comm.Spa.Dynamic.Contracts 0.1.30`
- `PulseTrade.Comm.Spa.Dynamic.Renderer 0.1.81` exact Contracts `[0.1.30]`
- `PulseTrade.Comm.Spa.Dynamic.Interactive.Client 0.1.72` exact Contracts `[0.1.30]`、Renderer `[0.1.81]`
- `PulseTrade.Comm.Spa.Dynamic.Ptcs 0.1.51` exact Contracts `[0.1.30]`
- `PulseTrade.Comm.Spa.Dynamic.Ptcs.Client 0.1.82` exact Contracts `[0.1.30]`、Renderer `[0.1.81]`

Retired且禁止採用：Renderer `0.1.79/0.1.80`、Interactive.Client `0.1.70/0.1.71`、Ptcs.Client `0.1.80/0.1.81`。

## 驗收

1. Contracts／PTCS adapters驗`remove-trace` codec、round-trip、validation與accepted/no-frame。
2. Renderer pure tests驗Marker/OverviewStripe merge、stable-id dedupe、round-robin、None/Unavailable與stripe temporal resolution。
3. F# Playwright驗dark hollow marker有halo但semantic stroke不變、plot labels=0、data window無內部scroll、逐trace hide/show/remove、最後trace收合與Reset復原。
4. 4,000-bar browser gate維持無owner phase >100ms；cursor 300 transitions記p95/max且console/page error為零。
5. Package verifier驗Interactive bundle manifest、exact dependencies與current version。
6. Daedalus以真SPAA clean package graph驗trace controls、dense cursor events、dark marker與scenario replacement；GREEN後才執行public push/readback。

## Owner Evidence（2026-09-27）

- Focused suites：Contracts `44/44`、Renderer `49/49`、Dynamic.Ptcs `15/15`、Ptcs.Client `17/17`、Interactive.Client `12/12`。
- `verify-ta-generic-marker-playwright.fsx` PASS：4,000 bars、48 paths、all `75ms`、pointer p95 `19.92ms`、max `35ms`。
- `verify-ta-renderer-playwright.fsx` PASS：300 cursor transitions p95 `35ms`、max `59ms`；正式 phases max `58.76ms`、全部低於100ms。
- Interactive package verifier PASS；desktop/mobile screenshots為`artifacts/ta-generic-marker-playwright/marker-desktop.png`與`marker-mobile.png`。
- Owner local nupkg SHA-256：Contracts `E1F046A2598A80F208D88873152D6917F5F49BECC41EFA8CC011B5342B21E747`、Renderer `5C726F3B71F6E434B1DA5D24A3F8B1774034D5808907A5377540FCA80A5DD623`、Interactive.Client `6B368E92C5A0FB741E161F42E222A29C4B41F19076B3B2F02856112492986187`、Dynamic.Ptcs `71ECA1CBF590673DE99F4E6AE6C9F52138F56C7E970ABD95B6279A32EC5F4CF2`、Ptcs.Client `51537CA62D50E4671EA85CDD4FCAE6D7FE0798FB29E43EE44D5906B0607D8D4D`。
- Consumer exact-graph gate記於`DYN-VFY-035`；此RFC尚不宣稱public release或真SPAA consumer acceptance。
