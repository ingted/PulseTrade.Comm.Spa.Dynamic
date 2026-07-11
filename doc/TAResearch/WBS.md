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
| DYN-TA-003 | 4 | WebSharper TA renderer | rows, viewport, controls, status, bounded state | DYN-TA-001/002 | T-005..T-007,T-011,T-012,T-015 | 0% | Planned | [@DYN-TA-003](WBS.DYN-TA-003.md) |
| DYN-TA-004 | 5 | PTCS compatibility adapter | facade delegation, authenticated callbacks, transient mapping | DYN-TA-001..003 + PTCS seam | T-007,T-010,T-013,T-016,T-017 | 0% | Planned | [@DYN-TA-004](WBS.DYN-TA-004.md) |
| DYN-TA-005 | 6 | E2EQ adapter / parallel path | frame/action mapper and feature-gated shared renderer | DYN-TA-001..003 | T-013,T-014,T-019 | 0% | Planned | [@DYN-TA-004](WBS.DYN-TA-004.md) |
| DYN-TA-006 | 7 | Cross-host E2E and bounded soak | PTCS/E2EQ Playwright matrix, 20-poll/resource evidence | DYN-TA-004/005 | T-010,T-014,T-019,T-020 | 0% | Planned | [@DYN-TA-004](WBS.DYN-TA-004.md) |
| DYN-TA-007 | 8 | Static compatibility and DSL sync | static Canvas/FormInput regression and canonical DSL | DYN-TA-001..004 | T-001,T-016,T-017 | 0% | Planned | [@DYN-TA-004](WBS.DYN-TA-004.md) |
| DYN-TA-008 | 9 | Package/release closure | exact refs, NuGet, downstream bump, docs/runbook | DYN-TA-001..007 | T-001..T-020 | 0% | Planned | [@DYN-TA-004](WBS.DYN-TA-004.md) |

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
