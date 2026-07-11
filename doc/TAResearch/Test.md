# TEST-PTCS-DYNAMIC-TA-0001 Transport-Neutral Realtime TA Canvas Runtime

Status: Test design complete / Implementation not started
Date: 2026-07-11
REQ: `doc/TAResearch/REQ.md`
SD: `doc/TAResearch/SD.md`
WBS: `doc/TAResearch/WBS.md`

## 1. Test policy

- Contracts/reducer使用deterministic F# tests；UI milestone必須用Playwright實際操作。
- PTCS/E2EQ adapter parity需使用同一frame/action fixture，不接受兩套近似fixture。
- fake/internal renderer fixture只驗證pure component，不可取代真PTCS channel與真E2EQ adapter E2E。
- browser evidence需檢查geometry、visible values、interaction、network count、history/IndexedDB row count與console errors。

## 2. Matrix

| Test ID | Requirement | Level | Scenario / Expected | WBS | Status |
| --- | --- | --- | --- | --- | --- |
| DYN-TA-T-001 | REQ-001/002/018 | Contract | document/snapshot/patch/error/heartbeat strict roundtrip；static payload不誤進runtime | DYN-TA-001 | Designed |
| DYN-TA-T-002 | REQ-003/014/015 | Negative | unknown op/node/script/URL/DOM selector/oversized frame fail visibly，不執行 | DYN-TA-001 | Designed |
| DYN-TA-T-003 | REQ-010 | Reducer | duplicate no-op；gap/out-of-order/base mismatch要求resync且不改last-good data | DYN-TA-002 | Designed |
| DYN-TA-T-004 | REQ-006 | Reducer | ResetView只改local view；ResetCanvas送一次snapshot action並還原initial state | DYN-TA-002 | Designed |
| DYN-TA-T-005 | REQ-004/005 | Component | 所有TA row kinds、shared x-axis、separate y-scale、unknown kind controlled error | DYN-TA-003 | Designed |
| DYN-TA-T-006 | REQ-005 | Browser | zoom/pan/crosshair/toggle/row visibility不送network；值與可視K棒對齊 | DYN-TA-003 | Designed |
| DYN-TA-T-007 | REQ-007 | Browser | instrument/interval/range/Add Row各送恰好一次typed action，disabled/in-flight state合理 | DYN-TA-003/004 | Designed |
| DYN-TA-T-008 | REQ-008/009/010 | Lifecycle | visible/expanded/ready才poll；one-in-flight；timeout/backoff/reconnect/resync | DYN-TA-002/004 | Designed |
| DYN-TA-T-009 | REQ-009 | Lifecycle | hidden/collapse/unmount/disconnect取消timer/channel/subscription，disposed不再callback | DYN-TA-002 | Designed |
| DYN-TA-T-010 | REQ-011/012 | PTCS E2E | 500+ bars + 20 polls只更新revision，chat history/PCSL/IndexedDB message count不增加 | DYN-TA-004/006 | Designed |
| DYN-TA-T-011 | REQ-013 | Bounds | rows/bars/operations/items/bytes超限保留last-good Canvas並顯示limit reason | DYN-TA-001..003 | Designed |
| DYN-TA-T-012 | REQ-016 | Browser | Live -> Delayed -> Stale、Backfill、Unavailable與watermark/lag/quality正確呈現 | DYN-TA-003 | Designed |
| DYN-TA-T-013 | REQ-017 | Contract parity | PTCS與E2EQ adapter餵同frames，reducer final state完全相同 | DYN-TA-004/005 | Designed |
| DYN-TA-T-014 | REQ-017 | Browser parity | 兩host path主要chart/rows/toolbar geometry與interaction result等價 | DYN-TA-005/006 | Designed |
| DYN-TA-T-015 | REQ-015 | Static/source gate | 新runtime source無`JS.Inline`、手寫JS、string-built script/global callback | DYN-TA-001..003 | Designed |
| DYN-TA-T-016 | REQ-018 | Regression | existing static Canvas/FormInput/Argu/actors page與`CommHub.useDynamicSdui`相容 | DYN-TA-004/007 | Designed |
| DYN-TA-T-017 | REQ-014 | Extension behavior | absent走host fallback；present+invalid顯示controlled error，不silent fallback | DYN-TA-004 | Designed |
| DYN-TA-T-018 | REQ-008 | Dependency | Contracts無WebSharper/PTCS/fCell2/PTMD；Renderer無PTCS/fCell2/PTMD/SQL | DYN-TA-001 | Designed |
| DYN-TA-T-019 | REQ-017 | E2EQ AgentE2E | Historical/RT TA source/symbol/range、hover、tag/viewport/navigator regression通過 | DYN-TA-005 | Designed |
| DYN-TA-T-020 | REQ-013/016 | Soak | bounded working set長時間poll，不成長timer/channel/DOM series/history rows | DYN-TA-006 | Designed |

## 3. Playwright flows

### PTCS-hosted

1. 登入並開TA Canvas target。
2. initial history後操作zoom/pan/crosshair/toggle/Add Row/change range/reset。
3. 記錄before/after message row、IndexedDB message row、runtime revision與network action count。
4. 模擬stale、gap、disconnect/reconnect與close/reopen。

### E2EQ-hosted

1. 開Historical/Realtime TA shared renderer feature path。
2. 操作source/symbol/range、navigator、hover、toggle、resize。
3. 對照existing E2EQ AgentE2E domain invariants與geometry。
4. 同frame fixture與PTCS path比較state/visible rows/series bounds。

## 4. Evidence/verification

新增verifier前先更新repo的Verification文件，F# verifier負責codec/reducer/adapter parity；Playwright MCP負責UI操作與截圖/geometry。所有deterministic output以selector/summary保存，不dump完整frame/history。

## 5. Release gate

`DYN-TA-001..008`完成、T-001..020通過、PTCS companion seam與E2EQ parity均有真路徑證據後，才可宣稱runtime完成。只有pure renderer或fake fixture通過不構成交付。
