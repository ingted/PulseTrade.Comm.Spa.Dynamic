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
11. `doc/RFC-PTCS-DYNAMIC-0004.actor-dynamic-action-modes.md`：Actor Dynamic / Actor Argu action mode split。
12. `doc/RFC-PTCS-DYNAMIC-0005.actors-page-renderer.md`：ActorsPage page-level renderer contract。

## RFC Map

| RFC | Status | Purpose |
| --- | --- | --- |
| `RFC-PTCS-DYNAMIC-0001` | Proposed / first implementation exists | Adopt PTCS `RFC-PTC-SPA-0006` dynamic extension points: manifest, script asset, custom shape, message renderer。 |
| `RFC-PTCS-DYNAMIC-0002` | Draft / Review | Dynamic-owned Argu metadata, SDUI form renderer, SubmitArguForm, add-key renderer, and cross-project PTCS/PTC RN integration schedule。 |
| `RFC-PTCS-DYNAMIC-0003` | Draft / Review | Correct product direction to common SDUI DSL and arg-string-driven backend FormInput resolution: Canvas/FormInput share document model; Argu/DU is parser-backed adapter; PTCS.Host owns demo DU。 |
| `RFC-PTCS-DYNAMIC-0004` | Accepted / In development | Actor Dynamic direct actor key / DU target / proxy key mode split; Actor Argu FormInput-only; canvas renderer remains payload-based。 |
| `RFC-PTCS-DYNAMIC-0005` | Proposed / first implementation slice | ActorsPage page-level renderer for PTCS `/actors`; separate from generic Canvas message renderer。 |
| `RFC-PTCS-DYNAMIC-0013` | Accepted / DEV authorized | Production Notebook TA workspace：generic source ordering envelope、schema/editor/action contract、owner adapter boundary與real DIB/Playwright acceptance。 |

## Cross-Project References

