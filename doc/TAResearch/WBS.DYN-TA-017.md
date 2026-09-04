# @DYN-TA-017 Notebook TA Workspace Production

- RFC: `doc/RFC/RFC-PTCS-DYNAMIC-0013.notebook-ta-workspace-production.md`
- Status: Active
- Progress: 67%

| Slice | Deliverable | Test | Progress | Status |
| --- | --- | --- | ---: | --- |
| DYN-TA-017A | RFC/REQ/SA/SD/WBS/Test/Verification文件鏈 | T-056 | 100% | Done |
| DYN-TA-017B | generic source identity/snapshot/event、validation、codec、reducer | T-057..060 | 100% | Done：Contracts suite與full WebSharper chain通過。 |
| DYN-TA-017C | generic editor schema、versioned correlated action wire/lifecycle、stable row identity | T-061 | 100% | Done：`ptcs-dynamic-action.v1` request/result、single pending、timeout/disconnect/correlation fail-closed；Contracts 15/15與Interactive.Client alpha8 bundle package gate通過。 |
| DYN-TA-017D | WebSharper production workspace UX與multi-scale presentation | T-062/T-065 | 100% | Done：authoritative editor catalog、stable RowId Add/Edit/reject、PTCS wire、exact package graph與desktop/mobile Playwright均通過。owner real metadata/DIB由E/F追蹤。 |
| DYN-TA-017E | ColdFar Notebook adapter / typed chart root | T-063 | 0% | Daedalus-owned integration |
| DYN-TA-017F | real MDCQ DIB + Playwright MCP + release | T-064 | 0% | Depends on D/E + provider readiness |

## Boundary

- Aster owns generic contract/reducer/renderer and acceptance harness。
- Daedalus owns `StructuredSeries` authority、FsStl/TradeCore semantics、workspace resource transition and typed chart root。
- MdcQuoteAgent owns source truth/cursor/capability/readiness evidence。
- TradeWeaver/SOR payload only enters through owner adapter; Dynamic does not grow a SOR-specific union。

## D slice evidence

- Contracts `0.1.0-alpha15`：`TemporalPoint`保存source interval、scale、observed/available frontier、preview/final與projection；`TaWorkspaceDocument.EditorSchemas`與Rows同revision，`ptcs.dynamic.editor.binding.v1`保存row template/value；codec具完整WebSharper metadata。
- Renderer `0.1.0-alpha37`：同列支援多candlestick trace；coarse candle以跨base slots的outline span呈現，line可repeat，causal indicator只在close後step。Add/Edit使用document catalog，navigator drag只更新dynamic attributes，release只commit一次。
- PTCS adapter `0.1.0-alpha7-win54` / client `0.1.0-alpha8-win73`：`ta-browser.v4` columnar wire保留temporal metadata、editor catalog與row options，malformed array/catalog fail closed。
- Interactive.Client `0.1.0-alpha8` package帶可直接serve的single application bundle、minified bundle、Runtime與version-aligned manifest。
- Isolated gates：Contracts `15/15`、Renderer `22/22`、PTCS `11/11`、PTCS.Client `13/13`、bundle package verifier Pass；Playwright連續兩次通過desktop/mobile、pending lock、Add/Edit/reject、same RowId/count、remove/reset、shared cursor與0 console error。
- Daedalus consumer compatibility：Contracts alpha15、Renderer alpha37、Interactive.Client alpha8 的 nuspec 均 exact-pin `FSharp.Core [10.1.302]`；不要求 Daedalus 全域升級toolchain。`ApplyTemplate`仍由Daedalus Interactive.Extension controller執行authoritative prepare/swap/release。
- 尚未宣稱production：Daedalus SessionHost須消費Interactive.Client alpha8 action envelope；owner-normalized real DIB與MDCQ provider仍是E/F gate。

## Completion gate

只有 DIB kernel gate、Playwright MCP human workflow、real provider identity/cursor與 package manifest 同時可追溯時，才可將本項標 production complete。
