# WBS-PTCS-DYNAMIC-TA-0001 Transport-Neutral Realtime TA Canvas Runtime

Status: Document chain complete / Development not started
Date: 2026-07-11
RFC: `doc/RFC/RFC-PTCS-DYNAMIC-0007.realtime-ta-canvas-runtime.md`
Test: `doc/TAResearch/Test.md`
Test ID: `TEST-PTCS-DYNAMIC-TA-0001`

## 1. Progress

| WBS ID | Work item | Deliverable | Depends on | Tests | Progress | Status |
| --- | --- | --- | --- | --- | ---: | --- |
| DYN-TA-000 | RFC/REQ/SA/SD/Test/WBS chain | reviewable canonical docs | none | links/encoding/check | 100% | Review |
| DYN-TA-001 | Contracts package | runtime/frame/action types, codec, validation, limits | DYN-TA-000 | T-001,T-002,T-011,T-015,T-018 | 0% | Planned |
| DYN-TA-002 | Pure reducer/runtime registry | revision ordering, effects, mount/dispose, poll state | DYN-TA-001 | T-003,T-004,T-008,T-009 | 0% | Planned |
| DYN-TA-003 | WebSharper TA renderer | TA rows, viewport, controls, status, bounded state | DYN-TA-001/002 | T-005..T-007,T-011,T-012,T-015 | 0% | Planned |
| DYN-TA-004 | PTCS compatibility adapter | facade delegation, authenticated callbacks, transient mapping | DYN-TA-001..003 + PTCS seam | T-007,T-010,T-013,T-016,T-017 | 0% | Planned |
| DYN-TA-005 | E2EQ adapter / parallel path | E2EQ frame/action mapper and feature-gated shared renderer | DYN-TA-001..003 | T-013,T-014,T-019 | 0% | Planned |
| DYN-TA-006 | Cross-host E2E and soak | PTCS/E2EQ Playwright matrix, 20-poll/long-run bounded evidence | DYN-TA-004/005 | T-010,T-014,T-019,T-020 | 0% | Planned |
| DYN-TA-007 | Static compatibility and DSL sync | static Canvas/FormInput regression, canonical DSL update | DYN-TA-001..004 | T-001,T-016,T-017 | 0% | Planned |
| DYN-TA-008 | Package/release closure | exact refs, NuGet push, downstream bump, docs/runbook | DYN-TA-001..007 | T-001..020 | 0% | Planned |

## 2. Dependency schedule

```text
Contracts
  -> reducer/runtime registry
  -> TA renderer
       -> PTCS adapter (requires PTCS companion seam)
       -> E2EQ adapter (can proceed independently)
  -> cross-host parity/soak
  -> compatibility + DSL + release
```

PTCS seam暫時blocked時，Contracts/Reducer/Renderer/E2EQ adapter仍可推進；但不得把E2EQ-only成功誤標為PTCS production E2E完成。

## 3. Review gates before DEV

1. 接受Contracts + Renderer + PTCS facade三層package。
2. 接受E2EQ只referenceContracts/Renderer，由adapter重用同一renderer。
3. 接受RuntimeFrame、typed operations、5秒poll與bounded limits。
4. 接受PTCS core companion seam為PTCS path必要dependency，不以HTTP/history workaround代替。
5. SD/Test與E2EQ migration parity gate review完成。

未accepted前不進package split、NuGet push、E2EQ renderer replacement或PTCS core modification。
