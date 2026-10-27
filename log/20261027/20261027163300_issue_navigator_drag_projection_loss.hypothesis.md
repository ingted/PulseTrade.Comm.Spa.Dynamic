# Issue hypothesis: navigator drag loses a Some projection before action construction

時間：2026-10-27 16:33:00 +08:00

## Phenomenon

0.3.4 runtime trace證long pointerdown讀到seq10的`projection=Some`，state/prepared的coverage revision/generation/active range一致；但同一drag最後`ui:4`仍無CoverageIntent。最早偏離已不在frame publication，而在pointerdown後的drag lifecycle。

## Repository state

- Repo：`C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic`
- Branch：`20260915_033.ptcs_group_support`
- HEAD/upstream：`9c77d325f8397c2f98a410ac058fc970f7bae790`
- 外部dirty：`lib-packs.txt`、`tests/PulseTrade.Comm.Spa.Dynamic.Ptcs.Tests/Program.fs`、`.pcsl/**`、`temp/**`，本輪不納入／不改寫。

## Hypotheses and distinguishing evidence

1. `startNavigatorDrag` receives `Some projection` but stores only local window/pointer geometry; finish therefore cannot build global CoverageIntent. Evidence: active drag type/closure lacks projection or derived authority while start parameter is otherwise unused.
2. Start stores projection, but `finishNavigatorDragFromElement` resolves a different legacy/cached drag path. Evidence: move/preview reads one state while finish/action builder reads another mutable/closure whose projection is absent.

## Experiment

- Trace production call graph and exact variable use from `startNavigatorDrag` through move and finish; do not infer from names.
- Add one RED gate that executes or contract-locks the production lifecycle with `Some projection` and proves finish currently yields bare action.
- Fix only the earliest authority break, then verify adjacent local-only drag and resident/global paths remain semantically distinct.

## Estimated change

Root fix預計不含type declaration約10–25行，測試約20–40行；若超過2x，先在本檔追加原因。

## Correction / Experiment 2 — physical track edge classification

- 0.3.4 telemetry反證原假設：pointerdown與finish共用同一`loadedCoverageSnapshot=Some` closure，projection沒有在drag lifecycle遺失。
- 真18884重現：中段drag送出`NavigatorWindow` intent；第二次由local `1198/count80`拖到視覺左邊界時，CSS pixel換算得`rawDelta=-1200`、requested start `-2`，preview已正確clamp為`0/count80`，但`navigatorBoundaryDirection`先把2根量化誤差判成Earlier，sync outcome=`coverage-boundary`。約34ms後才送bare VisibleRangeChanged，server依main-plot語意只前移80根，final local1118而非0。
- Source of truth修正：有`GlobalOverview`的resident track只有pointer真越出實體track才代表outside-resident request；pointer仍在`0..trackWidth`內時必須提交clamped local window，並由既有event-time path附`Purpose=NavigatorWindow`。跨軸GLOBAL ordinal仍禁止推算。
- 最小修正：抽出pure boundary selector，`GlobalOverview=Some`時沿既有`residentNavigatorBoundaryDirection`的physical-edge gate，其他local-only navigator維持logical boundary；production finish只改此分類點。預估source 8–15行、test 12–25行，仍在原上限內。

## Candidate result

- Source實作8個非型別宣告行左右，test 19行，未超過2x門檻。
- DYN-T-731修前以0.3.4 package-backed compile得到FS0039 RED；0.3.5修後focused1/1、Renderer121/121，並保留local-only Earlier與physical overscroll Earlier相鄰契約。
- Interactive.Client20/20、package verifier及fresh exact-package BrowserDemo均通過；這些只完成owner candidate，不替代Daedilus真Gate B。

## Owner final

- Source `3e039739f9c0cc704062a7497c31d1a6487a7cc4`已push；final Renderer／Interactive.Client packages及兩DLL provenance皆exact該commit。
- Fresh isolated final gates：Renderer121/121、Interactive20/20、package verifierPASS、BrowserDemo WS9002 x1／0 errors。
- 假設已由Daedilus finish telemetry與DYN-T-731收斂；剩餘驗收是同一真consumer sequence是否送出NavigatorWindow intent並settle local0，不再擴source hypothesis。
