# RFC-PTCS-DYNAMIC-0013 Notebook TA Workspace Production

- ID: RFC-PTCS-DYNAMIC-0013
- Status: Accepted / DEV authorized
- Date: 2026-09-04
- Owner: Aster / PulseTrade.Comm.Spa.Dynamic
- WBS: `doc/TAResearch/WBS.DYN-TA-017.md`
- Test: `doc/TAResearch/Test.md` DYN-TA-T-056..064

## 背景

既有 TA Canvas 已具備 bounded snapshot/patch、viewport、cursor、Add/Remove Row 與 PTCS transient adapter，但 ColdFar Notebook 的實際 extension 仍以固定 session/view/resource 為主。M12 使用 synthetic bars 與手工 `SDUIJson`，不能代表 production Notebook 工作流。

四方於 2026-09-04 已凍結 owner boundary：MDCQ 擁有 source truth/cursor，Daedalus 擁有 FsStl/TradeCore semantics、`StructuredSeries` contract 與 Notebook resource controller，Aster 擁有 generic SDUI contract/reducer/renderer。PTCS.Dynamic 不複製 `StructuredSeries` DTO、不聚合行情、不計算 TA。

## 目標

1. 提供跨 domain 的 source snapshot/event ordering envelope，以及 deterministic duplicate/gap/revision/epoch/resync semantics。
2. 提供 schema-driven editor、correlated action 與 stable row identity，使同類不同參數的 row 可 add/remove/reconfigure。
3. 維持單一 WebSharper renderer，消費 owner adapter 產生的 normalized document/frame，不直接參考 MDCQ、TradeCore、FsStl 或 FCell2。
4. 支援多尺度、history-to-live、availability/quality 的呈現，但 projection/bucket/calendar semantics 由 owner adapter提供。
5. 以 dedicated typed chart root、`dotnet dib` 與 Playwright MCP 完成 production-level Notebook 驗收。

## 非目標

- 不定義 FsStl `CHART` 語法或 `TraderChart` 最終型別名稱。
- 不擁有 `StructuredSeriesObservation/Batch/IStructuredSeriesSink`。
- 不從 1K 計算 5K/30K/60K/930K/1380K，也不修正 TA 演算法。
- 不把 FCell2、SQL、provider SDK 或 SOR domain union 加入 Dynamic contracts。
- 不用 synthetic、loopback 或 DOM marker 取代 real provider/browser acceptance。

## 決策

### 1. Source envelope 是 ordering seam，不是 domain model

```fsharp
type SourceStreamIdentity =
    { SourceId: string
      SchemaKey: string
      Epoch: string }

type SourceSnapshotEnvelope =
    { Stream: SourceStreamIdentity
      SourceRevision: int64
      LastSequence: int64
      CapturedAtUtc: DateTimeOffset
      Payload: SduiValue }

type SourceEventEnvelope =
    { Stream: SourceStreamIdentity
      BaseSourceRevision: int64
      NewSourceRevision: int64
      Sequence: int64
      EventTimeUtc: DateTimeOffset
      Payload: SduiValue }
```

source state 套用 event 時由 domain owner 提供 pure payload reducer。Dynamic 只驗 identity、sequence、base/new revision 與 bounded safe payload。domain reducer 失敗、epoch/schema改變、sequence gap 或 base revision mismatch 均保留 last-good state並產生 typed snapshot request。

source revision/sequence 是 evidence，不能直接當 `DocumentRevision`。只有 owner workspace transition 成功後，才由 Interactive.Extension 建立新 document revision。

### 2. Daedalus contract 單一權威

Daedalus repo 的 `StructuredSeries` public type 是 provider ingest contract。MdcQuote-owned adapter把 source batch交給 Daedalus sink；Daedalus workspace adapter再產生 Dynamic source envelope/runtime frame。Aster package不得 reference或重宣告該 DTO。

### 3. UI mutation 由 backend authority 決定

renderer送 correlated request，UI只呈現 pending。backend prepare new resources、驗 revision/capability/schema後原子 swap，再回 accepted revision；reject只顯示該 action錯誤，不移除既有 Canvas。resource cleanup由 Daedalus workspace controller exactly-once處理。

### 4. Temporal projection 邊界

- renderer只畫 owner 提供的 actual source interval/frontier/availability/quality。
- availability缺失不得以 receive/render time補造；causal profile須拒絕或標 unavailable。
- coarse candle跨 base slots呈現；歷史 coarse line依 owner projection對齊 base slots；live partial/final依 capability更新。
- `930K`是兩段 half-day product key，不是固定 930 分鐘；`1380K`依 CME calendar。Dynamic 不自行建立 boundary。

### 5. Browser implementation

browser與server可共用 `RuntimeFrame` type，但 System.Text.Json codec與 WebSharper typed JSON codec是兩條 wire，bytes不可混用。前端只使用 F#/WebSharper；禁止手寫 JavaScript、inline script與 string-built callback。

## 開發順序

1. DYN-TA-017A：RFC/REQ/SA/SD/WBS/Test/Verification。
2. DYN-TA-017B：source envelope、validation、codec、pure reducer與 tests。
3. DYN-TA-017C：generic editor schema、correlated action result、stable row identity。
4. DYN-TA-017D：renderer pending/reject、multi-instance row、多尺度 presentation。
5. DYN-TA-017E：Daedalus-owned Interactive.Extension adapter與 typed chart root integration。
6. DYN-TA-017F：real MDCQ + `dotnet dib` + Playwright MCP production acceptance、package/release。

## 驗收

1. Snapshot/event codec roundtrip，invalid identity/revision/time/payload fail closed。
2. Duplicate no-op；gap、epoch/schema change、revision mismatch與 reducer reject保留 last-good並要求 snapshot。
3. 同 template 不同參數 row 可新增、移除、修改；reject不清 Canvas；revision conflict resync。
4. 1K+5K與5K+30K alignment、missing/partial/final/availability/quality以 browser geometry/legend/cursor驗證。
5. Notebook cell最後只需 dedicated chart root，不含手工 JSON 或 `KernelInvocationContext.Display` plumbing。
6. real-path DIB與Playwright皆通過；M12只作 regression。

## 相容性與 rollback

既有 `sdui-runtime.v1`、static Canvas/FormInput與 PTCS adapters維持相容。新 source envelope是 additive contract；未裝新 adapter時不啟用。rollback只移除新 adapter/feature capability，不修改既有 durable document/history。

## 關聯文件

- `M:\202608\AGENTS_Feedback\Aster_RFC001_PTCS-DynamicNotebookTAWorkspace.md`
- `G:\coldfar_py\coldfar-symbolics\SymbolicNet6.TradeCore.FsStl\StructuredSeries.fs`
- `doc/TAResearch/REQ.md`
- `doc/TAResearch/SA.md`
- `doc/TAResearch/SD.md`
- `doc/TAResearch/WBS.DYN-TA-017.md`
- `doc/TAResearch/Test.md`

