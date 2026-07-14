# @DYN-TA-013 2000-point Full Bootstrap / Commit-on-Release

Status: Done / 100%

| Slice | Deliverable | Tests | Progress | Status |
| --- | --- | --- | ---: | --- |
| DYN-TA-013A | RFC/REQ/SA/SD/WBS/Test alignment | T-035..038 | 100% | Done |
| DYN-TA-013B | full=2000 / delta=200 / first-data-full / compact wire | T-035 | 100% | Done：`ta-browser.v3` columnar full保留2000，stable delta仍<=200；empty-to-first-data為full。 |
| DYN-TA-013C | draft navigator state與release single commit | T-036 | 100% | Done：`input`只更新preview，`change`才commit chart；warm-up traces依timestamp對齊reference timeline。 |
| DYN-TA-013D | isolated F# Playwright real drag/head-tail/zero-network | T-037 | 100% | Done：real mouse drag期間render sequence不變，release恰增1，head/tail均可達且local navigation不送action。 |
| DYN-TA-013E | exact package release與formal 82 loaded>=2000 gate | T-038 | 100% | Done：Renderer alpha19、Ptcs alpha7-win39、Ptcs.Client alpha8-win47；正式82 gate載入2000、可見48。 |

## Boundary

不加入thumbnail、不一次render 2000 DOM、不以on-release server paging取代browser-local viewport。SQL actual coverage由PTMD/Host gate負責，Dynamic只誠實呈現reduced-state loaded count。

## Completion evidence

- isolated package suites：Renderer `17/17`、Ptcs.Client `8/8`、Dynamic.Ptcs `7/7`。
- formal artifact：`G:\PulseTrade.fs.Comm.Log\verification\ptcsHostTaResearchLive\run-ta2000-final-bounded-win39-alpha45-20260715022108`。
- formal route實際載入2000 points、DOM只畫48 candlesticks；drag/release、head/tail、delta cap、same-connection rejection recovery、reconnect與memory gate均通過。
