# @DYN-TA-017 Notebook TA Workspace Production

- RFC: `doc/RFC/RFC-PTCS-DYNAMIC-0013.notebook-ta-workspace-production.md`
- Status: Active
- Progress: 64%

| Slice | Deliverable | Test | Progress | Status |
| --- | --- | --- | ---: | --- |
| DYN-TA-017A | RFC/REQ/SA/SD/WBS/Test/Verification文件鏈 | T-056 | 100% | Done |
| DYN-TA-017B | generic source identity/snapshot/event、validation、codec、reducer | T-057..060 | 100% | Done：Contracts suite與full WebSharper chain通過。 |
| DYN-TA-017C | generic editor schema、versioned correlated action wire/lifecycle、stable row identity | T-061 | 100% | Done：`ptcs-dynamic-action.v1` request/result、single pending、timeout/disconnect/correlation fail-closed；Contracts 15/15與Interactive.Client alpha4 bundle package gate通過。 |
| DYN-TA-017D | WebSharper production workspace UX與multi-scale presentation | T-062 | 85% | Generic isolated path完成：temporal-point、1K/5K candle span、repeat、30K causal step、multi-candle、generic editor、pending/reject、desktop/mobile Playwright；owner real metadata/DIB仍待E/F。 |
| DYN-TA-017E | ColdFar Notebook adapter / typed chart root | T-063 | 0% | Daedalus-owned integration |
| DYN-TA-017F | real MDCQ DIB + Playwright MCP + release | T-064 | 0% | Depends on D/E + provider readiness |

## Boundary

- Aster owns generic contract/reducer/renderer and acceptance harness。
- Daedalus owns `StructuredSeries` authority、FsStl/TradeCore semantics、workspace resource transition and typed chart root。
- MdcQuoteAgent owns source truth/cursor/capability/readiness evidence。
- TradeWeaver/SOR payload only enters through owner adapter; Dynamic does not grow a SOR-specific union。

## D slice evidence

- Contracts `0.1.0-alpha12`：`TemporalPoint`保存source interval、scale、observed/available frontier、preview/final與projection；`ptcs-dynamic-action.v1`把action result與authoritative runtime frame分流。Interactive.Client `0.1.0-alpha4` package帶可直接serve的single application bundle、minified bundle、Runtime與manifest。
- Renderer `0.1.0-alpha33`：同列支援多candlestick trace；coarse candle以跨base slots的outline span呈現，line可repeat，causal indicator只在close後step。
- PTCS adapter `0.1.0-alpha7-win50` / client `0.1.0-alpha8-win68`：`ta-browser.v4` columnar wire保留temporal metadata，malformed array fail closed。
- Isolated gates：Contracts `15/15`、Renderer `22/22`、PTCS `10/10`、PTCS.Client `12/12`、bundle package verifier Pass；Playwright MCP驗desktop/mobile、pending lock、accepted mutation與0 console error。
- Daedalus consumer compatibility：Contracts alpha12、Renderer alpha33、Interactive.Client alpha4 的 nuspec 均 exact-pin `FSharp.Core [10.1.302]`；不要求 Daedalus 全域升級 toolchain。`ApplyTemplate` 仍由 Daedalus Interactive.Extension controller 執行 authoritative prepare/swap/release。
- 尚未宣稱production：Daedalus SessionHost須消費Interactive.Client alpha4 action envelope；owner-normalized real DIB與MDCQ provider仍是E/F gate。

## Completion gate

只有 DIB kernel gate、Playwright MCP human workflow、real provider identity/cursor與 package manifest 同時可追溯時，才可將本項標 production complete。
