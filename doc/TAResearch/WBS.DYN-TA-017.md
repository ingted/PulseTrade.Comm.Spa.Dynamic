# @DYN-TA-017 Notebook TA Workspace Production

- RFC: `doc/RFC/RFC-PTCS-DYNAMIC-0013.notebook-ta-workspace-production.md`
- Status: Active
- Progress: 94%

| Slice | Deliverable | Test | Progress | Status |
| --- | --- | --- | ---: | --- |
| DYN-TA-017A | RFC/REQ/SA/SD/WBS/Test/Verification文件鏈 | T-056 | 100% | Done |
| DYN-TA-017B | generic source identity/snapshot/event、validation、codec、reducer | T-057..060 | 100% | Done：Contracts suite與full WebSharper chain通過。 |
| DYN-TA-017C | generic editor schema、versioned correlated action wire/lifecycle、stable row identity | T-061 | 100% | Done：`ptcs-dynamic-action.v1` request/result、single pending、timeout/disconnect/correlation fail-closed；Contracts 16/16與Interactive.Client alpha10 bundle package gate通過。 |
| DYN-TA-017D | WebSharper production workspace UX與multi-scale presentation | T-062/T-065 | 100% | Done：authoritative editor catalog、stable RowId Add/Edit/reject、PTCS wire、exact package graph與desktop/mobile Playwright均通過。owner real metadata/DIB由E/F追蹤。 |
| DYN-TA-017E | ColdFar Notebook adapter / typed chart root | T-063 | 90% | Aster M15以Interactive.Extension exact alpha15及TradeCore.FsStl win139驗證`FsStlSeries<FloatingPoint>`、time-keyed sequence、Deedle Frame、NestedMap與typed `TA_CHART -> FsStlTaView.compile -> ForQuoteSlot -> Decode`；7 rows／12 requests／12 dataRefs／minimum scale 1皆GREEN。Dynamic Renderer/Interactive.Client `0.1.3`已發布並正確呈現authored row label。Daedalus M13擁有真QuoteSlot/session與final-cell auto-display，尚待該production DIB iframe gate。 |
| DYN-TA-017F | real MDCQ DIB + Playwright MCP + release | T-064 | 75% | fixed-range真MDCQ與alpha15 M15均GREEN：2 frames、7 rows、4 axes、axis points=`3820/763/127/63`；dev.69後連續兩輪alpha14再加一輪alpha15未重現0-row／timeout／role exit。既有Playwright已驗1K+SMA、5K DMI、30K MACD、60K Heikin-Ashi、shared hover、hide/show與console 0。尚待M13 final-cell的alpha15 iframe/Playwright，以及OPEN_END history→live ACK gate。 |
| DYN-TA-017G | SPAA/DIExt generic runtime hardening：retention/resync -> cursor/range -> reconnect/application lifecycle | T-066..068 | 100% | Done：三個phase均完成；Interactive.Client具single Start/Dispose、bounded reconnect、snapshot timeout/resync與stale generation fence。 |
| DYN-TA-017H | shared temporal axis、scalar series與PTCS v5 compact transport | T-069/070 | 98% | Generic 3,820×28真browser capacity、F# Playwright與Playwright MCP均通過；待Daedalus real MDCQ/DIB browser gate。 |

## Boundary

- Aster owns generic contract/reducer/renderer and acceptance harness。
- Daedalus owns `StructuredSeries` authority、FsStl/TradeCore semantics、workspace resource transition and typed chart root。
- MdcQuoteAgent owns source truth/cursor/capability/readiness evidence。
- TradeWeaver/SOR payload only enters through owner adapter; Dynamic does not grow a SOR-specific union。

## D slice evidence

