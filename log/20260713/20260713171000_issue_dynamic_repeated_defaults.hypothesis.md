# Dynamic repeated Argu defaults hypothesis

## 現象

正式 PTCS Host `http://10.28.112.109:82/page/ta-research` 載入四列 TA canonical Argu 後，四個 `Add_Row`、六個 `Trace_Sma` 與重複 MACD cases 都顯示各群組最後一筆 default，組出的 raw Argu 因而失真。

## Repo 狀態

| Repo | Branch | Baseline | 狀態 |
| --- | --- | --- | --- |
| PulseTrade.Comm.Spa.Dynamic | `20260711_027.win.TACanvas` | `3814188` | reply presentation milestone 尚未 commit；本問題修正前先保留該 baseline diff |

## 假設

| ID | 假設 | 影響位置 | 證據 / 反證 | 機率 | 驗證 |
| --- | --- | --- | --- | ---: | --- |
| H1 | Server `defaultsFromParsedTargetForSchema` 以 `Map.ofArray` 壓平同 binding 的多個 occurrence | `Server/ArguForm.fs` | repeated `Add_Row.*` 最後值覆蓋前值；程式明確以 binding 作唯一 key | 0.45 | 保留 ordered entries，逐 document node occurrence 套用後檢查 document defaults |
| H2 | Client `defaultsFromDocument` 以 `Map.ofSeq` 再次壓平 repeated nodes | `Client/ArguFormRenderer.fs` | 即使 server document 有 ordered nodes，client 仍只保留同 binding 最後值 | 0.40 | 將 map value 改為 ordered occurrence arrays，render 時逐次消費 |
| H3 | Argu parser 本身把 repeated root cases 解析成最後一筆 | Dynamic server parser | 既有 parsed target 會保留 repeated `RootCases`，且 UI 確實產生正確數量 rows，反證此假設 | 0.08 | server focused test 比對 parsed case 順序 |
| H4 | 瀏覽器舊 IndexedDB/DOM hydration 導致重複 | PTCS client cache | 完整 reload 與新 WebSocket session 後仍可重現；因此不是單純 stale DOM | 0.07 | 修正 H1/H2 後以正式 82 reload 驗證 |

## 預估修改

非型別宣告約 45 行：抽出 ordered default entries、加入 occurrence-aware document apply 與 client reader，另補 server/client focused tests。若超過約 90 行，需新增 experiment 記錄原因。

## 驗收

1. 四個 `Add_Row` 依序為 price/dmi/macd-long/macd-short。
2. SMA periods 依序為 13/21/34/89/144/233。
3. full raw preview 等於 Host canonical request。
4. Dynamic build/tests 與 82 Playwright reply flow 通過。
