# @DYN-TA-018 Browser Range Cache Resume

- RFC: `doc/RFC/RFC-PTCS-DYNAMIC-0014.browser-range-cache-resume.md`
- Status: Active
- Progress: 68%

| Slice | Deliverable | Test | Progress | Status |
| --- | --- | --- | ---: | --- |
| DYN-TA-018A | RFC/REQ/SA/SD/WBS/Test/Verification文件鏈 | T-071 | 100% | Done |
| DYN-TA-018B | cache identity/coverage/entry validation與pure rehydrate/write gate | T-072/073 | 100% | Done；Contracts tests 21/21，共用browser/server semantic validator |
| DYN-TA-018C | WebSharper F# IndexedDB store、bounded LRU與no-cache fallback | T-074 | 100% | Done；Playwright persistence/LRU/coverage/corrupt/semantic-invalid/clear gate pass |
| DYN-TA-018D | Interactive.Client cache hydrate、resume/full handshake與range revisit | T-073/075 | 40% | `writeAcceptedState`與`tryRehydrate` WebSharper API、paused local interaction/remote suppression已完成；仍待Daedalus SPAA application lifecycle接線與authoritative resume/full |
| DYN-TA-018E | FSSTL `.dib` + SPAA Playwright reload/range/cache E2E | T-076 | 0% | Depends D + Daedalus host + MDCQ per-scale warm-up |

## Owner boundary

- Aster：generic cache contract、reducer gate、browser storage/client、acceptance harness。
- Daedalus：以stable Program/DataSource/schema產生OwnerFingerprint，保留range-bearing QueryFingerprint作診斷，接SPAA application lifecycle、action handler與authoritative frames。
- MdcQuoteAgent：per-scale history/live provider capability；不讀寫browser cache。

## Completion gate

只有真FSSTL/MDCQ `.dib`、cache reload/range revisit Playwright、authoritative full/delta取代cache及package manifest全部通過，才可標production complete。

## Evidence

- 2026-09-09：`RuntimeCache` 已完成 identity/coverage validation、accepted frame write gate、bounded codec、current-session rehydrate 與 authoritative snapshot replacement。`dotnet run --project tests/PulseTrade.Comm.Spa.Dynamic.Contracts.Tests.fsproj -c Release -- --summary` 通過 21/21。
- 2026-09-09：browser read改用`RuntimeCacheEntryValidation`共用完整Document/Snapshot/reducer語意驗證；F# Playwright證明JSON合法但workspace/document不一致的record會回`Miss`、實體刪除且console/page error為0。
- 2026-09-09：Interactive.Client新增WebSharper-safe `writeAcceptedState`/`tryRehydrate`；Renderer在`PausedForResync`仍可local pan/zoom/hover/cursor且不送remote range/cursor action。Contracts 21/21、Renderer 26/26與兩組F# Playwright通過。
