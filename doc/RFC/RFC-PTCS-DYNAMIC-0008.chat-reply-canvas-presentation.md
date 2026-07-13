# RFC-PTCS-DYNAMIC-0008 Chat Reply Canvas Presentation Adapter

- ID：RFC-PTCS-DYNAMIC-0008
- Status：Accepted / DEV authorized
- Date：2026-07-13
- Related：`G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\doc\RFC\RFC-PTC-SPA-0020.chat-reply-canvas-presentation.md`、`RFC-PTCS-DYNAMIC-0007.realtime-ta-canvas-runtime.md`
- REQ：`doc/TAResearch/REQ.ChatReplyCanvasPresentation.md`
- Gap evidence：`G:\PulseTrade2.fs\misc\2026-07-13_ta 圖不見，只剩下 json，底下的 FormInput高度只能有現在的 三分之一，不然都不用看圖了.png`

## 背景

Dynamic目前有兩條不一致的presentation路徑：static `fskynet-sdui` renderer先顯示summary card，再直接切到fixed fullscreen overlay；TA runtime則由專用page/client mount立即顯示完整Canvas。後者使target key等同TA page，破壞PTCS原本逐reply的chat timeline。

Dynamic應只判斷「這一則reply如何呈現」，不應決定PTCS page shell。TA Canvas仍需要authenticated transient channel，但channel lifecycle應由該reply的inline/fullscreen mount驅動。

## 目標

1. 對plain、static SDUI、runtime TA SDUI做strict classification；不匹配時回`None`讓PTCS顯示文字。
2. runtime TA reply提供domain-aware摘要，不在summary階段建立chart/runtime channel。
3. 同一presentation同時支援lazy inline與既有near-fullscreen render。
4. per-reply/canvas instance獨立；多個inline TA Canvas可共存且各自bounded poll。
5. inline/fullscreen共享同一runtime state，避免雙channel、雙poll或view reset。
6. 以純WebSharper F#實作；v2 path不得使用`JS.Inline`或手寫JavaScript，既有legacy fullscreen path在migration slice改由typed WebSharper API實作。
7. 正確解開formal Host回覆的nested `fCell2/RuntimeFrame/SduiValue` envelope；claim成功時只交付summary/presentation，不把原始series JSON呈現給使用者。
8. Dynamic append input renderer不得強迫PTCS進Form mode；Plain/Form composer由PTCS owns。

## 非目標

- 不擁有PTCS chat message identity、session scrollbar或page navigation。
- 不解析target key來決定presentation；只看reply payload與PTCS提供的context。
- 不把summary、expand state或fullscreen state寫入server journal。
- 不修改TA calculation、PTMD query或RN durable route。
- 不提供DU type/canonical Argu profile editor；既有FormInput renderer只在Host明確選擇Form mode時mount。

## 情境

1. Dynamic收到plain string時不攔截；同一session下一則static SDUI顯示Canvas summary，第三則runtime TA顯示TA summary。
2. TA summary保持零active channel；使用者展開時才open，收合後立即dispose。
3. 兩則TA reply同時inline，各自poll與保存view state；其中一則fullscreen不影響另一則。
4. malformed declared runtime payload只在該message card顯示錯誤，其他reply仍可操作。

## 決策

- 新增Dynamic message presentation v2 adapter，不以target key或page-level renderer推斷TA。
- runtime TA採parse-once summary model與lazy Canvas mount；inline/fullscreen共享logical canvas identity。
- static Canvas保留legacy fullscreen，並可經v2 adapter獲得inline mode。
- v2及被遷移的legacy fullscreen path使用pure WebSharper F#，不延續現有inline JavaScript做法。

## Strict classification

```text
raw reply payload
  -> unwrap bounded reply/fCell2/RuntimeFrame envelope
  -> valid runtime TA envelope/document?  -> RuntimeTa
  -> valid static fskynet-sdui?           -> StaticSdui
  -> otherwise                            -> None
```

分類順序與schema/version必須strict；不得以target alias、page title、actor address、字串包含`TA Research`或key tuple猜測。Malformed已宣告runtime schema的payload回controlled Dynamic error presentation，不得`None`成為看似正常plain success。

正式Host payload可能是JSON-string包JSON、`ptc.comm.fcell2.value.v1` rows、`fCell2.A`多frame或F# DU `Case/Fields` JSON。adapter必須以bounded、typed decoder逐層解開，取得`replied msg:`或合法RuntimeFrame；不得把整段series DOM文字當作fallback成功。

## TA summary model

Dynamic從已驗證的document/snapshot metadata產生transport-neutral summary：

```fsharp
type TaReplySummary =
    { Instrument: string
      Interval: string
      RequestedRange: string option
      Coverage: string option
      Rows: TaRowSummary array
      Watermark: string option
      Freshness: string
      Quality: string }

type TaRowSummary =
    { RowId: string
      Label: string
      Traces: string array }
```

trace label需包含主要parameters。四列範例摘要應能辨識K/SMA 13/21/34/89/144/233、DMI ±DI7/ADX7/21、兩組MACD periods，而不是只顯示「4 rows」。