| Project | File | Relevance |
| --- | --- | --- |
| PTCS | `G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\doc\RFC-PTC-SPA-0007.dynamic-argu-form-extensions.md` | PTCS core seam for append input renderer and add-key dialog renderer。 |
| PTCS | `G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\doc\RFC-PTC-SPA-0008.unified-sdui-target-extension-contract.md` | PTCS companion contract for direct DSL target and Dynamic-owned target key binding. PTCS stores ordered key list; PTCS.Dynamic interprets `[actor; template; canonicalArgString]` when extension is present。 |
| PTCS | `G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\doc\RFC-PTC-SPA-0009.actor-dynamic-action-modes-and-full-address-tree.md` | PTCS core action shell contract for Add actor key / Add target key / Add proxy key and full actor address tree display。 |
| PTCS | `G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\doc\RFC-PTC-SPA-0015.explicit-actor-argu-target-key.md` | PTCS companion RFC for ActorArgu explicit `[proxy; "target-v1"; target; template; raw]` key and `ActorArguTargetCommand.TargetActorAddress` projection。 |
| Dynamic | `doc/RFC-PTCS-DYNAMIC-0006.explicit-actor-argu-target-key.md` | Dynamic package RFC replacing beta64 `actor-argu-proxy` hidden rewrite with explicit Add Target Key fields。 |
| PTCS | `G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\doc\RFC-PTC-SPA-0010.actors-page-dynamic-dsl-rendering.md` | PTCS companion contract for `/actors` page-level ActorsPage rendering and fallback mutual exclusion。 |
| PTCS | `G:\PulseTrade2.fs\Libs\PulseTrade.Comm.Spa\doc\RFC-PTC-SPA-0013.acl-login-open-extension-boundary.md` | PTCS companion contract for final ACL/Login open extension boundary and `poc.full.nuget.journal.ACL2.fsx` gate。 |
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
| `DYN-T-511` | `DYN-WBS-511` | Package Expecto verifies `DynamicArguResolveEndpoint` accepts `[actorAddress; duTypeOrTemplateKey; canonicalArgString]`, resolves through registered Argu parser, returns backend FormInput DSL with alias/default projection, includes the `DataRange` tail section, preserves exact canonical enum default option values, and reconstructs the full PFCF raw command exactly through the document-backed full-form path。 |
| `DYN-T-512` | `DYN-WBS-512` | Browser E2E verifier `G:\PulseTrade.fs\Libs\PulseTrade.Comm\scripts\verify-ptcs-host-dynamic-argu-live.fsx` starts PTCS.Host loopback, loads the Dynamic extension DLL, resolves `[actorAddress; duTypeOrTemplateKey; canonicalArgString]` through backend FormInput DSL, verifies visible alias/default rendering, submits the exact raw PFCF data-range command, and checks DurableProxy echo readback。Manual add-target-key dialog and public deployed 81/443 gates remain separate verification items。 |
| `DYN-T-517` | `DYN-WBS-517` / PTCS `WBS-054` | Canvas `Tree` renderer for PTC ActorTreeDocument: package test covers DSL decode/controlled failures, and PTCS browser E2E must prove Dynamic Canvas Tree and no-Dynamic fallback table consume the same `ActorTreeDocument` projection。 |
| `DYN-T-520` | `DYN-WBS-518` | Mode dispatch for `actor-dynamic-target`, `actor-dynamic-proxy`, and `actor-argu-target`。 |
| `DYN-T-521` | `DYN-WBS-518` | Actor Dynamic direct actor key can send/echo `canvas_demo.json` and render canvas; non-canvas reply falls back to normal。 |
| `DYN-T-522` | `DYN-WBS-518` | Actor Dynamic DU/FormInput target still resolves `[actor; template; canonicalArgString]` through backend parser。 |
| `DYN-T-523` | `DYN-WBS-518` | Actor Dynamic proxy key builder stores `[proxyActorAddress; "proxy-v1"; rnActorAddress; targetKind]`。 |
| `DYN-T-524` | `DYN-WBS-518` | Actor Argu exposes no proxy key and no canvas behavior。 |
| `DYN-T-525` | `DYN-WBS-518` | Canvas renderer only claims payloads with `schema=fskynet-sdui`。 |
| `DYN-T-526` | `DYN-WBS-519` | Package Expecto verifies ActorsPage classifier accepts `ActorTopologyPage` payload；full WebSharper build is covered by DYN-VFY-001。 |
| `DYN-T-527` | `DYN-WBS-519` | Package Expecto verifies normal Canvas payload is not claimed by ActorsPage classifier。 |
| `DYN-T-528..532` | `DYN-WBS-519` | Implemented: host/port grouping, role ordering, full address tree/grid/cards/actions, PTCS `/actors` Playwright accepted path, unsupported fallback path, and browser-local report schedule start/stop. Remaining: strict parser, persisted/server-side report schedule, restart/cache sync, cross-service GW/RN registry feed, and failover visual states。 |
| `DYN-VFY-009` | `DYN-WBS-521` | Demo, production-SQL no-wait, browser Playwright, and formal service redeploy slices passed on 2026-07-02 with `src\poc.full.nuget.journal.ACL2.fsx` plus PTC Host verification; latest package-startup and browser slices use PTCS beta71 + Dynamic beta61 + `PulseTrade.Comm.Spa.ACL 0.1.0-alpha11` + `PulseTrade.Comm.Spa.Login 0.1.0-alpha13`, verifying the NoGithubOAuth local-login host and PTCS Playwright gate can run on the FAkka.WebSocket win12 stack-safe loop. Latest formal service evidence remains beta70/beta60/alpha10/alpha12 until redeploy. Remaining final gate is fallback cleanup and service redeploy on the beta71 package set。 |
| `DYN-VFY-009A/B` | `DYN-WBS-521` | `src\full.nuget.journal.ACL2.NoLogin.fsx` GitHub-only variant keeps PTCS.Login disabled. 009A verifies NoLogin health/static/durable probe/PingPong/Echo reuse; 009B adds `PFCF_AKKA_CMD_FOR_ProtoTyping` under `pfcf-akka-cmd-prototyping` and verifies canonical PFCF arg-string parser/build/resolve/default projection with `datarange` tail ordering。 |
| `DYN-VFY-009C` | `DYN-WBS-521` | `src\full.nuget.journal.ACL2.NoGithubOAuth.fsx` is the local-login-only ACL2 variant. It starts only the PTCS.Login listener, defaults fixed mode to 82, keeps PTCS.ACL/PTCS.Login/Dynamic/PFCF prototype active, verifies local sys-admin/Terry login and ACL matrix, and avoids GitHub OAuth client id/secret and GitHub OAuth host startup。 |
| `DYN-TA-T-056..060` | `DYN-TA-017A/B` | `tests/PulseTrade.Comm.Spa.Dynamic.Contracts.Tests.fsproj` verifies document ownership、source envelope codec/validation/reducer、last-good preservation與dependency boundary。 |
| `DYN-TA-T-061` | `DYN-TA-017C` | Contracts suite驗generic editor、stable row identity、correlated lifecycle及`ptcs-dynamic-action.v1` request/result codec；Interactive.Client alpha2提供single-pending timeout/disconnect/mismatch fail-closed。 |
| `DYN-TA-T-062` | `DYN-TA-017D` | Contracts/Renderer/PTCS/Ptcs.Client package suites及BrowserDemo Playwright MCP驗temporal multi-scale projection、generic editor、pending/reject與desktop/mobile；real owner evidence不由synthetic gate取代。 |
| `DYN-TA-T-063..064` | `DYN-TA-017E..F` | ColdFar Notebook `dotnet dib` fixture及Playwright MCP real-path matrix；Contracts alpha12 / Renderer alpha33 / Interactive.Client alpha4以exact `FSharp.Core [10.1.302]`解除Daedalus restore blocker，SessionHost、MDCQ provider identity/cursor與resource lifecycle仍是production gate。 |
