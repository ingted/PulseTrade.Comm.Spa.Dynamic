# RFC-PTCS-DYNAMIC-0005 Actors Page Renderer

狀態：Proposed / Review

日期：2026-06-28

關聯文件：

- `G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\doc\RFC-PTC-SPA-0010.actors-page-dynamic-dsl-rendering.md`
- `C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic\doc\RFC-PTCS-DYNAMIC-0004.actor-dynamic-action-modes.md`
- `C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic\doc\RFC-PTCS-DYNAMIC-0003.unified-sdui-form-dsl-roadmap.md`
- `C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic\doc\SDUI_DSL_zh-Hant.md`
- 使用者視覺回饋：`G:\PulseTrade2.fs\misc\2026-06-28_actors tab page.png`

## 背景

PTCS `RFC-PTC-SPA-0010` 將 `/actors` 的 final design 改成 page-level SDUI：

- 有 Dynamic extension 且支援 `ActorsPage` renderer 時，整個 `/actors` page 由 Dynamic DSL renderer 呈現。
- 沒有 Dynamic extension 或沒有 `ActorsPage` renderer 時，PTCS core 不走 renderer，直接顯示 fallback tree/grid/table。

這和既有 Dynamic generic Canvas renderer 不同。`Actor Dynamic` message reply 的 Canvas renderer 只負責一般訊息 payload，例如 echo 回 `schema=fskynet-sdui` JSON 後畫 canvas。Actors page 則是固定路由 `/actors` 的 topology page，不應呈現 raw JSON preview card，也不應要求使用者點 `展開 Canvas`。

因此 PTCS.Dynamic 需要自己的 renderer-side RFC，定義如何 claim `ActorsPage`，如何 interpret ActorTopology DSL，如何把 node blocks、tree、grid、cards 與 controls 視為同一份 page document。

## 目標

1. 新增 Dynamic extension 的 page-level renderer contract：`ActorsPage`。
2. 明確區分：
   - generic Canvas message renderer：處理 `Actor Dynamic` reply payload。
   - ActorsPage renderer：處理 `/actors` 整頁 topology document。
3. Dynamic renderer 必須一次 render 整個 Actors page，包括：
   - page header / sync status；
   - PTCS/GW/RN/Unknown node blocks；
   - per-node actor hierarchy tree；
   - grid/table details；
   - cards / node summary；
   - reload / report generation / report schedule controls。
4. Node grouping 以 `actorSystem@host:port` 為 key；不同 port 必須分不同區塊。
5. Node block 排序為 PTCS Host -> GW Host -> RN Host -> Unknown。
6. 保持 WebSharper F# 實作；不新增 JavaScript source 或 inline JavaScript。

## 非目標

1. Dynamic 不擁有 Actor Registry truth source、PCSL projection、Journal、MessageFabric 或 `/actors/api/*`。
2. Dynamic 不負責判斷 PTCS/GW/RN service 是否真的部署成功；只根據 PTCS 提供的 DSL 欄位呈現。
3. Dynamic 不取代 PTCS no-Dynamic fallback。
4. Dynamic 不在 `/actors` 使用 generic Canvas preview card 充當 ActorsPage support。
5. 本 RFC 不要求實作 production RN.Host split-service proxy proof；那仍屬 PTC/RN WBS。

## 決策

### D1. ActorsPage is a dedicated renderer kind

Dynamic 必須註冊 dedicated page renderer，而不是讓 generic message renderer claim `/actors` payload。

Conceptual discriminator：

```text
schema = "fskynet-sdui"
surface = "ActorsPage"
documentType = "ActorTopologyPage"
```

`surface=Canvas` 仍保留給一般 Dynamic canvas message。`surface=ActorsPage` 才能 render `/actors`。

### D2. No placeholder support

若 Dynamic extension 尚未實作 `ActorsPage` renderer，必須回報 `None` / not-supported，讓 PTCS core 使用 fallback。

禁止以下行為：

- 顯示 `FSkynet 動態畫布 (Canvas)` summary card；
- 顯示 raw JSON preview；
- 顯示 `展開 Canvas` button；
- 只 render tree 但讓 PTCS core fallback grid/table 混在同頁。

### D3. Whole page ownership when claimed

Dynamic 一旦 claim `ActorsPage` document，就要 render 整個 page。PTCS core 不應再同時 render fallback tree/grid/card DOM。

Dynamic renderer output 至少包含：

```text
ActorsPage
  Header
  NodeBlocks
    PTCS Host block(s)
    GW Host block(s)
    RN Host block(s)
    Unknown block(s)
  DetailsGrid
  SummaryCards
  ActionControls
```

### D4. Node grouping is transport-aware

Dynamic 使用 PTCS DSL 提供的 normalized node identity，不自行從 label 猜測 port。若 DSL 沒有 explicit node group，第一版 renderer 可以根據 actor address parse：

```text
akka.tcp://<actorSystem>@<host>:<port>/<path>
```

但 parse 結果只能作 UI grouping，不得回寫 registry 或 canonical state。

### D5. Role ordering is stable and visible

