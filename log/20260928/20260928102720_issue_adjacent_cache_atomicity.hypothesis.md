# Issue: adjacent cache atomicity

## Context

RFC-PTCS-DYNAMIC-0032 的 adjacent cache selection 會先把 cached page document rebase 到目前 accepted loaded coverage，再交由 phased rehydrate 套用。

## Observation

- `BrowserRuntimeCache.selectAdjacent` 回傳的 `RebasedEntry` 同時包含該頁的 `Document` 與 `Snapshot`。
- `RuntimeCacheProjection.tryCreateRehydrateFrame` 只從 entry 建立 snapshot frame。
- `RuntimeCacheProjection.completeRehydrate` 保留 current document，因此 A -> B -> cached A 可能得到 A data 搭配 B `activeDetail`/`queryGeneration`。

## Hypothesis

Cache entry 本身是同一次 capture 的 atomic presentation commit。Rehydrate 必須在保留目前 runtime identity、authoritative revision 與 transport envelope 的前提下，同時套用 entry document 與 snapshot candidate；否則 consumer 被迫自行拼接 owner document。

## Experiment

1. 新增 A -> B -> cached A regression，要求 rehydrated document 與 data 都來自 A entry。
2. 保留既有 `completeRehydrate` API，新增 entry-aware completion 並讓 sync/phased production paths 使用。
3. 跑 Contracts、Renderer、Interactive、Dynamic.Ptcs、Ptcs.Client focused suites 與 BrowserDemo E2E。

## Scope

- `src/PulseTrade.Comm.Spa.Dynamic.Contracts/RuntimeCache.fs`
- `src/PulseTrade.Comm.Spa.Dynamic.Interactive.Client/BrowserCache.fs`
- 對應 tests、version graph、RFC/WBS/Test/DevLog。

## Stop Condition

若 cached document 無法通過既有 cache identity/workspace/schema validation，仍須拒絕 rehydrate；不得以 consumer-side document assembly 或 relaxed validation 繞過。

## Result

- Hypothesis confirmed：舊phased path可把cached A snapshot套入current B document。
- `tryPrepareRehydrate`先驗完整entry並以cached Document/View建立candidate；`completeRehydrateEntry`保留current authoritative revisions/transport，再atomic commit cached presentation。
- DYN-T-648C以250筆真實A temporal data、rebased ordinal 250及latest generation/segments驗證；同一publish的Renderer狀態為`Viewing 251-500`。
- Focused suites `47/55/15/15/17`與BrowserCache/Renderer F# Playwright皆通過；舊candidate graph retired，新graph為`0.1.33/0.1.86/0.1.77/0.1.54/0.1.87`。
