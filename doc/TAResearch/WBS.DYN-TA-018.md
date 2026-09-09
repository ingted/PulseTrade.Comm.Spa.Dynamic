# @DYN-TA-018 Browser Range Cache Resume

- RFC: `doc/RFC/RFC-PTCS-DYNAMIC-0014.browser-range-cache-resume.md`
- Status: Done
- Progress: 100%

| Slice | Deliverable | Test | Progress | Status |
| --- | --- | --- | ---: | --- |
| DYN-TA-018A | RFC/REQ/SA/SD/WBS/Test/Verification文件鏈 | T-071 | 100% | Done |
| DYN-TA-018B | cache identity/coverage/entry validation與pure rehydrate/write gate | T-072/073 | 100% | Done；Contracts tests 21/21，共用browser/server semantic validator |
| DYN-TA-018C | WebSharper F# IndexedDB store、bounded LRU與no-cache fallback | T-074 | 100% | Done；Playwright persistence/LRU/coverage/corrupt/semantic-invalid/clear gate pass |
| DYN-TA-018D | Interactive.Client cache hydrate、resume/full handshake與range revisit | T-073/075 | 100% | Done；OPEN_END cache只保存各temporal axis finalized prefix及同步裁切的series，rehydrate只供display-first並保留current revision、進`PausedForResync`；owner一律重新連authoritative full/provider，cache revision不作continuation authority |
| DYN-TA-018E | FSSTL `.dib` + SPAA Playwright reload/range/cache E2E | T-076 | 100% | Done；M16真provider驗3,820 bars／7 rows、projected candles、30K horizontal-step、7列shared cursor x1一致且125ms/no-rerender；M17驗4,000 bars、current 1K preview、IndexedDB finalized-prefix cache hit、authoritative provider reattach與live revision 2->3。 |

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
- 2026-09-09：M15以temp feed correction通過canonical FloatingPoint、time-key sequence、NestedMap、Deedle Frame與typed多尺度TA view；M13 fixed-history真MDCQ取得3,820 committed bars、4 scales、76 TA series與28 visible refs。Playwright MCP驗desktop/mobile、48/200 viewport、row toggle與1K/5K shared hover。
- 2026-09-09：SPAA cache reload顯示`CACHE READY`，network只有一次`/api/workspace/inspect`且沒有`/runs`；double-run race最後為READY，兩次inspect只建立一次run，console 0。OPEN_END cache仍未因fixed-history成功而宣稱完成。
- 2026-09-09：Daedalus接受OPEN_END generic authority：cache保存finalized prefix但不保存preview作續接權威；rehydrate保留current `DataRevision`並進`PausedForResync`，owner立即重連authoritative full/provider。Contracts 0.1.7以T-072/073驗每個axis與其temporal series同步裁切、preview-only fail closed及cache revision不作continuation。
- 2026-09-09：shared cursor從main chart reconciliation state分離，chart建立時保存已解析且bounded的cursor readers；mousemove不再重解28條完整series。generic 3,820-position/28-series/7-row F# Playwright直接量第一條SVG crosshair `x1`為62ms，`data-chart-render-sequence`不變且console 0；Daedalus M16真provider以相同gate量得125ms且7列`x1`一致。
- 2026-09-09：Daedalus M17驗OPEN_END loaded=4,000、current 1K preview、IndexedDB finalized-prefix cache hit、authoritative provider reattach及live revision 2->3。最終public exact graph為Contracts 0.1.7、Renderer 0.1.14、Interactive.Client 0.1.11、Dynamic.Ptcs 0.1.7、Ptcs.Client 0.1.8；public nuspec dependency readback與local SHA均完成。
