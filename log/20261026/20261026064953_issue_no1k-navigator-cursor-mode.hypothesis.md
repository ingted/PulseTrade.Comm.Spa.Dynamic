# DEBUG hypothesis: no-1K navigator authority / cursor mode

## Phenomenon

- Consumer：0.2.39，FSSTL僅30K/60K rows，GLOBAL resident initial `start=2995494,count=1491`。
- Gestures local geometry正確：left shrink `100/1391`，right shrink `100/1291`，body-left `50/1291`。
- Authority偏離：GLOBAL resident卻跳至2952345，selection X 1530.139→1495.136，發送4 actions；可見target包含`2952445/1291/gen1`與`2952395/1291/gen2`。
- 新契約：右鍵切換CURSOR。CURSOR顯示十字並以actual pointer delta等量反向pan；非CURSOR隱藏十字，pointerdown位於visible midpoint左／右時分別拖resident4500內visible left／right boundary。

## Earliest divergence candidates

1. Initial retained navigator authority在無1K/5K active row時組合GLOBAL 1K resident start與30K active ordinal/count，使projection、range text與dispatch單位不一致。
2. Existing pointer lifecycle沒有一個explicit interaction mode；body/edge hit-test或draft/terminal path同時送coverage，造成resize多送。
3. Authoritative frame替換後queued gesture沒有用同一mode／axis identity settlement，把preview當新intent。

## Discriminating experiments

- Pure model RED A：建立GLOBAL resident start/count + only-active 30K window，分別計算left/right resize/body pan；斷言target仍在同一GLOBAL ordinal domain且一gesture一intent。
- Pure model RED B：pointer delta為不同像素距離時，CURSOR pan delta必須等比改變且方向相反；boundary mode只改左或右邊，寬度／start符合clamp。
- Source wiring RED：錨contextmenu mode toggle、crosshair visibility、mode-specific pointerdown/dispatch、reactive replacement後mode與draft terminal cleanup。
- Browser RED：真DOM右鍵切模式，同距離drag驗geometry/action count，mouseleave/replacement後無stale cursor/draft。

## Pseudocode and size estimate before product edit

```text
type NavigatorInteractionMode = CursorPan | BoundaryResize

onContextMenu:
  prevent default
  toggle mode
  clear active draft/listeners
  render mode + cursor visibility

onPointerDown(latest projection, x):
  snapshot identity, authority, x, visible window
  if CursorPan then gesture = Pan(pointerStart=x)
  else gesture = Resize(side = x < visibleMidpoint ? Left : Right)

onPointerMove(x):
  deltaOrdinal = round((x - pointerStart) / trackWidth * authority.domainCount)
  CursorPan -> shift visible window by -deltaOrdinal
  Resize Left -> move start only; Resize Right -> move end only
  clamp within retained resident domain; render preview only

onPointerUp:
  derive one typed coverage intent from final preview + axis-bound authority
  dispatch at most once when material; clear draft/listeners

settle projection:
  accept/reanchor/reject only when identity + axis/domain authority matches
  never reinterpret active-scale ordinal as GLOBAL ordinal
```

- Product estimate：約80–140行非型別宣告（model helpers/reducer 30–50；DOM event/render 45–75；wiring 5–15）。Tests約70–120行。若product diff超過280行，在本檔追加`## Experiment`解釋為何無法以更小seam完成。

## Boundaries

- Owner：asteer = Dynamic Renderer/Interactive.Client；daedilus = Research backend、18884 process與real Playwright acceptance。
- 禁改：`lib-packs.txt`、`tests/PulseTrade.Comm.Spa.Dynamic.Ptcs.Tests/Program.fs`、`.pcsl/`、`temp/`與Research source。
- 不採用：delay、清cache、hard-coded 1K→30K ratio、強行停service、舊版package覆寫。
