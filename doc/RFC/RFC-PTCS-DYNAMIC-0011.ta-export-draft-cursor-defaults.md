# RFC-PTCS-DYNAMIC-0011 TA Full Export / Draft Query / Cursor Geometry

- Status：Implemented
- Date：2026-07-15
- Owners：Dynamic.Ptcs server / Ptcs.Client / Renderer
- Supersedes：RFC-0010 `Copy JSON`輸出compact canonical document的行為
- Related：`doc/TAResearch/REQ.md`、`doc/TAResearch/WBS.DYN-TA-015.md`
- Host companion：`G:\PulseTrade.fs\Libs\PulseTrade.Comm\doc\RFC\RFC-PTC-PTCSH-0005.TAExportDraftCursorDefaults.md`

## 背景

TA durable reply故意只含compact `SduiDocument`，OHLCV與indicator series只在reply展開後的transient browser state。現有「複製 JSON」因此只能取得layout/query metadata，不足以離線重建研究圖。另有三個interaction缺口：interval select立即修改reactive state而在Apply前重render；cross-row cursor以endpoint比例計算，K棒則以slot center計算；indicator線在2000-bar研究圖偏粗。

## 目標

1. 將copy action改成下載完整runtime export，檔名`yyyyMMddHHmmss-<GUID>.json`。
2. export包含document、query/provider metadata、timeline、OHLCV、indicator series、revision與freshness/quality。
3. collapsed/unmounted狀態平時不開channel、不poll；使用者明確按下載時可建立一次性headless channel，取得full snapshot後立即dispose。已展開時沿用既有channel。
4. query inputs維持local draft，只有Apply送typed action並觸發authoritative render。
5. cursor與K棒共用slot-center geometry；indicator line由Host/trace metadata控制並使用較細預設。

## 非目標

- 不把OHLCV加入durable Document、chat journal、IndexedDB message store或PCSL。
- 不新增server download endpoint或server-side file。
- 不修改PTCS core、RN、PTMD、E2EQ。
- 不使用handwritten JavaScript或inline JS。

## 決策

### Export lifecycle

`TaResearchTransientClientHandle`新增`RequestJsonExport`。已展開時由既有handle排隊送`RequestFullSnapshot`；收合時`requestJsonExportOnce`建立不mount renderer的一次性client，在initial mount完成後送full request，下載成功即dispose。server看到該action必須使用`stateToWire next`回`full`，不能用`stateToWireAgainst(Some current)`產生空delta。

client收到合法full wire後建立：

```fsharp
[<CLIMutable>]
type TaResearchJsonExport =
    { schema: string
      exportedAtUtc: string
      documentRevision: int64
      dataRevision: int64
      state: TaBrowserStateWire }
```

以typed WebSharper `Blob`、`URL.CreateObjectURL`及`HTMLElement.Click`下載。pending export在success/error/close/dispose都清除；不因下載改history、viewport或poll cadence。

### Draft query

instrument/interval/from/to改為非reactive draft。document revision變更才同步authoritative值；poll/data revision不得覆蓋使用者尚未Apply的draft。Apply讀draft並送一次`ChangeQuery`。

### Slot geometry

```text
slot = width / visibleCount
x(index) = slot * (index + 0.5)
index(pointerRatio) = floor(pointerRatio * visibleCount)
```

K棒、line points、cross-row cursor與hit-test共用此公式。第一/最後一根落在半slot內，不再落在SVG邊界。

## 主要type/function變更

| Module/file | Change |
| --- | --- |
| `Server/TaResearchTransient.fs` | `RequestFullSnapshot` branch強制full wire |
| `Dynamic.Ptcs.Client/Client.fs` | handle新增`RequestJsonExport`；export pending/full wire/download envelope |
| `Dynamic.Ptcs.Client/ReplyPresentation.fs` | `copy-json`改`download-json`；mounted沿用handle，collapsed使用one-shot client |
| `Dynamic.Renderer/Renderer.fs` | query draft與slot-center projection |
| `Dynamic.Renderer/RendererModel.fs` | pointer ratio映射slot index |

## 影響範圍

- 修改Dynamic server adapter、PTCS browser client與renderer package；不修改PTCS core、RN、PTMD或E2EQ。
- `copy-json` presentation action由`download-json`取代；這是明確的UI action compatibility change，既有durable document schema不變。
- Host consumer需exact-pin本RFC發布的Dynamic package graph，正式服務需重新部署後執行cross-repo browser gate。

## 驗收

1. 下載檔名符合規格，JSON parse後具2000 timeline、OHLCV與所有indicator series；revision/query metadata一致。
2. collapsed平時channel/poll為0；明確點下載後只有bounded one-shot mount/full/close，檔案成功且不啟動週期poll。
3. interval select後跨至少一個poll，render sequence、server query與remote action count不變；Apply後恰一次更新。
4. first/middle/last cursor X與K棒center誤差<=1px，所有rows共用同一X。
5. F# model/component與F# Playwright通過；無handwritten JavaScript。

## 關聯文件

- Current state：`doc/TAResearch/REQ.md`、`SA.md`、`SD.md`、`WBS.md`、`Test.md`。
- Host companion：`G:\PulseTrade.fs\Libs\PulseTrade.Comm\doc\RFC\RFC-PTC-PTCSH-0005.TAExportDraftCursorDefaults.md`。
- Verification owner：`G:\PulseTrade.fs\Libs\PulseTrade.Comm\scripts\verify-ptcs-host-ta-research-live.fsx`。
