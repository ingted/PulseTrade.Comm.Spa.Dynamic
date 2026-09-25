# RFC-PTCS-DYNAMIC-0026 Runtime Projection Commit

- ID：RFC-PTCS-DYNAMIC-0026
- 狀態：Accepted / DEV Authorized
- Owner：PTCS Dynamic Contracts／Renderer／Interactive.Client
- 關聯：RFC-PTCS-DYNAMIC-0024、RFC-PTCS-DYNAMIC-0025、DYN-WBS-563
- Consumer correlation：`rfc-tradecore-0028-runtime-projection-commit-20260925`

## 背景

Daedalus 的 DIB companion 會以單一 authoritative action 切換 scenario overlay、summary、trades、timeline 與 download identity。Host action ACK 只表示 action handler 已接受或完成；它不證明 browser reducer 已接受 frame、Renderer 已完成 current-generation row projection，也不證明使用者已可觀察到新畫面。

現有 `data-chart-document-revision`／`data-chart-data-revision` 位於會被重建的 chart stack。same-topology incremental refresh只更新既有 row Vars，不保證重建該節點或更新 attributes；因此兩欄只可作 diagnostic，不能作 commit contract。

## 目標

1. 提供不含 Backtest／TradeCore domain 的 typed browser projection commit receipt。
2. 只有 current generation 的全部 visible rows 完成 projection，並再跨過一個 animation-frame boundary 後才發布。
3. 在 stable application root 同時提供 edge event 與 level watermark，讓 early／late subscriber 使用同一份 receipt。
4. rapid replacement、reject、resync、disconnect、dispose與stale row work不得誤發布。
5. Consumer可在送 action 前訂閱，並以相同 canvas identity及 `DataRevision >= expected`完成等待。

## 非目標

- 不把 action ACK、transport ACK、cache write或provider transaction改名成projection commit。
- 不在 contract 內加入 scenario、marker、summary、trade或download欄位。
- 不以 DOM mutation polling、chart diagnostic attrs或固定 timeout判定完成。
- 不要求每次同revision UI-local操作發布receipt。
- 不修改或重啟 Daedalus SPAA `127.0.0.1:18883`。

## 情境

### Same-topology data replacement

Reducer接受新的DataRevision，但chart shell不重建。Renderer逐列更新既有row projection；最後一列完成後再排一個frame，才發布一次receipt。Consumer不能在第一列或排程完成前解除等待。

### Document／topology replacement

Renderer先準備candidate，再逐列mount。只有current render generation全部完成且仍對應同一RuntimeState時發布；被較新candidate取代的generation不得發布。

### Late subscriber

Consumer在edge event後才掛上listener時，可從stable application root attributes或typed handle讀最新level watermark；不必重送action，也不必監看chart stack。

### Reject／disconnect／dispose

invalid frame、reducer reject、resync request、stale socket generation、中斷中的row work與Dispose後 callback皆不得提高projection sequence或覆蓋watermark。

## 決策

### Typed receipt

Contracts新增WebSharper-safe `RuntimeProjectionCommitReceiptV1`：

```fsharp
type RuntimeProjectionCommitReceiptV1 = {
    Identity: RuntimeIdentity
    DocumentRevision: int64
    DataRevision: int64
    LastTransportSequence: int64
    ProjectionSequence: int64
}
```

`ProjectionSequence`由單一browser application自1開始單調增加；canvas identity變更後仍可增加，但consumer必須先比Identity。revision與transport sequence來自已接受的RuntimeState，不由Renderer推算。

### Renderer completion

Renderer提供新的commit callback入口；既有`render`保留並以`ignore`委派，避免不需要commit contract的PTCS client被迫改API。

```text
accepted RuntimeState candidate
  -> generation-safe prepare
  -> current visible rows mount/refresh complete
  -> requestAnimationFrame
  -> candidate仍是current且未dispose
  -> publish receipt once
```

相同identity／document／data／transport tuple去重。UI-local cursor、resize、draft或viewport render不得提高receipt。若row topology為空，shell完成後仍須經相同paint boundary。

### Stable root publication

Interactive.Client是唯一publication authority：

- level：stable `options.RootElementId` element上的versioned `data-ptcs-runtime-commit-*` attributes；
- edge：bubbling、non-cancelable `CustomEvent`，名稱固定為`ptcs-dynamic-runtime-committed-v1`，`Detail`為同一typed receipt；
- typed handle：`GetLastProjectionCommit`與`SubscribeProjectionCommitted`。

新的application start會先清除同root的舊watermark；Dispose保留最後一筆可診斷值，但不再發event。DOM event以WebSharper DOM API建立，禁止inline JavaScript。

### Consumer等待規則

Consumer應先subscribe，再送authoritative action；收到event或讀level後，只有identity相同且committed revision達expected才完成。超時只表示browser projection未被證明，不得把action ACK當成功替代。

## 取捨

- 額外一個frame會增加最多一個display cadence的確認延遲，但換取paint-observable語意。
- Stable root attrs重複少量typed欄位，但解決event先於subscriber的競態；它不是第二套authority。
- 保留舊`render`入口降低下游升版範圍；Interactive.Client採新入口並擁有公開subscription API。

## 影響

- Contracts：receipt型別、驗證、satisfaction helper與DOM contract constants。
- Renderer：current-generation rows-complete＋paint callback；不新增domain邏輯。
- Interactive.Client：stable root watermark/event與typed handle observer。
- Dynamic.Ptcs／Ptcs.Client：若public dependency graph因exact references需要升版，只作相容回歸，不新增authority。
- DIExt／SPAA：只消費receipt；不得自行觀察chart attrs或重建renderer completion邏輯。

## 驗收

1. Contracts正常、identity mismatch、revision不足、invalid sequence均有pure tests。
2. Same-topology refresh及full topology mount各只發布一筆，且發生於全部row完成後的下一個frame。
3. Rapid A->B只可發布B；stale generation、reject、resync、disconnect、Dispose不發布。
4. Early subscriber收到event；late subscriber由root／handle取得同一receipt；identity change不誤用舊watermark。
5. 4,000-slot replacement、cursor及現有owner performance gates不回歸，console/page error為零。
6. Final exact local graph交Daedalus fresh-kernel E2E；consumer通過前不public push。

## 關聯追溯

- REQ：`doc/TAResearch/REQ.md` DYN-TA-REQ-075..079
- SA：`doc/TAResearch/SA.md`「Runtime projection commit analysis」
- SD：`doc/TAResearch/SD.md`「Runtime projection commit revision 18」
- WBS：DYN-WBS-563
- Test：DYN-T-600..606、DYN-TA-T-104..109
