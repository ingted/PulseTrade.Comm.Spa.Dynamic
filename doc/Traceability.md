# PulseTrade.Comm.Spa.Dynamic Traceability

本文件索引 PTCS.Dynamic 的 current-state 文件、RFC 與測試入口。正式 RFC 與 WBS 更新時同步本文件。

## Reading Order

1. `README.md`：package purpose and WebSharper bundle packaging note。
2. `doc/REQ.md`：current requirements。
3. `doc/SA.md`：architecture and package boundary。
4. `doc/SD.md`：implementation design。
5. `doc/WBS.md`：work breakdown and cross-project dependency order。
6. `doc/TEST.md`：verification gates。
7. `doc/RFC-PTCS-DYNAMIC-0001.adopt-ptcs-dynamic-extension-points.md`：adoption of PTCS dynamic extension points。
8. `doc/RFC-PTCS-DYNAMIC-0002.dynamic-argu-form-runtime.md`：Dynamic Argu Form formal RFC。
9. `doc/RFC-PTCS-DYNAMIC-0003.unified-sdui-form-dsl-roadmap.md`：Unified SDUI / Form DSL roadmap and product boundary correction。
10. `doc/DevLog.md`：append-only milestone log。

## RFC Map

| RFC | Status | Purpose |
| --- | --- | --- |
| `RFC-PTCS-DYNAMIC-0001` | Proposed / first implementation exists | Adopt PTCS `RFC-PTC-SPA-0006` dynamic extension points: manifest, script asset, custom shape, message renderer。 |
| `RFC-PTCS-DYNAMIC-0002` | Draft / Review | Dynamic-owned Argu metadata, SDUI form renderer, SubmitArguForm, add-key renderer, and cross-project PTCS/PTC RN integration schedule。 |
| `RFC-PTCS-DYNAMIC-0003` | Draft / Review | Correct product direction to common SDUI DSL and arg-string-driven backend FormInput resolution: Canvas/FormInput share document model; Argu/DU is parser-backed adapter; PTCS.Host owns demo DU。 |

## Cross-Project References

| Project | File | Relevance |
| --- | --- | --- |
| PTCS | `G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\doc\RFC-PTC-SPA-0007.dynamic-argu-form-extensions.md` | PTCS core seam for append input renderer and add-key dialog renderer。 |
| PTCS | `G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\doc\RFC-PTC-SPA-0008.unified-sdui-target-extension-contract.md` | PTCS companion contract for direct DSL target and Dynamic-owned target key binding. PTCS stores ordered key list; PTCS.Dynamic interprets `[actor; template; canonicalArgString]` when extension is present。 |
| PTC RN | `G:\PulseTrade.fs\Libs\PulseTrade.Comm\doc\RFC-PTC-0016.resource-node-sharded-function-proxy.md` | RN DurableProxy consumes `ActorArguTargetCommand.RawArgu` and adapts to legacy actor/service。 |
| PTC WBS | `G:\PulseTrade.fs\Libs\PulseTrade.Comm\doc\WBS.md` rows `PTC3-063`..`PTC3-067` | RN/RN.Host production gates and final Dynamic -> RN E2E。 |
| PTCS.Host demo DU | `C:\Users\Administrator\test_gemini\PulseTrade.Comm.Spa.Dynamic\doc\example DU.txt` | Big5/cp950 source material for host-local `PFCF_AKKA_CMD` demo subset；not package API。 |

## Verification Map

| Test ID | WBS | Expected verifier |
| --- | --- | --- |
| `DYN-T-401` | `DYN-WBS-401` | Document chain review / sensitive scan / encoding scan。 |
| `DYN-T-402` | `DYN-WBS-402` | `tests/PulseTrade.Comm.Spa.Dynamic.Tests` metadata/schema tests。 |
| `DYN-T-403` | `DYN-WBS-403` | `tests/PulseTrade.Comm.Spa.Dynamic.Tests` SubmitArguForm codec tests。 |
| `DYN-T-404` | `DYN-WBS-404` | PTCS `Scripts/verify.dynamicArguFormDurableProxy.playwright.fsx` append input renderer browser path、renderer fallback、invalid blank submit isolation、built-in textarea regression 與 geometry gate。 |
| `DYN-T-405` | `DYN-WBS-405` | PTCS `Scripts/verify.dynamicArguFormDurableProxy.playwright.fsx` add-key/readback path、built-in add-key fallback 與 duplicate target key idempotency gate。 |
| `DYN-T-406` | `DYN-WBS-406` | PTCS `Scripts/verify.dynamicArguFormDurableProxy.playwright.fsx` covers `--pcsl-root` run-scoped fresh root, `actor-dynamic` Playwright page creation, Dynamic add-key target, common Argu controls, raw Argu send, RN DurableProxy fCell2 forwarding, legacy echo reply, and PTCS full target-key readback；production split-service proof still external/open。 |
| `DYN-T-501` | `DYN-WBS-501/502` | Package Expecto verifies `SduiFormDocument` / node / action / binding model with PFCF_AKKA_CMD FormInput document。 |
| `DYN-T-502` | `DYN-WBS-503` | Package Expecto verifies Argu-to-FormDsl adapter and server/frontend raw arg codec agreement for multiple PFCF_AKKA_CMD union cases。 |
| `DYN-T-503` | `DYN-WBS-504` | First-slice regression verifies `DynamicTargetKey.tryResolve` for `[actor; formDslId]` and legacy `[actor; duType; cases...]`, including controlled failures。Superseded by DYN-T-507..511 for new canonical DU/template arg-string target。 |
| `DYN-T-504` | `DYN-WBS-505` | Backend-linked option provider tests using registered provider only。 |
| `DYN-T-505` | `DYN-WBS-506` | Browser E2E for actor key bound to direct DSL target。 |
| `DYN-T-506` | `DYN-WBS-506` | Browser E2E for actor key bound to DU/template + canonical arg string using PTCS.Host demo DU。 |
| `DYN-T-507` | `DYN-WBS-507/508` | Backend resolver verifies `[ actorAddress; duTypeOrTemplateKey; canonicalArgString ]` target resolution and controlled parse failure。 |
| `DYN-T-508` | `DYN-WBS-509` | Alias binding verifies case/field/option aliases enter DSL labels but not raw command semantics。 |
| `DYN-T-509` | `DYN-WBS-508` | Parser-backed Form DSL defaults verify rendered section order and `SduiFormNode.DefaultValues` come from registered Argu parse result and token scan。 |
| `DYN-T-510` | `DYN-WBS-510` | Subcommand raw builder verifies `ParseResults<PFCF_AKKA_CMD_DATA_RANGE>` rebuilds exact PFCF command with `datarange` tail ordering。 |
| `DYN-T-511` | `DYN-WBS-511` | Browser E2E verifies add target key -> backend resolved FormInput DSL -> alias/default rendering -> submit exact raw command -> RN/echo reply。 |