Renderer 依 role hint 排序：

1. `ptcs-host`
2. `gw-host`
3. `rn-host`
4. `unknown`

Node block header 顯示完整 node address / identity，避免只看到局部 path：

```text
PTCS Host
akka.tcp://PtcsExtHost...@127.0.0.1:13884
```

### D6. Tree layout prioritizes inspectability

ActorsPage tree 不是 decorative canvas。它必須可檢查、可水平 scroll、可看完整 actor address。

Required visual properties：

- tree nodes 有直角 connector lines；
- expandable node 有 boxed `+` / `-`；
- active/degraded/stale 有明確狀態 marker；
- full `akka.tcp://...` address 不用 ellipsis 截斷；
- tree block 的 horizontal scroll 不影響 grid；
- grid/table details 作為 tree 後方的 sortable/filterable detail view。

### D7. Controls are DSL actions

reload、state report generation、report schedule 不應由 PTCS core fallback 硬塞在 Dynamic page 外面。Dynamic path 下這些操作必須由 DSL action controls 描述與 render。

第一版可先支援：

```text
Action.Reload
Action.GenerateReport(outputDirectory)
Action.ScheduleReport(outputDirectory, interval)
```

實際 HTTP command endpoint 仍由 PTCS core 提供。

## ActorsPage DSL interpretation sketch

此草圖對應 PTCS RFC-0010；正式 type/codec 於 SD 階段固定。

```text
ActorsPageDocument
  schema: "fskynet-sdui"
  surface: "ActorsPage"
  documentType: "ActorTopologyPage"
  version: 1
  projectionVersion: int64
  syncedAtUtc: string
  nodeGroups:
    - role: "ptcs-host" | "gw-host" | "rn-host" | "unknown"
      nodeKey: string
      displayName: string
      actorSystem: string
      host: string option
      port: int option
      status: "active" | "degraded" | "stale" | "unknown"
      tree:
        rootNodes: ActorTreeNodeDsl list
  grid:
    columns: GridColumnDsl list
    rows: ActorGridRowDsl list
  cards:
    - kind: string
      title: string
      fields: FieldDsl list
  actions:
    - kind: "reload" | "generate-report" | "schedule-report"
```

## 影響範圍

| Area | Expected change |
| --- | --- |
| `Client/DynamicRenderer.fs` | 保留 generic Canvas message renderer；不得 claim `ActorsPage`。 |
| new/extended page renderer module | 註冊 `ActorsPage` renderer，decode ActorTopology DSL，render full page。 |
| `SDUI_DSL_zh-Hant.md` | 補 `surface=ActorsPage` 與 ActorTopology page node/grid/card/action vocabulary。 |
| `SDUI_Developer_Manual.md` | 說明 ActorsPage renderer 與 Canvas message renderer 的差異。 |
| `tests` | 補 DSL decode、node grouping、role ordering、full address rendering model tests。 |
| PTC cross-repo Playwright | 驗證 public/local `/actors` Dynamic path 不出現 Canvas preview card，且 fallback path 仍可用。 |

## 驗收

| Test ID | Scenario | Expected |
| --- | --- | --- |
| DYN-T-526 | ActorsPage renderer registration | Dynamic 可以明確 claim `surface=ActorsPage` / `documentType=ActorTopologyPage`。 |
| DYN-T-527 | Generic Canvas does not claim ActorsPage | `surface=ActorsPage` 不會落入 generic Canvas summary card。 |
| DYN-T-528 | Node grouping by host/port | 不同 `actorSystem@host:port` 產生不同 node block。 |
| DYN-T-529 | Role ordering | PTCS Host -> GW Host -> RN Host -> Unknown 順序穩定。 |
| DYN-T-530 | Full-page render contract | tree、grid、cards、actions 由同一份 ActorsPage DSL render。 |
| DYN-T-531 | Full address inspectability | actor address 完整可見，寬度不足時 block-level horizontal scroll。 |
| DYN-T-532 | Dynamic absent / unsupported path | Dynamic 不支援 ActorsPage 時回 `None`，PTCS fallback 生效。 |

## Open Questions

1. `role` hint 應完全由 PTCS DSL 提供，還是 Dynamic 第一版允許 best-effort name-based inference？
2. Grid sorting/filtering state 是否由 Dynamic renderer local state 管理，或沿用 PTCS `/actors` IndexedDB namespace？
3. Report schedule UI 的 interval / output path validation message 是否由 PTCS API 回傳 DSL validation state，或 Dynamic 先做 client-side shape validation？

## 結論

PTCS.Dynamic 需要獨立 ActorsPage renderer contract。這個 renderer 不是 generic Canvas preview，也不是只 render tree 的 widget；它是 `/actors` 整頁 Dynamic DSL presentation layer。

Implementation path：

1. PTCS emits `ActorsPage` DSL.
2. Dynamic registers `ActorsPage` renderer.
3. Dynamic-supported `/actors` becomes fully DSL-rendered.
4. Dynamic-unsupported `/actors` stays on PTCS fallback tree/grid/table.
