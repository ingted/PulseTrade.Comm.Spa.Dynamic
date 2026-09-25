# Snapshot Chunk Decoder / Assembler Hypothesis

## 現象

Daedalus 的正式 SMA/DMI machine E2E 取得 `ptcs-dynamic-snapshot-chunk.v1` 的 `start / item / commit` packets，卻無法透過 Contracts owner API 還原 `RuntimeFrame`。Contracts 目前只有 encoder；可用的 assembler 位於 Interactive.Client，依賴 browser JSON、`requestAnimationFrame` 與 runtime reducer，純 .NET consumer 若要驗證只能重造 wire protocol。

## Repo 狀態

- Root: `C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic`
- Branch: `20260915_033.ptcs_group_support`
- Start commit: `e9faf01`
- Start dirty state: clean

## 核心假設

1. `RuntimeSnapshotTransport.fs` 已擁有 authoritative wire records 與 encoder，因此 decoder、ordered assembler 與 batch metadata 應下沉至同一 Contracts module，而非讓 machine consumer 依賴 Interactive.Client。
2. `Client.fs` 與 `FramePump.fs` 現有 batch 驗證重複，且 browser-only packet parsing 造成語意分叉。兩者可保留 RAF 分段 decode/reduce，但 framing 狀態轉移應委派給同一個純 state machine。
3. Wire schema、packet JSON、canonical `RuntimeFrame` validation 與 publication lifecycle不需改變；新增 API 只補齊 owner package 的 decode/reassemble 能力。

## 預計 API / 資料流

```text
create(generation)
  -> accept(generation, encodedPacket) -> AwaitingMore | Completed RuntimeFrame
  -> finish() -> RuntimeFrame or structured runtime-snapshot-chunk-* error
```

State 保存 generation、batch id、expected/next index、seen refs、empty-snapshot header 與 staged values。每個 transition 均為純函式；decode 使用既有 WebSharper typed JSON / `BrowserRuntimeCodec`，可同時由 .NET 與 WebSharper JS 呼叫。Interactive.Client 只負責排程 staged value decode、canonical reducer 與 publish。

## 驗收與反證

- encoder round-trip 可由純 .NET Contracts API還原相同 `RuntimeFrame`。
- partial、duplicate、out-of-order、trailing packet、mismatched generation 均回既有 `runtime-snapshot-chunk-*` structured code。
- zero-item snapshot 可 start/commit 完成。
- browser WebSocket 與 frame-pump tests 改走同一 assembler，仍保持 supersede、last-good 與 reducer error semantics。
- 若 typed WebSharper JSON 在 .NET/JS 任一 target 無法穩定判別 envelope，假設失敗；改採 Contracts 既有 cross-runtime codec 能力，但不手刻 JSON parser。

## 估算

非型別宣告實作約 120-180 行；若超過 360 行，須先記錄複雜度來源並重新縮小 seam。

## Experiment

- 實作量超過原估算：新增約318行Contracts assembler與272行tests，原因是完整ordered stream不只單batch round-trip，還必須保留legacy混流、global packet index、錯誤優先序與incremental API；範圍仍集中於單一owner module，未新增service/framework。
- `RuntimeSnapshotTransportAssembler`加入`decodePacket`、`create/createAt`、`acceptPacket/acceptEncoded`、`finish/reassemble/decodeFrames`；browser `Client.fs`與`FramePump.fs`改用同一framing transition。
- 第一版在commit的`acceptPacket`同步執行完整canonical validation，BrowserDemo量得commit RAF 208ms，與後續phased reducer重複。修正為accept只產candidate；machine `finish/decodeFrames`完整驗證，browser交phased reducer驗證。
- Active batch若chunk decode失敗，只有legacy frame同時decode＋validate成功才回interleaved；legacy也失敗時保留原schema/kind/decode error。

## Conclusion

- 三個核心假設成立；typed JSON可跨.NET/WebSharper使用，無需手刻parser。
- Contracts 41/41；Renderer／Interactive／Dynamic.Ptcs／Ptcs.Client為46/12/14/16。
- 修正重複validation後F# Playwright five-candle 67.28ms、scenario 79.81ms、All 42.76ms，所有acceptance phase over100=0。
- Local exact graph為Contracts 0.1.26、Renderer 0.1.62、Interactive.Client 0.1.54、Dynamic.Ptcs 0.1.49、Ptcs.Client 0.1.76；未public push，待Daedalus fresh-cache machine consumer與真SPAA gate。
