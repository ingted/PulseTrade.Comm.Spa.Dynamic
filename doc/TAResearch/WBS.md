# WBS-PTCS-DYNAMIC-TA-0001 Transport-Neutral Realtime TA Canvas Runtime

Status: Accepted / DEV authorized
Date: 2026-07-11
RFC: `doc/RFC/RFC-PTCS-DYNAMIC-0007.realtime-ta-canvas-runtime.md`
Test: `doc/TAResearch/Test.md`
Test ID: `TEST-PTCS-DYNAMIC-TA-0001`

## 1. Progress

| WBS ID | Priority | Work item | Deliverable | Depends on | Tests | Progress | Status | Detail |
| --- | ---: | --- | --- | --- | --- | ---: | --- | --- |
| DYN-TA-000 | 0 | Accepted document chain | canonical docs and traceability | none | document/check | 100% | Done | this file |
| DYN-TA-00A | 1 | Legacy SDUI readiness closure | common DSL/direct Canvas/strict schema/invalid-node gates before new runtime | DYN-TA-000 | T-000A | 100% | Done | [@DYN-TA-00A](WBS.DYN-TA-00A.md) |
| DYN-TA-001 | 2 | Contracts package | runtime/frame/action types, codec, validation, limits | DYN-TA-00A + PTMD contracts vocabulary | T-001,T-002,T-011,T-015,T-018 | 100% | Done | [@DYN-TA-001](WBS.DYN-TA-001.md) |
| DYN-TA-002 | 3 | Pure reducer/runtime registry | ordering, effects, mount/dispose, poll state | DYN-TA-001 | T-003,T-004,T-008,T-009 | 100% | Done | [@DYN-TA-001](WBS.DYN-TA-001.md) |
| DYN-TA-003 | 4 | WebSharper TA renderer | rows, viewport, controls, status, bounded state | DYN-TA-001/002 | T-005..T-007,T-011,T-012,T-015 | 99% | Active | [@DYN-TA-003](WBS.DYN-TA-003.md) |
| DYN-TA-004 | 5 | PTCS compatibility adapter | isolated server adapter, authenticated callbacks, transient mapping, browser adapter | DYN-TA-001..003 + PTCS seam | T-007,T-010,T-013,T-016,T-017 | 99% | Active | [@DYN-TA-004](WBS.DYN-TA-004.md) |
| DYN-TA-005 | 6 | E2EQ adapter / parallel path | frame/action mapper and feature-gated shared renderer | DYN-TA-001..003 | T-013,T-014,T-019 | 55% | Active | [@DYN-TA-005](WBS.DYN-TA-005.md) |
| DYN-TA-006 | 7 | Cross-host E2E and bounded soak | PTCS/E2EQ Playwright matrix, 20-poll/resource evidence | DYN-TA-004/005 | T-010,T-014,T-019,T-020 | 86% | Active | [@DYN-TA-004](WBS.DYN-TA-004.md) |
| DYN-TA-007 | 8 | Static compatibility and DSL sync | static Canvas/FormInput regression and canonical DSL | DYN-TA-001..004 | T-001,T-016,T-017 | 40% | Active | [@DYN-TA-007](WBS.DYN-TA-007.md) |
| DYN-TA-008 | 0 | Package/release closure | 0.1.1 exact refs, NuGet, downstream bump, docs/runbook | DYN-TA-001..007 | T-001..T-020 | 88% | Active | [@DYN-TA-008](WBS.DYN-TA-008.md) |
| DYN-TA-009 | 1 | Composite row / multi-trace DSL | additive trace contracts、renderer、legacy projection、四列geometry | DYN-TA-001..004 | T-021 | 100% | Completed | [@DYN-TA-009](WBS.DYN-TA-009.md) |
| DYN-TA-010 | 2 | Browser delta wire v2 | keyed upsert/remove-before/status delta、client merge/resync | DYN-TA-009 | T-022 | 100% | Completed | [@DYN-TA-010](WBS.DYN-TA-010.md) |
| DYN-TA-011 | 1 | Mixed-reply TA presentation closure | production envelope decode、summary-only collapsed、lazy inline/fullscreen、Plain/Form boundary | DYN-TA-003/004/010 + PTCS WBS-069 | T-023..030 | 95% | External blocked | runtime/E2E完成，只剩public NuGet credential，詳見[@DYN-TA-011](WBS.DYN-TA-011.md) |
| DYN-TA-012 | 1 | Loaded-range viewport與pointer cross-row cursor修正 | requested/loaded/visible語意、canonical navigator、follow-latest、pointer hit-test、formal 82 gate | DYN-TA-003/011 | T-006,T-031..034 | 100% | Done | renderer/model與formal beta111/beta100/alpha60 terminal/browser/memory gate通過，詳見[@DYN-TA-012](WBS.DYN-TA-012.md) |
| DYN-TA-013 | 1 | 2000-point full bootstrap與commit-on-release navigator | full/delta cap分流、compact wire、draft/committed viewport、formal weak-device interaction gate | DYN-TA-004/012 | T-035..038 | 100% | Done | [@DYN-TA-013](WBS.DYN-TA-013.md) |
| DYN-TA-014 | 1 | Overview / typed Add Row / reset / copy | dual-handle overview、full-range compressed view、stable typed editor、canonical reset、reply copy action | DYN-TA-013 + PTCS WBS-071 | T-039..044 | 100% | Done | [@DYN-TA-014](WBS.DYN-TA-014.md) |
| DYN-TA-015 | 1 | Full runtime export / draft query / slot cursor | full-data download、Apply boundary、shared slot geometry、exact package/formal gate | DYN-TA-014 | T-045..050 | 100% | Done | [@DYN-TA-015](WBS.DYN-TA-015.md) |
| DYN-TA-016 | 1 | Editor shell / capability poll / Reset regression | stable editor DOM、capability-gated poll、multi-row reset | DYN-TA-015 | T-051..055 | 100% | Done | [@DYN-TA-016](WBS.DYN-TA-016.md) |
| DYN-TA-017 | 1 | Notebook TA Workspace production | generic source envelope、editor/action contract、shared temporal axis、production renderer與real DIB | DYN-TA-016 + owner contracts | T-056..070 | 100% | Done；M16真provider capacity/shared-axis/cursor gate完成 | [@DYN-TA-017](WBS.DYN-TA-017.md) |
| DYN-TA-018 | 0 | Browser range cache/resume | owner fingerprint、accepted projection cache、IndexedDB、resume/full handshake與真DIB range E2E | DYN-TA-017 + Daedalus fingerprint/action host | T-071..076 | 100% | Done；M17 OPEN_END finalized-prefix cache/history-to-live authoritative replacement完成 | [@DYN-TA-018](WBS.DYN-TA-018.md) |
| DYN-TA-019 | 0 | Live preview chart hot path | same-position close/TA原位更新、topology/data view分離、cursor不中斷 | DYN-TA-017/018 + owner live frame | T-077 | 95% | Active；generic exact-package gate完成，等待Daedalus真SPAA browser closure | [@DYN-TA-019](WBS.DYN-TA-019.md) |

