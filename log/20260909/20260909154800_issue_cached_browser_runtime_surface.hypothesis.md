# Cached browser runtime surface hypothesis

## 現象

SPAA 從 browser WebSharper code 呼叫 `RuntimeCache.tryCreateEntry` / `RuntimeCache.tryRehydrate` 會出現 WS9001，且 cached `PausedForResync` 畫面無法 local pan/zoom，click cursor 仍可能嘗試送 remote command。

## Repo baseline / dirty

- Repo: `C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic`
- Branch: `20260715_030.win.TACanvas_cross.bar.cursor.first.done`
- Baseline: `d1890d3 feat: validate browser runtime cache entries`
- 建立本檔時 code worktree clean；log 檔為本切片新增。

## 核心假設

1. `RuntimeCache` 與 server-only `RuntimeCacheCodec` 位於同一個未標記 JavaScript 的 module，導致其中本來是 pure/reducer-based 的 create/rehydrate 也無 WebSharper surface。抽成 `[<WebSharper.JavaScript>] RuntimeCacheProjection` 並讓 server module 委派，可保持單一 canonical contract。
2. Renderer 的 `viewportCommandsDisabledNow` 同時控制 local state mutation 與 remote dispatch；`PausedForResync` 因而過度封鎖。local update應先完成，再以 `remoteDisabled` 決定是否送 action。

## 影響位置

- `src/PulseTrade.Comm.Spa.Dynamic.Contracts/TemporalSeries.fs`
- `src/PulseTrade.Comm.Spa.Dynamic.Contracts/RuntimeCache.fs`
- `src/PulseTrade.Comm.Spa.Dynamic.Interactive.Client/BrowserCache.fs`
- `src/PulseTrade.Comm.Spa.Dynamic.Renderer/Renderer.fs`
- 對應 tests/browser demos/verifiers/docs。

## 預估實作

非 type 宣告約 70-110 行：cache projection抽取/委派約35-50行差異，browser adapter約15行，Renderer policy約10-20行，測試 fixture 約20行。若超過約220行，需在本檔追加原因再擴張。

## 驗證方式

- Contracts focused tests：server facade與JS projection結果一致、invalid entry fail closed。
- Interactive browser E2E：accepted state write/read/rehydrate成功；invalid state拒絕且不寫入。
- Renderer browser E2E：Paused local pan/zoom與hover保留，remote visible-range/shared-cursor為0；Ready各為1。
- package verifier：bundle metadata/manifest/exact dependency與公開 API marker。

## Experiment

實作差異約352行，超過原預估上限。主因不是產品抽象擴張，而是browser端不能直接使用server `TemporalAxisCodec`；必須新增bounded、declared-axis-only的WebSharper coverage decoder，並擴充兩個browser demo fixture與Playwright assertions。server facade保留原完整decoder，browser/server只共用entry construction與rehydrate reducer，避免為了可編譯而降低server驗證。

結果：Contracts與Renderer full WebSharper Rebuild通過；Contracts 21/21、Renderer 26/26；Interactive cache Playwright驗accepted write、invalid reject、current-session rehydrate，Renderer Playwright驗paused local interaction與remote suppression。核心假設成立。
