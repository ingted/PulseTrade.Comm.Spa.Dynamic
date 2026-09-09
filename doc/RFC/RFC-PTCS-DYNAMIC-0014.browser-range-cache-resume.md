# RFC-PTCS-DYNAMIC-0014 Browser Range Cache Resume

- ID: RFC-PTCS-DYNAMIC-0014
- Status: Accepted / DEV authorized
- Date: 2026-09-09
- Owner: Aster / PulseTrade.Comm.Spa.Dynamic
- Cross-owner: Daedalus / Interactive.Extension 與 SPAA host
- WBS: `doc/TAResearch/WBS.DYN-TA-018.md`
- Test: `doc/TAResearch/Test.md` DYN-TA-T-071..076

## 背景

FSSTL TA workspace 已能以 `RuntimeFrame` 顯示 3,820 筆 shared-axis history，並以 `VisibleRangeChanged` 送出半開 UTC range。但 SPAA 目前只做一次 fixed HTTP snapshot，`SubmitAction` 固定回 `workspace-action-not-ready`；browser 也只保留 process-memory last-good state。重新整理、重連或移回已載入區間時仍會重拉 K/TA，且沒有可驗證的 IndexedDB projection。

既有 `DynamicActionRequest/Result` 與 `RuntimeFrame` 不應被另一套 SPAA wire 取代。`Accepted` 只表示 action 已受理；資料狀態仍只能由 reducer 接受的 authoritative `RuntimePatch`/`RuntimeSnapshot` 改變。

## 目標

1. 讓 owner 以 opaque、stable fingerprint 標示完全相同的 source/query/TA semantics。
2. 只把 `RuntimeReducer` 已接受的 bounded document/data projection寫入 IndexedDB。
3. reload/reconnect/range revisit 可先呈現 cache last-good，再向 host做 authoritative resume/full resync。
4. 沿用 `VisibleRangeChanged`、`PollDelta`、`RequestFullSnapshot` 與 `RuntimeFrame`；不建立 SPAA-specific transport。
5. 提供純 F#/WebSharper browser implementation，以及可重複的 `.dib` + Playwright integration gate。

## 非目標

- cache 不是行情、TA、session 或 source cursor authority。
- Dynamic 不產生 FSSTL fingerprint，不解析 MDCQ query，不計算 TA/calendar/bucket。
- 不快取 credential、WebSocket capability、actor address或任意 host secret。
- 不以 cache hit 靜默略過 server validation；offline mode 另案設計。
- 不把完整歷史無界寫入單一 record，也不建立第二套 renderer/runtime state machine。

## 情境

1. **首次開啟**：無 cache，browser request full snapshot；接受後寫入 bounded entry。
2. **重新整理**：document/fingerprint相符時先顯示 `CACHED / RESYNCING`；host確認 revision後送delta或full。
3. **回到已載區間**：covering entry可先顯示；同一筆 `VisibleRangeChanged`仍送host，後續frame取代cache。
4. **fingerprint或schema改變**：不讀舊資料，直接full snapshot；舊entry可由LRU清理。
5. **corrupt/oversize/gap**：cache decode/validation失敗即刪除該entry，不改last-good、不放寬frame限制。

## 方案取捨

### A. SPAA 自建 IndexedDB/wire

拒絕。會複製 reducer、frame ordering與renderer cache，並把 FSSTL application變成第二個 Dynamic runtime。

### B. 只以 URL/session id 當 cache key

拒絕。session/capability會重建，且 URL 無法證明 instrument、range、source profile與TA參數相同。

### C. owner fingerprint + Dynamic projection metadata

採用。owner只產生 opaque canonical fingerprint；Dynamic擁有 document/canvas/data revision、range coverage、codec、validation與browser storage。

## 決策

### 1. Cache identity

```fsharp
type RuntimeCacheIdentity =
    { OwnerFingerprint: string
      SchemaRevision: int64 }

type RuntimeCacheCoverage =
    { StartEventTimeUtc: DateTimeOffset
      EndEventTimeExclusiveUtc: DateTimeOffset }

type RuntimeCacheEntry =
    { CacheIdentity: RuntimeCacheIdentity
      WorkspaceId: string
      Document: TaWorkspaceDocument
      Snapshot: RuntimeSnapshot
      DocumentRevision: int64
      DataRevision: int64
      Coverage: RuntimeCacheCoverage
      CapturedAtUtc: DateTimeOffset }
```