## Presentation lifecycle

```text
CreatePresentation(replyContext)
  -> parse/classify once
  -> RenderSummary()                         // no channel
  -> MountInline(host)
       create/reuse RuntimeRegistry instance
       open PTCS transient channel
       render bounded chart
  -> MountFullscreen(host)
       reuse same logical instance/state
  -> Dispose(mode/all)
       cancel poll/in-flight/subscription
```

- Collapsed時保留validated summary model，不保留active chart DOM。
- Inline -> Fullscreen不得另建第二個logical canvas；可移動mount或以same identity remount/resync。
- Fullscreen -> Inline需保留zoom/toggle/query draft等local view state。
- 兩則reply即使document內容相同，只要reply identity不同就各自擁有presentation state。

## Static Canvas相容

既有static SDUI summary/fullscreen不移除。v2 adapter應讓static Canvas也可選擇inline呈現，但舊Host只支援legacy renderer時仍保持原summary + fullscreen行為。static payload不open TA transient channel。

## Layout contract

1. Dynamic inline root不設定fixed viewport、不修改PTCS ancestor overflow。
2. inline TA chart完整高度交由外層chat timeline scroll；只在必要時提供horizontal viewport。
3. fullscreen renderer使用PTCS提供的fullscreen host/close callback，不自行建立不可追蹤的global overlay。
4. summary、inline、fullscreen controls需提供stable test id、keyboard focus與accessible label。
5. 每個TA row必須顯示自己的Y axis scale、ticks與unit；price、DMI/ADX、MACD不得假設共用Y domain，也不得只畫series而沒有可判讀尺度。
6. 所有rows共享同一time viewport/domain，X axis只在整個chart stack最底部呈現一次；zoom/pan/range change必須同步更新所有rows與shared X ticks。
7. pointer hit-test必須產生一條跨越所有rows的shared vertical cursor，並以同一timestamp/bar identity顯示每個row的series values。cursor不可在各row各自選到不同bar。

## 方案取捨

| 方案 | 決策 | 原因 |
| --- | --- | --- |
| TA專用page renderer | Reject as default chat path | 把一則reply升級成整頁，target無法回多型內容。 |
| renderer收到target key後推斷TA | Reject | route與presentation耦合，無法處理同target不同reply。 |
| context-aware message presentation adapter | Accept | 保留PTCS timeline，Dynamic仍owns schema與renderer。 |
| collapsed仍維持poll | Reject | 看不到的Canvas浪費channel/CPU/DOM。 |

## 影響範圍

- `PulseTrade.Comm.Spa.Dynamic` client bundle：message presentation v2 registration、strict classifier、summary、inline/fullscreen adapter。
- `PulseTrade.Comm.Spa.Dynamic.Renderer`：mount target可替換、view state export/import或equivalent remount seam。
- `PulseTrade.Comm.Spa.Dynamic.Ptcs.Client`：channel lifecycle由presentation mount狀態驅動，不由page load固定啟動。
- `RFC-PTCS-DYNAMIC-0007` runtime規則：`expanded`明確指Inline或Fullscreen；Collapsed不得poll。
- tests：classifier、summary、independent instances、dispose、view preservation與PTCS Playwright integration。

## 驗收

1. plain reply回`None`；static與runtime TA回正確presentation kind。
2. 四列TA summary含instrument/1m/range/四列及完整indicator period摘要。
3. Collapsed狀態無chart DOM、open frame、poll timer；Inline後才open並render。
4. 兩則TA reply可同時Inline，兩者channel/canvas identity不混線。
5. Inline -> Fullscreen -> Inline保留local view與data revision，沒有雙poll。
6. Collapse/unmount/disconnect後resource count回baseline；delta不增加chat/IndexedDB history。
7. desktop/mobile Playwright驗證session scrollbar、長Canvas、其他reply可見、fullscreen round-trip與zero console error。
8. Playwright移動cursor跨至少三個bars，驗證vertical cursor X位置、shared timestamp與每個row value readout同步變更；zoom後仍對齊同一time domain。
9. formal TA reply初始card只顯示摘要；DOM不得包含完整point-array/raw `Case/Fields` JSON。Collapsed時open/action/poll frame count為0。
10. PTCS切Plain/Form不影響Dynamic reply cards；Form renderer只在Form mode被呼叫，Plain維持單一host textarea。

## Review evidence

- 原始缺口：`G:\PulseTrade2.fs\misc\2026-07-13_Y軸不見了，共用的，X軸也不見了，跟隨游標移動的cross rows vertical cursor(隨著bar的移動呈現指到每 row 指標值)也不見了.png`。
- 修正版desktop/fullscreen/mobile證據位於`G:\PulseTrade2.fs\misc\2026-07-13_PTCS_TA_axes_*.png`；mock本體為`G:\PulseTrade2.fs\misc\2026-07-13_PTCS_ChatReplyCanvasPresentation_Mock.html`。
- review mock以CSS hover zones表達interaction；production renderer不得沿用離散zones，必須用實際time-scale hit-test取得bar identity並同步所有rows。