九月`基於FSSTL的TA Research`第一個production切片`DYN-TA-017/018`已完成；舊`DYN-TA-008`package closure只在後續TA切片需要發布直接dependency時同步處理，不擴張到legacy consumer。

目前TA cache/renderer local candidate graph為Contracts `0.1.10`、Renderer `0.1.25`、Interactive.Client `0.1.21`、Dynamic.Ptcs `0.1.10`與Ptcs.Client `0.1.18`；五顆均已完成exact-package build/test與local library-packs staging，但尚未public push。上一個public graph Contracts `0.1.8`、Renderer `0.1.21`、Interactive.Client `0.1.17`、Dynamic.Ptcs `0.1.8`與Ptcs.Client `0.1.14`仍是目前公開版；DYN-TA-019待Daedalus以真SPAA sustained/geometry gate確認0.1.25後才發布/關閉。`artifacts/staging`歷史build副本不修改、不作active consumer。


## 2. Legacy prerequisite policy

`DYN-TA-00A`只關閉會讓新runtime建立在錯誤基礎上的既有缺口：

- `DYN-WBS-502/503/505` common Form/Canvas DSL與codec邊界。
- `DYN-WBS-506/512` direct DSL target browser path。
- `DYN-WBS-519` strict ActorsPage schema parsing；移除token `IndexOf`分類風險。

Public OAuth human confirmation、production RN service proof與ACL service redeploy留在原WBS，由對應Host/ops slice處理，不阻擋pure Contracts/Renderer。

## 3. UI milestone order

1. Static direct Canvas/invalid schema regression。
2. Runtime document + fixed chart workspace first viewport。
3. local viewport interactions with zero network effects。
4. remote query/Add Row actions with clear in-flight/error state。
5. transient polling/stale/resync/resource bounds。
6. E2EQ parity, then PTCS adapter, then package release。

每個milestone都需要F# Playwright test case與人類視角review，不以source marker或build取代。
