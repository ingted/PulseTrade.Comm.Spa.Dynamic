# Issue hypothesis: main projection settlement reanchor

## Proven facts

- Daedilus 的真18884 0.2.48 rerun已證server接受main exact-pan：coverage revision `5 -> 6`、query generation `1 -> 2`、ActiveDetail start `50090 -> 50430`（`+340`）、GLOBAL resident start `1433048 -> 1443248`（`+10200 = 340 * 30`）。
- 同一settlement後Renderer local visible start卻由`1645 -> 1985`（再`+340`）；使用者語意是resident與visible一起平移，因此resident-relative offset應維持`1645`，pointer canonical event-time亦應維持。
- Consumer端原先的coverage-intent equality已修正並證server relocation GREEN；剩餘偏離發生在accepted projection進入Renderer後，不是transport reject或query generation。
- Evidence authority：Daedilus direct message `msg-fsi-cc0518aedf2c44a7b10bb73e05d1df0c`與`main-chart-drag-two-stage-setup/{main-commit-gate,result}.json`。

## Expected invariant and source of truth

- non-CURSOR main exact-pan以event-time visible range加coverage intent請server同時移resident與visible；accepted projection的ActiveDetail／resident位置是新authority。
- 若server按同一base-axis delta relocation，Renderer settlement不得把optimistic local delta再次套到新projection；應恢復pointerdown前resident-relative visible offset並維持pointer anchor。
- Resize、navigator、CURSOR、failed/rejected action、generation/revision gate與wire shape不變。

## Earliest divergence and candidate cause

- 最早偏離在accepted document/projection settlement：browser-local committed window仍保留optimistic `+340` local start，接著相對新resident timeline渲染，形成double-apply。
- 候選根因：pending main-pan context只保存target optimistic window，沒有保存pre-drag resident-relative offset／anchor，或settlement branch無法辨識同delta relocation並reanchor。
- Distinguishing RED：固定pre-window local start、main pan delta與accepted ActiveDetail同delta relocation；settlement結果須回pre-window local start，若projection未按預期relocate則沿既有fail-safe path。

## Experiment boundary

- 先定位main pan pending/settlement state與projection identity seam，新增一個pure regression，再做最小reanchor helper與單一accepted branch wiring。
- 不修改consumer、Action contract或server coverage validation。
- 若需要新增wire field、GLOBAL ordinal換算或跨axis比例推測，停止並回到RFC設計。

## Estimated implementation size

- Pure helper/context test：約20-40 non-type lines。
- Renderer state/wiring：約20-50 non-type lines；若source總增量超過90行，停止重估因果模型。