- Contracts `0.1.0-alpha19`：`TemporalPoint`保存source interval、scale、observed/available frontier、preview/final與projection；document新增validated `BaseRowId`，cursor/range action使用UTC event-time與4000 hard cap；codec具完整WebSharper metadata與atomic candidate retention。
- Renderer `0.1.0-alpha45`：同列支援多candlestick trace；base row真實timestamp驅動shared cursor，coarse row只取finalized containing/as-of/missing。viewport release送半開event-time range，pending期間不重入且不因feedback重畫2000-point chart。
- PTCS adapter `0.1.0-alpha7-win55` / client `0.1.0-alpha8-win78`：`ta-browser.v4` wire保留BaseRowId及cursor/range fields，malformed range/catalog fail closed。
- Interactive.Client `0.1.0-alpha19` package帶可直接serve的single application bundle、minified bundle、Runtime與version-aligned manifest；application handle具idempotent Start/terminal Dispose，斷線保留last-good並只建立一條replacement channel，且只在authoritative Snapshot accepted後重設backoff。
- Isolated gates：Contracts `17/17`、Renderer `24/24`、PTCS `12/12`、PTCS.Client `14/14`、bundle package verifier Pass；F# Playwright及Playwright MCP皆通過desktop/mobile、range pending lock、cursor action與0 console error/warning。
- Daedalus consumer compatibility：Contracts alpha19、Renderer alpha45、Interactive.Client alpha19 的 nuspec 均 exact-pin `FSharp.Core [10.1.400]`；最小consumer只需在document指定BaseRowId並處理兩個typed action。`ApplyTemplate`仍由Daedalus Interactive.Extension controller執行authoritative prepare/swap/release。
- Retention/resync hardening：patch先依序套入不可見candidate data，再對各受影響series驗最終retained count；same-key replace與trim後append不再誤拒，跨operation合計超限會整批拒絕、保留last-good state並要求full resync。Contracts `16/16`與`DYN-VFY-018`三項邊界均通過。
- Lifecycle gate：exact-package pure lifecycle `4/4`；F# Playwright以真WebSocket force-drop驗last-good、single reconnect/full snapshot及terminal dispose。Playwright MCP另驗desktop/mobile UI、無overflow/console error。
- 尚未宣稱production：Daedalus SessionHost須消費Interactive.Client alpha19 action envelope；owner-normalized real DIB與MDCQ provider仍是E/F gate。
- Shared temporal axis：`TemporalAxis`保存一份sparse Position/time authority，`TemporalSeries`只保存Position/value；Position不可由scale推算。五條scalar O/H/L/C/V由`TaCandleDataRefs`合成K棒。PTCS `ta-browser.v5`直接搬運shared values；3820 x 28 deterministic frame、malformed/revision/unknown-position fail-closed及legacy相容已通過T-069/070的非UI部分。
- Generic browser capacity：BrowserDemo實際載入3,820-position shared axis與28條shared scalar series；Renderer alpha48以互不重疊的left-resize/move/right-resize hit regions維持窄selection可操作性。F# Playwright驗48-bar平移、左右resize、All、desktop/mobile與console 0；Playwright MCP另實際切換All/48並驗`Loaded 3820`、viewport與geometry。
- Current releases：Renderer `0.1.0-alpha48`、Interactive.Client `0.1.0-alpha22`、Ptcs.Client `0.1.0-alpha8-win81`，分別exact依賴Contracts alpha20、Renderer alpha48及PTCS beta117；NuGet push均回`Created`，public nuspec已回`200`並完成dependency readback。
- M15 typed package gate：Daedalus repo commit `70afe353`只承載automation acceptance，不複製production binding。Interactive.Extension exact `0.1.0-alpha15`載入後，`dotnet dib`驗證canonical FloatingPoint series、顯式time-key sequence、Deedle Frame、NestedMap、1440K date key與1380K fail-closed；typed view為7 rows／12 requests／12 dataRefs。真SPAA fixed-range run為2 frames與4條shared axes。
- Owner boundary：M13才是production notebook，沿用真provider/session並以`taView.ForQuoteSlot slotId`作最後expression；M15不建立第二套`createBinding`或手動`Display`。本session Browser MCP無可控browser，故alpha15 iframe gate保持未完成，不以`dotnet dib`替代。

## Completion gate

只有 DIB kernel gate、Playwright MCP human workflow、real provider identity/cursor與 package manifest 同時可追溯時，才可將本項標 production complete。