Dynamic維持generic identity：`OwnerFingerprint`是owner產生的opaque canonical identity，Dynamic只驗nonblank、長度與schema revision，不重算其內容。SPAA以stable ProgramFingerprint、DataSourceFingerprint及cache semantic/schema version產生OwnerFingerprint；requested/cached time range不得進入。可含range的`QueryFingerprint`只供單次query診斷。exact reload只比對OwnerFingerprint；同owner的range revisit先由Dynamic驗coverage，再交owner確認provider authority並等待authoritative frame。session-specific capability不進cache。

### 2. Authoritative handshake

```text
host -> Document frame
browser -> validate document/cache identity
  cache hit -> hydrate validated last-good -> PollDelta(cachedDataRevision)
  cache miss -> RequestFullSnapshot(cache-miss)
host -> validate owner fingerprint/revision
  can resume -> RuntimePatch(base=cachedDataRevision)
  cannot resume -> RuntimeSnapshot
browser -> RuntimeReducer accepts candidate -> render -> cache write
```

現有 host若仍立即送 `Document + Snapshot`，browser維持相容並以新Snapshot為準。支援resume的host才採document-first handshake。cache hit不直接產生action success，也不允許browser自行提高revision。

### 3. Range action

Renderer維持送一筆 correlated `DynamicActionRequest(VisibleRangeChanged)`。Interactive.Client可並行查 covering cache並顯示stale projection，但同一request仍送server。`DynamicActionResult.Accepted`只解除command pending；對應資料由後續frame套用。cache沒有correlation authority，不以action request id拼出假frame。

### 4. Rehydrate與寫入 gate

- rehydrate先用當前document與cached snapshot建立candidate，再走與frame相同的validation/reducer規則。
- cache entry的舊DocumentId/CanvasInstanceId不得覆蓋當前session identity。
- 只有無`RequestResync` effect、Document存在、Data完整且coverage可解析時可寫。
- heartbeat、Error、rejected action、invalid patch、paused-for-resync均不寫cache。
- cache state只作stale presentation；status明確顯示`CACHED / RESYNCING`直到authoritative frame成功。

### 5. IndexedDB storage

- Database: `PulseTrade.Comm.Spa.Dynamic.Interactive`
- Store: `runtimeSnapshots`
- Schema version: `1`
- 每entry仍受既有16 MiB frame上限；預設最多8 entries，按`CapturedAtUtc` LRU刪除。
- 實作用WebSharper F# `JS.Get/JS.Apply/JS.Set` typed boundary；不新增手寫`.js`或inline JavaScript。
- storage unavailable/denied/quota exceeded時降級為no-cache，不阻止WebSocket authoritative path。

## 影響範圍

| Owner | Project | Change |
| --- | --- | --- |
| Aster | Dynamic.Contracts | cache identity/entry/coverage validation、rehydrate/write decision pure API |
| Aster | Dynamic.Interactive.Client | IndexedDB adapter、cache-first stale render、resume/full request、LRU |
| Aster | Dynamic tests/scripts/docs | unit/package/browser/.dib integration gates |
| Daedalus | Interactive.Extension | canonical owner fingerprint；document-first resume/full action handler |
| Daedalus | SPAA | 移除`workspace-action-not-ready`，將range/resync導向authoritative frame producer |
| MdcQuoteAgent | MdcQuote Next | 提供per-scale warm-up/range capability；不接觸browser cache |

## 驗收

1. cache identity blank/oversize/negative revision、invalid coverage、corrupt/oversize payload fail closed。
2. accepted snapshot/patch可建立entry；invalid/gap/error/rejected action不寫入。
3. cache rehydrate不得沿用舊session identity或提高authority revision；新snapshot必能取代。
4. IndexedDB在reload後仍存在，cache hit先顯示last-good且只建立一條WebSocket；server full/delta後回READY。
5. range cache hit/miss都恰送一次server action；Accepted前後都不能冒充frame。
6. `.dib`以真FSSTL多尺度 `FloatingPoint`/Frame/map normalization建立workspace；Playwright驗range revisit、network request、cache reload與console 0。

## Rollback

cache為opt-in capability。移除cache identity/capability後，Interactive.Client回到既有WebSocket full-snapshot路徑；IndexedDB舊entry由schema/LRU清除，不影響server truth或Notebook session。

## 關聯文件

- `doc/RFC/RFC-PTCS-DYNAMIC-0013.notebook-ta-workspace-production.md`
- `doc/TAResearch/REQ.md`
- `doc/TAResearch/SA.md`
- `doc/TAResearch/SD.md`
- `doc/TAResearch/WBS.DYN-TA-018.md`
- `doc/TAResearch/Test.md`
- `G:\coldfar_py\coldfar-symbolics\doc_new2\Integration\Aster.SPAA_DIExt_TA_Workspace.md`
