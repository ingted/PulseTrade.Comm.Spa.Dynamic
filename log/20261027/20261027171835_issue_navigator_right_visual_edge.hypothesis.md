# Issue hypothesis: navigator move misses the right visual edge

時間：2026-10-27 17:18:35 +08:00

## Phenomenon

0.3.5已讓inside-track左視覺邊緣提交local0並讓真正Earlier coverage通過；同一exact stack的右視覺邊緣action也有正確NavigatorWindow intent，但move anchor保留約1px inset，bar delta少3，final local4418而非maxStart4421。

## Repository state

- Repo：`C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic`
- Branch：`20260915_033.ptcs_group_support`
- HEAD/upstream：`c075bf47a3539fce54195093e7003441bc04c768`
- 外部dirty：`lib-packs.txt`、`tests/PulseTrade.Comm.Spa.Dynamic.Ptcs.Tests/Program.fs`、`.pcsl/**`、`temp/**`，本輪不納入／不改寫。

## Hypothesis and distinguishing evidence

1. Move draft只以pointer center位移換算raw bar delta，未在selection右視覺邊緣等於track右緣時snap maxStart。若成立，exact geometry的舊preview為4418，但加入half selection pixel width的right-edge condition後為4421。
2. 若實際release clientX與Daedilus target不同，則raw delta不是4418。現有stable attrs已證drag delta4418、preview4419-4497與action/final4418，故此候選優先且不再擴wire trace。

## Experiment

- 找到pointer→delta與selection width authority，抽pure selector回傳edge-snapped delta/draft。
- DYN-T-732鎖inside right visual edge=maxStart、ordinary interior unchanged、pointer overscroll仍Later、left local0不回歸。

## Estimated change

Source非型別宣告約10–20行，test約20–35行；若超過2x，先追加原因。

## Candidate result

- Exact geometry支持假設1；未新增wire trace。Source helper約20行、production wiring約20行，仍未超過2x門檻。
- DYN-T-732修前0.3.5 compile FS0039 RED；0.3.6修後focused1/1、Renderer122/122，並鎖interior unchanged、outside no-snap與Later boundary。
- Interactive20/20、package verifier與fresh exact-package BrowserDemo通過；owner candidate不能替代Daedilus right-edge與Later coverage真驗收。

## Owner final

- Source `e0575622b5efcf78619d37a68f91e353af417d6a`已push；final Renderer／Interactive packages與fresh BrowserDemo DLL provenance皆exact。
- Fresh final gates：Renderer122/122、Interactive20/20、package verifierPASS、BrowserDemo WS9002 x1／0 errors。
- Source hypothesis已收斂；只剩同一真consumer的right-edge local4421與Later coverage驗收，未GREEN不close。

## Consumer resolution

- 0.3.6 exact真序列已證right-edge local4421與Later active51445；左右edge、Earlier/Later、browser/action errors契約全綠。
- A/D/Tagging/FirstPaint/No1K/formal stress相鄰回歸全綠，本issue resolved。既知2018 return tail為獨立consumer pending，沒有改動本因果或close boundary。
