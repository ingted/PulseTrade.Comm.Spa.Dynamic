# REQ-PTCS-DYNAMIC-TA-0002 Chat Reply Canvas Presentation

- Status：Draft / RFC review
- Date：2026-07-13
- RFC：`doc/RFC/RFC-PTCS-DYNAMIC-0008.chat-reply-canvas-presentation.md`
- PTCS dependency：`G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\doc\RFC\RFC-PTC-SPA-0020.chat-reply-canvas-presentation.md`

## Functional requirements

| ID | Requirement |
| --- | --- |
| DYN-CHAT-REQ-001 | Dynamic必須strict區分plain、static `fskynet-sdui`與runtime TA payload；plain回`None`。 |
| DYN-CHAT-REQ-002 | classification只依validated reply payload/schema/version，不得依target key、actor address、page title或alias猜測。 |
| DYN-CHAT-REQ-003 | TA presentation預設輸出compact summary，不立即mount chart或open/poll transient channel。 |
| DYN-CHAT-REQ-004 | TA summary至少包含instrument、interval/scale、requested range、actual coverage、ordered rows/traces、indicator parameters、freshness與quality。 |
| DYN-CHAT-REQ-005 | Inline mount必須render於PTCS提供的message-body host，且不修改chat/page ancestor overflow。 |
| DYN-CHAT-REQ-006 | Fullscreen必須沿用PTCS提供的fullscreen host並保留既有static Canvas能力。 |
| DYN-CHAT-REQ-007 | Inline與Fullscreen必須共享同一logical `canvasInstanceId`與runtime state，不得雙channel或雙poll。 |
| DYN-CHAT-REQ-008 | 多個reply presentation依reply identity隔離；相同payload也不得共用expand/view lifecycle。 |
| DYN-CHAT-REQ-009 | Collapsed、unmount、reply removal與disconnect必須取消timer、in-flight、channel與subscription。 |
| DYN-CHAT-REQ-010 | malformed declared SDUI/runtime payload顯示bounded controlled error；不得silent當成功Canvas或破壞其他reply。 |
| DYN-CHAT-REQ-011 | Static SDUI保持legacy summary/fullscreen相容；v2可增加inline，但不得要求TA transient channel。 |
| DYN-CHAT-REQ-012 | presentation不得寫chat history、PCSL business stream或IndexedDB message row；snapshot/delta只更新mounted instance。 |
| DYN-CHAT-REQ-013 | v2 presentation與被遷移的legacy fullscreen path只能使用typed F#與WebSharper API；不得使用`JS.Inline`、手寫JavaScript或string-built script。 |
| DYN-CHAT-REQ-014 | reducer/unit/Playwright需涵蓋classification、summary、lazy mount、兩則獨立展開、fullscreen round-trip、session scroll及cleanup。 |
| DYN-CHAT-REQ-015 | 每個TA row必須提供獨立Y axis domain、ticks、labels與unit；不同量綱不得共用或省略尺度。 |
| DYN-CHAT-REQ-016 | 所有TA rows必須共享同一time viewport，並只在chart stack底部render一組shared X axis；所有row points須按同一timestamp/bar identity對齊。 |
| DYN-CHAT-REQ-017 | pointer移動必須顯示跨所有rows的shared vertical cursor，並同步顯示該timestamp下每個row/trace的indicator values；zoom/pan後hit-test仍須對齊。 |

## Four-row summary example

```text
BTCUSDT · 1m · 600 bars · 2026-07-12 12:49..13:36 · Stale
Price: K, SMA 13/21/34/89/144/233
DMI: +DI 7, -DI 7, ADX 7/21
MACD Long: 34/89/144
MACD Short: 13/21/7
```

顯示文字可依locale調整，但資訊不得退化成只有Canvas title或row count。

## UX acceptance

1. summary card可用一個明確expand control展開Inline；Full screen action在Inline header可見。
2. Inline root完整跟隨message card寬度，Canvas高度由rows決定，外層session負責上下scroll。
3. 展開/收合其中一則reply時，其他reply DOM、view state與scroll anchor不被重置。
4. fullscreen close後回到原inline位置與view，不建立另一則reply或另一個target。
5. Y axes、shared X axis與cross-row cursor在inline及fullscreen都存在；cursor readout可掃描同一bar的price、DMI/ADX與MACD values。
